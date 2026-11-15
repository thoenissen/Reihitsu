# Lead — primary-and-partial-constructors

- Issues: thoenissen/Reihitsu#1680 (`agent-filed`, bug, analyzer), thoenissen/Reihitsu#1742 (`agent-filed`, bug, analyzer). Goals of both are proposals.
- Slug: `primary-and-partial-constructors`
- Branch: `claude/beautiful-shannon-h7hcav` (the session's designated branch; the session forbids pushing to any other, so the playbook's `claude/issues-1680-1742-<slug>` name is not used)
- PR: thoenissen/Reihitsu#1744 (draft claim)
- Lane: **bug fix**, for both.
  - #1680: reported observed-versus-expected difference — documentation analyzers report a positional record / primary-constructor type twice (SHARED-DIAG one diagnostic per element).
  - #1742: reported observed-versus-expected difference — syntax-node rules on constructors miss the defining part of a partial constructor.
- Roles run: skeptic (premise check), test implementer (reproduction), implementer, cleaner, spec checker, auditor, validator. Spec author: only for a clarification, skipped unless a role asks for one.
- Maintainer clarifications: none.

## Maintainer answers

None yet.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | Use the session's designated branch `claude/beautiful-shannon-h7hcav` | The session environment permits pushing only to this branch | Branch name is internal | 5a879f2 |
| 2 | Skeptic | #1680: per-analyzer probe for ~50 declaration-kind documentation analyzers (issue goal) | Drop; tests only for the confirmed candidates (RH8204, RH8304, RH8305, RH8307, RH8308, RH8309) | Affectedness is decided by the registered trivia kind; the skeptic's probe shows no declaration-kind analyzer receives a node twice | Tests only | – |
| 3 | Skeptic | #1680: scope — include region rules RH7302, RH7306, RH7307, RH7310 (same mechanism) | Include as adjacent fix | Same mechanism, same shared guard, each breaks an unchanged one-diagnostic requirement, common shape (`#endregion` before `}` of a primary-constructor class) | Brings output back to unchanged requirements | – |
| 4 | Skeptic | #1680 architecture finding: private guard copies in RH8305/RH8307, missing in eight analyzers; guard drops constructor-run-only nodes | Fix narrowly with one shared cheap guard used by every trivia-kind registration (RH8305/RH8307 copies replaced); it must keep nodes only the constructor run sees (reported once) | One statement of the rule; a cheap syntactic check | Unchanged one-diagnostic requirements decide the output | – |
| 5 | Skeptic | #1742 output question 2 (RH8017/RH8018/RH8030/RH8101–RH8105/RH8108 on defining part) | Not asked: unchanged SHARED-DOC-001/005 leave partial elements outside these rules; keep today's output (not reported) | An unchanged spec decides it | Unchanged spec decides | – |

## Follow-up candidates

- RH8102 reports nothing for a mismatched `<param>` on a partial constructor's implementing part, while it reports on partial-method parts (01-skeptic.md, #1742). Specs exclude partial parts for RH8101; to be assessed by the issue manager.

## State

- Last step: skeptic (01-skeptic.md, 71ae774).
- Open: escalation 9 + output question for #1742 (report defining part via new shared mechanism vs record limitation) — asked in chat, waiting. Whole run stopped until answered.
- Next after the answer: (B) spec author records the limitation; test implementer for #1680 reproductions.
