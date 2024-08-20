using educlient.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System;

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
            var WorkingOnlineTable = database.Table<WorkingOnlineDataDO>();

            List<WorkingOnlineDataDO> resultData;

            if (query_memberId.HasValue)
            {
                // Query based on userId
                if (query_dateFrom.HasValue && query_dateTo.HasValue)
                {
                    resultData = WorkingOnlineTable.Find(x =>
                        x.memberId == query_memberId.Value &&
                        ((x.dateFrom >= query_dateFrom.Value && x.dateTo <= query_dateTo.Value) ||
                        (x.dateFrom <= query_dateFrom.Value && x.dateTo >= query_dateTo.Value) ||
                        (x.dateFrom <= query_dateTo.Value && x.dateTo >= query_dateFrom.Value) ||
                        (x.dateTo >= query_dateFrom.Value && x.dateFrom <= query_dateTo.Value))
                    ).ToList();
                }
                else if (query_dateFrom.HasValue)
                {
                    // Only dateFrom is provided
                    resultData = WorkingOnlineTable.Find(x =>
                        x.memberId == query_memberId.Value &&
                        x.dateFrom >= query_dateFrom.Value
                    ).ToList();
                }
                else if (query_dateTo.HasValue)
                {
                    // Only dateTo is provided
                    resultData = WorkingOnlineTable.Find(x =>
                        x.memberId == query_memberId.Value &&
                        x.dateTo <= query_dateTo.Value
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
                        (x.dateFrom >= query_dateFrom.Value && x.dateTo <= query_dateTo.Value) ||
                        (x.dateFrom <= query_dateFrom.Value && x.dateTo >= query_dateTo.Value) ||
                        (x.dateFrom <= query_dateTo.Value && x.dateTo >= query_dateFrom.Value) ||
                        (x.dateTo >= query_dateFrom.Value && x.dateFrom <= query_dateTo.Value)
                    ).ToList();
                }
                else if (query_dateFrom.HasValue)
                {
                    // Only dateFrom is provided
                    resultData = WorkingOnlineTable.Find(x => x.dateFrom >= query_dateFrom.Value).ToList();
                }
                else if (query_dateTo.HasValue)
                {
                    // Only dateTo is provided
                    resultData = WorkingOnlineTable.Find(x => x.dateTo <= query_dateTo.Value).ToList();
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
            List<WorkingOnlineDataDO> returnData = new List<WorkingOnlineDataDO>();

            var WorkingOnlineTable = database.Table<WorkingOnlineDataDO>();

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
            var insertData = inputData.Select(input => new WorkingOnlineDataDO
            {
                dateFrom = input.dateFrom,
                dateTo = input.dateTo,
                sumDay = input.sumDay,
                memberId = input.memberId,
                reason = input.reason,
                approvalStatus = input.approvalStatus,
                note = input.note,
            }).ToList();

            var WorkingOnlineTable = database.Table<WorkingOnlineDataDO>();
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
            var WorkingOnlineTable = database.Table<WorkingOnlineDataDO>();

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
            existingRecord.dateFrom = inputData.dateFrom;
            existingRecord.dateTo = inputData.dateTo;
            existingRecord.sumDay = inputData.sumDay;
            existingRecord.memberId = inputData.memberId;
            existingRecord.reason = inputData.reason;
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
            var WorkingOnlineTable = database.Table<WorkingOnlineDataDO>();

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
            var WorkingOnlineTable = database.Table<WorkingOnlineDataDO>();

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
    }

    public class WorkingOnlineResult : ApiResultBaseDO
    {
        public List<WorkingOnlineDataDO> data { get; set; }
    }

    public class WorkingOnlineInput
    {
        public int id { get; set; }
        public DateTime dateFrom { get; set; }
        public DateTime dateTo { get; set; }
        public float sumDay { get; set; }
        public int memberId { get; set; }
        public string reason { get; set; }
        public string approvalStatus { get; set; }
        public string note { get; set; }
    }

}