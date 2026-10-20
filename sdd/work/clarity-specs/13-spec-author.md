Role: spec author
Commit: df2edbf
Verdict: spec written

Repair round for the two spec-author findings in `12-spec-checker.md`. Nothing else changed.

## Re-derived

- **RH3103.** `RH3103UseShorthandForNullableTypesCodeFixProvider.TryGetTypeSyntax` maps a reported generic name whose parent is an `AliasQualifiedNameSyntax` (`S::Nullable<int>`) or a `QualifiedNameSyntax` to that parent. The fix therefore replaces the whole name, qualifier included. This matches `AliasQualifiedNullableGenericInTypeofIsReportedAndFixed`, which expects `typeof(int?)`. The analyzer reports `S::Nullable<int>` on `Nullable<int>` only (RH3103-003).
- **RH3201-001.** The sentence that named the excluded case stated no behavior. The case it pointed at (#1235 Q1) is now simply outside the MUST's condition.

## Changed requirements

- **RH3103-001.** "alias-qualified (`global::System.Nullable<int>`)" now reads "qualified with dots (`System.Nullable<int>`, also behind `global::` as in `global::System.Nullable<int>`), or qualified by an alias alone (`S::Nullable<int>` with `using S = System;`)". The term "alias-qualified" no longer appears in the spec. Each form now has one name, used the same way in -001, -003, and -004.
- **RH3103-003.** The exclusion now reads "a name qualified by an alias alone, such as `S::Nullable<int>`". The behavior is unchanged.
- **RH3103-004.** The fix replaces the reported name "together with any qualifier in front of it — `System.`, `global::System.`, or an alias such as `S::` —" with the nullable type.
- **RH3201-001.** The MUST now applies "when the line above the comment or the line below it does not start with a comment". The sentence naming the excluded `//`/`///`/`/*` neighbor case and pointing to RH3201-002 is gone. The example's comments all meet the condition.

## Checks

- `scripts/test.sh --project architecture`: 72/72 passed.
- `scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH3103|FullyQualifiedName~RH3201"`: 61/61 passed.

No test changed. Existing tags still fit: `AliasQualifiedNullableGenericInTypeofIsReportedAndFixed` carries RH3103-001/-004, which now state its behavior explicitly.

## Open questions to file

_None new._
