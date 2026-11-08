Role: auditor
Commit: 051f337f
Verdict: #1595: pass; #1593: findings; #1616: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict. It named the work: audit 6, the repair delta of repair round 2 (`df9c9147..051f337f`), checked against audit 5 (`42-auditor.md`), decisions 38–45, *Special cases do not grow the source* (`AGENTS.md`), and decision 34.

State verified:

- The checkout is `051f337f`.
- `git ls-remote origin refs/heads/main` gives `3578f98e85661df7824399a6ce0514afff415a2a`, which is an ancestor of the head.
- `scripts/lane-check.sh behavior-change`: confirmed.
- The diff matches handovers 43–47. Production code changed only in `src/Reihitsu.Core/OrderingDeclarationUtilities.cs` (decisions 38, 39, 42) and in `RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider.cs` (decision 40). Tests were only added or re-tagged. The specs and change docs changed as `43-spec-author.md` states. Nothing outside `src/`, `sdd/specs/`, `sdd/changes/`, and this run's `sdd/work/` changed.
- The round did not grow into an unrelated surface, so this is a delta audit.

Method:

- Static tracing of every changed guard against the specs and its counterpart.
- `scripts/apply-fix.sh` on the head and on a temporary worktree of `origin/main`, under LF and CRLF.
- One `dotnet build` with `LangVersion` 14, to check the partial-property shape of #17.
- Four mutation runs on a temporary worktree of the head: M1, M2, and M3 of #15, each on its own, and one run with the new relaxations of #20 and #21 together. Each run covered the RH7102, RH7103, RH7109, RH7309, and RH2001 test classes, and each surviving mutant was checked live with `apply-fix.sh`.

Fixtures and worktrees lived in the scratchpad and have been removed.

## thoenissen/Reihitsu#1595

### Scope
Repair delta. No RH7102 code of its own changed. RH7102 reaches decision 39's refusal through `TypeMemberOrderingCodeFixProviderBase.IsMoveSafe` → `OrderingDeclarationUtilities.ChangesMemberOrder` → the private rank overload (`:422`), which is audited under #1593. #18 applies to RH7102 too, and is listed there.

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| — | — | — | — | — | — | No finding of its own. The shared guard's findings are under #1593 | — |

### Guard scope
_N/A — no RH7102-specific guard changed; the shared guard is under #1593._

### Policy ownership
_N/A — unchanged._

### Assertion adequacy
_N/A — no test added or changed for RH7102 (`44-test-implementer.md`). The decision-39 path is the shared one, and the RH7109 and RH7309 tests cover it._

### Failure classes dismissed
See *Whole run*.

### Architecture findings
_None of its own._

### Hints
_None._

## thoenissen/Reihitsu#1593

### Scope
Repair delta for decisions 38, 39, 42, 43 and 45:

- `HasBackingField` and `IsWithoutBody`;
- the *No backing field from a branch* clause in `WrapsWholeUnits`;
- `DeclarationHoldsConditionalDirective` and where it is called;
- the deletion of `HasConditionalAccessorWithoutBody`;
- the public summaries.

### Previous findings
| # | Status | Evidence |
|---|---|---|
| 14 | closed | `HasConditionalAccessorWithoutBody` is deleted, together with its group spans and its second parse. "An accessor without a body gives a backing field" now has one predicate, `IsWithoutBody`, which `HasBackingField` (`:540`, Any) and the `WrapsWholeUnits` clause (`:705-713`) share. The plain `get => 0; set;` struct case is withheld: `NoCodeFixWhenAPropertyOfAStructHasAnAccessorWithoutABodyNextToAnAccessorWithABody` was red before the change. The conditional member is refused by the clause. A member that #14's enumeration did not name now fails: #17 |
| 15 | closed | M1, M2, and M3 were each run as a separate mutant, and each is killed. M1: `NoCodeFixWhenAGroupInTheCrossedSpanIsNestedInAGroupOutsideBothSpans` fails (2 rows). M2: `NoCodeFixWhenTheActiveBranchOfAGroupInACrossedAccessorBodyUsesTheFieldKeyword` fails (2 rows). M3: the two `…GroupOfWholeAccessorsOf…UsesTheFieldKeyword` tests fail (4 rows). M4's code, the active path of `HasConditionalAccessorWithoutBody`, is deleted. Its successor, the active half of the new clause, has no test that kills it: #20 repeats #15 |
| 16 | closed | Both public `ChangesMemberOrder` summaries (`:294-300`, `:313-321`) and the private rank overload's summary now name the SHARED-ORDER-008 refusals and the declaration check |

### Decision 34 and *Special cases do not grow the source*
`OrderingDeclarationUtilities.cs` changed by +72/−53 lines in this round. The code lines stayed level (`45-implementer.md`): about 29 were deleted with #14, about 15 were added for decision 39, and about 14 for the accessor clause.

- Nothing in this file evaluates a condition, combines branches, or adds a reader. The accessor clause reuses the parsed units that `ReadsAsWholeUnits` already returns.
- The declaration check is a position test plus the existing whole-member parse. It is within decision 34.
- Its scope is #18.

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 17 | high | `src/Reihitsu.Core/OrderingDeclarationUtilities.cs:539-543` (`HasBackingField`) | FC-024, FC-002, FC-016 | #14 | PR-introduced | **The partial-property exemption now drops every partial declaration with a bodiless accessor, and the implementing declaration is one of them.** SHARED-ORDER-007 (as changed in this round) exempts only "the defining declaration of a partial property". The code returns `partial == false` as soon as *any* accessor lacks a body, so an implementing declaration with mixed accessors counts no backing field. On `main` and before this round the branch was "all accessors bodiless", so an implementing declaration such as `get => field; set;` fell through to the `field` check and counted one. `public partial int Value { get => field; set; }` compiles in C# 14 (built with `LangVersion` 14). Fixture: partial struct `S`, with one part holding the defining `public partial int Value { get; set; }`. The other part holds an `IComparable` region with the implementing `get => field; set;` and `CompareTo`, then a `Fields` region with `private int _x;`. RH7309 on the head: **fixed** under LF and CRLF, moving `_x` in front of `Value`'s backing field, which reorders the struct's layout in every build. On `main`: **no-fix-offered**. Enumeration over declaration × accessors × `field`: (1) Non-partial, all bodiless: holds. (2) Non-partial, mixed: holds (new). (3) Non-partial, all bodied with or without `field`: holds. (4) Partial defining declaration: holds. (5) Partial implementing declaration, all bodied with `field`: holds. (6) Abstract or extern: holds. (7) **Partial implementing declaration, mixed, with `field`: fails** (a regression against `main`). (8) **Partial implementing declaration, mixed, without `field`** (`get => 0; set;`, which compiles): **fails** against the new SHARED-ORDER-007 text, and is also fixed on `main`. This is the third finding on the same decision, the backing-field count of a property (#11, #14) | Refuse or count the class with one condition. A partial declaration is the defining one only when *all* its accessors lack a body: in the `Any(IsWithoutBody)` branch, return `true` unless the declaration is partial and every accessor is bodiless. Add the fixture above (members 7 and 8) as an RH7309 `VerifyNoCodeFixIsOffered` test, LF and CRLF |
| 18 | medium | `OrderingDeclarationUtilities.cs:422-423` (`DeclarationHoldsConditionalDirective` is checked next to the `WrapsWholeUnits` gate, so it refuses every move); SHARED-ORDER-008 type-declaration paragraph; tests `RH7109…NoCodeFixWhenAConditionalGroupWrapsAnAttributeListOfTheType`, `…StandsInTheBaseListOfTheType` | FC-001 (inverse: the refusal is wider than the decision it protects); `AGENTS.md`, *Code-fix coverage* ("None of this licenses an imprecise guard") and the last bullet of *Special cases do not grow the source* | – | PR-introduced | **Decision 39 protects the layout decision, which the header can change in another build, but the check withholds every member move of the type, including moves that reorder no instance field.** The spec's own Why gives only layout as the reason: a conditional `[StructLayout]`, or `struct` in place of `class`. Initializer order does not depend on the header. Two classes that set no layout lose fixes that `main` offers, as RH7103 shows (head **no-fix-offered**, `main` **fixed**, LF and CRLF, moving a static method in front of an instance method). The first is `#if !NETSTANDARD` `[System.Serializable]` `#endif` on `public class C`. The second is `public sealed class C : System.IDisposable` / `#if NET` / `, System.IAsyncDisposable` / `#endif`. A conditional attribute or base-list entry on a type is a usual multi-targeting shape (`[Serializable]`, `[SupportedOSPlatform]`, `IAsyncDisposable`, `ISpanFormattable`). Decision 39 argued that the shape is rare ("Withholds only a rare shape"). For non-layout types that premise does not hold. If the class is common, the last bullet of *Special cases do not grow the source* says that withholding it is the maintainer's question, not a decision the lead takes | Narrow the refusal at no extra cost. Feed the check into the layout flag instead of the gate: `(fieldOrderSetsLayout \|\| DeclarationHoldsConditionalDirective(typeDeclaration)) && ChangesOrder(IsLayoutField)`. A conditional header then makes the type count as layout-sensitive, and only moves that reorder two instance fields are refused. This keeps every decision-39 case: the conditional `[StructLayout]`, the conditional `struct`, and the RH7309 test. Main's output returns for the non-layout examples, so nothing is put in question. The RH7103 `[Serializable]` example in SHARED-ORDER-008 and the two RH7109 tests on a plain class then become `Verify` with `main`'s output. If the lead keeps the wide refusal, ask it as an output question |
| 20 | medium | `OrderingDeclarationUtilities.cs:705` (active accessors of *No backing field from a branch*); the RH7103, RH7109 and RH7309 test classes | FC-017, FC-003 | #15 | new mechanism | **The active half of the new clause has no killing test.** Mutant: `branchAccessors` keeps only the excluded accessors (`listAccessors.Where(accessor => false)`). All 555 rows of the five rule classes stay green. The mutant is live: RH7103, `public class C { public int Value { get => 0; #if !DEBUG set; #endif } public static int Shared => 1; }` is **no-fix-offered** on the head and **fixed** under the mutant (LF and CRLF). SHARED-ORDER-008 demands the refusal ("no branch, active or excluded"). The test that should pin it, `RH7309…NoCodeFixWhenAnActiveConditionalAccessorWithoutABodyStandsNextToAnAccessorWithABody`, uses a struct. There, `HasBackingField` already counts the active `set;` and the layout check refuses, so the test passes with or without the clause. This is #15's M4, the active accessor path, again masked between the same guard and the same test classes | Add the RH7103 class input above as a `VerifyNoCodeFixIsOffered` test, LF and CRLF, tagged SHARED-ORDER-008 |

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Backing field of a property (layout count) | Modifiers (abstract, extern, partial), `AccessorList.Accessors.Any(IsWithoutBody)`, then `field` tokens | SHARED-ORDER-007 (only the *defining* partial declaration is exempt); the C# 14 rules for semi-auto and partial properties | **Disagree** for an implementing partial declaration with mixed accessors | #17 |
| *No backing field from a branch* (SHARED-ORDER-008) | Accessors of the list whose `SpanStart` lies strictly between the group's first and last directive, plus the accessors that `ReadsAsWholeUnits` parsed from each excluded branch; the "all bodiless" exception over the list's active accessors plus those | `HasBackingField`'s Any on the active list; the 3A answer and decision 43 (2A) | They agree. I traced every case where a build has a backing field and the analyzed build does not. Each one has a bodiless accessor in some branch together with a bodied accessor, and each is refused. In an all-bodiless list the analyzed build already counts the backing field. The two-sibling-groups case (`#if A get; #endif #if B set { } #endif`) can only over-count, which is the safe side | holds; test gap #20 |
| Declaration of the reordered type (decision 39, 43 1A) | Leading trivia of every declaration token from the first token up to and including `{` (`TakeWhile(SpanStart <= OpenBraceToken.SpanStart)`). A directive on the first token refuses only when the excluded text right behind it does not read as whole members | SHARED-ORDER-008 type-declaration paragraph; `FieldOrderSetsLayout` (the symbol of the analyzed build), whose decision the check protects | The span agrees with the spec. I traced attribute lists active and excluded, `#elif`/`#else` with the active branch in the header, a whole type in front with an `#else` branch, and a nested type behind a group of whole members of the outer type. A type without `{` has no members to move. The *consequence* is wider than the decision it protects | #18 |
| `field` clause (merged with the accessor clause) | `activeTokens` together with the tokens of the parsed excluded branches | SHARED-ORDER-008 *No `field`* | Same predicate as audit 5, now over one list of parsed branches. M2 and M3 are killed | holds |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| "An accessor without a body gives a backing field" | `rg -n "IsWithoutBody\|Body == null && .*ExpressionBody == null" src -g '!*Test*'`: `OrderingDeclarationUtilities.IsWithoutBody` (used by `HasBackingField` and `WrapsWholeUnits`). The other hits (`AttributeTargetUtilities.cs:49`, `RH5408…CodeFixProvider.cs:84`, `RH5047…Analyzer.cs:50`) decide "simple auto-property" for other rules | one owner. Its partial exemption is #17 |
| Parse of excluded type-body text | `ParseInTypeBody`, used by `GetMembersOfDisabledText` and `ReadsAsWholeUnits`. `DeclarationHoldsConditionalDirective` reuses `ReadsAsWholeUnits` | one wrapper (decision 35) |
| Conditional-directive kind | `OrderingDeclarationUtilities.IsConditionalDirective`, `SyntaxTriviaUtilities.IsConditionalDirective` (private), the inline pattern at `RH7004…CodeFixProvider.cs:130`, and now the inline pattern in the RH2001 fix (`:255`) | Parity: all four test the same four kinds. Acceptable for the reasons audit 5 gave (input type, private helper). See also *Hints* |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `RH7309…NoCodeFixWhenAPropertyOfAStructHasAnAccessorWithoutABodyNextToAnAccessorWithABody`, plus the `set { }` and class counterparts | A mixed list counts a backing field, and only the layout uses it | An action, or a missing action | `VerifyNoCodeFixIsOffered` / `Verify` | adequate (red before the change). Partial declarations are not covered: #17 |
| `RH7103…NoCodeFixWhenAGroupOfWholeAccessorsOfACrossedPropertyOfAClassHoldsAnAccessorWithoutABody` | The excluded half of *No backing field from a branch* | An action | `VerifyNoCodeFixIsOffered` | adequate (red before) |
| `RH7309…NoCodeFixWhenAnActiveConditionalAccessorWithoutABodyStandsNextToAnAccessorWithABody` | The active half of the clause | An action | `VerifyNoCodeFixIsOffered` | **inadequate**: the layout count masks the clause (#20) |
| `RH7309…NoCodeFixWhenAnExcludedConditionalAccessorWithoutABody…`, the acc1 pair, and the `set { }` counterparts | Excluded half; the all-bodied branches keep the fix | An action, or a missing action | both helpers | adequate |
| M1, M2, and M3 tests | Enclosing group; active `field`; accessor-list `field` arm | An action | `VerifyNoCodeFixIsOffered` | adequate (each mutant killed, see *Previous findings*) |
| RH7109 decision-39 tests (layout attribute, `[System.Obsolete]`, `struct`/`class`, base list) and `RH7309…NoCodeFixWhenAConditionalDirectiveStandsInTheAttributeListsOfTheType` | No move in a type with a conditional header | An action | `VerifyNoCodeFixIsOffered` | adequate as tests. The attribute test with the group excluded pins the first-token arm, and the keyword and base-list tests pin the arm for the other tokens. The `[System.Obsolete]` and base-list tests on a plain class pin #18's breadth |
| `RH7109…FixMovesAFieldOfATypeBehindAGroupOfAWholeTypeInFrontOfIt`, `…WithAnAttributeListOutsideAnyGroup` | Whole-type group in front (43 1A); an unconditional attribute keeps the fix | A missing action, or any byte | `Verify` (`main`'s output) | adequate |

### Failure classes dismissed
See *Whole run*.

### Architecture findings
#17 and #20 repeat earlier classes of this run (`squad/README.md`, *Architecture findings*).

- **#17 (repeats #11, #14).** The decision is whether a property has a backing field. It is made in `OrderingDeclarationUtilities.HasBackingField` against the compiler's rules for semi-auto and partial properties, and three audits in a row have found a shape on which the two disagree: a conditional accessor (#11), a mixed list (#14), and now a mixed implementing partial declaration (#17). The cheap end of this loop is the refusal side. Any declaration whose accessor list is not all bodiless, or not all bodied, could count a backing field, partial or not. Over-counting only withholds layout reorders.
- **#20 (repeats #15, M4).** The active accessor path is again untested, because the layout count masks it in the RH7309 struct test.

### Hints
- `IsLayoutField`'s summary (`OrderingDeclarationUtilities.cs:483-485`) still says a property gives a backing field "because it is an auto-property or uses the `field` keyword". `get => 0; set;` is neither, and since decision 38 it counts. This is FC-031 and a comment only, so it is listed as finding #22 under *Whole run*.

## thoenissen/Reihitsu#1616

### Scope
Repair delta for decision 40: `HasConditionalGroupOfOtherModifiers` in the RH2001 fix, RH2001-009 as changed, the amendment to SHARED-FIX-009, and the five RH2001 tests.

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 19 | medium | `src/Reihitsu.Analyzer.CodeFixes/Rules/Design/RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider.cs:197-198`, `:238-273`; `sdd/specs/shared/code-fixes.md` SHARED-FIX-009 (first bullet and the "reads no excluded code" paragraph); RH2001-009 | Auditor check 11 (cost of special cases); `AGENTS.md`, *Special cases do not grow the source* | – | PR-introduced | **About 30 code lines serve one rare class.** That class is a conditional group between a property's modifiers whose every branch holds only `public`, `private`, `protected`, `internal`, `new`, or `static`. To keep the fix there, the code tokenizes every excluded branch with `SyntaxFactory.ParseTokens` and compares each token, active or excluded, with a keyword set, and it adds a comment scan. That is a reading of what another build's modifiers are, a parser-like rule over disabled text. The rule names exactly that as machinery rare inputs never justify. To admit it, the run amended SHARED-FIX-009, the shared principle recorded from batch 3 ("fixes do not parse or infer excluded configurations"): RH2001 is no longer "a fix that reads no excluded code". A cheap refusal would close the same hazard with no reading at all. The hazard is `extern`, a second `readonly`, `unsafe`, `required`, and any other modifier. The refusal: any `#if`/`#elif`/`#else`/`#endif` in the leading trivia of a modifier behind the first, or of the type, withholds the fix. That is the trivia walk already at `:249-255`, ending in `.Any()`. Its cost is decision 36's kept fix (`private #if DEBUG static #endif int P { get; }`), which is itself a rare shape. Withholding the fix for that class puts nothing in question (`squad/README.md`, *What reaches the maintainer*). The test-only gaps of #21 also disappear | Refuse the class: withhold the fix for every conditional directive between the modifiers or between the last modifier and the type. Delete the branch reading. Revert the SHARED-FIX-009 amendment, so that RH2001 reads no excluded code. State the refusal in RH2001-009 and pin it with the existing extern, readonly, and unsafe tests. The decision-36 and `new` tests become `VerifyNoCodeFixIsOffered`. If the lead keeps decision 36's fix instead, #21 stands |
| 21 | medium | `RH2001…CodeFixProvider.cs:266` (comment scan of the active span), `:264`/`:271` (comments of excluded tokens), `:267-269` (the token kinds of the active branch); `src/Reihitsu.Analyzer.Test/Design/RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests.cs` | FC-017, FC-003 | – | new mechanism | **Three conditions that RH2001-009 demands have no test.** Mutant: the active comment scan, the comment checks on tokens, and the active-token half were dropped together. All 555 rows of the five rule classes stay green. Every new RH2001 test puts the modifier in an *excluded* branch (`#if DEBUG`, with `DEBUG` undefined), and none holds a comment. Each relaxation is live (`apply-fix.sh`, LF and CRLF, head → mutant): (1) `private` / `#if !DEBUG` / `unsafe` / `#endif` / `int Value { get; }`: no-fix → **fixed**, writing `readonly` *inside* the active branch (`#if !DEBUG readonly unsafe #endif int _value;`). A build with `DEBUG` then loses `readonly`, which is the placement RH2001-009 forbids. (2) The same with `// note` above `static` in an active branch: no-fix → fixed. (3) The same in an excluded branch: no-fix → fixed | Add `VerifyNoCodeFixIsOffered` tests (LF and CRLF, RH2001-009) for an active branch holding `unsafe`, and for a comment in an active and in an excluded branch. Moot if #19 is taken |

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| RH2001: a conditional group of other modifiers withholds the fix | Groups with a directive in the leading trivia of `Modifiers[1..]` or of the type. For each group: from the end of the `#if` to the start of the `#endif`, with active tokens, comment trivia, and the tokens of `DisabledTextTrivia` | RH2001-009 (a directive between two modifiers or between the last modifier and the type); the RH7105 rank table in `ModifierOrderingUtilities.cs:259-277` (ranks before `readonly`); the existing crossing check in `ModifierOrderingUtilities` | They agree. The allowed set equals RH7105's ranks before `readonly` (accessibility, `static`, `abstract`, `virtual`, `sealed`, `override`, `new`) restricted to modifiers valid on a field. A group whose `#if` stands in front of the first modifier is read whole, which only withholds (`45-implementer.md`); `e5` (`#if !X private #else internal #endif static`) is still fixed, as RH2001-009 allows. A conditional `protected` keeps the fix, and an excluded reference to the property is withheld by the existing `HasExcludedCodeOccurrenceAsync` (checked with a derived class that references `Value` under `#if DEBUG`: no fix) | holds; cost #19, tests #21 |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Modifiers that RH7105 puts in front of `readonly` | `ModifierOrderingUtilities` rank table (`:259-277`) and the literal six-keyword set in `HasConditionalGroupOfOtherModifiers` (`:270`) | A second, derived statement. Parity holds today (see *Guard scope*). It goes away with #19 |
| Reading excluded text | `OrderingDeclarationUtilities.ParseInTypeBody` (member moves) and, new, `SyntaxFactory.ParseTokens` in the RH2001 fix | A second reader of excluded code: #19 |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `VerifyNoCodeFixIsOfferedForAConditionalExtern…`, `…Readonly…`, `…Unsafe…` | An excluded disallowed modifier withholds the fix | An action | `VerifyNoCodeFixIsOffered` | adequate for the excluded token path (red before) |
| `VerifyFixIsOfferedForAConditionalNewBetweenTheModifiersAndTheType`, `VerifyFixIsOfferedForAConditionalModifierBetweenTheModifiersAndTheType` | Allowed modifiers keep the fix, with `readonly` behind the group | Any byte; a second iteration | `Verify` (Fix All) | adequate |
| (none) | An active disallowed modifier, or a comment in any branch, withholds the fix | — | — | **missing**: #21 |

### Failure classes dismissed
See *Whole run*.

### Architecture findings
_None._ #19 is a cost finding, not a structural one. Its second statement of the RH7105 order and its second excluded-text reader are listed under *Policy ownership*.

### Hints
_None._

## Whole run

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 22 | low | `src/Reihitsu.Core/OrderingDeclarationUtilities.cs:483-485` (`IsLayoutField` summary) | FC-031 | – | PR-introduced | The summary still names "an auto-property or uses the `field` keyword". Since decision 38, a property with any bodiless accessor counts. Comment only | Say "a property that `HasBackingField` gives a backing field" |

### Visible output is decided
Every expected value that this round adds or changes is covered by one of the following:

- **SHARED-FIX-003 bug fixes:** the plain mixed struct, the conditional `set;` in a struct, and the conditional `[StructLayout]`.
- **The standing answer:** decisions 43 (1A, 2A) and 45 (the class accessor case), all listed in the change doc `member-move-conditional-groups.md`.
- **Decision 40:** a withholding for a rare class. The `extern`, `readonly`, and `unsafe` tests, and the `new` fix under decision 36's standing answer, are in `rh2001-modifier-reorder-trivia.md`.

One exception is #18. The RH7109 tests `NoCodeFixWhenAConditionalGroupWrapsAnAttributeListOfTheType` and `…StandsInTheBaseListOfTheType`, and the RH7103 `[System.Serializable]` example of SHARED-ORDER-008, withhold fixes for non-layout types. They are covered only by decision 39's premise that the shape is rare, which #18 disputes. With #18's narrowing they return to `main`'s output, and nothing remains in question. No rule page changed.

### Scope
Every edit is in the components that decisions 38–40 and 42 name. The added source carries no `TODO`, no tracker number, and no run vocabulary: `git diff df9c9147..HEAD -- src | rg "^\+" | rg "TODO|#[0-9]{3,4}\b|decision|audit|M[1-4]\b"` finds nothing. Production growth this round: net +58 lines, almost all of it decision 40 (`45-implementer.md`); see #19.

### Comments (FC-031)
- **Up to date:** the summaries of `HasBackingField`, `WrapsWholeUnits`, `DeclarationHoldsConditionalDirective`, `IsWithoutBody`, `HasConditionalGroupOfOtherModifiers`, and the three `ChangesMemberOrder` overloads.
- **Stale:** `IsLayoutField`'s summary (#22).

### Failure classes dismissed
- **FC-004, FC-005, FC-019, FC-020, FC-021:** the changed guards edit nothing. Comments and directives in the modifier list are the subject of decision 40 and of #21. The member-move guard reads directives by position only.
- **FC-006, FC-024:** these are #17 (partial members). The conditional `struct`/`class` header is covered by #18's tests. `FieldOrderSetsLayout` is unchanged.
- **FC-007, FC-008, FC-009:** the action paths are unchanged. Every new withholding is decided at registration from the original document, and the `Verify` tests iterate and run Fix All.
- **FC-010 to FC-014, FC-028, FC-030:** no formatter code changed.
- **FC-018, FC-036:** RH2001's rename path is unchanged. Excluded references are still withheld by `HasExcludedCodeOccurrenceAsync` (probed).
- **FC-022:** `ParseTokens` turns a literal or comment in a branch into one token or one trivia. A comment withholds the fix, and a literal is not a modifier, so it withholds too.
- **FC-023, FC-025, FC-026, FC-029, FC-037:** no identifier, expression-bodied kind, token re-resolution, key/value layout, or text-only claim is involved.
- **FC-027:** nested types, lambdas, and accessor bodies are covered by the member-move rows of audit 5, which this round leaves unchanged. Nested reordered types and the whole type in front are rows of *Guard scope*.
- **FC-032:** the build and the test results of `46-cleaner.md` are on `fdc73e22`. Only `[skip ci]` handover commits follow, so no compiled change came after them.
- **FC-033, FC-034, FC-035:** every new test has LF and CRLF rows. The expected values come from `main` (`Verify`) or from the spec examples. The red runs before the change are recorded in `45-implementer.md`.
- **FC-001, FC-002, FC-003, FC-015, FC-016, FC-017, FC-031:** these are the findings and tables above.

### Hints
_None beyond the sections per issue._
