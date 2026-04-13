# Reihitsu

A collection of C# development tools built on the Roslyn compiler platform, including a code analyzer with code fixes and a standalone formatting CLI tool.

## Packages

| Package | Description | |
|---------|-------------|---|
| **[Reihitsu.Analyzer](Reihitsu.Analyzer.Package/README.MD)** | Roslyn-based analyzer package enforcing coding rules for clarity, design, naming, formatting, documentation, and performance. | [![NuGet](https://img.shields.io/nuget/v/Reihitsu.Analyzer)](https://www.nuget.org/packages/Reihitsu.Analyzer) |
| **[Reihitsu.Cli](Reihitsu.Cli/README.MD)** | Command-line formatting tool for C# source files. | [![NuGet](https://img.shields.io/nuget/v/Reihitsu.Cli)](https://www.nuget.org/packages/Reihitsu.Cli) |

## Quick Start

### Analyzer

```shell
dotnet add package Reihitsu.Analyzer
```

### CLI

```shell
dotnet tool install -g Reihitsu.Cli
reihitsu-format src/
```

## License

This project is licensed under the terms of the [LICENSE](LICENSE) file.

## SonarCloud

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=thoenissen_Reihitsu&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=thoenissen_Reihitsu)
[![Bugs](https://sonarcloud.io/api/project_badges/measure?project=thoenissen_Reihitsu&metric=bugs)](https://sonarcloud.io/summary/new_code?id=thoenissen_Reihitsu)
[![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=thoenissen_Reihitsu&metric=code_smells)](https://sonarcloud.io/summary/new_code?id=thoenissen_Reihitsu)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=thoenissen_Reihitsu&metric=coverage)](https://sonarcloud.io/summary/new_code?id=thoenissen_Reihitsu)
[![Duplicated Lines (%)](https://sonarcloud.io/api/project_badges/measure?project=thoenissen_Reihitsu&metric=duplicated_lines_density)](https://sonarcloud.io/summary/new_code?id=thoenissen_Reihitsu)