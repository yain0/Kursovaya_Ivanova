using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TourService.ViewModels
{
    [DataContract]
    public class TourViewModel
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Date { get; set; }

        [DataMember]
        public string Place { get; set; }

        [DataMember]
        public decimal Price { get; set; }

        public bool Equipment { get; set; }

        public int Count { get; set; }

        public decimal Total { get; set; }
    }
}
