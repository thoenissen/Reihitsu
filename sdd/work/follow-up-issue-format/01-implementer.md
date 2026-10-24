Role: implementer
Commit: 0f5078c
Verdict: tests pass

No tests exist for this change (Markdown only; the test implementer was skipped, see `00-lead.md`). "tests pass" here means: nothing to run, and the lane check confirms the claim.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `sdd/templates/follow-up-issue.md` | New. Variant picker table; statement that the format is not a public template and that behavior changes and new rules (including a rule's own unspecified gap and a P11 new-rule candidate) use the public feature-request forms; common fields (title `<Scope>: <outcome>`, `Lane:` spelled as `scripts/lane-check.sh` takes it, optional `Principle:`, self-contained body, English); three fill-in skeletons in fenced `markdown` blocks — Spec bug (Lane, Principle, Requirement ID + one-line wording, Input, Today's output, Required output, Affected, Blocked by, Done when), Clarification (Lane, Principle, Spec path, Behavior to write down with example, Covering test, Done when), Internal (Lane, Principle, What and why, Files or areas, Done when). | thoenissen/Reihitsu#1441 (no spec requirement; process change) |
| `sdd/README.md` | Layout tree line for `templates/` names the follow-up issue; new last bullet of the Layout section points to the template and says behavior changes and new rules use the public feature-request forms. | thoenissen/Reihitsu#1441 |
| `squad/lead.md` | *Follow-ups*: the draft is written in the format of `sdd/templates/follow-up-issue.md` for a spec bug, a clarification, or internal work; from the matching `.github/ISSUE_TEMPLATE/` form for a behavior change, a new rule, or a bug in behavior no spec covers. Rest of the paragraph unchanged. | thoenissen/Reihitsu#1441 |

No file under `.github/`, `sdd/specs/`, `src/`, or any test changed (`git diff --stat 1b9a028 HEAD`).

## Decisions taken beyond the issue text (for the spec checker and auditor to challenge)

1. **Bug without a spec.** The issue's spec-bug variant needs a requirement ID; a bug in a rule that has no spec yet (allowed by `sdd/README.md`, Layout, last-but-one bullet) has none. The document and `squad/lead.md` route that case to the public bug-report form, which is what the lead playbook did for every follow-up before this change. Alternative not taken: a `Requirement: none — no spec yet` value in the spec-bug variant.
2. **Rule's own gap (P11).** A gap in a rule's own construct whose form the spec does not cover is a behavior change per P11, so the document lists it with the feature-request forms; one whose form the spec covers is a spec bug.
3. **Lane spelling.** The `Lane:` value uses the lane-check names (`docs`, `tests`, `bug-fix`, `behavior-change`, `tooling`) rather than the table labels of `squad/README.md`; the line links the lanes section. Spec bug defaults to `bug-fix`, clarification to `tests` (a docs-lane diff cannot add a requirement to an enforced spec, `squad/README.md`), internal leaves it open but excludes `behavior-change`.
4. **Title convention.** `<Scope>: <outcome>` with no `[ANALYZER BUG]`-style prefix — invented here; the issue asks for a convention but names none.
5. **Field correspondence.** The spec-bug section states how its fields map onto the public bug-report forms (requirement ↔ description; input / required / today's output ↔ reproducible example / expected / actual), per the maintainer's wish for consistency with the public forms. Version, IDE, and OS fields of the public forms are not carried over: a follow-up is found in the repository tree, not in a released build.
6. No labels are prescribed; the repository's label set was not read (out of scope: GitHub reads were limited to the issue).

## Policy owners
Where follow-up issue drafting is described: `rg -n "ISSUE_TEMPLATE|follow-up issue" --glob '*.md' .` before the change found `squad/lead.md:84` (the only playbook instruction on drafting) and the `sdd/README.md` mentions of "a follow-up issue whose text the maintainer approves" (Open questions, P11), which say when, not how, and stay unchanged. `rg -n "templates" src/Reihitsu.ArchitectureTests` found no test that enumerates `sdd/templates/`.

## Tests run
- `scripts/lane-check.sh tooling --head worktree --no-install` before committing, and `scripts/lane-check.sh tooling --no-install` on 0f5078c (template included): `TEXT-ONLY PROOF: PASS` and `LANE CHECK: confirmed — claimed tooling, diff needs tooling` both times.

## Disputed tests or scope
_None._ The prompt carried no intended fix beyond the issue's own scope.

## Follow-up candidates
_None._
