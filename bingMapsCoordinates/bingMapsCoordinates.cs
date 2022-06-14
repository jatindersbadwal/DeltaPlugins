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

namespace bingMapsCoordinates
{
    public class Program
    {
        static void Main(string[] args)
        {
            RunAsync().Wait();
            Console.ReadLine();
        }
        async static Task RunAsync()
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
                HttpResponseMessage response = await client.GetAsync("REST/v1/Locations?addressLine=" + addressLine + "&maxResults=5 &key=" + bingMapsKey);
                if (response.IsSuccessStatusCode)
                {
                    var getResponseString = await response.Content.ReadAsStringAsync();
                    //var getResponse = await response.Content.ReadAsAsync<Address>();
                    Root JSONaddress = JsonSerializer.Deserialize<Root>(getResponseString);
                    // dynamic data = JSONaddress; 

                    var formatAddress = JSONaddress.resourceSets[0].resources[0].address.formattedAddress;
                    var formatLatitude = JSONaddress.resourceSets[0].resources[0].geocodePoints[0].coordinates[0];
                    var formatLongitude = JSONaddress.resourceSets[0].resources[0].geocodePoints[0].coordinates[1];
                    Console.WriteLine(formatAddress+"\n");
                    Console.WriteLine(formatLatitude + ", "+ formatLongitude);
                }
            }
        }
    }
}