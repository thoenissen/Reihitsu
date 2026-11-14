# Lead — formatter-fix-agreement

- Issues: thoenissen/Reihitsu#1529 (`agent-filed`), thoenissen/Reihitsu#1543 (`agent-filed`). Both goals are proposals.
- Slug: `formatter-fix-agreement`
- Branch: `claude/squad-1529-1543-wokrft` (the session's designated branch; see Decisions #1)
- PR: thoenissen/Reihitsu#1697 (draft)
- Lane: behavior change
  - #1529: an open output question on which layout the RH5111/RH6023 fixes and whole-document formatting share for a wrapped member of an assigned conditional access; whichever reading is chosen changes the formatter's or the fixes' visible output, so a spec change and likely a change doc.
  - #1543: an open question whether the blank-line phase removes blank lines between an own-line comment and further braces (and chained keywords, per the issue's comment), or whether RH5024/RH5025/RH5026 exempt them; either reading changes formatter output or diagnostics and amends FMT-BLANK-020.
- Roles: spec author → skeptic → (output questions) → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publishing if there are follow-up candidates. None skipped.
- Maintainer clarifications: none beyond the prompt.

## Maintainer answers

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | Use the session's designated branch `claude/squad-1529-1543-wokrft` instead of `claude/issues-1529-1543-<slug>` | The session environment permits pushes only to the designated branch | Branch names are internal | claim commit |

## Follow-up candidates

(none yet)

## Worklist / next step

- Done: claim, draft PR #1697, `scripts/prepare.sh` (SDK 10.0.404 installed).
- Next: spawn the spec author.
