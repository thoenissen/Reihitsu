Role: spec author (amendment round)
Commit: 8e99390
Verdict: spec written

All changes are clarifications carrying out the maintainer's answers (Q1, Q1b, Q2, Q3, Q4) and lead decisions 4, 8, 9 (`00-lead.md`). No requirement ID was added, retired, or renumbered, so `requirement-ids.txt` and `sdd/specs/README.md` are unchanged; no change doc. `Uses` rows gained SHARED-DIAG-008 where a requirement now cites it (RH7105, RH7106, RH7107, RH7108). `scripts/test.sh --project architecture --filter "FullyQualifiedName~Spec|FullyQualifiedName~RuleDocumentation"` passes (37/37), and `scripts/test.sh --project analyzer --filter "FullyQualifiedName~RuleDocumentation"` passes (7/7: convention and example tests).

The prompt carried no intended fix or hoped-for verdict beyond the lead's decisions and the maintainer's answers.

## thoenissen/Reihitsu#1264

### Changed requirements

- RH7309-002 — A region after a region of a later category is reported — clarified (Q1-A, Q1b-A): the analyzer judges the regions as the active build configuration leaves them; a top-level region whose `#region` lies in an excluded `#if`/`#elif`/`#else` branch is neither reported nor counted as an earlier region; a region in the compiled branch is checked against the regions outside every branch and the earlier ones of its branch; so two regions of different branches are never compared (SHARED-TYPEREGION-006). Two examples: the Q1 input (not reported, with or without `DEBUG`) and the Q1b input (reported, no fix, where `DEBUG` is defined; not reported where it is not).
- RH7309-004 — The fix orders the type's regions by category — clarified: the blocks are all top-level regions, also one in an excluded branch; such a region is a block whose conditional directive lies outside it, so RH7309-005 withholds the fix (today's behavior, kept).
- RH7309-005 — new example: an excluded-branch region in front of two out-of-order regions, `DEBUG` undefined → `#region Fields` reported, no fix.
- Rule pages (decision 4): RH7308 — "top-level region" dropped; the sentence "Only members placed directly inside the region are considered" replaced by: a member is judged by the innermost region around it, checked against the nested region's label only, and not checked when that label is not recognized. RH7401–RH7404, RH7409–RH7412 — "top-level" dropped in *Description* and *How to fix it*, plus one sentence: "When regions are nested, the innermost region around the <member> is the one that counts." RH7405–RH7408 — "a top-level override region whose label is used by override <members> in the same type" replaced by "an override region: a region named after a base type that one of the type's overrides comes from", plus the same nesting sentence. Examples unchanged (they still match). RH7309's page keeps "top-level region categories": RH7309 still orders only top-level regions. Partial members (Q2) and several interfaces are corner cases and stay off the pages.

### Examples for the tests

RH7309-002 (two new examples; tests need `DEBUG` defined for the reported arm of Q1b — the analyzer test helpers must pass the preprocessor symbol, or the test writes `#if !NEVER` for the active branch), RH7309-005 (new excluded-branch example).

### Output questions

_None._

### Internal choices

- RH7309 fix blocks include regions in an excluded branch (wrote today's behavior). Alternative: blocks only from active regions, which would offer the fix in the new RH7309-005 example by leaving the whole `#if … #endif` in the code before the first moved block. That changes visible output (fix offered vs. withheld), so it would be an output question; I did not raise it because nothing in the run's issues asks for it and the decision keeps today's output.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1333

No change in this round.

## thoenissen/Reihitsu#1334

No text change in this round. Q2-A (reported once, on the part SHARED-DIAG-012 selects — the implementing part in the example; RH7405–RH7408: no part may lie in an override region) is what 187b246 already wrote in RH7401-001 … RH7412-001; verified, nothing contradicts it.

## thoenissen/Reihitsu#1339

### Changed requirements

- RH7102-005 — The fix moves the constant in front of the first field of its group — clarified (decision 9): only the fields the analyzer compares the constant with (RH7102-003) count as earlier fields — the same top-level region, also nested in it, or, for a constant outside every region, outside every region; a field of the group in another region, or outside every region while the constant lies in one, is never the target, also when it comes first. New example (skeptic's input): `_a` in `A`, `_b` and `K` in `B` → `K` moves in front of `_b`.
- RH7109-005 — The fix moves the field in front of the first field of its group — clarified the same way, with the matching example (`_c` moves in front of `_b`).

### Examples for the tests

RH7102-005 and RH7109-005 (new region examples, fix offered). Exact blank-line placement in the fixed output follows SHARED-ORDER-001/002; the test implementer confirms it against the shipped move.

### Output questions

_None._

### Internal choices

_None._

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1337

### Changed requirements

- RH5102-004 — clarified (decision 8, skeptic finding 1): the judged span is where each argument starts — the gaps between `(`, the arguments, the commas, and `)`. A conditional directive, or code it excludes, in a gap → not reported; one inside an argument (e.g. a lambda body) lies outside the span and still withholds only the fix; a comment or `#region`/`#pragma` in a gap withholds only the fix. Example extended with the skeptic's lambda case (`Run(1, 2, () => { #if DEBUG … #endif })`: reported, no fix — today's output kept).
- RH5109-004 — the same for the parameter list. Example extended with a conditional directive inside a default value (`int c = 1 #if DEBUG + 1 #endif + 2`): reported, no fix.
- RH7107-001, RH7108-001 — clarified (decision 8, skeptic finding 2): the judged span is the accessor list outside the accessors' bodies (in front of, between, and behind the accessors, attribute lists and keywords included); a conditional directive inside an accessor's block or expression body MUST NOT keep the accessor from being reported. New examples: `#if !NEVER` around `set`/`remove` → not reported; `#if DEBUG` inside the `set`/`remove` body → reported.
- RH7105-001, RH7106-001 — clarified (Q3-B, decision 13): "between its modifiers" is the judged span, from the first modifier through the last one; a conditional directive, or code it excludes, behind the last modifier lies outside it — reported, fix withheld (SHARED-MODORDER-003). New examples: the issue's `static #if FEATURE #endif public int Value` → not reported; the Q3 input (`static public #if DEBUG readonly #endif int _a;`, `DEBUG` undefined) → reported on `static`, no fix; RH7106 counterpart with `internal protected #if DEBUG static #endif int B;`.
- SHARED-MODORDER-003 — unchanged; keeps "a conditional directive included". Checked 187b246: it did not touch RH7105, RH7106, or SHARED-MODORDER-003, and nothing in it contradicts Q3-B.
- RH5111-005 — unchanged; Q4-A (directive inside the value: reported, no fix) is what 187b246 wrote.

### Examples for the tests

RH5102-004, RH5109-004, RH7105-001, RH7106-001, RH7107-001, RH7108-001 (new or extended examples). Note for the test implementer: the RH7105/RH7106 "behind the last modifier" examples are pinned with the symbol undefined; with `DEBUG` defined the `#if` stands between `public` and `readonly`, inside the judged span, and the declaration is not reported — consistent with the text, not written as an example.

### Output questions

_None._

### Internal choices

- RH5109-004's inside-a-parameter example uses a default value, the one place a parameter can hold a conditional directive followed by more of the same parameter. Alternative: an attribute argument; same verdict.

### Follow-up candidates

_None_ beyond those the lead already holds (RH5101-004 wording; the ~60 rule specs without SHARED-DIAG-008).

## thoenissen/Reihitsu#1412

No change in this round.

## thoenissen/Reihitsu#1592

No change in this round.

## Whole run

Files changed: `sdd/specs/analyzer/RH7309.md`, `RH7105.md`, `RH7106.md`, `RH7107.md`, `RH7108.md`, `RH5102.md`, `RH5109.md`, `RH7102.md`, `RH7109.md`; `documentation/rules/RH7308.md`, `RH7401.md` … `RH7412.md`; this handover.
