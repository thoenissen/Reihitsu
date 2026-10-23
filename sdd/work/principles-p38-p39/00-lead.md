Role: lead
Commit: aba1d5d12680b867a142f6a1cad3af8ff78bf3a8
Verdict: lane widened to tests

## Run

- Issue: thoenissen/Reihitsu#1415 — record principles P38, P39 and the P29/P35 clarifications in the shared specs.
- Slug: `principles-p38-p39`
- Branch: `claude/compassionate-lovelace-z2h5k5` — the session's designated development branch, used instead of `claude/issue-1415-<slug>` because the session may push only to that branch.
- Draft PR (claim): thoenissen/Reihitsu#1418

## Lane

Docs / text only. The issue asks for spec wording only: four decisions of question round 10 of thoenissen/Reihitsu#912 written into `sdd/specs/` (shared diagnostics or code-fix spec for P38 with a citation from `analyzer/RH3103.md`; `shared/code-fixes.md` and `shared/element-documentation.md` SHARED-DOC-004 for P39; SHARED-FIX-008 for the P35 clarification; SHARED-DOC-004 for the P29 clarification). The issue states no behavior change, no change doc, and that every code change follows in separate issues (#1417, #1262, #1388). If the spec diff needs covering tests (for example a new requirement in an enforced spec), the lane widens to tests before any implementer starts.

Roles run: spec author (writes the text), spec checker. Validator only if a compiled file changes.
Roles skipped: skeptic, test implementer, implementer, cleaner, auditor — not part of the docs lane.

## Maintainer clarifications

- The maintainer answered round 10 items 1–4 of thoenissen/Reihitsu#912 with the recommended option each; the issue records the outcome. Source questions: #1411 (P38), #1407 Q2 (P29) and Q3 (P39), #1408 Q-1380-2 (P35).
- The code follow-ups are listed in the issue's comment; none of them joins this run.
- No open `SDD open questions:` issue exists for the touched specs (#1407, #1408 and #1411 are closed).

## Lane widening

After the spec author (01-spec-author.md, commit 21c2f49) the run widens from docs to tests, before any implementer started. The diff adds SHARED-DIAG-011 to the enforced spec `shared/diagnostics.md`, and `RequirementCoverageTests.EveryRequirementOfAnEnforcedSpecIsCovered` fails until a test covers it; a docs-lane diff cannot add a requirement to an enforced spec (`squad/README.md`, Lanes). The path-only lane check still reads docs, which is narrower than the claim and therefore admitted.

Roles of the tests lane: spec author (done), skeptic (spec against the code), test implementer, cleaner, spec checker, validator. Skipped: implementer and auditor — not part of the tests lane.

## Escalation 1 — answered (2026-10-23)

The skeptic (02-skeptic.md) raised four blocking questions. The maintainer's answers, recorded in `squad/decisions/2026-10-23-1415-*.md`:

- Q1 (P38 reach): reading (b). Wherever the change a rule asks for cannot be written at that position, the construct is not reported. This is a general principle that applies to every rule. Any compile error the position itself causes counts, not only the grammar and the language version.
- Q2 (P39 and splits): reading (b). A fix must not lose documentation. The RH7101 split of a documented multi-variable declaration is withheld, and the user rearranges the code by hand.
- Q3 (SHARED-FIX-008 scope): reading (b). Only the line breaks in the span the fix replaces, rewrites, or moves count, its trailing trivia included. A line break ending the line outside that span is not in the fix's scope.
- Q4 (examples): reading (b). Examples use only shapes that conform today, and the requirement text states the principles in full. Where no conforming shape exists, the follow-up adds the example together with its fix and tests.
- Follow-up candidates: the maintainer agrees they are filed. The lead drafts them for approval of the exact text.

## Lane widening to tooling

The decisions entries under `squad/decisions/` classify as tooling: `scripts/lane-check.sh tests --head worktree` reports `mismatch — claimed tests, diff needs tooling`. No implementer has started, so the run widens to tooling along tests → tooling, as thoenissen/Reihitsu#1404 did for the same reason. Roles: spec author (done), skeptic (done), test implementer, cleaner, spec checker, auditor, validator. The auditor is added by the widening. The implementer is skipped because the run changes no production or tooling code; the tests only add or remove requirement tags and add tests of existing behavior.

## Follow-ups filed (maintainer approved the exact text)

- Comments with new instances on thoenissen/Reihitsu#1417 (RH3103 `using static`), #1262 (RH7309 shapes 1–3), #1336 (RH7101 documented split), #1388 (RH5406 and RH7309 line endings).
- New issue thoenissen/Reihitsu#1422: RH3104 `new S();` and RH5417 under C# 10 are not reported (P38).
- The formatter question about splitting a documented field (from 03-spec-author.md) is not filed: thoenissen/Reihitsu#1384 (P36) already decides and tracks it.

## Repair round 1 (after 07-auditor.md, verdict findings)

- Finding 1 (medium; formatter-delegating fixes inherit the documented field split): this is a pre-existing defect that the new SHARED-FIX-003 condition now names. It is a follow-up candidate and is not repaired in this run. The lead drafts a comment on thoenissen/Reihitsu#1384 for the maintainer to approve, and lists the violation in the run report.
- Finding 2 (low; stale summary of `FixCarriesADocumentationCommentBehindCodeWithTheMovedDeclaration`, FC-031): introduced by this PR and routed to the test implementer. It is comment-only, so the proof is `scripts/verify-text-only.sh`.
