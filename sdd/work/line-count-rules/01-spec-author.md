Role: spec author
Commit: faf4794
Verdict: #353: change doc written; #354: change doc written

Both issues share one change doc, `sdd/changes/2026-11/line-count-rules.md` (Status: Draft), and one shared spec for the counting, `sdd/specs/shared/line-count.md`. The prompt carried no intended fix or verdict.

Files written:

- `sdd/changes/2026-11/line-count-rules.md` — change doc for both issues.
- `sdd/specs/shared/line-count.md` — new shared spec (SHARED-LINES), code owners `—`.
- `sdd/specs/analyzer/RH2008.md`, `sdd/specs/analyzer/RH2111.md` — new rule specs.
- `sdd/specs/analyzer/RH0001.md` — RH0001-004 changed, RH0001-007 added; intro and non-goal name the new readers.
- `sdd/specs/README.md`, `sdd/specs/requirement-ids.txt` — rows and IDs.
- `documentation/rules/RH2008.md`, `documentation/rules/RH2111.md` — new rule pages (UTF-8 with BOM, like the others), each with a `Configuration` section modeled on RH4009's page.
- `src/Reihitsu.Analyzer.Package/README.MD` — one row each under Design. The titles there must match the descriptor titles (`AnalyzerMetadataDiscovery` reads this file).

`documentation/rules/RH0001.md` is left unchanged: its text is generic ("only known configuration sections"), and its example stays valid.

## thoenissen/Reihitsu#353

### Changed requirements

- RH2008-001 — The limit is `design.maximumMethodLines` — added
- RH2008-002 — A member with a body is judged by the lines of its body — added
- RH2008-003 — Nested functions count toward the member that holds them — added
- RH2008-004 — A member without a body is not reported — added
- RH2008-005 — One diagnostic per member, located on its name — added
- SHARED-LINES-001 — A line of code holds a character of a token of the counted span — added (shared with #354)
- SHARED-LINES-002 — A declaration is reported when its count is greater than its limit — added (shared with #354)
- RH0001-004 — The root is an object of known sections — changed (`design` is a known section; shared with #354)
- RH0001-007 — The `design` section holds the line limits — added (shared with #354)

### Examples for the tests

SHARED-LINES-001, SHARED-LINES-002, RH2008-002, RH2008-003, RH2008-004, RH2008-005, RH0001-007. RH2008-001 has no example of its own; its tests are the SHARED-LINES-002 cases (missing file, empty section, `0`, `"2"`, a problem in another section).

Every C# example of the new specs and the change doc compiles (all 15 blocks, markup removed, built as one net11.0 library with `LangVersion` preview in the scratchpad). None was run through an analyzer, because neither rule exists yet.

Facts the test implementer needs:

- The analyzer tests need a `reihitsu.json` additional file. RH4009's and RH8402's tests show how the existing tests pass one.
- `RuleDocumentationExampleTests` runs every rule page's Violation/Correction example without a `reihitsu.json`. RH4009 and RH8402 are opted out in `RuleDocumentationExampleOptOuts` for that reason, and RH2008 and RH2111 need the same opt-out. Their page examples also use types that are not declared (`Order`, `Address`, `_repository`).
- `AnalyzerPackageMetadataTests` keeps a list of rules whose message format differs from the title. The messages proposed in the change doc ("This member has {0} lines of code, which exceeds the limit of {1}." and "This type has {0} lines of code, which exceeds the limit of {1}.") differ from the titles.
- `Release.ruleset` and the `Reihitsu.sln` solution items list every rule and rule page. This role does not edit them.

### Output questions

These are drafted in the form of `AGENTS.md` *Output questions*. The specs are written on the recommended options. Questions 1 to 4 apply to both issues, and question 5 applies only to #354.

**1 · RH2008 / RH2111 · where the limit is configured**

Input:
```csharp
class C
{
    void M()
    {
        M();
        M();
        M();
        M();
    }
}
```

Before (`main`): not reported, and no line limit can be configured anywhere.

After, A (recommended): `reihitsu.json` holds `{ "design": { "maximumMethodLines": 3, "maximumTypeLines": 500 } }`. RH0001 validates it.
```csharp
class C
{
    void {|RH2008:M|}()
    {
        M();
        M();
        M();
        M();
    }
}
```

After, B: `.editorconfig` holds `dotnet_diagnostic.RH2008.maximum_lines = 3` and `dotnet_diagnostic.RH2111.maximum_lines = 500`. The diagnostic is the same as in A. A mistyped key or a value like `abc` is ignored silently, and the limit can differ per folder.

Why A: every Reihitsu setting lives in `reihitsu.json`, where RH0001 reports typos and invalid values. Today `.editorconfig` only sets severities.

**2 · RH2008 / RH2111 · when no limit is configured**

Input: the input of question 1, with no `reihitsu.json` (or one without `design`).

Before (`main`): not reported.

After, A (recommended): not reported. Both rules stay inactive until a limit is configured, as RH4009 does without its namespace list.

After, B: built-in defaults apply, for example 50 lines for a method body and 500 for a type body. Here that gives no RH2008, but every project that updates the package gets warnings for any method over 50 lines.

After, C: the rules are off by default (rule page row "Enabled by default ❌"). Once a project enables them in `.editorconfig`, defaults of 50 and 500 apply unless `reihitsu.json` sets other limits.

Why A: no limit suits every project. A or C keep a package update from producing new warnings or breaking `TreatWarningsAsErrors` builds, and A needs one step to turn on instead of two.

**3 · RH2008 / RH2111 · lines of nested local functions, lambdas, and types**

Input, with `{ "design": { "maximumMethodLines": 3, "maximumTypeLines": 4 } }`:
```csharp
class Outer
{
    void M()
    {
        Local();

        void Local()
        {
            M();
        }
    }
}
```

Before (`main`): not reported.

After, A (recommended): `M`'s body holds five lines of code (`Local();`, `void Local()`, its two braces, `M();`). `Outer`'s body holds eight.
```csharp
class {|RH2111:Outer|}
{
    void {|RH2008:M|}()
    {
        Local();

        void Local()
        {
            M();
        }
    }
}
```

After, B: nested local functions, lambdas, and nested types are left out of their container's count. `M` holds one line, `Local` one, and `Outer` four: `void M()`, its two braces, and `Local();`. Nothing is reported, because a count equal to the limit is accepted.

Why A: a reader of `M` reads `Local` too. Under B, moving code into a local function or a nested type satisfies the limit without making the container shorter to read.

**4 · RH2008 / RH2111 · rule IDs, titles, and messages**

Input: the input of question 1, under the configuration of question 1A.

Before (`main`): not reported.

After, A (recommended): `RH2008 · Methods should not exceed the line limit`, message "This member has 4 lines of code, which exceeds the limit of 3." The type rule is `RH2111 · Types should not exceed the line limit`, message "This type has {count} lines of code, which exceeds the limit of {limit}." The IDs fit the ranges: RH20xx holds member-level design rules (RH2002 async void, RH2005 fields), and RH21xx holds type declarations (RH2101 to RH2110).

After, B: both in the type-declaration range, as RH2111 (methods) and RH2112 (types), with the same titles and messages.

Why A: it keeps member-level and type-level design rules in their existing ranges, and each message states the count and the limit, as both issues ask.

**5 · RH2111 · Types should not exceed the line limit — unions and enums**

Input, with `{ "design": { "maximumTypeLines": 2 } }`:
```csharp
union U(int, string)
{
    public int A => 1;
    public int B => 2;
    public int C => 3;
}

enum E
{
    A,
    B,
    C,
}
```

Before (`main`): not reported.

After, A (recommended): the union is reported, and the enum is not.
```csharp
union {|RH2111:U|}(int, string)
{
    public int A => 1;
    public int B => 2;
    public int C => 3;
}

enum E
{
    A,
    B,
    C,
}
```

After, B: only the kinds #354 names (class, struct, record, record struct, interface), so neither is reported.

After, C: the union and the enum are both reported.

Why A: a union body holds members like a struct's, and the other design rules already treat unions as type declarations. An enum body is a list of values, not code a reader works through.

### Internal choices

- One change doc for both issues, which share the mechanism. The counting definition and the comparison with the limit live once, in `shared/line-count.md`. The alternative was to restate them in both rule specs.
- `shared/line-count.md` lists no code owner (`—`) because no type implements it yet. If the implementer adds a shared counting helper, it becomes this spec's code owner, and the row in `sdd/specs/README.md` follows. The alternative was to name a type up front.
- The settings are named `design.maximumMethodLines` and `design.maximumTypeLines`, with `design` after the rules' category. A per-rule section such as `"RH2008": { "maximumLines": … }` was the alternative. The names are part of output question 1.
- The settings accept only a JSON whole number from 1 to 2147483647: no fraction, no exponent, and not `0`. The alternative was to accept any number and round it.
- A valid limit takes effect even when RH0001 reports a problem in another section, as RH4009 keeps working (SHARED-LINES-002). The alternative was RH8402's behavior, where any configuration problem silences the rule.
- RH2008 locates the diagnostic on the operator token for an operator, the `operator` keyword for a conversion operator, `this` for an indexer, and the keyword for an accessor. The alternative was the whole signature.
- An expression body counts the `=>` and the expression. The `;` behind it does not count. The alternative counted it, which adds a line only when the `;` stands alone.
- Each accessor is judged on its own, and a property's accessors are not summed (RH2008-004). Properties and indexers with an expression body are judged as a getter. Lambdas are not judged on their own (a non-goal).
- A partial member is judged on the part that holds its body, located per SHARED-DIAG-012.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#354

### Changed requirements

- RH2111-001 — The limit is `design.maximumTypeLines` — added
- RH2111-002 — A type is judged by the lines between its braces — added
- RH2111-003 — A nested type counts toward the type that holds it — added
- RH2111-004 — Each part of a partial type is judged on its own — added
- RH2111-005 — A type without a brace body is not reported — added
- RH2111-006 — One diagnostic per declaration, located on its name — added
- SHARED-LINES-001, SHARED-LINES-002, RH0001-004, RH0001-007 — see #353

### Examples for the tests

RH2111-002, RH2111-003, RH2111-004, RH2111-005. RH2111-006 is shown by those examples. RH2111-001 is tested with the SHARED-LINES-002 cases written for `maximumTypeLines`.

### Output questions

Questions 1 to 4 under #353 apply to this issue too. Question 5 is this issue's own.

The change doc's visible output change 4 also needs the maintainer's decision: today RH8402 reports nothing while RH0001 finds any problem, and an unknown `design` section is such a problem. After this change, a valid `design` section no longer silences RH8402. This follows from question 1A, and it comes with that answer.

### Internal choices

- Each part of a partial type is judged on its own, as the issue proposes (RH2111-004).
- Extension blocks count toward their type and are not judged as types. A file's total length is not judged. Both are non-goals.
- Documentation comments and every kind of directive are excluded by one cause: they are trivia (SHARED-LINES-001). The issue lists `#region` only.

### Follow-up candidates

_None._

## Whole run

- I ran `scripts/test.sh --project architecture` after writing the specs. The only problem in my own text was that the `Uses` row of `shared/line-count.md` lacked RH0001-003, RH0001-007, and SHARED-DIAG-008; I fixed it. A rerun of `SpecStructureTests` passes 11 of 12 tests.
- These failures remain, and they are expected until the analyzers and their tests exist:
  - `AnalyzerSpecHeaderMatchesItsRule`: `analyzer/RH2008.md` and `analyzer/RH2111.md` are not named after an existing rule ID.
  - `EveryCodeOwnerNamesAnExistingType`: `RH2008MethodsShouldNotExceedTheLineLimitAnalyzer` and `RH2111TypesShouldNotExceedTheLineLimitAnalyzer` are not declared.
  - `EveryRequirementOfAnEnforcedSpecIsCovered`: RH0001-007, RH2008-001 to RH2008-005, RH2111-001 to RH2111-006, and SHARED-LINES-001 and SHARED-LINES-002 have no covering test.
- The other 194 architecture tests passed.
