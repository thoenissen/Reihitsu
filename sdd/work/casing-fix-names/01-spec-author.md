Role: spec author
Commit: a8fa3fa
Verdict: #971: change doc written; #1072: change doc written; #1451: change doc written; #1434: change doc written

The prompt carried no intended fix or verdict. All four issues are `agent-filed`; their goals were checked against the maintainer decisions they cite (thoenissen/Reihitsu#912 P2/P3, thoenissen/Reihitsu#1009 Q2/Q3, thoenissen/Reihitsu#1010 Q1, thoenissen/Reihitsu#1020 Q1). Every "before" below was measured with `scripts/apply-fix.sh` on `a8fa3fa` unless it says otherwise; every new example compiles (scratch build of all new examples, a referenced-assembly stub included, on .NET 11).

Change docs:
- `sdd/changes/2026-11/casing-fix-name-derivation.md` — thoenissen/Reihitsu#971 and thoenissen/Reihitsu#1451 (shared cause: the name derivation of SHARED-CASING-006 and the styles it targets).
- `sdd/changes/2026-11/casing-fix-keyword-names.md` — thoenissen/Reihitsu#1072.
- `sdd/changes/2026-11/casing-names-dictated-elsewhere.md` — thoenissen/Reihitsu#1434.

No rule page changes (each change is a fix-output detail or an exception; the pages show the normal case and none of their examples is affected).

## thoenissen/Reihitsu#971

### Changed requirements
- SHARED-CASING-006 — The fix derives the new name from the words of the old one — changed: for camelCase and `_camelCase` targets, the leading run of uppercase letters of the joined name is lowercased; when the run has two or more letters and a lowercase letter follows directly, its last letter keeps its case. The coverage sentence that excluded first words starting with two uppercase letters is removed. PascalCase targets unchanged.

### Examples for the tests
- SHARED-CASING-006: "a leading acronym, for RH4106" (`HTTPClient`, `XMLParser`, `IOStream`, `HTTP2Client`, `ABc`), "a leading acronym, for RH4105" (`XMLParser`), "a leading acronym in a PascalCase target, for RH4110" (`HTTPClient_timeout` → `HTTPClientTimeout`, unchanged today; `_httpClientTimeout` for `_camelCase`). Existing RH4106/RH4105 examples (`FirstName`, `URL`, `iPhone`) must keep passing.
- Reached also through RH4107, RH4115, RH4117, and the RH2001 backing-field name (`private int HTTPClient { get; set; }` → `_httpClient`; before `_hTTPClient`).
- Coverage first: `CasingUtilitiesTests` (Core) holds conversion tests; extend there and add one rule-level test per target style.

### Output questions
See *Whole run*, questions 1.

### Internal choices
- The run is measured on the joined name, not on the first word: `XML_Parser` → `xmlParser`, `X_YZ` → `xYz` (unchanged today). Alternative: first word only; same output for every case I found.

### Follow-up candidates
_None._

## thoenissen/Reihitsu#1072

### Changed requirements
- SHARED-CASING-013 — The fix is withheld when the new name needs the verbatim prefix — added.
- SHARED-CASING-007 — The fix renames the declaration and every reference — changed: the derived-name clause of the coverage paragraph moved into a bullet that refers to SHARED-CASING-013, and the closing sentence lists SHARED-CASING-013.
- RH4105-003, RH4115-003, RH4117-002 — changed: withheld also as SHARED-CASING-013 specifies; `Uses` gains SHARED-CASING-013.
- `shared/casing.md` `Uses` gains SHARED-DIAG-002 (cited in the Why of SHARED-CASING-013).

### Examples for the tests
- SHARED-CASING-013: the RH4105 block with `Class`, `Object`, and `Await` in an async method (no fix; today `@class`, `@object`, `@await`), and the contextual block `Var`, `Nameof`, `Await` in a non-async method (fixed to `var`, `nameof`, `await`; verified unchanged today). One test each through RH4115 and RH4117 with a reserved keyword is enough.

### Output questions
See *Whole run*, question 6.

### Internal choices
- New requirement rather than a new condition of SHARED-CASING-008 (different reason: P3, not "no usable name"). Alternative: extend SHARED-CASING-008.
- Stated by cause ("the rename would write `@`"), with reserved keywords and `await` in an async member as the named cases. Alternative: a keyword list.

### Follow-up candidates
_None._

## thoenissen/Reihitsu#1451

### Changed requirements
- SHARED-CASING-001, SHARED-CASING-002, SHARED-CASING-003 — changed: the covered character set adds Nl, Mn, Mc, and Pc; any such character other than `_` is accepted wherever a letter or digit is. New example under SHARED-CASING-001 (`Café` with U+0301, `ClassⅫ` not reported; `café` reported).
- SHARED-CASING-006 — changed: only `_` splits words; other characters are kept in their word; a combining mark counts as part of the character in front of it (all-uppercase test and acronym run); Nl and Pc other than `_` count like a digit. A first word starting with a letter number is added to the uncovered list.
- RH4005-002 — changed: `i`/`I` followed by `_` or a digit, or alone, is the prefix; underscores behind it are dropped; nothing behind the prefix withholds the fix.
- RH4121-003 — changed: `t`/`T` followed by `_` is the prefix with its underscores; empty body gives `T`. The coverage exclusion of `T_…` is removed.

### Examples for the tests
- SHARED-CASING-001: the `Café` / `ClassⅫ` / `café` block (write the fixtures with `́` and `Ⅻ` escapes or byte-exact; tools decode escapes).
- SHARED-CASING-006: "characters that belong to their word" (`café_bar` → `CaféBar`, `order‿line` → `Order‿line`, `CAFÉ_NAME` → `CaféName`; today `CafeBar`, `OrderLine`, `CafeName`).
- RH4005-002: `i_document` → `IDocument`, `I_reader` → `IReader` (today `IIDocument`, `IIReader`); "a prefix with nothing usable behind it": `I`, `i`, `I_`, `i2Factory` → no fix (today `II`, `II`, not measured for `I_`, `II2Factory`).
- RH4121-003: `T_Key`, `t_value`, `T_1` → `TKey`, `TValue`, `T1`; `T_` → `T` (today `TTKey`, `TT1`, `TT`; `T__Key` also `TTKey` today → `TKey`).

### Output questions
See *Whole run*, questions 2 to 5.

### Internal choices
- `_` stays the only separator although it is Pc too (the decision on thoenissen/Reihitsu#1009 Q3 says Pc belongs to the word; `_` is the established separator of SHARED-CASING-006).
- Lt (titlecase letters) and Cf (formatting characters) stay outside the covered set; nothing in the issues asks for them.

### Follow-up candidates
_None._

## thoenissen/Reihitsu#1434

### Changed requirements
- SHARED-CASING-014 — A name that another compilation or a native entry point dictates is not reported — added (the issue proposes SHARED-CASING-011, which is taken).
- SHARED-CASING-007 — changed: the cross-project variant of the "excluded code reached through the rename cascade" example now states that `Leaf.process` is not reported and `Base.process` gets no fix.
- SHARED-CASING-009 — changed: the referenced-assembly cascade example now reports `IWorker.process`, whose implementation `Middle.process` overrides `MetaBase.process` (the old example's `Leaf.process` is no longer reported). Not runnable with `apply-fix.sh` (needs a referenced assembly); the test implementer verifies that the fix is withheld.
- RH4102-001, RH4103-001, RH4112-001, RH4113-001, RH4114-001, RH4116-001 — changed: the coverage sentences that left dictated members out are replaced by "MUST NOT be reported" for names SHARED-CASING-014 exempts; `Uses` gains SHARED-CASING-014. RH4103-001 now covers every method except an `extern` method without `[DllImport]`.
- RH4104-001, RH4104-002 — changed: a `[DllImport]` local function without `EntryPoint` is not reported; the RH4104-002 example now uses `EntryPoint = nameof(getpid)` (reported, no fix; verified today).
- SHARED-FIX-003 — changed wording only in the native-entry-point example: drops "where the rule reports both methods" and states that RH4103 does not report `getpid` itself; `shared/code-fixes.md` `Uses` gains SHARED-CASING-014.
- Non-goal added to `shared/casing.md`: parameter and type-parameter names of a dictated member stay with RH4105 and RH4121.

### Examples for the tests
- SHARED-CASING-014: the `Worker`/`Leaf` block against a referenced assembly (the analyzer test's metadata-reference support), the in-compilation chain (`Base`/`Derived`, reported), the explicit-implementation tuple block (`IPairs.get` exempt, `ILocal.range` reported), the native block (`getpid`, `getuid` not reported; `parentId`, `localPid` reported and fixed, verified today), and the "same in another project of the solution" variant.
- Existing tests that this change contradicts (they assert today's reports): in `RH4103MethodNameCasingAnalyzerTests`, `NoFixIsOfferedWhenTheRenameCascadesFromAnOverrideToAMethodOfAReferencedProjectReferencedInExcludedCode`, `FixRenamesAFileLocalOverrideTogetherWithTheOverriddenMethodWhenNoExcludedCodeNamesIt` (Leaf in the main project overrides `BaseProject`'s `Base.process`), and `NoFixIsOfferedWhenTheRenameCascadesToADllImportImplementationInADependentProject` (`Native.getpid` has `[DllImport]` without `EntryPoint`). RH4104's extern-local-function test likewise. These depend on output question 8 and 9.

### Output questions
See *Whole run*, questions 7 to 9.

### Internal choices
- "Dictated" follows overrides through every level and counts explicit implementations for the tuple bullet; the boundary rule specs already drew as coverage.
- `[DllImport]`/`[LibraryImport]` with any `EntryPoint` argument, `nameof` included, stays reported (as the issue says; the fix is withheld for `nameof` by SHARED-FIX-003).

### Follow-up candidates
- **RH4120 · Record primary constructor parameter casing — a positional parameter whose property implements a referenced-assembly property.** Input, with a referenced assembly that declares `public interface INamed { string name { get; } }`:

  ```csharp
  public record Person(string name) : INamed;
  ```

  Before (`main`, derived from the analyzer, not measured): `name` is reported by RH4120; renaming it breaks the implicit implementation. After, A: not reported (the synthesized property is dictated, as SHARED-CASING-014 states for properties). After, B: reported as today. Recommendation: A, same cause as SHARED-CASING-014; outside the issue's rule list, and the parameter is not itself the member SHARED-CASING-014 names.

## Whole run

Lane check and full test run not done by this role. `scripts/test.sh --project architecture`: 197 passed, 1 failed — `EveryRequirementOfAnEnforcedSpecIsCovered`, for SHARED-CASING-013 and SHARED-CASING-014 only (no tagged tests yet; the test implementer adds them). Spec-structure, `Uses`, ledger, and code-owner checks pass.

### Output questions

**1 · RH4106 · Private field casing (and RH4105, RH4107, RH4115, RH4117, RH2001's backing field) — leading acronym**

Input:
```csharp
public class C
{
    private int HTTPClient;
    private int IOStream;
    private int HTTP2Client;

    public int Sum => HTTPClient + IOStream + HTTP2Client;
}
```

Before (`main`):
```csharp
    private int _hTTPClient;
    private int _iOStream;
    private int _hTTP2Client;
```

After, A (recommended):
```csharp
    private int _httpClient;
    private int _ioStream;
    private int _http2Client;
```

After, B: as before.

Why A: it is the output thoenissen/Reihitsu#971 proposes from the decision on thoenissen/Reihitsu#928; PascalCase targets keep `HTTPClient` either way.

**2 · RH4005 · Interface name casing — existing `I` prefix**

Input: `interface i_document`, `interface I_reader`, `interface I`, `interface i`, `interface i2Factory`.

Before (`main`): `IIDocument`, `IIReader`, `II`, `II`, `II2Factory`.

After, A (recommended): `IDocument`, `IReader`; no fix offered for `I`, `i`, `i2Factory` (the diagnostic stays; `I2Factory` would still be reported because a digit follows the `I`).

After, B: `IDocument`, `IReader`, no fix for `I` and `i`, and `i2Factory` → `II2Factory` as today (a digit after `i` is not a prefix).

Why A: the maintainer's decision on thoenissen/Reihitsu#1010 Q1 counted `i2Factory` among the doubled prefixes.

**3 · RH4121 · Type parameter names — existing `T` prefix with `_`**

Input: `class C<T_Key>`, `class C<t_key>`, `class C<T_1>`, `class C<T_>`.

Before (`main`): `TTKey`, `TTKey`, `TT1`, `TT`.

After, A (recommended): `TKey`, `TKey`, `T1`, `T`.

After, B: `TKey`, `TKey`, `T1`; no fix offered for `T_`.

Why A: `T` alone is a valid name, and `t` already becomes `T`.

**4 · RH4002 · Class name casing (and every PascalCase, camelCase, `_camelCase` rule) — combining mark, letter number, connector punctuation**

Input (`é` = `e` + U+0301, `Ⅻ` = U+216B, `‿` = U+203F):
```csharp
class Café { }
class ClassⅫ { }
class café_bar { }
class order‿line { }
```

Before (`main`): all four reported; fixes give `Cafe`, `Class`, `CafeBar`, `OrderLine`.

After, A (recommended): `Café` and `ClassⅫ` not reported; `café_bar` → `CaféBar`, `order‿line` → `Order‿line`.

After, B: all four still reported; no fix for `Café` and `ClassⅫ` (the derived name equals the old one); `café_bar` → `CaféBar`, `order‿line` → `Order‿line`.

Why A: the characters have no case for the style to judge, and B leaves two diagnostics with no possible outcome.

**5 · RH4110 · Constant field casing (and every PascalCase target) — combining mark in a word in uppercase letters**

Input (`É` = `E` + U+0301):
```csharp
private const int CAFÉ_NAME = 1;
```

Before (`main`): `CafeName` (U+0301 dropped).

After, A (recommended): `CaféName` — the mark counts with its letter, so `CAFÉ` is a word in uppercase letters.

After, B: `CAFÉName` — the mark is not an uppercase letter, so the word keeps its case.

Why A: the same name written with the precomposed `É` gives `CaféName` today.

**6 · RH4105 · Parameter casing (and RH4115, RH4117) — derived name needs `@`**

Input:
```csharp
void M(int Class, object Object) { }
async Task<int> N(int Await) { await Task.Yield(); return Await; }
int P(int Var, int Nameof) { return Var + Nameof; }
```

Before (`main`): `@class`, `@object`, `@await` (and `return @await;`); `var`, `nameof`.

After, A (recommended): no fix offered for `Class`, `Object`, `Await` in the async method (diagnostics stay); `var`, `nameof` as before; `Await` in a non-async method still becomes `await`.

After, B: no fix for `Class` and `Object`; `Await` in the async method still becomes `@await`.

Why A: thoenissen/Reihitsu#1072's goal is that the fix writes no `@` the author did not write, and `@await` is one.

**7 · RH4103 · Method name casing (and RH4102, RH4112–RH4114, RH4116) — names dictated by a referenced assembly**

Input, with a referenced assembly declaring `abstract class MetaBase { abstract void process(); … }` and `interface IMeta { void run(); }`:
```csharp
public class Worker : MetaBase, IMeta
{
    public override void process() { }
    public override (int first, int second) pair() => default;
    public void run() { }
}

public class Leaf : Worker
{
    public override void process() { }
}
```

Before (`main`): every lowercase name reported (`process` twice, `pair`, `run`, `first`, `second`), with no usable fix.

After, A (recommended): none reported — also `Leaf.process`, whose chain reaches `MetaBase.process` through `Worker.process`.

After, B: none reported except `Leaf.process` (only a direct override or implementation counts).

Why A: `Leaf.process` cannot be renamed either; the rule specs already drew the boundary "through every level".

**8 · RH4103 · Method name casing — override of a method declared in another project of the solution**

Input: project `BaseProject` declares `public class Base { public virtual void process() { } }`; the referencing project declares:
```csharp
public class Leaf : Base
{
    public override void process() { }
}
```

Before (`main`, from `RH4103MethodNameCasingAnalyzerTests`): `Leaf.process` reported in the IDE and in tests; its fix renames `Leaf.process` and `Base.process`. In a command-line build, where `BaseProject` is a referenced assembly, the same declaration is outside the analyzer's source.

After, A (recommended): `Leaf.process` not reported anywhere; `Base.process` is reported in `BaseProject`, and its fix renames both.

After, B: `Leaf.process` reported when `Base` is source of the solution (IDE, tests), not reported when it is a compiled reference (command-line build).

Why A: the IDE and the build report the same diagnostics, and the fix on `Base.process` still renames `Leaf.process`.

**9 · RH4104 · Local function name casing — `[DllImport]` local function without `EntryPoint`**

Input:
```csharp
public int M()
{
    return getpid();

    [DllImport("libc")]
    static extern int getpid();
}
```

Before (`main`): reported, no fix offered.

After, A (recommended): not reported, like the same method under RH4103.

After, B: reported, no fix offered (as today; thoenissen/Reihitsu#1434 does not list RH4104).

Why A: the runtime binds the local function by its name exactly as it binds a method.
