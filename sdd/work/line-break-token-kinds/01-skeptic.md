Role: skeptic
Commit: 204e0cb37ac54fc5a8c20bcd33df494f2a61d210
Verdict: proceed narrower

The prompt carried no intended fix or hoped-for verdict. `00-lead.md` describes the defect as structural and says no visible output should change. I checked that framing against the code instead of taking it as given.

## Premise

The issue is accurate as a structural statement. It does not report a behavior defect, and the issue says so itself ("All three agree today").

- `src/Reihitsu.Core/LineEndingUtilities.cs:31-65` (`DetectEndOfLine`) counts `SyntaxKind.EndOfLineTrivia` from `DescendantTrivia(descendIntoTrivia: true)` and `SyntaxKind.XmlTextLiteralNewLineToken` from `DescendantTokens(descendIntoTrivia: true)`.
- `src/Reihitsu.Core/LineEndingUtilities.cs:89-108` (`NormalizeLineEndings`) uses the same two kinds and the same two traversals. It rewrites the tokens with `ReplaceTokens` and `SyntaxFactory.XmlTextNewLine`, and the trivia with `ReplaceTrivia` and `SyntaxFactory.EndOfLine`.
- `src/Reihitsu.Analyzer/Rules/Layout/RH5604CodeMustNotContainMixedLineEndingsAnalyzer.cs:55-71` uses the same two kinds and the same two traversals. At `:78` it then calls `DetectEndOfLine(root)`, which walks the tree a second time.

There are three statements of one carrier set (FC-015). This is the "one rule is stated in more than one place" case of `squad/README.md`, *Architecture findings*. Today no input separates the three statements: all three test the same kinds with the same `descendIntoTrivia: true` walks of the same tree. No reproduction exists, because no output is wrong. In my reading, escalation 4 ("a bug does not reproduce") is meant for a report that claims wrong output. This issue claims none, so its premise is the code structure, and the code shows it. The lead decides whether escalation 4 applies; I recommend it does not.

Reachability: all three statements run for the repository's own callers. The analyzer runs in every compilation. `NormalizeLineEndings` runs from the RH5604 code fix (`RH5604CodeMustNotContainMixedLineEndingsCodeFixProvider.cs:42`) and from whole-document formatting and the CLI (`LineEndingNormalizationPhase.cs:26`). `DetectEndOfLine` runs from every formatter entry point and from about 30 code fixes.

Run to confirm that the three agree on every carrier the dispatch admits (fixtures below):

```shell
scripts/apply-fix.sh RH5604 <scratch>/eol   # analyzer, then code fix, then re-analysis; LF and CRLF arms
scripts/format.sh <copy of scratch>/eol     # whole pipeline through the CLI
```

`CODE-FIX RUN: 8 fixture(s) x 2 endings — 14 fixed, 0 no-fix-offered, 2 no-diagnostic, 0 not-converged, 0 no-progress, 0 analyzer-failure, 0 parse-error, 0 invalid-result, 0 line-ending-drift`

The two `no-diagnostic` arms are `RawInterpolationText.cs`, which is expected under RH5604-003. After `format.sh`, the only fixture that still holds U+2028 is `RawInterpolationText.cs`. The formatter rewrote every other carrier, so the normalization and the analyzer agree on all eight shapes.

Each fixture is LF except for one U+2028 (written `⟨2028⟩` below; written byte-exact with `python3 -I`, checked with `rg -c $' '`). U+2028 is used because `apply-fix.sh` turns a lone `\r` into `\n` (`FixtureLineEndings.Normalize`), which would hide that case (FC-033):

```text
Cdata.cs               ///     <![CDATA[a⟨2028⟩    /// b]]>                 -> reported, fixed
XmlComment.cs          ///     <!-- a⟨2028⟩    /// b -->                    -> reported, fixed
InsideTag.cs           ///     <para⟨2028⟩    /// foo="a">x</para>          -> reported, fixed
AttributeValue.cs      ///     <para foo="a⟨2028⟩    /// b">x</para>        -> reported, fixed
DelimitedCdata.cs      /**  * <summary><![CDATA[a⟨2028⟩     * b]]></summary> */ -> reported, fixed
InterpolationHole.cs   string s = $"{1 +⟨2028⟩        2}";                 -> reported, fixed
RawInterpolationHole.cs  $"""  {1 +⟨2028⟩ 2}  """ (hole)                    -> reported, fixed
RawInterpolationText.cs  $"""  a⟨2028⟩ {1}  """ (literal text)              -> no-diagnostic; formatter keeps the U+2028
```

Full fixture shape, for example `InsideTag.cs`:

```csharp
class C
{
    /// <summary>
    /// <para⟨2028⟩    /// foo="a">x</para>
    /// </summary>
    int A;
}
```

The code-fix diff for every reported arm splits the line at the U+2028 into two lines in the arm's line ending. `InsideTag.cs [LF]` gives:

```diff
-    /// <paraU+2028    /// foo="a">x</para>
+    /// <para
+    /// foo="a">x</para>
```

## Mechanism and candidates

Mechanism: the set of line-break carriers (which trivia and which tokens count as a line break that gets counted, reported, and rewritten) is decided independently in three walks of the tree. The three agree only because each copy happens to list the same two `SyntaxKind`s with the same traversal flags.

Dispatch: the kind tests at `LineEndingUtilities.cs:33`, `:52`, `:90`, `:100` and at `RH5604...Analyzer.cs:55`, `:65`, over `DescendantTrivia`/`DescendantTokens(descendIntoTrivia: true)`. Where each carrier comes from in the syntax tree:

| Carrier position | Kind the walk sees | Defining syntax |
|---|---|---|
| Between tokens, including inside an interpolation hole of a regular or raw interpolated string | `EndOfLineTrivia` | token trivia |
| End of a directive (`#pragma`, `#region`, `#if`, `#warning`, …) | `EndOfLineTrivia` | `DirectiveTriviaSyntax.EndOfDirectiveToken` trivia (structured) |
| Inside an XML element tag of a doc comment (`<para⟨eol⟩ foo=…>`) | carried; observed rewritten (`InsideTag.cs`) | trivia or token inside `DocumentationCommentTriviaSyntax` |
| Doc-comment text, CDATA, XML comment, attribute value, in `///` and `/** */` | `XmlTextLiteralNewLineToken` | `XmlTextSyntax`, `XmlCDataSectionSyntax`, `XmlCommentSyntax`, `XmlTextAttributeSyntax` text tokens |

The table does not claim which of the two kinds Roslyn uses in the tag row. I did not run a kind dump, and the observed output is the same either way.

Not carriers, and they must stay not carriers (RH5604-003, FMT-EOL-004, the RH5604 non-goals):
- string-literal text: verbatim, raw, interpolated verbatim and interpolated raw text parts;
- `/* */` comments that are not documentation;
- disabled text.

## Breaking inputs

The change keeps output unchanged, so what can break is a carrier that the shared definition gains or loses compared with today.

Failure classes that reach the change:
- **FC-015 / FC-016.** The issue's own class. Every other production statement of the carrier set has to go through the shared definition. I searched the production code for every use of `XmlTextLiteralNewLineToken` and for `EndOfLineTrivia` compared against a line ending. The only statements of the carrier set are the three the issue names. The other matches decide something else (see *Surfaces*).
- **FC-017.** A test that only uses CRLF↔LF is weaker than the guarantee. RH5604-001/005 and FMT-EOL-001 also cover lone `\r`, U+0085, U+2028 and U+2029.
- **FC-033.** `apply-fix.sh` normalizes a lone `\r`. Use a Unicode terminator there, or a unit test that keeps the bytes.
- **FC-040** (documentation mode). Whether `///`/`/** */` text is structured trivia, and so carries `XmlTextLiteralNewLineToken`, depends on `DocumentationMode`. The three components read the same tree, so the refactor cannot make them disagree on mode. I did not run anything under `DocumentationMode.None` and claim nothing about it (see *Follow-up candidates*).

Matrix of what the refactor can change. Axis 1 is the carrier position from the table above (4 rows) plus the non-carriers (3 rows). Axis 2 is the consumer: detection, analyzer report, normalization. Axis 3 is the terminator: `\r\n`, `\n`, lone `\r`, U+0085, U+2028, U+2029. Today every carrier cell is "counted / reported / rewritten" and every non-carrier cell is "ignored". I confirmed this for all eight fixtures above on the analyzer, the fix, and the formatter, with U+2028. Existing tests already pin lone `\r` and the Unicode terminators between tokens, at a directive end, and inside doc comments (`RH5604...AnalyzerTests.VerifyForeignTerminator*`, `ReihitsuFormatterLineEndingTests.OtherLineTerminatorsBetweenTokensAreRewritten`).

Inputs the test implementer must cover, as characterization tests. They are not reproductions: they pass on `main` and have to keep passing.
1. The carrier positions no existing test names: a line break inside CDATA, inside an XML comment, inside an element tag, and inside a multi-line attribute value of a doc comment; CDATA in a `/** */` comment; a line break inside an interpolation hole of a regular and of a raw interpolated string. Each is reported by RH5604 and rewritten by the fix and by the formatter. Use the fixtures above.
2. A raw interpolated string whose literal text holds a foreign terminator: not reported, not rewritten (the `RawInterpolationText.cs` fixture).
3. The analyzer's "mixed" decision versus the detection's "predominant" decision. These are two different rules over the same carrier set, and they must stay distinct:
   - `RH5604...Analyzer.cs:73` treats line breaks as mixed when there is more than one distinct text.
   - `DetectEndOfLine` puts every single-character break in the `\n` bucket (FMT-PIPE-007).

   Observed:
   - A file whose every line break is U+2028 is **not reported** by RH5604 (`apply-fix.sh`: `AllU2028.cs [LF] == no-diagnostic`, same for `[CRLF]`).
   - The formatter rewrites that file to LF: `format.sh` turns `'class C⟨2028⟩{⟨2028⟩    int x;⟨2028⟩}⟨2028⟩'` into `'class C\n{\n    int x;\n}'`.
   - A file of `\n` plus one lone `\r` or U+2028 **is reported** (RH5604-001 example; the `*.cs [LF]` arms above).

   If one shared walk were made to decide both "mixed" and "predominant", these outputs would flip. Pin a file whose line breaks are all one non-CRLF/LF terminator (not reported), and a file that mixes `\n` with a lone `\r` (reported).
4. `DetectEndOfLine` fallback to `Environment.NewLine` when no carrier exists (`ReihitsuFormatterHelpersTests.DetectEndOfLineFallsBackToEnvironmentNewLineWhenNoEndOfLineTriviaFound` exists already).

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `Reihitsu.Core` `LineEndingUtilities` (`DetectEndOfLine`, `NormalizeLineEndings`) | Must change: home of the shared carrier definition, and both methods use it |
| RH5604 analyzer | Must change: its carrier selection uses the shared definition. Its distinct-text "mixed" decision (`counts.Count <= 1`) and its per-line location stay its own and stay as they are |
| RH5604 code fix / Fix All | Must stay (calls `NormalizeLineEndings`) |
| Formatter `LineEndingNormalizationPhase`, `ReihitsuFormatterHelpers.DetectEndOfLine`, all formatter entry points | Must stay |
| The ~30 code fixes that call `DetectEndOfLine` | Must stay; output unchanged |
| `LineEndingUtilities.HoldsOtherLineBreak` (SHARED-FIX-008) | Not involved: it decides on text lines on purpose, including literals and comments, because it guards a fix that copies a span as text. It is a different rule, and moving it to the carrier set would change which fixes are withheld |
| `CopyrightHeaderTemplateResolver.NormalizeLineEndings` (RH8402) | Not involved: it compares a header against the file's first text line break. A different decision |
| `RH8308...Analyzer.cs:90` (`XmlTextLiteralNewLineToken`) | Not involved: it reads line structure inside a doc comment, not line endings |
| `TargetWriteBackUtilities.EndTargetLineAsFormatted` | Not involved: it writes the one trailing `EndOfLineTrivia` of a target, not the carrier set |
| Specs RH5604, FMT-PIPE-007, FMT-EOL-* | Must stay: they already state the carrier set (between tokens, directive ends, documentation comments) and the exclusions; no requirement changes |
| Rule page `documentation/rules/RH5604.md` | Not involved |
| Tests | Existing RH5604 and line-ending tests stay with unchanged expectations; characterization tests added as listed above |

## Refuse option

There is no input class to refuse: no output is wrong, so no fix is withheld and no construct is left as written. The cheaper alternative is **no change**: keep the three copies, link the issue with `Refs`, and accept the risk of drift. That risk is small. The set has been stable, and drift needs either a new Roslyn carrier kind or a one-sided edit. The requested change is also small: a shared selection in `LineEndingUtilities` and two call sites. No new type is needed, and it fits well under the 100-line limit of escalation 9. It also removes the analyzer's second walk of the tree. The change is cheap and removes a real FC-015 hazard, so I recommend making it, narrowed as below.

Declaring a shape unsupported: not an option. No shape costs a special case here.

Narrower than the issue: the shared definition covers only *which trivia and tokens are line-break carriers*. It does not merge the analyzer's "mixed" rule (distinct texts) into the detection's "predominant" rule (length buckets), because that would change RH5604 output (breaking input 3). It does not absorb `HoldsOtherLineBreak` or RH8402, because those are different decisions.

## Output questions

_None._ The change brings nothing visible into question as long as the outputs in *Breaking inputs* stay as observed.

## Internal choices

1. **Shape of the shared definition.** The issue proposes "one enumeration of the carrier tokens and trivia". The normalization needs separate token and trivia sets for `ReplaceTokens`/`ReplaceTrivia`, while the detection and the analyzer need only text and position. Two options give the same output:
   - (a) one shared pair of kind predicates (one for trivia, one for tokens), used by all three;
   - (b) one shared enumeration of carriers, which the normalization splits again.

   Recommend whichever keeps the normalization's two-step replacement as it is; no output differs. The "for example" in the issue is a proposal, not a requirement.
2. **Analyzer's second walk.** The analyzer can take the predominant ending from its own counts or keep calling `DetectEndOfLine`. Calling `DetectEndOfLine` keeps FMT-PIPE-007's bucket rule in one place. Computing the ending inside the analyzer would be a fourth statement of that rule (FC-015 again). Recommend: keep calling `DetectEndOfLine`, or have the detection and the analyzer share one counting routine. Do not re-derive the bucket rule in the analyzer.

## Issue goals

- "One shared definition … used by the detection, the RH5604 analyzer, and the normalization": keep.
- "for example one enumeration of the carrier tokens and trivia in `Reihitsu.Core`": a prescribed solution; treat it as optional (internal choice 1). No visible change.
- "The output of the analyzer, the fix, and the formatter is unchanged, and the existing RH5604 and line-ending tests pass with unchanged expectations": keep. Add the characterization tests above, because the existing tests do not name every carrier position the shared definition must keep.
- The issue's "Principle: FMT-PIPE-009" line does not apply: no code fix reaches a phase here. The definition belongs in `Reihitsu.Core` because the analyzer cannot reference the formatter. No visible change.

## Architecture findings

The issue itself is one, and the run is set up to resolve it. Components: `LineEndingUtilities.DetectEndOfLine`, `LineEndingUtilities.NormalizeLineEndings`, `RH5604CodeMustNotContainMixedLineEndingsAnalyzer.OnSyntaxTree`. Shared decision: which trivia and tokens carry a line break. Where the rule is stated: `LineEndingUtilities.cs:33,52,90,100`; `RH5604...Analyzer.cs:55,65`. I found no further production statement of it.

## Follow-up candidates

- **Not settled, no claim made: RH5604 under `DocumentationMode.None`.** RH5604 registers a plain `RegisterSyntaxTreeAction` with no documentation-mode check. Under `None`, `///`/`/** */` may be lexed as ordinary comments, so the carriers inside a `/** */` comment may differ from what the CLI's parse sees (FC-040). I did not run it. Whether it reaches a caller, and whether the outcome differs, is for a separate premise check, not this run: the three components share one tree, so this refactor cannot change it.
- **Spec completeness, internal.** RH5604-002 names only all-CRLF and all-LF files as not reported. A file whose line breaks are all lone `\r` or all one Unicode terminator is also not reported today (observed above), and the formatter rewrites it to `\n` (FMT-PIPE-007 decides this). This is P11, not a defect: the rule's construct is mixed line endings, and such a file is not mixed. A clarifying requirement could be added to RH5604 later. It is not an output question, because today's output follows the rule's stated purpose.
