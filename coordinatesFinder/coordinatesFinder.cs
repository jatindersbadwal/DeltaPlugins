using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using bingMapsModel;
using Microsoft.Xrm.Sdk;
//using System.Net;
using System.IO;

namespace coordinatesFinder
{
    public class Program : IPlugin
    {
        //String addressFinder = String.Empty;
        public void Execute(IServiceProvider serviceProvider)
        {
            {
                ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
                // Obtain the execution context from the service provider.  
                IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
                IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
                // The InputParameters collection contains all the data passed in the message request.  
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                { // Obtain the target entity from the input parameters.  
                    Entity entity = (Entity)context.InputParameters["Target"];
                    if (entity.LogicalName != "contact") // if record is not created in contact entity then return
                    {
                        return;
                    }
                    else
                    {
                        String addressField = string.Empty;
                        if (entity.Contains("address1_line1"))
                        {
                            addressField = entity.Attributes["address1_line1"].ToString();
                        }
                        else
                        {
                            addressField = null;
                        }
                        if (addressField != null || addressField != "")
                        {
                            String addressFinder = addressField;
                            String message = RunAsync(addressFinder).Result;
                            //Task awaitResult = RunAsync(addressFinder);
                            if(message.Length != 0)
                            {
                                entity.Attributes["description"] = message.ToString();
                            }
                        }
                        else
                        {
                            return;
                        }
                    }
                }
            }
        }

       public async Task<string> RunAsync(string address)
        {
            String bingMapsKey = "AsrOHOrxfF61RlwaErE1SLmuf1UktNSyrtMkYQkJNeph25nkbDg6IoMlI3RuKON_";
            using (var client = new HttpClient())
            {
                
                //go get data
                client.BaseAddress = new Uri("http://dev.virtualearth.net/");
                client.DefaultRequestHeaders.Accept.Clear();
                
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage response = await client.GetAsync("REST/v1/Locations?addressLine=" + address + "&maxResults=1 &key=" + bingMapsKey);
                if (response.IsSuccessStatusCode)
                {
                    var getResponseString = await response.Content.ReadAsStringAsync();
                    Root JSONaddress = JsonSerializer.Deserialize<Root>(getResponseString);
                    String formatAddress = JSONaddress.resourceSets[0].resources[0].address.formattedAddress;
                    // var formatLatitude = JSONaddress.resourceSets[0].resources[0].geocodePoints[0].coordinates[0];
                    // var formatLongitude = JSONaddress.resourceSets[0].resources[0].geocodePoints[0].coordinates[1];
                    return formatAddress;
                }
                else
                {
                    return null;
                }
            }
        }
    }
}