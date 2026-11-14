Role: spec author
Commit: 42d05f4
Verdict: change doc written

Amendment round after output batch 1 (lane behavior change, decision 10). Change doc: `sdd/changes/2026-11/documentation-rules-reported-elements.md` (Status `Draft`; the lead sets it). It covers Q1 C, Q2 A, and Q4 B, records each answer under `Decisions`, and lists every visible output change, the RH8307 rule page included. Q4 B is in it: it changes RH8201-004's count and RH8201-001/RH8203-001 coverage to an output that differs from `main`, so it is not a clarification. The prompt carried no intended fix or hoped-for verdict beyond the lead's recorded decisions.

No new requirement IDs and no retirements in this round; `sdd/specs/requirement-ids.txt` is unchanged and consistent (RH8303-006, RH8401-006, RH8402-007 from round 01 stay). `scripts/test.sh --project architecture --filter "…Spec|…RequirementCoverage|…Change|…RuleDocumentation"`: 49 passed, 2 failed, the same two expected failures as in 01 (the three new IDs without tests; three tests tagging retired RH8307-006).

## thoenissen/Reihitsu#1345

No change in this round (Q3 B confirms round 01: not declared unsupported; RH8303-006 stands).

## thoenissen/Reihitsu#1346

No change in this round.

## thoenissen/Reihitsu#1347

### Changed requirements

- RH8307-001 — Content that starts beside the start tag and ends below it is reported — changed: only an element at the top level of the comment (one no other element encloses) is reported; the nested `<para>` instance moved out of its example; Why names the structuring elements; coverage points to RH8307-002 for nested elements.
- RH8307-002 — Other layouts are not reported — changed: an element nested inside another element is not reported, whatever its layout and kind (`<para>`, `<b>`, `<c>`, `<see>`); new negative example (`<para>` and `<b>` half-wrapped inside `<remarks>`); new two-sided example for the qualifier "at the top level" (`<remarks>Text⏎<para>First⏎second</para>⏎</remarks>`: `<remarks>` reported, `<para>` not). The `/** */` sentence drops its "top level or nested" (now general).
- Intro sentence and Non-goals of `RH8307.md` name the top level / nested elements.
- Rule page `documentation/rules/RH8307.md` — Description changed to name top-level elements and say nested elements are not checked: what is reported changes in the normal case (nested `<para>` is common). Listed in the change doc as a visible change; the lead may want it in the next batch if it reads Q1 C as not covering the page text.

### Examples for the tests

RH8307-001 (example without `<para>`), RH8307-002 (the nested `<para>`/`<b>` block; the top-level qualifier block).

### Test tags the amendment retires or moves

- `VerifyContentStartingBesideTheStartTagIsReported` (`RH8307…AnalyzerTests.cs:238`, tags RH8307-001, -003): its `{|#2:<para>First…|}` expectation is now wrong; drop that diagnostic (the spec example dropped it).
- `VerifyFixForNestedElementOfSingleLineDocumentationComment` (`:874`, tags RH8307-004, RH8307-006): nested `<para>` in `///` is no longer reported; becomes a no-diagnostic test tagged RH8307-002 only.
- `VerifyNoCodeFixForNestedElementOfDelimitedDocumentationComment` (`:847`, RH8307-006): not reported; retag RH8307-002.
- `VerifySpecificationExampleNoCodeFixForElementOfDelimitedDocumentationComment` (`:824`, RH8307-006): not reported; retag RH8307-002.
- RH8307-006 stays retired (round 01); no test may tag it.

## thoenissen/Reihitsu#1348

### Changed requirements

- FMT-BLANK-010 — No blank line after a documentation comment — reworded (decision 9): the addition now states the required output — exactly one blank line between the documentation comment and an active `#region`/`#endregion` directive, whether the input has none, one, or several — and no longer claims a single owner or order independence. Why: the line belongs to the directive (SHARED-BLANK-005); without it the output carries what RH5031 reports. Example unchanged.

### Examples for the tests

FMT-BLANK-010 (region example), unchanged from 01.

## thoenissen/Reihitsu#1270

### Changed requirements

- RH8106-001 — coverage changed: "at most one `<returns>` element without text" removed.
- RH8106-002 — The diagnostic is on the element — changed: one diagnostic per reported `<returns>` element; several empty elements are each reported; new example (`<returns/>` and `<returns></returns>` on `F`, both reported).
- RH8306-001 — coverage changed: several top-level summaries of a property in one part are covered without the "at most one reported" limit. The partial-property paragraph (reported once, SHARED-DIAG-012) stays.
- RH8306-004 — The diagnostic covers the summary — changed: one diagnostic per reported `<summary>`; a partial property is reported once (RH8306-001); new example (`Size` with two sentence summaries, both reported).

### Examples for the tests

RH8106-002 (`F`), RH8306-004 (`Size`), plus the 01 examples (RH8106-001 `F`/`G`, RH8306-001 `Depth`/`Height`, RH8203-001 `Notifier`, RH8305-001 `D`).

### Test tags the amendment retires or moves

None. Existing RH8106-002/RH8306-004 tests have one reported element each and stay valid.

## thoenissen/Reihitsu#1092

Decision 8, option C: `sdd/specs/shared/element-documentation.md` restored to `cf6d26b` (`git diff cf6d26b` is empty for the file). This removes the SHARED-DOC-002 clarification, its example, the duplicate example under SHARED-DOC-009, the `file` modifier, and the `Uses` entry SHARED-DIAG-004 — every spec-diff defect the skeptic listed there. No test may tag SHARED-DOC-002 for the partial-type group; the characterization test asserts `main`'s output (RH8002 reports the second part) and tags no requirement for that case.

## thoenissen/Reihitsu#1153

No change in this round.

## thoenissen/Reihitsu#1617

### Changed requirements

- RH8201-001 — A documented member that inherits its documentation is reported — changed: a partial method, property, indexer, or event is judged part by part; each part whose own comment holds no `<inheritdoc>` is reported, whatever the other part carries; whether the member inherits is decided for the member as a whole by its symbol (SHARED-DIAG-004), so both parts inherit when the member is `override` or implements an interface member, whichever part the compiler maps. Coverage: the exclusion of partial members is replaced by per-part coverage. Why extended. New example (both parts of `M` reported; `P` defining part with `<inheritdoc/>` not reported, implementing part reported). `Uses` gains SHARED-DIAG-004.
- RH8201-004 — changed: one diagnostic per member, and for a partial member one per reported part.
- RH8201-005 — example added: the fix of each part replaces only that part's comment (Fix All result on `M`).
- RH8203-001 — changed: a partial method, property, indexer, or event is judged part by part; each part carrying `<inheritdoc>` is reported when the member inherits nothing; inheritance decided on the member as a whole. Coverage: only parts of partial types and partial constructors stay uncovered. Why extended. New example (`M` implementing `I.M`, `<inheritdoc/>` on both parts, not reported; `N` both parts reported). `Uses` gains SHARED-DIAG-004. RH8203-002 (one diagnostic per declaration) unchanged: each part is a declaration.

### Examples for the tests

RH8201-001 (partial `M`/`P`), RH8201-005 (partial fix example), RH8203-001 (partial `M`/`N`). Per the skeptic, test RH8201 and RH8203 for method, property, indexer, and event, documentation on the defining part, the implementing part, and both; the issue's own input stays a characterization test (nothing reported).

### Test tags the amendment retires or moves

- `VerifyNoDiagnosticForDocumentedImplementingPartialInterfaceImplementation` (`RH8201InheritdocShouldBeUsedAnalyzerTests.cs:2532`, untagged): asserts the opposite of the amended RH8201-001; it must report the implementing part and be tagged RH8201-001 (and RH8201-005 if it checks the fix).
- `VerifyDiagnosticForDocumentedDefiningPartialInterfaceImplementation` (`:2485`, untagged): still valid; tag RH8201-001.

## Output questions

_None._

## Internal choices

- Q4 B is stated in the rule specs (RH8201-001, RH8203-001), not by extending SHARED-DIAG-004's second paragraph to documentation comments, because RH8306 still reports a partial property once and would then contradict it. Alternative: extend SHARED-DIAG-004 and change RH8306 too (a visible change nobody asked for).
- RH8307 top-level `<b>` (an inline formatting element directly at the comment's top level) stays uncovered, as in 01; no new question, since the answer's "formatting tags are not reported at all" concerns nested tags and a top-level one is rare.
- RH8307 rule page updated (role step 4: what is reported changes in the normal case). Alternative: leave the page; then it would still promise checks on nested elements.
- One change doc for all three answers rather than one per issue: they share the cause (the batch-1 count/scope decisions on documentation rules) and the run.
- Decision 9: FMT-BLANK-010 states output only; the single-owner structure stays a follow-up (already in `00-lead.md`).

## Follow-up candidates

_None_ new. The RH8307 nested-element feature issue and the others are already in `00-lead.md`.

## Whole run

Files changed in this round: `sdd/changes/2026-11/documentation-rules-reported-elements.md` (new), `sdd/specs/analyzer/RH8106.md`, `RH8201.md`, `RH8203.md`, `RH8306.md`, `RH8307.md`, `sdd/specs/formatter/blank-lines.md`, `sdd/specs/shared/element-documentation.md` (reverted to base), `documentation/rules/RH8307.md`, and this handover. No code owner changed; `sdd/specs/README.md` unchanged.
