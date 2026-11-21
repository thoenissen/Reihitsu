Role: skeptic
Commit: b76128f (spec diff and change doc at 064eb20; production code unchanged from d63eab4)
Verdict: output question

My prompt carried no intended fix or hoped-for verdict. I worked from the issue, `00-lead.md`, `01-spec-author.md`, and `git diff d63eab4 064eb20`.

## Premise

The premise is real for case 1. Callers that reach it: the analyzer on any user document (IDE or build), the CLI through whole-document formatting, and the RH5201 code fix, which formats the chain node. The formatter decides it in `ChainLineBreakRewriter.GetWrappedLayoutLineBreaks` (`src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/ChainLineBreakRewriter.cs:135-180`):

- `isRootLineIntact` is cleared at link 0 only when the gap in front of it has a line break and unjoinable trivia.
- It is also cleared by any earlier link that gets a line break. A prefix link gets one when `StartsLine || HasInnerLineBreak(false)`.
- The first invoked link is broken when `isWrappedByUser || isRootLineIntact == false`.

`RH5201MethodChainsShouldBeAlignedAnalyzer.IsMisplaced` (`src/Reihitsu.Analyzer/Rules/Layout/RH5201MethodChainsShouldBeAlignedAnalyzer.cs:79-97`) returns `false` for `linkIndex == FirstInvokedLinkIndex` when the link does not start a line, so it never reports the first call. The spec author's *before* claims hold. My fixtures below confirm the rule's other cases on `main` code (branch head, no production change).

Case 2 (the columns of attached parts and operator rests that trivia keeps on their own line): no caller reaches it.
- `scripts/format.sh` refuses the issue's own example `a.B() // keep` ⏎ `[0]` ⏎ `.C()` (fixture H1) as unsupported.
- `scripts/apply-fix.sh RH5201` gives `no-fix-offered` for it.
- A comment inside a `?.` operator is refused too (F04, F05: "unsupported syntax: a comment or directive inside a ?. or !. operator").

I agree with dropping goal 2.

Severity: the shape is uncommon (the issue counts 0 occurrences in the repository), but `declaration.Declaration` ⏎ `.Variables.Select(…)` is ordinary code. Inside RH5201's own construct the formatter changes it without a diagnostic. P11 makes that the rule's gap, not a new-rule candidate. The cost is one condition in the analyzer, and the RH5201 fix already produces the formatter's layout.

### Fixtures

Shared header for F01–F16:

```csharp
#nullable enable
using System.Linq;

class C
{
    C Next { get; set; } = null!;
    C? Opt { get; set; }
    int[] Items { get; set; } = null!;
    C Self() => this;

    void M(C c)
    {
        <body>
    }
}
```

Commands: `scripts/format.sh <copy>` and `scripts/apply-fix.sh RH5201 <copy>`, each on its own copy of the directory. All apply-fix arms were the same under LF and CRLF.

| # | Body (input) | Formatter output | RH5201 on `main` |
|---|---|---|---|
| F01 | `var a = c /* x */` ⏎ `                .Next.Self();` | `c /* x */` ⏎ `.Next` ⏎ `.Self();` | no-diagnostic |
| F02 | `var a = c` ⏎ `                /* x */ .Next.Self();` | `c` ⏎ `/* x */ .Next` ⏎ `.Self();` (both at col 16) | no-diagnostic |
| F03 | `var a = c.Next /* x` ⏎ `        */ .Next.Self()` ⏎ `                 .Self();` | `c.Next /* x` ⏎ `*/.Next.Self()` ⏎ `.Self();` (first call kept) | fixed (column only) |
| F04 | `var a = c.Opt? // keep` ⏎ `                 .Next.Self();` | refused, FMT-UNSUP-015 | no-diagnostic |
| F05 | `var a = c? // keep` ⏎ `                 .Next.Self();` | refused, FMT-UNSUP-015 | no-diagnostic |
| F06 | `var a = c` ⏎ `                 .Next` ⏎ `                 .Next.Self();` | `c.Next` ⏎ `.Next` ⏎ `.Self();` | no-diagnostic (RH5112 reports the first link) |
| F07 | `var a = c?.Opt` ⏎ `                 ?.Opt?.Self();` | `c?.Opt` ⏎ `?.Opt` ⏎ `?.Self();` | no-diagnostic |
| F08 | `var a = c.Next` ⏎ `#if X` ⏎ `.Next.Self();` ⏎ `#else` ⏎ `.Opt!.Self();` ⏎ `#endif` | active branch: `.Opt` ⏎ `!.Self();` | no-diagnostic |
| F09 | `var a = c.Items[` ⏎ `            0].ToString().Length.ToString()` ⏎ `                 .Trim();` | `c.Items[0].ToString()` ⏎ `.Length` ⏎ `.ToString()` ⏎ `.Trim();` (first call kept) | fixed (later links) |
| F11 | `var a = c.Next` ⏎ `                 /* x` ⏎ `                 */ .Next.Self();` | `c.Next` ⏎ (blank) ⏎ `/* x` ⏎ `*/` ⏎ `.Next` ⏎ `.Self();` | **no-diagnostic** |
| F12 | `var a = c // keep` ⏎ `                .Next.Next;` | unchanged | no-diagnostic |
| F13 | `var a = (c).Next` ⏎ `                   .Next.Self();` | `(c).Next` ⏎ `.Next` ⏎ `.Self();` | no-diagnostic |
| F15 | `var a = c!` ⏎ `                 .Next.Self();` | `c!.Next.Self();` (one line) | fixed (RH5201-003) |
| F16 | `var a = c.Opt!` ⏎ `                 .Next.Self();` | `c.Opt` ⏎ `!.Next` ⏎ `.Self();` | fixed (RH5201-003 only) |
| H2 | `var a = c.Next.Opt!` ⏎ `                 ?.Next.Self();` | `c.Next.Opt` ⏎ `!?.Next` ⏎ `.Self();` | fixed (RH5201-003 only) |

Every formatter result matches the new SHARED-CHAIN-005 paragraph:
- Bullet 1 covers F06, F07, F08, F11, F13, F16 and H2.
- Bullet 2 covers F01 and F02.
- The exclusions cover F12 (no call) and F15 (break inside the first link's operator).
- F03 and F09 are not counted, which matches SHARED-COMMENT-012's "behind code on the comment's first line" and the rule that only link operators count.

## Mechanism and candidates

_N/A as a bug._ This is a behavior change, but the dispatch still bounds the candidates.

**Dispatch.** `FluentChainAnalyzerBase` registers `SimpleMemberAccessExpression`, `ConditionalAccessExpression`, `InvocationExpression`, `ElementAccessExpression` and `SuppressNullableWarningExpression`. `ChainLineBreakRewriter` visits the same five kinds.

**What breaks the root's line in front of the first invoked link (`FluentChain.FirstInvokedLinkIndex > 0`, or a held first link):**
1. A prefix link (`0 < i < FirstInvokedLinkIndex`) whose operator starts a line through an end-of-line trivia: F06, F07, F13, change doc 1.
2. The same, through a directive's or disabled text's line break: F08.
3. The same, behind the `*/` of a delimited comment that starts its own line: F11. SHARED-CHAIN-007 counts this; the formatter breaks it.
4. The same, behind the `*/` of a comment that starts on the token's line: F03. Not counted, by spec and by formatter.
5. A plain line break between a prefix link's operator tokens (`?`⏎`.`, `!`⏎`.`, `!`⏎`?.`): F16, H2, change doc 5. Counted.
6. A comment- or directive-held line break between a prefix link's operator tokens: F04. Refused by the formatter. See *Output questions*.
7. A first link held by `//`, `/* */` on the root's line, `/* */` in front of the link, a directive, or disabled text: change docs 2 and 4, F01, F02. Counted.
8. A plain line break in front of the first link: change doc 6 `e`. Not counted.
9. A plain line break inside the first link's operator: F15. Not counted.
10. A comment-held line break inside the first link's operator: F05. Not counted as the spec is written, and refused. See *Output questions*.
11. A line break inside an attached part or the root, not at a link operator: F09. Not counted.

Operator kinds `.`, `?.`, `!.` and `!?.` appear in F07, F16 and H2. The member-binding shape inside a conditional access appears in F07.

## Breaking inputs

**Classes that reach the change:**
- FC-013 (analyzer and formatter disagree): every row of the table must give the same answer on both surfaces.
- FC-015 (one decision with two owners): see *Architecture findings*.
- FC-005, FC-019, FC-020, FC-021 (a comment or a directive in the gap, a `/* */` ending mid-line): rows 2, 3, 4, 6, 7 and 10.
- FC-003 (one side of a boundary): row 8 against row 7, row 9 against row 5, row 4 against row 3.
- FC-007 and FC-008 (fix convergence; Fix All on a chain that already has another diagnostic): change doc 5, F16, H2.
- FC-017 (LF only): run the fix tests under CRLF.

**Matrix for "the first invoked link does not start a line".**

Axis 1, what breaks the root's line:
- rows 1–3, 5 and 7: must be reported;
- row 6: per the answer to the output question;
- rows 4 and 8–11: must not be reported.

Axis 2, where the first invoked link sits:
- it shares the line of the breaking prefix link (`.Items.Select`): reported;
- it sits behind another unbroken prefix link on that line (`c` ⏎ `.Next.Next.Self()` with a held first link): reported;
- it already starts a line (F10, change doc 6 `f`): not reported by -006, and judged by -002/-003 alone;
- there is no call (F12, change doc 6 `h`): not reported.

Every cell above was traced on the formatter. The analyzer side does not exist yet. The test implementer must cover, beyond the spec examples:
- F01: a block comment on the root's line;
- F02: a block comment in front of the first link;
- F06: a plain first-link break together with a wrapped prefix link. Both RH5112 and RH5201 report;
- F07: a conditional-access chain;
- F08: a directive in front of a prefix link (not the first link);
- F11: a prefix behind the `*/` of a comment on its own line, aligned to the anchor;
- F13: a parenthesized root;
- F16 and H2: a split `!.` or `!?.` in a prefix link. Today one diagnostic; after the change, two;
- F03 and F15: not reported by -006.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH5201 analyzer | must change (RH5201-006) |
| RH5201 code fix / Fix All | must stay. It already gives the formatter's layout (verified for F06, F07, F08, F11, F13, F16 and H2 through `scripts/format.sh`). The Fix All scenario must now hold two diagnostics in one chain (FC-008) |
| Formatter (`ChainLineBreakRewriter`) | must stay. Output is unchanged; all of SHARED-CHAIN-005's new paragraph is today's behavior |
| `Reihitsu.Core` `FluentChain` | not involved by requirement. See *Architecture findings* on where the shared decision lives |
| RH5112, RH5048 | must stay. F06 and change doc 2 add an RH5201 diagnostic next to theirs; the fixes converge because each pass keeps or joins the chain the formatter's way |
| Rule page `documentation/rules/RH5201.md` | changes (change doc 7). The new bullet covers the common prefix case only; the held-first-link case stays implicit, which suits a short page |
| Tests | new analyzer and fix tests for RH5201-006; a formatter characterization test for the new SHARED-CHAIN-005 example |

## Refuse option

**Refuse** means keep the case as a documented parity exception: no diagnostic, no fix. It saves one analyzer condition and leaves a layout inside RH5201's own construct that the formatter changes silently (FC-013 for CLI and IDE users together). The class is not rare enough to justify that: a wrapped property prefix ahead of a call is ordinary code. The requested change adds no new type, no new mechanism and no fix code. I do not recommend refusing.

**Declaring a shape unsupported** is no option here. No shape forces a special case: the formatter handles every row with its existing guard (`WouldJoinAcrossUnjoinableTrivia`, `EnsureLineBreak`). Rows 6 and 10 are already refused by FMT-UNSUP-015.

**Narrower option:** implement only the issue's literal case (row 1). This would leave rows 2, 3, 5 and 7 as formatter changes without a diagnostic. It is not cheaper, because the formatter's single predicate covers all of them. I do not recommend it.

## Output questions

**1 · RH5201 · Member-access chains should be aligned — comment inside the operator of a wrapped prefix link**

The spec as written (bullet 1: "between that operator's own tokens", counted as SHARED-CHAIN-007 counts one, which counts the comment's end of line) reports this case. RH5201-003 handles the same comment-held line break differently: it judges the link "by the line its first token is on". The change doc's *Visible output changes* does not list this case, so it is in question either way.

Input:
```csharp
var a = c.Opt? // keep
         .Next.Self();
```

Before (`main`): not reported. The CLI refuses the file as unsupported ("a comment or directive inside a ?. or !. operator").

After, A:
```csharp
var a = c.Opt? // keep
         .Next{|RH5201:.|}Self();
```
The fix is withheld (SHARED-FIX-010). The CLI still refuses the file.

After, B (recommended): not reported. The CLI still refuses the file.

Why B: a comment-held line break inside an operator then counts the same way RH5201-003 already counts it, as if the line break were not there. The analyzer already uses that check (`HasInnerLineBreak(true)`). A only adds a diagnostic that no fix can clear on a file the CLI refuses anyway.

The sibling case `var a = c? // keep` ⏎ `.Next.Self();` (row 10, first link) is not reported as the spec is written, and is not reported under B either. Under A it stays unreported, which is inconsistent with A's own reasoning; making A consistent would need a third reading that also reports row 10. If the maintainer picks A, the spec author should state row 10 explicitly.

## Internal choices

- **Change doc completeness.** Add the F11 shape (prefix link behind the `*/` of a comment that starts its own line, in the anchor column) to visible change 1 or 5. Today it is **not reported at all** (see *Follow-up candidates*). After the change, the first call gets one diagnostic. Change doc 5 describes only example `r`, whose prefix link is misaligned and so already reported. The spec already decides it; this is listing only. Recommend: add it.
- **The RH5201 spec's `Uses` row.** It lacks SHARED-COMMENT-012, though SHARED-CHAIN-005's exception and now its new paragraph lean on it. Recommend: add it. Wording only.

## Issue goals

- Goal 1 (the first call after a kept prefix wrap): keep it, widened as the spec does to every input the formatter breaks (rows 2, 3, 5, 7). The widening is visible and listed in change doc items 2–5. It is asked through the lead's batch.
- Goal 2 (columns of trivia-kept pieces): drop it. No caller reaches it (H1, F04 and F05 are refused). Dropping changes no visible output.
- The "Preferred compliant example" for case 2 (`[0]` aligned under the chain): drop it for the same reason.
- "Use the shared `Reihitsu.Core.FluentChain` model": this prescribes a solution. It is the lead's choice, and see *Architecture findings*.

## Architecture findings

Risk introduced by the change (FC-015): the formatter's private `ChainLineBreakRewriter.GetWrappedLayoutLineBreaks` decides "does the root's line reach the first invoked link". RH5201 will need the same decision. Restating it inside the analyzer would add a second statement of one rule. The two copies already differ in one place: the formatter uses `HasInnerLineBreak(false)` and RH5201-003 uses `HasInnerLineBreak(true)`, and Output question 1 comes from exactly that difference. `FluentChain` in `Reihitsu.Core` already holds the shared halves of this decision (`IsFirstLinkBlocked`, `IsPrefixLink`, `GetAnchorLink`). The components are the formatter's line-break phase and the RH5201 analyzer; the decision is whether the first invoked link must start its own line. The lead decides where it lives.

## Follow-up candidates

**RH5201 misses a link behind the `*/` of a delimited comment that starts its own line, when that line starts in the anchor column.**
- SHARED-COMMENT-012 and SHARED-CHAIN-005's exception require RH5201 to report such a link "whatever column it reaches", and the formatter breaks it.
- `IsMisplaced` treats the link as starting a line (`StartsLine` is true by the line break in front of the comment) and compares only the line-start column (`*/`). It reports nothing.
- This is a pre-existing gap against unchanged requirements, so a bug with no output in question. It fits issue #893's purpose ("layouts the formatter changes without a diagnostic"). It is small and in the component the run changes, so it is an adjacent-fix candidate or a follow-up.
- After this run, G3 and F11 get reported through the first call, but G1 and G2 stay silent.

Fixtures (`class C { C Next {get;set;} = null!; C Self() => this; void M(C c) { … } }`, `scripts/format.sh` and `scripts/apply-fix.sh RH5201`, LF and CRLF):

```csharp
        var a = c.Self()                 // G1: later link
                 .Self()
                 /* x
                 */ .Self();
        var a = c.Next                   // G2: chain without a call
                 .Next
                 /* x
                 */ .Next;
        var a = c.Next                   // G3: prefix link
                 /* x
                 */ .Next.Self()
                 .Self();
```

The formatter output puts each link behind `*/` on its own line (for G1: `.Self()` ⏎ (blank) ⏎ `/* x` ⏎ `*/` ⏎ `.Self();`). RH5201 on `main` gives `no-diagnostic` for G1, G2 and G3.

Observed in passing, not raised: the formatter inserts a blank line in front of an own-line `/* */` comment inside a chain (F11, G1–G3). This is pre-existing and outside the issue, and spec example `r` already shows such a blank line in its input.
