﻿/// <author> Anish Bhagavatula and Mohammad Umar Sultan </author>
/// <summary>
/// This file contains all the global constants used by testing files
/// </summary>

using PlexShareNetwork.Queues;
using PlexShareNetwork.Serialization;
using System;
using System.Dynamic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace PlexShareNetwork
{
	public static class NetworkTestGlobals
	{
		public const string dashboardName = "Dashboard";

		// Priorities of each module (true is for high priority)
		public const bool dashboardPriority = true;

		// Used to generate random strings
		private static readonly Random random = new();

        private static readonly Serializer _serializer = new();

        /// <summary>
        /// Returns a randomly generated alphanumeric string
        /// </summary>
        public static string RandomString(int length)
		{
			const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
			return new string(Enumerable.Repeat(chars, length)
			.Select(s => s[random.Next(s.Length)]).ToArray());
		}

        public static void SendPacket(Packet packet, TcpClient socket)
        {
            string sendString = "BEGIN" + _serializer.Serialize(packet) + "END";
            byte[] bytes = Encoding.ASCII.GetBytes(sendString);
            socket.Client.Send(bytes);
        }

        public static void AssertSinglePacketReceive(Packet sendPacket, ReceivingQueue receivingQueue)
        {
            while (receivingQueue.Size() < 1)
            {
                Thread.Sleep(100);
            }
            Assert.True(receivingQueue.Size() == 1);

            Packet receivedPacket = receivingQueue.Dequeue();
            AssertPacketEquality(sendPacket, receivedPacket);
        }

        public static void AssertTenPacketsReceive(Packet[] sendPackets, ReceivingQueue receivingQueue)
        {
            while (receivingQueue.Size() < 10)
            {
                Thread.Sleep(100);
            }
            Assert.True(receivingQueue.Size() == 10);

            for (var i = 0; i < 10; i++)
            {
                Packet receivedPacket = receivingQueue.Dequeue();
                AssertPacketEquality(sendPackets[i], receivedPacket);
            }
        }

        public static void AssertPacketEquality(Packet packet1, Packet packet2)
        {
            Assert.Equal(packet1.serializedData, packet2.serializedData);
            Assert.Equal(packet1.destination, packet2.destination);
            Assert.Equal(packet1.moduleOfPacket, packet2.moduleOfPacket);
        }
	}

	/// <summary>
	/// An implementation of the notification handler.
	/// </summary>
	public class TestNotificationHandler : INotificationHandler
	{
		public string? Data = null;
		public string? Event = null;
        public string? ClientID = null;
        public TcpClient? Socket = null;

		public void OnDataReceived(string data)
		{
			Event = "OnDataReceived";
			Data = data;
		}

		public void OnClientJoined(TcpClient socket)
		{
			Event = "OnClientJoined";
			Socket = socket;
		}

		public void OnClientLeft(string clientId)
		{
			Event = "OnClientLeft";
			ClientID = clientId;
		}

		public void WaitForEvent()
		{
            while (Event == null)
            {
                Thread.Sleep(100);
            }
        }

		public void Reset()
		{
			Event = null;
			Data = null;
            Socket = null;
            ClientID = null;
		}
	}
}
