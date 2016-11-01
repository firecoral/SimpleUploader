using System;
using System.Collections;
using System.Text;

namespace DigiProofs.JSONUploader
{
    /// <summary>
    /// Summary description for Class1.
    /// </summary>
    public class LogEvent {
	private DateTime timeStamp;
	private string message;
	private string detail;

	public LogEvent(string message, string detail) {

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
	    foreach (LogEvent ev in this) {
		result.Append(ev + Environment.NewLine);
	    }
	    return result.ToString();
	}
    }
}
