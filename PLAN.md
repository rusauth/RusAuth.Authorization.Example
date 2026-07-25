# RusAuth authorization example modernization

Status: Complete; source publication and release evidence recorded in the handoff

## Goal

Keep `RusAuth.Authorization.Example.slnx` aligned with the released public RusAuth authorization packages, make the application runnable from Rider and its published executable with local configuration, and prove the example against packages restored from NuGet.org.

## Phases

1. Package alignment
   - Target released `RusAuth.Authorization` and `RusAuth.Authorization.Contracts` `1.0.7`.
   - Update the .NET SDK and test/tooling dependencies to the reviewed versions.
   - Do not use cross-repository project references or private `InternalSso` packages.
2. Startup and configuration
   - Load optional `appsettings.{Environment}.Local.json` after the standard configuration providers.
   - Keep local tokens and data-protection paths outside source control and publish output.
   - Keep tracked configuration free of credentials and document the environment-variable alternatives.
3. Verification
   - Restore from NuGet.org only after public `1.0.7` packages are published.
   - Build the solution and run the complete test suite with an explicit discovered/passed count.
   - Start the built application in `Development` and require `/health/ready` to return HTTP 200.
   - Inspect publish output to prove local configuration is excluded.
4. Release handoff
   - Publish only the intended source changes; preserve and exclude `.idea` and generated artifacts.
   - Record the exact RusAuth package tags/commits, example commit, tests, and startup evidence.

## Acceptance

- The example restores public RusAuth `1.0.7` packages from NuGet.org with no local feed or cross-repository dependency.
- Rider launch and direct executable launch both apply the ignored local overlay.
- No local secrets or `appsettings.Development.Local.json` enter publish output or Git.
- The complete test suite passes and `/health/ready` returns HTTP 200.

## Completion evidence

- NuGet.org-only cold restore completed for both projects with `NuGetAudit=false`; transient package-download timeouts were retried successfully by NuGet.
- Release build completed with zero warnings and zero errors.
- Tests: 10 passed, 0 failed, 0 skipped.
- The built executable started in `Development` and `/health/ready` returned HTTP 200.
- Publish output contained `appsettings.json` and the executable, and excluded `appsettings.Development.Local.json`.
- `RusAuth.Authorization.Contracts` `1.0.7` is provenance-bound to `724434534fdb306dd5e6be7b6b2e53844bc21bee`.
- `RusAuth.Authorization` `1.0.7` is provenance-bound to `96f506b6bc9bb5e99d9948dcd9d0053afbd0b92c` and pins Contracts `[1.0.7]`.
- Reviewed example implementation commit: `555a2b478237c02f6436cdc8a474e288508451fa`.
