Role: skeptic
Commit: 0f48a85
Verdict: proceed (with the goals adjusted below)

My prompt carried no intended fix, suspected cause, or hoped-for verdict, so I had nothing to set aside.

## Premise

Real, but the issue's command and counts are stale.

- **Path.** The issue names `src/*.Test/`, but #1932 moved the test projects to `tests/`. `rg -l 'DiagnosticId, *"' src --glob '*.cs'` finds **0** files today. Over `tests/`, the same pattern matches 262 lines in 23 files.
- **False positives.** 4 of those lines, in 3 files, are not messages. They are command-line argument arrays and must stay as they are:
  - `tests/Reihitsu.ArchitectureTests/General/ApplyFixWrapperPositionalArgumentTests.cs:62` (`[FindScript("apply-fix.sh"), UnknownDiagnosticId, "--", "--no-install"]`)
  - `tests/Reihitsu.Tooling.Test/Unit/CodeFixRunCommandLanguageVersionTests.cs:365`
  - `tests/Reihitsu.Tooling.Test/Unit/CodeFixRunCommandTests.cs:539`, `:561`
- **What remains.** 258 message literals in 20 files of `tests/Reihitsu.Analyzer.Test`, covering 15 rules. Every one of these messages has no placeholders. Each literal equals its `AnalyzerResources.resx` `<ID>MessageFormat` value byte for byte. In each file, the test base's `TAnalyzer` is the analyzer the literal belongs to, so the base's `MessageFormat` property (`tests/Reihitsu.Analyzer.Test/Base/AnalyzerTestsBase{TAnalyzer}.cs:30`) returns exactly that text. All 18 analyzers involved (these 15 plus RH2008/RH2009/RH2111 below) derive from `DiagnosticAnalyzerBase`, directly or through `FluentChainAnalyzerBase`/`LineCountAnalyzerBase`, so the cast in that property holds.
- **Convention.** The repository's convention is `MessageFormat`: 3262 `Diagnostics(<id>, …)` call sites pass `MessageFormat` against 258 that pass a literal. Placeholder messages are already formatted as `string.Format(MessageFormat, …)` in about 10 `CreateMessage` helpers (for example `tests/Reihitsu.Analyzer.Test/Formatting/RH7309RegionsShouldFollowCategoryOrderAnalyzerTests.cs:1696`).
- **Coupling confirmed by running it.** In a throw-away worktree of `0f48a85` under the scratchpad, I changed only `RH3003MessageFormat` in `src/Reihitsu.Analyzer/AnalyzerResources.resx` from `Use string.Empty for empty strings.` to `Use string.Empty for an empty string.`, then ran:

  ```
  scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Clarity.RH3003UseStringEmptyForEmptyStringsAnalyzerTests"
  Failed!  - Failed: 7, Passed: 53, Total: 60
  Expected diagnostic message to be "Use string.Empty for empty strings." was "Use string.Empty for an empty string."
  ```

  The 7 failures are exactly the 7 literal sites in that file. The other 53 tests already use `MessageFormat` and pass. The worktree has been removed.
- **Lane consequence confirmed.** `squad/README.md` (*Lanes*) admits no test path in the wording lane, so such a diff widens to tests. That happened in the runs of #1898 and #1902: `git log -S` shows that the RH5048, RH5112, and RH5201 literals were introduced by those two wording runs.

Reachability (`AGENTS.md`, *Supported inputs*) does not apply: the change is test-only and changes no behavior that any caller sees.

## Mechanism and candidates

_N/A_ (not a bug). For completeness, the defining property is: **a test passes, as an expected text, a string literal equal to (or interpolating) a resource text of `AnalyzerResources.resx` or `CodeFixResources.resx`, instead of reading that resource.**

The issue's `rg` pattern finds only one sink of this class. I derived the candidate sinks from where the test bases accept an expected text, not from the issue:

- `Diagnostics(id, string message, count)`
- `Diagnostics(id, Func<int,string>, count)`
- `.WithMessage(...)`
- `ExpectedDiagnostic(..., message)` (`FormatterTestsBase{TAnalyzer}.cs:89`)
- `Assert.*` on `descriptor.Title` and on `CodeAction.Title`
- free-text asserts in the self-hosting scan

To cover all of them, I searched every resource value of both `.resx` files as an exact C# literal across `tests/`. For placeholder values, I searched their longest fixed fragment instead. Script: a Python loop over `ET.parse(resx).findall("data")` calling `rg -F --glob '*.cs' '"<value>"' tests`. The members it found are listed under *Breaking inputs*.

## Breaking inputs

None of the failure classes in `squad/failure-classes.md` reaches production code, because there is none in this change. FC-017 (an assertion weaker than the guarantee) and FC-035 (expected values taken from the code) are relevant to the test changes; see the last bullet of this section.

Here is every member of the class, with the change I recommend for each. "Pinned by" names what keeps the wording itself under test after the change (`README.md`, *Coverage first*).

| # | Member | Sites | Recommended change |
|---|---|---|---|
| 1 | `Diagnostics(<X>.DiagnosticId, "<literal>"[, n])`, no placeholders | 258 lines, 20 files (below) | Replace the literal with `MessageFormat`. |
| 2 | Placeholder message as an interpolated literal in a helper: `.WithMessage($"This member has {count} lines of code, which exceeds the limit of {limit}.")` | `RH2008MethodsShouldNotExceedTheLineLimitAnalyzerTests.cs:1989`, `RH2009MethodsShouldNotExceedTheComplexityLimitAnalyzerTests.cs:2921`, `RH2111TypesShouldNotExceedTheLineLimitAnalyzerTests.cs:885` (all under `tests/Reihitsu.Analyzer.Test/Design/`) | `string.Format(MessageFormat, count, limit)` (or `complexity, limit`), matching the existing `CreateMessage` helpers. The rg command does not find these, but the issue's purpose covers them. |
| 3 | Self-hosting scan asserts that quote the formatted RH2008/RH2111 message | `tests/Reihitsu.SelfHosting.Test/Analyzers/LineCountSelfHostingTests.cs:108, 123, 138, 153` | Build the message part from the analyzer's public `DiagnosticAnalyzerBase.MessageFormat` with `string.Format`, and keep the scan's own `[RH2008] (<AnalyzerType>) ` prefix literal. The prefix is the scan's format, not wording. |
| 4 | Descriptor-title literal: `Assert.AreEqual("<title>", descriptor.Title.ToString(...))` in `VerifyTitleNamesFirstMemberAccess` | `RH5048CommentsMustNotBePlacedBeforeFirstCallOfWrappedChainAnalyzerTests.cs:627`, `RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzerTests.cs:1803` | See *Internal choices* 1. I recommend deleting both test methods. Once the literal is gone they assert nothing beyond `AnalyzerResourcesTests.EveryAnalyzerPublishesTheResourcesItsDiagnosticIdNames`. The wording stays pinned by `AnalyzerPackageMetadataTests.AnalyzerTitleResourcesMatchRuleDocumentationTitles` (title against the rule-page heading) and `MessageFormatResourcesMatchTitleWhenNoPlaceholders`. |
| 5 | Code-fix title literal: `Assert.AreEqual("Keep first wrapped member access on original line", actions[0].Title)` | `RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzerTests.cs:1871` (`VerifyCodeFixTitleNamesFirstWrappedMemberAccess`) | Replace the literal with `CodeFixResources.RH5112Title`. The test still checks that exactly one action is offered and that it carries the rule's title resource. |

Files and counts for member 1 (all under `tests/Reihitsu.Analyzer.Test/`):

| File | Literal lines |
|---|---|
| `Clarity/RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzerTests.cs` | 52 |
| `Clarity/RH3002StatementMustNotUseUnnecessaryParenthesesAmbiguousShapeTests.cs` | 41 |
| `Clarity/RH3002StatementMustNotUseUnnecessaryParenthesesAssignmentTargetTests.cs` | 28 |
| `Clarity/RH3002StatementMustNotUseUnnecessaryParenthesesReceiverTests.cs` | 26 |
| `Clarity/RH3003UseStringEmptyForEmptyStringsAnalyzerTests.cs` | 7 |
| `Clarity/RH3004UseLambdaSyntaxAnalyzerTests.cs` | 10 |
| `Clarity/RH3004UseLambdaSyntaxMultiLineTokenAndDirectiveTests.cs` | 1 |
| `Clarity/RH3004UseLambdaSyntaxStatementShiftTests.cs` | 1 |
| `Clarity/RH3005UseReadableConditionsAnalyzerTests.cs` | 12 |
| `Clarity/RH3006ConditionalExpressionsMustDeclarePrecedenceAnalyzerTests.cs` | 10 |
| `Clarity/RH3007DoNotUseQuerySyntaxAnalyzerTests.cs` | 11 |
| `Clarity/RH3101DoNotPrefixCallsWithBaseUnlessLocalImplementationExistsAnalyzerTests.cs` | 7 |
| `Clarity/RH3102CodeMustNotContainEmptyStatementsAnalyzerTests.cs` | 8 |
| `Clarity/RH3103UseShorthandForNullableTypesAnalyzerTests.cs` | 17 |
| `Clarity/RH3104DoNotUseDefaultValueTypeConstructorAnalyzerTests.cs` | 9 |
| `Clarity/RH3105DoNotPrefixLocalMembersWithThisAnalyzerTests.cs` | 8 |
| `Clarity/RH3201CommentsMustContainTextAnalyzerTests.cs` | 7 |
| `Formatting/RH5048CommentsMustNotBePlacedBeforeFirstCallOfWrappedChainAnalyzerTests.cs` | 1 (line 653) |
| `Formatting/RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzerTests.cs` | 1 (line 1840) |
| `Formatter/Formatting/RH5201CommentExemptChainWithNonInvokedPrefixDotTests.cs` | 1 (line 160, `VerifyFormatter`) |

Checked and not members:

- No `ExpectedDiagnostic(...)` call passes a literal.
- No `.WithMessage("…")` passes a plain literal.
- Every `Diagnostics(id, index => …)` site goes through `CreateMessage` → `string.Format(MessageFormat, …)`.
- No other test project quotes a resource text.
- No message contains `{{` or `}}`, so the raw `MessageFormat` equals the reported message for every placeholder-free member.

On FC-017/FC-035: after the change, members 1 and 5 no longer fail when the wording itself is wrong. That is the purpose of the change, and the guarantee is not lost. Each analyzer has a single descriptor tied to its resource (`EveryAnalyzerPublishesTheResourcesItsDiagnosticIdNames`), and the wording is pinned against the rule page by `AnalyzerPackageMetadataTests`. Members 2 and 3 keep checking the substituted arguments, which carry their real content. No new test is needed. The existing tests are the coverage, and the validator's full run is the evidence.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Analyzer (`src/Reihitsu.Analyzer`) | must stay, including `AnalyzerResources.resx` |
| Code fixes (`src/Reihitsu.Analyzer.CodeFixes`) | must stay, including `CodeFixResources.resx` |
| Fix All | not involved |
| Formatter | not involved |
| `Reihitsu.Core` | not involved |
| Rule docs (`documentation/rules/`) | not involved |
| Specs (`sdd/specs/`) | not involved; none of the touched tests carries a `[Requirement]` |
| `tests/Reihitsu.Analyzer.Test` | must change (members 1, 2, 4, 5) |
| `tests/Reihitsu.SelfHosting.Test` | must change (member 3), if the lead takes it in |
| `tests/Reihitsu.Tooling.Test`, `tests/Reihitsu.ArchitectureTests` | must stay (false positives of the issue's `rg`) |
| Test bases (`AnalyzerTestsBase{TAnalyzer}`) | must stay; `MessageFormat` already exists |

## Refuse option

Make no change. This costs nothing now but keeps the coupling: every future wording run on one of these 18 rules widens from the wording lane to the tests lane, as the runs of #1898 and #1902 did. The change itself is a mechanical, test-only edit of about 270 lines with no new type and no production line, and it follows the convention of 3262 existing call sites. Refusing is not cheaper in any way that matters.

Narrower option: members 1 and 2 only (the issue's stated scope plus its placeholder clause). Members 3, 4, and 5 are the same class and cost about 8 lines. Leaving them out keeps RH2008, RH2111, RH5048, and RH5112 coupled to the tests.

Declaring a shape unsupported does not apply; nothing is formatted.

## Output questions

_None._ No visible output changes. Diagnostics, code fixes, the formatter, rule pages, and the CLI are untouched.

## Internal choices

1. **Member 4 (descriptor-title literal tests).**
   - (a) Delete both `VerifyTitleNamesFirstMemberAccess` methods. They are redundant with `EveryAnalyzerPublishesTheResourcesItsDiagnosticIdNames` plus `AnalyzerTitleResourcesMatchRuleDocumentationTitles`.
   - (b) Replace the literal with `AnalyzerResources.ResourceManager.GetString("RH5048Title")`. That leaves a tautological test (`AnalyzerResources` has no per-rule properties; `AnalyzerResourcesHoldsNoPropertyPerRule`).
   - (c) Keep the literals as deliberate wording pins. That defeats the issue's purpose for RH5048 and RH5112.

   **Recommend (a).** Deleting a redundant test is not silencing one: the value stays pinned, and the handover names where.
2. **Member 3 (self-hosting asserts).** Take it in, recommended, or leave it as a placeholder literal as the issue allows. Taking it in is the only way a RH2008/RH2111 wording edit leaves the test projects unchanged.
3. **Members 1 and 2: helper or inline.** `MessageFormat` inline at each site, as the existing 3262 sites do. **Recommend inline**; no new helper.

## Issue goals

- **"Test files under `src/Reihitsu.Analyzer.Test/` … (list them with the `rg` command above)"**: stale. Change the path to `tests/`. The `rg` pattern also matches the 4 Tooling/ArchitectureTests argument-array lines, which must stay. No visible output.
- **Done when: "The `rg` command above finds no message literal for a message without placeholders"**: change it to "no test under `tests/` quotes the text of an `AnalyzerResources.resx` or `CodeFixResources.resx` entry as a string literal, or interpolates it". The `rg` command checks one sink of the class and finds 0 under `src`. The resource-literal scan in *Mechanism and candidates* is the check that covers members 1 to 5. No visible output.
- **"Where a message has placeholders, keep the literal or format `MessageFormat` … whichever reads clearer"**: tighten it to "format `MessageFormat`". The goal's own "Done when" ("a rule's wording edit … leaves its test files unchanged") is otherwise false for RH2008, RH2009, and RH2111, and the repository already formats placeholders this way in its `CreateMessage` helpers. No visible output.
- **"About 21 files, 260 lines"**: today the count is 20 files and 258 lines (member 1), plus the 3 + 4 + 2 + 1 sites of members 2 to 5.
- **"`AnalyzerResourcesTests` already ties each `MessageFormat` to its resource entry"**: correct (`EveryAnalyzerPublishesTheResourcesItsDiagnosticIdNames`).

## Architecture findings

_None._

## Follow-up candidates

- **Convention test against recurrence.** The class recurred twice within a few days (#1898, #1902). A `Reihitsu.ArchitectureTests` check would keep it from coming back: it would fail when a `.cs` file under `tests/` holds a string literal equal to a resource value of `AnalyzerResources.resx` or `CodeFixResources.resx`, essentially the scan used here. This is internal only, with no visible output.
