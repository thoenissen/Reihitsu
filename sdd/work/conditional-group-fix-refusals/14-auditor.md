Role: auditor
Commit: 8eaa395f9b5314e9739d3175eb9a8a2908d7908f
Verdict: pass

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

- This audits only the delta of repair round 1, measured against the first audit (`08-auditor.md`, on `fce5e46`). The audited diff is `git diff fce5e46 8eaa395f -- . ':(exclude)sdd/work'`.
- SHAs:
  - The checkout is at head `8eaa395`.
  - `origin/main` is `95aa3e6` (`git ls-remote` agrees) and is an ancestor of head.
  - `scripts/lane-check.sh behavior-change`: confirmed.
- The diff is what handovers 09–13 describe:
  - production: `OrderingDeclarationUtilities.cs` and the RH2001 code fix;
  - tests: the RH2001 and RH7109 test classes;
  - specs: RH2001-004 and RH2001-010, SHARED-FIX-009, SHARED-ORDER-008;
  - the change doc.
- The scope did not grow. The repair stays inside the requirements and components that findings #1–#6 named. No new type, no new public member, and no new requirement ID.
- Counterparts I opened outside the diff:
  - the RH2001 fix's other guards: `IsSupportedPropertyShape` and `HasConditionalDirectiveBetweenModifiers`;
  - the two callers of `ChangesMemberOrder`: `TypeMemberOrderingCodeFixProviderBase` (`:80`) and `RegionBlockLayout` (`:71`);
  - `ChangesInitializerExecutionOrder`, whose only callers are Core tests.
- I ran `scripts/apply-fix.sh` on head over new fixtures in `/tmp/claude-0/-home-user-Reihitsu/f0b6b3e1-bd2b-54f9-b9f7-643b09526771/scratchpad/audit14/`, LF and CRLF. Results are quoted below.
- I trusted the implementer's and the cleaner's focused test results (719 of 719) and re-ran no tests.

## Previous findings

| # | Status | Evidence |
|---|---|---|
| 1 | closed | See *Finding #1* below. |
| 2 | closed | See *Finding #2* below. |
| 3 | closed | See *Finding #3* below. |
| 4 | closed | See *Finding #4* below. |
| 5 | closed | `ChangesOrder(IsLayoutField) && (fieldOrderSetsLayout \|\| …)` gives the same boolean, and the scans now short-circuit behind the cheap check. |
| 6 | closed | `ChangesInitializerExecutionOrder` `<returns>` now names the group in front of the spans, matching its summary. |

### Finding #1

`WrapsWholeUnitsOf(checksNesting: true)` now checks only two things: the group ends with `#endif`, and no other group lies inside it. The "lies inside another group" half moved into `WrapsWholeUnits`, which is the SHARED-ORDER-008 spans reader. So RH2001 no longer applies that half.

Members of the first audit, on head:

| Member | Result |
|---|---|
| Whole file wrapped in `#if NET` | fixed (test `VerifyFixKeepsAGroupOfAWholeMemberInsideTheActiveBranchOfAGroupThatWrapsTheFile`, which is `main`'s output) |
| Class-body region | fixed when no directive of the enclosing group stands in front of the first modifier or behind the property (fixture `rh2001b/c1_region_nonadjacent.cs`) |
| Class-body region whose `#endif` stands behind the property | withheld; Decision #17(a), a listed rare class |
| Region whose `#if` stands directly in front of the inner group | withheld (`rh2001/a5_region_wrapped_kept.cs`); the change doc's rare class "a group in front of the first modifier that holds another group" covers it |
| A group that holds another group | withheld; a listed rare class |
| A whole-member group with no enclosing group | fixed |

For the SHARED-ORDER-008 readers, the result is unchanged:
- The check `#if` count equals `#endif` count over `[0, groupStart)` now runs once per group, before the boundary-list intersection. Before, it ran once per list inside `.Any`, with the same outcome. An empty list set gives `false` either way.
- `DeclarationHoldsConditionalDirective` still reads with no nesting check (`checksNesting: false`).

### Finding #2

`TryCreateFieldDeclaration` now also refuses when `node.Initializer == null` and `ConditionalGroupsWrapWholeMembers(node.GetLastToken().GetNextToken())` fails. All of these return `no-fix-offered` on LF and CRLF:

| Member | Evidence |
|---|---|
| `= 5;` behind the property, next member | spec test |
| `= 5;` behind the property, closing brace of the type body | test |
| `= 5;` behind the property, no modifier | test |
| The token behind is the first token of a nested type (its `Parent` is the nested type) | `a1_nested_type_next.cs` |
| The token behind is an `enum` member | `a4_enum_next.cs` |
| The token behind is the closing brace of a nested type | `a2_in_nested_last.cs` |
| `#if X = 5; #else <member> #endif` behind the property | `a6_else_active_behind.cs` |
| `#if !X <property> #else <property with initializer> #endif` straddling the property | `a3_straddle_else_init.cs` |

The class enumeration is now complete:

| Position | Covered by |
|---|---|
| In front of the first modifier | RH2001-010 |
| Between modifiers, or in front of the type | RH2001-009 |
| Inside the declaration | `KeepsEveryCommentAndDirective`; an active `#else` initializer puts the directives in front of `=`, RH2001-007 |
| Behind a property without an initializer | new check |
| Behind a property with an initializer | the property ends in `;`, so nothing can join it (example and test `VerifyFixKeepsAConditionalModifierBehindAPropertyWithAnInitializer`) |

A directive is never in trailing trivia. So everything between the property's last token and the next token is in that next token's leading trivia.

### Finding #3

The change doc now has a section *Rare-class withholdings*, outside *Visible output changes*. It names:
- the four classes of the first audit;
- the two classes this round creates.

The PR's `Review notes` must name them as well; that is the lead's job at publishing.

### Finding #4

`DeclarationHoldsConditionalDirective` now reads up to `SemicolonToken` when `OpenBraceToken` is `None`.

| Member | Evidence |
|---|---|
| class, struct (`!X` with `Auto`), record, record struct | tests |
| Directive in front of the `;` (base list) | test |
| Primary-constructor `partial class C(int x)` with a conditional base list and `;` | `rh7109/b2_semicolon_primary_ctor.cs`: no fix |
| Control without a directive | test and `b3_semicolon_plain.cs`: fixed |
| The issue input | `b1_semicolon.cs`: no fix |

The other-document member is read per `DeclaringSyntaxReferences` entry, so the document does not matter.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|

_None._ No new finding, and none that repeats the class of findings #1–#6.

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| RH2001: can an excluded branch behind the property join its declaration? | Leading trivia of `GetLastToken().GetNextToken()`, only when `Initializer == null`; each group is read by the shared whole-members test with the "no group inside" check | The front reading (`Modifiers[0]` trivia), with the same helper and the same flag | The same test in both positions. The boundary at `Initializer` is tested on both sides (FC-003). A straddling group is read from both of its directive positions | ok |
| RH2001: does a group lie inside another group? | Not read for RH2001. The outer group is read only through its own directive in front of the first modifier or behind the property | SHARED-ORDER-008 `WrapsWholeUnits` (directives in `[0, groupStart)`) | The difference is intended and stated in RH2001-010. For an active property, an inner group inside an excluded outer branch always has an outer `#else`/`#elif`/`#endif` in the same token's trivia, so the outer group is read and withholds the fix (traced) | ok |
| SHARED-ORDER-008: header of a declaration that ends in `;` | Tokens up to and including `SemicolonToken` when `OpenBraceToken` is `None` | `FieldOrderSetsLayout(typeSymbol)` (attributes of every declaration) | Now agree for `{}` declarations and for `;` declarations | ok |
| The layout condition for groups in front of the spans | `GetGroups(OpenBrace.End .. reorderedSpan.Start)`, now only after `ChangesOrder(IsLayoutField)` | unchanged | Same result | ok |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| A group holds another group (`#endif` end, no group inside) | `WrapsWholeUnitsOf` only. `rg -n "GetConditionalDirectives\|GetRelatedDirectives\|WholeUnits" src --glob '!*Test*/**'` | one owner |
| A group lies inside another group | `WrapsWholeUnits` only (SHARED-ORDER-008 spans and in front of them) | one owner. RH2001 does not apply it, by spec |
| The trivia scan for conditional directives in a span | The new private `GetConditionalDirectives` (used twice). `ChangesMemberOrder`'s local `GetGroups` scans the type declaration instead of the root and groups the result. The two predate this round in shape and serve different callers | ok |
| Callers of the changed helpers | `ConditionalGroupsWrapWholeMembers`: only the RH2001 fix (`:202`, `:204`). `ChangesMemberOrder`: `TypeMemberOrderingCodeFixProviderBase:80`, `RegionBlockLayout:71`. `ChangesInitializerExecutionOrder`: Core tests only | ok |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| The 5 new RH2001 `VerifyNoCodeFixIsOffered…` tests: excluded initializer next member, closing brace, no modifier; conditional modifier behind; nested group behind | diagnostic reported, no action | a registered action | `VerifyNoCodeFixIsOffered` | adequate |
| `VerifyFixKeepsAGroupOfAWholeMemberBehind…`, `…ConditionalModifierBehindAPropertyWithAnInitializer`, `…InsideTheActiveBranchOfAGroupThatWrapsTheFile` | exact output and convergence; the expected values are `main`'s | any output difference | `Verify` | adequate |
| `VerifyFixAllSkipsThePropertyBehindWhichAnExcludedInitializerStands` | Fix All converts `Second` and `Third` only | `First` converted, or another property left | `Verify`, `MarkupMode.Allow` on the fixed and batch states | adequate |
| Renamed `VerifyNoCodeFixIsOfferedForAGroupBehindThePropertyThatHoldsAnotherGroupInItsBranch` | no action. The name and summary now describe the reason that applies | an action | `VerifyNoCodeFixIsOffered` | adequate (FC-031 fixed for this test) |
| The 6 new RH7109 `;`-declaration `NoCodeFix…` tests | no action | an action | `VerifyNoCodeFixIsOffered` | adequate |
| `FixMovesAFieldOfAPartialClassWhoseOtherDeclarationEndsInASemicolonWithoutAConditionalDirective` | exact move | output difference | `Verify` | adequate; the counterpart that keeps the refusal from widening |

## Visible output is decided (check 10)

| Expected value | Covered by |
|---|---|
| The finding #2 inputs, including Fix All | unchanged SHARED-FIX-003 |
| The finding #4 inputs (class, struct, record, record struct, A5 with `;`) | unchanged SHARED-FIX-003 |
| The file-wrapped group; a group of whole members behind the property; `static` behind a property with an initializer; the `;` declaration without a directive | `main`'s output, unchanged |
| The base list in front of `;`; the conditional modifier behind; the nested group behind; the renamed test | rare classes listed in the change doc under *Rare-class withholdings* (Decision #14, #17(a)) |

No rule page changed.

## Cost of special cases (check 11)

The new guards are cheap checks on trivia that already exists:
- `Initializer == null`;
- one `GetNextToken()`;
- the `OpenBraceToken` kind.

They reuse the shared parse. The formatter is not involved. No shape is declared unsupported, and no FC-042 guard exists.

## Comments and scope (checks 8, 9)

- The changed summaries describe the new code:
  - `ConditionalGroupsWrapWholeMembers`;
  - `WrapsWholeUnits` and `WrapsWholeUnitsOf`;
  - `DeclarationHoldsConditionalDirective`;
  - `ChangesInitializerExecutionOrder` `<returns>`;
  - the RH2001 fix's remarks.
- The added production lines carry no `TODO`, no commented-out code, and no tracker numbers.
- No edit lies outside the run's scope.

## Failure classes dismissed

The first audit's dismissals (`08-auditor.md`, *Whole run*) still hold. This round adds only refusals and one reordered boolean, and changes no rewrite. Classes I re-checked against what the round moved:

| Class | Why it does not apply |
|---|---|
| FC-001, FC-002 | The behind check's qualifiers are explicit conditions in the code: "without an initializer" is `Initializer == null`, and "the token behind the last token" is `GetNextToken()`. The `;` reading is an explicit `OpenBraceToken` kind check. |
| FC-003 | Both sides of the `Initializer` boundary and of the `;`/`{}` boundary are tested. |
| FC-016 | Every member of the #2 and #4 classes is enumerated and traced above. |
| FC-020 | The behind position is now read. Every position where a directive can stand next to the property is covered (table under *Finding #2*). |
| FC-024 | `partial` declarations ending in `;` are covered in four type kinds; the other-document case is per declaration. |
| FC-025 | Expression-bodied properties are refused by `IsSupportedPropertyShape` (`ExpressionBody != null`). |
| FC-027 | The token behind can belong to a nested type, an enum member, or the closing brace of a nested type. Each was checked (`a1`, `a2`, `a4`). |
| FC-031 | See checks 8 and 9 above. |
| FC-033, FC-034 | Every fixture ran on LF and CRLF, and all new tests are `[DataRow]` LF/CRLF except the Fix All test, as in the first round. |

## Architecture findings

_None._

## Hints

- **Behind guard is wider than the harm.** The behind-the-property check refuses every excluded branch that is not whole members, such as an attribute list or a modifier of the next member (`VerifyNoCodeFixIsOfferedForAConditionalModifierBehindAPropertyWithoutAnInitializer`). The only text that can join a property without an initializer is an initializer starting with `=`. A narrower check would be "the excluded branch's first token is `=`". It would be a second reading of the branch next to the shared helper (Decision #6). The change doc lists the class as rare, so this is not a finding; the lead may consider it.
- **RH2001 guard count.** The RH2001 fix now has three adjacent guards on conditional-directive trivia: between the modifiers, in front of the first modifier, and behind the property. Folding them into one "conditional directives around the declaration" check is a possible later cleanup. This repeats the hint of the first audit.
- **Cosmetic, compiled behavior unaffected.** The RH2001 fix's remarks break a line early ("… (RH2001-010). The field gets a new" / "name, …"). In `WrapsWholeUnits`, one summary line is longer than its neighbours. The `ConditionalGroupsWrapWholeMembers` summary says "the type body that holds the token". When the token behind the property begins a nested type, the code takes that nested type as the owner. The parse does not depend on which type owns it, so the result is the same.
