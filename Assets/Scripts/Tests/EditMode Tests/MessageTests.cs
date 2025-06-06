using LiteNetLib.Utils;
using NUnit.Framework;
using Shared.Networking;
using Shared.Networking.Messages;
using System.Collections.Generic;

public class MessageTests
{
    [Test]
    public void MessageSerialization()
    {
        var writer = new NetDataWriter();

        var reader = new NetDataReader(writer);

        System.Numerics.Vector2 v2 = new System.Numerics.Vector2(1f, 2f);
        System.Numerics.Vector3 v3 = new System.Numerics.Vector3(1f, 2f, 3f);
        System.Numerics.Vector4 v4 = new System.Numerics.Vector4(1f, 2f, 3f, 4f);
        System.Numerics.Quaternion q = new System.Numerics.Quaternion(1f, 2f, 3f, 4f);

        List<INetworkMessage> sentMessages = new List<INetworkMessage>();

        var custom = new CustomMessage
            (0
            ,"testMessage");

        var health = new PlayerHPUpdateMessage
            (1,
            67);

        var death = new PlayerDeathMessage
            (2);

        var dereg = new PlayerDeregistrationMessage
            (3);

        var reg = new PlayerRegistrationMessage
            (4, false);

        var spawn = new PlayerSpawnMessage
            (5,
            new System.Numerics.Vector3(4f, 4.5f, 5f),
            new System.Numerics.Quaternion(6f, 7f, 8f, 9f));

        var update = new PlayerUpdateMessage
            (6,
            new System.Numerics.Vector2(-1f, 3f),
            new System.Numerics.Vector3(9f, 7f, 5f),
            new System.Numerics.Quaternion(-1f, 2f, -3f, 4f));

        var projDespawn = new ProjectileDespawnMessage
            (7);

        var projSpawn = new ProjectileSpawnMessage
            (8,
            9,
            new System.Numerics.Vector3(2f, 7.3f, -8.1f),
            new System.Numerics.Vector3(1.2f, -6.1f, 5.8f));

        var projRequest = new RequestProjectileSpawnMessage
            (10,
            new System.Numerics.Vector3(9.4f, 8.5f, -1.6f),
            new System.Numerics.Vector3(9.4f, 5.6f, -7f));

        TestSerialization(writer, reader, v2, NetworkingUtils.GetVector2, NetworkingUtils.Put);
        TestSerialization(writer, reader, v3, NetworkingUtils.GetVector3, NetworkingUtils.Put);
        TestSerialization(writer, reader, v4, NetworkingUtils.GetVector4, NetworkingUtils.Put);
        TestSerialization(writer, reader, q, NetworkingUtils.GetQuaternion, NetworkingUtils.Put);


        TestSerialization(writer, reader, custom);

        TestSerialization(writer, reader, health);
        TestSerialization(writer, reader, death);
        TestSerialization(writer, reader, dereg);
        TestSerialization(writer, reader, reg);
        TestSerialization(writer, reader, spawn);
        TestSerialization(writer, reader, update);

        TestSerialization(writer, reader, projDespawn);
        TestSerialization(writer, reader, projSpawn);
        TestSerialization(writer, reader, projRequest);
    }

    void TestSerialization<T>(NetDataWriter writer, NetDataReader reader, T testObject)
        where T : struct, INetSerializable
    {
        TestSerialization(
            writer, reader, testObject,

            (NetDataReader r) =>
            {
                reader.Get<T>(out var o);
                return o;
            },

            (NetDataWriter w, T o) =>
                w.Put<T>(o));
    }

    void TestSerialization<T>(NetDataWriter writer, NetDataReader reader, T testObject, Getter<T> Get, Putter<T> Put)
        where T : struct
    {
        writer.Reset();
        Put(writer, testObject);

        reader.SetSource(writer);
        var receivedObject = Get(reader);

        Assert.AreEqual(testObject, receivedObject);
    }
    delegate T Getter<T>(NetDataReader reader);
    delegate void Putter<T>(NetDataWriter writer, T obj);

    //// A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    //// `yield return null;` to skip a frame.
    //[UnityTest]
    //public IEnumerator MessageTestsWithEnumeratorPasses()
    //{
    //    // Use the Assert class to test conditions.
    //    // Use yield to skip a frame.
    //    yield return null;
    //}
}
