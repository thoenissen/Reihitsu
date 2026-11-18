Role: auditor
Commit: 0831dfe193c442351722817386bf294c860cc657
Verdict: findings

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Full audit. Head `0831dfe` (= `33be182` plus a `00-lead.md` state line); `origin/main` = `6d574e1159eef2628a716c472a569e37d9676532` (confirmed with `git ls-remote`), merged (`git merge-base --is-ancestor` true). The diff is what the handovers describe: the RH5307 analyzer (two methods made `internal`), the RH5307 and RH5401 code fixes, the two analyzer test files, the RH5307/RH5401 specs, the change doc, the two rule pages, and `sdd/work/`. `scripts/lane-check.sh behavior-change`: confirmed.

Probes: `scripts/apply-fix.sh` (LF and CRLF) over 30 RH5401 and 7 RH5307 fixtures written outside the repository under `/tmp/claude-0/-home-user-Reihitsu/5c2095c3-beb1-5980-beb5-54178f424a54/scratchpad/audit12/` (outputs `rh5401.out`, `rh5401b.out`, `rh5401c.out`, `rh5307.out` next to them). LF and CRLF gave the same status and diff in every fixture; no fixture was not-converged, no-progress, invalid, or drifted.

## Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | medium | `src/Reihitsu.Analyzer.CodeFixes/Rules/Layout/RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedCodeFixProvider.cs`, `KeepsBlankLinesBehindAttributeLists`; `sdd/specs/analyzer/RH5401.md` RH5401-004 ("A gap the formatter creates when it splits one attribute list … has no counterpart as written and holds no blank line; it does not withhold the fix") | FC-001, FC-016 | – | PR-introduced | The guard compares only the gaps behind each attribute list as written. A blank line written *between two attributes of one list* is not in any compared gap, and when FMT-STRUCT-028 splits that list the formatter drops it. The fix is newly offered for these inputs (on `main` every attributed property was refused) and removes a blank line the user wrote in the attribute region — the line the maintainer's answers 3, 5, and 6 say the fix does not touch, and which the spec sentence quoted claims cannot exist. Failing members (all `fixed`, LF and CRLF): `[Obsolete,⏎⏎ CLSCompliant(false)]⏎int E⏎    => 1;` → `[Obsolete]⏎[CLSCompliant(false)]⏎int E => 1;` (fixture `p03`); the same with a third list `[Serializable]` behind it (`q04`); two blank lines behind the comma (`s03`); the blank line in front of the comma, `[Obsolete⏎⏎ , CLSCompliant(false)]` (`s02`). Holding members: a comment next to the blank line, which keeps the formatter from splitting, `[Obsolete,⏎⏎ // c⏎ CLSCompliant(false)]` (`s01`, blank line kept); a blank line inside attribute arguments `[Obsolete("a",⏎⏎ false)]` (`q01`), behind `[` (`q02`), in front of `]` (`q03`) — all kept; every gap behind a list (`p01`, `p02`, `p04`–`p07`, `p10`, `p11`, `p13`, `p16`–`p18`) behaves as RH5401-004 states. Rare shape (a blank line inside an attribute list). No test covers it. | Refuse the class by its cause with the same cheap count: withhold when the blank lines over the whole attribute region — from the first list's `[` through the gap in front of the property's first own token — differ in number between the property as written and as formatted (or, equivalently narrow, when a list as written that the formatter splits holds a blank line). Spec author adjusts the RH5401-004 sentence on split gaps; test implementer adds a withheld test for `p03`. |
| 2 | low | RH5401 fix: inline comment in `RegisterCodeFixesCoreAsync` ("Only offer the action when the formatter is actually able to collapse the property. For shapes the formatter keeps multi-lined the formatting is a no-op …") and the name `CanCollapse` | FC-031 | – | PR-introduced | `CanCollapse` now also withholds the fix where formatting would change a blank line behind an attribute list, where the formatting is not a no-op. The call site's comment and the method name still describe only the one-line test. | Reword the call-site comment to name both conditions; optionally rename `CanCollapse` (for example to a name that says the fix would be complete and blank-line neutral). |
| 3 | low | `sdd/changes/2026-11/rh5307-rh5401-fix-the-reported-construct.md`, *Visible output changes*, RH5401 | – | – | same requirement | The fix newly splits a multi-attribute list into one list per line (`[Obsolete, CLSCompliant(false)]⏎int L⏎    => 1;` → `[Obsolete]⏎[CLSCompliant(false)]⏎int L => 1;`, RH5401-003 example `L`; also `VerifyFixSplitsAnAttributeListWithSeveralAttributesOnThePropertysLine`). The output follows the decision "the fix lays the attribute lists out as the formatter does" (#1219 Q1 as recorded) and unchanged FMT-STRUCT-028, so it is decided, but the change doc's RH5401 before/after blocks show only `A`, `B`, `E` and the decision text names only FMT-BREAK-027. | Add `L` to the RH5401 before/after block and FMT-STRUCT-028 to the decision sentence. Text only. |

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| RH5307: which list the fix joins | `root.FindNode(diagnostic span, getInnermostNodeForTie: true)`, accepted only when it is an `ElementAccessExpressionSyntax` or `BracketedArgumentListSyntax` whose `Span` equals the diagnostic span | Analyzer's location: `elementAccessExpression.GetLocation()` for an element access, `bracketedArgumentList.GetLocation()` otherwise (RH5307-003) | Same node. `FindNode` stops at the first ancestor of the span's first token whose full span contains the span; no descendant of an element access has its span, and an implicit access's list is found before its same-span parent. Traced and run for receiver kinds `j[0]`, `c?[0]`, `a[0].P[0]`, `this[0]`, `base[…]`, `(j[0])`, `c?.P`; containers `x[…]`, `n?[…]`, initializer `[…] = 1`, invocation argument, collection element, interpolation hole, assignment target; nesting depth 2 and 3 (`r01`: three reported lists, fixed in 3 iterations; Fix All passes = diagnostics, each pass fixes the innermost) | holds |
| RH5307: whether the fix is offered | `IsIndexerArgumentList`, `CanSafelyCollapseToSingleLine` on the resolved list | The analyzer's same two methods (now one statement, `internal`) plus its brackets-on-different-lines check, which the diagnostic's existence already implies | Same predicate on the same node | holds |
| RH5307: Fix All position mapping | `DocumentOrderFixAllProvider.MapSpan` keeps a span only when no change falls inside it; the fix needs the exact span | `TryGetFormatTarget` requires `Span == span` | A lost span triggers re-analysis, so a stale span never reaches the exact-span test | holds |
| RH5401: own code on one line | Formatted text from the first token behind the last attribute list (`AttributeLists.Last().GetLastToken().GetNextToken()`) to `Span.End`, excluding that token's leading trivia and the `;`'s trailing trivia | RH5401-004 ("from its first token behind its attribute lists … through its `;`, without the trivia in front of that token or behind the `;`") | Same boundaries; a comment or directive between the lists and the first token is excluded (`p05`, `p10`, `p11`, `p17` offered), one inside the own code counts (`p09` stays one line, fixed) | holds |
| RH5401: blank lines behind attribute lists | Per list as written: `TokenGapAnalysis.Between(]`, next token`).BlankLineCount`, compared with the gap behind the formatted list that holds that list's last attribute | Maintainer answer 6B: withhold "when formatting would insert or remove a blank line behind an attribute list"; RH5401-004 | Agrees for every gap behind a list as written. Does not inspect blank lines between attributes of one list that the formatter splits — finding #1 | finding #1 |
| RH5401: guard input vs applied formatting | Guard reads `FormatNode` (detached); the fix applies `FormatNodeInDocumentAsync` through `FormatMemberOnLinesOfItsOwnAsync` (FC-028) | — | Every offered probe kept the gaps' counts in the applied result; the shared-line case (`int _f; [Obsolete] int E`, `p20`) separates the member outside the compared gaps | holds |
| RH5401: defensive branches | `formattedAttributes.Count` mismatch, `Attributes.Count == 0`, formatted attribute without an `AttributeListSyntax` parent | — | Unreachable in compiling code; each withholds | holds (dead but safe) |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| RH5307 reporting condition | `rg -n "IsIndexerArgumentList\|CanSafelyCollapseToSingleLine" src --glob '!*Test*/**'`: declarations and use in `RH5307IndexerBracketedArgumentsShouldBeSingleLinedAnalyzer`, use in the code fix; the fix's private copies are deleted | one owner |
| RH5401 own-code one-line measure | `rg -n "IndexOf\('\\\\n'\)" src/Reihitsu.Analyzer.CodeFixes`: only `CanCollapse` | one owner |
| Blank-line count of a token gap | `TokenGapAnalysis.BlankLineCount` (Reihitsu.Core), used by the formatter's blank-line policies and now by the RH5401 fix; `rg -n "KeepsBlankLinesBehindAttributeLists\|BlankLineCount" src --glob '!*Test*/**'` finds no private counter | one owner |
| "First token behind the attribute lists" | `AttributeLists.Last().GetLastToken().GetNextToken()` in the RH5401 fix and `OrderingDeclarationUtilities.cs:889`; one expression, no rule to drift | not a duplicated policy |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH5307 `VerifyFixJoins…` (11 single-member tests) | The reported list, and only it, is joined; receiver and container stay as written | Another list joined, no fix, an action that changes nothing, or any other byte different, under LF or CRLF | `VerifyCodeFixWithEndOfLine`: `Verify` (iterative, Fix All in document/project/solution, re-analysis) plus exact raw text of one fix | adequate |
| `VerifyFixesJoinBothListsWhenTheReceiverListIsReportedToo`, `VerifyFixesJoinEveryReportedListWhateverItsReceiverOrContainer` | Every reported list is joined, one at a time and by Fix All | Outer list left wrapped (today's defect), or another layout | `Verify` with both EOL | adequate |
| `VerifyEachFixJoinsItsOwnListWhenTheReceiverListIsReportedToo` | Each diagnostic's fix joins its own list; the intermediate text is exact, in both orders | The outer diagnostic's fix touching the receiver's list | `TryApplyCodeFixAtAsync` with a location at each element access, `Assert.AreEqual` on raw text | adequate (FC-008, FC-009) |
| `VerifyNullConditionalElementAccessIsNotReported` | `c?[1,⏎2]` not reported | A diagnostic | `Verify` | adequate |
| RH5401 offered tests (`A`, `B`, `L`, `E`, `F`, one blank line without comment, directives, doc comment above, trailing comments, shared line, multi-line attribute arguments, comment in brackets) | Exact formatter layout of the property; blank lines behind lists as written | Any different byte, a withheld fix, under LF or CRLF | `VerifyCodeFixWithEndOfLine` / `Verify` | adequate |
| RH5401 withheld tests (`D`, `G`, `H`, `K`, `M`, the RH5401-004 combined example, wrapped expression, comment in front of `=>`, FMT-UNSUP-009 attribute list) | Diagnostic stays, no action registered | An action registered | `VerifyNoCodeFixIsOffered` (markup diagnostics + no action for any diagnostic) | adequate; both sides of the blank-line qualifier have tests (FC-003) |
| `VerifyFixAllLeavesAPropertyWhoseBlankLinesTheFormatterWouldChange` | Fix All and iterative fixes fix `E`, `A` and leave `D` reported and as written | `D` changed, or `E`/`A` unfixed | `Verify` with `MarkupMode.Allow` on fixed and batch-fixed states, `D` marked | adequate |
| (missing) blank line between attributes of a list the formatter splits | Withheld (after #1's change) | Fix offered, blank line removed | — | missing, part of finding #1 |

## Failure classes dismissed

- FC-002 — the qualifiers ("behind its attribute lists", "without the trivia in front of that token") are explicit conditions; checked in *Guard scope*.
- FC-004, FC-005, FC-019, FC-020, FC-021 — neither fix relocates a comment or directive in the probes: comments and directives between lists and own code stay (`p04`–`p07`, `p10`, `p11`, `p17`, `s01`); a comment directly behind a list is withheld (`p01`, `p13`, `p16`, `q07`); `#region` inside the property is withheld (`p15`). RH5307's join is unchanged formatter behavior; comments in the receiver or container stay (`c`, `p` tests).
- FC-006, FC-022–FC-025, FC-036, FC-018 — whitespace-only rewrites of a construct already in its final kind; no rename; RH5401 covers properties only.
- FC-007 — every probe converged (`fixed`), multi-diagnostic probes in 2–3 iterations.
- FC-008 — nested RH5307 spans (2 and 3 levels) and RH5401 BatchFixer with a withheld member covered.
- FC-009 — RH5307 no-progress members (`vb`, `vi`, `vj`, `vn`) now fixed; RH5401 withholds where the result would keep the diagnostic.
- FC-010–FC-014 — no formatter phase changed; both fixes delegate to it unchanged; the fixes' outputs equal whole-document formatting except where SHARED-FIX-006 or the blank-line rule says otherwise.
- FC-015 — see *Policy ownership*.
- FC-017 — exact-text assertions under LF and CRLF, iterative and Fix All.
- FC-026 — several identical `[`/`]` tokens on one line resolved by exact node span, not by token search (`r01`, `r03`, `r07`).
- FC-027 — enclosing scopes enumerated in *Guard scope*, RH5307 row 1.
- FC-028 — see *Guard scope*, RH5401 detached vs in-document.
- FC-029 — no key/value layout.
- FC-030 — RH5307 still formats only the list; RH5401 formats only the property (`p14`: a `///` comment written between the attribute list and the property is inside the node and normalized; see *Hints*).
- FC-031–FC-035, FC-037 — expected values come from the spec examples, not from patched code; no text-only claim; finding #2 is the one FC-031 item.
- FC-038–FC-043 — no shape declared; FC-043: both fixes ask `FindUnsupportedSyntax` about the node they format (unchanged); FMT-UNSUP-009 in an attribute list is tested withheld.
- FC-044 — the RH5401 guard reads the formatter's computed result rather than modelling the blank-line phase.

## Architecture findings

_None._ RH5307's analyzer/fix disagreement on the reported list (skeptic's finding) is resolved: the fix reads the diagnostic's node and the analyzer's single predicate. RH5401 reads the formatter's result and does not restate the blank-line rule.

## Hints

- **Pre-existing, unverified:** `CanCollapse` detects a line break with `IndexOf('\n')` (on `main`: `formatted.ToString().IndexOf('\n')`). In a document whose line ending is a lone `\r`, own code that stays on several lines contains no `\n`, so the one-line test passes; the fix would be offered and leave the diagnostic. `TokenGapAnalysis` counts every C# line terminator (`LineEndingUtilities`). Not run: `apply-fix.sh` covers LF and CRLF only. Not caused by the run.
- A `///` comment written between an attribute list and the property's own code with a blank line above it (`p14`) gets the fix, which spreads the one-line `<summary>` over three lines. The comment is inside the reported node, so RH5401-003 ("as the formatter formats it in place") allows it; the compiler warns about the placement (CS1587). Without the blank line the fix is withheld (`q07`). Rare shape; recorded only.

## Follow-up candidates

_None_ beyond `00-lead.md` (FMT-BREAK-014 and `c?[1,⏎2]`).
