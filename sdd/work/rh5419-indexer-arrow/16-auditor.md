Role: auditor
Commit: 84a0f1efdd0ec442a5ccffc21d6742b2c9609cbf
Verdict: findings

Inputs: thoenissen/Reihitsu#851 (`agent-filed`), `00-lead.md` (decisions 6–8), `11-auditor.md`, and the repair handovers `12-spec-author.md`, `13-test-implementer.md`, `14-implementer.md`, `15-spec-checker.md`. The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

- Head `84a0f1efdd0ec442a5ccffc21d6742b2c9609cbf` (checked out). `origin/main` `31d2931af8e2894fba9e66a281edd23b4de9ae6c` is an ancestor of the head (`git merge-base --is-ancestor`).
- Repair delta against `57a8dea`. Outside `sdd/work/`, the delta changes the change doc, `sdd/specs/analyzer/RH5419.md` (RH5419-004 and its examples), the RH5419 code fix provider (`FormatsOnOneLineKeepingBlankLines`, `JoinParameterList`), and the RH5419 analyzer tests. This matches the handovers. The analyzer, the formatter, the rule page, and the shared `AttributeListCodeFixUtilities` are unchanged. The scope did not grow: the spec change is the one decision 6 ordered.
- Executed: `scripts/apply-fix.sh RH5419` over 13 new fixtures in the session scratchpad, one indexer per file, LF and CRLF. Result: 16 fixed, 10 no-fix-offered, 0 not-converged, 0 line-ending drift. The fixtures are in the sections below. Evidence from `11-auditor.md` on code the repair did not touch is reused.

## Previous findings

- #1 — **closed**. The one-line check now measures from the first token behind the attribute lists through the `;`. Each member (a)–(d) has a withheld test: `VerifyFixIsWithheldForALineCommentBehindThis`, `…ForACommentOnALineOfItsOwnInFrontOfTheOpeningBracket`, `…ForADirectiveBetweenThisAndTheOpeningBracket`, and `…ForALineCommentBehindThisInFrontOfAParameterListOnOneLine`. The spec example tests cover them as well. Neighbors of the members are also withheld: `int // c⏎ this[…]` (B3) and `int I // c⏎ .this[…]` (B5). The members that held before still get a one-line fix: `this /* c */ [`, and a bare line break between `this` and `[` (test `VerifyFixPutsIndexersOnOneLine`).
- #2 — **closed**. `JoinParameterList` selects the gaps with `TokenGapAnalysis.Between(...).HasLineBreak` and joins them with `LineBreakTriviaUtilities.JoinGaps`. The private trivia rewrite and the inline `EndOfLineTrivia` check are gone (`rg -n EndOfLineTrivia` on the provider finds nothing). Gap shapes checked with `apply-fix.sh`, each giving one line: a blank line inside the list (B9), trailing whitespace on the joined lines (B10), and line breaks directly behind `[` and in front of `]` (B13).

## Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 3 | medium | `RH5419…CodeFixProvider.FormatsOnOneLineKeepingBlankLines` (lines 142–157) and `RH5401…CodeFixProvider.FormatsOnOneLineKeepingBlankLines` (lines 57–70) | FC-015 | – | PR-introduced (this repair) | The one-line check that RH5419-004 defines as "the span RH5401's fix measures" now has two owners. Both compute the first token behind the attribute lists the same way: `AttributeLists.Count > 0 ? AttributeLists.Last().GetLastToken().GetNextToken() : GetFirstToken()`. Both look for `'\n'` from that token through the end of the declaration, and both then call `AttributeListCodeFixUtilities.KeepsBlankLinesBehindAttributeLists`. The handover's reason for not sharing it ("a two-line expression on different node types", `14-implementer.md` *Policy owners*) does not hold. Both nodes are `MemberDeclarationSyntax` with `AttributeLists`. `AttributeListCodeFixUtilities.KeepsBlankLinesBehindAttributeLists(MemberDeclarationSyntax, MemberDeclarationSyntax)`, which both providers already call, takes exactly that pair. The handover's search also missed `ConditionalGroupUnitUtilities.cs:233` and the private `DeclarationModifierUtilities.GetFirstTokenBehindAttributeLists(MemberDeclarationSyntax)` (`src/Reihitsu.Core/DeclarationModifierUtilities.cs:298`), which compute the same start token. Today the two checks agree on every input: the same start token, and the same end, because an expression-bodied indexer's `Span.End` is its `;`. Nothing fails yet. But the spec ties the two rules together, so a change to RH5401's span would silently leave RH5419 behind. | Move the check (start token, the line-break test through the end, and the blank-line test) into one method of `AttributeListCodeFixUtilities`, called with the original and formatted `MemberDeclarationSyntax`. Both providers call it, and the two private copies of the start-token expression go away. |
| 4 | low | `sdd/specs/analyzer/RH5419.md` RH5419-004 ("would still span several lines: because a literal …, because the formatter keeps the expression wrapped, or because a comment, a directive, or disabled text in front of the `[` …"); the change doc's *What*; no test | – (spec text, untested member) | – | same requirement | The code correctly withholds the fix when the formatter keeps a bare line break in front of the `[`, as the governing clause requires. RH5419-004 lists its causes after a colon, as if the list were complete, but this cause is not on it. The spec names the opposite side, a bare line break between `this` and `[` that the formatter joins, so a reader could take every bare line break to keep the fix. Withheld, observed with `apply-fix.sh` (LF and CRLF): `public⏎ int this[int a,⏎ int b] => a;` (B1), `int⏎ this[int a,⏎ int b] => a;` (B2), and `int I.⏎ this[int a,⏎ int b] => a;` (B4). Before this repair, these were offered as two-line results that finding #1 did not list. Now the code follows the spec's governing clause. No test pins any of them. Nothing visible is in question: the input is rare, and withholding is what decision 6 ordered. | In RH5419-004, name the cause: a line break in front of the `[` that the formatter keeps, such as one between a modifier, the type, an explicit interface name, and `this`. Alternatively, mark the list as examples. Add one withheld test, for example `int⏎ this[int a,⏎ int b] => a;`, under LF and CRLF. |

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Offer only when the formatted code is one line (`FormatsOnOneLineKeepingBlankLines`) | Formatted text from the first token behind the attribute lists (`SpanStart`, so its leading trivia is excluded) to `;`.`Span.End` | (1) RH5401's `FormatsOnOneLineKeepingBlankLines`: same start token, through `formatted.Span.End`. (2) RH5419-004: "from the first token behind its attribute lists through its `;`", with leading trivia and attribute lists not measured. (3) Maintainer answer D: the whole declaration on one line | Same start and end on all three. Both sides of each boundary are pinned. A line comment behind a modifier is withheld (`VerifyFixIsWithheldForALineCommentBehindAModifier`, which also rules out a span that starts at the type or at `this`). A documentation comment keeps the fix (the `short` spec example, which rules out a span that starts at `FullSpan.Start`). An attribute list on its own line keeps the fix (`VerifyFixPutsAnIndexerWithAttributeListsOnOneLine`). Probes: `[Obsolete] // c⏎ int this[…]` is offered, and the comment stays behind the list (B6). `[Obsolete] int this[…` is offered, with the list on its own line (B7). A block comment in front of the first token (B8) and one between a modifier and the type (B11) are offered as one line. | holds (duplication: #3) |
| Which gaps the fix joins (`JoinParameterList`) | Each gap from `[` through `]` where `TokenGapAnalysis.Between(token, next).HasLineBreak` | (1) `HoldsCommentDirectiveOrDisabledTextInWrappedParameterList`: the same gaps, the same exclusion of the `[`'s leading trivia and the `]`'s trailing trivia, and it runs first through `&&` short-circuiting in the offer path, so the apply path only reaches inputs the guard admitted. (2) The precondition of `JoinGaps` ("the caller … has already refused every gap that must keep its line break") | Same span. A list whose `[` and `]` share a line yields no gap, and the guard returns `false` for it. Every joined gap holds only whitespace and line breaks, so `CreateJoinedTrailingTrivia` and `RemoveLeadingEndOfLineAndWhitespace` remove the whole gap, as the old private rewrite did. The output is the same, and the RH5419 and RH5401 tests pass (`14-implementer.md`). Adjacent gaps that share a token (`a,⏎b,⏎c`) are composed by `JoinGaps`, which reads `replacements[token]` first. | holds |
| Other guards (trivia inside the list, attribute lists, unsupported shapes) | unchanged since `11-auditor.md` | — | evidence reused | holds |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Joining gaps onto one line | `LineBreakTriviaUtilities.JoinGaps`, called by four formatter rewriters and now by the RH5419 provider (`rg -n "JoinGaps\(" src --glob '!*Test*'`) | one owner, ok (#2 closed) |
| Whether a gap holds a line break | `TokenGapAnalysis.Between(...).HasLineBreak`, also used by `TokenGapUtilities.HasLineBreakBetween` | one owner, ok |
| The one-line span of a single-line fix for an expression-bodied member (RH5401-004, RH5419-004) | `RH5401…CodeFixProvider.FormatsOnOneLineKeepingBlankLines:57-70` and `RH5419…CodeFixProvider.FormatsOnOneLineKeepingBlankLines:142-157` (`rg -n "AttributeLists\.Last\(\)\.GetLastToken\(\)\.GetNextToken\(\)" src --glob '!*Test*'`, which also finds `ConditionalGroupUnitUtilities.cs:233`); `DeclarationModifierUtilities.GetFirstTokenBehindAttributeLists` (private, Core) computes the same start token | two owners of the check, finding #3 |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `VerifyFixIsWithheldForALineCommentBehindThis`, `…ForACommentOnALineOfItsOwnInFrontOfTheOpeningBracket`, `…ForADirectiveBetweenThisAndTheOpeningBracket`, `…ForALineCommentBehindThisInFrontOfAParameterListOnOneLine` | A line break that trivia in front of `[` keeps withholds the fix, and the diagnostic stays | Any action registered: the old span starting at `[` registers one for all four, which the 12 failures recorded in `13-test-implementer.md` show | `VerifyNoCodeFixIsOffered` (exact markup, actions checked for every diagnostic), LF and CRLF | adequate |
| `VerifyFixIsWithheldForALineCommentBehindAModifier` | The measured span starts at the first token behind the attribute lists, not at the type or at `this` | An action registered when the span starts later | as above | adequate |
| `VerifySpecificationExampleOfWithheldFixes` (four new indexers) | The spec example holds | An action for any of them | as above | adequate |
| `VerifySpecificationExampleOfBlockCommentsThatDoNotWithholdTheFix` (documentation comment added) | The leading trivia of the first token is not measured, and the documentation comment stays above the one-line indexer | No fix (a span from `FullSpan.Start`), or a different document | `Verify` (whole document, Fix All), LF and CRLF | adequate |
| (missing) a bare line break the formatter keeps in front of `[` | — | — | — | see #4 |

## Failure classes dismissed

Delta only. Classes `11-auditor.md` dismissed on code this repair did not touch keep their evidence.

- FC-001, FC-002, FC-003: traced in *Guard scope*. Both sides of the new span boundary are pinned by tests.
- FC-004, FC-019, FC-020, FC-021: `JoinGaps` keeps every trivia that is not whitespace or a line break. It only receives gaps the guard has proven to hold nothing else, so a drift of the guard would now keep comments rather than delete them. The exposure `11-auditor.md` #2 named is gone. Comments in front of the first token (B8), between a modifier and the type (B11), and behind an attribute list (B6) stay in place. Comments and directives that keep a line break in front of `[` are withheld (B3, B5, and the tests).
- FC-005: the new withheld class covers comments, directives, and disabled text alike, because it reads the formatted text, not a trivia kind.
- FC-006, FC-022, FC-024–FC-027: the repair changes no token rewrite. `JoinGaps` writes the same minimal separator (`CreateJoinSeparator`).
- FC-007, FC-008: an offered fix still yields a one-line text, now over a wider span, so RH5419 is silent on it. Fix All goes through the same per-diagnostic offer check (`VerifyFixAllLeavesTheIndexersWhoseFixIsWithheld`).
- FC-009, FC-043: `FindUnsupportedSyntax` is unchanged and asks about the same node.
- FC-010–FC-014, FC-032, FC-035, FC-038–FC-042: no formatter change and no declared shape. The withheld tests assert no output that a patched copy could have produced.
- FC-015, FC-016: #2 closed. A new second owner is #3.
- FC-017: see *Assertion adequacy*.
- FC-028: the wider span now includes modifiers, the type, and `this`. The formatter's line joins there do not depend on columns, and the offer check (`FormatNode`) and the application (`FormatNodeInDocumentAsync`) agree on every probe: all 16 offered arms converged in one iteration.
- FC-031, FC-037: the summaries of `JoinParameterList` and `FormatsOnOneLineKeepingBlankLines`, and the inline comment, describe the new code. There is no `TODO`, no commented-out code, and no tracker number.
- FC-044: the offer decision reads the formatter's output and models no layout.
- All others: unchanged code paths, as dismissed in `11-auditor.md`.

## Architecture findings

_None._ #3 is a private copy of a three-step check next to the shared utility that already owns its third step. Sharing it is a narrow move into `AttributeListCodeFixUtilities` and needs no structural change.

## Hints

- `14-implementer.md` *Policy owners* reports the `rg` for the start-token expression as finding "these two". The same search also finds `ConditionalGroupUnitUtilities.cs:233`.

## Follow-up candidates

_None_ beyond those already in `00-lead.md`.
