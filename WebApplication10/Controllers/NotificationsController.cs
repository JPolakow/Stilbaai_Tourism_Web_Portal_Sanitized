using FirebaseAdmin.Messaging;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stilbaai_Tourism_Web_Portal.Models;
using Stilbaai_Tourism_Web_Portal.Properties;

namespace Stilbaai_Tourism_Web_Portal.Controllers
{
   public class NotificationsController : Controller
   {
      private readonly ILogger<NotificationsController> _logger;

      //---------------------------------------------------------------------------------------
      //default constructor
      public NotificationsController(ILogger<NotificationsController> logger)
      {
         _logger = logger;
      }

      //---------------------------------------------------------------------------------------
      //open view 
      [Authorize]
      public async Task<IActionResult> Notifications()
      {
         return View();
      }

      //---------------------------------------------------------------------------------------
      //recieves the data from the view
      [Authorize]
      [HttpPost]
      public async Task<IActionResult> SendNotification(string NOTIFICATION_CONTENT)
      {
         try
         {
            bool result = SendNotificationToFirebase(NOTIFICATION_CONTENT);

            if (!result)
            {
               return Json(new { success = false, message = "The notification could not send, please try again." });
            }

            return Json(new { success = true });
         }
         catch (Exception ex)
         {
            _logger.LogError(ex, "An error occurred while updating an eel.");
            return Json(new { success = false, message = "An error occurred while sending the notification." });
         }
      }

      //---------------------------------------------------------------------------------------
      //send notification to firebase
      [Authorize]
      private bool SendNotificationToFirebase(string content)
      {
         try
         {
            var firebaseApp = FirebaseApp.DefaultInstance;

            if (firebaseApp == null)
            {
               FirebaseApp.Create(new AppOptions()
               {
                  Credential = GoogleCredential.FromStream(new MemoryStream(Resources.firebase_admin))
               });
            }

            // See documentation on defining a message payload.
            var message = new Message()
            {
               Topic = "Notification",
               Notification = new Notification()
               {
                  Title = "StilBaai Tourism",
                  Body = content
               }
            };

            // Send a message to the device corresponding to the provided
            // registration token.
            string response = FirebaseMessaging.DefaultInstance.SendAsync(message).Result;

            // Response is a message ID string.
            Console.WriteLine("Successfully sent message: " + response);

            // Return true to indicate success
            return true;
         }
         catch (Exception ex)
         {
            // Log or handle the exception as needed
            Console.WriteLine("Failed to send message: " + ex.Message);

            // Return false to indicate failure
            return false;
         }
      }
   }
}
