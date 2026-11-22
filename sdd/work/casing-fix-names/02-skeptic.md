Role: skeptic
Commit: 77bf6dc
Verdict: #971: output question; #1072: proceed; #1451: proceed; #1434: output question

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Read: the four issues and their comments, the decisions on thoenissen/Reihitsu#1009 and thoenissen/Reihitsu#1010, `00-lead.md`, `01-spec-author.md`, the spec diff `a8fa3fa..8939ef7` and the three change docs, `CasingUtilities`, `CasingAnalyzerBase`, `NamingAccessibilityClassifier`, the RH4005 fix's `OnTransformIdentifier`, the GetLocations of RH4102/RH4103/RH4104/RH4114/RH4116, and the rename flow of `CasingCodeFixProviderBase{T}`. Every "today" output below was measured with `scripts/apply-fix.sh` on this commit (LF arm quoted; the CRLF arm was identical). Fixtures lived in the scratchpad and were deleted. `apply-fix.sh` checks only syntax errors of a result, so no statement below says a fixed result compiles.

No formatter phase or formatter entry point is involved in any issue: the casing fixes rename through `Renamer.RenameSymbolAsync` only (`src/Reihitsu.Analyzer.CodeFixes/Base/CasingCodeFixProviderBase{T}.cs:141`, `:436`). Declaring a shape unsupported is therefore not an option for any of the four issues, and no entry point predicts a phase's decision (question 10: none). No P11/FC-013 relation: the formatter does not judge names.

## thoenissen/Reihitsu#971

### Premise

Real and reachable. Caller: the code fixes of RH4105, RH4106, RH4107, RH4115, RH4117 (`CasingUtilities.ToCamelCase` / `ToUnderlineCamelCase`) and RH2001's backing-field name (`RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider.cs:380`). Mechanism today (`src/Reihitsu.Core/CasingUtilities.cs`, `ConvertToPascalCase` + `ToCamelCase`): only the first character of the joined PascalCase name is lowered, and a word is lowercased only when every character after its first is uppercase.

```
scripts/apply-fix.sh RH4105 <dir>   # Params.cs
-    int M(int A_B, int I_O_Stream, int XMLParser)
+    int M(int aB, int iOStream, int xMLParser)
```

The issue was filed as a bug, but SHARED-CASING-006 on `main` excludes first words that start with two uppercase letters from coverage, so it is a behavior change, as triage said. Severity: cosmetic, but the output (`_hTTPClient`) is a name nobody would write; a fix that writes it is worse than none.

### Mechanism and candidates

_N/A_ (behavior change). Candidates are every caller of `ToCamelCase` / `ToUnderlineCamelCase` (listed under *Premise*); `ToPascalCase` callers are untouched by this issue.

### Breaking inputs

The spec's camelCase step operates on the **joined** name (`01-spec-author.md`, *Internal choices*: "same output for every case I found"). That claim is wrong: the joined reading and the first-word reading differ wherever the first word's converted form ends in an uppercase letter that runs into the next word. Measured today vs. the two readings of the new text, `_camelCase` target (RH4106) and camelCase target (RH4105):

```
scripts/apply-fix.sh RH4106 <dir>   # Fields.cs
-    private int A_B;
-    private int A_B_Cd;
-    private int m_HTTPClient;
-    private int X_Y_Z;
+    private int _aB;
+    private int _aBCd;
+    private int _mHTTPClient;
+    private int _xYZ;
```

| Old name | Today (measured) | Joined reading (spec text) | First-word reading |
|---|---|---|---|
| `A_B` (RH4106) | `_aB` | `_ab` | `_aB` |
| `A_B_Cd` (RH4106) | `_aBCd` | `_abCd` | `_aBCd` |
| `m_HTTPClient` (RH4106) | `_mHTTPClient` | `_mhttpClient` | `_mHTTPClient` |
| `X_Y_Z` (RH4106) | `_xYZ` | `_xyz` | `_xYZ` |
| `I_O_Stream` (RH4105) | `iOStream` | `ioStream` | `iOStream` |
| `HTTPClient`, `XML_Parser`, `X_YZ`, `FName`, `first_SName` | — | same in both readings | same |

The joined reading glues the `m` of `m_` into the acronym (`_mhttpClient`), which contradicts the requirement's own *Why* ("the `_` of a leading `m_` is a word boundary"). Visible output → output question 1 below.

Second class member the change doc does not show: a plural acronym, which is common in field and parameter names. Measured today:

```
scripts/apply-fix.sh RH4105 <dir>   # Plural.cs
-    int M(int URLs, int IDs, int APIs)
+    int M(int uRLs, int iDs, int aPIs)
```

Under the new text (both readings): `urLs`, `iDs`, `apIs` (the run's last letter keeps its case in front of the `s`). This is what the decided rule literally gives; it belongs in the maintainer's view of question 1 of `01-spec-author.md`, so the lead should add the row there (see *Output questions*).

Failure classes that reach the change: FC-023 (Unicode identifiers, overlapping with thoenissen/Reihitsu#1451), FC-008/SHARED-CASING-010 (more lowercasing means more Fix All collisions), FC-015 (RH2001 reuses the shared conversion; no private copy found — `rg IsLetterOrDigit|IsUpper` in `src` finds no other casing copy besides RH4005's prefix check, which is a different decision).

Inputs the test implementer must cover:

| Input | Expected (spec) | Existing test |
|---|---|---|
| `HTTPClient`, `XMLParser`, `IOStream`, `HTTP2Client`, `ABc` → RH4106 | `_httpClient`, `_xmlParser`, `_ioStream`, `_http2Client`, `_aBc` | none found |
| `XMLParser` → RH4105 | `xmlParser` | none found |
| `FirstName`, `LAST_NAME`, `m_value`, `__double`, `URL`, `iPhone` → RH4106 | unchanged | `RH4106PrivateFieldCasingAnalyzerTests` (around line 399) pins them |
| `FName`, `first_SName` → camelCase | `fName`, `firstSName` (unchanged) | `CasingUtilitiesTests.ToCamelCaseTest` pins them |
| `URLs` → RH4105 | per answer to the amended question 1 | none found |
| `A_B`, `m_HTTPClient` → RH4106 | per output question 1 below | none found |
| two fields `HTTPClient` and `HttpClient` in one class, Fix All (RH4106) | both derive `_httpClient`; Fix All skips both (SHARED-CASING-010) — new collision created by this change | none found |
| RH2001 `private int HTTPClient { get; set; }` | backing field `_httpClient` | none found |
| one camelCase rule other than RH4105 (RH4115 or RH4117) and RH4107 | leading run lowercased | none found |

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `Reihitsu.Core` `CasingUtilities.ToCamelCase`, `ToUnderlineCamelCase` | must change |
| `CasingUtilities.ToPascalCase`, `ToTypeParameterName` | must stay (PascalCase targets keep `HTTPClient`) |
| Analyzers (reporting) | not involved |
| Code-fix providers (RH4105/4106/4107/4115/4117, RH2001) | not involved in code; output changes through the shared helper |
| Fix All | must stay correct: collisions grow (`HTTPClient` + `HttpClient`) |
| Rule pages | not involved |
| Tests | `CasingUtilitiesTests` rows + one rule-level test per target style |

### Refuse option

Withhold the fix for a camelCase/`_camelCase` target whose first word starts with two uppercase letters — exactly the class `main`'s spec leaves uncovered. Cost: one cheap check, no new type; the requested change costs a few lines in the camelCase step of `CasingUtilities`, no new type. The class is not rare (`HTTPClient`, `IOStream`, `URLs` are common names), and the target output was decided on thoenissen/Reihitsu#928, so the requested change is the better option. No simpler option.

### Output questions

**1 · RH4106 · Private field casing (and RH4105, RH4107, RH4115, RH4117, RH2001's backing field) — single-letter words before an uppercase word**

Input:
```csharp
public class C
{
    private int A_B;
    private int m_HTTPClient;

    public int Sum => A_B + m_HTTPClient;
}
```

Before (`main`):
```csharp
    private int _aB;
    private int _mHTTPClient;
```

After, A (recommended):
```csharp
    private int _aB;
    private int _mHTTPClient;
```

After, B:
```csharp
    private int _ab;
    private int _mhttpClient;
```

Why A: the leading uppercase run is taken from the first word only, so a word boundary the author marked with `_` is never merged into an acronym; it changes nothing beyond the issue's case (`HTTPClient` → `_httpClient` in both).

Amendment to question 1 of `01-spec-author.md` (no separate choice; the row shows what the recommended rule gives for a common name): add `private int URLs;` → before `_uRLs`, after A `_urLs`.

### Internal choices

- The spec author's *Internal choices* entry for #971 ("same output for every case I found") is not an internal choice; it is output question 1 above. Once answered, SHARED-CASING-006's second paragraph should say "the leading run of uppercase letters of the first word" (answer A) and the change doc's *Decisions* should record it.

### Issue goals

- "Extend `CasingUtilitiesTests` with the table above; reproduce through RH4106 and at least one camelCase rule" — keep.
- "Spec: RH4106-004 … The open-question non-goal of RH4106 is removed" — stale: the behavior now lives in SHARED-CASING-006; check that the RH4106 spec carries no leftover non-goal for leading acronyms (none in the diff). No visible change.
- "Squad lane: bug fix" — stale (triage reclassified); behavior change. No visible change.

### Architecture findings

_None._

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1072

### Premise

Real and reachable through the RH4105, RH4115, and RH4117 fixes — the only callers of `ToCamelCase`; every other style starts with `_` or an uppercase letter, so only these can derive a keyword. The spec author measured `Class` → `@class`. I measured the contextual-keyword members:

```
scripts/apply-fix.sh RH4115 <dir>
AwaitLambda.cs:  var Await = 1; … return Await;   →  var @await = 1; … return @await;   (inside an async lambda)
Select.cs:       var Select = 1; return from x in xs select Select;
                 →  var select = 1; return from x in xs select @select;
Yield.cs:        var Yield = 1; yield return Yield;  →  var yield = 1; yield return yield;
Dynamic.cs:      var Dynamic = 1; var Async = 2;     →  var dynamic = 1; var async = 2;
Field.cs:        var Field = 1; (inside a property getter)  →  no-fix-offered
Value.cs:        var Value = 1; (inside a setter)           →  no-fix-offered
```

```
scripts/apply-fix.sh RH4105 <dir>   # Verbatim.cs
-    int M(int @Class)
+    int M(int @class)
```

Severity is right: parameters such as `Event`, `Default`, `Base`, `Lock`, `String`, `Object` are plausible in converted code, and `@event` only looks corrected (SHARED-DIAG-002 compares without `@`).

### Mechanism and candidates

_N/A_ (behavior change). The membership the spec states — "the rename would write `@` at the declaration or a reference" — is a result the Renamer computes, and the measurements show it is position-dependent: `select` is written without `@` at the declaration and with `@` at the query reference. The fix flow already holds the renamed solution before it decides (`CasingCodeFixProviderBase{T}.cs:436` onward), so the cause can be read there; a check by a keyword list or a list of contexts would be FC-044 and would miss the query-keyword member.

### Breaking inputs

Failure classes: FC-044 (modelling the Renamer's escaping), FC-023 (verbatim input), FC-027 (other enclosing scopes: lambda, query, local function), FC-008 (Fix All with one withheld candidate).

| Input | Expected (spec) | Existing test |
|---|---|---|
| RH4105 `Class`, `Object` | no fix | none found |
| RH4105 `Await` in an async method | no fix | none found |
| RH4115 `Await` in an async lambda (measured `@await` today) | no fix | none found |
| RH4115 `Select` referenced inside a query expression (measured `@select` at the reference only) | no fix — the reference needs `@` | none found |
| RH4105 `Var`, `Nameof`, `Await` in a non-async method | `var`, `nameof`, `await` | none found (spec author verified today's output) |
| RH4115 `Yield`, `Dynamic`, `Async` (measured fixed, no `@`) | fixed as today | none found |
| RH4115 `Field` in an accessor, `Value` in a setter (measured no fix today) | no fix, unchanged | none found; not this issue's concern |
| RH4105 `@Class` (author wrote `@`; measured `@class` today) | no fix (spec: the rename writes `@`) | none found |
| RH4117 one reserved keyword | no fix | none found |
| Fix All (Document) over a parameter list with `Class` and `Count` | `Count` → `count`, `Class` keeps its diagnostic | none found |

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `CasingCodeFixProviderBase{T}` (withholding) | must change |
| `CasingUtilities` | must stay |
| Analyzers | not involved (the diagnostic stays) |
| Fix All | must skip the withheld candidates and keep fixing the rest |
| Rule pages | not involved (`general-notes.md` covers "a diagnostic without a code fix") |

### Refuse option

The requested change is itself a refusal (withhold the fix). A narrower option — reserved keywords only — leaves `@await` and `@select`, so the issue's "Done when" ("no longer write a `@` escape the author did not write") would not be met; the spec's cause-based wording is the right scope. No simpler option.

### Output questions

None beyond question 6 of `01-spec-author.md`, which I agree with. `@Class` → no fix (instead of today's `@class`) is a withheld fix for a rare class and puts nothing in question (`squad/README.md`, *What reaches the maintainer*); the PR names it under `Review notes`.

### Internal choices

- New requirement SHARED-CASING-013 rather than a condition of SHARED-CASING-008: agree.
- `@Class` (verbatim input): the spec withholds; the issue's goal wording ("a `@` the author did not write") would allow the fix. Recommend the spec as written — one cause, no exception for input spelling; rare.

### Issue goals

- "it changes SHARED-CASING-008 (or adds a requirement)" — a requirement was added; fine.
- "A contextual keyword (`var`, `value`, …) is … not affected" — narrow: a contextual keyword that needs `@` where it stands (`await` in async code, a query keyword inside a query) is affected. The spec already states this; the PR should say so. No visible change beyond question 6.
- "regression tests cover a reserved keyword (withheld) and a contextual keyword (fixed)" — keep, and add the `@await`/`@select` withheld cases.

### Architecture findings

_None._ (See *Mechanism*: reading the Renamer's result avoids an FC-044 model.)

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1451

### Premise

Real and reachable through the RH4005, RH4121, and every PascalCase/camelCase fix. Today: `IsPascalCase`/`IsCamelCase` test `char.IsLetterOrDigit` (`CasingUtilities.cs`), so Mn, Mc, Nl and non-`_` Pc characters make a name non-conforming, and `ConvertToPascalCase` treats them as separators and drops them. RH4005's prefix test is `char.IsUpper(identifier[1])` (`RH4005InterfaceNameCasingCodeFixProvider.cs:44-46`), and RH4121's is `IsUpper || IsDigit` of the second character (`CasingUtilities.ToTypeParameterName`), so `i_document` and `T_Key` get a second prefix. The spec author measured the "before" values; the code agrees.

Severity: every member is a rare input — decomposed accents (editors produce precomposed `é`), letter numbers, `‿`, `i_document`, `T_Key`. The maintainer decided the outputs (thoenissen/Reihitsu#1009 Q3: "every identifier character belongs to the word; the fix never drops one"; thoenissen/Reihitsu#1010 Q1: an existing `I` prefix in any case, with or without `_`, counts; no usable name → withheld). The Q3 decision covers the fix only; not reporting `Café` and `ClassⅫ` goes beyond it and is correctly question 4 of `01-spec-author.md`.

### Mechanism and candidates

_N/A_ (behavior change).

### Breaking inputs

Failure classes: FC-023 (Unicode identifiers), FC-015 (RH4005's analyzer and fix each state an `I`-prefix test; they answer different questions — "is the name valid" vs "is there a prefix to keep" — and both stay; no drift introduced).

Members the spec decides that the examples do not show (each rare; the spec gives a definite answer, listed so the test implementer can pick):

| Input | Spec gives | Note |
|---|---|---|
| `a_́b` (a word that starts with a combining mark) → RH4002 | `Áb` | The mark now attaches to `A`; `a_b` gives `AB`. Defined by the text, rare; no question. |
| `First＿Name` (U+FF3F FULLWIDTH LOW LINE, Pc) → RH4002 | accepted, not reported | Looks like `_` but is not the separator. Defined, rare. |
| `i18nService` → RH4005 | no fix (derived `I18nService` still reported); today `II18nService` | Same class as `i2Factory`, question 2 of `01-spec-author.md`. |
| `I_O_Stream` → RH4005 | `IOStream` (today `IIOStream`) | Prefix `I_` dropped. |
| `T__Key`, `t_`, `T_T` → RH4121 | `TKey`, `T`, `TT` | |
| `class C<T_, T>` → RH4121 | `T_` derives `T`, collides — SHARED-CASING-009 | existing collision rule |

Inputs the test implementer must cover:

| Input | Expected (spec) | Existing test |
|---|---|---|
| `Café` (U+0301), `ClassⅫ` → RH4002 | not reported (per question 4) | none found |
| `café_bar`, `order‿line`, `CAFÉ_NAME` (U+0301) | `CaféBar`, `Order‿line`, `CaféName` | none found |
| `i_document`, `I_reader` → RH4005 | `IDocument`, `IReader` | none found |
| `I`, `i`, `I_`, `i2Factory` → RH4005 | no fix (per question 2) | none found |
| `T_Key`, `t_value`, `T_1`, `T_` → RH4121 | `TKey`, `TValue`, `T1`, `T` | `CasingUtilitiesTests.ToTypeParameterNameTest` pins the unchanged rows (`tKey`, `t1`, `Type`, `Tkey`, `first_name`) |
| `_`, `__` → RH4005/RH4121 | unchanged (no fix) | `CasingUtilitiesTests` pins `_`, `__` |

Fixtures must be written byte-exact (`squad/README.md`, *Rules for every role*).

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `CasingUtilities.IsPascalCase`, `IsCamelCase`, `IsUnderlineCamelCase`, `ConvertToPascalCase`, `ToTypeParameterName` | must change |
| Every casing analyzer that takes `IsPascalCase`/`IsCamelCase` (reporting of Mn/Mc/Nl/Pc names) | output changes through the shared helper; no analyzer code change |
| RH4005 analyzer `IsInterfacePascalCase` | must stay (`I` + uppercase) |
| RH4005 code fix `OnTransformIdentifier` | must change |
| RH4121 analyzer (`IsTypeParameterName`) | must stay apart from the covered character set |
| Rule pages | not involved |

### Refuse option

Withhold the fix for an old name with a Mn/Mc/Nl/non-`_` Pc character, and for an `i_`/`I_`/`t_`/`T_` prefix: a cheap check, no new type, and the class is rare. But the maintainer decided the target names, and the requested change is a handful of lines in `CasingUtilities` and the RH4005 fix, no new type, so the requested change is proportionate. The issue itself listed withholding as the rejected alternative. No simpler option.

### Output questions

None beyond questions 2 to 5 of `01-spec-author.md`, which I agree with.

### Internal choices

- `_` stays the only separator although it is Pc: agree (consistent with SHARED-CASING-006).
- Lt and Cf stay outside the covered set: agree; no issue asks for them.

### Issue goals

- "Every character C# allows in an identifier part belongs to the word … The fix never drops a character" — keep; the spec narrows "every character" to the named categories (Lt, Cf outside), which no issue example needs. No visible change for the issue's examples.
- `class Café { } // not reported, or renamed without dropping U+0301` — an open choice in the issue; question 4.

### Architecture findings

_None._

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1434

### Premise

Real and reachable through the analyzer in any build: `CasingAnalyzerBase` registers a syntax-node action and reports every name its `GetLocations` yields; RH4102/RH4103/RH4104/RH4112–RH4114/RH4116 yield by syntax alone (RH4103 and RH4102 skip explicit interface implementations by syntax; `NamingAccessibilityClassifier` returns `NotApplicable` for explicit property implementations). No semantic check exists, so every override or implementation of a metadata member with a lowercase name and every `[DllImport]` method without `EntryPoint` is reported. The `[DllImport("libc")] static extern int getpid();` case is common Linux interop code; the metadata cases need a referenced library with non-.NET naming and are less common.

### Mechanism and candidates

_N/A_ (behavior change). Class matrix of "dictated":

- member kind: method, field-like event, accessor event, property, local function (native only), tuple element in a signature;
- relation: direct override, override through levels, implicit implementation on the member's own type, implicit implementation on behalf of a **derived** type, explicit implementation (tuple names only), static abstract implementation;
- dictating declaration: referenced assembly, another project of the solution, same compilation;
- tuple origin: written in the metadata member's declaration, or **supplied by a type argument written in this compilation** (`IEnumerable<(string key, int value)>`);
- native: `[DllImport]` method / local function, `[LibraryImport]` partial method; `EntryPoint` absent, literal, `nameof`.

Two members are not decided by the spec text, or are decided in a way the change doc does not list; see output questions 1 and 2 below.

### Breaking inputs

Failure classes: FC-044 (deciding "implements" by a model instead of the compiler's result), FC-024 (explicit implementations, static interface members, partial members), FC-016 (sibling rules: RH4111 is not affected — explicit implementations are `NotApplicable`, private properties cannot override; RH4120 is a follow-up; RH4105/RH4121 are a non-goal).

**Member A — implementation on behalf of a derived type.** Existing test `RH4103MethodNameCasingAnalyzerTests.NoFixIsOfferedWhenADerivedTypeImplementsAReferencedAssemblyInterfaceWithTheMethod` (line 1120) pins today: `A.process` is reported and gets no fix, where `public class A { public void process() {} }` and `public class B : A, IMeta {}` with `IMeta` in a referenced assembly. SHARED-CASING-014 says a member is dictated when it "implements — implicitly or explicitly — a member declared outside the compilation"; whether `A.process` implements `IMeta.process` (it does for `B`, not for `A`) is not stated. The two readings give different output → output question 1.

**Member B — tuple names supplied by a source type argument.** Derived from the syntax-only RH4116 GetLocations, not measured (no fix provider, so `apply-fix.sh` cannot run it):

```csharp
using System.Collections;
using System.Collections.Generic;

public class Pairs : IEnumerable<(string key, int value)>
{
    public IEnumerator<(string key, int value)> GetEnumerator()
    {
        yield break;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
```

Today: `key` and `value` reported twice (base list and `GetEnumerator`). Under SHARED-CASING-014 as written: `GetEnumerator` implements a BCL member, so it is dictated and its signature's tuple names are not reported; the base-list ones stay reported. The names are not chosen by the other compilation — they come from the type argument written in this file — so the requirement's *Why* ("only the declaration in the other compilation could change them") does not hold for them. The shape is common with BCL generics (`IEnumerable<(…)>`, `IComparer<(…)>`, `IEqualityComparer<(…)>`, `JsonConverter<(…)>` overrides), and the change doc's *Visible output changes* does not list it → output question 2.

Inputs the test implementer must cover (with the referenced-assembly support `CompiledAssemblyReference.CreateAsync` the existing tests use):

| Input | Expected (spec) | Existing test |
|---|---|---|
| `Worker`/`Leaf` block of SHARED-CASING-014 (method, event, property, tuple, implicit implementation, two levels) | none reported | contradicted: `RH4103…NoFixIsOfferedForADirectOverrideOfAReferencedAssemblyMember`, `…WhenTheRenameCascadesToAnOverrideOfAReferencedAssemblyMember`, `…WhenTheRenameCascadesToAnImplementationOfAReferencedAssemblyInterface`, `RH4102…CustomEventOverrideOfAReferencedAssemblyEvent`, `RH4102…FieldLikeEventOverrideOfAReferencedAssemblyEvent`, `RH4112/RH4113/RH4114…PropertyOverrideOfAReferencedAssemblyProperty`, `RH4114…ImplementationOfAReferencedAssemblyInterfaceProperty` — each must be checked: a test whose reported declaration is now dictated is updated, one whose reported declaration is a source member that cascades into a dictated one keeps its assertion |
| in-compilation chain `Base`/`Derived` | both reported | likely pinned by existing RH4103 override tests (check) |
| another project of the solution (`Base` in `BaseProject`, `Leaf` overrides) | `Leaf.process` not reported (question 8 of `01-spec-author.md`) | contradicted: `RH4103…FixRenamesAnOverrideTogetherWithTheOverriddenMethodOfAReferencedProject` (line 1650), `…NoFixIsOfferedWhenTheRenameCascadesFromAnOverrideToAMethodOfAReferencedProjectReferencedInExcludedCode` |
| derived-type implementation (member A) | per output question 1 | `RH4103…NoFixIsOfferedWhenADerivedTypeImplementsAReferencedAssemblyInterfaceWithTheMethod` |
| tuple through a source type argument (member B) | per output question 2 | none found |
| `IPairs.get` explicit implementation vs `ILocal.range` | `get` exempt; `range` reported | none found |
| `[DllImport("libc")] getpid` method, `[LibraryImport("libc")]` partial | not reported | contradicted: `RH4103…NoFixIsOfferedForADllImportMethodWithoutEntryPoint`, `…ForALibraryImportMethodWithoutEntryPoint`, `…FixIsWithheldForANativeEntryPointTakenFromTheMethodName`, and the cascade tests `…CascadesToADllImportImplementationWithoutEntryPoint`, `…CascadesToALibraryImportImplementationWithoutEntryPoint`, `…StaticVirtualMethodCascadesToADllImportImplementation`, `…CascadesToADllImportImplementationInADependentProject` (the implementation's diagnostic disappears; the interface member stays reported without a fix) |
| `[DllImport]` local function without `EntryPoint` | not reported (question 9) | contradicted: `RH4104…NoFixIsOfferedForAnExternLocalFunctionBoundByItsName` (line 417) |
| `EntryPoint = "getppid"`, `EntryPoint = nameof(getpid)` | reported; fix offered / withheld | `RH4103…FixRenamesADllImportMethodWithALiteralEntryPoint`, `…NoFixIsOfferedForADllImportMethodWhoseEntryPointIsNameofTheMethod` |
| partial property or method part that overrides a metadata member | both parts not reported | none found |

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `CasingAnalyzerBase` and the analyzers RH4102, RH4103, RH4104, RH4112, RH4113, RH4114, RH4116 | must change (need the semantic model; today syntax only) |
| Casing code fixes, `CasingRenameSafetyUtilities` | must stay (cascade withholding already covers renames that reach a dictated member) |
| RH4111, RH4105, RH4121 | not involved (RH4111: no private member can be dictated; RH4105/RH4121: non-goal) |
| RH4120 | not involved in this run; follow-up candidate (already in `00-lead.md`) |
| Rule pages | not involved |

### Refuse option

Keep reporting and withhold the fix — `main`'s behavior for most members (the cascade checks already withhold). It costs nothing, but it leaves diagnostics the author cannot resolve, which principle P2 (thoenissen/Reihitsu#912) rejected and the issue lists as its rejected alternative. The requested change needs a semantic check in the analyzers (override chain, the containing type's interface mappings, a containing-member lookup for tuple types, an attribute check); likely below 100 production lines and no new type besides a helper. The native part is not rare; the metadata part is uncommon but decided by P2. Proceed. A narrower option that drops member B's change: decide tuple names by where the name is written (see question 2, option B) — more machinery, not cheaper.

### Output questions

**1 · RH4103 · Method name casing (and RH4102, RH4112–RH4114) — a base-class method that implements a referenced-assembly interface member for a derived class**

Input, with a referenced assembly that declares `public interface IMeta { void process(); }`:
```csharp
public class A
{
    public void process()
    {
    }
}

public class B : A, IMeta
{
}
```

Before (`main`): `process` reported, no fix offered.

After, A (recommended): `process` reported, no fix offered (as today).

After, B: not reported.

Why A: `A` itself implements nothing, and the analyzer would have to search every derived type to find the implementation — and cannot see a derived type in another project at all, so IDE and build could still differ; today's output stays.

**2 · RH4116 · Tuple element casing — tuple names that come from a type argument written in the same file**

Input:
```csharp
using System.Collections;
using System.Collections.Generic;

public class Pairs : IEnumerable<(string key, int value)>
{
    public IEnumerator<(string key, int value)> GetEnumerator()
    {
        yield break;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
```

Before (`main`):
```csharp
public class Pairs : IEnumerable<(string {|RH4116:key|}, int {|RH4116:value|})>
{
    public IEnumerator<(string {|RH4116:key|}, int {|RH4116:value|})> GetEnumerator()
```

After, A (recommended):
```csharp
public class Pairs : IEnumerable<(string {|RH4116:key|}, int {|RH4116:value|})>
{
    public IEnumerator<(string key, int value)> GetEnumerator()
```

After, B: as before.

Why A: the names in `GetEnumerator` can only change together with the base list, which stays reported, so each choice is reported once; B would need to trace which names come from the other library's declaration and which from a type argument.

### Internal choices

- If question 2 is answered A, SHARED-CASING-014's *Why* needs one sentence: the tuple names of a dictated member's signature are repeated from the member it implements after its type arguments are substituted, and a name supplied by a type argument written in this compilation is reported where that type argument is written. The change doc then lists the case under *Visible output changes*. If answered B, the tuple bullet must say "an element name that the declaration in the other compilation writes".
- If question 1 is answered A, SHARED-CASING-014 should state that a member that implements an interface member only on behalf of a derived type is not dictated, so the existing test at line 1120 keeps its assertion under a SHARED-CASING-014 tag.
- "No `EntryPoint` argument" is decided by the attribute's syntax/arguments, while `CasingRenameSafetyUtilities.GetNativeEntryPoint` (CodeFixes, line 503) computes the entry point name (`GetDllImportData().EntryPointName ?? method.Name`). They answer different questions (report vs. fix safety), and the spec states both; recommend keeping them separate and not routing the analyzer's question through the fix's helper (the analyzer project cannot reference CodeFixes anyway).

### Issue goals

- "New requirement SHARED-CASING-011" — the ID is taken; SHARED-CASING-014 is right. No visible change.
- "a member declared outside the compilation's source (a referenced assembly)" — widened by the spec to "another project of the solution" (question 8 of `01-spec-author.md`). Visible; asked.
- RH4104 is not in the issue's rule list; including `[DllImport]` local functions is question 9. Visible; asked.
- "each instance has a regression test tagged with SHARED-CASING-011" — tag SHARED-CASING-014 instead. No visible change.
- "a tuple element name that such an override or implementation must repeat" — as written, the spec also exempts names supplied by a source type argument (question 2).

### Architecture findings

_None._

### Follow-up candidates

_None_ beyond the RH4120 candidate already recorded in `00-lead.md`.

## Whole run

No build or test run by this role beyond the `apply-fix.sh` measurements above (each run reported 0 analyzer-failure, 0 parse-error, 0 invalid-result).
