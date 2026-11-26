# Lead — trailing-comma-branch-end

- Issue: thoenissen/Reihitsu#1837 (`agent-filed`; goals are proposals). RH5409/RH5410/RH5411 trailing-comma fix and formatter in front of `#else`/`#elif`/`#endif` with no `#if` between the comma and the closing brace.
- Slug: `trailing-comma-branch-end`
- Branch: `claude/funny-ptolemy-pt5lxq` (session-designated). Draft PR: thoenissen/Reihitsu#1955.
- Lane: behavior-change — the issue requests changed code-fix and formatter output (a fix offered where none is today), which changes SHARED-TRAIL-002 and FMT-STRUCT-019/020 and needs a change doc.
- Roles: spec author → skeptic → (output questions) → test implementer → implementer → cleaner → spec checker → auditor → validator. None skipped.
- Maintainer clarifications: none beyond the issue. The issue itself is an output question (A: offer fix + formatter removes; B: unchanged, close issue).

## Maintainer answers

1. Output question (RH5409/5410/5411, comma in front of `#else`/`#elif`/`#endif` with no `#if` behind it). Options: A (fix + formatter remove), B (unchanged), C (only the RH5409/5410/5411 code fix removes the comma; the formatter keeps it as on `main`, so formatter-delegating fixes such as RH5303 keep `3,`). Answer: **C**. The maintainer's concern was that the formatter cannot know the directives; after the clarification that the check reads directive kinds only, they still chose C.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | skeptic | Escalation 10 (declare the shape unsupported)? | Not asked again | Maintainer already rejected declaring this shape in `sdd/changes/2026-11/report-across-conditional-directives.md` | Covered by that maintainer answer | — |
| 2 | skeptic | Change-doc reason for withholding on an `#if` in excluded code | Keep withholding; reason becomes "rare class, withheld" | Skeptic showed the pairing argument is wrong | Behavior unchanged; text only | pending |
| 3 | skeptic | SHARED-TRAIL-004 Why overclaims vs SHARED-FIX-009 | Qualify the clause | Accuracy | Spec text only | pending |
| 4 | skeptic | Change doc code-fix section | Superseded by answer C: the formatter and the formatter-delegating fixes stay unchanged; change doc says so | Answer C | Covered by maintainer answer | pending |
| 5 | skeptic | Issue's "Before" refers to an earlier change | "Before" is plain `main` | Change landed | none | — |

## Follow-up candidates

(none yet)

## State

- Last step: spec author 01 (9e96bfa), skeptic 02 (0ee6074). Output question batch 1 answered: C.
- Next: spec author (fresh, 03) rewrites change doc/spec for C + decisions 2-3, Status Accepted; then test implementer.
