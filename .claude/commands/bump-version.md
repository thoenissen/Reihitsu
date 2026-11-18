# Bump Version

Raise the version of Reihitsu, refresh the analyzer binaries the repository builds itself with, prove the solution still builds under them, publish the bump as a pull request, and tag the merged commit on `main`. **$ARGUMENTS** is the new version: `1.4.0`, or a prerelease `1.4.0-beta2` / `1.4.0-rc1`. A leading `v` is dropped. If the argument is missing or does not match `X.Y.Z`, `X.Y.Z-betaN`, or `X.Y.Z-rcN`, ask for it and stop.

The bump changes nothing but the version properties and the four files under `binaries/`. It fixes no finding, formats nothing, and touches no other file.

## 1. Start from `main`

1. Fetch the latest `main` and the tags; a cloud checkout is often shallow:

   ```shell
   git rev-parse --is-shallow-repository   # true → git fetch --unshallow origin
   git fetch origin main --tags
   ```

2. Stop and report if the tag `v<version>` already exists (`git rev-parse -q --verify refs/tags/v<version>`), or if `<version>` is not higher than the current version in `Directory.Build.props`.
3. Work on the session's development branch, or else on `bump/v<version>`, created from `origin/main`. The working tree must be clean.

## 2. Set the version

Edit the first `PropertyGroup` with version properties in `Directory.Build.props`:

| Argument | `VersionPrefix` | `BetaNumber` | `ReleaseCandidateNumber` |
|---|---|---|---|
| `1.4.0` | `1.4.0` | empty | empty |
| `1.4.0-beta2` | `1.4.0` | `2` | empty |
| `1.4.0-rc1` | `1.4.0` | empty | `1` |

Leave every other property, including the derived `AssemblyVersion`, `Version`, and `PackageVersion`, as it is.

## 3. Build the new analyzer

```shell
scripts/build.sh
```

This Release build still runs the old analyzer from `binaries/` over the solution. If it fails, stop and report: `main` itself is broken, and that is not the bump's to fix.

## 4. Replace the binaries

Copy the four freshly built assemblies over the ones under `binaries/`, which `Directory.Build.props` loads as analyzers for the whole solution:

```shell
for project in Reihitsu.Core Reihitsu.Analyzer Reihitsu.Analyzer.CodeFixes Reihitsu.Formatter; do
    cp "src/$project/bin/Release/netstandard2.0/$project.dll" binaries/
done
```

Check that each copied file carries the new version: `strings -el binaries/<project>.dll | grep '^<version>+'` must print the informational version, `<version>+<commit of main>`. `git status` must show exactly `Directory.Build.props` and the four DLLs as changed.

## 5. Build under the new analyzer

The compiler server and incremental builds can keep the old analyzer assemblies loaded, so shut the server down and rebuild from scratch:

```shell
dotnet build-server shutdown
scripts/build.sh --no-incremental
```

The Release build treats warnings as errors, so every finding the new analyzer reports in the repository's own code fails it. If it fails, do **not** fix the findings in the bump and do not commit. Report to the maintainer, as `AGENTS.md` (*Asking the maintainer*) describes: the failing rule IDs, the number of findings per rule, and two or three representative locations with the reported code; then recommend whether to fix them in a pull request of their own before the bump or to treat them as an analyzer defect, and stop.

## 6. Publish the pull request

1. Commit exactly the five files with the subject `Bump version to <version>`.
2. Push the branch and open a pull request against `main` titled `Bump version to <version>`. Fill the headings of `.github/PULL_REQUEST_TEMPLATE.md`: the *Summary* names the new version and that the binaries the solution analyzes itself with were rebuilt from the current `main`; *Why* is the release; *Linked issues* says none; drop *Review notes* and *Follow-up work* unless there is something to say.
3. Subscribe to the pull request's activity and end the turn. CI and the maintainer's merge arrive as events; do not poll.

## 7. Tag the merged commit

When the pull request is merged:

1. Fetch `main` and find the commit the merge put on it — the squash commit whose subject is `Bump version to <version> (#<N>)`, or the merge commit:

   ```shell
   git fetch origin main
   git log origin/main --format='%H %s' -5
   ```

2. Check that this commit's `Directory.Build.props` carries the new version (`git show <sha>:Directory.Build.props`).
3. Create the tag as a lightweight tag, like the earlier release tags, on exactly that commit and push only the tag:

   ```shell
   git tag v<version> <sha>
   git push origin v<version>
   ```

4. Unsubscribe from the pull request and report the tag and its commit. Creating the GitHub release from the tag is the maintainer's step; mention that `/generate-release-notes v<version> <previous tag>` writes its notes.

If the pull request is closed without merging, unsubscribe, create no tag, and report.
