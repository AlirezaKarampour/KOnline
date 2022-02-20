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
            NetworkManagerServer.Instance.GiveNetworkID();
            NetworkManagerServer.Instance.TrackNetID(this);
        }
#endif


#if !SERVER_BUILD
        public SerializableObject()
        {
            ClassID = this.GetType().Name;
            NetworkManagerClient.Instance.GetNetworkID(this);
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

#if SERVER_BUILD
       


#endif
        private void Awake()
        {
#if !SERVER_BUILD
            NetworkManagerClient.Instance.GetNetworkID(this);
            
            

            ClassID = this.GetType().Name;
#endif
#if SERVER_BUILD

            


#endif
        }


    }
}
