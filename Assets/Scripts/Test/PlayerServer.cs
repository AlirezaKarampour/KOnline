#if SERVER_BUILD
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Konline.Scripts.Serilization;
using Konline.Scripts.UDP;
using System.Net;
using Unity.Networking.Transport;

public partial class Player : SerializableObjectMono
{

    private void Update()
    {

        if (m_AllowMove)
        {
            transform.position += (transform.forward * Time.deltaTime);
        }

        Position[0] = transform.position.x;
        Position[1] = transform.position.y;
        Position[2] = transform.position.z;


        UpdateClient();

    }
}
#endif

