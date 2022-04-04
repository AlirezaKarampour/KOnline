using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using UnityEngine;
using System.Net;
using System.IO;
using Konline.Scripts.Serilization;
using Unity.Networking.Transport;

namespace Konline.Scripts.UDP
{
    public enum PacketType
    {
        Hello,
        Create,
        Update,
        Destroy,
        ACK,
    }

    public class Packet
    {
        public NetworkConnection RemoteEP;
        public PacketType PacketType;
        public byte[] Payload;

        public Packet()
        {
        }
        //hello
        public Packet(NetworkConnection RemoteEP)
        {
            this.RemoteEP = RemoteEP;
            this.PacketType = PacketType.Hello;
            this.Payload = new byte[1];
        }


        //Create
        public Packet(NetworkConnection RemoteEP, SerializableObject SO)
        {
            this.PacketType = PacketType.Create;
            this.RemoteEP = RemoteEP;
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

        
        public Packet(NetworkConnection RemoteEP, string prefabName , int tempID)
        {
            this.PacketType = PacketType.Create;
            
            this.RemoteEP = RemoteEP;
            byte[] payload;
            bool isSO = false;
            
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms, Encoding.UTF8))
                {
                    bw.Write(isSO);
                    bw.Write(prefabName);
                    bw.Write(tempID);
                }
                payload = ms.ToArray();
            }
            this.Payload = payload;
            
        }

        public Packet(NetworkConnection RemoteEP, SerializableObjectMono[] SOMs , int tempID)
        {
            this.PacketType = PacketType.Create;
            
            this.RemoteEP = RemoteEP;
            byte[] payload;
            bool isSO = false;

            using(MemoryStream ms = new MemoryStream())
            {
                using(BinaryWriter bw = new BinaryWriter(ms, Encoding.UTF8))
                {
                    bw.Write(isSO);
                    bw.Write(SOMs[0].PrefabName);
                    bw.Write(tempID);
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
        public Packet(NetworkConnection RemoteEP, byte[] payload)
        {
            this.PacketType = PacketType.Update;
            this.Payload = payload;
            
            this.RemoteEP = RemoteEP;
        }

       
        

        public static byte[] PacketToBytes(Packet packet)
        {
            using(MemoryStream ms = new MemoryStream())
            {
                using(BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(sizeof(int) + packet.Payload.Length);
                    bw.Write((int)packet.PacketType);
                    if (packet.Payload != null)
                    {
                        bw.Write(packet.Payload);
                    }
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
