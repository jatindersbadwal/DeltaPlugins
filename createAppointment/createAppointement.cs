using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//dynamics 365 crm namespaces 
using Microsoft.Xrm.Sdk;
using System.ServiceModel;

namespace createAppointment
{
    public class createAppointement : IPlugin
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
                    Entity rendezvous = new Entity("appointment");

                    rendezvous["subject"] = "First Appointment with the customer.";
                    rendezvous["description"] = "Check if there are any new issues that need resolution.";
                    rendezvous["scheduledstart"] = DateTime.Now.AddDays(6);
                    rendezvous["scheduledend"] = DateTime.Now.AddDays(7);
                    rendezvous["category"] = context.PrimaryEntityName;

                    // Refer to the account in the task activity.
                    if (context.OutputParameters.Contains("id"))
                    {
                        Guid regardingobjectid = new Guid(context.OutputParameters["id"].ToString());
                        string regardingobjectidType = "account"; // account entity is target for regarding field

                        rendezvous["regardingobjectid"] =
                        new EntityReference(regardingobjectidType, regardingobjectid);

                        Guid requiredattendees = new Guid(context.OutputParameters["id"].ToString());
                        string requiredattendeesType = "account"; // account entity is target for regarding field

                        rendezvous["requiredattendees"] =
                        new EntityReference(requiredattendeesType, requiredattendees);

                    }

                    // Obtain the organization service reference which you will need for  
                    // web service calls.                  
                    IOrganizationServiceFactory serviceFactory =
                        (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                    IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

                    // Create the task in Microsoft Dynamics CRM.
                    // Not creating a tracing service in this plugin for debugbing  
                    /* 
                      tracingService.Trace("rendezvousPlugin: Creating the task activity.");
                    */
                    service.Create(rendezvous);
                }

                catch (FaultException<OrganizationServiceFault> ex)
                {
                    throw new InvalidPluginExecutionException("An error occurred in rendezvousPlugin.", ex);
                }

                catch (Exception ex)
                {
                    throw;
                    // tracingService.Trace("rendezvousPlugin: {0}", ex.ToString());
                }
            }
            else
            {
                return;
            }
        }
    }
}
