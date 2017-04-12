using System;
using System.Linq;
using System.Threading.Tasks;
using System.Json;

using Android.App;
using Android.Widget;
using Android.OS;
using Android.Content;

using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Xamarin.Auth;

namespace RememberIt
{
    [Activity(Label = "Welcome to Remember!", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : Activity
    {
        // UI Scheduler
        static readonly TaskScheduler UIScheduler = TaskScheduler.FromCurrentSynchronizationContext();

        // Client ID
        public static string clientId = "2a6ce4df-f26f-4864-bb28-7f806abbcc67";
        public static string commonAuthority = "https://login.windows.net/common";

        // Redirect URI
        public static Uri returnUri = new Uri("http://rememberit-redirect");

        // Graph URI
        const string graphResourceUri = "https://graph.windows.net";
        public static string graphApiVersion = "2013-11-08";

        // AuthenticationResult will hold the result after authentication completes
        AuthenticationResult authResult = null;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our buttons from the layout resource,
            // and attach events to them
            Button loginButton = FindViewById<Button>(Resource.Id.myButton);
            loginButton.Click += delegate { StartActivity(typeof(RememberListActivity)); };


            // ADAL Authentication
            Button adalButton = FindViewById<Button>(Resource.Id.adalButton);
            adalButton.Click += async (sender, args) =>
            {
                var authContext = new AuthenticationContext(commonAuthority);
                if (authContext.TokenCache.ReadItems().Count() > 0)
                {
                    authContext = new AuthenticationContext(authContext.TokenCache.ReadItems().First().Authority);
                }
                authResult = await authContext.AcquireTokenAsync(graphResourceUri, clientId, returnUri, new PlatformParameters(this));

                if ((authResult != null) && !string.IsNullOrEmpty(authResult.AccessToken))
                {
                    Toast.MakeText(this, string.Format("Welcome {0} {1}.", authResult.UserInfo.GivenName,
                                                       authResult.UserInfo.FamilyName), ToastLength.Long).Show();
                    StartActivity(typeof(RememberListActivity));
                }
            };

            // Facebook Authentication
            Button fbButton = FindViewById<Button>(Resource.Id.fbButton);
            fbButton.Click += (sender, args) =>
            {
                LoginToFacebook(true);
            };
        }

        void LoginToFacebook(bool allowCancel)
        {
            var auth = new OAuth2Authenticator(
                clientId: "395508370820207", // APP ID retrieved from Facebook created Application
                scope: "email",  // the scopes for the API
                authorizeUrl: new Uri("https://m.facebook.com/dialog/oauth/"),  // the auth URL for the service
                redirectUrl: new Uri("https://www.facebook.com/connect/login_success.html"));

            // To allow cancel of the authentication
            auth.AllowCancel = allowCancel;

            // If authorization succeeds or is canceled, .Completed will be fired.
            auth.Completed += (s, ee) =>
            {
                if (!ee.IsAuthenticated)
                {
                    // Not Authenticated
                    var builder = new AlertDialog.Builder(this);
                    builder.SetMessage("Not Authenticated");
                    builder.SetPositiveButton("Ok", (o, e) => { });
                    builder.Create().Show();
                    return;
                }

                // Now that we're logged in, make a OAuth2 request to get the user's info.
                var request = new OAuth2Request("GET", new Uri("https://graph.facebook.com/me"), null, ee.Account);
                request.GetResponseAsync().ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        // Problem in authentication
                        var builder = new AlertDialog.Builder(this);
                        builder.SetTitle("Error");
                        builder.SetMessage(t.Exception.Flatten().InnerException.ToString());
                        builder.SetPositiveButton("Ok", (o, e) => { });
                        builder.Create().Show();
                    }
                    else if (!t.IsCanceled)
                    {
                        // Authentication succeeded, show the toast then go the main activity
                        var obj = JsonValue.Parse(t.Result.GetResponseText());
                        Toast.MakeText(this, string.Format("Welcome {0}.", obj["name"]), ToastLength.Long).Show();
                        StartActivity(typeof(RememberListActivity));
                    }


                }, UIScheduler);
            };

            var intent = auth.GetUI(this);
            StartActivity(intent);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            AuthenticationAgentContinuationHelper.SetAuthenticationAgentContinuationEventArgs(requestCode, resultCode, data);
        }
    }
}

