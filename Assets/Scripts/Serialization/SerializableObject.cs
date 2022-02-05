using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Konline.Scripts.Serilization
{
    public abstract class SerializableObject
    {
        public int NetworkID;
        public string ClassID;

        public SerializableObject()
        {
            NetworkID = NetworkManager.Instance.GiveNetworkID();
            NetworkManager.Instance.TrackNetID(this);
            Debug.Log(NetworkID);

            ClassID = this.GetType().Name;
        }
    }
}
