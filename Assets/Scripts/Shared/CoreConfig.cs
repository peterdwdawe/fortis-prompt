using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Shared.Configuration
{
    public class NetworkConfig
    {

        public float TickInterval { get; private set; } = 0.02f;
        public int TickIntervalMS => (int)(TickInterval * 1000);

        public uint PlayerUpdateTickCount { get; private set; } = 4;
    }

    public class NetworkState
    {
        public readonly NetworkConfig config;

        public NetworkState(NetworkConfig config)
        {
            this.config = config;
        }

        //TODO: increment every tick, double check sync?
        public float CurrentTime => CurrentTick * config.TickInterval;
        public uint CurrentTick { get; private set; } = 0;

        public void Tick()
        {
            CurrentTick++;
        }

        public void SetCurrentTick(uint currentTick)
        {
            CurrentTick = currentTick;
        }
    }

    public class PlayerConfig
    {
        public float MovementSpeed { get; private set; } = 4f;
        public float RotationSpeed { get; private set; } = 0.25f;
        public float Radius { get; private set; } = 0.5f;
        public int MaxHP { get; private set; } = 100;
        public float RespawnTime { get; private set; } = 5f;
    }
    public class ProjectileConfig
    {
        public int Damage { get; private set; } = 25;
        public float MovementSpeed { get; private set; } = 8f;
        public float Duration { get; private set; } = 4f;
    }
    public class GameConfig
    {
        public byte MaxPlayers { get; private set; } = 16;
    }
}

//TODO: make everything readable from json or similar? would love to use ScriptableObjects but no engine refs on server
