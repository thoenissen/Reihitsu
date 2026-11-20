Role: test implementer
Commit: f4a0114
Verdict: tests written

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

Wording change only: no requirement changes (change doc `sdd/changes/2026-11/rh5112-rh5048-member-access-wording.md`, Accepted), so no new test carries a `[Requirement]` tag. Expected strings come from the maintainer's answers 1-3 in `00-lead.md` and the change doc's *Visible output changes* 1-3, not from code. Each new test pins one visible string as a literal; before this run, only the titles were pinned (indirectly, against the rule-page headings), and the message formats and the RH5112 code-fix title were pinned by nothing.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzerTests.VerifyTitleNamesFirstMemberAccess` | — (visible text, change doc item 1) | descriptor `Title` | fail: title is "Wrapped fluent calls should keep the first call on the original line." |
| `RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzerTests.VerifyWrappedPropertyFirstLinkIsReportedWithMemberAccessMessage` | — (visible text, change doc item 1; input of item 1) | `Verify(source, fixedSource, …)` with a literal message | fail on the message only ("Wrapped fluent calls should keep the first call on the original line."); with `MessageFormat` substituted the diagnostic location and fix output pass, so nothing but the text is in question |
| `RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzerTests.VerifyCodeFixTitleNamesFirstWrappedMemberAccess` | — (visible text, change doc item 3) | `GetCodeFixActionsAsync` | fail: one action offered, titled "Keep first wrapped fluent call on original line" |
| `RH5048CommentsMustNotBePlacedBeforeFirstCallOfWrappedChainAnalyzerTests.VerifyTitleNamesFirstMemberAccess` | — (visible text, change doc item 2) | descriptor `Title` | fail: title is "Comments must not be placed before the first call of a wrapped chain" |
| `RH5048CommentsMustNotBePlacedBeforeFirstCallOfWrappedChainAnalyzerTests.VerifyCommentBeforeWrappedPropertyFirstLinkIsReportedWithMemberAccessMessage` | — (visible text, change doc item 2; input of item 2) | `Verify(source, …)` with a literal message | fail on the message only; passes with `MessageFormat` substituted |
| `AnalyzerPackageMetadataTests.AnalyzerTitleResourcesMatchRuleDocumentationTitles` (existing, unchanged) | — | metadata | fail on RH5048 and RH5112 only, until the resources change |

Punctuation as answered: RH5112 strings end with a period, RH5048 strings do not. The title tests compare exactly (no `NormalizeRuleTitle`), so they also pin the punctuation the metadata test ignores.

## Existing tests that assert a changed output

`rg` over `src/*Test*` for each old string ("Wrapped fluent calls should keep the first call", "first call of a wrapped chain", "Keep first wrapped fluent call") finds no test literal. Every RH5112/RH5048 diagnostic test reads `MessageFormat` from the analyzer instance (`AnalyzerTestsBase{TAnalyzer}.cs:30`), so none of them changes. `AnalyzerTitleResourcesMatchRuleDocumentationTitles` and `PackageReadmeDescriptionsMatchRuleDocumentationTitles` are left as they are; the first fails as intended until the implementer edits the resources. Test `<summary>` comments that say "first call" are left unchanged (skeptic's recommendation; most of those inputs do use a call as first link).

## Not covered, and why

- Code-fix output, Fix All, idempotency, CRLF, comment/directive in the gap, formatter parity: no logic changes; reporting, locations, and the fix rewrite stay (skeptic, *Surfaces*). Existing tests cover them.
- Failure classes in `squad/failure-classes.md`: none reaches a resource-string change; no analyzer, fix, or formatter code path changes.
- Rule pages, package README, spec titles: changed by the spec author and covered by `AnalyzerPackageMetadataTests` and `RuleDocumentationConventionTests` (and architecture tests, 198/198 passing on this head with my tests).
- `<summary>` comments of the analyzer classes: not test-observable.

## Commands
```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~MemberAccess|FullyQualifiedName~AnalyzerPackageMetadataTests"
```
On this head: 6 failed (the 5 new tests and `AnalyzerTitleResourcesMatchRuleDocumentationTitles`), 62 passed.

## Follow-up candidates
_None._ (The RH5201 candidate is already in `00-lead.md`.)
