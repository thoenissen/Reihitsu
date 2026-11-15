Role: skeptic
Commit: 5fdb9b7
Verdict: #1680: proceed wider; #1742: output question (and escalation 9 if the lead wants the structural fix)

How everything below was established: a throwaway probe outside the repository (a file-based app referencing `src/Reihitsu.Analyzer`, deleted afterwards) that runs either every `[DiagnosticAnalyzer]` of `Reihitsu.Analyzer` with every ID forced to warning, or a probe analyzer that logs every syntax-node callback with its node, span, and `ContainingSymbol`, over a `CSharpCompilation` with `DocumentationMode.Diagnose` and `LanguageVersion.Preview`. It was run with Microsoft.CodeAnalysis 5.9.0 (the version the tests use) and again with 5.12.0 (the compiler of the installed SDK 11.0.100); both give identical results for every probe below. `scripts/apply-fix.sh` was used for the code-fix check. The prompt carried no intended fix, cause, or verdict.

## thoenissen/Reihitsu#1680

### Premise

Real, and wider than reported. Driver facts (node probe):

- For a type declaration with a parameter list (record, record struct, class, struct, with or without parameters), every **structured-trivia node** — documentation comments with all their `Xml*` descendants, and preprocessor directives such as `#region`/`#endregion` — attached to a token of the type declaration is handed to syntax-node actions **twice**: once with the type as `ContainingSymbol`, once with the primary constructor.
- Excepted: trivia of tokens inside an attribute list (a doc comment in front of `[Obsolete]` is visited once), trivia inside the parameter list and inside the base type's argument list `: B(a)` (visited **only** in the constructor run), and trivia inside member declarations (visited once, for the member).
- Included: trivia between an attribute list and the first modifier, between modifiers, in the base list, in front of `;` of `record R(int A);`, and in front of the closing `}` of the body.
- No non-trivia node is visited twice (the `RecordDeclaration`, `ParameterList`, `Parameter` nodes are visited once each).

Observed duplicates with all analyzers (same ID, same span, count 2):

```csharp
///<summary>Customer.</summary> trailing                 // RH8308 x2
///
/// <remarks>x</remarks><param name="A">a</param>      // RH8304 x2, RH8309 x2
public record R(int A);

/// <summary>
/// Customer <placeholder>x</placeholder>.              // RH8204 x2
/// </summary>
public record R2(int A);
```

and, the common shape, in front of the closing brace of a class with a primary constructor:

```csharp
public class Service(int value)
{
    #region Properties
    public int Value => value;
    #endregion // Member Implementation                 // RH7306 x2

    #region lower                                       // RH7302 x2, RH7310 x2
    #endregion
}

public class Other(int value)
{
    #region Properties
    public int Value => value;
    #endregion // Members                               // RH7307 x2
}
```

`#region lower region` in front of `public struct S(int a)` and `#region Empty` / `#endregion` in front of a primary-constructor type also give RH7302 x2 and RH7310 x2. The issue's own example is reported once by RH8305 today (guarded). In a nested primary-constructor record (`public sealed record Inner(int A);` inside a class), RH7302 and RH8308 double the same way.

Caller: the analyzers themselves on any user compilation; the duplicates show in the analyzer test verifier and any `CompilationWithAnalyzers` consumer. **The command-line build collapses them**: building a scratch project with the SDK compiler and the analyzer DLL, the SARIF error log lists RH7306 at `Common.cs(15,5)` once. IDE behavior was not verified. Severity is therefore low (a duplicated entry, no wrong code change), but every failing member contradicts an unchanged one-diagnostic requirement: RH8204-002, RH8304-003, RH8308-003, RH8309-003, RH7302-003, RH7310-003, SHARED-REGDESC-002 (RH7306, RH7307). Nothing visible is in question.

Code fixes are not harmed: `scripts/apply-fix.sh` for RH8308, RH8304, RH7302, RH7310 over the fixtures above: every fixture `fixed (1 action(s), 1 iteration(s))` under LF and CRLF.

### Mechanism and candidates

Mechanism: an analyzer whose syntax-node action is registered on a structured-trivia kind reports once per callback, and the driver calls it twice for structured trivia attached to the header or closing-brace tokens of a type declaration that has a parameter list (outside its attribute lists and parameter list).

Dispatch: `AnalysisContext.RegisterSyntaxNodeAction` with a trivia kind. Every registration on such a kind in `src/Reihitsu.Analyzer` (rg over all `Register*Action` calls, including base classes):

| Analyzer | Kinds | Today |
|---|---|---|
| RH8204 | `XmlElement`, `XmlEmptyElement` | fails |
| RH8304 | `SingleLineDocumentationCommentTrivia` | fails |
| RH8305 | `SingleLineDocumentationCommentTrivia` | once, guarded by `IsPrimaryConstructorRun` |
| RH8307 | `XmlElement` | once, guarded by `IsPrimaryConstructorRun` |
| RH8308 | `SingleLineDocumentationCommentTrivia` | fails |
| RH8309 | `SingleLineDocumentationCommentTrivia` | fails |
| RH7302 | `RegionDirectiveTrivia` | fails |
| RH7310 | `RegionDirectiveTrivia` | fails |
| RH7306, RH7307 (`RegionDescriptionAnalyzerBase`) | `RegionDirectiveTrivia`, `EndRegionDirectiveTrivia` | fail |

Not candidates (handed once): every documentation analyzer registered on declaration kinds (RH8001–RH8034, RH8101–RH8111, RH8201–RH8203, RH8306), and rules that use syntax-tree actions (RH8301, RH8302, RH8303, RH7301, RH5031, RH5032 — observed once). The issue's "RH8308, RH8309, RH8204, and others" is right for those three; the rest of "every documentation analyzer" is not affected.

### Breaking inputs

Failure classes: FC-016 (two analyzers were fixed, eight siblings keep the hazard), FC-015 (the guard is a private call in two analyzers), FC-005 (the issue covers documentation comments; directives are the same trivia), FC-020, FC-024, FC-033.

Class matrix — axes: (a) the trivia kind: single-line doc comment, delimited `/** */` doc comment (only RH8204 and RH8307 see it), `#region`, `#endregion`; (b) where it is attached: in front of the first modifier/keyword, between an attribute list and the first modifier, in front of `;` of a body-less declaration, in front of the closing `}`, in front of an attribute list, inside the parameter list, inside the base argument list; (c) the type: `record`, `record struct`, `class`, `struct`, nested or top-level, with an empty `()` parameter list, `partial` with the parameter list on one part only.

- (b) "in front of the first token", "after an attribute list", "in front of `;`", "in front of `}`": visited twice → fail for every candidate whose kind is there.
- (b) "in front of an attribute list": visited once → holds today.
- (b) "inside the parameter list", "inside the base argument list": visited only in the constructor run → reported once today by the unguarded analyzers, and **not reported at all** by the guarded ones. Observed: `public record R(` / `/// <summary>A.</summary> trailing` / `int A);` — RH8305 does not report the summary, while the same comment in an ordinary method's parameter list is reported by RH8305 (RH8308 reports both). So the existing `IsPrimaryConstructorRun` guard has a failing member of its own; it is rare (a doc comment there draws CS1587). A `#region` inside the parameter list is reported once by RH7302 today; any change must keep it.
- (c) every type kind behaves the same; the other part of a partial type without the parameter list is visited once.

Inputs the test implementer must cover, per candidate: the doc comment / directive in front of a positional record and of a class with a primary constructor (the issue's two cases), the `#endregion`/`#region` in front of the closing `}` of a primary-constructor class (the common shape), one case after an attribute list, and — for the guard's edge — one case inside the parameter list that must stay reported once. Tests must use the analyzer verifier, not a build: the command-line build hides the duplicate (FC-033).

### Surfaces

| Surface | |
|---|---|
| Analyzers RH8204, RH8304, RH8308, RH8309 | must change |
| Analyzers RH7302, RH7306, RH7307, RH7310 | must change (scope growth beyond the issue's "documentation analyzers"; same mechanism) |
| RH8305, RH8307 | must stay once for the reported case; their guard drops ctor-only nodes (above) |
| `DocumentationAnalysisUtilities.IsPrimaryConstructorRun`, `DocumentationModeAnalysisContextExtensions` | may change (internal) |
| Code fixes, Fix All | not involved (converge today) |
| Formatter, `Reihitsu.Core`, CLI | not involved |
| Rule pages | not involved |
| Specs | not involved: every member breaks an unchanged requirement |
| Tests | must change: reproductions per candidate |

### Refuse option

Leaving the duplicates: costs nothing but leaves eight analyzers in breach of unchanged requirements; no fix is withheld, so the refusal options of `AGENTS.md`, *Code-fix coverage*, do not apply. Declaring a shape unsupported is not an option: the defect is in analyzer dispatch, the formatter is not involved. Simpler option: none smaller than a guard on the trivia-kind registrations; extending the existing guard as written to all of them would also drop the ctor-only members above.

### Output questions

_None._

### Internal choices

1. Scope: include the four region rules (RH7302, RH7306, RH7307, RH7310) or file them. Recommend include: same mechanism, the `#endregion` before `}` of a primary-constructor class is common in a repository that requires regions (RH7305), each is a few lines, and each breaks an unchanged requirement.
2. Where the guard lives. The issue prescribes `RegisterSyntaxNodeActionWithDocumentationModeCheck`; the region rules do not use that helper, and the declaration-kind documentation rules that do need no guard. Internal; recommend whatever single place covers every trivia-kind registration, as long as it does not drop the nodes only the constructor run visits.

### Issue goals

- "Done when: for each documentation analyzer, a test with a positional record (and a class with a primary constructor)" — drop for the ~50 analyzers registered on declaration kinds: whether an analyzer is affected is decided by the registered node kind, not per analyzer, and the probe shows none of them is handed a node twice. Tests for the candidates above suffice. No visible change.
- "Each analyzer that does reports once, through one shared guard" — keep the outcome; the guard's location is a prescribed solution (internal choice 2).
- "The other documentation analyzers were not checked" / scope "documentation analyzers" — widen to the region rules (internal choice 1). Brings output to unchanged requirements; nothing visible in question.

### Architecture findings

The decision "this callback repeats one already made for the same node" has one statement per analyzer: private calls of `IsPrimaryConstructorRun` in RH8305 (`RH8305...Analyzer.cs:49`) and RH8307 (`:155`), absent from eight analyzers with the same registration shape (FC-015/FC-016). The guard as written also decides more than it should: it drops the nodes only the constructor run sees (RH8305, parameter-list example above). See also the cross-issue finding under *Whole run*.

### Follow-up candidates

_None_ beyond the scope question above (if the lead does not take the region rules in, they are a follow-up of the same mechanism).

## thoenissen/Reihitsu#1742

### Premise

Real, as a driver fact, and wider than "rules on constructors". Node probe on

```csharp
public partial class C
{
    /// <summary>
    /// Defining.
    /// </summary>
    #region DefRegion
    #endregion
    [System.Obsolete]
    protected internal partial C(int a);

    /// <summary>
    /// Implementing.
    /// </summary>
    protected internal partial C(int a) { }
}
```

hands **no** node of the defining part to a syntax-node action — not the `ConstructorDeclaration`, its attribute list, parameter list, parameters, types, nor the structured trivia in its leading trivia (the doc comment and the `#region`/`#endregion` in front of it). The implementing part and both parts of a partial method are visited. Same with Roslyn 5.9.0 and 5.12.0 (SDK compiler). With the implementing part in a generated file (`// <auto-generated/>`), the defining part — the hand-written one — gets no syntax-node diagnostic at all.

Caller: the analyzers on any user compilation; partial constructors are C# 14 and valid. Reported case reproduces.

All analyzers over mirrored parts (each part carries the same violations; partial-method parts as control, `pcall` fixture):

| Reported on the partial-method defining part and the constructor implementing part, missing on the constructor defining part | Reported on the constructor defining part too |
|---|---|
| RH7302 (region in front of it), RH8304, RH8305, RH8307, RH8308, RH8309, RH8204, RH8102, RH8101, RH5512 (attribute list placement), RH6013, RH5109, RH3103, RH4122 | RH7105, RH7106 (`DeclarationModifierUtilities.GetDeclarationsJudgedWith`), RH2004 (reports the defining part when neither part has an accessibility, SHARED-DIAG-012), and tree/token-level rules (RH5031, RH5032, RH5602, RH6002, RH6006, RH8302) |

Documentation-presence rules: `public partial V(int value);` without a doc comment, implementing part documented → RH8017 not reported; the same shape on a partial method (`public partial void M(int value);`) → RH8020 reported on the defining part. Both parts undocumented, private: RH8018 only on the implementing part, RH8021 on both parts of a method.

Severity low, as the issue says: missing diagnostics on a rare, new construct; no code is changed wrongly.

### Mechanism and candidates

Mechanism: the analyzer driver never hands the defining part of a partial constructor, or any node or structured trivia inside or in front of it, to a syntax-node action, so every rule that judges such a node through a syntax-node action skips it, unless it reaches the part from another callback.

Dispatch: syntax-node registrations whose kinds occur in a body-less constructor declaration or its leading trivia (rg over `Register*Action` calls and base classes): `ConstructorDeclaration` (RH2004, RH5108, RH7105, RH7106, RH8017/RH8018 via `SplitElementDocumentationAnalyzerBase`, RH8030 and RH8101–RH8104 via `DocumentationAnalysisUtilities.DocumentableDeclarationKinds`/`ParameterOwnerKinds`), `ParameterList` (RH5105, RH5106, RH5107, RH5109), `Parameter` (RH4105, RH4122, RH5111), `AttributeList`/`Attribute` (RH6013, RH6014, RH8501, `TargetAttributePlacementAnalyzerBase`, `TargetAttributeListShapeAnalyzerBase`), type syntax in parameter types (RH1001, RH3103, RH6011, RH6012, RH6015), default values (RH6023, RH6024 and expression rules), doc-comment trivia (RH8204, RH8304, RH8305, RH8307, RH8308, RH8309), region trivia (RH7302, RH7306, RH7307, RH7310). Roughly 35 analyzers; the table above holds the members confirmed by running.

### Breaking inputs

Failure classes: FC-016 (RH7105/RH7106 fixed, the siblings keep the hazard), FC-015 (three ways to reach the defining part, see *Architecture findings*), FC-024, FC-020 (a `#region`/`#endregion` in front of the defining part), FC-033 (the generated-implementing-part case needs two documents, one generated).

Class matrix — axes: (a) where the node is: the declaration itself, attribute list, parameter list, parameter, parameter type, default value, doc comment in front, directive in front; (b) where the implementing part is: same type declaration, other part of the type in the same file, other file, generated file; (c) the containing type: class, struct, record, record struct; (d) the rule's partial policy: judges each part as written (SHARED-DIAG-004, second and fourth paragraphs: comment text, names, parameters, layout), judges the member once (SHARED-DIAG-004 first paragraph, SHARED-DIAG-012), or its spec does not cover partial elements (see *Output questions*). Every cell of (a)×(b)×(c) is unvisited by the driver; which cells fail depends on (d): the per-part rules fail in every cell; the once-per-member rules hold where they already select the part from another callback (RH2004).

A change that makes a rule reach the defining part from another callback must not double-report when a host's driver does hand it the part (`GetDeclarationsJudgedWith` already never judges the defining part on its own for that reason), and must keep once-per-member rules at one diagnostic (SHARED-DIAG-012).

### Surfaces

| Surface | |
|---|---|
| Per-part syntax-node analyzers listed above | must change, or the class is refused (below) |
| RH7105, RH7106, RH2004 | must stay |
| Documentation-presence rules (RH8017, RH8018, RH8030, RH8101–RH8105, RH8108) | output question (specs exclude partial elements) |
| `Reihitsu.Core` (`DeclarationModifierUtilities.GetDeclarationsJudgedWith`) | may change (internal) |
| Code fixes | involved only in that a newly reported defining part gets the rule's existing fix; SHARED-DIAG-004 keeps a fix to its part |
| Formatter, CLI, rule pages | not involved |

### Refuse option

Refuse: leave the defining part of a partial constructor unjudged by syntax-node rules and record the driver limitation in `sdd/specs/shared/diagnostics.md`. Zero production lines. But the per-part rules' specs require the defining part to be judged (e.g. RH4122 reports every parameter, SHARED-DIAG-004's fourth paragraph judges each part's parameters as written), so the refusal changes a spec to match today's output — a visible decision, hence an output question below. The class is rare: C# 14, mainly the source-generator pattern.

Requested change: about 35 analyzers. Either one change per analyzer, in the shape of `GetDeclarationsJudgedWith` (FC-015 times 35), or one new shared mechanism that re-dispatches the defining part's nodes from the containing type — a new mechanism and likely more than 100 production lines or a new type, which is escalation 9.

Declaring a shape unsupported: not an option; the formatter is not involved and this is analyzer dispatch.

Narrower option: fix only the rules whose subject is a constructor's own text and which users meet first — doc-comment rules and naming/layout of parameters — and file the rest. Still per-rule copies.

### Output questions

1 · Partial constructors — diagnostics on the defining part (all per-part rules; RH4122 · Parameter names should not be a single letter as the example)

Input:
```csharp
public partial class C
{
    public partial C(int a);

    public partial C(int a)
    {
    }
}
```

Before (`main`): RH4122 on `a` of the implementing part only.

After, A:
```csharp
public partial class C
{
    public partial C(int {|RH4122:a|});

    public partial C(int {|RH4122:a|})
    {
    }
}
```

After, B: unchanged — implementing part only; the specs state that the defining part of a partial constructor is not analyzed by these rules.

Why: no recommendation between A and B on output alone — A is what the specs already demand and what partial methods get; B costs nothing. The lead should ask it together with escalation 9 (A's cost), recommending **B** if A needs a new shared mechanism above the escalation-9 threshold, **A** otherwise. I would pick B: the input class is rare and A spreads one workaround over ~35 analyzers for a driver behavior.

2 · RH8017 · Non-private constructors must be documented — undocumented defining part of a partial constructor

Input:
```csharp
public partial class V
{
    public partial V(int value);

    /// <summary>
    /// Initializes.
    /// </summary>
    /// <param name="value">Value</param>
    public partial V(int value)
    {
    }
}
```

Before (`main`): not reported (RH8020 does report the same shape on a partial method's defining part).

After, A: RH8017 on `V` of the defining part `public partial {|RH8017:V|}(int value);`.

After, B (recommended): not reported.

Why B: SHARED-DOC-001 and SHARED-DOC-005 leave partial elements outside the documentation-presence rules, so A decides new behavior that belongs in a run about partial-element documentation, not in a driver workaround. (Same question for RH8018, RH8030, RH8101–RH8105, RH8108.)

### Internal choices

Whether the existing `GetDeclarationsJudgedWith` (Core) or a new place carries the reach to the defining part, if A is chosen — internal; recommend one place for all rules (*Architecture findings*).

### Issue goals

- "Expected output: both parts are judged the same way partial methods ... are, each part reported on its own span" — change: a rule that judges the member once (SHARED-DIAG-004 first paragraph) reports once on the part SHARED-DIAG-012 selects, not on each part; RH2004 already does so for partial constructors. No visible change from this correction.
- "The set is every such rule [that registers `ConstructorDeclaration` or looks at a constructor's parameters or modifiers]" — widen: the class also includes rules on attribute lists, parameter types, default values, and the doc comments and region directives in front of the defining part (RH7302 confirmed). Visible output of the widening is question 1.
- "A shared helper would avoid one copy per rule" — prescribed solution; internal.

### Architecture findings

Reaching the defining part of a partial constructor is done in three ways: `DeclarationModifierUtilities.GetDeclarationsJudgedWith` (`src/Reihitsu.Core/DeclarationModifierUtilities.cs:137`, RH7105/RH7106), the containing-type selection of `PartialMemberUtilities` (RH2004), and not at all in every other syntax-node rule (FC-015/FC-016). Any fix of question 1 A adds members to this set.

### Follow-up candidates

- If question 1 is answered B or narrowed: the per-part rules left unfixed, as one `agent-filed` follow-up naming the driver behavior and the confirmed members.
- RH8102 on the implementing part `public partial V()` documented with `<param name="value">` (no such parameter) reports nothing, while RH8102 reports a mismatched `<param>` on partial-method parts. Not investigated further; specs exclude partial parts for RH8101 (`RH8101.md:28`). Out of scope.

## Whole run

Both issues are one structural fact seen from two sides: the analyzer driver's syntax-node dispatch for constructor-shaped declarations differs from the syntax tree — the primary constructor makes the header and closing-brace trivia of its type arrive twice, the partial constructor's defining part never arrives. Today each consumer compensates in its own analyzer (RH8305, RH8307; RH7105, RH7106; RH2004). An architecture finding for the lead: one owner for "which node a syntax-node action judges, once" would serve both issues; whether that is in scope is the lead's decision under escalation 9.

No repository file other than this handover was changed; probe fixtures and builds were in the scratchpad and are deleted.
