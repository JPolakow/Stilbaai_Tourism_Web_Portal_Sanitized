using Microsoft.IdentityModel.Tokens;
using MySql.Data.MySqlClient;
using Stilbaai_Tourism_Web_Portal.Classes;
using Stilbaai_Tourism_Web_Portal.Models;
using System.Collections.Concurrent;

namespace Stilbaai_Tourism_Web_Portal.DBHandelers
{
   public class BusinessDBHandeler
   {
      private readonly ToolBoxSingleton _ToolBox = ToolBoxSingleton.Instance;
      private MySqlConnection connection;

      //---------------------------------------------------------------------------------------
      //
      public async Task GetBusiness()
      {
         try
         {
            List<BusinessModel> newEntries = new List<BusinessModel>();

            using (connection = new MySqlConnection(Properties.Resources.ResourceManager.GetString("ConnString")))
            {
               await connection.OpenAsync();

               string query = "SELECT * FROM `stil_app_db`.`Business_Table`;";

               using (var command = new MySqlCommand(query, connection))
               using (var reader = await command.ExecuteReaderAsync())
               {
                  while (await reader.ReadAsync())
                  {
                     var newEntry = new BusinessModel
                     {
                        BUSINESS_ID = reader.GetInt32(0),
                        BUSINESS_NAME = reader.GetString(1),
                        BUSINESS_TEL_NUM = reader.GetString(2),
                        BUSINESS_MOBILE_NUM = reader.GetString(3),
                        BUSINESS_EMAIL = reader.GetString(4),
                        BUSINESS_WEBSITE = reader.GetString(5),
                        BUSINESS_ADDRESS = reader.GetString(6),
                        BUSINESS_CONTACT_PERSON = reader.GetString(7),
                        BUSINESS_DESCRIPTION = reader.GetString(8),
                        BUSINESS_CATEGORY_ID = reader.GetInt32(9)
                     };

                     newEntries.Add(newEntry);
                  }
               }
            }

            // Use a ConcurrentBag to ensure thread safety
            var concurrentBag = new ConcurrentBag<BusinessModel>(newEntries);
            _ToolBox.BusinessList = concurrentBag.ToList();
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
      public async Task<List<string>> GetBusinessImages(int BusinessId)
      {
         try
         {
            using (connection = new MySqlConnection(Properties.Resources.ResourceManager.GetString("ConnString")))
            {
               await connection.OpenAsync();

               List<string> urls = new List<string>();

               string query = $"SELECT * FROM `stil_app_db`.`Activity_Image_Table` WHERE ACTIVITY_ID = @ID;";

               using (var command = new MySqlCommand(query, connection))
               {
                  command.Parameters.AddWithValue("@ID", BusinessId);

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
      public async Task<bool> UpdateBusiness(BusinessModel business)
      {
         try
         {
            using (connection = new MySqlConnection(Properties.Resources.ResourceManager.GetString("ConnString")))
            {
               await connection.OpenAsync();

               string query = "UPDATE `stil_app_db`.`Business_Table` SET " +
                           "`BUSINESS_NAME` = @NAME, " +
                           "`BUSINESS_TELL_NUM` = @TEL_NUM, " +
                           "`BUSINESS_MOBILE_NUM` = @MOBILE_NUM, " +
                           "`BUSINESS_EMAIL` = @EMAIL, " +
                           "`BUSINESS_WEBSITE` = @WEBSITE, " +
                           "`BUSINESS_ADDRESS` = @ADDRESS, " +
                           "`BUSINESS_CONTACT_PERSON` = @CONTACT_PERSON, " +
                           "`BUSINESS_DESCRIPTION` = @DESCRIPTION, " +
                           "`BUSINESS_CATEGORY_ID` = @CATEGORY_ID " +
                           "WHERE `BUSINESS_ID` = @ID;";

               using (var command = new MySqlCommand(query, connection))
               {
                  command.Parameters.AddWithValue("@NAME", string.IsNullOrEmpty(business.BUSINESS_NAME) ? "" : business.BUSINESS_NAME);
                  command.Parameters.AddWithValue("@TEL_NUM", string.IsNullOrEmpty(business.BUSINESS_TEL_NUM) ? "" : business.BUSINESS_TEL_NUM);
                  command.Parameters.AddWithValue("@MOBILE_NUM", string.IsNullOrEmpty(business.BUSINESS_MOBILE_NUM) ? "" : business.BUSINESS_MOBILE_NUM);
                  command.Parameters.AddWithValue("@EMAIL", string.IsNullOrEmpty(business.BUSINESS_EMAIL) ? "" : business.BUSINESS_EMAIL);
                  command.Parameters.AddWithValue("@WEBSITE", string.IsNullOrEmpty(business.BUSINESS_WEBSITE) ? "" : business.BUSINESS_WEBSITE);
                  command.Parameters.AddWithValue("@ADDRESS", string.IsNullOrEmpty(business.BUSINESS_ADDRESS) ? "" : business.BUSINESS_ADDRESS);
                  command.Parameters.AddWithValue("@CONTACT_PERSON", string.IsNullOrEmpty(business.BUSINESS_CONTACT_PERSON) ? "" : business.BUSINESS_CONTACT_PERSON);
                  command.Parameters.AddWithValue("@DESCRIPTION", string.IsNullOrEmpty(business.BUSINESS_DESCRIPTION) ? "" : business.BUSINESS_DESCRIPTION);
                  command.Parameters.AddWithValue("@CATEGORY_ID", !int.IsPositive(business.BUSINESS_CATEGORY_ID) ? "" : business.BUSINESS_CATEGORY_ID);
                  command.Parameters.AddWithValue("@ID", business.BUSINESS_ID);

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
      public async Task<bool> DeleteBusiness(int BusinessId)
      {
         try
         {
            using (connection = new MySqlConnection(Properties.Resources.ResourceManager.GetString("ConnString")))
            {
               await connection.OpenAsync();

               string query = "DELETE FROM `stil_app_db`.`Business_Table` WHERE BUSINESS_ID = @BUSINESS_ID";

               using (var command = new MySqlCommand(query, connection))
               {
                  command.Parameters.AddWithValue("@BUSINESS_ID", BusinessId);

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
      public async Task<int> AddBusiness(BusinessModel business)
      {
         try
         {
            using (connection = new MySqlConnection(Properties.Resources.ResourceManager.GetString("ConnString")))
            {
               await connection.OpenAsync();

               //default insert into SQL statement
               string query = "INSERT INTO `stil_app_db`.`Business_Table` " +
                   "(`BUSINESS_NAME`, `BUSINESS_TELL_NUM`, `BUSINESS_MOBILE_NUM`, `BUSINESS_EMAIL`, `BUSINESS_WEBSITE`, `BUSINESS_ADDRESS`, `BUSINESS_CONTACT_PERSON`, `BUSINESS_DESCRIPTION`, `BUSINESS_CATEGORY_ID`) " +
                   "VALUES (@NAME, @TEL_NUM, @MOBILE_NUM, @EMAIL, @WEBSITE, @ADDRESS, @CONTACT_PERSON, @DESCRIPTION, @CATEGORY_ID);";

               using (var command = new MySqlCommand(query, connection))
               {
                  //command.Parameters.AddWithValue is a way to paramiterise the SQL, avoiding a SQL injection attack
                  command.Parameters.AddWithValue("@NAME", string.IsNullOrEmpty(business.BUSINESS_NAME) ? "" : business.BUSINESS_NAME);
                  command.Parameters.AddWithValue("@TEL_NUM", string.IsNullOrEmpty(business.BUSINESS_TEL_NUM) ? "" : business.BUSINESS_TEL_NUM);
                  command.Parameters.AddWithValue("@MOBILE_NUM", string.IsNullOrEmpty(business.BUSINESS_MOBILE_NUM) ? "" : business.BUSINESS_MOBILE_NUM);
                  command.Parameters.AddWithValue("@EMAIL", string.IsNullOrEmpty(business.BUSINESS_EMAIL) ? "" : business.BUSINESS_EMAIL);
                  command.Parameters.AddWithValue("@WEBSITE", string.IsNullOrEmpty(business.BUSINESS_WEBSITE) ? "" : business.BUSINESS_WEBSITE);
                  command.Parameters.AddWithValue("@ADDRESS", string.IsNullOrEmpty(business.BUSINESS_ADDRESS) ? "" : business.BUSINESS_ADDRESS);
                  command.Parameters.AddWithValue("@CONTACT_PERSON", string.IsNullOrEmpty(business.BUSINESS_CONTACT_PERSON) ? "" : business.BUSINESS_CONTACT_PERSON);
                  command.Parameters.AddWithValue("@DESCRIPTION", string.IsNullOrEmpty(business.BUSINESS_DESCRIPTION) ? "" : business.BUSINESS_DESCRIPTION);
                  command.Parameters.AddWithValue("@CATEGORY_ID", !int.IsPositive(business.BUSINESS_CATEGORY_ID) ? "" : business.BUSINESS_CATEGORY_ID);

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
      public async Task<int> AddBusinessImage(string url, int BusinessId)
      {
         try
         {
            using (connection = new MySqlConnection(Properties.Resources.ResourceManager.GetString("ConnString")))
            {
               await connection.OpenAsync();

               //default insert into SQL statement
               string query = "INSERT INTO `stil_app_db`.`Business_Image_Table` (`BUSINESS_IMAGE_URL`, `BUSINESS_ID`) VALUES (@URL, @ID);";

               using (var command = new MySqlCommand(query, connection))
               {
                  //command.Parameters.AddWithValue is a way to paramiterise the SQL, avoiding a SQL injection attack
                  command.Parameters.AddWithValue("@URL", string.IsNullOrEmpty(url) ? "" : url);
                  command.Parameters.AddWithValue("@ID", BusinessId.ToString());

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
      public async Task<bool> DeleteImage(int BusinessId, string url)
      {
         try
         {
            using (connection = new MySqlConnection(Properties.Resources.ResourceManager.GetString("ConnString")))
            {
               await connection.OpenAsync();

               string query = "DELETE FROM `stil_app_db`.`Business_Image_Table` WHERE BUSINESS_IMAGE_URL = @URL AND BUSINESS_ID = @BUSINESSID;";

               using (var command = new MySqlCommand(query, connection))
               {
                  command.Parameters.AddWithValue("@URL", url);
                  command.Parameters.AddWithValue("@BUSINESSID", BusinessId);

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
      public async Task GetBusinessCategory()
      {
         try
         {
            List<BusinessCategoryModel> newEntries = new List<BusinessCategoryModel>();

            using (var connection = new MySqlConnection(Properties.Resources.ResourceManager.GetString("ConnString")))
            {
               await connection.OpenAsync();

               string query = "SELECT * FROM `stil_app_db`.`Business_Category_Table`;";

               using (var command = new MySqlCommand(query, connection))
               using (var reader = await command.ExecuteReaderAsync())
               {
                  while (await reader.ReadAsync())
                  {
                     var newEntry = new BusinessCategoryModel
                     {
                        BUSINESS_CATEGORY_ID = reader.GetInt32(0),
                        BUSINESS_CATEGORY_TYPE = reader.GetString(1)
                     };

                     newEntries.Add(newEntry);
                  }
               }
            }

            // Use a ConcurrentBag to ensure thread safety
            var concurrentBag = new ConcurrentBag<BusinessCategoryModel>(newEntries);
            _ToolBox.BusinessCategoryList = concurrentBag.ToList();
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
