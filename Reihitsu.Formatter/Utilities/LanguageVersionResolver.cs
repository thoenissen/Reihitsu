using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Reihitsu.Formatter.Utilities;

/// <summary>
/// Resolves the C# language version a formatting run targets. It is the single owner of the supported maximum and of
/// the clamp every entry point applies, so formatter rules only ever see a concrete version
/// </summary>
internal static class LanguageVersionResolver
{
    #region Constants

    /// <summary>
    /// The newest C# language version the formatter supports. Every requested version is clamped to it
    /// </summary>
    public const LanguageVersion MaxSupportedLanguageVersion = LanguageVersion.CSharp14;

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Resolves a requested language version to the concrete version a formatting run uses
    /// </summary>
    /// <param name="languageVersion">The requested language version, which may be symbolic</param>
    /// <returns>A concrete language version no newer than <see cref="MaxSupportedLanguageVersion"/></returns>
    /// <remarks>
    /// <see cref="LanguageVersion.Default"/>, <see cref="LanguageVersion.Latest"/>, and <see cref="LanguageVersion.LatestMajor"/>
    /// mean the newest supported version rather than Roslyn's newest, so a Roslyn upgrade cannot move them past the maximum.
    /// <see cref="LanguageVersion.Preview"/> and every concrete version above the maximum are clamped down to it
    /// </remarks>
    public static LanguageVersion Resolve(LanguageVersion languageVersion)
    {
        if (languageVersion is LanguageVersion.Default or LanguageVersion.Latest or LanguageVersion.LatestMajor)
        {
            return MaxSupportedLanguageVersion;
        }

        var effectiveVersion = languageVersion.MapSpecifiedToEffectiveVersion();

        return effectiveVersion > MaxSupportedLanguageVersion ? MaxSupportedLanguageVersion : effectiveVersion;
    }

    /// <summary>
    /// Resolves the language version carried by parse options to the concrete version a formatting run uses
    /// </summary>
    /// <param name="parseOptions">The parse options, typically a syntax tree's or a project's</param>
    /// <returns>The resolved language version, or <see cref="MaxSupportedLanguageVersion"/> when no C# parse options are available</returns>
    public static LanguageVersion Resolve(ParseOptions parseOptions)
    {
        return parseOptions is CSharpParseOptions csharpParseOptions
                   ? Resolve(csharpParseOptions.LanguageVersion)
                   : MaxSupportedLanguageVersion;
    }

    /// <summary>
    /// Determines whether resolving the requested language version lowers it to <see cref="MaxSupportedLanguageVersion"/>
    /// </summary>
    /// <param name="languageVersion">The requested language version</param>
    /// <returns><see langword="true"/> if the request names a version newer than the supported maximum; otherwise, <see langword="false"/></returns>
    /// <remarks>
    /// The symbolic "newest" values never exceed the maximum because <see cref="Resolve(LanguageVersion)"/> maps them to it
    /// </remarks>
    public static bool ExceedsMaximum(LanguageVersion languageVersion)
    {
        if (languageVersion is LanguageVersion.Default or LanguageVersion.Latest or LanguageVersion.LatestMajor)
        {
            return false;
        }

        return languageVersion.MapSpecifiedToEffectiveVersion() > MaxSupportedLanguageVersion;
    }

    #endregion // Methods
}