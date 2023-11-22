using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stilbaai_Tourism_Web_Portal.Classes;
using Stilbaai_Tourism_Web_Portal.Models;
using Stilbaai_Tourism_Web_Portal.Workers;
using System.Diagnostics;

namespace Stilbaai_Tourism_Web_Portal.Controllers
{
   public class EatController : Controller
   {
      private readonly ILogger<EatController> _logger;
      private readonly ToolBoxSingleton _ToolBox = ToolBoxSingleton.Instance;
      private readonly EatsDBHandeler db = new EatsDBHandeler();

      //---------------------------------------------------------------------------------------
      /// <summary>
      /// default constructor
      /// </summary>
      /// <param name="logger"></param>
      public EatController(ILogger<EatController> logger)
      {
         _logger = logger;
      }

      //==========VIEWS==========

      #region Views

      //---------------------------------------------------------------------------------------
      /// <summary>
      /// open edit eats view, allows for the editing of a selected view
      /// </summary>
      /// <param name="id"></param>
      /// <returns></returns>
      [Authorize]
      public IActionResult EditEats(int id)
      {
         try
         {
            // Use LINQ to find the restaurant with the specified ID
            var selectedEat = _ToolBox.EatsList.FirstOrDefault(r => r.EAT_ID == id);

            if (selectedEat == null)
            {
               return NotFound();
            }

            return View(selectedEat);
         }
         catch (Exception ex)
         {
            // Handle the exception, log it, and provide a user-friendly error message.
            _logger.LogError(ex, "An error occurred in EditEats action.");
            return View("Error");
         }
      }

      //---------------------------------------------------------------------------------------
      /// <summary>
      /// open view eats view, allows for the viewing of all eats
      /// </summary>
      /// <returns></returns>
      [Authorize]
      public async Task<IActionResult> ViewEats()
      {
         await this.db.GetEat();

         ViewBag.Eats = _ToolBox.EatsList;
         return View();
      }

      //---------------------------------------------------------------------------------------
      /// <summary>
      /// open add new eat view
      /// </summary>
      /// <returns></returns>
      [Authorize]
      public IActionResult AddNewEat()
      {
         return View();
      }

      //---------------------------------------------------------------------------------------
      /// <summary>
      /// open view eat images view
      /// </summary>
      /// <param name="id"></param>
      /// <returns></returns>
      [Authorize]
      public async Task<IActionResult> ViewEatImages(int id)
      {
         var selectedEat = _ToolBox.EatsList.FirstOrDefault(r => r.EAT_ID == id);

         List<string> urls;
         urls = await this.db.GetEatImages(selectedEat.EAT_ID);

         if (urls != null)
         {
            ViewBag.imageUrls = urls;
         }
         else
         {
            return NotFound();
         }

         return View(selectedEat);
      }

      #endregion

      //==========POSTBACKS==========

      //---------------------------------------------------------------------------------------
      /// <summary>
      /// post back to delte an image
      /// </summary>
      /// <param name="eatId"></param>
      /// <param name="imageUrl"></param>
      /// <returns></returns>
      [Authorize]
      [HttpPost]
      public async Task<IActionResult> DeleteImage(int eatId, string imageUrl)
      {
         try
         {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
               return Json(new { success = false, message = "Invalid image URL provided." });
            }

            bool deletionResult = await this.db.DeleteImage(eatId, imageUrl);

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
      /// <summary>
      /// save eat post back, handles the updating of the model
      /// </summary>
      /// <param name="updatedEat"></param>
      /// <returns></returns>
      [Authorize]
      [HttpPost]
      public async Task<IActionResult> SaveEat(EatModel updatedEat)
      {
         try
         {
            if (!ModelState.IsValid)
            {
               return Json(new { success = false, message = "Invalid model data or Eat ID." });
            }

            bool updateResult = await this.db.UpdateEat(updatedEat);

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
            _logger.LogError(ex, "An error occurred while updating an eat.");
            return Json(new { success = false, message = "An error occurred while updating the eat." });
         }
      }

      //---------------------------------------------------------------------------------------
      /// <summary>
      /// delete entry postback
      /// </summary>
      /// <param name="EAT_ID"></param>
      /// <returns></returns>
      [Authorize]
      [HttpPost]
      public async Task<IActionResult> DeleteEat(int EAT_ID)
      {
         try
         {
            if (EAT_ID <= 0)
            {
               return Json(new { success = false, message = "Invalid EAT_ID provided." });
            }

            //check no images are associated in the db
            List<string> urls;
            urls = await this.db.GetEatImages(EAT_ID);

            if (urls.Count != 0)
            {
               return Json(new { success = false, message = "This entry has images, please remove them first." });
            }

            bool deletionResult = await this.db.DeleteEat(EAT_ID);

            if (deletionResult)
            {
               return Json(new { success = true });
            }
            else
            {
               // Deletion failed for some reason, return a suitable error response
               return Json(new { success = false, message = "Deletion failed. Please try again." });
            }
         }
         catch (Exception ex)
         {
            _logger.LogError(ex, "An error occurred while deleting the eat.");
            return Json(new { success = false, message = "Internal server error. Please try again later." });
         }
      }

      //---------------------------------------------------------------------------------------
      /// <summary>
      /// method to add a new entry, and the images to storage
      /// </summary>
      /// <param name="newEat"></param>
      /// <param name="imageFiles"></param>
      /// <returns></returns>
      [Authorize]
      [HttpPost]
      public async Task<IActionResult> AddEat(EatModel newEat, List<IFormFile> imageFiles)
      {
         try
         {
            if (!ModelState.IsValid)
            {
               return Json(new { success = false, message = "Invalid model data or Eat ID." });
            }

            if (imageFiles == null || !imageFiles.Any())
            {
               return Json(new { success = false, message = "No images provided." });
            }

            int newEatId = await AddEatToDatabase(newEat);

            if (newEatId == -1)
            {
               return Json(new { success = false, message = "Error while saving the eat data." });
            }

            List<string> imageURLs = await ProcessAndSaveImages(imageFiles);

            if (imageURLs == null || imageURLs.Count == 0)
            {
               return Json(new { success = false, message = "Error while saving images." });
            }

            int results = await AddImagesToDatabase(imageURLs, newEatId);

            if (results == 0 || results != imageURLs.Count)
            {
               return Json(new { success = false, message = "Entry created however, One of more images failed to save, please check" });
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
      /// <summary>
      /// method to add a new images ot storage
      /// </summary>
      /// <param name="imageFiles"></param>
      /// <param name="eatId"></param>
      /// <returns></returns>
      [Authorize]
      [HttpPost]
      public async Task<IActionResult> AddImages(List<IFormFile> imageFiles, int eatId)
      {
         try
         {
            if (!ModelState.IsValid || eatId <= 0)
            {
               return Json(new { success = false, message = "Invalid model data or Eat ID." });
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

            int results = await AddImagesToDatabase(imageURLs, eatId);

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
      /// <summary>
      /// send the images to the api handeler
      /// </summary>
      /// <param name="imageFiles"></param>
      /// <returns></returns>
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
      /// <summary>
      /// add the new eat to the db
      /// </summary>
      /// <param name="newEat"></param>
      /// <returns></returns>
      [Authorize]
      private async Task<int> AddEatToDatabase(EatModel newEat)
      {
         return await this.db.AddEat(newEat);
      }

      //---------------------------------------------------------------------------------------
      /// <summary>
      /// add the image url/s to the db
      /// </summary>
      /// <param name="imageURLs"></param>
      /// <param name="newEatId"></param>
      /// <returns></returns>
      [Authorize]
      private async Task<int> AddImagesToDatabase(List<string> imageURLs, int newEatId)
      {
         int rowsAfftected = 0;

         foreach (string url in imageURLs)
         {
            rowsAfftected += await this.db.AddEatImage(url, newEatId);
         }

         return rowsAfftected;
      }

      //---------------------------------------------------------------------------------------
      /// <summary>
      /// error response
      /// </summary>
      /// <returns></returns>
      [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
      public IActionResult Error()
      {
         return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
      }
   }
}
//-------------------------------------====END OF FILE====-------------------------------------