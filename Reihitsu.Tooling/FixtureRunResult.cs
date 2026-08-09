using Reihitsu.Tooling.Enumerations;

namespace Reihitsu.Tooling;

/// <summary>
/// The observations one fixture produced under one line-ending arm
/// </summary>
public sealed class FixtureRunResult
{
    #region Properties

    /// <summary>
    /// How the fixture ended
    /// </summary>
    public FixtureOutcome Outcome { get; }

    /// <summary>
    /// Number of times a code action was applied
    /// </summary>
    public int Iterations { get; }

    /// <summary>
    /// Number of code actions the provider registered for the first fixed diagnostic. A value above one means the
    /// runner picked the first of several offers, which a sweep has to see rather than infer
    /// </summary>
    public int RegisteredActions { get; }

    /// <summary>
    /// Source the fixture was analyzed from, after normalization to the arm's line ending
    /// </summary>
    public string OriginalSource { get; }

    /// <summary>
    /// Source after the last applied code action
    /// </summary>
    public string FinalSource { get; }

    /// <summary>
    /// Whether the final source still uses the arm's line ending exclusively
    /// </summary>
    public bool PreservedLineEnding { get; }

    #endregion // Properties

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="FixtureRunResult"/> class
    /// </summary>
    /// <param name="outcome">How the fixture ended</param>
    /// <param name="iterations">Number of times a code action was applied</param>
    /// <param name="registeredActions">Number of code actions registered for the first fixed diagnostic</param>
    /// <param name="originalSource">Source the fixture was analyzed from</param>
    /// <param name="finalSource">Source after the last applied code action</param>
    /// <param name="preservedLineEnding">Whether the final source still uses the arm's line ending exclusively</param>
    public FixtureRunResult(FixtureOutcome outcome,
                            int iterations,
                            int registeredActions,
                            string originalSource,
                            string finalSource,
                            bool preservedLineEnding)
    {
        Outcome = outcome;
        Iterations = iterations;
        RegisteredActions = registeredActions;
        OriginalSource = originalSource;
        FinalSource = finalSource;
        PreservedLineEnding = preservedLineEnding;
    }

    #endregion // Constructor
}