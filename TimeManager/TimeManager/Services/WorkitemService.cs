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
using System.Linq;
using System.Threading.Tasks;

namespace TimeManager.Services
{
    public class WorkitemService
    {
        public NetworkCredential credentials { get; set; } = new NetworkCredential("nabadani", "ikk)KKM3LT", "admsk");

        private HttpClient httpClient => new HttpClient(new HttpClientHandler() { Credentials = credentials });
        private WorkItemTrackingHttpClient wiClient => new VssConnection(new Uri(ConfigurationManager.AppSettings.Get("tfsApiEndpoint")), new VssClientCredentials(new WindowsCredential(credentials))).GetClient<WorkItemTrackingHttpClient>();
        public WorkitemService() {}

        // Get items list
        public List<WorkItem> GetItems()
        {
            Wiql query = new Wiql() { Query =
                "SELECT * " +
                "FROM workitems " +
                "WHERE [Work Item Type] In ('Task','Bug') " +
                "AND [State] = 'Active'" +
                "AND [Assigned To] = @Me" };
            List<string> queryResults = wiClient.QueryByWiqlAsync(query).Result.WorkItems.Select(wi => wi.Url).ToList();

            var requests = new List<Task<HttpResponseMessage>>();
            queryResults.ForEach(item => requests.Add(httpClient.GetAsync(item)));
            Task.WaitAll(requests.ToArray());
            var responses = requests.Select(r => JsonConvert.DeserializeObject<Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItem>(r.Result.Content.ReadAsStringAsync().Result));

            var result = responses.Select(r => new WorkItem(r)).ToList();
            return result;
        }

        // Patch changed items
        public void PatchItems(List<WorkItem> items)
        {
            var requests = new List<Task<Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItem>>();
            items.ForEach(item =>
            {
                if (item.hours > 0)
                {
                    var estimate = float.Parse(item.trackerWorkitem.Fields["Microsoft.VSTS.CMMI.Estimate"].ToString());
                    var remaining = float.Parse(item.trackerWorkitem.Fields["Microsoft.VSTS.Scheduling.RemainingWork"].ToString());
                    var completed = float.Parse(item.trackerWorkitem.Fields["Microsoft.VSTS.Scheduling.CompletedWork"].ToString());

                    JsonPatchDocument patchDocument = new JsonPatchDocument();
                    patchDocument.AddRange(new List<JsonPatchOperation>() {
                        new JsonPatchOperation() {
                            Operation = Operation.Add,
                            Path = "/fields/Microsoft.VSTS.Scheduling.RemainingWork",
                            Value = remaining - item.hours
                        },
                        new JsonPatchOperation() {
                            Operation = Operation.Add,
                            Path = "/fields/Microsoft.VSTS.Scheduling.CompletedWork",
                            Value = completed + item.hours
                        }
                    });
                    requests.Add(wiClient.UpdateWorkItemAsync(patchDocument, item.Id));
                }
            });
            Task.WaitAll(requests.ToArray());
        }
    }
}
