using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vanjaro.Installer.Managers
{
    class ReleaseManager
    {
        public static IEnumerable<Release> GetReleases()
        {
            var client = new GitHubClient(new ProductHeaderValue("Vanjaro.Platform"));
            return client.Repository.Release.GetAll("vanjarosoftware", "Vanjaro.Platform").Result;
        }
        

    }
}
