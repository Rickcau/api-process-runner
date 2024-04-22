using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace api_process_runner.Util
{
    public class StepResult
    {
        public string? StepName { get; set; }
        public string? StatusCode { get; set; }
        public string? Message { get; set; }
    }

    public class ProcessResult
    {
        public StepResult? SiebelLookupResult { get; set; }
        public StepResult? AddressCheckResult { get; set; }
        public StepResult? GoogleSearchResult { get; set; }
        public string? MasterStatusCode { get; set; }
    }
    public class ProcessRunnerSteps
    {
        private string _address;
        #region Example useage
        // Instantiate the ProcessRunner class
        /*
            var processRunner = new ProcessRunner();

            // Run the steps
            var processResult = processRunner.RunSteps();

            // Convert the result to JSON
            var json = JsonSerializer.Serialize(processResult, new JsonSerializerOptions { WriteIndented = true });

            // Print the JSON result
            Console.WriteLine(json);
            */
        #endregion
        #region Important Notes
        // Like we will need to pass in a pointer to the file that trigger ProcessRunner
        // This data will be used to perform the lookups.  Since we do not know that that data looks like yet 
        // we will need to add that after the fact.
        #endregion
        public ProcessRunnerSteps(string address)
        {
            this._address = address;
        }

        public ProcessResult RunSteps()
        {
            var processResult = new ProcessResult();
            #region Step 1: Siebel Lookup
            processResult.SiebelLookupResult = new StepResult { StepName = "Siebel Lookup" };
            try
            {
                // Stub out Siebel Lookup
                // Replace with actual implementation
                // using the _address details that were passed in execute the logic to verify the Siebel lookup
                // Set the values to determine PASS or FAIL
                processResult.SiebelLookupResult.StatusCode = "PASS";
                processResult.SiebelLookupResult.Message = "Success";
                if (processResult.SiebelLookupResult.StatusCode == "FAIL") {
                    return processResult; // Exit and return the result
                }
            }
            catch (Exception ex)
            {
                // If there is an excepton with the API call set StatusCode to FAIL and exit
                processResult.SiebelLookupResult.StatusCode = "FAIL";
                processResult.SiebelLookupResult.Message = ex.Message;
                processResult.MasterStatusCode = "FAIL";
                return processResult;
            }
            #endregion

            #region Step 2: Address Check using GIACT
            processResult.AddressCheckResult = new StepResult { StepName = "Address Check" };
            try
            {
                // Stub out Address Check using GIACT
                // Replace with actual implementation
                // using the _address details that were passed in execute the logic to verify the Address Check using GIACT lookup
                // Set the values to determine PASS or FAIL
                processResult.AddressCheckResult.StatusCode = "PASS";
                processResult.AddressCheckResult.Message = "Success";
                if (processResult.AddressCheckResult.StatusCode == "FAIL")
                {
                    processResult.MasterStatusCode = "FAIL";
                    return processResult; // Exit and return the result
                }
            }
            catch (Exception ex)
            {
                processResult.AddressCheckResult.StatusCode = "FAIL";
                processResult.AddressCheckResult.Message = ex.Message;
                processResult.MasterStatusCode = "FAIL";
                return processResult;
            }
            #endregion

            #region Step 3: Google Address Check
            processResult.GoogleSearchResult = new StepResult { StepName = "Google Search" };
            try
            {
                // using the _address details that were passed in execute the logic to verify perform the Google search
                var googleSearchResult = PerformGoogleSearch(_address);
                processResult.GoogleSearchResult.StatusCode = googleSearchResult ? "PASS" : "FAIL";
                processResult.GoogleSearchResult.Message = googleSearchResult ? "Success" : "Failed to verify address";
                if (processResult.GoogleSearchResult.StatusCode == "FAIL")
                {
                    processResult.MasterStatusCode = "FAIL";
                    return processResult; // Exit and return the result
                }
            }
            catch (Exception ex)
            {
                processResult.GoogleSearchResult.StatusCode = "FAIL";
                processResult.GoogleSearchResult.Message = ex.Message;
                processResult.MasterStatusCode = "FAIL";
                return processResult;
            }
            #endregion

            #region Set Master Status Code to PASS or FAIL
            if (processResult.SiebelLookupResult.StatusCode == "PASS" &&
                processResult.AddressCheckResult.StatusCode == "PASS" &&
                processResult.GoogleSearchResult.StatusCode == "PASS")
            {
                processResult.MasterStatusCode = "PASS";
            }
            else
            {
                processResult.MasterStatusCode = "FAIL";
            }
            #endregion

            #region Example JSON response
            /*
                {
                    "SiebelLookupResult": {
                        "StepName": "Siebel Lookup",
                        "StatusCode": "PASS",
                        "Message": "Success"
                    },
                    "AddressCheckResult": {
                        "StepName": "Address Check",
                        "StatusCode": "PASS",
                        "Message": "Success"
                    },
                    "GoogleSearchResult": {
                        "StepName": "Google Search",
                        "StatusCode": "PASS",
                        "Message": "Success"
                    },
                    "MasterStatusCode":"PASS"
                }

            */
            #endregion
            return processResult;
        }

        #region GoogleSearch
        private bool PerformGoogleSearch(string address)
        {
            // Implement Google Search API call to verify address
            // Replace with actual implementation
            using (var client = new HttpClient())
            {
                // var address = "1600 Amphitheatre Parkway, Mountain View, CA"; // Replace with actual address
                var apiKey = "YOUR_GOOGLE_API_KEY"; // Replace with actual API key
                var url = $"https://maps.googleapis.com/maps/api/place/textsearch/json?query={address}&key={apiKey}";
                var response = client.GetAsync(url).Result;
                var content = response.Content.ReadAsStringAsync().Result;
                var jsonData = JsonDocument.Parse(content);
                var results = jsonData.RootElement.GetProperty("results");
                foreach (var result in results.EnumerateObject())
                {
                    var types = result.Value.GetProperty("types");
                    if (types.EnumerateArray().Any(t => t.GetString() == "hospital" || t.GetString() == "health"))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        #endregion
    }
}
