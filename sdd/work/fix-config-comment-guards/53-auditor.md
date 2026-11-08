Role: auditor
Commit: 9d9c536a
Verdict: #1595: findings (shared guard, #25); #1593: findings; #1616: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict. It named the work: audit 7, the repair delta of repair round 3 (`051f337f..9d9c536a`), checked against audit 6 (`48-auditor.md`), decisions 46–52 and batch 7, and *Special cases do not grow the source* (`AGENTS.md`).

State verified:

- The checkout was `9d9c536a` when the audit started. The parallel spec checker then committed `e76e63f6`, which adds only `52-spec-checker.md` and a line in `00-lead.md` (`[skip ci]`). The compiled content is identical, so the audit stands for `e76e63f6` too.
- `origin/main` is `3578f98e85661df7824399a6ce0514afff415a2a`, an ancestor of the head.
- `scripts/lane-check.sh behavior-change`: confirmed.
- The diff matches handovers 49–51. Production code changed only in `src/Reihitsu.Core/OrderingDeclarationUtilities.cs` (decisions 46, 47, 49) and in `RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider.cs` (decision 48). Tests changed in the RH2001, RH7103, RH7109, and RH7309 classes. The specs and change docs changed as `49-spec-author.md` states. `6cb404d2..HEAD` touches only `sdd/work/`.
- The round did not grow into an unrelated surface, so this is a delta audit.

Method:

- Static tracing of every changed guard against the specs and its counterparts.
- `scripts/apply-fix.sh` on temporary worktrees of `origin/main`, the previous head `051f337f`, and the head, under LF and CRLF.
- 14 fixtures: RH2001 r1–r8, RH7309 i0/i1, and RH7109 p0/p1. Worktrees and fixtures lived in the scratchpad and have been removed.

## thoenissen/Reihitsu#1595

### Scope
Repair delta. No RH7102 code of its own changed. Decisions 46 and 47 reach RH7102 through `TypeMemberOrderingCodeFixProviderBase.IsMoveSafe` → `OrderingDeclarationUtilities.ChangesMemberOrder`. #25 applies to RH7102's moves as well. Its RH7102 member, a constant moved across instance fields of a partial type, cannot reorder two instance fields, so RH7102 is not affected in practice. The finding is listed under #1593.

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| — | — | — | — | — | — | No finding of its own. The shared guard's findings are under #1593 | — |

### Guard scope
_N/A — no RH7102-specific guard changed._

### Policy ownership
_N/A — unchanged._

### Assertion adequacy
_N/A — no RH7102 test added or changed (`50-test-implementer.md`)._

### Failure classes dismissed
See *Whole run*.

### Architecture findings
_None of its own._

### Hints
_None._

## thoenissen/Reihitsu#1593

### Scope
Repair delta for decisions 46, 47 and 49:

- `HasBackingField` without the partial exemption;
- `DeclarationHoldsConditionalDirective` moved from the gate into the layout flag of the private rank overload of `ChangesMemberOrder`;
- the summaries;
- the RH7103, RH7109 and RH7309 tests.

### Previous findings
| # | Status | Evidence |
|---|---|---|
| 17 | closed | `HasBackingField` (`:531-541`) has no `PartialKeyword` branch: `rg PartialKeyword` in the file finds nothing. Members 7 and 8 of #17's enumeration are pinned by `RH7309…NoCodeFixWhenTheImplementingDeclarationOfAPartialPropertyOfAStructHasAnAccessorWithoutABody…`, both red before the change. The defining declaration counts by decision 46 (`…DefiningDeclaration…StandsInTheCrossedBlock`, red before). The all-bodied implementing declaration keeps `main`'s fix (`Verify`). Every member of the enumeration now either holds or over-counts, which is the safe side |
| 18 | closed | Plain classes regain `main`'s fix for moves that reorder no instance field. The four new RH7103 and RH7109 `Fix…` tests were red before the change and use `main`'s output. The test implementer is right that audit 6 overstated the remedy: the two RH7109 tests on a plain class move instance fields and stay withheld under decision 47's form, and batch 7 (A) decides exactly that. A member of the same decision that #18's enumeration did not name now fails in the other direction: #25 |
| 20 | closed | `RH7103…NoCodeFixWhenAnActiveGroupOfWholeAccessorsOfACrossedPropertyOfAClassHoldsAnAccessorWithoutABody` has audit 6's input byte for byte. Audit 6 showed that the mutant (`branchAccessors` without the active accessors) fixes exactly this input with `apply-fix.sh`, so the test kills it. The evidence was not re-run |
| 22 | closed | `IsLayoutField`'s summary (`:483-484`) now points to `HasBackingField` |

### Decision 34 and *Special cases do not grow the source*
`OrderingDeclarationUtilities.cs` changed by +22/−26 lines, and its code shrank:

- `HasBackingField` is now one expression.
- The gate lost one condition.
- The layout flag gained one disjunct.

Nothing evaluates a condition, combines branches, or adds a reader. Decision 46 trades precision for a refusal, as the rule asks. Within decision 34.

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 25 | high | `src/Reihitsu.Core/OrderingDeclarationUtilities.cs:447` (`DeclarationHoldsConditionalDirective(typeDeclaration)` reads only the reordered declaration); counterpart `FieldOrderSetsLayout` (`:451-477`, "the attributes of every part of a partial type count"); SHARED-ORDER-008, type-declaration paragraph | FC-002, FC-024 | #18 | same requirement | **A conditional layout attribute on another part of a partial type does not make the type layout-sensitive, and the fix reorders its fields.** The decision is whether a conditional header makes the type's layout depend on the build. The components are the same two as in #18 (`DeclarationHoldsConditionalDirective` and the layout decision), and this time the check is too narrow. Fixture: part 1 is `#if NET [StructLayout(LayoutKind.Sequential)] #endif public partial class C { }`, and part 2 is `public partial class C { private int _a; private readonly int _c; public C(int c) { _c = c; } }`. RH7109 on the head **fixes** it, under LF and CRLF. The fix moves `_c` in front of `_a`, which reorders the sequential layout of `C` in every build that defines `NET` (SHARED-ORDER-007, SHARED-FIX-003 *Build configurations*). `main` fixes it too. The single-declaration counterpart (p0, the same attribute on a non-partial `C`) is **no-fix-offered** on the head, as decision 47 and batch 7 intend. Enumeration over which declaration holds the directive × directive: (1) the reordered part holds it: holds (p0). (2) **Another part holds a conditional `[StructLayout(Sequential)]` on a class: fails** (p1). (3) **Another part of a struct holds an active `#if !X [StructLayout(LayoutKind.Auto)] #endif`: fails** (traced). The analyzed build sees `Auto`, so the field order does not set the layout, while a build with `X` has sequential layout. (4) Another part of a struct holds an excluded `[StructLayout(LayoutKind.Auto)]`: holds, because the struct already counts. (5) A conditional `struct`/`class` keyword: holds, because every part has to carry it, the reordered part included. The lead dropped this candidate in decision 44 as "unverified". It is now verified. The class is rare | Refuse the class with the same position check, run on every part. In the two public overloads, which hold the symbol, pass `FieldOrderSetsLayout(typeSymbol) \|\| typeSymbol.DeclaringSyntaxReferences.Select(reference => reference.GetSyntax()).OfType<TypeDeclarationSyntax>().Any(DeclarationHoldsConditionalDirective)` as the flag, and drop the call from `:447`. This adds no new reading. Make SHARED-ORDER-008 say "a declaration of the type". Add p1 as an RH7109 `VerifyNoCodeFixIsOffered` test, LF and CRLF |
| 26 | low | `OrderingDeclarationUtilities.cs:333-347` (`ChangesInitializerExecutionOrder` summary) | FC-031 | – | PR-introduced | The summary says the method ranks "without the layout check that only the type's symbol decides". Since decision 47 the private overload ORs in `DeclarationHoldsConditionalDirective`, so with the flag `false` this method still refuses instance-field reorders of a type with a conditional header. `51-implementer.md` says the opposite ("asks no layout question"). The method is internal, and only Core tests call it. The finding is comment-only | Say that a type whose declaration holds a conditional directive still gets the layout check, or pass the header check in from the public overloads (#25's change does that, which keeps this method free of layout) |

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Backing field of a property (layout count, decision 46) | `abstract`/`extern` modifiers, then `AccessorList.Accessors.Any(IsWithoutBody)`, then `field` tokens of the declaration | SHARED-ORDER-007 as changed; the compiler's rules for semi-auto and partial properties | They agree with the spec. The code can count more fields than the compiler declares, never fewer. Interface properties are abstract without the keyword and still count, but no fix can reorder two of them: RH7309 does not analyze interfaces, and the RH7102 and RH7103 moves cross instance members without reordering them (i0/i1: no diagnostic) | holds |
| Conditional header → type counts as layout-sensitive (decision 47) | Unchanged: the leading trivia of the reordered declaration's tokens up to `{`, and the first-token arm with the whole-member parse | `FieldOrderSetsLayout` (attributes of every part); SHARED-ORDER-008 type-declaration paragraph | The consequence now matches the spec: only reorders of instance fields are refused. The span covers one part, while the counterpart covers every part | #25 |
| *No backing field from a branch* | Unchanged since audit 6 | — | — | holds; its active half is now pinned (#20 closed) |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| "A conditional header makes the layout build-dependent" | `rg -n DeclarationHoldsConditionalDirective src -g '!*Test*'`: the definition (`:721`) and one call (`:447`) | One owner. Its span is narrower than `FieldOrderSetsLayout`'s, which is #25 |
| "An accessor without a body gives a backing field" | `rg -n "IsWithoutBody\|HasBackingField" src -g '!*Test*'`: `IsWithoutBody`, used by `HasBackingField` and `WrapsWholeUnits` | One owner |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH7309 `…ImplementingDeclaration…AccessorWithoutABody…` (×2), `…DefiningDeclaration…StandsInTheCrossedBlock` | Partial declarations count by shape | An action | `VerifyNoCodeFixIsOffered` | adequate (red before) |
| RH7309 `CodeFixMovesABlockOfAStructAcrossTheImplementingDeclaration…AllHaveABody` | The all-bodied side keeps the fix | A missing action, or any byte | `Verify` (`main`'s output) | adequate |
| RH7103 `NoCodeFixWhenAnActiveGroupOfWholeAccessors…OfAClass…` | Active half of the clause, with no layout mask | An action | `VerifyNoCodeFixIsOffered` | adequate (audit 6's mutant fixes this input) |
| RH7103 `FixMovesAStaticMethodOfAClassWithAConditionalAttributeList`, `…ConditionalEntryInItsBaseList`; RH7109 `FixMovesAStaticFieldOfAClassWhose…` (×2) | A header directive refuses no move that reorders no instance field | A missing action, or any byte | `Verify` (`main`'s output) | adequate (red before; they kill a revert to the gate) |
| RH7109/RH7309 decision-39 tests (re-pointed summaries) | A header directive plus an instance-field reorder is refused | An action | `VerifyNoCodeFixIsOffered` | adequate (they kill dropping the disjunct). No test covers another partial part (#25) |

### Failure classes dismissed
See *Whole run*.

### Architecture findings
#25 repeats #18: the same decision fails between the same components (`DeclarationHoldsConditionalDirective` against the layout decision that `FieldOrderSetsLayout` makes from every part), this time on the part axis. The structural cause is that the layout question is answered by two predicates with different spans: one reads the symbol across all parts, and one reads the syntax of a single part. #25's change gives both the same scope, every declaration of the symbol.

### Hints
_None._

## thoenissen/Reihitsu#1616

### Scope
Repair delta for decision 48:

- `HasConditionalDirectiveBetweenModifiers` in place of `HasConditionalGroupOfOtherModifiers`;
- RH2001-009 as changed;
- the reverted SHARED-FIX-009 amendment;
- the RH2001 tests.

### Previous findings
| # | Status | Evidence |
|---|---|---|
| 19 | closed | The branch reading, the keyword set, and the comment scan are deleted. `rg -n "ParseTokens\|ParseInTypeBody" src -g '!*Test*'` finds only `ParseInTypeBody` and the `ParseTokens` calls in `ExcludedCodeUtilities` and `TokenSeparationUtilities`, both already on `main`. The SHARED-FIX-009 amendment is reverted. The guard's new boundary (`Modifiers.Count > 0`, `Skip(1)`) leaves members of the class unguarded, which #23 and #24 report |
| 21 | superseded | Decision 48 removed the mechanism whose conditions #21 named |

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 23 | high | `src/Reihitsu.Analyzer.CodeFixes/Rules/Design/RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider.cs:247` (`node.Modifiers.Count > 0 &&`); RH2001-009 ("of the type when a modifier stands in front of it") | FC-001, FC-024 | #19 | PR-introduced | **A property with no modifier gets no check, so a conditional modifier in front of its type is converted again, which this round started.** The decision is which modifiers the field carries in each build, between the RH2001 fix and the excluded branches, as in #19. Decision 48 added `Count > 0`. Before this round, `Skip(1)` over no modifiers still inspected the type's leading trivia, which for such a property is its first token. r3 is `#if DEBUG` / `extern` / `#endif` / `int Value { get; }` in a class. It is **no-fix-offered** on `main` and on `051f337f`, and **fixed** on the head (LF and CRLF) to `#if DEBUG extern #endif readonly int _value;`. A build with `DEBUG` then declares an `extern` field (CS0106, SHARED-FIX-003 *Build configurations*). Members: (r3) get-only with `extern`: **fails**, regressed this round. (r4) `{ get; set; }` with `extern`: **fails** (`extern int _value;`). It was withheld on `051f337f`, and `main` fixes it the same broken way. (r5) get-only with `public` in the group: **fixed** on the head and on `051f337f`, while `main` withholds it. In a build with `DEBUG`, a public property becomes a public field with another name. (r6) a group of a whole member above it: fixed on the head, which is correct in every build; `main` and `051f337f` withhold it. Holding members: a directive between two modifiers, and one between the last modifier and the type of a property that has a modifier (the existing tests) | Refuse the class. Drop `node.Modifiers.Count > 0`, so that the first token of a property without modifiers, its type, is checked. A property without an accessibility modifier already violates RH2004, so the class is rare by definition and the refusal raises no question. It also withholds r6, as `main` does. Reword RH2001-009 to match. Add r3 as a `VerifyNoCodeFixIsOffered` test, LF and CRLF |
| 24 | high | Same method, `.Skip(1)`: the leading trivia of the first modifier is not inspected; RH2001-009 ("A conditional directive in front of the property's first token is not covered … RH2001-004 states the fix"); RH2001-004 (the directives above stay in front of the field) | FC-001, FC-015, FC-024 | #19 | same requirement | **A conditional group that ends directly in front of the first modifier, and holds a modifier, is converted, and the result breaks another build.** r1 is `#if DEBUG` / `extern` / `#endif` / `private int Value { get; }`. It is fixed on `main`, `051f337f`, and the head to `#if DEBUG extern #endif private readonly int _value;`, and in a build with `DEBUG` that is an `extern` field (CS0106). r2 is the same with `readonly` in a struct: fixed everywhere, and the `DEBUG` build has `readonly` twice (CS1004). These are the hazards that RH2001-009's own Why names. The run's new text sends this position to RH2001-004, whose output violates SHARED-FIX-003 there. The lead dropped this candidate in decision 44 as "unverified", and it is now verified. The same question, whether a group in front of a declaration's first token holds whole members or part of the declaration, is answered by `DeclarationHoldsConditionalDirective`'s first-token arm for a type declaration: it refuses when an excluded branch does not read as whole members. The RH2001 fix does not ask it (FC-015). The cheap position refusal is "any conditional directive in the leading trivia of the first modifier". It would also withhold r7 (a `#if DEBUG private int _trace; #endif` group right above `private static int Value { get; }`) and r8 (the same above `private int Value { get; set; }`). `main` and the head fix both, and the RH2001-009 counterpart example is of that kind. An auto-property directly below an `#endif` is ordinary code | Two options, for the lead. (A) The position refusal above, with the RH2001-009 counterpart example turned into no-fix. Because it withholds common code that `main` fixes, it is the maintainer's question (last bullet of *Special cases do not grow the source*). (B) Refuse only when an excluded branch in front of the first modifier does not read as whole members, through the existing `ReadsAsWholeUnits` reading that SHARED-FIX-009 permits and `DeclarationHoldsConditionalDirective` already uses. This needs that reading reachable from the code-fix assembly. Pin r1 and r2 with `VerifyNoCodeFixIsOffered`, and r7 with `Verify` |

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| RH2001: a conditional directive around the modifiers withholds the fix (decision 48) | The leading trivia of `Modifiers[1..]` and of the type, only when the property has a modifier; the trivia kinds `#if`/`#elif`/`#else`/`#endif` | RH2001-009; SHARED-FIX-003 *Build configurations*; the first-token arm of `DeclarationHoldsConditionalDirective` (the same question for a type declaration) | Between modifiers and in front of the type behind a modifier: they agree, and the existing tests kill dropping either arm. At the property's first token they disagree. The arm for a property without modifiers was removed this round (#23). The first modifier was never inspected (#24). `#elif` in the pattern is redundant, since it always shares a trivia list with an `#else`/`#endif` or an `#if` in the span, but it is harmless and the spec names it | #23, #24 |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Reading excluded text | `ParseInTypeBody` (member moves); `ParseTokens` in `ExcludedCodeUtilities` (names) and `TokenSeparationUtilities`, both already on `main` | The RH2001 reader is gone (#19 closed) |
| Conditional-directive kind | `OrderingDeclarationUtilities.IsConditionalDirective`, `SyntaxTriviaUtilities.IsConditionalDirective` (private), `RH7004…CodeFixProvider.cs:130`, and the RH2001 trivia-kind pattern (`:252`) | The same four kinds everywhere, as audit 6 found. Acceptable |
| Whether a group in front of a declaration's first token holds whole members | `DeclarationHoldsConditionalDirective` (type declarations, with the whole-member parse); the RH2001 fix does not ask it | Two answers to one question: #24 |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `VerifyNoCodeFixIsOfferedForAConditionalModifierBetweenTheModifiersAndTheType` | Type arm | An action | `VerifyNoCodeFixIsOffered` | adequate (red before; kills dropping `Concat(Type…)`) |
| `VerifyNoCodeFixIsOfferedForAConditionalGroupWithADirectiveBetweenTwoModifiers` | Modifier arm | An action | `VerifyNoCodeFixIsOffered` | adequate (red before; kills dropping the `Skip(1)` arm) |
| `VerifyFixKeepsAConditionalGroupInFrontOfTheFirstModifier` | A whole-member group above keeps the fix | A missing action, or any byte | `Verify` | adequate (kills `Skip(0)`). It is also the input that option A of #24 would turn into no-fix |
| `extern`, `readonly`, `unsafe` tests | The general refusal | An action | `VerifyNoCodeFixIsOffered` | adequate |
| (none) | A property without modifiers, with a group in front of its type | — | — | **missing**: #23 (and #24 for the first modifier) |

### Failure classes dismissed
See *Whole run*.

### Architecture findings
#23 and #24 repeat #19's class: which modifiers the field carries in another build. Each round placed the guard's boundary at a different spot in the modifier list: between modifiers (decision 36), a keyword reading (decision 40), and a position check from the second modifier on (decision 48). Every time, the property's first token has stayed outside it. The structural cause is the boundary rule "a directive in front of the first token belongs to RH2001-004". It is the boundary the member-move guard settled for type declarations with a whole-member test (SHARED-ORDER-008, first-token arm), and RH2001 settles it without that test.

### Hints
_None._

## Whole run

### Findings
#23–#26 are listed under their issues. No finding is confined to the whole run.

### Visible output is decided
Every expected value that this round adds or changes is covered:

- decision 46: the partial RH7309 tests, listed in `member-move-conditional-groups.md`;
- decision 47 with batch 7 (A): the instance-field withholdings on plain classes, and the four `Fix…` tests with `main`'s output;
- decision 48: the RH2001 withholdings, a rare class withheld under the standing answer, listed in `rh2001-modifier-reorder-trivia.md`.

The `VerifyFixKeepsAConditionalGroupInFrontOfTheFirstModifier` output is RH2001-009's counterpart example; `main`'s fix for that shape is r7. No rule page changed. If the lead takes option A of #24, that output becomes a question.

### Scope
Every edit is within decisions 46–49. `git diff 051f337f..HEAD -- src | rg "^\+" | rg "TODO|#[0-9]{3,4}\b|decision|audit"` finds nothing. Production code shrank: net −24 lines against `051f337f`, and +550 against `main`.

### Comments (FC-031)
- **Up to date:**
  - `HasBackingField`, `IsLayoutField`, and `DeclarationHoldsConditionalDirective`;
  - the three `ChangesMemberOrder` summaries;
  - `HasConditionalDirectiveBetweenModifiers` and the `TryCreateFieldDeclaration` remarks.
- **Stale:** `ChangesInitializerExecutionOrder` (#26).

### Failure classes dismissed
- **FC-004, FC-005, FC-019, FC-020, FC-021:** the changed guards only withhold or allow. They write no trivia. Comments and non-conditional directives in the modifier list keep the existing `TryWriteModifiersInOrder` path, which this round left unchanged.
- **FC-006:** partial types and members are the subject of #17 (closed) and #25.
- **FC-007, FC-008, FC-009:** the action paths are unchanged. Every new withholding is decided at registration, and the `Verify` tests iterate and run Fix All.
- **FC-010 to FC-014, FC-028, FC-030:** no formatter code changed.
- **FC-018, FC-036:** RH2001's rename path is unchanged. Excluded references are still withheld by `HasExcludedCodeOccurrenceAsync`.
- **FC-022:** this round reads no excluded text.
- **FC-023, FC-025, FC-026, FC-029, FC-037:** no identifier, expression-bodied kind, token re-resolution, key/value layout, or text-only claim is involved.
- **FC-027:** nested types are unchanged since audit 6. Only the reordered declaration's own header is read, which is #25's axis.
- **FC-032:** the implementer ran the build and the tests on `6cb404d2`. Only handover commits follow (`[skip ci]`), and decision 52 records why the cleaner was skipped.
- **FC-033, FC-034, FC-035:** every new and changed test has an LF and a CRLF row. The expected values come from `main` or from the spec examples, and the red runs before the change are recorded in `51-implementer.md`.
- **FC-001, FC-002, FC-003, FC-015, FC-016, FC-017, FC-024, FC-031:** these are the findings and tables above.

### Hints
_None beyond the sections per issue._
