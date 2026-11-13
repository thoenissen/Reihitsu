Role: test implementer
Commit: 09e4e1bb09b6f4491eb20c9e2825d9506203abc9 (every "Expected now" below measured on this production code, Debug build; tests added on top)
Verdict: tests written

Repair round 2: decisions 17 to 20 of `00-lead.md`, which cover auditor 14 findings 1 to 5. The tests follow the examples in `15-spec-author.md`. The prompt carried no intended fix, suspected cause, or hoped-for verdict. All four issues are `agent-filed`. No production code was changed.

## Tests

### Decision 17 (finding 1): SHARED-BRACE-002, a body that ends the document

File `src/Reihitsu.Analyzer.Test/Formatting/RH5405BracesMustNotBeOmittedAnalyzerTests.cs`. A new private helper, `VerifyTopLevelCodeFixWithEndOfLine`, runs the verifier with `OutputKind.ConsoleApplication`, so top-level statements compile. It then asserts the raw text that `ApplyCodeFixAsync` returns, line endings and the end of the file included.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `FixWrapsABodyOnTheLineOfItsIfThatEndsTheDocument` [LF/CRLF] (spec example) | SHARED-BRACE-002 | `VerifyTopLevelCodeFixWithEndOfLine` | red: no fix, the text is unchanged |
| `FixWrapsABodyBelowItsIfWithACommentBehindItThatEndsTheDocument` [LF/CRLF] (spec prose twin, `// c`) | SHARED-BRACE-002 | same | red: no fix |
| `FixWrapsABodyBelowItsIfThatEndsTheDocument` [LF/CRLF] (sibling without the comment) | SHARED-BRACE-002 | same | red: no fix |
| `FixKeepsTrailingWhitespaceOfABodyThatEndsTheDocumentOnTheBodyLine` [LF/CRLF] (`M();···` at the end of the file; the whitespace stays on the body line, and nothing stands behind `}`) | SHARED-BRACE-002 | same | red: no fix |
| `FixWrapsABodyOnTheLineOfItsIfFollowedByAFinalLineBreak` [LF/CRLF] (other side: a final line break is present and stays behind `}`) | SHARED-BRACE-002 | same | green |

In every red test the analyzer step passes (the diagnostic is reported). The failure is in the verifier's code-fix step: the diff shows the input unchanged against the expected braces.

### Decision 18 (finding 2): widened SHARED-ATTR-022

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH5506 `VerifyDiagnosticWithoutCodeFixOnTheLineOfTheBraceOfTheNamespaceThatHoldsTheClass` (spec example) | RH5506-004, SHARED-ATTR-022 | `VerifyNoCodeFixIsOffered` | red (fix offered) |
| RH5506 `VerifyDiagnosticWithoutCodeFixOnTheLineOfTheBraceOfTheClassThatHoldsTheNestedClass` | SHARED-ATTR-022 | same | red |
| RH5506 `VerifyCodeFixSplitsAListOfAClassInAFileScopedNamespace` [LF/CRLF] (other side: no `{` holds the class) | SHARED-ATTR-022, SHARED-ATTR-017 | `VerifyCodeFixWithEndOfLine` | green |
| RH5514 `SplitFixIsWithheldOnTheLineOfTheBraceOfTheClassThatHoldsTheMethod` (spec example) | RH5514-005, SHARED-ATTR-022 | `VerifyNoCodeFixIsOffered` | red |
| RH5514 `SplitFixIsWithheldOnTheLineOfTheBraceOfTheInterfaceThatHoldsTheMethod` | SHARED-ATTR-022 | same | red |
| RH5514 `SplitFixIsWithheldOnTheLineOfTheBraceOfTheBlockThatHoldsTheLocalFunction` (spec example) | RH5514-005, SHARED-ATTR-022 | same | red |
| RH5514 `SplitFixIsWithheldOnTheLineOfTheBraceOfTheBlockThatHoldsTheLocalFunctionBehindAStatement` ("what stands between does not matter") | SHARED-ATTR-022 | same | red |
| RH5514 `SplitFixIsOfferedBehindTheBraceOfAnotherBodyOnTheLineOfTheList` [LF/CRLF] (spec example `void M() { } [A, B] void N() { }`) | SHARED-ATTR-022, SHARED-ATTR-017 | `VerifySplitFix` | green |
| RH5518 `VerifyDiagnosticWithoutCodeFixForEnumMemberListOnTheLineOfTheEnumBrace` (enum arm) | SHARED-ATTR-022 | `VerifyNoCodeFixIsOffered` | red |
| RH5518 `VerifyDiagnosticWithoutCodeFixForFieldListOnTheLineOfTheClassBrace` | RH5518-004, SHARED-ATTR-022 | same | red |
| RH5526 `VerifyDiagnosticWithoutCodeFixOnTheLineOfTheBraceOfTheClassThatHoldsTheDelegate` (delegate arm) | RH5526-004, SHARED-ATTR-022 | same | red |

The accessor-list arm and the RH5518/RH5531 examples were covered in 08 and stay green. Every red test fails on `Assert.IsEmpty(actions)` inside `VerifyNoCodeFixIsOffered`, after the diagnostic step has passed.

### Decision 19 (finding 3): new SHARED-ATTR-023

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH5518 `VerifyDiagnosticWithoutCodeFixForALineBreakBehindTheColonOfTheTargetSpecifier` (spec example `[field:⏎A, B]`) | RH5518-004, SHARED-ATTR-023 | `VerifyNoCodeFixIsOffered` | red |
| RH5518 `VerifyDiagnosticWithoutCodeFixForALineBreakInFrontOfTheTargetName` (`[⏎field: A, B]`) | SHARED-ATTR-023 | same | red |
| RH5518 `VerifyDiagnosticWithoutCodeFixForALineBreakBetweenTheTargetNameAndItsColon` (`[field⏎: A, B]`) | SHARED-ATTR-023 | same | red |
| RH5518 `VerifyCodeFixSplitsAListWithATargetSpecifierAndALineBreakBetweenTwoAttributes` [LF/CRLF] (spec example `[field: A,⏎B]`) | SHARED-ATTR-023, SHARED-ATTR-017 | `VerifyCodeFixWithEndOfLine` | green |
| RH5518 `VerifyCodeFixSplitsAListWithoutATargetSpecifierWhoseFirstAttributeStartsOnTheNextLine` [LF/CRLF] (`[⏎A, B]` → `[A]⏎[B]`) | SHARED-ATTR-023, SHARED-ATTR-017 | same | green |
| RH5514 `SplitFixIsWithheldForALineFeedInsideAnAttributeInACarriageReturnLineFeedDocument` (spec example) | RH5514-005, SHARED-ATTR-023 | `VerifyNoCodeFixIsOffered` | red |
| RH5514 `SplitFixIsWithheldForALineFeedInsideAStringArgumentInACarriageReturnLineFeedDocument` (line break inside a literal) | SHARED-ATTR-023 | same | red |
| RH5514 `SplitFixIsWithheldForALineFeedBetweenTwoAttributesInACarriageReturnLineFeedDocument` (a break the fix drops) | SHARED-ATTR-023 | same | red |
| RH5514 `SplitFixIsWithheldForALineBreakInsideTheListInADocumentWhoseLineBreaksAreLoneCarriageReturns` (spec example) | SHARED-ATTR-023, SHARED-FIX-008 | same | red |
| RH5514 `SplitFixKeepsALineBreakInsideAnAttributeWhenEveryLineBreakIsTheDetectedOne` [LF/CRLF] (spec example, inner break kept) | SHARED-ATTR-023, SHARED-ATTR-017 | `VerifyCodeFixWithEndOfLine` | green |
| RH5514 `SplitFixIsOfferedForALineBreakBetweenTwoAttributesWhenEveryLineBreakIsTheDetectedOne` [LF/CRLF] (other side of the LF-between-attributes test) | SHARED-ATTR-023, SHARED-ATTR-017 | `VerifySplitFix` | green |
| RH5514 `SplitFixInsertsTheDetectedLineEndingWhenTheListLineEndsInAnotherOne` (existing test, unchanged assertion; tag added: an LF that ends the list's line, outside `[`…`]`, does not withhold) | SHARED-ATTR-017, SHARED-ATTR-023, SHARED-FIX-008 | `ApplyCodeFixAsync` raw text | green |

The other side of the lone-`\r` test, `[A, B]` on one line in a lone-`\r` document, is the existing `SplitFixInsertsLineFeedInADocumentWhoseLineBreaksAreLoneCarriageReturns`, which stays green. New in RH5514: an `ArgumentAttributeDeclarations` constant (`A(int, int)`, `B`, `S(string)`) and a `WithLineFeedMarkers` helper. New in RH5518: an `AttributeDeclarations` constant (`A`, `B`).

### Decision 20 (finding 4): RH7309-007, a block that keeps its place

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH7309 `FixIsOfferedForALineFeedInABlockThatKeepsItsPlaceOfACarriageReturnLineFeedDocument`: three blocks, `Fields` / `IDisposable` / `Methods`. `Fields` keeps slot 0 and holds an LF in a CRLF document. The other two swap, and the LF stays. | RH7309-004, RH7309-007; FC-003 | `ApplyCodeFixAsync` raw text | green |

Mutation check in a scratch worktree of the head, removed afterwards: I deleted the `order[slot] == slot → continue` branch in `MovesOtherLineBreak`. This test went red, and it was the only one of the RH7309 line-ending tests that did.

### Decision 20 (finding 5): stale Fix All comments

The `GetFixAllScenario` comments of RH5405, RH5406, and RH5407 are reworded. Each fix now inserts the braces around its own body as text changes, without formatting the owning `if`. The two `if` statements in different methods give changes that lie apart. This is a comment-only change: the scenarios and their assertions are unchanged.

## Scenarios (bug fix)

_N/A._ This round reproduces no bug. The red tests are spec-derived tests for the repair, which implementer 17 makes.

## Not covered, and why

- **Brace end-of-file case for RH5406 and RH5407.** `rg -n "TryCreateClosingChange" src/Reihitsu.Analyzer.CodeFixes` shows a single call site in `StatementBracesCodeFixProviderBase`, which all three brace providers share. The end-of-file branch is in that method, so the RH5405 tests cover it.
- **An `else` body that ends the document.** It goes through the same `nextToken` branch as the `if` body. The `if`-body tests reach that branch.
- **SHARED-ATTR-022 with struct, record, and record struct bodies.** These are the same syntax family as class and interface (`TypeDeclarationSyntax`). The class and interface tests cover them. Enum (`EnumDeclarationSyntax`), namespace, block, and accessor list are each tested, because each has its own `{` owner.
- **SHARED-ATTR-022's "a list on a parameter" side.** The parameter rule, RH5524, does not use the split fix: it is not among the 14 split rules. A top-level local function and a file-scoped namespace have no `{` in the shape at all. The file-scoped namespace is tested.
- **RH5502 and RH5504 (`[assembly:⏎A, B]`).** The SHARED-ATTR-023 target-specifier check is in the shared `TargetAttributeListShapeCodeFixProviderBase`. `rg -n "ApplySplitCodeFixAsync" src/Reihitsu.Analyzer.CodeFixes` shows one call site. The RH5518 tests reach it.
- **CRLF variants of the withheld tests.** A withheld fix writes nothing. The offered sides have LF and CRLF variants, and the line-ending withholds are built from explicit mixed and lone-`\r` documents.
- **A comment or directive in the gap (SHARED-ATTR-019).** SHARED-ATTR-019 already withholds those lists, and its tests stay. A comment between the `{` and the list does not change the line comparison.
- **Failure classes:**
  - FC-008 (Fix All) is unchanged by these withholds.
  - The formatter classes (FC-010 to FC-012, FC-028, FC-030, FC-038 to FC-043) do not apply: neither fix calls the formatter.

## Commands

```shell
scripts/test.sh --project analyzer --configuration Debug --filter "FullyQualifiedName~RH5405BracesMustNotBeOmitted|FullyQualifiedName~RH5406Braces|FullyQualifiedName~RH5407UseBraces|FullyQualifiedName~RH7309Regions|FullyQualifiedName~RH5506Class|FullyQualifiedName~RH5514Method|FullyQualifiedName~RH5518Field|FullyQualifiedName~RH5526Delegate"
scripts/test.sh --project architecture --configuration Debug
```

Results:
- First command: `Failed: 24, Passed: 376, Total: 400`. The 24 failures are the red tests above: 8 brace DataRow cases and 16 attribute tests.
- Architecture run: `Passed: 117`. `EveryRequirementOfAnEnforcedSpecIsCovered` is green now that SHARED-ATTR-023 is tagged.
- `scripts/format.sh` changed none of the 8 test files.
