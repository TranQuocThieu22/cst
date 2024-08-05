using educlient.Data;
using educlient.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace educlient.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NgayPhepChungController : ControllerBase
    {
        private readonly IDbLiteContext database;
        public NgayPhepChungController(IDbLiteContext dataContext)
        {
            database = dataContext;
        }
        // GET: NgayPhepChungController
        [HttpGet]
        public DsNgayPhepCaNhanResult GetAll()
        {
            var tb = database.Table<DsNgayPhepCaNhanDO>();
            return new DsNgayPhepCaNhanResult
            {
                data = tb.FindAll().ToArray(),
            };
        }
        [HttpGet, Route("{id}")]
        public DsNgayPhepCaNhanResult GetById(Guid id)
        {
            DsNgayPhepCaNhanResult returnData = new DsNgayPhepCaNhanResult();
            var tb = database.Table<DsNgayPhepCaNhanDO>();
            returnData.data.Append(tb.FindById(id));
            return new DsNgayPhepCaNhanResult
            {
                data = returnData.data,
            };
        }
        // GET: NgayPhepChungController
        [HttpPost, Route("Insert")]
        public DsNgayPhepCaNhanResult insert([FromBody] DsNgayPhepCaNhanInput[] inputData)
        {
            var insertData = inputData.Select(input => new DsNgayPhepCaNhanDO
            {
                Ngay = System.DateTime.Today,
                SoLuongBuoi = input.SoLuongBuoi,
                LyDoNghi = input.LyDoNghi,
                UserNhap = input.UserNhap,
                AqUser = input.AqUser,
                TrangThai = input.TrangThai,
            }).ToList();
            var tb = database.Table<DsNgayPhepCaNhanDO>();
            tb.Insert(insertData);
            var data = tb.FindAll();
            return new DsNgayPhepCaNhanResult
            {
                data = tb.FindAll().ToArray(),
            };
        }
        [HttpPut, Route("{id}")]
        public DsNgayPhepCaNhanResult update(Guid id, [FromBody] DsNgayPhepCaNhanInput inputData)
        {
            var tb = database.Table<DsNgayPhepCaNhanDO>();

            var existingRecord = tb.FindById(id);
            if (existingRecord == null)
            {
                new DsNgayPhepCaNhanResult
                {
                    code = 400,
                    message = "data not found",
                }; // Return 404 if the record does not exist
            }

            // Update the existing record with new values
            existingRecord.Ngay = DateTime.Today;
            existingRecord.SoLuongBuoi = inputData.SoLuongBuoi;
            existingRecord.LyDoNghi = inputData.LyDoNghi;
            existingRecord.UserNhap = inputData.UserNhap;
            existingRecord.AqUser = inputData.AqUser;
            existingRecord.TrangThai = inputData.TrangThai;


            // Update the record in the collection
            tb.Update(existingRecord);
            var data = tb.FindAll();
            return new DsNgayPhepCaNhanResult
            {
                data = tb.FindAll().ToArray(),
            };
        }
        [HttpDelete, Route("{id}")]
        public DsNgayPhepCaNhanResult delete(Guid id)
        {
            var tb = database.Table<DsNgayPhepCaNhanDO>();

            var existingRecord = tb.FindById(id);
            if (existingRecord == null)
            {
                new DsNgayPhepCaNhanResult
                {
                    code = 400,
                    message = "data not found",
                }; // Return 404 if the record does not exist
            }
            tb.Delete(id);
            var data = tb.FindAll();
            return new DsNgayPhepCaNhanResult
            {
                data = tb.FindAll().ToArray(),
            };
        }
    }
}
