---
title: Ren private identity and consent
type: documentation
status: draft
visibility: private
---

# Ren private identity and consent

Ren's conversation is available only at the private archive alcove. Approach
Ren, use the shared `E` focus prompt, choose a question with the arrow keys,
and press `Enter`. `Escape` closes the panel and leaves every lead available.

After the current Ada call is recorded, the player may ask the private identity
question. Ren's authored C01 response confirms the former name, describes the
earlier unwanted relay, declines immediate contact, and permits a refusal to be
relayed. The response records E04 and refusal authorization. The roster
comparison E03 is not required to ask.

Later, after both shared-repair accounts form E06 and the private route is
verified, the player may offer those assurances. Ren's resulting agreement is a
separate explicit authorization. Acquiring a repair account or checking the
route never changes consent. Repeating a question reuses its stable event and
cannot replace a later agreement with a refusal.

```sh
python3 scripts/run_unity_tests.py --platform playmode --filter CommonHouse.Tests.RenConsentTests
```

The focused tests cover the private spatial target, canonical C01 response,
E01-only identity eligibility, cancellation, explicit later agreement, and
idempotent repeated questions. They are scripted interaction evidence, not a
human playtest or final dialogue polish.
