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
    public class NgayPhepCaNhanController : ControllerBase
    {
        private readonly IDbLiteContext database;
        public NgayPhepCaNhanController(IDbLiteContext dataContext)
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
            var tb = database.Table<DsNgayPhepCaNhanDO>().Include(x => x.ThongTinCaNhan);
            var tbThongTinCaNhan = database.Table<DsThongTinCaNhanDataDO>();

            var dataNgayPhep = tb.FindById(id);
            dataNgayPhep.ThongTinCaNhan = tbThongTinCaNhan.FindById(dataNgayPhep.ThongTinCaNhanId);
            var data = new List<DsNgayPhepCaNhanDO> { dataNgayPhep };

            return new DsNgayPhepCaNhanResult
            {
                data = new DsNgayPhepCaNhanDO[] { dataNgayPhep },
            };
        }
        // GET: NgayPhepChungController
        [HttpPost, Route("Insert")]
        public DsNgayPhepCaNhanResult Insert([FromBody] DsNgayPhepCaNhanInput[] inputData)
        {
            List<DsNgayPhepCaNhanDO> insertData = new List<DsNgayPhepCaNhanDO>();

            foreach (var input in inputData)
            {
                var newData = new DsNgayPhepCaNhanDO
                {
                    id = Guid.NewGuid(),
                    Ngay = DateTime.Today,
                    SoLuongBuoi = input.SoLuongBuoi,
                    LyDoNghi = input.LyDoNghi,
                    TrangThai = input.TrangThai,
                    UserNhap = input.UserNhap,
                    AqUser = input.AqUser,
                    ThongTinCaNhanId = input.ThongTinCaNhanId

                };
                insertData.Add(newData);
            }
            var tb = database.Table<DsNgayPhepCaNhanDO>();
            tb.InsertBulk(insertData); // Assuming there's a method for bulk insert in your database library

            // Return the list of inserted data
            return new DsNgayPhepCaNhanResult
            {
                data = insertData.ToArray()
            };
        }


        [HttpPut, Route("{id}")]
        public DsNgayPhepCaNhanResult Update(Guid id, [FromBody] DsNgayPhepCaNhanInput inputData)
        {
            var tb = database.Table<DsNgayPhepCaNhanDO>();

            // Find the existing record by id
            var existingRecord = tb.FindById(id);
            if (existingRecord == null)
            {
                return new DsNgayPhepCaNhanResult
                {
                    code = 404,
                    message = "Data not found"
                };
            }

            // Update the existing record with new values
            existingRecord.Ngay = DateTime.Today;
            existingRecord.SoLuongBuoi = inputData.SoLuongBuoi;
            existingRecord.LyDoNghi = inputData.LyDoNghi;
            existingRecord.TrangThai = inputData.TrangThai;
            existingRecord.UserNhap = inputData.UserNhap;
            existingRecord.AqUser = inputData.AqUser;
            existingRecord.ThongTinCaNhanId = inputData.ThongTinCaNhanId;

            // Update the record in the database
            tb.Update(existingRecord);

            // Return the updated record
            return new DsNgayPhepCaNhanResult
            {
                data = new DsNgayPhepCaNhanDO[] { existingRecord }
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
                    code = 404,
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
