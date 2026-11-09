namespace Reihitsu.Analyzer.CodeFixes.Rules.Naming;

/// <summary>
/// Fixes for events with accessors reported by <see cref="Analyzer.Rules.Naming.RH4102EventNameCasingAnalyzer"/>; it is
/// not exported, because <see cref="RH4102EventNameCasingCodeFixProvider"/> fixes every RH4102 diagnostic
/// </summary>
internal sealed class RH4102CustomEventNameCasingCodeFixProvider : RH4102EventNameCasingCodeFixProvider;