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

namespace Konline.Scripts.UDP
{
    public class UDPClient : MonoBehaviour
    {
        public int TickRate = 60;
        public int Delay = 100;

        private Socket m_ClientSOCK;
        private Queue<Packet> m_SendQ;

        private bool m_IsSending = false;
        private bool m_ShouldSend = true;


        public static event Action OnClientTick;

        private void Awake()
        {
            m_SendQ = new Queue<Packet>();
            m_ClientSOCK = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            m_ClientSOCK.DontFragment = true;
        }

        // Start is called before the first frame update
        void Start()
        {
            IPEndPoint remoteIPEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 15000);
            EndPoint remoteEP = (EndPoint)remoteIPEP;
            m_ClientSOCK.Connect(remoteEP);

            StartRecvLoop();

            StartSendLoop();

        }

        // Update is called once per frame
        void Update()
        {

        }


        private void StartRecvLoop()
        {
            StateObject so = new StateObject(1500);
            so.Socket = m_ClientSOCK;
            IPEndPoint remoteIPEP = new IPEndPoint(IPAddress.Any, 0);
            so.UDP_RemoteEP = (EndPoint)remoteIPEP;
            m_ClientSOCK.BeginReceiveFrom(so.buffer, 0, so.buffer.Length, SocketFlags.None, ref so.UDP_RemoteEP, RecvCB, so);
        }

        private void RecvCB(IAsyncResult ar)
        {
            StateObject so = (StateObject)ar.AsyncState;
            Socket clientSOCK = so.Socket;
            int len = clientSOCK.EndReceiveFrom(ar, ref so.UDP_RemoteEP);

            if (len > 0)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    ms.Write(so.buffer, 0, len);
                    ms.Position = 0;
                    byte[] data = new byte[len];
                    ms.Read(data, 0, data.Length);
                    Packet packet = new Packet();
                    if (so.UDP_RemoteEP is IPEndPoint)
                    {
                        packet.RemoteEP = (IPEndPoint)so.UDP_RemoteEP;
                    }
                    Packet.BytesToPacket(packet, data);

                    NetworkManagerClient.Instance.AddToRecvQueue(packet);

                }
            }

            clientSOCK.BeginReceiveFrom(so.buffer, 0, so.buffer.Length, SocketFlags.None, ref so.UDP_RemoteEP, RecvCB, so);
        }


        private async void StartSendLoop()
        {
            float timer = 0f;
            while (m_ShouldSend)
            {
                timer += Time.deltaTime * 1000;

                if (timer >= (1000 / TickRate))
                {
                    OnClientTick?.Invoke();

                    if (m_SendQ.Count > 0)
                    {
                        if (m_IsSending == false)
                        {
                            StateObject so = MakeStateObject(m_ClientSOCK);

                            await Task.Delay(Delay);

                            m_ClientSOCK.BeginSendTo(so.buffer, 0, so.buffer.Length, SocketFlags.None, so.UDP_RemoteEP, SendCB, so);
                            m_IsSending = true;
                            timer = 0;
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
        }

        private void SendCB(IAsyncResult ar)
        {
            StateObject so = (StateObject)ar.AsyncState;
            Socket clientSOCK = so.Socket;
            clientSOCK.EndSendTo(ar);
            m_IsSending = false;
        }

        public void AddToSendQueue(Packet Packet)
        {
            m_SendQ.Enqueue(Packet);
        }

        private StateObject MakeStateObject(Socket soSocket)
        {
            if (m_SendQ.Count > 0)
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

        private void OnDestroy()
        {
            m_ClientSOCK.Close();
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