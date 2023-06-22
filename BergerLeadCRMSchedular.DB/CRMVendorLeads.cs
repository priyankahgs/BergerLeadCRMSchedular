using BergerLeadCRMSchedular.ExceptionManager;
using BergerLeadCRMSchedular.Models.CRMLeadApi.Input;
using BergerLeadCRMSchedular.Models.CRMLeadApi.Output;
using BergerLeadCRMSchedular.Models.Helper.Constants;
using BergerLeadCRMSchedular.Models.Helper.ExternalApiCall;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace BergerLeadCRMSchedular.DB
{
    public class CRMVendorLeads
    {
        public void UpdateCRMVendorLeads(DateTime fromDate, DateTime toDate)
        {
            BergerEntities db = new BergerEntities();
            int maxRecords = Convert.ToInt32(ConfigurationManager.AppSettings["API.CRMBerger.MaxRecords"]);

            //DateTime fromDate = new DateTime(2019, 01, 01);
            //DateTime toDate = DateTime.UtcNow.AddMinutes(330);
            ////DateTime toDate = new DateTime(2020, 06, 09);

            try
            {
                LogException.Log("BergerLeadCRMSchedular.DB BergerLeadCRMSchedular.DB CRMBulkVendorLeads.UpdateCRMLeadStatusForBulkVendorLeads :  Fetching Records.");

                var dbData = db.crmVendorLeads
                         .Where(d => d.CRMLeadId != null && d.IsActive == true
                         //&& d.CRMLeadSubStatusId == null //(d.CRMLeadSubStatusId == null || d.CRMLeadSubStatu.CRMLeadMainStatu.CRMLeadMainStatus == CRMLeadStatus.CRM_LEAD_MAIN_STATUS)
                         && d.CRMLeadSubStatusId != null && (d.CRMPincode == null || d.CRMPincode == "")
                         && d.CreatedDate >= fromDate && d.CreatedDate <= toDate
                          )
                          .OrderBy(d => d.CreatedDate)
                          .ToList();

                if (dbData == null || dbData.Count == 0)
                {
                    LogException.Log("BergerLeadCRMSchedular.DB BergerLeadCRMSchedular.DB CRMBulkVendorLeads.UpdateCRMLeadStatusForBulkVendorLeads :  Not data found in Database.");
                    return;
                }

                LogException.Log("BergerLeadCRMSchedular.DB BergerLeadCRMSchedular.DB CRMBulkVendorLeads.UpdateCRMLeadStatusForBulkVendorLeads :  Fetching Records Completed. Data Count is " + dbData.Count + "");

                //var splitData = ListExtensions.splitList(dbData, maxRecords);
                //List<OutputModel> FinalOutputModel = new List<OutputModel>();
                foreach (var getlist in dbData)
                {
                    InputModel model = new InputModel();
                    model.Origin = CRMLeadSource.WEBSITE;
                    model.LeadDetails = new List<LeadModel>();
                    model.LeadDetails.Add(new LeadModel()
                    {
                        Id = getlist.CRMLeadId,
                        MobileNo = getlist.Telephone
                    });

                    try
                    {
                                               

                        LogException.Log("Lead Id : " + model.LeadDetails[0].Id);

                        var data = CRMBergerLeadImportApi.GetInstance().PostReturnList<InputModel, OutputModel>("lead/info", model);
                        var leadDetail = model.LeadDetails[0];

                        var VendorLeadquery = db.crmVendorLeads
                                            .Where(d => d.CRMLeadId == leadDetail.Id)
                                            .Select(d => d)
                                            .FirstOrDefault();

                        if (VendorLeadquery != null)
                        {
                            var _data = data.FirstOrDefault();
                            string status = _data.Status;

                            LogException.Log("Status : " + status);

                            int CRMLeadSubStatusId = db.CRMLeadSubStatus.Where(x => x.CRMLeadSubStatus.Trim() == status.Trim()).Select(x => x.CRMLeadSubStatusId).FirstOrDefault();
                            VendorLeadquery.CRMLeadSubStatusId = CRMLeadSubStatusId;

                            LogException.Log("CRMLeadSubStatusId : " + CRMLeadSubStatusId);

                            if (CRMLeadSubStatusId == 0)
                            {
                                CRMLeadSubStatu cRMLeadSubStatu = new CRMLeadSubStatu();
                                cRMLeadSubStatu.CRMLeadSubStatus = status;
                                cRMLeadSubStatu.CRMLeadMainStatusId = 3;
                                cRMLeadSubStatu.IsActive = true;
                                cRMLeadSubStatu.CreatedDate = DateTime.Now;
                                db.CRMLeadSubStatus.AddObject(cRMLeadSubStatu);
                                db.SaveChanges();

                                CRMLeadSubStatusId = cRMLeadSubStatu.CRMLeadSubStatusId;
                            }

                            VendorLeadquery.CRMRecordId = _data.Id;
                            VendorLeadquery.CRMPincode = _data.PinCode;
                            VendorLeadquery.CRMLeadStatusUpdatedOn = DateTime.Now;
                            try
                            {
                                db.SaveChanges();
                                LogException.Log("Status Updated.");
                            }
                            catch (Exception ex)
                            {
                                LogException.Log(string.Format("{0},{1},{2},{3}", "Exception in BergerLeadCRMSchedular.DB CRMBulkVendorLeads.UpdateCRMLeadStatusForBulkVendorLeads : ", status, leadDetail.Id, ex.Message));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogException.Log(string.Format("CRMBulkVendorLeads splitData exception : .{0}", ex.Message));
                    }
                }
            }
            catch (Exception ex)
            {
                LogException.Log(string.Format("{0},{1}", "Exception in BergerLeadCRMSchedular.DB CRMBulkVendorLeads.UpdateCRMLeadStatusForBulkVendorLeads : ", ex.Message));
            }
        }
    }
}
