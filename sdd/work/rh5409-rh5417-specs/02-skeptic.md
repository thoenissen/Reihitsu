Role: skeptic
Commit: 2165263
Verdict: proceed narrower

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Premise

The run is real: thoenissen/Reihitsu#992 asks for specs of existing behavior, and every rule RH5409–RH5417 exists with a provider. I checked the spec diff against the code that decides it:

- `TrailingCommaAnalyzerBase<TNode>`: reports the last separator when it is a comma, unless `SyntaxTriviaUtilities.ContainsConditionalCompilationBoundary` finds `#if`/`#elif`/`#else`/`#endif` or `DisabledTextTrivia` between the comma's end and the closing brace. `TrailingCommaCodeFixHelper.RemoveTrailingComma`: removes the whole line, line break included, when the trimmed line is `,`; otherwise only the comma's span.
- `EmptyTypeDeclarationSemicolonAnalysisUtilities.ShouldReport`: right kind, no semicolon token, neither brace missing, `Members.Count == 0`, version at least the minimum. `CanConvertSafely` adds `HasMeaningfulBodyTrivia`: any non-whitespace, non-EOL trivia after the header's last token, around `{`, or before `}`. The provider registers only when `CanConvertSafely` holds and calls `FormatNodeInDocumentAsync` on the type.
- RH5417 analyzer: `Block` whose parent is one of the 10 types RH5417-001 names, with `{` and `}` on the same line. Fix: `GetFormattingMember` returns the parent `MemberDeclarationSyntax` (method, constructor, finalizer, operator, conversion operator) or the accessor's enclosing member, and nothing otherwise.
- Formatter counterparts: `EnumTrailingCommaRemovalTransform` and `InitializerTrailingCommaRemovalTransform` use the same guard over the same gap. `EmptyTypeDeclarationSemicolonTransform` uses `CanConvertSafely`. `CodeFixDisabledStructuralTransforms` disables only `AccessorExpressionBody`, so the empty-type transform runs inside the fix.

The following hold as written: SHARED-TRAIL-001..003, RH5409-001, RH5410-001/002, RH5411-001/002, SHARED-EMPTY-001..004, RH5412-001 to RH5416-001, RH5417-001, RH5417-002, and RH5417-004. **RH5417-003 claims more than the fix does (F1).** Four further findings (F2–F5) are below.

All probes ran with `scripts/apply-fix.sh <ID> <dir>` (LF and CRLF agreed in every case) or `scripts/format.sh <file>` run twice. The fixtures were in the scratchpad and are deleted.

### F1 — RH5417-003 is falsified for a member that shares its line

RH5417-003 says the fix puts "the closing brace on lines of their own", and its Why says "formatting it changes nothing". Actual output of `apply-fix.sh RH5417`:

- `class C` / `{` / `    void M() { } int _x;` / `}` → `    void M()` / `    {` / `    } int _x;`
- `class C` / `{` / `    void M() { } void N() { }` / `}` → `    void M()` / `    {` / `    } void N()` / `      {` / `      }` (2 applications, one per diagnostic). `format.sh` on the same input gives `void N()` on its own line at column 4.
- `class C { void M() { } }` → `class C { void M()` / `          {` / `          } }`

In each case every brace *starts* its line, which is the FMT-BREAK-002 wording, and RH5417 does not report the result again. But the closing brace does not have its line to itself. The cause is FMT-PIPE-010: the target keeps its trailing trivia, and keeps its leading trivia when it does not start its line.

Also, `void M() { Foo(); ; }` → `void M()` / `{` / `Foo();;` / `}`. "Each statement … on lines of their own" is not literal here because of the empty-statement exception in FMT-BREAK-006.

Other RH5417 probes matched the spec:

- `void M() /* c */ { }` and `get /* g */ { … }`: the comment stays and the brace moves (FMT-BREAK-003, second sentence).
- `void M() { } // note`: the comment stays behind `}`.
- A `get { … }` followed by a `set` inside `#if X`: only `get` is expanded.
- A default interface method is fixed.
- A top-level `void L() { }` gets no fix.
- The method fix also expands a one-line lambda inside the method.

### F2 — The delegating fixes rewrite `#region` text that their diagnostic does not report (SHARED-FIX-003)

- RH5417, in a multi-line property: `#region getters` / `get { return _x; }` / `#endregion` → `#region Getters`, a blank line, the expanded `get`, a blank line, `#endregion // Getters`.
- RH5417, when only `get { return _x; }` is reported and the unreported multi-line `set` body holds `#region store` / `_x = value;` / `#endregion`: the result has `#region Store` … `#endregion // Store`.
- RH5412: `[Obsolete]` / `#region legacy` / `#endregion` / `class A` / `{` / `}` → `[Obsolete]`, blank line, `#region Legacy`, blank line, `#endregion // Legacy`, blank line, `class A;`.

A region in the target's own leading trivia is not renamed: `#region helpers` above `void M() { }`, and `#region types` above an empty `class A`, both stay unchanged. A region inside a base list withholds the fix, because `#endregion` lands before `{`.

SHARED-FIX-003 allows a fix to change directive text only where its own diagnostic reports it. `squad/decisions/2026-10-18-shared-fix-003-skeptic-scope.md` (answer 2) classifies the same pattern in RH7103 as a bug. The specs do not state this behavior, which is correct. It is a decided case (P1) and has not been filed yet.

### F3 — Bodies holding a non-conditional directive are left out, yet Q4 of the spec handover presumes their answer

`class A` / `{` / `    #region R` / `    #endregion` / `}` and `class A` / `{` / `#pragma warning disable CS0001` / `}` are both reported, and both give `no-fix-offered`. SHARED-EMPTY-001 ("nothing but whitespace, line breaks, and comments") does not cover them; SHARED-EMPTY-004 covers withholding the fix. Q4 states that "under either reading" a `#region` body stays reported, but that is the maintainer's call. A reading (c), "no report when any directive is inside the braces", produces different output. Either specify the non-conditional case as SHARED-TRAIL-002 does for commas, or make it part of Q4.

### F4 — SHARED-EMPTY-004 restates FMT-STRUCT-018

After referencing FMT-STRUCT-018, SHARED-EMPTY-004 repeats its condition word for word ("anything other than whitespace and line breaks after the declaration's header, before or after the opening brace, or before the closing brace"). `sdd/README.md` says: "A rule that a shared policy already states is referenced by its ID, never restated." SHARED-TRAIL-002 is different: it is the analyzer's own decision and extends FMT-STRUCT-019 (`#region`/`#pragma` do not block, excluded code does), so it stands.

### F5 — Minor

- The two Whys contradict each other. RH5413-001 says "A record struct is a record, which RH5416 checks"; RH5415-001 says "A record struct is a struct, which RH5416 checks".
- `EmptyTypeDeclarationSemicolonAnalysisUtilities` is claimed by SHARED-EMPTY, while FMT-STRUCT-018's Why names `CanConvertSafely` as its own decision. This is mechanically allowed: FMT-STRUCT does not list the type, and the same precedent exists for RH3202/`ExpressionBodyRewriteUtilities`. It is a judgment call for the spec checker.

### Checked and consistent

- **FC-013, trailing commas:** the analyzer and the formatter share one guard over one gap, so the formatter removes exactly what is reported.
- **FC-013, RH5417:** I ran `format.sh` twice on a fixture holding every reported body kind: a field-initializer lambda, an anonymous method with and without parameters, lambdas in arguments, in an array initializer, a tuple, and an anonymous object, an async lambda, local functions (also `static`), accessors, event accessors, an indexer, an operator, a conversion operator, a constructor, and a finalizer. No one-line body remained, and the second pass changed nothing.
- **FC-013, empty types:** a body with a comment is reported and kept by the formatter. That is the same decision as withholding the fix (FMT-STRUCT-018), and it is a supported end state because the comment can be moved by hand. `class C { };` is neither reported nor converted (Q3).
- **FC-014:** every concern has an owning formatter requirement (FMT-STRUCT-017..020, FMT-BREAK-002/025).
- **#1150:** confirmed in `ReihitsuFormatter.FormatNodeInDocumentAsync`, which returns its input on `HasSyntaxErrors` or `IsAutoGeneratedSource`. #1150 already names the RH54xx providers, so the lead's comment only confirms the instance. The trailing-comma fixes edit text directly and are not affected.
- **Trailing-comma probes:**
  - `enum F` with `A,` / `B,` / two `#pragma warning` lines → reported; the fix gives `B` and keeps both pragmas.
  - `A` / `#if X` / `#endif` / `    ,` / `}` → reported; the comma line is removed. The directive lies before the comma, outside the gap.
  - `A,` / `B` / `    ,` / `#region R` / `#endregion` / `}` → reported; the comma line is removed.
  - An array initializer ending in `1,` / `#if false` / `2,` / `#endif` → not reported.
  - `{ { 1, 2, } }` as a dictionary element is a parse error, so there is no gap there.

## Mechanism and candidates

_N/A_ — this run is not a bug fix. The dispatch the tests must cover:

- **RH5409:** `EnumDeclaration`.
- **RH5410:** `ArrayInitializerExpression`, reached from a field/local/property `= { }`, `ArrayCreationExpression`, `ImplicitArrayCreationExpression`, `StackAllocArrayCreationExpression`, `ImplicitStackAllocArrayCreationExpression`, and nested initializers of multi-dimensional arrays.
- **RH5411:** `CollectionInitializerExpression`, reached from `ObjectCreationExpression`, `ImplicitObjectCreationExpression`, and a member `L = { }`.
- **Not dispatched:** `ObjectInitializerExpression` (index initializers included), `WithInitializerExpression`, `ComplexElementInitializerExpression`, `CollectionExpression`, `AnonymousObjectCreationExpression`.
- **RH5412–RH5416:** `ClassDeclaration`, `StructDeclaration`, `InterfaceDeclaration`, `RecordDeclaration`, `RecordStructDeclaration`.
- **RH5417:** `Block` whose parent is one of `MethodDeclaration`, `ConstructorDeclaration`, `DestructorDeclaration`, `OperatorDeclaration`, `ConversionOperatorDeclaration`, `AccessorDeclaration`, `LocalFunctionStatement`, `SimpleLambdaExpression`, `ParenthesizedLambdaExpression`, `AnonymousMethodExpression`. A fix exists only for the first six.

## Breaking inputs

- **FC-002/FC-003, SHARED-TRAIL-002 on both sides of the boundary:** a conditional directive or excluded code after the comma (not reported) against `#region` and `#pragma` after the comma (reported). `#pragma` has no test today. Add a conditional directive *before* the comma (reported).
- **FC-005/FC-019/FC-020/FC-021, SHARED-EMPTY-004:** each of the four guarded positions (after the header, before `{`, after `{`, before `}`) withholds the fix. A comment *inside* the header (`class /* c */ A`) gets the fix, with the result `class /* c */ A;`. A comment behind `}` on a multi-line declaration also gets the fix: `class A` / `{` / `} // note` → `class A; // note`.
- **FC-006, version gates:** RH5412–RH5414 at C# 11 and C# 12, RH5415 at C# 8, RH5416 at C# 9. The test for each must expect the compiler errors.
- **FC-033, RH5415 at C# 8:** this needs to be established. Roslyn may not parse `record` as a type-declaration start below C# 9. If it does not, a C# 8 test passes without reaching the version gate, so the test should also assert that the tree contains a `RecordDeclarationSyntax`. I did not verify this.
- **FC-007/FC-008:** two accessor diagnostics in one property, where both fixes format the same member (existing: `VerifySingleLineBlockAccessorsAreFixedOntoSeparateLines`). For RH5410, an inner and an outer comma reported in one multi-dimensional initializer.
- **FC-010:** removing a comma-only line under CRLF.
- **FC-024:** `partial`, `static`, `file`, `readonly ref struct`, a primary constructor, a base list, and constraints on empty types.
- **FC-025/FC-027, RH5417-001/003:** every bodied member kind. A member that shares its line with a sibling or with the enclosing braces (F1) is a characterization test only and must not be tagged RH5417-003.
- **FC-004/FC-030:** region text inside the formatted member or type (F2). Do not pin it under a requirement tag. If it is pinned at all, it is a characterization test that names the filed question.
- **FC-009:** a no-op fix under a case-differing `<AUTO-GENERATED>` marker, or with a syntax error elsewhere in the file (#1150). Do not pin this as intended behavior.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Analyzers RH5409–RH5417 | Must stay |
| Code fixes RH5409–RH5417 and their bases | Must stay (lane); F1/F2 defects are filed, not fixed |
| Fix All (batch fixer) | Must stay |
| `Reihitsu.Core` (`EmptyTypeDeclarationSemicolonAnalysisUtilities`, `SyntaxTriviaUtilities`) | Must stay |
| Formatter (FMT-STRUCT-017..020, FMT-BREAK-002/003/006/025, FMT-PIPE-010/011) | Not involved |
| Specs RH5417, SHARED-EMPTY, RH5413/RH5415 | Must change: F1 wording, F3, F4, F5 |
| Rule pages RH5409–RH5417 | Must stay; the spec checker compares them |
| Tests | Must change: covering tests per the list above |

## Simpler option

For F1, state RH5417-003 with FMT-BREAK-002's own wording: the opening brace, each statement as FMT-BREAK-006 lays it out, and the closing brace each *start* their own line. Drop "formatting it changes nothing" from the Why. That claim holds for every probe above. The shapes where the member shares its line are left out and filed.

## Questions for the maintainer

- **RH5417, a member that shares its line (new; for the RH5417 open-questions issue).** Input: `class C` / `{` / `    void M() { } int _x;` / `}`. The two readings give different output:
  - (a) P10 decides it: the code the fix changed is laid out as the formatter lays it out, so the result is `    void M()` / `    {` / `    }` / `    int _x;`.
  - (b) P9 limits the fix to M, and the gap after `}` belongs to the next member, a concern RH5417 does not name (FMT-BREAK-007). The result stays `    } int _x;`, as today.

  The same applies to `void M() { } void N() { }` and to `class C { void M() { } }`. The lead decides whether P10 already settles this ("Decided by principle P10 (#912)") or files it as open.
- **F2 is not a question.** SHARED-FIX-003 decides it. File it "Decided by principle P1 (#912)", with SHARED-FIX-003 as the violated requirement, in the RH5417 issue and in the `shared/empty-type-declarations.md` issue, using the three inputs above.
- **F3 depends on Q4 of the spec handover.** Either Q4 gains reading (c) and asks about non-conditional directives too, or the spec author specifies them.
