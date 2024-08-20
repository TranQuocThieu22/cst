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
    public class NgayPhepCaNhanController : ControllerBase
    {
        private readonly IDbLiteContext database;
        public NgayPhepCaNhanController(IDbLiteContext dataContext)
        {
            database = dataContext;
        }

        [HttpGet]
        public IndividualDayOffResult GetAll([FromQuery] DateTime? query_dateFrom = null, [FromQuery] DateTime? query_dateTo = null, [FromQuery] int? query_memberId = null)
        {
            var IndividualDayOffTable = database.Table<IndividualDayOff>();

            List<IndividualDayOff> resultData;

            //if (query_dateFrom.HasValue && query_dateTo.HasValue)
            //{
            //    resultData = IndividualDayOffTable.Find(x =>
            //    (x.dateFrom >= query_dateFrom.Value && x.dateTo <= query_dateTo.Value) ||
            //    (x.dateFrom <= query_dateFrom.Value && x.dateTo >= query_dateTo.Value) ||
            //    (x.dateFrom <= query_dateTo.Value && x.dateTo >= query_dateFrom.Value) ||
            //    (x.dateTo >= query_dateFrom.Value && x.dateFrom <= query_dateTo.Value)
            //    ).ToList();
            //}
            //else if (query_dateFrom.HasValue)
            //{
            //    // Only dateFrom is provided
            //    resultData = IndividualDayOffTable.Find(x => x.dateFrom >= query_dateFrom.Value).ToList();
            //}
            //else if (query_dateFrom.HasValue)
            //{
            //    // Only dateTo is provided
            //    resultData = IndividualDayOffTable.Find(x => x.dateTo <= query_dateTo.Value).ToList();
            //}
            //else
            //{
            //    // No date filters provided
            //    resultData = IndividualDayOffTable.FindAll().ToList();
            //}

            if (query_memberId.HasValue)
            {
                // Query based on userId
                if (query_dateFrom.HasValue && query_dateTo.HasValue)
                {
                    resultData = IndividualDayOffTable.Find(x =>
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
                    resultData = IndividualDayOffTable.Find(x =>
                        x.memberId == query_memberId.Value &&
                        x.dateFrom >= query_dateFrom.Value
                    ).ToList();
                }
                else if (query_dateTo.HasValue)
                {
                    // Only dateTo is provided
                    resultData = IndividualDayOffTable.Find(x =>
                        x.memberId == query_memberId.Value &&
                        x.dateTo <= query_dateTo.Value
                    ).ToList();
                }
                else
                {
                    // Only userId filter provided
                    resultData = IndividualDayOffTable.Find(x => x.memberId == query_memberId.Value).ToList();
                }
            }
            else
            {
                // Query all (no userId filter)
                if (query_dateFrom.HasValue && query_dateTo.HasValue)
                {
                    resultData = IndividualDayOffTable.Find(x =>
                        (x.dateFrom >= query_dateFrom.Value && x.dateTo <= query_dateTo.Value) ||
                        (x.dateFrom <= query_dateFrom.Value && x.dateTo >= query_dateTo.Value) ||
                        (x.dateFrom <= query_dateTo.Value && x.dateTo >= query_dateFrom.Value) ||
                        (x.dateTo >= query_dateFrom.Value && x.dateFrom <= query_dateTo.Value)
                    ).ToList();
                }
                else if (query_dateFrom.HasValue)
                {
                    // Only dateFrom is provided
                    resultData = IndividualDayOffTable.Find(x => x.dateFrom >= query_dateFrom.Value).ToList();
                }
                else if (query_dateTo.HasValue)
                {
                    // Only dateTo is provided
                    resultData = IndividualDayOffTable.Find(x => x.dateTo <= query_dateTo.Value).ToList();
                }
                else
                {
                    // No filters provided
                    resultData = IndividualDayOffTable.FindAll().ToList();
                }
            }

            return new IndividualDayOffResult
            {
                data = resultData
            };
        }


        [HttpGet, Route("{id}")]
        public IndividualDayOffResult GetById(int id)
        {
            List<IndividualDayOff> returnData = new List<IndividualDayOff>();

            var IndividualDayOffTable = database.Table<IndividualDayOff>();

            var resultData = IndividualDayOffTable.FindById(id);
            if (resultData == null)
            {
                return new IndividualDayOffResult
                {
                    code = 404,
                    message = "Data not found"
                };
            }
            returnData.Add(resultData);

            return new IndividualDayOffResult
            {
                code = 200,
                result = true,
                data = returnData,
            };
        }


        [HttpPost]
        public IndividualDayOffResult Insert([FromBody] IndividualDayOffInput[] inputData)
        {
            var insertData = inputData.Select(input => new IndividualDayOff
            {
                dateFrom = input.dateFrom,
                dateTo = input.dateTo,
                sumDay = input.sumDay,
                memberId = input.memberId,
                reason = input.reason,
                isAnnual = input.isAnnual,
                isWithoutPay = input.isWithoutPay,
                approvalStatus = input.approvalStatus,
                note = input.note,
            }).ToList();

            var IndividualDayOffTable = database.Table<IndividualDayOff>();
            IndividualDayOffTable.Insert(insertData);
            var resultData = IndividualDayOffTable.FindAll();

            return new IndividualDayOffResult
            {
                data = resultData.ToList(),
            };
        }

        [HttpPut, Route("{id}")]
        public IndividualDayOffResult Update(int id, [FromBody] IndividualDayOffInput inputData)
        {
            var IndividualDayOffTable = database.Table<IndividualDayOff>();

            var existingRecord = IndividualDayOffTable.FindById(id);
            if (existingRecord == null)
            {
                new IndividualDayOffResult
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
            existingRecord.isAnnual = inputData.isAnnual;
            existingRecord.isWithoutPay = inputData.isWithoutPay;
            existingRecord.approvalStatus = inputData.approvalStatus;
            existingRecord.note = inputData.note;

            // Update the record in the collection
            IndividualDayOffTable.Update(existingRecord);
            var resultData = IndividualDayOffTable.FindAll();

            return new IndividualDayOffResult
            {
                data = resultData.ToList()
            };
        }

        [HttpDelete, Route("{id}")]
        public IndividualDayOffResult Delete(int id)
        {
            var IndividualDayOffTable = database.Table<IndividualDayOff>();

            var existingRecord = IndividualDayOffTable.FindById(id);
            if (existingRecord == null)
            {
                new IndividualDayOffResult
                {
                    code = 400,
                    message = "data not found",
                };
            }
            IndividualDayOffTable.Delete(id);
            var data = IndividualDayOffTable.FindAll();

            return new IndividualDayOffResult
            {
                data = data.ToList(),
            };
        }

        [HttpPut, Route("DuyetNgayPhep")]
        public IndividualDayOffResult ApproveIndividualDayOff([FromBody] ApprovalInput inputData)
        {
            var IndividualDayOffTable = database.Table<IndividualDayOff>();

            var existingRecord = IndividualDayOffTable.FindById(inputData.id);
            if (existingRecord == null)
            {
                new IndividualDayOffResult
                {
                    code = 400,
                    message = "data not found",
                };
            }

            // Update the existing record with new values
            existingRecord.approvalStatus = inputData.approvalStatus;

            // Update the record in the collection
            IndividualDayOffTable.Update(existingRecord);
            var resultData = IndividualDayOffTable.FindAll();

            return new IndividualDayOffResult
            {
                data = resultData.ToList()
            };
        }
    }

    public class IndividualDayOffResult : ApiResultBaseDO
    {
        public List<IndividualDayOff> data { get; set; }
    }

    public class IndividualDayOffInput
    {
        public int id { get; set; }
        public DateTime dateFrom { get; set; }
        public DateTime dateTo { get; set; }
        public float sumDay { get; set; }
        public int memberId { get; set; }
        public string reason { get; set; }
        public bool isAnnual { get; set; }
        public bool isWithoutPay { get; set; }
        public string approvalStatus { get; set; }
        public string note { get; set; }
    }

    public class ApprovalInput
    {
        public int id { get; set; }
        public string approvalStatus { get; set; }
    }
}
