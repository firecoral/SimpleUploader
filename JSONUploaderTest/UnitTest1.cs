using System;
using System.Diagnostics;
using System.Threading.Tasks;
using DigiProofs.JSONUploader;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JSONUploaderTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1() {
            Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
            RunAsync().Wait();
        }
        static async Task RunAsync() {
            NetSession session;
            bool forceHTTP10 = false;
            string proxy = null;
            try {
                session = new NetSession("cdev.digiproofs.com", "cself@digiproofs.com", "Token", proxy, forceHTTP10);
                Debug.WriteLine("testing GetToken");
                string token = await session.GetToken("2081");
                Console.WriteLine("token: {0}", session.UploadToken);
                EventList eventList = await session.GetEventList();
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }


        }
    }
}
