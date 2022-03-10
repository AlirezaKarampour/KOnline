#if SERVER_BUILD
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Konline.Scripts.Serilization;

public partial class Player : SerializableObjectMono
{

    private void Update()
    {
        Debug.Log(m_AllowMove);

    }
}
#endif

