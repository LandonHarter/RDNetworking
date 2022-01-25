using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Networking.Client
{

    public static class Client
    {

        public static int RetryConnectCount { get; set; } = 3;
        private static int failIndex = 0;

        public static readonly int BufferSize = 4096;
        
        // Client properties
        public static string ConnectedIP { get; private set; }
        public static int ConnectedPort { get; private set; }
        public static bool IsConnected { get; private set; }
        public static int ClientID { get; private set; }
        public static ClientRole ClientRole { get; private set; }

        private static TCP tcp;
        private static UDP udp;

        public delegate void PacketHandler(Packet packet);
        public static Dictionary<int, PacketHandler> PacketHandlers { get; private set; }

        public static void Connect(string ip = "127.0.0.1" /* Local address */, int port = 5000) {
            ConnectedIP = ip;
            ConnectedPort = port;

            InitializeClientData();
            
            new Thread(Update).Start();

            tcp = new TCP();
            udp = new UDP();
            
            tcp.Connect();
        }

        private static void Update() {
            while (IsConnected) {
                ThreadManager.Update();
            }
        }

        public static void Disconnect() {
            tcp.Disconnect();
            IsConnected = false;
        }

        public static void SendTCP(Packet packet) {
            packet.WriteLength();
            
            tcp.SendData(packet);
        }

        public static void CreateListener(int packetID, PacketHandler callback) {
            if (PacketHandlers.ContainsKey(packetID)) {
                Console.WriteLine("Failed to create packet listener. Packet ID is already in use.", ConsoleColor.Red);
                return;
            }
            
            PacketHandlers.Add(packetID, callback);
        }

        public static void RemoveListener(int packetID) {
            if (!PacketHandlers.ContainsKey(packetID)) {
                Console.WriteLine($"Failed to remove packet listener. There is no listener with the ID: {packetID}");
                return;
            }

            PacketHandlers.Remove(packetID);
        }

        public static void ReplaceListener(int packetID, PacketHandler packetHandler) {
            if (!PacketHandlers.ContainsKey(packetID)) {
                Console.WriteLine($"Failed to replace packet listener. There is no listener with the ID: {packetID}");
                return;
            }
            
            RemoveListener(packetID);
            CreateListener(packetID, packetHandler);
        }

        private static void InitializeClientData() {
            PacketHandlers = new Dictionary<int, PacketHandler>() {
                { (int)ServerPackets.AssignID, ClientReceive.AssignID }
            };
            
            Console.WriteLine("Initialized client data");
        }

        public class TCP
        {

            public TcpClient socket;

            private NetworkStream stream;
            private Packet receivedData;
            private byte[] receiveBuffer;

            public void Connect() {
                socket = new TcpClient {
                    ReceiveBufferSize = BufferSize,
                    SendBufferSize = BufferSize
                }; // Initialize new TCP client
                
                receiveBuffer = new byte[BufferSize];
                socket.BeginConnect(ConnectedIP, ConnectedPort, ConnectCallback, socket); // Attempt to connect
            }

            public void Disconnect() {
                if (!tcp.socket.Connected) {
                    Console.WriteLine("Failed to disconnect: Not currently connected to server", ConsoleColor.Red);
                    return;
                }
            
                tcp.socket.Close(); // Disconnect the client
                Console.WriteLine("Disconnected from server");

                socket = null;
            }

            public void SendData(Packet packet) {
                try {
                    if (socket != null) {
                        stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
                    }
                }
                catch (Exception ex) {
                    Console.WriteLine($"Failed to send data via TCP: {ex.Message}");
                }
            }
            
            private void ConnectCallback(IAsyncResult result) {
                try {
                    socket.EndConnect(result); // End connection and begin initialization

                    if (!socket.Connected) {
                        return;
                    }

                    stream = socket.GetStream();
                    receivedData = new Packet();
                    stream.BeginRead(receiveBuffer, 0, BufferSize, ReceiveCallback, null);

                    Console.WriteLine("Conneted to server!", ConsoleColor.Green);
                    failIndex = 0;
                }
                catch (Exception ex) {
                    Console.WriteLine("Failed to connect to server", ConsoleColor.Red);
                    failIndex++; // Increment retry count

                    if (failIndex < RetryConnectCount) {
                        Connect(); // Retry connection
                        Console.WriteLine("Attempting to connect again...", ConsoleColor.Yellow);
                    }
                    else {
                        Console.WriteLine("Cannot connect to server. Either this server does not exist, or is down.");
                        failIndex = 0;
                    }
                }
            }

            private bool HandleData(byte[] data) {
                int packetLength = 0;
                receivedData.SetBytes(data);

                if (receivedData.UnreadLength() >= 4) {
                    packetLength = receivedData.ReadInt();
                    if (packetLength <= 0) return true;
                }

                while (packetLength > 0 && packetLength <= receivedData.UnreadLength()) {
                    byte[] packetBytes = receivedData.ReadBytes(packetLength);
                    
                    using (Packet packet = new Packet(packetBytes)) {
                        int id = packet.ReadInt();
                        PacketHandlers[id](packet);
                    }
                    
                    packetLength = 0;
                    if (receivedData.UnreadLength() >= 4) {
                        packetLength = receivedData.ReadInt();
                        if (packetLength <= 0) return true;
                    }
                }

                if (packetLength <= 1) return true;

                return false;
            }

            private void ReceiveCallback(IAsyncResult result) {
                try {
                    int byteLength = stream.EndRead(result);
                    if (byteLength <= 0) {
                        Client.Disconnect();
                        return;
                    }

                    byte[] data = new byte[BufferSize];
                    Array.Copy(receiveBuffer, data, byteLength);

                    receivedData.Reset(HandleData(data));
                    stream.BeginRead(receiveBuffer, 0, BufferSize, ReceiveCallback, null);
                }
                catch {
                    Disconnect();
                }
            }

        }

        public class UDP
        {

            public UdpClient socket;
            public IPEndPoint endPoint;

            public UDP() {
                endPoint = new IPEndPoint(IPAddress.Parse(ConnectedIP), ConnectedPort);
            }

            public void Connect(int localPort) {
                socket = new UdpClient(localPort);
                
                socket.Connect(endPoint);
                socket.BeginReceive(ReceiveCallback, null);

                using (Packet packet = new Packet()) {
                    SendData(packet);
                }
            }

            public void Disconnect() {
                Disconnect();

                endPoint = null;
                socket = null;
            }

            public void SendData(Packet packet) {
                try {
                    if (socket != null) {
                        socket.BeginSend(packet.ToArray(), packet.Length(), null, null);
                    }
                }
                catch (Exception ex) {
                    Console.WriteLine($"Failed to send packet via UDP: {ex.Message}");
                }
            }

            private void HandleData(byte[] data) {
                using (Packet packet = new Packet(data)) {
                    int packetLength = packet.ReadInt();
                    data = packet.ReadBytes(packetLength);
                }

                ThreadManager.ExecuteOnMainThread(() => {
                    using (Packet newPacket = new Packet(data)) {
                        int packetID = newPacket.ReadInt();
                        PacketHandlers[packetID](newPacket);
                    }
                });
            }

            private void ReceiveCallback(IAsyncResult result) {
                try {
                    byte[] data = socket.EndReceive(result, ref endPoint);
                    socket.BeginReceive(ReceiveCallback, null);

                    if (data.Length < 4) {
                        Disconnect();
                        return;
                    }

                    HandleData(data);
                }
                catch (Exception ex) {
                    Disconnect();
                }
            }

        }

        private static class ClientReceive
        {

            public static void AssignID(Packet packet) {
                int id = packet.ReadInt();

                ClientID = id;
                Console.WriteLine($"Server: Assigned ID number {ClientID}");
                udp.Connect(((IPEndPoint)tcp.socket.Client.LocalEndPoint).Port);
                
                ClientSend.Login("wrongpassword");
            }
        
        }

        private static class ClientSend
        {

            public static void Login(string password) {
                using (Packet packet = new Packet((int)ClientPackets.Login)) {
                    packet.Write(password);
                    SendTCP(packet);
                }
            }
            
        }
        
    }

}