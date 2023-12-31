﻿using MySql.Data.MySqlClient;
using Stilbaai_Tourism_Web_Portal.Classes;
using Stilbaai_Tourism_Web_Portal.Models;
using System.Collections.Concurrent;

namespace Stilbaai_Tourism_Web_Portal.DBHandelers
{
   public class EelDBHandeler
   {
      private readonly ToolBoxSingleton _ToolBox = ToolBoxSingleton.Instance;
      private MySqlConnection connection;

      //---------------------------------------------------------------------------------------
      /// <summary>
      /// get all entries from db
      /// </summary>
      /// <returns></returns>
      public async Task GetEel()
      {
         try
         {
            List<EelModel> newEntries = new List<EelModel>();

            using (connection = new MySqlConnection(Properties.Resources.ResourceManager.GetString("ConnString")))
            {
               await connection.OpenAsync();

               string query = "SELECT * FROM `stil_app_db`.`Eel_Table`;";

               using (var command = new MySqlCommand(query, connection))
               using (var reader = await command.ExecuteReaderAsync())
               {
                  while (await reader.ReadAsync())
                  {
                     var newEntry = new EelModel
                     {
                        EEL_ID = reader.GetInt32(0),
                        EEL_NAME = reader.GetString(1),
                        EEL_CONTACT_NUM = reader.GetString(2),
                        EEL_ADDRESS = reader.GetString(3),
                        EEL_DESCRIPTION = reader.GetString(4)
                     };

                     newEntries.Add(newEntry);
                  }
               }
            }

            // Use a ConcurrentBag to ensure thread safety
            var concurrentBag = new ConcurrentBag<EelModel>(newEntries);
            _ToolBox.EelList = concurrentBag.ToList();
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
      /// <param name="EelId"></param>
      /// <returns></returns>
      public async Task<List<string>> GetEelImages(int EelId)
      {
         try
         {
            using (connection = new MySqlConnection(Properties.Resources.ResourceManager.GetString("ConnString")))
            {
               await connection.OpenAsync();

               List<string> urls = new List<string>();

               string query = $"SELECT * FROM `stil_app_db`.`Eel_Image_Table` WHERE EEL_ID = @ID;";

               using (var command = new MySqlCommand(query, connection))
               {
                  command.Parameters.AddWithValue("@ID", EelId);

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
      /// <param name="eel"></param>
      /// <returns></returns>
      public async Task<bool> UpdateEel(EelModel eel)
      {
         try
         {
            using (connection = new MySqlConnection(Properties.Resources.ResourceManager.GetString("ConnString")))
            {
               await connection.OpenAsync();

               string query = "UPDATE `stil_app_db`.`Eel_Table` SET " +
                           "`EEL_NAME` = @EEL_NAME, " +
                           "`EEL_CONTACT_NUM` = @EEL_CONTACT_NUM, " +
                           "`EEL_ADDRESS` = @EEL_ADDRESS, " +
                           "`EEL_DESCRIPTION` = @EEL_DESCRIPTION " +
                           "WHERE `EEL_ID` = @EEL_ID;";

               using (var command = new MySqlCommand(query, connection))
               {

                  command.Parameters.AddWithValue("@EEL_NAME", string.IsNullOrEmpty(eel.EEL_NAME) ? "" : eel.EEL_NAME);
                  command.Parameters.AddWithValue("@EEL_CONTACT_NUM", string.IsNullOrEmpty(eel.EEL_CONTACT_NUM) ? "" : eel.EEL_CONTACT_NUM);
                  command.Parameters.AddWithValue("@EEL_ADDRESS", string.IsNullOrEmpty(eel.EEL_ADDRESS) ? "" : eel.EEL_ADDRESS);
                  command.Parameters.AddWithValue("@EEL_DESCRIPTION", string.IsNullOrEmpty(eel.EEL_DESCRIPTION) ? "" : eel.EEL_DESCRIPTION);
                  command.Parameters.AddWithValue("@EEL_ID", eel.EEL_ID);

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
      /// add images for an entry
      /// </summary>
      /// <param name="url"></param>
      /// <param name="EelId"></param>
      /// <returns></returns>
      public async Task<int> AddEelImage(string url, int EelId)
      {
         try
         {
            using (connection = new MySqlConnection(Properties.Resources.ResourceManager.GetString("ConnString")))
            {
               await connection.OpenAsync();

               //default insert into SQL statement
               string query = "INSERT INTO `stil_app_db`.`Eel_Image_Table` (`EEL_IMAGE_URL`, `EEL_ID`) VALUES (@URL, @ID);";

               using (var command = new MySqlCommand(query, connection))
               {
                  //command.Parameters.AddWithValue is a way to paramiterise the SQL, avoiding a SQL injection attack
                  command.Parameters.AddWithValue("@URL", string.IsNullOrEmpty(url) ? "" : url);
                  command.Parameters.AddWithValue("@ID", EelId.ToString());

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
      /// <param name="EelId"></param>
      /// <param name="url"></param>
      /// <returns></returns>
      public async Task<bool> DeleteImage(int EelId, string url)
      {
         try
         {
            using (connection = new MySqlConnection(Properties.Resources.ResourceManager.GetString("ConnString")))
            {
               await connection.OpenAsync();

               string query = "DELETE FROM `stil_app_db`.`Eel_Image_Table` WHERE EEL_IMAGE_URL = @URL AND EEL_ID = @EELID;";

               using (var command = new MySqlCommand(query, connection))
               {
                  command.Parameters.AddWithValue("@URL", url);
                  command.Parameters.AddWithValue("@EELID", EelId);

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