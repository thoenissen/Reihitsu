Role: skeptic
Commit: eaa17de
Verdict: #353: output question; #354: output question

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Both issues are `agent-filed`, so their goals are proposals (`squad/README.md`, *Issues are hypotheses*).

## thoenissen/Reihitsu#353

### Premise

Real. No rule measures the size of a member today: `src/Reihitsu.Analyzer/Rules/Design/` holds RH2001 to RH2007 and RH2101 to RH2110, none of which counts lines, and `rg "RH2008|RH2111"` finds the IDs only in this run's spec diff. The caller is the analyzer host itself: every compilation that references the package and configures a limit reaches the rule. The CLI and the formatter are not involved.

Statements of the issue that are stale or wrong against the repository:

- "Configuration key shape should follow existing analyzer options (`dotnet_diagnostic.RH####.<option> = <value>`)" — no such option exists. `rg "AnalyzerConfigOptions|GetOptions\("` over `src/Reihitsu.Analyzer` and `src/Reihitsu.Core` finds nothing; every Reihitsu setting is read from `reihitsu.json` through `ConfigurationManager` (RH4009, RH8402), and RH0001's non-goal states that `.editorconfig` configures severities, not Reihitsu's own settings. The spec's choice of `reihitsu.json` (output question 1, A) follows the repository; the issue's key shape follows nothing in it.
- "alongside the cognitive-complexity rule" — that rule does not exist; it is thoenissen/Reihitsu#352 (open, `agent-filed`, same milestone 1.5.0). It will need a limit of its own; a `design` section with one setting per measure (`maximumMethodLines`, `maximumTypeLines`) leaves room for it. Nothing to change in this run.

Claims of the spec author I checked against the code:

- RH4009 keeps working when RH0001 reports a problem elsewhere: `RH4009…Analyzer.OnCompilationStart` reads `.Configuration` and registers when `AllowedNamespaceDeclarations?.Count > 0`, without looking at `Errors`. Confirmed.
- RH8402 is silenced by any problem: `RH8402…Analyzer.cs:73` returns when `configurationLoadResult.Errors.IsDefaultOrEmpty == false`. Confirmed, so the change doc's visible output change 4 is real.
- An unknown section is reported today: `ConfigurationManager.ParseConfiguration`, `default:` branch, "Unknown configuration section '…'.". Confirmed, so the change doc's *Before* lines for a `design` section are right. No existing test uses a `design` section (`rg "'design'|\"design\"" src --glob '*.cs'` is empty), so RH0001-004's change breaks no test.
- The location choices of RH2008-005 (operator token, `operator` keyword of a conversion operator, `this` of an indexer, identifier otherwise) are the repository's existing convention: `OrderingDeclarationUtilities` (`src/Reihitsu.Core/OrderingDeclarationUtilities.cs:160-175`), `DocumentationAnalysisUtilities.cs:201-202`, and RH2004 (`RH2004AccessModifierMustBeDeclaredAnalyzer.cs:106-107`) place theirs the same way.
- SHARED-DIAG-012 applies to RH2008-004 as cited: the rule judges only the part with a body, which is the only part it would report judged alone (item 2).

Every count stated in the spec examples, the change doc, the rule pages, and the drafted output questions I recounted by hand under SHARED-LINES-001; all agree. I did not compile them again; the spec author did.

**Formatter interaction (no conflict, but say so in the spec).** The count follows the text as it is laid out, so formatting can raise it. Fixture, `scripts/format.sh <dir>` on `Fixture.cs`:

```csharp
namespace Sample;

public class C
{
    private int _a, _b, _c;

    [System.Obsolete, System.CLSCompliant(false)]
    public void M()
    {
        if (_a > 0) { _b = 1; }
        _c = 2;
    }
}
```

Observed output:

```csharp
namespace Sample;

public class C
{
    private int _a;
    private int _b;
    private int _c;

    [System.Obsolete]
    [System.CLSCompliant(false)]
    public void M()
    {
        if (_a > 0)
        {
            _b = 1;
        }

        _c = 2;
    }
}
```

Under SHARED-LINES-001 the body of `M` holds 2 lines of code before and 5 after; the body of `C` holds 7 before and 13 after. With `maximumMethodLines` 4, `M` is clean before formatting and reported after it. Classification: not FC-013 — SHARED-DIAG-010 covers layout rules whose code fix lays code out as the formatter does, and RH2008/RH2111 judge size and have no fix; not FC-014 — the concern is size, which no formatter phase judges, and the remedy is splitting the declaration, not a layout; not P11 — the formatter does not rewrite a construct the rule names. The rule never asks for a layout the formatter refuses, so I see no disagreement under `sdd/README.md` *Open questions*. The field and the attribute list of the input above are reported by RH7101 (Do not combine fields) and RH5514 (Method attribute lists must use one attribute per list), so for those shapes code that is clean under the other rules keeps its count when formatted; I did not check which rule reports the one-line `if` block. See *Internal choices* for the spec wording.

### Mechanism and candidates

_N/A_ — a new rule, not a bug. The dispatch the implementer has to cover, from the syntax type hierarchy rather than the issue text: `MethodDeclarationSyntax`, `ConstructorDeclarationSyntax`, `DestructorDeclarationSyntax`, `OperatorDeclarationSyntax`, `ConversionOperatorDeclarationSyntax`, `LocalFunctionStatementSyntax`, `AccessorDeclarationSyntax` (`get`, `set`, `init`, `add`, `remove`), `PropertyDeclarationSyntax` and `IndexerDeclarationSyntax` with an `ExpressionBody` (accessor list absent). Every one exists as a block-bodied and an expression-bodied form. Members inside an extension block are members like any other. Excluded by the spec: `EventFieldDeclarationSyntax`, abstract/extern/interface members without a body, partial defining parts, auto-accessors, anonymous functions on their own, initializers, top-level statements.

### Breaking inputs

Failure classes that reach a counting analyzer:

- **FC-003 (boundary on one side only)** — every limit test needs the count equal to the limit (not reported) and one above (reported). The spec examples already pair them (SHARED-LINES-002, RH2008-003 with 3 and 2).
- **FC-020 / FC-021 / FC-019 (directives and comments in the counted span)** — each kind of trivia alone on a line, and next to a token on a line: `//` line, `/* */` over several lines between two tokens of one statement, `///` before a local function, `#region`/`#endregion`, `#pragma warning`, `#if` with the excluded branch both inactive by an undefined symbol and as `#if false`, `#else` branch active. A line with a token and an end-of-line comment counts; a line inside a multi-line `/* */` that holds no token does not.
- **FC-022 (literals)** — a verbatim string over several lines, a raw string literal with a blank line inside it (each line counts, the blank one included), an interpolated raw string whose hole spans lines, and comment-looking text such as `"// x"` or `"#if"` inside a literal (counted as code).
- **FC-024 (partial, explicit implementation)** — partial method, partial property with an expression body in the implementing part, partial constructor, partial event (defining part not reported, implementing part reported on its name); an explicit interface implementation `void I.M()` (located on `M`) and an explicit interface operator.
- **FC-025 (expression-bodied kinds)** — each of the seven expression-bodied member kinds plus expression-bodied accessors (`get => …`), with the `=>` on the signature line and on a line of its own; the `;` alone on its line must not add a line (RH2008-002).
- **FC-027 (nesting of the same kind)** — a local function in a local function (counts toward both containers, each judged), a lambda in a lambda, a local function inside a lambda inside a member, a local function in an accessor, a local function in top-level statements (judged on its own; RH2008-002 says "every local function").
- **FC-023 (identifiers)** — a verbatim identifier (`void @class()`) locates the diagnostic on the whole identifier token.
- **FC-033 (line endings)** — the same input under LF and CRLF must give the same count; a lone `\r` too, if the test helpers allow it.
- **FC-040 (documentation mode)** — a `///` line inside a body is structured trivia with documentation parsing and a plain comment without it; either way it is trivia, so the count must not change between the two modes.

No class has a matrix in `squad/failure-classes.md` that applies; there is no defect to enumerate.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH2008 analyzer (new) | Must change |
| `ConfigurationManager` and the configuration model (`design` section) | Must change (shared with #354) |
| RH0001 analyzer | Must stay in code; its output changes through the loader (RH0001-004, RH0001-007) |
| RH8402 analyzer | Must stay in code; its output changes, because a valid `design` section is no longer an error (change doc change 4) |
| RH4009 analyzer | Must stay |
| Code fix, Fix All | Not involved (no fix) |
| Formatter, CLI | Not involved |
| `Reihitsu.Core` | Not involved, unless the lead places a shared counting helper there (internal choice) |
| Rule page `documentation/rules/RH2008.md`, package README row | Must change (written) |
| `Release.ruleset`, `Reihitsu.sln` solution items, `AnalyzerResources.resx`/`.cs` | Must change (every Design rule is listed there; `rg -l RH2007`) |
| `RuleDocumentationExampleOptOuts`, `AnalyzerPackageMetadataTests` message-format list | Must change (spec author's handover) |
| Analyzer tests, RH0001 tests | Must change |

### Refuse option

The refuse option for a new rule is not to add it: close the issue with `Refs #353` and no diagnostic. Its cost is zero production lines; the requested change costs one analyzer, one configuration section in the loader with validation, a counting helper (or two copies, see #354), resources, a rule page, and tests — no new mechanism in the formatter or a code fix, and no special case for a rare input class. Nothing in the run is withheld for a rare class, because there is no fix. Declaring a shape unsupported is not an option: no formatter phase or code fix is involved. No simpler option: a narrower rule (methods only, no accessors) saves almost nothing, since the counting is the same for every kind.

### Output questions

The spec author's questions 1 to 4 are sound and should be asked as drafted, with these amendments; questions 6 to 8 below are missing from the draft.

- **Question 1, A** — add the RH8402 consequence (change doc change 4) as one line under *After, A*: "A valid `design` section next to a `copyright` section no longer turns the copyright-header check (RH8402) off." It follows from A, but it is a visible change and is listed in the change doc, so it must reach the maintainer and should not ride on the answer silently.
- **Question 4, A** — the reason "RH21xx holds type declarations (RH2101 to RH2110)" is loose: RH2109 is "Razor @code blocks should not be used". Say "RH21xx holds the rules about type declarations and their nesting". The question should also show one diagnostic span that is not a name, so the maintainer sees the location rule for unnamed members, e.g. `public static C operator {|RH2008:+|}(C left, C right)` and `{|RH2008:get|}`.

**6 · RH2008 · Methods should not exceed the line limit — the accessors of one property**

Input, with `{ "design": { "maximumMethodLines": 4 } }`:
```csharp
class C
{
    private int _value;

    public int Value
    {
        get
        {
            var result = _value;
            result += 1;
            return result;
        }
        set
        {
            var input = value;
            input -= 1;
            _value = input;
        }
    }
}
```

Before (`main`): not reported.

After, A (recommended): not reported; each accessor holds three lines of code.
```csharp
class C
{
    private int _value;

    public int Value
    {
        get
        {
            var result = _value;
            result += 1;
            return result;
        }
        set
        {
            var input = value;
            input -= 1;
            _value = input;
        }
    }
}
```

After, B: the accessors of a property, indexer, or event are counted together, six lines, and the property is reported on its name.
```csharp
class C
{
    private int _value;

    public int {|RH2008:Value|}
    {
        get
        {
            var result = _value;
            result += 1;
            return result;
        }
        set
        {
            var input = value;
            input -= 1;
            _value = input;
        }
    }
}
```

Why A: each accessor is a method of its own that a reader enters separately, and the issue names the accessors, not the property, as the members to judge.

**7 · RH2008 · Methods should not exceed the line limit — a lambda in a field initializer**

Input, with `{ "design": { "maximumMethodLines": 2 } }`:
```csharp
using System;

class C
{
    private static readonly Func<int, int> Compute = value =>
    {
        var result = value;
        result += 1;
        return result;
    };
}
```

Before (`main`): not reported.

After, A (recommended): not reported. A lambda is judged only as part of the member that holds it, and an initializer is not a member body.
```csharp
using System;

class C
{
    private static readonly Func<int, int> Compute = value =>
    {
        var result = value;
        result += 1;
        return result;
    };
}
```

After, B: a lambda or anonymous method outside every judged body is judged on its own and reported on the name of the field or property it initializes.
```csharp
using System;

class C
{
    private static readonly Func<int, int> {|RH2008:Compute|} = value =>
    {
        var result = value;
        result += 1;
        return result;
    };
}
```

Why A: the issue names no lambdas, long lambdas in initializers are rare, and the type limit still counts their lines; B judges a construct that has no name of its own. (The cost of A: moving a method's code into an initializer lambda escapes the method limit — the same argument the spec uses for question 3.)

**8 · RH0001 · reihitsu.json configuration must be valid — a limit that is not a whole number**

Input, `reihitsu.json`:
```json
{ "design": { "maximumMethodLines": 50.0, "maximumTypeLines": "500", "maximumFileLines": 900 } }
```

Before (`main`): "Unknown configuration section 'design'." on `"design"`.

After, A (recommended): "The 'design.maximumMethodLines' setting must be a whole number from 1 to 2147483647." on `50.0`, the same for `maximumTypeLines` on `"500"`, and "Unknown configuration setting 'design.maximumFileLines'." on `"maximumFileLines"`. Neither rule is active.

After, B: any JSON number with a whole value is accepted (`50.0`, `5e1`); `"500"` and the unknown setting are reported as in A, and the method limit is 50.

Why A: one way to write a limit, and the message names exactly what is accepted.

### Internal choices

- **State that the count follows the layout.** Add one sentence to SHARED-LINES-001's `Why:` or the change doc's *What a line of code is*: the lines are those of the text as written, so laying code out differently — on one line or over several — changes the count, and a project sets its limit against code laid out by its own style. Without it, the next role or reviewer will rediscover the formatter interaction above and raise it as an analyzer/formatter disagreement. Recommended.
- **Name the line split.** SHARED-LINES-001 says "the lines of the text"; state that a line is a line of the source text as the compiler splits it, so CRLF, LF, and a lone CR give the same count (FC-033). Recommended; no visible difference for any input a test writes today.
- **One counting owner.** RH2008 and RH2111 apply the same definition; one helper is SHARED-LINES' code owner, and two private copies would be FC-015 from the start. The lead's call; recommended one owner.

### Issue goals

- "Let the limit live in `.editorconfig`" / "Configuration key shape should follow existing analyzer options" — drop: based on a convention the repository does not have (see *Premise*). Visible; question 1.
- "method-shaped member: constructors, operators, local functions, property accessors" — the spec widens to finalizers, conversion operators, event accessors, and expression-bodied properties and indexers. Each is a method under another syntax, so the widening serves the issue's purpose. Visible; I recommend the lead show it in question 4's input or name it in question 1's text rather than ask it separately.
- "The exact definition … should be documented in the rule doc" — change: rule pages stay short and carry no corner cases (`AGENTS.md`, *Key conventions*); the exact definition is SHARED-LINES-001, and the page states the essentials, as written. Not visible beyond the page as written.
- "message should report the measured line count and the configured limit" — kept.
- "the cognitive-complexity rule" — stale reference to an open issue (thoenissen/Reihitsu#352); no effect.

### Architecture findings

_None._ The spec states each rule once: the counting in SHARED-LINES, the validation in RH0001-007, referenced rather than restated by RH2008-001 and SHARED-LINES-002.

### Follow-up candidates

- **RH0001 · a setting written twice in one section.** `{ "naming": { "allowedNamespaceDeclarations": ["A"], "allowedNamespaceDeclarations": ["B"] } }` — from reading `ConfigurationManager.ParseNaming`/`ParseAllowedNamespaceDeclarations`, the later value replaces the earlier one without a report; I did not run it. RH0001's spec does not decide duplicate keys for any section, and the new `design` section inherits the gap. Outside the run's issues; an `agent-filed` follow-up that asks whether RH0001 reports the second occurrence. Low value; the lead may drop it.

## thoenissen/Reihitsu#354

### Premise

Real, on the same evidence as #353: no rule judges the size of a type. The caller is the analyzer host. The `.editorconfig` key-shape claim is stale in the same way. "Aggregating across files would require cross-file analysis the analyzer surface does not currently provide" is wrong as a reason — a symbol's `DeclaringSyntaxReferences` give every part, and other rules judge partial types by their symbol (SHARED-DIAG-004) — but the conclusion stands for the issue's other reason: a reader opens one part, and only per-part judging gives a place to act (RH2111-004's `Why:`). The formatter interaction under #353 applies here too, more strongly: field splitting and one attribute per list add lines to a type body (fixture under #353: 7 lines before, 13 after).

### Mechanism and candidates

_N/A_ — a new rule. Dispatch: `ClassDeclarationSyntax`, `StructDeclarationSyntax`, `InterfaceDeclarationSyntax`, `RecordDeclarationSyntax` (`record`, `record class`, `record struct`), and the union declaration of C# 15, each with a brace body or with `;` instead. Excluded by the spec: `EnumDeclarationSyntax`, `DelegateDeclarationSyntax`, extension blocks (counted toward their type, not judged).

### Breaking inputs

- **FC-003** — a body at the limit and one above (RH2111-002's `S` and `C`).
- **FC-020 / FC-021** — `#region`/`#endregion` around members, `///` and `/** */` documentation, `#if` around a member with the member inactive and active, `#pragma` between members. A member whose attribute list stands on its own line counts that line.
- **FC-022** — a field initialized with a multi-line raw string: every line of the literal counts toward the type.
- **FC-024** — partial type with parts in one file and in two files; a part in generated code is not analyzed (SHARED-DIAG-001) while the other part still is; a `file` type.
- **FC-027** — a type nested two levels deep (each container counts it, each level judged); a nested type inside a partial part; an extension block with members (counted toward the type, never reported itself).
- **RH2111-005** — `record R(int X);`, `class C;`, `record struct S(int X);`, and a positional record with a body (only the body counts, the parameter list over several lines does not).
- **FC-033, FC-040** — as under #353.
- Type and member in one file: one RH2111 and several RH2008 diagnostics side by side, no interference.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH2111 analyzer (new) | Must change |
| `ConfigurationManager`, RH0001, RH8402, RH4009 | As under #353 (one shared change) |
| Code fix, Fix All, formatter, CLI | Not involved |
| Rule page `documentation/rules/RH2111.md`, package README row, `Release.ruleset`, `Reihitsu.sln`, resources, opt-out and metadata test lists | Must change |
| RH2101, RH2104 to RH2108, RH2110 (nested types) | Must stay; they report nesting independently of size |

### Refuse option

As under #353: not adding the rule costs nothing and leaves the issue open with `Refs #354`. No fix, no formatter phase, no unsupported shape. No simpler option.

### Output questions

Questions 1 to 4 under #353 (with the amendments there), question 5 of the spec author's draft as drafted, and question 8 under #353 apply. Question 5 is sound: the union reading follows `union-analyzer-rules.md`, and its *Before*, *After*, and counts are right.

### Internal choices

- The spec excludes documentation comments and every directive by one cause (trivia), wider than the issue's "region directives". Recommended as written: it is the issue's "comment-only lines" and "region directives" stated by their cause (refuse by the cause, not by the cells).

### Issue goals

- `.editorconfig` key shape — drop, as under #353; visible, question 1.
- Kinds "class, struct, record, record struct, interface" — widened to unions; visible, question 5.
- "partial: evaluated per declaration" — kept, with the issue's cross-file reason corrected (see *Premise*); not visible.
- "exact definition documented in the rule doc" — changed as under #353.

### Architecture findings

_None._

### Follow-up candidates

_None_ beyond the one under #353.

## Whole run

- Formatter fixture run: `scripts/format.sh` on a scratchpad directory holding `Fixture.cs` (quoted under #353), one pass; output quoted above. The fixture directory was deleted afterwards. No other command was run; no test was run, since neither rule exists.
