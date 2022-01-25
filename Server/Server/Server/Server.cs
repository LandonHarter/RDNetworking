using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Networking.Server
{

    public static class Server
    {

        // Server Options
        public static int MaxPlayers { get; private set; }
        public static int Port { get; private set; }
        public static int TicksPerSecond { get; set; } = 30;

        // Server Listeners
        private static TcpListener tcpListener;
        private static UdpClient udpListener;
        
        // Properties
        public static bool Open { get; private set; }
        
        // Clients
        public static Dictionary<int, ServerClient> clients { get; private set; } = new Dictionary<int, ServerClient>();
        
        // Packets
        public delegate void PacketHandler(int client, Packet packet);
        public static Dictionary<int, PacketHandler> PacketHandlers { get; private set; }

        private static Dictionary<string, ClientRole> Passwords = new Dictionary<string, ClientRole>();

        public static void Start(int maxPlayers, int port) {
            MaxPlayers = maxPlayers;
            Port = port;
            
            InitializeServerData();

            tcpListener = new TcpListener(IPAddress.Any, Port); // Create socket and bind to port
            tcpListener.Start(); // Start the TCP Listener
            tcpListener.BeginAcceptTcpClient(TCPClientConnect, null); // Client connect callback

            udpListener = new UdpClient(Port);
            udpListener.BeginReceive(UDPReceiveCallback, null);

            string hostName = Dns.GetHostName();
            string hostIp = Dns.GetHostByName(hostName).AddressList[0].ToString();
            Console.WriteLine($"Server started at {hostIp}:{Port}");

            Open = true;
            new Thread(Update).Start();
        }

        private static void Update() {
            Console.WriteLine($"Server running at {TicksPerSecond} ticks per second", ConsoleColor.Green);
            
            DateTime nextLoop = DateTime.Now;
            while (Open) {
                while (nextLoop < DateTime.Now) {
                    ThreadManager.Update();
                    
                    nextLoop = nextLoop.AddMilliseconds(TicksPerSecond);
                    if (nextLoop > DateTime.Now) {
                        Thread.Sleep(nextLoop - DateTime.Now);
                    }
                }
            }
        }

        public static void RegisterPassword(string password, ClientRole role) {
            if (role == ClientRole.User) return;
            
            if (Passwords.ContainsKey(password)) {
                Console.WriteLine($"Cannot set password to {password}. This password is already used by another role.", ConsoleColor.Red);
                return;
            }
            
            Passwords.Add(password, role);
        }

        public static void SendTCP(int client, Packet packet) {
            packet.WriteLength();
            clients[client].tcp.SendData(packet);
        }
        
        public static void SendUDP(IPEndPoint clientEndPoint, Packet packet) {
            try {
                if (clientEndPoint != null) {
                    packet.WriteLength();
                    udpListener.BeginSend(packet.ToArray(), packet.Length(), clientEndPoint, null, null);
                }
            }
            catch (Exception ex) {
                Console.WriteLine($"Failed to send data via UDP: {ex.Message}", ConsoleColor.Red);
            }
        }

        public static void SendTCPToAll(Packet packet) {
            packet.WriteLength();

            for (int i = 1; i <= MaxPlayers; i++) {
                if (clients[i].tcp.socket == null) break;
                clients[i].tcp.SendData(packet);
            }
        }
        
        public static void SendUDPToAll(Packet packet) {
            packet.WriteLength();

            for (int i = 1; i <= MaxPlayers; i++) {
                clients[i].udp.SendData(packet);
            }
        }

        private static void InitializeServerData() {
            for (int i = 1; i <= MaxPlayers; i++) {
                clients.Add(i, new ServerClient(i));
            }

            PacketHandlers = new Dictionary<int, PacketHandler>() {
                { (int)ClientPackets.Login, ServerReceive.ClientLogin },
                { (int)ClientPackets.SendChat, ServerReceive.ClientSendChat },
            };
            RegisterPassword("adminpassword", ClientRole.Admin);
            
            Console.WriteLine("Initialized server data");
        }

        private static void TCPClientConnect(IAsyncResult result) {
            TcpClient client = tcpListener.EndAcceptTcpClient(result); // Get newly connected client
            tcpListener.BeginAcceptTcpClient(TCPClientConnect, null); // Start listening for new clients again
            Console.WriteLine($"Connection from {client.Client.RemoteEndPoint}", ConsoleColor.Green);

            for (int i = 1; i <= MaxPlayers; i++) {
                if (clients[i].tcp.socket == null) {
                    clients[i].tcp.Connect(client);
                    return;
                }
            }
            
            Console.WriteLine("Failed to connect client: Max clients reached");
        }

        private static void UDPReceiveCallback(IAsyncResult result) {
            if (udpListener == null) {
                return;
            }

            try {
                IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = udpListener.EndReceive(result, ref clientEndPoint);

                udpListener.BeginReceive(UDPReceiveCallback, null);

                if (data.Length < 4) {
                    return;
                }

                using (Packet packet = new Packet(data)) {
                    int id = packet.ReadInt();

                    if (id == 0) {
                        return;
                    }

                    if (clients[id].udp.endPoint == null) {
                        clients[id].udp.Connect(clientEndPoint);
                    }

                    if (clients[id].udp.endPoint.ToString() == clientEndPoint.ToString()) {
                        clients[id].udp.HandleData(packet);
                    }
                }
            }
            catch (Exception ex) {
                Console.WriteLine($"Failed to receive data via UDP: {ex.Message}", ConsoleColor.Red);
            }
        }

        private static class ServerReceive
        {

            public static void ClientLogin(int client, Packet packet) {
                string password = packet.ReadString();

                if (Passwords.TryGetValue(password, out ClientRole newRole)) {
                    clients[client].Role = newRole;
                    Console.WriteLine($"Client {client} logged in as {newRole.ToString()}");
                }
            }
            
            public static void ClientSendChat(int client, Packet packet) {
                string message = packet.ReadString();

                using (Packet newPacket = new Packet((int)ServerPackets.DistributeChat)) {
                    newPacket.Write(clients[client].tcp.socket.Client.RemoteEndPoint.ToString());
                    newPacket.Write(message);
                    
                    SendTCPToAll(newPacket);
                }
                
                Console.WriteLine($"{clients[client].tcp.socket.Client.RemoteEndPoint}: {message}");
            }
            
        }
    }
}