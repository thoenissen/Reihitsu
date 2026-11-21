Role: auditor
Commit: 358147a
Verdict: pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict to ignore.

## Scope

Full audit. Head `358147a`. The prompt named `90a5e8b`; the only commit since is `358147a` "Advance lead handover", which changes `sdd/work/cyclomatic-complexity/00-lead.md` alone, so the audited tree is the same as `90a5e8b`'s outside the work folder. `git ls-remote origin refs/heads/main` = `77a1f37072dec83d4ed5facef9c7c6fe45f76633`, an ancestor of HEAD (merged). The diff against `77a1f37` (42 files) matches what handovers 01–12 describe: the new analyzer, `CognitiveComplexityWalker`, `MemberBodyUtilities` and `ConfiguredLimitUtilities` (decisions 1 and 2), the RH2008/LineCountAnalyzerBase delegation, the `maximumMethodComplexity` setting, the resources, the rule page and the scoring guide, the specs (RH2009, SHARED-LIMIT, RH0001-007, SHARED-LINES-002, RH2008 header, index, IDs), the change doc, tests, `Release.ruleset` `None`, the self-hosting ignore entry, and the README and sln rows.

Issue thoenissen/Reihitsu#352 (`agent-filed`). Two of its goals were dropped or replaced by maintainer answers recorded in `00-lead.md`: lambdas judged on their own (answers 10, 19, 22, follow-up) and the `.editorconfig` key (replaced by `reihitsu.json`, clarification 4). Everything else is addressed: cognitive metric with nesting increments (answers 1, 2), a Design ID, the message names value and limit (RH2009-006), and no code fix.

How the work was checked: static tracing of the walker against every clause of RH2009-003/004/005, and of the analyzer against RH2009-001/002/006 and SHARED-LIMIT. Then a scratch probe (`/tmp/claude-0/-home-user-Reihitsu/d67f12c8-68c5-5f64-bb71-040061df6b78/scratchpad/probe`, outside the repository) ran the Release `Reihitsu.Analyzer.dll`, built after the last analyzer commit `2fae9e7`, at limit 1 on inputs the tests do not hold. Every result matched the spec:
- Recursion: a C# 14 extension-block member that calls itself on a receiver, plain and generic → 4 (recursion counted). A classic reduced extension call, plain and generic → 4. `nameof(N)` → 3 (not recursion). A partial implementation calling itself → 4, reported on the implementing part only. A generic method `this.Q<int>(…)` and `new C<string>().Q<T>(…)` → 4. A generic local function `Local<string>(…)` → 4, holder 0.
- Patterns: `case … when o is 1 or 2` and an arm `when i is 3 or 4` counted, switch expression outside a nested part → no nesting increment, not reported. `o is { } and not string` → 1. `o is int[] { Length: 1 or 2 } and [1 or 2, ..]` → 3 (subpattern and list-pattern trees counted on their own). Method total 7.
- Misc: a `?:` chain inside an interpolation hole → 6. `((n & b) | (b ^ a)) == true` with `bool?`/`bool` → 3 sequences, total 7. `catch` with a filter inside a `catch` block holding an `if` → 10. An event `add` accessor → 3.
- A 5000-operand `&&` chain: no stack failure, correct score.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| – | – | – | – | – | – | None | – |

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Root of a logical-operator tree (`IsLogicalTreeRoot`, `CognitiveComplexityWalker.cs:314`) | parent chain through `ParenthesizedExpressionSyntax`, then a `BinaryExpressionSyntax` that `IsLogicalOperator` accepts | `CollectLogicalOperators` (`:293`): descends through parentheses and logical binaries only | Same two node kinds, same `IsLogicalOperator` predicate, so every non-root is collected by exactly one root. Matches RH2009-003 "joined directly or through parentheses; `!` or any other expression starts a new tree" | ok |
| `&`/`|`/`^` counted (`IsLogicalOperator`, `:268`) | bound operator: `MethodKind.BuiltinOperator` whose return type is `bool`/`bool?` | RH2009-003: "the logical operator of `bool` or `bool?` operands" | A built-in `&`/`|`/`^` returns `bool`/`bool?` only for `bool`/`bool?` operands (integers and enums return their own type). User-defined operators and `dynamic` are excluded, as the spec's "logical operator of bool" wording implies | ok |
| Root of a combinator tree (`VisitBinaryPattern`, `:486`) | parent chain through `ParenthesizedPatternSyntax`, then `BinaryPatternSyntax` | `CollectCombinators` (`:168`): descends `BinaryPattern` and `ParenthesizedPattern` only | Agree. `not`, subpatterns, and list patterns start new trees (probe: 3 trees in one pattern) | ok |
| Whether combinators count (`_countsPatternCombinators`) | set `true` for an `is` pattern, `false` for a `CasePatternSwitchLabel` pattern and a switch-arm pattern, restored after each pattern. `when` clauses and arm expressions are visited after the restore | RH2009-003: combinators of `is` count, those of a case label or arm do not, an `is` in a `when` clause counts | Agree. Patterns occur only in those three owners; none holds an expression that could hold an `is` | ok |
| Chain continuation of `?:` (`StripParentheses(WhenFalse) is ConditionalExpressionSyntax`, `:373`) | the false branch through any number of parentheses | RH2009-003 item 2: "directly or inside one or more pairs of parentheses" | Agree. A `!`, cast, or other operator around it nests (test `ConditionalBehindAnotherOperatorInFalseBranchNests`) | ok |
| `else if` continuation (`node.Else.Statement is IfStatementSyntax`, `:347`) | the `else` clause's statement itself | RH2009-003 item 1/2 and RH2009-004 bullet 1 | Agree. `else { if … }` nests | ok |
| Reportable only with a nested structure (`_structuralNesting`, `:46`, `:210`) | incremented by every `VisitNested` except lambdas and anonymous methods. Checked only in `AddNestingStructure` (item 1 structures) | RH2009-004: an item-1 structure in the nested part of a construct other than a lambda | Agree. Item 2/3 increments never set it. Conditions, governing expressions, filters, and `for` header parts are visited outside `VisitNested` | ok |
| Recursion target (`Normalize`, `:117`; analyzer `:68`) | invocation symbol after `ReducedFrom` → `OriginalDefinition` → `PartialDefinitionPart`, compared with the member's symbol normalized the same way. Member symbol only for `MethodDeclarationSyntax`/`LocalFunctionStatementSyntax` | RH2009-003 recursion: "an invocation that binds to the member's own symbol, on any receiver" of "the judged method or local function" (answer 26) | Agree. The probe confirmed extension-block, reduced-extension, generic, partial, and local-function forms | ok |
| Judged members and name token (`MemberBodyUtilities.TryGetBody`) | the 13 syntax kinds, block body or expression body | RH2008-002/004/005 (RH2009-002/006 reference them). RH2008 uses the same helper | One owner, same mapping as RH2008's removed switch, token for token | ok |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Member → (body, expression body, name token) | `rg "ExpressionBody, method.Identifier\|accessor.Keyword\)" src`: only `src/Reihitsu.Analyzer/Core/MemberBodyUtilities.cs:54,60` (the formatter hit `AccessorListLayout.cs:221` is unrelated) | one owner |
| Reading the `design` section, default on invalid, `>` comparison | `rg "Configuration\?\.Design\|> limit\|ExceedsLimit\|ResolveLimit" src` (non-test): `ConfiguredLimitUtilities.cs:22,32,44`. Callers: `LineCountAnalyzerBase.cs:209–243`, `RH2009…Analyzer.cs:51,80`. `ConfigurationManager.cs:423` is the parser creating the section | one owner |
| Validation of limit values (`ParseLimit`) | `ConfigurationManager.cs` only, shared by the five settings | one owner |
| Cognitive complexity | `rg -l Complexity src` (non-test): the walker, the analyzer, and the config data types only | one owner |
| Settings listed for users | `rg maximumTestTypeLines` / `maximumMethodComplexity`: no schema file or other listing that misses the new setting | ok |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH2009 scoring tests (most at limit 1) | exact score per construct | any other score changes the message `Complexity(key, score, limit)` asserts exactly | `VerifyComplexity` + `Complexity` (exact message and location) | adequate: the score is asserted, not just presence |
| Flat-body / nested-body pairs (`FlatBodyFarAboveTheDefaultLimitIsNotReported` + `SameBodyWithOneNestedIfIsReportedByItsWholeComplexity`, `StructureInConditionDoesNotMakeTheMemberReportable` + `StructureInConditionIsOutsideTheNestedPartOfItsConstruct`, `ElseIfChainIsNotReported` + `IfInElseBlockMakesTheMemberReportable`, lambda pairs) | RH2009-004 decides reporting, not a low score | a flat body above the limit reported, or a nested twin not reported | `VerifyComplexity` | adequate: both sides of the boundary (FC-003) |
| Limit tests (equal / below / default 15 with 15 and 16, 16 invalid-setting rows, problem-elsewhere rows, test-project rows) | SHARED-LIMIT-001/002, one limit for test code | reporting at equality, a configured value ignored, or an invalid one taking effect | `VerifyComplexity` + global config | adequate |
| `RecursionOnAnotherInstanceExample` + `SameNamedMemberOfAnotherTypeIsNotRecursion` | binding by symbol, not by name | `Leaf.Depth()` scoring, or `Left.Depth()` not scoring | `VerifyComplexity` | adequate: token-identical pair differing by 1 point |
| RH0001 new and changed rows | RH0001-007 accepts and validates `maximumMethodComplexity` | wrong message or column | `VerifyConfiguration` (exact message and position) | adequate |
| `ReportLeavesOutComplexityViolations` | the self-hosting report drops RH2009 | report non-empty; or vacuous pass | control assertion that RH2009 reports the sample directly, then `Collect` is empty | adequate: the control rules out a vacuous pass |
| `ReleaseRulesetSwitchesOffComplexityRule`, `ComplexityAnalyzerHasARow` | decision 4 / scan row | entry missing or other action | exact `"None"` | adequate |
| RH2008/RH2111 re-tags | unchanged behavior under SHARED-LIMIT | — | existing assertions unchanged | adequate |

Visible output (check 10): every expected RH2009 value is derived from RH2009-001…006 as approved (answer 25) and amended by answer 26. The RH0001 values follow RH0001-007 as changed in the change doc (*Visible output changes* 7). The rule page and guide are covered by answers 21, 24, and 25, and by decision 5 (class wrapper only). The rule page Violation scores 16 and the Correction is behavior-equivalent (traced). Every worked score in `documentation/cognitive-complexity.md` was recomputed by hand and matches the spec and the implementation (Conditions 12, Patterns 7, Switches 5, Loops 8, Lambdas 7, Worked3 17, Worked4 13, Worked5 5/3).

## Failure classes dismissed
- FC-001, FC-002, FC-003: traced under *Guard scope*; every qualifier ("directly or through parentheses", "statement of an else clause", "other than a lambda", "bool or bool?", "is pattern vs case label/arm") is an explicit condition, and boundaries have two-sided tests.
- FC-004, FC-005, FC-019, FC-020, FC-021: no edit of trivia or gaps. Directives only as SHARED-DIAG-008: the walker visits nodes, never disabled text (tests with `DefineSymbols`).
- FC-006, FC-018, FC-022, FC-023 (except location), FC-026, FC-029, FC-036: no rewrite, rename, or literal handling. A verbatim identifier's location is tested (`VerbatimIdentifierIsReportedOnTheWholeToken`).
- FC-007, FC-008, FC-009, FC-030, FC-043: no code fix.
- FC-010, FC-011, FC-012, FC-028, FC-038–FC-042: no formatter phase or precheck change. FC-039's analyzer analogue (nested descent) is covered: lambda in lambda, local function in lambda/local function/field initializer, switch expression in a `when` clause.
- FC-013, FC-014: complexity is not a concern any formatter phase judges.
- FC-015, FC-016: one owner each (*Policy ownership*). Recursion siblings (extension block, reduced extension, generic, partial, another instance, another overload, method group, delegate, `nameof`) traced or probed.
- FC-024, FC-025: partial method and property, explicit interface implementation, extension block, every expression-bodied kind and accessor are tested. Partial recursion was probed.
- FC-027: multi-line, nested, and other-scope forms are covered by tests and by the probe (interpolation hole, nested `catch`, event accessor).
- FC-031: summaries of `ParseLimit`, `ResolveLimit`, `TryGetCountedSpan`, the walker, and both utilities describe the new code. No `TODO` and no tracker number in source (diff grep).
- FC-032: the probe used a DLL built after the last analyzer commit (`2fae9e7`). Nothing compiled has changed since.
- FC-033, FC-034: not a bug fix. The issue's example is the exact `ScoreExample`.
- FC-035: expected values came from the spec before the implementation (07). The 10 additions were derived from the amended spec.
- FC-037: no text-only claim.
- FC-040: documentation comments hold no statements.
- FC-044: recursion reads the compiler's binding (`GetSymbolInfo`), and `#if` reads the parsed tree; nothing is modelled.

## Architecture findings
_None._ Decisions 1 and 2 already merged the two policies that would otherwise have had two owners.

## Hints
- `Debug.ruleset` does not list RH2009 (only `Release.ruleset` was changed, decision 4). Once `/bump-version` puts RH2009 into `binaries/`, Debug builds of this repository will show RH2009 warnings (not errors) on its own code. The self-hosting follow-up in `00-lead.md` could mention it. Not a defect of this run.
- `00-lead.md` lists maintainer answer 12 after answer 26, and decision 3 after decision 6. It is a work file that is deleted before publishing; no action needed.

## Follow-up candidates
_None_ beyond the two already listed in `00-lead.md`.
