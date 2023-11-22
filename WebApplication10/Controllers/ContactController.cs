using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stilbaai_Tourism_Web_Portal.Classes;
using Stilbaai_Tourism_Web_Portal.Models;
using Stilbaai_Tourism_Web_Portal.Workers;
using System.Diagnostics;

namespace Stilbaai_Tourism_Web_Portal.Controllers
{
   public class ContactController : Controller
   {
      private readonly ILogger<ContactController> _logger;
      private readonly ToolBoxSingleton _ToolBox = ToolBoxSingleton.Instance;
      private readonly ContactDBHandeler db = new ContactDBHandeler();

      //---------------------------------------------------------------------------------------
      /// <summary>
      /// default constructor
      /// </summary>
      /// <param name="logger"></param>
      public ContactController(ILogger<ContactController> logger)
      {
         _logger = logger;
      }

      //==========VIEWS==========

      #region Views
      //---------------------------------------------------------------------------------------
      /// <summary>
      /// open edit contact view, allows for the editing of a selected view
      /// </summary>
      /// <param name="id"></param>
      /// <returns></returns>
      [Authorize]
      public IActionResult EditContact(int id)
      {
         try
         {
            // Use LINQ to find the restaurant with the specified ID
            var selectedContact = _ToolBox.ContactList.FirstOrDefault(r => r.CONTACT_ID == id);

            if (selectedContact == null)
            {
               return NotFound();
            }

            return View(selectedContact);
         }
         catch (Exception ex)
         {
            // Handle the exception, log it, and provide a user-friendly error message.
            _logger.LogError(ex, "An error occurred in EditContacts action.");
            return View("Error");
         }
      }

      //---------------------------------------------------------------------------------------
      /// <summary>
      /// open view contacts view, allows for the viewing of all contacts
      /// </summary>
      /// <returns></returns>
      [Authorize]
      public async Task<IActionResult> ViewContacts()
      {
         await this.db.GetContact();

         ViewBag.Contacts = _ToolBox.ContactList;
         return View();
      }

      //---------------------------------------------------------------------------------------
      /// <summary>
      /// open add new contact view
      /// </summary>
      /// <returns></returns>
      [Authorize]
      public IActionResult AddNewcontact()
      {
         return View();
      }

      #endregion

      //==========POSTBACKS==========

      //---------------------------------------------------------------------------------------
      /// <summary>
      /// save contact post back, handles the updating of the model
      /// </summary>
      /// <param name="updatedContact"></param>
      /// <returns></returns>
      [Authorize]
      [HttpPost]
      public async Task<IActionResult> SaveContact(ContactModel updatedContact)
      {
         try
         {
            if (!ModelState.IsValid)
            {
               return Json(new { success = false, message = "Invalid model data or Contact ID." });
            }

            bool updateResult = await this.db.UpdateContact(updatedContact);

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
            _logger.LogError(ex, "An error occurred while updating a contact.");
            return Json(new { success = false, message = "An error occurred while updating the contact." });
         }
      }

      //---------------------------------------------------------------------------------------
      /// <summary>
      /// delete entry postback
      /// </summary>
      /// <param name="CONTACT_ID"></param>
      /// <returns></returns>
      [Authorize]
      [HttpPost]
      public async Task<IActionResult> DeleteContact(int CONTACT_ID)
      {
         try
         {
            if (CONTACT_ID <= 0)
            {
               return Json(new { success = false, message = "Invalid CONTACT_ID provided." });
            }

            bool deletionResult = await this.db.DeleteContacr(CONTACT_ID);

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
            _logger.LogError(ex, "An error occurred while deleting the contact.");
            return Json(new { success = false, message = "Internal server error. Please try again later." });
         }
      }

      //---------------------------------------------------------------------------------------
      /// <summary>
      /// method to add a new entry
      /// </summary>
      /// <param name="newContact"></param>
      /// <returns></returns>
      [Authorize]
      [HttpPost]
      public async Task<IActionResult> AddContact(ContactModel newContact)
      {
         try
         {
            if (!ModelState.IsValid)
            {
               return Json(new { success = false, message = "Invalid model data or Contact ID." });
            }

            int newContactId = await AddContactToDatabase(newContact);

            if (newContactId != -1)
            {
               return Json(new { success = true });
            }
            else
            {
               return Json(new { success = false, message = "Error while saving your data." });
            }
         }
         catch (Exception ex)
         {
            _logger.LogError(ex, "An error occurred while adding images.");
            return Json(new { success = false, message = "Internal server error. Please try again later." });
         }
      }

      //---------------------------------------------------------------------------------------
      /// <summary>
      /// add the new contact to the db
      /// </summary>
      /// <param name="newContact"></param>
      /// <returns></returns>
      [Authorize]
      private async Task<int> AddContactToDatabase(ContactModel newContact)
      {
         return await this.db.AddContact(newContact);
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
