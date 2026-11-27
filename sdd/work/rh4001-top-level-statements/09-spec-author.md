Role: spec author
Commit: 5c7de75
Verdict: spec written

Repair round 1, finding #1 of `08-auditor.md` (spec side). My prompt carried a suggested change; I re-derived it from the spec text and did not rely on the suggestion.

The third example of RH4001-007 says the file's "statements stand inside `Main`", but `Main` was empty. The example now holds one statement inside `Main`, so the code matches the sentence. No sentence of the requirement changed, and no other example changed. The description stays correct: the statement is a `StatementSyntax` under a method, not a member of the compilation unit, so the file is checked, and `Program` matches `Program.cs` (RH4001-001), so it is not reported. The change doc and rule page carry no copy of this example and are unchanged.

The example's code now reads, in a file `Program.cs`, not reported:

```csharp
internal static class Program
{
    private static void Main()
    {
        System.Console.WriteLine();
    }
}

internal class Helper
{
}
```

The statement is `System.Console.WriteLine();`, the same one the reported-side test `FileWithStatementsOnlyInsideTypeMemberIsReportedAndFileIsRenamed` uses. It needs no `using` and compiles under `OutputKind.ConsoleApplication`.

For the test implementer: mirror this code in `FileWithStatementsOnlyInsideMainAndMatchingFirstTypeIsNotReported`, and make its summary say that a file whose statements stand only inside a method of its first type is not exempted and is not reported when that type matches the file name.

## Changed requirements
- RH4001-007 — A file with top-level statements is not reported — clarified (third example only: a statement added inside `Main`)

## Examples for the tests
- RH4001-007, third example (`FileWithStatementsOnlyInsideMainAndMatchingFirstTypeIsNotReported`)

## Output questions
_None._

## Internal choices
- Added a statement to the example instead of rewording the sentence to "whose entry point is `Main`". The sentence names the boundary of the requirement (statements inside a type member), and an example that actually holds such a statement shows that boundary.

## Follow-up candidates
_None._
