using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SEMB_ERP.Function;
using SEMB_ERP.Models;
using SEMB_ERP.Service;
using System.Security.Claims;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Http;

namespace SEMB_ERP.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ImportExportFactory _importexportFactory;
        //private readonly FileManagementService _fileManagement;
        private readonly ILogger<UserController> _logger;
        private readonly IWebHostEnvironment _environment;
        public UserController(ImportExportFactory importexportFactory, ILogger<UserController> logger, ApplicationDbContext context, IWebHostEnvironment environment)
        {
            this._context = context;
            _importexportFactory = importexportFactory;
            //_fileManagement = fileManagement;
            _logger = logger;
            _environment = environment;
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
                //List<string> supplierList = db.GetSupplierList();
                //ViewBag.supplierList = supplierList;
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
                //return Content("Upload Success!!", "text/plain");

                return PartialView("_TableTempOrder", dataTemp);
            }
        }
        [Authorize(Policy = "RequireRequestor")]
        [HttpPost]
        public async Task<IActionResult> SubmitUploadOrder(IFormFile file_support, string id_upload)
        {
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
                    string filePath = getNextFileName(_environment.WebRootPath + "\\Documents\\" + id_upload + "-" +file_support.FileName);
                    //filePaths.Add(filePath);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file_support.CopyToAsync(stream);
                        file_support_db = Path.GetFileName(filePath);
                    }
                }
                string submit = db.SubmitUploadOrder(file_support_db, id_upload, sesa_id);
                return Content("success;Succesfully Submitted!", "text/plain");
            }
        }
        [Authorize(Policy = "RequireRequestor")]
        [HttpPost]
        public async Task<IActionResult> SubmitOrder(IFormFile file_support, string material_type, string partno, string po_no, double qty, string uom, string revision, string project_name, 
            string storage_requirement, string supplier_name, string pic, string order_type, double unit_price, double length_mm, double width_mm, double height_mm, string remark)
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
                    string filePath = getNextFileName(_environment.WebRootPath + "\\Documents\\" + id_upload + "-" + file_support.FileName);
                    //filePaths.Add(filePath);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file_support.CopyToAsync(stream);
                        file_support_db = Path.GetFileName(filePath);
                    }
                }
                string submit = db.SubmitOrder(id_upload, material_type, partno, po_no, qty, uom, revision, project_name, storage_requirement, supplier_name, 
                                                pic, order_type, unit_price, length_mm, width_mm, height_mm, remark, file_support_db, sesa_id);
                return Content("success;Succesfully Submitted!", "text/plain");
            }
        }
        [Authorize(Policy = "RequireRequestor")]
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
                ViewBag.listStatus = listStatus;
                ViewBag.userRoles = userRoles;
                return View(userDetail);
            });
        }
        public IActionResult GetOrderList()
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
                var mstData = (from OrderList in _context.v_order
                               select
                                   new
                                   {
                                       OrderList.id_order,
                                       OrderList.material_type,
                                       OrderList.partno,
                                       OrderList.po_no,
                                       OrderList.qty,
                                       OrderList.uom,
                                       OrderList.revision,
                                       OrderList.project_name,
                                       OrderList.storage_requirement,
                                       OrderList.sbin,
                                       OrderList.supplier_name,
                                       OrderList.pic,
                                       OrderList.order_type,
                                       OrderList.length_mm,
                                       OrderList.width_mm,
                                       OrderList.height_mm,
                                       OrderList.unit_price,
                                       OrderList.file_support,
                                       OrderList.status_code,
                                       OrderList.status_desc
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
                for (int i = 0; i < 13; i++)
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
                        else if (fieldName == "qty")
                        {
                            mstData = mstData.Where(m => m.qty.ToString().Contains(searchColVal));
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
                        else if (fieldName == "pic")
                        {
                            mstData = mstData.Where(m => m.pic.Contains(searchColVal));
                        }
                        else if (fieldName == "order_type")
                        {
                            mstData = mstData.Where(m => m.order_type.Contains(searchColVal));
                        }
                        else if (fieldName == "unit_price")
                        {
                            mstData = mstData.Where(m => m.unit_price.ToString().Contains(searchColVal));
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
                //return Content("Upload Success!!", "text/plain");

                return PartialView("_TableGR", dataGR);
            }
        }
        [HttpPost]
        public async Task<IActionResult> SubmitGR(string id_order_spq_string)
        {
            string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (sesa_id == "")
            {
                return Content("Session Timeout, Please relogin!!", "text/plain");
            }
            else
            {
                var db = new DatabaseAccessLayer();
                string submit = db.SubmitGR(id_order_spq_string, sesa_id);
                return Content(submit, "text/plain");
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
        [Authorize(Policy = "RequireRequestor")]
        public IActionResult AddNonConformity()
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
    }
}
