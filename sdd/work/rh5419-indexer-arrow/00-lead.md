# Lead — rh5419-indexer-arrow

- Issue: thoenissen/Reihitsu#851 — "[ANALYZER FEATURE] Expression style get-only indexers should be single lined". Label `agent-filed`: goals are proposals.
- Slug: `rh5419-indexer-arrow`
- Branch: `claude/modest-albattani-nbx229` (session-designated). Draft PR: #1882.
- Lane: **behavior change / new rule** — the issue requests a new analyzer rule (new diagnostic + code fix). The issue itself names the new rule, so this is the issue's own deliverable, not escalation 5.
- Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publish if follow-up candidates exist. None skipped.
- Notes from intake: the issue proposes ID RH5418, but RH5418 is already taken on `main` (EmptyUnionsShouldUseSemicolonDeclarations); the next free RH54## ID is RH5419. Dependency #848 is closed (merged via PR #854). RH5401 stays properties-only per the issue.
- Maintainer clarifications: none.

## Maintainer answers

- Batch 2, Q1 (block comment inside a one-line parameter list, `int this[short a /* c */]\n    => a;`): **A** — fix offered, gives `int this[short a /* c */] => a;`. Spec and change doc already written on A; nothing to route.

- Batch 1, Q1 (rule as specified: reports, fix-withheld cases, title, rule page example): **A**.
- Batch 1, Q2 (wrapped parameter list): neither A nor B — maintainer says the parameter list should not be wrapped, the indexer should be single line. Unclear which cases (does the rule report a wrapped parameter list when the arrow is already on the `]` line? does the formatter change?) — asked back (Q2 follow-up, options 2C/2D).
- Batch 1, Q2 follow-up: **D**. The rule also reports an expression-bodied indexer whose parameter list spans several lines, also when the arrow already stands on the `]` line (`int this[byte a,\n byte b] => a;` is reported). The fix joins the whole declaration onto one line (`int this[byte a, byte b] => a;`), and is withheld when a comment or directive sits inside the parameter list. The maintainer added: the formatter may be changed in this run if the fix needs it; otherwise the formatter joining indexer parameter lists is a follow-up.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 8 | Repair round 2 | Run the cleaner after the implementer's repair? | Skipped | The implementer's handover shows `scripts/format.sh` changed nothing on the provider and the Release build has 0 warnings — the cleaner's two checks | Internal process step | 381f12c |
| 7 | Audit 1 | Package README Formatter column ✔ for RH5419 although the formatter keeps a wrapped parameter list | Keep ✔ | Same convention as RH5111, which also carries an SHARED-DIAG-010 exception and ✔; the formatter clears the arrow part | README column is not a rule page or diagnostic; convention unchanged | — |
| 6 | Audit 1 | Fix offered when a comment/directive between `this` and `[` keeps the `[` on its own line | Refuse the class: withhold the fix | Rare input class (comment between `this` and `[`), AGENTS.md Code-fix coverage; the maintainer's D asked for one line, which the fix cannot give here | Withholding for a rare class puts nothing in question | (round 2) |
| 5 | Repair round 1 | Re-run cleaner after the ruleset-only repair? | Skipped | The repair touched only `Release.ruleset` (XML, no C#); nothing for the formatter or the Release build to clean beyond what the implementer's build showed | Internal process step | 90e48de |
| 4 | Implementer | Shared `AttributeListCodeFixUtilities` used by RH5401 and RH5419 | Accepted | Moves RH5401's guard instead of copying it (avoids FC-015 two owners); ~89 of ~120 lines moved; part of the issue's own fix, not escalation 9 | RH5401 behavior unchanged, its tests pass | b3265ac |
| 3 | Spec author follow-up | Change the formatter in this run? | No; the fix joins the parameter list itself, formatter join stays a follow-up | Maintainer allowed a formatter change only if the fix needs it; it does not (formatter keeps a one-line list, FMT-BREAK-012) | Maintainer answer covers it | 65f3e42 |
| 2 | Skeptic | Skeptic's two follow-up candidates (`this`/`[` split; comment/#if in indexer parameter list) | Dropped | Rare input classes; the first is decided unreported by RH5108's non-goal | Not filed; formatter output unchanged | — |
| 1 | Intake | Rule ID | RH5419 instead of the issue's RH5418 | RH5418 is taken on `main`; the issue says "next free RH54## ID" | The issue's intent ("next free ID") decides it | — |

## Follow-up candidates

- Formatter: join a wrapped parameter list of an expression-bodied indexer onto one line, so a `reihitsu-format` run clears RH5419's parameter-list report — unless this run changes the formatter because the fix needs it (maintainer answer, Q2 follow-up). Source: maintainer answer.
- Release.ruleset lacks RH5417; add an architecture test that every shipped rule is listed in Release.ruleset. Source: 09-implementer.md.

## Worklist / next step

- Spec author done (01-spec-author.md, commit 7f9df95): change doc written; open output question Q1 (wrapped parameter list: fix offered vs withheld). Lane check confirmed behavior-change.
- Skeptic done (02-skeptic.md, 3ddc02a): proceed. FC-015 risk for auditor: RH5401 attribute-list guard must not be privately copied into RH5419 provider. Follow-up candidates recommended for dropping (rare / decided by RH5108 non-goal).
- Output-question batch 1 posted to maintainer (Q1 rule as specified incl. title; Q2 wrapped parameter list A/B). Spec author follow-up done (03-spec-author.md, 65f3e42): change doc Accepted; SHARED-DIAG-010 gains RH5419 exception. Lane check confirmed.
- Open output question for batch 2 (before auditor): block comment inside a single-line parameter list (`int this[short a /* c */]\n    => a;`) — A fix offered (`int this[short a /* c */] => a;`), B withheld. Spec written on A. Source: 03-spec-author.md.
- Test implementer done (04-test-implementer.md, 4575497/6d0634b): 47 analyzer tests + 5 formatter tests; provider must support Fix All. Spec example RH5419-001 `int this[uint a, uint b] => a;` does not compile (CS0266) -> spec author fixes (`=> 1;`), in parallel with implementer.
- Spec example fixed (06-spec-author.md, 7ee7c97). Implementer done (05-implementer.md, b3265ac): tests pass; new shared helper AttributeListCodeFixUtilities (moved from RH5401).
- Cleaner clean (07-cleaner.md, 0579d84).
- Spec checker (08-spec-checker.md, 3fca7b3): findings — (1) Release.ruleset lacks RH5419 entry -> implementer repair (09-implementer.md); (2) change-doc item 5 awaits batch-2 answer.
- Ruleset repaired (09-implementer.md, 90e48de). Repair round 1 of 3 (spec checker findings).
- Batch 2 output question posted (block comment in one-line parameter list). Answered 1A.
- Spec checker re-run consistent (10-spec-checker.md, 55e4af0). Build green. Auditor spawned on head 55e4af0, main 31d2931 (11-auditor.md).
- Auditor (11-auditor.md, 861011f) audited 57a8dea: findings. #1 fix offered with comment/directive between `this` and `[` leaves two lines -> rare class, refuse (withhold fix): spec author amends RH5419-004, test implementer flips 2 tests + adds directive/one-line-list cases, implementer measures one-line check from first token after attribute lists. #2 FC-015 JoinParameterList copies LineBreakTriviaUtilities.JoinGaps / TokenGapAnalysis.HasLineBreak -> implementer reuses them. README Formatter ✔ kept (decision 7).
- Round 2: spec author done (12-spec-author.md, 2b019eb).
- Round 2: test implementer done (13-test-implementer.md, 668497b), 12 expected failures.
- Audit 2 (16-auditor.md, 1b9e517, on 84a0f1e): #1, #2 closed. New #3 (FC-015, RH5419/RH5401 one-line-span check duplicated -> one shared method; implementer) and #4 (RH5419-004 cause list omits bare line break the formatter keeps in front of `[`, e.g. `public\n int this[...]`; spec author words it, test implementer adds a withheld test LF/CRLF). #3 is a different decision from #2 (formatter join vs. RH5401 span), so not a repeated class.
- Round 3: spec author done (17, 647eeba); implementer done (18, 37635a0/f2dde25): shared StandsOnOneLineKeepingBlankLines, tests green, format clean.
- Round 3: test implementer done (19, f5fa77f), 98/98. Cleaner skipped (format clean, Release 0 warnings per 18; decision 8 reasoning). Spec checker (20) next.
- Round 3: spec checker consistent (20, be039f7). Auditor (21) spawned.
- Audit 3 (21, e5567e0): PASS, text cleanup. AUDITED SHA: e6151e9. #5 spec phrase in RH5419-004 -> spec author (22); hint: redundant inline comment in RH5419 provider -> cleaner (23). Prove with verify-text-only --base e6151e9. Then validator (24), publish.
- Text cleanup done: spec author (22, 97ee25b), cleaner (23, bbd78bc). verify-text-only vs e6151e9 run on HEAD. Next: validator (24).
- Repair round 3 of 3 (last): spec author (17) || implementer (18) -> test implementer (19) -> spec checker (20) -> auditor (21, repair only) -> validator (22).
- Round 2: spec checker consistent (15-spec-checker.md, 65c91ea). Auditor (16) spawned.
- Round 2: implementer done (14-implementer.md, 381f12c/689e2e0): tests pass, full analyzer 11371/11371, Release 0 warnings, format.sh no change. Cleaner skipped (decision 8). Next: spec checker (15), auditor (16, repair only), validator (17).
- Repair round 2 of 3: spec author (12) -> test implementer (13) -> implementer (14) -> cleaner (15) -> spec checker (16) -> auditor (17, repair only) -> validator.
- Next (old): spec checker (08), batch-2 output question, auditor, validator. Next: route answers to spec author (status Accepted), then test implementer.
