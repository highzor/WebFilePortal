using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WebFileService
{

    [DataContract]
    public class DocumentDTO
    {
        
        [DataMember]
        public string FileName { get; set; }
        [DataMember]
        public Guid FileId { get; set; }
      
        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public string FileNameInFileStorage { get; set; }

        [DataMember]
        public byte[] Content { get; set; }
        [DataMember]
        public DateTime? CreateDate { get; set; }
        [DataMember]
        public string MimeType { get; set; }
        [DataMember]
        public string UserName { get; set; }
        [DataMember]
        public Guid UserId { get; set; }

        [DataMember]
        public decimal FileSize { get; set; }
    }
}