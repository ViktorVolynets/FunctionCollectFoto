using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ImageMagick;
using System.Buffers.Text;

namespace FunctionCollectFoto
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
           
            string base64 = data?.base64;
            string picture = data?.picture;

            string responseMessage = string.IsNullOrEmpty(base64)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"This HTTP triggered function executed successfully.";

            byte[] arr = Convert.FromBase64String(base64);
         
          using (MagickImage image = new MagickImage(picture))
            {
                using (MagickImage watermark = new MagickImage(arr))
                {
                    image.Composite(watermark, Gravity.Southeast, CompositeOperator.Over);
                    watermark.Evaluate(Channels.Alpha, EvaluateOperator.Divide, 60);
                    image.Composite(watermark, 1, 1, CompositeOperator.Over);
                    responseMessage = image.ToBase64();
                }
            }           
            return new OkObjectResult(responseMessage);
        }
    }
}
