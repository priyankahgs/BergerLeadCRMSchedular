using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BergerLeadCRMSchedular.Models.Helper.ExternalApiCall
{
    public class CRMBergerLeadImportApi : ApiCall
    {
        private static readonly CRMBergerLeadImportApi Instance = new CRMBergerLeadImportApi();

        private CRMBergerLeadImportApi()
            : base("API.CRMBerger.LeadImport.BaseUrl", "API.CRMBerger.LeadImport.Token")
        {
        }

        public static CRMBergerLeadImportApi GetInstance()
        {
            return Instance;
        }
    }
}
