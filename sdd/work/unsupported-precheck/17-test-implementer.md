Role: test implementer
Commit: 4369feb9
Verdict: tests written

Repair round 2, Decision 18, finding 2 of `16-auditor.md` (finding 1 for the RH7104 case). The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond naming the derivations to cover. No production code and no existing test is changed; every test below is new and only appended to its class.

Each test puts a refused shape (a comment between a local's type and its name, FMT-UNSUP-004) inside the node the fix formats or builds, and outside the diagnostic's own node, and asserts with `VerifyNoCodeFixIsOffered` that the diagnostic is reported without a fix. RH3004 and RH3208 are the two exceptions noted under *Not covered*.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH5201MethodChainsShouldBeAlignedAnalyzerTests.NoFixIsOfferedWhenTheChainContextHoldsACommentTheFormatterRefuses` (chain context, `FluentChainCodeFixHelper.IsFormattingRefused`) | SHARED-FIX-010 | `VerifyNoCodeFixIsOffered` | pass |
| `RH3208ExpressionStyleLocalFunctionsShouldNotBeUsedAnalyzerTests.NoFixIsOfferedForALabeledLocalFunctionWhoseBodyHoldsACommentTheFormatterRefuses` (label context, `GetFormattingInput`) | SHARED-FIX-010 | `VerifyNoCodeFixIsOffered` | pass |
| `RH7102ConstantsMustAppearBeforeFieldsAnalyzerTests.NoFixIsOfferedWhenAnotherMemberOfTheTypeHoldsACommentTheFormatterRefuses` (type-member-ordering base, built type) | SHARED-FIX-010 | `VerifyNoCodeFixIsOffered` | pass |
| `RH7105DeclarationKeywordsMustFollowOrderAnalyzerTests.NoFixIsOfferedWhenTheBodyOfTheReorderedMemberHoldsACommentTheFormatterRefuses` (modifier-ordering base, built member) | SHARED-FIX-010 | `VerifyNoCodeFixIsOffered` | pass |
| `RH7107PropertyAccessorsMustFollowOrderAnalyzerTests.NoFixIsOfferedWhenTheOtherAccessorHoldsACommentTheFormatterRefuses` (built property; shape in the setter, diagnostic on the getter) | SHARED-FIX-010 | `VerifyNoCodeFixIsOffered` | pass |
| `RH7108EventAccessorsMustFollowOrderAnalyzerTests.NoFixIsOfferedWhenTheOtherAccessorHoldsACommentTheFormatterRefuses` (built event; shape in `remove`, diagnostic on `add`) | SHARED-FIX-010 | `VerifyNoCodeFixIsOffered` | pass |
| `RH8309XmlDocumentationElementsMustFollowPrescribedOrderAnalyzerTests.NoFixIsOfferedWhenTheDocumentedMemberHoldsACommentTheFormatterRefuses` (built declaration) | SHARED-FIX-010 | `VerifyNoCodeFixIsOffered` | pass |
| `RH3004UseLambdaSyntaxAnalyzerTests.NoFixIsOfferedWhenTheAnonymousMethodBodyHoldsACommentTheFormatterRefuses` (built lambda) | SHARED-FIX-010 | `VerifyNoCodeFixIsOffered` | pass |
| `RH7104PartialElementsMustDeclareAccessModifierAnalyzerTests.NoFixIsOfferedWhenTheTypeHoldsACommentTheFormatterRefuses` (built type) | SHARED-FIX-010 | `VerifyNoCodeFixIsOffered` | pass |
| `RH5110GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzerTests.NoFixIsOfferedWhenTheConstrainedDeclarationHoldsACommentTheFormatterRefuses` (finder: constraint owner) | SHARED-FIX-010 | `VerifyNoCodeFixIsOffered` | pass |
| `RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzerTests.NoFixIsOfferedWhenTheFormattingScopeHoldsACommentTheFormatterRefuses` (finder: `IsScopeRefused`, scope = enclosing block) | SHARED-FIX-010 | `VerifyNoCodeFixIsOffered` | pass |
| `RH5308ConditionalExpressionsShouldBeFormattedCorrectlyAnalyzerTests.NoFixIsOfferedWhenTheStatementHoldsACommentTheFormatterRefuses` (finder: `GetFormatTarget`, the statement) | SHARED-FIX-010 | `VerifyNoCodeFixIsOffered` | pass |
| `RH5403StatementMustNotBeOnSingleLineAnalyzerTests.NoFixIsOfferedWhenTheOwningStatementHoldsACommentTheFormatterRefuses` (finder: owning `if`; shape in `else`) | SHARED-FIX-010 | `VerifyNoCodeFixIsOffered` | pass |
| `RH8305SummaryElementMustSpanAtLeastThreeLinesAnalyzerTests.NoFixIsOfferedWhenTheDocumentedMemberHoldsACommentTheFormatterRefuses` (finder: documented member) | SHARED-FIX-010 | `VerifyNoCodeFixIsOffered` | pass |
| `RH8307TextAfterOpeningXmlTagMustBeOnSameLineAsClosingTagAnalyzerTests.NoFixIsOfferedWhenTheDocumentedMemberHoldsACommentTheFormatterRefuses` (finder: documented member) | SHARED-FIX-010 | `VerifyNoCodeFixIsOffered` | pass |
| `RH6024BinaryOperatorsMustBeSpacedCorrectlyAnalyzerTests.NoFixIsOfferedWhenTheBinaryExpressionHoldsACommentTheFormatterRefuses` (operator-spacing base; shape in a lambda operand) | SHARED-FIX-010 | `VerifyNoCodeFixIsOffered` | pass |
| `RH5405BracesMustNotBeOmittedAnalyzerTests.NoFixIsOfferedWhenTheIfStatementHoldsACommentTheFormatterRefuses` (statement-braces base; shape in `else`) | SHARED-FIX-010 | `VerifyNoCodeFixIsOffered` | pass |

## Not vacuous
Each test was checked against its counterpart without the refused shape: `/* c */` was removed from all 17 new tests on a scratch copy (for RH5201 replaced by spaces of the same width, so the chain's columns stay), and all 17 then failed with `No code fix may be offered for the diagnostic at …`. The fix's other guards therefore accept each input, and the refused comment is what withholds the fix. The files were then restored. Because each comment sits outside the diagnostic's own node, a registration that asked about that node instead of the formatted or built node would offer the fix and fail the test (RH3004 and RH3208 excepted, below).

## RH7104 (finding 1)
The RH7104 test passes on this head. The auditor states that the type as it stands and the type the fix builds get the same answer today, and no input can tell them apart: the only refused shape is a comment inside a variable declaration, and the fix only adds accessibility keywords in front of the type's modifiers. No test can fail on finding 1 until a refused shape touches a type's modifiers or leading trivia. This test pins the outcome for the implementer's change (query the built type via a helper shared with the action): it must stay green. Finding 1 is a structural defect against SHARED-FIX-010's "MUST NOT decide the question by a check of its own", and the review checks it, not a test.

## Not covered, and why
- **RH3208 label context vs. local function.** A labeled statement adds only `identifier :` around the local function, and a local function's only places for a declaration besides its arrow clause are parameter defaults and attribute arguments, which must be constants (a lambda there does not compile). So no compiling refused shape can sit inside the label context and outside the arrow clause, which is the diagnostic's span. The test covers the label path with the shape inside the arrow clause. It cannot tell a context query from a local-function query, because both give the same answer for every input.
- **RH3004 built lambda vs. anonymous method.** The lambda the fix builds keeps the anonymous method's block, so any refused shape in it is also in the anonymous method the diagnostic points at. The test covers the built-node query. A test that tells the two apart needs a refused shape that the conversion creates, and none exists.
- **RH5112.** Uses the same `FluentChainCodeFixHelper.IsFormattingRefused` as RH5201 (`rg -n "IsFormattingRefused" src/Reihitsu.Analyzer.CodeFixes` gives the helper and the two call sites RH5112:44 and RH5201:46, with no override), so one test covers both.
- **Other rules on the same bases.** RH7103, RH7106, and RH7109 use the type-member-ordering base (or, for RH7106, the modifier-ordering base); RH6023 uses the operator-spacing base; RH5406 and RH5407 use the statement-braces base. In each base the registration query is in the sealed `RegisterCodeFixesAsync`, and the formatted node comes from a private finder or builder that no subclass overrides. The subclasses supply only `TryGetTargetMember` or `GetUpdatedModifiers`, the target member or the modifier order, which doesn't change which node is queried. One test per base covers them.
- **CRLF, Fix All, idempotency.** No formatted output exists to compare: the fix is withheld, and the formatter's line-ending handling of the refused input is tested in `UnsupportedVariableDeclarationTests`.
- **Failure classes.** Only FC-017 (assertion weaker than the requirement) and FC-033 (vacuous pass) apply to a no-fix registration test, and both are addressed under *Not vacuous*. The rest concern rewrites, layout, or naming, which these tests do not exercise.

## Commands
```shell
scripts/test.sh --project analyzer --filter "TestCategory=SHARED-FIX-010"
```
Result on the working tree after these changes: `Passed!  - Failed: 0, Passed: 29, Skipped: 0, Total: 29` (12 pre-existing, 17 new).
