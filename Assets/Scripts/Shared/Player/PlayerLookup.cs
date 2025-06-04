using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Player
{
    //public class PlayerLookup
    //{
    //    Dictionary<int, Player> players;

    //    public PlayerLookup()
    //    {
    //        players = new Dictionary<int, Player>(NetworkConfig.MaxConnectionCount);
    //    }

    //    public void OnPlayerInstantiated(Player player)
    //    {
    //        var ID = player.ID;
    //        if (players.ContainsKey(ID))
    //        {
    //            Console.WriteLine($"OnPlayerInstantiated Error: player ID {ID} already exists! overwriting...");
    //            //TODO();//cleanup previous? this shouldn't be happening anyway
    //        }

    //        players[ID] = player;
    //    }
    //    public void OnPlayerDestroyed(Player player)
    //    {
    //        var ID = player.ID;
    //        if (!players.ContainsKey(ID))
    //        {
    //            Console.WriteLine($"OnPlayerDestroyed Error: player ID {ID} not found in lookup! ignoring...");
    //            return;
    //        }

    //        players.Remove(ID);
    //    }
    //    public bool TryGetPlayer(int ID, out Player player)
    //        => players.TryGetValue(ID, out player);
    //}
}
