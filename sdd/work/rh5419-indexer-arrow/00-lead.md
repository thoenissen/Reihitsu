# Lead — rh5419-indexer-arrow

- Issue: thoenissen/Reihitsu#851 — "[ANALYZER FEATURE] Expression style get-only indexers should be single lined". Label `agent-filed`: goals are proposals.
- Slug: `rh5419-indexer-arrow`
- Branch: `claude/modest-albattani-nbx229` (session-designated). Draft PR: #1882.
- Lane: **behavior change / new rule** — the issue requests a new analyzer rule (new diagnostic + code fix). The issue itself names the new rule, so this is the issue's own deliverable, not escalation 5.
- Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publish if follow-up candidates exist. None skipped.
- Notes from intake: the issue proposes ID RH5418, but RH5418 is already taken on `main` (EmptyUnionsShouldUseSemicolonDeclarations); the next free RH54## ID is RH5419. Dependency #848 is closed (merged via PR #854). RH5401 stays properties-only per the issue.
- Maintainer clarifications: none.

## Maintainer answers

- Batch 1, Q1 (rule as specified: reports, fix-withheld cases, title, rule page example): **A**.
- Batch 1, Q2 (wrapped parameter list): neither A nor B — maintainer says the parameter list should not be wrapped, the indexer should be single line. Unclear which cases (does the rule report a wrapped parameter list when the arrow is already on the `]` line? does the formatter change?) — asked back (Q2 follow-up, options 2C/2D).
- Batch 1, Q2 follow-up: **D**. The rule also reports an expression-bodied indexer whose parameter list spans several lines, also when the arrow already stands on the `]` line (`int this[byte a,\n byte b] => a;` is reported). The fix joins the whole declaration onto one line (`int this[byte a, byte b] => a;`), and is withheld when a comment or directive sits inside the parameter list. The maintainer added: the formatter may be changed in this run if the fix needs it; otherwise the formatter joining indexer parameter lists is a follow-up.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 4 | Implementer | Shared `AttributeListCodeFixUtilities` used by RH5401 and RH5419 | Accepted | Moves RH5401's guard instead of copying it (avoids FC-015 two owners); ~89 of ~120 lines moved; part of the issue's own fix, not escalation 9 | RH5401 behavior unchanged, its tests pass | b3265ac |
| 3 | Spec author follow-up | Change the formatter in this run? | No; the fix joins the parameter list itself, formatter join stays a follow-up | Maintainer allowed a formatter change only if the fix needs it; it does not (formatter keeps a one-line list, FMT-BREAK-012) | Maintainer answer covers it | 65f3e42 |
| 2 | Skeptic | Skeptic's two follow-up candidates (`this`/`[` split; comment/#if in indexer parameter list) | Dropped | Rare input classes; the first is decided unreported by RH5108's non-goal | Not filed; formatter output unchanged | — |
| 1 | Intake | Rule ID | RH5419 instead of the issue's RH5418 | RH5418 is taken on `main`; the issue says "next free RH54## ID" | The issue's intent ("next free ID") decides it | — |

## Follow-up candidates

- Formatter: join a wrapped parameter list of an expression-bodied indexer onto one line, so a `reihitsu-format` run clears RH5419's parameter-list report — unless this run changes the formatter because the fix needs it (maintainer answer, Q2 follow-up). Source: maintainer answer.

## Worklist / next step

- Spec author done (01-spec-author.md, commit 7f9df95): change doc written; open output question Q1 (wrapped parameter list: fix offered vs withheld). Lane check confirmed behavior-change.
- Skeptic done (02-skeptic.md, 3ddc02a): proceed. FC-015 risk for auditor: RH5401 attribute-list guard must not be privately copied into RH5419 provider. Follow-up candidates recommended for dropping (rare / decided by RH5108 non-goal).
- Output-question batch 1 posted to maintainer (Q1 rule as specified incl. title; Q2 wrapped parameter list A/B). Spec author follow-up done (03-spec-author.md, 65f3e42): change doc Accepted; SHARED-DIAG-010 gains RH5419 exception. Lane check confirmed.
- Open output question for batch 2 (before auditor): block comment inside a single-line parameter list (`int this[short a /* c */]\n    => a;`) — A fix offered (`int this[short a /* c */] => a;`), B withheld. Spec written on A. Source: 03-spec-author.md.
- Test implementer done (04-test-implementer.md, 4575497/6d0634b): 47 analyzer tests + 5 formatter tests; provider must support Fix All. Spec example RH5419-001 `int this[uint a, uint b] => a;` does not compile (CS0266) -> spec author fixes (`=> 1;`), in parallel with implementer.
- Spec example fixed (06-spec-author.md, 7ee7c97). Implementer done (05-implementer.md, b3265ac): tests pass; new shared helper AttributeListCodeFixUtilities (moved from RH5401).
- Next: cleaner (07-cleaner.md), spec checker (08), batch-2 output question, auditor, validator. Next: route answers to spec author (status Accepted), then test implementer.
