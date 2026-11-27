Role: test implementer
Commit: f29c578
Verdict: tests written

Repair round 1, finding #1 of `08-auditor.md` (test side). My prompt carried the auditor's suggested change and the lead's planned repair; I re-derived the change from the repaired third example of RH4001-007 (`09-spec-author.md`, `sdd/specs/analyzer/RH4001.md`) and did not rely on the suggestions. Where I differ from the spec author's suggested summary, it is stated below.

## Change

`FileWithStatementsOnlyInsideMainAndMatchingFirstTypeIsNotReported` now holds `System.Console.WriteLine();` inside `Main`, so its code is the repaired example 3 verbatim (file `Program.cs`, `OutputKind.ConsoleApplication`, no diagnostic).

Its summary now reads: "Verifies that a file whose statements stand only inside a method of its first type, and not at the top level, is not reported when that type matches the file name". The earlier summary claimed the file "is checked", and the spec author suggested "is not exempted". I dropped both claims: this test cannot observe them, because `Program` matches `Program.cs` and the file is not reported whether it is exempted or checked (the auditor's falsifiability point holds with or without the statement). The claim that such a file is checked is asserted by its boundary partner `FileWithStatementsOnlyInsideTypeMemberIsReportedAndFileIsRenamed`, which reports `Startup.cs` on `Program`; this test only pins the example's stated outcome.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `FileWithStatementsOnlyInsideMainAndMatchingFirstTypeIsNotReported` — RH4001-007, third example (repaired) | RH4001-007 | `Verify(source, configure)` | Passes |

## Not covered, and why
- The other four RH4001-007 tests and every other test of the class are unchanged: the spec repair touched only example 3, and `rg "private static void Main" tests/Reihitsu.Analyzer.Test/Naming/RH4001TypeNameShouldMatchFileNameAnalyzerTests.cs` shows the only other `Main` already holds the same statement.
- No fail-first run: the test pins existing, unchanged behavior and passes for every predicate, by construction of the example (see *Change*). The FC-003 other side is `FileWithStatementsOnlyInsideTypeMemberIsReportedAndFileIsRenamed`.
- Line endings, formatter, code fix: unchanged from `03-test-implementer.md`; no layout or rewrite is involved.

## Commands
```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Naming.RH4001TypeNameShouldMatchFileNameAnalyzerTests"
scripts/test.sh --project architecture
```
Observed: analyzer 49 passed, 0 failed; architecture 198 passed. `scripts/format.sh` on the test file changed nothing.

## Follow-up candidates
_None._
