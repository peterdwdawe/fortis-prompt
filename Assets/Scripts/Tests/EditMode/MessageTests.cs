using LiteNetLib.Utils;
using NUnit.Framework;
using Shared.Configuration;
using Shared.Networking;
using Shared.Networking.Messages;
using System;
using System.Collections.Generic;

namespace Tests.EditMode
{
    public class MessageTests
    {
        [Test]
        public void MessageSerialization()
        {
            //TODO(); // write more tests: test rpc messages serialization, send/receive, player/projectile simulation etc

            var writer = new NetDataWriter();

            var reader = new NetDataReader(writer);

            System.Numerics.Vector2 v2 = new System.Numerics.Vector2(1f, 2f);
            System.Numerics.Vector3 v3 = new System.Numerics.Vector3(1f, 2f, 3f);
            System.Numerics.Vector4 v4 = new System.Numerics.Vector4(1f, 2f, 3f, 4f);
            System.Numerics.Quaternion q = new System.Numerics.Quaternion(1f, 2f, 3f, 4f);

            List<IStandardNetworkMessage> sentMessages = new List<IStandardNetworkMessage>();

            var custom = new CustomMessage
                (0
                , "testMessage");

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
            var gameConfig = new GameConfigurationMessage
                (new GameConfigData());

            TestSerialization(writer, reader, v2, NetworkingExtensions.GetVector2, NetworkingExtensions.Put);
            TestSerialization(writer, reader, v3, NetworkingExtensions.GetVector3, NetworkingExtensions.Put);
            TestSerialization(writer, reader, v4, NetworkingExtensions.GetVector4, NetworkingExtensions.Put);
            TestSerialization(writer, reader, q, NetworkingExtensions.GetQuaternion, NetworkingExtensions.Put);


            TestSerialization(writer, reader, custom);

            TestSerialization(writer, reader, health);
            TestSerialization(writer, reader, death);
            TestSerialization(writer, reader, dereg);
            TestSerialization(writer, reader, reg);
            TestSerialization(writer, reader, spawn);
            TestSerialization(writer, reader, update);

            TestSerialization(writer, reader, projDespawn);
            TestSerialization(writer, reader, projSpawn);
            TestSerialization(writer, reader, gameConfig);
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

        void TestSerialization<T>(
            NetDataWriter writer, NetDataReader reader, T testObject,
            Func<NetDataReader, T> Get, Action<NetDataWriter, T> Put)
            where T : struct
        {
            writer.Reset();
            Put(writer, testObject);

            reader.SetSource(writer);
            var receivedObject = Get(reader);

            Assert.AreEqual(testObject, receivedObject);
        }

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
}