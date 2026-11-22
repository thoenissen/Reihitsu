Role: auditor
Commit: 0a5c616
Verdict: pass, text cleanup

The prompt carried no intended fix, suspected cause, or hoped-for verdict to ignore.

## Scope

Head 0a5c616 (`git rev-parse HEAD`); `git ls-remote origin refs/heads/main` = 79751883bd389bcabe85d703a9bc61c956529d66, an ancestor of HEAD. Repair delta against 5a0472e (`git diff 5a0472e HEAD`, excluding `sdd/work`): `OrderingDeclarationUtilities.ChangesMemberOrder(TypeDeclarationSyntax, bool, Func<int,int>)` (anchor of the layout clause), SHARED-ORDER-007/008, RH7309-005, the change doc `ordering-move-refuses-conditional-groups.md`, and four RH7309 tests. The diff is what `15` to `19` describe. Scope did not grow: same component, same requirement, no new type or member. `scripts/lane-check.sh behavior-change`: confirmed. No `TODO` and no tracker reference in the added source or tests.

Previous findings:

- **#1 — closed.** The failing member (group between two runs, second run reorders instance fields, type is a class) is now refused: the anchor is the start of the last member that is `IsLayoutField` and stands at a crossed position, and every instance field whose order changes stands at a crossed position (for fields at `p < q` with `rank(p) > rank(q)`, `q` has a larger rank in front of it and `p` a smaller one behind it, which is exactly `GetCrossedPositions`), so every directive in front of a reordered field is read. Pinned by `NoCodeFixWhenAGroupBetweenTwoReorderedRunsPrecedesAReorderOfInstanceFieldsThatOpensAStruct` (the counterexample) and `NoCodeFixWhenAGroupOfMembersBetweenTwoReorderedRunsPrecedesAReorderOfInstanceFields` (the RH7309-005 example). The members #1 listed as holding still hold: group in front of the first run, inside a run, behind the last run (`ConditionalGroupBetweenBlocksThatNoMovedBlockCrossesDoesNotWithholdTheFix`), second run reorders only methods or static members (`ConditionalGroupBetweenTwoPairsReorderedOnTheirOwnSideDoesNotWithholdTheFix`; fixture `staticsecond` below), struct (`fieldOrderSetsLayout`).
- Member moves (RH7102, RH7103, RH7109): the spans are one contiguous stretch `[target.FullSpan.Start, moved.FullSpan.End)`, so a directive between the first crossed position and the last crossed layout field is crossed and refuses before the clause; with no crossed layout field the anchor is `-1`, but then `ChangesOrder(IsLayoutField)` is false as well. Same result as at 5a0472e in every case; not re-run (implementer `17`: 621 of 621 for the six rules).

Executed: `scripts/apply-fix.sh RH7309` on HEAD over six fixtures (the RH7309-005 example layout with the `#if DEBUG` method group between `Methods` and `IDisposable`), LF and CRLF, scratchpad directory:

| Fixture | Second run (`IDisposable`, `Base`) | First run (`Object`, `Methods`) | Expected | Result |
|---|---|---|---|---|
| `prop` | auto-properties `D`, `B` | methods | no fix (layout fields reordered behind the group) | no-fix-offered |
| `evt` | field-like events `D`, `B` | methods | no fix | no-fix-offered |
| `fieldprop` | field `_d`, auto-property `B` | methods | no fix | no-fix-offered |
| `computedprop` | expression-bodied properties | methods | fix (no backing field) | fixed |
| `staticsecond` | static fields `_d`, `_b` | instance fields `_o`, `_m` | fix (group behind the last instance field of the spans) | fixed |
| `overrefuse` | field `_d`, order kept | instance fields `_o`, `_m` swapped | no fix: `_d` is a moved field and the group stands in front of it (1668-2 A "in front of the moved fields", decision 13) | no-fix-offered |

Every member kind of `IsLayoutField` reaches the anchor through the same predicate that `ChangesOrder` uses.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 2 | low | `src/Reihitsu.Core/OrderingDeclarationUtilities.cs:272` (public `ChangesMemberOrder(TypeDeclarationSyntax, INamedTypeSymbol, Func{int,int})` summary); `tests/Reihitsu.Analyzer.Test/Formatting/RH7309RegionsShouldFollowCategoryOrderTriviaTests.cs:1736`, `:1802` (and the older `:1695`) | FC-031 | – | PR-introduced | The public overload, whose behavior the repair changed underneath it, still says a type counts as layout-sensitive when "the reordered declaration's body holds a conditional directive in front of the code the reorder moves"; for RH7309 the clause now reads up to the last instance field of the spans, and a group in front of moved code that holds no instance field (`ConditionalGroupBetweenTwoPairsReorderedOnTheirOwnSideDoesNotWithholdTheFix`) does not count. The two new test summaries (and the older one at `:1695`) name "the SHARED-ORDER-008 place in the reordered body in front of the spans", which the repair renamed *In the reordered body, in front of the moved fields* and re-anchored. Comment text only; compiled behavior is correct | State the anchor as the private overload does: "in front of the last instance field of the type's layout in the code the reorder moves or moves code across"; rename the place in the three test summaries to "in front of the moved fields" |
| 3 | low | `sdd/specs/analyzer/RH7309.md:609` (RH7309-005, new example, sentence behind the code block) | – | – | PR-introduced | The example's group is `#if DEBUG` / `public void Trace() { }` / `#endif`, yet the explanation says "in a build with `DEBUG`, the group could close `C` and open a struct whose fields `_d` and `_b` become". For the text shown, a build with `DEBUG` compiles one method and nothing else; the refusal is positional (the fix reads no branch, SHARED-ORDER-008 *Why*), and the sentence gives a reason the example itself contradicts. Spec text only | Give the positional reason: "the fix does not read the group, and a group in that place can close `C` and open a struct whose fields `_d` and `_b` become" (as SHARED-ORDER-008's *Why* puts it) |

Why `pass, text cleanup`: #2 is in XML documentation comments of a method and of tests, #3 in a spec's prose; neither reaches compiled behavior, diagnostics, or rule metadata.

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| A conditional directive outside the spans makes the type count as layout-sensitive (SHARED-ORDER-008 *In the reordered body, in front of the moved fields*) | body offsets `< lastCrossedLayoutFieldOffset` (start of the last `IsLayoutField` member at a crossed position; `-1` when none) | the fields whose order changes (`ChangesOrder(IsLayoutField)`, same predicate, ranked at `SpanStart` as the crossing is); 1668-2 A; SHARED-ORDER-008 and RH7309-005 text | Every reordered field is crossed, so the anchor is at or behind each of them; a directive behind it stands behind every reordered field (keeps: `FixReordersInstanceFieldsOfTheFirstRunInFrontOfAGroupBetweenTwoReorderedRuns`); strict `<` is exact because a directive is trivia in front of its member's `SpanStart`, and one inside a crossed member is refused earlier | ok |
| The clause applies only to a reorder of instance fields | `ChangesOrder(IsLayoutField)` | the anchor's `IsLayoutField` | Same predicate; a crossed field whose order stays does not trigger the clause alone (`FixReordersTheBlocksWhenAGroupBetweenTwoReorderedRunsPrecedesAnInstanceFieldWhoseOrderStays`) | ok |
| Index `isCrossed[member.SpanStart - bodyStart]` | members of `typeDeclaration.Members` | `GetCrossedPositions` length `CloseBrace.SpanStart - bodyStart` | Every member's first token lies between the braces; the same bounds the directive offsets already used | ok |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Where a body directive makes the type layout-sensitive | `rg -n "IsLayoutField\|GetCrossedPositions\|lastCrossedLayoutFieldOffset" src`: only `OrderingDeclarationUtilities.ChangesMemberOrder(…, Func<int,int>)`, reached by `TypeMemberOrderingCodeFixProviderBase.IsMoveSafe` (RH7102, RH7103, RH7109) and `RegionBlockLayout.ChangesMemberOrder` (RH7309). The RH7309 provider's "in front of the first block" (`:220`) is SHARED-ORDER-011's prefix for `#pragma warning`/`#nullable`, a different decision | one owner |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `NoCodeFixWhenAGroupBetweenTwoReorderedRunsPrecedesAReorderOfInstanceFieldsThatOpensAStruct` | finding #1's counterexample is refused | an action registered (the 5a0472e result) | `VerifyNoCodeFixIsOffered` (diagnostics as marked, no action) | adequate; red before the repair (`16`) |
| `NoCodeFixWhenAGroupOfMembersBetweenTwoReorderedRunsPrecedesAReorderOfInstanceFields` | RH7309-005 example verbatim, positional rule regardless of group content | an action registered | `VerifyNoCodeFixIsOffered` | adequate; red before the repair |
| `FixReordersInstanceFieldsOfTheFirstRunInFrontOfAGroupBetweenTwoReorderedRuns` | a group behind every instance field of the spans keeps the fix; distinguishes the chosen anchor from the end of the spans | no fix, or other text | `Verify` (exact fixed and batch state) | adequate |
| `FixReordersTheBlocksWhenAGroupBetweenTwoReorderedRunsPrecedesAnInstanceFieldWhoseOrderStays` | the clause needs a reorder of instance fields (FC-003 other side) | no fix, or other text | `Verify` | adequate |

No CRLF arm in the four tests; the decision reads directive positions only, and the six fixtures above ran LF and CRLF with equal results.

## Failure classes dismissed
Byte-identical to `14-auditor.md` (#1668 section) for every class the repair does not move: FC-005, FC-021 (no comment guard or join); FC-006, FC-018, FC-022 to FC-029, FC-036 (text moves only); FC-007, FC-009 (mechanics unchanged; both `Verify` tests run iterative and batch); FC-010 to FC-014, FC-028, FC-030, FC-032 (no formatter surface); FC-034, FC-035, FC-037 (fixed outputs are `main`'s moves of the same input); FC-038 to FC-043 (no shape declared, decision 2); FC-044 (no branch read, no build modelled). Re-traced for the repair: FC-001/FC-002 (guard scope above), FC-003 (both sides: group in front of / behind the last crossed field; field reordered / kept), FC-015 (one owner), FC-016 (all four fixes share the call; member moves equal to before), FC-017 (table), FC-019/FC-020 (field, field-like event, auto-property, `field`-keyword property, computed property all go through `IsLayoutField`), FC-031 (#2), FC-033 (LF and CRLF fixtures).

## Architecture findings
_None._

## Hints
- `overrefuse` shows the anchor counts a crossed instance field whose own order stays (`_d` in `overrefuse`) whenever some other instance fields are reordered. That is what the spec states and what 1668-2 A covers (`_d` is in a moved block); it withholds a fix only in a rare RH7309 shape, so no narrowing is suggested.

## Follow-up candidates
_None._
