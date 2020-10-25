using System;
using Newtonsoft.Json.Linq;

class QUser 
{
  public string id;
  public string pcid;
  public string usbId;
  public string lvl;
  public DateTime subscriptionDate;
  public DateTime subscriptionExpDate;
  public bool active;
  public bool trialUse;
  public bool trial;
  public bool unlimitedSub;
  public string hash;

  public bool isHashOk;

  //-----------------------------------------------------------------------
  public static QUser LoadUser(JObject jReply) 
  {
    QUser res = new QUser();

    res.id = (string)jReply["user"]["id"];
    res.pcid = (string)jReply["user"]["pcid"];
    res.usbId = (string)jReply["user"]["usbId"];
    res.lvl = (string)jReply["user"]["lvl"];
    res.subscriptionDate = (DateTime)jReply["user"]["subscriptionDate"];
    res.subscriptionExpDate = (DateTime)jReply["user"]["subscriptionExpDate"];
    res.active = (bool)jReply["user"]["active"];
    res.trialUse = (bool)jReply["user"]["trialUse"];
    res.trial = (bool)jReply["user"]["trial"];
    res.unlimitedSub = (bool)jReply["user"]["unlimitedSub"];
    res.hash = (string)jReply["hash"];

    string checkHash = QExec.MD5Hash(res.pcid + '|'
       + res.subscriptionExpDate.ToString("dd.MM.yyyy") + '|'
       + res.unlimitedSub.ToString().ToLower() + '|'
       + res.trial.ToString().ToLower() + '|'
       + res.lvl + '|'
       + res.active.ToString().ToLower() + '|'
       + QExec.salt);

    res.isHashOk = (res.hash == checkHash);

    return res;
  }

  //-----------------------------------------------------------------------
  public void PrintUser() 
  {
    Console.WriteLine("User config:");
    Console.WriteLine("id = {0}", id);
    Console.WriteLine("pcid = {0}", pcid);
    Console.WriteLine("usbId = {0}", usbId);
    Console.WriteLine("lvl = {0}", lvl);
    Console.WriteLine("subscriptionDate = {0}", subscriptionDate);
    Console.WriteLine("subscriptionExpDate = {0}", subscriptionExpDate);
    Console.WriteLine("active = {0}", active);
    Console.WriteLine("trialUse = {0}", trialUse);
    Console.WriteLine("trial = {0}", trial);
    Console.WriteLine("unlimitedSub = {0}", unlimitedSub);
    Console.WriteLine("hash = {0} ({1})\n", hash, isHashOk? "Ok": "Wrong");
  }
}
