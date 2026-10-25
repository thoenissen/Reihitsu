Role: spec author
Commit: 9079a22 (revision 8; revision 7 was derived from 27c08b3, revision 6 from 7a040e2, revision 5 from 707fc15, revision 4 from 639e551, revision 3 from 1ef05b5, revision 2 from d099e34, revision 1 from 5a58686)
Verdict: #948: spec written; #949: spec written; #954: spec written; #1390: spec written; #888: spec written

Clarifications only, no change doc (bug-fix lane). New IDs: FMT-SWITCH-014, FMT-STRUCT-025, SHARED-BODY-004, all inserted into `sdd/specs/requirement-ids.txt` at their sorted positions. The lead prompt carried no intended fix or verdict; the issue texts' own cause hypotheses (#949, #954) were not taken over — see the facts below.

How the statements were established: `scripts/trace.sh` over each issue input and its variants at `5a58686`, `scripts/format.sh` for converged outputs, `scripts/apply-fix.sh` for RH5031 and RH3202, and a scratch `net10.0` library compiled with `dotnet build` for the C# scope claims of FMT-SWITCH-014. `SpecStructureTests` and `SpecConsistencyTests` pass on the spec diff; `RequirementCoverageTests.EveryRequirementOfAnEnforcedSpecIsCovered` fails only for FMT-SWITCH-014, FMT-STRUCT-025, and SHARED-BODY-004 (no covering test yet — the test implementer's part). The analyzer project's `RuleDocumentation` tests pass on the changed rule pages.

Revision 2 carries the maintainer's answers to Q1–Q3 (summarized in `00-lead.md`, "Escalation 1"); the questions are closed and recorded under "Maintainer answers" in the `Whole run` section. Revision 3 carries the skeptic's spec notes (`02-skeptic.md`) and the maintainer's answers to the skeptic's questions (`00-lead.md`, "Escalation (questions from 02-skeptic.md)"), each marked "(revision 3)" below. Revision 4 is repair round 1 (`07-auditor.md` findings A3 and A5; maintainer answer A5 → A in `00-lead.md`), marked "(revision 4)". Revision 5 carries the two spec items of `03-test-implementer.md` "Revision 3 — repair round 1" (maintainer answer A on chain links behind a comment tail, and the routed query-clause clarification, both in `00-lead.md`), marked "(revision 5)". Revision 6 follows the merge of `origin/main` with thoenissen/Reihitsu#1462 (rule pages carry no withheld fixes or exceptions; the gap is recorded in the rule's spec) and records the maintainer's answer to Q4, marked "(revision 6)". Revision 7 is repair round 2 (`07-auditor.md` finding A9), marked "(revision 7)". Revision 8 is repair round 3 (A9, narrowed), marked "(revision 8)". The spec architecture tests (`Spec*`, `RequirementCoverage*`) pass at revision 8.

## thoenissen/Reihitsu#948

### Changed requirements

- FMT-SWITCH-014 — A local used by another section leaves the switch alone — added (`sdd/specs/formatter/switch-case-braces.md`). Clarification of FMT-PIPE-003 as decided in the maintainer's #919 Q1 answer (whole switch unchanged, like FMT-SWITCH-011), extended by the issue comment to local functions.

### Facts

- Reproduced: the issue input is braced and section 2 no longer compiles.
- The issue's "expected output = the input unchanged" is not reachable: FMT-BLANK-002 inserts a blank line after `int y = 1;` (trace: `BlankLinePhase`). The spec example therefore carries that blank line in its input, so "is left unchanged" holds for the whole pipeline.
- Compiled (scratch project): `F(out var o);` (expression statement), `var b = F(out var o);` (local declaration) and `if (p is int n)` directly in a section are visible in another section; a local function declared in section 2 can be called from section 1; once braced, the use fails with CS0103. A pattern variable in a `while` condition is not visible in another section (CS0103 unbraced) — hence the requirement names only local declaration, expression statement and `if` condition, with a coverage sentence for other statement kinds.
- The nested-block counter-example (field `y` used in section 2, local `y` inside an `if` block in section 1) compiles both unbraced and braced.

- (revision 3) FMT-SWITCH-014's list of directly declared forms is widened to every form the skeptic's compiled evidence shows C# scopes to the whole switch block (routed by the lead as a clarification): `const` and deconstruction declarations; `out var` / pattern variables in `return`, `throw`, `yield return`, a nested switch's governing expression, and a `lock` expression, besides expression statements, local declarations, and `if` conditions; any of these behind a label. Uses in another section's labels (`case K:`, `when L() > 0`) count. The self-scoped forms are named as the other side: `else if` condition, embedded statements of `if`/`else`, `while`/`do`/`for`/`foreach`/`using`/`fixed` headers, sections of a nested switch. The coverage sentence now leaves only a member name behind `.` unspecified. New example: a `const` used as another section's `case K:` (traced: today only `SwitchCaseBracePhase` changes it). The nested-block counter-example gains a `foreach (var y in ys)` header (compiled unbraced; traced braced output).

- (revision 4, auditor A3) FMT-SWITCH-014's third bullet names the initializer of a local declaration (`var b = F(out var o);`). Evidence: my scratch build at revision 1 (`var b = F(out var o);` in section 1, `o = 2; G(o);` in section 2 compiles unbraced) and the auditor's scratch build; revision 1 had named the form, revision 3 dropped it from the bullet by mistake.

- (revision 5) FMT-SWITCH-014 names query clauses next to lambda bodies: a variable a pattern declares inside a query clause (`from a in xs where a is int n select n`) is scoped to that clause and does not count, also inside a local declaration's initializer. The `Why:` says so. Source: the test implementer's compiled observation, routed by the lead as a clarification.

### Examples for the tests

FMT-SWITCH-014 (four unchanged inputs: local, pattern variable, local function used above its declaration, `const` used as another section's label; two braced: local used only in its own section, locals in a nested block and a `foreach` header). Reproduction tags FMT-SWITCH-014 and FMT-PIPE-003. The skeptic's #948 breaking inputs list further compiled forms (`return … out var`, deconstruction, labeled declaration, `when L()`).

### Non-goals

- Bracing only the sections that are not involved (the decision is the whole switch).
- Uses by a name that is not a simple name (`o.y`), and expression variables of other statement kinds — left unspecified by the coverage sentence.

### Open questions for the maintainer

_None._

## thoenissen/Reihitsu#949

### Changed requirements

- FMT-SWITCH-007 — Comments survive brace insertion — clarified: a comment leading the terminal `break` stays in front of it, behind the new block's closing brace, with the FMT-BLANK-011 blank line in the output of the pass that inserts the braces; new example (the issue input and expected output). `Uses` gains FMT-BLANK-011 and FMT-PIPE-004.

### Facts

- Reproduced (trace, issue input): pass 1 output has no blank line between `}` and `// before break`; pass 2 `BlankLinePhase` inserts it (FMT-PIPE-004 violated).
- The issue states the blank-line phase does not insert the line. The trace shows otherwise: pass 1 `BlankLinePhase` does insert the blank line before `// before break`, and the `SwitchCaseBracePhase` diff of the same pass shows it gone.
- Unbraced shape (`case 1: A(); // before break break;`, section not multi-line): pass 1 inserts the blank line, pass 2 stable — correct today.
- Already-braced input (`{ A(); B(); }` then the comment): pass 1 inserts the blank line, stable — correct today.
- RH5020 agrees with the expected output: RH5020-001/-002 report a line-leading comment whose preceding token is `}` (not `{` or a label colon), and do not report the comment when its line above is blank.

- (revision 3, maintainer answer to skeptic Q2 → A) FMT-SWITCH-007 gains a sentence: when several comments lead the terminal `break`, the blank lines between them are kept, as FMT-PIPE-013 keeps them for wrapped code; second new example (`// one`, blank, `// two`, `break;`). FMT-PIPE-013 itself is unchanged: its list covers the comments of the wrapped code, and the terminal `break` stays outside the block, so the sentence belongs to FMT-SWITCH-007. Facts: today the issue-shaped input with two comments loses the blank line between them for good (pass 2 restores only the line above `// one`); the already braced form with the blank line between the comments is stable today.

### Examples for the tests

FMT-SWITCH-007 (two new examples). Reproduction tags FMT-BLANK-011, FMT-PIPE-004, FMT-SWITCH-007; the two-comment test tags FMT-SWITCH-007 (and FMT-PIPE-013 only if it asserts the wrapped code's comments too).

### Open questions for the maintainer

_None._

## thoenissen/Reihitsu#954

### Changed requirements

- FMT-BLANK-007 — No blank lines inside braces or at the start of a file — clarified with an example: an empty region directly after `{` (the issue input and expected output).
- FMT-BLANK-013 — Region directives are framed by blank lines — clarified with an example (maintainer answer Q2 → A): a comment that is the first content after `{` gets no blank line above it, but a blank line between it and the `#region` below (SHARED-BLANK-005).

### Facts

- Reproduced (trace): pass 1 `LineBreakPhase` adds a blank line after `{`; pass 2 `BlankLinePhase` removes it and `LineBreakPhase` adds it again. The final text is the same in every pass, so `trace.sh` reports "Stable after pass 2" — the text-level FMT-PIPE-004 check does not catch this; FMT-BLANK-007 on the output does.
- Same in a method body with a blank line already between the directives, and with `// c` above the region (all three verified). `#if X` / `#endif` directly inside braces: no change (verified).
- `line-breaks.md` already states in its non-goals: "Where this phase moves a token onto a new line it inserts no blank line."
- The issue's sentence "No phase undoes another phase's edit in any pass" is not a requirement of any spec and was not added; the observable requirement is FMT-BLANK-007.
- Comment variant (Q2): today the formatter leaves `// c` directly above `#region A` there, and `scripts/apply-fix.sh RH5031` reports that output; with a member above the comment the line is inserted today.

### Examples for the tests

FMT-BLANK-007 (new region example), FMT-BLANK-013 (new comment example — the #954 `// c` variant asserts the full spec output). Reproduction tags FMT-BLANK-007 (SHARED-BLANK-005 for the brace exemption, SHARED-BLANK-006 for the kept line between the directives); the `// c` variant also tags FMT-BLANK-013 and SHARED-BLANK-005.

### Open questions for the maintainer

_None._ (Q2 answered: A.)

## thoenissen/Reihitsu#1390

### Changed requirements

- FMT-STRUCT-025 — An `async` function of an unknown task type keeps its expression body — added (`structural-transforms.md`). Instance of FMT-STRUCT-024 / FMT-PIPE-003 as decided in #1371 Q-S1 (P36).
- FMT-STRUCT-004 — Expression-bodied methods and local functions get a block body — clarified: the `return … otherwise` bullet no longer covers an `async` function of an unrecognized return type; one sentence points to FMT-STRUCT-025.
- FMT-STRUCT-006 — The converted body keeps the expression's trivia — clarified: a comment on a line of its own between the token before the arrow and the arrow, or between the arrow and the expression, leads the created statement on a line of its own, in source order; the expression still joins `return`. Coverage sentence leaves comments sharing a line with the arrow or the expression unspecified. New example (before-arrow, after-arrow with `return`, after-arrow with an expression statement).
- FMT-STRUCT-006, further (maintainer answer Q3 → A): a comment that starts the line after the arrow and is followed on that line by the expression stays directly in front of the expression, which joins `return` with it: `int Value() =>` / `/* c */ 42;` becomes `return /* c */ 42;`. Added to the example. The coverage sentence now leaves unspecified only a comment on the arrow's line (#963) and such a comment in front of an expression that becomes an expression statement or a `throw` statement (the answer covered the `return` form).
- SHARED-BODY-004 — No fix for an `async` member of an unknown task type — added (`sdd/specs/shared/expression-body-conversion.md`, maintainer answer Q1 → B): RH3202/RH3208 keep reporting the member FMT-STRUCT-025 keeps, and no code fix is offered for it. SHARED-BODY-003 points to it; the shared spec's `Uses` gains FMT-STRUCT-025. `sdd/specs/analyzer/RH3202.md` and `RH3208.md` name SHARED-BODY-004 in their introduction and `Uses`. RH3203/RH3205/RH3206/RH3207 are unaffected: constructors, finalizers, operators, and conversion operators cannot be `async`.
- (revision 6) The `## Exceptions` sections added in revisions 2–3 to `documentation/rules/RH3202.md` and `RH3208.md` are removed; both pages are identical to `origin/main` again. Under thoenissen/Reihitsu#1462 (`AGENTS.md` "Code-fix coverage", `create-rule-doc`, `squad/roles/spec-author.md` step 4) a withheld fix is covered by `general-notes.md` ("A diagnostic without a code fix") and recorded in the rule's spec. `sdd/specs/analyzer/RH3202.md` and `RH3208.md` now say in their introduction that the fix is deliberately withheld for an `async` member whose return type is not written as a task type, which stays reported and is converted by hand (SHARED-BODY-004); both already listed SHARED-BODY-004 under `Uses`.
- (superseded in revision 6) `documentation/rules/RH3202.md`, `documentation/rules/RH3208.md` — new `## Exceptions` section: the code fix is not offered for an `async` method / local function whose return type is not written as `Task`, `ValueTask`, `Task<T>`, or `ValueTask<T>` (alias, custom task type); convert it by hand (AGENTS.md, Code-fix coverage). Examples unchanged.
- (revision 3, maintainer answer to skeptic Q1 → B) FMT-STRUCT-025 and FMT-STRUCT-004 look through a nullable annotation: `Task?` / `ValueTask?` under `async` convert to an expression statement, `Task<T>?` / `ValueTask<T>?` to `return`. New FMT-STRUCT-025 example (`async Task? RunAsync()`, `async Task<int>? GetAsync()`). SHARED-BODY-004 states that such a member is converted by the fix (SHARED-BODY-003). The RH3202/RH3208 rule pages read "(with or without `?`)".
- (revision 4, auditor A5, maintainer answer A) FMT-STRUCT-006: the move of own-line comments in front of the created statement now applies only when the arrow's line holds no comment. When it holds one, in front of the arrow or behind it (`=> // b`, `=> /* c */ 42`), the conversion moves no own-line comment in front of the created statement, and all comments of the member keep their source order. The placement of the arrow-line comment and of the other comments of such a member stays unspecified (thoenissen/Reihitsu#963); the answer's "output matches `origin/main`" is not pinned as a layout in the spec, because that layout is the #963 placement. The `Why:` explains the order reversal the move would cause. No example: the only output would pin the unspecified arrow-line layout; tests assert the source order and that the own-line comment is not inside the block in front of the statement (auditor inputs: `int A2()` / `// a` / `=> // b` / `42;` and `int Get3()` / `// own` / `=> /* glued */ 3;`).
- (revision 7, auditor A9) FMT-STRUCT-006's condition for not moving comments is narrowed from "the arrow's line holds a comment" to a comment that stands directly at the arrow: behind it on its line before the next token (`=> // b`, `=> /* c */ 42`), or in front of it on its line behind the last line break in front of it (a line `/* c */ => 42;`). A comment further along the arrow's line, behind the expression or the semicolon (`=> 42 /* t */;`, `=> 42; // t`), now explicitly does not stop the move; the coverage sentence leaves the placement of a comment directly at the arrow unspecified, and the `Why:` explains both sides. Derived from maintainer answer A5 → A (move no comment when the arrow carries one, keep source order) and the code's check `ExpressionBodyToBlockConverter.ArrowLineHoldsComment` (the arrow's trailing trivia, and its leading trivia back to the last line break). Verified with `format.sh` on this branch: `int M()` / `// a` / `=> 42 /* t */;` gives `{` / `// a` / `return 42 /* t */;` / `}`; `int N()` / `// b` / `=> 42; // t` gives `{` / `// b` / `return 42;` / `} // t`; `int P()` / `// c` / `=> // d` / `42;` keeps `// c` in front of `{// d`. (Superseded in revision 8: the text did decide the `int M() /* c */ =>` case, the wrong way.)
- (revision 8, auditor A9 narrowed) FMT-STRUCT-006: a comment "in front of the arrow" now counts only on a line on which no token stands in front of the arrow, behind the last line break in front of it — what `ExpressionBodyToBlockConverter.ArrowLineHoldsComment` checks (the arrow's own trailing trivia, and its leading trivia back to the last line break; a comment behind `)` on the arrow's line is trailing trivia of `)`). New explicit other side: a comment behind the token in front of the arrow on the same line (`int M() /* c */ =>`) does not stand at the arrow and MUST NOT stop the move. Verified with `format.sh` on this branch, stable on a second run: `int M() /* c */ =>` / `// a` / `42;` gives `int M() /* c */` / `{` / `// a` / `return 42;` / `}`; `int A4()` / `// a` / `/* c */ => 42;` keeps `// a` above `/* c */ {` (no move); `int G()` / `// own` / `=> /* glued */ 3;` keeps `// own` above `{/* glued */` (no move).
- (revision 3) FMT-STRUCT-006's coverage sentence also leaves a documentation comment (`///`) around the arrow unspecified (skeptic: do not pin it in this run).
- FMT-BREAK-021 — Object initializers are always laid out over several lines — clarified: names the `with` initializer; new example (the issue's expression-bodied property and a local). The `Why` notes no rule reports a `with` initializer (RH5301 lists it as a non-goal).

### Facts

- Async alias (`using T = System.Threading.Tasks.Task;`, `async T RunAsync() => await T.Delay(1);`): today `return await T.Delay(1);` (CS1997). The current check recognizes `Task`/`ValueTask` as a simple name or the right part of a (alias-)qualified name; every other `async` return type gets `return`.
- FMT-STRUCT-025 follows the issue's decision literally: an `async` function returning a generic alias (`using V = ValueTask<int>`) or a custom task-like type, which today converts to compiling code, keeps its expression body. The example shows this on purpose.
- Qualified `System.Threading.Tasks.Task` / `Task<int>` under `async`: converted today as the FMT-STRUCT-025 example shows (verified with `format.sh`).
- Comment before the arrow: today `int Other()`, blank line, `// before arrow`, `{` — the comment stands between the signature and the brace. Comment after the arrow: today `return`, blank line, `// c`, `42;`. Same shape with an expression statement (`void Run() =>` / `// run` / `Foo();`): today `{        // run` on the brace line. All three are in the FMT-STRUCT-006 example; the third is not named in the issue but is the same position with the other statement form, and FMT-PIPE-013 already requires a comment on a line of its own to stay on one.
- `with` initializer: a one-line initializer with two or more elements keeps `{` on the `with` line in pass 1 and gets its own line in pass 2 (FMT-PIPE-004 violated); a one-element initializer and an already wrapped one reach the layout in pass 1. Verified the same for a converted expression-bodied method returning a `with` expression. The FMT-BREAK-021 example output is the converged output of `format.sh` run twice.

### Examples for the tests

FMT-STRUCT-025 (alias / custom type kept; qualified and nullable forms converted), FMT-STRUCT-006 (new example, four members), FMT-BREAK-021 (new `with` example), SHARED-BODY-004 (RH3202 and RH3208 blocks: reported, no code fix offered). Reproductions tag FMT-STRUCT-025 + FMT-PIPE-003 (alias), FMT-STRUCT-006 (comments), FMT-BREAK-021 + FMT-PIPE-004 (`with`), SHARED-BODY-004 (RH3202/RH3208 fix withheld; today `scripts/apply-fix.sh RH3202` applies a fix that yields CS1997).

### Non-goals

- A comment behind `=>` on the arrow's line (`int Get() => // why`), which is thoenissen/Reihitsu#963 (milestone 1.2.1), and field splitting from the same issue.
- Changing what RH3202/RH3208 report: the `async` member stays reported (Q1 → B); only the fix is withheld.
- Other FMT-BREAK-021 text changes asked by thoenissen/Reihitsu#1291 (another issue edits the same requirement; expect a textual merge there).

### Open questions for the maintainer

_None._ (Q1 answered: B; Q3 answered: A.)

## thoenissen/Reihitsu#888

### Changed requirements

- FMT-BREAK-019 — Chains follow the shared chain layout — clarified: a later call behind the closing `*/` of a multi-line comment does not start its line (SHARED-COMMENT-012), so the formatter breaks in front of its operator; new example. `Uses` of `line-breaks.md` gains SHARED-COMMENT-012, as the maintainer asked.

### Facts

- Reproduced (trace, issue input): the only change is the FMT-BLANK-011 blank line above `/* x`; `.D()` stays behind `*/`. The example input already carries that blank line so the example shows only the line break.
- The expected `.D()` column (17, the anchor `.B`) follows SHARED-CHAIN-007; the comment's last line `*/` keeps its column (comment content).
- RH5201 analyzer unchanged per the maintainer. `sdd/specs/analyzer/RH5201.md` does not list SHARED-COMMENT-012 under `Uses`; not changed here (follow-up candidate).
- Related open issue outside the run: thoenissen/Reihitsu#1382 (P33, token behind a multi-line comment tail: formatter breaks before it, RH5203/RH5204).

- (revision 3) FMT-BREAK-019's sentence reads "a later link" instead of "a later call", matching SHARED-CHAIN-005, which covers every link after the first invoked link, a property read included (skeptic's #888 note, routed by the lead).

- (revision 5, maintainer answer A) SHARED-CHAIN-005 (`sdd/specs/shared/fluent-chain-layout.md`) gains the exception: in a wrapped chain, a link whose operator stands behind the closing `*/` of a multi-line delimited comment starts its own line also when it is a prefix link, a link of a chain without an invoked link, or the first invoked link; the formatter breaks before its operator and RH5201 reports it as SHARED-COMMENT-012 specifies. New example (prefix `.B` and first invoked `.C()` behind `*/`); its output is what `format.sh` produces today on this branch, and a second run changes nothing. `Uses` gains SHARED-COMMENT-012. FMT-BREAK-019's sentence now covers any link behind such a comment, pointing to the exception. Ownership: `fluent-chain-layout.md` and its code owners belong to no concurrent run; no code owner changed.
  - Facts on this branch: `scripts/apply-fix.sh RH5201` reports the prefix and first-invoked inputs and fixes each in one iteration (LF, CRLF); on the formatted output it reports nothing. So RH5201 already agrees, and the maintainer's "RH5201 unchanged" holds.
  - Left unspecified, by the exception's own sentence: the chain's first link behind a comment tail (`var t = a /* x` / `*/ .C()` / `.D();`), which SHARED-CHAIN-003/-004 govern — today the formatter leaves `.C()` behind `*/` (as `*/.C()`) and RH5201 reports nothing; and whether a chain whose only line breaks lie inside such a comment counts as wrapped (the skeptic's #888 follow-up). Q4, answered B in revision 6 (see the Whole run section).

### Examples for the tests

FMT-BREAK-019 (new example; the skeptic's `?.D()` / `.E` input is covered by the "later link" wording), SHARED-CHAIN-005 (new example: prefix link and first invoked link behind a comment tail). Reproduction tags SHARED-COMMENT-012 and FMT-BREAK-019; the RH5201 code-fix test tags SHARED-CHAIN-008 / SHARED-FIX-001 (one-pass convergence), under LF and CRLF as the issue asks.

### Open questions for the maintainer

_None._

## Whole run

### Open questions for the maintainer

_None._ (Q4 answered in revision 6, see below.)

### Maintainer answers (Escalation 1, recorded in `00-lead.md`)

- Q1 (#1390, RH3202/RH3208 on an `async` member FMT-STRUCT-025 keeps) → B: the report stays, the code fix is withheld for that input. Run R7 starts after this run, so the RH3xxx expression-body fix base and its shared files may change here (escalation 5 approved by the answer). Carried into SHARED-BODY-004, SHARED-BODY-003, the RH3202/RH3208 specs and rule pages.
- Q2 (#954 variant, no blank line between a comment first after `{` and `#region`) → A: fixed in this run. Carried into the FMT-BLANK-013 example.
- Q3 (#1390, `int B() =>` / `/* c */ 42;`) → A: `return /* c */ 42;`. Carried into FMT-STRUCT-006.

### Maintainer answer to Q4 (revision 6, recorded in `00-lead.md`, "Escalation 1 before the auditor")

- Q4 (#888, the chain's first link behind a multi-line comment tail, `var t = a /* x` / `*/ .C()` / `.D();`) → B: it keeps today's behavior (`.C()` stays behind `*/`) and stays unspecified in this run; follow-up candidate. No spec change: the SHARED-CHAIN-005 exception already excludes the chain's first link, which SHARED-CHAIN-003 and SHARED-CHAIN-004 govern. The maintainer did not object to SHARED-BODY-004 being part of the bug fix without a change doc.

### Maintainer answers to the skeptic's questions (recorded in `00-lead.md`)

- Skeptic Q1 (nullable task type) → B: carried into FMT-STRUCT-004, FMT-STRUCT-025, and SHARED-BODY-004 (the rule-page wording of revision 3 was removed with the pages' `## Exceptions` sections in revision 6).
- Skeptic Q2 (blank line between two comments before the terminal `break`) → A: carried into FMT-SWITCH-007.
- Skeptic Q3 (`TokenGapNormalizer` change, escalation 5) → A: no spec change.
- Routed clarifications: FMT-SWITCH-014 declaration forms, FMT-BREAK-019 "later link".

Not taken: the skeptic's precision note on the title of SHARED-BODY-001 ("A member the formatter cannot convert is not reported"). Its normative sentence names only FMT-STRUCT-007 and stays correct next to SHARED-BODY-004; the title was left unchanged because renaming headings was not routed.

### Follow-up candidates

- (revision 6) The chain's first link behind a multi-line comment tail (Q4 → B): whether it starts its own line, as the SHARED-CHAIN-005 exception asks for the other links.

- `sdd/specs/analyzer/RH5201.md` could list SHARED-COMMENT-012 under `Uses` (RH5201 checks line starts); not requested in this run.
