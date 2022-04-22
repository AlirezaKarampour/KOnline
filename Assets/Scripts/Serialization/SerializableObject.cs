using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Threading.Tasks;
using Konline.Scripts.UDP;
using Unity.Networking.Transport;

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

        public virtual void UpdateClient()
        {
            byte[] data = BinarySerializer.Serialize(this);
            foreach (NetworkConnection networkConnection in NetworkManagerServer.Instance.Server.Connections)
            {
                Packet packet = new Packet(networkConnection, data);
                NetworkManagerServer.Instance.AddToSendQueue(packet);
            }
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

        public virtual void UpdateServer()
        {
            byte[] data = BinarySerializer.Serialize(this);
            Packet packet = new Packet(NetworkManagerClient.Instance.Client.Connection, data);
            NetworkManagerClient.Instance.AddToSendQueue(packet);
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

#if !SERVER_BUILD
        public bool IsOwner;
#endif

        protected virtual void Awake()
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
        
        protected virtual void UpdateServer()
        {
            byte[] data = BinarySerializer.Serialize(this);
            Packet packet = new Packet(NetworkManagerClient.Instance.Client.Connection, data);
            NetworkManagerClient.Instance.AddToSendQueue(packet);
        }
            

#endif
#if SERVER_BUILD
        
        protected virtual void UpdateClient()
        {
            byte[] data = BinarySerializer.Serialize(this);
            foreach (NetworkConnection networkConnection in NetworkManagerServer.Instance.Server.Connections)
            {
                Packet packet = new Packet(networkConnection, data);
                NetworkManagerServer.Instance.AddToSendQueue(packet);
            }
        }
       



#endif
    }
}
