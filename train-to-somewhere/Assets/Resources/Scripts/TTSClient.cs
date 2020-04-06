using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;

public class TTSClient : MonoBehaviour
{
    public System.Net.IPAddress defaultAddress = System.Net.IPAddress.Parse("127.0.0.1");
    public ushort defaultPort = 4296;

    private void Awake()
    {
        string rawAddr = IniFile.IniReadValue(IniFile.Sections.Train, IniFile.Keys.Address);
        System.Net.IPAddress address;
        if (rawAddr == null)
        {
            address = defaultAddress;
            IniFile.IniWriteValue(IniFile.Sections.Train, IniFile.Keys.Address, address.ToString());
        } else
        {
            address = System.Net.IPAddress.Parse(rawAddr);
        }

        string rawPort = IniFile.IniReadValue(IniFile.Sections.Train, IniFile.Keys.Port);
        ushort port;
        if (rawPort == null)
        {
            port = defaultPort;
            IniFile.IniWriteValue(IniFile.Sections.Train, IniFile.Keys.Port, port.ToString());
        } else
        {
            port = ushort.Parse(rawPort);
        }

        //UnityClient client = gameObject.AddComponent(typeof(UnityClient)) as UnityClient;
        UnityClient client = gameObject.AddComponent<UnityClient>() as UnityClient;
        client.Address = address;
        client.Port = port;
    }
}
