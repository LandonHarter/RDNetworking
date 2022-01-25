using System;

namespace Networking.Client
{

    public class Program
    {
        
        public static void Main(string[] args) {
            Client.Connect();
            Console.ReadKey();
            Client.Disconnect();
        }
        
    }

}