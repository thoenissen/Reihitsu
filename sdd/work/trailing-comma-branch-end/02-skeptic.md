Role: skeptic
Commit: 9e96bfa91a2e4e015c9df56822889ba711fe2e58
Verdict: output question

The prompt carried no intended fix or hoped-for verdict beyond the issue's own A/B question. The spec author's notes for the implementer (where the guard goes) are an intended fix; I did not use them.

## Premise

Real, and reached by every caller.

- Today's behavior, run on `main` (no symbols defined): `scripts/apply-fix.sh RH5409` over the two fixtures below gives `no-fix-offered` under LF and CRLF. `scripts/format.sh` over `enum Level` (input below) keeps `High,`; the only change it makes is removing the final newline.

```csharp
enum E
{
    A,
#if !X
    B,
#endif
}
```

```csharp
enum F
{
    A,
#if X
    B,
#else
    C,
#endif
}
```

- What decides it: `SyntaxTriviaUtilities.ContainsConditionalCompilationBoundary` (`src/Reihitsu.Core/SyntaxTriviaUtilities.cs:466`). It returns true for any `#if`/`#elif`/`#else`/`#endif` or disabled text in the span from the comma's end to the next token. It has three callers, all about trailing commas: `TrailingCommaCodeFixHelper.CommaPrecedesConditionalCompilationBoundary` (code fix and Fix All), `EnumTrailingCommaRemovalTransform:55`, and `InitializerTrailingCommaRemovalTransform:58`. The two transforms are registered unconditionally in `StructuralTransformPhase.CreateRewriters` (lines 68–69), so they run in whole-document formatting and in every node-level entry point.
- Callers that reach the case: the RH5409/RH5410/RH5411 code fixes and their Fix All; whole-document formatting (the CLI), but only for a comma in a branch that is active with no symbols defined (see *Output questions*); and every code fix that formats, through the formatter, a node that holds such a list (settled below).
- Safety argument, checked by reasoning and not taken from the issue. Suppose the gap holds no `#if`. Then every `#elif`/`#else`/`#endif` in it belongs to a group that is already open at the comma. A build that compiles the comma takes, in each of those groups, the same branch the analyzed build takes. Because the gap opens no group, that build reads the same text from the comma to the brace as the analyzed build does: the same trivia, with the same later branches skipped. So no build that compiles the comma compiles an element behind it. The argument holds, and it does not depend on the excluded text. The one exception is SHARED-FIX-009's decided gap, a string or comment opened by an excluded branch (see *Internal choices* 2).
- The issue's own example does not change on the CLI. `reihitsu-format` defines no symbols, so `B,` there is disabled text. The spec author already established this, and I agree.

## Mechanism and candidates

_N/A._ This is a behavior change, not a bug. The decision is the trailing-comma gap check above. The candidate lists are the three syntax kinds those three callers handle: `EnumDeclarationSyntax` (RH5409, FMT-STRUCT-019), and `InitializerExpressionSyntax` of kind `ArrayInitializerExpression` (RH5410) or `CollectionInitializerExpression` (RH5411, FMT-STRUCT-020). Object initializers, collection expressions, and other comma lists are not trailing-comma rules and are not involved.

### Which other code fixes change their result (the claim the spec author left unprobed)

Settled with `scripts/apply-fix.sh` on `main`. Each rule got a "cond" fixture, with `#if X ⏎ 2, ⏎ #else ⏎ 3, ⏎ #endif` inside an initializer or enum in the node the fix formats, and a twin that proves the trailing-comma transform runs on that node. The twin is either `3, ⏎ #region R ⏎ #endregion` or a plain `3,` with no directive; both are fixable today.

| Fix | cond fixture | twin | Changes under A |
|---|---|---|---|
| RH5303 | fixed, `3,` kept | `#region` twin: comma removed | yes |
| RH5301 (`new D { Value = 1, Items = { 1, …3, #endif } }`) | fixed, `3,` kept | `#region` twin: comma removed | yes |
| RH5304 (same input) | fixed, `3,` kept | `#region` twin: comma removed | yes |
| RH3202 (`int[] F() => new[] { 1, … };`) | fixed, `3,` kept | plain twin: `3` | yes |
| RH3208 (same, as a local function) | fixed, `3,` kept | plain twin: `3` | yes |
| RH5110 (`class C<T> where T : class { enum Level { Low, #if DEBUG Trace, #else High, #endif } }`) | fixed, `High,` kept | `#region` twin: `High` (comma removed) | yes, for enums too; RH5110-spec line 85 makes the whole declaration the formatted node |
| RH7101 (`private int _a, _b;` next to that enum) | fixed, enum untouched | `#region` twin: enum untouched | no: the formatted node is the field only |
| RH5111, RH5101 | no-fix-offered | no-fix-offered | no: withheld for both shapes |
| RH5403 | no diagnostic | — | no: a block that holds a directive cannot be on one line |

Example of RH5303 on `main` (`cond.cs`, LF; CRLF is identical):

```csharp
var list = new List<int> { 1,
#if X
    2,
#else
    3,
#endif
};
```
→
```csharp
var list = new List<int>
           {
               1,
#if X
    2,
#else
               3,
#endif
           };
```

The rule behind the class: under A, a code fix changes its result when it formats a node through the formatter, that node holds an enum or an array/collection initializer whose last comma has only `#elif`/`#else`/`#endif` in its gap, and the fix is offered for that node. The six fixes marked "yes" are confirmed members. Others that delegate to the formatter, such as RH3203, RH3004, RH5205, RH5302, and RH5308, are probably members but were not probed. The change doc's sentence "RH5301 and RH5304 … change in the same way" is now confirmed but understates the reach; see *Internal choices* 3.

## Breaking inputs

Failure classes that reach the changed code:

- FC-001 (the guard's span): the gap must stay comma end → closing-brace start. Directives in the comma's leading trivia are outside it; see the matrix row "leading-trivia group".
- FC-002 / FC-005 / FC-020 (directive kinds): every directive kind in the gap.
- FC-003 (both sides): `#endif` alone versus `#endif` followed by `#if`.
- FC-008 (Fix All): two fixable commas in one document, and a fixable comma next to a withheld one.
- FC-010 (CRLF): change-doc examples under LF and CRLF.
- FC-013 (code fix and formatter decide alike): every row must agree between the fix and the formatter.
- FC-031 (stale comments): the summary of `ContainsConditionalCompilationBoundary` ("deliberately textual … case-by-case reasoning … let the unsafe removal through"), the summary of `TrailingCommaCodeFixHelper.CommaPrecedesConditionalCompilationBoundary`, and the summaries of the existing tests that call the current behavior intended. The history behind that Core comment cannot be checked: the clone is shallow, and `dc6d942` is its oldest commit. A's check still evaluates no branch and no condition, so the comment's concern does not apply to it.

Class matrix. Axes: directive kinds in the gap × list kind × surface × build (symbol defined or not) × line ending.

| Gap content | Fix / formatter under A | Existing test pinning it |
|---|---|---|
| `#endif` only (active branch, symbol defined) | offered / removed | flips: `RH5409…AnalyzerTests.VerifySpecificationExampleReportsCommaInFrontOfEndIfWhereTheSymbolIsDefined`, the `withSymbol` halves of `VerifyEnumWithConditionalBlockAfterFinalMemberIsReportedWithoutFix` (RH5409), `VerifyArrayWithConditionalBlockAfterFinalItemIsReportedWithoutFix` (RH5410), and `VerifyCollectionWithConditionalBlockAfterFinalItemIsReportedWithoutFix` (RH5411) |
| `#endif` only (`#if !NEVER` / `#if true`) | offered / removed | flips: `RH5409…VerifyCommaInFrontOfEndIfOfAnActiveBlockIsReportedWithoutFix`; `StructuralTransformPhaseIntegrationTests.PreservesTrailingCommaWhenActiveIfTrueBranchFollowsFinalElement` and `PreservesTrailingCommaInEnumWhenActiveIfTrueBranchFollowsFinalMember` |
| `#else` + disabled + `#endif` | offered / removed | flips: `StructuralTransformPhaseIntegrationTests.PreservesTrailingCommaWhenElseBranchFollowsFinalElement` |
| `#elif` + disabled + `#endif` | offered / removed | none found |
| nested: `#endif` `#endif` (comma inside two active groups) | offered / removed | none found |
| `#endif` + `#if Y` … `#endif` | withheld / kept | none found (spec example of SHARED-TRAIL-002) |
| `#else` + `#if Y` in excluded code | withheld / kept | none found (spec example; see *Internal choices* 1) |
| `#if` directly (comma in front of `#if`) | withheld / kept | many, e.g. `VerifyFixAllKeepsOnlyTheCommaInFrontOfAConditionalBlock` (all three rules), `PreservesTrailingCommaBefore…` — unchanged |
| `#endif` + `#region`/`#pragma` | offered / removed | none found |
| `#region`/`#pragma` only | offered / removed | `VerifyCommaInFrontOfPragmaDirectivesIsReportedAndFixed`, `RemovesTrailingCommaWhenRegionDirectiveFollowsFinalElement` — unchanged |
| leading-trivia group: `B ⏎ #if !X ⏎ , ⏎ #endif` | fix offered, removes the comma-only line; formatter refuses (FMT-UNSUP-009) | none found for the fix |
| comment on the comma: `High, // c ⏎ #endif` | offered / removed, comment stays behind `High` | none found |
| nested initializer: inner comma in front of `#endif` | inner removed, outer unaffected | none found (`PreservesInnerTrailingCommaInNestedArrayInitializerWhileRemovingOuter` covers the `#if` side, unchanged) |
| code fixes that format the list (RH5303, RH5301, RH5304, RH3202, RH3208, RH5110) | `3` instead of `3,` | none found; one fixture of RH5303 (the change doc's) is enough to pin the reach |

Every "after" output in the change doc is stable under formatting. On `main` I ran `scripts/format.sh` over the comma-free after states: `enum Level`, the RH5410 array, the RH5411 list, the `#elif`/`#else` enums `E`/`F`, RH5303's after, and `High // the last ⏎ #endif`. The only change was the final newline, so later phases treat the comma-free shape as stable and add no blank line in front of the directive.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH5409/RH5410/RH5411 analyzers (diagnostic, location, message) | must stay |
| RH5409/RH5410/RH5411 code fix and Fix All | must change |
| `EnumTrailingCommaRemovalTransform`, `InitializerTrailingCommaRemovalTransform` | must change |
| `Reihitsu.Core` (`ContainsConditionalCompilationBoundary`; it has no other callers) | involved: its three callers make the one decision; it must stay one statement (FC-015) |
| Other formatter-delegating code fixes (RH3202, RH3208, RH5110, RH5301, RH5303, RH5304, …) | output changes, code must stay |
| RH7101, RH5111, RH5101, RH5403 | not involved (probed) |
| Precheck / FMT-UNSUP | not involved; the removal produces no declared shape |
| Node-level entry points | not involved; no entry point predicts anything new |
| Rule pages RH5409–RH5411, `general-notes.md` | must stay |
| Tests | the flips listed above are visible-output expectations and need the maintainer's answer |

## Refuse option

Refusing is option B, which is today's behavior: the fix is withheld for the whole class and the formatter keeps the comma. It costs nothing, and the class is rare (the issue says so, and it takes a conditional group at the end of an enum or initializer). The requested change costs replacing one predicate at three call sites. It adds no type and no new mechanism, and it stays a kind check.

`AGENTS.md`, *Code-fix coverage*, bears on the choice directly: "A fix that refuses inputs it could rewrite safely is still worth narrowing, as long as the narrowing stays a cheap check." A fits that sentence. The argument above shows the class is safe, and the check reads only directive kinds.

Declaring the shape unsupported: a conditional directive or excluded code between the last comma and the closing brace of an enum member list, an array initializer, or a collection initializer. It would delete only the two transforms' keep-the-comma checks, and it would refuse every file that has an `#if` block at the end of such a list, which is common code. The maintainer already rejected this exact shape in `sdd/changes/2026-11/report-across-conditional-directives.md` (*Alternatives considered*: "Declare a conditional directive between the last comma and the closing brace unsupported … rejected by the maintainer"; *Visible output changes* item 6). Under A it would delete even less. I do not recommend re-asking. The lead decides whether that answer covers it.

No simpler option.

## Output questions

1. **RH5409/RH5410/RH5411 · trailing comma at the end of a conditional branch.** This is the issue's A/B question. It should be shown with an input the CLI actually changes, and with the full reach of A:

Input (the CLI defines no symbol):
```csharp
enum Level
{
    Low,
#if DEBUG
    Trace,
#else
    High,
#endif
}
```
Before (`main`): `High,` is reported with no fix offered; `reihitsu-format` keeps it (observed).

After, A (recommended):
```csharp
enum Level
{
    Low,
#if DEBUG
    Trace,
#else
    High
#endif
}
```
The fix and `reihitsu-format` both remove the comma. In a project that defines `DEBUG`, the fix also removes the comma behind `Trace`. The same comma also disappears from the result of every code fix that formats the enclosing node through the formatter, confirmed for RH3202, RH3208, RH5110, RH5301, RH5303, and RH5304 (RH5303 example in the change doc: `3` instead of `3,`).

After, B: unchanged.

Why A: the removal is safe in every build, and the repository's own rule says a guard that refuses safe input is worth narrowing when the narrowing stays a cheap check.

## Internal choices

1. **The stated reason for withholding on an `#if` in excluded code is inaccurate.** The change doc (*Decisions*, *Alternatives considered*) and SHARED-TRAIL-004's Why say that telling such an `#if` apart "would need the directives paired". It would not need pairing. As the argument under *Premise* shows, any excluded text in the gap lies in a later branch of a group open at the comma, so ignoring it is safe. Roslyn's `DirectiveTriviaSyntax.IsActive` is a flag the tree already provides. I did not run whether it reads `false` for an `#if` inside a skipped branch, so I make no claim about its value. Options:
   - (a) Keep the spec's behavior (withhold) and change only the reason, to "rare; withholding is the default for a rare class". No visible change beyond A, and nothing more in question.
   - (b) Withhold only for an `#if` the analyzed build reads. This offers the fix for `B, ⏎ #else ⏎ #if Y ⏎ C, ⏎ #endif ⏎ #endif`, which is one more visible change for a rare shape, and it depends on a Roslyn result that has not been run.

   Recommend (a): it keeps A's question as the only one, and the shape is rare.
2. **SHARED-TRAIL-004's Why overclaims** with "a build that does not compile the comma does not see the change". SHARED-FIX-009 itself states that a build which compiles an excluded branch opening a string or comment reads the code behind that branch as content of the string or comment. Such a build would see the comma vanish from that content. This is SHARED-FIX-009's decided gap, and it applies already today to commas whose gap holds no directive. Recommend dropping the clause or pointing it at SHARED-FIX-009's decided gap. Spec wording only, nothing visible.
3. **The change doc's "Code fixes that format an initializer" section** should state the class (every code fix that formats a node holding such a list through the formatter) and name the confirmed members: RH3202, RH3208, RH5110 (enums too), RH5301, RH5303, RH5304. It should stop naming RH5301/RH5304 as a single unprobed sentence. Change-doc text only; the reach goes into the output question.
4. The SHARED-TRAIL-002/004 split and the unchanged FMT-STRUCT-020 text are fine. No recommendation to change them.

## Issue goals

- "Before (`main` plus the change that made this reported)" is stale: that change has landed (`report-across-conditional-directives`), so "before" is plain `main`. No visible effect.
- "the formatter removes it too", said of the issue's `#if DEBUG ⏎ B, ⏎ #endif` example, does not hold for the CLI, which defines no symbol. It holds only for formatting with `DEBUG` defined, which code fixes do through the project's parse options. The question should be asked with an input the CLI changes, as in *Output questions*. This changes how the question is shown, not what it asks.
- "Done when … tests cover the directive kinds and Fix All" stays. It is the matrix above.
- No goal to drop.

## Architecture findings

_None._ The decision has one statement, the Core helper, and three callers that use it. It becomes an architecture finding only if the narrowed predicate is copied into one of the callers instead of staying shared (FC-015).

## Follow-up candidates

_None._ RH5110 formatting the whole declaration, enum bodies included, is what RH5110's spec requires (line 85), not FC-030.
