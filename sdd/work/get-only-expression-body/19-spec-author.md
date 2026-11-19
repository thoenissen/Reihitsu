Role: spec author
Commit: a6256c7
Verdict: spec written

Repair round 1, spec checker finding 1 (18-spec-checker.md, both issues). My prompt carried no intended fix or hoped-for verdict beyond the worklist; I worked from 00-lead.md, 13-spec-author.md, 16-implementer.md, and 18-spec-checker.md.

## Changed requirements

None. Introduction of `sdd/specs/shared/get-only-expression-bodies.md` only.

- First tried what 13 planned: adding `ExpressionBodyCodeFixUtilities` and `BlankLineCodeFixUtilities` to the SHARED-GETONLY `Code owners` header and its `sdd/specs/README.md` row. `scripts/test.sh --project architecture` failed `SpecStructureTests.EveryTypeIsCodeOwnerOfAtMostOneSpec`: `ExpressionBodyCodeFixUtilities` already owns SHARED-BODY, `BlankLineCodeFixUtilities` already owns SHARED-STMT, and `sdd/README.md` (Code owners) allows a type to own at most one spec. 13's plan for the existing-owner case was wrong; reverted.
- Instead the header and the README row stay as they are (the three get-only types), and the introduction gains one sentence: the code-fix base hands two steps to shared helpers that stay owners of their own specs — `ExpressionBodyCodeFixUtilities` (SHARED-BODY) for the splice of SHARED-GETONLY-005, in both conversion directions, and `BlankLineCodeFixUtilities` (SHARED-STMT) for the shared-line decision, also across a comment, of SHARED-GETONLY-008 and SHARED-FIX-007. This is what `sdd/README.md` prescribes: the behavior is specified here, the type's owner spec is where its main behavior lives, and the caller spec names what it relies on.
- SHARED-BODY: no change. `ExpressionBodyCodeFixUtilities` is already its owner; specs name types, not methods, so `ReplaceBodyAsync` needs no mention, and SHARED-BODY-003's text is unchanged by the refactor (16: RH3202/RH3203/RH3208 results unchanged).

`scripts/test.sh --project architecture`: Passed 198, Failed 0.

## Examples for the tests

None new.

## Output questions

_None._

## Internal choices

- Owners left in their existing specs and named in the SHARED-GETONLY introduction, because the architecture test and `sdd/README.md` forbid a second owner spec (alternative: move `BlankLineCodeFixUtilities` out of SHARED-STMT, or split the new member into its own type — both larger than the finding and a code change).

## Follow-up candidates

_None._
