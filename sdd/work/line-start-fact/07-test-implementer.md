Role: test implementer
Commit: c846ab3
Verdict: tests written

Tests for the change doc `sdd/changes/2026-10/fixed-keyword-spacing.md` (Accepted) and the changed FMT-SPACE-012 (`06-spec-author.md`, decision `squad/decisions/2026-10-31-line-start-fact-fixed-spacing.md`). The prompt carried no intended fix.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `src/Reihitsu.Formatter.Test/Regression/Spacing/HorizontalSpacingRequirementTests.OneSpaceAfterFixedKeyword` (the FMT-SPACE-012 `fixed` example) | FMT-SPACE-012 | `AssertRuleResult` (LF and CRLF, second pass, phase check) | red: `fixed(` is kept |
| `….OneSpaceAfterFixedKeywordBehindBlockComment` (the change doc's `fixed/* c */(…)` → `fixed/* c */ (…)`) | FMT-SPACE-012, FMT-SPACE-013 | `AssertRuleResult` | red |
| `….OneSpaceAfterEachOfNestedFixedKeywords` (two stacked `fixed(` statements) | FMT-SPACE-012 (FC-026) | `AssertRuleResult` | red |
| `….SeveralSpacesAfterFixedKeywordBecomeOne` (`fixed   (` → `fixed (`) | FMT-SPACE-012 | `AssertRuleResult` | green: FMT-SPACE-002's collapse already gives it |
| `….SpacedFixedKeywordIsKept` (stable code) | FMT-SPACE-012 | `AssertRuleResult` | green |
| `….FixedSizeBufferKeepsOneSpaceAfterFixedKeyword` (`public fixed int _buffer[4];` unchanged) | FMT-SPACE-012 | `AssertRuleResult` | green |
| `….SeveralSpacesInFixedSizeBufferBecomeOne` (`fixed  int` → `fixed int`, the change doc's "no exception" decision) | FMT-SPACE-012 | `AssertRuleResult` | green |
| `src/Reihitsu.Analyzer.Test/Formatter/Formatting/RH6001KeywordsMustBeSpacedCorrectlyFormatterTests.VerifyFormatterFixesGluedFixedKeyword` (RH6001 reports `fixed(`; the formatter output `fixed (` is expected, stable on a second pass, and raises no RH6001, under LF and CRLF) | RH6001-001 (FC-013, FC-014) | own: `Verify(…, AllowUnsafe, diag)` + `ReihitsuFormatter.FormatSyntaxTree` twice + `Verify(output, AllowUnsafe)` | red: the formatter keeps `fixed(` |

`VerifyFormatter` itself cannot host this fixture. It runs the analyzer verifier without a way to allow `unsafe` code, so the compiler error CS0227 would fail it. The parity test therefore does the same three steps on its own, with `ApplyAllowUnsafeToTestProject`.

Runs on c846ab3:
- **Formatter project:** `scripts/test.sh --project formatter --filter "FullyQualifiedName~HorizontalSpacingRequirementTests"` → `Failed: 3, Passed: 19`. The three failures are the red rows above, each with `fixed(` kept where `fixed (` is expected.
- **Analyzer project:** `--filter "FullyQualifiedName~RH6001KeywordsMustBeSpacedCorrectlyFormatterTests"` → `Failed: 1, Passed: 3`. In the parity test the diagnostic step passes and the formatter-output assertion fails.

## Not covered, and why

- **`fixed` followed by a line break before `(`.** A gap across a line break is not FMT-SPACE-012's (FMT-SPACE-001). Other `fixed` shapes behind a comment (`fixed/// x` / `(…)`) are already covered by the RH6001 rows of the line-start run.
- **The `fixed` statement in CRLF.** `AssertRuleResult` runs both line endings, and the parity test loops over both.
- **The keyword spacing of `fixed` with a directive in the gap (FC-020).** No requirement places a space next to a directive, and the spacing phase leaves such a gap to FMT-SPACE-001/002.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~HorizontalSpacingRequirementTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH6001KeywordsMustBeSpacedCorrectlyFormatterTests"
```
