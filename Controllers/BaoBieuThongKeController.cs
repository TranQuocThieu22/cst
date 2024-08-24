﻿using educlient.Data;
using LiteDB;
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
        private readonly IDbLiteContext database;
        public BaoBieuThongKeController(IDbLiteContext dataContext)
        {
            database = dataContext;
        }

        [HttpGet("ThongKeTinhTienAnTrua")]
        public ThongKeTinhTienAnTruaResult ThongKeTinhTienAnTrua([FromQuery] int year, [FromQuery] int month, [FromQuery] int? query_memberId = null)
        {
            // Get the tables
            var membersTable = database.Table<AQMember>();
            var dayOffsTable = database.Table<IndividualDayOff>();
            var workingOnlineTable = database.Table<WorkingOnlineDataDO>();
            var commissionTable = database.Table<Commission>();
            var aqDayOffTable = database.Table<DayOff>();

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
            var resultList = new List<ThongKeTinhTienAnTruaDataDO>();
            foreach (var member in membersData)
            {
                // Find day-off data for each member by year
                var dayOffData = dayOffsTable.Find(x =>
                    x.memberId == member.id &&
                    x.dateFrom.Year == year &&
                    x.dateFrom.Month == month &&
                    x.approvalStatus == "Đã duyệt" &&
                    x.sumDay > 0.5
                    ).ToList();

                var countDayOff = 0;
                foreach (var dayOff in dayOffData)
                {
                    countDayOff += (int)dayOff.sumDay;
                }

                var wfhData = workingOnlineTable.Find(x =>
                    x.memberId == member.id &&
                    x.dateFrom.Year == year &&
                    x.dateFrom.Month == month &&
                    x.approvalStatus == "Đã duyệt" &&
                    x.sumDay > 0
                    ).ToList();

                var countWorkingOnline = 0;
                foreach (var wfhday in wfhData)
                {
                    countWorkingOnline += (int)wfhday.sumDay;
                }

                var commissionData = commissionTable.Find(x =>
                    x.memberList.Where(m => m.id == member.id).Any() &&
                    x.dateFrom.Year == year &&
                    x.dateFrom.Month == month
                    ).ToList();

                float countCommission = 0;
                commissionData.ForEach(d => countCommission += d.sumDay);

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
                var resultData = new ThongKeTinhTienAnTruaDataDO
                {
                    id = member.id,
                    fullName = member.fullName,
                    nickName = member.nickName,
                    total_IndividualDayOff = countDayOff,
                    total_WorkingOnline = countWorkingOnline,
                    total_CommissionDay = countCommission,
                    total_AQDayOff = countaqDayOff
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
            var membersTable = database.Table<AQMember>();
            var commissionTable = database.Table<Commission>();

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
                commissionData.ForEach(d => countCommission += d.sumDay);

                var totalExpense = 0;
                commissionData.ForEach(d => totalExpense += d.commissionExpenses);


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

        public class ThongKeTinhTienAnTruaDataDO
        {
            public int id { get; set; }
            public string fullName { get; set; }
            public string nickName { get; set; }
            public float total_IndividualDayOff { get; set; }
            public float total_WorkingOnline { get; set; }
            public float total_CommissionDay { get; set; }
            public float total_AQDayOff { get; set; }
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

    }
}
