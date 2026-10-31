Role: test implementer
Commit: def9b43
Verdict: tests written

Repair round for decisions 8 and 11 of `00-lead.md`. The coordinator's message carried the tasks and no intended fix, suspected cause, or hoped-for verdict; nothing was ignored. The coordinator also named "the five tests" for `ComputeBaseIndentLevel`. There were seven, and all seven were deleted (see *Deleted tests*).

Expected values:
- RH5204: the change doc's *Visible output changes* (`after` block), which carries the maintainer's zero-offset answer.
- RH5405: the same section (`after` block). The comment is required by SHARED-FIX-003; the braces in column 13 stay as today (thoenissen/Reihitsu#1521).

`Verify` compares the whole document. So the RH5204 test also pins two things the change doc's `after` block shows for this run: the blank line behind `while (a)` and `/* note` in column 12. Neither is decided by a requirement.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH5405BracesMustNotBeOmittedAnalyzerTests.VerifyFixKeepsTheTrailingCommentOfAnElseIfCondition` | SHARED-FIX-003 | `Verify(source, fixedSource)` | passes on this branch; fails on `origin/main` `c24bc1d` (result differs at the `else if` line) |
| `RH5405BracesMustNotBeOmittedAnalyzerTests.VerifyFixKeepsTheTrailingCommentOfAnElseIfConditionWithCarriageReturnLineFeed` | SHARED-FIX-003 | `Verify` with CRLF | passes on this branch; fails on `main` |
| `RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzerTests.VerifyEmbeddedIfBehindMultiLineCommentIsFixedInOnePass` | RH5204-006, SHARED-FIX-001 | `Verify(source, fixedSource, onConfigure, diagnostics)` with `NumberOfIncrementalIterations = 1` and `NumberOfFixAllIterations = 1` | passes on this branch; fails on `main` (`/* note` in column 8, braces and body not moved) |
| `RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzerTests.VerifyEmbeddedIfBehindMultiLineCommentIsFixedInOnePassWithCarriageReturnLineFeed` | RH5204-006, SHARED-FIX-001 | same, CRLF | passes on this branch; fails on `main` |

How the `main` results were taken: both test files were copied into a temporary worktree of `origin/main`, outside the repository and removed afterwards, and the filter below was run there. All four tests failed in "Iterative code fix application" with a content mismatch.

The RH5204 input reports two diagnostics, on `{` and `}`. `X();` is not reported, so the markup holds two locations. Each test expects one incremental iteration and one Fix All iteration. That is the convergence SHARED-FIX-001 asks for, and `main` misses it (`no-progress`, `07-spec-author.md`).

Both classes on this branch: 91 of 91 tests pass (`RH5405…Tests` and `RH5204…Tests`).

## Deleted tests

Decision 8: the seven tests in `src/Reihitsu.Formatter.Test/Unit/ReihitsuFormatterHelpersTests.cs` that call `ReihitsuFormatterHelpers.ComputeBaseIndentLevel`. The coordinator said five; `05-implementer.md` also says five. There are seven, and all seven call the wrapper, so leaving any would stop the build once the wrapper is removed.

- `ComputeBaseIndentLevelReturnsZeroForTopLevelMemberInFileScopedNamespace`
- `ComputeBaseIndentLevelReturnsOneForMemberInsideBlockScopedNamespace`
- `ComputeBaseIndentLevelReturnsTwoForMethodInsideClassInsideBlockScopedNamespace`
- `ComputeBaseIndentLevelCountsNestedTypesCorrectly`
- `ComputeBaseIndentLevelCountsSwitchSectionForStatementInsideCaseBody`
- `ComputeBaseIndentLevelCountsSwitchSectionForStatementInsideDefaultBody`
- `ComputeBaseIndentLevelDoesNotCountSwitchSectionForCaseLabel`

The `using Microsoft.CodeAnalysis.CSharp.Syntax;` that only those tests used was removed. No requirement tag was attached to any of them. The remaining 18 tests of the class pass. No other test references the wrapper. Core's `SyntaxIndentationUtilities.ComputeBaseIndentLevel` keeps its caller in `TargetAttributePlacementCodeFixProviderBase`.

## Not covered, and why

- **Fix All with several instances in one document:** the two shapes do not co-occur meaningfully in one method. Both classes already carry a `GetFixAllScenario`, and the new tests set the Fix All iteration count to 1 for their own document.
- **Formatter parity (`VerifyFormatter`):** whole-document formatting is not changed by this run. It braces the inner `if` of the RH5204 input (FMT-STRUCT), so it gives a different, already-specified result.
- **FC-019/FC-020 (directive in the gap):** the gap here is a comment by construction. A directive in front of an embedded `if` is a different shape, and no requirement of this run names it.
- **Braces' column for RH5405:** pinned as today (column 13), per the instruction. It is thoenissen/Reihitsu#1521's to change.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~VerifyFixKeepsTheTrailingCommentOfAnElseIfCondition|FullyQualifiedName~VerifyEmbeddedIfBehindMultiLineCommentIsFixedInOnePass"
scripts/test.sh --project formatter --filter "FullyQualifiedName~ReihitsuFormatterHelpersTests"
```
