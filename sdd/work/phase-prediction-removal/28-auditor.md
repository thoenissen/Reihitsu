Role: auditor
Commit: 4a2e3ae
Verdict: findings

My prompt carried no intended fix, no suspected cause, and no verdict anyone hoped for.

## Scope

Repair audit. Head 4a2e3aea4e2cadec26605630c0ffd449d769b4d0 (checked out, clean). `git ls-remote origin refs/heads/main` gives 929fc232447d305a2400606fd7a8acdf69cea7c0, which is unchanged and merged. The repair diff is `git diff 724ce92 HEAD`. It matches handovers 18–27 and the decisions recorded at the end of `00-lead.md`.

**Scope grew.** The repair is the structural change the maintainer chose under escalation 12 (`audit-1`, 1B). It adds FMT-PIPE-017, retires RH5303-006 and RH5304-005, and changes SHARED-FIX-007, FMT-RAW-005, FMT-RAW-007 and FMT-INDENT-006. It replaces the node-level column step and touches the documentation phase. So everything the repair moved gets a full audit: `ReihitsuFormatter`, `FormattingContext`, `ReihitsuFormatterHelpers`, `LayoutComputer`, `IndentationPhase`, `IndentationRewriter`, `DocumentationCommentFormattingPhase`, the RH5303/RH5304 providers and helper, and the tests. That audit includes the counterparts these reach: the context entry point, its callers, and the analyzers that check fix output.

Fixtures and probe harnesses are in my scratchpad, outside the repository. The harnesses are file-based apps that call the public node-level entry points, or `FormattingPipeline.Execute` with an observer through reflection. Notation: `→` is a tab, `/` separates lines, and columns are 0-based. Every `apply-fix.sh` result holds under both LF and CRLF.

## Previous findings

| # | Status | Evidence on 4a2e3ae |
|---|---|---|
| 1 | closed | I re-ran all seven members through `apply-fix.sh` and compared each result with `format.sh` on the same input: RH5303 with an object creation, a collection creation, a call argument, an `&&` operand, and the arrow target; RH5304 with an object creation; RH5301 with a complex element. Each fix output equals the document-level output. Every one converges, including RH5303 on the collection creation, which was `no-progress`. The one difference in `m_args` is an expression-bodied method outside the code the fix changes. The holding members still hold. One member of #1's class that the first enumeration did not name fails; see #8. |
| 2 | closed | Through `FormatNodeInDocumentAsync`, all four failing shapes now move `///` onto its own line: method-body `{` with a gap tab, method-body `{` with tab indentation (now at 8, per FMT-PIPE-017), type `{`, and case label. The two shapes that held still hold. |
| 3 | closed | Decided 2A. FMT-RAW-007 and FMT-INDENT-006 now state the line column, and `RawStringInterpolationHoleTests.CommentAboveCommentLedHoleLineTakesTheColumnOfTheLeadingComment` pins it. |
| 4 | closed | `﻿var s = """` / `    a` / `    """;` through `FormatSyntaxTree` gives content and closing marker at character column 9. The same holds with `.Trim()` / `.ToUpper()` behind the closing marker (`.ToUpper` at 12, under `.Trim`), behind a leading `/* c */` (17), and for `$"""` with a hole line (10, with the hole line moved with the literal). |
| 5 | closed | Both summaries now name the tab replacement phase. |
| 6 | superseded by #8 | The new AGENTS sentence says "no step after the last phase moves a line, also in the node-level entry points". The context entry point's restore in #8 is such a step. |
| 7 | superseded by #9 | The phase now clones an attached root, so the `lineOffset` branch has no production caller. The test added for it passes without that branch. |

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 8 | high | `ReihitsuFormatter.FormatNodeInDocumentWithContextAsync`: the unconditional `formattedFirstToken.WithLeadingTrivia(originalFirstToken.LeadingTrivia)` after the pipeline. Counterpart: `LayoutComputer`/`IndentationRewriter` | FC-001, FC-016, FC-028, FC-007 | #1 | same requirement (FMT-PIPE-017) | Here a step after the last phase still decides a written-back line's column. When the target starts a later line of the context, the phases lay that line out and align the target's other lines to anchors on it. The entry point then restores the line's original whitespace, so it moves back while the lines aligned to it stay. FMT-PIPE-017 says "every line they write back MUST end in the column the indentation phase gives it". It exempts only "the leading trivia that FMT-PIPE-010 keeps", and FMT-PIPE-010 keeps the target's leading trivia only when the target does not start its line or is a direct child of the root. So this restore is not exempt. **Fails:** (a) RH5111 on `        int a = 1,` / `              b =` / `                  Other(1,` / `                        2);` (target: the declarator; context: the statement). The fix gives `b = Other(1,` with `b` at 14 and `2` at 18, while `1` is at 24, and RH5203 reports the result. Document-level output: `b` at 8 and `2` at 18 under `1`. The base (c49431e) gives the same fix output, so the code is unchanged by the run; the requirement is the run's. (b) Public API: `Call(1,` / `                 Other(2,` / `                 3));` with the argument `Other(…)` as target and the statement as context. Result: `Other` stays at 17 and `3` lands at 19, while `2` is at 23. Document-level output: `Other` at 13 and `3` at 19 under `2`. (c) RH5111 on a misaligned parameter (`int b =` / `5`) or enum member (`B =` / `2`). The joined target keeps its first line at the input's column (15 and 6) instead of the phase's column (11 and 4). No aligned line breaks, but FMT-PIPE-017 is not met. **Hold:** a mid-line target, where the restore changes no column (RH3208 behind a label, the chain tests); `FluentChainCodeFixHelper`, which falls back to formatting the whole context when the chain's layout differs (skeptic n5). `FormatNodeInDocumentAsync` restores exactly under FMT-PIPE-010's condition (`keepsLeadingTrivia`), so its restore is a no-op for columns. No test runs the context entry point on a target that starts a later line of the context. | A repeat of #1's class is escalation 12. The options: restrict the context restore to FMT-PIPE-010's condition, so the phases' column for the target's first line is written back; or state the exemption in FMT-PIPE-017 and FMT-PIPE-010 and record the RH5111 member as a known gap. Either way, add a test for (a) and (b) and bring the AGENTS sentence (#6) in line. |
| 9 | low | `IndentationRewriter.Apply`, the `lineOffset` mapping and its comment; `IndentationPhaseRawStringMoveTests.MisalignedRawStringInAttachedMethodBelowTheFirstLineIsCorrected` | FC-017 | – | PR-introduced | `IndentationPhase.Execute` now clones every attached root (`CSharpSyntaxTree.Create`) before computing the layout. Every other caller of `LayoutComputer.Compute` plus `IndentationRewriter.Apply` passes a tree root (`PhaseCombinationTests`, `IndentationPhaseIntegrationTests`, `IndentationRewriterTests`). So `lineOffset` is always 0, and the comment describes a caller that does not exist. The test handover 22 names for this branch goes through the clone and passes without the branch. It would equally pass without the clone, because the branch alone handles that input. The clone is also unreachable from the pipeline: in two probes, `HorizontalSpacingPhase` returned a new node even for a target already formatted, so the indentation phase never received an attached root. | Remove the dead mapping, or test it directly with `Compute` plus `Apply` on an attached non-root node. Name the existing test for what it falsifies. |
| 10 | low | `Reihitsu.Core.DocumentationCommentUtilities.ShiftContinuationExteriorMarkers` (public) | — | – | PR-introduced | Its only production caller, `AdjustLeadingWhitespace`, was deleted in c5df1c3. Only its own unit test still calls it. The repair leaves it as dead shared code (25-implementer follow-up). | Remove it together with its test, or record it as the follow-up the implementer named. |

#8 is a blocking repeat of #1's class, so this is escalation 12 before any further repair round. #9 and #10 do not block on their own.

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Whether and by how much the root is shifted (`ShiftToRootFirstTokenColumn`) | `RootFirstTokenColumn` set; the root's first token's line placed; `TryGetLineTextColumn(firstToken)`. The offset is `RootFirstTokenColumn - GetColumnBeforeRawStringShift(firstToken)` after block indentation, applied to every layout entry before the sweeps | What the rewriter writes for that line, and FMT-PIPE-017 (offset 0 behind a comment tail) | Agree on: equals value (offset 6); arrow; a misindented statement led by `/* c */` (first token stays); a two-space class (reading X); a statement whose first token stands behind a tail in its own leading trivia (offset 0, matching document level); a mid-line target whose leading trivia an earlier phase breaks (the restore returns the token to `docCol`, and the lines aligned to it were computed from `docCol`) | holds |
| The first token's document column (`ComputeDocumentColumn`) | A tab counts 4 only inside a whitespace trivia of the token's own leading trivia, and only when the entry point writes that trivia back (`!keepsLeadingTrivia`); a tab inside a comment counts 1; U+FEFF at text position 0 counts 0 | `TabReplacementPhase` (own whitespace replaced); FMT-TAB-003 (comment tabs kept); the layout's zero-width BOM; decision C1 B (whitespace kept outside the target counts 1) | Agree, covered by the tests of a01, n1, the C2a input, and a direct child of the root (unchanged from before) | holds |
| Restoring the target's leading trivia | `FormatNodeInDocumentAsync`: `keepsLeadingTrivia` = direct child of root, or not starting its line. `FormatNodeInDocumentWithContextAsync`: always | FMT-PIPE-010's keep condition; FMT-PIPE-017's exemption | The first entry point agrees; the context entry point restores more than the exemption allows | finding #8 |
| `///` written behind the preceding token of a detached root (`IsWrittenBehindRootPrecedingToken`) | `RootPrecedingToken.Exists`; `EndsLine` false; the owning token has no previous token in the tree; the comment is on the root's first line | `IsWrittenBehindSource` on the document text (the attached case); `DocumentsFollowingCode` replacing `IsStrandedBehindCode` (equivalent once "written behind" holds) | Agree on: `{ /// Doc` (type, method), `case 1: /// Doc`, a preceding token whose trailing trivia ends in a multi-line comment tail. Previous-member shapes go through the text path and still hold. The root's first token always opens its parent node, so the stranded branch cannot differ | holds |
| BOM column for the raw-string move (`GetOpeningQuotesColumnBeforeRawStringShift`) | +1 on line 0 when `text[0]` is U+FEFF, the line is placed, and its text starts on it | RH5202's character columns; the BOM handling in `IndentationRewriter.AddLineStartWhitespace` | Agree on the four BOM probes. Other alignments keep the zero-width BOM (decision `third-amendment` Q1 B); see *Hints* | holds |
| Cloning an attached root (`IndentationPhase.Execute`) | `root.Parent != null` | What the pipeline hands over | The pipeline never hands over an attached root in the probes, so the clone is defensive | finding #9 |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Node-level column | `rg "AdjustNodeIndentation\|ComputeTokenColumn\|RootFirstTokenColumn\|ComputeDocumentColumn" src`: `ReihitsuFormatterHelpers.ComputeDocumentColumn` counts and `LayoutComputer.ShiftToRootFirstTokenColumn` applies. `ComputeTokenColumn` remains only for `TargetAttributePlacementCodeFixProviderBase` (attribute insertion column, a different decision). `AdjustNodeIndentation` is gone. The context entry point's restore still decides the target's first line | one owner, apart from the restore (finding #8) |
| BOM column on line 0 | `rg uFEFF src/Reihitsu.Formatter`: the layout and `ComputeDocumentColumn` count it as zero width; `GetOpeningQuotesColumnBeforeRawStringShift` counts it as one column, for the raw-string move only; the rewriter writes BOM plus layout | two conventions on one line, justified by decision `third-amendment` Q1 B; raw strings agree with RH5202 |
| `///` behind the preceding token of a node-level root | `DocumentationCommentFormattingPhase`: `IsWrittenBehindSource` (text) and `IsWrittenBehindRootPrecedingToken` (facts). `CommentIndentationContributor` and the blank-line rewriters use `PrecedingTokenFacts.Resolve` | consistent; `FormattingContext.RootPrecedingToken` is the one source of the detached facts |
| Withholding the RH5303/RH5304 fix | `rg "ContainsComplexElementBehindMultiLineCommentTail\|StartsLineInsideComment\|RH5303-006\|RH5304-005" src`: nothing outside the retired spec headings and the ledger | removed |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `ReihitsuFormatterNodeColumnTests` (9 tests) | The node-level output has the phases' columns, the first token stays in place, and outside tabs count 1 while own tabs count 4 | A line off its column, or a second pass that changes the output | `AssertFormatsTarget`/`…WithContext`: exact output, a second pass, LF and CRLF; `AssertRuleResult` where document-level parity is stated | adequate. The context case covers only a mid-line target (finding #8) |
| RH5301/RH5303/RH5304 `…IsFixedAsTheFormatterLaysItOut` (auditor members, rewritten tail tests) | The fix output equals the document-level output | The output differs, or the result is reported | `VerifyCodeFixMatchesFormatter`: markup diagnostics, one application, equality with `Format(source)`, then `Verify(fixedSource)` expecting no diagnostic | adequate |
| RH5303/RH5304 Fix All on two tail creations; v1/v2 examples | Every creation is fixed in one application | A remaining diagnostic | verifier with fixed source; `MarkupMode.Allow` removed | adequate |
| RH3202 a01, a02, n4 (both halves), h1 | Tab-indented and two-space members; raw strings under their quotes; hole lines moved with the literal | Wrong column | `Verify(source, fixed)` | adequate |
| `ReihitsuFormatterNodeBlankLineTests …WhenTheTargetHoldsATab` / `…IsIndentedWithTabs` | The `///` is moved on a detached root | The comment is kept behind `{` or the label | `AssertFormatsTarget`: two passes, LF and CRLF | adequate |
| `ByteOrderMarkTests.RawStringOnTheLineOfTheByteOrderMark…` | The literal at character column 9 | Column 8 | `AssertRuleResult` | adequate |
| `RawStringInterpolationHoleTests.CommentAboveCommentLedHoleLine…`, `CommentLedLineAlignmentTests.CommentAboveCommentLedLine…` | The comment takes the line column | The comment lands under the token | `AssertRuleResult` | adequate |
| `IndentationPhaseRawStringMoveTests.MisalignedRawStringInAttachedMethodBelowTheFirstLineIsCorrected` | An attached node holding a raw string is laid out correctly | — | phase-level exact output | weak: it passes without the `lineOffset` branch it was written for, and also without the clone (finding #9) |
| (missing) | The context entry point on a target that starts a later line of the context | — | — | no test (finding #8) |

## Failure classes dismissed

- FC-002, FC-003: FMT-PIPE-017's qualifiers (a comment tail, a kept outside tab, own whitespace, the BOM) are explicit predicates. The tests cover both sides of each.
- FC-004, FC-005, FC-019, FC-020, FC-021: the repair moves trivia only in the documentation relocation, which uses the same `MoveBeforeOwningToken` as the attached path.
- FC-006, FC-018, FC-023–FC-025, FC-029, FC-036: no syntax rewrite, rename, or member-kind logic changed.
- FC-008, FC-009: the Fix All tests were rewritten to fix every creation. No fix is withheld, and every probe fix changes the document.
- FC-010, FC-011, FC-012: the node-level tests run a second pass under LF and CRLF. No loop was added.
- FC-013: I ran RH5203, RH5301, and RH5303 on the probe fix outputs, and every probe output equals the document-level output. The one exception is #8(a), where RH5203 reports the RH5111 output.
- FC-014: no new rule.
- FC-022, FC-027: I probed plain and interpolated raw strings and comment tails with equals-value, arrow, statement, lambda (RH5305), assignment (RH5304), and member (RH3202) targets. Each fix output equals the document-level output, or differs only where reading X and outside code (blank lines) are decided.
- FC-026: no token is resolved again by position.
- FC-030: the repair does not change which target a fix picks.
- FC-032: the implementer ran every project, including the self-hosting scans, on 9081b4c. Later commits only remove a dead helper and its tests; the cleaner and the implementer re-ran formatter, core, and analyzer afterwards.
- FC-033, FC-034, FC-035: tabs are written as escapes, the auditor's shapes are used verbatim, and the expected values are document-level output.
- FC-037: no comment-only proof was relied on.
- FC-031: I checked the changed summaries. Only the `IndentationRewriter` comment is off (finding #9).
- FC-015, FC-016, FC-001, FC-017, FC-007, FC-028: reported above.

## Architecture findings

**A node-level entry point still decides the column of a written-back line after the last phase.** This repeats the class of #1.

- **Components:**
  - `ReihitsuFormatter.FormatNodeInDocumentWithContextAsync`: its unconditional restore of the target's first token's leading trivia;
  - `IndentationPhase` (`LayoutComputer`, `IndentationRewriter`): it places that line and aligns the target's other lines to anchors on it.
- **Shared decision.** The column of the target's first line, when the target starts a later line of the context, and with it the columns of every line aligned to an anchor on that line.
- **Inputs that show it.** #8(a): RH5111 on a later declarator, whose result RH5203 reports. #8(b): the public API with an argument on a later line.
- **Relation to #1.** It is the same decision, failing between the same two components (a node-level step after the pipeline against the indentation phase), in an input shape #1's enumeration did not name. Under `squad/README.md`, *A repeated class is an architecture signal*, this is escalation 12.

## Hints

- On a line 0 led by U+FEFF, alignments other than the raw-string move still treat the BOM as zero width. The phase therefore writes `﻿Call(1,` / `     2);` with `2` at 5 while `1` is at character column 6. This is unchanged from the base and is left as it is by decision `third-amendment` Q1 B. A rule that counts characters would see the arguments one column apart. Only string input reaches it.
- `FormatNodeInDocumentWithContextAsync` sets no `RootPrecedingToken`. A target that starts the context and carries a `///` behind the preceding token keeps that comment where it is through the same restore as #8. Not verified by a probe.
- The cosmetic blank line after the new SHARED-FIX-007 bullet and the `squad/failure-classes.md` history paragraph remain, as the spec checker noted.
