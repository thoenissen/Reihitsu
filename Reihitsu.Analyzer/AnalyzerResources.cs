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
    /// Gets the localized string for RH0334MessageFormat.
    /// </summary>
    internal static string RH0334MessageFormat => GetString(nameof(RH0334MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0334Title.
    /// </summary>
    internal static string RH0334Title => GetString(nameof(RH0334Title));

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