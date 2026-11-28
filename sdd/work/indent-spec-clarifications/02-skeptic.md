Role: skeptic
Commit: 166907b
Verdict: proceed

Both issues are real spec-text work, and every claim the spec diff of `d9ba2be` makes about today's output holds on `166907b`. I found three text defects for the lead to fix before the tests are written. None of them changes output.

1. FMT-INDENT-010's second paragraph still says that no region directive inside a member reaches a phase. That is true only of active ones.
2. FMT-INDENT-010's new "whitespace and text rules for directives apply to it as to an active one" also covers the blank-line padding, which SHARED-BLANK-007 excludes.
3. FMT-INDENT-021's new `Why:` puts the label's colon "on the `when` line", which is false for a guard that wraps.

There is also one class member the diff leaves unspecified: an inactive directive that is not a region directive. My prompt carried no intended fix, suspected cause, or hoped-for verdict.

Every fixture below was run with `scripts/format.sh <file>` (whole document, LF) on `166907b`, and then once more. The second pass changed nothing for any of them (`Formatted 0 of 7 file(s).` for r1, r2, r3, r4, r5, r8, w2, and `Formatted 0 of 1` for l1). The inputs were deleted afterwards. They are quoted here so the test implementer can reproduce them.

## #1400 — formatter-spec clarifications from round 9

### Premise

Real, and the issue's examples match today's output. It is a clarification, not a bug, so the reachability question is about whether whole-document formatting reaches the cases the text describes. The CLI does: every fixture below went through it.

- Item 22. The spec example in FMT-INDENT-010 gives the output the spec shows (r1): columns 2 and 10 are kept, and `#region Inner` / `#endregion // Inner` are written. The tab example gives `····#region Inner`, `→disabled`, `····#endregion // Inner` (r2: the input is `class C⏎{⏎#if NEVER⏎→#region Inner⏎→disabled⏎→#endregion⏎#endif⏎⏎    int _a;⏎}⏎`, and `cat -A` of the output shows `    #region Inner`, `^Idisabled`, `    #endregion // Inner`).
  - Mechanism in code: `LayoutComputer.SetDirectiveIndentation` (`src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/LayoutComputer.cs:466`) registers a layout only for region directives where `IsInactiveDirective == false`. `IndentationRewriter.WriteLineStart` (`IndentationRewriter.cs:179`) moves only active non-region directives to column 0, and it keeps the whitespace of a line that has no layout. So every inactive directive, region or not, keeps its column.
  - `TabReplacementPhase` replaces every `WhitespaceTrivia` and the whitespace between the tokens of every directive, active or not.
- Item 23. `LambdaAlignmentContributor.GetAnchorToken` (`src/Reihitsu.Formatter/Pipeline/Indentation/Contributors/LambdaAlignmentContributor.cs:31,48`) returns `AsyncKeyword` when it is present, and otherwise `Parameter.Identifier` or `ParameterList.OpenParenToken`. Modifiers, attribute lists, and the return type are never the anchor, as the new paragraph says. Fixture l1 confirms the spec example verbatim, and also the prose claims:

  ```csharp
  // input (inside void M() of a class with using System; using System.Threading.Tasks;)
  Func<int, Task<int>> d = static async x =>
  {
      return x;
  };
  Func<int, Task<int>> e = async static x =>
  {
      return x;
  };
  Func<int, int> f = [Obsolete] (x) =>
  {
      return x;
  };
  Func<int> g = static () =>
  {
      return 1;
  };
  Func<int, Task<int>> h = async Task<int> (x) =>
  {
      return x;
  };
  N(static int (x) =>
  {
      return x;
  });
  ```

  ```csharp
  // output
  Func<int, Task<int>> d = static async x =>
                                  {
                                      return x;
                                  };

  Func<int, Task<int>> e = async static x =>
                           {
                               return x;
                           };

  Func<int, int> f = [Obsolete]
      (x) =>
      {
          return x;
      };

  Func<int> g = static () =>
                       {
                           return 1;
                       };

  Func<int, Task<int>> h = async Task<int> (x) =>
                           {
                               return x;
                           };

  N(static int (x) =>
               {
                   return x;
               });
  ```

  The `[A] (x)` case never stays on one line. The attribute list of a lambda is a method-target list, which RH5513 and FMT-BREAK-027 put on a line of its own. So "`[A] (x) => …` under `(`" holds, but `(` then starts its own continuation line. That is existing, intended behavior (SHARED-ATTR lists the lambda under the method target), and the sentence is correct as written.
- Item 24 is already done, as the issue comment of 2026-11-16 says: FMT-SPACE-001 line 23 holds the sentence, and `MultiLineCommentGapSpacingTests.SpaceBehindMultiLineCommentInFrontOfBinaryOperatorBecomesOneSpaceAsBehindSingleLineComment` (FMT-SPACE-001, FMT-SPACE-013) pins both sides of the comment.
- Counterpart analyzers agree with "not re-indented".
  - `scripts/apply-fix.sh RH7304`, `RH7301`, and `RH7302` over the formatter output of r1 and r3 all report `no-diagnostic`. RH7304's expected indentation comes from the first token after `#region`, and for an inactive region that token lies behind the `#endregion`, so the pair is skipped.
  - On the original r1, `RH7301` reports and fixes the bare `#endregion` (`fixed (1 action(s))`). This confirms the new `Why:` claim that the analyzer reports `#endregion` comments in inactive branches.
  - RH6004 skips every `DirectiveTriviaSyntax { IsActive: false }` (`RH6004PreprocessorKeywordsMustNotBePrecededBySpaceAnalyzer.cs`, line 39 without the doc comments).

### Mechanism and candidates

_N/A_ for a bug. The candidate set of the clarified decision, derived from the dispatch in `IndentationRewriter.WriteTriviaLineStart`/`WriteLineStart` and `LayoutComputer.SetDirectiveIndentation`, covers every directive trivia (`DirectiveTriviaSyntax`) that starts a line:

| Directive | Column today | Specified by |
|---|---|---|
| active `#region`/`#endregion` | member column (layout) | FMT-INDENT-010, sentence 1 |
| active other directive | column 0 | FMT-INDENT-009 ("every active … directive") |
| inactive `#region`/`#endregion` | kept | FMT-INDENT-010, new sentence |
| inactive other directive (`#pragma`, nested `#if`/`#endif`, … inside a branch not taken) | kept | **nothing**; see below |

The last row is unspecified. Fixture d1:

```csharp
class C
{
#if NEVER
      #pragma warning disable CS0169
   #if X
    int _b;
   #endif
#endif

    int _a;
}
```

The output keeps every column; the run removed only the final newline. In fixture d2 (`#if NEVER⏎→#pragma→warning disable CS0169⏎#endif` in the same class), `cat -A` gives `    #pragma    warning disable CS0169`. Both tabs are replaced, and the column the replacement gives is kept.

FMT-INDENT-011 also says "Only the other lines that start with a token, a comment, or a directive are re-indented". That reads as "every directive line is re-indented", which the new FMT-INDENT-010 sentence contradicts for inactive regions, and which d1 contradicts for every inactive directive. The issue's own item 22 says "A directive inside inactive code is not re-indented, but it is still a directive"; it does not say "a region directive".

### Breaking inputs

Failure classes that reach the change: FC-020 (directives in fully inactive `#if` blocks, the subject itself), FC-017 (assert under every line ending and over a second pass), FC-033 (the tab example must reach the formatter as a real tab: write `\t` in the test input rather than a literal tab that an editor or a test-source format could normalize), and FC-034 (test the issue's shape, the tab in front of an inactive `#region`, not an active cousin).

Class matrix for item 22. The axes are the position of the inactive region directive and whether its column differs from the column re-indentation would give. Only a cell whose column differs can pin "not re-indented".

| Position | Fixture | Output | Pinned by |
|---|---|---|---|
| between type members, off-column | r1 | columns 2 and 10 kept, text written | none found; the spec author's test 1 |
| between type members, tab in front | r2 | tab → 4 spaces, column kept, tab in disabled text kept | none found (`TabReplacementPhaseTests.TabInFrontOfEndifAfterDisabledTextIsReplacedAndTabInsideDisabledTextIsKept` covers an active `#endif`); the spec author's test 2 |
| between namespace/type members, column 0 where the member column is 8 | r5 | `#region Inner` / `#endregion // Inner` stay in column 0 | none found |
| inside a member body, off-column | r3 | columns 2 and 10 kept, text written | none found (`UnsupportedRegionInsideMemberTests.RegionInDisabledTextIsNotChecked` asserts only that the input is supported; the inactive regions in `UnsupportedRegionInsideMemberTests`, around line 144, and in `DisabledRegionLayoutTests` already stand in the column re-indentation would give) |
| inside an argument list / array initializer | r8 | columns 3, 5, 1, and 0 kept | none found |
| `#region   foo   ` / `#   endregion   ` in a branch not taken | r4 | `#region Foo`, `#endregion // Foo`, and the leading space kept | FMT-REGION-007 tests for the text; the column is not pinned |
| region text in a branch not taken | — | — | `DisabledRegionLayoutTests.RegionInsideDisabledCodeIsRenamedWithoutBlankLinePadding` |

Fixture r3 (whole document, output keeps both columns and reads `#region Inner` / `#endregion // Inner`):

```csharp
class C
{
    void M()
    {
#if NEVER
  #region inner
        disabled();
          #endregion
#endif
        M();
    }
}
```

Fixture r5 (output identical except `Inner` and `#endregion // Inner`):

```csharp
namespace N
{
    class C
    {
        int _a;
#if NEVER
#region inner
        int _b;
#endregion
#endif
    }
}
```

Fixture r8: the output keeps every directive column and writes `#region Inner`, `#endregion // Inner`, `#region R`, and `#endregion // R`. The initializer moves under `new` and loses its trailing comma, which other rules decide.

```csharp
class C
{
    void M(int a, int b)
    {
        M(1,
#if NEVER
   #region inner
          3,
     #endregion
#endif
          2);
        var x = new int[]
        {
            1,
#if NEVER
 #region r
#endregion
#endif
            2,
        };
    }
}
```

Inputs the test implementer must cover:

- The spec author's three examples: FMT-INDENT-010 code blocks (r1), the FMT-INDENT-010 tab example (r2), and the FMT-INDENT-028 second example. Pinned: none found, except that `int (x)` is pinned by `LambdaReturnTypeJoinTests.SpecExampleReturnTypeJoinsTheParameterListInEveryPosition`, which is tagged FMT-BREAK-045, not FMT-INDENT-028.
- r3, the inactive region inside a member at an off column. It pins the corrected second paragraph (see *Internal choices* 1). Pinned: none found.
- `static async x => { … }`, block under `async`. The new FMT-INDENT-028 paragraph states it, and no test pins `async` behind `static`: the existing FMT-INDENT-028 tests use `async x`. It can join the FMT-INDENT-028 characterization test as a fourth lambda. Pinned: none found.
- If the lead adopts *Internal choices* 2: d1 and d2, the inactive non-region directives keeping their column. Pinned: none found. My search of the formatter tests found only active `#else`/`#endif` at off columns.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `sdd/specs/formatter/indentation.md` (FMT-INDENT-010, -011, -028; `Uses` row) | must change (text only) |
| `sdd/specs/formatter/region-formatting.md` Non-goals | may change (*Follow-up candidates*) |
| `sdd/specs/formatter/horizontal-spacing.md` (item 24) | not involved; already done |
| Formatter tests | must change: new characterization tests only |
| `IndentationPhase`, `LayoutComputer`, `IndentationRewriter`, `LambdaAlignmentContributor`, `TabReplacementPhase`, `RegionFormattingPhase` | must stay |
| Analyzers RH7301, RH7302, RH7304, RH6004, RH5513 | must stay; consistent with the clarified text (see *Premise*) |
| Code fixes, Fix All, `Reihitsu.Core`, rule docs | not involved |

### Refuse option

The refuse option for a spec clarification is to leave the text as it was. That costs nothing in production code, but so does the change. The old "MUST NOT be touched" contradicts today's output, which the maintainer confirmed in round 9, so leaving it is not cheaper in any way that matters.

Declaring a shape unsupported is not an option here. An inactive region inside a member, the only candidate, would delete no special case: the `IsInactiveDirective == false` filter in `SetDirectiveIndentation` also serves inactive regions between members, and "a shape that deletes nothing is not declared". It would also refuse files that hold ordinary `#if DEBUG` code around a region.

No simpler option.

### Output questions

_None._ Every case is today's output, and the maintainer confirmed it in round 9, #912 items 22 and 23, option (a). The widening in *Internal choices* 2 also states today's output and changes nothing visible.

### Internal choices

1. **FMT-INDENT-010, second paragraph: qualify "active".** Today the paragraph reads "A region directive inside a member … reaches no phase in whole-document formatting: the formatter refuses such an input (FMT-UNSUP-023)" and "The one region directive inside a member that the phases receive stands in the leading trivia of the first token of a node-level input". FMT-UNSUP-023 refuses only an *active* directive (`unsupported-syntax.md:1822`), and r3 shows an inactive region inside a member reaching every phase: its text is written and its column kept. The new first paragraph now speaks of inactive directives in general, so the contradiction sits inside one requirement.
   - Recommend: "An active region directive inside a member …" in both sentences. An inactive one inside a member falls under the new sentence (kept column).
   - Alternative: leave it as is, which keeps a false statement.
   - No visible output either way.
2. **State "not re-indented" for every inactive directive, once.** Options:
   - (a) Put the rule in FMT-INDENT-011 next to disabled text: "a directive in a branch the build does not compile, region or not, is not re-indented and keeps its column; it is still a directive, so FMT-TAB-001/-002 apply". Fix FMT-INDENT-011's "Only the other lines that start with … a directive are re-indented" to except it, and have FMT-INDENT-010 reference FMT-INDENT-011 for its inactive case and keep only the region-specific part (FMT-REGION-006/-007).
   - (b) Keep the diff's region-only sentence and add an "inactive non-region directives keep their column" sentence to FMT-INDENT-009.
   - (c) Leave the non-region case unspecified, as the diff does.

   Recommend (a). It is the issue's own wording ("A directive inside inactive code"), the shared behavior then stands in one place instead of being restated, and it removes the FMT-INDENT-011 contradiction. Either (a) or (b) adds d1/d2 to the tests. No visible output in any option.
3. **FMT-INDENT-010: narrow "the whitespace and text rules for directives apply to it as to an active one".** Blank-line padding around a region directive is a whitespace rule for directives too, and SHARED-BLANK-007 says it MUST NOT apply in a branch not taken (r1: no blank lines inserted). Recommend: "the tab and region-text rules apply to it as to an active one", optionally adding "its blank lines are not padded (SHARED-BLANK-007)" with SHARED-BLANK-007 in the `Uses` row. The alternative, keeping the sentence, overstates. No visible output.
4. **FMT-INDENT-028 wording: "the first token of its parameters".** For `static () => { … }` (l1 `g`) there are no parameters, yet the anchor is the `(`. Recommend: "the opening parenthesis of its parameter list or its single parameter", which matches the first sentence of the requirement. No visible output.

### Issue goals

- Item 24: drop it. It is already done (issue comment 2026-11-16, FMT-SPACE-001), and the spec author rightly left it unedited. No visible output.
- Item 22's cross-reference to FMT-CLEAN-002/-003: change it. Both requirements are retired and point at FMT-TAB-001/-002, which the diff references instead. That is correct.
- Item 22 itself: its target is FMT-INDENT-010, but its text speaks of any directive in inactive code. I would widen it as *Internal choices* 2 (a) says. No visible output.
- "Done when": keep it. The characterization tests are allowed by it and are needed: see *Breaking inputs*.

### Architecture findings

_None._ "An inactive directive keeps its column" is decided in one place: the layout filter in `LayoutComputer` plus the no-layout fallback in `IndentationRewriter`. The analyzers (RH6004, RH7304) skip inactive directives consistently with it.

### Follow-up candidates

- `sdd/specs/formatter/region-formatting.md`, Non-goals: "a region directive inside a member is refused as unsupported (FMT-UNSUP-023)" has the same over-generalization as *Internal choices* 1. An inactive one is not refused, and FMT-REGION-006/-007 write its text (r3). This is spec text only and can be fixed in this run beside FMT-INDENT-010, or left as a follow-up.

## #1936 — FMT-INDENT-021 Why text

### Premise

Real. The old `Why:` ("must not reach the column of the section's statements, one level deeper") contradicts the example, where `when` and `break;` share column 16. `CaseWhenClauseContributor` gives the `when` line the `case` column + 4. That agrees with FMT-INDENT-034's column: the construct the `when` line continues is the `CasePatternSwitchLabel`, whose `case` starts the earlier line. The issue's case is reachable only where `when` starts a line, that is, behind a comment (the spec example) or behind a pattern that spans several lines (line-breaks.md:2294). Without one of those, the line-break phase joins `when` onto the label (w1: `case 1⏎when Check(value):` became `case 1 when Check(value):`).

### Mechanism and candidates

_N/A._

### Breaking inputs

The new `Why:` says "the colon that ends the label on the `when` line tells the guard apart". When the guard itself wraps, the colon is not on the `when` line. Fixture w2:

```csharp
class C
{
    void M(int value)
    {
        switch (value)
        {
            case int x // positive
            when Check(x)
            && Check(value):
                {
                    break;
                }
        }
    }

    bool Check(int value)
    {
        return true;
    }
}
```

The output has `when Check(x)` in column 16, `&& Check(value):` in column 21, and the section's `{` in column 16. So the colon ends the label on the label's last line. The behavior is pinned by `ListAndHeaderAlignmentTests.WrappedWhenClauseIsIndentedOneLevelDeeperThanCase` and `CaseWhenClauseContributorTests.AlignsWrappedWhenKeywordToCasePlusIndent`. No test is needed: the change is text only.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `sdd/specs/formatter/indentation.md` FMT-INDENT-021 `Why:` | must change (text only) |
| `CaseWhenClauseContributor`, tests, analyzers, code fixes, rule docs | not involved |

### Refuse option

Leave the old `Why:`. It contradicts its own example, so it is not a real alternative. No unsupported shape is involved, and no simpler option exists.

### Output questions

_None._

### Internal choices

- **"the colon that ends the label on the `when` line" → "the colon that ends the label".** That holds for a guard written over several lines (w2) and for the example alike. Recommend changing it. The alternative, keeping it, is false for w2. No visible output.
- The analogy to FMT-INDENT-034 is sound: the column FMT-INDENT-034 would give agrees with FMT-INDENT-021, and FMT-INDENT-021 stays the owner. Keep it.

### Issue goals

None to drop. "no test or code changes" holds.

### Architecture findings

_None._

### Follow-up candidates

_None._
