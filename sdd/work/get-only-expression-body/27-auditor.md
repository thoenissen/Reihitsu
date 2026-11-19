Role: auditor
Commit: 65ac48113cd27694fb1cc5f7f73bfb52003cc086
Verdict: #849: pass; #850: pass

My prompt named the issues, the slug, the head and base, and the handovers to read. It also named decision 18 as the reason for this delta audit. It carried no intended fix, suspected cause, or hoped-for verdict. I worked from `00-lead.md` (decision 18, maintainer answers), handovers 21 to 25, the spec, and the diff. Fixtures live in the scratchpad (`au3/d`, `au3/i`), outside the repository.

## Scope

- The checkout is the head `65ac48113cd27694fb1cc5f7f73bfb52003cc086`. `origin/main` is `e3cf4cdc459371c3a532a3580e6b5fc49a7cf609`, the SHA in my prompt, and it is an ancestor of the head. I fetched to confirm.
- This is a delta audit against `af54bb9`, the commit audit 21 audited (decision 18: an architecture finding, refused by its cause, not a repair round). The delta outside `sdd/work/` has four files:
  - `sdd/specs/shared/get-only-expression-bodies.md` (`4040015`): SHARED-GETONLY-008 paragraph 2 restated by its cause, `Uses` gains SHARED-BLANK-002 and SHARED-BLANK-008, and two examples were added;
  - `src/Reihitsu.Analyzer.CodeFixes/Core/BlankLineCodeFixUtilities.cs` (`7350c45`): `IsCommentGapToSiblingMember` and two summaries;
  - the RH3209 and RH3210 analyzer test files (`48f23ae`).

  These match handovers 22 to 25. The repair did not grow. `IsCommentGapToSiblingMember` has one caller, `SharesLineWithSiblingMemberAcrossComment`, and that has one caller, `GetOnlyMemberExpressionBodyCodeFixProviderBase.CanConvert` (`rg`). So the change reaches only RH3209 and RH3210, not the RH3202 family or `TargetAttributePlacementCodeFixProviderBase`. Those use only `SharesLineWithSiblingMember`, which is unchanged.
- Finding numbers continue from #5. No new finding.

### Previous findings

| # | Status | Evidence |
|---|---|---|
| 4 | closed | The guard no longer lists comment kinds. It withholds the fix when the gap holds any trivia other than whitespace and `SyntaxTriviaUtilities.AreSeparatedByEndOfLine(left, right)` is false. Every member of #4 now holds. `/** c */` in Parse, Diagnose, and None mode, in both directions and for both rules, is covered by tests (23, green per 24). I also traced the members #4 named as holding, which still hold: `/* c */` and `/**/` (`au3/d/d3`, `no-fix-offered`), and `//` behind the member (`d7`, fixed, comment kept behind the new semicolon). I also checked members #4 did not list. A multi-line documentation comment `/** a` / `b */` in the gap is withheld in both directions, for RH3209 (`d1`, `d2`) and RH3210 (`i1`). Two comments in one gap, `/* a */ /* b */`, are withheld (`d6`). A comment gap behind a converted member that is the middle one of three members is withheld (`d4`). A documentation comment that begins the member's own line, `/** d */ public int E { get => 4; } public int F => 5;`, is not in a sibling gap. The fix is offered and `F` goes onto a line of its own (`d8`). All of this ran under LF and CRLF. |
| 5 | closed | The private `IndexOf('\n')` test is gone. The guard now calls the Core owner `AreSeparatedByEndOfLine` / `EndsLine` (SHARED-BLANK-008). A CR-only `EndOfLineTrivia` is now an end of line as well. Both multi-line-comment inputs of #5 are pinned by tests in both directions for both rules. Under decision 18 they assert the withheld fix. The other side of the boundary, a multi-line comment followed by a real end of line, is pinned by `MultiLineCommentBehindTheMemberWithTheSiblingOnTheNextLineKeepsTheFix` (RH3209). In front of the member, that side is `d5`, which is fixed: `E` is converted and the comment stays. |

## thoenissen/Reihitsu#849

### Findings

_N/A — no confirmed finding in the delta._

### Against the specs

- **SHARED-GETONLY-008, paragraph 2 (restated).** The code matches the spec's condition one to one:
  - the gap is the trivia between the member's first token and the token in front of it, or between its last token and the token behind it, and the other token belongs to a sibling of the same list (`IsGapToSiblingMember`, unchanged);
  - the gap holds "any trivia other than whitespace" (`Any(!WhitespaceTrivia)`);
  - "no line break separates its two tokens as SHARED-BLANK-008 defines one" (`AreSeparatedByEndOfLine == false`).

  The conjunction is safe when the gap holds only an end of line. In that case `Any(!WhitespaceTrivia)` is true, but `AreSeparatedByEndOfLine` is also true, so the fix is not withheld.
- The explanatory paragraph says only delimited comments can stand in such a gap. That is true for every input that compiles. Skipped-token trivia appears only with a syntax error, and the fix is not offered there anyway. Even if such trivia stood in a gap, the guard would withhold the fix, which is the safe direction.
- **SHARED-GETONLY-008, paragraph 1.** It is unchanged and still reachable. A line that begins with the tail of a multi-line comment whose head stands behind a non-sibling token, such as the type's `{`, is still converted in place. The case where the head stands behind a sibling is now paragraph 2's, "whether or not the line begins with a member", and the spec says so.
- **New spec examples.** The `/** c */` and `/* a` / `b */` examples in both directions correspond to the new `…WithholdsFix` tests (same members, same diagnostic spans). The "other side" example is unchanged.
- **SHARED-GETONLY-003, -005, -006, -007 and SHARED-FIX-010.** The delta does not touch them. `CanConvert`'s other conjuncts are unchanged.

### Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Fix withheld for a non-whitespace gap to a sibling on one line (SHARED-GETONLY-008 ¶2) | `left.TrailingTrivia` + `right.LeadingTrivia` for (`firstToken.GetPreviousToken()`, `firstToken`) and (`lastToken`, `lastToken.GetNextToken()`), when the far token's member is a sibling in the same list | Separation guard `SharesLineWithSiblingMember`: same spans, whitespace only. Line break: `SyntaxTriviaUtilities.AreSeparatedByEndOfLine` (SHARED-BLANK-008), the owner that the formatter's spacing (`AdjacentTokenSpacingUtilities`), `SyntaxTokenPositionUtilities`, and `TryInsertBlankLineBefore` in the same file also read | Spans are identical to the separation guard. A whitespace-only gap is never a comment gap, and a non-whitespace gap is never a separation gap, so the two guards cannot both fire. A gap that holds a line break is neither. The line-break semantics now equal the Core owner's | ok |

### Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Does a line break separate two adjacent tokens | `SyntaxTriviaUtilities.AreSeparatedByEndOfLine` / `EndsLine` (Core). `rg -n "IndexOf\('\\\\n'\)" src/Reihitsu.Analyzer.CodeFixes` finds only RH5401's check of formatted text, which makes a different decision (whether a formatted member fits on one line) | one owner. #5 is resolved |
| Is a trivia a comment, for the sibling gap | no kind list left. The guard decides by "not whitespace" | there is no second owner to drift. #4's class cannot recur |
| Does a member share its line with a sibling | `BlankLineCodeFixUtilities.IsGapToSiblingMember`, unchanged | one owner |

### Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `DocumentationCommentBetweenTheMemberAndAMemberBehindItOnTheSameLineWithholdsFix` and `…InFrontOfIt…` (Parse, Diagnose, None × LF, CRLF) | `/** c */` in a sibling gap withholds the fix in every documentation mode (FC-040) | any action registered in any mode | `VerifyNoCodeFixIsOffered(source, DocumentationMode)`. It applies the mode to the verifier and to the document it builds itself, asserts the marked diagnostic, asserts that at least one fixable diagnostic exists, and asserts that no action is registered for any of them | ok |
| `MultiLineCommentBetweenTheMemberAndAMemberBehindItWithholdsFix` and `…InFrontOfIt…` (LF, CRLF) | a line break inside a multi-line comment does not separate the members (SHARED-BLANK-002) | an action registered | `VerifyNoCodeFixIsOffered` (Parse) | ok |
| `MultiLineCommentBehindTheMemberWithTheSiblingOnTheNextLineKeepsTheFix` (LF, CRLF) | a real end of line behind the comment keeps the fix, and the comment follows the new semicolon | the fix withheld; the comment moved or reindented | `VerifyCodeFixWithEndOfLine`, exact text | ok. It pins the other side of the boundary (FC-003) |
| The RH3210 counterparts (4 tests) | as above, for indexers | as above | as above | ok. One shared guard. The keep-fix side is covered by RH3209 and by fixture `d5` |

### Visible output is decided

- **The multi-line-comment gap moves from "fix offered" to "no fix offered".** That is withholding a fix for a rare class, which puts nothing in question (decision 18; `squad/README.md`, *What reaches the maintainer*). No expected fix text changed: only `VerifyNoCodeFixIsOffered` tests were added, and no existing test changed.
- **The documentation-comment gap is now withheld.** This is the same decision.
- **The keep-fix test's expected text** equals today's output, and that output is demanded by the unchanged SHARED-GETONLY-003 and SHARED-GETONLY-005.
- No rule page and no change doc changed.

### Cost of special cases

- The guard is the refusal itself: a cheap check of the gap's trivia built from the Core line-break owner, with no comment-kind modelling (*Code-fix coverage*). The production diff is +9/−7 in one method, with no new member.
- No shape is declared, so FC-042 cannot apply.

### Failure classes dismissed

- FC-001: the inspected span is unchanged and still equals the separation guard's span (see *Guard scope*).
- FC-002: every qualifier in SHARED-GETONLY-008 ¶2 ("same list", "first/last token", "trivia other than whitespace", "no line break as SHARED-BLANK-008 defines") is an explicit conjunct.
- FC-003: both sides of the line-break boundary are pinned (withheld multi-line gap versus multi-line comment plus end of line).
- FC-004, FC-005, FC-019, FC-020, FC-021: the delta only widens when the fix is withheld. No rewrite changed, so no trivia, directive, or comment can be lost or moved by it.
- FC-006 to FC-012, FC-022 to FC-025, FC-027 to FC-030: the conversion, separation, Fix All, loops, and phases are unchanged in the delta (see 21 for the conversion path).
- FC-010, FC-033: every fixture ran under LF and CRLF with no drift. The guard now also recognises CR-only `EndOfLineTrivia`.
- FC-013, FC-014: there is no analyzer change, and the analyzer still reports every withheld case (the helper asserts the diagnostic).
- FC-015: resolved (see *Policy ownership*).
- FC-016: #4 and #5 are refused by their cause. The members #4 left out (multi-line documentation comment, two comments, middle member) hold (fixtures above).
- FC-017: see the table.
- FC-018, FC-026, FC-036: no rename and no text search. The private text search was removed.
- FC-031: the summaries of `IsCommentGapToSiblingMember` and `SharesLineWithSiblingMemberAcrossComment` describe the new condition. The `CanConvert` summary ("where a comment separates the member from a sibling member on one line") still holds, because only a comment can stand in such a gap in code that compiles. The comments name no tracker numbers and hold no TODO.
- FC-032: the delta has no formatter change and no merge of `main` since 21. The cleaner's Release build on the head has 0 warnings (25).
- FC-034, FC-035: this is not a bug run. The expected values come from SHARED-GETONLY-008 as restated.
- FC-037: no text-only proof is claimed.
- FC-038, FC-039, FC-041, FC-042, FC-043: no shape is declared, and the precheck is unchanged.
- FC-040: now covered by the three-mode rows. The outcome is the same in every documentation mode.
- FC-044: the guard reads the parsed trivia and the Core owner, and models nothing.

### Architecture findings

_None._ The two owners that #4 and #5 named (a comment-kind list and a line-break text search) are gone, and the guard reads the Core owner.

### Hints

_None._

### Follow-up candidates

_None_ beyond `00-lead.md`.

## thoenissen/Reihitsu#850

### Findings

_N/A — no confirmed finding._ RH3210 shares the guard through `GetOnlyMemberExpressionBodyCodeFixProviderBase`.

### Against the specs, Guard scope, Policy ownership, Assertion adequacy, Failure classes dismissed, Architecture findings

As under #849. Fixture `au3/i/i1` (a multi-line `/** a` / `b */` gap in front of an indexer) gives `no-fix-offered` under LF and CRLF. The four RH3210 tests from 23 cover `/** c */` in all three documentation modes and the multi-line gap, in both directions.

### Hints

_None._

### Follow-up candidates

_None._

## Whole run

- Commands:
  - `scripts/apply-fix.sh RH3209 au3/d`: 8 fixtures × 2 endings. 6 fixed (`d5`, `d7`, `d8`), 10 `no-fix-offered` (`d1`, `d2`, `d3`, `d4`, `d6`). None not-converged, invalid, or drifting.
  - `scripts/apply-fix.sh RH3210 au3/i`: `i1` gives `no-fix-offered` under LF and CRLF.
- I re-ran no repository test. I rely on 24's focused run (148/148), the analyzer project (11107/11107), the architecture project (198/198), and 25's Release build (0 warnings).
