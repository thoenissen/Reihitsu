Role: implementer
Commit: 69a1330
Verdict: #1264: tests pass; #1333: tests pass; #1334: tests pass; #1339: test disputed; #1337: tests pass; #1412: tests pass; #1592: tests pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Work was taken from the handovers, the decisions in `00-lead.md` (rows 6, 7, 10, 13 followed), and the spec diff. Built and tested in Debug.

## thoenissen/Reihitsu#1264

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/RegionDirectiveUtilities.cs` | New `GetOwningTypeDeclaration` (innermost `BaseTypeDeclarationSyntax` whose span holds the directive, so enums count); new `GetTypeRegions` (every region of the type with its depth), `GetTopLevelRegions` is its depth-1 subset; private `BelongsToType` and the nested-span list removed. New `TryFindInnermostContainingRegion` (innermost region, only when a top-level region holds the member). New `IsInConditionalBranch(directive, scope)`: walks previous directives back to the scope's start, counting `#endif`/`#if` | SHARED-TYPEREGION-006, RH7311-002 |
| `RH7308…Analyzer.cs` | Judges a member by the innermost region | RH7308-001 |
| `RH7311…Analyzer.cs` | A region in a branch of a conditional that starts inside the type is compared with nothing | RH7311-001 |
| `RH7309…Analyzer.cs` | A region whose `#region` is inactive (excluded branch) is neither reported nor an earlier region | RH7309-002 |
| `MembersShouldBeGroupedByRegionsAnalyzerBase`, `NonOverrideMembers…AnalyzerBase` | Judge by the innermost region (see #1333/#1334 for the rest of their change) | SHARED-REGGROUP-001 |

### Notes
- The RH7309 fix needed no change for the enum safety-rail rows (decision 6): it reads `GetTopLevelRegions`, which no longer returns the enum's regions.
- RH7311 reading: "a branch" is taken as a branch of a conditional directive that starts inside the type declaration. A type wholly wrapped in `#if NET…` still has its regions compared. The spec text "outside every branch of a conditional directive" read literally would stop RH7311 from reporting anything in such a type; I took the narrower reading because the Why (two equal labels in one outline) holds there. No test covers either reading. For the spec checker / lead.

## thoenissen/Reihitsu#1333

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer/Core/PartialMemberUtilities.cs` | New `GetDeclaredSymbols`: one `(symbol, location)` per declarator of an event field, the one symbol and `GetDiagnosticLocation` otherwise | SHARED-REGGROUP-005, SHARED-DIAG-006 |
| Both RH74xx bases | Loop over `GetDeclaredSymbols`; each declarator judged, partial-part choice made on the declarator's own symbol, reported at its identifier | SHARED-REGGROUP-005 |
| RH7401–RH7412 concrete analyzers | Abstract members now take the symbol from the base: `GetExpectedRegionNames(member, symbol)` (grouping base) and `IsRelevantNonOverrideMember(member, symbol)` (override-region base). The concrete classes keep only the syntax-kind filter | SHARED-REGGROUP-005 |

The loop has no position-dependent branch, so no middle-declarator test is owed (the test implementer's condition).

## thoenissen/Reihitsu#1334

### Changes
| File | What changed | Requirement |
|---|---|---|
| `NonOverrideMembers…AnalyzerBase` | Override-region names now come from the type symbol: every non-implicit member of any kind that overrides (ordinary methods, properties, indexers, events, each event declarator), from every declaration of the type. A part in another tree is now judged by its own regions instead of counting as "in an override region". `TryGetOverrideRegionName` removed from the base and from RH7405–RH7408 | SHARED-REGGROUP-006, RH7405-001 |
| `OverrideMemberUtilities.cs` | New `GetOverrideRegionNames(ISymbol)` (empty or the introducing type's name) | SHARED-REGGROUP-003 |
| `InterfaceImplementationUtilities.cs` | `GetInterfaceRegionName` → `GetInterfaceRegionNames`: every interface whose member the member implements, in `AllInterfaces` order; the grouping base accepts a region named after any of them; the message names the first. `GetImplementedInterfaceName` (used by `DocumentationAnalysisUtilities`) is the first of them, unchanged in behavior | SHARED-REGGROUP-004, SHARED-DIAG-009 |
| `PartialMemberUtilities.cs` | `GetStandInSymbol(symbol, compilation, ct)` → `GetDefiningPartSymbol(symbol)`: an implementing part always judges the interface on its defining part's symbol, not only when the defining part is generated. Both parts now reach the same expected names, and `GetReportedPart` keeps the report to one part | RH7409-001, SHARED-DIAG-012 |

## thoenissen/Reihitsu#1339

### Changes
| File | What changed | Requirement |
|---|---|---|
| `RH7102…Analyzer.cs`, `RH7109…Analyzer.cs` | The seen-group key carries the top-level region index (`GetContainingRegionIndex`), as RH7103 does | RH7102-003, RH7109-003, SHARED-ORDER-004 |
| `RH7102…CodeFixProvider.cs`, `RH7109…CodeFixProvider.cs` | The target is the first field of the group in the same top-level region (or outside every region) | RH7102-005, RH7109-005 |

### Disputed tests or scope
- `RH7102…CodeFixTargetsTheFirstFieldOfTheGroupInTheConstantsOwnRegion` and `RH7109…CodeFixTargetsTheFirstFieldOfTheGroupInTheReadonlyFieldsOwnRegion` still fail. They assert the fix moves `K` (`_c`) in front of `_b` inside `#region B`. The target is now `_b`, as RH7102-005/RH7109-005 require, but `_b` is the first member of its region, so `#region B` is in its leading trivia and lies in the crossed span without its partner. Unchanged SHARED-ORDER-004 says: "A fix MUST NOT offer a move when … the crossed span (SHARED-ORDER-003) contains a `#region` or `#endregion` whose partner lies outside that span", and its own example (`#region Fields` / `_instance` / `_static`, RH7103) shows exactly this shape with "no code fix is offered". RH7102-006/RH7109-006 also list SHARED-ORDER-004 as withholding. So the new RH7102-005/RH7109-005 examples (spec author, cfa5e69) and these two tests contradict SHARED-ORDER-004. Making them pass would mean changing the shared move to insert behind the target's `#region` line, a change to SHARED-ORDER-004/-009 for every member reordering fix. I did not do that. Today the fix is withheld for this input, which is a valid "reported, no fix" result. Options for the lead: (a) the spec author changes the two examples so that the target is not the first member of its region (e.g. `_b0; _b; K` inside `B`) and the test implementer follows; (b) an output question if the fix should move behind the region line, which would widen the shared move. I recommend (a).

## thoenissen/Reihitsu#1337

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/SyntaxTriviaUtilities.cs` | New `LeadingTriviaContainsConditionalCompilationBoundary(token)` and `ContainsConditionalCompilationBoundaryInElementGaps(elementsWithSeparators, closeToken)`, reusing the private `IsConditionalDirective` and disabled-text test of `ContainsConditionalCompilationBoundary` | SHARED-DIAG-008 |
| `src/Reihitsu.Core/AccessorOrderingUtilities.cs` | New `ContainsConditionalCompilationBoundaryOutsideBodies(accessorList)`: the leading trivia of every token after `{` that is not inside a block or expression body (a body's first token is checked, since its leading trivia is in front of the body) | RH7107-001, RH7108-001 |
| `RH7101…Analyzer.cs` | Not reported when `[first declarator start, last declarator start)` holds a conditional boundary | RH7101-001 |
| `RH7105…`, `RH7106…Analyzer.cs` | Not reported when `[first modifier start, last modifier start)` holds one; behind the last modifier stays reported, no fix (decision 13, Q3-B) | RH7105-001, RH7106-001, SHARED-MODORDER-003 |
| `RH7107…`, `RH7108…Analyzer.cs` | Not reported when the accessor list holds one outside the bodies | RH7107-001, RH7108-001 |
| `RH5102…`, `RH5109…Analyzer.cs` | Not reported when a gap of the list holds one; inside an argument/parameter stays reported | RH5102-004, RH5109-004 |
| `RH5111…Analyzer.cs` | Not reported when the leading trivia of `=` or of the value's first token holds one (Q4-A: inside the value stays reported) | RH5111-005 |

Every check filters the four conditional kinds together, so `#elif` needs no separate test per rule (the test implementer's condition).

## thoenissen/Reihitsu#1412

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/DeclarationModifierUtilities.cs` | New `GetDeclarationsJudgedWith(member)`: the member itself, unless it is the defining part of a partial constructor (`partial`, no body); for a type declaration also its defining partial-constructor parts | RH7105-001, RH7106-001, SHARED-DIAG-004 |
| `RH7105…`, `RH7106…Analyzer.cs` | `OnDeclaration` judges every declaration `GetDeclarationsJudgedWith` returns, so the type declaration action, which is already registered, judges the defining part. Excluding the defining part from its own action keeps the count at one if a later Roslyn starts visiting it | RH7105-001, RH7106-001 |

Decision 10 is followed: only RH7105/RH7106.

## thoenissen/Reihitsu#1592

### Changes
| File | What changed | Requirement |
|---|---|---|
| `RH7309…CodeFixProvider.cs` | The type is `RegionDirectiveUtilities.GetOwningTypeDeclaration(<the reported #region trivia>)`, the same decision `GetTypeRegions`/`GetTopLevelRegions` uses for the analyzer (decision 7, one statement), instead of `FirstAncestorOrSelf<TypeDeclarationSyntax>` of the token behind the directive | RH7309-004, SHARED-FIX-005, SHARED-TYPEREGION-006 |

## Whole run

### Policy owners
- Region ownership: `rg -n "FindTrivia\(.*FirstAncestorOrSelf<TypeDeclarationSyntax>|Token\.Parent\?\.FirstAncestorOrSelf<TypeDeclarationSyntax>" src --glob '!*Test*'`: no hits after the change. The one owner is `RegionDirectiveUtilities.GetOwningTypeDeclaration`, used by `GetTypeRegions` (all region consumers) and the RH7309 fix.
- Innermost region: `rg -n "TryFindContainingRegion|TryFindInnermostContainingRegion" src --glob '!*Test*'`: RH7308 and both RH74xx bases use `TryFindInnermostContainingRegion`. `TryFindContainingRegion` has no production caller left; it stays because `RegionDirectiveUtilities.TryFindContainingRegion` has a Core test (`RegionDirectiveUtilitiesTests`, line 178). The cleaner or the lead may remove it together with that test.
- Conditional-boundary spans: `rg -n "ContainsConditionalCompilationBoundary" src --glob '!*Test*'` → `SyntaxTriviaUtilities` (primitive and the two new token/gap forms), `OrderingDeclarationUtilities.HasModifiersSplitByConditionalDirective` (RH7102/RH7103, span to the token after the last modifier), RH7101, RH7105/RH7106 (span to the last modifier, the second statement decision 13 keeps), `AccessorOrderingUtilities`, RH5102/RH5109/RH5111, and the existing callers.
- Override and interface names: `OverrideMemberUtilities.GetOverrideRegionNames`, `InterfaceImplementationUtilities.GetInterfaceRegionNames`. The latter replaces `GetInterfaceRegionName` and `PartialMemberUtilities.GetStandInSymbol`, which now have no other callers.

### Production diff size
`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'`: 37 files changed, 536 insertions(+), 380 deletions(-). Net +156 across seven issues, and no new type.

### Deleted special cases
_N/A._

### Tests run
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH7308|…RH7309|…RH7311|…RH740|…RH741"`: Passed 390/390.
- `… --filter "…RH7102|…RH7109"`: 2 failed (the disputed pair), the rest passed.
- `… --filter "…RH7101|…RH7105|…RH7106|…RH7107|…RH7108|…RH5102|…RH5109|…RH5111"`: Passed after the #1412 change (77/77 for RH7105/RH7106, the rest passed before).
- Full `--project analyzer`: Failed 2, Passed 9262, Total 9264 (only the two disputed tests).
- Full `--project core`: Passed 652/652 (includes `GetTopLevelRegionsExcludesTheRegionsOfANestedEnum`).
- Full `--project architecture`: Passed 118/118.
- Formatter and CLI not run: no changed Core helper is used outside `Reihitsu.Analyzer*` (`rg` over the changed members). The self-hosting scan was not run locally; CI runs it. RH7308, RH7311, and RH74xx now judge nested regions and branches, so the repository's own code may show new findings there.

### Disputed tests or scope
- #1339: the two RH7102/RH7109 fix-target tests and their spec examples contradict SHARED-ORDER-004 (see that section).
- #1264: the RH7311 reading of "branch" (see that section). No test.
- Scope: these public members in `Reihitsu.Core` are new, each with one or two consumers in the analyzers: `RegionDirectiveUtilities.GetTypeRegions`, `GetOwningTypeDeclaration`, `TryFindInnermostContainingRegion`, `IsInConditionalBranch`; `SyntaxTriviaUtilities.LeadingTriviaContainsConditionalCompilationBoundary`, `ContainsConditionalCompilationBoundaryInElementGaps`; `AccessorOrderingUtilities.ContainsConditionalCompilationBoundaryOutsideBodies`; `DeclarationModifierUtilities.GetDeclarationsJudgedWith`. Both RH74xx bases (public abstract classes) changed their protected abstract members' signatures. No new type, and no guard beyond a cheap syntactic check (the branch walk and the gap checks read directive and trivia kinds only).

### Architecture findings
_None._ Decision 7's structural change is done (above). Decision 13's two modifier-span statements stay apart, as decided.

### Follow-up candidates
_None_ beyond those already in `00-lead.md`.
