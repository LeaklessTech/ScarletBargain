# Site 6457: a Prisoner's Dilemma – Team Proposal Document

## 1. Basic Gameplay & Core Mechanics
- **Core Gameplay Loop**:  
Player toggles between four prisoners to pathfind and escape a proceduraly generated horror environment. Each prisoner must evade the monster, locate the escape point and STAY ALIVE.  The player alternates control to coordinate escapes while the non-selected characters are controled by a simple AI. In addition to the monster gaining characteristics on a single round of gameplay, the next round of gameplay is impacted by how many prisoners, and what type, were saved on the previous round.  

- **Unique Gameplay Elements**:  
  * Monster Evolution: As the monster captures prisoners it gains the strengths and the weaknesses of the prisoner that it captured.  Potential capabilities include speed, pathfinding effectiveness and environemntal manipulation around doors / room entry points.
  * Procedural Levels: The levels vary on each game run.
  * Potential idea - start out as one prisoner, and you are not able to control the other one until you find them.

- **Demo Video Notes (for presentation)**:  

  * what do we have here

## 2. Formal Elements of the Game

### a) Players & Interaction Modes
- Number of players (MVP -> single / future potential multiplayer)
- Interaction style (MVP -> toggle control, future potential co-op possibilities)

### b) Objectives
- Primary objective(s): escape with at least one prisoner
- Secondary / stretch objectives: escape with more than one prisoner
- Perfect win: escape with all prsoners

### c) Procedures
- **Starting action**: Player spawns in a room alone with a note.   
- **Progression of action**: Player explores the generated environment and finds other prisoners who can aid in their escape. Audio queues inform the prisoner that monster may be close by.  
- **Special actions**: 
  * Each prisoner has a unique skill that can aid escape: (1) speed, (2) environmental manipulation (doors), and (3) better hearing.
  * If the monster catches one of the prisoners outside of the view of the player, they are made aware through sound effects.
  * When the monster catches a prisoner they are delayed for anywhere from 5 to 15 seconds as they "train" on the skills of the prisoner.  For example, if the monster catches the fast prisoner the monster then becomes faster.
- **Resolving action**: Between one and four prisoners find the exit point of the procedurally generated level before all of them are captured by the monster.

### d) Rules
- Rules defining objects & concepts (characters, monster, environment):  Can move character, can jummp, can toggle between characters, can interact with objects (doors)
- Rules restricting actions: Can tell characters to stay in place.  Can have characters follow the one that you control.
- Rules determining effects:  Non controlled prisoner characters mainly stay where they are, but depending on attributes may effectively hide from the monster. 

### e) Resources
- Prisoners are a resource from the perspective of the monster, who can power up / down by absorbing the prisoners skills.
- Initial plan is to have only one exit point (may update to scale difficulty)
- Prisoners are also a resource from the perspective of the player, who gets a better score, and more team skills by adding prisoners to the group.

### f) Conflict
- Opponent: the monster
- Obstacles: doors / room access points
- Player dilemma: when the exit is found prior to finding all prisoners, to exit, or keep searching.  To 'sacrafice' a prisoner by telling them to stay put. 

### g) Boundaries
- Map boundaries (horror setting such as prison/hospital, procedural generation)  
- Access points to rooms may be walls that can be broken by certain prisoners types, or doors that can be unlocked by other prisoner types. 

### h) Outcome
- Win conditions: highest win is all four escape
- Partial win conditions: some escape
- Loss condition(s): all are captured by the monster


## 3. Narrative / Story Components
- **Theme / Setting**: horror atmosphere such as hospital or prison
- **Backstory**: Prisoners are in an experiment to train AI, monster can learn from capturing escaping prisoners
- **Atmosphere Goals**: the player should feel an excitement and fear from the sound and environment and a genuine sense of having to balance survival with making decisions for the good of the group


## 4. Work Plan

Team divides game into key functional areas and builds trello ticket boards around the functions.

- **Functional Breakout**: 
 * AI Systems
 * Procedual Generation
 * Art and Audio
 * Gameplay control
 * Pathfinding

 - **Key Algorithms / Libraries**:
 * Kruskal's algorithm for minimum spanning tree to connect rooms
 * A-Star for pathfinding 
 * Behavoir Tree data structure

