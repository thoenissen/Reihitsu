# Beta Code Review — Session Prompts

The solution-wide beta review is executed in batches across multiple sessions. Session 1 covered Core, Cli, Formatter, the Analyzer infrastructure, and all Analyzer rule categories except Layout. Run the prompts below one per session, in order. Each session appends to `plans/review/findings.md`, ticks `plans/review/coverage.md`, and commits to the branch `review/beta-code-review`.

Shared rules for every session:

- The review criteria are in `plans/review/criteria.md`. The severity scale and finding format are defined there.
- Findings only — no positive feedback. A file without findings gets no entry.
- The review is read-only: do not change production code. Only `plans/review/*` may be modified.
- Read `findings.md` first so already-reported systemic issues are not duplicated; extend the affected-file list of an existing finding instead.
- For heavily patterned directories: read every file, but report repeated systemic issues once with the list of affected files.

---

## Session 2 — Analyzer rules: Layout (102 files)

```
Continue the beta code review on branch review/beta-code-review (git switch review/beta-code-review first).

Scope: all unticked files ([ ]) in plans/review/coverage.md under Reihitsu.Analyzer\Rules\Layout (102 files).

Procedure:
1. Read plans/review/criteria.md (sections A and C apply) and the existing findings in plans/review/findings.md, especially the systemic findings about DiagnosticAnalyzerBase, blank-line definitions, and analyzer/formatter policy divergence.
2. Read every unticked Layout rule file. Most derive from reviewed base classes (StatementShouldBePreceded/FollowedBy, TargetAttributePlacement/ListShape, EmptyTypeDeclarationShouldUseSemicolon) — for those, check only the rule-specific overrides. Read the unique mid-size rules fully (RH5020-5030, RH5101-5112, RH5201-5206, RH5301-5307, RH5401-5408, RH5601-5604).
3. Watch especially for: divergence between a Layout rule's detection logic and the corresponding formatter phase (Pipeline\LineBreaks, BlankLines, Indentation, HorizontalSpacing) — files the formatter produces must not be flagged, and vice versa; duplicated detection logic that belongs in Reihitsu.Core; GetText()/ToString() allocations per node; wrong diagnostic locations.
4. Append findings to plans/review/findings.md under a new subsection "### Rules: Layout (batch 14)". Tick all reviewed files in plans/review/coverage.md.
5. Commit both files to the branch with message "Add beta review findings: Analyzer Layout rules".

The review is read-only apart from plans/review/*. No positive feedback — findings only.
```

## Session 3 — CodeFixes: infrastructure, Clarity, Design, Documentation (~48 files)

```
Continue the beta code review on branch review/beta-code-review (git switch review/beta-code-review first).

Scope: all unticked files in plans/review/coverage.md under Reihitsu.Analyzer.CodeFixes\Base, \Core, \Properties, \Rules\Clarity, \Rules\Design, \Rules\Documentation.

Procedure:
1. Read plans/review/criteria.md (sections A and D apply) and the existing findings in plans/review/findings.md.
2. Read every unticked file. Check per code fix: semantic preservation (comments/trivia must not vanish), comprehensiveness (fix handles every shape the analyzer flags — repo contract: comprehensive fix or no fix), FixAll/equivalence keys, formatting delegation to ReihitsuFormatter.FormatNodeInDocumentAsync/FormatNode instead of manual trivia surgery, robustness on incomplete code, cancellation.
3. Resolve these open cross-checks from findings.md while in this area:
   - The RH3202/RH3203 fixes (expression-body to block): do they produce `return throw ...;` like the formatter's ExpressionBodyToBlockConverter does (Critical there)? Verify and extend that finding with the code-fix side.
   - The interpolation-marker fix (RH3204): shares Core StringInterpolationUtilities — check IsMissing vs ContainsDiagnostics handling.
4. Append findings to plans/review/findings.md under "## Reihitsu.Analyzer.CodeFixes" / "### Infrastructure, Clarity, Design, Documentation (batch 15)". Tick reviewed files in coverage.md.
5. Commit with message "Add beta review findings: CodeFixes infrastructure and first categories".

The review is read-only apart from plans/review/*. No positive feedback — findings only.
```

## Session 4 — CodeFixes: Layout, part 1 (first ~53 files)

```
Continue the beta code review on branch review/beta-code-review (git switch review/beta-code-review first).

Scope: the first half (alphabetical order) of the unticked files in plans/review/coverage.md under Reihitsu.Analyzer.CodeFixes\Rules\Layout. Stop after ~53 files; the rest is session 5.

Procedure:
1. Read plans/review/criteria.md (sections A and D) and the existing findings, especially the formatter Critical findings (comment swallowing on line joins, comment deletion on brace insertion) — code fixes that share those helpers inherit those bugs; extend the affected-file lists.
2. Read every file in scope. Same checklist as session 3 (semantic preservation, comprehensiveness, FixAll, formatting delegation, robustness).
3. Append findings under "### Rules: Layout fixes, part 1 (batch 16)". Tick reviewed files in coverage.md.
4. Commit with message "Add beta review findings: CodeFixes Layout part 1".

The review is read-only apart from plans/review/*. No positive feedback — findings only.
```

## Session 5 — CodeFixes: Layout, part 2 (remaining ~53 files)

```
Continue the beta code review on branch review/beta-code-review (git switch review/beta-code-review first).

Scope: all remaining unticked files in plans/review/coverage.md under Reihitsu.Analyzer.CodeFixes\Rules\Layout.

Procedure: identical to session 4 (criteria A and D, extend systemic findings instead of duplicating). Append findings under "### Rules: Layout fixes, part 2 (batch 17)". Tick reviewed files in coverage.md.

Commit with message "Add beta review findings: CodeFixes Layout part 2".

The review is read-only apart from plans/review/*. No positive feedback — findings only.
```

## Session 6 — CodeFixes: Naming, Organization, Spacing (~72 files)

```
Continue the beta code review on branch review/beta-code-review (git switch review/beta-code-review first).

Scope: all unticked files in plans/review/coverage.md under Reihitsu.Analyzer.CodeFixes\Rules\Naming, \Rules\Organization, \Rules\Spacing.

Procedure:
1. Read plans/review/criteria.md (sections A and D) and the existing findings.
2. Read every file in scope. Same checklist as session 3.
3. Resolve these open cross-checks from findings.md:
   - Naming fixes use Core CasingUtilities: ToCamelCase/ToUnderLineCamelCase crash on identifiers like "__" and ToPascalCase can return an empty identifier (Major in Core) — verify whether the RH41xx fixes guard against this; if not, the Core finding escalates to Critical (fix corrupts code).
   - RH7105/RH7106 fixes: ModifierOrderingUtilities moves tokens together with their trivia — verify whether a doc comment or indentation on the first modifier ends up mid-list after reordering (open cross-check in findings.md under Reihitsu.Core).
   - RH7204/RH7207 fixes: confirm which alias-ordering policy each fix applies (Major conflict already recorded in batch 12) and whether running both fixes oscillates.
4. Append findings under "### Rules: Naming, Organization, Spacing fixes (batch 18)". Tick reviewed files in coverage.md. Update or close the "Open cross-checks" section in findings.md.
5. Commit with message "Add beta review findings: CodeFixes Naming, Organization, Spacing".

The review is read-only apart from plans/review/*. No positive feedback — findings only.
```

## Session 7 — Closeout

```
Close out the beta code review on branch review/beta-code-review (git switch review/beta-code-review first).

Procedure:
1. Verify completeness: Select-String -Path plans/review/coverage.md -Pattern '- \[ \]' must return zero matches. If files remain, review them now (criteria in plans/review/criteria.md).
2. Final pass over plans/review/findings.md:
   - Resolve or explicitly close every entry in the "Open cross-checks" section.
   - Deduplicate findings; merge entries that describe the same root cause across assemblies (e.g. the duplicated using-ordering policies, the duplicated GetModifiers switches, the comment-swallowing family).
   - Confirm every finding has a severity tag and a path:line reference.
   - Add a summary section at the top: counts per severity and per assembly, plus the top recommended fix order (Criticals first: expression-body throw conversion, comment swallowing on line joins, switch-brace comment deletion).
3. Verify git status shows no production-code changes (only plans/review/*).
4. Commit with message "Finalize beta code review report".

Afterwards, on request: derive issue drafts in plans/issues/ per finding cluster (separate task, /draft-issue).
```
