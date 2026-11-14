Role: auditor
Commit: 5dde9962bd09724a48743398060eda1fdd8ed042
Verdict: #1264: pass; #1333: pass; #1334: pass; #1339: pass; #1337: findings; #1412: findings; #1592: pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict. State verified: HEAD is `5dde996`, `origin/main` fetched is `8a26419228353ec26062c404565abb5c9c68ae10` and is an ancestor of HEAD, `scripts/lane-check.sh bug-fix` confirms the lane, and the diff (`git diff 8a26419…HEAD -- . ':(exclude)sdd/work'`, 98 files) is what handovers 05 and 08 describe. Execution was limited to `scripts/apply-fix.sh` over fixtures in the session scratchpad (outside the repository, deleted afterwards); every other statement is from static tracing. Finding numbers start at `#1` (first audit of the run).

## thoenissen/Reihitsu#1264

### Scope
Full audit. RH7308, RH7309 (analyzer and fix), RH7311, both RH74xx bases, `RegionDirectiveUtilities` (`GetTypeRegions`, `GetOwningTypeDeclaration`, `TryFindInnermostContainingRegion`, `IsInConditionalBranch`), rule pages RH7308 and RH7401–RH7412.

### Findings
_None._

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Which declaration owns a region | `GetOwningTypeDeclaration`: innermost `BaseTypeDeclarationSyntax` whose `Span` (leading trivia excluded) holds the directive | Removed `BelongsToType` (type span minus nested `TypeDeclarationSyntax` spans) | Same result for every type kind; differs only for enums, as SHARED-TYPEREGION-006 requires. A `#region` in front of a nested type, its attribute list, or its doc comment lies before the nested type's `Span` and stays with the outer type; a `#region` in front of a nested type's `}` stays with the nested type | holds |
| Which region judges a member | `TryFindInnermostContainingRegion`: deepest pair of `GetTypeRegions` whose directives enclose the member, only when a depth-1 pair encloses it | `GetTopLevelRegions` = depth-1 subset of the same list (RH7102/RH7103/RH7109/RH7110 scope) | Depth is counted over the type's own regions only, so the two agree on "lies in a top-level region" | holds |
| RH7309: which regions are compared | Active `#region` directives of `GetTopLevelRegions` | RH7309 fix sorts every top-level region, inactive ones included | An inactive top-level region always has an unbalanced conditional directive in an inter-block gap, so `CanReorderSafely` withholds the fix whenever the two lists differ; outputs are "reported, no fix" (Q1b-A) | holds |
| RH7311: region in a branch | `IsInConditionalBranch`: previous directives back to the type's `SpanStart` | RH7309 `IsActive` | Different decisions by design: RH7311-001 (any branch, compared with nothing; decision 16 for branches that start outside the type) vs. RH7309 per Q1/Q1b (active configuration). Both satisfy SHARED-TYPEREGION-006's "different branches of one conditional" sentence | holds (justified) |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Region ownership (type ↔ region) | `RegionDirectiveUtilities.GetOwningTypeDeclaration`, used by `GetTypeRegions` and the RH7309 fix (`rg "FirstAncestorOrSelf<TypeDeclarationSyntax>"` in region code: none left) | one owner |
| Innermost judging region | `TryFindInnermostContainingRegion` (RH7308, `MembersShouldBeGroupedByRegionsAnalyzerBase.HasMatchingRegion`, `NonOverride…AnalyzerBase.TryGetContainingRegionName`) | one owner; the two private `TryGetContainingRegionName` wrappers in RH7308 and the override base are identical and pre-existing (hint) |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH7308 `MemberInANestedRegion*` (5 tests), `MemberBehindANestedRegionIsJudgedByTheTopLevelRegion` | Innermost region judges; depth 1, 2; unrecognized and bare inner label | Diagnostic on the wrong member or missing | `Verify` with markup (exact diagnostics) | adequate |
| RH7309 enum tests, `CodeFixReordersTheBlocksOfTheTypeAroundANestedEnumWithARegion` | Enum regions belong to the enum; fix leaves enum untouched (safety-rail rows of 02-skeptic) | Report on the enum region; enum members reordered | `Verify(source, fixed)` exact text | adequate |
| RH7309 branch tests (if/else, if/elif, compiled/excluded, earlier region) | Active-configuration judging (Q1, Q1b) | Report in the excluded configuration | `Verify` with and without `DEBUG`; `VerifyNoCodeFixIsOffered(…, "DEBUG")` asserts diagnostics and no action | adequate |
| RH7311 branch and enum tests | RH7311-001, decision 16 | Report on a branch region; no report on a type wrapped in `#if` | `Verify` | adequate |
| Core `GetTopLevelRegionsExcludesTheRegionsOfANestedEnum` | Helper excludes enum regions | Enum pair returned | direct assertion | adequate |

### Visible output
Every changed expected value is demanded by unchanged SHARED-TYPEREGION-006 (innermost region, enum ownership, branches) or by Q1/Q1b; RH7308 with an unrecognized inner label by decision 5 (SHARED-TYPEREGION-006 + RH7308-002). Rule-page edits (RH7308, RH7401–RH7412) restate SHARED-TYPEREGION-006/SHARED-REGGROUP-006, which are unchanged.

## thoenissen/Reihitsu#1333

### Findings
_None._

### Guard scope
_N/A — no guard added; `PartialMemberUtilities.GetDeclaredSymbols` enumerates every declarator with no position-dependent branch._

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Symbol(s) of a member declaration for RH74xx | `PartialMemberUtilities.GetDeclaredSymbols` (both bases). `PartialMemberUtilities.GetDeclaredSymbol` (first declarator) remains for RH2004 and RH8203, which judge a declaration as a whole | justified, different consumers |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH7403/RH7407/RH7411 `…JudgedOnItsOwn`, `…InReversedOrder`, `…ReportedOncePerDeclarator…`, partial field-like event tests | SHARED-REGGROUP-005 / SHARED-DIAG-006 per declarator, both orders | Diagnostic only at the first declarator | `Verify` with markup | adequate |

### Visible output
Demanded by unchanged SHARED-DIAG-006.

## thoenissen/Reihitsu#1334

### Findings
_None._

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Override region names | `typeSymbol.GetMembers()`: non-implicit members, ordinary methods, properties, indexers, events (each declarator) | SHARED-REGGROUP-006 (any kind, any declaration) | Implicit record members and accessor methods excluded; explicit interface implementations cannot override | holds |
| Interface names of an implementation | `AllInterfaces` whose member maps to the defining-part symbol | `GetImplementedInterfaceName` (first of the same list) for `DocumentationAnalysisUtilities` | Same list, first element unchanged | holds |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Override region name of one member | `OverrideMemberUtilities.GetOverrideRegionNames` (RH7401–RH7404 and the override base) | one owner |
| Interface region names | `InterfaceImplementationUtilities.GetInterfaceRegionNames` | one owner |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH7405–RH7408 `…MakesOverrideRegion`, partial-in-other-file tests | Any kind, any declaration (SHARED-REGGROUP-006) | Missing report on `N` | `Verify` with markup, multi-file | adequate |
| RH7409 two-interface tests (both base-list orders, either/neither region) | SHARED-REGGROUP-004 | Order-dependent verdict | `Verify` | adequate |
| RH7401/RH7409 partial tests (one/two files) | Q2-A, SHARED-DIAG-012 | Report on the wrong part or twice | `Verify` | adequate |

### Visible output
Demanded by unchanged SHARED-DIAG-009 and SHARED-DIAG-012, and Q2-A.

## thoenissen/Reihitsu#1339

### Findings
_None._

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Fields compared | Group key `(GetContainingRegionIndex, group)` in RH7102/RH7109 analyzers | Fix target filter `GetContainingRegionIndex(...) == regionIndex` in both fixes | Same index function, same top-level list | holds |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Region scope of member-ordering comparisons | `RegionDirectiveUtilities.GetContainingRegionIndex` (RH7102, RH7103, RH7109, RH7110 analyzers and fixes) | one owner |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH7102/RH7109 `…AreNotCompared`, `…IsNotComparedWithAFieldInARegion` | RH7102-003/RH7109-003 | Cross-region report | `Verify` | adequate |
| `CodeFixTargetsTheFirstFieldOfTheGroupIn…OwnRegion`, `NoCodeFixWhenTheFirstFieldOfTheGroupInTheOwnRegionIsTheFirstMemberOfThatRegion` | Target in own region; SHARED-ORDER-004 still withholds | Move across `#region B`; fix offered there | `Verify(source, fixed)`; `VerifyNoCodeFixIsOffered` | adequate |

### Visible output
Diagnostics: unchanged RH7102-003/RH7109-003. The fix result of `CodeFixTargetsTheFirstFieldOfTheGroupIn…OwnRegion` (offered now, withheld on `main`) rests on the reworded RH7102-005/RH7109-005, aligned under the maintainer's triage comment on #1339 (see Hints).

## thoenissen/Reihitsu#1337

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | high | `src/Reihitsu.Core/AccessorOrderingUtilities.cs:112-123` (`ContainsConditionalCompilationBoundaryOutsideBodies`), used by RH7107 and RH7108 | FC-001, FC-002 | – | PR-introduced | The excluded span is `accessor.Body ?? accessor.ExpressionBody`; Roslyn's `ArrowExpressionClauseSyntax` ends before the accessor's `;`, so a conditional directive or excluded code at the end of an expression body (leading trivia of the `;`) counts as "outside the bodies" and keeps the accessor list unreported. RH7107-001/RH7108-001 say a directive inside "an expression body" MUST NOT keep it from being reported (the C# grammar's accessor body is `=> expression ;`), and `main` reported these inputs. Counterexample (apply-fix, LF and CRLF): `set => _a = value⏎#if DEBUG⏎+ 1⏎#endif⏎;⏎get => _a;` → no diagnostic; RH7108 `remove => _h -= value⏎#if DEBUG⏎?? throw new InvalidOperationException()⏎#endif⏎; add => _h += value;` → no diagnostic. Members that hold: directive at the end of a block body (`}` lies inside the body span) → reported and fixed; directive inside the expression (`N(value,⏎#if DEBUG 1 #else 2 #endif⏎)`) → reported and fixed. No test covers an expression-bodied accessor with a directive at any position, for either rule | Treat the `;` of an accessor with an expression body as part of that body (one token check), and add RH7107 and RH7108 tests for a directive inside an expression body, at its end included. If the lead instead reads "expression body" as Roslyn's arrow clause, the spec must say the gap before `;` is judged, and that output (unreported vs. reported on `main`) is undecided |
| 2 | medium | `src/Reihitsu.Analyzer/Rules/Organization/RH7105DeclarationKeywordsMustFollowOrderAnalyzer.cs:69-75`, `src/Reihitsu.Analyzer/Rules/Organization/RH7106ProtectedMustComeBeforeInternalAnalyzer.cs:69-75` | FC-015 | – | PR-introduced | The judged modifier span under SHARED-DIAG-008/SHARED-MODORDER-003 (`[first modifier start, last modifier start)`, `modifiers.Count > 1`) is written inline twice, as identical private copies, next to the shared `OrderingDeclarationUtilities.HasModifiersSplitByConditionalDirective` that owns the RH7102/RH7103 span. Decision 13 keeps the two spans apart deliberately, but the RH7105/RH7106 span itself now has two owners that can drift | One helper (e.g. in `DeclarationModifierUtilities` or beside `HasModifiersSplitByConditionalDirective`) used by both analyzers |
| 3 | medium | `src/Reihitsu.Analyzer.Test/Ordering/RH7105…AnalyzerTests.cs`, `RH7106…AnalyzerTests.cs` | FC-003 | – | PR-introduced | The new span is tested on its upper boundary (behind the last modifier: reported) and its interior (between modifiers: not reported), but not on its lower boundary: a conditional directive in the leading trivia of the first modifier, which is where the `#endif` of a preceding conditionally compiled member lands (`#if DEBUG⏎void Log() { }⏎#endif⏎static public int A;`). That shape is common, and widening the span to the first modifier's leading trivia would silence it with every test green. Today it holds (apply-fix: reported and fixed, LF and CRLF) | Add one test per rule: a misordered declaration directly behind a conditionally compiled member stays reported and fixed |

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| RH7101 declarator span | `[first declarator start, last declarator start)` | RH7101-001 "between its declarators"; decision 14 | Initializers of non-last declarators included, last one excluded, as decided | holds |
| RH7105/RH7106 modifier span | `[first modifier start, last modifier start)` | `HasModifiersSplitByConditionalDirective` (RH7102/RH7103: to the token after the last modifier) | Differ by design (Q3-B, decision 13) | holds; duplication is #2, lower boundary #3 |
| RH7107/RH7108 accessor-list span | Leading trivia of every list token except `{` and body tokens after a body's first token | RH7107-001/RH7108-001 "outside bodies, block or expression body" | Expression body's `;` counted as outside | **#1** |
| RH5102/RH5109 gaps | First token of every element, every separator, and `)` | RH5102-004/RH5109-004 gaps | Agree, `(` leading trivia excluded | holds |
| RH5111 span | Leading trivia of `=` and of the value's first token | RH5111-005 "two gaps around the `=`" | Agree | holds |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Conditional boundary in a span | `SyntaxTriviaUtilities.ContainsConditionalCompilationBoundary`, `LeadingTriviaContainsConditionalCompilationBoundary`, `ContainsConditionalCompilationBoundaryInElementGaps`; `AccessorOrderingUtilities.ContainsConditionalCompilationBoundaryOutsideBodies`; `OrderingDeclarationUtilities.HasModifiersSplitByConditionalDirective`; inline in RH7101, RH7105, RH7106 | primitives shared; RH7105/RH7106 inline copy is #2 |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH7101 four new tests | Between declarators / non-last vs. last initializer | Wrong verdict on either side | `Verify` | adequate |
| RH7105/RH7106 `…SplitByAConditionalDirective…`, `…BehindTheLastModifier…`, `…WhereTheSymbolIsDefined…` | Interior and upper boundary | — | `Verify` with/without `DEBUG`; `VerifyNoCodeFixIsOffered` | lower boundary missing (#3) |
| RH7107/RH7108 new tests | Directive between accessors, behind attribute list, empty pair, inside block body | — | `Verify` | expression body missing (#1) |
| RH5102/RH5109/RH5111 new tests | Gaps vs. inside element/value; both sides of `=` | Report with a gap directive; silence with an inside directive | `Verify`, `VerifyNoCodeFixIsOffered` | adequate |

### Visible output
Demanded by unchanged SHARED-DIAG-008 and RH7101-001 (decision 14), Q3-B and Q4-A. Finding #1's expected value is not covered: no requirement or answer makes an expression-bodied accessor with a trailing conditional directive unreported.

## thoenissen/Reihitsu#1412

### Findings
Finding #3 (above) covers the RH7105/RH7106 tests of this issue's analyzers too; nothing else.

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Defining part judged once | `GetDeclarationsJudgedWith`: a type yields its direct `ConstructorDeclarationSyntax` members with `partial` and no body; a defining part never yields itself | Roslyn driver visits only the implementing part (02-skeptic) | Each part judged exactly once, also if a later Roslyn visits the defining part; generated types are not analyzed, so their defining parts are not either | holds |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| "Defining part of a partial constructor" | `DeclarationModifierUtilities.IsDefiningPartOfPartialConstructor` (private), `PartialMemberUtilities` (symbol-based, for other rules) | different inputs (syntax for a node action vs. symbol); acceptable |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH7106 `BothPartsOfAPartialConstructorAreReported` | Both parts reported and each fixed on its own (SHARED-DIAG-004) | Missing report or fix on the defining part | `Verify(source, fixed)` | adequate |
| RH7105 partial-constructor tests, RH7106 other-file/generated/record tests | Defining part reported in every placement | Missing report | `Verify` (diagnostics only); the RH7105 fix on the defining part shares `ModifierOrderingCodeFixProviderBase` with RH7106, whose fix test covers it | adequate |

## thoenissen/Reihitsu#1592

### Findings
_None._

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Type the RH7309 fix reorders | `GetOwningTypeDeclaration(FindTrivia(diagnostic start))` | Analyzer's `GetTopLevelRegions` (same function via `GetTypeRegions`) | One statement (decision 7). apply-fix: a block starting with a nested struct whose own regions are inverted → outer type fixed, nested type fixed in its own iteration; a block starting with a doc-commented nested interface → outer type fixed; nested enum with inverted regions → enum untouched | holds |

### Policy ownership
See #1264 (region ownership: one owner).

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `CodeFixReordersTheTypeWhenTheReportedBlockStartsWith…` (3), `CodeFixForTheOuterTypeLeavesTheRegionsOfANestedClass…` | RH7309-004 / SHARED-FIX-005 | Nested type reordered or no-op fix offered | `Verify(source, fixed)`; `TryApplyCodeFixAtAsync` with exact expected text | adequate |

## Whole run

### Failure classes dismissed
- FC-004, FC-019, FC-020, FC-021 — no new rewrite; the changed fixes (RH7102/RH7109 target, RH7309 type lookup) reuse the existing move/reorder safety (SHARED-ORDER-004, `CanReorderSafely`), which the run's tests pin with region directives in the crossed span.
- FC-005 — every new check filters conditional directives and disabled text only; `#region`/`#pragma` stay reported (tests in RH5102, RH5111).
- FC-006, FC-022, FC-023, FC-025, FC-026, FC-029, FC-036 — no expression rewrite, literal, identifier, or rename touched.
- FC-007, FC-008, FC-009 — RH7102/RH7109 fixes move within one region and are withheld for the SHARED-ORDER-004 shape (tested); RH7309 identity reorder has no input once the type lookup agrees (02-skeptic); Fix All overlap between an outer and a nested type's reorder is pre-existing on `main`.
- FC-010, FC-011, FC-012, FC-028, FC-030, FC-038–FC-043 — no formatter or precheck change; code fixes still format through `ReihitsuFormatter` unchanged.
- FC-013, FC-014 — RH5102/RH5109 newly silent on a gap directive the formatter still splits: P11, the rule's own construct, decided by SHARED-DIAG-008 (02-skeptic).
- FC-016 — the issues' sibling shapes are enumerated in 02-skeptic and covered; #1412's other constructor rules are a recorded follow-up.
- FC-018 — no naming fix.
- FC-024 — partial members and types: covered by #1334, #1333, #1412 tests (one/two files, generated part).
- FC-027 — nesting depth 1/2 and nested types/enums covered; accessor body kinds: see #1.
- FC-031 — every changed method's summary describes the new code (`GetTopLevelRegions`, `TryFindInnermostContainingRegion`, `GetDefiningPartSymbol`, `GetReportedPart` remarks, base summaries); no stale "first declarator"/"stand-in" text in changed members.
- FC-032 — no compiled formatter change; the self-hosting scan is not run locally (see Hints).
- FC-033, FC-034, FC-035 — reproductions use the reported shapes with defined/undefined symbols, multi-file and `.g.cs` sources; expected values come from the specs and answers, not from patched code.
- FC-037 — not a text-only change.
- Unsupported shapes — no shape in the run costs a formatter special case; no FMT-UNSUP change.

### Checks 8, 9, 11
- No `TODO`, commented-out code, or tracker number in the source diff.
- Scope: every changed path belongs to an issue of the run or a recorded decision (4, 15, 16, 17).
- Cost of special cases: every new guard is a cheap syntactic check (token trivia kinds, directive walk); none serves only a rare class where refusal would have done.

### Architecture findings
_None._ The structural finding of 02-skeptic (#1592) is resolved by one owner; decision 13's two modifier spans are a maintainer-decided difference, not a defect.

### Hints
- #1339: the RH7102/RH7109 fix is now offered for an in-region inversion whose group also has an earlier field in another region (withheld on `main`). It rests on RH7102-005/RH7109-005 as reworded in this run (decision 9), under the maintainer's triage comment on #1339 that the PR aligns the rule's own spec text; the lead may want to confirm that this comment covers a fix-offered change.
- `RH7308…TryGetContainingRegionName` and `NonOverrideMembersShouldNotBePlacedInOverrideRegionsAnalyzerBase.TryGetContainingRegionName` are identical private copies (pre-existing; the run edited both in parallel).
- `MembersShouldBeGroupedByRegionsAnalyzerBase` now calls `GetDeclaredSymbol` for every member before the syntax-kind filter in `GetExpectedRegionNames`, for eight analyzers; cheap per call, but filtering by kind first would avoid it.
- RH7308, RH7311, and RH74xx now judge nested regions and branches; the self-hosting scan (CI) may show new findings in the repository's own code.
