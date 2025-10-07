using System.Collections.Immutable;
using System.Text.Json;

using Microsoft.CodeAnalysis;

namespace Reihitsu.Analyzer.Data;

/// <summary>
/// Management of the configuration
/// </summary>
internal class ConfigurationManager
{
    /// <summary>
    /// Try to get configuration
    /// </summary>
    /// <param name="additionalFiles">Additional files</param>
    /// <param name="configuration">Configuration</param>
    /// <returns>Is a configuration available?</returns>
    public static bool TryGetConfiguration(ImmutableArray<AdditionalText> additionalFiles, out Configuration configuration)
    {
        configuration = null;

        var file = additionalFiles.FirstOrDefault(file => file.Path.EndsWith("reihitsu.json"));

        if (file != null)
        {
            var text = file.GetText()?.ToString();

            if (string.IsNullOrWhiteSpace(text) == false)
            {
                configuration = new Configuration();

                var jsonDocument = JsonDocument.Parse(text);

                if (jsonDocument.RootElement.TryGetProperty(nameof(Configuration.Naming), out var namingElement))
                {
                    configuration.Naming = new ConfigurationCategoryNaming();

                    if (namingElement.TryGetProperty(nameof(ConfigurationCategoryNaming.AllowedNamespaceDeclarations), out var allowedNamespaceDeclarationsElement)
                        && allowedNamespaceDeclarationsElement.ValueKind == JsonValueKind.Array)
                    {
                        configuration.Naming.AllowedNamespaceDeclarations = allowedNamespaceDeclarationsElement.EnumerateArray()
                                                                                                               .Where(item => item.ValueKind == JsonValueKind.String)
                                                                                                               .Select(item => item.GetString())
                                                                                                               .ToList();
                    }
                }

                return true;
            }
        }

        return false;
    }
}