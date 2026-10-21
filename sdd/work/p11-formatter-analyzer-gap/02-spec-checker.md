Role: spec checker
Commit: d7e6871
Verdict: consistent

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| – | – | – | None. | – |

## Observations (not findings; no mismatch with the diff, the issue, or the maintainer's clarifications)

- `sdd/README.md`, section `The formatter may do more than the analyzers (P11)`: the section says a case "is filed as one of two kinds" but does not say where each kind is filed (a change-doc-backed behavior change or a bug against the rule; a new-rule candidate). The issue's "Done when" asks only for the two kinds, and `squad/README.md` limits issue creation to "SDD open questions" issues with the maintainer's approval, so the gap is deliberate and left to the lead. The spec author may widen it if the maintainer wants a filing destination named.
- Same section, first sentence: "rewriting a construct that no rule reports" reads slightly wider than the "own gap" half, where the rule does report the construct and skips only one form of it. The two bullets that follow resolve this, so it is wording only.
- The closing sentence (any other disagreement follows `Open questions`) will need narrowing when the mirror principle P31 (thoenissen/Reihitsu#1331) lands; the spec author's handover already says so.

## Checked
1. Change doc exactly when required: ok. The diff changes only the rulebook text; no requirement is added, changed, or retired, and no rule, phase, or surface is added or removed, so none is required and none is present. The maintainer's clarifications say no change doc.
2. Specs match the diff: ok. `git diff 8e0f35dac358ca06f977629db45a2be1fd28c830...HEAD` touches `sdd/README.md` and `sdd/work/p11-formatter-analyzer-gap/` only; no spec is touched. The RH5110 example is consistent with RH5110-001 (class, struct, interface, record class, method, delegate, local function; no record struct), so "behavior change" is right. The RH5101 example is consistent with RH5101-001 (invocation, object creation, implicit object creation, constructor initializer, primary constructor base type; no attribute argument list), so "new-rule candidate" is right. The two examples match the issue body; the section states the rule in the issue's terms and is not a requirement.
3. IDs: ok. No new IDs; `sdd/specs/requirement-ids.txt` untouched.
4. Code owners exist: ok (n/a). No spec header touched.
5. User documentation: ok (n/a). No rule page, code fix, or diagnostic ID touched.
6. Open questions recorded: ok. The spec author's handover lists none; the text itself defines P11 as the answer to a class of them.
7. Requirements covered and specs consistent: ok (n/a). `sdd/specs/` untouched, so `scripts/test.sh --project architecture` was not required and not run; no new active requirement, so no covering test needed. `scripts/lane-check.sh docs --head worktree` -> `LANE CHECK: confirmed — claimed docs, diff needs docs`.
8. Nothing left behind: ok. `sdd/work/` holds only `p11-formatter-analyzer-gap/`, this run's, which the lead deletes before publishing.

Maintainer's clarifications also verified: only `sdd/README.md` edited (`AGENTS.md` untouched), a stable heading exists (`## The formatter may do more than the analyzers (P11)`, anchor `#the-formatter-may-do-more-than-the-analyzers-p11`, linked from `Open questions`), one example per half, no tracker numbers in the text, no files under `sdd/specs/` touched.
