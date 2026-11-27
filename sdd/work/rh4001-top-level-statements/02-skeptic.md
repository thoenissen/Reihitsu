Role: skeptic
Commit: 8bc0a68
Verdict: proceed

My prompt carried no intended fix, suspected cause, or hoped-for verdict; nothing was ignored.

## Premise

Real, and common. The caller is the analyzer itself, which runs in the IDE and in the build, plus its rename code fix. `RH4001TypeNameShouldMatchFileNameAnalyzer.OnSyntaxTreeAction` (`src/Reihitsu.Analyzer/Rules/Naming/RH4001TypeNameShouldMatchFileNameAnalyzer.cs`) takes the first `BaseTypeDeclarationSyntax`/`DelegateDeclarationSyntax` under the compilation unit and its namespaces. It never looks at `GlobalStatementSyntax`, so a file with top-level statements is compared with the type declared after them.

The shape is what the stock `dotnet new webapi` template produces: a `record WeatherForecast` declared after the top-level statements of `Program.cs`. Every minimal-API project created from that template is reported today, and the fix renames its entry point.

`scripts/apply-fix.sh RH4001 <dir>`, with each fixture in its own folder:

```csharp
// l/Program.cs
var forecast = new WeatherForecast(System.DateTime.Now, 20);

System.Console.WriteLine(forecast);

internal record WeatherForecast(System.DateTime Date, int TemperatureC);
```

`== l/Program.cs [LF] == fixed`, `Document renamed: l/Program.cs -> l/WeatherForecast.cs` (CRLF the same).

```csharp
// a/Program.cs  (the #1011 Q3 input)
System.Console.WriteLine(new Helper());

internal class Helper
{
}
```

`fixed`, `Document renamed: a/Program.cs -> a/Helper.cs` (LF and CRLF).

The maintainer's decision is on record: #1011, comment of 2026-10-18 (question round 3, "Q3 — Behavior change: files with top-level statements are not reported. Tracked in #1086."). RH4001-007 states exactly that decision, so the spec is not wrong instead. No other requirement's `Why:` conflicts with it: RH4001-001 already limited itself to files without top-level statements.

## Mechanism and candidates

_N/A._ This is a behavior change, not a bug. The decision is one predicate over the compilation unit's members. Candidate member kinds, from `CompilationUnitSyntax.Members` (`MemberDeclarationSyntax`):

- `GlobalStatementSyntax`, which wraps every top-level statement kind, local functions and local declarations included. These are exempt.
- `BaseNamespaceDeclarationSyntax`, both block and file-scoped, and `BaseTypeDeclarationSyntax`/`DelegateDeclarationSyntax`. These are not statements.
- `IncompleteMemberSyntax`. It occurs in parse-error input only.

Global attributes, usings, and extern aliases are not members.

## Breaking inputs

The failure classes that reach the change are FC-002, FC-003, and FC-033. The FC-033 risk is test harness only.

- **FC-003, the statement boundary on both sides.** Example 3 in the spec does not test the "statement inside a type member is checked" side. Its file `Program.cs` declares `Program` first, so it gives no diagnostic whether or not the predicate wrongly counts statements inside methods: a `DescendantNodes().OfType<StatementSyntax>()` predicate passes it. The other side needs a mismatched name. Today's output:

  ```csharp
  // k/Startup.cs
  internal static class Program
  {
      private static void Main()
      {
          System.Console.WriteLine();
      }
  }
  ```

  `fixed`, `Document renamed: k/Startup.cs -> k/Program.cs`. It must stay reported after the change. Existing test: none found. The test implementer should add it, with the diagnostic on `Program`.
- **FC-002, "among its members".** The predicate looks only at the members of the compilation unit, not at descendants. The boundary test above covers it.
- **FC-033, the harness.** A test project's default output kind is a library, where top-level statements raise compiler error CS8805. Tests with top-level statements must set `TestState.OutputKind = OutputKind.ConsoleApplication`, as `RH2008`, `RH2009`, and `RH3106` tests do. Otherwise the verifier fails on the compiler error, or the error gets suppressed and hides what is being tested. The multi-document example ("`Startup.cs` is reported also when another file of the project holds top-level statements") needs both documents in one ConsoleApplication project, named as in the spec. The RH4001-006 tests show how to add a named second document.

Candidate inputs to cover:

| Input | Today | After | Existing test |
|---|---|---|---|
| `Program.cs`: statement + `class Helper` (spec ex. 1) | reported, renamed to `Helper.cs` | not reported | none found |
| `Program.cs`: statement + top-level local function + block namespace `Tools { class Calculator }` (spec ex. 2) | reported, renamed to `Calculator.cs` (spec author's run) | not reported | none found |
| `Program.cs`: `record WeatherForecast` after statements (template shape, fixture `l`) | reported, renamed | not reported | none found; optional, ex. 1 is the same predicate |
| `Startup.cs`: `class Program` whose `Main` holds a statement (fixture `k`) | reported | reported | **none found — add** |
| `Program.cs`: `class Program` with `Main`, then `Helper` (spec ex. 3) | not reported | not reported | none found |
| `Startup.cs`: `class Helper`, while another document holds top-level statements (spec ex. 4) | reported | reported | none found |
| `Program.cs`: statement + `public partial class Program { }` (fixture `m`) | not reported | not reported | none found; unchanged either way, not required |

Inputs I settled and the test implementer should not cover:

- **Parse-error shapes.** Statements after a type, statements inside a block namespace, statements after a file-scoped namespace, and a top-level local function after a type all gave `parse-error` from `apply-fix.sh` (fixtures `b`, `c`, `f`, `j`). Code that fails to parse is not a supported input for any decision here.
- **Statements before a file-scoped namespace** (fixture `d`). This parses, but the compiler rejects it. The spec's syntactic predicate exempts it, which is harmless.
- **A lone `;` top-level statement** (fixture `g`). The compiler rejects it when it is the only top-level statement. The predicate exempts it, harmless.
- **A statement only in an inactive `#if` group** (fixture `e`, reported today). It is disabled text, not a member, so the file is still checked. That follows from the parsed tree of the configuration being analyzed, which is the compiler's own result, not a model of it (not FC-044). This case needs no test and no spec sentence.
- **Script files (`.csx`).** No caller reaches them: the analyzer runs on project compilations.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Analyzer `RH4001TypeNameShouldMatchFileNameAnalyzer` | Must change: return early when the root's members hold a `GlobalStatementSyntax` |
| Code fix `RH4001TypeNameShouldMatchFileNameCodeFixProvider` | Must stay: it is only offered on a reported diagnostic |
| Fix All | Not involved: RH4001 has none |
| `RH4001TypeNameShouldMatchFileNameHelper` | Must stay |
| `Reihitsu.Core` | Not involved |
| Formatter | Not involved: a file name is not something the formatter judges, so FC-013/FC-014 do not apply |
| Rule page `documentation/rules/RH4001.md` | Must stay; see *Internal choices* |
| Tests | Must change: new RH4001-007 tests (the architecture test `EveryRequirementOfAnEnforcedSpecIsCovered` fails until one exists) |
| `RuleDocumentationExampleOptOuts` | Must stay |
| Tooling tests that use RH4001 (`CodeFixRunCommandTests`) | Must stay: their fixtures hold no top-level statements |

## Refuse option

The change is itself the cheapest refusal: one member check in the analyzer, with no new type and no new mechanism.

Withholding only the fix would be the narrower alternative: keep the diagnostic on `Program.cs` and offer no rename. It is not cheaper, and it leaves a warning that the user can clear only by moving the helper type out or by renaming the entry point. The maintainer decided against reporting at all.

Declaring an unsupported shape does not apply: this is an analyzer decision with no formatter phase or code-fix special case to delete.

The input class is not rare: it is the standard template's shape. There is no simpler option.

## Output questions

_None._ The one visible change is the maintainer's recorded #1011 Q3 decision.

## Internal choices

- **Predicate: "any member of the compilation unit is a `GlobalStatementSyntax`" versus "the first member is one".** In valid code, top-level statements must come before every namespace and type declaration, so the two agree on every input that compiles. Statements placed after a type fail to parse. I recommend the spec's "any member": it reads as the requirement says, and the first-member variant saves nothing.
- **Rule page unchanged.** I agree. `.claude/commands/create-rule-doc.md` (lines 34 and 105) and the `AGENTS.md` rule-page convention keep a rule-specific exception off the page; it belongs in the spec. The page's existing `## Exceptions` section (split files, the EF timestamp prefix, generic braces) predates that guidance. It is precedent, but not a reason to add to it. Removing or trimming that section is outside this run.
- **Whole file versus "only the types next to the statements".** The change doc's first Decision. In compiling code every type follows the statements, so the distinction changes no output. Keep it as written.

## Issue goals

The issue's goals hold. Its single "Done when" item (the #1011 Q3 input is not reported, a file without top-level statements still is, both have tests) serves the purpose and needs no change. I would sharpen the "still is" half: it should include a file whose statements stand inside a type member under a mismatched name (fixture `k`). A file with no statements at all is not enough. This changes no visible output.

## Architecture findings

_None._

## Follow-up candidates

_None._
