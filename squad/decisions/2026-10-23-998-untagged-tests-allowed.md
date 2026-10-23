# Whether every test method must carry a requirement tag or an exemption

| | |
|---|---|
| Date | 2026-10-23 |
| Escalation | 5 |
| Run | issue #998 |

## Question

Before the test implementer started, three questions on the issue's "every test method is tagged or visibly exempted" rule, given about 3,050 untagged tests in the shipped-surface test projects: 1, what a test that verifies no requirement on its own does (1A exempt it visibly and keep "a test that verifies a requirement only incidentally does not tag it"; 1B tag the nearest requirement; 1C add a requirement to its spec); 2, how architecture and tooling tests are handled (2A exempt them as a class with one reason; 2B tag them against a new conventions spec); 3, how fine-grained an exemption must be (3A per method; 3B per class with method overrides; 3C a baseline list of today's untagged tests).

## Answer

None of the options: the maintainer narrowed the issue's scope. The per-test enforcement is dropped, because an exemption attribute would only repeat the test's summary. The run keeps the switch of `RequirementCoverageTests` to opt-out — every spec area enforced by default, with an empty exemption list or a reason per entry. A test without a `[Requirement]` tag stays allowed, the rulebook keeps "a test that verifies a requirement only incidentally does not tag it", and there is no exemption attribute and no exemption list for tests. Architecture and tooling tests need no special handling. New tests are covered by process instead: the test-implementer role states that every test derived from a requirement carries that requirement's tag, and the spec checker or the auditor checks that the tests a run adds for a requirement carry its tag. `sdd/README.md` (*Tests and requirements*, *Checks*) and the change doc state this.

## Reason

An exemption attribute would only repeat the test's documentation summary.
