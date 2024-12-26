using educlient.Data;
using LiteDB;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using static MongoDB.Bson.Serialization.Serializers.SerializerHelper;


namespace educlient.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ThongTinCaNhanController : ControllerBase
    {
        private static string HashPassword(string pwd)
        {
            if (string.IsNullOrEmpty(pwd)) return "";
            SHA256 sha = new SHA256Managed();
            var pwdBuff = Encoding.ASCII.GetBytes(pwd);
            var hashedPwd = sha.TransformFinalBlock(pwdBuff, 0, pwdBuff.Length);
            var hash = new StringBuilder();
            foreach (var b in sha.Hash)
            {
                hash.Append(string.Format("{0:x2}", b));
            }
            sha.Clear();
            return hash.ToString();
        }

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

            var memberList = NhanVienAQ.Select(member => new AQMemberDTO
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
                isLunchStatus = member.isLunchStatus,
                isActive = member.isActive,
                maSoCCCD = member.MaSoCCCD,
                address = member.address,
                workingYear = member.workingYear,
                contractStartDate = member.contractStartDate,
                contractExpireDate = member.contractExpireDate,
                contractType = member.contractType,
                minAbsenceQuota = member.minAbsenceQuota,
                additionalAbsenceQuota = member.additionalAbsenceQuota,
                minWFHQuota = member.minWFHQuota,
                additionalWFHQuota = member.additionalWFHQuota,
                employeeType = member.employeeType
            }).ToList();

            return new AQMembersResult
            {
                message = "Success",
                code = 200,
                result = true,
                data = memberList
            };
        }

        [HttpGet, Route("{id}")]
        public AQMembersResult GetById(int id)
        {
            List<AQMemberDTO> returnData = new List<AQMemberDTO>();

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

            var aqMemberReturn = new AQMemberDTO
            {
                id = aqMember.id,
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
                isLunchStatus = aqMember.isLunchStatus,
                isActive = aqMember.isActive,
                maSoCCCD = aqMember.MaSoCCCD,
                address = aqMember.address,
                workingYear = aqMember.workingYear,
                contractStartDate = aqMember.contractStartDate,
                contractExpireDate = aqMember.contractExpireDate,
                contractType = aqMember.contractType,
                minAbsenceQuota = aqMember.minAbsenceQuota,
                additionalAbsenceQuota = aqMember.additionalAbsenceQuota,
                minWFHQuota = aqMember.minWFHQuota,
                additionalWFHQuota = aqMember.additionalWFHQuota,
                employeeType = aqMember.employeeType
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
        public ApiResultBaseDO Insert([FromBody] AQMemberInsertDTO[] inputData)
        {

            var aqMembers = inputData.Select(input => new AQMember
            {
                TFSName = input.TFSName,
                password = HashPassword("1234"),
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
                isActive = input.isActive,
                isLunchStatus = input.isLunchStatus,
                address = input.address,
                MaSoCCCD = input.maSoCCCD,
                minAbsenceQuota = input.minAbsenceQuota,
                minWFHQuota = input.minWFHQuota,
                contractStartDate = input.contractStartDate,
                contractExpireDate = input.contractExpireDate,
                contractType = input.contractType,
                employeeType = input.employeeType
            }).ToList();

            if (aqMembers.Count == 1)
            {
                var AQMemberTable = database.Table<AQMember>();
                var newId = AQMemberTable.Insert(aqMembers[0]);
                var existingRecord = AQMemberTable.FindById(newId);
                var aqMember = new AQMemberDTO
                {
                    id = existingRecord.id,
                    TFSName = existingRecord.TFSName,
                    fullName = existingRecord.fullName,
                    email = existingRecord.email,
                    phone = existingRecord.phone,
                    avatar = existingRecord.avatar != null ? $"data:image/png;base64,{Convert.ToBase64String(existingRecord.avatar)}" : null,
                    birthDate = existingRecord.birthDate,
                    startDate = existingRecord.startDate,
                    nickName = existingRecord.nickName,
                    role = existingRecord.role,
                    isLeader = existingRecord.isLeader,
                    isLunchStatus = existingRecord.isLunchStatus,
                    isActive = existingRecord.isActive,
                    maSoCCCD = existingRecord.MaSoCCCD,
                    minAbsenceQuota = existingRecord.minAbsenceQuota,
                    minWFHQuota = existingRecord.minWFHQuota,
                    address = existingRecord.address,
                    workingYear = existingRecord.workingYear,
                    contractStartDate = existingRecord.contractStartDate,
                    contractExpireDate = existingRecord.contractExpireDate,
                    contractType = existingRecord.contractType,
                    employeeType = existingRecord.employeeType
                };

                var returnList = new List<AQMemberDTO>();

                returnList.Add(aqMember);

                return new InsertResultDTO
                {
                    message = "Insert Success",
                    code = 200,
                    result = true,
                    data = returnList,
                    numberOfNewRecord = 1
                };
            }
            else
            {
                var AQMemberTable = database.Table<AQMember>();
                var numberOfNewRecord = AQMemberTable.InsertBulk(aqMembers);

                return new InsertResultDTO
                {
                    //todo
                    message = "Insert Success",
                    code = 200,
                    result = true,
                    data = new List<AQMemberDTO>(),
                    numberOfNewRecord = numberOfNewRecord

                };
            }
        }

        [HttpPut, Route("{id}")]
        public ApiResultBaseDO Update(int id, [FromBody] AQMemberUpdateDTO inputData)
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
            existingRecord.isLunchStatus = inputData.isLunchStatus;
            existingRecord.minAbsenceQuota = inputData.minAbsenceQuota;
            existingRecord.minWFHQuota = inputData.minWFHQuota;
            existingRecord.isActive = inputData.isActive;
            existingRecord.address = inputData.address;
            existingRecord.MaSoCCCD = inputData.maSoCCCD;
            existingRecord.contractStartDate = inputData.contractStartDate;
            existingRecord.contractExpireDate = inputData.contractExpireDate;
            existingRecord.contractType = inputData.contractType;
            existingRecord.employeeType = inputData.employeeType;

            // Update the record in the collection
            AQMemberTable.Update(existingRecord);

            //return updated record
            var aqMemberReturn = new AQMemberDTO
            {
                id = existingRecord.id,
                TFSName = existingRecord.TFSName,
                fullName = existingRecord.fullName,
                email = existingRecord.email,
                phone = existingRecord.phone,
                avatar = existingRecord.avatar != null ? $"data:image/png;base64,{Convert.ToBase64String(existingRecord.avatar)}" : null,
                birthDate = existingRecord.birthDate,
                startDate = existingRecord.startDate,
                nickName = existingRecord.nickName,
                role = existingRecord.role,
                isLeader = existingRecord.isLeader,
                isLunchStatus = existingRecord.isLunchStatus,
                minAbsenceQuota = existingRecord.minAbsenceQuota,
                minWFHQuota = existingRecord.minWFHQuota,
                isActive = existingRecord.isActive,
                maSoCCCD = existingRecord.MaSoCCCD,
                address = existingRecord.address,
                workingYear = existingRecord.workingYear,
                contractStartDate = existingRecord.contractStartDate,
                contractExpireDate = existingRecord.contractExpireDate,
                contractType = existingRecord.contractType,
                employeeType = existingRecord.employeeType
            };

            return new UpdateResultDTO
            {
                message = "Update Success",
                code = 200,
                result = true,
                data = aqMemberReturn
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

            return new DeletetResultDTO
            {
                message = "Delete Success",
                code = 200,
                result = true,
                id = id
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

        [HttpPut, Route("avatar/{id}")]
        public ApiResultBaseDO UpdateAvatar(int id, [FromBody] UserAvatarDTO inputData)
        {
            var AQMemberTable = database.Table<AQMember>();
            var aqmember = AQMemberTable.FindById(id);
            aqmember.avatar = !string.IsNullOrEmpty(inputData.avatar)
                ? Convert.FromBase64String(inputData.avatar.Substring(inputData.avatar.IndexOf(",") + 1))
                : null;

            AQMemberTable.Update(aqmember);

            return new ResponeUpdateUserAvatar
            {
                message = "Update Avatar Success",
                code = 200,
                result = true,
                data = inputData.avatar
            };

        }

        [HttpPost, Route("AnnualAQDataStatus")]
        public ApiResultBaseDO CreateAnnualAQDataStatus([FromBody] AnnualAQDataStatusInput inputData)
        {
            var annualAQDataStatusTable = database.Table<AnnualAQDataStatus>();
            var annualAQDataStatus = annualAQDataStatusTable.Query().Where(x => x.year == inputData.year).FirstOrDefault();

            if (annualAQDataStatus == null)
            {
                annualAQDataStatus = new AnnualAQDataStatus
                {
                    year = inputData.year,
                    isSetup = false,
                    numberOfSetup = 0
                };
                annualAQDataStatusTable.Insert(annualAQDataStatus);
            }
            else
            {
                return new ApiResultBaseDO
                {
                    message = "Duplicate year input",
                    code = 200,
                    result = true
                };
            }

            return new ApiResultBaseDO
            {
                message = "Success",
                code = 200,
                result = true
            };
        }

        [HttpGet, Route("AnnualAQDataStatus")]
        public AnnualAQDataStatusResult GetAnnualAQDataStatus([FromQuery] int year)
        {
            var currentYear = DateTime.Now.Year;
            if (year < 2024 || year > currentYear)
            {
                return new AnnualAQDataStatusResult
                {
                    message = "No data",
                    code = 200,
                    result = true,
                };
            }

            var annualAQDataStatusTable = database.Table<AnnualAQDataStatus>();
            var annualAQDataStatus = annualAQDataStatusTable.Query().Where(x => x.year == year).FirstOrDefault();

            if (annualAQDataStatus == null)
            {
                return new AnnualAQDataStatusResult
                {
                    message = "Return new status instance",
                    code = 200,
                    result = true,
                    data = new AnnualAQDataStatus
                    {
                        year = year,
                        isSetup = false,
                        numberOfSetup = 0
                    }
                };
            }

            return new AnnualAQDataStatusResult
            {
                message = "Success",
                code = 200,
                result = true,
                data = annualAQDataStatus
            };
        }

        [HttpGet, Route("AnnualAQData")]
        public AQAnnualDataResult GetAnnualAQData([FromQuery] int year)
        {
            var currentYear = DateTime.Now.Year;
            if (year < 2024 || year > currentYear + 1)
            {
                return new AQAnnualDataResult
                {
                    message = "No data",
                    code = 200,
                    result = true,
                };
            }

            var AQMemberTable = database.Table<AQMember>();
            var annualAQDataStatusTable = database.Table<AnnualAQDataStatus>();

            var annualAQDataStatus = annualAQDataStatusTable.Query().Where(x => x.year == year).FirstOrDefault();
            if (annualAQDataStatus == null)
            {
                var NhanVienAQTempList = AQMemberTable.Query().Where(x => x.isActive == true).ToList();

                var dataList_previousYear = NhanVienAQTempList.Select(member => new MemberAnnualData
                {
                    id = member.id,
                    fullName = member.fullName,
                    isActive = member.isActive,
                    workingYear = member.workingYear,
                    minAbsenceQuota = member.minAbsenceQuota,
                    additionalAbsenceQuota = member.additionalAbsenceQuota,
                    minWFHQuota = member.minWFHQuota,
                    additionalWFHQuota = member.additionalWFHQuota
                }).ToList();

                return new AQAnnualDataResult
                {
                    message = "Success",
                    code = 200,
                    result = true,
                    data = new AQAnnualData
                    {
                        year = year,
                        memberAnnualDataList = dataList_previousYear
                    }
                };
            }

            var NhanVienAQ = AQMemberTable.Query().Where(x => x.isActive == true).ToList();

            var dataList = NhanVienAQ.Select(member => new MemberAnnualData
            {
                id = member.id,
                fullName = member.fullName,
                isActive = member.isActive,
                workingYear = member.workingYear,
                minAbsenceQuota = member.minAbsenceQuota,
                additionalAbsenceQuota = member.additionalAbsenceQuota,
                minWFHQuota = member.minWFHQuota,
                additionalWFHQuota = member.additionalWFHQuota
            }).ToList();

            return new AQAnnualDataResult
            {
                message = "Success",
                code = 200,
                result = true,
                data = new AQAnnualData
                {
                    year = year,
                    memberAnnualDataList = dataList
                }
            };

        }

        [HttpPatch, Route("AnnualAQData")]
        public ApiResultBaseDO UpdateAnnualAQData([FromBody] AQAnnualDataInput inputData)
        {
            var currentYear = inputData.year;
            var existingAnnualAQDataStatus = database.Table<AnnualAQDataStatus>().FindOne(x => x.year == currentYear);
            var annualAQDataStatusTable = database.Table<AnnualAQDataStatus>();

            if (existingAnnualAQDataStatus == null)
            {
                var newAnnualAQDataStatus = new AnnualAQDataStatus
                {
                    year = currentYear,
                    isSetup = true,
                    numberOfSetup = 1
                };
                annualAQDataStatusTable.Insert(newAnnualAQDataStatus);

                foreach (var member in inputData.memberAnnualDataList)
                {
                    // Find the corresponding member in the AQMember table
                    var existingMember = database.Table<AQMember>().FindOne(x => x.id == member.id);

                    if (existingMember != null)
                    {
                        // Update the fields of the member
                        existingMember.workingYear = member.workingYear;
                        existingMember.additionalWFHQuota = member.additionalWFHQuota;
                        existingMember.additionalAbsenceQuota = member.additionalAbsenceQuota;
                        existingMember.isLunchStatus = member.isLunchStatus;

                        // Save the changes to the database
                        database.Table<AQMember>().Update(existingMember);
                    }
                }
            }

            else
            {
                foreach (var member in inputData.memberAnnualDataList)
                {
                    // Find the corresponding member in the AQMember table
                    var existingMember = database.Table<AQMember>().FindOne(x => x.id == member.id);

                    if (existingMember != null)
                    {
                        // Update the fields of the member
                        existingMember.workingYear = member.workingYear;
                        existingMember.additionalWFHQuota = member.additionalWFHQuota;
                        existingMember.additionalAbsenceQuota = member.additionalAbsenceQuota;
                        existingMember.isLunchStatus = member.isLunchStatus;

                        // Save the changes to the database
                        database.Table<AQMember>().Update(existingMember);
                    }
                }
            }

            existingAnnualAQDataStatus.isSetup = true;
            existingAnnualAQDataStatus.numberOfSetup += 1;
            database.Table<AnnualAQDataStatus>().Update(existingAnnualAQDataStatus);
            return new ApiResultBaseDO
            {
                message = "Update Success",
                code = 200,
                result = true
            };

        }


        [HttpGet, Route("HanMucNghiPhepNam")]
        public IndividualDayOffDetailDO GetDetailAbsenceQuota([FromQuery] int userId, [FromQuery] int year)
        {
            var AQMemberTable = database.Table<AQMember>();
            var aqmember = AQMemberTable.FindById(userId);
            //var absenceQuota = aqmember.detailAbsenceQuota.actualAbsenceQuotaByYear.FirstOrDefault(x => x.year == year);

            var absenceQuota = new absenceQuota
            {
                minAbsenceQuota = aqmember.minAbsenceQuota,
                additionalAbsenceQuota = aqmember.additionalAbsenceQuota
            };

            return new IndividualDayOffDetailDO
            {
                message = "Success",
                code = 200,
                result = true,
                data = absenceQuota
            };
        }

        [HttpGet, Route("SL_HopDongSapHetHan")]
        public CountNearExpiredContract GetCountNearExpiredContract()
        {
            DateTime today = DateTime.Today;
            DateTime thirtyDaysFromNow = today.AddDays(30);
            var AQMemberTable = database.Table<AQMember>();

            var memberList = AQMemberTable.Query().Where(
                x => x.isActive == true &&
                x.contractExpireDate >= today && x.contractExpireDate <= thirtyDaysFromNow
                ).ToList();

            return new CountNearExpiredContract
            {
                message = "Success",
                code = 200,
                result = true,
                data = memberList.Count
            };

        }

        [HttpGet, Route("HopDongSapHetHan")]
        public AQMembersResult GetListNearExpiredContract()
        {
            DateTime today = DateTime.Today;
            DateTime thirtyDaysFromNow = today.AddDays(31);
            var AQMemberTable = database.Table<AQMember>();

            var memberList = AQMemberTable.Query().Where(
                x => x.isActive == true &&
                x.contractExpireDate >= today && x.contractExpireDate <= thirtyDaysFromNow
                ).ToList();

            var memberReturnList = memberList.Select(member => new AQMemberDTO
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
                isLunchStatus = member.isLunchStatus,
                minAbsenceQuota = member.minAbsenceQuota,
                minWFHQuota = member.minWFHQuota,
                isActive = member.isActive,
                maSoCCCD = member.MaSoCCCD,
                address = member.address,
                workingYear = member.workingYear,
                contractStartDate = member.contractStartDate,
                contractExpireDate = member.contractExpireDate,
                contractType = member.contractType,
                employeeType = member.employeeType
            }).ToList();


            return new AQMembersResult
            {
                message = "Success",
                code = 200,
                result = true,
                data = memberReturnList
            };
        }
    }


}

public class detailInput
{
    public int userId { get; set; }
    public int year { get; set; }
    public int data { get; set; }
}
//public class detailAbsenceQuotaDO : ApiResultBaseDO
//{
//    public actualAbsenceQuotaByYear data { get; set; }
//}
//public class detailWFHQuotaDO : ApiResultBaseDO
//{
//    public actualWFHQuotaByYear data { get; set; }
//}
//public class detailLunchDataDO : ApiResultBaseDO
//{
//    public detailLunch data { get; set; }
//}
public class AQMembersResult : ApiResultBaseDO
{
    public List<AQMemberDTO> data { get; set; }
}

public class MemberCommissionList : ApiResultBaseDO
{
    public List<MemberCommission> data { get; set; }
}

public class AQMemberInsertDTO
{
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
    public bool isLunchStatus { get; set; }
    public bool isActive { get; set; }
    public string maSoCCCD { get; set; }
    public string address { get; set; }
    public int minWFHQuota { get; set; }
    public int minAbsenceQuota { get; set; }
    public DateTime? contractStartDate { get; set; } = null;
    public DateTime? contractExpireDate { get; set; } = null;
    public string contractType { get; set; }
    public int employeeType { get; set; }
}

public class InsertResultDTO : ApiResultBaseDO
{
    public List<AQMemberDTO> data { get; set; }
    public int numberOfNewRecord { get; set; }
}

public class AQMemberUpdateDTO
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
    public bool isLunchStatus { get; set; }
    public int minWFHQuota { get; set; }
    public int minAbsenceQuota { get; set; }
    public bool isActive { get; set; }
    public string maSoCCCD { get; set; }
    public string address { get; set; }
    public DateTime? contractStartDate { get; set; } = null;
    public DateTime? contractExpireDate { get; set; } = null;
    public string contractType { get; set; }
    public int employeeType { get; set; }
}

public class UpdateResultDTO : ApiResultBaseDO
{
    public AQMemberDTO data { get; set; }
}

public class DeletetResultDTO : ApiResultBaseDO
{
    public int id { get; set; }
}

public class AQMemberDTO
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
    public bool isLunchStatus { get; set; }
    public int workingYear { get; set; }
    public int minWFHQuota { get; set; }
    public int additionalWFHQuota { get; set; }
    public int minAbsenceQuota { get; set; }
    public int additionalAbsenceQuota { get; set; }
    public bool isActive { get; set; }
    public string maSoCCCD { get; set; }
    public string address { get; set; }
    public DateTime? contractStartDate { get; set; } = null;
    public DateTime? contractExpireDate { get; set; } = null;
    public string contractType { get; set; }
    public int employeeType { get; set; }
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

public class UserAvatarDTO
{
    public int id { get; set; }
    public string avatar { get; set; }
}

public class ResponeUpdateUserAvatar : ApiResultBaseDO
{
    public string data { get; set; }
}

public class IndividualDayOffDetailDO : ApiResultBaseDO
{
    public absenceQuota data { get; set; }
}

public class absenceQuota
{
    public int minAbsenceQuota { get; set; }
    public int additionalAbsenceQuota { get; set; }
}


public class AnnualAQDataStatusInput
{
    public int year { get; set; }
}

public class AnnualAQDataStatusResult : ApiResultBaseDO
{
    public AnnualAQDataStatus data { get; set; }
}

public class AQAnnualDataResult : ApiResultBaseDO
{
    public AQAnnualData data { get; set; }
}

public class CountNearExpiredContract : ApiResultBaseDO
{
    public int data { get; set; }
}

public class AQAnnualData
{
    public int year { get; set; }
    public List<MemberAnnualData> memberAnnualDataList { get; set; }

}

public class MemberAnnualData
{
    public int id { get; set; }
    public string fullName { get; set; }
    public bool isActive { get; set; }
    public int workingYear { get; set; }
    public int minWFHQuota { get; set; }
    public int additionalWFHQuota { get; set; }
    public int minAbsenceQuota { get; set; }
    public int additionalAbsenceQuota { get; set; }
}

public class AQAnnualDataInput
{
    public int year { get; set; }
    public int numberOfSetup { get; set; }
    public List<MemberAnnualDataInput> memberAnnualDataList { get; set; }
}

public class MemberAnnualDataInput
{
    public int id { get; set; }
    public int workingYear { get; set; }
    public int additionalWFHQuota { get; set; }
    public int additionalAbsenceQuota { get; set; }
    public bool isLunchStatus { get; set; }
}


