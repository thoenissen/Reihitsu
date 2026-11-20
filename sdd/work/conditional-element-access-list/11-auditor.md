Role: auditor
Commit: 82d818ae6ce4bd7ab01f45ea09e307ae2973363b
Verdict: pass, text cleanup

My prompt carried no intended fix, suspected cause, or hoped-for verdict. I changed nothing but this handover.

Finding #1 is confined to spec text (`sdd/specs/formatter/pipeline.md`) and the change doc's `Affected requirements` row: it cannot reach compiled behavior, a diagnostic, or rule metadata — the code already does what the corrected text would say, and the run's tests already assert it. Because it rewords a normative requirement (FMT-PIPE-024), the spec checker should confirm the repair.

## Whole run

### Scope

- Head `82d818ae` (checked out; `git log -1`), a merge of `origin/main` `423c1a44ef507feac057c05aaa5b465e7b75c84f` (`git rev-parse origin/main` gives that SHA; `git merge-base --is-ancestor` confirms it is merged). Full audit of `git diff 423c1a44 HEAD`.
- The merge brought only `Release.ruleset` (RH5113, RH5308, RH5417, RH6023, RH6024, RH7110, RH7308–RH7311, RH7409–RH7412 as errors), `RulesetTests.cs`, and a change doc, all after the cleaner's build. `scripts/build.sh` (Release) on the head: `Build succeeded. 0 Warning(s), 0 Error(s)`.
- The diff is what the handovers describe: production 5 files (`SyntaxNodeUtilities.IsIndexArgumentList` new; `LineBreakListRewriter.IsElementAccessArgumentList` delegates to it; `RootConstruct` summary; RH5307 analyzer drops `IsIndexerArgumentList` and asks the shared predicate; RH5307 code fix asks it), specs FMT-BREAK-014 and RH5307, the rule page, the change doc, the tests of `04` and `06`, and `sdd/work/`. Nothing outside the run's scope (check 9).

### Evidence I gathered (fixtures in the scratchpad, outside the repository)

- `scripts/format.sh`, two passes, LF and CRLF: `o⏎.B()?[1,⏎2]⏎.ToString()`, `o?.B().C()?[1,⏎2]`, `c?[1,⏎2]!`, `o?[() => 1,⏎2]`, a lambda argument that spans lines (kept wrapped), `c?[…]` in a conditional branch and a switch arm, `ds?[0]?[1,⏎2]`, `e?[0]?[1,⏎2]`, `c?[1,⏎2] + c?[3,⏎4]`, `o?.Q?.Q?[1,⏎2]`, `c?[ 1 ,⏎2 ]`, and tab-indented `c?[1,⏎→→→2]`, `c?[→1,⏎…2] ?? 0`. Every list joins in pass 1, pass 2 changes nothing (`Formatted 0`), the CRLF copy equals the LF output. The chain case gives `o.B()?[1, 2]⏎                  .ToString();`, identical to the element-access analog `o⏎.B()[1,⏎2]⏎.ToString()` on the same head.
- `scripts/apply-fix.sh RH5307`, LF and CRLF: the same chain case, `c?[x?[1,⏎2] ?? 0, 3]`, `[c?[1,⏎2], 4]`, `$"{c?[1,⏎2]}"`, a switch arm, `c?[1,⏎2]!`, `c?[c?[0,⏎1] ?? 0,⏎2]` (nested; inner reported first, then the outer), tabbed input, and `c?⏎#pragma …⏎[1,⏎2]` plus a `#if`/`#else` pair of `?[…]` lists: all `fixed`, 0 not-converged, 0 no-progress, 0 line-ending-drift; the disabled branch stays as written, the directive stays on its line.
- `scripts/apply-fix.sh RH5115` on `o.B().C()?⏎[1,⏎2].ToString()` and `o.B().C()⏎?[1,⏎2]` (chains with links, so the fix formats the chain): `o.B().C()?[1, 2].ToString();` and `o.B().C()?[1, 2];`, converged. This RH5115 result changes with the run; it is visible output change 2 of the change doc ("every code fix that formats a node through the formatter"), covered by answer 1A.

## thoenissen/Reihitsu#1877

### Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | medium | `sdd/specs/formatter/pipeline.md:1436` (FMT-PIPE-024 fact table, row *Element-access arguments*) and `:1445` (the line-break bullet: "may collapse the arguments of an element access onto one line"); `sdd/changes/2026-11/conditional-element-access-list.md` (`Affected requirements`) | FC-015 (one rule stated in more than one place), FC-031 | – | PR-introduced | FMT-PIPE-024 says the node-level entry points give "the facts in the table below … and no other", and defines the fact as "whether the root is the bracketed argument list of an element access or of an implicit element access". Through the shared predicate, `ReihitsuFormatterHelpers.CaptureRootConstruct` now sets `RootConstruct.ElementAccessArguments` also when the target is the list of an element binding (`a?[⇥1,⏎2]` as target of `FormatNodeInDocumentAsync` → `a?[1, 2]`), and the run's test `ReihitsuFormatterDetachedRootTests.ConditionalElementAccessArgumentListTargetGetsItsArgumentsCollapsed` is tagged FMT-PIPE-024 while asserting exactly that. So the requirement's text excludes what its covering test asserts, and the change doc does not list FMT-PIPE-024 as affected. It is also a third statement of "which bracketed lists hold an index" in the specs, next to FMT-BREAK-014 — the one the run's decision 2 made the single definition — and it drifted with this very change. The behavior is right (FMT-BREAK-014 and RH5307-004 need it for the RH5307 fix); only the text is stale | Spec author: let the row refer to FMT-BREAK-014 ("whether the root is a bracketed argument list that holds an index, FMT-BREAK-014"), word line 1445 the same way ("may join the root's index list"), and add FMT-PIPE-024 (clarified, no behavior change of its own) to the change doc's `Affected requirements` |

### Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Which bracketed list holds an index (join eligibility) | `BracketedArgumentListSyntax.Parent` kind only (`SyntaxNodeUtilities.IsIndexArgumentList`, `SyntaxNodeUtilities.cs:192`) | RH5307 analyzer `OnBracketedArgumentList`, RH5307 fix `TryGetFormatTarget` — the same predicate; FMT-BREAK-014's three parent kinds | identical by construction; `VariableDeclaratorSyntax` (fixed buffer) false on every surface (Core test, formatter characterization test) | holds |
| Node-level root fact `ElementAccessArguments` | the target's parent, read once in `CaptureRootConstruct` through `IsElementAccessArgumentList(list, None)` → shared predicate | `LineBreakListRewriter.IsElementAccessArgumentList` for a non-root list | same predicate on the same node, once with parent, once by fact | holds (code); spec text: finding #1 |
| Join safety | `CanSafelyCollapseInteriorList`: `node.Span` comments/directives (`InteriorContainsCommentOrDirective`), elements multi-line both as written and after attribute formatting | RH5307 `CanSafelyCollapseToSingleLine`: same span check, every argument single-line as written | analyzer stricter (P11); whenever RH5307 reports, the formatter joins; unchanged by the run and identical for element access and element binding | holds |
| Gap ownership around `?[` | FMT-BREAK-014: `[`..`]`; `AttachedPartGapUtilities.IsElementBindingOpenBracket` (FMT-BREAK-040, RH5115) the gaps in front of `?` and between `?` and `[` | RH5307 location = list span; RH5307 judges only `node.Span` | disjoint spans; `c?⏎[1,⏎2]` and `c⏎?[1,⏎2]` tests and RH5115/RH5307 either-order convergence pass | holds |
| Unsupported check of the fix | `FindUnsupportedSyntax(list)` — the node `FormatNodeInDocumentAsync` receives | SHARED-FIX-010, FMT-UNSUP-003/014 | same node (FC-043); a comment before `?` or between `?` and `[` is outside the list or holds only part of the shape, so the fix is offered as for `d /* c */ [1,⏎2]` on `main` | holds |

### Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Which bracketed lists hold an index | `rg` over `src` for the three parent kinds and the three predicate names: `SyntaxNodeUtilities.IsIndexArgumentList` (owner); `LineBreakListRewriter.IsElementAccessArgumentList` (wrapper adding the root fact); RH5307 analyzer and fix (callers); `IsIndexerArgumentList` gone. Specs: FMT-BREAK-014 (definition), RH5307-001 (refers to it), FMT-PIPE-024 row (restates the old set — finding #1) | one code owner; one stale spec statement |
| Which `[` starts an attached part | `AttachedPartGapUtilities` (`:55` element access, `:73` element binding), RH5115 | different decision (the gap in front), not a copy |
| Diagnostic location | RH5307 `Parent is ElementAccessExpressionSyntax ? access : list`; RH5307-003's own decision, not eligibility | ok |
| Alignment of a wrapped bracketed list | `ArgumentAlignmentContributor:116/122` (all bracketed lists); unchanged, applies to every parent alike | ok |

### Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `ConditionalElementAccessArgumentListTests` (16 join rows) | exact one-pass output of FMT-BREAK-014 for `?[` lists in each context | list left wrapped, other layout changed, second pass or a phase alone changes the output, CRLF differs | `AssertRuleResult` (exact text, LF+CRLF, second pass, per-phase stability) | adequate |
| …`BlockComment…`/`Directive…KeepsConditionalElementAccessListAsWritten` | FMT-BREAK-001: no join across a comment/directive | any change of the input | `AssertRuleResult(input)` | adequate |
| …`MultiLineArgumentKeeps…Wrapped` | an argument that spans lines keeps the list wrapped | list joined | `AssertRuleResult` | adequate |
| …`CommentInFrontOfQuestionMark…IsUnsupported` | FMT-UNSUP-014 still refuses with a wrapped binding list | output changed, no/other reason, wrong location | `AssertUnsupportedFor` | adequate |
| …`FixedSizeBufferSizeListStaysWrapped` | characterization of `main` (decision 3) | the run's predicate admitting `VariableDeclaratorSyntax` | `AssertRuleResult` | adequate (untagged by design) |
| `ListElementAttributeLayoutSinglePassTests.ConditionalElementAccessJoinsWrappedArgumentsWhenNestedIndexerCollapses` | `a?[b[1,⏎2],⏎3]` → `a?[b[1, 2], 3]` in one pass (answer 1A) | outer list left wrapped | `AssertRuleResult` | adequate |
| `ReihitsuFormatterDetachedRootTests.ConditionalElementAccessArgumentListTargetGetsItsArgumentsCollapsed` | binding list as `FormatNodeInDocumentAsync` target is joined, tab in target | list left wrapped or column moved | `AssertFormatsTarget` | adequate |
| RH5101/RH5111/RH5302/RH5308 `VerifyFixJoinsConditionalElementAccessList…` | a fix that formats a node holding the list joins it (answer 1A) | list left wrapped | `VerifyCodeFixWithEndOfLine` (verifier + single application, LF+CRLF) | adequate |
| `RH5307…FormatterTests` parity pair | analyzer reports what the formatter joins and not what it keeps (FC-013) | diagnostic mismatch or formatter output differs | `VerifyFormatter` | adequate |
| `SyntaxNodeUtilitiesTests.IsIndexArgumentList…` | three parent kinds true (incl. `d![`), fixed buffer false | any parent kind dropped or added | direct call | adequate |

### Failure classes dismissed

- FC-001, FC-002, FC-003 — the predicate is a parent-kind test with no qualifier; both sides of every guard the run relies on are tested (comment inside vs. behind `]` vs. in front of `?`; all-single-line vs. multi-line argument).
- FC-004, FC-005, FC-019, FC-020, FC-021 — the join rewrites only whitespace between `[` and `]`, behind the unchanged `InteriorContainsCommentOrDirective` guard; `//`, `/* */`, `#if/#else/#endif` between brackets, a comment behind `]`, and a `#pragma`/`#if` between `?` and `[` traced or run (kept, refused by FMT-UNSUP-014, or fixed with the directive in place).
- FC-006, FC-022–FC-026, FC-029, FC-036 — whitespace-only rewrite; no literal, identifier, member-kind, rename, or key/value layout involved; two identical `c?[…]` on one line (`b2`) both join.
- FC-010, FC-011, FC-012 — two passes and CRLF on a 13-shape corpus are stable; no loop added; FMT-BREAK-040 and FMT-BREAK-014 own disjoint gaps and their tests run in one pipeline.
- FC-013, FC-014 — the analyzer reports exactly the lists the formatter joins with the stricter all-single-line guard (pre-existing, P11); no new rule.
- FC-015, FC-016 — code: one owner; the fourth parent (fixed buffer) is excluded on every surface and pinned. Specs: finding #1.
- FC-017 — see *Assertion adequacy*.
- FC-027 — enclosing contexts traced: declaration value, invocation argument, initializer index, lambda body, interpolation hole, receiver of element access and invocation, `.Member` behind, chain with links, conditional branch, switch arm, collection-expression element.
- FC-028 — `FormatNode` on a list inside a tree is dropped as unreachable (decision 5); `FormatNodeInDocumentAsync` is covered.
- FC-031 — every changed summary describes the new code (`IsElementAccessArgumentList`, `RootConstruct.ElementAccessArguments`, `CanSafelyCollapseBracketedArguments` remarks unchanged and still true); spec text: finding #1.
- FC-032 — the formatter, analyzer, Core, and architecture suites were run on the compiled change (`05`, `06`); the merged ruleset change was checked by my Release build.
- FC-033, FC-034, FC-035 — the reproductions are the issue's own `c?[1,⏎2]`, under LF and CRLF; expected values come from the spec examples and the element-access analog on unchanged code.
- FC-037 — no text-only claim.
- FC-038 — the join carries only whitespace and needs a gap free of comments, so it cannot write an FMT-UNSUP shape.
- FC-039, FC-040, FC-042, FC-044 — no shape declared or changed; no guard modelling a computed result; the comment guard is mode-independent and shared with element accesses.
- FC-041, FC-043 — the fix asks `FindUnsupportedSyntax` about the list it formats; see *Guard scope*.

### Architecture findings

One rule stated in more than one place (README, *Architecture findings*, third bullet): which bracketed argument lists hold an index is defined in FMT-BREAK-014 and restated in FMT-PIPE-024's *Element-access arguments* fact (`pipeline.md:1436`, `:1445`). The run moved the code to one owner and made RH5307-001 refer to FMT-BREAK-014 (decision 2), but FMT-PIPE-024 kept the old two-kind list and now contradicts the code and its own tagged test (finding #1). Text-only: refer to FMT-BREAK-014 instead of restating it.

### Hints

- `LineBreakListRewriter.IsElementAccessArgumentList` and `RootConstruct.ElementAccessArguments` now also mean an element binding's list; their summaries say so, but the names are narrower than the meaning. Not a finding; a rename would ride along with finding #1's wording if the lead wants it.
- `sdd/specs/formatter/indentation.md:544` lists "element-access arguments" among the aligned lists, while `ArgumentAlignmentContributor:122` aligns every bracketed argument list (also an element binding's, also a fixed buffer's). Pre-existing loose wording, no wrong output.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1878

### Findings

_None._ Every requirement of the issue is addressed: `c?[1,⏎2]` is reported and fixed like `c[1,⏎2]` (same conditions, location per answer 2A); `d![1,⏎2]` is stated in RH5307-001 and tested (`VerifyFixJoinsElementAccessListBehindNullForgivingOperator`); the exclusion is removed from RH5307-001 and the rule page. The issue's proposed location (the whole conditional access) was replaced by answer 2A, as the change doc records.

### Guard scope

See #1877: the RH5307 analyzer and fix use the shared predicate; the location (`RH5307…Analyzer.cs:79`) gives an element binding's list the list span, and `TryGetFormatTarget`'s `BracketedArgumentListSyntax list when list.Span == span` arm resolves it, because `FindNode(…, getInnermostNodeForTie: true)` returns the list rather than the element binding of the same span. For `e?[0,⏎1][1,⏎2]` the two diagnostics have distinct spans (`[0,⏎1]` vs `[0,⏎1][1,⏎2]`) and map to distinct lists — `VerifyEachFixJoinsItsOwnListWhenTheReceiverIsAnElementBinding` pins both orders.

### Policy ownership

See #1877.

### Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `VerifyConditionalAndNullForgivingElementAccessListsAreReportedAlike` | spec example of RH5307-001/-003: five lists reported, each on its exact span | a missing, extra, or misplaced diagnostic | `Verify(source)` (exact markup) | adequate |
| `…WithCommentBetweenBrackets…`, `…WithDirectiveBetweenBrackets…`, `…TheFormatterKeeps…IsNotReported` | RH5307-002 / one-line list not reported | any diagnostic | `Verify(source)` | adequate |
| `VerifyFixJoinsConditionalElementAccessListsAndKeepsTheCodeAround`, `VerifyFixesJoinTheListsOfTwoConditionalElementAccesses` | RH5307-004 spec example: every diagnostic located, iterations and Fix All reach the text (decision 6) | wrong final text, Fix All or iteration mismatch | `Verify(source, fixed)` LF+CRLF | adequate; single-application behavior is pinned separately below |
| `VerifyFixJoins…` single-diagnostic rows (`c?[`, `c?[⏎`, `c!?[`, `d![`, `o?.Q?[`, `.ToString()`, comment before/behind `?`, comment line before `[`) | the fix joins only the list, code around as written | any other change or no fix | `VerifyCodeFixWithEndOfLine` | adequate |
| `VerifyEachFixJoinsItsOwnListWhenTheReceiverIsAnElementBinding` | each fix changes only its own list; either order converges | a fix joining the other list | `TryApplyCodeFixAtAsync` + `ApplyCodeFixAsync`, exact text | adequate |
| `VerifyFixAndIndexAccessFixConvergeInEitherOrder` | RH5307 and RH5115 fixes converge in either order | divergence or a leftover diagnostic | `Verify` + RH5115 verifier; RH5115 on `?` per unchanged RH5115 (decision 7) | adequate |

### Failure classes dismissed

As under #1877; specific to the rule:

- FC-007 — every fix in my sweep converged; the fix's node-level join depends on the root fact, which the shared predicate now captures.
- FC-008 — overlapping spans with the same start (`e?{|[0,⏎1]|}` inside `{|[0,⏎1][1,⏎2]|}`) and two bindings in a row are run through Fix All (`DocumentOrderFixAllProvider`).
- FC-009 — the analyzer's guard is stricter than the formatter's, and the root fact is set, so an offered fix always changes the list.
- FC-018 — no rename.
- FC-030 — the fix formats only the list; the tests assert the `?`, receiver, `?? 0`, `.ToString()`, and a trailing comment stay as written.

### Architecture findings

_None_ beyond the one under #1877 (the RH5307 side of the duplicate was resolved by decision 1 and decision 2).

### Hints

- `SHARED-DIAG-010` ends with "the rule reports it … and its fix is withheld (SHARED-FIX-010)" for a shape the formatter refuses. Read alone it suggests the RH5307 fix is withheld for `c /* c */ ?[1,⏎2]` and `c?⏎// c⏎[1,⏎2]`, whose documents whole-document formatting refuses (FMT-UNSUP-014); SHARED-FIX-010, which it cites, decides on the node the fix formats (the list, which does not hold the shape), so the run's tests — fix offered — follow the precise requirement, exactly as `main` does for `d /* c */ [1,⏎2]`. Pre-existing wording, no output in question.

### Follow-up candidates

_None._

## Visible output is decided (check 10)

Every expected value the diff adds or changes is covered: the formatter joins and the RH5101/RH5111/RH5302/RH5308 results by answer 1A (change doc visible output changes 1 and 2), the RH5307 reports, locations, fixes, and the rule page by answer 2A (changes 3–5); the comment-around-`?` fix results by unchanged SHARED-FIX-006/-010; the RH5115 location (decision 7) and the fixed-buffer characterization are `main`'s unchanged output. No `undecided output`.

## Cost of special cases (check 11)

The run adds no guard, condition, or special case: one parent kind joins an existing predicate, and a private copy is deleted. No FMT-UNSUP shape is declared or touched; no guard serving only a declared shape is in the diff.
