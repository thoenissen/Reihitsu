Role: spec author
Commit: 86c63dcd9f43a0a8e7d7ce400ba6531e224e0067
Verdict: spec written

The prompt carried no intended fix, suspected cause, or hoped-for verdict. No change doc: every requirement writes down existing behavior. No rule page under `documentation/rules/` is wrong in a way that is not an open question below, so none was changed (RH5304's "is not rewritten" and RH5404's "members on their own line" / "extension blocks by hand" sentences touch open questions and stay until those are decided).

## Changed requirements

All added (63 IDs, registered in `sdd/specs/requirement-ids.txt`; specs listed in `sdd/specs/README.md`):

- New shared specs, because more than one rule spec relies on the same behavior:
  - `shared/initializer-layout.md` (code owner `InitializerLayoutAnalysisUtilities`): SHARED-INIT-001 — A comment in front of a token anchors its line (RH5301, RH5303, RH5304); SHARED-INIT-002 — A wrapped complex element is laid out as a block (RH5303, RH5304).
  - `shared/statement-blocks.md` (code owner `StatementBlockParentPolicy`): SHARED-BLOCK-001 — The block of a control-flow statement is checked (RH5402, RH5403).
  - `shared/statement-braces.md` (code owner `StatementBracesCodeFixProviderBase`): SHARED-BRACE-001 — The bodies of `if` and `else` are checked; SHARED-BRACE-002 — The fix wraps the body in braces (RH5405, RH5406, RH5407).
- RH5301-001 Braces outside the column of `new` are reported; -002 Members outside their column are reported; -003 The diagnostic is on the creation or on the member; -004 The fix lays out the object creation.
- RH5302-001 An operator at the end of a line is reported; -002 An operator that starts a line outside its column is reported; -003 The diagnostic is on the operator; -004 The fix lays out the whole chain; -005 A comment behind an operator at the end of a line withholds the fix.
- RH5303-001 Braces outside the column of `new` are reported; -002 Elements outside their own line or column are reported; -003 A misaligned complex element is reported; -004 Where the diagnostic is; -005 The fix lays out the collection initializer.
- RH5304-001 A split assignment is reported; -002 A wrapped collection initializer that is not a block is reported; -003 The diagnostic is on the assignment; -004 The fix lays out the assignment.
- RH5305-001 Wrapped brackets out of column are reported; -002 Wrapped elements that share a line are reported; -003 The diagnostic is on the collection expression; -004 The fix lays out the collection expression.
- RH5306-001 Wrapped brackets out of column are reported; -002 Wrapped inner patterns that share a line are reported; -003 The diagnostic is on the list pattern; -004 The fix lays out the list pattern.
- RH5307-001 A wrapped element access is reported; -002 A wrapped element access the formatter cannot join is not reported; -003 Where the diagnostic is; -004 The fix joins the argument list.
- RH5308-001 An operator that does not start its line is reported; -002 An operator that starts its line outside its column is reported; -003 The diagnostic is on the first misplaced operator; -004 The fix lays out the conditional expression.
- RH5401-001 An arrow or expression below the name is reported; -002 The diagnostic is on the property; -003 The fix puts the property on one line; -004 The fix is withheld when the property stays on several lines.
- RH5402-001 An opening brace on the line of its statement is reported; -002 The diagnostic is on the opening brace; -003 The fix moves the brace to its own line.
- RH5403-001 A block with statements on one line is reported; -002 The diagnostic is on the opening brace; -003 The fix lays out the statement.
- RH5404-001 A type body on one line is reported; -002 The diagnostic is on the type's name; -003 The fix lays out the declaration.
- RH5405-001 An unbraced body on one line is reported; -002 The diagnostic is on the body.
- RH5406-001 An unbraced body on several lines is reported; -002 The diagnostic is on the body.
- RH5407-001 A branch without braces next to one with braces is reported; -002 The diagnostic is on the body without braces.
- RH5408-001 A simple auto-property on several lines is reported; -002 A property that is not simple, or that cannot be joined, is not reported; -003 A comment or directive that blocks the join is not reported; -004 The diagnostic covers the declaration without its attributes; -005 The fix puts the declaration on one line.

Uses that reach outside the batch: FMT-BREAK-001, -002, -006, -014, -017, -018, -021, -022, -024, -026; FMT-INDENT-018, -020, -022, -023, -025; FMT-STRUCT-001, -017; SHARED-GAP-001 (RH5402's provider derives from `CommentSafeSpanReplacementCodeFixProviderBase`). `SyntaxNodeUtilities` is named by no spec.

## Examples for the tests

Every requirement above. How the examples were established, so the next role can try to break them:

- Each example was run through a scratch harness built on `Reihitsu.Tooling` (`CodeFixTargetResolver`, the real analyzers and providers, latest language version, a DLL project): the markup was stripped, the source re-analyzed, and the diagnostics re-marked; every one of the 55 example blocks reproduced its markup exactly. For a block followed by a result block, the fixes were applied one after another, first fixable diagnostic first, until none was offered, and the text matched the result block exactly. "is reported, and no code fix is offered" blocks registered no action for any diagnostic.
- `scripts/test.sh --project architecture --filter "FullyQualifiedName~SpecConsistencyTests|FullyQualifiedName~SpecStructureTests"`: 26 passed. `RequirementCoverageTests` is expected to fail until the tests are tagged.
- Shared requirements are covered through the rule tests: SHARED-INIT-001 by RH5301's comment-prefixed tests (`VerifyNoDiagnosticForCommentPrefixed*`, `VerifyShiftedCommentBeforeAssignmentIsDetectedAndFixed`) and RH5303/RH5304's; SHARED-INIT-002 by RH5303's complex-element tests; SHARED-BLOCK-001 by RH5402/RH5403 tests (`VerifyCatchBlockIsExcluded`, `VerifyOrdinaryAndVariableForeachBodiesAreDetected`, `VerifyAdditionalStatementParentsAreDetected`); SHARED-BRACE-001/-002 by RH5405–RH5407 tests.
- RH5401-002's example (attributed property) is reported without a fix; that fix gap is open question RH5401 Q1, so a test of RH5401-002 asserts the location only, not the absence of a fix.
- Fix examples were chosen so that the fix changes nothing outside the reported construct (no blank line inserted in front of a statement, no documentation comment, no other body). Tests that pin such extra changes today (for example RH5405's `VerifyBlankLineIsInsertedAboveFixedStatement*`) stay as characterization tests but are not tagged with these fix requirements.

## Open questions to file

One issue per spec, titled `SDD open questions: <spec path>`, sub-issue of thoenissen/Reihitsu#912. None blocks this run.

### `sdd/specs/shared/statement-braces.md`

1. **Decided by principle P1 (#912)** — SHARED-FIX-003. The brace fix duplicates a comment that trails the body. Input (inside `void M(bool a, bool b)`): `if (a)⏎    M(a, b); // trail`. RH5405 reports `M(a, b);`; the fix produces `if (a)⏎{⏎    M(a, b); // trail⏎} // trail`. Document formatting gives the comment once (FMT-STRUCT-002), and adding a second copy changes comment text the user wrote. Same base class, so RH5406 and RH5407 are affected alike. Touches SHARED-BRACE-002, SHARED-FIX-003, FMT-STRUCT-002, FMT-PIPE-010.
2. **Decided by principle P9 (#912)** — SHARED-FIX-006 (#1138). The fix formats the whole `if` statement, so the other branch is rewritten too. Input: `if (a)⏎    M(a, b);⏎else⏎{⏎    int  x = 1;⏎}`. RH5405 and RH5407 report `M(a, b);`; the fix also turns `int  x = 1;` into `int x = 1;`. Touches SHARED-BRACE-002.
3. Normal open question (P9 against the existing tests). The fix inserts a blank line in front of the `if` when it follows another statement: `M(a, b);⏎if (a)⏎    M(a, b);` → `M(a, b);⏎⏎if (a)⏎{…}`. The tests `VerifyBlankLineIsInsertedAboveFixedStatementAfterDeclaration`, `…AfterClosingBrace`, and `VerifyBlankLinesAreInsertedAboveAdjacentFixedStatements` (RH5405) pin that blank line as intended, while P9 says a fix changes only the code its diagnostic is about. Reading A (P9): the blank line is outside the diagnostic and is not inserted. Reading B (P10, the formatter's layout of the changed statement includes the blank line in front of it): it stays. Recommendation: reading A, consistent with #1140. Touches SHARED-BRACE-002.
4. **Instance of #1150** (one line for the lead): a body whose trivia holds a directive — `if (a)⏎#if DEBUG⏎    M(a, b);⏎#else⏎    M(b, a);⏎#endif` — is reported by RH5405; the formatter refuses to brace it (FMT-STRUCT-003), and the offered fix returns the document unchanged.

### `sdd/specs/analyzer/RH5301.md`

1. **Decided by principle P11 (#912)**. A nested object initializer assigned to a member is not checked: `new C⏎{⏎    Inner = { P = 1,⏎       Q = 2 }⏎}` is not reported by RH5301 (nor by any other rule), while the formatter lays it out (FMT-BREAK-021, FMT-INDENT-023). It is an object initializer, the construct the rule names. Touches RH5301-001, RH5301-002.
2. **Decided by principle P11 (#912)**. A member on the line of the opening brace, in the member column, is not reported: `var a = new C⏎        {   P = 1,⏎            Q = 2⏎        };` (P four columns right of `new`). The formatter moves `P = 1,` to its own line (FMT-BREAK-021). Touches RH5301-002.

### `sdd/specs/analyzer/RH5302.md`

1. **Decided by principle P12 (#912)** (and P7). An operator whose line starts with a comment is reported by its own column: `var z = a⏎    /* c */ && b;` reports `&&`. P12: the line's first element is the comment, so the line is not reported. The fix then inserts a blank line in front of the comment and leaves the diagnostic (SHARED-FIX-001); a second application changes nothing (SHARED-FIX-005). Related: #1147 (same principle for RH5110, RH5203, RH5206), #1148 (blank line before a line-leading comment). Touches RH5302-002, RH5302-004.
2. **Decided by principle P11 (#912)**. An operator alone on its line is not reported: `var x = a⏎        &&⏎        b;` (operator in column). The formatter joins it to `&& b` (FMT-BREAK-017). Touches RH5302-002.

### `sdd/specs/analyzer/RH5303.md`

1. **Decided by principle P9 (#912)**. The fix formats the whole enclosing statement (or `=` clause or `=>` clause), not only the creation: `if (new List<int> { 1 }.Count > 0)⏎{⏎    int  x = 1;⏎}` → the fix also turns `int  x` into `int x`. Touches RH5303-005, SHARED-FIX-006.

### `sdd/specs/analyzer/RH5304.md`

1. Normal open question (P4). A nested collection initializer on one line (`L = { 1, 2 }` inside `new C { … }`) is not reported, and the rule page says it is accepted and not rewritten; the formatter expands it to the multi-line block layout (FMT-BREAK-021), and the RH5304 fix of another assignment in the same statement expands it as well. Reading A: the one-line form is intended, and the formatter (and the fix scope) are wrong. Reading B: the formatter is right, and RH5304 should report the one-line form. Touches RH5304-001, RH5304-002, RH5304-004, FMT-BREAK-021.
2. **Decided by principle P11 (#912)**. A nested collection assignment inside a nested object initializer is not checked: `new C⏎{⏎    Inner =⏎    {⏎        L = {⏎          1⏎            }⏎    }⏎}` is not reported, because only the members of an object creation's own initializer are checked. Touches RH5304-001, RH5304-002.
3. **Decided by principle P9 (#912)**. The fix formats the whole enclosing statement: in an `if` condition it rewrites the `if` body (`int  x` → `int x`), and in the same initializer it rewrites every other assignment, including an unreported `L = { 1, 2 }`. Touches RH5304-004, SHARED-FIX-006.

### `sdd/specs/analyzer/RH5305.md`

1. **Decided by principle P11 (#912)**. The column of the elements of a wrapped collection expression is not checked: `int[] f = [⏎      1,⏎                2⏎          ];` (brackets in one column, elements on their own lines but out of column) is not reported; the formatter indents them one level deeper than `[` (FMT-INDENT-025). Touches RH5305-002.
2. **Decided by principle P9 (#912)**. The fix formats the nearest enclosing statement, `=` clause, `=>` clause, lambda, switch arm, or argument: for `foreach (var x in (int[])[1,⏎ 2])⏎{⏎    int  z = 1;⏎}` it also turns `int  z` into `int z`. Touches RH5305-004, SHARED-FIX-006.

### `sdd/specs/analyzer/RH5306.md`

1. Normal open question (P4). A list pattern with a `//` or `/* */` comment or a directive inside it, in front of its `[` on the previous line, or behind its `]` (for example `v is [1,⏎ 2] // c`) is not reported; the formatter lays all of these out (verified with `scripts/format.sh --dry-run`), and RH5305 reports and fixes the same shapes for collection expressions. Reading A: the exemption is intended. Reading B: they are reported like RH5305's, with the fix withheld only where a comment would have to move. Touches RH5306-001, RH5306-002.
2. **Decided by principle P13 (#912)**. A `///` comment inside a list pattern does not exempt it, unlike `//` and `/* */`: `v is [1,⏎ /// doc⏎ 2]` is reported, and the fix inserts a blank line in front of the `///` line. Touches RH5306-001, RH5306-004.
3. Normal open question (P4). A list pattern with an inner pattern that spans several lines is not reported: `v is [{ Length: 1,⏎ Rank: 1 }, "b",⏎ "c"]`; the formatter puts every inner pattern on its own line (FMT-BREAK-013, FMT-BREAK-022). Touches RH5306-001, RH5306-002.
4. **Decided by principle P11 (#912)**. The column of the inner patterns is not checked (same shape as RH5305 Q1); the formatter re-indents them (FMT-INDENT-025). Touches RH5306-002.
5. **Decided by principle P9 (#912)**. The fix formats the enclosing statement or switch arm: `if (v is [1,⏎ 2])⏎{⏎    int  x = 1;⏎}` → also `int x`. Touches RH5306-004, SHARED-FIX-006.

### `sdd/specs/analyzer/RH5307.md`

1. Normal open question. A null-conditional element access is not checked: `c?[1,⏎ 2]` is not reported (its argument list belongs to an element binding expression); the formatter leaves it as well. Is `?[…]` part of the rule? Touches RH5307-001, FMT-BREAK-014.
2. **Decided by principle P9 (#912)**. The fix formats the enclosing statement, `=` clause, `=>` clause, or argument: `if (m[1,⏎ 2] > 0)⏎{⏎    int  y = 1;⏎}` → also `int y`. Touches RH5307-004, SHARED-FIX-006.

### `sdd/specs/analyzer/RH5308.md`

1. **Decided by principle P12 (#912)** (and P7). `var y = a⏎    /* c */ ? 1⏎    : 2;` reports `?` by its own column; P12 says the comment leads the line, so it is not reported. The fix inserts a blank line and leaves the diagnostic (SHARED-FIX-001); the next application changes nothing (SHARED-FIX-005). Touches RH5308-002, RH5308-004.
2. **Decided by principle P7 (#912)** — SHARED-FIX-001, SHARED-FIX-005. A `?` followed by a comment: `var x = a ? // c⏎        1⏎        : 2;` is reported and the fix is offered; it produces `var x = a ? // c⏎        1⏎                    : 2;` (the `1` at column 8, the `?` still behind `a`), which is still reported, and a second application changes nothing. Touches RH5308-001, RH5308-004.
3. **Decided by principle P11 (#912)**. An operator alone on its line is not reported: `a⏎        ?⏎        1⏎        : 2` (operators in column); the formatter joins `? 1` (FMT-BREAK-018). Touches RH5308-002.
4. Normal open question (P4). A nested conditional on one line, `a ? 1 : b ? 2 : 3`, is not reported (the rule page says a one-line conditional is never reported), while the formatter always spreads a conditional that contains a nested one (FMT-BREAK-018). Reading A: the one-line nested form is intended, and FMT-BREAK-018 is wrong. Reading B: RH5308 reports it. Touches RH5308-001, FMT-BREAK-018.
5. **Decided by principle P9 (#912)**. The fix formats the whole enclosing statement or member: `if ((a⏎    ? 1⏎    : 2) > 0)⏎{⏎    int  y = 1;⏎}` → also `int y`. Touches RH5308-004, SHARED-FIX-006.

### `sdd/specs/analyzer/RH5401.md`

1. Normal open question (guard scope; `AGENTS.md`, Code-fix coverage). An attributed property gets no fix: `[Obsolete]⏎int E⏎    => 1;` is reported without a fix, because the guard requires the whole formatted declaration, attribute line included, to be one line; the formatter would produce `[Obsolete]⏎int E => 1;`, which clears the diagnostic. Touches RH5401-002, RH5401-003, RH5401-004.
2. **Decided by principle P9 (#912)**. The fix rewrites the property's documentation comment: `/// <summary>Doc</summary>⏎int I⏎    => 1;` → the comment becomes three lines (`/// <summary>`, `/// Doc`, `/// </summary>`). Same pattern as the RH5111/RH5113 instances of #1140. Touches RH5401-003, SHARED-FIX-006.

### `sdd/specs/analyzer/RH5403.md`

1. **Decided by principle P9 (#912)**. The fix formats the whole owning statement: for the block of an `if`, the `else` body is rewritten too (`else⏎{⏎    int  x = 1;⏎}` → `int x`). Touches RH5403-003, SHARED-FIX-006.
2. Normal open question (P9 against the formatter's statement spacing; same question as `shared/statement-braces.md` Q3). The fix inserts a blank line in front of a `do`, `for`, `using`, or `if` that follows another statement: `while (a) { }⏎do { M(a); } while (a);` → a blank line before `do` (and before the next statement). Reading A: not inserted (P9). Reading B: kept as part of the formatter's layout of the statement. Touches RH5403-003.

### `sdd/specs/analyzer/RH5404.md`

1. Normal open question. A one-line extension block, `static class X⏎{⏎    extension(int value) { public int Doubled => value * 2; }⏎}`, is reported with no source location (`Location.None`: the extension block has no identifier), so an IDE cannot show it on the code or offer the fix. When the fix is invoked anyway, it formats the type that contains the file's first token (`X`) and leaves `extension(int value) {` on one line. The test `VerifySingleLineExtensionBlockIsFlagged` pins the location-less diagnostic and says correcting it is "tracked separately", but no issue tracks it. The rule page says extension blocks are expanded by hand. Touches RH5404-001, RH5404-002, RH5404-003.
2. **Decided by principle P11 (#912)**. Members that share a line with a brace are not reported when the braces are on different lines: `class J { int _y; int _z;⏎}`. The rule page says the opening brace, the members, and the closing brace each appear on their own line, and the formatter splits them (FMT-BREAK-002, FMT-BREAK-007). Touches RH5404-001.
3. **Decided by principle P9 (#912)**. The fix formats the members' own code: `class D { int  _x; }` → `int _x;`. Touches RH5404-003, SHARED-FIX-006.

### `sdd/specs/analyzer/RH5408.md`

1. **Decided by principle P9 (#912)**. The fix rewrites the property's documentation comment: `/// <summary>Doc</summary>⏎int K⏎{⏎    get;⏎}` → the comment becomes three lines. Touches RH5408-005, SHARED-FIX-006.

`shared/initializer-layout.md`, `shared/statement-blocks.md`, RH5402, RH5405, RH5406, and RH5407 have no questions of their own.

## Follow-up candidates outside this run's scope

- Formatter (for `SDD open questions: sdd/specs/formatter/line-breaks.md`, or a bug against FMT-BREAK-017/-018): joining an operator that stands alone on its line inserts a blank line in front of it. `var x = a⏎        &&⏎        b;` → `var x = a⏎⏎        && b;` (`scripts/trace.sh`: the blank line appears in `LineBreakPhase`). The same happens for `?` alone on its line.
- P11 new-rule candidates for the run report (constructs the rules do not name, which the formatter lays out): anonymous object creations (FMT-BREAK-021) next to RH5301; binary operators other than `&&`/`||` (FMT-BREAK-017) next to RH5302; expression-bodied indexers (FMT-BREAK-026) next to RH5401; auto-indexer accessor lists (FMT-BREAK-024) next to RH5408; `try`/`catch`/`finally`, `checked`/`unchecked`/`unsafe` blocks (FMT-BREAK-002) next to RH5402/RH5403; property and parenthesized patterns (FMT-BREAK-022) next to RH5306.

## Decisions needed

_None._
