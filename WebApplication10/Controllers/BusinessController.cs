using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stilbaai_Tourism_Web_Portal.Classes;
using Stilbaai_Tourism_Web_Portal.DBHandelers;
using Stilbaai_Tourism_Web_Portal.Models;
using System.Diagnostics;

namespace Stilbaai_Tourism_Web_Portal.Controllers
{
   public class BusinessController : Controller
   {
      private readonly ILogger<BusinessController> _logger;
      private readonly ToolBoxSingleton _ToolBox = ToolBoxSingleton.Instance;
      private readonly BusinessDBHandeler db = new BusinessDBHandeler();

      //---------------------------------------------------------------------------------------
      /// <summary>
      /// default constructor
      /// </summary>
      /// <param name="logger"></param>
      public BusinessController(ILogger<BusinessController> logger)
      {
         _logger = logger;
      }

      //==========VIEWS==========

      #region Views

      //---------------------------------------------------------------------------------------
      /// <summary>
      /// open edit businesss view, allows for the editing of a selected view
      /// </summary>
      /// <param name="id"></param>
      /// <returns></returns>
      [Authorize]
      public IActionResult EditBusiness(int id)
      {
         try
         {
            // Use LINQ to find the restaurant with the specified ID
            var selectedBusiness = _ToolBox.BusinessList.FirstOrDefault(r => r.BUSINESS_ID == id);

            if (selectedBusiness == null)
            {
               return NotFound();
            }

            List<string> Categories = new List<string>();

            for (int i = 0; i != _ToolBox.BusinessCategoryList.Count; i++)
            {
               Categories.Add(_ToolBox.BusinessCategoryList[i].BUSINESS_CATEGORY_TYPE);
            }
            ViewBag.Categories = Categories;

            int SelectedCategoryID = _ToolBox.BusinessList.FirstOrDefault(x => x.BUSINESS_ID == id).BUSINESS_CATEGORY_ID;
            if (SelectedCategoryID != 0)
            {
               string SelectedCategory = _ToolBox.BusinessCategoryList.FirstOrDefault(x => x.BUSINESS_CATEGORY_ID == SelectedCategoryID).BUSINESS_CATEGORY_TYPE;
               ViewBag.SelectedCategory = SelectedCategory;
            }

            return View(selectedBusiness);
         }
         catch (Exception ex)
         {
            // Handle the exception, log it, and provide a user-friendly error message.
            _logger.LogError(ex, "An error occurred in EditBusinesss action.");
            return View("Error");
         }
      }

      //---------------------------------------------------------------------------------------
      /// <summary>
      /// open view businesss view, allows for the viewing of all businesss
      /// </summary>
      /// <returns></returns>
      [Authorize]
      public async Task<IActionResult> ViewBusiness()
      {
         await db.GetBusinessCategory();
         await this.db.GetBusiness();

         ViewBag.Business = _ToolBox.BusinessList;
         return View();
      }

      //---------------------------------------------------------------------------------------
      /// <summary>
      /// open add new business view
      /// </summary>
      /// <returns></returns>
      [Authorize]
      public IActionResult AddNewBusiness()
      {

         List<string> Categories = new List<string>();
         for (int i = 0; i != _ToolBox.BusinessCategoryList.Count; i++)
         {
            Categories.Add(_ToolBox.BusinessCategoryList[i].BUSINESS_CATEGORY_TYPE);
         }
         ViewBag.Categories = Categories;

         return View();
      }

      //---------------------------------------------------------------------------------------
      /// <summary>
      /// open view business images view
      /// </summary>
      /// <param name="id"></param>
      /// <returns></returns>
      [Authorize]
      public async Task<IActionResult> ViewBusinessImages(int id)
      {
         var selectedBusiness = _ToolBox.BusinessList.FirstOrDefault(r => r.BUSINESS_ID == id);

         List<string> urls;
         urls = await this.db.GetBusinessImages(selectedBusiness.BUSINESS_ID);

         if (urls != null)
         {
            ViewBag.imageUrls = urls;
         }
         else
         {
            return NotFound();
         }

         return View(selectedBusiness);
      }

      #endregion

      //==========POSTBACKS==========

      //---------------------------------------------------------------------------------------
      /// <summary>
      /// post back to delte an image
      /// </summary>
      /// <param name="businessId"></param>
      /// <param name="imageUrl"></param>
      /// <returns></returns>
      [Authorize]
      [HttpPost]
      public async Task<IActionResult> DeleteImage(int businessId, string imageUrl)
      {
         try
         {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
               return Json(new { success = false, message = "Invalid image URL provided." });
            }

            bool deletionResult = await this.db.DeleteImage(businessId, imageUrl);

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
      /// save business post back, handles the updating of the model
      /// </summary>
      /// <param name="updatedBusiness"></param>
      /// <returns></returns>
      [Authorize]
      [HttpPost]
      public async Task<IActionResult> SaveBusiness(BusinessModel updatedBusiness)
      {
         try
         {
            if (!ModelState.IsValid)
            {
               return Json(new { success = false, message = "Invalid model data or Business ID." });
            }

            updatedBusiness.BUSINESS_CATEGORY_ID = _ToolBox.BusinessCategoryList.FirstOrDefault(x => x.BUSINESS_CATEGORY_TYPE == updatedBusiness.BUSINESS_CATEGORY_NAME).BUSINESS_CATEGORY_ID;

            bool updateResult = await this.db.UpdateBusiness(updatedBusiness);

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
            _logger.LogError(ex, "An error occurred while updating an business.");
            return Json(new { success = false, message = "An error occurred while updating the business." });
         }
      }

      //---------------------------------------------------------------------------------------
      /// <summary>
      /// delete entry postback
      /// </summary>
      /// <param name="BUSINESS_ID"></param>
      /// <returns></returns>
      [Authorize]
      [HttpPost]
      public async Task<IActionResult> DeleteBusiness(int BUSINESS_ID)
      {
         try
         {
            if (BUSINESS_ID <= 0)
            {
               return Json(new { success = false, message = "Invalid BUSINESS_ID provided." });
            }

            //check no images are associated in the db
            List<string> urls;
            urls = await this.db.GetBusinessImages(BUSINESS_ID);

            if (urls.Count != 0)
            {
               return Json(new { success = false, message = "This entry has images, please remove them first." });
            }

            bool deletionResult = await this.db.DeleteBusiness(BUSINESS_ID);

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
            _logger.LogError(ex, "An error occurred while deleting the business.");
            return Json(new { success = false, message = "Internal server error. Please try again later." });
         }
      }

      //---------------------------------------------------------------------------------------
      /// <summary>
      /// method to add a new entry, and the images to storage
      /// </summary>
      /// <param name="newBusiness"></param>
      /// <param name="imageFiles"></param>
      /// <returns></returns>
      [Authorize]
      [HttpPost]
      public async Task<IActionResult> AddBusiness(BusinessModel newBusiness, List<IFormFile> imageFiles)
      {
         try
         {
            if (!ModelState.IsValid)
            {
               return Json(new { success = false, message = "Invalid model data or Business ID." });
            }

            if (imageFiles == null || !imageFiles.Any())
            {
               return Json(new { success = false, message = "No images provided." });
            }

            newBusiness.BUSINESS_CATEGORY_ID = _ToolBox.BusinessCategoryList.FirstOrDefault(x => x.BUSINESS_CATEGORY_TYPE == newBusiness.BUSINESS_CATEGORY_NAME).BUSINESS_CATEGORY_ID;

            int newBusinessId = await AddBusinessToDatabase(newBusiness);

            if (newBusinessId == -1)
            {
               return Json(new { success = false, message = "Error while saving the business data." });
            }

            List<string> imageURLs = await ProcessAndSaveImages(imageFiles);

            if (imageURLs == null || imageURLs.Count == 0)
            {
               return Json(new { success = false, message = "Error while saving images." });
            }

            int results = await AddImagesToDatabase(imageURLs, newBusinessId);

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
      /// <param name="businessId"></param>
      /// <returns></returns>
      [Authorize]
      [HttpPost]
      public async Task<IActionResult> AddImages(List<IFormFile> imageFiles, int businessId)
      {
         try
         {
            if (!ModelState.IsValid || businessId <= 0)
            {
               return Json(new { success = false, message = "Invalid model data or Business ID." });
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

            int results = await AddImagesToDatabase(imageURLs, businessId);

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
      /// add the new business to the db
      /// </summary>
      /// <param name="newBusiness"></param>
      /// <returns></returns>
      [Authorize]
      private async Task<int> AddBusinessToDatabase(BusinessModel newBusiness)
      {
         return await this.db.AddBusiness(newBusiness);
      }

      //---------------------------------------------------------------------------------------
      /// <summary>
      /// add the image url/s to the db
      /// </summary>
      /// <param name="imageURLs"></param>
      /// <param name="newBusinessId"></param>
      /// <returns></returns>
      [Authorize]
      private async Task<int> AddImagesToDatabase(List<string> imageURLs, int newBusinessId)
      {
         int rowsAfftected = 0;

         foreach (string url in imageURLs)
         {
            rowsAfftected += await this.db.AddBusinessImage(url, newBusinessId);
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