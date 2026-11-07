Role: spec author
Commit: 92e579f
Verdict: text written

Text cleanup after audit 4, for lead decision 16 (finding #7). My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Changed requirements

- **RH5202-006 — The fix is withheld when a leftward shift would change the value of a raw string nested in a hole — clarified (Why text only).** The *Why* no longer predicts when other fixes release the outer fix. The two sentences that did (release when the nested opening quotes stand at least the shift from their line start; no release otherwise) are replaced by one: "While the condition holds, the outer diagnostic stays reported without a fix; it may stay so until other fixes change the code so that the condition no longer holds, and the outer literal's fix is then offered on the result (SHARED-FIX-002)." This holds for every member, including finding #7's depth-2 member, where an enclosing nested literal's fix releases the outer one. The normative sentence, conditions, examples, and IDs are unchanged. The examples still state the release for their own inputs, which the tests pin.

Proof: `scripts/verify-text-only.sh --base HEAD --head worktree --strict-docs` — `TEXT-ONLY PROOF: PASS — base=92e579f head=worktree — 1 changed file(s) (1 non-compiled, 0 C# comment/layout-only); xml-doc impact: none`.

The rule page is unchanged.

## Examples for the tests

_None._ No behavior or example changed.

## Output questions

_None._

## Internal choices

- Did not name the nested literal's own fix as an example of the "other fixes", so the text makes no claim about which fix releases the outer one. Alternative: "such as the nested literal's own"; rejected as a partial prediction of the kind decision 16 removes.

## Follow-up candidates

_None._
