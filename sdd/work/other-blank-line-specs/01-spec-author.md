Role: spec author
Commit: 2ece304c86590c33a108ccca50f6accdc5b4ec61
Verdict: spec written

No change doc: every requirement writes down behavior the code has at the commit above (clarifications), so none is required (`sdd/README.md`, *When a change doc is required*). No production code, test, or rule page was touched; the rule pages under `documentation/rules/` already agree with the specs.

## Shared-spec decisions

- **Span-removal fix base (RH5022–RH5028).** No new shared spec. All seven rules rely on the existing SHARED-WS-001 ("the fix deletes exactly the reported whitespace"), which already names "a span of blank lines" in its scope. `BlankLineSpanRemovalCodeFixProviderBase` implements exactly that behavior (it only derives from `WhitespaceSpanRemovalCodeFixProviderBase`), so it was added to the `Code owners` of `shared/whitespace-removal-fixes.md` and its index row. That is a header change only; no SHARED-WS requirement changed. The location of each diagnostic (the whole blank line including its line break) is implemented separately by every analyzer and is stated per rule.
- **Region-directive pair (RH5031/RH5032).** New shared spec `sdd/specs/shared/region-directive-blank-lines.md` (prefix SHARED-REGION), because both specs rely on the same two requirements: the diagnostic location and the inserted line. Code owners are the two bases both rules derive from, `RegionDirectiveBlankLineAnalyzerBase` and `RegionDirectiveBlankLineCodeFixProviderBase`. What is reported is referenced from SHARED-BLANK-005 to SHARED-BLANK-007.
- **RH5011.** `StatementShouldBeFollowedByABlankLineAnalyzerBase<TStatement>` is used only by RH5011 and implements its reporting, so it is a code owner of `analyzer/RH5011.md`.
- No existing requirement of SHARED-BLANK, FMT-BLANK, SHARED-STMT, or SHARED-WS was changed, and no change is needed. Newly referenced from enforced specs, and therefore now enforced: SHARED-BLANK-002, -003, -005, -006, -007 and SHARED-REGION-001, -002 (SHARED-BLANK-001, -004 and SHARED-WS-001 already had covering tests).

## Changed requirements

- RH5011-001 — A `break` followed by a statement or a switch section needs a blank line — added
- RH5011-002 — A `break` before a closing brace or a directive is not reported — added
- RH5011-003 — The diagnostic is on the `break` keyword — added
- RH5011-004 — The fix inserts a blank line below the `break` line — added
- RH5011-005 — A statement on the `break` line is moved below a blank line — added
- RH5011-006 — A comment on the `break` line withholds the same-line fix — added
- RH5020-001 — A comment on its own line needs a blank line above it — added
- RH5020-002 — The first comment of a block, a switch section, or a file is not reported — added
- RH5020-003 — A comment that follows a comment or a directive is not reported — added
- RH5020-004 — The diagnostic is on the comment — added
- RH5020-005 — The fix inserts one empty line above the comment — added
- RH5022-001 — A blank line below an opening brace is reported — added
- RH5022-002 — A brace inside a comment, a string, or disabled code is not considered — added
- RH5022-003 — One diagnostic per brace, on the first blank line — added
- RH5022-004 — The fix deletes the blank line — added
- RH5023-001 — Each blank line after the first of a run is reported — added
- RH5023-002 — Lines of literals, multi-line comments, and disabled code are not considered — added
- RH5023-003 — One diagnostic per surplus line — added
- RH5023-004 — The fix deletes the blank line — added
- RH5024-001 — A blank line above a closing brace is reported — added
- RH5024-002 — Only the line directly above the brace counts — added
- RH5024-003 — A brace inside a comment, a string, or disabled code is not considered — added
- RH5024-004 — One diagnostic per brace, on the line directly above it — added
- RH5024-005 — The fix deletes the blank line — added
- RH5025-001 — A blank line above the opening brace of a body is reported — added
- RH5025-002 — Only the line directly above the brace counts — added
- RH5025-003 — One diagnostic per brace, on the line directly above it — added
- RH5025-004 — The fix deletes the blank line — added
- RH5026-001 — A blank line above a chained keyword is reported — added
- RH5026-002 — Only a keyword that begins its line is considered — added
- RH5026-003 — One diagnostic per keyword, on the line directly above it — added
- RH5026-004 — The fix deletes the blank line — added
- RH5027-001 — A blank line above a `do` statement's footer is reported — added
- RH5027-002 — A `while` statement is not reported — added
- RH5027-003 — One diagnostic per footer, on the line directly above it — added
- RH5027-004 — The fix deletes the blank line — added
- RH5028-001 — Blank lines at the start of a file are reported — added
- RH5028-002 — A file that starts with content is not reported — added
- RH5028-003 — The fix deletes the leading blank lines — added
- RH5030-001 — A statement after a closing brace needs a blank line — added
- RH5030-002 — The diagnostic is on the closing brace — added
- RH5030-003 — The fix inserts the blank line directly before the next statement — added
- RH5031-001 — A region directive needs a blank line above it — added
- RH5031-002 — The diagnostic is on the directive — added
- RH5031-003 — The fix inserts the blank line above the directive — added
- RH5032-001 — A region directive needs a blank line below it — added
- RH5032-002 — The diagnostic is on the directive — added
- RH5032-003 — The fix inserts the blank line below the directive — added
- SHARED-REGION-001 — The diagnostic covers the directive's text — added
- SHARED-REGION-002 — The fix inserts one empty line on the reported side — added

Files: `sdd/specs/analyzer/RH5011.md`, `RH5020.md`, `RH5022.md`–`RH5028.md`, `RH5030.md`–`RH5032.md`, `sdd/specs/shared/region-directive-blank-lines.md`; `sdd/specs/shared/whitespace-removal-fixes.md` (code owner only); `sdd/specs/README.md` (13 rows added, SHARED-WS row's code owners); `sdd/specs/requirement-ids.txt` (50 IDs inserted at their sorted positions, no line moved).

## How the statements were established

- Every example was run, not only read. A scratch harness (outside the repository) extracted each `csharp` block of the 13 new specs, stripped the markup, ran the rule's analyzer through `CodeFixTargetResolver`, compared the reported spans with the markup, and for each before/after pair applied the rule's first code action until the diagnostic disappeared and compared the result with the "after" block. Result: all 44 checked blocks match (positive spans, negative "is not reported" blocks, and every fix result); RH5011-006's block reports and offers no action.
- Formatter agreement was checked with `scripts/trace.sh` where a spec says the formatter makes the same choice (RH5030-003 against FMT-BLANK-005) and for the open questions below.
- `scripts/test.sh --project architecture --filter "FullyQualifiedName~SpecConsistencyTests|FullyQualifiedName~SpecStructureTests"`: 26 passed, 0 failed. `RequirementCoverageTests` will fail until the tests are tagged: the new analyzer requirements and the newly enforced shared IDs listed above have no `[Requirement]` tag yet.

## Examples for the tests

- RH5011-001, RH5011-002, RH5011-004, RH5011-005, RH5011-006
- RH5020-001, RH5020-002, RH5020-003, RH5020-005
- RH5022-001, RH5022-002, RH5022-003, RH5022-004
- RH5023-001, RH5023-002, RH5023-004
- RH5024-001, RH5024-002, RH5024-003, RH5024-005
- RH5025-001, RH5025-002, RH5025-004
- RH5026-001, RH5026-002, RH5026-004
- RH5027-001, RH5027-002, RH5027-004
- RH5028-001, RH5028-002, RH5028-003 (RH5028-001 also states a file holding only whitespace; RH5028-002 an empty file — both verified, no block)
- RH5030-001, RH5030-003; the SHARED-BLANK-003 and SHARED-BLANK-004 examples apply to RH5030
- RH5031-001, RH5031-003, RH5032-001, RH5032-003; SHARED-REGION-001, SHARED-REGION-002; the SHARED-BLANK-005, -006, -007 examples apply to RH5031/RH5032
- The location requirements (RH5011-003, RH5020-004, RH5022-003, RH5023-003, RH5024-004, RH5025-003, RH5026-003, RH5027-003, RH5030-002, RH5031-002, RH5032-002) point at the examples of their rule.
- SHARED-BLANK-002 is used by RH5020-001; its example's comment behind `b */` is the negative side.

## Checked against the already-filed issues

- thoenissen/Reihitsu#949: RH5020 reports the comment that leads a terminal `break` in both shapes — unbraced (`a++;` then `// before break`) and after brace insertion (`}` then `// before break`) — and its fix inserts the blank line above the comment, which agrees with the expected output of #949. The case is left out of the RH5020 spec, as instructed; nothing to file.
- thoenissen/Reihitsu#954, #1000, #1001: none of the new requirements touches them; nothing re-filed.

## Open questions to file

None of these blocks the run; each case is left out of the specs. No `SDD open questions` issue exists yet for any of the four target specs (searched), so each needs a new one under thoenissen/Reihitsu#912.

### Q1 — RH5025 deletes a blank line inside an interpolated raw string

- **Target spec:** `sdd/specs/analyzer/RH5025.md` (new issue `SDD open questions: sdd/specs/analyzer/RH5025.md`).
- **Touches:** RH5025-001, RH5025-004, SHARED-WS-001; FMT-BLANK-015 (the formatter keeps this line).
- **Input:**

```csharp
class C
{
    string M(int x)
    {
        return $"""
            a

            {x}
            """;
    }
}
```

- **Real output:** RH5025 reports the empty line between `a` and `{x}` — the interpolation's `{` is an opening-brace token that begins its line. The code fix deletes it:

```csharp
        return $"""
            a
            {x}
            """;
```

- **Why it looks wrong:** the deleted line is part of the string's value, so the fix changes program behavior; the formatter leaves the line alone (FMT-BLANK-015), and RH5025's `Why:` (a body detached from its header) does not apply to an interpolation. The same holds for a verbatim interpolated string (`$@"…"`) whose interpolation starts a line.
- **Decision needed:** a bug (RH5025 ignores interpolation braces, or at least lines inside string content), or intended (a clarification adds it — not recommended, since the fix alters a literal).

### Q2 — RH5025 reports the blank line above a standalone block statement

- **Target spec:** `sdd/specs/analyzer/RH5025.md` (same new issue as Q1).
- **Touches:** RH5025-001; FMT-BLANK-008 (the formatter removes the same line).
- **Input:**

```csharp
class C
{
    void M(int a)
    {
        a++;

        {
            a++;
        }
    }
}
```

- **Real output:** RH5025 reports the blank line above `{`; the fix and the formatter both remove it, leaving `a++;` directly above the block.
- **Why it looks wrong:** a standalone block has no declaration or header it belongs to; the rule page describes RH5025 as keeping "a declaration and its opening brace" together, and the blank line here separates two statements, as the statement-kind rules ask elsewhere.
- **Readings:** A — reported (a clarification adds standalone blocks to RH5025-001; FMT-BLANK-008 stays). B — not reported (analyzer and formatter keep the line; a change doc first, since FMT-BLANK-008 changes).

### Q3 — RH5027 and RH5004 contradict each other on a one-line `do` statement

- **Target spec:** `sdd/specs/analyzer/RH5027.md` (new issue `SDD open questions: sdd/specs/analyzer/RH5027.md`).
- **Touches:** RH5027-001, RH5027-004, RH5004-001.
- **Input:**

```csharp
class C
{
    void M(int a)
    {
        a++;

        do a++; while (a < 3);
    }
}
```

- **Real output:** RH5027 reports the blank line above the `do` statement — it checks the line above the line that holds `while`, which here is the `do` line. Its fix deletes it, and RH5004 then reports `do` because the blank line it requires is gone. Restoring it brings RH5027 back; no text satisfies both rules. The formatter keeps the blank line (FMT-BLANK-001).
- **Why it looks wrong:** the blank line separates the `do` statement from the code before it, not the footer from the body.
- **Decision needed:** a bug (RH5027 skips a footer that shares the `do` keyword's line; RH5027-001 already states the reported case only for a `while` on a later line), or intended.

### Q4 — RH5011's fix puts a blank line in front of `else`, which RH5026 forbids

- **Target spec:** `sdd/specs/analyzer/RH5011.md` (new issue `SDD open questions: sdd/specs/analyzer/RH5011.md`).
- **Touches:** RH5011-001, RH5011-004, RH5011-005, RH5026-001.
- **Input:**

```csharp
class C
{
    void M(int a)
    {
        while (true)
        {
            if (a > 0)
                break;
            else
                a++;
        }
    }
}
```

- **Real output:** RH5011 reports `break` (its next token, `else`, is neither `}` nor a directive). The fix inserts an empty line between `break;` and `else`, and RH5026 then reports that line; deleting it brings RH5011 back. On one line, `if (a > 0) break; else a++;` is reported too, and the fix produces `if (a > 0) break;`, an empty line, and `else a++;`, which RH5026 reports as well.
- **Why it looks wrong:** `else` continues the `if` statement; it is not code after the `break` in the sense of RH5011-001's `Why:` (a new path or a new case). The two rules leave no clean state.
- **Decision needed:** a bug (RH5011 does not report a `break` whose next token continues the enclosing statement, such as `else`; RH5011-001 states the reported case only for a following statement or switch section), or intended.

### Q5 — RH5023 reports an empty span at the end of a file with one trailing blank line

- **Target spec:** `sdd/specs/analyzer/RH5023.md` (new issue `SDD open questions: sdd/specs/analyzer/RH5023.md`).
- **Touches:** RH5023-001, RH5023-003, RH5023-004, SHARED-FIX-001.
- **Input:** the file text `"class C\n{\n}\n\n"` (one blank line after `}`, then the final line break; under CRLF the same).
- **Real output:** RH5023 reports one diagnostic with a zero-length span at the end of the file (position 13) — it treats the empty position after the last line break as a blank line that follows the blank line above it. The code fix deletes the empty span, so nothing changes; `scripts/apply-fix.sh RH5023` reports `no-progress` under LF and CRLF. With `"class C\n{\n}\n\n\n"` it reports the real second blank line and the same empty span.
- **Why it looks wrong:** the file has a single blank line, which RH5023-001's `Why:` allows, and a diagnostic whose fix can never clear it contradicts SHARED-FIX-001.
- **Decision needed:** a bug (the position after the last line break is not a line; RH5023-001 already states the reported case only for a blank line that ends with a line break), or intended.

### Q6 — RH5022 does not report a blank line below an opening brace that carries a trailing comment

- **Target spec:** `sdd/specs/analyzer/RH5022.md` (new issue `SDD open questions: sdd/specs/analyzer/RH5022.md`).
- **Touches:** RH5022-001; FMT-BLANK-007 (the formatter removes the line).
- **Input:**

```csharp
class C
{
    void M(int a)
    { // note

        a++;
    }
}
```

- **Real output:** RH5022 reports nothing — the brace's line does not end with the brace. The formatter removes the blank line (FMT-BLANK-007, whose `Why:` says the formatter mirrors RH5022).
- **Why it looks wrong:** the blank line still sits directly below the opening brace and adds height without separating anything, and analyzer and formatter disagree. (RH5024 and RH5025 do report the mirrored cases `} // note` and `{ // note`, because their brace begins the line.) Counterpoint: SHARED-BLANK-005 treats a brace line with a trailing comment as a content line for regions.
- **Readings:** A — not reported (a clarification adds the trailing-comment case to RH5022-001 as a stated exclusion; FMT-BLANK-007's mirror sentence is then imprecise). B — reported (a bug fix in RH5022).

## Follow-up candidates (out of scope, not filed)

- `BlankLineCodeFixUtilities` (used by RH5030's fix and RH3208's) implements the insertion RH5030-003 specifies; it is left without a spec owner here, because RH3208 has no spec yet and the helper is a trivia utility. When RH3208 is specified, decide whether the helper belongs to RH5030 or to a shared spec.
