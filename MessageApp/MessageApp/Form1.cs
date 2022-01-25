using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Networking.Client;

namespace MessageApp
{
    public partial class Form1 : Form
    {

        public static Form1 Instance { get; private set; }

        public Form1()
        {
            Instance = this;

            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Client.SendMessage(MessageBox.Text);
            MessageBox.Clear();
        }

        public void CreateMessage(Packet packet)
        {
            Messages.Text += $"\n{packet.ReadString()}: {packet.ReadString()}";
        }

    }
}
