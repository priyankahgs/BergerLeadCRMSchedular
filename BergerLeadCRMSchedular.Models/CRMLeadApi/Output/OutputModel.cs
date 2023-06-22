using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BergerLeadCRMSchedular.Models.CRMLeadApi.Output
{
    public class OutputModel
    {
      public string Id {get;set;}
      public string Name {get;set;}
      public string Email {get;set;}
      public string Telephone {get;set;}
      public string Status { get; set; }
      public string PinCode { get; set; }
    }
}
