#if !SERVER_BUILD
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using Konline.Scripts.UDP;
using System;

namespace Konline.Scripts.Serilization {
    public partial class NetworkManagerClient : GenericSingleton<NetworkManagerClient>
    {
        [SerializeField] private UDPClient m_Client;

        public const string SERVER_ADDR = "127.0.0.1";
        public const int SERVER_PORT = 15000;

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

            m_RecvQ = new Queue<Packet>();
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
            AnalyzePacket();
        }

        private void AnalyzePacket()
        {
            while(m_RecvQ.Count > 0)
            {
                Packet packet = m_RecvQ.Dequeue();
                if(packet.PacketType == PacketType.Create)
                {
                    string ClassID;
                    int NetID;
                    int tempNetID;
                    using(MemoryStream ms = new MemoryStream())
                    {
                        ms.Write(packet.Payload, 0, packet.Payload.Length);
                        ms.Position = 0;
                        using(BinaryReader br = new BinaryReader(ms , Encoding.UTF8))
                        {
                            ClassID = br.ReadString();
                            NetID = br.ReadInt32();
                            tempNetID = br.ReadInt32();
                        }
                    }
                    SerializableObject SO = m_TempSOs[tempNetID];
                    SO.NetworkID = NetID;
                    TrackNetID(SO);
                }
            }
        }



        public void GetNetworkID(SerializableObject serializableObject)
        {
            serializableObject.NetworkID = m_TempID;
            m_TempID++;
            m_TempSOs.Add(serializableObject.NetworkID, serializableObject);

            Packet packet = new Packet(SERVER_ADDR, SERVER_PORT, serializableObject);
            m_Client.AddToSendQueue(packet);

        }

        //needs to be compeleted!
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
