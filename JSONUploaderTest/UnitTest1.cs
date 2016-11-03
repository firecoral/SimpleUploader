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
            string proxy = null;
            try {
                session = new NetSession("cdev.digiproofs.com", "cself@digiproofs.com", "Token", proxy);
                Debug.WriteLine("testing GetToken");
                await session.GetToken("2081");
                Console.WriteLine("token: {0}", session.UploadToken);
                await session.GetEventList();
                foreach (Event ev in session.EventList) {
                    Console.WriteLine("{0}: {1}", ev.id, ev.title);
                }
                if (session.EventList.Length == 0) {
                    Console.WriteLine("No Events:  Stopping testing");
                    Environment.Exit(0);
                }
                string event_id = session.EventList[0].id;
                string page_id = await session.NewPage(event_id, "New Page: Windows Testing");
                Console.WriteLine("Added new page: {0} to event {1}", page_id, event_id);
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }


        }
    }
}
