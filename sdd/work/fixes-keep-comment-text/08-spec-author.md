Role: spec author
Commit: 5d48433
Verdict: spec written

Repair round 1, for finding 1 in `07-spec-checker.md`, as `00-lead.md` decision 12 states. The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond decision 12.

## Changed requirements

- **FMT-PIPE-010** — Node-level entry points format only their target — clarified (scope narrowed).
  - "Both MUST keep the target's trailing trivia" stays for both entry points, as on `main`.
  - The write-once clause now names `FormatNodeInDocumentAsync` only. That matches the code: only that entry point writes the trivia back through `WriteBackTrailingTrivia` (05-implementer).
  - One sentence was added: no caller passes `FormatNodeInDocumentWithContextAsync` a target whose last token formatting leaves inside the result, so the requirement does not decide that case for it (AGENTS.md, *Supported inputs*; callers RH5111, RH7101, RH3208, the chain helper, per 05-implementer).
  - The following sentence now begins "Both format…", so its subject stays unambiguous.
  - The examples are unchanged. Both already name `FormatNodeInDocumentAsync`, and the second one is the write-once case.
  - `Uses` is unchanged. SHARED-FIX-003 is still cited in the *Why*.

No new IDs. `scripts/test.sh --project architecture --filter "FullyQualifiedName~Sdd"`: Passed 51, Failed 0.

## Examples for the tests

FMT-PIPE-010: the existing `FormatNodeInDocumentAsync` tests remain the coverage. No test is needed for `FormatNodeInDocumentWithContextAsync`.

## Output questions

_None._

## Internal choices

- I stated the WithContext case as explicitly undecided in the spec rather than leaving it silent. The alternative was to say nothing; that would let a reader take "Both MUST keep the target's trailing trivia" as covering the duplicate case.

## Follow-up candidates

_None._ An unreachable case is not a candidate.
