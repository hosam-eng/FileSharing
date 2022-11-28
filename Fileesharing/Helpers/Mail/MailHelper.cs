using System.Net.Mail;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;

namespace Fileesharing.Helpers.Mail
{

	public class MailHelper : IMailHelper
	{
		private readonly IConfiguration _config;

		public MailHelper(IConfiguration config) 
		{
			this._config = config;
		}
		public void SendMail(InputEmailMessage model)
		{
			using (SmtpClient client=new SmtpClient(_config.GetValue<string>("Mail:Host"), _config.GetValue<int>("Mail:Port")) )
			{
				var msg=new MailMessage();
				msg.To.Add(model.Email);
				msg.Subject=model.Subject;
				msg.Body=model.Body;
				msg.IsBodyHtml = true;
				msg.From = new MailAddress(_config.GetValue<string>("Mail:From"), _config.GetValue<string>("Sender"), System.Text.Encoding.UTF8);
				client.Credentials = new System.Net.NetworkCredential(_config.GetValue<string>("Mail:From"), _config.GetValue<string>("Mail:Pwd"));
				client.Send(msg);
			}
		}
	}
}
