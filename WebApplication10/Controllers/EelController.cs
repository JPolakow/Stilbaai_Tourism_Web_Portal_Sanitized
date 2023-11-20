using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stilbaai_Tourism_Web_Portal.Classes;
using Stilbaai_Tourism_Web_Portal.DBHandelers;
using Stilbaai_Tourism_Web_Portal.Models;
using System.Diagnostics;

namespace Stilbaai_Tourism_Web_Portal.Controllers
{
   public class EelController : Controller
   {
      private readonly ILogger<EelController> _logger;
      private readonly ToolBoxSingleton _ToolBox = ToolBoxSingleton.Instance;
      private readonly EelDBHandeler db = new EelDBHandeler();

      //---------------------------------------------------------------------------------------
      //default constructor
      public EelController(ILogger<EelController> logger)
      {
         _logger = logger;
      }

      //==========VIEWS==========
      //---------------------------------------------------------------------------------------
      //open edit eels view, allows for the editing of a selected view
      [Authorize]
      public async Task<IActionResult> EditEel()
      {
         try
         {
            await this.db.GetEel();

            // Use LINQ to find the restaurant with the specified ID
            var selectedEel = _ToolBox.EelList.FirstOrDefault(r => r.EEL_ID == 1);

            if (selectedEel == null)
            {
               return NotFound();
            }

            return View(selectedEel);
         }
         catch (Exception ex)
         {
            // Handle the exception, log it, and provide a user-friendly error message.
            _logger.LogError(ex, "An error occurred in EditEel action.");
            return View("Error");
         }
      }

      //---------------------------------------------------------------------------------------
      //open view eel images view
      [Authorize]
      public async Task<IActionResult> ViewEelImages()
      {
         var selectedEel = _ToolBox.EelList.FirstOrDefault(r => r.EEL_ID == 1);

         List<string> urls;
         urls = await this.db.GetEelImages(selectedEel.EEL_ID);

         if (urls != null)
         {
            ViewBag.imageUrls = urls;
         }
         else
         {
            return NotFound();
         }

         return View(selectedEel);
      }

      //==========POSTBACKS==========

      //---------------------------------------------------------------------------------------
      //post back to delte an image
      [Authorize]
      [HttpPost]
      public async Task<IActionResult> DeleteImage(string imageUrl)
      {
         try
         {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
               return Json(new { success = false, message = "Invalid image URL provided." });
            }

            bool deletionResult = await this.db.DeleteImage(1, imageUrl);

            if (deletionResult)
            {
               return Json(new { success = true });
            }
            else
            {
               return Json(new { success = false, message = "Deletion failed. Please try again." });
            }
         }
         catch (Exception ex)
         {
            _logger.LogError(ex, "An error occurred while deleting the image.");
            return Json(new { success = false, message = "Internal server error. Please try again later." });
         }
      }


      //---------------------------------------------------------------------------------------
      //save eel post back, handles the updating of the model
      [Authorize]
      [HttpPost]
      public async Task<IActionResult> SaveEel(EelModel updatedEel)
      {
         try
         {
            if (!ModelState.IsValid)
            {
               return Json(new { success = false, message = "Invalid model data or eel ID." });
            }

            bool updateResult = await this.db.UpdateEel(updatedEel);

            if (updateResult)
            {
               return Json(new { success = true });
            }
            else
            {
               return Json(new { success = false, message = "Update failed. Please try again." });
            }
         }
         catch (Exception ex)
         {
            _logger.LogError(ex, "An error occurred while updating an eel.");
            return Json(new { success = false, message = "An error occurred while updating the eel." });
         }
      }

      //---------------------------------------------------------------------------------------
      //method to add a new entry, and the images ot storage
      [Authorize]
      [HttpPost]
      public async Task<IActionResult> AddImages(List<IFormFile> imageFiles)
      {
         try
         {
            if (!ModelState.IsValid)
            {
               return Json(new { success = false, message = "Invalid model data or eel ID." });
            }

            if (imageFiles == null || !imageFiles.Any())
            {
               return Json(new { success = false, message = "No images provided." });
            }

            List<string> imageURLs = await ProcessAndSaveImages(imageFiles);

            if (imageURLs == null || imageURLs.Count == 0)
            {
               return Json(new { success = false, message = "Error while saving images." });
            }

            int results = await AddImagesToDatabase(imageURLs);

            if (results == 0 || results != imageURLs.Count)
            {
               return Json(new { success = false, message = "One of more images failed to save, please check" });
            }

            return Json(new { success = true });
         }
         catch (Exception ex)
         {
            _logger.LogError(ex, "An error occurred while adding images.");
            return Json(new { success = false, message = "Internal server error. Please try again later." });
         }
      }


      //---------------------------------------------------------------------------------------
      //send the images to the api handeler
      [Authorize]
      private async Task<List<string>> ProcessAndSaveImages(List<IFormFile> imageFiles)
      {
         try
         {
            List<string> imageURLs = new List<string>();

            foreach (var imageFile in imageFiles)
            {
               if (imageFile.Length > 0)
               {
                  byte[] imageBytes;

                  using (MemoryStream stream = new MemoryStream())
                  {
                     await imageFile.CopyToAsync(stream);
                     imageBytes = stream.ToArray();
                  }

                  string imageUrl = await _ToolBox.APIHandeler.AddImage(imageBytes, "test");

                  if (imageUrl != "error")
                  {
                     imageURLs.Add(imageUrl);
                  }
                  else
                  {
                     return null;
                  }
               }
            }

            return imageURLs;
         }
         catch (Exception ex)
         {
            _logger.LogError(ex, "An error occurred while adding images.");
            return null;
         }
      }

      //---------------------------------------------------------------------------------------
      //add the image url/s to the db
      [Authorize]
      private async Task<int> AddImagesToDatabase(List<string> imageURLs)
      {
         int rowsAfftected = 0;

         foreach (string url in imageURLs)
         {
            rowsAfftected += await this.db.AddEelImage(url, 1);
         }

         return rowsAfftected;
      }

      //---------------------------------------------------------------------------------------
      //error response
      [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
      public IActionResult Error()
      {
         return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
      }
   }
}
//-------------------------------------====END OF FILE====-------------------------------------