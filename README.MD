| ID     | Description                                                           | Analyzer | Code Fix |
|--------|-----------------------------------------------------------------------|:--------:|:--------:|
|        | **Claritiy**                                                          |          |          |
| [RH0001](documentation/rules/RH0001.md) | The logical operator ! should not be used for clarity.                | &#10004; | &#10004; |
|        | **Design**                                                            |          |          |         
| [RH0101](documentation/rules/RH0101.md) | Private auto-implemented properties should not be used.               | &#10004; | &#10004; |
| [RH0102](documentation/rules/RH0102.md) | Async void methods should not be used.                                | &#10004; | &#10060; |
| [RH0103](documentation/rules/RH0103.md) | NotImplementedException should not be used.                           | &#10004; | &#10060; |
|        | **Naming**                                                            |          |          |
| [RH0201](documentation/rules/RH0201.md) | The name of the type should match the filename.                       | &#10004; | &#10004; |
| [RH0202](documentation/rules/RH0202.md) | Class names should be in PascalCase.                                  | &#10004; | &#10004; |
| [RH0203](documentation/rules/RH0203.md) | Struct names should be in PascalCase.                                 | &#10004; | &#10004; |
| [RH0204](documentation/rules/RH0204.md) | Enum names should be in PascalCase.                                   | &#10004; | &#10004; |
| [RH0205](documentation/rules/RH0205.md) | Enum member names should be in PascalCase.                            | &#10004; | &#10004; |
| [RH0206](documentation/rules/RH0206.md) | Interface names should be in PascalCase.                              | &#10004; | &#10004; |
| [RH0207](documentation/rules/RH0207.md) | Event names should be in PascalCase.                                  | &#10004; | &#10004; |
| [RH0208](documentation/rules/RH0208.md) | Delegate names should be in PascalCase.                               | &#10004; | &#10004; |
| [RH0209](documentation/rules/RH0209.md) | Method names should be in PascalCase.                                 | &#10004; | &#10004; |
| [RH0210](documentation/rules/RH0210.md) | Local function names should be in PascalCase.                         | &#10004; | &#10004; |
| [RH0211](documentation/rules/RH0211.md) | Method parameter names should be in camelCase.                        | &#10004; | &#10004; |
| [RH0212](documentation/rules/RH0212.md) | Private field names should be in camelCase.                           | &#10004; | &#10004; |
| [RH0213](documentation/rules/RH0213.md) | Protected field names should be in PascalCase.                        | &#10004; | &#10004; |
| [RH0214](documentation/rules/RH0214.md) | Internal field names should be in PascalCase.                         | &#10004; | &#10004; |
| [RH0215](documentation/rules/RH0215.md) | Public field names should be in PascalCase.                           | &#10004; | &#10004; |
| [RH0216](documentation/rules/RH0216.md) | Const field names should be in PascalCase.                            | &#10004; | &#10004; |
| [RH0217](documentation/rules/RH0217.md) | Private property names should be in PascalCase.                       | &#10004; | &#10004; |
| [RH0218](documentation/rules/RH0218.md) | Protected property names should be in PascalCase.                     | &#10004; | &#10004; |
| [RH0219](documentation/rules/RH0219.md) | Internal property names should be in PascalCase.                      | &#10004; | &#10004; |
| [RH0220](documentation/rules/RH0220.md) | Public property names should be in PascalCase.                        | &#10004; | &#10004; |
| [RH0221](documentation/rules/RH0221.md) | Local variable names should be in camelCase.                          | &#10004; | &#10004; |
| [RH0222](documentation/rules/RH0222.md) | Tuple element names should be in camelCase.                           | &#10004; | &#10004; |
| [RH0223](documentation/rules/RH0223.md) | Deconstruction variable names should be in camelCase.                 | &#10004; | &#10004; |
| [RH0224](documentation/rules/RH0224.md) | Tuple expression argument names should be in camelCase.               | &#10004; | &#10004; |
| [RH0225](documentation/rules/RH0225.md) | File scoped namespace declarations should be in PascalCase.           | &#10004; | &#10060; |
| [RH0226](documentation/rules/RH0226.md) | Namespace declarations should be in PascalCase.                       | &#10004; | &#10060; |
| [RH0227](documentation/rules/RH0227.md) | The given namespace is not allowed (see reihitsu.json)                | &#10004; | &#10060; |
|        | **Formatting**                                                        |          |          |
| [RH0301](documentation/rules/RH0301.md) | The description of the #region and #endregion should match.           | &#10004; | &#10004; |
| [RH0302](documentation/rules/RH0302.md) | The object initializer should be formatted correctly.                 | &#10004; | &#10004; |
| [RH0303](documentation/rules/RH0303.md) | The try-Statement should be preceded by a blank line.                 | &#10004; | &#10004; |
| [RH0304](documentation/rules/RH0304.md) | The if-Statement should be preceded by a blank line.                  | &#10004; | &#10060; |
| [RH0305](documentation/rules/RH0305.md) | The while-Statement should be preceded by a blank line.               | &#10004; | &#10060; |
| [RH0306](documentation/rules/RH0306.md) | The do-Statement should be preceded by a blank line.                  | &#10004; | &#10060; |
| [RH0307](documentation/rules/RH0307.md) | The using-Statement should be preceded by a blank line.               | &#10004; | &#10060; |
| [RH0308](documentation/rules/RH0308.md) | The foreach-Statement should be preceded by a blank line.             | &#10004; | &#10060; |
| [RH0309](documentation/rules/RH0309.md) | The for-Statement should be preceded by a blank line.                 | &#10004; | &#10060; |
| [RH0310](documentation/rules/RH0310.md) | The return-Statement should be preceded by a blank line.              | &#10004; | &#10060; |
| [RH0311](documentation/rules/RH0311.md) | The goto-Statement should be preceded by a blank line.                | &#10004; | &#10060; |
| [RH0312](documentation/rules/RH0312.md) | The break-Statement should be preceded by a blank line.               | &#10004; | &#10060; |
| [RH0313](documentation/rules/RH0313.md) | The break statement should be followed by a blank line.               | &#10004; | &#10060; |
| [RH0314](documentation/rules/RH0314.md) | The continue-Statement should be preceded by a blank line.            | &#10004; | &#10060; |
| [RH0315](documentation/rules/RH0315.md) | The throw-Statement should be preceded by a blank line.               | &#10004; | &#10060; |
| [RH0316](documentation/rules/RH0316.md) | The switch-Statement should be preceded by a blank line.              | &#10004; | &#10060; |
| [RH0317](documentation/rules/RH0317.md) | The checked-Statement should be preceded by a blank line.             | &#10004; | &#10060; |
| [RH0318](documentation/rules/RH0318.md) | The unchecked-Statement should be preceded by a blank line.           | &#10004; | &#10060; |
| [RH0319](documentation/rules/RH0319.md) | The fixed-Statement should be preceded by a blank line.               | &#10004; | &#10060; |
| [RH0320](documentation/rules/RH0320.md) | The lock-Statement should be preceded by a blank line.                | &#10004; | &#10060; |
| [RH0321](documentation/rules/RH0321.md) | The yield-Statement should be preceded by a blank line.               | &#10004; | &#10060; |
| [RH0322](documentation/rules/RH0322.md) | Single line comments should be preceded by a blank line.              | &#10060; | &#10060; |
| [RH0323](documentation/rules/RH0323.md) | Assignments and statements should be separated by a blank line.       | &#10060; | &#10060; |
| [RH0324](documentation/rules/RH0324.md) | Method chains should be aligned.                                      | &#10004; | &#10004; |
| [RH0325](documentation/rules/RH0325.md) | Expression style methods should not be used.                          | &#10004; | &#10060; |
| [RH0326](documentation/rules/RH0326.md) | Expression style constructors should not be used.                     | &#10004; | &#10060; |
| [RH0327](documentation/rules/RH0327.md) | Expression style get only properties should be single lined.          | &#10004; | &#10060; |
| [RH0328](documentation/rules/RH0328.md) | The description of the #region should start with an uppercase letter. | &#10004; | &#10060; |
| [RH0329](documentation/rules/RH0329.md) | Logical expressions should be formatted correctly.                    | &#10004; | &#10004; |
|        | **Documentation**                                                     |          |          |
| [RH0401](documentation/rules/RH0401.md) | The \<inheritdoc/> Tag should be used if possible.                    | &#10004; | &#10004; |
|        | **Performance**                                                       |          |          |
| [RH0501](documentation/rules/RH0501.md) | Types used as keys must implement equality members.                   | &#10004; | &#10060; |
| [RH0502](documentation/rules/RH0502.md) | Types used for equality comparison must implement equality members.   | &#10004; | &#10060; |


# SonarCloud
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=thoenissen_Reihitsu&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=thoenissen_Reihitsu)
[![Bugs](https://sonarcloud.io/api/project_badges/measure?project=thoenissen_Reihitsu&metric=bugs)](https://sonarcloud.io/summary/new_code?id=thoenissen_Reihitsu)
[![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=thoenissen_Reihitsu&metric=code_smells)](https://sonarcloud.io/summary/new_code?id=thoenissen_Reihitsu)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=thoenissen_Reihitsu&metric=coverage)](https://sonarcloud.io/summary/new_code?id=thoenissen_Reihitsu)
[![Duplicated Lines (%)](https://sonarcloud.io/api/project_badges/measure?project=thoenissen_Reihitsu&metric=duplicated_lines_density)](https://sonarcloud.io/summary/new_code?id=thoenissen_Reihitsu)