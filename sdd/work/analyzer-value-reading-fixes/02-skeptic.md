Role: skeptic
Commit: 92aec76 (spec diff c2866ea..575d1a7; 92aec76 changes only `00-lead.md`)
Verdict: #1071: output question (Unicode escapes), otherwise proceed; #1268: proceed; #1289: proceed (cost warning, see Refuse option); #974: output question (reframed: only RH4009 is open)

My prompt carried no intended fix, suspected cause, or hoped-for verdict. Every issue is `agent-filed`. Fixtures were run from the scratchpad and deleted afterwards.

## thoenissen/Reihitsu#1071

### Premise

Real, through the analyzer on any compilation and through the RH4001 code fix (IDE / `dotnet format`-style fix application); no formatter or CLI path is involved.

- RH4001: `RH4001TypeNameShouldMatchFileNameHelper.GetExpectedFileNameStem` (`src/Reihitsu.Analyzer/Rules/Naming/RH4001TypeNameShouldMatchFileNameHelper.cs:37` and `:47`) builds the expected stem from `Identifier.Text`, which keeps `@`. The analyzer and the fix (`GetRenamedFileName`) both use it. Observed with `scripts/apply-fix.sh RH4001 <fixtures>` (LF and CRLF alike):
  - `class.cs` with `public class @class { }` → reported, fix renames to `@class.cs`
  - `Order.cs` with `public class @Order { }` → reported, fix renames to `@Order.cs`
  - `Bill.cs` with `public class @Invoice { }` → fix renames to `@Invoice.cs`
  - `Cache{TKey}.cs` with `public class Cache<@TKey> { }` → reported, fix renames to `Cache{@TKey}.cs`
  - `Other.cs` with `public class Store<@TKey> { }` → fix renames to `Store{@TKey}.cs`
  - `Abc.cs` with `public class Abc { }` → reported, fix renames to `Abc.cs` (a file name holding a backslash)
  - `Plain.cs` with `public class Plain { }` → no diagnostic (control)
- RH4009: `RH4009NamespaceNotAllowedAnalyzer.cs:63` compares `namespaceSyntax.Name.ToString()` against the list. Not executed: no repository entry point runs an analyzer with a `reihitsu.json` additional file other than a unit test, and no existing test covers `@` or inner trivia. The premise rests on the code region; the test implementer's reproduction settles it.

The SHARED-DIAG-002 demand is unchanged and names `class @class` and `namespace A . B` itself, so every visible change listed by the spec author is decided.

### Mechanism and candidates

Mechanism: a name is compared or turned into a file name from its source spelling (`SyntaxToken.Text`, `SyntaxNode.ToString()`) instead of from the value of its identifiers.

Candidates, from the code that reads names:

| Rule | Where the name is read | Defining type | Spelling axes |
|---|---|---|---|
| RH4001 analyzer | type identifier | `BaseTypeDeclarationSyntax.Identifier`, `DelegateDeclarationSyntax.Identifier` | `@`, Unicode escape |
| RH4001 analyzer | type-parameter identifiers | `TypeParameterSyntax.Identifier` (of `TypeDeclarationSyntax`/`DelegateDeclarationSyntax`) | `@`, Unicode escape |
| RH4001 fix | same two, through the same helper | same | same |
| RH4009 analyzer | namespace name | `BaseNamespaceDeclarationSyntax.Name` (`IdentifierNameSyntax`, `QualifiedNameSyntax`) | `@` on any part, whitespace, line break, or comment between parts, Unicode escape |

The comparable-file-name side (`GetComparableFileName`) is a file-system string and has no spelling axis.

### Breaking inputs

Failure classes: FC-023 (verbatim and Unicode identifiers), FC-003 (both sides: `@` name in the matching and in a non-matching file), FC-016 (the fix and the analyzer share one helper, so both sides must be covered), FC-034 (reproduce the issue's own shapes).

Class matrix — RH4001, axis 1: where the spelling sits {type name, type parameter}; axis 2: spelling {`@`, Unicode escape}; axis 3: surface {analyzer in the matching file, analyzer in a mismatching file, fix rename}. Every cell fails today (observed above for `@` × all, and escape × type name; escape × type parameter not run, same code path). The escape cells depend on the output question below.

Class matrix — RH4009, axis 1: spelling {`@` on first part, `@` on a later part, spaces around `.`, line break around `.`, comment between parts, Unicode escape}; axis 2: {declaration matches an entry by value, does not match}; axis 3: {block-scoped, file-scoped}. Every "matches by value" cell fails today by the code region (not run); the "does not match" cells are reported today and must stay reported, on the whole name.

Inputs the test implementer must cover, at least: the RH4001-002/-003/-005 examples (`@class` in `class.cs` and in `@class.cs`, `@Order` in `Order.cs`, `Cache<@TKey>` in both file names, fix renames `Invoice.cs` → `Order.cs` / `Cache{TKey}.cs`, `@class.cs` → `class.cs`), a file-scoped `namespace @MyCompany.Project;`, and the RH4009-002 example including `namespace {|RH4009:@MyCompany . Data|}` with its location on the whole name. RH4001-006 (fix withheld when the target name exists) must be checked with a `@`-named type too, since the target name it checks changes.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH4001 analyzer + `RH4001TypeNameShouldMatchFileNameHelper` | must change (helper) |
| RH4001 code fix | output changes through the helper; provider code itself not required to change |
| Fix All | not involved (RH4001 returns no Fix All provider) |
| RH4009 analyzer | must change |
| `Reihitsu.Analyzer/Core/NamespaceCasingHelper.GetLocations` | existing helper that already yields each namespace part's `ValueText` (used by RH4007, RH4008, RH7002); must stay, and is the candidate to reuse rather than a private copy (FC-015) |
| `Reihitsu.Core`, formatter, CLI | not involved |
| Rule pages RH4001, RH4009 | must stay: they mention neither `@` nor trivia |
| Specs | RH4001-002/-003/-005, RH4009-002 already clarified by 575d1a7 |

### Refuse option

No code fix is involved for RH4009, so there is nothing to withhold. For RH4001, the refuse option would withhold the fix for a type whose name or type parameters carry `@` or an escape; the analyzer would still have to compare by value (SHARED-DIAG-002 binds the analyzer), so refusing saves nothing — the fix's name comes from the same helper. Requested change is a few lines in two places. The class is rare (`@` on a type name is seldom written) but the requirement is unchanged and decided. No unsupported-shape option: no formatter path. Simpler option: reuse `NamespaceCasingHelper.GetLocations` for RH4009.

### Output questions

**RH4001 · The name of the type should match the filename, and RH4009 · Namespace not allowed — a name written with a Unicode escape**

SHARED-DIAG-002 names the `@` prefix and inner trivia; it does not name Unicode escapes, and the spec author left them out. The C# language treats `Abc` and `Abc` as the same identifier, as it does `@Abc`. A fix that reads the identifier's value changes this output as a side effect, so it should be decided rather than slip through.

Input — `Abc.cs`:

```csharp
public class Abc
{
}
```

Before (`main`, observed): reported; the fix renames `Abc.cs` to `Abc.cs`.

After, A (recommended): not reported, because the type is named `Abc`. In `Invoice.cs`, the fix renames the file to `Abc.cs`. Likewise, with the list `[ "A.B" ]`, `namespace A.B { }` is not reported.

After, B: unchanged — reported in `Abc.cs`; the fix renames it to `Abc.cs`; `namespace A.B { }` is reported.

Why A: the escape is spelling like `@`, and B writes a file name containing `\`, which is a path separator on Windows.

### Internal choices

- RH4009-002's dash clause ("its identifiers without `@`, joined by dots without whitespace or comments") restates SHARED-DIAG-002 instead of only referencing it; harmless, but a second statement can drift. Recommend keeping only the reference; no output effect.
- RH4009-002's new last sentence (an entry is compared as written, so `"@MyCompany.Project"` matches nothing). I agree it follows from the unchanged "is not exactly one of the entries": reading the entry as C# spelling would need a rule no spec states. Visible effect against `main` (`namespace @MyCompany.Project` with that entry becomes reported) is demanded by SHARED-DIAG-002 ("same result as the name without `@`") together with the literal comparison. Recommend keeping it, with a test, so the auditor sees the changed output decided.

### Issue goals

"Done when: both rules compare and derive names from the identifier's value, with regression tests for each input in the questions" — keep. Add RH4001-006 with a `@` name (see Breaking inputs). Nothing dropped.

### Architecture findings

_None._ The analyzer and fix of RH4001 share one helper; the analyzer's private `GetIdentifier` copy only serves the diagnostic location.

### Follow-up candidates

- RH4009 and nested namespace declarations: RH4009-002 covers declarations "declared at file level"; the analyzer registers on every `NamespaceDeclaration` and compares that declaration's own name, so `namespace A { namespace B { } }` is presumably compared as `B` (code reading, not run). No spec decides the nested case. Outside the run's issues; an `agent-filed` follow-up carrying the question, with today's output established by a test first.

## thoenissen/Reihitsu#1268

### Premise

Real, through the analyzer. Observed: the existing characterization `RH2003NotImplementedExceptionShouldNotBeUsedAnalyzerTests.VerifyCreationThroughUsingAliasIsNotReported` passes on this commit (`throw new NIE()` with `using NIE = System.NotImplementedException;` is not reported, while `System.NotImplementedException` is). Cause in the code: the syntactic pre-filter `GetSimpleTypeName(objectCreation.Type) != "NotImplementedException"` (`RH2003NotImplementedExceptionShouldNotBeUsedAnalyzer.cs`, `OnObjectCreationExpression`) returns before the semantic check.

### Mechanism and candidates

Mechanism: the explicit-creation path rejects a creation unless the rightmost identifier as written is `NotImplementedException`, so a type name whose written identifier differs from the type's name never reaches the semantic check.

Dispatch: `RegisterSyntaxNodeAction` on `ObjectCreationExpression` (pre-filtered) and `ImplicitObjectCreationExpression` (semantic only, unaffected). Candidates by `ObjectCreationExpressionSyntax.Type` kind:

| Type syntax | Example | Today |
|---|---|---|
| `IdentifierNameSyntax` naming an alias of the type | `new NIE()` (also `@NIE`, a `global using` alias in another file, an alias declared inside a namespace block) | not reported — fails |
| `QualifiedNameSyntax` through a namespace alias | `new S.NotImplementedException()` | reported (existing test `VerifyCreationThroughNamespaceAliasIsReportedLikeTheTypeName`) — holds |
| `AliasQualifiedNameSyntax` | `new S::NotImplementedException()`, `new global::System.NotImplementedException()` | passes the pre-filter by code reading; `S::` with a using alias is not covered by any test and was not compiled — the test implementer compiles the RH2003-001 example |
| `IdentifierNameSyntax` alias named `NotImplementedException` to another type | `using NotImplementedException = System.NotSupportedException;` | not reported (semantic check) — holds, RH2003-003 |

"RH2003 is checked for other syntactic pre-filters": there is exactly one (`GetSimpleTypeName`); the implicit path has none.

### Breaking inputs

FC-016 (sibling shapes: alias in a namespace block, `global using` alias, `@NIE`), FC-003 (alias to `NotImplementedException` reported; alias to another type not reported), FC-034 (the issue's own `throw new NIE();`). The existing characterization test must be turned into a requirement test, not left asserting the old output.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH2003 analyzer, explicit-creation path | must change |
| RH2003 analyzer, target-typed path | must stay |
| Code fix / Fix All | not involved (none exists) |
| `Reihitsu.Core`, formatter, CLI, rule page | not involved / must stay |
| Spec | RH2003-001/-003 already clarified by 575d1a7 |

### Refuse option

None applies: no code fix to withhold and no formatter path; a missed diagnostic against SHARED-DIAG-002 is not a refusable class. The requested change is a few lines. Note for cost: the pre-filter exists to avoid semantic binding on every object creation; the change trades some of that for correctness (internal).

### Output questions

_None._ The visible change (aliased creations become reported) is demanded by SHARED-DIAG-002's alias sentence.

### Internal choices

_None._

### Issue goals

All three goals stand ("alias and qualified forms are reported", "checked for other pre-filters", "each input has a regression test"). The qualified form already works; its test is a characterization-turned-requirement, not a reproduction.

### Architecture findings

_None._

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1289

### Premise

Real, through the analyzer on a `.razor` additional file. Observed: the three characterization tests `VerifyCodeBlockTextInCommentsAndStringsIsReported`, `VerifyCodeBlockTextInCSharpCommentAndStringLiteralsIsReported`, and `VerifyEscapedAndEmailLikeCodeBlockTextIsReported` pass on this commit, i.e. every instance of the issue is reported today. The cause is the analyzer's only decision: the regex `@code\s*\{` over the whole file text (`RH2109RazorCodeBlocksShouldNotBeUsedAnalyzer.cs`, `_codeBlockPattern`).

Razor facts, observed with a throw-away `Microsoft.NET.Sdk.Razor` project built with the local SDK (10.0.404) and deleted afterwards:

- `<div>@code { int X; }</div>` fails to compile: `RZ2005: The 'code' directive must appear at the start of the line.`
- `<div>` / `    @code { int Z; }` / `</div>` compiles: a directive that starts a line, after white space, inside an element is a directive.
- A `@code` block holding `// @code {`, `string s = "@code {";`, a raw string with a line `@code {`, and a `/* */` comment with a line `@code {` compiles; so do a multi-line `@* … *@` around a whole `@code { … }` block, `user@code{`, and `@@code { }`. All of these are real, buildable inputs that RH2109 reports today.

### Mechanism and candidates

Mechanism: the analyzer reports every occurrence of the character sequence `@code`, white space, `{` anywhere in the file, without deciding whether that `@` is a Razor transition at the start of a line in markup, so every occurrence inside Razor text (SHARED-DIAG-003) is reported.

Dispatch: no syntax tree; `OnCompilation` iterates `AdditionalFiles` ending in `.razor` and runs the regex over `SourceText.ToString()`.

### Breaking inputs

Failure classes: FC-022 (raw, verbatim, interpolated literals; comment-looking text in literals), FC-027 (the multi-line form, and other enclosing C# scopes), FC-003 (each text kind next to a real directive that stays reported), FC-034 (the issue's own shapes), FC-033 (the test's raw-string fixture must keep the `@code {` line at the column the case needs).

Class matrix, axes:

1. Text kind (SHARED-DIAG-003): Razor comment `@* *@`; HTML comment `<!-- -->`; escape `@@`; `@` after a letter or digit (`user@code{`, `a1@code{`); C# `//` (and `///`); C# `/* */` (and `/** */`); C# regular, verbatim, raw, interpolated (text part), UTF-8 string. A char literal cannot hold `@code`.
2. Line position of the `@code {` inside the text: on the line where the text opens (mid-line), or starting its own line inside multi-line text (Razor comment, HTML comment, `/* */`, verbatim string, raw string, multi-line interpolated verbatim/raw).
3. Host of the C# kinds: `@code { }` block; `@functions { }` block; `@{ }` block; control-flow block (`@if (…) { }`); explicit expression `@( … )`; implicit expression `@M("…")`.

Every cell where the text contains `@code`, optional white space, and `{` fails today (one regex, no context). Cells that hold today and must stay: a real directive at the start of a line, after optional white space, in markup (also inside an element), with the brace on the same or the next line; `@code` text without a following `{`.

Commonness: the multi-line Razor comment around a whole `@code` block (axis 1 Razor comment × axis 2 own line) is the one member people commonly write — commenting out a code block. It must be covered. The C# kinds in hosts other than a `@code` block (axis 3 beyond the first) are rare.

Inputs the test implementer must cover, at least: SHARED-DIAG-003's last example (`Counter.razor`), the three characterization inputs turned around, the multi-line `@* … *@` around a `@code { … }` block next to a real directive, a multi-line `<!-- … -->` holding a `@code {` line, and a `@code {` line inside a multi-line `/* */` and inside a raw string within a `@code` block (each with a real directive in the same file that stays reported, and the expected spans of the real ones).

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH2109 analyzer | must change |
| Code fix / Fix All | not involved (none exists) |
| `Reihitsu.Core`, formatter, CLI | not involved |
| Rule page RH2109 | must stay (comment and escape behavior is general-notes material) |
| Spec | RH2109-001 coverage sentence clarified by 575d1a7; enough as is |
| `@functions` | not in this run (thoenissen/Reihitsu#1452); the characterization `VerifyFunctionsBlockIsNotReported` must stay |

### Refuse option

There is no fix to withhold and no formatter construct; the refuse option of `AGENTS.md` does not exist for this analyzer, and not reporting a real directive would be a new defect. No unsupported-shape option: no formatter path.

Cost, which the lead should weigh against *Scope*: deciding axis 1 for Razor and HTML comments, `@@`, and letter-before-`@` needs a scan that tracks comment state across lines — small. Deciding the C# kinds needs to know where C# blocks begin and end (brace matching that skips braces inside C# strings, chars, and comments) and to lex C# comments and literals — a small Razor/C# scanner, plausibly more than 100 production lines or a new type. A narrower option that still meets the issue's instances and the common member: decide Razor/HTML comments, `@@`, and letter-before-`@` everywhere, and the C# kinds inside `@code` (and `@functions`) blocks only; the C# kinds in `@{ }`, control-flow, and expression hosts stay reported and become a follow-up. Which part is in scope is the lead's decision; flagging that the issue's "within a code block" wording hides the block-extent problem.

### Output questions

_None for the issue's cases_: SHARED-DIAG-003 decides every text cell.

### Internal choices

- Constraint for whoever implements: a `@code {` in the middle of a line that is not text (`<div>@code { int X; }</div>`) is reported today and is undecided by RH2109-001's coverage sentence. An implementation that only looks at `@code` at the start of a line would stop reporting it — a visible change nobody decided. Either the implementation keeps that report, or the lead asks the question below (see Follow-up candidates) in this batch. Recommend asking it now, because its answer decides how much of the scanner is needed: with "not reported", every mid-line case needs no classification at all.

### Issue goals

"Each input has a regression test" — keep, and add the multi-line Razor comment around a code block, which the issue does not list but is the common member of its class.

### Architecture findings

_None._

### Follow-up candidates

**RH2109 · Razor @code blocks should not be used — `@code` in the middle of a line, outside any comment or literal**

Input — `Counter.razor`:

```razor
<div>@code { int X; }</div>
```

Before (`main`): reported on `@code` (the regex matches anywhere). The Razor compiler rejects the file with RZ2005 ("The 'code' directive must appear at the start of the line").

After, A (recommended): not reported — it is not a directive.

After, B: reported, as today.

Why A: Razor itself accepts `@code` only at the start of a line, so the rule's subject never occurs mid-line; the file already fails to build with its own error.

(Outside the run's issues, so a follow-up by the README — unless the lead pulls it into this batch per the internal choice above.)

## thoenissen/Reihitsu#974

### Premise

Real by the code region, through the analyzer on any compilation with a `reihitsu.json` additional file: `ConfigurationManager.ParseConfiguration` (`src/Reihitsu.Analyzer/Data/ConfigurationManager.cs:127-131`) breaks out of its loop at the root object's `EndObject` and never reads again, so trailing content never reaches `Utf8JsonReader`. Not executed: no repository entry point other than a unit test runs RH0001 with an additional file, and no existing test covers trailing content; the test implementer's reproduction settles it. The CLI does not load `reihitsu.json` (no reference to `ConfigurationManager` or `reihitsu.json` under `src/Reihitsu.Cli` or `src/Reihitsu.Formatter`); the consumers are RH0001, RH4009, RH8402 and the RH8402 fix. The maintainer decided the case as a bug in thoenissen/Reihitsu#932 (Q1): trailing content is an RH0001 syntax error "like every other invalid JSON", a trailing comment included.

### Mechanism and candidates

Mechanism: the loader validates the root value and stops, so whatever follows the root value is never read; only an error before the root value ends throws.

Candidates (`ParseConfiguration`'s exits after the root value is read):

| Exit | Root | Today with trailing content |
|---|---|---|
| loop break at `EndObject` | object | nothing about the trailing content; the root's own problems reported; configuration applied (RH4009, RH8402 active when no other problem) |
| early return after "The configuration root must be a JSON object." | array, string, number, `true`/`false`/`null` | "root must be a JSON object" reported, trailing content ignored (code reading: `GetCurrentValueSpan` parses only the root value) |

### Breaking inputs

Class matrix, axes: root kind {object, array, primitive} × trailing content {token (`junk`), second value (`{ }`), `//` comment, `/* */` comment, white space that is not JSON white space (e.g. U+00A0)} × root has another problem {no, yes (unknown section / not an object)}. Cells with JSON white space only (`{ }` + spaces, tabs, CR, LF) hold and must stay valid. All other cells fail today. The non-object-root cells are rare, but RH0001-003 ("A file that cannot be parsed MUST NOT be validated further") decides them the same way: `[ ] junk` must report the syntax error at `j`, not "root must be a JSON object". FC-003: one test with trailing white space only, one with each trailing kind.

Positions in the RH0001-003 example (`j`, second `{`, `/`) are the spec author's claims about `Utf8JsonReader`; not run — the test asserts them and the implementer's result decides.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `ConfigurationManager` | must change |
| RH0001 analyzer | must stay (reports whatever the loader returns) |
| RH4009 analyzer | output depends on the output question |
| RH8402 analyzer and fix | must stay; RH8402-001 already makes it inactive on any problem |
| CLI, formatter, `Reihitsu.Core` | not involved |
| Rule pages | must stay |
| Specs | RH0001-003 clarified; RH4009-001/-002 wait for the answer |

### Refuse option

None applies: no fix, no formatter. The requested change is small (one more read after the root). No unsupported-shape option.

### Output questions

The spec author's question stands, but I would narrow it. RH0001's half is already decided by the unchanged RH0001-003 — its title ("An empty or malformed file is one problem"), "A file that cannot be parsed MUST NOT be validated further", and the clarified "not valid JSON … anything other than white space after the root value", which match the maintainer's "like every other invalid JSON". Under the spec author's option B, RH0001 would report two problems for one malformed file, which contradicts RH0001-003. RH8402 is decided (inactive on any problem). What no requirement decides is RH4009: RH4009-001 silences it only for JSON "malformed before its root value ends", a qualifier written to leave this case open. So the real choice is:

**RH4009 · Namespace not allowed — `reihitsu.json` with content after its root object**

Input — `reihitsu.json`:

```json
{ "naming": { "allowedNamespaceDeclarations": [ "A" ] } } junk
```

and

```csharp
namespace B
{
}
```

Before (`main`): RH0001 reports nothing; `namespace {|RH4009:B|}` is reported (by the code region; not run).

After, A (recommended): RH0001 reports invalid JSON syntax at the `j`; `namespace B` is not reported, like with every other malformed file.

After, C: RH0001 reports invalid JSON syntax at the `j`; `namespace {|RH4009:B|}` is still reported from the root object's list.

Why A: every other malformed file turns RH4009 off, and C would apply a configuration that RH0001 calls unparsable.

If the lead keeps the spec author's B, I recommend A over it for the same reason, plus RH0001-003's "one problem".

### Internal choices

- RH0001-003's clarification says "anything other than white space"; the normative test is "not valid JSON", whose white space is only space, tab, line feed, and carriage return. A trailing U+00A0 is therefore invalid JSON. Recommend wording it "JSON white space" so it cannot be read as Unicode white space; no output depends on the wording once the normative sentence decides it. (Note: the empty-file check uses `string.IsNullOrWhiteSpace`, i.e. Unicode white space; a file of only U+00A0 is reported as empty, not as invalid JSON. Pre-existing, outside the issue, not a candidate worth a follow-up.)

### Issue goals

"Check that the CLI's own configuration loading behaves the same way" — drop: the CLI loads no `reihitsu.json` (its spec lists it as a non-goal). No visible effect.

### Architecture findings

_None._

### Follow-up candidates

_None._
