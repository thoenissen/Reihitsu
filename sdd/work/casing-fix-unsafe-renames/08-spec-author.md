Role: spec author
Commit: 65c5ac8
Verdict: spec written

Repair round 1, Finding 1 of `07-spec-checker.md`. No prompt-supplied fix was followed; the wording is re-derived from the finding, SHARED-FIX-003 (first condition: a compile error the code did not have), and the test of lead decision 12.

## Changed requirements
- SHARED-CASING-009 — The fix is withheld when the new name would collide — clarified (ID kept, `sdd/specs/shared/casing.md`):
  - The first bullet now applies only "where that declaration is a compile error", so it no longer reads as withholding every rename after which a member has the type's name. It stays a refinement of SHARED-FIX-003's compile-error condition and adds no withholding of its own.
  - The enum sentence becomes a general statement of the bullet's negative side: an enum member may have its enum's name, and a method of an interface its interface's name; neither collides, and the fix is offered.
  - New example "RH4005, for a method of an interface declared directly in a namespace", with before and after identical to `RH4005InterfaceNameCasingAnalyzerTests.FixRenamesANamespaceInterfaceToTheNameOfOneOfItsMethods`.

## Examples for the tests
SHARED-CASING-009 — the new RH4005 example is already covered by `RH4005InterfaceNameCasingAnalyzerTests.FixRenamesANamespaceInterfaceToTheNameOfOneOfItsMethods` (tagged SHARED-CASING-009, SHARED-FIX-003). No new test is needed.

## Output questions
_None._

## Internal choices
- Qualified the bullet by the compile-error condition (the finding's second option) and named only the interface method as an exception, rather than listing interface members in general. Alternative: "an interface member or nested type may have the interface's name". Rejected because only the method case is proven by a test; other interface member kinds (for example static members, nested types) were not compiled in this run, and the compile-error qualifier decides them without the spec asserting either way.

## Follow-up candidates
_None._
