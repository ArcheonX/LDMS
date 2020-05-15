using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LDMS.ViewModels
{
    public class FileUpload
    { 
        public string Extension { get; set; } 
        public string FileName { get; set; } 
        public int FileSize { get; set; } 
        public string Contents { get; set; }  
        public string FileType { get; set; }
    }
}
