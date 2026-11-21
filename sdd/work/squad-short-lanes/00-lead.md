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

## Follow-up candidates

(none yet)

## State

- Last step: claim, prepare, 00-lead written.
- Next: spawn spec author.
