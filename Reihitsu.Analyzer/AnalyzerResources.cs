using System;
using System.Resources;

namespace Reihitsu.Analyzer;

/// <summary>
/// Provides strongly typed access to localized analyzer diagnostic strings
/// </summary>
internal static class AnalyzerResources
{
    #region Properties

    /// <summary>
    /// Gets the resource manager used to resolve localized strings
    /// </summary>
    internal static ResourceManager ResourceManager { get; } = new("Reihitsu.Analyzer.AnalyzerResources", typeof(AnalyzerResources).Assembly);

    /// <summary>
    /// Gets the localized string for RH3001MessageFormat
    /// </summary>
    internal static string RH3001MessageFormat => GetString(nameof(RH3001MessageFormat));

    /// <summary>
    /// Gets the localized string for RH3001Title
    /// </summary>
    internal static string RH3001Title => GetString(nameof(RH3001Title));

    /// <summary>
    /// Gets the localized string for RH3101MessageFormat
    /// </summary>
    internal static string RH3101MessageFormat => GetString(nameof(RH3101MessageFormat));

    /// <summary>
    /// Gets the localized string for RH3101Title
    /// </summary>
    internal static string RH3101Title => GetString(nameof(RH3101Title));

    /// <summary>
    /// Gets the localized string for RH3102MessageFormat
    /// </summary>
    internal static string RH3102MessageFormat => GetString(nameof(RH3102MessageFormat));

    /// <summary>
    /// Gets the localized string for RH3102Title
    /// </summary>
    internal static string RH3102Title => GetString(nameof(RH3102Title));

    /// <summary>
    /// Gets the localized string for RH3002MessageFormat
    /// </summary>
    internal static string RH3002MessageFormat => GetString(nameof(RH3002MessageFormat));

    /// <summary>
    /// Gets the localized string for RH3002Title
    /// </summary>
    internal static string RH3002Title => GetString(nameof(RH3002Title));

    /// <summary>
    /// Gets the localized string for RH3201MessageFormat
    /// </summary>
    internal static string RH3201MessageFormat => GetString(nameof(RH3201MessageFormat));

    /// <summary>
    /// Gets the localized string for RH3201Title
    /// </summary>
    internal static string RH3201Title => GetString(nameof(RH3201Title));

    /// <summary>
    /// Gets the localized string for RH3003MessageFormat
    /// </summary>
    internal static string RH3003MessageFormat => GetString(nameof(RH3003MessageFormat));

    /// <summary>
    /// Gets the localized string for RH3003Title
    /// </summary>
    internal static string RH3003Title => GetString(nameof(RH3003Title));

    /// <summary>
    /// Gets the localized string for RH3103MessageFormat
    /// </summary>
    internal static string RH3103MessageFormat => GetString(nameof(RH3103MessageFormat));

    /// <summary>
    /// Gets the localized string for RH3103Title
    /// </summary>
    internal static string RH3103Title => GetString(nameof(RH3103Title));

    /// <summary>
    /// Gets the localized string for RH3104MessageFormat
    /// </summary>
    internal static string RH3104MessageFormat => GetString(nameof(RH3104MessageFormat));

    /// <summary>
    /// Gets the localized string for RH3104Title
    /// </summary>
    internal static string RH3104Title => GetString(nameof(RH3104Title));

    /// <summary>
    /// Gets the localized string for RH3004MessageFormat
    /// </summary>
    internal static string RH3004MessageFormat => GetString(nameof(RH3004MessageFormat));

    /// <summary>
    /// Gets the localized string for RH3004Title
    /// </summary>
    internal static string RH3004Title => GetString(nameof(RH3004Title));

    /// <summary>
    /// Gets the localized string for RH3005MessageFormat
    /// </summary>
    internal static string RH3005MessageFormat => GetString(nameof(RH3005MessageFormat));

    /// <summary>
    /// Gets the localized string for RH3005Title
    /// </summary>
    internal static string RH3005Title => GetString(nameof(RH3005Title));

    /// <summary>
    /// Gets the localized string for RH3006MessageFormat
    /// </summary>
    internal static string RH3006MessageFormat => GetString(nameof(RH3006MessageFormat));

    /// <summary>
    /// Gets the localized string for RH3006Title
    /// </summary>
    internal static string RH3006Title => GetString(nameof(RH3006Title));

    /// <summary>
    /// Gets the localized string for RH3105MessageFormat
    /// </summary>
    internal static string RH3105MessageFormat => GetString(nameof(RH3105MessageFormat));

    /// <summary>
    /// Gets the localized string for RH3105Title
    /// </summary>
    internal static string RH3105Title => GetString(nameof(RH3105Title));

    /// <summary>
    /// Gets the localized string for RH3007MessageFormat
    /// </summary>
    internal static string RH3007MessageFormat => GetString(nameof(RH3007MessageFormat));

    /// <summary>
    /// Gets the localized string for RH3007Title
    /// </summary>
    internal static string RH3007Title => GetString(nameof(RH3007Title));

    /// <summary>
    /// Gets the localized string for RH2001MessageFormat
    /// </summary>
    internal static string RH2001MessageFormat => GetString(nameof(RH2001MessageFormat));

    /// <summary>
    /// Gets the localized string for RH2001Title
    /// </summary>
    internal static string RH2001Title => GetString(nameof(RH2001Title));

    /// <summary>
    /// Gets the localized string for RH2002MessageFormat
    /// </summary>
    internal static string RH2002MessageFormat => GetString(nameof(RH2002MessageFormat));

    /// <summary>
    /// Gets the localized string for RH2002Title
    /// </summary>
    internal static string RH2002Title => GetString(nameof(RH2002Title));

    /// <summary>
    /// Gets the localized string for RH2003MessageFormat
    /// </summary>
    internal static string RH2003MessageFormat => GetString(nameof(RH2003MessageFormat));

    /// <summary>
    /// Gets the localized string for RH2003Title
    /// </summary>
    internal static string RH2003Title => GetString(nameof(RH2003Title));

    /// <summary>
    /// Gets the localized string for RH2004MessageFormat
    /// </summary>
    internal static string RH2004MessageFormat => GetString(nameof(RH2004MessageFormat));

    /// <summary>
    /// Gets the localized string for RH2004Title
    /// </summary>
    internal static string RH2004Title => GetString(nameof(RH2004Title));

    /// <summary>
    /// Gets the localized string for RH2005MessageFormat
    /// </summary>
    internal static string RH2005MessageFormat => GetString(nameof(RH2005MessageFormat));

    /// <summary>
    /// Gets the localized string for RH2005Title
    /// </summary>
    internal static string RH2005Title => GetString(nameof(RH2005Title));

    /// <summary>
    /// Gets the localized string for RH7001MessageFormat
    /// </summary>
    internal static string RH7001MessageFormat => GetString(nameof(RH7001MessageFormat));

    /// <summary>
    /// Gets the localized string for RH7001Title
    /// </summary>
    internal static string RH7001Title => GetString(nameof(RH7001Title));

    /// <summary>
    /// Gets the localized string for RH8501MessageFormat
    /// </summary>
    internal static string RH8501MessageFormat => GetString(nameof(RH8501MessageFormat));

    /// <summary>
    /// Gets the localized string for RH8501Title
    /// </summary>
    internal static string RH8501Title => GetString(nameof(RH8501Title));

    /// <summary>
    /// Gets the localized string for RH2006MessageFormat
    /// </summary>
    internal static string RH2006MessageFormat => GetString(nameof(RH2006MessageFormat));

    /// <summary>
    /// Gets the localized string for RH2006Title
    /// </summary>
    internal static string RH2006Title => GetString(nameof(RH2006Title));

    /// <summary>
    /// Gets the localized string for RH2007MessageFormat
    /// </summary>
    internal static string RH2007MessageFormat => GetString(nameof(RH2007MessageFormat));

    /// <summary>
    /// Gets the localized string for RH2007Title
    /// </summary>
    internal static string RH2007Title => GetString(nameof(RH2007Title));

    /// <summary>
    /// Gets the localized string for RH3106MessageFormat
    /// </summary>
    internal static string RH3106MessageFormat => GetString(nameof(RH3106MessageFormat));

    /// <summary>
    /// Gets the localized string for RH3106Title
    /// </summary>
    internal static string RH3106Title => GetString(nameof(RH3106Title));

    /// <summary>
    /// Gets the localized string for RH3107MessageFormat
    /// </summary>
    internal static string RH3107MessageFormat => GetString(nameof(RH3107MessageFormat));

    /// <summary>
    /// Gets the localized string for RH3107Title
    /// </summary>
    internal static string RH3107Title => GetString(nameof(RH3107Title));

    /// <summary>
    /// Gets the localized string for RH7002MessageFormat
    /// </summary>
    internal static string RH7002MessageFormat => GetString(nameof(RH7002MessageFormat));

    /// <summary>
    /// Gets the localized string for RH7002Title
    /// </summary>
    internal static string RH7002Title => GetString(nameof(RH7002Title));

    /// <summary>
    /// Gets the localized string for RH2101MessageFormat
    /// </summary>
    internal static string RH2101MessageFormat => GetString(nameof(RH2101MessageFormat));

    /// <summary>
    /// Gets the localized string for RH2101Title
    /// </summary>
    internal static string RH2101Title => GetString(nameof(RH2101Title));

    /// <summary>
    /// Gets the localized string for RH2102MessageFormat
    /// </summary>
    internal static string RH2102MessageFormat => GetString(nameof(RH2102MessageFormat));

    /// <summary>
    /// Gets the localized string for RH2102Title
    /// </summary>
    internal static string RH2102Title => GetString(nameof(RH2102Title));

    /// <summary>
    /// Gets the localized string for RH2103MessageFormat
    /// </summary>
    internal static string RH2103MessageFormat => GetString(nameof(RH2103MessageFormat));

    /// <summary>
    /// Gets the localized string for RH2103Title
    /// </summary>
    internal static string RH2103Title => GetString(nameof(RH2103Title));

    /// <summary>
    /// Gets the localized string for RH2104MessageFormat
    /// </summary>
    internal static string RH2104MessageFormat => GetString(nameof(RH2104MessageFormat));

    /// <summary>
    /// Gets the localized string for RH2104Title
    /// </summary>
    internal static string RH2104Title => GetString(nameof(RH2104Title));

    /// <summary>
    /// Gets the localized string for RH2105MessageFormat
    /// </summary>
    internal static string RH2105MessageFormat => GetString(nameof(RH2105MessageFormat));

    /// <summary>
    /// Gets the localized string for RH2105Title
    /// </summary>
    internal static string RH2105Title => GetString(nameof(RH2105Title));

    /// <summary>
    /// Gets the localized string for RH2106MessageFormat
    /// </summary>
    internal static string RH2106MessageFormat => GetString(nameof(RH2106MessageFormat));

    /// <summary>
    /// Gets the localized string for RH2106Title
    /// </summary>
    internal static string RH2106Title => GetString(nameof(RH2106Title));

    /// <summary>
    /// Gets the localized string for RH2107MessageFormat
    /// </summary>
    internal static string RH2107MessageFormat => GetString(nameof(RH2107MessageFormat));

    /// <summary>
    /// Gets the localized string for RH2107Title
    /// </summary>
    internal static string RH2107Title => GetString(nameof(RH2107Title));

    /// <summary>
    /// Gets the localized string for RH2108MessageFormat
    /// </summary>
    internal static string RH2108MessageFormat => GetString(nameof(RH2108MessageFormat));

    /// <summary>
    /// Gets the localized string for RH2108Title
    /// </summary>
    internal static string RH2108Title => GetString(nameof(RH2108Title));

    /// <summary>
    /// Gets the localized string for RH2109MessageFormat
    /// </summary>
    internal static string RH2109MessageFormat => GetString(nameof(RH2109MessageFormat));

    /// <summary>
    /// Gets the localized string for RH2109Title
    /// </summary>
    internal static string RH2109Title => GetString(nameof(RH2109Title));

    /// <summary>
    /// Gets the localized string for RH4001MessageFormat
    /// </summary>
    internal static string RH4001MessageFormat => GetString(nameof(RH4001MessageFormat));

    /// <summary>
    /// Gets the localized string for RH4001Title
    /// </summary>
    internal static string RH4001Title => GetString(nameof(RH4001Title));

    /// <summary>
    /// Gets the localized string for RH4002MessageFormat
    /// </summary>
    internal static string RH4002MessageFormat => GetString(nameof(RH4002MessageFormat));

    /// <summary>
    /// Gets the localized string for RH4002Title
    /// </summary>
    internal static string RH4002Title => GetString(nameof(RH4002Title));

    /// <summary>
    /// Gets the localized string for RH4003MessageFormat
    /// </summary>
    internal static string RH4003MessageFormat => GetString(nameof(RH4003MessageFormat));

    /// <summary>
    /// Gets the localized string for RH4003Title
    /// </summary>
    internal static string RH4003Title => GetString(nameof(RH4003Title));

    /// <summary>
    /// Gets the localized string for RH4004Title
    /// </summary>
    internal static string RH4004Title => GetString(nameof(RH4004Title));

    /// <summary>
    /// Gets the localized string for RH4004MessageFormat
    /// </summary>
    internal static string RH4004MessageFormat => GetString(nameof(RH4004MessageFormat));

    /// <summary>
    /// Gets the localized string for RH4101Title
    /// </summary>
    internal static string RH4101Title => GetString(nameof(RH4101Title));

    /// <summary>
    /// Gets the localized string for RH4101MessageFormat
    /// </summary>
    internal static string RH4101MessageFormat => GetString(nameof(RH4101MessageFormat));

    /// <summary>
    /// Gets the localized string for RH4005Title
    /// </summary>
    internal static string RH4005Title => GetString(nameof(RH4005Title));

    /// <summary>
    /// Gets the localized string for RH4005MessageFormat
    /// </summary>
    internal static string RH4005MessageFormat => GetString(nameof(RH4005MessageFormat));

    /// <summary>
    /// Gets the localized string for RH4102Title
    /// </summary>
    internal static string RH4102Title => GetString(nameof(RH4102Title));

    /// <summary>
    /// Gets the localized string for RH4102MessageFormat
    /// </summary>
    internal static string RH4102MessageFormat => GetString(nameof(RH4102MessageFormat));

    /// <summary>
    /// Gets the localized string for RH4006Title
    /// </summary>
    internal static string RH4006Title => GetString(nameof(RH4006Title));

    /// <summary>
    /// Gets the localized string for RH4006MessageFormat
    /// </summary>
    internal static string RH4006MessageFormat => GetString(nameof(RH4006MessageFormat));

    /// <summary>
    /// Gets the localized string for RH4103Title
    /// </summary>
    internal static string RH4103Title => GetString(nameof(RH4103Title));

    /// <summary>
    /// Gets the localized string for RH4103MessageFormat
    /// </summary>
    internal static string RH4103MessageFormat => GetString(nameof(RH4103MessageFormat));

    /// <summary>
    /// Gets the localized string for RH4104Title
    /// </summary>
    internal static string RH4104Title => GetString(nameof(RH4104Title));

    /// <summary>
    /// Gets the localized string for RH4104MessageFormat
    /// </summary>
    internal static string RH4104MessageFormat => GetString(nameof(RH4104MessageFormat));

    /// <summary>
    /// Gets the localized string for RH4105Title
    /// </summary>
    internal static string RH4105Title => GetString(nameof(RH4105Title));

    /// <summary>
    /// Gets the localized string for RH4105MessageFormat
    /// </summary>
    internal static string RH4105MessageFormat => GetString(nameof(RH4105MessageFormat));

    /// <summary>
    /// Gets the localized string for RH4106Title
    /// </summary>
    internal static string RH4106Title => GetString(nameof(RH4106Title));

    /// <summary>
    /// Gets the localized string for RH4106MessageFormat
    /// </summary>
    internal static string RH4106MessageFormat => GetString(nameof(RH4106MessageFormat));

    /// <summary>
    /// Gets the localized string for RH4107Title
    /// </summary>
    internal static string RH4107Title => GetString(nameof(RH4107Title));

    /// <summary>
    /// Gets the localized string for RH4107MessageFormat
    /// </summary>
    internal static string RH4107MessageFormat => GetString(nameof(RH4107MessageFormat));

    /// <summary>
    /// Gets the localized string for RH4108Title
    /// </summary>
    internal static string RH4108Title => GetString(nameof(RH4108Title));

    /// <summary>
    /// Gets the localized string for RH4108MessageFormat
    /// </summary>
    internal static string RH4108MessageFormat => GetString(nameof(RH4108MessageFormat));

    /// <summary>
    /// Gets the localized string for RH4109Title
    /// </summary>
    internal static string RH4109Title => GetString(nameof(RH4109Title));

    /// <summary>
    /// Gets the localized string for RH4109MessageFormat
    /// </summary>
    internal static string RH4109MessageFormat => GetString(nameof(RH4109MessageFormat));

    /// <summary>
    /// Gets the localized string for RH4110Title
    /// </summary>
    internal static string RH4110Title => GetString(nameof(RH4110Title));

    /// <summary>
    /// Gets the localized string for RH4110MessageFormat
    /// </summary>
    internal static string RH4110MessageFormat => GetString(nameof(RH4110MessageFormat));

    /// <summary>
    /// Gets the localized string for RH4111Title
    /// </summary>
    internal static string RH4111Title => GetString(nameof(RH4111Title));

    /// <summary>
    /// Gets the localized string for RH4111MessageFormat
    /// </summary>
    internal static string RH4111MessageFormat => GetString(nameof(RH4111MessageFormat));

    /// <summary>
    /// Gets the localized string for RH4112Title
    /// </summary>
    internal static string RH4112Title => GetString(nameof(RH4112Title));

    /// <summary>
    /// Gets the localized string for RH4112MessageFormat
    /// </summary>
    internal static string RH4112MessageFormat => GetString(nameof(RH4112MessageFormat));

    /// <summary>
    /// Gets the localized string for RH4113Title
    /// </summary>
    internal static string RH4113Title => GetString(nameof(RH4113Title));

    /// <summary>
    /// Gets the localized string for RH4113MessageFormat
    /// </summary>
    internal static string RH4113MessageFormat => GetString(nameof(RH4113MessageFormat));

    /// <summary>
    /// Gets the localized string for RH4114Title
    /// </summary>
    internal static string RH4114Title => GetString(nameof(RH4114Title));

    /// <summary>
    /// Gets the localized string for RH4114MessageFormat
    /// </summary>
    internal static string RH4114MessageFormat => GetString(nameof(RH4114MessageFormat));

    /// <summary>
    /// Gets the localized string for RH4115Title
    /// </summary>
    internal static string RH4115Title => GetString(nameof(RH4115Title));

    /// <summary>
    /// Gets the localized string for RH4115MessageFormat
    /// </summary>
    internal static string RH4115MessageFormat => GetString(nameof(RH4115MessageFormat));

    /// <summary>
    /// Gets the localized string for RH4116Title
    /// </summary>
    internal static string RH4116Title => GetString(nameof(RH4116Title));

    /// <summary>
    /// Gets the localized string for RH4116MessageFormat
    /// </summary>
    internal static string RH4116MessageFormat => GetString(nameof(RH4116MessageFormat));

    /// <summary>
    /// Gets the localized string for RH4117Title
    /// </summary>
    internal static string RH4117Title => GetString(nameof(RH4117Title));

    /// <summary>
    /// Gets the localized string for RH4117MessageFormat
    /// </summary>
    internal static string RH4117MessageFormat => GetString(nameof(RH4117MessageFormat));

    /// <summary>
    /// Gets the localized string for RH4118Title
    /// </summary>
    internal static string RH4118Title => GetString(nameof(RH4118Title));

    /// <summary>
    /// Gets the localized string for RH4118MessageFormat
    /// </summary>
    internal static string RH4118MessageFormat => GetString(nameof(RH4118MessageFormat));

    /// <summary>
    /// Gets the localized string for RH4007Title
    /// </summary>
    internal static string RH4007Title => GetString(nameof(RH4007Title));

    /// <summary>
    /// Gets the localized string for RH4007MessageFormat
    /// </summary>
    internal static string RH4007MessageFormat => GetString(nameof(RH4007MessageFormat));

    /// <summary>
    /// Gets the localized string for RH4008Title
    /// </summary>
    internal static string RH4008Title => GetString(nameof(RH4008Title));

    /// <summary>
    /// Gets the localized string for RH4008MessageFormat
    /// </summary>
    internal static string RH4008MessageFormat => GetString(nameof(RH4008MessageFormat));

    /// <summary>
    /// Gets the localized string for RH4009Title
    /// </summary>
    internal static string RH4009Title => GetString(nameof(RH4009Title));

    /// <summary>
    /// Gets the localized string for RH4009MessageFormat
    /// </summary>
    internal static string RH4009MessageFormat => GetString(nameof(RH4009MessageFormat));

    /// <summary>
    /// Gets the localized string for RH4119Title
    /// </summary>
    internal static string RH4119Title => GetString(nameof(RH4119Title));

    /// <summary>
    /// Gets the localized string for RH4119MessageFormat
    /// </summary>
    internal static string RH4119MessageFormat => GetString(nameof(RH4119MessageFormat));

    /// <summary>
    /// Gets the localized string for RH4010Title
    /// </summary>
    internal static string RH4010Title => GetString(nameof(RH4010Title));

    /// <summary>
    /// Gets the localized string for RH4010MessageFormat
    /// </summary>
    internal static string RH4010MessageFormat => GetString(nameof(RH4010MessageFormat));

    /// <summary>
    /// Gets the localized string for RH4011Title
    /// </summary>
    internal static string RH4011Title => GetString(nameof(RH4011Title));

    /// <summary>
    /// Gets the localized string for RH4011MessageFormat
    /// </summary>
    internal static string RH4011MessageFormat => GetString(nameof(RH4011MessageFormat));

    /// <summary>
    /// Gets the localized string for RH4120Title
    /// </summary>
    internal static string RH4120Title => GetString(nameof(RH4120Title));

    /// <summary>
    /// Gets the localized string for RH4120MessageFormat
    /// </summary>
    internal static string RH4120MessageFormat => GetString(nameof(RH4120MessageFormat));

    /// <summary>
    /// Gets the localized string for RH7301MessageFormat
    /// </summary>
    internal static string RH7301MessageFormat => GetString(nameof(RH7301MessageFormat));

    /// <summary>
    /// Gets the localized string for RH7301Title
    /// </summary>
    internal static string RH7301Title => GetString(nameof(RH7301Title));

    /// <summary>
    /// Gets the localized string for RH5301MessageFormat
    /// </summary>
    internal static string RH5301MessageFormat => GetString(nameof(RH5301MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5301Title
    /// </summary>
    internal static string RH5301Title => GetString(nameof(RH5301Title));

    /// <summary>
    /// Gets the localized string for RH5001MessageFormat
    /// </summary>
    internal static string RH5001MessageFormat => GetString(nameof(RH5001MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5001Title
    /// </summary>
    internal static string RH5001Title => GetString(nameof(RH5001Title));

    /// <summary>
    /// Gets the localized string for RH5002MessageFormat
    /// </summary>
    internal static string RH5002MessageFormat => GetString(nameof(RH5002MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5002Title
    /// </summary>
    internal static string RH5002Title => GetString(nameof(RH5002Title));

    /// <summary>
    /// Gets the localized string for RH5003MessageFormat
    /// </summary>
    internal static string RH5003MessageFormat => GetString(nameof(RH5003MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5003Title
    /// </summary>
    internal static string RH5003Title => GetString(nameof(RH5003Title));

    /// <summary>
    /// Gets the localized string for RH5004MessageFormat
    /// </summary>
    internal static string RH5004MessageFormat => GetString(nameof(RH5004MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5004Title
    /// </summary>
    internal static string RH5004Title => GetString(nameof(RH5004Title));

    /// <summary>
    /// Gets the localized string for RH5005MessageFormat
    /// </summary>
    internal static string RH5005MessageFormat => GetString(nameof(RH5005MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5005Title
    /// </summary>
    internal static string RH5005Title => GetString(nameof(RH5005Title));

    /// <summary>
    /// Gets the localized string for RH5006MessageFormat
    /// </summary>
    internal static string RH5006MessageFormat => GetString(nameof(RH5006MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5006Title
    /// </summary>
    internal static string RH5006Title => GetString(nameof(RH5006Title));

    /// <summary>
    /// Gets the localized string for RH5007MessageFormat
    /// </summary>
    internal static string RH5007MessageFormat => GetString(nameof(RH5007MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5007Title
    /// </summary>
    internal static string RH5007Title => GetString(nameof(RH5007Title));

    /// <summary>
    /// Gets the localized string for RH5008MessageFormat
    /// </summary>
    internal static string RH5008MessageFormat => GetString(nameof(RH5008MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5008Title
    /// </summary>
    internal static string RH5008Title => GetString(nameof(RH5008Title));

    /// <summary>
    /// Gets the localized string for RH5009MessageFormat
    /// </summary>
    internal static string RH5009MessageFormat => GetString(nameof(RH5009MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5009Title
    /// </summary>
    internal static string RH5009Title => GetString(nameof(RH5009Title));

    /// <summary>
    /// Gets the localized string for RH5010MessageFormat
    /// </summary>
    internal static string RH5010MessageFormat => GetString(nameof(RH5010MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5010Title
    /// </summary>
    internal static string RH5010Title => GetString(nameof(RH5010Title));

    /// <summary>
    /// Gets the localized string for RH5011MessageFormat
    /// </summary>
    internal static string RH5011MessageFormat => GetString(nameof(RH5011MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5011Title
    /// </summary>
    internal static string RH5011Title => GetString(nameof(RH5011Title));

    /// <summary>
    /// Gets the localized string for RH5012MessageFormat
    /// </summary>
    internal static string RH5012MessageFormat => GetString(nameof(RH5012MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5012Title
    /// </summary>
    internal static string RH5012Title => GetString(nameof(RH5012Title));

    /// <summary>
    /// Gets the localized string for RH5013MessageFormat
    /// </summary>
    internal static string RH5013MessageFormat => GetString(nameof(RH5013MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5013Title
    /// </summary>
    internal static string RH5013Title => GetString(nameof(RH5013Title));

    /// <summary>
    /// Gets the localized string for RH5014MessageFormat
    /// </summary>
    internal static string RH5014MessageFormat => GetString(nameof(RH5014MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5014Title
    /// </summary>
    internal static string RH5014Title => GetString(nameof(RH5014Title));

    /// <summary>
    /// Gets the localized string for RH5015MessageFormat
    /// </summary>
    internal static string RH5015MessageFormat => GetString(nameof(RH5015MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5015Title
    /// </summary>
    internal static string RH5015Title => GetString(nameof(RH5015Title));

    /// <summary>
    /// Gets the localized string for RH5016MessageFormat
    /// </summary>
    internal static string RH5016MessageFormat => GetString(nameof(RH5016MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5016Title
    /// </summary>
    internal static string RH5016Title => GetString(nameof(RH5016Title));

    /// <summary>
    /// Gets the localized string for RH5017MessageFormat
    /// </summary>
    internal static string RH5017MessageFormat => GetString(nameof(RH5017MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5017Title
    /// </summary>
    internal static string RH5017Title => GetString(nameof(RH5017Title));

    /// <summary>
    /// Gets the localized string for RH5018MessageFormat
    /// </summary>
    internal static string RH5018MessageFormat => GetString(nameof(RH5018MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5018Title
    /// </summary>
    internal static string RH5018Title => GetString(nameof(RH5018Title));

    /// <summary>
    /// Gets the localized string for RH5019MessageFormat
    /// </summary>
    internal static string RH5019MessageFormat => GetString(nameof(RH5019MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5019Title
    /// </summary>
    internal static string RH5019Title => GetString(nameof(RH5019Title));

    /// <summary>
    /// Gets the localized string for RH5201MessageFormat
    /// </summary>
    internal static string RH5201MessageFormat => GetString(nameof(RH5201MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5201Title
    /// </summary>
    internal static string RH5201Title => GetString(nameof(RH5201Title));

    /// <summary>
    /// Gets the localized string for RH5112MessageFormat
    /// </summary>
    internal static string RH5112MessageFormat => GetString(nameof(RH5112MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5112Title
    /// </summary>
    internal static string RH5112Title => GetString(nameof(RH5112Title));

    /// <summary>
    /// Gets the localized string for RH5303MessageFormat
    /// </summary>
    internal static string RH5303MessageFormat => GetString(nameof(RH5303MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5303Title
    /// </summary>
    internal static string RH5303Title => GetString(nameof(RH5303Title));

    /// <summary>
    /// Gets the localized string for RH5206MessageFormat
    /// </summary>
    internal static string RH5206MessageFormat => GetString(nameof(RH5206MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5206Title
    /// </summary>
    internal static string RH5206Title => GetString(nameof(RH5206Title));

    /// <summary>
    /// Gets the localized string for RH7401MessageFormat
    /// </summary>
    internal static string RH7401MessageFormat => GetString(nameof(RH7401MessageFormat));

    /// <summary>
    /// Gets the localized string for RH7401Title
    /// </summary>
    internal static string RH7401Title => GetString(nameof(RH7401Title));

    /// <summary>
    /// Gets the localized string for RH7402MessageFormat
    /// </summary>
    internal static string RH7402MessageFormat => GetString(nameof(RH7402MessageFormat));

    /// <summary>
    /// Gets the localized string for RH7402Title
    /// </summary>
    internal static string RH7402Title => GetString(nameof(RH7402Title));

    /// <summary>
    /// Gets the localized string for RH7403MessageFormat
    /// </summary>
    internal static string RH7403MessageFormat => GetString(nameof(RH7403MessageFormat));

    /// <summary>
    /// Gets the localized string for RH7403Title
    /// </summary>
    internal static string RH7403Title => GetString(nameof(RH7403Title));

    /// <summary>
    /// Gets the localized string for RH7404MessageFormat
    /// </summary>
    internal static string RH7404MessageFormat => GetString(nameof(RH7404MessageFormat));

    /// <summary>
    /// Gets the localized string for RH7404Title
    /// </summary>
    internal static string RH7404Title => GetString(nameof(RH7404Title));

    /// <summary>
    /// Gets the localized string for RH7405MessageFormat
    /// </summary>
    internal static string RH7405MessageFormat => GetString(nameof(RH7405MessageFormat));

    /// <summary>
    /// Gets the localized string for RH7405Title
    /// </summary>
    internal static string RH7405Title => GetString(nameof(RH7405Title));

    /// <summary>
    /// Gets the localized string for RH7406MessageFormat
    /// </summary>
    internal static string RH7406MessageFormat => GetString(nameof(RH7406MessageFormat));

    /// <summary>
    /// Gets the localized string for RH7406Title
    /// </summary>
    internal static string RH7406Title => GetString(nameof(RH7406Title));

    /// <summary>
    /// Gets the localized string for RH7407MessageFormat
    /// </summary>
    internal static string RH7407MessageFormat => GetString(nameof(RH7407MessageFormat));

    /// <summary>
    /// Gets the localized string for RH7407Title
    /// </summary>
    internal static string RH7407Title => GetString(nameof(RH7407Title));

    /// <summary>
    /// Gets the localized string for RH7408MessageFormat
    /// </summary>
    internal static string RH7408MessageFormat => GetString(nameof(RH7408MessageFormat));

    /// <summary>
    /// Gets the localized string for RH7408Title
    /// </summary>
    internal static string RH7408Title => GetString(nameof(RH7408Title));

    /// <summary>
    /// Gets the localized string for RH5408MessageFormat
    /// </summary>
    internal static string RH5408MessageFormat => GetString(nameof(RH5408MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5408Title
    /// </summary>
    internal static string RH5408Title => GetString(nameof(RH5408Title));

    /// <summary>
    /// Gets the localized string for RH7003MessageFormat
    /// </summary>
    internal static string RH7003MessageFormat => GetString(nameof(RH7003MessageFormat));

    /// <summary>
    /// Gets the localized string for RH7003Title
    /// </summary>
    internal static string RH7003Title => GetString(nameof(RH7003Title));

    /// <summary>
    /// Gets the localized string for RH5409MessageFormat
    /// </summary>
    internal static string RH5409MessageFormat => GetString(nameof(RH5409MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5409Title
    /// </summary>
    internal static string RH5409Title => GetString(nameof(RH5409Title));

    /// <summary>
    /// Gets the localized string for RH5410MessageFormat
    /// </summary>
    internal static string RH5410MessageFormat => GetString(nameof(RH5410MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5410Title
    /// </summary>
    internal static string RH5410Title => GetString(nameof(RH5410Title));

    /// <summary>
    /// Gets the localized string for RH5411MessageFormat
    /// </summary>
    internal static string RH5411MessageFormat => GetString(nameof(RH5411MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5411Title
    /// </summary>
    internal static string RH5411Title => GetString(nameof(RH5411Title));

    /// <summary>
    /// Gets the localized string for RH5412MessageFormat
    /// </summary>
    internal static string RH5412MessageFormat => GetString(nameof(RH5412MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5412Title
    /// </summary>
    internal static string RH5412Title => GetString(nameof(RH5412Title));

    /// <summary>
    /// Gets the localized string for RH5413MessageFormat
    /// </summary>
    internal static string RH5413MessageFormat => GetString(nameof(RH5413MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5413Title
    /// </summary>
    internal static string RH5413Title => GetString(nameof(RH5413Title));

    /// <summary>
    /// Gets the localized string for RH5414MessageFormat
    /// </summary>
    internal static string RH5414MessageFormat => GetString(nameof(RH5414MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5414Title
    /// </summary>
    internal static string RH5414Title => GetString(nameof(RH5414Title));

    /// <summary>
    /// Gets the localized string for RH5415MessageFormat
    /// </summary>
    internal static string RH5415MessageFormat => GetString(nameof(RH5415MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5415Title
    /// </summary>
    internal static string RH5415Title => GetString(nameof(RH5415Title));

    /// <summary>
    /// Gets the localized string for RH5416MessageFormat
    /// </summary>
    internal static string RH5416MessageFormat => GetString(nameof(RH5416MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5416Title
    /// </summary>
    internal static string RH5416Title => GetString(nameof(RH5416Title));

    /// <summary>
    /// Gets the localized string for RH3204MessageFormat
    /// </summary>
    internal static string RH3204MessageFormat => GetString(nameof(RH3204MessageFormat));

    /// <summary>
    /// Gets the localized string for RH3204Title
    /// </summary>
    internal static string RH3204Title => GetString(nameof(RH3204Title));

    /// <summary>
    /// Gets the localized string for RH5304MessageFormat
    /// </summary>
    internal static string RH5304MessageFormat => GetString(nameof(RH5304MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5304Title
    /// </summary>
    internal static string RH5304Title => GetString(nameof(RH5304Title));

    /// <summary>
    /// Gets the localized string for RH5305MessageFormat
    /// </summary>
    internal static string RH5305MessageFormat => GetString(nameof(RH5305MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5305Title
    /// </summary>
    internal static string RH5305Title => GetString(nameof(RH5305Title));

    /// <summary>
    /// Gets the localized string for RH5306MessageFormat
    /// </summary>
    internal static string RH5306MessageFormat => GetString(nameof(RH5306MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5306Title
    /// </summary>
    internal static string RH5306Title => GetString(nameof(RH5306Title));

    /// <summary>
    /// Gets the localized string for RH5307MessageFormat
    /// </summary>
    internal static string RH5307MessageFormat => GetString(nameof(RH5307MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5307Title
    /// </summary>
    internal static string RH5307Title => GetString(nameof(RH5307Title));

    /// <summary>
    /// Gets the localized string for RH5020MessageFormat
    /// </summary>
    internal static string RH5020MessageFormat => GetString(nameof(RH5020MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5020Title
    /// </summary>
    internal static string RH5020Title => GetString(nameof(RH5020Title));

    /// <summary>
    /// Gets the localized string for RH5021MessageFormat
    /// </summary>
    internal static string RH5021MessageFormat => GetString(nameof(RH5021MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5021Title
    /// </summary>
    internal static string RH5021Title => GetString(nameof(RH5021Title));

    /// <summary>
    /// Gets the localized string for RH3202MessageFormat
    /// </summary>
    internal static string RH3202MessageFormat => GetString(nameof(RH3202MessageFormat));

    /// <summary>
    /// Gets the localized string for RH3202Title
    /// </summary>
    internal static string RH3202Title => GetString(nameof(RH3202Title));

    /// <summary>
    /// Gets the localized string for RH3203MessageFormat
    /// </summary>
    internal static string RH3203MessageFormat => GetString(nameof(RH3203MessageFormat));

    /// <summary>
    /// Gets the localized string for RH3203Title
    /// </summary>
    internal static string RH3203Title => GetString(nameof(RH3203Title));

    /// <summary>
    /// Gets the localized string for RH5401MessageFormat
    /// </summary>
    internal static string RH5401MessageFormat => GetString(nameof(RH5401MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5401Title
    /// </summary>
    internal static string RH5401Title => GetString(nameof(RH5401Title));

    /// <summary>
    /// Gets the localized string for RH7302MessageFormat
    /// </summary>
    internal static string RH7302MessageFormat => GetString(nameof(RH7302MessageFormat));

    /// <summary>
    /// Gets the localized string for RH7302Title
    /// </summary>
    internal static string RH7302Title => GetString(nameof(RH7302Title));

    /// <summary>
    /// Gets the localized string for RH5302MessageFormat
    /// </summary>
    internal static string RH5302MessageFormat => GetString(nameof(RH5302MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5302Title
    /// </summary>
    internal static string RH5302Title => GetString(nameof(RH5302Title));

    /// <summary>
    /// Gets the localized string for RH5202MessageFormat
    /// </summary>
    internal static string RH5202MessageFormat => GetString(nameof(RH5202MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5202Title
    /// </summary>
    internal static string RH5202Title => GetString(nameof(RH5202Title));

    /// <summary>
    /// Gets the localized string for RH5101MessageFormat
    /// </summary>
    internal static string RH5101MessageFormat => GetString(nameof(RH5101MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5101Title
    /// </summary>
    internal static string RH5101Title => GetString(nameof(RH5101Title));

    /// <summary>
    /// Gets the localized string for RH5102MessageFormat
    /// </summary>
    internal static string RH5102MessageFormat => GetString(nameof(RH5102MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5102Title
    /// </summary>
    internal static string RH5102Title => GetString(nameof(RH5102Title));

    /// <summary>
    /// Gets the localized string for RH5203MessageFormat
    /// </summary>
    internal static string RH5203MessageFormat => GetString(nameof(RH5203MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5203Title
    /// </summary>
    internal static string RH5203Title => GetString(nameof(RH5203Title));

    /// <summary>
    /// Gets the localized string for RH5602MessageFormat
    /// </summary>
    internal static string RH5602MessageFormat => GetString(nameof(RH5602MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5602Title
    /// </summary>
    internal static string RH5602Title => GetString(nameof(RH5602Title));

    /// <summary>
    /// Gets the localized string for RH5028MessageFormat
    /// </summary>
    internal static string RH5028MessageFormat => GetString(nameof(RH5028MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5028Title
    /// </summary>
    internal static string RH5028Title => GetString(nameof(RH5028Title));

    /// <summary>
    /// Gets the localized string for RH5603MessageFormat
    /// </summary>
    internal static string RH5603MessageFormat => GetString(nameof(RH5603MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5603Title
    /// </summary>
    internal static string RH5603Title => GetString(nameof(RH5603Title));

    /// <summary>
    /// Gets the localized string for RH5103MessageFormat
    /// </summary>
    internal static string RH5103MessageFormat => GetString(nameof(RH5103MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5103Title
    /// </summary>
    internal static string RH5103Title => GetString(nameof(RH5103Title));

    /// <summary>
    /// Gets the localized string for RH7101MessageFormat
    /// </summary>
    internal static string RH7101MessageFormat => GetString(nameof(RH7101MessageFormat));

    /// <summary>
    /// Gets the localized string for RH7101Title
    /// </summary>
    internal static string RH7101Title => GetString(nameof(RH7101Title));

    /// <summary>
    /// Gets the localized string for RH7303MessageFormat
    /// </summary>
    internal static string RH7303MessageFormat => GetString(nameof(RH7303MessageFormat));

    /// <summary>
    /// Gets the localized string for RH7303Title
    /// </summary>
    internal static string RH7303Title => GetString(nameof(RH7303Title));

    /// <summary>
    /// Gets the localized string for RH5110MessageFormat
    /// </summary>
    internal static string RH5110MessageFormat => GetString(nameof(RH5110MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5110Title
    /// </summary>
    internal static string RH5110Title => GetString(nameof(RH5110Title));

    /// <summary>
    /// Gets the localized string for RH5604MessageFormat
    /// </summary>
    internal static string RH5604MessageFormat => GetString(nameof(RH5604MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5604Title
    /// </summary>
    internal static string RH5604Title => GetString(nameof(RH5604Title));

    /// <summary>
    /// Gets the localized string for RH7304MessageFormat
    /// </summary>
    internal static string RH7304MessageFormat => GetString(nameof(RH7304MessageFormat));

    /// <summary>
    /// Gets the localized string for RH7304Title
    /// </summary>
    internal static string RH7304Title => GetString(nameof(RH7304Title));

    /// <summary>
    /// Gets the localized string for RH7305MessageFormat
    /// </summary>
    internal static string RH7305MessageFormat => GetString(nameof(RH7305MessageFormat));

    /// <summary>
    /// Gets the localized string for RH7305Title
    /// </summary>
    internal static string RH7305Title => GetString(nameof(RH7305Title));

    /// <summary>
    /// Gets the localized string for RH7305AMessageFormat
    /// </summary>
    internal static string RH7305AMessageFormat => GetString(nameof(RH7305AMessageFormat));

    /// <summary>
    /// Gets the localized string for RH7305ATitle
    /// </summary>
    internal static string RH7305ATitle => GetString(nameof(RH7305ATitle));

    /// <summary>
    /// Gets the localized string for RH7306MessageFormat
    /// </summary>
    internal static string RH7306MessageFormat => GetString(nameof(RH7306MessageFormat));

    /// <summary>
    /// Gets the localized string for RH7306Title
    /// </summary>
    internal static string RH7306Title => GetString(nameof(RH7306Title));

    /// <summary>
    /// Gets the localized string for RH5204MessageFormat
    /// </summary>
    internal static string RH5204MessageFormat => GetString(nameof(RH5204MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5204Title
    /// </summary>
    internal static string RH5204Title => GetString(nameof(RH5204Title));

    /// <summary>
    /// Gets the localized string for RH7207MessageFormat
    /// </summary>
    internal static string RH7207MessageFormat => GetString(nameof(RH7207MessageFormat));

    /// <summary>
    /// Gets the localized string for RH7207Title
    /// </summary>
    internal static string RH7207Title => GetString(nameof(RH7207Title));

    /// <summary>
    /// Gets the localized string for RH5111MessageFormat
    /// </summary>
    internal static string RH5111MessageFormat => GetString(nameof(RH5111MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5111Title
    /// </summary>
    internal static string RH5111Title => GetString(nameof(RH5111Title));

    /// <summary>
    /// Gets the localized string for RH5205MessageFormat
    /// </summary>
    internal static string RH5205MessageFormat => GetString(nameof(RH5205MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5205Title
    /// </summary>
    internal static string RH5205Title => GetString(nameof(RH5205Title));

    /// <summary>
    /// Gets the localized string for RH5029MessageFormat
    /// </summary>
    internal static string RH5029MessageFormat => GetString(nameof(RH5029MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5029Title
    /// </summary>
    internal static string RH5029Title => GetString(nameof(RH5029Title));

    /// <summary>
    /// Gets the localized string for RH5030MessageFormat
    /// </summary>
    internal static string RH5030MessageFormat => GetString(nameof(RH5030MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5030Title
    /// </summary>
    internal static string RH5030Title => GetString(nameof(RH5030Title));

    /// <summary>
    /// Gets the localized string for RH5031MessageFormat
    /// </summary>
    internal static string RH5031MessageFormat => GetString(nameof(RH5031MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5031Title
    /// </summary>
    internal static string RH5031Title => GetString(nameof(RH5031Title));

    /// <summary>
    /// Gets the localized string for RH5032MessageFormat
    /// </summary>
    internal static string RH5032MessageFormat => GetString(nameof(RH5032MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5032Title
    /// </summary>
    internal static string RH5032Title => GetString(nameof(RH5032Title));

    /// <summary>
    /// Gets the localized string for RH7501MessageFormat
    /// </summary>
    internal static string RH7501MessageFormat => GetString(nameof(RH7501MessageFormat));

    /// <summary>
    /// Gets the localized string for RH7501Title
    /// </summary>
    internal static string RH7501Title => GetString(nameof(RH7501Title));

    /// <summary>
    /// Gets the localized string for RH7004MessageFormat
    /// </summary>
    internal static string RH7004MessageFormat => GetString(nameof(RH7004MessageFormat));

    /// <summary>
    /// Gets the localized string for RH7004Title
    /// </summary>
    internal static string RH7004Title => GetString(nameof(RH7004Title));

    /// <summary>
    /// Gets the localized string for RH7307MessageFormat
    /// </summary>
    internal static string RH7307MessageFormat => GetString(nameof(RH7307MessageFormat));

    /// <summary>
    /// Gets the localized string for RH7307Title
    /// </summary>
    internal static string RH7307Title => GetString(nameof(RH7307Title));

    /// <summary>
    /// Gets the localized string for RH6001MessageFormat
    /// </summary>
    internal static string RH6001MessageFormat => GetString(nameof(RH6001MessageFormat));

    /// <summary>
    /// Gets the localized string for RH6001Title
    /// </summary>
    internal static string RH6001Title => GetString(nameof(RH6001Title));

    /// <summary>
    /// Gets the localized string for RH6002MessageFormat
    /// </summary>
    internal static string RH6002MessageFormat => GetString(nameof(RH6002MessageFormat));

    /// <summary>
    /// Gets the localized string for RH6002Title
    /// </summary>
    internal static string RH6002Title => GetString(nameof(RH6002Title));

    /// <summary>
    /// Gets the localized string for RH6003MessageFormat
    /// </summary>
    internal static string RH6003MessageFormat => GetString(nameof(RH6003MessageFormat));

    /// <summary>
    /// Gets the localized string for RH6003Title
    /// </summary>
    internal static string RH6003Title => GetString(nameof(RH6003Title));

    /// <summary>
    /// Gets the localized string for RH8301MessageFormat
    /// </summary>
    internal static string RH8301MessageFormat => GetString(nameof(RH8301MessageFormat));

    /// <summary>
    /// Gets the localized string for RH8301Title
    /// </summary>
    internal static string RH8301Title => GetString(nameof(RH8301Title));

    /// <summary>
    /// Gets the localized string for RH6004MessageFormat
    /// </summary>
    internal static string RH6004MessageFormat => GetString(nameof(RH6004MessageFormat));

    /// <summary>
    /// Gets the localized string for RH6004Title
    /// </summary>
    internal static string RH6004Title => GetString(nameof(RH6004Title));

    /// <summary>
    /// Gets the localized string for RH6005MessageFormat
    /// </summary>
    internal static string RH6005MessageFormat => GetString(nameof(RH6005MessageFormat));

    /// <summary>
    /// Gets the localized string for RH6005Title
    /// </summary>
    internal static string RH6005Title => GetString(nameof(RH6005Title));

    /// <summary>
    /// Gets the localized string for RH6006MessageFormat
    /// </summary>
    internal static string RH6006MessageFormat => GetString(nameof(RH6006MessageFormat));

    /// <summary>
    /// Gets the localized string for RH6006Title
    /// </summary>
    internal static string RH6006Title => GetString(nameof(RH6006Title));

    /// <summary>
    /// Gets the localized string for RH6007MessageFormat
    /// </summary>
    internal static string RH6007MessageFormat => GetString(nameof(RH6007MessageFormat));

    /// <summary>
    /// Gets the localized string for RH6007Title
    /// </summary>
    internal static string RH6007Title => GetString(nameof(RH6007Title));

    /// <summary>
    /// Gets the localized string for RH6008MessageFormat
    /// </summary>
    internal static string RH6008MessageFormat => GetString(nameof(RH6008MessageFormat));

    /// <summary>
    /// Gets the localized string for RH6008Title
    /// </summary>
    internal static string RH6008Title => GetString(nameof(RH6008Title));

    /// <summary>
    /// Gets the localized string for RH6009MessageFormat
    /// </summary>
    internal static string RH6009MessageFormat => GetString(nameof(RH6009MessageFormat));

    /// <summary>
    /// Gets the localized string for RH6009Title
    /// </summary>
    internal static string RH6009Title => GetString(nameof(RH6009Title));

    /// <summary>
    /// Gets the localized string for RH6010MessageFormat
    /// </summary>
    internal static string RH6010MessageFormat => GetString(nameof(RH6010MessageFormat));

    /// <summary>
    /// Gets the localized string for RH6010Title
    /// </summary>
    internal static string RH6010Title => GetString(nameof(RH6010Title));

    /// <summary>
    /// Gets the localized string for RH6011MessageFormat
    /// </summary>
    internal static string RH6011MessageFormat => GetString(nameof(RH6011MessageFormat));

    /// <summary>
    /// Gets the localized string for RH6011Title
    /// </summary>
    internal static string RH6011Title => GetString(nameof(RH6011Title));

    /// <summary>
    /// Gets the localized string for RH6012MessageFormat
    /// </summary>
    internal static string RH6012MessageFormat => GetString(nameof(RH6012MessageFormat));

    /// <summary>
    /// Gets the localized string for RH6012Title
    /// </summary>
    internal static string RH6012Title => GetString(nameof(RH6012Title));

    /// <summary>
    /// Gets the localized string for RH6013MessageFormat
    /// </summary>
    internal static string RH6013MessageFormat => GetString(nameof(RH6013MessageFormat));

    /// <summary>
    /// Gets the localized string for RH6013Title
    /// </summary>
    internal static string RH6013Title => GetString(nameof(RH6013Title));

    /// <summary>
    /// Gets the localized string for RH6014MessageFormat
    /// </summary>
    internal static string RH6014MessageFormat => GetString(nameof(RH6014MessageFormat));

    /// <summary>
    /// Gets the localized string for RH6014Title
    /// </summary>
    internal static string RH6014Title => GetString(nameof(RH6014Title));

    /// <summary>
    /// Gets the localized string for RH6015MessageFormat
    /// </summary>
    internal static string RH6015MessageFormat => GetString(nameof(RH6015MessageFormat));

    /// <summary>
    /// Gets the localized string for RH6015Title
    /// </summary>
    internal static string RH6015Title => GetString(nameof(RH6015Title));

    /// <summary>
    /// Gets the localized string for RH6016MessageFormat
    /// </summary>
    internal static string RH6016MessageFormat => GetString(nameof(RH6016MessageFormat));

    /// <summary>
    /// Gets the localized string for RH6016Title
    /// </summary>
    internal static string RH6016Title => GetString(nameof(RH6016Title));

    /// <summary>
    /// Gets the localized string for RH6017MessageFormat
    /// </summary>
    internal static string RH6017MessageFormat => GetString(nameof(RH6017MessageFormat));

    /// <summary>
    /// Gets the localized string for RH6017Title
    /// </summary>
    internal static string RH6017Title => GetString(nameof(RH6017Title));

    /// <summary>
    /// Gets the localized string for RH6018MessageFormat
    /// </summary>
    internal static string RH6018MessageFormat => GetString(nameof(RH6018MessageFormat));

    /// <summary>
    /// Gets the localized string for RH6018Title
    /// </summary>
    internal static string RH6018Title => GetString(nameof(RH6018Title));

    /// <summary>
    /// Gets the localized string for RH6019MessageFormat
    /// </summary>
    internal static string RH6019MessageFormat => GetString(nameof(RH6019MessageFormat));

    /// <summary>
    /// Gets the localized string for RH6019Title
    /// </summary>
    internal static string RH6019Title => GetString(nameof(RH6019Title));

    /// <summary>
    /// Gets the localized string for RH6020MessageFormat
    /// </summary>
    internal static string RH6020MessageFormat => GetString(nameof(RH6020MessageFormat));

    /// <summary>
    /// Gets the localized string for RH6020Title
    /// </summary>
    internal static string RH6020Title => GetString(nameof(RH6020Title));

    /// <summary>
    /// Gets the localized string for RH6021MessageFormat
    /// </summary>
    internal static string RH6021MessageFormat => GetString(nameof(RH6021MessageFormat));

    /// <summary>
    /// Gets the localized string for RH6021Title
    /// </summary>
    internal static string RH6021Title => GetString(nameof(RH6021Title));

    /// <summary>
    /// Gets the localized string for RH6022MessageFormat
    /// </summary>
    internal static string RH6022MessageFormat => GetString(nameof(RH6022MessageFormat));

    /// <summary>
    /// Gets the localized string for RH6022Title
    /// </summary>
    internal static string RH6022Title => GetString(nameof(RH6022Title));

    /// <summary>
    /// Gets the localized string for RH5601MessageFormat
    /// </summary>
    internal static string RH5601MessageFormat => GetString(nameof(RH5601MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5601Title
    /// </summary>
    internal static string RH5601Title => GetString(nameof(RH5601Title));

    /// <summary>
    /// Gets the localized string for RH5402MessageFormat
    /// </summary>
    internal static string RH5402MessageFormat => GetString(nameof(RH5402MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5402Title
    /// </summary>
    internal static string RH5402Title => GetString(nameof(RH5402Title));

    /// <summary>
    /// Gets the localized string for RH5403MessageFormat
    /// </summary>
    internal static string RH5403MessageFormat => GetString(nameof(RH5403MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5403Title
    /// </summary>
    internal static string RH5403Title => GetString(nameof(RH5403Title));

    /// <summary>
    /// Gets the localized string for RH5404MessageFormat
    /// </summary>
    internal static string RH5404MessageFormat => GetString(nameof(RH5404MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5404Title
    /// </summary>
    internal static string RH5404Title => GetString(nameof(RH5404Title));

    /// <summary>
    /// Gets the localized string for RH5405MessageFormat
    /// </summary>
    internal static string RH5405MessageFormat => GetString(nameof(RH5405MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5405Title
    /// </summary>
    internal static string RH5405Title => GetString(nameof(RH5405Title));

    /// <summary>
    /// Gets the localized string for RH5022MessageFormat
    /// </summary>
    internal static string RH5022MessageFormat => GetString(nameof(RH5022MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5022Title
    /// </summary>
    internal static string RH5022Title => GetString(nameof(RH5022Title));

    /// <summary>
    /// Gets the localized string for RH8302MessageFormat
    /// </summary>
    internal static string RH8302MessageFormat => GetString(nameof(RH8302MessageFormat));

    /// <summary>
    /// Gets the localized string for RH8302Title
    /// </summary>
    internal static string RH8302Title => GetString(nameof(RH8302Title));

    /// <summary>
    /// Gets the localized string for RH5023MessageFormat
    /// </summary>
    internal static string RH5023MessageFormat => GetString(nameof(RH5023MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5023Title
    /// </summary>
    internal static string RH5023Title => GetString(nameof(RH5023Title));

    /// <summary>
    /// Gets the localized string for RH5024MessageFormat
    /// </summary>
    internal static string RH5024MessageFormat => GetString(nameof(RH5024MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5024Title
    /// </summary>
    internal static string RH5024Title => GetString(nameof(RH5024Title));

    /// <summary>
    /// Gets the localized string for RH5025MessageFormat
    /// </summary>
    internal static string RH5025MessageFormat => GetString(nameof(RH5025MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5025Title
    /// </summary>
    internal static string RH5025Title => GetString(nameof(RH5025Title));

    /// <summary>
    /// Gets the localized string for RH5026MessageFormat
    /// </summary>
    internal static string RH5026MessageFormat => GetString(nameof(RH5026MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5026Title
    /// </summary>
    internal static string RH5026Title => GetString(nameof(RH5026Title));

    /// <summary>
    /// Gets the localized string for RH5027MessageFormat
    /// </summary>
    internal static string RH5027MessageFormat => GetString(nameof(RH5027MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5027Title
    /// </summary>
    internal static string RH5027Title => GetString(nameof(RH5027Title));

    /// <summary>
    /// Gets the localized string for RH8303MessageFormat
    /// </summary>
    internal static string RH8303MessageFormat => GetString(nameof(RH8303MessageFormat));

    /// <summary>
    /// Gets the localized string for RH8303Title
    /// </summary>
    internal static string RH8303Title => GetString(nameof(RH8303Title));

    /// <summary>
    /// Gets the localized string for RH5406MessageFormat
    /// </summary>
    internal static string RH5406MessageFormat => GetString(nameof(RH5406MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5406Title
    /// </summary>
    internal static string RH5406Title => GetString(nameof(RH5406Title));

    /// <summary>
    /// Gets the localized string for RH5407MessageFormat
    /// </summary>
    internal static string RH5407MessageFormat => GetString(nameof(RH5407MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5407Title
    /// </summary>
    internal static string RH5407Title => GetString(nameof(RH5407Title));

    /// <summary>
    /// Gets the localized string for RH5104MessageFormat
    /// </summary>
    internal static string RH5104MessageFormat => GetString(nameof(RH5104MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5104Title
    /// </summary>
    internal static string RH5104Title => GetString(nameof(RH5104Title));

    /// <summary>
    /// Gets the localized string for RH5105MessageFormat
    /// </summary>
    internal static string RH5105MessageFormat => GetString(nameof(RH5105MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5105Title
    /// </summary>
    internal static string RH5105Title => GetString(nameof(RH5105Title));

    /// <summary>
    /// Gets the localized string for RH5106MessageFormat
    /// </summary>
    internal static string RH5106MessageFormat => GetString(nameof(RH5106MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5106Title
    /// </summary>
    internal static string RH5106Title => GetString(nameof(RH5106Title));

    /// <summary>
    /// Gets the localized string for RH5107MessageFormat
    /// </summary>
    internal static string RH5107MessageFormat => GetString(nameof(RH5107MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5107Title
    /// </summary>
    internal static string RH5107Title => GetString(nameof(RH5107Title));

    /// <summary>
    /// Gets the localized string for RH5108MessageFormat
    /// </summary>
    internal static string RH5108MessageFormat => GetString(nameof(RH5108MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5108Title
    /// </summary>
    internal static string RH5108Title => GetString(nameof(RH5108Title));

    /// <summary>
    /// Gets the localized string for RH5109MessageFormat
    /// </summary>
    internal static string RH5109MessageFormat => GetString(nameof(RH5109MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5109Title
    /// </summary>
    internal static string RH5109Title => GetString(nameof(RH5109Title));

    /// <summary>
    /// Gets the localized string for RH7102Title
    /// </summary>
    internal static string RH7102Title => GetString(nameof(RH7102Title));

    /// <summary>
    /// Gets the localized string for RH7102MessageFormat
    /// </summary>
    internal static string RH7102MessageFormat => GetString(nameof(RH7102MessageFormat));

    /// <summary>
    /// Gets the localized string for RH7103Title
    /// </summary>
    internal static string RH7103Title => GetString(nameof(RH7103Title));

    /// <summary>
    /// Gets the localized string for RH7103MessageFormat
    /// </summary>
    internal static string RH7103MessageFormat => GetString(nameof(RH7103MessageFormat));

    /// <summary>
    /// Gets the localized string for RH7104Title
    /// </summary>
    internal static string RH7104Title => GetString(nameof(RH7104Title));

    /// <summary>
    /// Gets the localized string for RH7104MessageFormat
    /// </summary>
    internal static string RH7104MessageFormat => GetString(nameof(RH7104MessageFormat));

    /// <summary>
    /// Gets the localized string for RH7105Title
    /// </summary>
    internal static string RH7105Title => GetString(nameof(RH7105Title));

    /// <summary>
    /// Gets the localized string for RH7105MessageFormat
    /// </summary>
    internal static string RH7105MessageFormat => GetString(nameof(RH7105MessageFormat));

    /// <summary>
    /// Gets the localized string for RH7106Title
    /// </summary>
    internal static string RH7106Title => GetString(nameof(RH7106Title));

    /// <summary>
    /// Gets the localized string for RH7106MessageFormat
    /// </summary>
    internal static string RH7106MessageFormat => GetString(nameof(RH7106MessageFormat));

    /// <summary>
    /// Gets the localized string for RH7201Title
    /// </summary>
    internal static string RH7201Title => GetString(nameof(RH7201Title));

    /// <summary>
    /// Gets the localized string for RH7201MessageFormat
    /// </summary>
    internal static string RH7201MessageFormat => GetString(nameof(RH7201MessageFormat));

    /// <summary>
    /// Gets the localized string for RH7202Title
    /// </summary>
    internal static string RH7202Title => GetString(nameof(RH7202Title));

    /// <summary>
    /// Gets the localized string for RH7202MessageFormat
    /// </summary>
    internal static string RH7202MessageFormat => GetString(nameof(RH7202MessageFormat));

    /// <summary>
    /// Gets the localized string for RH7203Title
    /// </summary>
    internal static string RH7203Title => GetString(nameof(RH7203Title));

    /// <summary>
    /// Gets the localized string for RH7203MessageFormat
    /// </summary>
    internal static string RH7203MessageFormat => GetString(nameof(RH7203MessageFormat));

    /// <summary>
    /// Gets the localized string for RH7204Title
    /// </summary>
    internal static string RH7204Title => GetString(nameof(RH7204Title));

    /// <summary>
    /// Gets the localized string for RH7204MessageFormat
    /// </summary>
    internal static string RH7204MessageFormat => GetString(nameof(RH7204MessageFormat));

    /// <summary>
    /// Gets the localized string for RH7107Title
    /// </summary>
    internal static string RH7107Title => GetString(nameof(RH7107Title));

    /// <summary>
    /// Gets the localized string for RH7107MessageFormat
    /// </summary>
    internal static string RH7107MessageFormat => GetString(nameof(RH7107MessageFormat));

    /// <summary>
    /// Gets the localized string for RH7108Title
    /// </summary>
    internal static string RH7108Title => GetString(nameof(RH7108Title));

    /// <summary>
    /// Gets the localized string for RH7108MessageFormat
    /// </summary>
    internal static string RH7108MessageFormat => GetString(nameof(RH7108MessageFormat));

    /// <summary>
    /// Gets the localized string for RH7109Title
    /// </summary>
    internal static string RH7109Title => GetString(nameof(RH7109Title));

    /// <summary>
    /// Gets the localized string for RH7109MessageFormat
    /// </summary>
    internal static string RH7109MessageFormat => GetString(nameof(RH7109MessageFormat));

    /// <summary>
    /// Gets the localized string for RH7205Title
    /// </summary>
    internal static string RH7205Title => GetString(nameof(RH7205Title));

    /// <summary>
    /// Gets the localized string for RH7205MessageFormat
    /// </summary>
    internal static string RH7205MessageFormat => GetString(nameof(RH7205MessageFormat));

    /// <summary>
    /// Gets the localized string for RH7206Title
    /// </summary>
    internal static string RH7206Title => GetString(nameof(RH7206Title));

    /// <summary>
    /// Gets the localized string for RH7206MessageFormat
    /// </summary>
    internal static string RH7206MessageFormat => GetString(nameof(RH7206MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0001Title
    /// </summary>
    internal static string RH0001Title => GetString(nameof(RH0001Title));

    /// <summary>
    /// Gets the localized string for RH0001MessageFormat
    /// </summary>
    internal static string RH0001MessageFormat => GetString(nameof(RH0001MessageFormat));

    /// <summary>
    /// Gets the localized string for RH0002Title
    /// </summary>
    internal static string RH0002Title => GetString(nameof(RH0002Title));

    /// <summary>
    /// Gets the localized string for RH0002MessageFormat
    /// </summary>
    internal static string RH0002MessageFormat => GetString(nameof(RH0002MessageFormat));

    /// <summary>
    /// Gets the localized string for RH8402Title
    /// </summary>
    internal static string RH8402Title => GetString(nameof(RH8402Title));

    /// <summary>
    /// Gets the localized string for RH8402MessageFormat
    /// </summary>
    internal static string RH8402MessageFormat => GetString(nameof(RH8402MessageFormat));

    /// <summary>
    /// Gets the localized string for RH8031Title
    /// </summary>
    internal static string RH8031Title => GetString(nameof(RH8031Title));

    /// <summary>
    /// Gets the localized string for RH8031MessageFormat
    /// </summary>
    internal static string RH8031MessageFormat => GetString(nameof(RH8031MessageFormat));

    /// <summary>
    /// Gets the localized string for RH8032Title
    /// </summary>
    internal static string RH8032Title => GetString(nameof(RH8032Title));

    /// <summary>
    /// Gets the localized string for RH8032MessageFormat
    /// </summary>
    internal static string RH8032MessageFormat => GetString(nameof(RH8032MessageFormat));

    /// <summary>
    /// Gets the localized string for RH8033Title
    /// </summary>
    internal static string RH8033Title => GetString(nameof(RH8033Title));

    /// <summary>
    /// Gets the localized string for RH8033MessageFormat
    /// </summary>
    internal static string RH8033MessageFormat => GetString(nameof(RH8033MessageFormat));

    /// <summary>
    /// Gets the localized string for RH8001MessageFormat
    /// </summary>
    internal static string RH8001MessageFormat => GetString(nameof(RH8001MessageFormat));

    /// <summary>
    /// Gets the localized string for RH8001Title
    /// </summary>
    internal static string RH8001Title => GetString(nameof(RH8001Title));

    /// <summary>
    /// Gets the localized string for RH8002MessageFormat
    /// </summary>
    internal static string RH8002MessageFormat => GetString(nameof(RH8002MessageFormat));

    /// <summary>
    /// Gets the localized string for RH8002Title
    /// </summary>
    internal static string RH8002Title => GetString(nameof(RH8002Title));

    /// <summary>
    /// Gets the localized string for RH8003MessageFormat
    /// </summary>
    internal static string RH8003MessageFormat => GetString(nameof(RH8003MessageFormat));

    /// <summary>
    /// Gets the localized string for RH8003Title
    /// </summary>
    internal static string RH8003Title => GetString(nameof(RH8003Title));

    /// <summary>
    /// Gets the localized string for RH8004MessageFormat
    /// </summary>
    internal static string RH8004MessageFormat => GetString(nameof(RH8004MessageFormat));

    /// <summary>
    /// Gets the localized string for RH8004Title
    /// </summary>
    internal static string RH8004Title => GetString(nameof(RH8004Title));

    /// <summary>
    /// Gets the localized string for RH8005MessageFormat
    /// </summary>
    internal static string RH8005MessageFormat => GetString(nameof(RH8005MessageFormat));

    /// <summary>
    /// Gets the localized string for RH8005Title
    /// </summary>
    internal static string RH8005Title => GetString(nameof(RH8005Title));

    /// <summary>
    /// Gets the localized string for RH8006MessageFormat
    /// </summary>
    internal static string RH8006MessageFormat => GetString(nameof(RH8006MessageFormat));

    /// <summary>
    /// Gets the localized string for RH8006Title
    /// </summary>
    internal static string RH8006Title => GetString(nameof(RH8006Title));

    /// <summary>
    /// Gets the localized string for RH8007MessageFormat
    /// </summary>
    internal static string RH8007MessageFormat => GetString(nameof(RH8007MessageFormat));

    /// <summary>
    /// Gets the localized string for RH8007Title
    /// </summary>
    internal static string RH8007Title => GetString(nameof(RH8007Title));

    /// <summary>
    /// Gets the localized string for RH8008MessageFormat
    /// </summary>
    internal static string RH8008MessageFormat => GetString(nameof(RH8008MessageFormat));

    /// <summary>
    /// Gets the localized string for RH8008Title
    /// </summary>
    internal static string RH8008Title => GetString(nameof(RH8008Title));

    /// <summary>
    /// Gets the localized string for RH8009MessageFormat
    /// </summary>
    internal static string RH8009MessageFormat => GetString(nameof(RH8009MessageFormat));

    /// <summary>
    /// Gets the localized string for RH8009Title
    /// </summary>
    internal static string RH8009Title => GetString(nameof(RH8009Title));

    /// <summary>
    /// Gets the localized string for RH8010MessageFormat
    /// </summary>
    internal static string RH8010MessageFormat => GetString(nameof(RH8010MessageFormat));

    /// <summary>
    /// Gets the localized string for RH8010Title
    /// </summary>
    internal static string RH8010Title => GetString(nameof(RH8010Title));

    /// <summary>
    /// Gets the localized string for RH8011MessageFormat
    /// </summary>
    internal static string RH8011MessageFormat => GetString(nameof(RH8011MessageFormat));

    /// <summary>
    /// Gets the localized string for RH8011Title
    /// </summary>
    internal static string RH8011Title => GetString(nameof(RH8011Title));

    /// <summary>
    /// Gets the localized string for RH8012MessageFormat
    /// </summary>
    internal static string RH8012MessageFormat => GetString(nameof(RH8012MessageFormat));

    /// <summary>
    /// Gets the localized string for RH8012Title
    /// </summary>
    internal static string RH8012Title => GetString(nameof(RH8012Title));

    /// <summary>
    /// Gets the localized string for RH8013MessageFormat
    /// </summary>
    internal static string RH8013MessageFormat => GetString(nameof(RH8013MessageFormat));

    /// <summary>
    /// Gets the localized string for RH8013Title
    /// </summary>
    internal static string RH8013Title => GetString(nameof(RH8013Title));

    /// <summary>
    /// Gets the localized string for RH8014MessageFormat
    /// </summary>
    internal static string RH8014MessageFormat => GetString(nameof(RH8014MessageFormat));

    /// <summary>
    /// Gets the localized string for RH8014Title
    /// </summary>
    internal static string RH8014Title => GetString(nameof(RH8014Title));

    /// <summary>
    /// Gets the localized string for RH8015MessageFormat
    /// </summary>
    internal static string RH8015MessageFormat => GetString(nameof(RH8015MessageFormat));

    /// <summary>
    /// Gets the localized string for RH8015Title
    /// </summary>
    internal static string RH8015Title => GetString(nameof(RH8015Title));

    /// <summary>
    /// Gets the localized string for RH8016MessageFormat
    /// </summary>
    internal static string RH8016MessageFormat => GetString(nameof(RH8016MessageFormat));

    /// <summary>
    /// Gets the localized string for RH8016Title
    /// </summary>
    internal static string RH8016Title => GetString(nameof(RH8016Title));

    /// <summary>
    /// Gets the localized string for RH8017MessageFormat
    /// </summary>
    internal static string RH8017MessageFormat => GetString(nameof(RH8017MessageFormat));

    /// <summary>
    /// Gets the localized string for RH8017Title
    /// </summary>
    internal static string RH8017Title => GetString(nameof(RH8017Title));

    /// <summary>
    /// Gets the localized string for RH8018MessageFormat
    /// </summary>
    internal static string RH8018MessageFormat => GetString(nameof(RH8018MessageFormat));

    /// <summary>
    /// Gets the localized string for RH8018Title
    /// </summary>
    internal static string RH8018Title => GetString(nameof(RH8018Title));

    /// <summary>
    /// Gets the localized string for RH8019MessageFormat
    /// </summary>
    internal static string RH8019MessageFormat => GetString(nameof(RH8019MessageFormat));

    /// <summary>
    /// Gets the localized string for RH8019Title
    /// </summary>
    internal static string RH8019Title => GetString(nameof(RH8019Title));

    /// <summary>
    /// Gets the localized string for RH8020MessageFormat
    /// </summary>
    internal static string RH8020MessageFormat => GetString(nameof(RH8020MessageFormat));

    /// <summary>
    /// Gets the localized string for RH8020Title
    /// </summary>
    internal static string RH8020Title => GetString(nameof(RH8020Title));

    /// <summary>
    /// Gets the localized string for RH8021MessageFormat
    /// </summary>
    internal static string RH8021MessageFormat => GetString(nameof(RH8021MessageFormat));

    /// <summary>
    /// Gets the localized string for RH8021Title
    /// </summary>
    internal static string RH8021Title => GetString(nameof(RH8021Title));

    /// <summary>
    /// Gets the localized string for RH8022MessageFormat
    /// </summary>
    internal static string RH8022MessageFormat => GetString(nameof(RH8022MessageFormat));

    /// <summary>
    /// Gets the localized string for RH8022Title
    /// </summary>
    internal static string RH8022Title => GetString(nameof(RH8022Title));

    /// <summary>
    /// Gets the localized string for RH8023MessageFormat
    /// </summary>
    internal static string RH8023MessageFormat => GetString(nameof(RH8023MessageFormat));

    /// <summary>
    /// Gets the localized string for RH8023Title
    /// </summary>
    internal static string RH8023Title => GetString(nameof(RH8023Title));

    /// <summary>
    /// Gets the localized string for RH8024MessageFormat
    /// </summary>
    internal static string RH8024MessageFormat => GetString(nameof(RH8024MessageFormat));

    /// <summary>
    /// Gets the localized string for RH8024Title
    /// </summary>
    internal static string RH8024Title => GetString(nameof(RH8024Title));

    /// <summary>
    /// Gets the localized string for RH8025MessageFormat
    /// </summary>
    internal static string RH8025MessageFormat => GetString(nameof(RH8025MessageFormat));

    /// <summary>
    /// Gets the localized string for RH8025Title
    /// </summary>
    internal static string RH8025Title => GetString(nameof(RH8025Title));

    /// <summary>
    /// Gets the localized string for RH8026MessageFormat
    /// </summary>
    internal static string RH8026MessageFormat => GetString(nameof(RH8026MessageFormat));

    /// <summary>
    /// Gets the localized string for RH8026Title
    /// </summary>
    internal static string RH8026Title => GetString(nameof(RH8026Title));

    /// <summary>
    /// Gets the localized string for RH8027MessageFormat
    /// </summary>
    internal static string RH8027MessageFormat => GetString(nameof(RH8027MessageFormat));

    /// <summary>
    /// Gets the localized string for RH8027Title
    /// </summary>
    internal static string RH8027Title => GetString(nameof(RH8027Title));

    /// <summary>
    /// Gets the localized string for RH8028MessageFormat
    /// </summary>
    internal static string RH8028MessageFormat => GetString(nameof(RH8028MessageFormat));

    /// <summary>
    /// Gets the localized string for RH8028Title
    /// </summary>
    internal static string RH8028Title => GetString(nameof(RH8028Title));

    /// <summary>
    /// Gets the localized string for RH8029MessageFormat
    /// </summary>
    internal static string RH8029MessageFormat => GetString(nameof(RH8029MessageFormat));

    /// <summary>
    /// Gets the localized string for RH8029Title
    /// </summary>
    internal static string RH8029Title => GetString(nameof(RH8029Title));

    /// <summary>
    /// Gets the localized string for RH8030MessageFormat
    /// </summary>
    internal static string RH8030MessageFormat => GetString(nameof(RH8030MessageFormat));

    /// <summary>
    /// Gets the localized string for RH8030Title
    /// </summary>
    internal static string RH8030Title => GetString(nameof(RH8030Title));

    /// <summary>
    /// Gets the localized string for RH8202MessageFormat
    /// </summary>
    internal static string RH8202MessageFormat => GetString(nameof(RH8202MessageFormat));

    /// <summary>
    /// Gets the localized string for RH8202Title
    /// </summary>
    internal static string RH8202Title => GetString(nameof(RH8202Title));

    /// <summary>
    /// Gets the localized string for RH8101MessageFormat
    /// </summary>
    internal static string RH8101MessageFormat => GetString(nameof(RH8101MessageFormat));

    /// <summary>
    /// Gets the localized string for RH8101Title
    /// </summary>
    internal static string RH8101Title => GetString(nameof(RH8101Title));

    /// <summary>
    /// Gets the localized string for RH8102MessageFormat
    /// </summary>
    internal static string RH8102MessageFormat => GetString(nameof(RH8102MessageFormat));

    /// <summary>
    /// Gets the localized string for RH8102Title
    /// </summary>
    internal static string RH8102Title => GetString(nameof(RH8102Title));

    /// <summary>
    /// Gets the localized string for RH8103MessageFormat
    /// </summary>
    internal static string RH8103MessageFormat => GetString(nameof(RH8103MessageFormat));

    /// <summary>
    /// Gets the localized string for RH8103Title
    /// </summary>
    internal static string RH8103Title => GetString(nameof(RH8103Title));

    /// <summary>
    /// Gets the localized string for RH8104MessageFormat
    /// </summary>
    internal static string RH8104MessageFormat => GetString(nameof(RH8104MessageFormat));

    /// <summary>
    /// Gets the localized string for RH8104Title
    /// </summary>
    internal static string RH8104Title => GetString(nameof(RH8104Title));

    /// <summary>
    /// Gets the localized string for RH8105MessageFormat
    /// </summary>
    internal static string RH8105MessageFormat => GetString(nameof(RH8105MessageFormat));

    /// <summary>
    /// Gets the localized string for RH8105Title
    /// </summary>
    internal static string RH8105Title => GetString(nameof(RH8105Title));

    /// <summary>
    /// Gets the localized string for RH8106MessageFormat
    /// </summary>
    internal static string RH8106MessageFormat => GetString(nameof(RH8106MessageFormat));

    /// <summary>
    /// Gets the localized string for RH8106Title
    /// </summary>
    internal static string RH8106Title => GetString(nameof(RH8106Title));

    /// <summary>
    /// Gets the localized string for RH8107MessageFormat
    /// </summary>
    internal static string RH8107MessageFormat => GetString(nameof(RH8107MessageFormat));

    /// <summary>
    /// Gets the localized string for RH8107Title
    /// </summary>
    internal static string RH8107Title => GetString(nameof(RH8107Title));

    /// <summary>
    /// Gets the localized string for RH8108MessageFormat
    /// </summary>
    internal static string RH8108MessageFormat => GetString(nameof(RH8108MessageFormat));

    /// <summary>
    /// Gets the localized string for RH8108Title
    /// </summary>
    internal static string RH8108Title => GetString(nameof(RH8108Title));

    /// <summary>
    /// Gets the localized string for RH8109MessageFormat
    /// </summary>
    internal static string RH8109MessageFormat => GetString(nameof(RH8109MessageFormat));

    /// <summary>
    /// Gets the localized string for RH8109Title
    /// </summary>
    internal static string RH8109Title => GetString(nameof(RH8109Title));

    /// <summary>
    /// Gets the localized string for RH8110MessageFormat
    /// </summary>
    internal static string RH8110MessageFormat => GetString(nameof(RH8110MessageFormat));

    /// <summary>
    /// Gets the localized string for RH8110Title
    /// </summary>
    internal static string RH8110Title => GetString(nameof(RH8110Title));

    /// <summary>
    /// Gets the localized string for RH8111MessageFormat
    /// </summary>
    internal static string RH8111MessageFormat => GetString(nameof(RH8111MessageFormat));

    /// <summary>
    /// Gets the localized string for RH8111Title
    /// </summary>
    internal static string RH8111Title => GetString(nameof(RH8111Title));

    /// <summary>
    /// Gets the localized string for RH8401MessageFormat
    /// </summary>
    internal static string RH8401MessageFormat => GetString(nameof(RH8401MessageFormat));

    /// <summary>
    /// Gets the localized string for RH8401Title
    /// </summary>
    internal static string RH8401Title => GetString(nameof(RH8401Title));

    /// <summary>
    /// Gets the localized string for RH8203MessageFormat
    /// </summary>
    internal static string RH8203MessageFormat => GetString(nameof(RH8203MessageFormat));

    /// <summary>
    /// Gets the localized string for RH8203Title
    /// </summary>
    internal static string RH8203Title => GetString(nameof(RH8203Title));

    /// <summary>
    /// Gets the localized string for RH8204MessageFormat
    /// </summary>
    internal static string RH8204MessageFormat => GetString(nameof(RH8204MessageFormat));

    /// <summary>
    /// Gets the localized string for RH8204Title
    /// </summary>
    internal static string RH8204Title => GetString(nameof(RH8204Title));

    /// <summary>
    /// Gets the localized string for RH8304MessageFormat
    /// </summary>
    internal static string RH8304MessageFormat => GetString(nameof(RH8304MessageFormat));

    /// <summary>
    /// Gets the localized string for RH8304Title
    /// </summary>
    internal static string RH8304Title => GetString(nameof(RH8304Title));

    /// <summary>
    /// Gets the localized string for RH8305MessageFormat
    /// </summary>
    internal static string RH8305MessageFormat => GetString(nameof(RH8305MessageFormat));

    /// <summary>
    /// Gets the localized string for RH8305Title
    /// </summary>
    internal static string RH8305Title => GetString(nameof(RH8305Title));

    /// <summary>
    /// Gets the localized string for RH8306MessageFormat
    /// </summary>
    internal static string RH8306MessageFormat => GetString(nameof(RH8306MessageFormat));

    /// <summary>
    /// Gets the localized string for RH8306Title
    /// </summary>
    internal static string RH8306Title => GetString(nameof(RH8306Title));

    /// <summary>
    /// Gets the localized string for RH8307MessageFormat
    /// </summary>
    internal static string RH8307MessageFormat => GetString(nameof(RH8307MessageFormat));

    /// <summary>
    /// Gets the localized string for RH8307Title
    /// </summary>
    internal static string RH8307Title => GetString(nameof(RH8307Title));

    /// <summary>
    /// Gets the localized string for RH8308MessageFormat
    /// </summary>
    internal static string RH8308MessageFormat => GetString(nameof(RH8308MessageFormat));

    /// <summary>
    /// Gets the localized string for RH8308Title
    /// </summary>
    internal static string RH8308Title => GetString(nameof(RH8308Title));

    /// <summary>
    /// Gets the localized string for RH8201MessageFormat
    /// </summary>
    internal static string RH8201MessageFormat => GetString(nameof(RH8201MessageFormat));

    /// <summary>
    /// Gets the localized string for RH8201Title
    /// </summary>
    internal static string RH8201Title => GetString(nameof(RH8201Title));

    /// <summary>
    /// Gets the localized string for RH1001MessageFormat
    /// </summary>
    internal static string RH1001MessageFormat => GetString(nameof(RH1001MessageFormat));

    /// <summary>
    /// Gets the localized string for RH1001Title
    /// </summary>
    internal static string RH1001Title => GetString(nameof(RH1001Title));

    /// <summary>
    /// Gets the localized string for RH1002MessageFormat
    /// </summary>
    internal static string RH1002MessageFormat => GetString(nameof(RH1002MessageFormat));

    /// <summary>
    /// Gets the localized string for RH1002Title
    /// </summary>
    internal static string RH1002Title => GetString(nameof(RH1002Title));

    /// <summary>
    /// Gets the localized string for RH1003MessageFormat
    /// </summary>
    internal static string RH1003MessageFormat => GetString(nameof(RH1003MessageFormat));

    /// <summary>
    /// Gets the localized string for RH1003Title
    /// </summary>
    internal static string RH1003Title => GetString(nameof(RH1003Title));

    /// <summary>
    /// Gets the localized string for RH5501Title
    /// </summary>
    internal static string RH5501Title => GetString(nameof(RH5501Title));

    /// <summary>
    /// Gets the localized string for RH5501MessageFormat
    /// </summary>
    internal static string RH5501MessageFormat => GetString(nameof(RH5501MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5502Title
    /// </summary>
    internal static string RH5502Title => GetString(nameof(RH5502Title));

    /// <summary>
    /// Gets the localized string for RH5502MessageFormat
    /// </summary>
    internal static string RH5502MessageFormat => GetString(nameof(RH5502MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5503Title
    /// </summary>
    internal static string RH5503Title => GetString(nameof(RH5503Title));

    /// <summary>
    /// Gets the localized string for RH5503MessageFormat
    /// </summary>
    internal static string RH5503MessageFormat => GetString(nameof(RH5503MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5504Title
    /// </summary>
    internal static string RH5504Title => GetString(nameof(RH5504Title));

    /// <summary>
    /// Gets the localized string for RH5504MessageFormat
    /// </summary>
    internal static string RH5504MessageFormat => GetString(nameof(RH5504MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5505Title
    /// </summary>
    internal static string RH5505Title => GetString(nameof(RH5505Title));

    /// <summary>
    /// Gets the localized string for RH5505MessageFormat
    /// </summary>
    internal static string RH5505MessageFormat => GetString(nameof(RH5505MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5506Title
    /// </summary>
    internal static string RH5506Title => GetString(nameof(RH5506Title));

    /// <summary>
    /// Gets the localized string for RH5506MessageFormat
    /// </summary>
    internal static string RH5506MessageFormat => GetString(nameof(RH5506MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5507Title
    /// </summary>
    internal static string RH5507Title => GetString(nameof(RH5507Title));

    /// <summary>
    /// Gets the localized string for RH5507MessageFormat
    /// </summary>
    internal static string RH5507MessageFormat => GetString(nameof(RH5507MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5508Title
    /// </summary>
    internal static string RH5508Title => GetString(nameof(RH5508Title));

    /// <summary>
    /// Gets the localized string for RH5508MessageFormat
    /// </summary>
    internal static string RH5508MessageFormat => GetString(nameof(RH5508MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5509Title
    /// </summary>
    internal static string RH5509Title => GetString(nameof(RH5509Title));

    /// <summary>
    /// Gets the localized string for RH5509MessageFormat
    /// </summary>
    internal static string RH5509MessageFormat => GetString(nameof(RH5509MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5510Title
    /// </summary>
    internal static string RH5510Title => GetString(nameof(RH5510Title));

    /// <summary>
    /// Gets the localized string for RH5510MessageFormat
    /// </summary>
    internal static string RH5510MessageFormat => GetString(nameof(RH5510MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5511Title
    /// </summary>
    internal static string RH5511Title => GetString(nameof(RH5511Title));

    /// <summary>
    /// Gets the localized string for RH5511MessageFormat
    /// </summary>
    internal static string RH5511MessageFormat => GetString(nameof(RH5511MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5512Title
    /// </summary>
    internal static string RH5512Title => GetString(nameof(RH5512Title));

    /// <summary>
    /// Gets the localized string for RH5512MessageFormat
    /// </summary>
    internal static string RH5512MessageFormat => GetString(nameof(RH5512MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5513Title
    /// </summary>
    internal static string RH5513Title => GetString(nameof(RH5513Title));

    /// <summary>
    /// Gets the localized string for RH5513MessageFormat
    /// </summary>
    internal static string RH5513MessageFormat => GetString(nameof(RH5513MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5514Title
    /// </summary>
    internal static string RH5514Title => GetString(nameof(RH5514Title));

    /// <summary>
    /// Gets the localized string for RH5514MessageFormat
    /// </summary>
    internal static string RH5514MessageFormat => GetString(nameof(RH5514MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5515Title
    /// </summary>
    internal static string RH5515Title => GetString(nameof(RH5515Title));

    /// <summary>
    /// Gets the localized string for RH5515MessageFormat
    /// </summary>
    internal static string RH5515MessageFormat => GetString(nameof(RH5515MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5516Title
    /// </summary>
    internal static string RH5516Title => GetString(nameof(RH5516Title));

    /// <summary>
    /// Gets the localized string for RH5516MessageFormat
    /// </summary>
    internal static string RH5516MessageFormat => GetString(nameof(RH5516MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5517Title
    /// </summary>
    internal static string RH5517Title => GetString(nameof(RH5517Title));

    /// <summary>
    /// Gets the localized string for RH5517MessageFormat
    /// </summary>
    internal static string RH5517MessageFormat => GetString(nameof(RH5517MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5518Title
    /// </summary>
    internal static string RH5518Title => GetString(nameof(RH5518Title));

    /// <summary>
    /// Gets the localized string for RH5518MessageFormat
    /// </summary>
    internal static string RH5518MessageFormat => GetString(nameof(RH5518MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5519Title
    /// </summary>
    internal static string RH5519Title => GetString(nameof(RH5519Title));

    /// <summary>
    /// Gets the localized string for RH5519MessageFormat
    /// </summary>
    internal static string RH5519MessageFormat => GetString(nameof(RH5519MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5520Title
    /// </summary>
    internal static string RH5520Title => GetString(nameof(RH5520Title));

    /// <summary>
    /// Gets the localized string for RH5520MessageFormat
    /// </summary>
    internal static string RH5520MessageFormat => GetString(nameof(RH5520MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5521Title
    /// </summary>
    internal static string RH5521Title => GetString(nameof(RH5521Title));

    /// <summary>
    /// Gets the localized string for RH5521MessageFormat
    /// </summary>
    internal static string RH5521MessageFormat => GetString(nameof(RH5521MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5522Title
    /// </summary>
    internal static string RH5522Title => GetString(nameof(RH5522Title));

    /// <summary>
    /// Gets the localized string for RH5522MessageFormat
    /// </summary>
    internal static string RH5522MessageFormat => GetString(nameof(RH5522MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5523Title
    /// </summary>
    internal static string RH5523Title => GetString(nameof(RH5523Title));

    /// <summary>
    /// Gets the localized string for RH5523MessageFormat
    /// </summary>
    internal static string RH5523MessageFormat => GetString(nameof(RH5523MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5524Title
    /// </summary>
    internal static string RH5524Title => GetString(nameof(RH5524Title));

    /// <summary>
    /// Gets the localized string for RH5524MessageFormat
    /// </summary>
    internal static string RH5524MessageFormat => GetString(nameof(RH5524MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5525Title
    /// </summary>
    internal static string RH5525Title => GetString(nameof(RH5525Title));

    /// <summary>
    /// Gets the localized string for RH5525MessageFormat
    /// </summary>
    internal static string RH5525MessageFormat => GetString(nameof(RH5525MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5526Title
    /// </summary>
    internal static string RH5526Title => GetString(nameof(RH5526Title));

    /// <summary>
    /// Gets the localized string for RH5526MessageFormat
    /// </summary>
    internal static string RH5526MessageFormat => GetString(nameof(RH5526MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5527Title
    /// </summary>
    internal static string RH5527Title => GetString(nameof(RH5527Title));

    /// <summary>
    /// Gets the localized string for RH5527MessageFormat
    /// </summary>
    internal static string RH5527MessageFormat => GetString(nameof(RH5527MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5528Title
    /// </summary>
    internal static string RH5528Title => GetString(nameof(RH5528Title));

    /// <summary>
    /// Gets the localized string for RH5528MessageFormat
    /// </summary>
    internal static string RH5528MessageFormat => GetString(nameof(RH5528MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5529Title
    /// </summary>
    internal static string RH5529Title => GetString(nameof(RH5529Title));

    /// <summary>
    /// Gets the localized string for RH5529MessageFormat
    /// </summary>
    internal static string RH5529MessageFormat => GetString(nameof(RH5529MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5530Title
    /// </summary>
    internal static string RH5530Title => GetString(nameof(RH5530Title));

    /// <summary>
    /// Gets the localized string for RH5530MessageFormat
    /// </summary>
    internal static string RH5530MessageFormat => GetString(nameof(RH5530MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5531Title
    /// </summary>
    internal static string RH5531Title => GetString(nameof(RH5531Title));

    /// <summary>
    /// Gets the localized string for RH5531MessageFormat
    /// </summary>
    internal static string RH5531MessageFormat => GetString(nameof(RH5531MessageFormat));

    /// <summary>
    /// Gets the localized string for RH5113Title
    /// </summary>
    internal static string RH5113Title => GetString(nameof(RH5113Title));

    /// <summary>
    /// Gets the localized string for RH5113MessageFormat
    /// </summary>
    internal static string RH5113MessageFormat => GetString(nameof(RH5113MessageFormat));

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
        return ResourceManager.GetString(name)
                   ?? throw new InvalidOperationException($"The resource string '{name}' could not be resolved");
    }

    #endregion // Methods
}