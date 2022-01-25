using System;

namespace Networking.Server
{
    public class Program
    {

        public static void Main(string[] args) {
            Server.Start(10, 5000); // Start the server with a max of 10 players on port 5000

            Console.ReadKey();
        }
        
    }
}