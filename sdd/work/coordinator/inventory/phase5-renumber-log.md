# Phase 5 — requirement-ID renumbering by #1429: open-issue follow-up log

Source of the map: `git diff 1b9a028^ 1b9a028 -- sdd/specs/requirement-ids.txt` plus the old specs' `Status: Retired` lines.

- Renumbered: RH3202-005 → RH3202-002; RH5002-004 → RH5002-002.
- Removed (all were already `Retired`): RH0001-007, RH1003-005 (header `Code fix` row); RH2005-004, RH3202-006, RH5002-006, RH6002-007, RH8107-008 (SHARED-FIX-002, Fix All); RH3202-002/-003/-004 (SHARED-BODY-001/-002/-003); RH5002-002/-003/-005 (SHARED-STMT-002/-003/-004); RH5030-003 (SHARED-AFTERBRACE-001).

## Issues touched in this phase

None. No open issue needed an edit or a comment.

## Issues checked

- All 215 open issues: bodies scanned for `RH0001|RH1003|RH2005|RH3202|RH5002|RH5030|RH6002|RH8107-<n>`.
  Hits only in #1284 (RH3202-002, already new numbering), #1242 and #1241 (RH8107-006/-007, unchanged), #974 (RH0001-003), #973 (RH1003-001), #967 (RH2005-003). The cited IDs are unchanged, so these issues need nothing.
- All 69 open issues with comments: every comment read. No comment cites a renumbered or removed ID. #1091 mentions that "the case-insensitive wording of RH8107-001 is retired", which is text, not an ID change.
- #1284 and #972 (the PR's follow-up list): both bodies were already edited at 2026-10-24T15:01, right after the merge. #1284 now reads `RH3202-002`. #972 cites only `SHARED-STMT-004` and no RH5002 requirement. Nothing left to do.

## Doubts

- I could not see the earlier body text of #1284 and #972, because the MCP tools show no edit history. The current text is consistent with the map, so I assume the 15:01 edits applied it correctly.
- Issue titles were checked only for the first 100 open issues; none cites a requirement ID. Bodies and comments were checked for all.
