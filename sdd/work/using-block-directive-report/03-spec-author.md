Role: spec author
Commit: bd5a53e
Verdict: spec written

Follow-up round: the maintainer's answers (1A, 2A, 3A, escalation 10 A — not declared) and the lead's decisions 1 and 2 are carried into the change doc and SHARED-USING-009. The change doc is `Status: Accepted`. My prompt carried no intended fix or hoped-for verdict beyond routing the recorded answers.

Change doc: `sdd/changes/2026-11/using-block-directive-report.md`.

## Changed requirements

- SHARED-USING-009 — Blocks with preprocessor directives are not reordered — changed (wording only, no change in behavior from 01): the reason for RH7207's not checking blank lines now says the using phase does not lay out the group boundaries of such a block (FMT-USING-004) and other formatter requirements write blank lines there (FMT-BLANK-013), so the check would report the formatter's own output (SHARED-DIAG-010); the Why replaces "the order is content" with the SHARED-DIAG-008 basis ("a directive keeps the fix and the formatter away, not the rule"). `Uses` gains FMT-BLANK-013. The two example lead-ins of the "directive after the block" example get their colons.

No other requirement changes. RH7201-001 to RH7207-001 and -004 stay as written in 1db90d1.

Change doc: Status Accepted; Why rebased on SHARED-DIAG-008 and the trailing-comma / attribute-shape precedent, with a paragraph saying why the basis is not SHARED-DIAG-010's order exemption; case 5 notes that the DEBUG-build report covers the formatter's own output (skeptic case 1a); new case 6, `#if`/`#else` alternatives in a `NETFRAMEWORK` build (skeptic case 1b), before confirmed `no-diagnostic` with `scripts/apply-fix.sh RH7207` and the using block kept by `scripts/format.sh`; Decisions gains "Maintainer answers (2026-11-27)" with the four answers, and the RH7207 decision states the lead's decision 1 with the formatter fact (e.cs) and that it was decided in the run; Alternatives mark each B as rejected by the maintainer and add the unsupported-shape alternative.

`scripts/test.sh --project architecture`: 198 passed.

## Examples for the tests

Unchanged from 01: SHARED-USING-009 (all examples), RH7201-001 to RH7207-001, RH7201-004 to RH7207-004. Additionally the change doc's case 6 (`#if`/`#else`, RH7207 only, in the build that defines `NETFRAMEWORK`; not reported in the other build) — the spec states it through RH7207-001 and SHARED-USING-009 without a dedicated example; the test implementer may derive one test from it.

## Output questions

_None._ All answered.

## Internal choices

- Applied both skeptic wording recommendations (basis of the report; the blank-line statement) in SHARED-USING-009 and the change doc, as decision 2 asks.
- FMT-USING-004's own text ("MUST keep … their blank lines") left unchanged: the lead holds its scoping against FMT-BLANK-013 as a follow-up candidate, and SHARED-USING-009 no longer relies on that sentence. Alternative: rescope it in this run (wording only, formatter spec).
- Case 6 added to the change doc rather than as a new SHARED-USING-009 example, keeping the requirement's examples to one per position.

## Follow-up candidates

_None new._ The two in `00-lead.md` stand (fix and formatter for a block whose every directive precedes its first using; FMT-USING-004 vs FMT-BLANK-013 wording).
