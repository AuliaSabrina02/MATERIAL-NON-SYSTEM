using ClosedXML.Excel;
using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml.Spreadsheet;
using MailKit.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Bcpg;
using SEMB_ERP.Function;
using SEMB_ERP.Models;
using SEMB_ERP.Service;
using System.Data;
using System.Diagnostics;
using System.Linq.Dynamic.Core;
using System.Security.Claims;
using Microsoft.Data.SqlClient;

namespace SEMB_ERP.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ImportExportFactory _importexportFactory;
        //private readonly FileManagementService _fileManagement;
        private readonly ILogger<UserController> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly DatabaseAccessLayer _dal;

        public UserController(ImportExportFactory importexportFactory, ILogger<UserController> logger, ApplicationDbContext context, IWebHostEnvironment environment)
        {
            this._context = context;
            _importexportFactory = importexportFactory;
            //_fileManagement = fileManagement;
            _logger = logger;
            _environment = environment;
            this._dal = new DatabaseAccessLayer();
        }
        public IActionResult Index()
        {
            return View();
        }
        [Authorize(Policy = "RequireRequestor")]
        public IActionResult AddNewOrder()
        {
            return this.CheckSession(() =>
            {
                var db = new DatabaseAccessLayer();
                string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                string name = User.FindFirst("semb_erp_name")?.Value;
                string plant = db.GetUserPlant(sesa_id);
                ViewBag.name = name;
                ViewBag.sesa_id = sesa_id;
                ViewBag.plant = plant;
                return View();
            });
        }
        [HttpGet]
        public IActionResult GetPIC(string search_value)
        {
            var db = new DatabaseAccessLayer();
            List<CategoryModel> listPIC = db.GetPIC(search_value);
            return Json(new { items = listPIC });
        }
        private string getNextFileName(string fileName)
        {
            string extension = Path.GetExtension(fileName);
            int i = 0;
            while (System.IO.File.Exists(fileName))
            {
                if (i == 0)
                    fileName = fileName.Replace(extension, "(" + ++i + ")" + extension);
                else
                    fileName = fileName.Replace("(" + i + ")" + extension, "(" + ++i + ")" + extension);
            }

            return fileName;
        }
        [Authorize(Policy = "RequireRequestor")]
        public IActionResult UploadOrder(IFormFile file_upload)
        {
            string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (sesa_id == "")
            {
                return Content("Session Timeout, Please relogin!!", "text/plain");
            }
            else
            {
                DateTime now = DateTime.Now;
                string id_upload = now.ToString("yyMMddHHmmssfff");
                _importexportFactory.ImportOrder(file_upload, id_upload, sesa_id);

                var db = new DatabaseAccessLayer();
                List<OrderTempListModel> dataTemp = db.GetTempOrder(id_upload, sesa_id);
                ViewBag.id_upload = id_upload;
                return PartialView("_TableTempOrder", dataTemp);
            }
        }

        public JsonResult GetDiscussions(int id_order)
        {
            try
            {
                string fullIdentity = User.Identity?.Name ?? "";
                string sesaId = fullIdentity.Contains("\\") ? fullIdentity.Split('\\')[1] : fullIdentity;

                if (string.IsNullOrEmpty(sesaId))
                {
                    sesaId = User.FindFirstValue("sesa_id") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
                }

                if (string.IsNullOrEmpty(sesaId))
                {
                    return Json(new { error = "User not authenticated" });
                }

                DatabaseAccessLayer dal = new DatabaseAccessLayer();
                var discussions = dal.GetDiscussions(id_order, sesaId);
                dal.MarkDiscussionAsRead(id_order, sesaId);

                return Json(discussions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting discussions");
                return Json(new { error = ex.Message });
            }
        }
        public IActionResult MaterialReturn()
        {
            ViewBag.listStatus = new List<string> { "Sent", "Approved", "In Transit", "Returned" };

            // Kirim list kosong menggunakan model yang baru dibuat
            return View(new List<MaterialReturnModel>());
        }

        [HttpGet]
        public IActionResult ReturnMaterialReceiver()
        {
            string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            ViewBag.sesa_id = sesa_id;
            ViewBag.listStatus = new List<string> { "Sent", "Approved", "In Transit", "Returned" };
            var db = new DatabaseAccessLayer();
            var userRoles = db.GetUserRole(sesa_id);
            ViewBag.userRoles = userRoles.Select(r => r.role).ToList();
            return View();
        }

        [HttpGet]
        public IActionResult GetOrderListByStatusJson(string status, string search = "")
        {
            try
            {
                string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                List<string> user_roles = User.Claims
                                              .Where(c => c.Type == "semb_erp_role")
                                              .Select(c => c.Value)
                                              .ToList();
                if (string.IsNullOrEmpty(sesa_id))
                    return Unauthorized("Sesi anda berakhir atau ID User tidak ditemukan.");

                var db = new DatabaseAccessLayer();
                List<UserDetailModel> userDetail = db.GetUserDetail(sesa_id);
                if (userDetail == null || !userDetail.Any())
                    return BadRequest("Detail user tidak ditemukan di database.");

                var user = userDetail.First();
                var departments = string.IsNullOrEmpty(user.other_dept)
                    ? new List<string>()
                    : user.other_dept.Split(',').Select(d => d.Trim()).ToList();

                var query = _context.v_order.Where(o => o.status_desc == status);

                if (!string.IsNullOrEmpty(search))
                    query = query.Where(o => o.partno.Contains(search));

                if (!user_roles.Contains("admin") && !user_roles.Contains("receiver"))
                    query = query.Where(o => o.pic == sesa_id || departments.Contains(o.pic_department));

                var orderData = query.Select(o => new OrderListModel
                {
                    id_order = o.id_order,
                    partno = o.partno,
                    po_no = o.po_no,
                    qty = o.qty,
                    uom = o.uom,
                    status_desc = o.status_desc,
                    record_date = o.record_date
                }).ToList();

                return Json(orderData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult GetTotalReturned(string partno, string po_no)
        {
            var total = _context.material_return
                    .Where(r => r.partno == partno
         && r.po_no == po_no)
                .Sum(r => (decimal?)r.qty_return) ?? 0;

            return Json(total);
        }

        [HttpPost]
        public string CreateReturn(MaterialReturnModel model, IFormFile image_support_file)
        {
            string sesa_id = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var db = new DatabaseAccessLayer();

            // Pastikan urutan parameter sesuai: model, file, lalu sesa_id
            return db.CreateReturn(model, image_support_file, sesa_id);
        }

        [HttpPost]
        public string UpdateStatusByReceiver(int id_return, int status)
        {
            var db = new DatabaseAccessLayer();
            return db.UpdateReturnStatus(id_return, status);
        }



        [HttpPost]
        public dynamic GetReturnList(IFormCollection form)
        {
            var db = new DatabaseAccessLayer();
            return db.GetReturnList(form);
        }



        [HttpPost]
        public string UpdateReturn(MaterialReturnModel model, IFormFile image_support_file)
        {
            // Ambil sesa_id untuk keperluan log jika nanti dibutuhkan
            string sesa_id = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var db = new DatabaseAccessLayer();

            // Pastikan urutan parameter: model, baru file, baru string
            return db.UpdateReturn(model, image_support_file, sesa_id);
        }

        //[HttpGet]
        //public IActionResult PalletProblem()
        //{


        //    return View();
        //}
        [HttpPost]
        public JsonResult GetPalletProblems()
        {
            try
            {
                var draw = Request.Form["draw"].FirstOrDefault();
                var start = Request.Form["start"].FirstOrDefault();
                var length = Request.Form["length"].FirstOrDefault();
                var sortColumn = Request.Form["order[0][column]"].FirstOrDefault();
                var sortDirection = Request.Form["order[0][dir]"].FirstOrDefault();

                // TAMBAH INI - baca filter per kolom
                var searchPalletNo = Request.Form["columns[1][search][value]"].FirstOrDefault();
                var searchDate = Request.Form["columns[2][search][value]"].FirstOrDefault();
                var searchIssue = Request.Form["columns[3][search][value]"].FirstOrDefault();
                var searchCreatedBy = Request.Form["columns[4][search][value]"].FirstOrDefault();
                var searchUpdatedBy = Request.Form["columns[5][search][value]"].FirstOrDefault();

                int pageSize = length != null ? Convert.ToInt32(length) : 10;
                int skip = start != null ? Convert.ToInt32(start) : 0;

                var result = _dal.GetPalletProblems(skip, pageSize, searchPalletNo, searchDate, searchIssue, searchCreatedBy, searchUpdatedBy, sortColumn, sortDirection);

                return Json(new
                {
                    draw = draw,
                    recordsFiltered = result.totalRecords,
                    recordsTotal = result.totalRecords,
                    data = result.data
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult UpdatePalletProblemStatus(int id, string status)
        {
            try
            {
                string sesa_id = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
                var result = _dal.UpdatePalletProblemStatus(id, status, sesa_id);
                return Json(result);
            }
            catch (Exception ex)
            {
                return Json("error;" + ex.Message);
            }
        }

        [Authorize(Policy = "RequireRequestor")]
        [HttpPost]
        public JsonResult SaveAsTemplate(OrderTemplateModel model)
        {
            try
            {
                // Pastikan sesa_id diambil dengan benar dan konsisten (misal: ToLower)
                string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value?.ToLower();
                model.created_by = sesa_id;
                model.created_date = DateTime.Now;

                // Tambahkan pengecekan jika model kosong
                if (model == null) return Json(new { status = "error", message = "Data model is empty" });

                var db = new DatabaseAccessLayer();
                string result = db.SaveOrderTemplate(model);

                if (result.StartsWith("success"))
                {
                    return Json(new { status = "success", message = "Template saved successfully" });
                }
                else
                {
                    return Json(new { status = "error", message = result.Split(';')[1] });
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = "error", message = ex.Message });
            }
        }

        [Authorize(Policy = "RequireRequestor")]
        [HttpGet]
        public JsonResult GetTemplateList()
        {
            try
            {
                string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var db = new DatabaseAccessLayer();
                var templates = db.GetOrderTemplates(sesa_id);

                return Json(templates);
            }
            catch (Exception ex)
            {
                return Json(new List<OrderTemplateModel>());
            }
        }

        [HttpGet]
        public IActionResult GetPalletProblemDetail(int id)
        {
            try
            {
                var data = _dal.GetPalletProblemById(id);

                if (data == null)
                {
                    return Content("<p class='text-danger'>Data not found</p>");
                }

                var html = $@"
                    <table class='table table-bordered'>
                        <tr>
                            <th width='30%'>Pallet No</th>
                            <td>{data.pallet_no}</td>
                        </tr>
                        <tr>
                            <th>Request No</th>
                            <td>{data.req_no}</td>
                        </tr>
                        <tr>
                            <th>Date</th>
                            <td>{data.date:dd/MM/yyyy}</td>
                        </tr>
                        <tr>
                            <th>Issue</th>
                            <td>{data.issue}</td>
                        </tr>
                        <tr>
                            <th>Created By</th>
                            <td>{data.created_by ?? "-"}</td>
                        </tr>
                        <tr>
                            <th>Created Date</th>
                            <td>{data.created_date:dd/MM/yyyy HH:mm:ss}</td>
                        </tr>
                        {(data.updated_by != null ? $@"
                        <tr>
                            <th>Updated By</th>
                            <td>{data.updated_by}</td>
                        </tr>
                        <tr>
                            <th>Updated Date</th>
                            <td>{data.updated_date:dd/MM/yyyy HH:mm:ss}</td>
                        </tr>" : "")}
                        <tr>
                            <th>Status</th>
                            <td><span class='badge badge-success'>{data.status}</span></td>
                        </tr>
                    </table>
                ";

                return Content(html);
            }
            catch (Exception ex)
            {
                return Content($"<p class='text-danger'>Error: {ex.Message}</p>");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddPalletProblem(IFormCollection form)
        {
            try
            {
                string palletNo = form["pallet_no"].ToString().Trim();

                // Check if Pallet No already exists
                if (_dal.CheckPalletNoExists(palletNo))
                {
                    return Content("error;Pallet No already exists!");
                }

                var palletProblem = new PalletProblem
                {
                    pallet_no = palletNo,
                    req_no = form["req_no"].ToString().Trim(),
                    date = DateTime.Parse(form["date"].ToString()),
                    issue = form["issue"].ToString().Trim(),
                    created_by = form["created_by"].ToString().Trim(),
                    created_date = DateTime.Now,
                    status = "Active"
                };

                bool success = _dal.AddPalletProblem(palletProblem);

                if (success)
                {
                    return Content("success;Pallet Problem has been added successfully!");
                }
                else
                {
                    return Content("error;Failed to add Pallet Problem");
                }
            }
            catch (Exception ex)
            {
                return Content($"error;{ex.Message}");
            }
        }
        public IActionResult PalletProblem()
        {
            string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var db = new DatabaseAccessLayer();
            string plant = db.GetUserPlant(sesa_id);
            string name = User.FindFirst("semb_erp_name")?.Value;

            ViewBag.sesa_id = sesa_id;
            ViewBag.name = name;
            ViewBag.plant = plant;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdatePalletProblem(IFormCollection form)
        {
            try
            {
                int id = int.Parse(form["id"].ToString());
                string palletNo = form["pallet_no"].ToString().Trim();

                // Check if Pallet No already exists (exclude current record)
                if (_dal.CheckPalletNoExists(palletNo, id))
                {
                    return Content("error;Pallet No already exists!");
                }

                var palletProblem = new PalletProblem
                {
                    id = id,
                    pallet_no = palletNo,
                    req_no = form["req_no"].ToString().Trim(),
                    date = DateTime.Parse(form["date"].ToString()),
                    issue = form["issue"].ToString().Trim(),
                    updated_by = form["created_by"].ToString().Trim(),
                    updated_date = DateTime.Now
                };

                bool success = _dal.UpdatePalletProblem(palletProblem);

                if (success)
                {
                    return Content("success;Pallet Problem has been updated successfully!");
                }
                else
                {
                    return Content("error;Failed to update Pallet Problem");
                }
            }
            catch (Exception ex)
            {
                return Content($"error;{ex.Message}");
            }
        }

        [HttpGet]
        public JsonResult GetPalletProblemById(int id)
        {
            try
            {
                var data = _dal.GetPalletProblemById(id);

                if (data == null)
                {
                    return Json(new { error = "Data not found" });
                }

                return Json(new
                {
                    id = data.id,
                    pallet_no = data.pallet_no,
                    req_no = data.req_no,
                    date = data.date.ToString("yyyy-MM-dd"),
                    issue = data.issue,
                    created_by = data.created_by
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }
        [HttpPost]
        public IActionResult DeletePalletProblem(int id)
        {
            try
            {
                bool success = _dal.DeletePalletProblem(id);

                if (success)
                {
                    return Content("success;Pallet Problem has been deleted successfully!");
                }
                else
                {
                    return Content("error;Failed to delete Pallet Problem");
                }
            }
            catch (Exception ex)
            {
                return Content($"error;{ex.Message}");
            }
        }

        [HttpGet]
        public IActionResult GetPalletProblemByDate(string date)
        {
            try
            {
                var result = _dal.GetPalletProblems(0, 9999, "", "", "", "", "", "1", "asc");

                System.Diagnostics.Debug.WriteLine($"=== PALLET DEBUG ===");
                System.Diagnostics.Debug.WriteLine($"Date param: '{date}'");
                System.Diagnostics.Debug.WriteLine($"Total data: {result.data.Count}");

                // ✅ Print semua tanggal yang ada di data
                foreach (var item in result.data)
                {
                    System.Diagnostics.Debug.WriteLine($"  date='{item.date}' | .Date='{item.date.Date}' | ToString='{item.date.ToString("yyyy-MM-dd")}'");
                }

                List<PalletProblem> filteredData;

                if (DateTime.TryParse(date, out DateTime parsedDate))
                {
                    filteredData = result.data
                        .Where(x => x.date.Date == parsedDate.Date)
                        .OrderByDescending(x => x.created_date)
                        .ToList();

                    System.Diagnostics.Debug.WriteLine($"Parsed date: '{parsedDate.Date}'");
                    System.Diagnostics.Debug.WriteLine($"Filtered: {filteredData.Count} records");
                }
                else
                {
                    // ✅ Kalau TryParse gagal, coba manual parse yyyy-MM-dd
                    System.Diagnostics.Debug.WriteLine($"TryParse failed, trying manual parse...");
                    var parts = date.Split('-');
                    if (parts.Length == 3)
                    {
                        int year = int.Parse(parts[0]);
                        int month = int.Parse(parts[1]);
                        int day = int.Parse(parts[2]);
                        var manualDate = new DateTime(year, month, day).Date;

                        filteredData = result.data
                            .Where(x => x.date.Date == manualDate)
                            .OrderByDescending(x => x.created_date)
                            .ToList();

                        System.Diagnostics.Debug.WriteLine($"Manual parsed: '{manualDate}' | Filtered: {filteredData.Count}");
                    }
                    else
                    {
                        filteredData = new List<PalletProblem>();
                    }
                }

                System.Diagnostics.Debug.WriteLine($"=== END DEBUG ===");

                ViewBag.DateFilter = date;
                ViewBag.TotalRecords = filteredData.Count;

                return PartialView("_PalletProblemByDatePartial", filteredData);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult PalletList()
        {

            return View();
        }

        [HttpPost]
        public JsonResult GetPalletList()
        {
            try
            {
                var draw = Request.Form["draw"].FirstOrDefault();
                var start = Request.Form["start"].FirstOrDefault();
                var length = Request.Form["length"].FirstOrDefault();
                var sortColumn = Request.Form["order[0][column]"].FirstOrDefault();
                var sortDirection = Request.Form["order[0][dir]"].FirstOrDefault();

                // TAMBAH INI
                var searchPalletNo = Request.Form["columns[1][search][value]"].FirstOrDefault();
                var searchDate = Request.Form["columns[2][search][value]"].FirstOrDefault();
                var searchIssue = Request.Form["columns[3][search][value]"].FirstOrDefault();
                var searchCreatedBy = Request.Form["columns[4][search][value]"].FirstOrDefault();
                var searchUpdatedBy = Request.Form["columns[5][search][value]"].FirstOrDefault();

                int pageSize = length != null ? Convert.ToInt32(length) : 10;
                int skip = start != null ? Convert.ToInt32(start) : 0;

                var result = _dal.GetPalletList(skip, pageSize, searchPalletNo, searchDate, searchIssue, searchCreatedBy, searchUpdatedBy, sortColumn, sortDirection);
                return Json(new
                {
                    draw = draw,
                    recordsFiltered = result.totalRecords,
                    recordsTotal = result.totalRecords,
                    data = result.data
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult GetPalletListDetail(int id)
        {
            try
            {
                var data = _dal.GetPalletProblemById(id);

                if (data == null)
                {
                    return Content("<p class='text-danger'>Data not found</p>");
                }

                var html = $@"
            <table class='table table-bordered'>
                <tr>
                    <th width='30%'>Pallet No</th>
                    <td>{data.pallet_no}</td>
                </tr>
                <tr>
                    <th>Request No</th>
                    <td>{data.req_no}</td>
                </tr>
                <tr>
                    <th>Date</th>
                    <td>{data.date:dd/MM/yyyy}</td>
                </tr>
                <tr>
                    <th>Issue</th>
                    <td>{data.issue}</td>
                </tr>
                
                <tr>
                    <th>Created By</th>
                    <td>{data.created_by ?? "-"}</td>
                </tr>
                <tr>
                    <th>Created Date</th>
                    <td>{data.created_date:dd/MM/yyyy HH:mm:ss}</td>
                </tr>
            </table>
        ";

                return Content(html);
            }
            catch (Exception ex)
            {
                return Content($"<p class='text-danger'>Error: {ex.Message}</p>");
            }
        }


        [HttpPost]
        public string SendDiscussion(int id_order, string message, int? parent_id)
        {
            try
            {
                string fullIdentity = User.Identity?.Name ?? "";
                string sesa_id = fullIdentity.Contains("\\")
                    ? fullIdentity.Split('\\')[1]
                    : fullIdentity;
                if (string.IsNullOrEmpty(sesa_id))
                {
                    sesa_id = User.FindFirstValue("sesa_id")
                             ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
                }
                if (string.IsNullOrEmpty(sesa_id))
                {
                    return "Error: User not authenticated. Please login again.";
                }
                if (string.IsNullOrWhiteSpace(message))
                {
                    return "Error: Message cannot be empty.";
                }
                DatabaseAccessLayer dal = new DatabaseAccessLayer();
                string result = dal.InsertDiscussion(id_order, sesa_id, message, parent_id);

                if (result == "OK")
                {
                    Task.Run(() =>
                    {
                        try
                        {
                            bool emailSent = dal.SendDiscussionEmailNotification(id_order, sesa_id, message);

                            if (emailSent)
                            {
                                _logger.LogInformation($"Email notification sent for discussion on Order #{id_order} by {sesa_id}");
                            }
                            else
                            {
                                _logger.LogWarning($"Email notification failed for discussion on Order #{id_order}");
                            }
                        }
                        catch (Exception emailEx)
                        {
                            _logger.LogError(emailEx, $"Error sending email notification for Order #{id_order}");
                        }
                    });

                    return "OK";
                }
                else
                {
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending discussion");
                return "Error: " + ex.Message;
            }
        }

        [HttpPost]
        public JsonResult GetPalletSummary()
        {
            try
            {
                string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var db = new DatabaseAccessLayer();
                string plant = db.GetUserPlant(sesa_id);

                // ✅ ToList dulu sebelum Count
                var palletData = _context.v_pallet_header
                    .Where(p => p.plant == "ALL" || p.plant == plant)
                    .Select(p => new { p.status_pallet }) // Ambil yang dibutuhkan saja
                     .ToList(); // ✅ Execute query dulu

                var summary = new
                {
                    totalCreated = palletData.Count(p => p.status_pallet == "CREATION"),
                    totalTransferred = palletData.Count(p => p.status_pallet == "TRANSFER" ||
                                                              p.status_pallet == "RECEIVED"),
                    totalSupplied = palletData.Count(p => p.status_pallet == "SUPPLIED")
                };

                return Json(summary);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }



        [HttpPost]
        public IActionResult GetPalletStatusChartData()
        {
            string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var db = new DatabaseAccessLayer();
            string plant = db.GetUserPlant(sesa_id);

            // Ambil data dan group berdasarkan status
            var statusGroups = _context.v_pallet_header
                .Where(p => (p.plant == "ALL" || p.plant == plant))
                .GroupBy(p => p.status_pallet)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count()
                }).ToList();

            // Siapkan untuk Highcharts
            var categories = statusGroups.Select(x => x.Status).ToArray();
            var dataValues = statusGroups.Select(x => x.Count).ToArray();

            return Json(new { categories, dataValues });
        }

        public IActionResult GetPalletListByStatus(string status)
        {
            string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var db = new DatabaseAccessLayer();
            string plant = db.GetUserPlant(sesa_id);

            var data = (from Pallet in _context.v_pallet_header
                        where Pallet.status_pallet == status.ToUpper()
                        && (Pallet.plant == "ALL" || Pallet.plant == plant)
                        select Pallet).ToList();

            ViewBag.StatusFilter = status.ToUpper();  // ✅ Tambahkan ini
            ViewBag.TotalRecords = data.Count;        // ✅ Tambahkan ini

            return PartialView("_PalletListPartial", data);
        }

        public IActionResult DashboardPalletReceiver()
        {
            string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var db = new DatabaseAccessLayer();

            // Get user details
            var userDetail = db.GetUserDetail(sesa_id);
            var userRoles = db.GetUserRole(sesa_id);

            ViewBag.sesa_id = sesa_id;
            ViewBag.name = userDetail.FirstOrDefault()?.name ?? "";
            ViewBag.userRoles = userRoles;

            return View(userDetail);
        }

        [HttpPost]
        public IActionResult GetPalletHistoryList()
        {
            try
            {
                string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var db = new DatabaseAccessLayer();
                string plant = db.GetUserPlant(sesa_id);

                var draw = Request.Form["draw"].FirstOrDefault();
                var searchValue = Request.Form["search[value]"].FirstOrDefault();

                // 1. Query Dasar
                var query = from Pallet in _context.v_pallet_header
                            join request in _context.v_request
                            on Pallet.id_request equals request.id_request
                            where (Pallet.plant == "ALL" || Pallet.plant == plant)
                            select Pallet;

                // 2. Filter pencarian (Jika ada input dari user)
                if (!string.IsNullOrEmpty(searchValue))
                {
                    query = query.Where(m => m.request_no.Contains(searchValue) ||
                                             m.pallet_no.Contains(searchValue));
                }

                // 3. AMBIL HANYA 10 DATA TERBARU
                // Urutkan dari yang paling baru, lalu ambil 10 teratas
                var rawData = query.OrderByDescending(p => p.record_date)
                                   .Take(10)
                                   .ToList();

                // 4. Formatting data untuk tampilan
                var data = rawData.Select(p => new
                {
                    p.id_pallet,
                    p.pallet_no,
                    p.request_no,
                    p.status_pallet,
                    record_date = p.record_date != null ? p.record_date.Value.ToString("yyyy-MM-dd HH:mm") : "-",
                    transfer_date = p.receive_date != null ? p.receive_date.Value.ToString("yyyy-MM-dd HH:mm") : "-",
                    supplied_date = p.supply_date != null ? p.supply_date.Value.ToString("yyyy-MM-dd HH:mm") : "-"
                }).ToList();

                int recordsTotal = data.Count; // Totalnya sekarang hanya maksimal 10

                var jsonData = new
                {
                    draw = draw,
                    recordsFiltered = recordsTotal,
                    recordsTotal = recordsTotal,
                    data = data
                };

                return Ok(jsonData);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        public IActionResult GetRecentPalletHistory()
        {
            string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var db = new DatabaseAccessLayer();
            string plant = db.GetUserPlant(sesa_id);

            var data = (from Pallet in _context.v_pallet_header
                        where (Pallet.plant == "ALL" || Pallet.plant == plant)
                        orderby Pallet.record_date descending
                        select new
                        {
                            Pallet.id_pallet,
                            Pallet.pallet_no,
                            Pallet.request_no,
                            Pallet.status_pallet,
                            record_date = Pallet.record_date != null ? Pallet.record_date.Value.ToString("yyyy-MM-dd HH:mm") : "-",
                            transfer_date = Pallet.receive_date != null ? Pallet.receive_date.Value.ToString("yyyy-MM-dd HH:mm") : "-",
                            supplied_date = Pallet.supply_date != null ? Pallet.supply_date.Value.ToString("yyyy-MM-dd HH:mm") : "-"
                        })
                        .Take(10)
                        .ToList<dynamic>();

            return PartialView("_PalletHistoryPartial", data);
        }
        private void SendEmail(string toEmail, string subject, string body)
        {
            try
            {
                _logger.LogInformation($"Sending email to: {toEmail}, Subject: {subject}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending email to {toEmail}");
                throw;
            }
        }
        [Authorize(Policy = "RequireRequestor")]
        [HttpPost]
        public async Task<IActionResult> SubmitUploadOrder(IFormFile file_support, string id_upload)
        {
            string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(sesa_id))
            {
                return Content("Session Timeout, Please relogin!!", "text/plain");
            }

            var db = new DatabaseAccessLayer();
            string file_support_db = "";
            if (file_support != null && file_support.Length > 0)
            {
                string filePath = getNextFileName(_environment.WebRootPath + "\\Documents\\" + id_upload + " - " + file_support.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file_support.CopyToAsync(stream);
                    file_support_db = Path.GetFileName(filePath);
                }
            }

            string submit = db.SubmitUploadOrder(file_support_db, id_upload, sesa_id);

            if (submit.ToLower().Contains("success"))
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(500);
                        var dalEmail = new DatabaseAccessLayer();
                        var orders = dalEmail.GetAllOrderIdsByUploadId(id_upload);

                        foreach (var (id, priority) in orders)
                        {
                            // Kirim status update email
                            dalEmail.TriggerStatusUpdateEmail(id, sesa_id, 1);

                            // Cek priority, kalau High kirim urgent email
                            System.Diagnostics.Debug.WriteLine($"[SubmitUploadOrder] Order {id} priority: '{priority}'");
                            if (!string.IsNullOrEmpty(priority) && priority.Trim().Equals("High", StringComparison.OrdinalIgnoreCase))
                            {
                                System.Diagnostics.Debug.WriteLine($"[SubmitUploadOrder] Order {id} HIGH, sending urgent email...");
                                try
                                {
                                    dalEmail.TriggerUrgentReceiverEmail(id, sesa_id);
                                    System.Diagnostics.Debug.WriteLine($"[SubmitUploadOrder] Urgent email sent for order {id}");
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine($"[SubmitUploadOrder] Urgent email failed: {ex.Message}");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[SubmitUploadOrder] ERROR: {ex.Message}");
                    }
                });
            }

            return Content("success;Successfully Submitted!", "text/plain");
        }

        [Authorize(Policy = "RequireAny")]
        public IActionResult RequestList()
        {
            return this.CheckSession(() =>
            {
                string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                List<string> userRoles = User.Claims
                                            .Where(c => c.Type == "semb_erp_role")
                                            .Select(c => c.Value)
                                            .ToList();
                //sesa_id = "SESA126011";
                var db = new DatabaseAccessLayer();
                List<UserDetailModel> userDetail = db.GetUserDetail(sesa_id);
                List<string> listStatus = db.GetStatusRequest();
                List<string> listPrinter = db.GetPrinter();
                string name = User.FindFirst("semb_erp_name")?.Value;
                ViewBag.name = name;
                ViewBag.sesa_id = sesa_id;
                ViewBag.listStatus = listStatus;
                ViewBag.userRoles = userRoles;
                ViewBag.listPrinter = listPrinter;

                return View(userDetail);
            });
        }


        [Authorize(Policy = "RequireRequestor")]
        [HttpPost]
        public async Task<IActionResult> SubmitOrder(
        IFormFile file_support,
        string material_type,
        string partno,
        string po_no,
        double qty,
        string uom,
        string revision,
        string project_name,
        string storage_requirement,
        string supplier_name,
        string pic,
        string order_type,
        double unit_price,
        string priority_request,
        double length_mm,
        double width_mm,
        double height_mm,
        string remark,
        string gatepass)
        {
            DateTime now = DateTime.Now;
            string id_upload = now.ToString("yyMMddHHmmssfff");
            string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(sesa_id))
            {
                return Content("Session Timeout, Please relogin!!", "text/plain");
            }

            var db = new DatabaseAccessLayer();
            string file_support_db = "";
            if (file_support != null && file_support.Length > 0)
            {
                string filePath = getNextFileName(_environment.WebRootPath + "\\Documents\\" + id_upload + " - " + file_support.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file_support.CopyToAsync(stream);
                    file_support_db = Path.GetFileName(filePath);
                }
            }
            string submit = db.SubmitOrder(
                id_upload, material_type, partno, po_no, qty, uom, revision, project_name,
                storage_requirement, supplier_name, pic, order_type, unit_price, priority_request,
                length_mm, width_mm, height_mm, remark, gatepass, file_support_db, sesa_id);
            if (submit.ToLower().Contains("success"))
            {
                System.Diagnostics.Debug.WriteLine($"=== ORDER SUBMITTED ===");
                System.Diagnostics.Debug.WriteLine($"id_upload: {id_upload}");
                System.Diagnostics.Debug.WriteLine($"sesa_id: {sesa_id}");
                System.Diagnostics.Debug.WriteLine($"priority_request: '{priority_request}'");
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(500);
                        var dalEmail = new DatabaseAccessLayer();
                        int orderId = dalEmail.GetOrderIdByUploadId(id_upload);

                        System.Diagnostics.Debug.WriteLine($"Order ID retrieved: {orderId}");

                        if (orderId > 0)
                        {
                            System.Diagnostics.Debug.WriteLine("Sending status update email to requestor...");
                            try
                            {
                                dalEmail.TriggerStatusUpdateEmail(orderId, sesa_id, 1);
                                System.Diagnostics.Debug.WriteLine("Status update email sent successfully");
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Status update email failed: {ex.Message}");
                            }
                            System.Diagnostics.Debug.WriteLine($"Checking priority: '{priority_request}'");

                            if (!string.IsNullOrEmpty(priority_request))
                            {
                                string priorityTrimmed = priority_request.Trim();
                                System.Diagnostics.Debug.WriteLine($"Priority after trim: '{priorityTrimmed}'");

                                if (priorityTrimmed.Equals("High", StringComparison.OrdinalIgnoreCase))
                                {
                                    System.Diagnostics.Debug.WriteLine("Priority is HIGH, sending urgent email to receivers...");
                                    try
                                    {
                                        dalEmail.TriggerUrgentReceiverEmail(orderId, sesa_id);
                                        System.Diagnostics.Debug.WriteLine("Urgent receiver email sent successfully");
                                    }
                                    catch (Exception ex)
                                    {
                                        System.Diagnostics.Debug.WriteLine($"Urgent receiver email failed: {ex.Message}");
                                        System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                                    }
                                }
                                else
                                {
                                    System.Diagnostics.Debug.WriteLine($"Priority is '{priorityTrimmed}' (not HIGH), skipping urgent email");
                                }
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine("Priority request is NULL or empty, skipping urgent email");
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("ERROR: Order ID is 0 or negative, cannot send emails");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"=== EMAIL NOTIFICATION CRITICAL ERROR ===");
                        System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                        System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                        if (ex.InnerException != null)
                        {
                            System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                        }
                    }
                });

                return Content("success;Successfully Submitted!", "text/plain");
            }

            return Content("error;Failed to submit order.", "text/plain");
        }
        [HttpPost]
        public async Task<IActionResult> UpdateOrder(IFormFile file_support, string id_order, string material_type, string partno, string po_no, double qty, string uom, string revision, string project_name,
          string storage_requirement, string supplier_name, string order_type, double unit_price, string priority_request, double length_mm, double width_mm, double height_mm, string remark, string gatepass)
        {
            DateTime now = DateTime.Now;
            string id_upload = now.ToString("yyMMddHHmmssfff");
            string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (sesa_id == "")
            {
                return Content("Session Timeout, Please relogin!!", "text/plain");
            }
            else
            {
                var db = new DatabaseAccessLayer();
                string file_support_db = "";
                if (file_support != null && file_support.Length > 0)
                {
                    string filePath = getNextFileName(_environment.WebRootPath + "\\Documents\\" + id_upload + " - " + file_support.FileName);
                    //filePaths.Add(filePath);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file_support.CopyToAsync(stream);
                        file_support_db = Path.GetFileName(filePath);
                    }
                }
                // Menambahkan priority_request ke dalam parameter db.UpdateOrder
                string submit = db.UpdateOrder(id_order, id_upload, material_type, partno, po_no, qty, uom, revision, project_name, storage_requirement, supplier_name,
                                                order_type, unit_price, priority_request, length_mm, width_mm, height_mm, remark, gatepass, file_support_db, sesa_id);
                return Content("success;Updated Succesfully!", "text/plain");
            }
        }

        [Authorize(Policy = "RequireAny")]
        public IActionResult OrderList()
        {
            return this.CheckSession(() =>
            {
                string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                List<string> userRoles = User.Claims
                                            .Where(c => c.Type == "semb_erp_role")
                                            .Select(c => c.Value)
                                            .ToList();
                //sesa_id = "SESA126011";
                var db = new DatabaseAccessLayer();
                List<UserDetailModel> userDetail = db.GetUserDetail(sesa_id);
                List<string> listStatus = db.GetStatus();
                List<string> listPrinter = db.GetPrinter();
                string name = User.FindFirst("semb_erp_name")?.Value;
                ViewBag.name = name;
                ViewBag.sesa_id = sesa_id;
                ViewBag.listStatus = listStatus;
                ViewBag.userRoles = userRoles;
                ViewBag.listPrinter = listPrinter;
                return View(userDetail);
            });
        }
        [HttpPost]
        public IActionResult GetOrderList()
        {
            try
            {
                string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                List<string> user_roles = User.Claims
                                            .Where(c => c.Type == "semb_erp_role")
                                            .Select(c => c.Value)
                                            .ToList();
                var db = new DatabaseAccessLayer();
                List<UserDetailModel> userDetail = db.GetUserDetail(sesa_id);
                var user = userDetail.First();

                // Handle null/empty other_dept
                var departments = string.IsNullOrEmpty(user.other_dept)
                    ? new List<string>()
                    : user.other_dept.Split(',').Select(d => d.Trim()).ToList();

                // ✅ TAMBAHKAN LOG INI - Kalau tidak muncul, berarti method ini tidak dipanggil!
                System.Diagnostics.Debug.WriteLine("=== GetOrderList CALLED ===");
                System.Diagnostics.Debug.WriteLine($"User: {sesa_id}");
                System.Diagnostics.Debug.WriteLine($"Roles: {string.Join(", ", user_roles)}");

                var draw = Request.Form["draw"].FirstOrDefault();
                var start = Request.Form["start"].FirstOrDefault();
                var length = Request.Form["length"].FirstOrDefault();
                var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][data]"].FirstOrDefault();
                var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
                var searchValue = Request.Form["search[value]"].FirstOrDefault();

                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int recordsTotal = 0;

                // ✅ QUERY SUDAH BENAR (sesuai log EF yang Anda kirim)
                var mstData = (from OrderList in _context.v_order
                               where user_roles.Contains("receiver") ||
                                     user_roles.Contains("admin") ||
                                     (user_roles.Contains("requestor") &&
                                      (OrderList.pic == sesa_id ||
                                       string.IsNullOrEmpty(OrderList.pic_department) ||
                                       departments.Contains(OrderList.pic_department)))
                               select new
                               {
                                   OrderList.id_order,
                                   OrderList.material_type,
                                   OrderList.partno,
                                   OrderList.po_no,
                                   OrderList.qty,
                                   OrderList.picked_qty,
                                   OrderList.available_qty,
                                   OrderList.uom,
                                   OrderList.revision,
                                   OrderList.project_name,
                                   OrderList.storage_requirement,
                                   OrderList.supplier_name,
                                   OrderList.pic,
                                   OrderList.order_type,
                                   OrderList.length_mm,
                                   OrderList.width_mm,
                                   OrderList.height_mm,
                                   OrderList.unit_price,
                                   OrderList.priority_request,
                                   OrderList.file_support,
                                   OrderList.status_code,
                                   OrderList.status_desc,
                                   OrderList.pic_name,
                                   OrderList.pic_department,
                                   OrderList.remark,
                                   OrderList.gatepass,
                                   OrderList.record_date,
                               });

                int totalBeforeFilter = mstData.Count();
                System.Diagnostics.Debug.WriteLine($"Total BEFORE filter: {totalBeforeFilter}");

                // Sorting
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection)))
                {
                    mstData = mstData.OrderBy(sortColumn + " " + sortColumnDirection);
                }

                // Global search
                if (!string.IsNullOrEmpty(searchValue))
                {
                    mstData = mstData.Where(m => m.partno.Contains(searchValue) || m.pic.Contains(searchValue));
                }

                // Column filters
                for (int i = 0; i < 18; i++)
                {
                    var searchColVal = Request.Form["columns[" + i.ToString() + "][search][value]"];
                    var fieldName = Request.Form["columns[" + i.ToString() + "][data]"].FirstOrDefault();

                    if (!string.IsNullOrEmpty(searchColVal))
                    {
                        System.Diagnostics.Debug.WriteLine($"Filter: {fieldName} = {searchColVal}");

                        if (fieldName == "status_desc")
                            mstData = mstData.Where(m => m.status_desc.Contains(searchColVal));
                        else if (fieldName == "material_type")
                            mstData = mstData.Where(m => m.material_type.Contains(searchColVal));
                        else if (fieldName == "partno")
                            mstData = mstData.Where(m => m.partno.Contains(searchColVal));
                        else if (fieldName == "po_no")
                            mstData = mstData.Where(m => m.po_no.Contains(searchColVal));
                        else if (fieldName == "qty")
                            mstData = mstData.Where(m => m.qty.ToString().Contains(searchColVal));
                        else if (fieldName == "picked_qty")
                            mstData = mstData.Where(m => m.picked_qty.ToString().Contains(searchColVal));
                        else if (fieldName == "available_qty")
                            mstData = mstData.Where(m => m.available_qty.ToString().Contains(searchColVal));
                        else if (fieldName == "uom")
                            mstData = mstData.Where(m => m.uom.Contains(searchColVal));
                        else if (fieldName == "revision")
                            mstData = mstData.Where(m => m.revision.Contains(searchColVal));
                        else if (fieldName == "project_name")
                            mstData = mstData.Where(m => m.project_name.Contains(searchColVal));
                        else if (fieldName == "storage_requirement")
                            mstData = mstData.Where(m => m.storage_requirement.Contains(searchColVal));
                        else if (fieldName == "supplier_name")
                            mstData = mstData.Where(m => m.supplier_name.Contains(searchColVal));
                        else if (fieldName == "pic_name")
                            mstData = mstData.Where(m => m.pic_name.Contains(searchColVal));
                        else if (fieldName == "order_type")
                            mstData = mstData.Where(m => m.order_type.Contains(searchColVal));
                        else if (fieldName == "unit_price")
                            mstData = mstData.Where(m => m.unit_price.ToString().Contains(searchColVal));
                        else if (fieldName == "priority_request")
                            mstData = mstData.Where(m => m.priority_request.Contains(searchColVal));
                        else if (fieldName == "gatepass")
                            mstData = mstData.Where(m => m.gatepass != null && m.gatepass.Contains(searchColVal));
                    }
                }

                recordsTotal = mstData.Count();
                System.Diagnostics.Debug.WriteLine($"Total AFTER filter: {recordsTotal}");

                var data = mstData.Skip(skip).Take(pageSize).ToList();

                System.Diagnostics.Debug.WriteLine($"Returning {data.Count} records");
                System.Diagnostics.Debug.WriteLine("=========================");

                var jsonData = new
                {
                    draw = draw,
                    recordsFiltered = recordsTotal,
                    recordsTotal = recordsTotal,
                    data = data
                };

                return Ok(jsonData);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack: {ex.StackTrace}");
                return StatusCode(500, new { error = ex.Message });
            }
        }
        [HttpGet]
        public JsonResult GetUnreadDiscussionCount()
        {
            try
            {
                string fullIdentity = User.Identity?.Name ?? "";
                string sesaId = fullIdentity.Contains("\\") ? fullIdentity.Split('\\')[1] : fullIdentity;
                if (string.IsNullOrEmpty(sesaId))
                {
                    sesaId = User.FindFirstValue("sesa_id") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
                }
                if (string.IsNullOrEmpty(sesaId))
                {
                    return Json(0); // ✅ return plain angka
                }

                DatabaseAccessLayer dal = new DatabaseAccessLayer();
                int unreadCount = dal.GetUnreadDiscussionCount(sesaId);
                return Json(unreadCount); // ✅ return plain angka
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread discussion count");
                return Json(0); // ✅ return plain angka
            }
        }
        [Authorize(Policy = "RequireRequestor")]
        [HttpGet]
        public IActionResult TemplateList()
        {
            string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            string name = User.FindFirst(ClaimTypes.Name)?.Value;

            ViewBag.sesa_id = sesa_id;
            ViewBag.name = name;

            return View();
        }

        [Authorize(Policy = "RequireRequestor")]
        [HttpPost]
        public IActionResult DeleteTemplate(int id_template)
        {
            try
            {
                string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var db = new DatabaseAccessLayer();
                string result = db.DeleteOrderTemplate(id_template, sesa_id);
                return Content(result, "text/plain");
            }
            catch (Exception ex)
            {
                return Content("error;" + ex.Message, "text/plain");
            }
        }
        [Authorize(Policy = "RequireRequestor")]
        [HttpGet]
        public IActionResult DownloadTemplate(int id_template)
        {
            string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            try
            {
                var db = new DatabaseAccessLayer();
                var template = db.GetOrderTemplateDetail(id_template, sesa_id);
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Order Template");
                    string[] headers = {
                "Material Type*", "Part No*", "PO No*", "Qty*", "UOM*", "Revision*",
                "Project Name*", "Storage Requirement*", "Supplier Name*", "Order Type*",
                "Price ($)*", "Priority Request*", "Length (mm)", "Width (mm)", "Height (mm)", "Remark", "Gatepass"
            };

                    for (int i = 0; i < headers.Length; i++)
                    {
                        worksheet.Cell(1, i + 1).Value = headers[i];
                    }

                    worksheet.Cell(2, 1).Value = template.material_type;
                    worksheet.Cell(2, 2).Value = template.partno;
                    worksheet.Cell(2, 3).Value = template.po_no;
                    worksheet.Cell(2, 4).Value = template.qty;
                    worksheet.Cell(2, 5).Value = template.uom;
                    worksheet.Cell(2, 6).Value = template.revision;
                    worksheet.Cell(2, 7).Value = template.project_name;
                    worksheet.Cell(2, 8).Value = template.storage_requirement;
                    worksheet.Cell(2, 9).Value = template.supplier_name;
                    worksheet.Cell(2, 10).Value = template.order_type;
                    worksheet.Cell(2, 11).Value = template.unit_price;
                    worksheet.Cell(2, 12).Value = template.priority_request;
                    worksheet.Cell(2, 13).Value = template.length_mm;
                    worksheet.Cell(2, 14).Value = template.width_mm;
                    worksheet.Cell(2, 15).Value = template.height_mm;
                    worksheet.Cell(2, 16).Value = template.remark;
                    worksheet.Cell(2, 17).Value = template.gatepass;

                    worksheet.Columns().AdjustToContents();

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        var content = stream.ToArray();
                        string fileName = $"Template_{template.partno}_{DateTime.Now:yyyyMMdd}.xlsx";
                        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                    }
                }
            }
            catch (Exception ex)
            {
                return Content("Error: " + ex.Message);
            }
        }


        [Authorize(Policy = "RequireRequestor")]
        [HttpGet]
        public IActionResult DownloadOrderDetail(int id_order)
        {
            try
            {
                var db = new DatabaseAccessLayer();
                var order = db.GetOrderDetail(id_order);

                if (order == null)
                {
                    return Content("Error: Data tidak ditemukan.");
                }

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Order Detail");
                    string[] headers = {
                "Material Type*", "Part No*", "PO No*", "Qty*", "UOM*", "Revision*",
                "Project Name*", "Storage Requirement*", "Supplier Name*", "Order Type*",
                "Price ($)*", "Priority Request*", "Length (mm)", "Width (mm)", "Height (mm)", "Remark", "Gatepass"
            };

                    for (int i = 0; i < headers.Length; i++)
                    {
                        worksheet.Cell(1, i + 1).Value = headers[i];
                    }

                    worksheet.Cell(2, 1).Value = order.material_type;
                    worksheet.Cell(2, 2).Value = order.partno;
                    worksheet.Cell(2, 3).Value = order.po_no;
                    worksheet.Cell(2, 4).Value = order.qty;
                    worksheet.Cell(2, 5).Value = order.uom;
                    worksheet.Cell(2, 6).Value = order.revision;
                    worksheet.Cell(2, 7).Value = order.project_name;
                    worksheet.Cell(2, 8).Value = order.storage_requirement;
                    worksheet.Cell(2, 9).Value = order.supplier_name;
                    worksheet.Cell(2, 10).Value = order.order_type;
                    worksheet.Cell(2, 11).Value = order.unit_price;
                    worksheet.Cell(2, 12).Value = order.priority_request;
                    worksheet.Cell(2, 13).Value = order.length_mm;
                    worksheet.Cell(2, 14).Value = order.width_mm;
                    worksheet.Cell(2, 15).Value = order.height_mm;
                    worksheet.Cell(2, 16).Value = order.remark;
                    worksheet.Cell(2, 17).Value = order.gatepass;

                    worksheet.Columns().AdjustToContents();

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        var content = stream.ToArray();
                        string fileName = $"Order_{order.partno}_{DateTime.Now:yyyyMMdd}.xlsx";
                        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                    }
                }
            }
            catch (Exception ex)
            {
                return Content("Error: " + ex.Message);
            }
        }

        [HttpGet]
        public IActionResult ExportOrderList()
        {
            using (XLWorkbook wb = new XLWorkbook())
            {

                DateTime currentDateTime = DateTime.Now;
                string formattedDateTime = currentDateTime.ToString("yyyyMMddHHmmss");

                var db = new DatabaseAccessLayer();
                System.Data.DataTable dt = db.GetExportOrderList().Tables[0];
                wb.Worksheets.Add(dt);
                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Putaway Order.xlsx");
                }
            }
        }
        public IActionResult GetDataGR(string id_order_string)
        {
            string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (sesa_id == "")
            {
                return Content("Session Timeout, Please relogin!!", "text/plain");
            }
            else
            {
                var db = new DatabaseAccessLayer();
                List<OrderListModel> dataGR = db.GetDataGR(id_order_string);

                return PartialView("_TableGR", dataGR);
            }
        }
        [HttpPost]
        public IActionResult SubmitGR(string id_order_spq_string)
        {
            string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(sesa_id))
            {
                return Content("Session Timeout, Please relogin!!", "text/plain");
            }

            var db = new DatabaseAccessLayer();
            string result = db.SubmitGR(id_order_spq_string, sesa_id);

            // Cek apakah database mengembalikan pesan sukses "OK"
            if (result.StartsWith("OK"))
            {
                // Jalankan Task background untuk kirim email agar aplikasi tidak lambat
                _ = Task.Run(() =>
                {
                    try
                    {
                        var pairs = id_order_spq_string.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var pair in pairs)
                        {
                            var parts = pair.Split(',');
                            if (parts.Length > 0 && int.TryParse(parts[0], out int id_order))
                            {
                                var dalEmail = new DatabaseAccessLayer();
                                // Status Code 2 = Received
                                dalEmail.TriggerStatusUpdateEmail(id_order, sesa_id, 2);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Email Error: {ex.Message}");
                    }
                });
            }

            return Content(result, "text/plain");
        }


        [HttpGet]
        public async Task<IActionResult> GetScheduleCount()
        {
            try
            {
                // Ambil SESA ID dari claim user login
                string sesaId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(sesaId)) return Json(0);

                // ✅ HITUNG SCHEDULE YANG SUDAH DINOTIFIKASI TAPI BELUM COMPLETED
                var count = await _context.ScheduledRequests
                    .Where(s => s.SesaId == sesaId
                             && s.IsNotified == true      // ✅ Sudah dinotifikasi (email sudah dikirim)
                             && s.IsCompleted == false)   // ✅ Belum completed
                    .CountAsync();

                return Json(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching schedule count");
                return Json(0);
            }
        }
        public IActionResult GetGRDetail(int id_gr)
        {
            string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (sesa_id == "")
            {
                return Content("Session Timeout, Please relogin!!", "text/plain");
            }
            else
            {
                var db = new DatabaseAccessLayer();
                List<OrderListModel> dataGR = db.GetGRDetail(id_gr);
                List<string> listPrinter = db.GetPrinter();
                ViewBag.id_gr = id_gr;
                ViewBag.listPrinter = listPrinter;
                //return Content("Upload Success!!", "text/plain");

                return PartialView("_TableGRDetail", dataGR);
            }
        }

        [Authorize(Policy = "RequireAny")]
        public IActionResult StoragebinList()
        {
            return this.CheckSession(() =>
            {
                string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                List<string> userRoles = User.Claims
                                            .Where(c => c.Type == "semb_erp_role")
                                            .Select(c => c.Value)
                                            .ToList();
                //sesa_id = "SESA126011";
                var db = new DatabaseAccessLayer();
                List<UserDetailModel> userDetail = db.GetUserDetail(sesa_id);
                List<string> listStatus = db.GetStatus();
                List<string> listPrinter = db.GetPrinter();
                string name = User.FindFirst("semb_erp_name")?.Value;
                ViewBag.name = name;
                ViewBag.sesa_id = sesa_id;
                ViewBag.listStatus = listStatus;
                ViewBag.userRoles = userRoles;
                ViewBag.listPrinter = listPrinter;
                return View(userDetail);
            });
        }

        public IActionResult GetStoragebinList()
        {
            try
            {
                string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                List<string> user_roles = User.Claims
                                            .Where(c => c.Type == "semb_erp_role")
                                            .Select(c => c.Value)
                                            .ToList();
                var db = new DatabaseAccessLayer();
                List<UserDetailModel> userDetail = db.GetUserDetail(sesa_id);
                var user = userDetail.First();

                var draw = Request.Form["draw"].FirstOrDefault();
                var start = Request.Form["start"].FirstOrDefault();
                var length = Request.Form["length"].FirstOrDefault();
                //var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
                var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][data]"].FirstOrDefault();
                var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
                var searchValue = Request.Form["search[value]"].FirstOrDefault();
                var column0Value = Request.Form["columns[0][search][value]"];
                var column1Value = Request.Form["columns[1][search][value]"];
                var column2Value = Request.Form["columns[2][search][value]"];
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int recordsTotal = 0;

                var departments = user.other_dept.Split(',').Select(d => d.Trim()).ToList();

                var mstData = (from StoragebinList in _context.v_order_sbin
                               where user_roles.Contains("receiver") || user_roles.Contains("admin") || user_roles.Contains("requestor")
                               select
                                   new
                                   {
                                       StoragebinList.id_order,
                                       StoragebinList.material_type,
                                       StoragebinList.partno,
                                       StoragebinList.po_no,
                                       StoragebinList.storage_bin,
                                       StoragebinList.box_id,
                                       StoragebinList.qty,
                                       StoragebinList.picked_qty,
                                       StoragebinList.available_qty,
                                       StoragebinList.uom,
                                       StoragebinList.revision,
                                       StoragebinList.project_name,
                                       StoragebinList.storage_requirement,
                                       StoragebinList.supplier_name,
                                       StoragebinList.pic,
                                       StoragebinList.order_type,
                                       StoragebinList.length_mm,
                                       StoragebinList.width_mm,
                                       StoragebinList.height_mm,
                                       StoragebinList.unit_price,
                                       StoragebinList.file_support,
                                       StoragebinList.status_code,
                                       StoragebinList.status_desc,
                                       StoragebinList.pic_name,
                                       StoragebinList.pic_department,
                                       StoragebinList.remark,
                                       StoragebinList.gatepass,
                                       StoragebinList.record_date
                                   });

                //var mstData = (from temp in _context.mst_material_plant select temp);
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection)))
                {
                    mstData = mstData.OrderBy(sortColumn + " " + sortColumnDirection);
                }
                if (!string.IsNullOrEmpty(searchValue))
                {
                    mstData = mstData.Where(m => m.partno.Contains(searchValue)
                                                || m.pic.Contains(searchValue));
                }
                for (int i = 0; i < 18; i++)
                {
                    var searchColVal = Request.Form["columns[" + i.ToString() + "][search][value]"];
                    var fieldName = Request.Form["columns[" + i.ToString() + "][data]"].FirstOrDefault();
                    if (!string.IsNullOrEmpty(searchColVal))
                    {
                        if (fieldName == "status_desc")
                        {
                            mstData = mstData.Where(m => m.status_desc.Contains(searchColVal));
                        }
                        else if (fieldName == "material_type")
                        {
                            mstData = mstData.Where(m => m.material_type.Contains(searchColVal));
                        }
                        else if (fieldName == "partno")
                        {
                            mstData = mstData.Where(m => m.partno.Contains(searchColVal));
                        }
                        else if (fieldName == "po_no")
                        {
                            mstData = mstData.Where(m => m.po_no.Contains(searchColVal));
                        }
                        else if (fieldName == "storage_bin")
                        {
                            mstData = mstData.Where(m => m.storage_bin.Contains(searchColVal));
                        }
                        else if (fieldName == "box_id")
                        {
                            mstData = mstData.Where(m => m.box_id.Contains(searchColVal));
                        }
                        else if (fieldName == "qty")
                        {
                            mstData = mstData.Where(m => m.qty.ToString().Contains(searchColVal));
                        }
                        else if (fieldName == "picked_qty")
                        {
                            mstData = mstData.Where(m => m.picked_qty.ToString().Contains(searchColVal));
                        }
                        else if (fieldName == "available_qty")
                        {
                            mstData = mstData.Where(m => m.available_qty.ToString().Contains(searchColVal));
                        }
                        else if (fieldName == "uom")
                        {
                            mstData = mstData.Where(m => m.uom.Contains(searchColVal));
                        }
                        else if (fieldName == "revision")
                        {
                            mstData = mstData.Where(m => m.revision.Contains(searchColVal));
                        }
                        else if (fieldName == "project_name")
                        {
                            mstData = mstData.Where(m => m.project_name.Contains(searchColVal));
                        }
                        else if (fieldName == "storage_requirement")
                        {
                            mstData = mstData.Where(m => m.storage_requirement.Contains(searchColVal));
                        }
                        else if (fieldName == "supplier_name")
                        {
                            mstData = mstData.Where(m => m.supplier_name.Contains(searchColVal));
                        }
                        else if (fieldName == "pic_name")
                        {
                            mstData = mstData.Where(m => m.pic_name.Contains(searchColVal));
                         }
                        else if (fieldName == "order_type")
                        {
                            mstData = mstData.Where(m => m.order_type.Contains(searchColVal));
                        }
                        else if (fieldName == "unit_price")
                        {
                            mstData = mstData.Where(m => m.unit_price.ToString().Contains(searchColVal));
                        }
                        else if (fieldName == "gatepass")
                        {
                            mstData = mstData.Where(m => m.gatepass.Contains(searchColVal));
                        }
                    }
                }
                recordsTotal = mstData.Count();
                var data = mstData.Skip(skip).Take(pageSize).ToList();
                var jsonData = new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data };
                return Ok(jsonData);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpGet]
         public IActionResult ExportStoragebinList()
         {
            using (XLWorkbook wb = new XLWorkbook())
            {

                DateTime currentDateTime = DateTime.Now;
                string formattedDateTime = currentDateTime.ToString("yyyyMMddHHmmss");

                var db = new DatabaseAccessLayer();
                System.Data.DataTable dt = db.GetExportStoragebinList().Tables[0];
                wb.Worksheets.Add(dt);
                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Storage Bin List.xlsx");
                }
            }
        }

        [Authorize(Policy = "RequireReceiver")]
        public IActionResult AddNonConformity()
        {
            return this.CheckSession(() =>
            {
                var db = new DatabaseAccessLayer();
                string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                string name = User.FindFirst("semb_erp_name")?.Value;
                string plant = db.GetUserPlant(sesa_id);
                List<string> catList = db.GET_CAT_NON_CONF();
                //List<string> supplierList = db.GetSupplierList();
                //ViewBag.supplierList = supplierList;
                ViewBag.name = name;
                ViewBag.sesa_id = sesa_id;
                ViewBag.plant = plant;
                ViewBag.catList = catList;
                return View();
            });
        }

        [HttpPost]
        public async Task<IActionResult> SUBMIT_NON_CONF(IFormFile file_support, string material_type, string partno, string po_no, double qty, string uom,
            string supplier_name, string pic, string category_issue, string detail_issue)
        {
            DateTime now = DateTime.Now;
            string id_upload = now.ToString("yyMMddHHmmssfff");
            string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (sesa_id == "")
            {
                return Content("Session Timeout, Please relogin!!", "text/plain");
            }
            else
            {
                var db = new DatabaseAccessLayer();
                string file_support_non_conf = "";
                if (file_support != null && file_support.Length > 0)
                {
                    string filePath = getNextFileName(_environment.WebRootPath + "\\Documents\\Non_Conf\\" + material_type + " - " + file_support.FileName);
                    //filePaths.Add(filePath);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file_support.CopyToAsync(stream);
                        file_support_non_conf = Path.GetFileName(filePath);
                    }
                }
                string submit = db.SUBMIT_NON_CONF(material_type, partno, po_no, qty, uom, supplier_name,
                                                pic, category_issue, detail_issue, file_support_non_conf, sesa_id);
                return Content("success;Succesfully Submitted!", "text/plain");
            }
        }

        public IActionResult NonConfList()
        {
            return this.CheckSession(() =>
            {
                var db = new DatabaseAccessLayer();
                string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                string name = User.FindFirst("semb_erp_name")?.Value;
                string plant = db.GetUserPlant(sesa_id);
                List<string> userRoles = User.Claims
                                            .Where(c => c.Type == "semb_erp_role")
                                            .Select(c => c.Value)
                                            .ToList();
                List<string> catList = db.GET_CAT_NON_CONF();
                ViewBag.catList = catList;
                ViewBag.name = name;
                ViewBag.sesa_id = sesa_id;
                ViewBag.plant = plant;
                ViewBag.userRoles = userRoles;
                return View();
            });
        }


        public IActionResult DashboardReceiver()
        {
            string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var db = new DatabaseAccessLayer();
            var userDetail = db.GetUserDetail(sesa_id);
            var userRoles = db.GetUserRole(sesa_id);
            ViewBag.sesa_id = sesa_id;
            ViewBag.name = userDetail.FirstOrDefault()?.name ?? "Guest";
            ViewBag.userRoles = userRoles; return View(userDetail);
        }
        public IActionResult GET_NON_CONF_LIST()
        {
            try
            {
                string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                List<string> user_roles = User.Claims
                                            .Where(c => c.Type == "semb_erp_role")
                                            .Select(c => c.Value)
                                            .ToList();
                var db = new DatabaseAccessLayer();
                List<UserDetailModel> userDetail = db.GetUserDetail(sesa_id);
                var user = userDetail.First();

                var draw = Request.Form["draw"].FirstOrDefault();
                var start = Request.Form["start"].FirstOrDefault();
                var length = Request.Form["length"].FirstOrDefault();
                //var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
                var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][data]"].FirstOrDefault();
                var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
                var searchValue = Request.Form["search[value]"].FirstOrDefault();
                var column0Value = Request.Form["columns[0][search][value]"];
                var column1Value = Request.Form["columns[1][search][value]"];
                var column2Value = Request.Form["columns[2][search][value]"];
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int recordsTotal = 0;
                var mstData = (from NonConfList in _context.V_NON_CONF
                               where NonConfList.created_by_sesa == sesa_id
   || user_roles.Contains("receiver")
   || user_roles.Contains("admin")
   || (user_roles.Contains("requestor") && NonConfList.pic_sesa == sesa_id)
                               select
                                   new
                                   {
                                       NonConfList.id_non_conf,
                                       NonConfList.material_type,
                                       NonConfList.partno,
                                       NonConfList.po_no,
                                       NonConfList.qty,
                                       NonConfList.uom,
                                       NonConfList.supplier_name,
                                       NonConfList.pic,
                                       NonConfList.category_issue,
                                       NonConfList.detail_issue,
                                       NonConfList.file_doc,
                                       NonConfList.is_close,
                                       NonConfList.created_by,
                                       NonConfList.close_date,
                                       NonConfList.closed_by,
                                       NonConfList.close_comment
                                   });

                //var mstData = (from temp in _context.mst_material_plant select temp);
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection)))
                {
                    mstData = mstData.OrderBy(sortColumn + " " + sortColumnDirection);
                }
                if (!string.IsNullOrEmpty(searchValue))
                {
                    mstData = mstData.Where(m => m.partno.Contains(searchValue)
                                                || m.pic.Contains(searchValue));
                }

                for (int i = 0; i < 9; i++)
                {
                    var searchColVal = Request.Form["columns[" + i.ToString() + "][search][value]"];
                    var fieldName = Request.Form["columns[" + i.ToString() + "][data]"].FirstOrDefault();
                    if (!string.IsNullOrEmpty(searchColVal))
                    {
                        if (fieldName == "material_type")
                        {
                            mstData = mstData.Where(m => m.material_type.Contains(searchColVal));
                        }
                        else if (fieldName == "partno")
                        {
                            mstData = mstData.Where(m => m.partno.Contains(searchColVal));
                        }
                        else if (fieldName == "po_no")
                        {
                            mstData = mstData.Where(m => m.po_no.Contains(searchColVal));
                        }
                        else if (fieldName == "qty")
                        {
                            mstData = mstData.Where(m => m.qty.ToString().Contains(searchColVal));
                        }
                        else if (fieldName == "uom")
                        {
                            mstData = mstData.Where(m => m.uom.Contains(searchColVal));
                        }
                        else if (fieldName == "supplier_name")
                        {
                            mstData = mstData.Where(m => m.supplier_name.Contains(searchColVal));
                        }
                        else if (fieldName == "pic")
                        {
                            mstData = mstData.Where(m => m.pic.Contains(searchColVal));
                        }
                        else if (fieldName == "category_issue")
                        {
                            mstData = mstData.Where(m => m.category_issue.Contains(searchColVal));
                        }
                        else if (fieldName == "detail_issue")
                        {
                            mstData = mstData.Where(m => m.detail_issue.Contains(searchColVal));
                        }
                    }
                }

                recordsTotal = mstData.Count();
                var data = mstData.Skip(skip).Take(pageSize).ToList();
                var jsonData = new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data };
                return Ok(jsonData);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpGet]
        public IActionResult GetRequestListByStatus(string status)
        {
            try
            {
                status = status?.Trim() ?? "";
                var cleanStatus = System.Text.RegularExpressions.Regex.Replace(status, @"\s+\d+$", "").Trim();

                string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                List<string> user_roles = User.Claims
                                            .Where(c => c.Type == "semb_erp_role")
                                            .Select(c => c.Value)
                                            .ToList();
                var db = new DatabaseAccessLayer();
                List<UserDetailModel> userDetail = db.GetUserDetail(sesa_id);
                var user = userDetail.First();

                // ✅ Cast ke object supaya compatible dengan List<dynamic>
                var requestData = (from req in _context.v_request
                                   where (req.requested_by == sesa_id ||
                                          user_roles.Contains("admin") ||
                                          user_roles.Contains("receiver"))
                                   && req.status_desc.ToUpper() == cleanStatus.ToUpper()
                                   orderby req.record_date descending
                                   select new
                                   {
                                       id_request = req.id_request,
                                       request_no = req.request_no != null ? req.request_no.ToString() : "",
                                       status_desc = req.status_desc != null ? req.status_desc.ToString() : "",
                                       requested_by_name = req.requested_by_name != null ? req.requested_by_name.ToString() : "",
                                       record_date = req.record_date != null ? req.record_date.Value.ToString("dd-MM-yyyy HH:mm") : ""
                                   })
                                   .ToList()
                                   .Cast<object>() // ✅ INI YANG PENTING
                                   .ToList();

                ViewBag.StatusFilter = cleanStatus;
                ViewBag.TotalRecords = requestData.Count;
                ViewBag.UserRoles = user_roles;
                ViewBag.SesaId = sesa_id;

                return PartialView("_RequestListByStatusPartial", requestData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        public IActionResult DashboardRequestor()
        {
            string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var db = new DatabaseAccessLayer();
            var userDetail = db.GetUserDetail(sesa_id);
            var userRoles = db.GetUserRole(sesa_id);
            ViewBag.sesa_id = sesa_id;
            ViewBag.name = userDetail.FirstOrDefault()?.name ?? "Guest";
            ViewBag.userRoles = userRoles;
            return View(userDetail);
        }


        [HttpPost]
        public IActionResult GetReceiverSummary()
        {
            try
            {
                string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // --- 1. DATA UNTUK BOX DASHBOARD (v_order) ---
                var baseQuery = _context.v_order.AsQueryable();

                int submission = baseQuery.Count(o => o.status_desc.ToUpper() == "SUBMISSION");
                int received = baseQuery.Count(o => o.status_desc.ToUpper() == "RECEIVED");
                int putaway = baseQuery.Count(o => o.status_desc.ToUpper() == "PUTAWAY");
                int partiallyPicked = baseQuery.Count(o => o.status_desc.ToUpper() == "PARTIALLY PICKED");
                int closed = baseQuery.Count(o => o.status_desc.ToUpper() == "CLOSED");

                // --- 2. DATA UNTUK TABEL DASHBOARD (Hanya 5 Variance terbaru) ---
                // Mengganti v_request menjadi v_picking_variance
                var recentVariance = (from v in _context.v_picking_variance
                                      orderby v.record_date descending
                                      select new
                                      {
                                          v.partno,
                                          v.sbin,
                                          v.qty,
                                          v.request_no,
                                          v.name,
                                          record_date = v.record_date.HasValue
                                              ? v.record_date.Value.ToString("dd MMM yyyy HH:mm")
                                              : "-"
                                      }).Take(5).ToList();

                // --- 3. DATA UNTUK PIE CHART ---
                var pickingStatusStats = _context.v_request
                    .GroupBy(r => r.status_desc)
                  .Select(g => new {
                      label = g.Key.ToUpper(),
                      count = g.Count()
                  }).ToList();

                var result = new
                {
                    totalRequest = baseQuery.Count(),
                    submission = submission,
                    received = received,
                    putaway = putaway,
                    partiallyPicked = partiallyPicked,
                    closed = closed,
                    recentVariance = recentVariance, // Data variance baru
                    pickingStatusStats = pickingStatusStats
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult GetBlockBinDetail(string partno)
        {
            try
            {
                if (string.IsNullOrEmpty(partno))
                {
                    ViewBag.ErrorMessage = "Part number is required.";
                    return PartialView("_BlockBinDetail", new List<BlockBinDetailModel>());
                }

                // Get data from view for specific partno
                var blockBinData = _context.v_block_bin
                    .Where(b => b.partno == partno)
                    .Select(b => new BlockBinDetailModel
                    {
                        no = b.no,
                        partno = b.partno,
                        sbin = b.sbin,
                        variance_date = b.variance_date
                    })
                    .OrderByDescending(b => b.variance_date)
                    .ToList();

                ViewBag.PartNo = partno;
                ViewBag.TotalRecords = blockBinData.Count;

                return PartialView("_BlockBinDetail", blockBinData);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("_BlockBinDetail", new List<BlockBinDetailModel>());
            }
        }

        [HttpGet]
        public IActionResult GetReservationListByStatus(string status)
        {
            try
            {
                string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var db = new DatabaseAccessLayer();

                // Create form collection dengan filter status
                var formCollection = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            { "draw", "1" },
            { "start", "0" },
            { "length", "1000" }, // Get all data
            { "columns[0][search][value]", status }, // Filter by status
            { "search[value]", "" }
        });

                var result = db.GetReservationList(formCollection, sesa_id);

                // Extract data list
                var dataList = ((dynamic)result).data as List<MaterialReservationModel>;

                ViewBag.Status = status;
                ViewBag.TotalRecords = dataList?.Count ?? 0;

                return PartialView("_ReservationListByStatus", dataList);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("_ReservationListByStatus", new List<MaterialReservationModel>());
            }
        }

        [HttpGet]
        public IActionResult GetReturnListByStatus(string status)
        {
            try
            {
                var db = new DatabaseAccessLayer();

                // Create form collection dengan filter status
                var formCollection = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            { "draw", "1" },
            { "start", "0" },
            { "length", "1000" }, // Get all data
            { "columns[0][search][value]", status }, // Filter by status
            { "search[value]", "" }
        });

                var result = db.GetReturnList(formCollection);

                // Extract data list
                var dataList = ((dynamic)result).data as List<MaterialReturnModel>;

                ViewBag.Status = status;
                ViewBag.TotalRecords = dataList?.Count ?? 0;

                return PartialView("_ReturnListByStatus", dataList);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("_ReturnListByStatus", new List<MaterialReturnModel>());
            }
        }


        [HttpPost]
        public IActionResult DeleteReturn(int id)
        {
            try
            {
                var db = new DatabaseAccessLayer();
                string result = db.DeleteMaterialReturn(id);

                if (result == "OK")
                {
                    return Content("OK");
                }
                else
                {
                    return Content(result);
                }
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }

        [HttpGet]
        public IActionResult GetOrderListByStatus(string status)
        {
            try
            {
                // 1. Ambil identitas user dari Claims
                string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                List<string> user_roles = User.Claims
                                              .Where(c => c.Type == "semb_erp_role")
                                              .Select(c => c.Value)
                                              .ToList();

                if (string.IsNullOrEmpty(sesa_id))
                {
                    return Unauthorized("Sesi anda berakhir atau ID User tidak ditemukan.");
                }

                // 2. Ambil detail departemen user
                var db = new DatabaseAccessLayer();
                List<UserDetailModel> userDetail = db.GetUserDetail(sesa_id);

                if (userDetail == null || !userDetail.Any())
                {
                    return BadRequest("Detail user tidak ditemukan di database.");
                }

                var user = userDetail.First();
                var departments = string.IsNullOrEmpty(user.other_dept)
                    ? new List<string>()
                    : user.other_dept.Split(',').Select(d => d.Trim()).ToList();

                // 3. Query Dasar (Filter by Status)
                var query = _context.v_order.Where(o => o.status_desc == status);

                // 4. Logika Hak Akses (Filtering Data)
                // Jika user BUKAN admin dan BUKAN receiver, maka dia adalah requestor murni
                // Kita batasi datanya hanya yang dia buat (PIC) atau departemennya sama
                if (!user_roles.Contains("admin") && !user_roles.Contains("receiver"))
                {
                    query = query.Where(o => o.pic == sesa_id ||
                                             departments.Contains(o.pic_department));
                }

                // 5. Mapping ke ViewModel
                var orderData = query.Select(OrderList => new OrderListModel
                {
                    id_order = OrderList.id_order,
                    material_type = OrderList.material_type,
                    partno = OrderList.partno,
                    po_no = OrderList.po_no,
                    qty = OrderList.qty,
                    picked_qty = OrderList.picked_qty,
                    available_qty = OrderList.available_qty,
                    uom = OrderList.uom,
                    revision = OrderList.revision,
                    project_name = OrderList.project_name,
                    storage_requirement = OrderList.storage_requirement,
                    supplier_name = OrderList.supplier_name,
                    pic = OrderList.pic,
                    pic_name = OrderList.pic_name,
                    pic_department = OrderList.pic_department,
                    order_type = OrderList.order_type,
                    length_mm = OrderList.length_mm,
                    width_mm = OrderList.width_mm,
                    height_mm = OrderList.height_mm,
                    unit_price = OrderList.unit_price,
                    priority_request = OrderList.priority_request,
                    file_support = OrderList.file_support,
                    status_code = OrderList.status_code,
                    status_desc = OrderList.status_desc,
                    remark = OrderList.remark,
                    gatepass = OrderList.gatepass,
                    record_date = OrderList.record_date
                }).ToList();

                // 6. Data pendukung untuk View/Modal
                ViewBag.StatusFilter = status;
                ViewBag.TotalRecords = orderData.Count;
                ViewBag.UserRoles = user_roles;
                ViewBag.SesaId = sesa_id;

                return PartialView("_OrderListByStatusPartial", orderData);
            }
            catch (Exception ex)
            {
                // Log error secara internal
                System.Diagnostics.Debug.WriteLine($"ERROR in GetOrderListByStatus: {ex.Message}");
                return StatusCode(500, new { error = "Terjadi kesalahan internal: " + ex.Message });
            }
        }

        [HttpGet]
        public IActionResult GetCatNonConf(string search_value)
        {
            var db = new DatabaseAccessLayer();
            List<CategoryModel> listCAT = db.GetCatNonConf(search_value);
            return Json(new { items = listCAT });
        }

        [HttpPost]
        public async Task<IActionResult> UPDATE_NON_CONF(IFormFile file_support, string id_non_conf, string material_type, string partno, string po_no, double qty, string uom,
        string supplier_name, string pic, string category_issue, string detail_issue)
        {
            DateTime now = DateTime.Now;
            string id_upload = now.ToString("yyMMddHHmmssfff");
            string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (sesa_id == "")
            {
                return Content("Session Timeout, Please relogin!!", "text/plain");
            }
            else
            {
                var db = new DatabaseAccessLayer();
                string file_support_non_conf = "";
                if (file_support != null && file_support.Length > 0)
                {
                    string filePath = getNextFileName(_environment.WebRootPath + "\\Upload\\Non_Conf\\" + material_type + " - " + file_support.FileName);
                    //filePaths.Add(filePath);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file_support.CopyToAsync(stream);
                        file_support_non_conf = Path.GetFileName(filePath);
                    }
                }
                string submit = db.UPDATE_NON_CONF(id_non_conf, material_type, partno, po_no, qty, uom, supplier_name,
                                                pic, category_issue, detail_issue, file_support_non_conf, sesa_id);
                return Content("success;Updated Succesfully!", "text/plain");
            }
        }

        [Authorize(Policy = "RequireReceiver")]
        public IActionResult Putaway()
        {
            return this.CheckSession(() =>
            {
                var db = new DatabaseAccessLayer();
                string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                string name = User.FindFirst("semb_erp_name")?.Value;
                string plant = db.GetUserPlant(sesa_id);
                List<string> catList = db.GET_CAT_NON_CONF();
                //List<string> supplierList = db.GetSupplierList();
                //ViewBag.supplierList = supplierList;
                ViewBag.name = name;
                ViewBag.sesa_id = sesa_id;
                ViewBag.plant = plant;
                ViewBag.catList = catList;
                return View();
            });
        }
        [HttpGet]
        public IActionResult GetPartBin(string box_id)
        {
            var db = new DatabaseAccessLayer();
            List<PartModel> listBin = db.GetPartBin(box_id);
            return PartialView("_TablePartBin", listBin);
            //return Json(new { status = "OK" });
        }

        [Authorize(Policy = "RequireReceiver")]
        public IActionResult BinMaterial(string binId) //add string erpId
        {
            var referrer = Request.Headers["Referer"].ToString();
            if (!referrer.Contains("/User/Putaway")) // Check if the referrer is the expected page
            {
                return RedirectToAction("Putaway"); // Redirect to a safe page
            }

            return this.CheckSession(() =>
            {
                var db = new DatabaseAccessLayer();
                string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                string name = User.FindFirst("semb_erp_name")?.Value;
                string plant = db.GetUserPlant(sesa_id);
                List<string> catList = db.GET_CAT_NON_CONF();
                //List<string> supplierList = db.GetSupplierList();
                //ViewBag.supplierList = supplierList;
                ViewBag.name = name;
                ViewBag.sesa_id = sesa_id;
                ViewBag.plant = plant;
                ViewBag.catList = catList;
                ViewBag.binId = binId;
                //Viewbag.erpId = erpId;
                return View();
            });
        }

        [HttpGet]
        public IActionResult GetBinItem(string box_id)
        {
            var db = new DatabaseAccessLayer();
            List<PartModel> listBin = db.GetBinItem(box_id);

            // Transform the list into a format suitable for DataTables
            var result = listBin.Select(item => new
            {
                PartNo = item.partno,
                PartName = item.partname,
                Quantity = item.qty
            });

            return Json(result);
        }

        [HttpPost]
        public IActionResult InsertBinItem(string box_id, string sesa_id)
        {
            var db = new DatabaseAccessLayer();
            string insertResult = db.InsertBinItem(box_id, sesa_id);

            // Return the result directly
            return Content(insertResult, "text/plain");
        }

        [HttpPost]
        public IActionResult DeleteBinItem(string partName)
        {
            var db = new DatabaseAccessLayer();
            string deleteResult = db.DeleteBinItem(partName);

            // Return the result directly
            return Content(deleteResult, "text/plain");
        }

        [HttpPost]
        public IActionResult ConfirmBinItem(string binId, string sesa_id)
        {
            var db = new DatabaseAccessLayer();
            List<int> orderIds;
            string confirmResult = db.ConfirmBinItem(binId, sesa_id, out orderIds);

            System.Diagnostics.Debug.WriteLine($"========== CONFIRM BIN ITEM ==========");
            System.Diagnostics.Debug.WriteLine($"binId: {binId}");
            System.Diagnostics.Debug.WriteLine($"sesa_id: {sesa_id}");
            System.Diagnostics.Debug.WriteLine($"confirmResult: {confirmResult}");
            System.Diagnostics.Debug.WriteLine($"orderIds.Count: {orderIds.Count}");

            if (confirmResult == "OK" && orderIds.Count > 0)
            {
                // ✅ Kirim email di background
                Task.Run(() =>
                {
                    try
                    {
                        DatabaseAccessLayer dalEmail = new DatabaseAccessLayer();

                        System.Diagnostics.Debug.WriteLine($"📧 Starting email process for {orderIds.Count} orders");

                        foreach (int id_order in orderIds)
                        {
                            System.Diagnostics.Debug.WriteLine($"📧 Sending email for id_order: {id_order}");
                            dalEmail.TriggerStatusUpdateEmail(id_order, sesa_id, 3); // 3 = Putaway
                            System.Diagnostics.Debug.WriteLine($"✅ Email sent successfully for id_order: {id_order}");
                        }

                        System.Diagnostics.Debug.WriteLine($"========== EMAIL PROCESS COMPLETE ==========");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ Email Error: {ex.Message}");
                        System.Diagnostics.Debug.WriteLine($"❌ Stack Trace: {ex.StackTrace}");
                    }
                });
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ No email sent. confirmResult: {confirmResult}, orderIds.Count: {orderIds.Count}");
            }

            return Content(confirmResult, "text/plain");
        }

        [HttpPost]
        public IActionResult ValidateBin(string anybinId)
        {
            var db = new DatabaseAccessLayer();
            string validateResult = db.ValidateBin(anybinId);

            // Return the result directly
            return Content(validateResult, "text/plain");
        }

        [HttpPost]
        public IActionResult ClearBin(string sesa_id)
        {
            var db = new DatabaseAccessLayer();
            string validateResult = db.ClearBin(sesa_id);

            // Return the result directly
            return Content(validateResult, "text/plain");
        }
        [HttpGet]
        public IActionResult GetTotalTempReq()
        {
            string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var db = new DatabaseAccessLayer();
            int total_req = db.GetTotalTempReq(sesa_id);

            // Return the result directly
            return Content(total_req.ToString(), "text/plain");
        }
        [HttpPost]
        public IActionResult AddReqPicking(int picking_id_order, decimal picking_req_qty)
        {
            string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (sesa_id == "")
            {
                return Content("Session Timeout, Please relogin!!", "text/plain");
            }
            else
            {
                var db = new DatabaseAccessLayer();
                string submit = db.AddReqPicking(picking_id_order, picking_req_qty, sesa_id);
                return Content(submit, "text/plain");
            }
        }
        public IActionResult GetTempReqList()
        {
            string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (sesa_id == "")
            {
                return Content("Session Timeout, Please relogin!!", "text/plain");
            }
            else
            {
                var db = new DatabaseAccessLayer();
                List<RequestTempModel> dataList = db.GetTempReqList(sesa_id);
                //return Content("Upload Success!!", "text/plain");

                return PartialView("_TableRequestTemp", dataList);
            }
        }
        [HttpPost]
        public IActionResult DeleteTempReq(int id_temp)
        {
            var db = new DatabaseAccessLayer();
            string deleteResult = db.DeleteTempReq(id_temp);
            // Return the result directly
            return Content(deleteResult, "text/plain");
        }
        //[HttpPost]
        //public IActionResult SubmitReqPicking(string remark, string plant)
        //{
        //    string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        //    var db = new DatabaseAccessLayer();
        //    string submit = db.SubmitReqPicking(remark ?? "", sesa_id, plant);
        //    return Content(submit, "text/plain");
        //}
        //public IActionResult RequestMonitoring()
        //{
        //    return this.CheckSession(() =>
        //    {
        //        string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //        List<string> userRoles = User.Claims
        //                                    .Where(c => c.Type == "semb_erp_role")
        //                                    .Select(c => c.Value)
        //                                    .ToList();
        //        //sesa_id = "SESA126011";
        //        var db = new DatabaseAccessLayer();
        //        List<UserDetailModel> userDetail = db.GetUserDetail(sesa_id);
        //        List<string> listStatus = db.GetStatusRequest();
        //        string name = User.FindFirst("semb_erp_name")?.Value;
        //        ViewBag.name = name;
        //        ViewBag.sesa_id = sesa_id;
        //        ViewBag.listStatus = listStatus;
        //        ViewBag.userRoles = userRoles;
        //        return View(userDetail);
        //    });
        //}

        [HttpPost]
        public IActionResult SubmitReqPicking(string remark, string plant)
        {
            string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(sesa_id))
            {
                return Content("Session Timeout, Please relogin!!", "text/plain");
            }

            var db = new DatabaseAccessLayer();
            var affectedOrders = db.GetAffectedOrdersBeforeSubmit(sesa_id);
            string submit = db.SubmitReqPicking(remark ?? "", sesa_id, plant);
            if (submit.StartsWith("OK;"))
            {
                _ = Task.Run(() =>
                {
                    try
                    {
                        Console.WriteLine("===== EMAIL TRIGGER START =====");
                        Console.WriteLine($"Affected orders: {affectedOrders.Count}");

                        var dalEmail = new DatabaseAccessLayer();

                        foreach (var orderInfo in affectedOrders)
                        {
                            int id_order = orderInfo.Item1;
                            int new_status_code = orderInfo.Item2;

                            Console.WriteLine($"Sending email: Order {id_order} → Status {new_status_code}");

                            dalEmail.TriggerStatusUpdateEmail(id_order, sesa_id, new_status_code);
                        }

                        Console.WriteLine("===== EMAIL TRIGGER END =====");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Email trigger error: {ex.Message}");
                        Console.WriteLine($"Stack: {ex.StackTrace}");
                    }
                });
            }

            return Content(submit, "text/plain");
        }
        public IActionResult GetRequestList()
        {
            try
            {
                string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                List<string> user_roles = User.Claims
                                            .Where(c => c.Type == "semb_erp_role")
                                            .Select(c => c.Value)
                                            .ToList();
                var db = new DatabaseAccessLayer();
                List<UserDetailModel> userDetail = db.GetUserDetail(sesa_id);
                var user = userDetail.First();

                var draw = Request.Form["draw"].FirstOrDefault();
                var start = Request.Form["start"].FirstOrDefault();
                var length = Request.Form["length"].FirstOrDefault();
                //var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
                var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][data]"].FirstOrDefault();
                var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
                var searchValue = Request.Form["search[value]"].FirstOrDefault();
                var column0Value = Request.Form["columns[0][search][value]"];
                var column1Value = Request.Form["columns[1][search][value]"];
                var column2Value = Request.Form["columns[2][search][value]"];
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int recordsTotal = 0;
                var mstData = (from RequestList in _context.v_request
                               where RequestList.requested_by == sesa_id || user_roles.HasAnyRole("admin", "receiver")
                               orderby RequestList.status_request ascending
                               select
                                   new
                                   {
                                       RequestList.id_request,
                                       RequestList.request_no,
                                       RequestList.status_desc,
                                       RequestList.requested_by_name,
                                       RequestList.record_date
                                   });

                //var mstData = (from temp in _context.mst_material_plant select temp);
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection)))
                {
                    mstData = mstData.OrderBy(sortColumn + " " + sortColumnDirection);
                }
                if (!string.IsNullOrEmpty(searchValue))
                {
                    mstData = mstData.Where(m => m.request_no.Contains(searchValue)
                                                || m.status_desc.Contains(searchValue));
                }
                //for (int i = 0; i < 4; i++)
                //{
                //    var searchColVal = Request.Form["columns[" + i.ToString() + "][search][value]"];
                //    var fieldName = Request.Form["columns[" + i.ToString() + "][data]"].FirstOrDefault();
                //    if (!string.IsNullOrEmpty(searchColVal))
                //    {
                //        if (fieldName == "request_no")
                //        {
                //            mstData = mstData.Where(m => m.request_no.Contains(searchColVal));
                //        }
                //        else if (fieldName == "status_desc")
                //        {
                //            mstData = mstData.Where(m => m.status_desc.Contains(searchColVal));
                //        }
                //        else if (fieldName == "requested_by_name")
                //        {
                //            mstData = mstData.Where(m => m.requested_by_name.Contains(searchColVal));
                //        }
                //        else if (fieldName == "record_date")
                //        {
                //            mstData = mstData.Where(m => m.record_date.Contains(searchColVal));
                //        }
                //    }
                //}
                recordsTotal = mstData.Count();
                var data = mstData.Skip(skip).Take(pageSize).ToList();
                var jsonData = new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data };
                return Ok(jsonData);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public IActionResult GetReqDetail(int id_request)
        {
            var db = new DatabaseAccessLayer();
            var reqInfo = JsonConvert.DeserializeObject<RequestListModel>(db.GetRequestInfo(id_request));
            List<RequestListModel> dataList = db.GetReqDetail(id_request);
            List<PickBoxModel> boxList = db.GetPickBox(id_request);
            List<TcodeModel> tcodeList = db.GetTcodeHistory(id_request);
            ViewBag.reqInfo = reqInfo;
            ViewBag.boxList = boxList;
            ViewBag.tcodeList = tcodeList;
            //return Content("Upload Success!!", "text/plain");

            return PartialView("_TableRequestDetail", dataList);
        }


        public IActionResult GetRecentVariance()
        {
            var data = _context.v_picking_variance
                .OrderByDescending(x => x.record_date)
                .Take(10)
                .Select(x => new {
                    x.partno,
                    x.sbin,
                    x.qty,
                    x.request_no,
                    x.name,
                    x.record_date
                })
                .ToList<dynamic>();

            return PartialView("_VarianceHistoryPartial", data);
        }
        [HttpGet]
        public IActionResult ExportRequestList()
        {
            using (XLWorkbook wb = new XLWorkbook())
            {

                DateTime currentDateTime = DateTime.Now;
                string formattedDateTime = currentDateTime.ToString("yyyyMMddHHmmss");

                var db = new DatabaseAccessLayer();
                System.Data.DataTable dt = db.GetExportRequestList().Tables[0];
                wb.Worksheets.Add(dt);
                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Request List Details.xlsx");
                }
            }
        }
        [Authorize(Policy = "RequireReceiverAdmin")]
        public IActionResult BlockBin()
        {
            return this.CheckSession(() =>
            {
                string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                List<string> userRoles = User.Claims
                                            .Where(c => c.Type == "semb_erp_role")
                                            .Select(c => c.Value)
                                            .ToList();
                //sesa_id = "SESA126011";
                var db = new DatabaseAccessLayer();
                List<UserDetailModel> userDetail = db.GetUserDetail(sesa_id);
                string name = User.FindFirst("semb_erp_name")?.Value;
                ViewBag.name = name;
                ViewBag.sesa_id = sesa_id;
                ViewBag.userRoles = userRoles;
                return View(userDetail);
            });
        }
        public IActionResult GetBlockBin()
        {
            try
            {
                var draw = Request.Form["draw"].FirstOrDefault();
                var start = Request.Form["start"].FirstOrDefault();
                var length = Request.Form["length"].FirstOrDefault();
                //var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
                var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][data]"].FirstOrDefault();
                var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
                var searchValue = Request.Form["search[value]"].FirstOrDefault();
                var column0Value = Request.Form["columns[0][search][value]"];
                var column1Value = Request.Form["columns[1][search][value]"];
                var column2Value = Request.Form["columns[2][search][value]"];
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int recordsTotal = 0;
                var mstData = (from BlockBin in _context.v_block_bin
                               select
                                   new
                                   {
                                       BlockBin.no,
                                       BlockBin.partno,
                                       BlockBin.sbin,
                                       BlockBin.variance_date
                                   });

                //var mstData = (from temp in _context.mst_material_plant select temp);
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection)))
                {
                    mstData = mstData.OrderBy(sortColumn + " " + sortColumnDirection);
                }
                if (!string.IsNullOrEmpty(searchValue))
                {
                    mstData = mstData.Where(m => m.partno.Contains(searchValue)
                                                || m.sbin.Contains(searchValue));
                }
                recordsTotal = mstData.Count();
                var data = mstData.Skip(skip).Take(pageSize).ToList();
                var jsonData = new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data };
                return Ok(jsonData);
            }
            catch (Exception ex)
             {
                throw;
            }
        }
        [Authorize(Policy = "RequireReceiverAdmin")]
        public IActionResult VarianceHistory()
        {
            return this.CheckSession(() =>
            {
                string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                List<string> userRoles = User.Claims
                                            .Where(c => c.Type == "semb_erp_role")
                                            .Select(c => c.Value)
                                            .ToList();
                //sesa_id = "SESA126011";
                var db = new DatabaseAccessLayer();
                List<UserDetailModel> userDetail = db.GetUserDetail(sesa_id);
                string name = User.FindFirst("semb_erp_name")?.Value;
                ViewBag.name = name;
                ViewBag.sesa_id = sesa_id;
                ViewBag.userRoles = userRoles;
                return View(userDetail);
            });
        }
        public IActionResult GetVarianceHistory()
        {
            try
            {
                var draw = Request.Form["draw"].FirstOrDefault();
                var start = Request.Form["start"].FirstOrDefault();
                var length = Request.Form["length"].FirstOrDefault();
                //var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
                var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][data]"].FirstOrDefault();
                var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
                var searchValue = Request.Form["search[value]"].FirstOrDefault();
                var column0Value = Request.Form["columns[0][search][value]"];
                var column1Value = Request.Form["columns[1][search][value]"];
                var column2Value = Request.Form["columns[2][search][value]"];
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int recordsTotal = 0;
                var mstData = (from Variance in _context.v_picking_variance
                               select
                                   new
                                   {
                                       Variance.id_var,
                                       Variance.request_no,
                                       Variance.partno,
                                       Variance.sbin,
                                       Variance.qty,
                                       Variance.name,
                                       Variance.record_date
                                   });

                //var mstData = (from temp in _context.mst_material_plant select temp);
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection)))
                {
                    mstData = mstData.OrderBy(sortColumn + " " + sortColumnDirection);
                }
                if (!string.IsNullOrEmpty(searchValue))
                {
                    mstData = mstData.Where(m => m.partno.Contains(searchValue)
                                                || m.sbin.Contains(searchValue)
                                                || m.request_no.Contains(searchValue)
                                                || m.name.Contains(searchValue));
                }
                recordsTotal = mstData.Count();
                var data = mstData.Skip(skip).Take(pageSize).ToList();
                var jsonData = new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data };
                return Ok(jsonData);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        [HttpPost]
        public IActionResult OpenBlockBin(string partno, string sbin)
        {
            string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var db = new DatabaseAccessLayer();
            string open = db.OpenBlockBin(partno, sbin, sesa_id);
            return Content(open, "text/plain");
        }
        [Authorize(Policy = "RequireReceiver")]
        public IActionResult Picking()
        {
            return this.CheckSession(() =>
            {
                string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                string name = User.FindFirst("semb_erp_name")?.Value;
                List<string> userRoles = User.Claims
                                            .Where(c => c.Type == "semb_erp_role")
                                            .Select(c => c.Value)
                                            .ToList();
                //sesa_id = "SESA126011";
                var db = new DatabaseAccessLayer();
                List<RequestListModel> reqList = db.GetReqList();
                db.DeleteTempPicking(sesa_id);
                ViewBag.name = name;
                ViewBag.sesa_id = sesa_id;
                ViewBag.userRoles = userRoles;
                return View(reqList);
            });
        }
        [HttpPost]
        public IActionResult StartPicking(int id_request)
        {
            string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var db = new DatabaseAccessLayer();
            string submit = db.StartPicking(id_request, sesa_id);
            return Content(submit, "text/plain");
        }
        [Authorize(Policy = "RequireReceiver")]
        public IActionResult PickingMaterial(int id_request)
        {
            string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            string name = User.FindFirst("semb_erp_name")?.Value;
            List<string> userRoles = User.Claims
                                        .Where(c => c.Type == "semb_erp_role")
                                        .Select(c => c.Value)
                                        .ToList();
            //sesa_id = "SESA126011";
            var db = new DatabaseAccessLayer();
            var reqInfo = JsonConvert.DeserializeObject<RequestListModel>(db.GetRequestInfo(id_request));
            List<RequestListModel> reqList = db.GetReqMaterial(id_request, sesa_id);
            ViewBag.id_request = id_request;
            ViewBag.reqInfo = reqInfo;
            ViewBag.name = name;
            ViewBag.sesa_id = sesa_id;
            ViewBag.userRoles = userRoles;
            return View(reqList);
        }
        [HttpPost]
        public IActionResult UpdatePickingMaterialQty(int id_det, double value_data)
        {
            int returnValue;
            var db = new DatabaseAccessLayer();
            string submit = db.UpdatePickingMaterialQty(id_det, value_data);

            return Content(submit, "text/plain");
        }
        [Authorize(Policy = "RequireReceiver")]
        public IActionResult ScanBin(int id_request, int id_det)
        {
            string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            string name = User.FindFirst("semb_erp_name")?.Value;
            List<string> userRoles = User.Claims
                                        .Where(c => c.Type == "semb_erp_role")
                                        .Select(c => c.Value)
                                        .ToList();
            var db = new DatabaseAccessLayer();
            var pickInfo = JsonConvert.DeserializeObject<PickingModel>(db.GetBinPicking(id_request, id_det, sesa_id));
            var reqInfo = JsonConvert.DeserializeObject<RequestListModel>(db.GetRequestInfo(id_request));
            ViewBag.id_request = id_request;
            ViewBag.id_det = id_det;
            ViewBag.pickInfo = pickInfo;
            ViewBag.reqInfo = reqInfo;
            ViewBag.name = name;
            ViewBag.sesa_id = sesa_id;
            ViewBag.userRoles = userRoles;
            return View();
        }
        [HttpPost]
        public IActionResult DeleteTempBox()
        {
            string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var db = new DatabaseAccessLayer();
            string delTemp = db.DeleteTempBox(sesa_id);
            return Content(delTemp, "text/plain");
        }
        [Authorize(Policy = "RequireReceiver")]
        public IActionResult ScanBox(int id_request, int id_det, string sbin)
        {
            string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            string name = User.FindFirst("semb_erp_name")?.Value;
            List<string> userRoles = User.Claims
                                        .Where(c => c.Type == "semb_erp_role")
                                        .Select(c => c.Value)
                                        .ToList();
            var db = new DatabaseAccessLayer();
            List<string> printList = db.GetPrinter();
            List<PickingModel> pickList = db.GetPickingTempQty(id_request, id_det, sbin, sesa_id);
            List<TempBoxModel> tempList = db.GetTempBox(id_request, id_det, sbin, sesa_id);
            var reqInfo = JsonConvert.DeserializeObject<RequestListModel>(db.GetRequestInfo(id_request));
            ViewBag.id_request = id_request;
            ViewBag.id_det = id_det;
            ViewBag.sbin = sbin;
            ViewBag.printList = printList;
            ViewBag.pickList = pickList;
            ViewBag.reqInfo = reqInfo;
            ViewBag.name = name;
            ViewBag.sesa_id = sesa_id;
            ViewBag.userRoles = userRoles;
            return View(tempList);
        }
        [HttpPost]
        public IActionResult CheckBox(int id_request, int id_det, string partno, string sbin, string box_id, decimal remain_qty)
        {
            string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var db = new DatabaseAccessLayer();
            string checkBox = db.CheckBox(id_request, id_det, partno, sbin, box_id, remain_qty, sesa_id);
            return Content(checkBox, "text/plain");
        }
        [HttpPost]
        public IActionResult BoxKitting(string box_id, decimal kitting_qty, int id_request, int id_det, string partno, string sbin, decimal remain_qty)
        {
            string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            string name = User.FindFirst("semb_erp_name")?.Value;
            var db = new DatabaseAccessLayer();
            string kit = db.BoxKitting(box_id, kitting_qty, sesa_id, name);
            string[] res = kit.Split(';');
            if (res[0] == "OK")
            {
                string checkBox = db.CheckBox(id_request, id_det, partno, sbin, res[2], remain_qty, sesa_id); //INSERT TO TEMP
                string[] res2 = checkBox.Split(';');
                if (res2[0] == "OK")
                {
                    return Content(kit, "text/plain");
                }
                else
                {
                    return Content(checkBox, "text/plain");
                }
            }
            else
            {
                return Content(kit, "text/plain");
            }
        }
        [HttpPost]
        public IActionResult FinishPicking(int id_request, int id_det, string sbin)
        {
            string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var db = new DatabaseAccessLayer();
            string fin = db.FinishPicking(id_request, id_det, sbin, sesa_id);
            return Content(fin, "text/plain");
        }
        [HttpPost]
        public IActionResult VariancePicking(int id_request, int id_det, string sbin)
        {
            string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var db = new DatabaseAccessLayer();
            string variance = db.VariancePicking(id_request, id_det, sbin, sesa_id);
            return Content(variance, "text/plain");
        }
        [Authorize(Policy = "RequireReceiverAdmin")]
        public IActionResult MasterBin()
        {
            return this.CheckSession(() =>
            {
                var db = new DatabaseAccessLayer();
                string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                string name = User.FindFirst("semb_erp_name")?.Value;
                string plant = db.GetUserPlant(sesa_id);
                List<string> catList = db.GET_CAT_NON_CONF();
                //List<string> supplierList = db.GetSupplierList();
                //ViewBag.supplierList = supplierList;
                ViewBag.name = name;
                ViewBag.sesa_id = sesa_id;
                ViewBag.plant = plant;
                ViewBag.catList = catList;
                return View();
            });
        }

        [HttpGet]
        public IActionResult GetMstBin()
        {
            var db = new DatabaseAccessLayer();
            List<BinModel> listBin = db.GetMstBin();

            // Transform the list into a format suitable for DataTables
            var result = listBin.Select(item => new
            {
                BinId = item.BinId,
                BinName = item.BinName,
                Length = item.Length,
                Width = item.Width,
                Height = item.Height
            });

            return Json(result);
        }

        [HttpPost]
        public IActionResult DeleteMstBins(string binNamesString)
        {
            if (string.IsNullOrEmpty(binNamesString))
            {
                return BadRequest("No bin names provided.");
            }

            // Split the string into an array
            var binNamesList = binNamesString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            var db = new DatabaseAccessLayer();
            string deleteResult = db.DeleteMstBins(binNamesList);

            // Return the result directly
            return Content(deleteResult, "text/plain");
        }
        [HttpPost]
        public IActionResult AddMstBin(string bin_name, string bin_length, string bin_width, string bin_height)
        {
            var db = new DatabaseAccessLayer();
            string addResult = db.AddMstBin(bin_name, bin_length, bin_width, bin_height);

            // Return the result directly
            return Content(addResult, "text/plain");
        }

        public IActionResult UpdtMstBin(string updt_bin_name, string updt_bin_length, string updt_bin_width, string updt_bin_height)
        {
            var db = new DatabaseAccessLayer();
            string addResult = db.UpdtMstBin(updt_bin_name, updt_bin_length, updt_bin_width, updt_bin_height);
            //string addResult = "OK";
            // Return the result directly
            return Content(addResult, "text/plain");
        }
        [Authorize(Policy = "RequireReceiverAdmin")]
        public IActionResult RequestDetail()
        {
            return this.CheckSession(() =>
            {
                string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                List<string> userRoles = User.Claims
                                            .Where(c => c.Type == "semb_erp_role")
                                            .Select(c => c.Value)
                                            .ToList();
                //sesa_id = "SESA126011";
                var db = new DatabaseAccessLayer();
                List<UserDetailModel> userDetail = db.GetUserDetail(sesa_id);
                string name = User.FindFirst("semb_erp_name")?.Value;
                ViewBag.name = name;
                ViewBag.sesa_id = sesa_id;
                ViewBag.userRoles = userRoles;
                return View(userDetail);
            });
        }
        public IActionResult GetRequestDetail()
        {
            try
            {
                string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                List<string> user_roles = User.Claims
                                            .Where(c => c.Type == "semb_erp_role")
                                            .Select(c => c.Value)
                                            .ToList();
                var db = new DatabaseAccessLayer();
                List<UserDetailModel> userDetail = db.GetUserDetail(sesa_id);
                var user = userDetail.First();
                var draw = Request.Form["draw"].FirstOrDefault() ?? "1";
                var start = Request.Form["start"].FirstOrDefault() ?? "0";
                var length = Request.Form["length"].FirstOrDefault() ?? "10";
                var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][data]"].FirstOrDefault();
                var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
                var searchValue = Request.Form["search[value]"].FirstOrDefault();
                var column0Value = Request.Form["columns[0][search][value]"];
                var column1Value = Request.Form["columns[1][search][value]"];
                var column2Value = Request.Form["columns[2][search][value]"];
                int pageSize = int.TryParse(length, out int parsedLength) ? parsedLength : 10;
                int skip = int.TryParse(start, out int parsedSkip) ? parsedSkip : 0;
                int recordsTotal = 0;
                var mstData = (from RequestDetail in _context.v_request_detail
                               select new
                               {
                                   RequestDetail.id_det,
                                   RequestDetail.id_request,
                                   RequestDetail.id_order,
                                   RequestDetail.request_no,
                                   RequestDetail.partno,
                                   RequestDetail.qty,
                                   RequestDetail.picked_qty,
                                   RequestDetail.uom,
                                   RequestDetail.status_picking
                               });
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection)))
                {
                    mstData = mstData.OrderBy(sortColumn + " " + sortColumnDirection);
                }
                if (!string.IsNullOrEmpty(searchValue))
                {
                    mstData = mstData.Where(m => (m.request_no != null && m.request_no.Contains(searchValue))
                                                || (m.partno != null && m.partno.Contains(searchValue))
                                                || (m.status_picking != null && m.status_picking.Contains(searchValue)));
                }
                recordsTotal = mstData.Count();
                var data = mstData.Skip(skip).Take(pageSize).ToList();
                var jsonData = new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data };
                return Ok(jsonData);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        [HttpGet]
        public IActionResult ExportRequestDetail()
        {
            using (XLWorkbook wb = new XLWorkbook())
            {

                DateTime currentDateTime = DateTime.Now;
                string formattedDateTime = currentDateTime.ToString("yyyyMMddHHmmss");

                var db = new DatabaseAccessLayer();
                System.Data.DataTable dt = db.GetExportRequestDetail().Tables[0];
                wb.Worksheets.Add(dt);
                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Request Details.xlsx");
                }
            }
        }
        [HttpPost]
        public IActionResult OpenVariance(int id_det)
        {
            string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var db = new DatabaseAccessLayer();
            string upd = db.OpenVariance(id_det, sesa_id);

            return Content(upd, "text/plain");
        }
        [HttpPost]
        public IActionResult DeletePartnumber(int id_det)
        {
            string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var db = new DatabaseAccessLayer();
            string upd = db.DeletePartnumber(id_det, sesa_id);

            return Content(upd, "text/plain");
        }
        [Authorize(Policy = "RequireReceiver")]
        public IActionResult Consolidation()
        {
            return this.CheckSession(() =>
            {
                string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                string name = User.FindFirst("semb_erp_name")?.Value;
                List<string> userRoles = User.Claims
                                            .Where(c => c.Type == "semb_erp_role")
                                            .Select(c => c.Value)
                                            .ToList();
                //sesa_id = "SESA126011";
                var db = new DatabaseAccessLayer();
                db.DeleteTempConsol(sesa_id);
                List<RequestListModel> reqList = db.GetConsolList();
                ViewBag.name = name;
                ViewBag.sesa_id = sesa_id;
                ViewBag.userRoles = userRoles;
                return View(reqList);
            });
        }
        [HttpPost]
        public IActionResult StartConsol(int id_request)
        {
            string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var db = new DatabaseAccessLayer();
            string submit = db.StartConsol(id_request, sesa_id);
            return Content(submit, "text/plain");
        }
        [Authorize(Policy = "RequireReceiver")]
        public IActionResult ConsolScanBox(int id_request)
        {
            string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            string name = User.FindFirst("semb_erp_name")?.Value;
            List<string> userRoles = User.Claims
                                        .Where(c => c.Type == "semb_erp_role")
                                        .Select(c => c.Value)
                                        .ToList();
            var db = new DatabaseAccessLayer();
            List<string> printList = db.GetPrinter();
            List<TempBoxModel> tempList = db.GetTempConsolBox(id_request, sesa_id);
            List<PalletModel> palletList = db.GetPalletID(id_request);
            var reqInfo = JsonConvert.DeserializeObject<RequestListModel>(db.GetRequestInfo(id_request));
            ViewBag.id_request = id_request;
            ViewBag.printList = printList;
            ViewBag.reqInfo = reqInfo;
            ViewBag.palletList = palletList;
            ViewBag.name = name;
            ViewBag.sesa_id = sesa_id;
            ViewBag.userRoles = userRoles;
            return View(tempList);
        }
        [HttpPost]
        public IActionResult CheckBoxConsol(int id_request, string box_id)
        {
            string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var db = new DatabaseAccessLayer();
            string checkBox = db.CheckBoxConsol(id_request, box_id, sesa_id);
            return Content(checkBox, "text/plain");
        }
        [HttpPost]
        public IActionResult CreatePallet(int id_request, int id_pallet)
        {
            string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var db = new DatabaseAccessLayer();
            string submit = db.CreatePallet(id_request, id_pallet, sesa_id);
            return Content(submit, "text/plain");
        }
        public IActionResult GetBoxList(string orderId)
        {
            var db = new DatabaseAccessLayer();
            List<BoxModel> listBin = db.GetBoxList(orderId);

            // Transform the list into a format suitable for DataTables
            var result = listBin.Select(item => new
            {
                BoxId = item.BoxId,
                PartNo = item.PartNo,
                Qty = item.Qty,
                Sbin = item.Sbin,
                Unit = item.Unit,
                Pstats = item.Pstats
            });

            return Json(result);
        }

        [HttpPost]
        public IActionResult DeleteOrderList(string orderId)
        {
            var db = new DatabaseAccessLayer();
            string deleteResult = db.DeleteOrderList(orderId);

            // Return the result directly
            return Content(deleteResult, "text/plain");
        }
        [Authorize(Policy = "RequireRequestor")]
        public IActionResult PalletReceived()
        {
            return this.CheckSession(() =>
            {
                string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                List<string> userRoles = User.Claims
                                            .Where(c => c.Type == "semb_erp_role")
                                            .Select(c => c.Value)
                                            .ToList();
                //sesa_id = "SESA126011";
                var db = new DatabaseAccessLayer();
                List<UserDetailModel> userDetail = db.GetUserDetail(sesa_id);
                string name = User.FindFirst("semb_erp_name")?.Value;
                ViewBag.name = name;
                ViewBag.sesa_id = sesa_id;
                ViewBag.userRoles = userRoles;
                return View(userDetail);
            });
        }
        public IActionResult GetPalletReceived()
        {
            try
            {
                var draw = Request.Form["draw"].FirstOrDefault();
                var start = Request.Form["start"].FirstOrDefault();
                var length = Request.Form["length"].FirstOrDefault();
                //var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
                var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][data]"].FirstOrDefault();
                var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
                var searchValue = Request.Form["search[value]"].FirstOrDefault();
                var column0Value = Request.Form["columns[0][search][value]"];
                var column1Value = Request.Form["columns[1][search][value]"];
                var column2Value = Request.Form["columns[2][search][value]"];
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int recordsTotal = 0;
                var mstData = (from Pallet in _context.v_pallet_header
                               where Pallet.status_pallet == "CREATION" || Pallet.status_pallet == "TRANSFER"
                               select
                                   new
                                   {
                                       Pallet.id_pallet,
                                       Pallet.id_request,
                                       Pallet.request_no,
                                       Pallet.pallet_no,
                                       Pallet.status_pallet,
                                       Pallet.record_date,
                                       Pallet.name,
                                   });

                //var mstData = (from temp in _context.mst_material_plant select temp);
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection)))
                {
                    mstData = mstData.OrderBy(sortColumn + " " + sortColumnDirection);
                }
                if (!string.IsNullOrEmpty(searchValue))
                {
                    mstData = mstData.Where(m => m.request_no.Contains(searchValue)
                                                || m.pallet_no.Contains(searchValue)
                                                || m.name.Contains(searchValue));
                }
                recordsTotal = mstData.Count();
                var data = mstData.Skip(skip).Take(pageSize).ToList();
                var jsonData = new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data };
                return Ok(jsonData);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        [Authorize(Policy = "RequireReceiver")]
        public IActionResult Transfer()
        {
            return this.CheckSession(() =>
            {
                string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                string name = User.FindFirst("semb_erp_name")?.Value;
                List<string> userRoles = User.Claims
                                            .Where(c => c.Type == "semb_erp_role")
                                            .Select(c => c.Value)
                                            .ToList();
                //sesa_id = "SESA126011";
                var db = new DatabaseAccessLayer();
                List<TempPalletModel> tempList = db.GetTempPallet(sesa_id);
                ViewBag.name = name;
                ViewBag.sesa_id = sesa_id;
                ViewBag.userRoles = userRoles;
                return View(tempList);
            });
        }
        [HttpPost]
        public IActionResult InsertPalletTransfer(string pallet_no)
        {
            string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var db = new DatabaseAccessLayer();
            string submit = db.InsertPalletTransfer(pallet_no, sesa_id);
            return Content(submit, "text/plain");
        }
        [HttpPost]
        public IActionResult RemovePalletTransfer(int id_temp)
        {
            string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var db = new DatabaseAccessLayer();
            string submit = db.RemovePalletTransfer(id_temp);
            return Content(submit, "text/plain");
        }
        [HttpPost]
        public IActionResult SubmitPalletTransfer()
        {
            string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var db = new DatabaseAccessLayer();
            string submit = db.SubmitPalletTransfer(sesa_id);
            return Content(submit, "text/plain");
        }
        public IActionResult GetDeptList()
        {
            var db = new DatabaseAccessLayer();
            List<RequestListModel> listDept = db.GetDeptList();
            var result = listDept.Select(item => new
            {
                Depts = item.department,
            });

            return Json(result);
        }

        public IActionResult GetReqLists()
        {
            var db = new DatabaseAccessLayer();
            List<RequestListModel> ReqsList = db.GetReqLists();
            var result = ReqsList.Select(item => new
            {
                id_request = item.id_request,
                request_no = item.request_no,
                status_code = item.status_code,
                status_desc = item.status_desc,
                remark = item.remark,
                record_date = item.record_date,
                lead_time = item.lead_time,
                requested_by = item.requested_by,
                requested_by_name = item.requested_by_name,
                department = item.department
            });

            return Json(result);
        }

        [Authorize(Policy = "RequireReceiverPlantreceiverAdmin")]
        public IActionResult PalletTransferOpen()
        {
            return this.CheckSession(() =>
            {
                string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                List<string> userRoles = User.Claims
                                            .Where(c => c.Type == "semb_erp_role")
                                            .Select(c => c.Value)
                                            .ToList();
                //sesa_id = "SESA126011";
                var db = new DatabaseAccessLayer();
                List<UserDetailModel> userDetail = db.GetUserDetail(sesa_id);
                List<string> listStatus = db.GetStatusRequest();
                string name = User.FindFirst("semb_erp_name")?.Value;
                ViewBag.name = name;
                ViewBag.sesa_id = sesa_id;
                ViewBag.listStatus = listStatus;
                ViewBag.userRoles = userRoles;
                return View(userDetail);
            });
        }

        [Authorize(Policy = "RequireReceiverPlantreceiverAdmin")]
        public IActionResult PalletTransferHistory()
        {
            return this.CheckSession(() =>
            {
                string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                List<string> userRoles = User.Claims
                                            .Where(c => c.Type == "semb_erp_role")
                                            .Select(c => c.Value)
                                            .ToList();
                //sesa_id = "SESA126011";
                var db = new DatabaseAccessLayer();
                List<UserDetailModel> userDetail = db.GetUserDetail(sesa_id);
                List<string> listStatus = db.GetStatusRequest();
                string name = User.FindFirst("semb_erp_name")?.Value;
                ViewBag.name = name;
                ViewBag.sesa_id = sesa_id;
                ViewBag.listStatus = listStatus;
                ViewBag.userRoles = userRoles;
                return View(userDetail);
            });
        }

        public IActionResult GetPalletTransferList()
        {
            string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var db = new DatabaseAccessLayer();
            string plant = db.GetUserPlant(sesa_id);

            try
            {
                var draw = Request.Form["draw"].FirstOrDefault();
                var start = Request.Form["start"].FirstOrDefault();
                var length = Request.Form["length"].FirstOrDefault();
                //var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
                var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][data]"].FirstOrDefault();
                var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
                var searchValue = Request.Form["search[value]"].FirstOrDefault();
                var column0Value = Request.Form["columns[0][search][value]"];
                var column1Value = Request.Form["columns[1][search][value]"];
                var column2Value = Request.Form["columns[2][search][value]"];
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int recordsTotal = 0;
                var mstData = (from Pallet in _context.v_pallet_header
                               join request in _context.v_request
                                on Pallet.id_request equals request.id_request
                               where (Pallet.status_pallet == "CREATION" || Pallet.status_pallet == "TRANSFER" || Pallet.status_pallet == "RECEIVED") && (Pallet.plant == "ALL" || Pallet.plant == plant)
                               select
                                   new
                                   {
                                       Pallet.id_pallet,
                                       Pallet.id_request,
                                       Pallet.request_no,
                                       Pallet.pallet_no,
                                       Pallet.status_pallet,
                                       Pallet.record_date,
                                       receive_date = Pallet.receive_date.HasValue ? Pallet.receive_date.Value : (DateTime?)null,
                                       Pallet.received_by,
                                       Pallet.received_by_name,
                                       Pallet.supply_date,
                                       Pallet.supplied_by,
                                       Pallet.supplied_by_name,
                                       Pallet.supply_comment,
                                   });

                //var mstData = (from temp in _context.mst_material_plant select temp);
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection)))
                {
                    mstData = mstData.OrderBy(sortColumn + " " + sortColumnDirection);
                }
                if (!string.IsNullOrEmpty(searchValue))
                {
                    mstData = mstData.Where(m => m.request_no.Contains(searchValue)
                                                || m.pallet_no.Contains(searchValue));
                }
                recordsTotal = mstData.Count();
                var data = mstData.Skip(skip).Take(pageSize).ToList();
                var jsonData = new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data };
                return Ok(jsonData);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public IActionResult GetPalletTransferHistory()
        {
            try
            {
                var draw = Request.Form["draw"].FirstOrDefault();
                 var start = Request.Form["start"].FirstOrDefault();
                var length = Request.Form["length"].FirstOrDefault();

                // Proteksi sortColumn agar tidak null
                var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][data]"].FirstOrDefault() ?? "record_date";
                var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault() ?? "desc";

                var searchValue = Request.Form["search[value]"].FirstOrDefault();

                int pageSize = length != null ? Convert.ToInt32(length) : 10;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int recordsTotal = 0;

                // 1. Definisikan Query Awal
                var mstData = (from Pallet in _context.v_pallet_header
                               join request in _context.v_request on Pallet.id_request equals request.id_request
                               where Pallet.status_pallet == "SUPPLIED"
                               select new
                               {
                                   Pallet.id_pallet,
                                   // Paksa id_request jadi string di level aplikasi agar tidak bentrok saat sorting/search
                                   id_request = Pallet.id_request.ToString(),
                                   Pallet.request_no,
                                   Pallet.pallet_no,
                                   Pallet.status_pallet,
                                   Pallet.record_date,
                                   Pallet.receive_date,
                                   Pallet.received_by,
                                   Pallet.received_by_name,
                                   Pallet.supply_date,
                                   Pallet.supplied_by,
                                   Pallet.supplied_by_name,
                                   Pallet.supply_comment,
                               });

                // 2. Filter Search (Hanya kolom string)
                if (!string.IsNullOrEmpty(searchValue))
                {
                    mstData = mstData.Where(m => m.request_no.Contains(searchValue)
                                              || m.pallet_no.Contains(searchValue)
                                              || m.supplied_by_name.Contains(searchValue));
                }

                // 3. Hitung Total Records sebelum paging
                recordsTotal = mstData.Count();

                // 4. Sorting Dinamis (DIBUNGKUS TRY-CATCH KHUSUS)
                try
                {
                    if (!string.IsNullOrEmpty(sortColumn))
                    {
                        // Gunakan System.Linq.Dynamic.Core jika tersedia
                        mstData = mstData.OrderBy(sortColumn + " " + sortColumnDirection);
                    }
                }
                catch
                {
                    // Jika sorting gagal karena konversi, default ke record_date
                    mstData = mstData.OrderByDescending(x => x.record_date);
                }

                // 5. Execution & Paging
                var data = mstData.Skip(skip).Take(pageSize).ToList();

                var jsonData = new
                {
                    draw = draw,
                    recordsFiltered = recordsTotal,
                    recordsTotal = recordsTotal,
                    data = data
                };

                return Ok(jsonData);
            }
            catch (Exception ex)
            {
                // Log error di sini jika perlu
                return BadRequest(new { message = ex.Message });
            }
        }
        public IActionResult UpdateReceived(string id_pallet, string sesa_id)
        {
            var db = new DatabaseAccessLayer();
            string Result = db.UpdateReceived(id_pallet, sesa_id);

            // Return the result directly
            return Content(Result, "text/plain");
        }

        public IActionResult UpdateSupplied(string id_pallet, string sesa_id, string supplied_name)
        {
            var db = new DatabaseAccessLayer();
            string Result = db.UpdateSupplied(id_pallet, sesa_id, supplied_name);

            // Return the result directly
            return Content(Result, "text/plain");
        }

        public IActionResult GetPalletDetail(string plt_id)
        {
            var db = new DatabaseAccessLayer();
            List<PickBoxModel> ReqsList = db.GetPalletDetail(plt_id);
            var result = ReqsList.Select(item => new
            {
                id_pick = item.id_pick,
                id_request = item.id_request,
                id_det = item.id_det,
                box_id = item.box_id,
                partno = item.partno,
                qty = item.qty,
                sbin = item.sbin,
                picked_by = item.picked_by,
                record_date = item.record_date
            });

            return Json(result);
        }

        public IActionResult close_non_conf(string id_non_conf, string sesa_id, string cls_comment)
        {
            var db = new DatabaseAccessLayer();
            string Result = db.close_non_conf(id_non_conf, sesa_id, cls_comment);

            // Return the result directly
            return Content(Result, "text/plain");
        }
        [Authorize(Policy = "RequireReceiverAdmin")]
        public IActionResult CreateListBox(string box_id, decimal total_qty, decimal spq)
        {
            string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (sesa_id == "")
            {
                return Content("Session Timeout, Please relogin!!", "text/plain");
            }
            else
            {
                var db = new DatabaseAccessLayer();
                List<string> listPrinter = db.GetPrinter();
                List<BoxModel> dataTemp = db.GetTempBoxKitting(total_qty, spq);
                //return Content("Upload Success!!", "text/plain");
                ViewBag.box_id = box_id;
                ViewBag.total_qty = total_qty;
                ViewBag.listPrinter = listPrinter;
                return PartialView("_TableTempListBoxKitting", dataTemp);
            }
        }
        [Authorize(Policy = "RequireReceiverAdmin")]
        public IActionResult CreateBoxKitting(string box_id, string qtys)
        {
            string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var db = new DatabaseAccessLayer();
            string Result = db.CreateBoxKitting(box_id, qtys, sesa_id);

            // Return the result directly
            return Content(Result, "text/plain");
        }

        [Authorize(Policy = "RequireAny")]
        public IActionResult AgingMovement()
        {
            return this.CheckSession(() =>
            {
                string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                List<string> userRoles = User.Claims
                                            .Where(c => c.Type == "semb_erp_role")
                                            .Select(c => c.Value)
                                            .ToList();
                //sesa_id = "SESA126011";
                var db = new DatabaseAccessLayer();
                List<UserDetailModel> userDetail = db.GetUserDetail(sesa_id);
                List<string> listStatus = db.GetStatus();
                List<string> listPrinter = db.GetPrinter();
                string name = User.FindFirst("semb_erp_name")?.Value;
                ViewBag.name = name;
                ViewBag.sesa_id = sesa_id;
                ViewBag.listStatus = listStatus;
                ViewBag.userRoles = userRoles;
                ViewBag.listPrinter = listPrinter;
                return View(userDetail);
            });

            return View();
        }
        public IActionResult GetAgingMovementList()
        {
            try
            {
                string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                List<string> user_roles = User.Claims
                                            .Where(c => c.Type == "semb_erp_role")
                                            .Select(c => c.Value)
                                            .ToList();
                var db = new DatabaseAccessLayer();
                List<UserDetailModel> userDetail = db.GetUserDetail(sesa_id);
                var user = userDetail.First();

                var draw = Request.Form["draw"].FirstOrDefault();
                var start = Request.Form["start"].FirstOrDefault();
                var length = Request.Form["length"].FirstOrDefault();
                //var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
                var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][data]"].FirstOrDefault();
                var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
                var searchValue = Request.Form["search[value]"].FirstOrDefault();
                var column0Value = Request.Form["columns[0][search][value]"];
                var column1Value = Request.Form["columns[1][search][value]"];
                var column2Value = Request.Form["columns[2][search][value]"];
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int recordsTotal = 0;

                var departments = user.other_dept.Split(',').Select(d => d.Trim()).ToList();

                var mstData = (from AgingMovement in _context.v_order_aging
                               where user_roles.Contains("receiver") || user_roles.Contains("admin") || (user_roles.Contains("requestor"))
                               select
                                   new
                                   {
                                       AgingMovement.status_desc,
                                       AgingMovement.material_type,
                                       AgingMovement.partno,
                                       AgingMovement.po_no,
                                       AgingMovement.project_name,
                                       AgingMovement.storage_requirement,
                                       AgingMovement.supplier_name,
                                       AgingMovement.pic_name,
                                       AgingMovement.order_type,
                                       AgingMovement.last_updated,
                                       AgingMovement.aging,
                                   });

                //var mstData = (from temp in _context.mst_material_plant select temp);
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection)))
                {
                    mstData = mstData.OrderBy(sortColumn + " " + sortColumnDirection);
                }
                if (!string.IsNullOrEmpty(searchValue))
                {
                    mstData = mstData.Where(m => m.partno.Contains(searchValue));
                }
                for (int i = 0; i < 8; i++)
                {
                    var searchColVal = Request.Form["columns[" + i.ToString() + "][search][value]"];
                    var fieldName = Request.Form["columns[" + i.ToString() + "][data]"].FirstOrDefault();
                    if (!string.IsNullOrEmpty(searchColVal))
                    {
                        if (fieldName == "status_desc")
                        {
                            mstData = mstData.Where(m => m.status_desc.Contains(searchColVal));
                        }
                        else if (fieldName == "material_type")
                        {
                            mstData = mstData.Where(m => m.material_type.Contains(searchColVal));
                        }
                        else if (fieldName == "partno")
                        {
                            mstData = mstData.Where(m => m.partno.Contains(searchColVal));
                        }
                        else if (fieldName == "po_no")
                        {
                            mstData = mstData.Where(m => m.po_no.Contains(searchColVal));
                        }
                        else if (fieldName == "project_name")
                        {
                            mstData = mstData.Where(m => m.project_name.Contains(searchColVal));
                        }
                        else if (fieldName == "storage_requirement")
                        {
                            mstData = mstData.Where(m => m.storage_requirement.Contains(searchColVal));
                        }
                        else if (fieldName == "supplier_name")
                        {
                            mstData = mstData.Where(m => m.supplier_name.Contains(searchColVal));
                        }
                        else if (fieldName == "order_type")
                        {
                            mstData = mstData.Where(m => m.order_type.Contains(searchColVal));
                        }
                    }
                }
                var agingCategory = Request.Form["aging_category"].FirstOrDefault();
                if (!string.IsNullOrEmpty(agingCategory))
                {
                    if (agingCategory == "<= 3 Months")
                        mstData = mstData.Where(m => m.aging <= 90);
                    else if (agingCategory == "<= 6 Months")
                        mstData = mstData.Where(m => m.aging > 90 && m.aging <= 180);
                    else if (agingCategory == "<= 12 Months")
                        mstData = mstData.Where(m => m.aging > 180 && m.aging <= 365);
                    else if (agingCategory == "> 1 Year")
                        mstData = mstData.Where(m => m.aging > 365);
                }

                recordsTotal = mstData.Count();
                var data = mstData.Skip(skip).Take(pageSize).ToList();
                var jsonData = new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data };
                return Ok(jsonData);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public IActionResult AgingMovementPartial(string agingCategory)
        {
            ViewBag.AgingCategory = agingCategory;
            return PartialView("_AgingMovementPartial");
        }


        public IActionResult RequestSchedule()
        {
            return View();
        }
        [HttpPost]
        public IActionResult CreateSchedule([FromBody] ScheduledRequest model)
        {
            try
            {
                string fullIdentity = User.Identity?.Name ?? "";
                string sesaId = fullIdentity.Contains("\\")
                    ? fullIdentity.Split('\\')[1]
                    : fullIdentity;
                if (string.IsNullOrEmpty(sesaId))
                {
                    sesaId = User.FindFirstValue("sesa_id")
                        ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
                }
                if (string.IsNullOrEmpty(sesaId))
                {
                    return Json(new
                    {
                        success = false,
                        message = "SESA ID cannot be identified. Please re-login."
                    });
                }
                string userEmail = User.FindFirstValue(ClaimTypes.Email)
                    ?? "no-email@company.com";

                DatabaseAccessLayer dal = new DatabaseAccessLayer();
                int newId = dal.CreateSchedule(
                    sesaId,
                    userEmail,
                    model.ScheduledDate,
                    model.Title,
                    model.Description,
                    model.SendEmail  // ← tambah ini
                );
                return Json(new
                {
                    success = true,
                    message = "Schedule created successfully!",
                    id = newId
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Error: " + ex.Message
                });
            }
        }
        [HttpGet]
        public IActionResult GetSchedules()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== GetSchedules CALLED ===");
                string fullIdentity = User.Identity?.Name ?? "";
                string sesaId = fullIdentity.Contains("\\")
                    ? fullIdentity.Split('\\')[1]
                    : fullIdentity;
                if (string.IsNullOrEmpty(sesaId))
                {
                    sesaId = User.FindFirstValue("sesa_id")
                        ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
                }
                if (string.IsNullOrEmpty(sesaId))
                    return Json(new { success = false, message = "User not authenticated" });

                // ← TAMBAH INI: cek role user
                List<string> userRoles = User.Claims
                    .Where(c => c.Type == "semb_erp_role")
                    .Select(c => c.Value)
                    .ToList();
                bool isAdmin = userRoles.Contains("admin");

                DatabaseAccessLayer dal = new DatabaseAccessLayer();

                // ← UBAH INI: kalau Admin ambil semua, kalau bukan ambil milik sendiri
                var schedules = isAdmin
                    ? dal.GetAllSchedules()
                    : dal.GetUserSchedules(sesaId);

                var data = new List<object>();
                foreach (DataRow row in schedules.Rows)
                {
                    var scheduledDate = Convert.ToDateTime(row["scheduled_date"]);
                    var formattedDate = scheduledDate.ToString("yyyy-MM-ddTHH:mm:ss",
                        System.Globalization.CultureInfo.InvariantCulture);
                    data.Add(new
                    {
                        id = Convert.ToInt32(row["id"]),
                        title = row["title"].ToString(),
                        start = formattedDate,
                        description = row["description"]?.ToString() ?? "",
                        isCompleted = Convert.ToBoolean(row["is_completed"])
                    });
                }
                return Json(new { success = true, data = data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpGet]
        public IActionResult GetScheduleDetailsPartial(string category)
        {
            try
            {
                var list = new List<dynamic>();
                using (SqlConnection conn = new SqlConnection(_context.Database.GetDbConnection().ConnectionString))
                {
                    string sql = @"SELECT [id], [scheduled_date], [title], [description], [is_completed]
                     FROM [dbo].[scheduled_requests]
                     WHERE 1=1";

                    if (category == "UPCOMING")
                        sql += " AND is_completed = 0 AND scheduled_date >= GETDATE() ORDER BY scheduled_date ASC";
                    else if (category == "COMPLETED")
                        sql += " AND is_completed = 1 ORDER BY scheduled_date DESC";
                    else
                        sql += " ORDER BY scheduled_date DESC";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        if (conn.State == ConnectionState.Closed) conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                list.Add(new
                                {
                                    id = reader["id"],
                                    scheduled_date = reader["scheduled_date"],
                                    title = reader["title"].ToString(),
                                    description = reader["description"]?.ToString(),
                                    is_completed = reader["is_completed"]
                                });
                            }
                        }
                    }
                }
                ViewBag.Category = category;
                return PartialView("_ScheduleDetailsTable", list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching schedule details");
                return Content("<div class='alert alert-danger'>Error: " + ex.Message + "</div>");
            }
        }
        //[HttpPost]
        //public IActionResult CreateSchedule([FromBody] ScheduledRequest model)
        //{
        //    try
        //    {
        //        // Ambil SESA ID dari user yang login
        //        string fullIdentity = User.Identity?.Name ?? "";
        //        string sesaId = fullIdentity.Contains("\\")
        //            ? fullIdentity.Split('\\')[1]
        //            : fullIdentity;

        //        if (string.IsNullOrEmpty(sesaId))
        //        {
        //            sesaId = User.FindFirstValue("sesa_id")
        //                ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        //        }

        //        if (string.IsNullOrEmpty(sesaId))
        //        {
        //            return Json(new
        //            {
        //                success = false,
        //                message = "SESA ID cannot be identified. Please re-login."
        //            });
        //        }

        //        string userEmail = User.FindFirstValue(ClaimTypes.Email)
        //            ?? "no-email@company.com";

        //        // Insert ke database
        //        DatabaseAccessLayer dal = new DatabaseAccessLayer();
        //        int newId = dal.CreateSchedule(
        //            sesaId,
        //            userEmail,
        //            model.ScheduledDate,
        //            model.Title,
        //            model.Description
        //        );

        //        return Json(new
        //        {
        //            success = true,
        //            message = "Schedule created successfully!",
        //            id = newId
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new
        //        {
        //            success = false,
        //            message = "Error: " + ex.Message
        //        });
        //    }
        //}

        [HttpPost]
        public async Task<IActionResult> MarkScheduleCompleted([FromBody] int scheduleId)
        {
            try
            {
                // ✅ Update langsung pakai SQL
                var sql = "UPDATE scheduled_requests SET is_completed = 1 WHERE id = @id";

                await _context.Database.ExecuteSqlRawAsync(sql,
                    new SqlParameter("@id", scheduleId));

                return Json(new { success = true, message = "Schedule marked as completed" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking schedule as completed");
                return Json(new { success = false, message = ex.Message });
            }
        }
        public IActionResult GetAgingMovementChart()
        {
            var db = new DatabaseAccessLayer();
            List<ChartModel> result = db.GetAgingMovementChart();

            return Json(result);
        }

        [Authorize(Policy = "RequireRequestor")]
        public IActionResult AddNewShipment()
        {
            return this.CheckSession(() =>
            {
                var db = new DatabaseAccessLayer();
                string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                string name = User.FindFirst("semb_erp_name")?.Value;
                string plant = db.GetUserPlant(sesa_id);
                //List<string> supplierList = db.GetSupplierList();
                //ViewBag.supplierList = supplierList;
                ViewBag.name = name;
                ViewBag.sesa_id = sesa_id;
                ViewBag.plant = plant;
                return View();
            });
        }
        [Authorize(Policy = "RequireRequestor")]
        public IActionResult UploadShipment(IFormFile file_upload)
        {
            string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (sesa_id == "")
            {
                return Content("Session Timeout, Please relogin!!", "text/plain");
            }
            else
            {
                DateTime now = DateTime.Now;
                string id_upload = now.ToString("yyMMddHHmmssfff");
                _importexportFactory.ImportShipment(file_upload, id_upload, sesa_id);

                var db = new DatabaseAccessLayer();
                List<ShipmentTempModel> dataTemp = db.GetTempShipment(id_upload, sesa_id);
                ViewBag.id_upload = id_upload;
                //return Content("Upload Success!!", "text/plain");

                return PartialView("_TableTempShipment", dataTemp);
            }
        }
        [Authorize(Policy = "RequireRequestor")]
        [HttpPost]
        public async Task<IActionResult> SubmitUploadShipment(string id_upload)
        {
            string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (sesa_id == "")
            {
                return Content("Session Timeout, Please relogin!!", "text/plain");
            }
            else
            {
                var db = new DatabaseAccessLayer();
                string submit = db.SubmitUploadShipment(id_upload, sesa_id);
                return Content("success;Succesfully Submitted!", "text/plain");
            }
        }
        public IActionResult ShipmentList()
        {
            return this.CheckSession(() =>
            {
                var db = new DatabaseAccessLayer();
                string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                string name = User.FindFirst("semb_erp_name")?.Value;
                string plant = db.GetUserPlant(sesa_id);
                List<string> userRoles = User.Claims
                                            .Where(c => c.Type == "semb_erp_role")
                                            .Select(c => c.Value)
                                            .ToList();
                List<string> catList = db.GET_CAT_NON_CONF();
                ViewBag.catList = catList;
                ViewBag.name = name;
                ViewBag.sesa_id = sesa_id;
                ViewBag.plant = plant;
                ViewBag.userRoles = userRoles;
                return View();
            });
        }

        public IActionResult GET_SHIPMENT_LIST()
        {
            try
            {
                string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                List<string> user_roles = User.Claims
                                            .Where(c => c.Type == "semb_erp_role")
                                            .Select(c => c.Value)
                                            .ToList();
                var db = new DatabaseAccessLayer();
                List<UserDetailModel> userDetail = db.GetUserDetail(sesa_id);
                var user = userDetail.First();

                var draw = Request.Form["draw"].FirstOrDefault();
                var start = Request.Form["start"].FirstOrDefault();
                var length = Request.Form["length"].FirstOrDefault();
                //var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
                var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][data]"].FirstOrDefault();
                var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
                var searchValue = Request.Form["search[value]"].FirstOrDefault();
                var column0Value = Request.Form["columns[0][search][value]"];
                var column1Value = Request.Form["columns[1][search][value]"];
                var column2Value = Request.Form["columns[2][search][value]"];
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int recordsTotal = 0;

                // Filter for OPEN shipments only (status != CLOSED)
                var mstData = (from ShipmentList in _context.v_shipment
                               where ShipmentList.inserted_by == sesa_id && ShipmentList.status_shipment != "CLOSED"
                               select
                                   new
                                   {
                                       ShipmentList.id_shipment,
                                       ShipmentList.id_upload,
                                       ShipmentList.project_name,
                                       ShipmentList.stage_name,
                                       ShipmentList.wo_no,
                                       ShipmentList.partno,
                                       ShipmentList.revision,
                                       ShipmentList.qty,
                                       ShipmentList.is_coated,
                                       ShipmentList.ship_date,
                                       ShipmentList.status_shipment,
                                       ShipmentList.inserted_by_name,
                                       ShipmentList.pic_department,
                                       received_date = ShipmentList.received_date.HasValue ? ShipmentList.received_date.Value : (DateTime?)null,
                                       ShipmentList.received_by_name,
                                       ShipmentList.storage_dest,
                                       ShipmentList.gatepass_no
                                   });

                //var mstData = (from temp in _context.mst_material_plant select temp);
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection)))
                {
                    mstData = mstData.OrderBy(sortColumn + " " + sortColumnDirection);
                }
                if (!string.IsNullOrEmpty(searchValue))
                {
                    mstData = mstData.Where(m => m.partno.Contains(searchValue)
                                                || m.project_name.Contains(searchValue)
                                                || m.wo_no.Contains(searchValue));
                }

                for (int i = 0; i < 16; i++)
                {
                    var searchColVal = Request.Form["columns[" + i.ToString() + "][search][value]"];
                    var fieldName = Request.Form["columns[" + i.ToString() + "][data]"].FirstOrDefault();
                    if (!string.IsNullOrEmpty(searchColVal))
                    {
                        if (fieldName == "project_name")
                        {
                            mstData = mstData.Where(m => m.project_name.Contains(searchColVal));
                        }
                        else if (fieldName == "stage_name")
                        {
                            mstData = mstData.Where(m => m.stage_name.Contains(searchColVal));
                        }
                        else if (fieldName == "wo_no")
                        {
                            mstData = mstData.Where(m => m.wo_no.Contains(searchColVal));
                        }
                        else if (fieldName == "partno")
                        {
                            mstData = mstData.Where(m => m.partno.Contains(searchColVal));
                        }
                        else if (fieldName == "revision")
                        {
                            mstData = mstData.Where(m => m.revision.Contains(searchColVal));
                        }
                        else if (fieldName == "qty")
                        {
                            mstData = mstData.Where(m => m.qty.ToString().Contains(searchColVal));
                        }
                        else if (fieldName == "is_coated")
                        {
                            mstData = mstData.Where(m => m.is_coated.Contains(searchColVal));
                        }
                        else if (fieldName == "status_shipment")
                        {
                            mstData = mstData.Where(m => m.status_shipment.Contains(searchColVal));
                        }
                        else if (fieldName == "inserted_by_name")
                        {
                            mstData = mstData.Where(m => m.inserted_by_name.Contains(searchColVal));
                        }
                        else if (fieldName == "pic_department")
                        {
                            mstData = mstData.Where(m => m.pic_department.Contains(searchColVal));
                        }
                        else if (fieldName == "received_by_name")
                        {
                            mstData = mstData.Where(m => m.received_by_name.Contains(searchColVal));
                        }
                        else if (fieldName == "storage_dest")
                        {
                            mstData = mstData.Where(m => m.storage_dest.Contains(searchColVal));
                        }
                        else if (fieldName == "gatepass_no")
                        {
                            mstData = mstData.Where(m => m.gatepass_no.Contains(searchColVal));
                        }
                    }
                }

                recordsTotal = mstData.Count();
                var data = mstData.Skip(skip).Take(pageSize).ToList();
                var jsonData = new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data };
                return Ok(jsonData);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpPost]
        public IActionResult ReceiveShipment(string id_shipment)
        {
            string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (sesa_id == "")
            {
                return Content("Session Timeout, Please relogin!!", "text/plain");
            }
            else
            {
                var db = new DatabaseAccessLayer();
                string result = db.ReceiveShipment(id_shipment, sesa_id);
                return Content(result, "text/plain");
            }
        }
        [HttpPost]
        public IActionResult GetRequestorSummary()
        {
            try
            {
                string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                List<string> user_roles = User.Claims
                    .Where(c => c.Type == "semb_erp_role")
                    .Select(c => c.Value)
                    .ToList();

                // ===== ORDER SUMMARY (chart 1) =====
                var baseQuery = _context.v_order
                    .Where(o => o.pic == sesa_id || user_roles.Contains("admin"));

                // ===== PICKING SUMMARY (chart 2) =====
                var pickingQuery = _context.v_request
                    .Where(r => r.requested_by == sesa_id || user_roles.Contains("admin"));

                var pickingSummary = new
                {
                    request = pickingQuery.Count(x => x.status_desc.Trim().ToLower() == "request"),
                    startPicking = pickingQuery.Count(x => x.status_desc.Trim().ToLower() == "start picking"),
                    picking = pickingQuery.Count(x => x.status_desc.Trim().ToLower() == "picking"),
                    consolidation = pickingQuery.Count(x => x.status_desc.Trim().ToLower() == "consolidation"),
                    transferring = pickingQuery.Count(x => x.status_desc.Trim().ToLower() == "transfering"),
                    plantReceived = pickingQuery.Count(x => x.status_desc.Trim().ToLower() == "plant received"), // ← tambah ini
                    supplied = pickingQuery.Count(x => x.status_desc.Trim().ToLower() == "supplied"),
                    donePicking = pickingQuery.Count(x => x.status_desc.Trim().ToLower() == "done picking")
                };

                // ===== RECENT PICKING =====
                var recentPicking = pickingQuery
                    .OrderByDescending(r => r.record_date)
                    .Take(5)
                    .Select(r => new {
                        r.id_request,
                        r.request_no,
                        r.status_desc,
                        r.requested_by_name,
                        r.record_date
                    })
                    .ToList();

                return Json(new
                {
                    submission = baseQuery.Count(o => o.status_desc.Trim().ToUpper() == "SUBMISSION"),
                    received = baseQuery.Count(o => o.status_desc.Trim().ToUpper() == "RECEIVED"),
                    putaway = baseQuery.Count(o => o.status_desc.Trim().ToUpper() == "PUTAWAY"),
                    partiallyPicked = baseQuery.Count(o => o.status_desc.Trim().ToUpper() == "PARTIALLY PICKED"),
                    closed = baseQuery.Count(o => o.status_desc.Trim().ToUpper() == "CLOSED"),

                    pickingSummary,
                    recentPicking
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        public IActionResult GET_SHIPMENT_LIST_CLOSE()
        {
            try
            {
                string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                List<string> user_roles = User.Claims
                                            .Where(c => c.Type == "semb_erp_role")
                                            .Select(c => c.Value)
                                            .ToList();
                var db = new DatabaseAccessLayer();
                List<UserDetailModel> userDetail = db.GetUserDetail(sesa_id);
                var user = userDetail.First();

                var draw = Request.Form["draw"].FirstOrDefault();
                var start = Request.Form["start"].FirstOrDefault();
                var length = Request.Form["length"].FirstOrDefault();
                //var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
                var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][data]"].FirstOrDefault();
                var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
                var searchValue = Request.Form["search[value]"].FirstOrDefault();
                var column0Value = Request.Form["columns[0][search][value]"];
                var column1Value = Request.Form["columns[1][search][value]"];
                var column2Value = Request.Form["columns[2][search][value]"];
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int recordsTotal = 0;

                // Filter for CLOSED shipments only
                var mstData = (from ShipmentList in _context.v_shipment
                               where ShipmentList.inserted_by == sesa_id && ShipmentList.status_shipment == "CLOSED"
                               select
                                   new
                                   {
                                       ShipmentList.project_name,
                                       ShipmentList.stage_name,
                                       ShipmentList.wo_no,
                                       ShipmentList.partno,
                                       ShipmentList.revision,
                                       ShipmentList.qty,
                                       ShipmentList.is_coated,
                                       ShipmentList.ship_date,
                                       ShipmentList.status_shipment,
                                       ShipmentList.inserted_by_name,
                                       received_date = ShipmentList.received_date.HasValue ? ShipmentList.received_date.Value : (DateTime?)null,
                                       ShipmentList.received_by_name,
                                       ShipmentList.storage_dest,
                                       ShipmentList.gatepass_no,
                                       closed_date = ShipmentList.closed_date.HasValue ? ShipmentList.closed_date.Value : (DateTime?)null
                                   });

                //var mstData = (from temp in _context.mst_material_plant select temp);
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection)))
                {
                    mstData = mstData.OrderBy(sortColumn + " " + sortColumnDirection);
                }
                if (!string.IsNullOrEmpty(searchValue))
                {
                    mstData = mstData.Where(m => m.partno.Contains(searchValue)
                                                || m.project_name.Contains(searchValue)
                                                || m.wo_no.Contains(searchValue));
                }

                for (int i = 0; i < 14; i++)
                {
                    var searchColVal = Request.Form["columns[" + i.ToString() + "][search][value]"];
                    var fieldName = Request.Form["columns[" + i.ToString() + "][data]"].FirstOrDefault();
                    if (!string.IsNullOrEmpty(searchColVal))
                    {
                        if (fieldName == "project_name")
                        {
                            mstData = mstData.Where(m => m.project_name.Contains(searchColVal));
                        }
                        else if (fieldName == "stage_name")
                        {
                            mstData = mstData.Where(m => m.stage_name.Contains(searchColVal));
                        }
                        else if (fieldName == "wo_no")
                        {
                            mstData = mstData.Where(m => m.wo_no.Contains(searchColVal));
                        }
                        else if (fieldName == "partno")
                        {
                            mstData = mstData.Where(m => m.partno.Contains(searchColVal));
                        }
                        else if (fieldName == "revision")
                        {
                            mstData = mstData.Where(m => m.revision.Contains(searchColVal));
                        }
                        else if (fieldName == "qty")
                        {
                            mstData = mstData.Where(m => m.qty.ToString().Contains(searchColVal));
                        }
                        else if (fieldName == "is_coated")
                        {
                            mstData = mstData.Where(m => m.is_coated.Contains(searchColVal));
                        }
                        else if (fieldName == "status_shipment")
                        {
                            mstData = mstData.Where(m => m.status_shipment.Contains(searchColVal));
                        }
                        else if (fieldName == "inserted_by_name")
                        {
                            mstData = mstData.Where(m => m.inserted_by_name.Contains(searchColVal));
                        }
                        else if (fieldName == "received_by_name")
                        {
                            mstData = mstData.Where(m => m.received_by_name.Contains(searchColVal));
                        }
                        else if (fieldName == "storage_dest")
                        {
                            mstData = mstData.Where(m => m.storage_dest.Contains(searchColVal));
                        }
                        else if (fieldName == "gatepass_no")
                        {
                            mstData = mstData.Where(m => m.gatepass_no.Contains(searchColVal));
                        }
                    }
                }

                recordsTotal = mstData.Count();
                var data = mstData.Skip(skip).Take(pageSize).ToList();
                var jsonData = new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data };
                return Ok(jsonData);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public IActionResult GetRecentNonConf()
        {
            string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            List<string> user_roles = User.Claims
                .Where(c => c.Type == "semb_erp_role")
                .Select(c => c.Value)
                .ToList();

            var data = _context.V_NON_CONF
                .Where(x => x.created_by_sesa == sesa_id
                    || user_roles.Contains("receiver")
                    || user_roles.Contains("admin")
                    || (user_roles.Contains("requestor") && x.pic_sesa == sesa_id))
                .OrderByDescending(x => x.id_non_conf)
                .Take(5)
                .Select(x => new {
                    x.id_non_conf,
                    x.partno,
                    x.qty,
                    x.uom,
                    x.supplier_name,
                    x.category_issue,
                    x.detail_issue,
                    x.file_doc
                })
                .ToList<dynamic>();

            return PartialView("_NonConfPartial", data);
        }

        [HttpPost]
        public IActionResult BinningShipment(string id_shipment, string storage_dest, string gatepass_no)
        {
            string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (sesa_id == "")
            {
                return Content("Session Timeout, Please relogin!!", "text/plain");
            }
            else
            {
                var db = new DatabaseAccessLayer();
                string result = db.BinningShipment(id_shipment, storage_dest, gatepass_no, sesa_id);
                return Content(result, "text/plain");
            }
        }

        [Authorize(Policy = "RequireRequestor")]
        [HttpPost]
        public IActionResult AddShipmentManual(string project_name, string stage_name, string wo_no,
            string partno, string revision, decimal qty, string is_coated, DateTime ship_date)
        {
            DateTime now = DateTime.Now;
            string id_upload = now.ToString("yyMMddHHmmssfff");
            string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(sesa_id))
            {
                return Content("error;Session Timeout, Please relogin!!", "text/plain");
            }
            else
            {
                try
                {
                    var db = new DatabaseAccessLayer();
                    string result = db.InsertShipmentManual(id_upload, project_name, stage_name, wo_no, partno,
                        revision, qty, is_coated, ship_date, sesa_id);
                    return Content(result, "text/plain");
                }
                catch (Exception ex)
                {
                    return Content($"error;Failed to add shipment: {ex.Message}", "text/plain");
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSchedule([FromBody] int id)
        {
            try
            {
                var schedule = await _context.ScheduledRequests.FindAsync(id);
                if (schedule == null)
                    return Json(new { success = false, message = "Schedule not found." });

                _context.ScheduledRequests.Remove(schedule);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public IActionResult DashboardAdmin()
        {
            return View();
        }

        [Authorize(Policy = "RequireRequestorAdmin")]
        [HttpPost]
        public IActionResult DeleteShipment(string id_shipment)
        {
            string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(sesa_id))
            {
                return Content("error;Session Timeout, Please relogin!!", "text/plain");
            }
            else
            {
                try
                {
                    var db = new DatabaseAccessLayer();
                    string result = db.DeleteShipment(id_shipment, sesa_id);
                    return Content(result, "text/plain");
                }
                catch (Exception ex)
                {
                    return Content($"error;Failed to delete shipment: {ex.Message}", "text/plain");
                }
            }


        }
    }
}

