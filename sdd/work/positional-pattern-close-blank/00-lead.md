# Lead — positional-pattern-close-blank

- Issue: thoenissen/Reihitsu#1935 (`agent-filed`; labels enhancement, formatter; milestone 1.6.0)
- Slug: `positional-pattern-close-blank`
- Branch: `claude/squad-1935-ptmco4` (session-designated); draft PR #1946
- Lane: **behavior change** — the issue requests changed formatter output (a blank line in front of a positional pattern's closing `)` is removed); FMT-BLANK-019 has to change, so a change doc is required.
- Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager only if follow-up candidates exist.

## Maintainer answers

1. Issue #1935 output question (recorded on the issue, comment of 2026-11-22): **option A** — the blank line in front of a positional pattern's closing `)` is removed, like the one in front of a list pattern's `]`. The work touches `BraceBlankLinePolicy`, FMT-BLANK-019, and the requirements that refer to it.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | spec author | Does the maintainer's answer (option A) cover change-doc item 2 (comment/directive gaps in front of the positional `)`, FMT-BLANK-009/018/020)? | Covered; not asked again | The answer names FMT-BLANK-019 "and the requirements that refer to it" and states the rule as "like a list pattern's `]`"; each item-2 After equals main's output for the list-pattern form | The visible change is covered by the maintainer's answer | fdca76f |
| 2 | skeptic | Skeptic's spec-wording points 1-3 (FMT-BLANK-019 cross-reference to FMT-BLANK-014; empty clause `P(⏎⏎)` sentence + example; owner-node contexts phrased as examples) | Accept all three; routed to a fresh spec author as an amendment | Wording precision; the empty-clause result equals main's for `[⏎⏎]` | Spec wording only; the empty-clause output follows option A ("like a list pattern's `]`") | 96d9be0 |
| 3 | skeptic | Architecture finding FC-015: closing-token set listed in both `BraceBlankLinePolicy` and `CommentIndentationContributor` | Change the structure in this run: one shared predicate used by both components, routed to the implementer | The drift between the two lists is this issue's cause; the issue itself asks to keep them in step; the shared predicate is a few lines within the run's component and its named counterpart, far below escalation 9 | Both lists hold the same tokens after the change; no output changes beyond option A | 3a0c01e |
| 4 | skeptic | Is the issue comment recording option A the maintainer's answer? | Yes | Posted on the maintainer's account as an explicit decision, placing the run in 1.6.0 | — | |
| 5 | skeptic | Output-question batch before the test implementer | None to ask | Every visible change is covered by the option-A answer (decision 1) | — | |
| 6 | test implementer | Add a detached `FormatNode` test? | No | The rule depends only on the `)` token and its owning node, not on the entry point; the node-in-document path is covered by the RH3202 test | Tests only | f651f95 |
| 7 | implementer | Home of the shared closing-token predicate | Keep it in `BraceBlankLinePolicy`; `CommentIndentationContributor` calls it | A new home would need a new type for one method; no architecture test forbids the reference; the set is stated once | Internal structure; indentation output unchanged | 3a0c01e |

## Follow-up candidates

(none yet)

## Audited SHA

(none yet)

## Worklist / next step

- Done: spec author (01, fdca76f), lane check confirmed.
- Done: skeptic (02, d5d0947) — proceed.
- Done: spec author amendment (03, 96d9be0), change doc Accepted.
- Done: test implementer (04, f651f95/b65151b): 14 formatter + 2 RH3202 runs fail as intended, 4 pin unchanged behavior.
- Done: implementer (05, 3a0c01e/215e75d): all tests pass.
- Next: cleaner (06).
