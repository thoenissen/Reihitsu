Role: auditor
Commit: af54bb99ad4af5ef8263f42089d793ccbe1497b0
Verdict: #849: findings; #850: findings

My prompt named the issues, the slug, the head and base, and the handovers to read. It carried no intended fix, suspected cause, or hoped-for verdict. I worked from `00-lead.md` (decisions 12 to 17, maintainer answers), handovers 11 to 19, the specs, the change doc, and the diff. Fixtures live in the scratchpad (`au2/d`, `au2/i`, `au2/m`, `au2/p`, and formatted copies `au2/f`, `au2/mf`), outside the repository.

## Scope

- The checkout is the head `af54bb99ad4af5ef8263f42089d793ccbe1497b0`. `origin/main` is `e3cf4cdc459371c3a532a3580e6b5fc49a7cf609`, the SHA in my prompt, and it is an ancestor of the head (merge `4d673d1`).
- This is a repair delta against `d7990b4`, the commit the previous audit (`11-auditor.md`) audited. Run-owned changes in the delta:
  - production `af77361` and `400352d`: `GetOnlyMemberExpressionBodyCodeFixProviderBase`, `ExpressionBodyCodeFixUtilities`, `BlankLineCodeFixUtilities`, and the two providers;
  - tests `6fb5a80` and `c191c72`;
  - specs and change doc `1976629`, `9095b22`, and `e6012b7`.

  These match handovers 12 to 19.
- The merge of `main` brought formatter changes, among them FMT-UNSUP-014 and the attached-part line-start rewriter. Its only conflict was the AGENTS.md phase-table row. The resolution keeps both sides: main's new precheck shape, and the run's `GetOnlyMemberExpressionBodyRewriteUtilities.ContainsBlockingDirective` guard name.
- The repair grew into the RH3202 family's shared splice (`ExpressionBodyCodeFixUtilities.ReplaceBodyAsync`). That is decision 17's structural option and stays within the components the run changes. No new surface, and no spec change outside SHARED-GETONLY. I audited that move, its counterparts, and the added tests, not the whole run again.
- Finding numbers continue from #3.

### Previous findings

| # | Status | Evidence |
|---|---|---|
| 1 | closed | Every listed member now holds. (a), (b), (c), (d), and (f) are separated through `FormatMemberOnLinesOfItsOwnAsync`. Tests cover them for both rules, iterative and Fix All, LF and CRLF (12, 15; 16 reports them green). (e), the `/* c */` gap, is now withheld under the new SHARED-GETONLY-008 paragraph 2 (decision 14). The fix now fails on a member #1 did not name, a documentation comment in the gap: new finding #4, which repeats #1. |
| 2 | closed | `DelimitedCommentOnALineOfItsOwnInFrontOfTheGettersSemicolonWithholdsFix` and its siblings (12) put the comment in the leading trivia of the getter's or `return`'s semicolon, which is the seam #2 named. They exist for both rules. |
| 3 | closed | The header and index row list the three get-only types. The introduction names `ExpressionBodyCodeFixUtilities` (SHARED-BODY) and `BlankLineCodeFixUtilities` (SHARED-STMT) as the helpers the fix hands steps to (19, `e6012b7`). A second owner spec is forbidden by `SpecStructureTests.EveryTypeIsCodeOwnerOfAtMostOneSpec`. |

## thoenissen/Reihitsu#849

### Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 4 | high | `src/Reihitsu.Analyzer.CodeFixes/Core/BlankLineCodeFixUtilities.cs` `IsCommentGapToSiblingMember` (line 255), called through `SharesLineWithSiblingMemberAcrossComment` from `GetOnlyMemberExpressionBodyCodeFixProviderBase.CanConvert` (line 96) | FC-001, FC-015, FC-040 | #1 | PR-introduced | SHARED-GETONLY-008 paragraph 2 says the fix MUST NOT be offered when the gap to a sibling on the same line "holds a comment". The check recognises only `SingleLineCommentTrivia` and `MultiLineCommentTrivia`. It does not use the shared owner `SyntaxTriviaUtilities.IsCommentTrivia`, which the same file uses at line 430 and which includes documentation comments. In `DocumentationMode.Parse` or `Diagnose`, Roslyn lexes `/** c */` as `MultiLineDocumentationCommentTrivia` in the next token's leading trivia. So the fix is offered, `SharesLineWithSiblingMember` (whitespace only) breaks no gap, and both members stay on one line. That is the shape SHARED-GETONLY-008 and SHARED-FIX-007 forbid; the formatter separates them. `scripts/apply-fix.sh` (Parse mode, LF and CRLF alike), with the formatter's output for the RH3209 rows: <br>• RH3209, sibling behind: `public int A { get => 1; } /** c */ public int B => 2;` becomes `public int A => 1; /** c */ public int B => 2;`. The formatter writes `public int A => 1;`, a blank line, then `/** c */ public int B => 2;`. <br>• RH3209, sibling in front: `public int D => 3; /** c */ public int E { get => 4; }` becomes `public int D => 3; /** c */ public int E => 4;`. <br>• RH3210, sibling behind: `public int this[int i] { get => i; } /** c */ public int B => 2;` becomes `public int this[int i] => i; /** c */ public int B => 2;`. <br>• RH3210, sibling in front: `public int D => 3; /** c */ public int this[int i] { get => i; }` becomes `public int D => 3; /** c */ public int this[int i] => i;`. <br>The same text in `DocumentationMode.None` lexes as `MultiLineCommentTrivia` and is withheld (traced), so the outcome depends on the documentation mode (FC-040). Members that hold: `/* c */` and `/**/` (`no-fix-offered`); `//` and `///`, which always end the line; a multi-line `/* a` / `b */` gap (finding #5). Neither rule has a test with a documentation comment in the gap. | Refuse by the cause, not by a list of comment kinds. A gap to a sibling that holds no line break and anything but whitespace is the comment class (a directive or disabled text cannot stand there), or use `SyntaxTriviaUtilities.IsCommentTrivia`. Add `/** c */` gap tests in both directions for both rules, at least in Parse mode. |
| 5 | medium | the same method, the line-break condition `gap.All(obj => obj.ToFullString().IndexOf('\n') < 0)` | FC-015, FC-003 | – | PR-introduced | **Policy.** The line-break test is a private text search. Core owns this decision in `SyntaxTriviaUtilities.EndsLine` / `AreSeparatedByEndOfLine` (SHARED-BLANK-008), and that owner says a line break inside a multi-line comment does not separate tokens (SHARED-BLANK-002). The new check counts it as a line break, and misses a CR-only `EndOfLineTrivia`. **Parity, no corruption.** The divergence is unjustified in the diff, but the outputs are fine today, through the line-start exception: <br>• `public int A { get => 1; } /* a` / `       b */ public int B => 2;` becomes `public int A => 1; /* a` / `       b */ public int B => 2;`, equal to the formatter's output. <br>• The mirror, `public int D => 3; /* a` / `       b */ public int E { get => 4; }`, becomes `… b */ public int E => 4;`, also equal to the formatter. <br>**Untested boundary.** 16 says no test pins this. A rewrite to the Core owner, or a fix for #4 through it, would flip both inputs to "no fix offered" and no test would fail. | Pin both multi-line-comment inputs with tests of their current fix output. Then either keep the text search, with the reason (the gap's line break as the spec means it) in its summary, or route through the Core owner and accept the withholding. |

### Against the specs

- **SHARED-GETONLY-008, paragraph 1** holds through the reused helper.
  - Fixtures `au2/p`, all equal to the expected separation:
    - a documentation comment in front of the member's own line (`/** d */ public int A { get => 1; } public int B => 2;`);
    - an attribute (`[Obsolete] public int A …`);
    - a method sibling in front (`public void M() { } public int A { get { return 1; } }`);
    - three members on one line, which converges in 2 iterations.
  - The one-line type is converted in place (tests from 15).
- **SHARED-GETONLY-008, paragraph 2:** broken for documentation comments (#4).
- **SHARED-GETONLY-005:** `ReplaceBodyAsync` takes the same tokens the old private splice took. The body start in the member as written is `{` of the accessor list. In the formatted member it is the arrow. The signature token's trailing trivia comes from the formatter, and the semicolon from the formatted member. `ConvertBodyAsync` keeps its old early exits: no expression body in the input, and a formatter result that still has one (in that case `withBodyOf` returns `null`).
- **SHARED-GETONLY-006 and SHARED-FIX-010 after the merge.** FMT-UNSUP-014 (attached-part comments) arrives through `FindUnsupportedSyntax`. The getter query and the converted-member query both see it, so the fix is withheld for it, and the analyzer still reports.
  - The conversion touches only the trivia of `{`, `}`, `return`, `;`, and the expression's outer edges. It never touches a gap inside the expression, so it cannot produce FMT-UNSUP-014 (FMT-UNSUP-005).
  - Separating the members rewrites only whitespace-only edge trivia, so the refused-shape answer for the converted member cannot change between `CanConvert` and the conversion (FC-043).
- **SHARED-FIX-002** holds through `DocumentOrderFixAllProvider`. Each fix is registered again on the current document, so `CanConvert`'s comment-gap guard decides on the code the earlier fixes produced.

### Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Fix withheld for a comment in the gap to a sibling (SHARED-GETONLY-008 ¶2) | `firstToken.GetPreviousToken().TrailingTrivia` + `firstToken.LeadingTrivia`, and `lastToken.TrailingTrivia` + `lastToken.GetNextToken().LeadingTrivia`, when the far token's member is a sibling in the same list | the separation guard `SharesLineWithSiblingMember`, the same spans, whitespace only; comment classification `SyntaxTriviaUtilities.IsCommentTrivia` | Spans agree with the separation guard. The comment-kind set is narrower than the owner's (documentation comments are missing). The line-break test differs from `EndsLine` | finding #4, finding #5 |
| Separate the member (¶1) | `FormatMemberOnLinesOfItsOwnAsync`: `lineBeginsWithMember` plus whitespace-only edge gaps | RH3202-002 / RH5417-003, the same helper | One owner | ok |
| Whether the formatter's result holds a body to take | `getBodyStartToken(formattedMember)` plus `withBodyOf` returning `null` | the old private checks in both callers | `ConvertBodyAsync`: a formatted member with an expression body gives `null`, as before. The get-only fix: a formatted member without an expression body gives `null`, as before | ok |

### Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Splice: keep the signature, take the formatter's body and the gap in front of it | `ExpressionBodyCodeFixUtilities.ReplaceBodyAsync`, the only one. `rg "GetPreviousToken\(\).*TrailingTrivia\|ReplaceToken\(lastSignatureToken" src/Reihitsu.Analyzer.CodeFixes` | one owner. The architecture finding of 11 is resolved |
| Does a member share its line with a sibling | `BlankLineCodeFixUtilities.IsGapToSiblingMember`, used by `SharesLineWithSiblingMember` and `SharesLineWithSiblingMemberAcrossComment` | one owner |
| Is a trivia a comment | `SyntaxTriviaUtilities.IsCommentTrivia` (Core, documentation comments included), and a private kind list in `IsCommentGapToSiblingMember` | two owners that have drifted (#4) |
| Does a line break separate two tokens | `SyntaxTriviaUtilities.EndsLine` / `AreSeparatedByEndOfLine` (Core), and the `IndexOf('\n')` test in `IsCommentGapToSiblingMember` | two owners with different semantics (#5) |
| Fix All order | `DocumentOrderFixAllProvider`, which 20 providers and bases share | reused |

### Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| Shared-line tests of 12 (RH3209: 4, RH3210: 2) | exact fix text, member and sibling on separate lines, no blank line inserted | the members still on one line; the sibling in column 0 | `VerifyCodeFixWithEndOfLine` (LF, CRLF) | ok |
| `FixAllPutsTwoConverted…OfOneLineOnLinesOfTheirOwn` (both rules), `VerifySpecificationExampleOfMembersSharingTheirLine` | exact text, iterative and Fix All | a whitespace-only line, or one member left on the shared line | `Verify` + `ConvertEndOfLine` | ok |
| `CommentBetween…OnTheSameLineWithholdsFix` (RH3209: 3, RH3210: 1) | no action for a comment gap | an action registered | `VerifyNoCodeFixIsOffered` (Parse mode) | ok for `/* c */`. No documentation-comment case (#4) |
| `CommentBehindTheMemberWithTheSiblingOnTheNextLineKeepsTheFix` | the fix is offered when the gap holds a line break, and the comment follows the new semicolon | the fix withheld; the comment moved | `VerifyCodeFixWithEndOfLine` | ok. A line break inside a multi-line comment is untested (#5) |
| One-line type tests (RH3209: 2, RH3210: 1) | the type on one line is converted in place, nothing separated | a line break inserted | `VerifyCodeFixWithEndOfLine` | ok |
| Finding-2 tests (RH3209: 3, RH3210: 2) | no action for a comment in the leading trivia of the getter's or `return`'s semicolon | an action registered | `VerifyNoCodeFixIsOffered` | ok |

### Visible output is decided

- **Separation of a shared line:** demanded by the unchanged SHARED-FIX-007 *Shared line*.
- **Fix withheld for a comment gap:** a withheld fix for a rare class, which puts nothing in question (decision 14).
- **One-line type, converted in place.** SHARED-FIX-007 itself states no exception for it. The repository already applies this rare-class exception to the sibling rules, RH3202-002 and RH5417-003, with the same reasoning ("a type written on one line is rare, and RH5404 reports it"). SHARED-GETONLY-008 adopts it by reference (decision 15). I count it as leaving a rare construct as written, not as an undecided output. If the lead reads the AGENTS.md rare-class rule as covering only withholding for code fixes, this output is the one to ask about.
- **Fix-text expected values of the shared-line tests:** each equals the formatter's separation. No rule page changed.

### Cost of special cases

- `SharesLineWithSiblingMemberAcrossComment` serves only a rare class (decision 14). It is the refusal itself, as a cheap trivia check, which is what *Code-fix coverage* asks for. #4 is about its scope, not its existence.
- No production line serves only a declared shape (FC-042). No shape is declared.

### Failure classes dismissed

- FC-002: SHARED-GETONLY-008's qualifiers are explicit conditions of the predicate: "same list" (`IsGapToSiblingMember`), "first/last token", and "no line break".
- FC-004, FC-019, FC-020, FC-021: separation rewrites only whitespace-only edge trivia. A comment gap is refused, and a directive cannot share a line. Comments behind `;` or `}` follow the new semicolon (tests from 12).
- FC-005: a directive in the gap always ends its line, so a separate test is not needed.
- FC-006, FC-022, FC-023, FC-024, FC-025: the conversion is unchanged by the repair. Only its placement on the line moved.
- FC-007, FC-008: one iteration per diagnostic. `p4` (three members, two reported) converges in 2 iterations, and the Fix All tests pass.
- FC-009: every offered fix converts the accessor list.
- FC-010, FC-033: every fixture ran under LF and CRLF, with no drift.
- FC-011, FC-012: no loop or phase was changed by the run.
- FC-013, FC-014: unchanged from 11.
- FC-016: the sibling shapes of #1 are enumerated in 12/15, and #4 is the member they missed.
- FC-017: see the table.
- FC-018, FC-036: no rename.
- FC-026: annotations, no text search.
- FC-027: the multi-line accessor list and a multi-line comment gap were checked (`m3`, the tests from 12).
- FC-028: only `FormatNodeInDocumentAsync` is used.
- FC-029: not applicable.
- FC-030: the splice keeps the signature. Separation edits only the two edge gaps that SHARED-FIX-007 names.
- FC-031: the summaries of `ReplaceBodyAsync`, `ConvertBodyAsync`, `ConvertAsync`, `ApplyCodeFixAsync`, `CanConvert`, and the class summary of `ExpressionBodyCodeFixUtilities` all describe the new code.
- FC-032: the merge changed the formatter after the cleaner's run. The lead re-ran RH3202/3203/3208/3209/3210 after the merge (311/311), and I re-ran the run's formatter-parity tests on the head (6/6, below).
- FC-034: not a bug run.
- FC-035: expected values come from SHARED-FIX-007, and the comment-gap expectations from the formatter on the head (12).
- FC-037: no text-only proof is claimed. The cleaner's member reorder is behavioral and was tested.
- FC-038, FC-039, FC-041, FC-042: no shape is declared, and the run does not change the precheck.
- FC-043: dismissed under *Against the specs*.
- FC-044: the line-start exception reads the parsed tree. Nothing is modelled.

### Architecture findings

#4 repeats #1. The decision is the same: whether, and how, the fixed member gets a line of its own when a sibling shares it. It fails between the same components, the get-only fix and SHARED-FIX-007 / the formatter. The new cell is on the comment-kind axis: a documentation comment in the gap. Decision 14 refused the class by a list of trivia kinds instead of by its cause, the gap to a sibling with no line break holding anything but whitespace. The Core owner of "is a trivia a comment" already exists. Under `squad/README.md`, *Architecture findings*, the lead decides this as an architecture finding (refuse by the cause), not as another repair round.

### Hints

- The RH3202 family (`FormatMemberOnLinesOfItsOwnAsync` callers) keeps a member and its sibling on one line across a documentation comment too. This is already the follow-up candidate in `00-lead.md` for comment gaps; the documentation-comment cell belongs to it.

### Follow-up candidates

_None_ beyond `00-lead.md`.

## thoenissen/Reihitsu#850

### Findings

Findings #4 and #5 apply to RH3210 through the shared base `GetOnlyMemberExpressionBodyCodeFixProviderBase`. #4 has its own indexer counterexamples: `au2/i/d4.cs` (sibling behind) and `au2/i/i2.cs` (sibling in front) are both `fixed` with the members left on one line, as quoted under #849.

### Against the specs, Guard scope, Policy ownership, Assertion adequacy, Failure classes dismissed, Architecture findings

As under #849, with one call site for both rules. The RH3210 repair tests are the indexer rows in the assertion table. The indexer-side tests for paragraph 1 (`FixPutsAMemberBehindTheClosingBraceOnTheNextLine`, `FixPutsTheConvertedIndexerOnTheLineAfterAMemberInFrontOfIt`, `FixAllPutsTwoConvertedIndexersOfOneLineOnLinesOfTheirOwn`) are green per 16.

### Hints

_None._

### Follow-up candidates

_None._

## Whole run

- Commands:
  - `scripts/apply-fix.sh RH3209 <au2/d|au2/m|au2/p>`
  - `scripts/apply-fix.sh RH3210 <au2/i>`
  - `scripts/format.sh <au2/f|au2/mf>`
  - `scripts/test.sh --project analyzer --filter "FullyQualifiedName~ShouldUseAnExpressionBodyFormatterTests"`: Passed 6, Failed 0. I re-ran it for FC-032 after the merge of formatter changes.
- No other repository test was re-run. Findings #4 and #5 rest on the fixture runs above.
