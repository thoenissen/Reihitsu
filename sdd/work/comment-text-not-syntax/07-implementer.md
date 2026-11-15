Role: implementer
Commit: e43cec7 (base; production change and this handover in the commit that adds this file)
Verdict: tests pass

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## thoenissen/Reihitsu#966

### Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/UsingDirectiveOrderingSafety.cs` | `CanSafelyReorder` now decides on trivia kinds: a block is unsafe when any of its using directives `ContainsDirectives` (Roslyn's node flag, which covers the leading and trailing trivia of every token of the directive, so also directives between its tokens — matrix B1–B3). The textual regex `_preprocessorDirectiveRegex`, `ContainsPreprocessorDirective`, and the edge-only `HasUnsafeTrivia` are removed, as are the now-unused `System` and `System.Text.RegularExpressions` usings. XML summary rewritten to state the new decision (FC-031). | SHARED-USING-009 |

Why `ContainsDirectives` per directive meets the requirement exactly: a directive after the last using stands in the leading trivia of the next declaration's first token, never in the `;`'s trailing trivia, so it is outside every `UsingDirectiveSyntax` and does not count; a directive in the leading trivia of the first using (file header, `#nullable enable`) and between tokens are inside the node and count; a `#`-led line of a `/* */` or `/** */` comment is comment trivia, not directive trivia, and does not count. Disabled text only exists next to a directive, so it needs no check of its own.

All callers (RH7201–RH7207 analyzers, `UsingDirectiveOrderingCodeFixProviderBase`, the RH7207 provider, `UsingDirectiveOrderingRewriter`) keep calling the one helper unchanged.

A9 (`#else` line in a comment that starts inside a conditional group) now gets its diagnostic; the fix stays withheld and the formatter leaves the document unchanged through the existing FMT-PIPE-033 / SHARED-FIX-012 path — no change was needed for it.

### Policy owners

- `rg -n --type cs 'CanSafelyReorder' src -g '!*Test*'`: the one owner `UsingDirectiveOrderingSafety.CanSafelyReorder`; callers listed above.
- `rg -n --type cs '\\s\*#' src -g '!*Test*'`: no other textual directive check remains.
- `rg -n --type cs 'ContainsDirectives' src -g '!*Test*'`: other uses (`OrderingMoveSafety`, `SyntaxTriviaUtilities.ContainsDirectives`, `ReihitsuFormatterHelpers`, …) are their own decisions over other spans; none decides using-block safety.

### Tests run

```text
scripts/test.sh --configuration Debug --project core --filter "FullyQualifiedName~Reihitsu.Core.Test.UsingDirectiveOrderingSafetyTests"   Passed 30, Failed 0
scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Regression.UsingDirectives"  Passed 79, Failed 0
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH720"   Passed 274, Failed 0
scripts/test.sh --configuration Debug --project core          Passed 682, Failed 0
scripts/test.sh --configuration Debug --project formatter     Passed 5016, Failed 0
scripts/test.sh --configuration Debug --project analyzer      Passed 9395, Failed 0
scripts/test.sh --configuration Debug --project cli           Passed 279, Failed 0
scripts/test.sh --configuration Debug --project architecture  Passed 118, Failed 0
```

`scripts/format.sh` over the changed file: reformatted (final newline only).

## thoenissen/Reihitsu#1289

### Changes

_None._ Under the maintainer's answer A the class stays reported; the test implementer's RH2109 tests already pass on the unchanged analyzer (included in the analyzer run above).

### Policy owners

_N/A._

### Tests run

Covered by `scripts/test.sh --configuration Debug --project analyzer` above (all RH2109 tests pass).

## Whole run

### Production diff size

`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'`: 1 file changed, 6 insertions(+), 41 deletions(-). No new type, member, or dependency.

### Deleted special cases

_N/A._ (No shape declared unsupported.)

### Disputed tests or scope

_None._

### Architecture findings

_None._

### Follow-up candidates

_None_ new (the RH7203 block-namespace indentation candidate from `05-test-implementer.md` stands; this change does not touch it).
