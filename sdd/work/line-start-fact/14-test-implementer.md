Role: test implementer
Commit: 20c85d9
Verdict: tests written

Answers `12-auditor.md` finding #8, using the corrected change doc and the new FMT-SPACE-012 example (`13-spec-author.md`). The prompt carried no intended fix.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `src/Reihitsu.Formatter.Test/Regression/Spacing/HorizontalSpacingRequirementTests.OneSpaceBehindCommentGluedToFixedSizeBufferType`: the FMT-SPACE-012 example, `public fixed/* c */int _b[4];` → `public fixed/* c */ int _b[4];` | FMT-SPACE-012, FMT-SPACE-013 (FC-003, FC-017) | `AssertRuleResult` (LF and CRLF, second pass, phase check) | green |
| `….OneSpaceBehindSpacedCommentGluedToFixedSizeBufferType`: `public fixed /* c */int _c[4];` → `public fixed /* c */ int _c[4];` | FMT-SPACE-012, FMT-SPACE-013 | `AssertRuleResult` | green |
| `….SeveralSpacesBehindCommentInFixedSizeBufferBecomeOne`: `public fixed /* d */  int _d[4];` → `public fixed /* d */ int _d[4];` | FMT-SPACE-012, FMT-SPACE-013 | `AssertRuleResult` | green |

The inputs and outputs are the rows of the spec author's table. The third row is the whitespace side of the boundary: its output was the same before the change.

## Mutation check

I removed `case SyntaxKind.FixedKeyword:` from `KeywordSpacingRule.IsSpacedKeyword` (`src/Reihitsu.Formatter/Pipeline/HorizontalSpacing/Rules/KeywordSpacingRule.cs:100`) and ran the class. I restored the file with `git checkout` afterwards; nothing of that edit is committed.

- **Head:** `scripts/test.sh --project formatter --filter "FullyQualifiedName~HorizontalSpacingRequirementTests"` gives `Passed: 25, Failed: 0`.
- **Without `fixed` in the rule:** `Failed: 5, Passed: 20`. The failures are:
  - the two new comment-glued buffer rows;
  - the three statement rows from `07-test-implementer.md` (`OneSpaceAfterFixedKeyword`, `…BehindBlockComment`, `…NestedFixedKeywords`).
- **Rows that stay green without `fixed`:** the third new row and the two older buffer rows (`FixedSizeBufferKeepsOneSpaceAfterFixedKeyword`, `SeveralSpacesInFixedSizeBufferBecomeOne`). FMT-SPACE-002's collapse produces their output on its own, so they pin output the change must keep but cannot catch the removal. That is the role the auditor gave them.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~HorizontalSpacingRequirementTests"
```
