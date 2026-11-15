Role: spec author
Commit: e614938
Verdict: text written

Repair round 2, decision 20 of `00-lead.md`, audit finding #3 of `21-auditor.md`. The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the task itself.

## Changed requirements
_None._ No requirement changed. In the change doc `sdd/changes/2026-11/comma-behind-comment-unsupported.md`, the *Checked and kept* entry for `ContainsNonFormattingTrivia(separator.LeadingTrivia)` no longer says "the text is the same" without qualification. It now says that the structural phase's text equals `main`'s, and that the final output differs from `main`'s first pass only when a comment line follows the comma's line. It gives the example `B /** doc B */,` ⏎ `// note` ⏎ `}` (enum member list, array initializer): the output now converges in one pass with no blank line in front of `// note`, where `main` inserted the blank line on the first pass and removed it on the second (FMT-PIPE-004). The entry also says the output equals the output of the same text written without the comma.

## Examples for the tests
_None new._ Finding #3's rows (decision 20) are the test implementer's, derived from FMT-PIPE-004 and FMT-STRUCT-019/020. The expected value is the `Parse`/`Diagnose` output: no blank line in front of `// note`.

## Output questions
_None._ FMT-PIPE-004, which is unchanged, decides the output (decision 20).

## Internal choices
- I left *Visible output changes* alone and put the convergence note in the *Checked and kept* entry, where the claim stood. The change is an idempotency repair that FMT-PIPE-004 already demands, not a new output. The alternative was to add an Input/Before/After block to *Visible output changes*.

## Follow-up candidates
_None._
