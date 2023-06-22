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
    public class CRMLeadAskConsultant
    {
        public void UpdateCRMLeadStatusForAskConsultant(DateTime fromDate, DateTime toDate)
        {
            BergerEntities db = new BergerEntities();
            int maxRecords = Convert.ToInt32(ConfigurationManager.AppSettings["API.CRMBerger.MaxRecords"]);

            //DateTime fromDate = new DateTime(2019, 01, 01);
            //DateTime toDate = DateTime.UtcNow.AddMinutes(330);
            ////DateTime toDate = new DateTime(2019, 12, 30);

            try
            {
                //var dbData = (from be in db.tbl_AskConsultant
                //              where be.CRMLeadId != null
                //              && (be.CRMLeadSubStatusId == null || be.CRMLeadSubStatu.CRMLeadMainStatu.CRMLeadMainStatus == CRMLeadStatus.CRM_LEAD_MAIN_STATUS)
                //              select new LeadModel
                //              {
                //                  Id = be.CRMLeadId
                //              }
                //            ).ToList();

                var dbData = db.tbl_AskConsultant
                          .Where(d => d.CRMLeadId != null
                           //&& d.CRMLeadSubStatusId == null //(d.CRMLeadSubStatusId == null || d.CRMLeadSubStatu.CRMLeadMainStatu.CRMLeadMainStatus == CRMLeadStatus.CRM_LEAD_MAIN_STATUS)
                           && d.CRMLeadSubStatusId != null && (d.CRMPincode == null || d.CRMPincode == "")
                           && d.CreatedDate >= fromDate && d.CreatedDate <= toDate                            
                           )
                           .OrderBy(d => d.CreatedDate)
                           .ToList();


                if (dbData == null || dbData.Count == 0)
                {
                    LogException.Log("BergerLeadCRMSchedular.DB CRMLeadAskConsultant.UpdateCRMLeadStatusForAskConsultant :  Not data found in Database.");
                    return;
                }

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
                        var data = CRMBergerLeadImportApi.GetInstance().PostReturnList<InputModel, OutputModel>("lead/info", model);
                        
                        var leadDetail = model.LeadDetails[0];

                        var askConsulatantQuery = db.tbl_AskConsultant
                                            .Where(d => d.CRMLeadId == leadDetail.Id)
                                            .Select(d => d)
                                            .FirstOrDefault();

                        if (askConsulatantQuery != null)
                        {
                            var _data = data.FirstOrDefault();
                            string status = _data.Status;

                            int CRMLeadSubStatusId = db.CRMLeadSubStatus.Where(x => x.CRMLeadSubStatus.Trim() == status.Trim()).Select(x => x.CRMLeadSubStatusId).FirstOrDefault();
                            askConsulatantQuery.CRMLeadSubStatusId = CRMLeadSubStatusId;
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
                            askConsulatantQuery.CRMRecordId = _data.Id;
                            askConsulatantQuery.CRMPincode = _data.PinCode;
                            askConsulatantQuery.CRMLeadStatusUpdatedOn = DateTime.Now;
                            try
                            {
                                db.SaveChanges();
                            }
                            catch (Exception ex)
                            {
                                LogException.Log(string.Format("{0},{1},{2},{3}", "BergerLeadCRMSchedular.DB CRMLeadAskConsultant.UpdateCRMLeadStatusForAskConsultant : ", status, leadDetail.Id, ex.Message));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogException.Log(string.Format("CRMLeadAskConsultant splitData exception : .{0}", ex.Message));
                    }
                }               
            }
            catch (Exception ex)
            {
                LogException.Log(string.Format("{0},{1}", "Exception in BergerLeadCRMSchedular.DB CRMLeadAskConsultant.UpdateCRMLeadStatusForAskConsultant : ", ex.Message));
            }

            LogException.Log("CRMLeadAskConsultant Updated.");
        }
    }
}
