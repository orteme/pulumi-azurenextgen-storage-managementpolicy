using System.Threading.Tasks;
using Pulumi;
using Pulumi.AzureNextGen.Resources.Latest;
using Pulumi.AzureNextGen.Storage.Latest;
using Pulumi.AzureNextGen.Storage.Latest.Inputs;

class MyStack : Stack
{
    public MyStack()
    {
        // Create an Azure Resource Group
        var resourceGroup = new ResourceGroup("resourceGroup", new ResourceGroupArgs
        {
            ResourceGroupName = "my-rg",
            Location = "WestUS"
        });

        // Create an Azure resource (Storage Account)
        var storageAccount = new StorageAccount("sa", new StorageAccountArgs
        {
            ResourceGroupName = resourceGroup.Name,
            AccountName = "pulumimgmtpolicytest",
            Location = resourceGroup.Location,
            Sku = new SkuArgs
            {
                Name = SkuName.Standard_LRS
            },
            Kind = Kind.StorageV2
        });
        
        // Create Storage Blob Container resource
        var storageBlobContainer = new BlobContainer("blob", new BlobContainerArgs
        {
            AccountName = storageAccount.Name,
            ResourceGroupName = resourceGroup.Name,
            ContainerName = "blobcontainer",
        });

        // Create Storage Management Policy resource
        var managementPolicy = new ManagementPolicy("mgmtpolicy", new ManagementPolicyArgs
        {
            ResourceGroupName = resourceGroup.Name,
            AccountName = storageAccount.Name,
            ManagementPolicyName = "blobcontainerpolicy",
            Policy = new ManagementPolicySchemaArgs
            {
                Rules = new InputList<ManagementPolicyRuleArgs>
                {
                    new ManagementPolicyRuleArgs
                    {
                        Definition = new ManagementPolicyDefinitionArgs
                        {
                            Filters = new ManagementPolicyFilterArgs
                            {
                                BlobTypes = { "blockBlob", "appendBlob" },
                                PrefixMatch = { "blobcontainer" }
                            },
                            Actions = new ManagementPolicyActionArgs
                            {
                                BaseBlob = new ManagementPolicyBaseBlobArgs
                                {
                                    Delete = new DateAfterModificationArgs
                                    {
                                        DaysAfterModificationGreaterThan = 90
                                    }
                                }
                            }
                        },
                        Enabled = true,
                        Type = "Lifecycle",
                        Name = $"blobcontainerrule",
                    }
                }
            }
        });
    }
}
