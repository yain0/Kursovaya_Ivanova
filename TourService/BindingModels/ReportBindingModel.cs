using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TourService.BindingModels
{
    [DataContract]
    public class ReportBindingModel
    {
        [DataMember]
        public DateTime DateFrom { get; set; }

        [DataMember]
        public DateTime DateTo { get; set; }

        [DataMember]
        public string FontPath { get; set; }

        [DataMember]
        public string FilePath { get; set; }

        [DataMember]
        public string ClientId { get; set; }
    }
}
