using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using DigiProofs.JSONUploader;
using DigiProofs.Logger;
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
            LogList log = new LogList();
            log.Add(new LogEntry("Running Test1", ""));
            try {
                session = new NetSession(log, "cdev.digiproofs.com", "cself@digiproofs.com", "Token", proxy);
                log.Add(new LogEntry("Testing GetToken", ""));
                string token = await session.GetToken("2081");
                log.Add(new LogEntry("Got token:" + token, ""));
                log.Add(new LogEntry("Testing GetEventList", ""));
                await session.GetEventListAsync();
                foreach (Event ev in session.EventList) {
                    Console.WriteLine("{0}: {1}", ev.event_id, ev.title);
                }
                if (session.EventList.Length == 0) {
                    log.Add(new LogEntry("No Events: Stopping the test", ""));
                }
                else {
                    int event_id = session.EventList[0].event_id;
                    log.Add(new LogEntry("Testing NewPage", ""));
                    int page_id = await session.NewPage(event_id, "New Page: Windows Testing");
                    log.Add(new LogEntry(String.Format("Added new page: {0} to event {1}", page_id, event_id), ""));

                    log.Add(new LogEntry("Testing Upload", ""));
                    string filename = "C:\\test1.jpg";
                    Stream image = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
                    string image_id = await session.Upload(page_id, "test1.jpg", image);
                    log.Add(new LogEntry(String.Format("Uploaded image: {0}", image_id), ""));
                }
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
            finally {
                Console.Write(log.ToString());
            }


        }
    }
}
