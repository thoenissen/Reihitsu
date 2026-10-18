Role: spec author
Commit: fee39fc (skeptic handover on top of 8f4e3ed; analyzer and code-fix sources unchanged since 72b08b0)
Verdict: spec written

Repair round after `02-skeptic.md`. I re-derived each of its points from the code and ran every example again. The skeptic's suggested wordings were checked, not copied. No change doc: no intended behavior changes.

## How the statements were established

- Every C# block of the 21 analyzer specs, `shared/fluent-chain-layout.md`, `shared/whitespace-removal-fixes.md`, and the new `shared/line-join-rules.md` was run again through the scratch harness described in `01-spec-author.md`.
- The harness checks the marked spans, before/after fixes (applied until the rule reports nothing), and every "no code fix is offered" or "the fix is offered" claim.
- Result: 0 mismatches. No example has a compile error any more except the intended `int _x = ;` in SHARED-JOIN-002.
- I confirmed these skeptic findings myself:
  - A1: `return M /* note */ (` / `1,` / `2);` is fixed by RH5101.
  - A2: `Get(/* c */ x)` / `.Where(…)` is not reported by RH5048 and is reported by RH5112. `a // c` / `?[0]` / `.ToString()` behaves the same way.
  - A3a: `M($"""` / `{x}` / `""" /* c */);` is not reported.
  - A3b: `/* a */ /* b */` reports both comments, and the fix for `/* b */` reorders them.
  - A4: `/* note */` on the line above gets a blank line. Below `case 1:` no blank line is added.
  - A5: a parameter or an enum member with a comment in front of it is fixed.
  - D1: the fix inserts `/* c */` inside `@"a` / `b"`.
  - D5: `/* c */ where T : class` gets a blank line and stays reported.
- `SpecConsistencyTests` and `SpecStructureTests` pass. `RequirementCoverageTests` fails only on uncovered requirements: the 74 rule IDs plus SHARED-CHAIN-001 to 009, SHARED-JOIN-001/002, SHARED-USING-010, and SHARED-WS-003.

## Changed requirements

IDs are new in this run. Several were dropped and renumbered, and no test tags them yet. The ledger was rebuilt as the base ledger at 72b08b0 plus the current headings, so it only adds lines.

**A — requirements that were false for some inputs**

- **RH5101-004, RH5102-004, RH5109-004 — changed (A1).** The fix is withheld only for a comment or directive inside the list, or on a line of its own in front of `(`. A comment on the line of the token in front of `(` does not withhold it. The example now shows that case.
- **RH5048-001 and RH5112-001 — changed (A2).** What counts is the gap directly in front of the first link's operator, behind the root and the attached parts that belong to it. A comment inside an attached part of the root is neither reported nor blocking; the examples show both sides.
  - SHARED-CHAIN-004 is merged and left unchanged. "Root" there includes its attached parts (SHARED-CHAIN-001), so it does not contradict the rule specs.
- **RH5104-001 — changed (A3).** It reports a comment that has a token on one of its lines, and only outside the tokens of a multi-line interpolated string. These cases are left out:
  - a comment next to only another comment;
  - a comment in the trivia of a multi-line interpolated string's start or end token.
- **RH5104-002 — changed (A3, C4).** It now covers a comment on its own line, a directive comment (now a reference to SHARED-COMMENT-002), and a comment inside a multi-line interpolation. The documentation-comment clause and the `void M() { } /// tail` example are removed (B4).
- **RH5104-004 — changed (D1).** It applies only when the comment's line does not start inside a literal or a comment.
- **RH5104-005 — changed (A4).** No blank line is inserted below a switch label. The `/* */`-only line above is left out (question D10).
- **RH5111-001 — changed (A6, B3).**
  - It covers a simple assignment that forms an expression statement or an object-initializer member, so it neither claims nor denies the Q2 input.
  - An enum member whose `=` sits below its target is no longer claimed. RH5111-002 still reports an enum member whose value is below the `=`; that case is verified and its fix converges.
- **RH5111-004 — changed (D8).** It applies only when the construct is not inside the value of an assignment.
- **RH5111-005 — changed (A5).** Full trivia is checked only for a variable declarator. A comment in front of a parameter or an enum member does not withhold the fix; this is now shown as the "fix is offered" example.
- **RH5110-001 — changed (A7, D5).** It says "record class", and the column check applies only to a `where` with only whitespace in front of it on its line.
- **RH5203-001 and RH5203-003 — changed (A8, D6).** The rule and its fix are stated only for an argument with nothing but whitespace in front of it on its line.
- **SHARED-CHAIN-009 — changed (A9).** It now names the real context node (the innermost statement or member declaration that contains the chain and starts a line) and the real trigger (the chain's text or start column differs). It is also listed in the shared fix-scope question.

**B — open-question inputs that were still in a spec**

- **RH5101 and RH5102 non-goals — changed (B1).** "Attribute argument lists" removed.
- **RH5108-001 — changed (B2).** The "MUST NOT" sentence and the class primary constructor and lambda examples are removed. Indexers moved to the non-goals.
- **RH5103-003 — changed (D4).** It states the indentation only when the preceding statement starts its line, or shares a `case` label's line. B5 is unchanged: RH5103-001 gives a sufficient condition and does not deny Q1.

**C — shared behavior restated instead of referenced**

- **SHARED-JOIN-001 — added (C3)** in the new `sdd/specs/shared/line-join-rules.md`: a gap holding a comment, a directive, or disabled text is not reported. It replaces the former RH5101-002, RH5105-002, RH5106-002, RH5107-002, RH5108-002, and RH5113-002, which are now referenced in each `-001`.
- **SHARED-JOIN-002 — added (C2):** a file with a syntax error is not analyzed. It replaces the former RH5105-003 and RH5106-003. Code owner: `SyntaxTreeErrorVerdictCache`.
- **RH5048-002 — dropped (C1).** RH5048-001 now references SHARED-COMMENT-003.
- **RH5112-002 — dropped.** It restated SHARED-CHAIN-004, which is now referenced in RH5112-001.
- **SHARED-WS-003 — clarified.** Its Why references SHARED-JOIN-001, and `Uses` lists it.

**Renumbering**

| Spec | Old ID | New ID |
|---|---|---|
| RH5101 | 003, 004, 005 | 002, 003, 004 |
| RH5105 | 004, 005 | 002, 003 |
| RH5106 | 004, 005 | 002, 003 |
| RH5107 | 003, 004 | 002, 003 |
| RH5108 | 003, 004 | 002, 003 |
| RH5112 | 003, 004 | 002, 003 |
| RH5113 | 003, 004 | 002, 003 |

**Other edits**

- **RH5206-003 — changed (#956 swap).** The example now uses arms that each start their own line.
- **RH5204-001 — clarified.** It now also covers the label of a labeled statement, with an example.
- **Why sentences softened.** RH5101-003, RH5102-003, RH5110-003, RH5111-004, RH5113-003, RH5205-003, and RH5206-003 no longer claim "identical to formatted code" (D2, D11).
- **`documentation/rules/RH5101.md`.** The page now says the fix lays out the argument list.
- **`sdd/specs/README.md`.** It has a row for the new shared spec.

Final requirement IDs: RH5048-001; RH5101-001…004; RH5102-001…004; RH5103-001…004; RH5104-001…005; RH5105-001…003; RH5106-001…003; RH5107-001…003; RH5108-001…003; RH5109-001…004; RH5110-001…003; RH5111-001…005; RH5112-001…003; RH5113-001…003; RH5114-001…003; RH5201-001…005; RH5202-001…003; RH5203-001…003; RH5204-001…006; RH5205-001…003; RH5206-001…003; SHARED-CHAIN-008, SHARED-CHAIN-009; SHARED-JOIN-001, SHARED-JOIN-002; SHARED-WS-003.

## Examples for the tests

- Every requirement ID listed above needs a tagged test, together with these shared IDs that the new specs use and no test tags yet: SHARED-CHAIN-001 to SHARED-CHAIN-009, SHARED-JOIN-001, SHARED-JOIN-002, SHARED-USING-010, and SHARED-WS-003.
- SHARED-JOIN-001 should be tagged from the join tests of RH5101, RH5105–RH5108, and RH5113. SHARED-JOIN-002 should be tagged from RH5105 and RH5106 tests that use a source with a syntax error (CS1525).
- RH5204-004 uses `→` for a tab.
- The skeptic's "Breaking inputs" list applies. Do not pin the inputs of the questions below.

## Open questions to file

None of these blocks the run, and each case is left out of the specs. No issue exists yet for any of the target specs: #918 and #1031 (`formatter/line-breaks.md`) and #1066 (`shared/code-fixes.md`) are closed. Each target therefore needs a new `SDD open questions: <spec path>` issue, as a sub-issue of #912.

### `SDD open questions: sdd/specs/formatter/line-breaks.md`

**Q1 — A list wrapped only after its opening parenthesis is joined onto one line.**
- Input: `M(` / `    1, 2);` in a method body.
- Real output (`scripts/trace.sh`, LineBreakPhase): `M(1, 2);`.
- Why it looks wrong: FMT-BREAK-012 says a list wrapped anywhere, "after its opening parenthesis or between two elements", becomes one element per line (`M(1,` / `  2);`). The RH5101 and RH5102 fixes produce the same `M(1, 2)`.
- Touches: FMT-BREAK-012, RH5101-003, RH5102-003.

**Q2 — The formatter breaks the line after a lambda's attribute list** (skeptic D11).
- Input: `Func<int, int> c = [Obsolete] (x) =>` / `{ return x; };`.
- Real output: the formatter puts a line break behind `[Obsolete]`.
- Why it looks wrong: FMT-BREAK-027 is about the attribute lists of declarations, and a lambda is not a declaration.
- Touches: FMT-BREAK-027.

### `SDD open questions: sdd/specs/formatter/pipeline.md`

**Q1 — Node-level formatting gives other columns than document formatting** (skeptic D2, D11; FC-028).
- Inputs and real outputs:
  - RH5101 fix: `M(` / `"""` / `a` / `""",` / `2);` becomes `M("""` with `a` and the closing `"""` at column 9, while the opening quotes are at column 10. Formatting the whole document gives column 10, and RH5202 reports the fix's result.
  - RH5102 fix: `M(1, """` / `a` / `""");` has the same off-by-one.
  - RH5205 fix: for `Func<int, int> c = [Obsolete] (x) =>` / `{…}`, the fix puts `(x) =>` and `{` at column 27, while document formatting puts them at column 8.
- Why it looks wrong: FMT-PIPE-010 is meant to change only the target, not to give it a different layout. The fixes therefore produce code that formatting would change again. This is not covered by #951.
- Touches: FMT-PIPE-010, RH5101-003, RH5102-003, RH5205-003.

### `SDD open questions: sdd/specs/shared/code-fixes.md`

**Q1 — Formatting fixes change code their diagnostic is not about** (FC-030; skeptic D9).
- FMT-PIPE-010's reason is that code fixes "may only change the code their diagnostic is about". These fixes, however, format a node much larger than the diagnostic:
  - **RH5110** formats the whole owning declaration:
    - for `class C<T> where T : class` it reformats every member, for example `int  x  =  1;` → `int x = 1;`;
    - `interface I<T> where T : class` / `{` / `}` becomes `… where T : class;` (FMT-STRUCT-017).
  - **RH5204** formats the smallest node that starts on an earlier line:
    - a misindented `return` gets the whole body formatted, with braces (FMT-STRUCT-001) and blank lines (FMT-BLANK-001/002);
    - for a misindented `else`, the `if` is formatted at its stale column and the document's RH5204 count rises from 4 to 8 (also FC-007).
  - **RH5206** formats the enclosing statement and inserts a blank line before `return status switch {…};`.
  - **RH5112/RH5201 (SHARED-CHAIN-009 fallback):** `if (x==1 && a` / `.B())` / `{` / `int  y = 1;` / `}` also formats the body (`int y = 1;`).
  - **RH5101/RH5102** also format inside arguments: `M(` / `1+2,` / `3)` becomes `M(1 + 2,` / `3)`.
  - **RH5111 (skeptic D8):** for a declarator inside a lambda that is the value of an assignment (`a = () => { var x` / `= 1; … };`), the fix formats the whole outer assignment, including another statement `int  y = x;`. A `// other` comment anywhere in the lambda withholds the fix.
- Decision needed: may a code fix that delegates to the formatter change code its diagnostic is not about?
- Touches: RH5101-003, RH5102-003, RH5110-003, RH5111-004, RH5111-005, RH5113-003, RH5204-006, RH5206-003, SHARED-CHAIN-009, FMT-PIPE-010.
- The documentation-comment cases of RH5111 and RH5113 are P1 instances and are filed with those specs.

### `SDD open questions: sdd/specs/analyzer/RH5101.md`

**Q1 — Attribute argument lists are not checked (P4).**
- Input: `[A(` / `    1)]` on a method, where `A` is an attribute with a `params int[]` constructor.
- Real output: RH5101 reports nothing, while the formatter writes `[A(1)]`.
- Why it looks wrong: FMT-BREAK-012 covers attribute argument lists and says "RH5101, RH5102, and RH5109 report the same shapes".
- Touches: RH5101-001, FMT-BREAK-012.

### `SDD open questions: sdd/specs/analyzer/RH5102.md`

**Q1 — Attribute argument lists are not checked (P4).**
- Input: `[A(1, 2,` / `   3)]`.
- Real output: RH5102 reports nothing, while the formatter writes one argument per line.
- Why it looks wrong: same as RH5101 Q1.
- Touches: RH5102-001, FMT-BREAK-012.

### `SDD open questions: sdd/specs/analyzer/RH5103.md`

**Q1 — A statement behind a multi-line statement is not reported (P4).**
- Input: `M(1,` / `  2); M(3);` in a block.
- Real output: not reported, because start lines are compared. The formatter splits the line (FMT-BREAK-006).
- Touches: RH5103-001, FMT-BREAK-006.

**Q2 — The fix is withheld behind an embedded or labeled statement.**
- Input: `if (a) M(); M();` and `L: M(); M();`.
- Real output: the second `M();` is reported and no code fix is offered, because the token in front of it belongs to the nested statement. The formatter splits both lines.
- Why it looks wrong: the gap holds only a space. The rule page names only a stray semicolon and the first statement of a line as cases without a fix.
- Touches: RH5103-004.

**Q3 — Top-level statements are not checked (P4)** (skeptic D3).
- Input: `Console.WriteLine(1); Console.WriteLine(2);` at file level.
- Real output: not reported, because only blocks and switch sections are analyzed. `format.sh` splits the line.
- Touches: RH5103-001, FMT-BREAK-006.

**Q4 — The fix in a block written on one line** (skeptic D4).
- Input: `void N() { M(); M(); }` and `Action a = () => { M(); M(); };`.
- Real output: `void N() { M();` / `M(); }`, with the moved `M();` at the method's column, because the preceding statement's line is indented like the method header.
- Why it looks wrong: the moved statement belongs to the block, one level deeper. RH5103-003's own Why says a line's indentation that belongs to something else must not be copied.
- Touches: RH5103-003.

### `SDD open questions: sdd/specs/analyzer/RH5104.md`

**Q1 — Decided by principle P1 (#912); violates SHARED-FIX-003 (#1068). The fix inserts the comment inside a literal or a comment** (skeptic D1).
- Inputs and real outputs:
  - `var s = @"a` / `b" + /* c */ x;` becomes `@"a` / (blank line) / `/* c */` / `b" + x;`. **The string's value changes.**
  - `var s = """` / `a` / `""" + /* c */ x;`, `M("""` / `a` / `""" /* c */);`, and `$"""` / `{x}` / `""" + /* c */ x;` insert a blank line and the comment into the raw string's content.
  - `/** <summary>` / `M</summary> */ void M() { } // c` inserts `// c` into the documentation comment.
- Touches: RH5104-004.

**Q2 — Decided by principle P6 (#912).**
- Input: `void N() { } /** tail */` as the last member of a class.
- Real output: not reported when documentation comments are parsed; reported with `DocumentationMode.None`.
- Why it looks wrong: under P6, a `/** */` that documents nothing is an ordinary block comment and would be reported in every mode.
- Touches: RH5104-001, RH5104-002.

**Q3 — Reporting depends on the documentation mode.**
- Input: `void M() { } /// tail`.
- Real output: not reported when documentation comments are parsed; reported with `DocumentationMode.None`. RH5104 classifies by trivia kind, and under the default mode the lexer moves a trailing `///` into the next token's leading trivia.
- Why it looks wrong: SHARED-COMMENT-001 classifies by text so that "the same source must not change its diagnostics" with the project setting.
- Touches: RH5104-001, RH5104-002.

**Q4 — Two comments on one line are reported and reordered** (skeptic A3b).
- Input: `/* a */ /* b */` on a line of its own above `M();`.
- Real output: both comments are reported. The fix for `/* a */` gives (blank line) / `/* a */` / `/* b */`. The fix for `/* b */` gives (blank line) / `/* b */` / `/* a */`, which reorders the comments.
- Why it looks wrong: the line holds no code. Applying the fix in the order of the diagnostics reorders user-authored comments.
- Touches: RH5104-001, RH5104-004.

**Q5 — A comment behind a multi-line interpolated string is not reported** (skeptic A3a).
- Input: `M($"""` / `{x}` / `""" /* c */);`.
- Real output: not reported. Every comment whose token has a multi-line interpolated string as an ancestor is exempt, also outside the interpolations.
- Why it looks wrong: the comment shares the line with `);` and is not in the string.
- Touches: RH5104-001, RH5104-002.

**Q6 — A `/* */` line above is treated as code** (skeptic D10).
- Input: `M();` / `/* note */` / `M(); // c`.
- Real output: `M();` / `/* note */` / (blank line) / `// c` / `M();`.
- Why it looks wrong: the inserted blank line separates `/* note */` from the code it stood above. A line starting with `//` gets no blank line.
- Touches: RH5104-005.

### `SDD open questions: sdd/specs/analyzer/RH5108.md`

**Q1 — Several parameter lists are not checked (P4).**
- Input: `class P(` / `    int a)`, `delegate (` / `    int y) { return y; }`, `(` / `    int x) => x`, and `extension(` / `    int value)`.
- Real output: RH5108 reports none of them, while the formatter joins the first parameter in all four (FMT-BREAK-012).
- Why it looks wrong: RH5105 and RH5106 cover primary constructors, anonymous methods, and extension blocks, and RH5106 also covers parenthesized lambdas.
- Touches: RH5108-001.

### `SDD open questions: sdd/specs/analyzer/RH5110.md`

**Q1 — Record structs are not checked (P4).**
- Input: `record struct S<T> where T : class;`.
- Real output: not reported, while the formatter writes `record struct S<T>` / `    where T : class;`.
- Touches: RH5110-001.

**Q2 — Decided by principle P7 (#912); violates SHARED-FIX-001 and SHARED-FIX-005 (#1068). A block comment in front of `where`** (skeptic D5; also P4).
- Input: `void N<T>()` / `    /* c */ where T : class`.
- Real output: reported. The fix inserts a blank line between the header and the comment and makes no further progress, so the diagnostic stays. `format.sh` gives the same output, so RH5110 reports formatted code.
- Touches: RH5110-001, RH5110-003, FMT-BREAK-023.

### `SDD open questions: sdd/specs/analyzer/RH5111.md`

**Q1 — Decided by principle P7 (#912); violates SHARED-FIX-001. An enum member whose `=` is below its target** (also P4).
- Input: `enum E` / `{` / `    A` / `        = 1,` / `}`.
- Real output: reported. The fix only re-indents `= 1,` and the diagnostic stays. The formatter keeps the line break too (FMT-BREAK-016 does not list enum members), so formatted code is reported.
- Touches: RH5111-001, RH5111-004, FMT-BREAK-016.

**Q2 — An assignment used as a value is not checked (P4).**
- Input: `var b = x =` / `    4;` and `var c = x` / `    = 5;`.
- Real output: not reported, while the formatter joins both.
- Touches: RH5111-001, RH5111-002.

**Q3 — Decided by principle P1 (#912); violates SHARED-FIX-003 (#1068).**
- Input: `/// <summary>P</summary>` / `int P { get; set; }` / `    = 5;`.
- Real output: the fix rewrites the documentation comment to three lines.
- Touches: RH5111-004.

### `SDD open questions: sdd/specs/analyzer/RH5113.md`

**Q1 — Decided by principle P1 (#912); violates SHARED-FIX-003 (#1068).**
- Input: `/// <summary>A</summary>` / `int _a` / `    ;`.
- Real output: the fix rewrites the documentation comment to three lines.
- Touches: RH5113-003.

### `SDD open questions: sdd/specs/analyzer/RH5114.md`

**Q1 — Decided by principle P6 (#912).**
- Input: `using System; /** first */ using System.Linq;`.
- Real output: reported, and no code fix is offered.
- Why it looks wrong: under P6, this `/** */` is an ordinary block comment, and the fix is offered for `/* text */` in the same position.
- Touches: RH5114-003.

### `SDD open questions: sdd/specs/analyzer/RH5203.md`

**Q1 — The fix moves the comment in front of the argument to column 0** (includes skeptic D6; that part is P7 and P4).
- Inputs and real outputs:
  - `M(1,` / `            // two` / `            2);`: the fix removes every whitespace trivia in front of the argument, so `// two` lands at column 0.
  - `M(1,` / `            /* two */ 2);`: reported. The fix writes `/* two */` at column 0, followed by 10 spaces and `2`, and then makes no progress.
  - `format.sh` on the second input gives `M(1,` / (blank line) / `/* two */ 2);` at column 10. RH5203 still reports that, and a blank line now sits inside an argument list.
- Touches: RH5203-001, RH5203-003.

### `SDD open questions: sdd/specs/analyzer/RH5204.md`

**Q1 — Lines that start with a type, an identifier, or an attribute list are never checked (P4).**
- Input (in `class C`): `      int _a;` (no modifier), `[Obsolete]` on its own line at a wrong column, and in a method body `          M(1);` and `          var y = 1;`, plus a `///` comment in front of `int P { get; set; }`.
- Real output: none of these is reported, while `      private int _b;` and `          return;` are. The formatter re-indents all of them.
- Touches: RH5204-001, RH5204-002.

**Q2 — The diagnostic on a `///` comment is placed after the `///`.**
- Input: `      /// <summary>M</summary>` in front of `    public void M()`.
- Real output: the diagnostic is located on ` <summary>M</summary>` plus the line break.
- Touches: RH5204-005.

### `SDD open questions: sdd/specs/analyzer/RH5206.md`

**Q1 — Decided by principle P7 (#912); violates SHARED-FIX-005 (#1068). A block comment in front of an arm** (skeptic D7; also P4).
- Input: `status switch` / `{` / `/* idle */ 0 => "Idle",` / `_ => "Other"` / `}`, with every line correctly aligned.
- Real output: reported. The fix changes nothing, and the formatter leaves the code as it is.
- Touches: RH5206-001, RH5206-003.

## Follow-up comments

- **#956:** RH5102 reports `M(1, 2` / `    );`, and its fix only re-indents `)`, so the diagnostic stays. It also reports `M(1` / `  , 2, 3);`, and its fix changes nothing. RH5206-003 and its tests use arms on separate lines; they follow the one-arm-per-line change when it lands.
- **#953:** `x +=` / `    3;` — the formatter keeps the line break and puts `3;` at the statement's column.
- **#1097 (premise to recheck, from the skeptic):**
  - RH5048 reports the comment in front of the first link (SHARED-CHAIN-004), not only the comment in front of the first call.
  - So `M(items` / `// c` / `.Where(…))` is reported by both RH5048 and RH5038.
  - After #1097, `M(a.P` / `// c` / `.B())` would be reported by neither rule.
