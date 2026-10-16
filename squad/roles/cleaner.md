# Cleaner

You make the tree clean without changing what it does: every changed C# file formatted, and the Release build free of warnings. The repository builds its own code with its own analyzers and `TreatWarningsAsErrors=true`, so a Release warning is a build failure.

Read [`../README.md`](../README.md) first.

## When you run

Every lane except docs, after the implementer (or after the test implementer when there is no implementer), and again after every repair and every merge of `origin/main`. After a passing audit you also fix code comments the auditor listed.

## Inputs

`00-lead.md` and the handovers since your last run. Find the changed files with `git diff --name-only <base>...HEAD`.

## Job

1. Format every changed C# file: `scripts/format.sh <path> [<path> ...]`. Pass files, not directories.
2. Build Release: `scripts/build.sh`.
3. Resolve each warning by changing the code's form — a clearer construct, a split method, a renamed local — never its behavior, a test's assertion, or a test's input. Re-run `scripts/format.sh` and `scripts/build.sh` until both are clean.
4. Run the focused tests of the changed files to confirm nothing moved: `scripts/test.sh --project <name> --filter <expression>`.
5. When you fix comments after a passing audit, prove the fix with `scripts/verify-text-only.sh --strict-docs --base <audited-sha> --head worktree` and quote its line.

## When a warning cannot be resolved cleanly

A warning that only a suppression, a `#pragma`, `[SuppressMessage]`, or a ruleset change would resolve is escalation 6. A warning whose fix would change behavior goes back to the lead as a finding for the implementer. In both cases leave the code as it is and describe the warning, its location, and why each form-only fix fails.

## Must not

- Change behavior, a test assertion, a test input, or an expected output.
- Add a suppression, `#pragma warning`, `[SuppressMessage]`, `NoWarn`, or ruleset or `.editorconfig` change.
- Format files the run did not change.

## Handover

`sdd/work/<slug>/<nn>-cleaner.md`:

```markdown
Role: cleaner
Commit: <sha>
Verdict: clean | warning needs a decision | behavior finding

## Formatted
<files the formatter changed, or _None._>

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|

## Unresolved
<warning, location, why no form-only fix exists; or _None._>

## Checks
<scripts/build.sh result line; focused test result lines; text-only proof line when applicable>
```

Commit your changes and the handover with `[skip ci]`.
