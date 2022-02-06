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

        [HideInInspector]
        [Shared]
        public string ClassID;

#if SERVER_BUILD
        public ClientManagerServer Client;
#endif


#if !SERVER_BUILD
        public SerializableObject()
        {
            NetworkID = NetworkManagerClient.Instance.GiveNetworkID();
            NetworkManagerClient.Instance.TrackNetID(this);
            

            ClassID = this.GetType().Name;
        }
#endif

#if SERVER_BUILD
        public SerializableObject(ClientManagerServer client)
        {
            this.Client = client;
            NetworkID = Client.GiveNetworkID();
            Client.TrackNetID(this);

            ClassID = this.GetType().Name;
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
        public ClientManagerServer Client;
#endif
        private void Awake()
        {
#if !SERVER_BUILD
            NetworkID = NetworkManagerClient.Instance.GiveNetworkID();
            NetworkManagerClient.Instance.TrackNetID(this);
            

            ClassID = this.GetType().Name;
#endif
#if SERVER_BUILD

            NetworkID = Client.GiveNetworkID();
            Client.TrackNetID(this);

            ClassID = this.GetType().Name;
#endif
        }


    }
}
