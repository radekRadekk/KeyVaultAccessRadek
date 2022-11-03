using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace KeyVaultAccessRadek;

public static class KeyVaultAccessRadek
{
    [FunctionName("KeyVaultAccessRadek")]
    public static async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]
        HttpRequest req, ILogger log)
    {
        string name = req.Query["name"];

        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        dynamic data = JsonConvert.DeserializeObject(requestBody);
        name = name ?? data?.name;

        var secret = await GetSecret(name);
        
        return secret != null
            ? (ActionResult) new OkObjectResult(secret)
            : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
    }

    public static async Task<string> GetSecret(string name)
    {
        const string secretName = "SecretNumber";
        const string keyVaultName = "KeyVaultRadek";
        const string kvUri = $"https://{keyVaultName}.vault.azure.net";

        try
        {
            var client = new SecretClient(new Uri(kvUri), new DefaultAzureCredential());

            var secret = await client.GetSecretAsync(secretName);
            return secret.Value.Value;
        }
        catch (Exception)
        {
            return null;
        }
    }
}