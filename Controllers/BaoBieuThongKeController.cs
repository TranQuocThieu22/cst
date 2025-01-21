using educlient.Data;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using static educlient.Controllers.NgayPhepCaNhanController;

namespace educlient.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaoBieuThongKeController : ControllerBase
    {
        private readonly IDbLiteContext _database;

        public BaoBieuThongKeController(IDbLiteContext dataContext)
        {
            _database = dataContext;
        }

        [HttpGet("ThongKeTinhTienAnTrua")]
        public ThongKeTinhTienAnTruaResult ThongKeTinhTienAnTrua([FromQuery] int year, [FromQuery] int month)
        {
            float total_IndividualDayOff = 0;
            float total_WorkingOnline = 0;
            int total_IndividualDayOff_full = 0;
            int total_IndividualDayOff_half = 0;
            int total_WorkingOnline_full = 0;
            int total_WorkingOnline_half = 0;
            int total_CommissionDay_full = 0;
            int total_CommissionDay_half = 0;
            float total_CommissionDay = 0;
            float total_AQDayOff = 0;

            var resultList = new List<ThongKeTinhTienAnTruaDataDO>();

            var membersTable = _database.Table<AQMember>();
            var dayOffsTable = _database.Table<IndividualDayOff>();
            var workingOnlineTable = _database.Table<WorkingOnlineDay>();
            var commissionTable = _database.Table<Commission>();
            var aqDayOffTable = _database.Table<DayOff>();

            var membersData = membersTable.FindAll().ToList();
            if (membersData == null)
            {
                return new ThongKeTinhTienAnTruaResult
                {
                    message = "Member not found",
                    code = 404,
                    result = false,
                    data = null
                };
            }

            foreach (var member in membersData)
            {
                total_AQDayOff = 0;
                total_CommissionDay_full = 0;
                total_CommissionDay_half = 0;
                // Find day-off data for each member by month-year
                total_IndividualDayOff_full = dayOffsTable.Find(x =>
                    x.memberId == member.id &&
                    x.date.Year == year &&
                    x.date.Month == month &&
                    x.approvalStatus == "Đã duyệt" &&
                    x.periodType == 1
                    ).ToList().Count;

                total_IndividualDayOff_half = dayOffsTable.Find(x =>
                    x.memberId == member.id &&
                    x.date.Year == year &&
                    x.date.Month == month &&
                    x.approvalStatus == "Đã duyệt" &&
                    x.periodType == 2 || x.periodType == 3
                    ).ToList().Count;

                total_IndividualDayOff = total_IndividualDayOff_full + (float)(total_IndividualDayOff_half * 0.5);

                // Find wfh data for each member by month-year
                total_WorkingOnline_full = workingOnlineTable.Find(x =>
                    x.memberId == member.id &&
                    x.date.Year == year &&
                    x.date.Month == month &&
                    x.approvalStatus == "Đã duyệt" &&
                    x.periodType == 1
                    ).ToList().Count;

                total_WorkingOnline_half = workingOnlineTable.Query()
                 .Where(x => x.date.Year == year &&
                    x.date.Month == month &&
                    x.approvalStatus == "Đã duyệt" && x.periodType == 2 || x.periodType == 3)
                 .ToList()
                 .Where(x => x.memberId == member.id)
                  .ToList().Count;

                total_WorkingOnline = total_WorkingOnline_full + (float)(total_WorkingOnline_half * 0.5);

                var commissionData = commissionTable.Query()
                    .Where(x => x.dateFrom.Year == year && x.dateFrom.Month == month)
                    .ToList()  // Get filtered by date records first
                    .Where(x => x.memberList.Select(m => m.id).Contains(member.id))  // Then filter for specific member ID
                    .ToList();

                if (commissionData.Any())  // Check if commissionData contains records
                {
                    foreach (var data in commissionData)
                    {
                        if (data.sumDay == 0.5)
                        {
                            total_CommissionDay_half += 1;
                        }
                        else
                        {
                            DateTime startDate = data.dateFrom;
                            DateTime endDate = data.dateTo;
                            // If the record spans across different months
                            if (startDate.Month != endDate.Month || startDate.Year != endDate.Year)
                            {
                                DateTime monthStart = new DateTime(startDate.Year, startDate.Month, 1);
                                DateTime monthEnd = monthStart.AddMonths(1).AddDays(-1);
                                DateTime endOfFirstMonth = endDate < monthEnd ? endDate : monthEnd;

                                if (startDate <= endOfFirstMonth)
                                {
                                    int daysInFirstMonth = (endOfFirstMonth - startDate).Days + 1;
                                    total_CommissionDay_full += daysInFirstMonth;
                                }
                            }
                            else
                            {
                                total_CommissionDay_full += (int)data.sumDay;
                            }
                        }
                    }
                }
                else
                {
                    total_CommissionDay_full = 0;  // Set to 0 if no commission data
                }

                total_CommissionDay = total_CommissionDay_full + (float)(total_CommissionDay_half * 0.5);

                DateTime startOfCurrentMonth = new DateTime(year, month, 1);
                DateTime endOfCurrentMonth = startOfCurrentMonth.AddMonths(1).AddDays(-1);

                var AQDayOffData = aqDayOffTable.Find(x =>
                    (x.dateFrom >= startOfCurrentMonth && x.dateTo <= endOfCurrentMonth) ||
                    (x.dateFrom <= startOfCurrentMonth && x.dateTo >= endOfCurrentMonth) ||
                    (x.dateFrom <= endOfCurrentMonth && x.dateTo >= startOfCurrentMonth) ||
                    (x.dateTo >= startOfCurrentMonth && x.dateFrom <= endOfCurrentMonth)
                ).ToList();

                foreach (var AQDayOff in AQDayOffData)
                {
                    DateTime startDate = AQDayOff.dateFrom;
                    DateTime endDate = AQDayOff.dateTo;

                    if (startDate >= startOfCurrentMonth && endDate <= endOfCurrentMonth)
                    {
                        total_AQDayOff += AQDayOff.sumDay;
                    }
                    else if (startDate < startOfCurrentMonth && endDate <= endOfCurrentMonth)
                    {
                        total_AQDayOff += GetWeekdaysCount(startOfCurrentMonth, endDate);
                    }
                    else if (startDate <= endOfCurrentMonth && endDate > endOfCurrentMonth)
                    {
                        total_AQDayOff += GetWeekdaysCount(startDate, endOfCurrentMonth);
                    }
                }


                var resultData = new ThongKeTinhTienAnTruaDataDO
                {
                    id = member.id,
                    fullName = member.fullName,
                    nickName = member.nickName,
                    employeeType = member.employeeType,
                    total_IndividualDayOff = total_IndividualDayOff,
                    total_WorkingOnline = total_WorkingOnline,
                    total_IndividualDayOff_full = total_IndividualDayOff_full,
                    total_IndividualDayOff_half = total_IndividualDayOff_half,
                    total_WorkingOnline_full = total_WorkingOnline_full,
                    total_WorkingOnline_half = total_WorkingOnline_half,
                    total_CommissionDay_full = total_CommissionDay_full,
                    total_CommissionDay_half = total_CommissionDay_half,
                    total_AQDayOff = total_AQDayOff,
                    total_CommissionDay = total_CommissionDay
                };

                resultList.Add(resultData);
            }
            return new ThongKeTinhTienAnTruaResult
            {
                message = "Success",
                code = 200,
                result = true,
                data = resultList
            };
        }

        /// <summary>
        /// Calculates the number of weekdays (excluding Saturdays and Sundays) between two dates inclusive.
        /// </summary>
        private int GetWeekdaysCount(DateTime start, DateTime end)
        {
            int weekdayCount = 0;
            for (DateTime date = start; date <= end; date = date.AddDays(1))
            {
                if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                {
                    weekdayCount++;
                }
            }
            return weekdayCount;
        }

        [HttpGet("ThongKeTinhTienCongTac")]
        public ThongKeTinhTienCongTacResult ThongKeTinhTienCongTac([FromQuery] DateTime? query_dateFrom = null, [FromQuery] DateTime? query_dateTo = null, int? year = null)
        {
            var totalExpense = 0;
            int total_CommissionDay_full = 0;
            int total_CommissionDay_half = 0;

            // Get the tables
            var membersTable = _database.Table<AQMember>();
            var commissionTable = _database.Table<Commission>();

            var membersData = membersTable.FindAll().ToList();
            if (membersData == null)
            {
                return new ThongKeTinhTienCongTacResult
                {
                    message = "Member not found",
                    code = 404,
                    result = false,
                    data = null
                };
            }
            var resultList = new List<ThongKeTinhTienCongTacDataDO>();
            foreach (var member in membersData)
            {
                totalExpense = 0;
                total_CommissionDay_full = 0;
                total_CommissionDay_half = 0;
                var commissionData = commissionTable.Query()
                  .Where(x => x.dateFrom >= query_dateFrom.Value && x.dateTo <= query_dateTo.Value)
                  .ToList()  // Get filtered by date records first
                  .Where(x => x.memberList.Select(m => m.id).Contains(member.id))  // Then filter for specific member ID
                  .ToList();

                if (commissionData.Any(d => d.memberList.Any(m => m.id == member.id)))
                {
                    // Execute only for members with a commission
                    commissionData.ForEach(d =>
                    {
                        if (d.sumDay == 0.5)
                        {
                            total_CommissionDay_half += 1;
                        }
                        total_CommissionDay_full += (int)d.sumDay;
                        var memberExpenseData = d.memberList.Find(x => x.id == member.id);
                        if (memberExpenseData != null)
                        {
                            totalExpense += memberExpenseData.memberExpenses;
                        }
                    });
                }

                // Combine member data with their day-off data
                var resultData = new ThongKeTinhTienCongTacDataDO
                {
                    id = member.id,
                    fullName = member.fullName,
                    nickName = member.nickName,
                    total_CommissionDay_full = total_CommissionDay_full,
                    total_CommissionDay_half = total_CommissionDay_half,
                    total_CommissionPayment = totalExpense
                };

                resultList.Add(resultData);
            }
            return new ThongKeTinhTienCongTacResult
            {
                message = "Success",
                code = 200,
                result = true,
                data = resultList
            };
        }

        [HttpGet("ThongKeNgayNghiCaNhan")]
        public ThongKeNgayNghiCaNhanResult ThongKeNgayNghiById([FromQuery] int year, [FromQuery] int month, [FromQuery] int? query_memberId = null)
        {
            // Get the tables
            var membersTable = _database.Table<AQMember>();
            var dayOffsTable = _database.Table<IndividualDayOff>();
            var aqDayOffTable = _database.Table<DayOff>();

            var member = membersTable.FindById(query_memberId);
            if (member == null)
            {
                return new ThongKeNgayNghiCaNhanResult
                {
                    message = "Member not found",
                    code = 404,
                    result = false,
                    data = null
                };
            }
            var resultList = new List<ThongKeNgayNghiCaNhanDataDO>();
            // Find day-off data for each member by year
            var dayOffData = dayOffsTable.Find(x =>
                x.memberId == query_memberId &&
                x.date.Year == year &&
                x.date.Month == month &&
                x.approvalStatus == "Đã duyệt"
                ).ToList();

            var countDayOff = 0;
            //foreach (var dayOff in dayOffData)
            //{
            //    countDayOff += (int)dayOff.sumDay;
            //}

            var aqDayOffData = aqDayOffTable.Find(x =>
                x.dateFrom.Year == year &&
                x.dateFrom.Month == month
                ).ToList();
            var countaqDayOff = 0;
            foreach (var aqDayOff in aqDayOffData)
            {
                countaqDayOff += (int)aqDayOff.sumDay;
            }


            // Combine member data with their day-off data
            var resultData = new ThongKeNgayNghiCaNhanDataDO
            {
                id = member.id,
                fullName = member.fullName,
                nickName = member.nickName,
                userTotalDayOff_Month = countDayOff + countaqDayOff,
            };

            resultList.Add(resultData);

            return new ThongKeNgayNghiCaNhanResult
            {
                message = "Success",
                code = 200,
                result = true,
                data = resultList
            };
        }

        [HttpGet("Thongkehanmuccanhan")]
        public ThongKeCaNhanNamHienTaiResult GetThongkehanmuccanhan([FromQuery] int year)
        {
            var resultList = new List<ThongKeNghiPhepVaLamOnlineDO>();

            var AQMemberTable = _database.Table<AQMember>();
            var dayOffsTable = _database.Table<IndividualDayOff>();
            var workingOnlineTable = _database.Table<WorkingOnlineDay>();

            var membersData = AQMemberTable.FindAll().ToList();
            if (membersData == null)
            {
                return new ThongKeCaNhanNamHienTaiResult
                {
                    message = "Member not found",
                    code = 404,
                    result = false,
                    data = null
                };
            }

            foreach (var member in membersData)
            {
                //dayOff data
                int minAbsenceQuota = AQMemberTable.FindById(member.id).minAbsenceQuota;
                int additionalAbsenceQuota = AQMemberTable.FindById(member.id).additionalAbsenceQuota;
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
                        x.memberId == member.id &&
                        x.date.Year == year &&
                        x.approvalStatus == "Đã duyệt"
                        ).ToList();



                foreach (var dayOff in dayOffData)
                {
                    totalDayOff_with_permission += (dayOff.dayOffType == 3 || dayOff.dayOffType == 4) ? 1 : 0;
                    if (dayOff.periodType == 1)
                    {
                        totalDayOff_fullType1 += (dayOff.dayOffType == 1) ? 1 : 0;
                        totalDayOff_fullType2 += (dayOff.dayOffType == 2) ? 1 : 0;

                    }
                    else if (dayOff.periodType == 2 || dayOff.periodType == 3)
                    {
                        totalDayOff_type3_4 += (dayOff.dayOffType == 3 || dayOff.dayOffType == 4) ? (float)0.5 : 0;
                        totalDayOff_halfType1 += (dayOff.dayOffType == 1) ? 1 : 0;
                        totalDayOff_halfType2 += (dayOff.dayOffType == 2) ? 1 : 0;
                    }
                    else
                    {
                        totalDayOff_type3_4 += (dayOff.dayOffType == 3 || dayOff.dayOffType == 4) ? 1 : 0;
                    }
                }

                usedMinAbsenceQuota = (float)(totalDayOff_fullType1 + totalDayOff_halfType1 * 0.5);
                usedAdditionalAbsenceQuota = (float)(totalDayOff_fullType2 + totalDayOff_halfType2 * 0.5);
                remainMinAbsenceQuota = minAbsenceQuota - usedMinAbsenceQuota;
                remainAdditionalAbsenceQuota = additionalAbsenceQuota - usedAdditionalAbsenceQuota;



                //workingOnline data
                int minWfhQuota = AQMemberTable.FindById(member.id).minWFHQuota;
                int additionalWfhQuota = AQMemberTable.FindById(member.id).additionalWFHQuota;
                float totalWorkingOnlineDay = 0;
                float totalWorkingOnlineDay_with_permission = 0;
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
                        x.memberId == member.id &&
                        x.date.Year == year &&
                        x.approvalStatus == "Đã duyệt"
                        ).ToList();

                totalDayOff = (float)(totalDayOff_fullType1 + totalDayOff_fullType2 + totalDayOff_halfType1 * 0.5 + totalDayOff_halfType2 * 0.5 + totalDayOff_type3_4);
                foreach (var workingOnlineDay in workingOnlineDayData)
                {
                    totalWorkingOnlineDay_with_permission += (workingOnlineDay.wfhType == 3 || workingOnlineDay.wfhType == 4) ? 1 : 0;

                    if (workingOnlineDay.periodType == 1)
                    {
                        totalWorkingOnlineDay_fullType1 += (workingOnlineDay.wfhType == 1) ? 1 : 0;
                        totalWorkingOnlineDay_fullType2 += (workingOnlineDay.wfhType == 2) ? 1 : 0;
                    }
                    else if (workingOnlineDay.periodType == 2 || workingOnlineDay.periodType == 3)
                    {
                        totalWorkingOnlineDay_halfType1 += (workingOnlineDay.wfhType == 1) ? 1 : 0;
                        totalWorkingOnlineDay_halfType2 += (workingOnlineDay.wfhType == 2) ? 1 : 0;
                        totalWorkingOnlineDay_type3_4 += (workingOnlineDay.wfhType == 3 || workingOnlineDay.wfhType == 4) ? (float)0.5 : 0;
                    }
                    else
                    {
                        totalWorkingOnlineDay_type3_4 += (workingOnlineDay.wfhType == 3 || workingOnlineDay.wfhType == 4) ? 1 : 0;
                    }
                }

                usedMinWfhQuota = (float)(totalWorkingOnlineDay_fullType1 + totalWorkingOnlineDay_halfType1 * 0.5);
                usedAdditionalWfhQuota = (float)(totalWorkingOnlineDay_fullType2 + totalWorkingOnlineDay_halfType2 * 0.5);
                remainMinWfhQuota = minWfhQuota - usedMinWfhQuota;
                remainAdditionalWfhQuota = additionalWfhQuota - usedAdditionalWfhQuota;

                totalWorkingOnlineDay = (float)(totalWorkingOnlineDay_fullType1 + totalWorkingOnlineDay_fullType2 + totalWorkingOnlineDay_halfType1 * 0.5 + totalWorkingOnlineDay_halfType2 * 0.5 + totalWorkingOnlineDay_type3_4);

                var ThongKeCaNhanByMember = new ThongKeNghiPhepVaLamOnlineDO
                {
                    memberId = member.id,
                    fullName = member.fullName,
                    role = member.role,
                    employeeType = member.employeeType,
                    minAbsenceQuota = minAbsenceQuota,
                    //dayOff data
                    additionalAbsenceQuota = additionalAbsenceQuota,
                    totalDayOff = totalDayOff,
                    totalDayOffFullType1 = totalDayOff_fullType1,
                    totalDayOffFullType2 = totalDayOff_fullType2,
                    totalDayOffHalfType1 = totalDayOff_halfType1,
                    totalDayOffHalfType2 = totalDayOff_halfType2,
                    totalDayOffType3_4 = totalDayOff_type3_4,
                    usedAdditionalAbsenceQuota = usedAdditionalAbsenceQuota,
                    usedMinAbsenceQuota = usedMinAbsenceQuota,
                    remainAdditionalAbsenceQuota = remainAdditionalAbsenceQuota,
                    remainMinAbsenceQuota = remainMinAbsenceQuota,
                    //workingOnline data
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

                resultList.Add(ThongKeCaNhanByMember);
            }

            return new ThongKeCaNhanNamHienTaiResult
            {
                message = "Success",
                code = 200,
                result = true,
                data = resultList
            };
        }

        public class ThongKeTinhTienAnTruaDataDO
        {
            public int id { get; set; }
            public string fullName { get; set; }
            public string nickName { get; set; }
            public int employeeType { get; set; }
            public float total_IndividualDayOff { get; set; }
            public float total_WorkingOnline { get; set; }
            public int total_IndividualDayOff_full { get; set; }
            public int total_IndividualDayOff_half { get; set; }
            public int total_WorkingOnline_full { get; set; }
            public int total_WorkingOnline_half { get; set; }
            public int total_CommissionDay_full { get; set; }
            public int total_CommissionDay_half { get; set; }
            public float total_AQDayOff { get; set; }
            public float total_CommissionDay { get; set; }
        }

        public class ThongKeTinhTienAnTruaResult : ApiResultBaseDO
        {
            public List<ThongKeTinhTienAnTruaDataDO> data { get; set; }
        }

        public class ThongKeTinhTienCongTacDataDO
        {
            public int id { get; set; }
            public string fullName { get; set; }
            public string nickName { get; set; }
            public float total_CommissionDay_full { get; set; }
            public float total_CommissionDay_half { get; set; }
            public float total_CommissionPayment { get; set; }
        }

        public class ThongKeTinhTienCongTacResult : ApiResultBaseDO
        {
            public List<ThongKeTinhTienCongTacDataDO> data { get; set; }
        }

        public class ThongKeNgayNghiCaNhanDataDO
        {
            public int id { get; set; }
            public string fullName { get; set; }
            public string nickName { get; set; }
            public float userTotalDayOff_Month { get; set; }
        }

        public class ThongKeNgayNghiCaNhanResult : ApiResultBaseDO
        {
            public List<ThongKeNgayNghiCaNhanDataDO> data { get; set; }
        }

        public class ThongKeCaNhanNamHienTaiResult : ApiResultBaseDO
        {
            public List<ThongKeNghiPhepVaLamOnlineDO> data { get; set; }
        }

        public class ThongKeNghiPhepVaLamOnlineDO
        {
            public int memberId { get; set; }
            public string fullName { get; set; }
            public string role { get; set; }
            public int employeeType { get; set; }
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
}
