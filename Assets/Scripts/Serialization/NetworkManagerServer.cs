#if SERVER_BUILD
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;

namespace Konline.Scripts.Serilization
{
    public class NetworkManagerServer : GenericSingleton<NetworkManagerServer>
    {

        public Dictionary<string, ClientManagerServer> Clients;

        public override void Awake()
        {
            base.Awake();
            Debug.Log("server");


        }



        public void AddClient(IPEndPoint endPoint , ClientManagerServer clientManager)
        {
            string clientAddrr = endPoint.ToString();
            Clients.Add(clientAddrr, clientManager);
        }

        public ClientManagerServer FindClient(IPEndPoint clientEndPoint)
        {
            string clientAddrr = clientEndPoint.ToString();
            ClientManagerServer clientM = Clients[clientAddrr];

            return clientM;
        }
    }
}
#endif