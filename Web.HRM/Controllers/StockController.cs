using DocumentFormat.OpenXml.VariantTypes;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Meo.Web.DBContext;
using Meo.Web.ViewModels;
using Microsoft.Ajax.Utilities;
using Microsoft.Office.Interop.Excel;
using NPOI.SS.Formula.Functions;
using PdfSharp.Pdf;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Windows.Interop;
using static ICSharpCode.SharpZipLib.Zip.ExtendedUnixData;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

namespace Web.HRM.Controllers
{
    public class StockController : Controller
    {
        DBContext db = new DBContext();

        #region -------------- Stock Receive --------------
        public ActionResult Receive()
        {
            try
            {
                if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    ViewData["Supplier"] = db.Suppliers.Where(e => e.Status.Equals(false)).ToList();
                    return View();
                }
                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public ActionResult _SearchStock()
        {
            ViewData["Supplier"] = db.Suppliers.Where(e => e.Status.Equals(false)).ToList();
            return PartialView();
        }

        public ActionResult PurchaseOrder(int? id, int? supid, string pono, string remarks, string flag)
        {
            try
            {
                if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    // Check if duplicate PONo exists
                    if (pono != null && pono != "")
                    {
                        var poExist = id > 0 ? db.StockReceives.FirstOrDefault(x => x.PONo == pono && x.Id != id) : db.StockReceives.FirstOrDefault(x => x.PONo == pono);//null;
                        if (poExist != null)
                        {
                            return Json(new { success = false, message = "Duplicate PO No detected." }, JsonRequestBehavior.AllowGet);
                        }
                    }

                    // Ensure previous added item got removed
                    if (flag == "new" || flag == "edit")
                    {
                        var poItemToRemove = db.ItemReceives.Where(x => x.POId == 0).ToList();
                        if (poItemToRemove.Count > 0)
                        {
                            foreach (var item in poItemToRemove)
                            {
                                var toDlt = db.ItemReceives.FirstOrDefault(x => x.Id == item.Id);
                                db.ItemReceives.Remove(toDlt);
                            }

                            db.SaveChanges();
                        }
                    }

                    // Proceed to create
                    PurchaseOrder po = new PurchaseOrder();
                    ViewData["Supplier"] = db.Suppliers.Where(x => x.Status == false && x.Active == false).ToList();
                    ViewData["Products"] = this.GetProductList();

                    if (id > 0)
                    {
                        var poDetails = db.StockReceives.Find(id);
                        po.Id = poDetails.Id;
                        po.PONo = poDetails.PONo;
                        po.SupplierId = poDetails.SupplierId;
                        po.PODate = poDetails.PODate;
                        po.Qty = poDetails.Qty;
                        po.TotalAmount = poDetails.TotalAmount;
                        po.Remarks = poDetails.Remarks;

                        po.ItemReceives =
                           (from i in db.ItemReceives
                            join p in db.Products on i.ProductId equals p.ProductId
                            where i.POId == id || i.POId == 0
                            select new UpdateItemReceive
                            {
                                Id = i.Id,
                                POId = i.POId,
                                PONo = pono,
                                ProductName = p.ProductName,
                                Qty = i.Qty,
                                UnitPrice = i.UnitPrice,
                                LineDiscAmt = i.LineDiscAmt,
                                LineTotal = i.LineTotal,
                                Remarks = i.Remarks,
                            }).ToList();
                    }
                    else
                    {
                        po.PONo = pono;
                        po.SupplierId = supid;
                        po.PODate = DateTime.Now;
                        po.Qty = 0;
                        po.TotalAmount = 0;
                        po.Remarks = remarks;

                        po.ItemReceives =
                            (from i in db.ItemReceives
                             join p in db.Products on i.ProductId equals p.ProductId
                             where i.POId == 0
                             select new UpdateItemReceive
                             {
                                 Id = i.Id,
                                 PONo = pono,
                                 ProductName = p.ProductName,
                                 Qty = i.Qty,
                                 UnitPrice = i.UnitPrice,
                                 LineDiscAmt = i.LineDiscAmt,
                                 LineTotal = i.LineTotal,
                                 Remarks = i.Remarks,
                             }).ToList();
                    }

                    po.subtotal = po.ItemReceives.Sum(item => item.LineTotal);
                    po.totalDisc = po.ItemReceives.Sum(item => item.LineDiscAmt);
                    po.totalQty = po.ItemReceives.Sum(item => item.Qty);

                    return View(po);
                }

                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public IList<ProductGIRO> GetProductList()
        {
            var proList = (from p in db.Products
                           join t in db.Types on p.TypeId equals t.TypeId
                           join s in db.Stock on p.ProductId equals s.ProductId into proGroup
                           from stk in proGroup.DefaultIfEmpty()
                           where p.Active == false && p.Status == false && (stk == null || stk.Status == false) && t.TypeName == "Product"
                           select new ProductGIRO
                           {
                               ProductId = p.ProductId,
                               ProductCode = p.ProductCode,
                               ProductName = p.ProductCode + " (" + p.ProductName + ") ",
                               TypeName = t.TypeName,
                               Cost = p.Cost,
                               Price = p.Price,
                               QtyAvailable = stk.QtyAvailable ?? 0,
                               // CreditBuy = p.CreditBuy,
                           }).ToList();

            return proList;
        }

        public ActionResult PreviewPO(int? id)
        {
            try
            {
                if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    PurchaseOrder po = new PurchaseOrder();

                    var typeid = db.Types.FirstOrDefault(e => e.Active.Equals(false) && e.Status.Equals(false) && e.Module == "Product" && e.TypeName == "Product").TypeId;
                    ViewBag.Products = db.Products.Where(x => x.Status == false && x.Active == false && x.TypeId == typeid).ToList();
                    ViewData["Products"] = ViewBag.Products;

                    po = (from sr in db.StockReceives
                          where sr.Id == id
                          select new PurchaseOrder
                          {
                              Id = sr.Id,
                              PONo = sr.PONo,
                              PODate = sr.PODate,
                              SupplierId = sr.SupplierId,
                              Qty = sr.Qty,
                              TotalAmount = sr.TotalAmount,
                              Remarks = sr.Remarks,
                              Status = sr.Status,
                              AddBy = sr.AddBy,
                              AddDate = sr.AddDate,
                              ModBy = sr.ModBy,
                              ModDate = sr.ModDate,

                              ItemReceives =
                              (from i in db.ItemReceives
                               join p in db.Products on i.ProductId equals p.ProductId into pgroup
                               from pr in pgroup.DefaultIfEmpty()
                               where i.POId == id
                               select new UpdateItemReceive
                               {
                                   POId = sr.Id,
                                   PONo = sr.PONo,
                                   ProductName = pr.ProductName,
                                   Qty = i.Qty,
                                   UnitPrice = i.UnitPrice,
                                   LineDiscAmt = i.LineDiscAmt,
                                   LineTotal = i.LineTotal,
                                   Remarks = i.Remarks,
                               }).ToList(),
                          }).FirstOrDefault();

                    po.subtotal = po.ItemReceives.Sum(item => item.LineTotal);
                    po.totalDisc = po.ItemReceives.Sum(item => item.LineDiscAmt);
                    po.totalQty = po.ItemReceives.Sum(item => item.Qty);

                    ViewData["SupplierName"] = po.SupplierId > 0 ? db.Suppliers.FirstOrDefault(x => x.Id == po.SupplierId).Name : "";

                    return View(po);
                }

                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public virtual ActionResult Read_StockReceive([DataSourceRequest] DataSourceRequest request)
        {
            return Json(db.StockReceives.Where(o => o.Status.Equals(false)).ToDataSourceResult(request, o => new StockReceives()
            {
                Id = o.Id,
                PONo = o.PONo,
                PODate = o.PODate,
                SupplierId = o.SupplierId,
                Qty = o.Qty,
                TotalAmount = o.TotalAmount,
                Remarks = o.Remarks,
                Status = o.Status,
                AddBy = o.AddBy,
                ModBy = o.ModBy,
                AddDate = o.AddDate,
                ModDate = o.ModDate,
            }), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Create_StockReceive(int? supid, string pono, string remarks, int? totalqty, decimal totalDisc, decimal subtotal)
        {
            try
            {
                // Create PO header
                if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    StockReceives po = new StockReceives()
                    {
                        PONo = pono,
                        PODate = DateTime.Now,
                        SupplierId = supid,
                        Qty = totalqty,
                        TotalDis = totalDisc,
                        TotalAmount = subtotal,
                        Remarks = remarks,
                        Status = false,
                        AddBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString(),
                        ModBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString(),
                        AddDate = DateTime.Now,
                        ModDate = DateTime.Now,
                    };

                    if (!string.IsNullOrEmpty(pono))
                    {
                        var isValid = db.StockReceives.FirstOrDefault(x => x.PONo == pono);
                        if (isValid != null)
                        {
                            //ModelState.AddModelError(string.Empty, "Duplicated PO No. found, please check!");
                            // Return JavaScript to close the window
                            string msg = "Duplicated PO No. found, please check!";
                            return Content(msg);
                        }
                    }

                    db.StockReceives.Add(po);
                    db.SaveChanges();

                    // Update PO header
                    List<ItemReceive> items = db.ItemReceives.Where(x => x.POId == 0).ToList();
                    if (items.Count > 0 && po.Id > 0)
                    {
                        foreach (var i in items)
                        {
                            i.POId = po.Id;
                            i.PONo = pono;
                            i.AddBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString();
                            i.ModBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString();
                            i.AddDate = DateTime.Now;
                            i.ModDate = DateTime.Now;
                            db.ItemReceives.Attach(i);
                            db.Entry(i).State = EntityState.Modified;

                            // update stock availability
                            var avai = db.Stock.FirstOrDefault(x => x.ProductId == i.ProductId);
                            if (avai != null)
                            {
                                avai.QtyAvailable += i.Qty;
                                avai.ModBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString();
                                avai.ModDate = DateTime.Now;
                                db.Stock.Attach(avai);
                                db.Entry(avai).State = EntityState.Modified;
                            }
                            else
                            {
                                var stock = new Stock()
                                {
                                    ProductId = i.ProductId,
                                    QtyAvailable = i.Qty,
                                    Status = false,
                                    AddBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString(),
                                    ModBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString(),
                                    AddDate = DateTime.Now,
                                    ModDate = DateTime.Now,
                                };

                                db.Stock.Add(stock);
                            }
                        }
                    }

                    db.SaveChanges();
                    ViewBag.PONo = pono;

                    return Content(po.Id.ToString() ?? "-1", "text/html");//RedirectToAction("PurchaseOrder", "Stock", new { pono = po.PONo });
                }
                else
                {
                    return RedirectToAction("Login", "Account");
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as appropriate for your application
                // For simplicity, rethrow the exception for now
                throw;
            }
        }

        [HttpPost]
        public ActionResult Update_StockReceive(int poid, int? supid, string pono, string remarks, int? totalqty, decimal totalDisc, decimal subtotal)
        {
            try
            {
                // Create PO header
                if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    StockReceives po = db.StockReceives.Find(poid);
                    po.PONo = string.IsNullOrEmpty(pono) ? null : pono;
                    po.PODate = DateTime.Now;
                    po.SupplierId = supid;
                    po.Qty = totalqty;
                    po.TotalDis = totalDisc;
                    po.TotalAmount = subtotal;
                    po.Remarks = remarks;
                    po.Status = false;
                    po.ModBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString();
                    po.ModDate = DateTime.Now;

                    if (!string.IsNullOrEmpty(pono))
                    {
                        var isValid = db.StockReceives.FirstOrDefault(x => x.PONo == pono && x.Id != poid);
                        if (isValid != null)
                        {
                            //ModelState.AddModelError(string.Empty, "Duplicated PO No. found, please check!");
                            // Return JavaScript to close the window
                            string msg = "Duplicated PO No. found, please check!";
                            return Content(msg);
                        }
                    }

                    db.StockReceives.Attach(po);
                    db.Entry(po).State = EntityState.Modified;
                    db.SaveChanges();

                    // Update PO header
                    List<ItemReceive> items = db.ItemReceives.Where(x => x.POId == 0).ToList();
                    if (items.Count > 0 && poid > 0)
                    {
                        foreach (var i in items)
                        {
                            i.POId = poid;
                            i.PONo = pono;
                            i.AddBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString();
                            i.ModBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString();
                            i.AddDate = DateTime.Now;
                            i.ModDate = DateTime.Now;
                            db.ItemReceives.Attach(i);
                            db.Entry(i).State = EntityState.Modified;

                            // update stock availability
                            var avai = db.Stock.FirstOrDefault(x => x.ProductId == i.ProductId);
                            if (avai != null)
                            {
                                avai.QtyAvailable += i.Qty;
                                avai.ModBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString();
                                avai.ModDate = DateTime.Now;
                                db.Stock.Attach(avai);
                                db.Entry(avai).State = EntityState.Modified;
                            }
                            else
                            {
                                var stock = new Stock()
                                {
                                    ProductId = i.ProductId,
                                    QtyAvailable = i.Qty,
                                    Status = false,
                                    AddBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString(),
                                    ModBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString(),
                                    AddDate = DateTime.Now,
                                    ModDate = DateTime.Now,
                                };

                                db.Stock.Add(stock);
                            }
                        }
                    }

                    db.SaveChanges();
                    ViewBag.PONo = pono;
                    return Content(poid.ToString() ?? "-1", "text/html");//RedirectToAction("PurchaseOrder", "Stock", new { pono = po.PONo });
                }
                else
                {
                    return RedirectToAction("Login", "Account");
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as appropriate for your application
                // For simplicity, rethrow the exception for now
                throw;
            }
        }

        [HttpPost]
        public ActionResult Destroy_StockReceive([DataSourceRequest] DataSourceRequest request, StockReceives stockReceive)
        {
            try
            {
                if (stockReceive != null && ModelState.IsValid)
                {
                    bool isError = false;
                    var header = db.StockReceives.Find(stockReceive.Id);
                    var subitem = db.ItemReceives.Where(x => x.POId == header.Id).ToList();

                    foreach (var sub in subitem)
                    {
                        var product = db.Stock.FirstOrDefault(p => p.ProductId == sub.ProductId);
                        if (product != null)
                        {
                            product.QtyAvailable -= sub.Qty;
                            product.ModBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString();
                            product.ModDate = DateTime.Now;
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "\nFailed to delete. \nProduct not found! Please contact your administator for more information.");
                            isError = true;
                            break;
                        }

                        if (product.QtyAvailable < 0)
                        {
                            ModelState.AddModelError(string.Empty, "\nFailed to delete. \nProduct quantity not sufficient! ");
                            isError = true;
                            break;
                        }
                    };

                    if (ModelState.IsValid)
                    {
                        header.Status = true;

                        db.StockReceives.Attach(header);
                        db.Entry(header).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }

                return Json(new[] { stockReceive }.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                return View("YourErrorViewName", ModelState);
            }
        }
        #endregion

        #region -------------- Item Receive -----------------
        [HttpPost]
        public ActionResult AddItemReceive(int? poid, string pono, int pid, int qty, decimal price, decimal disc, decimal total, int? supid, string remarks)
        {
            try
            {
                if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    var typeid = db.Products.FirstOrDefault(x => x.ProductId == pid).TypeId;
                    var pro = db.ItemReceives.FirstOrDefault(x => x.ProductId == pid && x.POId == poid);

                    if (pro != null)
                    {
                        pro.Qty += qty;
                        pro.LineTotal = pro.Qty * price;
                        db.ItemReceives.Attach(pro);
                        db.Entry(pro).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    else
                    {
                        var item = new ItemReceive
                        {
                            ProductId = pid,
                            Qty = qty,
                            UnitPrice = price,
                            LineDiscAmt = disc,
                            LineTotal = total,
                            AddBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString(),
                            AddDate = DateTime.Now,
                        };

                        db.ItemReceives.Add(item);
                        db.SaveChanges();
                    }

                    return Json(new { supid, pono, remarks });
                }

                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        [HttpPost]
        public ActionResult DeleteItemReceive(int id)
        {
            ItemReceive item = db.ItemReceives.Find(id);

            // Check if the item exists
            if (item == null)
            {
                ModelState.AddModelError(string.Empty, "Item not found.");
                return new HttpStatusCodeResult(HttpStatusCode.NotFound, "Item not found.");
            }

            if (item.POId > 0)
            {
                var stock = db.Stock.FirstOrDefault(x => x.ProductId == item.ProductId);
                if (stock != null)
                {
                    stock.QtyAvailable -= item.Qty;

                    if (stock.QtyAvailable < 0)
                    {
                        string msg = "Failed to delete. Product quantity not sufficient!";
                        return Json(new { success = false, msg });
                    }

                    stock.ModBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString();
                    stock.ModDate = DateTime.Now;
                    db.Stock.Attach(stock);
                    db.Entry(stock).State = EntityState.Modified;
                }
            }

            db.ItemReceives.Remove(item);
            db.SaveChanges();
            return Json(new { success = true });
        }
        #endregion

        #region -------------- Stock Return -------------- 
        public ActionResult Return()
        {
            try
            {
                if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    ViewData["Supplier"] = db.Suppliers.Where(e => e.Status.Equals(false)).ToList();
                    return View();
                }
                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public ActionResult _SearchReturn()
        {
            ViewData["Supplier"] = db.Suppliers.Where(e => e.Status.Equals(false)).ToList();
            return PartialView();
        }

        public virtual ActionResult Read_StockReturn([DataSourceRequest] DataSourceRequest request)
        {
            return Json(db.StockReturns.Where(o => o.Status.Equals(false)).ToDataSourceResult(request, o => new StockReturns()
            {
                Id = o.Id,
                CNNo = o.CNNo,
                CNDate = o.CNDate,
                SupplierId = o.SupplierId,
                Qty = o.Qty,
                //TotalDisc = o.TotalDisc,
                TotalAmount = o.TotalAmount,
                Remarks = o.Remarks,
                Status = o.Status,
                AddBy = o.AddBy,
                ModBy = o.ModBy,
                AddDate = o.AddDate,
                ModDate = o.ModDate,
            }), JsonRequestBehavior.AllowGet);
        }

        public ActionResult CreditNote(int? id, int? supid, string cnno, string remarks, string flag)
        {
            try
            {
                if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    // Check if duplicate CNNo exists
                    if (cnno != null && cnno != "")
                    {
                        var poExist = id > 0 ? db.StockReturns.FirstOrDefault(x => x.CNNo == cnno && x.Id != id) : db.StockReturns.FirstOrDefault(x => x.CNNo == cnno);//null;
                        if (poExist != null)
                        {
                            return Json(new { success = false, message = "Duplicate CN No detected." }, JsonRequestBehavior.AllowGet);
                        }
                    }

                    // Ensure previous added item got removed
                    if (flag == "new" || flag == "edit")
                    {
                        var poItemToRemove = db.ItemReturns.Where(x => x.CNId == 0).ToList();
                        if (poItemToRemove.Count > 0)
                        {
                            foreach (var item in poItemToRemove)
                            {
                                var toDlt = db.ItemReturns.FirstOrDefault(x => x.Id == item.Id);
                                db.ItemReturns.Remove(toDlt);
                            }

                            db.SaveChanges();
                        }
                    }

                    CreditNote cn = new CreditNote();
                    var typeid = db.Types.FirstOrDefault(e => e.Active.Equals(false) && e.Status.Equals(false) && e.Module == "Product" && e.TypeName == "Product").TypeId;
                    ViewData["Supplier"] = db.Suppliers.Where(x => x.Status == false && x.Active == false).ToList();
                    ViewBag.Products = this.GetProductList();

                    if (id > 0)
                    {
                        var cnDetails = db.StockReturns.Find(id);
                        cn.Id = cnDetails.Id;
                        cn.CNNo = cnno ?? cnDetails.CNNo;
                        cn.CNDate = cnDetails.CNDate;
                        cn.SupplierId = supid ?? cnDetails.SupplierId;
                        cn.Qty = cnDetails.Qty;
                        cn.TotalAmount = cnDetails.TotalAmount;
                        cn.Remarks = remarks ?? cnDetails.Remarks;

                        cn.ItemReturns =
                            (from i in db.ItemReturns
                             join p in db.Products on i.ProductId equals p.ProductId
                             where i.CNId == id || i.CNId == 0
                             select new UpdateItemReturn
                             {
                                 Id = i.Id,
                                 CNId = cn.Id,
                                 CNNo = cnno,
                                 ProductName = p.ProductName,
                                 Qty = i.Qty,
                                 UnitPrice = i.UnitPrice,
                                 LineDiscAmt = i.LineDiscAmt,
                                 LineTotal = i.LineTotal,
                                 Remarks = i.Remarks,
                             }).ToList();
                    }
                    else
                    {
                        cn.CNNo = cnno;
                        cn.CNDate = DateTime.Now;
                        cn.SupplierId = supid;
                        cn.Qty = 0;
                        cn.TotalAmount = 0;
                        cn.Remarks = remarks;

                        cn.ItemReturns =
                            (from i in db.ItemReturns
                             join p in db.Products on i.ProductId equals p.ProductId
                             where i.CNId == 0
                             select new UpdateItemReturn
                             {
                                 Id = i.Id,
                                 CNNo = cnno,
                                 ProductName = p.ProductName,
                                 Qty = i.Qty,
                                 UnitPrice = i.UnitPrice,
                                 LineDiscAmt = i.LineDiscAmt,
                                 LineTotal = i.LineTotal,
                                 Remarks = i.Remarks,
                             }).ToList();
                    }

                    cn.subtotal = cn.ItemReturns.Sum(item => item.LineTotal);
                    cn.totalDisc = cn.ItemReturns.Sum(item => item.LineDiscAmt);
                    cn.totalQty = cn.ItemReturns.Sum(item => item.Qty);

                    return View(cn);
                }

                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public ActionResult PreviewCN(int? id)
        {
            try
            {
                if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    CreditNote cn = new CreditNote();

                    var typeid = db.Types.FirstOrDefault(e => e.Active.Equals(false) && e.Status.Equals(false) && e.Module == "Product" && e.TypeName == "Product").TypeId;
                    ViewBag.Products = db.Products.Where(x => x.Status == false && x.Active == false && x.TypeId == typeid).ToList();
                    ViewData["Products"] = ViewBag.Products;

                    cn = (from sr in db.StockReturns
                          where sr.Id == id
                          select new CreditNote
                          {
                              Id = sr.Id,
                              CNNo = sr.CNNo,
                              CNDate = sr.CNDate,
                              SupplierId = sr.SupplierId,
                              Qty = sr.Qty,
                              TotalAmount = sr.TotalAmount,
                              Remarks = sr.Remarks,
                              Status = sr.Status,
                              AddBy = sr.AddBy,
                              AddDate = sr.AddDate,
                              ModBy = sr.ModBy,
                              ModDate = sr.ModDate,

                              ItemReturns = (from i in db.ItemReturns
                                             join p in db.Products on i.ProductId equals p.ProductId into pgroup
                                             from pr in pgroup.DefaultIfEmpty()
                                             where i.CNId == id
                                             select new UpdateItemReturn
                                             {
                                                 CNId = sr.Id,
                                                 CNNo = sr.CNNo,
                                                 ProductName = pr.ProductName,
                                                 Qty = i.Qty,
                                                 UnitPrice = i.UnitPrice,
                                                 LineDiscAmt = i.LineDiscAmt,
                                                 LineTotal = i.LineTotal,
                                                 Remarks = i.Remarks,
                                             }).ToList(),
                          }).FirstOrDefault();

                    cn.subtotal = cn.ItemReturns.Sum(item => item.LineTotal);
                    cn.totalDisc = cn.ItemReturns.Sum(item => item.LineDiscAmt);
                    cn.totalQty = cn.ItemReturns.Sum(item => item.Qty);

                    ViewData["SupplierName"] = cn.SupplierId > 0 ? db.Suppliers.FirstOrDefault(x => x.Id == cn.SupplierId).Name : "";

                    return View(cn);
                }

                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        [HttpPost]
        public ActionResult Create_StockReturn(int? supid, string cnno, string remarks, int? totalqty, decimal totaldisc, decimal subtotal)
        {
            try
            {
                // Create PO header
                if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    StockReturns cn = new StockReturns()
                    {
                        CNNo = cnno,
                        CNDate = DateTime.Now,
                        SupplierId = supid,
                        Qty = totalqty,
                        TotalDisc = totaldisc,
                        TotalAmount = subtotal,
                        Remarks = remarks,
                        Status = false,
                        AddBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString(),
                        ModBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString(),
                        AddDate = DateTime.Now,
                        ModDate = DateTime.Now,
                    };

                    if (!string.IsNullOrEmpty(cnno))
                    {
                        var isValid = db.StockReturns.FirstOrDefault(x => x.CNNo == cnno);
                        if (isValid != null)
                        {
                            //ModelState.AddModelError(string.Empty, "Duplicated CN No. found, please check!");
                            // Return JavaScript to close the window
                            string msg = "Duplicated CN No. found, please check!";
                            return Content(msg);
                        }
                    }

                    db.StockReturns.Add(cn);
                    db.SaveChanges();

                    // Update PO header
                    List<ItemReturn> items = db.ItemReturns.Where(x => x.CNId == 0).ToList();
                    if (items.Count > 0 && cn.Id > 0)
                    {
                        foreach (var i in items)
                        {
                            i.CNId = cn.Id;
                            i.CNNo = cnno;
                            i.AddBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString();
                            i.ModBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString();
                            i.AddDate = DateTime.Now;
                            i.ModDate = DateTime.Now;
                            db.ItemReturns.Attach(i);
                            db.Entry(i).State = EntityState.Modified;

                            // update stock availability
                            var avai = db.Stock.FirstOrDefault(x => x.ProductId == i.ProductId);
                            if (avai != null)
                            {
                                avai.QtyAvailable -= i.Qty;
                                avai.ModBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString();
                                avai.ModDate = DateTime.Now;
                                db.Stock.Attach(avai);
                                db.Entry(avai).State = EntityState.Modified;
                            }
                            else
                            {
                                var stock = new Stock()
                                {
                                    ProductId = i.ProductId,
                                    QtyAvailable = i.Qty,
                                    Status = false,
                                    AddBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString(),
                                    ModBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString(),
                                    AddDate = DateTime.Now,
                                    ModDate = DateTime.Now,
                                };

                                db.Stock.Add(stock);
                            }
                        }

                        db.SaveChanges();
                    }

                    ViewBag.CNNo = cnno;
                    return Content(cn.Id.ToString() ?? "-1", "text/html");//RedirectToAction("PurchaseOrder", "Stock", new { pono = po.PONo });
                }
                else
                {
                    return RedirectToAction("Login", "Account");
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as appropriate for your application
                // For simplicity, rethrow the exception for now
                throw;
            }
        }

        [HttpPost]
        public ActionResult Update_StockReturn(int cnid, int? supid, string cnno, string remarks, int? totalqty, decimal totalDisc, decimal subtotal)
        {
            try
            {
                // Create PO header
                if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    StockReturns cn = db.StockReturns.Find(cnid);
                    cn.CNNo = string.IsNullOrEmpty(cnno) ? null : cnno;
                    cn.CNDate = DateTime.Now;
                    cn.SupplierId = supid;
                    cn.Qty = totalqty;
                    cn.TotalAmount = subtotal;
                    cn.Remarks = remarks;
                    cn.Status = false;
                    cn.ModBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString();
                    cn.ModDate = DateTime.Now;

                    if (!string.IsNullOrEmpty(cnno))
                    {
                        var isValid = db.StockReturns.FirstOrDefault(x => x.CNNo == cnno && x.Id != cnid);
                        if (isValid != null)
                        {
                            //ModelState.AddModelError(string.Empty, "Duplicated PO No. found, please check!");
                            // Return JavaScript to close the window
                            string msg = "Duplicated PO No. found, please check!";
                            return Content(msg);
                        }
                    }

                    db.StockReturns.Attach(cn);
                    db.Entry(cn).State = EntityState.Modified;
                    db.SaveChanges();

                    // Update PO header
                    List<ItemReturn> items = db.ItemReturns.Where(x => x.CNId == 0).ToList();
                    if (items.Count > 0 && cnid > 0)
                    {
                        foreach (var i in items)
                        {
                            i.CNId = cnid;
                            i.CNNo = cnno;
                            i.AddBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString();
                            i.ModBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString();
                            i.AddDate = DateTime.Now;
                            i.ModDate = DateTime.Now;
                            db.ItemReturns.Attach(i);
                            db.Entry(i).State = EntityState.Modified;

                            // update stock availability
                            var avai = db.Stock.FirstOrDefault(x => x.ProductId == i.ProductId);
                            if (avai != null)
                            {
                                avai.QtyAvailable -= i.Qty;
                                avai.ModBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString();
                                avai.ModDate = DateTime.Now;
                                db.Stock.Attach(avai);
                                db.Entry(avai).State = EntityState.Modified;
                            }
                            else
                            {
                                var stock = new Stock()
                                {
                                    ProductId = i.ProductId,
                                    QtyAvailable = i.Qty,
                                    Status = false,
                                    AddBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString(),
                                    ModBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString(),
                                    AddDate = DateTime.Now,
                                    ModDate = DateTime.Now,
                                };

                                db.Stock.Add(stock);
                            }
                        }
                    }

                    db.SaveChanges();
                    ViewBag.CNNo = cnno;
                    return Content(cnid.ToString() ?? "-1", "text/html");//RedirectToAction("PurchaseOrder", "Stock", new { cnno = cn.CNNo });
                }
                else
                {
                    return RedirectToAction("Login", "Account");
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as appropriate for your application
                // For simplicity, rethrow the exception for now
                throw;
            }
        }

        [HttpPost]
        public ActionResult Destroy_StockReturn([DataSourceRequest] DataSourceRequest request, StockReturns stockReturn)
        {
            if (stockReturn != null && ModelState.IsValid)
            {
                bool isError = false;
                var header = db.StockReturns.Find(stockReturn.Id);
                var subitem = db.ItemReturns.Where(x => x.CNId == header.Id).ToList();

                foreach (var sub in subitem)
                {
                    var product = db.Stock.FirstOrDefault(p => p.ProductId == sub.ProductId);
                    if (product != null)
                    {
                        product.QtyAvailable += sub.Qty;
                        product.ModBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString();
                        product.ModDate = DateTime.Now;
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "\nFailed to delete. \nProduct not found! Please contact your administator for more information.");
                        isError = true;
                        break;
                    }

                    if (product.QtyAvailable < 0)
                    {
                        ModelState.AddModelError(string.Empty, "\nFailed to delete. \nProduct quantity not sufficient! ");
                        isError = true;
                        break;
                    }
                };

                header.Status = true;
                db.StockReturns.Attach(header);
                db.Entry(header).State = EntityState.Modified;
                db.SaveChanges();
            }

            return Json(new[] { stockReturn }.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region -------------- Item Return --------------
        [HttpPost]
        public ActionResult AddItemReturn(int? cnid, string cnno, int pid, int qty, decimal price, decimal disc, decimal total, int? supid, string remarks, string subremark)
        {
            try
            {
                if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    var typeid = db.Products.FirstOrDefault(x => x.ProductId == pid).TypeId;
                    var pro = db.ItemReturns.FirstOrDefault(x => x.ProductId == pid && x.CNId == cnid);

                    if (pro != null)
                    {
                        pro.Qty += qty;
                        pro.LineTotal = pro.Qty * price;
                        db.ItemReturns.Attach(pro);
                        db.Entry(pro).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    else
                    {
                        var item = new ItemReturn
                        {
                            ProductId = pid,
                            Qty = qty,
                            UnitPrice = price,
                            LineDiscAmt = disc,
                            LineTotal = total,
                            Remarks = subremark,
                            AddBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString(),
                            AddDate = DateTime.Now,
                        };

                        db.ItemReturns.Add(item);
                        db.SaveChanges();
                    }

                    return Json(new { supid, cnno, remarks });
                }

                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        [HttpPost]
        public ActionResult DeleteItemReturn(int id)
        {
            ItemReturn item = db.ItemReturns.Find(id);

            if (item.CNId > 0)
            {
                var count = db.Stock.FirstOrDefault(x => x.ProductId == item.ProductId);
                if (count != null)
                {
                    count.QtyAvailable += item.Qty;
                    count.ModBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString();
                    count.ModDate = DateTime.Now;
                    db.Stock.Attach(count);
                    db.Entry(count).State = EntityState.Modified;
                }
            }

            db.ItemReturns.Remove(item);
            db.SaveChanges();

            return new EmptyResult();
        }
        #endregion
    }
}