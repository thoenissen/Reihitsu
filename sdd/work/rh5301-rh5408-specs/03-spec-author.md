Role: spec author
Commit: 1c60f6bb96f13281b469419de2c89ed3d3dd94a4
Verdict: spec written

Repair round on the skeptic's findings S1–S8 (`02-skeptic.md`). Each finding was re-derived with the scratch harness (real analyzers and providers through `Reihitsu.Tooling`); all eight reproduce. The skeptic's suggested wording was used as a suggestion only.

## Findings

- **S1 — applied.** SHARED-INIT-001 now applies only to a token in front of which at most one comment starts on the token's own line; the two-comment shape (`/* a */ /* b */ P = 1`, reported, no-op fix) is left out and filed (initializer-layout Q1, P12 and P7).
- **S2 — applied.** SHARED-INIT-001 now decides the extra level by where the comment starts: a comment that starts on an earlier line than a closing brace starts one level deeper, also when it ends on the brace's line. Both sides are in the examples (`/* last⏎ one */ };` one level deeper is not reported; in the column of `new` it is reported on the creation).
- **S3 — applied.** RH5303-002, RH5304-002, RH5305-002, RH5306-002, and SHARED-INIT-002 now state the code's condition: an element on the line of the opening or closing delimiter, or on a line on which an earlier element starts. The RH5305 case (an element on the last line of a multi-line element) is filed (RH5305 Q3, P11). The negative sentences of RH5305-002 and RH5306-002 were dropped, because they also claimed the unchecked element columns (RH5305 Q1, RH5306 Q4). The optional single shared statement was not made: each rule implements the condition on its own construct with no shared type to own it, and the wording is now identical.
- **S4 — applied.** RH5305-001 now applies only to a closing bracket with no comment in front of it on its line; the comment-led bracket (reported, no-op fix) is filed (RH5305 Q4, P12 and P7).
- **S5 — applied.** RH5306-001 now names the comments the code counts: inside the pattern, in front of `[` below the line of the token before the pattern, and behind `]` on its line; a comment behind the token in front of the pattern does not count. RH5306-002's example shows that case reported.
- **S6 — applied.** RH5307-004 is narrowed to an implicit element access and to an element access that is not inside another bracketed argument list and whose receiver does not start with an element access. The withheld cases are filed (RH5307 Q3, normal, guard scope).
- **S7 — applied.** SHARED-BRACE-002 states the brace column only for an `if` that starts its line; the `else if` case is filed (statement-braces Q5, P10) and for RH5403 (RH5403 Q3, P10 and P9). RH5403-003 states no column and stays.
- **S8 — applied.** RH5404-003 now says that the members of a class, struct, interface, record, or record struct each go on their own line (FMT-BREAK-007, added to `Uses`), that enum members move between the braces and keep their layout, and that an empty enum keeps its braces; its example shows both enums. `documentation/rules/RH5404.md` ("How to fix it") now says the same.
- **Filing notes — applied.** SHARED-FIX-006 and SHARED-FIX-007 are cited as "planned in #1138". RH5306 Q1 and Q3 are now marked P11: the exemptions are gaps inside the construct the rule names, and the formatter lays out those shapes. RH5304 Q1 and RH5308 Q4 stay normal two-reading questions, because their rule pages state the opposite intent.

No requirement ID was added, retired, or renumbered in this round; the ledger and the index are unchanged.

## Changed requirements

Clarified (wording repaired to the code, IDs unchanged): SHARED-INIT-001, SHARED-INIT-002, SHARED-BRACE-002, RH5303-002, RH5304-002, RH5305-001, RH5305-002, RH5306-001, RH5306-002, RH5307-004, RH5404-003. All other requirements as listed in `01-spec-author.md`.

## Examples for the tests

Every requirement, as in `01-spec-author.md`. All 55 example blocks were re-run after the repair: markup reproduced exactly, and every result block matched the single fixes applied one after another. `scripts/test.sh --project architecture --filter "FullyQualifiedName~SpecConsistencyTests|FullyQualifiedName~SpecStructureTests"`: 26 passed. `scripts/test.sh --project analyzer --filter "FullyQualifiedName~RuleDocumentationConventionTests|FullyQualifiedName~RuleDocumentationExampleTests"`: 5 passed.

Do not tag the shapes left out above as fix requirements: the S1 and S4 no-op fixes, RH5302 Q1, RH5308 Q1/Q2, the #1150 directive body, the S6 withheld fixes, and the `else if` layout of S7 are characterization only.

## Open questions to file

The complete list; new or changed against `01-spec-author.md`: `shared/statement-braces.md` Q5, `shared/initializer-layout.md` Q1, RH5305 Q3 and Q4, RH5306 Q1 and Q3 (now marked P11), RH5307 Q3, RH5403 Q3, and "planned in #1138" wherever SHARED-FIX-006 or SHARED-FIX-007 is cited. One issue per spec, titled `SDD open questions: <spec path>`, sub-issue of thoenissen/Reihitsu#912. None blocks this run.

### `sdd/specs/shared/statement-braces.md`

1. **Decided by principle P1 (#912)** — SHARED-FIX-003. The brace fix duplicates a comment that trails the body. Input (inside `void M(bool a, bool b)`): `if (a)⏎    M(a, b); // trail`. RH5405 reports `M(a, b);`; the fix produces `if (a)⏎{⏎    M(a, b); // trail⏎} // trail`. Document formatting gives the comment once (FMT-STRUCT-002), and adding a second copy changes comment text the user wrote. Same base class, so RH5406 and RH5407 are affected alike. Touches SHARED-BRACE-002, SHARED-FIX-003, FMT-STRUCT-002, FMT-PIPE-010.
2. **Decided by principle P9 (#912)** — SHARED-FIX-006 (planned in #1138). The fix formats the whole `if` statement, so the other branch is rewritten too. Input: `if (a)⏎    M(a, b);⏎else⏎{⏎    int  x = 1;⏎}`. RH5405 and RH5407 report `M(a, b);`; the fix also turns `int  x = 1;` into `int x = 1;`. Touches SHARED-BRACE-002.
3. Normal open question (P9 against the existing tests). The fix inserts a blank line in front of the `if` when it follows another statement: `M(a, b);⏎if (a)⏎    M(a, b);` → `M(a, b);⏎⏎if (a)⏎{…}`. The tests `VerifyBlankLineIsInsertedAboveFixedStatementAfterDeclaration`, `…AfterClosingBrace`, and `VerifyBlankLinesAreInsertedAboveAdjacentFixedStatements` (RH5405) pin that blank line as intended, while P9 says a fix changes only the code its diagnostic is about. Reading A (P9): the blank line is outside the diagnostic and is not inserted. Reading B (P10, SHARED-FIX-007 planned in #1138: the formatter's layout of the changed statement includes the blank line in front of it): it stays. Recommendation: reading A, consistent with #1140. Touches SHARED-BRACE-002.
4. **Instance of #1150** (one line for the lead): a body whose trivia holds a directive — `if (a)⏎#if DEBUG⏎    M(a, b);⏎#else⏎    M(b, a);⏎#endif` — is reported by RH5405; the formatter refuses to brace it (FMT-STRUCT-003), and the offered fix returns the document unchanged.
5. **Decided by principle P10 (#912)** — SHARED-FIX-007 (planned in #1138). For the body of an `else if`, the fix lays out the inner `if` from its own column, which document formatting does not do. Input: `if (a)⏎{⏎    M(a, b);⏎}⏎else if (b)⏎    M(a, b);`. RH5405 reports the inner `M(a, b);`; the fix puts `{`, the body, and `}` at the column of the inner `if` keyword (the `else` column + 5); `scripts/format.sh` puts the braces at the `else` column, also on the fix result. Same base class, so RH5406 and RH5407 alike. Touches SHARED-BRACE-002.

### `sdd/specs/shared/initializer-layout.md`

1. **Decided by principle P12 (#912)**, also P7 (SHARED-FIX-005). With two comments in front of a member on its line, the second comment is checked against the member column too: `var c = new C⏎        {⏎            /* a */ /* b */ P = 1⏎        };` (`/* a */` in the member column) reports `P = 1` (RH5301); the fix is offered and changes nothing, and `scripts/format.sh` leaves the line as it is. P12: the line's first element, `/* a */`, is in its column, so the line is not reported. The same predicate serves RH5303 and RH5304. Touches SHARED-INIT-001, SHARED-FIX-005.

### `sdd/specs/analyzer/RH5301.md`

1. **Decided by principle P11 (#912)**. A nested object initializer assigned to a member is not checked: `new C⏎{⏎    Inner = { P = 1,⏎       Q = 2 }⏎}` is not reported by RH5301 (nor by any other rule), while the formatter lays it out (FMT-BREAK-021, FMT-INDENT-023). It is an object initializer, the construct the rule names. Touches RH5301-001, RH5301-002.
2. **Decided by principle P11 (#912)**. A member on the line of the opening brace, in the member column, is not reported: `var a = new C⏎        {   P = 1,⏎            Q = 2⏎        };` (P four columns right of `new`). The formatter moves `P = 1,` to its own line (FMT-BREAK-021). Touches RH5301-002.

### `sdd/specs/analyzer/RH5302.md`

1. **Decided by principle P12 (#912)** (and P7). An operator whose line starts with a comment is reported by its own column: `var z = a⏎    /* c */ && b;` reports `&&`. P12: the line's first element is the comment, so the line is not reported. The fix then inserts a blank line in front of the comment and leaves the diagnostic (SHARED-FIX-001); a second application changes nothing (SHARED-FIX-005). Related: #1147 (same principle for RH5110, RH5203, RH5206), #1148 (blank line before a line-leading comment). Touches RH5302-002, RH5302-004.
2. **Decided by principle P11 (#912)**. An operator alone on its line is not reported: `var x = a⏎        &&⏎        b;` (operator in column). The formatter joins it to `&& b` (FMT-BREAK-017). Touches RH5302-002.

### `sdd/specs/analyzer/RH5303.md`

1. **Decided by principle P9 (#912)**. The fix formats the whole enclosing statement (or `=` clause or `=>` clause), not only the creation: `if (new List<int> { 1 }.Count > 0)⏎{⏎    int  x = 1;⏎}` → the fix also turns `int  x` into `int x`. Touches RH5303-005, SHARED-FIX-006 (planned in #1138).

### `sdd/specs/analyzer/RH5304.md`

1. Normal open question (P4). A nested collection initializer on one line (`L = { 1, 2 }` inside `new C { … }`) is not reported, and the rule page says it is accepted and not rewritten; the formatter expands it to the multi-line block layout (FMT-BREAK-021), and the RH5304 fix of another assignment in the same statement expands it as well. Reading A: the one-line form is intended, and the formatter (and the fix scope) are wrong. Reading B: the formatter is right, and RH5304 should report the one-line form. Touches RH5304-001, RH5304-002, RH5304-004, FMT-BREAK-021.
2. **Decided by principle P11 (#912)**. A nested collection assignment inside a nested object initializer is not checked: `new C⏎{⏎    Inner =⏎    {⏎        L = {⏎          1⏎            }⏎    }⏎}` is not reported, because only the members of an object creation's own initializer are checked. Touches RH5304-001, RH5304-002.
3. **Decided by principle P9 (#912)**. The fix formats the whole enclosing statement: in an `if` condition it rewrites the `if` body (`int  x` → `int x`), and in the same initializer it rewrites every other assignment, including an unreported `L = { 1, 2 }`. Touches RH5304-004, SHARED-FIX-006 (planned in #1138).

### `sdd/specs/analyzer/RH5305.md`

1. **Decided by principle P11 (#912)**. The column of the elements of a wrapped collection expression is not checked: `int[] f = [⏎      1,⏎                2⏎          ];` (brackets in one column, elements on their own lines but out of column) is not reported; the formatter indents them one level deeper than `[` (FMT-INDENT-025). Touches RH5305-002.
2. **Decided by principle P9 (#912)**. The fix formats the nearest enclosing statement, `=` clause, `=>` clause, lambda, switch arm, or argument: for `foreach (var x in (int[])[1,⏎ 2])⏎{⏎    int  z = 1;⏎}` it also turns `int  z` into `int z`. Touches RH5305-004, SHARED-FIX-006 (planned in #1138).
3. **Decided by principle P11 (#912)**. An element that starts on the last line of a multi-line element before it is not reported: `int[] a = [⏎    N(1,⏎      2), 3⏎];` (brackets in one column). The analyzer only compares the lines on which elements start; the formatter puts `3` on its own line (FMT-BREAK-022). Touches RH5305-002.
4. **Decided by principle P12 (#912)**, also P7 (SHARED-FIX-005). A closing bracket that a comment precedes on its line is checked by the bracket's own column: `int[] a = [⏎              1⏎          /* end */ ];` (the comment in the column of `[`) is reported; the fix is offered and changes nothing, and `scripts/format.sh` leaves it as it is. P12: the line's first element is the comment, in its column. Touches RH5305-001, SHARED-FIX-005.

### `sdd/specs/analyzer/RH5306.md`

1. **Decided by principle P11 (#912)** (a gap inside the construct the rule names; the exemption is a deliberate guard in the analyzer, so the follow-up changes behavior). A list pattern with a `//` or `/* */` comment or a directive inside it, in front of its `[` below the line of the token before it, or behind its `]` (for example `v is [1,⏎ 2] // c`) is not reported; the formatter lays all of these out (verified with `scripts/format.sh --dry-run`), and RH5305 reports and fixes the same shapes for collection expressions. A comment behind the token in front of the pattern (`v is // c⏎[1,⏎ 2]`) does not exempt it. Touches RH5306-001, RH5306-002.
2. **Decided by principle P13 (#912)**. A `///` comment inside a list pattern does not exempt it, unlike `//` and `/* */`: `v is [1,⏎ /// doc⏎ 2]` is reported, and the fix inserts a blank line in front of the `///` line. Touches RH5306-001, RH5306-004.
3. **Decided by principle P11 (#912)** (same reasoning as Q1). A list pattern with an inner pattern that spans several lines is not reported: `v is [{ Length: 1,⏎ Rank: 1 }, "b",⏎ "c"]`; the formatter puts every inner pattern on its own line (FMT-BREAK-013, FMT-BREAK-022). Touches RH5306-001, RH5306-002.
4. **Decided by principle P11 (#912)**. The column of the inner patterns is not checked (same shape as RH5305 Q1); the formatter re-indents them (FMT-INDENT-025). Touches RH5306-002.
5. **Decided by principle P9 (#912)**. The fix formats the enclosing statement or switch arm: `if (v is [1,⏎ 2])⏎{⏎    int  x = 1;⏎}` → also `int x`. Touches RH5306-004, SHARED-FIX-006 (planned in #1138).

### `sdd/specs/analyzer/RH5307.md`

1. Normal open question. A null-conditional element access is not checked: `c?[1,⏎ 2]` is not reported (its argument list belongs to an element binding expression); the formatter leaves it as well. Is `?[…]` part of the rule? Touches RH5307-001, FMT-BREAK-014.
2. **Decided by principle P9 (#912)**. The fix formats the enclosing statement, `=` clause, `=>` clause, or argument: `if (m[1,⏎ 2] > 0)⏎{⏎    int  y = 1;⏎}` → also `int y`. Touches RH5307-004, SHARED-FIX-006 (planned in #1138).
3. Normal open question (guard scope; `AGENTS.md`, Code-fix coverage). The fix guards a different argument list than the reported one: it resolves a list from the diagnostic's first token, which is the receiver's first token, so it checks the bracketed argument list that encloses the access, or the argument list of an element access that starts the receiver. Reported, no fix offered: `var a = x[m[1,⏎            2]];`, `var a = j[0 /* c */][1,⏎                     2];`, `var a = j[N(1,⏎            2)][1,⏎                2];`. `scripts/format.sh` joins the reported list in the first and third (`x[m[1, 2]]`, `j[N(1,⏎ 2)][1, 2]`); with a plain receiver (`j[0][1,⏎ 2]`) the fix is offered and joins. The fix refuses inputs it could rewrite safely. Touches RH5307-004.

### `sdd/specs/analyzer/RH5308.md`

1. **Decided by principle P12 (#912)** (and P7). `var y = a⏎    /* c */ ? 1⏎    : 2;` reports `?` by its own column; P12 says the comment leads the line, so it is not reported. The fix inserts a blank line and leaves the diagnostic (SHARED-FIX-001); the next application changes nothing (SHARED-FIX-005). Touches RH5308-002, RH5308-004.
2. **Decided by principle P7 (#912)** — SHARED-FIX-001, SHARED-FIX-005. A `?` followed by a comment: `var x = a ? // c⏎        1⏎        : 2;` is reported and the fix is offered; it produces `var x = a ? // c⏎        1⏎                    : 2;` (the `1` at column 8, the `?` still behind `a`), which is still reported, and a second application changes nothing. Touches RH5308-001, RH5308-004.
3. **Decided by principle P11 (#912)**. An operator alone on its line is not reported: `a⏎        ?⏎        1⏎        : 2` (operators in column); the formatter joins `? 1` (FMT-BREAK-018). Touches RH5308-002.
4. Normal open question (P4). A nested conditional on one line, `a ? 1 : b ? 2 : 3`, is not reported (the rule page says a one-line conditional is never reported), while the formatter always spreads a conditional that contains a nested one (FMT-BREAK-018). Reading A: the one-line nested form is intended, and FMT-BREAK-018 is wrong. Reading B: RH5308 reports it. Touches RH5308-001, FMT-BREAK-018.
5. **Decided by principle P9 (#912)**. The fix formats the whole enclosing statement or member: `if ((a⏎    ? 1⏎    : 2) > 0)⏎{⏎    int  y = 1;⏎}` → also `int y`. Touches RH5308-004, SHARED-FIX-006 (planned in #1138).

### `sdd/specs/analyzer/RH5401.md`

1. Normal open question (guard scope; `AGENTS.md`, Code-fix coverage). An attributed property gets no fix: `[Obsolete]⏎int E⏎    => 1;` is reported without a fix, because the guard requires the whole formatted declaration, attribute line included, to be one line; the formatter would produce `[Obsolete]⏎int E => 1;`, which clears the diagnostic. Touches RH5401-002, RH5401-003, RH5401-004.
2. **Decided by principle P9 (#912)**. The fix rewrites the property's documentation comment: `/// <summary>Doc</summary>⏎int I⏎    => 1;` → the comment becomes three lines (`/// <summary>`, `/// Doc`, `/// </summary>`). Same pattern as the RH5111/RH5113 instances of #1140. Touches RH5401-003, SHARED-FIX-006 (planned in #1138).

### `sdd/specs/analyzer/RH5403.md`

1. **Decided by principle P9 (#912)**. The fix formats the whole owning statement: for the block of an `if`, the `else` body is rewritten too (`else⏎{⏎    int  x = 1;⏎}` → `int x`). Touches RH5403-003, SHARED-FIX-006 (planned in #1138).
2. Normal open question (P9 against the formatter's statement spacing; same question as `shared/statement-braces.md` Q3). The fix inserts a blank line in front of a `do`, `for`, `using`, or `if` that follows another statement: `while (a) { }⏎do { M(a); } while (a);` → a blank line before `do` (and before the next statement). Reading A: not inserted (P9). Reading B: kept as part of the formatter's layout of the statement. Touches RH5403-003.
3. **Decided by principle P10 (#912)** — SHARED-FIX-007 (planned in #1138), and P9. For a block of an `else if`, the fix lays out the inner `if` from its own column: `else if (b) { M(a, b); }` → the block lands at the inner `if` column, where document formatting puts it at the `else` column. For `else if (b)⏎{⏎    M(a, b);⏎}⏎else { M(a, b); }`, where the last block is reported, the fix also re-indents the correct `else if` body and moves the `else` to that column. Touches RH5403-003.

### `sdd/specs/analyzer/RH5404.md`

1. Normal open question. A one-line extension block, `static class X⏎{⏎    extension(int value) { public int Doubled => value * 2; }⏎}`, is reported with no source location (`Location.None`: the extension block has no identifier), so an IDE cannot show it on the code or offer the fix. When the fix is invoked anyway, it formats the type that contains the file's first token (`X`) and leaves `extension(int value) {` on one line. The test `VerifySingleLineExtensionBlockIsFlagged` pins the location-less diagnostic and says correcting it is "tracked separately", but no issue tracks it. The rule page says extension blocks are expanded by hand. Touches RH5404-001, RH5404-002, RH5404-003.
2. **Decided by principle P11 (#912)**. Members that share a line with a brace are not reported when the braces are on different lines: `class J { int _y; int _z;⏎}`. The rule page says the opening brace, the members, and the closing brace each appear on their own line, and the formatter splits them (FMT-BREAK-002, FMT-BREAK-007). Touches RH5404-001.
3. **Decided by principle P9 (#912)**. The fix formats the members' own code: `class D { int  _x; }` → `int _x;`. Touches RH5404-003, SHARED-FIX-006 (planned in #1138).

### `sdd/specs/analyzer/RH5408.md`

1. **Decided by principle P9 (#912)**. The fix rewrites the property's documentation comment: `/// <summary>Doc</summary>⏎int K⏎{⏎    get;⏎}` → the comment becomes three lines. Touches RH5408-005, SHARED-FIX-006 (planned in #1138).

`shared/statement-blocks.md`, RH5402, RH5405, RH5406, and RH5407 have no questions of their own.

## Follow-up candidates outside this run's scope

- Formatter (for `SDD open questions: sdd/specs/formatter/line-breaks.md`, or a bug against FMT-BREAK-017/-018): joining an operator that stands alone on its line inserts a blank line in front of it. `var x = a⏎        &&⏎        b;` → `var x = a⏎⏎        && b;` (`scripts/trace.sh`: the blank line appears in `LineBreakPhase`). The same happens for `?` alone on its line.
- P11 new-rule candidates for the run report (constructs the rules do not name, which the formatter lays out): anonymous object creations (FMT-BREAK-021) next to RH5301; binary operators other than `&&`/`||` (FMT-BREAK-017) next to RH5302; expression-bodied indexers (FMT-BREAK-026) next to RH5401; auto-indexer accessor lists (FMT-BREAK-024) next to RH5408; `try`/`catch`/`finally`, `checked`/`unchecked`/`unsafe` blocks (FMT-BREAK-002) next to RH5402/RH5403; property and parenthesized patterns (FMT-BREAK-022) next to RH5306.

## Decisions needed

_None._
