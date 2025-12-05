using AQFramework;
using AQFramework.Languages;
using educlient.Data;
using LiteDB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace educlient.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    
    public class TinhNangBaoGiaController : ControllerBase
    {
        private readonly IDbLiteContext database;
        private readonly IConfiguration config;

        public TinhNangBaoGiaController(IDbLiteContext dataContext, IConfiguration cf)
        {
            database = dataContext;
            config = cf;
        }

        // --- API THÊM MỚI
        [HttpPost, Route("Add")]
        public IActionResult Add([FromForm] QuotationFeatureInput input)
        {
            var col = database.Table<QuotationFeature>();

            // Map dữ liệu
            var newRecord = new QuotationFeature
            {
                Name = input.Name,
                Type = input.Type, // group, module, feature
                ParentId = input.ParentId,
                IncludedInPackage = input.IncludedInPackage ?? new PackageIncluded(),
                DetailPrice = input.DetailPrice,
                FeatureDescription = input.FeatureDescription,
                DevNote = input.DevNote,
                SaleNote = input.SaleNote,
                SupNote = input.SupNote,
                YoutubeUrl = input.YoutubeUrl,
                Attachments = new List<FeatureAttachment>()
            };

            // Xử lý File Upload
            ProcessAttachments(input.UploadFiles, newRecord);

            col.Insert(newRecord);
            return Created("", newRecord);
        }

        // --- API LẤY DANH SÁCH ---
        [HttpGet]
        public IActionResult GetAll()
        {
            var col = database.Table<QuotationFeature>();
            return Ok(col.FindAll());
        }

        [HttpPut, Route("Update")]
        public IActionResult Update([FromForm] QuotationFeatureInput input)
        {
            var col = database.Table<QuotationFeature>();
            var existingRecord = col.FindById(input.Id);

            if (existingRecord == null) return NotFound("Không tìm thấy bản ghi");

            // Update thông tin text
            existingRecord.Name = input.Name;
            existingRecord.DetailPrice = input.DetailPrice;
            existingRecord.FeatureDescription = input.FeatureDescription;
            existingRecord.IncludedInPackage = input.IncludedInPackage; // LiteDB tự xử lý object con
            existingRecord.DevNote = input.DevNote;
            existingRecord.SaleNote = input.SaleNote;
            existingRecord.SupNote = input.SupNote;
            existingRecord.YoutubeUrl = input.YoutubeUrl;

            // Xử lý XÓA file cũ (nếu có yêu cầu)
            if (input.RemoveFileIds != null && input.RemoveFileIds.Count > 0)
            {
                foreach (var fileId in input.RemoveFileIds)
                {
                    // Xóa trong Storage
                    database.FileStorage.Delete(fileId);
                    // Xóa trong List Attachments
                    var fileToRemove = existingRecord.Attachments.FirstOrDefault(x => x.FileId == fileId);
                    if (fileToRemove != null) existingRecord.Attachments.Remove(fileToRemove);
                }
            }

            // Xử lý THÊM file mới
            ProcessAttachments(input.UploadFiles, existingRecord);

            col.Update(existingRecord);
            return Ok(existingRecord);
        }

        // --- API DOWNLOAD FILE ---
        [HttpGet, Route("DownloadFile")]
        public IActionResult DownloadFile(string fileId)
        {
            var fileInfo = database.FileStorage.FindById(fileId);
            if (fileInfo == null) return NotFound("File không tồn tại");

            var stream = database.FileStorage.OpenRead(fileId);
            return File(stream, fileInfo.MimeType, fileInfo.Filename);
        }

        // --- API XÓA ---
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var col = database.Table<QuotationFeature>();
            var record = col.FindById(id);

            if (record == null)
            {
                return NotFound("Không tìm thấy bản ghi cần xóa.");
            }

            // --- BƯỚC 1: VALIDATE RÀNG BUỘC CHA-CON ---
            // Kiểm tra xem có thằng con nào đang trỏ tới thằng này không
            var hasChildren = col.Exists(x => x.ParentId == id);

            if (hasChildren)
            {
                // Nếu còn con thì CHẶN LẠI, trả về lỗi 400
                string msg = "";
                if (record.Type == "group")
                    msg = "Có lỗi xảy ra khi xóa!";
                else if (record.Type == "module")
                    msg = "Có lỗi xảy ra khi xóa!";
                else
                    msg = "Có lỗi xảy ra khi xóa!";

                return BadRequest(msg);
            }

            // --- BƯỚC 2: NẾU KHÔNG CÓ CON -> TIẾN HÀNH XÓA FILE & DATA ---
            try
            {
                // Xóa file đính kèm trong Storage
                if (record.Attachments != null)
                {
                    foreach (var file in record.Attachments)
                    {
                        if (!string.IsNullOrEmpty(file.FileId))
                        {
                            database.FileStorage.Delete(file.FileId);
                        }
                    }
                }

                // Xóa bản ghi trong DB
                col.Delete(id);

                return Ok(new { message = "Xóa thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi server: " + ex.Message);
            }
        }

        // --- HELPER: XỬ LÝ UPLOAD VÀO LITEDB ---
        private void ProcessAttachments(List<IFormFile>? files, QuotationFeature record)
        {
            if (files == null || files.Count == 0) return;

            foreach (var file in files)
            {
                var fileId = Guid.NewGuid().ToString();
                using (var stream = file.OpenReadStream())
                {
                    // Upload vào LiteDB FileStorage
                    database.FileStorage.Upload(fileId, file.FileName, stream);
                }

                // Xác định loại file để hiển thị icon ở frontend
                string ext = Path.GetExtension(file.FileName).ToLower();
                string type = "other";
                if (ext.Contains("doc")) type = "word";
                else if (ext.Contains("pdf")) type = "pdf";
                else if (ext.Contains("xls") || ext.Contains("sheet")) type = "excel";
                else if (ext.Contains("png") || ext.Contains("jpg")) type = "image";

                // Thêm vào danh sách Attachments
                record.Attachments.Add(new FeatureAttachment
                {
                    FileId = fileId,
                    FileName = file.FileName,
                    FileSize = file.Length,
                    FileType = type
                });
            }
        }

        public class QuotationFeatureFileDTO
        {
            public int Id { get; set; } 
            public IFormFile? FileWord { get; set; }
            public IFormFile? FilePdf { get; set; }
        }

        public class QuotationFeatureInput : QuotationFeature
        {
            public List<IFormFile>? UploadFiles { get; set; }
            public List<string>? RemoveFileIds { get; set; }
        }

    }


}
