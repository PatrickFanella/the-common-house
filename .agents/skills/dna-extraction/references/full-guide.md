# DNA Extraction — Full Guide

Detailed reference moved from `SKILL.md` for progressive disclosure.

## Core Principle

**The first ideas when adapting are surface elements. Functional DNA is what elements DO, not what they ARE.**

Example (Hamlet): prince status is form. Functional DNA includes:
- Proximity to power center without being power holder
- Structural obligation vs personal desire
- Insider access to corruption without clean authority to act

## State Model (EX0–EX7)

### EX0: No Extraction
- Symptoms: source named, analysis not started
- Ask: source work? medium? extraction goal?
- Intervention: start with emotional core + genre identification

### EX1: Surface Reading
- Symptoms: plot summary instead of function
- Ask: why element exists, what breaks if removed, what audience feels
- Intervention: six-axis extraction per element

### EX2: Single-Axis Extraction
- Symptoms: only plot or only character/thematic extraction
- Ask: what other functions does this element serve?
- Intervention: force all axes + cross-reference genre

### EX3: Missing Emotional Core
- Symptoms: mechanical mapping, no emotional experience defined
- Ask: what do fans love emotionally?
- Intervention: emotional beat mapping + primary/secondary genre identification

### EX4: Structural/Stylistic Conflation
- Symptoms: style treated as load-bearing structure
- Ask: if changed, does story break?
- Intervention: classify structural vs stylistic

### EX5: Missing Relationships
- Symptoms: character functions isolated, relational dynamics missing
- Ask: what does this character mean TO others?
- Intervention: relationship web + foil mapping

### EX6: No Hierarchy
- Symptoms: everything marked essential
- Ask: what is primary vs reinforcing vs optional?
- Intervention: hierarchy + impact scoring

### EX7: Extraction Complete
- Symptoms: emotional core, hierarchy, structure/style split, links complete
- Intervention: validation + handoff to synthesis

## Six Extraction Axes

For each element:
1. **Form** — what it is
2. **Structural function** — what plot mechanics it enables
3. **Character function** — what arcs/transforms it enables
4. **Emotional function** — what audience feeling it produces
5. **Thematic function** — what idea/question it explores
6. **Relational function** — what dynamics/tensions it creates

## Tone and Voice Extraction

Track tonal signature separately from structure:
- Sincerity level
- Humor mode
- Emotional expression mode
- Dialogue density
- Conflict style

Also extract:
- Character voice distinctiveness
- Dialogue function mix (information/relationship/conflict/subtext)
- Tonal shifts and triggers

## Depth Levels

- **quick**: core functions + genre + key elements
- **standard**: full six-axis + relationships + structure
- **detailed**: beat-level + tonal variation + dialogue patterns

## Diagnostic Process

1. Identify source + goal
2. Map emotional experience
3. List major elements
4. Extract all six axes per element
5. Split structural vs stylistic
6. Build hierarchy
7. Identify trope clusters
8. Validate completeness
9. Generate DNA document

## Key Questions

### Emotional Core
- What do lovers of this work love most?
- What genre promise is delivered?
- Where are emotional peaks/valleys?

### Character Function
- Lie / want / need?
- Transformation?
- Contrast pairings and what they reveal?

### Structural Function
- What breaks if removed?
- What information handoff happens?
- Cause or effect?

### Adaptability
- Universal vs setting-specific?
- Could different form serve same function?
- Essential vs characteristic?

## Anti-Patterns

- **Plot Summary Trap**: event recap ≠ function extraction
- **Favorite Element Bias**: uneven depth by preference
- **Everything Is Essential**: no adaptation room
- **Form-As-Function**: element name repeated as “function”

## Tooling Reference

### `scripts/extract-functions.ts`
Use for six-axis extraction sessions, depth control, per-element extraction, validation.

### `scripts/emotional-beat-map.ts`
Use for emotional timeline peaks/valleys + genre expectation comparison.

### `scripts/structural-stylistic.ts`
Use for structural vs stylistic classification.

## Output Model

### Suggested structure

```text
{project}/dna-library/
├── extractions/
├── clusters/
└── syntheses/
```

### Work extraction schema (core fields)
- `_meta` (type/source/depth/date/clusters)
- `emotional_core`
- `tone`
- `characters`
- `plot_structures`
- `relationships`
- `structural_requirements`
- `adaptable_without_breaking`
- `links`

### Cluster schema (core fields)
- `_meta`
- `core_functions`
- `required_elements`
- `variance_axes`
- `source_works`
- `links`

## Operating Boundaries

Do not:
- accept plot summary as extraction
- skip to synthesis before EX7 readiness
- treat all elements as equal
- confuse form with function
- skip emotional core

## Output Persistence

Persist to files:
- extraction JSONs
- cluster docs
- beat maps / validation outputs

Keep in conversation:
- iterative probing questions
- state diagnosis discussion
- rationale dialogue

## Integration Graph

### Inbound
- `story-sense` → evaluation-ready source analysis
- `genre-conventions` → genre grounding for emotional core

### Outbound
- EX3 gaps → `genre-conventions`
- EX5 relationship gaps → `character-arc`
- EX7 complete → `adaptation-synthesis`

### Complementary
- `cliche-transcendence`
- `genre-conventions`
- `character-arc`
- `story-sense`
