using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HTTPServer
{
    public enum RequestMethod
    {
        GET,
        POST,
        HEAD
    }

    public enum HTTPVersion
    {
        HTTP10,
        HTTP11,
        HTTP09
    }

    class Request
    {
        string[] requestLines;
        RequestMethod method;
        public string relativeURI;
        Dictionary<string, string> headerLines;

        public Dictionary<string, string> HeaderLines
        {
            get { return headerLines; }
        }

        HTTPVersion httpVersion;
        string requestString;
        string[] contentLines;

        public Request(string requestString)
        {
            this.requestString = requestString;
        }
        /// <summary>
        /// Parses the request string and loads the request line, header lines and content, returns false if there is a parsing error
        /// </summary>
        /// <returns>True if parsing succeeds, false otherwise.</returns>
        public bool ParseRequest()
        {
            int count = 0;
            //TODO: parse the receivedRequest using the \r\n delimeter   
            // check that there is atleast 3 lines: Request line, Host Header, Blank line (usually 4 lines with the last empty line for empty content)
            for (int i = 0; i < requestString.Length; i++) {
                if (requestString.Length-1 > i && requestString[i] == '\r' && requestString[i+1] == '\n')
                {
                    count++;
                }
            }
            if (count < 3)
                return false;
            // Parse Request line
            if (!ParseRequestLine())
                return false;
            // Validate blank line exists
            if (!ValidateBlankLine())
                return false;
            // Load header lines into HeaderLines dictionary
            if (!LoadHeaderLines())
                return false;
            for(int i = 0; i <= headerLines.Count; i++)
            {
                requestString.Substring(requestString.IndexOf('\n') + 1);
            }
            contentLines = requestString.Split('\n');
            return true;
        }

        private bool ParseRequestLine()
        {
            requestLines = requestString.Split(' ');
            try
            {
                method = (RequestMethod)System.Enum.Parse(typeof(RequestMethod), requestLines[0]);
            }
            catch (Exception)
            {
                return false;
            }
            try
            {
                httpVersion = (HTTPVersion)System.Enum.Parse(typeof(HTTPVersion),
                    requestLines[2].Substring(0,
                    requestLines[2].IndexOf('\n') )
                    .Replace(".", string.Empty)
                    .Replace("/", string.Empty));
            }
            catch(Exception)
            {
                return false;
            }

            if (!ValidateIsURI(requestLines[1]))
                return false;
            relativeURI = requestLines[1];
            requestLines[2] = requestLines[2].Substring(0, requestLines[2].IndexOf('\n') );
            requestString = requestString.Substring(requestString.IndexOf('\n') + 1);
            return true;
        }

        private bool ValidateIsURI(string uri)
        {
            return Uri.IsWellFormedUriString(uri, UriKind.RelativeOrAbsolute);
        }

        private bool LoadHeaderLines()
        {
            string tempString = requestString.Substring(0, requestString.IndexOf("\r\n\r\n"));
            Console.WriteLine(tempString);
            try
            {
                headerLines = tempString.Split('\n')
                            .Select(part => part.Split(':'))
                            .Where(part => part.Length == 2)
                            .ToDictionary(sp => sp[0], sp => sp[1]);
            }
            catch(Exception ex)
            {
                return false;
            }
            if (headerLines.Count == 0)
                return false;
            return true;
            }

        private bool ValidateBlankLine()
        {
            if (requestString.Contains("\r\n\r\n"))
                return true;
            else return false;
        }

    }
}
