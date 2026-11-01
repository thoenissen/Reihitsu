Role: test implementer
Commit: 7dd5f74
Verdict: tests written

Repair round 3 (`00-lead.md` row 26): mirror FMT-SPACE-021's corrected second example and cover the parameter lists the bullet now names (`19-spec-author.md`, `18-auditor.md` Finding #2). The prompt named the change to make; it matches the spec author's handover, which I worked from. Both tests cover behavior the branch already has, so they must pass on this head.

File: `src/Reihitsu.Formatter.Test/Regression/Spacing/DeclaredNameSpacingTests.cs`.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `MembersAndStatementsThatDeclareAVariableAreSpacedLikeTheSpecificationExample` — `unsafe class C` replaced by `unsafe struct C` in `input` and `expected`; every other line unchanged, so both literals equal the spec's blocks | FMT-SPACE-021 | `AssertRuleResult` | pass |
| `ParameterNamesOfEveryParameterListGetOneSpaceBehindTheirType` — adds a conversion operator (`explicit operator C(int?v)`), an anonymous method (`delegate(int?m, int[]n)`), and an extension block (`extension(string?s)` with a member `N(int[]k)`) to the existing lists; summary now names all ten lists, matching the spec bullet | FMT-SPACE-021 | `AssertRuleResult` | pass |

The added constructs are written in their canonical block layout (block-bodied operator, anonymous method and extension member), so that only the declared-name gaps differ between input and expected. A first draft with `delegate(...) { return 0; }` on one line and an expression-bodied extension member failed on unrelated layout (FMT-STRUCT and line-break rewrites); I removed that noise rather than pinning it.

Discrimination: in a scratch worktree of `origin/main` (removed afterwards) both tests fail on the declared-name gap — `class C(int?a, int[]b)` and `private int?_f;` stay tight where one space is expected.

## Not covered, and why
- One test for all parameter lists: every list reaches the single `ParameterSyntax` arm of `DeclaredNameSpacingRule` (`rg "ParameterSyntax" src/Reihitsu.Formatter/Pipeline/HorizontalSpacing/Rules/DeclaredNameSpacingRule.cs`); the lead asked for the new lists to be named, so they are added to the existing test rather than split per arm.
- Function pointer parameters have no name and destructors no parameters (`19-spec-author.md`), so they have nothing to test.
- Joined forms, comments, directives, CRLF: unchanged from `15-test-implementer.md`; `AssertRuleResult` runs LF, CRLF, a second pass and phase stability.

## Commands
```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Regression.Spacing.DeclaredNameSpacingTests"
scripts/test.sh --project formatter
```
The formatter project passes on this head: 3959 passed, 0 failed. `scripts/format.sh` over the test file changed nothing.
