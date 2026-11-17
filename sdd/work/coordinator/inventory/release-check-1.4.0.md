# Release-readiness check — Reihitsu 1.4.0

Checked on a detached worktree of `origin/main` at `06a2f5d3` (28 commits since `v1.3.0`), built with `scripts/build.sh` (Release, 0 warnings). For comparison, the `v1.3.0` CLI was built in a second worktree. Probes used the built CLI (`reihitsu-format`) and `scripts/apply-fix.sh` on small inputs in the scratchpad. There was also a differential run over a real-world corpus: shallow clones of Newtonsoft.Json, serilog, AutoMapper, and command-line-api, 1,886 `.cs` files. Nothing was committed or pushed.

Scope list verified against milestone 6 (44 issues, all closed except #1850, which covers scripts/CI) and against `git log v1.3.0..origin/main`. Most of the 28 commits are internal: tests, scripts, squad, and architecture tests. Of the user-visible ones, #1822 (no elastic trivia) and #1841 (type-kind set) change no output, and #1821 is spec-only (see 2).

## Verdicts

| # | Feature | Verdict |
|---|---|---|
| 1a | FMT-UNSUP-010: a directive inside a multi-declarator field (#1809) | good |
| 1b | FMT-UNSUP-011: a comment or directive behind a binary operator (#1831) | good, with one observation (O1) |
| 1c | FMT-UNSUP-012: a comment or directive behind an expression body's `=>` (#1839) | good |
| 2 | #1587 / #1821: a split field repeats the line break behind the type | good (spec-only: no behavior change, and the output equals v1.3.0's) |
| 3 | #1786 / #1813: argument list joined onto its name; #1781 / #1812: the first comment behind `(`, `[`, `<` | good |
| 4 | #1795 / #1815: `--max-parallelism` | good |
| 5 | #1759 / #1832: rules report across conditional directives, and fixes are withheld | good |
| 6 | #1802 / #1840: precheck performance; #1788 / #1841: the type-kind set | good |

**No gap found in what 1.4.0 ships.** No blocker, no should-fix. One release-step reminder and one observation for the maintainer are below.

## Evidence per feature

### 1. New unsupported shapes (FMT-UNSUP-010, -011, -012)

- **Spec conformance.** I wrote 37 single-file probes, one per bullet of the three requirements, refused (R) and not refused (N). Every probe gave the expected verdict, the reason text, and the location:
  - **R, FMT-UNSUP-011:** `is`+enum member, `is`+type, `??`, `as`, a one-line `*`, and a binary operator inside a lambda.
  - **N, FMT-UNSUP-011:** `is` with a constant pattern, `is not null`, the pattern combinators `>`/`or`, `+=`, the range `..`, a comment inside a parenthesized right operand, a comment in front of the operator, and the disabled-text `| B` example.
  - **R, FMT-UNSUP-012:** `get`, `init`, `add`, constructor, operator, conversion operator, finalizer, indexer, and a local function inside a lambda.
  - **N, FMT-UNSUP-012:** a lambda, a switch arm, a comment before `=>`, a comment before `;`, and a comment inside an argument.
  - **R, FMT-UNSUP-010:** `[Obsolete]` followed by a `#pragma` in front of `private int _a, _b;`, and `#if` inside a lambda value.
  - **N, FMT-UNSUP-010:** an event field, a local declaration, a single declarator with `#if` among its modifiers, `, _b` as disabled text in front of `;`, and `#if` around the whole field.
- **CLI.** A refused file is reported as `error: <path>(l,c): unsupported syntax: <reason>. The file was left unchanged.` and the file is not touched. Exit code 2; the summary counts it as skipped.
- **Ordinary code is not refused.** None of the following is refused:
  - comments on their own line above members or above an operator;
  - trailing `// c` behind an expression body's `;` or behind a right operand;
  - `a /* c */ && b`;
  - `#pragma`, `#region`, or `#if` around whole multi-declarator fields. These are now split, where v1.3.0 kept them whole.
- **Corpus (1,886 files).** v1.4.0 refuses 6 files and v1.3.0 refuses 4. The 2 new refusals are both FMT-UNSUP-011 in trailing-operator style (see O1). There are no FMT-UNSUP-010 or FMT-UNSUP-012 refusals. After a write run, `--check` reports 0 of 1,886 files, so the output is idempotent. Only 4 files' output differs from v1.3.0: the 2 refused files (v1.3.0 had re-indented `value > …` after a `#if` behind `||`), and 2 files where blank lines are no longer inserted behind `(` (the #1812 behavior).
- **SHARED-FIX-010.** The following fixes are withheld when their formatted node holds the shape:
  - RH5302 (`a && // c⏎b`; `b && /* c */ c`);
  - RH3202 (`=> a && // c⏎b`; `=> /* c */ 42`; `=>⏎// c⏎42`);
  - RH5401 (`=>⏎// c⏎1`; `=>⏎_a && /* c */ _b`);
  - RH7101 (a directive in the declarators; a shape elsewhere in the type).

  The fixes are still offered on clean siblings: RH5401 on a property without the shape, and RH7101 behind `#region`. In v1.3.0, RH3202 on `=> a && // c⏎b` produced `return a && // c⏎        b;` with the operand in the column of `return`; v1.4.0 withholds it.
- **Deleted guards are gone:**
  - `FieldDeclarationSplitTransform.CarriesDirective`;
  - RH5302's `IsTrailingOperator`;
  - `LeadsWithCommentOnItsLine`;
  - the arrow-trailing parts of `ArrowLineHoldsComment`, of which only the part for a comment in front of the arrow remains, as the change doc keeps.

  The phases do not produce the new shapes. A block getter with `return /* c */ _v;` stays a block, and `get { return _v /* c */; }` becomes `=> _v /* c */;`. A second run is stable.

### 2. Split field repeats its head (#1587, PR #1821)

The PR only specifies today's behavior in FMT-STRUCT-021 and adds tests. The v1.3.0 and v1.4.0 outputs for `Dictionary<…>⏎    _a = new(), _b = new();` and `private⏎    int _c, _d;` are identical and match the spec example. The repeated declarator line starts at the member column, which the spec shows as today's output.

### 3. Argument-list join and first comment (#1786, #1781)

- **Joined onto their names in 1.4.0 (left on the next line in v1.3.0):** `new C⏎(1)`, `nameof⏎(M)`, a delegate call `f⏎(1)`, `M⏎(⏎// c⏎a, b)`, and `: this⏎(1)`.
- **Left as written:** `WriteLine // why⏎(a)` and `#if` in front of `(`, both by guard.
- **No blank line added** above the first comment behind `(`, `[`, or generic `<`.
- **Blank line still added** above a comment behind a comma, both by the formatter and by the RH5020 fix.

### 4. `--max-parallelism` (#1795)

- **Help, README, and spec agree.** `--help` lists the option with the CLI-023 wording, and `src/Reihitsu.Cli/README.MD` documents it with an example.
- **Validation follows CLI-023.**
  - Rejected with `Invalid value … Expected a whole number of 1 or more.` and exit code 2: `0`, `-1`, `abc`, an empty value, `1.5`, `+3`, and `99999999999`.
  - A missing count gives `Missing value for --max-parallelism.`
  - Accepted: `03`, `2147483647`, and `1000`. When the option is repeated, the last value is used.
- **Output is deterministic.** On the corpus, `--check --verbose` output with the default and with `--max-parallelism 1` is byte-identical, and the file order equals v1.3.0's sequential order. The summary and exit code are unchanged.
- **Ctrl+C in write mode.** It prints `Operation canceled.` and exits with code 2. All 99 files written before the cancel are named, and no other file changed.

### 5. Reporting across conditional directives (#1759)

With no symbols defined, each of the following is reported and gets **no** fix:

- RH5409 (comma in front of `#if` and in front of an empty `#if`/`#endif`);
- RH5410 and RH5411 (comma in front of `#if`);
- RH8201 (file holds `#if`);
- RH7105 (empty `#if` between modifiers);
- RH7102 and RH7103 (`#if`/`#else` member pairs);
- RH7107 and RH7108 (accessor inside `#if`/`#else`);
- RH5102, RH5109, and RH5111 (`#if` in the judged span);
- RH5506 and RH5524 (the change-doc examples);
- RH5412 (only member inside `#if DEBUG`).

The trailing-comma fixes withhold per diagnostic in `RegisterCodeFixesCoreAsync`, so the BatchFixer (Fix All) withholds them too. Fixes stay offered where they are safe:

- RH5409 with a comment or `#region` in front of `}`;
- RH8201 in a file without `#if`;
- RH7107 across an empty, balanced `#if X`/`#endif`. The directives travel with `get`, which SHARED-ORDER-003 allows, and every build stays the same.

No fix moved code that holds a directive. The rule pages lost their *Exceptions* sections, and `general-notes.md` states the new reading.

### 6. Precheck performance and the type-kind set (#1802, #1841)

The precheck is linear. A file with a 4k/8k/16k-element array initializer and a refused shape at its end is refused in 0.22/0.26/0.37 s. Whole-file formatting of large initializers grows about quadratically (4k elements: 5.2 s; 8k: 22 s), but v1.3.0 is the same (5.8 s at 4k), so this is not a 1.4.0 regression and is out of scope. #1841 is an architecture test only.

## Release-step reminder

- `Directory.Build.props` still has `<VersionPrefix>1.3.0</VersionPrefix>`, so `reihitsu-format --version` on main prints `1.3.0+06a2f5d3`. Earlier releases bumped this in a separate commit ("Bump version to 1.3.0"), so this is an ordinary release step, not a gap.

## Observation for the maintainer (not a gap; the spec decides it)

- **O1. The trailing-operator style with a comment or `#if` behind the operator is now refused as a whole file.** FMT-UNSUP-011 declares this deliberately, and the refusal is reported. The two corpus hits are typical:

  ```csharp
  if (value < DateParseHandling.None ||
  #if HAVE_DATE_TIME_OFFSET
      value > DateParseHandling.DateTimeOffset
  #else
      value > DateParseHandling.DateTime
  #endif
      )
  ```

  ```csharp
  info.Name == method &&
  // check that the method overrides the original on DynamicObjectProxy
  info.DeclaringType != methodDeclaringType
  ```

  The rate is 2 of 1,886 files (0.1%) in code that was never formatted by Reihitsu. Release notes should mention that such files are now reported as unsupported, with the remedy: move the comment or directive in front of the operator, or use leading operators.

## Gaps

None.
