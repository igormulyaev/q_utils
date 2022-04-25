using System;
using System.Text;


class CMain 
{
  //-----------------------------------------------------------------------
  static public void Main (string[] args) 
  {
    int port;

    if (args.Length == 3 && int.TryParse(args[1], out port)) {
      string address = args[0];
      string password = args[2];
      TorControl.Execute(address, port, password);
    }
    else {
      helpOut();
    }
  }

  //-----------------------------------------------------------------------
  static private void helpOut () 
  {
    Console.WriteLine("Usage: TorControl <address> <port> <password>");
  }
}