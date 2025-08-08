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

### Run Example Blazor application

1. Create `aspnet` realm
2. Create `BKU2` client
3. Set client credentials
4. Add client roles `Employee` and `Manager`
5. In the client scope `BKU2-dedicated` create role mapper
6. Add users `Employee` and `Manager`. Assign role `Employee` to the user `Employee` and `Manager` to the user `Manager`. Don't forget set user's passwords.
7. Edit `appsettings.info` to set correspondent parameters
8. After realm configured you can run `BlazorKeyCloak` application.

## Client Parameters
<img width="1513" height="798" alt="image" src="https://github.com/user-attachments/assets/4174a754-f58d-47cd-a851-424073a93674" />
<img width="1017" height="554" alt="image" src="https://github.com/user-attachments/assets/6e63fe7a-2b59-4087-b48c-668ea062b5be" />


## Client Role Mapper
<img width="1183" height="810" alt="image" src="https://github.com/user-attachments/assets/04365c7a-c946-493f-aadb-d5b0b377a844" />
