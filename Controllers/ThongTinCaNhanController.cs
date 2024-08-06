using educlient.Data;
using educlient.Models;
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
        // GET: NgayPhepChungController
        [HttpGet]
        public DsThongTinCaNhanResult GetAll()
        {
            var tb = database.Table<DsThongTinCaNhanDataDO>();

            return new DsThongTinCaNhanResult
            {
                data = tb.FindAll().ToList(),
            };
        }
        [HttpGet, Route("{id}")]
        public DsThongTinCaNhanResult GetById(Guid id)
        {
            List<DsThongTinCaNhanDataDO> returnData = new List<DsThongTinCaNhanDataDO>();
            var tb = database.Table<DsThongTinCaNhanDataDO>();
            var data = tb.FindById(id);
            if (data == null)
            {
                return new DsThongTinCaNhanResult
                {
                    code = 404,
                    message = "Data not found"
                };
            }
            returnData.Add(data);
            return new DsThongTinCaNhanResult
            {
                code = 200,
                result = true,
                data = returnData,
            };
        }
        // GET: NgayPhepChungController
        [HttpPost, Route("Insert")]
        public DsThongTinCaNhanResult insert([FromBody] DsThongTinCaNhanInput[] inputData)
        {
            var insertData = inputData.Select(input => new DsThongTinCaNhanDataDO
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
            var tb = database.Table<DsThongTinCaNhanDataDO>();
            tb.Insert(insertData);

            return new DsThongTinCaNhanResult
            {
                data = insertData.ToList(),
            };
        }
        [HttpPut, Route("{id}")]
        public DsThongTinCaNhanResult update(Guid id, [FromBody] DsThongTinCaNhanInput inputData)
        {
            var tb = database.Table<DsThongTinCaNhanDataDO>();

            var existingRecord = tb.FindById(id);
            if (existingRecord == null)
            {
                return new DsThongTinCaNhanResult
                {
                    code = 404,
                    message = "Data not found"
                };
            }
            // Update the existing record with new values
            // Update the existing record with new values
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
            tb.Update(existingRecord);

            return new DsThongTinCaNhanResult
            {
                data = new List<DsThongTinCaNhanDataDO> { existingRecord }
            };
        }


        [HttpDelete, Route("{id}")]
        public DsThongTinCaNhanResult delete(Guid id)
        {
            var tb = database.Table<DsThongTinCaNhanDataDO>();

            var existingRecord = tb.FindById(id);
            if (existingRecord == null)
            {
                return new DsThongTinCaNhanResult
                {
                    code = 404,
                    message = "Data not found"
                };
            }
            tb.Delete(id);

            return new DsThongTinCaNhanResult
            {
                data = new List<DsThongTinCaNhanDataDO> { existingRecord }
            };
        }
    }
}
