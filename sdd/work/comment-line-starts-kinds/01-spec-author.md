Role: spec author
Commit: 03699c0
Verdict: spec written

Inputs: thoenissen/Reihitsu#1139 and its comment, `00-lead.md`, the decision comments on thoenissen/Reihitsu#1117, #1119, #1123, #1125, the P6/P12/P13 entries on thoenissen/Reihitsu#912, thoenissen/Reihitsu#1077, #1146, #1147, #1341. My prompt carried no intended fix or verdict.

## Changed requirements

- SHARED-COMMENT-001 — Only ordinary comments are checked, every kind alike — changed (P13 and P6). A `///` or `/** */` comment that documents nothing is an ordinary comment in every documentation mode. A comment documents a declaration only where the compiler reads it as that declaration's documentation, which means no CS1587. A line holding only `/* */` counts like a `//` line wherever a rule tells comment lines from code lines. The old example (a `///` and a `/** */` behind `[Obsolete]`, both not reported) now shows them reported.
- SHARED-COMMENT-011 — A line that starts with a comment is not judged by the token behind it — added (P12). This is the only ID minted. It is in `requirement-ids.txt` at its sorted position.
- RH5110-001, RH5112-001, RH5201-001, RH5201-002, RH5203-001, RH5204-001, RH5205-001, RH5206-001 — clarified. Each gains one sentence that references SHARED-COMMENT-011, and its spec lists SHARED-COMMENT-011 under `Uses`.
- The intro paragraph of `shared/comment-placement.md` names the two cross-rule requirements. The code owners, the index row in `sdd/specs/README.md`, and the non-goals are unchanged.
- No change doc (tests lane, as the lead decided). `documentation/` is unchanged. The rule pages carry no corner cases, and `general-notes.md` already says a comment can keep a diagnostic from being reported. A user-facing P13 note would describe behavior that does not ship yet.

How the scope of SHARED-COMMENT-011 was chosen: a rule "checks where a line starts" when it decides whether a token starts its line, or in which column a line starts. By that test:
- In: RH5110, RH5112, RH5201, RH5203, RH5204, RH5205, RH5206.
- Out: RH5101 and RH5105–RH5108, RH5111, and RH5113 check whether a token is on a later line than the token in front of it. SHARED-JOIN-001 already exempts a comment in that gap, except for RH5111, which is open question 3 below.
- Out: RH5102, RH5103, RH5109, and RH5114 check whether two elements share a line. RH5104 reports the comment itself. RH5202 measures the closing quotes of a raw string.

How "documents nothing" was chosen: thoenissen/Reihitsu#1341 (round 8, P13) treats `[Obsolete]⏎/// <summary>G</summary>⏎int G;` and a `///` behind an attribute list (RH8309) as comments that document nothing. FMT-DOC-003 already uses "documents nothing; the compiler reports CS1587". I checked CS1587 with a scratch build on SDK 10.0.402. It is reported for a `///` and a `/** */` behind an attribute list, in front of a namespace, an accessor, or a local function, and for a trailing `/// tail` before `}`. It is not reported for `/// <summary>M</summary>⏎/** <remarks>M</remarks> */⏎void M()`.

## Examples for the tests

Before writing the spec, I ran each shape below through a scratch probe: Release-built `Reihitsu.Analyzer.dll` in a console host under `/tmp/…/scratchpad`, with no repository file touched.

**SHARED-COMMENT-011.** These shapes conform today and can be pinned:
- RH5204: `    /* a */   private int _a;` is not reported. The comment is at the scope column, and `private` behind it is in a wrong column.
- RH5201: `.ToList()` behind `/* all */`, with the comment in the anchor column and extra spaces before the link, is not reported. The comment-led link `/* b */ .Select(…)` at the anchor is not reported for not starting its line.
- The other side is reported. RH5203 reports `P(1, /* one */⏎            2);`, where the comment is at the end of the line above. It also reports `P(1,⏎          // two⏎            2);`, where the comment is on its own line above.
- RH5112: `items⏎        /* first */ .Where(…)` is not reported. RH5048 reports the comment, which is SHARED-CHAIN-004 territory.

These shapes do not conform and must not be pinned:
- The RH5110, RH5203, and RH5206 shapes in the first example block. They are thoenissen/Reihitsu#1147.
- RH5205: `x =>⏎<anchor>/* body */ {`. It is still reported, which is a new instance. See below.

**SHARED-COMMENT-001.** These shapes conform today:
- RH5033 reports `/**/` behind an attribute list, in both documentation modes.
- RH5033 reports `//// note` behind an attribute list with `DocumentationMode.None`.
- RH5034 does not report `/// <summary>M</summary>⏎/** <remarks>M</remarks> */⏎void M()`, in either mode.

These shapes do not conform:
- RH5033 behind an attribute list does not report `/// <summary>M</summary>` or `/** <summary>N</summary> */`, in either mode.
- RH5038 does not report `M(⏎/// first⏎1);`, in either mode.
- RH5104 handles the `/* note */` comment line wrongly. That case is thoenissen/Reihitsu#1146.

## Existing tests that contradict the amended SHARED-COMMENT-001

These tests are tagged SHARED-COMMENT-001 and assert that a comment that documents nothing is not reported. That is behavior the requirement now calls a bug. The test implementer must not flip them, because the code does not conform. Keeping them contradicts clarification 5. This needs the lead's decision (escalation 4), not mine.

- `RH5033CommentsMustNotBePlacedBetweenAttributesAndDeclarationAnalyzerTests`:
  - `VerifyNoDiagnosticForDocumentationCommentAfterAttribute`
  - `VerifyNoDiagnosticForDocumentationCommentAfterAttributeWithoutDocumentationParsing`
  - `VerifyNoDiagnosticForDocumentationCommentAfterAttributeWithDocumentationParseMode`
  - `VerifyEmptyBlockCommentAfterAttributeIsReportedButBlockDocumentationCommentIsNot`
  - `VerifyEmptyBlockCommentAfterAttributeIsReportedButBlockDocumentationCommentIsNotWithoutDocumentationParsing`

  In the last two, the `/**/` half conforms. Only the `/** <summary>Method</summary> */` half contradicts.
- `RH5038CommentsMustNotBePlacedInsideArgumentListAnalyzerTests`:
  - `VerifyNoDiagnosticForDocumentationCommentInArgumentList`
  - `VerifyNoDiagnosticForDocumentationCommentInArgumentListWithoutDocumentationParsing`

`scripts/test.sh --project architecture --filter "FullyQualifiedName~Spec|FullyQualifiedName~RequirementCoverage"`: 38 passed and 1 failed. The failure is the expected `SHARED-COMMENT-011: no test covers it (shared/comment-placement.md:481)`.

## Further instances of existing follow-ups (for the lead to comment on)

- thoenissen/Reihitsu#1147 (P12): RH5205 reports the opening brace of `Func<int, int> f = x =>⏎                           /* body */ {` even though the comment is in the lambda's anchor column. RH5205-001 now references SHARED-COMMENT-011.
- thoenissen/Reihitsu#1077 (P6): the comment-placement rules do not report a `/** */` that documents nothing. One example is `[Obsolete]⏎/** <summary>N</summary> */⏎void N()`, which RH5033 does not report in either mode.
- Not changed here: RH5104-001 and RH5104-005 still exclude `///` and `/**` and name only `//` lines. Thoroughly Reihitsu#1146 states that the RH5104 spec gains the P13 wording there.

## Open questions to file

No `SDD open questions: sdd/specs/shared/comment-placement.md` issue exists yet, so a new one is needed for all three. Question 1 is marked "Decided by principle P13 (#912)".

1. **Decided by principle P13 (#912): comment-placement rules do not report a `///` that documents nothing.**
   - Input: `[Obsolete]⏎/// <summary>M</summary>⏎void M()` for RH5033, and `M(⏎/// first⏎1);` for RH5038.
   - Real output: not reported, with documentation comments parsed or not.
   - Expected under P13: reported. The compiler reports CS1587 for both, so they document nothing and are ordinary comments.
   - Touches: SHARED-COMMENT-001, RH5033-001, RH5038-001, and every rule whose spec uses SHARED-COMMENT-001 (RH5033–RH5049). No follow-up covers these rules today; thoenissen/Reihitsu#1341 covers only RH83xx/RH8401.
   - Boundary shape for the same follow-up: `/// <summary>N</summary>⏎// note⏎/// <remarks>N</remarks>⏎void N()`. The compiler reports CS1587 for the first `///`, so it documents nothing. RH5034 reports `// note` today.

2. **What does a rule that checks where a line starts do with a leading comment that is itself out of place?** SHARED-COMMENT-011 leaves this out ("does not cover").
   - Input: `void M<T>()⏎/* c */ where T : class` (comment in column 0), `P(1,⏎        /* two */ 2);` (comment right of the alignment column), and the same with an RH5206 arm.
   - Real output: RH5110 reports `where`, RH5203 reports `2`, and RH5206 reports the switch expression. RH5201 reports a link whose leading comment is off the anchor, as RH5201-002 states. RH5204 reports `      /* b */ private int _b;` on `private`.
   - Why it looks unclear: P12 says the line is judged by its first element "as RH5204 does", which points to reporting by the comment's column. RH5110-001, RH5203-001, and RH5206-001 check only lines with nothing but whitespace in front of the token, which says not reported. The #912 wording "such lines are not reported" also allows the second reading.
   - Readings: (a) the rule judges the comment's column, so the shapes above stay reported. (b) the rule never reports a comment-led line, and the comment rules handle the comment.
   - Touches: SHARED-COMMENT-011, RH5110-001, RH5201-002, RH5203-001, RH5204-001, RH5205-001, RH5206-001.

3. **Does P12 cover RH5111?**
   - Input: `int x⏎    /* c */ = 1;`
   - Real output: reported (RH5111-001), with the fix withheld (RH5111-005).
   - Why it looks unclear: RH5111 is a line-break rule, and the issue names line breaks. But it checks whether `=` starts on a later line than its target, which a leading comment does not change, so the scope test above leaves it out.
   - Readings: (a) not covered, so it stays reported. (b) covered, so it is not reported.
   - Touches: SHARED-COMMENT-011, RH5111-001, RH5111-002.

## Decisions needed

_None for this role._ The test-handling decision above belongs to the lead.
