using System;
using System.Collections.Generic;
using System.Text;

namespace LDMS.ViewModels.SearchModel
{
    public class ResultMessage
    {
        public bool result { get; set; }
        public string message { get; set; }
        public dynamic data { get; set; }
    }
}
