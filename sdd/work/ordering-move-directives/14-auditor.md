Role: auditor
Commit: 5a0472e
Verdict: #970: pass; #1668: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict to ignore.

## thoenissen/Reihitsu#970

### Scope

Full audit of SHARED-ORDER-011 and its changes to SHARED-ORDER-005/006, RH7102-006, RH7103-007, RH7107-005, RH7108-005, RH7109-006, and RH7309-005: `OrderingMoveSafety.ContainsPositionSensitiveDirectivesThatBlockRelocation`, `IsCarriedByItsDeclaration`, `GetDirectivesOfKind`, `FormClosedPairs`, `Closes`, `SplitsConditionalGroup`, `RelocationChangesDirectiveScope`; `SyntaxTriviaUtilities.GetPositionSensitiveDirectives` and the deletion of `ContainsPositionSensitiveDirectives` (decision 10); `RH7309RegionsShouldFollowCategoryOrderCodeFixProvider.CanReorderSafely`/`ContainsUnsafeDirectives`; the tests of `04`, `08`, and the Core tests. Counterparts opened outside the diff: `AccessorOrderingUtilities.MoveRangeContainsDirectives`, the RH7107/RH7108 providers' `IsMoveSafe`, `TypeMemberOrderingCodeFixProviderBase.IsMoveSafe`, `DirectivePairSplitScan`, `RegionBlockLayout`.

Checked against the spec, by tracing:

- *Inside one declaration*: the directive's token is found among the list's nodes by `node.Span` (excluding the first token's leading trivia); `tokenStart == declaration.SpanStart` hands the first token to SHARED-ORDER-005. For member and accessor moves every declaration with a token in either span lies wholly in it (the spans are bounded by `FullSpan`s of list nodes); for RH7309, `span.Contains(declaration.Span)` holds for every member of a block or gap. Nested-type members resolve to the nested type, as the spec says.
- *Pairs*: `Closes` requires `disable`→`restore` with `ErrorCodes` compared by `ToString()` (trivia-free, so the *Suppress in Source* trailing comment matches) in order; `#pragma warning enable` (Roslyn's `EnableKeyword`) is neither opening nor closing and refuses. `#nullable` needs `enable`/`disable` → `restore` with the same `TargetToken` kind (`None` for no target). Odd counts refuse.
- *One branch*: `SplitsConditionalGroup` reads every conditional directive between the pair's two directives in the whole tree and refuses when a related directive lies outside.
- *Nothing open in front*: `[0, spansStart)` of the whole document, `spansStart` = target's `FullSpan.Start` for member and accessor moves, the first block's start for RH7309 (970-4 B). Directives in excluded branches are directive trivia (Roslyn lexes every directive of an excluded section with `LexSingleDirective`), so they count, as the spec requires.
- Soundness: with every directive of the kind in front closed and every directive of the kind in both spans inside one declaration and closed there, the project's setting is in effect at every declaration boundary of the spans before and after the move, so no code changes its governing directive. Kinds are checked independently, which is sound because `#pragma warning` state and the nullable context are separate.
- RH7309 inspects every block and gap (970-4 B) with the same predicate, so the architecture finding of `02-skeptic.md` (decision 1) is closed: one owner of the pairing rule.

Answers 970-1 A, 970-2 A, 970-3 B, 970-4 B all hold in code and tests.

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| – | – | – | – | – | – | None. | – |

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| A `#pragma warning`/`#nullable` directive in a span is carried (SHARED-ORDER-011 *Inside one declaration*) | leading trivia of tokens of one list node behind its first token (`node.Span`, `tokenStart != SpanStart`) | SHARED-ORDER-005: leading trivia of the first token refuses (`tokenStart == declaration.SpanStart`) | Complementary at the first-token boundary; both sides tested (`NoCodeFixWhenTheDisableOfAPairStandsInFrontOfTheFirstTokenOfACrossedMethod`, `FixMovesAMemberAcrossAPairWhoseDisableStandsBehindTheAttributeListOfACrossedMethod`) | ok |
| Pairs inside the declaration | `GetDirectivesOfKind(declaration, declaration.Span)` | the directives the outer check enumerates (`GetPositionSensitiveDirectives(root, span)`) | Same start boundary (`SpanStart` inside the span); the outer one descends into structured trivia, which holds no directive in reachable input | ok |
| Nothing open in front | `[0, spansStart)` of the document | SHARED-ORDER-003 crossed span start (`targetNode.FullSpan.Start`); RH7309 first block start | Equal anchors; directives at `spansStart` belong to the crossed span (and SHARED-ORDER-005) | ok |
| One branch | `(opening.Span.End, closing.SpanStart)` over the whole tree | `GetRelatedDirectives` of each conditional between | Group wholly between keeps, split refuses; both tested (RH7107, RH7108, WP in-front) | ok |
| RH7309 spans for SHARED-ORDER-011 | every block and gap (`CanReorderSafely`) | SHARED-ORDER-011 RH7309 clause, 970-4 B | Equal | ok |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Which directives are position sensitive | `SyntaxTriviaUtilities.GetPositionSensitiveDirectives` (only caller `OrderingMoveSafety.ContainsPositionSensitiveDirectivesThatBlockRelocation`); `ContainsPositionSensitiveDirectives` gone from `src` and `tests` | one owner |
| Which `#pragma warning`/`#nullable` pair a reorder carries | `OrderingMoveSafety.ContainsPositionSensitiveDirectivesThatBlockRelocation` (callers `RelocationChangesDirectiveScope` for member and accessor moves, `RH7309…CodeFixProvider.ContainsUnsafeDirectives`). `rg "PragmaWarningDirectiveTrivia|NullableDirectiveTrivia" src` also finds `DirectivePairSplitScan` — a different decision (whether inserted braces split a pair), with its own ID normalization; not a copy | one owner |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| WP fix tests (crossed pair, own-body pair, no IDs, several IDs, trailing comments, consecutive pairs, `#nullable` targets, enclosing nullable pair, behind attribute list, nested type, closed pair in front, around earlier type, group wholly between) | the fix is offered and produces the exact moved text | no fix, or any other text | `Verify`/`VerifyCodeFixWithEndOfLine` (exact fixed and batch state, convergence) | adequate |
| WP/RH7107/RH7108/RH7309 no-fix tests (lone, other IDs, nested pairs, nullable mismatch, first token, unclosed in front active and excluded, split pairs, `#line`, `#warning`, `#pragma checksum`) | diagnostic reported, no action registered | an action is registered | `VerifyNoCodeFixIsOffered` (asserts the diagnostic exists and every fixable diagnostic has no action) | adequate |
| WP `UnclosedNullableDirectiveInFrontOfTheSpansWithholdsOnlyTheMoveAcrossANullablePair` | the in-front check is per kind | an open `#nullable` refusing a `#pragma warning` pair move, or keeping a `#nullable` pair move | both helpers | adequate |
| WP `FixAllCorrectsEveryDiagnostic` | Fix All over two moves across pairs | stale positions or a missed move | batch base scenario | adequate |
| Core `MoveRangeContainsDirectives…ClosedWarningPair`/`…UnclosedWarningDirective`/`…LineDirective`, `AccessorOrderingUtilitiesTests` pair tests | boolean of the guard | the opposite result | `Assert.IsFalse/IsTrue` | adequate |
| `SyntaxTriviaUtilitiesTests.GetPositionSensitiveDirectives*` | exact directive kinds in order | another kind, or none | `Assert.AreSequenceEqual`/`IsEmpty` | adequate (stronger than the old bool) |
| `OrderingMoveSafetyTests.ContainsPositionSensitiveDirectivesThatBlockRelocationReturnsTrueForMissingRoot` | null root refuses | `false` | `Assert.IsTrue` | adequate |

### Failure classes dismissed
- FC-005 — no comment guard is involved; the predicates read directive trivia only.
- FC-006, FC-022 to FC-029, FC-036 — the moves are unchanged text moves (SHARED-ORDER-009); no rewrite of syntax, literals, identifiers, or expression bodies.
- FC-007, FC-009 — convergence and action registration unchanged; every new fix test runs the iterative and batch verifier.
- FC-010 to FC-014, FC-028, FC-030, FC-032 — no formatter surface: the ordering fixes do not format.
- FC-018 — no rename.
- FC-021 — no line join.
- FC-034, FC-035 — not a bug reproduction; expected outputs are `main`'s output of the same input without the pragma lines (`01`, `02`), not taken from a patched copy.
- FC-037 — no comment-only claim.
- FC-038 to FC-043 — no shape declared (decision 2).
- Traced and holding: FC-001/FC-002 (guard spans above), FC-003 (both sides of the first token, of "in front", of *One branch*), FC-004 (pairs move with their declaration), FC-008 (Fix All), FC-015 (policy table), FC-016 (RH7107, RH7108, RH7309 siblings all reach the same predicate), FC-017 (table), FC-019/FC-020 (directives in every gap kind the spans hold), FC-031 (summaries of `IsMoveSafe`, `MoveRangeContainsDirectives`, `CanReorderSafely`, `ContainsUnsafeDirectives`, `GetPositionSensitiveDirectives` and its three crefs describe the new code), FC-033 (LF and CRLF on the spec examples), FC-044 (no pragma state is modelled; refusal by closed pairs).

### Architecture findings
_None._ The skeptic's finding is resolved by decision 1.

### Hints
- `IsCarriedByItsDeclaration` re-reads the document prefix `[0, spansStart)` and the declaration's directives once per directive in the spans; the prefix result is the same for every directive of one kind. Code-fix registration only, so not a hot path.

### Follow-up candidates
_None._

## thoenissen/Reihitsu#1668

### Scope

Full audit of SHARED-ORDER-007/008 as changed, RH7102-006, RH7103-007, RH7109-006, RH7309-005, decisions 4, 6, 7, 9: `OrderingDeclarationUtilities.ChangesMemberOrder(…, Func<int,int>)`, `GetCrossedPositions`, `DeclarationHoldsConditionalDirective`, `WrapsWholeDeclaration`, `AnyDeclarationHoldsConditionalDirective`; the shrunk `ConditionalGroupUnitUtilities` and its one remaining caller `ConditionalGroupsWrapWholeMembers` (RH2001-010); `RegionBlockLayout` (outside the diff, RH7309's rank function); the tests of `04` and `09`.

Checked, by tracing and with `scripts/apply-fix.sh RH7309` on 5a0472e and on `origin/main` (7975188, temporary worktree, removed), LF and CRLF:

- Member moves: the rank function (`0` in front of the target's `FullSpan.Start`, `2` crossed, `1` moved, `3` behind) makes `GetCrossedPositions` exactly the moved and the crossed span of SHARED-ORDER-003, so any `#if`/`#elif`/`#else`/`#endif` there refuses (1668-1 B), nested types included (`DescendantTrivia` descends). A group behind the spans and one in front of them for a move that reorders no instance field keep the fix.
- RH7309: `RegionBlockLayout.GetNewRank` keeps every gap at its written rank, so a gap or a kept block is crossed exactly when a block is moved across it (decision 9, spec sentence of `07`).
- 1668-2 A exception: `WrapsWholeDeclaration` requires exactly `#if`/`#endif`, the `#if` on the first token and the `#endif` on the token behind the last token (end of file included); a foreign `#endif` on the first token fails it, as decision 4 reads the answer.
- RH2001-010 behavior unchanged: `WrapsWholeMembers` with the nesting check and `ReadsAsWholeMembers` equal `main`'s `WrapsWholeUnitsOf(…, (type, false), true, …)` with the type-body arm of `ReadsAsWholeUnits`.
- SHARED-ORDER-007: counting only active members is sound because no member of excluded code can lie in the spans (any conditional directive there refuses), and excluded code in front of or behind the spans keeps its order against them.

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | high | `src/Reihitsu.Core/OrderingDeclarationUtilities.cs:418` (`offset < Array.IndexOf(isCrossed, true)`); `sdd/specs/shared/ordering-move-safety.md:630` (SHARED-ORDER-008, *In the reordered body, in front of the spans*, RH7309 clause) | FC-001, FC-002 | – | PR-introduced | The layout clause reads conditional directives only in front of the **first** crossed position. RH7309 can reorder two disjoint runs of blocks; a group in the uncrossed code between them stands in front of the moved fields of the second run but is not read, so the RH7309 fix reorders instance fields that another build lays out as a struct. Answer 1668-2 A says any `#if` block in front of the moved fields withholds an instance-field reorder, and SHARED-ORDER-008's own *Why* names exactly this shape (a group that closes the type and opens a struct). `main` withholds the fix. Counterexample, `class C : Base, IDisposable` with blocks `Object` (override `ToString`), `Methods` (`Dispose`), then `#if X` / `}` / `public struct S` / `{` / `#endif`, then `#region IDisposable` (`private int _d;`) and `#region Base` (`private int _b;`): RH7309 reorders to `Methods`, `Object`, `Base`, `IDisposable`; HEAD: fixed (1 action, LF and CRLF), `_b` now in front of `_d`; in a build with `X` that is the field order of struct `S`, which sets its layout (SHARED-FIX-003, *Run time* and *Build configurations*). `origin/main`: no-fix-offered. Members of the class (rare input class): group between two runs + second run reorders instance fields + type is a class — **fails**. Holding: group in front of the first run (control fixture with only `Methods` in front of the group: no-fix-offered on HEAD); group inside a run (spans refusal); group behind the last run (no member behind it is reordered; `ConditionalGroupBetweenBlocksThatNoMovedBlockCrossesDoesNotWithholdTheFix`); second run reorders only methods or static members (no instance field; spec example 2, `ConditionalGroupBetweenTwoPairsReorderedOnTheirOwnSideDoesNotWithholdTheFix`); second run reorders initialized members (refused by the initializer count of the active tree); struct (already `fieldOrderSetsLayout`); member moves RH7102/RH7103/RH7109 (one contiguous run, so "in front of the first crossed position" is "in front of the moved code") | Refuse this class: for an instance-field reorder, count a conditional directive anywhere in the body in front of the last moved or crossed position (in front of the moved fields, as 1668-2 A states), and say so in SHARED-ORDER-008's RH7309 clause and the private `ChangesMemberOrder` summary ("in front of the first such position"); add a no-fix RH7309 test on the counterexample. Withholds only a rare class and restores `main`'s output; nothing visible is in question |

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| A conditional directive in the spans refuses (SHARED-ORDER-008, first paragraph) | `GetCrossedPositions` over the body `(OpenBrace.End, CloseBrace.SpanStart)` | `OrderingMoveSafety.MoveRangeContainsDirectives` spans (`target.FullSpan.Start` … `moved.FullSpan.End`) for member moves; RH7309 `CanReorderSafely` blocks and gaps | Equal for member moves; for RH7309 a subset by design (uncrossed blocks and gaps excluded, decision 9) | ok |
| The type counts as layout-sensitive by a group in the body in front of the spans | `offset < first crossed position` | 1668-2 A ("in front of the moved fields"); SHARED-ORDER-008 *Why*; `ChangesMemberOrder(…, Func)` public summary ("in front of the code the reorder moves") | Disagree when RH7309 reorders two disjoint runs | finding #1 |
| In front of a declaration of the type, with the whole-declaration exception | leading trivia of the first token; `#if` there, `#endif` on `GetLastToken().GetNextToken(includeZeroWidth: true)` | `DeclarationHoldsConditionalDirective`'s token range (`TakeWhile(SpanStart <= bodyToken.SpanStart)`) | Agree; end-of-file side tested (`FixMovesAFieldOfATypeThatAGroupWrapsWholeAtTheEndOfTheDocument`), foreign `#endif` tested (`NoCodeFixWhenAGroupAroundTheTypeClosesBehindAnotherType`, `…PartialClass…EndsOnTheFirstTokenOfTheReorderedOne`) | ok |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Conditional directive in the spans refuses a member move | `OrderingDeclarationUtilities.ChangesMemberOrder(…, Func<int,int>)`, reached by `TypeMemberOrderingCodeFixProviderBase.IsMoveSafe` (RH7102, RH7103, RH7109) and `RegionBlockLayout.ChangesMemberOrder` (RH7309) | one owner |
| Which body positions a reorder moves or crosses | `GetCrossedPositions` (`GetReorderedSpan` gone) | one owner |
| Whether a conditional group around members is harmless (excluded-text parse) | `ConditionalGroupUnitUtilities.WrapsWholeMembers`/`ReadsAsWholeMembers`/`GetExcludedText`/`ParseInTypeBody`, only via `ConditionalGroupsWrapWholeMembers` (RH2001-010); `WrapsWholeUnits`, `GetBoundaryLists`, `IsUnitListOfTypeBody`, `ForbidsFieldKeyword`, `GetMembersOfDisabledText`, `DeclaresTypeWithAttributes` gone from `src` | one owner; the RH2001 divergence is the recorded follow-up |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH7103 no-fix matrix (crossed body, moved leading trivia, `#if false`, attribute lists, accessors moved and crossed, switch section, nested type, `#elif`, target's leading trivia), RH7102 and RH7109 moved/crossed tests | diagnostic reported, no action | an action registered | `VerifyNoCodeFixIsOffered` | adequate |
| RH7103 `FixMovesAMemberInFrontOfAGroupBehindTheSpans`, `FixMovesAStaticMethodBehindAGroupInTheBodyInFrontOfTheSpans`; RH7109 `FixMovesAFieldOfATypeThatAGroupWrapsWhole*` | exact moved text | no fix or other text | `Verify`/`VerifyCodeFixWithEndOfLine` | adequate |
| RH7103 `FixAllMovesOnlyTheStaticMembersWhoseSpansHoldNoConditionalDirective`, RH7102 `FixAllSkips…` | Fix All moves the safe member, leaves the other reported | both moved, or none | `Verify` with markup in fixed and batch state | adequate |
| RH7109 TypeGroup layout tests (group in body in front, `DEBUG` defined or not; `#else`/`#elif`/empty `#else` wraps; foreign `#endif`) | 1668-2 A for member moves | fix offered | `VerifyNoCodeFixIsOffered` | adequate |
| RH7309 Trivia `ConditionalPairInsideAKeptBlockThatMovedBlocksCrossWithholdsTheFix`, `ConditionalGroupBetweenTwoPairsReorderedOnTheirOwnSideDoesNotWithholdTheFix`, `ConditionalGroupInCodeThatAMovedBlockCrossesBetweenTwoReorderedPairsWithholdsTheFix`, `NoCodeFixWhenAGroupInFrontOfTheFirstBlockPrecedesAReorderOfInstanceFields` | decision 9 crossing, 1668-2 A in front of the first block | the other outcome | both helpers | adequate for what they cover; no test has a group between two runs with instance fields in the second run — the missing test of finding #1 |

### Failure classes dismissed
- FC-005, FC-021 — no comment guard or join.
- FC-006, FC-022 to FC-029, FC-036, FC-018 — text moves only; no rewrite or rename.
- FC-007, FC-009 — unchanged mechanics; every fix test runs the iterative and batch verifier.
- FC-010 to FC-014, FC-028, FC-030, FC-032 — no formatter surface.
- FC-034, FC-035, FC-037 — not a reproduction, outputs are `main`'s for unchanged inputs, no text-only claim.
- FC-038 to FC-043 — no shape declared (decision 2).
- Traced and holding: FC-003 (target leading trivia vs separated by `_count`; crossed vs uncrossed RH7309 gaps and kept blocks; exception's `#endif` place), FC-004, FC-008, FC-015, FC-016 (RH7102/RH7103/RH7109/RH7309 share one call), FC-017, FC-019/FC-020 (`#if false`, `#elif`, `#else`, active groups, every unit list), FC-031 (the summaries of `ChangesMemberOrder` overloads, `ChangesInitializerExecutionOrder`, `DeclarationHoldsConditionalDirective`, `WrapsWholeDeclaration`, `GetCrossedPositions`, `ConditionalGroupUnitUtilities` describe the new code, except the private overload's "in front of the first such position", which finding #1 changes), FC-033, FC-044 (no excluded branch read by the member-moving fixes). FC-001/FC-002: finding #1.

### Architecture findings
_None._ Finding #1 is one component reading the wrong anchor, not two owners of one decision.

### Hints
_None._

### Follow-up candidates
_None_ beyond RH2001-010's remaining excluded-text parse, which `00-lead.md` records.

## Whole run

- State: checkout at 5a0472e (`git rev-parse HEAD`); `git ls-remote origin refs/heads/main` = 79751883bd389bcabe85d703a9bc61c956529d66, an ancestor of HEAD (merged in 87e7b5e). The diff (`git diff --stat origin/main...HEAD`, 44 files) matches the handovers: six production files (`OrderingMoveSafety`, `OrderingDeclarationUtilities`, `ConditionalGroupUnitUtilities`, `SyntaxTriviaUtilities`, `TypeMemberOrderingCodeFixProviderBase`, the RH7309 provider), the specs and two change docs, and the tests of 04, 08, 09.
- `scripts/lane-check.sh behavior-change`: confirmed.
- Visible output is decided: every changed expected value — flips from a fix to no fix (1668-1 B, 1668-2 A, decision 6, decision 9), flips from no fix to a fix (970-1 to 970-4) — is covered by `Maintainer answers` or by a decision the answers cover; every new fixed output is `main`'s move of the same input without the directives (SHARED-ORDER-002/009, unchanged). No rule page changes.
- Scope: no edit outside the run; no `TODO` and no tracker reference in the added source or tests.
- Special cases: SHARED-ORDER-011's checks are cheap positional checks that serve common code (local suppressions in member bodies); #1668 deletes the excluded-text parse from the member-moving fixes. No guard serves a declared unsupported shape.
- Executed: `scripts/apply-fix.sh RH7309` over two fixtures (finding #1 and its control) on 5a0472e and in an `origin/main` worktree; no test was re-run.
