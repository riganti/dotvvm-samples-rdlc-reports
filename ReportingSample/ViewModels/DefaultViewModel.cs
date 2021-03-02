using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.Hosting;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using AspNetCore.Reporting;
using DotVVM.Framework.Storage;

namespace ReportingSample.ViewModels
{
    public class DefaultViewModel : MasterPageViewModel
    {
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly IReturnedFileStorage returnedFileStorage;

        public string ReportUrl { get; set; }

        public DefaultViewModel(IWebHostEnvironment webHostEnvironment, IReturnedFileStorage returnedFileStorage)
        {
            this.webHostEnvironment = webHostEnvironment;
            this.returnedFileStorage = returnedFileStorage;
        }

        public override async Task PreRender()
        {
            if (!Context.IsPostBack)
            {
                ReportUrl = await PrepareReportTempHtmlFile();
            }

            await base.PreRender();
        }

        private async Task<string> PrepareReportTempHtmlFile()
        {
            // get report and render HTML together with CSS
            var report = GetReport();
            var result = report.Execute(RenderType.Html, parameters: GetReportParams());

            var reportHtml = @$"<html>
    <head>
        <style type='text/css'>{Encoding.UTF8.GetString(result.SecondaryStream)}</style>
    </head>
    <body>
        {Encoding.UTF8.GetString(result.MainStream)}
    </body>
</html>";

            // IReturnedFileStorage is a temporary file store
            // we can store some file here to be able to get it using its ID in the iframe
            var fileId = await returnedFileStorage.StoreFile(new MemoryStream(Encoding.UTF8.GetBytes(reportHtml)), new ReturnedFileMetadata()
            {
                FileName = "report.html",
                MimeType = "text/html"
            });

            // build the URL for the iframe - it points to ReportTempFilesPresenter
            var fileVirtualUrl = Context.Configuration.RouteTable["ViewReport"].BuildUrl(new { id = fileId });
            return Context.TranslateVirtualPath(fileVirtualUrl);
        }

        public void SaveAsPdf()
        {
            var report = GetReport();
            var result = report.Execute(RenderType.Pdf, parameters: GetReportParams());

            Context.ReturnFile(result.MainStream, "report.pdf", "application/pdf");
        }

        private static Dictionary<string, string> GetReportParams()
        {
            var parameters = new Dictionary<string, string>();
            parameters.Add("param1", "Report sample");
            return parameters;
        }

        private LocalReport GetReport()
        {
            var path = Path.Combine(webHostEnvironment.ContentRootPath, "Reports/MyReport.rdlc");
            return new LocalReport(path);
        }
    }
}
