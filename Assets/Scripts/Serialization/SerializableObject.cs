using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;

namespace Konline.Scripts.Serilization
{
    public abstract class SerializableObject
    {
        [Shared]
        public int NetworkID;

        [HideInInspector]
        [Shared]
        public string ClassID;



#if SERVER_BUILD
        public SerializableObject()
        {
            ClassID = this.GetType().Name;
            NetworkID = NetworkManagerServer.Instance.GiveNetworkID();
            NetworkManagerServer.Instance.TrackNetID(this);
        }
#endif


#if !SERVER_BUILD
        public SerializableObject()
        {
            ClassID = this.GetType().Name;
            NetworkManagerClient.Instance.GetNetworkID(this);
        }

        public SerializableObject(int NetID)
        {
            ClassID = this.GetType().Name;
            this.NetworkID = NetID;
            
        }
#endif

            

    }

    public abstract class SerializableObjectMono : MonoBehaviour
    {
        [Shared]
        public int NetworkID;

        [HideInInspector] 
        [Shared]
        public string ClassID;


        private void Awake()
        {
#if !SERVER_BUILD



#endif
#if SERVER_BUILD
            ClassID = this.GetType().Name;
            NetworkID = NetworkManagerServer.Instance.GiveNetworkID();
            NetworkManagerServer.Instance.TrackNetID(this);
#endif
        }


    }
}
