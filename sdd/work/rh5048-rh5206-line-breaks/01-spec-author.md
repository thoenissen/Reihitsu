Role: spec author
Commit: 72b08b004874e7a190d3a6159d9be74bff67e4d3
Verdict: spec written

Lane: specs and tests for existing behavior; no change doc (no intended behavior changes). Every requirement states what the code at the commit above does. My prompt carried no intended fix or verdict.

## How the statements were established

- Read every analyzer and code-fix provider of the 21 rules, `FluentChain`, `FluentChainAnalyzerBase`, `FluentChainCodeFixHelper`, `CollapseTokenGapCodeFixProviderBase`, `CommentSafeSpanReplacementCodeFixProviderBase`, `ParameterListParentPolicy`, the rule pages, and the existing test names.
- Every C# block of the new and changed specs was run through a scratch harness (outside the repository) built on the shipped analyzers and providers (via `Reihitsu.Tooling`), `LanguageVersion.Latest`, `DocumentationMode.Diagnose`, trusted-platform references: each `{|RH####:…|}` span must equal a reported diagnostic and no other diagnostic of that rule may appear; a marked block directly followed by an unmarked block is fixed by applying the first offered fix until the rule reports nothing, and must equal the second block; "no code fix is offered" / "the fix is offered" are checked per diagnostic. Result: 0 mismatches in all 21 specs, `shared/fluent-chain-layout.md`, and `shared/whitespace-removal-fixes.md`. Only compile errors left: the intended syntax error `int _x = ;` in RH5105-003 and RH5106-003 (CS1525).
- Formatter claims in open questions were checked with `scripts/format.sh` / `scripts/trace.sh` on scratch copies.
- `SpecConsistencyTests` and `SpecStructureTests` pass (`scripts/test.sh --project architecture --filter "FullyQualifiedName~Spec"`). `RequirementCoverageTests.EveryRequirementOfAnEnforcedSpecIsCovered` fails with 95 uncovered IDs, which is the test implementer's work (list below).

## Changed requirements

New specs (all requirements added):

- `sdd/specs/analyzer/RH5048.md` — RH5048-001, RH5048-002
- `sdd/specs/analyzer/RH5101.md` — RH5101-001 to RH5101-005
- `sdd/specs/analyzer/RH5102.md` — RH5102-001 to RH5102-004
- `sdd/specs/analyzer/RH5103.md` — RH5103-001 to RH5103-004
- `sdd/specs/analyzer/RH5104.md` — RH5104-001 to RH5104-005
- `sdd/specs/analyzer/RH5105.md` — RH5105-001 to RH5105-005
- `sdd/specs/analyzer/RH5106.md` — RH5106-001 to RH5106-005
- `sdd/specs/analyzer/RH5107.md` — RH5107-001 to RH5107-004
- `sdd/specs/analyzer/RH5108.md` — RH5108-001 to RH5108-004
- `sdd/specs/analyzer/RH5109.md` — RH5109-001 to RH5109-004
- `sdd/specs/analyzer/RH5110.md` — RH5110-001 to RH5110-003
- `sdd/specs/analyzer/RH5111.md` — RH5111-001 to RH5111-005
- `sdd/specs/analyzer/RH5112.md` — RH5112-001 to RH5112-004
- `sdd/specs/analyzer/RH5113.md` — RH5113-001 to RH5113-004
- `sdd/specs/analyzer/RH5114.md` — RH5114-001 to RH5114-003
- `sdd/specs/analyzer/RH5201.md` — RH5201-001 to RH5201-005
- `sdd/specs/analyzer/RH5202.md` — RH5202-001 to RH5202-003
- `sdd/specs/analyzer/RH5203.md` — RH5203-001 to RH5203-003
- `sdd/specs/analyzer/RH5204.md` — RH5204-001 to RH5204-006
- `sdd/specs/analyzer/RH5205.md` — RH5205-001 to RH5205-003
- `sdd/specs/analyzer/RH5206.md` — RH5206-001 to RH5206-003

Shared specs (clarifications of existing behavior that more than one spec relies on):

- SHARED-CHAIN-008 — A chain fix gives the chain its formatted layout — added (used by RH5112 and RH5201)
- SHARED-CHAIN-009 — A chain fix formats the code in front of the chain only when the chain's columns depend on it — added (RH5112, RH5201)
- SHARED-WS-003 — The fix joins a reported token to the token before it — added (RH5105, RH5106, RH5108; the issue's `CollapseTokenGapCodeFixProviderBase` case). The intro sentence of `shared/whitespace-removal-fixes.md` now names this kind of gap.
- Code owners: `FluentChainAnalyzerBase` and `FluentChainCodeFixHelper` added to SHARED-CHAIN; `CollapseTokenGapCodeFixProviderBase` added to SHARED-WS. `sdd/specs/README.md` rows updated, 21 analyzer rows added; 87 IDs inserted into `sdd/specs/requirement-ids.txt` (additions only).
- Existing SHARED-CHAIN-001 to SHARED-CHAIN-007 unchanged; the intro now also names the RH5112/RH5201 code fixes.

Reused, not restated: SHARED-CHAIN-001 to 009, SHARED-COMMENT-001/002 (RH5048), SHARED-USING-010 (RH5114), FMT-BREAK, FMT-INDENT, FMT-RAW, FMT-USING requirements as listed under each spec's `Uses`. No requirement was retired.

Rule pages corrected to match the code (no behavior change): `documentation/rules/RH5107.md` (only parameter lists are checked — the page said "parameter or argument list"), `documentation/rules/RH5106.md` ("last argument" → "last parameter" in the body; the title is the resource title and is unchanged), `documentation/rules/RH5206.md` (a switch expression written on one line is reported too; the page said "multi-line switch expressions"). Nine spec titles were aligned with their rule-page titles.

Deliberately left out of the specs (decided bugs, not refiled): #951 cases (multi-line holes, nested raw strings, tabs) in RH5202; the constraint-clause wrap of #955 in RH5110; the `c.B().P++` / comment / `.ToString()` input of #1058 Q1 in RH5048.

## Examples for the tests

Every requirement ID above needs a `[Requirement("<ID>")]`-tagged test; the examples are verified seeds. `RequirementCoverageTests` currently lists as uncovered all 85 new `RH5048-…`/`RH51…`/`RH52…` IDs plus these shared IDs that the new enforced specs list under `Uses` and that no test tags yet:

- SHARED-CHAIN-001 to SHARED-CHAIN-009 (tag from the RH5048, RH5112, RH5201 tests; SHARED-CHAIN-008/009 examples are taken from `VerifyLinksOfTwoChainsAreFixedTogether`-style and `VerifyFixLeavesCodeBehindTheChainUntouched` / `VerifyFixJoinsFirstCallBehindMisspacedDeclaration` shapes)
- SHARED-USING-010 (RH5114 tests; its second sentence — a first-directive line that begins inside a comment — needs its own case)
- SHARED-WS-003 (RH5105, RH5106, RH5108 fix tests)

Notes for the test implementer:

- RH5105-003 and RH5106-003 need a test whose source has a syntax error (expected compiler diagnostic CS1525).
- RH5204 examples use `→` for a tab character (RH5204-004), as RH5601 does.
- Requirements with a "fix withheld" or "fix offered" clause (RH5101-005, RH5102-004, RH5103-004, RH5109-004, RH5111-005) have existing `GetCodeFixActionsAsync`-style tests that can be tagged.
- Tests that verify one requirement only incidentally should not tag it (`sdd/README.md`).

## Open questions to file

None of these blocks the run; each case is left out of the specs. Existing open-question issues for these specs: none (#918 and #1031 for `formatter/line-breaks.md`, #1066 for `shared/code-fixes.md` are closed), so each needs a new `SDD open questions: <spec path>` issue, a sub-issue of #912.

### `SDD open questions: sdd/specs/formatter/line-breaks.md` (new)

Q — A list wrapped only after its opening parenthesis is joined onto one line.
Input: `M(` / `    1, 2);` inside a method. Real output (`scripts/trace.sh`, LineBreakPhase): `M(1, 2);`. Why it looks wrong: FMT-BREAK-012 says a list wrapped anywhere, "after its opening parenthesis or between two elements", becomes one element per line (`M(1,` / `  2);`). The RH5101 and RH5102 fixes delegate to the formatter and produce the same `M(1, 2)` (RH5102 reports this input because both arguments start on one line of a multi-line list). Touches FMT-BREAK-012, RH5101-004, RH5102-003.

### `SDD open questions: sdd/specs/analyzer/RH5101.md` (new)

Q — Attribute argument lists are not checked (P4). Input: `[A(` / `    1)]` on a method, `A` an attribute with a `params int[]` constructor. Real output: RH5101 reports nothing; the formatter writes `[A(1)]` (FMT-BREAK-012 covers attribute argument lists and says "RH5101, RH5102, and RH5109 report the same shapes"). Touches RH5101-001, FMT-BREAK-012.

### `SDD open questions: sdd/specs/analyzer/RH5102.md` (new)

Q — Attribute argument lists are not checked (P4). Input: `[A(1, 2,` / `   3)]`. Real output: RH5102 reports nothing; the formatter writes one argument per line. Touches RH5102-001, FMT-BREAK-012.

### `SDD open questions: sdd/specs/analyzer/RH5103.md` (new)

Q1 — A statement behind a multi-line statement is not reported (P4). Input: `M(1,` / `  2); M(3);` in a block. Real output: not reported, because RH5103 compares the start lines of the two statements; the formatter puts `M(3);` on its own line (FMT-BREAK-006). Touches RH5103-001, FMT-BREAK-006.

Q2 — The fix is withheld behind an embedded or labeled statement. Input: `if (a) M(); M();` and `L: M(); M();` in a block. Real output: the second `M();` is reported, no code fix is offered (the token in front of it belongs to the nested statement, not to the reported statement's predecessor); the formatter splits both lines. Why it looks wrong: the gap holds only a space, so the fix could split it safely; the rule page names only a stray semicolon and the first statement of a line as cases without a fix. Touches RH5103-004.

### `SDD open questions: sdd/specs/analyzer/RH5104.md` (new)

Q1 — Decided by principle P6 (#912), violates nothing merged yet (P6 extends the #915 Q4 decision). Input: `void N() { } /** tail */` as the last member of a class. Real output: not reported in `DocumentationMode.Diagnose`/`Parse` (the comment is classified as documentation), reported with `DocumentationMode.None`. By P6 a `/** */` that documents nothing is an ordinary block comment, so RH5104 would report it in every mode. Touches RH5104-002.

Q2 — Reporting depends on the documentation mode. Input: `void M() { } /// tail`. Real output: not reported with documentation comments parsed, reported with `DocumentationMode.None` (RH5104 classifies by trivia kind). SHARED-COMMENT-001 classifies by text so that "the same source must not change its diagnostics" with the project setting. Touches RH5104-001, RH5104-002.

### `SDD open questions: sdd/specs/analyzer/RH5108.md` (new)

Q — Several parameter lists are not checked (P4). Input: `class P(` / `    int a)`, `delegate (` / `    int y) { return y; }`, `(` / `    int x) => x`. Real output: RH5108 reports none of them (it registers only methods, constructors, local functions, operators, conversion operators, delegates, records, and record structs); the formatter joins the first parameter in all three (FMT-BREAK-012). RH5105 and RH5106 do cover primary constructors, anonymous methods, and (RH5106) parenthesized lambdas. Touches RH5108-001.

### `SDD open questions: sdd/specs/analyzer/RH5110.md` (new)

Q — Record structs are not checked (P4). Input: `record struct S<T> where T : class;`. Real output: not reported (`RecordStructDeclaration` is not registered; `record` and `struct` are); the formatter writes `record struct S<T>` / `    where T : class;` (FMT-BREAK-023). Touches RH5110-001.

### `SDD open questions: sdd/specs/analyzer/RH5111.md` (new)

Q1 — Decided by principle P7 (#912), violates SHARED-FIX-001. Input: `enum E` / `{` / `    A` / `        = 1,` / `}`. Real output: reported; the fix only re-indents `= 1,` to the member column and the diagnostic stays; the formatter also keeps the line break (FMT-BREAK-016 lists assignments, declarators, and property initializers, not enum members), so formatted code is reported (P4 as well). Touches RH5111-001, RH5111-004, FMT-BREAK-016.

Q2 — An assignment used as a value is not checked (P4). Input: `var b = x =` / `    4;` and `var c = x` / `    = 5;`. Real output: not reported (an assignment whose parent is an `= value` clause is skipped); the formatter joins both to one line. Touches RH5111-001, RH5111-002.

Q3 — Decided by principle P1 (#912), violates SHARED-FIX-003 (#1068). Input: `/// <summary>P</summary>` / `int P { get; set; }` / `    = 5;`. Real output: the fix formats the whole property, including its documentation comment, which becomes `/// <summary>` / `/// P` / `/// </summary>` — the fix changes the text of a comment. Touches RH5111-004.

### `SDD open questions: sdd/specs/analyzer/RH5113.md` (new)

Q — Decided by principle P1 (#912), violates SHARED-FIX-003 (#1068). Input: `/// <summary>A</summary>` / `int _a` / `    ;`. Real output: the fix formats the whole field declaration and rewrites the documentation comment to three lines. Touches RH5113-004.

### `SDD open questions: sdd/specs/analyzer/RH5114.md` (new)

Q — Decided by principle P6 (#912). Input: `using System; /** first */ using System.Linq;`. Real output: reported, no code fix is offered, because the fix is withheld when a documentation comment leads the moved directive. By P6 a `/** */` that documents nothing (here, in front of a using directive) is an ordinary block comment, and the fix is offered for `using System.Text; /* text */ using System.IO;` (RH5114-003). Touches RH5114-003.

### `SDD open questions: sdd/specs/analyzer/RH5203.md` (new)

Q — The fix moves a comment above the argument to column 0. Input: `M(1,` / `            // two` / `            2);`. Real output: `2` is reported; the fix removes every whitespace trivia in front of the argument, so `// two` ends up at column 0 and `2);` is aligned. Why it looks wrong: the fix changes the comment's indentation and leaves it at the left margin, unrelated to the code; only the argument's own indentation is wrong. Touches RH5203-003.

### `SDD open questions: sdd/specs/analyzer/RH5204.md` (new)

Q1 — Lines that start with a type or an identifier are never checked (P4). Input (inside `class C`): `      int _a;` (no modifier), and in a method body `          M(1);`, `          var y = 1;`, and a `///` comment in front of `int P { get; set; }`. Real output: none reported (a line is checked only when its first token is directly owned by the declaration, statement, accessor, clause, or label — a keyword or modifier — or is a scope brace or an enum member name); `      private int _b;` and `          return;` in the same positions are reported, and the formatter re-indents all of them (FMT-INDENT-002). Touches RH5204-001, RH5204-002.

Q2 — The diagnostic on a misindented `///` comment is not on the comment. Input: `      /// <summary>M</summary>` in front of `    public void M()`. Real output: reported, located on ` <summary>M</summary>` plus the line break (the structured trivia's span, after the `///` exterior). Touches RH5204-005.

### `SDD open questions: sdd/specs/shared/code-fixes.md` (new; #1066 is closed)

Q — Formatting fixes format a node much larger than the diagnostic, applying unrelated formatter rules. FMT-PIPE-010's reason is that "code fixes … may only change the code their diagnostic is about", but these fixes pick a large target:
- RH5110 formats the whole owning declaration: for `class C<T> where T : class` it reformats every member (e.g. `int  x  =  1;` → `int x = 1;`, other methods' constraint clauses), and `interface I<T> where T : class` / `{` / `}` becomes `interface I<T>` / `    where T : class;` (FMT-STRUCT-017).
- RH5204 formats the smallest node that starts on an earlier line: for a misindented `return` in a method body, the whole body is formatted, adding braces to `if`/`else` (FMT-STRUCT-001) and blank lines (FMT-BLANK-001/002); for a misindented `else`, the `if` statement is formatted at its stale column and the RH5204 count of the document rises from 4 to 8.
- RH5206 formats the enclosing statement: for `return status switch {` … `};` it inserts a blank line before `return` (FMT-BLANK-001).
Decision needed: may a code fix that delegates to the formatter change code its diagnostic is not about? Touches RH5110-003, RH5204-006, RH5206-003, FMT-PIPE-010. (The documentation-comment cases of RH5111 and RH5113 are P1 instances, filed with those specs.)

## Instances of decided follow-ups (one-line comments for the lead)

- #956 (argument lists laid out like parameter lists): RH5102 reports `M(1, 2` / `    );` and its fix only re-indents `)`, so the diagnostic stays; RH5102 reports `M(1` / `  , 2, 3);` and its fix changes nothing (no progress). Both converge once the formatter moves the comma and the closing parenthesis.
- #953 (continuation lines one level deeper): `x +=` / `    3;` — the formatter keeps the line break and puts `3;` at the statement's column.

## Follow-up candidates (out of scope)

- None beyond the open questions.
