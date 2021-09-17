using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//namespace for dynamics 365 interaction 
using Microsoft.Xrm.Sdk;
using System.ServiceModel;

namespace createTask
{
    class createtaskplugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            //code goes here
            // Obtains the tracing service, not using in this plugin as of now - JSB
            /*
            ITracingService tracingService =
            (ITracingService)serviceProvider.GetService(typeof(ITracingService)); */



            // Obtain the execution context from the service provider.  
            IPluginExecutionContext context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));

            // The InputParameters collection contains all the data passed in the message request.  
            if (context.InputParameters.Contains("Target") &&
                context.InputParameters["Target"] is Entity)
            {
                // Obtain the target entity from the input parameters.  
                Entity entity = (Entity)context.InputParameters["Target"];

                if (entity.LogicalName != "account") // if record is created in account entity then 
                    return;
                try
                {
                    // Plug-in business logic goes here.  
                    // Create a task activity to follow up with the account customer in 6-7 days. 
                    Entity followup = new Entity("task");

                    followup["subject"] = "Send e-mail to the new customer.";
                    followup["description"] = "Follow up with the customer. Check if there are any new issues that need resolution.";
                    followup["scheduledstart"] = DateTime.Now.AddDays(6);
                    followup["scheduledend"] = DateTime.Now.AddDays(7);
                    followup["category"] = context.PrimaryEntityName;

                    // Refer to the account in the task activity.
                    if (context.OutputParameters.Contains("id"))
                    {
                        Guid regardingobjectid = new Guid(context.OutputParameters["id"].ToString());
                        string regardingobjectidType = "account"; // account entity is target for regarding field

                        followup["regardingobjectid"] =
                        new EntityReference(regardingobjectidType, regardingobjectid);
                    }

                    // Obtain the organization service reference which you will need for  
                    // web service calls.                  
                    IOrganizationServiceFactory serviceFactory =
                        (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                    IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

                    // Create the task in Microsoft Dynamics CRM.
                    // Not creating a tracing service in this plugin for debugbing  
                    /* 
                      tracingService.Trace("FollowupPlugin: Creating the task activity.");
                    */
                    service.Create(followup);
                }

                catch (FaultException<OrganizationServiceFault> ex)
                {
                    throw new InvalidPluginExecutionException("An error occurred in FollowUpPlugin.", ex);
                }

                catch (Exception ex)
                {
                    throw;

                    // tracingService.Trace("FollowUpPlugin: {0}", ex.ToString());

                }
            }
            else 
            {
                return;
            }
        }
    }
}
