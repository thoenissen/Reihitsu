Role: skeptic
Commit: 9c9ea5c
Verdict: proceed

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Inputs read: thoenissen/Reihitsu#1486, `00-lead.md`, `01-spec-author.md`, `sdd/changes/2026-11/region-directive-separator.md`, `git diff f8dc7ef..HEAD -- sdd/specs`, `RegionNamingRewriter`, `RegionDirectiveUtilities`, `ReihitsuFormatter` node-level entry points, the region analyzers' description reads, and every code fix that calls the formatter.

## Premise

Real, and reached by the CLI and whole-document formatting. `scripts/format.sh` on `9c9ea5c` (fixture in a scratch directory, deleted):

| Input line | Output today |
|---|---|
| `#region    Baz` … `#endregion    Baz` | `#region    Baz` … `#endregion // Baz` |
| `#region    bar   ` (three trailing spaces) | `#region Bar` (trailing spaces gone) |
| `#region Bar   ` (three trailing spaces) | `#region Bar   ` (kept) |
| `#region` … `#endregion    // Stale` | unchanged |

Mechanism of today's output, from the code: `RegionNamingRewriter.Rewrite` calls `BuildRegionTrivia` only when `CapitalizeFirstLetter` changes the description; `BuildRegionTrivia` clears the keyword's trailing trivia and writes `" " + trimmed description`. No other phase writes the separator. The issue's description of today's code holds on `9c9ea5c`.

**A second caller reaches the case, and the change doc says none does.** Code fixes that format a node holding a whole region pair run `RegionFormattingPhase` on it (`FormattingPipeline.Execute` in `FormatNodeInDocumentAsync`). `scripts/apply-fix.sh` on `9c9ea5c`:

- RH7105 on `static public class C { #region    bar … #endregion // Bar }` writes `public static class C` **and** `#region Bar` — the region phase already applies FMT-REGION-001 inside the fix.
- RH7105 (same type with `#region    Bar`), RH7104 (`partial class C` with a region pair), RH8305 (`/// <summary>Text</summary>` on a class with a region pair): each fix keeps `#region    Bar` today. After FMT-REGION-007 each writes `#region Bar`.

The class is every code fix whose formatted node contains both directives of a described region pair: the type-level fixes (RH7104, RH7105, RH7106, RH8305, RH8307, RH8309, RH5110 on a type) and member-level fixes whose formatted member body holds a region pair. RH7102/RH7103/RH7109 withhold for regions (spec author's probe). RH7301 and RH7302 fixes do not call the formatter and are not involved. This is a visible output change the change doc must list (Output question 3).

## Mechanism and candidates

_N/A_ (behavior change). Candidate set derived from the dispatch: `RegionNamingRewriter.Rewrite` iterates `RegionDirectiveUtilities.GetRegionPairs` (all `RegionDirectiveTriviaSyntax`/`EndRegionDirectiveTriviaSyntax` found by `DescendantTrivia(descendIntoTrivia: true)`, active and inactive, LIFO-paired); an unpaired directive is never rewritten. A `#region` line is `HashToken` [whitespace] `RegionKeyword` [keyword trailing trivia] [`PreprocessingMessageTrivia` on `EndOfDirectiveToken`] `EndOfDirectiveToken` [end of line].

## Breaking inputs

Failure classes that reach the change: FC-020 (`#region` in an `#if false` block), FC-010 and FC-017 (LF and CRLF; second pass; FMT-PIPE-027 phase-alone check), FC-028 (`FormatNode` / `FormatNodeInDocumentAsync` vs document formatting), FC-030 and FC-032 (formatter-delegating code fixes change output), FC-002 ("has a description" must be the predicate, not "is rewritten"), FC-003 (both sides: one space already vs several).

Candidate inputs the test implementer must cover (each under LF and CRLF, with a second pass):

| Axis | Members |
|---|---|
| Separator | one space (unchanged); several spaces; a tab (four spaces after preparation); tab mixed with spaces |
| First character of the description | upper-case letter; lower-case letter (FMT-REGION-001 also applies); digit or `_` |
| Trailing whitespace behind the description | none; present (decided by Output question 2) |
| Location | between members; inside a method body; nested pair; straddling pair (FMT-REGION-005); `#if false` branch (FMT-REGION-006) |
| Description | present; absent (`#region` with or without trailing whitespace stays, FMT-REGION-004) |
| `#endregion` of a description-less region | `#endregion    // Stale`; `#endregion//Stale` (decided by Output question 1) |
| Caller | whole document; a code fix formatting a type with a region pair (RH7105 or RH7104); a code fix formatting a member whose `#region` is in its leading trivia and whose `#endregion` is outside it (RH3202 — unchanged today: `#region    bar` is kept, Output question 4) |

Existing tests whose expected output changes: `TabReplacementTests.TabBetweenRegionKeywordAndDescriptionKeepsFourSpacesUnlessTheRegionLineIsRewritten` and `TabReplacementTests.TabInsideRegionDescriptionIsKeptInBothDirectives` (`src/Reihitsu.Formatter.Test/Regression/TabReplacement/TabReplacementTests.cs`). Tests that must keep `#region    …` because they run the preparation phase alone: `TabReplacementPhaseTests.TabBetweenRegionKeywordAndDescriptionIsReplacedAndTabsInsideFreeTextAreKept` and `FormattingPipelineTests.PreparationPhaseExecutedDirectlyReplacesTabsAndRewritesLineBreaks`. Analyzer fixtures with wide separators (RH7301, RH7302, RH7311, RH7401 tests) belong to fixes that do not call the formatter; run the analyzer project in full anyway (FC-032).

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `RegionFormattingPhase` / `RegionNamingRewriter` | must change |
| `TabReplacementPhase` (FMT-TAB-002) | must stay; only its spec example changes |
| `HorizontalSpacingPhase`, `CleanupPhase` | must stay |
| `Reihitsu.Core` `RegionDirectiveUtilities` | must stay — `GetRegionDescription` trims and is shared by RH7301, RH7302, RH7308, RH7309, RH7311 and the region-grouping bases |
| Analyzers (RH7301, RH7302, RH7304, RH7308, RH7309, RH7311, RH5031) | must stay — every one reads the trimmed description or a column; no diagnostic changes |
| Code fixes that delegate to the formatter | no code change; output changes for a formatted node with a described region pair (Output question 3) |
| RH7301 / RH7302 code fixes | not involved (no formatter call); RH7302-004 keeps the separator as written, as it does today for a capitalized line |
| Fix All | not involved |
| Rule pages, `general-notes.md` | not involved |
| CLI options, exit codes, messages | not involved |
| Change doc | must change — the `Code fixes` bullet under *Unchanged* is wrong |

## Simpler option

_None_ for the change itself; it is one condition in one rewriter. Under Output question 2, option C is the simpler rule and the simpler code: one canonical form for every described `#region`, compared and written like `#endregion` already is under FMT-REGION-002.

## Output questions

### 1 · Formatter — `#endregion` of a region without a description, with several spaces before its text

The spec author's question 1. I agree with **B**. Input `#region` … `#endregion    // Stale`; before (`main`): unchanged; A: unchanged; B: `#endregion // Stale`, and `#endregion//Stale` becomes `#endregion //Stale`. Why B: one separator rule for both directives; FMT-REGION-004's reason (do not delete the only label) still holds, since only the whitespace in front of the label changes. Note for the maintainer: under B, `#endregion//Stale` gains a space that was not there, which goes slightly beyond the issue's wording (normalizing an existing separator).

### 2 · Formatter — trailing whitespace on a `#region` line with a description

The spec author's question 2, with a third option. Today a line rewritten for capitalization loses its trailing whitespace (`#region    bar···` → `#region Bar`), a line with one space keeps it (`#region Bar···`).

Input (`···` = three spaces):

```csharp
class C
{
    #region    Bar···

    int a;

    #endregion // Bar

    #region Baz···

    int b;

    #endregion // Baz
}
```

Before (`main`): both lines unchanged.

After, A: `#region Bar···` and `#region Baz···` — only the separator changes.

After, B: `#region Bar` and `#region Baz···` — a line rewritten for its separator is trimmed, a line with one space keeps its trailing whitespace.

After, C (recommended):

```csharp
class C
{
    #region Bar

    int a;

    #endregion // Bar

    #region Baz

    int b;

    #endregion // Baz
}
```

Why C: every described `#region` line gets one form, as every `#endregion` already does (FMT-REGION-002); B keeps an output that depends on what the separator was, the same kind of dependence the change removes for the first letter. If the maintainer wants to stay strictly within the issue, B; C also settles the `#region` part of the spec author's follow-up 1.

### 3 · Code fixes — a fix that formats a type with a region pair (new; the change doc must list it)

E.g. RH7105 · Declaration keywords must follow order.

Input:

```csharp
namespace N
{
    static public class C
    {
        #region    Bar

        private static int _a;

        #endregion // Bar
    }
}
```

Before (`main`):

```csharp
namespace N
{
    public static class C
    {
        #region    Bar

        private static int _a;

        #endregion // Bar
    }
}
```

After, A (recommended): as before, but `#region Bar`.

After, B: as before (the fix keeps the separator).

Why A: the fix's formatted node then reads as whole-document formatting writes it; the same fixes already capitalize `#region    bar` to `#region Bar` today, and B would need a node-level exception (FC-028). Same for RH7104, RH7106, RH8305, RH8307, RH8309, RH5110 and member-level fixes whose formatted member holds a region pair.

### 4 · Code fixes — a `#region` in a formatted member's leading trivia whose `#endregion` lies outside that member

Only an output question if the spec keeps its current wording; it disappears if the spec pins reading A (see *Internal choices*).

Input (RH3202 · Expression-style methods should not be used):

```csharp
public class C
{
    #region    Bar

    public int M() => 1;

    #endregion // Bar
}
```

Before (`main`): the fix writes the block body and keeps `#region    Bar` (verified; `#region    bar` is not capitalized either).

After, A (recommended): unchanged from `main` — `#region    Bar` stays; the next whole-document format writes `#region Bar`.

After, B: the fix also writes `#region Bar`.

Why A: the region phase rewrites only pairs it sees, as for FMT-REGION-001 today; B would make the separator, but not the capitalization, reach a directive whose pair lies outside the formatted node.

## Internal choices

- **Scope of FMT-REGION-007.** Its text says "a `#region` that has a description", with no pairing condition; FMT-REGION-001 has the same wording, but the code rewrites only paired directives. Recommend stating in FMT-REGION-007 (or once for the phase) that it applies to the `#region` directives the phase pairs with an `#endregion` (FMT-REGION-003), which keeps Output question 4 at `main`'s output. Consequence: no visible change beyond the change doc's list.
- **Disabled branches.** The spec diff adds FMT-REGION-007 to FMT-REGION-006 with a second, different reason in its `Why:`. FMT-REGION-007 already carries the `#if false` example. Recommend stating the disabled-branch qualifier inside FMT-REGION-007 and leaving FMT-REGION-006 unchanged (one decision, one `Why:`). No visible difference.
- **FMT-TAB-002 `Uses`.** The example still turns `foo` into `Foo`, which is FMT-REGION-001; the diff drops FMT-REGION-001 from `Uses` and from the prose. Recommend naming FMT-REGION-001 in the prose for `Foo` and keeping it in `Uses`. No visible difference.
- **Change doc.** Replace the `Code fixes` bullet under *Unchanged* with a visible-change section for formatter-delegating fixes (Output question 3), and add question 1/2 outcomes once answered.

## Issue goals

- "`#region` or `#endregion`" (Proposed Behavior): the `#endregion` half only reaches the `#endregion` of a description-less region, because FMT-REGION-002 already rewrites every other one; it changes visible output → Output question 1.
- "Done when" items (issue input, tab case, FMT-TAB-002 example and test): keep.
- Change-doc demand "how it relates to FMT-REGION-002": met by the change doc; keep.
- Unstated in the issue: code-fix output changes (Output question 3) and trailing whitespace (Output question 2).

## Architecture findings

One rule stated in two places, in the run's own spec diff: FMT-REGION-001's last sentence ("When the formatter rewrites a `#region` line, it MUST write it as `#region`, one space, and the trimmed description") and FMT-REGION-007 both state the separator. The spec author kept both on purpose pending Output question 2. Recommend moving the line form into FMT-REGION-007 once question 2 is answered, so FMT-REGION-001 states only the capitalization. No code component is duplicated: `BuildRegionTrivia` stays the single writer, and `TabReplacementPhase` only removes the tab (FMT-PIPE-019) without predicting the width.

## Follow-up candidates

1. **A code fix writes half of a region pair (pre-existing, FC-028 / FC-007).** `FormatNodeInDocumentAsync` restores the target's leading trivia when the target's parent is the root, but the region phase has already paired that trivia's `#region` with an `#endregion` inside the target. RH7104 on a top-level `partial class C` preceded by `#region    bar` whose `#endregion` lies inside the class writes `#endregion // Bar` and keeps `#region    bar` (verified with `scripts/apply-fix.sh` on `9c9ea5c`); RH7301 compares descriptions ordinally, so it would report `bar` against `Bar` after the fix (not run). Reached by RH7104 and every type-level fix on a straddling region in a file without a namespace. Not caused by this run; the separator half behaves the same way after it.
2. The spec author's follow-up 1 (trailing whitespace behind a directive's message text): if Output question 2 is answered C, it narrows to directives other than `#region`.
3. The spec author's follow-up 2 (whitespace between `#` and `region`): agreed as a candidate, low priority.

No new-rule candidate: no rule's construct includes the separator (RH7302's non-goals exclude the spacing), so P11 lets the formatter normalize it alone.
