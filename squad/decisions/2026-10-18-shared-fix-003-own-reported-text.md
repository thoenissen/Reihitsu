# Does SHARED-FIX-003 forbid a fix to change comment, directive, or literal text its own rule reports?

| | |
|---|---|
| Date | 2026-10-18 |
| Escalation | 1 |
| Run | issue #1068 |

## Question

SHARED-FIX-003 forbids a fix whose result changes the text of a comment, directive, or literal the user wrote. Read literally, that forbids shipped fixes whose purpose is to edit such text (for example RH7302 capitalizing `#region properties`, RH8107 removing a `<returns>` element, RH5604 rewriting line breaks inside documentation comments). Options: scoped to the text the fix's own diagnostic reports; literal, with every such fix a bug or a listed exception; or documentation comments and region text exempted by kind.

## Answer

Scoped. A fix may change the text of a comment, directive, or literal only where that text is what its own diagnostic reports; any other such text stays unchanged.

## Reason

none given
