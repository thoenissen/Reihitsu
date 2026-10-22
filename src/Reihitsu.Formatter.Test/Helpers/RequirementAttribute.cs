using System;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.Formatter.Test.Helpers;

/// <summary>
/// Names a spec requirement that the test method covers. The ID is also exposed as a test category, so
/// <c>--filter "TestCategory=&lt;ID&gt;"</c> runs every test of one requirement. <c>RequirementCoverageTests</c> in
/// <c>Reihitsu.ArchitectureTests</c> reads this attribute from the test sources, so the ID must be a string literal
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class RequirementAttribute : TestCategoryBaseAttribute
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="id">Requirement ID as it heads the requirement in its spec, such as <c>RH3202-001</c></param>
    public RequirementAttribute(string id)
    {
        Id = id;
        TestCategories = [id];
    }

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// Requirement ID
    /// </summary>
    public string Id { get; }

    #endregion // Properties

    #region TestCategoryBaseAttribute

    /// <inheritdoc/>
    public override IList<string> TestCategories { get; }

    #endregion // TestCategoryBaseAttribute
}