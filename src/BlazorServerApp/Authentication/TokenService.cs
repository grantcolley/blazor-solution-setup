//using Microsoft.JSInterop;
//using System.Threading.Tasks;

//namespace BlazorServerApp.Authorization
//{
//    public class TokenService : ITokenService
//    {
//        private const string accessTokenKey = "access_token";

//        private IJSRuntime jsRuntime;

//        public TokenService(IJSRuntime jsRuntime)
//        {
//            this.jsRuntime = jsRuntime;
//        }

//        public async Task<string> GetAccessToken()
//        {
//            var token = await jsRuntime.InvokeAsync<string>("localStorage.getItem", accessTokenKey);
//            return token;
//        }

//        public async Task SetAccessToken(string token)
//        {
//            await jsRuntime.InvokeVoidAsync("localStorage.setItem", accessTokenKey, token);
//        }

//        public async Task Logout()
//        {
//            await jsRuntime.InvokeVoidAsync("localStorage.removeItem", accessTokenKey);
//        }
//    }
//}