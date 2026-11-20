using System;
using System.IO;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Reihitsu.SelfHosting.Test.Utilities;

/// <summary>
/// A file of the repository that the scan passes to the analyzers as an <c>AdditionalFiles</c> item
/// </summary>
internal sealed class SelfHostingAdditionalText : AdditionalText
{
    #region Fields

    /// <summary>
    /// Text of the file, read once
    /// </summary>
    private readonly SourceText _text;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="SelfHostingAdditionalText"/> class
    /// </summary>
    /// <param name="path">Absolute path of the file</param>
    internal SelfHostingAdditionalText(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        Path = path;
        _text = SourceText.From(File.ReadAllText(path));
    }

    #endregion // Constructor

    #region AdditionalText

    /// <inheritdoc/>
    public override string Path { get; }

    /// <inheritdoc/>
    public override SourceText GetText(CancellationToken cancellationToken = default)
    {
        return _text;
    }

    #endregion // AdditionalText
}