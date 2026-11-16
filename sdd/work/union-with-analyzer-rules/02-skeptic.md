Role: skeptic
Commit: 856504b
Verdict: output question

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Premise

Real. thoenissen/Reihitsu#1767 is `agent-filed`; its goals were treated as proposals.

**Reachability.** Every union and `with(…)` case in the run can be reached:

- **Analyzers and their code fixes** run in a user project at C# 15. `LanguageVersion.Latest` maps to `CSharp15` in Roslyn 5.12, and a union compiles against the .NET 11 runtime assemblies without the test-only `UnionSupport` (scratch compile below).
- **The CLI** parses at C# 15 by default: `LanguageVersionResolver.MaxSupportedLanguageVersion = CSharp15`, and `FormatCommandHandler` uses it in its parse options.
- **Whole-document formatting** runs on that same tree.

No case was dropped for being unreachable.

**How I checked.** I used `scripts/apply-fix.sh` and `scripts/format.sh` on fixtures in the scratchpad. I also used a scratch compile, a file-based app referencing Microsoft.CodeAnalysis.CSharp 5.12.0 over the net11 trusted-platform assemblies, to get compiler errors and symbol facts. No existing entry point prints those. The fixtures are deleted.

Symbol facts:

- `UnionDeclarationSyntax` derives from `TypeDeclarationSyntax`. Code that matches `TypeDeclarationSyntax` or `BaseTypeDeclarationSyntax` already sees unions. Only explicit `SyntaxKind` lists and `switch`/`is` chains miss them.
- The union symbol has `TypeKind=Struct` and `BaseType=System.ValueType`. It implicitly implements `System.Runtime.CompilerServices.IUnion` with a generated `Value` property.
  - A user-declared `Value` is CS0102.
  - A user `override string ToString()` compiles and overrides `System.ValueType.ToString`. So change doc example 8 (RH7401, region `Object`) holds under SHARED-REGGROUP-003.
- An attribute limited to `AttributeTargets.Class` on a union gives CS0592. This confirms the struct target.
- Case types cannot carry attributes (`union U([Obsolete] int, string);` is CS1073). RH5108-002's attribute-list clause therefore never applies to a union.
- A public single-parameter constructor in a union is CS9374. This confirms the spec author's list.
- These all compile: change doc examples 1, 3, 5 and 8 (nested union in a union, `readonly public union`, `internal protected union` in a class, static, readonly and const fields).

**Before-state, checked against `main` behavior on this branch (no production change yet):**

- RH2004:
  - Over change doc example 1, only `Get` is fixed (`private int Get()`). `Choice` and `Inner` are not reported.
  - A class, delegate, enum, static field and indexer declared in a union are already reported and fixed with `private`. The fix's default is `_ => PrivateKeyword` in `RH2004AccessModifierMustBeDeclaredCodeFixProvider.CreateUpdatedDeclaration`.
  - A `union IU(int, string);` nested in an interface is not reported today. Under Q1 A it gets `public` from the same switch.
- RH5404, RH5105, RH5106, RH5108, RH5110, RH5507, RH5508: no diagnostic on the union inputs of change doc examples 2, 9 and 10.
- RH5107 and RH5109 already report and fix the case-type list: `union L5(int⏎, string)` becomes `union L5(int,⏎         string)`, and RH5109 gives one case type per line.
- RH5101 already fixes `[with(⏎4), 1]` to `[with(4), 1]`. RH5305 already reports a `with(…)` element and explodes the collection.
- Formatter, whole document, two passes, idempotent:
  - `[Serializable, Obsolete] public union U1(…)` keeps one list. `[Serializable][Obsolete] public union U2(…)` is already broken to two lines, so placement works and only the split is missing. `[type: Serializable, Obsolete]` on a union keeps one list. Struct twins are split.
  - `union L4<T>(T, string) where T : class;` gets `where` on its own line, 4 columns right of the declaration's first token. The nested-in-class form gets the same layout with a body. This agrees with RH5110-001's column rule, so widening RH5110 is FC-013-safe.

**One "before" in the change doc is wrong.**

- RH7104 already reports `partial class N` declared in a union and fixes it to `private partial class N`. `scripts/apply-fix.sh RH7104` over change doc example 4: `fixed`, with only `N` changed. `RH7104PartialElementsMustDeclareAccessModifierAnalyzer` registers `ClassDeclaration`. Its fix takes the accessibility from the semantic model and passes `typeDeclaration.Parent is TypeDeclarationSyntax`, which a union is.
- So change doc example 4's "before" line "(the class `N` in the union: not reported either)" and the sentence at `sdd/changes/2026-11/union-analyzer-rules.md:150` are false.
- The only new RH7104 output is the top-level `partial union U`. The union part of RH7104-004 ("private for a type nested in … union") writes down existing behavior. The handover listed RH7104-004 as *changed*; it is *clarified*.
- This must be corrected before Q1 reaches the maintainer, because the question shows "before" outputs.

**Change doc example 3 is right, against the letter of RH7110-001.** RH7110 on `Z` follows the code: `narrowestSeenByGroup` keeps the narrowest accessibility seen, and a reported member does not update it. It also follows SHARED-USING-007's general sentence: every ordering rule compares an element "with every earlier element of its group". RH7110-001's text still says "the previous member of the same group". For `private A`, `public B`, `public Z`, that reading reports `B` only. Existing test `MembersAreComparedWithTheLatestAccessibilitySeenInTheirGroup` asserts the code's reading and is tagged SHARED-USING-007, not RH7110-001. See *Internal choices*.

**Spec diff vs. other specs.** Checked against FMT-STRUCT-028, FMT-BREAK-011/012/023, FMT-STRUCT-017, SHARED-ATTR-001, SHARED-REGGROUP-003, SHARED-USING-007, P11 and SHARED-DIAG-010:

- No requirement with a good `Why:` is contradicted.
- FMT-STRUCT-028 already says "a type", so the formatter split for a union is a bug against an unchanged requirement (see Q2 below).
- The union-declaration-layout change doc decided the formatter side of unions and records no attribute decision. Nothing conflicts.

## Mechanism and candidates

Not a bug fix. The reason for the gap, stated so membership can be decided:

> A rule misses unions exactly when it selects declarations by an explicit list of type-declaration kinds or concrete syntax types (`RegisterSyntaxNodeAction(…, SyntaxKind.ClassDeclaration, …)`, `parent is ClassDeclarationSyntax or …`, a `switch` over concrete types). A rule that matches `TypeDeclarationSyntax`/`BaseTypeDeclarationSyntax`, the semantic model, or `SyntaxKind.ParameterList`/`ArgumentList` already sees unions.

I derived the candidates from the dispatch code (`rg` for `SyntaxKind.StructDeclaration|StructDeclarationSyntax|RecordStructDeclaration|ClassDeclarationSyntax` under `src/Reihitsu.Analyzer`, `src/Reihitsu.Core`, `src/Reihitsu.Analyzer.CodeFixes`), not from the issue.

Explicit lists that miss `UnionDeclaration`:

| Rule / owner | Defining code | In the spec diff | Note |
|---|---|---|---|
| RH2004 | registration list | yes | |
| RH5105/RH5106 | `Core/ParameterListParentPolicy.IsClosingParenthesisCovered` | yes | One shared owner for both rules |
| RH5108 | `Initialize`: `RecordDeclaration`, `RecordStructDeclaration` only | yes | Class and struct primary constructors stay unchecked (follow-up) |
| RH5110 | registration + `switch` | yes | Record struct is still missing (follow-up) |
| RH5404 | registration; comment at `:90-92` | yes | The comment's reason is stale: .NET 11 ships the support types (the scratch compile succeeded) |
| RH5507/RH5508 (and the formatter split) | `Reihitsu.Core/AttributeTargetUtilities.TryResolveImplicitTarget` | yes | One owner for analyzer and formatter |
| RH7102, RH7103, RH7104, RH7105, RH7106, RH7109, RH7110 | registration lists | yes | |
| RH7305/RH7305A | `Base/TypesOrganizedWithRegionsAnalyzerBase` | yes | |
| RH7308, RH7309, RH7311 | registration lists | yes | |
| RH7401–RH7412 | `Base/MembersShouldBeGroupedByRegionsAnalyzerBase`, `Base/NonOverrideMembersShouldNotBePlacedInOverrideRegionsAnalyzerBase` | yes | |
| RH2104 (nested structs), RH2106 (nested records) | one kind each | Q4 | A nested union is neither |
| RH5413/RH5416 (empty struct/record struct) | one kind each | Q3 | |
| RH2103 (struct primary constructors) | `StructDeclaration` | no | A case-type list is not a primary constructor. The exclusion is right but not recorded (see *Issue goals*) |
| RH2005 (fields must be private) | `Parent is ClassDeclarationSyntax` or record class | no | Its non-goal "Fields of structs, record structs" covers a union by its struct nature. Not recorded (see *Internal choices*) |
| RH4003/RH4011, RH8003/4/9/10 | struct kinds | n/a | Naming and documentation, done by the sibling issue (RH4012, RH8035/RH8036) |

Already reach unions through `TypeDeclarationSyntax`, `BaseTypeDeclarationSyntax`, the semantic model, or list kinds. No change is needed:

- RH2101 and the other nested-type rules as container (SHARED-NESTED-001);
- RH7104 for a nested type (above);
- RH2004 for members and nested non-union types;
- RH7002, RH4001, RH8201/RH8203;
- RH5035, RH5107, RH5109 (`ParameterList`);
- RH5101, RH5102, RH5203, RH5038 (`ArgumentList`);
- RH5305;
- RH6006 (token scan; no rule checks the space before `(`, per its non-goals).

The spec author's list matches this set. I found no rule the spec diff should cover but misses.

## Breaking inputs

Failure classes that reach the changed code:

- **FC-013 / SHARED-DIAG-010.** Each widened layout rule must not flag formatter output.
  - Verified for RH5110 (column) and the RH5507/RH5508 shape after the split, which the same Core change turns on in both surfaces.
  - The spec author verified RH5105/RH5106/RH5108 joins.
  - The test implementer should assert formatter output is clean for one union per widened layout rule.
- **FC-006, rewrites that compile:**
  - RH2004 inserts `private`/`internal`/`public` on a union: top level, in a union, in a class, in an interface.
  - RH7104 gives `internal partial union`.
  - RH7105 orders `readonly`, `partial`, `file` with accessibility.
  - RH7106 orders `protected internal`.
  - RH5110 moves `where` on a bodiless union (`;` stays behind the clause).
- **FC-024:** a `partial` union split over two declarations for RH7104, RH7305 and RH7311 (each declaration judged on its own); a `file union`.
- **FC-019/FC-020/FC-021** in gaps the union fixes join:
  - a comment, and an `#if` block, between the union name and `(`; RH5105 must stay silent per SHARED-JOIN-001, while RH5035-002 reports the comment;
  - behind `(` (RH5108);
  - in front of `)` (RH5106);
  - between `)` and `where` (RH5110);
  - a comment inside `[A /* c */, B]` on a union: neither the formatter nor the RH5508 fix splits it (FMT-STRUCT-028).
- **FC-030/FC-032:** the formatter split now reaches every code fix that formats a union node. The RH5404 fix on `[A, B] union U(int, string) { public int X => 1; }` will also split the list. That is the formatter's output, and it is consistent, but the test should pin it.
- **FC-027:** a union nested in a class, struct, record, interface and union for every widened rule; a generic union with several constraint clauses (RH5110-002: one diagnostic, on the first misplaced clause).
- **FC-017:** CRLF for every fix (`apply-fix.sh` already runs both); a second fix pass converges.

Candidate inputs beyond the spec examples:

- `[type: A, B] union U(int, string);` splits into two `type:` lists (formatter and RH5508).
- `[A][B] union U(int, string);` gets RH5507 only.
- `union IU(int, string);` in an interface gets RH2004 with fix `public union IU`.
- A one-line empty `union V(int, string) { }` gets RH5404, fix `union V(int, string);`. Whether anything else reports it depends on Q3.
- A union with `public override string ToString()` in `#region Methods` gets RH7401 (region `Object`).
- A union implementing `IComparable` gets RH7409 (region `IComparable`).
- The RH5404 test canary at `RH5404ElementMustNotBeOnSingleLineAnalyzerTests.cs:539-554` lists `UnionDeclarationSyntax` as excluded and must be inverted.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Analyzer: registrations of the rules in the table above | must change, by adding the union kind to each list |
| `Reihitsu.Analyzer/Core/ParameterListParentPolicy` | must change (RH5105/RH5106) |
| `Reihitsu.Core/AttributeTargetUtilities.TryResolveImplicitTarget` | must change: `UnionDeclarationSyntax` maps to `AttributeTargets.Struct`. This one edit moves RH5507, RH5508 and the formatter split together |
| Formatter phases | must stay. The split follows from Core. No phase code changes; the visitors already know unions |
| Code fixes RH2004, RH5404, RH5105, RH5106, RH5108, RH5110, RH5507, RH5508, RH7102, RH7103, RH7104, RH7105, RH7106, RH7109, RH7309 | should need no change: they key on `TypeDeclarationSyntax`, the semantic model, or the formatter. The implementer confirms with the tests |
| Fix All | not involved beyond the existing providers |
| RH5404 analyzer comment `:90-92` | must change: it is removed with the registration (Done-when 2) |
| Rule pages RH5105, RH5108, RH5404, RH5507, RH5508 | must change, as in the diff |
| Other rule pages | must stay |
| New rules RH2110/RH5418 | only on Q3/Q4 answers (escalation 5) |
| Tests | must change: union tests per widened rule plus the RH5404 canary |

## Refuse option

**Refuse:** keep every rule silent on unions (Q1 B) and keep the formatter's single list (Q2 C).

- Cost of refusing: about 20 rule specs gain a "unions are not checked" decision. Q2 C also needs an exception added to FMT-STRUCT-028, which today says "a type" with a good `Why:`.
- Cost of the requested change: one `SyntaxKind.UnionDeclaration` per registration list (about 17 files), one type in `ParameterListParentPolicy`, one `case` in `AttributeTargetUtilities`, and RH5108/RH5110 registrations. No new type, no new mechanism, no guard.
- The input class is not rare: unions are a released C# 15 feature that the CLI formats by default.
- Refusing costs more spec text than the change costs code, so I do not recommend it.

**Declaring a shape unsupported** is not an option here. The change adds no special case, guard or refusal that a precheck shape could delete.

**Simpler options:**

- The formatter half of Q2 needs no question; see *Output questions*.
- Q3 and Q4 can be answered B ("no new rule") without blocking Q1/Q2. A new rule there is cheap: RH5413 and RH2104 are about 30-line subclasses of existing bases, plus resources, a rule page, a spec and tests. B is still a complete outcome under P11 for Q3, because the formatter owns the empty body (FMT-STRUCT-017).

## Output questions

The spec author's Q1–Q4 stand, with these corrections and additions before the lead asks them.

**Q1** (unions join the type-kind rules).

- Fix the RH7104 "before" first. In change doc example 4, the type `N` nested in a union is already reported and fixed today. Only `partial union U` is new.
- Q1's own example (RH2004 on `Choice` and `Get`) is right as written.
- Recommendation: A. Each rule names a part that a union has in the same form as a struct. The formatter already lays these parts out that way, and the cost is one kind per list.

**Q2** (union attribute lists).

- The formatter half is decided. FMT-STRUCT-028 is unchanged in substance and requires a type's list to be split. Today's union output is a bug against it, and fixing a bug against an unchanged requirement puts nothing in question (`squad/README.md`, *What reaches the maintainer*).
- Option C would change FMT-STRUCT-028 against its `Why:`.
- Only the analyzer half is in question: whether RH5507 and RH5508 report unions.
- One visible detail Q2 should show: under A, the diagnostic on a union reads with the struct rule's title and message ("Struct attributes must be on their own line"). Record structs read the same way today.
- Under B (own rule IDs) the analyzer bases would need a union exception against the shared target resolution. That is a special case the Core change otherwise avoids.
- Recommendation: A.

```csharp
// input
[Serializable, Obsolete] public union U(int, string);
```

```csharp
// main: not reported; formatter output
[Serializable, Obsolete]
public union U(int, string);
```

```csharp
// A (recommended): RH5507 and RH5508 on `[Serializable, Obsolete]` ("Struct attributes …"); formatter and RH5508 fix:
[Serializable]
[Obsolete]
public union U(int, string);
```

```csharp
// B: not reported by RH5507/RH5508 (new union rule IDs instead, escalation 5); formatter output as in A
```

**Q3** (empty union body, new rule RH5418).

- The next free ID is right (RH5417 is taken by "Function and accessor body braces must not share a line").
- The question should show that under Q1 A the one-line form `union V(int, string) { }` is already reported by RH5404, and its fix gives `union V(int, string);`. Q3 decides only the multi-line form:

```csharp
public union U(int, string)
{
}
```

- Under B nothing reports it and the formatter writes `public union U(int, string);`. P11 allows that.
- Recommendation: A, for parity with RH5412–RH5416. B is acceptable and avoids escalation 5.

**Q4** (nested union, new rule RH2110).

- RH2108 and RH2109 are taken (delegates; Razor `@code`), so RH2110 is the next free ID.
- A nested union is a struct to the compiler, but RH2104's message names structs, and every nestable syntax kind has its own nested-type rule (RH2106 covers record and record struct together). Folding unions into RH2104 is not a good third option.
- Recommendation: A.

Issue question 3 (RH5305 and `with(…)`) is not in question, as the spec author states. Today's output (checked with `apply-fix.sh`) is what FMT-BREAK-022 and RH5305-002 already require.

## Internal choices

- **Change doc example 4 and RH7104-004.** Rewrite example 4's "before" as "RH7104 on `N` only, fixed to `private partial class N`". Drop the sentence at line 150. Move RH7104-004 from *changed* to *clarified* in the change doc's affected list.
  - Alternative: leave them as they are. The maintainer would then see a false before-state.
  - Recommendation: correct them.
- **RH7110-001 wording.** RH7110-001 says "the previous member of the same group". The code and SHARED-USING-007 compare with every earlier member of the group, and an existing test asserts that.
  - The spec author is editing RH7110-001 anyway. Replacing "the previous member of the same group … that carries accessibility modifiers" with "an earlier member of the same group … (SHARED-USING-007)" brings the text in line with the code. It changes no output.
  - Alternative: leave it and file a follow-up.
  - Recommendation: fix it in this run as an adjacent spec-only cleanup.
- **The change doc's affected list** omits RH7311-002, whose nested-type list gains "union". Recommendation: add it.
- **Record the exclusions the issue's Done-when asks for, as one-line non-goals:**
  - RH2103: "The case-type list of a union, which is not a primary constructor."
  - RH2005: "Fields of unions, which are structs." Unions can declare only static fields; a `public static int S;` in a union stays unreported, as in a struct.
  - Recommendation: add both. They change no output.

## Issue goals

- **"RH5108 (type parameter lists, records only)":** the issue misreads RH5108. It checks the first *parameter*, not type parameters. The spec diff handles it correctly. Nothing to drop.
- **"the nested-type rules RH2101–RH2107, one kind each":**
  - RH2101 and its siblings already treat a union as a container (SHARED-NESTED-001 clarified).
  - RH2102 and RH2103 are about primary constructors and do not apply to a case-type list.
  - Only "a nested union is reported" is open, and that is Q4. Drop "RH2101–RH2107 cover unions" as stated; it changes no output beyond Q4.
- **"RH5412–RH5416 … there is no union variant":** this is Q3. Keep it as a question, not a goal.
- **"Verify with a test per RH55xx rule":**
  - Only RH5507 and RH5508 resolve the struct target.
  - The other RH55xx rules key on other targets: class, interface, method, and so on. Unions are also not parameter owners in any attribute sense, because case types cannot carry attributes (CS1073).
  - Change to "a test for RH5507 and RH5508, and one formatter test". No visible output changes.
- **Done when 1, "or a recorded decision excludes them":** covered by the non-goals proposed above plus the answers to Q3/Q4.
- **Done when 2 (RH5404 comment):** right. The comment's stated reason no longer holds: the scratch compile against the net11 assemblies succeeds without support types. Keep this goal.

## Architecture findings

The analyzers state the set of "type declarations with a member body" again in each rule: about 17 registration lists, `ParameterListParentPolicy`, and the `AttributeTargetUtilities` switch. The formatter states it again in its visitors (`DeclarationBraceLineBreakRewriter`, `MemberLineStartRewriter`, `EmptyTypeDeclarationSemicolonTransform`, …).

- The copies have drifted: every one missed `union`, RH5110 also misses record struct, and RH5108 lists records but not classes.
- This issue and its formatter sibling exist because of that.
- The narrow fix extends existing copies and adds no new one. Some lists differ on purpose (RH7309 excludes interfaces; RH2005 is class-only), so a single shared set would also need per-rule exceptions.
- Recommendation: fix narrowly in this run. File a follow-up for one shared kind set in `Reihitsu.Analyzer/Core`, or an architecture test that fails when a `TypeDeclarationSyntax` subtype is missing from a rule's list without a recorded exclusion.

## Follow-up candidates

- **RH5108 and class/struct primary constructors** (spec author's candidate, likely a duplicate of #1158): confirmed by the registration list. A union's case-type list will be checked while `class K(⏎int a)` is not.
- **RH5110 and `record struct` constraint clauses** (spec author's candidate, likely a duplicate of #1154): confirmed. `RecordStructDeclaration` is absent from RH5110's registration.
- **Type-declaration kind lists duplicated across analyzers and formatter:** see *Architecture findings*.
- **RH7110-001 wording** ("the previous member" vs. SHARED-USING-007's "every earlier element"): spec-only, no output change. This becomes a follow-up only if the lead does not take it into this run (see *Internal choices*).
