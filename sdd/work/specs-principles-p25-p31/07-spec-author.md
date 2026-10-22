Role: spec author
Commit: 304af06
Verdict: spec written

Inputs: `06-spec-checker.md` (findings 1 and 2), the current RH7104, RH8401, RH8402, RH8501 specs, `documentation/rules/RH7104.md`, and the RH7104 analyzer tests. I re-derived both findings against SHARED-DIAG-004 and SHARED-DOC-008 as they stand at `304af06`. No ID was renumbered or retired.

## Changed requirements

- RH7104-001 — A partial type without accessibility modifier is reported — clarified. The restriction "when the declaration is the type's only declaration" is removed. Each declaration is now judged on its own, also when another declaration of the type states the accessibility (SHARED-DIAG-004). This is today's behavior: `PartThatOmitsTheAccessibilityIsReportedAndFixedAloneWhenAnotherPartDeclaresIt` and the untagged `PartialTypesWithPublicAndCompoundAccessibilityAreFixed` both report the omitting part.
- RH7104-004 — title "The fix adds the type's default accessibility" became "The fix adds the type's accessibility" — clarified.
  - The fix inserts the accessibility the type has, in front of the reported declaration's first modifier, and changes no other declaration. That is the accessibility another declaration states, or, when none states one, the default (`internal` / `private` / `public` as before).
  - The trivia and attribute sentence is unchanged. The Why is extended.
  - This matches the code, which writes the symbol's `DeclaredAccessibility`. The untagged `PartialTypesWithPublicAndCompoundAccessibilityAreFixed` shows `public` and `protected internal` taken from another part. Its empty bodies also collapse to `;`, which is the P14d shape of thoenissen/Reihitsu#1335 and not part of this clause.
  - The rule page `documentation/rules/RH7104.md` already says "Add the correct access modifier" and needs no change.
- RH7104 `Uses`: `—` → `SHARED-DIAG-004`.
- RH8401-001 — clarified. After its existing coverage sentence, it now says the rule's subject is an ordinary comment, so SHARED-DOC-008 requires it to check every documentation mode, and that what it reports in mode `None` is not covered here. I kept the scope wording rather than stating behavior in mode `None`, which is non-conforming (thoenissen/Reihitsu#1346). `Uses`: + SHARED-DOC-008.
- RH8402-003 — clarified the same way. The rule's subject is the text of the file, and what it reports in mode `None` is not covered here (#1346). `Uses`: + SHARED-DOC-008.
- RH8501-001 — clarified. The rule reads no documentation comment, so it checks syntax trees in every documentation mode, `None` included (SHARED-DOC-008). This conforms today: the existing SHARED-DOC-008 RH8501 test reports in mode `None`. `Uses`: `—` → `SHARED-DOC-008`.

`scripts/test.sh --project architecture`: Passed, 72 of 72.

## Examples for the tests

- RH7104-001 and RH7104-004 (new clauses): the shape of `PartThatOmitsTheAccessibilityIsReportedAndFixedAloneWhenAnotherPartDeclaresIt` (members in the bodies, another part `public`) covers both. Suggestion for the lead: add `[Requirement("RH7104-001")]` and `[Requirement("RH7104-004")]` to that test, or add one focused test per clause. RH7104-001 and RH7104-004 are already covered by other tests, so coverage does not fail either way. A `protected internal` variant with member bodies would pin the "states" side for a compound accessibility. Do not pin empty bodies (#1335).
- RH8501-001 (every-mode sentence): the existing RH8501 mode-`None` test tagged SHARED-DOC-008. It may also be tagged RH8501-001.
- RH8401-001 and RH8402-003: no new normative behavior to pin (scope sentences only). Do not pin mode `None` (#1346).

## Open questions to file

_None._
