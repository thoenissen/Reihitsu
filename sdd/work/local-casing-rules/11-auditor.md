Role: auditor
Commit: f6cabbd043f8439c6f0de58ea656b3f4e53d4f3e
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Inputs: `00-lead.md` (answers 1B, 2A, 3B, 4A; decisions 1 to 7), handovers `01` to `10`, the specs RH4105, RH4115, RH4117, RH4128, `shared/casing.md` (SHARED-CASING-015), both change docs, the rule pages, and the diff `fe7b8f85...f6cabbd0`.

## Scope

Head `f6cabbd043f8439c6f0de58ea656b3f4e53d4f3e` checked out, clean tree. Base `origin/main` = `fe7b8f85d45a85e3354da783ca73a20b3e39eb6d`, an ancestor of the head (merged). Full audit. The diff (62 files) matches the handovers: three new helper types, the RH4128 analyzer and fix, the RH4105/RH4115/RH4117/RH4124 analyzer changes, the RH4105 fix provider, the shared casing fix base, resources, `Release.ruleset`, `Reihitsu.sln`, package README, rule pages, specs, change docs, and the tests listed in `04`/`06`. `scripts/lane-check.sh behavior-change`: confirmed. The cleaner's Release build and the roles' focused test results are trusted. One probe was run with `scripts/apply-fix.sh RH4106` (fixture in the session scratchpad, LF and CRLF).

## thoenissen/Reihitsu#1087

### Findings
_None specific to this issue._ The SHARED-CASING-015 findings under *Whole run* reach the RH4115 and RH4117 fixes as well.

### Requirements
- RH4115-001/-002, RH4117-001/-003: one predicate, `DeconstructionVariableUtilities.IsDeconstructionVariable`, decides the boundary; RH4117 reports exactly what it includes, and RH4115 reports exactly what it excludes, on the same node kind (`SingleVariableDesignation`), so no designation is reported twice or by no rule. Traced members: parenthesized designation anywhere (declaration, `foreach`, `var (…)` pattern also inside a property pattern), declaration expression inside a tuple expression (also nested, `foreach`, and a `for` initializer `for ((var I, var J) = …)`, which reaches the same branch), `out var` (parent `ArgumentList`, excluded), the pattern of a positional subpattern directly, behind `and`, behind parentheses (`(int Wrapped)`), designations of recursive and list patterns as elements, a positional pattern nested in a property or list pattern; RH4115 side: whole-pattern designation, property subpattern, list element and slice (`SlicePattern` stops the walk), top-level `(var X)`, simple pattern variable. All agree with RH4117-003 and answer 1B. The two shared-source tests pin both sides.
- RH4115-004 (`_` by value text, every declaration form), RH4115-005 (`IsConst: false`), RH4117-002 (unchanged provider node type covers the new positions): hold.

## thoenissen/Reihitsu#1088

### Findings
_None specific to this issue._

### Requirements
- RH4128-001 (every `const` local declaration statement, wherever it stands; one diagnostic per declarator), RH4128-002 (`_` by value text), RH4128-003 (shared rename, Fix All from the base): hold. RH4115 `IsConst: false` and RH4128 `IsConst: true` split `LocalDeclarationStatement` without overlap or gap.
- Resources, code-fix title, `Release.ruleset`, `Reihitsu.sln`, package README row, rule page, spec index: present; pinned by `AnalyzerPackageMetadataTests`, `AnalyzerResourcesTests`, `CodeFixResourcesTests`.

## thoenissen/Reihitsu#1978

### Findings
_None specific to this issue._

### Requirements
- RH4105-005: the analyzer registers `RangeVariableDeclarationUtilities.DeclarationKinds` (from, let, join, join-into, continuation), reports at the declaring identifier only. RH4105-002: `_` skipped for parameters and range variables; record primary-constructor parameters still skipped. RH4105-006: `GetDeclaredSymbol` resolves `QueryClauseSyntax` (from, let, join), `JoinIntoClauseSyntax`, `QueryContinuationSyntax`, so every existing guard runs on range-variable renames (tests for SHARED-CASING-007/-009/-010/-013 on range variables pass). Title and message widened in resources, rule page, README row, class summary.
- `CasingCodeFixProviderBase<SyntaxNode>` for RH4105: `FindNode` on a parameter identifier still yields the `ParameterSyntax` (no parent of a parameter has the same full span), and any other node yields an empty identifier, which `TryGetFixedIdentifier` refuses. No regression for parameters.

## Whole run

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | medium | `tests/Reihitsu.Analyzer.Test/Naming/*` for `CasingAnonymousMemberUtilities.GetMemberNamedByReference` (`src/Reihitsu.Analyzer.CodeFixes/Base/CasingAnonymousMemberUtilities.cs:89-98`) | FC-003, FC-017 | – | new mechanism | The conditional-access branch of SHARED-CASING-015 is tested on its positive side only (`new { other?.Value }`, `Other = other?.Value`). The boundary `conditionalAccess.WhenNotNull == expression` has no test on its other side: a renamed reference that is the receiver of a conditional access whose inferred member name comes from another identifier. Members of the class: `new { Text?.Length }` and `new { this.Text?.Length }` (receiver; must get no name, the member is `Length`), and the tail cells `new { other?._next.Value }`, `new { other?._next?.Value }` (WhenNotNull a member access, nested conditional access; must get `Value = `). All hold today (probe below), but with the `WhenNotNull` check dropped, `new { Text?.Length }` becomes `new { Text = _text?.Length }`, renaming the member from `Length` to `Text` (SHARED-FIX-003), and every test stays green. | Add the receiver cells (`new { Text?.Length }`, `new { this.Text?.Length }` → no name written) and the two tail cells to one SHARED-CASING-015 test, e.g. in `RH4106PrivateFieldCasingAnalyzerTests` |
| 2 | medium | `src/Reihitsu.Analyzer.CodeFixes/Base/CasingAnonymousMemberUtilities.cs:72-99` vs `src/Reihitsu.Analyzer.CodeFixes/Rules/Design/RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider.cs:198-230` | FC-015, FC-044 | – | new mechanism | Which identifier the compiler names an anonymous object member after is the compiler's rule. RH2001's fix reads it (`SyntaxFacts.TryGetInferredMemberName`); the new helper restates it by hand (identifier → member access name / member binding → up the `WhenNotNull` chain). The two statements agree for every compiling shape traced (identifier, simple member access, `!.`, `?.` with member binding, member access, and nested conditional access as tail; receiver positions excluded), so no corrupted output is confirmed; but they already differ in one cell: the helper accepts every `MemberAccessExpressionSyntax`, including the pointer member access `p->Value`, which the compiler's rule (simple member access only) does not name — non-compiling code, so no caller-visible defect. A second statement of a compiler-computed rule is the shape FC-044 names. | Read the compiler's rule: select a member when `NameEquals == null`, `SyntaxFacts.TryGetInferredMemberName(member.Expression)` is a non-empty name, and `member.Expression.GetLastToken()` is the reference token (for every shape the compiler names, the name token is the expression's last token); delete the hand walk |

Probe for finding 1 (`scripts/apply-fix.sh RH4106`, LF and CRLF identical, `fixed`, 1 action, 2 iterations):

```csharp
var a = new { other?._next.Value };      // -> new { Value = other?._next._value }
var b = new { other?._next?.Value };     // -> new { Value = other?._next?._value }
var c = new { other!.Value };            // -> new { Value = other!._value }
var d = new { Text?.Length };            // -> new { _text?.Length }
var e = new { this.Text?.Length };       // -> new { this._text?.Length }
var f = new { other?._next.Text.Length };// -> new { other?._next._text.Length }
```

with `return a.Value.ToString() + b.Value + c.Value + d.Length + e.Length + f.Length;` unchanged and compiling.

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Deconstruction variable (RH4117 includes, RH4115 excludes) | `designation.Parent`; for a pattern, the ancestors through `ParenthesizedPattern` and `and` patterns up to a `Subpattern` whose parent is a `PositionalPatternClause`; for a declaration expression, `Argument` in `TupleExpression` | The same predicate in both analyzers (single owner); RH4117-003's syntactic statement | Same span on both sides; `or`/`not` cannot declare; slice and property subpatterns stop the walk | holds |
| Local constant (RH4128 includes, RH4115 excludes) | `LocalDeclarationStatementSyntax.IsConst` | Complementary `IsConst: true` / `IsConst: false` on the same node kind | Exact complement | holds |
| `_` exemption (RH4105, RH4115, RH4128) | identifier `ValueText` | RH4120's existing `ValueText != "_"`; `TryGetFixedIdentifier` refuses `_` anyway | Same test on value text (`@_` included) | holds |
| Range-variable declaration (RH4105 analyzer, RH4105 fix, RH4124) | node kind and its identifier | `RangeVariableDeclarationUtilities` for all three; `GetDeclaredSymbol` cases for the fix | Same kinds; `FromClause`/`LetClause`/`JoinClause` are `QueryClauseSyntax` | holds |
| Anonymous member named by a renamed reference (SHARED-CASING-015) | the reference token; its `IdentifierName`; member-access name or member binding; the `WhenNotNull` chain of conditional accesses; the declarator with `NameEquals == null` | Compiler's inference rule as `SyntaxFacts.TryGetInferredMemberName` (used by RH2001) | Agree on every compiling shape; differ on `->` (non-compiling) | finding #2; the receiver side untested: finding #1 |
| Rename safety after naming | every check compares `namedSolution` with the renamed solution | Pre-run checks compared the original solution | The naming edits lie in members whose expression holds a reference the renamer renames, so every document they touch is also a document the rename changes and gets the hiding-carrier and compile checks; writing `Name = ` with the inferred name keeps the anonymous type identical | holds |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Deconstruction-variable boundary | `DeconstructionVariableUtilities.IsDeconstructionVariable` (RH4115, RH4117). `rg "ParenthesizedVariableDesignationSyntax\|PositionalPatternClauseSyntax" src`: also `Formatter/.../DeclaredNameSpacingRule.cs`, `PrefixKeywordSpacingRule.cs`, blank-line and indentation sites — layout decisions, not the boundary | one owner |
| Range-variable declarations | `RangeVariableDeclarationUtilities` (RH4105 analyzer and fix, RH4124); `CasingCodeFixProviderBase{T}.GetDeclaredSymbol` maps node types to symbols as for every other kind | one owner |
| Anonymous member inferred from an identifier | `CasingAnonymousMemberUtilities.GetMemberNamedByReference` (hand walk); `RH2001…CodeFixProvider.GivesInferredMemberNameAsync` (`SyntaxFacts.TryGetInferredMemberName`); `RH3002` and formatter sites (`AnonymousObjectContributor`, `LineBreakInitializerRewriter`, `JoinedGapUtilities`, `SyntaxIndentationUtilities`) decide layout or parentheses, not the name | two statements of the compiler's rule: finding #2 |
| Casing rename | `CasingCodeFixProviderBase{T}` (`Renamer.RenameSymbolAsync` once, line 435) for all 27 casing providers; RH2001's own rename is a different rule | one owner |
| Local constant vs variable | `IsConst` in RH4115 and RH4128 | complementary one-modifier test, as the skeptic judged |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH4115/RH4117 `…NestedAndSwitchPositions…`, `…NoWholeElement…` (shared sources) | each designation is reported by exactly one of the two rules | a designation reported by both or neither | `Verify(source)` with exact markup, both classes | adequate |
| RH4117 `FixRenames…` (tuple, `var (…)` pattern, positional elements, switch) | fix renames every reference, compiles, converges | different text, compile error, remaining diagnostic | `Verify(source, fixed)` (single, Fix All, recompile) | adequate |
| RH4115 `LocalNamedDiscard…`, `LocalConstantsAreNotChecked…` | no report for `_` / `const`; `_count`, non-const still reported | a report on `_`/`const`, or none on the neighbours | `Verify(source)` | adequate (both sides) |
| RH4128 tests (12) | report on every const container, not on non-const, not `_`; fix with references in `case`, constant value, attribute argument; collision and keyword refusals; Fix All one iteration | any deviation in exact text | `Verify`, `GetFixAllScenario` | adequate |
| RH4105 range-variable tests (15) | report per clause, not at uses; fix across continuation/join-into; refusals 009/010/013/007 | deviation in exact text or offered fix | `Verify`; same-name test with `BatchFixedCode = source`, `NumberOfFixAllIterations = -1` (at most one iteration, result still compared exactly) | adequate |
| RH4115 multi-line anonymous object (LF, CRLF) | `Name = ` inserted, comments and layout kept, per single action and full fix | moved comment, line-ending drift | `Verify` plus `TryApplyCodeFixAtAsync` per diagnostic on raw text | adequate |
| SHARED-CASING-015 tests (RH4101, RH4102, RH4105, RH4106 ×4, RH4107–RH4115, RH4117 ×2, RH4120, RH4128) | inferred member keeps its old name; member with own name or named after another identifier gets none | `new { value }`, or a name written where none belongs | `Verify(source, fixed)` exact text | adequate for identifier, member access, `?.` member binding, receiver of member access; conditional-access receiver and tail cells missing: finding #1 |

### Failure classes dismissed
- FC-004, FC-005, FC-019, FC-020, FC-021: SHARED-CASING-015 joins no lines and moves no token; the member's leading trivia (comments, a directive) moves onto the inserted name token, i.e. stays in front of the member; covered by the multi-line test with own-line, trailing `//`, and `/* */` comments under LF and CRLF. A directive inside the member makes the document a hiding carrier and the rename is withheld (SHARED-FIX-012).
- FC-006: `Name = ` with the inferred name keeps the anonymous type's member names, order, and types, so the type and every read stay the same; every fix test recompiles.
- FC-007, FC-008: `Verify` runs Fix All and recompiles; naming edits touch only anonymous members, never an annotated declaration, so Fix All re-resolution is unaffected.
- FC-009: a single fix is registered only when the full checked rename succeeds and applies the same computation. The shared Fix All provider offering an action that changes nothing is pre-existing (hint).
- FC-010 to FC-014, FC-028, FC-029, FC-038 to FC-043: no formatter phase, precheck, or formatter call is changed; the rename fixes do not format.
- FC-018: renames go through the Renamer; references across continuations, `case` labels, attribute arguments tested.
- FC-022, FC-025, FC-026: no literal, expression-body, or token re-resolution involved.
- FC-023: `_`, `@Value`, `from Group` → `@group` refused; `@_` deconstruction variable needs a variable named `@_` in a deconstruction, which no one writes.
- FC-024: partial and interface cascades unchanged; the cascade search is the existing one.
- FC-027: nesting covered (nested tuples, designations, positional patterns, queries in lambdas and field initializers, constants in every container).
- FC-030: the fix changes only the renamed tokens and the inserted names.
- FC-031: changed summaries checked (`ApplyCodeFixAsync`, `RenameDeclarationAsync`, `RenameWithoutConflictAsync`, `PrepareNamedRenameAsync`, RH4105/RH4115/RH4117/RH4128 analyzers, RH4124); no stale reference to `HasRenameConflictAsync` or the old RH4105 title.
- FC-032, FC-033, FC-034, FC-035, FC-037: no formatter change, no reproduction, expected values come from specs (RH4128 checked against the RH4115 rename before the rule existed), no text-only claim.
- FC-036: anonymous members are SHARED-CASING-015; range-variable names visible in expression trees (lambda parameters, compiler-generated transparent identifiers) change as lambda-parameter renames already do on `main` — the maintainer's #1978 answer accepts the fix.
- FC-001, FC-002, FC-003, FC-015, FC-016, FC-017, FC-044: walked above; findings #1 and #2.

### Visible output is decided
Every changed expected value is covered: RH4115 `_` (RH4115-004, maintainer 2026-10-18), local constants to RH4128 (2026-10-18, 2026-11-21), `const _` (2A), deconstruction forms and positional elements (2026-10-18, 1B), range variables and title/message (#1978 option A), fix kept for anonymous members (3B) and for every rule using the shared rename, including RH4106's previously withheld read-by-name case (4A); refusals under SHARED-CASING-009/-010/-013/-007 are unchanged requirements. The rule-page changes (RH4105, RH4115, RH4117, new RH4128) describe only these decided behaviors.

### Scope
No edit outside the run's scope beyond decision 6 (RH4124 reads the shared range-variable helper, behavior unchanged). No `TODO`, no tracker numbers in source.

## Architecture findings

**The compiler's anonymous-member naming rule is stated twice (finding #2).** Components: `CasingAnonymousMemberUtilities.GetMemberNamedByReference` (new, casing fixes) and `RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider.GivesInferredMemberNameAsync` (reads `SyntaxFacts.TryGetInferredMemberName`). Shared decision: whether an anonymous object member takes its name from a given identifier. Places: `CasingAnonymousMemberUtilities.cs:82-99` (hand walk over member access, member binding, conditional access) versus `RH2001…CodeFixProvider.cs:219` (compiler API). No compiling input shows them disagreeing; the one differing cell is a pointer member access (`new { p->Value }`), which the compiler does not name. Reading the API removes the second statement.

## Hints

- Cost: `NameAnonymousObjectMembersAsync` runs a solution-wide `SymbolFinder.FindReferencesAsync` that `PrepareRenameAsync` has just run for the cascade, and a third time when names were written; `ApplyCodeFixAsync` now repeats the whole checked rename that registration already computed. Correct, but every casing fix (type renames included) pays it on registration, application, and per Fix All candidate. Passing the cascade's reference result into the naming step would halve it.
- The shared casing Fix All provider offers an action when every candidate is skipped (SHARED-CASING-010), so tests need `NumberOfFixAllIterations` (`RH4105…RangeVariablesWithTheSameNewNameInOneQueryAreNotFixed`, `RH4106…FixAllSkipsAFieldWithALeadingAcronymThatCollidesWithAnotherField`). Pre-existing; the action is decided at application time, not at offer time.

## Follow-up candidates

- RH2001-011 (already in `00-lead.md`): RH2001's fix withholds itself where the casing fixes now keep the member name; it could reuse the SHARED-CASING-015 rewrite. Structural note: if finding #2 is repaired by reading `SyntaxFacts.TryGetInferredMemberName`, both fixes then read one rule.
