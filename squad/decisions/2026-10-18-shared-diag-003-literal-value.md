# Does SHARED-DIAG-003 protect a literal's characters or its value, and does it state the fix side?

| | |
|---|---|
| Date | 2026-10-18 |
| Escalation | 1 |
| Run | issue #1069 |

## Question

Three rules change a literal's characters without changing its value: RH5202's fix shifts the content lines of a raw string whose closing delimiter is misaligned, RH3204 fixes `$"a{{b}}"` to `"a{b}"`, and RH3202's fix re-indents a raw string through the formatter. Options: content is the literal's value; content is its characters with an exemption for rules about literals; or content is its characters with no exemption. Second question: should SHARED-DIAG-003 keep its clause that a code fix must not change literal or comment text, which duplicates SHARED-FIX-003 (thoenissen/Reihitsu#1068)?

## Answer

Content is the literal's value. How the value is spelled — escapes such as `{{` and the raw-string indentation the compiler strips — is the literal's own syntax, which a rule about literals may check and a fix may respell while the value stays the same. SHARED-DIAG-003 drops its fix clause and covers only what is reported; the fix side is left to SHARED-FIX-003.

## Reason

none given (both answers were the lead's recommendation; the value reading matches FMT-RAW-001, and dropping the clause states the fix rule once).
