# Source code comments
Source code comments should only be created by using single line comments (`//`). The comment lines should be placed on their own line which is preced by a empty line, excepts it the start of a scope.

# XML documentation
All types and members and of it components should be documented using the xml comment format. When declaring a member which overides a member of a base type the \<inheritdoc/>-tag should be use to reduce redundance. 


## Formatting and Spacing
All tag lines should be preceded by a single space. If the documentatin spans only one line the tags should be placed on the same line. The exception is the summary-tag which ever should be placed on a seperated line. 

```csharp
/// <summary>
/// Get documentation in the choosen language
/// </summary>
/// <param name="ieftLanguageTag">IEFT language tag</param>
/// <returns>Return the full documentation in the given language.</returns>
private string GetDocumentation(string ieftLanguageTag)
```

## Tags

| Tag          | Is used? | Explanation                                                                                |
|:------------ |:--------:|:------------------------------------------------------------------------------------------ |
| summary      | &#10004; | Used on every type or member                                                               |
| remarks      | &#10004; | Can be used if a detailed explanation is needed                                            |
| returns      | &#10004; | Used on evenry member which returns data                                                   |
| param        | &#10004; | Used for every parameter of a method                                                       |
| paramref     | &#10004; | Can be used to mention a parameter                                                         |
| typeparam    | &#10004; | Used for every type parameter                                                              |
| typeparamref | &#10004; | Can be used to mention a type parameter                                                    |
| exception    | &#10060; | Should not be used because it tends to be inconplete                                       |
| para         | &#10004; | Can be used to create paragraph                                                            | 
| list         | &#10004; | Can be used to create a list                                                               |
| c            | &#10004; | Can used to indicate a text as code (single line)                                          |
| code         | &#10004; | Can used to indicate a text as code (multi line)                                           |
| example      | &#10004; | Cam used to create a example                                                               |
| inheritdoc   | &#10004; | Should be used on all member which use the override keyword                                |
| include      | &#10060; | The documentation should not be located in aother file                                     |
| see          | &#10004; | Can be used to refer to another (cref, href, langword) of the documentation or source code |
| seealso      | &#10060; | Should only be used if a external documentation is generated                               |
| value        | &#10060; | Instead of the value-tag the summary-tag is used.                                          |


# Miscellaneous 
## Commented out source code
Source code should not be commented out, but deleted. For a better understanding, the current implementation can be explained via comments, which should make it unnecessary to look at the old source code. Old versions of the source code can be viewed via the source code control. 