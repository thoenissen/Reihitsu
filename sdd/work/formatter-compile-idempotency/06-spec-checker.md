Role: spec checker
Commit: bbee7c0 (diff `origin/main..bbee7c0`; `src/` production code last changed in 2dea131, tests corrected in 5a6472f)
Verdict: consistent (no findings for the spec author, implementer, or test implementer; three items for the lead under "For the lead")

I changed nothing except this handover. My prompt carried no intended fix, suspected cause, or hoped-for verdict.

How the checks were run: `git diff origin/main` over `sdd/specs/`, `documentation/rules/`, `sdd/specs/requirement-ids.txt`, and the test files; a script comparing every added or changed test method's `[Requirement]` tags in the diff against the `Tests` tables of `03-test-implementer.md` (it flagged only two parenthetical mentions, see check 8); `scripts/test.sh --project architecture`; `scripts/test.sh --project analyzer --filter "FullyQualifiedName~RuleDocumentation"`.

- `scripts/test.sh --project architecture`: `Passed!  - Failed:     0, Passed:    84, Skipped:     0, Total:    84` (includes `SpecConsistencyTests`, `SpecStructureTests`, `RequirementCoverageTests`).
- `scripts/test.sh --project analyzer --filter "FullyQualifiedName~RuleDocumentation"`: `Passed!  - Failed:     0, Passed:     7, Skipped:     0, Total:     7`.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None._ | | | | |

## thoenissen/Reihitsu#948

- Spec: FMT-SWITCH-014 added in `sdd/specs/formatter/switch-case-braces.md` (requirement format: one heading, normative sentence first, `Why:`, examples, input/output pairs; the "unchanged" examples carry the FMT-BLANK-002 blank line so the whole pipeline leaves them alone). `Uses` of the spec gains FMT-BLANK-011 and FMT-PIPE-004 (for FMT-SWITCH-007, #949). Code owners (`SwitchCaseBracePhase`, `SwitchCaseBraceRewriter`) unchanged and still correct: the diff adds a method to `SwitchCaseBraceRewriter`, no type.
- Matches the diff: `HasCrossSectionLocalUse` / `GetDirectlyDeclaredLocalNames` implement the declaration forms listed (local declaration, `const`, deconstruction, local function, `out var`/pattern in expression statement, `return`, `throw`, `yield return`, `if` condition, nested switch governing expression, `lock`, each behind labels) and the self-scoped exclusions (04-implementer.md describes both lists identically).
- Change doc: none, and none required: FMT-SWITCH-014 writes down FMT-PIPE-003 for this input (maintainer's earlier #919 Q1 decision, whole switch unchanged like FMT-SWITCH-011); the code is brought back to the spec.
- IDs: FMT-SWITCH-014 follows FMT-SWITCH-013 in `requirement-ids.txt`.
- Tests: `SwitchSectionSharedLocalTests` (new): every added test carries `FMT-SWITCH-014` (the issue reproduction also `FMT-PIPE-003`). The tags equal the `Tests` table; the table's mention of FMT-BLANK-002 and FMT-SWITCH-008 sits in the test description, not in its requirement column, so no tag is owed. The four unchanged examples (local, pattern variable, local function, `const` label) and the braced examples (own-section local, nested block / `foreach` header) are tagged.
- Open questions: none.

## thoenissen/Reihitsu#949

- Spec: FMT-SWITCH-007 clarified in `switch-case-braces.md`: a comment leading the terminal `break` stays in front of it behind the new block, with the FMT-BLANK-011 blank line already in the pass that inserts the braces; blank lines between several such comments are kept. Two new examples (single comment, two comments). Header `Uses` gains FMT-BLANK-011 and FMT-PIPE-004 (IDs exist in the ledger).
- Matches the diff: `BuildTerminalBreakLeadingTrivia` in `SwitchCaseBraceRewriter` keeps the blank lines in front of each comment and drops blank lines no comment follows, as the sentence says.
- Change doc: none required (bug fix; FMT-BLANK-011 and FMT-PIPE-004 already require the result). No new ID.
- Tests: `TerminalBreakCommentTests` (new) tags `FMT-SWITCH-007` (+ `FMT-BLANK-011`, `FMT-PIPE-004` on the reproductions); the RH5020 formatter parity tests tag `FMT-BLANK-011` / `FMT-SWITCH-007`. Both spec examples are tagged on at least one test (`CommentBeforeTerminalBreakGetsBlankLineInThePassThatInsertsTheBraces`, `BlankLineBetweenCommentsBeforeTerminalBreakIsKept`).
- Open questions: none (the skeptic's blank-line-between-comments question was answered A and carried into the spec).

## thoenissen/Reihitsu#954

- Spec: FMT-BLANK-007 and FMT-BLANK-013 in `sdd/specs/formatter/blank-lines.md` each gain one example (empty region directly after `{`; comment first after `{` followed by `#region`, which gets a blank line below the comment). Both state behavior SHARED-BLANK-005 and SHARED-BLANK-006 already require (`sdd/specs/shared/blank-line-spacing.md:215`: the line directly above a region directive must be blank unless it ends with `{`; a comment there does not satisfy it). Code owners of `blank-lines.md` already list `BlankLineEditor`, `BlankLineRegionDirectiveRewriter`, `BlankLineTriviaBoundaryRewriter`; `TokenGapNormalizer` is already an owner of `line-breaks.md`. No type added, removed, or renamed in the whole diff (checked: no new `class`/`struct`/`record`/`interface`/`enum` in `src/` outside tests, all 13 production files are `M`).
- Change doc: none required (clarification plus bug fix; maintainer answer Q2 -> A makes the comment variant a bug against SHARED-BLANK-005).
- Tests: `RegionDirectlyAfterOpeningBraceTests` (new) and the three parity tests tag `FMT-BLANK-007`, `FMT-BLANK-013`, `SHARED-BLANK-005`, `SHARED-BLANK-006` as in the table; the corrected `RegionFormattingFullPipelineTests.EmptyRegionInsideBlockIsPreservedAndFormatted` carries `FMT-BLANK-007` (listed in the table as "corrected"). Both new spec examples are tagged (`FormattedEmptyRegionAfterTypeBodyBraceIsUnchanged` / `EmptyRegionAfterTypeBodyBraceGetsNoBlankLineAfterTheBrace` for FMT-BLANK-007; `CommentFirstAfterBraceIsSeparatedFromEmptyRegionBelow` for FMT-BLANK-013). The `SHARED-BLANK-007` tag on `InactiveEmptyRegionDirectlyAfterOpeningBraceIsUnchanged` names an existing requirement, in the table.
- Open questions: none in the spec author's handover. The implementer's two follow-up candidates about the same area are listed under "For the lead".

## thoenissen/Reihitsu#1390

- Specs, all consistent with the diff:
  - `structural-transforms.md`: FMT-STRUCT-025 added (async function of a task type that is not written as `Task`/`ValueTask`/`Task<…>`/`ValueTask<…>`, with or without `?`, keeps its expression body; examples for alias/custom kept, qualified and nullable converted); FMT-STRUCT-004 clarified (nullable annotation, pointer to FMT-STRUCT-025); FMT-STRUCT-006 clarified (own-line comments around the arrow lead the created statement in source order, `return /* c */ 42;`, documentation comment / arrow-line comment / expression-statement-or-`throw` in front of an expression left unspecified) with a new four-member example. The implementation matches: `RequiresAsyncExpressionBodyPreservation` is the single predicate, `CreateBlock` and `CreateReturnStatement` handle exactly the comment cases listed and keep today's behavior for directives, documentation comments, and the arrow-line comment.
  - `line-breaks.md`: FMT-BREAK-021 names the `with` initializer; the `Why` states no rule reports it; new example. `VisitWithExpression` in `LineBreakInitializerRewriter` implements it.
  - `shared/expression-body-conversion.md`: SHARED-BODY-004 added (rule keeps reporting, code fix withheld for an `async` member FMT-STRUCT-025 keeps; a nullable-annotated task type is converted); SHARED-BODY-003 points to it; `Uses` gains FMT-STRUCT-025. `analyzer/RH3202.md` and `analyzer/RH3208.md` list SHARED-BODY-004 under `Uses` and name it in the introduction. The ledger line SHARED-BODY-004 / FMT-STRUCT-025 sits at its sorted position.
  - Code-fix side: `ExpressionBodyToBlockCodeFixProviderBase{TNode}.cs` and `RH3202…CodeFixProvider.cs` register no action when the predicate holds; both are owners listed in the specs (`RH3202.md` code owners, `SHARED-BODY` header), and the analyzers are unchanged, matching "the member stays reported".
- User documentation: `documentation/rules/RH3202.md` and `RH3208.md` gain `## Exceptions` with one bullet each, placed between "How to fix it" and "Examples" as `create-rule-doc` prescribes; no formatter mention, no compiler error code, short, footer present; `### Correction` blocks unchanged (the fix output for the example did not change); the bullet matches SHARED-BODY-004 (alias or custom task type, "with or without `?`"). `RuleDocumentation` tests pass (7/7). Both rules sit in the Clarity range RH3xxx; `helpLinkUri` pages exist.
- Change doc: none, and I judge none required. FMT-STRUCT-025 instantiates FMT-STRUCT-024 / FMT-PIPE-003 as decided earlier for the same family (the spec author cites #1371 Q-S1); SHARED-BODY-004 brings a code fix that produced non-compiling code (CS1997) back to SHARED-FIX-003 and the maintainer's explicit answer in this run (Escalation 1 Q1 -> B); FMT-STRUCT-006 and the nullable look-through are the already-required "comment carried into the block" and "the output compiles" behavior. If the lead reads SHARED-BODY-004 as new intended behavior of RH3202/RH3208 rather than a bug fix, `sdd/README.md` ("When in doubt, ask the maintainer") means a change doc; see "For the lead".
- Tests: `AsyncUnknownTaskTypeExpressionBodyTests`, `ExpressionBodyArrowCommentTests`, `WithInitializerLayoutTests`, and the RH3202/RH3208 analyzer tests tag exactly the requirements in the table (FMT-STRUCT-025 + FMT-PIPE-003, FMT-STRUCT-006 (+ FMT-PIPE-013), FMT-BREAK-021 + FMT-PIPE-004, SHARED-BODY-004 + SHARED-BODY-003); the corrected existing tests `PreservesCommentBeforeArrow` (method, constructor) and the two `ExpressionBodyNextLineExpressionTests` tests now carry `FMT-STRUCT-006`, as the table lists. Every example the spec author named under `Examples for the tests` is tagged on at least one new or newly tagged test, including the nullable and qualified forms and the RH3202 and RH3208 SHARED-BODY-004 blocks. The disputed analyzer test was fixed in 5a6472f by the verifier's markup handling only; its tag is unchanged.
- Open questions: none (skeptic Q1 answered B and carried into FMT-STRUCT-004/-025, SHARED-BODY-004, both rule pages).

## thoenissen/Reihitsu#888

- Spec: FMT-BREAK-019 clarified in `line-breaks.md` (a later link whose operator stands behind the closing `*/` of a multi-line comment does not start its line per SHARED-COMMENT-012, so the formatter breaks before its operator; example with the expected `.D()` column); `Uses` gains SHARED-COMMENT-012 as the maintainer asked (the ID exists in the ledger, `requirement-ids.txt:1330`). Wording "later link" matches SHARED-CHAIN-005. No ledger change needed.
- Matches the diff: `ChainLineBreakRewriter.EnsureLineBreak` / `IsBehindMultiLineCommentTail` break the line only behind a multi-line `/* */` tail; a one-line `/* x */` is unaffected (SHARED-COMMENT-011), as the spec's reading requires. `ChainLineBreakRewriter` is already a code owner. RH5201 analyzer behavior and `analyzer/RH5201.md` are unchanged per the maintainer; no rule page is affected.
- Change doc: none required (clarification plus bug fix; the maintainer decided SHARED-COMMENT-012 binds the formatter).
- Tests: `ChainLinkBehindMultiLineCommentTests` (new) tags `FMT-BREAK-019` and `SHARED-COMMENT-012`; the RH5201 formatter parity test tags both; the two RH5201 code-fix tests (LF, CRLF) tag `SHARED-COMMENT-012` and `SHARED-CHAIN-008`, as the table says. The spec's new example is tagged.
- Open questions: none.

## Checked
1. Change doc exactly when required: ok. No change doc in the diff; all five issues are bug fixes with clarifying requirements (judgment call on SHARED-BODY-004 under "For the lead").
2. Specs match the diff: ok. Every behavior changed in `src/` is stated (FMT-SWITCH-007/-014, FMT-BLANK-007/-013, FMT-STRUCT-004/-006/-025, FMT-BREAK-019/-021, SHARED-BODY-003/-004); no requirement contradicts the code; format of new requirements and examples ok (one heading each, normative sentence first, `Why:`, analyzer markup `{|RH####:…|}` in SHARED-BODY-004).
3. IDs: ok. FMT-STRUCT-025, FMT-SWITCH-014, SHARED-BODY-004 take the next free number and sit at their sorted positions in `requirement-ids.txt`; nothing reused, renumbered, or deleted.
4. Code owners exist: ok. `SpecConsistencyTests` passes (architecture 84/84); the diff adds, renames, and removes no type, so no owner list and no `sdd/specs/README.md` row needs a change.
5. User documentation: ok. RH3202 and RH3208 pages match SHARED-BODY-004 and `create-rule-doc`; `RuleDocumentation` tests 7/7; no other rule page is affected (RH5201, RH5020, RH5022, RH5031 behavior and corrections unchanged).
6. Open questions reach the maintainer: ok for the spec author's handover (none left; Escalation 1 and the skeptic escalation answered and recorded in `00-lead.md`, nothing written into a spec as an open question). The implementer's follow-up candidates are not yet confirmed to have reached the maintainer: see "For the lead".
7. Requirements covered, specs consistent: ok. `scripts/test.sh --project architecture`: `Passed!  - Failed:     0, Passed:    84, Skipped:     0, Total:    84`. Each new active requirement (FMT-SWITCH-014, FMT-STRUCT-025, SHARED-BODY-004) has covering `[Requirement]` tests in the diff.
8. New tests carry their requirement's tag: ok. The tags in the diff equal the `Tests` tables of `03-test-implementer.md` method by method; the only differences my script found are FMT-BLANK-002 and FMT-SWITCH-008, named in two descriptions in the first column, not in the requirement column, so no tag is owed. Every `Examples for the tests` requirement of the spec author is tagged on at least one new or newly tagged test.
9. Nothing left behind: ok. `sdd/work/` holds only `formatter-compile-idempotency/` (the lead deletes it before publishing); `git status` is clean before this handover.

## For the lead
Not findings against the spec, code, or tests; items that need the lead's or maintainer's attention before the PR is marked ready.

1. **SHARED-BODY-004 as bug fix or behavior change.** I treated it as a bug fix (the fix today yields CS1997; SHARED-FIX-003 and AGENTS.md "Prefer no fix over an unsafe one"; maintainer answered Q1 -> B in this run). If the lead or maintainer reads "RH3202/RH3208 now offer no fix for some members" as changed intended behavior, a change doc is required (`sdd/README.md`, "When a change doc is required"). A one-line confirmation in the lead's message to the maintainer closes this.
2. **Follow-up candidates from `04-implementer.md` that must reach the maintainer, not only the PR** (sdd/README, "Open questions": an observation is never recorded only in a PR description). None of them is in a spec and none changes behavior in this run: (a) `ReihitsuFormatterLineEndingTests.LineBreaksEndingPragmaAndRegionDirectivesAreRewritten` pins a blank line the formatter inserts directly in front of `#pragma warning restore` in front of `#endregion` and `}`, which FMT-BLANK-016 forbids (`TokenGapNormalizer` beyond the approved scope); (b) `DisabledRegionLayoutTests.RegionInsideActiveConditionalIsStillFormatted` pins no blank line between `#if DEBUG` and `#region` directly after `{`, while SHARED-BLANK-005 read literally asks for one (I did not run RH5031 on it); (c) start of file with a comment above `#region` keeps its exemption. Each needs the maintainer's decision or an approved follow-up issue.
3. **Smaller follow-ups already named by earlier roles**, for the run report: `RH5201.md` could list SHARED-COMMENT-012 under `Uses`; `RH3202…CodeFixProvider` duplicates the base's registration loop (R7); RH7501 fix output and FMT-PIPE-013 for the first statement moved into a braced section; the nested `Inner = { … }` initializer.

## Revision 1 (after repair round 1)

Commit: 99bb38d (diff `origin/main..99bb38d`; repair round since my first pass: `6555cd1..99bb38d`)
Verdict: consistent (no findings); one open question still has to reach the maintainer, see "For the lead"

Repair-round changes checked: spec author revisions 4–5 (`18b4688`, `ce7abdd`), test implementer revisions 3–4 (`1199b0c`, `91187ea`), implementer revision 2 (`780162b`, `BlankLineEditor`, `ExpressionBodyToBlockConverter`, `SwitchCaseBraceRewriter`), cleaner revision 1. My prompt carried no intended fix or verdict. I changed no file other than this handover.

- `scripts/test.sh --project architecture`: `Passed!  - Failed:     0, Passed:    84, Skipped:     0, Total:    84`. No rule page changed in the repair round, so I did not re-run the `RuleDocumentation` tests (7/7 at my first pass).

### Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None._ | | | | |

### Per issue
- **#948 (A2, A3).** FMT-SWITCH-014 now names the initializer of a local declaration (`var b = F(out var o);`) in the third bullet and query clauses next to lambda bodies, in the declaration sentence, the "MUST NOT trigger" list, and the `Why:`. This matches `IsBoundInsideSection` (a lambda, anonymous-method, or local-function parameter or an enclosing block's local binds the name) and `GetDesignatedNames` stopping at a query body, as `04-implementer.md` revision 2 describes. Tests: the lambda-parameter, local-function-parameter, and query-clause tests tag `FMT-SWITCH-014`; the two local-declaration-initializer tests tag `FMT-SWITCH-014`; tags equal the tables in revision 3.
- **#949 (A1).** FMT-SWITCH-007's text is unchanged; the `///` case is covered by its existing sentence plus FMT-BLANK-011 ("ordinary and documentation comments alike"). `BuildTerminalBreakLeadingTrivia` no longer appends a line break behind a comment that already ends with one. `DocumentationCommentBeforeTerminalBreakIsLaidOutInThePassThatInsertsTheBraces` tags FMT-SWITCH-007, FMT-BLANK-011, FMT-PIPE-004, as the table says.
- **#954 (A4, maintainer answer B).** Documentation comments only in `BlankLineEditor.cs`; no spec text changed and none is owed: the conditional-directive case is not specified and stays the follow-up the maintainer named. No code owner changed.
- **#1390 (A5, A6).** FMT-STRUCT-006 (rev 4): the own-line move applies unless the arrow's line holds a comment; then no own-line comment is moved and all comments keep source order; the placement of the arrow-line comment stays unspecified (thoenissen/Reihitsu#963), no example pinned. This matches `ArrowLineHoldsComment` in `ExpressionBodyToBlockConverter` (no comment moved, `CreateReturnStatement` drops whitespace-only trivia). Tests tag `FMT-STRUCT-006` (formatter) and `SHARED-BODY-003` (RH3202 `VerifyFixKeepsCommentOrderWhenTheArrowLineCarriesAComment`), as the table lists; the A6 rename `BlockCommentLeadingNextLineExpressionJoinsReturnKeyword` keeps its `FMT-STRUCT-006` tag and its assertion matches the spec (`return /* c */ 42;`).
- **#888 (A7, maintainer answer A).** SHARED-CHAIN-005 (`sdd/specs/shared/fluent-chain-layout.md`) gains the exception and a new example; its `Uses` gains SHARED-COMMENT-012 (exists in the ledger); FMT-BREAK-019's sentence covers any link behind the tail and points to the exception. Format ok (normative sentence, `Why:`, example with input and output). Code owners of the shared spec unchanged, and no `src/` file under Core or the analyzers changed in the repair round, so no owner update is owed. Change doc: none required. The spec author (rev 5) and the test implementer (rev 4) both record that the formatter and RH5201 already behaved this way on 2dea131 (the five new tests pass there), so the exception writes down existing behavior, which is a clarification, not a changed requirement. Tests: `PrefixAndFirstInvokedLinksBehindMultiLineCommentTailStartTheirOwnLines` (the spec example verbatim) tags SHARED-CHAIN-005 and FMT-BREAK-019; the prefix, first-invoked, chain-without-invoked-link, and RH5201 formatter-parity tests tag SHARED-CHAIN-005 and SHARED-COMMENT-012 (FMT-BREAK-019 on the formatter ones); the `!.` and `/** */` tests tag FMT-BREAK-019 and SHARED-COMMENT-012. Every example the spec author named for #888 (FMT-BREAK-019, SHARED-CHAIN-005) is tagged on a test the diff adds.

### Checked
1. Change doc exactly when required: ok. None in the diff; the repair round only clarifies (FMT-SWITCH-014, FMT-STRUCT-006) or writes down behavior that already exists (SHARED-CHAIN-005 exception).
2. Specs match the diff: ok. Every behavior change in the repair round (the FMT-SWITCH-014 narrowing, `///` before the terminal `break`, the arrow-line comment guard) is stated; no requirement contradicts the code.
3. IDs: ok. No new ID in the repair round; `requirement-ids.txt` unchanged since my first pass.
4. Code owners exist: ok. No type added, renamed, or removed in the repair round (only methods in three existing classes); `SpecConsistencyTests` pass.
5. User documentation: ok. No rule page changed in the repair round; RH5201's behavior is unchanged, so `documentation/rules/RH5201.md` needs nothing.
6. Open questions reach the maintainer: one still open, see "For the lead" item 1. The spec author's Q4 is in the handover; `00-lead.md` records no answer.
7. Requirements covered, specs consistent: ok. Architecture 84/84 (above).
8. New tests carry their requirement's tag: ok. I re-ran the comparison of the diff's `[Requirement]` tags against the tables in `03-test-implementer.md` (revisions 1–4): no mismatch for any test the diff adds or newly tags; the only unmatched names were table cells with an abbreviation (`RH3202…`) and existing tests I had verified at the first pass.
9. Nothing left behind: ok. `sdd/work/` holds only this run's directory; `git status` clean before this note.

### For the lead
1. **Q4 (spec author rev 5, #888, not blocking): the chain's first link behind a multi-line comment tail** (`var t = a /* x` / `*/ .C()` / `.D();`). The spec author recommends B (keep today's behavior, follow-up issue if wanted). `00-lead.md` has no answer. SHARED-CHAIN-005's exception says it "does not decide the chain's first link", so the spec stays correct either way, but the question must reach the maintainer (in the lead's message before the next stage or as a maintainer-approved follow-up issue), not only the PR. The same sentence leaves open whether a chain whose only breaks lie inside such a comment counts as wrapped (skeptic's #888 follow-up).
2. My first-pass items still stand: SHARED-BODY-004 as a bug fix rather than a changed behavior (confirm in the lead's message), and the follow-up candidates (FMT-BLANK-016 pragma/region gap; start-of-file comment above `#region`; `BuildLeadingTrivia` appending a line break behind `///` for the first moved statement; RH3202 provider duplication; `RH5201.md` `Uses`). The conditional-directive case under SHARED-BLANK-005 is now the maintainer's A4 follow-up.

## Revision 2 (after merge of origin/main, 7a040e2)

Commit: edeb12e (diff `origin/main..edeb12e`; delta since my revision 1: `3642dd8..edeb12e`)
Verdict: consistent (no findings); revision 1's item 1 (Q4) is closed

My prompt carried no intended fix or verdict. I changed no file other than this handover.

Delta checked: `origin/main` merged (7a040e2; brings thoenissen/Reihitsu#1462 and #1456), spec author revision 6 (`4253cc4`, `35a852c`), the maintainer's answer to Q4 in `00-lead.md`, the cleaner's revision 2. The run's own production files under `src/` (switch, chain, expression-body, blank-line, `TokenGapNormalizer`, initializer rewriters) did not change since revision 1. `origin/main` is the merge base (3973631), so `git diff origin/main` is the run's own diff.

- `scripts/test.sh --project architecture`: `Passed!  - Failed:     0, Passed:    84, Skipped:     0, Total:    84`.
- `scripts/test.sh --project analyzer --filter "FullyQualifiedName~RuleDocumentation"`: `Passed!  - Failed:     0, Passed:     7, Skipped:     0, Total:     7`.

### Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None._ | | | | |

### What changed against revision 1
- **Check 5 (user documentation), now with the amended rule.** `git diff origin/main -- documentation` is empty: the `## Exceptions` sections I accepted in revision 0 on `documentation/rules/RH3202.md` and `RH3208.md` are gone, and both pages equal `origin/main`. Under the amended check ("a page note about a withheld fix, an exception, or a corner case is a finding") that was the right move; no page of this run carries one now. The withheld fix is recorded where #1462 says: `sdd/specs/analyzer/RH3202.md` and `RH3208.md` state in their introductions that the fix is deliberately withheld for an `async` member whose return type is not written as a task type, that the member stays reported, and that the author converts it by hand, naming SHARED-BODY-004; both already list SHARED-BODY-004 under `Uses`. This agrees with SHARED-BODY-004 and with the code (provider guards unchanged). `general-notes.md` ("A diagnostic without a code fix") covers the user-facing side.
- **Check 6 (open questions).** Q4 (the chain's first link behind a multi-line comment tail) is answered B in `00-lead.md` ("Escalation 1 before the auditor"): today's behavior stays, unspecified in this run, follow-up candidate. SHARED-CHAIN-005's exception ("does not decide the chain's first link") already says exactly that, and the spec author's revision 6 marks it answered. No spec text change is owed. The same answer records that SHARED-BODY-004 is treated as part of the bug fix with no change doc, which closes my revision 1 item 2 (first part).
- **Checks 1-4, 7-9 re-run on the delta.** Change doc: none, none required. Ledger: `git diff origin/main -- sdd/specs/requirement-ids.txt` still adds only FMT-STRUCT-025, FMT-SWITCH-014, SHARED-BODY-004 at their sorted positions (the merge brought other runs' IDs, not touched). Code owners: no type added or renamed by this run; `SpecConsistencyTests` pass. Coverage and tags: the test files of this run are unchanged since revision 1, so the tag comparison against `03-test-implementer.md` stands (no mismatch). `sdd/work/` holds only this run's directory; `git status` clean before this note.

### For the lead
Still open from earlier passes, unchanged, for the run report / maintainer-approved follow-up issues: FMT-BLANK-016 pragma/region gap in `LineBreaksEndingPragmaAndRegionDirectivesAreRewritten`; the conditional directive between `{` and `#region` under SHARED-BLANK-005 (maintainer's A4 follow-up); a comment above `#region` at the start of a file; `BuildLeadingTrivia` appending a line break behind `///` for the first moved statement; the chain's first link behind a comment tail (Q4, B); RH3202 provider duplicating the base's registration loop; `RH5201.md` `Uses` could list SHARED-COMMENT-012. The `sdd/work/` directory of this run must be deleted by the lead before publishing.

## Revision 3 (after repair round 2)

Commit: 0a39eb7 (diff `origin/main..0a39eb7`; delta since revision 2: `2f5432b..0a39eb7`, which is `structural-transforms.md`, `SwitchCaseBraceRewriter.cs`, and `SwitchSectionSharedLocalTests.cs`)
Verdict: consistent (no findings); one untested sentence noted for the lead

My prompt carried no intended fix or verdict. I changed no file other than this handover.

- `scripts/test.sh --project architecture`: `Passed!  - Failed:     0, Passed:    84, Skipped:     0, Total:    84`. No rule page, ledger line, or code-owner list changed in this round.

### Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None._ | | | | |

### Delta checked
- **A9, FMT-STRUCT-006 (spec author revision 7, `54d2743`).** The condition "the arrow's line holds a comment" is now "a comment stands directly at the arrow": behind the arrow on its line before the next token (`=> // b`, `=> /* c */ 42`), or in front of it behind the last line break (`/* c */ => 42;`). A comment behind the expression or the semicolon (`=> 42 /* t */;`, `=> 42; // t`) explicitly does not stop the move. The `Why:` and the coverage sentence were updated to match. Against the code: `ArrowLineHoldsComment` looks at the arrow's trailing trivia and its leading trivia back to the last line break, which is the same set. I ran `scripts/format.sh` on a scratch file outside the repository: `// a` / `=> 42 /* t */;` gives `{` / `// a` / `return 42 /* t */;` / `}`; `// b` / `=> 42; // t` gives `{` / `// b` / `return 42;` / `} // t`; `// c` / `=> // d` / `42;` leaves `// c` in front of `{// d`. All three match the spec text. Format ok (one requirement, normative sentence first, `Why:`, examples unchanged). No ID, no owner, no change doc: the sentence narrows a clarification of behavior the code already has.
- **A8, A2 narrowed (FMT-SWITCH-014).** The requirement text is unchanged in this round. The repaired guard (`IsBoundByAncestor`: a binder counts only for names inside its C# scope; attribute lists, parameter lists, default values, return type, and constraint clauses of a lambda or local function stay uses) matches the existing sentences "a local ... used only in the section that declares it" and "a local declared inside a nested block, lambda, ... local-function body ... MUST NOT trigger this guard". The `Why:` already states block scoping to the whole switch and CS0103 on bracing. Conservative on unlisted binders (still unbraced, still compiles), as `04-implementer.md` revision 3 states. `Uses` and owners of `switch-case-braces.md` unchanged.
- **Tests (check 8).** The 13 new tests of `03-test-implementer.md` revision 5 (four A8, nine A2 narrowed, plus the passing nested-switch-section one) all carry `[Requirement("FMT-SWITCH-014")]`, and the four A8 tests also `FMT-PIPE-003`, as the revision 5 tables and prose state (my comparison script flagged them only because the revision 5 tables put the requirement in a differently placed column). The tag sets of all earlier tests are unchanged. The requirement the spec author names for these items (FMT-SWITCH-014) is tagged on tests the diff adds.
- **Checks 1-5, 7, 9.** Change doc: none, none required (no new behavior, only spec wording and a narrower guard for FMT-SWITCH-014). `git diff origin/main --stat` shows no new type; ledger unchanged; `documentation/` unchanged; `sdd/work/` holds only this run's directory; `git status` clean before this note.
- **Check 6.** No new question from the spec author in this round.

### For the lead
1. **Untested sentence (not a finding under checks 7 or 8, since FMT-STRUCT-006 is covered and tagged).** A9 added a normative sentence that a comment behind the expression or the semicolon (`=> 42 /* t */;`, `=> 42; // t`) does not stop the move of own-line comments. No test in the diff pins it: `ExpressionBodyArrowCommentTests` covers comments before and after the arrow and the three arrow-line cases from repair round 1, none of the trailing-position cases. I confirmed the behavior by hand (above), so it is a coverage gap, not a defect. A short test pair tagged FMT-STRUCT-006 would close it; the lead may route it to the test implementer or leave it for the run report.
2. All earlier follow-up candidates stand unchanged (see revision 2). The lead deletes this run's `sdd/work/` directory before publishing.
