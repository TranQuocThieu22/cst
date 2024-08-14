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
    public class NgayCongTacController : Controller
    {
        private readonly IDbLiteContext database;
        public NgayCongTacController(IDbLiteContext dataContext)
        {
            database = dataContext;
        }
        // GET: NgayPhepChungController
        [HttpGet]
        public DsNgayCongTacResult GetAll()
        {
            var tb = database.Table<DsNgayCongTacDO>();
            return new DsNgayCongTacResult
            {
                data = tb.FindAll().ToArray(),
            };
        }
        [HttpGet, Route("{id}")]
        public DsNgayCongTacResult GetById(Guid id)
        {
            DsNgayCongTacResult returnData = new DsNgayCongTacResult();
            var tb = database.Table<DsNgayCongTacDO>();
            returnData.data.Append(tb.FindById(id));
            return new DsNgayCongTacResult
            {
                data = returnData.data,
            };
        }
        [HttpPost, Route("Insert")]
        public DsNgayCongTacResult Insert([FromBody] DsNgayCongTacInput[] inputData)
        {
            List<DsNgayCongTacDO> insertData = new List<DsNgayCongTacDO>();

            foreach (var input in inputData)
            {
                //var newData = new DsNgayCongTacDO
                //{
                //    id = Guid.NewGuid(),
                //    Ngay = DateTime.Today,
                //    SoLuongBuoi = input.SoLuongBuoi,
                //    NoiDungCongTac = input.NoiDungCongTac,
                //    TruongCongTac = input.TruongCongTac,
                //    UserNhap = input.UserNhap,
                //    AqUser = input.AqUser
                //};

                //insertData.Add(newData);
            }

            var tb = database.Table<DsNgayCongTacDO>();
            tb.InsertBulk(insertData); // Assuming there's a method for bulk insert in your database library

            // Return the list of inserted data
            return new DsNgayCongTacResult
            {
                data = insertData.ToArray()
            };
        }

        [HttpPut, Route("{id}")]
        public DsNgayCongTacResult Update(Guid id, [FromBody] DsNgayCongTacInput inputData)
        {
            var tb = database.Table<DsNgayCongTacDO>();

            // Find the existing record by id
            var existingRecord = tb.FindById(id);
            if (existingRecord == null)
            {
                return new DsNgayCongTacResult
                {
                    code = 404,
                    message = "Data not found"
                };
            }

            // Update the existing record with new values
            //existingRecord.Ngay = DateTime.Today;
            //existingRecord.SoLuongBuoi = inputData.SoLuongBuoi;
            //existingRecord.NoiDungCongTac = inputData.NoiDungCongTac;
            //existingRecord.TruongCongTac = inputData.TruongCongTac;
            //existingRecord.UserNhap = inputData.UserNhap;
            //existingRecord.AqUser = inputData.AqUser;

            // Update the record in the database
            tb.Update(existingRecord);

            // Return the updated record
            return new DsNgayCongTacResult
            {
                data = new DsNgayCongTacDO[] { existingRecord }
            };
        }

        [HttpDelete, Route("{id}")]
        public DsNgayCongTacResult delete(Guid id)
        {
            var tb = database.Table<DsNgayCongTacDO>();

            var existingRecord = tb.FindById(id);
            if (existingRecord == null)
            {
                return new DsNgayCongTacResult
                {
                    code = 404,
                    message = "Data not found"
                };
            }
            tb.Delete(id);
            var data = tb.FindById(id);
            return new DsNgayCongTacResult
            {
                data = new DsNgayCongTacDO[] { existingRecord }
            };
        }
    }
}
