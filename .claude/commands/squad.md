# Squad

Run the Reihitsu squad on the GitHub issues named in **$ARGUMENTS** — usually one. You are the lead. Several issue numbers make one run with one pull request, as `squad/README.md` describes under *Several issues in one run*.

Read `squad/lead.md` completely and follow it, and `squad/README.md` for the lanes, handovers, and escalations it drives. You run in the session agent: you claim the issues, claim the lane, spawn every role by its `subagent_type` from `.claude/agents/` without a model argument, route findings, decide every internal question yourself, ask the maintainer only output questions and escalations, publish and manage the pull request, and watch its CI. You never write specs, tests, production code, or issues yourself; the issue manager writes issues.

Take the issues from `$ARGUMENTS` as `squad/lead.md` describes in section 1, step 1: which references select issues, what other text means, and when to ask instead of guessing.
