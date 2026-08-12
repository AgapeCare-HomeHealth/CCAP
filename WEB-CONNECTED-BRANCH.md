# CCAP Web-Connected Branch

Branch snapshot: `feature/connect-web`

Base snapshot: Clean Architecture backend foundation (`CCAP-CleanArchitecture-Rebuilt-v2`).

## What is connected

`CCAP.Web` now calls `CCAP.API` using `HttpClient`.

Connected pages:
- `/admin/users` -> `GET /api/users`
- `/patients` -> `GET /api/patients`

Connected backend commands already available:
- login
- create/update/delete user
- activate/deactivate user
- add patient call note
- complete patient care
- archive patient

## API URL

Development default:
`https://localhost:7218`

Change `CCAP.Web/appsettings.json` if the API runs at a different URL.

## Important: authentication

The user/patient GET endpoints are protected by `[Authorize]`.
This branch wires the HTTP client, but a complete Web login/token handler still needs
to be added before protected endpoints can be used from a clean browser session.

## Returning to the previous state

If you are using Git, keep the previous backend snapshot on a separate branch and create:

```powershell
git switch -c feature/connect-web
```

Commit the Web integration on this branch.

To return to the backend-only state:

```powershell
git switch <your-backend-only-branch>
```

The ZIP itself cannot carry your existing repository's Git history, so this file records
the intended branch boundary.

## Recommended Git branch boundary

This ZIP is the `feature/connect-web` snapshot. Keep the previous
`CCAP-CleanArchitecture-Rebuilt-v2.zip` as the backend-only rollback point.

If your local repository already contains the backend-only commit:

```powershell
git switch <backend-only-branch>
git switch -c feature/connect-web
# copy this ZIP's contents over the working tree
git status
git add CCAP.Web CCAP.API CCAP.Application CCAP.Infrastructure CCAP.Domain
git commit -m "Connect CCAP Web to API"
```

To return:

```powershell
git switch <backend-only-branch>
```
