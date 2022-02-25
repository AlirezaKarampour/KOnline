using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using UnityEngine;
using System.Net;
using System.IO;
using Konline.Scripts.Serilization;

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

        //Create
        public Packet(string address, int port, SerializableObject SO)
        {
            this.PacketType = PacketType.Create;
            IPEndPoint EP = new IPEndPoint(IPAddress.Parse(address), port);
            this.RemoteEP = EP;
            byte[] payload;
            bool isSO = true;

            using(MemoryStream ms = new MemoryStream())
            {
                using(BinaryWriter bw = new BinaryWriter(ms , Encoding.UTF8))
                {
                    bw.Write(isSO);
                    bw.Write(SO.ClassID);
                    bw.Write(SO.NetworkID);
                }
                payload = ms.ToArray();
            }
            this.Payload = payload;
        }

        public Packet(string address, int port, string prefabName)
        {
            this.PacketType = PacketType.Create;
            IPEndPoint EP = new IPEndPoint(IPAddress.Parse(address), port);
            this.RemoteEP = EP;
            byte[] payload;
            bool isSO = false;
            
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms, Encoding.UTF8))
                {
                    bw.Write(isSO);
                    bw.Write(prefabName);
                }
                payload = ms.ToArray();
            }
            this.Payload = payload;
            
        }

        public Packet(string address, int port, SerializableObjectMono[] SOMs)
        {
            this.PacketType = PacketType.Create;
            IPEndPoint EP = new IPEndPoint(IPAddress.Parse(address), port);
            this.RemoteEP = EP;
            byte[] payload;
            bool isSO = false;

            using(MemoryStream ms = new MemoryStream())
            {
                using(BinaryWriter bw = new BinaryWriter(ms, Encoding.UTF8))
                {
                    bw.Write(isSO);
                    bw.Write(SOMs[0].PrefabName);
                    foreach(SerializableObjectMono obj in SOMs)
                    {
                        bw.Write(obj.NetworkID);
                    }
                }
                payload = ms.ToArray();
            }
            this.Payload = payload;
        }



        //Update
        public Packet(string address, int port, byte[] payload)
        {
            this.PacketType = PacketType.Update;
            this.Payload = payload;
            IPEndPoint EP = new IPEndPoint(IPAddress.Parse(address), port);
            this.RemoteEP = EP;
        }

        public static byte[] PacketToBytes(Packet packet)
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

        


        public static void BytesToPacket(Packet packet , byte[] data)
        {
            using(MemoryStream ms = new MemoryStream())
            {
                ms.Write(data, 0, data.Length);
                ms.Position = 0;
                using(BinaryReader br = new BinaryReader(ms))
                {
                    PacketType packetType = (PacketType)br.ReadInt32();
                    packet.PacketType = packetType;
                    byte[] payload = br.ReadBytes((int)(ms.Length - sizeof(PacketType)));
                    packet.Payload = payload;
                }
            }

        }

    }
}
