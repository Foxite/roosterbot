using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentEmail.Core;
using FluentEmail.Core.Models;
using FluentEmail.Smtp;

namespace RoosterBot.Meta {
	internal class EmailNotificationHandler {
		private readonly EmailSettings m_Settings;
		private readonly SmtpSender m_Sender;

		public EmailNotificationHandler(NotificationService notificationService, EmailSettings settings) {
			m_Settings = settings;
			m_Sender = new SmtpSender();
			notificationService.NotificationAdded += OnNotification;
		}

		private Task OnNotification(NotificationEventArgs e) {
			return m_Sender.SendAsync(new Email() {
				Data = new EmailData() {
					FromAddress = new Address(m_Settings.SenderAddress, "RoosterBot"),
					ToAddresses = m_Settings.Recipients.Select(str => new Address(str)).ToList(),
					Subject = "RoosterBot notification",
					Body = "RoosterBot has emitted a notification. The message is as follows: " + e.Message
				}
			});
		}
	}

	internal class EmailSettings {
		public IList<string> Recipients { get; set; } = new List<string>();
		public string SenderAddress { get; set; } = "";
		public string SmtpServer    { get; set; } = "";
		public string SmtpUsername  { get; set; } = "";
		public string SmtpPassword  { get; set; } = "";

		public bool IsEmpty =>
			Recipients == null || Recipients.Count == 0 || Recipients.All(str => string.IsNullOrWhiteSpace(str)) ||
			string.IsNullOrWhiteSpace(SenderAddress) ||
			string.IsNullOrWhiteSpace(SmtpServer)    ||
			string.IsNullOrWhiteSpace(SmtpUsername)  ||
			string.IsNullOrWhiteSpace(SmtpPassword);
	}
}