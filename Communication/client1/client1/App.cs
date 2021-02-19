using System;
using System.Text;
using Xamarin.Forms;
using System.Net;  
using System.Net.Sockets;  
using System.Threading;  
using Tizen.System;



namespace client1
{
    //Creating a State Object 
    public class StateObject
    {
        public Socket workSocket = null;
        // Size of receive buffer.  
        public const int BufferSize = 256;
        // Receive buffer.  
        public byte[] buffer = new byte[BufferSize];
        // Received data string.  
        public StringBuilder sb = new StringBuilder();
    }

    public class Client
    {

        public static String MESSAGE = String.Empty;

        // Port Number and ipaddress of the class
        private int portNumber;
        private String hostIpAddress;

        public static Socket client;

        public static int counter = 0;

        private static String TOBESENT = "Strong storm coming, pack up and leave, 5 minutes";

        public static Label _label = new Label();
        Feedback feedback = new Feedback();

        //Needing these functions for threading  
        private static ManualResetEvent connectDone =
            new ManualResetEvent(false);
        private static ManualResetEvent sendDone =
            new ManualResetEvent(false);
        private static ManualResetEvent receiveDone =
            new ManualResetEvent(false);


        // The response from the remote device.  
        private static String recievedString = String.Empty;

        //Constructor of the client class
        public Client(String ipAddress, int port)
        {

            hostIpAddress = ipAddress;
            portNumber = port;
        }


        public void startClient()
        {
            try
            {

                IPAddress ip = System.Net.IPAddress.Parse(hostIpAddress);
                Console.WriteLine("got the host ip Adress");
                IPEndPoint ipEndPoint = new IPEndPoint(ip, portNumber);
                Console.WriteLine("Created the endpoint");
                client = new Socket(ip.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);
                Console.WriteLine(ip.AddressFamily + "");
                Console.WriteLine("Created the socket");
                client.BeginConnect(ipEndPoint,
                    new AsyncCallback(ConnectCallback), client);
                Console.WriteLine("socket began");
                connectDone.WaitOne();
                Console.WriteLine("Connection thread is set");
            }
            catch (Exception e)
            {
                Console.WriteLine("Socket created failed");
                Console.WriteLine(e.ToString());
            }
        }

        public void communicate()
        {
            try
            {
                StateObject state = new StateObject();
                state.workSocket = Client.client;
                // Begin receiving the data from the remote device. 
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                                        new AsyncCallback(ReceiveCallback), state);
                receiveDone.WaitOne();
                receiveDone.Reset();
                byte[] byteData = Encoding.ASCII.GetBytes(TOBESENT);
                // Begin sending the data to the remote device.  
                client.BeginSend(byteData, 0, byteData.Length, 0,
                    new AsyncCallback(SendCallback), client);
                sendDone.WaitOne();
                sendDone.Reset();

                Client.counter++;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = client.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to server.", bytesSent);
                sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }


        private static void ReceiveCallback(IAsyncResult ar)
        {

            try
            {
                // Retrieve the state object and the client socket
                // from the asynchronous state object.  
                Console.WriteLine("openned");
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;

                // Read data from the remote device.  
                int bytesRead = client.EndReceive(ar);

                if (bytesRead == System.Text.ASCIIEncoding.ASCII.GetByteCount(TOBESENT))
                {
                    // There might be more data, so store the data received so far.  
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
                    Console.WriteLine(state.sb.ToString());
                    recievedString = state.sb.ToString();
                    Client.MESSAGE = recievedString;
                    receiveDone.Set();

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket client = (Socket)ar.AsyncState;
                // Complete the connection.  
                client.EndConnect(ar);
                Console.WriteLine("Socket connected to {0}",
                client.RemoteEndPoint.ToString());
                // Signal that the connection has been made.  
                connectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }

    public class App : Application
        {
            Button button;
            Label label;

            Client client;
            void OnButtonClicked(object sender, EventArgs e)
            {
                client = new Client("192.168.0.167", 9000);
                client.startClient();

                while (Client.counter < 100)
                {
                    client.communicate();
                    Console.WriteLine("" + Client.counter);
                }

                    Client._label.Text = Client.MESSAGE;
                    Feedback feedback = new Feedback();
                    feedback.Play(FeedbackType.Vibration, "Email");
            }

            public App()
            {
                button = new Button
                {
                    Text = "Click here to start the client",
                    BackgroundColor = Color.Red,
                    HorizontalOptions = LayoutOptions.Center,
                };
                Client._label.Text = "recieved Message";
                Client._label.HorizontalOptions = LayoutOptions.Center;
                button.Clicked += OnButtonClicked;

                // The root page of your application
                MainPage = new ContentPage
                {
                    Content = new StackLayout
                    {
                        VerticalOptions = LayoutOptions.Center,
                        Children = {
                        button,
                        Client._label,
                    }
                    }
                };
            }

            protected override void OnStart()
            {

                // Handle when your app starts
            }

            protected override void OnSleep()
            {
                // Handle when your app sleeps
            }

            protected override void OnResume()
            {
                // Handle when your app resumes
            }
        }
    }


