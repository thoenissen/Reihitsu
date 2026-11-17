Role: spec author
Commit: ed58ae5
Verdict: change doc written

Follow-up round after the maintainer's batch-1 answers (`00-lead.md`, *Maintainer answers*: 1A 2A 3A 4A 5A 6A) and the skeptic's handover `02-skeptic.md`. The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## What changed

- `sdd/changes/2026-11/report-across-conditional-directives.md`:
  - `Status: Accepted`.
  - *Decisions* gains *Maintainer answers (batch 1, 2026-11-17)*, one item per answer, including 6A: the conditional-directive-before-the-closing-brace shape is not declared unsupported, with what declaring would delete (the formatter's keep-the-comma check in its two trailing-comma transforms) and what it would refuse (common code).
  - *Decisions* gains *Two fixes that relied on their analyzer's silence withhold themselves*: the trailing-comma fix (RH5409–RH5411) and the RH8201 fix are now withheld by SHARED-TRAIL-002 and RH8201-003 themselves, Fix All included, and every other fix the change reaches refuses the new inputs under the requirement that names its withheld fix (SHARED-ORDER-003/007/008, RH5102-004, RH5109-004, RH5111-005, SHARED-MODORDER-003, SHARED-ATTR-019/020, SHARED-FIX-010, SHARED-EMPTY-004). This replaces the issue's "code fixes keep withholding as today".
  - *Visible output changes*, trailing commas: reported in both builds for the common shape (the comma behind `A` without `DEBUG`, the comma behind `B` with `DEBUG`), never with a fix, the formatter keeps both commas; the comma written inside the branch (`A` / `#if DEBUG` / `, B` / `#endif`) is the shape no build reports; RH5410/RH5411 behave the same.
  - *Visible output changes*, RH5111: states that `reihitsu-format` keeps the gap and that RH5111's SHARED-DIAG-010 exception now covers a directive of any kind.
  - *Alternatives considered*: the B options of questions 2–5 and escalation 10 are recorded as rejected by the maintainer.
- `sdd/specs/shared/trailing-commas.md`, SHARED-TRAIL-002: two examples added — the same enum in a build with `DEBUG` (comma behind `B` reported, no fix) and the comma-inside-the-branch shape (not reported in either build). The normative sentence already covered both; no wording change.
- `documentation/rules/general-notes.md`: decision 5 — the `#region`/`#endregion` sentence is back at the end of the first paragraph of *Code excluded by `#if`*, where "such a branch" refers to a branch whose condition is false.

Checked, no change needed:

- SHARED-DIAG-010 and RH5111-005 already state that the exception covers a comment or a preprocessor directive of any kind, conditional directives included (written in `3d85973`).
- RH8201-003 already requires the fix withheld for an implicit implementation when an `#if` appears in one of the named files.
- RH7311-001 already states answer 4A (compiled branch compared with regions outside; branches never compared with each other).
- Answers 1 and 5 match what the change doc and the specs already say; nothing else changes.

Checks: `scripts/apply-fix.sh RH5409` on the comma-inside-the-branch shape, with `#if !NEVER` (active) and `#if DEBUG` (excluded): `no-diagnostic` under LF and CRLF. The DEBUG-build example of SHARED-TRAIL-002 cannot run until the analyzer reports (apply-fix defines no symbols); it compiles. `scripts/test.sh --project architecture`: 185 passed. `scripts/test.sh --project analyzer --filter "FullyQualifiedName~RuleDocumentation"`: 7 passed. No requirement ID added or retired.

## Changed requirements

- SHARED-TRAIL-002 — A comma in front of a conditional directive is reported without a fix — clarified (examples only).

## Examples for the tests

SHARED-TRAIL-002 (both new examples, in addition to those listed in `01-spec-author.md`); the change doc's trailing-comma section for RH5410/RH5411 variants. The rest is unchanged from `01-spec-author.md`.

## Output questions

_None._

## Internal choices

- The "both builds" and clean-shape points are stated as examples under SHARED-TRAIL-002, not as a new requirement: the normative sentence already decides both. Alternative: a new requirement for the comma inside the branch.
- The withheld trailing-comma and RH8201 fixes are recorded as a decision in the change doc in behavior terms (which requirement withholds them), not as an implementation note about where the guard lives. Alternative: name the providers, which the change doc template does not ask for.

## Follow-up candidates

_None_ beyond those in `01-spec-author.md` (SHARED-USING-009) and `02-skeptic.md` (narrower trailing-comma guard for a comma in front of `#else`/`#elif`/`#endif`), which stand unchanged.
