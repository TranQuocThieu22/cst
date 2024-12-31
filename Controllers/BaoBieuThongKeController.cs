using educlient.Data;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

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
            int total_IndividualDayOff = 0;
            int total_WorkingOnline = 0;
            int total_IndividualDayOff_full = 0;
            int total_IndividualDayOff_half = 0;
            int total_WorkingOnline_full = 0;
            int total_WorkingOnline_half = 0;
            int total_CommissionDay_full = 0;
            int total_CommissionDay_half = 0;
            int total_AQDayOff = 0;

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

                total_IndividualDayOff = total_IndividualDayOff_full + total_IndividualDayOff_half;

                // Find wfh data for each member by month-year
                total_WorkingOnline_full = workingOnlineTable.Find(x =>
                    x.memberId == member.id &&
                    x.date.Year == year &&
                    x.date.Month == month &&
                    x.approvalStatus == "Đã duyệt" &&
                    x.periodType == 1
                    ).ToList().Count;

                total_WorkingOnline_half = workingOnlineTable.Find(x =>
                    x.memberId == member.id &&
                    x.date.Year == year &&
                    x.date.Month == month &&
                    x.approvalStatus == "Đã duyệt" &&
                    x.periodType == 2 || x.periodType == 3
                    ).ToList().Count;

                total_WorkingOnline = total_WorkingOnline_full + total_WorkingOnline_half;

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
                        else if (data.sumDay > 0 && data.sumDay < 1)  // Fractional day condition
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


                var AQDayOffData = aqDayOffTable.Find(x =>
                    x.dateFrom.Year == year &&
                    x.dateFrom.Month == month
                    ).ToList();

                foreach (var AQDayOff in AQDayOffData)
                {
                    total_AQDayOff += (int)AQDayOff.sumDay;
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
                    total_AQDayOff = total_AQDayOff
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

        [HttpGet("ThongKeTinhTienCongTac")]
        public ThongKeTinhTienCongTacResult ThongKeTinhTienCongTac([FromQuery] DateTime? query_dateFrom = null, [FromQuery] DateTime? query_dateTo = null, int? year = null)
        {
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
                var commissionData = commissionTable.Find(x =>
                    x.memberList.Where(m => m.id == member.id).Any() &&
                    ((x.dateFrom >= query_dateFrom.Value && x.dateTo <= query_dateTo.Value)) &&
                    x.dateFrom.Year == year
                    ).ToList();

                float countCommission = 0;
                var totalExpense = 0;

                if (commissionData.Any(d => d.memberList.Any(m => m.id == member.id)))
                {
                    // Execute only for members with a commission
                    commissionData.ForEach(d =>
                    {
                        countCommission += d.sumDay;
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
                    total_CommissionDay = countCommission,
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

        public class ThongKeTinhTienAnTruaDataDO
        {
            public int id { get; set; }
            public string fullName { get; set; }
            public string nickName { get; set; }
            public int employeeType { get; set; }
            public int total_IndividualDayOff { get; set; }
            public int total_WorkingOnline { get; set; }
            public int total_IndividualDayOff_full { get; set; }
            public int total_IndividualDayOff_half { get; set; }
            public int total_WorkingOnline_full { get; set; }
            public int total_WorkingOnline_half { get; set; }
            public int total_CommissionDay_full { get; set; }
            public int total_CommissionDay_half { get; set; }
            public int total_AQDayOff { get; set; }
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
            public float total_CommissionDay { get; set; }
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

    }
}
