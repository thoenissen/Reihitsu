Role: spec author
Commit: 890019d
Verdict: text written

The prompt carried no intended fix, suspected cause, or hoped-for verdict; the auditor's suggested wording was treated as a suggestion only.

## Change

`scripts/README.md`, section "Hang timeout", second sentence of the first paragraph (auditor finding 1). The old text, "about 10 minutes after the last test before it finished", holds only for a host that runs tests one at a time. The timer restarts on every test start or finish in the host, so the sentence now names that event, consistent with the paragraph's first sentence:

> A test that blocks forever therefore fails the run about 10 minutes after the last test in its host started or finished — its own start when the host runs tests one at a time, or a later test's when it runs them in parallel — whatever the blocked code checks;

The wording differs from the auditor's suggestion ("after the last other test in its test host finished"), which misses the sequential case, where the last event is the blocked test's own start, and a parallel test's start that no finish follows. Nothing else changed. Markdown only: no C# file changed, so `verify-text-only.sh` has nothing to prove. `scripts/lane-check.sh tooling --head worktree` → `confirmed — claimed tooling, diff needs tooling`.

## Changed requirements
_None._

## Examples for the tests
_None._

## Output questions
_None._

## Internal choices
- Name the timer's reset event (any test start or finish in the host) rather than only another test's finish, as the auditor suggested; alternative: the auditor's wording.

## Follow-up candidates
_None._
