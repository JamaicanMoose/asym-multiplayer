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
        else
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

    void SpawnPlayer(ushort clientID)
    {
        GameObject player = GameObject.Instantiate(Resources.Load($"Prefabs/NetworkPlayer", typeof(GameObject))) as GameObject;
        player.transform.parent = GameObject.Find("Train").transform;
        player.transform.localPosition = UniqueSpawnPosition();
        player.GetComponent<TTSID>().Init();
        GameObject.FindGameObjectWithTag("Network")
            .GetComponent<TTSIDMap>().addTransform(player.GetComponent<TTSID>().id, player.transform);
        GameObject.FindGameObjectWithTag("Network")
            .GetComponent<TTSServer>().clientPlayerMap[clientID] = player.GetComponent<TTSID>().id;
    }

    void TSSInitGame()
    {
        Transform world = GameObject.Find("World").transform;

        GameObject.Find("Train").GetComponent<TTSTrainController>().BuildTrain();

        // Instantiate players
        SpawnPlayer(65000);
        foreach (IClient c in darkRiftServer.Server.ClientManager.GetAllClients())
            SpawnPlayer(c.ID);

        // Sync all gameObjects
        IEnumerable<TTS.GameObjectInitMessage> oInitIter = from c in InOrderChildren(world)
                                                           where c != world
                                                           select new TTS.GameObjectInitMessage(c.gameObject);
        TTS.ObjectSync os = GetComponent<TTS.ObjectSync>();
        foreach (TTS.GameObjectInitMessage m in oInitIter)
            os.initBuffer.Add(m);
        os.TriggerBufferSync();

        // Tell clients to associate w) their respective NetworkPlayers
        Dictionary<ushort, ushort> clientPlayerMap = GameObject.FindGameObjectWithTag("Network")
            .GetComponent<TTSServer>().clientPlayerMap;
        foreach (IClient c in darkRiftServer.Server.ClientManager.GetAllClients())
            using (DarkRiftWriter w = DarkRiftWriter.Create())
            {
                w.Write(clientPlayerMap[c.ID]);
                using (Message m = Message.Create((ushort)TTS.MessageType.PLAYER_ASSOC, w))
                    c.SendMessage(m, SendMode.Reliable);
            }        
    }

    public void TTSStartGame()
    {
        acceptingConnections = false;
        TSSInitGame();
        GameObject.FindGameObjectWithTag("StartMenu").SetActive(false);

        GameObject.FindGameObjectWithTag("Network").GetComponent<TTSGeneric>().GameStart();

        using (DarkRiftWriter startGameWriter = DarkRiftWriter.Create())
            using (Message startGameMessage = Message.Create((ushort)TTS.MessageType.START_GAME, startGameWriter))
                foreach (IClient c in darkRiftServer.Server.ClientManager.GetAllClients())
                    c.SendMessage(startGameMessage, SendMode.Reliable);
    }

    private Vector3 UniqueSpawnPosition()
    {
        Vector3 startPosition = new Vector3(0, .5f, -1);
        Vector3 offSet = new Vector3(0, 0, 2);
        spawnCount++;

        //Server player spawn position is (0, 3.5f, 1), each position is +2z offset
        return startPosition + offSet * spawnCount;
    }
}
