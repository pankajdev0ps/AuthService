using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace AuthService
{
    public static class LoginFunction
    {
        [FunctionName("LoginFunction")]
        public static async Task<int> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            
            string userEmailID  = data.emailID;
            string userPassword = data.password;
            string userType     = data.usertype;
            int result          = 0;

            var config = new ConfigurationBuilder()
            .SetBasePath(context.FunctionAppDirectory)
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();
            
            string connectionString = config.GetConnectionString("SqlDBConnectionString");
            //string connectionString = Environment.GetEnvironmentVariable("SqlDBConnectionString");

            SqlConnection sqlConnection = new SqlConnection(connectionString);
            SqlCommand sqlCommand = new SqlCommand(
                "select [dbo].ufn_ValidateLogin(@userEmailID,@userPassword,@customerType)",
                sqlConnection
            );

            sqlCommand.Parameters.AddWithValue("@userEmailID", userEmailID);
            sqlCommand.Parameters.AddWithValue("@userPassword", userPassword);
            sqlCommand.Parameters.AddWithValue("@customerType", userType);

            try
            {
                sqlConnection.Open();
                result = Convert.ToInt32(sqlCommand.ExecuteScalar());
            }
            catch (Exception e)
            {
                result = -1;
                log.LogInformation("Exception Occured\n\n" + e.Message + "\n\n");
            }
            finally
            {
                sqlConnection.Close();
            }

            return result;
        }
    }
}