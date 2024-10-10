﻿using educlient.Data;
using educlient.Models;
using LiteDB;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using static MongoDB.Bson.Serialization.Serializers.SerializerHelper;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;


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
                detailLunch = member.detailLunch,
                detailWFHQuota = member.detailWFHQuota,
                detailAbsenceQuota = member.detailAbsenceQuota,
                isActive = member.isActive,
                maSoCCCD = member.MaSoCCCD,
                address = member.address,
                workingYear = member.workingYear,
                detailContract = member.detailContract
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
                detailLunch = aqMember.detailLunch,
                detailWFHQuota = aqMember.detailWFHQuota,
                detailAbsenceQuota = aqMember.detailAbsenceQuota,
                isActive = aqMember.isActive,
                maSoCCCD = aqMember.MaSoCCCD,
                address = aqMember.address,
                workingYear = aqMember.workingYear,
                detailContract = aqMember.detailContract
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
                isLunchStatus = input.isLunchStatus,
                isActive = input.isActive,
                address = input.address,
                MaSoCCCD = input.maSoCCCD,
                workingYear = 0,
                detailContract = new detailContract
                {
                    contractStartDate = input.detailContract.contractStartDate,
                    contractExpireDate = input.detailContract.contractExpireDate,
                    contractDuration = input.detailContract.contractDuration,
                    contractType = input.detailContract.contractType,
                },
                detailAbsenceQuota = new detailAbsenceQuota
                {
                    minAbsenceQuota = input.minAbsenceQuota,
                    actualAbsenceQuotaByYear = new List<actualAbsenceQuotaByYear> {
                        new actualAbsenceQuotaByYear
                        {
                            year = DateTime.Now.Year,
                            absenceQuota = input.minAbsenceQuota
                        }
                    }
                },
                detailWFHQuota = new detailWFHQuota
                {
                    minWFHQuota = input.minWFHQuota,
                    actualWFHQuotaByYear = new List<actualWFHQuotaByYear>
                    {
                        new actualWFHQuotaByYear
                        {
                            year = DateTime.Now.Year,
                            WFHQuota = input.minWFHQuota
                        }
                    }
                },
                detailLunch = input.isLunchStatus == true ? new List<detailLunch> {
                    new detailLunch
                    {
                        year = DateTime.Now.Year,
                        lunchByMonth = Enumerable.Range(DateTime.Now.Month, 12 - DateTime.Now.Month + 1)
                            .Select(month => new lunchByMonth
                            {
                                month = month,
                                isLunch = true
                            })
                            .ToList()
                    }
                }
                :
                new List<detailLunch> {
                    new detailLunch
                    {
                        year = DateTime.Now.Year,
                        lunchByMonth = Enumerable.Range(DateTime.Now.Month, 12 - DateTime.Now.Month + 1)
                            .Select(month => new lunchByMonth
                            {
                                month = month,
                                isLunch = false,
                                note = ""

                            })
                            .ToList()
                    }
                }
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
                    detailLunch = existingRecord.detailLunch,
                    detailWFHQuota = existingRecord.detailWFHQuota,
                    detailAbsenceQuota = existingRecord.detailAbsenceQuota,
                    isActive = existingRecord.isActive,
                    maSoCCCD = existingRecord.MaSoCCCD,
                    address = existingRecord.address,
                    workingYear = existingRecord.workingYear,
                    detailContract = existingRecord.detailContract
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
            existingRecord.detailLunch = inputData.detailLunch;
            existingRecord.detailWFHQuota = inputData.detailWFHQuota;
            existingRecord.detailAbsenceQuota = inputData.detailAbsenceQuota;
            existingRecord.isActive = inputData.isActive;
            existingRecord.address = inputData.address;
            existingRecord.MaSoCCCD = inputData.maSoCCCD;
            existingRecord.workingYear = inputData.workingYear;
            existingRecord.detailContract = inputData.detailContract;

            // Update the record in the collection
            AQMemberTable.Update(existingRecord);

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
                detailLunch = existingRecord.detailLunch,
                detailWFHQuota = existingRecord.detailWFHQuota,
                detailAbsenceQuota = existingRecord.detailAbsenceQuota,
                isActive = existingRecord.isActive,
                maSoCCCD = existingRecord.MaSoCCCD,
                address = existingRecord.address,
                workingYear = existingRecord.workingYear,
                detailContract = existingRecord.detailContract
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
            if (year < 2024 || year >= 2026)
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
            if (year < 2024 || year >= 2026)
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
                    absenceQuotaBase = member.detailAbsenceQuota.minAbsenceQuota,
                    absenceQuotaBaseCurrent = member.detailAbsenceQuota.actualAbsenceQuotaByYear.FirstOrDefault(x => x.year == year - 1).absenceQuota,
                    wfhQuotaBase = member.detailWFHQuota.minWFHQuota,
                    wfhQuotaBaseCurrent = member.detailWFHQuota.actualWFHQuotaByYear.FirstOrDefault(x => x.year == year - 1).WFHQuota,
                    lunchPayment = member.detailLunch.FirstOrDefault(x => x.year == year - 1).lunchByMonth.FirstOrDefault(x => x.month == 12).isLunch ? member.detailLunch.FirstOrDefault(x => x.year == year - 1).lunchByMonth.FirstOrDefault(x => x.month == 12).lunchFee : 0,
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
                absenceQuotaBase = member.detailAbsenceQuota.minAbsenceQuota,
                absenceQuotaBaseCurrent = member.detailAbsenceQuota.actualAbsenceQuotaByYear.FirstOrDefault(x => x.year == year).absenceQuota,
                wfhQuotaBase = member.detailWFHQuota.minWFHQuota,
                wfhQuotaBaseCurrent = member.detailWFHQuota.actualWFHQuotaByYear.FirstOrDefault(x => x.year == year).WFHQuota,
                lunchPayment = member.detailLunch.FirstOrDefault(x => x.year == year).lunchByMonth.FirstOrDefault(x => x.month == 12).isLunch ? member.detailLunch.FirstOrDefault(x => x.year == year).lunchByMonth.FirstOrDefault(x => x.month == 12).lunchFee : 0,
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


        [HttpGet, Route("HanMucNghiPhepNam")]
        public IndividualDayOffDetailDO GetDetailAbsenceQuota([FromQuery] int userId, [FromQuery] int year)
        {
            var AQMemberTable = database.Table<AQMember>();
            var aqmember = AQMemberTable.FindById(userId);
            var absenceQuota = aqmember.detailAbsenceQuota.actualAbsenceQuotaByYear.FirstOrDefault(x => x.year == year);
            return new IndividualDayOffDetailDO
            {
                message = "Success",
                code = 200,
                result = true,
                data = absenceQuota
            };
        }
    }


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
        public int lunchFee { get; set; }
        public int minWFHQuota { get; set; }
        public int minAbsenceQuota { get; set; }
        public bool isActive { get; set; }
        public string maSoCCCD { get; set; }
        public string address { get; set; }
        public int workingYear { get; set; }
        public detailContract detailContract { get; set; }
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
        public List<detailLunch> detailLunch { get; set; }
        public detailWFHQuota detailWFHQuota { get; set; }
        public detailAbsenceQuota detailAbsenceQuota { get; set; }
        public bool isActive { get; set; }
        public string maSoCCCD { get; set; }
        public string address { get; set; }
        public int workingYear { get; set; }
        public detailContract detailContract { get; set; }
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
        public List<detailLunch> detailLunch { get; set; }
        public int workingYear { get; set; }
        public detailWFHQuota detailWFHQuota { get; set; }
        public detailAbsenceQuota detailAbsenceQuota { get; set; }
        public bool isActive { get; set; }
        public string maSoCCCD { get; set; }
        public string address { get; set; }
        public detailContract detailContract { get; set; }
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
        public actualAbsenceQuotaByYear data { get; set; }
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
        public int absenceQuotaBase { get; set; }
        public int absenceQuotaBaseCurrent { get; set; }
        public int wfhQuotaBase { get; set; }
        public int wfhQuotaBaseCurrent { get; set; }
        public int lunchPayment { get; set; }

    }

    public class AQAnnualDataInput
    {
        public int year { get; set; }
        public int numberOfSetup { get; set; }
        public List<MemberAnnualData> memberAnnualDataList { get; set; }
    }

    public class MemberAnnualDataInput
    {
        public int id { get; set; }

        public int workingYear { get; set; }
        public int absenceQuotaBaseCurrent { get; set; }
        public int wfhQuotaBaseCurrent { get; set; }

        public int lunchPayment { get; set; }
    }


}