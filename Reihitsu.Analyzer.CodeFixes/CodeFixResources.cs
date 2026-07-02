using System;
using System.Resources;

namespace Reihitsu.Analyzer.CodeFixes;

/// <summary>
/// Provides strongly typed access to localized code-fix strings
/// </summary>
internal static class CodeFixResources
{
    #region Fields

    /// <summary>
    /// Gets the resource manager used to resolve localized strings
    /// </summary>
    private static readonly ResourceManager _resourceManagerInstance = new("Reihitsu.Analyzer.CodeFixes.CodeFixResources", typeof(CodeFixResources).Assembly);

    #endregion // Fields

    #region Properties

    /// <summary>
    /// Gets the localized string for RH3001Title
    /// </summary>
    internal static string RH3001Title => GetString(nameof(RH3001Title));

    /// <summary>
    /// Gets the localized string for RH3101Title
    /// </summary>
    internal static string RH3101Title => GetString(nameof(RH3101Title));

    /// <summary>
    /// Gets the localized string for RH3102Title
    /// </summary>
    internal static string RH3102Title => GetString(nameof(RH3102Title));

    /// <summary>
    /// Gets the localized string for RH3002Title
    /// </summary>
    internal static string RH3002Title => GetString(nameof(RH3002Title));

    /// <summary>
    /// Gets the localized string for RH3201Title
    /// </summary>
    internal static string RH3201Title => GetString(nameof(RH3201Title));

    /// <summary>
    /// Gets the localized string for RH3003Title
    /// </summary>
    internal static string RH3003Title => GetString(nameof(RH3003Title));

    /// <summary>
    /// Gets the localized string for RH3103Title
    /// </summary>
    internal static string RH3103Title => GetString(nameof(RH3103Title));

    /// <summary>
    /// Gets the localized string for RH3104Title
    /// </summary>
    internal static string RH3104Title => GetString(nameof(RH3104Title));

    /// <summary>
    /// Gets the localized string for RH3004Title
    /// </summary>
    internal static string RH3004Title => GetString(nameof(RH3004Title));

    /// <summary>
    /// Gets the localized string for RH3005Title
    /// </summary>
    internal static string RH3005Title => GetString(nameof(RH3005Title));

    /// <summary>
    /// Gets the localized string for RH3006Title
    /// </summary>
    internal static string RH3006Title => GetString(nameof(RH3006Title));

    /// <summary>
    /// Gets the localized string for RH3105Title
    /// </summary>
    internal static string RH3105Title => GetString(nameof(RH3105Title));

    /// <summary>
    /// Gets the localized string for RH2001Title
    /// </summary>
    internal static string RH2001Title => GetString(nameof(RH2001Title));

    /// <summary>
    /// Gets the localized string for RH2004Title
    /// </summary>
    internal static string RH2004Title => GetString(nameof(RH2004Title));

    /// <summary>
    /// Gets the localized string for RH2005Title
    /// </summary>
    internal static string RH2005Title => GetString(nameof(RH2005Title));

    /// <summary>
    /// Gets the localized string for RH3106Title
    /// </summary>
    internal static string RH3106Title => GetString(nameof(RH3106Title));

    /// <summary>
    /// Gets the localized string for RH3107Title
    /// </summary>
    internal static string RH3107Title => GetString(nameof(RH3107Title));

    /// <summary>
    /// Gets the localized string for RH4001Title
    /// </summary>
    internal static string RH4001Title => GetString(nameof(RH4001Title));

    /// <summary>
    /// Gets the localized string for RH4002Title
    /// </summary>
    internal static string RH4002Title => GetString(nameof(RH4002Title));

    /// <summary>
    /// Gets the localized string for RH4003Title
    /// </summary>
    internal static string RH4003Title => GetString(nameof(RH4003Title));

    /// <summary>
    /// Gets the localized string for RH4004Title
    /// </summary>
    internal static string RH4004Title => GetString(nameof(RH4004Title));

    /// <summary>
    /// Gets the localized string for RH4101Title
    /// </summary>
    internal static string RH4101Title => GetString(nameof(RH4101Title));

    /// <summary>
    /// Gets the localized string for RH4005Title
    /// </summary>
    internal static string RH4005Title => GetString(nameof(RH4005Title));

    /// <summary>
    /// Gets the localized string for RH4102Title
    /// </summary>
    internal static string RH4102Title => GetString(nameof(RH4102Title));

    /// <summary>
    /// Gets the localized string for RH4006Title
    /// </summary>
    internal static string RH4006Title => GetString(nameof(RH4006Title));

    /// <summary>
    /// Gets the localized string for RH4103Title
    /// </summary>
    internal static string RH4103Title => GetString(nameof(RH4103Title));

    /// <summary>
    /// Gets the localized string for RH4104Title
    /// </summary>
    internal static string RH4104Title => GetString(nameof(RH4104Title));

    /// <summary>
    /// Gets the localized string for RH4105Title
    /// </summary>
    internal static string RH4105Title => GetString(nameof(RH4105Title));

    /// <summary>
    /// Gets the localized string for RH4106Title
    /// </summary>
    internal static string RH4106Title => GetString(nameof(RH4106Title));

    /// <summary>
    /// Gets the localized string for RH4107Title
    /// </summary>
    internal static string RH4107Title => GetString(nameof(RH4107Title));

    /// <summary>
    /// Gets the localized string for RH4108Title
    /// </summary>
    internal static string RH4108Title => GetString(nameof(RH4108Title));

    /// <summary>
    /// Gets the localized string for RH4109Title
    /// </summary>
    internal static string RH4109Title => GetString(nameof(RH4109Title));

    /// <summary>
    /// Gets the localized string for RH4110Title
    /// </summary>
    internal static string RH4110Title => GetString(nameof(RH4110Title));

    /// <summary>
    /// Gets the localized string for RH4111Title
    /// </summary>
    internal static string RH4111Title => GetString(nameof(RH4111Title));

    /// <summary>
    /// Gets the localized string for RH4112Title
    /// </summary>
    internal static string RH4112Title => GetString(nameof(RH4112Title));

    /// <summary>
    /// Gets the localized string for RH4113Title
    /// </summary>
    internal static string RH4113Title => GetString(nameof(RH4113Title));

    /// <summary>
    /// Gets the localized string for RH4114Title
    /// </summary>
    internal static string RH4114Title => GetString(nameof(RH4114Title));

    /// <summary>
    /// Gets the localized string for RH4115Title
    /// </summary>
    internal static string RH4115Title => GetString(nameof(RH4115Title));

    /// <summary>
    /// Gets the localized string for RH4116Title
    /// </summary>
    internal static string RH4116Title => GetString(nameof(RH4116Title));

    /// <summary>
    /// Gets the localized string for RH4117Title
    /// </summary>
    internal static string RH4117Title => GetString(nameof(RH4117Title));

    /// <summary>
    /// Gets the localized string for RH4118Title
    /// </summary>
    internal static string RH4118Title => GetString(nameof(RH4118Title));

    /// <summary>
    /// Gets the localized string for RH4010Title
    /// </summary>
    internal static string RH4010Title => GetString(nameof(RH4010Title));

    /// <summary>
    /// Gets the localized string for RH4011Title
    /// </summary>
    internal static string RH4011Title => GetString(nameof(RH4011Title));

    /// <summary>
    /// Gets the localized string for RH4120Title
    /// </summary>
    internal static string RH4120Title => GetString(nameof(RH4120Title));

    /// <summary>
    /// Gets the localized string for RH7301Title
    /// </summary>
    internal static string RH7301Title => GetString(nameof(RH7301Title));

    /// <summary>
    /// Gets the localized string for RH5001Title
    /// </summary>
    internal static string RH5001Title => GetString(nameof(RH5001Title));

    /// <summary>
    /// Gets the localized string for RH5002Title
    /// </summary>
    internal static string RH5002Title => GetString(nameof(RH5002Title));

    /// <summary>
    /// Gets the localized string for RH5003Title
    /// </summary>
    internal static string RH5003Title => GetString(nameof(RH5003Title));

    /// <summary>
    /// Gets the localized string for RH5004Title
    /// </summary>
    internal static string RH5004Title => GetString(nameof(RH5004Title));

    /// <summary>
    /// Gets the localized string for RH5005Title
    /// </summary>
    internal static string RH5005Title => GetString(nameof(RH5005Title));

    /// <summary>
    /// Gets the localized string for RH5006Title
    /// </summary>
    internal static string RH5006Title => GetString(nameof(RH5006Title));

    /// <summary>
    /// Gets the localized string for RH5007Title
    /// </summary>
    internal static string RH5007Title => GetString(nameof(RH5007Title));

    /// <summary>
    /// Gets the localized string for RH5008Title
    /// </summary>
    internal static string RH5008Title => GetString(nameof(RH5008Title));

    /// <summary>
    /// Gets the localized string for RH5009Title
    /// </summary>
    internal static string RH5009Title => GetString(nameof(RH5009Title));

    /// <summary>
    /// Gets the localized string for RH5010Title
    /// </summary>
    internal static string RH5010Title => GetString(nameof(RH5010Title));

    /// <summary>
    /// Gets the localized string for RH5011Title
    /// </summary>
    internal static string RH5011Title => GetString(nameof(RH5011Title));

    /// <summary>
    /// Gets the localized string for RH5012Title
    /// </summary>
    internal static string RH5012Title => GetString(nameof(RH5012Title));

    /// <summary>
    /// Gets the localized string for RH5013Title
    /// </summary>
    internal static string RH5013Title => GetString(nameof(RH5013Title));

    /// <summary>
    /// Gets the localized string for RH5014Title
    /// </summary>
    internal static string RH5014Title => GetString(nameof(RH5014Title));

    /// <summary>
    /// Gets the localized string for RH5015Title
    /// </summary>
    internal static string RH5015Title => GetString(nameof(RH5015Title));

    /// <summary>
    /// Gets the localized string for RH5016Title
    /// </summary>
    internal static string RH5016Title => GetString(nameof(RH5016Title));

    /// <summary>
    /// Gets the localized string for RH5017Title
    /// </summary>
    internal static string RH5017Title => GetString(nameof(RH5017Title));

    /// <summary>
    /// Gets the localized string for RH5018Title
    /// </summary>
    internal static string RH5018Title => GetString(nameof(RH5018Title));

    /// <summary>
    /// Gets the localized string for RH5019Title
    /// </summary>
    internal static string RH5019Title => GetString(nameof(RH5019Title));

    /// <summary>
    /// Gets the localized string for RH5301Title
    /// </summary>
    internal static string RH5301Title => GetString(nameof(RH5301Title));

    /// <summary>
    /// Gets the localized string for RH5020Title
    /// </summary>
    internal static string RH5020Title => GetString(nameof(RH5020Title));

    /// <summary>
    /// Gets the localized string for RH5021Title
    /// </summary>
    internal static string RH5021Title => GetString(nameof(RH5021Title));

    /// <summary>
    /// Gets the localized string for RH5201Title
    /// </summary>
    internal static string RH5201Title => GetString(nameof(RH5201Title));

    /// <summary>
    /// Gets the localized string for RH5112Title
    /// </summary>
    internal static string RH5112Title => GetString(nameof(RH5112Title));

    /// <summary>
    /// Gets the localized string for RH5303Title
    /// </summary>
    internal static string RH5303Title => GetString(nameof(RH5303Title));

    /// <summary>
    /// Gets the localized string for RH5206Title
    /// </summary>
    internal static string RH5206Title => GetString(nameof(RH5206Title));

    /// <summary>
    /// Gets the localized string for RH5304Title
    /// </summary>
    internal static string RH5304Title => GetString(nameof(RH5304Title));

    /// <summary>
    /// Gets the localized string for RH5305Title
    /// </summary>
    internal static string RH5305Title => GetString(nameof(RH5305Title));

    /// <summary>
    /// Gets the localized string for RH5306Title
    /// </summary>
    internal static string RH5306Title => GetString(nameof(RH5306Title));

    /// <summary>
    /// Gets the localized string for RH5307Title
    /// </summary>
    internal static string RH5307Title => GetString(nameof(RH5307Title));

    /// <summary>
    /// Gets the localized string for RH3202Title
    /// </summary>
    internal static string RH3202Title => GetString(nameof(RH3202Title));

    /// <summary>
    /// Gets the localized string for RH3203Title
    /// </summary>
    internal static string RH3203Title => GetString(nameof(RH3203Title));

    /// <summary>
    /// Gets the localized string for RH5401Title
    /// </summary>
    internal static string RH5401Title => GetString(nameof(RH5401Title));

    /// <summary>
    /// Gets the localized string for RH5408Title
    /// </summary>
    internal static string RH5408Title => GetString(nameof(RH5408Title));

    /// <summary>
    /// Gets the localized string for RH5409Title
    /// </summary>
    internal static string RH5409Title => GetString(nameof(RH5409Title));

    /// <summary>
    /// Gets the localized string for RH5410Title
    /// </summary>
    internal static string RH5410Title => GetString(nameof(RH5410Title));

    /// <summary>
    /// Gets the localized string for RH5411Title
    /// </summary>
    internal static string RH5411Title => GetString(nameof(RH5411Title));

    /// <summary>
    /// Gets the localized string for RH5412Title
    /// </summary>
    internal static string RH5412Title => GetString(nameof(RH5412Title));

    /// <summary>
    /// Gets the localized string for RH5413Title
    /// </summary>
    internal static string RH5413Title => GetString(nameof(RH5413Title));

    /// <summary>
    /// Gets the localized string for RH5414Title
    /// </summary>
    internal static string RH5414Title => GetString(nameof(RH5414Title));

    /// <summary>
    /// Gets the localized string for RH5415Title
    /// </summary>
    internal static string RH5415Title => GetString(nameof(RH5415Title));

    /// <summary>
    /// Gets the localized string for RH5416Title
    /// </summary>
    internal static string RH5416Title => GetString(nameof(RH5416Title));

    /// <summary>
    /// Gets the localized string for RH3204Title
    /// </summary>
    internal static string RH3204Title => GetString(nameof(RH3204Title));

    /// <summary>
    /// Gets the localized string for RH7302Title
    /// </summary>
    internal static string RH7302Title => GetString(nameof(RH7302Title));

    /// <summary>
    /// Gets the localized string for RH5302Title
    /// </summary>
    internal static string RH5302Title => GetString(nameof(RH5302Title));

    /// <summary>
    /// Gets the localized string for RH5202Title
    /// </summary>
    internal static string RH5202Title => GetString(nameof(RH5202Title));

    /// <summary>
    /// Gets the localized string for RH5101Title
    /// </summary>
    internal static string RH5101Title => GetString(nameof(RH5101Title));

    /// <summary>
    /// Gets the localized string for RH5102Title
    /// </summary>
    internal static string RH5102Title => GetString(nameof(RH5102Title));

    /// <summary>
    /// Gets the localized string for RH5203Title
    /// </summary>
    internal static string RH5203Title => GetString(nameof(RH5203Title));

    /// <summary>
    /// Gets the localized string for RH5602Title
    /// </summary>
    internal static string RH5602Title => GetString(nameof(RH5602Title));

    /// <summary>
    /// Gets the localized string for RH5028Title
    /// </summary>
    internal static string RH5028Title => GetString(nameof(RH5028Title));

    /// <summary>
    /// Gets the localized string for RH5603Title
    /// </summary>
    internal static string RH5603Title => GetString(nameof(RH5603Title));

    /// <summary>
    /// Gets the localized string for RH5103Title
    /// </summary>
    internal static string RH5103Title => GetString(nameof(RH5103Title));

    /// <summary>
    /// Gets the localized string for RH7101Title
    /// </summary>
    internal static string RH7101Title => GetString(nameof(RH7101Title));

    /// <summary>
    /// Gets the localized string for RH7303Title
    /// </summary>
    internal static string RH7303Title => GetString(nameof(RH7303Title));

    /// <summary>
    /// Gets the localized string for RH5110Title
    /// </summary>
    internal static string RH5110Title => GetString(nameof(RH5110Title));

    /// <summary>
    /// Gets the localized string for RH5604Title
    /// </summary>
    internal static string RH5604Title => GetString(nameof(RH5604Title));

    /// <summary>
    /// Gets the localized string for RH7304Title
    /// </summary>
    internal static string RH7304Title => GetString(nameof(RH7304Title));

    /// <summary>
    /// Gets the localized string for RH7306Title
    /// </summary>
    internal static string RH7306Title => GetString(nameof(RH7306Title));

    /// <summary>
    /// Gets the localized string for RH7309Title
    /// </summary>
    internal static string RH7309Title => GetString(nameof(RH7309Title));

    /// <summary>
    /// Gets the localized string for RH5204Title
    /// </summary>
    internal static string RH5204Title => GetString(nameof(RH5204Title));

    /// <summary>
    /// Gets the localized string for RH5111Title
    /// </summary>
    internal static string RH5111Title => GetString(nameof(RH5111Title));

    /// <summary>
    /// Gets the localized string for RH5205Title
    /// </summary>
    internal static string RH5205Title => GetString(nameof(RH5205Title));

    /// <summary>
    /// Gets the localized string for RH5029Title
    /// </summary>
    internal static string RH5029Title => GetString(nameof(RH5029Title));

    /// <summary>
    /// Gets the localized string for RH5030Title
    /// </summary>
    internal static string RH5030Title => GetString(nameof(RH5030Title));

    /// <summary>
    /// Gets the localized string for RH5031Title
    /// </summary>
    internal static string RH5031Title => GetString(nameof(RH5031Title));

    /// <summary>
    /// Gets the localized string for RH5032Title
    /// </summary>
    internal static string RH5032Title => GetString(nameof(RH5032Title));

    /// <summary>
    /// Gets the localized string for RH7501Title
    /// </summary>
    internal static string RH7501Title => GetString(nameof(RH7501Title));

    /// <summary>
    /// Gets the localized string for RH7004Title
    /// </summary>
    internal static string RH7004Title => GetString(nameof(RH7004Title));

    /// <summary>
    /// Gets the localized string for RH6001Title
    /// </summary>
    internal static string RH6001Title => GetString(nameof(RH6001Title));

    /// <summary>
    /// Gets the localized string for RH6002Title
    /// </summary>
    internal static string RH6002Title => GetString(nameof(RH6002Title));

    /// <summary>
    /// Gets the localized string for RH6003Title
    /// </summary>
    internal static string RH6003Title => GetString(nameof(RH6003Title));

    /// <summary>
    /// Gets the localized string for RH8301Title
    /// </summary>
    internal static string RH8301Title => GetString(nameof(RH8301Title));

    /// <summary>
    /// Gets the localized string for RH6004Title
    /// </summary>
    internal static string RH6004Title => GetString(nameof(RH6004Title));

    /// <summary>
    /// Gets the localized string for RH6005Title
    /// </summary>
    internal static string RH6005Title => GetString(nameof(RH6005Title));

    /// <summary>
    /// Gets the localized string for RH6006Title
    /// </summary>
    internal static string RH6006Title => GetString(nameof(RH6006Title));

    /// <summary>
    /// Gets the localized string for RH6007Title
    /// </summary>
    internal static string RH6007Title => GetString(nameof(RH6007Title));

    /// <summary>
    /// Gets the localized string for RH6008Title
    /// </summary>
    internal static string RH6008Title => GetString(nameof(RH6008Title));

    /// <summary>
    /// Gets the localized string for RH6009Title
    /// </summary>
    internal static string RH6009Title => GetString(nameof(RH6009Title));

    /// <summary>
    /// Gets the localized string for RH6010Title
    /// </summary>
    internal static string RH6010Title => GetString(nameof(RH6010Title));

    /// <summary>
    /// Gets the localized string for RH6011Title
    /// </summary>
    internal static string RH6011Title => GetString(nameof(RH6011Title));

    /// <summary>
    /// Gets the localized string for RH6012Title
    /// </summary>
    internal static string RH6012Title => GetString(nameof(RH6012Title));

    /// <summary>
    /// Gets the localized string for RH6013Title
    /// </summary>
    internal static string RH6013Title => GetString(nameof(RH6013Title));

    /// <summary>
    /// Gets the localized string for RH6014Title
    /// </summary>
    internal static string RH6014Title => GetString(nameof(RH6014Title));

    /// <summary>
    /// Gets the localized string for RH6015Title
    /// </summary>
    internal static string RH6015Title => GetString(nameof(RH6015Title));

    /// <summary>
    /// Gets the localized string for RH6016Title
    /// </summary>
    internal static string RH6016Title => GetString(nameof(RH6016Title));

    /// <summary>
    /// Gets the localized string for RH6017Title
    /// </summary>
    internal static string RH6017Title => GetString(nameof(RH6017Title));

    /// <summary>
    /// Gets the localized string for RH6018Title
    /// </summary>
    internal static string RH6018Title => GetString(nameof(RH6018Title));

    /// <summary>
    /// Gets the localized string for RH6019Title
    /// </summary>
    internal static string RH6019Title => GetString(nameof(RH6019Title));

    /// <summary>
    /// Gets the localized string for RH6020Title
    /// </summary>
    internal static string RH6020Title => GetString(nameof(RH6020Title));

    /// <summary>
    /// Gets the localized string for RH6021Title
    /// </summary>
    internal static string RH6021Title => GetString(nameof(RH6021Title));

    /// <summary>
    /// Gets the localized string for RH6022Title
    /// </summary>
    internal static string RH6022Title => GetString(nameof(RH6022Title));

    /// <summary>
    /// Gets the localized string for RH5601Title
    /// </summary>
    internal static string RH5601Title => GetString(nameof(RH5601Title));

    /// <summary>
    /// Gets the localized string for RH5402Title
    /// </summary>
    internal static string RH5402Title => GetString(nameof(RH5402Title));

    /// <summary>
    /// Gets the localized string for RH5403Title
    /// </summary>
    internal static string RH5403Title => GetString(nameof(RH5403Title));

    /// <summary>
    /// Gets the localized string for RH5404Title
    /// </summary>
    internal static string RH5404Title => GetString(nameof(RH5404Title));

    /// <summary>
    /// Gets the localized string for RH5405Title
    /// </summary>
    internal static string RH5405Title => GetString(nameof(RH5405Title));

    /// <summary>
    /// Gets the localized string for RH5022Title
    /// </summary>
    internal static string RH5022Title => GetString(nameof(RH5022Title));

    /// <summary>
    /// Gets the localized string for RH8302Title
    /// </summary>
    internal static string RH8302Title => GetString(nameof(RH8302Title));

    /// <summary>
    /// Gets the localized string for RH5023Title
    /// </summary>
    internal static string RH5023Title => GetString(nameof(RH5023Title));

    /// <summary>
    /// Gets the localized string for RH5024Title
    /// </summary>
    internal static string RH5024Title => GetString(nameof(RH5024Title));

    /// <summary>
    /// Gets the localized string for RH5025Title
    /// </summary>
    internal static string RH5025Title => GetString(nameof(RH5025Title));

    /// <summary>
    /// Gets the localized string for RH5026Title
    /// </summary>
    internal static string RH5026Title => GetString(nameof(RH5026Title));

    /// <summary>
    /// Gets the localized string for RH5027Title
    /// </summary>
    internal static string RH5027Title => GetString(nameof(RH5027Title));

    /// <summary>
    /// Gets the localized string for RH8303Title
    /// </summary>
    internal static string RH8303Title => GetString(nameof(RH8303Title));

    /// <summary>
    /// Gets the localized string for RH5406Title
    /// </summary>
    internal static string RH5406Title => GetString(nameof(RH5406Title));

    /// <summary>
    /// Gets the localized string for RH5407Title
    /// </summary>
    internal static string RH5407Title => GetString(nameof(RH5407Title));

    /// <summary>
    /// Gets the localized string for RH5104Title
    /// </summary>
    internal static string RH5104Title => GetString(nameof(RH5104Title));

    /// <summary>
    /// Gets the localized string for RH5105Title
    /// </summary>
    internal static string RH5105Title => GetString(nameof(RH5105Title));

    /// <summary>
    /// Gets the localized string for RH5106Title
    /// </summary>
    internal static string RH5106Title => GetString(nameof(RH5106Title));

    /// <summary>
    /// Gets the localized string for RH5107Title
    /// </summary>
    internal static string RH5107Title => GetString(nameof(RH5107Title));

    /// <summary>
    /// Gets the localized string for RH5108Title
    /// </summary>
    internal static string RH5108Title => GetString(nameof(RH5108Title));

    /// <summary>
    /// Gets the localized string for RH5109Title
    /// </summary>
    internal static string RH5109Title => GetString(nameof(RH5109Title));

    /// <summary>
    /// Gets the localized string for RH7102Title
    /// </summary>
    internal static string RH7102Title => GetString(nameof(RH7102Title));

    /// <summary>
    /// Gets the localized string for RH7103Title
    /// </summary>
    internal static string RH7103Title => GetString(nameof(RH7103Title));

    /// <summary>
    /// Gets the localized string for RH7104Title
    /// </summary>
    internal static string RH7104Title => GetString(nameof(RH7104Title));

    /// <summary>
    /// Gets the localized string for RH7105Title
    /// </summary>
    internal static string RH7105Title => GetString(nameof(RH7105Title));

    /// <summary>
    /// Gets the localized string for RH7106Title
    /// </summary>
    internal static string RH7106Title => GetString(nameof(RH7106Title));

    /// <summary>
    /// Gets the localized string for RH7201Title
    /// </summary>
    internal static string RH7201Title => GetString(nameof(RH7201Title));

    /// <summary>
    /// Gets the localized string for RH7202Title
    /// </summary>
    internal static string RH7202Title => GetString(nameof(RH7202Title));

    /// <summary>
    /// Gets the localized string for RH7203Title
    /// </summary>
    internal static string RH7203Title => GetString(nameof(RH7203Title));

    /// <summary>
    /// Gets the localized string for RH7204Title
    /// </summary>
    internal static string RH7204Title => GetString(nameof(RH7204Title));

    /// <summary>
    /// Gets the localized string for RH7107Title
    /// </summary>
    internal static string RH7107Title => GetString(nameof(RH7107Title));

    /// <summary>
    /// Gets the localized string for RH7108Title
    /// </summary>
    internal static string RH7108Title => GetString(nameof(RH7108Title));

    /// <summary>
    /// Gets the localized string for RH7109Title
    /// </summary>
    internal static string RH7109Title => GetString(nameof(RH7109Title));

    /// <summary>
    /// Gets the localized string for RH7205Title
    /// </summary>
    internal static string RH7205Title => GetString(nameof(RH7205Title));

    /// <summary>
    /// Gets the localized string for RH7206Title
    /// </summary>
    internal static string RH7206Title => GetString(nameof(RH7206Title));

    /// <summary>
    /// Gets the localized string for RH7207Title
    /// </summary>
    internal static string RH7207Title => GetString(nameof(RH7207Title));

    /// <summary>
    /// Gets the localized string for RH8202Title
    /// </summary>
    internal static string RH8202Title => GetString(nameof(RH8202Title));

    /// <summary>
    /// Gets the localized string for RH8107Title
    /// </summary>
    internal static string RH8107Title => GetString(nameof(RH8107Title));

    /// <summary>
    /// Gets the localized string for RH8401Title
    /// </summary>
    internal static string RH8401Title => GetString(nameof(RH8401Title));

    /// <summary>
    /// Gets the localized string for RH8204Title
    /// </summary>
    internal static string RH8204Title => GetString(nameof(RH8204Title));

    /// <summary>
    /// Gets the localized string for RH8304Title
    /// </summary>
    internal static string RH8304Title => GetString(nameof(RH8304Title));

    /// <summary>
    /// Gets the localized string for RH8305Title
    /// </summary>
    internal static string RH8305Title => GetString(nameof(RH8305Title));

    /// <summary>
    /// Gets the localized string for RH8307Title
    /// </summary>
    internal static string RH8307Title => GetString(nameof(RH8307Title));

    /// <summary>
    /// Gets the localized string for RH8308Title
    /// </summary>
    internal static string RH8308Title => GetString(nameof(RH8308Title));

    /// <summary>
    /// Gets the localized string for RH8309Title
    /// </summary>
    internal static string RH8309Title => GetString(nameof(RH8309Title));

    /// <summary>
    /// Gets the localized string for RH8402Title
    /// </summary>
    internal static string RH8402Title => GetString(nameof(RH8402Title));

    /// <summary>
    /// Gets the localized string for RH8201Title
    /// </summary>
    internal static string RH8201Title => GetString(nameof(RH8201Title));

    /// <summary>
    /// Gets the localized string for RH5501Title
    /// </summary>
    internal static string RH5501Title => GetString(nameof(RH5501Title));

    /// <summary>
    /// Gets the localized string for RH5502Title
    /// </summary>
    internal static string RH5502Title => GetString(nameof(RH5502Title));

    /// <summary>
    /// Gets the localized string for RH5503Title
    /// </summary>
    internal static string RH5503Title => GetString(nameof(RH5503Title));

    /// <summary>
    /// Gets the localized string for RH5504Title
    /// </summary>
    internal static string RH5504Title => GetString(nameof(RH5504Title));

    /// <summary>
    /// Gets the localized string for RH5505Title
    /// </summary>
    internal static string RH5505Title => GetString(nameof(RH5505Title));

    /// <summary>
    /// Gets the localized string for RH5506Title
    /// </summary>
    internal static string RH5506Title => GetString(nameof(RH5506Title));

    /// <summary>
    /// Gets the localized string for RH5507Title
    /// </summary>
    internal static string RH5507Title => GetString(nameof(RH5507Title));

    /// <summary>
    /// Gets the localized string for RH5508Title
    /// </summary>
    internal static string RH5508Title => GetString(nameof(RH5508Title));

    /// <summary>
    /// Gets the localized string for RH5509Title
    /// </summary>
    internal static string RH5509Title => GetString(nameof(RH5509Title));

    /// <summary>
    /// Gets the localized string for RH5510Title
    /// </summary>
    internal static string RH5510Title => GetString(nameof(RH5510Title));

    /// <summary>
    /// Gets the localized string for RH5511Title
    /// </summary>
    internal static string RH5511Title => GetString(nameof(RH5511Title));

    /// <summary>
    /// Gets the localized string for RH5512Title
    /// </summary>
    internal static string RH5512Title => GetString(nameof(RH5512Title));

    /// <summary>
    /// Gets the localized string for RH5513Title
    /// </summary>
    internal static string RH5513Title => GetString(nameof(RH5513Title));

    /// <summary>
    /// Gets the localized string for RH5514Title
    /// </summary>
    internal static string RH5514Title => GetString(nameof(RH5514Title));

    /// <summary>
    /// Gets the localized string for RH5515Title
    /// </summary>
    internal static string RH5515Title => GetString(nameof(RH5515Title));

    /// <summary>
    /// Gets the localized string for RH5516Title
    /// </summary>
    internal static string RH5516Title => GetString(nameof(RH5516Title));

    /// <summary>
    /// Gets the localized string for RH5517Title
    /// </summary>
    internal static string RH5517Title => GetString(nameof(RH5517Title));

    /// <summary>
    /// Gets the localized string for RH5518Title
    /// </summary>
    internal static string RH5518Title => GetString(nameof(RH5518Title));

    /// <summary>
    /// Gets the localized string for RH5519Title
    /// </summary>
    internal static string RH5519Title => GetString(nameof(RH5519Title));

    /// <summary>
    /// Gets the localized string for RH5520Title
    /// </summary>
    internal static string RH5520Title => GetString(nameof(RH5520Title));

    /// <summary>
    /// Gets the localized string for RH5521Title
    /// </summary>
    internal static string RH5521Title => GetString(nameof(RH5521Title));

    /// <summary>
    /// Gets the localized string for RH5522Title
    /// </summary>
    internal static string RH5522Title => GetString(nameof(RH5522Title));

    /// <summary>
    /// Gets the localized string for RH5523Title
    /// </summary>
    internal static string RH5523Title => GetString(nameof(RH5523Title));

    /// <summary>
    /// Gets the localized string for RH5524Title
    /// </summary>
    internal static string RH5524Title => GetString(nameof(RH5524Title));

    /// <summary>
    /// Gets the localized string for RH5525Title
    /// </summary>
    internal static string RH5525Title => GetString(nameof(RH5525Title));

    /// <summary>
    /// Gets the localized string for RH5526Title
    /// </summary>
    internal static string RH5526Title => GetString(nameof(RH5526Title));

    /// <summary>
    /// Gets the localized string for RH5527Title
    /// </summary>
    internal static string RH5527Title => GetString(nameof(RH5527Title));

    /// <summary>
    /// Gets the localized string for RH5528Title
    /// </summary>
    internal static string RH5528Title => GetString(nameof(RH5528Title));

    /// <summary>
    /// Gets the localized string for RH5529Title
    /// </summary>
    internal static string RH5529Title => GetString(nameof(RH5529Title));

    /// <summary>
    /// Gets the localized string for RH5530Title
    /// </summary>
    internal static string RH5530Title => GetString(nameof(RH5530Title));

    /// <summary>
    /// Gets the localized string for RH5531Title
    /// </summary>
    internal static string RH5531Title => GetString(nameof(RH5531Title));

    /// <summary>
    /// Gets the localized string for RH5113Title
    /// </summary>
    internal static string RH5113Title => GetString(nameof(RH5113Title));

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Resolves a localized string by resource key
    /// </summary>
    /// <param name="name">The resource key to resolve</param>
    /// <returns>The localized string for the requested key</returns>
    /// <exception cref="InvalidOperationException">Thrown when the key cannot be resolved to a localized string</exception>
    private static string GetString(string name)
    {
        return _resourceManagerInstance.GetString(name)
                   ?? throw new InvalidOperationException($"The resource string '{name}' could not be resolved");
    }

    #endregion // Methods
}