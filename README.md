# TODO: upload client and server builds!!!
# Fortis Multiplayer Prompt
My submission for the given [Multiplayer Programming Prompt](docs/prompt.md).

## Get Started
# TODO: add release links where needed
To try out the project, download the latest client and server builds from "Releases", or clone the repo and build from there. 
On the client build, just enter the server address and port you want to connect to, and hit connect. Your local character should be orange, and any networked player characters should be purple.
### Local Setup
Running the server and multiple instances of the client all on one machine is the only way I've tested so far, and is straightforward. On the client side, the address defaults to localhost:5000, and the default port in ServerConfig.json is also 5000, so it should work out of the box.
### Configuration
Both the client and server builds use .json files to store configurations. Packaged with the server are ServerConfig.json and GameConfig.json. All the values in GameConfig are sent to clients as they connect and available during gameplay. ClientConfig.json holds any client-only configuration data. The settings are as follows:

**ServerConfig.json:**
| Property | Type | Description | Default |
| --- | --- | --- | --- |
| ServerTickInterval | float | Time between server updates, in seconds | 0.015 |
| Port | int | Port number to run on | 5000 |
| NetworkKey | string | Must match ClientConfig.NetworkKey to connect successfully | fortis_connect_test |
| RpcTimeout | float | Duration, in seconds, to wait for an rpc response before returning a failure | 1.0 |

**ClientConfig.json:**
| Property | Type | Description | Default |
| --- | --- | --- | --- |
| PlayerUpdateInterval | float | Time between transform & controls updates, in seconds | 0.05 |
| NetworkKey | string | Must match ServerConfig.NetworkKey to connect successfully | fortis_connect_test |
| RpcTimeout | float | Duration, in seconds, to wait for an rpc response before returning a failure | 1.0 |

**GameConfig.json:**
| Property | Type | Description | Default |
| --- | --- | --- | --- |
| MaxPlayerCount | byte | Max # of clients allowed to connect | 16 |
| PlayerMoveSpeed | float | Player speed in m/s | 4.0 |
| PlayerRotateSpeed | float | Player rotation speed in rotations/second | 2.0 |
| PlayerRadius | float | Player capsule radius in metres | 0.5 |
| PlayerMaxHP | int | Player max HP | 100 |
| PlayerRespawnTime | float | Player respawn time in seconds | 5.0 |
| ProjectileDamage | int | Projectile damage dealt on hit | 25 |
| ProjectileSpeed | float | Projectile speed in m/s | 8.0 |
| ProjectileLifetime | float | Projectile lifetime in seconds | 4.0 |

## Info
The server build targets .NET 8.0 and the client was built using Unity 2022.3.62f1.

### Libraries Used   
**Client Only:**
 - [Unity Test Framework 1.4.3](https://docs.unity3d.com/Packages/com.unity.test-framework@1.4) (plus dependencies) for unit testing.
 - [Unity UI 1.0.0](https://docs.unity3d.com/Packages/com.unity.ugui@1.0) & [TextMeshPro 3.0.9](https://docs.unity3d.com/Packages/com.unity.textmeshpro@3.0) (plus dependencies) for GUI.

**Server & Client:**
- [LiteNetLib 1.3.1](https://github.com/RevenantX/LiteNetLib/) for the networking transport layer.
- [System.Text.Json 9.0.5](https://learn.microsoft.com/en-ca/dotnet/api/system.text.json?view=net-8.0) for config file serialization.

### Issues and Limitations
- I've only tested the server locally, using "localhost" as the network address. I don't see why there would be any problems when deployed to a server, but haven't tested it.
- Code coverage is very low - currently only testing network message serialization/deserialization.
- Player-player/player-world/projectile-world collision is not yet implemented.
- Bots are not yet implemented.

## Roadmap

## To-Do List
- [ ] Readme/Roadmap

**Code Quality**
- [X] Clean up unused functions
- [X] Search TODO's for anything I forgot to return to
- [X] Merge common client/server functions
- [X] Abstract into interfaces as much as possible, reduce references to concrete classes
- [X] Refactor to use composition as much as possible vs inheritence
- [ ] Review folder structure (including config save locations) and mirror it in namespaces
- [ ] Sort functions, properties, events, etc. consistently across project to improve readability
- [ ] Add/Improve Comments

**Features**
- [ ] Expand on unit tests
- [ ] Player collision, if time permits
- [ ] Bots, if time permits
- [X] Improve configs 
  - [X] Maybe store configs on server side only, and send them over to client when connecting to ensure sync?


## Next Steps
- Server Authority: let server decide if movement is valid - i.e. character delta position not too large. Force correction if not
- Improve interpolation when receiving position updates
- Lag compensation / tick synchronization for players and projectiles
- Implement collisions
- Implement bots
- Improve code coverage
- Other Ideas?

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
