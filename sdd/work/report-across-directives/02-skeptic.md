Role: skeptic
Commit: 3d85973
Verdict: output question

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Inputs: thoenissen/Reihitsu#1759 and its comment, the maintainer decision recorded on thoenissen/Reihitsu#1756, `00-lead.md`, `01-spec-author.md`, the change doc `sdd/changes/2026-11/report-across-conditional-directives.md`, and `git diff 340608b 3d85973`.

## Premise

**The principle is decided.** The maintainer's general rule (analyzers judge the code as the current build's symbols are set) is recorded on thoenissen/Reihitsu#1756, whose scope comment names #1759 as the general reversal. The reversal itself is not in question; which rules it reaches and what each shows is.

**Reachability.** Every changed output is an analyzer diagnostic or a code-fix result on user source; the caller is the analyzer in the user's build and the IDE code-fix path. No formatter entry point changes.

**The issue's example is wrong.** It says RH5412 does not report `class C { #if DEBUG int _x; #endif }` today. It does. `scripts/apply-fix.sh RH5412 <dir>` on `3d85973`:

```csharp
class A
{
#if DEBUG
    int _x;
#endif
}
```
→ `no-fix-offered` (LF and CRLF). `class B { #region R #endregion }` → `no-fix-offered`. `#if !NEVER int _x; #endif` → `no-diagnostic`. So SHARED-EMPTY-001 is a clarification, as the spec author says, and the empty-type rules have no visible change.

**The issue's "Not in scope: code fixes keep withholding themselves as today" is wrong for two fixes.** Today two fixes are safe only because their analyzer never reports the input. With the analyzer guard removed, they would be offered:

- Trailing commas (RH5409, RH5410, RH5411): `TrailingCommaRemovalCodeFixProviderBase.RegisterCodeFixesCoreAsync` registers the fix for every diagnostic with no guard. The only conditional-directive check is the analyzer's `ContainsConditionalCompilationBoundary(node, gap)` in `TrailingCommaAnalyzerBase{TNode}.cs`. If that check only moves out of the analyzer, the fix removes the comma in front of `#if` and breaks every build that compiles the branch.
- RH8201: `HasConditionalInterfaces` sits only in `RH8201InheritdocShouldBeUsedAnalyzer`. The fix (BatchFixer) has no copy.

SHARED-TRAIL-002 and RH8201-003 in the spec diff require the fix to be withheld, so the spec is right. The code fix is a must-change surface for these two, not "as today".

**Every other fix already refuses the new inputs with a guard of its own**, read in the code at `3d85973`:

| Rule(s) | Fix guard that covers a conditional directive or excluded code in the judged span |
|---|---|
| RH5102, RH5109 | `SyntaxNodeUtilities.LeadingAndInteriorContainsCommentOrDirective(list)` (any directive) |
| RH5111 | `ContainsCommentOrDirective` / `InteriorContainsCommentOrDirective` |
| RH7101 | `ReihitsuFormatter.FindUnsupportedSyntax` (FMT-UNSUP-010), confirmed below |
| RH7105, RH7106 | `ModifierOrderingCodeFixProviderBase.ReorderingWouldDropCommentOrDirective` (`trivia.IsDirective` in front of any modifier but the first, plus disabled text behind the last) |
| RH7102, RH7103 | `TypeMemberOrderingCodeFixProviderBase.IsMoveSafe`: `MoveRangeContainsDirectives` (SHARED-ORDER-003) and `ChangesMemberOrder` (SHARED-ORDER-007/008). A group in the modifiers does not wrap whole units, so ORDER-008 refuses it |
| RH7107, RH7108 | `AccessorOrderingUtilities.MoveRangeContainsDirectives` (SHARED-ORDER-003). In the spec example, `#endif` is leading trivia of the moved `get` and its `#if` is outside the moved half, so the move is refused, as the spec says |
| SHARED-ATTR-019 split | `InteriorContainsCommentOrDirective(attributeList)` |
| SHARED-ATTR-020 merge | `CanMergeAttributeListGroup` → `GroupInteriorContainsCommentOrDirective` |
| RH5412–RH5416, RH5418 | SHARED-EMPTY-004 (observed `no-fix-offered` above) |

All of these fixes use BatchFixer or `DocumentOrderFixAllProvider`, which run the registration guard per diagnostic, so Fix All inherits each guard. That includes the two new guards, if they are put into registration.

RH7101's guard, confirmed with `scripts/format.sh <dir>`:

```csharp
class C
{
    private int _a,
#if DEBUG
                _b,
#endif
                _c;

    private int
#if DEBUG
        _x,
#endif
        _d, _e;
}
```
→ `F.cs(4,1): unsupported syntax: a directive inside a field declaration with several declarators. The file was left unchanged.`

**Rules not in the change doc.** I derived the candidates from the code, not from the issue: every analyzer or Core caller of `ContainsConditionalCompilationBoundary*`, `HasModifiersSplitByConditionalDirective`, `IsInConditionalBranch`, `IfDirectiveTrivia`, `IsActive`, or `DisabledTextTrivia`. Beyond the change doc's list, the hits are:

- `RegionDirectiveBlankLineAnalyzerBase`, RH8302, and RH7309 already use `IsActive`, which is the new reading.
- RH6004 and `FormattingTextAnalysisUtilities` look at layout of inactive directives, with no silence.
- RH7310 is kept, per the change doc.

RH7109 and RH7110 judge modifiers too and never had a guard. They already report across a conditional directive, which is consistent with the reversal. No silence guard is missing from the change doc.

## Mechanism and candidates

_N/A_ (behavior change). The dispatch fact that makes most of the change a deletion: Roslyn parses only the active branch into nodes, and excluded code is `DisabledTextTrivia`. So once its guard is gone, each analyzer already sees the analyzed build's version. RH7311 is the exception: it must read `RegionDirectiveTriviaSyntax.IsActive`, as RH7309 already does, instead of `RegionDirectiveUtilities.IsInConditionalBranch`.

## Breaking inputs

Failure classes that reach the change are FC-020, FC-005, FC-004, FC-008, FC-013/FC-014 (below), and FC-016.

**FC-016 (one rule fixed, siblings keep the hazard), as a class.** The class is "the analyzer's silence was the only thing keeping the fix away from the input". It is settled per rule in the table above: two members fail (trailing commas, RH8201) and every other member holds. For each rule whose silence is removed, the test implementer covers this matrix:

- **Directive shape in the judged span:**
  - an empty `#if X … #endif` block;
  - an `#if NEVER` branch with excluded code;
  - an `#if !NEVER` branch with active code;
  - an `#if … #else …` group and an `#elif` group;
  - `#if false` (FC-020).
- **Expected result for each shape:** reported, and the fix withheld.
- **Contrast:** the same directive outside the judged span, where the fix is offered as the rule's spec states.
- **Fix All:** one guarded and one unguarded diagnostic in one document. Only the unguarded one changes. This matters most for RH5409/RH5410/RH5411 and RH8201, whose guards are new (FC-008).
- **RH7311:** a region in the compiled branch vs. outside (reported); regions in two branches of one group (never compared); two regions inside the same compiled branch (now compared, reported).

**FC-013 / FC-014 (the formatter keeps what a rule now reports).** Three rule groups will report a shape that `reihitsu-format` keeps. CLI users get a diagnostic that formatting never clears, and no fix. I observed each:

- **RH5111.** `scripts/format.sh` keeps the line break around `=` (it only re-indents `= 1` from column 13 to column 9):
  ```csharp
          var value
  #if DEBUG
  #endif
              = 1;
  ```
  SHARED-DIAG-010 covers RH5111, so this extends a P31 exception. It is in the change doc, but see *Output questions* 2.
- **Attribute shape (SHARED-ATTR-019/020).** `[A,\n#if DEBUG\n B,\n#endif\n C]` and `[A]\n#if DEBUG\n[B]\n#endif\n[C] int a` come out unchanged. The `#pragma` variant is reported today, so this has a precedent. These rules are not SHARED-DIAG-010 layout rules; no conflict.
- **Trailing commas.** The formatter keeps the comma in front of a conditional directive (FMT-STRUCT-019/020, `ContainsConditionalCompilationBoundary` in `EnumTrailingCommaRemovalTransform` and `InitializerTrailingCommaRemovalTransform`). Not a layout rule, so no SHARED-DIAG-010 conflict; see *Output questions* 1.

RH5102 and RH5109 do **not** conflict. `scripts/format.sh` (stable on a second pass) breaks every list below one argument per line. Its output then satisfies RH5102-001 and RH5109 in the analyzed build:

```csharp
M(1, 2,
#if DEBUG
#endif
  3);                         // → 1, / 2, / … / 3 on own lines
M(1,
#if DEBUG
#endif
  2, 3);                      // → 2, and 3 split
M(
#if DEBUG
#endif
    1, 2,
    3);                       // → each argument on its own line
void P(int a, int b,
#if DEBUG
#endif
       int c)                 // → each parameter on its own line
void P(int a,
#if DEBUG
#endif
       int b, int c)          // → each parameter on its own line
```

`M(1, 2\n#if DEBUG\n , 4\n#endif\n );` stays unchanged. RH5102-001 does not report it: the arguments start on one line, and the closing parenthesis is not judged.

**Not a member.** Excluded text that is itself badly laid out is reported in a build that defines the symbol, and `reihitsu-format` cannot clear it there, because the CLI parses with `CSharpParseOptions.Default` and no symbols (`FormatCommandHandler.cs:143`). An example is `4, 5,` on one line inside `#if DEBUG` in an argument list. The same is already true for every construct inside an `#if` branch, and `general-notes.md` covers excluded code. This change does not create it.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Analyzers: RH5102, RH5109, RH5111, RH7101, RH7102, RH7103, RH7105, RH7106, RH7107, RH7108, `TargetAttributeListShapeAnalyzerBase`, `TrailingCommaAnalyzerBase{TNode}`, RH8201 | Must change: remove the reporting guard |
| RH7311 analyzer | Must change: compare active regions (`IsActive`) instead of skipping every region in a branch |
| Trailing-comma fix (`TrailingCommaRemovalCodeFixProviderBase`) | Must change: gains a withhold guard for a conditional directive or disabled text between the comma and the brace |
| RH8201 fix | Must change: gains the `#if`-in-file guard (moved from the analyzer) for implicit implementations only |
| Other fixes listed under *Premise* | Must stay: their own guards already refuse the new inputs; only tests change |
| `Reihitsu.Core` (`OrderingDeclarationUtilities.HasModifiersSplitByConditionalDirective`, `AccessorOrderingUtilities`, `SyntaxTriviaUtilities`) | Must change only where a helper becomes unused or its doc comment states the old silence; the ordering move-safety helpers must stay |
| Formatter | Not involved (FMT-STRUCT-019/020 keep the comma; the formatter spec's *Why* still reads correctly) |
| Rule pages | RH5409, RH5410, RH5411, RH8201 lose *Exceptions* (in the diff); `general-notes.md` changes, with a wording defect noted under *Internal choices* |
| RH5412–RH5416, RH5418, RH2001, RH2005, RH3002, RH7003 | Not involved in behavior; spec references only |
| Tests | Must change: the old-silence tests the spec author listed turn into "reported, no fix" tests, and new tests cover the matrix above |

## Refuse option

- **Keep the old silence (status quo) per rule.** It costs nothing in production, but it contradicts the maintainer's general rule on thoenissen/Reihitsu#1756. The requested change is mostly deletions: about a dozen analyzer guards removed, one analyzer rewritten (RH7311), and two small fix guards, which are the existing analyzer checks moved. It adds no new type and no new mechanism, so it is cheaper than most refusals. Not recommended as a whole. Per rule, it remains the *B* option of each output question.
- **Declaring a shape unsupported (escalation 10).** This is an option for exactly one shape the run touches:
  - *Shape:* a conditional directive (`#if`, `#elif`, `#else`, `#endif`) or disabled text between the comma after the last element and the closing brace of an enum member list, an array initializer, or a collection initializer.
  - *Special cases it would delete:* the keep-the-comma check in the formatter's two trailing-comma transforms (`EnumTrailingCommaRemovalTransform`, `InitializerTrailingCommaRemovalTransform`). The new SHARED-TRAIL-002 fix guard would become SHARED-FIX-010, so nothing is gained there.
  - *Common code it would refuse:* every file with an `#if` block at the end of an enum or initializer, such as platform-specific enum members. That is common code.
  - *Recommendation:* do not declare. If the maintainer wants it, do it in a change of its own.
- **For attribute lists, declaring is not an option.** The formatter's split guard (`AttributeListShapeUtilities`, `InteriorContainsCommentOrDirective`) also serves comments, so declaring the conditional shape would delete nothing.
- **For RH5111,** SHARED-DIAG-010's general rule is a narrower option. RH5111 would keep a gap the formatter keeps silent for a reason that holds in every build; this is *Output questions* 2 B.

## Output questions

The spec author's three questions stand; on 2 and 3 I agree with A. RH7309 already compares only active regions, so A on RH7311 makes the two region-order rules read alike. Two additions:

**1 · RH5409 · Final enum members must not have trailing commas (also RH5410, RH5411): the spec author's question 1, with what the maintainer should also see**

Input, the common shape:
```csharp
enum E
{
    A,
#if DEBUG
    B,
#endif
}
```
Before (`main`): not reported in either build.

After, A: reported **in both builds**, never with a fix, and `reihitsu-format` keeps both commas:
```csharp
    A{|RH5409:,|}          // build without DEBUG
#if DEBUG
    B{|RH5409:,|}          // build with DEBUG (the #endif lies between the comma and the brace)
#endif
```
The only shape clean in both builds is the comma inside the branch:
```csharp
enum E
{
    A
#if DEBUG
    , B
#endif
}
```
`scripts/format.sh` leaves it unchanged (apart from unrelated initializer indentation in the array variant). `scripts/apply-fix.sh` RH6002, RH5107, RH5409, and RH5410 report `no-diagnostic` on it, both with `#if DEBUG` (excluded) and with `#if !NEVER` (active).

After, B: not reported (SHARED-TRAIL-002 keeps its own silence).

The spec author recommends A. I see no defect in A, but the maintainer should pick knowing that A flags the shape above in every build, and that the only remedy is the leading-comma style.

**2 · RH5111 · Assignments must have proper line breaks: conditional directive around `=`**

Input:
```csharp
var value
#if DEBUG
#endif
    = 1;
```
Before (`main`): not reported.

After, A (spec diff): reported, no fix offered. `reihitsu-format` keeps the line break, so the diagnostic never goes away by formatting. This extends RH5111's existing exception to SHARED-DIAG-010 (today: comment, `#region`, `#endregion`, `#pragma`).

After, B: not reported, because SHARED-DIAG-010 keeps a layout rule silent on a gap the formatter keeps.

Recommendation: A. The exception's own reason (a directive between a target and its `=` is syntax the rule asks the author to remove) applies to a conditional directive unchanged. Still, the change doc lists this case without marking it as a P31-exception decision, so it should be asked as its own line rather than folded into the general visible-change list.

## Internal choices

- **`general-notes.md` wording.** The diff moved "The `#region` and `#endregion` directives inside such a branch are an exception: the compiler still reads them …" from the first paragraph to the end of the new second paragraph. There, "such a branch" has no antecedent: the new paragraph speaks of "an `#if` block" inside checked code, not of a branch whose condition is false. Recommend keeping that sentence at the end of the first paragraph, where it qualified "not checked". This is a wording fix with no output decision.
- **Fix half bounded "by the part the fix rewrites"** (spec author's choice) versus blanket withholding. I agree with the spec author: blanket withholding would remove RH2005's offered fix, a visible regression with no safety gain.
- **Per-rule SHARED-DIAG-008 references for the six empty-type rules** (issue comment) versus the single SHARED-EMPTY-001 statement. I agree with one statement. Behavior is unchanged, and `RH5412`/`RH5416` already have reported-without-fix tests. Missing per-rule tests for RH5413–RH5415/RH5418 are cheap but not required by any changed requirement.

## Issue goals

- "Nine analyzer specs rely on it": stale. The real set is wider (attribute-shape rules, trailing commas, RH7311, RH8201) and is listed in the change doc. Dropping the count changes nothing visible.
- The example "Today: RH5412 does not report it": false (see *Premise*). Drop it; nothing visible changes, because RH5412 already reports without a fix.
- "Not in scope: code fixes keep withholding themselves as today": false for RH5409/RH5410/RH5411 and RH8201, whose fixes must gain a guard. Keep the intent (no fix offered); change the means. Nothing visible beyond what the spec diff states.
- "The guards are removed where they only served the reporting half": for trailing commas and RH8201, the guard moves to the fix instead of being removed.
- "A test per rule (reported, no fix)": keep it for every rule whose output changes. For the rules whose output does not change (empty types, RH2001, RH2005, RH3002, RH7003), existing tests suffice; the lead decides. No visible effect.
- "RH2001-007 to be confirmed": confirmed by the spec author (`no-fix-offered`). Nothing to drop.

## Architecture findings

_None._ The two fixes that relied on their analyzer's silence are not two components owning one decision. The analyzer alone owned it, and the change moves it to the fix, which is the change's own work.

## Follow-up candidates

- **Narrower trailing-comma guard (optional, low value).** A comma in front of `#else`, `#elif`, or `#endif` with no `#if` between it and the brace sits inside the branch that ends there. No build that compiles the comma compiles a later element, so removing it keeps every build intact. This is a cheap check on directive kinds that evaluates no condition. It would give the DEBUG half of *Output questions* 1 a fix, but the build without DEBUG stays reported. The formatter (FMT-STRUCT-019/020) would have to follow. File it only if the maintainer picks 1A and wants fixes there.
- The spec author's SHARED-USING-009 candidate stands; I have nothing to add to it.
