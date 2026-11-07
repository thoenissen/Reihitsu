Role: test implementer
Commit: b916957 (base); this handover's commit carries the tests
Verdict: #967: tests written; #1286: tests written; #1287: tests written; #1288: nothing to do

Repair round 1, for audit findings 1, 2a, 2b, 4a, and 4b of `10-auditor.md`. Finding 3 is a code-structure finding and has no test of its own. The prompt carried no intended fix, suspected cause, or hoped-for verdict, so there was nothing to ignore. I read the auditor's "Suggested change" column only to learn which outcomes it considers acceptable. The expected values come from the requirements and the maintainer's answers, not from any code. No production code was touched.

On `b916957` plus these tests, the whole analyzer project gives 22 failed and 7129 passed. The 22 failures are exactly the tests marked "fails" below, and nothing else fails. `scripts/test.sh --project architecture` (Release build, which compiles the test project) passes 115 and shows no warning in the changed files. `scripts/format.sh` was run over every changed file.

I checked the reason for every failure:
- Finding 1: the fix result has `int Counter;` at column 0, and the comment, directive, or `#if` block is gone.
- Finding 2a: "No code fix may be offered".
- Finding 2b: the result is `private int⏎    _value;`, and the comment, directive, or `#if` block is gone.
- Finding 4: "expected 1, actual 0" diagnostics.

None of them fails on a compile error.

## Harness change

`src/Reihitsu.Analyzer.Test/Base/AnalyzerTestsBase{TAnalyzer,TCodeFix}.cs`:
- **`TryApplyCodeFixAsync(source)`** returns the raw text of the first offered fix, or `null` when no action is registered. `ApplyCodeFixAsync` now delegates to it and still throws, so its behavior is unchanged.
- **`VerifyCodeFixIsWithheldOrProducesOneOf(source, mayBeWithheld, accepted…)`** does three things:
  - It checks the marked diagnostics with `Verify(source)`.
  - It accepts a withheld fix only when `mayBeWithheld` is true.
  - Otherwise it requires the result to equal one of the accepted texts exactly, line endings included, and then re-analyzes the result: no diagnostic, no compile error.

**Why the tests accept more than one result.** For findings 1 and 2b, the specs forbid dropping or moving the comment or directive (SHARED-FIX-003). They leave two things open:
- **Withhold or carry.** The auditor offers both, and `AGENTS.md` (*Code-fix coverage*) treats a withheld fix as a supported end state.
- **Layout.** Two layouts are accepted for the line that follows the removed or rebuilt token:
  - *in place*: the line keeps its column and its trivia;
  - *formatter*: `Format(in place)`, which outdents to column 4 and, in front of a comment line, inserts a blank line. I observed this with `scripts/format.sh` on the candidate outputs.

The tests accept exactly these outcomes and reject today's output. A withheld fix is not accepted when the gap holds only indentation.

**Open point for the lead.** SHARED-FIX-007 read literally ("formatting the result changes nothing in that code") picks the formatter layout, including the inserted blank line. RH2005's fix does not format, and the "rewritten in place" sentence of SHARED-FIX-003 points to the in-place layout. Which code-fix output RH2005 and RH2001 should write here is therefore not decided by an unchanged requirement, unless the lead reads one of them as deciding it. If the lead treats it as an output question, the tests reduce to the chosen option by deleting the other accepted text.

## thoenissen/Reihitsu#967

### Tests

These are in `Design/RH2005FieldsMustBePrivateAnalyzerTests.cs`. The finding's class spans these axes:
- the leading trivia of a replaced `public` that is not the first modifier and starts its own line: indentation only, a comment line, a `#pragma` line, or an `#if` block with disabled text;
- LF and CRLF.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `VerifyFixKeepsTheLineOfAPublicThatStartsItsOwnLineBehindAnotherModifier` | RH2005-003; FC-004 | `VerifyCodeFixIsWithheldOrProducesOneOf`, not withheld | fails (`int` at column 0) |
| `…BehindAnotherModifierCarriageReturnLineFeed` | RH2005-003; FC-017 | same, CRLF | fails |
| `VerifyFixKeepsACommentLineInFrontOfAPublicBehindAnotherModifier` | RH2005-003, SHARED-FIX-003; FC-004, FC-021 | same, may be withheld | fails (comment dropped) |
| `…CarriageReturnLineFeed` | RH2005-003, SHARED-FIX-003 | same, CRLF | fails |
| `VerifyFixKeepsADirectiveLineInFrontOfAPublicBehindAnotherModifier` | RH2005-003, SHARED-FIX-003; FC-005, FC-020 | same, may be withheld | fails (directive dropped) |
| `…CarriageReturnLineFeed` | RH2005-003, SHARED-FIX-003 | same, CRLF | fails |
| `VerifyFixKeepsAnExcludedModifierInFrontOfAPublicBehindAnotherModifier` | RH2005-003, SHARED-FIX-003; FC-020 | same, may be withheld | fails (`#if` block dropped) |

### Not covered, and why
- **Position of `public` among the modifiers, and attribute lists in front.** These go through one call site. `rg -n "AddAccessibilityModifiers?\(" src --glob '!*Test*'` shows only `DeclarationModifierUtilities`.
  - `AddAccessibilityModifiers(MemberDeclarationSyntax, …)` takes `GetFirstTokenBehindAttributeLists`, which is `Modifiers[0]` whenever there are modifiers.
  - The token-list overload removes every accessibility keyword through `RemoveAccessibilityModifiers`, whatever its index.
  - So `static volatile⏎public` and an attribute list in front reach the same code as `static⏎public`.
- **Fix All.** `RH2005FieldsMustBePrivateCodeFixProvider.GetFixAllProvider` returns `WellKnownFixAllProviders.BatchFixer`, which runs the same `ApplyCodeFixAsync` per diagnostic. The class's Fix All scenario already covers that path.
- **Trailing trivia of a non-first `public`** (`static public /* c */ int`). The comment shares a line with the tokens around it, and SHARED-FIX-003's in-place condition says that "a comment between two tokens of one line" is not decided by it. It is not a member of this class.
- **RH2004** cannot reach this class: it reports only declarations without an accessibility keyword (auditor).

## thoenissen/Reihitsu#1286

### Tests

These are in `Design/RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests.cs`. The finding's class spans these axes:
- the trivia spans the field builder replaces (the `;` and the name) and the spans it keeps (the type and the modifiers);
- the trivia kind: comment line, `#pragma` line, or `#if` block;
- LF and CRLF;
- each side of the condition (FC-003).

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `VerifyNoFixIsOfferedForACommentLineInFrontOfTheSemicolon` | RH2001-007; FC-001, FC-004 | `VerifyNoCodeFixIsOffered` | fails (2a) |
| `…CarriageReturnLineFeed` | RH2001-007; FC-017 | same, CRLF | fails |
| `VerifyNoFixIsOfferedForADirectiveLineInFrontOfTheSemicolon` | RH2001-007; FC-005 | `VerifyNoCodeFixIsOffered` | fails |
| `…CarriageReturnLineFeed` | RH2001-007 | same, CRLF | fails |
| `VerifyNoFixIsOfferedForAConditionalBlockInFrontOfTheSemicolon` | RH2001-007; FC-020 | `VerifyNoCodeFixIsOffered` | fails |
| `VerifyFixIsOfferedForASemicolonOnItsOwnLineWithoutCommentOrDirective` | RH2001-007; FC-003 | `VerifyCodeFixIsWithheldOrProducesOneOf`, not withheld | passes |
| `VerifyFixKeepsACommentLineInFrontOfTheName` | SHARED-FIX-003; FC-004 | same, may be withheld | fails (2b) |
| `…CarriageReturnLineFeed` | SHARED-FIX-003 | same, CRLF | fails |
| `VerifyFixKeepsADirectiveLineInFrontOfTheName` | SHARED-FIX-003; FC-005 | same, may be withheld | fails |
| `…CarriageReturnLineFeed` | SHARED-FIX-003 | same, CRLF | fails |
| `VerifyFixKeepsAConditionalBlockInFrontOfTheName` | SHARED-FIX-003; FC-020 | same, may be withheld | fails |
| `VerifyFixIsOfferedForANameOnItsOwnLineWithoutCommentOrDirective` | RH2001-004; FC-003 | same, not withheld | passes |
| `VerifyFixKeepsACommentLineInFrontOfTheType` | SHARED-FIX-003 | same, may be withheld | passes (a member that holds; it guards the guard restructure of decision 17) |
| `VerifyFixKeepsACommentBehindTheTypeOnTheSameLine` | SHARED-FIX-003; FC-021 | same, may be withheld | passes (a member that holds) |

### Not covered, and why
- **A `;` without an initializer.** No such member exists: an auto-property without an initializer has no `;`.
- **A comment in front of `;` on the initializer's line** (`= 5 /* c */;`). It is the trailing trivia of the initializer's last token, so it lies inside the initializer, which RH2001-007 excludes and the builder keeps.
- **The positions RH2001-007 already lists.** These are the identifier's trailing trivia, the accessor list, and the `}`/`=` gaps. They are covered by the earlier tests (auditor's *Assertion adequacy*).

## thoenissen/Reihitsu#1287

### Tests

The finding's class spans these axes:
- the rule that uses `PartialMemberUtilities` (RH2002, RH2004, RH8306, RH7401–RH7412);
- the member kind;
- which part is generated: defining, implementing, or both;
- the document order.

The failing cells are the generated implementing part of an RH2004 constructor, and the generated defining part in RH7409–RH7412. I added the cells next to them that hold, because decision 16 replaces the shared mechanism.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH2004 `VerifyPartialConstructorWithGeneratedImplementingPartIsReportedOnTheDefiningPart` | SHARED-DIAG-012; FC-016, FC-024 | `Verify` + `P.g.cs` | fails (4a) |
| RH2004 `VerifyPartialConstructorWithGeneratedDefiningPartIsReportedOnTheImplementingPart` | SHARED-DIAG-012 | `Verify` + `P.g.cs` | passes |
| RH2004 `VerifyPartialConstructorWithBothPartsGeneratedIsNotReported` | SHARED-DIAG-012 | `Verify`, two `*.g.cs` | passes |
| RH2004 `VerifyPartialConstructorInTwoDocumentsIsReportedOnTheDefiningPartIn{First,Second}Document` | SHARED-DIAG-012 | `Verify`, two documents | pass |
| RH2004 `VerifyPartial{Indexer,Event}WithGenerated{Implementing,Defining}PartIsReportedOnThe{Defining,Implementing}Part` (4 tests) | SHARED-DIAG-012 | `Verify` + `P.g.cs` | pass |
| RH8306 `VerifyPartialPropertyWithGeneratedImplementingPartIsReportedOnTheDefiningPart` | SHARED-DIAG-012 | `Verify` + `C.g.cs` | passes |
| RH7409 `VerifyPartialMethodWithGeneratedDefiningPartIsReportedOnItsImplementingPart` | SHARED-DIAG-012; FC-002, FC-016 | `Verify` + `C.g.cs` | fails (4b) |
| RH7410 `VerifyPartialPropertyWithGeneratedDefiningPartIsReportedOnItsImplementingPart` | SHARED-DIAG-012 | same | fails |
| RH7411 `VerifyPartialEventWithGeneratedDefiningPartIsReportedOnItsImplementingPart` | SHARED-DIAG-012 | same | fails |
| RH7412 `VerifyPartialIndexerWithGeneratedDefiningPartIsReportedOnItsImplementingPart` | SHARED-DIAG-012 | same | fails |
| RH7409–RH7412 `VerifyPartial{Method,Property,Event,Indexer}WithGeneratedImplementingPartIsReportedOnItsDefiningPart` (4 tests) | SHARED-DIAG-012 | same | pass |
| RH7401–RH7404 `VerifyPartialOverride{Method,Property,Event,Indexer}WithGeneratedDefiningPartIsReportedOnItsImplementingPart` (4 tests) | SHARED-DIAG-012 | same | pass |
| RH7405–RH7408 `VerifyPartialNonOverride{Method,Property,Event,Indexer}WithGeneratedDefiningPartIsReportedOnItsImplementingPart` (4 tests) | SHARED-DIAG-012 | same | pass |
| RH7405 `VerifyPartialNonOverrideMethodWithGeneratedImplementingPartIsReportedOnItsDefiningPart` | SHARED-DIAG-012 | same | passes |

In every RH74 test, both parts lie outside a matching region (RH7401–RH7404, RH7409–RH7412), or both lie in an override region (RH7405–RH7408). Each test therefore stays within the coverage sentence of its rule, "each of its parts, judged on its own, would be reported".

### Not covered, and why
- **RH74 only-some-parts cells** (for example, the implementing part in region `I` with the defining part generated outside it). They are not covered by RH7409-001 to RH7412-001; that is #1334's question.
- **Generated implementing part for RH7402–RH7404 and RH7406–RH7408.** For these the defining part is the selected part, and it is analyzed by every RH74 rule. The pre-filters (`OverriddenX`, `IsRelevantNonOverrideMember`) hold for it. Each base is pinned by one test: RH7401 has the existing test, and RH7405 has the new one.
- **RH2002.** Both generated cells already have tests.
- **Partial methods in RH2004.** They are not reported (RH2004-002).
- **Parts marked only by `[GeneratedCode]`.** Dropped by decision 19.

## thoenissen/Reihitsu#1288

Nothing to do: the audit has no finding for it.

## Commands

```shell
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Design.RH2001|FullyQualifiedName~Reihitsu.Analyzer.Test.Design.RH2004|FullyQualifiedName~Reihitsu.Analyzer.Test.Design.RH2005|FullyQualifiedName~RH8306|FullyQualifiedName~Reihitsu.Analyzer.Test.Formatting.RH74"
scripts/test.sh --project architecture
```

Result on `b916957` plus these tests: focused run 22 failed, 350 passed; whole analyzer project 22 failed, 7129 passed; architecture 115 passed.
