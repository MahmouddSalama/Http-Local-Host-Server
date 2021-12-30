using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace HTTPServer
{
    class Server
    {
        Socket serverSocket;
        public Server(int portNumber, string redirectionMatrixPath)
        {
            //TODO: call this.LoadRedirectionRules passing redirectionMatrixPath to it
            LoadRedirectionRules(redirectionMatrixPath);
            //TODO: initialize this.serverSocket
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint hostEndPoint = new IPEndPoint(IPAddress.Any,portNumber);
            serverSocket.Bind(hostEndPoint);
        }

        public void StartServer()
        {
            // TODO: Listen to connections, with large backlog.
            serverSocket.Listen(1000);
            // TODO: Accept connections in while loop and start a thread for each connection on function "Handle Connection"
            while (true)
            {
                Socket clientSocket = serverSocket.Accept();
                Thread newthread = new Thread(new ParameterizedThreadStart(HandleConnection));
                newthread.Start(clientSocket);
                //TODO: accept connections and start thread for each accepted connection.

            }
        }

        public void HandleConnection(object obj)
        {
            // TODO: Create client socket 
            Socket clientSocket = (Socket)obj;
            // set client socket ReceiveTimeout = 0 to indicate an infinite time-out period
            clientSocket.ReceiveTimeout = 0;
            // TODO: receive requests in while true until remote client closes the socket.
            while (true)
            {
                try
                {
                    // TODO: Receive request
                    byte[] receivedData = new byte[1024 * 1024];
                    int receivedLen = clientSocket.Receive(receivedData);
                    string receivedString = Encoding.ASCII.GetString(receivedData, 0, receivedLen);
                    // TODO: break the while loop if receivedLen==0
                    if (receivedLen == 0)
                        break;
                    // TODO: Create a Request object using received request string
                    Request clientRequest = new Request(receivedString);
                    // TODO: Call HandleRequest Method that returns the response
                    Response response = HandleRequest(clientRequest);
                    //Console.WriteLine(response.ResponseString);
                    // TODO: Send Response back to client
                    clientSocket.Send(Encoding.ASCII.GetBytes(response.ResponseString));

                }
                catch (Exception ex)
                {
                    Logger.LogException(ex);
                    // TODO: log exception using Logger class
                }
            }
            clientSocket.Close();
            // TODO: close client socket
        }

        Response HandleRequest(Request request)
        {
            string content ;
            string redirectionPath = string.Empty;
            try
            {
                //TODO: check for bad request 
                if (!request.ParseRequest())
                    return new Response(
                        StatusCode.BadRequest,
                        Configuration.ContentType,
                        LoadDefaultPage(Configuration.BadRequestDefaultPageName),
                        redirectionPath
                        );
                Dictionary<string, string> headerLines = request.HeaderLines; 

                //TODO: map the relativeURI in request to get the physical path of the resource.
                string physicalPath = Configuration.RootPath+ request.relativeURI.Replace('/','\\');
                //TODO: check for redirect
                redirectionPath = GetRedirectionPagePathIFExist(request.relativeURI);
                if (redirectionPath != string.Empty)
                {
                    return new Response(
                        StatusCode.Redirect, 
                        Configuration.ContentType, 
                        LoadDefaultPage(Configuration.RedirectionDefaultPageName),
                        redirectionPath
                        );
                }
                //TODO: check file exists
                content = LoadDefaultPage(physicalPath);
                if (content == string.Empty)
                    return new Response(
                        StatusCode.NotFound,
                        Configuration.ContentType,
                        LoadDefaultPage(Configuration.NotFoundDefaultPageName), 
                        redirectionPath
                        );
                //TODO: read the physical file
                return new Response(
                    StatusCode.OK, 
                    Configuration.ContentType,
                    content, 
                    redirectionPath
                    );
                // Create OK response
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                return new Response(
                    StatusCode.InternalServerError, 
                    Configuration.ContentType,
                    LoadDefaultPage(Configuration.InternalErrorDefaultPageName),
                    redirectionPath
                    );
                // TODO: log exception using Logger class
                // TODO: in case of exception, return Internal Server Error. 
            }
        }

        private string GetRedirectionPagePathIFExist(string relativePath)
        {
            // using Configuration.RedirectionRules return the redirected page path if exists else returns empty
            foreach (KeyValuePair<string, string> kvp in Configuration.RedirectionRules)
            {
                if ("/" + kvp.Key == relativePath)
                {
                    return kvp.Value;
                }
            }
            return string.Empty;
        }

        private string LoadDefaultPage(string defaultPageName)
        {
            string filePath = Path.Combine(Configuration.RootPath, defaultPageName);
            string content = string.Empty;
            // TODO: check if filepath not exist log exception using Logger class and return empty string
            try
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    content = reader.ReadToEnd();
                }
                return content;
            }
            catch (FileNotFoundException ex)
            {
                Logger.LogException(ex);
                return string.Empty;
            }
            // else read file and return its content
        }

        private void LoadRedirectionRules(string filePath)
        {
            Configuration.RedirectionRules = new Dictionary<string, string>();
            try
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        Configuration.RedirectionRules.Add(
                            line.Substring(0, line.IndexOf(',')),
                            line.Substring(line.IndexOf(',') + 1)
                            ); 
                        // Add to list.// Write to console.
                    }
                }
                // TODO: using the filepath paramter read the redirection rules from file 
                // then fill Configuration.RedirectionRules dictionary 
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                // TODO: log exception using Logger class
                Environment.Exit(1);
            }
        }
    }
}
