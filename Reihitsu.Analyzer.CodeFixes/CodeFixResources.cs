using System.Resources;

namespace Reihitsu.Analyzer;

/// <summary>
/// Provides strongly typed access to localized code-fix strings.
/// </summary>
internal static class CodeFixResources
{
    #region Fields

    /// <summary>
    /// Gets the resource manager used to resolve localized strings.
    /// </summary>
    private static readonly ResourceManager _resourceManager = new("Reihitsu.Analyzer.CodeFixResources", typeof(CodeFixResources).Assembly);

    #endregion // Fields

    #region Properties

    /// <summary>
    /// Gets the localized string for RH0001Title.
    /// </summary>
    internal static string RH0001Title => GetString(nameof(RH0001Title));

    /// <summary>
    /// Gets the localized string for RH0002Title.
    /// </summary>
    internal static string RH0002Title => GetString(nameof(RH0002Title));

    /// <summary>
    /// Gets the localized string for RH0003Title.
    /// </summary>
    internal static string RH0003Title => GetString(nameof(RH0003Title));

    /// <summary>
    /// Gets the localized string for RH0004Title.
    /// </summary>
    internal static string RH0004Title => GetString(nameof(RH0004Title));

    /// <summary>
    /// Gets the localized string for RH0005Title.
    /// </summary>
    internal static string RH0005Title => GetString(nameof(RH0005Title));

    /// <summary>
    /// Gets the localized string for RH0006Title.
    /// </summary>
    internal static string RH0006Title => GetString(nameof(RH0006Title));

    /// <summary>
    /// Gets the localized string for RH0007Title.
    /// </summary>
    internal static string RH0007Title => GetString(nameof(RH0007Title));

    /// <summary>
    /// Gets the localized string for RH0008Title.
    /// </summary>
    internal static string RH0008Title => GetString(nameof(RH0008Title));

    /// <summary>
    /// Gets the localized string for RH0009Title.
    /// </summary>
    internal static string RH0009Title => GetString(nameof(RH0009Title));

    /// <summary>
    /// Gets the localized string for RH0010Title.
    /// </summary>
    internal static string RH0010Title => GetString(nameof(RH0010Title));

    /// <summary>
    /// Gets the localized string for RH0011Title.
    /// </summary>
    internal static string RH0011Title => GetString(nameof(RH0011Title));

    /// <summary>
    /// Gets the localized string for RH0012Title.
    /// </summary>
    internal static string RH0012Title => GetString(nameof(RH0012Title));

    /// <summary>
    /// Gets the localized string for RH0013Title.
    /// </summary>
    internal static string RH0013Title => GetString(nameof(RH0013Title));

    /// <summary>
    /// Gets the localized string for RH0101Title.
    /// </summary>
    internal static string RH0101Title => GetString(nameof(RH0101Title));

    /// <summary>
    /// Gets the localized string for RH0104Title.
    /// </summary>
    internal static string RH0104Title => GetString(nameof(RH0104Title));

    /// <summary>
    /// Gets the localized string for RH0105Title.
    /// </summary>
    internal static string RH0105Title => GetString(nameof(RH0105Title));

    /// <summary>
    /// Gets the localized string for RH0110Title.
    /// </summary>
    internal static string RH0110Title => GetString(nameof(RH0110Title));

    /// <summary>
    /// Gets the localized string for RH0111Title.
    /// </summary>
    internal static string RH0111Title => GetString(nameof(RH0111Title));

    /// <summary>
    /// Gets the localized string for RH0201Title.
    /// </summary>
    internal static string RH0201Title => GetString(nameof(RH0201Title));

    /// <summary>
    /// Gets the localized string for RH0202Title.
    /// </summary>
    internal static string RH0202Title => GetString(nameof(RH0202Title));

    /// <summary>
    /// Gets the localized string for RH0203Title.
    /// </summary>
    internal static string RH0203Title => GetString(nameof(RH0203Title));

    /// <summary>
    /// Gets the localized string for RH0204Title.
    /// </summary>
    internal static string RH0204Title => GetString(nameof(RH0204Title));

    /// <summary>
    /// Gets the localized string for RH0205Title.
    /// </summary>
    internal static string RH0205Title => GetString(nameof(RH0205Title));

    /// <summary>
    /// Gets the localized string for RH0206Title.
    /// </summary>
    internal static string RH0206Title => GetString(nameof(RH0206Title));

    /// <summary>
    /// Gets the localized string for RH0207Title.
    /// </summary>
    internal static string RH0207Title => GetString(nameof(RH0207Title));

    /// <summary>
    /// Gets the localized string for RH0208Title.
    /// </summary>
    internal static string RH0208Title => GetString(nameof(RH0208Title));

    /// <summary>
    /// Gets the localized string for RH0209Title.
    /// </summary>
    internal static string RH0209Title => GetString(nameof(RH0209Title));

    /// <summary>
    /// Gets the localized string for RH0210Title.
    /// </summary>
    internal static string RH0210Title => GetString(nameof(RH0210Title));

    /// <summary>
    /// Gets the localized string for RH0211Title.
    /// </summary>
    internal static string RH0211Title => GetString(nameof(RH0211Title));

    /// <summary>
    /// Gets the localized string for RH0212Title.
    /// </summary>
    internal static string RH0212Title => GetString(nameof(RH0212Title));

    /// <summary>
    /// Gets the localized string for RH0213Title.
    /// </summary>
    internal static string RH0213Title => GetString(nameof(RH0213Title));

    /// <summary>
    /// Gets the localized string for RH0214Title.
    /// </summary>
    internal static string RH0214Title => GetString(nameof(RH0214Title));

    /// <summary>
    /// Gets the localized string for RH0215Title.
    /// </summary>
    internal static string RH0215Title => GetString(nameof(RH0215Title));

    /// <summary>
    /// Gets the localized string for RH0216Title.
    /// </summary>
    internal static string RH0216Title => GetString(nameof(RH0216Title));

    /// <summary>
    /// Gets the localized string for RH0217Title.
    /// </summary>
    internal static string RH0217Title => GetString(nameof(RH0217Title));

    /// <summary>
    /// Gets the localized string for RH0218Title.
    /// </summary>
    internal static string RH0218Title => GetString(nameof(RH0218Title));

    /// <summary>
    /// Gets the localized string for RH0219Title.
    /// </summary>
    internal static string RH0219Title => GetString(nameof(RH0219Title));

    /// <summary>
    /// Gets the localized string for RH0220Title.
    /// </summary>
    internal static string RH0220Title => GetString(nameof(RH0220Title));

    /// <summary>
    /// Gets the localized string for RH0221Title.
    /// </summary>
    internal static string RH0221Title => GetString(nameof(RH0221Title));

    /// <summary>
    /// Gets the localized string for RH0222Title.
    /// </summary>
    internal static string RH0222Title => GetString(nameof(RH0222Title));

    /// <summary>
    /// Gets the localized string for RH0223Title.
    /// </summary>
    internal static string RH0223Title => GetString(nameof(RH0223Title));

    /// <summary>
    /// Gets the localized string for RH0224Title.
    /// </summary>
    internal static string RH0224Title => GetString(nameof(RH0224Title));

    /// <summary>
    /// Gets the localized string for RH0228Title.
    /// </summary>
    internal static string RH0228Title => GetString(nameof(RH0228Title));

    /// <summary>
    /// Gets the localized string for RH0301Title.
    /// </summary>
    internal static string RH0301Title => GetString(nameof(RH0301Title));

    /// <summary>
    /// Gets the localized string for RH0303Title.
    /// </summary>
    internal static string RH0303Title => GetString(nameof(RH0303Title));

    /// <summary>
    /// Gets the localized string for RH0304Title.
    /// </summary>
    internal static string RH0304Title => GetString(nameof(RH0304Title));

    /// <summary>
    /// Gets the localized string for RH0305Title.
    /// </summary>
    internal static string RH0305Title => GetString(nameof(RH0305Title));

    /// <summary>
    /// Gets the localized string for RH0306Title.
    /// </summary>
    internal static string RH0306Title => GetString(nameof(RH0306Title));

    /// <summary>
    /// Gets the localized string for RH0307Title.
    /// </summary>
    internal static string RH0307Title => GetString(nameof(RH0307Title));

    /// <summary>
    /// Gets the localized string for RH0308Title.
    /// </summary>
    internal static string RH0308Title => GetString(nameof(RH0308Title));

    /// <summary>
    /// Gets the localized string for RH0309Title.
    /// </summary>
    internal static string RH0309Title => GetString(nameof(RH0309Title));

    /// <summary>
    /// Gets the localized string for RH0310Title.
    /// </summary>
    internal static string RH0310Title => GetString(nameof(RH0310Title));

    /// <summary>
    /// Gets the localized string for RH0311Title.
    /// </summary>
    internal static string RH0311Title => GetString(nameof(RH0311Title));

    /// <summary>
    /// Gets the localized string for RH0312Title.
    /// </summary>
    internal static string RH0312Title => GetString(nameof(RH0312Title));

    /// <summary>
    /// Gets the localized string for RH0314Title.
    /// </summary>
    internal static string RH0314Title => GetString(nameof(RH0314Title));

    /// <summary>
    /// Gets the localized string for RH0315Title.
    /// </summary>
    internal static string RH0315Title => GetString(nameof(RH0315Title));

    /// <summary>
    /// Gets the localized string for RH0316Title.
    /// </summary>
    internal static string RH0316Title => GetString(nameof(RH0316Title));

    /// <summary>
    /// Gets the localized string for RH0317Title.
    /// </summary>
    internal static string RH0317Title => GetString(nameof(RH0317Title));

    /// <summary>
    /// Gets the localized string for RH0318Title.
    /// </summary>
    internal static string RH0318Title => GetString(nameof(RH0318Title));

    /// <summary>
    /// Gets the localized string for RH0319Title.
    /// </summary>
    internal static string RH0319Title => GetString(nameof(RH0319Title));

    /// <summary>
    /// Gets the localized string for RH0320Title.
    /// </summary>
    internal static string RH0320Title => GetString(nameof(RH0320Title));

    /// <summary>
    /// Gets the localized string for RH0321Title.
    /// </summary>
    internal static string RH0321Title => GetString(nameof(RH0321Title));

    /// <summary>
    /// Gets the localized string for RH0302Title.
    /// </summary>
    internal static string RH0302Title => GetString(nameof(RH0302Title));

    /// <summary>
    /// Gets the localized string for RH0324Title.
    /// </summary>
    internal static string RH0324Title => GetString(nameof(RH0324Title));

    /// <summary>
    /// Gets the localized string for RH0325Title.
    /// </summary>
    internal static string RH0325Title => GetString(nameof(RH0325Title));

    /// <summary>
    /// Gets the localized string for RH0329Title.
    /// </summary>
    internal static string RH0329Title => GetString(nameof(RH0329Title));

    /// <summary>
    /// Gets the localized string for RH0330Title.
    /// </summary>
    internal static string RH0330Title => GetString(nameof(RH0330Title));

    /// <summary>
    /// Gets the localized string for RH0331Title.
    /// </summary>
    internal static string RH0331Title => GetString(nameof(RH0331Title));

    /// <summary>
    /// Gets the localized string for RH0332Title.
    /// </summary>
    internal static string RH0332Title => GetString(nameof(RH0332Title));

    /// <summary>
    /// Gets the localized string for RH0333Title.
    /// </summary>
    internal static string RH0333Title => GetString(nameof(RH0333Title));

    /// <summary>
    /// Gets the localized string for RH0334Title.
    /// </summary>
    internal static string RH0334Title => GetString(nameof(RH0334Title));

    /// <summary>
    /// Gets the localized string for RH0601Title.
    /// </summary>
    internal static string RH0601Title => GetString(nameof(RH0601Title));

    /// <summary>
    /// Gets the localized string for RH0602Title.
    /// </summary>
    internal static string RH0602Title => GetString(nameof(RH0602Title));

    /// <summary>
    /// Gets the localized string for RH0603Title.
    /// </summary>
    internal static string RH0603Title => GetString(nameof(RH0603Title));

    /// <summary>
    /// Gets the localized string for RH0604Title.
    /// </summary>
    internal static string RH0604Title => GetString(nameof(RH0604Title));

    /// <summary>
    /// Gets the localized string for RH0605Title.
    /// </summary>
    internal static string RH0605Title => GetString(nameof(RH0605Title));

    /// <summary>
    /// Gets the localized string for RH0606Title.
    /// </summary>
    internal static string RH0606Title => GetString(nameof(RH0606Title));

    /// <summary>
    /// Gets the localized string for RH0607Title.
    /// </summary>
    internal static string RH0607Title => GetString(nameof(RH0607Title));

    /// <summary>
    /// Gets the localized string for RH0608Title.
    /// </summary>
    internal static string RH0608Title => GetString(nameof(RH0608Title));

    /// <summary>
    /// Gets the localized string for RH0609Title.
    /// </summary>
    internal static string RH0609Title => GetString(nameof(RH0609Title));

    /// <summary>
    /// Gets the localized string for RH0610Title.
    /// </summary>
    internal static string RH0610Title => GetString(nameof(RH0610Title));

    /// <summary>
    /// Gets the localized string for RH0611Title.
    /// </summary>
    internal static string RH0611Title => GetString(nameof(RH0611Title));

    /// <summary>
    /// Gets the localized string for RH0612Title.
    /// </summary>
    internal static string RH0612Title => GetString(nameof(RH0612Title));

    /// <summary>
    /// Gets the localized string for RH0613Title.
    /// </summary>
    internal static string RH0613Title => GetString(nameof(RH0613Title));

    /// <summary>
    /// Gets the localized string for RH0614Title.
    /// </summary>
    internal static string RH0614Title => GetString(nameof(RH0614Title));

    /// <summary>
    /// Gets the localized string for RH0439Title.
    /// </summary>
    internal static string RH0439Title => GetString(nameof(RH0439Title));

    /// <summary>
    /// Gets the localized string for RH0444Title.
    /// </summary>
    internal static string RH0444Title => GetString(nameof(RH0444Title));

    /// <summary>
    /// Gets the localized string for RH0446Title.
    /// </summary>
    internal static string RH0446Title => GetString(nameof(RH0446Title));

    /// <summary>
    /// Gets the localized string for RH0401Title.
    /// </summary>
    internal static string RH0401Title => GetString(nameof(RH0401Title));

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Resolves a localized string by resource key.
    /// </summary>
    /// <param name="name">The resource key to resolve.</param>
    /// <returns>The localized string for the requested key.</returns>
    private static string GetString(string name)
    {
        return _resourceManager.GetString(name);
    }

    #endregion // Methods
}