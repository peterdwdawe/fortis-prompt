using System.Collections;
using System.Collections.Generic;

//TODO: make everything readable from json or similar? would love to use ScriptableObjects but no engine refs on server
public static class NetworkConfig
{
    static NetworkConfig()
    {
        TickInterval = 0.02f;
    }

    public static readonly float TickInterval;
    public static int TickIntervalMS => (int)(TickInterval * 1000);
    public static readonly int Port = 5000;
    public static readonly byte MaxConnectionCount = 16;
    public static uint PlayerUpdateTickCount { get; private set; } = 4;
}

public static class NetworkState
{
    //TODO: increment every tick, double check sync?
    public static float CurrentTime => CurrentTick * NetworkConfig.TickInterval;
    public static uint CurrentTick { get; private set; } = 0;

    public static void Tick()
    {
        CurrentTick++;
    }

    public static void SetCurrentTick(uint currentTick)
    {
        CurrentTick = currentTick;
    }
}

public static class PlayerConfig
{
    public static float MovementSpeed { get; private set; } = 4f;
    public static float RotationSpeed { get; private set; } = 0.25f;
    public static float Radius { get; private set; } = 0.5f;
    public static int MaxHP { get; private set; } = 100;
    public static float RespawnTime { get; private set; } = 5f;
}
public static class ProjectileConfig
{
    public static int Damage { get; private set; } = 25;
}
