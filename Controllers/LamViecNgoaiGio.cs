//using educlient.Data;
//using Microsoft.AspNetCore.Mvc;
//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace educlient.Controllers
//{
//    public class LamViecNgoaiGio : Controller
//    {
//        private readonly IDbLiteContext database;
//        public LamViecNgoaiGio(IDbLiteContext dataContext)
//        {
//            database = dataContext;
//        }
//        [HttpGet]
//        public OverTimeResult GetAll([FromQuery] DateTime? query_dateFrom = null, [FromQuery] DateTime? query_dateTo = null)
//        {
//            var OverTimeTable = database.Table<OverTime>();

//            List<OverTime> resultData;

//            if (query_dateFrom.HasValue && query_dateTo.HasValue)
//            {
//                resultData = OverTimeTable.Find(x =>
//                (x.dateFrom >= query_dateFrom.Value && x.dateTo <= query_dateTo.Value) ||
//                (x.dateFrom <= query_dateFrom.Value && x.dateTo >= query_dateTo.Value) ||
//                (x.dateFrom <= query_dateTo.Value && x.dateTo >= query_dateFrom.Value) ||
//                (x.dateTo >= query_dateFrom.Value && x.dateFrom <= query_dateTo.Value)
//                ).ToList();
//            }
//            else if (query_dateFrom.HasValue)
//            {
//                // Only dateFrom is provided
//                resultData = OverTimeTable.Find(x => x.dateFrom >= query_dateFrom.Value).ToList();
//            }
//            else if (query_dateFrom.HasValue)
//            {
//                // Only dateTo is provided
//                resultData = OverTimeTable.Find(x => x.dateTo <= query_dateTo.Value).ToList();
//            }
//            else
//            {
//                // No date filters provided
//                resultData = OverTimeTable.FindAll().ToList();
//            }

//            return new OverTimeResult
//            {
//                data = resultData
//            };
//        }

//        [HttpGet, Route("{id}")]
//        public OverTimeResult GetById(int id)
//        {
//            List<OverTime> returnData = new List<OverTime>();

//            var overTimeTable = database.Table<OverTime>();
//            var overTime = overTimeTable.FindById(id);

//            if (overTime == null)
//            {
//                return new OverTimeResult
//                {
//                    code = 404,
//                    message = "Data not found"
//                };
//            }
//            returnData.Add(overTime);

//            return new OverTimeResult
//            {
//                code = 200,
//                result = true,
//                data = returnData,
//            };
//        }

//        [HttpPost]
//        public OverTimeResult Insert([FromBody] OverTime[] inputData)
//        {
//            var insertData = inputData.Select(input => new OverTime
//            {
//                dateFrom = input.dateFrom,
//                dateTo = input.dateTo,
//                hours = input.hours,
//                fullName = input.fullName,
//                note = input.note
//            }).ToList();

//            var overTimeTable = database.Table<OverTime>();
//            overTimeTable.Insert(insertData);
//            var resultData = overTimeTable.FindAll();

//            return new OverTimeResult
//            {
//                data = resultData.ToList(),
//            };
//        }

//        [HttpPut, Route("{id}")]
//        public OverTimeResult Update(int id, [FromBody] OverTime inputData)
//        {
//            var overTimeTable = database.Table<OverTime>();

//            var existingRecord = overTimeTable.FindById(id);
//            if (existingRecord == null)
//            {
//                new CommissionsResult
//                {
//                    code = 400,
//                    message = "data not found",
//                };
//            }

//            // Update the existing record with new values
//            existingRecord.dateFrom = inputData.dateFrom;
//            existingRecord.dateTo = inputData.dateTo;
//            existingRecord.hours = inputData.hours;
//            existingRecord.fullName = inputData.fullName;
//            existingRecord.note = inputData.note;



//            // Update the record in the collection
//            overTimeTable.Update(existingRecord);
//            var resultData = overTimeTable.FindAll();

//            return new OverTimeResult
//            {
//                data = resultData.ToList()
//            };
//        }
//        [HttpDelete, Route("{id}")]
//        public OverTimeResult Delete(int id)

//        {
//            var overTimeTable = database.Table<OverTime>();
//            var existingRecord = overTimeTable.FindById(id);

//            if (existingRecord == null)
//            {
//                return new OverTimeResult
//                {
//                    code = 404,
//                    message = "Data not found"
//                };
//            }

//            overTimeTable.Delete(id);
//            var resultData = overTimeTable.FindAll();

//            return new OverTimeResult
//            {
//                data = resultData.ToList()
//            };
//        }
//        public class OverTimeResult : ApiResultBaseDO
//        {
//            public List<OverTime> data { get; set; }
//        }
//    }
//}
