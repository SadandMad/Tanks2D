using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tanks2D
{
    public partial class FormLogin : Form
    {
        internal String UserName;
        internal bool Connected = false;
        internal bool Replied = false;
        internal List<Sub> Subs = new List<Sub>();
        internal Task receiveUDP;

        internal class Packet
        {
            public byte Type;
            public UInt16 MsgLength;
            public byte[] Data;

            public Packet(byte T, string D)
            {
                Type = T;
                Data = Encoding.Unicode.GetBytes(D);
                MsgLength = (UInt16)(3 + Data.Length);
            }
            public Packet(byte[] D)
            {
                Type = D[0];
                MsgLength = BitConverter.ToUInt16(D, 1);
                Data = new byte[MsgLength - 3];
                Buffer.BlockCopy(D, 3, Data, 0, MsgLength - 3);
            }
            public Packet(byte T)
            {
                Type = T;
                MsgLength = 3;
            }
            public byte[] getBytes()
            {
                byte[] data = new byte[MsgLength];
                Buffer.BlockCopy(BitConverter.GetBytes(Type), 0, data, 0, 1);
                Buffer.BlockCopy(BitConverter.GetBytes(MsgLength), 0, data, 1, 2);
                if (MsgLength > 3)
                    Buffer.BlockCopy(Data, 0, data, 3, MsgLength - 3);
                return data;
            }
        }

        internal class Sub
        {
            public string name;
            public IPEndPoint endPoint;
            public Sub(string str, IPEndPoint iep)
            {
                name = str;
                endPoint = iep;
            }
        }

        public FormLogin()
        {
            InitializeComponent();
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            if (buttonConnect.Text == "Подключиться")
            {
                UserName = textBoxName.Text.Trim();
                if (UserName.Length > 3 && UserName.Length < 13)
                {
                    Connect();
                }
                else
                    MessageBox.Show("Введите имя для вашего аккаунта (4-12 символов)!");
            }
            else
            {
                UdpClient request = new UdpClient();
                foreach (Sub sub in Subs)
                {
                    MessageBox.Show(sub.name+" "+comboBox.SelectedItem.ToString());
                    if (sub.name == comboBox.SelectedItem.ToString())
                    {
                        MessageBox.Show("Equal");
                        request.Connect(sub.endPoint.Address, 8006);
                        Packet msg = new Packet(2, UserName);
                        request.Send(msg.getBytes(), msg.MsgLength);
                        request.Close();
                        break;
                    }
                }
            }
        }

        private void Connect()
        {
            textBoxName.Text = UserName;
            buttonConnect.Text = "Сыграть";
            buttonConnect.Enabled = false;
            UdpClient client = new UdpClient();
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("230.230.230.230"), 8005);
            try
            {
                Packet msg = new Packet(0, UserName);
                client.Send(msg.getBytes(), msg.MsgLength, endPoint);
                client.Close();
                textBoxName.Enabled = false;
                buttonConnect.Enabled = false;
                Connected = true;
                receiveUDP = new Task(() => ReceiveConnection());
                receiveUDP.Start();
            }
            catch (Exception ex)
            {
                client.Close();
                MessageBox.Show(ex.Message);
            }
        }
        private void ReceiveConnection()
        {
            try
            {
                while (Connected)
                {
                    UdpClient client = new UdpClient(8005);
                    client.JoinMulticastGroup(IPAddress.Parse("230.230.230.230"), 100);
                    IPEndPoint remoteIp = null;
                    byte[] data = client.Receive(ref remoteIp);
                    Packet msg = new Packet(data);
                    client.Close();
                    if (msg.Type == 0)
                    {
                        Sub sub = new Sub(Encoding.Unicode.GetString(msg.Data), new IPEndPoint(remoteIp.Address, 8006));
                        if (!Subs.Contains(sub))
                        {
                            Subs.Add(sub);
                            this.Invoke(new MethodInvoker(() =>
                            {
                                comboBox.Items.Add(sub.name);
                            }));
                        }
                        msg = new Packet(1, UserName);
                        UdpClient sender = new UdpClient();
                        sender.Connect(sub.endPoint.Address, 8006);
                        sender.Send(msg.getBytes(), msg.MsgLength);
                        sender.Close();
                    }
                    else if (msg.Type == 1)
                    {
                        Sub sub = new Sub(Encoding.Unicode.GetString(msg.Data), new IPEndPoint(remoteIp.Address, 8006));
                        if (!Subs.Contains(sub))
                        {
                            Subs.Add(sub);
                            this.Invoke(new MethodInvoker(() =>
                            {
                                comboBox.Items.Add(sub.name);
                            }));
                        }
                    }
                    else if (msg.Type == 2)
                    {
                        UdpClient reply = new UdpClient();
                        reply.Connect(new IPEndPoint(remoteIp.Address, 8006));
                        msg = new Packet(3, UserName);
                        reply.Send(msg.getBytes(), msg.MsgLength);
                        reply.Close();
                        using (var game = new Game1(1, remoteIp))
                            game.Run();
                        Close();
                    }
                    else if (msg.Type == 3)
                    {
                        using (var game = new Game1(2, remoteIp))
                            game.Run();
                        Close();
                    }
                    else
                        MessageBox.Show("An error occured!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        private void comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            buttonConnect.Enabled = true;
        }
    }
}