Role: skeptic
Commit: ed3790320e5e553359ff9ee94561ec59a71f4d45
Verdict: proceed narrower

The prompt carried no intended fix, suspected cause, or hoped-for verdict. It named one fact (`InterfaceImplementationUtilities` is also called by `DocumentationAnalysisUtilities`). I checked that fact against the code myself; the result is S6.

## Premise

Lane: specs and tests for existing behavior. So the premise is whether each requirement states what the code does today, and leaves out what looks wrong. I read the code that decides each statement: `MembersShouldBeGroupedByRegionsAnalyzerBase`, `NonOverrideMembersShouldNotBePlacedInOverrideRegionsAnalyzerBase`, the twelve `TryGetExpectedRegionName`/`TryGetOverrideRegionName`/`IsRelevantNonOverrideMember` overrides, `OverrideMemberUtilities`, `InterfaceImplementationUtilities`, `RegionDirectiveUtilities.GetTopLevelRegions`/`TryFindContainingRegion`/`GetRegionDescription`, `OrderingDeclarationUtilities.GetDiagnosticLocation`, and the RH7501 analyzer and code-fix provider. I ran the RH7501 fix with `scripts/apply-fix.sh RH7501` on 10 fixtures (LF and CRLF), and `scripts/format.sh` on 2 fixtures. All fixtures were in the scratchpad and are deleted.

Confirmed as written: the four facts the spec author's handover lists under "Code facts"; SHARED-REGGROUP-001, -003, -004, -005; RH7401-001 to RH7412-002 apart from S1 to S4; RH7501-001 to -003; and RH7501-004's after-block (`apply-fix`: `fixed`, LF and CRLF, output equal to the spec). Q1 is correctly left out: SHARED-REGGROUP-007's positive sentence is sufficient-only, and its negative sentence covers only regions that match no override of any kind. The two together do not decide a region that only an override of another member kind names. Q2 is correctly left out, because "the member it implements" is undefined when there are two. I agree with the spec author on #1240: an RH74xx verdict cannot change, because inactive region pairs enclose no member.

Findings. Each one requires a spec change or a new question before tests are written.

**S1 — SHARED-REGGROUP-002 restates SHARED-TYPEREGION-006 (duplicate shared requirement).** SHARED-TYPEREGION-006 already defines when a member lies in a top-level region: "wholly between … also when it lies in a region nested in it". It also says that a region in a member body is not top-level. SHARED-REGGROUP-002 states the same definition a second time, narrowed by "in no region nested in that region", and points back to SHARED-TYPEREGION-006 for "top-level region". The narrowing keeps nothing out of the spec. SHARED-REGGROUP-001 uses SHARED-TYPEREGION-006, so the nested case is decided through that requirement anyway, and it matches the code (`TryFindContainingRegion` returns the top-level pair around a nested region). The cost is two definitions that will diverge. #1260 rewrites SHARED-TYPEREGION-006 for P17, and that edit would not reach SHARED-REGGROUP-002. This is FC-015 in spec form, and it goes against the lead's clarification to reference SHARED-TYPEREGION IDs instead of restating them. Precedent: RH7308-001 uses SHARED-TYPEREGION-006 directly and narrows inline ("outside any region nested in it"). The spec author's statement that the nested-region case is "deliberately left out" is therefore not accurate as the diff stands.

**S2 — SHARED-REGGROUP-006 does not match the code for a field-like event with several declarators.** The bases report once per `MemberDeclarationSyntax`, at `Declaration.Variables[0].Identifier` (`GetDiagnosticLocation`). The relevance and region checks read only `Variables[0]` (RH7403, RH7407, RH7411). "One diagnostic per reported member … the identifier of its declarator" requires two diagnostics for `public event EventHandler F, G;`. Two inputs where the code gives one diagnostic, at `F`:

- RH7407: `F, G` declared in an override region.
- RH7411: `public event EventHandler A, X;` with both members from `IA`, outside any region.

P20 (SHARED-DIAG-006 via #1260) decides how many diagnostics such a declaration gets. Writing "the first declarator's identifier", as the merged RH7308 and RH7103 specs do, would write a P20-decided case into the spec. The clause has to cover only a declaration with one declarator. The diagnostic count and location for several declarators go into Q3 (P20).

**S3 — RH7501-004's conditions do not exclude directives or excluded code around the `break`.** Q4's inputs fall inside the requirement as written: there is a statement in front of the `break`, and the closing brace is first on its line. So RH7501-004 currently says the fix MUST be applied where P1 says it must be withheld. Two more real outputs that the same condition has to exclude:

- `A();` / `#region Exit` / `break;` / `#endregion` / `}` gives `apply-fix: invalid-result`. The `#endregion` stays inside the block, and `#region Exit` plus `break;` land behind it. The result is a compile error the code did not have (P1, SHARED-FIX-003).
- `A();` / `break;` / `#if DEBUG` / `A();` / `#endif` / `}` gives `fixed`. The `#if` block stays inside and `break;` moves behind the `}`, so in a DEBUG build `A()` now runs a second time. That is a run-time change in another configuration (P1, SHARED-FIX-003).

The excluding condition therefore covers the whole span from the statement in front of the `break` to the closing brace, not only the `break`'s leading trivia. Add both inputs to Q4 as P1 instances.

**S4 — RH7501-004 contradicts the real output when the `break` shares its line with the statement in front of it.**

```csharp
                {
                    A(); break;
                }
```

This gives `fixed`. The output is `A();                 }`, with the closing brace pulled up onto `A()`'s line, followed by `                break;`. The same happens with `A(); /* first */ break;`. Both of RH7501-004's conditions hold, and the requirement says "MUST leave the rest of the switch as it is laid out". That is false here. It is a P9 instance (SHARED-FIX-006 via #1138). RH7501-004 needs a third condition (the `break` is the first token on its line), and the input goes into Q5 next to Q5's input 2.

**S5 — The partial-type case of SHARED-REGGROUP-007 is undecided in the text, but the spec reads as deciding it, and the handover does not mention it.** `OnTypeDeclaration` collects the override region names only from the `Members` of the declaration being analyzed. Input: `partial class C : B { public override void M() { } }` together with `partial class C { #region B  public void N() { }  #endregion }`. Real output (from the code): RH7405 does not report `N`. It would report `N` if `M` were in the same declaration. SHARED-REGGROUP-001's "each declaration of a `partial` type on its own" (Why: "what the user reads in one file") and 007's "an override that the type declaration declares" read as stating this per-declaration verdict. 007's negative sentence ("no type that an override of the type comes from") leaves it open. The verdict on `N` depends on which file holds `M`, which is the same objection the spec author raises against Q1. P15 ("a partial declaration is one declaration", #1260) is close, but it is literally about judging the partial element, and `N` is not partial, so it does not decide this case. Leave it out and file it (see Questions).

**S6 — The ownership claim on `InterfaceImplementationUtilities` is admissible, but the clarification behind it rests on a false premise.** The lead's clarification records the maintainer's permission as "used only by RH7401–RH7412". In fact `DocumentationAnalysisUtilities.OverridesOrImplementsMember` calls `GetImplementedInterfaceName(...).Length != 0`. That call decides "implements an interface member" for RH8201 (directly for event fields, and through `CanInheritDocumentation`) and for RH8203 (through `CanInheritDocumentation`). RH8201-001/-002 (`sdd/specs/analyzer/RH8201.md`, around lines 29–30 and 115) specify that predicate in more detail than SHARED-REGGROUP-005 does: base interfaces of listed ones, `new` in a type that does not relist the interface, a base member that only a derived type's interface would map to. RH8203-001 states it too. By `sdd/README.md`, behavior decides ownership, not callers. SHARED-REGGROUP-005 specifies *which* interface (the override exclusion lives in `GetInterfaceRegionName`), and the documentation specs rely on *whether* a member implements one, which no SHARED-REGGROUP requirement states. So no `Uses` entry is owed, and the "at most one spec" rule holds. The risk is coupling. A change to `GetImplementedInterfaceName` made under SHARED-REGGROUP, such as resolving Q2 or restricting the interface walk, changes RH8201 and RH8203 output, while nothing in those specs points at SHARED-REGGROUP. `OverrideMemberUtilities` is used only by RH7401–RH7408, so claiming it is sound.

**S7 — There is no layout that is both formatter-stable and RH74xx-clean for a type whose simple name starts with a lower-case letter.** SHARED-REGGROUP-003 requires the exact name, for example `#region myBase` around an override of `myBase.M`. FMT-REGION-001 rewrites that to `#region MyBase`. `scripts/format.sh` on such a fixture writes `#region MyBase` / `#endregion // MyBase`, and then RH7401 reports (ordinal comparison). RH7302 independently reports `#region myBase`, so no region name satisfies both rules. This is FC-013 (formatter output is flagged), and an analyzer-versus-analyzer conflict, under P4 (case by case). It arises only for types the naming rules do not report, such as types from metadata or with suppressed casing rules, so it is low severity. It applies to all twelve RH74xx rules, interfaces included. SHARED-REGGROUP-003 writes the case into a requirement, so it should be left out and filed.

## Mechanism and candidates

_N/A._ This lane is not a bug fix. The dispatch-derived candidates are listed under Breaking inputs.

## Breaking inputs

Dispatch: both bases register `ClassDeclaration`, `StructDeclaration`, `RecordDeclaration`, and `RecordStructDeclaration` (not interface, enum, or extension block), and iterate the direct `Members` of the declaration. The member types are `MethodDeclarationSyntax` (RH7401, 7405, 7409), `PropertyDeclarationSyntax` (7402, 7406, 7410), `EventDeclarationSyntax` and `EventFieldDeclarationSyntax` with `Variables[0]` (7403, 7407, 7411), and `IndexerDeclarationSyntax` (7404, 7408, 7412). RH7501 registers `SyntaxKind.BreakStatement`. Its fix guard is `TryGetFixableNodes`, and it moves the `break` together with its whole leading and trailing trivia.

- **FC-024 (partial, explicit implementation, static interface members).** First, S5's partial type, for each of RH7405–RH7408. Second, a partial method or partial property whose definition part and implementation part lie in different regions, for RH7409/RH7410 (an implicit interface implementation) and for RH7401/RH7402 when the parts override. The interface rules compare `GetDeclaredSymbol(declaration)` with `FindImplementationForInterfaceMember`, which returns one symbol, so at most one of the two parts can be judged. I could not run an analyzer-only rule: `apply-fix.sh` needs a code-fix provider. This candidate is therefore unverified, derived from the code. If one part is skipped, or the member is reported twice, the requirement's "each method declaration that comes from an interface" overstates, and the input is a P15 instance to file. Also: an operator or conversion operator implementing a `static abstract` interface operator (not a method declaration, so not reported by RH7409); an explicit implementation of a generic interface; a user-written `Equals(R? other)` in a record (implements `IEquatable<R>`).
- **FC-023.** `class @Base` matched by `#region Base` (`ISymbol.Name` is the value); `#region @Base` does not match.
- **FC-027.** A nested type inside a region of the outer type (SHARED-REGGROUP-001 example). A member in a region nested in a top-level region (the P17 instance on #1264) is characterization only.
- **FC-013.** S7: a lower-case type name, `scripts/format.sh` followed by the analyzer.
- **FC-004, FC-019, FC-020, FC-021 (RH7501 fix).** Each run of `apply-fix.sh RH7501`, LF and CRLF:
  - `#region` / `#endregion` around the `break`: `invalid-result` (S3).
  - `#if DEBUG … #endif` between the `break` and `}`: `fixed`, run-time change in DEBUG (S3).
  - Q4's `#if` and `#pragma` inputs.
  - A `//` comment on its own line between the `break` and `}`: `fixed`. The comment stays inside the block and the `break` moves behind it. This is consistent with "leave the rest as laid out"; pin it.
  - `A(); break;` and `A(); /* first */ break;` on one line: `fixed`, with the brace joined onto the previous line (S4).
  - `} // end of default`: `fixed`, the `break` lands on the next line.
  - `case 1: {` with the brace on the label line: `fixed`, the `break` gets the closing brace's column.
- **Blank line in front of the `break` (the common input).** SHARED-BLANK-004 does not exempt a `break` inside a block, so RH5010-clean input has a blank line before the `break`. The fix moves that blank line too, so the result reads `}`, a blank line, `break;`. `scripts/format.sh` keeps that blank line, so this is not a P10 instance. RH7501-004 does not state it, and its "directly behind the block" and its Why ("the shape of FMT-SWITCH-002", which has no blank line) suggest otherwise. See Questions.
- **FC-008.** Fix All on a reported block that contains a nested `switch` whose own section block is reported. One-at-a-time application gives `fixed` in two iterations. The test implementer covers Fix All through `GetFixAllScenario`.
- **FC-007.** Every `fixed` fixture cleared its diagnostic in one application per diagnostic, with no line-ending drift.
- **RH7501-002.** `yield break;` ending a section's block in an iterator. This is a `YieldStatementSyntax` and is not reported.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH7401–RH7412 analyzers and both bases | must stay |
| `OverrideMemberUtilities`, `InterfaceImplementationUtilities` | must stay; the second also decides RH8201 and RH8203 (S6) |
| RH7501 analyzer and code fix, Fix All | must stay |
| `Reihitsu.Core` (`RegionDirectiveUtilities`, `OrderingDeclarationUtilities`) | must stay, not claimed |
| Formatter (`RegionFormattingPhase`, `SwitchCaseBracePhase`) | not involved; FMT-REGION-001 conflicts with SHARED-REGGROUP-003 (S7) |
| `sdd/specs/shared/member-region-grouping.md` | must change (S1, S2, S5, S7) |
| `sdd/specs/analyzer/RH7401.md` … `RH7412.md` | must change where they use SHARED-REGGROUP-002 (S1) |
| `sdd/specs/analyzer/RH7501.md` | must change (S3, S4; blank-line statement or question) |
| `documentation/rules/RH74*.md`, `RH7501.md` | must stay (consistent with the specs) |
| `sdd/specs/analyzer/RH8201.md`, `RH8203.md`, `shared/element-documentation.md` | not involved (S6 changes nothing in them) |
| Analyzer tests | must change: tags plus characterization tests for the inputs above |

## Simpler option

Drop SHARED-REGGROUP-002 and have the rule requirements say "lies in a top-level region (SHARED-TYPEREGION-006)", with RH7308-001's inline "outside any region nested in it" if the run wants the P17 case out of the RH74xx text. When #1260 rewrites SHARED-TYPEREGION-006, the RH74xx specs then follow it together with RH7308.

## Questions for the maintainer

None of these blocks the run. The lead relays the first. The others are open questions for the squad to file in the per-spec issues, or principle instances to add to existing questions. Each one leaves its case out of the spec.

1. **Ownership (relay, no output difference).** The permission to claim `InterfaceImplementationUtilities` assumed it is used only by RH7401–RH7412. It also decides RH8201's and RH8203's "implements an interface member" (S6). Does the maintainer keep the claim on SHARED-REGGROUP, knowing that a change made under that spec reaches the documentation rules?
2. **Shared spec, new question — overrides in another part of a partial type (S5).** Input: as in S5. (A) A region is an override region when any declaration of the type declares an override from that type, so `N` is reported. (B) Only the analyzed declaration counts (today), so `N` is not reported. Touches SHARED-REGGROUP-001, SHARED-REGGROUP-007, RH7405-001 to RH7408-001 and RH7405-002 to RH7408-002.
3. **Shared spec, new question — a type name that starts with a lower-case letter (S7, P4).** Input: `class myBase { public virtual void M() { } }` and `class C : myBase { #region myBase  public override void M() { }  #endregion }`. (A) Keep the exact ordinal match (today): RH7302 reports the region, and after formatting RH7401 reports `M`. (B) Match the name after FMT-REGION-001's capitalization: `#region MyBase` is accepted, and nothing is reported after formatting. Touches SHARED-REGGROUP-003 and every RH74xx `-001`.
4. **Add to Q3 (P20).** For several declarators, the diagnostic count and location (S2).
5. **Add to Q4 (P1).** The `#region`/`#endregion` input and the excluded-code-after-`break` input (S3).
6. **Add to Q5 (P9).** `A(); break;` on one line (S4).
7. **RH7501, blank line in front of the `break`.** Input: section block `{ A(); <blank line> break; }`. (A) The blank line moves with the `break` (today), giving `}` / blank line / `break;`. (B) The `break` lands directly under `}`, the FMT-SWITCH-002 shape. P14a (#1138: a fix does not change the blank lines between the node and its neighbours) may decide it. If so, file it as a P14a instance; otherwise ask. Either way RH7501-004 states neither reading until the question is decided. Touches RH7501-004.

New-rule candidate (P11) from the spec author: no objection.
