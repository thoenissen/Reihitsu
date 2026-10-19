# How far does SHARED-FIX-003 reach for doc references, formatter layout, field order, and SHARED-DIAG-003?

| | |
|---|---|
| Date | 2026-10-18 |
| Escalation | 3 |
| Run | issue #1068 |

## Question

The skeptic found four cases where the SHARED-FIX-003 wording turned shipped behavior into violations the earlier decisions had not considered:

1. Renames rewrite references inside documentation comments (RH4105 `<param name>`/`<paramref>`, RH4106 `<see cref>`; SHARED-CASING-007 requires every reference to be renamed). Options: such references are part of the rename; or the fix is withheld for any documented symbol.
2. Fixes that delegate to the formatter change comment, directive, or literal text elsewhere in the node they format (RH3202 reflows a doc comment and re-indents a raw string; RH7103 turns an unrelated `#region helpers` into `#region Helpers` and adds `// Helpers` to its `#endregion`). Options: layout changes do not count but word changes do; everything inside the formatted node is exempt; or every change counts.
3. RH7109 reorders instance fields of a struct with sequential layout, which changes field offsets. Options: counts as a run-time change; or declaration order never counts.
4. The parallel run for #1069 adds SHARED-DIAG-003 with a differently worded exemption for comment and literal text; the two disagree on RH5604-005. Options: SHARED-FIX-003 owns the fix rule and uses SHARED-DIAG-003 only for what counts as content, aligned by whichever PR merges second; or both wordings stay and the disagreement is filed as an open question.

## Answer

1. A name reference inside a documentation comment that binds to the renamed symbol is renamed with it; that is part of the rename, not a change to user-written content.
2. Layout does not count: whitespace, line breaks, and indentation of comments and directives, and re-indenting a raw string literal without changing its value. Changing words does count, so RH7103 renaming an unrelated region is a bug, handled by a follow-up.
3. Reordering fields whose order determines the layout (sequential or explicit struct layout) is a run-time change and is withheld; RH7109 on such structs is a bug, handled by a follow-up. Reordering with no observable effect stays safe.
4. SHARED-FIX-003 owns the rule for fixes and relies on SHARED-DIAG-003 only for what counts as literal or comment content, with no second wording. Whichever of the two PRs merges second aligns its text; this PR states that in its review notes.

## Reason

none given
