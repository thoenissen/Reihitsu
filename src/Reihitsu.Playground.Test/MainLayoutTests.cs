using System.Linq;
using System.Reflection;

using Bunit;

using Microsoft.AspNetCore.Components;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter;
using Reihitsu.Playground.Layout;

namespace Reihitsu.Playground.Test;

/// <summary>
/// Contains component tests for the playground <see cref="MainLayout"/>, rendered with bUnit
/// </summary>
[TestClass]
public class MainLayoutTests
{
    #region Methods

    /// <summary>
    /// Verifies that the footer shows the informational version of the formatter assembly as <c>v&lt;version&gt;</c>
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-007")]
    public void FooterShowsTheFormatterInformationalVersion()
    {
        var version = typeof(ReihitsuFormatter).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

        Assert.IsFalse(string.IsNullOrEmpty(version), "The formatter assembly must carry an informational version.");

        using (var context = new BunitContext())
        {
            var layout = RenderLayout(context);
            var footerTexts = layout.FindAll("footer span").Select(span => span.TextContent).ToList();

            Assert.Contains($"v{version}", footerTexts);
        }
    }

    /// <summary>
    /// Verifies that the header links to the GitHub repository
    /// </summary>
    [TestMethod]
    [Requirement("PLAY-007")]
    public void HeaderLinksToTheRepository()
    {
        using (var context = new BunitContext())
        {
            var layout = RenderLayout(context);
            var link = layout.Find("header a");

            Assert.AreEqual("https://github.com/thoenissen/Reihitsu", link.GetAttribute("href"));
        }
    }

    /// <summary>
    /// Renders the layout around a placeholder body
    /// </summary>
    /// <param name="context">The bUnit context to render in</param>
    /// <returns>The rendered layout</returns>
    private static IRenderedComponent<MainLayout> RenderLayout(BunitContext context)
    {
        RenderFragment body = builder => builder.AddContent(0, "Body");

        return context.Render<MainLayout>(parameters => parameters.Add(layout => layout.Body, body));
    }

    #endregion // Methods
}