using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BergerLeadCRMSchedular.BusinessLayer;

namespace BergerLeadCRMSchedular.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            new CRMLeadBL().UpdateCRMLeadStatus();            
        }
    }
}
        