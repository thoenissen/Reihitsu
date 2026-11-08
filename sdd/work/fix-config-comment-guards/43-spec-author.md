Role: spec author
Commit: c70f7eb3
Verdict: spec written

The prompt named decisions 38–40 as the input, which are the lead's recorded decisions. It carried no suspected cause or hoped-for verdict beyond them.

How I measured: `scripts/apply-fix.sh` on a temporary worktree of `origin/main` (`3578f98e`), LF and CRLF. Fixtures and the worktree lived in the scratchpad and are removed. `main` fixes all of these:

- RH7309, struct, `get => 0; set;`
- RH7309, struct, `get => 0; #if DEBUG set; #endif`
- RH7103, class, the same accessors, crossed by `Shared`
- RH7103, `#if DEBUG [System.Serializable] #endif` on the class
- RH7103, a whole `#if DEBUG` class `D` in front of `C`
- RH7109, `#if INTEROP [StructLayout(LayoutKind.Sequential)] #endif` on a class
- RH2001 with `extern`, `unsafe`, or `readonly` (struct) in a group behind `private`. Here `main` writes `readonly` in front of the group, and under CRLF it also reports `line-ending-drift`.

Process note: while I worked, the parallel test implementer ran `git stash`, which took my uncommitted spec edits with it. The stash was popped again, and my edits are intact. I committed only my files.

## Changed requirements

- **SHARED-ORDER-007 — Initializer execution order is kept — changed (decision 38).**
  - The backing-field rule is now stated once. An instance property that is not abstract, not extern, and not the defining declaration of a partial property has a backing field when *any* accessor of its list has neither a body nor an expression body, or when it uses `field`. `get => 0; set;` therefore counts.
  - The conditional-accessor count is removed. A group of whole accessors that could decide whether there is a backing field now refuses the move under SHARED-ORDER-008.
  - The old conditional example moves to SHARED-ORDER-008. A new example replaces it: an RH7309 struct with `get => 0; set;` gets no fix. Its counterpart, `get => 0; set { }`, keeps the fix.
- **SHARED-ORDER-008 — Only conditional groups of whole units keep a member move — changed (decisions 38 and 39).**
  - New condition *No backing field from a branch*: in an accessor-list group, no branch (active or excluded) may hold an accessor without a body, unless no accessor of the list, in or around the group, has a body.
  - A matching bullet is added to the refusal list, and the structural-test paragraph and the Why are updated.
  - New normative paragraph: no move at all in a type whose declaration holds a conditional directive in front of its body's `{`. This covers:
    - the leading trivia of any token after the declaration's first token, up to and including `{`;
    - the leading trivia of the first token, when the directive's group has an excluded branch that does not read as whole members.

    A group of whole types in front of the declaration keeps the moves. "A directive outside both spans is not read" now names this exception.
  - New examples:
    - acc1 (RH7309, moved here from 007), no fix;
    - an RH7103 class with `get => 0; #if DEBUG set; #endif`, no fix, with the `set { }` and `get;` counterparts in the prose;
    - RH7109 with a conditional `[StructLayout]`, no fix;
    - RH7103 with a conditional `[System.Serializable]`, no fix;
    - RH7103 with a whole `#if DEBUG` class in front of the type, fix kept, with its output.
- **RH2001-009 — retitled (decision 40).** New title: "…would cross, or for a conditional group of other modifiers". New MUST NOT: a conditional group with a directive between two modifiers, or between the last modifier and the type, withholds the fix when any branch (active or excluded) holds anything but `public`, `private`, `protected`, `internal`, `new`, or `static`. Comments and non-modifiers in a branch withhold it too. Decision 36's placement now applies to "such a group". New Why sentences. New example: `extern` and `unsafe` in class `C`, `readonly` in struct `S`, all reported with no fix. The `Uses` row adds SHARED-FIX-009, which the Sdd tests required.
- **SHARED-FIX-009 — changed.** The first bullet now also allows reading "the names or modifier keywords" of a branch. The "reads no excluded code" paragraph no longer gives RH2001 as its example. Instead it says that RH2001 reads the modifiers of conditional groups, and only to withhold.
- **Change doc `member-move-conditional-groups.md`.**
  - *What* is updated for decisions 38 and 39.
  - *Visible output changes* gains two withholdings that change no build, each with `main`'s output: RH7103 with the mixed conditional accessors, and RH7103 with the conditional attribute on the type.
  - It also lists three SHARED-FIX-003 bug-fix withholdings: plain `get => 0; set;` (RH7309), the conditional `set;` in a struct (RH7309), and the conditional `[StructLayout]` (RH7109).
  - The *Unchanged* paragraph is narrowed.
  - *Decisions*: the "conditional accessor counts" entry is replaced by decision 38, and decision 39 is added.
  - *Alternatives*: three are updated or added.
- **Change doc `rh2001-modifier-reorder-trivia.md`.** Adds SHARED-FIX-009 (changed) to *Affected*, a *What* bullet, and a *Visible output changes* entry: `extern` with `main`'s before, plus the `readonly` and `unsafe` cases in prose. It adds a *Decisions* entry for decision 40, corrects the decision-36 entry's last sentence, and adds one rejected alternative. The status stays Accepted.

No rule page changed: these are withheld fixes in corner cases. `scripts/test.sh --project architecture --filter Sdd`: 51/51 pass.

## Examples for the tests

- **SHARED-ORDER-007:** the plain `get => 0; set;` RH7309 example (no fix) and its `set { }` counterpart (fix).
- **SHARED-ORDER-008, the new examples:**
  - acc1 (RH7309);
  - the RH7103 class `get => 0; #if DEBUG set; #endif` (no fix), with counterparts `set { }` and `get;` (fix, as in the 3A example);
  - the conditional `[StructLayout]` (RH7109) and the conditional `[System.Serializable]` (RH7103), both no fix;
  - the whole-type `#if` block in front of `C` (RH7103, fix with output).
- **RH2001-009:** the `extern`/`unsafe`/`readonly` example (all three without a fix). The decision-36 example stays as it is.

## Output questions

The standing answer covers both of these. I wrote the spec on the recommended option of each. The lead records them and states them in the PR.

1. **RH7103/RH7109 · a member move in a type declared after a conditional block of another type.** Decision 39's words, "a conditional directive in the type's attribute lists or header", read literally, would also refuse this input, because the `#endif` stands in the leading trivia of `C`'s first token:

   ```csharp
   #if DEBUG
   public class D
   {
   }
   #endif

   public class C
   {
       public void Run()
       {
       }

       public static void Create()
       {
       }
   }
   ```

   - Before (`main`): the fix moves `Create` in front of `Run`.
   - **A (recommended):** fix kept, same output as `main`. A group in front of the declaration whose excluded branches read as whole members keeps the moves.
   - **B:** no fix offered.
   - Why A: B withholds the fix for code people commonly write, which *Special cases do not grow the source* does not license. A needs no new reading: it reuses the existing whole-member parse.
2. **RH7103 · whole accessors (batch 5, 3A) against decision 38.** Decision 38's words, "a bodiless accessor in any branch refuses", read literally, would also refuse the 3A example `get; #if DEBUG set; #endif`, which the maintainer kept.
   - **A (recommended):** refuse only when the list also holds an accessor with a body, as in the auditor's option (b). 3A keeps its fix.
   - **B:** refuse every such group, which withdraws 3A's fix.
   - Why A: A keeps the maintainer's 3A answer. That group gives the property a backing field in every build, and the active count sees it.

## Internal choices

- Decision 39 is stated inside SHARED-ORDER-008, as a paragraph with examples, and gets no new ID: the lead's prompt placed it there. The alternative was a new SHARED-ORDER-009.
- The allowed set in RH2001-009 is "only modifiers". A comment in a branch withholds the fix (the safe side). The alternative was to allow whitespace and comments too.
- The SHARED-FIX-009 wording was adjusted because RH2001 now reads excluded branches. Without the change, its paragraph contradicted RH2001-009.

## Follow-up candidates

- **A conditional `[StructLayout]` or `struct` on another partial declaration of the reordered type.** Decision 39 covers only the declaration being reordered, while `FieldOrderSetsLayout` reads every part through the symbol. Rare, and unchecked.
- **RH2001 with a conditional group in front of the property's first modifier** (`#if DEBUG extern #endif private int P { get; }`). RH2001-009 covers groups between modifiers only, and RH2001-007 leaves the property's leading trivia uncovered. A build with `DEBUG` would get an extern field. Unchecked with `apply-fix.sh`.
- **Conditional `using` directives directly in front of a type** in a file without a namespace. With the symbol undefined, the excluded `using` does not read as whole members, so the type's moves are refused. This is safe and rare. It is not a defect, only noted in case it proves common.
