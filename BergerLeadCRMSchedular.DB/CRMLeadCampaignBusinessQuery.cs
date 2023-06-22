using System;
using System.Collections.Generic;
using System.Linq;
using BergerLeadCRMSchedular.ExceptionManager;
using BergerLeadCRMSchedular.Models.CRMLeadApi.Input;
using BergerLeadCRMSchedular.Models.CRMLeadApi.Output;
using BergerLeadCRMSchedular.Models.Helper.Constants;
using BergerLeadCRMSchedular.Models.Helper.ExternalApiCall;
using BergerLeadCRMSchedular.Models.Helper;
using System.Configuration;

namespace BergerLeadCRMSchedular.DB
{
    public class CRMLeadCampaignBusinessQuery
    {
        public void UpdateCRMLeadStatusForCampaignBusinessQuaries(DateTime fromDate, DateTime toDate)
        {
            BergerEntities db = new BergerEntities();
            int maxRecords = Convert.ToInt32(ConfigurationManager.AppSettings["API.CRMBerger.MaxRecords"]);
            try
            {                
                LogException.Log("BergerLeadCRMSchedular.DB CRMLeadCampaignBusinessQuaries.UpdateCRMLeadStatusForCampaignBusinessQuaries :  Query Starts.");

                var dbData = db.CampaignBusinessQueries
                           .Where(d => d.CRMLeadId != null
                           ////&& (d.CRMLeadSubStatusId == null || d.CRMLeadSubStatu.CRMLeadMainStatu.CRMLeadMainStatus == CRMLeadStatus.CRM_LEAD_MAIN_STATUS)
                           //&& d.CRMLeadSubStatusId == null
                           && d.CRMLeadSubStatusId != null && (d.CRMPincode == null || d.CRMPincode == "")
                           && d.CreatedDate >= fromDate && d.CreatedDate <= toDate                            
                            )
                            .OrderBy(d => d.CreatedDate)
                            .ToList();

                if (dbData == null || dbData.Count == 0)
                {
                    LogException.Log("BergerLeadCRMSchedular.DB CRMLeadCampaignBusinessQuaries.UpdateCRMLeadStatusForCampaignBusinessQuaries :  Not data found in Database.");
                    return;
                }

                LogException.Log("BergerLeadCRMSchedular.DB CRMLeadCampaignBusinessQuaries.UpdateCRMLeadStatusForCampaignBusinessQuaries :  Query Executed. Data Count is " + dbData.Count + "");

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
                        LogException.Log("BE Lead Id : " + model.LeadDetails[0].Id);

                        var data = CRMBergerLeadImportApi.GetInstance().PostReturnList<InputModel, OutputModel>("lead/info", model);

                        var leadDetail = model.LeadDetails[0];

                        var businessquery = db.CampaignBusinessQueries
                                            .Where(d => d.CRMLeadId == leadDetail.Id)
                                            .Select(d => d)
                                            .FirstOrDefault();

                        if (businessquery != null)
                        {
                            var _data = data.FirstOrDefault();
                            string status = _data.Status;
                            string crmRecordId = _data.Id;
                            string recordId = businessquery.CRMRecordId;

                            LogException.Log("BE Status : " + status);

                            int CRMLeadSubStatusId = db.CRMLeadSubStatus.Where(x => x.CRMLeadSubStatus.Trim() == status.Trim()).Select(x => x.CRMLeadSubStatusId).FirstOrDefault();
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

                            businessquery.CRMLeadSubStatusId = CRMLeadSubStatusId;
                            if (string.IsNullOrWhiteSpace(recordId))
                            {
                                businessquery.CRMRecordId = crmRecordId;
                            }
                            businessquery.CRMPincode = _data.PinCode;
                            businessquery.CRMLeadStatusUpdatedOn = DateTime.Now;
                            try
                            {
                                db.SaveChanges();
                                LogException.Log("BE Status Updated.");
                            }
                            catch (Exception ex)
                            {
                                LogException.Log(string.Format("{0},{1},{2},{3},{4}", "Exception in BergerLeadCRMSchedular.DB CRMLeadCampaignBusinessQuaries.UpdateCRMLeadStatusForCampaignBusinessQuaries : ", status, leadDetail.Id, ex.Message, ex.InnerException));
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        LogException.Log(string.Format("CRMLeadCampaignBusinessQuaries splitData exception : .{0}", ex.Message));
                    }

                }


            }
            catch (Exception ex)
            {
                LogException.Log(string.Format("{0},{1}", "Exception in BergerLeadCRMSchedular.DB CRMLeadCampaignBusinessQuaries.UpdateCRMLeadStatusForCampaignBusinessQuaries : ", ex.Message));
            }
        }
    }
}
