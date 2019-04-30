using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace JsGameTest.Classes
{
    public class DatabaseQueries
    {
        private string connectionString = "Server = localhost; Database=TrustMeDb;Integrated Security = False;User ID = 'MASTER';Password = 'ADMINPASSWORD'";

        // Retrieve all stories
        public List<Story> RetrieveAllStories()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand("SELECT * FROM [Story]", connection);

                List<Story> StoryList = new List<Story>();

                try
                {
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            int rowCount = 0;

                            while (reader.Read())
                            {
                                Story newStory = new Story
                                {
                                    Id = (int)reader["Id"],
                                    OwnerId = (int)reader["OwnerId"],
                                    Date = (DateTime)reader["Date"],
                                    Description = (string)reader["Description"],
                                    IsRoot = Convert.ToBoolean(reader["IsRoot"]),
                                    Title = (string)reader["Title"],
                                    Status = (int)reader["Status"]
                                };

                                StoryList.Add(newStory);
                                rowCount++;
                            }

                            return StoryList;
                        }
                    }

                    return null;
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }
    }
}
