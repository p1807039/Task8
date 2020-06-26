using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Threading.Tasks;
using Clarifai.API;
using Clarifai.DTOs.Inputs;
using Clarifai.DTOs.Predictions;
using System.IO;
using System.Web;
using System.Web.Http.Results;

namespace Task8.Controllers
{
    public class ClarifaiController : ApiController
    {

        // API For Upload Image into Clarifai APi and Tagging After Upload

        [HttpPost]
        [Route("api/clarifai")]
        public async Task<IHttpActionResult> UploadImage()
        {
            //Retrieve file
            var file = HttpContext.Current.Request.Files[0];
            //Convert httpfile into byte array in order to parse into Clarifai
            byte[] fileData = null;
            using (var binaryReader = new BinaryReader(file.InputStream))
            {
                fileData = binaryReader.ReadBytes(file.ContentLength);
            }

            //Try Catch to catch any errors if unable to connect to API.
            try
            {
                // When passed in as a string
                //Calling API Key of Clarifai
                var Client = new ClarifaiClient("apikeys");

                // When using async/await, via bytes
                // Using Clarifai to Predict and Get Taggings.
                var res = await Client.Predict<Concept>(
                    Client.PublicModels.GeneralModel.ModelID,
                    new List<IClarifaiInput>
                    {
                        new ClarifaiFileImage(fileData)
                    },
                    "apikeys")
                .ExecuteAsync();

                //Check if response is successful or not.
                if (res.IsSuccessful)
                {
                    //Retrieve Name of Tagging
                    var concepts = res.Get()[0].Data.Select(c => $"{c.Name}");
                    //Join string
                    var body = string.Join("Taggings", concepts);

                    //to return formatted string to client side
                    return Ok(concepts);
                }
                else 
                {
                    return BadRequest($"The request was not successful: {res.Status.Description}");
                }
            }
            catch (Exception e)
            {
                return BadRequest($"Something went wrong: {e.Message}");
            }
        }
    }
}
 