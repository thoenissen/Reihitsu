Role: auditor
Commit: b05e1a1a1b78f71067bab2d19bfe47fedfa85708
Verdict: pass, text cleanup

The prompt carried no intended fix, suspected cause, or hoped-for verdict. It named the head, the base, the previous audit, decisions 13–17, and handovers 11–16.

## Scope

This is the repair-round delta audit of `5a1e7022b4dd963a5c668b85eb404d49a452b16c..b05e1a1a1b78f71067bab2d19bfe47fedfa85708` against base `origin/main` = `b673099172ca665ef61a85f760ffe436443bd72f`. I verified the state: the checkout is the head, `git ls-remote origin refs/heads/main` returns the base SHA, and the base is an ancestor of the head. The repair diff outside `sdd/work/` matches handovers 11–15: `CasingCodeFixProviderBase{T}.cs`, the RH4105 and RH4109 test classes, `sdd/specs/shared/casing.md`, `RH4105.md`, and `RH4120.md`. The scope did not grow, because the repair stays inside the one owner of the SHARED-CASING-012 decision and its spec.

I trusted the cleaner's Release build and the roles' focused test results and re-ran no test. I ran 10 new fixtures with `scripts/apply-fix.sh RH4105` (LF and CRLF agreed: 14 fixed, 6 no-fix-offered, 0 drift). The fixtures are in `/tmp/claude-0/-home-user-Reihitsu/7f636aa1-d45f-5a98-b71c-ebba69f2b65b/scratchpad/f17`.

Previous findings:
- **#1 — closed.** The guard now refuses members (a), (b), and (c). (a) `nameof(Names.Condition)` is a qualified operand, so it is not exempt. (b) `nameof(condition)` names the new name, which is excluded from the exempt set. (c) For an extension block owner, every member's parameter list is scanned with an empty exempt set, which covers both the literal and the `nameof` argument. Each member has a test (`NoFixIsOfferedWhenACallerArgumentExpressionIsAQualifiedNameofOfAnotherSymbolWithTheOldName`, `…IsANameofOfAnotherSymbolWithTheNewName`, `…ExtensionBlockNamesTheReceiverByACallerArgumentExpressionLiteral`, `…ByANameofCallerArgumentExpression`). The members the previous audit listed as holding still hold (see *Guard scope*).
- **#2 — closed.** `ApplyFixInMultiTargetSolutionAsync` now asserts that each copy's `GetLinkedDocumentIds()` is exactly the set of other projects' copies with the same path, and that the initiating holder has at least one linked copy. The test implementer's mutation (distinct paths per project) made the multi-target tests fail.

## thoenissen/Reihitsu#1597

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 3 | low | `src/Reihitsu.Analyzer.Test/Naming/RH4105MethodParameterCasingAnalyzerTests.cs:1329-1333`, XML summary of `NoFixIsOfferedForAnUnqualifiedNameofOfTheOldNameBeforeCSharp11` | FC-031 | – | PR-introduced | The summary still gives the reason that decision 17 and `14-spec-author.md` removed from the spec: "because before C# 11 that `nameof` refers to a member of the type with the same name, which the rename leaves as it is". `12-test-implementer.md` showed this is false on this repository's compiler, where the renamer renames the operand under C# 10. The reworded SHARED-CASING-012 *Why* now states a conservative refusal by language version instead. Only the test comment is affected; the assertion matches the spec. | Reword the summary to match the spec: before C# 11 the argument is not exempt, so the fix is withheld from the language version alone, without deciding what the operand refers to. |

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Which parameter lists are inspected (SHARED-CASING-012 bullets) | For each distinct `ContainingSymbol` of a cascade parameter, `GetParameters(owner)`: a method's or property's parameters, or an extension block's receiver. For an extension-block owner, also `GetParameters(member)` for every `GetMembers()` entry | SHARED-FIX-003 *Run time*: the lists a `CallerArgumentExpression` value can resolve against. These are the attribute's own list and, for an extension member, the receiver | Agree. A `CallerArgumentExpression` value resolves only against these lists. A local function or lambda inside an extension member resolves against its own list only. The member-parameter rename inside an extension block (`extmember.cs`) is inspected under its own owner. A generic receiver (`extgeneric.cs`, literal `"Value"`) is withheld. An extension block with a property and a new-name literal (`extprop.cs`) is withheld | holds |
| When a `nameof` argument is exempt (`IsExemptNameofArgument`) | The single argument is `nameof(<IdentifierName>)` with one argument. The identifier's `ValueText` is the old name of a cascade parameter of the same owner and not the new name. The attribute's tree has `CSharpParseOptions.LanguageVersion >= CSharp11` (effective version, so Latest and Preview qualify) | Whether the renamer changes the argument's value together with the parameter, which is the binding the spec's *Why* relies on | Agree for every member I constructed under the default (C# 14) language version, each with a same-named `const Condition` in the enclosing type: method (existing C# 11 test), indexer, local function, lambda, delegate, class primary constructor. In each case the fix renamed the operand together with the parameter, so the operand binds to the parameter and the value follows the rename. Before C# 11 the argument is not exempt, a conservative refusal (decisions 16 and 17). Every other shape is not exempt, which is conservative: a qualified operand, `this.X`, generic names, the new name, and extension-member lists | holds |
| Override parameter (outside the cascade) with its own literal | `override.cs`: base `nameof(Condition)` exempt, derived `"Condition"` literal | The derived parameter is not in the cascade and is not renamed. The base's attribute and parameter stay consistent | Unchanged by the repair; consistent | holds |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| The four new `NoFixIsOffered…` tests for (a), (b), (c) literal, (c) `nameof` | No action for the member | Any registered action | `Verify(src, src)` | adequate. All four were red before ba93a6f |
| `FixRenamesTheReceiverOfAnExtensionBlockWithoutACallerArgumentExpression` | Other side of (c): the receiver and its references are renamed | No fix, or another text | `Verify(src, fixed)` | adequate (FC-003) |
| `NoFixIsOfferedForAnUnqualifiedNameofOfTheOldNameBeforeCSharp11` and `FixRenamesAnUnqualifiedNameofOfTheOldNameFromCSharp11OnWhenTheTypeHasAMemberWithThatName` | The language-version boundary of the exemption on the same input | An action under C# 10, or no or another text under C# 11 | `Verify` + `SolutionTransforms` → `ApplyLanguageVersion` | adequate. A probe confirmed the C# 10 setting applies (`12-test-implementer.md`). The C# 10 summary is stale (#3) |
| Fix All for (a)–(d) | Not added | — | — | acceptable. Fix All reaches the same check through `RenameWithoutConflictAsync` (lines 316/323, 689, 1103 → 763). The existing `FixAllSkipsTheParameterNamedByACallerArgumentExpression` covers the Fix All side |

## thoenissen/Reihitsu#1598

### Findings
_None._ Finding #2 is closed (see *Scope*).

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `ApplyFixInMultiTargetSolutionAsync` linkage check (five RH4109 multi-target tests) | Every copy of a file that several projects compile is linked to exactly the other copies, and the initiating holder has a linked copy | Missing or extra linked IDs on any document | `Assert.AreSequenceEqual` over ordered IDs, plus `Assert.IsNotEmpty` | adequate. The mutation with distinct paths turned it red |

## thoenissen/Reihitsu#1016

### Findings
_None._ The repair does not touch this issue; the evidence of `10-auditor.md` still holds.

## thoenissen/Reihitsu#1015

### Findings
_None._ The repair does not touch this issue; the evidence of `10-auditor.md` still holds.

## thoenissen/Reihitsu#1084

### Findings
_None._ The repair does not touch this issue; the evidence of `10-auditor.md` still holds.

## Whole run

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| `CallerArgumentExpression` handling | Only `CasingCodeFixProviderBase{T}.cs`: both `HasCallerArgumentExpressionNaming` overloads and `IsExemptNameofArgument` (`rg -n CallerArgumentExpression src --glob '!*Test*'`) | one owner |
| Parameter list of a symbol (`GetParameters`) | New private helper. It replaces the inline switch that the old overload held, so there is no second copy | one owner |
| Language-version gating | `IsExemptNameofArgument` (C# 11 exemption) and the RH54xx `MinimumLanguageVersion` providers (`rg -n LanguageVersion src --glob '!*Test*'`). These are different decisions | not duplicated |

### Checks not tied to one issue
- **Specs (1).** SHARED-CASING-012's normative bullets match the code, as the *Guard scope* table shows. The *Why* no longer claims C# 10 binding. RH4105-003 and RH4120-003 refer to SHARED-CASING-012 and no longer restate it. Examples (a)–(d) and the extension block each have a test.
- **Visible output (10).** Every new expected value is either a withheld fix for a rare class (decision 13; SHARED-FIX-003 is unchanged), or the same output `main` already produces: on `main` the exemption accepted every `nameof`, so the C# 11 rename of both names and the extension-receiver control were already offered. No rule page changed.
- **Cost of special cases (11).** The guard widening is a cheap check: it looks at the argument's shape, compares identifier text with the cascade's names, reads the parse options, and enumerates an extension block's members. It does no binding. No new type was added, and no declared unsupported shape is involved.
- **Comments (8).** The summaries and remarks of `GetParameters`, `IsExemptNameofArgument`, and both `HasCallerArgumentExpressionNaming` overloads describe the new code. One test summary is stale (#3). The repair adds no `TODO` and no tracker number.
- **Scope (9).** Nothing is outside the run.
- **Dependencies.** `INamedTypeSymbol.IsExtension` and `ExtensionParameter` are new to production code. Roslyn stays at 5.9.0, so no package changed.

### Failure classes dismissed
- FC-001, FC-002: the inspected lists and the exemption now agree with the spec's span (*Guard scope*).
- FC-003: both sides of each new boundary are tested: extension block with and without the attribute, C# 10 and C# 11, an old-name and a new-name operand, qualified and unqualified operands.
- FC-016: I traced the sibling shapes of the exemption (indexer, local function, lambda, delegate, class primary constructor, override, extension member parameter, generic receiver). None lets the hazard through.
- FC-036: closed for the `CallerArgumentExpression` class. The other `nameof` and reflection exposures are unchanged from the first audit.
- FC-017: covered in the *Assertion adequacy* tables.
- FC-004, FC-005, FC-010 to FC-014, FC-019 to FC-022, FC-025 to FC-030, FC-038 to FC-043: the repair formats nothing and moves no trivia.
- FC-008: Fix All recomputes the check for each candidate through the same path.
- FC-015: see *Policy ownership*.
- FC-031: finding #3.
- FC-032: the cleaner built Release after the last production edit (ba93a6f). Later commits are spec and handover only.
- FC-033: the RH4109 linkage precondition is now asserted (#2 closed).
- FC-035: the repair tests (97b1643) predate the production change (ba93a6f).

### Architecture findings
_None._

### Hints
- The extension receiver's own parameter list keeps the cascade exemption (handover 13, 16). No test covers it, and the path has no run-time effect: a receiver cannot be optional, so a `CallerArgumentExpression` on it does nothing. This is not a finding.
- Fixture commands: `scripts/apply-fix.sh RH4105 /tmp/claude-0/-home-user-Reihitsu/7f636aa1-d45f-5a98-b71c-ebba69f2b65b/scratchpad/f17`. The fixtures cover an indexer, a local function, a lambda, a delegate, a class primary constructor with and without a same-named `const`, an override, an extension member parameter, a generic extension receiver, and an extension block with a property and a new-name literal.
