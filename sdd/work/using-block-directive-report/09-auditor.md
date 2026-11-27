Role: auditor
Commit: 6f9642a7166c2d704b824b30cca09013970b36fb
Verdict: pass

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Full audit of thoenissen/Reihitsu#1836. Head 6f9642a (checked out, clean tree). Base `origin/main` 5db426f, which equals `git ls-remote origin refs/heads/main` and is an ancestor of head. The diff since the spec checker (122bcf7) is the merge of 5db426f (#1967). That merge touches 25 test files of other rules, and none of them is in this run's diff. Production diff: 7 analyzer files, +8/−36. This matches 05-implementer. Tests and helper match 04/06. Specs and change doc match 01/03. `scripts/lane-check.sh behavior-change`: confirmed. The cleaner's Release build (8481900) predates the merge. The merged paths are disjoint test files, so I did not rebuild; the validator builds head.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|

_None._

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| RH7207 skips the blank-line check (`IsOrganized`, after the order check) | `UsingDirectiveOrderingSafety.CanSafelyReorder(GetUsings(scope))`: `ContainsDirectives` of every directive of the whole block. That is each directive's full span with leading and trailing trivia, global and local sections together | Fix guards `UsingDirectiveOrderingCodeFixProviderBase.cs:89`, `RH7207…CodeFixProvider.cs:68`, `UsingDirectiveCodeFixUtilities.cs:31`; formatter `UsingDirectiveOrderingRewriter.cs:76` (FMT-USING-004) | Same predicate on the same list (`GetUsings(scope)` / the rewriter's block list). Scope resolution `TryGetUsingDirectiveScope` takes the diagnostic's start token → enclosing `UsingDirectiveSyntax` → parent. For every location the seven analyzers report, including a name split by a `#pragma` (RH7203-003), that is the block the analyzer judged | holds |
| RH7201–RH7206 early return removed | none now (every block judged) | SHARED-USING-009 / -001 of each rule (no guard left) | Spec and code both report every block | holds |
| Build dependence of "unsafe" | Mid-block or leading directive: the directive is in a using's trivia in every build, so the block is unsafe in every build. Trailing conditional group: it is in the last using's trivia only in a build that compiles a using inside it | Formatter/CLI parse without symbols | Differs only for the trailing group, change doc case 5, covered by maintainer answer 1 | holds (decided) |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Block may not be reordered (SHARED-USING-009) | `rg -n CanSafelyReorder src`: one owner, `Reihitsu.Core/UsingDirectiveOrderingSafety.cs:23`. Consumers: 3 fix-side call sites, the formatter rewriter, and RH7207 analyzer `:67`. No private copy | one owner |
| Canonical order | `UsingDirectiveOrderingUtilities.ComputeCanonicalOrder` (analyzer); the formatter's `UsingGrouping.ComputeCanonicalOrder` is pre-existing and unchanged by the diff | unchanged by the run |
| Within-group comparison (SHARED-USING-007) | `UsingDirectiveOrderingUtilities.GetDirectivesOutOfOrderWithinTheirGroup` (RH7203, RH7204, RH7206) | one owner |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `…IsReportedWithoutCodeFix` (all 7 classes, 30 rows incl. DEBUG/NETFRAMEWORK/`#define`/scopes/global section) | exactly the marked diagnostics are reported; every fixable diagnostic of the plain source gets zero actions | a missing or extra diagnostic, a wrong span, or any registered action | `VerifyNoCodeFixIsOffered` (all overloads end in the private one: `Verify` exact set, then `Assert.IsNotEmpty` + `Assert.IsEmpty(actions)` per diagnostic, same symbols for both parts) | adequate |
| new overload `VerifyNoCodeFixIsOffered(string, Action, params string[])` | applies symbols to the analyzer run and the fix document alike | symbols missing from one half | sets `SolutionTransforms` symbols and passes them to `CreateTestDocument`. Overload resolution: `(src, null, "DEBUG")` cannot bind the `DocumentationMode` overload; `(src, cfg)` binds the non-expanded overload | adequate |
| RH7203 `FixAllOrganizesOnlyTheBlockWithoutPreprocessorDirective` | the safe namespace block is fixed and the unsafe compilation-unit diagnostic stays, incrementally and with Fix All | the unsafe block rewritten, or its diagnostic gone | `Verify(src, fixed, cfg)` with `MarkupMode.Allow` on fixed and batch-fixed states, iterations 1/1 | adequate (markup now asserted, not ignored) |
| RH7207 `…NotReportedForAMissingBlankLine` / `…ForABlankLineInsideAGroup` / `…InCanonicalOrderIsNotReportedForItsBlankLines` + existing `DiagnosticWhenMissingBlankLineBetweenGroups` | both sides of the blank-line boundary (FC-003): with a directive not reported, without one reported | a report in the unsafe block | `Verify` / `VerifyWithPreprocessorSymbols` | adequate |
| RH7207 `…OutOfOrderAndWithoutBlankLineIsReportedOnce…`, `…ReportedOnceWithoutCodeFix` | one diagnostic per block | a second diagnostic | exact-set `Verify` inside the helper | adequate |
| Build pairs (RH7203 `…WhenTheBuildCompilesAnOrderedBlock` ×2, RH7205 `Excluded…`, RH7207 `ConditionalAlternativeInOrder…`, `Excluded…DoesNotPreventReportAndFix`) | the other build is judged as it compiles | a report in the ordered build, or a lost fix after the block | `Verify` / `VerifyWithPreprocessorSymbols` / `Verify(src, fixed)` | adequate |
| Formatter `FormatterKeepsAReportedBlock…` ×2 | reported, formatter output identical, every phase stable, LF and CRLF | reorder or any change | `VerifyFormatterKeepsReportedShape` | adequate |
| Formatter `FormattedBlankLinesAroundARegionInsideAGroupAreNotReported` | input and formatted output (the FMT-BLANK-013 frame) are not reported; second pass stable | RH7207 on the formatter's output | `VerifyFormatter(src, fixed)`: three `Verify`s + phase stability, LF and CRLF | adequate |

Expected values came from the spec before the change: 04 ran them on unchanged production code, and 45 rows failed with the expected count mismatch. FC-035 does not apply.

Visible output is decided. Every changed expected value is covered by a maintainer answer or an unchanged requirement:
- Reported blocks of every directive kind and position, cases 1–6, the namespace, file-scoped, and global-section scopes, and the build pairs: answers 1A, 2A, 3A in `00-lead.md` and the change doc's `Decisions`.
- RH7207 not reporting blank lines in such a block: SHARED-DIAG-010 (decision 1, recorded in the change doc).
- Report and fix of a block followed by a directive, and the FMT-BLANK-013 / region output in the formatter test: unchanged SHARED-USING-009 clause, FMT-BLANK-013, and region formatting.

No rule page changed.

## Failure classes dismissed
- FC-001, FC-002: the guard is the existing whole-block predicate. It was neither narrowed nor qualified, and its span equals every counterpart's span (see *Guard scope*).
- FC-004, FC-006, FC-021–FC-030, FC-036: no rewrite or fix output changed. The fixes and the formatter keep their guard and output.
- FC-010, FC-011, FC-012: no formatter code changed. The formatter tests still run a second pass and the phase-stability check under LF and CRLF.
- FC-018: no rename.
- FC-032: the implementer's full analyzer run (12098/12099, the one failure being the later-repaired setup) and the focused 356/356 came after the production change. The merge after them only touches other rules' tests.
- FC-033, FC-034: the tests use the issue's and the spec's own shapes, including the `#pragma`-between-tokens and `#if/#else` shapes, under LF and CRLF where the shape has line-ending-sensitive trivia.
- FC-035: expected values from the spec, run red on unpatched code (04).
- FC-037: no text-only claim.
- FC-038–FC-043: nothing declared unsupported (answer 4A). The precheck is untouched.

Checked and holding:
- FC-003, FC-020: directive kind × position × build × scope, per the skeptic's matrix. A directive cannot sit in a using's trailing trivia, because the parser always puts it in the next token's leading trivia.
- FC-005, FC-019: the existing `HashLine…` and `CommentedOutConditionalDirectives…` tests keep `#` comment text safe.
- FC-007, FC-008, FC-009: no action registered; BatchFixer only applies registered actions; mixed-block Fix All test.
- FC-013, FC-014: order reports on the formatter's kept output are covered by answers 1 and 4. RH7207's layout part is exempt, so the region-frame output is not reported (test above).
- FC-015, FC-016: one owner; all seven analyzers changed together.
- FC-017: see table.
- FC-031: the RH7207 `IsOrganized` summary describes the new split. No RH7201–RH7206 comment mentioned the removed guard. `UsingDirectiveOrderingSafety` and `UsingDirectiveCodeFixUtilities` summaries are still true.
- FC-044: the analyzers read the parsed build, where excluded code is disabled text. Nothing models another build.

Other checks:
- No `TODO`, commented-out code, or tracker number in the diff's source.
- Self-hosting: no repository source file has a directive in its using block. A multiline `rg` and a header scan matched only raw-string test inputs.
- RH2111 test-type limit: RH7207 test class is at most ~2770 lines of code, counted as an upper bound from non-comment lines plus all blank lines, against a limit of 3000.
- Spec texts checked against the code: SHARED-USING-009, RH7201-001 to RH7207-001, -004, and the change doc cases 1–6. I checked the diagnostic locations of RH7207-003 on an alias target and of RH7203-003 across a `#pragma`.

## Architecture findings
_None._

## Hints
- A file whose compilation-unit block holds a `global using` section and a local section, with a directive only in the local section, now gets RH7203 etc. on the global section without a fix (RH7203 `GlobalSectionOfABlockWithPreprocessorDirectiveIsReportedWithoutCodeFix`). This follows the spec: the block, not the section, is what SHARED-USING-009 judges, and the fix organizes the whole block. The input is rare and reachable through the analyzers only. I name it so the PR's `Review notes` can mention it next to the withheld-fix classes.
- `AnalyzerTestsBase{TAnalyzer,TCodeFix}` now has three public `VerifyNoCodeFixIsOffered` overloads that each build the same preprocessor-symbol `SolutionTransforms` lambda. One overload taking `onConfigure`, a documentation mode, and symbols could serve all three. Test code only, not blocking.

## Follow-up candidates
_None new._ The two in `00-lead.md` stand:
- fix and formatter for a block whose every directive stands before its first using;
- scoping FMT-USING-004's "keep their blank lines" against FMT-BLANK-013.

The first matters more after this change, since `#nullable enable` headers now get diagnostics without a fix.
