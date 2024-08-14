using educlient.Data;
using educlient.Models;
using Microsoft.AspNetCore.Mvc;
using System;
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
        // GET: NgayPhepChungController

        [HttpGet]
        public DsNgayPhepChungResult GetAll([FromQuery] DateTime? query_dateFrom = null, [FromQuery] DateTime? query_dateTo = null)
        {
            var tb = database.Table<DsNgayPhepChungDO>();

            DsNgayPhepChungDO[] resultData;

            if (query_dateFrom.HasValue && query_dateTo.HasValue)
            {
                resultData = tb.Find(x =>
                (x.dateFrom >= query_dateFrom.Value && x.dateTo <= query_dateTo.Value) ||
                (x.dateFrom <= query_dateFrom.Value && x.dateTo >= query_dateTo.Value) ||
                (x.dateFrom <= query_dateTo.Value && x.dateTo >= query_dateFrom.Value) ||
                (x.dateTo >= query_dateFrom.Value && x.dateFrom <= query_dateTo.Value)
                ).ToArray();
            }
            else if (query_dateFrom.HasValue)
            {
                // Only dateFrom is provided
                resultData = tb.Find(x => x.dateFrom >= query_dateFrom.Value).ToArray();
            }
            else if (query_dateFrom.HasValue)
            {
                // Only dateTo is provided
                resultData = tb.Find(x => x.dateTo <= query_dateTo.Value).ToArray();
            }
            else
            {
                // No date filters provided
                resultData = tb.FindAll().ToArray();
            }

            return new DsNgayPhepChungResult
            {
                data = resultData,
            };
        }


        [HttpGet, Route("{id}")]
        public DsNgayPhepChungResult GetById(Guid id)
        {
            DsNgayPhepChungResult returnData = new DsNgayPhepChungResult();
            var tb = database.Table<DsNgayPhepChungDO>();
            returnData.data.Append(tb.FindById(id));
            return new DsNgayPhepChungResult
            {
                data = returnData.data,
            };
        }
        // GET: NgayPhepChungController
        [HttpPost, Route("Insert")]
        public DsNgayPhepChungResult insert([FromBody] DsNgayPhepChungInput[] inputData)
        {
            var insertData = inputData.Select(input => new DsNgayPhepChungDO
            {
                dateFrom = input.dateFrom,
                dateTo = input.dateTo,
                sumDay = input.sumDay,
                reason = input.reason,
                note = input.note,
            }).ToList();
            var tb = database.Table<DsNgayPhepChungDO>();
            tb.Insert(insertData);
            var data = tb.FindAll();
            return new DsNgayPhepChungResult
            {
                data = tb.FindAll().ToArray(),
            };
        }
        [HttpPut, Route("{id}")]
        public DsNgayPhepChungResult update(Guid id, [FromBody] DsNgayPhepChungInput inputData)
        {
            var tb = database.Table<DsNgayPhepChungDO>();

            var existingRecord = tb.FindById(id);
            if (existingRecord == null)
            {
                new DsNgayPhepChungResult
                {
                    code = 400,
                    message = "data not found",
                }; // Return 404 if the record does not exist
            }

            // Update the existing record with new values
            existingRecord.dateFrom = inputData.dateFrom;
            existingRecord.dateTo = inputData.dateTo;
            existingRecord.sumDay = inputData.sumDay;
            existingRecord.reason = inputData.reason;
            existingRecord.note = inputData.note;


            // Update the record in the collection
            tb.Update(existingRecord);
            var data = tb.FindAll();
            return new DsNgayPhepChungResult
            {
                data = tb.FindAll().ToArray(),
            };
        }
        [HttpDelete, Route("{id}")]
        public DsNgayPhepChungResult delete(Guid id)
        {
            var tb = database.Table<DsNgayPhepChungDO>();

            var existingRecord = tb.FindById(id);
            if (existingRecord == null)
            {
                new DsNgayPhepChungResult
                {
                    code = 400,
                    message = "data not found",
                }; // Return 404 if the record does not exist
            }
            tb.Delete(id);
            var data = tb.FindAll();
            return new DsNgayPhepChungResult
            {
                data = tb.FindAll().ToArray(),
            };
        }
    }
}
