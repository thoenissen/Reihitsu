Role: skeptic
Commit: 02f04f0
Verdict: proceed

The prompt carried no intended fix, suspected cause, or hoped-for verdict. The decided principle texts (P9, P10, P14a–d, P23, P24, the SHARED-FIX-002 clarification) are not reopened here. This handover challenges the wording the spec author added, the examples, and the non-goals, and checks them against the code and the other specs. Scratch fixtures lived outside the repository and are deleted.

## Premise

The run writes decided policy into `shared/code-fixes.md`, so the premise is real. The spec diff (`git diff 1f677d9 02f04f0 -- sdd/specs`) states every decided text except one clause (F3). It needs these repairs by the spec author before tests are tagged. F1 decides what the test implementer may tag.

**F1 — must fix. SHARED-FIX-007's normative sentence contradicts its own RH7303 example and the decided blank-line precedence.** "Formatting the result changes nothing in that code" is false for blank lines that P14a and P14c forbid, while the formatter inserts them:
- `scripts/format.sh` on the RH7303 example's output (`var x = 1;⏎x++;⏎x--;`) inserts a blank line after `var x = 1;`. That is the exact gap from which the fix removed `#region Work`, so it is code the fix changed.
- `scripts/format.sh` on `if (a) { }⏎M(a);` inserts a blank line after `}`. Round 7b decided that instance on thoenissen/Reihitsu#1263 (comment): nothing is inserted, because P14a forbids the blank line the formatter would want.
- thoenissen/Reihitsu#1209 Q3 and thoenissen/Reihitsu#1220 Q2 decided P14a over P10 for the blank line that the formatter's layout puts in front of the changed statement.

The precedence is decided but not written. Recommend: SHARED-FIX-007 says that the blank lines in front of and behind the changed code follow SHARED-FIX-006 and the removed-directive bullet, also where formatting the result would change them. The RH7303 example then says that formatting would insert a blank line after `var x = 1;` and that the fix inserts none. Without this, a SHARED-FIX-007 test that checks formatter stability fails on the spec's own RH7303 example.

**F2 — should fix. SHARED-FIX-006 has no pointer to SHARED-FIX-007's shared-line case.** In the RH5417 example, `_x` moves to the next line. That changes the layout of code around the changed member, which SHARED-FIX-006's first sentence forbids when read alone. The spec author added the reverse pointer (007 → 006), and a 006 → 007 pointer only for removed directive lines. Add the matching pointer for the shared line.

**F3 — should fix. A decided clause of P14b was dropped.** The issue's P14b ends "the lines the fix edits are laid out as the formatter lays them out (SHARED-FIX-007)". The spec drops the whole clause, not only the parenthetical self-reference. The lead's scope says decided texts are written as stated. Restore the clause without the parenthetical, or have the lead confirm with the maintainer that it only restates P10.
- Read as P10, the clause changes nothing.
- Read as "every line the fix edits", it changes the output for `void M() { } int  _x;` (`int  _x;` against `int _x;`).
- Only the P10 reading is consistent with P14d. Restoring the clause keeps the maintainer's text and does not pick a reading.

**F4 — should fix. SHARED-FIX-008 exempts literals only; the formatter and RH5604 exempt more.**
- FMT-PIPE-007 keeps the original line ending inside literals, inside comments other than documentation comments, and inside disabled text.
- `LineEndingUtilities.DetectEndOfLine`, RH5604-001 and RH5604-005 neither count nor convert those line breaks.
- SHARED-FIX-003 makes the line breaks inside a comment content.
- So in a document whose counted line breaks are all CRLF, an LF can sit inside a `/* */` comment or under `#if false`. A fix that keeps that comment would have to write CRLF there under SHARED-FIX-008, which SHARED-FIX-003 forbids.

Recommend: define "the line ending the document uses" by the line breaks FMT-PIPE-007 counts, and exempt the same three categories. Referencing FMT-PIPE-007 adds it to `Uses`. The new non-goal ("no line break, or line breaks that already mix") then reads against the same set.

**F5 — low. SHARED-FIX-007's `Why:` claims more than its examples bear out.**
- It says no layout the author chose is lost, and that the fix's own edit produced the shapes.
- In the RH7310 start-of-body example, the blank line after `{` is the author's and existed before the fix. thoenissen/Reihitsu#1263 decided its removal.
- In the RH5417 example, `M` and `_x` already shared their line before the fix.

A `Why:` decides edge cases, so reword it to fit the examples.

**F6 — low. The P14b bullet only fits a member that follows on the line.** "The other member MUST start on the next line" assumes the other member comes after the changed one. In the mirror shape `int _x; void M() { }`, RH5417 today gives `int _x; void M()` / `{` / `}`, with the braces at column 12. `M` does not get its own line, which breaks the bullet's first clause. thoenissen/Reihitsu#1144 does not list this instance. Add a sentence or an example for the case where the other member comes first. The instance is a follow-up candidate for thoenissen/Reihitsu#1144.

**F7 — low. SHARED-FIX-002 sentence order.** "Every reported diagnostic of a document, a project, or a solution whose fix is offered…": `whose` can attach to "solution". Reorder the sentence.

**F8 — low, optional. One SHARED-FIX-003 bound lost its example.** The bound "in a documentation comment, also the `///` exterior" had its only example in the replaced RH3202 example. Whitespace next to an XML tag keeps one example (RH8107); the `///` exterior has none now.

**F9 — optional, simpler.** SHARED-FIX-002's RH7103 example repeats SHARED-FIX-003's RH7103 example word for word. It could point to that example instead.

Checked and correct:
- RH7310 between members: "leaves two blank lines … one goes" is right. RH7310-004 removes the region's text, the inner blank line included, so the blank line before and the blank line after remain.
- The RH5101 example's `2);` at column 10 matches RH5101-003, which aligns the following arguments.
- The RH7310 start-of-body case matches the thoenissen/Reihitsu#1263 instance "a leading region removed after `{⏎⏎`".
- The RH3203 example matches thoenissen/Reihitsu#1284 instance 1.
- The SHARED-FIX-002 claim "in the result, `_b` is still reported and is offered its fix" holds: `scripts/apply-fix.sh RH7103` on the result of the `_a` fix moves `_b` in front of `_instance` in one iteration. I did not run the single Fix All application.

P23 ("a rule reports the construct it names for every input") is decided text. Read literally, it overreaches SHARED-DIAG-001: generated code is not analyzed. The test implementer must not read it as a requirement about generated code. Its `typeof(Nullable<>)` example is tracked by thoenissen/Reihitsu#1281. RH3103-001 still says "one written type argument".

## Mechanism and candidates

_N/A._ This is not a bug fix. The inputs the code decides but the spec does not mention are listed under Breaking inputs.

## Breaking inputs

Failure classes for the test implementer:
- **FC-030** — SHARED-FIX-006 tests need unformatted code next to the fix, on the same line and on the lines around it, so that a fix formatting the statement or member would show.
- **FC-003** — cover both sides of each qualifier:
  - a blank line that is the diagnostic's code (the RH5002 example under SHARED-FIX-001) against one that is not (RH3202-005);
  - a shared line (RH5417 `M`/`_x`, not met today) against a member on its own line (RH5417 `N`);
  - a single blank line left behind, which stays (RH7310 `Fields`), against a doubled one, of which one goes (RH7310 `Methods`).
- **FC-013** — SHARED-FIX-007 coverage should check that formatting the fix result changes nothing in the changed code. The RH7310 between-members output passes that check: `scripts/format.sh` changes only the final newline, which the CLI strips. The RH7303 output does not pass it until F1 is written.
- **FC-017 and FC-033** — the SHARED-FIX-008 test runs under CRLF and LF with exact output. It must show that the verifier compares line-ending bytes, so a fix that writes LF into a CRLF document fails the test.
- **FC-008** — the SHARED-FIX-002 RH7103 example takes a single Fix All application, with `_b` still expected as a diagnostic.
- **FC-019 and FC-020** — the P14c tests are directive-gap tests.
- **FC-034** — seed the tests from the spec examples, not from simplified cousins.

Conforming shapes, all run with `scripts/apply-fix.sh` under LF and CRLF, none with `[line-ending-drift]`:
- **SHARED-FIX-006**
  - `VerifyBlankLineAboveFixedMethodIsKept` (RH3202-005).
  - RH6024 on `int  z = a+b;` between `int  y=1;` and `return  z;`: only the two gaps change.
  - RH4106 on `private int Value;`, then two blank lines, then `int  Get( ) => Value;`: only the names change, and the two blank lines and `int  Get( )` stay.
- **SHARED-FIX-007**
  - The RH7310 between-members example as a whole: today's output equals the spec's output, not only its `Methods` half.
  - The RH5417 `N` half: own line, and the blank line above it stays.
- **SHARED-FIX-008** — the RH5405 example, where `if` is the first statement: `VerifyFixWrapsBodyOnLineOfIfInBracesWithCarriageReturnLineFeed`.

Shapes that break a new requirement today. Do not tag them, and do not write characterization tests for them:
- RH6024 lambda (thoenissen/Reihitsu#1141).
- RH3203 attribute and doc comment, and RH3202 two blank lines (thoenissen/Reihitsu#1284).
- RH5405 after a statement: it inserts a blank line before the `if`. `VerifyBlankLineIsInsertedAboveFixedStatementAfterDeclaration` and the related tests pin that blank line (thoenissen/Reihitsu#1209 Q3 → thoenissen/Reihitsu#1140).
- RH5417 `} int _x;` (thoenissen/Reihitsu#1144).
- RH7310 start and end of a body (thoenissen/Reihitsu#1263).
- RH5101 raw string (thoenissen/Reihitsu#1144).
- Ordering moves (thoenissen/Reihitsu#1335, thoenissen/Reihitsu#1331).
- RH5104-005 (see Questions).

Inputs the code decides that the spec does not mention:
- **A document whose line breaks are all a lone CR, or a Unicode line separator.** `DetectEndOfLine` counts a single-character end of line as `\n`, so a fix that delegates to the formatter would write `\n`. SHARED-FIX-008's first sentence ("the line ending the document already uses") forbids that. Derived from the code, not run.
- **A changed member whose closing brace ends up on the line of its type's closing brace.** RH5417 turns `void M() { } }` into `}` and `}` on one line (`} }`). P14b covers only "another member" (see Questions).

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `sdd/specs/shared/code-fixes.md` | Must change: F1 to F4, F5 to F9 optional |
| `sdd/specs/formatter/pipeline.md` (FMT-PIPE-010 `Why:` and `Uses`) | Changed as needed; nothing else |
| `sdd/specs/requirement-ids.txt` | Changed (three IDs); nothing else |
| Analyzer, code fixes, Fix All, formatter, `Reihitsu.Core` (`LineEndingUtilities`) | Must stay; no production change in this run |
| Tests | Must change: covering tests tagged SHARED-FIX-006, -007 and -008, from the conforming shapes above |
| `documentation/rules/` and `general-notes.md` | Not involved; nothing there contradicts the new requirements |
| Other specs now contradicted | Must stay in this run (scope). Tracked: SHARED-CHAIN-009, SHARED-BRACE-002, RH5101-003, RH5102-003, RH5110, RH5113, RH5204, RH5403 (thoenissen/Reihitsu#1140); SHARED-BODY-003 (thoenissen/Reihitsu#1284); RH5417-003 (thoenissen/Reihitsu#1144); RH7303-004 and RH7310-004 (thoenissen/Reihitsu#1263). The RH7103 non-goal still calls the fix's reformat scope an open question, which SHARED-FIX-006 now decides (thoenissen/Reihitsu#969). Untracked: RH5104-005 |

Follow-up candidates, beyond this run's scope:
- These requirements restate the inserted-line-ending rule that SHARED-FIX-008 now owns. `sdd/README.md` asks for a reference instead: SHARED-STMT-004, RH5002-005, SHARED-AFTERBRACE-001, SHARED-REGION-002, RH5011-005, RH8303-005, SHARED-ATTR-008 and SHARED-ATTR-017. The two SHARED-ATTR requirements cite FMT-PIPE-007.
- The RH7103 non-goal that names an open question should be cleaned up with thoenissen/Reihitsu#969.
- The RH5417 mirror shape (F6) should be added to thoenissen/Reihitsu#1144.

## Simpler option

_None._ The requirement set is fixed by decision. F9 is the only simplification, and it is optional.

## Questions for the maintainer

No `SDD open questions: sdd/specs/shared/code-fixes.md` issue is open (#1066, #1110, #1113 and #1126 are closed). None of these questions blocks this run's tests.

1. **Does P14a reach the blank line that RH5104-005 inserts above a moved comment?** This touches SHARED-FIX-006 and RH5104-005, and no follow-up tracks it.
   - Input: inside a method, `M();⏎M(); // second`.
   - Real output (`scripts/apply-fix.sh RH5104`): `M();⏎⏎// second⏎M();`. Tests tagged RH5104-005 pin it, for example `VerifyRelocatedCommentGetsPrecedingBlankLineWhenPreviousLineIsCode`.
   - Reading (a), P14a: RH5104's own non-goal gives that blank line to RH5020, so it is not the code its diagnostic is about, and none is inserted. Output: `M();⏎// second⏎M();`.
   - Reading (b): the blank line is part of placing the moved comment, and RH5104-005 stays. Output: today's.
2. **The spec author's P14b question (a fix that does not lay out the member). I support filing it.** Real output, now run:
   - RH4106 on `private int Value; private int _b;` gives `private int _value; private int _b;`.
   - RH4106 also changes a member that only refers to the field: `int Get() => Value; int Q() => 1;` becomes `int Get() => _value; int Q() => 1;`.
   - Under reading (b), `Get` would then also have to get its own line.
3. **Does SHARED-FIX-007 reach the line break between a changed member and its type's closing brace?**
   - Input: `class C⏎{⏎    void M() { } }`.
   - Real output (RH5417): `void M()⏎    {⏎    } }`.
   - Reading (a): the gap behind the changed brace is changed code, so the type's `}` starts the next line.
   - Reading (b): P14b names only members, so `} }` stays.
4. **Low priority: a document whose line breaks are a lone CR or a Unicode separator.** Is it covered by SHARED-FIX-008, which would require that ending, or is it a non-goal? Run the real output before filing; I derived it from `DetectEndOfLine` only.
