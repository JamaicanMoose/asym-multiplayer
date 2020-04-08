using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DarkRift;
using DarkRift.Server;

namespace TrainPlugin
{
    

    class Player
    {
        public ushort ID { get; set; }
        public float X { get; set; }
        public float Z { get; set; }

        public Player(ushort ID, float x, float z)
        {
            this.ID = ID;
            this.X = x;
            this.Z = z;
        }

    }
    
    public class TrainPlayerManager : Plugin
    {

        Dictionary<IClient, Player> players = new Dictionary<IClient, Player>();

        ushort SPAWN_TAG = 0;
        ushort MOVE_TAG = 1;
        ushort DESPAWN_TAG = 2;
        public override bool ThreadSafe => false;

        public override Version Version => new Version(1, 0, 0);

        public TrainPlayerManager(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
            ClientManager.ClientConnected += ClientConnected;
            ClientManager.ClientDisconnected += ClientDisconnected;
        }

        void ClientConnected(object sender, ClientConnectedEventArgs e)
        {
            Player newPlayer = new Player(e.Client.ID, 0, 0);

            //Message to 
            using (DarkRiftWriter newPlayerWriter = DarkRiftWriter.Create())
            {
                newPlayerWriter.Write(newPlayer.ID);
                newPlayerWriter.Write(newPlayer.X);
                newPlayerWriter.Write(newPlayer.Z);

                using (Message newPlayerMessage = Message.Create(SPAWN_TAG, newPlayerWriter))
                {
                    foreach (IClient client in ClientManager.GetAllClients().Where(x => x != e.Client))
                        client.SendMessage(newPlayerMessage, SendMode.Reliable);
                }
            }

            players.Add(e.Client, newPlayer);

            //Mesage to tell client that connected about all players            
            using (DarkRiftWriter playerWriter = DarkRiftWriter.Create())
            {
                foreach (Player player in players.Values)
                {
                    playerWriter.Write(player.ID);
                    playerWriter.Write(player.X);
                    playerWriter.Write(player.Z);
                }

                using (Message playerMessage = Message.Create(0, playerWriter))
                    e.Client.SendMessage(playerMessage, SendMode.Reliable);          
            }

            e.Client.MessageReceived += ClientMessageReceived;
            
        }

        void ClientMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if(e.GetMessage().Tag == MOVE_TAG)
            {
                MovementMessageReceived(sender, e);
;            }
        }

        void MovementMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    float newX = reader.ReadSingle();
                    float newZ = reader.ReadSingle();

                    Player player = players[e.Client];

                    player.X = newX;
                    player.Z = newZ;

                  using (DarkRiftWriter writer = DarkRiftWriter.Create())
                {
                    writer.Write(player.ID);
                    writer.Write(player.X);
                    writer.Write(player.Z);
                    message.Serialize(writer);
                }

                foreach (IClient c in ClientManager.GetAllClients().Where(x => x != e.Client))
                    c.SendMessage(message, e.SendMode);

                  

                }
            }
        }

        void ClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            players.Remove(e.Client);

            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(e.Client.ID);

                using (Message message = Message.Create(DESPAWN_TAG, writer))
                {
                    foreach (IClient client in ClientManager.GetAllClients())
                        client.SendMessage(message, SendMode.Reliable);
                }
            }
        }
    }

   


   
}
