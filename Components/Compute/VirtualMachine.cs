﻿using System;
using Pulumi;
using Pulumi.Random;
using AzureNative = Pulumi.AzureNative;
using Pulumi.AzureNative.Network;
using Pulumi.AzureNative.Compute;
using Pulumi.AzureNative.Compute.Inputs;
using Pulumi.AzureNative.Network.Inputs;

namespace Bk.Demo.Pulumi.Components.Compute;
internal sealed class VirtualMachine : ComponentResource
{
    public Output<string> Id { get; set; }
    public Output<string> Name { get; set; }
    public Output<string> Password { get; set; }

    public VirtualMachine(
        string name,
        Input<string> resourceGroupName,
        Input<string> location,
        Input<string> vNetId,
        Input<string> subnetName,
        ComponentResourceOptions? options = null)
        : base("VirtualMachine", name, options)
    {
        var password = new RandomPassword("myVirtualMachinePassword", new RandomPasswordArgs { Length = 25 });

        var networkInterface = new NetworkInterface("myNetworkInterface", new()
        {
            NetworkInterfaceName = "myNetworkInterface",
            ResourceGroupName = resourceGroupName,
            Location = location,
            EnableAcceleratedNetworking = false,
            IpConfigurations = new NetworkInterfaceIPConfigurationArgs
            {
                Name = "MyNetworkInterfaceIpConfiguration",
                PrivateIPAllocationMethod = IPAllocationMethod.Dynamic,
                Subnet = new AzureNative.Network.Inputs.SubnetArgs
                {
                    Id = Output.Format($"{vNetId}/subnets/{subnetName}")
                }
            }
        });

        var virtualMachine = new AzureNative.Compute.VirtualMachine("myVirtualMachine", new()
        {
            VmName = "myVirtualMachine",
            ResourceGroupName = resourceGroupName,
            Location = location,
            HardwareProfile = new HardwareProfileArgs
            {
                VmSize = VirtualMachineSizeTypes.Basic_A1,
            },
            NetworkProfile = new AzureNative.Compute.Inputs.NetworkProfileArgs
            {
                NetworkInterfaces =
            {
                new NetworkInterfaceReferenceArgs
                {
                    Id = networkInterface.Id,
                },
            },
            },
            OsProfile = new OSProfileArgs
            {
                AdminPassword = password.Result,
                AdminUsername = "myVmUser",
                ComputerName = "myVirtualMachine",
            },
            StorageProfile = new StorageProfileArgs
            {
                ImageReference = new ImageReferenceArgs
                {
                    Offer = "0001-com-ubuntu-server-focal",
                    Publisher = "canonical",
                    Sku = "20_04_lts-gen2",
                    Version = "latest",
                },
                OsDisk = new OSDiskArgs
                {
                    Name = "myVmOsDisk",
                    CreateOption = DiskCreateOptionTypes.FromImage,
                    DeleteOption = DiskDeleteOptionTypes.Delete
                },
            },
        });

        Id = virtualMachine.Id;
        Name = virtualMachine.Name;
        Password = password.Result;
    }
}
