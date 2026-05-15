using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Text.Json;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Reihitsu.Analyzer.Data;

/// <summary>
/// Management of the configuration
/// </summary>
internal static class ConfigurationManager
{
    #region Constants

    /// <summary>
    /// Configuration file name
    /// </summary>
    private const string ConfigurationFileName = "reihitsu.json";

    /// <summary>
    /// Configuration section name: copyright
    /// </summary>
    private const string CopyrightSectionName = "copyright";

    /// <summary>
    /// Configuration section name: naming
    /// </summary>
    private const string NamingSectionName = "naming";

    /// <summary>
    /// Configuration property name: allowedNamespaceDeclarations
    /// </summary>
    private const string AllowedNamespaceDeclarationsPropertyName = "allowedNamespaceDeclarations";

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Gets the configuration
    /// </summary>
    /// <param name="additionalFiles">Additional files</param>
    /// <returns>Configuration load result</returns>
    public static ConfigurationLoadResult GetConfiguration(ImmutableArray<AdditionalText> additionalFiles)
    {
        var file = additionalFiles.FirstOrDefault(currentFile => currentFile.Path.EndsWith(ConfigurationFileName, StringComparison.OrdinalIgnoreCase));

        if (file == null)
        {
            return new ConfigurationLoadResult();
        }

        var text = file.GetText();
        var result = new ConfigurationLoadResult
                     {
                         File = file,
                         Text = text
                     };

        if (text == null)
        {
            result.Errors = [CreateValidationError("The configuration file could not be read.")];

            return result;
        }

        if (string.IsNullOrWhiteSpace(text.ToString()))
        {
            result.Errors = [CreateValidationError("The configuration file must not be empty or whitespace-only.")];

            return result;
        }

        try
        {
            result.Configuration = ParseConfiguration(text, out var errors);
            result.Errors = [.. errors];
        }
        catch (JsonException exception)
        {
            result.Errors = [CreateValidationError("The configuration file contains invalid JSON syntax.", GetExceptionSpan(text, exception))];
        }

        return result;
    }

    /// <summary>
    /// Parse configuration
    /// </summary>
    /// <param name="text">Text</param>
    /// <param name="errors">Errors</param>
    /// <returns>Configuration</returns>
    private static Configuration ParseConfiguration(SourceText text, out List<ConfigurationValidationError> errors)
    {
        var configuration = new Configuration();

        errors = [];

        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(text.ToString()));

        if (reader.Read() == false)
        {
            errors.Add(CreateValidationError("The configuration file must not be empty or whitespace-only."));

            return configuration;
        }

        if (reader.TokenType != JsonTokenType.StartObject)
        {
            errors.Add(CreateValidationError("The configuration root must be a JSON object.", GetCurrentValueSpan(ref reader)));

            return configuration;
        }

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                break;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                continue;
            }

            var propertyName = reader.GetString();
            var propertySpan = GetPropertyNameSpan(ref reader);

            reader.Read();

            switch (propertyName)
            {
                case CopyrightSectionName:
                    {
                        ParseCopyright(ref reader, configuration, errors);
                    }
                    break;

                case NamingSectionName:
                    {
                        ParseNaming(ref reader, configuration, errors);
                    }
                    break;

                default:
                    {
                        errors.Add(CreateValidationError($"Unknown configuration section '{propertyName}'.", propertySpan));

                        SkipValue(ref reader);
                    }
                    break;
            }
        }

        return configuration;
    }

    /// <summary>
    /// Parse copyright category
    /// </summary>
    /// <param name="reader">Reader</param>
    /// <param name="configuration">Configuration</param>
    /// <param name="errors">Errors</param>
    private static void ParseCopyright(ref Utf8JsonReader reader, Configuration configuration, List<ConfigurationValidationError> errors)
    {
        var copyrightSectionSpan = GetCurrentValueSpan(ref reader);

        if (reader.TokenType != JsonTokenType.StartObject)
        {
            errors.Add(CreateValidationError($"The '{CopyrightSectionName}' section must be a JSON object.", copyrightSectionSpan));

            SkipValue(ref reader);

            return;
        }

        configuration.Copyright ??= new ConfigurationCategoryCopyright();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                break;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                continue;
            }

            var propertyName = reader.GetString();
            var propertySpan = GetPropertyNameSpan(ref reader);

            reader.Read();

            if (propertyName == ConfigurationCategoryCopyright.CopyrightTextPropertyName)
            {
                ParseCopyrightText(ref reader, configuration.Copyright, errors);

                continue;
            }

            ParseCopyrightPlaceholderValue(ref reader, configuration.Copyright, propertyName, errors, propertySpan);
        }

        ValidateCopyrightConfiguration(configuration.Copyright, errors, copyrightSectionSpan);
    }

    /// <summary>
    /// Parse copyright text
    /// </summary>
    /// <param name="reader">Reader</param>
    /// <param name="copyrightConfiguration">Copyright configuration</param>
    /// <param name="errors">Errors</param>
    private static void ParseCopyrightText(ref Utf8JsonReader reader, ConfigurationCategoryCopyright copyrightConfiguration, List<ConfigurationValidationError> errors)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            errors.Add(CreateValidationError($"The '{CopyrightSectionName}.{ConfigurationCategoryCopyright.CopyrightTextPropertyName}' setting must be a string.", GetCurrentValueSpan(ref reader)));

            SkipValue(ref reader);

            return;
        }

        copyrightConfiguration.CopyrightText = reader.GetString();
    }

    /// <summary>
    /// Parse copyright placeholder value
    /// </summary>
    /// <param name="reader">Reader</param>
    /// <param name="copyrightConfiguration">Copyright configuration</param>
    /// <param name="propertyName">Property name</param>
    /// <param name="errors">Errors</param>
    /// <param name="propertySpan">Property span</param>
    private static void ParseCopyrightPlaceholderValue(ref Utf8JsonReader reader, ConfigurationCategoryCopyright copyrightConfiguration, string propertyName, List<ConfigurationValidationError> errors, TextSpan propertySpan)
    {
        if (propertyName == ConfigurationCategoryCopyright.FileNamePlaceholderName)
        {
            errors.Add(CreateValidationError($"The '{CopyrightSectionName}.{ConfigurationCategoryCopyright.FileNamePlaceholderName}' setting is reserved and must not be configured.", propertySpan));

            SkipValue(ref reader);

            return;
        }

        if (reader.TokenType != JsonTokenType.String)
        {
            errors.Add(CreateValidationError($"The '{CopyrightSectionName}.{propertyName}' setting must be a string.", GetCurrentValueSpan(ref reader)));

            SkipValue(ref reader);

            return;
        }

        copyrightConfiguration.Placeholders[propertyName] = reader.GetString();
    }

    /// <summary>
    /// Validate copyright configuration
    /// </summary>
    /// <param name="copyrightConfiguration">Copyright configuration</param>
    /// <param name="errors">Errors</param>
    /// <param name="copyrightSectionSpan">Copyright section span</param>
    private static void ValidateCopyrightConfiguration(ConfigurationCategoryCopyright copyrightConfiguration, List<ConfigurationValidationError> errors, TextSpan copyrightSectionSpan)
    {
        if (string.IsNullOrWhiteSpace(copyrightConfiguration.CopyrightText))
        {
            errors.Add(CreateValidationError($"The '{CopyrightSectionName}.{ConfigurationCategoryCopyright.CopyrightTextPropertyName}' setting is required and must not be empty.", copyrightSectionSpan));

            return;
        }

        if (IsSupportedCopyrightHeader(copyrightConfiguration.CopyrightText) == false)
        {
            errors.Add(CreateValidationError($"The '{CopyrightSectionName}.{ConfigurationCategoryCopyright.CopyrightTextPropertyName}' setting must be a comment header that uses either '//' line comments or '/* */' block comments.", copyrightSectionSpan));
        }

        foreach (var placeholder in CopyrightHeaderTemplateResolver.GetPlaceholders(copyrightConfiguration.CopyrightText))
        {
            if (placeholder == ConfigurationCategoryCopyright.FileNamePlaceholderName)
            {
                continue;
            }

            if (copyrightConfiguration.Placeholders.ContainsKey(placeholder) == false)
            {
                errors.Add(CreateValidationError($"The placeholder '{placeholder}' used in '{CopyrightSectionName}.{ConfigurationCategoryCopyright.CopyrightTextPropertyName}' has no matching setting in '{CopyrightSectionName}'.", copyrightSectionSpan));
            }
        }
    }

    /// <summary>
    /// Determines whether a copyright header uses a supported comment style
    /// </summary>
    /// <param name="copyrightText">Copyright text</param>
    /// <returns><see langword="true"/> when the header uses line or block comments; otherwise <see langword="false"/></returns>
    private static bool IsSupportedCopyrightHeader(string copyrightText)
    {
        if (string.IsNullOrWhiteSpace(copyrightText))
        {
            return false;
        }

        var normalizedText = copyrightText.Replace("\r\n", "\n")
                                          .Replace('\r', '\n')
                                          .Trim();

        if (normalizedText.StartsWith("//", StringComparison.Ordinal))
        {
            var lines = normalizedText.Split('\n');

            return Array.TrueForAll(lines, currentLine => currentLine.StartsWith("//", StringComparison.Ordinal));
        }

        return normalizedText.StartsWith("/*", StringComparison.Ordinal)
               && normalizedText.EndsWith("*/", StringComparison.Ordinal);
    }

    /// <summary>
    /// Parse naming category
    /// </summary>
    /// <param name="reader">Reader</param>
    /// <param name="configuration">Configuration</param>
    /// <param name="errors">Errors</param>
    private static void ParseNaming(ref Utf8JsonReader reader, Configuration configuration, List<ConfigurationValidationError> errors)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            errors.Add(CreateValidationError($"The '{NamingSectionName}' section must be a JSON object.", GetCurrentValueSpan(ref reader)));

            SkipValue(ref reader);

            return;
        }

        configuration.Naming ??= new ConfigurationCategoryNaming();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                break;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                continue;
            }

            var propertyName = reader.GetString();
            var propertySpan = GetPropertyNameSpan(ref reader);

            reader.Read();

            switch (propertyName)
            {
                case AllowedNamespaceDeclarationsPropertyName:
                    {
                        ParseAllowedNamespaceDeclarations(ref reader, configuration.Naming, errors);
                    }
                    break;

                default:
                    {
                        errors.Add(CreateValidationError($"Unknown configuration setting '{NamingSectionName}.{propertyName}'.", propertySpan));
                        SkipValue(ref reader);
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// Parse allowed namespace declarations
    /// </summary>
    /// <param name="reader">Reader</param>
    /// <param name="namingConfiguration">Naming configuration</param>
    /// <param name="errors">Errors</param>
    private static void ParseAllowedNamespaceDeclarations(ref Utf8JsonReader reader, ConfigurationCategoryNaming namingConfiguration, List<ConfigurationValidationError> errors)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            errors.Add(CreateValidationError($"The '{NamingSectionName}.{AllowedNamespaceDeclarationsPropertyName}' setting must be a JSON array.", GetCurrentValueSpan(ref reader)));

            SkipValue(ref reader);

            return;
        }

        var allowedNamespaceDeclarations = new List<string>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
            {
                break;
            }

            if (reader.TokenType == JsonTokenType.String)
            {
                allowedNamespaceDeclarations.Add(reader.GetString());
            }
            else
            {
                errors.Add(CreateValidationError($"Entries in '{NamingSectionName}.{AllowedNamespaceDeclarationsPropertyName}' must be strings.", GetCurrentValueSpan(ref reader)));

                SkipValue(ref reader);
            }
        }

        namingConfiguration.AllowedNamespaceDeclarations = allowedNamespaceDeclarations;
    }

    /// <summary>
    /// Skip current value
    /// </summary>
    /// <param name="reader">Reader</param>
    private static void SkipValue(ref Utf8JsonReader reader)
    {
        if (reader.TokenType != JsonTokenType.StartArray
            && reader.TokenType != JsonTokenType.StartObject)
        {
            return;
        }

        JsonDocument.ParseValue(ref reader).Dispose();
    }

    /// <summary>
    /// Create validation error
    /// </summary>
    /// <param name="message">Message</param>
    /// <returns>Validation error</returns>
    private static ConfigurationValidationError CreateValidationError(string message)
    {
        return CreateValidationError(message, new TextSpan(0, 0));
    }

    /// <summary>
    /// Create validation error
    /// </summary>
    /// <param name="message">Message</param>
    /// <param name="span">Span</param>
    /// <returns>Validation error</returns>
    private static ConfigurationValidationError CreateValidationError(string message, TextSpan span)
    {
        return new ConfigurationValidationError
               {
                   Message = message,
                   Span = span
               };
    }

    /// <summary>
    /// Gets the current property name span
    /// </summary>
    /// <param name="reader">Reader</param>
    /// <returns>Text span</returns>
    private static TextSpan GetPropertyNameSpan(ref Utf8JsonReader reader)
    {
        var start = (int)reader.TokenStartIndex + 1;
        var length = reader.ValueSpan.Length;

        return new TextSpan(start, length);
    }

    /// <summary>
    /// Gets the current value span
    /// </summary>
    /// <param name="reader">Reader</param>
    /// <returns>Text span</returns>
    private static TextSpan GetCurrentValueSpan(ref Utf8JsonReader reader)
    {
        var start = (int)reader.TokenStartIndex;

        if (reader.TokenType is JsonTokenType.StartArray or JsonTokenType.StartObject)
        {
            var clonedReader = reader;

            JsonDocument.ParseValue(ref clonedReader).Dispose();

            return TextSpan.FromBounds(start, (int)clonedReader.BytesConsumed);
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            var valueStart = start + 1;
            var valueLength = reader.ValueSpan.Length;

            return new TextSpan(valueStart, valueLength);
        }

        var length = reader.ValueSpan.Length;

        if (length == 0)
        {
            length = 1;
        }

        return new TextSpan(start, length);
    }

    /// <summary>
    /// Gets the exception span
    /// </summary>
    /// <param name="text">Text</param>
    /// <param name="exception">Exception</param>
    /// <returns>Span</returns>
    private static TextSpan GetExceptionSpan(SourceText text, JsonException exception)
    {
        var line = exception.LineNumber.GetValueOrDefault();
        var column = exception.BytePositionInLine.GetValueOrDefault();
        var lineIndex = (int)Math.Min(line, text.Lines.Count - 1);
        var lineStart = text.Lines[lineIndex].Start;
        var position = lineStart + (int)column;

        return new TextSpan(Math.Min(position, text.Length), 0);
    }

    #endregion // Methods
}