using System;
using PrimeDocs.ConnectClient.WinApp.Repository;

namespace PrimeDocs.ConnectClient.WinApp.ViewModel
{
    public class Request
    {
        public string Uri { get; set; }

        public CData Username { get; set; }

        public CData Password { get; set; }

        public CData Content { get; set; }

        public DateTime Date { get; set; }
    }
}
