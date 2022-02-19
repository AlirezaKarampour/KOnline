#if !SERVER_BUILD
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Konline.Scripts.UDP;
using System;

namespace Konline.Scripts.Serilization {
    public partial class NetworkManagerClient : GenericSingleton<NetworkManagerClient>
    {
        
        private int m_TempID = 0;
        private Dictionary<int, SerializableObject> m_TempSOs;
        private Dictionary<int, SerializableObjectMono> m_TempSOMs;


        public Dictionary<int, SerializableObject> SerializableObjects;
        public Dictionary<int, SerializableObjectMono> SerializableObjectMonos;




        private Queue<Packet> m_RecvQ;

        public override void Awake()
        {
            base.Awake();
            Debug.Log("Client");
            
            SerializableObjects = new Dictionary<int, SerializableObject>();
            SerializableObjectMonos = new Dictionary<int, SerializableObjectMono>();

            
        }

        // Start is called before the first frame update
        void Start()
        {

            

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void GetNetworkID(SerializableObject serializableObject)
        {
            serializableObject.NetworkID = m_TempID;
            m_TempID++;
            m_TempSOs.Add(serializableObject.NetworkID, serializableObject);
        }
        public void GetNetworkID(SerializableObjectMono serializableObject)
        {
            serializableObject.NetworkID = m_TempID;
            m_TempID++;
            m_TempSOMs.Add(serializableObject.NetworkID, serializableObject);
        }

        public void TrackNetID(SerializableObject obj)
        {
            SerializableObjects.Add(obj.NetworkID, obj);
        }

        public void TrackNetID(SerializableObjectMono obj)
        {
            SerializableObjectMonos.Add(obj.NetworkID, obj);
        }

        public void AddToRecvQueue(Packet packet)
        {
            m_RecvQ.Enqueue(packet);
        }
    }
}
#endif
