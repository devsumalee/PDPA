//using System;
//using System.Web.Mvc;
//using Sustainsys.Saml2.HttpModule;
//using Sustainsys.Saml2.Configuration;
//using Sustainsys.Saml2.WebSso;
//using System.Diagnostics.CodeAnalysis;
//using Sustainsys.Saml2.Mvc;
//using PDPA.Models;
using System;
using System.Web.Mvc;
using Sustainsys.Saml2.HttpModule;
using Sustainsys.Saml2.Configuration;
using Sustainsys.Saml2.WebSso;
using System.Diagnostics.CodeAnalysis;
using Sustainsys.Saml2.Mvc;

namespace PDPA.Controllers
{
    [AllowAnonymous]
    public class AuthController : Controller
    {
        // GET: Auth
        private static IOptions options = null;

        /// <summary>
        /// The options used by the controller. By default read from config, 
        /// but can be set.
        /// </summary>
        public static IOptions Options
        {
            get
            {
                if (options == null)
                {
                    options = Configuration.Options.FromConfiguration;
                }
                return options;
            }
            set
            {
                options = value;
            }
        }

        /// <summary>
        /// SignIn action that sends the AuthnRequest to the Idp.
        /// </summary>
        /// <returns>Redirect with sign in request</returns>
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "HandledResult")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "CommandResult")]
        public ActionResult SignIn()
        {
            var result = CommandFactory.GetCommand(CommandFactory.SignInCommandName).Run(
                Request.ToHttpRequestData(),
                Options);

            if (result.HandledResult)
            {
                throw new NotSupportedException("The MVC controller doesn't support setting CommandResult.HandledResult.");
            }

            result.ApplyCookies(Response, Options.Notifications.EmitSameSiteNone(Request.UserAgent));
            return result.ToActionResult();
            //var samlEndpoint = "https://login.microsoftonline.com/";

            //var request = new AuthRequest(
            //    "https://login.microsoftonline.com/dc6df0e3-1692-4482-aa55-729e0a5e8361/saml2", //TODO: put your app's "entity ID" here
            //    "https://pdpa.gcme.co.th" //TODO: put Assertion Consumer URL (where the provider should redirect users after authenticating)
            //    );

            ////redirect the user to the SAML provider
            //return Redirect(request.GetRedirectUrl(samlEndpoint));
        }

        /// <summary>
        /// Assertion consumer Url that accepts the incoming Saml response.
        /// </summary>
        /// <returns>Redirect to start page on success.</returns>
        /// <remarks>The action effectively accepts the SAMLResponse, but
        /// due to using common infrastructure it is read for the current
        /// http request.</remarks>
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "HandledResult")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "CommandResult")]
        public ActionResult Acs()
        {
            var result = CommandFactory.GetCommand(CommandFactory.AcsCommandName).Run(
                Request.ToHttpRequestData(),
                Options);

            if (result.HandledResult)
            {
                throw new NotSupportedException("The MVC controller doesn't support setting CommandResult.HandledResult.");
            }

            result.SignInOrOutSessionAuthenticationModule();
            result.ApplyCookies(Response, Options.Notifications.EmitSameSiteNone(Request.UserAgent));
            return result.ToActionResult();
        }

        /// <summary>
        /// Metadata of the service provider.
        /// </summary>
        /// <returns>ActionResult with Metadata</returns>
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "HandledResult")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "CommandResult")]
        public ActionResult Index()
        {
            var result = CommandFactory.GetCommand(CommandFactory.MetadataCommand).Run(
                Request.ToHttpRequestData(),
                Options);

            result.ApplyHeaders(Response);

            if (result.HandledResult)
            {
                throw new NotSupportedException("The MVC controller doesn't support setting CommandResult.HandledResult.");
            }

            return result.ToActionResult();
        }

        /// <summary>
        /// Logout locally and if Idp supports it, perform a federated logout
        /// </summary>
        /// <returns>ActionResult</returns>
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "HandledResult")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "CommandResult")]
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Logout")]
        public ActionResult Logout()
        {
            var result = CommandFactory.GetCommand(CommandFactory.LogoutCommandName)
                .Run(Request.ToHttpRequestData(), Options);

            if (result.HandledResult)
            {
                throw new NotSupportedException("The MVC controller doesn't support setting CommandResult.HandledResult.");
            }

            result.SignInOrOutSessionAuthenticationModule();
            result.ApplyCookies(Response, Options.Notifications.EmitSameSiteNone(Request.UserAgent));
            return result.ToActionResult();
        }
    }
}