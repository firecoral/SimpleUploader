using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DigiProofs.JSONUploader {
    /// <summary>
    /// DigiProofs Network  Session
    /// This represents a series of connections used to upload images.
    /// First a NetSession is created with a server, email, and password.
    /// Then Login() is called to get the sessionID and current XML state
    /// of the various events.
    /// </summary>
    /// 

    public class Token {
        public int code { get; set; }
        public string token { get; set; }
        public string message { get; set; }
    }

    public class Page {
        public string max_images { get; set; }
        public string current_images { get; set; }
        public string id { get; set; }
        public string title { get; set; }
    }

    public class Event {
        public string current_images { get; set; }
        public int max_images { get; set; }
        public string id { get; set; }
        public string title { get; set; }
        public List<Page> pages { get; set; }
    }

    public class EventList {
        public string max_size { get; set; }
        public List<Event> events { get; set; }
        public string token { get; set; }
        public string message { get; set; }
        public int code { get; set; }
    }

    public class NewPage {
        public string page_id { get; set; }
        public string token { get; set; }
        public string message { get; set; }
        public string max_images { get; set; }
        public int code { get; set; }
        public int current_images { get; set; }
        public string title { get; set; }
    }

    public class Upload {
        public int code { get; set; }
        public string message { get; set; }
        public string token { get; set; }
        public string filename { get; set; }
        public string image_id { get; set; }
        public string page_id { get; set; }
    }

    // These are the errors that may occur during communication with the upload server.
    public enum SessionError {
        LoginFail,          // Server doesn't recognize email or password
        NotLoggedIn,        // Invalid (or nonexistent) token
        UploadDenied,       // Given (sub) account not permitted to upload to the server
        InvalidFile,        // Some problem opening or scaling an image file
        InvalidImage,       // Server didn't like image.
        EventExpired,       // This event has expired.
        PageFull,           // Page was too full for upload
        EventFull,          // Event has no more room for images.
        NetworkError,       // Any network error
        ServerError,        // Any error on the digiproofs servers
        UnknownWebError,    // An unexpected SoapException occured
        InternalError,      // Programming error
        UnknownError,	    // An unexpected error has occurred.
    };

    // This exception should handle all expected errors from the netsession
    public class SessionException : System.ApplicationException {
        public SessionError Error {
            get { return _sessionError; }
        }
        private SessionError _sessionError;

        public SessionException(string reason, SessionError error) : base(reason) {
            _sessionError = error;
        }
        public SessionException(string reason, SessionError error, Exception inner) : base(reason, inner) {
            _sessionError = error;
        }
    }
    public class NetSession {
        private string proxy;
        private string email;
        private string uploadToken;
        private Event[] eventList;
        private HttpClient httpClient;
        private HttpClient httpsClient;

        private Hashtable eventHash = new Hashtable();
        private Hashtable pageHash = new Hashtable();

        private LogList logs;

        public string Email {
            get { return email; }
        }
        public string UploadToken {
            get { return uploadToken; }
        }
        public Event[] EventList {
            get {
                if (this.uploadToken == null)
                    throw new SessionException("Not Logged In", SessionError.NotLoggedIn);
                return this.eventList;
            }
        }

        public NetSession(string host, string email, string uploadToken, string proxy) {
            this.email = email;
            this.uploadToken = uploadToken;
            string HTTPurl = "http://" + host + "/";
            string HTTPSurl = "https://" + host + "/";
            this.proxy = proxy;
            logs = new LogList();
            string vars = "  http URL: " + HTTPurl + Environment.NewLine + "  https URL: " + HTTPSurl + Environment.NewLine + "  Proxy: " + proxy + Environment.NewLine + "  email: " + email;
            logs.Add(new LogEvent("Session Created", vars));
            if (this.proxy != null) {
                var httpClientHandler = new HttpClientHandler {
                    Proxy = new WebProxy(this.proxy, false),
                    UseProxy = true
                };
                httpClient = new HttpClient(httpClientHandler);
                httpsClient = new HttpClient(httpClientHandler);
            }
            else {
                httpClient = new HttpClient();
                httpsClient = new HttpClient();
            }
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.BaseAddress = new Uri(HTTPurl);
            httpsClient.DefaultRequestHeaders.Accept.Clear();
            httpsClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpsClient.BaseAddress = new Uri(HTTPSurl);
        }

        // Use the password to obtain a token from the server.

        public async Task GetToken(string password) {
            try {
                HttpContent emailContent = new StringContent(this.email);
                HttpContent passwordContent = new StringContent(password);
                HttpContent commandContent = new StringContent("get_token");
                MultipartFormDataContent multipartFormDataContent = new MultipartFormDataContent();
                // The extra quotes are required on the following parameters to make Perl CGI parse properly.
                multipartFormDataContent.Add(emailContent, "\"email\"");
                multipartFormDataContent.Add(passwordContent, "\"password\"");
                multipartFormDataContent.Add(commandContent, "\"command\"");
                HttpResponseMessage response = await httpClient.PostAsync("ul/upload", multipartFormDataContent);
                if (response.IsSuccessStatusCode) {
                    // ReadAsAsync is a nice way to deserialize the JSON into an object,
                    // but the are library issues that keep it from working at the moment,
                    // so I've fallen back to the somewhat more manual deserialization.
                    //token = await response.Content.ReadAsAsync<Token>();
                    string result = await response.Content.ReadAsStringAsync();
                    Token token = JsonConvert.DeserializeObject<Token>(result);
                    // Login error (1020) is a bad password.  All others are severe errors.
                    switch (token.code) {
                        case 100:
                            this.uploadToken = token.token;
                            logs.Add(new LogEvent("got token for " + email, ""));
                            break;
                        case 1030:
                            throw new SessionException(token.message, SessionError.LoginFail);
                        case 1010:
                            throw new SessionException(token.message, SessionError.UploadDenied);
                        case 1001:
                            throw new SessionException(token.message, SessionError.InternalError);
                        default:
                            throw new SessionException(token.message, SessionError.UnknownError);
                    }
                }
            }
            catch (SessionException e) {
                this.uploadToken = null;
                throw e;
            }
            catch (System.Net.WebException e) {
                logs.Add(new LogEvent("Network connection error during login to " + email, e.Message));
                this.uploadToken = null;
                throw new SessionException(e.ToString(), SessionError.NetworkError, e);
            }

            catch (System.InvalidOperationException e) {
                logs.Add(new LogEvent("An InvalidOperationException occurred during login to " + email, e.Message));
                this.uploadToken = null;
                throw new SessionException(e.ToString(), SessionError.ServerError, e);
            }

            catch (Exception e) {
                logs.Add(new LogEvent("Unexpected error during login to " + email, e.Message));
                this.uploadToken = null;
                throw new SessionException(e.ToString(), SessionError.UnknownError, e);
            }
        }

        public async Task GetEventList() {
            try {
                if (this.uploadToken == null)
                    throw new SessionException("Not Logged In", SessionError.NotLoggedIn);
                EventList eventList = null;
                HttpContent commandContent = new StringContent("get_events");
                HttpContent tokenContent = new StringContent(this.uploadToken);
                MultipartFormDataContent multipartFormDataContent = new MultipartFormDataContent();
                // The extra quotes are required on the following parameters to make Perl CGI parse properly.
                multipartFormDataContent.Add(commandContent, "\"command\"");
                multipartFormDataContent.Add(tokenContent, "\"token\"");
                HttpResponseMessage response = await httpClient.PostAsync("ul/upload", multipartFormDataContent);
                if (response.IsSuccessStatusCode) {
                    string result = await response.Content.ReadAsStringAsync();
                    eventList = JsonConvert.DeserializeObject<EventList>(result);
                    switch (eventList.code) {
                        case 100:
                            this.eventList = eventList.events.ToArray();
                            foreach (Event ev in this.eventList) {
                                eventHash.Add(ev.id, ev);
                                foreach (Page pg in ev.pages)
                                    pageHash.Add(pg.id, pg);
                            }
                            break;
                        case 1030:
                            throw new SessionException(eventList.message, SessionError.NotLoggedIn);
                        case 1010:
                            throw new SessionException(eventList.message, SessionError.UploadDenied);
                        case 1001:
                            throw new SessionException(eventList.message, SessionError.InternalError);
                        default:
                            throw new SessionException(eventList.message, SessionError.UnknownError);
                    }

                }
            }
            catch (SessionException e) {
                this.uploadToken = null;
                throw e;
            }
            catch (System.Net.WebException e) {
                logs.Add(new LogEvent("Network connection error during event fetch to " + email, e.Message));
                this.uploadToken = null;
                throw new SessionException(e.ToString(), SessionError.NetworkError, e);
            }

            catch (System.InvalidOperationException e) {
                logs.Add(new LogEvent("An InvalidOperationException occurred during event fetch to " + email, e.Message));
                this.uploadToken = null;
                throw new SessionException(e.ToString(), SessionError.ServerError, e);
            }

            catch (Exception e) {
                logs.Add(new LogEvent("Unexpected error during event fetch to " + email, e.Message));
                this.uploadToken = null;
                throw new SessionException(e.ToString(), SessionError.UnknownError, e);
            }
        }

        public async Task<string> NewPage(string event_id, string title) {
            try {
                if (this.uploadToken == null)
                    throw new SessionException("Not Logged In", SessionError.NotLoggedIn);
                NewPage newPage = null;
                MultipartFormDataContent multipartFormDataContent = new MultipartFormDataContent();
                HttpContent commandContent = new StringContent("new_page");
                multipartFormDataContent.Add(commandContent, "\"command\"");
                HttpContent tokenContent = new StringContent(this.uploadToken);
                multipartFormDataContent.Add(tokenContent, "\"token\"");
                HttpContent event_idContent = new StringContent(event_id);
                multipartFormDataContent.Add(event_idContent, "\"event_id\"");
                if (title != null) {
                    HttpContent titleContent = new StringContent(title);
                    multipartFormDataContent.Add(titleContent, "\"title\"");
                }
                HttpResponseMessage response = await httpClient.PostAsync("ul/upload", multipartFormDataContent);
                if (response.IsSuccessStatusCode) {
                    string result = await response.Content.ReadAsStringAsync();
                    newPage = JsonConvert.DeserializeObject<NewPage>(result);
                    switch (newPage.code) {
                        case 100:
                            pageHash.Add(newPage.page_id, newPage);
                            logs.Add(new LogEvent("New Page created: " + newPage.title + "(" + newPage.page_id + ")", ""));
                            return newPage.page_id;
                        case 1030:
                            throw new SessionException(newPage.message, SessionError.NotLoggedIn);
                        case 1010:
                            throw new SessionException(newPage.message, SessionError.UploadDenied);
                        case 1001: // Missing Parameter
                        case 1040: // Invalid Event
                        case 1050: // Illegal Event
                            throw new SessionException(newPage.message, SessionError.InternalError);
                        default:
                            throw new SessionException(newPage.message, SessionError.UnknownError);
                    }

                }
            }
            catch (SessionException e) {
                this.uploadToken = null;
                throw e;
            }
            catch (System.Net.WebException e) {
                logs.Add(new LogEvent("Network connection error during page create " + email, e.Message));
                this.uploadToken = null;
                throw new SessionException(e.ToString(), SessionError.NetworkError, e);
            }

            catch (System.InvalidOperationException e) {
                logs.Add(new LogEvent("An InvalidOperationException occurred during page create " + email, e.Message));
                this.uploadToken = null;
                throw new SessionException(e.ToString(), SessionError.ServerError, e);
            }

            catch (Exception e) {
                logs.Add(new LogEvent("Unexpected error during page create " + email, e.Message));
                this.uploadToken = null;
                throw new SessionException(e.ToString(), SessionError.UnknownError, e);
            }
            return null;
        }

        public async Task<string> Upload(string page_id, string filename, Stream image) {
            try {
                if (this.uploadToken == null)
                    throw new SessionException("Not Logged In", SessionError.NotLoggedIn);
                NewPage newPage = null;
                MultipartFormDataContent multipartFormDataContent = new MultipartFormDataContent();
                HttpContent commandContent = new StringContent("upload");
                multipartFormDataContent.Add(commandContent, "\"command\"");
                HttpContent tokenContent = new StringContent(this.uploadToken);
                multipartFormDataContent.Add(tokenContent, "\"token\"");
                HttpContent page_idContent = new StringContent(page_id);
                multipartFormDataContent.Add(page_idContent, "\"page_id\"");
                StreamContent imageContent = new StreamContent(image);
                imageContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data");
                imageContent.Headers.ContentDisposition.FileName = filename;
                imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpg");
                imageContent.Headers.ContentLength = image.Length;
                multipartFormDataContent.Add(imageContent);
                HttpResponseMessage response = await httpClient.PostAsync("ul/upload", multipartFormDataContent);
                if (response.IsSuccessStatusCode) {
                    string result = await response.Content.ReadAsStringAsync();
                    Upload upload = JsonConvert.DeserializeObject<Upload>(result);
                    switch (newPage.code) {
                        case 100:
                            pageHash.Add(newPage.page_id, newPage);
                            logs.Add(new LogEvent("Upload Complete: " + upload.image_id, ""));
                            return upload.image_id;
                        case 1030:
                            throw new SessionException(upload.message, SessionError.NotLoggedIn);
                        case 1010:
                            throw new SessionException(upload.message, SessionError.UploadDenied);
                        case 1001: // Missing Parameter
                        case 1040: // Invalid Event
                        case 1050: // Illegal Event
                            throw new SessionException(upload.message, SessionError.InternalError);
                        default:
                            throw new SessionException(upload.message, SessionError.UnknownError);
                    }

                }
            }
            catch (SessionException e) {
                this.uploadToken = null;
                throw e;
            }
            catch (System.Net.WebException e) {
                logs.Add(new LogEvent("Network connection error during upload " + filename, e.Message));
                this.uploadToken = null;
                throw new SessionException(e.ToString(), SessionError.NetworkError, e);
            }

            catch (System.InvalidOperationException e) {
                logs.Add(new LogEvent("An InvalidOperationException occurred during upload " + filename, e.Message));
                this.uploadToken = null;
                throw new SessionException(e.ToString(), SessionError.ServerError, e);
            }

            catch (Exception e) {
                logs.Add(new LogEvent("Unexpected error during event fetch to " + filename, e.Message));
                this.uploadToken = null;
                throw new SessionException(e.ToString(), SessionError.UnknownError, e);
            }
            return null;
        }
    }
}
    
