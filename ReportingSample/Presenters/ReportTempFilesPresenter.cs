using DotVVM.Framework.Hosting;
using DotVVM.Framework.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReportingSample.Presenters
{
    public class ReportTempFilesPresenter : IDotvvmPresenter
    {
        private readonly IReturnedFileStorage returnedFileStorage;

        public ReportTempFilesPresenter(IReturnedFileStorage returnedFileStorage)
        {
            this.returnedFileStorage = returnedFileStorage;
        }

        public async Task ProcessRequest(IDotvvmRequestContext context)
        {
            var id = (Guid)context.Parameters["id"];
            var fileContents = returnedFileStorage.GetFile(id, out var metadata);

            context.HttpContext.Response.ContentType = metadata.MimeType;
            await fileContents.CopyToAsync(context.HttpContext.Response.Body);
        }
    }
}
