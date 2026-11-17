Role: spec author
Commit: 17fb08c (statements about today's output derived from `main` at this commit; the spec diff is 6990dee)
Verdict: change doc written

The prompt carried no intended fix or hoped-for verdict. The issue (thoenissen/Reihitsu#1732, `agent-filed`) proposes deleting "RH5401-004's format-and-compare for a comment behind `=>`"; that is not done: `CanCollapse` also decides the wrapped expression and a comment in front of the arrow, so it is kept, and the comment behind the arrow is withheld through `FindUnsupportedSyntax`, which the fix already asks (see *Internal choices*). The issue's suggested CLI reason text is replaced by another wording (output question 6).

## Changed requirements

- FMT-UNSUP-012 — Comments and directives between the arrow of an expression body and its expression are unsupported — added (`sdd/specs/formatter/unsupported-syntax.md`; `requirement-ids.txt` updated; `Uses` row updated).
- FMT-STRUCT-006 — The converted body keeps the expression's trivia — changed: every placement rule for a comment between the arrow and the expression, and the "comment directly behind the arrow" half of "directly at the arrow", removed; the gap is refused under FMT-UNSUP-012; the example now shows an own-line comment in front of the arrow and a comment behind the signature (`void Run() // run`) instead of the comments behind the arrow.
- FMT-STRUCT-007 — Directives that cannot survive keep the expression body — changed: the clause "any directive between the arrow and the start of the expression" is removed from the formatter's MUST and pointed to FMT-UNSUP-012; Why notes that `BlocksRewrite` keeps the case for the analyzers (SHARED-BODY-001).
- FMT-STRUCT-015 — Get-only members that keep their accessor list — changed: "around the getter's arrow, before the expression" becomes "in front of the getter's arrow"; behind the arrow is FMT-UNSUP-012.
- FMT-STRUCT-024 — A rewrite that cannot be completed safely is not applied — changed: example replaced (a conditional group whose `#if` precedes the member and whose `#else` is between arrow and `;`; `M` keeps its expression body — checked with `scripts/format.sh`).
- FMT-STRUCT-026 — The phase writes only the separators its text needs — changed: the two examples starting from a comment behind the arrow (`return/* c */ 42;` whitespace, and "a comment directly at the arrow keeps the other comments…") removed.
- SHARED-BODY-001 — A member the formatter cannot convert is not reported — changed: names the directive or disabled text between `=>` and the expression itself (FMT-UNSUP-012) instead of reaching it through FMT-STRUCT-007; a comment there is reported, fix withheld (SHARED-FIX-010). Output unchanged.
- RH5401-004 — The fix is withheld when the property stays on several lines — changed: a comment/directive in front of `=>` keeps the property on several lines; one behind `=>` withholds the fix because the formatter refuses the property (SHARED-FIX-010). New example member `int C => // note⏎1;` (reported, no fix). Output unchanged.
- RH7101-007 — The fix is withheld for a comment inside the declaration — changed: FMT-UNSUP-012 added to the list of shapes; new example (`int A => // c⏎_a;` elsewhere in the type).
- `AGENTS.md`, phase-ownership table, `PrecheckPhase` row: the sixth shape added.
- Change doc: `sdd/changes/2026-11/expression-body-arrow-comment-unsupported.md` (`Status: Draft`), with `Visible output changes` 1–10 and `Special cases` (Deleted / Checked and kept).

No rule page changes: the fixes lose their action only for the refused shape, which `documentation/rules/general-notes.md` covers.

## Examples for the tests

- FMT-UNSUP-012 — the refused example (location `// c1`, line 5, column 14), the directive example (location `#if DEBUG`, line 4, column 1, both builds), and the "other side" example (formatted; checked to parse and format today).
- FMT-STRUCT-006 — the new example (`Other` with `// before arrow`, `Run() // run` with `// first`); output checked with `scripts/format.sh` on `main`.
- FMT-STRUCT-024 — the new example (`M` keeps its expression body; checked on `main`).
- RH5401-004 — the new `C` member: reported, no fix offered.
- RH7101-007 — the new example: reported, no fix offered.
- SHARED-BODY-001 — existing examples stay valid; a comment between `=>` and the expression is reported and no fix is offered (change doc, change 9).
- Change doc *Visible output changes* 1–10: each input with its location; every "Before" was produced with `scripts/format.sh` or `scripts/apply-fix.sh` (RH3202, RH3203, RH3208, RH5401, RH7101) on `main` at 17fb08c.

Existing tests whose tagged behavior the spec diff removes (facts for the test implementer): `src/Reihitsu.Formatter.Test/Regression/Structural/ExpressionBodyArrowCommentTests.cs` (tagged FMT-STRUCT-006; many inputs hold a comment behind the arrow), `src/Reihitsu.Formatter.Test/Regression/Structural/ExpressionBodyDirectiveGuardTests.cs` (tagged FMT-STRUCT-007; directive-behind-arrow inputs), and the tests of the two removed FMT-STRUCT-026 examples. `scripts/test.sh --project architecture` passes except `EveryRequirementOfAnEnforcedSpecIsCovered`, which reports FMT-UNSUP-012 as uncovered — expected until the test implementer adds its tests.

## Output questions

All within thoenissen/Reihitsu#1732; each has its input, `main` output, and the output of each option in the change doc (`Visible output changes`). Locations below are 1-based (line, column). "Refused" means: file unchanged, CLI reports `error: <file>(<line>,<column>): unsupported syntax: <reason>. The file was left unchanged.`, exit code 2.

1. **Declare the shape (escalation 10, confirmation of the maintainer's 2026-11-14 decision) — properties, indexers, and the converted members.** Changes 1–4 of the change doc. Input `int A => // c⏎        1;` — `main`: `1;` in column 4 (member's column); `int B =>⏎        // c⏎        1;` — `main`: a blank line is inserted in front of `// c`, both in column 4; `int Get() => // c⏎42;` — `main`: `{// c` on the new brace; `int Other() =>⏎// after arrow⏎42;` — `main`: converted well, comment leads `return 42;` (this is what the declaration costs). A (recommended): refused. B: not declared; the issue's earlier alternatives (#1730/#1731, closed by the maintainer as superseded) would be reopened. Requirement IDs: FMT-UNSUP-012, FMT-STRUCT-006, FMT-BREAK-026. Recommendation A: it is the maintainer's recorded decision, and it deletes the converter's comment cases and the join refusal instead of adding a layout.
2. **Accessors and local functions belong to the shape.** Changes 6 and 7. `get => // c⏎_v;` — `main`: `_v;` in the accessor's column; `int Local() => /* c */ 1;` — `main`: `{/* c */`. A (recommended): refused. B: only members (RH5040's scope); accessors and local functions formatted as on `main`. Recommendation A: local functions run through the same conversion code, so B keeps every FMT-STRUCT-006 case alive; accessors share the column-of-the-member layout.
3. **Directives and disabled text belong to the shape.** Change 8. A property and a method whose expression is chosen by `#if DEBUG`/`#else` behind the arrow — `main`: the method keeps its body (FMT-STRUCT-007), and the active line of each lands in the member's column (`2;`/`0;` in column 4). A (recommended): refused at (4,1) in every build. B: comments only; directives formatted as on `main`. Recommendation A: the join refusal and the get-only seam check test directives alike, so B deletes neither.
4. **Also on one line.** Change 5. `class C{⏎    int Get() => /* c */ 42;⏎}` — `main`: `class C` brace fixed, `{/* c */` on the new brace. A (recommended): refused at (2,18); `class C{` stays as written. B: only a gap with a line break; the one-line case formatted as on `main`. Recommendation A: same boundary as FMT-UNSUP-008/011, and the converter's carry-over of a same-line comment onto the brace goes only under A.
5. **Code fixes withheld.** Changes 9 and 10. RH3202, RH3203, RH3205–RH3208 for a member holding the shape (`main`: offered, gives `{// c` or the own-line form); RH7101 when the type holds the shape anywhere (`main`: offered, splits the fields); every formatter-delegating fix whose node holds it. Diagnostics stay. RH5401 unchanged (reported, no fix, before and after). A (recommended): no fix offered. B: not possible without a check in each fix that copies the precheck (SHARED-FIX-010 forbids). Recommendation A: follows from SHARED-FIX-010 without code in the fixes.
6. **The CLI reason text.** A (recommended): `a comment or directive between the arrow of an expression body and its expression`. B (the issue's wording): ``a comment or directive between the `=>` of an expression-bodied member and its expression``. Recommendation A: no other reason uses backticks, and "expression body" covers accessors and local functions, which "member" does not.

## Internal choices

- `ExpressionBodyRewriteUtilities.BlocksRewrite` keeps its check of a directive between the arrow and the expression, because the analyzers of RH3202–RH3208 decide by it not to report the member; SHARED-BODY-001 now names the case itself. Alternative: delete the check and let those rules report such a member with no fix — a visible diagnostic change the issue does not ask for; not chosen (could be a change of its own).
- RH5401-004 changed in place, `CanCollapse` kept (it decides the wrapped expression and the comment in front of `=>`); alternative: delete the format-and-compare as the issue suggests — would lose the withholding for those two cases.
- Where FMT-UNSUP-010 refuses the same directive (a local function in a lambda in a field with several declarators), FMT-UNSUP-010's reason wins, as for FMT-UNSUP-009/011. Alternative: the new reason.
- Lambdas and switch-expression arms are not part of the shape (nothing would be deleted); written as a condition with both sides in FMT-UNSUP-012.
- FMT-UNSUP-005 gets no new example; FMT-UNSUP-012's Why states why no phase writes into the gap (created arrows have empty trivia; the FMT-BREAK-026 join carries only whitespace).
- FMT-STRUCT-024's example replaced rather than dropped, so the requirement keeps an example.
- `AGENTS.md`'s `PrecheckPhase` row updated with the sixth shape.

## Follow-up candidates

- **FMT-STRUCT-006 · comment directly in front of the arrow on its line.** Outside the run (the maintainer's decision names only the position behind `=>`). Input:
  ```csharp
  class C
  {
      int B()
          /* c */ => 1;
  }
  ```
  `main` output:
  ```csharp
  class C
  {
      int B()
      /* c */ {
          return 1;
      }
  }
  ```
  Looks wrong: the comment ends up in front of the opening brace on its line. FMT-STRUCT-006 leaves the placement of a comment directly at the arrow unspecified. Readings: A — keep as `main` (specify it); B — the comment leads the created statement (`{⏎/* c */⏎return 1;`); C — declare a comment in front of the arrow unsupported too (escalation 10, it would delete `ArrowLineHoldsComment` and the rest of the arrow-leading-trivia handling). RH5040 already reports the comment. Recommendation: B or C for the maintainer; no pick within this run.
