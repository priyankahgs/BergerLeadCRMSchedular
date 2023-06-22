using System;
using System.Collections.Generic;
using System.Linq;
using BergerLeadCRMSchedular.Models.CRMLeadApi.Input;
using BergerLeadCRMSchedular.Models.Helper.Constants;
using BergerLeadCRMSchedular.Models.CRMLeadApi.Output;
using BergerLeadCRMSchedular.Models.Helper.ExternalApiCall;
using BergerLeadCRMSchedular.ExceptionManager;
using System.Configuration;
using BergerLeadCRMSchedular.Models.Helper;

namespace BergerLeadCRMSchedular.DB
{
    public class CRMBulkDailyhuntLeads
    {
        public void UpdateCRMLeadStatusForBulkDailyhuntLeads(DateTime fromDate, DateTime toDate)
        {
            BergerEntities db = new BergerEntities();
            int maxRecords = Convert.ToInt32(ConfigurationManager.AppSettings["API.CRMBerger.MaxRecords"]);

            //DateTime fromDate = new DateTime(2020, 01, 01);
            //DateTime toDate = DateTime.UtcNow.AddMinutes(330);
            ////DateTime toDate = new DateTime(2020, 06, 30);

            try
            {
                //DateTime fromDate = new DateTime(2018, 9, 7);
                //DateTime toDate = new DateTime(2018, 9, 28);
                //DateTime toDate = DateTime.Now;

                //var dbData = (from fbleads in db.CRMDailyhuntLeads
                //              where fbleads.CRMLeadId != null
                //              && (fbleads.CRMLeadSubStatusId == null || fbleads.CRMLeadSubStatu.CRMLeadMainStatu.CRMLeadMainStatus == CRMLeadStatus.CRM_LEAD_MAIN_STATUS)
                //               //&& (fbleads.CRMLeadSubStatusId == null )
                //               //&& fbleads.CreatedDate >= fromDate && fbleads.CreatedDate <= toDate                              
                //              select new LeadModel
                //              {
                //                  Id = fbleads.CRMLeadId
                //              }
                //            ).ToList();

                var dbData = db.CRMDailyhuntLeads
                         .Where(d => d.CRMLeadId != null
                         //&& d.CRMLeadSubStatusId == null
                         && d.CRMLeadSubStatusId != null && (d.CRMPincode == null || d.CRMPincode == "")
                          ////&& (d.CRMLeadSubStatusId == null || d.CRMLeadSubStatu.CRMLeadMainStatu.CRMLeadMainStatus == CRMLeadStatus.CRM_LEAD_MAIN_STATUS)
                          && d.CreatedDate >= fromDate && d.CreatedDate <= toDate                          
                          )
                          .OrderBy(d => d.CreatedDate)
                          .ToList();

                if (dbData == null || dbData.Count == 0)
                {
                    LogException.Log("BergerLeadCRMSchedular.DB BergerLeadCRMSchedular.DB CRMBulkDailyhuntLeads.UpdateCRMLeadStatusForBulkDailyhuntLeads :  Not data found in Database.");
                    return;
                }

                LogException.Log("BergerLeadCRMSchedular.DB CRMBulkDailyhuntLeads.UpdateCRMLeadStatusForDailyhuntLeads :  Query Executed. Data Count is " + dbData.Count + "");

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
                        LogException.Log("Dailyhunt Lead Id : " + model.LeadDetails[0].Id);
                        var data = CRMBergerLeadImportApi.GetInstance().PostReturnList<InputModel, OutputModel>("lead/info", model);
                        var leadDetail = model.LeadDetails[0];

                        var facebookleadquery = db.CRMDailyhuntLeads
                                            .Where(d => d.CRMLeadId == leadDetail.Id)
                                            .Select(d => d)
                                            .FirstOrDefault();

                        if (facebookleadquery != null)
                        {
                            var _data = data.FirstOrDefault();
                            string status = _data.Status;

                            LogException.Log("Dailyhunt Enquiry Status : " + status);

                            int CRMLeadSubStatusId = db.CRMLeadSubStatus.Where(x => x.CRMLeadSubStatus.Trim() == status.Trim()).Select(x => x.CRMLeadSubStatusId).FirstOrDefault();
                            facebookleadquery.CRMLeadSubStatusId = CRMLeadSubStatusId;
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
                            facebookleadquery.CRMRecordId = _data.Id;
                            facebookleadquery.CRMPincode = _data.PinCode;
                            facebookleadquery.CRMLeadStatusUpdatedOn = DateTime.Now;
                            try
                            {
                                db.SaveChanges();
                                LogException.Log("DailyHunt Status Updated.");
                            }
                            catch (Exception ex)
                            {
                                LogException.Log(string.Format("{0},{1},{2},{3}", "Exception in BergerLeadCRMSchedular.DB CRMBulkDailyhuntLeads.UpdateCRMLeadStatusForBulkDailyhuntLeads : ", status, leadDetail.Id, ex.Message));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogException.Log(string.Format("CRMBulkDailyhuntLeads splitData exception : .{0} => {1}", ex.Message, ex.InnerException));
                    }

                }
            }
            catch (Exception ex)
            {
                LogException.Log(string.Format("{0},{1}", "Exception in BergerLeadCRMSchedular.DB CRMBulkDailyhuntLeads.UpdateCRMLeadStatusForBulkDailyhuntLeads : ", ex.Message));
            }

            LogException.Log("CRMBulkDailyhuntLeads Updated.");

        }
    }
}
