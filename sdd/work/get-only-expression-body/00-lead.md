# Lead — get-only-expression-body

- Issues: thoenissen/Reihitsu#849 (`agent-filed`, goals are proposals), thoenissen/Reihitsu#850 (`agent-filed`, goals are proposals)
- Slug: `get-only-expression-body`
- Branch: `claude/awesome-dijkstra-fyihwi` (designated by the session)
- PR: thoenissen/Reihitsu#1868 (draft claim)
- Lane: behavior change / new rule
  - #849: requests a new Clarity analyzer rule (and code fix) reporting get-only properties that the formatter collapses to an expression body (formatter behavior from #848, merged in thoenissen/Reihitsu#854).
  - #850: the same for indexers, as a separate rule. Same mechanism and component as #849, so one run.
- Roles run: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publishing if follow-up candidates exist. None skipped.
- Maintainer clarifications: none beyond the prompt.

## Maintainer answers

Batch 1 (before test implementer):
- Q1 (two rules RH3209/RH3210, Warning, on by default, span `{`..`}`, fix + Fix All, not-reported shapes, version gates, rule pages): **A** — as drafted.
- Q2 (comment the formatter keeps the accessor list for, e.g. `public int A // note` / comment above `get`): **B** — reported on the accessor list, no fix offered.
- Q3 (comment/directive the formatter refuses as unsupported between `get` and `=>` or `=>` and expression): **A** — reported, no fix.
- Q4 (fix keeps the rest of the signature as written, `public  int  A => _value;`): **A**.
- Q5 (escalation 10, declare comment/directive inside a get-only accessor list unsupported): not declared — the maintainer said this is not unsupported, since it is clear how such code is formatted. Read as 5A.
- Q7 (directive or disabled text the formatter keeps a get-only member's accessor list for, e.g. `#if WRITABLE set ... #endif`, or `#if TRACE get => ... #else get { return 0; } #endif`): **B** — reported on the accessor list, no fix offered.
- Q6 (escalation 9, move the get-only predicate/conversion into one new `Reihitsu.Core` type used by formatter, both analyzers, both fixes): **A**.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Start | Batch #849 and #850 into one run? | Yes | They share one mechanism (the formatter's get-only collapse) and one lane; two rules | Internal | b3e1523 |
| 2 | Skeptic | Issue goals: rule page records withheld fixes; README row; coordinate IDs with #628; drop #628 indexer row | Drop all four | Rule pages list no withheld fixes (general-notes covers it); README has no rule table; #628 closed, RH3209/RH3210 free; #628 already updated | Text/internal only | — |
| 3 | Skeptic | SHARED-GETONLY-006 queries the unsupported-syntax check on the original declaration | Keep the query, narrow it to the getter, state its reason as SHARED-FIX-003 | Built node loses the comment, so SHARED-FIX-010 alone would offer a dropping fix | Same visible result either way | — |
| 4 | Skeptic | Who writes the gap in front of `=>` in the fix | Shared converter + node-level format, splice arrow clause/semicolon onto signature as written (RH3202 precedent) | Avoids predicting the spacing phase | Internal mechanism | — |

| 5 | Spec 04 | Merge SHARED-GETONLY-003 (comments) and -007 (directives)? | Keep separate | Boundaries and reasons differ | Same visible outcome either way | 27fe5de |
| 6 | Spec 04 | Show fix output for a conditional group inside the getter's expression? | No; tests assert the fix is offered and equals the formatter's conversion | Avoid pinning the formatter's known indentation quirk (follow-up candidate) | Fix output follows the formatter (SHARED-GETONLY-005) | 27fe5de |
| 7 | Tests 05 | `DelimitedCommentTrailingTheExpressionStaysBehindIt`: `{ get => _value /* c */; }` reported with fix writing `=> _value /* c */;`? | Yes, test stands; spec author clarifies SHARED-GETONLY-003 wording ("in front of the getter's semicolon" only where the conversion deletes or joins that trivia) | The formatter converts this member (it does not keep the list), so Q2 B does not apply; SHARED-GETONLY-005 makes the fix write the formatter's result | Decided by Q2 B's scope and the unchanged SHARED-GETONLY-005; output equals today's formatter output | d7fb059 |
| 8 | Impl 06 | `FixKeepsCarriageReturnLineFeedLineEndings` (RH3209) compares a single-fix application against a two-diagnostic expected text | Test is wrong in mechanics; test implementer splits it so each CRLF fix check has one diagnostic | Test harness applies only the first fix in that step | Asserted outputs unchanged; only the test's structure | — |
| 9 | Impl 06 | AGENTS.md phase-table guard renamed to `GetOnlyMemberExpressionBodyRewriteUtilities.ContainsBlockingDirective` | Accept | Required by `PhaseOwnershipDocumentationTests` after the Q6 A move | Contributor doc only | 08a771a |
| 10 | Impl 06 | Core type also owns accessor-level conversion; new analyzer/code-fix base classes | Accept | Get-only conversion is built on the accessor conversion; one code path for both rules | Internal structure; formatter output unchanged (5908 tests green) | 08a771a |
| 11 | Impl 06 | Misspaced signature + multi-line expression: continuation lines keep formatter columns | Accept as known limit, same as RH3202 fix | Rare (input already violates spacing rules), not destructive; follows Q4 A | Covered by Q4 A; no code is dropped | 08a771a |
| 12 | Audit 11 (repair round 1) | Architecture finding: the new fix's "keep signature, splice formatter body" duplicates `ExpressionBodyCodeFixUtilities.ConvertBodyAsync` and has drifted (shared-line handling, finding 1) | Change the structure in this run: the get-only fix reuses the RH3202 family's existing splice/shared-line path instead of its own copy; if that needs more than about 100 production lines, apply the narrow wrapper fix (`FormatMemberOnLinesOfItsOwnAsync`) and file the shared splice as a follow-up | The narrow fix alone would keep a second statement of the splice; both owners are in CodeFixes, a component the run changes | Expected output is decided by the unchanged SHARED-FIX-007 (shared line) | — |
| 13 | Audit 11 | Findings 2 (missing SHARED-GETONLY-003 own-line comment tests) and 3 (code owners incomplete) | Repair: test implementer, spec author | Same requirement | Tests of existing behavior; doc header | — |
| 14 | Tests 12 | Shared line across a comment (auditor case e and mirror: `public int A { get => 1; } /* c */ public int B => 2;`) | Rare class (input already breaks one-member-per-line, comment in the gap): the fix is withheld for it; the two tests assert no fix offered; the RH3202 family's keep-on-one-line behavior becomes a follow-up candidate | AGENTS.md Code-fix coverage: rare class is refused by a cheap check, never modelled | Withholding a fix for a rare class puts nothing in question | — |
| 15 | Tests 12 | Line that does not begin with a member (type written on one line, sibling's wrapped initializer, multi-line literal tail) | SHARED-GETONLY adopts the same exception RH3202-002 / RH5417-003 state, through the shared-line helper the fix reuses; test for a one-line type added | One owner of the shared-line rule; consistent with the sibling family | Decided by the existing RH3202-002/RH5417-003 requirements the reused helper implements | — |
| 16 | Impl 16 (repair round 1) | Fix All switched from BatchFixer to `DocumentOrderFixAllProvider` | Accept | Needed by the shared-line Fix All tests; RH3202/RH5417 precedent | Fix All results equal one-at-a-time results (SHARED-FIX-007) | af77361 |
| 17 | Impl 16 | Structural option of decision 12 taken: `ExpressionBodyCodeFixUtilities.ReplaceBodyAsync` shared by RH3202/3203/3208 and RH3209/3210 | Accept (~40 code lines, no new type) | Resolves the audit's architecture finding | RH3202 family outputs unchanged (analyzer project 11052/11052 green) | af77361 |
| 18 | Audit 21 | Findings 4 (doc comment `/** c */` in the sibling gap escapes the comment-gap check; repeats finding 1's class) and 5 (private `IndexOf('\n')` line-break test instead of the Core helper) | Architecture finding: refuse the class by its cause — the fix is withheld when the gap between the member and a sibling member holds any trivia other than whitespace and is not separated by an end of line as the Core helpers (`SyntaxTriviaUtilities.EndsLine`/`AreSeparatedByEndOfLine`) define it; so a multi-line comment gap is withheld too. Not a repair round | Enumerated comment kinds miss the next kind; one cheap check on the cause, using the Core definition of a line break | Withholding a fix for a rare class (input already breaks one-member-per-line) puts nothing in question | — |
| 19 | Audit 21 | One-line type: member converted in place, nothing separated | Keep decision 15 | Adopts the existing RH3202-002 / RH5417-003 exception through the shared helper | Covered by those unchanged requirements | — |
## Follow-up candidates

- A balanced `#if` group inside a wrapped argument list in an expression body is indented inconsistently (active branch column 16, disabled branch and `)` column 22); existing formatter behavior (02-skeptic)

- Five copies of "remove trailing whitespace trivia" across Formatter, Core, CodeFixes could share one Core owner (06-implementer)
- The RH3202 family's fix (`BlankLineCodeFixUtilities.FormatMemberOnLinesOfItsOwnAsync`) keeps a converted member and a sibling on one line when a comment sits in the gap between them, against SHARED-FIX-007 and the formatter (12-test-implementer, `FixKeepsACommentBetweenTheConvertedMethodAndTheMemberBehindItAndDoesNotSeparateThem`); the same family also misses `/** c */` documentation comments in that gap (21-auditor)

## Worklist / next step

- Spec author done (89106f5): RH3209 (properties), RH3210 (indexers); change doc Draft; one output question open (comment the formatter keeps the accessor list for: A not reported, B reported without fix).
- Skeptic done (56e46fa): proceed. Architecture finding: shared Core owner for the get-only predicate/conversion (~250 moved lines + new type) -> escalation 9. Escalation 10 named (declare comment/directive in get-only accessor list unsupported; skeptic recommends no).
- Amendments pending for spec author: change doc shows signature-kept example; SHARED-GETONLY-006 narrowing (decision 3); directive example in visible change 5.
- Batch 1 answered (see Maintainer answers).
- Spec author 03 done (25e5014): answers carried out, change doc Accepted, Core type `GetOnlyMemberExpressionBodyRewriteUtilities`, SHARED-GETONLY-007 added (directive the formatter keeps the member for: not reported, drafted).
- Q7 answered B.
- Spec author 04 done (27fe5de): Q7 carried out.
- Test implementer 05 done (d7fb059): 68+4 analyzer tests + 6 formatter-parity tests; build fails only on the four missing rule types.
- Pending text item: spec author clarifies SHARED-GETONLY-003 wording (decision 7), after the implementer.
- Implementer 06 done (08a771a, e940dcd): all tests green except disputed CRLF test.
- Test implementer 07 (706343d) and spec author 08 (c458cf6) done.
- Cleaner 09 done (ee8b0a6): clean, Release 0 warnings.
- Spec checker 10 done (48888c2): consistent, no findings. Merged main 6d574e1 (comment-only, no overlap). Build green.
- Auditor 11 (972b3fc): findings — #1 high shared line (SHARED-FIX-007), #2 medium missing test, #3 low code owners; architecture finding decided (decision 12).
- Spec author 13 (1976629), test implementer 12 (6fb5a80) done; decisions 14, 15.
- Spec author 14 (9095b22, SHARED-GETONLY-008) and test implementer 15 (c191c72; 26 expected failures) done.
- Implementer 16 done (af77361, d170808): all green.
- Cleaner 17 done (400352d): member reorder only, Release clean.
- Spec checker 18 (03aa494): one low finding — SHARED-GETONLY code owners miss ExpressionBodyCodeFixUtilities (and possibly BlankLineCodeFixUtilities).
- Spec author 19 (e6012b7): owners named in intro (one-spec-per-type rule). Merged main e3cf4cd (formatter line-break changes); Release build green; RH3202/3203/3208/3209/3210 tests 311/311.
- Spec checker 20 consistent. Auditor 21 (5819614): findings 4 (high, repeats class) and 5 (medium) -> decision 18 (architecture finding, refuse by cause; no repair round used).
- Spec author 22 (4040015), test implementer 23 (48f23ae; 24 expected failures) done.
- Implementer 24 (7350c45): green.
- Cleaner 25 clean, spec checker 26 consistent, auditor 27 (562dfe3) PASS. Audited SHA: 65ac481.
- Next: validator 28 + issue manager 29 in parallel; then note, delete sdd/work, trigger, PR. (old:) implementer 24, cleaner, spec checker, auditor (delta), validator. (old:) spec checker 18, auditor 19 (repair only, audited base d7990b4), validator 20.
- (old) Repair round 1: test implementer 12 (shared-line tests + finding 2) and spec author 13 (finding 3) in parallel; then implementer 14; then cleaner, spec checker, auditor (repair only), validator.
