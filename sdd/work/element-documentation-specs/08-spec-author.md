Role: spec author
Commit: 473c841
Verdict: spec written

This is the repair round for findings 1 and 2 of `07-auditor.md`. I re-derived both from the code and from my own runs. I followed the auditor's suggestions where my analysis agreed, and I narrowed one more sentence for a case I found while checking finding 2 (see SHARED-DOC-005 below). The lead's message carried no intended fix or verdict to ignore. I did not touch any production code, test, rule page, ledger line, or index row. No ID was added, retired, or renumbered, and no example changed.

## Changed requirements

- **SHARED-DOC-002 — Private and non-private groups — changed** (finding 1).
  - Coverage sentence: "every declaration except a static constructor declared in an interface **and a partial type declared in several parts**". This is the same exclusion SHARED-DOC-001 and SHARED-DOC-005 already make, so the three agree.
  - `Why:` corrected. It said a modifier-less element in a class, struct, or record is private, which is false for a part of a partial type. It now adds: "a partial type declared in several parts is the exception, because it takes the accessibility that any of its parts declares."
  - I chose the whole-type exclusion over a narrower one ("a part without a modifier whose sibling has one"). A multi-part partial type is outside SHARED-DOC-001 and SHARED-DOC-005 anyway, so a narrower SHARED-DOC-002 would specify a group whose consequence no requirement states.
- **SHARED-DOC-005 — A top-level `<summary>` or `<inheritdoc>` documents the element — changed** (found while checking finding 2, not in the audit).
  - Coverage now reads "with the tag names `summary` and `inheritdoc` written in lower case, **without a namespace prefix, and in no XML namespace**".
  - Reason: well-formed `/// <x:summary xmlns:x="urn:x">…</x:summary>` and `/// <summary xmlns="urn:x">…</summary>` are accepted as documented, with no CS1570. The analyzer compares only the local name, and the compiler and tools do not treat a namespaced element as the summary. This is the same kind of question as Q5, and SHARED-DOC-005 covered it as intended.
- **SHARED-DOC-006 — no text change.** Its coverage is "what SHARED-DOC-005 covers", so it narrows with SHARED-DOC-005.
- **SHARED-DOC-007 — An included documentation file counts — changed** (finding 2). Coverage now reads "an included file that is well-formed XML and writes the tag names `summary` and `inheritdoc` as SHARED-DOC-005 covers them: in lower case, without a namespace prefix, and in no XML namespace". SHARED-DOC-005's lower-case condition was about the documentation comments only, so an included `<Summary>` was covered and contradicted the normative sentence.

### How the statements were established

I used a private copy of my harness, `sa-harness` in the session scratchpad: Roslyn 5.9.0, the Debug `Reihitsu.Analyzer` of this tree, an `XmlFileResolver`, and optional CS1570/CS1587 output. The scratchpad folder `harness`, which `01`/`03` used, now holds a different single-file harness written by another role. I left its source unchanged; I only rebuilt it once.

- **Finding 1**, in `Diagnose` and `Parse`. Inside a documented `public class Outer`:
  - `public partial class Visible` with a summary, plus a modifier-less `partial class Visible`: RH8002 on the second part.
  - The reverse order (the modifier-less part first, then `internal partial class Reverse` with a summary): RH8002 on the modifier-less part.
  - Undocumented pairs:
    - `internal partial struct S` and `partial struct S`: RH8003 and RH8004.
    - `public partial interface IV` and `partial interface IV`: RH8005 and RH8006.
    - `public partial record R` and `partial record R`: RH8007 and RH8008.
    - `internal partial record struct RS` and `partial record struct RS`: RH8009 and RH8010.
    - `private partial class Both` and `partial class Both`: RH8002 on both.
- **Finding 2**, with `upper.xml` selecting `<Summary>From the file</Summary>` and, for another member, `<INHERITDOC/>`: neither class is reported, in both modes.
- **Namespaced names:** the cases listed under SHARED-DOC-005 above, and the same prefixed summary in a later comment after a blank line. None is reported, in both modes, and no CS1570 is raised.
- **Auditor hint for Q2:** `/// <x:summary>Undeclared</x:summary>` is not reported, and the compiler raises CS1570 ("'x' is an undeclared prefix"). Confirmed.
- **Examples:** I re-ran all 29 example blocks in `Diagnose` and `Parse` (58 runs). All match their markup. The SHARED-DOC-008 prose block is reported in both modes, as its prose says, the same as in `03-spec-author.md`.
- **Architecture tests:** `scripts/test.sh --project architecture --filter "FullyQualifiedName~SpecConsistencyTests|FullyQualifiedName~SpecStructureTests|FullyQualifiedName~RequirementCoverageTests"`: 39 passed.
- **Tags:** I scanned the tests tagged SHARED-DOC-001/002/005/006/007 for partial types, upper-case tags, and namespaces. The only hits are `VerifyNoDiagnosticForPartialClassWithSummary` (RH8001) and `VerifyNoDiagnosticForPartialRecordWithSummary` (RH8007). Both have a single part, which is still covered, so no tag changes are needed.

## Examples for the tests

No example changed. The cases above are outside the specs, and no test may assert them.

## Open questions to file

These are additions to thoenissen/Reihitsu#1037. The text below is ready to paste.

### Addition to Q3 — paste at the end of Q3, before its "Touches" line, and replace that line

````markdown
### Q3, continued — the group of a part without an access modifier

Input (RH8001–RH8010):

```csharp
/// <summary>Outer</summary>
public class Outer
{
    /// <summary>Visible</summary>
    public partial class Visible
    {
    }

    partial class Visible
    {
    }

    internal partial struct S
    {
    }

    partial struct S
    {
    }
}
```

Current output, in `Parse` and `Diagnose` mode:
- RH8002 ("private classes") reports the second `Visible`; RH8001 reports nothing.
- RH8003 reports the first `S` and RH8004 ("private structs") the second.
- The same split holds for `partial interface` (RH8005/RH8006), `partial record` (RH8007/RH8008), and `partial record struct` (RH8009/RH8010), and whichever part comes first.

Mechanism: the group is decided from each part's own modifiers, so a part without one is put in the private group inside a class, struct, or record. C# gives a partial type the accessibility that any of its parts declares, so `Visible` is public. Partial constructors cannot reach this case: CS8799 requires the same accessibility modifiers on every part.

Why it looks wrong: the parts of one public type are judged by the private rule and the non-private rule, so a project that turns the private rules off still sees part of a public type reported, or misses it.

Readings, for the group only (who reports a part that is undocumented):
- (A) A partial type has the group of its declared accessibility, taken from all parts: the second `Visible` and the second `S` belong to RH8001 and RH8003.
- (B) As today: each part's own modifiers decide, and they belong to RH8002 and RH8004.

The group question combines with the documentation question above. Under its reading (A), `Visible` is reported by neither group, because one of its parts is documented. `S` stays undocumented under either reading.

Touches SHARED-DOC-001, SHARED-DOC-002, SHARED-DOC-005, SHARED-DOC-006, SHARED-DOC-007, and through them RH8001–RH8010, RH8017, and RH8018.
````

The final "Touches" line above replaces Q3's current one: SHARED-DOC-002 is added, and the rest stays.

### Addition to Q2 — paste after Q2's input block

````markdown
Another shape: a `<summary>` with an undeclared namespace prefix.

```csharp
/// <x:summary>Undeclared</x:summary>
public class Prefixed
{
}
```

Current output: not reported. The compiler reports CS1570 ("'x' is an undeclared prefix"), so the documentation is malformed. The analyzer compares only the local name `summary` of the first comment's top-level elements.
````

### Addition to Q5 — paste at the end of Q5, before its "Touches" line

````markdown
Included content is matched the same way. With `upper.xml`

```xml
<doc>
    <members>
        <member name="A">
            <Summary>From the file</Summary>
        </member>
        <member name="B">
            <INHERITDOC/>
        </member>
    </members>
</doc>
```

and

```csharp
/// <include file='upper.xml' path='doc/members/member[@name="A"]/*'/>
public class UpperIncluded
{
}

/// <include file='upper.xml' path='doc/members/member[@name="B"]/*'/>
public class UpperInheritdoc
{
}
```

neither class is reported, in `Parse` and `Diagnose` mode.

The match also ignores XML namespaces. These are well-formed (no CS1570), and none is reported:

```csharp
/// <x:summary xmlns:x="urn:x">Prefixed</x:summary>
public class Declared
{
}

/// <summary xmlns="urn:x">Default namespace</summary>
public class DefaultNamespace
{
}
```

The same holds for the prefixed summary in a later documentation comment after a blank line. The analyzer compares only an element's local name, case-insensitively, in the comment and in the compiler's expanded documentation. The compiler and documentation tools treat neither a `<Summary>` nor an element in another namespace as the summary.

Readings extended:
- (A) As today: any element whose local name is `summary` or `inheritdoc`, in any case and any namespace, counts.
- (B) Only `summary` and `inheritdoc` in lower case and in no namespace count: every class in this question is reported.
````

Q5's "Touches" line stays SHARED-DOC-005, SHARED-DOC-006, SHARED-DOC-007.

## Follow-up candidates (out of scope)

- Findings 3 and 4 of `07-auditor.md` belong to the test implementer and the cleaner.
- The extension-block hint for thoenissen/Reihitsu#983–#985 stands, as noted in `03-spec-author.md`.

## Decisions needed

_None._
