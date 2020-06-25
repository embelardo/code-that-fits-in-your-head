/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurant.RestApi
{
    public sealed class SmtpPostOffice : IPostOffice
    {
        private readonly string userName;
        private readonly string password;

        public SmtpPostOffice(
            string host,
            int port,
            string userName,
            string password,
            string fromAddress)
        {
            Host = host;
            Port = port;
            this.userName = userName;
            this.password = password;
            FromAddress = fromAddress;
        }

        public string Host { get; }
        public int Port { get; }
        public string FromAddress { get; }

        public async Task EmailReservationCreated(Reservation reservation)
        {
            if (reservation is null)
                throw new ArgumentNullException(nameof(reservation));

            using var msg = new MailMessage(FromAddress, reservation.Email);
            msg.Subject = $"Your reservation for {reservation.Quantity}.";
            msg.Body = CreateBody(reservation);

            using var client = new SmtpClient();
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(userName, password);
            client.Host = Host;
            client.Port = Port;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.EnableSsl = true;
            await client.SendMailAsync(msg).ConfigureAwait(false);
        }

        private static string CreateBody(Reservation reservation)
        {
            var sb = new StringBuilder();

            sb.Append("Thank you for your reservation. ");
            sb.AppendLine("Here's the details about your reservation:");
            sb.AppendLine();
            sb.AppendLine($"At: {reservation.At}.");
            sb.AppendLine($"Party size: {reservation.Quantity}.");
            sb.AppendLine($"Name: {reservation.Name}.");
            sb.AppendLine($"Email: {reservation.Email}.");

            return sb.ToString();
        }

        public override bool Equals(object? obj)
        {
            return obj is SmtpPostOffice other &&
                   Host == other.Host &&
                   Port == other.Port &&
                   FromAddress == other.FromAddress &&
                   userName == other.userName &&
                   password == other.password;
                   
        }

        public override int GetHashCode()
        {
            return
                HashCode.Combine(Host, Port, FromAddress, userName, password);
        }
    }
}
