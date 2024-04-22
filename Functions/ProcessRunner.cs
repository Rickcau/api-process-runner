using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using api_process_runner.Services;
using api_process_runner.Util;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using static Azure.Core.HttpHeader;
using static Google.Protobuf.WellKnownTypes.Field.Types;
using static System.Net.WebRequestMethods;
using Microsoft.SemanticKernel.ChatCompletion;

namespace api_process_runner.Functions
{
    public class ProcessRunnerFunction
    {

        private readonly ILogger<ProcessRunnerFunction> _logger;
        private readonly Kernel _kernel;
        private readonly AIHelper _aiHelper;
        private static BlobHelper? _blobHelper;
        private static string _fileparseSummary = "";
        private static string _blobConnection = Helper.GetEnvironmentVariable("BlobConnection");

        // If we need to use Chat Completon and AI Search the below items will be needed...
        private readonly IChatCompletionService _chat;
        private readonly ChatHistory _chatHistory;
        private readonly string _aiSearchIndex = Helper.GetEnvironmentVariable("AISearchIndex");
        private readonly string _semanticSearchConfigName = Helper.GetEnvironmentVariable("AISearchSemanticConfigName");

        public ProcessRunnerFunction(ILogger<ProcessRunnerFunction> logger, Kernel kernel, IChatCompletionService chat, ChatHistory chatHistory)
        {
            _logger = logger;
            _kernel = kernel;
            _aiHelper = new AIHelper(_kernel);
            _chat = chat;
            _chatHistory = chatHistory;
            _blobHelper = new BlobHelper()
            {
                Container = "output",
                ConnectionString = _blobConnection
            };
        }


        [Function(nameof(ProcessRunnerFunction))]
        public async Task Run([BlobTrigger("input/{name}", Connection = "BlobConnection")] Stream stream, string name)
        {
            _logger.LogInformation($"C# Blob trigger function Processed blob\n Name: {name} \n");

            #region Step 1:  Process the file that was dropped in the storage account
            var fileParseService = new FileParseService(stream, name);
            // Read the file and extract the data need to continue ReformattedTranscript function will need
            // to be redesigned so it can read the inbound file contents so it can take action
            fileParseService.ReformattedTranscript(); // TBD: STUB
            // The following code will need to be repurposed for your needs
            // We need to pull the SessionId and Email from inbound file name for use later
            // If sessionID and EMail are not needed repurpose to your needs.
            fileParseService.GetSessionIdAndEmail(name);
            // Based on the refactored ReformatterTranscript function, the .Summary object would have the data 
            // that needs to be processed in the next step.
            _fileparseSummary = fileParseService.Summary;
            #endregion

            #region Process Runner lets run the first 3 steps that do not require AI
            var somevalue = "1200 Central Ave, Charlotte, NC 28808";  // This data would come from the fileParseService.Summary or via some other variable
            var processRunner = new ProcessRunnerSteps(somevalue);

            if (processRunner.RunSteps().MasterStatusCode == "PASS" )
            {
                // TBD: STUB
                Console.WriteLine("Execute the next steps which will likely include AI");
                //- Checks the payload for details needed to perform the Call Notes lookup
                //- Performs an AI vector search to find the Call Note details
                //- Checks the Call Note details to see if OTP was completed, if verification was done STOP code is set
                //- Based on the logic outline in the Call Notes will dictate if any special plugins need to be invoked
                //- JSON response is sent back to the client
            }
            #endregion
            #region Example of using the BlobHelp Class to write data from the fileParseService to an Output Container
            /* The following code is not needed here, but if you need to write anything to Storage this is how you can do it.
            string response_summary = "";
            if (_blobHelper != null && _aiHelper != null)
            {
                await using var streamSummary = new MemoryStream(Encoding.UTF8.GetBytes(fileParseService.Summary));
                await _blobHelper.WriteToBlobAsync(streamSummary, $"ReformattedTranscript#{fileParseService.SessionId}#{fileParseService.Email}#.txt");

                response_summary = await _aiHelper.GetSummaryAsync(_fileparseSummary);
                // Now we can write the Summary of the Condensed Transcript to a Blob Container 
                _blobHelper.Container = "summary";  // set the container to be the summary container
                await using var streamsummary = new MemoryStream(Encoding.UTF8.GetBytes(response_summary));
                await _blobHelper.WriteToBlobAsync(streamsummary, $"Summary#{fileParseService.SessionId}#{fileParseService.Email}#.txt");
            } */
            #endregion


            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}
