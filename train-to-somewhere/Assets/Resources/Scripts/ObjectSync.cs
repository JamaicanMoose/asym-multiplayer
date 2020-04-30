using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift;
using DarkRift.Server;
using DarkRift.Server.Unity;

namespace TTS
{
    /*
     * TTS.ObjectSync Unity Script
     * - sends batch of TTSGameObjectSyncMessage at each frame
     * - should be last in script execution order
     */
    public class ObjectSync : MonoBehaviour
    {
        public List<GameObjectMovementMessage> movementBuffer = new List<GameObjectMovementMessage>();
        public List<GameObjectTDataMessage> trackedDataBuffer = new List<GameObjectTDataMessage>();

        XmlUnityServer darkRiftServer;

        // Start is called before the first frame update
        void Start()
        {
            darkRiftServer = GameObject.FindGameObjectWithTag("Network").GetComponent<XmlUnityServer>();
        }

        // Update is called once per frame
        void Update()
        {
            if (movementBuffer.Count > 0)
            {
                using (DarkRiftWriter objectSyncWriter = DarkRiftWriter.Create())
                {
                    objectSyncWriter.Write((uint)movementBuffer.Count);
                    foreach (GameObjectMovementMessage gos in movementBuffer)
                        objectSyncWriter.Write(gos);
                    using (Message objectSyncMessage = Message.Create((ushort)MessageType.GAME_OBJECT_MOVE, objectSyncWriter))
                        foreach (IClient c in darkRiftServer.Server.ClientManager.GetAllClients())
                            c.SendMessage(objectSyncMessage, SendMode.Unreliable);
                }
                movementBuffer.Clear();
            }
            if (trackedDataBuffer.Count > 0)
            {
                using (DarkRiftWriter objectSyncWriter = DarkRiftWriter.Create())
                {
                    objectSyncWriter.Write((uint)trackedDataBuffer.Count);
                    foreach (GameObjectTDataMessage gos in trackedDataBuffer)
                        objectSyncWriter.Write(gos);
                    using (Message objectSyncMessage = Message.Create((ushort)MessageType.GAME_OBJECT_TDATA, objectSyncWriter))
                        foreach (IClient c in darkRiftServer.Server.ClientManager.GetAllClients())
                            c.SendMessage(objectSyncMessage, SendMode.Reliable);
                }
                trackedDataBuffer.Clear();
            }
        }
    }
}
