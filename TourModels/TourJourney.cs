using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TourModels
{
    [DataContract]
    public class TourJourney
    {
        [DataMember]
        [Key, Column(Order = 1)]
        public int JourneyId { get; set; }

        [DataMember]
        [Key, Column(Order = 2)]
        public int TourId { get; set; }

        [DataMember]
        public int Count { get; set; }

        [DataMember]
        public decimal Price { get; set; }

        public virtual Journey Journey { get; set; }

        public virtual Tour Tour { get; set; }
    }
}
