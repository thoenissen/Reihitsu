Role: auditor
Commit: a79eb22927c131d1b6d88d811a429eaf3492450f
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict, so there was nothing to ignore.

State check:
- The checkout is `a79eb22`.
- `git ls-remote origin refs/heads/main` returns `b434355`, which is an ancestor of HEAD.
- `scripts/lane-check.sh bug-fix` confirms the lane.
- The repair diff `42db22c..a79eb22` is what handovers 11–16 describe:
  - specs RH2001 and RH2005, and `requirement-ids.txt`;
  - the test harness, and the RH2001, RH2004, RH2005, RH8306, and RH74xx tests;
  - `DeclarationModifierUtilities`, and the RH2005 and RH2001 fix providers;
  - `PartialMemberUtilities`, the RH2004 analyzer, `InterfaceImplementationUtilities`, and RH7409–RH7412.

I trusted the cleaner's build and test results (`15-cleaner.md`) and the implementer's full analyzer run (`14-implementer.md`).

Probes ran with `scripts/apply-fix.sh RH2005|RH2001 <dir>`, which runs each fixture under LF and CRLF. The fixtures are under `/tmp/claude-0/-home-user-Reihitsu/11377a95-24cc-5692-825a-5a87f974a5d7/scratchpad/audit17/`, outside the repository, and the repository tree is unchanged. "Observed" means a probe printed the result. "By trace" means it was read from the code.

## Scope

- Head `a79eb22`, base `b434355`.
- This is a repair-delta audit against `42db22c` (`10-auditor.md`).
- **Scope grew.** The repair changed the spec for Q5 and Q6: RH2005-003 is clarified, RH2005-005 is new, and RH2001-007 is widened. So the RH2005 and RH2001 fixes got a full audit against the changed requirements. #1287 and #1288 got a delta audit.

## Previous findings

| # | Status | Evidence |
|---|---|---|
| 1 | closed | Every member the finding listed now holds:<br>- The comment line, the `#pragma` line, and the `#if` block in front of a non-first `public` withhold the fix (RH2005-005). This is tested, LF and CRLF.<br>- The indentation-only line gives `private static⏎        int Counter;` (RH2005-003, Q5). This is tested, LF and CRLF.<br>- Observed with probes as well: `static volatile⏎ public int`, `static⏎ public volatile int`, an attribute list in front, and a blank line in front of `public`.<br>A member that the finding did not list fails; it is reported as #5. |
| 2 | closed | (a) A comment or directive in front of the `;` withholds the fix. (b) A comment or directive in front of the name, on a line below the type, withholds the fix. Both follow from `KeepsEveryCommentAndDirective`: the builder drops the name and the `;`, so their trivia is missing from the field, and the guard sees the difference. |
| 3 | closed | The private list of trivia kinds is gone. `GetCommentsAndDirectives` uses `SyntaxTriviaUtilities.IsCommentTrivia` and `IsDirectiveOrDisabledTextTrivia`. |
| 4 | closed | (a) RH2004 reports partial members from `OnNamedType`, which also sees the defining part of a partial constructor. (b) RH7409–RH7412 judge the interface on `GetStandInSymbol`. The tests of both cells pass, and so do the neighboring cells. |

## thoenissen/Reihitsu#967

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 5 | high | `src/Reihitsu.Core/DeclarationModifierUtilities.cs:155-172` (token-list overload: `trailingTrivia = accessibilityToken.TrailingTrivia`) with `:126-136` and `:205-237` (the repair's `CarryLeadingTriviaOfLaterAccessibilityModifiers`, which carries only the **leading** side) | FC-004, FC-016, FC-001, FC-020, FC-021 | #1 | same requirement | The removed `public` can be a modifier that is not first. Its **trailing** trivia then still goes to the new first modifier `private`, so a line break, a comment, or a directive-bearing gap that stood behind `public` moves in front of the other modifiers. This is the same decision as #1 (where the trivia of a removed non-first accessibility keyword goes), failing between the same two overloads; #1 enumerated only the leading side. Observed, identical under LF and CRLF, fix offered in every row:<br>**(f)** `static⏎        public⏎        int Counter;` gives `    private⏎static⏎                int Counter;`. This breaks RH2005-003's new sentence: `int` must stand on `public`'s line, in column 8. The repair made the output worse: on `42db22c`, by trace, `int` was at column 8 on the third line.<br>**(c)** `static public⏎#pragma warning disable CS0649⏎        int Counter;` gives `private⏎static #pragma warning disable CS0649⏎…`. The directive no longer starts its line, so the result does not compile (`invalid-result`). This breaks SHARED-FIX-003 (compile error, and a changed directive).<br>**(d)** `static public⏎#if NEVER⏎        readonly⏎#endif⏎        int Counter;` gives `private⏎static #if NEVER⏎…`. Also `invalid-result`; the same requirement is broken.<br>**(e)** `static public⏎        // note⏎        int Counter;` gives `private⏎static         // note⏎        int Counter;`. The comment line now stands behind `static`, not with the line of `int`, which SHARED-FIX-003 forbids.<br>**(b)** `static public // note⏎        int Counter;` gives `    private // note⏎static         int Counter;`. The end-of-line comment stays behind `private`, and `static` is moved off its line. SHARED-FIX-003 forbids that too.<br>**(a)** `static public⏎        int Counter;` gives `    private⏎static         int Counter;`, with `static` at column 0. No requirement decides this layout, but a repair of b–f changes it, so it is undecided output.<br>`static public /* c */ int` is follow-up candidate 5. SHARED-FIX-003 does not decide a comment between two tokens of one line.<br>Members that hold, observed: `public` first with any leading trivia; `static public int` on one line; `static volatile⏎ public int`; `static⏎ public volatile int`; `[Obsolete]⏎ static⏎ public int`; a blank line in front of a non-first `public`; and the five RH2005-005 inputs (withheld).<br>RH2004 cannot reach this, because it reports only declarations without an accessibility keyword. | Decide where both sides of a removed non-first accessibility keyword's trivia go, in one place. For example: its trailing trivia stays where it stood (on the kept token in front of it), and the new `private` gets a single space. Or withhold the fix when that trailing trivia holds a line break, a comment, or a directive, or when the token behind it starts with a directive. Members (b)–(f) are decided by RH2005-003 and SHARED-FIX-003. For (a), and for (b) and (e) if they are carried rather than withheld, the lead decides whether the output needs an output question. Add tests for (a)–(f), LF and CRLF. |

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| RH2005-005: a comment or directive in front of a later accessibility modifier (`HasCommentOrDirectiveInFrontOfLaterAccessibilityModifier`) | Leading trivia of every accessibility modifier at index ≥ 1 | `CarryLeadingTriviaOfLaterAccessibilityModifiers`, which moves exactly that span. RH2005-005 says "on a line below the modifier in front of it", and leading trivia of a non-first token always starts below that line | Agree | ok |
| Where the trivia of a removed non-first accessibility modifier goes | Leading side: `CarryLeadingTrivia…`. Trailing side: the token-list overload, which hands it to the new first token | The RH2005-005 guard inspects the leading side only, and no guard or carry covers the trailing side | Disagree: the trailing span is relocated without a guard | #5 |
| Where `private` stands (`GetFirstTokenBehindAttributeLists`) | Unchanged from `10-auditor.md` | – | – | ok |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Which modifiers the fix removes | `rg -n "IsAccessibilityModifier\(" src --glob '!*Test*'`: `DeclarationModifierUtilities` only. The new guard reuses it | one owner |
| Where the trivia of a removed accessibility keyword goes | `DeclarationModifierUtilities.AddAccessibilityModifiers(SyntaxTokenList, …)` reuses the first accessibility token's leading and trailing trivia for the new first token. `CarryLeadingTriviaOfLaterAccessibilityModifiers` moves the leading trivia of non-first tokens. The member overload overwrites the first token's leading trivia | Three statements over two methods; the trailing side is still decided by a rule that assumes the token was first | see *Architecture findings*, #5 |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `VerifyFixKeepsTheLineOfAPublicThatStartsItsOwnLineBehindAnotherModifier` (+CRLF) | RH2005-003 / Q5: the line of `public` stays, with `int` in its column | `int` at column 0, or `private` behind `static` | `Verify` exact text, iterative and Fix All | adequate |
| `VerifyNoFixIsOfferedFor{ACommentLine,ADirectiveLine}InFrontOfAPublicBehindAnotherModifier` (+CRLF), `…AnExcludedModifier…` | RH2005-005: the fix is withheld | a fix is registered | `VerifyNoCodeFixIsOffered` | adequate |
| No test for a non-first `public` that ends its line, (a)–(f) | – | – | – | missing, #5 |

## thoenissen/Reihitsu#1286

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 6 | medium | `src/Reihitsu.Analyzer.Test/Design/RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests.cs:1911` (`VerifyFixKeepsACommentLineInFrontOfTheType`) | FC-017 | – | PR-introduced | The test accepts three outcomes: withheld, `inPlace`, and `Format(inPlace)`. `inPlace` (`private⏎    // note⏎    int _value;`) is not what the formatter writes: today's output is `Format(inPlace)`, which inserts a blank line (`13-test-implementer.md`). SHARED-FIX-007 requires an offered fix to be laid out as the formatter lays it out, and the run already applies it to RH2001 (decision 21; the name-on-its-own-line tests are tagged SHARED-FIX-007). So the test passes for an output that a requirement forbids, which makes it weaker than the guarantee. Decision 22 reads only SHARED-FIX-003 as constraining this test. The sibling `VerifyFixKeepsACommentBehindTheTypeOnTheSameLine` is unaffected, because its `inPlace` is already formatter-stable. | Drop `inPlace` from the accepted set of this test, so withheld or `Format(inPlace)` remain. |

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| RH2001-007 / SHARED-FIX-003: the field loses no comment or directive (`KeepsEveryCommentAndDirective`) | The ordered texts of every comment, directive, and disabled-text trivia in `node.DescendantTrivia()`, against those of the built candidate | `TryCreateFieldDeclaration`, the builder itself: it drops the identifier, the accessor list, the `;`, and the leading trivia of `=` | Agree by construction: the guard compares the builder's output with its input. Register, apply, and the post-rename rebuild all call `TryCreateFieldDeclaration` | ok |
| RH2001-007 boundary | Observed: `= /* c */ 5` and `=⏎ // c⏎ 5` (inside the initializer) offered; `= 5; // tail`, a doc comment with `// tail`, and a multi-line accessor list with `⏎ = 5;` offered and kept; `int /* c */ Value /* c */` (equal text twice) withheld; an `#if` block behind the declaration offered and kept | RH2001-007 list and its "not covered" sentence | Agree | ok |
| The guard can't see a comment it keeps but relocates | Texts and order only, not position | SHARED-FIX-003 (moving a comment) | Not a disagreement of this repair. The one relocating path is the RH7105 reorder (follow-up candidate 4), which is outside the repair (see Hints) | ok |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Trivia kinds that count as comment or directive | `rg -n "DisabledTextTrivia\|SingleLineCommentTrivia" src/Reihitsu.Analyzer.CodeFixes/Rules/Design/RH200*.cs`: none in RH2001; it uses the `SyntaxTriviaUtilities` predicates | one owner |
| What the field loses | `TryCreateFieldDeclaration` only. The old position list is gone | one owner |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `VerifyNoFixIsOfferedFor{ACommentLine,ADirectiveLine,AConditionalBlock}InFrontOfThe{Semicolon,Name}` (+CRLF) | RH2001-007: the fix is withheld | a fix is registered | `VerifyNoCodeFixIsOffered` | adequate |
| `VerifyFixIsOfferedForASemicolonOnItsOwnLineWithoutCommentOrDirective` (+CRLF) | FC-003: a line break alone does not withhold the fix; the result is `private int _value = 5;` | withheld, or other text | `Verify` exact text | adequate |
| `VerifyFixIsOfferedForANameOnItsOwnLineWithoutCommentOrDirective` (+CRLF) | decision 21: `private int⏎    _value;` | other text | `Verify` exact text | adequate |
| `VerifyFixKeepsACommentLineInFrontOfTheType` | SHARED-FIX-003, and SHARED-FIX-007 when the fix is offered | the comment dropped or moved, or a layout that is not the formatter's | `VerifyCodeFixIsWithheldOrProducesOneOf` | weak, #6 |
| `VerifyFixKeepsACommentBehindTheTypeOnTheSameLine` | SHARED-FIX-003 | the comment dropped | same helper; `inPlace` is formatter-stable | adequate (decision 22) |
| Harness `TryApplyCodeFixAsync` / `VerifyCodeFixIsWithheldOrProducesOneOf` | `ApplyCodeFixAsync` still throws when no action is registered; the new helper re-verifies the result with `Verify`, so it has no diagnostic and no compile error | – | – | ok |

## thoenissen/Reihitsu#1287

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|

_N/A — no finding._ Traced:
- `GetReportedPart` without the constructor case gives the same result as before for every non-constructor caller (RH2002, RH8306, the RH74 bases). The old code returned the declaration exactly when the chosen part was the declaration, and so does the new code.
- RH2004's node action and `OnNamedType` are disjoint: both use the `TryGetParts` test. `OnNamedType` applies `ShouldSkip` to the defining part, which matches the per-part skip; partial methods and interface parents are skipped.
- `GetStandInSymbol` changes the judged symbol only for an implementing part whose defining part is generated. The cells where the defining part is not generated keep `main`'s characterization (decision 23).

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Which analysis reports a partial member (RH2004) | `OnNamedType`: `typeSymbol.GetMembers()` → `TryGetParts` → `SelectReportedPart` | The node action's early return on the same `TryGetParts` | Agree; one report per member | ok |
| Which symbol the interface region is judged on (`GetStandInSymbol`) | `PartialDefinitionPart` is in a generated tree (`IsGeneratedCode`) | `SelectReportedPart`'s candidate filter (the same `IsGeneratedCode`) | Agree; both exclude the generated defining part with the same predicate | ok |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Part symbols of a partial member | `rg -n "PartialDefinitionPart\|PartialImplementationPart" src --glob '!*Test*'`: `PartialMemberUtilities` only (`TryGetParts`, `GetStandInSymbol`) | one owner |
| Interface binding of a part | `rg -n "GetImplementedInterfaceName\|GetInterfaceRegionName" src --glob '!*Test*'`: RH7409–RH7412 go through `GetInterfaceRegionName`. `DocumentationAnalysisUtilities.OverridesOrImplementsMember:327-329` calls `GetImplementedInterfaceName` directly | follow-up candidate 6 (pre-existing, outside the issues); not repeated |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH2004 constructor, indexer, and event cells with a generated part, both parts generated, and both document orders | SHARED-DIAG-012: one diagnostic on the selected part | none, two, or the wrong part | `Verify` with `*.g.cs` | adequate |
| RH7401–RH7412 cells with a generated defining or implementing part | SHARED-DIAG-012 inside each rule's coverage sentence | none, or the generated part | `Verify` | adequate |

## thoenissen/Reihitsu#1288

Not touched by the repair. RH2004-003 and its tests are as audited in `10-auditor.md` (no finding).

## Whole run

### Visible output is decided
- RH2005-003 (Q5 example), RH2005-005 (Q6), and the RH2001-007 no-fix tests (Q6): covered by Maintainer answers 5A and 6A.
- RH2001 `;`-on-its-own-line: the RH2001-007 spec example.
- RH2001 name-on-its-own-line: decision 21, under SHARED-FIX-007, unchanged from `main`.
- SHARED-DIAG-012 locations: Q1 = A.
- The two multi-outcome RH2001 tests pin no new output (decision 22); #6 concerns their strength, not an undecided value.
- No rule page changed (`git diff 42db22c..a79eb22 -- documentation` is empty).

### Comments and conventions
- No `TODO`, no commented-out code, and no tracker number in an added `src/` line.
- FC-031 holds for the changed summaries:
  - `AddAccessibilityModifiers` remarks, `CarryLeadingTrivia…`, and `HasCommentOrDirective…` describe the code. The remarks do not say that the trailing trivia still goes to the new first token, which is #5's behavior.
  - `TryCreateFieldDeclaration`, `KeepsEveryCommentAndDirective`, and `IsSupportedPropertyShape` describe the code.
  - `GetReportedPart` (new contract), `GetStandInSymbol`, `OnNamedType`, and `GetInterfaceRegionName` describe the code.

### Scope
Within the four issues and decisions 16–24. The new public member `DeclarationModifierUtilities.HasCommentOrDirectiveInFrontOfLaterAccessibilityModifier` keeps the removed-modifier set with one owner (`14-implementer.md`). Lane confirmed.

### Failure classes dismissed
- **FC-002**: RH2005-005's qualifier "on a line below the modifier in front of it" is implied by leading trivia of a non-first token. RH2001-007's qualifiers follow from the builder.
- **FC-003**: both sides are tested for the RH2001 `;` and the name, and for RH2005 `public` first versus not first. The failing side of #5 is a different axis.
- **FC-005**: comment, `#pragma`, and `#if` are tested separately for RH2005-005 and RH2001-007.
- **FC-006**: no new rewrite. The RH2004 symbol action changes only the reporting path.
- **FC-007, FC-009**: the probes converge in one iteration. RH2001 rechecks the guard on apply. RH2005 registers only after the guard.
- **FC-008**: both fixes use BatchFixer over registered actions only, and the guards run at registration.
- **FC-010, FC-011, FC-012, FC-013, FC-014, FC-028, FC-029, FC-032**: no formatter or layout-rule change.
- **FC-015**: one owner per changed policy, except the trivia of a removed keyword (#5).
- **FC-018, FC-036**: the RH2001 rename path is unchanged. `Renamer` default options do not touch comment text, and the guard compares node and candidate in the same (renamed) tree.
- **FC-019, FC-020, FC-021**: walked by probes; failing members are in #5.
- **FC-022, FC-023, FC-025, FC-026, FC-027**: no literal, identifier, expression-body, or token re-resolution change. The multi-line accessor list was probed (holds).
- **FC-024**: partial members are #1287's subject, audited above. RH2004's symbol action covers nested types, because each named type gets its own action.
- **FC-030**: RH2001 still formats only its field.
- **FC-033, FC-034**: the repair tests go through the code-fix path, LF and CRLF, with the reported shapes.
- **FC-035**: expected values come from spec examples or decision 21, not from patched code.
- **FC-037**: no comment-only claim.

### Architecture findings
- **The trivia of a removed accessibility keyword is decided in two methods (#5).**
  - Components: `DeclarationModifierUtilities.AddAccessibilityModifiers(SyntaxTokenList, …)` and the member overload `AddAccessibilityModifiers(MemberDeclarationSyntax, …)` with its new `CarryLeadingTriviaOfLaterAccessibilityModifiers`.
  - The token-list overload gives the new first token the first accessibility keyword's leading and trailing trivia, which is right only when that keyword was first. The member overload overwrites the leading side for the first token, and now moves the leading side of later keywords elsewhere. The trailing side of a later keyword is still decided by the token-list overload's first-token assumption.
  - Inputs: (a)–(f) of #5.
  - This repeats #1's class (same decision, same components), which `README.md` (*A repeated class is an architecture signal*) routes to the lead as an architecture finding instead of another repair round.

### Hints
- **RH2001 reorder path (extends follow-up candidate 4), observed.** `    // c⏎    static private int Value { get; }` is now reported without a fix. The RH7105 reorder moves `static` behind `private`, but `static` keeps the property's leading trivia while the builder also gives that trivia to the new first token `private`. The candidate therefore holds `// c` twice, and the new guard withholds the fix. By trace, `42db22c` and `main` offered a fix that duplicated the comment. RH2001-004's coverage includes this input (a comment only in front of the first token), so the withheld fix is safer than before but still short of the requirement. The guard can't be narrowed until the builder carries that trivia correctly. Observed with it: `static // a⏎    private int Value { get; }` still gives `private     static // a⏎    readonly int _value;` (candidate 4 as recorded).
- **IDE display of RH2004 partial-member diagnostics (extends dropped hint 19).** Every partial property, indexer, event, and constructor diagnostic now comes from a `NamedType` symbol action, and can sit in a document other than the one being analyzed. Batch analysis shows it (tests). Single-document IDE analysis was not verified.
- **Formatter layout inside a declaration (pre-existing, not constructed beyond the test).** `Format(private⏎    // note⏎    int _value;)` inserts a blank line between `private` and the comment line (`13-test-implementer.md`). Whether a blank line inside a declaration's modifier gap is intended formatter output is outside this run.

## Commands

```shell
git ls-remote origin refs/heads/main
scripts/lane-check.sh bug-fix
scripts/apply-fix.sh RH2005 <scratchpad>/audit17/rh2005   # 10 fixtures x LF/CRLF: 16 fixed, 4 invalid-result
scripts/apply-fix.sh RH2001 <scratchpad>/audit17/rh2001   # 9 fixtures x LF/CRLF: 14 fixed, 4 no-fix-offered
```
