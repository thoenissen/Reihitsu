namespace Reihitsu.Formatter.Test.Helpers;

/// <summary>
/// The cause of a phase reversal: two phases that both own one formatting decision and decide it differently. The cause
/// names the phase that changes the final output when it runs alone, so an entry of the cause cannot name another phase
/// </summary>
public sealed class PhaseReversalCause
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="phase">The type name of the phase that changes the final output when it runs alone</param>
    /// <param name="description">What the two phases do</param>
    public PhaseReversalCause(string phase, string description)
    {
        Phase = phase;
        Description = description;
    }

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// The type name of the phase that changes the final output when it runs alone
    /// </summary>
    public string Phase { get; }

    /// <summary>
    /// What the two phases do
    /// </summary>
    public string Description { get; }

    #endregion // Properties
}