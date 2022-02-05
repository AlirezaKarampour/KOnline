using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
using System.IO;
using System.Text;

namespace Konline.Scripts.UDP
{
    public class UDPServer : MonoBehaviour
    {
        private Socket m_ServerSOCK;

        private void Awake()
        {
            m_ServerSOCK = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            IPEndPoint localIPEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 15000);
            EndPoint localEP = (EndPoint)localIPEP;

            m_ServerSOCK.Bind(localEP);
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
                    string msg = Encoding.UTF8.GetString(data);

                    Debug.Log(msg);
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
