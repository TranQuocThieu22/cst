using educlient.Data;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace educlient.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LamViecNgoaiGio : Controller
    {
        private readonly IDbLiteContext database;
        public LamViecNgoaiGio(IDbLiteContext dataContext)
        {
            database = dataContext;
        }
        [HttpGet]
        public WorkingOTResult GetAll([FromQuery] DateTime? query_dateFrom = null, [FromQuery] DateTime? query_dateTo = null, [FromQuery] int? query_memberId = null)
        {
            var OverTimeTable = database.Table<WorkingOTDataDO>();

            List<WorkingOTDataDO> resultData;

            if (query_memberId.HasValue)
            {
                // Query based on userId
                if (query_dateFrom.HasValue && query_dateTo.HasValue)
                {
                    resultData = OverTimeTable.Find(x =>
                        x.memberId == query_memberId.Value &&
                        ((x.date >= query_dateFrom.Value && x.date <= query_dateTo.Value))
                    //(x.dateFrom <= query_dateFrom.Value && x.dateTo >= query_dateTo.Value) ||
                    //(x.dateFrom <= query_dateTo.Value && x.dateTo >= query_dateFrom.Value) ||
                    //(x.dateTo >= query_dateFrom.Value && x.dateFrom <= query_dateTo.Value))
                    ).ToList();
                }
                else if (query_dateFrom.HasValue)
                {
                    // Only dateFrom is provided
                    resultData = OverTimeTable.Find(x =>
                        x.memberId == query_memberId.Value &&
                        x.date >= query_dateFrom.Value
                    ).ToList();
                }
                else if (query_dateTo.HasValue)
                {
                    // Only dateTo is provided
                    resultData = OverTimeTable.Find(x =>
                        x.memberId == query_memberId.Value &&
                        x.date <= query_dateTo.Value
                    ).ToList();
                }
                else
                {
                    // Only userId filter provided
                    resultData = OverTimeTable.Find(x => x.memberId == query_memberId.Value).ToList();
                }
            }
            else
            {
                // Query all (no userId filter)
                if (query_dateFrom.HasValue && query_dateTo.HasValue)
                {
                    resultData = OverTimeTable.Find(x =>
                        (x.date >= query_dateFrom.Value && x.date <= query_dateTo.Value)
                    //(x.dateFrom <= query_dateFrom.Value && x.dateTo >= query_dateTo.Value) ||
                    //(x.dateFrom <= query_dateTo.Value && x.dateTo >= query_dateFrom.Value) ||
                    //(x.dateTo >= query_dateFrom.Value && x.dateFrom <= query_dateTo.Value)
                    ).ToList();
                }
                else if (query_dateFrom.HasValue)
                {
                    // Only dateFrom is provided
                    resultData = OverTimeTable.Find(x => x.date >= query_dateFrom.Value).ToList();
                }
                else if (query_dateTo.HasValue)
                {
                    // Only dateTo is provided
                    resultData = OverTimeTable.Find(x => x.date <= query_dateTo.Value).ToList();
                }
                else
                {
                    // No filters provided
                    resultData = OverTimeTable.FindAll().ToList();
                }
            }

            return new WorkingOTResult
            {
                message = "Success",
                code = 200,
                result = true,
                data = resultData
            };
        }

        [HttpGet, Route("{id}")]
        public WorkingOTResult GetById(int id)
        {
            List<WorkingOTDataDO> returnData = new List<WorkingOTDataDO>();

            var workingOTTable = database.Table<WorkingOTDataDO>();
            var workingOT = workingOTTable.FindById(id);

            if (workingOT == null)
            {
                return new WorkingOTResult
                {
                    code = 404,
                    message = "Data not found"
                };
            }
            returnData.Add(workingOT);

            return new WorkingOTResult
            {
                message = "Success",
                code = 200,
                result = true,
                data = returnData,
            };
        }

        [HttpPost]
        public ApiResultBaseDO Insert([FromBody] WorkingOTInput[] inputData)
        {
            var insertData = inputData.Select(input => new WorkingOTDataDO
            {
                date = input.date,
                time = input.time,
                memberId = input.memberId,
                note = input.note
            }).ToList();

            var workingOTTable = database.Table<WorkingOTDataDO>();
            workingOTTable.Insert(insertData);

            return new ApiResultBaseDO
            {
                message = "Insert Success",
                code = 200,
                result = true
            };
        }

        [HttpPut, Route("{id}")]
        public ApiResultBaseDO Update(int id, [FromBody] WorkingOTInput inputData)
        {
            var workingOTTable = database.Table<WorkingOTDataDO>();

            var existingRecord = workingOTTable.FindById(id);
            if (existingRecord == null)
            {
                new ApiResultBaseDO
                {
                    code = 400,
                    message = "data not found",
                };
            }

            // Update the existing record with new values
            existingRecord.date = inputData.date;
            existingRecord.memberId = inputData.memberId;
            existingRecord.time = inputData.time;
            existingRecord.note = inputData.note;

            // Update the record in the collection
            workingOTTable.Update(existingRecord);

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
            var workingOTTable = database.Table<WorkingOTDataDO>();
            var existingRecord = workingOTTable.FindById(id);

            if (existingRecord == null)
            {
                return new ApiResultBaseDO
                {
                    code = 404,
                    message = "Data not found"
                };
            }

            workingOTTable.Delete(id);

            return new ApiResultBaseDO
            {
                message = "Delete Success",
                code = 200,
                result = true
            };
        }
        [HttpGet("ThongKeTienLamViecNgoaiGio")]
        public ThongKeTienLamViecNgoaiGioResult GetThongKeTienLamViecNgoaiGio([FromQuery] DateTime? query_dateFrom = null, [FromQuery] DateTime? query_dateTo = null, int? year = null)
        {
            var membersTable = database.Table<AQMember>();
            var membersData = membersTable.FindAll().ToList();
            var OverTimeTable = database.Table<WorkingOTDataDO>();
            if (membersData == null)
            {
                return new ThongKeTienLamViecNgoaiGioResult
                {
                    message = "Member not found",
                    code = 404,
                    result = false,
                    data = null
                };
            }
            var resultList = new List<ThongKeTienLamViecNgoaiGioDataDO>();
            var tmpData = new List<WorkingOTDataDO>();
            foreach (var member in membersData)
            {
                if (query_dateFrom.HasValue && query_dateTo.HasValue)
                {
                    tmpData = OverTimeTable.Find(x =>
                        x.memberId == member.id &&
                        ((x.date >= query_dateFrom.Value && x.date <= query_dateTo.Value))

                    ).ToList();
                }
                else if (query_dateFrom.HasValue)
                {
                    // Only dateFrom is provided
                    tmpData = OverTimeTable.Find(x =>
                         x.memberId == member.id &&
                        x.date >= query_dateFrom.Value
                    ).ToList();
                }
                else if (query_dateTo.HasValue)
                {
                    // Only dateTo is provided
                    tmpData = OverTimeTable.Find(x =>
                        x.memberId == member.id &&
                        x.date <= query_dateTo.Value
                    ).ToList();
                }
                else
                {
                    // Only userId filter provided
                    tmpData = OverTimeTable.Find(x => x.memberId == member.id).ToList();
                }
                var sumTime = tmpData.Sum(x => x.time);
                Debug.WriteLine(tmpData);
                var resultData = new ThongKeTienLamViecNgoaiGioDataDO
                {
                    nickName = member.nickName,
                    fullName = member.fullName,
                    SumHours = sumTime
                };
                resultList.Add(resultData);

            }
            return new ThongKeTienLamViecNgoaiGioResult
            {
                message = "Success",
                code = 200,
                result = true,
                data = resultList
            };
        }
        public class WorkingOTResult : ApiResultBaseDO
        {
            public List<WorkingOTDataDO> data { get; set; }
        }

        public class WorkingOTInput
        {
            public int id { get; set; }
            public DateTime date { get; set; }
            public float time { get; set; }
            public int memberId { get; set; }
            public string note { get; set; }
        }
        public class ThongKeTienLamViecNgoaiGioResult : ApiResultBaseDO
        {
            public List<ThongKeTienLamViecNgoaiGioDataDO> data { get; set; }


        }
        public class ThongKeTienLamViecNgoaiGioDataDO
        {
            public string nickName { get; set; }
            public string fullName { get; set; }
            public float SumHours { get; set; }

        }
    }
}
