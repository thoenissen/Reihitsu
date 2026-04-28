# Reihitsu.Analyzer

A Roslyn-based C# analyzer package that enforces coding rules across multiple categories: Clarity, Design, Naming, Formatting, Documentation, Ordering, and Performance.

## Installation

```shell
dotnet add package Reihitsu.Analyzer
```

## Rules

| ID| Description| Analyzer| Code Fix|
|--------|-----------------------------------------------------------------------|:--------:|:--------:|
|| **Clarity**|||
| [RH0001](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0001.md)| The logical operator ! should not be used for clarity.| ✔| ✔|
| [RH0002](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0002.md)| Do not prefix calls with `base.` unless a local implementation exists.| ✔| ✔|
| [RH0003](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0003.md)| Code must not contain empty statements.| ✔| ✔|
| [RH0004](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0004.md)| Statement must not use unnecessary parentheses.| ✔| ✔|
| [RH0005](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0005.md)| Comments must contain text.| ✔| ✔|
| [RH0006](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0006.md)| Use `string.Empty` for empty strings.| ✔| ✔|
| [RH0007](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0007.md)| Use shorthand for nullable types.| ✔| ✔|
| [RH0008](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0008.md)| Do not use default value type constructor.| ✔| ✔|
| [RH0009](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0009.md)| Use lambda syntax.| ✔| ✔|
| [RH0010](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0010.md)| Use readable conditions.| ✔| ✔|
| [RH0011](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0011.md)| Conditional expressions must declare precedence.| ✔| ✔|
| [RH0012](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0012.md)| Do not prefix local members with `this.`.| ✔| ✔|
| [RH0013](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0013.md)| Do not use query syntax.| ✔| ❌|
|| **Design**|||
| [RH0101](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0101.md)| Private auto-implemented properties should not be used.| ✔| ✔|
| [RH0102](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0102.md)| Async void methods should not be used.| ✔| ❌|
| [RH0103](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0103.md)| NotImplementedException should not be used.| ✔| ❌|
| [RH0104](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0104.md)| Access modifier must be declared.| ✔| ✔|
| [RH0105](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0105.md)| Fields must be private.| ✔| ✔|
| [RH0106](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0106.md)| File may only contain a single namespace.| ✔| ❌|
| [RH0107](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0107.md)| Code analysis suppressions must have a justification.| ✔| ❌|
| [RH0108](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0108.md)| Debug.Assert must provide message text.| ✔| ❌|
| [RH0109](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0109.md)| Debug.Fail must provide message text.| ✔| ❌|
| [RH0110](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0110.md)| Unnecessary delegate parentheses should be removed.| ✔| ✔|
| [RH0111](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0111.md)| Unnecessary attribute constructor parentheses should be removed.| ✔| ✔|
| [RH0112](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0112.md)| File may only contain a single top-level type.| ✔| ❌|
|| **Naming**|||
| [RH0201](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0201.md)| The name of the type should match the filename.| ✔| ✔|
| [RH0202](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0202.md)| Class names should be in PascalCase.| ✔| ✔|
| [RH0203](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0203.md)| Struct names should be in PascalCase.| ✔| ✔|
| [RH0204](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0204.md)| Enum names should be in PascalCase.| ✔| ✔|
| [RH0205](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0205.md)| Enum member names should be in PascalCase.| ✔| ✔|
| [RH0206](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0206.md)| Interface names should be in PascalCase.| ✔| ✔|
| [RH0207](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0207.md)| Event names should be in PascalCase.| ✔| ✔|
| [RH0208](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0208.md)| Delegate names should be in PascalCase.| ✔| ✔|
| [RH0209](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0209.md)| Method names should be in PascalCase.| ✔| ✔|
| [RH0210](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0210.md)| Local function names should be in PascalCase.| ✔| ✔|
| [RH0211](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0211.md)| Method parameter names should be in camelCase.| ✔| ✔|
| [RH0212](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0212.md)| Private field names should be in camelCase.| ✔| ✔|
| [RH0213](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0213.md)| Protected field names should be in PascalCase.| ✔| ✔|
| [RH0214](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0214.md)| Internal field names should be in PascalCase.| ✔| ✔|
| [RH0215](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0215.md)| Public field names should be in PascalCase.| ✔| ✔|
| [RH0216](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0216.md)| Const field names should be in PascalCase.| ✔| ✔|
| [RH0217](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0217.md)| Private property names should be in PascalCase.| ✔| ✔|
| [RH0218](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0218.md)| Protected property names should be in PascalCase.| ✔| ✔|
| [RH0219](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0219.md)| Internal property names should be in PascalCase.| ✔| ✔|
| [RH0220](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0220.md)| Public property names should be in PascalCase.| ✔| ✔|
| [RH0221](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0221.md)| Local variable names should be in camelCase.| ✔| ✔|
| [RH0222](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0222.md)| Tuple element names should be in camelCase.| ✔| ✔|
| [RH0223](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0223.md)| Deconstruction variable names should be in camelCase.| ✔| ✔|
| [RH0224](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0224.md)| Tuple expression argument names should be in camelCase.| ✔| ✔|
| [RH0225](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0225.md)| File scoped namespace declarations should be in PascalCase.| ✔| ❌|
| [RH0226](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0226.md)| Namespace declarations should be in PascalCase.| ✔| ❌|
| [RH0227](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0227.md)| The given namespace is not allowed (see reihitsu.json)| ✔| ❌|
| [RH0228](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0228.md)| Static readonly field names should be in PascalCase.| ✔| ✔|
|| **Formatting**|||
| [RH0301](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0301.md)| The description of the #region and #endregion should match.| ✔| ✔|
| [RH0302](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0302.md)| The object initializer should be formatted correctly.| ✔| ✔|
| [RH0303](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0303.md)| The try-Statement should be preceded by a blank line.| ✔| ✔|
| [RH0304](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0304.md)| The if-Statement should be preceded by a blank line.| ✔| ✔|
| [RH0305](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0305.md)| The while-Statement should be preceded by a blank line.| ✔| ✔|
| [RH0306](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0306.md)| The do-Statement should be preceded by a blank line.| ✔| ✔|
| [RH0307](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0307.md)| The using-Statement should be preceded by a blank line.| ✔| ✔|
| [RH0308](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0308.md)| The foreach-Statement should be preceded by a blank line.| ✔| ✔|
| [RH0309](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0309.md)| The for-Statement should be preceded by a blank line.| ✔| ✔|
| [RH0310](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0310.md)| The return-Statement should be preceded by a blank line.| ✔| ✔|
| [RH0311](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0311.md)| The goto-Statement should be preceded by a blank line.| ✔| ✔|
| [RH0312](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0312.md)| The break-Statement should be preceded by a blank line.| ✔| ✔|
| [RH0313](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0313.md)| The break statement should be followed by a blank line.| ✔| ❌|
| [RH0314](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0314.md)| The continue-Statement should be preceded by a blank line.| ✔| ✔|
| [RH0315](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0315.md)| The throw-Statement should be preceded by a blank line.| ✔| ✔|
| [RH0316](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0316.md)| The switch-Statement should be preceded by a blank line.| ✔| ✔|
| [RH0317](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0317.md)| The checked-Statement should be preceded by a blank line.| ✔| ✔|
| [RH0318](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0318.md)| The unchecked-Statement should be preceded by a blank line.| ✔| ✔|
| [RH0319](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0319.md)| The fixed-Statement should be preceded by a blank line.| ✔| ✔|
| [RH0320](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0320.md)| The lock-Statement should be preceded by a blank line.| ✔| ✔|
| [RH0321](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0321.md)| The yield-Statement should be preceded by a blank line.| ✔| ✔|
| [RH0322](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0322.md)| Single line comments should be preceded by a blank line.| ❌| ❌|
| [RH0323](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0323.md)| Assignments and statements should be separated by a blank line.| ❌| ❌|
| [RH0324](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0324.md)| Method chains should be aligned.| ✔| ✔|
| [RH0325](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0325.md)| Expression style methods should not be used.| ✔| ✔|
| [RH0326](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0326.md)| Expression style constructors should not be used.| ✔| ❌|
| [RH0327](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0327.md)| Expression style get only properties should be single lined.| ✔| ❌|
| [RH0328](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0328.md)| The description of the #region should start with an uppercase letter.| ✔| ❌|
| [RH0329](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0329.md)| Logical expressions should be formatted correctly.| ✔| ✔|
| [RH0330](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0330.md)| Raw string literals should be formatted correctly.| ✔| ✔|
| [RH0331](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0331.md)| The first argument should be on the same line as the opening parenthesis.| ✔| ✔|
| [RH0332](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0332.md)| Arguments should either all be on one line or each on its own line.| ✔| ✔|
| [RH0333](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0333.md)| Multi-line arguments should be aligned.| ✔| ✔|
| [RH0384](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0384.md)| Generic type constraints should be on their own line with proper indentation.| ✔| ✔|
|| **Documentation**|||
| [RH0401](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0401.md)| The \<inheritdoc/> Tag should be used if possible.| ✔| ✔|
| [RH0402](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0402.md)| Elements must be documented.| ✔| ❌|
| [RH0403](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0403.md)| Partial elements must be documented.| ✔| ❌|
| [RH0404](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0404.md)| Element documentation must have summary.| ✔| ❌|
| [RH0405](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0405.md)| Partial element documentation must have summary.| ✔| ❌|
| [RH0406](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0406.md)| Element documentation must have summary text.| ✔| ❌|
| [RH0407](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0407.md)| Partial element documentation must have summary text.| ✔| ❌|
| [RH0408](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0408.md)| Element documentation must not have default summary.| ✔| ❌|
| [RH0409](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0409.md)| Property documentation must have value text.| ✔| ❌|
| [RH0410](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0410.md)| Non-private record structs must be documented.| ✔| ❌|
| [RH0411](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0411.md)| Private record structs must be documented.| ✔| ❌|
| [RH0412](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0412.md)| Non-private enums must be documented.| ✔| ❌|
| [RH0413](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0413.md)| Private enums must be documented.| ✔| ❌|
| [RH0414](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0414.md)| Enum members in non-private enums must be documented.| ✔| ❌|
| [RH0415](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0415.md)| Enum members in private enums must be documented.| ✔| ❌|
| [RH0416](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0416.md)| Non-private delegates must be documented.| ✔| ❌|
| [RH0417](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0417.md)| Private delegates must be documented.| ✔| ❌|
| [RH0418](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0418.md)| Non-private constructors must be documented.| ✔| ❌|
| [RH0419](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0419.md)| Private constructors must be documented.| ✔| ❌|
| [RH0420](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0420.md)| Destructors must be documented.| ✔| ❌|
| [RH0421](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0421.md)| Non-private methods must be documented.| ✔| ❌|
| [RH0422](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0422.md)| Private methods must be documented.| ✔| ❌|
| [RH0423](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0423.md)| Non-private properties must be documented.| ✔| ❌|
| [RH0424](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0424.md)| Private properties must be documented.| ✔| ❌|
| [RH0425](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0425.md)| Non-private indexers must be documented.| ✔| ❌|
| [RH0426](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0426.md)| Private indexers must be documented.| ✔| ❌|
| [RH0427](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0427.md)| Non-private fields must be documented.| ✔| ❌|
| [RH0428](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0428.md)| Private fields must be documented.| ✔| ❌|
| [RH0429](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0429.md)| Non-private events must be documented.| ✔| ❌|
| [RH0430](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0430.md)| Private events must be documented.| ✔| ❌|
| [RH0431](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0431.md)| Element documentation must have summary text.| ✔| ❌|
| [RH0432](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0432.md)| \<value> tag must not be used.| ✔| ❌|
| [RH0433](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0433.md)| Element parameters must be documented.| ✔| ❌|
| [RH0434](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0434.md)| Element parameter documentation must match element parameters.| ✔| ❌|
| [RH0435](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0435.md)| Element parameter documentation must declare parameter name.| ✔| ❌|
| [RH0436](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0436.md)| Element parameter documentation must have text.| ✔| ❌|
| [RH0437](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0437.md)| Element return value must be documented.| ✔| ❌|
| [RH0438](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0438.md)| Element return value documentation must have text.| ✔| ❌|
| [RH0439](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0439.md)| Void return value must not be documented.| ✔| ✔|
| [RH0440](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0440.md)| Generic type parameters must be documented.| ✔| ❌|
| [RH0441](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0441.md)| Generic type parameter documentation must match type parameters.| ✔| ❌|
| [RH0442](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0442.md)| Generic type parameter documentation must declare parameter name.| ✔| ❌|
| [RH0443](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0443.md)| Generic type parameter documentation must have text.| ✔| ❌|
| [RH0444](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0444.md)| Single-line comments must not use documentation style slashes.| ✔| ✔|
| [RH0445](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0445.md)| \<inheritdoc/> must be used with inheriting class.| ✔| ❌|
| [RH0446](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0446.md)| Do not use placeholder elements.| ✔| ✔|
| [RH0447](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0447.md)| XML documentation elements must be on separate lines.| ✔| ✔|
| [RH0448](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0448.md)| Summary element must span at least three lines.| ✔| ✔|
| [RH0449](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0449.md)| XML documentation element text must not end with a period.| ✔| ✔|
| [RH0450](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0450.md)| Content after opening XML tag must be on same line as closing tag.| ✔| ✔|
|| **Ordering**|||
| [RH0601](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0601.md)| Constants must appear before fields.| ✔| ✔|
| [RH0602](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0602.md)| Static elements must appear before instance elements.| ✔| ✔|
| [RH0603](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0603.md)| Partial elements must declare access modifier.| ✔| ✔|
| [RH0604](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0604.md)| Declaration keywords must follow order.| ✔| ✔|
| [RH0605](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0605.md)| Protected must come before internal.| ✔| ✔|
| [RH0606](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0606.md)| System using directives must be placed before other using directives.| ✔| ✔|
| [RH0607](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0607.md)| Using alias directives must be placed after other using directives.| ✔| ✔|
| [RH0608](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0608.md)| Using directives must be ordered alphabetically by namespace.| ✔| ✔|
| [RH0609](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0609.md)| Using alias directives must be ordered alphabetically by alias name.| ✔| ✔|
| [RH0610](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0610.md)| Property accessors must follow order.| ✔| ✔|
| [RH0611](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0611.md)| Event accessors must follow order.| ✔| ✔|
| [RH0612](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0612.md)| Readonly elements must appear before non-readonly elements.| ✔| ✔|
| [RH0613](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0613.md)| Using static directives must be placed at correct position.| ✔| ✔|
| [RH0614](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0614.md)| Using static directives must be ordered alphabetically.| ✔| ✔|
|| **Performance**|||
| [RH0501](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0501.md)| Types used as keys must implement equality members.| ✔| ❌|
| [RH0502](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0502.md)| Types used for equality comparison must implement equality members.| ✔| ❌|
