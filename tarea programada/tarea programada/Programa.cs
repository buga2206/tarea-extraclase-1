using System;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace tarea_programada
{
    public partial class Programa : Form // Define la clase Programa que hereda de Form.
    {
        private TextBox txtMensaje;
        private TextBox txtMensajesRecibidos;
        private TextBox txtPuertoEnvio;
        private TextBox txtPuertoEscucha;
        private Button btnEnviar;
        private Label lblMensaje;
        private Label lblPuertoEnvio;
        private Label lblChat;
        private Label lblPuertoEscucha;
        private ClienteChat clienteChat;
        private int puertoEscucha;

        public Programa() // Constructor de la clase Programa.
        {
            InitializeComponent();
            InicializarComponentesPersonalizados();
        }

        private void InicializarComponentesPersonalizados()
        {
            // configuracion de los labels
            lblMensaje = new Label
            {
                Text = "Escriba su mensaje a enviar",
                Location = new Point(10, 10),
                AutoSize = true
            };
            this.Controls.Add(lblMensaje);

            lblPuertoEnvio = new Label
            {
                Text = "Puerto para enviar el mensaje",
                Location = new Point(290, 10),
                AutoSize = true
            };
            this.Controls.Add(lblPuertoEnvio);

            lblChat = new Label
            {
                Text = "Chat",
                Location = new Point(10, 90),
                AutoSize = true
            };
            this.Controls.Add(lblChat);

            lblPuertoEscucha = new Label
            {
                Text = "Estás en el puerto:",
                Location = new Point(420, 160),
                AutoSize = true
            };
            this.Controls.Add(lblPuertoEscucha);

            // textbox para ingresar mensaje
            txtMensaje = new TextBox
            {
                Name = "txtMensaje",
                Location = new Point(10, 30),
                Width = 240
            };
            this.Controls.Add(txtMensaje);

            // texttox para mostrar los mensajes recibidos y enviados
            txtMensajesRecibidos = new TextBox
            {
                Name = "txtMensajesRecibidos",
                Location = new Point(10, 110),
                Width = 350,
                Height = 350,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical
            };
            this.Controls.Add(txtMensajesRecibidos);

            // textbox para ingresar el puerto para enviar mensaje
            txtPuertoEnvio = new TextBox
            {
                Name = "txtPuertoEnvio",
                Location = new Point(290, 30),
                Width = 70,
            };
            this.Controls.Add(txtPuertoEnvio);

            // botón para enviar el mensaje
            btnEnviar = new Button
            {
                Name = "btnEnviar",
                Text = "Enviar",
                Location = new Point(450, 450),
                Width = 100,
                Height = 30
            };
            btnEnviar.Click += BtnEnviar_Click;
            this.Controls.Add(btnEnviar);

            // textbox que muestra el puerto donde se ubica el usuario
            txtPuertoEscucha = new TextBox
            {
                Name = "txtPuertoEscucha",
                Location = new Point(420, 180),
                Width = 120,
                ReadOnly = true
            };
            this.Controls.Add(txtPuertoEscucha);

            // se inicia el chat
            puertoEscucha = 8000;  // puerto predeterminado
            string[] args = Environment.GetCommandLineArgs();  // obtiene los argumentos de la línea de comandos
            if (args.Length > 2 && int.TryParse(args[2], out int puertoParsed))
            {
                puertoEscucha = puertoParsed;  // se signa el puerto desde los argumentos de la línea de comandos si es válido
            }
            txtPuertoEscucha.Text = puertoEscucha.ToString();  // muestra el puerto de escucha en el textbox
            clienteChat = new ClienteChat(puertoEscucha);  // se crea una instancia de ClienteChat
            clienteChat.OnMensajeRecibido += ClienteChat_OnMensajeRecibido;  // se signa el evento para mensajes recibidos
        }

        private void BtnEnviar_Click(object? sender, EventArgs e)  // método para manejar el evento click del botón
        {
            string mensaje = txtMensaje.Text;  // se obtiene el texto del mensaje a enviar
            if (int.TryParse(txtPuertoEnvio.Text, out int puerto))
            {
                clienteChat.EnviarMensaje($"Enviado desde puerto {puertoEscucha}: {mensaje}", puerto);  // envía el mensaje al puerto especificado
                txtMensajesRecibidos.AppendText("tú: " + mensaje + Environment.NewLine);  // muestra el mensaje enviado en el textbox
                txtMensaje.Clear();  // limpia el textbox del mensaje
            }
            else
            {
                MessageBox.Show("Por favor, ingrese un puerto válido.");  // muestra un mensaje de error si el puerto no es válido
            }
        }

        private void ClienteChat_OnMensajeRecibido(string mensaje)  // método para manejar el evento de mensaje recibido
        {
            Invoke(new Action(() =>
            {
                txtMensajesRecibidos.AppendText(mensaje + Environment.NewLine);  // muestra el mensaje recibido en el textbox
            }));
        }

        private void Programa_Load(object sender, EventArgs e)
        {
        }
    }

    public class ClienteChat  // se define la clase ClienteChat
    {
        private Socket socketEscucha;  // socket para escuchar conexiones entrantes
        private int puertoEscucha;  // puerto en el que se escuchan las conexiones
        private Socket socketEnvio;  // socket para enviar mensajes

        public ClienteChat(int puertoEscucha)  // constructor de la clase ClienteChat
        {
            this.puertoEscucha = puertoEscucha;  // se asigna el puerto de escucha
            socketEscucha = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);  // crea el socket de escucha
            socketEnvio = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);  // crea el socket de envío
            IniciarEscucha();  // se inicia la escucha de conexiones entrantes
        }

        public void EnviarMensaje(string mensaje, int puerto)  // método para enviar mensajes
        {
            try
            {
                socketEnvio = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);  // crea un nuevo socket de envío
                socketEnvio.Connect(new IPEndPoint(IPAddress.Loopback, puerto));  // conecta el socket al puerto especificado
                byte[] data = Encoding.UTF8.GetBytes(mensaje);  // codifica el mensaje en bytes
                socketEnvio.Send(data);  // envía el mensaje
                socketEnvio.Shutdown(SocketShutdown.Both);  // cierra la conexión
                socketEnvio.Close();  // cierra el socket
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al enviar mensaje: " + ex.Message);  // muestra un mensaje de error si ocurre una excepción
            }
        }

        public void IniciarEscucha()  // método para iniciar la escucha de conexiones entrantes
        {
            Task.Run(() =>
            {
                socketEscucha.Bind(new IPEndPoint(IPAddress.Any, puertoEscucha));  // se enlaza el socket al puerto de escucha
                socketEscucha.Listen(10);  // configura el socket para escuchar conexiones entrantes

                while (true)
                {
                    try
                    {
                        Socket handler = socketEscucha.Accept();  // acepta una conexión entrante
                        byte[] buffer = new byte[1024];  // buffer para almacenar los datos recibidos
                        int received = handler.Receive(buffer);  // recibe datos del socket
                        string mensajeRecibido = Encoding.UTF8.GetString(buffer, 0, received);  // decodifica los datos recibidos en una cadena
                        OnMensajeRecibido?.Invoke(mensajeRecibido);  // invoca el evento de mensaje recibido
                        handler.Shutdown(SocketShutdown.Both);  // cierra la conexión
                        handler.Close();  // cierra el socket
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error al recibir mensaje: " + ex.Message);  // muestra un mensaje de error si ocurre una excepción
                    }
                }
            });
        }

        public event Action<string> OnMensajeRecibido;
    }
}
