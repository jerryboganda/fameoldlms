# FAME Old LMS Deployment

This repository contains the source project for the latest safe LMS changes.

## What is included

- `FAME LMS Production Source Code/First Aid Made Easy.sln`
- `FAME LMS Production Source Code/FAME.Web/`
- source changes for Ambassador, auth, verification, and admin fixes

## What is intentionally excluded

- live secrets
- `Web.config`
- database backups and attached database files
- local QA evidence and temporary scripts
- published server copy folder
- build output folders (`bin/`, `obj/`)

## Before deploying

1. Clone the repository on the Windows server.
2. Restore NuGet packages.
3. Copy `FAME LMS Production Source Code/FAME.Web/Web.config.template` to `FAME LMS Production Source Code/FAME.Web/Web.config`.
4. Fill in all real production secrets and connection strings.
5. Build/publish the site from `FAME LMS Production Source Code/FAME.Web`.
6. Point IIS to the published output or website root, depending on your deployment model.

## Required config values

You must set these with real production values before deployment:

- SQL connection strings
- SMTP account/password/host/port
- OAuth secrets for Google/Facebook if used
- SMS keys only if you ever re-enable SMS flows
- JWT secret values
- Gemini API key if MCQ parsing is used

## Ambassador-related changes included

- fixed ambassador referral link and referral code attribution
- fixed admin vs ambassador login routing mixups
- fixed ambassador admin pages and edge-case actions
- enforced email verification for registration
- disabled active mobile OTP flow; email verification is the required path

## Recommended deploy command flow

Example on a Windows server with Visual Studio Build Tools installed:

```powershell
nuget restore "FAME LMS Production Source Code\First Aid Made Easy.sln"
msbuild "FAME LMS Production Source Code\First Aid Made Easy.sln" /p:Configuration=Release
```

Then publish or copy the built site into IIS.
