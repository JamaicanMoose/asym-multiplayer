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
        public ushort parentCarID { get; set; }

        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public float angleX = 0;
        public float angleY = 0;
        public float angleZ = 0;

        public Player(ushort ID, ushort parentID, float x, float y, float z)
        {
            this.ID = ID;
            this.parentCarID = parentID;

            this.X = x;
            this.Y = y;
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

        bool gameStarted = false;

        //MESSAGE TAGS
        const ushort SPAWN_TAG = 0;
        const ushort MOVE_TAG = 1;
        const ushort DESPAWN_TAG = 2;
        const ushort TRAIN_TAG = 3;
        const ushort OBJECT_TAG = 4;
        const ushort PICKUP_MOVE_TAG = 5;
        const ushort USE_TAG = 6;
        const ushort NUM_PLAYERS_TAG = 7;
        const ushort LAUNCH_TAG = 8;
        const ushort DESTROY_OBJ_TAG = 9;
        const ushort SPAWN_OBJ_TAG = 10;

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
            InteractableObject testPickup = new InteractableObject(UniqueID(), TEST_PICKUP, 0, 3.4f, -15f);
            objects.Add(testPickup.ID, testPickup);

            InteractableObject testInteractable = new InteractableObject(UniqueID(), TEST_INTERACTABLE, 1.23f, 3.4f, -4.61f);
            objects.Add(testInteractable.ID, testInteractable);
            
        }

        private ushort UniqueID()
        {
            IDCounter += 1;
            return IDCounter;
        }

        void ClientConnected(object sender, ClientConnectedEventArgs e)
        {
            //Create a new player object
            //1.07f is default player y
            Player newPlayer = new Player(e.Client.ID, testTrain.Cars[1].ID, 0, .5f, 0);

            //Add that player to our players dictionary
            players.Add(e.Client, newPlayer);

            using (DarkRiftWriter numPlayersWriter = DarkRiftWriter.Create())
            {
                numPlayersWriter.Write(players.Count);   

                using (Message numPlayersMessage = Message.Create(NUM_PLAYERS_TAG, numPlayersWriter))
                {
                    foreach (IClient client in ClientManager.GetAllClients())
                        client.SendMessage(numPlayersMessage, SendMode.Reliable);
                }
            }        

            e.Client.MessageReceived += ClientMessageReceived;

        }

        void ClientMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (e.GetMessage().Tag == MOVE_TAG)
            {
                MovementMessageReceived(sender, e);
                
            }
            else if (e.GetMessage().Tag == PICKUP_MOVE_TAG)
            {
                PickupMoveMessageReceived(sender, e);
            }
            else if (e.GetMessage().Tag == USE_TAG)
            {
                UseMessageReceived(sender, e);
            }
            else if(e.GetMessage().Tag == LAUNCH_TAG)
            {
                if(!gameStarted)
                {
                    LaunchGame();
                }
                else
                {
                   // AddPlayer(sender ,e);
                }
               
            }
            else if (e.GetMessage().Tag == DESTROY_OBJ_TAG)
            {
                DestroyObject(sender, e);
            }
            else if (e.GetMessage().Tag == SPAWN_OBJ_TAG)
            {
                SpawnObject(sender, e);
            }
        }

        void LaunchGame()
        {
            if(!gameStarted)
            {
                //Message to tell all players about train
                using (DarkRiftWriter trainWriter = DarkRiftWriter.Create())
                {

                    trainWriter.Write(testTrain.Cars.Count);
                    for (int i = 0; i < testTrain.Cars.Count; i++)
                    {
                        trainWriter.Write(testTrain.Cars[i].ID);
                        trainWriter.Write(testTrain.Cars[i].CarType);
                    }
                    using (Message trainMessage = Message.Create(TRAIN_TAG, trainWriter))
                    {
                        foreach (IClient client in ClientManager.GetAllClients())
                            client.SendMessage(trainMessage, SendMode.Reliable);
                    }

                }

                //Message to tell new player about interactable objects
                using (DarkRiftWriter objectWriter = DarkRiftWriter.Create())
                {

                    objectWriter.Write(objects.Count);
                    foreach (InteractableObject obj in objects.Values)
                    {
                        objectWriter.Write(obj.ID);
                        objectWriter.Write(obj.objectType);
                 
                        objectWriter.Write(obj.X);
                        objectWriter.Write(obj.Y);
                        objectWriter.Write(obj.Z);
                        objectWriter.Write(obj.angleX);
                        objectWriter.Write(obj.angleY);
                        objectWriter.Write(obj.angleZ);

                    }
                    using (Message objectMessage = Message.Create(OBJECT_TAG, objectWriter))
                    {
                        foreach (IClient client in ClientManager.GetAllClients())
                            client.SendMessage(objectMessage, SendMode.Reliable);
                    }

                }
                //Mesage to tell client that connected about all players            
                using (DarkRiftWriter playerWriter = DarkRiftWriter.Create())
                {
                    foreach (Player player in players.Values)
                    {
                        playerWriter.Write(player.ID);
                        playerWriter.Write(player.parentCarID);

                        playerWriter.Write(player.X);
                        playerWriter.Write(player.Y);
                        playerWriter.Write(player.Z);

                        playerWriter.Write(player.angleX);
                        playerWriter.Write(player.angleY);
                        playerWriter.Write(player.angleZ);
                    }

                    using (Message playerMessage = Message.Create(0, playerWriter))
                    {
                        foreach (IClient client in ClientManager.GetAllClients())
                            client.SendMessage(playerMessage, SendMode.Reliable);
                    }
                }
            }
            gameStarted = true;
            
        }

        void AddPlayer(object sender, MessageReceivedEventArgs e)
        {
          
                //Message to tell all players about train
                using (DarkRiftWriter trainWriter = DarkRiftWriter.Create())
                {

                    trainWriter.Write(testTrain.Cars.Count);
                    for (int i = 0; i < testTrain.Cars.Count; i++)
                    {
                        trainWriter.Write(testTrain.Cars[i].ID);
                        trainWriter.Write(testTrain.Cars[i].CarType);
                    }
                    using (Message trainMessage = Message.Create(TRAIN_TAG, trainWriter))
                    {
                      
                            e.Client.SendMessage(trainMessage, SendMode.Reliable);
                    }

                }

                //Message to tell new player about interactable objects
                using (DarkRiftWriter objectWriter = DarkRiftWriter.Create())
                {

                    objectWriter.Write(objects.Count);
                    foreach (InteractableObject obj in objects.Values)
                    {
                        objectWriter.Write(obj.ID);
                        objectWriter.Write(obj.objectType);

                        objectWriter.Write(obj.X);
                        objectWriter.Write(obj.Y);
                        objectWriter.Write(obj.Z);
                        objectWriter.Write(obj.angleX);
                        objectWriter.Write(obj.angleY);
                        objectWriter.Write(obj.angleZ);

                    }
                    using (Message objectMessage = Message.Create(OBJECT_TAG, objectWriter))
                    {
                  
                        e.Client.SendMessage(objectMessage, SendMode.Reliable);
                    }

                }
                //Mesage to tell client that connected about all players            
                using (DarkRiftWriter playerWriter = DarkRiftWriter.Create())
                {
                    foreach (Player player in players.Values)
                    {
                        playerWriter.Write(player.ID);
                        playerWriter.Write(player.parentCarID);

                        playerWriter.Write(player.X);
                        playerWriter.Write(player.Y);
                        playerWriter.Write(player.Z);

                        playerWriter.Write(player.angleX);
                        playerWriter.Write(player.angleY);
                        playerWriter.Write(player.angleZ);
                    }

                    using (Message playerMessage = Message.Create(0, playerWriter))
                    {
                         e.Client.SendMessage(playerMessage, SendMode.Reliable);
                    }
                }
            //Mesage to tell all other clients about new player          
            using (DarkRiftWriter newPlayerWriter = DarkRiftWriter.Create())
            {
                Player newPlayer = players[e.Client];
                newPlayerWriter.Write(newPlayer.ID);
                newPlayerWriter.Write(newPlayer.parentCarID);

                newPlayerWriter.Write(newPlayer.X);
                newPlayerWriter.Write(newPlayer.Y);
                newPlayerWriter.Write(newPlayer.Z);

                newPlayerWriter.Write(newPlayer.angleX);
                newPlayerWriter.Write(newPlayer.angleY);
                newPlayerWriter.Write(newPlayer.angleZ);
                

                using (Message newPlayerMessage = Message.Create(0, newPlayerWriter))
                {
                    foreach (IClient c in ClientManager.GetAllClients().Where(x => x != e.Client))
                        c.SendMessage(newPlayerMessage, SendMode.Reliable);
                }
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
                    
                    float newX = reader.ReadSingle();
                    float newY = reader.ReadSingle();
                    float newZ = reader.ReadSingle();

                    float newAngleX = reader.ReadSingle();
                    float newAngleY = reader.ReadSingle();
                    float newAngleZ = reader.ReadSingle();

                    InteractableObject obj = objects[objID];
         
                    obj.X = newX;
                    obj.Y = newY;
                    obj.Z = newZ;

                    obj.angleX = newAngleX;
                    obj.angleY = newAngleY;
                    obj.angleZ = newAngleZ;

                    using (DarkRiftWriter writer = DarkRiftWriter.Create())
                    {
                        writer.Write(objID);
                  
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
                    ushort parentID = reader.ReadUInt16();

                    float newX = reader.ReadSingle();
                    float newY = reader.ReadSingle();
                    float newZ = reader.ReadSingle();

                    float newAngleX = reader.ReadSingle();
                    float newAngleY = reader.ReadSingle();
                    float newAngleZ = reader.ReadSingle();

                    Player player = players[e.Client];
                    player.parentCarID = parentID;

                    player.X = newX;
                    player.Y = newY;
                    player.Z = newZ;

                    player.angleX = newAngleX;
                    player.angleY = newAngleY;
                    player.angleZ = newAngleZ;

                    using (DarkRiftWriter writer = DarkRiftWriter.Create())
                    {
                        writer.Write(player.ID);
                        writer.Write(player.parentCarID);
                        writer.Write(player.X);
                        writer.Write(player.Y);
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

        void DestroyObject(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    ushort objID = reader.ReadUInt16();

                    objects.Remove(objID);

                    using (DarkRiftWriter writer = DarkRiftWriter.Create())
                    {
                        writer.Write(objID);               

                        message.Serialize(writer);
                    }

                    foreach (IClient c in ClientManager.GetAllClients())
                        c.SendMessage(message, e.SendMode);



                }
            }
        }

        void SpawnObject(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    ushort objType = reader.ReadUInt16();
                    float xPos = reader.ReadSingle();
                    float yPos = reader.ReadSingle();
                    float zPos = reader.ReadSingle(); 
                    float xRot = reader.ReadSingle();
                    float yRot = reader.ReadSingle();
                    float zRot = reader.ReadSingle();

                    InteractableObject newObject = new InteractableObject(UniqueID(), objType, xPos, yPos, zPos);
                    newObject.angleX = xRot;
                    newObject.angleY = yRot;
                    newObject.angleZ = zRot;
                    objects.Add(newObject.ID, newObject);

                    using (DarkRiftWriter writer = DarkRiftWriter.Create())
                    {
                        writer.Write(newObject.ID);
                        writer.Write(newObject.objectType);

                        writer.Write(newObject.X);
                        writer.Write(newObject.Y);
                        writer.Write(newObject.Z);

                        writer.Write(newObject.angleX);
                        writer.Write(newObject.angleY);
                        writer.Write(newObject.angleZ);

                        message.Serialize(writer);
                    }

                    foreach (IClient c in ClientManager.GetAllClients())
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

            if(players.Count == 0)
            {
                testTrain = null;
                objects.Clear();
                IDCounter = 0;
                InitializeTrain();
                InitializeObjects();

                gameStarted = false;
            }
        }
    }





}
