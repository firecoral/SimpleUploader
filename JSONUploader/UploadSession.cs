using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
//using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DigiProofs.JSONUploader
{
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

    public class NetSession
    {
        private string HTTPurl;
        private string HTTPSurl;
        private string proxy;
        private string email;
        private string uploadToken;
        private bool forceHTTP10;
        static HttpClient httpClient = new HttpClient();

        private Hashtable eventHash = new Hashtable();
        private Hashtable pageHash = new Hashtable();

        private LogList logs;

        public string Email {
            get { return email; }
        }
        public string UploadToken {
            get { return uploadToken; }
        }

        public NetSession(string host, string email, string uploadToken, string proxy, bool forceHTTP10) {
            this.email = email;
            this.uploadToken = uploadToken;
            HTTPurl = "http://" + host + "/";
            HTTPSurl = "https://" + host + "/";
            this.proxy = proxy;
            this.forceHTTP10 = forceHTTP10;
            logs = new LogList();
            string vars = "  http URL: " + HTTPurl + Environment.NewLine + "  https URL: " + HTTPSurl + Environment.NewLine + "  Proxy: " + proxy + Environment.NewLine + "  email: " + email;
            logs.Add(new LogEvent("Session Created", vars));
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<string> GetToken(string password) {
            Token token = null;
            Console.WriteLine("email: {0}, password: {1}", this.email, password);
            httpClient.BaseAddress = new Uri(HTTPurl);
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
                ////token = await response.Content.ReadAsAsync<Token>();
                string result = await response.Content.ReadAsStringAsync();
                token = JsonConvert.DeserializeObject<Token>(result);
                this.uploadToken = token.token;
                Console.WriteLine("Code: {0}", token.code);
                Console.WriteLine("Message: {0}", token.message);
                Console.WriteLine("Token: {0}", this.uploadToken);
            }

            return token.token;
        }

    }
}
