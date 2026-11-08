Role: auditor
Commit: 8fd6edf
Verdict: pass, text cleanup

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Inputs: thoenissen/Reihitsu#1385, thoenissen/Reihitsu#1398, thoenissen/Reihitsu#1239 (read from the tracker), `00-lead.md`, handovers 01-08, the specs and the change doc `sdd/changes/2026-11/directive-before-switch-section.md`, and the diff `9e9735c..8fd6edf`.

The single finding (#1) is confined to XML documentation comments of private members of `SwitchCaseBraceRewriter` and `BlankLineEditor`. It cannot reach compiled behavior, diagnostics, public API documentation, or rule metadata: the members are `private`/`internal` in a non-public assembly and the text is `///` documentation only.

## Scope

Full audit. Head `8fd6edf5201096aa98415531c2d1056d8d1c0a75` (checked out, clean tree). Base `origin/main` = `9e9735c561761f44e5e9fec54070336f26aa5cb2`, as the prompt states, and an ancestor of head. The diff is what the handovers describe: three production files (`BlankLineEditor.cs`, `RegionNamingRewriter.cs`, `SwitchCaseBraceRewriter.cs`), the tests of 04/06, the spec diffs of 01/03, the change doc, and `sdd/work/directive-switch-region/`. `git diff aaa7241..HEAD -- src/` touches only `PhaseLineEndingOwnershipTests.cs` (Decision 6), so the implementer's full analyzer/cli/core/architecture results at aaa7241 still describe the production code (FC-032). `scripts/lane-check.sh behavior-change`: `LANE CHECK: confirmed — claimed behavior-change, diff needs behavior-change`.

Probes (fixtures under the session scratchpad, outside the repository): `scripts/format.sh --no-install` over 20 fixtures, a second pass over the blank-line set (`Formatted 0 of 10`, idempotent), `scripts/apply-fix.sh --no-install RH5011` over the formatted switch outputs (10 arms, all `no-diagnostic`), `RH7301` and `RH7302` over the formatted region outputs (6 arms each, all `no-diagnostic`).

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | low | `src/Reihitsu.Formatter/Pipeline/SwitchCaseBraces/Rewriter/SwitchCaseBraceRewriter.cs` `BuildSections`/`BuildSection` `<param name="anyMultiLine">`, `IsEmptySection` summary; `src/Reihitsu.Formatter/Pipeline/BlankLines/Utilities/BlankLineEditor.cs` `EnsureBlankLineBeforeStatement` `<returns>` | FC-031 | – | PR-introduced | (a) `anyMultiLine` is documented as "Whether any non-fall-through section in the switch is multi-line"; since this run the value comes from `HasAnyMultiLineSection`, which also skips `break`-only sections, so a `break`-only section with a multi-line label is a non-fall-through multi-line section that does not set it. (b) `IsEmptySection`, now the condition the changed `HasAnyMultiLineSection` relies on, is summarized as "empty (contains no statements)" while it returns true for a section of only `break` statements — the changed method's own summary says "only `break` statements", so the two summaries contradict each other. (c) `EnsureBlankLineBeforeStatement`'s summary was rewritten in this run, but its `<returns>` still says "or the original if one already exists", omitting the FMT-BLANK-006 exemption that the sibling `EnsureBlankLineBeforeSection` now documents ("if none is required or one already exists"). Comment text only | (a) "Whether any section with a body, other than a `break`-only section, is multi-line"; (b) "contains no statements other than `break`"; (c) align `<returns>` with `EnsureBlankLineBeforeSection` |

## thoenissen/Reihitsu#1385

### Against the specs
- FMT-BLANK-006 (changed): `EnsureBlankLineUnlessPrecededByDirective` returns the node unchanged when `SyntaxTriviaUtilities.IsPrecededByDirective(CombinedLeadingTrivia(firstToken))` holds — the last trivia in front of the token, apart from whitespace and line breaks, over the previous token's trailing trivia plus the token's leading trivia. That is the spec's definition of "directly preceded"; a whole conditional group ends in its `#endif` trivia (disabled text sits between the directives), so it counts. Now applied to statements and sections alike. Probes: `break;⏎#if NEVER … #elif OTHER … #endif⏎case 2:` (unchanged), `break;⏎#pragma …⏎case 2:⏎#pragma …⏎case 5:` (directive between stacked labels; unchanged), `a--;⏎#else⏎a += 2;⏎#endif⏎break;` in a loop (no blank line), `#pragma … // trailing` directly in front of `if` (the comment belongs to the directive; no blank line).
- FMT-BLANK-016 (clarified): otherwise the blank line goes at `FindIndexAfterLeadingDirectives(firstToken.LeadingTrivia)`, behind the last directive (active or inactive) and in front of the comments. Probes: `#pragma⏎/* c */ if` and `#pragma⏎/* multi⏎line */ if` (comment ending on the statement's line: one blank line behind `#pragma`, stable — `InsertBlankLineBeforeToken` counts the line breaks over the whole gap, two here, and the collapser leaves one); `a++; /* x */⏎#pragma⏎/// doc⏎if`; disabled group holding a blank line in its disabled text (`#if NEVER⏎a--;⏎⏎a--;⏎#endif⏎// why⏎if` → blank behind `#endif`); `#pragma⏎// why⏎#pragma restore⏎if` (directly preceded, none). All match the spec, all stable on a second pass.
- FMT-BLANK-004 (changed sentence): section gap exempt as above; `break;⏎// lead⏎#pragma⏎case 2:` gets the FMT-BLANK-011 blank line in front of `// lead` and no second one (skeptic member 15's shape in a section gap).
- FMT-BLANK-005 (`EnsureBlankLineAfterClosingBrace`) untouched; FMT-BLANK-013 region gaps: `a++;⏎#region Inner⏎// c⏎if …⏎#endregion` framed by FMT-BLANK-013, no extra blank line.
- Issue requirements: both instances of the issue are tested (`BlankLineBeforeIfGoesBehindEndIfOfActiveGroup`/`…DisabledGroup`, `PragmaDirectlyInFrontOfSwitchSectionGetsNoBlankLine`). The RH5002/RH5008 fix part and SHARED-STMT-004 are out of scope (moved to #1447 by triage, skeptic *Issue goals*); "change doc records the decision" is met by the change doc.
- Node-level path: the root-section/root-statement branch of `BlankLineBreakSpacingRewriter.Visit` reaches the same editor method; `ResolvePrecedingToken` supplies the document's preceding token. Covered by the two rewritten `ReihitsuFormatterNodeBlankLineTests`.

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Statement/section directly preceded by a directive (FMT-BLANK-006) | Last non-whitespace/non-EOL trivia of preceding token's trailing trivia + token's leading trivia | Analyzer `StatementShouldBePrecededByABlankLineAnalyzerBase` uses the same Core `IsPrecededByDirective`; RH5011-002 uses `IsFollowedByDirective` (first content trivia behind `break`) | Statements: identical owner. Sections: the spans differ by design — `break;⏎#pragma⏎// c⏎case 2:` is not reported by RH5011 but gets a blank line behind `#pragma` from the formatter (P11, spec example under FMT-BLANK-006); `break;⏎// c⏎#pragma⏎case 2:` is reportable by RH5011 and gets the FMT-BLANK-011 blank line in front of `// c`, so the output is RH5011-clean (apply-fix: `no-diagnostic`) | holds |
| Where the blank line goes | Index behind the last directive trivia of the token's own leading trivia | `EnsureBlankLineAfterClosingBrace` (same Core `FindIndexAfterLeadingDirectives`); `BlankLineTriviaBoundaryRewriter` comment boundary | Same helper, same span; a directive cannot sit in a trailing-trivia list, so the leading-trivia-only span loses nothing | holds |
| Gap already has a blank line | Whole gap (`HasBlankLineBeforeToken`) | Unchanged from `main` | Author blank in front of a directive kept, not doubled (FMT-BLANK-016 last sentence) | holds |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Directly preceded by a directive | `Reihitsu.Core/SyntaxTriviaUtilities.IsPrecededByDirective`; consumers `StatementShouldBePrecededByABlankLineAnalyzerBase{TStatement}.cs`, `BlankLineTriviaBoundaryRewriter.cs`, `BlankLineEditor.cs` | one owner |
| Insertion point behind the last directive | `SyntaxTriviaUtilities.FindIndexAfterLeadingDirectives`; consumers `BlankLineEditor` (two paths), `BlankLineCodeFixUtilities` | one owner |
| Statement-path and section-path insertion in the formatter | `BlankLineEditor.EnsureBlankLineUnlessPrecededByDirective` (the two public methods delegate); other inserting paths `EnsureBlankLineAfterClosingBrace` (FMT-BLANK-005, no exemption by spec), `EnsureBlankLineBeforeRegionDirectives`, `BlankLineRegionDirectiveRewriter`, `BlankLineTriviaBoundaryRewriter` — `rg "insertIndex: 0|\.Insert\(0"` in `Pipeline/BlankLines` finds no remaining index-0 insertion | one owner per decision; the diff merged two former copies |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `RequiredBlankLineBehindDirectiveTests` (24 tests) | Exact output, blank line behind the last directive / none when directly preceded | Any misplaced, missing, or doubled blank line; dropped directive/comment | `AssertRuleResult`: exact text, LF and CRLF, second pass, phase stability | adequate |
| `ReihitsuFormatterNodeBlankLineTests.InsertsNoBlankLineAboveSwitchSectionDirectlyPrecededByDirectiveAfterBreakSection`, `…InsertsBlankLineBehindDirectiveOfCommentedBlockStatementAfterBreak` | Same at node level (root section / root statement path) | Blank line in front of `#if !DEBUG` | `AssertFormatsTarget` (exact document text) | adequate |
| `RH5011…FormatterTests.VerifyFormatterKeepsSwitchSectionDirectlyPrecededByDirectiveWithoutBlankLine` | Formatter keeps the section gap and output is RH5011-clean | Inserted blank line, or an RH5011 diagnostic | `VerifyFormatter(source)`, LF and CRLF | adequate |
| `PhaseLineEndingOwnershipTests.LineBreaksAroundDirectivesAndCommentsAreTheDetectedLineEndingInEveryPhase` (expected changed) | FMT-PIPE-019; placement now per FMT-BLANK-016 | Foreign line ending in any phase, or wrong placement | `AssertEveryPhaseUsesTheDetectedLineEnding` | adequate; the changed expected value is demanded by FMT-BLANK-016/-006 (Decision 6) |

### Visible output is decided
Every changed or added expected value is demanded by FMT-BLANK-016 (unchanged first sentence, clarified coverage) or by FMT-BLANK-006 as changed by the accepted change doc, whose `Decisions` records answer 1A; the comment-between case is stated in answer 1A in `00-lead.md`. No rule page changed.

## thoenissen/Reihitsu#1398

### Against the specs
- FMT-SWITCH-004/-006: `HasAnyMultiLineSection` now skips `IsEmptySection` (only `break` statements) besides fall-through sections; `BuildSection` already left such sections bare with the same predicate. A `break`-only section reaches `IsMultiLineSection` only through `LabelContainsMultiLinePattern` (no non-terminal statement, no switch expression in a `break`), so skipping it removes exactly the label path FMT-SWITCH-004's new precedence sentence excludes. Matrix of the skeptic and test implementer (recursive/list/parenthesized pattern × case pattern/`when` clause × one/two `break`s × single/stacked labels × other sections single-line/multi-line) is covered by the eight tests; the other-side cell (break-only section next to a multi-line section) holds.
- A section `case Point {…}: { break; }` (a block holding only `break`) is not `IsEmptySection` and still braces the switch through its label; FMT-SWITCH-006 speaks of `break` statements, and a block is not one, so this is not a member. The `return`/`throw`-only neighbour is spec-decided and a follow-up candidate already.
- Issue requirements: both instances tested; "change doc" goal dropped by triage (bug against unchanged FMT-SWITCH-006), precedence stated in FMT-SWITCH-004.

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Section makes the switch multi-line | Every section's statements (`IsEmptySection`: all statements are `BreakStatement`), then `IsMultiLineSection` | `BuildSection`'s `IsEmptySection` (keeps the section bare) | Same predicate, same span: a section that `BuildSection` never braces never braces the switch | holds |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| `break`-only section | `SwitchCaseBraceRewriter.IsEmptySection` only (`rg "IsEmptySection"`), used by `HasAnyMultiLineSection` and `BuildSection` | one owner |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `SwitchCaseBraceRequirementTests.BreakOnlySection…` (5 unchanged-output tests) | Switch stays unbraced; label layout as FMT-BREAK/FMT-INDENT-025/-026 give it | Any brace or layout change | `AssertRuleResult` | adequate; label layouts are demanded by FMT-INDENT-025 (list pattern) and FMT-INDENT-026 (property pattern anchored to `is` in the `when` clause) |
| `…StaysBareWhenAnotherSectionBracesTheSwitch` | Other side of the boundary | `break`-only section braced, or switch not braced | `AssertRuleResult` with expected | adequate |
| `SwitchCaseBraceRewriterTests.BreakOnlySectionWithMultiLineParenthesizedPatternDoesNotAddBraces` | Phase output equals input | Braces added | phase alone, `Assert.AreEqual` | adequate (phase-level on purpose: the full-pipeline parenthesized label layout is the known follow-up defect) |
| `SwitchCaseBraceRewriterTests.SectionWithBodyAndMultiLineParenthesizedPatternAddsBraces` | Label with a body still braces every section | Section without block | structural assertions on each section | adequate for its purpose (the brace decision; text layout is not its subject) |

### Visible output is decided
Expected values are demanded by FMT-SWITCH-006, unchanged, and the layout requirements named above.

## thoenissen/Reihitsu#1239

### Against the specs
- FMT-REGION-002/-004/-007 (clarified per answer 2A): `RegionNamingRewriter.Rewrite` strips the `#` token's trailing trivia from both paired directives before building the canonical trivia, for described and undescribed pairs, active and disabled, so every rewritten line starts `#region`/`#endregion`; `AddReplacementIfChanged` then rewrites a line canonical except for the gap and leaves a canonical line's trivia alone. Probes: `#  endregion  // Fields` behind `#region Fields`; nested pairs with gaps; disabled `#if false` pair with spaces on both sides of the keyword — all as specified; RH7301 and RH7302 clean on the outputs.
- Trivia safety (FC-004): the `#` token's trailing trivia could only lose user text if a comment could stand there. `# /* c */ region fields`, the same inside `#if false`, and `# // c⏎region` are all syntax errors, which the formatter skips (FMT-PIPE-005), so the strip removes whitespace only.
- Decision 2: `RegionDirectiveUtilities.CreateCanonicalEndRegionTrivia` (Core, shared with the RH7301 fix) is unchanged; the formatter passes it an already stripped directive. The RH7301 fix keeps the gap (`RegionFixKeepsWhitespaceBetweenNumberSignAndEndRegionKeyword`), matching answer 2A ("RH7301 fix output unchanged").
- Issue requirements: the issue's input and the comment's `#  region    bar` shape are covered; "general directive policy" left out by answer 2A (follow-up candidate in `00-lead.md`).

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Gap after `#` removed | Trailing trivia of `HashToken` of every paired region directive | `RegionDirectiveUtilities.GetRegionPairs` decides which directives are paired (unchanged); node-level entry points skip the phase (`FormatsRegionDirectives`) | The strip applies exactly to the pairs the phase already rewrites; unpaired directive → syntax error → file untouched | holds |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Canonical `#endregion // <description>` line | `RegionDirectiveUtilities.CreateCanonicalEndRegionTrivia` (Core; RH7301 fix, formatter) plus the formatter's `WithoutWhitespaceAfterNumberSign` wrapper (`rg "HashToken"`: only the two new overloads) | Two surfaces now write different `#` gaps; the difference is justified by Decision 2 and answer 2A, documented at the overload, and pinned by the RH7301 characterization test — parity is deliberately not required. No finding |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `RegionLineFormTests` (9 new tests in region `Whitespace between the number sign and the keyword`) | Exact region lines without the gap, described/undescribed, tab, nested, disabled | Gap kept on either line, description or `#endregion` text altered | `AssertRuleResult` | adequate; the "nothing else changes" test covers FC-003's other side together with the existing canonical-line tests |
| `RH7301RegionsShouldMatchAnalyzerTests.RegionFixKeepsWhitespaceBetweenNumberSignAndEndRegionKeyword` | RH7301 fix output keeps the gap | Gap removed by the fix | `Verify` (batch base, fixed source exact) | adequate; expected value covered by answer 2A |

### Visible output is decided
Every new region expected value follows answer 2A recorded under `Maintainer answers` in `00-lead.md` and the clarified FMT-REGION-002/-004/-007; the RH7301 expected value is `main`'s output, covered by the same answer.

## Whole run

### Failure classes dismissed
- FC-005 — the only guards are directive guards (`IsPrecededByDirective`, `FindIndexAfterLeadingDirectives`); no comment guard is taken to cover directives.
- FC-006, FC-022, FC-023, FC-024, FC-025, FC-026, FC-029, FC-036 — no rewrite of tokens, literals, identifiers, members, or names; the run only inserts/omits a line break, omits braces, or removes whitespace trivia after `#`.
- FC-007, FC-008, FC-009, FC-018, FC-030 — no code fix changed; the RH5002/RH5008 fix part is #1447. The RH7301 fix output is pinned unchanged.
- FC-011 — no loop added.
- FC-012 — the inserted blank line lies between a directive and a comment, which the cleanup rewriter does not remove (it removes only blank lines between the last directive and a brace with nothing but whitespace between); second passes over all probes and every `AssertRuleResult` test are no-ops.
- FC-014 — no new rule.
- FC-028 — the region phase is skipped at node level; the blank-line and switch changes reach `FormatNode` and the document entry points through the same editor/rewriter, and no caller formats a detached statement whose own leading trivia carries a directive (skeptic, test implementer).
- FC-033, FC-034, FC-035 — tests were red at f574f97 on the specified difference only, with expected values from the spec examples; the reported instances themselves are tested.
- FC-037 — no text-only claim.
- Classes traced and holding: FC-001/FC-002 (guard tables above), FC-003 (both sides tested for each changed condition), FC-004/FC-019/FC-020/FC-021 (directive, disabled group, comment ending mid-line, `#` gap with comment = syntax error), FC-010 (second pass, LF/CRLF), FC-013 (RH5011/RH7301/RH7302 clean on outputs), FC-015/FC-016 (one owner per decision; the sibling editor paths agree), FC-017 (tables above), FC-027 (stacked labels, `when` clause, nested regions), FC-031 (finding #1), FC-032 (no production change after the implementer's full runs). Tab-width matrix: not reached — the run computes no column.

### Architecture findings
_None._ Each decision has one owner; the formatter/RH7301 difference in the `#` gap is a recorded decision, not a drift.

### Hints
- `AGENTS.md`'s phase-ownership row for `RegionFormattingPhase` lists the region-line decisions individually and does not name the new one (no whitespace between `#` and the keyword). It is covered by the row's opening "the text of every paired `#region` and `#endregion`", so this is not a finding; the lead may add it for completeness.
- #1239's outcome changes visible output without a change doc, treated as a reading of FMT-REGION-007 (Decision 4). `sdd/README.md` describes a clarification as writing down behavior that already exists; the two-readings framing makes it a bug fix against the chosen reading, which the lead decided. Recorded only so the PR states it.
