Role: skeptic
Commit: 2523a56b15b61a3f01252fb6d9294b46ceec576b
Verdict: proceed

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Premise

Real. The analyzers report every first link, not only calls. Neither analyzer checks for invocation:

- `RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzer.AnalyzeChain` reports on `chain.FirstLink.OperatorToken` when `FirstLink.StartsLine` holds and `IsFirstLinkBlocked` does not.
- `RH5048CommentsMustNotBePlacedBeforeFirstCallOfWrappedChainAnalyzer.AnalyzeChain` reports each ordinary comment in `RootLastToken.TrailingTrivia` and `FirstLink.OperatorToken.LeadingTrivia` when `FirstLink.StartsLine` holds.

SHARED-CHAIN-001 defines a link as "a member access or member binding together with its operator `.`, `?.`, `!.`, or `!?.`", so "member access" is the spec's own term for what both rules report. "Call" is not.

Evidence that the call-less case is reported today. I ran `scripts/test.sh --project analyzer --filter "FullyQualifiedName~AnalyzerPackageMetadataTests|FullyQualifiedName~VerifyWrappedFirstLinkOfCallLessChainIsFixedInOneApplication|FullyQualifiedName~VerifyCommentBeforeWrappedFirstLinkOfCallLessChainIsReported|FullyQualifiedName~VerifyWrappedPropertyFirstLinkIsFixedInOneApplication"`. Result: 10 passed and 1 failed.

- `VerifyWrappedFirstLinkOfCallLessChainIsFixedInOneApplication` passes. It reports RH5112 on the `.` of `return order` ⏎ `.Customer` ⏎ `.Address;`.
- `VerifyCommentBeforeWrappedFirstLinkOfCallLessChainIsReported` passes. It reports RH5048 on `// Comment` in `return order // Comment` ⏎ `.Customer` ⏎ `.Address;`.
- `AnalyzerTitleResourcesMatchRuleDocumentationTitles` fails, as the spec author expects. It names exactly RH5048 and RH5112, because the resources still say "first call" while the rule pages now say "first member access". Nothing else mismatches.

Caller: every analyzer report shows the title and message, and the IDE light bulb shows the RH5112 code-fix title. So any document with a wrapped call-less chain reaches the case. No regression is claimed, so I checked no history.

`scripts/test.sh --project architecture --filter "FullyQualifiedName~Sdd"` passes 70 of 70 on the spec diff.

## Mechanism and candidates

_N/A._ This is a wording change, not a bug. For the record, the first-link kinds the reworded text has to describe (SHARED-CHAIN-001, `FluentChainLink`) are:

- a member access with `.`, invoked or not;
- a member binding with `?.`;
- a member access with `!.`;
- a member binding with `!?.`.

"Member access" covers all four. An element access, a conditional element access, and a bare null-forgiving operator are attached parts, not links. RH5115 and RH5116 report those. The RH5112 page already says "An index access such as `?[0]` is not a member access and is not reported", which fits the new wording.

## Breaking inputs

None. No reporting condition, location, or fix output changes. FC classes do not apply: no analyzer, formatter, or code-fix logic is touched. Coverage for the tests:

- `AnalyzerTitleResourcesMatchRuleDocumentationTitles` and `PackageReadmeDescriptionsMatchRuleDocumentationTitles` pin the titles against the rule-page headings.
- The diagnostic tests read `MessageFormat` from the analyzer instance (`AnalyzerTestsBase{TAnalyzer}.cs:30`), so a message change cannot break them and no test pins the new message text.
- The RH5112 code-fix title (`CodeFixResources.RH5112Title`, used at `RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineCodeFixProvider.cs:54`) is pinned by nothing except `CodeFixResourcesTests.AllPropertiesShouldNotBeNullOrEmpty`.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `AnalyzerResources.resx` `RH5112Title`, `RH5112MessageFormat`, `RH5048Title`, `RH5048MessageFormat` | must change |
| `CodeFixResources.resx` `RH5112Title` | must change (it repeats "fluent call") |
| Analyzer `<summary>` comments of the two analyzer classes | must change (they repeat the title; text only) |
| Analyzer reporting logic, locations, severities, IDs | must stay |
| RH5112 code fix and Fix All (rewrite, equivalence key = provider type name) | must stay |
| Type names (`…FirstCall…`) | must stay (renaming changes the equivalence key and the spec code owners for no visible gain) |
| Formatter, `Reihitsu.Core`, CLI | not involved |
| Rule pages, package README, spec titles, RH5038 and comment-placement non-goal pointers | changed by the spec author; consistent with each other (architecture Sdd tests pass) |
| Localized resources | not involved: only the two neutral `.resx` files exist |
| `binaries/` | not involved (`/bump-version` rebuilds them) |

## Refuse option

There is no input class to refuse: this is a wording change, and declaring a shape unsupported is not an option, because no formatter or fix special case is at stake. The only alternative is "no change": keep "first call". It costs nothing in code, but it leaves a diagnostic on `.Customer` that names a call where there is none. The issue's own alternative is to narrow the rules back to calls. That contradicts SHARED-CHAIN-003, which says "whether it is a call or a property read", and the formatter's join of the first link, so it is rightly rejected.

Narrower option: the issue's literal scope, which is resources plus headings without change doc item 5. I would not take it. RH5048's "Why" would then say "The first call of a chain belongs on the line…" under a heading about the first member access.

## Output questions

The lead asks the change doc's five items in any case. I have two remarks on their wording, both visible on the rule pages and neither a new question.

1. **Change doc item 5, RH5048 "Why".** The new sentence reads "The first member access of a chain belongs on the line of the expression it accesses." A member access accesses a member *of* an expression, not the expression. The RH5112 page's description already says "on the line its receiver ends on". I recommend offering this variant to the maintainer with item 5:

   - Proposed: "The first member access of a chain belongs on the line of the expression it accesses. A comment in front of it keeps the member access apart from its receiver."
   - Recommended variant: "The first member access of a chain belongs on the line its receiver ends on. A comment in front of it keeps the member access apart from its receiver."

   Why: it matches RH5112's description word for word and is accurate for `order.Customer`.

2. **Change doc item 3, code-fix title.** I agree with the spec author's recommendation, "Keep first wrapped member access on original line". It is a pure substitution, and "wrapped" still says which member access.

## Internal choices

- **Change doc without a requirement change.** Agree with keeping it. The lane check needs a change doc for production paths in the behavior-change lane, and the change doc is where the before and after of the visible text live. Without it, the `.resx` edits would put the run in the bug-fix lane, but no spec requirement demands the new text.
- **Punctuation stays as is.** RH5112 keeps its period and RH5048 has none. Agree: `NormalizeRuleTitle` already makes the title test ignore it, and normalizing would be a separate visible change.
- **Type names stay.** Agree (see Surfaces).
- **Test-only doc comments.** Several RH5048 test `<summary>` comments say "before the first call of a wrapped chain" (`RH5048CommentsMustNotBePlacedBeforeFirstCallOfWrappedChainAnalyzerTests.cs:27,57,196,227,258`). Most of those tests do use a call as the first link, so the comments are accurate for their inputs. Rewording them is optional and the test implementer's call; I recommend leaving them.

## Issue goals

- **"Update localized resources via `/add-resource-texts`"**: drop. No localized `.resx` exists; only `AnalyzerResources.resx` and `CodeFixResources.resx` are present. No visible change.
- **"Only the resource strings and page headings remain"**: stale. The RH5112 "Why" and "How to fix" text and the RH5048 "Why" text still said "call". Change doc item 5 widens the goal. That is visible on the rule pages, but it is already in the change doc as its own item to ask.
- **"Code-fix titles … if they repeat the wording"**: keep. `RH5112Title` in `CodeFixResources.resx` says "Keep first wrapped fluent call on original line".
- Every other goal (new titles and messages, IDs and severities unchanged) stands.

## Architecture findings

_None._

## Follow-up candidates

1. **RH5201 · Method chains should be aligned: same wording mismatch for call-less chains.** RH5201 reports misaligned links of a chain without any call, for example:
   - `VerifyMisalignedLinkOfCallLessChainIsReportedAndAlignedUnderTheLastLink` (`RH5201FluentChainLinkFormatterTests.cs:440`)
   - `VerifySplitConditionalOperatorOfCallLessChainIsReported` (`RH5201MethodChainsShouldBeAlignedAnalyzerTests.cs:1929`)

   Its title and message say "Method chains should be aligned." and its code-fix title says "Align method chain". This is outside thoenissen/Reihitsu#897, so it is a follow-up issue that carries the output question.

   Input, taken from that test (`dynamic order`):
   ```csharp
   return order.Customer.Address
               .City;
   ```
   Before (`main`): RH5201 on the `.` of `.City`, "Method chains should be aligned."; the fix is titled "Align method chain".

   After, A (recommended): RH5201 on the same `.`, "Member-access chains should be aligned."; the fix is titled "Align member-access chain".

   After, B: unchanged.

   Why A: the title then describes a chain that has no method call, as #897 does for RH5112 and RH5048.
