Role: test implementer
Commit: 3a1e0df2d072f144c36c008c495a2fe19a412d41 (head before this commit; production code unchanged)
Verdict: tests written

Repair round 2 correction, decision 21 of `00-lead.md`: the two RH5405 end-of-file tests disputed in `17-implementer.md` fed both DataRows a document without any line break, so the detected line ending fell back to `Environment.NewLine` (FMT-PIPE-007) and the asserted output was platform-dependent. The prompt carried no intended fix, suspected cause, or hoped-for verdict. No production code was changed.

## Tests

File `src/Reihitsu.Analyzer.Test/Formatting/RH5405BracesMustNotBeOmittedAnalyzerTests.cs`. Each input now starts with `var count = args.Length;` and a line break, and the `if` tests `count > 0`; the body still ends the document without a final line break, so the end-of-file branch of SHARED-BRACE-002 is still what the tests reach. Each DataRow's document now holds one line break of its own ending, which FMT-PIPE-007 detects on every platform.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `FixWrapsABodyOnTheLineOfItsIfThatEndsTheDocument` [LF/CRLF] | SHARED-BRACE-002 | `VerifyTopLevelCodeFixWithEndOfLine` | green |
| `FixKeepsTrailingWhitespaceOfABodyThatEndsTheDocumentOnTheBodyLine` [LF/CRLF] | SHARED-BRACE-002 | same | green |

Assertions are otherwise unchanged: braces around the body, the body indented one level, whitespace and nothing else behind the body kept on its line, nothing behind `}`.

## Scenarios (bug fix)

_N/A._

## Not covered, and why

- The one-line document with no line break at all is no longer tested: its output depends on `Environment.NewLine` by FMT-PIPE-007, so no single expected text holds on every platform. The end-of-file branch itself is covered by both tests and by the two below-the-`if` tests.

## Commands

```shell
scripts/test.sh --project analyzer --configuration Debug --filter "FullyQualifiedName~RH5405BracesMustNotBeOmitted"
```

Result: `Failed: 0, Passed: 113, Total: 113`. `scripts/format.sh` on the test file: formatted 0.
