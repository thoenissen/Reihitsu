# Run: using-block-directive-report

- Issue: thoenissen/Reihitsu#1836 — `agent-filed` (goals are proposals). Labels: enhancement, analyzer. Milestone 1.6.0.
- Branch: `claude/fervent-mccarthy-vxx8ss` (session-designated). Draft PR: #1968.
- Lane: **behavior-change** — the issue requests changed behavior (RH7201–RH7207 reporting in a using block that holds a preprocessor directive, with no fix), which needs a change doc and a spec rewrite of SHARED-USING-009.
- Roles: spec author → skeptic → (output questions) → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager if follow-up candidates exist. None skipped.
- Maintainer clarifications: none yet. The issue itself is an output question (A: report without a fix; B: keep current silence), carried no recommendation.

## Maintainer answers

- Q1 (issue question, incl. cases 1a/1b): A — report an out-of-order using block that holds a directive, no fix offered; formatter keeps the block.
- Q2 (which directives): A — every directive kind (#if, #nullable, #pragma, #region, ...).
- Q3 (comparison span): A — compare across the whole block.
- Q4 (escalation 10, declare using block with directive unsupported): A — not declared (answer '4' asked back; maintainer chose A).

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | skeptic | Spec author's Q4: does RH7207 check blank lines in a using block that holds a directive? | No — only the order (spec author's A); dropped from the output batch | Skeptic fixture e.cs: formatter writes blank lines around a `#region` in the block (FMT-BLANK-013), stable; checking blank lines would report the formatter's own output | SHARED-DIAG-010 (unchanged) demands it | 1db90d1 |
| 2 | skeptic | Basis of Q1's recommendation | SHARED-DIAG-008 and the precedent of report-across-conditional-directives, not 'order is content'; spec author rewords SHARED-USING-009 / change doc and FMT-USING-004 blank-line remark after the answers | Skeptic: FC-014 treats using order as formatter-judged | Wording only | pending |

## Follow-up candidates

- FMT-USING-004 wording: scope 'keeps blank lines' to the using phase (FMT-BLANK-013 adds blank lines around a #region inside a block); internal, from 02-skeptic.md.
- Reorder a using block whose every directive stands before its first using (e.g. a `#nullable enable` header) in fix and formatter — changes formatter output; from 01-spec-author.md.

## Worklist / next step

- Done: spec author 01 (change doc written, 1db90d1; 4 output questions).
- Done: skeptic 02 (faeeaeb): output question.
- Done: output batch answered (1A 2A 3A 4A).
- Now: spec author follow-up (03) (route answers, reword basis, Status Accepted), then test implementer.
