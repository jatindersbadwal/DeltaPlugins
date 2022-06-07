using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//dynamics 365 crm namespaces 
using Microsoft.Xrm.Sdk;
using System.ServiceModel;
using Microsoft.Xrm.Sdk.Query;

namespace contactFilteredDuplicateDetection
{
    public class contactFilteredDuplicateDetection : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            ITracingService tracingService =(ITracingService)serviceProvider.GetService(typeof(ITracingService));
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
                    ColumnSet RecordCols = new ColumnSet(new String[] { "firstname", "lastname" });
                    var contact = service.Retrieve("contact", entity.Id, RecordCols);
                    String strLastName = String.Empty;
                    String strFirstName = String.Empty;
                    String strFullName = String.Empty;
                    strLastName =  strLastName + contact.Attributes["lastname"].ToString();
                    strFirstName = contact.Attributes["firstname"].ToString();
                    strFullName = strFirstName + strLastName;
                    // create a last name condition
                    ConditionExpression condition1 = new ConditionExpression();
                    condition1.AttributeName = "lastname";
                    condition1.Operator = ConditionOperator.Equal;
                    condition1.Values.Add(strLastName);
                    // create first name condition
                    ConditionExpression condition2 = new ConditionExpression();
                    condition2.AttributeName = "firstname";
                    condition2.Operator = ConditionOperator.Equal;
                    condition2.Values.Add(strFirstName);
                    // add conditions in the filter expression

                    FilterExpression nameFilter = new FilterExpression();
                    nameFilter.Conditions.Add(condition1);
                    nameFilter.Conditions.Add(condition2);

                    // get CRM records using Query Expression technique
                    QueryExpression query = new QueryExpression();
                    query.EntityName = "contact";
                    ColumnSet col = new ColumnSet("firstname", "lastname");
                    query.ColumnSet = col;
                    query.Criteria.AddFilter(nameFilter);

                    EntityCollection contactCollection = service.RetrieveMultiple(query);
                    String strCollection = string.Empty;
                    int counter = 0;
                    foreach (Entity record in contactCollection.Entities)
                    {
                        strCollection = record.Attributes["firstname"].ToString() + record.Attributes["lastname"].ToString();
                            if (strCollection == strFullName)
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

