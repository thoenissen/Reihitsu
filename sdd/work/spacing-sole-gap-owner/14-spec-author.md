Role: spec author
Commit: 6b947fc
Verdict: spec written

Repair round 2, auditor Finding #1 (`13-auditor.md`), spec part only. The lead reads answer 7C ("every declared name") as covering fields and the other variable-declaration forms (`00-lead.md`, row 25), so no behavior changes and no output question arises. The prompt carried no intended fix or hoped-for verdict beyond the finding's own suggested change.

## Changed requirements

- FMT-SPACE-021 — One space between a type or pattern and the name it declares — clarified. The scope sentence is now an exhaustive list of the declarations the rule covers, matching `DeclaredNameSpacingRule` arm by arm:
  - a parameter that has a type, of every parameter list;
  - a named tuple element;
  - every variable declaration: a field (including a `const` field and a fixed-size buffer), an event field, a local (including `const`, `ref`, and a `using` declaration), and the declaration of a `for` initializer, a `fixed` statement, and a `using` statement;
  - a declaration expression and a declaration pattern;
  - a `foreach` variable, a `catch` declaration, and a `from` or `join` range variable;
  - the designation behind a recursive or list pattern.

  The exclusion paragraph still leaves the return type of a method unspecified, as well as the return types of local functions and delegates, the type of a property and an indexer, and the type of an event with an accessor list. It says explicitly that a field and an event field are covered and that a property, an indexer, and an event with accessors are not. The paragraph now also names the three remaining gaps in front of a declared name, each with the rule that owns it:
  - `var (x, y)` belongs to FMT-SPACE-024;
  - a name inside a parenthesized designation belongs to FMT-SPACE-004 and FMT-SPACE-008;
  - the designation of a `var` pattern `o is var v` is decided by no rule, and FMT-SPACE-002 leaves one space because two identifiers cannot touch.

  A second example covers a field, an event field, a fixed-size buffer (`fixed int  _buf[4];` becomes `fixed int _buf[4];`), and the `fixed`, `for`, `using` statement, and `using` declaration forms. No requirement ID was added, so `requirement-ids.txt` and `sdd/specs/README.md` are unchanged.
- Change doc `sdd/changes/2026-11/spacing-sole-gap-owner.md`, entry 6. I added a before/after for a field, an event field, `fixed`, `for`, a `using` statement, and a `using` declaration. A closing line states that the types of properties, indexers, events with accessors, methods, and local functions stay as written (`int?P { get; set; }`, `int[]Q() => null;`).

### Completeness check

Roslyn's `VariableDeclarationSyntax` has these parents, and each one is now named:
- `FieldDeclarationSyntax`;
- `EventFieldDeclarationSyntax`;
- `LocalDeclarationStatementSyntax`, including `using` declarations and `const`;
- `ForStatementSyntax`;
- `FixedStatementSyntax`;
- `UsingStatementSyntax`.

`SingleVariableDesignationSyntax` and `DiscardDesignationSyntax` can sit under four parents that the rule covers, and each is named: `DeclarationExpressionSyntax`, `DeclarationPatternSyntax`, `RecursivePatternSyntax`, and `ListPatternSyntax`. Two other parents fall outside the rule, and each is now named as outside it: `VarPatternSyntax` and `ParenthesizedVariableDesignationSyntax`. The `var (` gap in front of a `ParenthesizedVariableDesignationSyntax` is owned by FMT-SPACE-024.

### Verification

Both new examples were run through the branch CLI (`scripts/format.sh`, at 6b947fc), and each output matches byte for byte apart from the final newline. The change-doc input also went through the CLI built from `origin/main` (`e3842c8`) in a scratch worktree, which I have since removed. It came back unchanged, so "Before: unchanged" holds. On `main`, the spec example's `fixed int  _buf[4]` already collapses to one space through FMT-SPACE-002, which is why I left it out of the change-doc input. Probes on the branch confirmed the stated exclusions:
- `int?P { get; set; }`, `int[]Q()`, the local function `int?L()`, and `event Action?E { add … }` keep their tight gap;
- `var  v` becomes `var v`;
- `var(a, b)` in a `foreach` and in a pattern becomes `var (a, b)`, which is FMT-SPACE-024.

## Examples for the tests

- FMT-SPACE-021: the new second example (field, event field, fixed-size buffer, `fixed`, `for`, `using` statement, `using` declaration). Finding #1 asks for full-pipeline `AssertRuleResult` tests of these members. The first example is unchanged.

## Output questions

_None._ Answer 7C covers the field output (lead, row 25), and every other listed form is the output the branch already gives.

## Internal choices

- The `var` pattern designation is listed as decided by no rule, with FMT-SPACE-002 giving one space, rather than being added to FMT-SPACE-021. Alternative: widen the requirement and the rule to `VarPatternSyntax`. The visible output is identical either way, so the narrower spec matches the code as it stands.
- Properties are not in the spec's new example, so a test cannot pin a type the requirement leaves unspecified. The change doc names them only in prose.

## Follow-up candidates

_None._
