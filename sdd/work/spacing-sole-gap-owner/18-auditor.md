Role: auditor
Commit: b19d570
Verdict: findings

## Scope

Repair-round-2 delta audit of thoenissen/Reihitsu#1493, `git diff b33090d b19d570`. The head is `b19d570465544a7c2637d18f3a6fdca483799041` on `claude/focused-brown-4e3k0u`. `git ls-remote origin refs/heads/main` returns `e3842c855ff487ae71a403d50dcd6613fb6d8194`, and `git merge-base --is-ancestor` confirms that the branch contains it. `scripts/lane-check.sh behavior-change` confirms the claimed lane. The prompt carried no intended fix, suspected cause, or hoped-for verdict.

The repair diff matches handovers `14` to `17`:
- FMT-SPACE-021's scope sentence becomes an exhaustive list, with a second example;
- change-doc entry 6 gains a before/after;
- `DeclaredNameSpacingRule`'s summary is corrected (comment only; the cleaner's `verify-text-only.sh` passed);
- eleven full-pipeline tests are added to `DeclaredNameSpacingTests`;
- a UTF-8 BOM is added to `RightCarriedDocumentationCommentSpacingTests.cs`, and I confirmed its first bytes are `EF BB BF`.

No production code changed, and the scope did not grow. Evidence from `13-auditor.md` is reused for everything outside the delta.

### Previous findings

- **#1 — closed.** Each member of the class that #1 listed is now covered in the spec's list, in change-doc entry 6 (or the spec's second example), in the rule summary, and in a test. The tests are `AssertRuleResult`, which compares exact text under LF and CRLF, runs a second pass, and checks phase stability:
  - field: `FieldNamesGetOneSpaceBehindTheirType`, plus `ConstFieldNameGetsOneSpaceBehindItsType` and `FixedSizeBufferNameGetsOneSpaceBehindItsType`;
  - event field: `EventFieldNamesGetOneSpaceBehindTheirType`;
  - `fixed`: `FixedStatementNamesGetOneSpaceBehindTheirType`;
  - `for`: `ForInitializerNamesGetOneSpaceBehindTheirType`;
  - `using` statement: `UsingStatementNameGetsOneSpaceBehindItsType`;
  - `using` declaration: `UsingDeclarationNamesGetOneSpaceBehindTheirType`.

  The members that #1 said hold still hold. The spec and the rule summary both name the property, indexer, method, local-function, and delegate types as unspecified, and the rule has no arm for them.

  I checked the class against the code arm by arm. Roslyn gives `VariableDeclarationSyntax` six parents: field, event field, local declaration (`const`, `ref`, and `using` included), `for`, `fixed`, and `using` statement. All six are named. The designation parents are named too: declaration expression, declaration pattern, recursive pattern, and list pattern are inside the rule, and `VarPatternSyntax` and `ParenthesizedVariableDesignationSyntax` are named as outside it. No arm of `IsDeclaredName` reaches a declaration the spec leaves out, and no declaration the spec names lacks an arm.
- **Hint "one new file has no BOM": closed** (see above).

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 2 | low | `sdd/specs/formatter/horizontal-spacing.md`, FMT-SPACE-021 second example (`unsafe class C { … private fixed int  _buf[4]; … }`); `src/Reihitsu.Formatter.Test/Regression/Spacing/DeclaredNameSpacingTests.cs`, `MembersAndStatementsThatDeclareAVariableAreSpacedLikeTheSpecificationExample`, which copies the example verbatim | – (example validity; no catalog class) | – | PR-introduced | The new example puts a fixed-size buffer in a `class`, which C# rejects. I compiled the example's output in a scratch project (`net10.0`, `AllowUnsafeBlocks`) and got `error CS1642: Fixed size buffer fields may only be members of structs` at `private fixed int _buf[4];`. That was the only error. With `unsafe struct C`, the same code compiles with only nullable-context and unused-member warnings. The formatter output is unaffected, since the input parses and the formatter judges only syntax. But the example that seeds the tests, and the test copied from it, show code no user can have in a buildable project. The separate test `FixedSizeBufferNameGetsOneSpaceBehindItsType` already uses `unsafe struct S` correctly. The change-doc example has no buffer and compiles. | Spec author: change the second example's `unsafe class C` to `unsafe struct C` (or move the buffer into a separate `unsafe struct`) in both blocks. Test implementer: mirror that change in the verbatim test. The expected output does not change otherwise. |

## Guard scope
_N/A — the repair changes no predicate, guard, or exemption; `DeclaredNameSpacingRule`'s code is byte-identical apart from its summary comment. Its arm-by-arm agreement with the rewritten FMT-SPACE-021 is recorded under Previous findings._

## Policy ownership
_N/A — no policy moved; production code changed only in one summary comment. The owners recorded in `13-auditor.md` are unchanged._

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| The ten new member tests in `DeclaredNameSpacingTests` (field, `const` field, fixed-size buffer, event field, `const`/`ref` local, `using` declaration, `fixed`, `for`, `using` statement, parameter lists) | FMT-SPACE-021: exactly one space between the type and every declarator's name, for every variable-declaration parent and every parameter list | A tight (`int?_f`) or doubled (`int  _j`) gap that survives, any other changed gap, or a second pass that changes something | `AssertRuleResult`: exact text under LF and CRLF, a second pass, phase stability | Adequate. Each test exercises both sides of the rule: a type ending in `?`, `]`, `>`, `)`, or `*` that must gain a space, and a type ending in a keyword that must collapse. Multi-declarator forms pin the declarators behind the comma too. The test implementer's discrimination run shows 10 of 11 tests failing on `main`. The exception is the fixed-size buffer test, whose input FMT-SPACE-002 already collapses there. It still pins FMT-SPACE-021 at head, so it is not vacuous for the requirement, only for the `main` comparison. |
| `MembersAndStatementsThatDeclareAVariableAreSpacedLikeTheSpecificationExample` | The spec's second example | As above | `AssertRuleResult` | Adequate as an assertion; its input does not compile, see finding #2. |
| `ParameterNamesOfEveryParameterListGetOneSpaceBehindTheirType` expected operator body | A visible expected value outside FMT-SPACE-021: `operator +(…) => l;` becomes a block | — | — | Covered (check 10) by FMT-STRUCT-005, which is unchanged. Every other expected gap in the new tests is demanded by FMT-SPACE-021 under answer 7C (`00-lead.md`, row 25), or by FMT-SPACE-005 and FMT-SPACE-008, which are unchanged. |

## Failure classes dismissed

- **FC-001, FC-002, FC-003.** No guard changed. The FMT-SPACE-021 boundary is now tested on both sides for every variable-declaration parent: spaced members, and members with tight and doubled gaps. The excluded member types are deliberately left unpinned, because the spec leaves them unspecified.
- **FC-004, FC-005, FC-019, FC-020, FC-021.** The repair adds no edit. The comment tests in the gap (`CommentBehindTheTypeKeepsItsPlace`, `DocumentationCommentInFrontOfTheNameKeepsItsPlace`) reach the same single `VariableDeclaratorSyntax` arm the new members use.
- **FC-006.** The formatter output changed by nothing. Finding #2 concerns the validity of the example's input, not a rewrite.
- **FC-007, FC-008, FC-009, FC-014, FC-018, FC-030, FC-036.** No code fix, rule, or rename is touched.
- **FC-010, FC-011, FC-012.** No phase changed. `AssertRuleResult` runs phase stability on every new input.
- **FC-013, FC-032.** Production behavior is byte-identical to `d09b7fc` (comment-only delta, proven text-only). The analyzer runs from `13-auditor.md` stay valid.
- **FC-015, FC-016.** One owner. The repair closes the class rather than an instance (see Previous findings).
- **FC-017.** `AssertRuleResult` is exact, with LF and CRLF, two passes, and phase stability.
- **FC-022 to FC-029.** No literal, identifier, `partial`, expression-bodied, re-resolution, nesting, detached-node, or key/value logic changed.
- **FC-031.** The rule summary now describes the code: every listed declaration has an arm, and every excluded type has none. The inline comments made no claim about member types.
- **FC-033, FC-034.** These tests are not reproductions. Each member is tested in the shape #1 named.
- **FC-035.** The expected values come from the spec examples and from answer 7C, not from patched code. Ten of eleven fail on `main`.
- **FC-037.** The text-only claim for the summary change rests on `scripts/verify-text-only.sh` (`16-cleaner.md`), not on `grep`.

## Architecture findings

_None._ The repair only aligns the text with the single owner, `DeclaredNameSpacingRule`; no second component decides the gap.

## Hints

- **Not a finding: the parameter bullet names fewer lists than the code covers.** FMT-SPACE-021's first bullet says "a parameter that has a type, of any parameter list". It then names method, local function, constructor or primary constructor, operator, indexer, delegate, and lambda. The single `ParameterSyntax` arm also spaces the parameters of:
  - an anonymous method (`delegate(int?x) { }`);
  - a conversion operator;
  - a C# 14 extension block (`extension(string?s)`).

  "Any parameter list" already states the scope, so the boundary is not implicit, as it was in #1. If the spec author reworks the example for #2, the list could add these lists or use "such as".
- **Carried from `13-auditor.md`, unchanged:** question 15 must reach the maintainer in the run report. Both pre-existing hints still hold: the `LineBreakPhase` stability reversal on lambda arguments next to an anonymous method, and the undecided gap in front of a relational pattern's operator.
