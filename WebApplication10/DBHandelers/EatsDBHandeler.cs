using MySql.Data.MySqlClient;
using Stilbaai_Tourism_Web_Portal.Classes;
using Stilbaai_Tourism_Web_Portal.Models;
using System.Collections.Concurrent;

namespace Stilbaai_Tourism_Web_Portal.Workers
{
   public class EatsDBHandeler
   {
      private readonly ToolBoxSingleton _ToolBox = ToolBoxSingleton.Instance;
      private MySqlConnection connection;

      //---------------------------------------------------------------------------------------
      //
      public async Task GetEat()
      {
         try
         {
            List<EatModel> newEntries = new List<EatModel>();

            using (var connection = new MySqlConnection(Properties.Resources.ResourceManager.GetString("ConnString")))
            {
               await connection.OpenAsync();

               string query = "SELECT * FROM `stil_app_db`.`Eat_Table`;";

               using (var command = new MySqlCommand(query, connection))
               using (var reader = await command.ExecuteReaderAsync())
               {
                  while (await reader.ReadAsync())
                  {
                     var newEntry = new EatModel
                     {
                        EAT_ID = reader.GetInt32(0),
                        EAT_NAME = reader.GetString(1),
                        EAT_TEL_NUM = reader.GetString(2),
                        EAT_MOBILE_NUM = reader.GetString(3),
                        EAT_EMAIL = reader.GetString(4),
                        EAT_WEBSITE = reader.GetString(5),
                        EAT_ADDRESS = reader.GetString(6),
                        EAT_CONTACT_PERSON = reader.GetString(7),
                        EAT_DESCRIPTION = reader.GetString(8)
                     };

                     newEntries.Add(newEntry);
                  }
               }
            }

            // Use a ConcurrentBag to ensure thread safety
            var concurrentBag = new ConcurrentBag<EatModel>(newEntries);
            _ToolBox.EatsList = concurrentBag.ToList();
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
      public async Task<List<string>> GetEatImages(int EatId)
      {
         try
         {
            using (connection = new MySqlConnection(Properties.Resources.ResourceManager.GetString("ConnString")))
            {

               List<string> urls = new List<string>();

               string query = $"SELECT * FROM `stil_app_db`.`Activity_Image_Table` WHERE ACTIVITY_ID = @ID;";

               using (var command = new MySqlCommand(query, connection))
               {
                  command.Parameters.AddWithValue("@ID", EatId);

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
      public async Task<bool> UpdateEat(EatModel eat)
      {
         try
         {
            using (connection = new MySqlConnection(Properties.Resources.ResourceManager.GetString("ConnString")))
            {
               await connection.OpenAsync();

               string query = "UPDATE `stil_app_db`.`Eat_Table` SET " +
                           "`EAT_NAME` = @NAME, " +
                           "`EAT_TEL_NUM` = @TEL_NUM, " +
                           "`EAT_MOBILE_NUM` = @MOBILE_NUM, " +
                           "`EAT_EMAIL` = @EMAIL, " +
                           "`EAT_WEBSITE` = @WEBSITE, " +
                           "`EAT_ADDRESS` = @ADDRESS, " +
                           "`EAT_CONTACT_PERSON` = @CONTACT_PERSON, " +
                           "`EAT_DESCRIPTION` = @DESCRIPTION " +
                           "WHERE `EAT_ID` = @ID;";

               using (var command = new MySqlCommand(query, connection))
               {
                  command.Parameters.AddWithValue("@NAME", string.IsNullOrEmpty(eat.EAT_NAME) ? "" : eat.EAT_NAME);
                  command.Parameters.AddWithValue("@TEL_NUM", string.IsNullOrEmpty(eat.EAT_TEL_NUM) ? "" : eat.EAT_TEL_NUM);
                  command.Parameters.AddWithValue("@MOBILE_NUM", string.IsNullOrEmpty(eat.EAT_MOBILE_NUM) ? "" : eat.EAT_MOBILE_NUM);
                  command.Parameters.AddWithValue("@EMAIL", string.IsNullOrEmpty(eat.EAT_EMAIL) ? "" : eat.EAT_EMAIL);
                  command.Parameters.AddWithValue("@WEBSITE", string.IsNullOrEmpty(eat.EAT_WEBSITE) ? "" : eat.EAT_WEBSITE);
                  command.Parameters.AddWithValue("@ADDRESS", string.IsNullOrEmpty(eat.EAT_ADDRESS) ? "" : eat.EAT_ADDRESS);
                  command.Parameters.AddWithValue("@CONTACT_PERSON", string.IsNullOrEmpty(eat.EAT_CONTACT_PERSON) ? "" : eat.EAT_CONTACT_PERSON);
                  command.Parameters.AddWithValue("@DESCRIPTION", string.IsNullOrEmpty(eat.EAT_DESCRIPTION) ? "" : eat.EAT_DESCRIPTION);
                  command.Parameters.AddWithValue("@ID", eat.EAT_ID);

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
      public async Task<bool> DeleteEat(int EAT_ID)
      {
         try
         {
            using (connection = new MySqlConnection(Properties.Resources.ResourceManager.GetString("ConnString")))
            {
               await connection.OpenAsync();

               string query = "DELETE FROM `stil_app_db`.`Eat_Table` WHERE EAT_ID = @EAT_ID";

               using (var command = new MySqlCommand(query, connection))
               {
                  command.Parameters.AddWithValue("@EAT_ID", EAT_ID);

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
      public async Task<int> AddEat(EatModel eat)
      {
         try
         {
            using (connection = new MySqlConnection(Properties.Resources.ResourceManager.GetString("ConnString")))
            {
               await connection.OpenAsync();

               //default insert into SQL statement
               string query = "INSERT INTO `stil_app_db`.`Eat_Table` " +
                   "(`EAT_NAME`, `EAT_TEL_NUM`, `EAT_MOBILE_NUM`, `EAT_EMAIL`, `EAT_WEBSITE`, `EAT_ADDRESS`, `EAT_CONTACT_PERSON`, `EAT_DESCRIPTION`) " +
                   "VALUES (@NAME, @TEL_NUM, @MOBILE_NUM, @EMAIL, @WEBSITE, @ADDRESS, @CONTACT_PERSON, @DESCRIPTION);";

               using (var command = new MySqlCommand(query, connection))
               {
                  //command.Parameters.AddWithValue is a way to paramiterise the SQL, avoiding a SQL injection attack
                  command.Parameters.AddWithValue("@NAME", string.IsNullOrEmpty(eat.EAT_NAME) ? "" : eat.EAT_NAME);
                  command.Parameters.AddWithValue("@TEL_NUM", string.IsNullOrEmpty(eat.EAT_TEL_NUM) ? "" : eat.EAT_TEL_NUM);
                  command.Parameters.AddWithValue("@MOBILE_NUM", string.IsNullOrEmpty(eat.EAT_MOBILE_NUM) ? "" : eat.EAT_MOBILE_NUM);
                  command.Parameters.AddWithValue("@EMAIL", string.IsNullOrEmpty(eat.EAT_EMAIL) ? "" : eat.EAT_EMAIL);
                  command.Parameters.AddWithValue("@WEBSITE", string.IsNullOrEmpty(eat.EAT_WEBSITE) ? "" : eat.EAT_WEBSITE);
                  command.Parameters.AddWithValue("@ADDRESS", string.IsNullOrEmpty(eat.EAT_ADDRESS) ? "" : eat.EAT_ADDRESS);
                  command.Parameters.AddWithValue("@CONTACT_PERSON", string.IsNullOrEmpty(eat.EAT_CONTACT_PERSON) ? "" : eat.EAT_CONTACT_PERSON);
                  command.Parameters.AddWithValue("@DESCRIPTION", string.IsNullOrEmpty(eat.EAT_DESCRIPTION) ? "" : eat.EAT_DESCRIPTION);

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
      public async Task<int> AddEatImage(string url, int id)
      {
         try
         {
            using (connection = new MySqlConnection(Properties.Resources.ResourceManager.GetString("ConnString")))
            {
               await connection.OpenAsync();

               //default insert into SQL statement
               string query = "INSERT INTO `stil_app_db`.`Eat_Image_Table` (`EAT_IMAGE_URL`, `EAT_ID`) VALUES (@URL, @ID);";

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
      public async Task<bool> DeleteImage(int id, string url)
      {
         try
         {
            using (connection = new MySqlConnection(Properties.Resources.ResourceManager.GetString("ConnString")))
            {
               await connection.OpenAsync();

               string query = "DELETE FROM `stil_app_db`.`Eat_Image_Table` WHERE EAT_IMAGE_URL = @URL AND EAT_ID = @EATID;";

               using (var command = new MySqlCommand(query, connection))
               {
                  command.Parameters.AddWithValue("@URL", url);
                  command.Parameters.AddWithValue("@EATID", id);

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