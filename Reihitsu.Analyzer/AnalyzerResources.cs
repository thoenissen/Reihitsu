using System.Resources;

namespace Reihitsu.Analyzer;

/// <summary>
/// Provides strongly typed access to localized analyzer diagnostic strings.
/// </summary>
internal static class AnalyzerResources
{
    #region Properties

    /// <summary>
    /// Gets the resource manager used to resolve localized strings.
    /// </summary>
    internal static ResourceManager ResourceManager { get; } = new("Reihitsu.Analyzer.AnalyzerResources", typeof(AnalyzerResources).Assembly);

    /// <summary>
    /// Gets the localized string for RH0001MessageFormat.
    /// </summary>
    internal static string RH0001MessageFormat => GetString(nameof(RH0001MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0001Title.
    /// </summary>
    internal static string RH0001Title => GetString(nameof(RH0001Title));

    /// <summary>
    /// Gets the localized string for RH0002MessageFormat.
    /// </summary>
    internal static string RH0002MessageFormat => GetString(nameof(RH0002MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0002Title.
    /// </summary>
    internal static string RH0002Title => GetString(nameof(RH0002Title));

    /// <summary>
    /// Gets the localized string for RH0003MessageFormat.
    /// </summary>
    internal static string RH0003MessageFormat => GetString(nameof(RH0003MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0003Title.
    /// </summary>
    internal static string RH0003Title => GetString(nameof(RH0003Title));

    /// <summary>
    /// Gets the localized string for RH0004MessageFormat.
    /// </summary>
    internal static string RH0004MessageFormat => GetString(nameof(RH0004MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0004Title.
    /// </summary>
    internal static string RH0004Title => GetString(nameof(RH0004Title));

    /// <summary>
    /// Gets the localized string for RH0005MessageFormat.
    /// </summary>
    internal static string RH0005MessageFormat => GetString(nameof(RH0005MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0005Title.
    /// </summary>
    internal static string RH0005Title => GetString(nameof(RH0005Title));

    /// <summary>
    /// Gets the localized string for RH0006MessageFormat.
    /// </summary>
    internal static string RH0006MessageFormat => GetString(nameof(RH0006MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0006Title.
    /// </summary>
    internal static string RH0006Title => GetString(nameof(RH0006Title));

    /// <summary>
    /// Gets the localized string for RH0007MessageFormat.
    /// </summary>
    internal static string RH0007MessageFormat => GetString(nameof(RH0007MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0007Title.
    /// </summary>
    internal static string RH0007Title => GetString(nameof(RH0007Title));

    /// <summary>
    /// Gets the localized string for RH0008MessageFormat.
    /// </summary>
    internal static string RH0008MessageFormat => GetString(nameof(RH0008MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0008Title.
    /// </summary>
    internal static string RH0008Title => GetString(nameof(RH0008Title));

    /// <summary>
    /// Gets the localized string for RH0009MessageFormat.
    /// </summary>
    internal static string RH0009MessageFormat => GetString(nameof(RH0009MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0009Title.
    /// </summary>
    internal static string RH0009Title => GetString(nameof(RH0009Title));

    /// <summary>
    /// Gets the localized string for RH0010MessageFormat.
    /// </summary>
    internal static string RH0010MessageFormat => GetString(nameof(RH0010MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0010Title.
    /// </summary>
    internal static string RH0010Title => GetString(nameof(RH0010Title));

    /// <summary>
    /// Gets the localized string for RH0011MessageFormat.
    /// </summary>
    internal static string RH0011MessageFormat => GetString(nameof(RH0011MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0011Title.
    /// </summary>
    internal static string RH0011Title => GetString(nameof(RH0011Title));

    /// <summary>
    /// Gets the localized string for RH0012MessageFormat.
    /// </summary>
    internal static string RH0012MessageFormat => GetString(nameof(RH0012MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0012Title.
    /// </summary>
    internal static string RH0012Title => GetString(nameof(RH0012Title));

    /// <summary>
    /// Gets the localized string for RH0013MessageFormat.
    /// </summary>
    internal static string RH0013MessageFormat => GetString(nameof(RH0013MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0013Title.
    /// </summary>
    internal static string RH0013Title => GetString(nameof(RH0013Title));

    /// <summary>
    /// Gets the localized string for RH0101MessageFormat.
    /// </summary>
    internal static string RH0101MessageFormat => GetString(nameof(RH0101MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0101Title.
    /// </summary>
    internal static string RH0101Title => GetString(nameof(RH0101Title));

    /// <summary>
    /// Gets the localized string for RH0102MessageFormat.
    /// </summary>
    internal static string RH0102MessageFormat => GetString(nameof(RH0102MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0102Title.
    /// </summary>
    internal static string RH0102Title => GetString(nameof(RH0102Title));

    /// <summary>
    /// Gets the localized string for RH0103MessageFormat.
    /// </summary>
    internal static string RH0103MessageFormat => GetString(nameof(RH0103MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0103Title.
    /// </summary>
    internal static string RH0103Title => GetString(nameof(RH0103Title));

    /// <summary>
    /// Gets the localized string for RH0104MessageFormat.
    /// </summary>
    internal static string RH0104MessageFormat => GetString(nameof(RH0104MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0104Title.
    /// </summary>
    internal static string RH0104Title => GetString(nameof(RH0104Title));

    /// <summary>
    /// Gets the localized string for RH0105MessageFormat.
    /// </summary>
    internal static string RH0105MessageFormat => GetString(nameof(RH0105MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0105Title.
    /// </summary>
    internal static string RH0105Title => GetString(nameof(RH0105Title));

    /// <summary>
    /// Gets the localized string for RH0106MessageFormat.
    /// </summary>
    internal static string RH0106MessageFormat => GetString(nameof(RH0106MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0106Title.
    /// </summary>
    internal static string RH0106Title => GetString(nameof(RH0106Title));

    /// <summary>
    /// Gets the localized string for RH0107MessageFormat.
    /// </summary>
    internal static string RH0107MessageFormat => GetString(nameof(RH0107MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0107Title.
    /// </summary>
    internal static string RH0107Title => GetString(nameof(RH0107Title));

    /// <summary>
    /// Gets the localized string for RH0108MessageFormat.
    /// </summary>
    internal static string RH0108MessageFormat => GetString(nameof(RH0108MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0108Title.
    /// </summary>
    internal static string RH0108Title => GetString(nameof(RH0108Title));

    /// <summary>
    /// Gets the localized string for RH0109MessageFormat.
    /// </summary>
    internal static string RH0109MessageFormat => GetString(nameof(RH0109MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0109Title.
    /// </summary>
    internal static string RH0109Title => GetString(nameof(RH0109Title));

    /// <summary>
    /// Gets the localized string for RH0110MessageFormat.
    /// </summary>
    internal static string RH0110MessageFormat => GetString(nameof(RH0110MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0110Title.
    /// </summary>
    internal static string RH0110Title => GetString(nameof(RH0110Title));

    /// <summary>
    /// Gets the localized string for RH0111MessageFormat.
    /// </summary>
    internal static string RH0111MessageFormat => GetString(nameof(RH0111MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0111Title.
    /// </summary>
    internal static string RH0111Title => GetString(nameof(RH0111Title));

    /// <summary>
    /// Gets the localized string for RH0112MessageFormat.
    /// </summary>
    internal static string RH0112MessageFormat => GetString(nameof(RH0112MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0112Title.
    /// </summary>
    internal static string RH0112Title => GetString(nameof(RH0112Title));

    /// <summary>
    /// Gets the localized string for RH0201MessageFormat.
    /// </summary>
    internal static string RH0201MessageFormat => GetString(nameof(RH0201MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0201Title.
    /// </summary>
    internal static string RH0201Title => GetString(nameof(RH0201Title));

    /// <summary>
    /// Gets the localized string for RH0202MessageFormat.
    /// </summary>
    internal static string RH0202MessageFormat => GetString(nameof(RH0202MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0202Title.
    /// </summary>
    internal static string RH0202Title => GetString(nameof(RH0202Title));

    /// <summary>
    /// Gets the localized string for RH0203MessageFormat.
    /// </summary>
    internal static string RH0203MessageFormat => GetString(nameof(RH0203MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0203Title.
    /// </summary>
    internal static string RH0203Title => GetString(nameof(RH0203Title));

    /// <summary>
    /// Gets the localized string for RH0204Title.
    /// </summary>
    internal static string RH0204Title => GetString(nameof(RH0204Title));

    /// <summary>
    /// Gets the localized string for RH0204MessageFormat.
    /// </summary>
    internal static string RH0204MessageFormat => GetString(nameof(RH0204MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0205Title.
    /// </summary>
    internal static string RH0205Title => GetString(nameof(RH0205Title));

    /// <summary>
    /// Gets the localized string for RH0205MessageFormat.
    /// </summary>
    internal static string RH0205MessageFormat => GetString(nameof(RH0205MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0206Title.
    /// </summary>
    internal static string RH0206Title => GetString(nameof(RH0206Title));

    /// <summary>
    /// Gets the localized string for RH0206MessageFormat.
    /// </summary>
    internal static string RH0206MessageFormat => GetString(nameof(RH0206MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0207Title.
    /// </summary>
    internal static string RH0207Title => GetString(nameof(RH0207Title));

    /// <summary>
    /// Gets the localized string for RH0207MessageFormat.
    /// </summary>
    internal static string RH0207MessageFormat => GetString(nameof(RH0207MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0208Title.
    /// </summary>
    internal static string RH0208Title => GetString(nameof(RH0208Title));

    /// <summary>
    /// Gets the localized string for RH0208MessageFormat.
    /// </summary>
    internal static string RH0208MessageFormat => GetString(nameof(RH0208MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0209Title.
    /// </summary>
    internal static string RH0209Title => GetString(nameof(RH0209Title));

    /// <summary>
    /// Gets the localized string for RH0209MessageFormat.
    /// </summary>
    internal static string RH0209MessageFormat => GetString(nameof(RH0209MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0210Title.
    /// </summary>
    internal static string RH0210Title => GetString(nameof(RH0210Title));

    /// <summary>
    /// Gets the localized string for RH0210MessageFormat.
    /// </summary>
    internal static string RH0210MessageFormat => GetString(nameof(RH0210MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0211Title.
    /// </summary>
    internal static string RH0211Title => GetString(nameof(RH0211Title));

    /// <summary>
    /// Gets the localized string for RH0211MessageFormat.
    /// </summary>
    internal static string RH0211MessageFormat => GetString(nameof(RH0211MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0212Title.
    /// </summary>
    internal static string RH0212Title => GetString(nameof(RH0212Title));

    /// <summary>
    /// Gets the localized string for RH0212MessageFormat.
    /// </summary>
    internal static string RH0212MessageFormat => GetString(nameof(RH0212MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0213Title.
    /// </summary>
    internal static string RH0213Title => GetString(nameof(RH0213Title));

    /// <summary>
    /// Gets the localized string for RH0213MessageFormat.
    /// </summary>
    internal static string RH0213MessageFormat => GetString(nameof(RH0213MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0214Title.
    /// </summary>
    internal static string RH0214Title => GetString(nameof(RH0214Title));

    /// <summary>
    /// Gets the localized string for RH0214MessageFormat.
    /// </summary>
    internal static string RH0214MessageFormat => GetString(nameof(RH0214MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0215Title.
    /// </summary>
    internal static string RH0215Title => GetString(nameof(RH0215Title));

    /// <summary>
    /// Gets the localized string for RH0215MessageFormat.
    /// </summary>
    internal static string RH0215MessageFormat => GetString(nameof(RH0215MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0216Title.
    /// </summary>
    internal static string RH0216Title => GetString(nameof(RH0216Title));

    /// <summary>
    /// Gets the localized string for RH0216MessageFormat.
    /// </summary>
    internal static string RH0216MessageFormat => GetString(nameof(RH0216MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0217Title.
    /// </summary>
    internal static string RH0217Title => GetString(nameof(RH0217Title));

    /// <summary>
    /// Gets the localized string for RH0217MessageFormat.
    /// </summary>
    internal static string RH0217MessageFormat => GetString(nameof(RH0217MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0218Title.
    /// </summary>
    internal static string RH0218Title => GetString(nameof(RH0218Title));

    /// <summary>
    /// Gets the localized string for RH0218MessageFormat.
    /// </summary>
    internal static string RH0218MessageFormat => GetString(nameof(RH0218MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0219Title.
    /// </summary>
    internal static string RH0219Title => GetString(nameof(RH0219Title));

    /// <summary>
    /// Gets the localized string for RH0219MessageFormat.
    /// </summary>
    internal static string RH0219MessageFormat => GetString(nameof(RH0219MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0220Title.
    /// </summary>
    internal static string RH0220Title => GetString(nameof(RH0220Title));

    /// <summary>
    /// Gets the localized string for RH0220MessageFormat.
    /// </summary>
    internal static string RH0220MessageFormat => GetString(nameof(RH0220MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0221Title.
    /// </summary>
    internal static string RH0221Title => GetString(nameof(RH0221Title));

    /// <summary>
    /// Gets the localized string for RH0221MessageFormat.
    /// </summary>
    internal static string RH0221MessageFormat => GetString(nameof(RH0221MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0222Title.
    /// </summary>
    internal static string RH0222Title => GetString(nameof(RH0222Title));

    /// <summary>
    /// Gets the localized string for RH0222MessageFormat.
    /// </summary>
    internal static string RH0222MessageFormat => GetString(nameof(RH0222MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0223Title.
    /// </summary>
    internal static string RH0223Title => GetString(nameof(RH0223Title));

    /// <summary>
    /// Gets the localized string for RH0223MessageFormat.
    /// </summary>
    internal static string RH0223MessageFormat => GetString(nameof(RH0223MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0224Title.
    /// </summary>
    internal static string RH0224Title => GetString(nameof(RH0224Title));

    /// <summary>
    /// Gets the localized string for RH0224MessageFormat.
    /// </summary>
    internal static string RH0224MessageFormat => GetString(nameof(RH0224MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0225Title.
    /// </summary>
    internal static string RH0225Title => GetString(nameof(RH0225Title));

    /// <summary>
    /// Gets the localized string for RH0225MessageFormat.
    /// </summary>
    internal static string RH0225MessageFormat => GetString(nameof(RH0225MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0226Title.
    /// </summary>
    internal static string RH0226Title => GetString(nameof(RH0226Title));

    /// <summary>
    /// Gets the localized string for RH0226MessageFormat.
    /// </summary>
    internal static string RH0226MessageFormat => GetString(nameof(RH0226MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0227Title.
    /// </summary>
    internal static string RH0227Title => GetString(nameof(RH0227Title));

    /// <summary>
    /// Gets the localized string for RH0227MessageFormat.
    /// </summary>
    internal static string RH0227MessageFormat => GetString(nameof(RH0227MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0228Title.
    /// </summary>
    internal static string RH0228Title => GetString(nameof(RH0228Title));

    /// <summary>
    /// Gets the localized string for RH0228MessageFormat.
    /// </summary>
    internal static string RH0228MessageFormat => GetString(nameof(RH0228MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0301MessageFormat.
    /// </summary>
    internal static string RH0301MessageFormat => GetString(nameof(RH0301MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0301Title.
    /// </summary>
    internal static string RH0301Title => GetString(nameof(RH0301Title));

    /// <summary>
    /// Gets the localized string for RH0302MessageFormat.
    /// </summary>
    internal static string RH0302MessageFormat => GetString(nameof(RH0302MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0302Title.
    /// </summary>
    internal static string RH0302Title => GetString(nameof(RH0302Title));

    /// <summary>
    /// Gets the localized string for RH0303MessageFormat.
    /// </summary>
    internal static string RH0303MessageFormat => GetString(nameof(RH0303MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0303Title.
    /// </summary>
    internal static string RH0303Title => GetString(nameof(RH0303Title));

    /// <summary>
    /// Gets the localized string for RH0304MessageFormat.
    /// </summary>
    internal static string RH0304MessageFormat => GetString(nameof(RH0304MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0304Title.
    /// </summary>
    internal static string RH0304Title => GetString(nameof(RH0304Title));

    /// <summary>
    /// Gets the localized string for RH0305MessageFormat.
    /// </summary>
    internal static string RH0305MessageFormat => GetString(nameof(RH0305MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0305Title.
    /// </summary>
    internal static string RH0305Title => GetString(nameof(RH0305Title));

    /// <summary>
    /// Gets the localized string for RH0306MessageFormat.
    /// </summary>
    internal static string RH0306MessageFormat => GetString(nameof(RH0306MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0306Title.
    /// </summary>
    internal static string RH0306Title => GetString(nameof(RH0306Title));

    /// <summary>
    /// Gets the localized string for RH0307MessageFormat.
    /// </summary>
    internal static string RH0307MessageFormat => GetString(nameof(RH0307MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0307Title.
    /// </summary>
    internal static string RH0307Title => GetString(nameof(RH0307Title));

    /// <summary>
    /// Gets the localized string for RH0308MessageFormat.
    /// </summary>
    internal static string RH0308MessageFormat => GetString(nameof(RH0308MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0308Title.
    /// </summary>
    internal static string RH0308Title => GetString(nameof(RH0308Title));

    /// <summary>
    /// Gets the localized string for RH0309MessageFormat.
    /// </summary>
    internal static string RH0309MessageFormat => GetString(nameof(RH0309MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0309Title.
    /// </summary>
    internal static string RH0309Title => GetString(nameof(RH0309Title));

    /// <summary>
    /// Gets the localized string for RH0310MessageFormat.
    /// </summary>
    internal static string RH0310MessageFormat => GetString(nameof(RH0310MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0310Title.
    /// </summary>
    internal static string RH0310Title => GetString(nameof(RH0310Title));

    /// <summary>
    /// Gets the localized string for RH0311MessageFormat.
    /// </summary>
    internal static string RH0311MessageFormat => GetString(nameof(RH0311MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0311Title.
    /// </summary>
    internal static string RH0311Title => GetString(nameof(RH0311Title));

    /// <summary>
    /// Gets the localized string for RH0312MessageFormat.
    /// </summary>
    internal static string RH0312MessageFormat => GetString(nameof(RH0312MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0312Title.
    /// </summary>
    internal static string RH0312Title => GetString(nameof(RH0312Title));

    /// <summary>
    /// Gets the localized string for RH0313MessageFormat.
    /// </summary>
    internal static string RH0313MessageFormat => GetString(nameof(RH0313MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0313Title.
    /// </summary>
    internal static string RH0313Title => GetString(nameof(RH0313Title));

    /// <summary>
    /// Gets the localized string for RH0314MessageFormat.
    /// </summary>
    internal static string RH0314MessageFormat => GetString(nameof(RH0314MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0314Title.
    /// </summary>
    internal static string RH0314Title => GetString(nameof(RH0314Title));

    /// <summary>
    /// Gets the localized string for RH0315MessageFormat.
    /// </summary>
    internal static string RH0315MessageFormat => GetString(nameof(RH0315MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0315Title.
    /// </summary>
    internal static string RH0315Title => GetString(nameof(RH0315Title));

    /// <summary>
    /// Gets the localized string for RH0316MessageFormat.
    /// </summary>
    internal static string RH0316MessageFormat => GetString(nameof(RH0316MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0316Title.
    /// </summary>
    internal static string RH0316Title => GetString(nameof(RH0316Title));

    /// <summary>
    /// Gets the localized string for RH0317MessageFormat.
    /// </summary>
    internal static string RH0317MessageFormat => GetString(nameof(RH0317MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0317Title.
    /// </summary>
    internal static string RH0317Title => GetString(nameof(RH0317Title));

    /// <summary>
    /// Gets the localized string for RH0318MessageFormat.
    /// </summary>
    internal static string RH0318MessageFormat => GetString(nameof(RH0318MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0318Title.
    /// </summary>
    internal static string RH0318Title => GetString(nameof(RH0318Title));

    /// <summary>
    /// Gets the localized string for RH0319MessageFormat.
    /// </summary>
    internal static string RH0319MessageFormat => GetString(nameof(RH0319MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0319Title.
    /// </summary>
    internal static string RH0319Title => GetString(nameof(RH0319Title));

    /// <summary>
    /// Gets the localized string for RH0320MessageFormat.
    /// </summary>
    internal static string RH0320MessageFormat => GetString(nameof(RH0320MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0320Title.
    /// </summary>
    internal static string RH0320Title => GetString(nameof(RH0320Title));

    /// <summary>
    /// Gets the localized string for RH0321MessageFormat.
    /// </summary>
    internal static string RH0321MessageFormat => GetString(nameof(RH0321MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0321Title.
    /// </summary>
    internal static string RH0321Title => GetString(nameof(RH0321Title));

    /// <summary>
    /// Gets the localized string for RH0324MessageFormat.
    /// </summary>
    internal static string RH0324MessageFormat => GetString(nameof(RH0324MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0324Title.
    /// </summary>
    internal static string RH0324Title => GetString(nameof(RH0324Title));

    /// <summary>
    /// Gets the localized string for RH0322MessageFormat.
    /// </summary>
    internal static string RH0322MessageFormat => GetString(nameof(RH0322MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0322Title.
    /// </summary>
    internal static string RH0322Title => GetString(nameof(RH0322Title));

    /// <summary>
    /// Gets the localized string for RH0325MessageFormat.
    /// </summary>
    internal static string RH0325MessageFormat => GetString(nameof(RH0325MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0325Title.
    /// </summary>
    internal static string RH0325Title => GetString(nameof(RH0325Title));

    /// <summary>
    /// Gets the localized string for RH0326MessageFormat.
    /// </summary>
    internal static string RH0326MessageFormat => GetString(nameof(RH0326MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0326Title.
    /// </summary>
    internal static string RH0326Title => GetString(nameof(RH0326Title));

    /// <summary>
    /// Gets the localized string for RH0327MessageFormat.
    /// </summary>
    internal static string RH0327MessageFormat => GetString(nameof(RH0327MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0327Title.
    /// </summary>
    internal static string RH0327Title => GetString(nameof(RH0327Title));

    /// <summary>
    /// Gets the localized string for RH0328MessageFormat.
    /// </summary>
    internal static string RH0328MessageFormat => GetString(nameof(RH0328MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0328Title.
    /// </summary>
    internal static string RH0328Title => GetString(nameof(RH0328Title));

    /// <summary>
    /// Gets the localized string for RH0329MessageFormat.
    /// </summary>
    internal static string RH0329MessageFormat => GetString(nameof(RH0329MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0329Title.
    /// </summary>
    internal static string RH0329Title => GetString(nameof(RH0329Title));

    /// <summary>
    /// Gets the localized string for RH0330MessageFormat.
    /// </summary>
    internal static string RH0330MessageFormat => GetString(nameof(RH0330MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0330Title.
    /// </summary>
    internal static string RH0330Title => GetString(nameof(RH0330Title));

    /// <summary>
    /// Gets the localized string for RH0331MessageFormat.
    /// </summary>
    internal static string RH0331MessageFormat => GetString(nameof(RH0331MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0331Title.
    /// </summary>
    internal static string RH0331Title => GetString(nameof(RH0331Title));

    /// <summary>
    /// Gets the localized string for RH0332MessageFormat.
    /// </summary>
    internal static string RH0332MessageFormat => GetString(nameof(RH0332MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0332Title.
    /// </summary>
    internal static string RH0332Title => GetString(nameof(RH0332Title));

    /// <summary>
    /// Gets the localized string for RH0333MessageFormat.
    /// </summary>
    internal static string RH0333MessageFormat => GetString(nameof(RH0333MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0333Title.
    /// </summary>
    internal static string RH0333Title => GetString(nameof(RH0333Title));

    /// <summary>
    /// Gets the localized string for RH0358MessageFormat.
    /// </summary>
    internal static string RH0358MessageFormat => GetString(nameof(RH0358MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0358Title.
    /// </summary>
    internal static string RH0358Title => GetString(nameof(RH0358Title));

    /// <summary>
    /// Gets the localized string for RH0371MessageFormat.
    /// </summary>
    internal static string RH0371MessageFormat => GetString(nameof(RH0371MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0371Title.
    /// </summary>
    internal static string RH0371Title => GetString(nameof(RH0371Title));

    /// <summary>
    /// Gets the localized string for RH0372MessageFormat.
    /// </summary>
    internal static string RH0372MessageFormat => GetString(nameof(RH0372MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0372Title.
    /// </summary>
    internal static string RH0372Title => GetString(nameof(RH0372Title));

    /// <summary>
    /// Gets the localized string for RH0375MessageFormat.
    /// </summary>
    internal static string RH0375MessageFormat => GetString(nameof(RH0375MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0375Title.
    /// </summary>
    internal static string RH0375Title => GetString(nameof(RH0375Title));

    /// <summary>
    /// Gets the localized string for RH0382MessageFormat.
    /// </summary>
    internal static string RH0382MessageFormat => GetString(nameof(RH0382MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0382Title.
    /// </summary>
    internal static string RH0382Title => GetString(nameof(RH0382Title));

    /// <summary>
    /// Gets the localized string for RH0383MessageFormat.
    /// </summary>
    internal static string RH0383MessageFormat => GetString(nameof(RH0383MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0383Title.
    /// </summary>
    internal static string RH0383Title => GetString(nameof(RH0383Title));

    /// <summary>
    /// Gets the localized string for RH0384MessageFormat.
    /// </summary>
    internal static string RH0384MessageFormat => GetString(nameof(RH0384MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0384Title.
    /// </summary>
    internal static string RH0384Title => GetString(nameof(RH0384Title));

    /// <summary>
    /// Gets the localized string for RH0334MessageFormat.
    /// </summary>
    internal static string RH0334MessageFormat => GetString(nameof(RH0334MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0334Title.
    /// </summary>
    internal static string RH0334Title => GetString(nameof(RH0334Title));

    /// <summary>
    /// Gets the localized string for RH0335MessageFormat.
    /// </summary>
    internal static string RH0335MessageFormat => GetString(nameof(RH0335MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0335Title.
    /// </summary>
    internal static string RH0335Title => GetString(nameof(RH0335Title));

    /// <summary>
    /// Gets the localized string for RH0336MessageFormat.
    /// </summary>
    internal static string RH0336MessageFormat => GetString(nameof(RH0336MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0336Title.
    /// </summary>
    internal static string RH0336Title => GetString(nameof(RH0336Title));

    /// <summary>
    /// Gets the localized string for RH0337MessageFormat.
    /// </summary>
    internal static string RH0337MessageFormat => GetString(nameof(RH0337MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0337Title.
    /// </summary>
    internal static string RH0337Title => GetString(nameof(RH0337Title));

    /// <summary>
    /// Gets the localized string for RH0338MessageFormat.
    /// </summary>
    internal static string RH0338MessageFormat => GetString(nameof(RH0338MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0338Title.
    /// </summary>
    internal static string RH0338Title => GetString(nameof(RH0338Title));

    /// <summary>
    /// Gets the localized string for RH0339MessageFormat.
    /// </summary>
    internal static string RH0339MessageFormat => GetString(nameof(RH0339MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0339Title.
    /// </summary>
    internal static string RH0339Title => GetString(nameof(RH0339Title));

    /// <summary>
    /// Gets the localized string for RH0340MessageFormat.
    /// </summary>
    internal static string RH0340MessageFormat => GetString(nameof(RH0340MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0340Title.
    /// </summary>
    internal static string RH0340Title => GetString(nameof(RH0340Title));

    /// <summary>
    /// Gets the localized string for RH0341MessageFormat.
    /// </summary>
    internal static string RH0341MessageFormat => GetString(nameof(RH0341MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0341Title.
    /// </summary>
    internal static string RH0341Title => GetString(nameof(RH0341Title));

    /// <summary>
    /// Gets the localized string for RH0342MessageFormat.
    /// </summary>
    internal static string RH0342MessageFormat => GetString(nameof(RH0342MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0342Title.
    /// </summary>
    internal static string RH0342Title => GetString(nameof(RH0342Title));

    /// <summary>
    /// Gets the localized string for RH0343MessageFormat.
    /// </summary>
    internal static string RH0343MessageFormat => GetString(nameof(RH0343MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0343Title.
    /// </summary>
    internal static string RH0343Title => GetString(nameof(RH0343Title));

    /// <summary>
    /// Gets the localized string for RH0344MessageFormat.
    /// </summary>
    internal static string RH0344MessageFormat => GetString(nameof(RH0344MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0344Title.
    /// </summary>
    internal static string RH0344Title => GetString(nameof(RH0344Title));

    /// <summary>
    /// Gets the localized string for RH0345MessageFormat.
    /// </summary>
    internal static string RH0345MessageFormat => GetString(nameof(RH0345MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0345Title.
    /// </summary>
    internal static string RH0345Title => GetString(nameof(RH0345Title));

    /// <summary>
    /// Gets the localized string for RH0346MessageFormat.
    /// </summary>
    internal static string RH0346MessageFormat => GetString(nameof(RH0346MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0346Title.
    /// </summary>
    internal static string RH0346Title => GetString(nameof(RH0346Title));

    /// <summary>
    /// Gets the localized string for RH0347MessageFormat.
    /// </summary>
    internal static string RH0347MessageFormat => GetString(nameof(RH0347MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0347Title.
    /// </summary>
    internal static string RH0347Title => GetString(nameof(RH0347Title));

    /// <summary>
    /// Gets the localized string for RH0348MessageFormat.
    /// </summary>
    internal static string RH0348MessageFormat => GetString(nameof(RH0348MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0348Title.
    /// </summary>
    internal static string RH0348Title => GetString(nameof(RH0348Title));

    /// <summary>
    /// Gets the localized string for RH0349MessageFormat.
    /// </summary>
    internal static string RH0349MessageFormat => GetString(nameof(RH0349MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0349Title.
    /// </summary>
    internal static string RH0349Title => GetString(nameof(RH0349Title));

    /// <summary>
    /// Gets the localized string for RH0350MessageFormat.
    /// </summary>
    internal static string RH0350MessageFormat => GetString(nameof(RH0350MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0350Title.
    /// </summary>
    internal static string RH0350Title => GetString(nameof(RH0350Title));

    /// <summary>
    /// Gets the localized string for RH0351MessageFormat.
    /// </summary>
    internal static string RH0351MessageFormat => GetString(nameof(RH0351MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0351Title.
    /// </summary>
    internal static string RH0351Title => GetString(nameof(RH0351Title));

    /// <summary>
    /// Gets the localized string for RH0352MessageFormat.
    /// </summary>
    internal static string RH0352MessageFormat => GetString(nameof(RH0352MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0352Title.
    /// </summary>
    internal static string RH0352Title => GetString(nameof(RH0352Title));

    /// <summary>
    /// Gets the localized string for RH0353MessageFormat.
    /// </summary>
    internal static string RH0353MessageFormat => GetString(nameof(RH0353MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0353Title.
    /// </summary>
    internal static string RH0353Title => GetString(nameof(RH0353Title));

    /// <summary>
    /// Gets the localized string for RH0354MessageFormat.
    /// </summary>
    internal static string RH0354MessageFormat => GetString(nameof(RH0354MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0354Title.
    /// </summary>
    internal static string RH0354Title => GetString(nameof(RH0354Title));

    /// <summary>
    /// Gets the localized string for RH0355MessageFormat.
    /// </summary>
    internal static string RH0355MessageFormat => GetString(nameof(RH0355MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0355Title.
    /// </summary>
    internal static string RH0355Title => GetString(nameof(RH0355Title));

    /// <summary>
    /// Gets the localized string for RH0356MessageFormat.
    /// </summary>
    internal static string RH0356MessageFormat => GetString(nameof(RH0356MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0356Title.
    /// </summary>
    internal static string RH0356Title => GetString(nameof(RH0356Title));

    /// <summary>
    /// Gets the localized string for RH0357MessageFormat.
    /// </summary>
    internal static string RH0357MessageFormat => GetString(nameof(RH0357MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0357Title.
    /// </summary>
    internal static string RH0357Title => GetString(nameof(RH0357Title));

    /// <summary>
    /// Gets the localized string for RH0359MessageFormat.
    /// </summary>
    internal static string RH0359MessageFormat => GetString(nameof(RH0359MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0359Title.
    /// </summary>
    internal static string RH0359Title => GetString(nameof(RH0359Title));

    /// <summary>
    /// Gets the localized string for RH0360MessageFormat.
    /// </summary>
    internal static string RH0360MessageFormat => GetString(nameof(RH0360MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0360Title.
    /// </summary>
    internal static string RH0360Title => GetString(nameof(RH0360Title));

    /// <summary>
    /// Gets the localized string for RH0361MessageFormat.
    /// </summary>
    internal static string RH0361MessageFormat => GetString(nameof(RH0361MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0361Title.
    /// </summary>
    internal static string RH0361Title => GetString(nameof(RH0361Title));

    /// <summary>
    /// Gets the localized string for RH0362MessageFormat.
    /// </summary>
    internal static string RH0362MessageFormat => GetString(nameof(RH0362MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0362Title.
    /// </summary>
    internal static string RH0362Title => GetString(nameof(RH0362Title));

    /// <summary>
    /// Gets the localized string for RH0363MessageFormat.
    /// </summary>
    internal static string RH0363MessageFormat => GetString(nameof(RH0363MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0363Title.
    /// </summary>
    internal static string RH0363Title => GetString(nameof(RH0363Title));

    /// <summary>
    /// Gets the localized string for RH0364MessageFormat.
    /// </summary>
    internal static string RH0364MessageFormat => GetString(nameof(RH0364MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0364Title.
    /// </summary>
    internal static string RH0364Title => GetString(nameof(RH0364Title));

    /// <summary>
    /// Gets the localized string for RH0365MessageFormat.
    /// </summary>
    internal static string RH0365MessageFormat => GetString(nameof(RH0365MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0365Title.
    /// </summary>
    internal static string RH0365Title => GetString(nameof(RH0365Title));

    /// <summary>
    /// Gets the localized string for RH0366MessageFormat.
    /// </summary>
    internal static string RH0366MessageFormat => GetString(nameof(RH0366MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0366Title.
    /// </summary>
    internal static string RH0366Title => GetString(nameof(RH0366Title));

    /// <summary>
    /// Gets the localized string for RH0367MessageFormat.
    /// </summary>
    internal static string RH0367MessageFormat => GetString(nameof(RH0367MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0367Title.
    /// </summary>
    internal static string RH0367Title => GetString(nameof(RH0367Title));

    /// <summary>
    /// Gets the localized string for RH0368MessageFormat.
    /// </summary>
    internal static string RH0368MessageFormat => GetString(nameof(RH0368MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0368Title.
    /// </summary>
    internal static string RH0368Title => GetString(nameof(RH0368Title));

    /// <summary>
    /// Gets the localized string for RH0369MessageFormat.
    /// </summary>
    internal static string RH0369MessageFormat => GetString(nameof(RH0369MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0369Title.
    /// </summary>
    internal static string RH0369Title => GetString(nameof(RH0369Title));

    /// <summary>
    /// Gets the localized string for RH0370MessageFormat.
    /// </summary>
    internal static string RH0370MessageFormat => GetString(nameof(RH0370MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0370Title.
    /// </summary>
    internal static string RH0370Title => GetString(nameof(RH0370Title));

    /// <summary>
    /// Gets the localized string for RH0373MessageFormat.
    /// </summary>
    internal static string RH0373MessageFormat => GetString(nameof(RH0373MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0373Title.
    /// </summary>
    internal static string RH0373Title => GetString(nameof(RH0373Title));

    /// <summary>
    /// Gets the localized string for RH0374MessageFormat.
    /// </summary>
    internal static string RH0374MessageFormat => GetString(nameof(RH0374MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0374Title.
    /// </summary>
    internal static string RH0374Title => GetString(nameof(RH0374Title));

    /// <summary>
    /// Gets the localized string for RH0376MessageFormat.
    /// </summary>
    internal static string RH0376MessageFormat => GetString(nameof(RH0376MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0376Title.
    /// </summary>
    internal static string RH0376Title => GetString(nameof(RH0376Title));

    /// <summary>
    /// Gets the localized string for RH0377MessageFormat.
    /// </summary>
    internal static string RH0377MessageFormat => GetString(nameof(RH0377MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0377Title.
    /// </summary>
    internal static string RH0377Title => GetString(nameof(RH0377Title));

    /// <summary>
    /// Gets the localized string for RH0378MessageFormat.
    /// </summary>
    internal static string RH0378MessageFormat => GetString(nameof(RH0378MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0378Title.
    /// </summary>
    internal static string RH0378Title => GetString(nameof(RH0378Title));

    /// <summary>
    /// Gets the localized string for RH0379MessageFormat.
    /// </summary>
    internal static string RH0379MessageFormat => GetString(nameof(RH0379MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0379Title.
    /// </summary>
    internal static string RH0379Title => GetString(nameof(RH0379Title));

    /// <summary>
    /// Gets the localized string for RH0380MessageFormat.
    /// </summary>
    internal static string RH0380MessageFormat => GetString(nameof(RH0380MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0380Title.
    /// </summary>
    internal static string RH0380Title => GetString(nameof(RH0380Title));

    /// <summary>
    /// Gets the localized string for RH0381MessageFormat.
    /// </summary>
    internal static string RH0381MessageFormat => GetString(nameof(RH0381MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0381Title.
    /// </summary>
    internal static string RH0381Title => GetString(nameof(RH0381Title));

    /// <summary>
    /// Gets the localized string for RH0601Title.
    /// </summary>
    internal static string RH0601Title => GetString(nameof(RH0601Title));

    /// <summary>
    /// Gets the localized string for RH0601MessageFormat.
    /// </summary>
    internal static string RH0601MessageFormat => GetString(nameof(RH0601MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0602Title.
    /// </summary>
    internal static string RH0602Title => GetString(nameof(RH0602Title));

    /// <summary>
    /// Gets the localized string for RH0602MessageFormat.
    /// </summary>
    internal static string RH0602MessageFormat => GetString(nameof(RH0602MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0603Title.
    /// </summary>
    internal static string RH0603Title => GetString(nameof(RH0603Title));

    /// <summary>
    /// Gets the localized string for RH0603MessageFormat.
    /// </summary>
    internal static string RH0603MessageFormat => GetString(nameof(RH0603MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0604Title.
    /// </summary>
    internal static string RH0604Title => GetString(nameof(RH0604Title));

    /// <summary>
    /// Gets the localized string for RH0604MessageFormat.
    /// </summary>
    internal static string RH0604MessageFormat => GetString(nameof(RH0604MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0605Title.
    /// </summary>
    internal static string RH0605Title => GetString(nameof(RH0605Title));

    /// <summary>
    /// Gets the localized string for RH0605MessageFormat.
    /// </summary>
    internal static string RH0605MessageFormat => GetString(nameof(RH0605MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0606Title.
    /// </summary>
    internal static string RH0606Title => GetString(nameof(RH0606Title));

    /// <summary>
    /// Gets the localized string for RH0606MessageFormat.
    /// </summary>
    internal static string RH0606MessageFormat => GetString(nameof(RH0606MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0607Title.
    /// </summary>
    internal static string RH0607Title => GetString(nameof(RH0607Title));

    /// <summary>
    /// Gets the localized string for RH0607MessageFormat.
    /// </summary>
    internal static string RH0607MessageFormat => GetString(nameof(RH0607MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0608Title.
    /// </summary>
    internal static string RH0608Title => GetString(nameof(RH0608Title));

    /// <summary>
    /// Gets the localized string for RH0608MessageFormat.
    /// </summary>
    internal static string RH0608MessageFormat => GetString(nameof(RH0608MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0609Title.
    /// </summary>
    internal static string RH0609Title => GetString(nameof(RH0609Title));

    /// <summary>
    /// Gets the localized string for RH0609MessageFormat.
    /// </summary>
    internal static string RH0609MessageFormat => GetString(nameof(RH0609MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0610Title.
    /// </summary>
    internal static string RH0610Title => GetString(nameof(RH0610Title));

    /// <summary>
    /// Gets the localized string for RH0610MessageFormat.
    /// </summary>
    internal static string RH0610MessageFormat => GetString(nameof(RH0610MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0611Title.
    /// </summary>
    internal static string RH0611Title => GetString(nameof(RH0611Title));

    /// <summary>
    /// Gets the localized string for RH0611MessageFormat.
    /// </summary>
    internal static string RH0611MessageFormat => GetString(nameof(RH0611MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0612Title.
    /// </summary>
    internal static string RH0612Title => GetString(nameof(RH0612Title));

    /// <summary>
    /// Gets the localized string for RH0612MessageFormat.
    /// </summary>
    internal static string RH0612MessageFormat => GetString(nameof(RH0612MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0613Title.
    /// </summary>
    internal static string RH0613Title => GetString(nameof(RH0613Title));

    /// <summary>
    /// Gets the localized string for RH0613MessageFormat.
    /// </summary>
    internal static string RH0613MessageFormat => GetString(nameof(RH0613MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0614Title.
    /// </summary>
    internal static string RH0614Title => GetString(nameof(RH0614Title));

    /// <summary>
    /// Gets the localized string for RH0614MessageFormat.
    /// </summary>
    internal static string RH0614MessageFormat => GetString(nameof(RH0614MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0402MessageFormat.
    /// </summary>
    internal static string RH0402MessageFormat => GetString(nameof(RH0402MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0402Title.
    /// </summary>
    internal static string RH0402Title => GetString(nameof(RH0402Title));

    /// <summary>
    /// Gets the localized string for RH0403MessageFormat.
    /// </summary>
    internal static string RH0403MessageFormat => GetString(nameof(RH0403MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0403Title.
    /// </summary>
    internal static string RH0403Title => GetString(nameof(RH0403Title));

    /// <summary>
    /// Gets the localized string for RH0404MessageFormat.
    /// </summary>
    internal static string RH0404MessageFormat => GetString(nameof(RH0404MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0404Title.
    /// </summary>
    internal static string RH0404Title => GetString(nameof(RH0404Title));

    /// <summary>
    /// Gets the localized string for RH0405MessageFormat.
    /// </summary>
    internal static string RH0405MessageFormat => GetString(nameof(RH0405MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0405Title.
    /// </summary>
    internal static string RH0405Title => GetString(nameof(RH0405Title));

    /// <summary>
    /// Gets the localized string for RH0406MessageFormat.
    /// </summary>
    internal static string RH0406MessageFormat => GetString(nameof(RH0406MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0406Title.
    /// </summary>
    internal static string RH0406Title => GetString(nameof(RH0406Title));

    /// <summary>
    /// Gets the localized string for RH0407MessageFormat.
    /// </summary>
    internal static string RH0407MessageFormat => GetString(nameof(RH0407MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0407Title.
    /// </summary>
    internal static string RH0407Title => GetString(nameof(RH0407Title));

    /// <summary>
    /// Gets the localized string for RH0408MessageFormat.
    /// </summary>
    internal static string RH0408MessageFormat => GetString(nameof(RH0408MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0408Title.
    /// </summary>
    internal static string RH0408Title => GetString(nameof(RH0408Title));

    /// <summary>
    /// Gets the localized string for RH0409MessageFormat.
    /// </summary>
    internal static string RH0409MessageFormat => GetString(nameof(RH0409MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0409Title.
    /// </summary>
    internal static string RH0409Title => GetString(nameof(RH0409Title));

    /// <summary>
    /// Gets the localized string for RH0410MessageFormat.
    /// </summary>
    internal static string RH0410MessageFormat => GetString(nameof(RH0410MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0410Title.
    /// </summary>
    internal static string RH0410Title => GetString(nameof(RH0410Title));

    /// <summary>
    /// Gets the localized string for RH0411MessageFormat.
    /// </summary>
    internal static string RH0411MessageFormat => GetString(nameof(RH0411MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0411Title.
    /// </summary>
    internal static string RH0411Title => GetString(nameof(RH0411Title));

    /// <summary>
    /// Gets the localized string for RH0412MessageFormat.
    /// </summary>
    internal static string RH0412MessageFormat => GetString(nameof(RH0412MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0412Title.
    /// </summary>
    internal static string RH0412Title => GetString(nameof(RH0412Title));

    /// <summary>
    /// Gets the localized string for RH0413MessageFormat.
    /// </summary>
    internal static string RH0413MessageFormat => GetString(nameof(RH0413MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0413Title.
    /// </summary>
    internal static string RH0413Title => GetString(nameof(RH0413Title));

    /// <summary>
    /// Gets the localized string for RH0414MessageFormat.
    /// </summary>
    internal static string RH0414MessageFormat => GetString(nameof(RH0414MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0414Title.
    /// </summary>
    internal static string RH0414Title => GetString(nameof(RH0414Title));

    /// <summary>
    /// Gets the localized string for RH0415MessageFormat.
    /// </summary>
    internal static string RH0415MessageFormat => GetString(nameof(RH0415MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0415Title.
    /// </summary>
    internal static string RH0415Title => GetString(nameof(RH0415Title));

    /// <summary>
    /// Gets the localized string for RH0416MessageFormat.
    /// </summary>
    internal static string RH0416MessageFormat => GetString(nameof(RH0416MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0416Title.
    /// </summary>
    internal static string RH0416Title => GetString(nameof(RH0416Title));

    /// <summary>
    /// Gets the localized string for RH0417MessageFormat.
    /// </summary>
    internal static string RH0417MessageFormat => GetString(nameof(RH0417MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0417Title.
    /// </summary>
    internal static string RH0417Title => GetString(nameof(RH0417Title));

    /// <summary>
    /// Gets the localized string for RH0418MessageFormat.
    /// </summary>
    internal static string RH0418MessageFormat => GetString(nameof(RH0418MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0418Title.
    /// </summary>
    internal static string RH0418Title => GetString(nameof(RH0418Title));

    /// <summary>
    /// Gets the localized string for RH0419MessageFormat.
    /// </summary>
    internal static string RH0419MessageFormat => GetString(nameof(RH0419MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0419Title.
    /// </summary>
    internal static string RH0419Title => GetString(nameof(RH0419Title));

    /// <summary>
    /// Gets the localized string for RH0420MessageFormat.
    /// </summary>
    internal static string RH0420MessageFormat => GetString(nameof(RH0420MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0420Title.
    /// </summary>
    internal static string RH0420Title => GetString(nameof(RH0420Title));

    /// <summary>
    /// Gets the localized string for RH0421MessageFormat.
    /// </summary>
    internal static string RH0421MessageFormat => GetString(nameof(RH0421MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0421Title.
    /// </summary>
    internal static string RH0421Title => GetString(nameof(RH0421Title));

    /// <summary>
    /// Gets the localized string for RH0422MessageFormat.
    /// </summary>
    internal static string RH0422MessageFormat => GetString(nameof(RH0422MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0422Title.
    /// </summary>
    internal static string RH0422Title => GetString(nameof(RH0422Title));

    /// <summary>
    /// Gets the localized string for RH0423MessageFormat.
    /// </summary>
    internal static string RH0423MessageFormat => GetString(nameof(RH0423MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0423Title.
    /// </summary>
    internal static string RH0423Title => GetString(nameof(RH0423Title));

    /// <summary>
    /// Gets the localized string for RH0424MessageFormat.
    /// </summary>
    internal static string RH0424MessageFormat => GetString(nameof(RH0424MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0424Title.
    /// </summary>
    internal static string RH0424Title => GetString(nameof(RH0424Title));

    /// <summary>
    /// Gets the localized string for RH0425MessageFormat.
    /// </summary>
    internal static string RH0425MessageFormat => GetString(nameof(RH0425MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0425Title.
    /// </summary>
    internal static string RH0425Title => GetString(nameof(RH0425Title));

    /// <summary>
    /// Gets the localized string for RH0426MessageFormat.
    /// </summary>
    internal static string RH0426MessageFormat => GetString(nameof(RH0426MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0426Title.
    /// </summary>
    internal static string RH0426Title => GetString(nameof(RH0426Title));

    /// <summary>
    /// Gets the localized string for RH0427MessageFormat.
    /// </summary>
    internal static string RH0427MessageFormat => GetString(nameof(RH0427MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0427Title.
    /// </summary>
    internal static string RH0427Title => GetString(nameof(RH0427Title));

    /// <summary>
    /// Gets the localized string for RH0428MessageFormat.
    /// </summary>
    internal static string RH0428MessageFormat => GetString(nameof(RH0428MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0428Title.
    /// </summary>
    internal static string RH0428Title => GetString(nameof(RH0428Title));

    /// <summary>
    /// Gets the localized string for RH0429MessageFormat.
    /// </summary>
    internal static string RH0429MessageFormat => GetString(nameof(RH0429MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0429Title.
    /// </summary>
    internal static string RH0429Title => GetString(nameof(RH0429Title));

    /// <summary>
    /// Gets the localized string for RH0430MessageFormat.
    /// </summary>
    internal static string RH0430MessageFormat => GetString(nameof(RH0430MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0430Title.
    /// </summary>
    internal static string RH0430Title => GetString(nameof(RH0430Title));

    /// <summary>
    /// Gets the localized string for RH0431MessageFormat.
    /// </summary>
    internal static string RH0431MessageFormat => GetString(nameof(RH0431MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0431Title.
    /// </summary>
    internal static string RH0431Title => GetString(nameof(RH0431Title));

    /// <summary>
    /// Gets the localized string for RH0432MessageFormat.
    /// </summary>
    internal static string RH0432MessageFormat => GetString(nameof(RH0432MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0432Title.
    /// </summary>
    internal static string RH0432Title => GetString(nameof(RH0432Title));

    /// <summary>
    /// Gets the localized string for RH0433MessageFormat.
    /// </summary>
    internal static string RH0433MessageFormat => GetString(nameof(RH0433MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0433Title.
    /// </summary>
    internal static string RH0433Title => GetString(nameof(RH0433Title));

    /// <summary>
    /// Gets the localized string for RH0434MessageFormat.
    /// </summary>
    internal static string RH0434MessageFormat => GetString(nameof(RH0434MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0434Title.
    /// </summary>
    internal static string RH0434Title => GetString(nameof(RH0434Title));

    /// <summary>
    /// Gets the localized string for RH0435MessageFormat.
    /// </summary>
    internal static string RH0435MessageFormat => GetString(nameof(RH0435MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0435Title.
    /// </summary>
    internal static string RH0435Title => GetString(nameof(RH0435Title));

    /// <summary>
    /// Gets the localized string for RH0436MessageFormat.
    /// </summary>
    internal static string RH0436MessageFormat => GetString(nameof(RH0436MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0436Title.
    /// </summary>
    internal static string RH0436Title => GetString(nameof(RH0436Title));

    /// <summary>
    /// Gets the localized string for RH0437MessageFormat.
    /// </summary>
    internal static string RH0437MessageFormat => GetString(nameof(RH0437MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0437Title.
    /// </summary>
    internal static string RH0437Title => GetString(nameof(RH0437Title));

    /// <summary>
    /// Gets the localized string for RH0438MessageFormat.
    /// </summary>
    internal static string RH0438MessageFormat => GetString(nameof(RH0438MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0438Title.
    /// </summary>
    internal static string RH0438Title => GetString(nameof(RH0438Title));

    /// <summary>
    /// Gets the localized string for RH0439MessageFormat.
    /// </summary>
    internal static string RH0439MessageFormat => GetString(nameof(RH0439MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0439Title.
    /// </summary>
    internal static string RH0439Title => GetString(nameof(RH0439Title));

    /// <summary>
    /// Gets the localized string for RH0440MessageFormat.
    /// </summary>
    internal static string RH0440MessageFormat => GetString(nameof(RH0440MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0440Title.
    /// </summary>
    internal static string RH0440Title => GetString(nameof(RH0440Title));

    /// <summary>
    /// Gets the localized string for RH0441MessageFormat.
    /// </summary>
    internal static string RH0441MessageFormat => GetString(nameof(RH0441MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0441Title.
    /// </summary>
    internal static string RH0441Title => GetString(nameof(RH0441Title));

    /// <summary>
    /// Gets the localized string for RH0442MessageFormat.
    /// </summary>
    internal static string RH0442MessageFormat => GetString(nameof(RH0442MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0442Title.
    /// </summary>
    internal static string RH0442Title => GetString(nameof(RH0442Title));

    /// <summary>
    /// Gets the localized string for RH0443MessageFormat.
    /// </summary>
    internal static string RH0443MessageFormat => GetString(nameof(RH0443MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0443Title.
    /// </summary>
    internal static string RH0443Title => GetString(nameof(RH0443Title));

    /// <summary>
    /// Gets the localized string for RH0444MessageFormat.
    /// </summary>
    internal static string RH0444MessageFormat => GetString(nameof(RH0444MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0444Title.
    /// </summary>
    internal static string RH0444Title => GetString(nameof(RH0444Title));

    /// <summary>
    /// Gets the localized string for RH0445MessageFormat.
    /// </summary>
    internal static string RH0445MessageFormat => GetString(nameof(RH0445MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0445Title.
    /// </summary>
    internal static string RH0445Title => GetString(nameof(RH0445Title));

    /// <summary>
    /// Gets the localized string for RH0446MessageFormat.
    /// </summary>
    internal static string RH0446MessageFormat => GetString(nameof(RH0446MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0446Title.
    /// </summary>
    internal static string RH0446Title => GetString(nameof(RH0446Title));

    /// <summary>
    /// Gets the localized string for RH0447MessageFormat.
    /// </summary>
    internal static string RH0447MessageFormat => GetString(nameof(RH0447MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0447Title.
    /// </summary>
    internal static string RH0447Title => GetString(nameof(RH0447Title));

    /// <summary>
    /// Gets the localized string for RH0448MessageFormat.
    /// </summary>
    internal static string RH0448MessageFormat => GetString(nameof(RH0448MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0448Title.
    /// </summary>
    internal static string RH0448Title => GetString(nameof(RH0448Title));

    /// <summary>
    /// Gets the localized string for RH0401MessageFormat.
    /// </summary>
    internal static string RH0401MessageFormat => GetString(nameof(RH0401MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0401Title.
    /// </summary>
    internal static string RH0401Title => GetString(nameof(RH0401Title));

    /// <summary>
    /// Gets the localized string for RH0501MessageFormat.
    /// </summary>
    internal static string RH0501MessageFormat => GetString(nameof(RH0501MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0501Title.
    /// </summary>
    internal static string RH0501Title => GetString(nameof(RH0501Title));

    /// <summary>
    /// Gets the localized string for RH0502MessageFormat.
    /// </summary>
    internal static string RH0502MessageFormat => GetString(nameof(RH0502MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0502Title.
    /// </summary>
    internal static string RH0502Title => GetString(nameof(RH0502Title));

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Resolves a localized string by resource key.
    /// </summary>
    /// <param name="name">The resource key to resolve.</param>
    /// <returns>The localized string for the requested key.</returns>
    private static string GetString(string name)
    {
        return ResourceManager.GetString(name);
    }

    #endregion // Methods
}