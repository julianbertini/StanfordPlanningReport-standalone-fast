using System;
using System.Net;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace VMS.TPS
{
    class HTTPServer
    {

        public bool ServeResources {get; set;}
        public Route Routes { get; set; }

        private readonly HttpListener _listener;
        private readonly Thread _listenerThread;
        private readonly Thread[] _workers;
        private readonly ManualResetEvent _ready, _stop;
        private readonly Queue<HttpListenerContext> _queue; 

        private const string ResourcesPath = @"frontend";

        public HTTPServer(int maxThreads)
        {
            _listener = new HttpListener();
            _workers = new Thread[maxThreads];
            _queue = new Queue<HttpListenerContext>();
            _stop = new ManualResetEvent(false);
            _ready = new ManualResetEvent(false);
            _listenerThread = new Thread(HandleRequests);
            ServeResources = false;
            Routes = new Route();
        }

        public void Start(string prefix)
        {
            // URI prefixes are required
            if (prefix == null || prefix.Length == 0)
                throw new ArgumentException("prefixes");

            // If listener is already listening, do not start again
            if (_listener.IsListening) return;

            // Add the prefixes.
            this._listener.Prefixes.Add(prefix);

            this._listener.Start();
            this._listenerThread.Start();

            for (int i = 0; i < _workers.Length; i++)
            {
                _workers[i] = new Thread(Worker);
                _workers[i].Start();
            }
        }

        public void HandleRequests()
        {
            Console.WriteLine("Listening...");
            while (_listener.IsListening)
            {
                var context = _listener.BeginGetContext(ContextReady, null);

                if (WaitHandle.WaitAny(new[] { _stop, context.AsyncWaitHandle }) == 0)
                    return;
            }
        }

        public void ContextReady(IAsyncResult result)
        {
            try
            {
                lock (_queue)
                {
                    _queue.Enqueue(_listener.EndGetContext(result));
                    _ready.Set();
                }
            }
            catch { return; }
        }

        public void Worker()
        {
            WaitHandle[] wait = new[] { _ready, _stop };

            while (WaitHandle.WaitAny(wait) == 0)
            {
                HttpListenerContext context;
                
                lock (_queue)
                {
                    if (_queue.Count > 0)
                        context = _queue.Dequeue();
                    else
                    {
                        _ready.Reset();
                        continue;
                    }
                }
                try { ProcessRequest(context);  }
                catch (Exception e) { Console.Error.WriteLine(e);  }
            }
        }

        public void Stop()
        {
            Console.WriteLine("Closing...");
            _stop.Set();
            _listenerThread.Join();
            foreach (Thread worker in _workers)
                worker.Join();
            _listener.Stop();
        }

        public void ProcessRequest(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;

            if (!request.IsLocal)
            {
                Console.WriteLine("Access denied - Request must be made locally.");
                return;
            }

            
            System.IO.Stream requestDataStream;
            String requestDataContents = "";
            Int32 strLen, nRead;
            byte[] bytes;

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
                HandleGET(context);
            } 
            else
            {
                // respond with an access denied or something from server;
                Console.WriteLine("Error - only accepting GET requests");
            }

        }

        public void HandleGET(HttpListenerContext context)
        {
            byte[] buffer = null;
            string responseFile = null;
            bool noResources = false;

            HttpListenerResponse response = context.Response;
            HttpListenerRequest request = context.Request;

            response.StatusCode = (int)HttpStatusCode.OK;
            response.StatusDescription = "OK";
            response.ProtocolVersion = new System.Version("1.1");
            response.AddHeader("Content-Type", "text/html; charset=utf-8");

            // Construct a response.

            // go through all defined routes and handle each accordingly 
            if (Routes.RoutesList != null)
            {
                try
                {
                    Route.RouteCallback value;

                    if (Routes.RoutesList.TryGetValue(request.Url.AbsolutePath, out value))
                    {
                        // run the callback associated with the route
                        responseFile = value(context);
                        if (responseFile != null)
                        {
                            string resourcePath = GetResourcePathByName(responseFile);
                            buffer = GetResource(resourcePath);
                        }
                        else
                        {
                            noResources = true;
                        }
                        // here set headers and such for response
                    }
                }
                catch
                {
                    Console.WriteLine("Error - something went wrong when looking at route and route callback in server");
                }
                
            }
            // handle resource requests, right now only js and css files
            if (ServeResources && buffer == null && !noResources)
            { 
                string extension = GetExtension(request.Url);

                if (extension == "css")
                {
                    response.AddHeader("Content-Type", "text/css; charset=utf-8");
                }
                else if (extension == "js")
                {
                    response.AddHeader("Content-Type", "application/javascript");
                }

                string resourcePath = GetResourcePathByUri(request.Url);
                buffer = GetResource(resourcePath);
            }

            // Get a response stream and write the response to it.
            if (buffer != null)
            {
                response.ContentLength64 = buffer.Length;
                System.IO.Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);

                // You must close the output stream.
                output.Flush();
            }

            response.Close();
        }

        public NameValueCollection GetQueryParams(HttpListenerRequest request)
        {
            return request.QueryString;
        }

        public byte[] GetResource(string resourcePath)
        {
            byte[] bytes = null;

            try
            {
                bytes = System.IO.File.ReadAllBytes(resourcePath);
            }

            catch
            {
                // Here probably make server respond with a not found or something like that.
                Console.WriteLine("Error - could not locate resource at request uri: " + resourcePath);
            }

            return bytes;

        }

        public string GetResourcePathByUri(Uri uri)
        {
            string root = System.IO.Path.Combine(Environment.GetEnvironmentVariable("ROOT_PATH"), ResourcesPath);
            string relativePath = uri.AbsolutePath;
            relativePath = relativePath.Substring(1, relativePath.Length - 1);
            relativePath = relativePath.Replace("/","\\");
            string resourcePath = System.IO.Path.Combine(root, relativePath);

            return resourcePath;
        }
        public string GetResourcePathByName(string filename)
        {
            string root = System.IO.Path.Combine(Environment.GetEnvironmentVariable("ROOT_PATH"), ResourcesPath);
            string resourcePath = System.IO.Path.Combine(root, filename);

            return resourcePath;
        }

        public string GetExtension(Uri uri)
        {
            string relativePath = uri.AbsolutePath;
            int dotIndex = relativePath.LastIndexOf(".");
            string extension = relativePath.Substring(dotIndex+1,relativePath.Length-dotIndex-1);

            return extension;
        }

        public void Send404(HttpListenerResponse response)
        {

            response.StatusCode = (int)HttpStatusCode.NotFound;
            response.StatusDescription = "Resource not found";
            response.ProtocolVersion = new System.Version("1.1");

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

            // close the output stream.
            output.Close();

            response.Close();
        }

    }

}
