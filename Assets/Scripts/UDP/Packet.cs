using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.IO;

namespace Konline.Scripts.UDP
{
    public enum PacketType
    {
        Hello,
        Create,
        Update,
        Destroy,
    }

    public class Packet
    {
        public IPEndPoint RemoteEP;
        public PacketType PacketType;
        public byte[] Payload;

        public Packet()
        {

        }
        public Packet(PacketType packetType , byte[] payload)
        {
            this.PacketType = packetType;
            this.Payload = payload;
        }

        public static byte[] Pack(Packet packet)
        {
            using(MemoryStream ms = new MemoryStream())
            {
                using(BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write((int)packet.PacketType);
                    bw.Write(packet.Payload);
                }
                return ms.ToArray();
            }
        }


        public static void UnPack(Packet packet , byte[] data)
        {
            using(MemoryStream ms = new MemoryStream())
            {
                ms.Write(data, 0, data.Length);
                ms.Position = 0;
                using(BinaryReader br = new BinaryReader(ms))
                {
                    PacketType packetType = (PacketType)br.ReadInt32();
                    byte[] payload = br.ReadBytes((int)(ms.Length - sizeof(PacketType)));
                    packet.PacketType = packetType;
                    packet.Payload = payload;
                    
                }
            }

        }

    }
}
