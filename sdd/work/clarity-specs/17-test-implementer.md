Role: test implementer
Commit: bb73e73
Verdict: tests written

This round applies the re-tagging list in `16-spec-author.md`, which follows `squad/decisions/2026-10-20-997-shared-prefix-collision.md`. The lead's prompt carried no intended fix or hoped-for verdict. Only requirement tags changed. No assertion and no production code was touched.

## Tests

| Test class | Change | Methods |
|---|---|---|
| `Design/RH3106UnnecessaryDelegateParenthesesShouldBeRemovedAnalyzerTests` | SHARED-EMPTY-001 → SHARED-PARENS-001, SHARED-EMPTY-002 → SHARED-PARENS-002 | `VerifyDiagnosticAndCodeFix`, `EmptyParameterListsOfAnonymousMethodsAreReportedOverTheParentheses`, `FixRemovesTheEmptyParameterList` |
| `Design/RH3107UnnecessaryAttributeConstructorParenthesesShouldBeRemovedAnalyzerTests` | the same | `VerifyDiagnosticsAndCodeFix`, `EmptyArgumentListsOfAttributesAreReportedOverTheParentheses`, `FixRemovesTheEmptyArgumentListAndKeepsACommentInFrontOfIt` |
| `Formatting/RH3208ExpressionStyleLocalFunctionsShouldNotBeUsedAnalyzerTests` | SHARED-BRACE-001 → SHARED-AFTERBRACE-001 | the 4 methods listed in 16 |
| `Formatting/RH5030BlankLineAfterClosingBraceAnalyzerTests` | SHARED-BRACE-001 → SHARED-AFTERBRACE-001 | the 11 methods listed in 16 |

Before the change, these four files held exactly 23 SHARED-EMPTY/SHARED-BRACE tags: 4 in RH3106, 4 in RH3107, 4 in RH3208, and 11 in RH5030. They are the 23 that 16 lists, so the file-scoped replacement touched nothing else. None of these four files carries a SHARED-EMPTY or SHARED-BRACE tag any more. `main`'s SHARED-EMPTY tags (RH5412–RH5416) and SHARED-BRACE tags (RH5405–RH5407) are unchanged.

## Not covered, and why

Nothing new: this round only renames tags. Findings 1–3 of `09-test-implementer.md` still stand.

## Commands

```shell
scripts/test.sh --project architecture
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Design.RH3106|FullyQualifiedName~Design.RH3107|FullyQualifiedName~Formatting.RH3208|FullyQualifiedName~Formatting.RH5030"
```

Results: architecture 72/72 passed; the four classes 88/88 passed.
