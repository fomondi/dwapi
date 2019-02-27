using System.Threading;
using System.Threading.Tasks;
using Dwapi.SharedKernel.DTOs;
using Microsoft.Extensions.Hosting;

namespace Dwapi.UploadManagement.Core.Services
{
    public class BackgroundSendService : BackgroundService
    {

        public SendManifestPackageDTO DwhPackage { get; set; }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new System.NotImplementedException();
        }
    }
}