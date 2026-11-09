Role: spec author
Commit: e608f81
Verdict: open question

This is the amendment round. Batch 1 (Q1–Q8) and the lead's Decisions 2–4 are carried out in the specs, the documentation, and the change doc (`sdd/changes/2026-11/unsupported-precheck.md`, still `Status: Draft`). Answer B to question 5 opens two positions at the edge of the span that the answer does not decide (questions 9 and 10 below). FMT-UNSUP-004 states that it leaves them undecided, and no example depends on them. The test implementer can write every test except the boundary tests for those two positions. The implementer needs the answers before writing the predicate.

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Changed requirements

- FMT-UNSUP-001 — An unsupported input is refused as a whole — changed. Adds `FormattingResult<T>` (`Output`, `Unsupported` of type `UnsupportedSyntax` with reason and location) for every formatting entry point, null `Unsupported` for FMT-PIPE-005/006, and the first shape in text order (skeptic defect 6).
- FMT-UNSUP-002 — The precheck runs on exactly what the phases receive — unchanged.
- FMT-UNSUP-003 — The decision is local to the input — clarified. Whole-document and node-level formatting agree for the same node, not for a node below the shape (skeptic defect 1).
- FMT-UNSUP-004 — retitled to "Comments inside a variable declaration are unsupported" — changed. The span runs from the first token, attributes and modifiers included, through the terminator. Exceptions: the leading trivia of the first token, the trailing trivia of `;`, and the inside of a value. Also: nested declarations in a value are checked on their own (defect 4), fixed-buffer brackets are in the span (defect 5), the reason text is stated, and two positions are left open.
- FMT-UNSUP-005 — Formatted output is never unsupported — changed (the example uses a comment inside the value).
- FMT-UNSUP-006 — A phase's refusal gives the same outcome — changed (one catch site where the precheck's outcome is produced, and no wrapping under an observer or the trace; Decision 3).
- FMT-UNSUP-007 — The formatter answers whether it refuses a node — added (`FindUnsupportedSyntax`, the same predicate as the precheck; Q8).
- FMT-PIPE-017 — the example text for the equals-value clause target is corrected (defect 1).
- FMT-PIPE-031 — changed. The FMT-UNSUP-004 clause is dropped (Decision 2), and so is the repeated-part condition. Examples were updated.
- FMT-STRUCT-022 — changed. Only leading trivia, the inside of a value, and the trivia behind `;`. A comment in front of `;` is now refused (defect 3, resolved by Q5 B).
- FMT-STRUCT-023 — clarified (Why no longer says the split rebuilds trivia from comments).
- FMT-STRUCT-030 — retitled to "A documented field is not split" — changed. The repeated-part condition is removed, and so are the examples with `/* c */` and `[Obsolete /* why */]`.
- FMT-INDENT-015, FMT-BLANK-018, FMT-SPACE-012, FMT-SPACE-014, FMT-DOC-003 — clarified. The examples now use a construct without a variable declaration, with the same output, verified with the CLI built from `main`.
- RH2001-007 — clarified (comments the field would carry withhold through SHARED-FIX-010).
- RH2001-009 — retitled and changed. Every comment among the modifiers or in front of the type withholds the fix. The comment-crossing and `readonly`-behind-comment rules are gone. The directive rules are unchanged (Q7).
- RH7101-004 — the example uses a comment inside the value.
- RH7101-006 — clarified (defect 2).
- RH7101-007 — retitled to "The fix is withheld for a comment inside the declaration" — changed (through FMT-UNSUP-007 and SHARED-FIX-010).
- SHARED-FIX-010 — changed. The query at registration targets exactly the node, the context, or the built node. Adds RH2001 and RH5303 examples (Q7, Q8).
- CLI-014 — changed (`Skipped <n> file(s) with unsupported syntax.`).
- CLI-017 — changed (exit code 2 for an unsupported file without the flag; it outranks 1).
- CLI-019 — changed (message, stream, location, summary, `--check`/`--dry-run`/`--verbose`, exit code; Q2–Q4).
- CLI-020 — changed (`warning:` line).
- PLAY-008 — Input with unsupported syntax is shown unchanged with a message — added (Q6).

Ledger: FMT-UNSUP-007 and PLAY-008 were added, and the ledger is sorted. `sdd/specs/README.md` needs no new row. `Uses` rows of every touched spec were checked mechanically and are exact.

Documentation: `src/Reihitsu.Cli/README.MD` (exit-code table) and `AGENTS.md` (precheck row of the ownership table). No rule page changes, because a withheld fix is covered by `general-notes.md`.

Checks: `scripts/test.sh --project architecture --filter "...Spec|...RuleDocumentation|...Requirement"` gives 50 passed and 1 failed. The failure is `EveryRequirementOfAnEnforcedSpecIsCovered`, for exactly the 11 new or untested IDs (CLI-019, CLI-020, FMT-UNSUP-001 to 007, PLAY-008, SHARED-FIX-010).

## Examples for the tests

FMT-UNSUP-001 (three examples), FMT-UNSUP-002, FMT-UNSUP-003, FMT-UNSUP-004 (five examples: the issue's cases, Q5 a–d with attributes and modifiers, the other side, a nested declaration, every declaration kind), FMT-UNSUP-005, FMT-UNSUP-007, FMT-STRUCT-022, FMT-STRUCT-030, FMT-PIPE-031, RH2001-009, RH7101-004, RH7101-007, SHARED-FIX-010 (RH7101, RH2001, RH5303), CLI-019, CLI-020, PLAY-008. FMT-UNSUP-006 is tested through `ExecutePhases` with a refusing test phase, with and without an observer.

Tests with a shape that FMT-UNSUP-004 now refuses, found by a Roslyn scan of string literals (categories beyond the 01 list): `FieldDeclarationSplitTests`, `ReihitsuFormatterSplitsFieldDeclarationTests`, `RH7101DoNotCombineFieldsAnalyzerTests`, `RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests` (modifiers), `HorizontalSpacingRequirementTests` (fixed buffer and `/** x */;`), `HorizontalSpacingTests`, `WrappedAngleBracketListTests`, `ListAndHeaderAlignmentTests`, `NestedListInParameterAlignmentTests`, `ArgumentContinuationLineAlignmentTests`, `ParameterContinuationLineAlignmentTests`, `StrandedDocumentationLineStartTests` (comment inside a type), `CommentLedFirstTokenAnchorTests`, `GluedCommentBlankLineTests`, `MultiLineGluedCommentBlankLineTests`, `CommentBeforeBraceBlankLineTests`, `LeadingTriviaMultiLineDetectionTests`, the RH5301/RH5303/RH5305 `FormatterTests` (comment in front of a value), `DocumentationCommentRequirementTests`, `StrandedDocumentationTokenSpacingTests`, `TrailingDocumentationCommentPreservationTests`, `RH6003SemicolonsMustBeSpacedCorrectlyFormatterTests` (comment in front of `;`), and `DocumentationCommentCollapseGuardTests` (attribute). Hits in tests of analyzers without a formatting fix (RH2005, RH3103, RH5035, RH5041, RH5042, RH5517–RH5520) are unaffected. The RH7004/RH7105/RH7106 hits need checking against SHARED-FIX-010. Whether a hit is a whole-pipeline input or a node target below the declaration decides whether it changes (FMT-UNSUP-003).

## Output questions

**9 · Formatter — a comment between an initializer value and the `,` or `;` behind it**

Input:
```csharp
class C
{
    void M()
    {
        var timeout=30 /* seconds */;
        int b=1 /* c */, c=2;
    }
}
```

Before (`main`):
```csharp
class C
{
    void M()
    {
        var timeout = 30 /* seconds */;
        int b = 1 /* c */, c = 2;
    }
}
```

After, A (recommended): the input unchanged, reported as unsupported.

After, B: as before.

Why A: answer 5 refused a comment in front of the value and one in front of the `;`, and A refuses this mirror gap as well. Under B, `int a = 1 /** c */;` would be refused when documentation comments are parsed (the comment then leads the `;`) and formatted when they are not (it then trails `1`). The CLI and a code fix would then disagree for the same text. Neither option deletes more code. B keeps a comment people do write, such as `= 30 /* seconds */;`, formatted.

**10 · Formatter — a comment on its own line in front of the `)` of a `using` or `fixed` statement**

Input:
```csharp
class C
{
    void M()
    {
        using (var s = Open()
            // c
            )
        {
        }
    }
}
```

Before (`main`):
```csharp
class C
{
    void M()
    {
        using (var s = Open()

        // c
        )
        {
        }
    }
}
```

After, A (recommended): the input unchanged, reported as unsupported.

After, B: as before.

Why A: the `)` ends this declaration as the `;` ends a local's declaration, and answer 5 refused a comment in front of the `;`. This keeps the check to one rule: everything from the first token through the terminator.

## Internal choices

- Names: `FormattingResult<T>` with `Output` and `Unsupported`, `UnsupportedSyntax` with the reason and the location (a span in the caller's syntax tree), and `ReihitsuFormatter.FindUnsupportedSyntax(SyntaxNode)`. The alternative is other names. Q1's option showed `result.Output` and `result.Unsupported`.
- `OrganizeUsingDirectivesInDocumentAsync` and `SplitsFieldDeclaration` keep their return types. The alternative is to wrap them as well. They refuse nothing.
- The location is the comment's span, and one-based line and column are derived by the host. The alternative is to store line and column in the result.
- Under Q5 B, attribute arguments count as part of the span, not as initializer values (`[Obsolete("x" /* c */)] private int _a;` is refused). The recorded answer says "attributes/modifiers included", and an attribute argument is no initializer value. If the lead reads this as a visible output still in question, it is question 11, with the same shape as 9.
- For a `for` initializer, the terminator is the `for` statement's first `;`. The answer's "the `;`" applies to it directly.
- For `using` and `fixed` statements, the span starts at the type's first token: the `using`/`fixed` keyword and `(` are not the declaration's tokens. Only a local declaration statement's own `using`, `await`, and `const` are in its span.
- `--verbose` prints no extra line for an unsupported file. The alternative is a `Skipped (unsupported syntax): <path>` line. Q4's answer gives the file one report line in every mode, and I read that as covering this. The lead can turn it into an output question.
- The summary line goes after `Skipped <n> file(s) with syntax errors.`. The alternative is another position.
- CLI-018's confirmation count stays unchanged. It counts selected files before any is formatted, as it does for files with syntax errors. The skeptic's suggestion to exclude refused files would need formatting before the prompt.
- RH7101-007 is kept as a rule-specific pointer requirement (via FMT-UNSUP-007 and SHARED-FIX-010), not retired, so the existing RH7101-007 tests keep their tag. The alternative is to retire it and move the examples under SHARED-FIX-010.
- RH2001-009 is rewritten, not retired: its directive rules stay, and its comment rules now follow from SHARED-FIX-010.
- Spec examples of other requirements that held a refused shape incidentally now use an expression-bodied property or a fixed buffer without the comment. The alternative is to drop the examples.

## Follow-up candidates

- The skeptic's new-rule candidate stands: no analyzer reports the unsupported shape. A rule would have to share the formatter's predicate, and the analyzer assembly does not reference the formatter. This is the maintainer's call, outside this run.
- `ModifierOrderingUtilities.TryWriteModifiersInOrder`'s comment handling is still reachable from RH7105 and RH7106 on members that are not variable declarations, and from no field. No action is needed. Noted for whoever later widens the precheck to other members.
