Role: test implementer
Commit: 49f88f0
Verdict: tests written

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Lane: behavior change, so the tests come from the spec examples (SHARED-EMPTY-001, -002, -005; FMT-STRUCT-017, -018), the change doc's *Visible output changes*, and the skeptic's candidate table (02-skeptic.md).

Every new test that asserts changed behavior fails on 49f88f0 for the right reason, which I checked in the test output. Analyzer tests fail with "expected N diagnostics, actual 0". Formatter tests fail because the output keeps `{ }` plus `;`. The RH5404 test fails because the fix output keeps `class I⏎{⏎};`. The tests marked "pass" in the table pass now and must stay green. `scripts/test.sh --project architecture` passes 198 of 198, so SHARED-EMPTY-005 now counts as covered.

## Tests

Short names. Files:
- Analyzer tests: `tests/Reihitsu.Analyzer.Test/Formatting/<Rule>…AnalyzerTests.cs`.
- Analyzer–formatter parity test: `tests/Reihitsu.Analyzer.Test/Formatter/Formatting/RH5412…FormatterTests.cs`.
- Formatter tests: `tests/Reihitsu.Formatter.Test/Regression/Structural/`. `EmptyTypeDeclarationTrailingSemicolonTests.cs` is a new file.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH5412 `VerifySpecificationExampleOfEmptyBodiesIsReported` (extended with examples `F` and `G`; count 4 → 6) | SHARED-EMPTY-001, RH5412-001 | `Verify` | fail (4 of 6) |
| RH5412 `VerifySpecificationExampleOfClassWithMemberAndSemicolonDeclarationIsNotReported` (extended with `class C { int _x; };`) | SHARED-EMPTY-002 | `Verify` | pass |
| RH5412 `VerifySpecificationExampleOfSemicolonBehindTheBracesIsFixed` (LF, CRLF) | SHARED-EMPTY-001, SHARED-EMPTY-005 | `Verify` + `ConvertEndOfLine` (fix one, iterative, Fix All) | fail |
| RH5412 `VerifySemicolonOnTheLineBelowTheClosingBraceIsFixedWithTheDocumentLineBreaks` (LF, CRLF) | SHARED-EMPTY-005; FC-010/FC-033 | `VerifyCodeFixWithEndOfLine` (raw text, no normalization) | fail |
| RH5412 `VerifyWhitespaceAndBlankLineBetweenClosingBraceAndSemicolonAreRemoved` | SHARED-EMPTY-005; FC-003 (whitespace side of the boundary) | `Verify` | fail |
| RH5412 `VerifyBlockCommentBehindTheSemicolonFollowsTheNewSemicolon` | SHARED-EMPTY-005; FC-004 | `Verify` | fail |
| RH5412 `VerifyNestedEmptyClassWithSemicolonBehindTheBracesIsFixed` | SHARED-EMPTY-001, SHARED-EMPTY-005; FC-027 | `Verify` | fail |
| RH5412 `VerifyTwoEmptyClassesWithSemicolonsOnOneLineAreFixed` (`class A { }; class B { };` → `class A;⏎class B;`) | SHARED-EMPTY-005; FC-026, FC-008 | `Verify` (Fix All and iterative) | fail |
| RH5412 `VerifySpecificationExampleOfCommentBetweenClosingBraceAndSemicolonIsReportedWithoutCodeFix` | SHARED-EMPTY-001, SHARED-EMPTY-004, SHARED-EMPTY-005 | `VerifyNoCodeFixIsOffered` | fail |
| RH5412 `VerifyCommentOnItsOwnLineBetweenClosingBraceAndSemicolonIsReportedWithoutCodeFix` (`}⏎// note⏎;`) | SHARED-EMPTY-005; FC-002, FC-019 (leading trivia of `;`) | `VerifyNoCodeFixIsOffered` | fail |
| RH5412 `VerifyDirectiveBetweenClosingBraceAndSemicolonIsReportedWithoutCodeFix` (`#region`/`#endregion`, empty `#if X`/`#endif`) | SHARED-EMPTY-005; FC-005, FC-020 | `VerifyNoCodeFixIsOffered` | fail |
| RH5412 `VerifyDisabledTextBetweenClosingBraceAndSemicolonIsReportedWithoutCodeFix` | SHARED-EMPTY-005; FC-020 | `VerifyNoCodeFixIsOffered` | fail |
| RH5412 `VerifyDocumentationCommentBetweenClosingBraceAndSemicolonIsReportedWithoutCodeFix` (None, Parse, Diagnose) | SHARED-EMPTY-005; FC-040 | `VerifyNoCodeFixIsOffered(source, DocumentationMode)` | fail |
| RH5412 `VerifyEmptyClassWithSemicolonBehindTheBracesIsNotReportedUnderCSharp11` | RH5412-001; FC-006 (gate, below) | `Verify` | pass |
| RH5412 `VerifyEmptyClassWithSemicolonBehindTheBracesIsReportedUnderCSharp12` | RH5412-001; FC-006 (gate, at) | `Verify` | fail |
| RH5413 `VerifyEmptyStructFollowedByASemicolonIsReportedAndFixed` (LF, CRLF) | SHARED-EMPTY-001, SHARED-EMPTY-005, RH5413-001 | `Verify` + `ConvertEndOfLine` | fail |
| RH5414 `VerifyEmptyInterfaceFollowedByASemicolonIsReportedAndFixed` (LF, CRLF) | SHARED-EMPTY-001, SHARED-EMPTY-005, RH5414-001 | `Verify` + `ConvertEndOfLine` | fail |
| RH5415 `VerifyEmptyRecordFollowedByASemicolonIsReportedAndFixed` (LF, CRLF; `record R(int Value)` and `record class Q`) | SHARED-EMPTY-001, SHARED-EMPTY-005, RH5415-001 | `Verify` + `ConvertEndOfLine` | fail |
| RH5416 `VerifyEmptyRecordStructFollowedByASemicolonIsReportedAndFixed` (LF, CRLF) | SHARED-EMPTY-001, SHARED-EMPTY-005, RH5416-001 | `Verify` + `ConvertEndOfLine` | fail |
| RH5418 `UnionWithBracesAndSemicolonIsReportedAndFixed` (LF, CRLF). Inverted from `UnionWithBracesAndSemicolonIsNotReported`, which pinned the old output | SHARED-EMPTY-001, SHARED-EMPTY-005, RH5418-001 | `Verify` + `ConvertEndOfLine` | fail |
| RH5418 `UnionWithCommentBetweenClosingBraceAndSemicolonIsReportedWithoutFix` | SHARED-EMPTY-005 | `Verify(src, src)` | fail |
| RH5404 `VerifyNestedEmptyTypeFollowedByASemicolonConvergesToSemicolonDeclaration` (LF, CRLF; the change doc's RH5404 block) | RH5404-003; FC-030 | `VerifyCodeFixWithEndOfLine` | fail |
| RH5412 formatter parity `VerifyFormatterFixesEmptyClassFollowedByASemicolon` | FMT-STRUCT-017; FC-013 | `FormatterTestsBase<TAnalyzer>.VerifyFormatter` | fail |
| `StructuralTransformRequirementTests.EmptyTypeBodyFollowedByASemicolonBecomesOneSemicolon` (the FMT-STRUCT-017 example) | FMT-STRUCT-017 | `AssertRuleResult` (LF, CRLF, second pass, phase stability) | fail |
| `StructuralTransformRequirementTests.EmptyTypeBodyWithCommentOrDirectiveInsideKeepsItsBraces` (extended with the `Behind` example) | FMT-STRUCT-018 | `AssertRuleResult(input)` | pass |
| `UnionDeclarationStructuralTests.EmptyUnionBodyFollowedByASemicolonBecomesOneSemicolon` | FMT-STRUCT-017 | `AssertRuleResult` (C# 15) | fail |
| `EmptyTypeDeclarationTrailingSemicolonTests.EveryEmptyTypeKindFollowedByASemicolonBecomesOneSemicolon` (struct, interface, record class, positional record, record struct) | FMT-STRUCT-017 | `AssertRuleResult` | fail |
| `…WhitespaceAndBlankLineBetweenClosingBraceAndSemicolonAreRemoved` | FMT-STRUCT-017; FC-003 | `AssertRuleResult` | fail |
| `…BlockCommentBehindTheSemicolonFollowsTheNewSemicolon` | FMT-STRUCT-017; FC-004 | `AssertRuleResult` | fail |
| `…NestedEmptyClassFollowedByASemicolonBecomesOneSemicolon` | FMT-STRUCT-017; FC-027 | `AssertRuleResult` | fail |
| `…TwoEmptyClassesFollowedBySemicolonsOnOneLineBecomeOneSemicolonEach` | FMT-STRUCT-017; FC-026 | `AssertRuleResult` | fail |
| `…EmptyClassFollowedByASemicolonKeepsItsBracesUnderCSharp11` | FMT-STRUCT-017; FC-006 (below gate) | `AssertRuleResult(input, null, C# 11)` | pass |
| `…EmptyRecordFollowedByASemicolonBecomesOneSemicolonUnderCSharp11` | FMT-STRUCT-017; FC-006 (record gate C# 9) | `AssertRuleResult` (C# 11) | fail |
| `…LineCommentBetweenClosingBraceAndSemicolonKeepsTheBraces` | FMT-STRUCT-018; FC-019 | private `AssertBracesAndTriviaAreKept` | pass |
| `…CommentOnItsOwnLineBetweenClosingBraceAndSemicolonKeepsTheBraces` | FMT-STRUCT-018; FC-019 | `AssertBracesAndTriviaAreKept` | pass |
| `…DirectiveBetweenClosingBraceAndSemicolonKeepsTheBraces` | FMT-STRUCT-018; FC-005, FC-020 | `AssertBracesAndTriviaAreKept` | pass |
| `…DisabledTextBetweenClosingBraceAndSemicolonKeepsTheBraces` | FMT-STRUCT-018; FC-020 | `AssertBracesAndTriviaAreKept` | pass |
| `…DocumentationCommentBetweenClosingBraceAndSemicolonKeepsTheBraces` (None, Parse, Diagnose) | FMT-STRUCT-018; FC-040 | `AssertRuleResult(input, null, docMode)` | pass |

How `AssertBracesAndTriviaAreKept` works, under LF and CRLF:
- It formats the input and compares the output with all spaces and line breaks removed (e.g. `classA{}//note;`). This proves that the braces, the trivia, and the `;` stay, in order.
- It then runs `AssertSecondPassIsStable`.

It deliberately does not pin columns. FMT-STRUCT-018 decides only that the braces and the `;` stay. Today the formatter indents a `;` on its own line by four spaces, which is follow-up candidate 2 in `00-lead.md` and is not specified.

## Scenarios (bug fix)

_Not applicable_ (behavior change lane).

## Not covered, and why

- **Core unit tests of `EmptyTypeDeclarationSemicolonAnalysisUtilities`**: none added.
  - All three callers (analyzer, code fix, formatter transform) are tested through their public surfaces above, on both sides of every new condition: a `;` present or absent, and whitespace or trivia between `}` and `;`.
  - A Core unit test would pin the helper's current signature, which the implementer may restructure.
- **The kind dimension**: tested once per rule, not collapsed into one test. Each rule has its own analyzer and fix class over a shared base with overrides, which `test-implementer.md` says is no proof of a single call site. The formatter dispatches per `Visit*Declaration`. One formatter test lists every kind, and unions have a separate test (C# 15).
- **Version gates per kind with `;`**: tested for RH5412 (C# 11 vs 12) and for the record gate in the formatter (C# 11 converts records). The other kinds' gates go through the same `minimumLanguageVersion` comparison after the `;` check, and their gates without `;` are already pinned by `Verify…UnderCSharp11/12`, `…UnderCSharp8/9`, and `…UnderCSharp9/10` in each rule's tests.
- **`class A⏎{⏎}; /* x */ class B;` (a comment, then another declaration behind the `;`)**: not covered. No spec example decides where `class B;` goes after the conversion; that is line-break layout (SHARED-FIX-007), which `VerifyFixGivesAClassThatSharesItsLineWithAClassBehindItALineOfItsOwn` already pins without a `;`.
- **`;;` (two semicolons)**: not covered. It is a parse error (02-skeptic.md), so the formatter skips the file and the fix is not offered (SHARED-FIX-011, pinned by `NoFixIsOfferedInAFileWithASyntaxError`).
- **Issue goals 2 and 3 (directives in the body)**: not covered. Lead decision 1 dropped them. They are already pinned by `VerifyRegionDirectivesInBodyAreReportedWithoutCodeFix`, `VerifyPragmaDirectiveInBodyIsReportedWithoutCodeFix`, `VerifyClassWhoseOnlyMemberLiesInAConditionalBlockIsReportedWithoutCodeFix`, RH5416 `VerifyDirectiveContainingRecordStructIsReportedWithoutCodeFix`, and RH5418 `UnionWithRegionOrPragmaInBodyIsReportedWithoutFix`. None of them changed.
- **Failure classes that cannot reach the change**:

  | Failure class | Why it cannot reach the change |
  |---|---|
  | FC-011 | No loop-until-stable is touched |
  | FC-012 | Only one phase (structural) changes; second-pass and phase-stability checks run in every `AssertRuleResult` |
  | FC-014 | The formatter already owns the construct |
  | FC-018, FC-036 | No rename |
  | FC-022, FC-023, FC-025, FC-029 | No literals, identifiers, expression bodies, or key/value layout |
  | FC-028 | The fix uses `FormatNodeInDocumentAsync` only. Document-scoped formatting is covered by the fix tests and whole-document formatting by the formatter tests; no caller formats a detached type declaration here |
  | FC-038 to FC-043 | No shape is declared unsupported (lead decision 3) |
  | FC-009 | The withheld cases assert that no action is registered |
  | FC-007 | Convergence is covered by `Verify`, which re-analyzes the fixed code and checks the iteration count |
- **Existing tests that assert a changed output**:
  - I searched `tests/` with `rg -U` for an empty type body followed by `;`, in both `.cs` and non-`.cs` files. The only hit was RH5418 `UnionWithBracesAndSemicolonIsNotReported`, now inverted (see table).
  - `EmptyStatementLineBreakTests` has `{⏎}⏎;` inside a method (an `if` block followed by an empty statement), not a type, so it is unaffected.
  - No CLI or self-hosting fixture holds the shape.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5412|FullyQualifiedName~RH5413|FullyQualifiedName~RH5414|FullyQualifiedName~RH5415|FullyQualifiedName~RH5416|FullyQualifiedName~RH5418|FullyQualifiedName~RH5404"
scripts/test.sh --project formatter --filter "FullyQualifiedName~EmptyTypeDeclarationTrailingSemicolonTests|FullyQualifiedName~StructuralTransformRequirementTests|FullyQualifiedName~UnionDeclarationStructuralTests"
scripts/test.sh --project architecture
```

On 49f88f0:
- Analyzer filter: 31 failed, 150 passed. Every failure is a new or changed test listed as "fail" above.
- Formatter filter: 8 failed, 38 passed. This was measured before I split the C# 11 test; the new file alone is 6 failed, 8 passed.
- Architecture: 198 passed.

## Follow-up candidates

_None_ new. The FMT-STRUCT-018 property tests touch follow-up 2 in `00-lead.md` (the indent of a `;` kept on its own line behind a type's closing brace) but do not pin it. Whoever takes that follow-up can tighten them to exact output.
