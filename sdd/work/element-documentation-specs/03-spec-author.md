Role: spec author
Commit: a36ff9a
Verdict: spec written

Repair round after `02-skeptic.md` (verdict "proceed narrower") and the decisions `squad/decisions/2026-10-18-rh8107-shared-doc-reference-clauses.md` and `squad/decisions/2026-10-18-rh8107-001-keeps-original-text.md`. I re-derived each change from the skeptic's findings against the code and my own runs. Its suggestions were taken as suggestions: where I did something different, it is noted below. The lead's message carried no intended fix or verdict to ignore. No change doc, production code, test, or rule page was touched.

## Changed requirements

- SHARED-DOC-001 — Every declaration of the rule's element kind is checked — changed: added the coverage sentence "an element with a single declaration; a partial type or constructor declared in several parts is not covered". Reason: Finding 4. The defining part of a partial constructor is never checked, which contradicts "check the declarations … wherever".
- SHARED-DOC-002 — Private and non-private groups — changed:
  - Added the coverage sentence "every declaration except a static constructor declared in an interface" (Finding 3).
  - The `Why:` now reads "a static constructor of a class, struct, or record … is private".
- SHARED-DOC-004 — The documentation comments of a declaration precede it — changed:
  - "First attribute list, modifier, or keyword" became "the declaration's first token — its first attribute list, modifier, keyword, or name".
  - Disabled text is now named among what does not separate the last documentation comment from the declaration.
  - Added a coverage sentence. It leaves out leading trivia in which an ordinary comment, a directive, or disabled text separates two documentation comments (Finding 1: the D8 and L1–L5 shapes).
  - The example is unchanged; it uses only single documentation comments.
- SHARED-DOC-005 — A top-level `<summary>` or `<inheritdoc>` documents the element — changed:
  - Now states "first documentation comment" explicitly.
  - Removed "matched case-insensitively" and "an element without an end tag counts", together with examples `C` (`<Summary>`) and `I` (no end tag). Both cases are now open questions (Finding 2 and question 4).
  - Coverage now reads: single declaration, placement as SHARED-DOC-004 covers, well-formed XML, lower-case tag names. A partial type or constructor declared in several parts is not covered.
  - Examples relettered A–G.
  - On question 4, I did not keep case-insensitive matching with a reason, as the skeptic offered. Accepting `<Summary>` as documentation, while the compiler and the tooltip do not treat it as a summary, contradicts the requirement's own `Why:`. So I left it out and list it for filing.
- SHARED-DOC-006 — A later documentation comment counts too — changed:
  - Added "when none of them has either element, undocumented".
  - Coverage is now "what SHARED-DOC-005 covers", so it excludes the separated shapes, malformed XML, and parts.
  - `Why:` corrected to "comments separated only by whitespace and line breaks".
  - The example is now the blank-line shape: D9 (not reported) plus two `<remarks>`-only comments (reported).
- SHARED-DOC-007 — An included documentation file counts — changed: coverage is now "what SHARED-DOC-005 covers, and an included file that is well-formed XML".
- RH8005-001, RH8011-001 — changed: `Why:` typos fixed ("An interface", "An enum").
- RH8017 — changed in two places, not in requirement text:
  - The primary-constructor non-goal now lists "RH8001 to RH8004 and RH8007 to RH8010".
  - The non-goal on RH8018 now names "the static constructors of classes, structs, records, and record structs".
- RH8018-001 — changed: "A static constructor of a class, struct, record, or record struct takes no access modifier, so it belongs to the private group". The intro line is narrowed the same way. A constructor declaration was already defined as one "in the body of a class, struct, record, or record struct", so neither RH8017 nor RH8018 claims the interface static constructor.
- RH8107-001 — restored to its exact text on `main`, as the maintainer decided (`git diff origin/main` shows no change to it).
- RH8107 `Uses` — now `SHARED-DOC-008` only. The SHARED-DOC-008 clause on RH8107-004 stays. The code-owner move from `01-spec-author.md` stays.

No ID was added, retired, or renumbered, and the ledger and `sdd/specs/README.md` are unchanged in this round.

### How the statements were established

- I ran all 29 C# example blocks of the 20 new specs through the scratch harness from `01-spec-author.md` (Roslyn 5.9.0, Debug `Reihitsu.Analyzer` at this commit, `XmlFileResolver`), in both `Diagnose` and `Parse` mode: 58 runs.
  - Every block's reported (rule, line, span text) set matched its markup exactly, with no compiler errors. That includes the SHARED-DOC-006 and SHARED-DOC-007 examples in `Parse` mode, which the skeptic found unverified.
  - The only difference is the SHARED-DOC-008 prose block, which is reported in `Diagnose` and `Parse` as its prose says. It is unchanged since `01-spec-author.md`, where its `None` run reported nothing.
- Separate runs:
  - Blank-line shapes D9 (remarks, blank line, summary) and D10 (summary, blank line, remarks): neither reported, in both modes. Two `<remarks>`-only comments separated by a blank line: reported.
  - Partial constructors (C# preview): an undocumented implementing part is reported; an undocumented defining part is not, in either the public pair or the private pair.
  - `public interface I { static I() { } }` is reported by RH8017.
- `scripts/test.sh --project architecture --filter "FullyQualifiedName~SpecConsistencyTests|FullyQualifiedName~SpecStructureTests"`: 26 passed.

## Examples for the tests

- **Requirements with examples:** RH8001-001 … RH8019-001 (each RH80nn-002 points at it), SHARED-DOC-001 to SHARED-DOC-007, and SHARED-DOC-008 (prose: `None` versus `Parse`/`Diagnose`).
- **Changed examples:**
  - SHARED-DOC-005: `<Summary>` and the unclosed `<summary>` are removed.
  - SHARED-DOC-006: now the blank-line shape.
- **Cases outside the specs:** the inputs listed under *Open questions to file* lie outside every requirement. They include the existing malformed-XML tests in `RH8001NonPrivateClassesMustBeDocumentedAnalyzerTests` (`/// <summary>A & B</summary>`, `/// <summary>Text <em`) and `RH8013NonPrivateEnumMembersMustBeDocumentedAnalyzerTests`.

## Open questions to file

All five go into one new issue, `SDD open questions: sdd/specs/shared/element-documentation.md`, as a sub-issue of thoenissen/Reihitsu#912. No such issue exists yet. Each case is left out of the specs through the coverage sentences named below.

### 1. Documentation comments separated by an ordinary comment, a directive, or disabled text

Input (RH8001, `Diagnose` mode):

```csharp
/// <summary>S</summary>
// c
/// <remarks>R</remarks>
public class D8
{
}

/// <remarks>R</remarks>
// c
/// <summary>S</summary>
// c
/// <remarks>R</remarks>
public class L1
{
}
```

Current output: `D8` is not reported and `L1` is reported.

- The analyzer reads the first documentation comment from the syntax tree. Only when that comment has no top-level `<summary>` or `<inheritdoc>` does it fall back to the compiler's documentation of the symbol.
- The compiler keeps only the documentation comments nearest the declaration that are separated by whitespace alone, and drops every earlier one with CS1587.
- So for `D8` the analyzer accepts a summary that the generated documentation does not contain. For `L1` it ignores a summary in a dropped comment.
- The skeptic observed the same for `#region`, `#if`/`#endif`, and `/* */` as separators (`02-skeptic.md`, table rows L3–L5). I reproduced `D8` with `// x`.

Why it looks wrong: the rule's `Why:` argues from the generated documentation and the tooltip, and in `D8` they show no summary.

Readings:
- (A) Only the comments the compiler keeps count: `D8` and `L1` are both reported.
- (B) Every documentation comment in the leading trivia counts: neither is reported.

Touches SHARED-DOC-004, SHARED-DOC-005, SHARED-DOC-006, and through them RH8001–RH8019. RH8107-001 has the same mechanism, since it reads the first comment, but it is outside this issue's spec.

### 2. Malformed documentation counts only in the first documentation comment

Input (RH8001):

```csharp
/// <summary>a & b</summary>
public class B
{
}

/// <summary>Text <em
public class U
{
}

/// <remarks>R</remarks>

/// <summary>a & b</summary>
public class A
{
}
```

Current output: `B` and `U` are not reported, and `A` is reported, in `Parse` and `Diagnose` mode.

- `A` was observed exactly as written, with a blank line between the two comments, and also with `// x` in its place. In both shapes the second comment is read only from the compiler's documentation, which is discarded as malformed.
- The compiler reports CS1570 for all three and writes "Badly formed XML comment ignored" into the generated file.
- The analyzer accepts `B` and `U` from the syntax tree of the first comment.

Why it looks wrong: whether the same `<summary>` counts depends on which documentation comment it sits in. For `B` and `U`, the generated documentation has no summary at all.

Readings:
- (A) A top-level `<summary>`/`<inheritdoc>` in any documentation comment counts, well-formed or not: nothing is reported.
- (B) Malformed documentation never counts: all three are reported. This contradicts the existing tests `VerifyNoDiagnosticForClassWithMalformedAmpersandInDocumentation` and `VerifyNoDiagnosticForClassWithUnclosedTagInDocumentation` (RH8001), and the malformed-ampersand test of RH8013.

Touches SHARED-DOC-005, SHARED-DOC-006, SHARED-DOC-007.

### 3. Partial types and partial constructors

Input (RH8001, RH8017, RH8018; partial constructors need C# 14):

```csharp
/// <summary>
/// Summary
/// </summary>
public partial class P
{
}

public partial class P
{
}

/// <remarks>
/// Remarks
/// </remarks>
public partial class P
{
}

/// <summary>PC</summary>
public partial class PC
{
    /// <summary>Defining int</summary>
    public partial PC(int value);
    public partial PC(int value) { }

    public partial PC(long value);
    /// <summary>Implementing long</summary>
    public partial PC(long value) { }

    partial PC(short value);
    partial PC(short value) { }
}
```

Current output:
- RH8001 is reported on the second `P` only.
- RH8017 is reported on the implementing `PC(int)`.
- RH8018 is reported on the implementing `PC(short)`.
- No defining part is ever reported, even when undocumented.

Mechanism:
- A part without a documentation comment counts as undocumented.
- A part with only `<remarks>` counts as documented, because the compiler joins the documentation of all parts.
- The generated documentation of `PC(int)` contains "Defining int". Why the defining part is skipped is not established; the skeptic found that the analyzer's code would report it if its action ran on that node.
- For comparison, outside this batch: RH8020 reports both undocumented parts of a partial method.

Why it looks wrong: the result depends on whether a part carries some unrelated comment, and on which part of a constructor is undocumented, not on whether the element is documented.

Readings:
- (A) A partial element is documented when any part has a top-level `<summary>` or `<inheritdoc>`, as the compiler documents it: nothing is reported here.
- (B) Every part must carry its own: the second and third `P`, the implementing `PC(int)`, the defining `PC(long)`, and both `PC(short)` parts are reported.

Touches SHARED-DOC-001, SHARED-DOC-005, SHARED-DOC-006, SHARED-DOC-007, and through them RH8001–RH8010, RH8017, and RH8018.

### 4. The static constructor of an interface

Input:

```csharp
/// <summary>I</summary>
public interface I
{
    static I()
    {
    }
}

/// <summary>C</summary>
public class C
{
    static C()
    {
    }
}
```

Current output: RH8017 reports `I`'s static constructor, and RH8018 reports `C`'s. Both compile.

Why it looks wrong: no code can call a static constructor in either place, and C# gives neither one an accessibility to choose, yet the rule depends on the containing declaration. The cause is that an interface member without a modifier is non-private.

Readings:
- (A) A static constructor is private wherever it is declared: RH8018 reports both.
- (B) As today: RH8017 reports `I`'s.

Touches SHARED-DOC-002, RH8017-001, RH8018-001.

### 5. Tag names matched case-insensitively

Input (RH8001):

```csharp
/// <Summary>Upper</Summary>
public class C
{
}

/// <INHERITDOC/>
public class D
{
}
```

Current output: neither is reported.

Why it looks wrong: XML element names are case-sensitive. The compiler writes `<Summary>` into the generated file unchanged, and documentation tools and the tooltip do not show it as the summary. Yet SHARED-DOC-005's `Why:` relies on what those show.

RH8107-001 (approved, on `main`) and FMT-DOC-008 match tag names case-insensitively too. That makes this possibly a repository convention, which is why the maintainer should decide it rather than the spec.

Readings:
- (A) Case-insensitive, as today: not reported.
- (B) Case-sensitive, like XML and the generated file: both reported.

Touches SHARED-DOC-005, SHARED-DOC-006, SHARED-DOC-007.

## Follow-up candidates (out of scope)

- Rule pages `documentation/rules/RH8001.md`–`RH8018.md` do not say what "private" means. This is unchanged from `01-spec-author.md`.
- For thoenissen/Reihitsu#983–#985: `HasImplicitPrivateAccessibility` treats a member of an extension block as implicitly private. None of this batch's kinds can be declared there, so SHARED-DOC-002 does not mention extension blocks. A later batch that adds methods or properties needs to.

## Decisions needed

_None._
