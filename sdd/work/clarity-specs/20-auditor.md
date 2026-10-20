Role: auditor
Commit: afd241dbb9fa6040e8653df43a9acdddee07e653
Verdict: findings

## Scope

Full audit, first round. Head afd241dbb9fa6040e8653df43a9acdddee07e653 against base `origin/main` 6ff1b0f14a45602f1dc76ed609f391b4659a0523.

The prompt listed what to check and carried no intended fix, suspected cause, or hoped-for verdict.

State verified:

- The checkout is the head SHA, and the tree is clean.
- `git ls-remote origin refs/heads/main` returns 6ff1b0f, and it is an ancestor of the head.
- The diff matches the handovers, in 78 files:
  - 28 under `sdd/specs`;
  - `documentation/rules/RH3201.md`;
  - 6 decision records;
  - `sdd/work/clarity-specs/`;
  - 23 test files under `src/Reihitsu.Analyzer.Test/`.
- No production path changed. Neither `main` merge (b4aa7ec, 96ef88c) brought a production file either, so the earlier focused test results still hold.
- `scripts/lane-check.sh tooling` gives `LANE CHECK: confirmed`.

The spec checker did not see the post-15 changes (the second merge and the SHARED-PARENS / SHARED-AFTERBRACE rename), so I re-ran `scripts/test.sh --project architecture` on the head: 72/72 passed. That covers unique IDs, ledger and index, code owners, `Uses`, and requirement coverage.

Further checks:

- **Ledger.** It gains exactly 78 IDs and loses none. It is sorted, and it has no duplicate.
- **Prefixes.** The shared prefixes are unique after the rename.
- **Tags.** None of this run's 23 test files carries a SHARED-EMPTY, SHARED-BRACE, RH3202-002..004, or RH5030-003 tag. `main`'s SHARED-EMPTY tags (RH5412–RH5416) and SHARED-BRACE tags (RH5405–RH5407) are untouched.
- **Assertions.** The only removed lines in the test diff are 19 retired `[Requirement]` attributes, so no assertion changed.
- **Open-question issues.** thoenissen/Reihitsu#1234 and thoenissen/Reihitsu#1238 cite SHARED-PARENS-002.
- **Leftovers.** `sdd/work/` holds only `clarity-specs/`.

Probes were run with `scripts/apply-fix.sh <rule> <dir>` (LF and CRLF, same result). Compile results come from the SDK's `csc.dll` against the net10.0 reference pack, run on the original and the fixed text. Fixtures lived in the session scratchpad and are reproduced inline below.

## Findings

| # | Severity | Location | Failure class | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|
| 1 | high | `sdd/specs/analyzer/RH3103.md`: RH3103-002 ("possibly qualified") and the last sentence of RH3103-001 | FC-002, FC-003 | PR-introduced | **The requirement does not hold.** `string M() => nameof(System.Nullable<int>);` and `string M() => nameof(global::System.Nullable<int>);` are both reported. In expression context the dotted name parses as a `MemberAccessExpressionSyntax`, and `RH3103UseShorthandForNullableTypesAnalyzer.ShouldSkip` only walks up through `QualifiedNameSyntax` and `AliasQualifiedNameSyntax` parents. Only `nameof(S::Nullable<int>)` (with `using S = System;`) is skipped: `no-diagnostic`. **The offered fix throws.** `apply-fix` aborts with `Unable to cast object of type 'NullableTypeSyntax' to type 'SimpleNameSyntax'`: `ApplyCodeFixAsync` replaces the `Name` of a member access with a `NullableTypeSyntax` through `ReplaceNode`. **No test catches it.** The two RH3103-002 tests use only the unqualified form, so the false clause is green and the requirement count passes. | Narrow RH3103-002, and RH3103-001's "The `nameof` argument itself MUST NOT be reported", to the forms that hold: unqualified, and qualified by an alias alone. If the alias-only clause stays, it needs a tagged test (`nameof(S::Nullable<int>)` is untested today). File the dotted forms in thoenissen/Reihitsu#1231: they are reported where `T?` cannot stand, and the fix throws. Pin the report with an untagged characterization test. |
| 2 | medium | `sdd/specs/analyzer/RH3005.md`, RH3005-004 | FC-006, FC-016, FC-027 | PR-introduced | **A fix inside the requirement's scope does not compile.** The input below compiles:<br>`class C { const int Max = 1; int G; int B; void M(bool x, bool y) { } void Run() { M(Max > G, B > (7)); } }`<br>It is reported on `>`. RH3005-005 does not withhold the fix: `Max > G` and the speculatively bound `G < Max` are both built-in. The fix writes `M(G < Max, B > (7));`, which parses as the generic invocation `G<Max, B>(7)`. csc reports `CS0307: The field 'C.G' cannot be used with type arguments` and `CS0246`. The same happens with a `static readonly` operand in an array initializer: `new[] { Min > G, B > (7) }` becomes `new[] { G < Min, B > (7) }`. **The scope was meant to exclude this.** This run narrowed RH3005-004 to keep P1 compile errors out of it (operands that are comparisons, #1227 Q1), and this input is still inside the scope. It is not in thoenissen/Reihitsu#1227, and no test pins it. | Apply the run's own standard, as for RH3004-003: either narrow RH3005-004's scope, or justify keeping it. File the case in thoenissen/Reihitsu#1227 as "Decided by principle P1 (#912), compile error". Add an untagged characterization test. |
| 3 | medium | `sdd/specs/analyzer/RH3001.md`, RH3001-004 | FC-006, FC-016 | PR-introduced | **The parenthesization RH3001-004 requires breaks this input.** The input below compiles:<br>`class T { public static bool operator >(T a, bool b) => b; public static bool operator <(T a, bool b) => b; }`<br>`class C { int G; int A; T B; void M(bool x, bool y) { } void Run(bool x) { M(G < A, B > !x); } }`<br>The fix writes `M(G < A, B > (x == false));`, which parses as `G<A, B>(x == false)`. csc reports `CS0307` and `CS0246`. The input is contrived: it needs a user-defined comparison with a `bool` operand. Still, RH3001-004 states this parenthesization as a MUST. No `SDD open questions: sdd/specs/analyzer/RH3001.md` issue exists, and no test pins the case. | Scope RH3001-004, or justify keeping it. File a new `SDD open questions: sdd/specs/analyzer/RH3001.md` issue (P1, compile error). Add an untagged characterization test. |
| 4 | medium | `sdd/specs/analyzer/RH3105.md` RH3105-004; `sdd/specs/analyzer/RH3101.md` RH3101-004 | FC-006, FC-016, FC-027 | PR-introduced | **Removing the qualifier creates a generic reading.** The RH3105 input below compiles:<br>`class C { int A; int B; int D; void M(bool x, bool y) { } void Run() { M(A < this.B, D > (7)); } }`<br>It is reported on `this` (speculative binding of `B` alone gives the same field). The fix writes `M(A < B, D > (7));`, a generic invocation: `CS0246`. The RH3101 input fails the same way:<br>`class P { protected int B; } class C : P { int A; int D; void M(bool x, bool y) { } void Run() { M(A < base.B, D > (7)); } }`<br>Its fix writes `M(A < B, D > (7));` and gives `CS0246`. Both requirements state the replacement without a scope. The case is in neither thoenissen/Reihitsu#1233 nor thoenissen/Reihitsu#1229, and no test pins it. **RH3002 is the only rule that guards against this.** RH3002-004's analyzer logic does not report the single-pair form `M(A < B, (D > (7)))` (`no-diagnostic`). | File both cases as P1 compile errors in thoenissen/Reihitsu#1233 and thoenissen/Reihitsu#1229. Add untagged characterization tests. Whether to scope the requirements is the owner's call, consistent with how RH3101-004 was left for thoenissen/Reihitsu#1229 Q1. |
| 5 | medium | `src/Reihitsu.Analyzer.Test/Clarity/RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzerTests.cs`, `ParenthesesAroundAWholeExpressionAreReportedInEveryListedPosition`; RH3002-001 | FC-017, FC-027 | same requirement | **Three listed positions have no test.** RH3002-001 lists an argument of an object creation, an argument of an element access, and the value after `=` in a member declaration. No test in the class covers any of the three: by grep, no `new X({\|…`, no `x[{\|…`, and no field or property initializer with markup. The test named "…InEveryListedPosition", copied from the spec example, covers only the other positions. All three hold today: `apply-fix` fixes `new C((a))`, `x[(a)]`, `int _f = (1);`, and `int P { get; } = (2);`. A regression in these clauses would therefore stay green. | Add the three positions to a test tagged RH3002-001, and to the spec example if the test stays a copy of it. Otherwise, rename the test so it does not claim every listed position. |
| 6 | low | `sdd/specs/analyzer/RH3002.md`, RH3002-001: "a parenthesized expression directly inside another pair of parentheses" | FC-002 | PR-introduced | Read literally, the inner `(c)` of `if ((c))` stands directly inside another pair of parentheses, namely the statement's. The sentence then claims thoenissen/Reihitsu#1224 Q1's statement-condition cases as reported, and they are not. Probe results: `if ((c))` is not reported, and `while (((c)))` loses only its innermost pair. The non-goal says a statement's parentheses "are not a parenthesized expression", but this sentence says "pair of parentheses". It is text only. | Write "directly inside another parenthesized expression". |
| 7 | low | `src/Reihitsu.Analyzer.Test/Clarity/RH3103UseShorthandForNullableTypesAnalyzerTests.cs`, summary of `NullableGenericInEveryListedTypePositionIsReportedOverTheWholeName` | FC-031 | PR-introduced | The summary calls `global::System.Nullable<long>` "alias-qualified". `13-spec-author.md` removed that term from RH3103 because RH3103-003 excludes names "qualified by an alias alone". The test carries RH3103-003, so the summary reads as if it covered the excluded form. The tag itself is right: the input has only dotted names. This is a comment and cannot reach compiled behavior. | Write "qualified with dots (also behind `global::`)". |

## Guard scope

No production predicate changed. These rows compare each requirement's scope with the code predicate that decides the same input.

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| RH3103-002: the `nameof` argument is exempt | Spec: the argument of `nameof`, "possibly qualified" | `RH3103…Analyzer.ShouldSkip`: walks `QualifiedNameSyntax` / `AliasQualifiedNameSyntax` parents up to an `ArgumentSyntax` of `nameof` | Disagree: a dotted name in expression context has a `MemberAccessExpressionSyntax` parent | Finding 1 |
| RH3005-004 fix scope | Spec: comparisons whose operands are not comparisons and that RH3005-005 does not withhold | `IsSwapSemanticsPreserving`: speculative binding of the swapped text alone; no re-parse in its surroundings | Disagree when a new `<` forms a generic name with the tokens after it | Finding 2 |
| RH3001-004 parentheses | Spec: added where the comparison binds differently | `Simplifier.Annotation` on the generated `ParenthesizedExpression`: decided by tree precedence, blind to the argument list around it | Disagree for `> (` after an earlier `<` | Finding 3 |
| RH3101-001/-004, RH3105-001/-004: the qualifier is redundant | Spec: the name alone binds to the same symbol | `SpeculativeRebindingHelper`: binds the name node alone | Disagree when the bare name joins a generic reading | Finding 4 |
| RH3201-001/-002 neighbor lines | Spec: the line above or below starts with a comment / with `//` that is not `///` | `IsBlankLineInsideSingleLineCommentBlock` + `IsSingleLineCommentLine` (`TrimStart`, `//`, not `IsDocumentationLine`); `//` comments only | Agree. Every RH3201-001-tagged input meets the condition (checked all 10). | ok |
| RH3002-009 | Spec: between `(` and `)` | `InteriorContainsCommentOrDirective(node.Span)` | Agree | ok |
| RH3004-004 | Spec: start of the method to the parameter list, and the parameter list to the body | `ContainsDiscardedTrivia`, same two spans | Agree | ok |
| RH3005-006 | Spec: between the comparison's first and last token | `InteriorContainsCommentOrDirective(binary)` | Agree | ok |
| SHARED-BODY-001 | Spec: FMT-STRUCT-007 | `ExpressionBodyRewriteUtilities.BlocksRewrite` (directive from arrow to expression; unbalanced conditional from arrow to `;`; region from the arrow's full start to `;`) | Agree | ok |
| SHARED-AFTERBRACE-001 | Spec: start of the leading trivia, or after the last directive | `FindIndexAfterLeadingDirectives` (index after the last directive) + `DetectEndOfLine`; text equals RH5030-003's word for word | Agree | ok |
| SHARED-PARENS-002 | Spec: a whitespace-only list; a comment in front of `(` stays | `RemoveNode(…, KeepExteriorTrivia)` in both providers | Agree | ok |
| RH3104-002 | Spec: a struct that declares its own parameterless constructor | `HasUserDefinedParameterlessConstructor` (`IsImplicitlyDeclared == false`) | Agree. Boundary run: a struct with a field initializer and only a parameterized constructor prints `0 0` for `new S().X` and `default(S).X`. | ok |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Whether an expression body can be converted | `ExpressionBodyRewriteUtilities.BlocksRewrite` is called by `ExpressionBodyAnalyzerBase<TNode>`, `RH3202…Analyzer`, `RH3203…Analyzer`, and the formatter's `ExpressionBodiedTransformUtilities`. `AccessorExpressionBodyTransform` names it in a doc comment only. | One owner. RH3202 and RH3203 keep private copies of the analyzer and fix scaffolding, at parity with the bases: `ExpressionBody.GetLocation()`, `FormatNodeInDocumentAsync`, `BatchFixer`. ok |
| Where an inserted blank line goes | `SyntaxTriviaUtilities.FindIndexAfterLeadingDirectives`, through `BlankLineCodeFixUtilities.InsertBlankLineBefore` (RH5030 and RH3208 fixes) and the formatter's `BlankLineEditor` | One owner. ok |
| Removing an empty list | `RemoveNode(…, SyntaxRemoveOptions.KeepExteriorTrivia)` in the RH3106 and RH3107 providers | Two identical copies, pre-existing. SHARED-PARENS names only the analyzer base. Hint. |
| A rewrite must not create a generic-name reading | Only the RH3002 analyzer (the RH3002-004 logic) | RH3001, RH3005, RH3101, and RH3105 splice text without it (findings 2–4). The code side is a follow-up candidate (hint). |
| What counts as a documentation line | `DocumentationAnalysisUtilities.IsDocumentationLine` (SHARED-DOC), used by RH3201, RH8301–RH8303, and `CommentPositionUtilities` | RH3201 states it in its own requirement, per `squad/decisions/2026-10-19-997-rh3201-uses.md`. ok |
| Code-owner claims | No spec of this run names `SyntaxNodeUtilities`, `SpeculativeRebindingHelper`, or `DocumentationAnalysisUtilities`. `ExpressionBodyRewriteUtilities` moved from RH3202 to SHARED-BODY. `BlankLineCodeFixUtilities` is owned only by SHARED-AFTERBRACE. | ok. The architecture tests confirm one spec per type. |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| The 37 SHARED-BODY-003-tagged tests (RH3202–RH3208) | `=> …;` becomes the formatter's block, and nothing outside it changes | A line outside the body differs between source and fixed text | `Verify` with exact fixed text | Adequate. I diffed all 37: no change outside the body beyond the raw literal's own indentation. The member-wide relayout is pinned only by the untagged #1237 tests. |
| `NoFix` tests: RH3001-006, RH3002-009, RH3004-004, RH3005-005/-006, RH3102-004, RH3103-005 | The diagnostic is reported, and no action is offered for any diagnostic | A missing diagnostic, or an offered action | `VerifyNoCodeFixIsOffered` (markup verify + every diagnostic gets zero actions) | Adequate |
| Pre-existing tests tagged by this run: `DocumentationCommentedYodaConditionIsNotOfferedACodeFix`, `BuiltInComparisonIsStillFixed` (RH3005), `VerifyDocumentationCommentedEmptyStatementIsNotOfferedACodeFix` (RH3102) | No action, or one action, at a synthesized location | — | `GetCodeFixActionsAsync`: does not prove the analyzer reports; one test asserts only the action count | Weaker, but each requirement also has a full-strength test. Adequate together; hint. |
| RH3204 `InterpolatedStringsWithoutInterpolationAreReportedOnlyWhenConvertedToString`, `FixWritesTheLiteralWithTheSameValue` | Exact spans and exact output | A wrong span or wrong text | `Verify` with `MarkupMode.None` + `WithSpan` | Adequate. The spans match the literals' lengths. |
| RH3103-002 tests | The `nameof` argument is not reported, possibly qualified | `nameof(System.Nullable<int>)` is reported | `Verify` | Inadequate for the qualified clause. Finding 1. |
| `ParenthesesAroundAWholeExpressionAreReportedInEveryListedPosition` | Every position RH3002-001 lists | A listed position not reported | `Verify` | Three positions untested. Finding 5. |
| Re-tagged tests (RH3202, RH5030, RH3106/RH3107, RH3208) | The requirement that replaced the retired or renamed ID | — | Unchanged assertions: the diff removes only `[Requirement]` lines | Adequate |
| RH3208 SHARED-AFTERBRACE-001 and RH3208-002/-003 tests | Blank line above a comment and after a directive; terminal `break`; column | A different placement | `Verify` with exact text | Adequate |
| RH3201-001/-005 tagged tests | The input meets the neighbor condition; the comment is single-line where -005 is tagged | — | `Verify` | Adequate. `EmptyMultiLineCommentWithNewlinesIsReportedAndFixed` correctly carries -001/-004 only. |
| Untagged characterization tests (`Apply`) | The exact current output of each open question | — | `ApplyCodeFixAsync` + exact text | Adequate as characterization |

## Failure classes dismissed

- **FC-001, FC-005.** No production guard was added or changed. The spec scopes were compared with their predicates under *Guard scope*. Where a requirement names comments and directives, the guard also covers directives (`…CommentOrDirective`).
- **FC-003.** Each fix-withholding requirement has both a withheld and an offered side among its tagged tests: RH3001-006, RH3002-009, RH3004-004, RH3005-006, RH3102-004, RH3103-005. The exception is RH3103-002's qualified side (finding 1).
- **FC-004, FC-019–FC-021.** No rewrite changed. The comment and directive cases of the specs are in their tests. Comment-loss cases are filed as P1 open questions (#1229, #1232, #1233, #1238).
- **FC-007, FC-008.** The second Fix All pass of RH3001, RH3002, RH3004, and RH3102 stays as it is (thoenissen/Reihitsu#1143), untagged.
- **FC-009, FC-018, FC-029.** Not reachable: no fix changed, no naming rule, no paired layout.
- **FC-010–FC-014.** No formatter or analyzer code changed, and FC-028 likewise. The SHARED-BODY fixes still use `FormatNodeInDocumentAsync`, and RH3208 its with-context variant.
- **FC-015, FC-016.** Recorded under *Policy ownership*. The sibling shapes of the generic-name hazard are findings 2–4.
- **FC-022.** RH3204's tests cover regular, verbatim, `@$`, raw, and `$$` raw strings, and `{{`.
- **FC-023.** No rename involved. I tried a field named `await` for RH3105, but that fixture does not parse at all, so it shows nothing.
- **FC-024, FC-025.** SHARED-BODY's tagged tests cover all six reported kinds: `async`, `Task<T>`, `ValueTask`, static virtual interface operators, instance compound-assignment and increment operators, `checked` conversions, static constructors, and local functions in a lambda and in top-level statements.
- **FC-026.** Not reachable: no edit re-resolves tokens.
- **FC-030.** No SHARED-BODY-003-tagged input has anything outside `=> … ;` to relay out (diffed). The relayout is filed as #1237.
- **FC-032.** I re-ran the architecture tests on the head after the last merge. Neither merge touched production code.
- **FC-033–FC-035.** RH3006 Q1 is pinned under CRLF. The tests copy the spec examples rather than patched output, and the expected values come from the unchanged code.
- **FC-036.** Not reachable: no rename.
- **FC-037.** Not reachable: no comment-only claim about a `.cs` file.

## Hints

- **Pre-existing code defect behind finding 1.** The RH3103 fix throws `InvalidCastException` for a reported dotted `Nullable<T>` in expression context, because it replaces a member access's `Name` with a `NullableTypeSyntax`. I verified this for the two `nameof` forms. It is a code bug for the maintainer, outside this lane.
- **Generic-name hazard.** Only RH3002 guards against a rewrite creating a generic-name reading. Giving RH3001, RH3005, RH3101, and RH3105 the same guard is a production follow-up candidate, not part of this run.
- **No change doc for the retirements.** RH3202-002..004 and RH5030-003 were retired without a change doc. That matches `sdd/README.md`'s "only when intended behavior changes" and the RH5002 → SHARED-STMT precedent (no change doc under `sdd/changes/`), and `15-spec-checker.md` accepted the lead's reading. Not a finding.
- **Leftover.** `sdd/work/clarity-specs/` is the only leftover, and the lead deletes it before publishing.
