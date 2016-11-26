using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using DigiProofs.Logger;

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
        public int page_id { get; set; }
        public string title { get; set; }
    }

    public class Event {
        public string current_images { get; set; }
        public int max_images { get; set; }
        public int event_id { get; set; }
        public string title { get; set; }
        public List<Page> pages { get; set; }

        public override string ToString() {
            return title;
        }
    }

    public class EventList {
        public int max_size { get; set; }
        public List<Event> events { get; set; }
        public string token { get; set; }
        public string message { get; set; }
        public int code { get; set; }
    }

    public class NewPage {
        public int page_id { get; set; }
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
        public int page_id { get; set; }
    }

    // These are the errors that may occur during communication with the upload server.
    public enum SessionError {
        LoginFail,          // Server doesn't recognize email or password
        NotLoggedIn,        // Invalid (or nonexistent) token
        UploadDenied,       // Given (sub) account not permitted to upload to the server
        InvalidFile,        // Some problem opening or scaling an image file
        InvalidImage,       // Server didn't like image.
        InvalidPage,        // Page doesn't exist (pro could remove it?)
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
        private LogList log;
        private int max_size = 0;

        private Hashtable eventHash = new Hashtable();
        private Hashtable pageHash = new Hashtable();

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
        public int MaxSize {
            get { return max_size; }
        }

        public NetSession(LogList log, string host, string email, string uploadToken, string proxy) {
            this.log = log;
            this.email = email;
            this.uploadToken = uploadToken;
            string HTTPurl = "http://" + host + "/";
            string HTTPSurl = "https://" + host + "/";
            this.proxy = proxy;
            string vars = "  http URL: " + HTTPurl + Environment.NewLine + "  https URL: " + HTTPSurl + Environment.NewLine + "  Proxy: " + proxy + Environment.NewLine + "  email: " + email;

            log.Add(new LogEntry("Session Created", vars));
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

        public async Task<string> GetToken(string password) {
            string resultJSON = "";
            try {
                HttpContent emailContent = new StringContent(Email);
                HttpContent passwordContent = new StringContent(password);
                HttpContent commandContent = new StringContent("get_token");
                MultipartFormDataContent multipartFormDataContent = new MultipartFormDataContent();
                // The extra quotes are required on the following parameters to make Perl CGI parse properly.
                multipartFormDataContent.Add(emailContent, "\"email\"");
                multipartFormDataContent.Add(passwordContent, "\"password\"");
                multipartFormDataContent.Add(commandContent, "\"command\"");
                HttpResponseMessage response = await httpsClient.PostAsync("ul/upload", multipartFormDataContent);
                if (response.IsSuccessStatusCode) {
                    // ReadAsAsync is a nice way to deserialize the JSON into an object,
                    // but the are library issues that keep it from working at the moment,
                    // so I've fallen back to the somewhat more manual deserialization.
                    //token = await response.Content.ReadAsAsync<Token>();
                    resultJSON = await response.Content.ReadAsStringAsync();
                    Token token = JsonConvert.DeserializeObject<Token>(resultJSON);
                    // Login error (1020) is a bad password.  All others are severe errors.
                    switch (token.code) {
                        case 100:
                            this.uploadToken = token.token;
                            log.Add(new LogEntry("got token for " + Email, ""));
                            return token.token;  // We return the token so that the calling program can cache it.
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
                log.Add(new LogEntry("Network connection error during login to " + Email, e.Message));
                this.uploadToken = null;
                throw new SessionException(e.ToString(), SessionError.NetworkError, e);
            }

            catch (System.InvalidOperationException e) {
                log.Add(new LogEntry("An InvalidOperationException occurred during login to " + Email, e.Message));
                this.uploadToken = null;
                throw new SessionException(e.ToString(), SessionError.ServerError, e);
            }

            catch (JsonException) {
                log.Add(new LogEntry("JSON Error", resultJSON));
                throw new SessionException("Internal Error", SessionError.InternalError);
            }

            catch (Exception e) {
                log.Add(new LogEntry("Unexpected error during login to " + Email, e.Message));
                this.uploadToken = null;
                throw new SessionException(e.ToString(), SessionError.UnknownError, e);
            }
            return null;

        }


        public async Task GetEventListAsync() {
            string resultJSON = "";
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
                    resultJSON = await response.Content.ReadAsStringAsync();
                    eventList = JsonConvert.DeserializeObject<EventList>(resultJSON);
                    switch (eventList.code) {
                        case 100:
                            max_size = eventList.max_size;
                            if (max_size > 0) {
                                log.Add(new LogEntry(String.Format("Upload Size limit: {0}", max_size), ""));
                            }
                            this.eventList = eventList.events.ToArray();
                            foreach (Event ev in this.eventList) {
                                eventHash.Add(ev.event_id, ev);
                                foreach (Page pg in ev.pages)
                                    pageHash.Add(pg.page_id, pg);
                            }
                            break;
                        case 1030:
                            this.uploadToken = null;
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
                throw e;
            }
            catch (System.Net.WebException e) {
                log.Add(new LogEntry("Network connection error during event fetch to " + email, e.Message));
                throw new SessionException(e.ToString(), SessionError.NetworkError, e);
            }

            catch (System.InvalidOperationException e) {
                log.Add(new LogEntry("An InvalidOperationException occurred during event fetch to " + email, e.Message));
                throw new SessionException(e.ToString(), SessionError.ServerError, e);
            }

            catch (JsonException) {
                log.Add(new LogEntry("JSON Error", resultJSON));
                throw new SessionException("Internal Error", SessionError.InternalError);
            }

            catch (Exception e) {
                log.Add(new LogEntry("Unexpected error during event fetch to " + email, e.Message));
                throw new SessionException(e.ToString(), SessionError.UnknownError, e);
            }
        }

        public async Task<int> NewPage(int event_id, string title) {
            string resultJSON = "";
            try {
                if (this.uploadToken == null)
                    throw new SessionException("Not Logged In", SessionError.NotLoggedIn);
                NewPage newPage = null;
                MultipartFormDataContent multipartFormDataContent = new MultipartFormDataContent();
                HttpContent commandContent = new StringContent("new_page");
                multipartFormDataContent.Add(commandContent, "\"command\"");
                HttpContent tokenContent = new StringContent(this.uploadToken);
                multipartFormDataContent.Add(tokenContent, "\"token\"");
                HttpContent event_idContent = new StringContent(event_id.ToString());
                multipartFormDataContent.Add(event_idContent, "\"event_id\"");
                if (title != null) {
                    HttpContent titleContent = new StringContent(title);
                    multipartFormDataContent.Add(titleContent, "\"title\"");
                }
                HttpResponseMessage response = await httpClient.PostAsync("ul/upload", multipartFormDataContent);
                if (response.IsSuccessStatusCode) {
                    resultJSON = await response.Content.ReadAsStringAsync();
                    newPage = JsonConvert.DeserializeObject<NewPage>(resultJSON);
                    switch (newPage.code) {
                        case 100:
                            pageHash.Add(newPage.page_id, newPage);
                            log.Add(new LogEntry("New Page created: " + newPage.title + "(" + newPage.page_id + ")", ""));
                            return newPage.page_id;
                        case 1030:
                            this.uploadToken = null;
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
                throw e;
            }
            catch (System.Net.WebException e) {
                log.Add(new LogEntry("Network connection error during page create " + email, e.Message));
                throw new SessionException(e.ToString(), SessionError.NetworkError, e);
            }

            catch (System.InvalidOperationException e) {
                log.Add(new LogEntry("An InvalidOperationException occurred during page create " + email, e.Message));
                throw new SessionException(e.ToString(), SessionError.ServerError, e);
            }

            catch (JsonException) {
                log.Add(new LogEntry("JSON Error", resultJSON));
                throw new SessionException("Internal Error", SessionError.InternalError);
            }

            catch (Exception e) {
                log.Add(new LogEntry("Unexpected error during page create " + email, e.Message));
                throw new SessionException(e.ToString(), SessionError.UnknownError, e);
            }
            return 0;
        }

        public async Task<string> Upload(int page_id, string filename, Stream image) {
            string resultJSON = "";
            try {
                if (this.uploadToken == null)
                    throw new SessionException("Not Logged In", SessionError.NotLoggedIn);
                MultipartFormDataContent multipartFormDataContent = new MultipartFormDataContent();
                HttpContent commandContent = new StringContent("upload");
                multipartFormDataContent.Add(commandContent, "\"command\"");
                HttpContent tokenContent = new StringContent(this.uploadToken);
                multipartFormDataContent.Add(tokenContent, "\"token\"");
                HttpContent page_idContent = new StringContent(page_id.ToString());
                multipartFormDataContent.Add(page_idContent, "\"page_id\"");
                StreamContent imageContent = new StreamContent(image);
                imageContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data");
                imageContent.Headers.ContentDisposition.Name = "\"file\"";
                imageContent.Headers.ContentDisposition.FileName = "\"" + filename + "\"";
                imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpg");
                //imageContent.Headers.ContentLength = image.Length;
                multipartFormDataContent.Add(imageContent);
                HttpResponseMessage response = await httpClient.PostAsync("ul/upload", multipartFormDataContent);
                if (response.IsSuccessStatusCode) {
                    resultJSON = await response.Content.ReadAsStringAsync();
                    Upload upload = JsonConvert.DeserializeObject<Upload>(resultJSON);
                    switch (upload.code) {
                        case 100:
                            log.Add(new LogEntry(String.Format("Upload Complete {0}: {1}", upload.image_id, filename), ""));
                            return upload.image_id;
                        case 1030:
                            this.uploadToken = null;
                            throw new SessionException(upload.message, SessionError.NotLoggedIn);
                        case 1010:
                            throw new SessionException(upload.message, SessionError.UploadDenied);
                        case 1060:
                            throw new SessionException(upload.message, SessionError.InvalidPage);
                        case 1090:
                            throw new SessionException(upload.message, SessionError.EventExpired);
                        case 1100:
                            throw new SessionException(upload.message, SessionError.PageFull);
                        case 1110:
                            throw new SessionException(upload.message, SessionError.InvalidImage);
                        case 1001: // Missing Parameter
                        case 1040: // Invalid Event
                        case 1050: // Illegal Event
                        case 1070: // Internal error (in Upload server)
                        case 1080: // Missing filename (in HTTP protocol)
                            throw new SessionException(upload.message, SessionError.InternalError);
                        default:
                            throw new SessionException(upload.message, SessionError.UnknownError);
                    }

                }
            }
            catch (SessionException e) {
                throw e;
            }
            catch (System.Net.WebException e) {
                log.Add(new LogEntry("Network connection error during upload " + filename, e.Message));
                throw new SessionException(e.ToString(), SessionError.NetworkError, e);
            }

            catch (System.InvalidOperationException e) {
                log.Add(new LogEntry("An InvalidOperationException occurred during upload " + filename, e.Message));
                throw new SessionException(e.ToString(), SessionError.ServerError, e);
            }

            catch (JsonException) {
                log.Add(new LogEntry("JSON Error", resultJSON));
                throw new SessionException("Internal Error", SessionError.InternalError);
            }

            catch (Exception e) {
                log.Add(new LogEntry("Unexpected error during event fetch to " + filename, e.Message));
                throw new SessionException(e.ToString(), SessionError.UnknownError, e);
            }
            return null;
        }
    }
}
    
