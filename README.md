# Fortis Multiplayer Prompt
  
## To-Do List
- Readme Roadmap
  - [ ] Complete

- Code Quality
  - [X] Clean up unused functions
  - [ ] Search TODO's for anything I forgot to return to
  - [ ] Merge common client/server functions
  - [ ] Abstract into interfaces as much as possible, reduce references to concrete classes
    - References that should probably be fixed:
      - Client/Server GameManagers
      - Client/Server NetworkManagers
      - Client/Server MessageHandlers
  - [ ] Refactor to use composition as much as possible vs inheritence
    - Classes that should probably be fixed:
      - ServerPlayer
      - ServerProjectile
      - LocalInputListener
  - [ ] Review folder structure (including config save locations) and mirror it in namespaces
  - [ ] Sort functions, properties, events, etc. consistently across classes to improve readability
  - [ ] Add/Improve Comments

- Features
  - [ ] Expand on unit tests
  - [ ] Player collision, if time permits
  - [ ] Bots, if time permits
  - [ ] Improve configs 
    - [ ] If I have time, an editor window in Unity would be great
    - [ ] Maybe store configs on server side only, and send them over to client when connecting to ensure sync?


## Next Steps
- Server Authority: let server decide if movement is valid - i.e. character delta position not too large. Force correction if not
- Improve interpolation when receiving position updates
- Improve server ticks: right now the time between ticks on the server is very unreliable.
  - Currently using Thread.Sleep and 20ms ticks, actual delay is closer to 30 ms.
- Improve configs:
  - Consider storing configs on server, and sending that data to clients when they connect? Right now two copies are stored, one on client side and one on server side. Fine for testing but a nightmare if shipped.
  - A better config editor would be great,  e.g. a ScriptableObject or Unity Editor window that can export to json on build for the server to use 
- Other Ideas?

## Checklist
### Required:

**Server:**
- [X] Spawn/Despawn Players on Connect/Disconnect
- [ ] Projectiles
  - [X] Registration/instantiation on request
  - [X] Hit detection
    -[X] Player health reduction
    -[X] Projectile destruction
  - [ ] Use RPC for above features
- [X] Update and forward character controls and transform data when received
- [X] Detect character death, despawn
- [X] Respawn characters after death
- [X] HP Bars
      
**Client:**
- [X] Spawn/Despawn Local Player on Connect/Disconnect
- [X] Spawn/Despawn Networked Players on Request
- [X] Send local controls + position/rotation on tick
- [X] Update Networked character controls + position/rotation on request
- [ ] Projectiles
  - [X] Request projectile creation from server
  - [X] Instantiation/destruction on request
  - [ ] Use RPC for above features
        
**Shared:**
- [X] Movement + Prediction
- [X] Projectile Instantiation, Simulation & Destruction (Not Including Hit detection)
- [X] Character Spawn/Despawn Logic
- [X] Easy-To-Edit Configurations
  - [X] Player Attributes - Speed, Size, etc.
  - [X] Projectile Attributes - Speed, Damage, etc.
  - [X] Game Configuration - Respawn Time, Max Player Count,
  - [X] Network Configuration - Tick Interval, Player Update Frequency

### Nice To Have:
- [X] Bandwidth Usage Reporting
- [X] Unit Tests _minimal_
  - [X] Message Serialization/Deserialization
- [X] Start Screen
- [ ] Collision
- [ ] Bots
  - [ ] Chase player
  - [ ] Attack w/ keep away distance
  - [ ] Run away at low health