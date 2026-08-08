# Repository scripts

Cross-platform entry points for building, testing, formatting, and proving changes. Every script exists as a `.sh` and a `.ps1` variant with the same behavior, so the repository instructions, the `gh-*` workflow skills, and a developer shell all drive the same commands.

The Claude workflows run on Linux and use the `.sh` variants; the Codex workflows run on Windows and use the `.ps1` variants. Both are maintained together — a change to one belongs in the other.

Each script resolves the .NET SDK itself through `lib/dotnet-env.sh` / `lib/dotnet-env.ps1`: it probes `dotnet --list-sdks` first and installs the required SDK into `$HOME/.dotnet` **only** when it is missing. On an image that already ships the SDK, that step is a no-op and nothing about the environment changes. Pass `--no-install` / `-NoInstall` to turn a missing SDK into a failure instead of an installation — the Codex environments require that, because they must not change the environment at all.

Paths are resolved against **your** working directory, not the repository root: the scripts address the solution, the test projects, and the CLI by absolute path instead of changing directory, so `scripts/format.sh Foo.cs` from a subdirectory means the `Foo.cs` next to you.

| Script | Purpose |
|---|---|
| `prepare.sh` / `prepare.ps1` | Verify the toolchain, installing the SDK only if needed |
| `build.sh` / `build.ps1` | `dotnet build Reihitsu.sln -c Release` |
| `test.sh` / `test.ps1` | Run one or all test projects, with an optional focused filter |
| `format.sh` / `format.ps1` | Format the given paths through `Reihitsu.Cli` |
| `trace.sh` / `trace.ps1` | Trace source changes at each top-level formatter phase without modifying the input |
| `verify-text-only.sh` / `verify-text-only.ps1` | Prove that a change carries no compiled behavior |
| `clean-bin-obj.ps1` | Remove all `bin` and `obj` directories |
| `install-cli.ps1` | Install or update the locally built `Reihitsu.Cli` global tool |
| `upload-issues.ps1` | User-owned issue upload tool — the `gh-*` workflows never invoke it |

## Examples

```bash
scripts/prepare.sh
scripts/build.sh
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH3204"
scripts/format.sh Reihitsu.Formatter/Pipeline/LineBreaks/LineBreakDetection.cs
scripts/trace.sh samples/Example.cs --passes 3 --no-install
scripts/verify-text-only.sh --base a1b2c3d --head worktree --strict-docs
```

```powershell
.\scripts\prepare.ps1
.\scripts\build.ps1
.\scripts\test.ps1 -Project analyzer -Filter "FullyQualifiedName~RH3204"
.\scripts\format.ps1 Reihitsu.Formatter\Pipeline\LineBreaks\LineBreakDetection.cs
.\scripts\trace.ps1 samples\Example.cs -Passes 3 -NoInstall
.\scripts\verify-text-only.ps1 -Base a1b2c3d -Head worktree
```

## Formatter pipeline trace

`trace.sh <file> [--passes N] [--no-install]` and `trace.ps1 <file> [-Passes N] [-NoInstall]` run one C# file through the twelve top-level `IFormattingPhase` boundaries. Paths are resolved relative to the caller, including when the wrapper is invoked outside the repository root. The input file is only read: each pass operates in memory, and the complete result is reparsed before the next pass to match repeated CLI invocations.

The trace defaults to at most three passes and requires a positive pass count. It prints a pass/phase header only when that phase changes serialized source. Line-content changes get a unified diff; when a change only normalizes line-ending separators, the shared diff generator has no line hunk, so the trace prints before/after CRLF, LF, and CR counts instead. It preserves the input's detected LF or CRLF convention and stops early after the first complete pass that makes no change. Syntax-invalid and generated inputs are reported as legitimate skips.

Exit codes are `0` when formatting stabilizes or the input is legitimately skipped, `1` when the source is still changing after the requested pass count, and `2` for invalid arguments, missing files, unavailable tooling, or execution failures. Execution errors identify the active pass and, when available, the active phase.

The implementation lives in `trace/trace.cs` as a [.NET 10 file-based app](https://learn.microsoft.com/en-us/dotnet/core/sdk/file-based-apps) with a repository-only project reference to `Reihitsu.Cli`. Like the text-only proof app, it is outside `Reihitsu.sln`; its local `Directory.Build.props` isolates it from solution packaging and analyzer settings.

## The text-only proof

`verify-text-only` answers one question mechanically: **can this change affect compiled behavior?** The `gh-*` workflows use it to decide whether an expensive preflight audit is required and to prove that a comment-only cleanup after a passing audit did not invalidate it.

It is syntax-aware rather than line-based, because a `grep` over the diff does not recognize every block-comment form and cannot tell a comment apart from a directive or from comment-looking text inside a string literal. For every changed C# file it parses both versions with Roslyn and proves that

- the non-trivia token stream is unchanged — which also covers string and character literal contents, since those are token text;
- no directive, disabled-text, or skipped-token structure changed;
- every changed syntax element belongs to an allowed comment, documentation, or layout trivia category;
- both versions parse without errors, so a malformed comment cannot slip through.

Generated XML-documentation impact is **reported separately** instead of folded into the verdict, because a documentation edit is exactly what a comment-only change is allowed to be. When the caller must also exclude public API documentation — `gh-preflight`'s `PASS — non-blocking cleanup` does — pass `--strict-docs` / `-StrictDocs`; a plain exit code `0` does not establish it. Interface members and enum members count as public even though they carry no accessibility modifier.

A file that is neither C# nor a known non-compiled text path (Markdown, `.claude/**`, `.codex/**`, `documentation/**`, issue and PR templates) is reported as carrying behavior. `scripts/**` and `.github/workflows/**` are behavioral regardless of extension: nothing there compiles, but everything there executes. Added, deleted, and renamed files are never text-only.

Two things about `--head worktree`. Both sides are compared with normalized line endings, because git rewrites line endings on checkout under `core.autocrlf` and a raw comparison would otherwise report every multi-line literal and disabled-text region as changed on Windows; a byte order mark is stripped from both sides for the same reason. And untracked files are ignored with a note rather than counted as additions — they are not part of the tree that will merge, so a stray scratch file must not decide the verdict.

Exit codes: `0` proven text-only, `1` not proven, `2` tool or setup error. The single `TEXT-ONLY PROOF: …` line is the quotable evidence the workflows record.

The proof itself lives in `proof/verify-text-only.cs` as a standalone [file-based app](https://learn.microsoft.com/dotnet/core/sdk/file-based-programs). It is deliberately outside `Reihitsu.sln`: it ships with no package, is not built by CI, and its local `Directory.Build.props` keeps the solution's versioning, ruleset, and self-hosting analyzer references out of it.

One consequence is worth knowing: its leading `#:package` directive is not C# the Roslyn parser accepts, so `Reihitsu.Cli` reports the file as syntax-invalid and leaves it untouched, and neither self-hosting test sees it. Keep it to the repository's style by hand.
