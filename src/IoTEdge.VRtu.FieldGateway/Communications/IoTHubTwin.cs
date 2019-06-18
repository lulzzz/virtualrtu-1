﻿using IoTEdge.VirtualRtu.Configuration;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace IoTEdge.VRtu.FieldGateway.Communications
{
    public class IotHubTwin
    {
        public IotHubTwin()
        {
        }
        public async Task<EdgeGatewayConfiguration> GetModuleConfigAsync()
        {
            ModuleClient client = await ModuleClient.CreateFromEnvironmentAsync();
            await client.OpenAsync();
            Twin twin = await client.GetTwinAsync();
            TwinCollection collection = twin.Properties.Desired;

            if (!collection.Contains("luss") || !collection.Contains("serviceUrl"))
            {
                Console.WriteLine("Twin has no luss property");
                return null;
            }

            string luss = collection["luss"];
            string serviceUrl = collection["serviceUrl"];

            if (string.IsNullOrEmpty(luss) || string.IsNullOrEmpty(serviceUrl))
            {
                Console.WriteLine("Twin has empty luss");
                return null;
            }

            return await GetConfigurationAsync(luss, serviceUrl);
        }


        private async Task<EdgeGatewayConfiguration> GetConfigurationAsync(string luss, string url)
        {
            string requestUrl = String.Format($"{url}?luss={luss}");
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage message = await httpClient.GetAsync(requestUrl);
            if (message.StatusCode != System.Net.HttpStatusCode.OK)
            {
                Console.WriteLine("Azure provisioning function returned status code '{0}' ... must use existing file.", message.StatusCode);
                return null;
            }

            Console.WriteLine("Configuration acquired from Azure provisioning function.");
            string jsonString = await message.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<EdgeGatewayConfiguration>(jsonString);
        }

    }
}
