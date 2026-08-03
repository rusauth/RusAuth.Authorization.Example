# RusAuth.Authorization.Example

`RusAuth.Authorization.Example` is the public reference application for third-party integration with RusAuth over REST.

## Purpose

Use this solution when your system only needs RusAuth phone call confirmation and does not participate in RusAuth internal SSO.

Important:

- the external client works only with the public RusAuth REST API
- the external client authenticates with the company token in the `api-key` header
- after confirmation RusAuth does not create a local application session for you, that remains your responsibility

## What the example demonstrates

- `CallToConfirm`
- receiving the RusAuth phone number and `TransactionId`
- accepting the confirmation webhook
- manual status checks with `CheckConfirmation`
- a local confirmation history

## Project structure

- `RusAuth.Authorization.Example`
  - Blazor Web App Server
  - public demo application available at `https://example-demo.rusauth.ru`
- `RusAuth.Authorization.Example.Tests`
  - unit tests for the example application logic
- `.github/workflows/ci.yml`
  - PR build/test workflow and container-image publication from `master`

## Package dependencies

The example consumes public NuGet packages:

- `RusAuth.Authorization` `1.0.7`
- `RusAuth.Authorization.Contracts` `1.0.7`

Standard `dotnet restore` uses NuGet.org. No private feed is required for the published package versions.

## Local development

Restore, build, and test:

```powershell
dotnet restore D:/Priority/RusAuth/RusAuth-Example/RusAuth.Authorization.Example.slnx --nologo
dotnet build D:/Priority/RusAuth/RusAuth-Example/RusAuth.Authorization.Example.slnx --nologo
dotnet test D:/Priority/RusAuth/RusAuth-Example/RusAuth.Authorization.Example.slnx --nologo
```

Local secret values should stay in `appsettings.Development.Local.json`, which is ignored by git.

## Configuration

Tracked `appsettings.json` keeps only the public defaults:

```json
{
  "RusAuth": {
    "BaseUrl": "https://auth-client.rusauth.ru/",
    "Token": "",
    "TimeOut": 15
  },
  "Example": {
    "CallbackBearerToken": ""
  }
}
```

Production secrets are not committed because this repository is public.

Provide runtime secrets through the deployment system that owns the environment:

- `RusAuth__Token`
- `Example__CallbackBearerToken`

The production deployment must also provide a persistent key-ring directory
and a dedicated PEM certificate/private-key pair through the topology-neutral
configuration keys below:

- `DataProtection__KeyRingPath`
- `DataProtection__CertificatePath`
- `DataProtection__CertificateKeyPath`

Production startup fails if persistence or encryption is absent. Certificate
renewal must preserve the private key. Replacing that key requires retaining
the old decryptor until all persisted key-ring files have rolled.

## Data Protection

The example uses ASP.NET Core antiforgery for the interactive server UI, so a
production host should persist the Data Protection key ring across application
restarts.

If the key ring changes unexpectedly, browsers may send stale antiforgery
cookies and the app can log:

- `The antiforgery token could not be decrypted.`

In that case the application can be healthy while existing browser cookies are
no longer valid. A browser refresh or cookie clear will issue a new token.

## CI/CD

GitHub Actions workflow:

- pull requests to `master`
  - restore
  - build
  - test
  - publish the app with the pinned Windows SDK
  - verify `wwwroot/_framework/blazor.web.js` exists in the published output
- push to `master`
  - repeat the same validation
  - smoke-test the runtime container's management/public health split and absent Swagger surface
  - package the published output into the runtime container image and push to GHCR
- manual workflow runs on `master`
  - repeat validation and publish the image without changing an environment

The runtime image is intentionally built from the already-published app output. This avoids a Linux SDK publish regression that was dropping the Blazor framework assets from the container image.

Images are published only as immutable `sha-<40-character-commit>` tags. There
is no `latest` deployment contract.

## Deployment boundary

This public repository deliberately contains no environment topology,
kubeconfig handling, cluster secret names, or deployment commands. Deployment
is owned by private environment automation. The application exposes:

- `/health/live`
- `/health/ready`

for host liveness and readiness checks.

Set `HealthChecks__ManagementPort` in production and expose that port only
inside the workload. Requests for `/health/*` arriving on the public
application port then return 404.

## Runtime behavior

The demo pages refresh after callbacks through an in-process confirmation
notifier. This is appropriate for the reference application's single-process
runtime model and avoids making the server reconnect to its own public URL
during prerender.
