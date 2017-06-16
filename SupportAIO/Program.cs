using System;
using System.IO;
using System.Net;
using System.Reflection;
using EloBuddy.SDK.Events;


namespace SupportAIO
{
    class Program
    {
        private static string dllPath = @"C:\Users\" + Environment.UserName + @"\AppData\Roaming\EloBuddy\Addons\Libraries\SupportAIO.dll";

        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += delegate (EventArgs args2)
            {
                if (!File.Exists(dllPath))
                {
                    using (var wc = new WebClient())
                    {
                        wc.DownloadFile("https://github.com/Toyota7/EloBuddy/raw/master/SupportAIO.dll", dllPath);
                    }
                }

                Assembly SampleAssembly = Assembly.LoadFrom(dllPath);
                Type myType = SampleAssembly.GetType("SupportAIO.Program");

                var main = myType.GetMethod("OnLoad", BindingFlags.NonPublic | BindingFlags.Static);
                                                                                                       
                if (main == null) Console.WriteLine("Method Not Found!");

                main.Invoke(Activator.CreateInstance(myType), null); 
            };
        }
    }
}
