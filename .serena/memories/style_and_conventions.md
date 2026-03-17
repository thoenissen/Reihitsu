# Coding Style & Conventions

- File-scoped namespaces
- `#region`/`#endregion` for member organization
- XML doc comments on all public/protected members; `<inheritdoc/>` for overrides
- `== false` instead of `!` for boolean negation
- No expression-bodied methods/constructors
- Private fields: `_camelCase`
- Analyzer class: `RH####<Description>Analyzer`, CodeFix: `RH####<Description>CodeFixProvider`
- Test class: `RH####<Description>AnalyzerTests`
- Test data in `<Category>/Resources/RH####.TestData.cs` (not inline strings)
- Resources in `.resx` files (AnalyzerResources, CodeFixResources, TestData)
- **`.Designer.cs` regeneration**: The `ResXFileCodeGenerator` is a Visual Studio-only tool and does NOT run during `dotnet build`. When adding/modifying entries in any `.resx` file, you MUST also manually update the corresponding `.Designer.cs` file to add/modify the matching property. Follow the existing pattern in the file (XML doc comment + `internal static string` property calling `ResourceManager.GetString`). This applies to `CodeFixResources.Designer.cs`, `AnalyzerResources.Designer.cs`, and all `TestData.Designer.cs` files.
- Do not edit `*.Designer.cs` files
- Update README.MD rule table when adding/modifying rules
