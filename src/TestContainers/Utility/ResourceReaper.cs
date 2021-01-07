using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;

namespace TestContainers.Utility
{
    public sealed class ResourceReaper
    {
        private ILogger _logger;

        private static readonly List<List<KeyValuePair<string, string>>> DEATH_NOTE = new List<List<KeyValuePair<string, string>>>();


        private ConcurrentDictionary<string, string> _registeredContainers = new ConcurrentDictionary<string, string>();
        private ConcurrentDictionary<string, bool> _registeredNetworks = new ConcurrentDictionary<string, bool>();
        private ConcurrentDictionary<string, bool> _registeredImages = new ConcurrentDictionary<string, bool>();
        private SemaphoreSlim _syncLock = new SemaphoreSlim(1, 1);


        private static readonly ResourceReaper _instance = new ResourceReaper();

        public static ResourceReaper Instance => _instance;

        private ResourceReaper()
        {
        }

        /**
     * Perform a cleanup.   
     */
        public async Task PerformCleanup()
        {
            await _syncLock.WaitAsync();
            try
            {
                foreach (var container in _registeredContainers)
                {
                    await StopContainer(container.Key,container.Value);
                }

                foreach (var network in _registeredNetworks.Keys)
                {
                    await RemoveNetwork(network);
                }

                foreach (var image in _registeredImages.Keys)
                {
                    await RemoveImage(image);
                }
             }
            finally
            {
                _syncLock.Release();
            }
        }

        /**
    * Register a filter to be cleaned up.
    *
    * @param filter the filter
    */
        public void RegisterFilterForCleanup(List<KeyValuePair<string,string>> filter)
        {
            //synchronized(DEATH_NOTE) {
            //    DEATH_NOTE.add(filter);
            //    DEATH_NOTE.notifyAll();
            //}
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
                    networks = await DockerClientFactory.Instance.Execute(c => c.Networks.ListNetworksAsync(new NetworksListParameters { Filters = { } }));
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
                        await DockerClientFactory.Instance.Execute(c => c.Networks.DeleteNetworkAsync(network.ID));
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
                await DockerClientFactory.Instance.Execute(c => c.Images.DeleteImageAsync(dockerImageName, new ImageDeleteParameters { Force = true }));
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Unable to delete image {dockerImageName}", dockerImageName);
            }
        }

        public async Task StopAndRemoveContainer(string containerId, string imageName)
        {
            await StopContainer(containerId, imageName);

            _registeredContainers.TryRemove(containerId, out _);
        }

        private async Task StopContainer(string containerId, string imageName)
        {
            bool running;
            try
            {
                var containerInfo = await DockerClientFactory.Instance.Execute(c => c.Containers.InspectContainerAsync(containerId));
                running = containerInfo.State != null && containerInfo.State.Running;
            }
            catch (DockerApiException e) when (e.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                //LOGGER.trace("Was going to stop container but it apparently no longer exists: {}", containerId);
                return;
            }
            catch (Exception e)
            {
                //LOGGER.trace("Error encountered when checking container for shutdown (ID: {}) - it may not have been stopped, or may already be stopped. Root cause: {}",
                //    containerId,
                //    Throwables.getRootCause(e).getMessage());
                return;
            }

            if (running)
            {
                try
                {
                    //  LOGGER.trace("Stopping container: {}", containerId);
                    await DockerClientFactory.Instance.Execute(c => c.Containers.KillContainerAsync(containerId, new ContainerKillParameters()));
                    //LOGGER.trace("Stopped container: {}", imageName);
                }
                catch (Exception e)
                {
                    //    LOGGER.trace("Error encountered shutting down container (ID: {}) - it may not have been stopped, or may already be stopped. Root cause: {}",
                    //        containerId,
                    //        Throwables.getRootCause(e).getMessage());
                }
            }

            try
            {
                var containerInfo = await DockerClientFactory.Instance.Execute(c => c.Containers.InspectContainerAsync(containerId));
            }
            catch (Exception e)
            {
                //    LOGGER.trace("Was going to remove container but it apparently no longer exists: {}", containerId);
                return;
            }

            try
            {
                //  LOGGER.trace("Removing container: {}", containerId);
                await DockerClientFactory.Instance.Execute(c => c.Containers.RemoveContainerAsync(containerId, new ContainerRemoveParameters { RemoveVolumes = true, Force = true }));
                //   LOGGER.debug("Removed container and associated volume(s): {}", imageName);
            }
            catch (Exception e)
            {
                //       LOGGER.trace("Error encountered shutting down container (ID: {}) - it may not have been stopped, or may already be stopped. Root cause: {}",
                //          containerId,
                //          Throwables.getRootCause(e).getMessage());
            }
        }
    }
}
