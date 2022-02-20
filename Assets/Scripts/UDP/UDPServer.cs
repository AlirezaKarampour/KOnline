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

namespace Konline.Scripts.UDP
{
    public class UDPServer : MonoBehaviour
    {
        private Socket m_ServerSOCK;
        private Queue<Packet> m_SendQ;

        private bool m_IsSending = false;
        private bool m_ShouldSend = true;
        private void Awake()
        {
            m_ServerSOCK = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            m_ServerSOCK.DontFragment = true;

            IPEndPoint localIPEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 15000);
            EndPoint localEP = (EndPoint)localIPEP;

            m_ServerSOCK.Bind(localEP);


            m_SendQ = new Queue<Packet>();
        }

        // Start is called before the first frame update
        void Start()
        {
            StateObject so = new StateObject(1500);
            so.Socket = m_ServerSOCK;
            IPEndPoint remoteIPEP = new IPEndPoint(IPAddress.Any, 0);
            so.UDP_RemoteEP = (EndPoint)remoteIPEP;

            m_ServerSOCK.BeginReceiveFrom(so.buffer, 0, so.buffer.Length, SocketFlags.None, ref so.UDP_RemoteEP, RecvCB, so);
            Debug.Log("server running on port: 1500...");

            SendLoop();
            



        }

        private void SendCB(IAsyncResult ar)
        {
            StateObject so = (StateObject)ar.AsyncState;
            Socket serverSOCK = so.Socket;
            serverSOCK.EndSendTo(ar);
            m_IsSending = false;
        }

        private async void SendLoop()
        {
            while (m_ShouldSend)
            {
                if(m_SendQ.Count > 0)
                {
                    if(m_IsSending == false)
                    {
                        StateObject so = MakeStateObject(m_ServerSOCK);
                        m_ServerSOCK.BeginSendTo(so.buffer, 0, so.buffer.Length, SocketFlags.None, so.UDP_RemoteEP, SendCB, so);
                        m_IsSending = true;
                    }
                    else
                    {
                        await Task.Yield();
                    }
                }
                else
                {
                    await Task.Yield();
                }
            }
        } 



        private void RecvCB(IAsyncResult ar)
        {
            StateObject so = (StateObject)ar.AsyncState;
            Socket serverSOCK = so.Socket;
            int len = serverSOCK.EndReceiveFrom(ar, ref so.UDP_RemoteEP);

            if (len > 0)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    ms.Write(so.buffer, 0, len);
                    ms.Position = 0;
                    byte[] data = new byte[len];
                    ms.Read(data, 0, data.Length);
                    Packet packet = new Packet();
                    if(so.UDP_RemoteEP is IPEndPoint)
                    {
                        packet.RemoteEP = (IPEndPoint)so.UDP_RemoteEP;
                    }
                    Packet.BytesToPacket(packet, data);

                    NetworkManagerServer.Instance.AddToRecvQueue(packet);

                }
            }

            serverSOCK.BeginReceiveFrom(so.buffer, 0, so.buffer.Length, SocketFlags.None, ref so.UDP_RemoteEP, RecvCB, so);

        }

        // Update is called once per frame
        void Update()
        {

        }

        private void OnDestroy()
        {

            m_ServerSOCK.Close();
        }

        public void AddToSendQueue(Packet packet)
        {
            m_SendQ.Enqueue(packet);
        }

        private StateObject MakeStateObject(Socket soSocket)
        {
            if(m_SendQ.Count > 0)
            {
                Packet packet = m_SendQ.Dequeue();
                byte[] data = Packet.PacketToBytes(packet);
                StateObject so = new StateObject(1500);
                so.Socket = soSocket;
                so.buffer = data;
                so.UDP_RemoteEP = packet.RemoteEP;
                return so;
            }
            return null;
        }
    }

    public class StateObject
    {
        public StateObject(int bufferSize)
        {
            buffer = new byte[bufferSize];

        }

        public Socket Socket;
        public byte[] buffer;
        public EndPoint UDP_RemoteEP;
    }
}
#endif