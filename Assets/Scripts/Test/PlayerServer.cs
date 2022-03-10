#if SERVER_BUILD
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Konline.Scripts.Serilization;
using Konline.Scripts.UDP;
using System.Net;

public partial class Player : SerializableObjectMono
{

    private void Update()
    {

        if (m_AllowMove)
        {
            transform.position += (transform.forward * Time.deltaTime);
            Position[0] = transform.position.x;
            Position[1] = transform.position.y;
            Position[2] = transform.position.z;
        }

        byte[] data = BinarySerializer.Serialize(this);
        foreach (KeyValuePair<IPAddress, int> entry in NetworkManagerServer.Instance.Clients)
        {
            Packet packet = new Packet(entry.Key.ToString(), entry.Value, data);
            NetworkManagerServer.Instance.AddToSendQueue(packet);
        }

    }
}
#endif

