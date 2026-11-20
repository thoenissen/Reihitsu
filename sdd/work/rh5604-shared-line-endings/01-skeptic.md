Role: skeptic
Commit: aaa4c27
Verdict: proceed

The prompt carried no intended fix, suspected cause, or hoped-for verdict. The issue suggests where the owner could live and that its quadratic cost is shared; this handover settles both as facts (below). It does not take either as given.

## Premise

Real. The duplication is in the source, and both copies are reached.

- `src/Reihitsu.Analyzer.CodeFixes/Rules/Layout/RH5604CodeMustNotContainMixedLineEndingsCodeFixProvider.cs:33-67` (`ApplyCodeFixAsync`) and `src/Reihitsu.Formatter/Pipeline/LineEndings/LineEndingNormalizationPhase.cs:21-46` (`Execute`) run the same two passes, token for token:
  1. They collect every `XmlTextLiteralNewLineToken` whose `Text` differs from the line ending. They replace those tokens with `SyntaxFactory.XmlTextNewLine(leading, eol, ValueText, trailing)` and copy the annotations.
  2. On the result, they collect every `EndOfLineTrivia` (`descendIntoTrivia: true`) whose text differs and replace it with `SyntaxFactory.EndOfLine(eol)`.

  The copies differ only in three ways:
  - **Where the line ending comes from.** The fix calls `ReihitsuFormatterHelpers.DetectEndOfLine(root)`. The phase reads `context.EndOfLine`. For whole-document formatting, `context.EndOfLine` is the same `DetectEndOfLine(root)` (`src/Reihitsu.Formatter/ReihitsuFormatter.cs:73,109`). For `FormatNode` it is detected on the detached node (`:149`).
  - **What comes back when nothing changes.** The fix returns the original `Document`. The phase returns the input node itself, which its XML summary promises.
  - **One extra early return in the fix** for "tokens replaced, no trivia to replace". It returns the same text the general path would.
- Callers that reach the code:
  - The RH5604 code fix and its Fix All (`WellKnownFixAllProviders.BatchFixer`) are offered on every file with mixed line endings.
  - The phase runs as `PreparationPhase/LineEndings` in every whole-document format (CLI, `FormatDocumentAsync`) and in every node-level entry point.
- Baseline: `scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5604"` passed 28 of 28. This covers the analyzer, the code fix, and the formatter tests of RH5604.
- No visible behavior is wrong today. The defect is structural only: FC-015 (a policy has more than one owner). No test can fail first. The "reproduction" in this lane is the existing green test set, which has to stay green with unchanged expectations, plus the coverage gaps listed under *Breaking inputs*.
- `git log` holds only one commit for each file because the clone is shallow, so there is no history evidence of earlier drift.

## Mechanism and candidates

Mechanism: the rewrite of every line break that differs from the detected line ending is implemented twice, once in the RH5604 code fix and once in the formatter's line-ending phase. FMT-PIPE-009 forbids the fix to call the phase. So neither copy can use the other, and a change to one is not seen by the other.

What the copies decide is which syntax elements carry a file's line breaks: `EndOfLineTrivia`, including the trivia that ends a directive, and `XmlTextLiteralNewLineToken`. `rg XmlTextLiteralNewLineToken src -g '!*Test*'` finds that set stated in four places, not two. The fifth hit, RH8308, is unrelated.

| Place | Use of the carrier set |
|---|---|
| `Reihitsu.Core/LineEndingUtilities.DetectEndOfLine` | counts both kinds to find the predominant line ending |
| `RH5604CodeMustNotContainMixedLineEndingsAnalyzer.OnSyntaxTree` | collects both kinds, counts distinct texts, reports the differing ones |
| `LineEndingNormalizationPhase.Execute` | rewrites both kinds (FMT-EOL-001..003) |
| `RH5604CodeMustNotContainMixedLineEndingsCodeFixProvider.ApplyCodeFixAsync` | rewrites both kinds (RH5604-005) |

The issue names only the two rewriters. See *Architecture findings* for the other two.

Candidate owner locations, read from the rule that bans types for code fixes (`src/Reihitsu.ArchitectureTests/Analyzer/CodeFixFormatterAccessTests.cs`, FMT-PIPE-009):

- **A static member on `LineEndingNormalizationPhase`** is banned. Every member of a phase is banned for fixes, as with `TabReplacementPhase.ReplaceWhitespaceTabs`.
- **`ReihitsuFormatterHelpers`** is allowed for fixes, but its summary says "Phases do not use it; the facts they share live in `Reihitsu.Core` and in `Pipeline/Core`". A phase calling it would contradict that statement.
- **`Reihitsu.Core/LineEndingUtilities`** is allowed for fixes, already referenced by the analyzer, fixes, and formatter, and already the home of `DetectEndOfLine`. It is public and needs no new type.
- **A new `ReihitsuFormatter` entry point** is allowed, with precedent in FMT-PIPE-030 and FMT-PIPE-032. It needs FMT-PIPE-009's member list amended (a spec diff), and it brings the guard hazard below.

## Breaking inputs

Failure classes that can reach the change:

- **FC-015 / FC-016:** the class itself, plus its sibling copies (see *Architecture findings*).
- **FC-022:** literals keep their line breaks.
- **FC-043 and the entry-point guards:** if the owner applies the formatter's guards, the fix's result changes. Settled below.
- **FC-009:** a registered action that changes nothing.
- **FC-008:** Fix All. Every RH5604 diagnostic yields the same whole-file rewrite.
- **FC-010 / FC-017:** CRLF idempotency, and LF-only assertions.
- **FC-028:** detached and document roots.
- **FC-033:** a harness that normalizes line endings away. Observed: `scripts/apply-fix.sh` rewrote a lone `\r` in a fixture before running the fix. Tests for this issue must pass byte-exact strings, as the existing RH5604 tests do.
- **FC-031:** the phase's XML summary describes the rewrite it would no longer contain.

FC-040 (documentation mode) affects both copies the same way, because each rewrites the tree it is handed. One owner leaves it as it is.

The axes along which the owner's result can differ from today's:

| Axis | Members | Covered today? |
|---|---|---|
| Carrier | code `EndOfLineTrivia`; directive-ending `EndOfLineTrivia`; `///` newline token; `/** */` newline token | fix: all four (`VerifyMixedLineEndingsAreDetectedAndFixed`, `VerifyLfAtTheEndOfADirectiveIsDetectedWhenCrLfIsPredominant`, `VerifyMixedLineEndingsInsideXmlDocumentationAreDetectedAndFixed`, `VerifyMixedLineEndingsInsideDelimitedXmlDocumentationAreDetectedAndFixed`); phase: formatter tests |
| Foreign terminator | `\n` in CRLF; `\r\n` in LF; lone `\r`; U+0085; U+2028; U+2029 | fix: `\n`, `\r\n`, lone `\r` in a doc comment only. **No fix test covers a lone `\r` in code trivia or any Unicode terminator.** Phase: all six (`ReihitsuFormatterLineEndingTests`, `PhaseLineEndingOwnershipTests`) |
| Kept line breaks | verbatim, raw, interpolated verbatim literal; multi-line comment; disabled text | fix: all (`VerifyLineEndingsInside*`, `...StaysExemptWhenTheSameEndingIsReportedElsewhere`) |
| Predominance | CRLF wins; LF wins; tie, in either order | fix: all |
| Nothing to rewrite | the phase returns its input reference; the fix returns its `Document` | phase: identity is promised in its summary and observed by the pipeline. Fix: unreachable, because the analyzer reports only when two texts occur, so at least one line ending differs from the predominant one |
| Formatter guards | file holding an unsupported shape; file with a syntax error | **settled below** |
| Root kind | compilation unit; detached node (`FormatNode`); node-level context root | phase: through its entry-point tests |

**Settled 1: the code fix handles a Unicode terminator today.** The command was `scripts/apply-fix.sh RH5604 <dir>`. The fixture `U2028.cs` was written byte-exact with Python. `X;` ends with U+2028 and every other line ends with `\n`:

```
internal class C
{
    internal int X;<U+2028>    internal int Y;
    /// <summary>
    /// Value
    /// </summary>
    internal int Z;
}
```

Observed: `fixed (1 action(s), 1 iteration(s))` under both LF and CRLF, `line-ending-drift 0`. The U+2028 became the arm's line ending, and `internal int Y;` moved to a line of its own. The test implementer should pin this, together with a lone `\r` in code trivia, in the RH5604 fix tests. Today no fix test covers it.

**Settled 2: the code fix normalizes a file that the formatter refuses as unsupported.** The fixture `Unsupported.cs` was written the same way, with U+2028 after `X;` and `\n` elsewhere:

```
internal class C
{
    internal int /* a */ X;<U+2028>    internal int Y;
}
```

- `scripts/apply-fix.sh RH5604 <dir>` returns `fixed` under LF and CRLF. The U+2028 is rewritten, and the comment stays.
- `scripts/format.sh <dir>/Unsupported.cs --dry-run` returns `unsupported syntax: a comment between a variable declaration's own tokens. The file was left unchanged.`

So an owner reached through a formatter entry point that applies the precheck would withhold the fix on such files, which is a visible change. The same holds for an entry point that applies the auto-generated guard. Whatever owner is chosen, it must apply none of these guards, or the fix's output changes. This file is the breaking input to pin.

A syntax-error fixture (`internal int X` without `;`) could not be settled. The fix runner reports `parse-error` and does not run the fix. By the source, `ApplyCodeFixAsync` has no syntax-error guard. I make no claim about its result.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH5604 analyzer | must stay. It is a sibling statement of the carrier set; see *Architecture findings* |
| RH5604 code fix | must change: it delegates to the owner. Output unchanged, including on files that hold an unsupported shape |
| RH5604 Fix All | must stay (`BatchFixer`) |
| `LineEndingNormalizationPhase` | must change: it delegates to the owner and keeps returning its input reference when nothing changes. Its XML summary must still be true (FC-031) |
| `Reihitsu.Core` | must change if the owner lives there (see *Internal choices*); otherwise not involved |
| `ReihitsuFormatter` public members | not involved, unless the lead picks the entry-point option, which also needs a FMT-PIPE-009 spec diff |
| Specs | `sdd/specs/formatter/line-endings.md` and `sdd/specs/analyzer/RH5604.md` list `Code owners`. They may need the new owner added as a clarification; that is for the spec checker. No requirement changes, and no change doc (no visible output change) |
| Rule doc `documentation/rules/RH5604.md` | not involved |
| Architecture tests | must stay green (`CodeFixFormatterAccessTests`) |
| Existing tests | must stay unchanged (issue's *Done when*) |

## Refuse option

- **Withhold or leave as written:** not applicable. No input class misbehaves, so there is nothing to refuse.
- **Declaring a shape unsupported:** not an option. No syntax shape is involved; the defect is a duplicated implementation.
- **No change:** costs 0 production lines and leaves two copies of about 20 lines each. The copies are identical today, and both are covered by tests that would catch drift in output (RH5604 fix tests and FMT-EOL tests), though not drift of intent.
- **Requested change:** one shared method and two call sites, likely a net reduction in production lines, with no new type. Escalation 9 does not apply: it is the issue's own fix, under 100 lines, and adds no type.

Rarity is not applicable: the class is "every mixed-line-ending file". Recommendation: proceed. The change is small and removes FC-015 at a policy the formatter and a fix both own.

## Output questions

_None._ Both readings of every option keep the fix's and the formatter's output, provided the owner applies no formatter guard (see Settled 2).

## Internal choices

1. **Where the owner lives.**
   - (a) `Reihitsu.Core/LineEndingUtilities`, next to `DetectEndOfLine`, taking the root and the line ending. FMT-PIPE-009 allows it explicitly. It needs no spec amendment beyond `Code owners`, and it matches `ReihitsuFormatterHelpers`' own statement that shared facts live in Core.
   - (b) A new `ReihitsuFormatter` entry point. It follows the FMT-PIPE-030/032 precedent ("a change to the phase is seen as a change to the fixes"), but it amends FMT-PIPE-009, adds a public member, and must skip every guard the other entry points apply (Settled 2).
   - (c) `ReihitsuFormatterHelpers`. It contradicts that class's summary that phases do not use it.
   - A member on the phase is banned.

   **Recommendation: (a).** It is the cheapest, carries no guard hazard, and the issue's motivation is satisfied: one statement used by both.
2. **Signature of the owner's "nothing changed" result.** The phase needs its input reference back when nothing changes, so it can keep its summary's promise. The fix may keep its own `Document` early return. **Recommendation:** the owner returns its input node unchanged by reference when nothing differs. The fix compares references or keeps its current behavior; this path is unreachable through the fix anyway.

## Issue goals

The issue is `agent-filed`.

- "Normalization written once and used by both": keep.
- "Existing RH5604 and line-ending tests pass with unchanged expectations": keep.
- "No code-fix code calls a phase or transform directly": already true today and enforced by `CodeFixFormatterAccessTests`. Keep it as a constraint; it is no work item.
- The paragraph on quadratic `ReplaceTokens`/`ReplaceTrivia` cost: not a *Done when* item. Drop it from the run's goals; it changes no visible output.
  - Measured with the CLI: `scripts/format.sh <dir> --check` on generated files of one class with N fields, either every line `\r\n` ("uniform") or alternating `\r\n`/`\n` ("mixed", half the line endings foreign).
  - 5000 lines: uniform 7.0 s, mixed 6.9 s. 20000 lines: uniform 74.1 s, mixed 89.6 s.
  - The roughly 15 s gap at 20000 lines fits superlinear cost but is not isolated to the phase, because the whole pipeline is already superlinear on that file. A 20000-line file with half its line endings foreign is rare.
- "A new public formatter member or a new type may go to the maintainer first": not needed under option (a). Under escalation 9, the issue's own fix is not escalated.

## Architecture findings

FC-015 is the issue's own subject: one decision has two owners, and one rule is stated in more than one place.

- **Components:** `RH5604CodeMustNotContainMixedLineEndingsCodeFixProvider.ApplyCodeFixAsync` and `LineEndingNormalizationPhase.Execute`.
- **Shared decision:** which elements carry a line break, and how each one is rewritten to the detected line ending.
- **Places:** the two methods cited under *Premise*.

The class (FC-016) has two further members that state the same carrier set (`EndOfLineTrivia` plus `XmlTextLiteralNewLineToken`) to count rather than to rewrite: `LineEndingUtilities.DetectEndOfLine` and `RH5604CodeMustNotContainMixedLineEndingsAnalyzer.OnSyntaxTree`. They agree with the rewriters today. A new carrier kind added to only one owner would make RH5604 report what the formatter leaves, or the reverse.

All four sit in components the run already touches or in their counterpart (the RH5604 analyzer), so they are within the lead's narrow-or-structural decision. They are not a separate follow-up by necessity.

## Follow-up candidates

- **Possible superlinear cost of the line-ending rewrite** on files with many foreign line endings. Evidence: the CLI timing above, not isolated. Worth filing only with a measurement of the phase alone; otherwise drop it.
- If the lead fixes the issue narrowly: one enumeration of the line-break carriers shared by `DetectEndOfLine`, the RH5604 analyzer, and the new owner (see *Architecture findings*).
