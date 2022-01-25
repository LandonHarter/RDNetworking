using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Networking.Client;

namespace MessageApp
{
    static class Program
    {
        
        [STAThread]
        static void Main()
        {
            Client.Connect();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());

            Client.Disconnect();
        }

    }

}
