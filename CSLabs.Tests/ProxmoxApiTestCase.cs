using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CSLabs.Api.Models;
using CSLabs.Api.Models.HypervisorModels;
using CSLabs.Api.Proxmox;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace CSLabs.Tests
{
    public class ProxmoxApiTestCase
    {
        private ProxmoxApi client;
        private HypervisorNode _hypervisorNode;
        [SetUp]
        public void Setup()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false,  true);

            IConfiguration configuration = builder.Build();
            var apiSection = configuration.GetSection("Api");
            _hypervisorNode = new HypervisorNode
            {
                Name = apiSection["Node"],
                Hypervisor = new Hypervisor
                {
                    Host = apiSection["Host"],
                    UserName = apiSection["Username"],
                    Password = apiSection["Password"]
                }
            };
            client = new ProxmoxApi(_hypervisorNode, apiSection["Password"]);
        }

        [Test]
        public async Task TestGetTicket()
        {
            var ticketResponse = await client.GetTicket(104);
            Assert.NotNull(ticketResponse.Ticket);
            Assert.Greater(ticketResponse.Ticket.Length, 1);
        }
        
        [Test]
        public async Task TestGetVMStatus()
        {
            var ticketResponse = await client.GetVmStatus(104);
            Assert.NotNull(ticketResponse);
        }
        
        [Test]
        public async Task TestCloneTemplate()
        {
            var newVmId = await client.CloneTemplate(_hypervisorNode, 103);
            Assert.NotNull(newVmId);
        }
        
        [Test]
        public async Task GetNodeStatus()
        {
            var result = await client.GetNodeStatus();
        }

        
        [Test]
        public async Task GetInterfaces()
        {
            await client.GetBridgeIds();
        }

        [Test]
        public async Task AddBridgeToVm()
        {
            var bridgeId = await client.CreateBridge();
            await client.AddBridgeToVm(111, bridgeId);
            await client.ApplyNetworkConfiguration();
        }
        
    }
}