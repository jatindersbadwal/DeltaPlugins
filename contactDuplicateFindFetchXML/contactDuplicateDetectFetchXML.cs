using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//dynamics 365 crm namespaces 
using Microsoft.Xrm.Sdk;
using System.ServiceModel;
using Microsoft.Xrm.Sdk.Query;
namespace contactDuplicateDetection
{
    public class contactDuplicateDetectFetchXML : IPlugin
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
                    ColumnSet RecordCols = new ColumnSet(new String[] { "firstname", "lastname" });
                    var contact = service.Retrieve("contact", entity.Id, RecordCols);
                    String strContact = String.Empty;
                    strContact = (contact.Attributes["firstname"].ToString() + contact.Attributes["lastname"].ToString()).ToLower();
                    //
                    //throw new InvalidPluginExecutionException(strContact);
                    string fetchquery = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                  <entity name='contact'>
                                    <attribute name='firstname' />
                                    <attribute name='lastname' />
                                  </entity>
                                </fetch>";
                    EntityCollection contactCollection = service.RetrieveMultiple(new FetchExpression(fetchquery));
                    String strCollection = string.Empty;
                    int counter = 0;
                    foreach (Entity record in contactCollection.Entities)
                    {
                        strCollection = (record.Attributes["firstname"].ToString() + record.Attributes["lastname"].ToString()).ToLower();
                        if (strCollection == strContact)
                        {
                            counter = counter + 1;
                            Entity updContact = new Entity("contact");
                            //string id = entity.Id.ToString();
                            updContact = service.Retrieve(updContact.LogicalName, entity.Id, new ColumnSet(true));
                            updContact["new_noofduplicates"] = counter.ToString();
                            updContact["new_duplicaterecord"] = true;
                            service.Update(updContact);
                        }
                    };
                }
                catch (Exception ex)
                {
                    throw new InvalidPluginExecutionException("An error occurred in contactDuplicateDetection Plugin.", ex);
                }
            }
            else
            {
                return;
            }
        }
    }
}

