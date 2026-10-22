Role: spec author
Commit: a5bb97b
Verdict: open question

Everything except one requirement is written and committed. The blocker is the shared side of P37 (SHARED-STMT-005), which has no conforming shape at all (lead clarification 6). It is drafted below but not written into the spec. My prompt carried no intended fix or verdict.

## Changed requirements

- FMT-PIPE-013 — Wrapping code in a block keeps the comments' layout — added (P32), `sdd/specs/formatter/pipeline.md`
- FMT-PIPE-014 — One treatment per syntax family — added (P34), `sdd/specs/formatter/pipeline.md`
- FMT-STRUCT-024 — A rewrite that cannot be completed safely is not applied — added (P36), `sdd/specs/formatter/structural-transforms.md`
- FMT-BLANK-016 — A required blank line goes after directives, never directly in front of one — added (P37, formatter side), `sdd/specs/formatter/blank-lines.md`
- SHARED-COMMENT-012 — A comment that spans several lines belongs to the line it starts on — added (P33), `sdd/specs/shared/comment-placement.md`
- SHARED-COMMENT-011 — clarified (P12): a line led by a comment that is out of place is reported, whatever column the token behind the comment reaches. The diagnostic goes where the rule's own requirements put it. A new Why sentence and an RH5204 example were added.
- SHARED-FIX-008 — clarified (P35): the line ending is the one FMT-PIPE-007 detects, and a lone `\r` counts as `\n`. In an all-`\r` document, the line breaks a fix creates are `\n`. Nothing else in `shared/code-fixes.md` was touched.
- FMT-STRUCT-002, FMT-STRUCT-006, FMT-SWITCH-007 — changed: each gained one sentence, "The layout of these comments is kept as FMT-PIPE-013 states."
- SHARED-STMT-005 — **not written** (see Decisions needed).

`Uses` changes:
- `pipeline.md`: added FMT-BREAK-011, FMT-BREAK-012 and FMT-BREAK-016 (cited in FMT-PIPE-014).
- `structural-transforms.md` and `switch-case-braces.md`: added FMT-PIPE-013.
- `comment-placement.md` already listed SHARED-BLANK-002.

The ledger has gained the five new IDs at their sorted positions. `SpecConsistencyTests` and `SpecStructureTests` pass. `RequirementCoverageTests.EveryRequirementOfAnEnforcedSpecIsCovered` fails only for FMT-BLANK-016, FMT-PIPE-013, FMT-PIPE-014 and FMT-STRUCT-024, which have no tests yet. That is expected for this stage.

Placement: every spec keeps its headings in ID order (checked over all specs), so each new requirement is appended at the end of its file rather than physically "next to" FMT-PIPE-002, FMT-PIPE-003, FMT-BLANK-006 or SHARED-STMT-004. Each new requirement cites the ones it relates to. FMT-PIPE-003 lives in `pipeline.md`, so "P36 next to FMT-PIPE-003" could only be met by citing it. FMT-STRUCT-024 does that.

SHARED-COMMENT-012 is listed under no enforced spec's `Uses`, so the coverage check does not require it. The lane still asks for a covering test. Adding it to the `Uses` of RH5203, RH5204 and the other line-start rules would mean editing analyzer specs, which I left out of scope.

No non-goals were added. The issue gives no new boundary that an existing non-goal does not already cover.

## Examples for the tests

Conforming shapes are inputs that behave as the requirement states today. "Verified" means I ran it myself: `scripts/trace.sh` for the formatter, `scripts/apply-fix.sh` for analyzers.

- **FMT-PIPE-013**
  - Conforming: the spec example (verified, stable after pass 2). Already-tagged tests that pin conforming shapes and could also carry FMT-PIPE-013:
    - `ControlFlowBraceCommentTests.LeadingCommentAboveUnbracedIfBodyIsPreserved` and `TrailingCommentOnUnbracedIfBodyIsPreserved`
    - `StructuralTransformRequirementTests.CommentsOfBracedIfAndElseBodiesStayWithTheirStatements`, plus its FMT-STRUCT-006 test with `42 /* why */`
    - `SwitchCaseBracesTests.LabelAndLeadingCommentsArePreservedWhenAddingBraces` and `TrailingStatementCommentIsPreservedWhenAddingBraces`
  - Non-conforming (thoenissen/Reihitsu#1381):
    - a blank line between `// one` and `// two` is dropped
    - `/* a */ // b` is glued to `/* a */// b`
    - in a switch section, `/* c */ A();` is split from its statement
  - The untagged `ExpressionBodyNextLineExpressionTests.LineCommentTrailingArrowWithExpressionOnNextLineConvertsInOnePass` pins `{// why`. That shape belongs to thoenissen/Reihitsu#963, not to this requirement.
- **FMT-PIPE-014**
  - Conforming: the spec example, a constructor and a method with leading-comma parameter lists plus `a⏎= b;` and `b =⏎a;` (verified). The existing FMT-BREAK-011, -012 and -016 tests are conforming too.
  - Non-conforming (thoenissen/Reihitsu#1383; arrow spacing on thoenissen/Reihitsu#958):
    - indexer parameter lists
    - `+=` and `??=` wraps
    - attribute-target colon spacing
    - property versus accessor and lambda arrow spacing
    - a comment before a closing `]`
- **FMT-STRUCT-024**
  - Conforming: the spec example, which is `ExpressionBodyDirectiveGuardTests.MethodWithDirectiveBeforeExpressionIsNotConverted` without `public` (tagged FMT-STRUCT-007). Also:
    - the other FMT-STRUCT-007 guard tests in that class
    - `StructuralTransformRequirementTests.IfBodyInsideConditionalGroupWithDisabledBranchIsNotBraced` (FMT-STRUCT-003)
    - `FieldDeclarationSplitTests` lines 1533 and 1556 (FMT-STRUCT-023)
  - Non-conforming:
    - a documented `public int X, Y;` is split (thoenissen/Reihitsu#1384; pinned by the untagged `FieldDeclarationSplitTests.DocumentedCombinedFieldsAreSplitAtMemberIndentation`)
    - `private int /* c */ _first, _second;` duplicates the comment (thoenissen/Reihitsu#963)
    - an `async` method whose return type is an alias of `Task` gets `return await` (thoenissen/Reihitsu#1390)
    - `/* before d */` ends up trailing the previous field (thoenissen/Reihitsu#1395)
- **FMT-BLANK-016**
  - Conforming: the spec example, which is `BlankLineRequirementTests.DirectiveDirectlyBeforeStatementSuppressesStatementKindBlankLine`. `BlankLineAfterClosingBraceGoesBehindDirective` (FMT-BLANK-005) also conforms.
  - Non-conforming (thoenissen/Reihitsu#1385): #1372 Q-B3, where a blank line is inserted in front of `#pragma` before `case 2:`, and Q-B4, where a directive group followed by a comment puts the line above `#if`.
- **SHARED-COMMENT-011 (amended)**
  - Conforming (verified with RH5204): `      /* a */ private int _a;` and `/**/private int _b;` inside `class C` are both reported, and both are fixed in one iteration. The second is the strong shape: `private` already stands in the member column, so only the comment's column can trigger the report.
  - Conforming but weak (verified with RH5110): with `/* c */` at column 4 or 12, `where` is reported. In those shapes the `where` keyword is off its column too.
- **SHARED-COMMENT-012**
  - Conforming (verified):
    - formatter: `{ /* p⏎        q */ Foo(); }` puts `Foo();` on its own line (spec example 1)
    - RH5203: `P(1,⏎          /* a⏎          b */ 2);` is reported (spec example 2). Its fix ends as `no-progress`, see below.
  - Non-conforming (thoenissen/Reihitsu#1382):
    - formatter: `int a; /* first⏎    second */ int b;` is kept. The untagged tests `OneLineClassBodyMembersFullPipelineTests.KeepsMemberAfterMultiLineCommentEndingMidLine` and `AccessorListAccessorLineBreakTests.AccessorAfterMultiLineCommentEndingMidLineRemainsUnchanged` pin it.
    - RH5204: `    /* a⏎    b */       private int _a;` is not reported (verified).
    - the SHARED-INIT-001 tail shapes.
- **SHARED-FIX-008 (amended)**
  - Conforming: the existing tagged tests (five in RH5405, plus RH7303 line 223 and RH7310 line 632) use uniform CRLF or LF documents and still hold. All-`\r` documents: per #1363 Q6, run by the issue author with the fixture runner without line-ending normalization, the RH5405 fix on `if (b) M(b);` and the RH5417 fix on `void M() { }` create `\n`. I did not re-run this: `apply-fix.sh` normalizes every fixture to LF and CRLF. A test has to build the `\r` document itself.
  - The requirement says nothing about the lone `\r` breaks a fix keeps in such a document. Today they stay `\r`.
- **FMT-STRUCT-002, FMT-STRUCT-006, FMT-SWITCH-007**: no new examples. All their tagged tests were read, and none pins a shape that FMT-PIPE-013 contradicts. That covers the phase-level unit tests too: `ExpressionBodiedTransformTests` lines 687 to 1026 and `ExpressionBodiedLocalFunctionTransformTests` line 418.

Existing tests tagged with a requirement whose amended wording they now contradict: **none found**. I checked every test tagged SHARED-COMMENT-011, SHARED-FIX-008, FMT-STRUCT-002, FMT-STRUCT-006 and FMT-SWITCH-007.

## Findings against the issue text

- **The issue's P12 example does not hold today.** The input is `class C⏎{⏎    void M<T>()⏎/* c */ where T : class⏎    {⏎    }⏎}`, and the issue says RH5110 "reports the line, as it does today". `apply-fix.sh RH5110` reports `no-diagnostic` under LF and CRLF. RH5110 measures the `where` keyword, which lands at column 8, the expected column. Under the clarified SHARED-COMMENT-011 the line must be reported. This is a further instance of thoenissen/Reihitsu#1147: RH5110 judges the token behind a line-leading comment instead of the comment. That issue's existing instance is the opposite direction, a comment in the right column that is still reported. The spec example therefore uses RH5204.
- **RH5203's fix on the P33 shape** `P(1,⏎          /* a⏎          b */ 2);` ends `no-progress (2 iteration(s))` under LF and CRLF. It moves `/* a` to column 0 and pads `2` with spaces. This is a further instance for thoenissen/Reihitsu#1382 (and the #888 family), which already lists this RH5203 input as "still reported".

## Contradictions left between active requirements (by the issue's scope)

The issue and its follow-ups change these existing requirements later. I left them as they are and report them here so nobody mistakes them for oversights:

- The FMT-STRUCT-022 text ("…including its documentation comment, MUST stay on the first generated field") and its example (a documented `_x, _y` split) contradict FMT-STRUCT-024. FMT-STRUCT-021 and -022 gain the guard in thoenissen/Reihitsu#1384. No test tagged FMT-STRUCT-022 pins a documented split.
- FMT-STRUCT-022, "a comment that trailed a separating comma MUST trail the field that comma now ends", gives `/* before d */` to `_c`, which conflicts with FMT-STRUCT-024's "next to other code". Tracked in thoenissen/Reihitsu#1395.
- SHARED-INIT-001 ("When a comment ends on the token's line, the token itself MUST NOT be checked") contradicts SHARED-COMMENT-012 for a comment that starts on an earlier line. The issue says SHARED-INIT-001 changes in thoenissen/Reihitsu#1382.
- FMT-BLANK-011's Why says that after a directive, a comment belongs to the directive's group. That sits uneasily with FMT-BLANK-016 placing the blank line between `#endif` and `// why`. They do not literally contradict: FMT-BLANK-011 governs only its own blank line. thoenissen/Reihitsu#1385 should reconcile the two.

## Interpretations for the skeptic to test

- **FMT-PIPE-014** lists exactly the families the issue names, as "Each of these is one family". It does not claim the list is complete. Whether, for example, bracketed argument lists form a family with argument lists is not decided; FMT-BREAK-014 joins them deliberately.
- **SHARED-COMMENT-012** limits the formatter's break to "where a requirement of the formatter asks that token to start its own line". The literal "the formatter breaks before it" would also break in `var x = /* a⏎b */ 1;`. I read the principle as "the token does not start a line". It is limited to delimited comments, because a `///` comment ends with its line.
- **FMT-BLANK-016** covers only the blank lines of FMT-BLANK-001 to -005. FMT-BLANK-013 deliberately frames `#region` with blank lines. Whether a gap that ends in a directive needs the line at all is left to the requirement that asks for it and to FMT-BLANK-006. thoenissen/Reihitsu#1385 extends that exemption to section gaps.
- **FMT-STRUCT-024's "next to other code"** keeps the thoenissen/Reihitsu#1395 decision possible: a comment moved onto its own line in front of the same declarator's field is not refused.

## Open questions to file

_None._

## Follow-up candidate (out of scope)

- `sdd/README.md`, "Tests and requirements", says enforced specs are `analyzer/`, `cli/`, `shared/code-fixes.md` and `shared/diagnostics.md`. `RequirementCoverageTests.EnforcedSpecPaths` also enforces `formatter/` and `playground/`.

## Decisions needed

**SHARED-STMT-005 (P37, shared side) has no conforming shape.**

- **Where the fix inserts today.** Every fix that SHARED-STMT-004 covers runs through `StatementShouldBePrecededByABlankLineCodeFixProviderBase` (21 providers, RH5001 to RH5019, RH5021 and RH5029). Its fix inserts its line break at index 0 of the statement's leading trivia. The only other path rebuilds a same-line gap, which can hold no directive. A directive always lies in that leading trivia.
- **Why no shape exercises the principle.**
  - Every reported statement with a directive in its gap gets the blank line in front of the first directive. That is #1372 Q-B4, non-conforming and tracked in thoenissen/Reihitsu#1385.
  - A statement whose last trivia is a directive is not reported at all (SHARED-STMT-003).
  - Gaps without a directive exercise nothing.
- **Drafted text** (not in the spec):

  > ### SHARED-STMT-005 — The blank line goes after directives, never directly in front of one
  > When the fix inserts its blank line into a gap that holds a preprocessor directive, it MUST insert it after the last directive of the gap, in front of the comments between that directive and the statement, and MUST NOT insert it directly in front of a directive. Example: `a++;⏎#if DEBUG⏎a--;⏎#endif⏎// why⏎if (a > 0) { }` gets its blank line between `#endif` and `// why`.

- **Options:**
  - (a) Write it now and tag only weak shapes, such as a SHARED-STMT-003 shape where nothing is reported, so nothing is inserted in front of the directive. This follows the SHARED-DIAG-004 precedent (`squad/decisions/2026-10-21-1260-diag004-weak-coverage.md`). The run report must state that the tests cannot fail.
  - (b) Defer the shared side to thoenissen/Reihitsu#1385, which already says "FMT-BLANK-006 and SHARED-STMT-004 state it". The spec then carries P37 only on the formatter side (FMT-BLANK-016).
  - (c) Write it as an amendment of SHARED-STMT-004 instead of a new ID. This is not recommended: the existing SHARED-STMT-004 tests (RH5001 line 198, RH5002 lines 26, 117 and 249) stay conforming, but the amendment would add a clause that none of them exercises.
- **Recommendation:** (b). It keeps the new requirement honest without a test that cannot fail.
