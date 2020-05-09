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
        INPUT_INTERACT,
        GAME_OBJECT_REMOVE
    }

    public class GameObjectGenericMessage : IDarkRiftSerializable
    {
        public GameObjectGenericMessage() { }

        public virtual void Serialize(SerializeEvent e) { }

        public virtual void Deserialize(DeserializeEvent e) { }

        public virtual void Load() { }

        public static void SerializePosition(Vector3 position, SerializeEvent e)
        {
            e.Writer.Write(new float[] { position.x, position.y, position.z });
        }

        public static Vector3 DeserializePosition(DeserializeEvent e)
        {
            float[] p = e.Reader.ReadSingles();
            return new Vector3(p[0], p[1], p[2]);
        }

        public static void SerializeRotation(Quaternion rotation, SerializeEvent e)
        {
            e.Writer.Write(new float[] { rotation.x, rotation.y, rotation.z, rotation.w });
        }

        public static Quaternion DeserializeRotation(DeserializeEvent e)
        {
            float[] r = e.Reader.ReadSingles();
            return new Quaternion(r[0], r[1], r[2], r[3]);
        }
    }

    public class GameObjectInitMessage : GameObjectGenericMessage
    {
        ushort ttsid;
        Vector3 position;
        Quaternion rotation;
        ushort parentTTSID;
        string tag;
        string name;
        string prefabID;

        public GameObjectInitMessage() {
            position = new Vector3();
            rotation = new Quaternion();
        }

        public GameObjectInitMessage(GameObject go)
        {
            ttsid = go.GetComponent<TTSID>().id;
            position = go.transform.localPosition;
            rotation = go.transform.localRotation;
            parentTTSID = TTSID.Get(go.transform.parent);
            tag = go.tag;
            name = go.name;
            prefabID = TTSPrefabID.Get(go);
        }

        public override void Load()
        {
            // Setup GameObject
            GameObject go;
            if (prefabID != "")
            {
                //Debug.Log(name);
                //Debug.Log(prefabID); // prefabID = "T"
                if (prefabID == "T")
                {
                    prefabID = "TrainTrack";
                }
                //Debug.Log(prefabID);
                go = GameObject.Instantiate(Resources.Load($"Prefabs/{prefabID}", typeof(GameObject))) as GameObject;
                go.name = name;
            }
            else
            {
                go = new GameObject(name);
                go.AddComponent<TTSID>();
                go.tag = tag;
            }
            // Setup Transform
            go.transform.parent = GameObject.FindGameObjectWithTag("Network")
                .GetComponent<TTSIDMap>().getTransform(parentTTSID);
            go.transform.localPosition = position;
            go.transform.localRotation = rotation;
            // Setup TTSID
            go.GetComponent<TTSID>().id = ttsid;
            GameObject.FindGameObjectWithTag("Network")
                .GetComponent<TTSIDMap>().addTransform(ttsid, go.transform);
        }

        public override void Deserialize(DeserializeEvent e)
        {
            ttsid = e.Reader.ReadUInt16();
            position = GameObjectGenericMessage.DeserializePosition(e);
            rotation = GameObjectGenericMessage.DeserializeRotation(e);
            parentTTSID = e.Reader.ReadUInt16();
            tag = e.Reader.ReadString();
            name = e.Reader.ReadString();
            prefabID = e.Reader.ReadString();
        }

        public override void Serialize(SerializeEvent e)
        {
            e.Writer.Write(ttsid);
            GameObjectGenericMessage.SerializePosition(position, e);
            GameObjectGenericMessage.SerializeRotation(rotation, e);
            e.Writer.Write(parentTTSID);
            e.Writer.Write(tag);
            e.Writer.Write(name);
            e.Writer.Write(prefabID);
        }
    }

    public class GameObjectRemoveMessage : GameObjectGenericMessage
    {
        ushort ttsid;

        public GameObjectRemoveMessage() { }

        public GameObjectRemoveMessage(ushort id)
        {
            ttsid = id;
        }

        public override void Load()
        {
            GameObject.FindGameObjectWithTag("Network")
                .GetComponent<TTSIDMap>().getTransform(ttsid)
                .GetComponent<TTSID>().Remove();
        }

        public override void Deserialize(DeserializeEvent e)
        {
            ttsid = e.Reader.ReadUInt16();
        }

        public override void Serialize(SerializeEvent e)
        {
            e.Writer.Write(ttsid);
        }
    }

    public class GameObjectMovementMessage : GameObjectGenericMessage {
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

        public override void Load()
        {
            Transform target = GameObject.FindGameObjectWithTag("Network")
                .GetComponent<TTSIDMap>().getTransform(ttsid);
            target.localPosition = position;
            target.localRotation = rotation;
          
        }

        public override void Deserialize(DeserializeEvent e)
        {
            ttsid = e.Reader.ReadUInt16();
            position = GameObjectGenericMessage.DeserializePosition(e);
            rotation = GameObjectGenericMessage.DeserializeRotation(e);
        }

        public override void Serialize(SerializeEvent e)
        {
            e.Writer.Write(ttsid);
            GameObjectGenericMessage.SerializePosition(position, e);
            GameObjectGenericMessage.SerializeRotation(rotation, e);
        }
    }

    public class GameObjectTDataMessage : GameObjectGenericMessage
    {
        public ushort ttsid;
        public ushort parent;
        public List<TDataMessagePart> trackedData;

        public GameObjectTDataMessage() { }

        public GameObjectTDataMessage(Transform target, List<TTS.TDataMessagePart> trackedData)
        {
            this.ttsid = target.GetComponent<TTSID>().id;
            this.parent = TTSID.Get(target.parent);
            this.trackedData = trackedData;
        }

        public override void Load()
        {
            Transform target = GameObject.FindGameObjectWithTag("Network")
                .GetComponent<TTSIDMap>().getTransform(ttsid);
            target.parent = GameObject.FindGameObjectWithTag("Network")
                .GetComponent<TTSIDMap>().getTransform(parent);
            foreach (TDataMessagePart m in trackedData)
                m.Load(target);
        }

        public override void Serialize(SerializeEvent e)
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
        
        public override void Deserialize(DeserializeEvent e)
        {
            ttsid = e.Reader.ReadUInt16();
            parent = e.Reader.ReadUInt16();
            //Debug.Log($"{ttsid} {parent}");
            //Works here
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