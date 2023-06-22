using System;
using System.Collections.Generic;
using System.Linq;
using BergerLeadCRMSchedular.ExceptionManager;
using BergerLeadCRMSchedular.Models.CRMLeadApi.Input;
using BergerLeadCRMSchedular.Models.CRMLeadApi.Output;
using BergerLeadCRMSchedular.Models.Helper.Constants;
using BergerLeadCRMSchedular.Models.Helper.ExternalApiCall;
using System.Configuration;
using BergerLeadCRMSchedular.Models.Helper;

namespace BergerLeadCRMSchedular.DB
{
    public class CRMLeadUndercoatsEnquiries
    {
        public void UpdateCRMLeadStatusForUndercoatsEnquiries(DateTime fromDate, DateTime toDate)
        {
            BergerEntities db = new BergerEntities();
            int maxRecords = Convert.ToInt32(ConfigurationManager.AppSettings["API.CRMBerger.MaxRecords"]);

            //DateTime fromDate = new DateTime(2019, 01, 01);
            //DateTime toDate = DateTime.UtcNow.AddMinutes(330);
            ////DateTime toDate = new DateTime(2019, 12, 30);

            try
            {
                //var dbData = (from be in db.tbl_UnderCoatEnquiry
                //                     where be.CRMLeadId != null
                //                     && (be.CRMLeadSubStatusId == null || be.CRMLeadSubStatu.CRMLeadMainStatu.CRMLeadMainStatus == CRMLeadStatus.CRM_LEAD_MAIN_STATUS)
                //                     select new LeadModel
                //                     {
                //                         Id = be.CRMLeadId
                //                     }
                //                     ).ToList();

                var dbData = db.tbl_UnderCoatEnquiry
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
                    LogException.Log("BergerLeadCRMSchedular.DB CRMLeadUndercoatsEnquiries.UpdateCRMLeadStatusForUndercoatsEnquiries :  Not data found in Database.");
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
                        MobileNo = getlist.MobileNo
                    });


                    try
                    {
                        var data = CRMBergerLeadImportApi.GetInstance().PostReturnList<InputModel, OutputModel>("lead/info", model);
                        var leadDetail = model.LeadDetails[0];

                        var undercoatenquiry = db.tbl_UnderCoatEnquiry
                                            .Where(d => d.CRMLeadId == leadDetail.Id)
                                            .Select(d => d)
                                            .FirstOrDefault();

                        if (undercoatenquiry != null)
                        {
                            var _data = data.FirstOrDefault();
                            string status = _data.Status;

                            int CRMLeadSubStatusId = db.CRMLeadSubStatus.Where(x => x.CRMLeadSubStatus.Trim() == status.Trim()).Select(x => x.CRMLeadSubStatusId).FirstOrDefault();
                            undercoatenquiry.CRMLeadSubStatusId = CRMLeadSubStatusId;
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
                            undercoatenquiry.CRMRecordId = _data.Id;
                            undercoatenquiry.CRMPincode = _data.PinCode;
                            undercoatenquiry.CRMLeadStatusUpdatedOn = DateTime.Now;
                            try
                            {
                                db.SaveChanges();
                            }
                            catch (Exception ex)
                            {
                                LogException.Log(string.Format("{0},{1},{2},{3}", "Exception in BergerLeadCRMSchedular.DB CRMLeadUndercoatsEnquiries.UpdateCRMLeadStatusForUndercoatsEnquiries : ", status, leadDetail.Id, ex.Message));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogException.Log(string.Format("CRMLeadUndercoatsEnquiries splitData exception : .{0}", ex.Message));
                    }

                }           
            }
            catch (Exception ex)
            {
                LogException.Log(string.Format("{0},{1}", "Exception in BergerLeadCRMSchedular.DB CRMLeadUndercoatsEnquiries.UpdateCRMLeadStatusForUndercoatsEnquiries : ", ex.Message));
            }

            LogException.Log("CRMLeadUndercoatsEnquiries Updated.");
        }
    }
}
