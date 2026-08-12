# Test login and Admin Management

Run both `CCAP.API` and `CCAP.Web` from the Visual Studio multiple-startup profile.

## Development login

- Email: `admin@ccap.local`
- Password: `Admin123!`

The API seeds the Administrator role and grants it all initial permissions.

## Test flow

1. Open `/login`.
2. Sign in with the development credentials.
3. Open `/admin/users`.
4. Click **Add New User**.
5. Select a role and discipline.
6. Save the user.
7. Edit the user and change the role.
8. Open `/admin/roles`.
9. Click the shield icon for a role.
10. Enable/disable permissions.
11. Save.
12. Log out and log in again to receive a JWT containing the updated permission claims.

The JWT contains role and permission claims. The API uses permission policies
for Users and Roles endpoints.

Do not use the development credentials outside local development.
