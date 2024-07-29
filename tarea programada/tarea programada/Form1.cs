using System;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace tarea_programada
{
    public partial class Form1 : Form
    {
        private TextBox txtMessage;
        private TextBox txtReceivedMessages;
        private TextBox txtPort;
        private Button btnSend;
        private ChatClient chatClient;

        public Form1()
        {
            InitializeComponent();
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            // Crear y configurar TextBox para ingresar mensajes
            txtMessage = new TextBox
            {
                Name = "txtMessage",
                Location = new Point(10, 10),
                Width = 200
            };
            this.Controls.Add(txtMessage);

            // Crear y configurar TextBox para mostrar mensajes recibidos
            txtReceivedMessages = new TextBox
            {
                Name = "txtReceivedMessages",
                Location = new Point(10, 50),
                Width = 200,
                Height = 100,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical
            };
            this.Controls.Add(txtReceivedMessages);

            // Crear y configurar TextBox para ingresar el puerto del destinatario
            txtPort = new TextBox
            {
                Name = "txtPort",
                Location = new Point(220, 10),
                Width = 60
            };
            this.Controls.Add(txtPort);

            // Crear y configurar el botón para enviar mensajes
            btnSend = new Button
            {
                Name = "btnSend",
                Text = "Enviar",
                Location = new Point(220, 50)
            };
            btnSend.Click += BtnSend_Click;
            this.Controls.Add(btnSend);

            // Inicializar el cliente de chat
            chatClient = new ChatClient(8000); // Cambia 8000 por el puerto de escucha deseado
            chatClient.OnMessageReceived += ChatClient_OnMessageReceived;
        }

        private void BtnSend_Click(object sender, EventArgs e)
        {
            string message = txtMessage.Text;
            if (int.TryParse(txtPort.Text, out int port))
            {
                chatClient.SendMessage(message, port);
                txtReceivedMessages.AppendText("Tú: " + message + Environment.NewLine);
                txtMessage.Clear();
            }
            else
            {
                MessageBox.Show("Por favor, ingrese un puerto válido.");
            }
        }

        private void ChatClient_OnMessageReceived(string message)
        {
            Invoke(new Action(() =>
            {
                txtReceivedMessages.AppendText("Amigo: " + message + Environment.NewLine);
            }));
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }
    }

    public class ChatClient
    {
        private Socket socket;
        private int listeningPort;
        private bool isConnected = false;

        public ChatClient(int listeningPort)
        {
            this.listeningPort = listeningPort;
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            StartListening();
        }

        public void Connect(string ipAddress, int port)
        {
            try
            {
                socket.Connect(new IPEndPoint(IPAddress.Parse(ipAddress), port));
                isConnected = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al conectar: " + ex.Message);
            }
        }

        public void SendMessage(string message, int port)
        {
            if (!isConnected)
            {
                Connect("127.0.0.1", port); // Conectar al puerto de destino
            }

            if (isConnected)
            {
                try
                {
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    socket.SendTo(data, new IPEndPoint(IPAddress.Loopback, port));
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al enviar mensaje: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("No se pudo conectar al servidor.");
            }
        }

        public void StartListening()
        {
            Task.Run(() =>
            {
                Socket listeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                listeningSocket.Bind(new IPEndPoint(IPAddress.Any, listeningPort));
                listeningSocket.Listen(10);

                while (true)
                {
                    try
                    {
                        Socket handler = listeningSocket.Accept();
                        byte[] buffer = new byte[1024];
                        int received = handler.Receive(buffer);
                        string receivedMessage = Encoding.UTF8.GetString(buffer, 0, received);
                        OnMessageReceived?.Invoke(receivedMessage);
                        handler.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error al recibir mensaje: " + ex.Message);
                    }
                }
            });
        }

        public event Action<string> OnMessageReceived;
    }
}
