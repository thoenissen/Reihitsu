Role: auditor
Commit: 6691572f558348d2574e9494ef94024a3d8bc59d
Verdict: pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Full audit. Head `6691572f558348d2574e9494ef94024a3d8bc59d` (checked out, clean tree). Base `origin/main` = `eed7195c0b6fb583ab4271173b7a4ad022851c5f`, the SHA in the prompt; it is an ancestor of head. The diff (32 files) matches what handovers 01–08 describe:
- production: `src/Reihitsu.Core/EmptyTypeDeclarationSemicolonAnalysisUtilities.cs` and `src/Reihitsu.Formatter/Pipeline/StructuralTransforms/Rewriter/EmptyTypeDeclarationSemicolonTransform.cs`;
- specs: SHARED-EMPTY-001/002/003/005, FMT-STRUCT-017/018, RH5412–RH5416 and RH5418 intros, `requirement-ids.txt`;
- the change doc;
- tests;
- `sdd/work/`.

`scripts/lane-check.sh behavior-change`: confirmed.

Issue covered: thoenissen/Reihitsu#1275 (`agent-filed`).
- Instance 1 (`};` reported, fixed, stray `;` removed) is implemented.
- Instances 2 and 3 (directives in the body), the RH5416 test change, the rule-page item, and the SHARED-TRAIL-002/DIAG-008 references were dropped by lead decision 1. The change doc's *Decisions* records this, and the dropped goals leave the `main` output unchanged.

How the audit was done: static tracing, plus two probe runs on 15 fixtures outside the repository (`/tmp/claude-0/-home-user-Reihitsu/448bcde3-c0aa-5d72-9258-bd1af9ed1167/scratchpad/fx`). The fixtures cover:
- a block namespace;
- doc comment + attribute + `partial` + generic constraint;
- a sibling behind `;` on the same line;
- a tab before `;`;
- `#if DEBUG` around the whole declaration;
- `}; /* x */ class B;`;
- no final newline;
- `#pragma` between `}` and `;`;
- `} ;  // c`;
- a body comment with `};`;
- a base list;
- two `};` on one line;
- `}⏎⏎;` behind usings;
- `}; // c1` followed by `// c2`;
- a nested `class I { };` between members.

Results:
- `scripts/apply-fix.sh RH5412`: 24 fixed, 4 no-fix-offered (`#pragma` between; body comment), 2 no-diagnostic (`#if DEBUG` excluded), 0 not-converged, 0 line-ending drift. Each fix converged in one iteration per diagnostic.
- `scripts/format.sh` twice over the same fixtures plus `record struct S { };`, `} /** d */;`, and a CRLF `}⏎;`: the first pass formatted 17 of 18, the second pass 0 of 18, and the outputs are byte-identical.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| _None_ | | | | | | | |

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| A declaration has a brace body (reported) vs is a semicolon declaration (`ShouldReport`) | `OpenBraceToken` kind (`None` = semicolon declaration), `OpenBraceToken`/`CloseBraceToken` `IsMissing` | `OrderingDeclarationUtilities.DeclarationHoldsConditionalDirective` (`OpenBraceToken.IsKind(None)` → semicolon declaration); `RH2111` (`OpenBraceToken.IsKind(OpenBraceToken)`) | Same criterion. The old `SemicolonToken` criterion now survives only in RH5404 (see *Policy ownership*) | ok |
| Removing braces and `;` discards trivia (`HasMeaningfulBodyTrivia`, withholds fix and conversion) | header token trailing, `{` leading and trailing, `}` leading; with a `;` behind `}` also `}` trailing and `;` leading | Trivia the transform drops. It strips only whitespace and line breaks from the header token. It discards the trivia of `{` and of `}`'s leading trivia. When a `;` follows, it also discards `}`'s trailing trivia and the `;`'s leading trivia. It carries the `;`'s trailing trivia, or `}`'s trailing trivia when there is no `;` | The inspected span equals the discarded span. Carried trivia is not inspected, which is correct. The "that follows it" qualifier (FC-002) is the `SemicolonToken.IsKind(SemicolonToken)` condition | ok |
| FMT-STRUCT-018 / SHARED-EMPTY-005 wording "between the closing brace and the semicolon" | spec text: anything other than whitespace and line breaks | `ContainsNonFormattingTrivia` (everything but `WhitespaceTrivia`/`EndOfLineTrivia`) | Same. Both boundaries are tested: `}; // note` is carried, and `} // note⏎;` and `}⏎// note⏎;` are withheld | ok |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Report / convert an empty type body | `EmptyTypeDeclarationSemicolonAnalysisUtilities.ShouldReport`/`CanConvertSafely`. Callers: `EmptyTypeDeclarationShouldUseSemicolonAnalyzerBase.OnTypeDeclaration`, `EmptyTypeDeclarationShouldUseSemicolonCodeFixProviderBase.RegisterCodeFixesCoreAsync`, and the five `EmptyTypeDeclarationSemicolonTransform.Visit*Declaration` | single owner |
| "A type declaration with a `SemicolonToken` is a semicolon declaration" | `RH5404ElementMustNotBeOnSingleLineAnalyzer.OnDeclaration` (`src/Reihitsu.Analyzer/Rules/Layout/RH5404ElementMustNotBeOnSingleLineAnalyzer.cs:52`); no longer in Core | Pre-existing second statement of the old criterion. The run does not depend on it: RH5404's fix output changes only through the formatter, which uses the Core owner. Already follow-up candidate 1 in `00-lead.md`, which names this line. Not a finding |
| Trailing trivia of the new semicolon | `EmptyTypeDeclarationSemicolonTransform.CreateSemicolonToken` only; the code fixes reach it through `ReihitsuFormatter` | single owner |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| Formatter `EmptyTypeDeclarationTrailingSemicolonTests` conversions; `StructuralTransformRequirementTests.EmptyTypeBodyFollowedByASemicolonBecomesOneSemicolon`; `UnionDeclarationStructuralTests.EmptyUnionBodyFollowedByASemicolonBecomesOneSemicolon` | exact output, exactly one `;`, comment carried | any other text, under LF or CRLF, on the first or second pass | `AssertRuleResult`: exact text under LF and CRLF, line-ending check, second pass, `PhaseStabilityCheck` | adequate |
| `…KeepsTheBraces` (line comment, own-line comment, directive, disabled text) | braces, trivia, and `;` kept in order; stable | a missing or reordered brace, trivia, or `;`; a second pass that differs | private `AssertBracesAndTriviaAreKept` (whitespace-stripped equality under LF and CRLF, plus `AssertSecondPassIsStable`) | Adequate for FMT-STRUCT-018, which decides no column. Columns are deliberately unpinned (lead decision 4, follow-up 2) |
| `…DocumentationCommentBetweenClosingBraceAndSemicolonKeepsTheBraces` (None/Parse/Diagnose); `EmptyTypeBodyWithCommentOrDirectiveInsideKeepsItsBraces` (`Behind`) | unchanged output in every documentation mode | any change | `AssertRuleResult(input, null, …)` | adequate (FC-040) |
| C# 11 gates (formatter class kept, record converted; analyzer class not reported, C# 12 reported) | both sides of the version gate with `;` | a report or conversion on the wrong side | `AssertRuleResult` / `Verify` + `ApplyLanguageVersion` | adequate (FC-003, FC-006) |
| RH5412 fix tests (`VerifySpecificationExampleOfSemicolonBehindTheBracesIsFixed`, whitespace/blank line, block comment, nested, two on own lines) | diagnostics on the name; fixed text; Fix All in one iteration | wrong text, location, count, or iteration count | `Verify` (single fix, iterative, Fix All) | adequate |
| RH5412 `VerifySemicolonOnTheLineBelowTheClosingBraceIsFixedWithTheDocumentLineBreaks`; RH5404 `VerifyNestedEmptyTypeFollowedByASemicolonConvergesToSemicolonDeclaration` | raw output keeps the document line break | LF in a CRLF document, or any other text | `VerifyCodeFixWithEndOfLine` (raw, no normalization) | adequate (FC-033) |
| RH5412 withheld tests (comment on the `}` line, own-line comment, `#region`/`#if`, disabled text, `/** */` in three modes) | reported, no action registered | a missing diagnostic or a registered action | `VerifyNoCodeFixIsOffered` | adequate (FC-009) |
| RH5413/RH5414/RH5415/RH5416/RH5418 `…FollowedByASemicolonIsReportedAndFixed` (LF, CRLF) | every kind reported and fixed | per kind: no diagnostic, or other text | `Verify` + `ConvertEndOfLine` | adequate |
| RH5418 `UnionWithCommentBetweenClosingBraceAndSemicolonIsReportedWithoutFix` | reported, no fix | missing diagnostic or applied fix | `Verify(src, src)`, the pattern of the existing union no-fix tests (it needs the union reference) | adequate |
| RH5412 parity `VerifyFormatterFixesEmptyClassFollowedByASemicolon` | formatter output carries no RH5412 diagnostic | diagnostic left after formatting | `FormatterTestsBase<TAnalyzer>.VerifyFormatter` | adequate (FC-013) |
| RH5412 `…ClassWithMemberAndSemicolonDeclarationIsNotReported` (+ `class C { int _x; };`) | not reported | a diagnostic | `Verify` | adequate; with `class B;` it covers both sides of the changed `ShouldReport` boundary |

Visible output decided (check 10):
- The base forms A–D and RH5413–RH5416 are covered by answer 1A.
- RH5418 is covered by 2A.
- Comment or directive between `}` and `;`: reported without a fix (3A); the `/** */`, `#if`, and disabled-text variants are the same class.
- The formatter conversion is covered by 4A.
- RH5404 nested is covered by 5A.
- `} ;`, `}⏎⏎;`, and `}; /* note */` follow SHARED-EMPTY-005 under 1A's answer (the stray `;` is folded into one, and a comment behind it follows).
- The formatter's `class A;⏎class B;` for two declarations on one line is the line-break layout that `main` already gives without `;`.
- Rule pages are unchanged.

No undecided expected value.

## Failure classes dismissed
- FC-007, FC-008: each fix converges in one iteration per diagnostic (apply-fix probe, `Verify`). The two-iteration Fix All for two reported declarations on one line exists on `main` for the same rule without `;` (05-implementer.md probe), so it is not caused by this PR; it is follow-up 3.
- FC-009: every withheld case asserts that no action is registered.
- FC-010, FC-012: `AssertRuleResult` runs a second pass and the phase-stability check under LF and CRLF; the probe's second format pass changed 0 files.
- FC-011: no loop is touched.
- FC-014: the structural phase owns the construct.
- FC-015, FC-016: single owner; RH5404's copy is follow-up 1.
- FC-018, FC-036: no rename.
- FC-022, FC-023, FC-025, FC-029: no literals, identifiers, expression bodies, or key/value layout in the changed decision.
- FC-024: `partial` with constraints converts as without `;` (probe f02).
- FC-026: the transform rewrites nodes, not positions; two declarations on one line are tested in the formatter.
- FC-027, FC-039: nested in a type, in a block namespace, and in the global namespace (tests and probes); `base.Visit*` runs first.
- FC-028, FC-041: the node-level and document paths run the same transform; no precheck change.
- FC-030: covered by answer 5A.
- FC-031: the summaries of `ShouldReport`'s inline comment, `HasMeaningfulBodyTrivia`, `CreateSemicolonToken`, and `ConvertToSemicolonDeclaration` describe the new code.
- FC-032: the cleaner built and tested after the last compiled change (715d3df).
- FC-034, FC-035: the tests use the reported shape and failed before the implementation (04-test-implementer.md).
- FC-037: no text-only claim.
- FC-038, FC-040 (beyond the tested `/** */`), FC-042, FC-043:
  - no shape is declared;
  - `///` between `}` and `;` is non-whitespace trivia in every mode, so the outcome does not depend on the mode;
  - the code fix asks `FindUnsupportedSyntax` about the declaration it formats, unchanged.
- FC-044: the guard is a cheap trivia check.
- An extension block is a `TypeDeclarationSyntax`, but no `Visit*` override or analyzer registration reaches it, and `ShouldReport` is kind-gated. An enum is not dispatched.

## Architecture findings
_None._

## Hints
- The change doc's *Decisions* bullet "leaves a declaration with a comment or directive between `}` and `;` as written" holds for the braces and for `} /* note */;`. With a comment or directive on its own line, the formatter still indents the `;` and, for a comment, adds a blank line in front of it. This happens on `main` and on head alike (probe f08: `}⏎#pragma warning disable CS0001⏎    ;`), so no output changes. It is follow-up candidate 2.

## Follow-up candidates
_None_ new. Candidates 1–3 in `00-lead.md` stand. For candidate 1, the second statement of the old criterion is `RH5404ElementMustNotBeOnSingleLineAnalyzer.cs:52`. Its structural cause is two owners of "is a semicolon declaration", Core and RH5404.
