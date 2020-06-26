using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Script.Serialization;

namespace Task8.Controllers
{
    public class ComputerVisionController : ApiController
    {
        //With Computer Vision - Read API
        [HttpPost]
        [Route("api/computervision")]
        public async Task<IHttpActionResult> ImageToText()
        {
            // Add your Computer Vision subscription key and endpoint to your environment variables.
            var subscriptionKey = "subscriptionkey";

            // An endpoint should have a format like "https://westus.api.cognitive.microsoft.com"
            var endpoint = "https://csctask8.cognitiveservices.azure.com/";

            // the Batch Read method endpoint
            var uriBase = endpoint + "/vision/v3.0//read/analyze";
            //Retrieve file
            var file = HttpContext.Current.Request.Files[0];
            //Convert hhtpfile into string
            //Convert hhtpfile into byte array
            byte[] fileData = null;
            using (var binaryReader = new BinaryReader(file.InputStream))
            {
                fileData = binaryReader.ReadBytes(file.ContentLength);
            }

            try
            {
                HttpClient client = new HttpClient();

                // Request headers.
                client.DefaultRequestHeaders.Add(
                    "Ocp-Apim-Subscription-Key", subscriptionKey);

                var url = uriBase;

                HttpResponseMessage response;

                // Two REST API methods are required to extract text.
                // One method to submit the image for processing, the other method
                // to retrieve the text found in the image.

                // operationLocation stores the URI of the second REST API method,
                // returned by the first REST API method.
                string operationLocation;
              
                // Adds the byte array as an octet stream to the request body.
                using (ByteArrayContent content = new ByteArrayContent(fileData))
                {
                    // This example uses the "application/octet-stream" content type.
                    // The other content types you can use are "application/json"
                    // and "multipart/form-data".
                    content.Headers.ContentType =
                        new MediaTypeHeaderValue("application/octet-stream");

                    // The first REST API method, Batch Read, starts
                    // the async process to analyze the written text in the image.
                    response = await client.PostAsync(url, content);
                }

                // The response header for the Batch Read method contains the URI
                // of the second method, Read Operation Result, which
                // returns the results of the process in the response body.
                // The Batch Read operation does not return anything in the response body.
                if (response.IsSuccessStatusCode)
                    operationLocation =
                        response.Headers.GetValues("Operation-Location").FirstOrDefault();
                else
                {
                    // Display the JSON error data.
                    string errorString = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("\n\nResponse:\n{0}\n",
                        JToken.Parse(errorString).ToString());
                    return BadRequest();
                }

                // If the first REST API method completes successfully, the second 
                // REST API method retrieves the text written in the image.
                //
                // Note: The response may not be immediately available. Text
                // recognition is an asynchronous operation that can take a variable
                // amount of time depending on the length of the text.
                // You may need to wait or retry this operation.
                //
                // This example checks once per second for ten seconds.
                string contentString;
                int i = 0;
                do
                {
                    System.Threading.Thread.Sleep(1000);
                    response = await client.GetAsync(operationLocation);
                    contentString = await response.Content.ReadAsStringAsync();
                    ++i;
                }
                while (i < 60 && contentString.IndexOf("\"status\":\"succeeded\"") == -1);

                if (i == 60 && contentString.IndexOf("\"status\":\"succeeded\"") == -1)
                {
                    Console.WriteLine("\nTimeout error.\n");
                    return BadRequest();
                }

                // Display the JSON response.
                Console.WriteLine("\nResponse:\n\n{0}\n");
                var results = JToken.Parse(contentString).ToString();

                return Ok(results);
            }
            catch (Exception e)
            {
                Console.WriteLine("\n" + e.Message);
                return BadRequest();
            }
        }

        //with Computer Vision - OCR
        [HttpPost]
        [Route("api/computervision2")]
        public async Task<IHttpActionResult> ImageToTextOCR()
        {
            // Add your Computer Vision subscription key and endpoint to your environment variables.
            var subscriptionKey = "subscriptionkey";

            // An endpoint should have a format like "https://westus.api.cognitive.microsoft.com"
            var endpoint = "https://csc-task8.cognitiveservices.azure.com";

            // the Batch Read method endpoint
            var uriBase = endpoint + "/vision/v2.1/ocr";
            //Retrieve file
            var file = HttpContext.Current.Request.Files[0];
            //Convert hhtpfile into string
            //Convert hhtpfile into byte array
            byte[] fileData = null;
            using (var binaryReader = new BinaryReader(file.InputStream))
            {
                fileData = binaryReader.ReadBytes(file.ContentLength);
            }

            try
            {
                HttpClient client = new HttpClient();

                // Request headers.
                client.DefaultRequestHeaders.Add(
                    "Ocp-Apim-Subscription-Key", subscriptionKey);

                var requestParameters = "language=unk&detectOrientation=true";
                var url = uriBase + "?" + requestParameters;

                HttpResponseMessage response;

                // Adds the byte array as an octet stream to the request body.
                using (ByteArrayContent content = new ByteArrayContent(fileData))
                {
                    // This example uses the "application/octet-stream" content type.
                    // The other content types you can use are "application/json"
                    // and "multipart/form-data".
                    content.Headers.ContentType =
                        new MediaTypeHeaderValue("application/octet-stream");

                    // The first REST API method, Batch Read, starts
                    // the async process to analyze the written text in the image.
                    response = await client.PostAsync(url, content);
                }

                // Asynchronously get the JSON response.
                string contentString = await response.Content.ReadAsStringAsync();

                // Display the JSON response.
                Console.WriteLine("\nResponse:\n\n{0}\n",
                    JToken.Parse(contentString).ToString());
                return Ok(contentString);
            }
            catch (Exception e)
            {
                Console.WriteLine("\n" + e.Message);
                return BadRequest();
            }
        }        
    }
}
