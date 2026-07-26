# RusAuth authorization example handoff

Date: 2026-07-25

Revalidated: 2026-07-26

## Outcome

`RusAuth.Authorization.Example.slnx` consumes only the released public packages:

- `RusAuth.Authorization.Contracts` `1.0.7`
- `RusAuth.Authorization` `1.0.7`

There are no cross-repository project references and no dependency on the private `RusAuth.Authorization.InternalSso` package.

The application loads the optional ignored `appsettings.{Environment}.Local.json` overlay after normal configuration. Base configuration is safe to commit, local startup configuration is ready, and local settings are excluded from publish output.

## Immutable package evidence

- `nuget-rusauth-authorization-contracts-v1.0.7` -> `724434534fdb306dd5e6be7b6b2e53844bc21bee`
- `nuget-rusauth-authorization-v1.0.7` -> `96f506b6bc9bb5e99d9948dcd9d0053afbd0b92c`
- Contracts package metadata was verified against commit `724434534fdb306dd5e6be7b6b2e53844bc21bee`.
- Authorization package metadata was verified against commit `96f506b6bc9bb5e99d9948dcd9d0053afbd0b92c`, with exact dependency `RusAuth.Authorization.Contracts [1.0.7]`.

## Verification

- NuGet.org-only cold restore: passed. NuGet retried transient 60-second download stalls and completed.
- Release build: passed with 0 warnings and 0 errors.
- Complete tests: 10 passed, 0 failed, 0 skipped.
- NuGet vulnerability audit: no vulnerable direct or transitive packages
  after overriding the test dependency with AngleSharp `1.5.2`.
- Direct executable startup in `Development`: passed.
- `GET /health/ready`: HTTP 200.
- Local overlay present for development startup: yes.
- Publish inspection: `appsettings.json` and executable present; `appsettings.Development.Local.json` absent.

## Source publication

Reviewed implementation commit:

- `555a2b478237c02f6436cdc8a474e288508451fa`

The publication audit compared all ten changed local paths with their GitHub blob hashes and found zero mismatches. Local settings, `.idea`, `bin`, `obj`, and generated artifacts were excluded.

## CI and deployment boundary

The public workflow retains restore, build, 10/10 tests, application publish,
Blazor asset verification, artifact upload, and GHCR image publication.
Environment topology, cluster credentials, secret-object names, and deployment
commands were removed from the public repository. Private environment
automation owns deployment and consumes the commit-tagged image.

## Origin reconciliation review

- `master` was fast-forwarded from `54fe3d6` to `origin/master`
  `d8dd40a577df2dc5afbf35bd39a1a46cee0b1687`.
- Incoming commits: `555a2b4`, `708b850`, and `d8dd40a`.
- Existing local work was preserved and reapplied; the CI workflow and
  `.gitignore` overlaps were resolved semantically.
- The solution has no direct or transitive `Library.*` package dependency, so
  adding a Library SDK package solely to pin `1.0.23` would create an unused
  dependency.
- NuGet.org restore and the vulnerability audit passed. Release build completed
  with zero warnings and errors, all 10 tests passed, and the built Development
  executable returned HTTP 200 from `/health/ready`.
- Publish validation found the executable, `appsettings.json`, and
  `wwwroot/_framework/blazor.web.js`; no Development or Local settings were
  published.
