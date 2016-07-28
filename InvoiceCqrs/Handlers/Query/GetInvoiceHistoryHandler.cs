﻿using System;
using System.Collections.Generic;
using System.Linq;
using InvoiceCqrs.Domain.ValueObjects;
using InvoiceCqrs.Messages.Queries;
using InvoiceCqrs.Persistence.EventStore;
using InvoiceCqrs.Visitors;
using MediatR;
using Newtonsoft.Json;

namespace InvoiceCqrs.Handlers.Query
{
    public class GetInvoiceHistoryHandler : IRequestHandler<GetInvoiceHistory, IList<EventHistoryItem>>
    {
        private readonly IInvoiceEventVisitor _InvoiceVisitor;
        private readonly Stream _Stream;

        public GetInvoiceHistoryHandler(Store store, IInvoiceEventVisitor invoiceVisitor)
        {
            _InvoiceVisitor = invoiceVisitor;
            _Stream = store.Open(Streams.Invoices);
        }

        public IList<EventHistoryItem> Handle(GetInvoiceHistory message)
        {
            var events = _Stream.Events
                .Where(evt => evt.ExternalId == message.InvoiceId)
                .Select(evt => JsonConvert.DeserializeObject(evt.JsonContent, evt.EventType) as IVisitable<IInvoiceEventVisitor>)
                .ToList();

            _InvoiceVisitor.Visit(events);

            return _InvoiceVisitor.EventHistory;
        }
    }
}