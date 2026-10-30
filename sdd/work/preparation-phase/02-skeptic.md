Role: skeptic
Commit: 8e0d91a
Verdict: proceed

My prompt carried no intended fix, suspected cause, or hoped-for verdict. All statements below come from the code at `8e0d91a`, whose production code matches `origin/main` (`66fd1d7`), and from runs of `scripts/format.sh` and `scripts/trace.sh` on fixtures in a temporary directory outside the repository.

## Premise

Real. Formatting the issue's class with `scripts/format.sh` (CLI, document level), with every line break replaced by the terminator in the first column:

| Line breaks of the input | `#region A` | Blank lines between `#endregion // A` and `#region B` | Matches the LF output |
|---|---|---|---|
| `\n` | column 4 | 1 | yes (reference) |
| `\r\n` | column 4 | 1 | yes |
| lone `\r` | column 0 | 1 | no |
| `\r\n`, `\n`, lone `\r` in turn | column 0 | 1 | no |
| U+0085 | column 4 | 2 | no |
| U+2028 | column 4 | 2 | no |
| U+2029 | column 4 | 2 | no |

Every output uses `\n` only. The spec author's handover reports U+0085 and U+2028; U+2029 fails the same way. FMT-PIPE-020 covers all of them.

How the change doc and the issue describe today's writers, checked against the code:

- `ControlFlowBraceTransform` is the one writer whose output, reached by a test input, holds a line ending other than the detected one in an LF file. `scripts/trace.sh` on an LF file with `if (a)` / `// c1` / `// c2` / `Foo();` / `else` / `/* e */` / `Bar();` shows the last phase changing `CRLF=5, LF=11` to `LF=16`. That is one `\r\n` per leading comment plus one behind each opening brace (`ControlFlowBraceTransform.cs:91` and `:116`). The transform handles `if` and `else` only (`VisitIfStatement`). Its closing-brace line break (`:84`) uses the context value.
- `BlankLineTokenCleanupRewriter` sets `endOfLineText = Environment.NewLine` at `:151` and `:287`, but no input can write that value. Both methods return early when they find no end-of-line trivia, and every path that writes a line break first overwrites the variable with the text of an end-of-line trivia from the input (`:161`, `:294`). The method writes a copy of an input line break, not `Environment.NewLine`.
- `DocCommentElementNormalizer.GetLineBreak` (`:102-107`) copies the start-tag line's break from the source text. After the preparation phase that break is the detected line ending. Its `Environment.NewLine` fallback is reached only when the start tag's line is the last line of the root's text.

So the issue's statement that two components "write other line endings" is accurate for `ControlFlowBraceTransform` only. Today, the other two are copies of input line breaks, which the preparation phase makes compliant. This does not change the target behavior.

Severity: the user-visible defect needs a file whose line breaks are a lone `\r` or a Unicode terminator. Such files are rare. The change's weight is structural: every phase gets one line-ending form, and a per-phase check replaces a final repair. The maintainer has approved that as the change doc. I see no reason to narrow it.

One limit of the premise, which FMT-PIPE-020 states correctly but the change doc's *What* sentence ("A file saved with a lone `\r` after every line gets the layout of its LF copy") reads more broadly. A file whose multi-line comment or disabled text spans lines keeps those inner breaks (FMT-EOL non-goal, thoenissen/Reihitsu#959), and those inner breaks already decide layout today; see *Breaking inputs*, defect 2. So a lone-`\r` file containing such a comment will still not get the layout of the same file saved with LF after this change. It gets the layout of FMT-PIPE-020's copy, whose comment keeps its `\r`. This needs no decision for this run: the requirement is unambiguous and the maintainer scoped those breaks out. The pull request should say so.

## Mechanism and candidates

_N/A_ as a bug of the run (behavior change). Two defects sit inside the run's subject. Their mechanisms are stated with their class under *Breaking inputs*.

## Breaking inputs

Failure classes that reach the change: FC-010, FC-012, FC-015, FC-016, FC-017, FC-020, FC-021, FC-022, FC-028, FC-031, FC-032, FC-033, FC-034.

### Defect 1 — layout depends on the terminator of a line break that FMT-EOL rewrites (the issue)

Mechanism: some components decide line structure from the text, and the text's line breaks disagree with what the tree holds. Two forms:

- A phase writes `\n` directly behind an input lone `\r`. The text then holds one `\r\n` line break where the tree holds two, and line numbers computed through the text are off.
- A component tests line-break characters itself and recognizes only `\r`/`\n`, while Roslyn and `SourceText` recognize five terminators. Examples: `FieldDeclarationSplitTransform.cs:123` (`EndsWith("\n")`), `BlankLineTriviaUtilities.cs:29`, `ChainLineBreakRewriter.cs:130`, `SwitchCaseWhenLineBreakRewriter.cs:58`, `IndentationRewriter.CountEmbeddedNewLines` (`:291-295`, counts `\n` only), `TokenGapAnalysis.cs:283-290` and `FormattingTextAnalysisUtilities.cs:383` in `Reihitsu.Core`. `ListElementInteriorJoinRewriter.cs:227`, `UsingTrailingTriviaBuilder.cs:162`, and `DocumentationCommentUtilities.cs:373` recognize all five.

Class matrix, checked against FMT-PIPE-020. The carrier column says where the foreign break stands.

| Carrier | `\n` | `\r\n` | lone `\r` | U+0085 | U+2028 | U+2029 |
|---|---|---|---|---|---|---|
| End-of-line trivia, whole file (the issue's class) | holds | holds | fails | fails | fails | fails |
| End-of-line trivia, mixed with `\r\n`/`\n` | — | — | fails | not run | not run | not run |
| Documentation comment newlines only (LF file, `// c` above `/// <summary>` / `/// A` / `/// </summary>` / `int x;` in a class) | holds | not run | fails: `// c` and the doc lines at column 0 instead of 4 | not run | not run | not run |

FMT-PIPE-020 covers every failing cell, because the preparation phase rewrites these carriers. The documentation-comment row is not in the spec author's example list. It is a separate FMT-PIPE-020 case: lone `\r` (or a Unicode terminator) only inside a documentation comment, in an otherwise LF file.

Inputs the test implementer must cover:

- FMT-PIPE-020, document and tree entry points:
  - the issue's two-region class exactly as shown (FC-034), under lone `\r`, U+0085, U+2028, U+2029, and mixed `\r\n`/`\n`/lone `\r`;
  - a tie (two `\r\n`, two lone `\r`), which detects `\r\n`;
  - the documentation-comment carrier above.

  Second pass on every output (FMT-PIPE-004): today the lone-`\r` output moves `#region A` on the second pass. The helper `NormalizeLineEndings` (`FormatterTestsBase.cs:107`) rewrites `\r\n` and `\n` but leaves a lone `\r` and the Unicode terminators alone. Run under its CRLF variant, a fixture may then hold both lone `\r` and `\r\n`, which is a different, mixed input. The regression must build its input with explicit terminators (FC-033).
- FMT-PIPE-019, per phase: every phase run alone through `ExecutePhases` on LF and on CRLF input:
  - Classify each line break by its trivia or token: end-of-line trivia, the line break that ends a directive, documentation newlines. Literals, other comments, and disabled text are exempt. A whole-text scan cannot tell those apart.
  - Count all five terminators. `AssertUsesLineEnding` (`FormatterTestsBase.cs:119-131`) checks only `\r` and `\n` (FC-017).
  - `Environment.NewLine` is `\n` on the Linux CI, so a fallback that writes it shows only under CRLF input (FC-010, FC-017).
  - Inputs that reach `ControlFlowBraceTransform`'s writes: one and two leading `//` comments, a `/* */` leading comment, an `else` branch with a leading comment, and a nested `else if`.
  - The FMT-PIPE-019 example (`if (a)` / `// leading` / `Foo();`) under LF and CRLF.
  - Directives (`#region`, `#if`/`#else`/`#endif` with `#if false` disabled text) and comments in the gaps the phases edit (FC-020, FC-021).
- FMT-PIPE-019, tabs: the three tests tagged FMT-PIPE-016 in `PhaseColumnOwnershipTests` (`:140`, `:166`, `:189`) retag to FMT-PIPE-019. Their phase-name assertions (`:151`, `:176`) and `ObservedPipeline.cs:53` assert `TabReplacementPhase` as the first observed name, which becomes `PreparationPhase/TabReplacement`.
- FMT-PIPE-001: observed names and order (twelve) through `Execute`. `TabReplacementPhase` and `LineEndingNormalizationPhase` each run alone through `ExecutePhases` are reported under their sub-phase names. `PreparationPhase` run alone reports both. `FormattingPipelineTests.cs:180` lists the old order.
- Kept line breaks stay: FMT-PIPE-020's copy keeps them, and FMT-PIPE-019 exempts them. A lone `\r` and U+2028 inside a verbatim string, inside a multi-line raw string, inside a `/* */` comment, and inside `#if false` disabled text, in an LF file, must keep their characters (FC-022). I ran the literal cells: today they keep their characters and get the LF layout.
- Node level (FC-028): a target of `FormatNodeInDocumentAsync`, of `FormatNodeInDocumentWithContextAsync`, and of `FormatNode` that holds a line break other than the detected one, for example one `\r\n` inside a target in an LF document. Today such a root stays attached to its document until the first phase that rewrites it. After the change the LineEndings sub-phase rewrites it at step 1.2, so it is detached before every other phase. The change doc's sentence "a node-level root stays attached to its document exactly as long as it does today" holds only for targets without such a line break. The phases are written to work on a detached root (`PrecedingTokenFacts.Resolve`, `IndentationPhase.cs:26-29`), so I expect no column change, but no test shows it. Cover FMT-PIPE-017 and FMT-PIPE-018 columns and the blank lines above the target for such a target. The FMT-PIPE-017 tests and `ByteOrderMarkTests` must pass unchanged (issue, Done when).
- The phase-level stability check (`PhaseStabilityCheck`) runs whatever `FormattingPipeline.CreatePhases` returns, read through reflection, and reports changes by phase name. Today's `ControlFlowBraceTransform` write followed by the final normalization is an FC-012 pair that this check does not see, because it runs each phase on the final output. That is why the per-phase line-ending test is needed in addition. `PhaseReversalExceptionList` names no preparation phase, so no entry is affected by the reorder.
- FC-032: code fixes reach the change through `FormatNodeInDocumentAsync`, so analyzer test results gathered before the implementation are not evidence after it.

### Defect 2 — a line break kept inside a multi-line comment or disabled text decides layout (outside the run)

Mechanism, inferred from the matrix (exactly the terminators that contain no `\n` fail) and not traced further: `IndentationRewriter.CountEmbeddedNewLines` counts only `\n` in the trivia of a line, while the layout numbers lines by every terminator. Any other terminator kept inside a trivia's text makes the rewriter assign the trivia in front of it, or the trivia itself, to the wrong line. Matrix, all outputs from `scripts/format.sh`; the method body is indented 8:

| Kept terminator inside | `\n` | `\r\n` | lone `\r` | U+0085 | U+2028 | U+2029 |
|---|---|---|---|---|---|---|
| `/* a…b */` starting its line | holds | holds | fails: comment at column 0 | fails: column 0 | fails: column 0 | fails: column 0 |
| `/* a…b */` starting its line, `// c` above it | holds | not run | `// c` holds at column 8; the block comment fails at column 0 | not run | not run | same as lone `\r` |
| `// c` above `#if false` disabled text | holds | not run | fails: column 0 | not run | fails: column 0 | fails: column 0 |
| `/* a…b */` behind code on its line | — | — | holds | — | — | — |
| verbatim / raw string literal | holds | — | holds | — | holds | — |

The file's own line ending does not matter: a CRLF file with a lone `\r` in the comment fails the same way. The preparation phase does not reach these breaks (FMT-EOL non-goal, thoenissen/Reihitsu#959). FMT-PIPE-020 stays true for them by construction, because its copy keeps the same breaks. This run neither introduces nor depends on the defect, so it is a follow-up candidate (below), not a run defect.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `FormattingPipeline` (`CreatePhases`, `ExecutePhases`, observer names) | must change: preparation first, `LineEndingNormalizationPhase` not last, sub-phase names reported (FMT-PIPE-001) |
| `PreparationPhase` (new type, named code owner in `sdd/specs/README.md`; `SpecConsistencyTests` fails until it exists) | must change |
| `TabReplacementPhase` | rules must stay (FMT-TAB); reported name changes |
| `LineEndingNormalizationPhase` | rules must stay (FMT-EOL); position changes; must keep returning its input node when it rewrites nothing (it does today) |
| `ControlFlowBraceTransform` (`:91`, `:116`) | must change: the only reached writer of a foreign line ending |
| `DocCommentElementNormalizer.GetLineBreak` | involved: copies a normalized break after the change; `Environment.NewLine` fallback only on the root text's last line |
| `BlankLineTokenCleanupRewriter` (`:151`, `:287`) | involved, no foreign write reachable today (see *Premise*) |
| Every other line-break writer (`SyntaxFactory.EndOfLine(context.EndOfLine)` or a copy of input trivia) | must stay |
| `ReihitsuFormatterHelpers.ComputeDocumentColumn` | see *Architecture findings*; FMT-PIPE-017 columns must stay |
| `ReihitsuFormatter` entry points | not involved: all five call `FormattingPipeline.Execute`; line-ending detection (`LineEndingUtilities`, FMT-PIPE-007) must stay |
| `Reihitsu.Cli` `PipelineTraceCommand` | must stay: it prints the observer's names (`:267-318`); `PipelineTraceCommandTests.cs:153` and `:188` assert `LineEndingNormalizationPhase` and must change |
| Formatter tests: `PhaseColumnOwnershipTests`, `ObservedPipeline`, `FormattingPipelineTests`, `PhaseOwnershipDocumentationTests`, `PhaseStabilityCheck` | must change or be checked, as listed under *Breaking inputs* |
| `DocumentationCommentFormattingPhaseTests.cs:145-147` | test comment "normalized later by `LineEndingNormalizationPhase`" becomes stale (FC-031); the test stays valid |
| Comments that name the old order (FC-031) | `FormattingPipeline.CreatePhases` summary, `LineEndingNormalizationPhase` summary, `ReihitsuFormatterHelpers.cs:136-140`; "first phase" in `CleanupPhase.cs:15`, `RawStringLineShifts.cs:266`, and `LayoutComputer.cs:342` is still true of the TabReplacement sub-phase |
| Analyzers, code fixes, Fix All | not involved in code; reached through the node-level entry points (FC-032) |
| `Reihitsu.Core` | not involved |
| Rule documentation (`documentation/rules/`) | not involved: no rule page names the phase order |
| `AGENTS.md`, `scripts/README.md` | changed by the spec author; see the note below |

Notes on the spec diff for the spec checker. None of them blocks the run.

- The `AGENTS.md` row `PreparationPhase/LineEndings` normalizes "XML-documentation newline tokens" but also says "line breaks inside literals, comments, and disabled text are never touched". Documentation comments are comments, so the row contradicts itself; FMT-PIPE-019 says "a comment that is not a documentation comment".
- FMT-PIPE-017 counts a tab "with the width the preparation phase gives it (FMT-PIPE-019)", but FMT-PIPE-019 names no width; the width is FMT-TAB-001's.
- `cleanup.md` retired FMT-CLEAN-002/003 still say "first phase of the pipeline". That is true of the TabReplacement sub-phase.

## Simpler option

_None._ The maintainer considered and rejected the narrower alternatives: normalize first and last, or normalize last with a no-fusion rule (change doc, *Alternatives considered*).

## Questions for the maintainer

_None._ The kept-break limit under *Premise* needs no choice: FMT-PIPE-020 defines it. The pull request should state it.

## Architecture findings

**The tab rule in a node-level target's leading whitespace is stated twice** (FC-015). The issue reports this; it is not new.

- Components: `TabReplacementPhase.ReplaceWhitespaceTabs` (`TabReplacementPhase.cs:100`) replaces a tab in whitespace trivia with `FormattingContext.IndentSize` spaces. `ReihitsuFormatterHelpers.ComputeDocumentColumn` with `IsInOwnLeadingWhitespace` (`ReihitsuFormatterHelpers.cs:145-166`, `:249-253`) runs before any phase. It counts a tab in the first token's own leading `WhitespaceTrivia` as `SyntaxIndentationUtilities.IndentSize` columns, "because the first phase replaces it" (its summary).
- Shared decision: which tabs in front of a node-level target's first token are replaced, and their width. The entry point predicts the first phase's result instead of measuring it.
- Places: the two above. I found no input on which they disagree today: the width constant is the same (`FormattingContext.IndentSize` is defined as `SyntaxIndentationUtilities.IndentSize`), and both cover exactly the token's own leading `WhitespaceTrivia`. The defect is the duplicate statement, which can drift apart.
- The issue's *Proposed Behavior* asks that this column be "measured with the preparation rule instead of the copy in `ComputeDocumentColumn`", but its *Done when* does not list it. The accepted change doc and the spec diff only reword FMT-PIPE-017. They do not decide whether the copy goes in this run. The maintainer chooses under escalation 12.

## Follow-up candidates

- **A line break kept inside a multi-line comment or disabled text decides layout.** This is defect 2 under *Breaking inputs*. A lone `\r`, U+0085, U+2028, or U+2029 inside a `/* */` comment or `#if false` disabled text puts that comment, or the comment above the disabled text, at column 0, in LF and CRLF files alike. Structural cause: line-break recognition is decided in many places with different terminator sets. `IndentationRewriter.CountEmbeddedNewLines` counts `\n` only, `SourceText` counts all five, and other private copies count `\r`/`\n`. thoenissen/Reihitsu#959, which would normalize those breaks, would make the defect unreachable in whole-file formatting. Its text does not mention the layout effect.
- **`scripts/trace.sh` is not executable.** It is committed with mode `100644`, while every other script under `scripts/` is `100755`. `scripts/trace.sh <file>` fails with "Permission denied", and only `bash scripts/trace.sh` runs it.
