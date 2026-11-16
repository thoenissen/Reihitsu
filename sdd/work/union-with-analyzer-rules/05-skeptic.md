Role: skeptic
Commit: 2a31a3e
Verdict: proceed

Scope: the spec diff of 2a31a3e only. That is the new specs RH5418 and RH2110, their rule pages and package README rows, the sibling non-goal edits, the removal of RH5404's non-goal, the `Why:` edit of FMT-STRUCT-017, and the change-doc additions. The premise check in `02-skeptic.md` still holds and is not repeated here. The prompt carried no intended fix, suspected cause, or hoped-for verdict.

The spec follows the maintainer's answers Q3 A and Q4 A (`00-lead.md`). Two defects in the diff must be fixed before tests are written: the RH2110-001 example does not compile, and the C# 14 clause of RH5418-001 cannot be tested. A third defect predates the run: the shared base that RH5418 will subclass ignores SHARED-DIAG-008. The lead must decide how to handle it; my recommendation is under *Internal choices* (1–3).

## Premise

**RH5418 and RH2110 are reachable.** At C# 15, both the CLI and a user project parse `union` as a `UnionDeclaration`. `02-skeptic.md` established this.

### How I checked

- **Scratch compile.** A file-based app references Microsoft.CodeAnalysis.CSharp 5.12.0 and compiles against the net11 trusted-platform assemblies. It parsed and compiled every new example at C# 14 and at C# 15, and printed each declaration's kind, braces, semicolon, member count, and errors.
- **Formatter.** `scripts/format.sh` ran twice on the RH5418 example and on edge inputs. The output was idempotent.
- **RH5412 and RH5413.** `scripts/apply-fix.sh` ran on fixtures with conditional directives in an empty body.
- **Self-hosting tests.** `scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.SelfHosting"` ran once.
- All fixtures are deleted.

### Results

**1. The RH2110-001 example does not compile.**
- In `interface I { union Value(int, string); }`, the name gives CS0542: "'Value': member names cannot be the same as their enclosing type".
- Cause: every union gets an implicit `Value` property from `IUnion`. Any union named `Value`, top-level or nested, gives CS0542.
- The rest of the example compiles: `Top`/`Inner`, `Choice`, `Result`, `Entry`, `Free`.
- The rule page (`Result`) compiles, and so do change-doc example 12 and the RH5418 example and page.
- The example needs another name, such as `Shape`. The test implementer must not use `Value`.

**2. Below C# 15, no union declaration exists.**
- At C# 14, `union A(int, string) { }` parses as a method or global statement, with CS1001 errors and others. Roslyn 5.12 builds a `UnionDeclarationSyntax` only from C# 15 on.
- So the RH5418-001 clause "Below C# 15 it MUST NOT report a union" can never be broken. "Under C# 14, no union is reported" can only be tested on code that has compiler errors.
- The `LanguageVersion.CSharp15` gate in the base never decides anything for a union. The formatter's union gate (FMT-STRUCT-017) does not decide anything either. See *Internal choices*.

**3. The RH5418 example matches the formatter.**
- Whole-document formatting of the RH5418-001 example gives `union A(int, string);` and `union F(long, string);`. It keeps `B` with its braces and comment.
- So the spec's "fix gives" for `A` and `F` is what the fix produces through `FormatMemberOnLinesOfItsOwnAsync`.
- Formatting also converts `struct E` and expression-bodied `D`. The RH5418 fix formats only the union it fixes, so the spec's fix output correctly leaves `D` and `E` alone (FC-030).

**4. A partial union part without a case-type list exists.**
- `partial union P(int, string) { }` with `partial union P { }` compiles.
- `partial union Q(int, string);` with `partial union Q;` compiles. The formatter turns the first pair into the second.
- So RH5418 reports a part that has no case-type list.
- The RH5418 non-goal says "a union without case types does not exist". That is true of the type: a non-partial `union NoCases { }` is CS9370. It is not true of a declaration. See *Internal choices*.

**5. A union with braces and a semicolon is not reported.**
- `union S1(int, string) { };` parses with braces and a `SemicolonToken`. `ShouldReport` returns false for it, as for `class C { };` under SHARED-EMPTY-001 ("ends with its closing brace").
- The formatter keeps `{` `}` `;`.
- This matches the siblings and is no conflict.

### Checked against siblings and shared specs

**RH5418:**
- Against RH5412–RH5416 and SHARED-EMPTY-001 to 004, it matches: same base, same location (the name), same fix base, Fix All, and SHARED-FIX-010 through `FindUnsupportedSyntax`.
- Against FMT-STRUCT-017 and 018, it matches. The fix output equals the formatter's output, and the formatter keeps the braces exactly where SHARED-EMPTY-004 withholds the fix.
- SHARED-DIAG-010 does not apply. "Semicolon versus braces" is a syntax form, not line breaks, blank lines, indentation, or spacing. The siblings already report the commented body the formatter keeps (SHARED-EMPTY-004), and RH5418 inherits that unchanged.
- FC-014 holds: FMT-STRUCT-017 owns the construct.

**RH2110:**
- It matches RH2104/RH2106 and SHARED-NESTED-001/002.
- `NestedTypeAnalyzerHelper.IsNestedType` is `Parent is TypeDeclarationSyntax`. It covers a union in a class, struct, interface, record, record struct, union, and static class. Each of these compiles; I checked them in the scratch compile.
- Nesting is not a formatter decision, so FC-014 does not apply.

**FMT-STRUCT-017 `Why:` edit.** It removes only a statement that is no longer true. The normative text is unchanged. No other spec or document still says that no rule reports a braced union. I checked with `rg`; the accepted `union-declaration-layout.md` is history.

**RH5404 non-goal removal.** Correct. RH5404-001 already names `union` (from 01). Both RH5404 and RH5418 report a one-line `union V(int, string) { }`, as `struct S { }` is reported by RH5404 and RH5413. Both fixes converge on `union V(int, string);`, as `02-skeptic.md` showed for RH5404.

**IDs.** RH2110 and RH5418 are free. Before this commit, `rg` found them nowhere in `src`, `Release.ruleset`, or `Reihitsu.sln`.

### Expected failures until implementation

The spec author listed the architecture Sdd checks. Two analyzer self-hosting tests also fail, and the implementer must turn them green:
- `PackageReadmeContainsNoExtraAnalyzerRows`: "rows for unknown analyzers: RH2110, RH5418".
- `RuleDocumentationExampleTests.EveryNonOptedOutDocumentPassesVerification`: `NoMatchingAnalyzer` for RH2110 and RH5418.

The other 14 tests pass. The doc-example verifier only parses at `LanguageVersion.Latest` and then runs the analyzer. Neither page needs `UnionSupport`.

## Mechanism and candidates

This is a new rule, not a bug.

**RH5418 candidates** (`EmptyTypeDeclarationShouldUseSemicolonAnalyzerBase`, registered on `SyntaxKind.UnionDeclaration`):
- `UnionDeclarationSyntax` with a body or with a semicolon;
- top level, in a namespace, or nested in any `TypeDeclarationSyntax`;
- `partial` parts with or without a case-type list;
- generic, with constraint clauses.

**RH2110 candidates** (`NestedTypeAnalyzerHelper`): a `UnionDeclarationSyntax` whose parent is any `TypeDeclarationSyntax`.
- An `ExtensionBlockDeclarationSyntax` parent is also a `TypeDeclarationSyntax`. A union there is CS9282, which no user code compiles with, so it is dropped (*Supported inputs*).

## Breaking inputs

### Pre-existing defect that RH5418 inherits

The class is SHARED-DIAG-008, FC-020. **Mechanism:** `EmptyTypeDeclarationSemicolonAnalysisUtilities.ShouldReport` judges the body by `Members.Count` and never looks at a conditional directive in the body's trivia. So every rule on that base reports an empty-looking body that holds `#if`/`#endif` or disabled text. SHARED-DIAG-008's own example says exactly this case is "not reported" for RH5412: `class A { #if DEBUG #endif }`.

Class matrix. Axis 1 is the rule; axis 2 is the directive in the body. "No fix" means the rule reports the body and offers no code fix.

| Directive in the body | RH5412 class | RH5413 struct | RH5416 record struct | RH5414, RH5415 | RH5418 union |
|---|---|---|---|---|---|
| `#if DEBUG` / `#endif`, empty | **reported**, no fix (apply-fix) | **reported**, no fix (apply-fix) | **reported**, no fix: the existing test `VerifyDirectiveContainingRecordStructIsReportedWithoutCodeFix` asserts it | same base, same code path (not run) | will be **reported** |
| `#if DEBUG` / `#else` / `#endif` | **reported**, no fix (apply-fix) | same path | same path | same | will be **reported** |
| `#if false` / `#endif` | **reported**, no fix (apply-fix) | same path | same path | same | will be **reported** |
| `#if FOO` with a member, then `#endif` (disabled member) | **reported**, no fix (apply-fix) | same path | same path | same | will be **reported** |
| `#region` / `#endregion`, or `#pragma` | reported, no fix: correct under SHARED-DIAG-008, existing RH5412 tests | same | same | same | reported, no fix: correct |

Every conditional cell fails. In every case the fix is withheld, because SHARED-EMPTY-004 / FMT-STRUCT-018 see the directive. So the user code is safe, and the only wrong output is the diagnostic. The formatter keeps all of these bodies, which I checked for the union. See *Internal choices* for how to handle it. Whatever the lead decides, the RH5418 tests must not copy RH5416's `VerifyDirectiveContainingRecordStructIsReportedWithoutCodeFix`, because that test asserts the wrong reading.

### Failure classes for the new rules

**FC-006 / FC-024**:
- `partial union P(int, string) { }` with `partial union P { }`: two diagnostics, and Fix All gives `partial union P(int, string);` and `partial union P;`, which compiles.
- A `readonly` union, as `B` in the example.
- A `file union FileU(int, string) { }` (top-level only).
- A generic union with `where`: the fix output keeps the clause on its own line, as in FMT-STRUCT-017's example (`union V<T>(T, string)⏎    where T : class;`).

**FC-019 / FC-020 / FC-021, comments and directives in the gaps** (SHARED-EMPTY-001, SHARED-EMPTY-004):
- comment after the header;
- comment before `{`;
- comment behind `{`;
- comment before `}`;
- a comment behind `}` on its line, which follows the semicolon (SHARED-EMPTY-003);
- `#region` and `#pragma` in the body: reported, no fix.

**FC-043 / SHARED-FIX-010.** A comment between a case type and a comma on a later line, such as `union U(int // c⏎    , string)⏎{⏎}`, is FMT-UNSUP-009, which applies to any comma. RH5418 reports it, and no fix is offered. A comment elsewhere in the case-type list, such as `union U(int /* a */, string)⏎{⏎}`, is reported and fixed, and the comment stays.

**FC-030 / FC-032.** After the Q2 change in `AttributeTargetUtilities`, the RH5418 fix on `[A, B] union U(int, string)⏎{⏎}` also splits the list, because it formats the whole union node. Pin that output. Today, before the Q2 change, the formatter keeps `[System.Serializable, System.Obsolete]` on a union as one list.

**FC-027, enclosing scopes:**
- RH5418: a union nested in a class (`F`), struct, record, record struct, interface, and union; the one-line `{ }` and the two-line `{⏎}` forms.
- RH2110: nested in a class, struct, interface, record, record struct, union, and static class, and at any depth (a union in a class in a class). Not reported: file level, a block namespace, a file-scoped namespace.
- RH2110 with SHARED-NESTED-002: the nested `partial union P(int, string);` with `partial union P { public int M() => 1; }` in a class compiles and gives two diagnostics. Two parts that both carry the case-type list are CS8863.

**FC-013:** formatter output for every RH5418 input is either `union …;` (not reported) or a body with a comment or directive (reported, no fix, as the siblings). No new disagreement.

**FC-017:** CRLF; Fix All over two unions in one document (`A` and `F` of the example), as the batch base requires.

**Test harness.** The analyzer tests compile against .NET 9 reference assemblies. Every union test needs the `UnionSupport` reference, as the RH4012 tests have. The Fix All scenario needs the synchronous accessor.

**Names.** Never name a test union `Value` (CS0542).

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Analyzer: new `RH5418EmptyUnionsShouldUseSemicolonDeclarationsAnalyzer` on the existing base, `UnionDeclaration`, `CSharp15` | must change (new) |
| Analyzer: new `RH2110NestedUnionsShouldNotBeUsedAnalyzer` on `NestedTypeAnalyzerHelper` | must change (new) |
| Code fix: new `RH5418…CodeFixProvider` on `EmptyTypeDeclarationShouldUseSemicolonCodeFixProviderBase` | must change (new); Fix All through the base |
| `EmptyTypeDeclarationSemicolonAnalysisUtilities` / the empty-type base | must stay, unless the lead takes the SHARED-DIAG-008 fix in (*Internal choices*) |
| `NestedTypeAnalyzerHelper` | must stay |
| Formatter (FMT-STRUCT-017/018) | must stay; it already converts unions |
| Resources, `Release.ruleset`, `Reihitsu.sln` items | must change (implementer) |
| Rule pages RH5418, RH2110; package README rows | written; content matches the siblings, no change needed |
| RH2104, RH2106, RH5413, RH5416 analyzers | must stay; they never see a union |
| Tests: new RH5418 (batch base) and RH2110 test classes, and an RH5418 formatter test like RH5416's | must change |

## Refuse option

There is nothing left to refuse. The maintainer answered Q3 and Q4 with A, a new rule each.
- **RH5418** reuses the empty-type bases and adds no guard or mechanism.
- **RH2110** reuses the nested-type helper.

Declaring a shape unsupported is not an option: the diff adds no special case or guard that a precheck shape could delete.

For the SHARED-DIAG-008 defect, refusing means skipping the conditional-directive class. That is already what the fix does: it is withheld. Only the diagnostic is wrong.

## Output questions

_None._ The new rules' outputs are decided by Q3 and Q4. The SHARED-DIAG-008 defect is decided by an unchanged requirement and its own example, so fixing it would put nothing in question; not fixing it leaves today's output on `main` unchanged.

## Internal choices

1. **RH2110-001 example: rename `Value`.**
   - Option: rename the interface's union, for example to `Shape`. The example then compiles.
   - Alternative: keep `Value`. Then the test cannot be written from the example.
   - Recommendation: rename. This is a spec-author edit before the test implementer starts.

2. **RH5418-001's C# 14 clause.** Below C# 15 the parser produces no union declaration, so the clause cannot fail. Options:
   - (a) Keep the clause and the sentence "Under C# 14, no union is reported", and write a characterization test with the expected CS1001 compiler errors. The test is noisy and proves only Roslyn's parser.
   - (b) Keep the clause for uniformity with SHARED-EMPTY-001 and the siblings, and replace the example's last sentence with one that says why: "Below C# 15, `union` does not declare a type, so nothing is reported." No C# 14 test.
   - (c) Drop the clause.
   - No output differs between them.
   - Recommendation: (b). The requirement ID is covered by the C# 15 tests, and *Supported inputs* needs no test for a case no input reaches.

3. **The pre-existing SHARED-DIAG-008 defect in the empty-type base** (*Breaking inputs*). Options:
   - (a) **Adjacent fix in this run.** In the base, do not report when the trivia of `{` or `}` holds a conditional directive or disabled text.
     - This is a cheap trivia check that `HasMeaningfulBodyTrivia` already half does.
     - It changes RH5412–RH5416 diagnostics to what the unchanged SHARED-DIAG-008 and its RH5412 example demand.
     - It inverts the wrong RH5416 test and adds one test per rule.
     - Nothing visible is in question. But it touches five other rules and a component the run does not otherwise change.
   - (b) **Follow-up.** RH5418 ships with the siblings' behavior, and the RH5418 tests write nothing for the conditional-directive cells, so nothing locks in the wrong reading.
   - Recommendation: (b), filed as one follow-up that names the whole matrix and the wrong RH5416 test. The *Scope* conditions favor it: the component is not one the run changes, and the fix spans five rules outside the issue.

4. **RH5418 non-goal wording.** "A union without case types does not exist" is false for a `partial` part (finding 4).
   - Suggested wording: "A union's case-type list, which is not a body: every union declares its case types in one of its declarations, and the list is never checked for being empty."
   - A spec-only edit; no output changes.
   - Recommendation: reword with the RH2110 example fix.

5. **RH2110-001 `Why:` wording.** "Every kind of declaration has its own nested-type rule" overstates it: RH2106 covers both `record` and `record struct`. "Every declaration keyword has its own nested-type rule" is accurate. The same sentence in the change doc's *Decisions* ("one rule per kind of declaration") can stay as it is or take the same fix. Recommendation: reword; it is minor.

## Issue goals

No goal of thoenissen/Reihitsu#1767 changes beyond what `02-skeptic.md` and the maintainer's answers already settled. The new rules meet Done-when 1's "or a recorded decision" for the empty and nested union.

## Architecture findings

_None new._ The defect in the empty-type base has one owner, the shared base and utilities, so it is a plain defect, not a structural one. The duplicated kind lists finding from `02-skeptic.md` stands. The two new rules follow the one-rule-per-kind pattern and add no copy of that list.

## Follow-up candidates

**The empty-type rules report a body that holds a conditional directive** (if the lead picks option 3b).
- RH5412–RH5416, and RH5418 once it exists, report an empty-looking body that holds `#if`/`#else`/`#endif`, `#if false`, or a member that `#if` disables. SHARED-DIAG-008 says they must not; its own RH5412 example is `class A { #if DEBUG #endif }`, "is not reported".
- Today: reported, and no fix is offered. Required: not reported.
- `#region` and `#pragma` stay reported, which is correct.
- The existing test `RH5416EmptyRecordStructsShouldUseSemicolonDeclarationsAnalyzerTests.VerifyDirectiveContainingRecordStructIsReportedWithoutCodeFix` asserts the wrong reading.
- Evidence: `scripts/apply-fix.sh RH5412` and `RH5413` on the four conditional fixtures of the matrix give `no-fix-offered` for each kind's own fixture.
