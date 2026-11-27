Role: spec author
Commit: 6fb0655
Verdict: text written

Repair round for auditor finding 1 (`08-auditor.md`). My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the finding itself; nothing to ignore.

## Change

`documentation/rules/RH4124.md` only:

- *Why is this a problem?* — "Descriptive parameter names make inline transformations and predicates easier to understand without tracing the parameter's type and usage." becomes "Descriptive parameter and range variable names make inline transformations, predicates, and queries easier to understand without tracing the variable's type and usage."
- *How to fix it* — "Rename the parameter to describe the value or role it represents." becomes "Rename the parameter or range variable to describe the value or role it represents."

The heading, *Description*, table, examples, and footer are unchanged; the page follows RH4124-003 and maintainer answer 1B. No spec, change doc, code, or test changed.

## Proof

`scripts/verify-text-only.sh --base 9397d10 --head worktree`:

    TEXT-ONLY PROOF: PASS — base=9397d10 head=worktree — 3 changed file(s) (3 non-compiled, 0 C# comment/layout-only); xml-doc impact: none

(The three files are this rule page and the two lead/auditor work files committed after 9397d10; this handover adds a fourth non-compiled work file.)

## Changed requirements
_None._

## Examples for the tests
_None._

## Output questions
_None._

## Internal choices
- Named both nouns ("parameter or range variable") rather than a neutral "name", matching the *Description* sentence; alternative was "Rename the variable …".

## Follow-up candidates
_None._
