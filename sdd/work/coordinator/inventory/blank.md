# Inventory — blank-line / comment-placement open questions

Issues #1000, #1001, #1058–#1066. No issue has comments; all content is from the bodies. 17 questions.

| Issue | Q | Rule/spec | Input → real output | Why it looks wrong | Requirement IDs touched | Pattern | Duplicate of | Suggested decision |
|---|---|---|---|---|---|---|---|---|
| #1000 | Q1 | SHARED-STMT base (RH5001–5019, 5021, 5029) | `case 1 when⏎ Check(value): Method(1); var y = 1;` → fix moves `var y` to own line, indented 4 cols deeper than `break;` | Contradicts "indented like the statement it follows" | SHARED-STMT-004 | NEW:fix-output-misformatted | — (layout sibling: #1062 Q1) | Clarification: documented limitation (RH5103.md); bug if cheap |
| #1001 | Q1 | RH5029, RH5021, formatter | `a++; var (b, c) = t; var d = …;` → blank inserted between `var (b,c)` and `var d`, not after `a++` | Splits a run of declarations, against RH5029-001 Why | RH5029-001, RH5021-001/002, FMT-BLANK-002 | NEW:scope-definition | — | Clarification (keep reading A; analyzer+formatter agree) |
| #1058 | Q1 | Region comment rules RH5035/38/39/43/44/46/47 | `a.B()⏎// c⏎?.Name` not reported; `a.B()⏎// c⏎!?.C()` reported | Comment rules' chain model differs from SHARED-CHAIN-001 | SHARED-COMMENT-004/009, SHARED-CHAIN-001 | NEW:scope-definition | same decision as #1058 Q1-add | Behavior change: follow SHARED-CHAIN-001 (reading a) |
| #1058 | Q1-add | same region rules | `c.B().Q⏎// c⏎++.ToString()` and `p->A()⏎// c⏎->B()` not reported | `++`/`->` are not chain links under SHARED-CHAIN-001 | SHARED-COMMENT-004/009, SHARED-CHAIN-001 | NEW:scope-definition | #1058 Q1 | Behavior change together with Q1 |
| #1058 | Q2 | same region rules (+FMT-BREAK-015) | `Use(o ??⏎// c⏎new { First = 1 })` not reported; `new D { A = 1 }` reported | Leading part (`new`, switched expr, pattern type) is outside construct's own layout | SHARED-COMMENT-004/006, FMT-BREAK-015 | NEW:scope-definition | — | Behavior change (b) with formatter in same change doc |
| #1058 | Q3 | region rules, RH5049 | `M(o⏎// c⏎is Color.Red)` not reported (treated as type test) | `Color.Red` is a constant pattern like `null` | SHARED-COMMENT-007/008, RH5049-001 | NEW:scope-definition | — | Clarification: syntax decides (reading a) |
| #1059 | Q1 | RH5025 | `$"""⏎a⏎⏎{x}⏎"""` → fix deletes empty line before `{x}` (interpolation brace) | Changes string value; formatter keeps line | RH5025-001/004, SHARED-WS-001, FMT-BLANK-015 | A (+E) | class-sibling: #1060 Q1 shape 3 | Bug: ignore interpolation braces |
| #1059 | Q2 | RH5025 (+RH5032) | `a++;⏎⏎{ a++; }`, dict `{ "a",1 },⏎⏎{ "b",2 }`, switch arm → blank deleted; below `#region` RH5032 re-inserts | Sibling braces aren't "declaration + brace"; region case cycles | RH5025-001/004, FMT-BLANK-008, RH5032-001, SHARED-BLANK-006 | NEW:scope-definition + F(cycle) | region cycle overlaps #1065 Q1 | Clarification (A, matches formatter) + separate cycle fix |
| #1060 | Q1 | RH5027 | `a++;⏎⏎do a++; while(..);` / `@"x⏎⏎".Length; while` / `/* x⏎⏎ */ while` → all four lines deleted | Wrong gap checked; edits literal/comment; cycle with RH5004 | RH5027-001/004, RH5004-001, SHARED-WS-001, FMT-BLANK-001/008 | A (+F cycle, +E) | literal shape ≈ #1059 Q1 | Bug: check only gap directly before `while` |
| #1061 | Q1 | RH5011 | `if (a>0) break; else a++;` / `do break; while(..)` → blank inserted before `else`/`while` | `else`/`while` continue statement; RH5026/RH5027 then report | RH5011-001/004/005, RH5026-001, RH5027-001 | F (cycle) (+E for do) | — | Bug: don't report when next token continues enclosing stmt |
| #1061 | Q2 | RH5011 | `break; /* a⏎b */⏎a++;` → blank inserted inside comment, every application again | Edits comment text; never converges | RH5011-004, SHARED-FIX-001, SHARED-BLANK-001 | D (+A) | root-cause sibling: #1062 Q1 | Bug: insert after comment end, or withhold |
| #1062 | Q1 | RH5030 | `} /* a⏎b */ a++;` → 1st apply only breaks line (`a++;` col 0), 2nd adds blank | Two applications; trailing space, unindented statement | RH5030-003, SHARED-FIX-001 | D (+fix-output-misformatted) | sibling #1061 Q2; not #972 | Bug: one application or withhold |
| #1063 | Q1 | RH5023 | file `"class C\n{\n}\n\n"` → zero-length diagnostic at EOF; fix changes nothing | Single blank line allowed; fix is a no-op | RH5023-001/003/004, SHARED-FIX-001 | D + F(no-op) | — | Bug: position after last line break isn't a line |
| #1064 | Q1 | RH5022 | `{ // note⏎⏎a++;` → not reported; formatter removes the blank | Analyzer/formatter disagree; mirrored RH5024/5025 report | RH5022-001, FMT-BLANK-007, SHARED-BLANK-005 | E | — | Bug: report it (reading B) |
| #1065 | Q1 | RH5024 vs RH5032 | `#region R⏎⏎}` → RH5024 deletes blank, RH5032 re-inserts; formatter keeps | No text satisfies both rules | RH5024-001/005, RH5032-001, SHARED-BLANK-006, FMT-BLANK-013 | F (cycle) + E | overlaps #1059 Q2 region cycle | Bug in RH5024 (reading A, matches formatter) |
| #1065 | Q2 | RH5011, SHARED-STMT rules | `break;⏎/// doc⏎⏎a++;` → reported; fix adds 2nd blank above `/// doc` | SHARED-BLANK-001 counts that line as blank | SHARED-BLANK-001, SHARED-STMT-001/004, RH5011-001/004 | NEW:line-model-mismatch | affects #1000's spec | Bug: helper counts the line (low severity) |
| #1066 | Q1 | SHARED-FIX-001 across RH5022/24/25/26/27 + RH5023 | `{⏎⏎⏎M();⏎⏎⏎}` → each rule needs 2 applications; RH5024–27 report same line as RH5023 | One application doesn't correct; cross-rule ownership unspecified | SHARED-FIX-001, RH5022–RH5027 (-001/-003/-004), RH5023-001, FMT-BLANK-007/008 | D (principle) | — | Principle: decide whether SHARED-FIX-001 is per-rule |

## Cross-issue duplicates / overlaps

- #1058 Q1 and Q1-add: one decision (comment rules follow SHARED-CHAIN-001).
- #1059 Q2 (region part) and #1065 Q1: both are blank-line-below-`#region` cycles with RH5032/SHARED-BLANK-006 (RH5025 vs RH5024). Decide together: either the brace rules skip a line SHARED-BLANK-006 requires, or SHARED-BLANK-006 exempts `#region` before a brace.
- #1059 Q1 and #1060 Q1 shapes 3/4: blank-line rule deletes an "empty line" that lies inside a literal/comment (RH5025 vs RH5027; different code, same class).
- #1061 Q2 and #1062 Q1: a fix inserting a blank where a multi-line block comment spans the gap; both SHARED-FIX-001.
- #1060 Q1 shape 1 and #1061 Q1: cross-rule cycles involving do-while (RH5027↔RH5004, RH5011↔RH5026/RH5027).
- #1065 Q2 touches the SHARED-STMT spec of #1000 (different defect).

## Recurring patterns

- **F (cycle)** — a fix leaves a diagnostic of another rule whose fix restores the original (sub-class of F): #1059 Q2 (RH5025↔RH5032), #1060 Q1 shape 1 (RH5027↔RH5004), #1061 Q1 (RH5011↔RH5026/RH5027), #1065 Q1 (RH5024↔RH5032).
- **NEW:scope-definition** — what syntactic shape counts as X (declaration, chain link, construct's own part, type test, sibling brace); analyzer behavior is consistent, spec must choose: #1001 Q1, #1058 Q1, Q1-add, Q2, Q3, #1059 Q2.
- **NEW:line-model-mismatch** — the analyzer's "line"/"blank line" ignores SHARED-BLANK-001 (counts lines inside literals/comments, the EOF position, or misses lines after doc-comment trivia). Root cause for #1059 Q1, #1060 Q1 (shapes 3/4), #1061 Q2, #1063 Q1, #1065 Q2.
- **NEW:fix-output-misformatted** — fix output compiles and clears, but with wrong indentation/trailing whitespace: #1000 Q1, #1062 Q1 (secondary).
- **E as secondary** is common: #1059 Q1, #1060 Q1, #1061 Q1, #1064 Q1 (primary), #1065 Q1 — formatter generally holds the "right" answer.

## #1066 (shared/code-fixes.md) — principle question

SHARED-FIX-001 says one application of a rule's fix must make *that rule* stop reporting the corrected code. For a run of ≥2 blank lines at a brace/keyword, RH5022 and RH5024–RH5027 each report only one line, so each needs two applications; RH5023 (excess blank lines) owns the rest of the run.
The question is whether SHARED-FIX-001 is read per rule (reading A: each rule must clear the whole run itself → bug) or across rules (reading B: runs belong to RH5023, combined pass suffices). Reading B works today for RH5022+RH5023 but not RH5024–27, which report the same line as RH5023, so B also needs a behavior change for those.
This also interacts with the code-fixes.md non-goal on "fixes leaving other rules' diagnostics", which is currently unspecified — the decision effectively defines whether cross-rule convergence is part of SHARED-FIX-001. The formatter already clears the run in one pass, so reading A also aligns analyzer with formatter.
