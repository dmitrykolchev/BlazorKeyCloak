### Secure Blazor Web App with Keycloak

This guide outlines the steps to build a Blazor Web App (.NET 10) with both Server and WebAssembly render modes, integrating Keycloak for authentication and role-based authorization to protect pages, database access, and a Web API.

#### 1. Keycloak Configuration

**1.1. Create a Realm:**
*   Create a new realm, e.g., `blazor_realm`.

**1.2. Create Two Clients (Crucial for Hybrid Apps):**
*   **Confidential Client (for Server-side):**
    *   **Client ID:** `blazor-server-client`
    *   **Client authentication:** `ON`
    *   **Valid Redirect URIs:** `https://localhost:PORT/signin-oidc`
    *   **Credentials:** Note the **Client Secret**.
*   **Public Client (for Client-side/WASM):**
    *   **Client ID:** `blazor-wasm-client`
    *   **Client authentication:** `OFF` (This is critical)
    *   **Valid Redirect URIs:** `https://localhost:PORT/authentication/login-callback`
    *   **Web Origins:** `https://localhost:PORT` (or `+` for development)

**1.3. Define Roles:**
*   In the **confidential client** (`blazor-server-client`), go to the `Roles` tab and create roles like `admin` and `user`.

**1.4. Create a User and Assign Roles:**
*   Create a test user and set a password.
*   In the user's `Role Mappings` tab, assign the client roles (`admin`, `user`) to the user.

**1.5. Configure a Mapper for Roles:**
*   In the **confidential client** (`blazor-server-client`), go to `Client Scopes` -> `[client-id]-dedicated` -> `Mappers`.
*   Create a new mapper of type **User Client Role**.
*   **Token Claim Name:** `roles` (This will create a top-level `roles` claim in the token).
*   **Multivalued:** `ON`.
*   **Add to access token:** `ON`.

#### 2. Blazor Web App Project Setup

*   Create a new **Blazor Web App** project in Visual Studio or via `dotnet new`.
*   **Interactive render mode:** Server and WebAssembly.
*   **Interactivity location:** Per page/component.

#### 3. Server-Side Authentication Setup (`Program.cs`)

**3.1. Install Packages:**
*   `Microsoft.AspNetCore.Authentication.OpenIdConnect`
*   `Microsoft.AspNetCore.Authentication.Cookies`

**3.2. Configure `appsettings.json`:**
```json
"Keycloak": {
  "Authority": "http://localhost:8080/realms/blazor_realm",
  "ClientId": "blazor-server-client", // The confidential client
  "ClientSecret": "YOUR_CLIENT_SECRET"
}
```

**3.3. Configure Services in `Program.cs`:**
```csharp
// using System.IdentityModel.Tokens.Jwt;

// Recommended: Prevents automatic renaming of claims like "roles".
// JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

builder.Services.AddAuthentication(options => {
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie()
.AddOpenIdConnect(options => {
    // Bind settings from appsettings.json
    builder.Configuration.GetSection("Keycloak").Bind(options);
    options.ResponseType = "code";
    options.SaveTokens = true;
    options.GetClaimsFromUserInfoEndpoint = false; // Recommended to rely on the token

    options.TokenValidationParameters = new TokenValidationParameters {
        NameClaimType = "name",
        // The default RoleClaimType works because the 'roles' claim is automatically
        // mapped to the standard .NET role claim type.
        RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
    };
});

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
```

**3.4. Add Login/Logout Endpoints (instead of Razor pages):**
```csharp
app.MapGet("/Account/Login", async (HttpContext context, string redirectUri = "/") => {
    await context.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme, new AuthenticationProperties { RedirectUri = redirectUri });
});

app.MapGet("/Account/Logout", async (HttpContext context, string redirectUri = "/") => {
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    await context.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme, new AuthenticationProperties { RedirectUri = redirectUri });
});
```
Update `LoginDisplay.razor` to navigate to these endpoints with `forceLoad: true`.

#### 4. Client-Side Authentication Setup (`Client/Program.cs`)

**4.1. Install Package:**
*   `Microsoft.AspNetCore.Components.WebAssembly.Authentication`

**4.2. Configure `Client/wwwroot/appsettings.json`:**
```json
"Keycloak": {
  "Authority": "http://localhost:8080/realms/blazor_realm",
  "ClientId": "blazor-wasm-client" // The public client
}
```

**4.3. Configure Services in `Client/Program.cs`:**
```csharp
builder.Services.AddOidcAuthentication(options => {
    builder.Configuration.Bind("Keycloak", options.ProviderOptions);
    options.ProviderOptions.ResponseType = "code";
    options.ProviderOptions.DefaultScopes.Add("roles");
});

builder.Services.AddHttpClient("ServerApi", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
    .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("ServerApi"));
```

#### 5. Authorization Implementation

**5.1. Protect Pages:**
*   Use the `[Authorize]` attribute on Razor components. To restrict by role, use `[Authorize(Roles = "admin")]`.
*   Ensure `Routes.razor` uses `<AuthorizeRouteView>`.

**5.2. Protect UI Elements:**
*   Use the `<AuthorizeView>` component to conditionally render content.
    ```razor
    <AuthorizeView Roles="admin">
        <p>This is visible only to admins.</p>
    </AuthorizeView>
    ```

#### 6. Database and Web API Access

**6.1. Create a Web API Endpoint (Server-side `Program.cs`):**
```csharp
// using System.Security.Claims;

app.MapGet("/api/products", (ClaimsPrincipal user) => {
    if (user.IsInRole("admin")) {
        // Return all products
    }
    // Return public products
}).RequireAuthorization(new AuthorizeAttribute { Roles = "user,admin" });
```
This protects the API endpoint and allows for fine-grained data access control based on the user's roles.

**6.2. Call the API from a WASM Component:**
*   Create a Razor component with `@rendermode InteractiveWebAssembly`.
*   Inject `HttpClient`.
*   Wrap the API call in a `try-catch` block to handle `AccessTokenNotAvailableException`.

```csharp
// using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

try
{
    products = await Http.GetFromJsonAsync<Product[]>("api/products");
}
catch (AccessTokenNotAvailableException exception)
{
    // Redirects the user to log in to acquire a new token
    exception.Redirect();
}
```
