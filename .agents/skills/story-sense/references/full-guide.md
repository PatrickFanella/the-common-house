# Story Sense Full Guide

Detailed diagnostic reference. Keep `SKILL.md` short and routing-focused.

## Decision Tree

```text
Is there anything on the page?
├── NO → story-idea-generator
└── YES → What's the dominant failure mode?
    ├── Feels generic → cliche-transcendence
    ├── World feels thin → worldbuilding
    ├── Non-humans feel fake → conlang
    ├── Characters flat → character-arc
    ├── Pacing off → scene-sequencing
    ├── Dialogue wooden → dialogue
    ├── Ending weak → endings
    ├── Meaning unclear → moral-parallax
    ├── Draft not progressing → drafting
    ├── Prose flat → prose-style
    └── Draft needs revision → revision
```

## State Notes (Extended)

### State 0: No Story

- Symptom: blank page.
- Entry intervention: `story-idea-generator`.

### State 1: Concept Without Foundation

- Symptom: idea exists but no stable premise/pressure.
- Intervention: `cliche-transcendence`, `key-moments`.

### State 2: World Without Life

- Symptom: setting exists but does not produce consequences.
- Intervention: `worldbuilding` and subsystem work (belief/economy/governance).

### State 3: Flat Non-Humans

- Symptom: species differ cosmetically only.
- Intervention: `conlang` + species logic constraints.

### State 4: Characters Without Dimension

- Symptom: characters act as plot devices.
- Intervention: `character-arc` + agency pressure design.

### State 4.5: Plot Without Pacing

- Symptom: scenes function locally but momentum decays.
- Intervention: `scene-sequencing`.

### State 5: Plot Without Purpose

- Symptom: events stack but don’t generate meaning.
- Intervention: `moral-parallax`, `key-moments`.

### State 5.5: Dialogue Feels Flat

- Symptom: same voice across characters, no subtext.
- Intervention: `dialogue`.

### State 5.75: Ending Doesn’t Land

- Symptom: setup strong, payoff weak.
- Intervention: `endings`.

### State 5.85: Draft Not Progressing

- Symptom: planning complete, pages not produced.
- Intervention: `drafting`.

### State 5.9: Prose Feels Flat

- Symptom: structural success, sentence-level weakness.
- Intervention: `prose-style`.

### State 6: Draft Complete, Needs Revision

- Symptom: draft exists, revision paralysis.
- Intervention: `revision`.

### State 7: Ready for Evaluation

- Symptom: story complete, quality/risk uncertain.
- Intervention: `sensitivity-check`, `story-analysis`.

## Script Usage

### `scripts/entropy.ts`

Inject creative randomness from curated lists.

```bash
deno run --allow-read scripts/entropy.ts lies
deno run --allow-read scripts/entropy.ts disasters --count 3
deno run --allow-read scripts/entropy.ts --combo
```

Lists: `lies`, `ghosts`, `disasters`, `dilemmas`, `professions`, `locations`, `collisions`, `openings`.

### `scripts/functions.ts`

Generate characters from abstract story functions.

```bash
deno run --allow-read scripts/functions.ts
deno run --allow-read scripts/functions.ts --setting scifi
deno run --allow-read scripts/functions.ts healer --setting fantasy
```

Functions include: `healer`, `enforcer`, `keeper_of_secrets`, `maker`, `trader`, `guide`, `entertainer`, `death_worker`, `transgressor`.

## Anti-Patterns (Extended)

1. **Prescribing instead of diagnosing**
   - Fix: ask clarifying questions before selecting intervention.
2. **Framework overload**
   - Fix: choose one intervention, then reassess.
3. **Ignoring writer energy**
   - Fix: balance precision with momentum.
4. **Treating structure as story**
   - Fix: keep testing emotional truth (“does this feel right?”).
