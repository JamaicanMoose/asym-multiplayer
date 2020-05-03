using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift;

namespace TTS
{
    enum MessageType : ushort
    {
        START_GAME,
        GAME_OBJECT_INIT,
        GAME_OBJECT_MOVE,
        GAME_OBJECT_TDATA,
        PLAYER_ASSOC,
        INPUT_MOVEMENT,
        INPUT_INTERACT
    }

    public class GameObjectInitMessage : IDarkRiftSerializable
    {
        ushort ttsid;
        Vector3 position;
        Quaternion rotation;
        ushort parentTTSID;
        string tag;
        string name;
        string prefabID;

        TTSIDMap idMap = GameObject.FindGameObjectWithTag("Network").GetComponent<TTSIDMap>();

        public GameObjectInitMessage()
        {

        }

        public GameObjectInitMessage(GameObject go)
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
            Transform parent = idMap.getTransform(parentTTSID);
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
            idMap.setTransform(go.transform);
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

    public class GameObjectMovementMessage : IDarkRiftSerializable {
        public ushort ttsid;
        public Vector3 position;
        public Quaternion rotation;

        public GameObjectMovementMessage() { }

        public GameObjectMovementMessage(Transform target)
        {
            ttsid = target.GetComponent<TTSID>().id;
            position = target.localPosition;
            rotation = target.localRotation;
        }

        public void Load(Transform target)
        {
            target.localPosition = position;
            target.localRotation = rotation;
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

    public class GameObjectTDataMessage : IDarkRiftSerializable
    {
        public ushort ttsid;
        public ushort parent;
        public List<TDataMessagePart> trackedData;

        public GameObjectTDataMessage() { }

        public GameObjectTDataMessage(Transform target, List<TTS.TDataMessagePart> trackedData)
        {
            this.ttsid = target.GetComponent<TTSID>().id;
            if (target.parent.GetComponent<TTSID>() != null)
                this.parent = target.parent.GetComponent<TTSID>().id;
            else
                this.parent = 0;
            this.trackedData = trackedData;
        }

        public void Load(Transform target)
        {
            target.parent = GameObject.FindGameObjectWithTag("Network").GetComponent<TTSIDMap>().getTransform(parent);

            foreach (TDataMessagePart m in trackedData)
                m.Load(target);
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(ttsid);
            e.Writer.Write(parent);

            e.Writer.Write((uint)trackedData.Count);
            foreach (TTS.TDataMessagePart m in trackedData)
            {
                e.Writer.Write(m.GetType().ToString());
                e.Writer.Write(m);
            }
        }
        
        public void Deserialize(DeserializeEvent e)
        {
            ttsid = e.Reader.ReadUInt16();
            parent = e.Reader.ReadUInt16();
            Debug.Log($"{ttsid} {parent}");

            trackedData = new List<TDataMessagePart>();
            uint numTrackedData = e.Reader.ReadUInt32();
            for (int i = 0; i < numTrackedData; i++)
            {
                Type mType = Type.GetType(e.Reader.ReadString());
                var readMType = e.Reader.GetType().GetMethod("ReadSerializable").MakeGenericMethod(new[] { mType });
                trackedData.Add(readMType.Invoke(e.Reader, new object[] { }) as TTS.TDataMessagePart);
            }
        }
    }

    public class InputInteractMessage : IDarkRiftSerializable
    {
        public bool fire1;
        public bool fire2;

        public InputInteractMessage() { }

        public InputInteractMessage(bool fire1, bool fire2)
        {
            this.fire1 = fire1;
            this.fire2 = fire2;
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(fire1);
            e.Writer.Write(fire2);
        }

        public void Deserialize(DeserializeEvent e)
        {
            fire1 = e.Reader.ReadBoolean();
            fire2 = e.Reader.ReadBoolean();
        }
    }

    public class InputMovementMessage : IDarkRiftSerializable
    {
        public Vector3 MoveDirection;
        public bool Dashing;

        public InputMovementMessage()
        {

        }

        public InputMovementMessage(Vector3 moveDirection, bool dashing)
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

    public abstract class TDataMessagePart : IDarkRiftSerializable
    {
        public abstract void Serialize(SerializeEvent e);

        public abstract void Deserialize(DeserializeEvent e);

        /*
         * Loads message data onto game object identified by ttsid.
         */
        public abstract void Load(Transform target);
    }
}