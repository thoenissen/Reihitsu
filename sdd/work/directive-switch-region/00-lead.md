# Lead — directive-switch-region

- Issues (prompt order): thoenissen/Reihitsu#1385 (agent-filed), thoenissen/Reihitsu#1398 (agent-filed), thoenissen/Reihitsu#1239 (agent-filed). All goals are proposals.
- Slug: `directive-switch-region`
- Branch: `claude/nifty-hypatia-urch1j` (session-mandated branch, see Decisions #1)
- Draft PR: thoenissen/Reihitsu#1657
- Lane: **behavior change** (widened from bug fix, Decision 4)
  - #1385: formatter part only (triage comment split the RH5002/RH5008 code-fix part into #1447). Bug against FMT-BLANK-016, which already requires the blank line after directives; triage says bug-fix lane, no change doc, coverage sentence widened as a clarification.
  - #1398: bug against FMT-SWITCH-006 (break-only section never makes the switch multi-line); triage: bug-fix lane, FMT-SWITCH-004 gains the precedence of -006 as a clarification.
  - #1239: bug against FMT-REGION-007 (a `#region` line is `#region`, one space, description) if that requirement covers whitespace between `#` and `region`. The issue asks the maintainer to confirm that reading; it is a two-readings output question for the batch before the test implementer. A "general directive policy" answer would widen the run to behavior change.
- Roles: spec author (clarifications only) → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator. Issue manager before publishing when follow-ups exist.

## Maintainer answers

- OQ1 (#1385, directive between a `break`-ending section and the next section, incl. a disabled `#if NEVER … #endif` group): **A** — no blank line is inserted; the directive suppresses the section blank line as FMT-BLANK-006 does for statements. With a comment between directive and section, the blank line goes after the directive (not in question).
- OQ2 (#1239, whitespace between `#` and `region`/`endregion`): **A** — every paired region line the formatter rewrites is written without whitespace after `#` (`#region Fields`, `#endregion // Fields`, `#region`, `#endregion note`, tab case too, also in disabled branches). RH7301 fix output unchanged. Other directives out of scope (follow-up).

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 2 | Skeptic | Should the #1239 change go into the Core helper `RegionDirectiveUtilities.CreateCanonicalEndRegionTrivia`, shared with the RH7301 code fix? | Keep any #1239 change inside the formatter's region phase; the RH7301 fix output stays as on `main` | Avoids putting a code-fix result in question; scope stays on the formatter | RH7301 output unchanged | — |
| 3 | Skeptic | FMT-BLANK-016 coverage list omits the blank line before a `break` (FMT-BLANK-004 first clause) | Spec author adds it as clarification wording | Same requirement, existing behavior is wrong against -016 | Wording only; output already demanded by FMT-BLANK-016 | — |
| 4 | Answers | Does OQ1=A need a change doc? | Yes: it changes FMT-BLANK-006's intended behavior (sections added). Lane widens bug fix → behavior change; spec author and skeptic already ran; change doc records 1A in `Decisions`. OQ2=A is a reading of the existing FMT-REGION-007 → clarification, no change doc | sdd/README *When a change doc is required* | Visible changes are the maintainer's answers | — |
| 5 | Answers | Second skeptic run on the change doc? | No — the doc only records the answered output; the skeptic already examined OQ1 and the spec diff | Saves a stage; nothing undecided | Answered | — |
| 1 | Claim | Branch name | Use the session's designated branch `claude/nifty-hypatia-urch1j` instead of `claude/issues-1385-1398-1239-<slug>` | The session environment forbids pushing to any other branch | Branch names are internal | 32e14d4 |

## Follow-up candidates

- Multi-line parenthesized pattern layout is broken (`o is (int⏎or long)`, `case (int⏎or long):`). Source: 02-skeptic.md.
- Open question: a `return`/`throw`-only section with a multi-line label braces the whole switch (`{ return; }`); spec requires it today. Source: 02-skeptic.md.
- (conditional on #1239 answer A) Whether the formatter should normalize whitespace between `#` and the keyword of other directives (`# pragma`, `# if`). Source: 01-spec-author.md.

## Worklist / next step

- Audited SHA: —
- Open output questions (batch before test implementer): OQ1 #1385 directive between a `break`-ending section and the next section (A: no blank line / B: blank line after directive); OQ2 #1239 whitespace between `#` and `region` (A: `#region Fields` / B: keep).
- Done: spec author (01, commit 9ee6cfb, verdict open question).
- Done: skeptic (02, commit 3372b14): #1385 proceed (+OQ1 second input), #1398 proceed, #1239 OQ2.
- Asked OQ1/OQ2; answered 1A 2A.
- Next: spec author (continued) writes change doc + FMT-BLANK-006/-016 amendment + region clarifications; then test implementer.
