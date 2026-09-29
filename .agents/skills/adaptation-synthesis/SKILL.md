---
name: adaptation-synthesis
description: Synthesize new works from extracted functional DNA documents. Use when adapting a source work to a new context, when combining multiple source extractions, or when generating variations that preserve function while changing form.
license: MIT
metadata:
  blacktower-pack: creative
  blacktower-kind: workflow
  blacktower-tier: standard
  blacktower-capabilities: worldbuilding-fiction,adaptation,synthesis
  author: jwynia
  version: "1.0"
  type: generator
  mode: generative
  domain: fiction
---

# adaptation-synthesis

## Use this skill

Synthesize new works from extracted functional DNA documents. Use when adapting a source work to a new context, when combining multiple source extractions, or when generating variations that preserve function while changing form.

## Workflow

1. Confirm the request matches this skill's stated intent; do not activate it
   for a keyword-only near miss.
2. Inspect the user's inputs, repository conventions, and available tools before
   selecting an approach.
3. Load only the relevant section of the detailed guide when the task needs
   domain-specific patterns or edge cases.
4. Perform the smallest complete unit of work and preserve existing behavior or
   user data unless change is explicitly requested.
5. Validate the result with the project's own checks plus any skill-specific
   verification described in the detailed guide.

## Constraints

- Prefer the project's existing stack and conventions over generic examples.
- State required credentials, network access, CLIs, and destructive side effects
  before relying on them.
- Use current authoritative documentation when provider or framework behavior is
  version-sensitive.
- Report concrete evidence, remaining risks, and any unverified assumptions.

## Resources

- `references/detailed-guide.md` — detailed framework and edge cases; load only the sections needed for the task.
- `scripts/` — deterministic helpers; inspect --help and side effects before use.
