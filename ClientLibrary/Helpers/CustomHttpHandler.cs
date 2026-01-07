using BaseLibrary.DTOs;
using ClientLibrary.Helpers;
using ClientLibrary.Services.Contracts;
using System.Net.Http.Headers;
using System.Net;

public class CustomHttpHandler(
    LocalStorageService localStorageService,
    IUserAccountService accountService) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var url = request.RequestUri!.AbsoluteUri;

        if (url.Contains("login") || url.Contains("register") || url.Contains("refresh-token"))
        {
            return await base.SendAsync(request, cancellationToken);
        }

        //Attach token BEFORE sending
        var storedToken = await localStorageService.GetToken();
        if (storedToken != null)
        {
            var session = Serializations.DeserializeJsonString<UserSession>(storedToken);
            if (!string.IsNullOrEmpty(session?.Token))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", session.Token);
            }
        }

        var response = await base.SendAsync(request, cancellationToken);

        //If not 401, return immediately
        if (response.StatusCode != HttpStatusCode.Unauthorized)
            return response;

        //Try refresh token ONCE
        if (storedToken == null) return response;

        var oldSession = Serializations.DeserializeJsonString<UserSession>(storedToken);
        if (string.IsNullOrEmpty(oldSession?.RefreshToken))
            return response;

        var refreshResult = await accountService.RefreshTokenAsync(
            new RefreshToken { Token = oldSession.RefreshToken });

        if (!refreshResult.Flag)
            return response;

        var newSession = new UserSession
        {
            Token = refreshResult.Token,
            RefreshToken = refreshResult.RefreshToken
        };

        await localStorageService.SetToken(
            Serializations.SerializableObj(newSession));

        //Retry request with new token
        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", newSession.Token);

        return await base.SendAsync(request, cancellationToken);
    }
}
