# Reihitsu

A collection of C# development tools built on the Roslyn compiler platform, including a code analyzer with code fixes and a standalone formatting CLI tool.

## Packages

| Package | Description | |
|---------|-------------|---|
| **[Reihitsu.Analyzer](Reihitsu.Analyzer.Package/README.MD)** | Roslyn-based analyzer package enforcing coding rules for clarity, design, naming, formatting, documentation, ordering, and performance. | [![NuGet](https://img.shields.io/nuget/v/Reihitsu.Analyzer)](https://www.nuget.org/packages/Reihitsu.Analyzer) |
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

## Showcase
The following is a code snippet that complies with the rules of the analyzer and formatter.

```csharp
/// <summary>
/// Builds report instances from a list of customer orders
/// </summary>
internal sealed class ReportBuilder
{
    #region Fields

    /// <summary>
    /// The orders that can are used to build the report
    /// </summary>
    private readonly IReadOnlyList<Order> _orders;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="ReportBuilder"/> class
    /// </summary>
    /// <param name="orders">The orders that can be used to build reports</param>
    public ReportBuilder(IReadOnlyList<Order> orders)
    {
        _orders = orders ?? throw new ArgumentNullException(nameof(orders));
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Builds a report for a specific customer
    /// </summary>
    /// <param name="customerId">The customer identifier to filter for</param>
    /// <param name="createdOn">The date assigned to the generated report</param>
    /// <param name="includeInactive">A value indicating whether inactive orders should be included</param>
    /// <returns>The generated report</returns>
    public Report Build(string customerId, DateOnly createdOn, bool includeInactive)
    {
        var items = _orders.Where(order => order.CustomerId == customerId)
                           .Where(order => includeInactive || order.IsActive)
                           .Select(order => new ReportItem
                                            {
                                                Id = order.Id,
                                                Lines = order.Lines
                                                             .Select(line => new ReportLine
                                                                             {
                                                                                 Name = line.Name,
                                                                                 Total = line.UnitPrice
                                                                                         * line.Quantity
                                                                             })
                                                             .ToArray()
                                            })
                           .OrderBy(item => item.Id)
                           .ToArray();

        return new Report(customerId,
                          createdOn,
                          items,
                          new ReportOptions
                          {
                              IncludeTotals = true,
                              UseShortDates = false,
                              Labels = [
                                           "stable",
                                           "demo"
                                       ]
                          });
    }

    #endregion // Methods
}
```

## License

This project is licensed under the terms of the [LICENSE](LICENSE) file.

## SonarCloud

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=thoenissen_Reihitsu&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=thoenissen_Reihitsu)
[![Bugs](https://sonarcloud.io/api/project_badges/measure?project=thoenissen_Reihitsu&metric=bugs)](https://sonarcloud.io/summary/new_code?id=thoenissen_Reihitsu)
[![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=thoenissen_Reihitsu&metric=code_smells)](https://sonarcloud.io/summary/new_code?id=thoenissen_Reihitsu)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=thoenissen_Reihitsu&metric=coverage)](https://sonarcloud.io/summary/new_code?id=thoenissen_Reihitsu)
[![Duplicated Lines (%)](https://sonarcloud.io/api/project_badges/measure?project=thoenissen_Reihitsu&metric=duplicated_lines_density)](https://sonarcloud.io/summary/new_code?id=thoenissen_Reihitsu)
