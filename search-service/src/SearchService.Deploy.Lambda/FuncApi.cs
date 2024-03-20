using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;

namespace SearchService.Deploy.LambdaHandlers;

public abstract class FuncApi
{
    public abstract Task<APIGatewayProxyResponse> ExecuteAsync(APIGatewayProxyRequest httpRequest);
}