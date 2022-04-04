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
using Unity.Networking.Transport;

namespace Konline.Scripts.Serilization
{
    public class NetworkManagerServer : GenericSingleton<NetworkManagerServer>
    {
        [SerializeField] public UDPServer Server;
        [SerializeField] private ClassStorage m_ClassStorage;
        public float m_Timer { get; private set; }

        private int m_NextAvalibleID = 10;

        public Dictionary<int, IPAddress> Clients;
        public Dictionary<int, SerializableObject> SerializableObjects;
        public Dictionary<int, SerializableObjectMono> SerializableObjectMonos;
        public List<PrefabInfo> PrefabInfos;

        private Queue<Packet> m_RecvQ;
        public override void Awake()
        {
            base.Awake();
            m_ClassStorage.Init();
            Clients = new Dictionary<int, IPAddress>();
            SerializableObjects = new Dictionary<int, SerializableObject>();
            SerializableObjectMonos = new Dictionary<int, SerializableObjectMono>();
            m_RecvQ = new Queue<Packet>();
            PrefabInfos = new List<PrefabInfo>();

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
                                Packet answer = new Packet(packet.RemoteEP, SO);
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
                                PrefabInfo prefabInfo = new PrefabInfo();

                                string prefabName = br.ReadString();
                                prefabInfo.Name = prefabName;

                                int tempID = br.ReadInt32();
                                GameObject prefab = m_ClassStorage.GiveServerPrefab(prefabName);
                                GameObject gameObject = Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity);
                                SerializableObjectMono[] SOMs = gameObject.GetComponents<SerializableObjectMono>();
                                prefabInfo.SOMs = SOMs;
                                prefabInfo.Owner = packet.RemoteEP;
                                PrefabInfos.Add(prefabInfo);

                                if (SOMs.Length > 0)
                                {
                                    foreach (NetworkConnection networkConnection in Server.Connections)
                                    {
                                        if(networkConnection == packet.RemoteEP)
                                        {
                                            Packet toSameClient = new Packet(networkConnection, SOMs, tempID);
                                            AddToSendQueue(toSameClient);
                                            continue;
                                        }
                                        Packet toOthers = new Packet(networkConnection, SOMs, 0);
                                        AddToSendQueue(toOthers);
                                    }
                                }
                                else
                                {
                                    Destroy(gameObject);
                                }
                            }
                        }
                    }
                    
                }
                else if (packet.PacketType == PacketType.Update)
                {
                    string ClassID;
                    int NetworkID;

                    using(MemoryStream ms = new MemoryStream())
                    {
                        ms.Write(packet.Payload, 0, packet.Payload.Length);
                        ms.Position = 0;

                        using(BinaryReader br = new BinaryReader(ms , Encoding.UTF8))
                        {
                            ClassID = br.ReadString();
                            NetworkID = br.ReadInt32();
                            ms.Position = 0;
                            Type type = Type.GetType(ClassID);
                            if (type.IsSubclassOf(typeof(SerializableObject)))
                            {
                                BinarySerializer.Deserialize(SerializableObjects[NetworkID], ms.ToArray());

                            }
                            else if (type.IsSubclassOf(typeof(SerializableObjectMono)))
                            {
                                BinarySerializer.Deserialize(SerializableObjectMonos[NetworkID], ms.ToArray());
                            }
                        }
                    }
                }
                else if (packet.PacketType == PacketType.Hello)
                {
                    for(int i = 0; i < PrefabInfos.Count; i++)
                    {
                        Packet answer = new Packet(packet.RemoteEP, PrefabInfos[i].SOMs, 0);
                        AddToSendQueue(answer);
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
            Server.AddToSendQueue(packet);
        }

    }

    public struct PrefabInfo
    {
        public string Name;
        public SerializableObjectMono[] SOMs;
        public NetworkConnection Owner;
    }
}
#endif