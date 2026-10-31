using System;

namespace Reihitsu.Formatter.Data.Enumerations;

/// <summary>
/// The facts about the construct a node-level formatting root stands in, one flag for each decision of the line-break phase
/// that reads the root's ancestors in its document (FMT-PIPE-024), except the member-access chain decision, whose facts are
/// a <see cref="Reihitsu.Core.FluentChainBoundary"/> owned by the chain rule itself. The root is formatted without a parent,
/// so each such decision reads its flag at the root instead. The flags are captured from the document before the pipeline runs
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
    /// The root is an accessor or an accessor list of a property, indexer, or event declaration that stands on one line apart
    /// from its attribute lists
    /// </summary>
    OneLineProperty = 2,

    /// <summary>
    /// The root is, or lies inside, an element of an argument-like list, with no construct between the root and that element
    /// that lays out the lines inside it
    /// </summary>
    ListElement = 4,

    /// <summary>
    /// The root is the bracketed argument list of an element access or of an implicit element access
    /// </summary>
    ElementAccessArguments = 8,

    /// <summary>
    /// A <see langword="case"/> label introduces the root as its pattern, directly or through <see langword="and"/>,
    /// <see langword="or"/>, and <see langword="not"/> patterns
    /// </summary>
    CaseLabelPattern = 16,

    /// <summary>
    /// The root is a collection initializer on the right of an assignment
    /// </summary>
    AssignedCollectionInitializer = 32
}