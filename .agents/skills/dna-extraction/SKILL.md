---
name: dna-extraction
description: Extract the functional DNA from existing works (TV, film, books, plays). Use when adapting a source work, when analyzing what makes something work, when creating trope maps for reuse, or when you need to separate structural necessity from stylistic choice.
license: MIT
metadata:
  blacktower-pack: creative
  blacktower-kind: workflow
  blacktower-tier: standard
  blacktower-capabilities: worldbuilding-fiction,dna,extraction
  author: jwynia
  version: "1.0"
  type: utility
  mode: evaluative
  domain: fiction
---

# DNA Extraction

Extract functional DNA from existing works so adaptation keeps what matters.

## Purpose

Find load-bearing functions under surface form.

Rule: **function over form**.

## When to Use

- Adapting an existing work to new setting/medium
- Building trope maps/clusters for reuse
- Diagnosing why a source work works emotionally
- Separating structural necessity from style

## When Not to Use

- You only need plot summary
- You already have complete extraction and are ready to synthesize (handoff to `adaptation-synthesis`)

## Workflow (Fast)

1. **Diagnose extraction state** (EX0–EX7)
   - EX0 no extraction
   - EX1 surface reading
   - EX2 single-axis extraction
   - EX3 missing emotional core
   - EX4 style/structure conflation
   - EX5 missing relationship functions
   - EX6 no hierarchy
   - EX7 complete
2. **Map emotional core first**
   - primary/secondary genre
   - key emotional beats
3. **Run six-axis extraction for each major element**
   - form, structural, character, emotional, thematic, relational
4. **Classify structural vs stylistic**
5. **Build hierarchy**
   - primary (breaks story if removed)
   - reinforcing (weakens story if removed)
   - optional flavor
6. **Validate readiness for synthesis**

## Output Checklist

- [ ] Source metadata captured (work/author/medium/depth/date)
- [ ] Emotional core defined (genre + experience + beats)
- [ ] Major elements extracted across all six axes
- [ ] Relationship functions documented
- [ ] Structural vs stylistic split complete
- [ ] Non-negotiables vs adaptable elements listed
- [ ] Cluster links recorded

## Guardrails

- Don’t accept plot-summary-as-extraction
- Don’t mark everything essential
- Don’t skip emotional core
- Don’t confuse element names with functions

## Resources

- Full guide: `references/full-guide.md`
- Scripts:
  - `scripts/extract-functions.ts`
  - `scripts/emotional-beat-map.ts`
  - `scripts/structural-stylistic.ts`
- Data:
  - `data/function-categories.json`
  - `data/extraction-templates.json`

## Integration / Handoffs

- Use `genre-conventions` for emotional-core genre diagnosis gaps
- Use `character-arc` for relationship/lie-want-need deepening
- Handoff to `adaptation-synthesis` only after EX7
