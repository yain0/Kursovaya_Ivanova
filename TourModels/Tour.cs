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
    public class Tour
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public DateTime Date { get; set; }

        [DataMember]
        public string Place { get; set; }

        [DataMember]
        public decimal Price { get; set; }

        [DataMember]
        public bool Equipment { get; set; }

        [ForeignKey("TourId")]
        public virtual List<TourJourney> TourJourneys { get; set; }
    }
}
