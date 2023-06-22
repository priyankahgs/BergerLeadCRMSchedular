using System;
using BergerLeadCRMSchedular.DB;
using BergerLeadCRMSchedular.ExceptionManager;

namespace BergerLeadCRMSchedular.BusinessLayer
{
    public class CRMLeadBL
    {
        public void UpdateCRMLeadStatus()
        {
            try
            {
                DateTime fromDate = DateTime.UtcNow.AddMinutes(330).AddDays(-15).Date.AddHours(00).AddMinutes(00).AddSeconds(00);
                //DateTime fromDate = new DateTime(2023,06, 14).AddMinutes(330).Date.AddHours(00).AddMinutes(00).AddSeconds(00);
                DateTime toDate = DateTime.UtcNow.AddMinutes(330).AddDays(-1).Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                //DateTime toDate = new DateTime(2023, 06, 14).AddMinutes(330).Date.AddHours(23).AddMinutes(59).AddSeconds(59);

                LogException.Log("fromDate : " + fromDate);
                LogException.Log("toDate : " + toDate);

                //////Business Quaries
                new CRMLeadBusinessQuaries().UpdateCRMLeadStatusForBusinessQuaries(fromDate, toDate);

                //////Campaign Business Quaries
                new CRMLeadCampaignBusinessQuery().UpdateCRMLeadStatusForCampaignBusinessQuaries(fromDate, toDate);

                //////CRMVendor Leads
                new CRMVendorLeads().UpdateCRMVendorLeads(fromDate, toDate);

                //////Ask Consultant
                new CRMLeadAskConsultant().UpdateCRMLeadStatusForAskConsultant(fromDate, toDate);

                //////Product Enquiries
                new CRMLeadProductEnquiries().UpdateCRMLeadStatusForProductEnquiries(fromDate, toDate);

                //////Construction Chemical Enquiries
                new CRMLeadConstructionChemicalEnquiries().UpdateCRMLeadStatusForConstructionChemicalEnquiries(fromDate, toDate);

                //////Undercoats Enquiries
                new CRMLeadUndercoatsEnquiries().UpdateCRMLeadStatusForUndercoatsEnquiries(fromDate, toDate);

                //////IndustrialUsers Enquiries
                new CRMLeadIndustrialUsersEnquiries().UpdateCRMLeadStatusForIndustrialUsersEnquiries(fromDate, toDate);

                //////HomePainting Enquiries
                new CRMLeadHomePaintingEnquiries().UpdateCRMLeadStatusForHomePaintingEnquiries(fromDate, toDate);

                ////Bulk Instagram Leads
                new CRMBulkInstagramLeads().UpdateCRMLeadStatusForBulkInstagramLeads(fromDate, toDate);

                ////Bulk LinkedIn Leads
                new CRMBulkLinkedInLeads().UpdateCRMLeadStatusForBulkLinkedInLeads(fromDate, toDate);

                ////Bulk CRM Facebook ECF Leads
                new CRMBulkFacebookECFLeads().UpdateCRMLeadStatusForBulkFacebookECFLeads(fromDate, toDate);

                ////Bulk CRM Facebook SBE Leads
                new CRMBulkFacebookSBELeads().UpdateCRMLeadStatusForBulkFacebookSBELeads(fromDate, toDate);

                ////Bulk CRM Dailyhunt Leads
                new CRMBulkDailyhuntLeads().UpdateCRMLeadStatusForBulkDailyhuntLeads(fromDate, toDate);

                ////Bulk CRM Google Leads
                new CRMBulkGoogleLeads().UpdateCRMLeadStatusForBulkGoogleLeads(fromDate, toDate);

                ////Bulk Facebook Leads
                new CRMBulkFacebookLeads().UpdateCRMLeadStatusForBulkFacebookLeads(fromDate, toDate);

            }
            catch (Exception ex)
            {
                LogException.Log(string.Format("{0},{1}", "Exception in BergerLeadCRMSchedular.BusinessLayer CRMLeadBL.UpdateCRMLeadStatus() : ", ex.Message));               
            }

            LogException.Log(string.Format("{0},{1}", "Run Successfully at : ", DateTime.Now));    
        }
    }
}
