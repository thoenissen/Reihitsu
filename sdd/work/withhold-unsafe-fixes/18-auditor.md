Role: auditor
Commit: 7eecbae
Verdict: findings (#1136: findings; #1134: findings; #1135: findings; #1262: pass; #1332: pass; #1336: pass; #1340: pass)

Inputs: the seven issues as named in `00-lead.md`, `00-lead.md` (maintainer answers 1–5, decisions 6–11), the first audit `10-auditor.md`, handovers `11`–`17`, the change doc `sdd/changes/2026-10/withhold-unsafe-fixes.md`, the specs the repair changes (SHARED-ORDER-007, RH7501-004) and the ones its code reads (SHARED-FIX-003, SHARED-ORDER-002/-003, RH7103-007, RH7109-006, RH7309-006, RH7004-004), and the diff `551f629..7eecbae`. The prompt carried no intended fix, suspected cause, or hoped-for verdict. Observations come from `scripts/apply-fix.sh <ID>` on fixtures in the session scratchpad (LF and CRLF, `DEBUG` not defined), on this branch and, where marked "main", on a temporary worktree of `a59a003` (removed afterwards). No test was re-run: no finding turns on a test result.

## Scope

Repair delta against `551f629`. Head `7eecbae` (checked out, clean). `origin/main` = `a59a003f6a9f85126892eac7646fc8945a4b9073`, an ancestor of the head. The diff matches handovers `11`–`17`: 10 production files (the 8 of the first round's set plus `TypeMemberOrderingCodeFixProviderBase` and the RH7103 provider, decision 6), the tests of `11-test-implementer.md`, SHARED-ORDER-007 and RH7501-004, and the change doc. The repair did not grow into an unrelated surface; the RH7103 provider is the adjacent fix decision 6 names. `scripts/lane-check.sh behavior-change`: confirmed. Evidence of the first audit that the repair did not touch is reused.

### Status of the first audit's findings

| # | Status | Evidence |
|---|---|---|
| 1 | closed | Members (a)–(e) are tested and give `main`'s output. Class traced again: a conditional group in front of `}` withholds only when its `#if` lies behind the declaration (nested groups, `#else` with disabled text, a group that starts among the statements: all as SHARED-FIX-003 requires); the end of the file is no partner; `#pragma warning` pairs per warning, `219` and `CS0219` alike (fixture g1: withheld), restore-all ends a single disable (g7: withheld), `#nullable` pairs per setting (g3, g8: withheld; g4, other setting: offered), `#line` pairs (g5: withheld), another warning code does not pair (g6: offered). See #11 for the tests |
| 2 | closed | The layout is read from the type symbol: partial class, record, `Auto`/`Explicit` on another part, `layoutKind:` named argument — tested; casts and aliases resolve through binding |
| 3 | closed | `ChangesDocumentedDeclaration` also checks the closing brace's leading trivia; `/** */` behind code lands there too (the lexer never puts a documentation comment in trailing trivia) |
| 4 | closed | Its listed members hold: RH7109 with the confirmed input withholds (tested); RH7309 uses the same owner. One owner of the comparison now exists. A member the enumeration did not name fails: #9 |
| 5 | closed | `} /* c */ }` restored, LF and CRLF tested |
| 6 | closed | Both inputs assert the full output. On `main` both comments are dropped (observed this round: `using (var s = new MemoryStream())` and `using (var s = new MemoryStream()` ⏎ `)`), so the kept comments are SHARED-FIX-003's |
| 7 | closed | RH7501-004 "first token on its line"; the summary of `CanMoveBreak` matches |
| 8 | closed | `<returns>` aligned |

## thoenissen/Reihitsu#1136

Findings: #11 (tests of the repaired pairing). #1 is closed (table above).

## thoenissen/Reihitsu#1134

Findings: #10 (excluded code that travels with the moved RH7109 field). #2 is closed.

Holds: RH7109 fixture c (`struct`, a static readonly field whose leading trivia holds an excluded instance field, moved across an active instance field): withheld, as SHARED-FIX-003 *Run time* and *Build configurations* require; `main` offers it.

## thoenissen/Reihitsu#1135

Findings: #9 (RH7103 layout, repeats #4), #10 (spec and test side for RH7103 as well). #4 is closed for its listed members.

Holds: RH7103 with excluded instance initializers in the moved static member's trivia (fixture i: withheld); RH7103 with an excluded static initializer that travels across an active one (traced: withheld for that diagnostic alone; Fix All converges in order through the earlier member's fix, fx2); RH7309 counts every member by segment through the same owner.

## thoenissen/Reihitsu#1262

Pass. #3 is closed. The three new RH7309 tests cover the confirmed shapes on both sides.

## thoenissen/Reihitsu#1332

Pass. #7 is closed; the new `case 4` test and the commented `break` example match RH7501-004.

## thoenissen/Reihitsu#1336

Pass. #5, #6, #8 are closed. The header-line move applies exactly when the semicolon's trailing trivia holds a comment and ends with a line break, the qualifier of RH7004-004.

## thoenissen/Reihitsu#1340

Pass. No change in the repair.

## Whole run

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 9 | high | `RH7103StaticElementsMustAppearBeforeInstanceElementsCodeFixProvider.IsMoveSafe` → `OrderingDeclarationUtilities.ChangesInitializerExecutionOrder` (`ChangesMemberOrder(type, false, moved, target)`) | FC-006, FC-016 | #4 | same requirement | The one owner of the member-order decision counts excluded members, but RH7103 hard-codes `fieldOrderSetsLayout: false` on the premise that a static move never reorders layout fields. Excluded code in the moved static member's leading trivia travels with it (SHARED-ORDER-002) and can declare instance fields. Confirmed (fixture h): `public struct S { private int _x;` ⏎ `#if DEBUG` ⏎ `private int _y;` ⏎ `#endif` ⏎ `private static int _s; }` — the RH7103 fix is offered and writes `#if DEBUG _y #endif`, `_s`, then `_x`, so a build with `DEBUG` lays out `_y` before `_x`, against SHARED-FIX-003 *Run time* and *Build configurations*. Members that hold: RH7109 with the same shape (fixture c: withheld), RH7309, RH7103's two initializer sets (fixture i, fx2). The same input is offered on `main`; the run took RH7103 into the shared owner (decision 6), and #4's enumeration named RH7103 only for initializers. Member outside the run, same mechanism: RH7102 has no member-order guard at all (fixtures j: layout, k: instance initializer — both offered on the branch and on `main`); see *Hints* | Let the owner decide the layout set itself: RH7103 resolves the type symbol (it already receives the semantic model) and asks `ChangesMemberOrder` with `FieldOrderSetsLayout(symbol)`, as RH7109 does, or the owner takes the symbol so that no consumer passes the flag. Add an RH7103 test for fixture h and list the withheld fix with case 10 of the change doc |
| 10 | medium | `sdd/specs/shared/ordering-move-safety.md` SHARED-ORDER-007; change doc case 10 (last paragraph); tests of RH7109/RH7103 | FC-002, FC-003 | – | PR-introduced | The repaired owner ranks excluded code in the moved member's leading trivia with the moved member, so its initializers and layout fields cross what the moved member crosses. That is right by SHARED-FIX-003 *Build configurations*, but nothing states or pins it, and the texts say otherwise: SHARED-ORDER-007 refuses only "when the moved member has a runtime initializer" and says such code "travels with it … and is not crossed"; the change doc says the fix "stays offered … when the `#if` pair stands directly in front of the moved member, because the excluded code then moves with it and both builds keep the initializer order". Confirmed (fixture a): `private int _a = Log("a");` ⏎ `#if DEBUG` ⏎ `private int _c = Log("c");` ⏎ `#endif` ⏎ `private readonly int _b;` — no fix on the branch; `main` and `551f629` offer it and a `DEBUG` build then runs `Log("c")` before `Log("a")`. Fixture c (layout, see #1134) is the same member for the layout set. No test covers either; the positive tests (`FixMovesExcludedCodeInFrontOfTheMovedFieldWithIt`, `…StaticFieldWithIt`) cross no initialized member | Spec author: SHARED-ORDER-007 states that members of excluded code in the moved declaration's leading trivia move with it and count as moved (so the move is refused when one of them has an initializer and a crossed member of its context has one); the change doc's case 10 names this input and corrects the sentence. Test implementer: RH7109 tests for fixtures a and c. Covered by the approved *Build configurations* bullet, as decision 11 reasons |
| 11 | medium | `RH7004UsingDeclarationsShouldNotBeUsedAnalyzerTests` against `SyntaxTriviaUtilities.SplitsDirectivePair` | FC-003, FC-017 | – | PR-introduced | The repair replaced pairing by kind with pairing per warning and per `#nullable` setting, added `GetWarningCode`, and made restore-all and setting-less `#nullable` end every earlier setting. The tests cover the offered side ((c), (d), (e)) and the withheld side only for pairs that name the same code or no code, which the old pairing also withheld. None fails if `GetWarningCode` stops equating `219` and `CS0219`, if a restore-all stops ending a single disable, or if `#nullable` stops pairing per setting: each would offer a fix that splits a pair (maintainer clarification on #1136). The code is right today (fixtures g1, g3, g5, g7, g8 withheld; g4, g6 offered) | Add withheld-side tests for `disable 219` … `restore CS0219`, `disable CS0219` … `restore`, `#nullable disable warnings` … `#nullable restore warnings`, `#nullable disable` … `#nullable restore warnings`, and the offered side for `#nullable disable annotations` … `#nullable enable warnings` |

Severity notes. #9 changes the memory layout of the user's struct in another build, a meaning-changing rewrite. It is pre-existing on `main` for RH7103, but it fails in the shared owner that decision 6 built for RH7103 and in the requirement (SHARED-FIX-003 *Run time*) the run adds, and it repeats #4's class, so it is an architecture signal (below), not a routine repair. #10 and #11 do not change the code's output; they leave a newly withheld fix and new guard conditions unstated or unpinned.

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| RH7004 conditional group in front of `}` | conditional directives in `}`'s leading trivia whose group `#if` starts behind the declaration's end | SHARED-FIX-003 *Build configurations* ("code that follows the declaration in another build") | yes | ok (#1 closed) |
| RH7004 brace split | every active directive of the root, paired per warning / `#nullable` setting, conditional and region groups as before, no end-of-file partner | SHARED-FIX-003 *Comments and directives in place* (a directive and the directive that ends its effect) | yes; over-withholds only for two `disable X` (decision 10) and a setting-less `#nullable` after both settings were ended (hint) | ok; tests #11 |
| RH7004 comment on the header line | semicolon's trailing trivia: holds a comment and ends with a line break | RH7004-004 "at the end of its line" | yes | ok (#5 closed) |
| Layout type | `INamedTypeSymbol.GetAttributes()` over every part; constructor argument value | SHARED-FIX-003 *Run time* | yes | ok (#2 closed) |
| Backing field | `FieldKeyword` tokens of the property | SHARED-FIX-003 *Run time* (the `field` keyword) | yes | ok (decision 8) |
| Single move ranks | `< target.FullSpan.Start` → 0, `moved.FullSpan` → 1, between → 2, behind → 3 | `MoveNodeBeforePreservingSeparators` (moved payload goes behind the target's gap and in front of the target's payload) | yes: excluded code in the target's trivia is crossed (fixture d: withheld), in the moved trivia it moves (fixtures a, c, i) | ok; texts #10 |
| Which sets count for a fix | RH7103: static and instance initializers; RH7109 and RH7309: also layout fields from the symbol | SHARED-FIX-003 *Run time* / *Build configurations* (every fix) | no — RH7103 drops the layout set | finding #9 |
| RH7309 documentation, closing brace | documentation trivia in members' and in the closing brace's leading trivia | RH7309-006 bullet 4, SHARED-FIX-003 *Documentation* | yes | ok (#3 closed) |
| RH7501 movable `break` | unchanged code; summary | RH7501-004 conditions 2, 3 | yes | ok (#7 closed) |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Member order (initializers, layout fields), counting excluded members | `OrderingDeclarationUtilities.ChangesMemberOrder` (rank overload and single-move overload), wrapper `ChangesInitializerExecutionOrder`; consumers RH7103 (flag `false`), RH7109 (`FieldOrderSetsLayout(symbol)`), `RegionBlockLayout.ChangesMemberOrder` → RH7309 (symbol). `rg "HasInitializer|IsLayoutField|ChangesLayoutOrder"`: no comparison outside the owner, `ChangesLayoutOrder` gone | one owner of the comparison; the layout set is still chosen per consumer — finding #9, architecture finding |
| Members of excluded code | `OrderingDeclarationUtilities.GetMembersOfDisabledText` only (moved out of `RegionBlockLayout`) | ok |
| Layout type | `FieldOrderSetsLayout(INamedTypeSymbol)`, `IsStructLayoutAttribute(AttributeData)`; `rg "StructLayout\|LayoutKind" src` outside tests: only `OrderingDeclarationUtilities` | ok |
| Brace split | `SyntaxTriviaUtilities.SplitsDirectivePair` (with `GetWarningCode`, consumer RH7004 only) plus RH7004's check in front of `}` | ok |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH7004 (a)–(e) `FixIsOffered…` (5, plus the LF/CRLF variant of (c)) | RH7004-004 for pairs SHARED-FIX-003 does not split | withheld, or another output | `Verify` (+ `DEBUG` transform), `ApplyCodeFixAsync` LF/CRLF | ok; withheld side of the new pairing missing (#11) |
| `FixKeepsACommentBehindTheUsingKeyword`, `FixKeepsACommentInFrontOfTheSemicolon` | comment kept in place, exact layout | dropped, moved, withheld | `Verify` | ok (#6 closed) |
| `FixKeepsACommentThatDoesNotEndItsLineBehindTheNewBlock` (+ both line endings) | RH7004-004 qualifier | `}}` or header-line move | `Verify`, `ApplyCodeFixAsync` LF/CRLF | ok |
| RH7109 partial/Auto/Explicit/named-argument tests (5); RH7309 partial tests (2) | layout from every part | fix offered for sequential, withheld for Auto/Explicit | `VerifyNoCodeFixIsOffered` / `Verify` | ok |
| `NoCodeFixWhenTheMoveCrossesAnInitializedFieldOfExcludedCode`, RH7103 counterpart; `FixCrossesAFieldOfExcludedCodeWithoutInitializer`; `FixMovesExcludedCodeInFrontOfThe…FieldWithIt` (2) | SHARED-ORDER-007 crossed excluded member, both sides | an action / no action | `VerifyNoCodeFixIsOffered` / `Verify` | ok; the moved-trivia side with a crossed initialized member is missing (#10) |
| `CodeFixReordersAStructWhosePropertyReadsAMemberNamedFieldBeforeCSharp14` | `field` identifier before C# 14 is no backing field | no fix | `Verify` with `LanguageVersion.CSharp13`; counter-side is the existing C# 14 test | ok |
| RH7309 `NoCodeFixWhenADocumentationComment…` (2), `CodeFixKeepsADocumentationCommentThatDocumentsNothing…` | RH7309-006 bullet 4, comment that documents nothing | an action / no action | `VerifyNoCodeFixIsOffered` / `Verify` | ok |
| RH7501 `case 4` tests (2), commented `break` example | RH7501-004 | an action; comment left behind | `VerifyNoCodeFixIsOffered` / `Verify` | ok |

### Visible output is decided

Every changed expected value of the repair is demanded by an accepted requirement or is `main`'s output: RH7004 (a)–(e) and the `/* c */ }` case are `main`'s outputs that RH7004-004 demands; the two #6 outputs keep comments that `main` drops, demanded by SHARED-FIX-003 condition 3, laid out by the formatter (first audit, decision 7); RH7109/RH7309 layout tests by RH7109-006, RH7309-006 and SHARED-FIX-003 *Run time*; the excluded-initializer tests by SHARED-ORDER-007, listed as case 10 (decision 11); RH7309 documentation tests by RH7309-006; RH7501 by RH7501-004 and the change doc's "shares its line with other code". No rule page changed. Unlisted visible changes: #10's withheld fix (fixtures a and c; demanded by the approved *Build configurations* bullet, so no output question under decision 11's reasoning, but the change doc must list it), and the repair of #9 would add one more withheld fix of the same kind.

### Failure classes dismissed

- FC-010, FC-011, FC-012, FC-013, FC-014, FC-028, FC-032: no formatter file changed.
- FC-018, FC-036: no rename.
- FC-022, FC-023, FC-025, FC-026, FC-029: no literal, identifier, expression-body, repeated-token, or key/value logic changed; `GetWarningCode` reads the literal's value, not its text.
- FC-007, FC-008: every new `Verify` with a fixed source runs the iterations and Fix All.
- FC-009: no new action that changes nothing.
- FC-030: whole-type formatting by RH7103/RH7109 stays with thoenissen/Reihitsu#969 (decision 9).
- FC-031: every changed summary and inline comment checked against the code (`ChangesMemberOrder` both overloads, `ChangesInitializerExecutionOrder`, `FieldOrderSetsLayout`, `HasBackingField`, `SplitsDirectivePair`, `WouldSeparateDirective`, the RH7004 inline comment, `ChangesDocumentedDeclaration`, `CanMoveBreak`, `IsMoveSafe`): they describe the new code.
- FC-033, FC-034: CRLF variants where the output is layout (#1 (c), #5); the guards of #2–#4 read attributes, members, and trivia kinds, not line structure.
- FC-035: the test implementer observed every "fails" test failing on the unchanged code before the implementer's commit.
- FC-037: no text-only claim.
- FC-004, FC-005, FC-019, FC-020, FC-021: walked for RH7004 (comments and each directive kind tested or traced), RH7309 (documentation trivia at the closing brace).
- FC-024: walked; partial types now resolve through the symbol.
- FC-001, FC-002, FC-003, FC-006, FC-015, FC-016, FC-017: walked; failing members are #9, #10, #11.

### Architecture findings

**The member-order decision has one comparison but its scope is still chosen per consumer.** `OrderingDeclarationUtilities.ChangesMemberOrder` takes `fieldOrderSetsLayout` from its caller. RH7109 and RH7309 pass `FieldOrderSetsLayout(typeSymbol)`; RH7103 reaches it through `ChangesInitializerExecutionOrder`, which passes `false`, a second statement of which sets apply to a fix. The input of #9 (a static member whose leading trivia holds excluded instance fields of a struct) is withheld by RH7109 (fixture c) and offered by RH7103 (fixture h). This is the class of #4 — the same decision, failing between the shared owner and one consumer — on a member #4's enumeration did not name, so it is handled as an architecture finding. Components: `src/Reihitsu.Core/OrderingDeclarationUtilities.cs`, `src/Reihitsu.Analyzer.CodeFixes/Rules/Organization/RH7103StaticElementsMustAppearBeforeInstanceElementsCodeFixProvider.cs`, both within the run.

### Hints

- RH7102 (outside the run, pre-existing on `main`): its fix moves a `const` together with the excluded code in its leading trivia and has no member-order guard, so a `const` that follows `#if DEBUG` instance fields or initialized fields reorders them in a `DEBUG` build (fixtures j: struct layout, k: `_i = Log("i")` then excluded `_c = Log("c")` then `const int K` — both offered). Structural cause: only RH7103 and RH7109 among the `TypeMemberOrderingCodeFixProviderBase` fixes ask the member-order owner; follow-up candidate.
- `ChangesMemberOrder` counts members of `#if false` blocks, which no build compiles; SHARED-ORDER-007 as written counts them, so this only over-withholds.
- `SplitsDirectivePair`: a setting-less `#nullable` directive still pairs with an earlier setting-less one after `#nullable … warnings` and `#nullable … annotations` directives have ended both settings (over-withholds; no caller input seen).
- `ChangesMemberOrder` now walks every trivia of the type and parses its disabled text for each RH7103/RH7109 registration; `typeDeclaration.ContainsDirectives == false` could skip the walk. `TypeMemberOrderingCodeFixProviderBase.RegisterCodeFixesAsync` fetches the semantic model also for RH7102, which does not use it. Code-fix registration is not a hot path.

### Follow-up candidates

- RH7102 fix: member order of excluded code in the moved `const`'s leading trivia (hint above).
