Role: spec author
Commit: 2480901
Verdict: change doc written

The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the binding answers and decisions in `00-lead.md`. This amendment carries out M6 (spec and change-doc side), M7, D15, and D16.

## Changed requirements

Reverted to `main` (M6: the inside of the construct is laid out as on `main`):
- RH5101-003, RH5102-003, RH5417-003 — reverted completely (requirement text, `Uses`, and the `1+2` example). They leave the change doc's `Affected requirements`.
- SHARED-FIX-006 — the *Inside the reported construct* bullet, its `Why` sentence, and its RH5101 and RH5404 examples are removed.
- RH5403-003, RH5404-003 — the M5 sentence, the M5 `Why` sentence, and the "statements/members keep their own code" examples are removed. The #1748 comment examples, `FMT-BLANK-001`/`FMT-BLANK-011`, and `SHARED-FIX-006` in `Uses` stay (M1, D5).

Changed (M7):
- RH5303-005, RH5304-004, RH5305-004, RH5306-004, RH5307-004, RH5308-004 — the fix changes only the reported construct (object creation, assignment, collection expression, list pattern, bracketed argument list, conditional). Its first token keeps its line and column, its other lines are laid out from that column (FMT-PIPE-017), and the code around it stays (SHARED-FIX-006). The text first went back to `main`, then gained these sentences. RH5307 has no "laid out from that column" sentence because its result is one line. `Uses` gains SHARED-FIX-006, and FMT-PIPE-017 where it was missing. New examples: RH5303-005 (`var  x=new List<int> { 1+2,⏎3 }`) and RH5308-004 (`var  r=a ? b+1⏎: 2;`, the maintainer's option C with the inside formatted as on `main`, per M6).
- RH5308-005 — changed (follows from M7 and unchanged SHARED-FIX-010). The fix is withheld only for a refused shape in the reported conditional itself, not for one elsewhere in the statement or member. New example of the other side.
- RH5308-006 — `Why` only. The requirement (no fix outside any statement or member) stays as on `main`, and the old reason ("the fix formats the statement or member") no longer holds. SHARED-FIX-005 leaves `Uses`.

Changed (D15):
- SHARED-FIX-006 *Leading trivia* — gains the exception for the line break SHARED-FIX-007 (*Shared line*) writes in front of a member that shares its line with a member in front of it. The exception references SHARED-FIX-007 and does not restate it.

Code owners (D16):
- `sdd/specs/formatter/pipeline.md` and `sdd/specs/README.md` — `PrecedingTokenFacts` and `RootListPosition` are removed from the FMT-PIPE code owners. The types still exist until the implementer deletes them (D13).

Change doc `sdd/changes/2026-11/fix-layout-scope.md` (still `Status: Draft`):
- Retitled.
- *What* drops the *parts* bullet and gains *The code around a list construct* (M7, with the RH5308-005/006 consequence).
- *What stays* now names the inside of the construct (M6) and the RH5110/RH5403/RH5404 header (D6).
- Visible changes:
  - Old change 6 (RH5101 `1+2`) and old change 7 (RH5404/RH5403 members) are removed.
  - New change 6: M7, RH5308 measured "before" from 04-skeptic, with an RH5303 sentence and the nested Fix All sentence.
  - New change 7: a refused shape elsewhere in the statement now gets a fix. Derived.
  - Change 1 gains the blank line behind `{` and the two blank lines (D16).
  - Change 2 gains RH2001, with a block, and the expression targets RH3004, RH5205, RH5301, and RH5302 (D16).
- *Decisions*:
  - Maps 1–9 to M1, M2, M4, M7, and SHARED-FIX-010.
  - Records M6, with M3/M5 carried to the follow-up.
  - New subsection for D15.
- *Alternatives*:
  - The D6 bullet is rewritten (D16): left out for scope, and names the write-back step.
  - New entries: deferring the inside (M6), Q2 A/B (M7), and the wider RH5308-005 withholding.

No new requirement IDs. No rule page changes: what is reported and how to fix it stay the same in the normal case.

## Examples for the tests

- RH5303-005 (new example) and RH5308-004 (new example): the code in front of the construct stays.
- RH5308-005: the new example where a sibling conditional with `/* c */` behind `?` no longer withholds the fix.
- Unchanged RH5304-004 to RH5307-004 examples. Each needs one test with code around the construct that formatting the document would change, derived from the new sentence, for example `var  a=` in front.
- SHARED-FIX-010 narrowing for RH5303–RH5307: a comment inside the variable declaration around the construct (FMT-UNSUP-004) withheld the fix on `main`, and now the fix is offered.
- Nested constructs (D16): RH5305 with a collection expression as an element, RH5306 with a nested list pattern, RH5308 with a multi-line conditional as a branch, each reported, plus RH5303 or RH5304 where both outer and inner are reported. On `main` both diagnostics had the same target (the statement). Now they are an outer and an inner node, so the batch fixer's merge must be pinned with a Fix All test against SHARED-FIX-002. Change 6 of the change doc says that Fix All still corrects both.
- RH5403/RH5404/RH5101/RH5102/RH5417: no test of "parts keep their own code". Any test written for M3/M5 in an earlier round must not land.
- Unchanged from 03-spec-author: SHARED-FIX-006 *Leading trivia*, FMT-PIPE-017/018/024, SHARED-CHAIN-009, RH5206-003, RH5110/5111/5113/5401/5408 and RH5403/RH5404 comment examples.
- Change doc change 1: a blank line behind `{` stays, and two blank lines above the target stay.
- Change doc change 2: RH2001.
- D15: SHARED-FIX-006 together with SHARED-FIX-007 *Shared line*. A member behind a sibling on its line gets its line break and keeps the rest of its leading trivia.

## Output questions

**1 · RH5308 · Conditional expressions should be formatted correctly — a conditional in an assembly attribute** (RH5308-006). Not blocking: the spec keeps `main`.

Input:
```csharp
[assembly: System.CLSCompliant(true ?
    true :
    false)]
```

Before (`main`): no fix offered.

After, A (recommended): the fix is offered, now that it formats the conditional itself (derived, not measured):
```csharp
[assembly: System.CLSCompliant(true
                                   ? true
                                   : false)]
```

After, B: no fix offered (as `main`; this is what the spec now states).

Why A: once the fix formats only the conditional, the withholding protects nothing, and deleting it deletes a guard. B is written into the spec because it is today's behavior. A would retire RH5308-006 and is a visible change, so it is the lead's to ask, or to leave as a follow-up.

Change 7 of the change doc (a fix offered where a refused shape stands elsewhere in the statement) is a visible output change. The lead turns it into an output question with the other listed changes. It follows from M7 and unchanged SHARED-FIX-010, so I did not list it here separately.

## Internal choices

- RH5308-006 keeps `main`'s withholding, and only its `Why` is restated (see Output question 1). Alternative: retire it now.
- RH5308-005 is narrowed rather than kept. Keeping it would need the fix to ask the formatter about a node it does not format, which SHARED-FIX-010 forbids.
- For RH5304, the construct is the member assignment (`Items = { … }`), not the object creation around it. That is what the diagnostic names. Alternative: the outer object creation, which would format sibling assignments.
- The D15 exception references SHARED-FIX-007 *Shared line* instead of restating where the member's new line is indented.
- The RH2001 example in the change doc derives the field name `_value`. The blank line and the three-line `<summary>` were measured by 04-skeptic.
- FMT-PIPE-017's tab example still names an `EqualsValueClauseSyntax` target. It is an entry-point example and stays true, although RH5303 no longer passes that node. It is left as is.

## Follow-up candidates

- FU4 (`00-lead.md`) is unchanged and carries M3, M5, and the spanning-part question.
- If question 1 is answered B, or the lead leaves it unasked: offering the RH5308 fix for a conditional in an `[assembly:]`/`[module:]` attribute argument.

## Whole run

- Changed files:
  - `sdd/changes/2026-11/fix-layout-scope.md`
  - `sdd/specs/shared/code-fixes.md`
  - `sdd/specs/formatter/pipeline.md`
  - `sdd/specs/README.md`
  - `sdd/specs/analyzer/RH5101.md`, `RH5102.md`, `RH5417.md` (back to `main`)
  - `RH5303.md`–`RH5308.md`, `RH5403.md`, `RH5404.md`
  - this handover
- `scripts/test.sh --project architecture --filter "FullyQualifiedName~Sdd|FullyQualifiedName~Rule"`: passed, 57/57, after SHARED-FIX-005 left RH5308's `Uses`.

## Amendment for M8, M9 (head 13971d2)

- RH5308-006 is retired (M9), and its heading stays. Its `Status: Retired` line gives the reason. RH5308-004 gains the `[assembly:]` example as the positive case, derived and not measured. This replaces Output question 1 and the follow-up candidate above.
- Change doc:
  - `Status: Accepted`, as the lead said.
  - *What* states that RH5308-006 is retired.
  - Change 7 is decided by M8 (A). Its "after" is still marked as derived, because it was not measured.
  - New change 8 is the assembly-attribute conditional (M9). The old changes 8 and 9 are now 9 and 10, and the references to them are updated.
  - *Decisions* maps 7 to M8 and 8 to M9.
  - *Alternatives* gains "Keeping RH5308-006".
- Tests: the new RH5308-004 assembly example, and the RH5308-005 example.
- The architecture test for spec tags now fails, which is expected. Three tests in `src/Reihitsu.Analyzer.Test/Formatting/RH5308ConditionalExpressionsShouldBeFormattedCorrectlyAnalyzerTests.cs` (around lines 702, 720 and 739) are tagged with the retired RH5308-006. The test implementer must rewrite them to show the fix offered and tag them RH5308-004. I do not touch tests. Every other Sdd/Rule check passes, 56/57.
