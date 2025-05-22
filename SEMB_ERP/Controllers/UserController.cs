using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SEMB_ERP.Function;
using SEMB_ERP.Models;
using SEMB_ERP.Service;
using System.Security.Claims;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using Org.BouncyCastle.Asn1.Ocsp;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using MailKit.Search;
using Org.BouncyCastle.Bcpg;

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
                    string filePath = getNextFileName(_environment.WebRootPath + "\\Documents\\" + id_upload + " - " +file_support.FileName);
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
                    string filePath = getNextFileName(_environment.WebRootPath + "\\Documents\\" + id_upload + " - " + file_support.FileName);
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

        [HttpPost]
        public async Task<IActionResult> UpdateOrder(IFormFile file_support, string id_order, string material_type, string partno, string po_no, double qty, string uom, string revision, string project_name,
            string storage_requirement, string supplier_name, string order_type, double unit_price, double length_mm, double width_mm, double height_mm, string remark)
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
                string submit = db.UpdateOrder(id_order, id_upload, material_type, partno, po_no, qty, uom, revision, project_name, storage_requirement, supplier_name,
                                                order_type, unit_price, length_mm, width_mm, height_mm, remark, file_support_db, sesa_id);
                return Content("success;Updated Succesfully!", "text/plain");
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
                string name = User.FindFirst("semb_erp_name")?.Value;
                ViewBag.name = name;
                ViewBag.sesa_id = sesa_id;
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
                                       OrderList.file_support,
                                       OrderList.status_code,
                                       OrderList.status_desc,
                                       OrderList.pic_name,
                                       OrderList.remark
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
                for (int i = 0; i < 15; i++)
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
                        else if (fieldName == "qty")
                        {
                            mstData = mstData.Where(m => m.qty.ToString().Contains(searchColVal));
                        }
                        else if (fieldName == "picked_qty")
                        {
                            mstData = mstData.Where(m => m.qty.ToString().Contains(searchColVal));
                        }
                        else if (fieldName == "available_qty")
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
        public IActionResult SubmitGR(string id_order_spq_string)
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
                List<string> catList = db.GET_CAT_NON_CONF();
                ViewBag.catList = catList;
                ViewBag.name = name;
                ViewBag.sesa_id = sesa_id;
                ViewBag.plant = plant;
                return View();
            });
        }

        public IActionResult GET_NON_CONF_LIST()
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
                var mstData = (from NonConfList in _context.V_NON_CONF
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
                                       NonConfList.created_by
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

        [Authorize(Policy = "RequireRequestor")]
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

        [Authorize(Policy = "RequireRequestor")]
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
        public IActionResult InsertBinItem(string box_id , string sesa_id)
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
            string comfirmResult = db.ConfirmBinItem(binId, sesa_id);

            // Return the result directly
            return Content(comfirmResult, "text/plain");
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
        [HttpPost]
        public IActionResult SubmitReqPicking(string remark)
        {
            string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var db = new DatabaseAccessLayer();
            string submit = db.SubmitReqPicking(remark ?? "", sesa_id);
            return Content(submit, "text/plain");
        }
        [Authorize(Policy = "RequireRequestor")]
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
                string name = User.FindFirst("semb_erp_name")?.Value;
                ViewBag.name = name;
                ViewBag.sesa_id = sesa_id;
                ViewBag.listStatus = listStatus;
                ViewBag.userRoles = userRoles;
                return View(userDetail);
            });
        }

        public IActionResult RequestMonitoring()
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
        public IActionResult GetRequestList()
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
                var mstData = (from RequestList in _context.v_request
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
            ViewBag.reqInfo = reqInfo;
            ViewBag.boxList = boxList;
            //return Content("Upload Success!!", "text/plain");

            return PartialView("_TableRequestDetail", dataList);
        }
        [Authorize(Policy = "RequireRequestor")]
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
        [Authorize(Policy = "RequireRequestor")]
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
        [Authorize(Policy = "RequireRequestor")]
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
        [Authorize(Policy = "RequireRequestor")]
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
        [Authorize(Policy = "RequireRequestor")]
        public IActionResult ScanBin(int id_request, int id_det)
        {
            string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            string name = User.FindFirst("semb_erp_name")?.Value;
            List<string> userRoles = User.Claims
                                        .Where(c => c.Type == "semb_erp_role")
                                        .Select(c => c.Value)
                                        .ToList();
            var db = new DatabaseAccessLayer();
            var pickInfo = JsonConvert.DeserializeObject<PickingModel>(db.GetBinPicking(id_request, id_det,sesa_id));
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
        [Authorize(Policy = "RequireRequestor")]
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
        [Authorize(Policy = "RequireRequestor")]
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
        [Authorize(Policy = "RequireRequestor")]
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
                var mstData = (from RequestDetail in _context.v_request_detail
                               select
                                   new
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

                //var mstData = (from temp in _context.mst_material_plant select temp);
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection)))
                {
                    mstData = mstData.OrderBy(sortColumn + " " + sortColumnDirection);
                }
                if (!string.IsNullOrEmpty(searchValue))
                {
                    mstData = mstData.Where(m => m.request_no.Contains(searchValue)
                                                || m.partno.Contains(searchValue)
                                                || m.status_picking.Contains(searchValue));
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
        public IActionResult OpenVariance(int id_det)
        {
            string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var db = new DatabaseAccessLayer();
            string upd = db.OpenVariance(id_det, sesa_id);

            return Content(upd, "text/plain");
        }
        [Authorize(Policy = "RequireRequestor")]
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
        [Authorize(Policy = "RequireRequestor")]
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
                PartNo= item.PartNo,
                Qty = item.Qty,
                Sbin = item.Sbin,
                Unit = item.Unit,
                Pstats = item.Pstats,
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
                               where Pallet.status_pallet=="CREATION" || Pallet.status_pallet=="TRANSFER"
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
        [Authorize(Policy = "RequireRequestor")]
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
               requested_by = item.requested_by,
               requested_by_name = item.requested_by_name,
               department = item.department
            });

            return Json(result);
        }

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
                               where Pallet.status_pallet=="CREATION" || Pallet.status_pallet=="TRANSFER" || Pallet.status_pallet == "RECEIVED"
                               select
                                   new
                                   {
                                       Pallet.id_pallet,
                                       Pallet.id_request,
                                       Pallet.request_no,
                                       Pallet.pallet_no,
                                       Pallet.status_pallet,
                                       Pallet.record_date,
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
                               where Pallet.status_pallet == "SUPPLIED"
                               select
                                   new
                                   {
                                       Pallet.id_pallet,
                                       Pallet.id_request,
                                       Pallet.request_no,
                                       Pallet.pallet_no,
                                       Pallet.status_pallet,
                                       Pallet.record_date,
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

        public IActionResult UpdateReceived(string id_pallet, string sesa_id)
        {
            var db = new DatabaseAccessLayer();
            string Result = db.UpdateReceived(id_pallet, sesa_id);

            // Return the result directly
            return Content(Result, "text/plain");
        }

        public IActionResult UpdateSupplied(string id_pallet, string sesa_id)
        {
            var db = new DatabaseAccessLayer();
            string Result = db.UpdateSupplied(id_pallet, sesa_id);

            // Return the result directly
            return Content(Result, "text/plain");
        }
    }
}
