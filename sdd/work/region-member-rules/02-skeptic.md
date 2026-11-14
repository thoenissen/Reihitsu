Role: skeptic
Commit: cfdc2b8
Verdict: #1264: proceed, output question (Q1 widened); #1333: proceed; #1334: proceed narrower; #1339: proceed, spec amendment; #1337: proceed narrower, spec amendment, output question (Q3); #1412: proceed narrower; #1592: proceed

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Every observation below was made at cfdc2b8 (code identical to `main`; only specs changed in 187b246). How it was run:

- `scripts/apply-fix.sh <ID> <dir> --no-install` for every rule with a fix (fixtures outside the repository, LF and CRLF; only the LF arm is quoted, CRLF agreed everywhere).
- For the rules without a fix (RH7308, RH7311, RH74xx) there is no entry point, so a throw-away file-based app in the scratchpad ran the shipped analyzers (`Reihitsu.Analyzer` via `Reihitsu.Tooling`) on fixtures through `CompilationWithAnalyzers`, no preprocessor symbols defined, `LanguageVersion.Preview`; a fixture split by a marker line became one compilation of several trees. Every fixture compiled without errors.
- `scripts/format.sh` twice on one fixture for the P11 check.
- A probe analyzer (scratchpad only) to see which nodes Roslyn hands to syntax-node actions (#1412).

Fact used by several sections: Roslyn parses a `#region`/`#endregion` inside an inactive `#if` branch as a region directive, so `GetTopLevelRegions` and every rule see the regions of both branches (fixture: `#if DEBUG #region Fields … #endif` then `#region Fields` outside, DEBUG undefined → RH7311 reports the outside one).

## thoenissen/Reihitsu#1264

### Premise

Real, every instance reproduces on `main`:

| Input | Observed | Spec (after 187b246) |
|---|---|---|
| RH7308-001 example 3 (`Value`, `_other` in `Properties` nested in `Fields`) | RH7308 on `Value` only | `_other` only |
| RH7308-001 example 4 (`Value` in unrecognized `Helpers` in `Fields`) | RH7308 on `Value` | not reported |
| issue comment of 2026-10-20 (RH7401, `M` in `Methods` in `B`, `N` in `B` in `Overrides`) | RH7401 on `N` | on `M` |
| RH7405-001 new nested example | RH7405 on `O` | on `N` |
| SHARED-TYPEREGION-006 enum example (RH7311) | RH7311 on the enum's `#region Values` | not reported |
| two `#region Values` wholly inside nested `enum E` | RH7311 on the second | not reported |
| RH7311-001 new `#if`/`#else`/outside example | RH7311 on the `#else` region and on the outside region | not reported |
| `#if DEBUG #region IDisposable … #else #region Fields … #endif` (Q1 input) | RH7309 on `#region Fields`, no fix offered | Q1 |
| region in `C : IDisposable` + `enum E { #region Fields A, #endregion }` | RH7309 on the enum region; fix result `invalid-result` (moves the enum's region block out of the enum and `C`'s block into it, does not parse) | not reported |
| `enum E { #region IDisposable A, #endregion #region Values B, #endregion }` in `C : IDisposable` | RH7309 reported and **fixed**: the fix swaps `A` and `B`, changing their values | not reported |

The last two rows are safety-rail outputs on `main` (code that does not parse; changed run-time values). The run repairs them through the shared helper, so no escalation is needed, but the test implementer must pin both.

Reachability: analyzers run on any user compilation (IDE and build); the RH7309 fix through the IDE light bulb and Fix All.

### Mechanism and candidates

- Enum: `RegionDirectiveUtilities.GetTopLevelRegions` (`src/Reihitsu.Core/RegionDirectiveUtilities.cs:128`) excludes only directives inside nested `TypeDeclarationSyntax` spans. Dispatch type: `BaseTypeDeclarationSyntax` = `TypeDeclarationSyntax` (class, struct, interface, record, record struct, extension block) ∪ `EnumDeclarationSyntax`. Enum is the only member with braces that is missed; delegates and namespaces cannot hold regions inside a type body. Consumers: RH7103 (analyzer + fix), RH7110, RH7305/RH7305A (`TypesOrganizedWithRegionsAnalyzerBase`), RH7308, RH7309 (analyzer + fix), RH7311, both RH74xx bases. Only RH7309 and RH7311 judge the enum's regions themselves; for the others an enum region contains no member of the type, so their verdicts change only for a region that crosses the enum's braces, which SHARED-TYPEREGION-006 already puts outside its scope. RH7310 is per directive and does not use the helper: not involved.
- Innermost region: RH7308 (`TryGetContainingRegionName`), `MembersShouldBeGroupedByRegionsAnalyzerBase.HasMatchingRegion` and `NonOverrideMembersShouldNotBePlacedInOverrideRegionsAnalyzerBase.TryGetContainingRegionName` judge a member by the top-level region from `TryFindContainingRegion`; nested regions are never consulted.
- Branches: RH7311 and RH7309 compare all top-level regions in order and have no notion of a conditional branch.

### Breaking inputs

FC-002 (directive/branch placement) and nesting depth. Matrix for "which region judges a member" (RH7308, RH7401–RH7412): depth {top-level only, one nested, two nested}; innermost label {matching/recognized, not matching, not recognized or empty `#region`}; outer label {matching, not matching}. All members settled by the spec diff; the observed failures are every member with depth ≥ 1. The test implementer should cover at least: nested match inside non-match, nested non-match inside match (both RH7401 and RH7405), nested bare `#region` inside `#region B` (RH7401 now reports an override there — a visible change, demanded by unchanged SHARED-TYPEREGION-006), two levels of nesting.

Enum matrix (RH7309, RH7311): enum regions {one, two} × relation to the type's regions {same description, category inversion} × enum position {before, between, after the type's regions}; plus the two RH7309 fix rows above.

Branch matrix (RH7311; RH7309 per Q1): branch kind {`#if`/`#else`, `#if`/`#elif`, `#if` only} × active branch {first, second} × a third region {none, outside every branch}.

### Surfaces

| Surface | |
|---|---|
| `Reihitsu.Core` `RegionDirectiveUtilities.GetTopLevelRegions` | must change (enum) |
| RH7308 analyzer | must change |
| RH7309 analyzer | must change (enum via helper; branches per Q1) |
| RH7309 code fix | must stay; follows the helper (see #1592 for its type lookup) |
| RH7311 analyzer | must change (enum via helper; branches) |
| RH7401–RH7412 bases | must change (innermost region) |
| RH7103, RH7110, RH7305, RH7305A | must stay (helper consumers; no verdict change expected) |
| RH7310 | not involved |
| Rule pages RH7308, RH7309, RH7401–RH7412 | must change: they say "top-level region" (RH7308.md:12, RH7309.md:12, RH7405.md:12 and siblings); RH7405–RH7408 also say "used by override methods" (lead decision 4) |
| Formatter | not involved |

### Refuse option

The defects are analyzer verdicts, not fixes; the refuse analogue is "do not judge members in nested regions" (`main`'s old RH7308-001 text). It costs the same lookup as judging by the innermost region and contradicts unchanged SHARED-TYPEREGION-006, so it is not cheaper. For RH7309's enum rows, withholding the fix is not enough: the diagnostic itself is wrong. Declaring a shape unsupported is not an option: none of these shapes costs a formatter special case. _None_ simpler.

### Output questions

**Q1 (widen the spec author's Q1).** Q1 asks only about regions in two branches of one conditional. A second case of the same rule has a different answer under the two plausible readings and should be asked together:

**Q1b · RH7309 · Regions should follow a fixed category order — a region inside one branch after a region outside every branch**

Input:
```csharp
using System;

public class C : IDisposable
{
    #region IDisposable

    public void Dispose()
    {
    }

    #endregion

#if DEBUG
    #region Diagnostics

    private int _calls;

    #endregion
#endif
}
```

Before (`main`): `#region Diagnostics` reported, no fix offered.

After, A (recommended): reported, no fix offered (unchanged).

After, B: not reported.

Why A: a build with `DEBUG` shows both regions in one outline, in the wrong order; only regions of two different branches never meet. Under Q1-A plus Q1b-A, RH7309 skips only pairs from different branches of one conditional (the SHARED-TYPEREGION-006 sentence), while RH7311 keeps its existing, narrower rule (a region in any branch is compared with nothing; RH7311-001 on `main`). That difference is pre-existing in RH7311 and not in question here.

### Internal choices

- RH7308, innermost label not recognized: I agree with the spec author that SHARED-TYPEREGION-006 ("judge it by the innermost region") together with RH7308-002 decides it; no output question.
- #1264's "Done when" asks to check RH7305, RH7305A, RH7310 for the enum assumption: RH7305/RH7305A consume the helper but cannot change verdict (see Mechanism); RH7310 does not use it. Recommend no change and no test beyond the helper's own unit test in `Reihitsu.Core.Test`.

### Issue goals

- "RH7305, RH7305A and RH7310 are checked for the same enum assumption" — met by this analysis; no code change. Nothing visible.

### Architecture findings

_None._ The enum decision lives in one helper; fixing it there is the structural fix the 2026-11-06 comment asks for.

### Follow-up candidates

- RH7311 does not compare a region in a branch with a region outside every branch, although a build that compiles the branch shows both in one outline (RH7311-001 on `main`). Decided by an unchanged requirement, so only a candidate question for a later issue.

## thoenissen/Reihitsu#1333

### Premise

Real. Observed (RH7403, RH7407, RH7411):

- `public override event EventHandler A, X;` in `#region B1`, `X` from `B2`: nothing; declarators swapped: RH7403 on `X`.
- `public event EventHandler A, X;` in `#region IA`, `X` from `IB`: nothing; swapped: RH7411 on `X`.
- `public override event EventHandler A, X;` (A from `B1`, X from `B2`), `#region B2` holding a plain event `F`: RH7407 does not report `F`; with `X, A`: reported.
- `public event EventHandler F, G;` in override region: one RH7407 at `F`.
- two unregioned interface events `A, B`: one RH7411 at `A`.

### Mechanism and candidates

Every RH74xx path reads only `Declaration.Variables[0]` of an `EventFieldDeclarationSyntax`: `TryGetExpectedRegionName` (RH7403, RH7411), `TryGetOverrideRegionName`/`IsRelevantNonOverrideMember` (RH7407), `PartialMemberUtilities.GetDeclaredSymbol`, and the location `OrderingDeclarationUtilities.GetDiagnosticLocation`. Candidates: `EventFieldDeclarationSyntax` is the only multi-declarator member kind the RH74xx rules see (fields never override or implement).

### Breaking inputs

FC for element enumeration (SHARED-DIAG-006): declarators {2, 3} × which declarator differs {first, middle, last} × rule {RH7403, RH7407 as reported member, RH7407 as override-region source, RH7411} × both orders (the issue's "Done when").

New member the issue does not name: a **partial** field-like event with several declarators compiles (C# 14): `public partial event EventHandler E, F;` with two implementing parts `E { add … remove … }`, `F { … }` in `class C : I`, all outside any region → RH7411 reports `E` only today. `PartialMemberUtilities.GetDeclaredSymbol` reads `Variables[0]`, so the partial-part choice for `F` is made on `E`'s symbol. Test it for RH7411 and RH7407.

### Surfaces

| Surface | |
|---|---|
| RH7403, RH7407, RH7411 analyzers | must change |
| `PartialMemberUtilities.GetDeclaredSymbol` | must change or be bypassed for multi-declarator partial events |
| Other RH74xx rules | not involved |
| Rule pages | not involved |

### Refuse option

None applies; the issue is missing and misplaced diagnostics, no fix exists. Cost of the change: per-declarator loop in three rules. _None_ simpler.

### Output questions

_None._ SHARED-DIAG-006 decides every input.

### Internal choices

_None._

### Issue goals

Keep all. Nothing visible changes beyond what SHARED-DIAG-006 demands.

### Architecture findings

_None._

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1334

### Premise

Real for Q1, Q2, Q4 and for Q6 only in RH7409–RH7412. Observed:

| Input | Observed |
|---|---|
| Q1 (property override makes `B` an override region; `N` in it) | RH7405 nothing |
| Q2 `C : IA, IB` with `M` in `#region IB` / `D : IB, IA` | RH7409 on `C.M`; `D.M` nothing |
| Q4 override in the other part, same file | RH7405 nothing |
| Q4, the two parts in two files | RH7405 nothing |
| Q6 RH7409, defining part in `Methods`, implementing in `I` | RH7409 on the defining part |
| Q6 RH7409, defining part in `I`, implementing in `Methods` | nothing |
| RH7401-001 new partial example, both placements | reported once on the part outside `B` — already as the spec states |
| RH7405-001 new partial example, both placements | reported once on the part in `B` — already as the spec states |
| RH7405, defining part (file 1, no region), implementing part (file 2) in override region `B` | nothing |
| RH7405, defining part (file 1) in `B`, implementing part (file 2, no region) | reported on the defining part |
| RH7409, defining part (file 1) in `I`, implementing part (file 2, no region) | nothing |

So for RH7401–RH7408 in one file, Q2 of the spec author changes no visible output; it changes output only for RH7409–RH7412 and for parts in two files.

### Mechanism and candidates

- Q1/Q4: `GetOverrideRegionNames` collects overrides of the rule's own kind from the analyzed declaration's `Members` only. The set SHARED-REGGROUP-006 now describes (any kind, any declaration) is a property of the type symbol, identical for every tree.
- Q2: `InterfaceImplementationUtilities.GetImplementedInterfaceName` returns the first interface of `AllInterfaces` that maps to the member, so base-list order decides; the base class compares to that one name.
- Q6 (RH7409–RH7412): for the implementing part, `FindImplementationForInterfaceMember` returns the defining part's symbol, so the implementing part's own analysis finds no interface and returns; the defining part's analysis returns early when the defining part matches. Nobody judges the implementing part (comment of 2026-11-07).
- Two files (RH7405–RH7408): `LiesInOverrideRegion` counts a part in another tree as lying in an override region; when that part is the defining part, `SelectReportedPart` picks it, the analysis of the implementing part stays silent, and the defining part's own analysis finds no override region. Under the clarified "any part" rule this is a missed diagnostic.

### Breaking inputs

Matrix for partial members: rule family {RH7401–RH7404, RH7405–RH7408, RH7409–RH7412} × kind {method, property, indexer, event} × which part is out of place {defining, implementing, both, neither} × files {one, two} × defining part generated {no, yes}. Failing members observed: RH7409 family, implementing part out of place, one or two files; RH7405 family, implementing part in override region, two files. Members settled as holding: RH7401 and RH7405 families, one file, both placements. Unverified and to be covered: RH7401–RH7404 with parts in two files.

Q2: two interfaces × both base-list orders × region named after first / second / neither; and the derived-interface case of SHARED-REGGROUP-004 so the change does not start accepting a derived interface's name.

Q1/Q4: override kind {method, property, indexer, field-like event declarator, accessor event} × place {same declaration, other part same file, other part other file}.

### Surfaces

| Surface | |
|---|---|
| `NonOverrideMembersShouldNotBePlacedInOverrideRegionsAnalyzerBase` and RH7405–RH7408 `TryGetOverrideRegionName` | must change |
| `MembersShouldBeGroupedByRegionsAnalyzerBase` (several accepted names) | must change |
| `InterfaceImplementationUtilities`, `PartialMemberUtilities.GetStandInSymbol` | must change (Q2, Q6) |
| `OverrideMemberUtilities` | must stay |
| Rule pages RH7405–RH7412 | must change (see #1264) |

### Refuse option

None; no fix exists. The two-file case could be refused (no diagnostic when the deciding part is in another tree), but the clarified override set is computable from the symbol for every tree, so refusing saves nothing. Not rare: partial types across files are common, partial members across files less so.

### Output questions

_None new._ On the spec author's Q2: the triage comment of 2026-10-24 states the rule; observed outputs show that for RH7401–RH7408 in one file nothing visible changes, so whatever the lead decides about Q2 only affects RH7409–RH7412 and two-file inputs.

### Internal choices

- SHARED-DIAG-009 third bullet ("a member of another kind that the verdict is not about") can be read against Q1 (the property override is a member of another kind). The issue's Q1 expectation and the triage comment resolve it for "any kind"; I agree with the spec author.

### Issue goals

- "the change doc is written" — dropped by the triage comment (bug-fix lane). Nothing visible.

### Architecture findings

_None._

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1339

### Premise

Real. RH7102 and RH7109 analyzers have no region handling at all. Observed with `apply-fix.sh`: the issue's RH7102 input, the #1664 input (field outside, constant in a region), the RH7109 input — all reported, no fix offered.

### Mechanism and candidates

RH7102/RH7109 iterate `typeDeclaration.Members` with one per-group set and never consult `GetTopLevelRegions`, unlike RH7103 and RH7110 (SHARED-ORDER-004).

### Breaking inputs

Region placement {both outside, both in one region, both in one region one of them nested, different regions, one outside one inside} × order of the regions × group {same, other}.

Fix target — a spec gap: today `RH7102ConstantsMustAppearBeforeFieldsCodeFixProvider.TryGetTargetMember` (and the RH7109 counterpart) picks the first earlier field of the group anywhere in the type. Input:

```csharp
class C
{
    #region A

    private int _a;

    #endregion

    #region B

    private int _b;

    private const int K = 1;

    #endregion
}
```

Observed: RH7102 on `K`, no fix offered (target `_a` crosses the region). After the analyzer change, `K` is still reported (against `_b`); RH7102-005 says the fix targets "the first earlier field … of the same accessibility group (RH7102-001)". Read with -005's Why ("the earliest position at which the constant satisfies the rule") and SHARED-ORDER-004, the target is `_b`, and the fix should be offered. I recommend the spec author write "in the same top-level region, or outside every region" into RH7102-005 and RH7109-005, and the test implementer pin the input (same shape for RH7109). If the lead reads the current text as also admitting "target `_a`, fix withheld", it becomes an output question (fix offered vs. not).

Tests that flip: `RH7109…MixedAccessibilityInterfaceViolationWithDirectivesHasNoCodeFix` (characterization, expects the diagnostic) and `RH7102…NoCodeFixWhenDirectivesAreInLeadingTrivia` (asserts only "no action"; the issue comment asks it to assert "no diagnostic"). Tests with regions inside an excluded `#if` branch (`FixMovesAFieldWithAGroupWhoseExcludedBranchHoldsARegionAroundWholeMembers`) keep their verdict: both fields lie outside every region, because the region pair lies wholly between them.

### Surfaces

| Surface | |
|---|---|
| RH7102, RH7109 analyzers | must change |
| RH7102, RH7109 code fixes | must change (target within the region) |
| Specs RH7102-005, RH7109-005 | clarification (above) |
| Rule pages | not involved |

### Refuse option

Keep the fix target as is; then every in-region inversion whose group also has a field in an earlier region stays without a fix. The inputs are ordinary region-organized classes, not rare; the change is a region-index comparison, as RH7103's fix already does. Not recommended.

### Output questions

_None_, unless the lead reads RH7102-005/RH7109-005 as above.

### Internal choices

_None._

### Issue goals

- "the change doc is written" — dropped by the triage comment. Nothing visible.

### Architecture findings

_None._

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1337

### Premise

Real for RH7101, RH7105, RH7106, RH7107, RH7108 (no directive handling in any of them). Observed (`apply-fix.sh`): RH7101 `_a, #if DEBUG _debug, #endif _b` → reported, no fix; RH7105/RH7106 `static #if FEATURE #endif public int Value` → reported, no fix; RH7107/RH7108 `#if !NEVER set/remove … #endif get/add` → reported, no fix.

Correction to the issue's instances: the RH7107 instance with `#if DEBUG` around `set` is **not reported** on `main` when `DEBUG` is undefined, because the `set` is then disabled text and the list has one accessor; it reproduces only in a build that defines the symbol. Tests must use a symbol that is defined, or an `#if !UNDEFINED` form, to reproduce.

Additional observed outputs:
- RH7107/RH7108 with an empty `#if DEBUG`/`#endif` pair between the accessors: reported and **fixed** — the fix moves `set`/`remove` behind the second accessor, so the pair ends up in front of `get`/`add`. Under SHARED-DIAG-008 this input is not reported.
- RH7107/RH7108 with `#if DEBUG … #endif` **inside an accessor body**: reported and fixed (the accessor moves with its body). SHARED-DIAG-008 keeps this reported ("a body the rule does not judge").
- RH5102 with `#if DEBUG … #endif` inside a lambda body that is the third argument of `Run(1, 2, () => { … })`: reported, no fix (RH5102-004 on `main`).

### Mechanism and candidates

The five analyzers report from the active token list alone. Judged spans and their dispatch: RH7101 `VariableDeclarationSyntax.Variables` of `FieldDeclarationSyntax`; RH7105/RH7106 `DeclarationModifierUtilities.GetModifiers` for the kinds SHARED-MODORDER-001 lists; RH7107 `AccessorListSyntax` of `PropertyDeclarationSyntax`/`IndexerDeclarationSyntax`; RH7108 `AccessorListSyntax` of `EventDeclarationSyntax`; RH5102 `ArgumentListSyntax`; RH5109 `ParameterListSyntax`; RH5111 the five construct forms RH5111-001 lists.

### Breaking inputs — and two spec defects in 187b246

1. **RH5102-004 / RH5109-004 over-reach.** The new text says a list with a conditional directive "between its parentheses" is not reported. That includes the inside of an argument — a lambda or anonymous-method body, an object or collection initializer, a nested call — which SHARED-DIAG-008 excludes ("a conditional directive elsewhere in the construct, such as in a body the rule does not judge, MUST NOT keep the construct from being reported") and which cannot change where any argument of this list starts in any build. As written, the spec diff turns the observed RH5102 lambda case above (reported today) into "not reported" — common code (`#if DEBUG` logging in a lambda passed to a wrapped call). It also disagrees with how the same commit scoped RH5111-005 (the gaps around `=` only). The judged span RH5102-001/RH5109-001 describe is where each element starts: the gaps between `(`, the elements, the commas, and `)`. I recommend the spec author narrow both texts to those gaps; then today's output for the lambda case stays (reported, no fix) and nothing visible is in question. If the lead keeps "between its parentheses", the lambda case is an output question.
2. **RH7107-001 / RH7108-001 unchanged, but literal.** They exclude a list that "contains" a conditional directive; an accessor body is inside the accessor list. Implemented literally, the body case above (reported and fixed today) loses its diagnostic and its fix. SHARED-DIAG-008 keeps it. I recommend the spec author clarify "between the accessors, outside their bodies" as a clarification; nothing visible changes.

Matrix for the class (SHARED-DIAG-008, conditional directive in a judged span) — every member the test implementer must cover per rule: directive shape {empty `#if`/`#endif` pair, `#if` around an element, `#if`/`#else` alternatives, `#elif`} × active branch {first, second, none} × position {between elements, before the first element inside the span, behind the last element inside the span, inside an element's body or initializer (must stay reported)} × non-conditional directive {`#region`/`#endregion`, `#pragma`, `#nullable`} (must stay reported, no fix). RH7101 member to pin: `#if` inside the initializer of a non-last declarator (RH7101-001's "between its declarators" makes it not reported) vs. of the last declarator (reported). RH5111: the existing test `VerifyDeclaratorWithDirectiveIsReportedWithoutCodeFix` uses `#if !UNDEFINED_SYMBOL`, so its excluded code is empty; place of the pair decides.

Tests that flip (all hold an empty conditional pair in the judged span): `RH7105…MisorderedModifiersWithDirectiveAreReportedWithoutFix`, `RH7106…MisorderedCompoundAccessModifiersWithDirectiveAreReportedWithoutFix`, `RH7107…`/`RH7108…NoCodeFixWhenDirectivesAreInAccessorLeadingTrivia`, `RH7107…`/`RH7108…NoCodeFixWhenDirectiveFollowsAccessorAttributeList`, `RH5102…`/`RH5109…VerifyDirectiveInside…ListWithholdsTheCodeFix`, `RH5111…VerifyDeclaratorWithDirectiveIsReportedWithoutCodeFix` (directive between target and `=`).

P11 note: the formatter splits `M(1, 2,⏎#if DEBUG⏎#endif⏎3)` into one argument per line (and the parameter-list counterpart); stable on the second pass. Once RH5102/RH5109 stop reporting it, this is analyzer-clean code the formatter changes — the rule's own construct, decided by SHARED-DIAG-008 in the rule's spec, so it is intended (P11), not a bug.

### Surfaces

| Surface | |
|---|---|
| RH7101, RH7105, RH7106, RH7107, RH7108 analyzers | must change |
| RH5102, RH5109, RH5111 analyzers | must change |
| Their code fixes | must stay (they are no longer reached for the excluded inputs) |
| `OrderingDeclarationUtilities.HasModifiersSplitByConditionalDirective` (`src/Reihitsu.Core`) | existing helper, used by RH7102/RH7103; its span runs from the first modifier to the token after the last one (see Q3) |
| Specs RH5102-004, RH5109-004, RH7107-001, RH7108-001 | clarification (above) |
| Rule pages | not involved (general-notes covers directives) |

### Refuse option

Not applicable: the change removes diagnostics; no fix is added. Declaring a conditional directive inside a modifier, accessor, or argument list unsupported would refuse whole files for the formatter, deletes no formatter special case this run knows of, and refuses common code (`#if DEBUG` inside lambda bodies). Not recommended; named because the README asks for it.

### Output questions

**Q3 — keep, with a stronger reason for A.** The spec author's Q3 (RH7105/RH7106, conditional directive between the last modifier and the type) stands as written. Add to its Why: RH7102-001 and RH7103-001 already define the judged modifier span as "from its first modifier up to the token that follows its last modifier", and the shared helper `HasModifiersSplitByConditionalDirective` implements exactly that span; reading B would give RH7105/RH7106 a second, different definition of the same span (see Architecture findings).

Consequence of A worth showing the maintainer as a second input, because there the excluded code is not a modifier:

```csharp
class C
{
    static public
#if WIDE
    long
#else
    int
#endif
    _a;
}
```

Before (`main`): `static` reported by RH7105, no fix offered. After A: not reported. After B: reported, no fix (as today). Recommendation stays A: the check stays one cheap span test shared with RH7102/RH7103, and the input is rare.

**Q4 — agree with A** (RH5111 directive inside the value: reported, no fix, unchanged).

### Internal choices

- The two spec defects above are decided by SHARED-DIAG-008 and keep today's output; recommend the lead route them to the spec author as clarifications before the test implementer.

### Issue goals

- "the listed tests expect no diagnostic" — correct for all seven named tests; each holds only an empty conditional pair in its judged span.
- "the change doc is written" — dropped by the triage comment. Nothing visible.

### Architecture findings

- **One rule, two statements (conditional on Q3).** The judged span of a modifier list under SHARED-DIAG-008 is stated in RH7102-001, RH7103-001 and implemented once in `OrderingDeclarationUtilities.HasModifiersSplitByConditionalDirective`; SHARED-MODORDER-003 states, for RH7105/RH7106, that a conditional directive behind the last modifier keeps the report. With Q3-A the two agree and RH7105/RH7106 can use the one helper; with Q3-B a second span definition is needed and the two statements stay apart.

### Follow-up candidates

- **RH5101-004** has the same wording as RH5102-004 on `main` and the same test (`RH5101…VerifyDirectiveInsideArgumentListWithholdsTheCodeFix`, empty `#if DEBUG`/`#endif` inside the argument list). RH5101's judged span is only the gap between `(` and the first argument, so whether that test's pair lies in it decides whether RH5101 needs the same alignment. More generally, about 60 rule specs state that "a comment or a preprocessor directive" withholds the fix and "the diagnostic is still reported" without referring to SHARED-DIAG-008 (e.g. RH5501–RH5531, RH3001–RH3105, RH7301, RH7303); a sweep for conditional directives inside their judged spans is a candidate, sibling of #1275.
- Formatter, rare shape, not destructive: `var value⏎#if DEBUG⏎#endif⏎            = 1;` is formatted to `= 1;` at the statement's column (8), not as a continuation line; stable. Candidate question only.

## thoenissen/Reihitsu#1412

### Premise

Real. Observed (`apply-fix.sh RH7106`): `internal protected partial C();` + `internal protected partial C() { }` → only the implementing part reported (and fixed); the defining part never.

### Mechanism and candidates

Not in Reihitsu's code: the Roslyn analyzer driver (Microsoft.CodeAnalysis 5.9.0, the version `Reihitsu.Tooling` references) never hands the **defining part of a partial constructor**, nor any node inside it (its `ParameterList`, `Parameter`s, types), to a syntax-node action. Probe analyzer registered for constructors, methods, properties, events, indexers, parameters, types, over one compilation: visited both parts of partial methods, properties, indexers and events, and of every partial constructor only the implementing part (class and struct alike). With the implementing part in a file named `*.g.cs` (generated) and `GeneratedCodeAnalysisFlags.None`, no constructor node of either part was visited. RH7105/RH7106 register `SyntaxKind.ConstructorDeclaration` and have no filter of their own. Dispatch candidate set: `ConstructorDeclarationSyntax` with `partial` and no body; no other member kind is affected.

### Breaking inputs

Part {defining, implementing} × implementing part {same file, other file, generated file} × rule {RH7105, RH7106} × type kind {class, struct, record}. The generated-implementing-part case (the source-generator pattern partial constructors exist for) yields no diagnostic for the hand-written defining part today, and a fix that relies on the implementing part's analysis would not reach it either.

### Surfaces

| Surface | |
|---|---|
| RH7105, RH7106 analyzers | must change (reach the defining part another way) |
| RH7105, RH7106 code fixes | must stay (SHARED-DIAG-004: change only the reported part) |
| Every other syntax-node rule on constructors or their parameters | not involved in this run; see follow-up |

### Refuse option

Leave the defining part unreported: a missing diagnostic on a C# 14 construct, rare today; costs nothing. Fixing it needs a mechanism the analyzers do not use now (the node never reaches the rule), so its cost is a new code path in two analyzers, plus its own test of the generated-part case. Severity is low: a modifier-order diagnostic on one part of a rare member. Recommend proceed narrower — RH7105/RH7106 only, as the issue asks — and keep the other rules out of the run.

### Output questions

_None._ RH7105-001, RH7106-001 and SHARED-DIAG-004 decide the input.

### Internal choices

_None._

### Issue goals

Keep. Nothing visible beyond what the specs demand.

### Architecture findings

_None_ within the README's definition (the cause is in the host, not two Reihitsu components).

### Follow-up candidates

- Every Reihitsu rule that registers a syntax-node action on `ConstructorDeclaration`, `ParameterList`, `Parameter`, or nodes inside a constructor declaration silently skips the defining part of a partial constructor (e.g. documentation, naming, accessibility, parameter-layout rules). Structural cause: Roslyn 5.9 does not run syntax-node actions on that declaration. Candidate issue to enumerate the affected rules.

## thoenissen/Reihitsu#1592

### Premise

Real, but shape 2 is the same defect as shape 1, and shape 1 today is not "offered and changes nothing" in the issue's own input. Observed (`apply-fix.sh RH7309`):

| Input | Observed |
|---|---|
| issue shape 1 (`#region Fields` block starts with `private class Other { }`, `Other` has no regions) | reported, **no fix offered** |
| block starts with `[Serializable] private record R(int A);` | reported, no fix offered |
| block starts with a nested class whose own regions are in order | reported, fix offered, `no-progress` (changes nothing) |
| block starts with a nested class whose own regions are out of order | the fix for `C`'s diagnostic reorders the **nested** class's regions, then `no-progress` |
| block starts with an extension block in a static class | reported, no fix offered |

The issue's shape 2 ("blocks already in order, identity permutation") arises only through the wrong type: the analyzer and the fix share `GetTopLevelRegions` and `RegionCategoryUtilities`, and a stable sort by category of a sequence with an inversion is never the identity. Once the type lookup is right, shape 2 has no input of its own; it gets a characterization test only if the test implementer can construct one.

### Mechanism and candidates

`RegisterCodeFixesCoreAsync` (`RH7309RegionsShouldFollowCategoryOrderCodeFixProvider.cs:317`) resolves the type as `FindTrivia(…).Token.Parent.FirstAncestorOrSelf<TypeDeclarationSyntax>()`: the token that owns the `#region` trivia is the next token after the directive, so whenever the block's first member is a nested `TypeDeclarationSyntax` (class, struct, interface, record, record struct, extension block; with or without attributes or a documentation comment in front), the lookup lands on the nested type. A nested enum or delegate first is fine (not a `TypeDeclarationSyntax`). An empty block whose next token is the type's `}` is fine.

### Breaking inputs

First member of the reported block {nested class, struct, interface, record, record struct, extension block} × attributes/doc comment in front {no, yes} × nested type's own regions {none, in order, out of order}; plus the enum rows of #1264. Expected per RH7309-004: the outer type's blocks reorder; the nested type is untouched.

### Surfaces

| Surface | |
|---|---|
| RH7309 code fix (type lookup) | must change |
| RH7309 analyzer | must stay |
| Fix All | involved: `WellKnownFixAllProviders.BatchFixer` applies the per-diagnostic action, so it uses the same lookup |

### Refuse option

Withhold the fix whenever the looked-up type is not the type that owns the region: cheap, but region blocks that start with a nested type (`#region Nested types`) are ordinary code, and RH7309-004 requires the fix there. Not recommended. The requested change is as cheap.

### Output questions

_None._ RH7309-004 and SHARED-FIX-005 decide both shapes.

### Internal choices

_None._

### Issue goals

- Shape 2 "has a test that asserts the verdict": it has no input apart from shape 1's cause; recommend covering it through the nested-type rows (in order / out of order) instead of a separate identity input. Nothing visible.

### Architecture findings

- **Analyzer and code fix own the same decision.** Which type declaration a `#region` belongs to is decided by the analyzer (it walks `GetTopLevelRegions` of the declaration it analyzes, SHARED-TYPEREGION-006) and decided again, differently, by the fix (`FirstAncestorOrSelf<TypeDeclarationSyntax>` of the token after the directive). The inputs above show them disagreeing. Not a rare class.

### Follow-up candidates

_None._

## Whole run

- No shape in the run's issues costs a formatter special case, so declaring a shape unsupported (escalation 10) is not an option anywhere in this run.
- Spec amendments recommended before the test implementer: RH5102-004, RH5109-004 (judged span = the gaps between the list's elements and parentheses), RH7107-001, RH7108-001 (outside accessor bodies), RH7102-005, RH7109-005 (fix target within the region). Each keeps today's output for the inputs it protects.
- Output questions for batch 1: the spec author's Q1 plus Q1b (#1264), Q2 (lead's confirmation, #1334), Q3 with the second input (#1337), Q4 (#1337).
- Safety-rail outputs on `main` that the run repairs: RH7309 fix on regions in a nested enum (unparseable result; reordered enum members change values).
- Scratch fixtures and probe apps were kept outside the repository and deleted.
