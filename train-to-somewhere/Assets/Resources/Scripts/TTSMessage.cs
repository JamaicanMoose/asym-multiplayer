using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift;

public class TTSMessage
{
    //S -> C
    public const ushort START_GAME = 0;
    public const ushort GAME_OBJECT_INIT = 1; //Setup GameObject on client
    public const ushort GAME_OBJECT_SYNC = 2; //Sync diff between server & client
    public const ushort PLAYER_ASSOC = 3;
    public const ushort MOVEMENT_INPUT = 4;
}

public class TTSGameObjectInitMessage : IDarkRiftSerializable
{
    ushort ttsid;
    Vector3 position;
    Quaternion rotation;
    ushort parentTTSID;
    string tag;
    string name;
    string prefabID;

    Dictionary<ushort,Transform> idMap = GameObject.FindGameObjectWithTag("Network").GetComponent<TTSIDMap>().idMap;

    public TTSGameObjectInitMessage()
    {

    }

    public TTSGameObjectInitMessage(GameObject go)
    {
        ttsid = go.GetComponent<TTSID>().id;
        Transform t = go.GetComponent<Transform>();
        position = t.localPosition;
        rotation = t.localRotation;
        TTSID ttsParentComp = go.transform.parent.GetComponent<TTSID>();
        if (ttsParentComp == null)
            parentTTSID = 0;
        else
            parentTTSID = ttsParentComp.id;
        tag = go.tag;
        name = go.name;
        TTSPrefabID pfid = go.GetComponent<TTSPrefabID>();
        if (pfid)
            prefabID = pfid.prefabID;
        else
            prefabID = "";
    }

    public void Load()
    {
        Transform parent = idMap[parentTTSID];
        GameObject go;
        TTSID idComp;
        if (prefabID != "")
        {
            go = GameObject.Instantiate(Resources.Load($"Prefabs/{prefabID}", typeof(GameObject))) as GameObject;
            go.name = name;
            idComp = go.GetComponent<TTSID>();
        }
        else
        {
            go = new GameObject(name);
            go.tag = tag;
            idComp = go.AddComponent<TTSID>();
        }
        go.transform.localPosition = position;
        go.transform.localRotation = rotation;
        go.transform.parent = parent;
        idComp.id = ttsid;
        idMap[ttsid] = go.transform;
    }

    public void Deserialize(DeserializeEvent e)
    {
        ttsid = e.Reader.ReadUInt16();
        position.x = e.Reader.ReadSingle();
        position.y = e.Reader.ReadSingle();
        position.z = e.Reader.ReadSingle();
        rotation.w = e.Reader.ReadSingle();
        rotation.x = e.Reader.ReadSingle();
        rotation.y = e.Reader.ReadSingle();
        rotation.z = e.Reader.ReadSingle();
        parentTTSID = e.Reader.ReadUInt16();
        tag = e.Reader.ReadString();
        name = e.Reader.ReadString();
        prefabID = e.Reader.ReadString();
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(ttsid);
        e.Writer.Write(position.x);
        e.Writer.Write(position.y);
        e.Writer.Write(position.z);
        e.Writer.Write(rotation.w);
        e.Writer.Write(rotation.x);
        e.Writer.Write(rotation.y);
        e.Writer.Write(rotation.z);
        e.Writer.Write(parentTTSID);
        e.Writer.Write(tag);
        e.Writer.Write(name);
        e.Writer.Write(prefabID);
    }
}

public class TTSInputMessage : IDarkRiftSerializable
{

    public Vector3 MoveDirection;
    public bool Dashing;

    public TTSInputMessage()
    {
       
    }

    public TTSInputMessage(Vector3 moveDirection, bool dashing)
    {
        MoveDirection = moveDirection;
        Dashing = dashing;
    }


    public void Deserialize(DeserializeEvent e)
    {      
        MoveDirection.x = e.Reader.ReadSingle();
        MoveDirection.y = e.Reader.ReadSingle();
        MoveDirection.z = e.Reader.ReadSingle();
        Dashing = e.Reader.ReadBoolean();     
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(MoveDirection.x);
        e.Writer.Write(MoveDirection.y);
        e.Writer.Write(MoveDirection.z);
        e.Writer.Write(Dashing);       
    }

}

public class TTSGameObjectSyncMessage : IDarkRiftSerializable
{
    public ushort ttsid;
    public Vector3 position;
    public Quaternion rotation;

    public TTSGameObjectSyncMessage()
    {

    }

    public TTSGameObjectSyncMessage(ushort id, Transform t)
    {
        ttsid = id;
        position = t.localPosition;
        rotation = t.localRotation;
    }


    public void Deserialize(DeserializeEvent e)
    {
        ttsid = e.Reader.ReadUInt16();
        position.x = e.Reader.ReadSingle();
        position.y = e.Reader.ReadSingle();
        position.z = e.Reader.ReadSingle();
        rotation.w = e.Reader.ReadSingle();
        rotation.x = e.Reader.ReadSingle();
        rotation.y = e.Reader.ReadSingle();
        rotation.z = e.Reader.ReadSingle();
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(ttsid);
        e.Writer.Write(position.x);
        e.Writer.Write(position.y);
        e.Writer.Write(position.z);
        e.Writer.Write(rotation.w);
        e.Writer.Write(rotation.x);
        e.Writer.Write(rotation.y);
        e.Writer.Write(rotation.z);
    }
     
    
}