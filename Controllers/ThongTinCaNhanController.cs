using educlient.Data;
using educlient.Models;
using LiteDB;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using static MongoDB.Bson.Serialization.Serializers.SerializerHelper;

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

            var NhanVienAQ = AQMemberTable.FindAll().ToList();

            var membersWithBase64 = NhanVienAQ.Select(member => new AQMemberInput
            {
                id = member.id,
                TFSName = member.TFSName,
                fullName = member.fullName,
                email = member.email,
                phone = member.phone,
                avatar = member.avatar != null ? $"data:image/png;base64,{Convert.ToBase64String(member.avatar)}" : null,
                birthDate = member.birthDate,
                startDate = member.startDate,
                nickName = member.nickName,
                role = member.role,
                isLeader = member.isLeader,
                isLunch = member.isLunch,
                WFHQuota = member.WFHQuota,
                absenceQuota = member.absenceQuota,
                isActive = member.isActive
            }).ToList();

            return new AQMembersResult
            {
                message = "Success",
                code = 200,
                result = true,
                data = membersWithBase64
            };
        }

        [HttpGet, Route("{id}")]
        public AQMembersResult GetById(int id)
        {
            List<AQMemberInput> returnData = new List<AQMemberInput>();

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

            var aqMemberReturn = new AQMemberInput
            {
                TFSName = aqMember.TFSName,
                fullName = aqMember.fullName,
                email = aqMember.email,
                phone = aqMember.phone,
                avatar = aqMember.avatar != null ? $"data:image/png;base64,{Convert.ToBase64String(aqMember.avatar)}" : null,
                birthDate = aqMember.birthDate,
                startDate = aqMember.startDate,
                nickName = aqMember.nickName,
                role = aqMember.role,
                isLeader = aqMember.isLeader,
                isLunch = aqMember.isLunch,
                WFHQuota = aqMember.WFHQuota,
                absenceQuota = aqMember.absenceQuota,
                isActive = aqMember.isActive
            };

            returnData.Add(aqMemberReturn);

            return new AQMembersResult
            {
                message = "Success",
                code = 200,
                result = true,
                data = returnData,
            };
        }

        [HttpPost]
        public ApiResultBaseDO Insert([FromBody] AQMemberInput[] inputData)
        {

            var aqMember = inputData.Select(input => new AQMember
            {
                TFSName = input.TFSName,
                fullName = input.fullName,
                email = input.email,
                phone = input.phone,
                avatar = !string.IsNullOrEmpty(input.avatar)
                ? Convert.FromBase64String(input.avatar.Substring(input.avatar.IndexOf(",") + 1))
                : null,
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

            return new ApiResultBaseDO
            {
                message = "Insert Success",
                code = 200,
                result = true
            };
        }

        [HttpPut, Route("{id}")]
        public ApiResultBaseDO Update(int id, [FromBody] AQMemberInput inputData)
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
            existingRecord.avatar = !string.IsNullOrEmpty(inputData.avatar)
                ? Convert.FromBase64String(inputData.avatar.Substring(inputData.avatar.IndexOf(",") + 1))
                : null;
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

            return new ApiResultBaseDO
            {
                message = "Delete Success",
                code = 200,
                result = true
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
                message = "Success",
                code = 200,
                result = true,
                data = result
            };
        }

        [HttpGet, Route("NhanVienTFSName")]
        public MemberTFSList GetMemberTFSName()
        {
            var AQMemberTable = database.Table<AQMember>();

            var result = AQMemberTable.Query()
                           .Select(x => new MemberTFS
                           {
                               id = x.id,
                               TFSName = x.TFSName,
                               fullName = x.fullName,
                               nickName = x.nickName
                           })
                           .ToList();

            return new MemberTFSList
            {
                message = "Success",
                code = 200,
                result = true,
                data = result
            };
        }
    }
    public class AQMembersResult : ApiResultBaseDO
    {
        public List<AQMemberInput> data { get; set; }
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

    public class MemberTFSList : ApiResultBaseDO
    {
        public List<MemberTFS> data { get; set; }
    }


    public class MemberTFS
    {
        public int id { get; set; }
        public string TFSName { get; set; }
        public string fullName { get; set; }
        public string nickName { get; set; }
    }


}



