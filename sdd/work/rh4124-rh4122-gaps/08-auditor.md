Role: auditor
Commit: 9397d1033c04f3cf4e697ff55b7987d79e632603
Verdict: pass, text cleanup

My prompt carried no intended fix, suspected cause, or hoped-for verdict; nothing to ignore.

## Scope

Full audit. Head `9397d10` (checked out, clean tree); base `origin/main` = `0feb51810751b245fec4c51eb6fd207d0f824f75` per the prompt and `git rev-parse origin/main`, and it is an ancestor of head (merged). The diff (`git diff 0feb518`, 21 files) matches what handovers 01–07 describe: two analyzers (+32/−3), two test classes (16 new tests), RH4124/RH4122 specs, RH4125 non-goal line, requirement ledger, change doc, two rule pages, resx title and message, package README row, and `sdd/work/`. `scripts/lane-check.sh behavior-change`: confirmed, the diff needs behavior-change.

Issue thoenissen/Reihitsu#1089 (`agent-filed`): both decisions are implemented. The issue's RH4124 shape `from x in values let y = x * 2 select y` is in RH4124-003's example and in `SingleLetterQueryRangeVariablesAreReported`. The RH4122 shape `extension(string t)` is in RH4122-004's example and in `SingleLetterExtensionBlockReceiversAreReported`. "Fixed where the rule's fix applies" does not apply: neither rule has a fix (SHARED-SINGLE non-goals), so the PR has to say that.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | low | `documentation/rules/RH4124.md`, *Why is this a problem?* and *How to fix it* | FC-031 (documentation analogue) | – | same requirement | The heading and *Description* now cover query range variables, but the two following sections still talk only about parameters: "Descriptive parameter names make inline transformations…" and "Rename the parameter to describe the value or role it represents." A user who opens the page from a diagnostic on `from x in values` is told to rename "the parameter", and `x` is not a parameter. RH4122.md does not have this problem, because a receiver is a parameter. | Say "parameter or range variable" in both sentences, or use a neutral noun such as "name". This is a rule-page text change that follows from the maintainer's decision on #1089 and answer 1B; the lead decides whether it needs asking. |

Why `pass, text cleanup`: finding 1 is limited to a Markdown rule page. It cannot reach compiled behavior, diagnostics, or rule metadata, and `AnalyzerPackageMetadataTests` pins only the H1, which is already correct.

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| RH4124: which syntax declares a range variable (`OnRangeVariableDeclaration`) | The `Identifier` token of the visited `FromClause`, `LetClause`, `JoinClause`, `JoinIntoClause`, or `QueryContinuation`. These are the only Roslyn query syntaxes that carry a declaring identifier; `Select`/`Where`/`OrderBy`/`Group` declare nothing. | RH4124-003's list "from (first, explicitly typed), let, join, into of join, into of continuation behind select or group". RH4124's existing `OnParameter`, which uses the anonymous-function container. | Same set. The kinds are syntactically disjoint from `ParameterSyntax`, so a variable cannot be reported twice. Uses are `IdentifierName` nodes and are never visited. A missing identifier in erroneous code has `ValueText == ""` and is not reported. | ok |
| RH4124: name test on a range variable | `identifier.ValueText`, so the `@` prefix and Unicode escapes are decoded | `OnParameter` uses `IParameterSymbol.Name` (also decoded); SHARED-SINGLE-001 says "without its verbatim prefix" | Equal for `@b` (→ `b`), `@_` (→ `_`, exempt), and `é` | ok |
| RH4122: `IsSupportedParameter` accepts `INamedTypeSymbol { IsExtension: true }` | The parameter's `ContainingSymbol` | Partition with RH4123 (local function), RH4124 (`AnonymousFunction`), and RH4105 (casing of the same receiver). The only parameter whose container is an extension type is the receiver; members inside a block have their method or indexer as container (RH4122-001). | Disjoint and complete, as the skeptic's partition showed. `IsFrameworkMandatedSignature` and `IsRecordPrimaryConstructorParameter` return false for a receiver; RH4122-004 and the change doc's *Decisions* say the exemptions do not apply. An unnamed receiver (`extension(int)`) has the name `""` and is not reported (RH4122-004, FC-002). | ok |
| RH4122: one diagnostic per extension block | The per-`ParameterSyntax` node action, located on `parameter.Identifier` | RH4122-004 "one diagnostic per extension block". The compiler-synthesized implementation methods have no `ParameterSyntax`, so this action does not reach them. | Agrees; pinned by `ExtensionBlockReceiverIsReportedOncePerBlock` (block with 4 members) | ok |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Single-letter name (SHARED-SINGLE-001) | `rg HasSingleLetterName src` finds only `SingleLetterIdentifierUtilities.HasSingleLetterName`. Both changed paths call it, and `rg "Length == 1" src/Reihitsu.Analyzer` finds no private copy. | one owner |
| Which syntax declares a query range variable | `rg "FromClause\|LetClause\|JoinIntoClause\|QueryContinuation" src` (outside the formatter): only RH4124. RH3007 reads only `QueryExpression.FromClause.FromKeyword` to place its location, which is not a naming decision. | one owner |
| Extension-block receiver as a parameter | RH4122 `IsSupportedParameter` (new). `CasingRenameSafetyUtilities` lines 332 and 871 handle `IsExtension` receivers for RH4105's rename-safety, which is a different decision (rename rebinding), not which rule reports the name. `ParameterListParentPolicy` is a layout policy. | one owner for "single-letter receiver"; the other places own other decisions |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH4124 `SingleLetterQueryRangeVariablesAreReported` | Every clause kind that declares a range variable is reported on its identifier, `@a` counts, and `_` is exempt | A missing, extra, or misplaced diagnostic | `Verify` with `{|RH4124:…|}` markup: exact ID, span, and count; compiler errors would also fail | adequate |
| RH4124 `QueryRangeVariablesWithLongerNamesAreNotReported`, `DiscardRangeVariablesAreNotReported` | No report for longer names, or for `_` in each of the 5 clause kinds | Any diagnostic | `Verify` (empty set) | adequate. The negative side of the FC-003 boundary is covered per clause kind. |
| RH4124 `UsesOfQueryRangeVariablesInLaterClausesAreNotReported` | Uses in `where`/`orderby`/`let`/`select` are not reported | Extra diagnostics on uses | `Verify` exact set | adequate |
| RH4124 explicit-type, second-`from`, group-continuation, `OneCharacterOtherThanUnderscore`, `EveryEnclosingScope` | Each member the skeptic listed (FC-016, FC-023, FC-027) | Missing report in that shape | `Verify` exact set | adequate |
| RH4122 `SingleLetterExtensionBlockReceiversAreReported`, generic, unnamed (plain and generic), modifiers/attributes, `OneCharacterOtherThanUnderscore`, partial parts | RH4122-004's positive and negative sides (FC-002, FC-003, FC-016, FC-023, FC-024) | A missing receiver report, or one on `extension(int)`/`_`/`value` | `Verify` exact set | adequate |
| RH4122 `ExtensionBlockReceiverIsReportedOncePerBlock` | One diagnostic per block even with several members | Duplicate report on `t` | `Verify` exact count and span | adequate |
| Message text (answer 1B) | The new title and message text | A stale resource text | The new tests use markup without a message. The message comes from the same descriptor as the existing tests, which assert `MessageFormat`; `AnalyzerPackageMetadataTests` pins resource title = rule-page H1 = README row, and message = title (`MessageFormatResourcesMatchTitleWhenNoPlaceholders`). | adequate |

FC-035: the expected values come from the spec examples (written in 01, before the implementation in 5397936), not from the implementation's output. The test implementer showed every positive test failing beforehand with missing diagnostics only.

Visible output is decided (check 10): every new expected diagnostic is covered by the maintainer's decisions on #1089, which the change doc's *Decisions* section and `00-lead.md` record. The `_` exemption follows SHARED-SINGLE-001, which the run does not change. The title, message, rule-page H1, and README row are covered by answer 1B under *Maintainer answers*. The rule-page *Description* sentences are listed in the change doc's *Visible output changes* and follow the same decisions. No test value is left undecided.

## Failure classes dismissed
- FC-001: no guard was narrowed. RH4122's container check was widened by one disjoint symbol kind, and RH4124 adds a new registration (see *Guard scope*).
- FC-004, FC-005, FC-019, FC-020, FC-021: the analyzers do not edit anything. They report on an identifier token, and trivia does not change which token is reported.
- FC-006, FC-022, FC-025, FC-026, FC-029: no rewrite; an analyzer only.
- FC-007, FC-008, FC-009, FC-018, FC-030, FC-036, FC-043: neither rule has a code fix, and none is added.
- FC-010, FC-011, FC-012, FC-013, FC-014, FC-028: no formatter involvement. A name is not something the formatter judges, so P11/SHARED-DIAG-010 has nothing to compare.
- FC-038 to FC-042: no unsupported shape is declared or touched.
- FC-032: no formatter change. The implementer ran the whole analyzer project, including self-hosting (12158 passed), after the compiled change, at 5397936, and the commits since then do not touch code.
- FC-033: no harness normalization reaches a diagnostic on an identifier.
- FC-034: the issue's own shapes are tested verbatim (see *Scope*).
- FC-037: no text-only claim is made for a `.cs` file.
- FC-044: no computed result is modelled. RH4124 reads syntax that is a declaration by construction, and RH4122 reads `ContainingSymbol`, which the compiler computes.
- FC-002, FC-003, FC-015, FC-016, FC-017, FC-023, FC-024, FC-027, FC-031, FC-035: traced above. Each holds, except FC-031 on the rule page (finding 1). The XML summaries of both analyzer classes and of `OnRangeVariableDeclaration` describe the new code.

Comments (check 8): no `TODO`, no commented-out code, and no tracker reference in changed source. Scope (check 9): no edit outside the run. Cost of special cases (check 11): none; each change is one registration or one branch. Code quality (check 7): the `_ => default` arm of the switch cannot be reached because the registration limits the node kinds, and `default(SyntaxToken).ValueText` is `""`, so it would not report anyway. No hot-path concern: five syntax kinds, and no semantic model on the range path.

## Architecture findings
_None._

## Hints
- `04-test-implementer.md` and `03-spec-author.md` say the RH4124 tests "assert the resource's `MessageFormat`". That holds for the pre-existing tests, not for the new markup tests. The message is still pinned (see *Assertion adequacy*), so this is no finding.

## Follow-up candidates
_None_ beyond the one already in `00-lead.md` (casing of query range variables).
