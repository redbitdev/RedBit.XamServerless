using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using RedBit.XamServerless.Core;

namespace RedBit.XamServerless.Functions
{
    public static class ImageProcessingStatusFunction
    {
        [FunctionName("ImageProcessingStatusFunction")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            // parse query parameter
            string id = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "id", true) == 0)
                .Value;

            // make sure we have a id
            if (id == null)
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, "id not in query string");

            // get the status
            var status = await TableManager.Default.GetStatus(id);

            // if we get no status then something is wrong
            if (status == null)
                return req.CreateResponse(HttpStatusCode.BadRequest, $"Not able to retreive record with id {id}");

            // return the status
            return req.CreateResponse(HttpStatusCode.OK, status);
        }
    }
}