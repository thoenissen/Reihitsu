Role: skeptic
Commit: d7fc7c4
Verdict: #1507: proceed narrower; #1428: proceed

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Shallow clone (54 commits): the history of the stale names in `AGENTS.md` is not available, so no claim below rests on it.

## thoenissen/Reihitsu#1507

### Premise

Real, as a regression guard; it repairs nothing today. No caller question applies: the change is an architecture test over a repository convention, and no shipped surface is involved (`AGENTS.md`, *Supported inputs*, has nothing to decide).

- `src/Reihitsu.ArchitectureTests/Formatter/PhaseOwnershipDocumentationTests.cs` checks only that each runtime phase appears once and in order; nothing reads the third column. Confirmed.
- I re-extracted the guard column (third cell of each of the 13 rows between the markers at `AGENTS.md:173` and `AGENTS.md:195`): 63 distinct backticked spans, no backticked span contains `|`, and column 2 holds no identifier at all (`#region`, `#endregion`, `#endregion // Description`, `///`, `/** */`, `param:`, `typevar:`). This matches the spec author's count and split: 58 resolve to declarations, `EndOfLineTrivia` and `XmlTextLiteralNewLineToken` are Roslyn `SyntaxKind` members, `?`, `:` and `sdd/specs/formatter/unsupported-syntax.md` are no identifiers.
- Every one of the 58 is declared in `Reihitsu.Formatter` or `Reihitsu.Core`, none only elsewhere. Declared in `Reihitsu.Core`: `AreInSameGroup` (also Formatter), `AreSeparatedByEndOfLine`, `BlankLineSpacingPolicy.IsTerminalDirectSwitchSectionBreak`, `DocumentationCommentUtilities.IsWrittenBehindSource`, `DocumentsFollowingCode`, `GetEndRegionText`, `RegionDirectiveUtilities.CreateCanonicalEndRegionTrivia`, `TokenGapAnalysis`, `UsingDirectiveOrderingSafety.CanSafelyReorder`; all others in `Reihitsu.Formatter`. `Reihitsu.Formatter.csproj` references only `Reihitsu.Core`.

### The spec author's resolution rule misses the issue's own example

Decision 1 resolves a plain name when *any* production project under `src/` declares a type or member with that name. One of the two stale names the issue cites is still declared in a production project, in an unrelated code fix:

```
src/Reihitsu.Analyzer.CodeFixes/Rules/Clarity/RH3101DoNotPrefixCallsWithBaseUnlessLocalImplementationExistsCodeFixProvider.cs:55:    private static bool CanRewrite(SyntaxNode root, SemanticModel semanticModel, ExpressionSyntax node, CancellationToken cancellationToken)
```

So if `CanRewrite` returned to the guard column, the test as specified would pass (FC-017: the assertion is weaker than the guarantee). `rg '\bCanRewrite\b|\bIsAfterSourceOnSameLine\b' src/Reihitsu.Formatter src/Reihitsu.Core` finds nothing. The seed cases the spec author lists cover `IsAfterSourceOnSameLine` only, which no project declares, so the tests would not show this gap (FC-034: a simplified cousin of the reported shape).

Narrower, principled scope: resolve against `Reihitsu.Formatter` and the projects it references (today `Reihitsu.Core`). A formatter phase can only call code in those assemblies, so a guard declared anywhere else cannot be that phase's guard. All 58 names pass under it, and `CanRewrite` fails. This changes no visible output and needs no `AGENTS.md` edit.

### Mechanism and candidates

_N/A._ This is not a bug fix.

### Breaking inputs

Relevant classes: FC-017 (the resolution rule above), FC-034 (seed case), FC-003 (both sides of each boundary).

Cases the test implementer must cover, beyond the spec author's list:

- a plain guard name declared only in `Reihitsu.Analyzer.CodeFixes` (the `CanRewrite` shape) fails. This is the reported shape itself.
- a plain guard name declared in `Reihitsu.Core` only (such as `AreSeparatedByEndOfLine`) passes, the other side of the scope boundary.
- a name that appears in production source only inside a comment or a string literal fails. Resolution reads declarations, not text, so a check by `rg`-style text matching is not enough.
- a name declared only in a test project fails. The spec author already excludes test projects, but no seed case shows it.
- `Type.Member` where the member exists but on a different type fails (the spec author's case). Also `Type.Member` where the type is declared only outside the scope fails.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `src/Reihitsu.ArchitectureTests/Formatter/` | must change (new test) |
| `AGENTS.md` | must stay (read only; editing it widens to tooling) |
| Analyzer, formatter phases, code fixes, Fix All, `Reihitsu.Core`, rule docs, CLI | not involved |
| specs under `sdd/specs/` | not involved |

### Refuse option

No change: a stale guard name costs an agent a failed search and a misleading pointer in its instructions. The requested change costs one test method, about 100 test lines, and no production line, new production type, or new mechanism. The table changes often (7 of the last 54 commits touch `AGENTS.md`), so the guard pays for itself. Declaring a shape unsupported is not applicable. No unsupported-shape or code-fix option exists for a documentation check.

Simpler option: the test project already loads the production assemblies (`ProjectReference` to `Reihitsu.Formatter` and `Reihitsu.Core`, plus ArchUnitNET), and `SpecCatalog.EnumerateProductionFiles` already scans production sources. Either is existing infrastructure. Which one to use is the implementer's choice, not mine.

### Output questions

_None._ Only an architecture test changes.

### Internal choices

1. **Resolution scope** (see above). Options: (a) any production project under `src/`, as in decision 1. It lets `CanRewrite` pass. (b) `Reihitsu.Formatter` and the projects it references. **Recommend (b):** it catches the issue's own example at no cost, and every current name passes.
2. **Slash-continued names.** Seven groups write `Type.A`/`B`, meaning `Type.B` (`ListElementInteriorJoinRewriter.CanJoin`/`IsOwnedGap`, `AttributeListShapeUtilities.SplitAttributeLists`/`MergeAttributeLists`/`UsesMergedListShape`, `RegionNamingRewriter.Rewrite`/`BuildRegionTrivia`/`BuildUndescribedEndRegionTrivia`, `RegionDirectiveUtilities.CreateCanonicalEndRegionTrivia`/`GetEndRegionText`, `LineBreakTriviaUtilities.CreateJoinSeparator`/`CreateJoinedTrailingTrivia`, `BlockWrapTriviaUtilities.GetCarriedLeadingTrivia`/`GetCarriedTrailingTrivia`). Each continued member is declared on the leading type today. But the slash also joins peers (`BlankLineEditor`/`TokenGapAnalysis`, `EndOfLineTrivia`/`XmlTextLiteralNewLineToken`). Options: (a) treat a continued span as a plain name, as decision 1 does. (b) Inherit the qualifier. That is a parse rule over prose punctuation, and a future `Type.A`/`PeerType` would fail falsely and force an `AGENTS.md` rewording. **Recommend (a).** With choice 1(b), the plain-name check is already scoped to the formatter's assemblies.
3. **Which spans are skipped.** Decision 1 skips every span that is not a (dotted) identifier. So a future `IsMultiLineSection()` or `Foo<T>` would be skipped silently. Options: (a) skip non-identifiers, as decided. (b) Skip only spans that contain no letter or contain `/` (today `?`, `:`, the path), and require everything else to resolve, so a decorated name fails loudly. **Recommend (b), weakly.** It fails closed at the same cost. Its price is that a future prose span with letters in the guard column (such as `#if`) fails and needs the predicate or the row adjusted. Pick (a) if the lead prefers no future friction.

### Issue goals

- "every backticked identifier in the table's guard column resolves to a declaration under `src/`": change it. As written, it fails today on `EndOfLineTrivia` and `XmlTextLiteralNewLineToken` (spec author, confirmed), and "under `src/`" is too wide to catch `CanRewrite` (above). Adjusted goal: every identifier span of the guard column resolves to a declaration in `Reihitsu.Formatter` or a project it references, or to a `SyntaxKind` member. The issue's purpose is met, so the PR can still close it. No visible output changes.
- "fails naming the identifier that does not resolve": keep. The failure should name every unresolved span at once, not stop at the first, so one run reports all stale names.

### Architecture findings

_None._

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1428

### Premise

Real, and latent on both counts: `ExemptedSpecs` is empty (`RequirementCoverageTests.cs:27`), and the issue keeps it empty. No caller question applies, since no shipped surface is involved.

- **`Uses` limited to `shared/`.** `IsEnforced` (`RequirementCoverageTests.cs:710-715`) keeps an exempted spec's requirement enforced only when `SpecPath` starts with `SharedDirectory` (`"shared/"`) and a non-exempted spec uses it. Confirmed. FMT-CLEAN-001 appears under `Uses` in the eight analyzer specs the spec author names (RH5602, RH6006, RH6007, RH6013, RH6018, RH6019, RH6020, RH6022). Listing a non-shared ID under `Uses` is legitimate: `sdd/README.md:49` asks a caller's spec to list the requirement IDs of the spec that owns the type it calls. So the `shared/` restriction contradicts the rulebook's own use of `Uses`, and widening it is right.
- **Orphan keys.** `IsEnforced` and `DescribeCoverageOfExemptedSpecs` both match keys with `ContainsKey(requirement.SpecPath)` (ordinal). A key that equals no `SpecPath` is silently inert. Confirmed. `CoverageCheckDoesNotExemptTheSpecsOfAnExemptedDirectory` (`:350`) shows that for a directory key.

### Mechanism and candidates

_N/A._ This is not a bug fix.

### Breaking inputs

Relevant classes: FC-003 (both sides of the `Uses` condition and of the key check), FC-031 (stale comments), FC-015 (one statement of the key set).

The key check must compare ordinally against the paths `SpecCatalog.EnumerateSpecFiles` yields, as decision 2 says. Two more reasons, beyond the index: `IsEnforced` matches ordinally with `/`, so any key that differs in case or separator exempts nothing. And `File.Exists` would accept such a key on a case-insensitive file system (macOS is a supported platform per `AGENTS.md`). Cases to add to the spec author's list:

- `Formatter/Cleanup.md` (wrong case, the file exists as `formatter/cleanup.md`) fails.
- `formatter\cleanup.md` (backslash) fails.
- `requirement-ids.txt` (a file under `specs/` that is no spec) fails.
- several bad keys are all named in one failure, not only the first.

The existing tests stay valid. `CoverageCheckDoesNotExemptTheSpecsOfAnExemptedDirectory` asserts only `FindUncoveredRequirements`, which the key check does not change. The two `shared/` `Uses` tests hold under the wider rule, as the spec author says.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `src/Reihitsu.ArchitectureTests/Sdd/RequirementCoverageTests.cs` | must change: the key check, `IsEnforced`, and the class summary (`:14-16`), `SharedDirectory` and its summary (`:29-33`), `IsEnforced`'s summary (`:703-704`), the summary of `FindUncoveredRequirements` (`:608-609`), and the name `ReadSharedIdsUsedByEnforcedSpecs` (FC-031) |
| `sdd/README.md` | changed by the spec author (0b74f22); see below |
| `ExemptedSpecs` contents | must stay empty |
| `SpecStructureTests` (`Uses` validation) | must stay |
| Every shipped surface | not involved |

### The spec author's text

`sdd/README.md:83` and `:172-174` (0b74f22) state both rules and agree with each other and with `:176` ("fails only through the `Uses` case above"). No other document states the `shared/`-only rule: `rg` over everything outside `src/` and `sdd/work/` finds it nowhere else. Every remaining statement of it is in `RequirementCoverageTests.cs` and is listed above. No change requested to the text.

One claim in the handover is incomplete. Under *The report*, it says that once orphan keys fail, the report "no spec is exempted" can no longer mislead. But `DescribeCoverageOfExemptedSpecs` (`:644-664`) builds its lines from *active requirements* whose spec path equals a key. A valid key that names a spec with no active requirement, such as a spec whose requirements are all retired, still yields "no spec is exempted". No such spec exists today (every spec has at least one requirement heading), so this is latent. See internal choice 2.

### Refuse option

No change: both gaps are inert while the list is empty, and the list is meant to stay empty. The requested change costs a few dozen test lines and no production code. Declaring a shape unsupported is not applicable.

Simpler alternative, which goes further than the issue: delete the exemption mechanism (`ExemptedSpecs`, the exemption branch of `IsEnforced`, the `Uses` lookup, the report, and their tests). Both gaps would vanish with it. The list has been empty for as long as this clone shows, and `sdd/README.md:83` says a PR that adds a requirement adds its test in the same PR. But `sdd/README.md` describes coverage as opt-out with exemptions by design, so removing it is a rulebook change outside this issue. **Recommend proceeding as the issue asks.** The lead may name the deletion as a follow-up candidate if it agrees the mechanism is dead.

### Output questions

_None._ Only an architecture test and the SDD rulebook change.

### Internal choices

1. **Key set** (decision 2): an ordinal match against `SpecCatalog.EnumerateSpecFiles`, rather than `File.Exists`. Agree. The case and backslash cases above are a further reason.
2. **The report.** Options: (a) leave it, as the spec author decided. It stays misleading for a valid key whose spec has no active requirement. (b) Build the report's lines from the keys, so each exempted spec gets a line even at `0 of 0 uncovered`, and "no spec is exempted" prints only for an empty list. **Recommend (b).** It closes the symptom the issue names, it is a change of a few lines in a method the run already edits, and it needs one test: a valid key whose fixture spec has only retired requirements. If the lead keeps (a), the PR should not claim the report symptom is fixed.

### Issue goals

- "Add a check that fails when a key does not name an existing spec file, naming the key": keep, with "spec file" read as decision 2 defines it, not as `File.Exists`.
- "with a test for a misspelled file and for a directory key": keep, and add the case, separator, index, and non-spec-file cases above.
- "a test on each side of that condition": keep. The spec author's cases cover both sides.
- "`sdd/README.md` states both rules": done in 0b74f22.
- "the exemption list stays empty": keep.

The issue does not list the report symptom as a goal, though its scope text names it. See internal choice 2. No goal change alters visible output.

### Architecture findings

_None._

### Follow-up candidates

- Possibly: remove the requirement-coverage exemption mechanism if it is dead (see *Refuse option*). This is internal and the lead decides. Only file it if the lead agrees the escape hatch has no remaining use.
