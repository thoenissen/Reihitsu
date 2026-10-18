Role: skeptic
Commit: a25a7b68468a3833b95f117e2faa30e44c59d1a2
Verdict: proceed narrower

My prompt carried no intended fix, suspected cause, or hoped-for verdict. Every statement below was settled by running `scripts/apply-fix.sh <RH####> <dir>` (LF and CRLF, same result in every arm unless noted) or `scripts/trace.sh <file> --passes 2` on fixtures in the scratchpad, outside the repository. "Iterations" is the runner's count: `fixed (1 action(s), 2 iteration(s))` on an input with one diagnostic means the rule reported again after the first application.

## Premise

The lane is real: the 12 rules have no spec, and none of this needs production code. Most of the spec diff matches the code: reporting conditions, locations, and fixes of RH5022–RH5028 and RH5031/RH5032 come straight from `FormattingTextAnalysisUtilities` (line text plus token start or end) and `RegionDirectiveBlankLineUtilities`. RH5011 comes from `StatementShouldBeFollowedByABlankLineAnalyzerBase` (`ContainsBlankLine`, `IsFollowedByDirective`), RH5030 from `BlankLineSpacingPolicy.RequiresBlankLineAfterClosingBrace` plus `BlankLineCodeFixUtilities`, and RH5020 from its text-line test plus `IsPrecededByCommentOrDirective` and `IsFirstCommentInBlock`. Defining SHARED-REGION as a shared spec is justified, because RH5031 and RH5032 both rely on SHARED-REGION-001 and -002. Adding `BlankLineSpanRemovalCodeFixProviderBase` to SHARED-WS is correct, because the class only derives from `WhitespaceSpanRemovalCodeFixProviderBase`.

The spec does not yet match the code in eight places. Some requirements state behavior the code does not have. Others are worded so that they cover a case that looks wrong as if it were intended. These go back to the spec author before tests are derived:

1. **RH5011-004 covers a fix that edits a comment and never converges.** When a multi-line block comment starts on the `break` line, "the line of the `break` statement" ends inside the comment. The fix inserts an empty line there, inside the comment's text. The diagnostic stays because the empty line is comment text (SHARED-BLANK-001), so every application adds one more. Both of these inputs ended `not-converged` after the cap of 4, with 4 empty lines inserted between `/* a` and `b */`:

   ```csharp
   class C
   {
       void M(int a)
       {
           while (true)
           {
               break; /* a
               b */
               a++;
           }
       }
   }
   ```

   The second input is the same with `b */ a++;` on one line. The formatter inserts the blank line after `b */` instead (trace, `BlankLinePhase`). The wording must leave this shape out, and it must be filed. It conflicts with SHARED-FIX-001 and with AGENTS.md (a fix must not alter comments), so it is a bug, not a question of intent.

2. **RH5020-003's qualifier "another comment that starts its own line" does not match the code.** The code exempts a comment when the trivia in front of it *in the same token's leading trivia* is a comment of any kind or a directive.
   - `a++;`, blank line, `/* a`, `b */ // c`, `// d`, `a++;` → `// d` is **not reported**. The spec reads it as reported, because `// c` does not start its line. The formatter also leaves it alone (`no-diagnostic`, trace shows no BlankLine change), which matches FMT-BLANK-011's "first comment that starts its own line … in a token's leading trivia".
   - `a++; /* a`, `b */ // c`, `// d`, `a++;` → `// d` **is reported**, because the chain behind `a++;` belongs to the `;` token's trailing trivia.

   The requirement needs rewording, not an open question. The two inputs above are the boundary pair for tests.

3. **RH5024-001 covers a case that SHARED-BLANK-006 contradicts.** Input: `class C`, `{`, `int _a;`, blank, `#region R`, `}`, `#endregion`.
   - RH5032 reports `#region R` and inserts a blank line above `}`. SHARED-BLANK-006 says the closing-brace exemption is for `#endregion` only, and the existing test `VerifyRegionBeforeClosingBraceIsDetectedAndFixed` pins that output.
   - RH5024 then reports that blank line and deletes it, and RH5032 reports again. No text satisfies both rules.
   - The formatter keeps the blank line (trace, `BlankLinePhase`), so formatter output is flagged by RH5024 (FC-013).

   RH5024-001's "whatever follows" wording has to stop covering a blank line directly below a `#region`. File it as a question. It touches SHARED-BLANK-006, which must not change in this run.

4. **RH5025-001's stated set is narrower than behavior that looks intended.** RH5025 reports the line above every opening-brace token that begins its line.
   - Outside "type, member, statement, initializer" but reported, with the formatter agreeing (FMT-BLANK-008): `namespace N` / blank / `{`, a lambda body (`() =>` / blank / `{`), and a switch-expression body (`a switch` / blank / `{`).
   - Also reported: a property pattern on a continuation line (`s is` / blank / `{ Length: 0 }`) and an anonymous object (`new` / blank / `{`).

   The `Why:` ("the brace belongs to the code above it") covers all of these. They should be written into RH5025-001 as clarifications rather than left unspecified. Only the shapes in Q1 and Q2 stay out.

5. **RH5030-003 covers a fix that needs two passes and leaves broken layout.** Input: `if (a > 0)`, `{`, `} /* a`, `b */ a++;`. It ends `fixed (1 action(s), 2 iteration(s))` with result `b */ ` (trailing space), an empty line, then `a++;` at column 0. The comment opens on the brace line and closes on the statement's line, so the brace's trailing trivia holds no line break. The first insertion "at the start of the next statement's leading trivia" only breaks the line. The formatter reaches the right final text (BlankLine, Indentation and Cleanup phases). The wording must leave this shape out, and it must be filed. It conflicts with SHARED-FIX-001, so it is a bug. It is not a duplicate of thoenissen/Reihitsu#972, which is about a different base class and the opposite effect.

6. **The RH5023 non-goal is factually wrong, and the "run" `Why:`s are imprecise.** RH5023's non-goal says RH5022 and RH5024–RH5027 "report the first blank line of a run there, while this rule reports the rest". Only RH5022 does that.
   - RH5024–RH5027 report the line directly above the token, which is the *last* line of a run. RH5023 reports that same line, and nobody reports the first line until the second pass.
   - Measured on a run of two blank lines, each of RH5022, RH5024, RH5025, RH5026 and RH5027 alone ends `fixed (1 action(s), 2 iteration(s))`.
   - RH5023 alone on the run above `}` deletes the second line and leaves the first for RH5024.

   The non-goal and the "a run of blank lines is RH5023's concern" `Why:`s of RH5024-004, RH5025-003, RH5026-003 and RH5027-003 have to state what happens. Whether one pass is required here is Question B below.

7. **The RH5022 and RH5024 non-goals contradict their own requirements.** In an empty body (`void M()`, `{`, blank, `}`), RH5022 and RH5024 both report the same line, and each fix alone clears it in one pass. RH5022's non-goal "A blank line directly before a closing brace — RH5024" and RH5024's mirror non-goal say the opposite. They need rewording.

8. **RH5027-001's "whether … or …" leaves three shapes unspecified.** It reads as the complete list ("`while` begins its line or follows the body's closing brace"), which leaves three shapes outside the requirement. RH5027 reports all of them, and the formatter keeps the line in all three:
   - `do` / blank / `a++; while (a < 3);` reports the blank line between `do` and its body.
   - `do` / `a += @"x` / blank / `".Length; while (a < 3);` reports a blank line of the verbatim string. **The fix changes the literal's value.**
   - `} /* x` / blank / `*/ while (a < 3);` reports a blank line of the comment's text, and the fix deletes it.

   If the narrow reading is intended, say so explicitly (only whitespace or the body's `}` before `while` on its line). Either way these shapes must go into Q3 (see below). RH5027-004's `Why:` ("can never touch a comment, a directive, or a token") is false for the last two.

### Restating instead of referencing

The maintainer asked for FMT-BLANK and shared IDs to be referenced where they own the policy. The spec author restated the policy in every analyzer spec and did not say why. I checked each pair, and restating is the accurate choice, because the formatter's policy differs in detail:

| Rule | Formatter policy | Where it differs |
|---|---|---|
| RH5011 | FMT-BLANK-004 | "in the same list" excludes `else` and do-`while` |
| RH5020 | FMT-BLANK-011 | covers documentation comments; "exactly one" |
| RH5022, RH5024, RH5028 | FMT-BLANK-007 | removes the whole run; differs at a trailing comment (Q6) |
| RH5025–RH5027 | FMT-BLANK-008 | differs for one-line `do` (Q3) and the three RH5027 shapes above |
| RH5023 | FMT-BLANK-014/-015 | excludes whole interpolated strings, holes included |

The spec author should record this in a handover note so the spec checker does not read it as a breach of the clarification. No SHARED-BLANK, FMT-BLANK, or SHARED-STMT requirement has to change for the corrections above.

### The spec author's open-question candidates

| Q | Reproduces | Real question about intent? | Duplicate? |
|---|---|---|---|
| Q1 RH5025 deletes a line of an interpolated raw string | Yes — `$"""` and `$@"` both, the blank line inside the string is removed | Yes. AGENTS.md (prefer no fix over one that changes a literal) points to "bug". Sibling: the RH5027 string and comment shapes (item 8) share the effect; file them with Q3, or name them in Q1 | No |
| Q2 RH5025 and a standalone block | Yes. **Wider than filed:** also reported, and also removed by the formatter: an element of an array or dictionary initializer (`{ 1 },` / blank / `{ 2 },`; `{ "a", 1 },` / blank / `{ "b", 2 },`) and a property pattern that starts a switch-expression arm (`{ Length: 0 } => 0,` / blank / `{ Length: 1 } => 1,`). A `#region A` / blank / `{ "a", 1 },` inside a dictionary initializer is reported by RH5025, while RH5032 requires that line and the formatter keeps it (FC-013). Widen Q2 to "an opening brace that starts a sibling (block statement, list element, switch arm), not a continuation of the code above", with these shapes and the region conflict | Yes | No |
| Q3 RH5027 vs RH5004 on a one-line `do` | Yes. RH5027 deletes the line, RH5004 inserts it again, the formatter keeps it. RH5004-001 is enforced and requires the line, so the "intended" reading would need a change to RH5004 (change doc). **Widen** with item 8's three shapes. They share one mechanism: RH5027 checks the line above the *line holding* `while`, not the gap directly before `while`, so anything but the body's `}` in front of `while` on its line moves the checked line out of the footer gap | Yes, bug-leaning | No |
| Q4 RH5011 vs RH5026 on `else` | Yes, both the two-line and the one-line form. RH5026 reports the inserted line in both. **Widen** to `do break; while (a < 3);`: RH5011 reports, the fix gives `do break;`, a blank line, then `while (a < 3);`, and RH5027 reports that line. The formatter leaves `do break; while (a < 3);` unchanged, so formatter output is flagged by RH5011 (FC-013). For `else` the formatter adds braces first, so its output is clean there. The mechanism: RH5011 reports any next token but `}`, including a token that continues the statement enclosing the `break` | Yes | No |
| Q5 RH5023 reports a zero-length span at the end of `"class C\n{\n}\n\n"` | Yes — `no-progress` under LF and CRLF; with `\n\n\n` it is `no-progress` after one real fix | **No.** The fix is offered and can never clear the diagnostic, which SHARED-FIX-001 already forbids. This is a bug against an existing enforced spec. Still leave it out of RH5023's spec, and file it as a bug, or in the question issue with "intended" marked as unavailable without a SHARED-FIX change | No |
| Q6 RH5022 and `{ // note` | Yes — `no-diagnostic`; the formatter removes the line (`BlankLinePhase`) | Yes. FMT-BLANK-007's "mirrors RH5022" against SHARED-BLANK-005's "a trailing comment makes it a content line" are two readings | No |

None of the six overlaps thoenissen/Reihitsu#949, #954, #1000 or #1001 (all read), and none has an `SDD open questions` issue under thoenissen/Reihitsu#912 yet (sub-issue list read). I also checked the decided thoenissen/Reihitsu#916 Q1: RH5020 gets no exception inside a using group. RH5020 reports `// I/O helpers` between `using System;` and `using System.IO;`, which agrees with that decision.

## Mechanism and candidates

_N/A — no bug fix in this lane._ The defects found have a mechanism sentence each above. These candidate sets come from the dispatch code, for the spec author and the test implementer:

- **RH5025** (`GetLineIndicesBeginningWithToken` over every `OpenBraceToken`). The owners of an opening brace are: `BlockSyntax` (as member, accessor, local-function, lambda, or statement body; or standalone), namespace and type declarations, `AccessorListSyntax`, `InitializerExpressionSyntax` (as `new` initializer, or as an element of another initializer), `AnonymousObjectCreationExpressionSyntax`, `SwitchStatementSyntax`, `SwitchExpressionSyntax`, `PropertyPatternClauseSyntax` (after a header, or starting an arm), and `InterpolationSyntax` (Q1). All of them were checked except the C# 14 extension block and a braced switch section `case 1:` / blank / `{`, which the test implementer should run.
- **RH5011** (next token after `break;`, reported unless it is `}`, a directive is first, or the end of the file): a statement, a switch label, `else` (Q4), the do-statement `while` (Q4), and at top level a type declaration (`while (a < 3)` / `break;` / `class C` is reported, which RH5011-001 does not mention but which is harmless). A `break` that ends the file is not reported, because `GetNextToken` skips the end-of-file token.
- **RH5027** (line above the line holding `while`). What precedes `while` on its line: nothing; the body's `}`; `do` (Q3); the end of an embedded body statement; the end of a multi-line literal or comment (item 8).

## Breaking inputs

- **FC-007 / FC-017 / FC-033** — The spec author's harness applied the first action "until the diagnostic disappeared". That hides a second pass. Tests must use the verifier's default iteration count and pin no multi-pass shape (items 1, 5, 6; Q5) as a tagged requirement. A run of two blank lines next to `{`, `}`, `else`, or `while` needs an explicit iteration override and must stay untagged until Question B is decided.
- **FC-019 / FC-021** — A `/* */` that opens on the `break` or `}` line and closes on the next line (items 1, 5), both with and without code after `*/`. A trailing `//` on the `break` line. The RH5020 boundary pair in item 2. `{ // note` (Q6) against `} // note`, which RH5024 reports.
- **FC-020** — `#region` / `#endregion` next to `{` and `}` on both sides (item 3, Q2's region shape), `#pragma` directly after `break` and after `}`, and region directives inside `#if NEVER` (SHARED-BLANK-007).
- **FC-022** — Verbatim, raw, and interpolated raw strings around every brace and keyword rule: RH5025 (Q1) and RH5027 (item 8) delete string content; RH5022, RH5024 and RH5026 cannot reach string content (shown by line geometry and covered by existing tests); RH5023 excludes whole interpolated strings.
- **FC-013** — Formatter output that the analyzer flags: item 3, RH5027's shapes in item 8, `do break; while (a < 3);`, and Q2's region shape. Analyzer-clean code that the formatter changes: Q6.
- **FC-027** — `else if`, nested initializers, a dictionary initializer, and a property pattern at an arm start (Q2).
- RH5011-001 references SHARED-BLANK-001, but `ContainsBlankLine` misses a whitespace-only line directly after a `///` comment. `break;` / `/// doc` / blank / `a++;` is reported, and the fix inserts a second blank line above the doc comment. The same helper backs the SHARED-STMT base, so this is a follow-up candidate for the shared spec, not for this run. Do not tag RH5011-001 on such an input.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `sdd/specs/analyzer/RH5011.md` | Must change — RH5011-004 wording (item 1); optionally mention the top-level type-declaration case |
| `sdd/specs/analyzer/RH5020.md` | Must change — RH5020-003 qualifier (item 2) |
| `sdd/specs/analyzer/RH5022.md`, `RH5024.md` | Must change — non-goals (item 7); RH5024-001 wording (item 3); RH5024-004 `Why:` (item 6) |
| `sdd/specs/analyzer/RH5023.md` | Must change — non-goal (item 6) |
| `sdd/specs/analyzer/RH5025.md` | Must change — RH5025-001 widened by the intended shapes (item 4); RH5025-003 `Why:` (item 6) |
| `sdd/specs/analyzer/RH5026.md` | Must change — RH5026-003 `Why:` (item 6) |
| `sdd/specs/analyzer/RH5027.md` | Must change — make RH5027-001's restriction explicit (item 8); RH5027-003/-004 `Why:` |
| `sdd/specs/analyzer/RH5030.md` | Must change — RH5030-003 wording (item 5) |
| `sdd/specs/analyzer/RH5028.md`, `RH5031.md`, `RH5032.md`, `shared/region-directive-blank-lines.md`, `shared/whitespace-removal-fixes.md` | Must stay |
| SHARED-BLANK, FMT-BLANK, SHARED-STMT, SHARED-FIX | Must stay (maintainer); item 3 and Question B touch SHARED-BLANK-006 and SHARED-FIX-001 only through filed questions |
| Analyzers, code fixes, Fix All, `Reihitsu.Core`, formatter | Not involved — no production change in this lane |
| Tests | Must change — `[Requirement]` tags; characterization tests for the defective shapes stay untagged |
| `documentation/rules/RH50xx.md` | Must stay — nothing found contradicts them |

## Simpler option

_None._ The corrections are wording changes in the specs. They need no new shared spec and no change to an existing requirement.

## Questions for the maintainer

Nothing here blocks the run. The lead files these with the spec author's candidates, as widened in the table above.

- **Question A** (RH5024, touches SHARED-BLANK-006 and RH5032-001). Input: `int _a;`, blank, `#region R`, then `}`.
  - Reading 1: RH5024 does not report a blank line directly below a `#region`, so RH5032's output stands. That is a bug fix in RH5024.
  - Reading 2: the `#region` before `}` is exempt in SHARED-BLANK-006, as `#endregion` already is, so RH5032 stops inserting the line. That needs a change doc, because SHARED-BLANK-006 and FMT-BLANK-013 change.
  - Today the two rules cycle, and the formatter sides with reading 1.
- **Question B** (RH5022–RH5027 with RH5023, touches SHARED-FIX-001). Input: a run of two blank lines next to `{`, `}`, `else`, `catch`, `finally`, or a do-statement `while`.
  - Reading 1: each rule alone clears the run in one application (report every line, or delete the whole run). That is a bug fix against SHARED-FIX-001.
  - Reading 2: runs belong to RH5023, and SHARED-FIX-001 is read across rules. For RH5022 that is a clarification only: its line and RH5023's never overlap, and one pass of both clears the run. For RH5024–RH5027 it is not enough, because today they report the same last line as RH5023, so even both rules together need two passes. They would have to report the run's first line instead, which is a behavior change (change doc).
  - Today each rule alone takes two passes. The formatter removes the whole run (FMT-BLANK-007, FMT-BLANK-008).
- **RH5011 multi-line comment (item 1) and RH5030 block comment (item 5).** Both already break SHARED-FIX-001, so the lead files them as bugs against the existing spec, or as questions with "intended" marked as unavailable.
