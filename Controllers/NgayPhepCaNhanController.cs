using educlient.Data;
using LiteDB;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using static MongoDB.Bson.Serialization.Serializers.SerializerHelper;

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
                    resultData = IndividualDayOffTable.Find(x =>
                        x.memberId == query_memberId.Value &&
                        x.date >= query_dateFrom.Value
                    ).ToList();
                }
                else if (query_dateTo.HasValue)
                {
                    // Only dateTo is provided
                    resultData = IndividualDayOffTable.Find(x =>
                        x.memberId == query_memberId.Value &&
                        x.date <= query_dateTo.Value
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
                    resultData = IndividualDayOffTable.Find(x => x.date >= query_dateFrom.Value).ToList();
                }
                else if (query_dateTo.HasValue)
                {
                    // Only dateTo is provided
                    resultData = IndividualDayOffTable.Find(x => x.date <= query_dateTo.Value).ToList();
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
                date = input.date,
                memberId = input.memberId,
                reason = input.reason,
                periodType = input.periodType,
                dayOffType = input.dayOffType,
                isDayOffWithPayment = input.isDayOffWithPayment,
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

        //[HttpPut, Route("chuyenData")]
        //public object ApiFixData()
        //{
        //    var individualDayOffTable = database.Table<IndividualDayOff>();
        //    var dateList = individualDayOffTable.Query().Select(x => new
        //    {
        //        x.numberOfDay_half,
        //        x.numberOfDay_whole,
        //        x.sumDay,
        //        x.totalIsAnnual,
        //        x.totalIsWithoutPay,
        //        x.isWithoutPay,
        //        x.isAnnual,
        //        x.id
        //    }).ToList();
        //    foreach (var item in dateList)
        //    {
        //        float totalAnnual = 0;
        //        float totalIsWithoutPay = 0;
        //        var numberOfDayHalf = 0;
        //        if (item.sumDay % 1 != 0) // Check if sumDay is a float with a fractional part
        //        {
        //            numberOfDayHalf += 1; // Increment numberOfDay_half by 1
        //        }

        //        if (item.isAnnual)
        //        {
        //            totalAnnual = (float)item.sumDay;
        //        }
        //        else if (item.isWithoutPay)
        //        {
        //            totalIsWithoutPay = (float)item.sumDay;
        //        }
        //        var numberOfDayTotal = (int)item.sumDay;

        //        var existingRecord = individualDayOffTable.FindById(item.id);
        //        try
        //        {
        //            existingRecord.numberOfDay_whole = numberOfDayTotal;
        //            existingRecord.numberOfDay_half = numberOfDayHalf;
        //            existingRecord.totalIsAnnual = totalAnnual;
        //            existingRecord.totalIsWithoutPay = totalIsWithoutPay;
        //            individualDayOffTable.Update(existingRecord);
        //        }
        //        catch (Exception e)
        //        {
        //            Console.WriteLine(e);
        //            throw;
        //        }
        //    }
        //    Console.WriteLine("done");
        //    return dateList;
        //}

        [HttpPut, Route("{id}")]
        public DayOffUpdateDTO Update(int id, [FromBody] IndividualDayOffInput inputData)
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
            existingRecord.date = inputData.date;
            existingRecord.memberId = inputData.memberId;
            existingRecord.periodType = inputData.periodType;
            existingRecord.dayOffType = inputData.dayOffType;
            existingRecord.reason = inputData.reason;
            existingRecord.isDayOffWithPayment = inputData.isDayOffWithPayment;
            existingRecord.approvalStatus = inputData.approvalStatus;
            existingRecord.note = inputData.note;

            // Update the record in the collection
            IndividualDayOffTable.Update(existingRecord);

            return new DayOffUpdateDTO
            {
                message = "Update Success",
                code = 200,
                result = true,
                data = existingRecord
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
                    x.date.Year == year &&
                    x.approvalStatus == "Đã duyệt"
                    ).ToList();

                var countDayOff = 0;
                //foreach (var dayOff in dayOffData)
                //{
                //    countDayOff += (int)dayOff.sumDay;
                //}

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
                    //absenceQuota = member.absenceQuota,
                    //WFHQuota = member.WFHQuota,
                    absenceQuota = 0,
                    WFHQuota = 0,
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

        [HttpGet("HanMucNghiPhepCaNhan")]
        public HanMucNghiPhepCaNhanResult GetHanMucNghiPhepCaNhan([FromQuery] int year, [FromQuery] int? query_memberId = null)
        {
            var resultList = new List<HanMucNghiPhepCaNhan>();

            var AQMemberTable = database.Table<AQMember>();
            var dayOffsTable = database.Table<IndividualDayOff>();

            int minAbsenceQuota = AQMemberTable.FindById(query_memberId).minAbsenceQuota;
            int additionalAbsenceQuota = AQMemberTable.FindById(query_memberId).additionalAbsenceQuota;
            float totalDayOff = 0;
            float totalDayOff_with_permission = 0;
            float totalDayOff_without_permission = 0;
            float totalDayOff_fullType1 = 0;
            float totalDayOff_fullType2 = 0;
            float totalDayOff_halfType1 = 0;
            float totalDayOff_halfType2 = 0;
            float totalDayOff_type3_4 = 0;
            float usedMinAbsenceQuota = 0;
            float usedAdditionalAbsenceQuota = 0;
            float remainMinAbsenceQuota = 0;
            float remainAdditionalAbsenceQuota = 0;

            var dayOffData = dayOffsTable.Find(x =>
                    x.memberId == query_memberId &&
                    x.date.Year == year &&
                    x.approvalStatus == "Đã duyệt"
                    ).ToList();

            totalDayOff = dayOffData.Count;

            foreach (var dayOff in dayOffData)
            {
                totalDayOff_with_permission += (dayOff.dayOffType == 3 || dayOff.dayOffType == 4) ? 1 : 0;
                totalDayOff_fullType1 += (dayOff.periodType == 1 && dayOff.dayOffType == 1) ? 1 : 0;
                totalDayOff_fullType2 += (dayOff.periodType == 1 && dayOff.dayOffType == 2) ? 1 : 0;
                totalDayOff_halfType1 += ((dayOff.periodType == 2 || dayOff.periodType == 3) && dayOff.dayOffType == 1) ? 1 : 0;
                totalDayOff_halfType2 += ((dayOff.periodType == 2 || dayOff.periodType == 3) && dayOff.dayOffType == 2) ? 1 : 0;
                totalDayOff_type3_4 += (dayOff.dayOffType == 3 || dayOff.dayOffType == 4) ? 1 : 0;
            }

            usedMinAbsenceQuota = (float)(totalDayOff_fullType1 + totalDayOff_halfType1 * 0.5);
            usedAdditionalAbsenceQuota = (float)(totalDayOff_fullType2 + totalDayOff_halfType2 * 0.5);
            remainMinAbsenceQuota = minAbsenceQuota - usedMinAbsenceQuota;
            remainAdditionalAbsenceQuota = additionalAbsenceQuota - usedAdditionalAbsenceQuota;

            totalDayOff_without_permission = totalDayOff - totalDayOff_with_permission;

            var HanMucNghiPhep = new HanMucNghiPhepCaNhan
            {
                year = year,
                memberId = query_memberId.Value,
                minAbsenceQuota = minAbsenceQuota,
                additionalAbsenceQuota = additionalAbsenceQuota,
                totalDayOff = totalDayOff,
                totalDayOffFullType1 = totalDayOff_fullType1,
                totalDayOffFullType2 = totalDayOff_fullType2,
                usedAdditionalAbsenceQuota = usedAdditionalAbsenceQuota,
                usedMinAbsenceQuota = usedMinAbsenceQuota,
                remainAdditionalAbsenceQuota = remainAdditionalAbsenceQuota,
                remainMinAbsenceQuota = remainMinAbsenceQuota,
            };

            resultList.Add(HanMucNghiPhep);

            return new HanMucNghiPhepCaNhanResult
            {
                message = "Success",
                code = 200,
                result = true,
                data = resultList
            };
        }

        public class IndividualDayOffResult : ApiResultBaseDO
        {
            public List<IndividualDayOff> data { get; set; }
        }


        public class IndividualDayOffInput
        {
            public int id { get; set; }
            public DateTime date { get; set; }
            public int memberId { get; set; }
            public string reason { get; set; }
            public int periodType { get; set; }
            public int dayOffType { get; set; }
            public bool isDayOffWithPayment { get; set; }
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

        public class HanMucNghiPhepCaNhanResult : ApiResultBaseDO
        {
            public List<HanMucNghiPhepCaNhan> data { get; set; }
        }

        public class HanMucNghiPhepCaNhan
        {
            public int memberId { get; set; }
            public int year { get; set; }
            public int minAbsenceQuota { get; set; }
            public int additionalAbsenceQuota { get; set; }
            public float totalDayOff { get; set; }
            public float totalDayOffFullType1 { get; set; }
            public float totalDayOffFullType2 { get; set; }

            public float totalDayOffHalfType1 { get; set; }
            public float totalDayOffHalfType2 { get; set; }
            public float totalDayOffType3_4 { get; set; }
            public float usedMinAbsenceQuota { get; set; }
            public float usedAdditionalAbsenceQuota { get; set; }
            public float remainMinAbsenceQuota { get; set; }
            public float remainAdditionalAbsenceQuota { get; set; }
        }

        public class DayOffUpdateDTO : ApiResultBaseDO
        {
            public IndividualDayOff data { get; set; }
        }
    }

}
