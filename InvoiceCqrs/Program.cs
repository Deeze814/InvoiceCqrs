﻿using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using InvoiceCqrs.Decorators;
using InvoiceCqrs.Persistence.EventStore;
using InvoiceCqrs.Util;
using InvoiceCqrs.Validation;
using InvoiceCqrs.Visitors.Invoices;
using MediatR;
using StructureMap;
using StructureMap.Graph;

namespace InvoiceCqrs
{
    class Program
    {
        static void Main(string[] args)
        {
            var container = new Container(cfg =>
            {
                cfg.Scan(scanner =>
                {
                    scanner.TheCallingAssembly();
                    scanner.ConnectImplementationsToTypesClosing(typeof(IRequestHandler<,>));
                    scanner.ConnectImplementationsToTypesClosing(typeof(IAsyncRequestHandler<,>));
                    scanner.ConnectImplementationsToTypesClosing(typeof(INotificationHandler<>));
                    scanner.ConnectImplementationsToTypesClosing(typeof(IAsyncNotificationHandler<>));

                    scanner.ConnectImplementationsToTypesClosing(typeof(IValidator<>));
                });

                cfg.For<SingleInstanceFactory>().Use<SingleInstanceFactory>(ctx => t => ctx.GetInstance(t));
                cfg.For<MultiInstanceFactory>().Use<MultiInstanceFactory>(ctx => t => ctx.GetAllInstances(t));
                cfg.For<IMediator>().Use<Mediator>();

                cfg.For<Store>().Use<Store>().Singleton();
                cfg.For<Application>().Use<Application>().Singleton();
                cfg.For<IInvoiceEventVisitor>().Use<InvoiceHistoryVisitor>();
                cfg.For<IGuidGenerator>().Use<SequentialGuidGenerator>();
                cfg.For<IDbConnection>().Singleton().Use(c => new SqlConnection(ConfigurationManager.ConnectionStrings["default"].ConnectionString));

                cfg.For(typeof(IRequestHandler<,>)).DecorateAllWith(typeof(CommandValidationDecorator<,>));
            });

            var app = container.GetInstance<Application>();
            app.Run();
        }
    }
}