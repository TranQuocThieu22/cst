using educlient.Data;
using educlient.Models;
using LiteDB;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace educlient.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ThongTinCaNhanController : ControllerBase
    {
        private readonly IDbLiteContext database;
        public ThongTinCaNhanController(IDbLiteContext dataContext)
        {
            database = dataContext;
        }

        [HttpGet]
        public AQMembersResult GetAll()
        {
            var AQMemberTable = database.Table<AQMember>();

            var NhanVienAQ = AQMemberTable.FindAll();

            return new AQMembersResult
            {
                data = NhanVienAQ.ToList()
            };
        }

        [HttpGet, Route("{id}")]
        public AQMembersResult GetById(int id)
        {
            List<AQMember> returnData = new List<AQMember>();

            var AQMemberTable = database.Table<AQMember>();

            var aqMember = AQMemberTable.FindById(id);
            if (aqMember == null)
            {
                return new AQMembersResult
                {
                    code = 404,
                    message = "Data not found"
                };
            }
            returnData.Add(aqMember);

            return new AQMembersResult
            {
                code = 200,
                result = true,
                data = returnData,
            };
        }

        [HttpPost]
        public AQMembersResult Insert([FromBody] AQMemberInput[] inputData)
        {

            var aqMember = inputData.Select(input => new AQMember
            {
                TFSName = input.TFSName,
                fullName = input.fullName,
                email = input.email,
                phone = input.phone,
                avatar = input.avatar,
                birthDate = input.birthDate,
                startDate = input.startDate,
                nickName = input.nickName,
                role = input.role,
                isLeader = input.isLeader,
                isLunch = input.isLunch,
                WFHQuota = input.WFHQuota,
                absenceQuota = input.absenceQuota,
                isActive = input.isActive

            }).ToList();

            var AQMemberTable = database.Table<AQMember>();
            AQMemberTable.Insert(aqMember);
            var NhanVienAQ = AQMemberTable.FindAll();

            return new AQMembersResult
            {
                data = NhanVienAQ.ToList()
            };
        }

        [HttpPut, Route("{id}")]
        public AQMembersResult Update(int id, [FromBody] AQMemberInput inputData)
        {
            var AQMemberTable = database.Table<AQMember>();

            var existingRecord = AQMemberTable.FindById(id);
            if (existingRecord == null)
            {
                return new AQMembersResult
                {
                    code = 404,
                    message = "Data not found"
                };
            }

            existingRecord.TFSName = inputData.TFSName;
            existingRecord.fullName = inputData.fullName;
            existingRecord.email = inputData.email;
            existingRecord.phone = inputData.phone;
            existingRecord.avatar = inputData.avatar;
            existingRecord.birthDate = inputData.birthDate;
            existingRecord.startDate = inputData.startDate;
            existingRecord.nickName = inputData.nickName;
            existingRecord.role = inputData.role;
            existingRecord.isLeader = inputData.isLeader;
            existingRecord.isLunch = inputData.isLunch;
            existingRecord.WFHQuota = inputData.WFHQuota;
            existingRecord.absenceQuota = inputData.absenceQuota;
            existingRecord.isActive = inputData.isActive;


            // Update the record in the collection
            AQMemberTable.Update(existingRecord);
            var NhanVienAQ = AQMemberTable.FindAll();

            return new AQMembersResult
            {
                data = NhanVienAQ.ToList()
            };
        }


        [HttpDelete, Route("{id}")]
        public AQMembersResult Delete(int id)
        {
            var AQMemberTable = database.Table<AQMember>();

            var existingRecord = AQMemberTable.FindById(id);
            if (existingRecord == null)
            {
                return new AQMembersResult
                {
                    code = 404,
                    message = "Data not found"
                };
            }
            AQMemberTable.Delete(id);

            return new AQMembersResult
            {
                data = new List<AQMember> { existingRecord }
            };
        }

        [HttpGet, Route("NhanVienCongTac")]
        public MemberCommissionList GetMemberCommissionList()
        {
            var AQMemberTable = database.Table<AQMember>();

            var result = AQMemberTable.Query()
                           .Select(x => new MemberCommission
                           {
                               id = x.id,
                               fullName = x.fullName,
                               nickName = x.nickName
                           })
                           .ToList();

            return new MemberCommissionList
            {
                data = result
            };
        }
    }

    public class AQMembersResult : ApiResultBaseDO
    {
        public List<AQMember> data { get; set; }
    }

    public class MemberCommissionList : ApiResultBaseDO
    {
        public List<MemberCommission> data { get; set; }
    }

    public class AQMemberInput
    {
        public int id { get; set; }
        public string TFSName { get; set; }
        public string fullName { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string avatar { get; set; }
        public DateTime birthDate { get; set; }
        public DateTime startDate { get; set; }
        public string nickName { get; set; }
        public string role { get; set; }
        public bool isLeader { get; set; }
        public bool isLunch { get; set; }
        public int WFHQuota { get; set; }
        public int absenceQuota { get; set; }
        public bool isActive { get; set; }
    }

    public class MemberCommission
    {
        public int id { get; set; }
        public string fullName { get; set; }
        public string nickName { get; set; }
    }
}
