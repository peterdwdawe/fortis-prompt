# Fortis Multiplayer Prompt

## To-Do
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

### Nice To Have:
- [X] Bandwidth Usage Reporting
- [ ] Unit Tests
- [X] Start Screen
- [ ] Collision
- [ ] Bots
  - [ ] Chase player
  - [ ] Attack w/ keep away distance
  - [ ] Run away at low health

## Next Steps
- Server Authority:
  - Let server decide if movement is valid - i.e. character delta position not too large. Force correction if not
- Other Ideas?
