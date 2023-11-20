using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stilbaai_Tourism_Web_Portal.Classes;
using Stilbaai_Tourism_Web_Portal.Models;
using Stilbaai_Tourism_Web_Portal.Workers;
using System.Diagnostics;
namespace Stilbaai_Tourism_Web_Portal.Controllers
{
   public class ActivityController : Controller
   {
      private readonly ILogger<ActivityController> _logger;
      private readonly ToolBoxSingleton _ToolBox = ToolBoxSingleton.Instance;
      private readonly ActivityDBHandeler db = new ActivityDBHandeler();

      //==========VIEWS==========

      #region Views

      //---------------------------------------------------------------------------------------
      //open edit activitys view, allows for the editing of a selected view
      [Authorize]
      public IActionResult EditActivity(int id)
      {
         try
         {
            // Use LINQ to find the restaurant with the specified ID
            var selectedActivity = _ToolBox.ActivityList.FirstOrDefault(r => r.ACTIVITY_ID == id);

            if (selectedActivity == null)
            {
               return NotFound();
            }

            List<string> Categories = new List<string>();

            for (int i = 0; i != _ToolBox.ActivityCategoryList.Count; i++)
            {
               Categories.Add(_ToolBox.ActivityCategoryList[i].ACTIVITY_CATEGORY_TYPE);
            }
            ViewBag.Categories = Categories;

            int SelectedCategoryID = _ToolBox.ActivityList.FirstOrDefault(x => x.ACTIVITY_ID == id).ACTIVITY_CATEGORY_ID;
            if (SelectedCategoryID != 0)
            {
               string SelectedCategory = _ToolBox.ActivityCategoryList.FirstOrDefault(x => x.ACTIVITY_CATEGORY_ID == SelectedCategoryID).ACTIVITY_CATEGORY_TYPE;
               ViewBag.SelectedCategory = SelectedCategory;
            }

            return View(selectedActivity);
         }
         catch (Exception ex)
         {
            // Handle the exception, log it, and provide a user-friendly error message.
            _logger.LogError(ex, "An error occurred in EditActivitys action.");
            return View("Error");
         }
      }

      //---------------------------------------------------------------------------------------
      //open view activitys view, allows for the viewing of all activitys
      [Authorize]
      public async Task<IActionResult> ViewActivities()
      {
         await db.GetActivityCategory();
         await this.db.GetActivity();

         ViewBag.Activity = _ToolBox.ActivityList;
         return View();
      }

      //---------------------------------------------------------------------------------------
      //open add new activity view
      [Authorize]
      public IActionResult AddNewActivity()
      {

         List<string> Categories = new List<string>();
         for (int i = 0; i != _ToolBox.ActivityCategoryList.Count; i++)
         {
            Categories.Add(_ToolBox.ActivityCategoryList[i].ACTIVITY_CATEGORY_TYPE);
         }
         ViewBag.Categories = Categories;

         return View();
      }

      //---------------------------------------------------------------------------------------
      //open view activity images view
      [Authorize]
      public async Task<IActionResult> ViewActivityImages(int id)
      {
         var selectedActivity = _ToolBox.ActivityList.FirstOrDefault(r => r.ACTIVITY_ID == id);

         List<string> urls;
         urls = await this.db.GetActivityImages(selectedActivity.ACTIVITY_ID);

         if (urls != null)
         {
            ViewBag.imageUrls = urls;
         }
         else
         {
            return NotFound();
         }

         return View(selectedActivity);
      }

      #endregion

      //==========POSTBACKS==========

      //---------------------------------------------------------------------------------------
      //post back to delte an image
      [Authorize]
      [HttpPost]
      public async Task<IActionResult> DeleteImage(int activityId, string imageUrl)
      {
         try
         {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
               return Json(new { success = false, message = "Invalid image URL provided." });
            }

            bool deletionResult = await this.db.DeleteImage(activityId, imageUrl);

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
      //save activity post back, handles the updating of the model
      [Authorize]
      [HttpPost]
      public async Task<IActionResult> SaveActivity(ActivityModel updatedActivity)
      {
         try
         {
            if (!ModelState.IsValid)
            {
               return Json(new { success = false, message = "Invalid model data or Activity ID." });
            }

            updatedActivity.ACTIVITY_CATEGORY_ID = _ToolBox.ActivityCategoryList.FirstOrDefault(x => x.ACTIVITY_CATEGORY_TYPE == updatedActivity.ACTIVITY_CATEGORY_NAME).ACTIVITY_CATEGORY_ID;

            bool updateResult = await this.db.UpdateActivity(updatedActivity);

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
            _logger.LogError(ex, "An error occurred while updating an activity.");
            return Json(new { success = false, message = "An error occurred while updating the activity." });
         }
      }

      //---------------------------------------------------------------------------------------
      //save activity post back, handles the updating of the model
      [Authorize]
      [HttpPost]
      public async Task<IActionResult> DeleteActivity(int ACTIVITY_ID)
      {
         try
         {
            if (ACTIVITY_ID <= 0)
            {
               return Json(new { success = false, message = "Invalid ACTIVITY_ID provided." });
            }

            //check no images are associated in the db
            List<string> urls = new List<string>();
            urls = await this.db.GetActivityImages(ACTIVITY_ID);

            if (urls.Count != 0)
            {
               return Json(new { success = false, message = "This entry has images, please remove them first." });
            }

            bool deletionResult = await this.db.DeleteActivity(ACTIVITY_ID);

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
            _logger.LogError(ex, "An error occurred while deleting the activity.");
            return Json(new { success = false, message = "Internal server error. Please try again later." });
         }
      }

      //---------------------------------------------------------------------------------------
      //method to add a new entry, and the images to storage
      [Authorize]
      [HttpPost]
      public async Task<IActionResult> AddActivity(ActivityModel newActivity, List<IFormFile> imageFiles)
      {
         try
         {
            if (!ModelState.IsValid)
            {
               return Json(new { success = false, message = "Invalid model data or Activity ID." });
            }

            if (imageFiles == null || !imageFiles.Any())
            {
               return Json(new { success = false, message = "No images provided." });
            }

            newActivity.ACTIVITY_CATEGORY_ID = _ToolBox.ActivityCategoryList.FirstOrDefault(x => x.ACTIVITY_CATEGORY_TYPE == newActivity.ACTIVITY_CATEGORY_NAME).ACTIVITY_CATEGORY_ID;

            int newActivityId = await AddActivityToDatabase(newActivity);

            if (newActivityId == -1)
            {
               return Json(new { success = false, message = "Error while saving the activity data." });
            }

            List<string> imageURLs = await ProcessAndSaveImages(imageFiles);

            if (imageURLs == null || imageURLs.Count == 0)
            {
               return Json(new { success = false, message = "Error while saving images." });
            }

            int results = await AddImagesToDatabase(imageURLs, newActivityId);

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
      [Authorize]
      [HttpPost]
      public async Task<IActionResult> AddImages(List<IFormFile> imageFiles, int activityId)
      {
         try
         {
            if (!ModelState.IsValid || activityId <= 0)
            {
               return Json(new { success = false, message = "Invalid model data or Activity ID." });
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

            int results = await AddImagesToDatabase(imageURLs, activityId);

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
      //add the new activity to the db
      [Authorize]
      private async Task<int> AddActivityToDatabase(ActivityModel newActivity)
      {
         return await this.db.AddActivity(newActivity);
      }

      //---------------------------------------------------------------------------------------
      //add the image url/s to the db
      [Authorize]
      private async Task<int> AddImagesToDatabase(List<string> imageURLs, int newActivityId)
      {
         int rowsAfftected = 0;

         foreach (string url in imageURLs)
         {
            rowsAfftected += await this.db.AddActivityImage(url, newActivityId);
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
