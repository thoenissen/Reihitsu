Role: auditor
Commit: 69b76812e5ba4333c873c1bc956c8125b08e261e
Verdict: findings

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Whole run

### Scope

- Head: `69b7681` (checked out, `git rev-parse HEAD`). Base: `origin/main` = `68f236ea94fdcbcf4aa14f98ff759d2c06708f32`, the SHA in my prompt; `git merge-base --is-ancestor` confirms the branch contains it.
- Full audit of `68f236e..69b7681`: 7 production files, 7 test files, the run's working docs. The diff is what `03-implementer.md` and `02-test-implementer.md` describe; no other path changed.
- `scripts/lane-check.sh bug-fix`: confirmed — claimed bug-fix, diff needs bug-fix.
- The three issues were read on GitHub (thoenissen/Reihitsu#1956, #1958, #1963); every Done-when item is addressed (per-issue sections). The new tests assert no visible output that an unchanged requirement does not demand (check 10, below).
- No test re-run: no finding turns on one. The build and focused test results of the implementer and cleaner were taken after the last production commit (`7db988d`).

### Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | low | `src/Reihitsu.Core/OrderingDeclarationUtilities.cs:583-585` (`DeclarationHoldsConditionalDirective`) | – (dead branch) | – | PR-introduced | The trivia are now filtered with `SyntaxTriviaUtilities.IsConditionalDirective` before `trivia.GetStructure() as DirectiveTriviaSyntax`. A trivia of kind `#if`/`#elif`/`#else`/`#endif` always has a `DirectiveTriviaSyntax` structure, so the `as` cannot yield `null` and `entry.Directive != null` can no longer be false. Before the change it filtered out every non-directive trivia; now it is a dead condition that tells the reader a null is possible. The project has nullable disabled (`<Nullable>` in no props or csproj), so the condition does not serve flow analysis either | Drop `entry.Directive != null`, and cast instead of `as` (or select the structure with `OfType<DirectiveTriviaSyntax>()` as `ConditionalGroupUnitUtilities.GetConditionalDirectives` does) |
| 2 | low | `tests/Reihitsu.Analyzer.Test/Core/CommentPositionUtilitiesTests.cs:44-111` | FC-015 (in the tests) | – | PR-introduced | The new `IsOwnedByNestedConstructReturnsTheOwnershipOfEachConstruct` repeats the 15 `DataRow` inputs of `IsOwnedByNestedConstructMatchesFormatterOwnership` verbatim, and its two assertions (analyzer == expected, formatter == expected) imply the old test's only assertion (analyzer == formatter). The old test's single extra check is the fixture guard `Assert.AreNotEqual(element.GetFirstToken(), nextToken, ...)`. The fixture list is now stated twice, so a row added to one test does not reach the other — the drift the run removes from the production code | Keep one test: give the existing parity test's rows the expected value and its two `Assert.AreEqual(isOwned, …)` assertions, keeping its fixture guard, and delete the new copy (or keep the new test and move the fixture guard into it, deleting the old). The issue's "tests pass unchanged" is an `agent-filed` proposal; folding strengthens the test and changes no row |

### Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| #1956: does a node on the token's path own the token's line (`SyntaxIndentationUtilities.IsOwningConstruct`) | One node and the token; the walk spans are unchanged: analyzer `nextToken.Parent` up to (excluding) the region (`CommentPositionUtilities.cs:320`), formatter `token.Parent` up to (excluding) the element (`ListElementInteriorUtilities.cs:77`), `LiesInListElement` from the node up with `default` token (`:50-62`) | The deleted copies in `CommentPositionUtilities` and `ListElementInteriorUtilities` | Body is byte-identical to both deleted bodies (eleven arms, same order, `token != collection.OpenBracketToken`, default arm `IsIndentingScope`); every caller's walk unchanged | holds |
| #1958: RH7004 `WouldSeparateDirective` membership test | Leading trivia of the closing brace (`RH7004…CodeFixProvider.cs:197`) | Old type pattern `ConditionalDirectiveTriviaSyntax or ElseDirectiveTriviaSyntax or EndIfDirectiveTriviaSyntax` | `ConditionalDirectiveTriviaSyntax` has exactly the concrete subtypes `IfDirectiveTriviaSyntax` and `ElifDirectiveTriviaSyntax`; each of the four types has exactly one kind, equal to the trivia's kind; same set, same span | holds |
| #1958: `GetConditionalDirectives` | `root.DescendantTrivia(span)` with `span.Contains(trivia.SpanStart)` (`ConditionalGroupUnitUtilities.cs:27-31`) | The deleted node-form `IsConditionalDirective(DirectiveTriviaSyntax)` | Filter moved before `GetStructure`; same trivia set, same order; `DescendantTrivia` does not descend into structured trivia either way | holds |
| #1958: `DeclarationHoldsConditionalDirective` | Leading trivia of the declaration's tokens up to the body token (`OrderingDeclarationUtilities.cs:581-587`) | Same deleted node form | Same set, same tokens; the `entry.Token != firstToken` / `WrapsWholeDeclaration` arms unchanged | holds (dead null check: finding #1) |
| #1963: trivia behind a token (`FindTriviaBehindToken`) | Trailing trivia of the token, then leading trivia of `behind.GetFirstToken()` (`PrecheckPhase.cs:755-759`) | The four inlined bodies it replaces (`:449`, `:463`, `:476`, `:600`) | Identical reads for every call site; conditional: `(Q ?? WT) ?? (C ?? WF)` equals the old right-associative `Q ?? (WT ?? (C ?? WF))`; dot/name: `name.GetFirstToken()` unchanged | holds |

### Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Which node lays out a token's line | `rg -U "InitializerExpressionSyntax[\s\S]{0,60}or AnonymousObjectCreationExpressionSyntax" src` and `rg -U "SwitchExpressionSyntax[\s\S]{0,80}RecursivePatternSyntax" src`: only `SyntaxIndentationUtilities.IsOwningConstruct` (`SyntaxIndentationUtilities.cs:150`); `:185` (`HasAnchorScopeAncestor`, initializer or anonymous object among the ancestors) decides another question. Consumers: `CommentPositionUtilities.cs:322`, `ListElementInteriorUtilities.cs:59, 79` | one owner |
| Conditional-directive set | `SyntaxTriviaUtilities.IsConditionalDirective` (`SyntaxTriviaUtilities.cs:127`), callers `SyntaxTriviaUtilities.cs:456, 490`, `OrderingDeclarationUtilities.cs:583`, `ConditionalGroupUnitUtilities.cs:29`, RH2001 fix `:497, 511, 513`, RH7004 fix `:197`. `rg "ElseDirectiveTriviaSyntax\|ConditionalDirectiveTriviaSyntax\|BranchingDirectiveTriviaSyntax" src` and `rg "ElifDirectiveTrivia" src` leave only uses that decide something else: `DirectivePairSplitScan.cs:86-88` (per-kind switch), `SyntaxTriviaUtilities.cs:673-674` (detached branches), `ReihitsuFormatterHelpers.cs:238, 247`, `TrailingCommaCodeFixHelper.cs:43`, `RH8201…CodeFixProvider.cs:53`, `DirectivePairSplitScan.cs:166`, RH7004 `:199` (`#if` only) | one owner |
| Token-to-node trivia read in the precheck | `rg "GetFirstToken\(\)\|FindFirstUnsupportedOperatorTrivia" PrecheckPhase.cs`: the only `.GetFirstToken().LeadingTrivia` read is `:758` in `FindTriviaBehindToken`; nine call sites (`:451, 452, 465, 478, 602, 704, 710, 716, 722, 728`). The token-to-token reads (`:501, 532, 568, 631`) and the omitted-type-argument read (`:777`) are other reads, left alone under lead decision 3 | one owner |

### Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `CommentPositionUtilitiesTests.IsOwnedByNestedConstructReturnsTheOwnershipOfEachConstruct` (15 rows) | Each explicit arm of `IsOwningConstruct`, the collection `[` exception, and the default arm give their value through both walks | Dropping or changing an arm flips its row (checked per row: each owned row's next token has the arm's node as nearest owning ancestor, and none of those nodes is an `IsIndentingScope` scope) | Direct calls of `IsOwnedByNestedConstruct` and `FindOwningConstruct` | adequate; duplicates the parity test's rows (finding #2) |
| `SyntaxTriviaUtilitiesTests.IsConditionalDirectiveRecognizesExactlyTheConditionalDirectives` (13 rows) | Exactly the four conditional kinds are members | Adding or losing a kind flips its row | Direct call; `.Single` on the directive text guarantees each row finds one trivia | adequate |
| `ConditionalGroupUnitUtilitiesTests.GetConditionalDirectivesReturnsExactlyTheConditionalDirectives` | The four conditional kinds, in text order, and no `#region`/`#pragma`/`#nullable` | A wrong filter adds or drops a kind | `Assert.AreSequenceEqual` on the kinds | adequate |
| `UnsupportedAssignmentCommentTests.CommentInTheTrailingTriviaOfTheOperatorIsReportedBeforeOneInFrontOfTheRightOperand` | Trailing trivia of the token is read before the node's leading trivia | Swapped read order reports `// second` | `AssertUnsupportedFor` (LF and CRLF, unchanged output, reason, exact location) | adequate |
| `UnsupportedBinaryOperatorCommentTests.CommentInTheTrailingTriviaOfTheOperatorIsReportedBeforeOneInFrontOfTheOperand` | Same, binary operator | Same | Same; no other shape sees either comment (the `=`'s gap holds none) | adequate |
| `UnsupportedConditionalOperatorCommentTests.CommentInTheTrailingTriviaOfTheOperatorIsReportedBeforeOneInFrontOfTheBranch` | Same, `?` and `:` gaps; the order between the two gaps is pinned by the existing `FirstOfTwoCommentsIsReported` | Same | Same | adequate |
| `UnsupportedLinkDotNameCommentTests.CommentInTheTrailingTriviaOfTheDotIsReportedBeforeOneInFrontOfTheName` | Same, `.` and name | Same | Same | adequate |

All seven were written before the production change (`b54a551` before `7db988d`) and pass on both, so their expected values were not taken from patched code (FC-035).

### Visible output (check 10)

The four precheck tests assert a reported location, a visible output. Each is demanded by the unchanged requirement it tags: FMT-UNSUP-008, -011, -016, and -020 each state that the location is "the span of the first comment, directive, or disabled text in that place" (`sdd/specs/formatter/unsupported-syntax.md:314`, `:623`, and the corresponding lines of -016 and -020). No other test of a visible surface changed; no rule page changed.

### Failure classes dismissed

- FC-001, FC-002, FC-003: no guard narrowed or widened; every inspected span is unchanged (Guard scope table).
- FC-004, FC-005, FC-019, FC-020, FC-021: no edit joins, moves, or deletes tokens or trivia; the RH7004 and ordering guards read the same directives as before.
- FC-006, FC-022 to FC-027, FC-029, FC-036: no rewrite, rename, or layout changed.
- FC-007, FC-008, FC-009, FC-018, FC-030: no code fix's registration, output, or Fix All changed; RH7004's and the ordering fixes' withholding predicates decide the same set.
- FC-010 to FC-014: no formatter phase's output and no diagnostic changed.
- FC-028, FC-041: `LiesInListElement` (the node-level fact) calls the same predicate with the same `default` token; the precheck reads the same spans, so node-level and document inputs get the same answer as before.
- FC-038 to FC-040, FC-042, FC-043: no shape declared or changed; the precheck's refusal set is unchanged.
- FC-044: nothing models a computed result.
- FC-017, FC-035: see Assertion adequacy.
- FC-016: the precheck's remaining two-sided reads are token-to-token (lead decision 3); the conditional-directive and owning-construct searches found no sibling copy.
- FC-031: checked per issue; no stale summary found in a changed method (hints below for unchanged ones).
- FC-032: the implementer's whole-project runs and the cleaner's Release build were taken after `7db988d`, the last production commit.
- FC-033, FC-034: no reproduction (lead decision 1).
- FC-037: no text-only claim.

### Architecture findings

_None._ Each issue is itself the FC-015 finding the run resolves; the diff leaves one owner per policy and adds no structure.

### Hints

- `ListElementInteriorJoinRewriter.cs:36` and `ListElementAligner.cs:84` say the constructs are those `FindOwningConstruct` "names". Its summary now defers to `SyntaxIndentationUtilities.IsOwningConstruct`, so the sentence is true through the cref chain only. The prose list at `ListElementInteriorJoinRewriter.cs:36-38` omits anonymous objects and accessor lists, as it did on `main` (unchanged lines).
- `SyntaxIndentationUtilities`'s class summary ("for indentation-sensitive formatting and code fixes") does not name the analyzers, which call `IsIndentingScope` since before this run and now `IsOwningConstruct` too.
- `IsOwningConstruct`'s `token` parameter is documented as "Token"; `LiesInListElement` passes `default`, which the inline comment at `ListElementInteriorUtilities.cs:57-58` explains.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1956

### Against the issue and specs

- Done when: the predicate exists once in `Reihitsu.Core` (`SyntaxIndentationUtilities.IsOwningConstruct`, public, `SyntaxIndentationUtilities.cs:150`), both callers use it (`CommentPositionUtilities.cs:322`, `ListElementInteriorUtilities.cs:59, 79`), both copies and the "must stay in step" note are gone, and no existing analyzer or formatter test changed. Holds.
- SHARED-COMMENT-006's construct list matches the arms; body moved verbatim, `AccessorListSyntax` kept (lead decision 2).
- `Reihitsu.Analyzer` already called `SyntaxIndentationUtilities.IsIndentingScope` from the deleted copy, so the analyzer's dependency on `Reihitsu.Core` at run time is not new.
- Comments: `IsOwnedByNestedConstruct`'s cref points at the new owner; `FindOwningConstruct`'s summary refers to it; the new owner's summary lists every arm, the accessor list included. No `TODO`, no tracker number.

### Findings

#2 (Whole run).

## thoenissen/Reihitsu#1958

### Against the issue and specs

- Done when: the kind test exists once (`SyntaxTriviaUtilities.IsConditionalDirective`); the RH7004 fix and `ConditionalGroupUnitUtilities` call it; `OrderingDeclarationUtilities`, the other caller of the deleted node form, calls it too; no existing RH7004 or conditional-group test changed. Holds.
- The uses left alone, which the issue asks the PR to name, are listed under Policy ownership (Whole run).
- No spec states the set; nothing to check against.

### Findings

#1 (Whole run).

## thoenissen/Reihitsu#1963

### Against the issue and specs

- Done when: `FindTriviaBehindToken` is the one statement of the token-to-node read; the issue's two methods and, under lead decision 3 (B), the conditional and dot-to-name readers call it; no existing precheck test changed. Holds.
- FMT-UNSUP-008, -011, -012, -016, -020 to -022 read the same spans in the same order; the wrappers' scope facts (what a binary expression is, what an arrow clause is) are kept in their summaries; `FindTriviaBehindToken`'s summary states the read order and every gap it serves.

### Findings

_None._
