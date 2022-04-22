#if SERVER_BUILD
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
    public class UDPServer : MonoBehaviour
    {
        private NetworkDriver m_Server;
        public NativeList<NetworkConnection> Connections;

        private Queue<Packet> m_SendQ;


        private void Start()
        {
            m_SendQ = new Queue<Packet>();

            m_Server = NetworkDriver.Create();
            NetworkEndPoint endPoint = NetworkEndPoint.AnyIpv4;
            endPoint.Port = 15000;
            if(m_Server.Bind(endPoint) != 0)
            {
                print("Failed to bind to 15000");
            }
            else
            {
                m_Server.Listen();
            }

            Connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);
        }

        private void Update()
        {
            m_Server.ScheduleUpdate().Complete();

            for (int i = 0; i < Connections.Length; i++)
            {
                if (!Connections[i].IsCreated)
                {
                    Connections.RemoveAtSwapBack(i);
                    --i;
                }
            }

            NetworkConnection c;
            while ((c = m_Server.Accept()) != default(NetworkConnection))
            {
                Connections.Add(c);
                Debug.Log("Accepted a connection");
            }

            DataStreamReader stream;
            for(int i = 0; i < Connections.Length; i++)
            {
                if (!Connections[i].IsCreated)
                {
                    continue;
                }

                NetworkEvent.Type cmd;
                while((cmd = m_Server.PopEventForConnection(Connections[i] , out stream)) != NetworkEvent.Type.Empty)
                {
                    if(cmd == NetworkEvent.Type.Data)
                    {
                        int length = stream.ReadInt();
                        NativeArray<byte> data = new NativeArray<byte>(length, Allocator.TempJob);
                        stream.ReadBytes(data);
                        byte[] data1 = data.ToArray();
                        data.Dispose();
                        Packet packet = new Packet();
                        packet.RemoteEP = Connections[i];
                        Packet.BytesToPacket(packet, data1);
                        NetworkManagerServer.Instance.AddToRecvQueue(packet);
                        
                    }
                    else if ( cmd == NetworkEvent.Type.Disconnect)
                    {
                        Debug.Log("Client disconnected from server");
                        Connections[i] = default(NetworkConnection);
                    }
                }
            }

            while(m_SendQ.Count > 0)
            {
                Packet packet = m_SendQ.Dequeue();
                byte[] data = Packet.PacketToBytes(packet);
                m_Server.BeginSend(packet.RemoteEP, out var writer);
                NativeArray<byte> data1 = new NativeArray<byte>(data, Allocator.TempJob);
                writer.WriteBytes(data1);
                m_Server.EndSend(writer);
                data1.Dispose();
            }

        }
        public void AddToSendQueue(Packet packet)
        {
            m_SendQ.Enqueue(packet);
        }


        private void OnDestroy()
        {
            m_Server.Dispose();
            Connections.Dispose();
        }
    }

   
}
#endif