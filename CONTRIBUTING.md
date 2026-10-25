# Contributing to Reihitsu

Thank you for your interest in Reihitsu. By taking part you agree to follow the [Code of Conduct](CODE_OF_CONDUCT.md).

## Questions and ideas

Use [Discussions](https://github.com/thoenissen/Reihitsu/discussions) for questions, support, and open-ended ideas. Use [Issues](https://github.com/thoenissen/Reihitsu/issues/new/choose) for bug reports and feature requests; pick the matching template, and include a minimal input that shows the problem.

Security problems are reported privately, see [SECURITY.md](SECURITY.md).

## Picking up an issue

Issues labeled `help wanted` are open for anyone to take, and `good first issue` marks the ones that suit a first contribution. Leave a short comment before you start so that two people do not work on the same issue. For anything else, ask in the issue first: a change that alters behavior is agreed on before it is built.

## Development setup

You need the .NET 10 SDK. The scripts under `scripts/` are the entry points for everything, and they install the SDK into `$HOME/.dotnet` when it is missing (see [`scripts/README.md`](scripts/README.md)). They are Bash scripts for Linux and macOS; on Windows, use WSL or Git Bash.

```shell
scripts/prepare.sh   # verify the toolchain
scripts/build.sh     # build the solution in Release
scripts/test.sh      # run all test projects except the self-hosting scans, which CI runs
```

Run a single test project or filter with `scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH3202"`.

## Making a change

1. Fork the repository and create a branch from `main`.
2. Write the test first. A bug fix starts with a test that reproduces the bug, for analyzer and formatter fixes alike.
3. Make the change. Behavior is specified under [`sdd/specs/`](sdd/README.md); read the rulebook there before changing behavior a spec covers, and update the spec together with the code.
4. Format the files you changed, then run the tests:

   ```shell
   scripts/format.sh <changed-path> [<changed-path> ...]
   scripts/test.sh
   ```

5. Open a pull request and fill in the template. Keep the description short and link the issue, for example `Closes #123`.

## Conventions

- Everything is in English: code, comments, documentation, commit messages, issues, and pull requests.
- A new or renamed analyzer rule comes with its page under `documentation/rules/RH####.md`.
- Source carries no `TODO` comments and no references to issue or pull request numbers. Unfinished work is a follow-up issue.
- Do not commit changes to the `binaries/` folder. It holds the prebuilt analyzer assemblies that the repository uses on itself, and the maintainer refreshes them at each version bump.
- Prefer small, focused tests over one large test with many cases.

[`AGENTS.md`](AGENTS.md) holds the complete repository instructions. It is written for coding agents, and the `squad/` folder describes how agents implement issues; as a human contributor you do not need to use either.
