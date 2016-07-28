﻿using System;
using InvoiceCqrs.Domain.Entities;

namespace InvoiceCqrs.Messages.Events.Payments
{
    public class PaymentReceived : IEvent<Payment>
    {
        public Guid Id { get; set; }

        public DateTime ReceivedOn { get; set; }

        public decimal Amount { get; set; }

        public DateTime EventDateTime { get; } = DateTime.Now;

        public void Apply(Payment target)
        {
            target.Id = Id;
            target.ReceivedOn = ReceivedOn;
            target.Balance = Amount;
            target.Amount = Amount;
        }
    }
}