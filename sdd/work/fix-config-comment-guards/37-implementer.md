Role: implementer
Commit: (this commit; see `git log -1 -- sdd/work/fix-config-comment-guards/37-implementer.md`)
Verdict: tests pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the lead's recorded decisions (32, 34, 35) and maintainer answers (batches 3–6). No test assertion, input, or expected output was changed. The 14 red tests of `36-test-implementer.md` were taken as red from that handover; the first focused run after the change was green.

## thoenissen/Reihitsu#1593 (and the shared member-order guard)

### Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/OrderingDeclarationUtilities.cs` | Reset to main (57acf0d), then: decision 16's `HasModifiersSplitByConditionalDirective` re-added unchanged; the per-configuration machinery (symbol enumeration, per-assignment parsing, `MaximumSymbolCount`, `KeepsReorderAmongMembers`, the split-member readers) is gone (decision 32). New, decision 34's structural check: the private `ChangesMemberOrder(…, Func<int,int>)` first computes the reordered span (`GetReorderedSpan`: first position a later one goes in front of, to the last position that goes in front of an earlier one = moved + crossed span), collects the conditional groups with a directive in it, and refuses when one does not wrap whole units (`WrapsWholeUnits`). A group wraps whole units when it ends with `#endif`, no conditional directive of another group lies between its directives or open around it (counted `#if` vs `#endif` in front of it), all its directives stand at a boundary of one common unit list (`GetBoundaryLists`: members of a type body at or under the reordered type, attribute lists of such a member, accessors of such a member's accessor list, statements of a block or switch section inside a member's body block incl. lambdas/local functions; the token begins a unit or follows the last one — close brace, the token behind the last attribute list, the next switch section, the switch's close brace), and every excluded branch (`GetExcludedText`: the disabled text behind the directive) parses on its own as whole units of that list (`ReadsAsWholeUnits`: one parse through `ParseInTypeBody`, the same `class C { … }` wrapper main's `GetMembersOfDisabledText` uses — now via that helper, behaviour unchanged — with the text placed into an empty attribute-list / accessor-list / method-body context; no syntax error and exactly the wrapper's shape). For accessor lists and statement lists inside an accessor body, no `field` token in the active branch or any excluded branch (`ForbidsFieldKeyword`). No condition is evaluated, no branch combined, no limit. `HasBackingField`: a property also counts as having a backing field when an accessor without body/expression body stands in a branch of a conditional group of its accessor list, active or excluded (`HasConditionalAccessorWithoutBody`, same accessor parse). Summaries of the changed methods updated | SHARED-ORDER-008, SHARED-ORDER-007, SHARED-FIX-009 |
| `src/Reihitsu.Core/OrderedMember.cs` | Deleted (only the per-configuration reading used it) | decision 32 |
| `src/Reihitsu.Analyzer.CodeFixes/Base/TypeMemberOrderingCodeFixProviderBase.cs` | `IsMoveSafe` summary names SHARED-ORDER-008 (doc only; decision 4's code unchanged) | FC-031 |

Kept unchanged: decision 4 (base-class guard for every member-moving fix), decision 16 (RH7102/RH7103 analyzers), `RegionBlockLayout` (RH7309 reaches the same check through the rank overload).

### Production diff of `src/Reihitsu.Core/` against 57acf0d (decision 34)

| File | Added | Removed | Note |
|---|---|---|---|
| `OrderingDeclarationUtilities.cs` | 357 | 7 | 227 added lines that are neither blank nor comment/doc; of these about 15 are decision 16's helper and the `using`, the rest the SHARED-ORDER-008 check and the SHARED-ORDER-007 accessor count. At e8b2a713 (head before this change) the file was +462/−66 against main, plus `OrderedMember.cs` +52 |
| `ModifierOrderingUtilities.cs` | 173 | 1 | #1616, untouched in this round |
| `OrderedMember.cs` | 0 | 0 | deleted (was +52 at head) |

Main's whole-member parse (`GetMembersOfDisabledText`) and the ranking in `ChangesMemberOrder` are main's, line for line, apart from calling the shared `ParseInTypeBody` wrapper.

### Policy owners

- `rg -n "ChangesMemberOrder\(" src -g '!*Test*'`: one owner, `OrderingDeclarationUtilities`; callers `TypeMemberOrderingCodeFixProviderBase.IsMoveSafe` (RH7102/7103/7109) and `RegionBlockLayout.ChangesMemberOrder` (RH7309).
- `rg -n "ParseCompilationUnit\(\$\"class C" src/Reihitsu.Core`: one place, `ParseInTypeBody`, used by main's disabled-member parse and by the new branch parse.
- `rg -n "GetRelatedDirectives|DisabledTextTrivia" src/Reihitsu.Core/OrderingDeclarationUtilities.cs`: excluded text is read only in this file (main's parse, `GetExcludedText`, `HasConditionalAccessorWithoutBody`). `SyntaxTriviaUtilities.IsConditionalDirective` is private and takes trivia, so a three-type `DirectiveTriviaSyntax` check was added locally.

### Tests run

- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH7109ReadonlyElements|FullyQualifiedName~RH7103StaticElements|FullyQualifiedName~RH7102Constants|FullyQualifiedName~RH7309Regions|FullyQualifiedName~RH2001PrivateAuto"`: Passed 509, Failed 0.
- `scripts/test.sh --configuration Debug --project analyzer`: Passed 7638, Failed 0.
- `scripts/test.sh --configuration Debug --project core`: Passed 660, Failed 0.
- `scripts/test.sh --configuration Debug --project architecture`: Passed 115, Failed 0.
- `scripts/build.sh` (Release): 0 warnings, 0 errors. `scripts/format.sh` on both changed files: 0 changed.
- Probes with `scripts/apply-fix.sh` (fixtures removed afterwards), LF and CRLF alike: excluded `private static int _z = 2;` in the moved field's trivia crossing an initialized static: no fix; one excluded branch holding a member plus a dangling attribute list: no fix; `#if/#elif/#else` of whole members in the crossed span: fixed; `#if` in one block and `#endif` in a sibling block of a crossed method: no fix; `#if DEBUG get; #else get => 0; #endif` in a class (no layout) crossed by RH7103: fixed; whole statements in a local function of a crossed method: fixed.

### Disputed tests or scope

- Accessor backing-field count reads the active accessors between a group's `#if` and `#endif` and every excluded accessor text of the accessor list, also for properties outside the reordered span; such properties cannot change the order, so this only costs a parse.
- `field` is matched by token text (keyword or identifier), so an identifier named `field` in an accessor group also withholds — the safe side; the spec says "contains `field` as a token".
- An excluded accessor-list branch holding only a comment is parsed as `int P { /* … */ }`; if Roslyn ever reports that as a syntax error the move is withheld (safe side; no test pins it).
- `GetReorderedSpan` evaluates the rank function once per character of the type body (RH7309's rank is O(blocks)); acceptable for a code fix, noted for the auditor.

### Architecture findings

_None._

### Follow-up candidates

_None new._

## thoenissen/Reihitsu#1595

No production change in this round. The `#if false` test (`NoCodeFixWhenAnInitializedFieldOfAnIfFalseBlockInFrontOfTheConstantWouldCrossAnInitializedField`), red at head, passes again with main's whole-member reading; the two retagged split-modifier tests refuse by place under SHARED-ORDER-008. Decision 4 and 16 code unchanged.

## thoenissen/Reihitsu#1616

No production change; all RH2001, RH7105, RH7106 tests pass. `ModifierOrderingUtilities.cs` and the RH2001 fix are untouched.
