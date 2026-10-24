# Phase 5 — split issues (bookkeeping P5-1b)

Created 2026-10-24 from `phase5-consolidated.md` sections A, C, H. Every new issue: milestone `vNext`, labels `enhancement` + `analyzer`/`formatter`, body in the matching feature-request template fields, ends with "Split from #N (the bug part stays there)." Each original got one "Triage split" comment. Labels and milestones of the originals were not changed.

| original | part moved | new issue | title |
|---|---|---|---|
| #951 | case 3 (tab-indented raw strings, FMT-RAW non-goal) | #1432 | FMT-RAW: align tab-indented multi-line raw strings (behavior change) |
| #1146 | Q3, Q5, Q6 (RH5104 widening) | #1433 | RH5104: `///` tails and `/* */` lines count as ordinary comments, comments after interpolated raw strings are reported (behavior change) |
| #1070 | no-report part (SHARED-CASING-011) | #1434 | Naming rules: do not report names dictated by metadata or a native entry point (behavior change) |
| #1288 | `public` for ordinary operators (#1254 Q1, RH2004-006) | #1435 | RH2004 fix adds `public` to operators (behavior change) |
| #1279 | keep `static`/`async`, parenthesize in cast/binary operands | #1436 | RH3004 fix keeps `static` and parenthesizes the lambda in operand positions (behavior change) |
| #1332 | Q10 (drop blank line before moved `break`) | #1437 | RH7501 fix drops the blank line before the moved `break` (behavior change) |
| #1382 | RH5204 and SHARED-INIT-001 parts | #1438 | RH5204 and initializer rules report a token behind a multi-line comment tail (behavior change) |
| #963 | `{// why` part (comment after `=>`) | #1439 | Expression body to block: a comment after `=>` becomes the block's first line (behavior change) |
| #958 | everything except arrows | #1440 | Formatter spacing for `?:`, element access, invocation, `::`, colons, initializer braces and more (behavior change) |
| #1397 | items 15, 17, 18 | #1442 | Formatter line breaks: unjoinable lists stay as written, parenthesized nested conditionals, parenthesized `case` patterns (behavior change) |
| #1393 | RH5417 part | #1443 | RH5417 fix moves the type's `}` off the edited line (behavior change) |
| #1079 | everything except RH5603 (RH5601, RH6004, RH6005, RH6021, RH7302) | #1444 | RH5601, RH6004, RH6005, RH6021, RH7302: tabs on later `///` lines, `# region`, repeated spaces (behavior change) |
| #1093 | everything except the formatter `[param: A] set;` join | #1445 | RH55xx attribute rules: placement follows the attached declaration, not the target specifier (behavior change) |
| #1276 | widening (P11 one-line body; formatter extension-block braces) | #1446 | RH5404 reports every one-line type body; formatter places extension-block braces like type braces (behavior change) |
| #1385 | SHARED-STMT-004 / RH5002/RH5008 fix part | #1447 | RH5002/RH5008 fixes and SHARED-STMT-004: the blank line goes after directives, never directly before one (behavior change) |
| #1092 | Q4 (static constructor private) | #1448 | RH8017/RH8018: a static constructor always belongs to the private group (behavior change) |
| #1153 | everything except RH8032 without location | #1449 | Documentation rules: parameters as the compiler binds them, summary text that renders, RH8030 on the produced documentation (behavior change) |
| #1353 | Q3 (header followed by line break / EOF) | #1450 | RH8402 reports a header not followed by a line break or end of file (behavior change) |
| #1084 | everything except `_` → `I_` | #1451 | RH4005/RH4121 and casing fixes keep an existing prefix and every identifier character (behavior change) |
| #1289 | `@functions` part | #1452 | RH2109 reports `@functions` blocks like `@code` (behavior change) |

#1441 is not part of this run (created by someone else in between).

## Skipped

- #1074 — not split by instruction (waits for the maintainer's J4 decision).
- None of the 20 listed splits was skipped.

## Notes for the coordinator

- #1070: the original body asks only for the no-report rule; the kept bug part (casing fixes withheld on metadata overrides / native entry points, SHARED-FIX-003) is stated in the split comment, not in the body. The body may need a rewrite to that bug.
- #1288: the moved `public` instance came from the round 7b comment, which had labeled it "bug (fix)"; moved as an extension per RH2004-006.
- #1276: the formatter extension-block braces (round 9 item 27 comment, a FMT-BREAK-002/-007 behavior change) went with the widening into #1446.
- #958: the original is already `enhancement`/`vNext`; it now keeps only the arrow part (FMT-PIPE-014), which is a bug — its labels/milestone may need adjusting by the bookkeeping agent.
- #1353: the part that stays is Q1/Q2 (clarification), not a bug.
