Role: auditor
Commit: 7a9f5d9
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

- Head `7a9f5d9ffdf09c48b470c0cf310c9d2cda4ec5f1`; the checkout is that SHA, and the tree is clean.
- Base `origin/main` `0866955f2b10e15906e33c02b3d4954006042cf7` (`git ls-remote` agrees). It is an ancestor of the head.
- This is a full audit of thoenissen/Reihitsu#1767, the run's first audit.
- The diff matches the handovers 01–12:
  - the kind lists widened by `SyntaxKind.UnionDeclaration`;
  - `ParameterListParentPolicy`, `AttributeTargetUtilities`, and the RH5108/RH5110 union actions;
  - the new rules RH2110 and RH5418 with their resources, ruleset, solution items, rule pages, and package rows;
  - the adjacent fix in `TargetAttributePlacementCodeFixProviderBase` (lead decision 16);
  - the specs, the change doc (Accepted), the tests, and the `FormatterTestsBase` overload.
- `scripts/lane-check.sh behavior-change` confirms the lane.

How I checked:
- I read the code statically and opened the counterparts that are not in the diff: `SyntaxIndentationUtilities.IsIndentingAncestor`/`IsBetweenBraces`, `LineBreakListRewriter.CollapseOwnedOpenParenToDeclarationLine`, `EmptyTypeDeclarationSemicolonAnalysisUtilities`, the empty-type fix base, the RH2004 fix's default-accessibility switch, and every consumer of `AttributeTargetUtilities`.
- I ran `scripts/apply-fix.sh` on HEAD for RH2004, RH5105, RH5106, RH5108, RH5110, RH5404, RH5418, RH5507, RH5508, RH7102, RH7103, RH7104, RH7105, and RH7106, over a flat corpus of 6 union fixtures in the scratchpad. The corpus covers:
  - a doc comment, `readonly`, a union in an interface, and attributes on a nested union;
  - a comment, and an `#if` around a constraint clause;
  - `(` and `)` and the first case type each on another line, and a comment between the name and `(`;
  - a one-line body, `[A, B]` on an empty body, a `#if` in the body, and a base list;
  - `internal protected`, `readonly internal protected`, and a nested `partial` union in two parts;
  - out-of-order members.
- Every fix output compiles in shape and matches the struct sibling. Every status was `fixed`, `no-diagnostic`, or `no-fix-offered`; the only `no-fix-offered` is the `#if` body under RH5418, the pre-existing SHARED-DIAG-008 defect (lead decision 14).
- I ran `scripts/format.sh` twice over the same corpus. The second pass formatted 0 of 6 files. The formatted output then gives `no-diagnostic` under RH5105, RH5106, RH5108, RH5110, RH5404, RH5507, and RH5508 (FC-013 holds). RH5418 reports only the `#if` body again.
- I built a worktree of `main` at the base SHA in the scratchpad (now removed) to compare the adjacent fix (finding #1).
- The fixtures are in the scratchpad, outside the repository.

## Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | medium | `src/Reihitsu.Analyzer.CodeFixes/Base/TargetAttributePlacementCodeFixProviderBase.cs:227-231` (`HasUnusableEnclosingScope`); tests `RH5505…`, `RH5507…`, `RH5521…AnalyzerTests` | FC-016, FC-017 (missing test for the members of the class the fix changes) | – | PR-introduced | The adjacent fix does more than the handovers say (details below the table): it ends an NRE on `main` for every type declared with a semicolon, not only unions. Only the union member has a test, and the claim in `08-implementer.md` that the other kinds keep their behavior is false. | Add one test each for the members that now get a fix (below the table), then rerun the focused tests. The lead extends decision 16, and the PR note, to name the crash fix for RH5505, RH5507, and RH5521. |
| 2 | low | `documentation/rules/RH5106.md:12` | FC-031 (documentation drift) | – | same requirement | RH5106-001 now covers a union's case-type list (Q1). The page still lists the owners it covers ("methods, constructors, … records, primary constructors, extension blocks and anonymous methods") and does not name unions. The run added "the case-type lists of unions" to the same list on `RH5105.md`, and both rules share one owner, `ParameterListParentPolicy`. Text only; it cannot reach compiled behavior or rule metadata. | Add "the case-type lists of unions" to that sentence, as on `RH5105.md`. Q1 A covers the visible change. |

**Finding 1 in detail.**

The crash on `main`:
- `HasUnusableEnclosingScope` reads `found.Open.SyntaxTree.GetText()` for every enclosing brace pair whose open brace starts before the attribute list.
- A type declared with a semicolon has a default `OpenBraceToken`. Its `SpanStart` is 0 and its `SyntaxTree` is null.
- So on `main`, the SeparateLine placement fix throws a `NullReferenceException` whenever such a type's attribute list does not start at position 0 of the file.
- Lead decision 16 records this only for RH5507 on a union. `08-implementer.md` (*Disputed tests or scope*) says that for a class, struct, or record declared with a semicolon "their behavior is unchanged".

The class enumerated, with each rule's placement fix on `using System;` followed by the declaration:

| Fixture | `main` | HEAD |
|---|---|---|
| RH5505, `[Serializable] internal class Plain;` | `apply-fix: Object reference not set to an instance of an object.` | fixed |
| RH5505, `[Serializable] internal record Example(int X);` | NRE | fixed |
| RH5507, `[Serializable] internal struct Empty;` | NRE | fixed |
| RH5507, `[Serializable] internal record struct Pair(int X);` | NRE | fixed |
| RH5521, `[Obsolete] internal interface IMarker;` | NRE | fixed |
| RH5507, union | NRE | fixed, and the only member with a test (`UnionAttributeListOnDeclarationLineIsReportedAndMoved`) |

Members that hold on `main` too:
- an attribute list at position 0 of the file. The existing tests `VerifyDiagnosticAndCodeFixForRecordDeclaration` (RH5505) and the record-struct test in RH5507 use this shape, which is why they never crashed;
- an attribute list inside a type that has braces.

The tests to add: one RH5505 test (class or record), one RH5507 test (struct or record struct), and one RH5521 test (interface). In each, a type declared with a semicolon follows other code, as in `using System;⏎⏎[A] internal record R(int X);`.

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| A type declared with a semicolon is no brace scope for the placement fix's column check (`braces is { Open.RawKind: None }`) | The open-brace token of each ancestor in the switch | `SyntaxIndentationUtilities.IsIndentingAncestor` → `IsBetweenBraces`. With default braces it returns `spanStart > 0 && spanStart < 0`, which is false, so `ComputeBaseIndentLevel` never counts such a type | Agree: both treat a braceless type as no scope. Only a type declaration can have `None` here; a missing brace keeps `OpenBraceToken` as its kind and still reaches the `IsMissing` check | ok (tests missing, #1) |
| RH5105/RH5106 cover a union's case-type list (`ParameterListParentPolicy`) | `ParameterListSyntax.Parent is UnionDeclarationSyntax` | Formatter `LineBreakListRewriter.CollapseOwnedOpenParenToDeclarationLine`; FMT-BREAK-011 joins union `U⏎(`, `U(⏎`, `…⏎)` (fix output equals formatter output in the corpus; the comment and directive gaps stay silent under SHARED-JOIN-001) | Agree | ok |
| RH5108 checks the first case type (`UnionDeclarationSyntax { ParameterList: { } }`) | The first parameter of the case-type list; a `partial` part without a list is skipped | FMT-BREAK-012 join | Agree (A5, A6: formatter output gives `no-diagnostic`) | ok |
| RH5110 checks union constraint clauses | `UnionDeclarationSyntax.ConstraintClauses`, through the shared `AnalyzeConstraintClauses` | FMT-BREAK-023 / FMT-INDENT-017 | Agree. The bodiless form (`;` behind the clause), comments, and `#if` all match | ok |
| A union's attribute target is `Struct` (`TryResolveImplicitTarget`) | The attribute list's parent `UnionDeclarationSyntax`, also through `type:` | The compiler applies union attributes to the struct target (CS0592 on `Class`, 02-skeptic). The formatter split and placement read the same function | One owner, so analyzer and formatter cannot disagree | ok |
| RH5418 empty-union decision | `EmptyTypeDeclarationSemicolonAnalysisUtilities.ShouldReport` / `CanConvertSafely` with `UnionDeclaration`, `CSharp15` | Formatter `EmptyTypeDeclarationSemicolonTransform` (FMT-STRUCT-017/018) | Agree, except the pre-existing conditional-directive cell (SHARED-DIAG-008, lead decision 14, follow-up) | ok |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Implicit attribute target of a declaration | `rg -n "AttributeTargets.Struct" src`: `Reihitsu.Core/AttributeTargetUtilities.TryResolveImplicitTarget` is the only resolver. RH5507/RH5508 analyzers and fixes only name their target. Consumers: `TargetAttributePlacementAnalyzerBase`/`TargetAttributeListShapeAnalyzerBase` (registered on `AttributeList`), their fix bases, `AttributeListShapeUtilities`, `AttributeTargetFormattingShared`, `AttributeTargetFormattingRewriter` (accessor-only type checks) | One owner, ok |
| Parameter-list owners for RH5105/RH5106 | `Reihitsu.Analyzer/Core/ParameterListParentPolicy` (only consumers RH5105, RH5106); the formatter's collapse is keyed per visitor and documented against the policy | ok |
| Which ancestors are brace scopes | `SyntaxIndentationUtilities.IsIndentingAncestor` (shared) and the private mirror in `TargetAttributePlacementCodeFixProviderBase.HasUnusableEnclosingScope`, whose own summary calls it a mirror. The mirror crashed on a braceless type, which the shared helper handles. The diff restores parity for that case | Two owners, pre-existing; see *Architecture findings* |
| Type-declaration kind lists | `rg -l "SyntaxKind.RecordStructDeclaration\|SyntaxKind.StructDeclaration\|SyntaxKind.ClassDeclaration"` over the analyzers, the fixes, Core, and the formatter. Every list of a rule the change doc names now carries the union. The lists without it are each a recorded exclusion: RH2101–RH2104, RH2106 (own nested rules / non-goals), RH5412–RH5416 (own empty rules), RH4002/RH4003/RH4011, RH8001–RH8004, RH8009/RH8010 (naming and documentation, sibling issue), RH2103 (non-goal) | Duplicated lists are the known architecture finding (lead decision 8, follow-up). No new copy |
| Empty-type decision | `Reihitsu.Core/EmptyTypeDeclarationSemicolonAnalysisUtilities`, shared by RH5418 and the formatter | ok |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `RH5507…UnionAttributeListOnDeclarationLineIsReportedAndMoved` | A union's list on the declaration line is reported and moved, top level and nested, with `type:` | The NRE (the list follows `using System;`), a missing diagnostic, or another layout | `Verify` (batch base, Fix All), LF+CRLF | Strong for the union. The other members of the same code path are untested (#1) |
| `RH5508…UnionAttributeListWithSeveralAttributesIsReportedAndSplit`, `…WithCommentInsideIsReportedWithoutFix` | A union list is split; with a comment inside, it is reported without a fix | An unsplit list, or a fix offered across a comment | `Verify`, LF+CRLF | ok |
| `UnionDeclarationStructuralTests.UnionAttributeList*`, `AttributeListStructuralTransformTests` "Union" row | FMT-STRUCT-028 for unions: split, `type:` kept, nested indentation, the comment case unchanged | One list kept, or a moved comment | `AssertRuleResult` (two passes, LF+CRLF), phase-level | ok |
| `RH5418…EmptyUnionsAreReportedAndFixedAsSpecified` and the 11 sibling tests | The spec example: `A`, `B`, `F` reported; `B` keeps its braces with no fix; FixedState keeps `B`'s diagnostic | A diagnostic on `C`/`D`/`E`, a fix on `B`, or another fix text | `Verify` + FixedState diagnostics, batch base with `GetFixAllScenario`, LF+CRLF | ok. The conditional-directive cells are left untested on purpose (decision 14) |
| `RH5418…FormatterTests.VerifyFormatterFixesEmptyUnion`, `RH5508…FormatterTests.VerifyFormatterSplitsUnionAttributeList` | The formatter output is analyzer-clean and stable (FC-013) | A diagnostic on the formatter output, or a second pass that changes it | New `VerifyFormatter(…, onConfigure, …)`: analyzer before, after one pass, and after a second pass, LF+CRLF. The overload routes the old overloads with an empty configuration, so existing callers are unchanged | ok |
| `RH2110…` (5 tests) | Nested in every type kind and at depth 2, both partial parts, not at top level or in a namespace, a nested struct not reported | Missing or extra diagnostics | `Verify` | ok |
| RH2004, RH5105, RH5106, RH5108, RH5110, RH5404, RH7104 union fix tests | Spec examples with exact fix output, LF+CRLF; framework Fix All over several diagnostics | Another fix text, or a fix that does not converge | `Verify` (batch bases) | ok; my corpus agrees with them |
| RH7102, RH7103, RH7105, RH7106, RH7109, RH7110, RH7305/RH7305A, RH7308, RH7309, RH7311, RH7401, RH7405–RH7412 union tests | A union is ordered and grouped as a struct is; the not-reported side (nested, bodiless, partial parts) has its own tests (FC-003) | A diagnostic missing on the union | `Verify` | ok. RH7402–RH7404 are unreachable: a union has no override property, event, or indexer (07-test-implementer) |
| `RH5404…VerifyFixLaysOutAUnionInsideTheFixedType` (changed: 2 diagnostics) | The nested one-line union is now reported too; the fixed text is unchanged | Count 1 | `Verify` | ok; Q1 A covers it |
| `RH2104…NestedUnionIsNotReportedAsNestedStruct`, `RH5413…EmptyUnionIsNotReportedAsEmptyStruct` | The struct rules do not see unions (characterization) | A diagnostic | `Verify` | ok |

**Visible output (check 10).** Q1 A covers every changed expected value of a union diagnostic or fix (unions join the type-kind rules as structs), and so do Q2 A (RH5507/RH5508, the split under the unchanged FMT-STRUCT-028), Q3 A (RH5418), and Q4 A (RH2110). The `with(…)` tests and the comment-in-list formatter test pin output that is unchanged from `main`. All the rule-page changes are covered too: RH5105, RH5108, RH5404, RH5507, RH5508, and the new RH2110 and RH5418. No undecided output.

## Failure classes dismissed

- **FC-001, FC-002:** the new guard (#1) and the widened predicates are traced under *Guard scope*. Each inspects exactly the node its decision concerns.
- **FC-003:** every widened condition has a not-reported test (bodiless union, nested region, partial parts, comment and directive gaps).
- **FC-004, FC-005, FC-019, FC-020, FC-021:**
  - No rewrite is new. The fixes reach unions through unchanged bases that already guard comments and directives.
  - The corpus covered a comment and `#if` before `where`, a comment between the name and `(` (RH5105 silent), a doc comment and a leading block comment before the union (RH2004 inserts behind them), and `#if` in an empty body (no fix).
- **FC-006, FC-024:** these compile in shape, and the corpus agrees with the tests:
  - RH2004 inserts `internal` at top level, `private` nested, and `public` in an interface;
  - RH7104 writes `private partial union` on both nested parts;
  - RH7105 gives `internal protected readonly union`, and RH7106 gives `protected internal`.
- **FC-022, FC-023, FC-025, FC-029, FC-036, FC-018:** no literal, identifier, expression-body, key/value, or rename decision changes.
- **FC-026:** no token re-resolution is added.
- **FC-027:** unions nested in class, struct, record, record struct, interface, union, and static class are tested for RH2110, RH5418, RH2004, and RH5404. A nested semicolon union is in the RH5507 test.
- **FC-028, FC-041:** no formatter entry point changes.
- **FC-007, FC-008:**
  - The fixes converged in the corpus within 1–4 iterations.
  - Fix All runs in the batch-base tests and in RH5418's `GetFixAllScenario`.
  - RH5418 and RH5404 both report `union V(int, string) { }`, and both converge on `union V(int, string);`.
- **FC-009, FC-043:** RH5418 asks `FindUnsupportedSyntax` about the union it formats. That is the unchanged empty-type fix base, covered by `UnionWithUnsupportedCommentInCaseTypeListIsReportedWithoutFix`.
- **FC-030:** the RH5110 fix formats the whole union, so an expression-bodied member becomes a block body. RH5110-003 requires exactly that ("format the declaration that owns the reported clause"), and the class and struct twins behave the same.
- **FC-010, FC-011, FC-012:** no phase code changes. Formatting the corpus twice was idempotent.
- **FC-013:** formatter output is clean under every widened layout rule (*Scope*).
- **FC-014:** FMT-STRUCT-017/018 own the empty-union body, and FMT-STRUCT-028 owns the attribute split.
- **FC-015:** see *Policy ownership*.
- **FC-016:** this is finding #1.
- **FC-017:** see the table above. LF and CRLF are covered, and the formatter tests run two passes.
- **FC-031:** the changed summaries were checked (`TargetAttributePlacementCodeFixProviderBase`, the RH5108/RH5110 union actions, the removed RH5404 comment, and the RH5404 canary doc). Only the rule page RH5106 drifts (#2).
- **FC-032:** the implementer's full run comes after the last compiled change (`0bb9339`). Only docs and handovers changed after it.
- **FC-033, FC-034, FC-035:** expected values come from spec examples and struct siblings, not from patched code. The union fixtures are the reported shape.
- **FC-037:** no comment-only claim is made.
- **FC-038–FC-042:** the run declares no shape and changes no precheck.

## Architecture findings

**Two owners of the brace-scope set** (pre-existing; the run exposes it).

- **The owners.** `TargetAttributePlacementCodeFixProviderBase.HasUnusableEnclosingScope` keeps a private mirror of `SyntaxIndentationUtilities.IsIndentingAncestor`'s brace kinds, and its summary says so.
- **The input that shows them disagreeing.** For a type declared with a semicolon, the shared helper answers "no scope": `IsBetweenBraces` on default tokens is false. The mirror dereferenced the default brace's null `SyntaxTree` and threw. Every semicolon-declared class, struct, record, record struct, interface, and union whose attribute list does not start the file hit it (#1).
- **The run's narrow fix.** It restores parity for that case, so no further divergence is known.
- **The structural change.** Let the fix base ask `SyntaxIndentationUtilities` (it has a public `IsIndentingScope`) instead of mirroring the switch. That is a follow-up candidate for the lead; it does not block the run.

The duplicated type-kind lists (lead decision 8) stand as recorded. The diff adds no new copy.

## Hints

- `Release.ruleset` has no entry for RH5417, although RH5416 and RH5418 are listed as `Error`. This is pre-existing and unrelated to the run, and may be intentional; I did not check why.
- I did not run the full suite. I trusted the implementer's full run and the cleaner's warning-free Release build, as the role directs, and no finding turns on a test result.
