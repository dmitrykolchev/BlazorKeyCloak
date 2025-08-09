using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Sam.Security;

public class CustomOpenIdConnectEvents : OpenIdConnectEvents
{
    public override Task UserInformationReceived(UserInformationReceivedContext context)
    {
        string jsonText = JsonSerializer.Serialize(context.User.RootElement);
        Console.WriteLine("UserInfo response: " + jsonText);
        return base.UserInformationReceived(context);
    }

    public override async Task TokenValidated(TokenValidatedContext context)
    {
        await base.TokenValidated(context);
        if (context.ProtocolMessage.IdToken != null)
        {
            // Add the id_token as a claim to the user's identity
            ((ClaimsIdentity?)context?.Principal?.Identity)?.AddClaim(new Claim("id_token", context.ProtocolMessage.IdToken));
        }
    }

    public override async Task RedirectToIdentityProviderForSignOut(RedirectContext context)
    {
        await base.RedirectToIdentityProviderForSignOut(context);
        var idTokenHint = context.HttpContext.User.FindFirst("id_token")?.Value;
        if (!string.IsNullOrEmpty(idTokenHint))
        {
            context.ProtocolMessage.IdTokenHint = idTokenHint;
        }
    }

    public override Task RedirectToIdentityProvider(RedirectContext context)
    {
        if (context.Options.UsePkce
            && context.Options.ResponseType != OpenIdConnectResponseType.Code
            && context.Options.ResponseType.Contains(OpenIdConnectResponseType.Code)
            && !context.Properties.Items.ContainsKey(OAuthConstants.CodeVerifierKey))
        {
            byte[] bytes = new byte[32];
            RandomNumberGenerator.Fill(bytes);
            string codeVerifier = Microsoft.AspNetCore.Authentication.Base64UrlTextEncoder.Encode(bytes);

            // Store this for use during the code redemption. See RunAuthorizationCodeReceivedEventAsync.
            context.Properties.Items.Add(OAuthConstants.CodeVerifierKey, codeVerifier);

            byte[] challengeBytes = SHA256.HashData(Encoding.UTF8.GetBytes(codeVerifier));
            string codeChallenge = WebEncoders.Base64UrlEncode(challengeBytes);

            context.ProtocolMessage.Parameters.Add(OAuthConstants.CodeChallengeKey, codeChallenge);
            context.ProtocolMessage.Parameters.Add(OAuthConstants.CodeChallengeMethodKey, OAuthConstants.CodeChallengeMethodS256);
        }

        context.ProtocolMessage.UiLocales = "ru-RU";
        context.ProtocolMessage.LoginHint = "";
        return Task.CompletedTask;
    }

    public override Task RemoteFailure(RemoteFailureContext context)
    {
        // TODO: нужно переводить на страницу с диагностикой (сообщение о том, что на самом деле произошло)
        // пока редирект на корневую страницу приложения
        context.HttpContext.Response.Redirect("/");
        context.HandleResponse();
        return Task.CompletedTask;
    }
}
