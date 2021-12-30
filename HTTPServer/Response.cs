using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace HTTPServer
{

    public enum StatusCode
    {
        OK = 200,
        InternalServerError = 500,
        NotFound = 404,
        BadRequest = 400,
        Redirect = 301
    }

    class Response
    {
        string responseString;
        public string ResponseString
        {
            get
            {
                return responseString;
            }
        }
        StatusCode code;
        List<string> headerLines = new List<string>();
        public Response(StatusCode code, string contentType, string content, string redirectoinPath)
        {
            // TODO: Add headlines (Content-Type, Content-Length,Date, [location if there is redirection])
            string statusLine= GetStatusLine(code);
            headerLines.Add("Content-Type: " + contentType + "\r\n");
            headerLines.Add("Content-Length: " + content.Length + "\r\n");
            DateTime currentDateTime = DateTime.Now;
            headerLines.Add("Date: " + currentDateTime + "\r\n");
            if(redirectoinPath.Length>0)
            headerLines.Add("location: " + redirectoinPath + "\r\n");
            // TODO: Create the request string
            responseString +=Configuration.ServerHTTPVersion + " " + statusLine+"\r\n";
            foreach(string s in headerLines)
            {
                responseString += s;
            }
            responseString += "\r\n";
            responseString += content;
        }

        private string GetStatusLine(StatusCode code)
        {
            // TODO: Create the response status line and return it

            int codeNum = (int)code;
            string errorType = code.ToString();
            string statusLine = codeNum +" "+errorType;
            return statusLine;
        }
    }
}