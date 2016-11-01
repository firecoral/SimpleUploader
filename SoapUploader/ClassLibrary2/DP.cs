// 
// This source code was auto-generated by wsdl, Version=1.1.4322.573.
// 
using System.Diagnostics;
using System.Xml.Serialization;
using System;
using System.Web.Services.Protocols;
using System.ComponentModel;
using System.Web.Services;
using System.Net;


namespace DigiProofs.SoapUpload {
    /// <remarks/>
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Web.Services.WebServiceBindingAttribute(Name="DPSoap", Namespace="urn:DP")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(Event[]))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(object[]))]
    public class DP : System.Web.Services.Protocols.SoapHttpClientProtocol {
	private bool forceHTTP10;
   
	/// <remarks/>
	public DP(string Url, string Proxy, bool forceHTTP10) {
	    this.Url = Url;
	    this.forceHTTP10 = forceHTTP10;
	    if (Proxy != null) {
		IWebProxy proxyObject = new WebProxy(Proxy, false);
        proxyObject.Credentials = CredentialCache.DefaultCredentials;
		this.Proxy = proxyObject;
	    }
	    //this.Url = "http://localhost/WebService1/DP.asmx";
	}
    
	/// <remarks/>
	[System.Web.Services.Protocols.SoapDocumentMethodAttribute("urn:DP/Login", RequestNamespace="urn:DP", ResponseNamespace="urn:DP", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
	public Session Login(string email, string password) {
	    object[] results = this.Invoke("Login", new object[] {
								     email,
								     password});
	    return ((Session)(results[0]));
	}
    
	/// <remarks/>
	public System.IAsyncResult BeginLogin(string email, string password, System.AsyncCallback callback, object asyncState) {
	    return this.BeginInvoke("Login", new object[] {
							      email,
							      password}, callback, asyncState);
	}
    
	/// <remarks/>
	public Session EndLogin(System.IAsyncResult asyncResult) {
	    object[] results = this.EndInvoke(asyncResult);
	    return ((Session)(results[0]));
	}
    
	/// <remarks/>
	[System.Web.Services.Protocols.SoapDocumentMethodAttribute("urn:DP/NewPage", RequestNamespace="urn:DP", ResponseNamespace="urn:DP", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
	public Page NewPage(string sessionID, int eventID, string title, string description) {
	    object[] results = this.Invoke("NewPage", new object[] {
								       sessionID,
								       eventID,
								       title,
								       description});
	    return ((Page)(results[0]));
	}
    
	/// <remarks/>
	public System.IAsyncResult BeginNewPage(string sessionID, int eventID, string title, string description, System.AsyncCallback callback, object asyncState) {
	    return this.BeginInvoke("NewPage", new object[] {
								sessionID,
								eventID,
								title,
								description}, callback, asyncState);
	}
    
	/// <remarks/>
	public Page EndNewPage(System.IAsyncResult asyncResult) {
	    object[] results = this.EndInvoke(asyncResult);
	    return ((Page)(results[0]));
	}
    
	/// <remarks/>
	[System.Web.Services.Protocols.SoapDocumentMethodAttribute("urn:DP/Upload", RequestNamespace="urn:DP", ResponseNamespace="urn:DP", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
	public string Upload(string sessionID, int pageID, string Caption, string filename, [System.Xml.Serialization.XmlElementAttribute(DataType="base64Binary")] System.Byte[] imageData) {
	    object[] results = this.Invoke("Upload", new object[] {
								      sessionID,
								      pageID,
								      Caption,
								      filename,
								      imageData});
	    return ((string)(results[0]));
	}
    
	/// <remarks/>
	public System.IAsyncResult BeginUpload(string sessionID, int pageID, string Caption, string filename, System.Byte[] imageData, System.AsyncCallback callback, object asyncState) {
	    return this.BeginInvoke("Upload", new object[] {
							       sessionID,
							       pageID,
							       Caption,
							       filename,
							       imageData}, callback, asyncState);
	}
    
	/// <remarks/>
	public string EndUpload(System.IAsyncResult asyncResult) {
	    object[] results = this.EndInvoke(asyncResult);
	    return ((string)(results[0]));
	}

	protected override System.Net.WebRequest GetWebRequest(Uri uri) {
	    System.Net.HttpWebRequest webRequest = (System.Net.HttpWebRequest) base.GetWebRequest(uri);
	    if (forceHTTP10)
		webRequest.KeepAlive = false;
	    return webRequest;
	}
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="urn:DP")]
    public class Session {
    
	/// <remarks/>
	public string SessionKey;
    
	/// <remarks/>
	public string MaxSize;
    
	/// <remarks/>
	public Event[] EventList;
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="urn:DP")]
    public class Event {

	public Event() {
	}
    
	/// <remarks/>
	public int eventID;
    
	/// <remarks/>
	public string title;
    
	/// <remarks/>
	public int maxImages;
    
	/// <remarks/>
	public int currentImages;
    
	/// <remarks/>
	public Page[] pageList;

	public override string ToString() {
	    return title;
	}

	// For testing
	public Event(int eventID, string title, int maxImages, int currentImages) {
	    this.eventID = eventID;
	    this.title = title;
	    this.maxImages = maxImages;
	    this.currentImages = currentImages;
	}
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="urn:DP")]
    public class Page {

	public Page() {
	}
    
	/// <remarks/>
	public int pageID;
    
	/// <remarks/>
	public string title;
    
	/// <remarks/>
	public int maxImages;
    
	/// <remarks/>
	public int currentImages;

	public override string ToString() {
	    return title;
	}

	// For testing
	public Page(int pageID, string title, int maxImages, int currentImages) {
	    this.pageID = pageID;
	    this.title = title;
	    this.maxImages = maxImages;
	    this.currentImages = currentImages;
	}
    }
}    
