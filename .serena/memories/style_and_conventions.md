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
- Do not edit `*.Designer.cs` files
- Update README.MD rule table when adding/modifying rules
