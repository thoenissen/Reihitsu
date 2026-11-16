# Phase 6 — triage of open issues without a milestone (2026-11-16)

Read-only proposal (`.claude/commands/triage.md` sections 1–3). Nothing written to GitHub. Re-checked via the REST API: the only open issues without a milestone are #1780, #1781, #1785, #1786.

Open milestones: vNext (2), Maintenance (4), 1.3.0 (11, current release), 1.4.0 (6), 1.5.0 (7); 1.2.1 (12) and 1.2.2 (13) have 0 open and are not released separately.

## Milestones per group

| Group | Recommended milestone |
|---|---|
| Maintenance | Maintenance |
| Extension | vNext |
| Minor bug | none in this set |
| Critical bug | none in this set |

## Issues

| # | Title | Group | Reason | Labels to add | Milestone |
|---|---|---|---|---|---|
| [#1780](https://github.com/thoenissen/Reihitsu/issues/1780) | Build: the Roslyn package version has one owner instead of 11 project files | Maintenance | Build hygiene only; premise confirmed (11 `.csproj` files under `src/` each pin `Microsoft.CodeAnalysis*` 5.12.0, no central version). Filed by PR #1779 (#1761). | – (has `maintenance`) | Maintenance |
| [#1781](https://github.com/thoenissen/Reihitsu/issues/1781) | FMT-BLANK-011: first comment of a parameter list / union case list gets a blank line behind `(` | Extension | Output question: today's output follows FMT-BLANK-011 as written (no exemption for parameter lists), so A changes formatter output; not caused by C# 15. Not reproduced here. Filed by PR #1779; the union-case-list half touches PR #1782 (#1762). | – (has `enhancement`, `formatter`) | vNext |
| [#1785](https://github.com/thoenissen/Reihitsu/issues/1785) | FMT-BREAK-002, FMT-BREAK-023: an extension block's braces and members start their own lines | Extension | Output question that changes layout of C# 14 extension blocks (not C# 15); belongs with #1636 (vNext). Filed by PR #1782, which deliberately added unions to the six kind lists instead of a generic kind set because of this issue. | – (has `enhancement`, `formatter`) | vNext |
| [#1786](https://github.com/thoenissen/Reihitsu/issues/1786) | Formatter: an argument list written on the line after its name is joined onto the name's line | Extension | Output question (join `Foo⏎(4)`); read from code, not run — columns and the claimed CLI vs RH5305 disagreement for `[with⏎(4)]` are unconfirmed; the shape is rare. Filed by PR #1782; the `with(` part overlaps #1763 (PR #1782). | `analyzer` (RH5305 fix named) | vNext |

## Doubts for the maintainer

- #1786: if the claimed CLI/RH5305 column disagreement for `[with⏎(4), ...]` reproduces, that part is a minor bug in new C# 15 support and would go to 1.3.0 (with or after PR #1782); the general join stays vNext. Recommendation: vNext now, move only if a repro confirms the disagreement.
- #1781: the union-case-list half could ride with 1.3.0 for consistency of the new union layout, but the defect predates C# 15 and A is a behavior change; recommendation vNext.

## Answer format

> `ok` — apply everything as proposed.
> `ok, #1786 → 1.3.0` — override single issues.
