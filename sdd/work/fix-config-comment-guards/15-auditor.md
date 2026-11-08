Role: auditor
Commit: 3b3f696
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict. The inputs were `00-lead.md`, `09-auditor.md` (audit 1), the repair handovers `10` to `14`, and `git diff cf4d7c6..HEAD`.

State verified:

- The checkout is `3b3f696`.
- `origin/main` is `57acf0d` and is an ancestor of the head.
- The repair diff matches the handovers. It changes these files:
  - `OrderingDeclarationUtilities.cs` (private members only).
  - The four analyzer test classes, with 11 tests and 22 rows.
  - The `RH7102.md` and `RH7103.md` specs.
- Scope did not grow: no other surface is touched and no requirement changed.
- `scripts/lane-check.sh behavior-change`: confirmed.

Fixtures ran with `scripts/apply-fix.sh RH7109` under LF and CRLF, from the scratchpad, with no symbol defined. Main results come from a temporary worktree at `57acf0d`, removed afterwards.

## thoenissen/Reihitsu#1595

### Scope
Repair delta against `cf4d7c6` (audit 1). Head `3b3f696`, base `57acf0d`.

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| — | — | — | — | — | — | No finding of its own. RH7102 reaches findings #3 and #4 through the shared guard (`TypeMemberOrderingCodeFixProviderBase.IsMoveSafe` → `OrderingDeclarationUtilities.ChangesMemberOrder`). Both are reported once, under #1593. | — |

The repair adds the RH7102 test d1. The `Uses` hint of audit 1 is closed (`11-spec-author.md`). The RH7102-001 and RH7102-006 wording matches `HasModifiersSplitByConditionalDirective`, whose span runs from the first modifier to the token behind the last one, so it includes that token's leading trivia.

### Guard scope
_N/A — the repair changes no RH7102-specific guard; see #1593._

### Policy ownership
_N/A — unchanged since audit 1 (one owner of the member-order guard)._

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `NoCodeFixWhenACrossedFieldIsStaticOnlyInAGroupInFrontOfItsFirstToken` (d1) | Reported, no action | An action is registered (red at 28d76dd) | `VerifyNoCodeFixIsOffered` | adequate |

### Failure classes dismissed
See *Whole run*.

### Architecture findings
_None of its own; see #1593._

### Hints
_None._

## thoenissen/Reihitsu#1593

### Scope
Repair delta against `cf4d7c6`. Head `3b3f696`, base `57acf0d`.

### Previous findings
- **#1: closed.** Every member it listed (a1, a2, a4, e1, c1, d1, b1) has a test that now passes, and the counterpart a3 holds. A member it did not name fails; it is reported as #3.
- **#2: closed.** o1, o2, g1, and g2 are fixed as on main, and the counterparts o3 and g3 hold. Members it did not name, or only mentioned as unconfirmed, fail; they are reported as #4.
- **Hint (token walk): closed.** `GetConditionalTriviaOfDeclaration` now descends only through nodes and tokens that contain directives, and `GetOrderedMembersOfDeclaration` returns early when `ContainsDirectives` is false. This is equivalent to the old walk: a `DisabledTextTrivia` always shares its trivia list with the directive that opens it, so a token that holds disabled text has `ContainsDirectives`.

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 3 | high | `src/Reihitsu.Core/OrderingDeclarationUtilities.cs:542-617` (`IsStaticInExcludedCodeInFront` reads only the first token's leading trivia), `:465-521` (`GetOrderedMembersOfDeclaration` / `GetConditionalTriviaOfDeclaration` read only `member.Span`) | FC-001, FC-020, FC-016 | #1 | same requirement | **A `static` that an excluded branch gives the declaration is lost when that branch lies in the previous member's span.** The conditional group opens in front of the previous member's terminator, and its `#endif` falls into the declaration's first-token trivia. The excluded text then belongs to the previous member's span. That member reads `private static` behind its own `;` as an incomplete member, which contributes nothing. The declaration's own first-token trivia holds only `#endif`, so neither reading attributes the `static` to it. The guard offers a fix that reorders the static initializers of the `DEBUG` build. Counterexample x9 (RH7109, **fixed** on head; listing below). In a `DEBUG` build, `_b` is `private static int _b = Log("b")`, and the fix puts `_c` in front of `_t`, so `Log("c")` runs before `Log("b")`. **Failing members** (all `fixed`, LF and CRLF): x9, the previous member's initializer split, with the `static` in the excluded `#if` branch; x4, the previous member's `;` written in both branches, with the `static` in the excluded `#if` branch. By construction, the same holds for RH7103, RH7102, and RH7309, which share `GetOrderedMembers`. **Members that hold:** x8, the same shape with the excluded branch behind an `#else` in the declaration's first-token trivia: no fix, through the empty-stack `#else` push. Also every member of #1. These shapes are unusual, because the previous member's terminator sits inside the group, but they are valid C# that a user document can hold. | Read each conditional group as a whole rather than per trivia list or member span. For every group that ends in the declaration's first-token trivia or inside its span, take all its branches (`DirectiveTriviaSyntax.GetRelatedDirectives()`) and read each excluded branch's text behind its last complete member as the declaration's head. Add x9 and x4 as `VerifyNoCodeFixIsOffered` tests, with x8 as the counterpart |
| 4 | high | `src/Reihitsu.Core/OrderingDeclarationUtilities.cs:487-489` (`isInstanceInAnyConfiguration`, `isNonConstantInAnyConfiguration`), `:504-510` (depth-0 `static` in the excluded head) | FC-001 (inverse: the guard is wider than its decision), FC-016 | #2 | PR-introduced | **The split member is counted with a context or kind that no configuration gives it, so correct fixes that main offers are withheld.** (a) An active `static` or `const` inside a conditional span makes the member count as instance, or as non-constant, "in some configuration", even when every other branch of the group also writes that modifier. Only an accessibility or `readonly` differs, yet the member gains an instance initializer and a layout-field entry. This is unchanged by the repair, and audit 1 missed it. (b) The depth-0 `static` scan of the excluded head takes a static lambda or anonymous method written as the initializer value for a `static` modifier. Counterexample x1 (RH7109: main **fixed**, head **no-fix-offered**; listing below). `_b` is static in both configurations, so moving `_c` in front of `_a` changes no initializer order. **Failing members** (main fixed, head no-fix-offered, LF and CRLF): x1, `static` in both branches, with `readonly` differing; x1b, `static` in both branches, with accessibility differing (`internal static` / `private static`); x3, `const` in both branches, with accessibility differing, counted as an instance initializer; h4, crossed `private Func<int> _f =` followed by `#if DEBUG static () => 1; #else () => 2; #endif`, counted as a static initializer. Traced but not constructed: the layout-field member of (a), a split `static` or `const` counted as a layout field in a struct. RH7109 cannot isolate it, because its move always crosses an instance field, but RH7309 can reach it. By construction, all four consumers are affected. **Members that hold:** x7, where the other branch really lacks `static`: no fix on main and head, correctly; h5, a static lambda nested in an argument list: fixed on main and head; and o1 to o3 and g1 to g3. No requirement demands these withholdings and no maintainer answer covers them, so this is a visible code-fix change that is undecided (see *Visible output is decided*). | Decide "instance or non-constant in some configuration" from the branches. It holds only when an excluded branch of the group, or the empty `#else` of an `#if` without one, lacks the modifier. Read `static` in the excluded head only in front of the declarator's `=`. Add x1, x1b, x3, and h4 as counterparts that keep main's fix |
| 5 | medium | `src/Reihitsu.Analyzer.Test/Ordering/RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzerTests.cs`, `RH7103…AnalyzerTests.cs` (repair tests) | FC-017 | – | PR-introduced | **The repair changed the `static` scan of the excluded head to depth 0 (`:506-509`), and no test pins it.** g2 and o1/o2 cover the depth guard only for `=`. Reverting the depth check for `static` would keep every test green while h5 (a nested `static` lambda in an excluded argument, fixed on main) lost its fix again. | Add h5 (below) as a `Verify(source, fixed)` counterpart with main's output. If #4 (b) is repaired by reading `static` only in front of `=`, add it with h4 |

Fixtures (members are the RH7109 targets; each type also declares `private static int Log(string name) { return 0; }`, and h5 also declares `private static int Run(Func<int> f) { return f(); }`):

```csharp
// x9 — #3, fixed on head: reorders Log("c") before Log("b") in DEBUG
public class C
{
    private static int _t;
    private int _a =
#if DEBUG
        1;
    private static
#else
        2;
#endif
    int _b = Log("b");
    private static readonly int _c = Log("c");
}

// x4 — #3, fixed on head: as x9, with the terminator ';' written in both branches
    private static int _t;
    private int _a
#if DEBUG
    ;
    private static
#else
    ;
#endif
    int _b = Log("b");
    private static readonly int _c = Log("c");

// x1 — #4 (a), main fixed, head no fix
    private int _a;
#if DEBUG
    private static readonly
#else
    private static
#endif
    int _b = Log("b");
    private readonly int _c = Log("c");

// x3 — #4 (a), main fixed, head no fix
    private int _a;
#if DEBUG
    internal const
#else
    private const
#endif
    int K = 1;
    private readonly int _c = Log("c");

// h4 — #4 (b), main fixed, head no fix
    private static int _t;
    private Func<int> _f =
#if DEBUG
        static () => 1;
#else
        () => 2;
#endif
    private static int _u;
    private static readonly int _c = Log("c");

// h5 — #5, fixed on main and head; no test pins it
    private static int _t;
    private int _f = Run(
#if DEBUG
        static () => 1);
#else
        () => 2);
#endif
    private static int _u;
    private static readonly int _c = Log("c");
```

The main output of h4 also moves the `() => 2;` continuation line from column 8 to column 4. That is the recorded whole-type formatting follow-up (decision 15), not part of #4. A counterpart test should avoid that continuation shape, as the repair's tests do.

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Excluded code in front gives the declaration `static` (`IsStaticInExcludedCodeInFront`) | Leading trivia of the declaration's first token; for each excluded text, the tokens behind its last depth-0 `;` or `}`; the backward walk stops at a group that completes a member | `GetConditionalTriviaOfDeclaration` (`member.Span`) and `GetOrderedMembersOfDisabledText` (outside every member's span) | Together they cover the declaration's span and its first-token trivia. They miss the excluded branch of a group that closes in that trivia but whose excluded branch lies in the previous member's span (x9, x4). An `#else` with an empty stack covers the branch when it lies in this trivia (x8) | **fails** (#3) |
| Excluded head gives an initializer (`=` at depth 0, `:504-510`) | Tokens of the excluded text up to its first depth-0 `;`; the initial depth comes from the active tokens of the declaration in front of it (`GetNestingDepth`) | `HasInitializerSyntax` (a declarator's or property's `Initializer`) | Agrees. o2 and g2 hold, 1593/e and 1593b/c are still withheld, and a bracket that the excluded text closes is tracked (`GetNestingDepths` lowers the depth before assigning it) | holds |
| Excluded head gives `static` (depth 0, `:504-510`) | Same tokens | `IsStatic` (a `static` modifier) | Wider: a static lambda or anonymous method as the initializer value stands at depth 0 (h4) | **fails** (#4 b) |
| Split member counts as instance / non-constant (`:487-489`) | Whether each active `static` / `const` lies inside a `GetConditionalSpans` part | SHARED-ORDER-007: "both contexts when one configuration declares it `static` and another does not" | Wider: it does not look at whether the other branches also carry the modifier (x1, x1b, x3) | **fails** (#4 a) |
| Unparsed excluded text gives an initializer (`GetOrderedMembersOfText`, `ContainsTopLevelToken`) | Each incomplete member and each skipped-token run, from depth 0 | `CreateOrderedMember` → `HasInitializer` | Agrees: o1 and g1 hold, and 1593/e is withheld | holds |
| Short-circuit (`ContainsDirectives == false`; `DescendantTokens(node => node.ContainsDirectives)`) | A node's full span, including the first token's leading trivia | The old full token walk | Equivalent (disabled text always shares its trivia list with a directive) | holds |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Bracket depth of excluded tokens | `rg -n "OpenBracketToken or SyntaxKind\|NestingDepth\|GetNestingDelta\|ContainsTopLevelToken" src -g '!*Test*'`: only `OrderingDeclarationUtilities.GetNestingDepth`, `GetNestingDepths`, `GetNestingDelta`, and `ContainsTopLevelToken`. `RegionDirectiveUtilities.NestingDepth` is region nesting, a different decision | one owner |
| What a split member contributes | `OrderingDeclarationUtilities` only (`GetOrderedMembersOfDeclaration`, `IsStaticInExcludedCodeInFront`, `GetOrderedMembersOfDisabledText`, `GetOrderedMembersOfText`) | one owner, but three readings of one conditional group, partitioned by trivia list and member span (see *Architecture findings*) |
| Member-order guard call | `rg -n "ChangesMemberOrder\(" src -g '!*Test*'`: `TypeMemberOrderingCodeFixProviderBase.IsMoveSafe`, `RegionBlockLayout.ChangesMemberOrder` | unchanged, one owner |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH7109 a1, a2, a4, e1; RH7103 c1; RH7102 d1; RH7309 b1 (`NoCodeFixWhen…InAGroupInFrontOfItsFirstToken`) | Reported, no action | An action is registered (each was red at 28d76dd) | `VerifyNoCodeFixIsOffered`: exact diagnostics, then no action | adequate |
| RH7109 a3 (`…InAGroupBehindItsFirstModifier`) | Reported, no action | An action | same | adequate (counterpart) |
| RH7109 o1, o2; RH7103 g1, g2 (`FixMoves…`) | Exact fixed text, which equals main's output | Any byte, or no fix (each was red) | `Verify(source, fixed)` with iterations and Fix All | adequate |
| RH7109 o3; RH7103 g3 | Exact fixed text | Any byte | `Verify` | adequate (counterparts) |
| (missing) nested `static` in the excluded head no longer counts | — | — | — | missing (#5) |

Every test runs as an LF and a CRLF `DataRow`. The expected outputs avoid the continuation-line shape of decision 15.

### Failure classes dismissed
See *Whole run*.

### Architecture findings
#3 repeats #1, and #4 repeats #2: the same decision fails in the same component, `OrderingDeclarationUtilities`. That decision is what a declaration that a conditional group splits contributes in each configuration. The structural cause is the unit of reading. The decision's unit is the conditional group with all its branches. The code reads the group in pieces, by the trivia list and the member span that each piece happens to fall into:

- `GetConditionalTriviaOfDeclaration` and `GetConditionalSpans` read inside `member.Span`.
- `IsStaticInExcludedCodeInFront` reads the first-token trivia.
- `GetOrderedMembersOfDisabledText` reads the rest.

Each reading then guesses the other branches from token scans, for example "an active `static` inside a conditional span means some configuration lacks it". A group that crosses a piece boundary loses a branch (#3, and #1 before it). A guess that ignores the sibling branches over-counts (#4, and #2 before it). Reading each group once through `GetRelatedDirectives()`, and deciding per branch, would remove both directions at the source. It stays within the one component the run already changes.

### Hints
_None._

## thoenissen/Reihitsu#1616

Nothing to audit: the repair touches no RH2001 code, test, spec, change doc, or rule page. Audit 1's result stands.

## Whole run

### Visible output is decided
The new expected values are covered as follows:

- `FixMoves…` tests (o1, o2, o3, g1, g2, g3): today's output on main. `10-test-implementer.md` ran them green on main.
- `NoCodeFixWhen…` tests: the unchanged SHARED-FIX-003 and SHARED-ORDER-007. These are meaning changes in another configuration.
- The RH7102/RH7103 spec examples: the unchanged SHARED-DIAG-008, decision 20.

No rule page changed. The withholdings of #4 are a code-fix result that changes from main's while no test asserts it and nothing decides it, so they are part of #4.

### Scope
The repair stays within `OrderingDeclarationUtilities` (private members), its consumers' test classes, and the two specs. The changed source carries no `TODO` and no tracker numbers.

### Comments (FC-031)
The summaries of `GetOrderedMembersOfDeclaration`, `IsStaticInExcludedCodeInFront`, `GetNestingDepth`, `GetNestingDepths`, `GetNestingDelta`, `ContainsTopLevelToken`, `GetConditionalTriviaOfDeclaration`, and `GetOrderedMembersOfText` describe the new code. `IsStaticInExcludedCodeInFront` states its rule ("in front of the declaration's first token") accurately; that rule is the gap behind #3.

### Failure classes dismissed (repair delta)
Audit 1's dismissals are reused for everything the repair did not move. For the moved code:

- **FC-002**: the qualifier "behind its last complete member" is explicit (`tailStart`).
- **FC-003**: depth 0 vs nested, in front vs behind the first modifier, and complete vs open groups are tested on both sides. The missing side for nested `static` is #5.
- **FC-008**: Fix All decides the guard per diagnostic on the original document, unchanged.
- **FC-009**: no new action path.
- **FC-022/023**: an `=`, a bracket, or a `static` inside a string literal or a comment of excluded text is a literal token or trivia in `ParseTokens`, not a counted token.
- **FC-026**: modifier matching is unchanged.
- **FC-027**: nested types are ranked by their own type, unchanged.
- **FC-032**: the implementer's analyzer, core, and architecture runs and the cleaner's Release build (`scripts/build.sh` green) were taken after the last compiled edit, e190498.
- **FC-033/034/035**: the expected outputs come from main, with LF and CRLF rows.
- **FC-010 to FC-014, FC-018, FC-025, FC-029, FC-030, FC-036, FC-037**: no formatter phase, rename path, or new rule is moved by the repair. The FC-030 continuation-line follow-up stays as decision 15 records it.

### Hints
_None._
