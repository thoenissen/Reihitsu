Role: auditor
Commit: 6be829f4c98ed6668906c561114a94684c0037dd
Verdict: findings

My prompt carried no intended fix, suspected cause, or hoped-for verdict. I worked from thoenissen/Reihitsu#1492, `00-lead.md` (decisions 16–22), `11-auditor.md`, `12-` through `16-`, the spec and change-doc diff, and the code. Outputs below were measured with the Debug CLI of this head (built with `scripts/build.sh --configuration Debug`, 0 warnings) and of the `origin/main` archive of the first audit (`3a7bbea`, session scratchpad), first and second pass, fixtures outside the repository.

## Scope

Repair delta against `440416769093c20a394e84098f82549189a51a41`. Head `6be829f4c98ed6668906c561114a94684c0037dd` (checked out, clean). Base `origin/main` = `3a7bbea376798e2a21cf9bc8fc1aeaeb4f26c81d`, an ancestor of head. The repair diff matches `12-` to `16-`: specs FMT-STRUCT-028/029, FMT-BREAK-027/032, the change doc, `AttributeListShapeUtilities`, `AttributeListMergeTransform`, `AttributeListSplitTransform`, `AttributeTargetFormattingRewriter.ShapeAccessorAttributeLists`, `AttributeTargetUtilities.MergeAttributeListGroup`, and the two attribute-list test files. Scope did not grow beyond the round-1 components. `scripts/lane-check.sh behavior-change`: confirmed. Focused run `AttributeListStructural|ParameterAttributeTargetLayout` (Debug): 126 passed.

### Previous findings

| # | Status | Evidence |
|---|---|---|
| 1 | closed | Merge now runs on every non-accessor owner with `UsesMergedListShape`, and the accessor path splits and merges with one predicate (`keepAccessorListsSingleLine \|\| UsesMergedListShape`), which is `main`'s `ResolveListShapeMode(list, keep)`. Measured equal to `main` (LF and CRLF): setter in a multi-line accessor list (`[param: A] [param: B] set`, also `[param: A]⏎[param: B]⏎set`), `[param: A] [C, D] [param: B] set`, method, field, delegate-like and class `typevar:` lists, local function, lambda (incl. `[param: A,⏎ B] [param: C]`), non-adjacent `[param: A] [B] [param: C]`, `[param: A]⏎[C]⏎[param: B]`, blank line between, `#region` + trailing comment, comment leading the declaration. Tests: `ParamAndTypevarListsOfAnotherOwnerAreMergedAndNotSplit`, `ParamListsOfASetter…`, `ParamAndTypevarListsOfEveryAccessor…`, `ParamAndTypevarListsOnOtherOwnersKeepTheirOutput`, `ParamListsOfALambdaKeepTheirOutput` |
| 2 | closed | FMT-STRUCT-028 states the `param:`/`typevar:` exception; FMT-STRUCT-029 states the merge on other owners; FMT-BREAK-032 states it for multi-line accessor lists, with an example equal to the measured output |
| 3 | closed | `[⏎ A, B] void M()`, `[⏎ method: A, B] void M()`, `[ method: A, B]`, accessor `[⏎ C, D] set` (multi-line list): one pass to `main`'s fixed point, LF and CRLF. Decision 18 and the change doc's visible change 1 cover it |
| 4 | closed | `void M([A,⏎ C] [B] int x, int y)` (LF, CRLF), `[A⏎ , C] [B]`, record, local function, lambda parameter list (`([A,⏎ C] [B] int x, int y) =>`), `[A]⏎[B,⏎ C]`: all wrap as on `main`. Holding members re-checked: `[⏎ A] [B]`, `[⏎ A]⏎ [B]`, `[param:⏎ A] [param: B]` equal `main` |
| 5 | closed | Both blocking tests now assert the directive line / the documentation comment's parts and both lists, once each and in order (`AssertInOrder`, which also rejects duplicates) |
| 6 | closed | Summaries of `AttributeListSplitTransform`, `AttributeListMergeTransform`, `AttributeListShapeUtilities` (class, `SplitAttributeLists`, `MergeAttributeLists`, `UsesMergedListShape`, `FindMergeableGroup`, `BuildSplitAttributeLists`), `ShapeAccessorAttributeLists`, and `MergeAttributeListGroup` describe the new code |
| Arch. 1 | closed | One predicate of the list decides split and merge on every owner; only "accessor or not" picks the phase (decision 2) |

## Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 7 | medium | `src/Reihitsu.Formatter/Pipeline/StructuralTransforms/Utilities/AttributeListShapeUtilities.cs:143` (`target = listToSplit.Target?.WithLeadingTrivia(…)`) and `:158-159` (`index > 0 \|\| target == null`); `src/Reihitsu.Formatter.Test/Unit/StructuralTransforms/AttributeListStructuralTransformTests.cs` `NothingBehindTheOpeningBracketStaysInTheFirstCreatedList` | FC-017 | – | same requirement | The repair added a second branch to "drop what stands behind the original `[`": with a specifier, the whitespace behind `[` is the specifier identifier's leading trivia, and the split drops it there instead of on the first attribute. No test has a specifier behind trivia after `[` (the new rows are `[⏎ A, B]`, `[⏎ A,⏎ B]`, `[  A, B]`; `CreatedListsKeepTheSpecifierAndTheWrittenOrder` has none). Reverting line 143 to `listToSplit.Target` leaves every test green while `[⏎        method: A, B] void M()` goes back to `main`'s non-idempotent first pass `[        method: A]⏎[        method: B]` (measured on `main`; head gives `[method: A]⏎[method: B]` in one pass, LF and CRLF). FMT-STRUCT-028 ("whitespace and line breaks behind the original list's `[` are dropped") covers this member, and decision 22 relies on that text. The accessor split (FMT-BREAK-032) reaches the same branch untested. | Add `[⏎        method: A, B]` → `[method: A][method: B]` and `[ return: A, B]` rows to the phase test, and one accessor row (`[⏎ param: …]` is not split, so use e.g. `[⏎ method: C, D] get;`) to the final-output tests. (Test implementer.) |
| 8 | low | `sdd/changes/2026-11/attribute-list-structural.md:258` (last sentence); `AGENTS.md:171` (`StructuralTransformPhase` row) | FC-031 | – | PR-introduced | The round-2 repair widened where the merges run, and two texts still describe the old extent. The change doc says "The accessor merge keeps none, since it runs only for an accessor list that stays on one line"; it now also runs in an accessor list over several lines for `param:`/`typevar:` lists (FMT-BREAK-032's own text, "In either layout…", is correct). The `AGENTS.md` row says the structural phase owns "merged lists of one target on parameters and type parameters"; it now also merges `param:`/`typevar:` lists on every non-accessor owner. Text only: neither reaches compiled behavior, diagnostics, public API documentation, or rule metadata. | Reword the change-doc sentence to "The accessor merge keeps none of the stretch's line breaks, in either layout (FMT-BREAK-032)", and extend the `AGENTS.md` row with "and of `param:`/`typevar:` lists on any non-accessor owner". (Spec author.) |

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Split a list (structural) | `CanSplit(list, UsesMergedListShape)` on every non-accessor owner | `main`'s `ResolveListShapeMode == SplitLists`; RH55xx `TargetAttributeListShapeAnalyzerBase` | Agree (measured: `[param: A, B]`, `[typevar: C, D]` kept; `[E, F]`, `[method: …]`, `[return: …]` split) | ok |
| Merge a group (structural) | `FindMergeableGroup(…, UsesMergedListShape)` on every node except `AccessorDeclarationSyntax`; `GetAttributeLists` returns `[]` for non-owners | The split's predicate (same function), `main`'s merge | Agree | ok |
| Accessor split and merge | One local predicate `keepAccessorListsSingleLine \|\| UsesMergedListShape` for both | `main`'s `ResolveListShapeMode(list, keepAccessorListsSingleLine)` for both | Agree (equivalent expression) | ok |
| Line break kept by the merge | `EndOfLine` in the first `[`'s trailing trivia → keep none; else `FindLineBreak` from `[`.Span.End to the token behind the group | Comment/directive guard `GroupInteriorContainsCommentOrDirective` from `nodes[0].Span.Start`; FMT-STRUCT-029 stretch | Agree on the left boundary now; the `[`-trailing exclusion matches the spec's "directly behind that `[` stays where it is". A break behind a specifier colon (`[param:⏎ A] [param: B]`) is both kept in place and carried; FMT-STRUCT-029 allows that ("MAY be dropped"), and the final output equals `main` on parameter and method owners | ok |
| What the first created list holds | Drops `[` trailing trivia, and the specifier's leading trivia (or the first attribute's when there is no specifier) | FMT-STRUCT-028; `CanSplit`'s comment guard over `node.Span` (covers both trivia, so no comment can be dropped) | Agree | ok; test gap on the specifier branch is finding 7 |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Merged-list shape of a list (formatter) | `AttributeTargetFormattingShared.ResolveListShapeMode`, read only through `AttributeListShapeUtilities.UsesMergedListShape`; accessor layout override in `ShapeAccessorAttributeLists` | One owner — ok |
| Split | `AttributeListShapeUtilities.BuildSplitAttributeLists`; code fix `TargetAttributeListShapeCodeFixProviderBase.BuildSplitReplacementLists` | Unchanged from audit 1 (decision 11, follow-up 6) |
| Merge | `AttributeTargetUtilities.MergeAttributeListGroup` (structural `None`, accessor and code fix `Spaced`) | One owner — ok |
| List shape (analyzer/code fix) | `TargetAttributeListShapeAnalyzerBase.ResolveListShapeMode`, `TargetAttributeListShapeCodeFixProviderBase.ResolveListShapeMode`, RH5531 overrides | As on `main`; the formatter now applies its result on every owner, so the SHARED-DIAG-010 disagreement of finding 1 is gone (head output equals `main`'s on every finding-1 member) |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| A `NothingBehindTheOpeningBracketStaysInTheFirstCreatedList` | Phase text `[A][B]`, LF/CRLF, round trip | Any byte behind the first `[` | `AssertAttributeListText` | adequate for its rows; specifier branch missing (finding 7) |
| A `LineBreakInTheStretchIsKeptOnce` (5 new rows) | Exactly one line break between merged `]` and type | 0 or 2 line breaks | `AssertLineBreaksBehindMergedList(exactlyOne)` | adequate |
| A `LineBreakDirectlyBehindTheOpeningBracketStaysThere` | One line break between `[` and first attribute, 2 attributes merged | Line break moved or lost, no merge | gap count + count | adequate |
| A `ParamAndTypevarListsOfAnotherOwnerAreMergedAndNotSplit` | Exact list texts of the owner, LF/CRLF, no elastic trivia, round trip | Not merged, split, wrong order | exact join of list texts | adequate (both sides: merged and never split) |
| B `LineBreaksBetweenTwoAttributesAreDroppedWithTheComma` (`P` row), `AccessorSplitDropsTheLineBreakBehindTheOpeningBracket` | Exact final output in one pass, LF/CRLF, phase stability | Indentation left behind `[` | `AssertRuleResult` | adequate |
| B `LambdaAttributeListWithALineBreakBehindTheOpeningBracketSettlesInOnePass`, `ParamListsOfALambdaKeepTheirOutput` | `main`'s fixed point in one pass, stable | Output difference | `AssertFirstPassAndSecondPass` (no phase check; lambdas unspecified) | adequate as characterization |
| B `LineBreakBetweenMergedListsStillWrapsTheParameterList` (`S` row), `LineBreakInsideTheFirstMergedListStillWrapsTheParameterList` | Exact final output, wrapped parameter list | Unwrapped list | `AssertRuleResult` | adequate |
| B `LineBreakInFrontOfTheClosingBracketOfTheFirstMergedListStillWrapsTheParameterList` | Merge and wrap in pass 1 and 2 | No merge, no wrap | `DoesNotContain` + `AssertInOrder` (whitespace before `]` excluded, reason given: hint of audit 1) | adequate |
| B `ParamListsOfASetter…OverSeveralLines…`, `ParamAndTypevarListsOfEveryAccessor…`, `ParamListsOfASetter…OnTheDeclarationsLine…`, `ParamAndTypevarListsOnOtherOwnersKeepTheirOutput` | Exact final output, LF/CRLF, phase stability | Not merged or split | `AssertRuleResult` | adequate |
| B `DirectiveLeadingTheFirstListBlocksTheMerge`, `DocumentationCommentBetweenTheListsBlocksTheMerge` | Merge refused; directive / comment parts and both lists survive, once, in order | Lost or duplicated directive/comment/list | `AssertInOrder` + stability | adequate (closes finding 5) |

Visible output (check 10): every expected value the repair adds is `main`'s output (measured by the test implementer on the `main` archive and re-measured here for the probes above), demanded by an unchanged requirement (FMT-PIPE-004/FMT-SPACE-004 for the one-pass rows), or covered by decisions 4, 18, 22 and maintainer answer 1. The `[⏎ A, B]` row is listed in the change doc's visible change 1. No rule page changed.

## Failure classes dismissed

- FC-001, FC-002 — re-traced above (Guard scope); the merge's line-break stretch and comment guard now share the left boundary.
- FC-003 — both sides of each new condition are tested: owner kind (parameter / other / accessor), accessor layout, `[`-trailing line break versus elsewhere, split versus merge.
- FC-005, FC-019, FC-020, FC-021 — split and merge guards unchanged; the newly dropped trivia (`[` trailing, specifier leading) lie inside the list's span, which `InteriorContainsCommentOrDirective` covers, so no comment or directive can be dropped; `#region`, trailing comment, and leading comment probes equal `main`.
- FC-006, FC-024 — specifiers kept; `type:`/unknown targets excluded by `TryResolveTarget` as before.
- FC-007, FC-008, FC-009, FC-018, FC-030 — no code-fix path changed: the two-argument `MergeAttributeListGroup` passes `Spaced`, for which the new condition is not evaluated (`separators == None &&` short-circuits).
- FC-010, FC-011, FC-012 — every probe stable on a second pass under LF and CRLF (except `main`'s own two-pass rows); loops remove one list or split one list per iteration.
- FC-013 — formatter output equals `main`'s on every finding-1 member, so RH5524 agrees as on `main`.
- FC-014, FC-036 — no rule or rename.
- FC-015 — one shape predicate (Policy ownership).
- FC-016 — split trivia behind `[` fixed on declaration, accessor, lambda; the specifier member holds but is untested (finding 7).
- FC-022, FC-023 — attributes are moved whole.
- FC-025, FC-029 — not reached.
- FC-026 — token identity, unchanged.
- FC-027 — nested owners: the merge transform visits bottom-up (`base.Visit` first); lambdas inside accessor bodies, local functions inside methods, and lambda parameters probed.
- FC-028 — both transforms are unconditional in `CreateRewriters`; `FormatNodeInDocumentAsyncSplitsAndMergesAttributeLists` unchanged and passing.
- FC-031 — finding 8; code comments re-read and accurate.
- FC-032 to FC-035, FC-037 — no text-only claim; test expectations written before the production repair (`13-test-implementer.md`, 22 failing at `9a97d4e`).

## Architecture findings

_None._ The repair resolves architecture finding 1 of `11-auditor.md` as decision 16 asks; no new shared decision or copy was added.

## Hints

- `AttributeListMergeTransform.Visit` now runs `MergeAttributeLists` (one `GetAttributeLists` call and a LINQ pass over an empty list) for every node of the tree; `AttributeListSplitTransform` already did the same, so this is parity, not a new hot-path cost.
- Pre-existing, unchanged: whitespace in front of a list's `]` needs a second pass on both `main` and head (audit 1 hint, follow-up 3).
