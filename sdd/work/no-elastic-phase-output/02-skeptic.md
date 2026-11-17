Role: skeptic
Commit: c892bf5
Verdict: proceed

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Every fixture below was written to a scratch directory outside the repository and deleted afterwards.

## Premise

The issue is real as a gap in an invariant. It is not a visible defect: no output changes today, and none can be shown.

- **A caller reaches it.** Two callers reach the one site that adds elastic trivia, the accessor attribute-list merge of the line-break phase (FMT-BREAK-032):
  - Whole-document formatting through the CLI (`FormatCommandHandler` → `FormatSyntaxTree`).
  - Node-level formatting through a code fix: RH5111 → `BlankLineCodeFixUtilities.FormatMemberOnLinesOfItsOwnAsync` → `FormatNodeInDocumentAsync` on the property. Fixture `Prop.cs`, run with `scripts/apply-fix.sh RH5111 <dir>`:

    ```csharp
    using System;

    class AAttribute : Attribute { }
    class BAttribute : Attribute { }

    class C
    {
        int P { get; [param: A] [param: B] set; }
            = 1;
    }
    ```

    The result under LF and CRLF was `int P { get; [param: A, B] set; } = 1;`. `scripts/format.sh` on the same file gives the same member line.
- **Where the elastic trivia comes from.** `AttributeTargetFormattingRewriter.cs:135` calls `MergeAttributeLists(..., AttributeListMergeSeparators.Spaced)`. That leads to `AttributeTargetUtilities.cs:308`, `firstList.WithAttributes(SyntaxFactory.SeparatedList(mergedAttributes))`. That list builds every comma of the merged list from the elements alone, and that includes commas the author wrote inside the first list. The spec author's probe (01) saw these commas carry elastic trivia from `LineBreakPhase` through `CleanupPhase`.
- **No other site.** I searched for other places that add elastic trivia in every phase outside the structural transforms, and in `Reihitsu.Core` code the phases call. I looked for `SyntaxFactory.Token(SyntaxKind…)` without trivia, `SeparatedList(` built from elements alone, `Block(`, `Elastic*`, `NormalizeWhitespace`, `SyntaxRemoveOptions`, `AddElasticMarker`, and the `Add*` list helpers. Every other match writes explicit trivia:
  - `SwitchCaseBraceRewriter.cs:728-742`: the braces get `WithLeadingTrivia`/`WithTrailingTrivia` right after `Token(kind)`.
  - `RegionNamingRewriter`: `PreprocessingMessage`.
  - The documentation phase and `RawStringLineShifts`: `ParseLeadingTrivia`.
  - Everywhere else: `EndOfLine(string)` / `Whitespace(string)`.

  The structural transforms are already held to this by FMT-STRUCT-026 and its tests. This agrees with the probe in 01, which found nothing else on any formatter fixture.
- **The issue's motivating example is stale.** It says a created `get=>""` formats to `get=> ""`. FMT-STRUCT-026 / thoenissen/Reihitsu#1488 removed that source, and FMT-SPACE-017 decides the spaces around `=>` anyway. The accessor-merge comma is the only instance left.
- **Mechanism of the risk (from the code).** `TrailingWhitespaceWriter.NeedsTrailingSpaceNormalization` / `BuildCollapsedTrailingTrivia` (lines 70-120) treat any `WhitespaceTrivia` as whitespace, and a zero-width elastic marker is one. So in a gap no spacing rule covers, a marker becomes one space that the text does not hold. This matches the spec's Why. The merge comma's gaps are both covered: no space before a comma, and FMT-SPACE-008 after it. That is why no output changes. The merge strips trivia from the attributes and refuses comments (`CanMergeAttributeListGroup`), so no comment or line break can sit in those gaps.
- **Code-fix path today (observed result, plus an inference about the cause).** The phase's elastic comma also stands in the node that `FormatNodeInDocumentAsync` returns to the code fix. The attribute-list merge fixes write the same elastic commas without any whitespace (RH5524, `scripts/apply-fix.sh RH5524 <dir>`). Fixture `void M([A] [B] int x) { }` (with `AAttribute`/`BAttribute` declared) gives `void M([A, B] int x) { }` under LF and CRLF. No Reihitsu code writes that space:
  - `unseparatedTokens` at `AttributeTargetUtilities.cs:323` skips a comma whose trailing trivia holds the marker.
  - The runner applies the action through `CodeAction.GetOperationsAsync` (`FixtureRunner.cs:370`) and formats nothing itself.

  So the space comes from Roslyn's code-action post-processing of the elastic trivia. This is an inference from the run and the code, not a separate experiment. It matters for *Breaking inputs* below.

## Mechanism and candidates

_N/A_ as a bug: this is a behavior change with no failing output. For the tests, the candidate set comes from `FormattingPipeline.CreatePhases` (every phase and each sub-phase of the preparation phase, all reported to the observer at `FormattingPipeline.cs:85-131`). The one adding site is `AttributeTargetFormattingRewriter` → `AttributeListShapeUtilities.MergeAttributeLists` → `AttributeTargetUtilities.MergeAttributeListGroup` with `Spaced`.

## Breaking inputs

Failure classes that reach the changed code:

- FC-013 (formatter versus analyzer/code fix) is reached through the shared merge only.
- The class "a phase's tree differs from its text", which is FMT-PIPE-021's concern.

No other catalog class reaches a change that moves no visible output.

Matrix of the accessor merge (axes: owner × accessor-list layout × target × lists merged × commas in the first list × entry point). Every member was run on today's code. Output is correct, idempotent on a second pass, and holds elastic commas:

| Owner | Layout | Target | Lists | First list already has a comma | Entry point | Output today |
|---|---|---|---|---|---|---|
| property | one line | `param:` | 2 | no | CLI, RH5111 fix | `int P { get; [param: A, B] set; }` |
| property | several lines | `param:` | 3 | yes, inside an attribute's arguments `A(1,2)` | CLI | `[param: A(1, 2), B, C] set;` |
| indexer | joined onto several lines | `param:` | 2 | yes, `[param: A, B] [param: C]` | CLI | `[param: A, B, C] set` |
| event | joined onto several lines | `param:` | 2 | no | CLI | `[param: A, B] remove` |
| property / indexer / accessors | both | `typevar:` | ≥2 | — | covered by the 5 tests 01 names | unchanged |

Fixture used for rows 2-4 (`scripts/format.sh <dir>`, run twice, second pass identical):

```csharp
using System;

class C
{
    event EventHandler E { add { } [param: A] [param: B] remove { } }

    int this[int i] { get => 0; [param: A, B] [param: C] set { } }

    int R
    {
        get; [param: A(1,2)] [param: B] [param: C]
        set;
    }
}
```

Output:

```csharp
using System;

class C
{
    event EventHandler E
    {
        add
        {
        }
        [param: A, B] remove
        {
        }
    }

    int this[int i]
    {
        get => 0;
        [param: A, B, C] set
        {
        }
    }

    int R
    {
        get;
        [param: A(1, 2), B, C] set;
    }
}
```

Candidates the test implementer must cover:

1. Every phase output of a document-level run on the matrix rows carries no elastic trivia, with `descendIntoTrivia` and the preparation sub-phases included.
2. A first list that already holds a comma. The author's comma is rebuilt by the same list builder, so "only the commas between merged lists" is not the whole class.
3. A node-level run (the RH5111 path above, or a direct `FormatNodeInDocumentAsync` on the property, as a code fix passes it) whose input is parsed from text, so any elastic trivia in a phase output was added.
4. The outputs above, unchanged.
5. The code-fix boundary:
   - The attribute-list merge fixes RH5502 … RH5531 must keep `[A, B]` (SHARED-ATTR-018). About 16 analyzer-test files already assert `, B]`.
   - Because their space comes from post-processing of elastic trivia (see *Premise*), the shared merge's code-fix separators must not change without these tests noticing. A merge comma with neither elastic trivia nor whitespace would most likely surface there as `[A,B]`. That outcome is inferred, not run.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Formatter, line-break phase (accessor attribute-list merge) | must change: its merge output adds elastic trivia |
| Formatter, every other phase | must stay; no other adding site found |
| `Reihitsu.Core` `AttributeTargetUtilities.MergeAttributeListGroup` | shared by the phase and the code fixes; whatever changes must keep the code fixes' output |
| Attribute-list merge code fixes RH5502 … RH5531 (and their Fix All) | must stay (SHARED-ATTR-018) |
| Code fixes that format a member through the formatter (RH5111 and others) | must stay; output identical |
| Analyzers | not involved |
| Rule pages | not involved |
| CLI | not involved (output unchanged) |
| Tests | `PhaseTreeRoundTrip` observer (issue's Done-when) plus focused FMT-PIPE-034 tests |

## Refuse option

There is no input class to refuse. The issue is about an invariant, not about an input shape, and every input that reaches the accessor merge is ordinary code. Withholding a fix or leaving a construct as written does not apply. Declaring a shape unsupported is not an option: no shape causes this.

The real alternative is **no change** (leave FMT-PIPE unspecified). Its cost is zero lines. What it gives up is that `PhaseTreeRoundTrip` cannot catch a future phase that adds an elastic marker in a gap no spacing rule covers. The change doc rejects that alternative with a sound reason.

The requested change is cheap: one adding site and one observer assertion. **Proceed.**

## Output questions

_None._ No visible output changes in any row above, and the change doc lists none.

## Internal choices

1. **What the phase's merge writes in place of the elastic comma, as a spec reading.** FMT-BREAK-031 forbids the phase to write whitespace in gaps it does not join, and FMT-BREAK-032 says "the space it writes is FMT-BREAK-031's recorded exception". The gaps around a comma the merge *creates* are neither a join nor a gap "on the same line before and after", so the spec does not say whether the phase may write `, ` with an explicit space. The two readings give the same final output, because the spacing phase normalizes the gap (FMT-SPACE-008).
   - Reading (a): the merge writes the comma with no whitespace.
   - Reading (b): the merge writes an explicit space behind it, which widens the recorded exception.

   **Recommend (a)**, plus one sentence in FMT-BREAK-032 saying the commas the merge writes carry no whitespace. That matches FMT-STRUCT-026's "only the separators its text needs" and FMT-BREAK-031's intent, and it keeps the recorded exception from growing silently. Either way, the code-fix merge keeps its own output (change doc, *The code fixes' merge keeps its output*).
2. **Wording of the second paragraph of FMT-PIPE-034.** "The tree that `FormatDocumentAsync` and `FormatSyntaxTree` pass through the phases is parsed from text" is true for every caller: the CLI and the playground parse files, and `FormatDocumentAsync` has no production caller. As written, though, it reads as a claim about any `Document`. **Recommend** qualifying it as "for every input a caller of the repository passes" (`AGENTS.md`, *Supported inputs*), or leaving it as it is. No output depends on it.
3. **The issue's "no phase returns elastic trivia" versus the spec's "adds".** I agree with the spec author (decision 1 in 00). For the observer assertion, "adds" is checkable as "carries none" whenever the input root carries none, which is the case for `FormatterTestsBase` (it parses the input text, lines 259/283, before `ExecuteAndCompare` at line 353).

## Issue goals

thoenissen/Reihitsu#1516 is `agent-filed`.

- "FMT-PIPE requires that no phase returns elastic trivia": change it to "adds", as the spec did. Visible output is unchanged. For document-level inputs the two readings are equivalent. "Returns" would make phases strip elastic trivia from code-fix nodes, and that is a code-fix output change nobody asked for.
- "The per-phase round-trip check asserts it on every formatter test fixture": keep. It is cheap, and those fixtures are parsed from text.
- "The formatter test suite keeps its outputs": keep. Add the code-fix suites, RH5502 … RH5531 and the formatter-backed fixes, which must keep theirs too.
- The issue's example (`get=>""` → `get=> ""`) is stale and should not be used as a test. The spec's accessor example replaces it.

## Architecture findings

_None._ One shared helper (`MergeAttributeListGroup`, `Spaced`) serves two consumers with different needs:

- The code fixes, whose comma space appears to come from Roslyn's post-processing of elastic trivia.
- The phase, which must now add none.

That is a parameter choice inside one implementation, not two owners of one decision. The change doc already lets the two diverge.

## Follow-up candidates

- **The attribute-list merge code fixes appear to depend on Roslyn's code-action post-processing for the space after the comma** (RH5524 run above: Reihitsu writes no space, and the result holds one). There is no visible defect: SHARED-ATTR-018's `[A, B]` holds in the runner and in the tests. It does put layout of a visible result in a formatter other than Reihitsu's.

  **Recommendation: no issue**, unless the implementer's change has to touch these separators anyway. In that case the lead may want the code-fix comma written explicitly in the same run, as an adjacent fix with a test, because nothing visible moves.
