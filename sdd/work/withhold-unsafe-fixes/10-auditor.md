Role: auditor
Commit: 551f629
Verdict: findings (#1136: findings; #1134: findings; #1135: findings; #1262: findings; #1332: findings; #1336: findings; #1340: pass)

Inputs: the seven issues as named in `00-lead.md`, `00-lead.md`, handovers `01`–`09`, the change docs `sdd/changes/2026-10/withhold-unsafe-fixes.md` and `sdd/changes/2026-10/silence-uncorrectable-namespaces-and-regions.md`, the specs they name, and the diff `a59a003..551f629`. The prompt carried no intended fix, suspected cause, or hoped-for verdict. Every observation below comes from `scripts/apply-fix.sh <ID>` on fixtures in the session scratchpad (documentation mode `Parse`, symbols only where a fixture starts with `#define`, LF and CRLF alike), run on this branch and, where marked "main", on a worktree of `a59a003`. No test was re-run: no finding turns on a test result.

## Scope

Full audit. Head `551f629` (checked out, clean). Base `origin/main` = `a59a003f6a9f85126892eac7646fc8945a4b9073`, an ancestor of the head. The diff matches the handovers: 13 named production files plus `SyntaxTriviaUtilities.cs` and the new `RegionBlockLayout.cs`, the tests of `06-test-implementer.md`, the specs and the two change docs. `scripts/lane-check.sh behavior-change`: confirmed.

Finding numbers start at #1 (first audit of the run). The findings table is under `Whole run`, because several findings span issues; each issue section names its findings.

## thoenissen/Reihitsu#1136

Findings: #1 (SHARED-FIX-003 brace split, RH7004 consumer), #4 (build configurations, RH7109).

SHARED-FIX-003 as written holds in the RH7004 and RH7501 examples (tests and traces). The brace-split predicate never withholds too little: grouping every `#pragma warning` (or `#nullable`, `#line`) with the next one of its kind only ever cuts a governed span into smaller spans, and any brace pair that splits the real span then splits one of the pieces. It withholds too much (#1).

## thoenissen/Reihitsu#1134

Findings: #2 (layout of a partial type), #4 (initializer in disabled text, RH7109).

Holds: default struct, `record struct`, `ref struct`, nested struct, a part of a partial struct, `[StructLayout(Sequential)]` on a class and a record, fully qualified with `Pack = 1`; `Auto`, `Explicit`, a plain class, static fields offered. Traced `ChangesLayoutOrder`: RH7109 moves a field in front of a non-readonly instance field of its group, so in a layout type every instance move crosses a layout field and is withheld, as required.

## thoenissen/Reihitsu#1135

Findings: #2 (RH7309 member), #4 (architecture: two owners of the initializer-order decision).

Holds: the issue input, a member between blocks, instance/event/property initializers, an initializer in disabled text of a block (`b_disabled_init_cross`: no fix), static versus instance, `const`, nested types, struct fields, auto-property, field-like event, `field` property. Traced `RegionBlockLayout.ChangesOrder`: members of one segment keep their order, gaps keep their rank, so the comparison is exact for the permutation.

## thoenissen/Reihitsu#1262

Findings: #3 (RH7309 documentation comment that documents nothing).

Holds: RH7303 both straddling shapes (the partner predicate is the analyzer's own `IsWithinElementBody`); RH7304-005 pairing (compiler pairing in analyzer and fix now agree); RH7304 fix withheld for `/* c */` and a multi-line comment in front of the first token; RH7306 descriptions judged by the analyzer's predicate, the word alone withheld for the reported directive and the partner; RH7309 comment directly above and across blank lines above a moved block (`d_comment_behind_endregion_prev_block`, `e_three_blocks_middle_stays`: no fix), comment above a block that keeps its place (fix), the three `///` shapes of RH7309-006 (no fix), documentation comments directly on members of moved blocks (`c_doc_above_member_in_moved_block`: fix, each comment stays with its member).

## thoenissen/Reihitsu#1332

Findings: #7 (RH7501-004 wording against the code).

Holds: every shape of the change doc and RH7501-004; `/* exit */ break; // done` on its own line moves with both comments (branch and main agree); the lone `break` keeps its empty block. `CanMoveBreak` inspects exactly the trivia between the token in front of the `break` and the closing brace (the previous token's trailing trivia, every trivia of the `break`'s tokens, the brace's leading trivia), which is the span of condition 3.

## thoenissen/Reihitsu#1336

Findings: #1 (RH7004 withheld where it was correct), #5 (RH7004 comment not at the end of its line), #6 (RH7004 comment tests), #8 (modifier-ordering doc comment).

Holds: A2 (`// resource` on the header line, LF and CRLF), `/* a */ // b` behind the declaration (`f_two_comments`), `using /* c */ var`, a comment in front of `;` (kept, see #6 for the layout), RH7101-006 in modes `Parse`/`Diagnose` with `///`, `/** */`, attributes, and the behind-code form; RH7105/RH7106 with a comment or any directive in front of the token after the last modifier.

## thoenissen/Reihitsu#1340

Pass. `IsEligibleNamespace` returns false for any `BaseNamespaceDeclarationSyntax` member, which is RH7003-005; a namespace holding only types stays reported (existing test). The `#if`-excluded nested namespace stays the recorded follow-up candidate.

## Whole run

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | high | `RH7004UsingDeclarationsShouldNotBeUsedCodeFixProvider.WouldSeparateDirective`; `SyntaxTriviaUtilities.SplitsDirectivePair` | FC-002, FC-016 (guard wider than the decision) | – | PR-introduced | The RH7004 fix is withheld for inputs that SHARED-FIX-003 does not withhold and RH7004-004 requires, each of which `main` fixes correctly. Failing members, all confirmed on the branch (`no-fix-offered`) and on main (correct fix): (a) `#if DEBUG` before the using declaration, `#endif` in front of the enclosing `}` — the closing-brace check counts any `#endif`, although the declaration lies inside the group, so no other configuration has code that "follows the declaration"; (b) the same with `#else` + disabled text + `#endif` in front of `}`; (c) `#pragma warning disable CS0219` among the following statements with no later `#pragma warning` — `SplitsDirectivePair` pairs it with the end of the file, but SHARED-FIX-003 splits only "a directive and the directive that ends its effect"; (d) `#nullable enable` among the following statements, same mechanism; (e) `#pragma warning disable CS0169` at type level and a balanced `disable/restore CS0219` pair wholly among the following statements — the predicate pairs every `#pragma warning` with the next one of any code, so `disable CS0169`…`disable CS0219` reads as a pair the new block splits. Members that hold: the genuine splits of the tests (one brace between `#if`/`#endif`, `#pragma`, `#nullable`, `#region` pairs), both braces between a pair, a pair wholly among the following statements ending before the last statement. The change doc lists none of (a)–(e), so each is also a visible change nobody decided. Example (a): `void M() {⏎#if DEBUG⏎ using var s = new MemoryStream();⏎ s.Flush();⏎#endif⏎}` — main gives `using (var s = new MemoryStream()) { s.Flush(); }` inside the `#if`; the branch offers no fix | Withhold on the closing brace only for a conditional group whose `#if` lies behind the declaration; pair `#pragma warning` by code (`disable X` … `restore X`) and do not treat the end of the file as a partner. Add tests for (a)–(e) |
| 2 | high | `OrderingDeclarationUtilities.FieldOrderSetsLayout` (consumers `ChangesLayoutOrder` → RH7109, `RegionBlockLayout.ChangesMemberOrder` → RH7309) | FC-024, FC-016 | – | same requirement | The layout decision reads only the attribute lists of the declaration in hand. Failing members: (a) RH7109, `partial class P` with `[StructLayout(LayoutKind.Sequential)]` on another part, `public int _a; public readonly int _b;` — fix offered, `_b` moved in front of `_a` (confirmed): the layout changes, against RH7109-006 and SHARED-FIX-003 *Run time*; (b) RH7309, the same partial class with `_handle` in an `IDisposable` block and `_a` in a later `Fields` block — fix offered, `_a` moved in front of `_handle` (confirmed), against RH7309-006 bullet 2; (c) a record with the attribute on another part — same mechanism (traced); (d) over-withholding, same mechanism: a partial struct with `Auto` or `Explicit` on another part (traced), and `[StructLayout(layoutKind: LayoutKind.Auto)]` with a named-colon argument, which the argument filter skips (confirmed: no fix for a struct the spec reports as offered). Members that hold: attribute on the same part, partial struct without any attribute, every test shape. The lead lists (a) as a follow-up candidate; it is a failing member of #1134's own class (FC-024 names partial types), so it is reported here | Decide the layout from the type symbol (`INamedTypeSymbol.GetAttributes()` merges the parts; the RH7309 fix already holds the symbol, the RH7109 provider can pass the semantic model's symbol to the guard), which also resolves aliases and named arguments. Add tests for (a), (b), (d) |
| 3 | high | `RH7309RegionsShouldFollowCategoryOrderCodeFixProvider.ChangesDocumentedDeclaration` | FC-001 | – | same requirement | The check inspects only documentation comments in the leading trivia of a member, so it sees only a comment that documents something before the fix. A `///` comment that documents nothing — it stands in the leading trivia of the type's closing brace — and lies in a block that moves documents a declaration after the fix, which SHARED-FIX-003 *Documentation* ("or a declaration it did not document is") and RH7309-006 bullet 4 forbid. Confirmed, mode `Parse`: blocks `IDisposable` then `Methods`, with `/// <summary>Stray</summary>` directly above `#endregion // Methods` (the last block) — fix offered, the comment now stands above `#endregion // Methods` ⏎ `#region IDisposable` ⏎ `public void Dispose()` and documents `Dispose`; the same with `} /// <summary>Stray</summary>` behind the closing brace of `M` (shape 1 in the last block). Members that hold: the three spec shapes (the comment documents a member of the next block or the gap), comments directly on members, a `///` above a `#region` (withheld by `SeparatesCommentFromBlock` when its block moves), mode `None` | Also check the documentation comments in the leading trivia of the type's closing brace (they must stay behind the last segment, or the fix is withheld); add tests for both confirmed shapes |
| 4 | high | `OrderingDeclarationUtilities.ChangesInitializerExecutionOrder` (RH7109) against `RegionBlockLayout.ChangesMemberOrder` (RH7309) | FC-015, FC-016 | – | same requirement | The run's SHARED-FIX-003 *Build configurations* bullet counts a run-time difference that only another build shows, and RH7309 applies it (members of disabled text count), but RH7109 does not. Confirmed: `int _a;` ⏎ `#if DEBUG` ⏎ `int _c = Log("c");` ⏎ `#endif` ⏎ `int _d;` ⏎ `readonly int _b = Log("b");` without `DEBUG` — the RH7109 fix moves `_b` in front of `_a`, so in a `DEBUG` build `Log("b")` now runs before `Log("c")`. Members that hold: the disabled text in the moved field's own leading trivia, which travels with it (SHARED-ORDER-006; confirmed, order unchanged); RH7309 with an initializer in a block's disabled text (no fix); RH7109 in a layout struct (withheld by the layout guard whatever the disabled text holds). Member outside the run: RH7103 uses the same predicate (follow-up). Also listed under *Architecture findings* | Give the initializer-order decision one owner that counts members of disabled text the way `RegionBlockLayout` does, and use it for the single move as well; add an RH7109 test for the confirmed input |
| 5 | medium | `RH7004UsingDeclarationsShouldNotBeUsedCodeFixProvider.ApplyCodeFixAsync` (`semicolonToken.TrailingTrivia.Any(IsCommentTrivia)`) | FC-002 | – | PR-introduced | RH7004-004 keeps a comment behind the declaration "at the end of its line" on the header line; the code moves any comment in the semicolon's trailing trivia and keeps only an end-of-line trivia for the statement. For a comment that does not end its line the separator is lost. Confirmed: `using var s = new MemoryStream(); /* c */ }` gives `using (var s = new MemoryStream()) /* c */` ⏎ `{` ⏎ `}}` — the new block's brace and the method's brace joined without a space (main: `} /* c */ }`). SHARED-FIX-003 leaves a comment between two tokens of one line undecided, so this output changed without a decision. Members that hold: a comment that ends the line (`// resource`, `/* a */ // b`) | Apply the header-line move only when the trailing trivia ends with a line break, as the requirement states |
| 6 | medium | `RH7004…AnalyzerTests.OfferedFixKeepsACommentBehindTheUsingKeyword`, `OfferedFixKeepsACommentInFrontOfTheSemicolon` (`AssertEveryOfferedFixKeeps`) | FC-017 | – | PR-introduced | The helper asserts only that the comment text occurs in each offered result. It passes when no fix is offered, and it cannot see a comment that moved: SHARED-FIX-003 condition 4 requires `/* c */` to stay behind `using` and `// c` to stay on the line in front of `)`, which the `;` is rewritten into. The real output for the semicolon input is `using (var s = new MemoryStream()` ⏎ (blank line) ⏎ `// c` ⏎ `)` — neither placement nor the inserted blank line is pinned, and the change doc does not list either output | Assert the full fixed output (`Verify` with the fixed source) for both inputs, so that a regression to dropping, moving, or withholding fails |
| 7 | low | `sdd/specs/analyzer/RH7501.md` RH7501-004 condition 2; summary of `RH7501…CodeFixProvider.CanMoveBreak` | FC-002, FC-031 | – | same requirement | RH7501-004 requires the fix when "the `break` is the first statement on its line"; the code withholds whenever any token precedes the `break` on its line. For `{ break;` ⏎ `}` all three conditions of RH7501-004 hold and the fix is withheld (confirmed; main offered it and wrote `{                 }`). The change doc ("withheld when the `break` shares its line with other code") decides the code's behavior, so the requirement text and the method summary are what disagree | Spec author: "the `break` is the first token on its line"; the summary of `CanMoveBreak` to match |
| 8 | low | `ModifierOrderingCodeFixProviderBase.ReorderingWouldDropCommentOrDirective` `<returns>` | FC-031 | – | PR-introduced | The summary now covers a comment or directive that would be relocated, and disabled text; `<returns>` still says "when a comment or directive would be dropped" | Align `<returns>` with the summary |

Severity notes. #1 is a fix withheld where it was safe; `AGENTS.md` (*Code-fix coverage*) calls that a defect of the guard's scope. It is reported high because it breaks RH7004-004's `MUST` for inputs SHARED-FIX-003 does not withhold, regresses five correct outputs of `main`, and changes visible output that no change doc lists. #2–#4 change the program or the documentation of the user's code.

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| RH7004 brace split (SHARED-FIX-003) | every directive of the root, governed spans per kind with the end of the file as partner; plus `}`'s leading trivia for any conditional directive or disabled text | SHARED-FIX-003 *Comments and directives in place* (pair = directive and the directive that ends its effect) | no — wider (#1) | finding #1 |
| RH7004 comment on the header line | any comment in the semicolon's trailing trivia | RH7004-004 "at the end of its line" | no — qualifier missing (#5) | finding #5 |
| RH7101 documented declaration | documentation-comment trivia in the declaration's leading trivia | RH7101-006 "in its leading trivia", modes `Parse`/`Diagnose` through the parser | yes | ok |
| RH7105/RH7106 trivia behind the last modifier | leading trivia of the token after the last modifier: comment, directive, disabled text | SHARED-MODORDER-003 | yes | ok |
| RH7109/RH7309 layout type | attribute lists of the one declaration | SHARED-FIX-003 *Run time* (the type) | no — one part of a partial type (#2) | finding #2 |
| RH7109 initializer order | active members between target and moved member | SHARED-ORDER-007 + SHARED-FIX-003 *Build configurations*; counterpart `RegionBlockLayout.ChangesMemberOrder` | no — disabled text not counted (#4) | finding #4 |
| RH7303 removable partner | `IsWithinElementBody(partner)` | RH7303 analyzer `IsWithinElementBody(directive)`; RH7303-005 | yes (same predicate) | ok |
| RH7304 pair skip | body region directives at or between the pair's two positions | RH7304-005 | yes | ok |
| RH7304 fix indentation | text from line start to the region's first token | SHARED-FIX-003 condition 1 | yes | ok |
| RH7306 suffix | descriptions via the analyzer's `GetDescription`/`EndsWithForbiddenSuffix`/`IsForbiddenSuffixAlone` | RH7306-001, -003, -004 | yes (one owner) | ok |
| RH7309 comment above a moved block | leading trivia of the `#region`'s token, back over whitespace and line breaks | RH7309-006 bullet 3, RH7309-004 | yes | ok |
| RH7309 documentation | documentation comments in members' leading trivia | RH7309-006 bullet 4, SHARED-FIX-003 *Documentation* (both directions) | no — comments that document nothing (#3) | finding #3 |
| RH7309 member order | type members + members parsed from body-level disabled text, by segment rank | RH7309-006 bullets 1, 2; SHARED-FIX-003 *Build configurations* | yes (except #2) | ok |
| RH7501 movable `break` | line of the previous token's end versus the `break`; `break` end versus `}`; trivia from the previous token to `}` | RH7501-004 conditions 2, 3 | no — "first statement" versus first token (#7) | finding #7 |
| RH7003 eligibility | `Members.Any(BaseNamespaceDeclarationSyntax)` | RH7003-005 | yes | ok |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Initializer execution order (SHARED-ORDER-007) | `OrderingDeclarationUtilities.ChangesInitializerExecutionOrder` (RH7103, RH7109); `RegionBlockLayout.ChangesMemberOrder` (RH7309); both use `HasInitializer`/`IsStatic` | two owners of the order decision, diverging on disabled text — finding #4, architecture finding |
| Layout type and layout fields | `OrderingDeclarationUtilities.FieldOrderSetsLayout`, `IsLayoutField` (only owner of `StructLayout`/`LayoutKind` in `src/`); consumers `ChangesLayoutOrder`, `RegionBlockLayout` | one owner; its scope is finding #2 |
| Brace split | `SyntaxTriviaUtilities.SplitsDirectivePair` (consumer RH7004 only); RH7004's private closing-brace check adds the other-configuration part | one owner plus a fix-local addition; both over-wide (#1) |
| RH7306 suffix | `RH7306…Analyzer.EndsWithForbiddenSuffix`, `ForbiddenSuffix`, `IsForbiddenSuffixAlone`, `GetDescription`; the fix's private copy is removed | ok |
| Region pairing | `RegionDirectiveUtilities.GetRegionPairs`, `TryFindMatchingDirective`; RH7304 analyzer and fix now both pair as the compiler | ok |
| Documentation-comment trivia | `SyntaxTriviaUtilities.IsDocumentationCommentTrivia` (RH7101, RH7309) | ok |
| Comment/directive between modifiers | `SyntaxNodeUtilities.IsCommentOrDirective` (modifier base) | ok |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `NoCodeFix…` tests (64 with mode `Parse`, 4 with `Diagnose`) | the diagnostic is reported and no action is registered for any reported diagnostic | an action registered for any diagnostic of the source | `VerifyNoCodeFixIsOffered`: `Verify` on the marked source, then registration for every diagnostic, both with the given mode and symbols | ok |
| `RH7004…FixKeepsACommentBehindTheDeclarationOnTheHeaderLine` (+ LF/CRLF variant) | exact output with the comment on the header line | any other output | `Verify` / `ApplyCodeFixAsync` per line ending | ok |
| `RH7004…OfferedFixKeepsACommentBehindTheUsingKeyword`, `…InFrontOfTheSemicolon` | SHARED-FIX-003 conditions 3 and 4 | comment dropped or moved | `AssertEveryOfferedFixKeeps` (text contains) | weak — finding #6 |
| `RH7004…FixIsOfferedForAPragmaPairAroundTheDeclarationAndEveryFollowingStatement`, balanced `#if` pair tests | the fix is offered and exact output | withheld or other output | `Verify` | ok; the over-withheld members of #1 have no test |
| `RH7109…NoCodeFixFor…` (9) and the counter-side tests | layout guard on both sides | a swap of instance fields offered | `VerifyNoCodeFixIsOffered` / `Verify` | ok; partial type on another part missing (#2) |
| `RH7309…NoCodeFixWhen…` documentation shapes | RH7309-006 bullet 4 | the comment handed to another member | `VerifyNoCodeFixIsOffered` | ok for the documenting direction; the "documents nothing" direction missing (#3) |
| `RH7309…CodeFixKeepsACommentAboveABlockThatKeepsItsPlace`, `…InDocumentationModeNone` | exact reorder output | other output, or no fix | `Verify` (iterations and Fix All) | ok |
| `RH7304…OfferedFixKeepsTheCodeParseable…` | SHARED-FIX-003 condition 1 | an offered result that does not parse | every offered fix parsed | ok (withholding satisfies the requirement) |
| `RH7304…RegionsWithADirectiveInsideExecutableCodeAreNotReported` and siblings | RH7304-005 | any report | `Verify` | ok |
| `RH7306…NoCodeFixWhen…` (3), `CodeFixRemovesTheTrailingWordFromAPartnerWithoutASpaceBehindTheCommentMarker` | RH7306-003/-004 | an action, or another output | `VerifyNoCodeFixIsOffered` / `Verify` | ok |
| `RH7501…NoCodeFix…` (12), `VerifyCodeFixLeavesEmptyBlockWhenBreakIsOnlyStatement` | RH7501-004 | an action; the block removed | `VerifyNoCodeFixIsOffered` / `ApplyCodeFixAsync` | ok |
| `RH7101…DocumentationComment…WithholdsTheFix…` (9), mode-`None` tests (7) | RH7101-006, RH7101-004 in mode `None` | an action in `Parse`/`Diagnose`; another split output in `None` | `VerifyNoCodeFixIsOffered` / `Verify` with mode | ok |
| `RH7105`/`RH7106` modifier tests | SHARED-MODORDER-003 | an action | `VerifyNoCodeFixIsOffered` | ok |
| `RH7003…NamespaceThatContains…` (3) | RH7003-005 | a report | `Verify` | ok |

### Visible output is decided

Every changed expected value in the tests is demanded by an accepted requirement (RH7003-005, RH7004-004, RH7101-006 with maintainer answer 4, RH7109-006, RH7303-005, RH7304-005, RH7306-004, RH7309-004/-006, RH7501-004, SHARED-MODORDER-003) or keeps `main`'s output; no rule page changed. Visible changes that no test pins and no change doc lists: #1 (fix withheld for five shapes `main` fixed correctly — restoring `main`'s output is demanded by RH7004-004, so no output question is needed if the repair does that), #5 (`}}` instead of `} /* c */ }`; restoring `main`'s output needs no question, any other output for that undecided comment position is an output question), #6 (output for a comment in front of `;`, now kept, demanded by SHARED-FIX-003 condition 3; its layout is the formatter's).

### Failure classes dismissed

- FC-010, FC-011, FC-012, FC-013, FC-014, FC-028, FC-032: no formatter file changed; the fixes call the formatter's entry points as on `main`.
- FC-018, FC-036: no rename.
- FC-022, FC-023, FC-025, FC-026, FC-029: no literal, identifier, expression-body, repeated-token, or key/value logic changed; RH7501 resolves the `break` and the brace by node, not by text.
- FC-030: the whole-member/type formatting of RH7105/RH7106/RH7109 is pre-existing (thoenissen/Reihitsu#969 and the recorded follow-up); the run neither depends on nor exposes it.
- FC-007, FC-008: every new `Verify` with a fixed source runs the iterations and Fix All; the withheld fixes have no action; RH7306 no longer registers a no-progress action.
- FC-009: RH7306 `//Implementation` is now withheld; the RH7309 identity order is the recorded pre-existing follow-up.
- FC-033, FC-034: CRLF variants and both symbol configurations are tested where the guard reads lines or symbols; the issues' own shapes are the reproductions.
- FC-035: expected values are spec examples or `main`'s outputs, observed before the implementer's commit.
- FC-037: no text-only claim.
- FC-003, FC-005, FC-019, FC-020, FC-021, FC-027: walked; each guard has a test on both sides of its boundary, comments and directives are tested separately; the failing members are #1, #3, #5.
- FC-006, FC-024: walked; failing member #2.

### Architecture findings

**The decision whether a reorder changes the execution order of initializers (SHARED-ORDER-007, with SHARED-FIX-003 *Build configurations*) has two owners.** `OrderingDeclarationUtilities.ChangesInitializerExecutionOrder` answers it for a single forward move (RH7103, RH7109) and counts active members only; `RegionBlockLayout.ChangesMemberOrder`, new in this run, answers it for a permutation of region blocks (RH7309) and also counts members of disabled text. Both use `HasInitializer` and `IsStatic`, but they disagree on which members exist: the input of finding #4 is withheld by RH7309 and fixed by RH7109. The same split exists for the layout decision (`ChangesLayoutOrder` against the layout branch of `ChangesMemberOrder`), where it does not show today because every RH7109 instance move in a layout type already crosses a layout field. Components: `Reihitsu.Core/OrderingDeclarationUtilities.cs`, `Reihitsu.Analyzer.CodeFixes/Core/RegionBlockLayout.cs`; within the components the run changes.

### Hints

- `OrderingDeclarationUtilities.HasBackingField` treats an identifier `field` in a property body as the `field` keyword under every language version, so a property that reads a member named `field` before C# 14 counts as having a backing field (over-withholds; traced, not run).
- Pre-existing, on `main`: the RH7004 fix for `using var s = …;` ⏎ `#nullable enable` ⏎ `string? t = null;` ⏎ `s.Flush();` inserts a blank line inside the new block and reports `line-ending-drift` under CRLF. The branch withholds this input (#1, member d), so the defect will surface again once #1 is repaired.

### Follow-up candidates

- RH7103 crosses initializers in disabled text with the same predicate as RH7109 (#4's member outside the run).
