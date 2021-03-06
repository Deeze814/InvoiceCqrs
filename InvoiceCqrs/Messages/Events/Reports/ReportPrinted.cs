﻿using System;
using InvoiceCqrs.Domain.Entities;

namespace InvoiceCqrs.Messages.Events.Reports
{
    public class ReportPrinted : IEvent
    {
        public DateTime EventDate { get; } = DateTime.UtcNow;

        public bool IsReprint { get; set; }

        public Guid RecordId { get; set; }

        public Guid ReportId { get; set; }

        public Guid TableId { get; set; }

        public Guid PrintedById { get; set; }
    }
}
