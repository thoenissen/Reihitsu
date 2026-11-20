Role: auditor
Commit: ab73ac60d309dd4b796a4becce56a72a298e1437
Verdict: pass

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Full audit of head `ab73ac6` against `origin/main` `725ea15`. The checkout is `ab73ac6`. `git fetch` shows `origin/main` at `725ea15`, and `725ea15` is an ancestor of HEAD, so main is merged. `scripts/lane-check.sh behavior-change` reports "confirmed". The diff (22 files) matches the handovers:

- two `.resx` files with five values;
- two analyzer `<summary>` lines;
- two rule pages and two package README rows;
- four spec text lines and a new change doc;
- five new tests in two test files;
- `sdd/work/`.

No other path is touched.

Issue thoenissen/Reihitsu#897 (`agent-filed`). Every goal is addressed or dropped with a recorded reason:

- New RH5112 and RH5048 titles and messages: done (maintainer answers 1 and 2).
- Code-fix title that repeats the wording: done (answer 3).
- Rule-page headings: done.
- Localized resources: dropped in lead decision 2. Verified that only the two neutral `.resx` files exist.
- IDs and severities: unchanged.

The run goes beyond the issue in two places, rule-page body sentences and package README rows. Both are covered by answers 4 and 5 and by change-doc change 4.

## Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|

_N/A — no confirmed finding._

What I checked:

- **Every resource key of the two rules is changed.** `rg 'name="RH5(112|048)'` finds exactly these: `RH5112Title`, `RH5112MessageFormat`, `RH5048Title`, `RH5048MessageFormat`, and `CodeFixResources` `RH5112Title`. All five changed. Neither rule has a `Description` resource.
- **No production source keeps the old wording.** `rg -i "fluent call|first call|wrapped fluent"` outside `sdd/work`, `binaries`, and historical change docs finds the old wording in no production source and no resource.
  - The remaining "first call" mentions in `documentation/rules/RH5201.md`, `documentation/rules/RH5112.md:14`, `shared/comment-placement.md:398-402`, and `shared/fluent-chain-layout.md:260-262` mean an actual invocation.
  - The test and formatter-test `<summary>` lines that say "first call" describe inputs whose first link is a call. The skeptic recommended keeping them and the test implementer did.
  - Mentions in older change docs (`fix-layout-scope.md`, `unsupported-link-operator-trivia.md`, `attached-part-below-element.md`) are history.
- **The code-fix behavior is unchanged.** The equivalence key is still `nameof(RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineCodeFixProvider)`, read at `RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineCodeFixProvider.cs:56`. No type was renamed, so the `sdd/specs/README.md:189,202` code owners stay valid.
- **The analyzer `.cs` edits are text-only.** `scripts/verify-text-only.sh --base 9115ddd --head 10b11d5` reports both analyzer files as text-only ("tokens identical, changed trivia: documentation"). The two `.resx` files are reported as behavioral, which is intended.
- **Encoding is preserved.** Both rule pages keep their UTF-8 BOM. Each `.resx` change is a one-line value swap with no line-ending change.
- **Punctuation is consistent.**
  - RH5112's resource ends with a period; RH5048's has none (answers 1 and 2).
  - Both package README rows end with a period, as every row did before.
  - `AnalyzerMetadataDiscovery.NormalizeRuleTitle` makes the title tests and the README test ignore the trailing period.
  - The new title tests compare exactly, so they pin the answered punctuation.

### Visible output is decided

| Visible surface changed | Covered by |
|---|---|
| RH5112 title and message: "Wrapped chains should keep the first member access on the original line." | Answer 1 (`00-lead.md`, change doc *Decisions*) |
| RH5048 title and message: "Comments must not be placed before the first member access of a wrapped chain" | Answer 2 |
| RH5112 code-fix title: "Keep first wrapped member access on original line" | Answer 3 |
| Rule-page headings and package README rows | Answers 1 and 2, recorded in change doc *Decisions* items 1 and 2 ("the rule page heading and the package README row carry the same title"). Also required by the unchanged `AnalyzerTitleResourcesMatchRuleDocumentationTitles` and `PackageReadmeDescriptionsMatchRuleDocumentationTitles` |
| RH5112 "Why" and "How to fix" sentences | Answer 4 |
| RH5048 "Why" sentence | Answer 5 (option B); text matches word for word |

No expected value of a test changed except the five new literals, and each of those is one of the answered strings.

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|

_N/A — the diff adds or changes no predicate, guard, or exemption. Reporting logic, locations, and the fix rewrite are untouched._

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| RH5112 title and message text | `AnalyzerResources.resx` `RH5112Title`/`RH5112MessageFormat` (single source). Mirrored by design in `documentation/rules/RH5112.md:1`, `src/Reihitsu.Analyzer.Package/README.MD:173`, and the spec title `sdd/specs/analyzer/RH5112.md:1`. The mirrors are kept in sync by `AnalyzerPackageMetadataTests` and the architecture Sdd tests | ok — all mirrors carry the new text |
| RH5048 title and message text | `AnalyzerResources.resx` `RH5048Title`/`RH5048MessageFormat`. Mirrors: `documentation/rules/RH5048.md:1`, `README.MD:160`, `sdd/specs/analyzer/RH5048.md:1`, plus the non-goal pointers in `RH5038.md:18` and `shared/comment-placement.md:17` | ok |
| RH5112 code-fix title | `CodeFixResources.resx` `RH5112Title`, used once at the provider (`:54`) | ok — one owner |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `RH5112…AnalyzerTests.VerifyTitleNamesFirstMemberAccess` | The descriptor title is exactly the answered string, period included | Any other title text or punctuation | `SupportedDiagnostics.Single().Title` with `Assert.AreEqual`, exact comparison | adequate |
| `RH5112…AnalyzerTests.VerifyWrappedPropertyFirstLinkIsReportedWithMemberAccessMessage` | A call-less chain is reported on the first `.` with the answered message, and the fix result is unchanged | A different message, location, or fix output | `Verify(source, fixed, Diagnostics(id, literal))`, where `Diagnostics` applies `.WithMessage` (`AnalyzerTestsBase{TAnalyzer}.cs:72`) | adequate — the message is a literal, not `MessageFormat` (FC-035 avoided) |
| `RH5112…AnalyzerTests.VerifyCodeFixTitleNamesFirstWrappedMemberAccess` | Exactly one action is offered, with the answered title | No action, several actions, or other title text | `GetCodeFixActionsAsync`, then `Assert.HasCount(1)` and `Assert.AreEqual` on `Title` | adequate |
| `RH5048…AnalyzerTests.VerifyTitleNamesFirstMemberAccess` | The descriptor title is exactly the answered string, with no period | Other text, or a trailing period | Exact `Assert.AreEqual` | adequate |
| `RH5048…AnalyzerTests.VerifyCommentBeforeWrappedPropertyFirstLinkIsReportedWithMemberAccessMessage` | A comment before a call-less first link is reported on the comment with the answered message | A different message or location | `Verify(source, Diagnostics(id, literal))` with `.WithMessage` | adequate |

The handovers record that the tests failed before `10b11d5` and pass after (`04`, `05`). Their expected strings come from the maintainer answers, not from the code.

## Failure classes dismissed

- **FC-001, FC-002, FC-003, FC-005, FC-016, FC-044 (guard scope and boundaries):** no guard or predicate changes.
- **FC-004, FC-019 to FC-021 (trivia and directives in edited gaps), FC-022 to FC-027, FC-029:** no rewrite, analyzer, or formatter logic changes; only resource strings and doc comments.
- **FC-006, FC-007 to FC-009, FC-030 (fix compiles, converges, Fix All, offered action changes the document, fix scope):** the fix code and its equivalence key are unchanged. Only `CodeAction.Title` text changes.
- **FC-010 to FC-014, FC-028, FC-038 to FC-043 (formatter phases, analyzer/formatter parity, unsupported shapes):** the formatter is not touched. The reported spans are unchanged, so parity with the formatter's join of the first link (SHARED-CHAIN-003) is unaffected.
- **FC-015:** checked under *Policy ownership*; each string has one source.
- **FC-017, FC-033, FC-034, FC-035:** checked under *Assertion adequacy*. The literal expected strings come from maintainer answers, and the message goes through `.WithMessage`, so the harness does not normalize it away.
- **FC-018, FC-036 (renames):** no type, member, or file is renamed. Lead decision 3 keeps the type names. Message text is keyed by nothing in the repository (an `rg` for the old strings in tests found no literal, per `04`).
- **FC-031:** both analyzer `<summary>` lines name the new title. The code-fix provider's summary carries no title text. The new tests' summaries describe their inputs.
- **FC-032:** analyzer results do not depend on message text; no project result is reused across a compiled change.
- **FC-037:** the comment-only `.cs` edits are proven with `scripts/verify-text-only.sh`, not with `grep` (see *Findings*).

## Architecture findings

_None._

## Hints

- The RH5048 test file still has `<summary>` lines such as "before the first call of a wrapped chain" (`RH5048CommentsMustNotBePlacedBeforeFirstCallOfWrappedChainAnalyzerTests.cs:27,57,196,227,258`). Those tests use a call as the first link, so the lines are accurate. They were left on the skeptic's recommendation. This is not a finding.

## Follow-up candidates

- RH5201 (title "Method chains should be aligned.", fix title "Align method chain") reports call-less chains too. It is already recorded in `00-lead.md` from `02-skeptic.md`, with its output question. Nothing new.
