using System.Threading.Tasks;
using CSLabsBackend.Email.ViewModels;
using CSLabsBackend.Util;
using FluentEmail.Core;

namespace CSLabsBackend.Email
{
    public static class EmailExtensions
    {
        public static async Task SendEmailVerification(this IFluentEmail email, string to,  string verificationLink)
        {
            await email
                .To(to)
                .Subject("Please Verify your email Address")
                .UsingTemplateFile("VerifyEmail.cshtml", new VerifyEmailViewModel
                {
                    VerificationLink = verificationLink
                })
                .SendAsync();
        }
    }
}