Role: auditor
Commit: cf4d7c6
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict. The inputs were `00-lead.md` (*Maintainer answers*, *Decisions taken*), handovers `01` to `08`, the specs and the change doc in the diff, and `git diff origin/main...HEAD`.

State verified:

- The checkout is `cf4d7c6`.
- `origin/main` is `57acf0d` and is an ancestor of the head.
- The diff matches the handovers: 15 source files, and the production paths that `06-implementer.md` names.
- `scripts/lane-check.sh behavior-change` confirms the lane.

Fixtures ran with `scripts/apply-fix.sh <ID>` under LF and CRLF, from a scratch directory outside the repository, with no preprocessor symbol defined. "Main" results come from a temporary worktree at `57acf0d`, removed afterwards.

## thoenissen/Reihitsu#1595

### Scope
Full audit. Head `cf4d7c6`, base `57acf0d`.

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| — | — | — | — | — | — | RH7102 joins the shared guard correctly (`TypeMemberOrderingCodeFixProviderBase.IsMoveSafe`, now private and unconditional). Findings #1 and #2 under #1593 reach RH7102 through that guard: #1 fixture d1, and #2 by construction. They are reported once, there. | — |

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Move safe for RH7102/RH7103/RH7109 (`TypeMemberOrderingCodeFixProviderBase.IsMoveSafe`) | `MoveRangeContainsDirectives` over target..moved, plus `ChangesMemberOrder` over the whole type body | RH7309 `RegionBlockLayout.ChangesMemberOrder` calls the same `OrderingDeclarationUtilities.ChangesMemberOrder` | Same owner and the same ranking of positions (rank 0/1/2/3 against segments) | holds |
| RH7102 analyzer: constant with split modifiers is not reported (`HasModifiersSplitByConditionalDirective`) | `[first modifier, token behind the last modifier)` | SHARED-DIAG-008 judged span (the modifier list) | For a reported constant, the `const` token is active, so any group that toggles it, or toggles the accessibility written next to it, ends inside the span. A group entirely in front of the first modifier cannot remove an active `const`. | holds |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Member-order guard call | `rg -n "ChangesMemberOrder\(" src -g '!*Test*'`: `TypeMemberOrderingCodeFixProviderBase.IsMoveSafe:79`, `RegionBlockLayout.ChangesMemberOrder:71`, and the owner `OrderingDeclarationUtilities`. The RH7103/RH7109 overrides are gone. | one owner |
| Modifier split under a conditional (analyzer side) | `OrderingDeclarationUtilities.HasModifiersSplitByConditionalDirective`, which reuses `SyntaxTriviaUtilities.ContainsConditionalCompilationBoundary`; its callers are the RH7102 and RH7103 analyzers | one owner (see #1593 for the guard's own split detection, which decides something else) |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| The 11 `NoCodeFixWhen…` RH7102 tests (rows a2, o, i, g, b2, d, q, n, r, l, m, and the split crossed `J`) | Reported, no action registered | An action is registered (each was red at `beeac8e`) | `VerifyNoCodeFixIsOffered`: exact diagnostics, then no action for any reported diagnostic | adequate |
| `Fix…` counterparts (h, f, k, p, c2, e) | Exact fixed text | Any change of the text | `Verify(source, fixed)` with the verifier's iterations and Fix All | adequate |
| `ConstantWhoseModifiersAnIfSplits…IsNotReported` (s1, s2) | No diagnostic | Any diagnostic | `Verify(source)` | adequate |
| `FixAllSkipsTheConstant…` | `K` stays and is still reported; `L` moves | `K` moved | `Verify` with `MarkupMode.Allow` on the fixed and batch states | adequate |

All of them run as LF and CRLF `DataRow`s. `WithEndOfLine` rewrites the input and the expected text alike.

### Failure classes dismissed
See *Whole run*.

### Architecture findings
_None._ Decision 4 is carried out: the guard has no opt-in left.

### Hints
_None._

## thoenissen/Reihitsu#1593

### Scope
Full audit. Head `cf4d7c6`, base `57acf0d`.

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | high | `src/Reihitsu.Core/OrderingDeclarationUtilities.cs:459-513` (`GetOrderedMembersOfDeclaration`), `:522-538` (`GetConditionalTriviaOfDeclaration` reads `memberDeclaration.Span`), `:741` (`GetOrderedMembersOfDisabledText` gives the excluded text in front of a member to "outside every member") | FC-001, FC-020, FC-016 | – | same requirement | **A split whose excluded part lies in front of the declaration's first token is not counted.** SHARED-ORDER-007 says a split member "counts in both contexts when one configuration declares it `static` and another does not". The guard reads a member's conditional parts only inside `member.Span`, which starts at the first token. Excluded text in that token's leading trivia goes to the "outside every member" path, which parses it alone (`private static` → an incomplete member), so it contributes nothing. A `static` that only another configuration compiles is lost, and the guard offers a fix that reorders that configuration's static initializers. Counterexample (`apply-fix.sh RH7109`: **fixed**, on main and head alike): `class C { private static int _t;` / `#if DEBUG` / `private static` / `#else` / `private` / `#endif` / `int _b = Log("b");` / `private static readonly int _c = Log("c"); … }`. The fix moves `_c` in front of `_t` and crosses `_b`, so in a `DEBUG` build `Log("c")` now runs before `Log("b")`. **Failing members** (all `fixed`, LF and CRLF): RH7109, crossed member, group opens in front of the first token (a1); RH7109, crossed member, whole `#if DEBUG static #endif` group in front of `private int _b = …` (a2); RH7109, crossed `#if DEBUG static readonly #else const #endif int K = 1;`, which is `static readonly` in `DEBUG` while `Log` reads `K` (a4); RH7109, moved member `#if DEBUG private static readonly #else private readonly #endif int _c = Log("c");` across `private static int _s = Log("s")` (e1); RH7103, crossed member (c1); RH7102, crossed member, with `K` carrying an excluded `private static int _x = Log("x")` (d1); RH7309, moved `Fields` block across `private static int _a = Log("a")` (b1). **Members that hold:** the same splits with the `#if` behind the first modifier (a3 and b2: no fix), and `#if !DEBUG static #endif private int _b;` in a struct (b3: no fix, because the `#endif` lies inside the span). The other contributions of excluded code in front of the first token hold or cannot occur there: non-`const` and instance layout field are covered through `GetConditionalSpans`, because the group's closing directive lies inside the span, and no initializer can stand in front of the first token. | Count the conditional groups in the leading trivia of the declaration's first token with the declaration: those whose branch closes inside the span, and a complete group directly in front of the first token whose excluded text declares no complete member. Read their excluded head tokens (`static`) for the member, as for excluded text inside the span. Add tests for a1, a2, a4, b1, c1, d1, e1 on their consumers, with a3/b2/b3 as counterparts |
| 2 | high | `src/Reihitsu.Core/OrderingDeclarationUtilities.cs:494-495` (`headTokens.Any(StaticKeyword / EqualsToken)`), `:787` (`unparsedTokens.Any(EqualsToken)` in `GetOrderedMembersOfText`) | FC-001 (inverse: the guard is wider than its decision), FC-016 | – | PR-introduced | **The guard counts any `=` in excluded text as a runtime initializer, at any nesting depth.** The new code counts every `=` token of excluded text in front of a declaration's first `;`, and every `=` of excluded text that parses as no complete member, as a runtime initializer, in both contexts in the latter case. It does not require that the `=` is a declarator's or property's initializer. Fixes that main offers, and that are correct in every configuration, are now withheld. That is a visible change to a code-fix result, and no requirement demands it and no maintainer answer covers it. Decision 6 rejected option B precisely because withholding a correct fix is such a change. Counterexample (RH7109; main **fixed**, head **no-fix-offered**): `private int _a = Log("a");` / `#if DEBUG` / `[Obsolete("x", DiagnosticId = "y")]` / `#endif` / `private readonly int _b;`. `_b` has no initializer in any configuration, so the move changes nothing. **Failing members** (main fixed, head no-fix-offered, LF and CRLF): an excluded attribute with a named argument in the member's leading trivia (o1, a field); the same inside the declaration span, behind an active `[NonSerialized]` (o2); an excluded `[DllImport("a", EntryPoint = "b")]` in front of a `static extern` method, crossing `private int _b = 2` (g1, RH7103; this reaches any member kind, because the "outside every member" path parses all excluded text); an excluded accessor `set { _v = value; }` of a static property crossing `public static int Counter = Log("c")` (g2, RH7103). **Members that hold:** an excluded attribute without a named argument (o3) and an excluded accessor body without `=` (g3); both are still fixed. All four consumers share `GetOrderedMembers`, so RH7102 and RH7309 have the same members by construction. The `static` scan on line 494 has the same depth problem, for example a `static` lambda in an excluded initializer part. It was not confirmed, because the one fixture I built (h1) is withheld on main as well, for another reason. | Count an excluded `=` (and `static`) only at the top nesting level of the excluded fragment, outside `( )`, `[ ]`, and `{ }`, or only where the fragment's own parse puts it in an initializer. Keep 1593/e and 1593b/c (`= Log("b");` behind an accessor list) withheld. Add o1, o2, g1, g2 as counterparts that keep their fix |

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| What a split member gives in another configuration (`GetOrderedMembersOfDeclaration` + `GetConditionalTriviaOfDeclaration`) | `member.Span`, without tokens under a `BlockSyntax` or `ArrowExpressionClauseSyntax` | `GetOrderedMembersOfDisabledText`: excluded text outside every `member.Span` | The two partition the text at the first token's `SpanStart`, but the decision's span is the declaration together with the conditional groups that open in front of it | **fails** (#1) |
| Excluded text gives an initializer / `static` (lines 494–495) | Every token in front of the first `;`, at any depth | `HasInitializerSyntax`: only a declarator's or property's `Initializer` | Wider than the counterpart | **fails** (#2) |
| Unparsed excluded text gives an initializer (line 787) | Every token of an `IncompleteMemberSyntax` and of skipped tokens | `CreateOrderedMember` → `HasInitializer` on complete members | Wider than the counterpart | **fails** (#2) |
| Member counted in both contexts (`isInstanceInAnyConfiguration`) | `static` tokens inside a `GetConditionalSpans` part | SHARED-ORDER-007 split bullet 2 | Agrees for every `static` inside the span (b3, 1593/g, 1593/f) | holds |
| `GetConditionalSpans` boundaries | Branches that open in front of the span (`#else`/`#elif`/`#endif` with an empty stack) and branches that close behind it | — | Traced: `#elif` with an empty stack opens `[start, elif)`, nested groups pop correctly | holds |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Members of excluded code | `rg -n "DisabledTextTrivia" src/Reihitsu.Core/OrderingDeclarationUtilities.cs src/Reihitsu.Analyzer.CodeFixes/Core/RegionBlockLayout.cs`: only `OrderingDeclarationUtilities` (`GetOrderedMembersOfDisabledText`, `GetOrderedMembersOfDeclaration`). `RegionBlockLayout` supplies only the rank | one owner; #1 is a boundary between two of its own methods, not a second owner |
| "Is this member split by a conditional" | Guard: `GetConditionalTriviaOfDeclaration` (`member.Span`). Analyzers: `HasModifiersSplitByConditionalDirective` (`[first modifier, next token)`) | Two predicates with different spans, but they decide different things (what to count, and whether to report). Not a duplicated policy |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH7309 `NoCodeFixWhen…` (1593/b, /c, /e, /g, 1593d/b) | Reported, no action | An action is registered (each was red) | `VerifyNoCodeFixIsOffered` | adequate |
| RH7309 counterparts 1593/a (fixed, spec example), 1593d/a and 1593/f (no fix) | Exact text / no action | Change of text / an action | `Verify(source, fixed)` / `VerifyNoCodeFixIsOffered` | adequate |
| RH7109 1593b/a, /b, /c; RH7103 split initializer | Reported, no action | An action (each was red) | `VerifyNoCodeFixIsOffered` | adequate |
| RH7103 `StaticMemberWhoseStaticModifierAnIfSplitsIsNotReported` | No diagnostic | Any diagnostic | `Verify(source)` | adequate |
| (missing) a member whose split opens in front of its first token; excluded `=` that is not an initializer | — | — | — | missing tests for #1 and #2 |

### Failure classes dismissed
See *Whole run*.

### Architecture findings
_None._ #1 and #2 lie inside the one owner of the decision, `OrderingDeclarationUtilities`, and no second component predicts or copies it.

### Hints
- `GetConditionalTriviaOfDeclaration` walks every token of every field, event field, and property, with an ancestor walk per token, on every guard call, even for a member that holds no directive. `memberDeclaration.ContainsDirectives` would short-circuit that. This runs on the code-fix path only, not in an analyzer.

## thoenissen/Reihitsu#1616

### Scope
Full audit. Head `cf4d7c6`, base `57acf0d`.

### Findings
_None._ Traced `TryWriteModifiersInOrder` for every gap shape. A crossing gap is refused. For a kept-order gap, the trivia goes behind the last modifier in front of it, and an added modifier is written behind it. When the last gap moves, its trivia goes to the next written modifier, and the type token's trivia is cleared. Two gaps can never map to the same modifier: if they did, the first gap would already have been refused. Extra RH2001 fixtures, all `fixed` and converging under LF and CRLF:

- i1: a balanced `#if` block above the property. Output `private static readonly int _value;`.
- i2: modifiers on two lines without a comment. Output: one line.
- i3: no modifiers, with `// lead`. Output `readonly int _value;`, with the comment kept once.
- i4: `static private // d` then a new line. Output `private static // d` then `readonly int _value;`.

i5 (`/* lead */ static /* a */ private`) gets no fix. All of these match RH2001-004/009 and the change doc. The RH7105/RH7106 path (`ModifierOrderingCodeFixProviderBase`) gives the same output as the removed `NormalizeModifierTrivia`. Its guard `ReorderingWouldDropCommentOrDirective` leaves no commented gap, so the discarded `nextTokenLeadingTrivia` (`out _`) cannot leave a duplicate behind, and `TryWriteModifiersInOrder` cannot return `false` there (permutations only), so no empty action is registered (FC-009).

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| RH2001 reorder withheld for a crossed comment or directive (`TryWriteModifiersInOrder` → `false`) | Trailing trivia of each written modifier, and the leading trivia of the token behind each gap, including the type token for the last gap | RH2001-009 ("between two modifiers that the new order puts the other way round"); `KeepsEveryCommentAndDirective` as a net | Agrees. Kept-order gaps and the last gap are never refused | holds |
| RH7105/RH7106 withheld (`ReorderingWouldDropCommentOrDirective`) | Any comment or directive among the modifiers or in front of the next token | SHARED-MODORDER-003 | Deliberately wider than RH2001-009, as the spec and the change doc (*Only a crossed comment withholds the fix*) state | holds |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Writing reordered modifiers (SHARED-MODORDER-002) | `rg -n "TryWriteModifiersInOrder\|NormalizeModifierTrivia" src -g '!*Test*'`: `ModifierOrderingUtilities.TryWriteModifiersInOrder`, called by `ModifierOrderingCodeFixProviderBase` and the RH2001 fix. `NormalizeModifierTrivia` is gone. | one owner (decision 7 carried out) |
| Order of modifiers | `OrderModifiersForRh7105` / `OrderModifiersForRh7106` | unchanged |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH2001 `VerifyFix…` (a, i, k, j, c/d/l spec example, `/* lead */`, e, n, `unsafe`, kept-order comment and directive, u2, h) | Exact field text, references renamed | Any byte of the output | `Verify(source, fixed)` (iterations and Fix All) | adequate; the expected text is in RH7105 order, so an RH7105 report on the result (FC-007) is excluded |
| `VerifyNoFixIsOffered…` (b/f/m, `#pragma` crossed) | Reported, no action | An action | `VerifyNoCodeFixIsOffered` | adequate |
| `VerifyFixAllSkipsTheProperty…` | One property withheld, two fixed | A withheld property changed, or a gap or line break left in a fixed one | `Verify` with `MarkupMode.Allow` | adequate |

### Failure classes dismissed
See *Whole run*.

### Architecture findings
_None._

### Hints
_None._

## Whole run

### Visible output is decided
Every expected value that the diff adds or changes is covered by one of the following:

- The RH2001 outputs and withdrawals: the change doc `Decisions` (batch 1 Q1 A, Q2 A, batch 2 A) and RH2001-004.
- The RH7102/RH7103/RH7109/RH7309 withholdings: the unchanged SHARED-FIX-003 / SHARED-ORDER-007.
- The "not reported" tests: the unchanged SHARED-DIAG-008 / RH7102-001.
- The fixed counterparts: today's output on main, or a spec example.

No rule page changed. The PR-introduced withholdings of #2 are a visible change that no test asserts and nothing covers; that is part of finding #2.

### Scope
The edits stay within the components the handovers and decisions 4, 7, 16, and 18 name. `scripts/lane-check.sh behavior-change`: confirmed. Source carries no `TODO` and no tracker numbers.

### Comments (FC-031)
The summaries of `IsMoveSafe`, both public `ChangesMemberOrder` overloads, `GetOrderedMembers*`, `HasInitializer`/`HasInitializerSyntax`, `IsLayoutField`/`DeclaresLayoutField`, `TryWriteModifiersInOrder`, and the RH2001 remarks describe the new code. `GetOrderedMembersOfText` states its `=` rule accurately; the rule itself is finding #2.

### Failure classes dismissed
- **FC-002**: the qualifier "only one configuration declares it `static`" is an explicit condition (`isInstanceInAnyConfiguration` / `isStaticInAnyConfiguration`).
- **FC-003**: crossed with and without an initializer, explicit vs. sequential layout, and kept vs. crossed comment are all tested on both sides.
- **FC-004/005/019/020/021**: RH2001 is tested with block and end-of-line comments, `#pragma`, `#region`, a doc comment, and a leading same-line comment. `#if` between RH2001 modifiers is the existing follow-up (SHARED-DIAG-008).
- **FC-006/024**: modifier order stays RH7105's. `FieldOrderSetsLayout` reads all partial parts through the symbol, unchanged.
- **FC-007**: the expected outputs are in RH7105 order, and the verifier iterates.
- **FC-008**: BatchFixer, with the guard decided per diagnostic on the original document, unchanged from main for RH7103/RH7109. RH7102 Fix All is tested.
- **FC-009**: traced (see #1616).
- **FC-010/011/012/013/014**: no formatter phase and no new rule changed.
- **FC-015/016**: see the policy ownership tables. Finding #1 enumerates every consumer.
- **FC-017**: see the assertion tables.
- **FC-018**: the rename path is unchanged.
- **FC-022/023**: the modifiers rewrite touches no literal or identifier. An `=` inside a string literal in excluded text is a literal token, not `EqualsToken`.
- **FC-025/029/036/037**: not reached.
- **FC-026**: modifiers are matched by kind, and legal modifier lists hold no duplicate kinds.
- **FC-027**: members of nested types lie inside a member span and are ranked by their own type.
- **FC-028**: the node-level formatting entry points used by the fixes are unchanged.
- **FC-030**: the whole-type formatting after a member move is the recorded follow-up (decision 15). It is not changed by this run, and the test counterparts avoid it.
- **FC-032**: the implementer's analyzer, core, and architecture runs and the cleaner's Release build were taken after the last compiled edit.
- **FC-033/034/035**: the tests use the skeptic's corrected same-accessibility inputs, both line endings, and expected values from specs, the change doc, or main.

### Architecture findings
_None._

### Hints
- `RH7102.md` and `RH7103.md` do not list SHARED-DIAG-008 under `Uses`, although decision 16's analyzer change and its tests rest on it. SHARED-DIAG-008 applies to every rule, and the spec checker accepted the spec, so this is not a finding.
- The *Worklist / next step* section of `00-lead.md` still carries two stale `Next:` lines (the amendment and the clarifications), which are done.
