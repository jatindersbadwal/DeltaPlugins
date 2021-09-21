using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace createManyToManyRel
{
    public class manyToMany : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            //Tracing Object
            ITracingService tracing = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            //ExecutionContext Object
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            //OrganizationServiceFactory Object
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            //OrganizationService Object
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            EntityReference targetEntity = null;
            string relationshipName = string.Empty;
            EntityReferenceCollection relatedEntities = null;

            // Get the Relationship Key from context
            if (context.InputParameters.Contains("Relationship"))
                relationshipName = context.InputParameters["Relationship"].ToString();

            // Check the Relationship Name with your intended one
            if (relationshipName != "cr8d1_eMedicines_cr8d1_eDoctor_cr8d1_eDoc.Referencing")
                return;

            // Get Entity 1 reference from Target Key from context
            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is EntityReference)
                targetEntity = (EntityReference)context.InputParameters["Target"];

            Entity doctor = service.Retrieve("cr8d1_edoctor", targetEntity.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("cr8d1_department"));
            var Department = doctor.GetAttributeValue<OptionSetValue>("cr8d1_department").Value;
            //throw new InvalidPluginExecutionException(Department.ToString());

            // Get Entity 2 reference from  RelatedEntities Key from context
            if (context.InputParameters.Contains("RelatedEntities") &&
                context.InputParameters["RelatedEntities"] is EntityReferenceCollection)
            {
                relatedEntities = context.InputParameters["RelatedEntities"] as EntityReferenceCollection;
                foreach (var ent in relatedEntities)
                {
                    Entity medicine = service.Retrieve("cr8d1_emedicines", ent.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("cr8d1_category"));
                    var category = medicine.GetAttributeValue<OptionSetValue>("cr8d1_category").Value;
                    //throw new InvalidPluginExecutionException(category.ToString());
                    //if department is diabetes and medicine category is not diabetes
                    if (Department == 346630001 && category != 346630001)
                    {
                        throw new InvalidPluginExecutionException("You cannot add non-diabetic medicines for this doctor.");
                    }
                    else if (Department == 346630000 && category != 346630000)
                    {
                        //if department is urology and medicine category is not urology
                        throw new InvalidPluginExecutionException("You cannot add non-urology medicines for this doctor.");
                    }
                }
            }
        }
    }
}