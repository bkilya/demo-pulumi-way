using Pulumi;
using AzureNative = Pulumi.AzureNative;
using Pulumi.AzureNative.Sql;
using Pulumi.AzureNative.Network;
using Pulumi.AzureNative.Network.Inputs;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.Storage;
using Pulumi.AzureNative.Storage.Inputs;
using System.Collections.Generic;
using SubnetArgs = Pulumi.AzureNative.Network.Inputs.SubnetArgs;
using Pulumi.AzureNative.Compute;
using Pulumi.AzureNative.Compute.Inputs;

return await Pulumi.Deployment.RunAsync(() =>
{
    var config = new Config("azure-native");

    var resourceGroup = new ResourceGroup("myResourceGroup", new ResourceGroupArgs
    {
        ResourceGroupName = "myResourceGroup",
        Location = config.Require("location"),
    });

    var virtualNetwork = new VirtualNetwork("myVirtualNetwork", new VirtualNetworkArgs
    {
        VirtualNetworkName = "myVirtualNetwork",
        ResourceGroupName = resourceGroup.Name,
        Location = resourceGroup.Location,
        AddressSpace = new AddressSpaceArgs
        {
            AddressPrefixes = { "10.0.0.0/16" },
        },
        Subnets = new SubnetArgs
        {
            Name = "myCoreSubnet",
            AddressPrefix = "10.0.1.0/24",
        }
    });

    var storageAccount = new StorageAccount("myStorageAccount", new StorageAccountArgs
    {
        AccountName = "mystorageaccount",
        ResourceGroupName = resourceGroup.Name,
        Location = resourceGroup.Location,
        Sku = new AzureNative.Storage.Inputs.SkuArgs { Name = AzureNative.Storage.SkuName.Standard_GRS }
    });

    var sqlServer = new Server("mySqlServer", new ServerArgs
    {
        ServerName = "mySqlServer",
        ResourceGroupName = resourceGroup.Name,
        Location = resourceGroup.Location,
        AdministratorLogin = "adminuser",
        AdministratorLoginPassword = "Password123!",
    });

    var sqlDatabase = new Database("myTestDatabase", new()
    {
        DatabaseName = "myTestDatabase",
        ResourceGroupName = resourceGroup.Name,
        Location = resourceGroup.Location,
        ServerName = sqlServer.Name,
        Sku = new AzureNative.Sql.Inputs.SkuArgs
        {
            Capacity = 2,
            Family = "Gen4",
            Name = "BC",
        },
    });

    var networkInterface = new NetworkInterface("myNetworkInterface", new()
    {
        NetworkInterfaceName = "myNetworkInterface",
        ResourceGroupName = resourceGroup.Name,
        Location = resourceGroup.Location,
        EnableAcceleratedNetworking = false,
        IpConfigurations = new NetworkInterfaceIPConfigurationArgs
        {
            Name = "MyNetworkInterfaceIpConfiguration",
            PrivateIPAllocationMethod = IPAllocationMethod.Dynamic,
            Subnet = new SubnetArgs
            {
                Id = Output.Format($"{virtualNetwork.Id}/subnets/myCoreSubnet")
            }
        }
    });

    var virtualMachine = new VirtualMachine("myVirtualMachine", new()
    {
        VmName = "myVirtualMachine",
        ResourceGroupName = resourceGroup.Name,
        Location = resourceGroup.Location,
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
            AdminPassword = "{your-password}",
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

    var storageAccountKeys = ListStorageAccountKeys.Invoke(new ListStorageAccountKeysInvokeArgs
    {
        ResourceGroupName = resourceGroup.Name,
        AccountName = storageAccount.Name
    });

    var primaryStorageKey = storageAccountKeys.Apply(accountKeys =>
    {
        var firstKey = accountKeys.Keys[0].Value;
        return Output.CreateSecret(firstKey);
    });

    // Export the primary key of the Storage Account
    return new Dictionary<string, object?>
    {
        ["primaryStorageKey"] = primaryStorageKey,
    };
});