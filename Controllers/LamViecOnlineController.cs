using educlient.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System;
using static educlient.Controllers.NgayPhepCaNhanController;

namespace educlient.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LamViecOnlineController : ControllerBase
    {
        private readonly IDbLiteContext database;
        public LamViecOnlineController(IDbLiteContext dataContext)
        {
            database = dataContext;
        }

        [HttpGet]
        public WorkingOnlineResult GetAll([FromQuery] DateTime? query_dateFrom = null, [FromQuery] DateTime? query_dateTo = null, [FromQuery] int? query_memberId = null)
        {
            var WorkingOnlineTable = database.Table<WorkingOnlineDay>();

            List<WorkingOnlineDay> resultData;

            if (query_memberId.HasValue)
            {
                // Query based on userId
                if (query_dateFrom.HasValue && query_dateTo.HasValue)
                {
                    resultData = WorkingOnlineTable.Find(x =>
                        x.memberId == query_memberId.Value &&
                        (x.date >= query_dateFrom.Value && x.date <= query_dateTo.Value)
                    //((x.dateFrom >= query_dateFrom.Value && x.dateTo <= query_dateTo.Value) ||
                    //(x.dateFrom <= query_dateFrom.Value && x.dateTo >= query_dateTo.Value) ||
                    //(x.dateFrom <= query_dateTo.Value && x.dateTo >= query_dateFrom.Value) ||
                    //(x.dateTo >= query_dateFrom.Value && x.dateFrom <= query_dateTo.Value))
                    ).ToList();
                }
                else if (query_dateFrom.HasValue)
                {
                    // Only dateFrom is provided
                    resultData = WorkingOnlineTable.Find(x =>
                        x.memberId == query_memberId.Value &&
                        x.date >= query_dateFrom.Value
                    ).ToList();
                }
                else if (query_dateTo.HasValue)
                {
                    // Only dateTo is provided
                    resultData = WorkingOnlineTable.Find(x =>
                        x.memberId == query_memberId.Value &&
                        x.date <= query_dateTo.Value
                    ).ToList();
                }
                else
                {
                    // Only userId filter provided
                    resultData = WorkingOnlineTable.Find(x => x.memberId == query_memberId.Value).ToList();
                }
            }
            else
            {
                // Query all (no userId filter)
                if (query_dateFrom.HasValue && query_dateTo.HasValue)
                {
                    resultData = WorkingOnlineTable.Find(x =>
                    (x.date >= query_dateFrom.Value && x.date <= query_dateTo.Value)
                    //(x.dateFrom >= query_dateFrom.Value && x.dateTo <= query_dateTo.Value) ||
                    //(x.dateFrom <= query_dateFrom.Value && x.dateTo >= query_dateTo.Value) ||
                    //(x.dateFrom <= query_dateTo.Value && x.dateTo >= query_dateFrom.Value) ||
                    //(x.dateTo >= query_dateFrom.Value && x.dateFrom <= query_dateTo.Value)
                    ).ToList();
                }
                else if (query_dateFrom.HasValue)
                {
                    // Only dateFrom is provided
                    resultData = WorkingOnlineTable.Find(x => x.date >= query_dateFrom.Value).ToList();
                }
                else if (query_dateTo.HasValue)
                {
                    // Only dateTo is provided
                    resultData = WorkingOnlineTable.Find(x => x.date <= query_dateTo.Value).ToList();
                }
                else
                {
                    // No filters provided
                    resultData = WorkingOnlineTable.FindAll().ToList();
                }
            }

            return new WorkingOnlineResult
            {
                message = "Success",
                code = 200,
                result = true,
                data = resultData
            };
        }


        [HttpGet, Route("{id}")]
        public WorkingOnlineResult GetById(int id)
        {
            List<WorkingOnlineDay> returnData = new List<WorkingOnlineDay>();

            var WorkingOnlineTable = database.Table<WorkingOnlineDay>();

            var resultData = WorkingOnlineTable.FindById(id);
            if (resultData == null)
            {
                return new WorkingOnlineResult
                {
                    code = 404,
                    message = "Data not found",
                    data = returnData
                };
            }
            returnData.Add(resultData);

            return new WorkingOnlineResult
            {
                code = 200,
                result = true,
                data = returnData,
                message = "Success"
            };
        }


        [HttpPost]
        public ApiResultBaseDO Insert([FromBody] WorkingOnlineInput[] inputData)
        {
            var insertData = inputData.Select(input => new WorkingOnlineDay
            {
                date = input.date,
                memberId = input.memberId,
                reason = input.reason,
                periodType = input.periodType,
                wfhType = input.wfhType,
                approvalStatus = input.approvalStatus,
                note = input.note,
            }).ToList();

            var WorkingOnlineTable = database.Table<WorkingOnlineDay>();
            WorkingOnlineTable.Insert(insertData);

            return new ApiResultBaseDO
            {
                message = "Insert Success",
                code = 200,
                result = true
            };
        }

        [HttpPut, Route("{id}")]
        public ApiResultBaseDO Update(int id, [FromBody] WorkingOnlineInput inputData)
        {
            var WorkingOnlineTable = database.Table<WorkingOnlineDay>();

            var existingRecord = WorkingOnlineTable.FindById(id);
            if (existingRecord == null)
            {
                new WorkingOnlineResult
                {
                    code = 400,
                    message = "data not found",
                };
            }

            // Update the existing record with new values
            existingRecord.date = inputData.date;
            existingRecord.memberId = inputData.memberId;
            existingRecord.reason = inputData.reason;
            existingRecord.periodType = inputData.periodType;
            existingRecord.wfhType = inputData.wfhType;
            existingRecord.approvalStatus = inputData.approvalStatus;
            existingRecord.note = inputData.note;

            // Update the record in the collection
            WorkingOnlineTable.Update(existingRecord);

            return new ApiResultBaseDO
            {
                message = "Update Success",
                code = 200,
                result = true
            };
        }

        [HttpDelete, Route("{id}")]
        public ApiResultBaseDO Delete(int id)
        {
            var WorkingOnlineTable = database.Table<WorkingOnlineDay>();

            var existingRecord = WorkingOnlineTable.FindById(id);
            if (existingRecord == null)
            {
                new WorkingOnlineResult
                {
                    code = 400,
                    message = "data not found",
                };
            }
            WorkingOnlineTable.Delete(id);
            var data = WorkingOnlineTable.FindAll();

            return new ApiResultBaseDO
            {
                message = "Delete Success",
                code = 200,
                result = true
            };
        }

        [HttpPut, Route("DuyetLamViecOnline")]
        public ApiResultBaseDO ApproveWorkingOnline([FromBody] ApprovalInput inputData)
        {
            var WorkingOnlineTable = database.Table<WorkingOnlineDay>();

            var existingRecord = WorkingOnlineTable.FindById(inputData.id);
            if (existingRecord == null)
            {
                new WorkingOnlineResult
                {
                    code = 400,
                    message = "data not found",
                };
            }

            // Update the existing record with new values
            existingRecord.approvalStatus = inputData.approvalStatus;

            // Update the record in the collection
            WorkingOnlineTable.Update(existingRecord);

            return new ApiResultBaseDO
            {
                message = "Approve Success",
                code = 200,
                result = true
            };
        }

        [HttpGet("HanMucLamViecOnlineCaNhan")]
        public HanMucLamViecOnlineCaNhanResult GetHanMucLamViecOnlineCaNhan([FromQuery] int year, [FromQuery] int? query_memberId = null)
        {
            var resultList = new List<HanMucLamViecOnlineCaNhan>();

            var AQMemberTable = database.Table<AQMember>();
            var workingOnlineTable = database.Table<WorkingOnlineDay>();

            int minWfhQuota = AQMemberTable.FindById(query_memberId).minWFHQuota;
            int additionalWfhQuota = AQMemberTable.FindById(query_memberId).additionalWFHQuota;
            float totalWorkingOnlineDay = 0;
            float totalWorkingOnlineDay_with_permission = 0;
            float totalWorkingOnlineDay_without_permission = 0;
            float totalWorkingOnlineDay_fullType1 = 0;
            float totalWorkingOnlineDay_fullType2 = 0;
            float totalWorkingOnlineDay_halfType1 = 0;
            float totalWorkingOnlineDay_halfType2 = 0;
            float totalWorkingOnlineDay_type3_4 = 0;
            float usedMinWfhQuota = 0;
            float usedAdditionalWfhQuota = 0;
            float remainMinWfhQuota = 0;
            float remainAdditionalWfhQuota = 0;

            var workingOnlineDayData = workingOnlineTable.Find(x =>
                    x.memberId == query_memberId &&
                    x.date.Year == year &&
                    x.approvalStatus == "Đã duyệt"
                    ).ToList();

            totalWorkingOnlineDay = workingOnlineDayData.Count;

            foreach (var workingOnlineDay in workingOnlineDayData)
            {
                totalWorkingOnlineDay_with_permission += (workingOnlineDay.wfhType == 3 || workingOnlineDay.wfhType == 4) ? 1 : 0;
                totalWorkingOnlineDay_fullType1 += (workingOnlineDay.periodType == 1 && workingOnlineDay.wfhType == 1) ? 1 : 0;
                totalWorkingOnlineDay_fullType2 += (workingOnlineDay.periodType == 1 && workingOnlineDay.wfhType == 2) ? 1 : 0;
                totalWorkingOnlineDay_halfType1 += ((workingOnlineDay.periodType == 2 || workingOnlineDay.periodType == 3) && workingOnlineDay.wfhType == 1) ? 1 : 0;
                totalWorkingOnlineDay_halfType2 += ((workingOnlineDay.periodType == 2 || workingOnlineDay.periodType == 3) && workingOnlineDay.wfhType == 2) ? 1 : 0;
                totalWorkingOnlineDay_type3_4 += (workingOnlineDay.wfhType == 3 || workingOnlineDay.wfhType == 4) ? 1 : 0;
            }

            usedMinWfhQuota = (float)(totalWorkingOnlineDay_fullType1 + totalWorkingOnlineDay_halfType1 * 0.5);
            usedAdditionalWfhQuota = (float)(totalWorkingOnlineDay_fullType2 + totalWorkingOnlineDay_halfType2 * 0.5);
            remainMinWfhQuota = minWfhQuota - usedMinWfhQuota;
            remainAdditionalWfhQuota = additionalWfhQuota - usedAdditionalWfhQuota;

            totalWorkingOnlineDay_without_permission = totalWorkingOnlineDay - totalWorkingOnlineDay_with_permission;

            var HanMucLamViecOnline = new HanMucLamViecOnlineCaNhan
            {
                year = year,
                memberId = query_memberId.Value,
                minWfhQuota = minWfhQuota,
                additionalWfhQuota = additionalWfhQuota,
                totalWorkOnlineDay = totalWorkingOnlineDay,
                totalWorkOnlineDayFullType1 = totalWorkingOnlineDay_fullType1,
                totalWorkOnlineDayFullType2 = totalWorkingOnlineDay_fullType2,
                totalWorkOnlineDayHalfType1 = totalWorkingOnlineDay_halfType1,
                totalWorkOnlineDayHalfType2 = totalWorkingOnlineDay_halfType2,
                totalWorkOnlineDayType3_4 = totalWorkingOnlineDay_type3_4,
                usedAdditionalWfhQuota = usedAdditionalWfhQuota,
                usedMinWfhQuota = usedMinWfhQuota,
                remainAdditionalWfhQuota = remainAdditionalWfhQuota,
                remainMinWfhQuota = remainMinWfhQuota,
            };

            resultList.Add(HanMucLamViecOnline);

            return new HanMucLamViecOnlineCaNhanResult
            {
                message = "Success",
                code = 200,
                result = true,
                data = resultList
            };
        }

    }

    public class WorkingOnlineResult : ApiResultBaseDO
    {
        public List<WorkingOnlineDay> data { get; set; }
    }

    public class WorkingOnlineInput
    {
        public int id { get; set; }
        public DateTime date { get; set; }
        public int memberId { get; set; }
        public int periodType { get; set; }
        public int wfhType { get; set; }
        public string reason { get; set; }
        public string approvalStatus { get; set; }
        public string note { get; set; }
    }

    public class HanMucLamViecOnlineCaNhanResult : ApiResultBaseDO
    {
        public List<HanMucLamViecOnlineCaNhan> data { get; set; }
    }

    public class HanMucLamViecOnlineCaNhan
    {
        public int memberId { get; set; }
        public int year { get; set; }
        public int minWfhQuota { get; set; }
        public int additionalWfhQuota { get; set; }
        public float totalWorkOnlineDay { get; set; }
        public float totalWorkOnlineDayFullType1 { get; set; }
        public float totalWorkOnlineDayFullType2 { get; set; }

        public float totalWorkOnlineDayHalfType1 { get; set; }
        public float totalWorkOnlineDayHalfType2 { get; set; }
        public float totalWorkOnlineDayType3_4 { get; set; }
        public float usedMinWfhQuota { get; set; }
        public float usedAdditionalWfhQuota { get; set; }
        public float remainMinWfhQuota { get; set; }
        public float remainAdditionalWfhQuota { get; set; }
    }

}