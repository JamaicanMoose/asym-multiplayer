using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TTSIDCounter : MonoBehaviour
{
    ushort nextID = 1;
    
    public ushort GenerateID()
    {
        ushort newID = nextID;
        nextID++;
        return newID;
    }
}
