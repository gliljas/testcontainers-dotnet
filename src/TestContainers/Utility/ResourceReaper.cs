using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;

namespace TestContainers.Utility
{
    public sealed class ResourceReaper
    {
        private ILogger _logger;
        private IDockerClient _dockerClient;
        private ConcurrentDictionary<string, string> _registeredContainers = new ConcurrentDictionary<string, string>();
        private ConcurrentDictionary<string, bool> _registeredNetworks = new ConcurrentDictionary<string, bool>();
        private ConcurrentDictionary<string, bool> _registeredImages = new ConcurrentDictionary<string, bool>();
        private ResourceReaper()
        {
            _dockerClient = DockerClientFactory.Instance.Client();
        }

        private async Task RemoveNetwork(string id)
        {
            try
            {
                IList<NetworkResponse> networks;
                try
                {
                    // Try to find the network if it still exists
                    // Listing by ID first prevents docker-java logging an error if we just go blindly into removeNetworkCmd
                    networks = await _dockerClient.Networks.ListNetworksAsync(new NetworksListParameters { Filters = { } });
                }
                catch (Exception e)
                {
                    _logger.LogTrace("Error encountered when looking up network for removal (id: {id}) - it may not have been removed", id);
                    return;
                }

                // at this point networks should contain either 0 or 1 entries, depending on whether the network exists
                // using a for loop we essentially treat the network like an optional, only applying the removal if it exists
                foreach (var network in networks)
                {
                    try
                    {
                        await _dockerClient.Networks.DeleteNetworkAsync(network.ID);
                        _registeredNetworks.TryRemove(network.ID, out bool _);
                       _logger.LogDebug("Removed network: {id}", id);
                    }
                    catch (Exception e)
                    {
                        _logger.LogTrace("Error encountered removing network (name: {id}) - it may not have been removed", network.Name);
                    }
                }
            }
            finally
            {
                _registeredNetworks.TryRemove(id, out var _);
            }
        }

        public void UnregisterNetwork(string identifier)
        {
            _registeredNetworks.TryRemove(identifier, out var _);
        }

        public void UnregisterContainer(string identifier)
        {
            _registeredContainers.TryRemove(identifier, out var _);
        }

        public void RegisterImageForCleanup(string dockerImageName)
        {
            //setHook();
            _registeredImages.TryAdd(dockerImageName, true);
        }

        private async Task RemoveImage(string dockerImageName)
        {
            _logger.LogTrace("Removing image tagged {dockerImageName}", dockerImageName);
            try
            {
                await _dockerClient.Images.DeleteImageAsync(dockerImageName, new ImageDeleteParameters { Force = true});
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Unable to delete image {dockerImageName}", dockerImageName);
            }
        }
    }
}
