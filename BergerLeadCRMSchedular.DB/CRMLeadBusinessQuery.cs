using BergerLeadCRMSchedular.ExceptionManager;
using BergerLeadCRMSchedular.Models.CRMLeadApi.Input;
using BergerLeadCRMSchedular.Models.CRMLeadApi.Output;
using BergerLeadCRMSchedular.Models.Helper.Constants;
using BergerLeadCRMSchedular.Models.Helper.ExternalApiCall;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace BergerLeadCRMSchedular.DB
{
    public class CRMLeadBusinessQuaries
    {
        public void UpdateCRMLeadStatusForBusinessQuaries(DateTime fromDate, DateTime toDate)
        {
            BergerEntities db = new BergerEntities();
            int maxRecords = Convert.ToInt32(ConfigurationManager.AppSettings["API.CRMBerger.MaxRecords"]);
            try
            {

                //DateTime fromDate = new DateTime(2020, 01, 01);
                //DateTime toDate = DateTime.UtcNow.AddMinutes(330);
                ////DateTime toDate = new DateTime(2020, 06, 10);

                //var dbData = (from be in db.tbl_BusinessQuery
                //              where be.CRMLeadId != null
                //              && (be.CRMLeadSubStatusId == null || be.CRMLeadSubStatu.CRMLeadMainStatu.CRMLeadMainStatus == CRMLeadStatus.CRM_LEAD_MAIN_STATUS)
                //              //&& (be.CRMLeadSubStatusId == null)
                //              //&& be.CreatedDate >= fromDate && be.CreatedDate <= toDate
                //              select new LeadModel
                //              {
                //                  Id = be.CRMLeadId
                //              }
                //            ).ToList();

                LogException.Log("BergerLeadCRMSchedular.DB CRMLeadBusinessQuaries.UpdateCRMLeadStatusForBusinessQuaries :  Query Starts.");
                //List<int> crmSourceIds = new List<int>();
                //crmSourceIds.Add(70);
                //crmSourceIds.Add(93);
                //crmSourceIds.Add(96);
                //crmSourceIds.Add(19);
                //crmSourceIds.Add(21);
                //crmSourceIds.Add(22);
                //crmSourceIds.Add(23);
                //crmSourceIds.Add(24);
                //crmSourceIds.Add(31);
                //crmSourceIds.Add(34);
                //crmSourceIds.Add(35);
                //crmSourceIds.Add(36);
                //crmSourceIds.Add(37);
                //crmSourceIds.Add(38);
                //crmSourceIds.Add(42);
                //crmSourceIds.Add(43);
                //crmSourceIds.Add(44);
                //crmSourceIds.Add(45);
                //crmSourceIds.Add(46);
                //crmSourceIds.Add(47);
                //crmSourceIds.Add(48);
                //crmSourceIds.Add(49);
                //crmSourceIds.Add(50);
                //crmSourceIds.Add(52);
                //crmSourceIds.Add(53);
                //crmSourceIds.Add(57);
                //crmSourceIds.Add(65);
                //crmSourceIds.Add(66);
                //crmSourceIds.Add(68);
                //crmSourceIds.Add(69);
                //crmSourceIds.Add(70);
                //crmSourceIds.Add(71);
                //crmSourceIds.Add(73);
                //crmSourceIds.Add(74);
                //crmSourceIds.Add(75);
                //crmSourceIds.Add(76);
                //crmSourceIds.Add(77);
                //crmSourceIds.Add(78);
                //crmSourceIds.Add(79);
                //crmSourceIds.Add(82);
                //crmSourceIds.Add(83);
                //crmSourceIds.Add(84);
                //crmSourceIds.Add(85);
                //crmSourceIds.Add(86);
                //crmSourceIds.Add(87);
                //crmSourceIds.Add(88);
                //crmSourceIds.Add(89);

                var dbData = db.tbl_BusinessQuery                            
                            .Where(d => d.CRMLeadId != null
                            ////&& (d.CRMLeadSubStatusId == null || d.CRMLeadSubStatu.CRMLeadMainStatu.CRMLeadMainStatus == CRMLeadStatus.CRM_LEAD_MAIN_STATUS)
                            //&& d.CRMLeadSubStatusId == null
                            && d.CRMLeadSubStatusId != null && (d.CRMPincode == null || d.CRMPincode == "")
                            ////&& (d.CRMSourceId == 70 || d.CRMSourceId == 78)
                            ////&& crmSourceIds.Contains(d.CRMSourceId.Value)
                            && d.CreatedDate >= fromDate && d.CreatedDate <= toDate
                            )
                            .OrderBy(d => d.CreatedDate)
                            .ToList();

                if (dbData == null || dbData.Count == 0)
                {
                    LogException.Log("BergerLeadCRMSchedular.DB CRMLeadBusinessQuaries.UpdateCRMLeadStatusForBusinessQuaries :  Not data found in Database.");
                    return;
                }

                LogException.Log("BergerLeadCRMSchedular.DB CRMLeadBusinessQuaries.UpdateCRMLeadStatusForBusinessQuaries :  Query Executed. Data Count is " + dbData.Count + "");

                //var splitData = ListExtensions.splitList(dbData, maxRecords);
                //List<OutputModel> FinalOutputModel = new List<OutputModel>();

                foreach (var getlist in dbData)
                {

                    LogException.Log("getlist" + getlist.CRMLeadId + "");

                    LogException.Log("MobileNo" + getlist.Telephone + "");

                    InputModel model = new InputModel();
                    model.Origin = CRMLeadSource.WEBSITE;
                    model.LeadDetails = new List<LeadModel>();
                    model.LeadDetails.Add(new LeadModel()
                    {
                        Id = getlist.CRMLeadId,
                        MobileNo = getlist.Telephone
                    });
                    LogException.Log("ModelLeadDetails Count" + model.LeadDetails.Count + "");
                    try
                    {
                        LogException.Log("BE Lead Id : " + model.LeadDetails[0].Id);

                        var data = CRMBergerLeadImportApi.GetInstance().PostReturnList<InputModel, OutputModel>("lead/info", model);
                        LogException.Log(string.Format("data", data));

                        var leadDetail = model.LeadDetails[0];

                        var businessquery = db.tbl_BusinessQuery
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
                                LogException.Log(string.Format("{0},{1},{2},{3},{4}", "Exception in BergerLeadCRMSchedular.DB CRMLeadBusinessQuaries.UpdateCRMLeadStatusForBusinessQuaries : ", status, leadDetail.Id, ex.Message, ex.InnerException));
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        LogException.Log(string.Format("CRMLeadBusinessQuaries splitData exception : .{0}", ex.Message));
                    }

                }


            }
            catch (Exception ex)
            {
                LogException.Log(string.Format("{0},{1}", "Exception in BergerLeadCRMSchedular.DB CRMLeadBusinessQuaries.UpdateCRMLeadStatusForBusinessQuaries : ", ex.Message));
            }
        }

    }

}

