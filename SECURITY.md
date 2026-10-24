# Security Policy

## Supported versions

Security fixes are released for the latest published version of `Reihitsu.Analyzer` and `Reihitsu.Cli`. Please update to the latest version before reporting.

## Reporting a vulnerability

Please do not report security problems in public issues, pull requests, or discussions.

Report them privately through GitHub instead:

1. Open the repository's **Security** tab.
2. Choose **Report a vulnerability**.
3. Describe the problem, the affected version, and steps to reproduce it.

The report is visible only to the maintainer. You will get a response as soon as possible, and the fix and its disclosure are coordinated with you in the same private thread.

## Scope

Reihitsu is a Roslyn analyzer and a formatting command-line tool that work on source code. Problems in scope include, for example, a crafted input file that makes the tool execute code, write outside the intended paths, or hang the build.

Wrong or missing diagnostics and formatting results are ordinary bugs. Please report them as [issues](https://github.com/thoenissen/Reihitsu/issues/new/choose).
