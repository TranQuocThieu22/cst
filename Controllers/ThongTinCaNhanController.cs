using educlient.Data;
using educlient.Models;
using Microsoft.AspNetCore.Mvc;
using System;
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
                data = tb.FindAll().ToArray(),
            };
        }
        [HttpGet, Route("{id}")]
        public DsThongTinCaNhanResult GetById(Guid id)
        {
            DsThongTinCaNhanResult returnData = new DsThongTinCaNhanResult();
            var tb = database.Table<DsThongTinCaNhanDataDO>();
            returnData.data.Append(tb.FindById(id));
            return new DsThongTinCaNhanResult
            {
                data = returnData.data,
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
                data = insertData.ToArray(),
            };
        }
        //[HttpPut, Route("{id}")]
        //public DsThongTinCaNhanResult update(Guid id, [FromBody] DsThongTinCaNhanInput inputData)
        //{
        //    var tb = database.Table<DsThongTinCaNhanDataDO>();

        //    var existingRecord = tb.FindById(id);
        //    if (existingRecord == null)
        //    {
        //        return new DsThongTinCaNhanResult
        //        {
        //            code = 404,
        //            message = "Data not found"
        //        };
        //    }
        //    // Update the existing record with new values
        //    existingRecord.NgayBatDauLamViec = DateTime.Today;
        //    existingRecord.SoLuongBuoi = inputData.SoLuongBuoi;
        //    existingRecord.HoTen = inputData.HoTen;
        //    existingRecord.PhongBan = inputData.PhongBan;
        //    existingRecord.AqUser = inputData.AqUser;
        //    existingRecord.TrangThai = inputData.TrangThai;
        //    existingRecord.email = inputData.email;
        //    existingRecord.ngaySinh = inputData.ngaySinh;
        //    existingRecord.IsLeader = inputData.IsLeader;

        //    // Update the record in the collection
        //    tb.Update(existingRecord);

        //    return new DsThongTinCaNhanResult
        //    {
        //        data = new DsThongTinCaNhanDataDO[] { existingRecord }
        //    };
        //}


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
                data = new DsThongTinCaNhanDataDO[] { existingRecord }
            };
        }
    }
}
