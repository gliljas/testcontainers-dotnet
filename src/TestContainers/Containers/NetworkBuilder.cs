using System;
using System.Collections.Generic;
using Docker.DotNet.Models;

namespace TestContainers
{
    public class NetworkBuilder
    {
        private string _driver;
        private List<Action<NetworksCreateParameters>> _networksCreateParametersModifiers = new List<Action<NetworksCreateParameters>>();

        internal NetworkBuilder Driver(string driver)
        {
            _driver = driver;
            return this;
        }

        internal Network Build()
        {
            return new Network() {
                Driver = _driver,
                NetworksCreateParametersModifiers = _networksCreateParametersModifiers
            };
        }

        internal NetworkBuilder WithCreateNetworkCmdModifier(Action<NetworksCreateParameters> modifier)
        {
            _networksCreateParametersModifiers.Add(modifier);
            return this;
        }
    }
}
