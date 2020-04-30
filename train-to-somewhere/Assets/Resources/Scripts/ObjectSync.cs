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
        public List<TTSGameObjectSyncMessage> buffer = new List<TTSGameObjectSyncMessage>();

        XmlUnityServer darkRiftServer;

        // Start is called before the first frame update
        void Start()
        {
            darkRiftServer = GameObject.FindGameObjectWithTag("Network").GetComponent<XmlUnityServer>();
        }

        // Update is called once per frame
        void Update()
        {
            if (buffer.Count > 0)
            {
                using (DarkRiftWriter objectSyncWriter = DarkRiftWriter.Create())
                {
                    objectSyncWriter.Write((uint)buffer.Count);
                    foreach (TTSGameObjectSyncMessage gos in buffer)
                        objectSyncWriter.Write(gos);
                    using (Message objectSyncMessage = Message.Create(TTSMessage.GAME_OBJECT_SYNC, objectSyncWriter))
                        foreach (IClient c in darkRiftServer.Server.ClientManager.GetAllClients())
                            c.SendMessage(objectSyncMessage, SendMode.Unreliable);
                }
                buffer.Clear();
            }
        }
    }
}
