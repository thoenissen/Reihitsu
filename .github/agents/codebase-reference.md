# Reihitsu Codebase Reference

This document provides a complete reference of the Reihitsu project structure, code patterns, and conventions. It is intended to be consumed by agents to avoid redundant codebase exploration.

---

## 1. Solution Structure

```
Reihitsu.sln
├── Reihitsu.Analyzer\Reihitsu.Analyzer\              # Analyzer library (netstandard2.0)
├── Reihitsu.Analyzer\Reihitsu.Analyzer.CodeFixes\     # Code fix providers (netstandard2.0)
├── Reihitsu.Analyzer\Reihitsu.Analyzer.Test\          # MSTest tests (net10.0)
└── Reihitsu.Analyzer\Reihitsu.Analyzer.Package\       # NuGet packaging
```

### Analyzer Project Layout

```
Reihitsu.Analyzer\
├── AnalyzerResources.resx              # Title/MessageFormat strings for all rules
├── AnalyzerResources.Designer.cs       # Auto-generated (do NOT edit)
├── Base\
│   ├── DiagnosticAnalyzerBase{TAnalyzer}.cs
│   ├── CasingAnalyzerBase{T}.cs
│   ├── StatementShouldBePrecededByABlankLineAnalyzerBase{TStatement,TAnalyzer}.cs
│   ├── StatementShouldBeFollowedByABlankLineAnalyzerBase{TStatement,TAnalyzer}.cs
│   └── StructEqualityPerformanceAnalyzerBase{TAnalyzer}.cs
├── Core\
│   ├── CasingUtilities.cs
│   ├── SyntaxTreeRegionSearcher.cs
├── Data\
│   ├── Configuration.cs
│   ├── ConfigurationCategoryNaming.cs
│   └── ConfigurationManager.cs
├── Enumerations\
│   └── DiagnosticCategory.cs
├── Extensions\
│   ├── PropertySymbolExtensions.cs
│   └── SyntaxTokenExtensions.cs
├── Properties\
│   ├── AssemblyInfo.cs
│   └── GlobalUsings.cs                # Contains: global using System; global using System.Linq;
└── Rules\
    ├── Clarity\      (RH00xx)
    ├── Design\       (RH01xx)
    ├── Naming\       (RH02xx)
    ├── Formatting\   (RH03xx)
    ├── Documentation\(RH04xx)
    └── Performance\  (RH05xx)
```

### Code Fixes Project Layout

```
Reihitsu.Analyzer.CodeFixes\
├── CodeFixResources.resx               # Title strings for code fixes
├── CodeFixResources.Designer.cs        # Auto-generated (do NOT edit)
├── Properties\
│   ├── AssemblyInfo.cs
│   └── GlobalUsings.cs                 # Contains: global using System; global using System.Linq;
└── Rules\
    ├── Clarity\
    ├── Design\
    ├── Documentation\
    ├── Formatting\
    ├── Naming\
    │   └── CasingCodeFixProviderBase{T}.cs   # Shared base for casing code fixes
    └── Performance\                          # Empty (no code fixes for performance rules)
```

**Note:** The CodeFixes project has `RootNamespace` set to `Reihitsu.Analyzer` in the .csproj so that the code fix classes share the same root namespace as the analyzer classes.

### Test Project Layout

```
Reihitsu.Analyzer.Test\
├── Base\
│   ├── AnalyzerTestsBase{TAnalyzer}.cs
│   └── AnalyzerTestsBase{TAnalyzer,TCodeFix}.cs
├── Verifiers\
│   ├── CSharpAnalyzerVerifierTest{TAnalyzer}.cs
│   ├── CSharpCodeFixVerifierTest{TAnalyzer,TCodeFix}.cs
│   └── CSharpVerifierHelper.cs
├── Clarity\
│   ├── RH0001NotOperatorShouldNotBeUsedAnalyzerTests.cs
│   └── Resources\
│       ├── TestData.resx / TestData.Designer.cs
│       ├── RH0001.TestData.cs
│       └── RH0001.ResultData.cs
├── Design\
│   ├── RH0101...Tests.cs, RH0102...Tests.cs, RH0103...Tests.cs
│   └── Resources\
├── Naming\
│   ├── RH0201...Tests.cs through RH0227...Tests.cs
│   └── Resources\
├── Formatting\
│   ├── RH0301...Tests.cs through RH0329...Tests.cs
│   └── Resources\
├── Documentation\
│   ├── RH0401...Tests.cs
│   └── Resources\
└── Performance\
    ├── RH0501...Tests.cs, RH0502...Tests.cs
    └── Resources\
```

---

## 2. Diagnostic Categories & ID Ranges

```csharp
internal enum DiagnosticCategory
{
    Clarity = 0,        // RH00xx
    Design = 1,         // RH01xx
    Naming = 2,         // RH02xx
    Formatting = 3,     // RH03xx
    Documentation = 4,  // RH04xx
    Performance = 5,    // RH05xx
}
```

---

## 3. Existing Rules Inventory

### Clarity (RH00xx)
| ID     | Analyzer Class | Has Code Fix | Base Class |
|--------|---------------|:---:|------------|
| RH0001 | `RH0001NotOperatorShouldNotBeUsedAnalyzer` | ✓ | `DiagnosticAnalyzerBase<T>` |

### Design (RH01xx)
| ID     | Analyzer Class | Has Code Fix | Base Class |
|--------|---------------|:---:|------------|
| RH0101 | `RH0101PrivateAutoPropertiesShouldNotBeUsedAnalyzer` | ✓ | `DiagnosticAnalyzerBase<T>` |
| RH0102 | `RH0102AsyncVoidShouldNotBeUsedAnalyzer` | ✗ | `DiagnosticAnalyzerBase<T>` |
| RH0103 | `RH0103NotImplementedExceptionShouldNotBeUsedAnalyzer` | ✗ | `DiagnosticAnalyzerBase<T>` |

### Naming (RH02xx)
| ID     | Analyzer Class | Has Code Fix | Base Class |
|--------|---------------|:---:|------------|
| RH0201 | `RH0201TypeNameShouldMatchFileNameAnalyzer` | ✓ | `DiagnosticAnalyzerBase<T>` |
| RH0202 | `RH0202ClassNameCasingAnalyzer` | ✓ | `CasingAnalyzerBase<T>` |
| RH0203 | `RH0203StructNameCasingAnalyzer` | ✓ | `CasingAnalyzerBase<T>` |
| RH0204 | `RH0204EnumNameCasingAnalyzer` | ✓ | `CasingAnalyzerBase<T>` |
| RH0205 | `RH0205EnumMemberCasingAnalyzer` | ✓ | `CasingAnalyzerBase<T>` |
| RH0206 | `RH0206InterfaceNameCasingAnalyzer` | ✓ | `CasingAnalyzerBase<T>` |
| RH0207 | `RH0207EventNameCasingAnalyzer` | ✓ | `CasingAnalyzerBase<T>` |
| RH0208 | `RH0208DelegateNameCasingAnalyzer` | ✓ | `CasingAnalyzerBase<T>` |
| RH0209 | `RH0209MethodNameCasingAnalyzer` | ✓ | `CasingAnalyzerBase<T>` |
| RH0210 | `RH0210LocalFunctionNameCasingAnalyzer` | ✓ | `CasingAnalyzerBase<T>` |
| RH0211 | `RH0211MethodParameterCasingAnalyzer` | ✓ | `CasingAnalyzerBase<T>` |
| RH0212 | `RH0212PrivateFieldCasingAnalyzer` | ✓ | `CasingAnalyzerBase<T>` |
| RH0213 | `RH0213ProtectedFieldCasingAnalyzer` | ✓ | `CasingAnalyzerBase<T>` |
| RH0214 | `RH0214InternalFieldCasingAnalyzer` | ✓ | `CasingAnalyzerBase<T>` |
| RH0215 | `RH0215PublicFieldCasingAnalyzer` | ✓ | `CasingAnalyzerBase<T>` |
| RH0216 | `RH0216ConstFieldCasingAnalyzer` | ✓ | `CasingAnalyzerBase<T>` |
| RH0217 | `RH0217PrivatePropertyCasingAnalyzer` | ✓ | `CasingAnalyzerBase<T>` |
| RH0218 | `RH0218ProtectedPropertyCasingAnalyzer` | ✓ | `CasingAnalyzerBase<T>` |
| RH0219 | `RH0219InternalPropertyCasingAnalyzer` | ✓ | `CasingAnalyzerBase<T>` |
| RH0220 | `RH0220PublicPropertyCasingAnalyzer` | ✓ | `CasingAnalyzerBase<T>` |
| RH0221 | `RH0221LocalVariableCasingAnalyzer` | ✓ | `CasingAnalyzerBase<T>` |
| RH0222 | `RH0222TupleElementCasingAnalyzer` | ✓ | `CasingAnalyzerBase<T>` |
| RH0223 | `RH0223DeconstructionVariableCasingAnalyzer` | ✓ | `CasingAnalyzerBase<T>` |
| RH0224 | `RH0224TupleElementCasingAnalyzer` | ✓ | `CasingAnalyzerBase<T>` |
| RH0225 | `RH0225FileScopedNamespaceCasingAnalyzer` | ✗ | `CasingAnalyzerBase<T>` |
| RH0226 | `RH0226NamespaceCasingAnalyzer` | ✗ | `CasingAnalyzerBase<T>` |
| RH0227 | `RH0227NamespaceNotAllowedAnalyzer` | ✗ | `DiagnosticAnalyzerBase<T>` |

### Formatting (RH03xx)
| ID     | Analyzer Class | Has Code Fix | Base Class |
|--------|---------------|:---:|------------|
| RH0301 | `RH0301RegionsShouldMatchAnalyzer` | ✓ | `DiagnosticAnalyzerBase<T>` |
| RH0302 | `RH0302ObjectInitializerShouldBeFormattedCorrectlyAnalyzer` | ✓ | `DiagnosticAnalyzerBase<T>` |
| RH0303 | `RH0303TryStatementsShouldBePrecededByABlankLineAnalyzer` | ✗ | `StatementShouldBePrecededByABlankLineAnalyzerBase<T,T>` |
| RH0304 | `RH0304IfStatementsShouldBePrecededByABlankLineAnalyzer` | ✗ | `StatementShouldBePrecededByABlankLineAnalyzerBase<T,T>` |
| RH0305 | `RH0305WhileStatementsShouldBePrecededByABlankLineAnalyzer` | ✗ | `StatementShouldBePrecededByABlankLineAnalyzerBase<T,T>` |
| RH0306 | `RH0306DoStatementsShouldBePrecededByABlankLineAnalyzer` | ✗ | `StatementShouldBePrecededByABlankLineAnalyzerBase<T,T>` |
| RH0307 | `RH0307UsingStatementsShouldBePrecededByABlankLineAnalyzer` | ✗ | `StatementShouldBePrecededByABlankLineAnalyzerBase<T,T>` |
| RH0308 | `RH0308ForeachStatementsShouldBePrecededByABlankLineAnalyzer` | ✗ | `StatementShouldBePrecededByABlankLineAnalyzerBase<T,T>` |
| RH0309 | `RH0309ForStatementsShouldBePrecededByABlankLineAnalyzer` | ✗ | `StatementShouldBePrecededByABlankLineAnalyzerBase<T,T>` |
| RH0310 | `RH0310ReturnStatementsShouldBePrecededByABlankLineAnalyzer` | ✗ | `StatementShouldBePrecededByABlankLineAnalyzerBase<T,T>` |
| RH0311 | `RH0311GotoStatementsShouldBePrecededByABlankLineAnalyzer` | ✗ | `StatementShouldBePrecededByABlankLineAnalyzerBase<T,T>` |
| RH0312 | `RH0312BreakStatementsShouldBePrecededByABlankLineAnalyzer` | ✗ | `StatementShouldBePrecededByABlankLineAnalyzerBase<T,T>` |
| RH0313 | `RH0313BreakStatementsShouldBeFollowedByABlankLineAnalyzer` | ✗ | `StatementShouldBeFollowedByABlankLineAnalyzerBase<T,T>` |
| RH0314 | `RH0314ContinueStatementsShouldBePrecededByABlankLineAnalyzer` | ✗ | `StatementShouldBePrecededByABlankLineAnalyzerBase<T,T>` |
| RH0315 | `RH0315ThrowStatementsShouldBePrecededByABlankLineAnalyzer` | ✗ | `StatementShouldBePrecededByABlankLineAnalyzerBase<T,T>` |
| RH0316 | `RH0316SwitchStatementsShouldBePrecededByABlankLineAnalyzer` | ✗ | `StatementShouldBePrecededByABlankLineAnalyzerBase<T,T>` |
| RH0317 | `RH0317CheckedStatementsShouldBePrecededByABlankLineAnalyzer` | ✗ | `StatementShouldBePrecededByABlankLineAnalyzerBase<T,T>` |
| RH0318 | `RH0318UncheckedStatementsShouldBePrecededByABlankLineAnalyzer` | ✗ | `StatementShouldBePrecededByABlankLineAnalyzerBase<T,T>` |
| RH0319 | `RH0319FixedStatementsShouldBePrecededByABlankLineAnalyzer` | ✗ | `StatementShouldBePrecededByABlankLineAnalyzerBase<T,T>` |
| RH0320 | `RH0320LockStatementsShouldBePrecededByABlankLineAnalyzer` | ✗ | `StatementShouldBePrecededByABlankLineAnalyzerBase<T,T>` |
| RH0321 | `RH0321YieldStatementsShouldBePrecededByABlankLineAnalyzer` | ✗ | `StatementShouldBePrecededByABlankLineAnalyzerBase<T,T>` |
| RH0324 | `RH0324MethodChainsShouldBeAlignedAnalyzer` | ✓ | `DiagnosticAnalyzerBase<T>` |
| RH0325 | `RH0325ExpressionStyleMethodsShouldNotBeUsedAnalyzer` | ✗ | `DiagnosticAnalyzerBase<T>` |
| RH0326 | `RH0326ExpressionStyleConstructorsShouldNotBeUsedAnalyzer` | ✗ | `DiagnosticAnalyzerBase<T>` |
| RH0327 | `RH0327ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedAnalyzer` | ✗ | `DiagnosticAnalyzerBase<T>` |
| RH0328 | `RH0328RegionsShouldStartWithAUpperCaseLetterAnalyzer` | ✗ | `DiagnosticAnalyzerBase<T>` |
| RH0329 | `RH0329LogicalExpressionsShouldBeFormattedCorrectlyAnalyzer` | ✓ | `DiagnosticAnalyzerBase<T>` |

### Documentation (RH04xx)
| ID     | Analyzer Class | Has Code Fix | Base Class |
|--------|---------------|:---:|------------|
| RH0401 | `RH0401InheritdocShouldBeUsedAnalyzer` | ✓ | `DiagnosticAnalyzerBase<T>` |

### Performance (RH05xx)
| ID     | Analyzer Class | Has Code Fix | Base Class |
|--------|---------------|:---:|------------|
| RH0501 | `RH0501TypesUsedAsKeysMustImplementEqualityMembersAnalyzer` | ✗ | `StructEqualityPerformanceAnalyzerBase<T>` |
| RH0502 | `RH0502TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzer` | ✗ | `StructEqualityPerformanceAnalyzerBase<T>` |

---

## 4. Code Templates

### 4.1 Analyzer (Simple — inheriting DiagnosticAnalyzerBase)

```csharp
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.<Category>;

/// <summary>
/// RH####: <Description>.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH####<Name>Analyzer : DiagnosticAnalyzerBase<RH####<Name>Analyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH####";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH####<Name>Analyzer()
        : base(DiagnosticId, DiagnosticCategory.<Category>, nameof(AnalyzerResources.RH####Title), nameof(AnalyzerResources.RH####MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// <Analysis method description>
    /// </summary>
    /// <param name="context">Context</param>
    private void On<EventName>(SyntaxNodeAnalysisContext context)
    {
        // Analysis logic...
        // Report: context.ReportDiagnostic(CreateDiagnostic(location));
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(On<EventName>, SyntaxKind.<Kind>);
    }

    #endregion // DiagnosticAnalyzer
}
```

### 4.2 Code Fix Provider (Simple — inheriting CodeFixProvider directly)

```csharp
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Analyzer.Rules.<Category>;

/// <summary>
/// Providing fixes for <see cref="RH####<Name>Analyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH####<Name>CodeFixProvider))]
public class RH####<Name>CodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applying code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="node">Node with diagnostics</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private async Task<Document> ApplyCodeFixAsync(Document document, <NodeType> node, CancellationToken cancellationToken)
    {
        // Code fix logic...
        return document;
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH####<Name>Analyzer.DiagnosticId];

    /// <inheritdoc/>
    public sealed override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    /// <inheritdoc/>
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        if (root != null)
        {
            foreach (var diagnostic in context.Diagnostics)
            {
                if (root.FindNode(diagnostic.Location.SourceSpan) is <NodeType> node)
                {
                    context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH####Title,
                                                              c => ApplyCodeFixAsync(context.Document, node, c),
                                                              nameof(RH####<Name>CodeFixProvider)),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // CodeFixProvider
}
```

### 4.3 Test Class (Analyzer + Code Fix)

```csharp
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.<Category>;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.<Category>.Resources;

namespace Reihitsu.Analyzer.Test.<Category>;

/// <summary>
/// Test methods for <see cref="RH####<Name>Analyzer"/> and <see cref="RH####<Name>CodeFixProvider"/>
/// </summary>
[TestClass]
public class RH####<Name>AnalyzerTests : AnalyzerTestsBase<RH####<Name>Analyzer, RH####<Name>CodeFixProvider>
{
    /// <summary>
    /// Verifying diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnostics()
    {
        await Verify(TestData.RH####TestData, TestData.RH####ResultData, Diagnostics(RH####<Name>Analyzer.DiagnosticId, AnalyzerResources.RH####MessageFormat, <count>));
    }
}
```

### 4.4 Test Class (Analyzer Only — No Code Fix)

```csharp
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.<Category>;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.<Category>.Resources;

namespace Reihitsu.Analyzer.Test.<Category>;

/// <summary>
/// Test methods for <see cref="RH####<Name>Analyzer"/>
/// </summary>
[TestClass]
public class RH####<Name>AnalyzerTests : AnalyzerTestsBase<RH####<Name>Analyzer>
{
    /// <summary>
    /// Verifying diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnostics()
    {
        await Verify(TestData.RH####TestData, Diagnostics(RH####<Name>Analyzer.DiagnosticId, AnalyzerResources.RH####MessageFormat, <count>));
    }
}
```

### 4.5 Test with Additional "No Diagnostics" Test Method

Some tests include a second method to verify that certain code does NOT trigger the diagnostic:

```csharp
/// <summary>
/// Verifying no diagnostics for <scenario description>
/// </summary>
/// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
[TestMethod]
public async Task VerifyNoDiagnosticsFor<Scenario>()
{
    await Verify(TestData.RH####NoDiagnosticsTestData);
}
```

---

## 5. Test Data File Conventions

### 5.1 File Naming

- Input test data: `RH####.TestData.cs`
- Expected code fix output: `RH####.ResultData.cs`
- Additional test scenarios: `RH####.<ScenarioName>.TestData.cs` / `RH####.<ScenarioName>.ResultData.cs`
- No-diagnostics test data: `RH####.NoDiagnosticsTestData.cs` or `RH####.<ScenarioName>.TestData.cs`

### 5.2 Test Data File Location

All test data `.cs` files go in `<Category>\Resources\` under the test project.

### 5.3 Diagnostic Markup Syntax

Test data files use Roslyn's markup syntax to mark expected diagnostic locations:

```csharp
// {|#<index>:<marked text>|} marks expected diagnostic at position #<index>
return {|#0:!|}false;       // Diagnostic #0 at the ! operator
return {|#1:!|}_field;      // Diagnostic #1 at the ! operator
```

The `#<index>` corresponds to the order in the `Diagnostics(...)` array (0-based).

### 5.4 TestData.resx Entry Format

Each test data file must be registered in the category's `TestData.resx` as a `ResXFileRef`:

```xml
<assembly alias="System.Windows.Forms" name="System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" />
<data name="RH####TestData" type="System.Resources.ResXFileRef, System.Windows.Forms">
  <value>RH####.TestData.cs;System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089;utf-8</value>
</data>
<data name="RH####ResultData" type="System.Resources.ResXFileRef, System.Windows.Forms">
  <value>RH####.ResultData.cs;System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089;utf-8</value>
</data>
```

**Important:** The `assembly alias` element for `System.Windows.Forms` must exist once in the resx. The resource name uses the format `RH####TestData` (no dot) while the file reference uses `RH####.TestData.cs` (with dot).

---

## 6. Required File Changes Checklist (New Rule)

When adding a new rule `RH####`, the following files must be created or modified:

### New Files to Create

1. **Analyzer**: `Reihitsu.Analyzer\Rules\<Category>\RH####<Name>Analyzer.cs`
2. **Code Fix** (if applicable): `Reihitsu.Analyzer.CodeFixes\Rules\<Category>\RH####<Name>CodeFixProvider.cs`
3. **Test class**: `Reihitsu.Analyzer.Test\<Category>\RH####<Name>AnalyzerTests.cs`
4. **Test data**: `Reihitsu.Analyzer.Test\<Category>\Resources\RH####.TestData.cs`
5. **Result data** (if code fix): `Reihitsu.Analyzer.Test\<Category>\Resources\RH####.ResultData.cs`

### Files to Modify

6. **AnalyzerResources.resx**: Add `RH####Title` and `RH####MessageFormat` entries
7. **CodeFixResources.resx** (if code fix): Add `RH####Title` entry
8. **TestData.resx** (in `<Category>\Resources\`): Add `ResXFileRef` entries for test data files
9. **Test .csproj**: Add `<Compile Remove="..."/>` and `<None Include="..."/>` entries for new test data files
10. **README.MD**: Add row to the rules table

### Do NOT Modify

- `AnalyzerResources.Designer.cs` — auto-generated, regenerated from resx
- `CodeFixResources.Designer.cs` — auto-generated, regenerated from resx
- `TestData.Designer.cs` — auto-generated, regenerated from resx

---

## 7. csproj Patterns for Test Data Files

In `Reihitsu.Analyzer.Test.csproj`, test data `.cs` files need two entries:

```xml
<!-- Exclude from compilation -->
<ItemGroup>
  <Compile Remove="<Category>\Resources\RH####.TestData.cs" />
  <Compile Remove="<Category>\Resources\RH####.ResultData.cs" />  <!-- if code fix exists -->
</ItemGroup>

<!-- Include as content -->
<ItemGroup>
  <None Include="<Category>\Resources\RH####.TestData.cs" />
  <None Include="<Category>\Resources\RH####.ResultData.cs" />  <!-- if code fix exists -->
</ItemGroup>
```

The `<Compile Remove>` entries are grouped in a single `<ItemGroup>` at the top, organized by category. The `<None Include>` entries are in a separate `<ItemGroup>`.

---

## 8. AnalyzerResources.resx Entry Format

```xml
<data name="RH####MessageFormat" xml:space="preserve">
  <value>The description of what the analyzer detects.</value>
</data>
<data name="RH####Title" xml:space="preserve">
  <value>The description of what the analyzer detects.</value>
</data>
```

**Note:** In most rules, Title and MessageFormat have the same value.

---

## 9. CodeFixResources.resx Entry Format

```xml
<data name="RH####Title" xml:space="preserve">
  <value>Description of the code fix action</value>
</data>
```

---

## 10. README.MD Table Format

The rules table uses this exact format:

```markdown
| ID     | Description                                                           | Analyzer | Code Fix |
|--------|-----------------------------------------------------------------------|:--------:|:--------:|
| RH#### | Description of the rule.                                              | &#10004; | &#10004; |
```

- `&#10004;` = ✓ (check mark) — rule is implemented
- `&#10060;` = ✗ (cross) — not implemented
- Category headers are bold: `|        | **CategoryName**                                                      |          |          |`
- Rules are listed in order by ID within their category section

---

## 11. Base Class Reference

### DiagnosticAnalyzerBase<TAnalyzer>

- **Location:** `Reihitsu.Analyzer\Base\DiagnosticAnalyzerBase{TAnalyzer}.cs`
- **Purpose:** Root base class for all analyzers
- **Constructor:** `internal DiagnosticAnalyzerBase(string diagnosticId, DiagnosticCategory category, string tileResourceName, string messageFormatResourceName)`
- **Key methods:**
  - `CreateDiagnostic(Location location)` → creates a `Diagnostic`
  - `CreateDiagnostic(ImmutableArray<Location> locations)` → creates a `Diagnostic` with multiple locations
- **Initialize:** Calls `context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None)` and in RELEASE enables concurrent execution

### CasingAnalyzerBase<T>

- **Location:** `Reihitsu.Analyzer\Base\CasingAnalyzerBase{T}.cs`
- **Purpose:** Base class for naming/casing rules
- **Constructor:** `private protected CasingAnalyzerBase(string diagnosticId, DiagnosticCategory category, string titleResourceName, string messageFormatResourceName, SyntaxKind type, Func<string, bool> casingValidation)`
- **Abstract method:** `GetLocations(SyntaxNode node)` — returns `IEnumerable<(string Name, Location Location)>`
- **Auto-registers:** `RegisterSyntaxNodeAction` for the given `SyntaxKind`

### StatementShouldBePrecededByABlankLineAnalyzerBase<TStatement, TAnalyzer>

- **Location:** `Reihitsu.Analyzer\Base\StatementShouldBePrecededByABlankLineAnalyzerBase{TStatement,TAnalyzer}.cs`
- **Purpose:** Base class for "statement should be preceded by blank line" rules
- **Constructor:** `private protected StatementShouldBePrecededByABlankLineAnalyzerBase(string diagnosticId, DiagnosticCategory category, string titleResourceName, string messageFormatResourceName, SyntaxKind syntaxKind)`
- **Abstract methods:** `GetLocation(TStatement)`, `GetPreviousToken(TStatement)`
- **Virtual method:** `IsRelevant(TStatement)` (defaults to `true`)

### StatementShouldBeFollowedByABlankLineAnalyzerBase<TStatement, TAnalyzer>

- **Location:** `Reihitsu.Analyzer\Base\StatementShouldBeFollowedByABlankLineAnalyzerBase{TStatement,TAnalyzer}.cs`
- **Purpose:** Base class for "statement should be followed by blank line" rules
- **Constructor:** Same pattern as preceded-by variant
- **Abstract methods:** `GetLocation(TStatement)`, `GetNextToken(TStatement)`

### StructEqualityPerformanceAnalyzerBase<TAnalyzer>

- **Location:** `Reihitsu.Analyzer\Base\StructEqualityPerformanceAnalyzerBase{TAnalyzer}.cs`
- **Purpose:** Base class for struct equality performance rules
- **Key method:** `AreEqualityMembersImplemented(Compilation, ITypeSymbol)` — checks if a type implements `IEquatable<T>` or overrides `Equals`/`GetHashCode`

### CasingCodeFixProviderBase<T>

- **Location:** `Reihitsu.Analyzer.CodeFixes\Rules\Naming\CasingCodeFixProviderBase{T}.cs`
- **Purpose:** Shared base for all casing code fix providers
- **Constructor:** `protected CasingCodeFixProviderBase(string diagnosticId, string title, Func<string, string> casingConversion)`
- **Abstract methods:** `ReplaceIdentifier(T node, string identifier)`, `GetIdentifier(T node)`

### AnalyzerTestsBase<TAnalyzer>

- **Location:** `Reihitsu.Analyzer.Test\Base\AnalyzerTestsBase{TAnalyzer}.cs`
- **Key methods:**
  - `Diagnostic(string diagnosticId)` — creates a `DiagnosticResult`
  - `Diagnostics(string diagnosticId, string message, int count = 1)` — creates array of `DiagnosticResult` with `InterpretAsMarkupKey`
  - `Diagnostics(string diagnosticId, Func<int, string> messageProvider, int count = 1)` — same with dynamic messages
  - `Verify(string source, params DiagnosticResult[] expected)` — verifies analyzer diagnostics
  - `Verify(string source, Action<CSharpAnalyzerVerifierTest<TAnalyzer>> onConfigure, params DiagnosticResult[] expected)` — with custom configuration
- **ReferenceAssemblies:** Uses `ReferenceAssemblies.Net.Net90`

### AnalyzerTestsBase<TAnalyzer, TCodeFix>

- **Location:** `Reihitsu.Analyzer.Test\Base\AnalyzerTestsBase{TAnalyzer,TCodeFix}.cs`
- **Inherits from:** `AnalyzerTestsBase<TAnalyzer>`
- **Additional methods:**
  - `Verify(string source, string fixedSource, params DiagnosticResult[] expected)` — verifies analyzer + code fix
  - `Verify(string source, string fixedSource, Action<CSharpCodeFixVerifierTest<TAnalyzer, TCodeFix>> onConfigure, params DiagnosticResult[] expected)` — with custom configuration

---

## 12. Package Versions

| Package | Version |
|---------|---------|
| Microsoft.CodeAnalysis.Analyzers | 4.14.0 |
| Microsoft.CodeAnalysis.CSharp | 4.14.0 |
| Microsoft.CodeAnalysis.CSharp.Workspaces | 4.14.0 |
| Microsoft.CodeAnalysis | 4.14.0 |
| StyleCop.Analyzers | 1.2.0-beta.556 |
| System.Text.Json | 8.0.6 |
| Microsoft.NET.Test.Sdk | 17.14.1 |
| MSTest.TestAdapter | 3.10.2 |
| MSTest.TestFramework | 3.10.2 |
| Microsoft.CodeAnalysis.CSharp.Analyzer.Testing | 1.1.2 |
| Microsoft.CodeAnalysis.CSharp.CodeFix.Testing | 1.1.2 |

---

## 13. Build & Test Commands

```powershell
# Build the solution
dotnet build Reihitsu.sln

# Run all tests
dotnet test Reihitsu.Analyzer\Reihitsu.Analyzer.Test\Reihitsu.Analyzer.Test.csproj

# Run specific test class
dotnet test --filter "FullyQualifiedName~RH0001"
```

---

## 14. Code Style Summary

- **File-scoped namespaces:** `namespace X;`
- **Regions:** `#region Name` / `#endregion // Name` — descriptions match, start uppercase
- **Boolean negation:** Use `== false` instead of `!`
- **No expression-bodied methods/constructors**
- **Private fields:** `_camelCase` prefix
- **XML doc comments:** Required on all public/protected members; use `<inheritdoc/>` for overrides
- **Blank lines before statements:** `if`, `try`, `return`, `foreach`, etc. must be preceded by a blank line (unless first in block)
