using System;
using System.Collections;
using System.Text;

namespace DigiProofs.Logger {
    /// <summary>
    /// Simple logger for the DigiProofs Uploader
    /// </summary>
    public class LogEntry {
        private DateTime timeStamp;
        private string message;
        private string detail;

        public LogEntry(string message, string detail) {

            this.message = message;
            this.detail = detail;
            this.timeStamp = DateTime.Now;
        }

        public override string ToString() {
            return timeStamp.ToShortDateString() + " " + timeStamp.ToLongTimeString() + " " + message + Environment.NewLine + detail;
        }
    }

    public class LogList : ArrayList {
        public override string ToString() {
            StringBuilder result = new StringBuilder();
            foreach (LogEntry entry in this) {
                result.Append(entry + Environment.NewLine);
            }
            return result.ToString();
        }
    }
}