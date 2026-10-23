# Whether the spec examples for P38, P39, and P35 show shapes that do not hold today (#1415)

| | |
|---|---|
| Date | 2026-10-23 |
| Escalation | 1 |
| Run | issue #1415 |

## Question

The issue's examples for P38 (RH3103 dotted `nameof`, `Nullable<int>.Equals`, `typeof(Nullable<>)`, the C# 11 alias), P39 (RH7309 behind-code `///`), and P35 (RH5406 in a lone-`\r` document) do not hold today and contradict RH3103-006 and RH7309-004 until #1417, #1262, and #1388 land. Options: (a) keep the issue's examples; (b) the requirement text states the principles in full, the examples use only shapes that conform today, and where no conforming shape exists the example is added by the follow-up with its fix and tests, as for P37 in #1380.

## Answer

(b). The follow-up candidates the skeptic found may be drafted and filed.

## Reason

none given.
