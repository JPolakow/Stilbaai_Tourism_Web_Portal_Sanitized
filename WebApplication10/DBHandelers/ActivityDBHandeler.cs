using Microsoft.IdentityModel.Tokens;
using MySql.Data.MySqlClient;
using Stilbaai_Tourism_Web_Portal.Classes;
using Stilbaai_Tourism_Web_Portal.Models;
using System.Collections.Concurrent;
using System.Data;

namespace Stilbaai_Tourism_Web_Portal.Workers
{
   public class ActivityDBHandeler
   {
      private ToolBoxSingleton _ToolBox = ToolBoxSingleton.Instance;
      private MySqlConnection connection;

      //---------------------------------------------------------------------------------------
      //
      public async Task GetActivity()
      {
         try
         {
            List<ActivityModel> newEntries = new List<ActivityModel>();

            using (var connection = new MySqlConnection(Properties.Resources.ResourceManager.GetString("ConnString")))
            {
               await connection.OpenAsync();

               string query = "SELECT * FROM `stil_app_db`.`Activity_Table`;";

               using (var command = new MySqlCommand(query, connection))
               using (var reader = await command.ExecuteReaderAsync())
               {
                  while (await reader.ReadAsync())
                  {
                     var newEntry = new ActivityModel
                     {
                        ACTIVITY_ID = reader.GetInt32(0),
                        ACTIVITY_NAME = reader.GetString(1),
                        ACTIVITY_TEL_NUM = reader.GetString(2),
                        ACTIVITY_MOBILE_NUM = reader.GetString(3),
                        ACTIVITY_EMAIL = reader.GetString(4),
                        ACTIVITY_WEBSITE = reader.GetString(5),
                        ACTIVITY_ADDRESS = reader.GetString(6),
                        ACTIVITY_CONTACT_PERSON = reader.GetString(7),
                        ACTIVITY_DESCRIPTION = reader.GetString(8),
                        ACTIVITY_CATEGORY_ID = reader.GetInt32(9)
                     };

                     newEntries.Add(newEntry);
                  }
               }
            }

            // Use a ConcurrentBag to ensure thread safety
            var concurrentBag = new ConcurrentBag<ActivityModel>(newEntries);
            _ToolBox.ActivityList = concurrentBag.ToList();
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
      //
      public async Task<List<string>> GetActivityImages(int ActivityId)
      {
         try
         {
            using (connection = new MySqlConnection(Properties.Resources.ResourceManager.GetString("ConnString")))
            {

               List<string> urls = new List<string>();

               string query = $"SELECT * FROM `stil_app_db`.`Activity_Image_Table` WHERE ACTIVITY_ID = @ID;";

               using (var command = new MySqlCommand(query, connection))
               {
                  command.Parameters.AddWithValue("@ID", ActivityId);

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
      //
      public async Task<bool> UpdateActivity(ActivityModel activity)
      {
         try
         {
            using (connection = new MySqlConnection(Properties.Resources.ResourceManager.GetString("ConnString")))
            {
               await connection.OpenAsync();

               string query = "UPDATE `stil_app_db`.`Activity_Table` SET " +
                           "`ACTIVITY_NAME` = @NAME, " +
                           "`ACTIVITY_TEL_NUM` = @TEL_NUM, " +
                           "`ACTIVITY_MOBILE_NUM` = @MOBILE_NUM, " +
                           "`ACTIVITY_EMAIL` = @EMAIL, " +
                           "`ACTIVITY_WEBSITE` = @WEBSITE, " +
                           "`ACTIVITY_ADDRESS` = @ADDRESS, " +
                           "`ACTIVITY_CONTACT_PERSON` = @CONTACT_PERSON, " +
                           "`ACTIVITY_DESCRIPTION` = @DESCRIPTION, " +
                           "`ACTIVITY_CATEGORY_ID` = @CATEGORY " +
                           "WHERE `ACTIVITY_ID` = @ID;";

               using (var command = new MySqlCommand(query, connection))
               {
                  command.Parameters.AddWithValue("@NAME", string.IsNullOrEmpty(activity.ACTIVITY_NAME) ? "" : activity.ACTIVITY_NAME);
                  command.Parameters.AddWithValue("@TEL_NUM", string.IsNullOrEmpty(activity.ACTIVITY_TEL_NUM) ? "" : activity.ACTIVITY_TEL_NUM);
                  command.Parameters.AddWithValue("@MOBILE_NUM", string.IsNullOrEmpty(activity.ACTIVITY_MOBILE_NUM) ? "" : activity.ACTIVITY_MOBILE_NUM);
                  command.Parameters.AddWithValue("@EMAIL", string.IsNullOrEmpty(activity.ACTIVITY_EMAIL) ? "" : activity.ACTIVITY_EMAIL);
                  command.Parameters.AddWithValue("@WEBSITE", string.IsNullOrEmpty(activity.ACTIVITY_WEBSITE) ? "" : activity.ACTIVITY_WEBSITE);
                  command.Parameters.AddWithValue("@ADDRESS", string.IsNullOrEmpty(activity.ACTIVITY_ADDRESS) ? "" : activity.ACTIVITY_ADDRESS);
                  command.Parameters.AddWithValue("@CONTACT_PERSON", string.IsNullOrEmpty(activity.ACTIVITY_CONTACT_PERSON) ? "" : activity.ACTIVITY_CONTACT_PERSON);
                  command.Parameters.AddWithValue("@DESCRIPTION", string.IsNullOrEmpty(activity.ACTIVITY_DESCRIPTION) ? "" : activity.ACTIVITY_DESCRIPTION);
                  command.Parameters.AddWithValue("@CATEGORY", !int.IsPositive(activity.ACTIVITY_CATEGORY_ID) ? "" : activity.ACTIVITY_CATEGORY_ID);
                  command.Parameters.AddWithValue("@ID", activity.ACTIVITY_ID);

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
      //
      public async Task<bool> DeleteActivity(int ACTIVITY_ID)
      {
         try
         {
            using (connection = new MySqlConnection(Properties.Resources.ResourceManager.GetString("ConnString")))
            {
               await connection.OpenAsync();

               string query = "DELETE FROM `stil_app_db`.`Activity_Table` WHERE ACTIVITY_ID = @ID";

               using (var command = new MySqlCommand(query, connection))
               {
                  command.Parameters.AddWithValue("@ID", ACTIVITY_ID);

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
      //
      public async Task<int> AddActivity(ActivityModel activity)
      {
         try
         {
            using (connection = new MySqlConnection(Properties.Resources.ResourceManager.GetString("ConnString")))
            {
               await connection.OpenAsync();

               //default insert into SQL statement
               string query = "INSERT INTO `stil_app_db`.`Activity_Table` " +
                   "(`ACTIVITY_NAME`, `ACTIVITY_TEL_NUM`, `ACTIVITY_MOBILE_NUM`, `ACTIVITY_EMAIL`, `ACTIVITY_WEBSITE`, `ACTIVITY_ADDRESS`, `ACTIVITY_CONTACT_PERSON`, `ACTIVITY_DESCRIPTION`, `ACTIVITY_CATEGORY_ID`) " +
                   "VALUES (@NAME, @TEL_NUM, @MOBILE_NUM, @EMAIL, @WEBSITE, @ADDRESS, @CONTACT_PERSON, @DESCRIPTION, @CATEGORY_ID);";

               using (var command = new MySqlCommand(query, connection))
               {
                  //command.Parameters.AddWithValue is a way to paramiterise the SQL, avoiding a SQL injection attack
                  command.Parameters.AddWithValue("@NAME", string.IsNullOrEmpty(activity.ACTIVITY_NAME) ? "" : activity.ACTIVITY_NAME);
                  command.Parameters.AddWithValue("@TEL_NUM", string.IsNullOrEmpty(activity.ACTIVITY_TEL_NUM) ? "" : activity.ACTIVITY_TEL_NUM);
                  command.Parameters.AddWithValue("@MOBILE_NUM", string.IsNullOrEmpty(activity.ACTIVITY_MOBILE_NUM) ? "" : activity.ACTIVITY_MOBILE_NUM);
                  command.Parameters.AddWithValue("@EMAIL", string.IsNullOrEmpty(activity.ACTIVITY_NAME) ? "" : activity.ACTIVITY_NAME);
                  command.Parameters.AddWithValue("@WEBSITE", string.IsNullOrEmpty(activity.ACTIVITY_NAME) ? "" : activity.ACTIVITY_NAME);
                  command.Parameters.AddWithValue("@ADDRESS", string.IsNullOrEmpty(activity.ACTIVITY_NAME) ? "" : activity.ACTIVITY_NAME);
                  command.Parameters.AddWithValue("@CONTACT_PERSON", string.IsNullOrEmpty(activity.ACTIVITY_NAME) ? "" : activity.ACTIVITY_NAME);
                  command.Parameters.AddWithValue("@DESCRIPTION", string.IsNullOrEmpty(activity.ACTIVITY_NAME) ? "" : activity.ACTIVITY_NAME);
                  command.Parameters.AddWithValue("@CATEGORY_ID", !int.IsPositive(activity.ACTIVITY_CATEGORY_ID) ? "" : activity.ACTIVITY_CATEGORY_ID);

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
      //
      public async Task<int> AddActivityImage(string url, int id)
      {
         try
         {
            using (connection = new MySqlConnection(Properties.Resources.ResourceManager.GetString("ConnString")))
            {
               await connection.OpenAsync();

               //default insert into SQL statement
               string query = "INSERT INTO `stil_app_db`.`Activity_Image_Table` (`ACTIVITY_IMAGE_URL`, `ACTIVITY_ID`) VALUES (@URL, @ID);";

               using (var command = new MySqlCommand(query, connection))
               {
                  //command.Parameters.AddWithValue is a way to paramiterise the SQL, avoiding a SQL injection attack
                  command.Parameters.AddWithValue("@URL", string.IsNullOrEmpty(url) ? "" : url);
                  command.Parameters.AddWithValue("@ID", id.ToString());

                  int rowsAffected = command.ExecuteNonQuery();

                  return rowsAffected;
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
      //
      public async Task<bool> DeleteImage(int id, string url)
      {
         try
         {
            using (connection = new MySqlConnection(Properties.Resources.ResourceManager.GetString("ConnString")))
            {
               await connection.OpenAsync();

               string query = "DELETE FROM `stil_app_db`.`Activity_Image_Table` WHERE ACTIVITY_IMAGE_URL = @URL AND ACTIVITY_ID = @ID;";

               using (var command = new MySqlCommand(query, connection))
               {
                  command.Parameters.AddWithValue("@URL", url);
                  command.Parameters.AddWithValue("@ID", id);

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
      //
      public async Task GetActivityCategory()
      {
         try
         {
            List<ActivityCategoryModel> newEntries = new List<ActivityCategoryModel>();

            using (var connection = new MySqlConnection(Properties.Resources.ResourceManager.GetString("ConnString")))
            {
               await connection.OpenAsync();

               string query = "SELECT * FROM stil_app_db.Activity_Category_Table;";

               using (var command = new MySqlCommand(query, connection))
               using (var reader = await command.ExecuteReaderAsync())
               {
                  while (await reader.ReadAsync())
                  {
                     var newEntry = new ActivityCategoryModel
                     {
                        ACTIVITY_CATEGORY_ID = reader.GetInt32(0),
                        ACTIVITY_CATEGORY_TYPE = reader.GetString(1)
                     };

                     newEntries.Add(newEntry);
                  }
               }
            }

            // Use a ConcurrentBag to ensure thread safety
            var concurrentBag = new ConcurrentBag<ActivityCategoryModel>(newEntries);
            _ToolBox.ActivityCategoryList = concurrentBag.ToList();
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
   }
}
//-------------------------------------====END OF FILE====-------------------------------------