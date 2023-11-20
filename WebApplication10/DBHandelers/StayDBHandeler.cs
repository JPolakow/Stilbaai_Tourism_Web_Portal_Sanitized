using MySql.Data.MySqlClient;
using Stilbaai_Tourism_Web_Portal.Classes;
using Stilbaai_Tourism_Web_Portal.Models;
using System.Collections.Concurrent;
using System.Security.Policy;

namespace Stilbaai_Tourism_Web_Portal.DBHandelers
{
   public class StayDBHandeler
   {
      private ToolBoxSingleton _ToolBox = ToolBoxSingleton.Instance;
      private MySqlConnection connection;

      //---------------------------------------------------------------------------------------
      //
      public async Task GetStay()
      {
         try
         {
            List<StayModel> newEntries = new List<StayModel>();

            using (var connection = new MySqlConnection(Properties.Resources.ResourceManager.GetString("ConnString")))
            {
               await connection.OpenAsync();

               string query = "SELECT * FROM `stil_app_db`.`Stay_Table`;";

               using (var command = new MySqlCommand(query, connection))
               using (var reader = await command.ExecuteReaderAsync())
               {
                  while (await reader.ReadAsync())
                  {
                     var newEntry = new StayModel
                     {
                        STAY_ID = reader.GetInt32(0),
                        STAY_NAME = reader.GetString(1),
                        STAY_TEL_NUM = reader.GetString(2),
                        STAY_MOBILE_NUM = reader.GetString(3),
                        STAY_EMAIL = reader.GetString(4),
                        STAY_WEBSITE = reader.GetString(5),
                        STAY_ADDRESS = reader.GetString(6),
                        STAY_CONTACT_PERSON = reader.GetString(7),
                        STAY_DESCRIPTION = reader.GetString(8),
                        STAY_CATEGORY_ID = reader.GetInt32(9)
                     };

                     newEntries.Add(newEntry);
                  }
               }
            }

            // Use a ConcurrentBag to ensure thread safety
            var concurrentBag = new ConcurrentBag<StayModel>(newEntries);
            _ToolBox.StayList = concurrentBag.ToList();
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
      public async Task<List<string>> GetStayImages(int StayId)
      {
         try
         {
            using (connection = new MySqlConnection(Properties.Resources.ResourceManager.GetString("ConnString")))
            {

               List<string> urls = new List<string>();

               string query = $"SELECT * FROM `stil_app_db`.`Stay_Image_Table` WHERE STAY_ID = @ID;";

               using (var command = new MySqlCommand(query, connection))
               {
                  command.Parameters.AddWithValue("@ID", StayId);

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
      public async Task<bool> UpdateStay(StayModel stay)
      {
         try
         {
            using (connection = new MySqlConnection(Properties.Resources.ResourceManager.GetString("ConnString")))
            {
               await connection.OpenAsync();

               string query = "UPDATE `stil_app_db`.`Stay_Table` SET " +
                           "`STAY_NAME` = @NAME, " +
                           "`STAY_TEL_NUM` = @TEL_NUM, " +
                           "`STAY_MOBILE_NUM` = @MOBILE_NUM, " +
                           "`STAY_EMAIL` = @EMAIL, " +
                           "`STAY_WEBSITE` = @WEBSITE, " +
                           "`STAY_ADDRESS` = @ADDRESS, " +
                           "`STAY_CONTACT_PERSON` = @CONTACT_PERSON, " +
                           "`STAY_DESCRIPTION` = @DESCRIPTION, " +
                           "`STAY_CATEGORY_ID` = @STAY_CATEGORY_ID " +
                           "WHERE `STAY_ID` = @ID;";

               using (var command = new MySqlCommand(query, connection))
               {
                  command.Parameters.AddWithValue("@NAME", string.IsNullOrEmpty(stay.STAY_NAME) ? "" : stay.STAY_NAME);
                  command.Parameters.AddWithValue("@TEL_NUM", string.IsNullOrEmpty(stay.STAY_TEL_NUM) ? "" : stay.STAY_TEL_NUM);
                  command.Parameters.AddWithValue("@MOBILE_NUM", string.IsNullOrEmpty(stay.STAY_MOBILE_NUM) ? "" : stay.STAY_MOBILE_NUM);
                  command.Parameters.AddWithValue("@EMAIL", string.IsNullOrEmpty(stay.STAY_EMAIL) ? "" : stay.STAY_EMAIL);
                  command.Parameters.AddWithValue("@WEBSITE", string.IsNullOrEmpty(stay.STAY_WEBSITE) ? "" : stay.STAY_WEBSITE);
                  command.Parameters.AddWithValue("@ADDRESS", string.IsNullOrEmpty(stay.STAY_ADDRESS) ? "" : stay.STAY_ADDRESS);
                  command.Parameters.AddWithValue("@CONTACT_PERSON", string.IsNullOrEmpty(stay.STAY_CONTACT_PERSON) ? "" : stay.STAY_CONTACT_PERSON);
                  command.Parameters.AddWithValue("@DESCRIPTION", string.IsNullOrEmpty(stay.STAY_DESCRIPTION) ? "" : stay.STAY_DESCRIPTION);
                  command.Parameters.AddWithValue("@STAY_CATEGORY_ID", !int.IsPositive(stay.STAY_CATEGORY_ID) ? "" : stay.STAY_CATEGORY_ID);
                  command.Parameters.AddWithValue("@ID", stay.STAY_ID);

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
      public async Task<bool> DeleteStay(int StayId)
      {
         try
         {
            using (connection = new MySqlConnection(Properties.Resources.ResourceManager.GetString("ConnString")))
            {
               await connection.OpenAsync();

               string query = "DELETE FROM `stil_app_db`.`Stay_Table` WHERE STAY_ID = @STAY_ID";

               using (var command = new MySqlCommand(query, connection))
               {
                  command.Parameters.AddWithValue("@STAY_ID", StayId);

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
      public async Task<int> AddStay(StayModel stay)
      {
         try
         {
            using (connection = new MySqlConnection(Properties.Resources.ResourceManager.GetString("ConnString")))
            {
               await connection.OpenAsync();

               //default insert into SQL statement
               string query = "INSERT INTO `stil_app_db`.`Stay_Table` " +
                   "(`STAY_NAME`, `STAY_TEL_NUM`, `STAY_MOBILE_NUM`, `STAY_EMAIL`, `STAY_WEBSITE`, `STAY_ADDRESS`, `STAY_CONTACT_PERSON`, `STAY_DESCRIPTION`, `STAY_CATEGORY_ID`) " +
                   "VALUES (@NAME, @TEL_NUM, @MOBILE_NUM, @EMAIL, @WEBSITE, @ADDRESS, @CONTACT_PERSON, @DESCRIPTION, @STAY_CATEGORY_ID);";

               using (var command = new MySqlCommand(query, connection))
               {
                  //command.Parameters.AddWithValue is a way to paramiterise the SQL, avoiding a SQL injection attack
                  command.Parameters.AddWithValue("@NAME", string.IsNullOrEmpty(stay.STAY_NAME) ? "" : stay.STAY_NAME);
                  command.Parameters.AddWithValue("@TEL_NUM", string.IsNullOrEmpty(stay.STAY_TEL_NUM) ? "" : stay.STAY_TEL_NUM);
                  command.Parameters.AddWithValue("@MOBILE_NUM", string.IsNullOrEmpty(stay.STAY_MOBILE_NUM) ? "" : stay.STAY_MOBILE_NUM);
                  command.Parameters.AddWithValue("@EMAIL", string.IsNullOrEmpty(stay.STAY_EMAIL) ? "" : stay.STAY_EMAIL);
                  command.Parameters.AddWithValue("@WEBSITE", string.IsNullOrEmpty(stay.STAY_WEBSITE) ? "" : stay.STAY_WEBSITE);
                  command.Parameters.AddWithValue("@ADDRESS", string.IsNullOrEmpty(stay.STAY_ADDRESS) ? "" : stay.STAY_ADDRESS);
                  command.Parameters.AddWithValue("@CONTACT_PERSON", string.IsNullOrEmpty(stay.STAY_CONTACT_PERSON) ? "" : stay.STAY_CONTACT_PERSON);
                  command.Parameters.AddWithValue("@DESCRIPTION", string.IsNullOrEmpty(stay.STAY_DESCRIPTION) ? "" : stay.STAY_DESCRIPTION);
                  command.Parameters.AddWithValue("@STAY_CATEGORY_ID", !int.IsPositive(stay.STAY_CATEGORY_ID) ? "" : stay.STAY_CATEGORY_ID);

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
      public async Task<int> AddStayImage(string url, int StayId)
      {
         try
         {
            using (connection = new MySqlConnection(Properties.Resources.ResourceManager.GetString("ConnString")))
            {
               await connection.OpenAsync();

               //default insert into SQL statement
               string query = "INSERT INTO `stil_app_db`.`Stay_Image_Table` (`STAY_IMAGE_URL`, `STAY_ID`) VALUES (@URL, @ID);";

               using (var command = new MySqlCommand(query, connection))
               {
                  //command.Parameters.AddWithValue is a way to paramiterise the SQL, avoiding a SQL injection attack
                  command.Parameters.AddWithValue("@URL", string.IsNullOrEmpty(url) ? "" : url);
                  command.Parameters.AddWithValue("@ID", StayId.ToString());

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
      //
      public async Task<bool> DeleteImage(int StayId, string url)
      {
         try
         {
            using (connection = new MySqlConnection(Properties.Resources.ResourceManager.GetString("ConnString")))
            {
               await connection.OpenAsync();

               string query = "DELETE FROM `stil_app_db`.`Stay_Image_Table` WHERE STAY_IMAGE_URL = @URL AND STAY_ID = @STAYID;";

               using (var command = new MySqlCommand(query, connection))
               {
                  command.Parameters.AddWithValue("@URL", url);
                  command.Parameters.AddWithValue("@STAYID", StayId);

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
      public async Task GetStayCategory()
      {
         try
         {
            List<StayCategoryModel> newEntries = new List<StayCategoryModel>();

            using (var connection = new MySqlConnection(Properties.Resources.ResourceManager.GetString("ConnString")))
            {
               await connection.OpenAsync();

               string query = "SELECT * FROM `stil_app_db`.`Stay_Category_Table`;";

               using (var command = new MySqlCommand(query, connection))
               using (var reader = await command.ExecuteReaderAsync())
               {
                  while (await reader.ReadAsync())
                  {
                     var newEntry = new StayCategoryModel
                     {
                        STAY_CATEGORY_ID = reader.GetInt32(0),
                        STAY_CATEGORY_TYPE = reader.GetString(1)
                     };

                     newEntries.Add(newEntry);
                  }
               }
            }

            // Use a ConcurrentBag to ensure thread safety
            var concurrentBag = new ConcurrentBag<StayCategoryModel>(newEntries);
            _ToolBox.StayCategoryList = concurrentBag.ToList();
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
