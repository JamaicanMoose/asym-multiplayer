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

        public float angleX = 0;
        public float angleY = 0;
        public float angleZ = 0;

        public Player(ushort ID, float x, float z)
        {
            this.ID = ID;
            this.X = x;
            this.Z = z;
        }

    }

    class Train
    {
        public ushort ID;
        public List<TrainCar> Cars;

        public Train(ushort ID)
        {
            this.ID = ID;
            Cars = new List<TrainCar>();

        }
    }

    class TrainCar
    {
        public ushort ID;
        public ushort CarType;
        //public List<InteractableObject> Objects;

        public TrainCar(ushort ID, ushort type)
        {
            this.ID = ID;
            this.CarType = type;
          //  this.Objects = new List<InteractableObject>();
        }
    }

    class InteractableObject
    {
        public ushort ID;
        public ushort objectType;
        public ushort parentCarID;

        //Local position relative to parentCar
        public float X;
        public float Y;
        public float Z;

        //Euler angles for rotation
        public float angleX = 0;
        public float angleY = 0;
        public float angleZ = 0;

        public InteractableObject(ushort ID, ushort type, float x, float y, float z)
        {
            this.ID = ID;
            this.objectType = type;
            this.X = x;
            this.Y = y;
            this.Z = z;

    
        }

    }
    
    public class TrainPlayerManager : Plugin
    {

        Dictionary<IClient, Player> players = new Dictionary<IClient, Player>();
        Dictionary<ushort, InteractableObject> objects = new Dictionary<ushort, InteractableObject>();

        Train testTrain;

        ushort IDCounter = 0;

        //MESSAGE TAGS
        const ushort SPAWN_TAG = 0;
        const ushort MOVE_TAG = 1;
        const ushort DESPAWN_TAG = 2;
        const ushort TRAIN_TAG = 3;
        const ushort OBJECT_TAG = 4;
        const ushort PICKUP_MOVE_TAG = 5;
        const ushort USE_TAG = 6;

        //TRAIN CAR TYPES
        const ushort ENGINE = 0;
        const ushort TESTCAR = 1;
        const ushort CABOOSE = 2;

        //INTERACTABLE OBJECT TYPES
        const ushort TEST_INTERACTABLE = 0;
        const ushort TEST_PICKUP = 1;

        public override bool ThreadSafe => false;

        public override Version Version => new Version(1, 0, 0);

        public TrainPlayerManager(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
            ClientManager.ClientConnected += ClientConnected;
            ClientManager.ClientDisconnected += ClientDisconnected;
            InitializeTrain();
            InitializeObjects();
        }

        void InitializeTrain()
        {
            testTrain = new Train(UniqueID());
            TrainCar engine = new TrainCar(UniqueID(), ENGINE);
            TrainCar testCar1 = new TrainCar(UniqueID(), TESTCAR);
            TrainCar testCar2 = new TrainCar(UniqueID(), TESTCAR);
            TrainCar caboose = new TrainCar(UniqueID(), CABOOSE);
            testTrain.Cars.Add(engine);
            testTrain.Cars.Add(testCar1);
            testTrain.Cars.Add(testCar2);
            testTrain.Cars.Add(caboose);
        }

        void InitializeObjects()
        {
            InteractableObject testPickup = new InteractableObject(UniqueID(), TEST_PICKUP, 0, .75f, 1.79f);
            testPickup.parentCarID = testTrain.Cars[0].ID;
            objects.Add(testPickup.ID, testPickup);

            InteractableObject testInteractable = new InteractableObject(UniqueID(), TEST_INTERACTABLE, 1.23f, 0.59f, -4.61f);
            testInteractable.parentCarID = testTrain.Cars[1].ID;
            objects.Add(testInteractable.ID, testInteractable);
;        }

        private ushort UniqueID()
        {
            IDCounter += 1;
            return IDCounter;
        }

        void ClientConnected(object sender, ClientConnectedEventArgs e)
        {
            Player newPlayer = new Player(e.Client.ID, 0, 0);

            //Message to tell all current players about new player
            using (DarkRiftWriter newPlayerWriter = DarkRiftWriter.Create())
            {
                newPlayerWriter.Write(newPlayer.ID);
                newPlayerWriter.Write(newPlayer.X);
                newPlayerWriter.Write(newPlayer.Z);

                newPlayerWriter.Write(newPlayer.angleX);
                newPlayerWriter.Write(newPlayer.angleY);
                newPlayerWriter.Write(newPlayer.angleZ);



                using (Message newPlayerMessage = Message.Create(SPAWN_TAG, newPlayerWriter))
                {
                    foreach (IClient client in ClientManager.GetAllClients().Where(x => x != e.Client))
                        client.SendMessage(newPlayerMessage, SendMode.Reliable);
                }
            }

            players.Add(e.Client, newPlayer);

            //Message to tell new player about train
            using (DarkRiftWriter trainWriter = DarkRiftWriter.Create())
            {               

                trainWriter.Write(testTrain.Cars.Count);
                for(int i = 0; i < testTrain.Cars.Count; i++)
                {
                    trainWriter.Write(testTrain.Cars[i].ID);
                    trainWriter.Write(testTrain.Cars[i].CarType);       
                }
                using (Message trainMessage = Message.Create(TRAIN_TAG, trainWriter))
                    e.Client.SendMessage(trainMessage, SendMode.Reliable);

            }

            //Message to tell new player about interactable objects
            using (DarkRiftWriter objectWriter = DarkRiftWriter.Create())
            {

                objectWriter.Write(objects.Count);
                foreach(InteractableObject obj in objects.Values)
                {
                    objectWriter.Write(obj.ID);
                    objectWriter.Write(obj.objectType);
                    objectWriter.Write(obj.parentCarID);
                    objectWriter.Write(obj.X);
                    objectWriter.Write(obj.Y);
                    objectWriter.Write(obj.Z);
                    objectWriter.Write(obj.angleX);
                    objectWriter.Write(obj.angleY);
                    objectWriter.Write(obj.angleZ);

                }
                using (Message objectMessage = Message.Create(OBJECT_TAG, objectWriter))
                    e.Client.SendMessage(objectMessage, SendMode.Reliable);

            }
            //Mesage to tell client that connected about all players            
            using (DarkRiftWriter playerWriter = DarkRiftWriter.Create())
            {
                foreach (Player player in players.Values)
                {
                    playerWriter.Write(player.ID);
                    playerWriter.Write(player.X);
                    playerWriter.Write(player.Z);

                    playerWriter.Write(player.angleX);
                    playerWriter.Write(player.angleY);
                    playerWriter.Write(player.angleZ);
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
;           }
            else if(e.GetMessage().Tag ==  PICKUP_MOVE_TAG)
            {
                PickupMoveMessageReceived(sender, e);
            }
            else if(e.GetMessage().Tag == USE_TAG)
            {
                UseMessageReceived(sender, e);
            }
        }

        void UseMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    ushort objID = reader.ReadUInt16();
                    ushort useTag = reader.ReadUInt16();
             

                    using (DarkRiftWriter writer = DarkRiftWriter.Create())
                    {
                        writer.Write(objID);
                        writer.Write(useTag);

                        using (Message useMessage = Message.Create(USE_TAG, writer))
                        {
                            foreach (IClient c in ClientManager.GetAllClients().Where(x => x != e.Client))
                                c.SendMessage(useMessage, SendMode.Reliable);
                        }


                    }

                }
            }
        }

        void PickupMoveMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    ushort objID = reader.ReadUInt16();
                    ushort parentID = reader.ReadUInt16();
                    if (parentID == 0)
                        return;
                    float newX = reader.ReadSingle();
                    float newY = reader.ReadSingle();
                    float newZ = reader.ReadSingle();

                    float newAngleX = reader.ReadSingle();
                    float newAngleY = reader.ReadSingle();
                    float newAngleZ = reader.ReadSingle();

                    InteractableObject obj = objects[objID];
                    obj.parentCarID = parentID;
                    obj.X = newX;
                    obj.Y = newY;
                    obj.Z = newZ;

                    obj.angleX = newAngleX;
                    obj.angleY = newAngleY;
                    obj.angleZ = newAngleZ;

                    using (DarkRiftWriter writer = DarkRiftWriter.Create())
                    {
                        writer.Write(objID);
                        writer.Write(parentID);
                        writer.Write(obj.X);
                        writer.Write(obj.Y);
                        writer.Write(obj.Z);
                        writer.Write(newAngleX);
                        writer.Write(newAngleY);
                        writer.Write(newAngleZ);

                        using (Message pickupMoveMessage = Message.Create(PICKUP_MOVE_TAG, writer))
                        {
                            foreach (IClient c in ClientManager.GetAllClients().Where(x => x != e.Client))
                                c.SendMessage(pickupMoveMessage, SendMode.Unreliable);
                        }
                     

                    }                
             
                }
            }
        }

        void MovementMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    float newX = reader.ReadSingle();
                    float newZ = reader.ReadSingle();

                    float newAngleX = reader.ReadSingle();
                    float newAngleY = reader.ReadSingle();
                    float newAngleZ = reader.ReadSingle();

                    Player player = players[e.Client];

                    player.X = newX;
                    player.Z = newZ;

                    player.angleX = newAngleX;
                    player.angleY = newAngleY;
                    player.angleZ = newAngleZ;

                  using (DarkRiftWriter writer = DarkRiftWriter.Create())
                {
                    writer.Write(player.ID);
                    writer.Write(player.X);
                    writer.Write(player.Z);
                    writer.Write(player.angleX);
                    writer.Write(player.angleY);
                    writer.Write(player.angleZ);

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
                foreach(InteractableObject obj in objects.Values)
                {
                    Console.WriteLine("X : " + obj.X + " Y: " + obj.Y + " Z: " + obj.Z);
                }
               
            }
        }
    }

   


   
}
