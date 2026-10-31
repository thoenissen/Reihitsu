using System;

namespace Reihitsu.Formatter.Data.Enumerations;

/// <summary>
/// The facts about the construct a node-level formatting root stands in, one flag for each decision of the line-break phase
/// that reads the root's ancestors in its document (FMT-PIPE-024). The root is formatted without a parent, so each such
/// decision reads its flag at the root instead. The flags are captured from the document before the pipeline runs
/// </summary>
[Flags]
internal enum RootConstruct
{
    /// <summary>
    /// No flag: the root is decided as a construct of its own
    /// </summary>
    None = 0,

    /// <summary>
    /// The root is the <see langword="if"/> statement of an <see langword="else"/> clause
    /// </summary>
    ElseIf = 1,

    /// <summary>
    /// The root's parent continues the root's member-access chain, so the root is not the outermost node of its chain
    /// </summary>
    ChainContinuation = 2,

    /// <summary>
    /// The root is an accessor or an accessor list of a property, indexer, or event declaration that stands on one line apart
    /// from its attribute lists
    /// </summary>
    OneLineProperty = 4,

    /// <summary>
    /// The root is, or lies inside, an element of an argument-like list, with no construct between the root and that element
    /// that lays out the lines inside it
    /// </summary>
    ListElement = 8,

    /// <summary>
    /// The root is the bracketed argument list of an element access or of an implicit element access
    /// </summary>
    ElementAccessArguments = 16,

    /// <summary>
    /// A <see langword="case"/> label introduces the root as its pattern, directly or through <see langword="and"/>,
    /// <see langword="or"/>, and <see langword="not"/> patterns
    /// </summary>
    CaseLabelPattern = 32,

    /// <summary>
    /// The root is a collection initializer on the right of an assignment
    /// </summary>
    AssignedCollectionInitializer = 64
}