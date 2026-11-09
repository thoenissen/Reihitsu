Role: spec author
Commit: 0e7c7a5
Verdict: open question

The run cannot write the CLI, entry-point, and Playground tests without the answers to questions 1–4 and 6, and the boundary tests of FMT-UNSUP-004 without question 5. The specs carry only what is decided; the change doc (`sdd/changes/2026-11/unsupported-precheck.md`, `Status: Draft`) lists the open points under *Open — not decided by this change doc yet*.

The prompt carried no intended fix or suspected cause; the maintainer's binding clarifications in `00-lead.md` (decisions 1–6, the issue's contract, the deletion acceptance criterion) were applied as binding.

## Changed requirements

New spec `sdd/specs/formatter/unsupported-syntax.md` (prefix FMT-UNSUP, row added to `sdd/specs/README.md`, IDs added to the ledger):

- FMT-UNSUP-001 — An unsupported input is refused as a whole — added
- FMT-UNSUP-002 — The precheck runs on exactly what the phases receive — added
- FMT-UNSUP-003 — The decision is local to the input — added
- FMT-UNSUP-004 — Comments between a variable declaration's own tokens are unsupported — added
- FMT-UNSUP-005 — Formatted output is never unsupported — added
- FMT-UNSUP-006 — A phase's refusal gives the same outcome — added

Other specs:

- FMT-PIPE-001 — Fixed phase order — changed (precheck phase as row 0, before the preparation phase)
- FMT-PIPE-027 — No phase changes the formatter's output — changed (a refused input is not formatted output)
- FMT-PIPE-029 — The context entry point writes back every field the split makes of its target — changed (no comment lines between fields; first example without the comma and later-declarator comments; the comma-comment example removed; the RH7101 input is one whose context the formatter does not refuse)
- FMT-PIPE-031 — The formatter answers whether it splits a field declaration — changed (false for a declaration FMT-UNSUP-004 refuses; the context is not part of the answer; examples replaced)
- FMT-STRUCT-022 — Split fields keep their comments — changed (only leading trivia, comments inside values, and trivia behind `;` remain)
- FMT-STRUCT-026 — The phase writes only the separators its text needs — changed (the field split's comment separators and the comma-comment example removed)
- FMT-STRUCT-030 — A documented field, or one with a comment in the part every field repeats, is not split — changed (the first-declarator boundary and its documentation-mode rules removed; the guard covers attributes, modifiers, and the type)
- RH7101-004 — The fix splits the declaration — changed (comma and later-declarator comment examples replaced)
- RH7101-006 — The fix is withheld for a documented declaration — changed (a doc comment in front of a later declarator now withholds the fix through FMT-UNSUP-004; that example removed)
- RH7101-007 — The fix is withheld for a comment in the part every field repeats — changed (examples for the declaration's own tokens now withheld; first-declarator examples removed)
- SHARED-FIX-010 — A fix whose formatting the formatter refuses is withheld — added
- CLI-001 — Arguments are options and paths — changed (`--suppress-unsupported`)
- CLI-019 — An unsupported file is never changed and is reported as an error — added
- CLI-020 — `--suppress-unsupported` reports an unsupported file as a warning — added

Documentation: `src/Reihitsu.Cli/README.MD` (option row; the exit-code table waits for question 3), `AGENTS.md` (phase order sentence, ownership-table row for the precheck phase, the "leave alone" convention). No rule page changes; `documentation/rules/general-notes.md` already covers a withheld fix and comments blocking one, so it is left alone.

Checks run: `scripts/test.sh --project architecture --filter "FullyQualifiedName~Spec|FullyQualifiedName~RuleDocumentation|FullyQualifiedName~Requirement"` — 50 passed, 1 failed: `EveryRequirementOfAnEnforcedSpecIsCovered`, for exactly the nine new IDs (CLI-019, CLI-020, FMT-UNSUP-001 to 006, SHARED-FIX-010), which have no tests yet. The ledger is sorted (`LC_ALL=C sort -c`).

The `before` outputs in the change doc were produced with the CLI built from `0e7c7a5` (`Reihitsu.Cli.dll --force` on scratch copies), not reasoned. They show that `main` formats cases 3 and 5 as `int c =// c3` with `1;` at the statement's column and `int f, // c5` with `g;` at the statement's column.

Spec examples that the first shape affects were enumerated mechanically: a Roslyn scan of every `csharp` block under `sdd/specs/` for comments in the trivia of the type tokens, declarator names, `=`, and `,` of each `VariableDeclarationSyntax`, in the documentation modes `Parse` and `None`. Formatter and code-fix examples that the decided part of FMT-UNSUP-004 refuses were rewritten above. Remaining hits:

- analyzer examples whose rule calls no formatter (RH3001, RH3103, RH3204, RH5035, RH5039, RH5041, RH5042, RH5104) or that already state "no code fix is offered" (RH5111-005): unchanged.
- FMT-PIPE-017's `var d = /* a …` example: its target is the equals-value clause, which holds no variable declaration, so FMT-UNSUP-003 keeps it formatted; RH5303 and RH5304 examples of the same shape follow FMT-PIPE-017.
- FMT-SPACE-014 (`int _a /** x */;`) and FMT-DOC (`int A /// stray`): refused only in the documentation mode `None`, where the comment is trailing trivia of the name; in `Parse` it leads the `;`. Unchanged.
- Depend on question 5 and are left unchanged until it is answered: FMT-INDENT-015 (`System.Collections.Generic.Dictionary<int, // first` / `string> _map;`, a comment inside the type), FMT-BLANK example at `formatter/blank-lines.md` (`int[] G =` / `/* b */ { 1, 2 };`, a comment leading the value), RH2005 and SHARED-MODORDER examples with a comment on its own line between a modifier and the type (no formatter call; diagnostics unchanged either way).

Tests that carry a changed requirement's tag and may assert the old field-split comment behavior: `FieldDeclarationSplitTests`, `ReihitsuFormatterFieldSplitTests`, `ReihitsuFormatterSplitsFieldDeclarationTests`, `StructuralTransformRequirementTests`, `StructuralTransformSeparatorTests`, `StructuralTransformSeparatorFinalOutputTests`, `RH7101DoNotCombineFieldsAnalyzerTests`, `RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzerTests`, `ArrowSpacingTests`, `ControlFlowBraceTransformTests`, `ExpressionBodiedTransformTests` (the last ones tag FMT-STRUCT-026 for other transforms).

## Examples for the tests

FMT-UNSUP-001, FMT-UNSUP-002, FMT-UNSUP-003, FMT-UNSUP-004 (three examples: refused, not refused, every declaration kind), FMT-UNSUP-005, FMT-STRUCT-022, FMT-STRUCT-026 (field-split sentence), FMT-STRUCT-030, FMT-PIPE-029 (first example), FMT-PIPE-031, RH7101-004, RH7101-006, RH7101-007, SHARED-FIX-010. CLI-019 and CLI-020 after questions 2–4 are answered. FMT-UNSUP-006 has no throw site in this change and no example; its covering test needs a phase that refuses, which no production phase does.

## Output questions

**1 · Formatter entry points — what they return for an unsupported input**

Input (`FormatSyntaxTree`, also `FormatDocumentAsync`, `FormatNode`, `FormatNodeInDocumentAsync`, `FormatNodeInDocumentWithContextAsync`):
```csharp
class C
{
    void M()
    {
        int /* c */ a = 1;
    }
}
```

Before (`main`): the formatted tree, of type `SyntaxTree`.

After, A (recommended):
```csharp
// every formatting entry point returns a result object instead of the bare Roslyn object
FormattingResult<SyntaxTree> result = ReihitsuFormatter.FormatSyntaxTree(tree);
// result.Output      — the formatted tree, or the input unchanged
// result.Unsupported — null when formatted; otherwise the reason and the location (line, column)
```

After, B:
```csharp
// the existing methods keep their return types and return the input unchanged, silently
SyntaxTree output = ReihitsuFormatter.FormatSyntaxTree(tree);
// a parallel method carries the outcome
bool formatted = ReihitsuFormatter.TryFormatSyntaxTree(tree, out SyntaxTree output, out UnsupportedSyntax unsupported);
```

Why A: one method per entry point, and no caller can ignore the outcome by calling the old method; the CLI, the Playground, and the code fixes all have to report or withhold on it.

**2 · CLI — message for an unsupported file**

Input: `reihitsu-format /work/u.cs`, where `u.cs` holds the code of question 1 (comment at line 5, column 13).

Before (`main`):
```text
Formatted: /work/u.cs

Formatted 1 of 1 file(s).
```

After, A (recommended) — standard error, in the `path(line,col)` form that CI logs and IDEs link, and one summary line:
```text
error: /work/u.cs(5,13): unsupported syntax: a comment between a variable declaration's own tokens. The file was left unchanged.

Formatted 0 of 1 file(s).
Skipped 1 file(s) with unsupported syntax.
```
With `--suppress-unsupported`, the first line starts with `warning:` instead of `error:`.

After, B — standard output, like the other skip lines (CLI-015):
```text
Skipped (unsupported syntax): /work/u.cs (line 5, column 13): a comment between a variable declaration's own tokens

Formatted 0 of 1 file(s).
Skipped 1 file(s) with unsupported syntax.
```
With `--suppress-unsupported`, the line ends with ` (warning)`.

Why A: an error is expected on standard error, and the `path(line,col)` form takes the user to the comment in one click.

**3 · CLI — exit code for an unsupported file without `--suppress-unsupported`**

Input: `reihitsu-format /work/u.cs` (as in question 2), and `reihitsu-format --check /work/u.cs /work/A.cs` with an unformatted `A.cs`.

Before (`main`): 0 for the first command (the file is formatted), 1 for the second.

After, A (recommended): 2 (`ExitCodes.Error`) for both commands; the error outranks "needs formatting".

After, B: a new code 3 for both commands, meaning "unsupported syntax found"; 2 keeps meaning "the tool failed".

Why A: CLI-017 already says 2 is the run that must not pass, and `--suppress-unsupported` is the way to let it pass; a new code adds a contract CI scripts would have to learn for little gain.

**4 · CLI — `--check` and `--dry-run` with an unsupported file**

Input: `reihitsu-format --check /work/u.cs /work/B.cs` and `reihitsu-format --dry-run /work/u.cs /work/B.cs`, `B.cs` already formatted.

Before (`main`): `Not formatted: /work/u.cs` / `1 of 2 file(s) need formatting.` (and `Would format:` with a diff for `--dry-run`), exit 1.

After, A (recommended): the same error (or warning) line as in write mode (question 2), no `Not formatted:` / `Would format:` line and no diff for `u.cs`, and it is not counted as needing formatting:
```text
error: /work/u.cs(5,13): unsupported syntax: a comment between a variable declaration's own tokens. The file was left unchanged.

0 of 2 file(s) need formatting.
Skipped 1 file(s) with unsupported syntax.
```

After, B: additionally list it as `Not formatted: /work/u.cs` and count it in `1 of 2 file(s) need formatting.`

Why A: the file does not need formatting in a way the tool can apply; counting it as such would make `--check` and write mode disagree about the same file.

**5 · Formatter — where the first shape ends (FMT-UNSUP-004)**

Input:
```csharp
class C
{
    System.Collections.Generic.Dictionary<int, // first
        string> _map;           // (a) comment inside the type

    private
        // c
        int _b;                 // (b) comment on its own line between a modifier and the type

    void M()
    {
        int c =
            // c
            1;                  // (c) comment on its own line in front of the value
        int d
            // c
            ;                   // (d) comment on its own line in front of the `;`
    }
}
```

Before (`main`): all four are formatted (FMT-INDENT-015 aligns `string>` under `int`).

After, A (recommended) — a comment counts when it is trivia of one of the declaration's own tokens, that is, any token of the type and every name, `=`, and `,`, except the leading trivia of the declaration's first token: (a) refuses the input; (b), (c), and (d) are formatted as today, because the comment leads the type's first token, a token of the value, or the `;`.

After, B — a comment counts when it stands anywhere between the declaration's first token and the end of its last declarator, outside initializer values: (a), (b), (c), and (d) all refuse the input.

After, C — only the positions the issue lists: (a) is formatted as today; (b), (c), and (d) as in A.

Why A: it is one cheap check over the tokens the declaration owns, it matches the issue's list ("the type, each declarator's name, `=`, `,`"), and B would refuse shapes the issue excludes, such as a comment between a modifier and the type, which leads a token of the field, not of the variable declaration.

**6 · Playground — an unsupported input**

Input: the code of question 1, pasted and formatted.

Before (`main`): the formatted code.

After, A (recommended): the input unchanged as the output, and below the button, like the syntax-error message of PLAY-006:
```text
The input contains syntax the formatter does not support and was left unchanged: line 5, column 13 — a comment between a variable declaration's own tokens.
```

After, B: the input unchanged as the output, without a message.

Why A: without a message the unchanged output looks like the formatter had nothing to do, the reason PLAY-006 gives for syntax errors.

## Internal choices

- The outcome gets its own spec, `formatter/unsupported-syntax.md` with prefix FMT-UNSUP, instead of requirements in `pipeline.md`; alternative: FMT-PIPE requirements. Each later shape becomes one requirement there.
- Code owners of the new spec are `—` until the types exist; the implementer's types (the precheck phase, `SyntaxNotSupportedException`) have to be added to the header and the index row then. Alternative: name types now, which `SpecConsistencyTests` would fail until they exist.
- The phase is named `PrecheckPhase` in the FMT-PIPE-001 table and in `AGENTS.md`; alternative: another name, changed in both places with the code.
- `OrganizeUsingDirectivesInDocumentAsync` is not checked (FMT-UNSUP-002), because it runs only the using phase; alternative: check its scope.
- `SplitsFieldDeclaration` answers false for a declaration FMT-UNSUP-004 refuses, and does not look at the rest of the type; the RH7101 fix is withheld for a refused type through SHARED-FIX-010. Alternative: keep its answer unchanged and rely on SHARED-FIX-010 alone.
- FMT-STRUCT-030's remaining guard is written so that it holds whatever question 5 decides: every comment in the trivia of a token in front of the first declarator's name, other than the declaration's leading trivia.
- The written-back leading and trailing trivia of node-level targets is not covered by the precheck, because the first shape excludes both (change doc, *Decisions*).
- SHARED-FIX-010 is a shared requirement that every fix gets without listing; RH7101 references it where its examples are withheld through it.
- FMT-UNSUP-004 names `fixed` statements and `using` statements and declarations among the declaration kinds, since they hold a `VariableDeclarationSyntax` too.
- `AGENTS.md` is updated (a tooling path, admitted by the behavior-change lane) so its phase list and ownership table name the new phase.

## Follow-up candidates

_None._
