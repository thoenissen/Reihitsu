Role: skeptic
Commit: ebfe9b0
Verdict: #1600: output question; #1150: proceed; #1274: output question; #1143: proceed; #1673: proceed; #1678: proceed

My prompt carried no intended fix, suspected cause, or hoped-for verdict. It named the requirements to challenge, and I checked those on the evidence below.

Scope: the spec diff `origin/main..HEAD`, which is e36792d and c679eb1: the change doc `sdd/changes/2026-11/fix-offering-safety.md`, `pipeline.md`, `unsupported-syntax.md`, `line-breaks.md`, `code-fixes.md`, `line-join-rules.md`, `ordering-move-safety.md`, `RH5308.md`, `RH7101.md`, the ledger, and `AGENTS.md`. I read it against `00-lead.md` (maintainer answers Q1–Q7, decisions 1–10). I did not redo 02's premise checks.

How I ran things:
- `scripts/build.sh` (exit 0).
- `scripts/format.sh` and `scripts/apply-fix.sh` (RH5308, RH5302, RH5020) on fixtures in the scratchpad, which I deleted afterwards.
- Every "observed" below comes from one of these runs on this tree.

## thoenissen/Reihitsu#1600

### Premise

Unchanged from 02. The spec diff carries out Q1 B, Q2 A, and Q3 B, and the lead's decision 7 on the line kinds. One reading of Q1 B is open; see *Output questions*.

### Mechanism and candidates

_N/A_. This stage checks the spec, not a bug.

### Breaking inputs

**FMT-PIPE-033, the hiding-carrier definition.**

1. **Over-refusal: balanced directives inside a carrier in a conditional group.** A carrier that holds an `#if … #endif` pair of its own counts, because it holds an `#endif` line. The build that excludes the group still skips it whole, because the `#endif` closes the carrier's own `#if`. Both inputs below are formatted today (`a+b` becomes `a + b`). Under FMT-PIPE-033 both are left unchanged, and SHARED-FIX-012 withholds every fix in them, although neither hides code in any build:

   ```csharp
   class C
   {
       int M(int a, int b)
       {
   #if DEBUG
           /*
   #if TRACE
           Log();
   #endif
           */
   #endif
           return a+b;
       }
   }
   ```

   ```csharp
   class C
   {
   #if NET
       const string T = """
           #if NULLABLE
           #nullable enable
           #endif
           """;
   #endif
       int M(int a, int b)
       {
           return a+b;
       }
   }
   ```

   - The class is rare, and the refusal is silent and whole-file. It is the lead's to decide as a rare-class refusal (`AGENTS.md`, *Code-fix coverage*); see *Internal choices*.
   - The Why currently overclaims. It says the narrowing "keeps commented-out code with its directives, and raw strings that hold … generator templates, formattable". That holds only for carriers outside every conditional group, and the second input above is a generator template that is refused.

2. **Carrier kinds the implementer must not miss.** The UTF-8 forms count as carriers too:
   - a verbatim UTF-8 string `@"…"u8` (`Utf8StringLiteralToken`);
   - a multi-line raw UTF-8 string `"""…"""u8` (`Utf8MultiLineRawStringLiteralToken`).

   An interpolated verbatim or interpolated raw string spreads its text over several `InterpolatedStringTextToken`s, so the `#else` line can be in any of them. Test input: an `@"…"u8` carrier and an `$@"…{x}…"` carrier whose `#else` line comes after a hole.

3. **Not a carrier.** None of these is a carrier, as a test control:
   - a `///` comment, whose lines start with `///`;
   - a ` * #else` line in a `/* */` comment;
   - a `#else` on the carrier's first line.

**SHARED-FIX-012**

4. **Fix All that bypasses registration.** `CasingCodeFixProviderBase`'s `CasingFixAllProvider` calls `ApplyFixAllAsync`. That method decides each candidate with `CanRegisterCodeFix` directly (`CasingCodeFixProviderBase{T}.cs` around line 1096), not through `RegisterCodeFixesAsync`.
   - A common base that checks in `RegisterCodeFixesAsync` does not reach a project- or solution-scope casing Fix All. That Fix All would rename in a carrier document, or rename into one.
   - SHARED-FIX-002 covers only fixes that are offered, so the spec already decides this case. The test must exist anyway.
   - Test input: a solution-scope casing Fix All started from a document without a carrier, with a second document that holds one and holds a reported name.
   - `DocumentOrderFixAllProvider` re-registers through `RegisterCodeFixesAsync`, and so does the BatchFixer, so both are covered by the base check.
5. **Fixes that change other documents.** This is wider than the casing base the spec author named:
   - `RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider` renames through `Renamer.RenameSymbolAsync` on the solution. It reaches every `partial` declaration of the type, also one in another document.
   - Test input: an RH2001 property whose type has a `partial` part in a second document that holds a carrier.
   - RH4001 moves the document's unchanged text to a new document. The base check already withholds it, because its own document holds the carrier.
6. **Cost per call.** The base check reads the whole document every time a fix registers, and the BatchFixer registers once per diagnostic. A per-tree cache, as `SyntaxTreeErrorVerdictCache` does for SHARED-JOIN-002, keeps that linear. This is an implementation note, not a spec point.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `ReihitsuFormatter.FormatDocumentAsync`/`FormatSyntaxTree` | must change (FMT-PIPE-033) |
| Node-level entry points | must stay (they do not check FMT-PIPE-033) |
| Common code-fix base (new type, Q3 B) | must change. Note the casing Fix All path (item 4) |
| `CasingCodeFixProviderBase`, RH2001 | must change: they also check the other documents they change |
| CLI, Playground | must stay as specified (no new requirement). See output question 1 |
| `Reihitsu.Core` | must change if the one definition lives there (FMT-PIPE-009 rules out the fixes calling formatter internals) |
| Rule pages | not involved |

### Refuse option

The run's change is itself a refusal: a whole-file leave and a whole-document withholding, as answered.

Declaring the shape unsupported was the maintainer's option A, and he did not choose it. Nothing simpler remains.

### Output questions

**1 · CLI and Playground — a file whose comment or string is code in another build**

The answer's text, "unchanged, silently, like a file with a syntax error. Nothing is reported", has two readings, because a file with a syntax error is not silent in the CLI or the Playground. Observed with `scripts/format.sh --verbose` on a syntax-error file and on FMT-PIPE-033's first example:

```text
Formatted: …/hide.cs
Skipped (syntax errors): …/err.cs

Formatted 1 of 2 file(s).
Skipped 1 file(s) with syntax errors.
```

The Playground shows "The input contains syntax errors and was left unchanged." for a syntax error (PLAY-006).

Input: FMT-PIPE-033's first example (`#if !A` / `/*` / `#else` / `var s = @"` / `*/` / … / `#endif`).

Before (`main`): the file is formatted. The block's lines move from column 12 to column 8.

After, A (recommended, the spec as written):
- The file is unchanged and counted as unchanged.
- `--verbose` prints `Unchanged: <path>`, and the summary has no skip line.
- The Playground shows the input with no message.

After, B:
- The file is unchanged and counted as skipped, like a file with a syntax error.
- `--verbose` prints a `Skipped (…): <path>` line, and the summary gains a `Skipped <n> file(s) …` line.
- The Playground shows a message.

Why A: the option the maintainer picked says "Nothing is reported". B needs a third result outcome, or a second statement of the carrier check in the CLI and the Playground (FC-015), plus new CLI and Playground text, all for a rare shape.

If the lead reads "Nothing is reported" as having decided this, the Why of FMT-PIPE-033 still has to change. It says "left alone silently, as a file with a syntax error is". The formatter does return both unchanged with `Unsupported` null, but the CLI reports the syntax-error file and not this one. The sentence should say that at formatter level they match, and that the CLI and the Playground count this file as unchanged.

### Internal choices

- **Balanced-directive over-refusal (item 1).** Recommend keeping the definition as written: one line scan, rare, and it only ever refuses. Correct FMT-PIPE-033's Why so that it names the over-refusal as decided: "a carrier whose `#elif`, `#else`, or `#endif` lines close only groups that the carrier itself opens hides nothing either, but counts; telling them apart means pairing directives inside the text". Narrowing would mean counting `#if` against `#endif` inside the text, which edges toward the "parser-like rules over text" that *Special cases do not grow the source* rules out.
- **The change doc's *What* says "from one check made before any fix registers".** The rename part (other documents) is decided inside the casing base and RH2001, after the renamed solution is computed, not before any fix registers. Reword to "every fix learns this when it registers; the check for its own document is made once, before any fix registers".
- **The FMT-PIPE-003 pointer** reads "is decided by FMT-PIPE-033". FMT-PIPE-033 also records a gap in which FMT-PIPE-003's MUST does not hold for another build (member (b)). For parity with the SHARED-FIX-003 bullet, add "and the case it leaves open".

### Issue goals

Nothing beyond 02. The spec diff matches the lead's narrowing of the line kinds.

### Architecture findings

None new. One definition is shared by FMT-PIPE-033 and SHARED-FIX-012, which the spec requires (FC-015).

### Follow-up candidates

None.

## thoenissen/Reihitsu#1150

### Premise

Unchanged from 02. The corrections in c679eb1 carry out 02's challenges 1–3:
- the first sentence is scoped to FMT-PIPE-005;
- the RH5308 and RH3004 examples replace RH6023/RH7105, with RH6023 as the control;
- FMT-PIPE-005's Why gains "apart from a shape refused as unsupported".

I found no conflict in the reworded text. RH5308-005 and RH5308-006 do not touch SHARED-FIX-011's example, which has no comment and has a statement ancestor.

### Mechanism and candidates

_N/A_

### Breaking inputs

None new. The test list in 02 stands.

### Surfaces

As in 02.

### Refuse option

As in 02.

### Output questions

_None._

### Internal choices

_None._

### Issue goals

As in 02.

### Architecture findings

_None._

### Follow-up candidates

- **RH5302-005 now conflicts with this run's SHARED-FIX-011.**
  - RH5302-005 withholds the fix by copying the formatter's join guard (`SyntaxTriviaUtilities.WouldJoinAcrossUnjoinableTrivia` at `RH5302…CodeFixProvider.cs:102`). It does not ask the formatter. SHARED-FIX-011's second sentence now requires a rule spec to "ask the formatter instead of copying its condition".
  - RH5302 also reports a gap that the formatter does not join because a comment sits in it, which SHARED-DIAG-010 forbids.
  - It is the binary-operator twin of the RH5308 option B that lead decision 6 dropped for contradicting the same two requirements.
  - Observed: `var x = a && // note⏎ b;` gives RH5302 `no-fix-offered`, and the formatter moves `b;` to column 8.
  - See *Whole run*, *Follow-up candidates*, for the output question.
- The RH5401-004 follow-up from 02 stands.

## thoenissen/Reihitsu#1274

### Premise

Unchanged from 02. The spec diff carries out Q4 C as one FMT-UNSUP requirement plus RH5308-005, and it deletes the two `WouldJoinAcrossUnjoinableTrivia` refusals.

The shape covers more than the answer needs; see *Output questions* 2.

### Mechanism and candidates

_N/A_

### Breaking inputs

**FMT-UNSUP-008 against the precheck contract** (`unsupported-syntax.md`, `AGENTS.md`):

- **Cheap and local.** The check reads the trailing trivia of `?`/`:` and the leading trivia of the branch's first token, all inside the `ConditionalExpressionSyntax`. Satisfied.
- **Partial shapes (FMT-UNSUP-003).** Every node that holds a conditional holds both operators and the trivia behind them. A node-level input whose first token is a branch's first token holds no conditional, and the deleted guards run only on a conditional. The change doc's claim holds.
- **Closure (FMT-UNSUP-005).** I formatted inputs with comments in every allowed place:
  - `a /* c1 */ ?⏎ 1 : 2`
  - `1 /* c2 */ :⏎ 2`
  - `a ? b /* c3 */ ? 1 : 2 : 3`
  - `// c4` in front of a `:` that ends its line
  - FMT-UNSUP-008's other-side example
  - `a // c9⏎ ? 1 : 2`

  Two passes. No output holds a comment behind `?` or `:`, and the second pass changes nothing. Satisfied for these.
- **Special cases, checked and kept: the list is incomplete.**
  - `DocumentationCommentFormattingPhase` puts a relocated `/** */` comment's line break into the owning token's leading trivia. Its comment (`DocumentationCommentFormattingPhase.cs` lines 232–235) names "the line placement of a conditional operator" as the reader it protects.
  - It serves every operator (`DelimitedDocumentationInExpressionTests`: "behind other operators"), so it stays.
  - The change doc must list it under *Checked and kept* (`AGENTS.md`: the change doc "lists … what it checked and keeps").

**Existing tests whose input holds the declared shape**, which the 03 list does not name. The test implementer rewrites them to the refusal, or to a withheld fix:
- `DelimitedDocumentationInExpressionTests`: every `?`/`:` case, tagged FMT-BREAK-018. Example input: `b ? 1 : /** d */ 2`.
- `RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzerTests`: `VerifyFixMovesDelimitedDocumentationBehind{Question,Colon}…` at lines 767–903. RH3202's fix formats the member, so SHARED-FIX-010 withholds it.
- `CommentJoinTests.TernaryQuestionIsNotCollapsedIntoTrailingComment` and `…TernaryColon…`.
- `JoinSeparatorPhaseTests.CommentInTheGapStillRefusesEveryJoin`, the `return b ? // r` part.
- `SameLineGapRulePhaseTests.DocumentationCommentsAroundTheQuestionMarkKeepTheirPlace` (`b ?/** y */1 : 2`), if it runs through an entry point.

**Members the test implementer covers** besides 03's list:
- the nullable `?` (`int? /* a */ b`) and `?.`/`?[]` stay unrefused: the check keys on `ConditionalExpressionSyntax`, not on a `?` token;
- a conditional in an interpolation hole, `$"{(a ? /* c */ 1 : 2)}"`, refused;
- `ref` branches: `a ? ref /* c */ x : ref y` is not refused, because the comment is behind the branch's first token.

**RH5308-005 and RH5308-006 examples.**
- RH5308 reports one diagnostic per chain, on the first misaligned operator (`FindFirstMisalignment`). The markers in RH5308-005 are right: `?` for `x`; `:` for `y`, whose `?` stands in column 20. The other-side example's `?` in column 16 is reported, and the fix is offered. Observed: `fixed (1 action, 1 iteration)`.
- RH5308-006's single `?` marker is right.
- The RH5308-005 other-side fix output carries a blank line; see *Follow-up candidates* below.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `PrecheckPhase` | must change (FMT-UNSUP-008) |
| `TernaryLineBreakRewriter` | must change (two guards deleted) |
| `DocumentationCommentFormattingPhase` | must stay; add to *Checked and kept* |
| RH5308 analyzer | must stay: it keeps reporting (see output question 3) |
| RH5308 fix | must stay: it already asks `FindUnsupportedSyntax` |
| Every other fix whose formatted node holds the shape (SHARED-FIX-010) | behavior changes with no code change: RH3202–RH3208, RH3004, RH7101 (the context is the whole type), RH5101–RH5113, RH5205/RH5206, RH5301–RH5307, RH5401–RH5408, RH5112/RH5201, RH2001, RH5417 |
| CLI | `--check` and write runs now exit with code 2 on such a file (CLI-017, CLI-019). The change doc's #4 shows the `error:` line but not the exit code; add it |
| Rule pages | not involved |

### Refuse option

The declared shape is the refusal the maintainer chose. The open question is its span: output question 2.

### Output questions

**2 · Formatter, CLI, and code fixes — an inline `/* */` comment behind `?` or `:` with the branch on the same line**

The guards that FMT-UNSUP-008 deletes fire only when the operator ends its line (`HasTrailingEndOfLine` in `NormalizeQuestionTokenPosition`/`NormalizeColonTokenPosition`, then `WouldJoinAcrossUnjoinableTrivia`). An ordinary `/* */` comment with the branch on the operator's line never reaches them. That part of the shape deletes nothing, and `AGENTS.md` says a shape that deletes nothing is not declared.

It also refuses code that no rule reports today and that the formatter keeps. Observed: the inputs below give `no-diagnostic` for RH5308, and `scripts/format.sh` leaves them as written. Lead decision 9 accepted only the multi-line, not-yet-laid-out `a ? /* c */ 1⏎ : 2`, which RH5308 does report.

Input:
```csharp
class C
{
    int M(bool a)
    {
        var x = a ? /* yes */ 1 : /* no */ 2;
        var y = a
                    ? /* yes */ 1
                    : /* no */ 2;

        return x + y;
    }
}
```

Before (`main`): formatted as written; not reported; every fix is offered.

After, A (the spec as written):
- The input is unchanged.
- `error: <path>(5,21): unsupported syntax: a comment or directive between a conditional operator and its branch. The file was left unchanged.`
- Exit code 2.
- Every fix whose formatted node holds either conditional is withheld, such as RH7101 in this type.
- Declared in this work.

After, B (recommended):
- Formatted as today, with no report and every fix offered.
- The refused shape becomes "a `//` or `///` comment, a `/** */` comment, a directive, or disabled text between the operator and its branch, or any comment there when the operator ends its line".
- `a ? // c`, `? 1 : // c`, the `#if` branch, and `a ? /* c */⏎ 1` are refused as in A.

Why B: an inline block comment keeps the branch on its operator's line, so it never breaks formatting, the reason the maintainer gave. Refusing it costs a CI error for code that formats cleanly today and buys no deleted special case.

B must keep `/** */` refused even inline. The documentation-comment phase moves a delimited documentation comment onto a line of its own: `b ? 1 : /** d */ 2` becomes `:` ⏎ ⏎ `/** d */ 2` (`DelimitedDocumentationInExpressionTests`). Without that, B's output would hold the refused shape (FMT-UNSUP-005).

If the lead holds that Q4's sentence ("comments are supported only above the `?` or `:` line") decided this literally, the recommendation is moot. Then the change doc's #4 must show the single-line and laid-out cases above as refused, because neither is reported today.

**3 · RH5308 · Conditional expressions should be formatted correctly — a refused conditional is still reported**

This needs the maintainer only if the posted Q4 C did not show the diagnostic staying.

- RH5308-005 keeps reporting the conditional. The change doc's #4 says "RH5308 reports the `?`, and no code fix is offered".
- SHARED-DIAG-010 says a layout rule whose fix formats "MUST NOT report a shape that the formatter deliberately keeps as it is: a shape that a requirement of the formatter leaves unchanged". FMT-UNSUP-008 is such a requirement.
- Lead decision 6 dropped the "report without a fix" option for this very input as contradicting SHARED-DIAG-010.
- The 03 reading, "refused is not kept", is defensible: the CLI reports the file and the user rewrites the comment, so the diagnostic is not permanent. But no spec says so.

Input: the change doc's #4 input. Before (`main`): reported on `?`, with a fix that changes nothing more.

After, A (recommended): reported on `?`, with no fix. SHARED-DIAG-010 gains one sentence: "A shape the formatter refuses as unsupported (FMT-UNSUP-001) is not a shape it keeps: the formatter asks the user to rewrite it."

After, B: not reported.

Why A: the IDE diagnostic and the CLI's `error:` line point to the same rewrite, and A follows the precedent of SHARED-FIX-010 ("the diagnostic is still reported").

Whether or not it reaches the maintainer, the SHARED-DIAG-010 sentence is needed, or the spec checker will flag RH5308-005 against SHARED-DIAG-010.

### Internal choices

_None beyond the three above._

### Issue goals

As in 02.

### Architecture findings

_None._

### Follow-up candidates

- **The layout FMT-UNSUP-008 points users to gets a blank line inside the expression.**
  - FMT-BLANK-011 (and RH5020, which mirrors it) puts a blank line in front of a comment on its own line in any token's leading trivia, `?` and `:` included.
  - Observed with `scripts/format.sh`, and with RH5020's fix, on the change doc's "not affected" example:

    ```csharp
            var x = a

                        // c
                        ? 1
                        : 2;
    ```

    RH5020 reports `// c` until the blank line is there.
  - The change doc shows that example without the blank line. It should show the output, or say it is input only.
  - The maintainer chose C partly because comments above the operator "do not break formatting". The lead may want to tell him about this when the change doc's outputs are confirmed.
  - Output question for a change of its own: **FMT-BLANK-011 / RH5020 — a comment on its own line in front of a wrapped operator.** Before: a blank line is inserted between `a` and `// c`. After A: no blank line inside an expression, and RH5020 does not report there. After B: as today. Recommend A.
- **`N(/* c7 */ 2)` comes out as `N(/* c7 */2)`** in FMT-UNSUP-008's other-side example. Observed. The space behind a block comment in front of an argument is dropped. This is outside this run. The test implementer must use the observed output, and the spacing is a follow-up candidate.
- 03's continuation-line candidate (`b + // c`, `int A => // c`) stands. RH5302's twin is in *Whole run*.

## thoenissen/Reihitsu#1143

No spec change, which is correct: SHARED-FIX-002 decides it.

The one new interaction: rules switched to `DocumentOrderFixAllProvider` re-register through `RegisterCodeFixesAsync`, so SHARED-FIX-012's base check applies to them as well. Nothing to change.

## thoenissen/Reihitsu#1673

RH5308-006 is correct and complete.

- `GetFormatTarget` returns the innermost `StatementSyntax`, or else the innermost `MemberDeclarationSyntax`.
- A conditional reaches neither only in an `[assembly:]` or `[module:]` attribute list. These cases all have a member declaration:
  - type and member attributes;
  - enum member values;
  - parameter defaults;
  - primary-constructor base arguments;
  - global statements and namespaces.
- `[assembly:]` cannot appear inside a namespace (CS1730).
- Its example gives one diagnostic on `?`. Observed today: `no-progress (1 iteration)`, the no-op this requirement removes.

It follows from SHARED-FIX-005, so it needs no change doc. Proceed.

## thoenissen/Reihitsu#1678

SHARED-ORDER-008's added reason carries out Q5 A. One wording point (internal):
- "and a move is judged in that build" reads as a general rule, against SHARED-FIX-003, *Build configurations* ("in every build configuration").
- It also reads against the rest of SHARED-ORDER-008, which does read excluded branches structurally.
- Recommend "and this gap leaves such a move to be judged in that build".

No visible change. Proceed.

## Whole run

### Does the diff carry out the answers and no more

| Answer | Carried out by | Beyond the answer |
|---|---|---|
| Q1 B | FMT-PIPE-033, `AGENTS.md` key convention | CLI and Playground reporting (output question 1); balanced-directive over-refusal (rare, lead's) |
| Q2 A | gap paragraphs in FMT-PIPE-033 and SHARED-FIX-012 | none |
| Q3 B | SHARED-FIX-012 | whole-document scope and renames (lead decision 9); casing Fix All path and RH2001 (test inputs) |
| Q4 C | FMT-UNSUP-008, RH5308-005, FMT-BREAK-018 Why | the inline sub-shape (output question 2); RH5308 keeps reporting (output question 3); the exit code 2 not shown in the change doc |
| Q5 A | SHARED-ORDER-008 | none (wording only) |
| Q6, Q7 | follow-ups, not in the diff | correct |

### Conflicts found

| Spec | Conflict | Resolution |
|---|---|---|
| SHARED-DIAG-010 | against RH5308-005 | add one sentence (output question 3) |
| SHARED-FIX-011 | against RH5302-005 (copies the formatter's join condition) and RH5401-004 | follow-ups |
| FMT-PIPE-033 Why | "as a file with a syntax error is" is not true of the CLI's output (CLI-008, CLI-014, CLI-015) or the Playground's (PLAY-006) | output question 1 |

I found no conflict with `unsupported-syntax.md`'s contract, FMT-PIPE-009, FMT-BREAK-001, or the `Uses` headers. I checked every new reference against its header.

### Change doc corrections (internal, for the spec author)

- *Special cases, Checked and kept*: add `DocumentationCommentFormattingPhase`'s delimited relocation.
- #4: add the exit code 2.
- The "not affected" example: show the formatted output, with the blank line, or mark it as input.
- *What*: the "one check before any fix registers" wording (#1600 *Internal choices*).
- FMT-PIPE-033 Why: the over-refusal sentence, and the CLI wording under output question 1.
- FMT-PIPE-003: the gap pointer.
- SHARED-ORDER-008: the wording.

### Follow-up candidates

**RH5302 · Logical expressions should be formatted correctly — comment behind an operator at the end of a line.** Declaring it unsupported is an option, so it is escalated as an output question (escalation 10, `AGENTS.md`). In a change of its own.

Input:
```csharp
class C
{
    bool M(bool a, bool b)
    {
        var x = a && // note
                b;

        return x;
    }
}
```

Before (`main`): reported on `&&`, no fix offered. The CLI moves `b;` to column 8.

After, A (recommended): the input is unchanged.
- The CLI reports `error: <path>(5,22): unsupported syntax: a comment or directive between a binary operator and its right operand. The file was left unchanged.`
- No longer offered: RH5302's fix and every fix whose formatted node holds such an operator.
- Declared in a change of its own.

After, B: not reported (SHARED-DIAG-010). The CLI output is as today.

Why A: it is the twin of FMT-UNSUP-008 and gives one rule for comments behind an operator. Declaring it would delete the join refusal in `BinaryOperatorLineBreakRewriter` (line 106), RH5302-005's fix-side copy of that refusal, and RH5302-005 itself.

The other follow-ups: the blank line in front of a comment above a wrapped operator (#1274), and `/* c */2` spacing (#1274).

### Lane

Behavior change, unchanged. Output questions 1–3 are visible-output questions about #1600 and #1274.

### Commands

- `scripts/build.sh` passed.
- `scripts/format.sh` passed on the fixtures for the carriers, the inline and single-line conditionals, the comment above the operator, the closure inputs, the syntax-error comparison with `--verbose`, and the RH5302 twin.
- `scripts/apply-fix.sh` passed for:
  - RH5308: the inline, single-line, RH5308-005 and RH5308-006 examples;
  - RH5302: the twin;
  - RH5020: the comment above `?`.
- Nothing in the repository changed except this file.
