using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http;
using System.Net.Http.Headers;
using bingMapsModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
//using BingMapsRESTToolkit;

namespace bingMapsCoordinates
{
   public class Program
    {
        static void Main(string[] args)
        {
            RunAsync().Wait();
            Console.ReadLine();
        }
        static async Task RunAsync()
        {
            Console.WriteLine("\tSEARCH ADDRESS......");
           // String addressLine = "84 stillman dr";
            String addressLine = Console.ReadLine();
            String bingMapsKey = "AsrOHOrxfF61RlwaErE1SLmuf1UktNSyrtMkYQkJNeph25nkbDg6IoMlI3RuKON_";
            using (var client = new HttpClient())
            {
                //go get data
                client.BaseAddress = new Uri("http://dev.virtualearth.net/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                Console.WriteLine("\tGETTING ADDRESS......");
                HttpResponseMessage response = await client.GetAsync("REST/v1/Locations?addressLine="+ addressLine + "&maxResults=1 &key="+bingMapsKey);
                if (response.IsSuccessStatusCode)
                {                   
                    var getResponseString = await response.Content.ReadAsStringAsync();
                    //var getResponse = await response.Content.ReadAsAsync<Address>();
                    dynamic JSONaddress = JsonConvert.DeserializeObject(getResponseString);
                    // dynamic data = JSONaddress; 
                    foreach (var data in JSONaddress.resourceSets)
                    {
                        foreach (var data1 in data.resources)
                        {
                            foreach (String address in data1.address)
                            {
                                Console.WriteLine(address);
                            };
                            foreach (String coordinates in data1.geocodePoints[0].coordinates)
                            {
                               Console.WriteLine(coordinates);                               
                            };                    
                        }
                    }
                }
            }
        }
    }
}