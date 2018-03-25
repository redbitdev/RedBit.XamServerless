using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace RedBit.XamServerless.Functions
{
    public static class ImageUploadFunc
    {
        [FunctionName("ImageUploadFunc")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            // Get request body
            dynamic data = await req.Content.ReadAsAsync<object>();

            if (data == null)
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, "No body found");

            if (data.imageb64 == null)
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, "No 'imageb64' property found in body");

            // get the base64 image
            var img = data?.imageb64;

            // convert to byte array
            var buffer = new byte[0];
            try
            {
                buffer = Convert.FromBase64String(img.ToString());
            }
            catch
            {
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, "unable to read 'imageb64' data");
            }

            return req.CreateResponse(HttpStatusCode.OK);
        }
    }
}
