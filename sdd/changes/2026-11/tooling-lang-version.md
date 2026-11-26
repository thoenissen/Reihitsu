# The pipeline trace and the code-fix fixture runner take a language version

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | none — the pipeline trace (`scripts/trace.sh`) and the code-fix fixture runner (`scripts/apply-fix.sh`) are repository tools that no spec covers: the CLI spec excludes the trace as a non-goal, and `Reihitsu.Tooling` gets no spec |

## What

`scripts/trace.sh` and `scripts/apply-fix.sh` accept the option `--lang-version <version>`, as `reihitsu-format` does (CLI-001, CLI-004), and run with that version's parse options.

- **Where.** The option may stand anywhere among the arguments before a literal `--`; after `--` every argument is positional, as for the tools' other options. The argument behind `--lang-version` is always its value, also when it starts with `-`.
- **Values and default.** A value is accepted exactly when `reihitsu-format` accepts it for `--lang-version` (CLI-001): `12`, `12.0`, `latest`, `preview`, and every other C# language version Roslyn names. It resolves as CLI-004 resolves it: without the option, and with `latest`, the newest version the formatter supports, today 15.0; a value newer than that, such as `preview`, is lowered to it, with one line on standard error, `<tool>: Warning: C# language version '<value>' is newer than the newest supported version <version>; formatting for <version>.`, where `<tool>` is `trace` or `apply-fix`. The warning changes no exit code.
- **What the version applies to.** The trace parses the input of every pass with it, so its syntax-error skip and every version-dependent formatter rule (FMT-PIPE-008) follow it. The fixture runner uses it for every parse it makes: the check whether a fixture parses (`parse-error`), the parse options of the ad-hoc project in which the analyzer reports and the code fix runs — and so the version the formatter reads inside the fix (FMT-PIPE-008) — and the check whether a fix result parses (`invalid-result`).
- **Errors.** Each of these fails before any file or fixture is read, with the message below on standard error, followed by the usage text, and exit code `2`, as the tools' other argument errors do:
  - a value that is not a C# language version: `<tool>: invalid value for --lang-version: '<value>'. Expected a C# language version such as 12, 12.0, latest, or preview.`;
  - `--lang-version` as the last argument: `<tool>: missing value for --lang-version.`;
  - `--lang-version` given twice: `<tool>: --lang-version may only be specified once.`
- **Usage.** The usage lines name the option: `usage: trace <file> [--passes <positive-integer>] [--lang-version <version>]` and `usage: apply-fix <diagnostic-id> <fixture-directory> [--max-iterations <positive-integer>] [--lang-version <version>]`.

Both tools always parse with the resolved version, as `reihitsu-format` does: with the option, the version its value resolves to; without it, the newest version the formatter supports. Neither tool keeps a second path that parses with Roslyn's default parse options or with `LanguageVersion.Latest`. Without the option, their output is the same as today's, because the Roslyn version the repository uses (5.12.0) maps both of today's options to C# 15, the newest version the formatter supports; after a Roslyn upgrade that maps them to a newer version, the tools still parse as `format.sh` does. `scripts/README.md` (*Formatter pipeline trace*, *The code-fix fixture runner*) lists the option.

## Why

A question that depends on the language version — a code fix or a formatter phase below C# 11, where an interpolation hole that holds a line break is laid out differently — cannot be answered with the repository's own entry points today: `scripts/format.sh` accepts `--lang-version`, but the trace and the fixture runner always parse with the newest version, and a throwaway probe is needed instead. The two tools exist so that a role answers such questions with one command.

Reproduced on the run's start commit b8b7365: `scripts/trace.sh <file> --lang-version 10` exits with `2` and `trace: unknown argument '--lang-version'.`, and `scripts/apply-fix.sh RH3202 <dir> --lang-version 10` with `apply-fix: unknown argument '--lang-version'.`. `scripts/format.sh <file> --dry-run --lang-version 10` on the file of example 1 keeps the interpolation hole as written, while without the option it moves the `+` to the start of the hole's second line.

## Visible output changes

None — no output an end user sees changes. Both tools are repository tooling, not part of `reihitsu-format`, the analyzers, or the code fixes, and without the new option their output is unchanged. With it, their output changes as follows.

### 1. The trace below C# 11

`src/H.cs`:

```csharp
class C
{
    string M(int a)
    {
        return $"{a +
            1}";
    }
}
```

`scripts/trace.sh src/H.cs` traces the hole's layout through `LineBreakPhase`, `HorizontalSpacingPhase`, `IndentationPhase`, and `CleanupPhase`, to this result:

```csharp
class C
{
    string M(int a)
    {
        return $"{a
                  + 1}";
    }
}
```

`scripts/trace.sh src/H.cs --lang-version 10` exits with `2` before the change. After it, the trace formats for C# 10, as `scripts/format.sh src/H.cs --lang-version 10` does: the hole keeps its line break where it is written, and the trace shows the same result as `format.sh` for that version.

### 2. The fixture runner below C# 11

A fixture directory that holds:

```csharp
class C
{
    string M(int a) => $"{a +
        1}";
}
```

`scripts/apply-fix.sh RH3202 <dir>` reports `fixed (1 action(s), 1 iteration(s))` with this result:

```csharp
class C
{
    string M(int a)
    {
        return $"{a
                  + 1}";
    }
}
```

`scripts/apply-fix.sh RH3202 <dir> --lang-version 10` exits with `2` before the change. After it, the analyzer and the code fix run in a project parsed as C# 10, and the fix's result is formatted for C# 10.

### 3. An unknown value

`scripts/trace.sh src/H.cs --lang-version 99` printed `trace: unknown argument '--lang-version'.` and now prints `trace: invalid value for --lang-version: '99'. Expected a C# language version such as 12, 12.0, latest, or preview.`, followed by the usage text; the exit code is `2` in both. The fixture runner does the same with the prefix `apply-fix:`.

### 4. A version newer than the newest supported one

`scripts/trace.sh src/H.cs --lang-version preview` prints `trace: Warning: C# language version 'preview' is newer than the newest supported version 15.0; formatting for 15.0.` on standard error and traces the file as without the option.

## Decisions

### The values and the resolution of `reihitsu-format`

Both tools accept exactly the values `reihitsu-format` accepts and resolve them as CLI-004 does, including the lowering of `preview` with a warning, so a question answered with the trace, the fixture runner, and `format.sh` for one `--lang-version` value is answered for one and the same version. The trace already resolves the version for the formatter as the CLI does; it now also parses with the resolved version, as the CLI does, so a file the CLI skips for a syntax error under a version is skipped by the trace under that version too.

### The default is the resolved version, not Roslyn's

Without the option, the tools parse with the newest version the formatter supports, through the same resolution as with a value, rather than with Roslyn's default parse options or `LanguageVersion.Latest`. The two agree today, but only because Roslyn 5.12.0 maps `Default` and `Latest` to the formatter's newest version; one code path keeps the tools on `format.sh`'s default after a Roslyn upgrade, which is the issue's "same default as `format.sh`".

### One owner of the newest supported version

Both tools resolve a value, and decide whether it needs the warning, through the formatter's own language-version resolution, the single owner of the newest supported version and of the lowering (FMT-PIPE-008). The fixture runner reaches it as the CLI does, through the formatter's internal visibility to the tooling assembly; it keeps no copy of the newest version or of the lowering, and the formatter's public surface does not grow for a repository tool.

### The tools' own conventions for errors and repetition

The error lines carry the tool's prefix and the lower-case start of the tools' other argument errors, and a repeated `--lang-version` is an error, as a repeated `--passes` or `--max-iterations` is. `reihitsu-format` lets the last of several values win; the tools keep one rule for all of their options instead.

### No new output line

Neither tool prints the version it runs for. The value is on the command line that started the run, and a line in the output of every run would change the output of runs without the option.

### No spec

Neither tool has a spec: `sdd/specs/cli/reihitsu-format.md` excludes the trace as a non-goal, and `sdd/README.md` gives `Reihitsu.Tooling` none. `scripts/README.md` is their documentation and lists the option; tests in `Reihitsu.Cli.Test` (the trace) and `Reihitsu.Tooling.Test` (the fixture runner) cover it.

## Alternatives considered

- **Pass a `preview` value to the fixture runner's project unlowered**, as a project with `LangVersion` `preview` gives it to an analyzer — rejected: the tools would then disagree with `format.sh` on what one value means, and the formatter inside the fix lowers it to the newest supported version anyway (FMT-PIPE-008).
- **Let the last of several values win, as `reihitsu-format` does** — rejected: see *Decisions*; each tool keeps one rule for repeated options.
- **Keep Roslyn's default parse options, or `LanguageVersion.Latest`, when the option is absent** — rejected: see *Decisions*; the tools would leave `format.sh`'s default after a Roslyn upgrade.
- **Copy the newest supported version and the lowering into the fixture runner** — rejected: a second owner of a value the formatter owns, which drifts apart at the next upgrade.
- **Make the formatter's language-version resolution public** — rejected: it widens the shipped formatter assembly's surface for a repository tool, where internal visibility, as the CLI already has, suffices.
- **Read the version from the fixture directory or a project file** — rejected: `reihitsu-format` takes the version only from its argument (non-goal of the CLI spec), and a fixture directory has no project.
- **Print the version in the `Tracing` line or the fixture runner's header** — rejected: see *Decisions*.
