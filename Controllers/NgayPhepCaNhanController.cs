using educlient.Data;
using LiteDB;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
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
                message = "Success",
                code = 200,
                result = true,
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
                message = "Success",
                code = 200,
                result = true,
                data = returnData,
            };
        }


        [HttpPost]
        public ApiResultBaseDO Insert([FromBody] IndividualDayOffInput[] inputData)
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

            return new ApiResultBaseDO
            {
                message = "Insert Success",
                code = 200,
                result = true
            };
        }

        [HttpPut, Route("{id}")]
        public ApiResultBaseDO Update(int id, [FromBody] IndividualDayOffInput inputData)
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

            return new ApiResultBaseDO
            {
                message = "Delete Success",
                code = 200,
                result = true
            };
        }

        [HttpPut, Route("DuyetNgayPhep")]
        public ApiResultBaseDO ApproveIndividualDayOff([FromBody] ApprovalInput inputData)
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

            return new ApiResultBaseDO
            {
                message = "Approve Success",
                code = 200,
                result = true
            };
        }
        [HttpGet("Thongkenghiphepnam")]
        public ThongKePhepNamResult GetNghiPhepInYear([FromQuery] int year, [FromQuery] int? query_memberId = null)
        {

            // Get the tables
            var membersTable = database.Table<AQMember>();
            var dayOffsTable = database.Table<IndividualDayOff>();
            var workingOnlineTable = database.Table<WorkingOnlineDataDO>();

            var membersData = membersTable.FindAll().ToList();
            if (membersData == null)
            {
                return new ThongKePhepNamResult
                {
                    message = "Member not found",
                    code = 404,
                    result = false,
                    data = null
                };
            }
            var resultList = new List<ThongKePhepNamDataDO>();
            foreach (var member in membersData)
            {

                // Find day-off data for each member by year
                var dayOffData = dayOffsTable.Find(x =>
                    x.memberId == member.id &&
                    x.dateFrom.Year == year &&
                    x.isAnnual == true &&
                    x.approvalStatus == "Đã duyệt" &&
                    x.sumDay != 0
                    ).ToList();

                var countDayOff = 0;
                foreach (var dayOff in dayOffData)
                {
                    countDayOff += (int)dayOff.sumDay;
                }

                var wfhData = workingOnlineTable.Find(x =>
                    x.memberId == member.id &&
                    x.dateFrom.Year == year &&
                    x.approvalStatus == "Đã duyệt" &&
                    x.sumDay != 0
                    ).ToList();

                var countWorkingOnline = 0;
                foreach (var wfhday in wfhData)
                {
                    countWorkingOnline += (int)wfhday.sumDay;
                }

                // Combine member data with their day-off data
                var resultData = new ThongKePhepNamDataDO
                {
                    fullName = member.fullName,
                    nickName = member.nickName,
                    absenceQuota = member.absenceQuota,
                    WFHQuota = member.WFHQuota,
                    DayOffs = countDayOff,
                    total_wfh = countWorkingOnline
                };

                resultList.Add(resultData);
            }
            return new ThongKePhepNamResult
            {
                message = "Success",
                code = 200,
                result = true,
                data = resultList
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
    public class ThongKePhepNamDataDO
    {
        public string fullName { get; set; }
        public string nickName { get; set; }
        public int absenceQuota { get; set; }
        public int WFHQuota { get; set; }
        public int DayOffs { get; set; }
        public int total_wfh { get; set; }
    }

    public class ThongKePhepNamResult : ApiResultBaseDO
    {
        public List<ThongKePhepNamDataDO> data { get; set; }
    }
}
