using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DarkRift;
using DarkRift.Server;
using DarkRift.Server.Unity;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Linq;

public class TTSLobby : MonoBehaviour
{
    public Text playerCount;
    public Text externalIP;
    XmlUnityServer darkRiftServer;
    bool acceptingConnections = true;

    private int spawnCount = 0;

    TTSServer server;

    private void Awake()
    {
        externalIP.text = new WebClient().DownloadString("http://icanhazip.com");
        darkRiftServer = gameObject.GetComponent<XmlUnityServer>();
        server = gameObject.GetComponent<TTSServer>();
    }


    // Start is called before the first frame update
    void Start()
    {
        darkRiftServer.Server.ClientManager.ClientConnected += TTSPlayerConnected;
        
    }


    void TTSPlayerConnected(object sender, ClientConnectedEventArgs e)
    {
        if (!acceptingConnections)
            e.Client.Disconnect();

        e.Client.MessageReceived += server.ClientMessageReceived;
    }


    // Update is called once per frame
    void Update()
    {
        playerCount.text = (1 + darkRiftServer.Server.ClientManager.Count).ToString();
    }

    IEnumerable<Transform> InOrderChildren(Transform root)
    {
        yield return root;
        if (root.gameObject.GetComponent<TTSPrefabID>() == null)
            foreach (Transform child in root)
                foreach (Transform t in InOrderChildren(child))
                    yield return t;
    }

    void TSSInitGame()
    {
        Transform world = GameObject.Find("World").transform;
        Transform train = GameObject.Find("Train").transform;
        Dictionary<ushort, ushort> clientPlayerMap = GameObject.FindGameObjectWithTag("Network").GetComponent<TTSServer>().clientPlayerMap;
        Dictionary<ushort, Transform> idTransformMap = GameObject.FindGameObjectWithTag("Network").GetComponent<TTSIDMap>().idMap;

        //spawn server player, client ID for serverPlayer is defined as 65000       
        GameObject serverPlayer = GameObject.Instantiate(Resources.Load($"Prefabs/NetworkPlayer", typeof(GameObject))) as GameObject;
        serverPlayer.transform.parent = train;
        serverPlayer.transform.localPosition = UniqueSpawnPosition();
        serverPlayer.GetComponent<TTSID>().Init();
        clientPlayerMap[65000] = serverPlayer.GetComponent<TTSID>().id;
  

        foreach (IClient c in darkRiftServer.Server.ClientManager.GetAllClients())
        {
      
            GameObject player = GameObject.Instantiate(Resources.Load($"Prefabs/NetworkPlayer", typeof(GameObject))) as GameObject;
            player.transform.parent = train;
            player.transform.localPosition = UniqueSpawnPosition();
            player.GetComponent<TTSID>().Init();
            clientPlayerMap[c.ID] = player.GetComponent<TTSID>().id;
            using (DarkRiftWriter playerAssocWriter = DarkRiftWriter.Create())
            {
                playerAssocWriter.Write(clientPlayerMap[c.ID]);
                using (Message playerAssocMessage = Message.Create(TTSMessage.PLAYER_ASSOC, playerAssocWriter))
                    c.SendMessage(playerAssocMessage, SendMode.Reliable);
            }
        }
        //foreach connected client
        //  create networked player instance
        //  add client id -> ttsid for networked player to map in TTSServer
        //  send player association message to client
        using (DarkRiftWriter initObjectsWriter = DarkRiftWriter.Create())
        {
            //IN WORLD ALL OBJECTS ARE CONTAINERS UNLESS THEYRE A PREFAB
            initObjectsWriter.Write(InOrderChildren(world).Count() - 1);
            foreach (Transform t in InOrderChildren(world))
            {
                if (t == world) continue;
                initObjectsWriter.Write(new TTSGameObjectInitMessage(t.gameObject));
                idTransformMap.Add(t.gameObject.GetComponent<TTSID>().id, t);
            }
            using (Message initObjectsMessage = Message.Create(TTSMessage.GAME_OBJECT_INIT, initObjectsWriter))
            {
                foreach (IClient c in darkRiftServer.Server.ClientManager.GetAllClients())
                    c.SendMessage(initObjectsMessage, SendMode.Reliable);
            }
        }

    }

    public void TTSStartGame()
    {
        acceptingConnections = false;
        TSSInitGame();
        GameObject.FindGameObjectWithTag("StartMenu").SetActive(false);

        GameObject.FindGameObjectWithTag("Network").GetComponent<TTSGeneric>().GameStart();

        using (DarkRiftWriter startGameWriter = DarkRiftWriter.Create())
        {
            using (Message startGameMessage = Message.Create(TTSMessage.START_GAME, startGameWriter))
            {
                foreach (IClient c in darkRiftServer.Server.ClientManager.GetAllClients())
                    c.SendMessage(startGameMessage, SendMode.Reliable);
            }
        }
   
    }

    private Vector3 UniqueSpawnPosition()
    {
        Vector3 startPosition = new Vector3(0, 3.5f, -1);
        Vector3 offSet = new Vector3(0, 0, 2);
        spawnCount++;

        //Server player spawn position is (0, 3.5f, 1), each position is +2z offset
        return startPosition + offSet * spawnCount;
    }
}
