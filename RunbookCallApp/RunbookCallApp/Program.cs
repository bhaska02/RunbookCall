using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RunbookCallApp.SCOService;
using System.Data.Services.Client;

namespace RunbookCallApp
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // Unique Runbook ID. Can be retreived from runbook designer or orchestrator.
                Guid runbookId = new Guid("41DAA7A0-8248-4EA7-8FE4-5BA44A309232");

                string serviceRoot = "http://01-PRD-SCAP01:81/Orchestrator2012/Orchestrator.svc";

                SCOService.OrchestratorContext context = new SCOService.OrchestratorContext(new Uri(serviceRoot));

                // we save these credentials in app.config.
                context.Credentials = new System.Net.NetworkCredential("svc-ORCHSVC", "6N!FhNx$pJ35nu", "g4saas");

                // Retrieve parameters for the runbook
                var runbookParams = context.RunbookParameters.Where(runbookParam => runbookParam.RunbookId == runbookId && runbookParam.Direction == "In");

                // This is sample parameter to trigger Study Load Runbook
                Dictionary<string, string> parameterValues = new Dictionary<string, string>();
                parameterValues.Add("DeploymentID", "27");
                parameterValues.Add("RTMS Service Address", "http://01-dev-sqldb01:5678/RMORTMS");
                parameterValues.Add("Network Path Processed", @"\\g4saas.local\Public\Work\Files_Artemis\DEV\Study_Load_Management\Processed_Files");
                parameterValues.Add("NetworkPath", @"\\g4saas.local\Public\Work\Files_Artemis\DEV\Study_Load_Management\Incoming_Files");
                parameterValues.Add("SSIS Package File Path", @"g4saas.local\Public\Work\Files_Artemis\Packages\Study_Load\StudyLoad\bin\SPStudyLoad.dtsx");
                parameterValues.Add("StudyID", "1");
                parameterValues.Add("DB Instance", @"01-dev-sqldb01\tpo");
                parameterValues.Add("Tenant DB Name", "ST054_RedBullSA");


                // Configure the XML for the parameters
                StringBuilder parametersXml = new StringBuilder();
                if (runbookParams != null && runbookParams.Count() > 0)
                {
                    parametersXml.Append("<Data>");
                    foreach (var param in runbookParams)
                    {
                        parametersXml.AppendFormat("<Parameter><ID>{0}</ID><Value>{1}</Value></Parameter>", param.Id.ToString("B"), parameterValues[param.Name]);
                    }
                    parametersXml.Append("</Data>");

                    // Create new job and assign runbook Id and parameters.
                    Job job = new Job();
                    job.RunbookId = runbookId;
                    job.Parameters = parametersXml.ToString();
                    job.CreationTime = DateTime.Now.AddMinutes(2);
                    context.AddToJobs(job);
                    context.SaveChanges();

                    Console.WriteLine("Successfully started runbook. Job ID: {0}", job.Id.ToString());
                    Console.ReadLine();

                }
            }
            catch (DataServiceQueryException ex)
            {
                Console.WriteLine("An error occurred during query execution.", ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred during query execution.", ex.Message);
            }
        }
    }
}
