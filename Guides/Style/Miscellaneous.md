# Miscellaneous
## Type specification
If you specify a type you should always use the keyword `var` if it is possible but it should not be forced e.g. trough casting. In addition if you use a native type and `var` is not an option (e.g. if you decalre parameter of an method) you should use the C# type keyword like `bool` and `int`.

# Coding
## ! operator
The `!` operator should not be used as it can easily be overlooked. Instead, a concrete comparison should be made with `false`.

```csharp
if (!CheckEntry(entry))
{

}

if (CheckEntry(entry) == false)
{

}
```

## Named tuples
Named tuples should not used to transfer data between objects. As the name is not a fixed part of the type definition, it can be redefined, which may lead to errors. To avoid these errors, the creation of a data class is preferred.

## Nested types
Types should not be defined nested within other types. This usually leads to large, cluttered files.

## Structs
In general you should use classes when defining types. Structures should only be used for special cases like interacting with unmanaged APIs.

## Private Properties
Private properties with automatic implementation should not be used because there is no benefit. Properties should be used to define the interface of an object.