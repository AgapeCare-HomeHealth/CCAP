# CCAP Local Startup

## Ports

- CCAP.Web HTTPS: https://localhost:7202
- CCAP.Web HTTP: http://localhost:5202
- CCAP.API HTTPS: https://localhost:7218
- CCAP.API HTTP: http://localhost:5218

The HTTP ports are intentionally different so API and Web can run together without an address collision.

## Visual Studio: start Web + API together

Open `CCAP.sln`, then:

1. Right-click the solution.
2. Select **Configure Startup Projects**.
3. Select **Multiple startup projects**.
4. Set **CCAP.API** = Start.
5. Set **CCAP.Web** = Start.
6. Keep the other projects = None.
7. Apply and run.

The API must be running at `https://localhost:7218` because CCAP.Web's Development configuration points to that URL.

## Production

`launchSettings.json` is for local development only. It is not used to configure the production server.

Production values should be supplied through deployment configuration/environment variables, especially:
- `ConnectionStrings__DefaultConnection`
- `Jwt__Key`
- `Jwt__Issuer`
- `Jwt__Audience`
- `Api__BaseUrl`

Do not commit production secrets.
