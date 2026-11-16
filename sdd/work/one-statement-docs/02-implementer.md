Role: implementer
Commit: 3b4ac9f
Verdict: tests pass

No intended fix, suspected cause, or hoped-for verdict was carried in the prompt beyond the instruction to report an unnecessary goal with evidence.

## thoenissen/Reihitsu#1470

### Changes
| File | What changed | Requirement |
|---|---|---|
| `scripts/README.md` | Section *The self-hosting scans* renamed to *The default test run*; it now states the set once (every solution test project except `Reihitsu.SelfHosting.Test`, defined by the `all)` branch and held by `TestScriptProjectSelectionTests`) and says it is the one statement. The `test.sh` table row links to it; the sentence "Both `Reihitsu.ArchitectureTests` and `Reihitsu.Tooling.Test` participate in the default `all` run" under the code-fix runner was dropped as a second statement | — (tool docs) |
| `scripts/test.sh` | Header comment no longer lists coverage; it points at the `all)` branch and `scripts/README.md`, *The default test run* | — |
| `AGENTS.md`, `CONTRIBUTING.md` | `scripts/test.sh` comment: "the default test run; scripts/README.md states what it covers" | — |
| `squad/README.md` | Validator row: "runs the default test set" with a link to the README section | — |
| `squad/roles/validator.md` | Coverage sentence replaced by a link; keeps "you add none to it" | — |
| `squad/roles/implementer.md` | Step 6: a project the default run leaves out (link) is not part of a local run; no project named | — |
| `squad/roles/auditor.md` | "the validator runs the default set (link), and CI runs the rest" — not listed by the issue, but it restated the split the same way | — |
| `.claude/agents/validator.md` | Description: "runs the default test set (`scripts/README.md`)" | — |

`rg -n -i "default (set|run|test)|self-hosting scan|except the self|SelfHosting" --glob '*.md'` outside `sdd/changes/`, `sdd/work/`, `squad/notes/` now finds coverage claims only in `scripts/README.md`. Kept: `squad/roles/test-implementer.md:55` (says `FormatterSelfHostingTests` runs in CI; a statement about that project, not about the default run) and `sdd/README.md:80` (where the `[Requirement]` attribute is declared).

The README claim "every test project of the solution except the scans" was checked against `TestScriptProjectSelectionTests.ShellAllIncludesEverySolutionTestProjectExceptSelfHosting`, which asserts exactly that.

### Policy owners
The set's owner is the `all)` branch of `scripts/test.sh` (`rg -n "all\)" scripts/test.sh`), guarded by `src/Reihitsu.ArchitectureTests/General/TestScriptProjectSelectionTests.cs`. Unchanged.

### Disputed tests or scope
_None._

## thoenissen/Reihitsu#1471

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Tooling/LaneClassifier.cs` | New private `IsRootMarkdownText`: a path without `/` ending in `.md` (case-insensitive) that is not one of `_toolingRootFiles` is text. `README.MD` and `CONTRIBUTING.md` removed from `_textFiles`, which the new rule covers; `_textFiles` keeps `LICENSE` and the PR template | — (tool) |
| `scripts/README.md`, *The lane check* | Text row: "every Markdown file at the repository root except `AGENTS.md` and `CLAUDE.md`" replaces the two names; Markdown and proven C# under `src/` now say "outside `src/Reihitsu.Tooling/`", which is what the code does (tooling prefixes are checked before `ClassifySourcePath`). New sentence: the table is the one statement, and a path belongs to the first row that names it (so `squad/notes/**` is text, the rest of `squad/**`, Markdown included, tooling) | — |
| `squad/README.md`, *Lanes* | "What the lane check derives from the paths" no longer lists paths: it names the five categories, links to the `scripts/README.md` table as the one statement, and states the lanes in terms of categories. The old Docs bullet ("Markdown") contradicted the classifier for `scripts/`, `squad/`, `.claude/` | — |

Rule decided: root Markdown is text, except the agent instructions `AGENTS.md` and `CLAUDE.md`, which stay tooling; Markdown under `scripts/`, `squad/` (outside `squad/notes/`) and `.claude/` stays tooling, as existing tests assert. This keeps every existing classification and generalizes the two named root files, so a future root Markdown file needs no code change. The "first row wins" ordering in the README was checked against the code order: change doc, text (prefixes, files, root Markdown), tooling (prefixes, root files, extensions), then `src/` (Markdown/proven C# text, test project, production).

### Policy owners
`rg -n "_textFiles|_toolingRootFiles|_textPrefixes|_toolingPrefixes" src` — only `src/Reihitsu.Tooling/LaneClassifier.cs`. No other classifier of paths into lanes exists.

### Production diff size
`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'`: 1 file changed, 17 insertions(+), 3 deletions(-). No new type; one new private method.

### Disputed tests or scope
_None._

## thoenissen/Reihitsu#1479

### Changes
| File | What changed | Requirement |
|---|---|---|
| `squad/failure-classes.md`, *Tab width across formatter phases* | Intro links `sdd/changes/2026-10/phase-prediction-removal.md`. Decision rewritten for the current pipeline: the preparation phase's tab replacement (`PreparationPhase/TabReplacement`, FMT-TAB-001, FMT-TAB-002) runs first, no later phase counts a tab (FMT-PIPE-019), `IndentationPhase` moves raw-string lines (FMT-RAW-001, FMT-RAW-007); the places a tab still reaches a column are a tab kept inside a comment/literal/disabled text (FMT-TAB-003) and the whitespace in front of a node-level target, counted as one column (FMT-PIPE-017, FMT-PIPE-018). Check names FMT-INDENT-029 and FMT-PIPE-017 next to FMT-RAW-007. New axis *Entry point* (the node-level member the issue asked to keep); new member of *What the tab stands in front of* and *Anchoring* for the missing case. The failing-cells table gains an *Entry point* column and a fifth row for `var x = a→/** c */.B()` / `.C();` (26 then 29) and the raw-string variant (34 then 37), from the change doc's table; the first four rows are labelled as found against the old `CleanupPhase`/`RawStringAlignmentPhase` split | — (squad text) |

Retired IDs no longer named: FMT-CLEAN-002 (retired, spec says FMT-TAB-001 states the rule) and FMT-PIPE-016 (retired in favour of FMT-PIPE-019). `RawStringAlignmentPhase` and `CleanupPhase` are named only as history. The cited test `TabReplacementTests` (`src/Reihitsu.Formatter.Test/Regression/TabReplacement/TabReplacementTests.cs`) holds `ChainLinkBehindTabAndDelimitedDocumentationCommentAlignsInOnePass`, `ChainLinkBehindRawStringBehindTabAndDelimitedDocumentationCommentAlignsInOnePass`, `FormatNodeAlignsChainLinkBehindTab...` and `FormatNodeInDocumentAlignsChainLinkBehindTab...`, with `.C()` expected in column 29.

### Disputed tests or scope
_None._

## thoenissen/Reihitsu#1544

### Changes
| File | What changed | Requirement |
|---|---|---|
| `AGENTS.md`, `BlankLinePhase` row | Guard column now names `` `BraceBlankLinePolicy` `` (the blank lines next to braces, brackets, pattern parentheses, and the comments glued to them) | — |

The `LineBreakPhase` goal is already met on 3f9f140: the row says "it keeps every blank line" (issue comment of 2026-11-16). Not changed; that goal is unnecessary in this run. `BraceBlankLinePolicy` is `src/Reihitsu.Formatter/Pipeline/BlankLines/Utilities/BraceBlankLinePolicy.cs` and is a code owner of `sdd/specs/formatter/blank-lines.md`.

### Disputed tests or scope
_None._

## Whole run

### Deleted special cases
_N/A._

### Tests run
```shell
scripts/build.sh --configuration Debug                                     # 0 Error(s)
scripts/test.sh --configuration Debug --no-build --project tooling         # Passed! Failed: 0, Passed: 133
scripts/test.sh --configuration Debug --no-build --project architecture    # Passed! Failed: 0, Passed: 119
scripts/format.sh src/Reihitsu.Tooling/LaneClassifier.cs                   # Formatted 0 of 1 file(s)
bash -n scripts/test.sh                                                    # ok
scripts/lane-check.sh tooling --head worktree                              # LANE CHECK: confirmed — claimed tooling, diff needs tooling
```
The seven tooling rows the test implementer handed over as failing now pass; `PhaseOwnershipDocumentationTests` (incl. `OwnershipMapGuardNamesResolveToDeclarations`) passes with the new guard name.

### Architecture findings
_None._

### Follow-up candidates
_None._
