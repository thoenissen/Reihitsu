Role: skeptic
Commit: b2fb1a8
Verdict: proceed narrower, plus one output question (RH7201–RH7207, a comment directly above the block's first directive with no preprocessor directive in front)

My prompt carried no intended fix or suspected cause. It did carry a scope instruction (make the audit concrete), which this handover follows.

## Premise

The issue was derived from reading code, not from running it. I checked it with `scripts/apply-fix.sh <ID> <dir>` on `b2fb1a8`. Every run below was clean under LF and CRLF alike: no `invalid-result`, no line-ending drift. A run compiles a fix result only in the build the fixture is analyzed in, with no conditional symbol defined. So "does not compile in the other build" below is read off the output; no compiler reported it.

The premise is **real for two fix families**, **open for a third** (an output question), and **holds for the rest of the scope** (table in *Breaking inputs*). Every failing input is reachable: the analyzer reports it, and the IDE code-fix caller and Fix All apply the fix to the user's document. No node-level shortcut is needed.

1. **Expression body → block: RH3202, RH3203, RH3205, RH3206, RH3207, RH3208.** These fixes split a `#pragma warning` pair or a `#nullable` pair across the new braces. `ExpressionBodyRewriteUtilities.BlocksRewrite` (`src/Reihitsu.Core/ExpressionBodyRewriteUtilities.cs:36`) refuses only unbalanced conditional directives and region directives (FC-005). All six rules failed on fixture A1 and were fixed with 1 action.
2. **Statement braces: RH5406, RH5407.** These fixes split every directive pair whose one directive stands between two lines of the body and whose partner stands outside the body: `#pragma warning`, `#nullable`, a conditional group, and a region pair. `StatementBracesCodeFixProviderBase.TryCreateChanges` (`src/Reihitsu.Analyzer.CodeFixes/Base/StatementBracesCodeFixProviderBase.cs`) checks directives only in the gap between the header and the body. The conditional case leaves the inserted `}` in only one branch, so the other build loses a brace (SHARED-FIX-003, first condition and *Build configurations*).
3. **Using ordering: RH7201–RH7207, open.** A comment that starts its line directly above the block's first using directive, with no blank line between them, stands with that directive (SHARED-FIX-003, *Comments and directives in place*). When the reorder moves that directive away from the top, the comment stays at the top (FMT-USING-009), and the fixes take that output.
   - **With a preprocessor directive in front (D2): decided, not a defect.** The maintainer answered on 2026-11-28 (`sdd/changes/2026-11/using-header-directive-reorder.md`, *Decisions*, case 5) that a comment directly below a header directive stays on top as part of the header, for the fixes and the formatter alike. `CommentDirectlyBelowAHeaderDirectiveStaysAtTheTop` pins it.
   - **Without a directive (D1): open.** The fixes do the same, and existing tests assert it. The literal SHARED-FIX-003 reading would withhold the fix, so this is an output question (below).

The issue's list is overbroad in one place. **RH3204** (removing the `$` from an interpolated string that has no holes) adds no braces and moves no code, so it is out of scope.

## Mechanism and candidates

Mechanism: **a fix that adds braces checks directives only for some kinds or only in some places. A directive inside the span the new braces enclose whose partner lies outside that span ends up split by one brace**: a `#pragma warning` or `#nullable` paired with its restore, a conditional group, or a region pair.

Dispatch: I took the candidates from the code-fix providers (233 `*CodeFixProvider.cs`), not from the issue text. I searched them for brace creation and removal (`SyntaxFactory.Block`, `WithBody`, `AccessorList`, `OpenBraceToken`), for moves (`InsertNodes*`, `WithMembers`, `WithStatements`, `WithAttributeLists`, `WithUsings`), and for their bases. In scope:

| Family | Providers | Defining type |
|---|---|---|
| Expression body → block (adds braces) | RH3202, RH3203, RH3205, RH3206, RH3207, RH3208 | `ExpressionBodyToBlockCodeFixProviderBase<TNode>` → formatter transforms, guard `ExpressionBodyRewriteUtilities.BlocksRewrite` |
| Statement braces (adds braces) | RH5405, RH5406, RH5407 | `StatementBracesCodeFixProviderBase` |
| Using declaration → statement (adds braces) | RH7004, already audited | its provider |
| Block → expression body (removes braces) | RH3209, RH3210 | `GetOnlyMemberExpressionBodyCodeFixProviderBase`, guard `GetOnlyMemberExpressionBodyRewriteUtilities.ContainsBlockingDirective` |
| Empty type → `;` (removes braces) | RH5412–RH5416, RH5418 | `EmptyTypeDeclarationShouldUseSemicolonCodeFixProviderBase` (SHARED-EMPTY-004) |
| Using reorder (moves directives) | RH7201–RH7207 | `UsingDirectiveOrderingCodeFixProviderBase`, `UsingDirectiveOrderingSafety.CanSafelyReorder`, formatter `UsingDirectiveOrderingPhase` |
| Region removal | RH7310 | its provider |
| Member and accessor reorder | RH7102, RH7103, RH7107, RH7108 (RH7109 already audited) | `TypeMemberOrderingCodeFixProviderBase`, `AccessorOrderingUtilities`, `OrderingMoveSafety` (SHARED-ORDER-006/011) |
| Attribute placement and list shape (moves and splits lists) | RH5501–RH5531 | `TargetAttributePlacementCodeFixProviderBase`, `TargetAttributeListShapeCodeFixProviderBase` (refuse on `InteriorContainsCommentOrDirective`, `WouldJoinAcrossUnjoinableTrivia`) |
| Rewrites in place that the maintainer named | RH2001 (thoenissen/Reihitsu#1990) | its provider (RH2001-007/009) |

Out of scope, with the reason:

- **Pure layout fixes:** RH5001–RH5032, RH5101–RH5116, RH5201–RH5206, RH5301–RH5308, RH5402–RH5404, RH5408–RH5411, RH5417, RH5419, RH5401, RH5601–RH5604, RH6001–RH6024. They change whitespace, line breaks, and blank lines between tokens. They add or remove no brace pair and move no code past other code, and a directive always holds a line of its own, so a line-break change cannot carry code across one.
- **Renames:** RH4xxx. They move nothing.
- **Documentation fixes:** RH8xxx. RH8309 reorders XML elements inside one comment, where no directive can stand.
- **Token rewrites in place:** RH3001–RH3007, RH3101–RH3107, RH3201, RH3204. RH3005 swaps operands, but only where `InteriorContainsCommentOrDirective` is false. RH3004 refuses comments and directives in the gaps it rewrites.
- **Already audited by the earlier change** (`sdd/changes/2026-10/withhold-unsafe-fixes.md`): RH7004, RH7101, RH7105, RH7106, RH7109, RH7303, RH7304, RH7306, RH7309, RH7501. RH7004 is spot-checked below because it belongs to the brace-adding class.

## Breaking inputs

Failure classes that reach the code: FC-004 (a move relocates a directive), FC-005 (a guard for one directive kind taken to cover all kinds), FC-020 (directives in the gap the edit touches), FC-021 (comments on the rewritten header). FC-044 limits the fix: pairing `#pragma warning disable` with *its* `restore` by warning IDs would model what the compiler computes. Refuse by directive kind instead.

### Class matrix: a brace-adding fix and a directive pair

Axes: the fix family; the directive kind; the side the partner stands on (behind the new `}`, or in front of the new `{`); and whether the directive is paired at all (a lone `#pragma warning disable` acts until the end of the file).

| Family | `#pragma warning` pair | `#nullable` pair | Conditional group | Region pair | `#line` pair | Lone `#pragma warning disable` inside |
|---|---|---|---|---|---|---|
| RH3202–RH3208 | **fails** on both sides (A1, A2) | **fails** (A3) | holds: not reported (SHARED-BODY-001, FMT-STRUCT-007) | holds: not reported or refused (FMT-UNSUP-023) | no diagnostic (A5) | fixed. Not a pair, so not a split; pinned as fixed by `VerifyExpressionBodyWithDirectiveAfterExpressionIsFixed` in each of the six rule test classes |
| RH5406, RH5407 | **fails** on both sides (B1, B2) | **fails** (B3) | **fails**: the other build does not compile (B4) | **fails** (B5) | no diagnostic (B6) | fixed. Not a pair; pinned by RH5406 `FixIsOfferedForADirectiveBetweenTwoLinesOfABodyBelowItsIf` |
| RH5405 | unreachable: RH5405 reports only a body that ends on its first line (multi-line body fixtures: `no-diagnostic`), and a directive inside a body needs two lines. A directive between the header and the body is withheld already (SHARED-BRACE-003) | same | same | same | same | same |
| RH7004 | holds: no fix offered (C1) | n/a | covered by RH7004-004 | — | — | holds: no fix offered (C2) |

Balanced pair wholly inside the body (B7): holds, fixed. Comments around the statement-brace fix (B8): hold, every comment keeps its code.

#### Fixtures and observed output

Each class below holds one `[Obsolete] static int Old() { return 1; }` plus the helper named in the fixture.

**A1. RH3202, pragma pair split at the end.** Same shape run with RH3203 (`C() => _x = Foo(1, …`), RH3205 (`operator +`), RH3206 (`implicit operator int`), RH3207 (`~C() => Bar(1, …`), and RH3208 (a local function inside `int N()`, with `return L();` behind it). All six were fixed with 1 action.

```csharp
using System;

class C
{
    [Obsolete]
    int Old() { return 1; }

    int Foo(int a, int b) { return a + b; }

    int M() => Foo(1,
#pragma warning disable CS0612
        Old());
#pragma warning restore CS0612
}
```

Observed output: `fixed (1 action(s), 1 iteration(s))`. The new `}` lands between `disable` and `restore`:

```csharp
    int M()
    {
        return Foo(1,
#pragma warning disable CS0612
                   Old());
    }
#pragma warning restore CS0612
```

**A2. RH3202, split at the start.** `#pragma warning disable CS0612` stands above `int M() => Foo(Old(),`, and `#pragma warning restore CS0612` stands between that line and `1);`. Observed output: fixed. The new `{` lands between the two directives.

**A3. RH3202, `#nullable` split at the end.**

```csharp
string M() => Foo("a",
#nullable disable
        null);
#nullable restore
```

Observed output: fixed. `}` lands between `#nullable disable` and `#nullable restore`.

**A4 (controls, RH3202).** All hold.

| Input | Observed |
|---|---|
| Unbalanced `#if X` / `#else` / `#endif` where `;` ends in a branch | `no-diagnostic` |
| Balanced `#if` / `#else` / `#endif` inside the argument list | fixed, braces outside the group |
| `#pragma` pair around the whole member | fixed, braces inside the pair |
| `int M() // the answer` ⏎ `=> 1;` | `no-fix-offered` (FMT-UNSUP-013) |
| `int M() => 1; // one` | fixed, `// one` behind the new `}`. The code is rewritten in place and the comment stays behind it, with no other code between (FMT-STRUCT-006). Same for RH3208 |

**A5. RH3202, `#line hidden` / `#line default` split.** Observed output: `no-diagnostic`. A `#line N` pair was not tried.

**B1. RH5406, pragma pair split at the end.** RH5407 shows the same with an `else` body whose `if` body is braced (fixed).

```csharp
using System;

class C
{
    [Obsolete]
    static int Old() { return 1; }

    static void Bar(int a, int b) { }

    void M(bool a)
    {
        if (a)
            Bar(1,
#pragma warning disable CS0612
                Old());
#pragma warning restore CS0612
    }
}
```

Observed output: fixed. `{` lands below `if (a)`, and `}` lands between `disable` and `restore`.

**B2. RH5406, split at the start.** `#pragma warning disable CS0612` stands above `if (a)`, and `#pragma warning restore CS0612` stands between `Bar(Old(),` and `1);`. Observed output: fixed. `{` lands between the two directives.

**B3. RH5406, `#nullable` split.** `Bar("a",` ⏎ `#nullable disable` ⏎ `null);` ⏎ `#nullable restore`. Observed output: fixed, split.

**B4. RH5406, conditional group that ends behind the body.** RH5407 shows the same on the `else` body (fixed). This is the most severe member.

```csharp
class C
{
    static void Bar(int a, int b) { }

    void M(bool a)
    {
        if (a)
            Bar(1,
#if !X
                2);
#else
                3);
#endif
    }
}
```

Observed output: fixed. A build that defines `X` loses the `}`:

```csharp
        if (a)
        {
            Bar(1,
#if !X
                2);
        }
#else
                3);
#endif
```

**B5. RH5406, region pair.** `Bar(1,` ⏎ `#region R` ⏎ `2);` ⏎ `#endregion`. Observed output: fixed, and `}` lands between `#region` and `#endregion`. RH7303 also reports this region, and the formatter refuses the file (FMT-UNSUP-023). The statement-brace fix edits text without the formatter, so it does not see that refusal.

**B6. RH5406, `#line hidden` / `#line default`.** Observed output: `no-diagnostic`.

**B7. RH5406, balanced pair inside the body.** Observed output: fixed, braces outside the pair. Holds.

**B8. RH5406, comments.** All of them stay with their code. Holds.

```csharp
        if (a) // header
            // above body
            Bar(1,
                2); // behind body
        // below body
        Bar(3, 4);
```

Observed output: `{` lands below the header line, and `}` lands above `// below body`.

**C1, C2. RH7004.** C1 places a pragma pair split inside the following statement; C2 places a lone pragma pair around the following statement. Both: `no-fix-offered`. Holds.

### Class matrix: using reorder and comments or directives

| Input (RH7203 unless named) | Observed | Verdict |
|---|---|---|
| D1: `// about Linq` ⏎ `using System.Linq;` ⏎ `using System;` | `// about Linq` ⏎ `using System;` ⏎ `using System.Linq;` | **fails** placement: the comment now stands with `System`. Same for RH7201 and RH7207 with `// about Zeta` ⏎ `using Zeta;` ⏎ `using System;`. Pinned as fixed by `CodeFixKeepsNamespaceIndentationBelowAHeader` in RH7201, RH7202, RH7204, RH7205, RH7206, RH7207 |
| D2: `#nullable enable` ⏎ `// about Linq` ⏎ `using System.Linq;` ⏎ `using System;` | comment and directive stay at the top, `System` moves up | holds by the maintainer's answer (case 5 of `using-header-directive-reorder.md`). Pinned by RH7203 `CommentDirectlyBelowAHeaderDirectiveStaysAtTheTop` |
| D3: `// Header` ⏎ blank ⏎ `// about Linq` ⏎ `using System.Linq;` ⏎ `using System;` | header stays, the comment moves with `Linq` | holds |
| D4: trailing `// linq` and `// system` | each travels with its directive | holds |
| D5: a comment above the second directive | travels with it | holds |
| D6: `#pragma warning disable` in front of the block, `restore` behind it | both stay | holds |
| Directive in front of a later directive | withheld (SHARED-USING-009, existing tests) | holds |

### Other members, all holding

- **RH7310.** Holds for a comment above the region with a blank line below the `#endregion`, for a trailing comment on the member above, and for a `#pragma` pair around the region. The analyzer reports no region that holds a comment or directive (RH7310-001/002), and removing the `#endregion` is the stated exception. One borderline member is under *Internal choices*: a comment directly above the `#region` with the next member directly below the `#endregion`. After the fix, the comment stands with that member.
- **RH3209, RH3210.** Removing braces is not a split under the definition, which covers moving a directive across a brace or adding braces. A `#pragma` inside `return` whose restore is behind the property gave `int P => Foo(1, #pragma… Old());`, and every directive governs the same code. A comment behind `return _x;` stays behind `=> _x;`.
- **RH5412.** A `#pragma` in the body, and an `#if` pair split by `}`: `no-fix-offered` (SHARED-EMPTY-004).
- **RH7102, RH7107.** A pragma pair or a lone disable in the move range: `no-fix-offered`. Comments above a moved const or accessor travel with it. RH7103 and RH7108 share `OrderingMoveSafety`, and their tests hold 110 and 8 `#pragma` lines.
- **RH2001.** `// the value` above the property and `// trailing` behind it stay. In `new { /* c */ Value }` the comment stays, giving `new { /* c */ Value = _value }`. A pragma pair around a rewritten reference stays.
- **Attribute fixes RH5501–RH5531.** Refused by an interior comment or directive and by the gap guard. Not run.

### What the test implementer must cover (Coverage first)

**Expression-body fixes.** I found no existing test for a pragma or `#nullable` pair split.

- One reproduction per rule for A1: RH3202, RH3203, RH3205, RH3206, RH3207, RH3208.
- On RH3202 also A2 and A3, since the family shares one base and the other directions need only one rule.
- Expected: the diagnostic stays and no fix is offered.
- The six `VerifyExpressionBodyWithDirectiveAfterExpressionIsFixed` tests stay as they are, or flip under option B of *Internal choices*.

**Statement braces.** I found no existing test for these.

- On RH5406: B1, B2, B3, B4, B5.
- On RH5407: B1 and B4 in their `else` form.
- Expected: no fix offered.
- B7 and B8 are already pinned in spirit by RH5406 `FixIsOfferedForADirectiveBetweenTwoLinesOfABodyBelowItsIf` and the SHARED-BRACE-002 comment tests. No new test for them.

**RH5405.** No test: no caller reaches the class.

**RH7201–RH7207.** Depends on the answer to the output question. Under B, no test changes: the `CodeFixKeepsNamespaceIndentationBelowAHeader` tests already pin D1. Under A, those six tests become no-fix tests, and RH7203 gets one no-fix test for D1. D2 stays as pinned in either case.

**RH7310.** I found no test for a comment directly above an empty region, nor for a `#pragma` pair around one; the existing tests cover a pragma *inside* a region, which is not reported.

- Add characterization tests for both. They satisfy the issue's "Done when" for this rule.
- Add the borderline member of *Internal choices* according to the lead's decision.

**RH3209, RH3210, RH5412–RH5418, RH7102, RH7103, RH7107, RH7108, RH2001, attribute fixes.** No new tests: existing tests pin the comment and directive refusals (the directive and comment counts in their test classes, plus the guards named above). Name them in the PR as audited.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Code fixes RH3202, RH3203, RH3205–RH3208 (fix side only) | Must change: withhold for a `#pragma warning` or `#nullable` directive inside the converted span whose partner lies outside it |
| `ExpressionBodyRewriteUtilities.BlocksRewrite` (Core) | **Must stay.** The analyzers' reporting (SHARED-BODY-001) and the formatter's conversion (FMT-STRUCT-007: "another directive … MUST NOT block the conversion") both read it. Changing it would stop the diagnostics and change CLI output, which are visible changes no requirement demands |
| `StatementBracesCodeFixProviderBase` (RH5405–RH5407) | Must change: withhold for a directive inside the enclosed body whose partner lies outside it |
| Spec SHARED-BRACE-003 | Must change (spec author, clarification). Its sentence "a directive between two lines of a body below its header do[es] not withhold the fix" contradicts SHARED-FIX-003 for B1–B5 and has to be narrowed to directives whose effect does not cross a new brace |
| Spec SHARED-BODY-001 or 003 | Should state the new withheld case (spec author). Today SHARED-FIX-003 alone demands it |
| RH7201–RH7207 fixes | Must stay under B, the recommendation. Under A, they withhold for D1. Under B, SHARED-FIX-003 gets a clarifying sentence |
| Formatter `UsingDirectiveOrderingPhase` (FMT-USING-009/012) | Must stay. The formatter keeps the header, and P11 allows the formatter more than a fix |
| Formatter `ControlFlowBraceTransform` | Not involved in this issue. Defect found, see *Follow-up candidates* |
| Analyzers | Must stay: every case stays reported |
| Rule pages | Not involved. A diagnostic without a fix is covered by `general-notes.md` |
| RH7310, RH3209/RH3210, RH5412–RH5418, ordering fixes, RH2001, attribute fixes | Must stay. Characterization tests only (RH7310) |

## Refuse option

The requested change *is* the refusal: withhold the fix for the class and keep the diagnostic. The pragma, `#nullable`, conditional, and region members are rare. A directive between two lines of one statement or expression, paired across the statement's end, is something few people write, and RH7303 already reports the region member.

- **Statement braces.** About one guard of a few lines. Core already has `SyntaxTriviaUtilities.ContainsUnbalancedConditionalDirectives` and `ContainsRegionDirectives`, and adding a directive-kind check over the enclosed span is cheap.
- **Expression bodies.** One override of `CanRegisterCodeFix` in the shared base.
- **No new type, and no modelling of pragma pairing (FC-044).**

**Declaring a shape unsupported** is an option for the statement-brace and conditional members. Shape: "a conditional directive or region directive between two tokens of an `if` or `else` body that has no braces, whose partner lies behind the body's last token". It would delete no special case: the formatter's `ControlFlowBraceTransform` has no guard for it (that is the follow-up defect), and the fix guard is new. So it fails the rule that "a shape that deletes nothing is not declared". **Not recommended.** I name it as AGENTS.md requires, for the lead to decide whether escalation 10 applies.

Other simpler options: none.

## Output questions

**1 · RH7201–RH7207 · using-directive ordering fixes: a comment directly above the block's first directive**

The same holds for RH7201 (System first), RH7202, RH7204, RH7205, RH7206, and RH7207 whenever the reorder moves the block's first directive away from the top.

Input:
```csharp
// about Linq
using System.Linq;
using System;

class C { }
```

Before (`main`, RH7203 fix):
```csharp
// about Linq
using System;
using System.Linq;

class C { }
```

After, A: no fix offered; the diagnostic stays (the input is unchanged).

After, B (recommended): as today (the comment stays at the top as a file header).

After, C:
```csharp
using System;
// about Linq
using System.Linq;

class C { }
```

Why B: on 2026-11-28 the maintainer decided the same case with a `#nullable enable` in front of the comment: the comment stays on top as part of the header, for the fixes and the formatter. B keeps both kinds of block consistent and matches FMT-USING-009, which calls such a comment most often a file header.

What B costs: SHARED-FIX-003 needs a sentence (spec author, clarification) saying that the comments FMT-USING-009 keeps at the top of a reordered using block are the block's header and stay there.

What would change the pick: A follows the literal SHARED-FIX-003 text, but it withholds the fix for a file whose header has no blank line below it, which may be common. C makes the fix disagree with the formatter's output.

## Internal choices

1. **How precise the new guards are.**
   - **A:** withhold when the span the new braces enclose holds a `#pragma warning`, `#nullable`, or `#line` directive while a directive of the same kind stands outside that span in the document. For statement braces, also withhold for an unbalanced conditional or region directive, using the existing Core helpers. A lone `#pragma warning disable` with no other `#pragma warning` in the file keeps the fix, so the seven existing "lone disable" tests stay. The six `VerifyExpressionBodyWithDirectiveAfterExpressionIsFixed` tests and RH5406 `FixIsOfferedForADirectiveBetweenTwoLinesOfABodyBelowItsIf` use `CS0618`/`CS0168` disables with no restore in the snippet.
   - **B:** withhold for any such directive in the span. This is simpler, but those seven tests flip to no-fix.
   - **Recommend A.** It is still a kind-and-position check, it refuses nothing the spec allows except an unrelated same-kind directive elsewhere, and it keeps the existing tests. Either way, nothing visible is in question: this is withholding for a rare class.
2. **Where the expression-body guard lives.** Put it in the fix's registration (`ExpressionBodyToBlockCodeFixProviderBase`), not in `BlocksRewrite`. See *Surfaces*: putting it in `BlocksRewrite` would change diagnostics and CLI output.
3. **RH7310, a comment that ends up above the next member.**

   ```csharp
       int _a;

       // The methods follow
       #region Methods
       #endregion
       int _b;
   ```

   Observed output: fixed to `// The methods follow` ⏎ `int _b;`, so the comment now stands with `_b`. Under the wording of SHARED-FIX-003, a removal is none of the three ways of moving a comment away, but the requirement's reason ("makes the comment describe other code") fits.
   - **Option 1:** keep the fix (characterization test).
   - **Option 2:** withhold when a comment stands directly above the `#region` and the line below the `#endregion` holds code. That is a cheap check, and a rare class.
   - **Recommend option 2.** Withholding for a rare class is the lead's call. If the lead reads the wording strictly, option 1.

## Issue goals

- **"Brace and wrap fixes: RH3202–RH3208":** drop RH3204. It wraps nothing. No visible change.
- **"Other fixes … found by searching":** made concrete in *Mechanism and candidates*. Layout, rename, documentation, and in-place token fixes are excluded for the reasons given there. No visible change.
- **"Done when: each fix is confirmed by a test with a comment and a directive input":** change to "each in-scope fix is confirmed by an existing or new test" (Coverage first).
  - No new tests for families whose existing tests already pin the comment and directive refusals: ordering, attributes, empty types, RH3209/RH3210, RH2001.
  - No test for RH5405, which no caller reaches.
  - No visible change.
- **"Withhold the fix and keep the diagnostic":** holds for every failing member. The RH72xx comment member is not a failing member under the recommendation; it is the output question.

## Architecture findings

FC-005 class: **two brace-adding components each guard their own subset of directive kinds**. `BlocksRewrite` covers conditionals and regions. `StatementBracesCodeFixProviderBase` covers only the header–body gap. RH7004's own guard refuses pragma pairs, and C1/C2 hold there. No shared "does an added brace pair split a directive pair" check exists, so each brace-adding fix restates part of it.

It is narrow enough to fix locally. A shared Core helper, "does a directive inside span S have a same-kind or partner directive outside S", would serve RH3202–RH3208, RH5405–RH5407, and possibly RH7004. Adopting it in RH7004 is optional, because RH7004 already holds.

The lead decides whether to add the helper (it has a second consumer) or two local guards.

## Follow-up candidates

1. **Formatter defect: `ControlFlowBraceTransform` braces an `if` body that holds a conditional group ending behind the body.** It is reachable by whole-document formatting and the CLI. FMT-STRUCT-003 guards only the body's leading and trailing trivia, while FMT-STRUCT-024 and FMT-PIPE-003 forbid a result that does not compile in another build.

   Command: `scripts/format.sh <dir>` on fixture B4 (the RH5406 input above, inside `class C`). Observed output: the file is formatted and reported as `Formatted:`, and the output is

   ```csharp
           if (a)
           {
               Bar(1,
   #if !X
                   2);
           }
   #else
                   3);
   #endif
   ```

   A build that defines `X` loses the `}`. This is a defect against unchanged requirements, so leaving the construct as written puts nothing in question. It is outside this issue, which is about code fixes. It is also an adjacent-fix candidate in the counterpart of the statement-brace fix, with a few lines of guard. The lead decides. The formatter's pragma and `#nullable` split in expression bodies (A1 under `format.sh`) is **not** a defect: FMT-STRUCT-007 allows it.
2. **Not a finding: the blank line above a comment in an argument list.** With a comment on its own line inside an argument list (`return Foo(1,` ⏎ `// two` ⏎ `2);`), both the RH3202 fix and `scripts/format.sh` insert a blank line above the comment. That is blank-line behavior outside SHARED-FIX-003, I did not judge it against the blank-line specs, and I am not proposing it.

Fixtures were written under the session scratchpad, outside the repository; every input whose result is stated is quoted above.
