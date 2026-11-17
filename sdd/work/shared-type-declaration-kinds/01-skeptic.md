Role: skeptic
Commit: 3d6a88ea470731605c718b63fd7c0ea7b5a2a4cb
Verdict: proceed narrower

The prompt carried no intended fix or hoped-for verdict; it asked which of the issue's options, or neither, is worth building.

## Premise

The issue does not report a bug. Every kind list it names matches its spec, and neither option would change any visible output. The mechanism it describes is real. A rule that registers an explicit kind list does not analyze a type-declaration kind it does not list. Commit 7a83845 (thoenissen/Reihitsu#1787) had to edit 19 lists one at a time to add `UnionDeclaration`: 17 analyzer files, `ParameterListParentPolicy`, and `AttributeTargetUtilities`.

The issue's examples are real, but the mechanism does not cause them. Run with `scripts/apply-fix.sh <id> <dir>`, LF and CRLF, at the commit above:

| Fixture | Rule | Result |
|---|---|---|
| `class C(⏎    int a)⏎{⏎}` | RH5108 | no-diagnostic |
| `struct S(⏎    int a)⏎{⏎}` | RH5108 | no-diagnostic |
| `record struct R(⏎    int A);` | RH5108 | fixed (1 action, 1 iteration) |
| `record struct R<T> where T : class;` | RH5110 | no-diagnostic |
| `record R<T> where T : class;` | RH5110 | fixed |
| `struct S<T> where T : class⏎{⏎}` | RH5110 | fixed |

On the same inputs, `scripts/format.sh` gives `class C(int a);`, `struct S(int a);`, and `record struct R<T>⏎    where T : class;`. So the formatter changes these inputs and the rules do not report them.

These gaps are not caused by a forgotten kind. RH5108-001 lists exactly the parameter-list owners it reports: method, constructor, local function, operator, conversion operator, delegate, record, record struct, union. Class and struct primary constructors are not on that list. RH5110-001 lists class, struct, interface, record class, union, method, delegate, and local function, and leaves out record struct. Both rules also register one handler per kind (`RH5108…Analyzer.cs`, `RH5110…Analyzer.cs`, `Initialize`), so they could not use a shared kind set anyway. Covering these cases would add diagnostics. Under P11 that is a behavior change of each rule, not part of this issue. The issue's own "Done when" says no diagnostic changes. → Follow-up candidates.

Reachability: an explicit kind list only matters for the analyzers and for whole-document formatting, and both reach every type declaration. The thing the issue worries about, a kind Roslyn adds in a later version, is not reachable today.

## Mechanism and candidates

This is not a bug. The structural point: each consumer picks type declarations with its own explicit list, so a kind added to the language reaches a consumer only when someone edits that list.

Candidates, taken from Roslyn's `BaseTypeDeclarationSyntax` hierarchy as RH5404's canary pins it (`src/Reihitsu.Analyzer.Test/Formatting/RH5404ElementMustNotBeOnSingleLineAnalyzerTests.cs:534`):

- `ClassDeclarationSyntax` (ClassDeclaration)
- `StructDeclarationSyntax` (StructDeclaration)
- `InterfaceDeclarationSyntax` (InterfaceDeclaration)
- `RecordDeclarationSyntax` (RecordDeclaration, RecordStructDeclaration)
- `UnionDeclarationSyntax` (UnionDeclaration)
- `ExtensionBlockDeclarationSyntax` (ExtensionBlockDeclaration)
- `EnumDeclarationSyntax` (EnumDeclaration)

The consumers do not share one set. Counted by `rg` over the production registrations:

| Set | Consumers |
|---|---|
| Class, Struct, Interface, Record, RecordStruct, Union | RH7102, RH7103, RH7104, RH7105, RH7106, RH7109, RH7110, RH7308, RH7311, `TypesOrganizedWithRegionsAnalyzerBase` |
| Same set without Interface | `MembersShouldBeGroupedByRegionsAnalyzerBase`, `NonOverrideMembersShouldNotBePlacedInOverrideRegionsAnalyzerBase`, RH7309 (SHARED-REGGROUP-001 and RH7309 both exclude interfaces) |
| Same set plus Enum | RH2004, `DocumentationAnalysisUtilities` |
| All eight kinds | RH5404 |
| Per-kind handlers, record struct missing | RH5110 |
| Per-kind handlers, record, record struct, and union only | RH5108 |
| One kind per rule, by design | RH2101–RH2110, RH4002–RH4012, RH5412–RH5418, RH8001–RH8036 |

Some consumers cannot read an `ImmutableArray<SyntaxKind>` at all:

- `ParameterListParentPolicy.IsClosingParenthesisCovered` is a type pattern, and it mixes non-type parents such as methods and lambdas into the same pattern.
- `AttributeTargetUtilities.TryResolveImplicitTarget` maps each type to a different `AttributeTargets` value, so it is not a set.
- The formatter rewriters are typed `Visit<X>Declaration` overrides on `CSharpSyntaxRewriter`: `LineBreakListRewriter`, `MemberLineStartRewriter`, `EmptyTypeDeclarationSemicolonTransform`, and `FieldDeclarationSplitTransform`. Roslyn has no `VisitTypeDeclaration` hook.
- `DeclarationBraceLineBreakRewriter` uses a `switch` on types.

## Breaking inputs

- FC-015: the same set is repeated across 10 registrations, but each one agrees with its own spec. I found no drift that changes output.
- FC-016: does not apply, because no fix is being made.

No defect class to enumerate. Every list I checked agrees with the requirement that names its kinds: RH5108-001, RH5110-001, RH7102-001, RH7103-001, RH7110-001, RH7309, RH7311-001, RH2004-001, SHARED-TYPEREGION, and SHARED-REGGROUP-001.

If the narrow option below is built, its test has to cover:

- every concrete `BaseTypeDeclarationSyntax` subtype, with every `SyntaxKind` each one carries;
- `RecordDeclarationSyntax` carrying two kinds. A canary that checks types only, like RH5404's, cannot see a new kind on an existing type, which is how `RecordStructDeclaration` arrived.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Analyzer registrations (all listed above) | Must stay. Each list is its spec's decision. |
| `ParameterListParentPolicy`, `AttributeTargetUtilities` | Must stay. They are not kind sets. |
| Formatter rewriters / visitors | Must stay. Typed visitor overrides cannot read a kind set. |
| Code fixes | Not involved |
| Rule docs, specs | Not involved |
| `Reihitsu.Core` | Not involved. The narrow option adds no production code. |
| `Reihitsu.ArchitectureTests` | May change (narrow option: one test) |
| Formatter / diagnostic / code-fix output | Must stay |

## Refuse option

There is no input class to refuse, because nothing produces wrong output. The equivalent of refusing is making no change. It costs 0 lines. The RH5404 canary already fails when Roslyn adds a new `BaseTypeDeclarationSyntax` subtype, and that is how unions surfaced.

No shape here can be declared unsupported.

Cost of the issue's options:

- **Option 1, a shared `ImmutableArray<SyntaxKind>` in Core.**
  - It can replace at most the ~16 multi-kind registrations, and it needs at least four sets or per-rule exclusions.
  - It cannot serve the policy, the attribute mapping, the typed formatter visitors, or the per-kind handlers in RH5108 and RH5110.
  - It does not deliver "one edit". Each consuming rule's spec still lists its kinds and would need its own edit. Adding a kind to the shared set would change the diagnostics of ten or more rules in one change, without each spec deciding it. Commit 7a83845 shows that per-rule decision is the real work: specs were edited and two rules were added.
  - Do not build.
- **Option 2, an architecture test that names every rule whose list lacks a kind and has no recorded exclusion.**
  - It needs to discover registrations, either through a recording `AnalysisContext` or a source scan.
  - It needs an exclusion table that repeats every spec's list a third time (spec, code, table), including about 35 one-kind-per-rule analyzers.
  - That adds a mechanism and creates one more place for the same rule to drift (the issue's own concern).
  - Do not build as specified.
- **Narrower option (recommended).** One test that pins the full set of type-declaration (syntax type, kinds) pairs Roslyn currently has, and fails when a Roslyn bump changes it. Its message tells the reader to review the kind lists and the specs that list type kinds. This adds the kind-level detection RH5404's type canary lacks. Cost: one test, no production lines, a tests-lane diff.

## Output questions

_None._ No option within this issue changes visible output. The finds that would change output are under Follow-up candidates.

## Internal choices

1. **Where the narrow canary lives.**
   - A: in `Reihitsu.ArchitectureTests`, repository-wide, next to RH5404's.
   - B: replace RH5404's canary.

   Recommend A. RH5404's canary is tied to RH5404's own spec and its comment in `RH5404…Analyzer.cs:86-88`. Removing it would also mean editing that comment.
2. **Narrow canary or no change at all.** Both change no output.
   - Recommend the canary. It closes the one blind spot (a new kind on an existing type) at the cost of one test.
   - Pick no change if you consider RH5404's existing type canary enough. In that case the PR uses `Refs #1788`.
3. **PR link.** The issue's purpose is not missing a new type-declaration kind. The canary meets it; "one edit" is shown not to be achievable. `Closes #1788` with the departure stated fits. That is your call.

## Issue goals

- **"Lane: tooling".** Wrong: the named paths are production and tests. The narrow option needs only the tests lane, which the claimed bug-fix lane admits. No visible change.
- **"One shared kind set … read by the registrations, the policy, and the visitors".** Drop. The policy, the attribute mapping, and the visitors cannot consume a kind set. The registrations use at least six different sets. The specs keep per-rule lists either way. No visible change.
- **"Done when: adding a type-declaration kind needs one edit".** Drop as unachievable. Each rule's spec and output decision needs its own edit. No visible change.
- **"Done when: or the architecture test names every rule whose list lacks the kind and has no recorded exclusion".** Change it to: "a test fails when Roslyn's set of type-declaration kinds changes". The per-rule exclusion table would restate every spec a third time. No visible change.
- **"No diagnostic or formatter output changes".** Keep.
- **"RH5108 and RH5110 still miss other kinds today".** The text is accurate as observed behavior, but it is outside this issue's no-output-change scope, and neither option would fix it. → Follow-up candidates.

## Architecture findings

_None._ Every list agrees with its spec, and I found no input where two components disagree. The repeated set is one decision per rule, each stated in that rule's spec.

## Follow-up candidates

Each of these changes visible output. They are written in output-question form for the follow-up issues; the issue manager should search for duplicates first.

**1 · RH5108 · Parameter list must follow declaration — class and struct primary constructors**

Input:
```csharp
class C(
    int a)
{
}
```

Before (`main`): not reported. The formatter writes `class C(int a);`.

After, A (recommended):
```csharp
class C(
    {|RH5108:int|} a)
{
}
```
The fix joins the first parameter to the opening parenthesis, as it already does for `record struct R(⏎    int A);`.

After, B: not reported (unchanged).

Why A: under P11 this is RH5108's own gap. A primary-constructor parameter list is the same construct as a record's, which it already reports. The same question applies to `struct S(⏎    int a)`.

**2 · RH5110 · Generic type constraints should be on their own line — record struct**

Input:
```csharp
record struct R<T> where T : class;
```

Before (`main`): not reported. The formatter writes:
```csharp
record struct R<T>
    where T : class;
```

After, A (recommended):
```csharp
record struct R<T> {|RH5110:where|} T : class;
```
The fix gives the formatter's layout above.

After, B: not reported (unchanged).

Why A: RH5110 already reports the same clause on `record R<T>` and on `struct S<T>`.

**3 · Formatter · braces of an extension block**

Input:
```csharp
public static class E
{
    extension(int value) {
        public int Twice() => value * 2; }
}
```

Before (`main`, `scripts/format.sh`): the method body is placed, but the extension block's braces stay where they are:
```csharp
public static class E
{
    extension(int value) {
        public int Twice()
        {
            return value * 2;
        } }
}
```

After, A (recommended):
```csharp
public static class E
{
    extension(int value)
    {
        public int Twice()
        {
            return value * 2;
        }
    }
}
```

After, B: unchanged.

Why A: FMT-BREAK-002 places the braces of every type and member body, and an extension block is the only braced declaration left out. `DeclarationBraceLineBreakRewriter`'s `switch` does not list `ExtensionBlockDeclarationSyntax`. Whether FMT-BREAK-002's word "type" covers an extension block is unstated, which is why this is a question.

RH5402 and RH5404 report nothing on this input either, so the analyzers and the formatter do not disagree here.

**4 · RH7103 and the other member-order rules (RH7102, RH7104–RH7106, RH7109, RH7110) · members of an extension block**

Input:
```csharp
public static class E
{
    extension(int value)
    {
        public int Twice() => value * 2;

        public static int Zero() => 0;
    }
}
```

Before (`main`): RH7103 does not report it. Checked with `scripts/apply-fix.sh RH7103`, which reports no-diagnostic. The same members in a `class` are reported and fixed. The other rules in this group do not register `ExtensionBlockDeclaration` either; I read that from their code but did not run them.

After, A: `Zero` is reported by RH7103, as it is in a class.

After, B (recommended until decided): not reported (unchanged).

Why: each of these rules' specs lists the kinds it checks and leaves out extension blocks. Whether an extension block's members are ordered like a type's is a new decision for each rule. The formatter does not judge member order, so leaving it unreported creates no permanent diagnostic.
