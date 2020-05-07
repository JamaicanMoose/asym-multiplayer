using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TTSIDCounter : MonoBehaviour
{
    ushort nextID = 1;
    List<ushort> releasedIDs = new List<ushort>();
    
    public ushort GenerateID()
    {
        ushort newID;
        if (releasedIDs.Count != 0)
        {
            newID = releasedIDs[0];
            releasedIDs.RemoveAt(0);
        } else
        {
            newID = nextID;
            nextID++;
        }
        return newID;
    }

    public void ReleaseID(ushort id)
    {
        releasedIDs.Add(id);
    }
}
