# Reihitsu.Analyzer

A Roslyn-based C# analyzer package that enforces coding rules across multiple categories: Analyzer, Performance, Design, Clarity, Naming, Layout, Spacing, Organization, and Documentation.

## Installation

```shell
dotnet add package Reihitsu.Analyzer
```

## Rules

The **Formatter** column indicates whether running the Reihitsu formatter can automatically correct the rule without relying on an analyzer code fix.

| ID| Description| Analyzer| Code Fix| Formatter|
|--------|-----------------------------------------------------------------------|:--------:|:--------:|:--------:|
|| **Analyzer**||||
| [RH0001](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0001.md)| reihitsu.json configuration must be valid.| ✔| ❌| ❌|
| [RH0002](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0002.md)| Documentation rules are currently not active for this project.| ✔| ❌| ❌|
|| **Performance**||||
| [RH1001](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH1001.md)| Types used as dictionary keys or in hash sets must implement equality members.| ✔| ❌| ❌|
| [RH1002](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH1002.md)| Types used for equality comparison must implement equality members.| ✔| ❌| ❌|
| [RH1003](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH1003.md)| Use string interpolation instead of string concatenation.| ✔| ❌| ❌|
|| **Design**||||
| [RH2001](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH2001.md)| Private auto-implemented properties should not be used.| ✔| ✔| ❌|
| [RH2002](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH2002.md)| Async void methods should not be used.| ✔| ❌| ❌|
| [RH2003](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH2003.md)| NotImplementedException should not be used.| ✔| ❌| ❌|
| [RH2004](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH2004.md)| Access modifier must be declared.| ✔| ✔| ❌|
| [RH2005](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH2005.md)| Fields must be private.| ✔| ✔| ❌|
| [RH2006](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH2006.md)| Debug.Assert must provide message text.| ✔| ❌| ❌|
| [RH2007](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH2007.md)| Debug.Fail must provide message text.| ✔| ❌| ❌|
| [RH2101](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH2101.md)| Nested classes should not be used.| ✔| ❌| ❌|
| [RH2102](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH2102.md)| Classes should not use parameterized primary constructors.| ✔| ❌| ❌|
| [RH2103](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH2103.md)| Structs should not use parameterized primary constructors.| ✔| ❌| ❌|
| [RH2104](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH2104.md)| Nested structs should not be used.| ✔| ❌| ❌|
| [RH2105](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH2105.md)| Nested interfaces should not be used.| ✔| ❌| ❌|
| [RH2106](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH2106.md)| Nested records should not be used.| ✔| ❌| ❌|
| [RH2107](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH2107.md)| Nested enums should not be used.| ✔| ❌| ❌|
| [RH2108](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH2108.md)| Nested delegates should not be used.| ✔| ❌| ❌|
| [RH2109](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH2109.md)| Razor @code blocks should not be used.| ✔| ❌| ❌|
|| **Clarity**||||
| [RH3001](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH3001.md)| Not operator should not be used.| ✔| ✔| ❌|
| [RH3002](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH3002.md)| Statement must not use unnecessary parentheses.| ✔| ✔| ❌|
| [RH3003](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH3003.md)| Use `string.Empty` for empty strings.| ✔| ✔| ❌|
| [RH3004](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH3004.md)| Use lambda syntax.| ✔| ✔| ❌|
| [RH3005](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH3005.md)| Use readable conditions.| ✔| ✔| ❌|
| [RH3006](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH3006.md)| Conditional expressions must declare precedence.| ✔| ✔| ❌|
| [RH3007](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH3007.md)| Do not use query syntax.| ✔| ❌| ❌|
| [RH3101](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH3101.md)| Do not prefix calls with `base.` unless a local implementation exists.| ✔| ✔| ❌|
| [RH3102](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH3102.md)| Code must not contain empty statements.| ✔| ✔| ❌|
| [RH3103](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH3103.md)| Use shorthand for nullable types.| ✔| ✔| ❌|
| [RH3104](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH3104.md)| Do not use default value type constructor.| ✔| ✔| ❌|
| [RH3105](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH3105.md)| Do not prefix local members with `this.`.| ✔| ✔| ❌|
| [RH3106](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH3106.md)| Unnecessary delegate parentheses should be removed.| ✔| ✔| ❌|
| [RH3107](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH3107.md)| Unnecessary attribute constructor parentheses should be removed.| ✔| ✔| ❌|
| [RH3201](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH3201.md)| Comments must contain text.| ✔| ✔| ❌|
| [RH3202](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH3202.md)| Expression style methods should not be used.| ✔| ✔| ✔|
| [RH3203](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH3203.md)| Expression style constructors should not be used.| ✔| ✔| ✔|
| [RH3204](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH3204.md)| Interpolated strings without interpolation should not use the dollar marker.| ✔| ✔| ✔|
|| **Naming**||||
| [RH4001](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH4001.md)| The name of the type should match the filename.| ✔| ✔| ❌|
| [RH4002](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH4002.md)| Class names should be in PascalCase.| ✔| ✔| ❌|
| [RH4003](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH4003.md)| Struct names should be in PascalCase.| ✔| ✔| ❌|
| [RH4004](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH4004.md)| Enum names should be in PascalCase.| ✔| ✔| ❌|
| [RH4005](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH4005.md)| Interface names should use the I prefix and PascalCase.| ✔| ✔| ❌|
| [RH4006](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH4006.md)| Delegate names should be in PascalCase.| ✔| ✔| ❌|
| [RH4007](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH4007.md)| File scoped namespace declarations should be in PascalCase.| ✔| ❌| ❌|
| [RH4008](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH4008.md)| Namespace declarations should be in PascalCase.| ✔| ❌| ❌|
| [RH4009](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH4009.md)| The given namespace is not allowed.| ✔| ❌| ❌|
| [RH4010](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH4010.md)| Record names should be in PascalCase.| ✔| ✔| ❌|
| [RH4011](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH4011.md)| Record struct names should be in PascalCase.| ✔| ✔| ❌|
| [RH4101](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH4101.md)| Enum member names should be in PascalCase.| ✔| ✔| ❌|
| [RH4102](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH4102.md)| Event names should be in PascalCase.| ✔| ✔| ❌|
| [RH4103](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH4103.md)| Method names should be in PascalCase.| ✔| ✔| ❌|
| [RH4104](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH4104.md)| Local function names should be in PascalCase.| ✔| ✔| ❌|
| [RH4105](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH4105.md)| Method parameter names should be in camelCase.| ✔| ✔| ❌|
| [RH4106](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH4106.md)| Private field names should be in _camelCase.| ✔| ✔| ❌|
| [RH4107](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH4107.md)| Protected field names should be in _camelCase.| ✔| ✔| ❌|
| [RH4108](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH4108.md)| Internal field names should be in PascalCase.| ✔| ✔| ❌|
| [RH4109](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH4109.md)| Public field names should be in PascalCase.| ✔| ✔| ❌|
| [RH4110](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH4110.md)| Const field names should be in PascalCase.| ✔| ✔| ❌|
| [RH4111](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH4111.md)| Private property names should be in PascalCase.| ✔| ✔| ❌|
| [RH4112](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH4112.md)| Protected property names should be in PascalCase.| ✔| ✔| ❌|
| [RH4113](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH4113.md)| Internal property names should be in PascalCase.| ✔| ✔| ❌|
| [RH4114](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH4114.md)| Public property names should be in PascalCase.| ✔| ✔| ❌|
| [RH4115](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH4115.md)| Local variable names should be in camelCase.| ✔| ✔| ❌|
| [RH4116](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH4116.md)| Tuple element names should be in camelCase.| ✔| ❌| ❌|
| [RH4117](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH4117.md)| Deconstruction variable names should be in camelCase.| ✔| ✔| ❌|
| [RH4118](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH4118.md)| Tuple expression argument names should be in camelCase.| ✔| ❌| ❌|
| [RH4119](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH4119.md)| Single-letter identifiers should not be used.| ✔| ❌| ❌|
| [RH4120](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH4120.md)| Record primary constructor parameter names should be in PascalCase.| ✔| ✔| ❌|
|| **Layout**||||
| [RH5001](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5001.md)| try-Statement should be preceded by a blank line.| ✔| ✔| ✔|
| [RH5002](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5002.md)| if-Statement should be preceded by a blank line.| ✔| ✔| ✔|
| [RH5003](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5003.md)| while-Statement should be preceded by a blank line.| ✔| ✔| ✔|
| [RH5004](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5004.md)| do-Statement should be preceded by a blank line.| ✔| ✔| ✔|
| [RH5005](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5005.md)| using-Statement should be preceded by a blank line.| ✔| ✔| ✔|
| [RH5006](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5006.md)| foreach-Statement should be preceded by a blank line.| ✔| ✔| ✔|
| [RH5007](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5007.md)| for-Statement should be preceded by a blank line.| ✔| ✔| ✔|
| [RH5008](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5008.md)| return-Statement should be preceded by a blank line.| ✔| ✔| ✔|
| [RH5009](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5009.md)| goto-Statement should be preceded by a blank line.| ✔| ✔| ✔|
| [RH5010](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5010.md)| break-Statement should be preceded by a blank line.| ✔| ✔| ✔|
| [RH5011](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5011.md)| break statement should be followed by a blank line.| ✔| ✔| ✔|
| [RH5012](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5012.md)| continue-Statement should be preceded by a blank line.| ✔| ✔| ✔|
| [RH5013](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5013.md)| throw-Statement should be preceded by a blank line.| ✔| ✔| ✔|
| [RH5014](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5014.md)| switch-Statement should be preceded by a blank line.| ✔| ✔| ✔|
| [RH5015](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5015.md)| checked-Statement should be preceded by a blank line.| ✔| ✔| ✔|
| [RH5016](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5016.md)| unchecked-Statement should be preceded by a blank line.| ✔| ✔| ✔|
| [RH5017](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5017.md)| fixed-Statement should be preceded by a blank line.| ✔| ✔| ✔|
| [RH5018](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5018.md)| lock-Statement should be preceded by a blank line.| ✔| ✔| ✔|
| [RH5019](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5019.md)| yield-Statement should be preceded by a blank line.| ✔| ✔| ✔|
| [RH5020](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5020.md)| Single line comments should be preceded by a blank line.| ✔| ✔| ✔|
| [RH5021](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5021.md)| Local declarations should be followed by a blank line.| ✔| ✔| ✔|
| [RH5022](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5022.md)| Opening brace must not be followed by blank line.| ✔| ✔| ✔|
| [RH5023](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5023.md)| Code must not contain multiple blank lines in a row.| ✔| ✔| ✔|
| [RH5024](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5024.md)| Closing brace must not be preceded by blank line.| ✔| ✔| ✔|
| [RH5025](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5025.md)| Opening brace must not be preceded by blank line.| ✔| ✔| ✔|
| [RH5026](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5026.md)| Chained statement blocks must not be preceded by blank line.| ✔| ✔| ✔|
| [RH5027](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5027.md)| While-do footer must not be preceded by blank line.| ✔| ✔| ✔|
| [RH5028](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5028.md)| Code must not contain blank lines at the start of the file.| ✔| ✔| ✔|
| [RH5029](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5029.md)| Local declarations should be preceded by a blank line.| ✔| ✔| ✔|
| [RH5030](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5030.md)| Blank line after closing brace.| ✔| ✔| ✔|
| [RH5101](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5101.md)| The first argument should be on the same line as the opening parenthesis.| ✔| ✔| ✔|
| [RH5102](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5102.md)| Arguments should either all be on one line or each on its own line.| ✔| ✔| ✔|
| [RH5103](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5103.md)| Code must not contain multiple statements on one line.| ✔| ✔| ✔|
| [RH5104](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5104.md)| Comments must be on their own line.| ✔| ✔| ✔|
| [RH5105](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5105.md)| Opening parenthesis must be on declaration line.| ✔| ✔| ✔|
| [RH5106](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5106.md)| Closing parenthesis must be on line of last argument.| ✔| ✔| ✔|
| [RH5107](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5107.md)| Comma must be on same line as previous parameter.| ✔| ✔| ✔|
| [RH5108](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5108.md)| Parameter list must follow declaration.| ✔| ✔| ✔|
| [RH5109](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5109.md)| Parameters must be on same line or separate lines.| ✔| ✔| ✔|
| [RH5110](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5110.md)| Generic type constraints should be on their own line with proper indentation.| ✔| ✔| ✔|
| [RH5111](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5111.md)| Assignments must have proper line breaks.| ✔| ✔| ✔|
| [RH5112](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5112.md)| Wrapped fluent calls should keep the first call on the original line.| ✔| ✔| ✔|
| [RH5113](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5113.md)| Declaration semicolon must stay on the declaration line.| ✔| ✔| ✔|
| [RH5201](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5201.md)| Method chains should be aligned.| ✔| ✔| ✔|
| [RH5202](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5202.md)| Raw string literals should be formatted correctly.| ✔| ✔| ✔|
| [RH5203](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5203.md)| Multi-line arguments should be aligned.| ✔| ✔| ✔|
| [RH5204](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5204.md)| Indentation must use 4 spaces per scope level.| ✔| ✔| ✔|
| [RH5205](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5205.md)| Statement lambda opening braces should be aligned.| ✔| ✔| ✔|
| [RH5206](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5206.md)| Switch expression braces should be anchored.| ✔| ✔| ✔|
| [RH5301](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5301.md)| Object initializer should be formatted correctly.| ✔| ✔| ✔|
| [RH5302](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5302.md)| Logical expressions should be formatted correctly.| ✔| ✔| ✔|
| [RH5303](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5303.md)| Collection initializer should be formatted correctly.| ✔| ✔| ✔|
| [RH5304](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5304.md)| Nested collection initializer assignments should be formatted correctly.| ✔| ✔| ✔|
| [RH5305](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5305.md)| Collection expressions should be formatted correctly.| ✔| ✔| ✔|
| [RH5306](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5306.md)| List patterns should be formatted correctly.| ✔| ✔| ✔|
| [RH5307](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5307.md)| Indexer bracketed arguments should be single lined.| ✔| ✔| ✔|
| [RH5401](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5401.md)| Expression style get-only properties should be single lined.| ✔| ✔| ✔|
| [RH5402](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5402.md)| Braces for multi-line statements must not share line.| ✔| ✔| ✔|
| [RH5403](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5403.md)| Statement must not be on a single line.| ✔| ✔| ✔|
| [RH5404](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5404.md)| Element must not be on a single line.| ✔| ✔| ✔|
| [RH5405](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5405.md)| Braces must not be omitted.| ✔| ✔| ✔|
| [RH5406](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5406.md)| Braces must not be omitted from multi-line child statements.| ✔| ✔| ✔|
| [RH5407](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5407.md)| Use braces consistently.| ✔| ✔| ✔|
| [RH5408](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5408.md)| Auto-properties should be single lined.| ✔| ✔| ✔|
| [RH5409](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5409.md)| Final enum members must not have trailing commas.| ✔| ✔| ✔|
| [RH5410](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5410.md)| Final array initializer items must not have trailing commas.| ✔| ✔| ✔|
| [RH5411](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5411.md)| Final collection initializer items must not have trailing commas.| ✔| ✔| ✔|
| [RH5412](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5412.md)| Empty classes should use semicolon declarations.| ✔| ✔| ✔|
| [RH5413](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5413.md)| Empty structs should use semicolon declarations.| ✔| ✔| ✔|
| [RH5414](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5414.md)| Empty interfaces should use semicolon declarations.| ✔| ✔| ✔|
| [RH5415](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5415.md)| Empty records should use semicolon declarations.| ✔| ✔| ✔|
| [RH5416](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5416.md)| Empty record structs should use semicolon declarations.| ✔| ✔| ✔|
| [RH5501](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5501.md)| Assembly attributes must be on their own line.| ✔| ✔| ✔|
| [RH5502](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5502.md)| Assembly attribute lists must use one attribute per list.| ✔| ✔| ✔|
| [RH5503](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5503.md)| Module attributes must be on their own line.| ✔| ✔| ✔|
| [RH5504](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5504.md)| Module attribute lists must use one attribute per list.| ✔| ✔| ✔|
| [RH5505](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5505.md)| Class attributes must be on their own line.| ✔| ✔| ✔|
| [RH5506](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5506.md)| Class attribute lists must use one attribute per list.| ✔| ✔| ✔|
| [RH5507](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5507.md)| Struct attributes must be on their own line.| ✔| ✔| ✔|
| [RH5508](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5508.md)| Struct attribute lists must use one attribute per list.| ✔| ✔| ✔|
| [RH5509](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5509.md)| Enum attributes must be on their own line.| ✔| ✔| ✔|
| [RH5510](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5510.md)| Enum attribute lists must use one attribute per list.| ✔| ✔| ✔|
| [RH5511](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5511.md)| Constructor attributes must be on their own line.| ✔| ✔| ✔|
| [RH5512](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5512.md)| Constructor attribute lists must use one attribute per list.| ✔| ✔| ✔|
| [RH5513](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5513.md)| Method attributes must be on their own line.| ✔| ✔| ✔|
| [RH5514](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5514.md)| Method attribute lists must use one attribute per list.| ✔| ✔| ✔|
| [RH5515](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5515.md)| Property attributes must be on their own line.| ✔| ✔| ✔|
| [RH5516](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5516.md)| Property attribute lists must use one attribute per list.| ✔| ✔| ✔|
| [RH5517](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5517.md)| Field attributes must be on their own line.| ✔| ✔| ✔|
| [RH5518](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5518.md)| Field attribute lists must use one attribute per list.| ✔| ✔| ✔|
| [RH5519](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5519.md)| Event attributes must be on their own line.| ✔| ✔| ✔|
| [RH5520](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5520.md)| Event attribute lists must use one attribute per list.| ✔| ✔| ✔|
| [RH5521](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5521.md)| Interface attributes must be on their own line.| ✔| ✔| ✔|
| [RH5522](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5522.md)| Interface attribute lists must use one attribute per list.| ✔| ✔| ✔|
| [RH5523](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5523.md)| Parameter attributes must stay inline.| ✔| ✔| ✔|
| [RH5524](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5524.md)| Parameter attributes must use merged attribute lists.| ✔| ✔| ✔|
| [RH5525](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5525.md)| Delegate attributes must be on their own line.| ✔| ✔| ✔|
| [RH5526](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5526.md)| Delegate attribute lists must use one attribute per list.| ✔| ✔| ✔|
| [RH5527](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5527.md)| Return value attributes must be on their own line.| ✔| ✔| ✔|
| [RH5528](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5528.md)| Return value attribute lists must use one attribute per list.| ✔| ✔| ✔|
| [RH5529](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5529.md)| Generic parameter attributes must use merged attribute lists.| ✔| ✔| ✔|
| [RH5530](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5530.md)| Accessor attributes must follow placement rules.| ✔| ✔| ✔|
| [RH5531](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5531.md)| Accessor attribute lists must follow shape rules.| ✔| ✔| ✔|
| [RH5601](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5601.md)| Tab characters must not be used.| ✔| ✔| ✔|
| [RH5602](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5602.md)| Code must not contain trailing whitespace.| ✔| ✔| ✔|
| [RH5603](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5603.md)| File must not end with a newline.| ✔| ✔| ✔|
| [RH5604](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH5604.md)| Code must not contain mixed line endings.| ✔| ✔| ✔|
|| **Spacing**||||
| [RH6001](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH6001.md)| Keywords must be spaced correctly.| ✔| ✔| ✔|
| [RH6002](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH6002.md)| Commas must be spaced correctly.| ✔| ✔| ✔|
| [RH6003](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH6003.md)| Semicolons must be spaced correctly.| ✔| ✔| ✔|
| [RH6004](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH6004.md)| Preprocessor keywords must not be preceded by space.| ✔| ✔| ✔|
| [RH6005](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH6005.md)| Operator keyword must be followed by space.| ✔| ✔| ✔|
| [RH6006](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH6006.md)| Opening parenthesis must be spaced correctly.| ✔| ✔| ✔|
| [RH6007](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH6007.md)| Opening square brackets must be spaced correctly.| ✔| ✔| ✔|
| [RH6008](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH6008.md)| Closing square brackets must be spaced correctly.| ✔| ✔| ✔|
| [RH6009](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH6009.md)| Opening braces must be spaced correctly.| ✔| ✔| ✔|
| [RH6010](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH6010.md)| Closing braces must be spaced correctly.| ✔| ✔| ✔|
| [RH6011](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH6011.md)| Opening generic brackets must be spaced correctly.| ✔| ✔| ✔|
| [RH6012](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH6012.md)| Closing generic brackets must be spaced correctly.| ✔| ✔| ✔|
| [RH6013](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH6013.md)| Opening attribute brackets must be spaced correctly.| ✔| ✔| ✔|
| [RH6014](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH6014.md)| Closing attribute brackets must be spaced correctly.| ✔| ✔| ✔|
| [RH6015](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH6015.md)| Nullable type symbols must not be preceded by space.| ✔| ✔| ✔|
| [RH6016](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH6016.md)| Member access symbols must be spaced correctly.| ✔| ✔| ✔|
| [RH6017](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH6017.md)| Increment/decrement symbols must be spaced correctly.| ✔| ✔| ✔|
| [RH6018](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH6018.md)| Negative signs must be spaced correctly.| ✔| ✔| ✔|
| [RH6019](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH6019.md)| Positive signs must be spaced correctly.| ✔| ✔| ✔|
| [RH6020](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH6020.md)| Dereference and access-of symbols must be spaced correctly.| ✔| ✔| ✔|
| [RH6021](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH6021.md)| Colons must be spaced correctly.| ✔| ✔| ✔|
| [RH6022](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH6022.md)| No space after new for implicitly typed arrays.| ✔| ✔| ✔|
|| **Organization**||||
| [RH7001](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH7001.md)| File may only contain a single namespace.| ✔| ❌| ❌|
| [RH7002](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH7002.md)| File may only contain a single top-level type.| ✔| ❌| ❌|
| [RH7003](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH7003.md)| File-scoped namespaces should be used.| ✔| ❌| ❌|
| [RH7004](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH7004.md)| Using declarations should not be used.| ✔| ✔| ❌|
| [RH7101](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH7101.md)| Do not combine fields.| ✔| ✔| ✔|
| [RH7102](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH7102.md)| Constants must appear before fields.| ✔| ✔| ❌|
| [RH7103](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH7103.md)| Static elements must appear before instance elements.| ✔| ✔| ❌|
| [RH7104](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH7104.md)| Partial elements must declare access modifier.| ✔| ✔| ❌|
| [RH7105](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH7105.md)| Declaration keywords must follow order.| ✔| ✔| ❌|
| [RH7106](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH7106.md)| Protected must come before internal.| ✔| ✔| ❌|
| [RH7107](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH7107.md)| Property accessors must follow order.| ✔| ✔| ❌|
| [RH7108](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH7108.md)| Event accessors must follow order.| ✔| ✔| ❌|
| [RH7109](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH7109.md)| Readonly elements must appear before non-readonly elements.| ✔| ✔| ❌|
| [RH7201](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH7201.md)| System using directives must be placed before other using directives.| ✔| ✔| ❌|
| [RH7202](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH7202.md)| Using alias directives must be placed after other using directives.| ✔| ✔| ❌|
| [RH7203](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH7203.md)| Using directives must be ordered alphabetically by namespace.| ✔| ✔| ❌|
| [RH7204](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH7204.md)| Using alias directives must be ordered alphabetically by alias name.| ✔| ✔| ❌|
| [RH7205](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH7205.md)| Using static directives must be placed at correct position.| ✔| ✔| ❌|
| [RH7206](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH7206.md)| Using static directives must be ordered alphabetically.| ✔| ✔| ❌|
| [RH7207](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH7207.md)| Using directives should be organized into groups.| ✔| ✔| ✔|
| [RH7301](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH7301.md)| #region and #endregion descriptions should match.| ✔| ✔| ✔|
| [RH7302](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH7302.md)| #region description should start with an uppercase letter.| ✔| ✔| ✔|
| [RH7303](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH7303.md)| Do not place regions within elements.| ✔| ✔| ✔|
| [RH7304](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH7304.md)| Region directives must use consistent indentation with containing code.| ✔| ✔| ✔|
| [RH7305](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH7305.md)| Types should be organized with regions.| ✔| ❌| ❌|
| [RH7305A](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH7305A.md)| Types with multiple member kinds should be organized with regions.| ✔| ❌| ❌|
| [RH7306](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH7306.md)| Region descriptions should not end with implementation.| ✔| ✔| ❌|
| [RH7307](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH7307.md)| Region descriptions should not be Member or Members.| ✔| ❌| ❌|
| [RH7308](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH7308.md)| Standard regions should contain only their matching member kind.| ✔| ❌| ❌|
| [RH7401](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH7401.md)| Override methods should be grouped by base type regions.| ✔| ❌| ❌|
| [RH7402](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH7402.md)| Override properties should be grouped by base type regions.| ✔| ❌| ❌|
| [RH7403](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH7403.md)| Override events should be grouped by base type regions.| ✔| ❌| ❌|
| [RH7404](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH7404.md)| Override indexers should be grouped by base type regions.| ✔| ❌| ❌|
| [RH7405](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH7405.md)| Non-override methods should not be placed in override regions.| ✔| ❌| ❌|
| [RH7406](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH7406.md)| Non-override properties should not be placed in override regions.| ✔| ❌| ❌|
| [RH7407](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH7407.md)| Non-override events should not be placed in override regions.| ✔| ❌| ❌|
| [RH7408](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH7408.md)| Non-override indexers should not be placed in override regions.| ✔| ❌| ❌|
| [RH7501](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH7501.md)| Break statements should not be inside explicit switch case blocks.| ✔| ✔| ❌|
|| **Documentation**||||
| [RH8001](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH8001.md)| Non-private classes must be documented.| ✔| ❌| ❌|
| [RH8002](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH8002.md)| Private classes must be documented.| ✔| ❌| ❌|
| [RH8003](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH8003.md)| Non-private structs must be documented.| ✔| ❌| ❌|
| [RH8004](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH8004.md)| Private structs must be documented.| ✔| ❌| ❌|
| [RH8005](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH8005.md)| Non-private interfaces must be documented.| ✔| ❌| ❌|
| [RH8006](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH8006.md)| Private interfaces must be documented.| ✔| ❌| ❌|
| [RH8007](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH8007.md)| Non-private records must be documented.| ✔| ❌| ❌|
| [RH8008](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH8008.md)| Private records must be documented.| ✔| ❌| ❌|
| [RH8009](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH8009.md)| Non-private record structs must be documented.| ✔| ❌| ❌|
| [RH8010](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH8010.md)| Private record structs must be documented.| ✔| ❌| ❌|
| [RH8011](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH8011.md)| Non-private enums must be documented.| ✔| ❌| ❌|
| [RH8012](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH8012.md)| Private enums must be documented.| ✔| ❌| ❌|
| [RH8013](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH8013.md)| Enum members in non-private enums must be documented.| ✔| ❌| ❌|
| [RH8014](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH8014.md)| Enum members in private enums must be documented.| ✔| ❌| ❌|
| [RH8015](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH8015.md)| Non-private delegates must be documented.| ✔| ❌| ❌|
| [RH8016](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH8016.md)| Private delegates must be documented.| ✔| ❌| ❌|
| [RH8017](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH8017.md)| Non-private constructors must be documented.| ✔| ❌| ❌|
| [RH8018](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH8018.md)| Private constructors must be documented.| ✔| ❌| ❌|
| [RH8019](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH8019.md)| Destructors must be documented.| ✔| ❌| ❌|
| [RH8020](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH8020.md)| Non-private methods must be documented.| ✔| ❌| ❌|
| [RH8021](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH8021.md)| Private methods must be documented.| ✔| ❌| ❌|
| [RH8022](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH8022.md)| Non-private properties must be documented.| ✔| ❌| ❌|
| [RH8023](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH8023.md)| Private properties must be documented.| ✔| ❌| ❌|
| [RH8024](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH8024.md)| Non-private indexers must be documented.| ✔| ❌| ❌|
| [RH8025](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH8025.md)| Private indexers must be documented.| ✔| ❌| ❌|
| [RH8026](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH8026.md)| Non-private fields must be documented.| ✔| ❌| ❌|
| [RH8027](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH8027.md)| Private fields must be documented.| ✔| ❌| ❌|
| [RH8028](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH8028.md)| Non-private events must be documented.| ✔| ❌| ❌|
| [RH8029](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH8029.md)| Private events must be documented.| ✔| ❌| ❌|
| [RH8030](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH8030.md)| Element documentation must have summary text.| ✔| ❌| ❌|
| [RH8031](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH8031.md)| Extension declarations must have summary text.| ✔| ❌| ❌|
| [RH8032](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH8032.md)| Extension declaration parameters must be documented.| ✔| ❌| ❌|
| [RH8033](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH8033.md)| Extension declaration type parameters must be documented.| ✔| ❌| ❌|
| [RH8101](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH8101.md)| Element parameters must be documented.| ✔| ❌| ❌|
| [RH8102](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH8102.md)| Element parameter documentation must match element parameters.| ✔| ❌| ❌|
| [RH8103](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH8103.md)| Element parameter documentation must declare parameter name.| ✔| ❌| ❌|
| [RH8104](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH8104.md)| Element parameter documentation must have text.| ✔| ❌| ❌|
| [RH8105](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH8105.md)| Element return value must be documented.| ✔| ❌| ❌|
| [RH8106](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH8106.md)| Element return value documentation must have text.| ✔| ❌| ❌|
| [RH8107](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH8107.md)| Void return value must not be documented.| ✔| ✔| ❌|
| [RH8108](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH8108.md)| Generic type parameters must be documented.| ✔| ❌| ❌|
| [RH8109](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH8109.md)| Generic type parameter documentation must match type parameters.| ✔| ❌| ❌|
| [RH8110](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH8110.md)| Generic type parameter documentation must declare parameter name.| ✔| ❌| ❌|
| [RH8111](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH8111.md)| Generic type parameter documentation must have text.| ✔| ❌| ❌|
| [RH8201](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH8201.md)| The inheritdoc tag should be used if possible.| ✔| ✔| ❌|
| [RH8202](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH8202.md)| \<value> tag must not be used.| ✔| ✔| ❌|
| [RH8203](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH8203.md)| \<inheritdoc> must be used with inheriting class.| ✔| ❌| ❌|
| [RH8204](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH8204.md)| Do not use placeholder elements.| ✔| ✔| ❌|
| [RH8301](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH8301.md)| Documentation lines must begin with single space.| ✔| ✔| ✔|
| [RH8302](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH8302.md)| Element documentation headers must not be followed by blank line.| ✔| ✔| ✔|
| [RH8303](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH8303.md)| Element documentation header must be preceded by blank line.| ✔| ✔| ✔|
| [RH8304](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH8304.md)| XML documentation elements must be on separate lines.| ✔| ✔| ❌|
| [RH8305](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH8305.md)| Summary element must span at least three lines.| ✔| ✔| ✔|
| [RH8306](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH8306.md)| XML documentation element text must not end with a period.| ✔| ✔| ❌|
| [RH8307](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH8307.md)| Content after opening XML tag must be on same line as closing tag.| ✔| ✔| ✔|
| [RH8308](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH8308.md)| No content should appear after closing XML tags.| ✔| ✔| ❌|
| [RH8401](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH8401.md)| Single-line comments must not use documentation style slashes.| ✔| ✔| ❌|
| [RH8402](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH8402.md)| Files must start with configured copyright header.| ✔| ✔| ❌|
| [RH8501](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH8501.md)| Code analysis suppressions must have a justification.| ✔| ❌| ❌|