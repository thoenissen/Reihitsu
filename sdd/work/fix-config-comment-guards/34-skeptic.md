Role: skeptic
Commit: e25a48fe
Verdict: #1595: proceed; #1593: proceed, with spec and change-doc amendments and one output question; #1616: proceed, with a SHARED-FIX-009 wording amendment so that it does not contradict RH2001-009

The prompt carried no intended fix, suspected cause, or hoped-for verdict. It named the spec diff and the change doc as the work, together with the maintainer's batch 3 and batch 4 answers and decision 32.

Method: I read `git diff 57acf0d -- sdd/specs sdd/changes`, `33-spec-author.md`, the findings tables of `09/15/22/29-auditor.md`, and `OrderingDeclarationUtilities` on `main` (`GetMembersOfDisabledText`, `ChangesMemberOrder`, `IsLayoutField`, `HasBackingField`). I measured `main` with `scripts/apply-fix.sh` on a temporary worktree at `57acf0d`, and HEAD in the repository. LF and CRLF gave the same status everywhere. The fixtures were outside the repository and have been deleted.

## thoenissen/Reihitsu#1595

### Premise
Settled in earlier stages (decisions 3–5, 12, 16). This stage changes RH7102-006 only by adding SHARED-ORDER-008. The new RH7102-006 text matches RH7102-001: a reported constant holds no conditional directive outside its leading trivia, so the only directives the fix still reads are groups of whole members in front of the constant. Those are counted under SHARED-ORDER-007, and the three RH7102 examples follow from that. Wording nit: "SHARED-ORDER-008 refuses a move for a conditional directive anywhere else in its declaration" adds nothing, because RH7102-001 already leaves out every constant whose declaration contains a conditional directive. Harmless.

### Mechanism and candidates
_N/A._ (spec stage)

### Breaking inputs
Covered under #1593: the guard is shared.

### Surfaces
| Surface | Must change / must stay / not involved |
|---|---|
| RH7102 analyzer | must stay (decision 16) |
| RH7102 code fix | must change: SHARED-ORDER-008 through the shared guard |
| Rule page | not involved |

### Simpler option
_None._

### Output questions
_None._

### Internal choices
_None._

### Issue goals
No new goal changes. The goals were already adjusted under decisions 3–5.

### Architecture findings
_None._

### Follow-up candidates
_None new._

## thoenissen/Reihitsu#1593

### Premise
The rule can be decided without evaluating anything. For each conditional group in the moved or crossed span, the fix answers four questions:

1. Is the group nested, or does it hold a nested group? If so, refuse.
2. Do all of its directives stand at member boundaries of one type body? Then each excluded branch is parsed on its own as members, and the fix checks that the branch ends every declaration it begins and closes no brace it did not open.
3. Do all of its directives stand inside one body block, where the branches hold whole statements and no accessor branch contains `field`? Then keep the move.
4. Otherwise, refuse.

No step reads a condition or combines branches.

I checked every failure class of the four audits against SHARED-ORDER-008 and SHARED-ORDER-007. Each member is either refused or kept on main's whole-member count:

| Audit finding | Members | Outcome under the new spec |
|---|---|---|
| #1 | a1 (`#endif` in front of `int`) | refused: a directive between the tokens of a member |
| #1 | a2 (`#if DEBUG static #endif` wholly in front of `private int _b = …`) | refused: the excluded text `static` is not a whole member declaration |
| #2 | o1, o2 (excluded attribute) | refused. Main fixes these, so this is a visible change; see *Must list* below |
| #3 | x9, x4 (the previous member's initializer or `;` split) | refused: a directive inside a declaration |
| #4 | x1, x1b, x3, h4, h5 | refused: a split modifier list, or a directive in an initializer |
| #6 | b2, b1s (brackets), arr1 (expression body) | refused |
| #6 | blk1, nt1 | refused: the excluded branch closes a brace it did not open |
| #7 | j1s, j2s (same name in branches that exclude each other) | each branch is a whole member and counts as present, so main's fix is kept. Correct |
| #8, #12 | the configuration limit | gone |
| #9 | tc1 (branch closes `C` and opens `D`) | refused |
| #9 | nt2 (branch opens `N`) | refused: the branch begins a declaration it does not end |
| #10 | sg1, sg1i, cr1 | refused: a split member |
| #11 | acc1 | refused: a directive in an accessor list |

SHARED-FIX-003 holds: every refusal is on the safe side, and every kept move counts only whole members as present, which is main's reading.

### Mechanism and candidates
_N/A._ (spec stage)

The dispatch the guard covers: `TypeMemberOrderingCodeFixProviderBase` (RH7102, RH7103, RH7109) and the RH7309 region reorder, both through `OrderingDeclarationUtilities.ChangesMemberOrder`. The groups that can appear in the spans are:

- groups in a member's first-token trivia or the closing brace's trivia (type body);
- groups between the tokens of a member declaration (attributes, modifiers, initializer, expression body, parameter list, accessor list);
- groups in a body block, a nested block, or a switch section;
- groups in a nested type's body;
- groups in a lambda or anonymous-method block inside an initializer.

### Breaking inputs
FC-001, FC-016, FC-020 and FC-027 are closed as the table above shows.

The spec text has the following gaps. Each one leaves a visible output that the spec does not state, or states against the maintainer's answers. The `main` results come from `apply-fix.sh` at `57acf0d`.

**Spec contradicts batch 4: a switch section.** A group of whole statements inside a `case` section of a crossed method (RH7103, `switch_section`) is **fixed** on main. SHARED-ORDER-008 requires "whole statements of that block … directly or in a block nested in it", and a switch section is not a block, so the strict reading refuses. Batch 4 keeps "a group of whole statements inside a member body", and a statement of a `case` section is one. Required amendment: define the group over the statements of one *statement list*, meaning a block or a switch section.

**Must list in the change doc: fixed on main, withheld under the spec, decided by batch 3 A.** Each of these is "an `#if` inside a member" and "not a group of whole members", so the answer covers it. The change doc's *Visible output changes* names none of these classes:

- `attr_crossed` (RH7109): a conditional attribute list in front of a crossed member. The group stands in the member's first-token trivia, and its excluded text `[System.Obsolete]` is not a whole member.

  ```csharp
  private int _a;
  #if DEBUG
      [System.Obsolete]
  #endif
      private int _b;
      private readonly int _c;
  ```

- `attr_moved` (RH7109): the same group in front of the moved `_c`. This is the most common real-world shape of the list, for example `#if NET7_0_OR_GREATER [SetsRequiredMembers] #endif`. Withholding is consistent with SHARED-FIX-009, because keeping the move would mean assembling the attribute and the active member into one declaration. SHARED-ORDER-008 should get an example for it.
- `accessor_list` (RH7103, class): `get; #if DEBUG set; #endif` on a crossed property.
- `arg_in_body` (RH7103): a conditional argument inside a statement of a crossed method body. The group is not whole statements.
- `lambda_init` (RH7109): a group of whole statements in a lambda block inside a crossed field initializer. The group is in an initializer, not in a member's body block.

**Spec example with a misleading reason (the `field` case).** The SHARED-ORDER-008 and change-doc example (RH7103, struct `S`, `Shared` moved across `Value`) moves a *static* member. No instance field changes order in any build, so main's fix is correct there. The spec's reason ("the fix does not work out which builds give it one") implies a risk that this input does not have. The withholding itself is decided by batch 4 ("except an accessor branch using `field`").

The `field` clause does matter for RH7309. Fixture `field_region` is **fixed** on main:

```csharp
public struct S : System.IComparable
{
    #region IComparable

    public int Value
    {
        get
        {
#if TRACE
            return field;
#else
            return 0;
#endif
        }
    }

    public int CompareTo(object obj)
    {
        return 0;
    }

    #endregion // IComparable

    #region Fields

    private int _x;

    #endregion // Fields
}
```

The fix moves `Fields` in front of `IComparable`, so a `TRACE` build lays out `_x` before `Value`'s backing field. Main's `HasBackingField` reads only active tokens, so it misses this. That is a SHARED-FIX-003 bug that the clause fixes, so it does not go in the change doc's list.

Recommendation: keep the RH7103 example, reworded like SHARED-FIX-009's example ("although no build would change here"). Add the RH7309 input above as the example the clause protects.

**Wording errors in SHARED-ORDER-008.**

- In the #1593 `X`/`Y` example, without `DEBUG` the `#else` stands in the leading trivia of `Y`'s first token. Only the `#endif` stands inside the declaration. Suggested text: "the `#endif` stands inside the declaration of `Y`, and the excluded branch begins a declaration that it does not end".
- The first example is headed "in the crossed span", but its group is the moved member's leading trivia: the text below it says so.

Holds (unchanged against main): j1s and j2s; the SHARED-ORDER-006 examples; RH7309-005's second example; `#if false` with whole members in the moved trivia.

### Surfaces
| Surface | Must change / must stay / not involved |
|---|---|
| `OrderingDeclarationUtilities` (Core) | must change: back to main's whole-member reading plus the SHARED-ORDER-008 gate; the per-configuration machinery goes |
| `TypeMemberOrderingCodeFixProviderBase` | must stay (decision 4) |
| RH7309 code fix | must change through the shared guard (spans as SHARED-ORDER-008 defines them) |
| RH7102/RH7103 analyzers | must stay (decision 16) |
| RH7109/RH7309 analyzers | not involved |
| Formatter | not involved |
| Rule pages | not involved: these are withheld fixes in corner cases, which `general-notes.md` covers |
| Tests | must change: the decision 21–31 rows that expect main's fix on a split member now expect no fix; the limit tests go; add the amendments' examples |

### Simpler option
None that is simpler than the spec. Two places where the spec is stricter than the answers need clarifying, not simplifying: the switch section above, and the nested type under *Output questions*.

### Output questions
**1 · RH7109 · Readonly elements must appear before non-readonly elements — an `#if` group of whole members inside a nested type that the move crosses**

Input:
```csharp
public class C
{
    private int _a;

    private class N
    {
#if DEBUG
        private int _d;
#endif
    }

    private readonly int _c;
}
```

Before (`main`):
```csharp
public class C
{
    private readonly int _c;
    private int _a;

    private class N
    {
#if DEBUG
        private int _d;
#endif
    }
}
```

After, A (recommended): same as `main`.

After, B: no fix offered; the diagnostic stays.

Why A: the members of a nested type take no part in the move (SHARED-ORDER-001), and a group of whole members is the shape the batch 3 answer keeps. SHARED-ORDER-008's wording "between two members of the type" reads as B. If A is chosen, SHARED-ORDER-008 says that the directives of a group of whole members stand at member boundaries of one type body, nested type bodies included.

The standing answer in `00-lead.md` applies.

### Internal choices
- **SHARED-FIX-009, "never offer it where it would be withheld otherwise".** Read literally, this contradicts SHARED-ORDER-008, whose whole-member check reads excluded text in order to keep a move, and SHARED-FIX-003's RH4106 example ("does not name `Value`, so the fix is offered"). Recommendation: "the reading can only withhold: what an excluded branch holds never makes the fix offer a change that it would not offer without that branch". The meaning stays the same and nothing visible changes.
- **SHARED-ORDER-008, empty or comment-only branches.** The spec should say whether an excluded or active branch that holds no declaration, only comments or whitespace, is a group of whole members. Recommendation: yes. Main fixes it; the `#else // nothing in release` variant was **fixed** on main.
- **SHARED-ORDER-008, other directives inside the branch text.** The spec should say whether `#region`/`#endregion` or other directives inside an excluded branch's text make it "not whole members". Recommendation: they do not (`region_in_branch` is **fixed** on main), so nothing visible changes. Moving an excluded `#pragma` is a pre-existing case; see *Follow-up candidates*.
- **SHARED-ORDER-008, RH7309 spans.** The spans cover only the blocks whose place changes, while RH7309-005's first sentence inspects every block and the code between blocks. That difference is harmless, because 005 is the wider guard. No change needed.

### Issue goals
The issue asked for its own example (the `X`/`Y` region block) to be judged correctly. Under batch 3 A, it is withheld instead, and the change doc lists it. The defect class the issue reports, a split member miscounted, can no longer occur, because no split member is counted. So the issue's purpose is met; `Closes` is the lead's call. The changed outcome of the issue's own example is visible and is covered by batch 3 A.

### Architecture findings
_None._ The new rule removes the prediction that audits 2–4 found. No member is read in parts.

### Follow-up candidates
- A `#pragma`, `#nullable`, or other position-sensitive directive inside an excluded branch moves with the members in the build that compiles it. SHARED-ORDER-005 sees only active trivia. This is pre-existing on main; check reach before filing.
- Unchanged from `00-lead.md`: the conditional `[StructLayout]` on the type header, which lies outside both spans.

## thoenissen/Reihitsu#1616

### Premise
RH2001 is not edited by this spec diff. SHARED-FIX-009 is a requirement for every fix, though, and as worded it contradicts RH2001-009, which this run wrote. RH2001-009 says that "a comment or directive between two modifiers that keep their order … does not withhold the fix". SHARED-FIX-009's second bullet says that "when the fix's safety depends on what an excluded branch makes of code outside it — a modifier … — the fix is withheld".

Measured on fixture `if_static` (RH2001); main and HEAD both **fixed**:

```csharp
public class C
{
    private
#if DEBUG
        static
#endif
        int Value { get; }

    public int Get()
    {
        return Value;
    }
}
```

The output on HEAD (LF):

```csharp
    private
#if DEBUG
        static
#endif
    readonly int _value;
```

The same output, with `extern` in the branch, is **fixed** too. I did not compile the `DEBUG` build of that output, so I make no claim about whether it compiles.

SHARED-DIAG-008 already lets a fix that "leaves the directive and its line in place" stay offered. So the two shared specs now disagree as well, and SHARED-FIX-009 would demand an unaudited change in every fix that keeps a directive in place.

### Mechanism and candidates
_N/A._

### Breaking inputs
`if_static` (above): reached by the RH2001 code fix on a private auto-property whose modifier list holds an `#if` group.

### Surfaces
| Surface | Must change / must stay / not involved |
|---|---|
| RH2001 analyzer, code fix, spec | must stay |
| `sdd/specs/shared/code-fixes.md` SHARED-FIX-009 | must change (wording, below) |

### Simpler option
Scope SHARED-FIX-009's second bullet to a fix that *reads* an excluded branch. Such a fix withholds where its safety would depend on what the branch makes of code outside it. A fix that reads no excluded code and leaves the directive in place stays governed by SHARED-FIX-003 and SHARED-DIAG-008. This keeps RH2001-009 and main's output, puts nothing visible in question, and leaves the member-move rule exactly as it is, because SHARED-ORDER-008 states it on its own.

The alternative is to extend the principle to RH2001: withhold for a conditional directive in the modifier list. That changes `if_static`, which is fixed on main, and would be an output question. Recommendation: the scoped wording.

### Output questions
_None_ if the scoped wording is taken.

### Internal choices
The scoped wording above. Recommendation: take it.

### Issue goals
_N/A._ Unchanged.

### Architecture findings
_None._ This is one principle stated twice (SHARED-FIX-009's second bullet and SHARED-DIAG-008's "leaves the directive in place" exception). The scoped wording removes the overlap.

### Follow-up candidates
- Add to the existing RH2001 follow-up (an `#if` between the property's modifiers, SHARED-DIAG-008): whether RH2001's fix, offered when a conditional group adds a modifier such as `extern` in another build, keeps that build compiling. Unverified; compile the `DEBUG` variant before filing.
- From `33-spec-author.md`: fixes other than the member-moving ones have not been checked against SHARED-FIX-009.

## Whole run
- The change doc's *A balanced group inside a member's body keeps the fix (pending the lead's confirmation)* is now decided by the batch 4 answer. Cite that answer, and cite it for the `field` exception too, instead of "follows from the next decision".
- The amendments for the spec author: the statement-list definition (#1593), the *Must list* additions to the change doc, the `field` example rework and the RH7309 example, the two example wordings, the SHARED-FIX-009 monotonicity wording, the empty-branch and directive-in-branch statements, and SHARED-FIX-009's second-bullet scope (#1616). After those, one output question remains: the nested type (#1593, question 1).
