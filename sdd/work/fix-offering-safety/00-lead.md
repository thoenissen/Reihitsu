# Lead — fix-offering-safety

- Run: issues thoenissen/Reihitsu#1600, #1150, #1274, #1143, #1673, #1678 (prompt order)
- Slug: `fix-offering-safety`
- Branch: `claude/trusting-babbage-audv24` (session-designated branch; see decision 1)
- PR: thoenissen/Reihitsu#1718 (draft claim)
- Lane: **behavior change** (widened from bug fix after batch 1, decision 8)

## Issues

| Issue | agent-filed | Lane | Reason |
|---|---|---|---|
| #1600 | yes | bug fix | Reported observed-vs-expected difference against SHARED-FIX-003 (fixes/formatter edit text that is string/comment content in another build configuration) |
| #1150 | yes | bug fix | Fixes offered where the formatter changes nothing (SHARED-FIX-005/007); carries spec clarification text the maintainer decided on 2026-11-06 (spec text only, no change doc) |
| #1274 | yes | bug fix | RH5308 fix does not converge on one input (SHARED-FIX-001/005, P7) |
| #1143 | yes | bug fix | Fix All needs a second application in several rules (SHARED-FIX-002) |
| #1673 | yes | bug fix | Fix registered although its action returns the document unchanged (SHARED-FIX-005) |
| #1678 | yes | bug fix | SHARED-FIX-003 gap for reorder fixes; the issue itself is an output question (A: record gap as decision in SHARED-ORDER-008; B: withhold field reorders in every partial type) |

All goals are proposals (all `agent-filed`).

## Roles

Bug-fix lane: spec author (clarification — #1150's maintainer-decided spec text; #1678 per the maintainer's answer), skeptic (premise), test implementer (reproduction), implementer, cleaner, spec checker, auditor, validator. Issue manager once before publishing if follow-up candidates exist.

## Maintainer clarifications (from issues, summarized)

- #1150: maintainer decided 2026-11-06 — in a file with a syntax error the diagnostic stays and the fix is not offered; generated code is never a reason for the formatter to decline a fix's input; a formatter-delegating fix that changes nothing is a defect of the formatter or of the diagnostic, unless a named formatter requirement declines the input. Spec text to write is listed in the issue (new SHARED-FIX requirement after SHARED-FIX-005, edits in pipeline.md FMT-PIPE-005/006 Why, line-join-rules.md SHARED-JOIN-002 Why, RH7101-001 Why, code-fixes.md non-goal).
- #1143: maintainer (2026-10-19) — Fix All must finish in one application; a guard-dependent second pass may stay with a comment naming the guard.
- #1600, #1678: triaged critical for 1.2.1 by the maintainer.

## Maintainer answers

Batch 1 (questions 1–7, 2026-11-14):
- Q1 (#1600, formatter, comment/string hiding code in another build): **B** — whole-document formatting leaves the file unchanged silently, like a syntax error; not declared unsupported.
- Q2 (#1600, string opened only by an excluded branch): **A** — unchanged; gap recorded as a decision in the specs.
- Q3 (#1600, guard reach, escalation 9): **B** — a common base class for the code-fix providers that runs the check before registering (new type approved). Plus a follow-up (tooling/process): revise the escalation rule more openly, not only for base classes — good architecture and design are required, but no large changes for edge cases.
- Q4 (#1274, RH5308 comment/directive behind `?`/`:`): **C confirmed** — declare unsupported in this work: a comment (or directive) between a conditional's `?`/`:` and its branch. Comments are supported only above the `?` or `:` line, so they do not break formatting.
- Q5 (#1678): **A** — unchanged, gap stays recorded in SHARED-ORDER-008. Maintainer's reason: analyzers always analyze with the defines as they currently stand; that is the common case.
- Q6 (FMT-STRUCT-023 field declaration with several declarators holding a directive): **B** — declare unsupported ("no pragmas in declarations"), in a change of its own → follow-up carrying the answer.
Batch 2 (questions 8–10, 2026-11-14):
- Q8 (CLI/Playground for a file skipped under FMT-PIPE-033): **B** — counted as skipped with its own message (`Skipped (code hidden in another build configuration): C.cs`) and a matching Playground message, like the syntax-error skip; not "Unchanged".
- Q9 (inline comment between `?`/`:` and its branch on one line, e.g. `a ? /* yes */ 1 : /* no */ 2`): **A** — FMT-UNSUP-008 as written: every comment in that position is refused (CLI error, exit code 2).
- Q10 (RH5302 twin: comment/directive between a binary operator and its right operand): **A** — declare unsupported in a change of its own → follow-up carrying the answer.

- Q7 (FMT-STRUCT-003 directive between header and unbraced body): **B** — declare unsupported in a change of its own → follow-up carrying the answer.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | claim | Branch name | Use the session-designated branch `claude/trusting-babbage-audv24` instead of `claude/batch-1600-<slug>` | The session environment permits pushing only to that branch | Branch name is internal | claim commit |
| 2 | skeptic | #1150: SHARED-FIX-011 example (RH6023 no longer formats), FMT-PIPE-005 "the one reason" ignoring unsupported-shape refusal, first sentence readable as blanket compare | Route corrections to a fresh spec author after the maintainer's answers; affected-fix list per 02-skeptic.md | Skeptic observed it on this tree; the maintainer's text stays, only example and consistency fixed | Spec wording only; behavior decided by the maintainer's 2026-11-06 answer | pending |
| 3 | skeptic | #1150 item 2 (RH5405–RH5407 directive body) | Drop: already resolved and tested (SHARED-BRACE-003) | Spec author and skeptic both confirm | Nothing changes | n/a |
| 4 | skeptic | #1673 scope | Narrow to RH5308 only; modifier ordering, RH7107, RH7108 do not reach an unchanged result / already check at registration | Skeptic evidence; agent-filed goal ("drop a fix where no input reaches") | Withholding a no-op fix is decided by SHARED-FIX-005 | pending |
| 5 | skeptic | #1143 scope | Proceed on RH6016, RH8204, RH5110, RH3001, RH3102, RH7102 and the remaining `NumberOfFixAllIterations = 2` tests; RH6024/6023/5506/5204 already resolved; RH5104 and RH5405 if/else are guard-dependent (keep pass with comment) | Skeptic evidence; issue allows guard-dependent passes | SHARED-FIX-002 decides the output | pending |
| 6 | lead | #1274 option B (withhold fix) | Dropped from the output question | Contradicts SHARED-FIX-011 (this run) and SHARED-DIAG-010 | Removing an option, not deciding an output |  n/a |
| 8 | batch 1 | Lane | Widen bug fix → behavior change; change doc required | Q1B (new formatter refusal), Q4C (new unsupported shape), Q3B (new type) are visible/structural changes that need a change doc | Lane is internal; outputs decided by the maintainer | pending |
| 9 | 03 spec author | Visible items beyond the answers: inline `a ? /* c */ 1` refused (FMT-UNSUP-008); SHARED-FIX-012 withholds every fix in a document holding the carrier (and renames touching such a document); a file holding both the carrier and an unsupported shape is left unchanged unreported (FMT-PIPE-033 before precheck); RH5308-006 | Accept all | Inline comment: maintainer said comments are supported only above the `?`/`:` line; whole-document withholding and the double-shape case are rare-class refusals (AGENTS.md *Code-fix coverage*); RH5308-006 is a SHARED-FIX-005 bug fix (#1673) | Covered by Q4/Q3 answers or rare-class refusal; no new output question | c679eb1 |
| 10 | 03 spec author | Follow-up "true branch after `// c` on `?` line lands in column 8" | Drop: resolved by FMT-UNSUP-008; the wider candidate keeps only `b + // c` and `int A => // c` | Shape now refused as unsupported | n/a | c679eb1 |
| 11 | 04 skeptic | RH5308 keeps reporting a conditional refused under FMT-UNSUP-008 (RH5308-005) vs SHARED-DIAG-010's literal text | Keep reporting; spec author adds one sentence to SHARED-DIAG-010 that a shape refused as unsupported is not a shape the formatter keeps | Maintainer chose Q4 C (declare) over B (not reported), so the diagnostic stays | Covered by the Q4 answer | pending |
| 12 | 04 skeptic | FMT-PIPE-033 also refuses a carrier holding a balanced `#if … #endif` (commented-out code, templates inside a conditional group) | Keep the definition; correct the Why | Rare class (carrier inside a conditional group); refusal is the safe side | Rare-class refusal (AGENTS.md *Code-fix coverage*) | pending |
| 13 | 04 skeptic | Internal spec/change-doc corrections (04-skeptic.md *Change doc corrections*, wording of FMT-PIPE-003, SHARED-ORDER-008, "one check" wording) and SHARED-FIX-012 coverage notes (CasingFixAllProvider, RH2001 Renamer, per-tree cache) | Route to fresh spec author with batch-2 answers; coverage notes go to test implementer/implementer via the handover | Internal | Internal | pending |
| 14 | 05 spec author | Visible details beyond answer Q8 B: summary line text, Playground message text, per-file line only with `--verbose`, such a file no longer affects the exit code (was 1 under `--check`), `--utf8-bom` no longer rewrites it, a file with both the carrier and an unsupported shape gets the skip line (no `error:`/exit 2) | Accept | All follow "like the syntax-error skip" (CLI-008: counted as skipped, no effect on exit code); double-shape file is a rare class | Covered by Q8 B | b80864a |
| 15 | 05 | Change doc acceptance | Set `Accepted` | Every visible output change has the maintainer's answer (batches 1–2) or is covered by decisions 9/14 | n/a | this commit |
| 16 | 06 test impl | #1143 second passes kept: RH5104 (`HasCommentBeforeOnLine`), RH5405 (SHARED-BRACE-003), RH5202 (RH5202-006), RH7310/RH3002 (second diagnostic appears only after the first fix) | Accept | Guard-dependent passes are allowed by the issue and SHARED-FIX-002; each carries a comment | SHARED-FIX-002 decides | 53f6660 |
| 17 | 06 test impl | Existing tests whose inputs hold FMT-UNSUP-008 rewritten to the refusal, 4 phase-level ternary join tests deleted (guards deleted per change doc) | Accept | Accepted change doc *Special cases* | Covered by Q4 C / Q9 A | 53f6660 |
| 18 | 07 implementer | Two SHARED-FIX-012 Fix All tests (RH4103, RH6024 `FixAllLeavesADocumentWithAHidingCarrierUnchanged`) lack `MarkupHandling = MarkupMode.Allow` for the diagnostic that stays | Test setup wrong, asserted output correct: route to a fresh test implementer | Setup only; expected diagnostic is what SHARED-FIX-012 demands | Asserted output unchanged | 0b5dfd9 |
| 19 | 07 implementer | Scope: 105-file mechanical move onto `ReihitsuCodeFixProviderBase`; new public `ReihitsuFormatter.HoldsHidingCarrier` (Playground/CLI need it); starting-code filter in shared `DocumentOrderFixAllProvider` (affects its earlier users, all tests pass); `PlaygroundFormatResult.HoldsHiddenCode`, `FileProcessResult.SkippedHiddenCodeCount` | Accept | Base type approved (Q3 B); no assembly is public API; the filter is SHARED-FIX-002 as clarified (fixes available on the starting document) | Fix All results only narrow toward SHARED-FIX-002; no answered output changes | 3c1336e |
| 20 | 07 implementer | `SameLineGapRulePhaseTests` `b ?/** y */1` input no longer reachable | Leave as is; no follow-up | Unreachable input is dropped (AGENTS.md *Supported inputs*); the test still passes | Test only | n/a |
| 21 | 13 auditor | Architecture finding (audit #1): `ReihitsuCodeFixProviderBase` and the custom `FixAllProvider.Create(ApplyFixAllAsync)` delegates of RH3002/RH7101 both decide whether a fix is offered; the delegates skip SHARED-FIX-012 | Change the structure in this run: the base owns the per-document decision and every Fix All path asks it; routed as repair round 2 (test implementer: project-scope Fix All tests for RH3002/RH7101; implementer: the change) | Narrow per-delegate copy would add a second statement of the rule; stays within the components the run changes | SHARED-FIX-012 already decides the output | pending |
| 22 | 13 auditor | Audit #2 (word-boundary rows for `HoldsGroupEndingLine`), #3 (CLI calls static `HoldsHidingCarrier` instead of `ISourceFormatter` seam) | Repair both in round 2 | Same requirement / PR-introduced | Tests and internal seam only | pending |
| 23 | 13 auditor | Hints: first-pass-only starting filter in `DocumentOrderFixAllProvider` (no reachable input found), no arch test tying `FormatsThroughFormatter` to formatter calls, XML summary line breaks | No action | No reachable input; all 29 overrides match; cosmetic | Nothing visible | n/a |
| 24 | 15 implementer | No guard stops a future provider from calling `FixAllProvider.Create` directly and bypassing the base | Add an architecture test in this round (only the base and `DocumentOrderFixAllProvider` may call it) | Keeps decision 21's single owner enforced; small, in the component the run changes | Test only | 079709c |
| 7 | lead | #1600 fix-side line kinds | `#elif`/`#else`/`#endif` lines in a multi-line string or `/* */` that starts inside a conditional group, not any `#` line | A plain `#` check withholds fixes on Markdown raw strings; skeptic shows only these lines can hide code | Only offers more fixes than the issue's wording; rare-class withholding | pending |

## Follow-up candidates

- Playground keeps a private copy of the formatter's syntax-error check (FC-015; pre-existing) — 07-implementer.md.
- `scripts/trace.sh` reports skips for syntax errors and generated files but not for files skipped under FMT-PIPE-033 — 07-implementer.md.

- Declare unsupported (maintainer answer Q10, 2026-11-14): a comment or directive between a binary operator and its right operand (RH5302 twin of FMT-UNSUP-008), in a change of its own; deletes the join refusal in `BinaryOperatorLineBreakRewriter` and RH5302-005's fix-side copy and RH5302-005 — 04-skeptic.md *Whole run*.

- FMT-BLANK-011 / RH5020 put a blank line in front of a comment above a wrapped `?`/`:` inside an expression (cuts against the maintainer's "comments above do not break formatting"); carries an output question — 04-skeptic.md.
- `N(/* c7 */ 2)` is formatted to `N(/* c7 */2)` — 04-skeptic.md.

- Declare unsupported (maintainer answer Q6, 2026-11-14): a preprocessor directive or disabled text inside a field declaration with several declarators, from its first token through its `;` (FMT-STRUCT-023), in a change of its own; deletes `FieldDeclarationSplitTransform.CarriesDirective`, the directive condition of `SplitsFieldDeclaration` (FMT-PIPE-031), RH7101-005 — 02-skeptic.md *Whole run*.
- Process/tooling (maintainer answer Q3, 2026-11-14): revise the escalation-9 rule (squad/README.md, AGENTS.md) more openly than only base classes — good architecture and design are required; what is ruled out is large changes for edge cases.
- Declare unsupported (maintainer answer Q7, 2026-11-14): a preprocessor directive between an embedded statement's header and its unbraced body (FMT-STRUCT-003), in a change of its own; deletes the structural phase's brace guard for it — 02-skeptic.md *Whole run*.

- #1150: SHARED-FIX-011 third sentence conflicts with RH5401-004 (RH5401 reports a property the formatter keeps) — 02-skeptic.md.
- Formatter class: a continuation line whose previous line ends with a `//` comment is laid out in the enclosing statement's/member's column (`b + // c` → `1;` in column 8, `int A => // c` → `1;` in column 4) — 02-skeptic.md #1274; the `?` member is dropped (decision 10).

## State

- Last step finished: 01 spec author (e36792d): SHARED-FIX-011 written for #1150; open output questions drafted for #1600, #1274, #1678.
- 02 skeptic done (14a8b5a).
- Batch 1 answered (see Maintainer answers).
- 03 spec author done (c679eb1): change doc `sdd/changes/2026-11/fix-offering-safety.md` (Draft), FMT-PIPE-033, SHARED-FIX-011/012, FMT-UNSUP-008, RH5308-005/006. Lane check behavior-change confirmed.
- 04 skeptic done (c4ea7c2); batch 2 answered (Q8 B, Q9 A, Q10 A).
- 05 spec author done (b80864a); change doc accepted.
- 06 test implementer done (53f6660 tests, 3493e63 handover): failing today — analyzer 67, formatter 65, cli 8, playground 3.
- 07 implementer done (3c1336e code, 0723292 handover): all tests pass except 2 disputed (setup).
- 08 test implementer done (0b5dfd9): both disputed tests fixed (setup), fail on 53f6660 for the right reason.
- 09 cleaner clean (dfff379).
- 10 spec checker (6595b16): 1 finding — change doc does not mention SHARED-FIX-011 / RH5308-006. Repair round 1: route to fresh spec author (11), then spec checker again (12).
- 11 spec author (a63cc2f): change doc #6, #7 and two Decisions added (docs only; cleaner skipped, no compiled change).
- 12 spec checker consistent (0957621). Merged origin/main 23cf11a (clean, build green 0 warnings, no provider left on bare CodeFixProvider).
- 13 auditor (c4cfd1e) audited 9d38a0b vs main 23cf11a: findings #1 (high, architecture), #2, #3 on #1600; others pass.
- Repair round 2: test implementer (14) → implementer (15) → cleaner (16) → spec checker (17) → auditor on the repair (18).
- 14 test implementer (417800b tests, 78a3fce handover): 4 RH3002/RH7101 Fix All tests fail today; word-boundary rows pass.
- 15 implementer (66a181f, 819807e): audit #1 and #3 repaired; full analyzer/formatter/CLI/architecture green.
- 16 test implementer (079709c): architecture test added, passes on head, fails on 417800b.
- Next: cleaner (17), spec checker (18), auditor on the repair (19). After answers: fresh spec author (spec corrections for #1150, answers' spec text/change doc; lane may widen to behavior change), then test implementer.
