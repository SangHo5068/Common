using System.Collections.Generic;

namespace Common.Net.Stomp
{
    public class StompMessage
    {
        private readonly IDictionary<string, string> _headers = new Dictionary<string, string>();

        public IDictionary<string, string> Headers
        {
            get => _headers;
        }

        /// <summary>
        /// Gets the body.
        /// </summary>
        public string Body { get; private set; }

        /// <summary>
        /// Gets the command.
        /// </summary>
        public string Command { get; private set; }


        /// <summary>
        ///   Initializes a new instance of the <see cref = "StompMessage" /> class.
        /// </summary>
        /// <param name = "command">The command.</param>
        public StompMessage(string command)
            : this(command, string.Empty)
        {
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref = "StompMessage" /> class.
        /// </summary>
        /// <param name = "command">The command.</param>
        /// <param name = "body">The body.</param>
        public StompMessage(string command, string body)
            : this(command, body, new Dictionary<string, string>())
        {
        }

        public StompMessage(string command, IDictionary<string, string> headers)
            : this(command, string.Empty, headers)
        {
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref = "StompMessage" /> class.
        /// </summary>
        /// <param name = "command">The command.</param>
        /// <param name = "body">The body.</param>
        /// <param name = "headers">The headers.</param>
        public StompMessage(string command, string body, IDictionary<string, string> headers)
        {
            Command = command;
            Body = body;
            _headers = headers;

            this["content-length"] = body.Length.ToString();
        }

        /// <summary>
        /// Gets or sets the specified header attribute.
        /// </summary>
        public string this[string header]
        {
            get => _headers.ContainsKey(header) ? _headers[header] : string.Empty;
            set => _headers[header] = value;
        }
    }
}
