
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
            var body = await req.Content.ReadAsStringAsync();

            if (body == null)
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, "No body found");

            // deserialize the payload
            var data = Newtonsoft.Json.JsonConvert.DeserializeObject<Core.UploadPayload>(body);

            if (data.Imageb64 == null)
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, "No 'imageb64' property found in body");

            // get the base64 image
            var img = data.Imageb64;

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

            // Upload to blob storage
            if (buffer.Length == 0)
            {
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, "unable to upload buffer with length of 0");
            }
            else
            {
                var url = await Core.BlobManager.Default.AddOriginalImage(buffer);
                return req.CreateResponse(HttpStatusCode.OK, new Core.UploadResponse { Url = url });
            }
        }
    }
}
