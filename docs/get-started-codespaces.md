# Provision Azure resources

Once you've opened the project, you can deploy its dependencies to Azure.

## Azure account setup

1. Sign up for a [free Azure account](https://azure.microsoft.com/free/) and create an Azure Subscription.
2. Check that you have the necessary permissions:

    * Your Azure account must have `Microsoft.Authorization/roleAssignments/write` permissions, such as [Role Based Access Control Administrator](https://learn.microsoft.com/azure/role-based-access-control/built-in-roles#role-based-access-control-administrator-preview), [User Access Administrator](https://learn.microsoft.com/azure/role-based-access-control/built-in-roles#user-access-administrator), or [Owner](https://learn.microsoft.com/azure/role-based-access-control/built-in-roles#owner). If you don't have subscription-level permissions, you must be granted [RBAC](https://learn.microsoft.com/azure/role-based-access-control/built-in-roles#role-based-access-control-administrator-preview) for an existing resource group and [deploy to that existing group](/docs/deploy_existing.md#resource-group).
    * Your Azure account also needs `Microsoft.Resources/deployments/write` permissions on the subscription level.

## Deploying with azd

From a Terminal window, open the folder with the clone of this repo and run the following commands.

1. Login to Azure:

    ```shell
    azd auth login
    ```

2. Provision and deploy dependencies for the project:

    ```shell
    azd provision -e <environment>
    ```

    You'll need to replace <envrionment> with an `azd` environment name (like "chat-app"), which will be used as a prefix for resources in Azure. Select a subscription from your Azure account, and select a [location where OpenAI is available](https://azure.microsoft.com/explore/global-infrastructure/products-by-region/?products=cognitive-services&regions=all) (like "francecentral"). Then it will provision the resources in your account and deploy the latest code. If you get an error or timeout with deployment, changing the location can help, as there may be availability constraints for the OpenAI resource.

3. When `azd` has finished deploying, you'll see an endpoint URI in the command output. Visit that URI, and you should see the chat app! 🎉

4. When you've made any changes to the app code, you can just run:

    ```shell
    azd deploy
    ```

## Run the application

Start the project using the `Run > Start Debugging` menu.

If using the command line, run the following from the `src` directory:

    ```shell
    dotnet run
    ```

## Using an existing deployment

In order to run this app, you need to have an Azure OpenAI account deployed (using the instructions above). After deployment, Azure OpenAI is configured for you using [User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets). If you could not run the deployment steps here, or you want to use an existing Azure OpenAI resource and deployment, open a terminal from the root of this repo and run the following

```bash
cd ./src/AIChatApp.AppHost
dotnet user-secrets set "ConnectionStrings:openai" "https://{account_name}.openai.azure.com/"
```

The value for the connection string can be found in the Keys & Endpoint section when examining your resource from the Azure portal. Alternatively, you can find the value in the Azure OpenAI Studio > Playground > Code View. An example endpoint is: https://docs-test-001.openai.azure.com/.