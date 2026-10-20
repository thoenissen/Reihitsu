Role: spec author
Commit: da17518
Verdict: spec written

This round applies the maintainer's answer in `squad/decisions/2026-10-20-997-test-findings-spec-corrections.md` to the two findings in `09-test-implementer.md`. Nothing else changed.

## Re-derived

- **SHARED-BODY-001, third example.** `ExpressionBodyAnalyzerBase<TNode>`, like the private copy in RH3203, reports `ExpressionBody.GetLocation()`. That span excludes trivia. With `DEBUG` undefined, `#if DEBUG` / `+ 1` / `#endif` is trailing trivia of `value`, so the span is `=> _value = value`. This matches `VerifyExpressionBodyWithBalancedGroupBeforeSemicolonIsFixed` and SHARED-BODY-002.
- **RH3103-003.** `RH3103UseShorthandForNullableTypesAnalyzer` widens the location to the parent only when the parent is a `QualifiedNameSyntax` whose `Right` is the generic name. For an `AliasQualifiedNameSyntax` parent such as `S::Nullable<int>`, it reports the generic name alone (`AliasQualifiedNullableGenericInTypeofIsReportedAndFixed`). `global::System.Nullable<long>` is a dotted qualified name and is covered whole.

## Changed requirements

- **SHARED-BODY-001 (example only).** The balanced-group example now marks `{|RH3203:=> _value = value|}`; the directives and `;` stand after the markup. The fixed output is unchanged.
- **RH3103-003 (narrowed).** The whole name is reported when the generic name is the part after the last `.` of a dotted qualified name, such as `System.Nullable<int>` or `global::System.Nullable<int>`. The requirement does not cover a name whose only qualifier is an alias, such as `S::Nullable<int>`. That test stays untagged for RH3103-003, as the test implementer already left it.

Not changed, per the instruction to change nothing else: RH3103-004 says "replace it" for the reported name. For `S::Nullable<int>`, the fix replaces the whole alias-qualified name. The existing test, tagged RH3103-001/-004, expects exactly that and passes.

## Checks

- `scripts/test.sh --project architecture`: 72/72 passed.
- `scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH3103|FullyQualifiedName~RH3203"`: 48/48 passed.

No test needed a change.

## Open questions to file

_None new._ The final list is in `08-spec-author.md`.
