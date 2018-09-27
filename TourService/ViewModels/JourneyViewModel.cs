using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TourService.ViewModels
{
    [DataContract]
    public class JourneyViewModel
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Date { get; set; }

        [DataMember]
        public List<TourViewModel> Tours { get; set; }

        public string Email { get; set; }
    }
}
