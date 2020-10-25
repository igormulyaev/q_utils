using System;
using System.Net.Http;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

class QExec 
{
  //-----------------------------------------------------------------------
  private static string url = "ivwqq9.-anvrtcom{-sw";
  public static string salt = "5v76PIhVm8";
  private const int qIdLength = 20;

  private static string qId;
  private static HttpClient httpClient;
  private static QUser qUser;

  //-----------------------------------------------------------------------
  public static void Execute (string[] args) 
  {
    url = ConvertString (url);
    salt = ConvertString (salt);

    SetQId(args[0]);

    httpClient = new HttpClient 
    { 
      BaseAddress = new Uri (url)
    };

    GetUser();

    for (int i = 1; i < args.Length; ++i)
    {
      bool ok;

      string cmd = args[i].ToUpper();

      Console.WriteLine("Command: {0}", cmd);

      switch (cmd[0])
      { 
        case 'T': // Trial
          ok = ActivateTrial();
          break;

        case 'V': // Version
          GetVersions();
          ok = true;
          break;

        case 'B': // Bot
        case 'F': // Flash
        case 'H': // Holder
          if (i + 2 < args.Length)
          { 
            ok = DownloadProgram (cmd[0], args[i + 1], args[i + 2]);
            i += 2;
          }
          else
          {
            Console.WriteLine ("Error: wrong arguments");
            ok = false;
          }
          break;

        case 'S': // Scripts
          if (i + 1 < args.Length)
          { 
            ok = DownloadScripts (args[i + 1]);
            i += 1;
          }
          else
          {
            Console.WriteLine ("Error: wrong arguments");
            ok = false;
          }
          break;

        default:
          Console.WriteLine ("Error: unknown command");
          ok = false;
          break;
      }

      if (!ok) 
      {
        break;
      }
    }
  }
  //-----------------------------------------------------------------------
  private static void GetUser () 
  {
    Task<string> answ = HttpGetPost("/api/users/?userId=" + qId + "&hash=" + MD5Hash(qId + "|" + salt));
    answ.Wait();
    string res = answ.Result;
    JObject jReply = JObject.Parse(res);

    qUser = QUser.LoadUser(jReply);

    qUser.PrintUser();
  }

  //-----------------------------------------------------------------------
  private static bool ActivateTrial () 
  {
    bool ok = false;

    if (qUser.active)
    {
      Console.WriteLine("Error: user always active");
    }
    else if (qUser.trial)
    {
      Console.WriteLine("Error: trial always active");
    }
    else if (qUser.trialUse)
    {
      Console.WriteLine("Error: trial always used");
    }
    else
    { 
      Task<string> answ = HttpGetPost("/api/users/trial/", "userId=" + qId + "&hash=" + MD5Hash(qId + '|' + salt));
      answ.Wait();
      string res = answ.Result;
      Console.WriteLine("{0}\n", res);
      
      GetUser();

      ok = true;
    }
    return ok;
  }
  //-----------------------------------------------------------------------
  private static void GetVersions () 
  {
    for (int i = 1; i <= 3; ++i)
    { 
      Task<string> answ = HttpGetPost("/api/managment/program?programId=" + i);
      answ.Wait();
      JObject jReply = JObject.Parse(answ.Result);
      Console.WriteLine("{0}: {1}", jReply["name"], jReply["version"]);
    }
  }

  //-----------------------------------------------------------------------
  private static bool DownloadProgram (char cmd, string version, string filename)
  {
    bool rc = false;

    if (qUser.active)
    {
      int programId = cmd == 'B'? 1: cmd == 'F'? 2: 3;

      Task<string> answ = HttpGetPost ("/api/managment/update?userId=" + qId + "&programId=" + programId + "&version=" + version + "&hash=" + MD5Hash(qId + '|' + programId + '|' + version + '|' + salt));
      answ.Wait ();
      File.WriteAllBytes (filename, Convert.FromBase64String (answ.Result));

      Console.WriteLine ("{0} downloaded", cmd);
      rc = true;
    }
    else
    {
      Console.WriteLine ("Error: User is not active");
    }
    return rc;
  }

  //-----------------------------------------------------------------------
  private static bool DownloadScripts (string filename)
  {
    bool rc = false;

    if (qUser.active)
    {
      Task<string> answ = HttpGetPost ("/api/managment/scripts?userId=" + qId + "&hash=" + MD5Hash(qId + '|' + salt));
      answ.Wait();

      string scripts = answ.Result;
      using (FileStream zipToOpen = File.Create(filename))
      {
        using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
        {
          int pos = 0;
          string key = "";
          string value = "";
          ZipArchiveEntry zEntry = null;

          while (true)
          {
            if (GetKeyValue(scripts, ref pos, ref key, ref value))
            {
              Console.WriteLine("{0} = {1}", key, (key == "content"? ("<" + value.Length.ToString() + " chars>"): value));
              if (key == "title")
              {
                zEntry = archive.CreateEntry(value + ".js");
              }
              else if (key == "content")
              {
                using (StreamWriter writer = new StreamWriter(zEntry.Open()))
                {
                  writer.WriteLine(value);
                }
              }
            }
            else
            {
              break;
            }
          };
        }
      }
      rc = true;
    }
    else
    {
      Console.WriteLine ("Error: User is not active");
    }
    return rc;
  }

  //-----------------------------------------------------------------------
  private static async Task<string> HttpGetPost (string urlExt, string postData = null) 
  { 
    HttpResponseMessage response;

    if (String.IsNullOrEmpty(postData))
    {
      response = await httpClient.GetAsync(urlExt);
    }
    else
    {
      StringContent content = new StringContent(postData, Encoding.UTF8, "application/x-www-form-urlencoded");
      response = await httpClient.PostAsync(urlExt, content);
    }

    response.EnsureSuccessStatusCode();
    var res = await response.Content.ReadAsStringAsync();
    response.Dispose();
    return res;
  }

  //-----------------------------------------------------------------------
  static private void SetQId (string s) 
  {
    if (s.Length != qIdLength || !IsHex(s)) 
    {
      qId = MD5Hash(s).Substring(0, qIdLength);
    }
    else 
    {
      qId = s;
    }
    Console.WriteLine("qId = {0}\n", qId);
  }

  //-----------------------------------------------------------------------
  public static string MD5Hash (string s) 
  {
    string result;

    using (MD5 md5 = MD5.Create()) {
      byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(s));

      StringBuilder sb = new StringBuilder(hash.Length * 2);
      foreach (byte b in hash) {
        sb.AppendFormat("{0:x2}", b);
      }
      result = sb.ToString();
    }

    return result;
  }

  //-----------------------------------------------------------------------
  private static string ConvertString(string s) 
  {
    var sb = new StringBuilder();
    int k = 0;
    foreach (byte b in Encoding.ASCII.GetBytes(s)) 
    {
      ++k;
      sb.Append(Convert.ToChar(b ^ k));
      k %= 3;
    }
    return sb.ToString();
  }

  //-----------------------------------------------------------------------
  private static bool IsHex(string s) 
  {
    bool isHex = true; 
    foreach(char c in s) 
    {
      isHex &= ((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f'));
    }
    return isHex;
  }

  //-----------------------------------------------------------------------
  static private bool GetKeyValue (string src, ref int pos, ref string key, ref string value)
  {
    char[] endChars = {',', '}'};

    bool rc = false;

    if (pos >= 0)
    {
      pos = src.IndexOf('"', pos) + 1;
      if (pos > 0)
      { 
        int posEndKey = src.IndexOf('"', pos);

        if (posEndKey >= 0)
        {
          key = src.Substring(pos, posEndKey - pos);

          pos = src.IndexOf(':', pos) + 1;
          if (pos > 0)
          {
            bool quoted = (src[pos] == '"');
            int posEndValue = -1;

            if (quoted)
            {
              ++ pos;
              posEndValue = src.IndexOf('"', pos);
            }
            else
            {
              posEndValue = src.IndexOfAny(endChars, pos);
            }

            if (posEndValue >= 0)
            {
              value = src.Substring(pos, posEndValue - pos);
              pos = posEndValue + 1;
              rc = true;
            }
          }
        }
      }
    }
    return rc;
  }
  //-----------------------------------------------------------------------
}