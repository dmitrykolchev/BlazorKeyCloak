# BlazorLearn

##Requirements
Install KeyCloak. Use Docker Desktop and KeyCloak image
1. Pull image
2. Run image

### Run KeyCloak
``` cmd
docker pull quay.io/keycloak/keycloak:latest
docker run -d -p 8080:8080 -e KEYCLOAK_ADMIN=admin -e KEYCLOAK_ADMIN_PASSWORD={ADMIN_PASSWORD} quay.io/keycloak/keycloak:latest start-dev
```

### Configure KeyCloak & Run Example Blazor application

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
### BKU2-PUB parameters
<img width="1059" height="776" alt="image" src="https://github.com/user-attachments/assets/18083cfb-af0c-4f6d-94a7-ed7664df6c5f" />
<img width="1246" height="443" alt="image" src="https://github.com/user-attachments/assets/190043c6-df03-4dab-b45a-9aada7614047" />


## Client Role Mapper
<img width="1183" height="810" alt="image" src="https://github.com/user-attachments/assets/04365c7a-c946-493f-aadb-d5b0b377a844" />
