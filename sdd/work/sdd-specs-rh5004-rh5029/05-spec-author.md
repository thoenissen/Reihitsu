Role: spec author
Commit: d3c6dc9 (production code unchanged since 33a29f7); spec commit c2c6931
Verdict: spec written

Repair round on the finding of `04-skeptic.md`. Since 37982a6, the `Why:` of RH5005-002 and RH5006-002 read "The statement's first keyword is where the missing separator becomes visible …". `sdd/README.md` says the `Why:` decides edge cases the sentence does not spell out. For `await using (…)` / `await foreach (…)` the first keyword is `await`, so that `Why:` leaned the open location question (thoenissen/Reihitsu#946) toward `await`. The skeptic's finding holds.

## Changed requirements

- RH5005-002 — The diagnostic is on the `using` keyword that starts the statement — changed (`Why:` only).
- RH5006-002 — The diagnostic is on the `foreach` keyword that starts the statement — changed (`Why:` only).

New `Why:` (RH5006 says "iteration variable" where RH5005 says "resource expression"):

> In such a statement the keyword is both the statement's first token and the word that names the statement, so the diagnostic sits where the missing separator becomes visible and where the reader sees which statement follows; the location stays stable when the resource expression changes.

Why this wording leaves the `await` form out:

- The reason rests on two properties coinciding in a statement that starts with its keyword: it is the first token, and it is the naming keyword.
- In the `await` form the two properties fall on different tokens: `await` is first, `using` / `foreach` names the statement.
- The `Why:` prefers neither one, so it decides nothing there.
- The heading, the normative sentence, the IDs, and the examples are unchanged.

## Not changed

- I accept the skeptic's note on attribute-prefixed statements (`[A] using (…)`, which does not compile, CS7014). It is code that does not compile and needs no spec. No wording change.
- RH5005-001, RH5006-001, RH5021-*, RH5029-*, and the rule pages stay as described in `03-spec-author.md`.

## Examples for the tests

Unchanged from `03-spec-author.md`.

## Checks

`scripts/test.sh --no-install --project architecture`: 71 passed, 1 failed. The failure is `EveryRequirementOfAnEnforcedSpecIsCovered`, which lists the same 34 uncovered IDs as before.

## Open questions to file

_None new._

## Decisions needed

_None._

## Note on the prompt

The lead's message pointed at the skeptic's finding. It carried no intended fix or hoped-for verdict.
