Role: spec author
Commit: b5fcff7
Verdict: spec written

No change doc: every requirement writes down current behavior. No production code, test, or rule page changed; the rule pages RH5409–RH5417 match the specs and needed no correction. The prompt carried no intended fix or verdict.

## Changed requirements

New shared spec `sdd/specs/shared/trailing-commas.md` (code owners `TrailingCommaAnalyzerBase<TNode>`, `TrailingCommaRemovalCodeFixProviderBase`, `TrailingCommaCodeFixHelper`; uses FMT-STRUCT-019, FMT-STRUCT-020):

- SHARED-TRAIL-001 — A comma after the last element is reported (location: the comma) — added
- SHARED-TRAIL-002 — A comma in front of a conditional directive is not reported (`#region`/`#pragma` do not block) — added
- SHARED-TRAIL-003 — The fix removes the comma (comma-only line removed with it) — added

New shared spec `sdd/specs/shared/empty-type-declarations.md` (code owners `EmptyTypeDeclarationShouldUseSemicolonAnalyzerBase`, `EmptyTypeDeclarationShouldUseSemicolonCodeFixProviderBase`, `EmptyTypeDeclarationSemicolonAnalysisUtilities`; uses FMT-STRUCT-017, FMT-STRUCT-018). `EmptyTypeDeclarationSemicolonAnalysisUtilities` is owned by no other spec (FMT-STRUCT-018 only names it in its Why, as FMT-STRUCT-007 does for `ExpressionBodyRewriteUtilities`, which RH3202 owns):

- SHARED-EMPTY-001 — A brace body without members is reported (only whitespace, line breaks, comments in it; ends with its closing brace; location: the name) — added
- SHARED-EMPTY-002 — A declaration with members or a semicolon is not reported — added
- SHARED-EMPTY-003 — The fix writes the semicolon declaration (FMT-STRUCT-017; a comment behind `}` follows `;`) — added
- SHARED-EMPTY-004 — A comment in the body withholds the fix (FMT-STRUCT-018) — added

Rule specs (all: Layout, Warning, enabled, Fix All, analyzer + provider as owners):

- RH5409-001 — A comma after the last enum member is reported — added
- RH5410-001 — A comma after the last array element is reported (field `= {}`, `new int[]`, `new[]`, `stackalloc int[]`, `stackalloc[]`, each nested initializer of a multi-dimensional array) — added
- RH5410-002 — Other initializers are not reported — added
- RH5411-001 — A comma after the last collection element is reported (`new List<int>{}`, `new(){}`, nested `L = {}`, dictionary complex elements) — added
- RH5411-002 — Other initializers are not reported (index initializer `[1] = 2,` included) — added
- RH5412-001 — An empty class is reported from C# 12 on — added
- RH5413-001 — An empty struct is reported from C# 12 on (not record structs) — added
- RH5414-001 — An empty interface is reported from C# 12 on — added
- RH5415-001 — An empty record is reported from C# 9 on (`record`, `record class`; not record structs) — added
- RH5416-001 — An empty record struct is reported from C# 10 on (not record classes) — added
- RH5417-001 — A function or accessor body on one line is reported (location: `{`) — added
- RH5417-002 — Other blocks and bodies are not reported — added
- RH5417-003 — The fix puts the braces of a member or accessor body on their own lines (FMT-BREAK-002/003/006/025, FMT-PIPE-011) — added
- RH5417-004 — No fix for local functions, lambdas, and anonymous methods — added

All 21 IDs are in `sdd/specs/requirement-ids.txt` (21 insertions, no removals, `LC_ALL=C sort -c` clean); the 11 specs are in `sdd/specs/README.md`. `scripts/test.sh --project architecture` filtered to `SpecConsistencyTests|SpecStructureTests|RequirementCoverageTests`: 38 passed, 1 failed — `EveryRequirementOfAnEnforcedSpecIsCovered`, listing exactly the 21 new IDs and nothing else.

How the examples were established: every example block was run through `scripts/apply-fix.sh` (LF and CRLF, `LanguageVersion.Latest`) as one fixture per block; the output matched the spec's result blocks, the negative blocks gave `no-diagnostic`, and SHARED-EMPTY-004 and RH5417-004 gave `no-fix-offered`. The version clauses rest on existing tests (C# 11 for RH5412–RH5414) and, for RH5415 (C# 8) and RH5416 (C# 9), on code reading only (`EmptyTypeDeclarationSemicolonAnalysisUtilities.ShouldReport` compares the parse options' version). Below those versions a record or record struct does not compile, so a test there has to expect the compiler's error too.

Deliberately left unspecified (each belongs to an open question below): what the fixes do to whitespace on the comma's line, to code outside the reported body, and whether a body holding a directive is reported.

## Examples for the tests

- SHARED-TRAIL-001, SHARED-TRAIL-002, SHARED-TRAIL-003 (examples use RH5409); the rule-level tests of RH5410/RH5411 cover these too. SHARED-TRAIL-002 states `#pragma` as well as `#region` as not blocking; only `#region` has a test today (RH5410).
- RH5409-001, RH5410-001, RH5410-002, RH5411-001, RH5411-002.
- SHARED-EMPTY-001 to SHARED-EMPTY-004 (examples use RH5412), and RH5412-001 to RH5416-001. RH5415 and RH5416 have no version test today.
- RH5417-001 to RH5417-004. Operator and conversion-operator fixes have no fix test today (only `…IsDetected`); RH5417-003's example is verified.

Tests whose expected output pins a case the spec leaves out (characterization tests — do not tag them with the requirement whose question is open):

- `RH5412EmptyClassesShouldUseSemicolonDeclarationsAnalyzerTests.VerifyBlankLineAboveCommentOfFixedClassInFileScopedNamespaceIsInserted` — pins the fix inserting a blank line outside the declaration (question 2).
- `RH5416EmptyRecordStructsShouldUseSemicolonDeclarationsAnalyzerTests.VerifyDirectiveContainingRecordStructIsReportedWithoutCodeFix` — pins that a body with `#if`/`#endif` is reported (question 4).

## Open questions to file

### 1. Trailing-comma fixes leave whitespace on the comma's line — Decided by principle P10 (#912)

Issue: new `SDD open questions: sdd/specs/shared/trailing-commas.md`. Requirements: SHARED-TRAIL-003; violates SHARED-FIX-007 (being added by #1138).

The fix removes the comma's characters only, so the whitespace around it stays. Formatting the result then changes the line the fix edited. Input → actual fix output (RH5409; the same helper serves RH5410 and RH5411):

- `    B ,` → `    B ` (trailing space).
- `    /* c */ ,` on its own line → `    /* c */ ` (trailing space).
- `    , // c` on its own line → `     // c` (comment indented one column too far).
- `    , }` → `     }` (brace indented one column too far).

Formatting each result removes the trailing space or re-indents the line. Why it looks wrong: P10, under which formatting the code a fix changed changes nothing.

### 2. Empty-type fixes reformat the whole declaration — Decided by principle P9 (#912)

Issue: new `SDD open questions: sdd/specs/shared/empty-type-declarations.md`. Requirements: SHARED-EMPTY-003, FMT-PIPE-010; violates SHARED-FIX-006 (being added by #1138). Related: #1140 and #969, the same pattern in other fixes.

`EmptyTypeDeclarationShouldUseSemicolonCodeFixProviderBase` formats the whole type declaration, so code outside the empty body changes too. Input → actual (RH5412 unless noted):

- `class C<T> : B where T : new()` / `{` / `}` → `class C<T> : B` / `    where T : new();` (constraint moved to its own line). The same happens for RH5414 with `interface I<T> : System.IDisposable where T : class`.
- `[Serializable][Obsolete]` / `class C` / `{` / `}` → the two attribute lists split onto two lines.
- `    internal  sealed  class  C` (double spaces, in a block namespace) → `    internal sealed class C;`.
- `///<summary>x</summary>` above an empty class in a block namespace → the comment rewritten to three lines (`/// <summary>` / `/// x` / `/// </summary>`).
- `namespace N;` / `// remark` / `class C` / `{` / `}` → a blank line inserted between `namespace N;` and `// remark`. This is pinned by the test named under "Examples for the tests".

Why it looks wrong: P9, under which a fix changes only the code its diagnostic is about. Here that code is the empty body.

### 3. `class C { };` is neither reported nor converted — Decided by principle P11 (#912)

Issue: `SDD open questions: sdd/specs/shared/empty-type-declarations.md`. Requirements: SHARED-EMPTY-001; also FMT-STRUCT-017.

Input:

```csharp
class C
{
};
```

Actual: RH5412 reports nothing, and `scripts/format.sh` leaves the code unchanged. The same holds for every kind RH5412–RH5416 check: the shared predicate treats any semicolon token after `}` as "already a semicolon declaration".

Why it looks wrong: this is an empty class with a brace body, the construct RH5412 names. The literal wording of FMT-STRUCT-017 ("MUST replace the empty brace body of a class …") covers it too, so the formatter part is a difference from an existing spec. SHARED-EMPTY-001 avoids the case: it reports a declaration "that ends with its closing brace". Expected under P11: reported, and fixed to `class C;`.

### 4. An empty-looking body that holds a conditional directive is reported but cannot be fixed — open, no principle decides it

Issue: `SDD open questions: sdd/specs/shared/empty-type-declarations.md`. Requirements: SHARED-EMPTY-001, SHARED-EMPTY-004; compare SHARED-TRAIL-002.

Input:

```csharp
class C
{
#if NEVER
    int _x;
#endif
}
```

Actual: RH5412 reports `C`, and no code fix is offered (`no-fix-offered`). The formatter keeps the braces (FMT-STRUCT-018).

Why it looks wrong: in a configuration that defines `NEVER`, `C` has a member and needs its braces. No edit satisfies the rule without deleting that member, so the warning can only be suppressed. The trailing-comma rules skip the analogous case on purpose: SHARED-TRAIL-002 and the "Exceptions" sections of RH5409–RH5411.

Competing readings:

- (a) Keep reporting; a diagnostic without a fix is a supported end state.
- (b) Do not report when a conditional directive or excluded code lies inside the braces, as SHARED-TRAIL-002 does. The RH5416 test with an empty `#if DEBUG`/`#endif` pair would then expect no diagnostic.

Under either reading, a `#region`/`#endregion` pair inside the braces stays reported without a fix, as today.

Recommendation: (b), for consistency with SHARED-TRAIL-002. The spec leaves every directive-holding body out of SHARED-EMPTY-001, so neither answer contradicts it.

### 5. RH5417 fix reformats the whole member — Decided by principle P9 (#912)

Issue: new `SDD open questions: sdd/specs/analyzer/RH5417.md`. Requirements: RH5417-003, FMT-PIPE-010; violates SHARED-FIX-006 (being added by #1138). Related: #1140.

The provider formats the whole member, the whole property for an accessor. Input → actual:

- `void  M( int a ) { }` → `void M(int a)` / `{` / `}` (signature spacing changed).
- `int P { get { return _x; } set  =>  _x = value; }` → the `set` accessor becomes `set => _x = value;` (its spacing changed although it is not reported).
- `void M(bool a) { if (a) M(a); }` → the `if` body gets braces (FMT-STRUCT-001; RH5405's concern).
- `///<summary>x</summary>` above `void M() { }` in a block namespace → the comment rewritten to three lines.
- `[Obsolete][CLSCompliant(false)]` above `void M() { }` → the attribute lists split onto two lines.
- `C() : base(1) { }` → `C()` / `    : base(1)` / `{` / `}` (the initializer moved to its own line).

Why it looks wrong: P9.

### For the lead's comment on #1150 (not a new question)

These fixes delegate to the formatter and are offered as no-ops when the formatter refuses its input:

- `EmptyTypeDeclarationShouldUseSemicolonCodeFixProviderBase` (RH5412–RH5416);
- `RH5417FunctionAndAccessorBodyBracesMustNotShareLineCodeFixProvider`.

Verified with `scripts/apply-fix.sh`: a file starting with `// <AUTO-GENERATED>` above `class C` / `{` / `}` (RH5412), and above `class C { void M() { } }` (RH5417), each gave `no-progress (1 iteration)`. The syntax-error case follows from FMT-PIPE-005 by code reading; the runner refuses unparseable fixtures. The trailing-comma fixes (RH5409–RH5411) edit text directly and are not affected.

## Decisions needed

_None._
