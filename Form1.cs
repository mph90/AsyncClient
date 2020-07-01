using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;

namespace AsyncClient
{
    public partial class Form1 : Form
    {
        Socket Socket;
        byte[] buffer = new byte[256];
        string username;

        public Form1()

        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            textBoxIP.Text = "127.0.0.1";
            textBoxPort.Text = "1001";
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            string ipaddress = textBoxIP.Text;
            int port = int.Parse(textBoxPort.Text);
            Connect(ipaddress, port);
            username = textBoxName.Text;
        }

        private void Connect(string ipaddress, int port)
        {
            try
            {
                // Try to make a socket connection
                Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream,
                ProtocolType.Tcp);
                Socket.BeginConnect(ipaddress, port, ConnectHandler, Socket);
            }
            catch (Exception ex)
            {
                listBox1.Items.Add("Socket connection error:\n" + ex.ToString());
                listBox1.SelectedIndex = listBox1.Items.Count - 1;
                listBox1.SelectedIndex = -1;
            }
        }

        private void ConnectHandler(IAsyncResult info) // CALLBACK
        {
            // Which socket is this using?
            Socket s = (Socket)info.AsyncState;
            // Complete the connection
            s.EndConnect(info);
            // Set up an event handler for receiving messages on socket s
            Receive(s);
        }

        private void buttonSend_Click(object sender, EventArgs e)
        {
            String displayMessage = username + ": " + textBoxMessage.Text;
            Transmit(Socket, displayMessage);
            textBoxMessage.Text = String.Empty;
        }

        private void Transmit(Socket s, string text)
        {
            // Prepare message
            byte[] messageBytes = Encoding.ASCII.GetBytes(text);
            // Send it
            s.BeginSend(messageBytes, 0, messageBytes.Length, 0,
            new AsyncCallback(TransmitHandler), s);
        }

        private void TransmitHandler(IAsyncResult info) // CALLBACK
        {
            // Which socket is this using?
            Socket s = (Socket)info.AsyncState;
            int bytesSent = s.EndSend(info);
        }

        private void Receive(Socket s)
        {
            s.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None,
            ReceiveHandler, s);
        }

        private void ReceiveHandler(IAsyncResult info) // CALLBACK
        {
            try
            {
                // Which socket is this using?
                Socket s = (Socket)info.AsyncState;
                // Read message
                int numBytesReceived = s.EndReceive(info);
                string message = Encoding.ASCII.GetString(buffer, 0, numBytesReceived);
                // Update display
                listBox1.Items.Add(message);
                listBox1.SelectedIndex = listBox1.Items.Count - 1;
                listBox1.SelectedIndex = -1;
                // Reset the event handler for new incoming messages on socket s
                Receive(s);
            }
            catch (Exception) { }
        }

        private void buttonDisconnect_Click(object sender, EventArgs e)
        {
            Socket.Close();
            Application.Exit();
        }

        private void textBoxMessage_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                String displayMessage = username + ": " + textBoxMessage.Text;
                Transmit(Socket, displayMessage);
                textBoxMessage.Text = String.Empty;
            }
        }
    }
}
