Role: auditor
Commit: df58320cd4a48a62639cbecc597989e553ccae96
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

- Head: df58320. `git ls-remote origin refs/heads/main` returns 4838e9a03f0736d9e4593286d0f251f3a92fb704. That matches the prompt and is an ancestor of the head.
- This is the repair delta against 8589046, the commit `16-auditor.md` audited. Commits 0730294 to df58320.
- Production: `RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider.cs` (+40/−5). It adds the new private `TupleTypeKeepsElementName`, adds one condition in `GivesInferredMemberNameAsync`, and updates two doc comments.
- Tests: 10 new tests in `RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests.cs`.
- Spec and change doc: RH2001-004 (coverage sentence), RH2001-011 (¶3 rewritten, ¶5 new, `Why`, examples), and `rh2001-rename-binding.md`.
- The diff matches handovers 17–21. No `src` file changed after 8b8b67e. The repair stayed within RH2001's fix, its spec, and its tests, so scope did not grow.
- `scripts/lane-check.sh behavior-change`: confirmed.
- Evidence: I ran `scripts/apply-fix.sh RH2001` on the head, and on `main` 4838e9a in a temporary worktree that I have since removed. LF and CRLF gave the same status for every fixture. The fixture directories are `/tmp/claude-0/-home-user-Reihitsu/4271dfb1-87ee-51f4-9b4e-ea7505ebffbd/scratchpad/fx22`, `fx22b`, and `fx22c`. The outputs are in `fx22-head.txt`, `fx22-main.txt`, and `fx22b-head.txt` in the same directory.
- I compiled every `fx22`/`fx22b` input. All of them compile except a20, which does not (CS1061). I also compiled `main`'s outputs. They compile for every member of #6 and fail for a06, a11, a14 (CS1061), and a19 (CS8516), which the head correctly withholds.

## Previous findings

| # | Status | Evidence |
|---|---|---|
| 4 | closed | Decision 19 refuses the class. g2, g5, and q1 (and the dependent g1) are withheld, are pinned by `VerifyNoCodeFixIsOffered` tests, and are stated in RH2001-011 ¶5. The change doc lists them as a rare-class refusal. ¶5 names `Sample<T> other` as keeping the fix, and that holds (c01, fixed as `other._value + _value`). The static variant `Sample<int>.Value` (c02) is withheld, as ¶5's wording covers. |
| 5 | closed | Every member #5 listed is now offered with `main`'s output: t1 to t6 (tests green, per 19 and 20). `foreach ((Value, var x) in …)` is not valid input (CS8186). The holding member `var pair = (Value, 1); return pair.Value;` stays withheld. Members #5 did not name still fail: see #6. |

## Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 6 | medium | `TupleTypeKeepsElementName` (the `GetTypeInfo(tuple).ConvertedType` clause and its `<returns>` doc); RH2001-011 ¶3 and its `Why`; change doc, inferred-name bullet and `Why` ("withholding it is a fix against SHARED-FIX-003") | FC-001, FC-002 | #5. It is the same decision (does a tuple element's inferred name survive the rename observably) in the same guard, and it is too wide in the same way: the fix is withheld where `main`'s fix compiles and changes nothing at run time. | PR-introduced | The clause reads the converted type of the innermost tuple. The compiler records a conversion on the enclosing node instead. Two groups of members fail. **(A) Spec says offered, code withholds.** `main` offers each one and its output compiles. a07 is a tuple nested in a tuple converted to a type without names, `((int, int), int) pair = ((Value, 1), 2);`. a08 is a tuple nested in the source of a deconstruction, `var ((a, b), c) = ((Value, 1), 2);`. a15 is a cast operand, `return ((int, int))(Value, 1);`. b02 is a conditional whose branches carry the same name, returned as `(int, int)`: `return flag ? (Value, 1) : (Value, 2);`. b03 is a switch-expression arm returned as `(int, int)`: `1 => (Value, 1)`. RH2001-011 ¶3 keeps the fix "where it is converted to a type whose element at that position has no name" and for "the source of a deconstruction". The `<returns>` doc of `TupleTypeKeepsElementName` makes the same claim. **(B) Spec and code withhold, but the stated reason does not hold.** In a01 `(Value, input) switch { (1, 2) => … }`, a02 `switch (Value, input)`, a03 `(Value, input) is (1, 2)`, a04 `(Value, input) == (1, 2)`, and a05 `$"{(Value, 1)}"`, the tuple keeps its natural type, but no code reads the name. In a16 `(int Value, int Other) pair = (Value, 1);`, the target type supplies the name. `main`'s output compiles, and no behavior changes, because tuple element names are compile-time only. So SHARED-FIX-003 does not demand withholding, although ¶3 and the change doc file it under SHARED-FIX-003. **Holding:** the deconstruction targets t1, t5, a13 (chained), and a18 (nested). Also holding: a09 (lambda body), a10 (collection initializer), a12 (`(int, int)?` target), a17 (`object` target), b01 (parenthesized return), b04 (`yield return`), b05 (collection expression), b06 (expression body), b07 (`??` operand), the natural-type reads a06, a11, a14 (generic inference), a19 (named positional subpattern, CS8516 on `main`), and the existing (e). | Refuse this class. Keep the fix withheld for these members. In RH2001-011 ¶3, and in the change doc as a rare-class refusal (`AGENTS.md`, *Code-fix coverage*) rather than under SHARED-FIX-003, state that the fix stays offered only where the tuple itself is a deconstruction target (nested included) or the tuple's own conversion drops the name, and that it is withheld everywhere else. Name a tuple nested in another tuple, a cast operand, a conditional branch or switch-expression arm, and a natural-type tuple whose name no code reads. Align the `<returns>` doc of `TupleTypeKeepsElementName` with that rule. If the lead judges the tuple switch on a property (a01, a02) to be commonly written code, that member is escalation 9. |
| 7 | medium | `RH2001…AnalyzerTests.cs` (the round's new tests) | FC-003, FC-017 | – | same requirement | Two boundaries that the round's spec text and code state have no test. (i) The `while (outermostTuple.Parent is ArgumentSyntax { Parent: TupleExpressionSyntax … })` walk in `TupleTypeKeepsElementName` serves only a tuple nested in a deconstruction target. RH2001-011 ¶3 states it ("also as a tuple nested in one"). No test has a nested target: `rg` finds no `((Value` or `((Name` target in the test class. Deleting the loop would withhold `((Value, other), x) = source;` (a18, offered on the head and on `main`), and every test would stay green. (ii) RH2001-011 ¶5 states that a reference through the containing type's own construction (`Sample<T> other`) keeps the fix. That is the offered side of g2. No test pins it, and the class has no test of an offered fix inside a generic containing type at all. A stricter symbol comparison would withhold every such fix without failing a test. | Add `Verify` tests with `main`'s exact output for a18 (`((_value, other), x) = source;`) and c01 (`other._value + _value` in `Sample<T>`), both tagged RH2001-011. |

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Is the tuple a deconstruction target (`TupleTypeKeepsElementName`, first clause)? | The tuple nesting is walked up through `ArgumentSyntax`/`TupleExpressionSyntax`. The clause then checks whether the outermost tuple is the `Left` of an `AssignmentExpressionSyntax` | The compiler's deconstruction assignment. A parenthesized tuple is not a valid target, and a `foreach` target must declare its variables (CS8186) | Agree. t1, t5, a13, and a18 are offered | holds; nested side untested (#7) |
| Does the tuple's type keep the inferred name (second clause)? | `GetTypeInfo(tuple).ConvertedType` of the innermost tuple, and the element name at the argument's position | The conversion the compiler applies. It is recorded on the enclosing tuple (a07, a08), the cast (a15), the conditional (b02), or the switch expression (b03), not on the inner tuple. The observable use of the name is a member access, a named subpattern, or generic inference that flows out | Too wide. The span is the inner tuple, but the conversion sits on its parent. The natural type is kept where nothing reads the name (a01–a05) | #6 |
| Which arm applies (`expression.Parent is not ArgumentSyntax`)? | The parent of the scanned expression | The two `switch` arms of the scan: an anonymous declarator's expression has the declarator as its parent, and a tuple element's expression has its `ArgumentSyntax` | Agree | holds |
| Rare shapes of decision 19 (comparison, `DescribeQualifier`) | Unchanged from `16-auditor.md` | — | Withheld by design, as RH2001-011 ¶5 states | holds |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Whether a tuple keeps an element name | `rg -n "TupleElements\|IsTupleType\|TryGetInferredMemberName" src --glob '!*Test*'` finds only the RH2001 provider (lines 219, 260–262) | one owner; holds |
| Inferred anonymous member names | `rg -n "InferredMemberName\|AnonymousObjectMemberDeclaratorSyntax" src --glob '!*Test*/**'` finds the RH2001 provider and `RH3002…Analyzer.cs:490`. RH3002 asks a different question (parentheses) | holds |
| Whether a rename may be applied | Unchanged: `CasingCodeFixProviderBase<T>.RenameWithoutConflictAsync` and the RH2001 provider. The FC-015 follow-up stays in `00-lead.md` | accepted by decisions 3, 9, 22 |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| g2, g5, g1, q1 (`VerifyNoFixIsOffered…`) | Diagnostic reported, no action registered | An action is registered | `VerifyNoCodeFixIsOffered` (`Assert.IsEmpty(actions)`). Each is red on `main` (18) | holds |
| t1 to t6 (`VerifyFixIsOffered…`) | Offered, with `main`'s exact output. t1 has two diagnostics, so the batch path runs | Withheld, or other text | `Verify` with exact result text and re-analysis. Each was red on 3593829 and is green on `main` (18) | holds |
| Missing | — | — | — | The nested deconstruction target and the own-construction generic offered side (#7). #6 (A) members need no test under a refusal; they would need one under a narrowing |

## Visible output

- t1 to t6: offered with `main`'s output, which unchanged SHARED-FIX-003 leaves offered. This follows decision 20.
- g1: withheld. Unchanged SHARED-FIX-003 demands it (`main`'s output is CS1061).
- g2, g5, q1: withheld as a rare-class refusal (decision 19). The change doc lists them under *Visible output changes*.
- #6: these offered-to-withheld changes have no test. They are covered only if the lead classes them as rare. The change doc files them under SHARED-FIX-003, which does not demand them.
- No rule page changed.

## Failure classes dismissed

- FC-004, FC-005, FC-019, FC-020, FC-021: the round changed no trivia handling. `TryCreateFieldDeclaration` and the guards of RH2001-007/009/010 are unchanged.
- FC-006: the round only withholds or keeps `main`'s offered output. Every offered fixture output compiles (`fx22-out-head`). Each fixture that `main` breaks (a06, a11, a14, a19) is withheld.
- FC-007, FC-008, FC-009: the guard runs per diagnostic at registration. t1 runs Fix All over two diagnostics.
- FC-010 to FC-014, FC-028, FC-038 to FC-043: no formatter phase, precheck, or formatter call changed.
- FC-015: see *Policy ownership*. There is no new copy.
- FC-016: see #6. I enumerated the sibling positions of a tuple: return, local, argument, cast, conditional, switch arm, nested, lambda, `yield`, collection, `??`, `object`, nullable, switch, `is`, `==`, interpolation, and generic inference.
- FC-018: the fix still uses `Renamer.RenameSymbolAsync`.
- FC-022, FC-023: no literal is touched. An interpolation hole is a01–a05's group (a05).
- FC-024: partial parts go through the unchanged `GetReferringDocuments`.
- FC-025, FC-026: the expression-bodied member is checked (b06, offered). No token-position logic changed.
- FC-027: the enclosing scopes of tuples are #6.
- FC-029, FC-030: not involved.
- FC-031: the summaries and remarks of `RenameChangesBindingAsync` and `GivesInferredMemberNameAsync` match the code. The `<returns>` of `TupleTypeKeepsElementName` overclaims (#6). There is no `TODO` and no tracker number.
- FC-032: the cleaner's Release build is at ffeb7a6/1f5ce36, after the last `src` commit 8b8b67e.
- FC-033, FC-034: apply-fix gave the same results under LF and CRLF. The tests use the reported shapes.
- FC-035: the expected values were measured on `main` (18), and I re-measured them here.
- FC-036: tuple element names are compile-time only (`ValueTuple` fields). Anonymous members are observable through reflection, so withholding them stays justified.
- FC-037: there is no text-only claim.

## Architecture findings

#6 repeats the class of #5. The component is `RH2001…CodeFixProvider.GivesInferredMemberNameAsync` with `TupleTypeKeepsElementName`, and the decision is whether renaming a tuple element's inferred name changes anything. Both rounds answered from a syntactic or local-type view of one tuple. Round 1 withheld every tuple element. Round 2 reads the innermost tuple's own converted type. But where the name survives depends on conversions recorded on enclosing nodes and on whether any code reads the name. Inputs: a07, a08, a15, b02, b03 (code against spec) and a01–a05, a16 (stated reason does not hold). The lead decides under `README.md`, *Architecture findings*, and refuses the class first. #7 has no structural cause.

## Hints

- a20 (`flag ? (Value, 1) : (2, 3)`, branches with different names) is offered. The conditional's type has no names, so this is correct. The fixture's `pair.Value` does not compile on the input either, so it is not a finding.
- The validator should still confirm the analyzer project's full test count in one run (19 reports 9952 across namespace parts after an OOM kill).
