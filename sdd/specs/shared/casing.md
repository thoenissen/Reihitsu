# Shared — Name casing

| | |
|---|---|
| ID prefix | SHARED-CASING |
| Status | Active |
| Code owners | `CasingAnalyzerBase`, `CasingCodeFixProviderBase<T>`, `CasingRenameSafetyUtilities`, `RecordPositionalParameterUtilities`, `ExcludedCodeUtilities`, `NamingAccessibilityClassifier`, `NamespaceCasingHelper`, `CasingUtilities` |
| Uses | SHARED-DIAG-002, SHARED-FIX-001, SHARED-FIX-002, SHARED-FIX-003 |

This spec covers the rules that require one kind of name to be written in a naming style. It defines the styles, the accessibility groups of fields and properties, how a namespace name is checked, and the code fix that renames a reported name. Each rule that uses this spec states which declarations it checks, which style it requires, where its diagnostic is located, and any exceptions of its own.

## Non-goals

- Which declarations a rule checks, the style it requires, where its diagnostic is located, and its own exceptions — each rule's spec.
- The `I` prefix of interface names and how its fix treats the prefix — RH4005.
- The naming style of type parameters — RH4121.
- Names that must not be a single letter — RH4122 to RH4127.
- Whether a type's name matches its file name — RH4001; which namespaces may be declared — RH4009.
- Tuple element names that the compiler infers from a renamed reference, such as `Value` in `(Value, 1)`: the rename changes them, and a read of such an element by its old name withholds the fix as SHARED-CASING-009 states.
- The names of the parameters and type parameters of a member whose name SHARED-CASING-014 exempts: the compiler does not require an override or an implementation to repeat them, so RH4105 and RH4121 decide them like any other.

## Requirements

### SHARED-CASING-001 — PascalCase

A name whose characters all belong to the Unicode categories Lu, Ll, Lm, Lo, Nl, Mn, Mc, Nd, or Pc MUST be accepted as PascalCase exactly when its first character is an uppercase letter (category Lu) and no other character is `_`. A combining mark (Mn, Mc), a letter number (Nl), and connector punctuation other than `_` (Pc) are accepted wherever a letter or a digit is.

Why: PascalCase is the .NET style for namespaces, types, and members that are visible outside their type. The uppercase letters inside the name mark its word boundaries, so a separator such as `_` adds nothing, and an acronym may stay uppercase. A combining mark, a letter number, and connector punctuation other than `_` are characters C# allows inside an identifier; they have no case for the style to judge, and they belong to the word they stand in.

Example — RH4002:

```csharp
class Order2
{
}

class HTTPClient
{
}

class {|RH4002:order|}
{
}

class {|RH4002:Order_Line|}
{
}

class {|RH4002:_Invoice|}
{
}
```

Example — a combining mark and a letter number, for RH4002, where `Café` is written as `Cafe` followed by U+0301 COMBINING ACUTE ACCENT, and `Ⅻ` is U+216B ROMAN NUMERAL TWELVE:

```csharp
class Café
{
}

class ClassⅫ
{
}

class {|RH4002:café|}
{
}
```

### SHARED-CASING-002 — camelCase

A name whose characters all belong to the categories that SHARED-CASING-001 names MUST be accepted as camelCase exactly when its first character is a lowercase letter (category Ll) and no other character is `_`. A combining mark, a letter number, and connector punctuation other than `_` are accepted wherever a letter or a digit is, as in SHARED-CASING-001.

Why: camelCase is the .NET style for parameters and locals. It differs from PascalCase only in the first letter, so a parameter and the property it initializes can share their words.

Example — RH4105:

```csharp
class C
{
    void M(int count2, int {|RH4105:Count|}, int {|RH4105:item_count|}, int {|RH4105:_total|})
    {
    }
}
```

### SHARED-CASING-003 — `_camelCase`

A name whose characters all belong to the categories that SHARED-CASING-001 names MUST be accepted as `_camelCase` exactly when it starts with `_` and is either just `_` or continues with a name that is camelCase as SHARED-CASING-002 defines it. Every other name MUST NOT be accepted, including a name with a second underscore or a digit after the prefix.

Why: The prefix tells a reader at the point of use that a name is state of the object, not a local or a parameter. What follows it is the camelCase name a local of the same meaning would have, so the prefix is the only difference between the two. `_` alone is the conventional name of an unused field.

Example — RH4106:

```csharp
public class C
{
    private int _;
    private int _value2;
    private int {|RH4106:__double|};
    private int {|RH4106:_Value|};
    private int {|RH4106:_1st|};
}
```

### SHARED-CASING-004 — Accessibility groups of fields and properties

A field or property declaration MUST belong to exactly one accessibility group, decided by its accessibility modifiers alone:

- `public` — the public group;
- `protected internal` — the internal group;
- `protected` or `private protected` — the protected group;
- `internal` — the internal group;
- `private` — the private group;
- no accessibility modifier — the public group in an interface, and the private group in a class, struct, or record.

An explicit interface implementation of a property MUST belong to no group. The accessibility of the containing type MUST NOT change the group.

Why: Each group has one naming rule, so every field and every property is checked against exactly one style. A `private protected` member is visible only to derived types, like a `protected` one; a `protected internal` member is visible to the whole assembly, like an `internal` one. An explicit interface implementation takes its name from the interface. The modifier is what the author wrote at the declaration; deriving the group from the containing types would change a member's style whenever its type is nested or changes its own accessibility.

Example — RH4108, which checks the internal group:

```csharp
public class C
{
    internal int {|RH4108:a|};
    protected internal int {|RH4108:b|};
    private protected int c;
    protected int d;
    public int e;
    int f;
}
```

Example — RH4106, which checks the private group:

```csharp
public class C
{
    int {|RH4106:a|};
    private int {|RH4106:b|};
}

public struct S
{
    int {|RH4106:a|};
}

public record R
{
    int {|RH4106:a|};
}

public interface I
{
    static int a;
    private static int {|RH4106:b|};
}
```

Example — RH4109, which checks the public group:

```csharp
public class C
{
    private class Nested
    {
        public int {|RH4109:value|};
    }
}

public interface I
{
    static int {|RH4109:shared|};
}
```

Example — RH4111, which checks the private group of properties:

```csharp
public interface I
{
    int value { get; }
}

public class C : I
{
    private int {|RH4111:count|} { get; set; }

    int I.value => 0;
}
```

### SHARED-CASING-005 — A namespace name is checked segment by segment

A rule that checks namespace names MUST check each identifier of a dotted namespace name on its own, and MUST report one diagnostic for each identifier that is not in the rule's style, located on that identifier.

Why: Each segment names a namespace of its own — `Company.Product` declares both `Company` and `Company.Product` — and marking the segment shows which of them is wrong.

Example — RH4008:

```csharp
namespace {|RH4008:company|}.Product.{|RH4008:data_access|}
{
}
```

### SHARED-CASING-006 — The fix derives the new name from the words of the old one

A rule's code fix MUST derive the new name from an old name whose characters all belong to the categories that SHARED-CASING-001 names, as follows. It splits the old name into words at every `_`, and drops the underscores; every other character belongs to its word and is kept, connector punctuation other than `_` included. It writes the first character of each word in uppercase. When every other character of a word is an uppercase letter, it writes them in lowercase; otherwise it keeps them. It then joins the words, which gives the new name for a PascalCase target.

For a camelCase target, it writes the leading run of uppercase letters of the first word, as the step above wrote it, in lowercase, before it joins the words. When that run has two or more letters and a lowercase letter of the same word follows it directly, the run's last letter keeps its case. The run never reaches into the second word: `A_B` gives `aB`, and `m_HTTPClient` gives `mHTTPClient`. For a `_camelCase` target, that camelCase name gets a `_` in front.

In these steps, a combining mark (Mn, Mc) counts as part of the character in front of it: it is kept as written, it does not keep a word from consisting of uppercase letters, and it neither ends a run of uppercase letters nor is the lowercase letter that follows one. A letter number (Nl) and connector punctuation other than `_` count like a digit: they are kept as written, and they keep a word from consisting of uppercase letters and end a run of uppercase letters.

This requirement covers an old name whose first word, the first non-empty part between underscores, starts with a letter whose one-character uppercase and lowercase mappings, in the invariant culture, differ from each other. A first word that starts with a digit, with a letter without case such as `注`, with a letter such as `ß` that has no one-character uppercase mapping, or with a letter number such as `Ⅻ`, is not covered. For such an old name, SHARED-CASING-008 decides whether the fix is offered.

Why: A separator such as `_`, or the `_` of a leading `m_`, is a word boundary in common naming styles. A word in uppercase letters only (`FIRST_NAME`, `URL`) is a shouted word, not an acronym inside a PascalCase name. Uppercase letters inside a word are kept, because they already mark the word boundaries of a PascalCase or camelCase name, and a PascalCase name may keep an acronym (SHARED-CASING-001). A camelCase name starts with a lowercase word, so an acronym at its start is written in lowercase as a whole (`httpClient`, not `hTTPClient`); the last uppercase letter in front of a lowercase letter is the first letter of the next word (`Client`), and keeps its case. The run is taken from the first word only, so a word boundary the author marked with `_` is never merged into an acronym. A combining mark modifies the letter in front of it and has no case of its own, so `CAFÉ` is a word in uppercase letters whether `É` is one character or `E` with U+0301; the fix never drops a character, because a character the author wrote is part of the name. Deriving the name mechanically keeps the result predictable across every rule.

Example — RH4110, PascalCase:

```csharp
public class C
{
    private const int {|RH4110:MAX_RETRIES|} = 3;
    private const int {|RH4110:retry_delay|} = 5;
    private const int {|RH4110:timeout|} = 7;

    public int Sum => MAX_RETRIES + retry_delay + timeout;
}
```

```csharp
public class C
{
    private const int MaxRetries = 3;
    private const int RetryDelay = 5;
    private const int Timeout = 7;

    public int Sum => MaxRetries + RetryDelay + Timeout;
}
```

Example — RH4105, camelCase:

```csharp
class C
{
    int M(int {|RH4105:FirstName|}, int {|RH4105:LAST_NAME|}, int {|RH4105:Input_Value|})
    {
        return FirstName + LAST_NAME + Input_Value;
    }
}
```

```csharp
class C
{
    int M(int firstName, int lastName, int inputValue)
    {
        return firstName + lastName + inputValue;
    }
}
```

Example — RH4106, `_camelCase`:

```csharp
public class C
{
    private int {|RH4106:FirstName|};
    private int {|RH4106:LAST_NAME|};
    private int {|RH4106:m_value|};
    private int {|RH4106:__double|};
    private int {|RH4106:URL|};
    private int {|RH4106:iPhone|};

    public int Sum => FirstName + LAST_NAME + m_value + __double + URL + iPhone;
}
```

```csharp
public class C
{
    private int _firstName;
    private int _lastName;
    private int _mValue;
    private int _double;
    private int _url;
    private int _iPhone;

    public int Sum => _firstName + _lastName + _mValue + _double + _url + _iPhone;
}
```

Example — a leading acronym, for RH4106, `_camelCase`:

```csharp
public class C
{
    private int {|RH4106:HTTPClient|};
    private int {|RH4106:XMLParser|};
    private int {|RH4106:IOStream|};
    private int {|RH4106:HTTP2Client|};
    private int {|RH4106:ABc|};
    private int {|RH4106:URLs|};

    public int Sum => HTTPClient + XMLParser + IOStream + HTTP2Client + ABc + URLs;
}
```

```csharp
public class C
{
    private int _httpClient;
    private int _xmlParser;
    private int _ioStream;
    private int _http2Client;
    private int _aBc;
    private int _urLs;

    public int Sum => _httpClient + _xmlParser + _ioStream + _http2Client + _aBc + _urLs;
}
```

The leading run of `HTTP2Client` is `HTTP`: the digit ends it, so the whole run is written in lowercase. In `FirstName` above, the run `F` has one letter and is written in lowercase although a lowercase letter follows it. In `URLs`, the `s` follows the run `URL` directly, so its last letter keeps its case.

Example — a leading run that ends with the first word, for RH4106, `_camelCase`:

```csharp
public class C
{
    private int {|RH4106:A_B|};
    private int {|RH4106:A_B_Cd|};
    private int {|RH4106:m_HTTPClient|};
    private int {|RH4106:X_Y_Z|};
    private int {|RH4106:XML_Parser|};

    public int Sum => A_B + A_B_Cd + m_HTTPClient + X_Y_Z + XML_Parser;
}
```

```csharp
public class C
{
    private int _aB;
    private int _aBCd;
    private int _mHTTPClient;
    private int _xYZ;
    private int _xmlParser;

    public int Sum => _aB + _aBCd + _mHTTPClient + _xYZ + _xmlParser;
}
```

The first word of each name is `A`, `M`, `X`, or `Xml` after the first step, so the run is that word's leading uppercase letters alone; the words behind it keep their case. `_mHTTPClient` keeps the acronym of its second word, as a PascalCase name does.

Example — a leading acronym, for RH4105, camelCase:

```csharp
class C
{
    int M(int {|RH4105:XMLParser|})
    {
        return XMLParser;
    }
}
```

```csharp
class C
{
    int M(int xmlParser)
    {
        return xmlParser;
    }
}
```

Example — a leading acronym in a PascalCase target, for RH4110, which keeps it:

```csharp
public class C
{
    private const int {|RH4110:HTTPClient_timeout|} = 30;

    public int Get() => HTTPClient_timeout;
}
```

```csharp
public class C
{
    private const int HTTPClientTimeout = 30;

    public int Get() => HTTPClientTimeout;
}
```

For a `_camelCase` target, the same old name becomes `_httpClientTimeout`.

Example — characters that belong to their word, for RH4002 and RH4110, where `é` and `É` are written as `e` and `E` followed by U+0301 COMBINING ACUTE ACCENT, and `‿` is U+203F UNDERTIE (category Pc):

```csharp
class {|RH4002:café_bar|}
{
}

class {|RH4002:order‿line|}
{
}

public class C
{
    private const int {|RH4110:CAFÉ_NAME|} = 1;

    public int Get() => CAFÉ_NAME;
}
```

```csharp
class CaféBar
{
}

class Order‿line
{
}

public class C
{
    private const int CaféName = 1;

    public int Get() => CaféName;
}
```

U+0301 is kept in its word, and it does not keep `CAFÉ` from being a word in uppercase letters. `‿` is not a separator: `order‿line` is one word.

### SHARED-CASING-007 — The fix renames the declaration and every reference

The code fix MUST rename the reported declaration's symbol with the Roslyn renamer, so that the declaration and every reference to it in the active code of the solution change together. It MUST NOT be offered when the declared symbol cannot be resolved.

The symbols that one such rename renames are the reported symbol's *rename cascade*: the symbol itself and every symbol the renamer renames with it, in whichever document or project of the solution that symbol is declared. Which symbols these are is the renamer's own determination, not a list of relations; for example, a member the symbol overrides, a member that overrides it, an interface member it implements, an implementation of it, and the other part of a partial method or a partial indexer together with its parameter at the same position, however that part spells it, and the other part of a partial method together with its type parameter of the same name, each of these in turn with the symbols the renamer renames with it. A member declared in a referenced assembly is not renamed and is not part of the cascade. Every other statement of this spec about the rename cascade refers to this one.

This requirement covers an old name that SHARED-CASING-006 covers. It further covers only a rename for which all of the following hold:

- Neither the old name of a symbol of the rename cascade nor the new name occurs as an identifier in code that an `#if`, `#elif`, or `#else` branch excludes, in a document where a symbol of the rename cascade can be referred to. The old names are those of every symbol of the cascade that source can refer to by its name, not only the reported symbol's: a symbol of the cascade that is spelled differently from the reported one, such as the parameter of the other part of a partial method, adds its own spelling. A symbol whose name source never writes, such as a property's accessor or a type's constructor, adds no old name, and neither does a symbol outside the cascade, such as a type parameter of the other part of a partial method that this part spells differently, which the renamer leaves as it is. A document where no symbol of the cascade can be referred to is outside the region, even when its excluded code names an old name or the new name.
- For no record does the rename change which parameters of its primary constructor have a member of the same name, declared in the record or inherited from its base type. The compiler declares a property for a positional parameter only when no such member exists, so a rename that changes the match adds or removes a public member.
- Neither SHARED-CASING-011 nor SHARED-CASING-012 withholds the rename. Those requirements name renames that can change at run time which method a construct calls, or which expression text a parameter receives, without a compile error and without a reference the renamer could change.
- SHARED-CASING-013 does not withhold the rename: the new name is written without the verbatim prefix `@` at the declaration and at every reference. Contextual keywords such as `value`, `var`, and `nameof` are covered and stay as they are.

A rename outside this coverage is withheld where SHARED-FIX-003 withholds it, and always where SHARED-CASING-011, SHARED-CASING-012, or SHARED-CASING-013 withholds it.

Why: Renaming only the declaration would leave every use pointing at a name that no longer exists.

Example — RH4103:

```csharp
public class Calculator
{
    public int {|RH4103:computeTotal|}(int value)
    {
        return computeFactor(value) * 2;
    }

    private int {|RH4103:computeFactor|}(int value)
    {
        return value + 1;
    }
}
```

```csharp
public class Calculator
{
    public int ComputeTotal(int value)
    {
        return ComputeFactor(value) * 2;
    }

    private int ComputeFactor(int value)
    {
        return value + 1;
    }
}
```

Example — a name in excluded code, for RH4106:

```csharp
public class C
{
    private int {|RH4106:Value|};

    public int Get()
    {
#if NEVER
        return Value + 1;
#else
        return Value;
#endif
    }
}
```

is reported, and no code fix is offered: the rename would leave `Value` in the excluded branch, which does not compile in a build that defines `NEVER` (SHARED-FIX-003). The same holds when the excluded branch declares `_value`, the new name. When the excluded branch names neither `Value` nor `_value`, the fix is offered and renames the field and the active reference.

Example — a name in excluded code reached through the rename cascade, for RH4103, in two documents:

```csharp
public class Base
{
    public virtual void {|RH4103:process|}()
    {
    }

    public void Run()
    {
#if NEVER
        process();
#endif
    }
}
```

```csharp
file class Leaf : Base
{
    public override void {|RH4103:process|}()
    {
    }
}
```

Both diagnostics are reported, and no code fix is offered for either: renaming `Leaf.process` renames `Base.process` with it, and `Base.process` can be referred to in the first document, whose excluded branch names `process`. That `Leaf` is file-local and cannot be referred to in the first document does not matter. The same holds when the overriding type is a private nested type, and for an implementation of an interface method. When `Base` is declared in another project of the solution, `Leaf.process` is not reported (SHARED-CASING-014), and no code fix is offered for `Base.process`, whose rename renames `Leaf.process` with it. When the excluded branch names neither `process` nor `Process`, the fix is offered and renames both methods.

Example — a name in excluded code reached through the other part of a partial method, for RH4121, in two documents:

```csharp
public partial class C
{
    public void Use()
    {
        Run<int>(1);
    }

    partial void Run<{|RH4121:Key|}>(Key value);
}
```

```csharp
public partial class C
{
    partial void Run<{|RH4121:Key|}>(Key value)
    {
#if NEVER
        Key copy = value;
#endif
    }
}
```

Both diagnostics are reported, and no code fix is offered for either: renaming the type parameter `Key` of either part renames the type parameter of the other part with it, and the excluded branch of the second document names `Key`. That the type parameter of the first part cannot be referred to in the second document does not matter. The same holds in the other direction, when the excluded code that names `Key` stands in the first document, for example `#if NEVER` around `[System.Obsolete(nameof(Key))]` on the declaration without a body, and the second document has none. When no excluded branch names `Key` or `TKey`, the fix is offered and renames the type parameter and its references in both parts to `TKey`.

Example — an old name of a differently spelled part in excluded code, for RH4105, in two documents:

```csharp
public partial class C
{
    public void Use()
    {
        Run(1);
    }

    partial void Run(int {|RH4105:Arg|});
}
```

```csharp
public partial class C
{
    partial void Run(int other)
    {
#if NEVER
        other++;
#endif
    }
}
```

is reported, and no code fix is offered: renaming `Arg` to `arg` renames the parameter `other` of the second part with it, and the excluded branch of the second document names `other`, which no longer exists after the rename. That `other` is not the reported name does not matter. The same holds in the other direction, when the second part spells the parameter `Other`, which is reported, and the first part carries `#if NEVER` around `[System.Obsolete(nameof(Arg))]` on its declaration: no code fix is offered for `Other`. It holds as well for the parameter of a partial indexer, `this[int Index]` in one part and `this[int position]` in the other, when an excluded branch of the second part names `position`. When no excluded branch names `Arg`, `other`, or `arg`, the fix is offered and renames the parameter in both parts and its references to `arg`.

When the parts of a partial method spell a type parameter differently, `Run<Key>` in one part and `Run<TItem>` in the other, the renamer renames only the reported part, so `TItem` is not an old name: an excluded branch of the second part that names `TItem` does not withhold the fix on `Key`, and the fix renames `Key` to `TKey` in the first part only.

Example — excluded code of an unrelated type, for RH4106, in two documents:

```csharp
public class C
{
    private int {|RH4106:Value|};

    public int Get()
    {
        return Value;
    }
}
```

```csharp
public class D
{
#if NEVER
    private int Value;
    private int _value;

    public int Other()
    {
        return Value + _value;
    }
#endif
}
```

The fix is offered and renames `C.Value` and its reference to `_value`; the second document stays unchanged. The rename cascade of the private field `C.Value` is the field alone, and it cannot be referred to in the second document, so the excluded branch there is outside the region.

### SHARED-CASING-008 — The fix is withheld when no usable name can be derived

The code fix MUST NOT be offered when the old name has no letter or digit, when the derived name is the old name itself or is not a valid identifier because it starts with a digit, or when the rule would still report the declaration under the derived name. The derived name is the name that the steps of SHARED-CASING-006 derive, or, for a rule whose fix requirement states a derivation of its own, such as RH4005 and RH4121, the name that derivation gives. The diagnostic is still reported.

The last condition holds for every old name, also for one that SHARED-CASING-006 does not cover. It decides those names: an old name whose first word starts with a digit, with a letter without case such as `注`, or with a letter whose mapping to the target case is not a letter of that case, such as `ϒ` for a camelCase target, is offered the fix only when the rule accepts the name derived from it.

Why: A name such as `__` has no words to build a name from. A derived name that equals the old one changes nothing, and one that is not an identifier does not compile. A derived name that the rule still reports corrects nothing in one application (SHARED-FIX-001), and a second application, if it changes the name at all, gives a name the first one should have given. For most of these old names, no name in the rule's style can be built from the old one without dropping or inventing characters, so the name is left for the author to choose.

Example — RH4105:

```csharp
class C
{
    void M(int {|RH4105:_1st|}, int {|RH4105:__|})
    {
    }
}
```

is reported, and no code fix is offered for either parameter: `__` has no letter, and `_1st` would become `1st`, which is not an identifier.

Example — RH4106:

```csharp
public class C
{
    private int {|RH4106:_1st|};
}
```

is reported, and no code fix is offered, because the derived name is `_1st` again.

Example — derived names that the rule still reports, for RH4106:

```csharp
public class C
{
    private int {|RH4106:__1st|};
    private int {|RH4106:_1_value|};
    private int {|RH4106:注文|};
    private int {|RH4106:注文_明細|};
    private int {|RH4106:ϒ_x|};
}
```

is reported, and no code fix is offered for any field: the derived names `_1st`, `_1Value`, `_注文`, `_注文明細`, and `_ϒx` are not in `_camelCase`, because a digit, a letter without case, or the uppercase letter `ϒ` follows the prefix, so RH4106 still reports each of them.

Example — derived names that the rule still reports, for RH4002 and RH4105:

```csharp
class {|RH4002:_注文|}
{
    void M(int {|RH4105:_ϒx|})
    {
    }
}
```

is reported, and no code fix is offered for either: `注文` does not start with an uppercase letter, and `ϒx` does not start with a lowercase letter, because `ϒ` has no lowercase mapping.

Example — a derived name that the rule accepts, for RH4106, where the first word starts with `ß`:

```csharp
public class C
{
    private int {|RH4106:ß_x|};
}
```

```csharp
public class C
{
    private int _ßX;
}
```

`ß` is a lowercase letter, so `_ßX` is in `_camelCase`, and the fix is offered.

### SHARED-CASING-009 — The fix is withheld when the new name would collide

The code fix MUST NOT be offered when renaming to the derived name would collide with an existing name: when the renamer reports a conflict, or when the result of the rename has a compile error that the code did not have, as SHARED-FIX-003 states. A compile error counts wherever the rename puts it, not only inside the declarations that contain the renamed symbol:

- inside the renamed type itself, whose member, nested type, type parameter, or positional parameter now has the type's name, where that declaration is a compile error — also for a type declared directly in a namespace or at file level, and for a partial type whose colliding member is declared in another part;
- in the namespace that declares the renamed type, when another type of that namespace in source already has the new name;
- in every other type that declares a symbol of the rename cascade (SHARED-CASING-007), also where that type is declared in another document.

A type with the new name in the same namespace makes the rename fail to compile only when both types have the same number of type parameters, and, when either of them is file-local, only when both are declared in the same file; otherwise the fix is offered. A declaration that has the type's name is not a compile error in every type: an enum member may have the name of its enum, and a method of an interface the name of its interface, so neither collides with it, and the fix is offered. The diagnostic is still reported.

The code fix MUST NOT be offered either when the rename would change which symbol an existing reference binds to, where that reference refers to a symbol outside the rename cascade (SHARED-CASING-007), in whichever document of the solution it stands. This holds in both of the ways such a binding can change:

- **Rewritten.** The renamer rewrites the reference so that it keeps its binding, without reporting a conflict: it qualifies a name with `this.`, `base.`, a type, or a namespace, turns an extension-method call into a static call, or casts an argument. The reference may be a simple name, a member access, a type name, a `nameof` operand, an invocation whose overload or extension method the new name would win, or an enum member's initializer.
- **Left as written.** The renamer leaves the reference's text unchanged, and the reference then binds to the renamed symbol: for example a member access whose receiver has the renaming type, or a type derived from it, where the rename makes the renamed member hide a member of a base type that the access called before. The compiler reports at most a warning.

A reference to a symbol of the rename cascade is renamed with it and is not such a reference. Which symbol a reference refers to decides this, not how it is spelled: a reference to a field `value` refers to a symbol outside the cascade even where the other part of a partial method spells its parameter, which the cascade renames, `value`; and so does a reference to a type `TItem` where the other part of a partial method spells its type parameter `TItem`, which the renamer leaves as it is (SHARED-CASING-007). A reference to another symbol whose binding the rename leaves unchanged, such as a call of an extension method in a namespace that the renamed method's namespace does not import, does not withhold the fix.

The code fix MUST NOT be offered either when the renamer would rewrite a reference to a symbol of the rename cascade beyond renaming it and qualifying its name: for example when it turns a call of the renamed extension method into a static call, or casts an argument of a call of the renamed method, so that the call keeps binding to the renamed symbol. A reference that the renamer renames, or renames and qualifies with `this.`, `base.`, a type, or a namespace, does not withhold the fix.

The code fix MUST NOT be offered either when a method, property, or event of the rename cascade (SHARED-CASING-007) would get the name that a member of an interface already has, where that interface is implemented by the type that declares that cascade member or by a type in the solution that derives from it — when that type is an interface, also by a type that implements it or an interface that inherits from it. This holds for every method, property, and event that the rename cascade renames, not only for the reported one: for example also for a member of another type that implements the same interface member, for the member of a base type that the reported member overrides, and for the interface member that the reported member implements. A type implements every interface it lists among its base types and every interface it inherits through a base type or a base interface. This holds whether or not a member of the cascade would implement that interface member after the rename, whatever implements the interface member today — an inherited member, a default implementation in the interface, or a member of a base type that lists the interface again — and whether or not a reference names it. It does not hold when no member of these interfaces has the new name for any method, property, or event of the cascade, such as when no type that declares a member of the cascade implements the interface or has a type derived from it that does, or when the interfaces those types implement have no member of the new name. The members of the cascade this paragraph checks are those that can implement an interface member by their name: its properties, its events, and its ordinary methods, that is, methods declared with a name of their own. Every other symbol of the cascade is not checked: a constructor, static constructor, finalizer, operator, conversion operator, accessor, explicit interface implementation of a method, or local function, and a field, parameter, local variable, type, or type parameter. So a renamed class, struct, record, or record struct is not withheld by this paragraph, although its rename cascade holds its constructors; and a renamed local function is not withheld, whatever interfaces the type that encloses it implements. A renamed symbol that is not checked itself is still withheld when its rename cascade holds a property, event, or ordinary method that this paragraph withholds: for example a positional parameter of a record, whose rename cascade holds the property the compiler declares for it.

So the fix changes no code other than the declarations of the rename cascade, the names of its references, and the qualifiers in front of those names. The diagnostic is still reported.

Why: A collision cannot be resolved without knowing which of the two names the author meant, so it is left to the author. The renamer renames every symbol of the rename cascade (SHARED-CASING-007), in whichever type and document it is declared, but a member declared in a referenced assembly keeps its name, so a compile error can appear in a type that does not contain the renamed declaration. Types with a different number of type parameters, and a file-local type and a type of another file, can share a name, so renaming to it is safe. A rename whose new name meets an existing binding is the same collision even when it compiles: a reference that the renamer rewrites is an edit of code the diagnostic does not name, and a reference that the renamer leaves as written calls another member at run time (SHARED-FIX-003). A call of a renamed member whose shape the renamer changes, into a static call or with a cast argument, is likewise an edit beyond the rename that the diagnostic asks for. A member that starts to implement an interface member makes a call through the interface reach another member at run time, although no reference in source changes its binding (SHARED-FIX-003). Whether a rename does that depends on the interface mapping of every type that inherits a member of the rename cascade, in any project of the solution, and on default implementations and interfaces that a derived type lists again; telling a rename that changes one of these mappings from one that does not would mean comparing all of them before and after the rename. The cascade renames members of other types too — implementers of the same interface member, the overridden member of a base type, the interface member itself — so their hierarchies count like that of the reported member, as for every other check of the rename. A wrongly cased member whose corrected name is the name of an interface member in the hierarchy of a type that declares a member of its rename cascade is rare, so the fix is withheld for every such name, also where no mapping would change. Only a property, an event, or an ordinary method can start to implement an interface member by its name. A constructor, finalizer, or local function belongs to no interface mapping, and an operator or conversion operator implements an interface operator by its operator token or its conversion types, which a rename does not change; an accessor implements an interface accessor only through its property or event, which the cascade holds and the check covers; and an explicit interface implementation names the interface member it implements, which the cascade holds too, so the hierarchy of that interface member, which holds the implementing type, is checked anyway. Checking only the kinds that can implement an interface member keeps the check independent of which other symbols the renamer cascades to: the rename of a type, whose cascade holds its constructors, keeps its fix, and so does that of a local function. A positional record parameter is renamed together with its property, and that property can start to implement an interface member, so the parameter's fix is withheld like the property's. A reference is matched to the cascade by what it refers to, because a name of the cascade can also be the name of another symbol in the same scope. Either way the author decides which name to give up. A wrongly cased declaration whose corrected spelling is exactly a name already bound at a reference in its scope, or that of a member of an interface implemented in the hierarchy of a type that declares a member of its rename cascade, is uncommon, and a diagnostic without a fix is a supported end state.

Example — RH4106:

```csharp
public class D
{
    private int {|RH4106:Value|};
    private int _value;
}
```

is reported, and no code fix is offered, because `_value` already exists.

Example — RH4002, with this file:

```csharp
namespace N
{
    class {|RH4002:order|}
    {
    }
}
```

and another file of the same project that declares `class Order` in `namespace N`: no code fix is offered. When the other file declares `file class Order` instead, or when the reported class is `order<T>`, the fix is offered and renames the class to `Order` or `Order<T>`. When `file class Order` is declared in the same file as `order`, no code fix is offered.

Example — RH4002, for a nested type:

```csharp
class Outer
{
    class {|RH4002:order|}
    {
        public int Order;
    }
}
```

is reported, and no code fix is offered, because renaming the class to `Order` would add a compile error: a member cannot have the name of its enclosing type.

Example — RH4002 and RH4006, for types declared directly in a namespace:

```csharp
namespace N
{
    class {|RH4002:order|}
    {
        public int Order;
    }

    delegate void {|RH4006:handler|}<Handler>();
}
```

is reported, and no code fix is offered for either type: `Order` would have a member named like the class, and `Handler` a type parameter named like the delegate. The same holds for a type declared at file level, in a file-scoped namespace, or as a `partial` type whose member `Order` is declared in another part.

Example — RH4004, for an enum member:

```csharp
enum {|RH4004:color|}
{
    Color,
}
```

```csharp
enum Color
{
    Color,
}
```

Example — RH4005, for a method of an interface declared directly in a namespace:

```csharp
namespace N
{
    interface {|RH4005:iOrder|}
    {
        void IOrder();
    }
}
```

```csharp
namespace N
{
    interface IOrder
    {
        void IOrder();
    }
}
```

Example — RH4103, for a member of another type of the rename cascade, with a referenced assembly that declares `public abstract class MetaBase { public abstract void process(); }`:

```csharp
public interface IWorker
{
    void {|RH4103:process|}();
}

public class Middle : MetaBase, IWorker
{
    public override void process()
    {
    }
}
```

is reported, and no code fix is offered: renaming `IWorker.process` would rename its implementation `Middle.process` with it, and `Middle` would then fail to compile, because `MetaBase.process` keeps its name and `Middle.Process` overrides nothing. `Middle.process` itself is not reported (SHARED-CASING-014). The same holds for an interface property or event that a property or an event implements which overrides a member declared in a referenced assembly. When `MetaBase` is declared in source, the rename renames all three methods together, and the fix is offered.

Example — RH4103, a rename that would win an extension-method call:

```csharp
using System.Collections.Generic;
using System.Linq;

namespace N
{
    public static class Extensions
    {
        public static List<int> {|RH4103:toList|}(this IEnumerable<int> source) => new List<int>();
    }

    public class User
    {
        public List<int> Get(int[] values) => values.ToList();
    }
}
```

is reported, and no code fix is offered: `values.ToList()` calls `Enumerable.ToList` today, and the renamer would rewrite it to `Enumerable.ToList(values)` so that it keeps calling it. The same holds for an instance method `first()` of a type that implements `IEnumerable<int>`, next to a call `bag.First()`.

When `User` is declared in another namespace, `M`, that does not import `N`:

```csharp
namespace M
{
    public class User
    {
        public List<int> Get(int[] values) => values.ToList();
    }
}
```

the call keeps binding to `Enumerable.ToList` after the rename, and the fix is offered: it renames `toList` to `ToList` and leaves the call as written.

Example — RH4103, a rename that would win overload resolution:

```csharp
public class Box
{
    public void {|RH4103:write|}(int value)
    {
    }

    public void Write(object value)
    {
    }

    public void Use(Box box) => box.Write(1);
}
```

is reported, and no code fix is offered: `box.Write(1)` calls `Write(object)` today, and the renamer would rewrite it to `box.Write((object)1)`.

Example — RH4105, a parameter that would hide a field:

```csharp
public class Person
{
    private string name;

    public void Rename(string {|RH4105:Name|})
    {
        name = Name.Trim();
    }
}
```

is reported, and no code fix is offered: `name` refers to the field today, and the renamer would rewrite it to `this.name`. The same holds for a local variable (RH4115), for a static field that the renamer would qualify with its type, and for a type that the renamer would qualify with its namespace, such as a class `order` (RH4002) whose namespace already uses a class `Order` imported from another namespace.

Example — RH4103, a rename that would hide an inherited member:

```csharp
public class Base
{
    public int Process() => 1;
}

public class Derived : Base
{
    public int {|RH4103:process|}() => 2;
}

public class User
{
    public int Use(Derived derived) => derived.Process();
}
```

is reported, and no code fix is offered: `derived.Process()` calls `Base.Process` and returns `1` today. After the rename the renamer leaves it as written, and it would call `Derived.Process` and return `2`. The same holds for a property (RH4114) or a field (RH4109) that the rename would make hide a member of a base type.

Example — RH4105, a reference to another symbol spelled like a name of the cascade:

```csharp
public partial class Counter
{
    private static int value;

    public partial int Get(int value);

    public partial int Get(int {|RH4105:Value|})
    {
        var total = Value;
        total += 2;
        return total + value;
    }
}
```

is reported, and no code fix is offered: the rename gives the parameter the name `value`, and the renamer would rewrite the field reference `value` to `Counter.value`. That the other part already spells its parameter `value` does not make the field reference one of the cascade. The same holds for an instance field, which the renamer would rewrite to `this.value`, and for `nameof(value)`, which it would rewrite to `nameof(Counter.value)`. It holds as well for RH4121, with a class `N.TItem` and a partial method `Count<TItem>()` whose other part declares `Count<{|RH4121:item|}>()` and calls `TItem.Size()`: the renamer would rewrite the call to `N.TItem.Size()`.

Example — RH4103, a call of the renamed extension method that the renamer would turn into a static call:

```csharp
using System.Collections.Generic;

namespace N
{
    public static class Extensions
    {
        public static List<int> {|RH4103:toList|}(this IEnumerable<int> source) => new List<int>();
    }

    namespace Inner
    {
        using System.Linq;

        public class User
        {
            public List<int> Get(int[] values) => values.toList();
        }
    }
}
```

is reported, and no code fix is offered: after the rename, `values.ToList()` would call `Enumerable.ToList`, and the renamer would rewrite the call to `Extensions.ToList(values)` so that it keeps calling the renamed method.

Example — RH4103, a call of the renamed method that the renamer would give a cast argument:

```csharp
public class Box
{
    public void {|RH4103:write|}(object value)
    {
    }

    public void Write(int value)
    {
    }

    public void Use(Box box) => box.write(1);
}
```

is reported, and no code fix is offered: after the rename, `box.Write(1)` would call `Write(int)`, and the renamer would rewrite the call to `box.Write((object)1)`. A reference of the cascade that the renamer only qualifies, such as a class `order` (RH4002) referenced as `order` in a type that declares a nested class `Order`, which the renamer rewrites to `N.Order`, does not withhold the fix.

Example — RH4103, a rename that would change which method implements an interface method:

```csharp
public interface IProcessor
{
    int Process();
}

public class Base : IProcessor
{
    public int Process() => 1;
}

public class Derived : Base, IProcessor
{
    public int {|RH4103:process|}() => 2;
}
```

is reported, and no code fix is offered: `((IProcessor)new Derived()).Process()` calls `Base.Process` and returns `1` today. `Derived` lists `IProcessor` among its own base types, so after the rename `Derived.Process` would implement `IProcessor.Process`, and the call would return `2`. The same holds for a property (RH4114) or an event (RH4102) whose new name is that of an interface property or event. No code fix is offered either when `Derived` is declared as `class Derived : Base`: `Base.Process` would keep implementing `IProcessor.Process`, but `Derived` still implements `IProcessor` through `Base`, and `IProcessor` has a member named `Process`.

Example — RH4103, a type derived from the renamed method's type that lists the interface:

```csharp
public interface IProcessor
{
    int Process();
}

public class Root
{
    public int Process() => 1;
}

public class Base : Root
{
    public int {|RH4103:process|}() => 2;
}

public class Derived : Base, IProcessor
{
}
```

is reported, and no code fix is offered: `Base` implements no interface, but `Derived` derives from it and implements `IProcessor`, which has a member named `Process`. `((IProcessor)new Derived()).Process()` calls `Root.Process` and returns `1` today, and would call `Base.Process` and return `2` after the rename. The same holds without `Root`, when `IProcessor` declares `int Process() => 1;` as a default implementation and `Base` derives from nothing; when `Root` itself implements `IProcessor` and `Derived` lists it again; and for a property (RH4114) or an event (RH4102) in the same shape.

Example — RH4103, a type outside the hierarchy of every type that implements the interface:

```csharp
public interface IProcessor
{
    int Process();
}

public class Worker : IProcessor
{
    public int Process() => 1;
}

public class Job : System.IDisposable
{
    public void Dispose()
    {
    }

    public int {|RH4103:process|}() => 2;
}
```

```csharp
public interface IProcessor
{
    int Process();
}

public class Worker : IProcessor
{
    public int Process() => 1;
}

public class Job : System.IDisposable
{
    public void Dispose()
    {
    }

    public int Process() => 2;
}
```

`Job` neither implements `IProcessor` nor has a type derived from it that does, and `IDisposable`, which it implements, has no member named `Process`.

Example — RH4103, another implementer of the same interface method, whose type implements an interface with a member of the new name:

```csharp
public interface IProcessor
{
    int {|RH4103:process|}();
}

public interface IOther
{
    int Process() => 1;
}

public class A : IProcessor
{
    public int {|RH4103:process|}() => 0;
}

public class B : IProcessor, IOther
{
    public int {|RH4103:process|}() => 2;
}
```

is reported, and no code fix is offered for any of the three methods. `A` implements no interface with a member named `Process`, but the rename cascade of `A.process` holds `IProcessor.process` and `B.process`, and `B` implements `IOther`. `((IOther)new B()).Process()` calls the default implementation and returns `1` today, and would call `B.Process` and return `2` after the rename. The same holds for a property (RH4114) or an event (RH4102) in the same shape.

Example — RH4103, the overridden method of a base type, which another derived type of the base implements an interface for:

```csharp
public interface IOther
{
    int Process() => 1;
}

public class Base
{
    public virtual int {|RH4103:process|}() => 2;
}

public class Derived : Base
{
    public override int {|RH4103:process|}() => 3;
}

public class Sibling : Base, IOther
{
}
```

is reported, and no code fix is offered for either method. `Derived` implements no interface, but the rename cascade of `Derived.process` holds `Base.process`, and `Sibling` derives from `Base` and implements `IOther`. `((IOther)new Sibling()).Process()` returns `1` today and would call `Base.Process` and return `2` after the rename. The same holds for a property (RH4114) or an event (RH4102) in the same shape.

Example — RH4104, a local function in a type that implements an interface with a member of the new name:

```csharp
public interface IProcessor
{
    int Process() => 1;
}

public class Worker : IProcessor
{
    public int Run()
    {
        return process();

        int {|RH4104:process|}() => 2;
    }
}
```

```csharp
public interface IProcessor
{
    int Process() => 1;
}

public class Worker : IProcessor
{
    public int Run()
    {
        return Process();

        int Process() => 2;
    }
}
```

A local function cannot implement `IProcessor.Process`, so the rename changes no interface mapping, and the fix is offered. The same holds for a local function `dispose` in a type that implements `System.IDisposable`.

Example — RH4002, a type whose new name is that of a member of an interface it implements:

```csharp
public interface IHasProcessor
{
    int Processor => 1;
}

public class {|RH4002:processor|} : IHasProcessor
{
}
```

```csharp
public interface IHasProcessor
{
    int Processor => 1;
}

public class Processor : IHasProcessor
{
}
```

The rename cascade of `processor` holds its implicit constructor, but a constructor cannot implement `IHasProcessor.Processor`, and a type is not an interface member, so the fix is offered. The same holds when a type derived from `processor` lists `IHasProcessor` instead, and for a struct (RH4003), a record (RH4010), or a record struct (RH4011) in the same shape.

Example — RH4120, a positional record parameter whose property would implement an interface property:

```csharp
public interface INamed
{
    string Name => "default";
}

public record Person(string {|RH4120:name|}) : INamed;
```

is reported, and no code fix is offered. The rename cascade of the parameter `name` holds the property the compiler declares for it, and `Person` implements `INamed`, which has a member named `Name`. `((INamed)new Person("x")).Name` returns `"default"` today, and would return `"x"` after the rename, because the declared property `Name` would implement `INamed.Name`. The fix is offered when `INamed` has no member named `Name`.

### SHARED-CASING-010 — Fix All skips names that would collide with each other

The Fix All provider MUST apply every rename that is safe on its own and in combination with the others. It MUST skip every member of a group of reported names that would receive the same new name, are declared in the same containing symbol, and whose renames conflict when both are applied. The skipped declarations keep their diagnostics. For such a group, the result differs from applying the single fixes one after another (SHARED-FIX-002): those rename the first member of the group and are then withheld for the others, as SHARED-CASING-009 specifies.

Why: Applied one at a time, the first of two colliding names would win and the second would be refused, so the result would depend on the order of the fixes. Skipping the whole group leaves the decision to the author. Names that receive the same new name in different containing symbols, or in sibling scopes of one symbol, do not collide and are renamed.

Example — RH4107:

```csharp
public class C
{
    protected int {|RH4107:Total|};

    protected int {|RH4107:total|};

    protected int {|RH4107:Limit|};

    public int Sum => Total + total + Limit;
}
```

```csharp
public class C
{
    protected int {|RH4107:Total|};

    protected int {|RH4107:total|};

    protected int _limit;

    public int Sum => Total + total + _limit;
}
```

Only `Limit` is renamed; `Total` and `total` would both become `_total` and keep their diagnostics.

Example — RH4105, where the two parameters are declared in different containing symbols:

```csharp
public class Handler
{
    public int Handle(int {|RH4105:InputValue|})
    {
        int Compute(int {|RH4105:Input_Value|})
        {
            return Input_Value + 1;
        }

        return Compute(InputValue);
    }
}
```

```csharp
public class Handler
{
    public int Handle(int inputValue)
    {
        int Compute(int inputValue)
        {
            return inputValue + 1;
        }

        return Compute(inputValue);
    }
}
```

Example — RH4115, where the two locals are declared in sibling scopes of one method:

```csharp
internal class C
{
    void M(bool condition)
    {
        if (condition)
        {
            int {|RH4115:BadName|} = 1;
        }

        if (!condition)
        {
            int {|RH4115:BAD_NAME|} = 2;
        }
    }
}
```

```csharp
internal class C
{
    void M(bool condition)
    {
        if (condition)
        {
            int badName = 1;
        }

        if (!condition)
        {
            int badName = 2;
        }
    }
}
```

### SHARED-CASING-011 — The fix is withheld when a method would receive a name the compiler looks up

The code fix MUST NOT be offered for a method that is not a local function when its derived name (SHARED-CASING-008) is one of `Add`, `AppendFormatted`, `AppendLiteral`, `Cast`, `Deconstruct`, `Dispose`, `DisposeAsync`, `GetAsyncEnumerator`, `GetAwaiter`, `GetEnumerator`, `GetPinnableReference`, `GroupBy`, `GroupJoin`, `Join`, `OrderBy`, `OrderByDescending`, `Select`, `SelectMany`, `ThenBy`, `ThenByDescending`, or `Where`. This holds whether or not the rename would change which method a construct calls. A method whose derived name is not in this list, and a local function, are not withheld by this requirement. The diagnostic is still reported.

Why: The compiler looks these names up for constructs that do not write them in source: `foreach`, `await foreach`, query clauses, `await`, deconstruction, collection initializers, `using`, `fixed`, and interpolated-string handlers. A method renamed to one of them can win that lookup over the method the construct called before, such as an extension method, an extension in a farther namespace, an explicit interface implementation, or a less specific overload, and the program then calls another method without a compile error (SHARED-FIX-003). The renamer does not see such a construct, because the construct names no symbol. Telling a rename that changes such a call from one that does not would mean comparing every such construct before and after the rename; a method spelled like one of these names in the wrong case is rare, so the fix is withheld for the whole list. A local function is never found by such a lookup.

Example — `foreach`, for RH4103:

```csharp
using System.Collections;
using System.Collections.Generic;

public class Numbers : IEnumerable<int>
{
    public IEnumerator<int> {|RH4103:getEnumerator|}()
    {
        yield return 1;
    }

    IEnumerator<int> IEnumerable<int>.GetEnumerator()
    {
        yield return 2;
    }

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<int>)this).GetEnumerator();
}
```

is reported, and no code fix is offered: `foreach (var number in new Numbers())` calls the interface implementation today and would call the renamed `GetEnumerator` after the rename, so it would yield `1` instead of `2`. The same holds for a class that declares only `getEnumerator`, where no `foreach` would call another method.

Example — a query clause, for RH4103:

```csharp
using System;
using System.Collections.Generic;
using System.Linq;

namespace N
{
    public static class Extensions
    {
        public static IEnumerable<int> {|RH4103:select|}(this IEnumerable<int> source, Func<int, int> selector)
        {
            return source;
        }
    }

    public class User
    {
        public IEnumerable<int> Get(List<int> values)
        {
            return from value in values select value * 2;
        }
    }
}
```

is reported, and no code fix is offered: the `select` clause calls `Enumerable.Select` today and would call `N.Extensions.Select` after the rename. The same holds for the other names of the list, such as `add` next to a collection initializer and `deconstruct` next to a deconstruction.

Example — names outside the list, for RH4103 and RH4104:

```csharp
public class C
{
    public int {|RH4103:select_all|}()
    {
        int {|RH4104:getEnumerator|}()
        {
            return 1;
        }

        return getEnumerator();
    }
}
```

```csharp
public class C
{
    public int SelectAll()
    {
        int GetEnumerator()
        {
            return 1;
        }

        return GetEnumerator();
    }
}
```

`SelectAll` is not in the list, and `getEnumerator` is a local function, so both fixes are offered.

### SHARED-CASING-012 — The fix is withheld when a caller-argument expression names the old or the new name

The code fix MUST NOT be offered for a parameter when a `CallerArgumentExpression` attribute in one of the following parameter lists has an argument whose constant value is the old name or the new name of a symbol of the parameter's rename cascade (SHARED-CASING-007), unless the argument is exempt:

- a parameter list that declares a symbol of the cascade;
- the parameter list of a member declared in an extension block whose receiver parameter is a symbol of the cascade.

The argument may be a string literal, a `nameof` expression, or any other constant, such as a `const` field. An argument is exempt only when all of the following hold:

- it is a `nameof` expression whose operand is a single unqualified identifier, such as `nameof(Condition)`; a qualified operand, such as `nameof(Names.Condition)`, is not exempt;
- that identifier is the old name of a parameter that is a symbol of the cascade and that the parameter list carrying the attribute declares itself; an identifier that is the new name, or the old name of a parameter another list declares, such as the receiver of an extension block for an attribute in a member's parameter list, is not exempt;
- the document is compiled with C# 11 or later.

The diagnostic is still reported.

Why: The attribute names a parameter by the value of its argument. The renamer changes that value only where the argument is a `nameof` that refers to a renamed parameter. The exemption relies on the scope C# 11 gives a `nameof` inside an attribute of a parameter list, where an unqualified name refers to the parameter of that name in the list. For an earlier language version the exemption is not taken: the check does not decide what the operand refers to there, and it refuses the fix from the document's language version alone, a conservative refusal that costs only a fix for code compiled before C# 11. Every other argument keeps its value through the rename: a string, a constant, and a `nameof` of another symbol, qualified or not; a rename for which the renamer would qualify such an operand to keep its binding is withheld by SHARED-CASING-009 in any case. When the value is the old name, the rename leaves the attribute naming no parameter; when it is the new name, the rename makes the attribute name the renamed parameter. Either way, the parameter that carries the attribute receives another value at run time (SHARED-FIX-003), and the compiler only warns. The check compares constant values with the cascade's names and looks only at the shape of a `nameof` argument, never at what it binds to; so an extension block, whose members' attributes can name the receiver from another parameter list, is refused for every such argument, `nameof` included. Such inputs are rare, and a rare class is refused with a cheap check (`AGENTS.md`, *Special cases do not grow the source*).

Example — RH4105:

```csharp
using System.Runtime.CompilerServices;

public static class Guard
{
    public static string Check(bool {|RH4105:Condition|}, [CallerArgumentExpression("Condition")] string expression = null)
    {
        return expression;
    }
}
```

is reported, and no code fix is offered: after renaming `Condition` to `condition`, the attribute would name no parameter, and `Guard.Check(1 > 0)` would return `null` instead of `"1 > 0"`. The same holds for `[CallerArgumentExpression("condition")]`, which names the new name and would start to receive the caller's expression, for a `const string` whose value is `"Condition"` used as the argument, and for a constructor, a local function, or a delegate that declares such a parameter list.

Example — RH4120:

```csharp
using System.Runtime.CompilerServices;

public record R(bool {|RH4120:flag|}, [CallerArgumentExpression("flag")] string Expression = null);
```

is reported, and no code fix is offered: after renaming `flag` to `Flag`, the attribute would name no parameter.

Example — a `nameof` of the parameter itself, for RH4105, compiled with C# 11 or later: the argument is exempt, and the fix renames the parameter and the `nameof` operand together:

```csharp
using System.Runtime.CompilerServices;

public static class Guard
{
    public static string Check(bool {|RH4105:Condition|}, [CallerArgumentExpression(nameof(Condition))] string expression = null)
    {
        return expression;
    }
}
```

```csharp
using System.Runtime.CompilerServices;

public static class Guard
{
    public static string Check(bool condition, [CallerArgumentExpression(nameof(condition))] string expression = null)
    {
        return expression;
    }
}
```

Example — a qualified `nameof` of another symbol that has the old name, for RH4105:

```csharp
using System.Runtime.CompilerServices;

public static class Names
{
    public const int Condition = 0;
}

public static class Guard
{
    public static string Check(bool {|RH4105:Condition|}, [CallerArgumentExpression(nameof(Names.Condition))] string expression = null)
    {
        return expression;
    }
}
```

is reported, and no code fix is offered: the `nameof` refers to `Names.Condition`, so the rename keeps its value `"Condition"`, which then names no parameter, and `Guard.Check(1 > 0)` would return `null` instead of `"1 > 0"`.

Example — an unqualified `nameof` of another symbol that has the new name, for RH4105:

```csharp
using System.Runtime.CompilerServices;

public static class Guard
{
    private const int condition = 0;

    public static string Check(bool {|RH4105:Condition|}, [CallerArgumentExpression(nameof(condition))] string expression = null)
    {
        return expression;
    }
}
```

is reported, and no code fix is offered: the operand `condition` is not the old name of the parameter, and the value `"condition"` is the new name, which after the rename names the renamed parameter, so `Guard.Check(1 > 0)` would return `"1 > 0"` instead of `null`.

Example — a `nameof` of the parameter itself, for RH4105, compiled with C# 10:

```csharp
using System.Runtime.CompilerServices;

public static class Guard
{
    public const int Condition = 0;

    public static string Check(bool {|RH4105:Condition|}, [CallerArgumentExpression(nameof(Condition))] string expression = null)
    {
        return expression;
    }
}
```

is reported, and no code fix is offered: before C# 11 the argument is not exempt, so the fix is refused without deciding what `nameof(Condition)` refers to. Compiled with C# 11 or later, the same input is exempt: `nameof(Condition)` refers to the parameter, and the fix renames both to `condition`.

Example — an extension block receiver, for RH4105:

```csharp
using System.Runtime.CompilerServices;

public static class GuardExtensions
{
    extension(bool {|RH4105:Condition|})
    {
        public string Check([CallerArgumentExpression("Condition")] string expression = null)
        {
            return expression;
        }
    }
}
```

is reported, and no code fix is offered: the attribute is in the parameter list of a member of the block whose receiver is renamed, and after renaming `Condition` to `condition` it would name no parameter, so `(1 > 0).Check()` would return `null` instead of `"1 > 0"`. The same holds for `[CallerArgumentExpression(nameof(Condition))]` in that member's parameter list: the receiver is declared by another parameter list, so the argument is not exempt. Without a `CallerArgumentExpression` attribute in the block, or with one whose value is neither `"Condition"` nor `"condition"`, the fix is offered and renames the receiver to `condition`.

### SHARED-CASING-013 — The fix is withheld when the new name needs the verbatim prefix

The code fix MUST NOT be offered when the rename would write the new name with the verbatim prefix `@` at the declaration or at a reference: when the derived name (SHARED-CASING-008) is a reserved C# keyword, such as `class`, `object`, or `string`, and when it is a contextual keyword that is a keyword where the declaration or a reference stands, such as `await` inside an async method, local function, or lambda. A contextual keyword that is written without `@` at the declaration and at every reference, such as `var`, `value`, `nameof`, or `await` in a member that is not async, does not withhold the fix. The diagnostic is still reported.

Why: The verbatim prefix turns a keyword into a name, but it is spelling the author did not write, and the name stays a keyword to every reader; a rule compares names by their value without the `@` (SHARED-DIAG-002), so the escaped name only looks corrected. Only a camelCase target can produce a keyword, because every keyword is lowercase; a `_camelCase` name starts with `_`, and the other styles with an uppercase letter. The cause, a `@` the rename would write, decides every contextual keyword alike, so no list of them has to follow the language.

Example — RH4105:

```csharp
using System.Threading.Tasks;

class C
{
    void M(int {|RH4105:Class|}, object {|RH4105:Object|})
    {
    }

    async Task<int> N(int {|RH4105:Await|})
    {
        await Task.Yield();
        return Await;
    }
}
```

is reported, and no code fix is offered for any parameter: `class` and `object` are reserved keywords, and inside the async method `N`, `await` is a keyword, so the rename would write `@class`, `@object`, and `@await`.

Example — contextual keywords that need no `@`, for RH4105:

```csharp
class C
{
    int M(int {|RH4105:Var|}, int {|RH4105:Nameof|}, int {|RH4105:Await|})
    {
        return Var + Nameof + Await;
    }
}
```

```csharp
class C
{
    int M(int var, int nameof, int await)
    {
        return var + nameof + await;
    }
}
```

### SHARED-CASING-014 — A name that another compilation or a native entry point dictates is not reported

A member is *dictated* when it, or a member it overrides through any number of levels, overrides or implements — implicitly or explicitly — a member declared outside the compilation that declares it: in a referenced assembly, or in another project of the solution. A rule that uses this spec MUST NOT report:

- the name of a dictated method, property, or event;
- the name of a positional parameter of a record whose member is dictated: the property the compiler declares for the parameter, or, when the compiler declares none because the record declares or inherits a member of the parameter's name, that member;
- an element name of a tuple type in the signature of a dictated member — its return type, the types of its parameters, or the type of a property, indexer, or event —, wherever the tuple type stands in that type: as the type itself, as a type argument, as the element type of an array, or as an element of another tuple type;
- the name of a method or local function declared with `[DllImport]`, or of a method declared with `[LibraryImport]`, whose attribute has no `EntryPoint` argument.

A member implements an interface member only when its own type implements that interface. A member that becomes the implementation of an interface member only because a derived type names the interface in its base list, while the member's own type does not implement it, is not dictated by that interface member, and is reported as its rule states. A member that overrides or implements only members declared in its own compilation, through every level, is not dictated and is reported as its rule states. A method or local function whose `[DllImport]` or `[LibraryImport]` attribute has an `EntryPoint` argument is reported as its rule states, whatever the argument's value. A positional parameter of a record whose member is not dictated is reported as its rule states, also when that member is declared in another compilation: a parameter that matches a property it inherits from a base record of a referenced assembly, which overrides and implements nothing, is reported.

Why: The author cannot choose these names. The compiler requires an override and an implicit implementation to repeat the name of the member it overrides or implements, and an override or implementation to repeat the tuple element names of that member's signature; only the declaration in the other compilation could change them, and a fix in this compilation cannot rename it. When the attribute names no entry point, the runtime looks the native function up by the method's name, so another name calls another function. A diagnostic that cannot be resolved without breaking the build or the native binding has no actionable outcome. The boundary is the compilation, not the solution: in the IDE another project of the solution is a compilation whose members are declared in source, and in a command-line build it is a referenced assembly, so deciding by the declaring compilation gives both the same diagnostics. A member that overrides a dictated member repeats its name as well, so the chain is followed through every level. The member that another project of the solution declares is reported in that project, and its fix renames the dictated members with it (SHARED-CASING-007). A member implements on behalf of a derived type only through that derived type, which the analyzer of the member's own type cannot always see, so only the member's own type decides; the fix of such a member stays withheld as SHARED-CASING-009 states. The compiler names the property it declares for a record's positional parameter after the parameter, so the parameter's name is that property's name, and renaming the parameter renames the property. When the record declares or inherits a member of the parameter's name, the compiler declares no property and matches the parameter to that member; renaming the parameter alone gives the record a second public property next to the dictated one, which changes its equality, `ToString`, `with`, and every consumer that reads its properties, and the dictated member's own name cannot change, so that diagnostic has no acceptable outcome either. A matching member that is only declared in another compilation, without overriding or implementing anything there, is not dictated: the exemption covers names a contract fixes, not every name another compilation chose. The tuple element names of a dictated member's signature are those of the member it overrides or implements after its type arguments are substituted; a name that a type argument written in this compilation supplies is reported where that type argument is written, so the author can still choose it, and is reported once.

Example — RH4102, RH4103, RH4114, and RH4116, with a referenced assembly that declares:

```csharp
public abstract class MetaBase
{
    public abstract void process();

    public abstract event System.EventHandler changed;

    public abstract int count { get; }

    public abstract (int first, int second) pair();
}

public interface IMeta
{
    void run();
}
```

```csharp
using System;

public class Worker : MetaBase, IMeta
{
    public override void process()
    {
    }

    public override event EventHandler changed;

    public override int count => 0;

    public override (int first, int second) pair() => default;

    public void run()
    {
    }
}

public class Leaf : Worker
{
    public override void process()
    {
    }
}
```

is not reported. `Leaf.process` overrides `Worker.process`, which is declared in source, but `Worker.process` overrides `MetaBase.process`. The same holds when `MetaBase` and `IMeta` are declared in another project of the solution, and for a property of the protected or internal group (RH4112, RH4113) that overrides a property declared there.

Example — positional parameters of records, for RH4114 and RH4120, with a referenced assembly that declares:

```csharp
public interface INamed
{
    string name { get; }
}

public abstract record MetaRecord
{
    public abstract string title { get; init; }
}

public record MetaPerson(string name);
```

```csharp
public record Person(string name) : INamed;

public record struct Point(string name) : INamed;

public record Item(string title) : MetaRecord;

public interface ILocal
{
    string {|RH4114:code|} { get; }
}

public record Local(string {|RH4120:code|}) : ILocal;

public record Holder(string {|RH4120:name|});

public record Sub(string {|RH4120:other|}) : Holder(other), INamed;

public record Derived(string name) : Person(name);

public record Annotated(string name) : INamed
{
    public string name { get; init; } = name;
}

public record Inherited(string {|RH4120:name|}) : MetaPerson(name);
```

The parameters `name` of `Person` and `Point` and `title` of `Item` are not reported: the property the compiler declares for `Person.name` and `Point.name` implicitly implements `INamed.name`, and the one it declares for `Item.title` overrides `MetaRecord.title`, both declared in the referenced assembly. `Derived.name` and `Annotated.name` are not reported either: the compiler declares no property for them, because `Derived` inherits the property `name` from `Person` and `Annotated` declares one, and in both cases that property implements `INamed.name`. `Local.code` is reported, because `ILocal` is declared in source. `Holder.name` is reported, because `Holder` does not implement `INamed`; its property implements `INamed.name` only for `Sub`. `Inherited.name` is reported: it matches the property `name` that `Inherited` inherits from `MetaPerson`, which is declared in the referenced assembly but overrides and implements nothing, so it is not dictated. The same holds when `INamed`, `MetaRecord`, and `MetaPerson` are declared in another project of the solution.

Example — a chain inside the compilation, for RH4103:

```csharp
public class Base
{
    public virtual void {|RH4103:process|}()
    {
    }
}

public class Derived : Base
{
    public override void {|RH4103:process|}()
    {
    }
}
```

Example — a tuple type in the signature of an explicit interface implementation, for RH4116, with the referenced assembly that declares `public interface IPairs { (int first, int second) get(); }`:

```csharp
public interface ILocal
{
    (int {|RH4116:low|}, int {|RH4116:high|}) range();
}

public class Pairs : IPairs, ILocal
{
    (int first, int second) IPairs.get() => default;

    (int {|RH4116:low|}, int {|RH4116:high|}) ILocal.range() => default;
}
```

The element names in the signature of `IPairs.get` are dictated by the referenced assembly; those of `ILocal.range` are dictated by `ILocal`, which is declared in source and reported there, so they keep their diagnostics. `ILocal.range` is reported by RH4103 too, which this example does not mark.

Example — a method that implements a referenced-assembly interface member only for a derived type, for RH4103, with a referenced assembly that declares `public interface IMeta { void process(); }`:

```csharp
public class A
{
    public void {|RH4103:process|}()
    {
    }
}

public class B : A, IMeta
{
}
```

`A` does not implement `IMeta`, so `A.process` is not dictated and is reported; its fix is withheld (SHARED-CASING-009).

Example — tuple element names that a type argument written in this compilation supplies, for RH4116:

```csharp
using System.Collections;
using System.Collections.Generic;

public class Pairs : IEnumerable<(string {|RH4116:key|}, int {|RH4116:value|})>
{
    public IEnumerator<(string key, int value)> GetEnumerator()
    {
        yield break;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
```

`GetEnumerator` implements `IEnumerable<T>.GetEnumerator`, which is declared outside the compilation, so the element names in its signature are not reported. The names are written in the base list, where they are reported.

Example — native entry points, for RH4103 and RH4104:

```csharp
using System.Runtime.InteropServices;

public static class Native
{
    [DllImport("libc")]
    public static extern int getpid();

    [DllImport("libc", EntryPoint = "getppid")]
    public static extern int {|RH4103:parentId|}();

    public static int M()
    {
        return getuid();

        [DllImport("libc")]
        static extern int getuid();
    }

    public static int N()
    {
        return localPid();

        [DllImport("libc", EntryPoint = "getpid")]
        static extern int {|RH4104:localPid|}();
    }
}
```

`getpid` and `getuid` are not reported; `parentId` and `localPid` name their entry point and are reported. The same holds for a `partial` method declared with `[LibraryImport("libc")]`, and for `[DllImport("libc", EntryPoint = nameof(getpid))]`, which has an `EntryPoint` argument and is reported.

### SHARED-CASING-015 — The fix keeps the name of an anonymous object member

When a reference that the rename (SHARED-CASING-007) renames gives a member of an anonymous object creation its name, the code fix MUST write that member's name explicitly, as `Name = ` in front of the member's renamed expression, with the name the member had before the rename. A member gets its name from a reference when the member has no `Name =` of its own and the compiler takes the member's name from the identifier of the reference: the reference is the member's whole expression, such as `Value` in `new { Value }`, or the name that ends a member access or a conditional access that is the member's whole expression, such as `Value` in `new { this.Value }`, `new { other.Value }`, or `new { other?.Value }`. This holds for every code fix that renames a reported name as SHARED-CASING-007 states. The member's other trivia stays where it was, and the rest of the rename is unchanged.

A member that has a name of its own, such as `new { Item = Value }`, and a member whose name the compiler takes from another identifier, such as `Length` in `new { Value.Length }`, are renamed as SHARED-CASING-007 states and get no name written. A member whose expression the compiler cannot name, such as `new { Value + 1 }`, does not compile and does not occur.

Why: The compiler names such a member after the identifier, so the rename alone would rename the member as well: the text that `ToString()` returns, and the names that a serializer or reflection reads, would change, which SHARED-FIX-003 forbids, and a read of the member by its old name, such as `item.Value`, would no longer compile. Written explicitly with its old name, the member keeps its name, its type, and its position, so the anonymous type stays the same type and every read of it stays valid. The name it keeps is the one it had, so it collides with no other member. Projecting a value into an anonymous object by its bare name is a common shape, also of query syntax, so the fix keeps the member rather than being withheld for it.

Example — RH4105 and RH4115:

```csharp
using System.Linq;

class C
{
    object M(int[] values)
    {
        var {|RH4115:Total|} = values.Sum();

        return values.Select({|RH4105:Value|} => new { Value, Total, Doubled = Value * 2 }).First();
    }
}
```

```csharp
using System.Linq;

class C
{
    object M(int[] values)
    {
        var total = values.Sum();

        return values.Select(value => new { Value = value, Total = total, Doubled = value * 2 }).First();
    }
}
```

Example — a member access, for RH4106, where the member is read by its name:

```csharp
class C
{
    private int {|RH4106:Value|};

    int M(C other)
    {
        var item = new { this.Value, Other = other?.Value };

        return item.Value + (item.Other ?? 0);
    }
}
```

```csharp
class C
{
    private int _value;

    int M(C other)
    {
        var item = new { Value = this._value, Other = other?._value };

        return item.Value + (item.Other ?? 0);
    }
}
```

`Other = other?.Value` has a name of its own and is only renamed.
