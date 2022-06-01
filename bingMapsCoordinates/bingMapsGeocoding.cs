using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Crm.Sdk;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using BingMapsRESTToolkit;

namespace bingMapsCoordinates
{
    public class bingMapsGeocoding :IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            // Obtain the execution context from the service provider.  
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
            // The InputParameters collection contains all the data passed in the message request.  
            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            { // Obtain the target entity from the input parameters.  
                Entity entity = (Entity)context.InputParameters["Target"] as Entity;
                //  Entity contactimage = (Entity)context.PreEntityImages["Target"];
                if (entity.LogicalName != "contact") // if record is not created in contact entity then return
                    return;
                try
                {   // get record for which duplicate detection will work
                    ColumnSet col = new ColumnSet("address1_line1");
                    //ColumnSet RecordCols = new ColumnSet(new String[] { "address1_line1" });
                    Guid id = new Guid("03c52698-141b-ec11-b6e6-0022482988ce");
                    var contact = service.Retrieve("contact", id, col);
                    // String strAddressLine1 = String.Empty;
                    var strAddressLine1 = contact.Attributes["emailaddress1"].ToString();

                }
                catch (Exception ex)
                {
                    throw new InvalidPluginExecutionException("An error occurred in Bing Maps Coordinates Plugin.", ex);
                }
            }
            else
            {
                return;
            }
        }
       
    }
}
