using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stilbaai_Tourism_Web_Portal.Classes;
using Stilbaai_Tourism_Web_Portal.DBHandelers;
using Stilbaai_Tourism_Web_Portal.Models;
using System.Diagnostics;

namespace Stilbaai_Tourism_Web_Portal.Controllers
{
   public class StayController : Controller
   {
      private readonly ILogger<StayController> _logger;
      private readonly ToolBoxSingleton _ToolBox = ToolBoxSingleton.Instance;
      private readonly StayDBHandeler db = new StayDBHandeler();

      //---------------------------------------------------------------------------------------
      //default constructor
      public StayController(ILogger<StayController> logger)
      {
         _logger = logger;
      }

      //==========VIEWS==========

      #region Views

      //---------------------------------------------------------------------------------------
      //open edit stays view, allows for the editing of a selected view
      [ValidateAntiForgeryToken]
      [Authorize]
      public IActionResult EditStay(int id)
      {
         try
         {
            // Use LINQ to find the restaurant with the specified ID
            var selectedStay = _ToolBox.StayList.FirstOrDefault(r => r.STAY_ID == id);

            if (selectedStay == null)
            {
               return NotFound();
            }

            List<string> Categories = new List<string>();

            for (int i = 0; i != _ToolBox.StayCategoryList.Count; i++)
            {
               Categories.Add(_ToolBox.StayCategoryList[i].STAY_CATEGORY_TYPE);
            }
            ViewBag.Categories = Categories;

            int SelectedCategoryID = _ToolBox.StayList.FirstOrDefault(x => x.STAY_ID == id).STAY_CATEGORY_ID;
            if (SelectedCategoryID != 0)
            {
               string SelectedCategory = _ToolBox.StayCategoryList.FirstOrDefault(x => x.STAY_CATEGORY_ID == SelectedCategoryID).STAY_CATEGORY_TYPE;
               ViewBag.SelectedCategory = SelectedCategory;
            }

            return View(selectedStay);
         }
         catch (Exception ex)
         {
            // Handle the exception, log it, and provide a user-friendly error message.
            _logger.LogError(ex, "An error occurred in EditStays action.");
            return View("Error");
         }
      }

      //---------------------------------------------------------------------------------------
      //open view stays view, allows for the viewing of all stays
      [ValidateAntiForgeryToken]
      [Authorize]
      public async Task<IActionResult> ViewStays()
      {
         await db.GetStayCategory();
         await this.db.GetStay();

         ViewBag.Stay = _ToolBox.StayList;
         return View();
      }

      //---------------------------------------------------------------------------------------
      //open add new stay view
      [ValidateAntiForgeryToken]
      [Authorize]
      public IActionResult AddNewStay()
      {

         List<string> Categories = new List<string>();
         for (int i = 0; i != _ToolBox.StayCategoryList.Count; i++)
         {
            Categories.Add(_ToolBox.StayCategoryList[i].STAY_CATEGORY_TYPE);
         }
         ViewBag.Categories = Categories;

         return View();
      }

      //---------------------------------------------------------------------------------------
      //open view stay images view
      [ValidateAntiForgeryToken]
      [Authorize]
      public async Task<IActionResult> ViewStayImages(int id)
      {
         var selectedStay = _ToolBox.StayList.FirstOrDefault(r => r.STAY_ID == id);

         List<string> urls = new List<string>();
         urls = await this.db.GetStayImages(selectedStay.STAY_ID);

         if (urls != null)
         {
            ViewBag.imageUrls = urls;
         }
         else
         {
            return NotFound();
         }

         return View(selectedStay);
      }

      #endregion

      //==========POSTBACKS==========

      //---------------------------------------------------------------------------------------
      //post back to delte an image
      [ValidateAntiForgeryToken]
      [Authorize]
      [HttpPost]
      public async Task<IActionResult> DeleteImage(int stayId, string imageUrl)
      {
         try
         {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
               return Json(new { success = false, message = "Invalid image URL provided." });
            }

            bool deletionResult = await this.db.DeleteImage(stayId, imageUrl);

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
      //save stay post back, handles the updating of the model
      [ValidateAntiForgeryToken]
      [Authorize]
      [HttpPost]
      public async Task<IActionResult> SaveStay(StayModel updatedStay)
      {
         try
         {
            if (!ModelState.IsValid)
            {
               return Json(new { success = false, message = "Invalid model data or Stay ID." });
            }

            updatedStay.STAY_CATEGORY_ID = _ToolBox.StayCategoryList.FirstOrDefault(x => x.STAY_CATEGORY_TYPE == updatedStay.STAY_CATEGORY_NAME).STAY_CATEGORY_ID;

            bool updateResult = await this.db.UpdateStay(updatedStay);

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
            _logger.LogError(ex, "An error occurred while updating an stay.");
            return Json(new { success = false, message = "An error occurred while updating the stay." });
         }
      }

      //---------------------------------------------------------------------------------------
      //save stay post back, handles the updating of the model
      [ValidateAntiForgeryToken]
      [Authorize]
      [HttpPost]
      public async Task<IActionResult> DeleteStay(int STAY_ID)
      {
         try
         {
            if (STAY_ID <= 0)
            {
               return Json(new { success = false, message = "Invalid STAY_ID provided." });
            }

            //check no images are associated in the db
            List<string> urls = new List<string>();
            urls = await this.db.GetStayImages(STAY_ID);

            if (urls.Count != 0)
            {
               return Json(new { success = false, message = "This entry has images, please remove them first." });
            }

            bool deletionResult = await this.db.DeleteStay(STAY_ID);

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
            _logger.LogError(ex, "An error occurred while deleting the stay.");
            return Json(new { success = false, message = "Internal server error. Please try again later." });
         }
      }

      //---------------------------------------------------------------------------------------
      //method to add a new entry, and the images to storage
      [ValidateAntiForgeryToken]
      [Authorize]
      [HttpPost]
      public async Task<IActionResult> AddStay(StayModel newStay, List<IFormFile> imageFiles)
      {
         try
         {
            if (!ModelState.IsValid)
            {
               return Json(new { success = false, message = "Invalid model data or Stay ID." });
            }

            if (imageFiles == null || !imageFiles.Any())
            {
               return Json(new { success = false, message = "No images provided." });
            }

            newStay.STAY_CATEGORY_ID = _ToolBox.StayCategoryList.FirstOrDefault(x => x.STAY_CATEGORY_TYPE == newStay.STAY_CATEGORY_NAME).STAY_CATEGORY_ID;

            int newStayId = await AddStayToDatabase(newStay);

            if (newStayId == -1)
            {
               return Json(new { success = false, message = "Error while saving the stay data." });
            }

            List<string> imageURLs = await ProcessAndSaveImages(imageFiles);

            if (imageURLs == null || imageURLs.Count == 0)
            {
               return Json(new { success = false, message = "Error while saving images." });
            }

            int results = await AddImagesToDatabase(imageURLs, newStayId);

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
      //method to add a new entry, and the images ot storage
      [ValidateAntiForgeryToken]
      [Authorize]
      [HttpPost]
      public async Task<IActionResult> AddImages(List<IFormFile> imageFiles, int stayId)
      {
         try
         {
            if (!ModelState.IsValid || stayId <= 0)
            {
               return Json(new { success = false, message = "Invalid model data or Stay ID." });
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

            int results = await AddImagesToDatabase(imageURLs, stayId);

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
      [ValidateAntiForgeryToken]
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
      //add the new stay to the db
      [ValidateAntiForgeryToken]
      [Authorize]
      private async Task<int> AddStayToDatabase(StayModel newStay)
      {
         return await this.db.AddStay(newStay);
      }

      //---------------------------------------------------------------------------------------
      //add the image url/s to the db
      [ValidateAntiForgeryToken]
      [Authorize]
      private async Task<int> AddImagesToDatabase(List<string> imageURLs, int newStayId)
      {
         int rowsAfftected = 0;

         foreach (string url in imageURLs)
         {
            rowsAfftected += await this.db.AddStayImage(url, newStayId);
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