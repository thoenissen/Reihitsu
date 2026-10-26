Role: auditor
Commit: 950f713
Verdict: findings

My prompt carried no intended fix, no suspected cause, and no verdict anyone hoped for.

## Scope

Repair audit of round 3, the run's last repair round. Head 950f713e085273ed029aff966402ca415ac37faa (checked out, clean). `git ls-remote origin refs/heads/main` gives 929fc232447d305a2400606fd7a8acdf69cea7c0, which is unchanged and merged. The repair diff is `git diff 504310f HEAD`. It matches handovers 29–36 and the decisions `audit-2` and `context-blank-lines`.

The repair moved:
- `FormatNodeInDocumentWithContextAsync`: the restore condition, the first-token column, and the new write-back through `RestoreBlankLines`;
- `ReihitsuFormatterHelpers.RestoreBlankLines` and its helpers;
- the `lineOffset` removal in `IndentationRewriter`, and a comment in `IndentationPhase`;
- FMT-PIPE-017 wording, the new FMT-PIPE-018, the AGENTS sentence, and the tests.

I audited all of that, and the counterparts it reaches: the callers of the context entry point (the RH5111 fix, `FluentChainCodeFixHelper` for RH5201/RH5112, and RH3208), the documentation phase's relocation, and the shared predicate for "starts its line". Fixtures are in my scratchpad, outside the repository. `→` is a tab, `/` separates lines, and columns are 0-based. Every `apply-fix.sh` result holds under both LF and CRLF.

## Previous findings

| # | Status | Evidence on 950f713 |
|---|---|---|
| 1 | closed (unchanged since 28) | — |
| 2 | closed for `FormatNodeInDocumentAsync`; its class fails again through the context entry point — see #11 | — |
| 6 | superseded by #11 | The AGENTS sentence now says no step after the last phase changes a line's column. #11 writes back the indentation phase's line-start whitespace into the middle of a line, which changes the column of the comment behind it. |
| 8 | closed | RH5111 on the later declarator (`b` at 8, `2` at 18 under `1`); the public-API argument target (`Other` at 13, `3` at 19 under `2`); the parameter at 11; the enum member at 4. Each equals the document-level output. |
| 9 | closed | The `lineOffset` mapping is gone. The clone is the only mechanism left, and `ReihitsuFormatterNodeColumnTests.RawStringLiteralTargetMovesContentUnderTheOpeningQuotes` is a test through the public API that reaches it: a literal target arrives at the indentation phase still attached. Handovers 29 and 30 show both tests failing once the clone is disabled. |
| 10 | closed by decision `audit-2` #10 A | Kept; its removal is a follow-up candidate. |

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 11 | high | `ReihitsuFormatter.FormatNodeInDocumentWithContextAsync`, the `keepsLeadingTrivia` condition (`StartsOnNewLineIncludingDocumentation`, which counts a `///` in the leading trivia as starting the line) and the write-back through `ReihitsuFormatterHelpers.RestoreBlankLines`, whose first gap is assumed to start its line. Counterparts: `DocumentationCommentFormattingPhase.RegisterRelocation`, which puts the line break into the *previous* token when that token lies inside the context root, and `FluentChainCodeFixHelper.HasSameChainLayoutAsync`, which compares the chain without its leading trivia | FC-001, FC-015, FC-016, FC-021, FC-028, FC-007 | #2, #8 | PR-introduced (by the #8 repair, 8316e8a/8cb740c) | The target's first token carries a `///` written behind the token in front of it, and that token lies inside the context. The condition treats the target as starting its line, so the formatted leading trivia is written back. In the context tree, the documentation phase moved the comment by appending a line break to the previous token, which is not written back, and the indentation phase put line-start indentation in front of the comment. `RestoreBlankLines` keeps that indentation, because gap 0 has zero line breaks on both sides. So the indentation lands in the middle of the document line, and the comment stays behind the code. This breaks FMT-INDENT-006 ("MUST NOT receive indentation in the middle of its line"), FMT-PIPE-017/018 (the line break the phases add is not written back; the comment does not get the phase's column), and SHARED-FIX-007 (document-level formatting moves the comment onto its own line). **Fails:** RH5111 on `First, /// Doc` / `Second =` / `2,` gives `First,     /// Doc` (4 spaces inserted). RH5111 on `void M(int first, /// Doc` / `int second =` / `1)` gives `int first,            /// Doc` (11 inserted). RH5201 on `{ /// Doc` / `a.Trim()` / `.ToUpper();` gives `{         /// Doc` (8 inserted; context: the method body). RH5201 on `var x = 1; /// Doc` / `a.Trim()` / `.ToUpper();` gives `var x = 1;         /// Doc` (8 inserted). The base (c49431e) leaves every one of these lines as written, and so did 504310f, which restored the original trivia. **Hold:** RH5111 on a declarator behind `int a = 1, /// Doc` (no fix is offered); `/* c */` behind the previous member (it is that token's trailing trivia); every `FormatNodeInDocumentAsync` shape, whose relocation inserts the line break into the target's own leading trivia and which strips the preceding whitespace (FMT-PIPE-010); every comment, directive, and blank-line member in `32-test-implementer.md`. No test puts a `///` behind the token in front of a context target (the hint of `28-auditor.md`, still unprobed by the roles). | This repeats #2's class (the relocation of a `///` behind the token in front of a node-level target, between the documentation phase and the entry point's write-back). It also repeats #8's (the context entry point's write-back decides a written-back column), and this was repair round 3. So it is escalation 12, and escalation 7 too. The structural cause is under *Architecture findings*. Any repair needs a test per member above. |

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Whether the context entry point writes back the original or the formatted leading trivia | `keepsLeadingTrivia` = direct child of root, or `StartsOnNewLineIncludingDocumentation(firstToken) == false` | FMT-PIPE-010's keep condition; the documentation phase's relocation, which puts the break into the previous token when that token is inside the root; `TokenGapAnalysis.StartsLineAtLeadingTriviaIndex` | FMT-PIPE-010 agrees by its wording ("starts its line"). For a `///` behind a token inside the context, though, the document line does not start at the target's leading trivia. `FormatNodeInDocumentAsync` copes, because its relocation inserts the break into the target's own trivia and the entry point strips the preceding whitespace; the context entry point does neither | finding #11 |
| Which gaps of the leading trivia start their line (`RestoreBlankLines`) | gap 0 always; any gap behind an item whose text ends with `\n` (a directive, a `///`) | `TokenGapAnalysis.StartsLineAtLeadingTriviaIndex(token, index, hasPreviousToken, previousTrailingTrivia)` in `Reihitsu.Core`, which reads the previous token's trailing trivia and does not count line breaks inside comment text | Agree for gaps behind directives, `///`, `//` and `/* */` comments, and for gap 0 after a line break in the previous token's trailing trivia. Disagree for gap 0 when the previous token's trailing trivia holds no line break | finding #11 |
| How many line breaks a gap gets (`RestoreGapLineBreaks`) | the original count where the gap starts its line, or where both sides have a break; otherwise the formatted gap | FMT-PIPE-018 ("a line break that is not a blank line … follows the phases") | Agree on the 32/34 members (two blank lines, a comment, `#if`, `#region`, `///`, several comments, CRLF). Gap 0 in #11 is not a blank-line gap, yet the restore treats it as one | finding #11 |
| The context's first-token column | own tabs ×4 only when the context's first token is the target's first token and the trivia is not kept | FMT-PIPE-017, first bullet; C1 B | Agree; see *Hints* for a tab-indented context with a target on a later line | holds |
| The attached input of the indentation phase | the clone, when `root.Parent != null` | the raw-string move and the layout, which now share the clone's line numbers | One mechanism, reachable through the public API and falsified by two tests | holds |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| "Does this trivia start its line" | `TokenGapAnalysis.StartsLineAtLeadingTriviaIndex` (Core; used by `BlankLineTriviaBoundaryRewriter` and the indentation phase); `IndentationRewriter.ApplyIndentation` (`startsAtLineStart` from the previous token's trailing EOL); `ReihitsuFormatterHelpers.StartsOnNewLineIncludingDocumentation` (FMT-PIPE-010's condition); and new, `ReihitsuFormatterHelpers.RestoreBlankLines` (gap 0 always, then `EndsWithLineBreak`) | more than one owner, and the new private copy drifts from the shared predicate (finding #11) |
| Restoring the target's leading trivia after the pipeline | `rg "WithLeadingTrivia\(originalFirstToken\|RestoreBlankLines" src/Reihitsu.Formatter`: `FormatNodeInDocumentAsync` (original trivia under `keepsLeadingTrivia`) and `FormatNodeInDocumentWithContextAsync` (original trivia under the same condition, otherwise `RestoreBlankLines`) | one condition, but the two entry points differ in where the relocation's break lands and in the whitespace strip (finding #11) |
| Blank lines in front of a context target | `BlankLinePhase` decides within the context; `RestoreBlankLines` replaces those counts with the document's after the pipeline | justified by FMT-PIPE-018 and SHARED-FIX-006; the restore only copies counts |
| Line numbers of the layout and the raw-string move | `rg "lineOffset\|GetFirstLine" src/Reihitsu.Formatter`: nothing; the clone in `IndentationPhase.Execute` is the only mechanism | one owner |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `ReihitsuFormatterNodeColumnTests` later-line argument tests (#8 b) | The target's first line is in the phase's column, and the lines aligned to it follow | `Other` kept at 17 | entry point with two passes, LF and CRLF, plus `AssertRuleResult` | adequate |
| RH5111 `Verify…OnALaterLineIsFixedAsTheFormatterLaysItOut` (#8 a, c) | The fix output equals the formatter's | An input column is kept | `VerifyCodeFixMatchesFormatter` | adequate |
| FMT-PIPE-018 tests (11 entry-point tests and 10 RH5111 tests) | The document's blank lines stay, and columns are the phase's | A blank line added or collapsed, or a column off | exact output, a second pass, LF and CRLF (entry point); verifier (RH5111). Handover 32 shows them failing on both earlier states | adequate for their members. None puts a `///` behind the token in front of the target (finding #11) |
| `RawStringLiteralTargetMovesContentUnderTheOpeningQuotes`, `MisalignedRawStringInAttachedMethodBelowTheFirstLineIsCorrected` | The attached input is laid out correctly | Content not under the quotes | entry point / phase-level, exact output | adequate; each fails without the clone (handovers 29, 30) |
| `DirectChildOfRootTargetWithContextKeepsItsLeadingTriviaAsItStands` | The kept trivia is written back as it stands | Trivia rewritten | exact output | adequate |
| (missing) | A context target with a `///` behind the token in front of it | — | — | no test (finding #11) |

## Failure classes dismissed

- FC-002, FC-003: FMT-PIPE-018's qualifier (starts its line, trivia not kept) is tested on both sides, including the direct child of the root.
- FC-004, FC-005, FC-019, FC-020: `RestoreBlankLines` keeps every item; the fallback when items differ is unreachable (31, 32). Comments and directives in the trivia are covered by the 32 members.
- FC-006, FC-018, FC-022–FC-025, FC-029, FC-036: no rewrite or rename.
- FC-008, FC-009: no Fix All change, and no fix is withheld.
- FC-010, FC-011, FC-012: a second pass under LF and CRLF in the entry-point tests. No loop.
- FC-013: RH5203, RH5111, and RH5204 report nothing on the #8 outputs. For #11 I did not run the comment and spacing rules on the fix output; the finding stands on FMT-INDENT-006, FMT-PIPE-017/018, and SHARED-FIX-007.
- FC-014: no new rule.
- FC-017: covered above.
- FC-026, FC-027: no positional re-resolution. The enclosing scopes probed are an enum, a parameter list, a declaration statement, a method body, and a statement list.
- FC-030: the target choices are unchanged.
- FC-031: the changed summaries describe the code. One nit is under *Hints*.
- FC-032: after 8cb740c, only tests and specs changed (f955427 adds tags). The cleaner ran the full formatter and analyzer projects on f955427.
- FC-033, FC-034, FC-035: tabs are escapes, the auditor's shapes are verbatim, and expected values are document-level output or the stated specs.
- FC-037: none relied on.

## Architecture findings

**"Does this part of a node-level target's leading trivia start its line" is stated in more than one place, and the context entry point's copies disagree with the documentation phase's relocation.**

- **Components:**
  - `FormatNodeInDocumentWithContextAsync`: `keepsLeadingTrivia`, via `StartsOnNewLineIncludingDocumentation`;
  - `ReihitsuFormatterHelpers.RestoreBlankLines`: gap 0 always starts its line;
  - `DocumentationCommentFormattingPhase.RegisterRelocation`: puts the line break into the previous token when that token is in the root;
  - the shared `TokenGapAnalysis.StartsLineAtLeadingTriviaIndex`, which reads the previous token's trailing trivia.
- **Shared decision:** whether the line of a context target's first leading item starts in the target's leading trivia, and so whether the indentation the phases wrote there is line-start indentation that can be written back.
- **Input that shows it:** the four failing members of #11 (RH5111 enum member and parameter; RH5201 behind `{ /// Doc` and behind `var x = 1; /// Doc`).
- **Relation to earlier findings:** it is #2's decision, the relocation of a `///` behind the token in front of a node-level target, failing between the same components: the documentation phase, and the node-level entry point's write-back. It reached the code through the repair of #8, the context entry point's write-back. Under `squad/README.md` this is escalation 12. This was the third repair round, so it is escalation 7 too.

## Hints

- **Tab-indented context, target on a later line.** RH5111 on `→→int a = 1,` / `→→    b =` / `→→        2;` now writes `  b = 2;` (two spaces, under `int` in character columns), where 504310f and the base kept `→→    b = 2;`. This follows from C1 B together with #8 A:
  - the context's first token counts its tabs as one column each, while the target's own trivia is rewritten with spaces;
  - no rule except RH5601 reports either output;
  - formatting either result moves `b` to 8, which SHARED-FIX-007's whitespace bullet does not literally cover (the tab stands in front of the context's first line, not in front of the changed code's).

  The maintainer may want to see it with the C1 decision.
- The inline comment at the write-back in `FormatNodeInDocumentWithContextAsync` says "so no line moves after the last phase". FMT-PIPE-017 now says "changes no line's column", and `RestoreBlankLines` does move lines vertically. Wording only.
- The change doc still says "Three new pipeline requirements"; with FMT-PIPE-018 there are four (spec checker observation).
- `squad/failure-classes.md` history paragraph: unchanged, as noted before.
