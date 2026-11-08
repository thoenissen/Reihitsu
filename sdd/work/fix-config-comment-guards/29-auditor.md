Role: auditor
Commit: 439f105
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict. It named the audit (audit 4: decisions 24 to 26, diff `45c37c5..HEAD`), the earlier audits (`09`, `15`, `22`), and the handovers `23` to `28`.

State verified:

- The checkout is `439f105`.
- `origin/main` is `57acf0d`, and it is an ancestor of the head.
- The diff matches the handovers. The production change is in `OrderingDeclarationUtilities.cs`, `ConditionalGroup.cs`, and `OrderedMember.cs`. The test change is 20 new or re-tagged tests in the RH7109 class, and the spec change is in SHARED-ORDER-007 (`ordering-move-safety.md`).
- `scripts/lane-check.sh behavior-change`: confirmed.

Fixtures ran with `scripts/apply-fix.sh` under LF and CRLF, with no symbol defined. Each ran from a scratch directory outside the repository, on the head and on a temporary worktree at `57acf0d` ("main"). One mutation ran on a temporary worktree of the head. The worktrees and the fixtures were removed afterwards.

## thoenissen/Reihitsu#1595

### Scope
Structural delta against `45c37c5` (audit 3). Head `439f105`, base `57acf0d`.

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| — | — | — | — | — | — | No finding of its own. RH7102 reaches #9 and #10 only through `TypeMemberOrderingCodeFixProviderBase.IsMoveSafe` → `OrderingDeclarationUtilities.ChangesMemberOrder`. They are reported once, under #1593. | — |

### Guard scope
_N/A — no RH7102-specific code changed in this range; see #1593._

### Policy ownership
_N/A — unchanged: one owner of the member-order guard (see #1593)._

### Assertion adequacy
_N/A — no RH7102 test added in this range._

### Failure classes dismissed
See *Whole run*.

### Architecture findings
_None of its own; see #1593._

### Hints
_None._

## thoenissen/Reihitsu#1593

### Scope
Structural delta against `45c37c5`. Head `439f105`, base `57acf0d`.

### Previous findings
- **#6: closed.** Every member it listed is withheld and has a passing test: b2, b1s, blk1, arr1, and nt1. Its counterpart b2c holds. The class is not closed: members that #6 did not name fail, and they are reported as #9.
- **#7: closed.** j1s and j2s are fixed with main's output and have tests. Their counterparts j1n, j1w, and j2w hold. The class is not closed: members that #7 did not name fail, and they are reported as #10.
- **#8: closed.** far9 is fixed (`FixMovesAFieldWhenConditionalGroupsThatExceedTheConfigurationLimitLieAwayFromTheMove`). The over-cap outcome is decision 25, covered by the maintainer's standing answer. The spec wording of the limit is #12.

### Is the class closed?
**Neither class is closed.**

Decision 24 made the per-configuration parse the only reader of a group's context. Decision 26 made each configuration be judged on its own. Both hold within what they reach: no group is predicted to be text from the active tree, and no member is joined by name. Two inputs of the reading are still synthesized, not read:

- **The extent of a region (#9; the direction of #1, #3, and #6).** A region is a group extended over the members it overlaps *in the active tree*. Each configuration of a region is parsed as a self-contained slice of the type's body. An excluded branch that closes the type's own body, or opens a nested type around active members behind the region, changes the context of text outside the region. Members are then lost, and the moved member itself changes its type in that build.
- **What a configuration is (#10; the direction of #2, #4, and #7).** A configuration is a choice of one branch per group, and side-by-side groups, as well as different regions, are combined as independent. A split member whose `static` comes from one group and whose initializer comes from a group with the negated (or the same) condition counts in a combination that no build compiles. Fixes that main offers are withheld where no build changes.

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 9 | high | `src/Reihitsu.Core/OrderingDeclarationUtilities.cs:569-610` (`GetConditionalRegions`: region extent from active-tree member overlap), `:423-426` (every member outside a region counts as a member of the type in every configuration), `:1124-1153` (`TryGetOrderedMembersOfRegion`: each configuration's slice is wrapped in `class C { … }`, and only `FirstOrDefault()` of its types is read) | FC-001, FC-016, FC-020, FC-027 | #6 | same requirement | **An excluded branch that closes the type's own body, or opens a nested type around active members behind the region, is read as if the region were self-contained.** The members that the branch puts into another type are lost. The moved member, which lies outside the region, is assumed to stay in the type in every configuration. **Failing members** (head and main `fixed`, LF and CRLF; listings below): **tc1**, where an excluded branch closes `C` and opens `D`. In `DEBUG` the fix moves `_c` from `D` into `C`, across `_z = Log("z")`. SHARED-ORDER-007 refuses this, because `_z` is a member of excluded code in the crossed span with a static initializer. The `DEBUG` build also no longer compiles: `Log` is `D`'s (CS0103). **nt2**, where an excluded branch opens a nested type `N` around `_d`, `_c`, and `Log`, and a second group in the closing brace's trivia, another region, closes it. In `DEBUG` the fix moves `_c` out of `N` into `C`, against SHARED-ORDER-001 ("Members … of a nested type MUST NOT be involved"). The `DEBUG` build no longer compiles: `Log` is `N`'s (CS0103). **Members that hold:** nt1, blk1, and arr1 (the bracket or nested type closes and reopens inside one region); nt3 (the same shape as nt2, with a static initialized `_z` inside the nested type: no fix on head and main, because the non-`DEBUG` reading crosses `_z`). Both inputs are valid C# in every build. A caller reaches them: the RH7109 code fix on a document | In each configuration's parse, require the slice to be self-contained: the wrapper type is the only top-level member, and its closing brace is the appended one and not missing. Otherwise count the region as changed, as an unread region is (`IsTouched`), or refuse. The structural alternative is in *Architecture findings*. Add tc1 and nt2 as `VerifyNoCodeFixIsOffered` tests, with nt3 as a counterpart |
| 10 | high | `src/Reihitsu.Core/OrderingDeclarationUtilities.cs:789-806` (`TryGetConfigurations`: side-by-side groups combined as independent), `:474-533` (`ChangesOrder`: different regions combined in any configuration); `sdd/specs/shared/ordering-move-safety.md` SHARED-ORDER-007 ("The fix may also read a combination of branches that no definition of the symbols selects … can only refuse a move") | FC-001 (inverse: wider than the decision), FC-016 | #7 | PR-introduced; undecided output | **A combination of branches that no build compiles gives a split member a context and an initializer that no configuration gives it.** Correct fixes that main offers are then withheld. **Failing members** (main `fixed`, head `no-fix-offered`, LF and CRLF; listings below): **sg1**, `private` / `#if DEBUG static #endif` / `int _b` / `#if !DEBUG = Log("b") #endif` / `;`, crossed by a static initialized `_c`. `DEBUG` compiles `static int _b;` and the other build compiles `int _b = Log("b");`. The combination of both branches reads `static int _b = Log("b")`. **sg1i**, the same with `#if DEBUG` in both groups (two groups with *one* condition read as independent), crossed by an instance initialized `_c`. **cr1**, where the crossed `_x` is `static` only without `DEBUG` and the moved `_c` is `static` only with `DEBUG`, each split by its own group. These are two regions, which `ChangesOrder` pairs in any combination. In every case, main's output is correct in both builds: no build moves two initializers of one context. **Members that hold:** sg1t (the same shape with `#if DEBUG` and `#if TRACE`: the `DEBUG`+`TRACE` build crosses a static initializer, so the head's withholding is correct, and main is wrong there); sg2 (two complete declarations in side-by-side groups: fixed on head and main); j1s, j2s, and j1n; sg3 (an excluded static initialized `_x` beside a moved member that is `static` only without `DEBUG`: withheld on head and on main alike, so nothing changes); cr2 (the head is correct to withhold). `25-spec-author.md` rejected the alternative of only real combinations "because it would … add no fix main offers". sg1, sg1i, and cr1 show that it would. Decision 26's premise, "withholds exactly where some build changes; offers only fixes main offers", fails for them. The spec's own *Why* ("combining an initializer of one build with a `static` of another would describe a member that no build compiles and withhold moves that no build changes") contradicts the "may also read" sentence. No test asserts these outputs, and no maintainer answer covers them | Read a configuration as an assignment of the preprocessor symbols that the conditions name, and let the parser evaluate every directive (`CSharpParseOptions.WithPreprocessorSymbols`). Do this jointly for every region that shares a symbol, or for the whole body. Then only combinations that a definition of the symbols selects are judged. Drop the spec's "may also read" sentence, or, if the lead keeps the over-approximation, ask its outputs as an output question (sg1 as the example). Add sg1, sg1i, and cr1 as `VerifyCodeFixIsOffered` tests (decision 15's continuation shapes rule out exact text), with sg1t as the `VerifyNoCodeFixIsOffered` counterpart |
| 11 | medium | `src/Reihitsu.Core/OrderingDeclarationUtilities.cs:777-778` (`LiesInsideBrackets`: the accessor-list exemption); RH7102, RH7103, RH7109, and RH7309 test classes | FC-017, FC-003 | – | new mechanism | **No test pins the accessor-list exemption.** That exemption alone decides whether a balanced group inside an accessor list is read per configuration. Mutation: dropping `&& openBrackets.Peek().Parent is not AccessorListSyntax` leaves all 390 RH7102, RH7103, RH7109, and RH7309 tests green. The mutation is live: acc1 (RH7309, a struct region whose property chooses `get;` under `DEBUG` and `get => 0;` otherwise, in front of a `Fields` region) is `no-fix-offered` on the head. On the mutant and on main it is `fixed`, which reorders the struct's layout in `DEBUG` | Add acc1 as a `VerifyNoCodeFixIsOffered` RH7309 test (SHARED-ORDER-007, RH7309-006). Add a counterpart where both branches are bodied accessors, which keeps main's fix |
| 12 | low | `sdd/specs/shared/ordering-move-safety.md`, SHARED-ORDER-007 limit paragraph | — | – | PR-introduced | **The spec states the cap per move; the code applies it per region.** "The fix reads at most 256 configurations for a move. When the `#if` groups that declare or split a member the move moves or crosses have more, the move is refused." In cap2, two regions of five groups each lie in the crossed span: 32 configurations each and 1024 for the move. The spec demands a refusal. The head offers the fix, which is correct, because the regions are compared pairwise and not as a product. The code is right and the text is wrong | State the limit per region: a region is the groups that share a member, and a region with more than 256 configurations refuses a move that moves or crosses part of it. Text only |
| 13 | low | `src/Reihitsu.Core/OrderedMember.cs:42` | FC-031 | – | PR-introduced | `HasInstanceInitializer` still reads "in some build configuration". This range removed that phrase from the type summary and from the two sibling properties, because a value now holds for one configuration | Drop "in some build configuration". Comment only |

Fixtures (RH7109 on `_c`, which the fix moves in front of `_t`, except acc1):

```csharp
// tc1 — #9, fixed on head and main; DEBUG: _c moves from D into C, across _z = Log("z"), and Log is not found
public class C
{
    private static int _t;
#if DEBUG
}

public class D
{
    private static int _z = Log("z");
#endif
    private int _d;
    private static readonly int _c = Log("c");

    private static int Log(string name) { return 0; }
}

// nt2 — #9, fixed on head and main; DEBUG: _c moves out of N into C, and Log is not found
public class C
{
    private static int _t;
#if DEBUG
    private class N
    {
#endif
        private int _d;
        private static readonly int _c = Log("c");

        private static int Log(string name) { return 0; }
#if DEBUG
    }
#endif
}

// nt3 — counterpart, no fix on head and main: the same opening group with
//     private static int _z = Log("z");
// inside it, closed by `#if DEBUG } #endif` right behind it, then `private int _d;` and _c, Log at member level

// sg1 — #10, main fixed, head no fix (each type ends with `private static int Log(string name) { return 0; }`)
    private static int _t;
    private
#if DEBUG
        static
#endif
        int _b
#if !DEBUG
        = Log("b")
#endif
        ;
    private int _d;
    private static readonly int _c = Log("c");

// sg1i — #10, main fixed, head no fix: `private int _t;`, `#if DEBUG static #endif` and `#if DEBUG = Log("b") #endif`
// in _b as above, `private static int _d;`, `private readonly int _c = Log("c");`

// sg1t — counterpart, head no fix (correct), main fixed: sg1 with `#if TRACE` in place of `#if !DEBUG`

// cr1 — #10, main fixed, head no fix
    private int _t;
    private
#if !DEBUG
        static
#endif
        int _x = Log("x");
    private int _d;
    private
#if DEBUG
        static
#endif
        readonly int _c = Log("c");

// acc1 — #11, RH7309; head no fix, mutant and main fixed
using System;

public struct S : IDisposable
{
    #region IDisposable

    public int Handle
    {
#if DEBUG
        get;
#else
        get => 0;
#endif
    }

    public void Dispose()
    {
    }

    #endregion // IDisposable

    #region Fields

    private int _a;

    #endregion // Fields
}

// cap2 — #12, fixed on head: `private int _t;`, then five groups `#if R1A1 private int _r1a1; #endif` … R1A5 and
// `private int _m1;`, then five groups R2A1 … R2A5 and `private int _m2;`, then `private readonly int _c;`
```

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Which text a configuration of a group changes (`GetConditionalRegions`) | The group, extended over the members it overlaps in the active tree | What the group's branches change in each build: the whole type body when a branch closes or opens a type brace | Narrower: an excluded branch that closes the type body, or opens a nested type closed by another region, changes text outside the region (tc1, nt2) | **fails** (#9) |
| Which branch combinations are configurations (`TryGetConfigurations`, `ChangesOrder` across regions) | Every product of one branch per group; regions combined in any configuration | The builds a definition of the symbols selects (SHARED-FIX-003, SHARED-ORDER-007 *Why*) | Wider: it includes combinations that no definition selects (sg1, sg1i, cr1) | **fails** (#10) |
| A group stays text while it lies inside a bracket (`IsBalanced` plus the per-configuration `IsCompiledDirective`/`LiesInsideBrackets`) | Every piece between all boundaries is balanced. The `#if`'s context comes from each configuration's parse | The per-configuration parse | Agrees. A balanced group lies at one depth in every branch. A candidate is promoted once any enumerated configuration compiles it outside every bracket. A candidate nested in a candidate that stays text is inside the same bracket. Promotion only adds configurations, so it only refuses | holds |
| Accessor-list exemption (`LiesInsideBrackets`) | The innermost open bracket in front of the `#if`, unless it is an accessor list | `HasBackingField` (auto-accessors or `field`) | Agrees for accessor-list content. A group inside an accessor *body* that decides `field` stays text, as on main (follow-up candidate) | holds; untested (#11) |
| Square-only groups (`holdsOnlySquareBrackets`) | Tokens at depth 0 of every piece | — | Agrees: an attribute list, an index, a rank, or a collection expression at depth 0 writes no modifier, initializer, or member | holds |
| Scoped cap (`IsTouched`) | Every position of an unread region's span, plus the members outside it | `getNewRank` of the move | Agrees with decision 25: far9 is fixed and the over-cap test is withheld. A region within one rank that no outside member crosses is left out | holds |
| Pairs of one region (`ChangesOrder`, first loop) | Each configuration's members on their own | SHARED-ORDER-007 per-configuration paragraph | Agrees (j1s, j2s, j1w, j2w, split `_b` and its counterpart) | holds |
| Pairs of an active member and a region member (`ChangesOrder`, second loop) | Every configuration of the region against every active member | — | Exact, as long as the active member is a member of the type in every configuration. #9 is where it is not | holds, except #9 |
| A configuration's text without a complete member (`=` fallback) | Incomplete members and skipped tokens | `HasInitializerSyntax` | Over-approximates, and only in combinations that no build compiles. It is part of #10's reach | see #10 |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| What a member contributes per configuration | `OrderingDeclarationUtilities` (`GetConditionalRegions`, `GetConditionalGroups`, `IsBalanced`, `TryGetConfigurations`, `TryGetOrderedMembersOfRegion`, `GetOrderedMembersOfConfiguration`, `ChangesOrder`) with the internal `ConditionalGroup` and `OrderedMember`. `rg -n "ParseCompilationUnit\(\|ParseTokens\(" src -g '!*Test*'` outside the formatter finds `OrderingDeclarationUtilities:706,1124`, `TokenSeparationUtilities:76`, and `ExcludedCodeUtilities:119`. The last two decide something else | one owner |
| Member-order guard call | `rg -n "ChangesMemberOrder\(" src -g '!*Test*'` finds `TypeMemberOrderingCodeFixProviderBase.IsMoveSafe:79` and `RegionBlockLayout.ChangesMemberOrder:71`, both through the one owner | one owner, unchanged |
| Bracket depth | `GetNestingDelta`, `GetNestingDepths`, `ContainsTopLevelToken`, and `LiesInsideBrackets`, which now runs on the parsed configuration | one owner |
| Removed readers | `rg -n "IsReadAsText\|GetMemberName\|GetEnclosingBody\|WritesOnlyInsideBrackets" src` finds none | removed, as decision 24 asks |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| b2, b1s, blk1, arr1, nt1 (`23-test-implementer.md`) | Withheld | An action. Each was red at `45c37c5` | `VerifyNoCodeFixIsOffered` | adequate |
| j1s, j2s, j1n, far9, blk1c, arr1c, nt1c | Main's exact output | Any byte, or no action. j1s, j2s, and far9 were red at `45c37c5`; all pass on main | `Verify(source, fixed)` | adequate |
| b2c, j1w, j2w | Withheld, as on main | An action | `VerifyNoCodeFixIsOffered` | adequate |
| Split `_b` example and its counterpart (`26-test-implementer.md`) | Offered (main as well) / withheld | No action / an action | `VerifyCodeFixIsOffered` / `VerifyNoCodeFixIsOffered` | adequate for the decision. The layout is decision 15's |
| `NoCodeFixWhenABalancedExcludedBranchEndsAnExpressionBodyAndDeclaresAStaticInitializedField` and its counterpart | The promotion of a balanced group that one configuration compiles at member level: withheld / offered | Red on main (main offers the fix). The counterpart guards against over-promotion | same | adequate. Both sides are tested (FC-003) |
| Over-cap test, far9 (re-tagged) | Both sides of "the move touches the over-cap region" | An action / no action | same | adequate |
| (missing) accessor-list exemption | — | Mutation survives (#11) | — | missing (#11) |
| (missing) tc1, nt2; sg1, sg1i, cr1 | — | — | — | missing (#9, #10) |

Every new test is an LF and CRLF `DataRow` pair. Expected outputs come from main (`23` and `26` ran them against main's production code), so FC-035 does not apply.

### Failure classes dismissed
See *Whole run*.

### Architecture findings
#9 repeats the class of #6, #3, and #1, and #10 repeats the class of #7, #4, and #2. The decision is the same each time: what each build configuration compiles at the type's member level. It fails in the same component, `OrderingDeclarationUtilities`.

Decisions 21 and 24 moved the reading into a per-configuration parse, but the configurations themselves are still synthesized from the group structure:

- **The extent of a configuration's reading (#9)** is a region, taken from the active tree's member overlap. A branch that changes the type body's own braces reaches beyond it.
- **The set of configurations (#10)** is the product of branch choices, with side-by-side groups and different regions treated as independent. The builds are what the symbols select.

A reading without either synthesis reads a configuration as an assignment of the symbols that the body's conditions name. It parses the type declaration's text once per assignment with `CSharpParseOptions.WithPreprocessorSymbols`, so the parser evaluates every directive. It finds the moved and crossed positions in the type of that parse, and refuses when a position is not a member of that type, or when the cap on assignments is exceeded and the move touches a conditional part. That removes regions, independent combinations, candidate promotion, and the bracket heuristics together. It stays inside the one component.

Decision 24 says that the run narrows if audit 4 finds this class again. The lead decides between that and the reading above.

### Hints
- Cost: every move evaluates every region of the type, including every method whose body holds an `#if`, with one parse per region and configuration. Fix All repeats this per diagnostic. This is not a finding: the code fix is not an analyzer hot path, and `24-implementer.md` measured the full analyzer suite at 2 min 31 s. A symbol-assignment reading (see *Architecture findings*) would parse the type once per assignment instead.

## thoenissen/Reihitsu#1616

Nothing to audit: this range touches no RH2001 code, test, spec, change doc, or rule page. Audit 1's result stands.

## Whole run

### Visible output is decided
The new expected values in this range are covered as follows:

- b2, b1s, blk1, arr1, nt1, and the balanced expression-body test are demanded by SHARED-FIX-003 and SHARED-ORDER-007, because each build named crosses an initializer of the moved member's context.
- j1s, j2s, j1n, far9, blk1c, arr1c, nt1c, b2c, j1w, j2w, the split `_b` example and its counterpart, and the expression-body counterpart: main's outcome.
- The over-cap test is decision 25, under the standing answer recorded in `00-lead.md`.

**Not covered:** the withholdings of #10 change code-fix results from main's. No test asserts them, and no maintainer answer covers them. They are part of #10.

No rule page changed.

### Scope
The range stays within `OrderingDeclarationUtilities`, `ConditionalGroup`, `OrderedMember`, the RH7109 test class, and SHARED-ORDER-007. The added source carries no `TODO` and no tracker number (`git diff 45c37c5..HEAD -- src | rg "TODO|#[0-9]{3,4}\b"`: none). `ConditionalGroup.GetAllBoundaries` is still used, by `IsBalanced`.

### Comments (FC-031)
These summaries describe the new code:

- both private `ChangesMemberOrder` overloads and the public one;
- `ChangesOrder`, `IsTouched`, `GetConditionalRegions`, `IsBalanced`, `LiesInsideBrackets`, and both `TryGetConfigurations`;
- `TryGetOrderedMembersOfRegion`, `IsCompiledDirective`, `GetOrderedMembersOfConfiguration`, and `MaximumConfigurationCount`;
- `ConditionalGroup.IsComplete` and `GetSelfAndNestedGroups`.

The one stale summary is #13.

### Failure classes dismissed (structural delta)
Audits 1 to 3's dismissals stand for everything this range did not move. For the moved code:

- **FC-002:** the qualifiers "balanced", "square-only", "compiled at member level", and "touched" are explicit conditions.
- **FC-003:** each new condition is tested on both sides, except the accessor-list exemption (#11).
- **FC-004, FC-005, FC-019, FC-021:** the guard edits nothing. Comments in a piece are trivia to `ParseTokens`, and a configuration's parse blanks only directives and other branches, with line breaks kept.
- **FC-006, FC-018, FC-025, FC-026, FC-029, FC-036, FC-037:** no rewrite, rename, or text-only claim is involved. The meaning changes of tc1 and nt2 are #9.
- **FC-007, FC-008, FC-009:** there is no new action path. Fix All decides each diagnostic on the original document.
- **FC-010 to FC-014, FC-028:** no formatter phase is moved. The member-moving fixes format through `ReihitsuFormatter`, as before.
- **FC-022:** a bracket, `=`, or `#if` inside a literal or comment of a piece is one token or trivia.
- **FC-023:** there is no name comparison left.
- **FC-024:** a partial type is read per declaration, as on main. A branch that reopens the type as another partial declaration is a member of #9 (tc1's shape).
- **FC-027:** the nested type, block, expression-body, and accessor contexts are covered by the guard-scope rows. The type's own body is #9.
- **FC-030:** decision 15's continuation shapes are asserted without layout.
- **FC-032:** no compiled change followed the cleaner's runs (`27-cleaner.md`: build clean, 487 + 660 tests).
- **FC-033, FC-034, FC-035:** LF and CRLF rows, the auditor's fixtures verbatim, and expected outputs from main.
- **FC-001, FC-015, FC-016, FC-017, FC-020, FC-031:** these are the findings or the tables above.

### Hints
_None._
