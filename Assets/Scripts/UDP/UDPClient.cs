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
    public class UDPClient : MonoBehaviour
    {
        private Socket m_ClientSOCK;

        private void Awake()
        {
            m_ClientSOCK = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            m_ClientSOCK.DontFragment = true;
        }

        // Start is called before the first frame update
        void Start()
        {
            IPEndPoint remoteIPEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 15000);
            EndPoint remoteEP = (EndPoint)remoteIPEP;
            m_ClientSOCK.Connect(remoteEP);

            byte[] data = Encoding.UTF8.GetBytes("alireza");

            m_ClientSOCK.Send(data);

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
