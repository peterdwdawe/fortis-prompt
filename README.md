# To-Do 
**Before Submitting:**
- Upload client and server builds as releases, reference in readme
- Add/Improve Comments
**Extras:**
- Improve unit tests
- Implement Collisions
- Implement Bots

# Fortis Multiplayer Prompt
My submission for the given [**Multiplayer Programming Prompt**](docs/prompt.md).
## Roadmap
The roadmap can be found [**here**](docs/roadmap.md).

## Get Started
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
 - [Unity UI 1.0.0](https://docs.unity3d.com/Packages/com.unity.ugui@1.0) & [TextMeshPro 3.0.9](https://docs.unity3d.com/Packages/com.unity.textmeshpro@3.0) (plus dependencies) for main menu and health bar GUI.

**Shared - Server & Client:**
- [LiteNetLib 1.3.1](https://github.com/RevenantX/LiteNetLib/) for the networking transport layer.
- [System.Text.Json 9.0.5](https://learn.microsoft.com/en-ca/dotnet/api/system.text.json?view=net-8.0) for config file serialization.

### Issues and Limitations
- I've only tested the server locally, using "localhost" as the network address. I don't see why there would be any problems when deployed to a server, but it hasn't been tested at this point.