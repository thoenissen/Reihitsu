Role: spec author
Commit: bc1060b
Verdict: change doc written

The prompt named head `6abe95c`; the branch head is `bc1060b`, which differs from it only in one line of `00-lead.md`. The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Premise check against the current tree

Every statement of thoenissen/Reihitsu#1486 about today's code holds on `bc1060b` (no production change since `f8dc7ef`):

- `RegionNamingRewriter.BuildRegionTrivia` is the only writer of the `#region` separator, and `Rewrite` calls it only when `CapitalizeFirstLetter` changes the description.
- No other phase touches the separator. `scripts/trace.sh` on the issue's input: only `RegionFormattingPhase` (the `#endregion` comment), `BlankLinePhase`, and `CleanupPhase` change it; `#region    Bar` reaches the output unchanged.
- A tab separator: `PreparationPhase/TabReplacement` writes four spaces, which reach the output (`#region    Bar`), as FMT-TAB-002's example states.
- The blocker thoenissen/Reihitsu#1485 is closed.

Further facts traced (all on `bc1060b`, `scripts/trace.sh --passes 2`, every input stable after pass 2):

| Input | Output today |
|---|---|
| `#region    Bar` … bare `#endregion` inside `#if false` | `#region    Bar` … `#endregion // Bar` |
| `#region Bar` + three trailing spaces | trailing spaces kept |
| description-less `#region` … `#endregion    // Stale` | unchanged |
| description-less `#region` … `#endregion//Stale` | unchanged |
| `#  region    Bar` … `#  endregion` | `#  region    Bar` … `#  endregion // Bar` |
| `#region    Bar /* x */` | kept; `#endregion // Bar /* x */` |

Node-level formatting runs `RegionFormattingPhase` (`FormattingPipeline.CreatePhases`), so a code fix that formats a node containing a region pair would write the new separator too. Probes with `scripts/apply-fix.sh`: RH7102 withholds its fix for a type with a region (`no-fix-offered`); RH5204 and RH5417 did not report the probe inputs. No reaching fix found; the change doc says so.

## Changed requirements

- FMT-REGION-007 — One space separates `#region` from its description — added
- FMT-REGION-006 — Regions in disabled code are named too — changed (also applies FMT-REGION-007; one sentence added to its `Why:`)
- FMT-TAB-002 — Tabs inside directives become four spaces each — changed (example: `#region    Bar` becomes `#region Bar`; the `Baz`/tab/`qux` sentence states one space in front of `Baz`)
- `region-formatting.md` — introduction mentions the separator; `Uses` gains FMT-TAB-002
- `tab-replacement.md` — `Uses`: FMT-REGION-001 replaced by FMT-REGION-007 (FMT-REGION-001 is no longer referenced there)
- `requirement-ids.txt` — FMT-REGION-007 inserted after FMT-REGION-006

Change doc: `sdd/changes/2026-11/region-directive-separator.md`, `Status: Draft`. Visible output changes listed: several spaces before an upper-case description; a tab before an upper-case description; the same inside `#if false`.

`scripts/test.sh --project architecture` (spec/coverage filters): 50 passed, 1 failed — `EveryRequirementOfAnEnforcedSpecIsCovered`, only for `FMT-REGION-007: no test covers it`, expected until the tests exist.

No rule page changes: no rule reports the separator, and RH7302-002 keeps `#region  Fields` unreported.

## Examples for the tests

- FMT-REGION-007 — its three examples (the issue's input; one space already, unchanged; the `#if false` case) and the tab case in FMT-TAB-002.
- FMT-TAB-002 — changed example. Existing tests whose expected output is the old behavior: `TabReplacementTests.TabBetweenRegionKeywordAndDescriptionKeepsFourSpacesUnlessTheRegionLineIsRewritten` and `TabReplacementTests.TabInsideRegionDescriptionIsKeptInBothDirectives` (`src/Reihitsu.Formatter.Test/Regression/TabReplacement/TabReplacementTests.cs`) expect `#region    Bar` / `#region    Baz\tqux`.
- FMT-REGION-006 — the `#if false` example of FMT-REGION-007.

## Output questions

### 1 · Formatter — `#endregion` of a region without a description, with several spaces before its text

Within the issue: its proposed behavior says one space between `#region` *or `#endregion`* and the text behind it. Every `#endregion` whose `#region` has a description is already written as `#endregion // <description>` (FMT-REGION-002), so the only `#endregion` lines the issue's `#endregion` half can change are those under a `#region` without a description, which FMT-REGION-004 leaves unchanged, "including any text after that `#endregion`". The spec currently leaves them out of FMT-REGION-007.

Input:

```csharp
class C
{
    #region

    int a;

    #endregion    // Stale
}
```

Before (`main`): unchanged.

After, A: unchanged (FMT-REGION-004 as it stands; the issue's `#endregion` half has no effect).

After, B (recommended):

```csharp
class C
{
    #region

    int a;

    #endregion // Stale
}
```

and `#endregion//Stale` becomes `#endregion //Stale` (the text itself is kept as written).

Why B: it is what the issue asks for and gives every region directive one separator rule; the text FMT-REGION-004 protects is kept, only the whitespace in front of it changes. Requirement IDs: FMT-REGION-004 (would change), FMT-REGION-007 (would extend to `#endregion`).

### 2 · Formatter — trailing whitespace on a `#region` line whose separator is rewritten

Within the issue's case: FMT-REGION-001 writes a rewritten line as `#region`, one space, and the *trimmed* description. Whether a line rewritten only for its separator is trimmed too decides the output. A line that already has one space keeps its trailing whitespace under both readings (today's behavior; see follow-up 1). FMT-REGION-007 is written so that it decides neither reading.

Input (three spaces behind `Bar`, shown as `···`):

```csharp
class C
{
    #region    Bar···

    int a;

    #endregion // Bar
}
```

Before (`main`): `#region    Bar···` unchanged.

After, A: `#region Bar···` — only the separator changes.

After, B (recommended):

```csharp
class C
{
    #region Bar

    int a;

    #endregion // Bar
}
```

Why B: every `#region` line the region phase rewrites then has the one form FMT-REGION-001 already writes, whichever requirement caused the rewrite. Requirement IDs: FMT-REGION-001, FMT-REGION-007.

## Internal choices

- The separator is a new requirement FMT-REGION-007 rather than a change of FMT-REGION-001's last sentence (alternative: widen FMT-REGION-001); FMT-REGION-001 is about capitalization, and the separator has a different reason.
- FMT-REGION-001 keeps its sentence about how a rewritten line is written (alternative: move it into FMT-REGION-007); depends on output question 2 and can move once it is answered.
- FMT-REGION-006 lists FMT-REGION-007 (alternative: leave disabled branches out). The disabled-branch output is listed in the change doc as a visible change, so the maintainer decides it with the others.
- No non-goal for the whitespace between `#` and the keyword or behind the description: both are undecided cases, so they are follow-up candidates, not non-goals.

## Follow-up candidates

1. **Trailing whitespace behind a directive's message text is kept.** `#region Bar` followed by three spaces, and likewise `#warning text` followed by spaces, keep the whitespace through the whole pipeline. FMT-CLEAN's non-goal tracks only a directive line *without* message text (thoenissen/Reihitsu#960) and comments (thoenissen/Reihitsu#959). Readings: keep (today) or remove (`#region Bar`). Recommendation: remove, as FMT-CLEAN-001 does for every other line. Reached through the CLI and whole-document formatting.
2. **Whitespace between `#` and `region`/`endregion` is kept.** `#  region    Bar` … `#  endregion` becomes `#  region    Bar` … `#  endregion // Bar` today (after this change `#  region Bar`). Readings: keep (today; consistent with FMT-TAB-002 keeping the whitespace between other directive tokens such as `#pragma warning    disable`) or write `#region`. Recommendation: low priority; decide together with the general policy for whitespace between directive tokens. Reached through the CLI.
