using Microsoft.CodeAnalysis;

namespace Reihitsu.Formatter.Test.Helpers;

/// <summary>
/// One phase of an observed pipeline run: its name and the nodes it received and returned
/// </summary>
public sealed class ObservedPhase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="name">The type name of the phase</param>
    /// <param name="input">The node the phase received</param>
    /// <param name="output">The node the phase returned</param>
    public ObservedPhase(string name, SyntaxNode input, SyntaxNode output)
    {
        Name = name;
        Input = input;
        Output = output;
    }

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// The type name of the phase
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The node the phase received
    /// </summary>
    public SyntaxNode Input { get; }

    /// <summary>
    /// The node the phase returned
    /// </summary>
    public SyntaxNode Output { get; }

    #endregion // Properties
}