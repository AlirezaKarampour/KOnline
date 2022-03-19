#if !SERVER_BUILD
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Konline.Scripts.Serilization;
using Unity.Networking.Transport;
using Unity.Collections;

namespace Konline.Scripts.UDP
{
    public class UDPClient : MonoBehaviour
    {
        private NetworkDriver m_Client;
        public NetworkConnection Connection;

        private Queue<Packet> m_SendQ;

        private bool m_IsDone = false;
        private bool m_AllowSend = false;

        private void Awake()
        {
            m_SendQ = new Queue<Packet>();
            m_Client = NetworkDriver.Create();
            Connection = default(NetworkConnection);

            NetworkEndPoint endPoint = NetworkEndPoint.LoopbackIpv4;
            endPoint.Port = 15000;
            Connection = m_Client.Connect(endPoint);
        }

        private void Start()
        {

            
        }

        private void Update()
        {
            m_Client.ScheduleUpdate().Complete();

            if (!Connection.IsCreated)
            {
                if (!m_IsDone)
                {
                    print("Clouldn't connent to server trying again!");
                    return;
                }
            }

            DataStreamReader reader;
            NetworkEvent.Type type;

            

            while((type = Connection.PopEvent(m_Client, out reader))!= NetworkEvent.Type.Empty)
            {
                if(type == NetworkEvent.Type.Connect)
                {
                    print("Connected to server");
                    m_AllowSend = true;
                }
                else if(type == NetworkEvent.Type.Data)
                {
                    int length = reader.ReadInt();
                    NativeArray<byte> data = new NativeArray<byte>(length, Allocator.TempJob);
                    reader.ReadBytes(data);
                    byte[] data1 = data.ToArray();
                    data.Dispose();
                    Packet packet = new Packet();
                    packet.RemoteEP = Connection;
                    Packet.BytesToPacket(packet, data1);
                    NetworkManagerClient.Instance.AddToRecvQueue(packet);
                }
                else if (type == NetworkEvent.Type.Disconnect)
                {
                    print("Client got disconnected from server");
                    Connection = default(NetworkConnection);
                    m_AllowSend = false;
                }

            }

            if (m_AllowSend)
            {
                while (m_SendQ.Count > 0)
                {
                    Packet packet = m_SendQ.Dequeue();
                    byte[] data = Packet.PacketToBytes(packet);
                    m_Client.BeginSend(packet.RemoteEP, out var writer);
                    NativeArray<byte> data1 = new NativeArray<byte>(data, Allocator.Temp);
                    writer.WriteBytes(data1);
                    m_Client.EndSend(writer);
                }
            }

        }



        public void AddToSendQueue(Packet Packet)
        {
            m_SendQ.Enqueue(Packet);
        }

        

        private void OnDestroy()
        {
            m_Client.Dispose();
        }
    }

    
}
#endif