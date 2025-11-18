# Design Questions

## Question 1
**Description**
Does the hiding mechanic effectively support our desired player experience of frantically searching for an escape from our AI monster?

**Justification**
The hiding mechanic is the primary way for players to interact with the game world. Therefore it is important we test whether or not the mechanic supports the intended experience.

## Question 2
**Description**
Do the actions of the AI monster support the experience of making the player feel fear and helplessness?

**Justification**
The AI monster is the player's only adversary (besides navigating the maze) preventing the player from escaping. Therefore it is important that the actions the monster takes makes the player feel fearful.

# Question 3
**Description**
Does finding and saving prisoners make the player feel like they are accomplishing an important task?

**Justification**
Saving prisoners from the simulation is a secondary goal of the game, we are attempting to frame it as an important task. Therefore it is important that we test whether or not we are successful in this framing.

# Question 4
**Description**
Is finding the ladder to escape the digital maze support the intended experience of narrowly escaping from a dangerous situation, providing a sense of relief?

**Justification**
The ladder escape is an integral aspect of our game. We want our players to see the ladder as a representation of escape from the current situation, as a relief from the horror maze they are in. Therefore we need to test if the ladder is having the intended effect on the experience of the player.

# Question 5
**Description**
Does the monster AI support the desired player experience of outwitting and avoiding a dangerous entity, does it make the player feel like they are in a game of cat and mouse?

**Justification**
We want the player to feel as if they are making informed decisions and that those decisions have an impact on the game world, specifically the monster AI behavior. Therefore we must test if the player feels like this is the case.

# Approach to Testing Design Questions
To test the design questions our team created two sets of questions to ask our players in a post-playtest questionaire. For the gameplay portion of the playtest we decided that a playtest time of 8 minutes was sufficient to gather insights into our design questions.

Our team decided that no measuring or logging of the game enviornment was necessary to test any of our design questions. Our team ultimately decided on one set of quantitative questions and another set of qualitative questions to efficiently test our design questions. The quantitative questions were designed to give our team concrete results that we could align to our design questions. The qualitative questions were designed to give playtesters an opportunity to address any thoughts they had that were not addressed in the quantitative section.

# Playtest Methods
Our method for playtesting was as follows:
- Gather demographic information on the playtester.
- Playtester plays the game for 8 minutes.
- Gather answers to the post-playtest questionaire.

We decided to gather demographic information before beginning the gameplay portion of the playtest so that we could smooth the playtester into the experience. After gathering the initial information we start the playtest proper, which involves the conductor of the playtest starting a timer for eight minutes and instructing the player to begin.

During the gameplay portion of our playtest we do not instruct the player in what/how to do things, unless they do not know the basics of controls (not knowing that WASD moves the player-character in the game world). After the eight minute timer has expired the conductor lets the playtester know and tells them that they may contiune playing if they so choose. We allow the playtester to make this choice because it could be an important indicator that they think the game is interesting.

After the playtester has excited the game the condutor of the playtest will begin asking them questions. There are two sets of questions that the playtesters are asked. One set of quantitative questions which are scored on a Likert scale (1-5, 1 being Strongly Disagree, 5 being Strongly Agree). The second set of questions are qualitative, the player is asked about an aspect of the game and are given free reign to answer how they please.

# Results Summary/Analysis
**Playtesters:** 10 (6 Male / 4 Female)  
**Age range:** 22–65 (mean 32.4)  
**Escaped:** 8/10 (80%)

## Quantitative Scores (1–5 Likert)

| Question                                               | Average | Notes                                      |
|--------------------------------------------------------|---------|--------------------------------------------|
| Q1.4 – Clarity of objective                            | **4.3** | Highest score – everyone instantly understood |
| Q1.6 – Would play again                                | **3.8** |                                            |
| Q1.8 – Enjoyed playing                                 | **3.8** |                                            |
| Q1.3 – Game was challenging                            | 3.1     |                                            |
| Q1.1 – Felt fearful of the monster                     | 3.0     |                                            |
| Q1.5 – Controls felt smooth                            | 3.0     |                                            |
| Q1.7 – Decisions had impact                            | 2.8     |                                            |
| Q1.2 – Felt it was important to save prisoners         | 2.4     | Second-lowest                              |
| Q1.9 – Felt immersed                                   | 2.25    | Lowest                                     |

## Key Insights

- **Core loop is instantly understood and liked** – high clarity (4.3), strong enjoyment & replay intent (3.8).
- **Emotional stakes are weak** – saving prisoners felt unimportant (2.4), low immersion (2.25), decisions impact low (2.8).
- **Controls rating improved** rated at 3.0 on average, but multiple players reported issues.
- The three testers who **failed to escape** (1 mid-age casual gamer + 2 older lower skill Mac users) gave **higher-than-average scores** on fear (3.3 vs 2.9), challenge (4.0 vs 2.7), & controls (3.7 vs 2.7) – **while matching on enjoyment/replay** → chase is fun & engaging when it catches you, but skilled players evade too easily.

## Conclusions from open-ended questions

### Most Frequent Issues / Requests
1. **Camera / mouse control bad or broken** – 7 mentions  
2. **Technical bugs & polish issues** (crouch, sprint, UI, volume, cursor lock, multi-monitor, accidental quit, doors/walls, headbob) – 8 mentions  
3. **Want more interactables** (items to pick up/use, safes, power-ups, shields, distractions) – 5 mentions  
4. **Prisoner interaction feels empty/broken** (no feedback, laggy following, no real rescue sense/outcomes) – 4 mentions  
5. **Monster not scary/threatening** (easy to evade, dumb AI, quiet footsteps, predictable) – 4 mentions  
6. **World feels bare/empty/prototype-y** (lack of details/content) – 4 mentions  
7. **More rooms/variety/content depth/enemies/goals** – 4 mentions  
8. **Audio polish / better soundtrack/volume** – 3 mentions  

### Positive Themes
- Objective **instantly clear to all 10 testers**
- Several praised retro/spooky art style & backrooms-like premise (mixed: some saw as fresh take, one as low inspiration)
- Genuine urgency/excitement/chase fun when monster engaged
- Difficulty mostly "about right"; one failure called "too hard"

## Key Takeaways

### High Priority Changes
1. Fix camera/mouse (sensitivity, cursor lock, multi-monitor, stability)
2. Comprehensive bug squash (crouch/sprint/UI/quit key/doors/walls/headbob/controls consistency)
3. Polish prisoner rescue (clear feedback/join cues, counter, voice lines, smoother following, save outcomes/rewards)

## Aspects that meet design requirements

## Aspects that do not meet design requirements

# Action Items / Future Work
The behavior where pressing “X” instantly closes the game should be removed or remapped and replaced with an explicit quit flow. The pause overlay should be the only path to quit, with a required confirmation step. This change reduces frustraion with the controls, addressing feedback from players who accidentally closed the game mid-run.

Prisoner interactions should become visually clear and satisfying, with feedback when a rescue occurs. When the player frees a prisoner, the game should trigger a distinct sound, a brief visual highlight, and/or update a visible HUD element such as “Prisoners Following: 2/5.” This removes ambiguity about whether prisoners are actually following and makes rescuing them feel like a concrete, rewarding action.

A simple, persistent objectives and controls screen should be introduced so players can recheck what they are supposed to do. Tutorial messages should remain visible long enough to be read comfortably (perhaps pausing the start of the game), and afterward the player should be able to press a key to bring up a compact summary (for example, “Goal: Rescue prisoners and reach the ladder”) plus a short control reference. This can be integrated into the existing pause overlay or implemented as a small new overlay.

The level layout and environment should be enriched so the space feels less empty and offers more meaningful choices. Adding more hiding spaces and props that suggest a functioning facility will address the “bare” or “early in development” impressions. Enhancing the algorithm to place these elements so players choose between routes, hiding spots, and riskier options will make navigation and moment-to-moment decisions more engaging while they rescue prisoners and evade the monster.
