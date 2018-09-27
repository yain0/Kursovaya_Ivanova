using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TourModels
{
    [DataContract]
    public class Journey
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string ClientId { get; set; }

        [DataMember]
        public DateTime DateCreate { get; set; }

        public virtual Client Client { get; set; }

        [ForeignKey("JourneyId")]
        public virtual List<TourJourney> TourJourneys { get; set; }
    }
}
