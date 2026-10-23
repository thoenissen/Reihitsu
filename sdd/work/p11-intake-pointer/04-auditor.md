Role: auditor
Commit: f7785f2de7a80a28b169913275bf47218b8e0075
Verdict: pass

## Scope
Full audit of the run's diff for thoenissen/Reihitsu#1402. Head `f7785f2de7a80a28b169913275bf47218b8e0075` (checked out, working tree clean); base `origin/main` = `git ls-remote origin refs/heads/main` = `3dde06028688d32bda8057ef8bb1f07cda0904fd`, an ancestor of the head. The diff outside `sdd/work/` is `AGENTS.md` only, +1 line (line 59), exactly what `01-implementer.md` describes; commit `6cece91` carries it, the other run commits touch only `sdd/work/p11-intake-pointer/`. `scripts/lane-check.sh tooling` on the head: `LANE CHECK: confirmed — claimed tooling, diff needs tooling`. The prompt carried no intended fix, suspected cause, or hoped-for verdict.

Checked against the issue's "What to do" and "Done when" and the maintainer's clarifications in `00-lead.md`:
- One sentence, one bullet, placed in *Issue intake — question the report* after the "Reject a framing the repository's own policy contradicts" bullet; no other line of `AGENTS.md` changed; `CLAUDE.md`, `sdd/README.md`, specs, code, and tests untouched.
- Trigger wording matches the issue: a report that asks an analyzer to follow the formatter, or calls the formatter wrong because no rule reports what it rewrites.
- Relative link `sdd/README.md#the-formatter-may-do-more-than-the-analyzers-p11` from the repository root resolves: the heading at `sdd/README.md:117` slugs to that anchor, and `sdd/README.md:115` already links the same anchor. Same link form as the existing `sdd/README.md#open-questions` link in `AGENTS.md`.
- The three clauses after the colon match P11 clause by clause: "may do more than the analyzers" = "may do more than the sum of the analyzer rules" (`:119`); "not widened to a different construct only because it roughly fits" = the new-rule-candidate bullet (`:127`); "a gap inside the rule's own construct is the rule's gap" = the rule's-own-gap bullet (`:121`). None contradicts the closing sentence on `main` (`:137`): both triggers concern what the formatter rewrites or what the analyzer should add, and a shape the formatter keeps is not reached by "the rule's gap".
- `git diff --check` clean; LF line endings like the rest of the file; no tracker number in the line; English.
- `PhaseOwnershipDocumentationTests` reads `AGENTS.md` only between the `formatter-phase-ownership` markers (lines 82 and 103); the edit at line 59 is outside them and contains neither marker. `SelfReferentialTrackerReferenceTests` scans `*.cs` only.

## Findings
_N/A — no confirmed finding._

## Guard scope
The diff adds no code predicate, guard, or exemption. The one decision-like element is the prose trigger of the new bullet, recorded as an analogue:

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| When an intake must consult P11 (`AGENTS.md:59`) | A report asking an analyzer to follow the formatter, or calling the formatter wrong because no rule reports what it rewrites | P11 scope, `sdd/README.md:119` ("the formatter rewriting a construct that no rule reports") and its closing sentence `:137` | The second trigger equals P11's scope. The first is wider: it also admits a report asking a layout rule to stop reporting a shape the formatter keeps; P11's closing sentence routes that case to SHARED-DIAG-010, so the pointer still leads to the right rule (see Hints) | ok |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| P11: the formatter may do more than the analyzers; rule's own gap vs. new-rule candidate | `rg -n -i 'roughly fits\|may do more than\|P11'`: `sdd/README.md:117-137` (owner); `sdd/README.md:115` (in-file pointer); `AGENTS.md:59` (new pointer with the maintainer-requested summary); `sdd/specs/shared/diagnostics.md:535` (SHARED-DIAG-010 Why, pre-existing reference to the same principle) | One owner. The `AGENTS.md` summary was requested by the maintainer (`00-lead.md`), links the owner, and is faithful clause by clause at the head; no drift. `squad/failure-classes.md:59` (FC-013) and `squad/roles/skeptic.md:23` state a conflicting rule; pre-existing, see Hints |

## Assertion adequacy
_N/A — no test added or changed; no test covers `AGENTS.md` prose, and the one test that reads `AGENTS.md` (`PhaseOwnershipDocumentationTests`) inspects only the marker-delimited phase map, which the edit does not touch._

## Failure classes dismissed
- FC-001, FC-002, FC-003 — no predicate or guard in the diff; the prose trigger is compared against P11 under Guard scope.
- FC-004, FC-005, FC-019, FC-020, FC-021 — no token, trivia, or directive edit; no C# file changed.
- FC-006, FC-022, FC-023, FC-024, FC-025, FC-026, FC-027, FC-028, FC-029, FC-036 — no rewrite, syntax, or rename; no compiled file changed.
- FC-007, FC-008, FC-009, FC-018, FC-030 — no code fix changed.
- FC-010, FC-011, FC-012, FC-013, FC-014 — no formatter phase or analyzer changed (FC-013's own text is discussed under Hints as a pre-existing policy conflict, not as a reachable code defect).
- FC-015, FC-016 — checked under Policy ownership: one owner, faithful pointer; no sibling instruction file carries a private copy of P11.
- FC-017, FC-033, FC-034, FC-035 — no test added or changed.
- FC-031 — no changed method; no other `AGENTS.md` sentence now contradicts the new bullet.
- FC-032 — no compiled change; no earlier project result depends on this prose.
- FC-037 — no `.cs` file in the diff, so no text-only proof is needed.

## Hints
- **Pre-existing conflict between FC-013 and P11 (follow-up candidate).** `squad/failure-classes.md:59` defines FC-013 as "analyzer-clean code is changed by the formatter", and `squad/roles/skeptic.md:23` tells the skeptic that "analyzer-clean code must stay formatter-stable (FC-013)". P11 (`sdd/README.md:119`) says the opposite for that direction: the formatter rewriting a construct no rule reports is intended. Concrete case: P11's own example `[A(\n    1)]` is RH5101-clean, the formatter rewrites it, P11 calls that intended, FC-013 calls it a failure. The run did not cause this, and the maintainer limited the change to `AGENTS.md`, so it does not block; after this run the intake bullet and the skeptic's conflict check give opposite answers for a report that "calls the formatter wrong because no rule reports what it rewrites". A search of the tracker found no issue for it. A follow-up would narrow FC-013 and skeptic check 3 to the direction P11 leaves open (formatter output flagged by the analyzer; a layout rule asking for more than the formatter, SHARED-DIAG-010), drafted for the maintainer's approval.
- **The first trigger is wider than the implementer's description.** `01-implementer.md` says the sentence "speaks only of the direction in which the formatter does more than a rule reports". "Asks an analyzer to follow the formatter" also covers a report asking a layout rule to stop reporting a shape the formatter keeps. P11's closing sentence routes that to SHARED-DIAG-010, so the pointer is not wrong, and the wording is the issue's own. This is an observation about the handover's claim, not about the line.
- **Handover commit line.** `02-cleaner.md` records `Commit: 5b7af62`, but its statements (the `git diff` listing `AGENTS.md`) come from a tree that includes `6cece91`. This is working-doc bookkeeping only; `sdd/work/` is deleted before publishing.
