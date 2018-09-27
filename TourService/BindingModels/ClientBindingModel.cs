using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TourService.BindingModels
{
    [DataContract]
    public class ClientBindingModel
    {
        [DataMember]
        public string UserName { get; set; }

        [DataMember]
        public string PasswordHash { get; set; }

        [DataMember]
        public string FIO { get; set; }
    }
}
