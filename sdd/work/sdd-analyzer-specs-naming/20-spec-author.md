Role: spec author
Commit: 86bf40be25e24d95e2c8323824460c4f6d91bd73
Verdict: text written

## What changed

This is a text cleanup for findings N1 and N2 of `19-auditor.md`:

- one sentence changed in `sdd/specs/shared/casing.md`;
- one sentence added in `sdd/specs/analyzer/RH4106.md`.

No example, requirement ID, ledger line, code owner, or header row changed. RH4106's `Uses` already lists SHARED-CASING-007.

`scripts/test.sh --project architecture --filter "FullyQualifiedName~Sdd"`: 39 passed, 0 failed.

## Re-derivation

I ran both findings against the shipped analyzer and fix, using my scratch harness outside the repository.

**N1 reproduced.**

- RH4002 renames `class _ßx` to `class ßx` and `class ß_y` to `class ßy`. Both are still reported, and no further fix is offered.
- Control: RH4106 renames `ß_x` to `_ßX`, which is corrected.
- `ß` has no one-character uppercase mapping: `char.ToUpperInvariant('ß')` is `ß`. So the boundary "a letter that has both an uppercase and a lowercase form" left it undecided.

**N2 confirmed by reading.** RH4106-004 was the only casing fix requirement without the inherited coverage sentence.

## Changed requirements

**SHARED-CASING-006 — clarified (N1).** The coverage sentence now reads:

> This requirement covers an old name whose first word, the first non-empty part between underscores, starts with a letter whose one-character uppercase and lowercase mappings, in the invariant culture, differ from each other. A first word that starts with a digit, with a letter without case such as `注`, or with a letter such as `ß` that has no one-character uppercase mapping, is not covered.

Every example of SHARED-CASING-006 stays inside the coverage. Each starts with `F`, `L`, `m`, `d`, `U`, `i`, `M`, `r`, `t` or `I`, whose mappings differ.

**RH4106-004 — clarified (N2).** The requirement now ends with:

> This requirement covers the renames that SHARED-CASING-007 covers.

That is the same sentence every sibling fix requirement carries. It keeps `__1st` → `_1st` (thoenissen/Reihitsu#1016) out of RH4106-004.

RH4106-004 keeps its meaning for the retag table in `06-spec-author.md`. No tag changes.

## Examples for the tests

None changed.

## Open questions to file

_None._ The `_ßx` input belongs to thoenissen/Reihitsu#1016, which the lead extends.

## Decisions needed

_None._
