using System;

namespace DocuSign.Models
{
    public class DocuSignConfiguration
    {
        public bool Enabled { get; set; }
        public string EmailSubject { get; set; }

        public string IntegratorKey { get; set; }

        public string UserId { get; set; }
        public string PrivateKey { get; set; }

        public string RestApiUrl { get; set; }

        public string SignHereTagAnchorString { get; set; }
        public string SignDateTagAnchorString { get; set; }        

        public DateTime DocuSignSynchronizationJobLastExecute { get; set; }
    }
}
