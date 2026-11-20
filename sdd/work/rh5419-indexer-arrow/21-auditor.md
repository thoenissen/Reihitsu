Role: auditor
Commit: e6151e9f5c58846974107faec9ab5bf5509ec439
Verdict: pass, text cleanup

Inputs: thoenissen/Reihitsu#851 (`agent-filed`), `00-lead.md`, `16-auditor.md`, and the repair handovers `17-spec-author.md`, `18-implementer.md`, `19-test-implementer.md`, `20-spec-checker.md`. The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

- Head `e6151e9f5c58846974107faec9ab5bf5509ec439` (checked out). `origin/main` is `31d2931af8e2894fba9e66a281edd23b4de9ae6c` (`git fetch` agrees with the prompt) and is an ancestor of the head (`git merge-base --is-ancestor`).
- Repair delta against `84a0f1e`. Outside `sdd/work/`, the delta changes six files: the change doc, `sdd/specs/analyzer/RH5419.md` (RH5419-004 and its withheld example), `AttributeListCodeFixUtilities` (new `StandsOnOneLineKeepingBlankLines`), the RH5401 and RH5419 code fix providers (each private one-line check replaced by a call to the shared method), and the RH5419 analyzer tests (three new withheld tests and one spec-example indexer). This matches the handovers. The analyzer, the formatter, the rule page, and every other consumer are unchanged. The scope did not grow: the spec change is the clarification #4 asked for, and its governing condition is unchanged.
- Executed: `scripts/apply-fix.sh RH5419` over six new fixtures and `scripts/apply-fix.sh RH5401` over two, in the session scratchpad, LF and CRLF. Result: RH5419 6 fixed, 6 no-fix-offered; RH5401 2 fixed, 2 no-fix-offered; 0 not-converged, 0 line-ending drift in both. The fixtures appear in the sections below. Evidence from `16-auditor.md` on code the repair did not touch is reused.

## Previous findings

- #1, #2 — **closed** in `16-auditor.md`; the repair does not touch `JoinParameterList` or the trivia guards.
- #3 — **closed**. `AttributeListCodeFixUtilities.StandsOnOneLineKeepingBlankLines(MemberDeclarationSyntax, MemberDeclarationSyntax)` is now the one owner of the start token, the line-break test, and the blank-line test. RH5401 calls it with `(propertyDeclaration, formatted)` and RH5419 with `(indexerDeclaration, formatted)`, the same arguments their private copies passed to `KeepsBlankLinesBehindAttributeLists`. Behavior is unchanged by trace. For RH5401, the start token, the end `formatted.Span.End`, and the `'\n'` search over the range are identical to the removed `Substring`/`IndexOf`. For RH5419, the end moves from `SemicolonToken.Span.End` to `Span.End`, which is the same position for an expression-bodied indexer with its `;`. `rg -n "IndexOf\('\\\\n'" src/Reihitsu.Analyzer.CodeFixes` and the start-token `rg` find only the shared method among the fixes. The two Core sites the handover names (`ConditionalGroupUnitUtilities.cs:233`, a token-identity match for unit lists, and `DeclarationModifierUtilities.GetFirstTokenBehindAttributeLists`, which returns the first modifier for modifier insertion) navigate to a similar token for other decisions, and do not own the one-line span. I accept the implementer's reasoning.
- #4 — **closed**. RH5419-004 now states the formatted-text condition as the rule ("whatever keeps a line break there") and marks the causes as examples ("Causes include …"). The list names a line break in front of `this` that the formatter keeps. The change doc's *What* and a new decision state the same. The tests pin the members B1, B2, and B4 under LF and CRLF, and the spec example adds the `decimal` indexer. One phrase of the amended text overstates the contrast; that is a repeat, finding #5.

## Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 5 | low | `sdd/specs/analyzer/RH5419.md` RH5419-004: "… and a bare line break between `this` and the `[`, which the formatter joins (FMT-BREAK-011), unlike a line break in front of `this`, do not withhold the fix" | – (spec text) | #4 | same requirement | The contrast "unlike a line break in front of `this`" is unqualified, so it reads as if every line break in front of `this` withholds the fix. The formatter joins some of them, and the fix is then offered, as the governing clause requires. Observed with `apply-fix.sh` (LF and CRLF): `List<⏎ int> this[int a,⏎ int b] => null;` is fixed to `List<int> this[int a, int b] => null;` in one iteration (C4). The code is right, and only the sentence is wrong. This is the same decision as #4: the spec's wording on which line breaks withhold the fix, compared with which ones the formatter joins. The text cannot reach compiled behavior, diagnostics, or rule metadata, so the verdict stays `pass, text cleanup`. | Qualify the contrast, for example "unlike one the formatter keeps in front of `this`". No test is needed: C4 follows the governing clause, and neither the spec nor the change doc gives it as an example. |

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Offer only when the formatted member stands on one line (`StandsOnOneLineKeepingBlankLines`) | Formatted text from `SpanStart` of the first token behind the attribute lists (or `GetFirstToken()` when there are none) to `formatted.Span.End`. The leading trivia of that token and the trailing trivia of the last token are excluded. | (1) RH5401-004 and RH5419-004: "from its first token behind its attribute lists through its `;`". (2) The removed private copies (identical start; end `Span.End` for RH5401, and `SemicolonToken.Span.End` for RH5419, the same position). (3) Maintainer answer D: the whole declaration on one line. | Same span on all three. Probes on the new causes: `public⏎ virtual int this[…]` (C1), `int I⏎ .this[…]` (C2), and `[Obsolete]⏎ public⏎ int this[…]` (C3) are withheld. A line break inside the return type that the formatter joins (C4), a bare line break between `this` and `[` (C5), and `]⏎ => a` (C6) are each offered as one line, in one iteration. RH5401 parity: `public⏎ int P⏎ => 1;` is withheld (P1), and `[Obsolete]⏎ public int P⏎ => 1;` is fixed with the list kept on its own line (P2). | holds |
| Blank lines behind attribute lists | Unchanged `KeepsBlankLinesBehindAttributeLists`, now reached through the shared method with the same arguments | — | evidence from `11-auditor.md` / `16-auditor.md` reused | holds |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| The one-line span of a single-line fix for an expression-bodied member (RH5401-004, RH5419-004) | `AttributeListCodeFixUtilities.StandsOnOneLineKeepingBlankLines` (`AttributeListCodeFixUtilities.cs:43`), called by both providers. `rg -n "AttributeLists\.Last\(\)\.GetLastToken\(\)\.GetNextToken\(\)" src --glob '!*Test*'` also finds `ConditionalGroupUnitUtilities.cs:233`, a token match for another decision. `DeclarationModifierUtilities.GetFirstTokenBehindAttributeLists` (private) serves modifier insertion. `rg -n "IndexOf\('\\\\n'" src/Reihitsu.Analyzer.CodeFixes` finds only the shared method. | one owner, ok (#3 closed) |
| Blank lines behind attribute lists | `AttributeListCodeFixUtilities.KeepsBlankLinesBehindAttributeLists`, which the shared method calls | one owner, ok |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `VerifyFixIsWithheldForALineBreakBetweenTheTypeAndThis` | A line break the formatter keeps between the type and `this` withholds the fix, and the diagnostic stays | Any registered action, or a different diagnostic span | `VerifyNoCodeFixIsOffered` (exact markup, every diagnostic's actions checked), LF and CRLF | adequate |
| `VerifyFixIsWithheldForALineBreakBetweenAModifierAndTheType` | The same, for a line break between a modifier and the type. It also pins that the span starts at the first token, not at the type. | as above | as above | adequate |
| `VerifyFixIsWithheldForALineBreakBetweenAnExplicitInterfaceNameAndThis` (`int I.⏎ this[…]`) | The same, for an explicit interface name | as above | as above | adequate |
| `VerifySpecificationExampleOfWithheldFixes` (adds the `decimal` indexer) | The spec example holds | An action for the new indexer | as above | adequate |
| RH5401 and RH5419 tests, through the shared method (218 pass, `18-implementer.md`) | The refactoring keeps both rules' offer decisions | A changed offer on any existing case | existing helpers | adequate. The refactoring is equivalent by trace, and P1/P2 confirm it on new inputs. |

## Failure classes dismissed

Delta only. Classes `16-auditor.md` dismissed on code this repair did not touch keep their evidence.

- FC-001, FC-002, FC-003: traced in *Guard scope*. Both sides of the "line break in front of `this`" boundary are covered: kept and withheld (tests, C1–C3), joined and offered (C4, C5).
- FC-004, FC-005, FC-019–FC-021: no rewrite changed. The shared method reads only formatted text.
- FC-006–FC-008, FC-022, FC-024–FC-028: offered arms converge in one iteration with no line-ending drift (C4–C6, P2). Fix All goes through the same per-diagnostic offer check.
- FC-009, FC-043: `FindUnsupportedSyntax` is unchanged and asks about the same node.
- FC-010–FC-014, FC-032, FC-035, FC-038–FC-042: no formatter change and no declared shape. The new tests assert no output taken from a patched copy.
- FC-015, FC-016: #3 closed. No private copy of the check is left in either provider.
- FC-017: see *Assertion adequacy*.
- FC-031, FC-037: the new method's summary and inline comment describe it. The RH5401 summary now points at the shared method. The RH5419 summary and inline comment name some causes, and claim no complete list (see *Hints*). No `TODO`, no commented-out code, and no tracker number in the added lines.
- FC-044: the offer decision reads the formatter's output and models no layout.
- All others: unchanged code paths, as dismissed in `11-auditor.md` and `16-auditor.md`.

## Architecture findings

_None._

## Hints

- The inline comment above the `return` in RH5419's `FormatsOnOneLineKeepingBlankLines` repeats, in shorter form, the comment inside `StandsOnOneLineKeepingBlankLines`. It does not name the line break in front of `this` that RH5419-004 now lists. The comment is accurate and does not claim to be complete, so it is not a finding. It could be dropped when the text of #5 is cleaned up.

## Follow-up candidates

_None_ beyond those already in `00-lead.md`.
