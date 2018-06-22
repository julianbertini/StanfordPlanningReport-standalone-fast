using System;
using System.Net;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StanfordPlanningReport
{
    class HTTPServer
    {

        public HttpListener listener;
        HtmlDocument doc;

        private const string testResultsHTMLPath = @"Z:\\Users\\Jbertini\\ESAPI\\StanfordPlanningReport-standalone-fast\\frontend\\testResultsIndex.html";

        public HTTPServer(HtmlDocument htmlDoc = null)
        {
            listener = null;
            doc = htmlDoc;
        }

        public void Start(string prefix, string prefix2)
        {
            // URI prefixes are required
            if (prefix == null || prefix.Length == 0)
                throw new ArgumentException("prefixes");

            // If listener is already listening, let it be
            if (listener == null) this.listener = new HttpListener();
            else if (listener.IsListening) return;

            // Add the prefixes.
            this.listener.Prefixes.Add(prefix);
            this.listener.Prefixes.Add(prefix2);

            this.listener.Start();


            IAsyncResult result = listener.BeginGetContext(new AsyncCallback(ListenerCallback), this.listener);
            Console.WriteLine("Listening...");

            // Here, in this space, is where I need to launch the InteractiveReport page; otherwise, the server will just terminate. 

        }

        public void ListenerCallback(IAsyncResult result)
        {
            if (this.listener == null) return;

            HttpListenerContext context = this.listener.EndGetContext(result);

            // start listening for the next request 
            this.listener.BeginGetContext(new AsyncCallback(ListenerCallback), this.listener);

            this.ProcessRequest(context);
        }

        public void ProcessRequest(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            System.IO.Stream requestDataStream;
            String requestDataContents = "";
            Int32 strLen, nRead;
            byte[] bytes;

            if (!request.IsLocal)
            {
                Console.WriteLine("Access denied - Request must be made locally.");
                return;
            }

            requestDataStream = request.InputStream;
            strLen = Convert.ToInt32(requestDataStream.Length);
            bytes = new byte[strLen];

            nRead = requestDataStream.Read(bytes, 0, strLen);

            foreach (Byte b in bytes)
            {
                requestDataContents += b.ToString();
            }

            if (request.HttpMethod == "GET")
            {
                handleGET(context);
            } 


        }

        public void Stop()
        {
            if (this.listener == null) return;

            this.listener.Close();
            this.listener = null;
        }

        public void SendOptionsResponse(HttpListenerContext context)
        {
            HttpListenerResponse response = context.Response;

            response.StatusCode = (int)HttpStatusCode.OK;
            response.StatusDescription = "OK";
            response.ProtocolVersion = new Version("1.1");

            // keep connection alive for subsequent POST request.
            response.KeepAlive = true;

            // Set HTTP header fields so allows CORS from file:// domain
            response.AddHeader("Access-Control-Allow-Origin", "null");
            response.AddHeader("Access-Control-Allow-Methods", "POST, GET");
            response.AddHeader("Access-Control-Allow-Headers", "Content-Type");

            byte[] buffer = System.Text.Encoding.UTF8.GetBytes("");
            System.IO.Stream output = response.OutputStream;
            output.Write(buffer, 0, 0);
            // You must close the output stream.
            output.Close();
            response.Close();
        }

        public void handleGET(HttpListenerContext context)
        {
            byte[] buffer;

           HttpListenerResponse response = context.Response;
            HttpListenerRequest request = context.Request;

            response.StatusCode = (int)HttpStatusCode.OK;
            response.StatusDescription = "OK";
            response.ProtocolVersion = new Version("1.1");


            // Construct a response.

            if (request.Url.AbsolutePath.Contains("update"))
            {
                response.AddHeader("Content-Type", "text/plain; charset=utf-8");
                string responseString = "Thanks!";
                buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            }
            else
            {
                string extension = getExtension(request.Url);

                if (extension == "css")
                {
                    response.AddHeader("Content-Type", "text/css; charset=utf-8");
                }
                else if (extension == "html")
                {
                    response.AddHeader("Content-Type", "text/html; charset=utf-8");
                }
                else if (extension == "js")
                {
                    response.AddHeader("Content-Type", "application/javascript");
                }

                buffer = getResource(request);
            }

            // Get a response stream and write the response to it.
            response.ContentLength64 = buffer.Length;
            System.IO.Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);

            // You must close the output stream.
            output.Close();
            response.Close();

            //this.Stop();

        }

        public byte[] getResource(HttpListenerRequest request)
        {
            byte[] bytes = null;
            string resourcePath = "";

            try
            {
                resourcePath = GetResourcePath(request.Url);
                bytes = System.IO.File.ReadAllBytes(resourcePath);
            }

            catch
            {
                // Here probably make server respond with a not found or something like that.
                Console.WriteLine("Error - could not locate resource. Request Uri: " + resourcePath);
            }

            return bytes;

        }

        public string GetResourcePath(Uri uri)
        {
            string root = System.IO.Path.GetDirectoryName(testResultsHTMLPath);
            string relativePath = uri.AbsolutePath;
            relativePath = relativePath.Substring(1, relativePath.Length - 1);
            relativePath = relativePath.Replace("/","\\");
            string resourcePath = System.IO.Path.Combine(root, relativePath);

            return resourcePath;
        }

        public string getExtension(Uri uri)
        {
            string relativePath = uri.AbsolutePath;
            int dotIndex = relativePath.LastIndexOf(".");
            string extension = relativePath.Substring(dotIndex+1,relativePath.Length-dotIndex-1);

            return extension;
        }

        public void Send404(HttpListenerContext context)
        {
            HttpListenerResponse response = context.Response;

            response.StatusCode = (int)HttpStatusCode.NotFound;
            response.StatusDescription = "Resource not found";
            response.ProtocolVersion = new Version("1.1");

            string message404;
            StringBuilder message = new StringBuilder();
            message.Append("<HTML><BODY>");
            message.Append("<h2> Error message 404: Sorry - Could not load requested page.</h2>");
            message.Append("<p> Please tell Shi or Julian</p>");
            message.Append("</BODY></HTML>");
            message404 = message.ToString();

            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(message404);

            // Get a response stream and write the response to it.
            response.ContentLength64 = buffer.Length;
            System.IO.Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);

            // You must close the output stream.
            output.Close();

            response.Close();
        }

    }

}
