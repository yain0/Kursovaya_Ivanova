using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TourService.BindingModels
{
    [DataContract]
    public class JourneyBindingModel
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string ClientId { get; set; }

        [DataMember]
        public List<TourJourneyBindingModel> TourJourneys { get; set; }
    }
}
