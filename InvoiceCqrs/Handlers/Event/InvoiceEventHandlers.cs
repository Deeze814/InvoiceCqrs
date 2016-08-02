﻿using System;
using System.Data;
using Dapper;
using InvoiceCqrs.Domain.Entities;
using InvoiceCqrs.Messages.Events.Invoices;
using InvoiceCqrs.Messages.Events.Payments;
using InvoiceCqrs.Messages.Events.Reports;
using InvoiceCqrs.Messages.Queries.Invoices;
using InvoiceCqrs.Persistence;
using InvoiceCqrs.Persistence.EventStore;
using MediatR;
using StructureMap.Building;

namespace InvoiceCqrs.Handlers.Event
{
    public class InvoiceEventHandlers : INotificationHandler<InvoiceCreated>, INotificationHandler<LineItemAdded>, INotificationHandler<PaymentApplied>,
        INotificationHandler<PaymentUnapplied>, INotificationHandler<PaymentReceived>, INotificationHandler<InvoiceBalanceUpdated>,
        INotificationHandler<ReportPrinted>
    {
        private readonly IMediator _Mediator;
        private readonly IDbConnection _DbConnection;
        private readonly Stream _InvoiceStream;

        public InvoiceEventHandlers(IMediator mediator, Store store, IDbConnection dbConnection)
        {
            _Mediator = mediator;
            _DbConnection = dbConnection;
            _InvoiceStream = store.Open(Streams.Invoices);
        }

        public void Handle(InvoiceCreated notification)
        {
            var invoice = new Invoice();
            notification.Apply(invoice);

            const string query =
                "INSERT INTO Accounting.Invoice (Id, Balance, CreatedById, InvoiceNumber, CompanyId, CreatedOn) " +
                "VALUES (@Id, @Balance, @CreatedById, @InvoiceNumber, @CompanyId, @CreatedOn)";

            _DbConnection.Execute(query, new
            {
                invoice.Id,
                invoice.Balance,
                CreatedById = invoice.CreatedBy.Id,
                invoice.InvoiceNumber,
                CompanyId = invoice.Company.Id,
                CreatedOn = notification.EventDateTime
            });
        }

        public void Handle(LineItemAdded notification)
        {
            var lineItem = new LineItem();
            notification.Apply(lineItem);

            const string query = @"
                INSERT INTO Accounting.LineItem (Id, InvoiceId, Description, Amount, IsPaid, CreatedOn, CreatedById)
                VALUES (@Id, @InvoiceId, @Description, @Amount, @IsPaid, @CreatedOn, @CreatedById)";

            _DbConnection.Execute(query, new
            {
                lineItem.Id,
                lineItem.InvoiceId,
                lineItem.Description,
                lineItem.Amount,
                lineItem.IsPaid,
                CreatedOn = notification.EventDateTime,
                CreatedById = lineItem.CreatedBy.Id
            });

        }

        public void Handle(PaymentApplied notification)
        {
            // If we needed to maintain a read-model version of the payment history
            // for an invoice, we'd probably do something here
        }

        public void Handle(PaymentUnapplied notification)
        {
            // If wee needed to maintain a read-model version of the payment history
            // for an invoice, we'd probably do something here
        }

        public void Handle(PaymentReceived notification)
        {
            if (ReadModel.Payments.ContainsKey(notification.Id))
            {
                throw new Exception($"Payment with id {notification.Id} has already been received");
            }

            var payment = new Payment();
            notification.Apply(payment);

            ReadModel.Payments[notification.Id] = payment;
        }

        public void Handle(InvoiceBalanceUpdated notification)
        {
            var invoice = _Mediator.Send(new GetInvoice {Id = notification.InvoiceId});
            notification.Apply(invoice);

            const string query = @"
                UPDATE Accounting.Invoice
                SET Balance = @Balance
                WHERE Id = @Id";

            _DbConnection.Execute(query, invoice);

            // This assumes a liability account, I'm considering invoices to 
            // be liabilities for the company that receives an invoice
            var ledgerEntry = new GeneralLedgerEntry
            {
                CreditAmount = notification.Amount > 0 ? notification.Amount : 0,
                DebitAmount = notification.Amount < 0 ? notification.Amount : 0,
                EntryDate = notification.EventDateTime,
                Id = Guid.NewGuid(),
                LineItemId = notification.LineItemId
            };

            ReadModel.GeneralLedgerEntries[ledgerEntry.Id] = ledgerEntry;
        }

        public void Handle(ReportPrinted notification)
        {
            if (notification.TableId != Table.InvoiceTableId)
            {
                // We're only interested in invoice reports, other reports
                // we could not care less about
                return;
            }

            _InvoiceStream.Write(notification.RecordId, new InvoicePrinted
            {
                InvoiceId = notification.RecordId,
                IsReprint = notification.IsReprint,
                PrintedById = notification.PrintedById,
                ReportId = notification.ReportId
            });
        }
    }
}
