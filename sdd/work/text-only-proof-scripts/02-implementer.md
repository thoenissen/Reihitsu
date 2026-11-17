Role: implementer
Commit: (this commit; see `git log -1 -- sdd/work/text-only-proof-scripts/02-implementer.md`)
Verdict: tests pass

Prompt note: the prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the lead's recorded decisions, so there was nothing to ignore.

## thoenissen/Reihitsu#1808

### Changes
| File | What changed | Requirement |
|---|---|---|
| `scripts/proof/verify-text-only.cs` | `IsNonCompiledPath` now checks for `.md` first. Markdown is text wherever it lives. The behavioral prefixes (`scripts/`, `.github/workflows/`) apply only after that check, and the non-compiled prefixes come last. The inline comment says the new reason. The `_behavioralPrefixes` summary now says "contents other than Markdown". | decision 1 |
| `scripts/README.md`, *The text-only proof* | Says that Markdown is text wherever it lives, `scripts/**` included. Every other file under `scripts/**` and `.github/workflows/**` stays behavioral. The lane check still classifies `scripts/**`, Markdown included, as tooling. | decision 1 |

My reading of decision 1: "every other file" means every non-Markdown file. Markdown under `.github/workflows/` therefore also counts as text. No such file exists (`git ls-files '.github/workflows/*'` lists five `.yml` files), and GitHub runs only `.yml`/`.yaml` from there. If the lead wants the exception limited to `scripts/` only, the check needs a prefix condition. See *Disputed tests or scope*.

### Manual verdicts (the proof has no tests)
Method: I made one throwaway `git worktree` per side under the scratchpad. In each, I made one uncommitted edit per scenario and ran `scripts/verify-text-only.sh --base HEAD --head worktree`, which is the proof of that worktree. "Before" is HEAD `7efb090` (pin 5.6.0, old classification). "After" is the same HEAD plus this change's `verify-text-only.cs`, committed temporarily in the detached worktree as `5dcf6a4`. Both worktrees are removed.

| Scenario | Before | After |
|---|---|---|
| A: prose edit in `scripts/README.md` only | exit 1, `FAIL — 1 of 1 changed file(s) carry compiled behavior` (`BEHAVIORAL scripts/README.md — not a C# file and not a known non-compiled text path`) | exit 0, `PASS — 1 changed file(s) (1 non-compiled, 0 C# comment/layout-only)` |
| A2: `scripts/README.md` + `documentation/rules/general-notes.md` | exit 1, `FAIL — 1 of 2` (scripts/README.md) | exit 0, `PASS — 2 changed file(s) (2 non-compiled, …)` |
| H: added `scripts/NOTES.md` (intent-to-add) | not run (behavioral by prefix) | exit 0, `PASS — 1 changed file(s) (1 non-compiled, …)` |
| B: comment edit in `scripts/build.sh` | exit 1, `FAIL — 1 of 1` (`BEHAVIORAL scripts/build.sh`) | exit 1, same |
| E: comment line added to `.github/workflows/build.yml` | exit 1, `FAIL — 1 of 1` | exit 1, same |
| I: `scripts/README.md` + `scripts/build.sh` | not run | exit 1, `FAIL — 1 of 2` (only `scripts/build.sh` behavioral) |
| F: comment edit in `scripts/proof/verify-text-only.cs` | exit 1, `BEHAVIORAL … — the base version does not parse` | exit 1, same |

In short: Markdown under `scripts/` is now proven text-only. A script, a workflow, or a C# file-based program under `scripts/` stays behavioral.

## thoenissen/Reihitsu#1817

### Changes
| File | What changed | Requirement |
|---|---|---|
| `scripts/proof/verify-text-only.cs` line 1 | `#:package Microsoft.CodeAnalysis.CSharp@5.6.0` → `@5.12.0` (= `RoslynVersion` in `Directory.Build.props`) | decision 2 |
| `src/Reihitsu.ArchitectureTests/General/RoslynVersionOwnership.cs` | `Check` now also reads every `.cs` file under the same skip rule (`bin`, `obj`, dot-directories). New `ReadPackageDirectives` parses a file with Roslyn only when its text contains `#:`. It reads the `IgnoredDirectiveTriviaSyntax` directives in the leading trivia of the first token, so only directives in front of the first token count and a string literal never does. It records each `#:package <id>[@<version>]` whose id passes the existing `IsRoslynPackage`. New `CheckPackageDirectives` compares each version, ordinal, with the trimmed value of the single root `RoslynVersion` definition. A missing version (decision 4) is a violation. Message: `<file>:<line>: #:package <id as written> must state exactly @<version>, the value of RoslynVersion, as its version`. When there is not exactly one root definition, it skips the comparison, because `CheckDefinitions` already reports that. The class summary and the summary of `Check` describe the directives. | decision 2, decision 4 |
| `scripts/README.md`, *The text-only proof* | The paragraph on the proof's file-based app now says the pin equals `RoslynVersion` and is written out because a file-based program cannot read the property. It also says the Roslyn-version guard fails on drift. | decision 2 |

I chose Roslyn over a line scan because the test project already references `Microsoft.CodeAnalysis` at `$(RoslynVersion)`. Roslyn decides "in front of the first token" exactly, including comments and block comments, and the `#:` pre-filter keeps the repository-wide walk cheap. I checked in a probe that Roslyn 5.12 returns `#:package …` as `IgnoredDirectiveTrivia` with a `Content` token `package <id>@<version>`, also behind a shebang, a `//` comment, or a `/* */` comment.

### Proof verdicts before and after the pin change
Same method and worktrees as for #1808. "Before" uses the 5.6.0 pin and "after" the 5.12.0 pin.

| Scenario | Before (5.6.0) | After (5.12.0) |
|---|---|---|
| C: text-only C# change (doc comment `<returns>` reworded in `src/Reihitsu.Core/CasingUtilities.cs`) | exit 0, `PASS — 1 changed file(s) (0 non-compiled, 1 C# comment/layout-only); xml-doc impact: 1 file(s)` | exit 0, identical |
| D: non-text-only C# change (`return false;` → `return true;` in the same file) | exit 1, `FAIL — 1 of 1 …` (`token stream differs (at index 42: '8324:false' became '8323:true')`) | exit 1, identical |
| G: pin change in `scripts/proof/verify-text-only.cs` (`@5.12.0` → `@5.11.0`) | n/a | exit 1, `the base version does not parse` |

The verdicts are unchanged. Also, the proof builds and runs with 5.12.0, because every "after" run above used it.

Guard drift check: I put the pin back to 5.6.0 temporarily and ran `EveryRoslynPackageItemUsesTheRepositoryVersionProperty`. It failed (`Assert.IsEmpty(violations)`). With 5.12.0 restored, it passes.

## Whole run

### Policy owners
- `rg -n "_behavioralPrefixes|IsNonCompiledPath|_nonCompiledPrefixes" scripts src`: the only owner is `scripts/proof/verify-text-only.cs`. `LaneClassifier` holds the separate lane classification, and decision 1 leaves it unchanged.
- `rg -n "IsRoslynPackage|RoslynVersion" src --glob '*.cs'`: the only owner is `RoslynVersionOwnership`. The directive check reuses its `IsRoslynPackage`, `IsSkipped`, and `VersionPropertyName`.

### Production diff size
`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'` is empty: no production change. Tooling and test path: `scripts/proof/verify-text-only.cs`, `scripts/README.md`, and `src/Reihitsu.ArchitectureTests/General/RoslynVersionOwnership.cs` (3 files, +123/−11 before commit). No new type.

### Deleted special cases
_N/A._

### Tests run
- `scripts/test.sh --configuration Debug --project architecture --filter "FullyQualifiedName~Reihitsu.ArchitectureTests.General.RoslynPackageVersionTests"`: `Passed! - Failed: 0, Passed: 36, Skipped: 0, Total: 36`
- `scripts/test.sh --configuration Debug --project architecture`: `Passed! - Failed: 0, Passed: 195, Skipped: 0, Total: 195`
- `scripts/lane-check.sh tooling --head worktree`: `LANE CHECK: confirmed — claimed tooling, diff needs tooling`

### Disputed tests or scope
- Adjacent fix, one sentence: the old README sentence "Added, deleted, and renamed files are never text-only" was already wrong before this run. Before and after, `Verify` returns for a non-C# file before it looks at the status. An added `documentation/NOTES.md` passed on the old head (`text-only documentation/NOTES.md — non-compiled text file`), and so does an added `scripts/NOTES.md` now. I rewrote the paragraph and stated what the code does: "A renamed file, and an added or deleted C# file, is never text-only". I did not change the code. If the lead wants added or deleted non-C# files to be behavioral, that is a separate decision.
- Markdown under `.github/workflows/` also counts as text (see #1808). Limiting the exception to `scripts/` would need one more prefix condition.
- Decision 4 (version-less directive) is implemented, but no test covers it. `string.Equals(null, version)` is false, so it is reported. The test implementer may want a test for it.
- The summary of the test class `RoslynPackageVersionTests` (test implementer's file) still describes only build files. I left it alone because it is not mine. It is for the cleaner or the test implementer.

### Architecture findings
_None._

### Follow-up candidates
- Every C# file under `scripts/` is a file-based program with `#:` directives. The proof parses with default `CSharpParseOptions` (no `FileBasedProgram` feature), so such a file "does not parse", and any change to it is behavioral (scenarios F and G). The result is right, but the reason is wrong. A C# file under `scripts/` without directives would go through the comment-only proof, which contradicts "every other file under `scripts/**` is behavioral". It would cost one cheap check: put the behavioral-prefix check before the C# branch, except for `.md`. The lane check does not depend on it, because `scripts/**` is tooling there anyway.
- The comment on `RoslynVersion` in `Directory.Build.props` ("every non-Testing Microsoft.CodeAnalysis* package reference uses") does not mention `#:package` pins. It is not changed here, because it is a root build file.
