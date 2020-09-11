/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurants.RestApi
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
            string fromAddress,
            IRestaurantDatabase restaurantDatabase)
        {
            Host = host;
            Port = port;
            this.userName = userName;
            this.password = password;
            FromAddress = fromAddress;
            RestaurantDatabase = restaurantDatabase;
        }

        public string Host { get; }
        public int Port { get; }
        public string FromAddress { get; }
        public IRestaurantDatabase RestaurantDatabase { get; }

        public async Task EmailReservationCreated(
            int restaurantId,
            Reservation reservation)
        {
            if (reservation is null)
                throw new ArgumentNullException(nameof(reservation));

            var r = await RestaurantDatabase.GetRestaurant(restaurantId)
                .ConfigureAwait(false);

            var subject = $"Your reservation for {r?.Name}.";
            var body = CreateBodyForCreated(reservation);
            var email = reservation.Email.ToString();

            await Send(subject, body, email).ConfigureAwait(false);
        }

        private static string CreateBodyForCreated(Reservation reservation)
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
        public async Task EmailReservationDeleted(
            int restaurantId,
            Reservation reservation)
        {
            if (reservation is null)
                throw new ArgumentNullException(nameof(reservation));

            var r = await RestaurantDatabase.GetRestaurant(restaurantId)
                .ConfigureAwait(false);

            var subject =
                $"Your reservation for {r?.Name} was cancelled.";
            var body = CreateBodyForDeleted(reservation);
            var email = reservation.Email.ToString();

            await Send(subject, body, email).ConfigureAwait(false);
        }

        private static string CreateBodyForDeleted(Reservation reservation)
        {
            var sb = new StringBuilder();

            sb.Append("Your reservation was cancelled. ");
            sb.AppendLine("Here's the details about your reservation:");
            sb.AppendLine();
            sb.AppendLine($"At: {reservation.At}.");
            sb.AppendLine($"Party size: {reservation.Quantity}.");
            sb.AppendLine($"Name: {reservation.Name}.");
            sb.AppendLine($"Email: {reservation.Email}.");

            return sb.ToString();
        }

        public async Task EmailReservationUpdating(
            int restaurantId,
            Reservation reservation)
        {
            if (reservation is null)
                throw new ArgumentNullException(nameof(reservation));

            var r = await RestaurantDatabase.GetRestaurant(restaurantId)
                .ConfigureAwait(false);

            var subject =
                $"Your reservation for {r?.Name} is changing.";
            var body = CreateBodyForUpdating(reservation);
            var email = reservation.Email.ToString();

            await Send(subject, body, email).ConfigureAwait(false);
        }

        private static string CreateBodyForUpdating(Reservation reservation)
        {
            var sb = new StringBuilder();

            sb.Append("Your reservation is changing. ");
            sb.AppendLine("Here's the details about your reservation:");
            sb.AppendLine();
            sb.AppendLine($"At: {reservation.At}.");
            sb.AppendLine($"Party size: {reservation.Quantity}.");
            sb.AppendLine($"Name: {reservation.Name}.");
            sb.AppendLine($"Email: {reservation.Email}.");

            return sb.ToString();
        }

        public async Task EmailReservationUpdated(
            int restaurantId,
            Reservation reservation)
        {
            if (reservation is null)
                throw new ArgumentNullException(nameof(reservation));

            var r = await RestaurantDatabase.GetRestaurant(restaurantId)
                .ConfigureAwait(false);

            var subject =
                $"Your reservation for {r?.Name} changed.";
            var body = CreateBodyForUpdated(reservation);
            var email = reservation.Email.ToString();

            await Send(subject, body, email).ConfigureAwait(false);
        }

        private static string CreateBodyForUpdated(Reservation reservation)
        {
            var sb = new StringBuilder();

            sb.Append("Your reservation changed. ");
            sb.AppendLine("Here's the details about your reservation:");
            sb.AppendLine();
            sb.AppendLine($"At: {reservation.At}.");
            sb.AppendLine($"Party size: {reservation.Quantity}.");
            sb.AppendLine($"Name: {reservation.Name}.");
            sb.AppendLine($"Email: {reservation.Email}.");

            return sb.ToString();
        }

        private async Task Send(string subject, string body, string email)
        {
            using var msg = new MailMessage(FromAddress, email);
            msg.Subject = subject;
            msg.Body = body;
            using var client = new SmtpClient();
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(userName, password);
            client.Host = Host;
            client.Port = Port;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.EnableSsl = true;
            await client.SendMailAsync(msg).ConfigureAwait(false);
        }

        public override bool Equals(object? obj)
        {
            return obj is SmtpPostOffice other &&
                   Host == other.Host &&
                   Port == other.Port &&
                   FromAddress == other.FromAddress &&
                   userName == other.userName &&
                   password == other.password &&
                   Equals(RestaurantDatabase, other.RestaurantDatabase);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                Host,
                Port,
                FromAddress,
                userName,
                password,
                RestaurantDatabase);
        }
    }
}
