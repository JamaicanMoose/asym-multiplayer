using System;
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
        public List<GameObjectInitMessage> initBuffer = new List<GameObjectInitMessage>();
        public List<GameObjectRemoveMessage> removeBuffer = new List<GameObjectRemoveMessage>();

        XmlUnityServer darkRiftServer;

        // Start is called before the first frame update
        void Start()
        {
            darkRiftServer = GameObject.FindGameObjectWithTag("Network").GetComponent<XmlUnityServer>();
        }

        public void TriggerBufferSync() { Update(); }

        private void SyncBuffer<T>(List<T> buf, SendMode mode, MessageType mt) where T : IDarkRiftSerializable
        {
            using (DarkRiftWriter w = DarkRiftWriter.Create())
            {
                w.Write((uint)buf.Count);
                foreach (T s in buf)
                    w.Write(s);
                using (Message m = Message.Create((ushort)mt, w))
                    foreach (IClient c in darkRiftServer.Server.ClientManager.GetAllClients())
                        c.SendMessage(m, mode);
            }
            buf.Clear();
        }

        // Update is called once per frame
        void Update()
        {
            if (movementBuffer.Count > 0)
                SyncBuffer<GameObjectMovementMessage>(movementBuffer, SendMode.Unreliable, MessageType.GAME_OBJECT_MOVE);
            if (trackedDataBuffer.Count > 0)
                SyncBuffer<GameObjectTDataMessage>(trackedDataBuffer, SendMode.Reliable, MessageType.GAME_OBJECT_TDATA);
            if (removeBuffer.Count > 0)
                SyncBuffer<GameObjectRemoveMessage>(removeBuffer, SendMode.Reliable, MessageType.GAME_OBJECT_REMOVE);
            if (initBuffer.Count > 0)
                SyncBuffer<GameObjectInitMessage>(initBuffer, SendMode.Reliable, MessageType.GAME_OBJECT_INIT);
        }
    }
}
