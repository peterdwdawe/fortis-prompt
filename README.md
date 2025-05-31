# Fortis Multiplayer Prompt

## To-Do
### Required:

**Server:**
- [ ] Spawn/Despawn Players on Connect/Disconnect
- [ ] Projectiles
  - [ ] Instantiation & registration via rpc
  - [ ] Hit detection: health reduction and notification
- [ ] Update and forward character controls and transform data when received
- [ ] Detect character death, despawn
- [ ] Respawn characters after death
      
**Client:**
- [ ] Spawn/Despawn Local Player on Connect/Disconnect
- [ ] Spawn/Despawn Networked Players on Request
- [ ] Send local controls + position/rotation on tick
- [ ] Update Networked character controls + position/rotation on request
- [ ] RPC Projectiles
  - [ ] Send RPC on projectile fire
  - [ ] Instantiate & Simulate projectiles on request
        
**Shared:**
- [ ] Movement + Prediction
- [ ] Projectile Simulation (Not Including Hit detection)
- [ ] Character Spawn/Despawn Logic

### Nice To Have:
- [ ] Bandwidth Usage Reporting
- [ ] Unit Tests
- [ ] Start Screen
- [ ] Collision
- [ ] Bots
  - [ ] Chase player
  - [ ] Attack w/ keep away distance
  - [ ] Run away at low health

## Next Steps
- Server Authority:
  - Let server decide if movement is valid - i.e. character delta position not too large. Force correction if not
- Other Ideas?
