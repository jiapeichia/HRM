using OfficeOpenXml;
using Meo.Web.DBContext;
using Meo.Web.ViewModels;
using System;
using System.Web.Mvc;
using Kendo.Mvc.Extensions;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using OfficeOpenXml.Style;

namespace Web.HRM.Controllers
{
    public class ReceiptController : Controller
    {
        private const double size = 13.5;
        DBContext db = new DBContext();
        int count = 11;
        public ActionResult Index2()
        {
            return View();
        }

        // Method 2 : Download file 
        [HttpPost]
        public ActionResult Index2(string salesId) // FileResult
        {
            try
            {
                if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    if (string.IsNullOrEmpty(salesId))
                    {
                        return View();
                    }

                    // Read the Excel template
                    var templatePath = Server.MapPath("~/Report/Receipt.xlsx");
                    using (var package = new ExcelPackage(new FileInfo(templatePath)))
                    {
                        var worksheet = package.Workbook.Worksheets[0];
                        var sales = this.GetInvoiceDetails(salesId);
                        var companyprofile = db.CompanyProfile.FirstOrDefault();

                        // Update body
                        UpdateHeader(worksheet, companyprofile);

                        // Update body
                        UpdateBodyTable(worksheet, sales, companyprofile);

                        // Update total section
                        UpdateTotalSection(worksheet, sales, companyprofile);

                        // Update footer
                        UpdateFooter(worksheet);

                        // Save the Excel file to a specific directory
                        string directoryPath = "D:\\BeYou_DBBackUp\\"; // Replace with your desired directory path
                        string fileName = "Receipt.xlsx";
                        //  string fileName = DateTime.Now.Year.ToString()
                        //+ DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString()
                        //+ "_" + "Receipt.xlsx";

                        string filePath = Path.Combine(directoryPath, fileName);

                        // Save the Excel file
                        byte[] excelData = package.GetAsByteArray();
                        System.IO.File.WriteAllBytes(filePath, excelData);
                        File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);

                        // Print file
                        Print(fileName);

                        // Introduce a 10-second delay using Task.Delay
                        //await Task.Delay(200); // 10 seconds delay

                        // Delete file
                        //DeleteExcelFile(directoryPath, fileName);

                        //return RedirectToAction("Index", "Home");
                        return new EmptyResult();
                    }
                }

                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                // Handle exceptions appropriately
                // Log the exception, return an error view, etc.
                //return View("Error", new { message = ex.Message });

                // Log the exception, return an error view with HandleErrorInfo
                return View("Error", new HandleErrorInfo(ex, "Receipt", "PrintReceipt"));
            }
        }

        private void UpdateHeader(ExcelWorksheet worksheet, CompanyProfile profile)
        {
            // Header Company Profile
            // Merge cells A1 to F1
            int startRow = 1; // Starting row index
            int endRow = 6; // Ending row index (adjust as needed)

            List<string> stringList = new List<string>();
            stringList.Add(profile.Name);
            stringList.Add(profile.RegNo);
            stringList.Add(profile.Address1);
            stringList.Add(profile.Address2);
            stringList.Add(profile.Address3);
            stringList.Add("TEL: " + profile.Tel);

            for (int row = startRow; row <= endRow; row++)
            {
                var mergedCells = worksheet.Cells["A" + row + ":E" + row];
                mergedCells.Merge = true;

                // Set text alignment to center
                mergedCells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                mergedCells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                mergedCells.Style.WrapText = true;

                // Set content for the merged cell
                mergedCells.Value = stringList[row - 1];

                // Hide the row if all cells in the row are empty
                if (string.IsNullOrEmpty(stringList[row - 1]))
                {
                    mergedCells.EntireRow.Hidden = true;
                }
            }
        }

        private void UpdateBodyTable(ExcelWorksheet worksheet, Receipt sales, CompanyProfile profile)
        {
            // Header 
            worksheet.Cells["A8"].Value = "Invoice No. : " + sales.SalesId;
            worksheet.Cells["A9"].Value = "Date : " + sales.Date.ToShortDateString();

            // Body
            worksheet.Cells["A" + count].Value = "Item";
            worksheet.Cells["B" + count].Value = "Price (" + profile.Currency + ")";
            worksheet.Cells["E" + count].Value = "Amount (" + profile.Currency + ")";
            worksheet.Cells["A" + count + ":" + "E" + count].Style.Font.Size = (float)size;

            sales.ReceiptItems.ForEach(item =>
            {
                count++;
                worksheet.Cells["A" + count].Style.WrapText = true;
                worksheet.Cells["A" + count].Value = item.ProductName + " testing for longer text display";
                worksheet.Cells["B" + count].Value = item.UnitPrice;
                worksheet.Cells["B" + count].Style.Numberformat.Format = "#,##0.00";
                worksheet.Cells["C" + count].Value = "*";
                worksheet.Cells["D" + count].Value = item.Quantity;
                worksheet.Cells["E" + count].Value = item.LineTotal;
                worksheet.Cells["E" + count].Style.Numberformat.Format = "#,##0.00";
                worksheet.Cells["A" + count + ":" + "E" + count].Style.Font.Size = (float)size;
            });
        }

        private void UpdateTotalSection(ExcelWorksheet worksheet, Receipt sales, CompanyProfile profile)
        {
            // Total
            var totalSectionStartCell = worksheet.Cells["A" + (count += 2)]; // Replace with the starting cell for your total section
            var totalSectionEndCell = worksheet.Cells["E" + count];
            var totalSectionRange = worksheet.Cells[totalSectionStartCell.Address + ":" + totalSectionEndCell.Address];
            totalSectionRange.Style.Border.Top.Style = ExcelBorderStyle.Dashed;
            worksheet.Cells["A" + count].Value = "TOTAL (" + profile.Currency + ")";
            worksheet.Cells["E" + count].Value = sales.TotalAmt;
            worksheet.Cells["E" + count].Style.Numberformat.Format = "#,##0.00";

            worksheet.Cells["A" + (count += 1)].Value = "PAID AMOUNT (" + profile.Currency + ")";
            worksheet.Cells["E" + count].Value = sales.PaidAmt;
            worksheet.Cells["E" + count].Style.Numberformat.Format = "#,##0.00";

            worksheet.Cells["A" + (count += 1)].Value = "BALANCE (" + profile.Currency + ")";
            worksheet.Cells["E" + count].Value = sales.BalAmt;
            worksheet.Cells["E" + count].Style.Numberformat.Format = "#,##0.00";

            var totalStartCell = worksheet.Cells["A" + count]; // Replace with the starting cell for your total section
            var totalEndCell = worksheet.Cells["E" + count];
            var totalRange = worksheet.Cells[totalStartCell.Address + ":" + totalEndCell.Address];
            totalRange.Style.Border.Bottom.Style = ExcelBorderStyle.Dashed;
        }

        private void UpdateFooter(ExcelWorksheet worksheet)
        {
            // Merge cells A1 to F1
            count += 2;
            var mergedCells = worksheet.Cells["A" + count + ":E" + count];
            mergedCells.Merge = true;

            // Set text alignment to center
            mergedCells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            mergedCells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            mergedCells.Style.WrapText = true;

            // Set content for the merged cell
            mergedCells.Value = "Thank you for choosing BeYou!";
        }

        [HttpGet]
        public Receipt GetInvoiceDetails(string salesid)
        {
            var salesVM = db.Saless.FirstOrDefault(x => x.SalesId == salesid);
            var salesitem = db.SalesItems.Where(x => x.SalesId == salesid).ToList();
            decimal lineTotal = 0;
            int qtyTotal = 0;

            var sales =
                (from sa in db.Saless
                 where sa.SalesId == salesid
                 select new Receipt
                 {
                     SalesId = sa.SalesId,
                     CusId = sa.CusId,
                     PaymentMethodId = sa.PaymentMethod,
                     Date = sa.PaymentDate,
                     SubTotal = sa.SubTotal,
                     TotalAmt = sa.TotalAmt,
                     PaidAmt = sa.PaidAmt,
                     DiscAmt = sa.DiscAmt,
                     DiscPercentageAmt = sa.DiscPercentageAmt,
                     BalAmt = sa.BalAmt,
                     Exchange = sa.Exchange,
                 }).FirstOrDefault();

            //var sales = (from sa in db.Saless
            //             join si in db.SalesItems on sa.SalesId equals si.SalesId into invgroup
            //             from item in invgroup.DefaultIfEmpty()
            //             join pro in db.Products on item.ProductId equals pro.ProductId
            //             where sa.SalesId == salesid
            //             select new Receipt
            //             {
            //                 SalesId = sa.SalesId,
            //                 Date = sa.PaymentDate,
            //                 TotalAmt = sa.TotalAmt,
            //                 PaidAmt = sa.PaidAmt,
            //                 DiscAmt = sa.DiscAmt,
            //                 BalAmt = sa.BalAmt,
            //             }).FirstOrDefault();


            // Get Exchange item
            var exchange =
               (from si in db.Exchanges
                join pro in db.Products on si.ProductId equals pro.ProductId
                where si.SalesId.Equals(salesid)
                select new ReceiptItem
                {
                    ProductCode = pro.ProductCode,
                    ProductName = pro.ProductName,
                    Quantity = si.Quantity ?? 0,
                    UnitPrice = -si.UnitPrice ?? 0,
                    LineTotal = -si.LineTotal ?? 0,
                }).ToList();

            var recpt1 =
                (from si in db.SalesItems
                 join pro in db.Products on si.ProductId equals pro.ProductId
                 where si.SalesId == salesid
                 select new ReceiptItem
                 {
                     ProductCode = pro.ProductCode,
                     ProductName = pro.ProductName,
                     UnitPrice = si.UnitPrice,
                     Quantity = si.Quantity,
                     LineTotal = si.LineTotal,
                     LineDiscount = si.LineDiscAmt,
                 }).ToList();

            var recpt2 =
                (from si in db.SalesItems
                 join pack in db.Packages on si.ProductId equals pack.ProductId
                 where si.SalesId == salesid
                 select new ReceiptItem
                 {
                     ProductCode = pack.Code,
                     ProductName = pack.Description,
                     UnitPrice = si.UnitPrice,
                     Quantity = si.Quantity,
                     LineTotal = si.LineTotal,
                     LineDiscount = si.LineDiscAmt,
                 }).ToList();

            sales.ReceiptItems = exchange.Concat(recpt1).Concat(recpt2).ToList();
            return sales;
        }

        // Print function
        public void Print(string fileName)
        {
            Microsoft.Office.Interop.Excel.Application excelApp = new Microsoft.Office.Interop.Excel.Application();
            Microsoft.Office.Interop.Excel.Workbook workbook = excelApp.Workbooks.Open("D:\\BeYou_DBBackUp\\" + fileName);

            // Print the workbook
            workbook.PrintOut();

            // Close Excel application
            excelApp.Quit();
        }

        //private void DeleteExcelFile(string directoryPath, string fileName)
        //{
        //    string filePath = Path.Combine(directoryPath, fileName);

        //    // Check if the file exists before attempting to delete
        //    if (System.IO.File.Exists(filePath))
        //    {
        //        // Delete the file
        //        System.IO.File.Delete(filePath);
        //    }

        //    // You can redirect to another action or return a response as needed
        //    // For example, redirect to a confirmation page
        //    //return RedirectToAction("Index", "Home");
        //    //return RedirectToAction("FileDeletedConfirmation");
        //}

        //static void OpenPdfInBrowser(string pdfFilePath)
        //{
        //    // Use the default web browser to open the PDF file
        //    Process.Start(new ProcessStartInfo(pdfFilePath) { UseShellExecute = true });

        //    //Process.Start(new ProcessStartInfo
        //    //{
        //    //    FileName = pdfFilePath,
        //    //    UseShellExecute = true,
        //    //    Verb = "print" // Opens the print dialog directly
        //    //});
        //}

        //public void PrintReceipt() // FileResult
        //{
        //    string excelFilePath = @"D:\BeYou_DBBackUp\10_11_2023_Receipt.xlsx";
        //    string pdfFilePath = @"D:\BeYou_DBBackUp\Receipt_output.pdf";

        //    // Create an Excel application instance
        //    Application excelApp = new Application();
        //    Workbook workbook = excelApp.Workbooks.Open(excelFilePath);

        //    try
        //    {
        //        // Save as PDF
        //        workbook.ExportAsFixedFormat(XlFixedFormatType.xlTypePDF, pdfFilePath);
        //        Console.WriteLine("PDF saved successfully.");

        //        // Open the PDF file in the default web browser
        //        OpenPdfInBrowser(pdfFilePath);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Error: {ex.Message}");
        //    }
        //    finally
        //    {
        //        // Close and release resources
        //        workbook.Close();
        //        excelApp.Quit();
        //        System.Runtime.InteropServices.Marshal.ReleaseComObject(excelApp);
        //    }
        //}

        //static void PrintPdf(string pdfFilePath)
        //{
        //    // Use the default PDF viewer to print the file
        //    //ProcessStartInfo info = new ProcessStartInfo()
        //    //{
        //    //    Verb = "print",
        //    //    FileName = pdfFilePath,
        //    //    CreateNoWindow = true,
        //    //    WindowStyle = ProcessWindowStyle.Hidden
        //    //};

        //    Process.Start(info)?.WaitForExit();
        //}

        //Method 1 : print directly
        //    [HttpPost]
        //public void PrintReceipt() // FileResult
        //{
        //    // Read the Excel template
        //    var templatePath = Server.MapPath("~/Report/Receipt.xlsx");
        //    using (var package = new ExcelPackage(new FileInfo(templatePath)))
        //    {
        //        var worksheet = package.Workbook.Worksheets[0];
        //        var sales = this.GetInvoiceDetails("INV2300043");

        //        // Update header and body
        //        UpdateHeaderAndBody(worksheet, sales);

        //        // Update total section
        //        UpdateTotalSection(worksheet, sales);

        //        // Update footer
        //        UpdateFooter(worksheet);

        //        // Set the paper size width to 80mm and height to 0 (auto)
        //        //PaperSize paperSize = new PaperSize("CustomPaperSize", 315, 0); // 80mm x auto

        //        //Set paper size and orientation
        //        var printDocument = new PrintDocument();

        //        //Method 1
        //        // Create a temporary file path
        //        var tempFilePath = Path.GetTempFileName();

        //        // Save the Excel content to the temporary file
        //        package.SaveAs(new FileInfo(tempFilePath));

        //        // Print the Excel document using the temporary file path
        //        printDocument.PrinterSettings.PrintFileName = tempFilePath;
        //        printDocument.Print();

        //        //// Method 2
        //        //// Handle the PrintPage event to specify the content to print
        //        //printDocument.PrintPage += (sender, e) =>
        //        //{
        //        //    using (var stream = new MemoryStream(package.GetAsByteArray()))
        //        //    {
        //        //        using (var image = System.Drawing.Image.FromStream(stream))
        //        //        {
        //        //            e.Graphics.DrawImage(image, e.MarginBounds);
        //        //        }
        //        //    }
        //        //}; 

        //        // Convert Excel to PDF using EPPlus
        //        //var pdfBytes = package.GetAsByteArray();
        //        //var pdfStream = new MemoryStream(pdfBytes);

        //        //// Print the PDF document
        //        //using (var document = PdfReader.Open(pdfStream, PdfDocumentOpenMode.Import))
        //        //{
        //        //    using (var printDocument = new PrintDocument())
        //        //    {
        //        //        printDocument.PrinterSettings.PrintFileName = templatePath; // Set the path to the original Excel file
        //        //        printDocument.PrinterSettings.PrinterName = "XP-80C"; // Get the default printer name
        //        //        printDocument.PrinterSettings.PrintRange = PrintRange.AllPages;

        //        //        printDocument.Print();
        //        //    }
        //        //}

        //        // Print the Excel document to the default printer
        //        //printDocument.Print();
        //        //return PartialView();
        //        //return RedirectToAction("ExistingPage");
        //    }
        //}

        public ActionResult Index(string salesId)
        {
            var companyprofile = db.CompanyProfile.FirstOrDefault();
            var sales = GetInvoiceDetails(salesId);
            var cusInfo = db.Customers.Find(sales.CusId);
            var paymentType = db.Types.Find(sales.PaymentMethodId).TypeName;

            ReceiptModel receipt = new ReceiptModel();
            var lineDisc = 0;

            if (paymentType.Equals("Credit Balance"))
            {
                sales.ReceiptItems.ForEach(item =>
                {
                    item.LineTotal = Math.Round(item.LineTotal, 0);
                    item.LineDiscount = Math.Round(item.LineDiscount, 0);
                    item.UnitPrice = Math.Round(item.UnitPrice, 0);
                });

                receipt = new ReceiptModel
                {
                    ReceiptHeader = "CREDIT SALES",
                    StoreName = companyprofile.Name,
                    RegNo = companyprofile.RegNo,
                    Address1 = companyprofile.Address1,
                    Address2 = companyprofile.Address2,
                    Address3 = companyprofile.Address3,
                    Tel = companyprofile.Tel,

                    SalesId = sales.SalesId,
                    CardNo = cusInfo.CardNo,
                    CreditBal = Math.Round(cusInfo.CreditBal, 0),
                    PaymentMethod = "Credit",
                    Date = sales.Date.ToString("dd/MM/yyyy"), //DateTime.Now.ToString("dd/MM/yyyy"),
                    ReceiptItems = sales.ReceiptItems,
                    TotalDisc = Math.Round(sales.DiscAmt ?? 0m + sales.DiscPercentageAmt ?? 0m, 2),
                    SubTotal = Math.Round(sales.SubTotal ?? 0m, 2),
                    CRTotalAmount = (int)Math.Round(sales.TotalAmt ?? 0m, 0),
                    //TotalPaid = Math.Round((decimal)sales.PaidAmt, 2),
                    TotalBalance = Math.Round(sales.BalAmt ?? 0m),
                };
            }
            else
            {
                receipt = new ReceiptModel
                {
                    ReceiptHeader = sales.Exchange == true ? "EXCHANGE" : "SALES",
                    StoreName = companyprofile.Name,
                    RegNo = companyprofile.RegNo,
                    Address1 = companyprofile.Address1,
                    Address2 = companyprofile.Address2,
                    Address3 = companyprofile.Address3,
                    Tel = companyprofile.Tel,

                    SalesId = sales.SalesId,
                    CardNo = cusInfo.CardNo,
                    PaymentMethod = paymentType,
                    Date = sales.Date.ToString("dd/MM/yyyy"),
                    ReceiptItems = sales.ReceiptItems,
                    TotalDisc = Math.Round((sales.DiscAmt ?? 0m) + (sales.DiscPercentageAmt ?? 0m), 2),
                    TotalPaid = Math.Round(sales.PaidAmt ?? 0m, 2),
                    TotalBalance = Math.Round(sales.BalAmt ?? 0m, 2),
                    SubTotal = Math.Round(sales.SubTotal ?? 0m, 2),
                    TotalAmount = Math.Round(sales.TotalAmt ?? 0m, 2),
                };
            }

            if (receipt.TotalBalance < 0)
            {
                receipt.TotalBalanceString = "(" + Math.Abs(Math.Round(receipt.TotalBalance, 2)) + ")";
            }
            //return new EmptyResult();
            return PartialView(receipt);
        }

        public ActionResult GIROReceipt(string salesId)
        {
            var companyprofile = db.CompanyProfile.FirstOrDefault();
            var sales = this.GetGIROInvoiceDetails(salesId, false);
            var cusInfo = db.Customers.Find(sales.CusId);
            var paymentType = db.Types.Find(sales.PaymentMethodId).TypeName;

            ReceiptModel receipt = new ReceiptModel();
            receipt = new ReceiptModel
            {
                ReceiptHeader = "SALES",
                StoreName = companyprofile.Name,
                RegNo = companyprofile.RegNo,
                Address1 = companyprofile.Address1,
                Address2 = companyprofile.Address2,
                Address3 = companyprofile.Address3,
                Tel = companyprofile.Tel,
                IsGIRO = sales.IsGIRO,
                SalesId = sales.SalesId,
                CardNo = cusInfo.CardNo,
                PaymentMethod = paymentType,
                Date = sales.Date.ToString("dd/MM/yyyy"),
                GIROPaymentCounts = sales.GIROPaymentCounts,
                ReceiptItems = sales.ReceiptItems,
                TotalAmount = Math.Round(sales.TotalAmt ?? 0m, 2),
                TotalPaid = Math.Round(sales.PaidAmt ?? 0m, 2),
                TotalBalance = Math.Round(sales.BalAmt ?? 0m, 2),
            };

            if (receipt.TotalBalance < 0 && receipt.IsGIRO)
            {
                receipt.TotalBalanceString = "(" + Math.Abs(receipt.TotalBalance) + ")";
            }

            return PartialView(receipt);
        }

        [HttpGet]
        public Receipt GetGIROInvoiceDetails(string salesid, bool isService)
        {
            var salesVM = db.Saless.FirstOrDefault(x => x.SalesId == salesid);
            var salesitem = db.SalesItems.Where(x => x.SalesId == salesid).ToList();
            decimal lineTotal = 0;
            int qtyTotal = 0;

            var searchId = salesid;
            if (!string.IsNullOrEmpty(salesVM.DueInvoice))
            {
                searchId = salesVM.DueInvoice;
            }

            var sales =
                (from sa in db.Saless
                 where sa.SalesId == searchId
                 select new Receipt
                 {
                     SalesId = salesid,
                     CusId = sa.CusId,
                     PaymentMethodId = salesVM.PaymentMethod,
                     Date = salesVM.PaymentDate,
                     TotalAmt = sa.TotalAmt,
                     PaidAmt = salesVM.PaidAmt,
                     DiscAmt = salesVM.DiscAmt,
                     BalAmt = salesVM.BalAmt,
                     Exchange = salesVM.Exchange,
                     IsGIRO = salesVM.GIRO,
                 }).FirstOrDefault();

            sales.GIROPaymentCounts = new List<ReceiptAmountItem>();

            if (!string.IsNullOrEmpty(salesVM.DueInvoice))
            {
                // Previous payment has been made
                int count = 1;
                var giroSales = db.Saless.Where(x => (x.DueInvoice == salesVM.DueInvoice || x.SalesId == salesVM.DueInvoice) && x.PaymentDate < salesVM.PaymentDate && x.SalesId != salesVM.SalesId && x.Status == false && x.Active == false)
                    .OrderBy(x => x.PaymentDate).ToList();

                giroSales.ForEach(item =>
                {
                    var payment = new ReceiptAmountItem
                    {
                        PaidCount = count++,
                        TotalPaid = item.PaidAmt,
                    };

                    sales.GIROPaymentCounts.Add(payment);
                });

                sales.ReceiptItems =
                (from si in db.SalesItems
                 join pack in db.Packages on si.ProductId equals pack.ProductId
                 where si.SalesId == searchId
                 select new ReceiptItem
                 {
                     ProductCode = pack.Code,
                     ProductName = pack.Description,
                     UnitPrice = si.UnitPrice,
                     Quantity = si.Quantity,
                     LineTotal = si.LineTotal
                 }).ToList();
            }
            else
            {
                // Get Exchange item
                var exchange =
                   (from si in db.Exchanges
                    join pro in db.Products on si.ProductId equals pro.ProductId
                    where si.SalesId.Equals(searchId)
                    select new ReceiptItem
                    {
                        ProductCode = pro.ProductCode,
                        ProductName = pro.ProductName,
                        Quantity = si.Quantity ?? 0,
                        UnitPrice = -si.UnitPrice ?? 0,
                        LineTotal = -si.LineTotal ?? 0,
                    }).ToList();

                var recpt1 =
                    (from si in db.SalesItems
                     join pro in db.Products on si.ProductId equals pro.ProductId
                     where si.SalesId == searchId
                     select new ReceiptItem
                     {
                         ProductCode = pro.ProductCode,
                         ProductName = pro.ProductName,
                         UnitPrice = si.UnitPrice,
                         Quantity = si.Quantity,
                         LineTotal = si.LineTotal
                     }).ToList();

                var recpt2 =
                    (from si in db.SalesItems
                     join pack in db.Packages on si.ProductId equals pack.ProductId
                     where si.SalesId == searchId
                     select new ReceiptItem
                     {
                         ProductCode = pack.Code,
                         ProductName = pack.Description,
                         UnitPrice = si.UnitPrice,
                         Quantity = si.Quantity,
                         LineTotal = si.LineTotal
                     }).ToList();

                sales.ReceiptItems = exchange.Concat(recpt1).Concat(recpt2).ToList();
            }

            // for service to display multiple payment
            if (isService)
            {
                var salesSub = db.Saless.Where(x => x.DueInvoice == searchId).ToList();
                if (salesSub.Count > 0)
                {
                    int count = 1;
                    salesSub.ForEach(item =>
                    {
                        var payment = new ReceiptAmountItem
                        {
                            PaidCount = count++,
                            TotalPaid = item.PaidAmt,
                        };

                        sales.GIROPaymentCounts.Add(payment);
                    });

                    sales.ReceiptItems =
                    (from si in db.SalesItems
                     join pack in db.Packages on si.ProductId equals pack.ProductId
                     where si.SalesId == searchId
                     select new ReceiptItem
                     {
                         ProductCode = pack.Code,
                         ProductName = pack.Description,
                         UnitPrice = si.UnitPrice,
                         Quantity = si.Quantity,
                         LineTotal = si.LineTotal
                     }).ToList();
                }
            }

            return sales;
        }

        public ActionResult PrintExcel(string fileName)
        {
            // Validate and sanitize the fileName to prevent security issues

            // Construct the full path to the Excel file
            string filePath = Path.Combine("D:\\BeYou_DBBackUp\\", fileName);

            // Perform any additional logic (e.g., Excel file validation)

            // Return the file for download
            return File(filePath, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        public ActionResult ServiceReceipt(int id)
        {
            var companyprofile = db.CompanyProfile.FirstOrDefault();
            var serviceHistory = db.ServiceHistories.Find(id);
            var service = db.Services.FirstOrDefault(x => x.SalesId == serviceHistory.SalesId);

            var sales = this.GetGIROInvoiceDetails(serviceHistory.SalesId, true);
            var cusInfo = db.Customers.Find(sales.CusId);
            var paymentType = db.Types.Find(sales.PaymentMethodId).TypeName;

            ServiceReceipt receipt = new ServiceReceipt
            {
                ReceiptHeader = "SERVICE RECEIPT",
                StoreName = companyprofile.Name,
                RegNo = companyprofile.RegNo,
                Address1 = companyprofile.Address1,
                Address2 = companyprofile.Address2,
                Address3 = companyprofile.Address3,
                Tel = companyprofile.Tel,

                SalesId = sales.SalesId,
                CardNo = cusInfo.CardNo,
                PaymentMethod = paymentType,
                Date = sales.Date.ToString("dd/MM/yyyy"),
                ServiceHistory = serviceHistory,
                TotalAmount = Math.Round((decimal)sales.TotalAmt, 2),
                TotalPaid = Math.Round(sales.PaidAmt ?? 0m, 2),
                //Math.Round((decimal)sales.PaidAmt, 2),
                TotalBalance = Math.Round(sales.BalAmt ?? 0m, 2),
                //Math.Round((decimal)sales.BalAmt, 2),
                courseBal = service?.CourseBal ?? 0,
                GIROPaymentCounts = sales.GIROPaymentCounts,
            };

            if (receipt.TotalBalance < 0)
            {
                decimal val = (decimal)receipt.GIROPaymentCounts.Sum(x => x.TotalPaid);
                decimal final = receipt.TotalBalance + val;
                receipt.TotalBalanceString = "(" + Math.Abs(final) + ")";
            }

            return PartialView(receipt);
        }
    }
}