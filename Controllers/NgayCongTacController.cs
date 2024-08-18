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
    public class NgayCongTacController : Controller
    {
        private readonly IDbLiteContext database;
        public NgayCongTacController(IDbLiteContext dataContext)
        {
            database = dataContext;
        }

        [HttpGet]
        public CommissionsResult GetAll([FromQuery] DateTime? query_dateFrom = null, [FromQuery] DateTime? query_dateTo = null)
        {
            var CommissionTable = database.Table<Commission>();

            List<Commission> resultData;

            if (query_dateFrom.HasValue && query_dateTo.HasValue)
            {
                resultData = CommissionTable.Find(x =>
                (x.dateFrom >= query_dateFrom.Value && x.dateTo <= query_dateTo.Value) ||
                (x.dateFrom <= query_dateFrom.Value && x.dateTo >= query_dateTo.Value) ||
                (x.dateFrom <= query_dateTo.Value && x.dateTo >= query_dateFrom.Value) ||
                (x.dateTo >= query_dateFrom.Value && x.dateFrom <= query_dateTo.Value)
                ).ToList();
            }
            else if (query_dateFrom.HasValue)
            {
                // Only dateFrom is provided
                resultData = CommissionTable.Find(x => x.dateFrom >= query_dateFrom.Value).ToList();
            }
            else if (query_dateFrom.HasValue)
            {
                // Only dateTo is provided
                resultData = CommissionTable.Find(x => x.dateTo <= query_dateTo.Value).ToList();
            }
            else
            {
                // No date filters provided
                resultData = CommissionTable.FindAll().ToList();
            }

            return new CommissionsResult
            {
                data = resultData
            };
        }

        [HttpGet, Route("{id}")]
        public CommissionsResult GetById(int id)
        {
            List<Commission> returnData = new List<Commission>();

            var CommissionTable = database.Table<Commission>();
            var commission = CommissionTable.FindById(id);

            if (commission == null)
            {
                return new CommissionsResult
                {
                    code = 404,
                    message = "Data not found"
                };
            }
            returnData.Add(commission);

            return new CommissionsResult
            {
                code = 200,
                result = true,
                data = returnData,
            };
        }

        [HttpPost]
        public CommissionsResult Insert([FromBody] CommissionInput[] inputData)
        {
            var insertData = inputData.Select(input => new Commission
            {
                dateFrom = input.dateFrom,
                dateTo = input.dateTo,
                sumDay = input.sumDay,
                comissionContent = input.comissionContent,
                transportation = input.transportation,
                memberList = input.memberList,
                commissionExpenses = input.commissionExpenses,
                note = input.note,
            }).ToList();

            var CommissionTable = database.Table<Commission>();
            CommissionTable.Insert(insertData);
            var resultData = CommissionTable.FindAll();

            return new CommissionsResult
            {
                data = resultData.ToList(),
            };
        }

        [HttpPut, Route("{id}")]
        public CommissionsResult Update(int id, [FromBody] CommissionInput inputData)
        {
            var CommissionTable = database.Table<Commission>();

            var existingRecord = CommissionTable.FindById(id);
            if (existingRecord == null)
            {
                new CommissionsResult
                {
                    code = 400,
                    message = "data not found",
                };
            }

            // Update the existing record with new values
            existingRecord.dateFrom = inputData.dateFrom;
            existingRecord.dateTo = inputData.dateTo;
            existingRecord.sumDay = inputData.sumDay;
            existingRecord.comissionContent = inputData.comissionContent;
            existingRecord.transportation = inputData.transportation;
            existingRecord.memberList = inputData.memberList;
            existingRecord.commissionExpenses = inputData.commissionExpenses;
            existingRecord.note = inputData.note;


            // Update the record in the collection
            CommissionTable.Update(existingRecord);
            var resultData = CommissionTable.FindAll();

            return new CommissionsResult
            {
                data = resultData.ToList()
            };
        }

        [HttpDelete, Route("{id}")]
        public CommissionsResult Delete(int id)
        {
            var CommissionTable = database.Table<Commission>();
            var existingRecord = CommissionTable.FindById(id);

            if (existingRecord == null)
            {
                return new CommissionsResult
                {
                    code = 404,
                    message = "Data not found"
                };
            }

            CommissionTable.Delete(id);
            var resultData = CommissionTable.FindAll();

            return new CommissionsResult
            {
                data = resultData.ToList()
            };
        }
    }

    public class CommissionsResult : ApiResultBaseDO
    {
        public List<Commission> data { get; set; }
    }

    public class CommissionInput
    {
        public int id { get; set; }
        public DateTime dateFrom { get; set; }
        public DateTime dateTo { get; set; }
        public float sumDay { get; set; }
        public string comissionContent { get; set; }
        public string transportation { get; set; }
        public List<CommissionMember> memberList { get; set; }
        public int commissionExpenses { get; set; }
        public string note { get; set; }
    }
}
