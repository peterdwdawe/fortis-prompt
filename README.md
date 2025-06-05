# Fortis Multiplayer Prompt

## Checklist
### Required:

**Server:**
- [X] Spawn/Despawn Players on Connect/Disconnect
- [ ] Projectiles
  - [ ] Instantiation & registration via rpc
  - [X] Hit detection: health reduction and notification
- [X] Update and forward character controls and transform data when received
- [X] Detect character death, despawn
- [X] Respawn characters after death
- [X] HP Bars
      
**Client:**
- [X] Spawn/Despawn Local Player on Connect/Disconnect
- [X] Spawn/Despawn Networked Players on Request
- [X] Send local controls + position/rotation on tick
- [X] Update Networked character controls + position/rotation on request
- [ ] RPC Projectiles
  - [ ] Send RPC on projectile fire
  - [X] Instantiate & Simulate projectiles on request
        
**Shared:**
- [X] Movement + Prediction
- [X] Projectile Simulation (Not Including Hit detection)
- [X] Character Spawn/Despawn Logic
- [ ] Easy-To-Edit Configurations
  - [ ] Player Attributes - Speed, Size, etc.
  - [ ] Projectile Attributes - Speed, Damage, etc.
  - [ ] Game Configuration (Should this be sent to player on connection?) - Respawn Time, Max Player Count,
  - [ ] Network Configuration (Should this be sent to player on connection?) - Tick Interval, Player Update Frequency
  - [ ] Connection Configuration? - Network Address, Port

### Nice To Have:
- [X] Bandwidth Usage Reporting _currently only visible from client_
- [X] Unit Tests _minimal_
  - [X] Message Serialization/Deserialization
- [X] Start Screen
- [ ] Collision
- [ ] Bots
  - [ ] Chase player
  - [ ] Attack w/ keep away distance
  - [ ] Run away at low health


## Next Steps
- Server Authority: let server decide if movement is valid - i.e. character delta position not too large. Force correction if not
- Smooth interpolation when receiving position updates
- Other Ideas?

  
## Code Quality To-Do List
- [ ] Clean up unused functions
- [ ] Merge common client/server functions
- [ ] Abstract into interfaces as much as possible, reduce references to concrete classes
- [ ] Improve config - maybe just create a basic JSON/XML file for all settings?
- [ ] Refactor to use composition as much as possible vs inheritence
- [ ] Sort functions, properties, events, etc. consistently across classes to improve readability
