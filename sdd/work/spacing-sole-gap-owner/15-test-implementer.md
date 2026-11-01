Role: test implementer
Commit: ebfdfd7
Verdict: tests written

Repair round 2, auditor Finding #1 and the BOM hint (`13-auditor.md`), lead row 25. The prompt carried no intended fix or hoped-for verdict. The tests cover behavior the branch already has, so each of them must pass on this head.

All new tests are in `src/Reihitsu.Formatter.Test/Regression/Spacing/DeclaredNameSpacingTests.cs`. They use `AssertRuleResult`, which runs the full pipeline under LF and CRLF, then a second pass and the phase-stability check.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `FieldNamesGetOneSpaceBehindTheirType` (`int?`, `static readonly int[]`, `List<int>`, tuple, `int  `) | FMT-SPACE-021 | `AssertRuleResult` | pass |
| `ConstFieldNameGetsOneSpaceBehindItsType` | FMT-SPACE-021 | `AssertRuleResult` | pass |
| `FixedSizeBufferNameGetsOneSpaceBehindItsType` (in an `unsafe struct`) | FMT-SPACE-021 | `AssertRuleResult` | pass |
| `EventFieldNamesGetOneSpaceBehindTheirType` (one and two declarators) | FMT-SPACE-021 | `AssertRuleResult` | pass |
| `ConstAndRefLocalNamesGetOneSpaceBehindTheirType` (`const`, `ref`, `ref readonly`) | FMT-SPACE-021 | `AssertRuleResult` | pass |
| `UsingDeclarationNamesGetOneSpaceBehindTheirType` (two declarators, `await using`) | FMT-SPACE-021 | `AssertRuleResult` | pass |
| `FixedStatementNamesGetOneSpaceBehindTheirType` (two declarators) | FMT-SPACE-021 | `AssertRuleResult` | pass |
| `ForInitializerNamesGetOneSpaceBehindTheirType` (two declarators) | FMT-SPACE-021 | `AssertRuleResult` | pass |
| `UsingStatementNameGetsOneSpaceBehindItsType` | FMT-SPACE-021 | `AssertRuleResult` | pass |
| `MembersAndStatementsThatDeclareAVariableAreSpacedLikeTheSpecificationExample` (the second example, verbatim) | FMT-SPACE-021 | `AssertRuleResult` | pass |
| `ParameterNamesOfEveryParameterListGetOneSpaceBehindTheirType` (primary constructor, constructor, operator, indexer, delegate, local function, explicitly typed lambda) | FMT-SPACE-021 | `AssertRuleResult` | pass |

As a discrimination check, I copied the test file into a scratch worktree of `origin/main` (since removed) and ran the class there. Ten of the eleven new tests fail on `main`, each on the declared-name gap: for example, `private int?_f;` stays as written where `private int? _f;` is expected. `FixedSizeBufferNameGetsOneSpaceBehindItsType` passes on `main`. Its input has only whitespace runs behind keyword types, which FMT-SPACE-002 already collapses there, and the spec author's handover records the same.

The operator test's expected output shows `=> l` turned into a block body. That is FMT-STRUCT behavior the pipeline already has, not something this test pins as its subject. The test asserts the full output only because `AssertRuleResult` compares whole documents.

## Joined forms
I found no join that reaches the type-to-name gap of any newly listed declaration, so I added no joined test. I checked this on head with the branch CLI (`scripts/format.sh`). The input put a line break between the type and the name of:
- a field, a `const` field, an event field, and a fixed-size buffer;
- a `fixed` statement, a `for` initializer, a `using` statement, and a `using` declaration;
- a `const` local, a `ref` local, and a plain local;
- the same statements inside a lambda body that stands in an argument and in an object-initializer argument.

Each wrap stayed a wrap. Only the name line was re-indented, to the column of the type. That indentation is not FMT-SPACE-021's concern, so I did not pin it. The existing joined tests (parameter and range variable, FMT-BREAK-031) remain the only joined forms.

## BOM
I prepended the missing UTF-8 BOM to `src/Reihitsu.Formatter.Test/Regression/Spacing/RightCarriedDocumentationCommentSpacingTests.cs`, and nothing else in that file changed. I then checked every `.cs` file added between the merge base with `origin/main` and HEAD by reading its first three bytes. That file was the only one without a BOM, and after the fix all of them have one.

## Not covered, and why
- The types the spec leaves unspecified (properties, indexers, events with accessors, and the return types of methods, local functions, and delegates) are not pinned, as instructed.
- The `var` pattern designation, `var (x, y)`, and names inside a parenthesized designation are outside FMT-SPACE-021 and are not tagged here.
- One test per form, although every variable-declaration form shares one arm: `rg "VariableDeclaratorSyntax" src/Reihitsu.Formatter/Pipeline/HorizontalSpacing/Rules/DeclaredNameSpacingRule.cs` shows the single arm `VariableDeclaratorSyntax { Parent: VariableDeclarationSyntax declaration }`. The lead asked for each listed member, so each has its own test anyway. The same holds for the parameter lists, which share the `ParameterSyntax` arm, so one test covers all of them.
- Comment and directive in the gap: the existing tests `CommentBehindTheTypeKeepsItsPlace` and `DocumentationCommentInFrontOfTheNameKeepsItsPlace` already cover the shared gap. The new members reach the same rule arm, and no join applies to them (see above).
- No code fix and no new rule, so FC-007, FC-008, and FC-014 cannot reach this change.

## Note
My first CLI probe ran `scripts/format.sh` over a scratchpad folder, `.../scratchpad/probe`, that turned out to hold probe files from earlier roles. It reformatted those scratch files. They are outside the repository and not part of the run's evidence.

## Commands
```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Regression.Spacing.DeclaredNameSpacingTests"
scripts/test.sh --project formatter
```
The formatter project passes on this head: 3959 passed, 0 failed.
