# Info
Simple Pulumi stack for demo and educational purposes.

# Prerequisites
- Pulumi
- Supported .NET versio
- Azure CLI

# How to use it:

1. Clone the repo.
2. Navigate into folder with: `Bk.Demo.Pulumi.csproj`
3. Create local state: `pulumi login file://c://pulumistate`
4. Create new stack: `pulumi stack init dev`
5. Login into Azure: `az login`
6. Select subscriproin for stack deployment: `az acount set --subscription {subscriptionId}`
7. Preview stack: `pulumi preview`
8. If there is no errors, deploy stack: `pulumi up`
9. Reveal secrets values in console output: `pulumi stack output --show-secrets`
10. Destroy stack to avoid charges: `pulumi destroy`

## Additional commands
- `pulumi config set {name} {value}` - Create new config value.
- `pulumi config set --secret {name} {value}` - Create new secret config calue.

