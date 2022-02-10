using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public static class Auth0VoodooStoreApp
{
    const string AppName = "VoodooStore";
    const string Domain = "voodoo.eu.auth0.com";
    const string ClientID = "555Y36IbqE7BzzOX9anf9mSa6N9cUOyg";

    //static string _audienceURL = $"https://{Domain}/api/";
    static string _redirectURL = "http://localhost:3000/";
    static string _state;

    //temp 
    const string code = "yE3X6KwFdCfps5QvI2dtyPkyC4YMo9RCO3SV__ARvNO5p&state=ar56-52KRaZREU2VajWEz2ucFC9y-1NyRRTbRakX5SM";

    static string AuthorizeURL => $"https://{Domain}/authorize?response_type=code&scope=openid%20profile&client_id={ClientID}&redirect_uri={_redirectURL}&state={_state}";

    [MenuItem("Voodoo/Auth0 Login")]
    public static async void Login()
    {
        // Generates state values.
        _state = randomDataBase64url(32);

        // Creates a redirect URI using an available port on the loopback address.
        Debug.Log("redirect URI: " + _redirectURL);

        // Creates an HttpListener to listen for requests on that redirect URI.
        var http = new HttpListener();
        http.Prefixes.Add(_redirectURL);
        http.Start();
        Debug.Log("Listening..");

        // Opens request in the browser.
        System.Diagnostics.Process.Start(AuthorizeURL);

        // Waits for the OAuth authorization response.
        var context = await http.GetContextAsync();

        // Sends an HTTP response to the browser.
        var response = context.Response;
        string responseString = string.Format("<html><head><meta http-equiv='refresh' content='10;url=https://developer.okta.com'></head><body>Please return to the app.</body></html>");
        var buffer = Encoding.UTF8.GetBytes(responseString);
        response.ContentLength64 = buffer.Length;
        var responseOutput = response.OutputStream;
        Task responseTask = responseOutput.WriteAsync(buffer, 0, buffer.Length).ContinueWith((task) =>
        {
            responseOutput.Close();
            http.Stop();
            Console.WriteLine("HTTP server stopped.");
        });

        // Checks for errors.
        if (context.Request.QueryString.Get("error") != null)
        {
            Debug.Log(String.Format("OAuth authorization error: {0}.", context.Request.QueryString.Get("error")));
            return;
        }

        if (context.Request.QueryString.Get("code") == null
            || context.Request.QueryString.Get("state") == null)
        {
            Debug.Log("Malformed authorization response. " + context.Request.QueryString);
            return;
        }

        // extracts the code
        var code = context.Request.QueryString.Get("code");
        var incoming_state = context.Request.QueryString.Get("state");

        // Compares the receieved state to the expected value, to ensure that
        // this app made the request which resulted in authorization.
        if (incoming_state != _state)
        {
            Debug.Log(String.Format("Received request with invalid state ({0})", incoming_state));
            return;
        }

        Debug.Log("Authorization code: " + code);

        //// Starts the code exchange at the Token Endpoint.
        //performCodeExchange(code, code_verifier, redirectURI);
        
        
        //var request = new UnityWebRequest(authorizationUrl);
        //await request.SendWebRequest();
        //if (string.IsNullOrEmpty(request.error) == false)
        //{
        //    Debug.Log(request.error);
        //}

        //Debug.Log(request.downloadHandler.text);
    }

    // ref http://stackoverflow.com/a/3978040
    public static int GetRandomUnusedPort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    /// <summary>
    /// Returns URI-safe data with a given input length.
    /// </summary>
    /// <param name="length">Input length (nb. output will be longer)</param>
    /// <returns></returns>
    public static string randomDataBase64url(uint length)
    {
        RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
        byte[] bytes = new byte[length];
        rng.GetBytes(bytes);
        return base64urlencodeNoPadding(bytes);
    }

    /// <summary>
    /// Base64url no-padding encodes the given input buffer.
    /// </summary>
    /// <param name="buffer"></param>
    /// <returns></returns>
    public static string base64urlencodeNoPadding(byte[] buffer)
    {
        string base64 = Convert.ToBase64String(buffer);

        // Converts base64 to base64url.
        base64 = base64.Replace("+", "-");
        base64 = base64.Replace("/", "_");
        // Strips padding.
        base64 = base64.Replace("=", "");

        return base64;
    }

    //async void performCodeExchange(string code, string code_verifier, string redirectURI)
    //{
    //    Debug.Log("Exchanging code for tokens...");

    //    // builds the  request
    //    string tokenRequestBody = string.Format("code={0}&redirect_uri={1}&client_id={2}&code_verifier={3}&client_secret={4}&scope=&grant_type=authorization_code",
    //        code,
    //        System.Uri.EscapeDataString(redirectURI),
    //        clientID,
    //        code_verifier,
    //        clientSecret
    //        );

    //    // sends the request
    //    HttpWebRequest tokenRequest = (HttpWebRequest)WebRequest.Create(tokenEndpoint);
    //    tokenRequest.Method = "POST";
    //    tokenRequest.ContentType = "application/x-www-form-urlencoded";
    //    //tokenRequest.Accept = "Accept=application/json;charset=UTF-8";
    //    byte[] _byteVersion = Encoding.ASCII.GetBytes(tokenRequestBody);
    //    tokenRequest.ContentLength = _byteVersion.Length;
    //    Stream stream = tokenRequest.GetRequestStream();
    //    await stream.WriteAsync(_byteVersion, 0, _byteVersion.Length);
    //    stream.Close();

    //    try
    //    {
    //        // gets the response
    //        WebResponse tokenResponse = await tokenRequest.GetResponseAsync();
    //        using (StreamReader reader = new StreamReader(tokenResponse.GetResponseStream()))
    //        {
    //            // reads response body
    //            string responseText = await reader.ReadToEndAsync();
    //            Console.WriteLine(responseText);

    //            // converts to dictionary
    //            Dictionary<string, string> tokenEndpointDecoded = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseText);

    //            string access_token = tokenEndpointDecoded["access_token"];
    //            Debug.Log(access_token);
    //            userinfoCall(access_token);
    //        }
    //    }
    //    catch (WebException ex)
    //    {
    //        if (ex.Status == WebExceptionStatus.ProtocolError)
    //        {
    //            var response = ex.Response as HttpWebResponse;
    //            if (response != null)
    //            {
    //                output("HTTP: " + response.StatusCode);
    //                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
    //                {
    //                    // reads response body
    //                    string responseText = await reader.ReadToEndAsync();
    //                    output(responseText);
    //                }
    //            }

    //        }
    //    }
    //}

    //async void userinfoCall(string access_token)
    //{
    //    output("Making API Call to Userinfo...");

    //    // sends the request
    //    HttpWebRequest userinfoRequest = (HttpWebRequest)WebRequest.Create(userInfoEndpoint);
    //    userinfoRequest.Method = "GET";
    //    userinfoRequest.Headers.Add(string.Format("Authorization: Bearer {0}", access_token));
    //    userinfoRequest.ContentType = "application/x-www-form-urlencoded";
    //    //userinfoRequest.Accept = "Accept=text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";

    //    // gets the response
    //    WebResponse userinfoResponse = await userinfoRequest.GetResponseAsync();
    //    using (StreamReader userinfoResponseReader = new StreamReader(userinfoResponse.GetResponseStream()))
    //    {
    //        // reads response body
    //        string userinfoResponseText = await userinfoResponseReader.ReadToEndAsync();
    //        output(userinfoResponseText);
    //    }
    //}

}
