#if SERVER_BUILD
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Konline.Scripts.Serilization;

public partial class Player : SerializableObjectMono
{

    private void Update()
    {
        if (m_AllowMove)
        {
            transform.position += (transform.forward * Time.deltaTime * 1);
            m_Position[0] = transform.position.x;
            m_Position[1] = transform.position.y;
            m_Position[2] = transform.position.z;
        }
    }
}
#endif

