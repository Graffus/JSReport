﻿using Microsoft.AspNetCore.Mvc;
using WebApp.Model;
using jsreport.AspNetCore;
using jsreport.Types;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;

namespace WebApp.Controllers
{
    public class HomeController : Controller
    {
        public IJsReportMVCService JsReportMVCService { get; }

        public HomeController(IJsReportMVCService jsReportMVCService)
        {
            JsReportMVCService = jsReportMVCService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [MiddlewareFilter(typeof(JsReportPipeline))]
        public IActionResult Invoice()
        {
            HttpContext.JsReportFeature().Recipe(Recipe.Docx);
            //HttpContext.JsReportFeature().OnAfterRender((r) => {
            //    using (var file = System.IO.File.Open("report.pdf", FileMode.Create))
            //    {
            //        r.Content.CopyTo(file);
            //    }
            //    r.Content.Seek(0, SeekOrigin.Begin);
            //});

            return View(InvoiceModel.Example());
        }

        [MiddlewareFilter(typeof(JsReportPipeline))]
        public IActionResult InvoiceDownload()
        {
            HttpContext.JsReportFeature().Recipe(Recipe.ChromePdf)
                .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"myReport.pdf\"");

            return View("Invoice", InvoiceModel.Example());
        }

        [MiddlewareFilter(typeof(JsReportPipeline))]
        public async Task<IActionResult> InvoiceWithHeader()
        {
            var header = await JsReportMVCService.RenderViewToStringAsync(HttpContext, RouteData, "Header", new { });

            HttpContext.JsReportFeature()
                .Recipe(Recipe.ChromePdf)
                .Configure((r) => r.Template.Chrome = new Chrome {
                    HeaderTemplate = header,
                    DisplayHeaderFooter = true,
                    MarginTop = "1cm",
                    MarginLeft = "1cm",
                    MarginBottom = "1cm",
                    MarginRight = "1cm"
                });

            return View("Invoice", InvoiceModel.Example());
        }

        [MiddlewareFilter(typeof(JsReportPipeline))]
        public IActionResult Items()
        {
            HttpContext.JsReportFeature()
                .Recipe(Recipe.HtmlToXlsx)
                .Configure((r) => r.Template.HtmlToXlsx = new HtmlToXlsx() { HtmlEngine = "chrome" });

            return View(InvoiceModel.Example());
        }

        [MiddlewareFilter(typeof(JsReportPipeline))]
        public IActionResult ItemsExcelOnline()
        {
            HttpContext.JsReportFeature()
                .Configure(req => req.Options.Preview = true)
                .Recipe(Recipe.HtmlToXlsx)
                .Configure((r) => r.Template.HtmlToXlsx = new HtmlToXlsx() { HtmlEngine = "chrome" });

            return View("Items", InvoiceModel.Example());
        }

        [MiddlewareFilter(typeof(JsReportPipeline))]
        public IActionResult InvoiceDebugLogs()
        {
            HttpContext.JsReportFeature()
                .DebugLogsToResponse()
                .Recipe(Recipe.ChromePdf);

            return View("Invoice", InvoiceModel.Example());
        }


        [MiddlewareFilter(typeof(JsReportPipeline))]
        public async Task<IActionResult> InvoiceWithCover()
        {
            var coverHtml = await JsReportMVCService.RenderViewToStringAsync(HttpContext, RouteData, "Cover", new { });
            HttpContext.JsReportFeature()              
                .Recipe(Recipe.ChromePdf)
                .Configure((r) =>
                {
                    r.Template.PdfOperations = new[]
                    {
                        new PdfOperation()
                        {
                            Template = new Template
                            {
                                Content = coverHtml,
                                Engine = Engine.None,
                                Recipe = Recipe.ChromePdf
                            },
                            Type = PdfOperationType.Append
                        }
                    };
                });

            return View("Invoice", InvoiceModel.Example());
        }

        [MiddlewareFilter(typeof(JsReportPipeline))]
        public async Task<IActionResult> ChartWithPrintTrigger()
        {
            HttpContext.JsReportFeature()
                .Recipe(Recipe.ChromePdf)
                .Configure(cfg =>
                {
                    cfg.Template.Chrome = new Chrome
                    {
                        WaitForJS = true
                    };
                });

            return View("Chart", new { });
        }
    }
}
