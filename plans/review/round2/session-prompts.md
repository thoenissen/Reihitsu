# 1.0.0 RC Code Review (Round 2) — Session Prompts

The round-2 review is executed in batches across multiple sessions, one prompt per session, in order. Every session starts in a fresh context: each prompt below is self-contained and carries everything the session needs. Each session appends to `plans/review/round2/findings.md`, ticks `plans/review/round2/coverage.md`, and commits to the branch `review/rc-code-review`.

Prerequisite: this planning branch is merged to `main`, so `plans/review/round2/*` exists on `main` when session 1 creates the review branch.

Shared rules for every session (repeated inside each prompt where they matter):

- The review criteria are in `plans/review/round2/criteria.md`. The severity scale and finding format are defined there. Sections A and G apply to every session; the per-area section is named in each prompt.
- Findings only — no positive feedback. A file without findings gets no entry.
- The review is read-only: do not change production code. Only `plans/review/round2/*` may be modified.
- Read `findings.md` first so already-reported systemic issues are not duplicated; extend the affected-file list of an existing finding instead.
- Files marked `*` in `coverage.md` changed substantively after round 1 and their changes were never reviewed — read them fully. Unmarked files deriving from an already-reviewed base may be pattern-read (rule-specific overrides only).
- When a suspicion can only be verified in a later session's area, record it under "Open cross-checks" in `findings.md` instead of guessing.
- Round-1 reference material lives in `plans/review/round1/` (findings, criteria). Consult it when judging whether a round-1 fix is complete, but do not re-report round-1 findings that are fixed.

---

## Session 1 — Core and CLI (50 files)

```
Start the RC code review (round 2). Create the review branch from main: git fetch origin main && git switch -c review/rc-code-review origin/main (if the branch already exists, git switch review/rc-code-review and pull it first).

Scope: all unticked files ([ ]) in plans/review/round2/coverage.md under Reihitsu.Core and Reihitsu.Cli (50 files).

Procedure:
1. Inventory refresh: the coverage checklist was generated on 2026-07-02; code may have landed since. List all .cs files under Reihitsu.Core, Reihitsu.Cli, Reihitsu.Formatter, Reihitsu.Analyzer and Reihitsu.Analyzer.CodeFixes (excluding obj\ and bin\) and reconcile against coverage.md: append files that are missing as unticked entries marked *, delete entries whose files no longer exist, and correct the per-project counts. Note the refresh in the commit message.
2. Read plans/review/round2/criteria.md (sections A, E, F, G apply) and plans/review/round2/findings.md.
3. Read every file in scope fully (nearly all are *-marked). Round-2 emphases for this area:
   - Core grew seven new utility classes during the round-1 fix wave (DocumentationCommentUtilities, FormattingSafetyUtilities, LineEndingUtilities, RawStringLiteralUtilities, RegionDirectiveBlankLineUtilities, UnaryOperatorSpacingUtilities, XmlDocumentationElementOrderingUtilities). Check each for correctness, layering purity, and consolidation integrity: no stale private copy of the same logic may survive elsewhere (record suspected copies as open cross-checks for the formatter/analyzer sessions).
   - CasingUtilities: round 1 found crashes/empty results for letterless identifiers (fixed in #264). Verify the guards cover all conversion paths and exotic identifiers (unicode, digits, @-verbatim).
   - UsingDirectiveOrderingUtilities: round 1 found three divergent alias-ordering policies (consolidated in #269). Verify exactly one policy exists and note where it is consumed — the analyzer sessions must confirm RH7204/RH7207 agree with it (record as open cross-check).
   - Cli: encoding contract after #274 (skip non-UTF-8 with warning) and #305 (UTF-8 BOM enforcement) — check read/write round-trip, BOM preservation, and that --check/--dry-run stay strictly side-effect-free. Re-check exit-code contract (#272), LCS prefix/suffix trimming (#273) for off-by-one on empty/identical/fully-different inputs, and Ctrl+C/cancellation plumbing.
4. Append findings to plans/review/round2/findings.md under "## Reihitsu.Core" / "## Reihitsu.Cli" as "### Session 1". Add cross-checks under "## Open cross-checks". Tick all reviewed files in coverage.md.
5. Commit to review/rc-code-review with message "Add RC review findings: Core and CLI".

The review is read-only apart from plans/review/round2/*. No positive feedback — findings only.
```

## Session 2 — Formatter, part 1: infrastructure, StructuralTransforms, LineBreaks (49 files)

```
Continue the RC code review (round 2) on branch review/rc-code-review (git switch review/rc-code-review first, then pull).

Scope: all unticked files ([ ]) in plans/review/round2/coverage.md under Reihitsu.Formatter root, \Enumerations, \Properties, \Pipeline (top level), \Pipeline\StructuralTransforms, and \Pipeline\LineBreaks (49 files).

Procedure:
1. Read plans/review/round2/criteria.md (sections A, B, G apply) and plans/review/round2/findings.md, including any open cross-checks recorded by session 1.
2. Read every file in scope fully — this directory carried most round-1 Criticals and every file changed in the fix wave. Round-2 emphases:
   - Comment-join guard completeness: round 1's worst Critical was line joins swallowing end-of-line comments (fixed centrally in LineBreakTriviaUtilities, #271). Enumerate every join/collapse call site across ChainLineBreakRewriter, TernaryLineBreakRewriter, BinaryOperatorLineBreakRewriter, LineBreakAssignmentRewriter, LineBreakListRewriter, DeclarationBraceLineBreakRewriter, PropertyLayoutLineBreakRewriter and the new DeclarationSemicolonLineBreakRewriter / SwitchCaseWhenLineBreakRewriter, and verify each routes through the guard. One unguarded path re-opens the corruption class.
   - ExpressionBodyToBlockConverter / ExpressionBodiedTransformUtilities: verify the throw-statement conversion and Task/ValueTask detection (#263) cover all seven expression-bodied member kinds, async modifiers, and generic Task<T>/ValueTask<T>.
   - ControlFlowBraceTransform / brace add-remove paths: comments must survive (#284); check nested and else-if shapes.
   - Token re-resolution: #329/#362 hardened re-resolution against same-kind span collisions. Verify the hardening in BracePlacer/TokenLocator is correct when multiple identical tokens share a line and after multi-step edits.
   - Idempotency reasoning per rewriter (criterion G.6): record oscillation candidates between LineBreaks subphases explicitly.
3. Append findings under "## Reihitsu.Formatter" as "### Session 2 — infrastructure, StructuralTransforms, LineBreaks". Add cross-checks (e.g. "does a delegating code fix inherit this?") under "## Open cross-checks". Tick reviewed files in coverage.md.
4. Commit with message "Add RC review findings: Formatter infrastructure, StructuralTransforms, LineBreaks".

The review is read-only apart from plans/review/round2/*. No positive feedback — findings only.
```

## Session 3 — Formatter, part 2: remaining pipeline phases (59 files)

```
Continue the RC code review (round 2) on branch review/rc-code-review (git switch review/rc-code-review first, then pull).

Scope: all unticked files ([ ]) in plans/review/round2/coverage.md under Reihitsu.Formatter\Pipeline\BlankLines, \HorizontalSpacing, \Indentation (including \Contributors), \SwitchCaseBraces, \UsingDirectives, \RegionFormatting, \DocumentationComments, \RawStringAlignment, \Cleanup, and \LineEndings (59 files).

Procedure:
1. Read plans/review/round2/criteria.md (sections A, B, G apply) and plans/review/round2/findings.md, including open cross-checks from sessions 1–2.
2. Read every file in scope fully. Round-2 emphases:
   - Indentation contributors: much of this code is newer than round 1 — recursive patterns (#347), conditional expressions (#338), multi-line dictionary indexers (#339), case-when clauses (#342: CaseWhenClauseContributor, plus PatternAnchor/ParenthesizedPatternContributor/RecursivePatternContributor). Check contributor interplay: two contributors claiming the same token, and behavior on deeply nested pattern shapes.
   - HorizontalSpacing: the re-lex safety fix for unary signs (#267, UnaryOperatorSpacingUtilities in Core) — verify all operator adjacencies are covered (- -, + +, & &, < <, ...), not just the reported pair, and that the check is applied by every spacing rule that removes gaps.
   - SwitchCaseBraceRewriter: comment preservation on AddBraces/RemoveBraces (#284) at all four positions round 1 identified.
   - BlankLines: the new BlankLineRegionDirectiveRewriter vs. the RH5031/RH5032 analyzers — parity (record a cross-check for session 5); preprocessor-directive transparency (#356); oscillation between blank-line insertion and collapsing.
   - UsingDirectives: confirm the single consolidated ordering policy from Core is the only one in use (cross-check from session 1).
   - DocumentationComments: continuation-prefix derivation (#270) — significant indentation inside <code> must survive.
   - CRLF: idempotency of every phase in this scope under CRLF line endings (#328/#361 fixed instances; look for the pattern elsewhere).
3. Append findings under "## Reihitsu.Formatter" as "### Session 3 — remaining pipeline phases". Update/resolve cross-checks addressed here; add new ones. Tick reviewed files in coverage.md.
4. Commit with message "Add RC review findings: Formatter remaining phases".

The review is read-only apart from plans/review/round2/*. No positive feedback — findings only.
```

## Session 4 — Analyzer and CodeFixes infrastructure (62 files)

```
Continue the RC code review (round 2) on branch review/rc-code-review (git switch review/rc-code-review first, then pull).

Scope: all unticked files ([ ]) in plans/review/round2/coverage.md under Reihitsu.Analyzer outside \Rules (Base, Core, Properties, root — 41 files) and Reihitsu.Analyzer.CodeFixes outside \Rules (Base, Core, Properties, root — 21 files).

Procedure:
1. Read plans/review/round2/criteria.md (sections A, C, D, G apply) and plans/review/round2/findings.md, including open cross-checks from sessions 1–3.
2. Read every file in scope fully — these bases are load-bearing for ~500 derived rule/fix files. Round-2 emphases:
   - DiagnosticAnalyzerBase: helpLinkUri contract, multi-location diagnostics (#278 fixed the primary-location bug), EnableConcurrentExecution/generated-code configuration inherited by every rule, statelessness.
   - The five new code-fix bases (CollapseTokenGapCodeFixProviderBase, CommentSafeSpanReplacementCodeFixProviderBase, RegionDirectiveBlankLineCodeFixProviderBase, RemoveWhitespaceRunCodeFixProviderBase, TrailingCommaRemovalCodeFixProviderBase): these encode the round-1 safety contracts (comment preservation #291, token-gap collapse safety). Verify the contract is actually enforced by the base, not merely available; check FixAll behavior and equivalence keys of each base.
   - CasingCodeFixProviderBase: guards for letterless/empty conversion results (#264) on every code path including Renamer-based renames.
   - StatementBracesCodeFixProviderBase: the header-deletion fix (#283) against same-line children and else-if chains.
   - EOL handling: fixes must use the document's detected line ending, not Environment.NewLine (#298) — verify the central mechanism and note bypasses as findings.
   - SpeculativeRebindingHelper and InterfaceImplementationUtilities (new since round 1): correctness and whether they belong in Core (layering).
   - ModifierOrderingCodeFixProviderBase.NormalizeModifierTrivia: round 1 recorded that it deletes comments between modifiers — verify whether that was fixed; if not, it is a real finding for this round.
3. Append findings under "## Reihitsu.Analyzer" / "## Reihitsu.Analyzer.CodeFixes" as "### Session 4 — infrastructure". Update cross-checks. Tick reviewed files in coverage.md.
4. Commit with message "Add RC review findings: analyzer and code-fix infrastructure".

The review is read-only apart from plans/review/round2/*. No positive feedback — findings only.
```

## Session 5 — Analyzer rules: Layout (109 files)

```
Continue the RC code review (round 2) on branch review/rc-code-review (git switch review/rc-code-review first, then pull).

Scope: all unticked files ([ ]) in plans/review/round2/coverage.md under Reihitsu.Analyzer\Rules\Layout (109 files).

Procedure:
1. Read plans/review/round2/criteria.md (sections A, C, G apply) and plans/review/round2/findings.md, especially open cross-checks pointing at Layout rules and the session-4 verdicts on the shared bases.
2. Read every *-marked file fully. Unmarked files deriving from a base reviewed in session 4 may be pattern-read (rule-specific overrides only). Round-2 emphases:
   - Analyzer/formatter policy parity (criterion G.4) is the core job of this session: for each rule, name the formatter phase that owns the same concern and check both directions — formatter output must not be flagged, analyzer-clean code must be formatter-stable. Round 1 found five divergent pairs (RH5109/RH5111/RH5206/RH5401/RH5408, aligned in #289); verify the alignment held and check the pairs nobody looked at.
   - New rules since round 1: RH5031/RH5032 (region blank lines, #345 — parity with BlankLineRegionDirectiveRewriter, resolve the session-3 cross-check), RH5113 (declaration semicolon placement, #343 — parity with DeclarationSemicolonLineBreakRewriter), and the case-when layout rules (#342 — parity with SwitchCaseWhenLineBreakRewriter).
   - Blank-line rules: preprocessor-directive transparency (#356) and statement-kind coverage (#287) — check the round-1 false-negative list is actually closed.
   - Text-line-scan rules (RH5022–RH5026 family): comment interiors, disabled #if blocks, and string interiors must not be flagged (#288); raw strings and interpolated raw strings are the hard case (#231/#266).
   - Diagnostic location precision and GetText()/ToString() allocations per node.
3. Append findings under "## Reihitsu.Analyzer" as "### Session 5 — Layout rules". Update cross-checks; add code-fix-side suspicions for sessions 8–9. Tick reviewed files in coverage.md.
4. Commit with message "Add RC review findings: Analyzer Layout rules".

The review is read-only apart from plans/review/round2/*. No positive feedback — findings only.
```

## Session 6 — Analyzer rules: Documentation and Organization (102 files)

```
Continue the RC code review (round 2) on branch review/rc-code-review (git switch review/rc-code-review first, then pull).

Scope: all unticked files ([ ]) in plans/review/round2/coverage.md under Reihitsu.Analyzer\Rules\Documentation (60 files) and Reihitsu.Analyzer\Rules\Organization (42 files).

Procedure:
1. Read plans/review/round2/criteria.md (sections A, C, G apply) and plans/review/round2/findings.md, including open cross-checks.
2. Read every *-marked file fully; pattern-read unmarked base-derived rules. Round-2 emphases:
   - Documentation: IsPurePrivateDeclaration classification (#285) across all accessibility shapes; DocumentationMode guards; RH8306 was rescoped twice (#357, then renamed/rescoped to noun-phrase property summaries in #370) — check the final behavior matches its rule doc and resources; RH8309 element ordering (#346) shares XmlDocumentationElementOrderingUtilities with the code fix after #364 — verify analyzer and fix consume the identical policy.
   - Organization: RH7103 per-region scoping (#355); member-reordering safety contracts (#294 — preprocessor directives, field-initializer order) as consumed by these analyzers' fixes (record cross-checks for session 9); the new interface-region rules RH7409–RH7412 (#348) and RH7308 (#344) — false positives on explicit implementations, static interface members, partial types; using-ordering rules RH7204/RH7207 must agree with the single Core policy (resolve the session-1 cross-check).
   - Resources and rule docs for every rule added or rescoped since round 1.
3. Append findings under "## Reihitsu.Analyzer" as "### Session 6 — Documentation and Organization rules". Update cross-checks. Tick reviewed files in coverage.md.
4. Commit with message "Add RC review findings: Analyzer Documentation and Organization rules".

The review is read-only apart from plans/review/round2/*. No positive feedback — findings only.
```

## Session 7 — Analyzer rules: Clarity, Design, Naming, Spacing, Analyzer, Performance (93 files)

```
Continue the RC code review (round 2) on branch review/rc-code-review (git switch review/rc-code-review first, then pull).

Scope: all unticked files ([ ]) in plans/review/round2/coverage.md under Reihitsu.Analyzer\Rules\Clarity (18), \Design (16), \Naming (32), \Spacing (22), \Analyzer (2), and \Performance (3) — 93 files.

Procedure:
1. Read plans/review/round2/criteria.md (sections A, C, G apply) and plans/review/round2/findings.md, including open cross-checks.
2. Read every *-marked file fully; pattern-read unmarked base-derived rules. Round-2 emphases:
   - Naming: the RH4107/RH4108 compound-accessibility partition (#286, generalized across property rules in #333) — verify no member shape is claimed by two casing rules or by none; RH4005 interface-prefix handling (#295).
   - Clarity: RH3204 interpolation rules after #265/#337 — escaped braces, arguments, raw interpolated strings; RH3202/RH3203 relationship to the formatter's expression-body transform (parity, criterion G.4).
   - Design: RH2001 (#296) and RH2004/RH2005 (#290) — analyzer-side shapes that drove those fix bugs.
   - Spacing: RH6002 unbound-generic commas (#325); every spacing rule that demands zero gap must be covered by the re-lex safety check (cross-check with session 3's HorizontalSpacing verdict).
   - If any rule in this scope was added for the 1.0 milestone after this plan was written (check coverage.md entries you did not expect), review it with extra care: newest code, never reviewed.
3. Append findings under "## Reihitsu.Analyzer" as "### Session 7 — remaining rule categories". Update cross-checks. Tick reviewed files in coverage.md.
4. Commit with message "Add RC review findings: Analyzer remaining rule categories".

The review is read-only apart from plans/review/round2/*. No positive feedback — findings only.
```

## Session 8 — CodeFixes rules: Layout (109 files)

```
Continue the RC code review (round 2) on branch review/rc-code-review (git switch review/rc-code-review first, then pull).

Scope: all unticked files ([ ]) in plans/review/round2/coverage.md under Reihitsu.Analyzer.CodeFixes\Rules\Layout (109 files).

Procedure:
1. Read plans/review/round2/criteria.md (sections A, D, G apply) and plans/review/round2/findings.md — especially session 2's verdict on the formatter comment-join guards and session 5's parity findings, since most Layout fixes delegate to FormatNodeInDocumentAsync and inherit whatever the formatter does.
2. Read every *-marked file fully; pattern-read unmarked base-derived fixes. Round-2 emphases:
   - Fix convergence (criterion G.3): for each fix, applying it must silence its diagnostic in one pass. Round 1 found manual re-implementations in RH5101/RH5102/RH5111 that were replaced by delegation (#271) — verify no manual line-join surgery survives anywhere in this directory.
   - Delegation scope: when a fix formats an enclosing scope, the formatter may legally change more than the flagged construct — check the fix scopes the formatting call tightly enough that unrelated edits (and unrelated diagnostics) don't appear.
   - RH5405/RH5406/RH5407 header preservation (#283) via the shared base; RH5031/RH5032 fixes through RegionDirectiveBlankLineCodeFixProviderBase; RH5113 and the case-when fixes as the newest members.
   - FixAll on files with many instances: overlapping spans, stale positions after the first application.
3. Append findings under "## Reihitsu.Analyzer.CodeFixes" as "### Session 8 — Layout fixes". Update cross-checks. Tick reviewed files in coverage.md.
4. Commit with message "Add RC review findings: CodeFixes Layout rules".

The review is read-only apart from plans/review/round2/*. No positive feedback — findings only.
```

## Session 9 — CodeFixes rules: Clarity, Design, Documentation, Naming, Organization, Spacing (104 files)

```
Continue the RC code review (round 2) on branch review/rc-code-review (git switch review/rc-code-review first, then pull).

Scope: all unticked files ([ ]) in plans/review/round2/coverage.md under Reihitsu.Analyzer.CodeFixes\Rules\Clarity (17), \Design (3), \Documentation (14), \Naming (25), \Organization (23), and \Spacing (22) — 104 files.

Procedure:
1. Read plans/review/round2/criteria.md (sections A, D, G apply) and plans/review/round2/findings.md, including all remaining open cross-checks — this is the last review session, so resolve every cross-check that points into this scope.
2. Read every *-marked file fully; pattern-read unmarked base-derived fixes. Round-2 emphases:
   - Semantics preservation: the Clarity fixes repaired in #292 (RH3001, RH3003, RH3104) — verify the repaired shapes and hunt sibling shapes with the same hazard; RH3204 fix after #265/#337.
   - Naming fixes: guards from #264 via CasingCodeFixProviderBase on every path; Renamer-based renames on partial types, explicit interface implementations, and shadowed locals.
   - Organization fixes: RH7004 statement-wrapping (#293) for local functions, labels, and gotos; member reordering (#294) — preprocessor directives and field-initializer dependency order; RH7204/RH7207 fixes against the consolidated Core policy (fix convergence, criterion G.3).
   - Design/Documentation fixes: RH2004/RH2005 doc-comment ownership (#290); RH8309 fix consuming the shared ordering utilities (#364) — convergence with its analyzer.
   - No-op and crash paths (#297): every registered code action must either change the document or not be offered.
3. Append findings under "## Reihitsu.Analyzer.CodeFixes" as "### Session 9 — remaining fix categories". Resolve cross-checks; anything unresolvable is handed to the closeout session explicitly. Tick reviewed files in coverage.md.
4. Commit with message "Add RC review findings: CodeFixes remaining rule categories".

The review is read-only apart from plans/review/round2/*. No positive feedback — findings only.
```

## Session 10 — Closeout

```
Close out the RC code review (round 2) on branch review/rc-code-review (git switch review/rc-code-review first, then pull).

Procedure:
1. Verify completeness: searching plans/review/round2/coverage.md for the pattern "- [ ]" must return zero matches (PowerShell: Select-String -Path plans/review/round2/coverage.md -Pattern '- \[ \]' -SimpleMatch). If files remain, review them now against plans/review/round2/criteria.md before continuing.
2. Final pass over plans/review/round2/findings.md:
   - Resolve or explicitly close every entry under "Open cross-checks".
   - Deduplicate findings; merge entries that describe the same root cause across assemblies into a "Cross-assembly findings (merged)" section, keeping only single-assembly findings in the per-assembly sections.
   - Confirm every finding has a severity tag and a path:line reference.
   - Fill in the "Summary" section at the top: counts per severity and per assembly, plus the recommended fix order (Criticals first).
   - Add a one-paragraph comparison with round 1 (plans/review/round1/findings.md): which round-1 finding classes stayed closed, which recurred, what is genuinely new.
3. Verify git status shows no production-code changes (only plans/review/round2/*).
4. Commit with message "Finalize RC code review report".

Afterwards, on request: derive issue drafts in plans/issues/ per finding cluster (separate task, /draft-issue).
```
