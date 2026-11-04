Role: spec author
Commit: 4da07393
Verdict: spec written

Wording round. Inputs: `00-lead.md` (*Decisions taken* rows 14 and 15), `04-skeptic.md` (*Internal choices* 1–4, class A and class D test candidates). No decided output changed. The open question on a tab-indented declaring type (04-skeptic, *Output questions* 1) is untouched: FMT-PIPE-029's tab sentence and example stay as written, which is the skeptic's option A. My prompt carried no intended fix, suspected cause, or hoped-for verdict.

Measured with today's fixes: `scripts/apply-fix.sh RH7101 … --max-iterations 1` on the two documentation-comment inputs (the change doc's "before" blocks), and `scripts/apply-fix.sh RH7203 …` on the class D input (the FMT-PIPE-030 example). Both line endings agree. `scripts/test.sh --project architecture --filter "FullyQualifiedName~Spec"`: 36 passed, 1 failed. The failure is `EveryRequirementOfAnEnforcedSpecIsCovered`, for FMT-PIPE-029/030/031, which still need tests, the same as in 03.

## Changed requirements

- FMT-PIPE-009 — Host-independent entry points — clarified (row 14). The rule names the types it bans: a phase, a transform, another rewriter a phase runs, a member of any of them, and `FormattingPipeline`. It is keyed on those types, not on a namespace. A helper or utilities class that is none of them stays callable wherever it is declared, `ReihitsuFormatterHelpers` and `ExpressionBodiedTransformUtilities` as named examples. `AGENTS.md`, *Key conventions*, says the same.
- FMT-PIPE-010 — Node-level entry points format only their target — clarified (row 15, 04 choice 1). Two exceptions are now named: the existing documentation-comment one, and the whitespace between a split target and another member on its line, which FMT-PIPE-029 replaces. The target's trailing trivia is kept "apart from the whitespace FMT-PIPE-029 replaces behind the target".
- FMT-PIPE-018 — The context entry point writes back its target's leading trivia as in the document — clarified (04 choice 1). One exception is added: a target that FMT-PIPE-029 moves off the line of a member in front of it. Its leading trivia is still written back as it stands. Its new line starts where the context's depth places a member, and its other lines are laid out from that column.
- FMT-PIPE-029 — The context entry point writes back every field the split makes of its target — clarified (row 15, 04 choice 2, class A):
  - "first line" for the front case and "last line" for the behind case;
  - the front case's line starts with the target's leading trivia, such as a documentation comment, at the member column;
  - a new gap bullet: a comment in the trailing trivia of the token in front of the gap stays behind it, and a documentation comment, which is leading trivia of the token behind the gap, goes with its member;
  - "exactly one line break and no blank line", also where document-level formatting writes one (SHARED-FIX-006);
  - "as document-level formatting replaces it" is dropped, and the Why is extended to match.
  
  Examples: the `/* c */` example gains `/* d */` behind the target. Two examples are new: a documentation comment in each gap, and a two-line target with a member behind its last line.
- FMT-PIPE-030 — The using entry point organizes one scope's using block — clarified (04 choice 3). The phase reads the scope where it stands in the document, and FMT-PIPE-023 does not apply. SHARED-USING-010 takes the document line of the first directive, also when the scope does not start that line. A new example covers the class D input, `namespace A { namespace B { … } }`, with the output measured from today's RH7203 fix.
- RH7101-004 — clarified: one line break and no blank line in the gap, and a documentation comment goes with the member it documents.
- Change doc `sdd/changes/2026-11/rh7101-fix-through-context-entry-point.md` (still `Status: Draft`):
  - case 7 gains the two documentation-comment inputs, with before (measured) and after;
  - the summary and the case-7 note follow the new gap wording;
  - the architecture answer notes that the rule is keyed on types, not on a namespace.
- SHARED-JOIN-002 reference from RH7101-001 — unchanged (04 choice 4, accepted as is).

## Examples for the tests

- **FMT-PIPE-029:** every example, three of them new or changed:
  - the `/* c */ … /* d */` example;
  - the documentation-comment example;
  - the two-line target.

  The two single-sided documentation-comment inputs of the change doc's case 7 cover the same ground for the RH7101 code-fix tests. 04-skeptic class A asks for a single Fix All iteration in those tests, and class B for `private int _a, _b; private int _c, _d;` with the harness default of one Fix All iteration. Class C lists four first-token shapes.
- **FMT-PIPE-030:** both examples. The new one guards that the scope is read in the document.
- **FMT-PIPE-009:** the architecture test is keyed on `IFormattingPhase` implementations, the transforms and rewriters those phases run, and `FormattingPipeline`. It is not keyed on the `Reihitsu.Formatter.Pipeline` namespace. The RH3202 and RH3205–RH3208 fixes' use of `ExpressionBodiedTransformUtilities` must pass it.

## Output questions

_None new._ The tab-indented declaring type (04-skeptic, *Output questions* 1) is with the maintainer, and the spec stays on option A until the answer comes.

## Internal choices

- The documentation-comment variants are added to the change doc's case 7 as visible output changes. They differ from today's fix output, but unchanged requirements already decide them (row 15). Alternative: leave them out of the change doc, which would leave visible changes unlisted.
- FMT-PIPE-009 names `ExpressionBodiedTransformUtilities` as an allowed example, so the architecture test's scope cannot be read as the namespace. Alternative: name only `ReihitsuFormatterHelpers`, which leaves the 04-skeptic ambiguity open.
- FMT-PIPE-030 states that FMT-PIPE-023 does not apply, rather than adding the entry point to FMT-PIPE-023 as an exception there. FMT-PIPE-023 lists only the node-level entry points, and this one is not one of them.

## Follow-up candidates

_None new._ The candidates in `00-lead.md` and 04-skeptic stand.
