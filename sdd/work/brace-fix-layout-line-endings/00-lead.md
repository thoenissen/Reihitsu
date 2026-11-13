# Lead — brace-fix-layout-line-endings

| | |
|---|---|
| Issues | thoenissen/Reihitsu#1416, #1388, #1627, #1142 (prompt order) — all four `agent-filed`: goals are proposals |
| Slug | brace-fix-layout-line-endings |
| Lane | bug fix |
| Branch | claude/laughing-wozniak-azmh7x |
| PR | #1691 (draft claim) |
| Base | origin/main c2866eaf294a9040b995a2b75de7587bdccc5efe |

## Lane per issue

- #1416 — RH5405–RH5407 fixes keep the wrapped statement's layout. Body says behavior change, but the maintainer's triage comments (2026-10-24) move it to the bug-fix lane, no change doc: SHARED-FIX-007 already requires it; SHARED-BRACE-002 is aligned to SHARED-FIX-007 as a clarification. Later comments: `else if` brace column still open here (decided by SHARED-FIX-007: column of `else`, must come from the formatter, not a prediction in the fix); `if (a) M(a, b); /* c */ b++;` leaves `b++;` behind the inserted `}` — whether it starts its own line is part of this issue's layout question.
- #1388 — fixes keep the document's line ending (RH5405, RH5011, RH8304, `FormatNode` via RH5401, RH5406 in lone-`\r` documents, RH7309 moved blocks, RH7004 with comment/directive before the next statement). Triage: bug fix against SHARED-FIX-008 / FMT-PIPE-007, no change doc; RH5011-004 and RH8304-004 aligned to SHARED-FIX-008 as a clarification.
- #1627 — RH5405–RH5407: trailing whitespace behind the body's trailing comment ends up behind the inserted `}`. Bug fix against SHARED-FIX-007.
- #1142 — attribute-list split fixes (RH5502…RH5531) change only the reported list and keep the line ending. Bug fix against SHARED-FIX-006 / SHARED-ATTR-017.

## Roles

Bug-fix lane: spec author (clarifications: SHARED-BRACE-002, RH5011-004, RH8304-004, SHARED-ATTR-017 if needed) → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publishing if follow-up candidates exist.

## Maintainer answers

Batch 1 (before test implementer), all answered as recommended:
1. RH5406, multi-line body starting on the `if`/`else` line → C: no fix offered; a body below its header keeps its lines.
2. RH5405, code behind the body on its line (`/* c */ b++;`) → A: `b++;` starts its own line behind the new `}`.
3. RH5405, blank line between header and body → A: removed, as on `main`.
4. #1627, trailing whitespace at end of body line → A: stays on the body's line, nothing behind `}`.
5. Escalation 10, directive/disabled text between header and unbraced body → A: not declared; brace fixes withheld for it.
7. RH5518/RH5528, reported list behind a property's `{` on a one-line accessor list (`public int Value { [field: First, Second] get => 1; }`) → B: no fix offered; the diagnostic stays.
6. RH5405, body below `if` not indented one level deeper (`if (a)⏎          M(a, b);`) → A: the body keeps its own column (every line of a body below its header stays).

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name: playbook prescribes `claude/batch-1416-…`; the session mandates `claude/laughing-wozniak-azmh7x` | Use the session branch | Session environment allows pushing only to that branch | Branch name only | claim commit |
| 2 | Claim | Lane of #1416 and #1388 (bodies say behavior change) | Bug fix | Maintainer triage comments on both issues moved them to the bug-fix lane with spec clarifications | Lane only | — |
| 3 | Skeptic | #1388 `FormatNode` instance (via RH5401) | Dropped with its FMT-PIPE-007 goal; FMT-PIPE-007 unchanged | No caller reaches it: `FormatNode` output only feeds the `'\n'` test in `CanCollapse` (02-skeptic) | Unreachable case | — |
| 4 | Skeptic | #1388 RH5405 CRLF comment instance and RH7004 instance do not reproduce | Characterization tests only; issue stays `Closes` (RH5011, RH8304, RH7309, lone-`\r` instances remain real) | 02-skeptic measured both green on c2866ea | Characterization only | — |
| 5 | Skeptic | `} else M();` — else stays behind new `}` | Accept spec author's SHARED-BRACE-002 reading | Decided by maintainer #1220 Q3 | Covered by a maintainer answer | c14bb8c |
| 6 | Skeptic | #1142 tab-indented list (spec author Q4) | Not asked; copy the tab (`\t[A]`/`\t[B]`), spec author drops the spaces-only restriction in SHARED-ATTR-017 | #1109 Q1 reading A includes the tab row | Covered by maintainer answer #1109 Q1 | — |
| 7 | Skeptic | Rare-class withholds for brace fixes: other code in front of the owning `if`/`else` on its line; directive between header and body (also fixes FC-009 no-op offer); tab on the owner's or a moving body line | Withhold; stated in SHARED-BRACE-002 | Rare classes (`AGENTS.md`, Code-fix coverage); cheap checks | Rare input class | — |
| 8 | Skeptic | Brace-fix line endings: SHARED-FIX-008 first vs second paragraph | Second paragraph (withhold in lone-`\r`/mixed documents when the moved body holds a line break), because the body is moved as text, like RH7004 | Mechanism decides; existing helper makes it cheap | Rare documents (lone `\r`/mixed) | — |
| 9 | Skeptic | RH7309 in lone-`\r`/mixed documents | SHARED-FIX-008 second paragraph: withhold | Moves text without the formatter, like RH7004 | Rare documents | — |
| 10 | Skeptic | Architecture finding: `ReihitsuFormatter.WriteBackTrailingTrivia` and cleanup/preparation phases both own the end of the target's last line; FMT-PIPE-010 vs SHARED-FIX-008 | Fix narrowly: the brace fixes stop delegating the whole `if` and wrap text (removes them from the class); file the write-back structural change (RH3202 etc., FMT-PIPE-010 alignment) as a follow-up | Cause spans all 29 formatter-delegating fixes, beyond the run's components | The follow-up carries it | — |
| 11 | Test implementer | RH5011-004 lone-`\r` sentence read literally gives `break;\r\n` (one CRLF, no blank line) | Accept the test's output `break;\n\r            default:` (one `\n` inserted, every input `\r` stays lone, a blank line exists); spec author rewords RH5011-004's sentence after the implementer | The only output that meets RH5011 and SHARED-FIX-008 together | Only all-lone-`\r` documents (rare); no other reading yields a correct fix | 7b3bed7 |
| 12 | Implementer | 13 `FixAllCorrectsEveryDiagnostic` tests (RH5502…RH5531 except RH5514) assert `[Second]⏎[Third]` | Tests are wrong; align to SHARED-ATTR-017 (`[Second] [Third]`) and the iteration count as RH5514's was — test implementer repair | Maintainer answer #1109 Q1 reading A (only the reported list changes) decides it | Covered by a maintainer answer | — |
| 13 | Implementer | Multi-line header (`if (a &&⏎ b) M();`): "on the header line" read by the header's last line (`)`/`else`), not the `if` keyword's line | Accept the implementer's reading; spec author words SHARED-BRACE-002/003 by the header's last line | The literal reading would put `{` inside the condition | Spec wording; the one output that compiles sensibly | 0d126cf |

## Follow-up candidates

- Formatter write-back of the target's trailing trivia undoes cleanup and line-ending normalization (RH3202 `=> _value;···` → `}···`; lone-`\r` behind target for every formatter-delegating fix); align FMT-PIPE-010 with SHARED-FIX-008 — 02-skeptic (#1627 architecture finding, follow-up candidates).
- RH8402 copies the first line's line break and hard-codes `\n` for a one-line file (by code) — 02-skeptic, Whole run.

## Worklist / next step

- Done: claim, draft PR #1691, prepare.sh; spec author 01 (c14bb8c, 4 output questions; flagged `} else M();` as a visible difference to add to the batch; FormatNode instance of #1388 possibly unreachable).
- Done: skeptic 02 (bb1c18e).
- Done: batch 1 answered (all as recommended).
- Done: spec author 03 (5360bf8): SHARED-BRACE-003 (withholds), RH7309-007, SHARED-ATTR-017 tabs.
- Done: Q6 answered A.
- Done: spec author 04 (cf5de70).
- Done: test implementer 05 (7b3bed7): 110 red reproductions; RH7004 not reproduced (characterization).
- Done: implementer 06 (0d126cf): 110 pass; 15 disputed tests.
- Done: answer 7 = B.
- Now: spec author 07 (answer 7 into SHARED-ATTR-017; decisions 11, 13 wording), then test implementer 08 (decision 12, answer 7 tests), then implementer 09 (answer 7 withhold), then cleaner.
- Open: spec author rewords RH5011-004 lone-`\r` sentence (decision 11) before the cleaner.
- Then: test implementer.
