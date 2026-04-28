using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.BlankLines;

/// <summary>
/// Tests for <see cref="Reihitsu.Formatter.Pipeline.FormattingPipeline"/> — blank lines around region directives
/// </summary>
[TestClass]
public class BlankLineRegionTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that the blank line before #endregion is not inserted after the following #region instead
    /// </summary>
    [TestMethod]
    public void BlankLineBeforeEndRegionNotMovedToAfterNextRegion()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 #region Constructor

                                 public C(string value)
                                 {
                                     Value = value;
                                 }

                                 public C(int value)
                                 {
                                     Value = value.ToString();
                                 }
                                 #endregion // Constructor

                                 #region Properties

                                 public string Value { get; }

                                 #endregion // Properties
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    #region Constructor

                                    public C(string value)
                                    {
                                        Value = value;
                                    }

                                    public C(int value)
                                    {
                                        Value = value.ToString();
                                    }

                                    #endregion // Constructor

                                    #region Properties

                                    public string Value { get; }

                                    #endregion // Properties
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that blank lines around <c>#region</c> / <c>#endregion</c> directives are correct
    /// when multiple regions with fields, constructors, and properties are present.
    /// A blank line must be inserted before <c>#endregion</c> and no extra blank line
    /// must be inserted after the following <c>#region</c>
    /// </summary>
    [TestMethod]
    public void BlankLinesAroundRegionsWithFieldsAndConstructorsAreCorrect()
    {
        // Arrange
        const string input = """
                             using System;
                             using System.Net.Http;

                             namespace N;

                             /// <summary>
                             /// Connector for accessing a remote API
                             /// </summary>
                             public sealed class ServiceConnector : IDisposable
                             {
                                 #region Fields

                                 /// <summary>
                                 /// Maximum retry count
                                 /// </summary>
                                 private const int MaxRetryCount = 3;

                                 /// <summary>
                                 /// Authentication token
                                 /// </summary>
                                 private readonly string _authToken;

                                 /// <summary>
                                 /// HTTP client instance
                                 /// </summary>
                                 private HttpClient _httpClient;

                                 #endregion // Fields

                                 #region Constructor

                                 /// <summary>
                                 /// Constructor
                                 /// </summary>
                                 /// <param name="authToken">Authentication token</param>
                                 public ServiceConnector(string authToken)
                                 {
                                     _authToken = authToken;
                                 }

                                 /// <summary>
                                 /// Constructor
                                 /// </summary>
                                 /// <param name="httpClient">HTTP client</param>
                                 /// <param name="authToken">Authentication token</param>
                                 public ServiceConnector(HttpClient httpClient, string authToken)
                                 {
                                     _authToken = authToken;
                                     _httpClient = httpClient;
                                 }
                                 #endregion // Constructor

                                 #region Properties

                                 /// <summary>
                                 /// Gets the HTTP client
                                 /// </summary>
                                 public HttpClient Client => _httpClient;

                                 #endregion // Properties

                                 #region IDisposable

                                 /// <inheritdoc/>
                                 public void Dispose()
                                 {
                                     _httpClient?.Dispose();
                                 }

                                 #endregion // IDisposable
                             }
                             """;

        const string expected = """
                                using System;
                                using System.Net.Http;

                                namespace N;

                                /// <summary>
                                /// Connector for accessing a remote API
                                /// </summary>
                                public sealed class ServiceConnector : IDisposable
                                {
                                    #region Fields

                                    /// <summary>
                                    /// Maximum retry count
                                    /// </summary>
                                    private const int MaxRetryCount = 3;

                                    /// <summary>
                                    /// Authentication token
                                    /// </summary>
                                    private readonly string _authToken;

                                    /// <summary>
                                    /// HTTP client instance
                                    /// </summary>
                                    private HttpClient _httpClient;

                                    #endregion // Fields

                                    #region Constructor

                                    /// <summary>
                                    /// Constructor
                                    /// </summary>
                                    /// <param name="authToken">Authentication token</param>
                                    public ServiceConnector(string authToken)
                                    {
                                        _authToken = authToken;
                                    }

                                    /// <summary>
                                    /// Constructor
                                    /// </summary>
                                    /// <param name="httpClient">HTTP client</param>
                                    /// <param name="authToken">Authentication token</param>
                                    public ServiceConnector(HttpClient httpClient, string authToken)
                                    {
                                        _authToken = authToken;
                                        _httpClient = httpClient;
                                    }

                                    #endregion // Constructor

                                    #region Properties

                                    /// <summary>
                                    /// Gets the HTTP client
                                    /// </summary>
                                    public HttpClient Client => _httpClient;

                                    #endregion // Properties

                                    #region IDisposable

                                    /// <inheritdoc/>
                                    public void Dispose()
                                    {
                                        _httpClient?.Dispose();
                                    }

                                    #endregion // IDisposable
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}