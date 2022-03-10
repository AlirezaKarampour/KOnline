#if !SERVER_BUILD
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using Konline.Scripts.UDP;
using Konline.Scripts.ObjectReplication;
using System;
using System.Threading.Tasks;

namespace Konline.Scripts.Serilization {
    public partial class NetworkManagerClient : GenericSingleton<NetworkManagerClient>
    {
        [SerializeField] private ClassStorage m_ClassStorage;
        [SerializeField] private UDPClient m_Client;

        public const string SERVER_ADDR = "127.0.0.1";
        public const int SERVER_PORT = 15000;

        private int m_TempID = 20;
        private Dictionary<int, SerializableObject> m_TempSOs;
        private Dictionary<int, GameObject> m_TempPrefabs;


        public Dictionary<int, SerializableObject> SerializableObjects;
        public Dictionary<int, SerializableObjectMono> SerializableObjectMonos;


        


        private Queue<Packet> m_RecvQ;

        public override void Awake()
        {
            base.Awake();
            Debug.Log("Client");
            m_ClassStorage.Init();

            m_RecvQ = new Queue<Packet>();
            m_TempSOs = new Dictionary<int, SerializableObject>();
            SerializableObjects = new Dictionary<int, SerializableObject>();
            SerializableObjectMonos = new Dictionary<int, SerializableObjectMono>();
            m_TempPrefabs = new Dictionary<int, GameObject>();

            
        }

        // Start is called before the first frame update
        async void Start()
        {
            Packet packet = new Packet(SERVER_ADDR, SERVER_PORT);
            AddToSendQueue(packet);

            GameObject obj = await SendCreateRequest("Player");
            Player player = obj.GetComponent<Player>();
            if(player != null)
            {
                Debug.Log(player.NetworkID);
            }
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
                                int NetID;
                                int tempNetID;

                                ClassID = br.ReadString();
                                NetID = br.ReadInt32();
                                tempNetID = br.ReadInt32();

                                if (tempNetID != 0)
                                {
                                    SerializableObject SO = m_TempSOs[tempNetID];
                                    SO.NetworkID = NetID;
                                    TrackNetID(SO);
                                    Debug.Log(SO.NetworkID);
                                }
                                else
                                {
                                    Type type = Type.GetType(ClassID);
                                    object obj = Activator.CreateInstance(type, new object[] { NetID });
                                    SerializableObject SO = (SerializableObject)obj;
                                    TrackNetID(SO);
                                }
                            }
                            else
                            {
                                string prefabName = br.ReadString();
                                int tempID = br.ReadInt32();
                                GameObject prefab = m_ClassStorage.GiveClientPrefab(prefabName);
                                GameObject gameObject = Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity);
                                SerializableObjectMono[] SOMs = gameObject.GetComponents<SerializableObjectMono>();
                                if(SOMs.Length > 0)
                                {
                                    foreach(SerializableObjectMono SOM in SOMs)
                                    {
                                        SOM.NetworkID = br.ReadInt32();
                                        Debug.Log(SOM.NetworkID);
                                        TrackNetID(SOM);
                                    }
                                    m_TempPrefabs.Add(tempID, gameObject);
                                }
                                else
                                {
                                    Destroy(gameObject);
                                }
                                
                                
                            }
                        }
                    }
                }
                else if(packet.PacketType == PacketType.Update)
                {
                    string ClassID;
                    int NetworkID;

                    using (MemoryStream ms = new MemoryStream())
                    {
                        ms.Write(packet.Payload, 0, packet.Payload.Length);
                        ms.Position = 0;

                        using (BinaryReader br = new BinaryReader(ms, Encoding.UTF8))
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

        public int GetTempID()
        {
            int id = m_TempID;
            m_TempID++;
            return id;
        }

        //needs to be compeleted!
        //public void GetNetworkID(SerializableObjectMono serializableObject)
        //{
        //    serializableObject.NetworkID = m_TempID;
        //    m_TempID++;
        //    m_TempSOMs.Add(serializableObject.NetworkID, serializableObject);

        //    Packet packet = new Packet(SERVER_ADDR, SERVER_PORT, serializableObject);
        //    m_Client.AddToSendQueue(packet);

        //}

        public async Task<GameObject> SendCreateRequest(string prefabName)
        {
            int tempID = GetTempID();

            if (m_ClassStorage.HasPrefab(prefabName))
            {
                Packet packet = new Packet(SERVER_ADDR, SERVER_PORT, prefabName , tempID);
                m_Client.AddToSendQueue(packet);
            }
            else
            {
                Debug.LogError("prefab doesn't exist");
            }

            while(m_TempPrefabs.ContainsKey(tempID) == false)
            {
                await Task.Yield();
            }

            return m_TempPrefabs[tempID];
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
            m_Client.AddToSendQueue(packet);
        }
    }
}
#endif
