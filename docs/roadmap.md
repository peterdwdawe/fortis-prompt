## Roadmap
Currently, the project implements most of the requirements laid out in the [prompt](docs/prompt.md). It's still lacking in a few areas:
- Code coverage is very low - unit tests currently only cover network message serialization/deserialization.
- Player-player/player-world/projectile-world collision is not yet implemented.
- Bots are not yet implemented.

### Unit Test Improvements
I'd like to run unit tests simulating games, including connection/disconnection and all the different actions that players can take.\
To do that, I'll need to change the project architecture slightly - pull the server console app (Program.cs) into its own project and build the remaining "Server" project as a library. Then I could reference the server project in the "EditModeTests" project, and simulate both server and client within the same tests. *(Yes, this could all be done outside of Unity Test Framework, but I like the idea of being able to test all the code in one place, including UnityEngine-only elements.)*

### Collision
In the project's current state, everything exists on one plane, players are modeled as circles, and projectiles are modeled as points. In these constraints, simple collision implementation won't be too bad: each update, we can just calculate movements, and then any characters that overlap can each be pushed away from each other by half of the penetration distance. This is very simplified, and will probably make the controls feel a bit sticky, especially when more than two players are colliding. It could be simulated on the server only, but would likely produce better results if also simulated on clients.\
If the map or chracters become any more complex, though, I'd recommend either running the server via Unity's dedicated server option to take advantage of their physics system, or using an external physics engine. At that point, any system that I could build quickly is not going to be as well optimized as those solutions.

### Bots
Bots could be implemented via a new type of IInputListener, simulated on the server and networked to clients the same way that players are. The biggest task would likely be putting together the bots' decision-making logic.

### Other Improvements
- **Expanded Server Authority:** Right now, the project is really only server authoritative in name - it wouldn't complain or correct if a player moved faster than their moveSpeed, or tried to spawn a projectile with the wrong ID, etc. It would be nice to add these details as a sort of proof-of-concept for anti-cheat measures.
- **Lag Compensation / Synchronization:** It would also be nice to take network latency into account when reacting to messages, e.g. simulating projectile movement and spawning them slightly forward to compensate for the networking time.