#if SERVER_BUILD
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using Konline.Scripts.UDP;
using Konline.Scripts.ObjectReplication;

namespace Konline.Scripts.Serilization
{
    public class NetworkManagerServer : GenericSingleton<NetworkManagerServer>
    {
        [SerializeField] private UDPServer m_Server;
        [SerializeField] private ClassStorage m_ClassStorage;

        private int m_NextAvalibleID = 10;

        public Dictionary<int, SerializableObject> SerializableObjects;
        public Dictionary<int, SerializableObjectMono> SerializableObjectMonos;

        private Queue<Packet> m_RecvQ;

        public override void Awake()
        {
            base.Awake();
            m_ClassStorage.Init();

            SerializableObjects = new Dictionary<int, SerializableObject>();
            SerializableObjectMonos = new Dictionary<int, SerializableObjectMono>();
            m_RecvQ = new Queue<Packet>();

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
                    bool isSO;
                    
                    using (MemoryStream ms = new MemoryStream())
                    {
                        ms.Write(packet.Payload, 0, packet.Payload.Length);
                        ms.Position = 0;
                        using (BinaryReader br = new BinaryReader(ms, Encoding.UTF8))
                        {
                            isSO = br.ReadBoolean();

                            if (isSO)
                            {
                                string ClassID;
                                int tempNetID;

                                ClassID = br.ReadString();
                                tempNetID = br.ReadInt32();

                                Type type = Type.GetType(ClassID);


                                
                                object obj = Activator.CreateInstance(type);
                                SerializableObject SO = (SerializableObject)obj;
                                Packet answer = new Packet(packet.RemoteEP.Address.ToString(), packet.RemoteEP.Port, SO);
                                byte[] payload;
                                using (MemoryStream ms1 = new MemoryStream())
                                {
                                    ms1.Write(answer.Payload, 0, answer.Payload.Length);
                                    using (BinaryWriter bw = new BinaryWriter(ms1, Encoding.UTF8))
                                    {
                                        bw.Write(tempNetID);
                                    }
                                    payload = ms1.ToArray();
                                }

                                answer.Payload = payload;
                                AddToSendQueue(answer);

                            }
                            else
                            {
                                string prefabName = br.ReadString();
                                GameObject prefab = m_ClassStorage.GiveServerPrefab(prefabName);
                                GameObject gameObject = Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity);
                                SerializableObjectMono[] SOMs = gameObject.GetComponents<SerializableObjectMono>();
                                if (SOMs.Length > 0)
                                {

                                    Packet answer = new Packet(packet.RemoteEP.Address.ToString(), packet.RemoteEP.Port, SOMs);
                                    AddToSendQueue(answer);
                                }
                                else
                                {
                                    Destroy(gameObject);
                                }
                            }
                        }
                    }
                    
                }


            }
        }



        public int GiveNetworkID()
        {
            int netID = m_NextAvalibleID;
            m_NextAvalibleID++;
            return netID;
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

        public void AddToSendQueue(Packet packet)
        {
            m_Server.AddToSendQueue(packet);
        }

    }
}
#endif