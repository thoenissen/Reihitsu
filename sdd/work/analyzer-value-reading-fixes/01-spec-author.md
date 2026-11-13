Role: spec author
Commit: 91e2fce
Verdict: #1071: spec written; #1268: spec written; #1289: spec written; #974: open question (one output question; the RH4009 wording for a configuration with content after its root waits for it)

Lane is bug fix: no change doc, no new requirement IDs, no rule page changed. Every edit below aligns a rule spec's text with an unchanged shared requirement (SHARED-DIAG-002, SHARED-DIAG-003) or states RH0001-003's existing reach explicitly. SHARED-DIAG applies to every rule without a `Uses` entry (`sdd/specs/shared/diagnostics.md`, intro), so no `Uses` row changed. My prompt carried no intended fix or suspected cause.

## thoenissen/Reihitsu#1071

### Changed requirements

- RH4001-002 — The type name is compared with the file name up to its first dot — clarified: the "written without `@`" restriction is replaced by "by its value (SHARED-DIAG-002)"; new example: `class @class` in `class.cs` and `class @Order` in `Order.cs` are not reported, `class @class` in `@class.cs` is reported.
- RH4001-003 — A generic type is expected with its type parameters in braces — clarified: the "type parameters written without `@`" restriction is dropped, names are values; new example: `Cache<@TKey>` is not reported in `Cache{TKey}.cs` and is reported in `Cache{@TKey}.cs`.
- RH4001-005 — The fix renames the file — clarified: the "type name written without `@`" restriction is dropped; new examples: `Invoice.cs` with `class @Order` → `Order.cs`, `Invoice.cs` with `Cache<@TKey>` → `Cache{TKey}.cs`, `@class.cs` with `class @class` → `class.cs`.
- RH4009-002 — A namespace that is not on the list is reported — clarified: the name compared is its value (SHARED-DIAG-002) instead of "a name written without `@`, whitespace, or comments"; new example: with the list `[ "MyCompany.Project" ]`, `namespace @MyCompany.Project`, `namespace MyCompany . Project`, and `namespace MyCompany./* x */Project` are not reported, and `namespace @MyCompany . Data` is reported on its whole name. The sentence "This requirement covers a configuration file that has no content after its root object." is left unchanged pending the #974 output question.

### Examples for the tests

RH4001-002, RH4001-003, RH4001-005 (the `@` examples), RH4009-002 (the second example). The visible changes against `main` (`@Order` in `Order.cs` no longer reported, `@class.cs` now reported, fix names without `@`, `@`/trivia namespace names matched) are all demanded by the unchanged SHARED-DIAG-002, whose text names `class @class` and `namespace A . B` itself.

### Output questions

_None._

### Internal choices

- RH4009-002 states that a list entry is compared as written, so an entry `"@MyCompany.Project"` matches no declaration, and both `namespace MyCompany.Project` and `namespace @MyCompany.Project` are reported. This follows from the unchanged normative "is not exactly one of the entries" together with SHARED-DIAG-002 (both spellings must give the same result); the alternative — also stripping `@` from entries — would have the configuration value interpreted as C# spelling, which no spec asks for. The lead may drop the sentence if it judges it outside the issue; the outcome does not depend on it.
- Unicode escapes in identifiers (`class Abc`) are not mentioned: SHARED-DIAG-002 names only the verbatim prefix and inner trivia, and the run's issues do not ask about escapes.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1268

### Changed requirements

- RH2003-001 — An explicit creation is reported — clarified: the coverage sentence that excluded `using` aliases is removed; the normative sentence now says "however its type name is written: qualified or not, and through a `using` alias of the type or of its namespace (SHARED-DIAG-002)"; new example with `NIE`, `S.NotImplementedException`, and `S::NotImplementedException`.
- RH2003-003 — Other exception types are not reported — clarified: adds the other side of the alias condition: `using NotImplementedException = System.NotSupportedException;` then `new NotImplementedException()` is not reported.

### Examples for the tests

RH2003-001 (alias example), RH2003-003 (alias of another type). The SHARED-DIAG-002 alias example already lists `NIE` and `S.NotImplementedException` for RH2003.

### Output questions

_None._

### Internal choices

_None._

### Follow-up candidates

_None._ RH2003-002 (target-typed `new()`) has no name to filter on and is unaffected.

## thoenissen/Reihitsu#1289

### Changed requirements

- RH2109-001 — A `@code` block in a `.razor` file is reported — clarified: the coverage sentence now adds that a `@code` that SHARED-DIAG-003 makes text is not a directive and is not reported, pointing at SHARED-DIAG-003's last example (HTML comment, Razor comment, `@@code`, `user@code{`) instead of restating it.

### Examples for the tests

SHARED-DIAG-003's last example (the `Counter.razor` block), and the C#-comment, string, and raw-string inputs inside a `@code` block named in thoenissen/Reihitsu#1289 (C# comments and literals are content under SHARED-DIAG-003's first two bullets). Three existing tests of `RH2109RazorCodeBlocksShouldNotBeUsedAnalyzerTests` are characterizations that pin today's (wrong) reports of these inputs; they are the test implementer's to turn around.

### Output questions

_None._

### Internal choices

_None._

### Follow-up candidates

_None._ Fact for the following roles: today the analyzer matches `@code\s*\{` anywhere in the file, also in the middle of a line (`<div>@code {` is reported). RH2109-001's coverage sentence leaves a mid-line `@code` that is not text undecided, and neither issue asks about it; the fix of this run changes only what SHARED-DIAG-003 makes text, and a mid-line `@code` that is not text keeps today's report. The `@functions` part is thoenissen/Reihitsu#1452 and not in this run.

## thoenissen/Reihitsu#974

### Changed requirements

- RH0001-003 — An empty or malformed file is one problem — clarified: "not valid JSON" now names "anything other than white space after the root value" next to comments and trailing commas; new example: `{ } junk` reports invalid JSON syntax at the `j`, `{ } { }` at the second `{`, `{ } // c` at the `/`; `{ }` followed only by white space is valid. This is already decided by the unchanged normative sentence (`{ } junk` is not valid JSON) and by the issue's expected behavior.

CLI: `sdd/specs/cli/reihitsu-format.md` lists reading `reihitsu.json` as a non-goal, and `ConfigurationManager` is referenced only by `Reihitsu.Analyzer` and `Reihitsu.Analyzer.CodeFixes` (RH0001, RH4009, RH8402 and its fix). The CLI does not share the configuration loading; there is nothing to align.

RH8402-001 already says nothing is reported "with any problem in the configuration", so once trailing content is an RH0001 problem RH8402 reports nothing for such a file — decided, no edit.

### Examples for the tests

RH0001-003 (the new example). Tests for RH4009 and for the other RH0001 problems in a file with trailing content wait for the answer below.

### Output questions

**RH0001 · Configuration file must be valid, and RH4009 · Namespace not allowed — a `reihitsu.json` with content after its root object**

Input — `reihitsu.json`:

```json
{ "naming": { "allowedNamespaceDeclarations": [ "A" ] }, "formatting": 1 } junk
```

and a source file:

```csharp
namespace B
{
}
```

Real output on `main`: RH0001 reports "Unknown configuration section 'formatting'." on `"formatting"` and no syntax error; RH4009 reports `B` (the loader stops reading at the root object's `}` and uses what it read).

Why it is in question: RH0001-003 makes the trailing `junk` a syntax error (decided), and says "A file that cannot be parsed MUST NOT be validated further". One reading treats a file with content after its root as a file that cannot be parsed; another treats the root object as parsed and the trailing text as one more problem. For RH4009, RH4009-001 silences the rule only for JSON "malformed before its root value ends", and RH4009-002 covers only a file without content after its root object — both written to leave this case open while #974 was undecided. Requirement IDs: RH0001-003, RH4009-001, RH4009-002.

Before (`main`):

```text
reihitsu.json: RH0001 "Unknown configuration section 'formatting'." on "formatting"
namespace {|RH4009:B|}
```

After, A (recommended) — content after the root makes the whole file invalid JSON, like any other syntax error:

```text
reihitsu.json: RH0001 "The configuration file contains invalid JSON syntax." at the `j` of `junk`
namespace B   — not reported (no configuration is loaded)
```

After, B — the root object is still read; the trailing content is one more problem:

```text
reihitsu.json: RH0001 "The configuration file contains invalid JSON syntax." at the `j` of `junk`
reihitsu.json: RH0001 "Unknown configuration section 'formatting'." on "formatting"
namespace {|RH4009:B|}
```

Recommendation: A — the issue asks that trailing content be treated "like every other invalid JSON", and every other syntax error already reports only itself and leaves RH4009 and RH8402 inactive; B would make a malformed file half-valid. On A, the spec author changes RH4009-001 to "malformed" without the "before its root value ends" qualifier, drops the coverage sentence of RH4009-002, and adds the `{ "formatting": 1 } junk` example to RH0001-003; on B, RH0001-003 and RH4009-001/-002 state that the root object of such a file is still validated and applied.

### Internal choices

_None._

### Follow-up candidates

_None._
