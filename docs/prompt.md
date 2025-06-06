## Programming Exercise
For this exercise, we will convert a single-player game into a multiplayer one.

### Here is the behavior that we want to see:
- Whenever a client connects, the server instantiates a character and assigns it to the
new player.
- When the client disconnects, the server destroys the disconnecting player character.
- The players must be able to control their own character movement.
- The character can shoot projectiles using RPCs.
- ***Add hit and damage from projectiles and show in a health bar***
- ***Player can die and respawn***
- ***Add configurations for anything you feel should be tunable by design***
### Technical Requirements
- The server should be written in C# in a .NET console application.
- Third-party frameworks such as [LitNetLib](https://github.com/RevenantX/LiteNetLib) for the transport layer are allowed.
- The code must be written in OOP and follow the SOLID principles. Make sure to
follow the Inversion of Control (IoC) principle.
- The business logic should be separated from the presentation and shared between
client and server.
- Write a readme file containing a Roadmap plan covering the next steps.
### Nice to have:
- Report on bandwidth usage.
- Unit test on core systems.
- ***Start screen with a button to start the game***
- ***Collision***
- ***Bots with the following:***
  - ***Chase and attack with keep away distance***
  - ***On low health run away***

### What will be evaluated:
- Adherence to technical requirements.
- Code fluency and maintainability.
- System and Architecture Design.
- Decisions and thought process
- Project organization.
### What will not be evaluated:
- Art quality.
- Physics handling.
