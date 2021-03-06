//using System.Net.Http;
//using System.Threading;
//using System.Threading.Tasks;

//namespace BlazorServerApp.Authorization
//{
//    public class AuthorizationMessageHandler : DelegatingHandler
//    {
//        private readonly ITokenService tokenService;

//        public AuthorizationMessageHandler(ITokenService tokenService)
//        {
//            this.tokenService = tokenService;
//        }

//        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
//        {
//            var accessToken = tokenService.GetAccessToken();

//            request.Headers.Add("Authorization", $"Bearer {accessToken}");

//            var response = await base.SendAsync(request, cancellationToken);

//            return response;
//        }
//    }
//}
