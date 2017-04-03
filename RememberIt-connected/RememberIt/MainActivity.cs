using Android.App;
using Android.Widget;
using Android.OS;
using System;
using System.Linq;
using Android.Content;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace RememberIt
{
    [Activity(Label = "Welcome to Remember!", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : Activity
    {
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
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            AuthenticationAgentContinuationHelper.SetAuthenticationAgentContinuationEventArgs(requestCode, resultCode, data);
        }
    }
}

