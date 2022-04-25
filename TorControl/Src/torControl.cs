using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class TorControl
{
  private static TcpClient tcpClient;
  private static NetworkStream stream;
  private const int bufSize = 1024;
  private static byte[] buf = new byte[bufSize];

  //-----------------------------------------------------------------------
  public static void Execute (string address, int port, string password) 
  {
    tcpClient = new TcpClient(address, port);
    Console.WriteLine("Connected");

    stream = tcpClient.GetStream();

    SendCommand("authenticate \"" + password + "\"");
    Console.WriteLine("[Q]uit [N]ewnym [S]hutdown");

    bool isLoop = true;

    while (isLoop && tcpClient.Connected) {
      string key = Console.ReadKey(true).Key.ToString();
      switch (key) {
        case "Q":
          SendCommand("quit");
          isLoop = false;
          break;

        case "N":
          SendCommand("signal newnym");
          break;

        case "S":
          SendCommand("signal shutdown");
          isLoop = false;
          break;
      }
    }
    tcpClient.Close();
    Console.WriteLine("Disconnected");
  }
  //-----------------------------------------------------------------------
  private static void SendCommand (string data)
  {
    Console.WriteLine(data);
    
    byte[] sendBuffer = Encoding.UTF8.GetBytes(data + "\n");
    stream.Write(sendBuffer, 0, sendBuffer.Length);    

    int rcnt = stream.Read(buf, 0, bufSize);

    if (rcnt > 0) {
      string reply = Encoding.UTF8.GetString(buf, 0, rcnt);
      Console.Write(reply);
    }
  }
}