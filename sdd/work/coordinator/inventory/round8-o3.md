# Inventory — round 8, family O3 (#995, PR #1302: member grouping by regions RH7401–RH7412, RH7501)

Issues #1304 (shared/member-region-grouping.md, Q1–Q6) and #1305 (RH7501, Q7–Q10), read 2026-10-21. Both open, neither has comments, neither is a sub-issue of #912 (listed there by comment). 10 questions; Q7 and Q8 have several inputs. All are `SDD open questions` issues; none skipped.
`⏎` = line break. Duplicates were checked against the titles of all 200 open issues (list_issues, both pages); no search API used.

Principles used: P1 unsafe fix (SHARED-FIX-003), P4 analyzer/formatter disagreement case by case (narrowed by P11), P9 fix scope (SHARED-FIX-006, #1138), P10 fix layout (SHARED-FIX-007, #1138), P14a no blank line inserted / blank lines to neighbours unchanged (#1138), P15 partial declaration = one declaration (SHARED-DIAG-004, #1260), P17 innermost declaration/region (#1260), P20 every element of its kind (SHARED-DIAG-006, #1260). AGENTS.md "Code-fix coverage" (no drop/reorder/relocate of directives) with its pending spec wording #1136.

| Issue | Q | Rule | Input → real output | Why it looks wrong | Req IDs | Principle | Duplicates / related | Kind | Fully decided? |
|---|---|---|---|---|---|---|---|---|---|
| #1304 | Q1 | RH7405–RH7408 | `class B { virtual int P }`; `class C : B { #region B⏎ override int P⏎ void N() #endregion }` → nothing reported; add any override *method* of `B` to `C` → RH7405 reports `N` | Whether `#region B` is an override region depends on whether an unrelated override of the rule's own member kind exists, not on the region `N` lies in | SHARED-REGGROUP-006, RH7405-001..RH7408-001, RH7405-002..RH7408-002 | none (issue: "not decided") — new pattern: verdict depends on an unrelated sibling (P25 candidate) | same objection as Q4; related Q2, Q6 | Behavior change (A: any override of any kind makes it an override region → new diagnostics; rule pages RH7405–RH7408 change). Recommend A | No |
| #1304 | Q2 | RH7409–RH7412 | `interface IA { void M(); } interface IB { void M(); }`; `class C : IA, IB { #region IB⏎ public void M(){} #endregion }` → RH7409 reports `M`, asks for `IA`; `class C : IB, IA` → nothing | Verdict changes with base-list order, not with code; `M` implements both | SHARED-REGGROUP-004, RH7409-001..RH7412-001 | none literally; P20's "verdict does not depend on position/order" and P18 point the same way (P25 candidate) | related Q1, Q4, Q6 | Bug-like order dependence; formally behavior change (fewer diagnostics). Recommend A (region of any implemented interface accepted) | No |
| #1304 | Q3 | RH7403, RH7407, RH7411 | `public override event EventHandler A, X;` in `#region B1` with `X` from `B2` → nothing; `public event EventHandler A, X;` in `#region IA` with `X` from `IB` → nothing; override region decided only by first declarator (`F` not reported); `public event EventHandler F, G;` in override region → one diagnostic at `F`; two unregioned interface events → one diagnostic at `A` | Only the first declarator is judged and reported | SHARED-REGGROUP-005, -006, RH7403-001, RH7407-001, RH7411-001 | P20 (marked) — verified: SHARED-DIAG-006 names "declarator of a field or event" and "order of the declarators" literally. Caveat: #1260's SHARED-DIAG-006 wording says nothing about the *number and location* of diagnostics (inputs 4/5); SHARED-REGGROUP-005 covers only one-declarator events | #1270 (P20 instance for RH8106/RH8203, other family); #1260 | Bug against SHARED-DIAG-006 (after #1260) | Yes — new P20 issue for RH7403/RH7407/RH7411 (blocked by #1260); add to #1260 the sentence "one diagnostic per declarator, at its identifier" (or decide it there) |
| #1304 | Q4 | RH7405–RH7408 | `class B { virtual void M() }`; `partial class C : B { override void M() }` + `partial class C { #region B⏎ void N() #endregion }` → `N` not reported; same part → reported | Verdict depends on which file holds the override | SHARED-REGGROUP-001, -006, RH7405-001..RH7408-001, -002 | Issue: "P15 close but not decided". Assessment: P15 judges a partial *type* by its symbol, and the override set is a property of the type symbol, so P15 fits in spirit; literally it names modifiers/async/static/accessibility only. P25 candidate also covers it | same objection as Q1; #1287 (P15 for RH2002/RH2004/RH2101); SHARED-REGGROUP-006 already says "in any declaration of the type" in its second sentence (spec self-inconsistent) | Behavior change (A: override in any part counts → new diagnostics). Recommend A | No (P15 extension; confirm) |
| #1304 | Q5 | RH7401–RH7412 vs RH7302 / FMT-REGION-001 | `class myBase { virtual void M() }`; `class C : myBase { #region myBase⏎ override void M() #endregion }` → RH7302 reports `#region myBase`; formatter rewrites to `#region MyBase` / `#endregion // MyBase`; then RH7401 reports `M` (ordinal) | No layout is clean for RH7302 + RH7401 and stable under the formatter | SHARED-REGGROUP-002, RH7401-001..RH7404-001, RH7409-001..RH7412-001, RH7405-001..RH7408-001 | P4 (marked, case by case) — correct; P11 does not apply (no analyzer widening). New pattern: unsatisfiable rule pair (P26 candidate, weak) | #964, #1151, #1239 (region line normalization, different cause); SHARED-FIX-004 (no fix here, so not literally violated) | Behavior change, low severity. Recommend B (also accept the name with its first character upper-cased by FMT-REGION-001's rule) | No |
| #1304 | Q6 | RH7401–RH7412 | `partial class C : B { #region Methods⏎ public override partial void M(); #endregion #region Other⏎ public override partial void M(){} #endregion }` → RH7401 reports `M` twice; interface case: defining parts in `Methods`, implementing parts in `I` → `M`, `P` reported once at defining parts; swapped → nothing | One member reported twice; verdict depends on which of two parts lies in the matching region | SHARED-REGGROUP-003, -004, -005, RH7401-001..RH7412-001, RH7405-002..RH7408-002 | P15 (marked) for the double report — verified: SHARED-DIAG-004 "reports it once". Which part decides: not decided (P25 candidate covers it) | #1287 (P15 instances); Q4 | Bug (double report, P15) + behavior choice (A both parts must lie in a matching region / B defining / C implementing). Recommend A, one diagnostic at the first part outside a matching region | No (double report Yes → extend #1287; part choice needs maintainer) |
| #1305 | Q7 | RH7501 fix | Input 1: `{ var y = 1;⏎#if DEBUG⏎ A(y);⏎#endif⏎ break; }` → `#if…#endif` + `break;` moved behind `}`; DEBUG build CS0103. Input 2: `{ A();⏎#region Exit⏎ break;⏎#endregion }` → `#endregion` stays inside, `#region Exit` + `break;` behind → does not compile. Input 3: `{ A(); break;⏎#if DEBUG⏎ A();⏎#endif }` → DEBUG runs `A()` a second time. Input 4: `#pragma warning disable CS0162` before / `restore` after `break` → `disable` moves out, `restore` stays inside → suppression runs to end of file; moved `break` keeps column 20 instead of 16 | Fix breaks code (1, 2), changes run-time behavior (3), reorders directives (4) | RH7501-004, SHARED-FIX-003 | Inputs 1–3: P1 (marked) — verified: input 2 = bullet 1 (compile error), input 3 = bullet 2 (run time), input 1 = bullet 1 read across configurations (bullet 4 literally names only renames; flag). Input 4: not one of P1's four bullets, but AGENTS.md Code-fix coverage ("reorder, or relocate … directives … must withhold itself") decides it; spec wording pending in #1136 | #1136 (SHARED-FIX-003 relocation/reorder of directives — exact cause of input 4); #948–#950 (formatter switch bugs, unrelated cause) | Bug: withhold the fix when a directive or excluded code lies between the statement before the `break` and the `}` (RH7501-004 already lists this as a condition, but does not say "withheld otherwise") | Yes — new RH7501 bug issue (with Q8); add input 4 as an instance on #1136 |
| #1305 | Q8 | RH7501 fix | Input 1: `case 1: { A(); break; }` (one line) → text before the brace is used as the `break`'s indentation → does not parse. Input 2: block ending `break; }` → a second `break;` appears, block `}` moves to column 0, switch `}` joins (`break; break;         }`). Input 3: `A(); break;⏎}` (or `A(); /* first */ break;`) → `A();                 }` then `break;` | Fix breaks code (1), adds code and rearranges neighbours (2, 3) | RH7501-004, SHARED-FIX-003, SHARED-FIX-006/007 (planned) | Input 1: P1 (marked) — verified, bullet 1. Inputs 2–3: P9 (marked) — verified: the switch `}` / block `}` / `A();` line are code around the change. Input 2's duplicated `break` is also outside P9's "only the code its diagnostic is about" | #1138 (blocks P9 bugs); #1140 (P9 line-break fixes, other rules) | Bug. Whether the fix is withheld for shared lines (RH7501-004's conditions 2 become the guard) or lays the `break` out on its own line (P10/P14b) is the squad's choice within P9; recommend withhold, matching the conditions the spec already states | Yes — same new RH7501 bug issue (blocked by #1138) |
| #1305 | Q9 | RH7501 | `case 1:⏎ {⏎ break;⏎ }` → reported; fix gives `case 1:⏎ {⏎ }⏎ break;` | FMT-SWITCH-006 calls `{ } break;` noise; formatter never produces it (but keeps an existing block, FMT-SWITCH-009) | RH7501-001, RH7501-004, FMT-SWITCH-006 | P4 (marked) — correct; no principle decides it | none open | Behavior change of the fix (B) or of the analyzer (C). Recommend B (fix removes the block that becomes empty; withhold when the block carries comments/directives) | No |
| #1305 | Q10 | RH7501 fix | `{⏎ A();⏎⏎ break;⏎ }` → `{⏎ A();⏎ }⏎⏎ break;` (blank line moves with `break`) | Neighbour of `break` changes from `A();` to `}`; result differs from FMT-SWITCH-002 shape; format.sh keeps the blank line | RH7501-004 (condition 1 currently excludes this input), SHARED-BLANK-004 | P14a + P10 (marked "supported, for confirmation") — verified: P14a forbids changing the blank lines between the changed node and its neighbours; keeping one blank line in front of `break` is the closest literal reading; option B would remove a blank line, which P14a forbids; P10 agrees because the formatter keeps it | RH5010 / SHARED-BLANK-004 asks for this blank line inside the block | Clarification: A. RH7501-004's condition 1 widened to cover it | Yes (P14a/P10) — record in the new RH7501 issue as a spec widening; only a confirmation, no maintainer choice needed |

## Groups by shared cause

1. **Verdict depends on incidental placement or order:** #1304 Q1 (unrelated override of another kind), Q2 (base-list order), Q4 (which partial part holds the override), Q6 part choice (which partial part lies in the region). One principle would decide all four (P25 candidate).
2. **Multi-declarator events:** #1304 Q3 — P20, mechanical.
3. **Partial member reported twice:** #1304 Q6 double report — P15, mechanical.
4. **Unsafe / scope-breaking RH7501 fix:** #1305 Q7 (P1 + #1136), Q8 (P1, P9). One bug issue for the RH7501 fix.
5. **RH7501 fix result shape:** #1305 Q9 (empty block), Q10 (blank line moves with `break`).
6. **Region name vs. formatter capitalization:** #1304 Q5 alone.

## Decided by principle → proposed follow-up action

- **New issue (bug): "RH7501 fix is withheld when it would break code, move directives, or rearrange shared lines (P1, P9)"** — #1305 Q7 inputs 1–4, Q8 inputs 1–3. Add the RH7501-004 sentence "outside these conditions the fix is withheld" (or a precise layout for shared lines, squad's choice within P9/P10). Include #1305 Q10 as a spec widening of RH7501-004 condition 1 (blank line moves with the `break`, P14a). If the maintainer chooses Q9 B, add it here as a behavior change (change doc). Blocked by #1138 (SHARED-FIX-006/007 cited); input 4 wording by #1136.
- **Extend #1136** (comment): RH7501 input 4 — a fix that moves one directive of a `#pragma warning disable/restore` pair across a brace reorders the pair; instance of "relocating/reordering directives".
- **New issue (bug): "RH7403, RH7407, RH7411 check every declarator of a field-like event (P20)"** — #1304 Q3 inputs 1–5; covers judging, override-region determination (input 3), and one diagnostic per declarator at its identifier (SHARED-REGGROUP-005 extended). Blocked by #1260. Alternative: extend #1270 and retitle it to "Rules check every element of their kind (P20)"; recommend a new issue because #1270 is the documentation family.
- **Comment on #1260**: SHARED-DIAG-006 should say how many diagnostics a declaration with several declarators gets and where (recommend: one per reported declarator, at its identifier), since #1304 Q3 inputs 4/5 cite it for that.
- **Extend #1287** (P15 instances: RH2002, RH2004, RH2101): add RH7401–RH7412 — a partial member is reported once (#1304 Q6 input 1). If the maintainer decides Q4/Q6 part choice, put those into the same issue (or a new "RH74xx judge partial types and members as one" issue if the scope grows).

## Needs maintainer decision

**M1 — #1304 Q1: does an override of another member kind make a region an override region?**

```csharp
class B { public virtual int P => 0; }
class C : B
{
    #region B
    public override int P => 1;
    public void N() { }
    #endregion
}
```

Today: nothing reported. (a) Any override of any kind from `B` makes `#region B` an override region → RH7405 reports `N` (behavior change, rule pages RH7405–RH7408 change). (b) Only overrides of the rule's own kind count (today). Recommendation: **(a)** — the region holds "B's inherited behavior"; whether `N` belongs there cannot depend on whether a method override exists elsewhere.

**M2 — #1304 Q2: a member implementing two interfaces**

```csharp
interface IA { void M(); }
interface IB { void M(); }
class C : IA, IB
{
    #region IB
    public void M() { }
    #endregion
}
```

Today: RH7409 reports `M` (wants `IA`); with `: IB, IA` nothing. (a) A region named after any interface whose member `M` implements is accepted (nothing reported in either order). (b) Only the first interface in base-list order (today). (c) Report always, demand the first interface regardless (stable but arbitrary). Recommendation: **(a)**.

**M3 — #1304 Q4: override in another part of a partial type**

```csharp
class B { public virtual void M() { } }
partial class C : B { public override void M() { } }
partial class C
{
    #region B
    public void N() { }
    #endregion
}
```

Today: `N` not reported (reported if `M` is in the same part). (a) Any part's override counts → RH7405 reports `N` (P15 read as "the type's override set is the symbol's"). (b) Only the analyzed part (today). Recommendation: **(a)**; it also removes the inconsistency inside SHARED-REGGROUP-006 ("analyzed type declaration" vs "any declaration of the type").

**M4 — #1304 Q5: lower-case type name vs RH7302 / FMT-REGION-001**

```csharp
class myBase { public virtual void M() { } }
class C : myBase
{
    #region myBase
    public override void M() { }
    #endregion
}
```

Today: RH7302 reports `#region myBase`; after formatting (`#region MyBase`) RH7401 reports `M`. (a) Exact ordinal match (today; no clean, formatter-stable layout exists). (b) Also accept the name with its first character upper-cased as FMT-REGION-001 does (`MyBase` accepted, `myBase` still accepted by RH7401 but reported by RH7302). (c) Accept case-insensitively. Recommendation: **(b)** — narrowest change that makes the formatter output clean; (c) would also accept `#region b` which SHARED-REGGROUP-002 deliberately rejects.

**M5 — #1304 Q6: which part of a partial member decides**

```csharp
interface I { void M(); }
partial class C : I
{
    #region Methods
    public partial void M();
    #endregion

    #region I
    public partial void M() { }
    #endregion
}
```

Today: `M` reported once at the defining part; swapped placement → nothing. (a) Both parts must lie in a matching region; reported once, at the first part that does not. (b) Defining part decides (today). (c) Implementing part decides. Recommendation: **(a)** — symmetric, independent of which part is where; the double report (RH7401 case) is P15 regardless.

**M6 — #1305 Q9: fix leaves an empty block**

```csharp
switch (x)
{
    case 1:
        {
            break;
        }
}
```

Today: reported; fix gives `case 1:⏎ {⏎ }⏎ break;`. (a) Keep (empty block). (b) Fix removes the block that becomes empty → `case 1:⏎ break;`; withheld when the block carries a comment or directive. (c) Do not report a `break` that is its block's only statement. Recommendation: **(b)** — the result is the FMT-SWITCH-006 shape; (c) leaves `{ break; }`, which is equally noisy.

**Confirmation only (no options needed): #1305 Q10** — blank line moves with the `break` (A), per P14a/P10; recommend confirming in the same message.

## Principle candidates

- **P25 (candidate): a verdict does not depend on incidental placement or order.** A rule's verdict on an element depends on that element, the group it lies in, and the symbol it belongs to — not on the order of a base list, on which part of a partial type holds a related declaration, or on whether an unrelated member of another kind exists. When an element legitimately belongs to several groups (several interfaces, several parts), any matching group is accepted; for a partial element, all parts are judged together. Decides #1304 Q1, Q2, Q4, Q6 (part choice) — all with recommendation (a). Generalizes P15 (symbol, not part), P18 and P20 (order of what lies between / declarator order). Likely to recur in RH7101–RH7110 ordering and RH7308/RH7309 region rules.
- **P26 (weak candidate, one instance): no unsatisfiable rule pair.** For every input there is a layout that every enabled rule accepts and the formatter keeps; when a matching rule compares a name against text another rule or the formatter canonicalizes, it accepts the canonical form. Decides #1304 Q5 (b). Related to SHARED-FIX-004 (fixes undo each other) and P7; only one instance in this family, so record as a note unless it recurs.
- Observation (not a principle): RH7501-004 states the fix's positive conditions but not "withheld otherwise"; squads write fix requirements this way elsewhere too. Worth a sentence in the new RH7501 issue.
