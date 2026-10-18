# Shared — Name casing

| | |
|---|---|
| ID prefix | SHARED-CASING |
| Status | Active |
| Code owners | `CasingAnalyzerBase`, `CasingCodeFixProviderBase<T>`, `NamingAccessibilityClassifier`, `NamespaceCasingHelper`, `CasingUtilities` |
| Uses | SHARED-FIX-002 |

This spec covers the rules that require one kind of name to be written in a naming style. It defines the styles, the accessibility groups of fields and properties, how a namespace name is checked, and the code fix that renames a reported name. Each rule that uses this spec states which declarations it checks, which style it requires, where its diagnostic is located, and any exceptions of its own.

## Non-goals

- Which declarations a rule checks, the style it requires, where its diagnostic is located, and its own exceptions — each rule's spec.
- The `I` prefix of interface names and how its fix treats the prefix — RH4005.
- The naming style of type parameters — RH4121.
- Names that must not be a single letter — RH4122 to RH4127.
- Whether a type's name matches its file name — RH4001; which namespaces may be declared — RH4009.

## Requirements

### SHARED-CASING-001 — PascalCase

A name whose characters are all letters of the Unicode categories Lu, Ll, Lm, or Lo, decimal digits, or `_` MUST be accepted as PascalCase exactly when its first character is an uppercase letter (category Lu) and every other character is a letter or a digit.

Why: PascalCase is the .NET style for namespaces, types, and members that are visible outside their type. The uppercase letters inside the name mark its word boundaries, so a separator such as `_` adds nothing, and an acronym may stay uppercase.

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

### SHARED-CASING-002 — camelCase

A name whose characters are all letters of the Unicode categories Lu, Ll, Lm, or Lo, decimal digits, or `_` MUST be accepted as camelCase exactly when its first character is a lowercase letter (category Ll) and every other character is a letter or a digit.

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

A name whose characters are all letters of the Unicode categories Lu, Ll, Lm, or Lo, decimal digits, or `_` MUST be accepted as `_camelCase` exactly when it starts with `_` and is either just `_` or continues with a name that is camelCase as SHARED-CASING-002 defines it. Every other name MUST NOT be accepted, including a name with a second underscore or a digit after the prefix.

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

A rule's code fix MUST derive the new name from an old name whose characters are all letters of the Unicode categories Lu, Ll, Lm, or Lo, decimal digits, or `_` as follows. It splits the old name into words at every `_`, and drops the underscores. It writes the first character of each word in uppercase. When every other character of a word is an uppercase letter, it writes them in lowercase; otherwise it keeps them. It then joins the words, which gives the new name for a PascalCase target. For a camelCase target, the first character of the joined words is written in lowercase, and for a `_camelCase` target, that camelCase name gets a `_` in front.

This requirement covers an old name whose first word, the first non-empty part between underscores, starts with a letter whose one-character uppercase and lowercase mappings, in the invariant culture, differ from each other. A first word that starts with a digit, with a letter without case such as `注`, or with a letter such as `ß` that has no one-character uppercase mapping, is not covered.

For a camelCase or `_camelCase` target, it further covers only an old name whose first word does not start with two uppercase letters, or consists of uppercase letters only.

Why: A separator such as `_`, or the `_` of a leading `m_`, is a word boundary in common naming styles. A word in uppercase letters only (`FIRST_NAME`, `URL`) is a shouted word, not an acronym inside a PascalCase name. Uppercase letters inside a word are kept, because they already mark the word boundaries of a PascalCase or camelCase name. Deriving the name mechanically keeps the result predictable across every rule.

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

### SHARED-CASING-007 — The fix renames the declaration and every reference

The code fix MUST rename the reported declaration's symbol with the Roslyn renamer, so that the declaration and every reference to it in the active code of the solution change together. It MUST NOT be offered when the declared symbol cannot be resolved.

This requirement covers an old name that SHARED-CASING-006 covers, and a derived name that is written without the verbatim prefix `@` at the declaration and at every reference: a name that is not a reserved C# keyword, and not `await` inside an async member. Contextual keywords such as `value`, `var`, and `nameof` are covered and stay as they are. For a type declared directly in a namespace, including the global namespace (a type declared at file level), it covers a type none of whose type parameters, and none of whose members other than enum members, already has the new name.

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

### SHARED-CASING-008 — The fix is withheld when no usable name can be derived

The code fix MUST NOT be offered when the old name has no letter or digit, or when the name that the steps of SHARED-CASING-006 derive is the old name itself or is not a valid identifier because it starts with a digit. The diagnostic is still reported.

Why: A name such as `__` has no words to build a name from. A derived name that equals the old one changes nothing, and one that is not an identifier does not compile.

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

### SHARED-CASING-009 — The fix is withheld when the new name would collide

The code fix MUST NOT be offered when renaming to the derived name would collide with an existing name: when the renamer reports a conflict; for a symbol that is not declared directly in a namespace, when the rename adds a compile error inside the declarations that contain the renamed symbol; and for a symbol declared directly in a namespace, when a type or another member of that namespace in source already has the new name. A symbol declared directly in a namespace includes a type declared at file level, which belongs to the global namespace; a nested type is not one. A type MUST be treated as colliding with another type only when both have the same number of type parameters, and, when either of them is file-local, only when both are declared in the same file. The diagnostic is still reported.

Why: A collision cannot be resolved without knowing which of the two names the author meant, so it is left to the author. Types with a different number of type parameters, and a file-local type and a type of another file, can share a name, so renaming to it is safe.

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

is reported, and no code fix is offered, because renaming the class to `Order` would add a compile error inside `Outer`: a member cannot have the name of its enclosing type.

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
