# Shared — Empty type declarations

| | |
|---|---|
| ID prefix | SHARED-EMPTY |
| Status | Active |
| Code owners | `EmptyTypeDeclarationShouldUseSemicolonAnalyzerBase`, `EmptyTypeDeclarationShouldUseSemicolonCodeFixProviderBase`, `EmptyTypeDeclarationSemicolonAnalysisUtilities` |
| Uses | FMT-STRUCT-017, FMT-STRUCT-018 |

This spec covers the rules that report a type declaration with an empty brace body: which declarations are reported, what the code fix produces, and when it is withheld. Each rule that uses this spec names the kind of type it checks and the C# version from which that kind has a semicolon declaration. The formatter's counterparts are FMT-STRUCT-017 and FMT-STRUCT-018.

## Non-goals

- Which kind of type a rule checks and from which C# version — each rule's spec: RH5412 for classes, RH5413 for structs, RH5414 for interfaces, RH5415 for records, RH5416 for record structs.
- Empty enums. An enum has no semicolon declaration, and no rule reports its empty body.
- Whether the type is needed at all.
- How the formatter converts an empty type in a document — FMT-STRUCT-017 and FMT-STRUCT-018.

## Requirements

### SHARED-EMPTY-001 — A brace body without members is reported

A rule that uses this spec MUST report a declaration of its kind whose braces enclose nothing but whitespace, line breaks, and comments, and that ends with its closing brace, when the project's C# version is at least the version the rule names. It MUST report one diagnostic per declaration, located on the declaration's name. A comment after the declaration's header, around the opening brace, or before the closing brace MUST NOT keep the declaration from being reported.

Why: A type without members says everything on its declaration line. The two braces add lines but no information, and the semicolon declaration states "no members" directly. A comment does not make the type less empty; SHARED-EMPTY-004 decides what it means for the fix.

Example — RH5412, for classes:

```csharp
class {|RH5412:A|}
{
}

class {|RH5412:B|} { }

class C
{
    class {|RH5412:D|}
    {
    }
}

class {|RH5412:E|}
{
    // reserved
}
```

`C` has the member `D` and is not reported.

### SHARED-EMPTY-002 — A declaration with members or a semicolon is not reported

A rule that uses this spec MUST NOT report a declaration of its kind that has a member, nor a declaration that already ends with a semicolon instead of a brace body.

Why: A type with members needs its braces, and a semicolon declaration is already the form the rule asks for.

Example — RH5412:

```csharp
class A
{
    int _x;
}

class B;
```

is not reported.

### SHARED-EMPTY-003 — The fix writes the semicolon declaration

The code fix MUST replace the braces with a semicolon directly after the declaration's header, as FMT-STRUCT-017 does. A comment that followed the closing brace on its line MUST follow the semicolon.

Why: The semicolon declaration is the form the diagnostic asks for, and it declares the same type. A comment behind the closing brace describes the whole declaration and stays at its end.

Example — RH5412:

```csharp
class {|RH5412:A|}
{
}

class {|RH5412:B|} { } // note
```

```csharp
class A;

class B; // note
```

### SHARED-EMPTY-004 — A comment in the body withholds the fix

The code fix MUST NOT be offered for a declaration whose braces FMT-STRUCT-018 keeps: one with anything other than whitespace and line breaks after the declaration's header, before or after the opening brace, or before the closing brace. The diagnostic is still reported (SHARED-EMPTY-001).

Why: The braces are the only place such a comment can stay. Removing them would delete the comment the author wrote; moving it would change what it describes, which the fix cannot decide for the author.

Example — RH5412:

```csharp
class {|RH5412:A|} // note
{
}

class {|RH5412:B|}
// note
{
}

class {|RH5412:C|}
{
    // reserved
}
```

is reported, and no code fix is offered.
