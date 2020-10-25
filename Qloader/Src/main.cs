using System;
using System.Text;


class CMain 
{
  //-----------------------------------------------------------------------
  static public void Main (string[] args) 
  {
    if (args.Length > 0) 
    {
      QExec.Execute(args);
    }
    else 
    {
      helpOut();
    }
  }

  //-----------------------------------------------------------------------
  static private void helpOut () 
  {
    Console.WriteLine("Qloader {id | seed} [commands]");
    Console.WriteLine("Commands:");
    Console.WriteLine("  T[rial] - activate trial period");
    Console.WriteLine("  V[ersion] - get bot's versions");
    Console.WriteLine("  B[ot] <version> <filename.zip> - download Bot");
    Console.WriteLine("  F[lash} <version> <filename.zip> - download Flash");
    Console.WriteLine("  H[older] <version> <filename.zip> - download Holder");
    Console.WriteLine("  S[cripts] <filename.zip> - download scripts");
  }
}