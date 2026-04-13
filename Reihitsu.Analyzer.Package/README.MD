# Reihitsu.Analyzer

A Roslyn-based C# analyzer package that enforces coding rules across multiple categories: Clarity, Design, Naming, Formatting, Documentation, and Performance.

## Installation

```shell
dotnet add package Reihitsu.Analyzer
```

## Rules

| ID| Description| Analyzer| Code Fix|
|--------|-----------------------------------------------------------------------|:--------:|:--------:|
|| **Clarity**|||
| [RH0001](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0001.md)| The logical operator ! should not be used for clarity.| ✔| ✔|
|| **Design**|||
| [RH0101](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0101.md)| Private auto-implemented properties should not be used.| ✔| ✔|
| [RH0102](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0102.md)| Async void methods should not be used.| ✔| ❌|
| [RH0103](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0103.md)| NotImplementedException should not be used.| ✔| ❌|
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
|| **Documentation**|||
| [RH0401](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0401.md)| The \<inheritdoc/> Tag should be used if possible.| ✔| ✔|
|| **Performance**|||
| [RH0501](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0501.md)| Types used as keys must implement equality members.| ✔| ❌|
| [RH0502](https://github.com/thoenissen/Reihitsu/blob/main/documentation/rules/RH0502.md)| Types used for equality comparison must implement equality members.| ✔| ❌|