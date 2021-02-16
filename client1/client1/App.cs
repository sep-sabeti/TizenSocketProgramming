using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;
using System.Net;  
using System.Net.Sockets;  
using System.Threading;  
using Tizen.System;



namespace client1
{

    //Creating a State Object 
    public class StateObject {  
    // Client socket.  
    public Socket workSocket = null;  
    // Size of receive buffer.  
    public const int BufferSize = 256;  
    // Receive buffer.  
    public byte[] buffer = new byte[BufferSize];  
    // Received data string.  
    public StringBuilder sb = new StringBuilder();  
} 


public class Client {  
  
    // Port Number and ipaddress of the class
        private int portNumber;
        private String hostIpAddress;

        public static Socket client;

        private static String TOBESENT = "Hey";


    
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
    public Client(String ipAddress, int port){

        hostIpAddress = ipAddress;
        portNumber = port;
    }

public String getReceivedText(){
    return recievedString;
}

    public void startClient(){
        try{
            IPAddress ip = System.Net.IPAddress.Parse(hostIpAddress);
            Console.WriteLine("got the host ip Adress");
            IPEndPoint ipEndPoint = new IPEndPoint(ip,portNumber);
            Console.WriteLine("Created the endpoint");
            client = new Socket(ip.AddressFamily,  
                SocketType.Stream, ProtocolType.Tcp);  
            Console.WriteLine(ip.AddressFamily + "");
    
            Console.WriteLine("Created the socket");
            client.BeginConnect(ipEndPoint,
                new AsyncCallback(ConnectCallback), client);  
            Console.WriteLine("socket began ");
            connectDone.WaitOne(); 
            Console.WriteLine("Thread is done");
        } catch(Exception e){
            Console.WriteLine("Socket created failed");
            Console.WriteLine(e.ToString());
        }
    }

    public void communicate(){
        try {  
            // Create the state object.  
            StateObject state = new StateObject();  
            state.workSocket = client;  
  
            // Begin receiving the data from the remote device.  
            client.BeginReceive( state.buffer, 0, StateObject.BufferSize, 0,  
                new AsyncCallback(ReceiveCallback), state);  
                            Console.WriteLine("Communication is done");
                Feedback feedback = new Feedback();
                feedback.Play(FeedbackType.Vibration, "Email");

            byte[] byteData = Encoding.ASCII.GetBytes(TOBESENT);  
  
        // Begin sending the data to the remote device.  
        client.BeginSend(byteData, 0, byteData.Length, 0,  
            new AsyncCallback(SendCallback), client);  




        } catch (Exception e) {  
            Console.WriteLine(e.ToString());  
        }  
    }

        private static void SendCallback(IAsyncResult ar) {  
        try {  
            // Retrieve the socket from the state object.  
            Socket client = (Socket) ar.AsyncState;  
  
            // Complete sending the data to the remote device.  
            int bytesSent = client.EndSend(ar);  
            Console.WriteLine("Sent {0} bytes to server.", bytesSent);  
  
            // Signal that all bytes have been sent.  
            sendDone.Set();  
        } catch (Exception e) {  
            Console.WriteLine(e.ToString());  
        }  
    } 

        private static void ReceiveCallback( IAsyncResult ar ) {  
        try {  
            // Retrieve the state object and the client socket
            // from the asynchronous state object.  
            StateObject state = (StateObject) ar.AsyncState;  
            Socket client = state.workSocket;  
  
            // Read data from the remote device.  
            int bytesRead = client.EndReceive(ar);  
  
            if (bytesRead > 0) {  
                // There might be more data, so store the data received so far.  
            state.sb.Append(Encoding.ASCII.GetString(state.buffer,0,bytesRead));  
  
                // Get the rest of the data.  
                client.BeginReceive(state.buffer,0,StateObject.BufferSize,0,  
                    new AsyncCallback(ReceiveCallback), state);  
            } else {  
                // All the data has arrived; put it in response.  
                if (state.sb.Length > 1) {  
                    recievedString = state.sb.ToString();  
                }  
                // Signal that all bytes have been received.  
                receiveDone.Set();  
            }  
        } catch (Exception e) {  
            Console.WriteLine(e.ToString());  
        }  
    }  

        private static void ConnectCallback(IAsyncResult ar) {  
        try {  
            // Retrieve the socket from the state object.  
            Socket client = (Socket) ar.AsyncState;  
            // Complete the connection.  
            client.EndConnect(ar);  
            Console.WriteLine("Socket connected to {0}",  
            client.RemoteEndPoint.ToString());  
            // Signal that the connection has been made.  
            connectDone.Set();  
        } catch (Exception e) {  
            Console.WriteLine(e.ToString());  
        }  
    } 



    

}  
    
    
        public class App : Application
    {
        Button button;
        Label label;

        Client client;
        void OnButtonClicked(object sender , EventArgs e)
{
        client = new Client("192.168.0.167",9750);
        client.startClient();
        client.communicate();
        (sender as Button).Text = client.getReceivedText();
        (sender as Button).BackgroundColor = Color.Blue;

}
        public App()
        {
            button = new Button{
                Text = "Click here to start the client",
                BackgroundColor = Color.Red,
                HorizontalOptions = LayoutOptions.Center,
                
            };

    
            button.Clicked +=  OnButtonClicked;
            
            // The root page of your application
            MainPage = new ContentPage {
                Content = new StackLayout {
                    VerticalOptions = LayoutOptions.Center,
                    Children = {
                        button,
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
