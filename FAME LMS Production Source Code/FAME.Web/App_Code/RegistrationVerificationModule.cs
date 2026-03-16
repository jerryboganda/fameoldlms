using First_Aid_Made_Easy.Models;
using System;
using System.Web;

public class RegistrationVerificationModule : IHttpModule
{
    public void Init(HttpApplication context)
    {
        context.AcquireRequestState += OnAcquireRequestState;
    }

    public void Dispose()
    {
    }

    private static void OnAcquireRequestState(object sender, EventArgs e)
    {
        var app = sender as HttpApplication;
        var context = app?.Context;
        var request = context?.Request;

        if (request == null || !string.Equals(request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (!request.Path.EndsWith("/Account/Register", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var email = request.Form["Email"];
        if (string.IsNullOrWhiteSpace(email))
        {
            return;
        }

        if (string.Equals(request.Form["EmailConfirmed"], "true", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var verifyCode = context.Session?["VerifyCode"] as VerifyCodeVM;
        var isVerified = verifyCode != null
            && verifyCode.IsVerified
            && string.Equals(verifyCode.Email, email, StringComparison.OrdinalIgnoreCase);

        if (isVerified)
        {
            return;
        }

        var redirectUrl = "~/Account/Register?verificationRequired=1";
        var referralCode = request.Form["ReferralCode"];
        if (!string.IsNullOrWhiteSpace(referralCode))
        {
            redirectUrl += "&refCode=" + HttpUtility.UrlEncode(referralCode);
        }

        redirectUrl += "&email=" + HttpUtility.UrlEncode(email);

        context.Response.Redirect(VirtualPathUtility.ToAbsolute(redirectUrl), false);
        app.CompleteRequest();
    }
}
