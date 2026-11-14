Role: spec author (correction round)
Commit: 7a34ced
Verdict: spec written

Carries out lead decisions 15 and 16 (`00-lead.md`). Clarifications only: no requirement ID added, retired, or renumbered, so `requirement-ids.txt` and `sdd/specs/README.md` are unchanged; no change doc; no rule page touched (neither change alters what is reported or how to fix it in the normal case). `scripts/test.sh --project architecture --filter "FullyQualifiedName~Spec"` passes (37/37). The prompt carried no intended fix or hoped-for verdict beyond the lead's decisions.

## thoenissen/Reihitsu#1264

### Changed requirements

- RH7311-001 — A repeated description is reported — clarified (decision 16): a region lies in a branch only when the `#if`, `#elif`, or `#else` that starts the branch lies inside the declaration; a conditional directive that encloses the whole declaration does not put its regions in a branch, and they are compared. Why extended (a whole-declaration `#if`, common for target-framework-specific types, leaves one outline per build). New example: `#if !NEVER` around `class C` with two `#region Methods` → the second is reported.

### Examples for the tests

RH7311-001 (new whole-declaration example). The example uses `#if !NEVER` so the branch is active without a preprocessor symbol in the test helpers.

### Output questions

_None._

### Internal choices

- "Inside the declaration" rather than "between its `{` and `}`": matches the implementer's scope walk (directives back to the type declaration's start); the two differ only for an `#if` between the type's header and its `{`, which would split the type's braces across a branch. Alternative: the brace-bounded wording.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1333

No change in this round.

## thoenissen/Reihitsu#1334

No change in this round.

## thoenissen/Reihitsu#1339

### Changed requirements

- RH7102-005 — The fix moves the constant in front of the first field of its group — clarified (decision 15): the region example now opens region `B` with `public int B;` (another accessibility group), so the target `_b` is not the first member of its region and no unpaired `#region` lies in the crossed span. The intro sentence states that, were `_b` the first member, the `#region B` line would lie in the crossed span and SHARED-ORDER-004 would withhold the fix (RH7102-006). Fixed output: `public int B; private const int K = 1; private int _b;` inside `B`.
- RH7109-005 — The fix moves the field in front of the first field of its group — clarified the same way: `public int B;` first in region `B`; fixed output `public int B; private readonly int _c; private int _b;`.
- SHARED-ORDER-004 — unchanged.

### Examples for the tests

RH7102-005 and RH7109-005 (rewritten region examples, fix offered). The SHARED-ORDER-004 shape (target first in its region, `#region B` in the crossed span) is reported with no fix, as SHARED-ORDER-004's own example and RH7102-006/RH7109-006 already state; the test implementer pins it per decision 15.

### Output questions

_None._

### Internal choices

_None._

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1337

No change in this round.

## thoenissen/Reihitsu#1412

No change in this round.

## thoenissen/Reihitsu#1592

No change in this round.
