Role: spec author
Commit: 058baad5
Verdict: text written

## Changed requirements
_None._ Repair round on auditor finding #1 (`03-auditor.md`), the `scripts/README.md` part only. `scripts/README.md`, *Test-host memory*: the sentence "so neither the build nor any other process sees it" now reads "so it reaches the test hosts and the processes they start, but not the build or `dotnet test` itself". Child processes of the test host inherit `GLIBC_TUNABLES` (for example `bash scripts/apply-fix.sh` started by `ApplyFixWrapperPositionalArgumentTests`). The `scripts/test.sh` comment is the cleaner's (04) and was not edited. No spec under `sdd/specs/` covers this tooling text.

## Examples for the tests
_None._

## Output questions
_None._

## Internal choices
_None._

## Follow-up candidates
_None._
