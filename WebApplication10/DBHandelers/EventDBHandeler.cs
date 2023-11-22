using Microsoft.IdentityModel.Tokens;
using MySql.Data.MySqlClient;
using Stilbaai_Tourism_Web_Portal.Classes;
using Stilbaai_Tourism_Web_Portal.Models;
using System.Collections.Concurrent;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Stilbaai_Tourism_Web_Portal.DBHandelers
{
   public class EventDBHandeler
   {
      private readonly ToolBoxSingleton _ToolBox = ToolBoxSingleton.Instance;
      private MySqlConnection connection;

      //---------------------------------------------------------------------------------------
      /// <summary>
      /// get all entries from db
      /// </summary>
      /// <returns></returns>
      public async Task GetEvent()
      {
         try
         {
            List<EventModel> newEntries = new List<EventModel>();

            using (var connection = new MySqlConnection(Properties.Resources.ResourceManager.GetString("ConnString")))
            {
               await connection.OpenAsync();

               string query = "SELECT * FROM `stil_app_db`.`Event_Table`;";

               using (var command = new MySqlCommand(query, connection))
               using (var reader = await command.ExecuteReaderAsync())
               {
                  while (await reader.ReadAsync())
                  {
                     var newEntry = new EventModel
                     {
                        EVENT_ID = reader.GetInt32(0),
                        EVENT_NAME = reader.GetString(1),
                        EVENT_NUM = reader.GetString(2),
                        EVENT_EMAIL = reader.GetString(3),
                        EVENT_WEBSITE = reader.GetString(4),
                        EVENT_ADDRESS = reader.GetString(5),
                        EVENT_PERSON = reader.GetString(6),
                        EVENT_DATE = reader.GetString(7),
                        EVENT_STARTTIME = reader.GetString(8),
                        EVENT_DURATION = reader.GetString(9),
                        EVENT_DESCRIPTION = reader.GetString(10)
                     };

                     newEntries.Add(newEntry);
                  }
               }
            }

            // Use a ConcurrentBag to ensure thread safety
            var concurrentBag = new ConcurrentBag<EventModel>(newEntries);
            _ToolBox.EventList = concurrentBag.ToList();
         }
         catch (MySqlException e)
         {
            System.Diagnostics.Trace.WriteLine(e.ToString());
         }
         catch (Exception e)
         {
            System.Diagnostics.Trace.WriteLine(e.ToString());
         }
      }

      //---------------------------------------------------------------------------------------
      /// <summary>
      /// get all images for an entry
      /// </summary>
      /// <param name="EventId"></param>
      /// <returns></returns>
      public async Task<List<string>> GetEventImages(int EventId)
      {
         try
         {
            using (var connection = new MySqlConnection(Properties.Resources.ResourceManager.GetString("ConnString")))
            {
               await connection.OpenAsync();

               List<string> urls = new List<string>();

               string query = $"SELECT * FROM `stil_app_db`.`Event_Image_Table` WHERE EVENT_ID = @ID;";

               using (var command = new MySqlCommand(query, connection))
               {
                  command.Parameters.AddWithValue("@ID", EventId);

                  using var reader = await command.ExecuteReaderAsync();

                  while (await reader.ReadAsync())
                  {
                     urls.Add(reader.GetString(1));
                  }

                  return urls;
               }
            }
         }
         catch (MySqlException e)
         {
            System.Diagnostics.Trace.WriteLine(e.ToString());
            return null;
         }
         catch (Exception e)
         {
            System.Diagnostics.Trace.WriteLine(e.ToString());
            return null;
         }
      }

      //---------------------------------------------------------------------------------------
      /// <summary>
      /// update entry 
      /// </summary>
      /// <param name="_event"></param>
      /// <returns></returns>
      public async Task<bool> UpdateEvent(EventModel _event)
      {
         try
         {
            using (connection = new MySqlConnection(Properties.Resources.ResourceManager.GetString("ConnString")))
            {
               await connection.OpenAsync();

               string query = "UPDATE `stil_app_db`.`Event_Table` SET " +
                           "`EVENT_NAME` = @EVENT_NAME, " +
                           "`EVENT_NUM` = @EVENT_NUM, " +
                           "`EVENT_EMAIL` = @EVENT_EMAIL, " +
                           "`EVENT_WEBSITE` = @EVENT_WEBSITE, " +
                           "`EVENT_ADDRESS` = @EVENT_ADDRESS, " +
                           "`EVENT_PERSON` = @EVENT_PERSON, " +
                           "`EVENT_DATE` = @EVENT_DATE, " +
                           "`EVENT_STARTTIME` = @EVENT_STARTTIME, " +
                           "`EVENT_DURATION` = @EVENT_DURATION, " +
                           "`EVENT_DESCRIPTION` = @EVENT_DESCRIPTION " +
                           "WHERE `EVENT_ID` = @EVENT_ID;";

               using (var command = new MySqlCommand(query, connection))
               {

                  command.Parameters.AddWithValue("@EVENT_NAME", string.IsNullOrEmpty(_event.EVENT_NAME) ? "" : _event.EVENT_NAME);
                  command.Parameters.AddWithValue("@EVENT_NUM", string.IsNullOrEmpty(_event.EVENT_NUM) ? "" : _event.EVENT_NUM);
                  command.Parameters.AddWithValue("@EVENT_EMAIL", string.IsNullOrEmpty(_event.EVENT_EMAIL) ? "" : _event.EVENT_EMAIL);
                  command.Parameters.AddWithValue("@EVENT_WEBSITE", string.IsNullOrEmpty(_event.EVENT_WEBSITE) ? "" : _event.EVENT_WEBSITE);
                  command.Parameters.AddWithValue("@EVENT_ADDRESS", string.IsNullOrEmpty(_event.EVENT_ADDRESS) ? "" : _event.EVENT_ADDRESS);
                  command.Parameters.AddWithValue("@EVENT_PERSON", string.IsNullOrEmpty(_event.EVENT_PERSON) ? "" : _event.EVENT_PERSON);
                  command.Parameters.AddWithValue("@EVENT_DATE", string.IsNullOrEmpty(_event.EVENT_DATE) ? "" : _event.EVENT_DATE);
                  command.Parameters.AddWithValue("@EVENT_STARTTIME", string.IsNullOrEmpty(_event.EVENT_STARTTIME) ? "" : _event.EVENT_STARTTIME);
                  command.Parameters.AddWithValue("@EVENT_DURATION", string.IsNullOrEmpty(_event.EVENT_DURATION) ? "" : _event.EVENT_DURATION);
                  command.Parameters.AddWithValue("@EVENT_DESCRIPTION", string.IsNullOrEmpty(_event.EVENT_DESCRIPTION) ? "" : _event.EVENT_DESCRIPTION);
                  command.Parameters.AddWithValue("@EVENT_ID", _event.EVENT_ID);

                  int rowsAffected = command.ExecuteNonQuery();

                  if (rowsAffected > 0)
                  {
                     Console.WriteLine("Update successful.");
                     return true;
                  }
                  else
                  {
                     Console.WriteLine("No rows were updated.");
                     return false;
                  }
               }


            }
         }
         catch (MySqlException e)
         {
            System.Diagnostics.Trace.WriteLine(e.ToString());
            return false;
         }
         catch (Exception e)
         {
            System.Diagnostics.Trace.WriteLine(e.ToString());
            return false;
         }
      }

      //---------------------------------------------------------------------------------------
      /// <summary>
      /// delete an entry
      /// </summary>
      /// <param name="EventId"></param>
      /// <returns></returns>
      public async Task<bool> DeleteEvent(int EventId)
      {
         try
         {
            using (connection = new MySqlConnection(Properties.Resources.ResourceManager.GetString("ConnString")))
            {
               await connection.OpenAsync();

               string query = "DELETE FROM `stil_app_db`.`Event_Table` WHERE EVENT_ID = @EVENT_ID";

               using (var command = new MySqlCommand(query, connection))
               {
                  command.Parameters.AddWithValue("@EVENT_ID", EventId);

                  int rowsAffected = command.ExecuteNonQuery();

                  if (rowsAffected > 0)
                  {
                     Console.WriteLine("Delete successful.");
                     return true;
                  }
                  else
                  {
                     Console.WriteLine("No rows were deleted.");
                     return false;
                  }
               }
            }
         }
         catch (MySqlException e)
         {
            System.Diagnostics.Trace.WriteLine(e.ToString());
            return false;
         }
         catch (Exception e)
         {
            System.Diagnostics.Trace.WriteLine(e.ToString());
            return false;
         }
      }

      //---------------------------------------------------------------------------------------
      /// <summary>
      /// add new entry
      /// </summary>
      /// <param name="_event"></param>
      /// <returns></returns>
      public async Task<int> AddEvent(EventModel _event)
      {
         try
         {
            using (connection = new MySqlConnection(Properties.Resources.ResourceManager.GetString("ConnString")))
            {
               await connection.OpenAsync();

               //default insert into SQL statement
               string query = "INSERT INTO `stil_app_db`.`Event_Table` " +
                   "(`EVENT_NAME`, `EVENT_NUM`, `EVENT_EMAIL`, `EVENT_WEBSITE`, `EVENT_ADDRESS`, `EVENT_PERSON`, `EVENT_DATE`, `EVENT_STARTTIME`, `EVENT_DURATION`, `EVENT_DESCRIPTION`) " +
                   "VALUES (@NAME, @NUM, @EMAIL, @WEBSITE, @ADDRESS, @PERSON, @DATE, @TIME, @DURATION, @DESCRIPTION);";

               using (var command = new MySqlCommand(query, connection))
               {
                  //command.Parameters.AddWithValue is a way to paramiterise the SQL, avoiding a SQL injection attack
                  command.Parameters.AddWithValue("@NAME", string.IsNullOrEmpty(_event.EVENT_NAME) ? "" : _event.EVENT_NAME);
                  command.Parameters.AddWithValue("@NUM", string.IsNullOrEmpty(_event.EVENT_NUM) ? "" : _event.EVENT_NUM);
                  command.Parameters.AddWithValue("@EMAIL", string.IsNullOrEmpty(_event.EVENT_EMAIL) ? "" : _event.EVENT_EMAIL);
                  command.Parameters.AddWithValue("@WEBSITE", string.IsNullOrEmpty(_event.EVENT_WEBSITE) ? "" : _event.EVENT_WEBSITE);
                  command.Parameters.AddWithValue("@ADDRESS", string.IsNullOrEmpty(_event.EVENT_ADDRESS) ? "" : _event.EVENT_ADDRESS);
                  command.Parameters.AddWithValue("@PERSON", string.IsNullOrEmpty(_event.EVENT_PERSON) ? "" : _event.EVENT_PERSON);
                  command.Parameters.AddWithValue("@DATE", string.IsNullOrEmpty(_event.EVENT_DATE) ? "" : _event.EVENT_DATE);
                  command.Parameters.AddWithValue("@TIME", string.IsNullOrEmpty(_event.EVENT_STARTTIME) ? "" : _event.EVENT_STARTTIME);
                  command.Parameters.AddWithValue("@DURATION", string.IsNullOrEmpty(_event.EVENT_DURATION) ? "" : _event.EVENT_DURATION);
                  command.Parameters.AddWithValue("@DESCRIPTION", string.IsNullOrEmpty(_event.EVENT_DESCRIPTION) ? "" : _event.EVENT_DESCRIPTION);

                  // Execute the query
                  int rowsAffected = command.ExecuteNonQuery();

                  if (rowsAffected > 0)
                  {
                     // Get the ID of the last inserted record
                     command.CommandText = "SELECT LAST_INSERT_ID();";
                     int lastInsertedId = Convert.ToInt32(command.ExecuteScalar());

                     return lastInsertedId;
                  }
                  else
                  {
                     return -1;
                  }
               }
            }
         }
         catch (MySqlException e)
         {
            System.Diagnostics.Trace.WriteLine(e.ToString());
            return -1;
         }
         catch (Exception e)
         {
            System.Diagnostics.Trace.WriteLine(e.ToString());
            return -1;
         }
      }

      //---------------------------------------------------------------------------------------
      /// <summary>
      /// add images for an entry
      /// </summary>
      /// <param name="url"></param>
      /// <param name="EventId"></param>
      /// <returns></returns>
      public async Task<int> AddEventImage(string url, int EventId)
      {
         try
         {
            using (connection = new MySqlConnection(Properties.Resources.ResourceManager.GetString("ConnString")))
            {
               await connection.OpenAsync();

               //default insert into SQL statement
               string query = "INSERT INTO `stil_app_db`.`Event_Image_Table` (`EVENT_IMAGE_URL`, `EVENT_ID`) VALUES (@URL, @ID);";

               using (var command = new MySqlCommand(query, connection))
               {
                  //command.Parameters.AddWithValue is a way to paramiterise the SQL, avoiding a SQL injection attack
                  command.Parameters.AddWithValue("@URL", string.IsNullOrEmpty(url) ? "" : url);
                  command.Parameters.AddWithValue("@ID", EventId.ToString());

                  int rowsAffected = command.ExecuteNonQuery();

                  return rowsAffected;
               }
            }
         }
         catch (MySqlException e)
         {
            System.Diagnostics.Trace.WriteLine(e.ToString());
            return 0;
         }
         catch (Exception e)
         {
            System.Diagnostics.Trace.WriteLine(e.ToString());
            return 0;
         }
      }

      //---------------------------------------------------------------------------------------
      /// <summary>
      /// delete an entries image
      /// </summary>
      /// <param name="EventId"></param>
      /// <param name="url"></param>
      /// <returns></returns>
      public async Task<bool> DeleteImage(int EventId, string url)
      {
         try
         {
            using (connection = new MySqlConnection(Properties.Resources.ResourceManager.GetString("ConnString")))
            {
               await connection.OpenAsync();

               string query = "DELETE FROM `stil_app_db`.`Event_Image_Table` WHERE EVENT_IMAGE_URL = @URL AND EVENT_ID = @EVENTID;";

               using (var command = new MySqlCommand(query, connection))
               {
                  command.Parameters.AddWithValue("@URL", url);
                  command.Parameters.AddWithValue("@EVENTID", EventId);

                  int rowsAffected = command.ExecuteNonQuery();

                  if (rowsAffected > 0)
                  {
                     Console.WriteLine("Delete successful.");
                     return true;
                  }
                  else
                  {
                     Console.WriteLine("No rows were deleted.");
                     return false;
                  }
               }
            }
         }
         catch (MySqlException e)
         {
            System.Diagnostics.Trace.WriteLine(e.ToString());
            return false;
         }
         catch (Exception e)
         {
            System.Diagnostics.Trace.WriteLine(e.ToString());
            return false;
         }
      }
   }
}
//-------------------------------------====END OF FILE====-------------------------------------