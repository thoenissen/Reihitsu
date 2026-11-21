namespace Reihitsu.Analyzer.Data;

/// <summary>
/// Category: Design
/// </summary>
internal class ConfigurationCategoryDesign
{
    #region Constants

    /// <summary>
    /// Configuration property name: maximumMethodLines
    /// </summary>
    public const string MaximumMethodLinesPropertyName = "maximumMethodLines";

    /// <summary>
    /// Configuration property name: maximumTestMethodLines
    /// </summary>
    public const string MaximumTestMethodLinesPropertyName = "maximumTestMethodLines";

    /// <summary>
    /// Configuration property name: maximumTypeLines
    /// </summary>
    public const string MaximumTypeLinesPropertyName = "maximumTypeLines";

    /// <summary>
    /// Configuration property name: maximumTestTypeLines
    /// </summary>
    public const string MaximumTestTypeLinesPropertyName = "maximumTestTypeLines";

    /// <summary>
    /// Configuration property name: maximumMethodComplexity
    /// </summary>
    public const string MaximumMethodComplexityPropertyName = "maximumMethodComplexity";

    #endregion // Constants

    #region Properties

    /// <summary>
    /// Limit of lines of code of a member's body in production code, or <see langword="null"/> when the setting has no valid value
    /// </summary>
    public int? MaximumMethodLines { get; set; }

    /// <summary>
    /// Limit of lines of code of a member's body in test code, or <see langword="null"/> when the setting has no valid value
    /// </summary>
    public int? MaximumTestMethodLines { get; set; }

    /// <summary>
    /// Limit of lines of code of a type's body in production code, or <see langword="null"/> when the setting has no valid value
    /// </summary>
    public int? MaximumTypeLines { get; set; }

    /// <summary>
    /// Limit of lines of code of a type's body in test code, or <see langword="null"/> when the setting has no valid value
    /// </summary>
    public int? MaximumTestTypeLines { get; set; }

    /// <summary>
    /// Limit of the cognitive complexity of a member's body, or <see langword="null"/> when the setting has no valid value
    /// </summary>
    public int? MaximumMethodComplexity { get; set; }

    #endregion // Properties
}