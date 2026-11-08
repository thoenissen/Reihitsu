Role: auditor
Commit: 45c37c5
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict. It named the audit, the decisions, and the handovers to read. Inputs:

- `00-lead.md`, decisions 21 to 23.
- Audits 1 and 2: `09-auditor.md` and `15-auditor.md`.
- Handovers `16` to `21`.
- SHARED-ORDER-007.
- `git diff 3b3f696..HEAD`.

State verified:

- The checkout is `45c37c5`.
- `origin/main` is `57acf0d`, and it is an ancestor of the head.
- The diff matches the handovers. The production code is `ConditionalGroup.cs` (new) and `OrderingDeclarationUtilities.cs`. The tests are in the RH7109, RH7103, and RH7309 classes, and the spec change is in `ordering-move-safety.md`.
- `scripts/lane-check.sh behavior-change`: confirmed.

Fixtures ran with `scripts/apply-fix.sh RH7109` under LF and CRLF, with no symbol defined. Each ran from a scratch directory, and on a temporary worktree at `57acf0d` for "main". The worktree and the fixtures were removed afterwards.

## thoenissen/Reihitsu#1595

### Scope
Structural delta against `3b3f696` (audit 2). Head `45c37c5`, base `57acf0d`.

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| — | — | — | — | — | — | No finding of its own. RH7102 reaches #6, #7, and #8 only through `TypeMemberOrderingCodeFixProviderBase.IsMoveSafe` → `OrderingDeclarationUtilities.ChangesMemberOrder`. They are reported once, under #1593. | — |

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
Structural delta against `3b3f696`. Head `45c37c5`, base `57acf0d`.

### Previous findings
- **#3: closed.** Every member it listed holds and has a passing test: x9 (RH7109 and RH7103) and x4. Its counterpart x8 also holds. The class is not closed: members that #3 did not name fail, and they are reported as #6.
- **#4: closed.** Every member it listed is fixed as on main and has a passing test: x1, x1b, x3, h4, and the RH7309 layout-field member. The class is not closed: a member that #4 did not name fails, and it is reported as #7.
- **#5: closed.** h5 is pinned by `FixIsOfferedWhenAnExcludedArgumentOfACrossedFieldsInitializerIsAStaticLambda`, and `19-test-implementer.md`/`16-test-implementer.md` show by mutation that the pin bites.

### Is the class closed?
**Not as a class.**

The per-configuration parse in `TryAddOrderedMembersOfRegion` removes both directions of the class for every group that it reads. A branch is no longer lost at a piece boundary. A sibling branch is no longer guessed from a token scan.

Two decisions about a group are still made outside that parse:

- **The read-as-text gate (`IsReadAsText`).** It decides from the active tree whether any configuration of a group can contribute a member. When another configuration changes the block or bracket that encloses the group, excluded members are lost again. This is the direction of #1 and #3, reported as #6.
- **The join of configurations by member name.** It unites the contributions of distinct declarations that share a name. They then count with a context and initializer that no configuration gives either of them. This is the direction of #2 and #4, reported as #7.

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 6 | high | `src/Reihitsu.Core/OrderingDeclarationUtilities.cs:598-609` (`IsReadAsText`), `:672-705` (`LiesInsideBrackets`, active tokens only), `:714-738` (`GetEnclosingBody`, active ancestors only), `:790` (nested children read as text stay as directives that the configuration parse evaluates with the real symbols) | FC-001, FC-016, FC-020 | #3 | same requirement | **A group that the active tree places inside a block, an expression body, a nested type body, or a bracket is read as text, but another configuration puts it at member level.** Its excluded members are then lost, and the fix reorders that build's initializers. The gate inspects only the active tree: the enclosing node of the directives, and the active tokens in front of the first directive. The decision it protects is whether *any* configuration of the group contributes a member. **Failing members** (all `fixed` on head and main, LF and CRLF; listings below): b2, a child group nested in a branch that closes the active bracket (bracket rule); b1s, a group side by side with an earlier group of the same member that closes the bracket in `DEBUG` (bracket rule); blk1, an excluded branch that closes the enclosing method block, declares a static initialized field, and opens a new method (block rule); arr1, the same across an expression body (`=> Run(1` … `#endif );`); nt1, the same across a nested type's body. In each case, a build with the excluded branch (`DEBUG`, or `DEBUG` with `TRACE`) runs `Log("c")` before `Log("z")` after the fix. **Members that hold:** b2c, the same nested `TRACE` group at member level in every configuration (no fix, head and main); the pinned attribute-list and collection-initializer probes, where every configuration has the same context; and every test of `16-test-implementer.md`. The shapes are unusual: brackets or blocks are left unbalanced across branches. They are still valid C# in every build, like x9 of audit 2. Main fails them too, because it ignored excluded code inside member spans. They fail against SHARED-ORDER-007 ("a member that code excluded by an `#if` … declares counts like any other member"; "a further member that excluded code inside its declaration declares counts … at that position") | Decide the gate per configuration, or only where no configuration can change the context. For the block, expression-body, and nested-type rule, require what `WritesOnlyInsideBrackets` already requires: every piece between two boundaries is balanced. That closes blk1, arr1, and nt1. For the bracket rule, use it only when the text from the enclosing bracket's opening token to the group holds no boundary of a group that is read per configuration, including the boundaries of an enclosing group. Or check the context in each configuration's parse. Add b2, b1s, blk1, arr1, and nt1 as `VerifyNoCodeFixIsOffered` tests, with b2c as the counterpart |
| 7 | high | `src/Reihitsu.Core/OrderingDeclarationUtilities.cs:1103-1122` (join of contributions by `GetMemberName`), `:1133-1142` | FC-001 (inverse: wider than the decision), FC-016 | #4 | PR-introduced; undecided output | **Distinct declarations that share a name in mutually exclusive branches are joined.** Each one then counts with a context and an initializer that no configuration gives it. Correct fixes that main offers are withheld. The join is meant to unite the per-configuration readings of *one* split declaration, as SHARED-ORDER-007's split-member paragraph states. The key is the name, so it also unites two complete declarations. Two such declarations are two members of excluded or active code, and the same requirement counts them each on its own. **Failing members** (main `fixed`, head `no-fix-offered`, LF and CRLF): j1s, `#if DEBUG private static int _b; #else private int _b = Log("b"); #endif`, crossed by a static initialized `_c`, which reads as a static initializer; j2s, `#if DEBUG private static int _b = Log("b"); #else private int _b; #endif`, crossed by an instance initialized `_c`, which reads as an instance initializer. In each case the main output is correct in both builds. **Members that hold:** j1n, the same shape with distinct names (`_b` / `_e`), fixed on head and main. A genuinely split declaration whose initializer comes from one configuration and whose `static` comes from another is united by the spec's own wording, so it is not part of this finding (see *Hints*). `17-implementer.md` named this join as open ("can only withhold more"). No row of `00-lead.md` decides it, and no test asserts it | Identify a split declaration across configurations by a source position that every configuration shares, for example the mapped position of its identifier or of a token outside the group, rather than by its name. Or count each configuration's members as they are and drop the join. Add j1s and j2s as `Verify(source, fixed)` tests with main's output. Avoid decision 15's continuation shapes, as the listings do |
| 8 | high | `src/Reihitsu.Core/OrderingDeclarationUtilities.cs:417-420`, `:451-477`, `:747-809` (`MaximumConfigurationCount`); test `NoCodeFixWhenConditionalGroupsInFrontOfACrossedFieldExceedTheConfigurationLimit` | FC-001 (inverse) | – | undecided output; PR-introduced | **The over-cap test asserts a withdrawn fix that no requirement demands and no maintainer answer covers.** Main offers the fix, and the test was red on main. Decision 23 records the cap as an internal choice: "an input no realistic source reaches". It is a visible code-fix change, which *Visible output is decided* requires a requirement or a maintainer answer for. **Its reach is also wider than the pinned shape.** An over-cap region anywhere in the type makes `TryGetOrderedMembers` fail, so `ChangesMemberOrder` refuses every move in the type. That includes moves that neither cross nor move a member of the region. Example far9 (main `fixed`, head `no-fix-offered`, LF and CRLF): `private int _t; private readonly int _c;`, then a separate `#if FEATURE` section holding nine member-level `#if A1`…`#if A9` field groups. The fix moves `_c` in front of `_t`, and neither member has an initializer. A type section under one feature symbol with several member-level `#if` groups inside is an ordinary multi-target shape. **Holds:** far7, with seven nested groups (129 configurations): fixed on head and main | Ask the over-cap outcome as an output question. The standing answer in `00-lead.md` applies, but the question is recorded and stated in the PR. Before asking, narrow the guard so that an over-cap region refuses only the moves that cross it or move part of it. For example, count it as one member with every contribution (static and instance initializer, layout field) at the positions of its members, instead of failing the whole type. Add far9 as a counterpart that keeps main's fix |

Fixtures (each type ends with `private static int Run(int a, int b = 0) { return a + b; }` and `private static int Log(string name) { return 0; }`; the diagnostic is RH7109 on `_c`, which the fix moves in front of `_t`):

```csharp
// b2 — #6, fixed on head and main; DEBUG+TRACE runs Log("c") before Log("z") after the fix
public class C
{
    private static int _t;
    private int _a = Run(1
#if DEBUG
        );
#if TRACE
    private static int _z = Log("z");
#endif
    private int _y = Run(2
#endif
        );
    private static readonly int _c = Log("c");

// b2c — counterpart, no fix on head and main: the TRACE group is at member level in every configuration
    private static int _t;
    private int _a = Run(1);
#if DEBUG
#if TRACE
    private static int _z = Log("z");
#endif
    private int _y = Run(2);
#endif
    private int _d;
    private static readonly int _c = Log("c");

// b1s — #6, fixed on head and main
    private static int _t;
    private int _a = Run(1
#if DEBUG
        );
#else
        , 2
#endif
#if TRACE && DEBUG
    private static int _z = Log("z");
#endif
#if !DEBUG
        );
#endif
    private int _d;
    private static readonly int _c = Log("c");

// blk1 — #6, fixed on head and main; arr1 is the same with `public int P => Run(1` / `public int Q => Run(2` and `);` behind #endif;
// nt1 is the same with `private class N` / `private class M` in place of the two methods
    private static int _t;

    public void M()
    {
#if DEBUG
    }

    private static int _z = Log("z");

    public void N()
    {
#endif
    }

    private static readonly int _c = Log("c");

// j1s — #7, main fixed, head no fix
    private static int _t;
#if DEBUG
    private static int _b;
#else
    private int _b = Log("b");
#endif
    private int _d;
    private static readonly int _c = Log("c");

// j2s — #7, main fixed, head no fix
    private int _t;
#if DEBUG
    private static int _b = Log("b");
#else
    private int _b;
#endif
    private static int _d;
    private readonly int _c = Log("c");

// far9 — #8, main fixed, head no fix (no Run/Log needed)
public class C
{
    private int _t;
    private readonly int _c;

#if FEATURE
#if A1
    private int _a1;
#endif
    // … the same group for A2 to A9
    private int _f;
#endif
}
```

The spacer `_d` in b1s, j1s, and j2s keeps the `#endif` out of `_c`'s leading trivia. Otherwise `MoveRangeContainsDirectives` withholds the fix on main and head alike, and the member-order guard is not reached.

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| A group changes nothing a member of the type contributes: block, expression-body, and nested-type rule (`IsReadAsText` → `GetEnclosingBody`) | The active ancestors of each directive's token | The per-configuration parse (`TryAddOrderedMembersOfRegion`) | Disagrees when an excluded branch closes the block or body and declares members (blk1, arr1, nt1). It does not check that every piece is balanced, while the bracket rule does | **fails** (#6) |
| The same decision, bracket rule (`WritesOnlyInsideBrackets` → `LiesInsideBrackets`) | Every piece is balanced, which holds for all branches. The context is taken from the active tokens of the member in front of the first directive | The per-configuration parse | Disagrees when an enclosing or earlier group, read per configuration, changes the bracket depth in another configuration (b2, b1s) | **fails** (#6) |
| The same decision, square-bracket-only pieces (`holdsOnlySquareBrackets`) | Every piece's tokens at depth 0 | — | Agrees: square brackets alone at depth 0 can write an attribute list, an index, or a rank, but no modifier, initializer, or member | holds |
| Region extension (`GetConditionalRegions`) | Group span, extended to every member whose full span overlaps it, with overlapping regions merged | Members read from the active code (`TryGetOrderedMembers`, `Contains(member.FullSpan)`) | Agrees: directives lie in members' leading trivia or inside them, or in the closing brace's trivia, so a region's ends are member or body boundaries and no member is read twice or missed | holds |
| A split declaration is one member across configurations (`GetMemberName` join) | The member's name | SHARED-ORDER-007 split-member paragraph: one declaration that a branch splits | Wider: it also joins distinct declarations of one name (j1s, j2s) | **fails** (#7) |
| Configuration cap (`MaximumConfigurationCount`) | The whole type: any region over the cap fails `TryGetOrderedMembers` | The moved and crossed parts (`getNewRank`) | Wider: it refuses moves that touch no member of the region (far9) | **fails** (#8) |
| Groups that the body does not open or close (`GetConditionalGroups`) | Bounded by the start or the end of the body | — | Holds. Two-header shape: the configuration that compiles the excluded header nests the members, so they contribute nothing there, but the other configuration has the same members. A header-only branch lies in front of every member and cannot be crossed. Pinned by the closing-brace tests | holds |
| A configuration's text without a complete member (`ContainsTopLevelToken(=)` fallback) | Incomplete members and skipped tokens of one configuration's parse | `HasInitializerSyntax` | Reached only by a combination of independent groups that no real build compiles. I found no shape, valid in every real build, where it adds a contribution main lacks | holds (over-approximation) |
| A `const` configuration contributes nothing (decision 22) | `IsConst` per parsed member | SHARED-ORDER-007 fourth split bullet | Agrees. The `K` example and its counterpart are tested, and the mutation in `19-test-implementer.md` bites | holds |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| What a split declaration or excluded code contributes per configuration | `OrderingDeclarationUtilities` (`TryGetOrderedMembers`, `GetConditionalGroups`, `GetConditionalRegions`, `GetConfigurations`, `TryAddOrderedMembersOfRegion`) with the internal `ConditionalGroup`. `rg -n "ParseCompilationUnit\(\|ParseTokens\(" src -g '!*Test*'` outside the formatter finds `OrderingDeclarationUtilities:631,1068`, `TokenSeparationUtilities:76` (token separation), and `ExcludedCodeUtilities:119` (names in excluded code for renames). Those two decide something else | one owner |
| Reading conditional groups | `rg -l "GetRelatedDirectives\|IfDirectiveTrivia\)" src -g '!*Test*'` finds `SyntaxTriviaUtilities` (a boundary inside a span), RH8201, RH7302/RH7306 (regions), and the RH7004 fix. These are different decisions | one owner for this decision |
| Bracket depth | `GetNestingDelta`, `GetNestingDepths`, and `ContainsTopLevelToken`; the new `WritesOnlyInsideBrackets` and `LiesInsideBrackets` reuse `GetNestingDelta` | one owner |
| Member-order guard call | `rg -n "ChangesMemberOrder\(" src -g '!*Test*'` finds `TypeMemberOrderingCodeFixProviderBase.IsMoveSafe:79` and `RegionBlockLayout.ChangesMemberOrder:71` | unchanged, one owner |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| The 15 tests of `16-test-implementer.md` that were red (x9, x4, P→F `#elif`, the two P→I forms, x1, x1b, x3, the I and `#elif` all-`static` forms, the moved all-`static` form, h4, RH7103 x9, RH7309 struct) | Withheld, or main's exact output | An action, no action, or any byte (each was red before 234e32a) | `VerifyNoCodeFixIsOffered`, `Verify(source, fixed)`, `VerifyCodeFixIsOffered` | adequate |
| Their counterparts (x8, x4n, `pisn`, the "lacks static" rows, RH7309 counterpart, h5) | The opposite side of each condition (FC-003) | The opposite outcome | same | adequate |
| `VerifyCodeFixIsOffered` (h4, h5, the `K` example) | Exact diagnostics; an action registered; its result has no diagnostic | No action | The helper does not assert the result's text. This is by design, because the layout is decision 15's follow-up. It decides only whether a fix is offered, which is the decision under test | adequate for that decision |
| `NoCodeFixWhenACrossedDeclarationIsAnInstanceFieldInsteadOfAConstantInTheActiveBranch` | `_b` gets no action, while `K` is reported too | An action for `_b` | `Verify` + `GetCodeFixActionsAsync` targeted on `_b` | adequate |
| The pins of decision 23: attribute lists, collection initializer, 8 groups, closing brace (two) | Main's outcome | Each bites under the mutations in `19-test-implementer.md` | `Verify` / `VerifyNoCodeFixIsOffered` | adequate |
| `NoCodeFixWhenConditionalGroupsInFrontOfACrossedFieldExceedTheConfigurationLimit` | Over-cap: no fix | An action | `VerifyNoCodeFixIsOffered` | adequate as a characterization. Its value is undecided (#8), and no test pins that the cap leaves a move away from the region alone |
| (missing) b2, b1s, blk1, arr1, nt1 withheld; j1s, j2s fixed; far9 fixed | — | — | — | missing (#6, #7, #8) |

Every test is an LF and CRLF `DataRow` pair. The expected outputs of the "keeps the fix" tests are main's outputs: `16-test-implementer.md` and `19-test-implementer.md` ran them against main's production code.

### Failure classes dismissed
See *Whole run*.

### Architecture findings
#6 repeats the class of #3 and #1, and #7 repeats the class of #4 and #2. The decision is the same: what a declaration that a conditional group splits, or excluded code, contributes in each configuration. It fails in the same component, `OrderingDeclarationUtilities`.

Decision 21 moved the decision's main reading to the right unit, the conditional group with all its branches, parsed per configuration. Two inputs to that reading are still decided outside it:

- **The read-as-text gate (#6)** predicts every configuration's context from the active tree. A prediction of one component's result from another view is the structural cause *Architecture findings* names.
- **The name join (#7)** guesses the identity of a split declaration across configurations from its name, not from the source it shares.

Both changes stay inside the one component the run changes. #8 is not part of the class. It is a guard-scope and visible-output finding of the new cap.

### Hints
- SHARED-ORDER-007's split-member paragraph unites a split declaration's contributions attribute by attribute. A declaration that is `static` without an initializer in one configuration, and an instance member with an initializer in another, therefore counts as a static initializer as well, although no configuration gives it one. The spec states this union (`18-spec-author.md`, *Internal choices*), so it is not a code finding. It is the reason the join of #7 exists. If #7 is repaired by pairing each configuration's own context and initializer, the spec sentence could follow, so that the run withholds only what some configuration actually does.
- Groups that sit side by side are treated as independent, so a region also reads combinations that no build compiles (`#if DEBUG` next to `#if !DEBUG`). This only adds configurations, and so only withholds. I found no shape, valid in every real build, where it withholds a fix that main offers. It does count toward the cap of #8.

## thoenissen/Reihitsu#1616

Nothing to audit: this range touches no RH2001 code, test, spec, change doc, or rule page. Audit 1's result stands.

## Whole run

### Visible output is decided
The new expected values are covered as follows:

- `16-test-implementer.md`, the "keeps the fix" tests: main's output, so nothing changes. The `NoCodeFix…` tests: the unchanged SHARED-FIX-003 and SHARED-ORDER-007, because each is a meaning change in another configuration.
- `19-test-implementer.md`, the `K` example, its counterpart, the attribute-list and collection-initializer tests, the 8-group test, and both closing-brace tests: main's outcome.
- The over-cap test: **not covered** (#8).
- The withholdings of #7 change a code-fix result from main's, while no test asserts them and nothing decides them. They are part of #7.

No rule page changed.

### Scope
The range stays within `OrderingDeclarationUtilities`, the new internal `ConditionalGroup`, the three consumer test classes, and SHARED-ORDER-007. The changed source carries no `TODO` and no tracker number (`rg -n "TODO|#[0-9]{3,4}\b"` over both files: none). The cleaner's S3267/S1168 edits keep the behavior: an empty list stands for "over the cap", and a valid result always holds at least one configuration.

### Comments (FC-031)
The summaries of these members describe the new code: `ChangesMemberOrder` (both private overloads), `TryGetOrderedMembers`, `GetConditionalGroups`, `GetConditionalRegions`, `IsReadAsText`, `WritesOnlyInsideBrackets`, `LiesInsideBrackets`, `GetEnclosingBody`, both `GetConfigurations` overloads, `TryAddOrderedMembersOfRegion`, `GetMemberName`, and every member of `ConditionalGroup`. `IsReadAsText` states its intent ("no configuration of it changes what a member of this type contributes") next to the conditions it checks. #6 shows that the conditions do not establish that intent. The comment describes the code and is not stale.

### Failure classes dismissed (structural delta)
Audit 1's and audit 2's dismissals stand for everything this range did not move. For the moved code:

- **FC-002:** the qualifiers "outermost", "not read as text", "without `#else`", and "not `const`" are explicit conditions.
- **FC-003:** both sides of the cap (8 and 9 groups), of the `const` configuration, and of each placement are tested. The missing side, a region away from the move, is #8.
- **FC-004, FC-005, FC-019, FC-021:** the guard edits nothing. A comment in a piece is trivia to `ParseTokens`, and the parse of a configuration blanks only directives and other branches.
- **FC-006, FC-018, FC-025, FC-026, FC-029, FC-036, FC-037:** no rewrite, rename, or text-only claim is involved.
- **FC-007, FC-008, FC-009:** there is no new action path. Fix All decides each diagnostic on the original document, unchanged.
- **FC-010 to FC-014, FC-028:** no formatter phase is moved. The member-moving fixes still format through `ReihitsuFormatter`, as before.
- **FC-022:** a bracket, `=`, or `#if` inside a string literal or comment of a piece is one literal token or trivia. A directive cannot stand inside a literal.
- **FC-023:** the join compares `Identifier.ValueText`, so `@_b` and `_b` are one name, as in C#.
- **FC-024:** partial types are unchanged. The guard reads the declaration being fixed, as on main.
- **FC-027:** a nested type's body is read as text unless an excluded branch closes it (nt1, #6).
- **FC-030:** decision 15's continuation shapes are avoided or asserted without layout.
- **FC-032:** the cleaner's compiled edit at `3bb3b43` was followed by its analyzer (449) and core (660) runs and a green `scripts/build.sh`. The validator still runs.
- **FC-033, FC-034, FC-035:** every test has LF and CRLF rows, and the expected outputs come from main.
- **FC-015 and FC-016:** see the tables. FC-016 is #6 and #7.

### Hints
_None._
