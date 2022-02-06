using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Konline.Scripts.Serilization
{
    public abstract class SerializableObject
    {
        [Shared]
        public int NetworkID;

        [Shared]
        public string ClassID;
        
        [Shared]
        public int ClientID;

        public SerializableObject()
        {
#if !SERVER_BUILD
            NetworkID = NetworkManagerClient.Instance.GiveNetworkID();
            NetworkManagerClient.Instance.TrackNetID(this);
            

            ClassID = this.GetType().Name;
#endif

#if SERVER_BUILD


            ClassID = this.GetType().Name;
#endif
        }
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
            NetworkID = NetworkManagerClient.Instance.GiveNetworkID();
            NetworkManagerClient.Instance.TrackNetID(this);
            

            ClassID = this.GetType().Name;
#endif
#if SERVER_BUILD



            ClassID = this.GetType().Name;
#endif
        }


    }
}
