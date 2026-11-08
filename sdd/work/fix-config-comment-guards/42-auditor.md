Role: auditor
Commit: df9c9147
Verdict: #1595: pass; #1593: findings; #1616: pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict. It named the work: audit 5, a full audit of `git diff origin/main...HEAD` after the maintainer's redirect (batches 3–6, decisions 32–37), with decision 34 as the constraint on the guard's size and kind, and the new `AGENTS.md` rule *Special cases do not grow the source*.

State verified:

- The checkout is `df9c9147`.
- `origin/main` is `3578f98e85661df7824399a6ce0514afff415a2a`, and it is an ancestor of the head.
- `scripts/lane-check.sh behavior-change`: confirmed.
- The diff matches the handovers. Production code changed in `OrderingDeclarationUtilities.cs` (+357/−7), `ModifierOrderingUtilities.cs`, `TypeMemberOrderingCodeFixProviderBase.cs`, `ModifierOrderingCodeFixProviderBase.cs`, the RH2001, RH7103, and RH7109 code fixes, and the RH7102 and RH7103 analyzers. `OrderedMember.cs` and `ConditionalGroup.cs` are gone. The tests only add to the five test classes: no test or assertion that exists on `main` is removed or changed.

Method:

- Static tracing of every new guard condition against SHARED-ORDER-007/-008, SHARED-FIX-009, and the failing members of audits 1–4.
- `scripts/apply-fix.sh` on the head under LF and CRLF, with no symbol defined. Fixtures lived in the scratchpad.
- One mutation run on a temporary worktree of the head: the four conditions of #15 relaxed together, then the RH7102, RH7103, RH7109, and RH7309 test classes run.
- One `dotnet build` with `LangVersion` 14, to check that a C# 14 property shape compiles.

The worktree and all fixtures were removed afterwards. Where I give `main`'s outcome by trace, it rests on this: the head's member-order guard only adds refusals to `main`'s reading, and adds `true` results to `HasBackingField`. So an input that the head fixes, `main` fixes too.

## thoenissen/Reihitsu#1595

### Scope
Full audit. Head `df9c9147`, base `3578f98e`. No RH7102-specific production code changed after audit 1. The RH7102 fix reaches SHARED-ORDER-008 only through `TypeMemberOrderingCodeFixProviderBase.IsMoveSafe` → `OrderingDeclarationUtilities.ChangesMemberOrder`, which is audited under #1593.

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| — | — | — | — | — | — | No finding of its own. The shared guard's findings are under #1593. | — |

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Base-class guard for every member-moving fix (decision 4) | `IsMoveSafe`: `MoveRangeContainsDirectives`, then `ChangesMemberOrder` | RH7103 and RH7109 no longer override (`private static`) | One call path for RH7102, RH7103, and RH7109 | holds |
| RH7102 analyzer: a constant whose modifiers a conditional directive splits is not reported (decision 16) | From the first modifier to the token behind the last one (`HasModifiersSplitByConditionalDirective`) | RH7103 analyzer, same helper; SHARED-DIAG-008 | Same span, same helper | holds, unchanged since audit 1 |

### Policy ownership
_N/A — unchanged since audit 1: one owner of the member-order guard (see #1593)._

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `NoCodeFixWhenTheMoveCrossesADeclarationThatIsConstOnlyInSomeConfigurations`, `NoCodeFixWhenACrossedFieldIsStaticOnlyInAGroupInFrontOfItsFirstToken` (retagged SHARED-ORDER-008) | Reported, no action | An action | `VerifyNoCodeFixIsOffered` | adequate |
| `NoCodeFixWhenAnInitializedFieldOfAnIfFalseBlockInFrontOfTheConstantWouldCrossAnInitializedField` | An excluded whole member counts, `#if false` included (SHARED-ORDER-007) | An action | `VerifyNoCodeFixIsOffered` | adequate |

### Failure classes dismissed
See *Whole run*.

### Architecture findings
_None of its own._

### Hints
_None._

## thoenissen/Reihitsu#1593

### Scope
Full audit of the member-order guard as decisions 32–35 rebuilt it: `main`'s whole-member reading, the SHARED-ORDER-008 gate, and the SHARED-ORDER-007 count of bodiless conditional accessors.

### Decision 34: size and kind of the guard
Measured on `OrderingDeclarationUtilities.cs` against `main`: +357/−7 lines, of which 227 added lines are code (not blank and not comments).

| Part | Code lines |
|---|---|
| `GetReorderedSpan` | 24 |
| gate in `ChangesMemberOrder` | 14 |
| `WrapsWholeUnits` | 30 |
| `GetBoundaryLists` | 57 |
| `ReadsAsWholeUnits` | 29 |
| `IsUnitListOfTypeBody` | 10 |
| `GetExcludedText` | 8 |
| `ForbidsFieldKeyword` | 6 |
| `IsConditionalDirective` | 4 |
| `ParseInTypeBody` | 4 |
| `HasConditionalAccessorWithoutBody` plus its hook in `HasBackingField` | about 29 |
| decision 16's `HasModifiersSplitByConditionalDirective` | 10 |

Kind, checked against decision 34 as decision 35 amended it:

- No condition is evaluated. No `BranchTaken`, `IsActive`, symbol, or `WithPreprocessorSymbols` use.
- No branch is combined: each excluded branch is parsed alone.
- There is no per-configuration parse, no configuration limit, and no new type; `OrderedMember` and `ConditionalGroup` are deleted.
- Directive places are decided from Roslyn syntax positions.
- Excluded text is read through the one wrapper `ParseInTypeBody`, with one prefix and suffix per unit kind. This is decision 35.

Every part of the gate serves a class that a maintainer answer keeps:

- whole members: batch 3 A;
- nested type: batch 5 1A;
- attribute lists: batch 5 2A;
- accessors: batch 5 3A;
- statements and switch sections: batch 4 A and decision 33.

`GetReorderedSpan` exists so that groups outside the moved and crossed spans keep `main`'s fixes (SHARED-ORDER-008). The one part outside decision 34's "structural check" is the second reader of excluded accessor text, `HasConditionalAccessorWithoutBody`. That part is finding #14.

### Previous findings of the run (classes the new rule must close)
The guard no longer reads a member in parts. Every directive that is not at a boundary of one unit list refuses the move. Every excluded branch must parse on its own as whole units. A branch that opens or closes a brace it does not close or open gives an error or a second top-level member in `ReadsAsWholeUnits`, and refuses. I traced each earlier class against the code and found the test that pins it:

| Audit finding | Members | Head |
|---|---|---|
| #1, #3 | a1, a2, x9, x4: a split in front of or behind the first token, or in the previous member's span | refused by place, or `static` is not a whole unit (`ReadsAsWholeUnits`: the attribute-list wrapper requires `Modifiers.Count == 0`). Tests: the "refused by place" set of `36-test-implementer.md` and `NoCodeFixWhenAnExcludedBranchInFrontOfTheMovedFieldHoldsAModifier` |
| #2, #4 | `=` and `static` scans of excluded text | gone. Whole attribute lists and arguments are kept or refused by place |
| #6 | b2, b1s, arr1, blk1, nt1 | refused: brackets and expression bodies are no boundary, and a branch that closes a block is not one method in the statement wrapper |
| #7 | j1s, j2s | `main`'s reading: both whole members count; fixed |
| #8, #12 | the configuration limit | removed |
| #9 | tc1, nt2 | refused: the excluded text parses as two types, or with an error |
| #10 | sg1, sg1i, cr1 | refused: a split member |
| #11 | acc1 | counted (SHARED-ORDER-007); no fix |

No earlier class recurs. The layout shapes under *Hints* are not a member of #9: see there.

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 14 | medium | `src/Reihitsu.Core/OrderingDeclarationUtilities.cs:532-536` (`HasBackingField`: a backing field only when *all* accessors lack a body), `:538-542` and `:875-900` (`HasConditionalAccessorWithoutBody`: *any* accessor without a body inside a conditional group, active or excluded) | FC-015, FC-016; check 11 (cost of special cases) | – | PR-introduced | **The run states the rule "an accessor without a body gives the property a backing field" a second time, only for conditional accessors, and the two statements disagree on the same C# 14 shape.** `get => 0; set;` compiles in C# 14 (verified with `LangVersion` 14) and gives `Value` a backing field. The head disagrees with itself on it. With `set;` inside `#if !DEBUG` … `#endif` (fixture m4, RH7309, struct `S`, `IComparable` region holding `Value`, then a `Fields` region holding `_x`), the head counts the backing field and offers no fix. Written without the directive, it counts none: RH7309 **fixed** under LF and CRLF, moving the `Fields` block in front of `Value`, which reorders the struct's layout in every build. That output is the same on `main`, because the all-bodiless branch is `main`'s. The second statement also re-reads excluded accessor text that `WrapsWholeUnits` already parses for the same group. It has its own group-span computation, and it runs for every property of the type, also outside the spans. It is about 29 lines that serve only a rare class: a property of a layout type whose accessor list mixes accessors with and without bodies across branches | State the rule once. Either: (a) a property that is not abstract, not extern, and not a partial defining declaration has a backing field when *any* accessor of its list lacks a body and an expression body, counting the excluded accessor units that `WrapsWholeUnits` parses. This closes the plain shape too, as an adjacent SHARED-FIX-003 fix, and drops the group spans. Or: (b) refuse the rare class (`AGENTS.md`, *Special cases do not grow the source*). An accessor group in which some branch holds an accessor without a body, while an accessor of the list has one, refuses the move like `field`. Delete `HasConditionalAccessorWithoutBody`, and leave the plain shape as a follow-up |
| 15 | medium | `OrderingDeclarationUtilities.cs:678-679` (a group inside no branch of another group), `:693` (the `field` check of the active branch), `:821-822` (the `AccessorListSyntax` arm of `ForbidsFieldKeyword`), `:897` (the active accessors of `HasConditionalAccessorWithoutBody`); the RH7102, RH7103, RH7109, and RH7309 test classes | FC-017, FC-003 | #11 (members M3, M4) | new mechanism | **Four guard conditions have no test.** I relaxed all four together on a worktree of the head: M1 dropped the enclosing-group count, M2 dropped the active-branch `field` check, M3 dropped the accessor-list arm, and M4 dropped the active-accessor path. All 410 tests of the RH7102, RH7103, RH7109, and RH7309 classes stayed green. Each relaxation is live. These inputs are `no-fix-offered` on the head (LF and CRLF), and by trace each mutant offers the fix. **M1**, RH7109: `#if !FEATURE` / `private readonly int _z;` / `private int _a;` / `#if DEBUG` / `private int _x;` / `#endif` / `private readonly int _c;` / `#endif`. The outer group lies outside both spans, and SHARED-ORDER-008 "No nesting" demands a refusal. **M2**, RH7103 struct: `get { #if !TRACE return field; #else return 0; #endif }`, crossed by `public static int Shared => 1;`. SHARED-ORDER-008 "No `field`" says "active or excluded". **M3**, RH7103 struct: `get => 0; #if DEBUG set => field = value; #endif`. In an RH7309 or RH7109 layout reorder, the mutant counts no backing field and reorders the `DEBUG` build's layout. **M4**, the RH7309 fixture m4 of #14: the mutant reorders the layout of the active build. M3 and M4 are the decision of #11, the conditional accessor's backing field, between the same guard and test classes. M1 and M2 are new members | Add M1 (RH7109), M2 and M3 (RH7103), and M4 plus an RH7309 variant of M3 as `VerifyNoCodeFixIsOffered` tests, tagged SHARED-ORDER-008 or SHARED-ORDER-007. If #14 takes option (b), M4 moves to the refusal and the `HasConditionalAccessorWithoutBody` row goes away |
| 16 | low | `OrderingDeclarationUtilities.cs:290-299` and `:310-321` (summaries of the public `ChangesMemberOrder` overloads); `src/Reihitsu.Analyzer.CodeFixes/Core/RegionBlockLayout.cs:60-68` | FC-031 | – | PR-introduced | The public overloads now also return `true` when the reordered span holds a directive of a group that does not wrap whole units. Their summaries still describe only initializer order and layout. The private overload's summary (`:394-399`) states the gate, and the public ones do not | Add the SHARED-ORDER-008 sentence to both public summaries. Comment only |

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Which groups SHARED-ORDER-008 judges | Directives whose `SpanStart` lies in `GetReorderedSpan`. For RH7102, RH7103, and RH7109 that is `[target.FullSpan.Start, moved.FullSpan.End)`. For RH7309 it is the first to the last block whose position changes, with the gaps between them | SHARED-ORDER-003's moved and crossed spans; `MoveRangeContainsDirectives` | Same bounds. A group with any directive in the span is judged whole, all its directives included | holds |
| Place: every directive at a boundary of one unit list | Each directive's `ParentTrivia.Token`: its first-token ancestors, the closing braces of the type, accessor list, block, and switch, the token behind the last attribute list, and the previous switch section | SHARED-ORDER-008 *Place*, four lists | The lists agree. A conditional switch section refuses: its two directives map to different sections, which is the safe side and is not demanded either way. An embedded statement without a block (`if (x)` + group) refuses | holds |
| No nesting | Conditional directives of the whole document between and in front of the group | SHARED-ORDER-008 *No nesting* | Agrees; untested half: #15 M1 | holds (test gap #15) |
| Whole units in every branch | The active branch, implied by the place condition. Each excluded branch is the `DisabledTextTrivia` right after its directive, parsed in one wrapper per kind and required to give exactly one class, with no error, and one member of the kind's shape | SHARED-ORDER-008 *Whole units*; `main`'s `GetMembersOfDisabledText` for type-body branches | Agrees. The 008 check is stricter than `main`'s count (error-free, one type), so the gate refuses before the count. Constructed probes refuse: a branch closing a block, a type, or an accessor list; `/*`; `[A] static`; `[A]` in front of a type's `}` | holds |
| `field` clause | Tokens with text `field` in the active branch and in each parsed excluded branch, for accessor lists and statement lists under an accessor | SHARED-ORDER-008 *No `field`* | Agrees. An identifier named `field` also refuses (safe side, `37-implementer.md`); `@field` does not, which is correct | holds (test gaps #15 M2, M3) |
| Backing field of a property (layout count) | All active accessors bodiless, or any bodiless accessor inside a conditional group (active or excluded), or a `field` keyword token | SHARED-ORDER-007; C# 14 accessor rules | **Disagree** for an unconditional mixed list (#14) | finding #14 |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Member-order guard (SHARED-ORDER-007/-008) | `rg -n "ChangesMemberOrder\(" src -g '!*Test*'`: `OrderingDeclarationUtilities` (owner); callers `TypeMemberOrderingCodeFixProviderBase.IsMoveSafe` and `RegionBlockLayout.ChangesMemberOrder` | one owner |
| Parse of excluded type-body text | `rg -n 'class C\\n' src/Reihitsu.Core`: `ParseInTypeBody` only, used by `GetMembersOfDisabledText`, `ReadsAsWholeUnits`, and `HasConditionalAccessorWithoutBody` | one wrapper (decision 35) |
| "An accessor without a body gives a backing field" | `HasBackingField:532-536` (all bodiless) and `HasConditionalAccessorWithoutBody` (any bodiless, conditional only) | **two statements that disagree**: #14 |
| Conditional-directive kind check | `OrderingDeclarationUtilities.IsConditionalDirective` (on `DirectiveTriviaSyntax`); `SyntaxTriviaUtilities.IsConditionalDirective` (private, on trivia); inline in `RH7004…CodeFixProvider:130` | Parity: all three test the same four kinds. The new copy is justified by its input type and because the existing one is private (`37-implementer.md`). Acceptable |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| The new and retagged SHARED-ORDER-008 refusals (initializer split, `K`, excluded `static`, nested group in the span, `X`/`Y`, tc1, nt2, conditional argument, lambda initializer, `#if false` prose, expression body, collection initializer, `field` in an accessor body ×2) | Reported, no action | An action | `VerifyNoCodeFixIsOffered` | adequate. Each was red against the per-configuration code or against `main` (`36-test-implementer.md`) |
| Kept moves (whole members in moved trivia, nested type, attribute lists ×2, accessors, switch section, comment-only and `#region` branches, nine attribute lists, nine groups) | Exact output, convergence, Fix All | Any byte, a second iteration | `Verify(source, fixed)` | adequate |
| acc1 and its all-bodied counterpart (SHARED-ORDER-007 count) | No fix with an excluded bodiless accessor; a fix when every branch has a body | An action, or no action | `VerifyNoCodeFixIsOffered` / `Verify` | adequate for the excluded path; the active path and the guard halves are #15 |

### Failure classes dismissed
See *Whole run*.

### Architecture findings
#14 is one rule stated in two places: `HasBackingField`'s all-bodiless test and `HasConditionalAccessorWithoutBody`'s any-bodiless test. The two already disagree on `get => 0; set;`. The run introduced the second statement. The failing member written without a directive is pre-existing on `main`. The lead decides under `squad/README.md`, *Architecture findings*: option (b) of #14 refuses the class, and option (a) states the rule once.

### Hints
- **Layout-ness is decided from the analyzed build's symbol (pre-existing; this verifies the follow-up candidate of `30-spec-author.md`).** Two shapes, both RH7109 **fixed** on the head under LF and CRLF, and therefore on `main`:
  - `#if X [StructLayout(LayoutKind.Sequential)] #endif` on `class C { private int _a; private readonly int _c; }`. In `X`, the fix reorders a sequential layout.
  - A group outside both spans, in front of the target: `public class C {` / `#if X` / `}` / `public struct D {` / `#endif` / `private readonly int _y;` / `private int _a;` / `private readonly int _c;` / `}`. In `X` the three fields are `D`'s, and the fix reorders a struct's layout.

  This is not a repeat of #9. All span members stay together in one type in every build, so initializer order holds. What fails is `FieldOrderSetsLayout`, read from the symbol, which is a different decision. SHARED-ORDER-008 says that "a conditional directive outside both spans neither refuses the move nor is read". Follow-up candidate, both shapes.
- **RH2001 breaks another build when a conditional group in the modifier list holds `extern` or `readonly` (pre-existing; this verifies the `34-skeptic.md` candidate).** `private` / `#if DEBUG` / `extern` / `#endif` / `int Value { get; }` gives `private #if DEBUG extern #endif readonly int _value;`. The `DEBUG` build then declares an extern field (CS0106). In struct `S`, the same with `readonly` gives a duplicate `readonly` (CS1004). Both are fixed on the head under LF and CRLF. `main` writes `readonly` in front of the group, and the result is the same. RH2001-009 leaves this place uncovered, and SHARED-FIX-009 leaves RH2001 to SHARED-FIX-003. Follow-up candidate.
- **Cost (not a hot path).** `GetReorderedSpan` calls the rank function once per character of the type body and allocates a list of that length. For RH7309 each call is O(blocks). `WrapsWholeUnits` enumerates every trivia of the document once per judged group. This runs only at code-fix registration. The member overload already knows its two spans, and `RegionBlockLayout` knows its blocks, so both could hand over the span directly.

## thoenissen/Reihitsu#1616

### Scope
`ModifierOrderingUtilities`, `ModifierOrderingCodeFixProviderBase`, and the RH2001 fix are unchanged since audit 1. That audit's result stands. This range changes only the decision 36 test, from a count to a pinned output, plus RH2001-009 and the change doc.

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| — | — | — | — | — | — | _None._ | — |

### Guard scope
_N/A — unchanged since audit 1 (`09-auditor.md`)._

### Policy ownership
_N/A — unchanged: `TryWriteModifiersInOrder` is the one writer of reordered modifiers._

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `VerifyFixIsOfferedForAConditionalModifierBetweenTheModifiersAndTheType` | `readonly` stands behind the group, the group stays in place, the use is renamed (RH2001-009 example) | Any byte of the output | `Verify(source, fixed)` (iterations and Fix All) | adequate. Stronger than the earlier count; watched red against `main`'s placement (`39-test-implementer.md`) |

### Failure classes dismissed
See *Whole run*.

### Architecture findings
_None._

### Hints
The RH2001 `extern`/`readonly` shape under #1593's *Hints* belongs to this rule's fix. It is listed there with the other build-configuration candidates.

## Whole run

### Visible output is decided
Every expected value that the diff adds is covered by one of the following:

- **RH7102, RH7103, RH7109, and RH7309 withholdings that `main` does not withhold:** batch 3 A, recorded in the change doc `member-move-conditional-groups.md` *Visible output changes* and *Decisions*. This covers the initializer split, `K`, the excluded `static`, `X`/`Y`, the nested group, the conditional argument, the lambda initializer, `#if false` prose, the expression body, and the collection initializer.
- **Withholdings that are SHARED-FIX-003 bug fixes:** tc1, nt2, the RH7309 `field` region, and acc1. These are demanded by the unchanged SHARED-FIX-003.
- **Kept moves:** `main`'s output, or the spec's output under batches 4 A and 5 1A/2A/3A. The nine-list and nine-group tests are `main`'s output.
- **RH2001 outputs:** batch 1 Q1/Q2 A, batch 2 A, and decision 36 under the standing answer. All are in the change doc `rh2001-modifier-reorder-trivia.md`.
- **"Not reported" tests:** the unchanged SHARED-DIAG-008 and RH7102-001 (decision 16).

No test that exists on `main` changed. No rule page changed.

### Scope
The edits stay within the components that decisions 4, 7, 16, and 32–36 name. The added source carries no `TODO`, no tracker number, and no run vocabulary: `git diff origin/main...HEAD -- src | rg "^\+" | rg "TODO|#[0-9]{3,4}\b"` finds nothing. `ChangesInitializerExecutionOrder` (internal, `main`'s) is unchanged.

### Comments (FC-031)
These summaries describe the new code:

- `IsMoveSafe`, and the private `ChangesMemberOrder(…, Func<int,int>)`;
- `HasBackingField`, `GetReorderedSpan`, `WrapsWholeUnits`, `GetExcludedText`, `GetBoundaryLists`, `IsUnitListOfTypeBody`, `ForbidsFieldKeyword`, `ReadsAsWholeUnits`, `HasConditionalAccessorWithoutBody`, and `ParseInTypeBody`;
- the RH2001 remarks.

The public overloads are #16.

### Failure classes dismissed
- **FC-002:** the qualifiers "whole", "no nesting", "active or excluded", and "inside an accessor's body" are explicit conditions. The untested ones are #15.
- **FC-004, FC-005, FC-019, FC-020, FC-021:** the guard edits nothing. Directives are the subject of every new test. Comment-only and `#region` branches are covered. RH2001's trivia handling is unchanged since audit 1.
- **FC-006, FC-024:** `FieldOrderSetsLayout` is unchanged and reads every partial part through the symbol. Its build-dependence is a pre-existing hint.
- **FC-007, FC-008, FC-009:** the action path is unchanged. Fix All decides each diagnostic on the original document, and the fix tests iterate.
- **FC-010 to FC-014, FC-028, FC-030:** no formatter phase is changed. The member-moving fixes format through `ReihitsuFormatter`, as on `main`. Whole-type formatting is decision 15's follow-up.
- **FC-018, FC-023, FC-025, FC-026, FC-029, FC-036, FC-037:** no rename, identifier comparison, expression-bodied kind, token re-resolution, key/value layout, or text-only claim is involved. `field` is matched by token text, with the effects noted under *Guard scope*.
- **FC-022:** `field`, braces, or `#if` inside a literal or comment of an excluded branch are one token or trivia in its parse.
- **FC-027:** the nested type, lambda, local function, switch section, embedded statement, and expression body contexts are rows of *Guard scope*.
- **FC-032:** no compiled change followed the cleaner's runs (`40-cleaner.md`). The build on `df9c9147` is green.
- **FC-033, FC-034, FC-035:** every new test has LF and CRLF rows. The spec examples are verbatim, and the expected values were taken from `main` or from the spec (`36-test-implementer.md`).
- **FC-001, FC-003, FC-015, FC-016, FC-017, FC-031:** these are the findings and tables above.

### Hints
_None beyond #1593's._
