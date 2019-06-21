using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Text;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Newtonsoft.Json;

using Microsoft.VisualStudio.Services.Client;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using Microsoft.VisualStudio.Services.WebApi.Patch;

namespace TimeManager.Services
{
    public class WorkitemService
    {
        public NetworkCredential credentials { get; set; } = new NetworkCredential("nabadani", "ikk)KKM3LT", "admsk");
        public WorkitemService() {}

        // Get items list
        public void GetItems()
        {
            var endpoint = ConfigurationManager.AppSettings.Get("tfsApiEndpoint");
            var uri = new Uri(endpoint);
            VssConnection connection = new VssConnection(uri, new VssClientCredentials(new WindowsCredential(credentials)));
            WorkItemTrackingHttpClient witClient = connection.GetClient<WorkItemTrackingHttpClient>();

            Wiql query = new Wiql() { Query = "SELECT [Id], [Title], [State] FROM workitems WHERE [Work Item Type] In ('Task','Bug') AND [Assigned To] = @Me" };
            WorkItemQueryResult queryResults = witClient.QueryByWiqlAsync(query).Result;
        }

        // Patch changed items
        public void PatchItems()
        {
            var endpoint = ConfigurationManager.AppSettings.Get("tfsApiEndpoint");
            var uri = new Uri(endpoint);
            VssConnection connection = new VssConnection(uri, new VssClientCredentials(new WindowsCredential(credentials)));
            WorkItemTrackingHttpClient witClient = connection.GetClient<WorkItemTrackingHttpClient>();

            JsonPatchDocument patchDocument = new JsonPatchDocument();
            patchDocument.AddRange(new List<JsonPatchOperation>() {
                new JsonPatchOperation() {
                    Operation = Operation.Add,
                    Path = "/fields/Microsoft.VSTS.Scheduling.RemainingWork",
                    Value = 111
                },
                new JsonPatchOperation() {
                    Operation = Operation.Add,
                    Path = "/fields/Microsoft.VSTS.Scheduling.CompletedWork",
                    Value = 111
                }
            });
            var updated = witClient.UpdateWorkItemAsync(patchDocument, 111);
        }
    }
}
