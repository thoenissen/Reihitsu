# SHARED-DIAG-003 Razor extension without a conforming shape (#1260)

| | |
|---|---|
| Date | 2026-10-21 |
| Escalation | 1 |
| Run | issue #1260 |

## Question

The P5 extension of SHARED-DIAG-003 (Razor and HTML comment text and `@@` are content; an `@` after a letter or digit is text) has no input that conforms today: RH2109 reports all of them (#1289), and three untagged RH2109 characterization tests pin the opposite. Options: write the sentence and keep the characterization tests until #1289; or defer the Razor sentence to #1289.

## Answer

Write the sentence as decided. SHARED-DIAG-003 stays covered by its conforming C# literal and comment tests; the three RH2109 characterization tests stay unchanged until #1289.

## Reason

none given.
