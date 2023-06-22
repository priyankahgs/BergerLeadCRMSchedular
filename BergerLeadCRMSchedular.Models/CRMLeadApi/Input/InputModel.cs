using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BergerLeadCRMSchedular.Models.CRMLeadApi.Input
{
    public class InputModel
    {
        public string Origin { get; set; }
        public List<LeadModel> LeadDetails { get; set; }
    }
}
