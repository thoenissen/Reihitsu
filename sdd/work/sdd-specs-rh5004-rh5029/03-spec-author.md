Role: spec author
Commit: e40651c (production code unchanged since 33a29f7); spec commit 37982a6
Verdict: spec written

Repair round on Finding 2 and on the deconstruction question of `02-skeptic.md`. Inputs:

- thoenissen/Reihitsu#946, as the lead read it: it states only the expected fix output ("one blank line above the whole statement, in front of `await`") and does not say which token the diagnostic of the `await` form sits on. That location is undecided.
- Q1 of `01-spec-author.md` was filed as thoenissen/Reihitsu#1000.
- The deconstruction question was filed as thoenissen/Reihitsu#1001.

`sdd/README.md` (*Open questions*) says an undecided case is left out of the spec: no requirement, no non-goal, no note.

## Changed requirements

- RH5005-002 — The diagnostic is on the `using` keyword that starts the statement — changed.
  - Before: "The analyzer MUST report the diagnostic on the `using` keyword."
  - Now: "When the `using` statement starts with the `using` keyword, the analyzer MUST report the diagnostic on that keyword."
- RH5006-002 — The diagnostic is on the `foreach` keyword that starts the statement — changed, the same way for `foreach`.

Why: the old sentence covered every `using` / `foreach` statement. That included `await using (…)` and `await foreach (…)`, so it fixed the diagnostic on the keyword behind `await`, which is today's token. That token is part of the undecided #946 defect, and a fix that moves the diagnostic to `await` would have contradicted an active requirement.

How the new wording handles it:

- It keeps today's location for every statement that starts with its own keyword.
- It says nothing about the `await` form: no requirement, no non-goal, no note. Neither #946 nor the issue number is named in the spec.
- A C# statement can carry no attribute or other prefix, so "starts with the keyword" excludes only the `await` form.
- The other side of the qualifier is left without an example on purpose, because it is the open case.
- The IDs are kept. They exist only on this branch and were never merged, so the wording is changed instead of retiring the IDs.
- `Why:` now reads "The statement's first keyword is where the missing separator becomes visible …".
- `Example: see RH5005-001` / `RH5006-001` still holds: those examples start with `using` / `foreach`.

Not changed, after re-deriving them:

- RH5005-001 / RH5006-001 (the `await` form is reported).
  - Through SHARED-STMT-001, they measure the gap in front of the statement's first token.
  - That agrees with #946's expected output (the blank line goes in front of `await`) and with the formatter, so they decide nothing that is open.
- RH5021-001, RH5021-002, RH5029-001, RH5029-002 (deconstruction, #1001).
  - The requirements use "local declaration" without defining whether `var (b, c) = t;` or `(int e, int f) = t;` is one. Their examples, the `const` / `using` mentions, and the location list (`var`, `const`, `using`, the declared type) contain no deconstruction.
  - Under either reading of #1001, the specs give no answer for a deconstruction input. Writing a definition now, either including or excluding deconstruction, would settle #1001 inside the spec, which the rulebook forbids. So the case stays out with no wording change.
  - RH5021-002 calls the exempt statement "an assignment, simple or compound". For `var b = a; var (c, d) = t;` both readings give the same output (nothing reported), so that sentence does not lean either way either.
  - The `await using var x = …;` declaration (RH5029 reports on `await`; RH5021 reports the expression statement after it) is not #946's form and is not affected. The specs still name no `await` example.
- Rule pages: `documentation/rules/RH5005.md` and `RH5006.md` say nothing about the location, so they need no change.

## Examples for the tests

Unchanged from `01-spec-author.md`, with these narrowings:

- RH5005-002 and RH5006-002 are covered by markup on the `using` / `foreach` keyword of a statement that starts with it. The existing tests do this, e.g. `{|#0:using|}`.
- Per the maintainer's instruction and the skeptic's list, no test is added in this run for:
  - the `await` form (#946);
  - deconstruction declarations (#1001);
  - the multi-line `case` label over-indent (#1000, never tagged SHARED-STMT-004).

## Checks

`scripts/test.sh --no-install --project architecture`: 71 passed, 1 failed. The failure is `EveryRequirementOfAnEnforcedSpecIsCovered`, listing the same 34 uncovered IDs as before this round. Everything else is green, as before.

## Open questions to file

_None new._ #1000 and #1001 are already filed.

## Decisions needed

_None._

## Note on the prompt

The lead's message carried evidence (what #946 states, and the issue numbers filed). It carried no intended fix or hoped-for verdict.
