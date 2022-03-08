using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Threading.Tasks;
using Konline.Scripts.UDP;

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
        public string PrefabName;

        [Shared]
        public int NetworkID;

        [HideInInspector]
        [Shared]
        public string ClassID;

        


        private void Awake()
        {
#if !SERVER_BUILD

            ClassID = this.GetType().Name;
            

#endif
#if SERVER_BUILD
            ClassID = this.GetType().Name;
            NetworkID = NetworkManagerServer.Instance.GiveNetworkID();
            NetworkManagerServer.Instance.TrackNetID(this);
#endif
        }




#if !SERVER_BUILD
        private void OnEnable()
        {
            UDPClient.OnClientTick += UpdateServer;
        }

        private void OnDisable()
        {
            UDPClient.OnClientTick -= UpdateServer;
        }


        protected virtual void UpdateServer()
        {

        }
            

#endif
#if SERVER_BUILD
        private void OnEnable()
        {
            UDPServer.OnServerTick += UpdateClient;
        }
        private void OnDisable()
        {
            UDPServer.OnServerTick -= UpdateClient;
        }

        protected virtual void UpdateClient()
        {

        }



#endif
    }
}
