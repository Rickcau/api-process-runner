﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.Extensions.Configuration;
using System.Net;

namespace api_process_runner.Util
{
    public class AIHelper
    {
        private Kernel _kernel;
        private string _promptSummarize = @"SUMMARIZE THE CONVERSATION IN 20 BULLET POINTS OR LESS

        SUMMARY MUST BE:
        - WORKPLACE / FAMILY SAFE NO SEXISM, RACISM OR OTHER BIAS/BIGOTRY
        - G RATED
        - IF THERE ARE ANY  INCONSISTENCIES IN THE TRANSCRIPT, DO YOUR BEST TO CALL THOSE OUT

        {{$input}}";

        private string _promptTranslation = @"Translate the input below into {{$language}}

        MAKE SURE YOU ONLY USE {{$language}}.

        {{$input}}

        Translation:";


        public AIHelper(Kernel kernel)
        {
            this._kernel = kernel;
        }

        public async Task<string> GetSummaryAsync(string transcript)
        {
            var summarizeFunction = _kernel.CreateFunctionFromPrompt(_promptSummarize, executionSettings: new OpenAIPromptExecutionSettings { MaxTokens = 100 });

            var response = await _kernel.InvokeAsync(summarizeFunction, new() { ["input"] = transcript });

            return response.GetValue<string>() ?? "";
        }

        public async Task<string> GetTranslationAsync(string content, string language)
        {
            var translationFunction = _kernel.CreateFunctionFromPrompt(_promptTranslation, executionSettings: new OpenAIPromptExecutionSettings { MaxTokens = 2000, Temperature = 0.7, TopP = 0.0 });

            var response = await _kernel.InvokeAsync(translationFunction, new() { ["input"] = content, ["language"] = language });

            return response.GetValue<string>() ?? "";
        }


    }
}
