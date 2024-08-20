using educlient.Data;
using educlient.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace educlient.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NgayPhepChungController : ControllerBase
    {
        private readonly IDbLiteContext database;
        public NgayPhepChungController(IDbLiteContext dataContext)
        {
            database = dataContext;
        }

        [HttpGet]
        public DayOffsResult GetAll([FromQuery] DateTime? query_dateFrom = null, [FromQuery] DateTime? query_dateTo = null)
        {
            var DayOffTable = database.Table<DayOff>();

            List<DayOff> resultData;

            if (query_dateFrom.HasValue && query_dateTo.HasValue)
            {
                resultData = DayOffTable.Find(x =>
                (x.dateFrom >= query_dateFrom.Value && x.dateTo <= query_dateTo.Value) ||
                (x.dateFrom <= query_dateFrom.Value && x.dateTo >= query_dateTo.Value) ||
                (x.dateFrom <= query_dateTo.Value && x.dateTo >= query_dateFrom.Value) ||
                (x.dateTo >= query_dateFrom.Value && x.dateFrom <= query_dateTo.Value)
                ).ToList();
            }
            else if (query_dateFrom.HasValue)
            {
                // Only dateFrom is provided
                resultData = DayOffTable.Find(x => x.dateFrom >= query_dateFrom.Value).ToList();
            }
            else if (query_dateFrom.HasValue)
            {
                // Only dateTo is provided
                resultData = DayOffTable.Find(x => x.dateTo <= query_dateTo.Value).ToList();
            }
            else
            {
                // No date filters provided
                resultData = DayOffTable.FindAll().ToList();
            }

            return new DayOffsResult
            {
                message = "Success",
                code = 200,
                result = true,
                data = resultData
            };
        }


        [HttpGet, Route("{id}")]
        public DayOffsResult GetById(int id)
        {
            List<DayOff> returnData = new List<DayOff>();

            var DayOffTable = database.Table<DayOff>();

            var dayOff = DayOffTable.FindById(id);
            if (dayOff == null)
            {
                return new DayOffsResult
                {
                    code = 404,
                    message = "Data not found"
                };
            }
            returnData.Add(dayOff);

            return new DayOffsResult
            {
                message = "Success",
                code = 200,
                result = true,
                data = returnData,
            };
        }


        [HttpPost]
        public ApiResultBaseDO Insert([FromBody] DayOffInput[] inputData)
        {
            var insertData = inputData.Select(input => new DayOff
            {
                dateFrom = input.dateFrom,
                dateTo = input.dateTo,
                sumDay = input.sumDay,
                reason = input.reason,
                note = input.note,
            }).ToList();

            var DayOffTable = database.Table<DayOff>();
            DayOffTable.Insert(insertData);

            return new ApiResultBaseDO
            {
                message = "Insert Success",
                code = 200,
                result = true
            };
        }

        [HttpPut, Route("{id}")]
        public ApiResultBaseDO Update(int id, [FromBody] DayOffInput inputData)
        {
            var DayOffTable = database.Table<DayOff>();

            var existingRecord = DayOffTable.FindById(id);
            if (existingRecord == null)
            {
                new DayOffsResult
                {
                    code = 400,
                    message = "data not found",
                };
            }

            // Update the existing record with new values
            existingRecord.dateFrom = inputData.dateFrom;
            existingRecord.dateTo = inputData.dateTo;
            existingRecord.sumDay = inputData.sumDay;
            existingRecord.reason = inputData.reason;
            existingRecord.note = inputData.note;

            // Update the record in the collection
            DayOffTable.Update(existingRecord);

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
            var DayOffTable = database.Table<DayOff>();

            var existingRecord = DayOffTable.FindById(id);
            if (existingRecord == null)
            {
                new DayOffsResult
                {
                    code = 400,
                    message = "data not found",
                };
            }
            DayOffTable.Delete(id);

            return new ApiResultBaseDO
            {
                message = "Delete Success",
                code = 200,
                result = true
            };
        }
    }

    public class DayOffsResult : ApiResultBaseDO
    {
        public List<DayOff> data { get; set; }
    }

    public class DayOffInput
    {
        public int id { get; set; }
        public DateTime dateFrom { get; set; }
        public DateTime dateTo { get; set; }
        public float sumDay { get; set; }
        public string reason { get; set; }
        public string note { get; set; }
    }
}
