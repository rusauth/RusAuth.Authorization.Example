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
  - public demo application deployed to `https://example-demo.rusauth.ru`
- `RusAuth.Authorization.Example.Tests`
  - unit tests for the example application logic
- `pipelines/charts/rusauth-authorization-example`
  - Helm chart for the K3s deployment behind Gateway API
- `.github/workflows/ci.yml`
  - PR build/test workflow and automatic `master` deployment

## Package dependencies

The example consumes public NuGet packages:

- `RusAuth.Authorization` `1.0.2`
- `RusAuth.Authorization.Contracts` `1.0.2`

Standard `dotnet restore` uses NuGet.org. No private feed is required for the published package versions.

## Local development

Restore, build, and test:

```powershell
dotnet restore D:\Developer\SERVER\RusAuth\Example\RusAuth.Authorization.Example.slnx --nologo
dotnet build D:\Developer\SERVER\RusAuth\Example\RusAuth.Authorization.Example.slnx --nologo
dotnet test D:\Developer\SERVER\RusAuth\Example\RusAuth.Authorization.Example.slnx --nologo
```

Local secret values should stay in `appsettings.Development.Local.json`, which is ignored by git.

## Configuration

Tracked `appsettings.json` keeps only the public defaults:

```json
{
  "RusAuth": {
    "BaseUrl": "https://rusauth.ru/auth-client-api/",
    "Token": "",
    "TimeOut": 15
  },
  "Example": {
    "CallbackBearerToken": ""
  }
}
```

Production secrets are not committed because this repository is public.

Runtime configuration is provided through Kubernetes secrets:

- `RusAuth__Token`
- `Example__CallbackBearerToken`

The callback bearer token is created in Kubernetes on first deploy and reused on later deploys unless a manual workflow run explicitly rotates it.

## Data Protection

The example uses ASP.NET Core antiforgery for the interactive server UI, so the Data Protection key ring must survive pod restarts.

The Helm chart now mounts a persistent volume and stores the key ring at:

- `/var/lib/rusauth-authorization-example/data-protection`

If that storage is removed or the key ring changes unexpectedly, browsers may send stale antiforgery cookies and the app can log:

- `The antiforgery token could not be decrypted.`

In that case the deployment is healthy, but existing browser cookies are no longer valid. A browser refresh or cookie clear will issue a new token.

## CI/CD

GitHub Actions workflow:

- pull requests to `master`
  - restore
  - build
  - test
  - publish the app with the pinned Windows SDK
  - verify `wwwroot/_framework/blazor.web.js` exists in the published output
  - Helm lint and render
- push to `master`
  - repeat the same validation
  - package the published output into the runtime container image and push to GHCR
  - deploy automatically to K3s namespace `rusauth-example-demo`

The runtime image is intentionally built from the already-published app output. This avoids a Linux SDK publish regression that was dropping the Blazor framework assets from the container image.

Required GitHub Actions secrets:

- `KUBECONFIG_B64`
- `GHCR_PULL_USERNAME`
- `GHCR_PULL_TOKEN`
- `RUSAUTH_EXAMPLE_API_TOKEN`

## Deployment

The production deployment uses:

- image: `ghcr.io/rusauth/rusauth-authorization-example`
- hostname: `example-demo.rusauth.ru`
- ingress model: Gateway API through the shared `edge` gateway in `gateway-system`
- namespace: `rusauth-example-demo`

The chart expects a runtime secret named `rusauth-authorization-example-secrets`.
HTTP to HTTPS redirection is handled at the Gateway. The app itself serves plain HTTP on port `8080` inside the cluster.

## Operational note

The application runs behind Traefik with TLS termination on the gateway. The app itself trusts forwarded headers and exposes:

- `/health/live`
- `/health/ready`

for Kubernetes liveness and readiness probes.

## Runtime behavior

The demo pages refresh after callbacks through an in-process confirmation notifier. This keeps the example reliable in the current single-replica deployment and avoids making the server reconnect to its own public URL during prerender.
