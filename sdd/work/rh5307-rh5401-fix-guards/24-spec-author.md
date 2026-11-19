Role: spec author
Commit: 5ec47d8
Verdict: change doc written

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

Scope: finding 1 of `23-spec-checker.md` only.

`sdd/changes/2026-11/rh5307-rh5401-fix-the-reported-construct.md`, *Visible output changes*, block "RH5401 · a property with attribute lists": example `Y` added to the input, the before line, the after line, and the fixed output, so the block now lists A, B, L, E, J, V, and Y. `Y` is the same text as in the RH5401-003 offered example (`[Obsolete("a", // c⏎ false)]⏎int Y⏎    => 1;` → `... int Y => 1;`). On `main` every property with attribute lists whose lists take lines of their own got no fix (checker, check 1), so `Y` moves from no fix to a fix like `J` and `V`.

Verification: `scripts/apply-fix.sh RH5401` on a fixture holding exactly the change doc's input gives, under LF and CRLF, `fixed (1 action(s), 7 iteration(s))` with output identical to the after block, `Y` included (`int Y => 1;` with the attribute list and its line comment unchanged).

No spec, rule page, requirement ID, or code owner changed; no architecture run needed.

## Changed requirements
_None._ (Change doc only.)

## Examples for the tests
_None new._ `Y` is already covered by `VerifyFixKeepsALineCommentBehindCodeInsideTheArgumentsOfAnAttribute` (RH5401-003, RH5401-004).

## Output questions
_None._

## Internal choices
_None._

## Follow-up candidates
_None._
