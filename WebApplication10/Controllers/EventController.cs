using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stilbaai_Tourism_Web_Portal.Classes;
using Stilbaai_Tourism_Web_Portal.DBHandelers;
using Stilbaai_Tourism_Web_Portal.Models;
using System.Diagnostics;

namespace Stilbaai_Tourism_Web_Portal.Controllers
{
   public class EventController : Controller
   {
      private readonly ILogger<EventController> _logger;
      private readonly ToolBoxSingleton _ToolBox = ToolBoxSingleton.Instance;
      private readonly EventDBHandeler db = new EventDBHandeler();

      //---------------------------------------------------------------------------------------
      //default constructor
      public EventController(ILogger<EventController> logger)
      {
         _logger = logger;
      }

      //==========VIEWS==========

      #region Views

      //---------------------------------------------------------------------------------------
      //open edit events view, allows for the editing of a selected view
      [ValidateAntiForgeryToken]
      [Authorize]
      public IActionResult EditEvent(int id)
      {
         try
         {
            // Use LINQ to find the restaurant with the specified ID
            var selectedEvent = _ToolBox.EventList.FirstOrDefault(r => r.EVENT_ID == id);

            if (selectedEvent == null)
            {
               return NotFound();
            }

            return View(selectedEvent);
         }
         catch (Exception ex)
         {
            // Handle the exception, log it, and provide a user-friendly error message.
            _logger.LogError(ex, "An error occurred in EditEvents action.");
            return View("Error");
         }
      }

      //---------------------------------------------------------------------------------------
      //open view events view, allows for the viewing of all events
      [ValidateAntiForgeryToken]
      [Authorize]
      public async Task<IActionResult> ViewEvents()
      {
         // await db.GetEventCategory();
         await this.db.GetEvent();

         ViewBag.Events = _ToolBox.EventList;
         return View();
      }

      //---------------------------------------------------------------------------------------
      //open add new event view
      [Authorize]
      public IActionResult AddNewEvent()
      {
         return View();
      }

      //---------------------------------------------------------------------------------------
      //open view event images view
      [ValidateAntiForgeryToken]
      [Authorize]
      public async Task<IActionResult> ViewEventImages(int id)
      {
         var selectedEvent = _ToolBox.EventList.FirstOrDefault(r => r.EVENT_ID == id);

         List<string> urls = new List<string>();
         urls = await this.db.GetEventImages(selectedEvent.EVENT_ID);

         if (urls != null)
         {
            ViewBag.imageUrls = urls;
         }
         else
         {
            return NotFound();
         }

         return View(selectedEvent);
      }

      #endregion

      //==========POSTBACKS==========

      //---------------------------------------------------------------------------------------
      //post back to delte an image
      [ValidateAntiForgeryToken]
      [Authorize]
      [HttpPost]
      public async Task<IActionResult> DeleteImage(int eventId, string imageUrl)
      {
         try
         {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
               return Json(new { success = false, message = "Invalid image URL provided." });
            }

            bool deletionResult = await this.db.DeleteImage(eventId, imageUrl);

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
      //save event post back, handles the updating of the model
      [ValidateAntiForgeryToken]
      [Authorize]
      [HttpPost]
      public async Task<IActionResult> SaveEvent(EventModel updatedEvent)
      {
         try
         {
            if (!ModelState.IsValid)
            {
               return Json(new { success = false, message = "Invalid model data or Event ID." });
            }

            bool updateResult = await this.db.UpdateEvent(updatedEvent);

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
            _logger.LogError(ex, "An error occurred while updating an event.");
            return Json(new { success = false, message = "An error occurred while updating the event." });
         }
      }

      //---------------------------------------------------------------------------------------
      //save event post back, handles the updating of the model
      [ValidateAntiForgeryToken]
      [Authorize]
      [HttpPost]
      public async Task<IActionResult> DeleteEvent(int EVENT_ID)
      {
         try
         {
            if (EVENT_ID <= 0)
            {
               return Json(new { success = false, message = "Invalid EVENT_ID provided." });
            }

            //check no images are associated in the db
            List<string> urls = new List<string>();
            urls = await this.db.GetEventImages(EVENT_ID);

            if (urls.Count != 0)
            {
               return Json(new { success = false, message = "This entry has images, please remove them first." });
            }

            bool deletionResult = await this.db.DeleteEvent(EVENT_ID);

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
            _logger.LogError(ex, "An error occurred while deleting the event.");
            return Json(new { success = false, message = "Internal server error. Please try again later." });
         }
      }

      //---------------------------------------------------------------------------------------
      //method to add a new entry, and the images to storage
      [ValidateAntiForgeryToken]
      [Authorize]
      [HttpPost]
      public async Task<IActionResult> AddEvent(EventModel newEvent, List<IFormFile> imageFiles)
      {
         try
         {
            if (!ModelState.IsValid)
            {
               return Json(new { success = false, message = "Invalid model data or Event ID." });
            }

            if (imageFiles == null || !imageFiles.Any())
            {
               return Json(new { success = false, message = "No images provided." });
            }

            int newEventId = await AddEventToDatabase(newEvent);

            if (newEventId == -1)
            {
               return Json(new { success = false, message = "Error while saving the event data." });
            }

            List<string> imageURLs = await ProcessAndSaveImages(imageFiles);

            if (imageURLs == null || imageURLs.Count == 0)
            {
               return Json(new { success = false, message = "Error while saving images." });
            }

            int results = await AddImagesToDatabase(imageURLs, newEventId);

            if (results == 0 || results != imageURLs.Count)
            {
               return Json(new { success = false, message = "Entry crevented however, One of more images failed to save, please check" });
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
      public async Task<IActionResult> AddImages(List<IFormFile> imageFiles, int eventId)
      {
         try
         {
            if (!ModelState.IsValid || eventId <= 0)
            {
               return Json(new { success = false, message = "Invalid model data or Event ID." });
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

            int results = await AddImagesToDatabase(imageURLs, eventId);

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
      //add the new event to the db
      [ValidateAntiForgeryToken]
      [Authorize]
      private async Task<int> AddEventToDatabase(EventModel newEvent)
      {
         return await this.db.AddEvent(newEvent);
      }

      //---------------------------------------------------------------------------------------
      //add the image url/s to the db
      [ValidateAntiForgeryToken]
      [Authorize]
      private async Task<int> AddImagesToDatabase(List<string> imageURLs, int newEventId)
      {
         int rowsAfftected = 0;

         foreach (string url in imageURLs)
         {
            rowsAfftected += await this.db.AddEventImage(url, newEventId);
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