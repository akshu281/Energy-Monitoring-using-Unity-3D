using UnityEngine;
using System.Collections;
using ChartAndGraph;
using SimpleJSON;

using Newtonsoft.Json.Linq;
using System.Text;
using System;


using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Collections.Generic;

public class StreamingGraph_vol1 : MonoBehaviour
{

    public GraphChart Graph;
    public int TotalPoints = 5;
    float lastTime = 0f;
    float lastX = 0f;

    void Start()
    {
        if (Graph == null) // the ChartGraph info is obtained via the inspector
            return;
        float x = 3f * TotalPoints;
        Graph.DataSource.StartBatch(); // calling StartBatch allows changing the graph data without redrawing the graph for every change
        //Graph.DataSource.ClearCategory("Player 1"); // clear the "Player 1" category. this category is defined using the GraphChart inspector
        Graph.DataSource.ClearCategory("Device 2"); // clear the "Player 2" category. this category is defined using the GraphChart inspector

        /*for (int i = 0; i < TotalPoints; i++)  //add random points to the graph
        {
            //Graph.DataSource.AddPointToCategory("Player 1", System.DateTime.Now - System.TimeSpan.FromSeconds(x), 10); // each time we call AddPointToCategory 
            //Graph.DataSource.AddPointToCategory("Player 2", System.DateTime.Now  - System.TimeSpan.FromSeconds(x), 5); // each time we call AddPointToCategory 
            x -= Random.value * 3f;
            lastX = x;
        }*/

        Graph.DataSource.EndBatch(); // finally we call EndBatch , this will cause the GraphChart to redraw itself
    }

    string ExecuteAndRead(string pCommand, IPAddress ip, int timeout)
    {
        string device_response = "";
        int mSendCommandTimeOut = timeout;
        int mReceiveCommandTimeOut = timeout;
        TcpClient mClient = new TcpClient();
        Debug.Log("in the Execute and Read loop");

        //Set send command timeout
        mClient.SendTimeout = mSendCommandTimeOut;

        //Set receive response timeout
        mClient.ReceiveTimeout = mReceiveCommandTimeOut;

        //Get the message bytes
        byte[] mMessage = Encoding.ASCII.GetBytes(pCommand);

        //Get the bytes of encrypted message
        byte[] mEncryptedMessage = EncryptMessage(mMessage, ProtocolType.Tcp);
        Debug.Log(mEncryptedMessage + "is the Encrypoted Message");
        Debug.Log(mMessage + "is the mMessage");

        //Connect to the device
        IAsyncResult result = mClient.BeginConnect(new IPAddress(ip.GetAddressBytes()), 9999, null, null);
        bool success = result.AsyncWaitHandle.WaitOne(timeout);

        if (success)
        {
            try
            {
                //Send the command
                using (NetworkStream stream = mClient.GetStream())
                {
                    //Write the message
                    stream.Write(mEncryptedMessage, 0, mEncryptedMessage.Length);

                    //Read the response
                    byte[] buffer = new byte[1024];
                    using (MemoryStream ms = new System.IO.MemoryStream())
                    {
                        int numBytesRead;
                        while ((numBytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            ms.Write(buffer, 0, numBytesRead);
                        }
                        device_response = Encoding.ASCII.GetString(DecryptMessage(ms.ToArray(), ProtocolType.Tcp));
                    }

                    stream.Close();
                }
            }
            catch (System.Exception ex)
            {
                // throw new NonCompatibleDeviceException(ex.Message, ex);
                Debug.Log("in the exception loop");
                //System.Console.WriteLine("non compatible device");

            }
            finally
            {
                //Close TCP connection
                if (mClient.Connected)
                {
                    mClient.EndConnect(result);
                }
            }
        }
        else
        {
            
            //System.Console.WriteLine("Unable to connect: to IP address");
        }

        return device_response;
    }

    byte[] EncryptMessage(byte[] pMessage, ProtocolType pProtocolType)
    {
        List<byte> mBuffer = new List<byte>();
        int key = 0xAB;

        if ((pMessage != null) && (pMessage.Length > 0))
        {
            //Añadimos el prefijo del mensaje
            if (pProtocolType == ProtocolType.Tcp)
            {
                mBuffer.Add(0x00);
                mBuffer.Add(0x00);
                mBuffer.Add(0x00);
                mBuffer.Add(0x00);
            }

            //Codificamos el mensaje
            for (int i = 0; i < pMessage.Length; i++)
            {
                byte b = (byte)(key ^ pMessage[i]);
                key = b;
                mBuffer.Add(b);
            }
        }

        return mBuffer.ToArray();
    }

    byte[] DecryptMessage(byte[] pMessage, ProtocolType pProtocolType)
    {
        List<byte> mBuffer = new List<byte>();
        int key = 0xAB;

        //Skip the first 4 bytes in TCP communications (4 bytes header)
        byte header = (pProtocolType == ProtocolType.Udp) ? (byte)0x00 : (byte)0x04;

        if ((pMessage != null) && (pMessage.Length > 0))
        {
            for (int i = header; i < pMessage.Length; i++)
            {
                byte b = (byte)(key ^ pMessage[i]);
                key = pMessage[i];
                mBuffer.Add(b);
            }
        }

        return mBuffer.ToArray();
    }

    void Update()
    {
        float time = Time.time;
        if (lastTime + 2f < time)
        {
            lastTime = time;
            //lastX += Random.value * 3f;
            //string T = "{"emeter":{ "get_realtime":{ "current":0.326835,"voltage":242.341364,"power":36.165750,"total":0.04500}}}";
            //string T = ExecuteAndRead("{ \"emeter\":{ \"get_realtime\":{ } } }", IPAddress.Parse("192.168.137.24"), 10000);
            string J = ExecuteAndRead("{ \"emeter\":{ \"get_realtime\":{ } } }", IPAddress.Parse("192.168.137.23"), 10000);

            //JObject jobject = JObject.Parse(T); 
            JObject jobject1 = JObject.Parse(J);
            //JObject jj = JObject.Parse(J);
            //string power = jobject["emeter"]["get_realtime"]["power"].ToString();
            string power1 = jobject1["emeter"]["get_realtime"]["power"].ToString();
           
            //string Current = jobject["emeter"]["get_realtime"]["current"].ToString();
            string Current1 = jobject1["emeter"]["get_realtime"]["current"].ToString();
            //float cur = float.Parse(Current);
            float cur1 = float.Parse(Current1);

            //string Voltage = jobject["emeter"]["get_realtime"]["voltage"].ToString();
            string Voltage1 = jobject1["emeter"]["get_realtime"]["voltage"].ToString();
            //float vol = float.Parse(Voltage);
            float vol1 = float.Parse(Voltage1);
            
            //Graph.DataSource.AddPointToCategoryRealtime("Player 1", System.DateTime.Now, vol); // each time we call AddPointToCategory 
            Graph.DataSource.AddPointToCategoryRealtime("Device 2", System.DateTime.Now, vol1);
            //Graph.DataSource.AddPointToCategoryRealtime("Player 2", System.DateTime.Now, vol); // each time we call AddPointToCategory 
            //Graph.DataSource.AddPointToCategoryRealtime("Player 2", System.DateTime.Now, 5); // each time we call AddPointToCategory
        }

    }
}
