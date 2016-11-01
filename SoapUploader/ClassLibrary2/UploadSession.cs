using System;
using System.Net;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;
using System.Diagnostics;
using System.Xml.Serialization;
using System.Web.Services.Protocols;
using System.Web.Services;

namespace DigiProofs.SoapUpload {
    public delegate void DPDoneHandler(object result, object state, SessionException e);

    //
    // This object holds the objects needed to handle an async return.
    public class AsyncObj {
	public DP dp;
	public DPDoneHandler callback;
	public object state;
	public AsyncObj(DP dp, DPDoneHandler callback, object state) {
	    this.dp = dp;
	    this.callback = callback;
	    this.state = state;
	}
    }

    // These are the errors that may occur during communication with the upload server.
    public enum SessionError {
	LoginFail,	    // Server doesn't recognize email or password
	InvalidSession,	    // SessionID doesn't match current that of server.
	InvalidFile,	    // Some problem opening or scaling an image file
	InvalidImage,	    // Server didn't like image.
	EventExpired,	    // This event has expired.
	PageFull,	    // Page was too full for upload
	EventFull,	    // Event has no more room for images.
	NetworkError,	    // Any network error
	ServerError,	    // Any error on the digiproofs servers
	UnknownSoapError,   // An unexpected SoapException occured
	UnknownError,	    // An unexpected error has occurred.
    };

    // This exception should handle all expected errors from the netsession
    public class SessionException : System.ApplicationException {
	public SessionError Error {
	    get {return _sessionError;}
	}
	private SessionError _sessionError;

	public SessionException(string reason, SessionError error): base(reason){
	    _sessionError = error;
	}
	public SessionException(string reason, SessionError error, Exception inner): base(reason, inner){
	    _sessionError = error;
	}
    }

    /// <summary>
    /// DigiProofs Network  Session
    /// This represents a series of connections used to upload images.
    /// First a NetSession is created with a server, email, and password.
    /// Then Login() is called to get the sessionID and current XML state
    /// of the various events.
    /// </summary>
    public class NetSession {
	private string HTTPurl;
	private string HTTPSurl;
	private string proxy;
	private string email;
	private string password;
	private Session session;
	private bool forceHTTP10;

	private Hashtable eventHash = new Hashtable();
	private Hashtable pageHash = new Hashtable();

	private LogList logs;

	public Event[] EventList {
	    get {
		if (session == null) 
		    throw new SessionException("Not Logged In", SessionError.LoginFail);
		return session.EventList;
	    }
	}
	
	public string Email {
	    get {return email;}
	}
	public string Password {
	    get {return password;}
	}
	public bool LoggedIn {
	    get {return loggedIn;}
	}
	private bool loggedIn;

	public NetSession(string host, string email, string password, string proxy, bool forceHTTP10) {
	    this.email = email;
	    this.password = password;
	    HTTPurl = "http://" + host + "/soap/DP.pl";
	    HTTPSurl = "https://" + host + "/soap/DP.pl";
	    this.proxy = proxy;
	    this.forceHTTP10 = forceHTTP10;
	    //HTTPurl = "http://" + host + "/WebService1/DP.asmx";
	    //HTTPSurl = "http://" + host + "/WebService1/DP.asmx";
	    loggedIn = false;
	    logs = new LogList();
	    string vars = "  http URL: " + HTTPurl + Environment.NewLine + "  https URL: " + HTTPSurl + Environment.NewLine + "  Proxy: " + proxy + Environment.NewLine + "  email: " + email;
	    logs.Add(new LogEvent("Session Created", vars));
	}

	//
	// Update the values in this session.
	// Resets everything but the logs.
	//
	public void Update(string host, string email, string password) {
	    this.email = email;
	    this.password = password;
	    HTTPurl = "http://" + host + "/soap/DP.pl";
	    HTTPSurl = "https://" + host + "/soap/DP.pl";
	    //HTTPurl = "http://" + host + "/WebService1/DP.asmx";
	    //HTTPSurl = "http://" + host + "/WebService1/DP.asmx";
	    loggedIn = false;
	    session = null;
	    eventHash.Clear();
	    pageHash.Clear();
	    string vars = "  http URL: " + HTTPurl + Environment.NewLine + "  https URL: " + HTTPSurl + Environment.NewLine + "  Proxy: " + proxy + Environment.NewLine + "  email: " + email;
	    logs.Add(new LogEvent("Session parameters changed", vars));
	}

	public string GetLogs() {
	    return logs.ToString();
	}

	public void Login() {
	    try {
		ServicePointManager.CertificatePolicy = new MyCertificateValidation();
		DP dp = new DP(HTTPSurl, proxy, forceHTTP10);
		//dp.Proxy = new WebProxy("http://localhost:8080", false);
		session = dp.Login(email, password);
		logs.Add(new LogEvent("Successful login to " + email, ""));
		foreach (Event ev in session.EventList) {
		    eventHash.Add(ev.eventID, ev);
		    foreach (Page pg in ev.pageList)
			pageHash.Add(pg.pageID, pg);
		}
		loggedIn = true;
	    }
	    catch (SoapException e) {
		if (e.Code.Name == "Client.InvalidLogin") {
		    logs.Add(new LogEvent("Invalid login to " + email, e.Message));
		    loggedIn = false;
		    throw new SessionException(e.ToString(), SessionError.LoginFail, e);
		}
		else {
		    logs.Add(new LogEvent("Unexpected error during login to " + email, e.Message));
		    loggedIn = false;
		    throw new SessionException(e.ToString(), SessionError.UnknownSoapError, e);
		}
	    }
	    catch (System.Net.WebException e) {
		logs.Add(new LogEvent("Network connection error during login to " + email, e.Message));
		loggedIn = false;
		throw new SessionException(e.ToString(), SessionError.NetworkError, e);
	    }

	    catch (System.InvalidOperationException e) {
		logs.Add(new LogEvent("An InvalidOperationException occurred during login to " + email, e.Message));
		loggedIn = false;
		throw new SessionException(e.ToString(), SessionError.ServerError, e);
	    }

	    catch (Exception e) {
		logs.Add(new LogEvent("Unexpected error during login to " + email, e.Message));
		loggedIn = false;
		throw new SessionException(e.ToString(), SessionError.UnknownError, e);
	    }
	}

	//
	// Create a new page for the current event and session
	//
	public int NewPage(int eventID, string title, string description, DPDoneHandler callback, object state) {
	    if (callback == null) {
		try {
		    DP dp = new DP(HTTPurl, proxy, forceHTTP10);
		    //dp.Proxy = new WebProxy("http://localhost:8080", false);
		    Page page = dp.NewPage(session.SessionKey, eventID, title, description);
		    pageHash.Add(page.pageID, page);
		    logs.Add(new LogEvent("New Page created: " + page.title + "(" + page.pageID + ")", ""));
		    return page.pageID;
		}
		catch (SoapException e) {
		    if (e.Code.Name == "Client.AuthError") {
			logs.Add(new LogEvent("Invalid session ID", e.Message));
			throw new SessionException(e.ToString(), SessionError.InvalidSession, e);
		    }
		    else if (e.Code.Name == "Client.InternalError") {
			logs.Add(new LogEvent("Internal Error", e.Message));
			throw new SessionException(e.Message, SessionError.ServerError, e);
		    }
		    else {
			logs.Add(new LogEvent("Unexpected soap error during new page creation", e.Message));
			throw new SessionException(e.ToString(), SessionError.UnknownSoapError, e);
		    }
		}
		catch (System.Net.WebException e) {
		    logs.Add(new LogEvent("Network connection error during new page creation", e.Message));
		    throw new SessionException(e.ToString(), SessionError.NetworkError, e);
		}

		catch (System.InvalidOperationException e) {
		    logs.Add(new LogEvent("An InvalidOperationException occurred during new page creation", e.Message));
		    throw new SessionException(e.ToString(), SessionError.ServerError, e);
		}

		catch (Exception e) {
		    logs.Add(new LogEvent("Unexpected error during new page creation.", e.Message));
		    throw new SessionException(e.ToString(), SessionError.UnknownError, e);
		}
	    }
	    else {
		DP dp = new DP(HTTPurl, proxy, forceHTTP10);
		AsyncObj ao = new AsyncObj(dp, callback, state);
		dp.BeginNewPage(session.SessionKey, eventID, title, description, new AsyncCallback(this.NewPageComplete), ao);
		return 0;
	    }
	}

	private void NewPageComplete(IAsyncResult ar) {
	    AsyncObj ao = (AsyncObj)ar.AsyncState;
	    try {
		Page page = ao.dp.EndNewPage(ar);
		pageHash.Add(page.pageID, page);
		logs.Add(new LogEvent("New Page created: " + page.title + "(" + page.pageID + ")", ""));
		ao.callback(page.pageID, ao.state, null);
	    }
	    catch (SoapException e) {
		switch (e.Code.Name) {
		    case "Client.AuthError":
			logs.Add(new LogEvent("Invalid session ID", e.Message));
			ao.callback(null, ao.state, new SessionException(e.ToString(), SessionError.InvalidSession, e));
			break;
		    case "Client.InternalError":
			logs.Add(new LogEvent("Internal Error", e.Message));
			ao.callback(null, ao.state, new SessionException(e.Message, SessionError.ServerError, e));
			break;
		    default:
			logs.Add(new LogEvent("Unexpected soap error during new page creation", e.Message));
			ao.callback(null, ao.state, new SessionException(e.ToString(), SessionError.UnknownSoapError, e));
			break;
		}
	    }
	    catch (System.Net.WebException e) {
		logs.Add(new LogEvent("Network connection error during new page creation", e.Message));
		ao.callback(null, ao.state, new SessionException(e.ToString(), SessionError.NetworkError, e));
	    }

	    catch (System.InvalidOperationException e) {
		logs.Add(new LogEvent("An InvalidOperationException occurred during new page creation", e.Message));
		ao.callback(null, ao.state, new SessionException(e.ToString(), SessionError.ServerError, e));
	    }

	    catch (Exception e) {
		logs.Add(new LogEvent("Unexpected error during new page creation.", e.Message));
		ao.callback(null, ao.state, new SessionException(e.ToString(), SessionError.UnknownError, e));
	    }
	}

	public string Upload(int pageID, string filename, int compression, DPDoneHandler callback, object state) {
	    ImageProcess imageData;
	    long fileLength;
	    try {
		fileLength = new FileInfo(filename).Length;
		imageData = new ImageProcess(filename, compression, Int32.Parse(session.MaxSize));
	    }
	    catch (Exception e) {
		logs.Add(new LogEvent("Error processing image file: " + filename, e.Message));
		throw new SessionException(e.ToString(), SessionError.InvalidFile, e);
	    }
	    if (callback == null) {
		MemoryStream stream;
		byte[] buffer = null;
		try {
		    stream = imageData.GetImageStream();
		    DP dp = new DP(HTTPurl, proxy, forceHTTP10);
		    //dp.Proxy = new WebProxy("http://localhost:8080", false);
		    string imageID = dp.Upload(session.SessionKey, pageID, "", filename, stream.GetBuffer());
		    logs.Add(new LogEvent("Upload Complete: " + imageID, ""));
		    return imageID;
		}
		catch (Exception e) {
		    logs.Add(new LogEvent("Error processing image file:" + filename, e.Message));
		    if (compression > 0) {
			logs.Add(new LogEvent("Attempting to copy uncompressed image: " + filename, ""));
			try {
			    imageData.setCompression(0);
			    stream = imageData.GetImageStream();
			    buffer = stream.ToArray();
			    stream.Close();
			}
			catch (Exception e2) {
			    logs.Add(new LogEvent("Error processing uncompressed image file:" + filename, e2.Message));
			    throw new SessionException(e.ToString(), SessionError.InvalidFile, e2);
			}
			DP dp = new DP(HTTPurl, proxy, forceHTTP10);
			//dp.Proxy = new WebProxy("http://localhost:8080", false);
			string imageID = dp.Upload(session.SessionKey, pageID, "", filename, stream.GetBuffer());
			logs.Add(new LogEvent("Upload Complete: " + imageID, ""));
			return imageID;
		    }
		}
	    }
	    else {
		DP dp = new DP(HTTPurl, proxy, forceHTTP10);
		byte[] buffer = null;
		MemoryStream stream;
		int CurrentCompression = compression;
		try {
		    stream = imageData.GetImageStream();
		    buffer = stream.ToArray();
		    stream.Close();
		}
		catch (Exception e) {
		    logs.Add(new LogEvent("Error processing image file:" + filename, e.Message));
		    if (compression > 0) {
			logs.Add(new LogEvent("Attempting to copy uncompressed image: " + filename, ""));
			try {
			    CurrentCompression = 0;
			    imageData.setCompression(0);
			    stream = imageData.GetImageStream();
			    buffer = stream.ToArray();
			    stream.Close();
			}
			catch (Exception e2) {
			    logs.Add(new LogEvent("Error processing uncompressed image file:" + filename, e2.Message));
			    throw new SessionException(e.ToString(), SessionError.InvalidFile, e2);
			}
		    }
		}
		AsyncObj ao = new AsyncObj(dp, callback, state);
		logs.Add(new LogEvent("Upload Started: " + filename,
				      "File Length: " + fileLength + Environment.NewLine +
				      "Compressed(" + CurrentCompression + ") Length: " + buffer.Length + Environment.NewLine));
		dp.BeginUpload(session.SessionKey, pageID, "", filename, buffer, new AsyncCallback(this.UploadComplete), ao);
		
		return null;
	    }
	    return null;
	}

	private void UploadComplete(IAsyncResult ar) {
	    AsyncObj ao = (AsyncObj)ar.AsyncState;
	    try {
		string imageID = ao.dp.EndUpload(ar);
		logs.Add(new LogEvent("Upload Complete: " + imageID, ""));
		ao.callback(imageID, ao.state, null);
	    }
	    catch (SoapException e) {
		if (e.Code.Name == "Client.AuthError") {
		    logs.Add(new LogEvent("Invalid session ID on Image Upload", e.Message));
		    ao.callback(null, ao.state, new SessionException(e.ToString(), SessionError.InvalidSession, e));
		}
		else if (e.Code.Name == "Client.InternalError") {
		    logs.Add(new LogEvent("Internal Error on Image Upload", e.Message));
		    ao.callback(null, ao.state, new SessionException(e.Message, SessionError.ServerError, e));
		}
		else if (e.Code.Name == "Client.Expired") {
		    logs.Add(new LogEvent("Event Expired on Image Upload", e.Message));
		    ao.callback(null, ao.state, new SessionException(e.Message, SessionError.EventExpired, e));
		}
		else if (e.Code.Name == "Client.PageFull") {
		    logs.Add(new LogEvent("Upload Failed - Page Full", e.Message));
		    ao.callback(null, ao.state, new SessionException(e.Message, SessionError.PageFull, e));
		}
		else if (e.Code.Name == "Client.EventFull") {
		    logs.Add(new LogEvent("Upload Failed - Event Full", e.Message));
		    ao.callback(null, ao.state, new SessionException(e.Message, SessionError.EventFull, e));
		}
		else if (e.Code.Name == "Client.InvalidImage") {
		    logs.Add(new LogEvent("Upload Failed - Image is not valid", e.Message));
		    ao.callback(null, ao.state, new SessionException(e.Message, SessionError.InvalidImage, e));
		}
		else {
		    logs.Add(new LogEvent("Unexpected soap error during new page creation", e.Message));
		    ao.callback(null, ao.state, new SessionException(e.ToString(), SessionError.UnknownSoapError, e));
		}
	    }
	    catch (System.Net.WebException e) {
		logs.Add(new LogEvent("Network connection error during image upload", e.Message));
		ao.callback(null, ao.state, new SessionException(e.ToString(), SessionError.NetworkError, e));
	    }

	    catch (System.InvalidOperationException e) {
		logs.Add(new LogEvent("An InvalidOperationException occurred during image upload", e.Message));
		ao.callback(null, ao.state, new SessionException(e.ToString(), SessionError.ServerError, e));
	    }

	    catch (Exception e) {
		logs.Add(new LogEvent("Unexpected error during image upload.", e.Message));
		ao.callback(null, ao.state, new SessionException(e.ToString(), SessionError.UnknownError, e));
	    }
	}

	public override string ToString() {
	    System.Text.StringBuilder res = new System.Text.StringBuilder();
	    String nl = Environment.NewLine;

	    res.AppendFormat("SessionID: {0}{1}", session.SessionKey, nl);
	    res.AppendFormat("MaxImageSize: {0}{1}", session.MaxSize, nl);
	    foreach (Event ev in session.EventList) {
		res.AppendFormat("Event {0}:{1}  Title: {2}{1}  MaxImages: {3}{1}  CurrentImages: {4}{1}",
		    ev.eventID, nl, ev.title, ev.maxImages, ev.currentImages);
		foreach (Page pg in ev.pageList) {
		    res.AppendFormat("  Page {0}:{1}    Title: {2}{1}    MaxImages: {3}{1}    CurrentImages: {4}{1}",
			pg.pageID, nl, pg.title, pg.maxImages, pg.currentImages);
		}
	    }
	    return res.ToString();
	}
    }

    //
    // This overrides the default certificate validation proceedure
    // to allow us to use the bogus certificate.
    // At some point, it should probably be modified to only allow
    // CertUNTRUSTEDTESTROOT through.
    //
    public  enum    CertificateProblem  : long {
	CertEXPIRED                   = 0x800B0101,
	CertVALIDITYPERIODNESTING     = 0x800B0102,
	CertROLE                      = 0x800B0103,
	CertPATHLENCONST              = 0x800B0104,
	CertCRITICAL                  = 0x800B0105,
	CertPURPOSE                   = 0x800B0106,
	CertISSUERCHAINING            = 0x800B0107,
	CertMALFORMED                 = 0x800B0108,
	CertUNTRUSTEDROOT             = 0x800B0109,
	CertCHAINING                  = 0x800B010A,
	CertREVOKED                   = 0x800B010C,
	CertUNTRUSTEDTESTROOT         = 0x800B010D,
	CertREVOCATION_FAILURE        = 0x800B010E,
	CertCN_NO_MATCH               = 0x800B010F,
	CertWRONG_USAGE               = 0x800B0110,
	CertUNTRUSTEDCA               = 0x800B0112
    }
    public class MyCertificateValidation : ICertificatePolicy {
	// Default policy for certificate validation.
	public static bool DefaultValidate = true; 
	public bool CheckValidationResult(ServicePoint sp, X509Certificate cert,
	    WebRequest request, int problem) {        
	    bool ValidationResult=false;
	    //Console.WriteLine("Certificate Problem with accessing " +
	    //	request.RequestUri);
	    //Console.Write("Problem code 0x{0:X8},",(int)problem);
	    //Console.WriteLine(GetProblemMessage((CertificateProblem)problem));
	    ValidationResult = DefaultValidate;
	    return ValidationResult; 
	}
            
	private String GetProblemMessage(CertificateProblem Problem) {
	    String ProblemMessage = "";
	    CertificateProblem problemList = new CertificateProblem();
	    String ProblemCodeName = Enum.GetName(problemList.GetType(),Problem);
	    if(ProblemCodeName != null)
		ProblemMessage = ProblemMessage + "-Certificateproblem:" +
		    ProblemCodeName;
	    else
		ProblemMessage = "Unknown Certificate Problem";
	    return ProblemMessage;
	}
    }
}
