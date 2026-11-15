Role: auditor
Commit: d8de284e3dec0f1f50efceba719c3ea7d0d2963e
Verdict: #1695: pass; #1696: pass; #1707: pass; #1721: pass

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

State: the checkout is `d8de284` with a clean tree. `origin/main` is `846aca462c94c0d85bc19a1d5ad9d78605590c29`, which is merged into the branch. The diff since `dff2aa9` is exactly what `09`–`12` describe: the RH3003 code fix (`TouchesWordToken` guard) and 11 RH3003 tests, plus work files. `scripts/lane-check.sh behavior-change` confirmed the lane.

## Scope
Repair delta against `dff2aa9` (head `d8de284`, base `846aca4`). The repair stays within RH3003's code fix and its tests, and it changes no spec. Scope did not grow, so this is a delta audit. Evidence for #1695, #1696, and #1721 is byte-identical to `08-auditor.md` (no file of theirs changed since `dff2aa9`), and their verdicts carry over.

Previous findings:
- #1 (high, RH3003 replacement glued to an adjacent word token): **closed**. Every member listed in #1 now holds. I observed this with `scripts/apply-fix.sh RH3003` under LF and CRLF, or it is pinned by a new test: `return""`, `yield return""`, `in""`, `select""`, `""is`, `""as`, `""switch`. The holding members still hold: punctuation on either side, and trivia between the literal and its neighbour.

Extra members I constructed for the class (axes: which side, neighbour kind, verbatim versus regular literal, trivia on the opposite side). All hold with `scripts/apply-fix.sh RH3003` under LF and CRLF, with 4 fixed and 0 line-ending drift:
- Query contextual keywords on both sides. `orderby""descending` → `orderby string.Empty descending`. Also `by""into`, `on""equals`, and `select""into`.
- Verbatim `return@""` → `return string.Empty`.
- The conditional `f?"":""` → `f?string.Empty:string.Empty` (no space, the author's spacing).
- `t==""&&""==t` gets no space.
- Word on one side with a comment on the other. `return""/* b */ + s` → `return string.Empty/* b */ + s`, and `return /* a */""is` → `return /* a */string.Empty is`. Comments are kept in place.

Not members: `""u8` is a `Utf8StringLiteralExpression`, which the analyzer does not register, so the literal's own boundary characters are always `"` or `@`. Constant contexts (`case""`, `s is""`) are not reported. Two reported literals separated only by a word token with no trivia cannot occur in reported code, so Fix All has no shared boundary.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| – | – | – | – | – | – | none | – |

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Space in front of `string` (`TouchesWordToken(previous, literal)`, `RH3003…CodeFixProvider.cs:50`) | The previous token's span end against the literal's span start (no trivia in between), and the previous token's last character as an identifier-part character | The C# lexer's token boundary: two adjacent tokens fuse only when both boundary characters are identifier-part characters. `string` starts with one, so the previous token's last character decides. | Agree. The literal's leading trivia is empty whenever the guard fires (the spans abut), so replacing it with one space drops nothing. | ok |
| Space behind `Empty` (`TouchesWordToken(literal, next)`, `:55`) | Literal span end against the next token's span start, and the next token's first character | Same. `Empty` ends with an identifier-part character. | Agree. The trailing trivia is empty when the guard fires. | ok |
| Missing or default neighbours | `RawKind == 0` and empty spans return false. `GetPreviousToken`/`GetNextToken` skip zero-width tokens. | – | Safe (no index into empty text) | ok |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Whether two adjacent tokens need a separating space | `rg "IsIdentifierPartCharacter\|IsIdentifierStartCharacter\|Span.End (==\|!=) .*SpanStart" src` (non-test) finds only `RH3003…CodeFixProvider.TouchesWordToken`. Fixes that delegate to the formatter get their spacing from `SpacingPolicy`. RH3001 and RH3006 build tokens without the formatter but wrap in parentheses (see `08`). | Single owner. Local to the one fix that builds a word-bounded replacement without the formatter. |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| Eight `…SeparatesTheReplacement…` tests (word token in front: `return`, `yield return`, `in`, `select`; behind: `is`, `as`, `switch`; both sides) | One space between `string.Empty` and an abutting word token, and nothing else changed | `returnstring.Empty`, a double space, or a changed line ending | `VerifyCodeFixWithEndOfLine` (exact text, LF and CRLF, fixed state compiled and re-analyzed) | Adequate. Both sides and the combined case are present, and they failed on `dff2aa9` (`09`). |
| Three `…KeepsTheAuthorsSpacing` tests (`a+""`, `(""+s)`, `s=""`) | No space next to a punctuation neighbour | `a+ string.Empty` (`main`'s clean-up output) | same | Adequate. They are the other side of the guard's boundary and would fail on an unconditional space. |

## Failure classes dismissed
- FC-001, FC-002: see *Guard scope*. The span and boundary characters agree with the lexer's rule.
- FC-003: both sides of the boundary (word versus punctuation, abutting versus trivia present) are tested.
- FC-004, FC-005, FC-019–FC-021: the guard replaces trivia only when it is empty. Comments on the opposite side are kept (probe above). A directive cannot stand between two abutting tokens.
- FC-006: the compile and meaning break of #1 is closed. Probes and tests compile, and the fixed state is re-analyzed.
- FC-007, FC-009: one pass. The diagnostic disappears.
- FC-008: replacements are per literal, and no two reported literals share a word-token boundary without trivia.
- FC-010–FC-014, FC-028, FC-038–FC-043: no formatter phase or unsupported shape is involved. The fix does not delegate to the formatter.
- FC-015, FC-016: see *Policy ownership*. The one `ApplyCodeFixAsync` handles regular and verbatim literals alike.
- FC-017: see *Assertion adequacy*.
- FC-022–FC-027, FC-029, FC-036: only the literal node is replaced. No other token is rewritten.
- FC-030: the change adds at most one space on a side that had none.
- FC-031: the inline comment and the summary of `TouchesWordToken` describe the new guard. No `TODO`, and no tracker reference.
- FC-032: the implementer ran the whole analyzer project after the compiled change (9411 passed).
- FC-033: uniform endings are the relevant documents. The line-ending drift tests from round 0 still pass (60 of 60 RH3003 tests).
- FC-034, FC-035: expected values come from SHARED-FIX-003/006 and RH3003-004, not from the code.
- FC-037: not a text-only claim.

## Visible output is decided
- Word-neighbour expected values (`return string.Empty`, …): demanded by SHARED-FIX-003 (no new compile error), which is unchanged, and equal to `main`'s output (Decision 10).
- Punctuation-neighbour expected values (`a+string.Empty`): demanded by SHARED-FIX-006 ("format only the node it replaces") and RH3003-004, both unchanged.
- No rule page changed. No undecided output.

## Cost of special cases
None. `TouchesWordToken` is a cheap syntactic check of the two tokens the rewrite touches, and it serves a class whose failure is a compile or meaning break.

## Architecture findings
_None._

## Hints
_None._
