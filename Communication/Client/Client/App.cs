using System;
using System.Text;
using Xamarin.Forms;
using System.Net;  
using System.Net.Sockets;  
using System.Threading;  
using Tizen.System;



namespace Client
{
    //Creating a State Object for handling the class inputs and outputs
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

        // static attributes to be shared among different classes

        public static String MESSAGE = String.Empty;

        private int portNumber;
        private String hostIpAddress;

        public static Socket client;

        public static int counter = 0;

        private static String TOBESENT = "Strong storm coming, pack up and leave, 5 minutes";
        private static String recievedString = String.Empty;

        //Classes for UI
        public static Label label = new Label();
        public static Button connection = new Button();

        public static Button reset = new Button();
        public static Feedback feedback = new Feedback();
        public static Feedback feedback2 = new Feedback();




        //Needing these functions for threading  
        private static ManualResetEvent connectDone =
            new ManualResetEvent(false);
        private static ManualResetEvent sendDone =
            new ManualResetEvent(false);
        private static ManualResetEvent receiveDone =
            new ManualResetEvent(false);



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


        public class Receive
        {
            public Receive()
            {

            }

            public void recieveStart()
            {

                StateObject state = new StateObject();
                state.workSocket = Client.client;
                // Begin receiving the data from the remote device. 
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                                        new AsyncCallback(ReceiveCallback), state);

            }
        }


        public void communicate()
        {
            try
            {
                Receive receive = new Receive();

                Thread receiveThread = new Thread(receive.recieveStart);
                receiveThread.Start();
                receiveDone.WaitOne();
                receiveDone.Reset();

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

                        // There might be more data, so store the data received so far.  
                        state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
                        Console.WriteLine(state.sb.ToString());
                        recievedString = state.sb.ToString();
                        Client.MESSAGE = recievedString;
                        receiveDone.Set();
                        Client.label.Text = Client.MESSAGE;

                        double riskScore = Double.Parse(Client.MESSAGE);

                        Console.WriteLine(riskScore);

                        if (riskScore > 7 && riskScore <= 9)
                        {
                            Client.feedback.Play(FeedbackType.Vibration, "Tap");

                        }
                        else if (riskScore > 9 && riskScore <= 9.5)
                        {
                            Client.feedback.Play(FeedbackType.Vibration, "Email");
                        }  else if (riskScore > 9.5) 
                        {
                            Client.feedback.Play(FeedbackType.Vibration, "WAKEUP");
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

                while (true)
                {
                    client.communicate();
                    Console.WriteLine("" + Client.counter);
                }

                // Client.feedback2.Play(FeedbackType.Sound, "Tap");
            }

            public App()
            {
                button = new Button
                {
                    Text = "Click here to start the client",
                    BackgroundColor = Color.Red,
                    HorizontalOptions = LayoutOptions.Center,
                };
                Client.label.Text = "recieved Message";
                Client.label.HorizontalOptions = LayoutOptions.Center;
                button.Clicked += OnButtonClicked;

                // The root page of your application
                MainPage = new ContentPage
                {
                    Content = new StackLayout
                    {
                        VerticalOptions = LayoutOptions.Center,
                        Children = {
                        button,
                        Client.label,
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


























// using System;
// using System.Text;
// using Xamarin.Forms;
// using System.Net;  
// using System.Net.Sockets;  
// using System.Threading;  
// using Tizen.System;



// namespace Client
// {
//     //Creating a State Object for handling the class inputs and outputs
//     public class StateObject
//     {
//         public Socket workSocket = null;
//         // Size of receive buffer.  
//         public const int BufferSize = 256;
//         // Receive buffer.  
//         public byte[] buffer = new byte[BufferSize];
//         // Received data string.  
//         public StringBuilder sb = new StringBuilder();

//     }

//     public class Client
//     {

//         // static attributes to be shared among different classes

//         public static String MESSAGE = String.Empty;

//         private int portNumber;
//         private String hostIpAddress;

//         public static Socket client;

//         public static int counter = 0;

//         private static String TOBESENT = "Strong storm coming, pack up and leave, 5 minutes";
//          private static String recievedString = String.Empty;

//         //Classes for UI
//         public static Label label = new Label();
//         public static Button connection = new Button();

//         public static Button reset = new Button();
//         public static Feedback feedback = new Feedback();
//         public static Feedback feedback2 = new Feedback();




//         //Needing these functions for threading  
//         private static ManualResetEvent connectDone =
//             new ManualResetEvent(false);
//         private static ManualResetEvent sendDone =
//             new ManualResetEvent(false);
//         private static ManualResetEvent receiveDone =
//             new ManualResetEvent(false);



//         //Constructor of the client class
//         public Client(String ipAddress, int port)
//         {

//             hostIpAddress = ipAddress;
//             portNumber = port;
//         }


//         public void startClient()
//         {
//             try
//             {

//                 IPAddress ip = System.Net.IPAddress.Parse(hostIpAddress);
//                 Console.WriteLine("got the host ip Adress");
//                 IPEndPoint ipEndPoint = new IPEndPoint(ip, portNumber);
//                 Console.WriteLine("Created the endpoint");
//                 client = new Socket(ip.AddressFamily,
//                     SocketType.Stream, ProtocolType.Tcp);
//                 Console.WriteLine(ip.AddressFamily + "");
//                 Console.WriteLine("Created the socket");
//                 client.BeginConnect(ipEndPoint,
//                     new AsyncCallback(ConnectCallback), client);
//                 Console.WriteLine("socket began");
//                 connectDone.WaitOne();
//                 Console.WriteLine("Connection thread is set");
//             }
//             catch (Exception e)
//             {
//                 Console.WriteLine("Socket created failed");
//                 Console.WriteLine(e.ToString());
//             }
//         }


//         public class Receive{
//             public Receive(){

//             }

//             public void recieveStart(){

//                             StateObject state = new StateObject();
//                 state.workSocket = Client.client;
//                 // Begin receiving the data from the remote device. 
//                 client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
//                                         new AsyncCallback(ReceiveCallback), state);

//             }
//         }


//         public class Send {
//             public Send(){

//             }

//             public void startSending(){
//             byte[] byteData = Encoding.ASCII.GetBytes(TOBESENT);
//                 // Begin sending the data to the remote device.  
//                 client.BeginSend(byteData, 0, byteData.Length, 0,
//                     new AsyncCallback(SendCallback), client);
//             }
//         }



//         public void communicate()
//         {
//             try
//             {
//                 Receive receive = new Receive();

//                 Thread receiveThread = new Thread(receive.recieveStart);
//                 receiveThread.Start();
//                 receiveDone.WaitOne();
//                 receiveDone.Reset();


//                 Send send = new Send();

//                 Thread sendData = new Thread(send.startSending);
//                 sendData.Start();
//                 sendDone.WaitOne();
//                 sendDone.Reset();

//                 Client.counter++;
//             }
//             catch (Exception e)
//             {
//                 Console.WriteLine(e.ToString());
//             }
//         }

//         private static void SendCallback(IAsyncResult ar)
//         {
//             try
//             {
//                 // Retrieve the socket from the state object.  
//                 Socket client = (Socket)ar.AsyncState;

//                 // Complete sending the data to the remote device.  
//                 int bytesSent = client.EndSend(ar);
//                 Console.WriteLine("Sent {0} bytes to server.", bytesSent);
//                 sendDone.Set();
//             }
//             catch (Exception e)
//             {
//                 Console.WriteLine(e.ToString());
//             }
//         }


//         private static void ReceiveCallback(IAsyncResult ar)
//         {

//             try
//             {
//                 // Retrieve the state object and the client socket
//                 // from the asynchronous state object.  
//                 Console.WriteLine("openned");
//                 StateObject state = (StateObject)ar.AsyncState;
//                 Socket client = state.workSocket;

//                 // Read data from the remote device.  
//                 int bytesRead = client.EndReceive(ar);

//                 if (bytesRead == System.Text.ASCIIEncoding.ASCII.GetByteCount(TOBESENT))
//                 {
//                     // There might be more data, so store the data received so far.  
//                     state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
//                     Console.WriteLine(state.sb.ToString());
//                     recievedString = state.sb.ToString();
//                     Client.MESSAGE = recievedString;
//                     receiveDone.Set();

//                 }
//             }
//             catch (Exception e)
//             {
//                 Console.WriteLine(e.ToString());
//             }
//         }

//         private static void ConnectCallback(IAsyncResult ar)
//         {
//             try
//             {
//                 // Retrieve the socket from the state object.  
//                 Socket client = (Socket)ar.AsyncState;
//                 // Complete the connection.  
//                 client.EndConnect(ar);
//                 Console.WriteLine("Socket connected to {0}",
//                 client.RemoteEndPoint.ToString());
//                 // Signal that the connection has been made.  
//                 connectDone.Set();
//             }
//             catch (Exception e)
//             {
//                 Console.WriteLine(e.ToString());
//             }
//         }
//     }

//     public class App : Application
//         {
//             Button button;
//             Label label;

//             Client client;
//             void OnButtonClicked(object sender, EventArgs e)
//             {
//                 client = new Client("192.168.0.167", 9000);
//                 client.startClient();

//                 while (Client.counter < 1000)
//                 {
//                     client.communicate();
//                     Console.WriteLine("" + Client.counter);
//                 }

//                     Client.label.Text = Client.MESSAGE;
//                     Client.feedback.Play(FeedbackType.Vibration, "Email");
//                     // Client.feedback2.Play(FeedbackType.Sound, "Tap");
//         }

//             public App()
//             {
//                 button = new Button
//                 {
//                     Text = "Click here to start the client",
//                     BackgroundColor = Color.Red,
//                     HorizontalOptions = LayoutOptions.Center,
//                 };
//                 Client.label.Text = "recieved Message";
//                 Client.label.HorizontalOptions = LayoutOptions.Center;
//                 button.Clicked += OnButtonClicked;

//                 // The root page of your application
//                 MainPage = new ContentPage
//                 {
//                     Content = new StackLayout
//                     {
//                         VerticalOptions = LayoutOptions.Center,
//                         Children = {
//                         button,
//                         Client.label,
//                     }
//                     }
//                 };
//             }

//             protected override void OnStart()
//             {

//                 // Handle when your app starts
//             }

//             protected override void OnSleep()
//             {
//                 // Handle when your app sleeps
//             }

//             protected override void OnResume()
//             {
//                 // Handle when your app resumes
//             }
//         }
//     }














