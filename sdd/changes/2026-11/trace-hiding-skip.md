# The pipeline trace skips a file that holds code hidden in another build configuration

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | none — the pipeline trace (`scripts/trace.sh`) is a repository tool that no spec covers; the CLI spec excludes it as a non-goal |

## What

The pipeline trace reports a file that holds a hiding carrier, as FMT-PIPE-033 defines it, as a legitimate skip, as it already reports a file with a syntax error and a generated file. On the first pass it prints

```text
Skipped (code hidden in another build configuration): <full path>
```

to standard output, runs no phase, and exits with `0`. It decides this by the formatter's definition, through `ReihitsuFormatter.HoldsHidingCarrier`, after the syntax-error and the generated-source check and before any phase runs. When a later pass's input holds a hiding carrier, the trace treats it as it treats a later pass that has a syntax error or looks generated: it prints `trace: pass <n> produced source with code hidden in another build configuration before the next pass could run.` to standard error and exits with `2`.

`scripts/README.md` (*Formatter pipeline trace*) names the new skip next to the other two.

## Why

The trace exists to show what the formatter does to a file. It calls the pipeline directly, so it bypasses the checks with which `ReihitsuFormatter.FormatDocumentAsync` and `FormatSyntaxTree` leave such a file unchanged (FMT-PIPE-033). For such a file it prints phase diffs for changes that `reihitsu-format` and the code fixes never make (CLI-021, SHARED-FIX-012), and says nothing about why. A reader who traces a file to learn why `reihitsu-format` left it alone sees the opposite of what happens.

The trace already handles the other two inputs whole-document formatting leaves alone — a syntax error (FMT-PIPE-005) and a generated file (FMT-PIPE-006) — by naming them as skipped. A hiding carrier is the third, and FMT-PIPE-033 treats it like those two. Reporting it the same way keeps the trace's picture of which files are formatted identical to the formatter's and the CLI's, with the CLI's wording for the reason.

Reproduced on `main` (e2a1f5f): the trace of the file below prints diffs from `LineBreakPhase`, `HorizontalSpacingPhase`, `IndentationPhase`, and `CleanupPhase` and exits with `0`, while `scripts/format.sh --check --verbose` on the same file prints `Skipped (code hidden in another build configuration): <path>`.

```csharp
class C
{
#if true
    string s = @"
#else
";
#endif
    void M( ) { }
}
```

## Visible output changes

None — no output an end user sees changes. The trace is repository tooling (`scripts/trace.sh`), not part of `reihitsu-format`, the analyzers, or the code fixes. Its own output changes as follows, for the file above:

Before:

```text
Tracing <path> (maximum passes: 3)
Pass 1
== Pass 1: LineBreakPhase ==
…
Pass 1 changed source in 4 phase(s).
Pass 2
Stable after pass 2 (0 phase changes observed).
```

After:

```text
Tracing <path> (maximum passes: 3)
Skipped (code hidden in another build configuration): <path>
```

Exit code `0` in both. A file that holds a hiding carrier and also a shape the formatter refuses as unsupported is now named as skipped instead of reaching the precheck, whose refusal the trace used to print to standard error on every pass; FMT-PIPE-033 applies before the precheck in the formatter and the CLI too (CLI-021).

## Decisions

### Skip instead of tracing with a note

The trace skips the file and runs no phase, as it does for a syntax error and a generated file. Every entry point a caller reaches leaves such a file unchanged — whole-document formatting by FMT-PIPE-033, the code fixes by SHARED-FIX-012, the CLI by CLI-021 — so a phase diff for it shows a change that never happens anywhere, and the trace's purpose is to show what does happen.

### The formatter's definition and the CLI's wording

The check is `ReihitsuFormatter.HoldsHidingCarrier`, the one public query every other caller uses, so the trace cannot drift from the formatter on which files count. The skip reason is the CLI's (`code hidden in another build configuration`, CLI-021), so the trace and `reihitsu-format --verbose` name the same file the same way.

### Order of the checks

Syntax errors first, then generated source, then the hiding carrier — the order of the CLI (CLI-021: a file with a syntax error or a generated file is skipped as such and not under the hiding-carrier skip) and of the formatter's own entry points. A file that is both generated and holds a carrier is reported as generated, as today.

### A later pass

The trace checks every pass's input, so the new check applies to later passes as the other two do, with the same outcome: an error that names the pass, and exit code `2`. No phase is known to produce a hiding carrier; the case exists so the three skips stay one rule rather than for an input known to reach it, and it needs no test of its own.

### No spec

The trace has no spec: `sdd/specs/cli/reihitsu-format.md` excludes it as a non-goal, and it is repository tooling. The issue's alternative — a tooling spec that states the trace deliberately does not skip — is rejected below, so no requirement is written; `scripts/README.md` is the trace's documentation and states the skip, and a tooling test in `Reihitsu.Cli.Test` covers it.

## Alternatives considered

- **Trace the phases and print a note that whole-document formatting skips the file** — rejected: the note would sit in front of diffs that no caller ever produces, and the trace would be the only consumer of FMT-PIPE-033 that treats such a file differently from a syntax error or a generated file. Tracing the phases of such a file remains possible by removing the conditional group from a copy of it.
- **Add a `--force` option that traces such a file anyway** — rejected: no one has asked for it, the other two skips have no such option, and the phases' behavior on the code outside the carrier is shown by a copy without the conditional group.
- **State in a spec that the trace deliberately does not skip** — rejected: it would document a picture of the formatter that the formatter does not have, and the trace has no spec in which to state it.
- **Exit with a code other than `0` for the skip** — rejected: `0` is the trace's code for a legitimately skipped input (`scripts/README.md`), and the CLI does not let a skipped file affect its exit code either (CLI-021).
