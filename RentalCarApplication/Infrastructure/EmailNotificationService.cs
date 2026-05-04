using RentalCarApplication.Core.Model;
using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.Json;

namespace RentalCarApplication.Infrastructure
{
    public class EmailNotificationService
    {
        private readonly EmailSettings _settings;

        public EmailNotificationService()
        {
            _settings = LoadSettings();
        }

        public bool IsEnabled => _settings != null &&
                                 _settings.Enabled &&
                                 !string.IsNullOrWhiteSpace(_settings.Host) &&
                                 !string.IsNullOrWhiteSpace(_settings.SenderEmail) &&
                                 !string.IsNullOrWhiteSpace(_settings.SenderPassword);

        public void SendOrderStatusNotification(Order order, Car car, string statusText)
        {
            string subject = $"Статус заказа №{order.OrderId}";
            string body = BuildOrderStatusBody(order, car, statusText);
            SendEmail(order.Email, subject, body);
        }

        private void SendEmail(string toEmail, string subject, string body)
        {
            if (!IsEnabled)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(toEmail))
            {
                throw new InvalidOperationException("Email получателя не указан.");
            }

            try
            {
                MailMessage message = new MailMessage
                {
                    From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
                    Subject = subject,
                    Body = body,
                    BodyEncoding = Encoding.UTF8,
                    SubjectEncoding = Encoding.UTF8,
                    IsBodyHtml = false
                };
                message.To.Add(toEmail);

                using SmtpClient client = new SmtpClient(_settings.Host, _settings.Port)
                {
                    EnableSsl = _settings.UseSsl,
                    Credentials = new NetworkCredential(_settings.SenderEmail, _settings.SenderPassword)
                };

                client.Send(message);
            }
            catch (SmtpException ex)
            {
                throw new InvalidOperationException(GetFriendlySmtpMessage(ex), ex);
            }
        }

        private static EmailSettings LoadSettings()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "emailsettings.json");
            if (!File.Exists(path))
            {
                return new EmailSettings();
            }

            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<EmailSettings>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new EmailSettings();
        }

        private static string BuildOrderStatusBody(Order order, Car car, string statusText)
        {
            string carName = car?.Brand ?? $"Автомобиль #{order.CarId}";
            return
$@"Здравствуйте!

Статус вашего заказа №{order.OrderId} изменён.
Новый статус: {statusText}
Автомобиль: {carName}
Дата аренды: {order.RentDate:dd.MM.yyyy}
Дата возврата: {order.ReturnDate:dd.MM.yyyy}
Стоимость: {order.Price} BYN";
        }

        private static string GetFriendlySmtpMessage(SmtpException ex)
        {
            string message = ex.Message ?? string.Empty;

            if (message.Contains("5.7.0") ||
                message.Contains("Authentication Required") ||
                message.Contains("not authenticated") ||
                message.Contains("requires a secure connection"))
            {
                return "Почтовый сервер отклонил вход. Проверьте emailsettings.json: адрес отправителя, пароль приложения, SSL и порт SMTP.";
            }

            if (message.Contains("failure sending mail"))
            {
                return "Не удалось отправить письмо. Проверьте интернет-соединение и настройки SMTP.";
            }

            return $"Не удалось отправить письмо: {message}";
        }
    }
}
