Role: spec author
Commit: a87851c
Verdict: text written

## What was written

`sdd/README.md` only, per the maintainer's clarifications in `00-lead.md`:

- New section `## The formatter may do more than the analyzers (P11)`, placed directly after `## Open questions`. Stable anchor: `#the-formatter-may-do-more-than-the-analyzers-p11`; a later mirror principle can sit after it.
- Content: the formatter may do more than the sum of the analyzer rules; a rule silent where the formatter rewrites is not an open question about the formatter; it is filed either as the rule's own gap (a bug against the rule's spec when the spec already covers that form, otherwise a behavior change with a change doc) or as a new-rule candidate (a different construct; a rule is not widened just because it roughly fits). The section closes by saying it settles only the "formatter does more" direction, and every other rule/formatter disagreement still follows `Open questions`.
- One example per half, from the issue body: `record struct S<T> where T : class;` under RH5110 (own gap) and `[A(⏎    1)]` under RH5101 (new-rule candidate).
- One bullet added at the end of `## Open questions` linking to the new section, so a reader of that section finds P11.

How the examples were checked against the specs (not by running code): `sdd/specs/analyzer/RH5110.md` RH5110-001 lists class, struct, interface, record class, method, delegate, local function — no record struct — so that form is not covered by the spec and the example correctly lands on "behavior change", not "bug". `sdd/specs/analyzer/RH5101.md` RH5101-001 lists invocation, object creation, implicit object creation, constructor initializer, primary constructor base type — no attribute argument list. The formatter's behavior on both inputs is taken from the issue, not reproduced.

No tracker numbers in the text. No spec requirement, no requirement ID, no change doc (no intended behavior of a shipped surface changes; the maintainer adopted the principle in the issue). `AGENTS.md` not edited.

Lane proof: `scripts/lane-check.sh docs --head worktree` → `TEXT-ONLY PROOF: PASS — base=8e0f35d head=worktree — 2 changed file(s) (2 non-compiled, 0 C# comment/layout-only)` and `LANE CHECK: confirmed — claimed docs, diff needs docs`.

## For the lead

- The prompt carried no intended fix or verdict to ignore.
- Possible pointer in `AGENTS.md` *Issue intake* (the issue offered it as an option): not made, per the clarification; maintainer's call in the run report.
- The closing sentence ("any other disagreement … is filed as described under Open questions") states the current rule for the opposite direction; the mirror-principle run will need to replace or narrow it.

## Changed requirements

_None._

## Examples for the tests

_None — docs lane, no tests._

## Open questions to file

_None._

## Decisions needed

_None._
