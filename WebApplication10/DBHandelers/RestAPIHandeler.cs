using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;

namespace Stilbaai_Tourism_Web_Portal.DBHandelers
{
   public class RestAPIHandeler
   {
      private readonly HttpClient _client;

      private readonly string apiUrl = Properties.Resources.ResourceManager.GetString("APIURL");
      private readonly string username = Properties.Resources.ResourceManager.GetString("APIUsername");
      private readonly string password = Properties.Resources.ResourceManager.GetString("APIPassword");

      //---------------------------------------------------------------------------------------
      public RestAPIHandeler()
      {
         _client = new HttpClient();
      }

      //---------------------------------------------------------------------------------------
      public async Task<string> AddImage(byte[] imageBytes, string name)
      {
         try
         {
            var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);

            // Add authentication header
            string credentials = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{username}:{password}"));
            request.Headers.Add("Authorization", "Basic " + credentials);

            // Add a user agent header to identify the request
            request.Headers.Add("User-Agent", "YourApp");

            // Add the image and information about the image type
            // The API is able to dynamically check what type the image is and can change the file name accordingly
            var imageStream = new MemoryStream(imageBytes);
            var imageContent = new StreamContent(imageStream);
            imageContent.Headers.Add("Content-Disposition", "attachment; filename=" + name + ".jpg");
            imageContent.Headers.Add("Content-Type", "image/jpg");
            request.Content = imageContent;

            // Send the response
            var response = await _client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
               // Handle success
               string jsonResponse = await response.Content.ReadAsStringAsync();
               string renderedUrl = ExtractRenderedUrlFromJson(jsonResponse);
               System.Diagnostics.Trace.WriteLine("Image successfully uploaded. Rendered URL: " + renderedUrl);
               return renderedUrl;
            }
            else
            {
               // Handle errors
               System.Diagnostics.Trace.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
               return null; // Indicate failure
            }
         }
         catch (Exception ex)
         {
            // Log and handle unexpected exceptions
            System.Diagnostics.Trace.WriteLine("An error occurred while making the API request" + ex);
            return null; // Indicate failure
         }
      }

      //---------------------------------------------------------------------------------------
      private static string ExtractRenderedUrlFromJson(string jsonResponse)
      {
         JObject jsonObject = JObject.Parse(jsonResponse);
         return jsonObject["guid"]["rendered"].Value<string>();
      }
   }
}
//-------------------------------------====END OF FILE====-------------------------------------