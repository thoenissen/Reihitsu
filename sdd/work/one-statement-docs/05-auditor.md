Role: auditor
Commit: 62a2512269144e98111df933daa77a729feffb1a
Verdict: pass, text cleanup (#1470: pass; #1471: text cleanup, finding #1; #1479: pass; #1544: pass)

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Full audit. Head `62a2512` (checked out, working tree clean); `origin/main` and `git ls-remote origin refs/heads/main` both `372daeee7a757fe7a3026896a594ece4df03fa8a`, an ancestor of the head. The diff outside `sdd/work/` touches 14 files: `.claude/agents/validator.md`, `AGENTS.md`, `CONTRIBUTING.md`, `scripts/README.md`, `scripts/test.sh`, `squad/README.md`, `squad/failure-classes.md`, `squad/roles/{auditor,implementer,validator}.md`, `src/Reihitsu.Tooling/LaneClassifier.cs`, and three test files. That matches 01-test-implementer and 02-implementer. Cleaner build and focused test results are taken as reported. No test was re-run, because no finding depends on a test result.

## thoenissen/Reihitsu#1470

### Findings
_None._

### Checked
- `scripts/README.md`, *The default test run*, is now the only statement of what the run covers. `rg -i "self-?hosting|every test project|all test projects|default (set|run)"` over the repository, excluding `src/`, `sdd/work/`, `sdd/changes/` and `squad/notes/`, finds these other hits, and none of them states the default run's coverage:
  - the `--project` value list and the examples in `scripts/test.sh`;
  - `scripts/README.md:27`, an example;
  - `squad/roles/test-implementer.md:55`, a statement about `FormatterSelfHostingTests`;
  - `sdd/README.md:80`, which says where the attribute is declared;
  - comments in the `Directory.Build.props` files under `scripts/`.
- The README claim matches the `all)` branch in `scripts/test.sh:80-88`: seven projects, with `SelfHosting` left out. `TestScriptProjectSelectionTests` holds that branch.
- The old anchor `#the-self-hosting-scans` has no remaining reference. Every new link targets `#the-default-test-run`, which matches the renamed heading. The relative paths from `squad/`, `squad/roles/` and `.claude/agents/` resolve.
- Decision 3 (`squad/roles/auditor.md`) is in scope. Its "and CI runs the rest" makes no claim about the projects the run covers.

## thoenissen/Reihitsu#1471

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | high (documentation only, see Verdict) | `scripts/README.md:116`, Text row, together with the new sentence at `scripts/README.md:126`; `squad/README.md:40`, Docs bullet; `squad/roles/spec-author.md:12` | FC-016 | – | same requirement | The issue's Done-when says the documents "no longer state a classification that differs from the classifier's". Three members of that class still differ. **(a)** The Text row does not name `squad/notes/**`, so `squad/notes/` is named only by the Tooling row (`squad/**`). The new sentence still claims "A path belongs to the first row that names it, so Markdown under `squad/notes/` is text". The table gives no basis for that, and the sentence also narrows the rule to Markdown. Counterexample: `squad/notes/2026-11-16-x.md`, or any non-Markdown file under `squad/notes/`. Read by the table, it is Tooling. `LaneClassifier` says Text: `_textPrefixes` holds `"squad/notes/"` for every file, and the existing test row `squad/notes/README.md` asserts Text. **(b)** The Docs bullet in `squad/README.md` admits "C# files when `scripts/verify-text-only.sh` proves the whole diff text-only". Counterexample: a diff with only a comment edit in `src/Reihitsu.Tooling/LaneClassifier.cs`. The proof passes because the file is C# under `src/`, but the tooling prefix is checked first, so the classifier gives Tooling and the lane check requires `tooling`, not `docs`. The table qualifies the same rule with "outside `src/Reihitsu.Tooling/`", and the bullet does not. **(c)** This one is outside the files the issue lists, and it is the same restatement that decision 3 took in for #1470. `squad/roles/spec-author.md:12` says that in the docs lane the spec author writes "Markdown". Markdown under `squad/` (outside `notes/`), `scripts/` and `.claude/`, and `AGENTS.md`/`CLAUDE.md`, are classified Tooling, so a docs-lane edit of them fails the lane check. These members hold, traced against `ClassifyPath`: root Markdown other than `AGENTS.md`/`CLAUDE.md` is Text; `AGENTS.md`/`CLAUDE.md` are Tooling; Markdown under `src/` outside Tooling is Text; Markdown under `src/Reihitsu.Tooling/` is Tooling; issue and PR templates are Text (first-row rule ahead of `.github/**`); a test project's `.csproj` is Tooling (first-row rule ahead of Test); C# test files with the proof passed are Text; `LICENSE`, the `binaries/**` files and the solution files match the table. | Text only. (a) Add `squad/notes/**` to the Text row and write the sentence as "every file under `squad/notes/`". (b) Reword the Docs bullet so it states only categories ("text only"), or qualify the C# part as the table does. (c) Point spec-author.md at the text category of the `scripts/README.md` table instead of saying "Markdown". |

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Root Markdown is text (`IsRootMarkdownText`, `LaneClassifier.cs:221`) | Whole path. Three conditions: there is no `/`, the name ends in `.md` (case-insensitive), and the path is not in `_toolingRootFiles` | `_toolingRootFiles` in the tooling branch (`:63`), which reads the same array, so there is one owner of the exclusion. `ClassifySourcePath` `.md` check (`:236`), which covers a different span (under `src/`) | The exclusion and the tooling list agree by construction. Spans are disjoint: root versus `src/`. The "root" qualifier of the rule is an explicit condition (FC-002). Each condition is pinned on both sides. Without the `/` check, the existing rows `scripts/README.md`, `squad/lead.md` and `.claude/agents/auditor.md` would turn Text, because the text branch runs before the tooling prefixes. Without the exclusion, the rows `AGENTS.md`/`CLAUDE.md` would turn Text. Without the `.md` check, `notes.txt` would turn Text | ok |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Path → lane category | `rg -n "_textFiles\|_toolingRootFiles\|_textPrefixes\|_toolingPrefixes" src` finds only `src/Reihitsu.Tooling/LaneClassifier.cs`. The prose restatements are finding #1 | one code owner; prose drift is #1 |
| Which non-C# paths count as text for the text-only proof (`scripts/README.md:97`, `verify-text-only`) | A separate decision: what counts as a behavior change in the proof, not which lane a path belongs to | not this policy; unchanged |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `LaneClassifierTests.ClassifyPathTreatsTheRootCommunityFilesAsText` (4 rows) | `CODE_OF_CONDUCT.md` and `SECURITY.md` are Text, whether or not the proof passed | Any other category | `ClassifyPath` directly | adequate |
| `LaneClassifierTests.ClassifyPathClassifiesEveryRootMarkdownFile` | No root `*.md` is Unknown | One root Markdown file classified Unknown | `Directory.GetFiles` at the root found from `Reihitsu.sln` | adequate for the Done-when "every root Markdown file is classified". It reads the file system, so untracked files are included too, which only makes it stricter. |
| `LaneCheckCommandTests.ExecuteConfirmsTheDocsLaneForARootCommunityFile` (2 rows) | A diff of only that file passes a `docs` claim | Non-success exit code, a missing `Text` line, or a different verdict line | `LaneCheckCommand.Execute` with a name-status diff (the real path) | adequate (FC-033: the scenario is not normalized away) |

## thoenissen/Reihitsu#1479

### Findings
_None._

### Checked
I checked every requirement the section now cites against its heading and text in `sdd/specs/`:
- FMT-TAB-001, FMT-TAB-002 and FMT-TAB-003. FMT-TAB-003 covers comments, literals and disabled text.
- FMT-PIPE-017. Its text says "a tab in the whitespace in front of the node … counts as one column".
- FMT-PIPE-018, FMT-PIPE-019 and FMT-PIPE-004.
- FMT-INDENT-029 (chain links).
- FMT-RAW-001 and FMT-RAW-007.
- SHARED-FIX-007.

The fifth row's cells (26 then 29, 34 then 37) match `sdd/changes/2026-10/phase-prediction-removal.md:38-39`. The four `TabReplacementTests` methods the row cites exist at lines 42, 76, 243 and 266. The retired FMT-CLEAN-002 and FMT-PIPE-016 no longer appear as current, and `CleanupPhase`/`RawStringAlignmentPhase` are named only as history. The missing member is in the axes and in the table. The *Entry point* axis keeps the FMT-PIPE-017 guidance that the issue asked to keep.

## thoenissen/Reihitsu#1544

### Findings
_None._

### Checked
- `BraceBlankLinePolicy` (`src/Reihitsu.Formatter/Pipeline/BlankLines/Utilities/BraceBlankLinePolicy.cs`) is used by `BlankLineTokenCleanupRewriter` and `BlankLineTriviaBoundaryRewriter`, and it is a code owner of `sdd/specs/formatter/blank-lines.md`. The row's gloss ("braces, brackets, pattern parentheses, and the comments glued to them") matches its summary and `IsClosingTokenOfPatternOrCollectionExpression`.
- Decision 2: the `LineBreakPhase` row on `main` already says "it keeps every blank line". That matches `line-breaks.md:10/15` ("keeps the blank lines it finds (FMT-BREAK-030)").
- Every row of the ownership block splits into 5 cells on `|`, so `cells[3]` is always the guard column.

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `PhaseOwnershipDocumentationTests.OwnershipMapGuardNamesResolveToDeclarations` | Every identifier-shaped backticked guard name resolves in the formatter or core assemblies, or as a `SyntaxKind` member | A name with no declaration. The test implementer showed it failing on `NoSuchPolicy.Decide` and `BlankLineEditor.NoSuchMember` | Reflection (`DeclaredOnly`, all bindings) | adequate for the Done-when. A bare name resolves against a member of any type, which is weaker; see Hints |

## Whole run

### Findings
Finding #1 above is the only one.

Why the verdict is `pass, text cleanup`: finding #1 is limited to three Markdown files. `LaneClassifier` and its tests already implement and pin the classification, so the repair cannot reach compiled behavior, diagnostics, or rule metadata. #1471's Done-when ("no longer state a classification that differs from the classifier's") is met only once this text cleanup lands.

### Guard scope
See #1471. No other predicate was added or changed.

### Failure classes dismissed
- FC-004 to FC-014, FC-018 to FC-030, FC-036, FC-038 to FC-044: these classes cover formatter phases, analyzers, code fixes, trivia, directives, literals, and unsupported shapes. The run changes no formatter, analyzer, or code-fix source. The only production change is the lane classifier, which handles path strings.
- FC-001, FC-002, FC-003, FC-015: traced under *Guard scope* and *Policy ownership* for #1471. They hold.
- FC-016: reached by the prose restatements. That is finding #1.
- FC-017, FC-033, FC-034, FC-035: traced under *Assertion adequacy*. The expected values come from the issue and from `squad/README.md` (*Lanes*), not from a patched copy of the code.
- FC-031: the summaries of `_textFiles` and `IsRootMarkdownText` describe the new code. The diff has no stale inline comment.
- FC-032: no compiled change happened after the cleaner's and the implementer's runs. The commits after `3b4ac9f` are handovers only.
- FC-037: no C# comment-only claim is made.

### Visible output
_N/A — no test of a visible surface and no rule page changes. The lane check, the scripts documentation, and the squad files are repository tooling._

### Cost of special cases
_N/A — the run adds no rare-input guard. `IsRootMarkdownText` is one cheap check that replaces two file names with a rule._

### Scope
Every edit lies in the files the issues name, or in `squad/roles/auditor.md`, which decision 3 took in. There are no `TODO`s and no tracker numbers in source. In `LaneClassifier.cs`, a one-line change to the `_textFiles` doc summary goes with the change.

### Architecture findings
_None._ Finding #1 is prose that restates the one code owner. The classifier has a single owner.

### Hints
- `IsDeclared` in `PhaseOwnershipDocumentationTests` resolves a single name (`IsOwnedGap`, `AreInSameGroup`, `IsLastLineEndedByFollowingToken`) against a member of any type in the formatter and core assemblies. If that member were removed from the type the row means while another type keeps a member of the same name, the stale entry would pass. This meets the issue's wording ("resolves to a declaration"); tying `A.B`/`C` continuations to `A` would be stricter.
- `00-lead.md` describes the 2026-11-16 comment on #1544 as from the "maintainer account". The comment is generated (it carries the Claude Code footer), so it is evidence, not a maintainer answer. Decision 2 stands on the row text on `main` either way.
- Each test project keeps a private `FindRepositoryRoot` copy (three in `ArchitectureTests/Sdd`, one in `PhaseOwnershipDocumentationTests`, and now one in `LaneClassifierTests`). This was already the pattern before the run, and it is test-only.

### Follow-up candidates
_None_ beyond the S1125 candidate already in `00-lead.md`.
