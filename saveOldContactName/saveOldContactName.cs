using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;



namespace saveOldContactName
{
    public class saveOldContactName: IPlugin
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
                {
                   // Define variables to store Preimage
                    string preFN = string.Empty;
                    string preLN = string.Empty;
                    // get PreImage from Context
                    if (context.PreEntityImages.Contains("NameImage") &&
                          context.PreEntityImages["NameImage"] is Entity)
                    {
                        Entity preMessageImage = (Entity)context.PreEntityImages["NameImage"];
                        // get firstname and lastname field value before database update is performed
                        preFN = (String)preMessageImage.Attributes["firstname"];
                        preLN = (String)preMessageImage.Attributes["lastname"];
                    }
                    // update the old values of firstname and lastname field in Previous Firstname and Previous Lastname field
                    Entity updContact =
                    service.Retrieve(context.PrimaryEntityName, entity.Id, new ColumnSet("new_previousfirstname", "new_previouslastname"));
                    updContact["new_previousfirstname"] = preFN;
                    updContact["new_previouslastname"] = preLN;
                    service.Update(updContact);
                     }
                catch (Exception ex)
                {
                    throw new InvalidPluginExecutionException("An error occurred in saveOldContactName Plugin.", ex);
                }
            }
            else
            {
                return;
            }
        }
    }
}
