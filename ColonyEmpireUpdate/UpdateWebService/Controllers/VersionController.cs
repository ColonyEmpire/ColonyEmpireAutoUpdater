using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;

namespace UpdateWebService.Controllers
{
    public class VersionController : ApiController
    {
        // GET: api/Version
        public string Get()
        {
            return File.ReadAllText(HttpContext.Current.Server.MapPath("~/Update/version.txt"));
        }

        // GET: api/Version/Download
        [HttpGet]
        [Route("api/Version/Download")]
        public HttpResponseMessage Download()
        {
            var stream = new MemoryStream();
            File.OpenRead(HttpContext.Current.Server.MapPath("~/Update/ColonyEmpire.zip")).CopyTo(stream);

            var result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(stream.ToArray())
            };
            result.Content.Headers.ContentDisposition =
                new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
                {
                    FileName = "ColonyEmpire.zip"
                };
            result.Content.Headers.ContentType =
                new MediaTypeHeaderValue("application/octet-stream");

            return result;
        }
    }
}
