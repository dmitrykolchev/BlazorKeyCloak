# Blazor Application & Keyсloak

Blazor Application uses KeyCloak authentication to authorize access to pages, database data and REST API.

## Requirements

Install Keyсloak. Use Docker Desktop and Keyсloak image
1. Pull image
2. Run image

### Run Keyсloak
``` cmd
docker pull quay.io/keycloak/keycloak:latest
docker run -d -p 8080:8080 -e KEYCLOAK_ADMIN=admin -e KEYCLOAK_ADMIN_PASSWORD={ADMIN_PASSWORD} quay.io/keycloak/keycloak:latest start-dev
```

### Configure Keyсloak & Run Example Blazor application

1. Create `aspnet` realm
2. Create `BKU2` client
3. Set client credentials
4. Add client roles `Employee` and `Manager`
5. In the client scope `BKU2-dedicated` create role mapper
6. Add users `Employee` and `Manager`. Assign role `Employee` to the user `Employee` and `Manager` to the user `Manager`. Don't forget set user's passwords.
7. Check and edit `BlazorKeyCloak\appsettings.json` to set correspondent parameters
8. Create `BKU2-PUB` client that will be used by the Blazor BACM client and will not use Client authentication (see image bellow)
9. Check and edit configuration file `BlazorKeyCloak.Client\wwwroot\appsettings.json`
10. After realm configured you can run `BlazorKeyCloak` application.

After that steps server authentication works.

## Client Parameters

### BKU2 parameters
<img width="1513" height="798" alt="image" src="https://github.com/user-attachments/assets/4174a754-f58d-47cd-a851-424073a93674" />
<img width="1017" height="554" alt="image" src="https://github.com/user-attachments/assets/6e63fe7a-2b59-4087-b48c-668ea062b5be" />

### BKU2 Client Role Mapper
<img width="1183" height="810" alt="image" src="https://github.com/user-attachments/assets/04365c7a-c946-493f-aadb-d5b0b377a844" />

### BKU2-PUB parameters

Client authentication for WASM client must be OFF.
Explanation: `BKU2` is used by two "personas": the server part (which needs Client Secret and Client authentication = ON) and the 
WASM client part (which is a public client and cannot store Client Secret securely, so it must have Client authentication = OFF). 
This creates a conflict. Because of this we need to create the second client `BKU2-PUB`

<img width="1059" height="776" alt="image" src="https://github.com/user-attachments/assets/18083cfb-af0c-4f6d-94a7-ed7664df6c5f" />
<img width="1246" height="443" alt="image" src="https://github.com/user-attachments/assets/190043c6-df03-4dab-b45a-9aada7614047" />

## Summary

Blazor Server (BlazorKeyCloak):
Is a confidential client. It can securely store Client Secret. Uses Client Secret to authenticate the app itself to 
Keyсloak during the "Authorization Code Flow". Its job is to establish a secure session for the user (via cookie). 
Uses the BKU2 client.

Blazor WASM Client (BlazorKeyCloak.Client):
Is a public client. All of its code (including configuration) is loaded into the user's browser and cannot store any secrets.
It does not need Client Secret. It uses the more secure PKCE (Proof Key for Code Exchange) mechanism, which is built into the 
Blazor OIDC library, to protect the "Authorization Code Flow". Its job is to obtain an Access Token to call protected APIs.
Uses the new BKU2-PUB client, which has Client authentication disabled.
