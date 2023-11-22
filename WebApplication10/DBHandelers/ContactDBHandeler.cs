using MySql.Data.MySqlClient;
using Stilbaai_Tourism_Web_Portal.Classes;
using Stilbaai_Tourism_Web_Portal.Models;
using System.Collections.Concurrent;

namespace Stilbaai_Tourism_Web_Portal.Workers
{
   public class ContactDBHandeler
   {
      private readonly ToolBoxSingleton _ToolBox = ToolBoxSingleton.Instance;
      private MySqlConnection connection;

      //---------------------------------------------------------------------------------------
      /// <summary>
      /// get all entries from db
      /// </summary>
      /// <returns></returns>
      public async Task GetContact()
      {
         try
         {
            List<ContactModel> newEntries = new List<ContactModel>();

            using (connection = new MySqlConnection(Properties.Resources.ResourceManager.GetString("ConnString")))
            {
               await connection.OpenAsync();

               string query = "SELECT * FROM `stil_app_db`.`Contact_Table`;";

               using (var command = new MySqlCommand(query, connection))
               using (var reader = await command.ExecuteReaderAsync())
               {
                  while (await reader.ReadAsync())
                  {
                     var newEntry = new ContactModel
                     {
                         CONTACT_ID = reader.GetInt32(0),
                         CONTACT_NAME = reader.GetString(1),
                         CONTACT_NUM = reader.GetString(2),
                         CONTACT_EMAIL = reader.GetString(3),
                         CONTACT_ADDRESS = reader.GetString(4),
                         CONTACT_PERSON = reader.GetString(5),
                     };

                     newEntries.Add(newEntry);
                  }
               }
            }

            // Use a ConcurrentBag to ensure thread safety
            var concurrentBag = new ConcurrentBag<ContactModel>(newEntries);
            _ToolBox.ContactList = concurrentBag.ToList();
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
      /// update entry
      /// </summary>
      /// <param name="contact"></param>
      /// <returns></returns>
      public async Task<bool> UpdateContact(ContactModel contact)
      {
         try
         {
            using (connection = new MySqlConnection(Properties.Resources.ResourceManager.GetString("ConnString")))
            {
               await connection.OpenAsync();

               string query = "UPDATE `stil_app_db`.`Contact_Table` SET " +
                           "`CONTACT_NAME` = @NAME, " +
                           "`CONTACT_NUM` = @NUM, " +
                           "`CONTACT_EMAIL` = @EMAIL, " +
                           "`CONTACT_ADDRESS` = @ADDRESS, " +
                           "`CONTACT_PERSON` = @CONTACT_PERSON " +
                           "WHERE `CONTACT_ID` = @ID;";

               using (var command = new MySqlCommand(query, connection))
               {
                  command.Parameters.AddWithValue("@NAME", string.IsNullOrEmpty(contact.CONTACT_NAME) ? "" : contact.CONTACT_NAME);
                  command.Parameters.AddWithValue("@NUM", string.IsNullOrEmpty(contact.CONTACT_NUM) ? "" : contact.CONTACT_NUM);
                  command.Parameters.AddWithValue("@EMAIL", string.IsNullOrEmpty(contact.CONTACT_EMAIL) ? "" : contact.CONTACT_EMAIL);
                  command.Parameters.AddWithValue("@ADDRESS", string.IsNullOrEmpty(contact.CONTACT_ADDRESS) ? "" : contact.CONTACT_ADDRESS);
                  command.Parameters.AddWithValue("@CONTACT_PERSON", string.IsNullOrEmpty(contact.CONTACT_PERSON) ? "" : contact.CONTACT_PERSON);
                  command.Parameters.AddWithValue("@ID", contact.CONTACT_ID);

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
      /// <param name="CONTACT_ID"></param>
      /// <returns></returns>
      public async Task<bool> DeleteContacr(int CONTACT_ID)
      {
         try
         {
            using (connection = new MySqlConnection(Properties.Resources.ResourceManager.GetString("ConnString")))
            {
               await connection.OpenAsync();

               string query = "DELETE FROM `stil_app_db`.`Contact_Table` WHERE CONTACT_ID = @ID";

               using (var command = new MySqlCommand(query, connection))
               {
                  command.Parameters.AddWithValue("@ID", CONTACT_ID);

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
      /// <param name="contact"></param>
      /// <returns></returns>
      public async Task<int> AddContact(ContactModel contact)
      {
         try
         {
            using (connection = new MySqlConnection(Properties.Resources.ResourceManager.GetString("ConnString")))
            {
               await connection.OpenAsync();

               //default insert into SQL statement
               string query = "INSERT INTO `stil_app_db`.`Contact_Table` " +
                   "(`CONTACT_NAME`, `CONTACT_NUM`, `CONTACT_EMAIL`, `CONTACT_ADDRESS`, `CONTACT_PERSON`) " +
                   "VALUES (@NAME, @NUM, @EMAIL, @ADDRESS, @CONTACT_PERSON);";

               using (var command = new MySqlCommand(query, connection))
               {
                  //command.Parameters.AddWithValue is a way to paramiterise the SQL, avoiding a SQL injection attack
                  command.Parameters.AddWithValue("@NAME", string.IsNullOrEmpty(contact.CONTACT_NAME) ? "" : contact.CONTACT_NAME);
                  command.Parameters.AddWithValue("@NUM", string.IsNullOrEmpty(contact.CONTACT_NUM) ? "" : contact.CONTACT_NUM);
                  command.Parameters.AddWithValue("@EMAIL", string.IsNullOrEmpty(contact.CONTACT_EMAIL) ? "" : contact.CONTACT_EMAIL);
                  command.Parameters.AddWithValue("@ADDRESS", string.IsNullOrEmpty(contact.CONTACT_ADDRESS) ? "" : contact.CONTACT_ADDRESS);
                  command.Parameters.AddWithValue("@CONTACT_PERSON", string.IsNullOrEmpty(contact.CONTACT_PERSON) ? "" : contact.CONTACT_PERSON);

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
   }
}
//-------------------------------------====END OF FILE====-------------------------------------