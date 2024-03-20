using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Newtonsoft.Json;

namespace SearchService.Deploy.LambdaHandlers;

public class Search: FuncApi
{
    public override Task<APIGatewayProxyResponse> ExecuteAsync(APIGatewayProxyRequest httpRequest)
    {
        return Task.FromResult(new APIGatewayProxyResponse
        {
            StatusCode = 200,
            Body = JsonConvert.SerializeObject(httpRequest.QueryStringParameters)
        });
    }
}
