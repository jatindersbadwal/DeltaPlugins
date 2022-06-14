using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm;
using Microsoft.Xrm.Sdk.Query;

namespace latestPhoneCallChecker
{
    public class latestPhoneCallChecker : IPlugin
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
                Entity entity = (Entity)context.InputParameters["Target"];
                if (entity.LogicalName != "phonecall") // if record is not created in activity entity then return
                {
                    return;
                }
                 // try
                {  // Get Activity record GUID
                    var activityId = Guid.Empty;
                    activityId = entity.Id;
                    DateTime noFirstCall = DateTime.MinValue;
                    //DateTime latestCallContact = DateTime.MinValue;
                   // DateTime ActualPhoneCall = DateTime.UtcNow;
                    var regardingEntity = (EntityReference)entity.Attributes["regardingobjectid"];
                    var regardingEntityId = regardingEntity.Id;
                    var regardingEntityType = regardingEntity.LogicalName;
                    if (regardingEntity == null && regardingEntityType != "contact")
                    {
                        return;
                    }
                    else
                    {
                        //get contact record related to the phone call activity
                        ColumnSet RecordCols = new ColumnSet("new_firstphonecall", "new_latestphonecall");
                        Entity contact = service.Retrieve("contact", regardingEntityId, RecordCols);
                        DateTime firstCallContact;
                        DateTime latestCallContact;
                        DateTime ActualPhoneCall;
                        
                        if (!(contact.Attributes["new_firstphonecall"].Equals(null)))
                        {
                            firstCallContact = (DateTime)contact.Attributes["new_firstphonecall"];
                        }
                        else
                        {
                            firstCallContact = noFirstCall;
                        }

                        if (!(contact.Attributes["new_latestphonecall"].Equals(null)))
                        {
                            latestCallContact = (DateTime)contact.Attributes["new_latestphonecall"];
                        }
                        else
                        {
                            latestCallContact = noFirstCall;
                        }
                        //create condition 1 regarding the phone record
                        ConditionExpression condition1 = new ConditionExpression();
                        condition1.AttributeName = "regardingobjectid";
                        condition1.Operator = ConditionOperator.Equal;
                        condition1.Values.Add(regardingEntityId);

                        // create condition 2 regarding phone call status as complete
                        ConditionExpression condition2 = new ConditionExpression();
                        condition2.AttributeName = "statecode";
                        condition2.Operator = ConditionOperator.Equal;
                        condition2.Values.Add(1);

                        //add conditions into Filter Expression
                        FilterExpression RegardingContactFilter = new FilterExpression();
                        RegardingContactFilter.Conditions.Add(condition1);
                        RegardingContactFilter.Conditions.Add(condition2);
                        //get all phone records related to the contact record using filter expression

                        QueryExpression query = new QueryExpression();
                        query.EntityName = "phonecall";
                        ColumnSet col = new ColumnSet("actualend");
                        query.ColumnSet = col;
                        query.Criteria.AddFilter(RegardingContactFilter);
                        // Retrieve multiple records using retrieve multiple and query expression         
                        EntityCollection phoneCallCollection = service.RetrieveMultiple(query);


                        foreach (Entity record in phoneCallCollection.Entities)
                        {
                     
                            if ((record.Attributes["actualend"].Equals(null)))
                            {
                                ActualPhoneCall = noFirstCall; 
                            }
                            else
                            {
                                ActualPhoneCall = (DateTime)record.Attributes["actualend"];
                            }
                      
                        // one scenario pending when 1st call will happen and will be updated, then the first and latest call dates must be
                        // updated in contact entity fields
                        // for first call created but not completed ever and contact fields are null
                        if (firstCallContact == noFirstCall)
                        {
                            Entity updContactFirstCall = new Entity("contact");
                            updContactFirstCall = service.Retrieve(updContactFirstCall.LogicalName, regardingEntityId, new ColumnSet("new_phonecallcheck", "new_firstphonecall", "new_latestphonecall"));
                            updContactFirstCall["new_firstphonecall"] = ActualPhoneCall;
                            updContactFirstCall["new_latestphonecall"] = ActualPhoneCall;
                            updContactFirstCall["new_phonecallcheck"] = true;
                            service.Update(updContactFirstCall);
                        } 
        
                        else if (latestCallContact < ActualPhoneCall)
                        {
                            Entity updContactLastCall = new Entity("contact");
                            updContactLastCall = service.Retrieve(updContactLastCall.LogicalName, regardingEntityId, new ColumnSet("new_latestphonecall","jobtitle"));
                            updContactLastCall["jobtitle"] = ActualPhoneCall.ToString();
                            updContactLastCall["new_latestphonecall"] = ActualPhoneCall;
                            service.Update(updContactLastCall);
                        }
                        else
                            {                           
                            }
                        };
                    }
                }
                // catch (Exception)
                //{
                //    throw new InvalidPluginExecutionException("An error occurred in latestPhoneCallChecker Plugin.");
                //}
            }
            else
            {
                return;
            }
        }
    }
}

