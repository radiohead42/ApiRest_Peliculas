using System.Net;
using System.Collections.Generic;

namespace RESTAPI.Models
{
    public class RespuestasApi
    {
        public RespuestasApi()
        {
            ErrorMessage = new List<string>();
        }
        
        public HttpStatusCode StatusCode { get; set; }
        public bool IsSucces { get; set; } = true;
        public List<string> ErrorMessage { get; set; }
        public object Result { get; set; }
    }
}