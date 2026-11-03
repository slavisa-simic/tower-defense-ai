# Native walkthrough record — 2026-11-02

Agent-operated Unity Game View, 1920×1080, normal simulation speed. Mouse/keyboard actions used the computer-use skill. Editor-only validation redirection created an empty profile before Main Menu startup. The game's real menus, placement input, funds, enemies and summary were used.

1. New Game → Danube Border → Start. Intro appears inside ordinary Preparation. Continue advances to objective; Homicides has a temporary outline. RSD/build/placement follow.
2. Select MG from the real build bar; place freely on southern land, then northern land, then center. Total construction 450 from ordinary 500 starting RSD. Start Wave explanation highlights the actual button. No placement cell was prescribed.
3. Start actual Wave 1. Emergence/Crawler prompts occur during live movement. Real kills increase RSD. First breaches introduce casualties. Selecting a built MG exposes the real structure panel and affordable Upgrade eligibility.
4. Normal early loss at 1,000 Homicides after 01:51: 33 kills, Wave 1, 825 RSD earned, three builds. Actual summary awards 0 RP and introduces persistence. No rewind or tutorial-specific penalty.
5. Return to Main Menu. Research button is highlighted once. On a later Editor Play entry with the same profile it does not repeat. First Research visit explains costs, prerequisites and new-run application. Available RP is correctly zero and purchases remain unavailable.
6. New Game → Industrial Port. Existing basic lessons remain shown. Only the new map/chokepoint message appears. No second quarantine intro or repeated build/placement lesson.
7. PAUSE → Settings → Tutorial OFF → Back → Resume → select Wire for the first time. No tutorial appears. Ordinary selection, build availability and standard hover information remain functional. The preference is saved immediately.

Findings fixed: default grey tutorial buttons in menu-only documents; inline card timeouts moving map/summary buttons under the pointer; stale detached Research tooltips. Build wording now recommends river coverage after the original deep placement lost to bypassing creatures. The native early-loss run predates that wording improvement; no claim that the wording has been tested on an unfamiliar human.

Profile evidence: `fresh-profile-after-loss.json`, `manual-profile-final.json`. The final profile shows sixteen independently encountered lessons, preserves the completed-run flag and stores enabled=false. State-dependent lessons not encountered naturally were covered by integration tests and assisted layout scenarios. The original test profile remains separate from the resolution-audit profile and the final suite's base profile.

The capture named `fresh-early-loss.png` was saved after the timed RP card had already expired in the initial implementation; the corrected stable RP layout is covered by all three `rp-<resolution>.png` captures. Likewise, `manual-port-retry.png` may show the normal Port after its timed card; all three `port-<resolution>.png` captures retain the actual lesson. Native tool screenshots during the walkthrough showed both prompts when first presented.
