## Prompt Checklist
### Required:
**Server:**
- [X] Spawn/Despawn Players on Connect/Disconnect
- [X] Projectiles
  - [X] Registration/instantiation on request
  - [X] Hit detection
    -[X] Player health reduction
    -[X] Projectile destruction
  - [X] Use RPC for above features
- [X] Update and forward character controls and transform data when received
- [X] Detect character death, despawn
- [X] Respawn characters after death
- [X] HP Bars
      
**Client:**
- [X] Spawn/Despawn Local Player on Connect/Disconnect
- [X] Spawn/Despawn Networked Players on Request
- [X] Send local controls + position/rotation on tick
- [X] Update Networked character controls + position/rotation on request
- [X] Projectiles
  - [X] Request projectile creation from server
  - [X] Instantiation/destruction on request
  - [X] Use RPC for above features
        
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