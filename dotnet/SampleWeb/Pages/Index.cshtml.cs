using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SampleWeb.Pages
{

    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger, IWebHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }

        public string ExternalLink { get; private set; } = string.Empty;
        public List<Employee> Employees { get; private set; } = new List<Employee>();
        private readonly IWebHostEnvironment _env;


        public void OnGet()
        {
            try
            {
                 var dbData = GetDataFromDB();
                 Employees = dbData;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                _logger.LogError(ex, "An error occurred while getting employees");
                var dbData = new List<Employee>();
                 var filePath = Path.Combine(
                _env.WebRootPath, "data/fallback.csv");
                string[] lines = System.IO.File.ReadAllLines(filePath);
                foreach (var line in lines)
                {
                    var data = line.Split(",");
                    Employee employee = new Employee();
                    employee.EmployeeID = data[0];
                    employee.Name = data[1];
                    employee.Hire_Date = data[2];
                    employee.Manager_ID = data[3];
                    employee.Salary = data[4];
                    employee.PHONE_NUMBER = data[5];
                    employee.Job_Id = data[6];
                    employee.Department_Name = data[7];
                    dbData.Add(employee);
                }
                 Employees = dbData;
            }

        }

        private static List<Employee> GetDataFromDB()
        {

            string token = Environment.GetEnvironmentVariable("DB_TOKEN");
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                string payload = JsonConvert.SerializeObject(new
                {
                    warehouse_id = "791486f2d3293fcd",
                    statement = "select * from `ida-sandbox-unitycatalog`.`enriched-unharmonized-techx01`.employee",
                    wait_timeout = "50s"
                });

                var httpContent = new StringContent(payload, Encoding.UTF8, "application/json");

                var task = Task.Run(() => client.PostAsync("https://adb-7241440174863553.13.azuredatabricks.net/api/2.0/sql/statements", httpContent));
                task.Wait();
                var response = task.Result;
                response.EnsureSuccessStatusCode();

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = response.Content.ReadAsStringAsync().Result;
                    var resjson = JsonConvert.DeserializeObject<JObject>(responseContent);
                    var data = resjson["result"]["data_array"];
                    List<Employee> employees = new List<Employee>();
                    foreach (var item in data)
                    {
                        Employee employee = new Employee();
                        employee.EmployeeID = item[0].ToString();
                        employee.Name = item[1].ToString();
                        employee.Hire_Date = item[2].ToString();
                        employee.Manager_ID = item[3].ToString();
                        employee.Salary = item[4].ToString();
                        employee.PHONE_NUMBER = item[5].ToString();
                        employee.Job_Id = item[6].ToString();
                        employee.Department_Name = item[7].ToString();
                        employees.Add(employee);
                    }
                    return employees;
                }
                else
                {
                    return null;
                }

            }
        }
    }
}
