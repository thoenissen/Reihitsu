Role: spec author
Commit: 2b8d28f
Verdict: spec written (examples brought in line with FMT-INDENT-031; change doc completed)

My prompt carried no intended fix, suspected cause, or hoped-for verdict; it routed the spec gaps of `05-test-implementer.md`, which are covered by the maintainer's answer 1B.

## Changed requirements

- FMT-BREAK-011 — Parameter lists hug their declaration — example output changed: the block of `delegate(int a, int b)` stands in column 32, under `delegate`.
- FMT-BREAK-041 — A continuation behind a keyword … joins its line — example output changed: the block of `static delegate` stands in column 29, under `delegate`.
- FMT-BREAK-044 — No line break is written into a non-verbatim interpolation before C# 11 — both outputs changed: C# 10 `b` (24) and `e` (18); C# 11 `a` (23), `b` (24), `c` and `d` (29), `e` (18), each under its `delegate`.

Normative text unchanged in all three; only the anonymous method's `{`…`}` lines moved. Each changed output block is byte-identical (after removing the raw-string indentation) to the test implementer's expected value: `ParenthesizedLambdaLineStartTests.SpecExampleAnonymousMethodParenthesisJoinsWhileLambdaArgumentKeepsItsLine`, `ContinuationJoinTests.SpecExampleJoinedConstructsAreLaidOutAsWhenWrittenOnOneLine`, and `NonVerbatimInterpolationLineBreakTests` `SpecCSharp10Expected` / `SpecCSharp11Expected` (checked by script, not by eye).

Change doc `sdd/changes/2026-11/anonymous-method-aligns-everywhere.md`:
- *Affected requirements* names the three updated examples.
- *Code-fix results* gains RH5201 (statement at depth 8→12, off depth 10→14, two-space file 4→8), RH5112 (off depth 10→14, two-space file 4→8), RH3202 (C# 11+: 8→22; C# 10 unchanged, one line), and the RH5417 formatter-parity result (4→38). The "before" columns are the outputs those tests asserted and passed with on 69c5e19; the lead sentence now says which results came from `scripts/apply-fix.sh` and which from the existing tests. The "after" columns equal the test implementer's rewritten expected values.

No requirement-ID reference, `Uses` row, or code owner added or removed, so the architecture tests were not rerun.

Scan of every `sdd/specs/**/*.md` for an anonymous method whose `{` line is not under its anchor: the remaining hits are inputs (FMT-INDENT-031 examples in `indentation.md`), the FMT-PIPE-033 example in `pipeline.md` (document returned unchanged), and the RH2002-003 example (diagnostic-only analyzer, no formatted output). None is a formatter output, so none was changed.

## Examples for the tests

FMT-BREAK-011, FMT-BREAK-041, FMT-BREAK-044 — already covered by the tests 05 rewrote; no new test needed.

## Output questions

_None._

## Internal choices

- RH5417 is listed as a formatter result in a separate paragraph, not as a code-fix bullet: its test is a `VerifyFormatter` parity test, not a code-fix test. Alternative: a bullet among the code fixes.

## Follow-up candidates

_None._
