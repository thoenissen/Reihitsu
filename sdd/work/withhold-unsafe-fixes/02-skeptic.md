Role: skeptic
Commit: 743b10a
Verdict: #1136: proceed narrower; #1134: proceed; #1135: proceed (Q-S1 open); #1262: question for the maintainer (Q-S2, Q-S3) and architecture finding; #1332: proceed; #1336: proceed (wider member set); #1340: proceed

Inputs: the seven issues with their comments, `00-lead.md`, `01-spec-author.md`, the spec diff `2ea4e56..743b10a`, and the code owners named below. The prompt carried no intended fix or hoped-for verdict. Every observation below comes from `scripts/apply-fix.sh <ID> <dir>` on scratch fixtures (documentation mode `Parse`, the runner's default; symbols only where a fixture starts with `#define`), LF and CRLF alike unless stated; the fixtures were deleted afterwards.

## thoenissen/Reihitsu#1136

### Premise

Real as a spec gap: SHARED-FIX-003 on `2ea4e56` names neither relocation nor disabled text. The examples the spec author added fail today where they should (RH7501 `#pragma` pair; RH7004 `#if DEBUG` pair, both configurations) and pass where they should (RH7004 balanced pair followed by `s.Close();`).

**The brace-split sentence is wider than the maintainer's clarification.** The clarification (lead handover) counts "one directive moved, its partner left behind". The spec now also counts "it adds a brace between the two" with no further qualifier, so a balanced brace pair added wholly between the two directives of a pair counts as a split. Inputs offered today, correct today, and withheld under the literal text:

- RH7004, `#pragma warning disable CS0168` above the using declaration and `#pragma warning restore CS0168` after the last following statement: today the fix gives `using (…) { s.Flush(); }` with both pragmas outside the new block — governed code unchanged, no directive separated from its partner.
- RH3202, `#pragma warning disable`/`restore` around `int M() => 1;`: today the fix gives a block body between the two pragmas — fine.
- Every expression-body (RH3202–RH3208) and brace fix (RH5405–RH5407) inside a `#region`/`#endregion` pair — the common case — adds braces between a region pair.

The inputs that must stay withheld: RH7004 with `#pragma warning disable` before `s.Flush();` and `restore` after it (today: `disable` ends up inside the new block, `restore` behind it), and RH7004 #1336 A1 (`#if`/`#endif` around the last statement). In both, one brace of the new pair lies between the two directives and its partner brace outside them; in the over-covered inputs both new braces lie between them. This is a spec repair for the spec author, not a maintainer question: the maintainer's wording already decides it.

### Mechanism and candidates

_N/A_ (spec clarification; the code defects it names are under #1332 and #1336).

### Breaking inputs

FC-004, FC-020, FC-005. The brace-split condition's members, along two axes — directive pair kind (conditional, region, `#pragma` disable/restore, `#nullable` set/restore) × where the new or crossed brace pair lies (one brace between and its partner outside; both braces between; both outside) — must be covered as: one-between → withheld; both-between → not withheld by this sentence; both-outside → not withheld. Tests should include a both-between member (RH7004 pragma pair around the declaration and all following statements) as the counter-example.

Disabled text: "drop, edit, or relocate" — a covering test that passes today: RH7004 balanced `#if DEBUG … #endif` among the following statements without `DEBUG` (disabled text moves into the block, unchanged except indentation).

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `sdd/specs/shared/code-fixes.md` (SHARED-FIX-003) | must change (brace-split sentence, see Premise) |
| Fixes outside the run (RH3202–RH3208, RH5405–RH5407, RH7310, others) | must stay; not audited against the new condition 4 — see Follow-up candidates |
| Analyzers | not involved |

### Simpler option

_None._

### Questions for the maintainer

_None_ for this issue.

### Architecture findings

_None._

### Follow-up candidates

- The new condition 4 (comments and directives in place) and the brace-split sentence apply to every fix. Only the fixes of this run were checked. The issue says violating fixes are fixed separately; the lead may want an audit issue over the fixes that add braces or move code (RH3202–RH3208, RH5405–RH5407, the RH72xx using fixes, RH7310).

## thoenissen/Reihitsu#1134

### Premise

Real. `RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsCodeFixProvider.IsMoveSafe` checks only `base.IsMoveSafe` and `OrderingDeclarationUtilities.ChangesInitializerExecutionOrder`; no code in `src/` outside tests mentions `StructLayout` or `LayoutKind`. Observed (fix offered, `a` and `b` swapped): `struct`, `record struct`, `ref struct`, `class` with `[StructLayout(LayoutKind.Sequential)]`. Offered and correct: `struct` with `LayoutKind.Auto`, plain `class`, static fields of a `struct`.

Is the spec wrong instead? No: SHARED-FIX-003 bullet *Run time* already decides it. Note for the spec author: RH7109-005 still says the fix MUST move, and RH7109-006 lists only SHARED-ORDER refusals, so RH7109 read alone contradicts SHARED-FIX-003 for a default struct. RH7309-004 and RH7101-004 were given an "unless … withholds" in this run; RH7109 was not.

### Mechanism and candidates

The member-ordering fix moves the field without asking whether the type's instance-field order is its layout. Dispatch: `TypeMemberOrderingCodeFixProviderBase` subclasses — RH7102 (moves `const` fields: no storage, not involved), RH7103 (moves static members only: the relative order of instance fields is kept, not involved), RH7109 (moves instance or static readonly fields: involved). Type kinds that reach `TypeDeclarationSyntax`: class, struct, record, record struct, interface.

### Breaking inputs

FC-002 (the qualifier "whose field order sets its layout" must become the predicate), FC-024, FC-016. Candidates for the test implementer:

- withheld: default `struct` (issue), `record struct`, `ref struct`, `class` and `record` with `[StructLayout(LayoutKind.Sequential)]`, the attribute fully qualified (`System.Runtime.InteropServices.StructLayout`) or with named arguments (`Pack = 1`), a struct nested in a class, one part of a `partial struct`;
- offered: `struct` with `LayoutKind.Auto` and with `LayoutKind.Explicit`, a class without the attribute, static readonly fields of a struct.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH7109 code fix | must change |
| RH7109 analyzer | must stay (diagnostic still reported) |
| Layout predicate | new decision, also needed by #1135 (RH7309-006 bullet 2): two consumers, one decision — see the note under #1135 |
| Fix All | must stay (BatchFixer per diagnostic) |
| `sdd/specs/shared/ordering-move-safety.md` | not involved (the issue asks for the guard there; SHARED-FIX-003 already states it, and restating it would put one rule in two places) |
| Rule page | not involved |

### Simpler option

_None._

### Questions for the maintainer

_None._

### Architecture findings

_None._

### Follow-up candidates

- Pre-existing, FC-030: the RH7109 fix (all `TypeMemberOrderingCodeFixProviderBase` fixes) formats the whole type — `private static int Next() => 1;` elsewhere in the type becomes a block body. Already tracked as thoenissen/Reihitsu#969; this run neither depends on it nor exposes it.

## thoenissen/Reihitsu#1135

### Premise

Real. `RH7309RegionsShouldFollowCategoryOrderCodeFixProvider.CanReorderSafely` checks only unbalanced conditional and position-sensitive directives. Observed (fix offered): the issue's input (`Fields` moved in front, `Y` becomes 6); a static initializer read from a member between blocks (`_y = _x + 1` in `IDisposable`, `_x` between blocks, `Fields` moved in front of both); struct `_handle`/`_a` swapped; and, under `#if DEBUG` without `DEBUG`, `Y = X + 1` in disabled text of the moved-across block (DEBUG build changes `Y`).

### Mechanism and candidates

The fix permutes whole blocks by category and checks only directives, not the order of the members the permutation changes. Candidates: every pair of same-context members with a runtime initializer (`OrderingDeclarationUtilities.HasInitializer`: non-const field, event field, property with initializer) whose relative order the permutation changes, each member in a block or in the code between blocks; every pair of instance fields of a layout type.

### Breaking inputs

FC-016, FC-020, FC-024. Candidates:

- withheld: the issue's input; the member-between-blocks input; instance initializers in two blocks; an event field and a property initializer each in a block; the struct input; an initializer in disabled text of a balanced `#if` inside a block that changes order with an initializer of another block (decided by SHARED-FIX-003 *Build configurations*, not by RH7309-006 bullet 1, which counts members as SHARED-ORDER-007 does, i.e. syntax members only);
- offered: a static and an instance initializer that change order (other context); `const` fields; initialized members of a nested type inside a block (another type's context).
- open: Q-S1 — auto-properties and field-like events of a layout struct.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH7309 code fix | must change |
| RH7309 analyzer | must stay |
| `OrderingDeclarationUtilities` (Reihitsu.Core) | involved: the initializer predicate (`HasInitializer`, `IsStatic`) is private, and the public `ChangesInitializerExecutionOrder` answers a single forward move (moved member, target, crossed range), not a permutation of blocks. A private copy of the predicate in the RH7309 fix would be a second owner of SHARED-ORDER-007 (FC-015). |
| Layout predicate | shared with #1134; a copy per fix would be a second owner (FC-015) |
| Rule page | not involved |

### Simpler option

_None._

### Questions for the maintainer

**Q-S1 — RH7309, auto-properties and field-like events in a layout struct.** SHARED-FIX-003 counts "the instance fields" of a layout type. The compiler gives an auto-property and an instance field-like event a backing field, emitted in declaration order, so they are part of the struct's layout.

```csharp
using System;

public struct S : IDisposable
{
    #region IDisposable

    public int Handle { get; set; }

    public void Dispose()
    {
    }

    #endregion // IDisposable

    #region Fields

    private int _a;

    #endregion // Fields
}
```

Today the fix is offered and puts `_a` in front of `Handle`'s backing field.

- **A** — Only declared fields count: the fix stays offered for this input.
- **B** — Backing fields count too: auto-properties (including `field`-backed properties) and instance field-like events of a layout type withhold the fix like fields. Withholds more reorders in structs; no other fix in the run is affected (RH7109 moves only fields, and a move that crosses an auto-property already swaps two fields).

Recommendation: **B** — the backing field's offset changes exactly like a declared field's, and "prefer no fix" applies. A would change if the maintainer reads "layout" as source-visible layout only.

### Architecture findings

_None_ today. The initializer-order and layout predicates each need one owner shared by RH7103/RH7109/RH7309; see Surfaces.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1262

### Premise

Real for every instance, with two corrections to the issue's framing.

- **RH7303 straddling** (`#region Members` at type level, `#endregion` in `M`): reproduced — the fix removes both. `RH7303DoNotPlaceRegionsWithinElementsCodeFixProvider` registers whenever `TryFindMatchingDirective` finds a partner, whether or not the partner is reported.
- **RH7304 `/* c */`**: reproduced — `    /* c */ #region Fields` (`invalid-result`). The fix copies the text from the line start to the first token as "indentation". RH7304-001's claim requires that token to start its line, so the report itself is outside the claim; only the fix is decided (SHARED-FIX-003 condition 1).
- **RH7304 Outer/Inner** (#1176 Q2): reproduced — reported and fixed today. The pairing is the same as the compiler's here; what is missing is the check that a region directive between the pair lies in executable code (RH7304-005).
- **RH7306**: reproduced — `#region Implementation`/`#endregion // Implementation` → `#region`/`#endregion //`; `#endregion //Implementation` behind `#region IDisposable` → `no-progress`. Cause: the architecture finding below.
- **RH7309 comment above a moved block**, **three `///` shapes**: reproduced, all offered (mode `Parse`).
- **RH7309 "regions in a field initializer or between a class and its nested enum"**: reproduced, but the defect is in the analyzer's region selection, not only in the fix. `RegionDirectiveUtilities.GetTopLevelRegions` excludes only directives inside nested `TypeDeclarationSyntax` spans and inside `IsWithinElementBody`. An `EnumDeclarationSyntax` is not a `TypeDeclarationSyntax`, and an equals-value clause or arrow expression is none of the node kinds `IsWithinElementBody` walks to. Observed:
  - a region in `C` and a region in nested `enum E`: RH7309 reports `#region Fields` of `E` as out of order against `C`'s `IDisposable` — against SHARED-TYPEREGION-006 ("A rule MUST NOT compare a region with a region of another declaration") — and the fix moves `E`'s region out of the enum (`invalid-result`);
  - two regions wholly inside nested `enum E`: reported as regions of `C`; the fix swaps the enum members, so `A` and `B` change their values (run-time change);
  - two regions in a switch expression of an expression-bodied method: reported; the fix puts `_ => 20` in front of `1 => 10`, which does not compile (CS8510), although the runner reports `fixed` because the result parses;
  - two regions in an array initializer of a field: reported; the fix swaps the elements (run-time change; the issue says "does not parse", which does not hold for this shape).
  
  The enum members are an analyzer bug against SHARED-TYPEREGION-006. The expression members are not decided by any spec: SHARED-TYPEREGION-005 does not classify an equals-value clause or an arrow expression, so SHARED-TYPEREGION-006's precondition fails and RH7309-001 claims no report; only SHARED-FIX-003 decides the fix. See Q-S3.

### Mechanism and candidates

One mechanism per defect:

- RH7303: the fix removes the partner directive without asking whether a diagnostic of its own rule reports it. Candidates: `#region` type-level/`#endregion` in body (reported directive: `#endregion`), the mirror (`#region` in body, `#endregion` type-level; see Q-S2), both in one body (holds).
- RH7304 fix: the fix takes "indentation" as all text from the line start to the first token. Candidates for what stands there: whitespace only (holds), a `/* */` comment (fails), the last line of a multi-line `/* */` comment (`b */ int _a;`, same mechanism), a tab (holds).
- RH7304 analyzer: a pair is checked although a region directive between its two directives lies in executable code. Candidates: directive inside a method, accessor, lambda, local function, statement, between the pair.
- RH7306: see Architecture findings.
- RH7309 comments: the fix moves block text and leaves the text between blocks in place, so whatever stands directly above a `#region` line or behind the last code of a block stays at its old position.
- RH7309 regions: see above; dispatch is `GetTopLevelRegions` with consumers RH7103 (analyzer and fix), RH7110, RH7308, RH7309 (analyzer and fix), RH7311, `TypesOrganizedWithRegionsAnalyzerBase`, `MembersShouldBeGroupedByRegionsAnalyzerBase`, `NonOverrideMembersShouldNotBePlacedInOverrideRegionsAnalyzerBase`.

### Breaking inputs

FC-004, FC-016, FC-019–FC-021, FC-027, FC-009 (RH7306 `no-progress`). Candidates beyond the spec author's examples:

- RH7304: `/* c */ int _a;` and the multi-line `/* … */` variant (withheld); `// c` on its own line above the first token, `///` above it, an attribute as first token (all offered today and correct: the first token starts its line).
- RH7306: `#endregion //Implementation` (no space) and `#endregion // Implementation` (both "alone", withheld by RH7306-004); `#endregion //IDisposable implementation` (offered, correct today); partner `#endregion //implementation` behind a reported `#region IDisposable implementation` — today the `#region` loses the word, the partner is left unchanged, its own RH7306 diagnostic stays and its fix then changes nothing (`no-progress`), and the two descriptions now differ (RH7301). That partner is a member of Q3 (spec author): with a space it becomes `#endregion //`, without one it stays `#endregion //implementation`, while RH7306-003 says the partner line "ends with the word as well" in both.
- RH7309: comment above a moved block, no blank line (withheld, bullet 3); `///` directly above the first block when that block moves (withheld); `///` behind a member in the code between blocks when the next block changes (withheld, bullet 4); comment above a block that keeps its slot (Q2); comment with a blank line below it (Q1).

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH7303 code fix | must change |
| RH7303 analyzer | must stay |
| RH7304 analyzer | must change (RH7304-005) |
| RH7304 code fix | must change |
| RH7306 code fix | must change; analyzer must stay — but see Architecture findings |
| RH7309 code fix | must change |
| RH7309 analyzer / `GetTopLevelRegions` (Reihitsu.Core) | depends on Q-S3: changing the shared helper reaches every consumer listed above (escalation 5) |
| Rule pages | not involved |

### Simpler option

_None._

### Questions for the maintainer

**Q-S2 — RH7303, the mirror of the straddling region.** The decision on this issue withholds the fix "for the pair" when `#region` is at type level and only `#endregion`, inside `M`, is reported. The mirror:

```csharp
class C
{
    void M()
    {
        #region Work
        M();
    }

    #endregion
}
```

Only `#region Work` is reported. Today the fix removes both lines. The clarified SHARED-FIX-003 (condition 3) explicitly allows it: "a fix that removes a region whose `#region` its diagnostic reports … removes the region's `#endregion` with it". RH7303-004 claims only pairs with both directives reported, so the spec allows both outcomes.

- **A** — Offer the fix (today): both lines go. The type-level `#endregion` that nothing reports is removed.
- **B** — Withhold it like the issue's shape: a pair with one directive between declarations is never removed by RH7303. Symmetric with the decided shape; the user removes the pair by hand.

Recommendation: **B** — the decision speaks of a straddling pair, and B gives both shapes one rule. A would be preferred if the maintainer sees the `#endregion` as belonging to the misplaced `#region`, as SHARED-FIX-003 now words it.

**Q-S3 — RH7309, regions that `GetTopLevelRegions` counts but SHARED-TYPEREGION-006 does not.** Inputs and today's results are under Premise (nested enum; switch expression; field initializer). The issue asks only for the fix to be withheld; the nested-enum shapes are reported against SHARED-TYPEREGION-006, by a Reihitsu.Core helper that eight analyzers and two fixes use.

- **A** — Correct `GetTopLevelRegions` for nested enums in this run, so the regions of `E` are no longer regions of `C` for any consumer; withhold the RH7309 fix for regions inside expressions (SHARED-FIX-003). A change to a shared helper with other consumers (escalation 5), with tests for each consumer the change reaches.
- **B** — Withhold only the RH7309 fix for these inputs; the analyzer keeps reporting the nested-enum region of `C`. The RH7309 fix then decides on its own which regions it may move, next to `GetTopLevelRegions` deciding which regions are top-level — two owners of one decision.
- **C** — Leave the region-selection instances out of this run; withhold nothing for them now, and file the helper defect (nested enums; expression contexts that SHARED-TYPEREGION-005 does not classify) as a follow-up issue. The issue then closes without this instance, or is linked with `Refs`.

Recommendation: **C** — the defect is in the shared helper and in a gap of SHARED-TYPEREGION-005, both outside the fix the issue is about; A grows the run's scope, and B creates a second owner. A would be preferred if the maintainer wants #1262 closed as written.

### Architecture findings

**RH7306: the analyzer and its code fix each decide whether a region "ends with the word `implementation`", on different texts.**

- Components: `RH7306RegionDescriptionsShouldNotEndWithImplementationAnalyzer.EndsWithForbiddenSuffix` and a private copy with the same name in `RH7306RegionDescriptionsShouldNotEndWithImplementationCodeFixProvider` (one uses `Trim`, the other `TrimEnd`).
- Shared decision: whether the description ends with the word, preceded by whitespace or the start of the description. The analyzer applies it to the description (SHARED-REGDESC-001 via `RegionDescriptionAnalyzerBase`); the fix applies it to the whole directive line, where the character in front of a description-initial word is `/`, not whitespace.
- Inputs that show the disagreement: `#endregion //Implementation` behind `#region IDisposable` — reported, the fix changes nothing (`no-progress`, the #1262 instance); `#endregion //implementation` behind `#region IDisposable implementation` — the partner keeps its word, so the pair disagrees (RH7301) and the partner's RH7306 fix changes nothing (`no-progress`). Members that hold: a space after `//`, more words in front of the word, every `#region` line (description and line tail coincide).
- RH7306-004 withholds the fix for the first input, but the second (and Q3) still depend on which text the fix judges.

### Follow-up candidates

- The helper defect of Q-S3, if the maintainer picks C.
- SHARED-TYPEREGION-005 does not classify an equals-value clause, an arrow expression, an attribute argument, or a parameter default as executable code or as between declarations; a spec gap shared by RH7303, RH7304, RH7310 and every top-level-region rule.

## thoenissen/Reihitsu#1332

### Premise

Real for every input. `RH7501…CodeFixProvider.TryGetFixableNodes` checks only that the `break` ends the block and the block ends the section. The fix moves the `break` with its whole leading trivia and leaves the closing brace's leading trivia in the block. Observed: Q7 input 1 (with `DEBUG`: `A(y)` leaves the scope of `y`; without it: the block closes in front of `#if DEBUG`); input 2 (`invalid-result`); input 3 (`A(); break;` sharing the line: `invalid-result`; on its own line: offered, `#if DEBUG A(); #endif` stays in the block and runs in the DEBUG build); input 4 (`disable` out, `restore` in); Q8 inputs 1–3 (`invalid-result`, a duplicated `break`, `A(); /* first */                 }`). Q9 `{ break; }` → `{ }` + `break;`: kept as RH7501-004 now requires. A `#nullable disable` in front of the `break` is moved out too (same mechanism).

### Mechanism and candidates

The fix moves the `break` token with all trivia from the end of the previous statement up to the `break`, and leaves all trivia between the `break` and `}` in the block, without checking what those spans hold or whether they share a line with other code. Axes: what lies in front of the `break` / between it and `}` — comment, conditional directive, region directive, `#pragma`/`#nullable`, disabled text; line sharing — `break` after code on its line, `}` after the `break` on its line, the whole block on the case label's line.

### Breaking inputs

FC-004, FC-019, FC-020, FC-021, FC-026 (Q8 input 2's duplicated `break`). The spec author's examples cover each axis. Extra members: `#nullable` in front of the `break`; a `#region`/`#endregion` pair wholly in front of the `break` (withheld by the third condition, by design); a comment on its own line between `break` and `}` (offered today; the comment stays above `}`, which SHARED-FIX-003 condition 4 accepts).

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH7501 code fix | must change |
| RH7501 analyzer | must stay |
| Fix All | must stay |
| Rule page | not involved |

### Simpler option

_None._

### Questions for the maintainer

_None._

### Architecture findings

_None._

### Follow-up candidates

_None_ (the blank-line case is thoenissen/Reihitsu#1437, outside the run).

## thoenissen/Reihitsu#1336

### Premise

Real for every instance:

- A1 (RH7004): as SHARED-FIX-003's RH7004 example states.
- A2: `using var s = new MemoryStream(); // resource` → `} // resource`.
- B3 (RH7101): `namespace N { int a, b; }` and `namespace N; int a, b;` → `no-progress`. The fix registers whenever the declaration carries no directive, and `ApplyCodeFixAsync` returns the document unchanged when the parent is not a type.
- N2 (RH7105/RH7106): `static public⏎#pragma …⏎int _a;`, the `#if`/`#else` variant, the `class D` variant, and `internal protected` all give `invalid-result`.
- RH7101 documented declaration (mode `Parse`, `///` and `/** */`, also with an attribute between the comment and the declaration): split, comment kept on the first field only.

**The RH7004 comment defect is wider than A2.** The fix builds the `using` statement from `usingDeclaration.Declaration`, so the `using` keyword and the `;` are replaced. Observed:

- `using /* c */ var s = …;` → `/* c */` is dropped;
- `using var s = new MemoryStream()⏎    // c⏎    ;` → `// c` is dropped and the output is `using (var s = new MemoryStream()⏎)` (FC-007 risk: a layout other rules report);
- `await /* c */ using var s = …;` → kept (the `await` keyword is reused);
- a comment in front of `using` → kept.

Dropping a comment is SHARED-FIX-003 condition 3, so these are bugs already decided by the spec, of the same class as A2.

### Mechanism and candidates

- RH7004: the fix keeps the trivia of the declaration's `Declaration` node and its leading trivia, sets the declaration's trailing trivia behind the new block, and loses the trivia of the replaced `using` and `;` tokens; the trivia in front of the parent block's `}` stays outside the new block. Trivia positions: leading trivia of `await`/`using` (kept), trailing trivia of `await` (kept), trailing trivia of `using` (dropped), leading trivia of `;` (dropped), trailing trivia of `;` (relocated: A2), the parent `}`'s leading trivia (stays outside the block: A1).
- RH7101 B3: the fix's registration and its rewrite disagree on the parent kind. Candidates: block namespace, file-scoped namespace (both fail).
- RH7105/RH7106: the fix replaces the last modifier's trailing trivia, including its line break, with one space, and never inspects the leading trivia of the token after the last modifier. Candidates for that trivia: a directive of any kind (fails, invalid), disabled text (fails, with `#if`), a comment on its own line or in front of the token on its line (Q4; today a blank line is inserted, which SHARED-FIX-006 *Blank lines* already forbids). Next token: type, `class`, `void`, `event`, `delegate`, constructor name.

### Breaking inputs

FC-004, FC-009, FC-020, FC-021, FC-016. Candidates: the issue's inputs; the RH7004 dropped-comment members above; RH7105 and RH7106 with `#pragma`, `#nullable`, `#if`/`#else`/`#endif` and `#region` behind the last modifier; RH7101 in both namespace forms; RH7101-006 with `///`, `/** */`, with an attribute list between the comment and the declaration, and the mode `None` and ordinary-comment counter-examples.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH7004 code fix | must change (A1, A2, and the dropped-comment members) |
| RH7101 code fix | must change (B3 registration; RH7101-006) |
| `FieldDeclarationSplitTransform` (formatter) | must stay in this run; the fix registers with the transform's own guard (`CarriesDirective`) today, and thoenissen/Reihitsu#1384 will give the transform a documentation guard. A fix-local documentation check now and a transform guard later would state "a documented declaration is not split" twice (RH7101-006 and FMT-STRUCT-021/-022). |
| `ModifierOrderingCodeFixProviderBase` (RH7105, RH7106) | must change |
| Analyzers | must stay |
| Rule pages | not involved |

### Simpler option

_None._

### Questions for the maintainer

_None_ beyond the spec author's Q4. Q4's input class has two members — a comment on its own line, and a `/* */` comment in front of the type on its line — and both insert a blank line today.

### Architecture findings

_None._

### Follow-up candidates

- Pre-existing, FC-030 against SHARED-FIX-006 *Formatting*: the RH7105/RH7106 fix formats the whole member, so `static public class D { int M() => 1; }` comes back with `M` given a block body. No open issue found by search; the lead should check for duplicates before proposing one.

## thoenissen/Reihitsu#1340

### Premise

Real for the reporting: `RH7003FileScopedNamespacesShouldBeUsedAnalyzer.IsEligibleNamespace` checks only that the namespace is the compilation unit's only member and the language version. The issue's description of a fix is wrong: RH7003 has no code fix (spec header `Code fix | None`, no provider), so no "broken fix disappears". The user is still asked for a conversion that cannot compile (CS8955), so the change stands at the same severity. RH7003-001's claim already excluded the case; RH7003-005 only adds the `MUST NOT`.

### Mechanism and candidates

The analyzer checks the namespace's position in the compilation unit, not what the namespace contains. Candidates: one nested namespace (fails), several (fails), a nested namespace after a type (fails), a namespace containing only types (holds, reported).

### Breaking inputs

FC-002 (the claim's qualifier "contains neither a namespace declaration" never became a condition). Members as above. One member is decided by composition rather than by RH7003-005: a nested namespace only in an `#if` branch that the analyzed configuration excludes is disabled text, not a member, so RH7003-005 does not apply. SHARED-DIAG-008 and the existing test `VerifyDirectiveContainingNamespaceIsReported` then make it reported, although the other configuration cannot convert. Listed under Follow-up candidates rather than as a question, because the specs decide it.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH7003 analyzer | must change |
| Code fix | not involved (none exists) |
| Rule page | must stay (the spec author's reason holds; the issue's "Done when" asks for a rule-page statement, so the pull request should say why it was not added) |

### Simpler option

_None._

### Questions for the maintainer

_None._

### Architecture findings

_None._

### Follow-up candidates

- The nested namespace in an excluded `#if` branch (see Breaking inputs).
- `documentation/rules/RH7003.md` carries an "Exceptions" section (spec author's note).

## Whole run

- Fixtures: scratch directories under the session scratchpad, deleted after the runs; every count above is from the runner's summary line (`fixed`, `invalid-result`, `no-progress`, `no-diagnostic`).
- Escalations for the lead: the architecture finding under #1262 (escalation 12); Q-S1, Q-S2, Q-S3 together with the spec author's Q1–Q4; the SHARED-FIX-003 brace-split repair (#1136) goes to the spec author before tests are written.
