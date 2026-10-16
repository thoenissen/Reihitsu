# CLI — reihitsu-format

| | |
|---|---|
| ID prefix | CLI |
| Status | Active |
| Code owners | `Program`, `ParseResult`, `FormatCommandHandler`, `ExitCodes`, `GeneratedFileUtilities`, `FileProcessResult`, `DefaultFileSystem`, `DefaultConsoleInput`, `DefaultSourceFormatter`, `DiffGenerator` |
| Uses | FMT-PIPE-005, FMT-PIPE-006, FMT-PIPE-008 |

This spec covers the `reihitsu-format` .NET tool: its arguments, which files it selects and skips, how it reads and writes them, the `--check` and `--dry-run` modes, its console output, and its exit codes. What the formatter does to a file's content is specified under `formatter/`.

## Non-goals

- The formatting itself — FMT-PIPE and the phase specs.
- The pipeline trace (`scripts/trace.sh`), which reuses internal CLI types but is a repository tool, not part of `reihitsu-format`.
- Reading `LangVersion`, `.editorconfig`, or `reihitsu.json` from the project. The tool takes the language version only from its argument.
- The exact layout of the usage text and the version string beyond what the requirements quote.
- The header of the `--dry-run` diff, which currently prefixes the full path with `a/` and `b/` (`--- a//work/src/A.cs`); whether that is intended is an open question.

## Requirements

Paths in the example outputs are shortened to `/work/…`; the tool prints full paths.

### CLI-001 — Arguments are options and paths

The CLI MUST accept the options `--check`, `--dry-run`, `--verbose`, `--force`, `--utf8-bom`, `--lang-version <version>`, `--help` (and `-h`), and `--version`, in any order and mixed with paths. Every other argument that starts with `-` MUST be an unknown option, unless it follows the argument `--`, after which every argument is a path. Without a path, the current directory MUST be used.

Why: Paths that start with `-` exist; `--` is the conventional way to pass them without changing how options are recognized.

Example:

```shell
reihitsu-format --check -- -odd
```

checks the directory `-odd`, while `reihitsu-format --check -odd` fails with `Unknown option: -odd`.

### CLI-002 — Invalid arguments fail before any file is read

The CLI MUST exit with code 2 and touch no file when an argument is invalid, writing the message and then the usage text to standard error:

- an unknown option: `Unknown option: <option>` (the first one);
- a `--lang-version` value that is not a C# language version: `Invalid value for --lang-version: '<value>'. Expected a C# language version such as 12, 12.0, latest, or preview.`;
- `--lang-version` as the last argument: `Missing value for --lang-version.`;
- `--check` together with `--dry-run`: `Options --check and --dry-run cannot be combined.`

These errors MUST take precedence over `--help` and `--version`.

Why: A mistyped option in CI must not silently turn into a formatting run that writes files, and must not be hidden by a help request in the same command line.

Example:

```shell
reihitsu-format --bogus --help
```

```text
Unknown option: --bogus
reihitsu-format [options] [--] [<paths>...]
…
```

exit code 2.

### CLI-003 — Help and version

With `--help` or `-h`, the CLI MUST print the usage text to standard output and exit with code 0 without processing files. With `--version`, it MUST print `reihitsu-format <informational version>` and exit with code 0. When both are given, help wins.

Why: Both are questions about the tool, not requests to format.

### CLI-004 — The language version comes from `--lang-version`

The CLI MUST parse every file with the language version given by `--lang-version`, or with the newest version the formatter supports when the option is absent or `latest`. A value newer than that, such as `preview`, MUST be lowered to it, with one warning on standard error: `Warning: C# language version '<value>' is newer than the newest supported version <version>; formatting for <version>.` Version-dependent formatting follows FMT-PIPE-008.

Why: Files are parsed without a project, so the tool cannot know the project's language version. Lowering an unsupported version keeps the run working and says so instead of failing.

Example:

```shell
reihitsu-format --check --lang-version preview src/B.cs
```

```text
Warning: C# language version 'preview' is newer than the newest supported version 14.0; formatting for 14.0.

0 of 1 file(s) need formatting.
```

exit code 0.

### CLI-005 — Every given path must exist

Before selecting any file, the CLI MUST check that every given path is an existing file or directory. If one is not, it MUST write `Path not found: <path>` to standard error and exit with code 2 without processing any file.

Why: A typo in one path of a CI command must fail the run instead of checking fewer files than intended.

Example:

```shell
reihitsu-format --check src/A.cs nope
```

```text
Path not found: nope
```

exit code 2.

### CLI-006 — File selection

The CLI MUST select a given file when its name ends with `.cs` (case-insensitively) and ignore any other given file. For a given directory, it MUST select every `*.cs` file below it, recursively, except files below a directory named `bin` or `obj` inside it; the names are compared with the platform's path case sensitivity, and the given directory itself is never excluded, even when it is named `bin` or `obj`. A file selected more than once — given directly and through a directory — MUST be processed once. When nothing is selected, the CLI MUST print `No .cs files found.` and exit with code 0.

Why: Build output contains generated copies and intermediate files that are not the user's source. A directory the user names explicitly is an explicit choice and is honored.

Example — given `src/` with `A.cs`, `sub/C.cs`, `bin/D.cs`, and `obj/x/E.cs`, the CLI processes `A.cs` and `sub/C.cs`; given `bin/` directly, it processes `bin/G.cs`. On Linux, `src/Bin/F.cs` is processed; on Windows it is excluded.

### CLI-007 — Generated files are skipped

The CLI MUST skip, without reading it for formatting, a selected file whose name ends with `.Designer.cs`, `.g.cs`, or `.g.i.cs` (case-insensitively), even when it was given explicitly. It MUST also skip a file the formatter recognizes as generated by its header (FMT-PIPE-006). Skipped generated files are counted in the summary and are not part of the file total.

Why: Generated code is overwritten by its generator; formatting it produces churn and diffs nobody reviews.

Example:

```shell
reihitsu-format --check src/H.g.cs
```

```text

0 of 0 file(s) need formatting.
Skipped 1 generated file(s).
```

exit code 0.

### CLI-008 — Files with syntax errors are left alone

The CLI MUST leave a file whose parse has errors unchanged, count it as skipped in the summary, and include it in the file total (FMT-PIPE-005). A skipped file MUST NOT affect the exit code.

Why: The formatter cannot guarantee to preserve the meaning of code it cannot parse; the compiler reports the error.

### CLI-009 — Encoding is detected and kept

The CLI MUST detect a file's encoding from its byte order mark — UTF-8, UTF-16 little or big endian, UTF-32 little or big endian — and read it as UTF-8 without a byte order mark otherwise. Invalid bytes for that encoding MUST make the CLI skip the file, write `Skipped (could not decode): <path>` to standard error, count it in the summary, and leave the exit code unaffected. A written file MUST keep its encoding and its byte order mark, or lack of one.

Why: Rewriting a file in another encoding changes every byte for tools that compare files; persisting replacement characters for undecodable bytes would destroy content silently.

Example — a UTF-16 LE file with byte order mark is written back as UTF-16 LE with byte order mark.

### CLI-010 — `--utf8-bom` normalizes the encoding

With `--utf8-bom`, the CLI MUST write every processed file as UTF-8 with a byte order mark, and a file whose content is already formatted but whose encoding differs MUST count as changed — written in write mode, reported in `--check` and `--dry-run` (without a diff, since the text does not change).

Why: A team that standardizes on UTF-8 with BOM needs CI to catch files that drifted, not only content changes.

Example — `n.cs` is formatted UTF-8 without a byte order mark:

```shell
reihitsu-format --check --utf8-bom n.cs
```

```text
Not formatted: /work/n.cs

1 of 1 file(s) need formatting.
```

exit code 1.

### CLI-011 — Write mode writes only changed files

Without `--check` and `--dry-run`, the CLI MUST write a file only when its formatted text or, with `--utf8-bom`, its encoding differs, print `Formatted: <path>` for it, and finish with the summary `Formatted <changed> of <total> file(s).`

Why: Leaving unchanged files untouched keeps their timestamps, so incremental builds and file watchers do not react.

Example:

```shell
reihitsu-format w/A.cs w/B.cs
```

```text
Formatted: /work/w/A.cs

Formatted 1 of 2 file(s).
```

exit code 0.

### CLI-012 — `--check` reports without writing

With `--check`, the CLI MUST NOT write any file. It MUST print `Not formatted: <path>` for each file that would change, finish with `<changed> of <total> file(s) need formatting.`, and exit with code 1 when at least one file would change.

Why: CI needs a yes/no answer and the list of offending files, without modifying the checkout.

Example — given `src/` with an unformatted `A.cs` and `sub/C.cs`, a formatted `B.cs`, a file with a syntax error, and three generated files:

```text
Not formatted: /work/src/A.cs
Not formatted: /work/src/sub/C.cs

2 of 4 file(s) need formatting.
Skipped 3 generated file(s).
Skipped 1 file(s) with syntax errors.
```

exit code 1.

### CLI-013 — `--dry-run` shows the diff without writing

With `--dry-run`, the CLI MUST NOT write any file. For each file that would change, it MUST print `Would format: <path>` followed by a unified diff of the text change, finish with `<changed> of <total> file(s) would be formatted.`, and exit with code 1 when at least one file would change.

Why: The diff lets a user review what the formatter would do before letting it write.

Example:

```shell
reihitsu-format --dry-run src/A.cs
```

```text
Would format: /work/src/A.cs
--- a//work/src/A.cs
+++ b//work/src/A.cs
@@ -1,4 +1,4 @@
 class A
 {
-    int  x;
-}
+    int x;
+}
\ No newline at end of file


1 of 1 file(s) would be formatted.
```

exit code 1. The removed final line break is FMT-CLEAN's.

### CLI-014 — The summary names every skipped group

After processing, the CLI MUST print an empty line, the mode's summary line (CLI-011 to CLI-013), and then, each only when non-zero: `Skipped <n> generated file(s).`, `Skipped <n> file(s) with syntax errors.`, `Skipped <n> file(s) that could not be decoded.`, and `<n> file(s) encountered errors.`

Why: A run that formatted fewer files than expected must say why.

### CLI-015 — `--verbose` reports every file

With `--verbose`, the CLI MUST additionally print one line for every file it does not change: `Unchanged: <path>`, `Skipped (generated): <path>`, or `Skipped (syntax errors): <path>`.

Why: When a file is not formatted, the user needs to know whether it was already clean or skipped.

### CLI-016 — A failing file does not stop the run

When processing a file fails for any other reason, the CLI MUST write `Error processing <path>: <message>` to standard error, continue with the remaining files, count the file in the summary, and exit with code 2 at the end.

Why: One unreadable file should not hide the state of all others, but the run as a whole must not report success.

### CLI-017 — Exit codes

The CLI MUST exit with:

- 0 when the run succeeded — in write mode whether or not files changed; with `--check` or `--dry-run` when no file would change; also for `--help`, `--version`, no selected files, and a declined confirmation (CLI-018);
- 1 when `--check` or `--dry-run` found a file that would change and no file failed;
- 2 for invalid arguments (CLI-002), a missing path (CLI-005), a failed file (CLI-016), a large run that cannot be confirmed (CLI-018), cancellation with Ctrl+C (`Operation canceled.` on standard error), and any unexpected error (`Error: <message>`).

Why: CI distinguishes "the code needs formatting" from "the tool could not do its job"; only the second is a broken pipeline.

### CLI-018 — Large write runs need confirmation

In write mode without `--force`, when more than 25 selected files are neither generated by name nor by header, the CLI MUST ask `This run will format <n> file(s). Continue? [y/N]` before reading any file for formatting. `y` or `yes`, in any case and surrounded by whitespace, MUST continue; any other answer MUST print `Formatting aborted. No files were changed.` and exit with code 0. When standard input is redirected, the CLI MUST NOT prompt; it MUST write `This run would format <n> file(s), but standard input is not interactive, so the confirmation prompt cannot be answered. Use --force to format without confirmation.` to standard error and exit with code 2. `--check` and `--dry-run` never prompt.

Why: Running the tool in the wrong directory can rewrite a whole tree; the prompt is the last chance to stop. A non-interactive run cannot answer, so it fails instead of guessing.

Example — 26 unformatted files, standard input redirected:

```shell
reihitsu-format big/ < /dev/null
```

```text
This run would format 26 file(s), but standard input is not interactive, so the confirmation prompt cannot be answered. Use --force to format without confirmation.
```

exit code 2.
