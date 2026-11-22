Role: spec author
Commit: a368d20
Verdict: spec written

Repair round for finding 1 of `08-spec-checker.md`. The prompt carried no intended fix, suspected cause, or hoped-for verdict.

Decision: `CasingRenameSafetyUtilities` is an owner of the spec. It is not new in this run (added before it, in the change that enabled RH2008/RH2111, by splitting checks out of `CasingCodeFixProviderBase<T>`), but it holds the checks that decide whether the casing fix is offered: the compiler-lookup method names of SHARED-CASING-011 (`IsCompilerLookupMethodRename`), the caller-argument expressions of SHARED-CASING-012 (`HasCallerArgumentExpressionNaming`), the verbatim-prefix guard of SHARED-CASING-013 added in this run (`WritesNameAsKeywordAsync`), and the rename cascade and the checks that the rename reaches only the declaration and its references (SHARED-CASING-010 coverage). The listing was missing before the run as well; the run's new guard makes it due now.

Edit: `CasingRenameSafetyUtilities` added after `CasingCodeFixProviderBase<T>` in the `Code owners` row of `sdd/specs/shared/casing.md` and in the SHARED-CASING row of `sdd/specs/README.md`. No requirement text changed.

`scripts/test.sh --project architecture`: `Passed! - Failed: 0, Passed: 198, Skipped: 0, Total: 198`.

## Changed requirements
_None._ (code-owner lists only)

## Examples for the tests
_None._

## Output questions
_None._

## Internal choices
- Listed `CasingRenameSafetyUtilities` as a code owner of SHARED-CASING rather than leaving it out as a helper of `CasingCodeFixProviderBase<T>`: it carries the guards of SHARED-CASING-011, -012, and -013 itself.

## Follow-up candidates
_None._
