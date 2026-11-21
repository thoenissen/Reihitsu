# 00-lead — squad-short-lanes

## Run

- Issue: thoenissen/Reihitsu#1906 — `agent-filed`, labels `maintenance`, milestone 1.5.0. Its goals were proposals, but the maintainer's triage comment on the issue (2026-11-21) accepted all four and added a fifth, so the five rules below are binding as the maintainer stated them.
- Slug: `squad-short-lanes`
- Branch: `claude/stoic-babbage-ieob4k` (designated by the session)
- PR: thoenissen/Reihitsu#1908 (draft claim)
- Lane: **behavior-change** (claimed). The diff will hold a process change doc under `sdd/changes/` plus tooling paths (`squad/**`, `scripts/**`, `CLAUDE.md`, `src/Reihitsu.Tooling/**` and its tests). Tooling alone would admit that diff, but the change doc is written first and alone needs behavior-change (precedent: run of #1792, `squad/notes/2026-11-16-large-agent-threshold.md`), and the lane-check change needs a premise check of what a "wording-only" diff is. Claiming the wider lane avoids a mid-run widening.
- Roles: spec author (change doc + process text in `squad/`, `scripts/README.md`, `CLAUDE.md`) → skeptic → test implementer (`Reihitsu.Tooling.Test` for the lane classifier) → implementer (`src/Reihitsu.Tooling`, `scripts/lane-check*`) → cleaner → spec checker → auditor → validator. Issue manager only if follow-up candidates arise.

## Maintainer clarifications (summarized from the triage comment on #1906)

1. Wording lane — accepted. A diff that only changes diagnostic titles or messages, code-fix titles, or rule-page text runs a short lane: spec author, spec checker, validator; no skeptic, test implementer, implementer split, or auditor unless a new test turns out to be needed. `scripts/lane-check.sh` must recognize such a diff (today `.resx` is production).
2. Coverage first — accepted. Before requesting or writing a test, the skeptic and the test implementer check whether an existing test already pins the value; "no new test" is a valid outcome.
3. Cheap self-hosting check — accepted. When a run adds lines to a test file, the validator also runs the matching self-hosting row locally.
4. Fewer pushes — accepted. The lead pushes handover commits at stage boundaries only.
5. (Added by the maintainer, option B) PR subscription only until CI is decided: the lead subscribes to the published PR only to receive the CI result of the final commit; green → unsubscribe at once, run ends; red → repair per `squad/lead.md` §9, push, wait for the next result, unsubscribe once green. Later review comments, merge, closed issues no longer reach the session; the maintainer sends further changes in chat. Files: `squad/lead.md` §9 and the matching sentence in `CLAUDE.md` ("The squad").

All accepted rules are stated in a process change doc under `sdd/changes/` with `Affected requirements: none — process change`.

## Maintainer answers

(none yet in this run)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Lane | behavior-change | Change doc first; lane-check design needs a premise check; avoids widening | Process and tooling only; no formatter, analyzer, code-fix, rule-page, or CLI output changes | claim |
| 2 | Spec author | Wording category scope | Only the two analyzer/code-fix `.resx` files, not every `.resx` | They hold diagnostic and code-fix texts; other `.resx` keep their category | Classification only | a9a156d |
| 3 | Spec author | Test paths in the wording lane | Wording lane admits no test path; a changed test widens to tests (no change doc) or behavior-change | Path-only lane check; checking literal-only test edits needs contents | Process only | a9a156d |
| 4 | Spec author | Text-only proof over `.resx` | `verify-text-only.sh --exclude`, passed per wording path by lane-check | Keeps the proof's scope exact | Tooling only | a9a156d |
| 5 | Spec author | Wording next to tooling / change doc | Wording may carry a change doc; wording + tooling needs one (mirrors production + tooling) | Consistency with existing rule | Process only | a9a156d |
| 6 | Spec author | Meaning of "stage boundaries" for pushes | The push list in `squad/lead.md` §2 *Pushes* | Concrete list a lead can follow | Process only | a9a156d |
| 7 | Spec author | Self-hosting trigger | Only when a run adds lines to a test file, as the maintainer stated | Binding wording of the answer | Process only | a9a156d |
| 8 | Spec author | Output-question batch in docs/wording lanes | One batch after the spec author, before the spec checker | These lanes have no test implementer | Process only | a9a156d |
| 9 | Spec author | Maintainer change requests after unsubscribe | Run like a repair round with a fresh `sdd/work/<slug>/` | Reuses existing mechanics | Process only | a9a156d |
| 10 | Skeptic | F1 who writes resource texts after a wording run widens | Spec author owns the two resource files' values in every lane and writes them before reporting "needs a test"; test-implementer file gets a clause for a run widened from wording | Otherwise nobody may write them in the tests lane | Process only | 75f9cbd |
| 11 | Skeptic | F2 an updated test literal widens the wording lane (stricter than "unless a new test") | Keep the path-based rule (option A); state it in the change doc and PR; follow-up candidate 1 removes the quoted literals so wording runs stop touching tests | Lanes are internal (`squad/README.md`); a path-only check cannot tell a literal update from a new test; the follow-up restores the maintainer's intent | Process only | 75f9cbd |
| 12 | Skeptic | F4 push points | Push once at each role-stage boundary (after each role's handover commit), plus the existing points | Closest to the maintainer's "stage boundaries"; keeps "a run can continue in a later session" true | Process only | 75f9cbd |
| 13 | Skeptic | F3, F5, F6 corrections | Route to spec author: cleaner.md lane list, line-count filter description, follow-up template lane list | Factual errors | Process only | 75f9cbd |
| 14 | Skeptic | Source of wording paths for `--exclude` | `lane-check.sh` asks the classifier for them; no second statement in bash; `scripts/README.md` says so | One statement of the classification | Tooling only | 75f9cbd |
| 15 | Skeptic | Claims wider than wording | State in `squad/README.md` lane bullets that tests/bug-fix claims cover a wording diff | Documents existing order semantics | Process only | 75f9cbd |
| 16 | Skeptic | Unsubscribe when remaining CI failures also exist on `main` | Keep; name it in the PR | A failure on `main` is decided by the lead anyway; waiting would never end | Process only | 75f9cbd |
| 18 | Spec author | Spec author's ownership of resource texts | Only changed texts of existing entries; a new rule's entries stay with the implementer; *Internal* follow-up variant keeps its lane list | Narrow ownership; wording changes user-visible text | Process only | 75f9cbd |
| 17 | Skeptic | Accept change doc | Accept after the amendment; the amending spec author sets `Status: Accepted` | Skeptic ran; amendment follows | Process only | 75f9cbd |

## Follow-up candidates

1. 21 test files quote a diagnostic message as a literal instead of `MessageFormat`; each pushes a wording change out of the wording lane. Cleanup, no visible change. (01-spec-author)
2. The line-count self-hosting rows also apply to production files; widening the validator's trigger to any C# under `src/` would be a process question for the maintainer. (01-spec-author)

## State

- Last step: spec-author amendment done (75f9cbd), change doc Accepted. No output questions (process only), so no stop before tests.
- Next: test implementer (Reihitsu.Tooling.Test, lane classifier + lane check).
