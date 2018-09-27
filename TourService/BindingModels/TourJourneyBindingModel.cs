using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TourService.BindingModels
{
    [DataContract]
    public class TourJourneyBindingModel
    {
        [DataMember]
        public int TourId { get; set; }

        [DataMember]
        public int Count { get; set; }
    }
}
