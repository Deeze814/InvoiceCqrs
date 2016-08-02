﻿using System.Data;
using System.Linq;
using Dapper;
using InvoiceCqrs.Domain.Entities;
using InvoiceCqrs.Domain.ValueObjects;
using InvoiceCqrs.Messages.Queries.Companies;
using MediatR;

namespace InvoiceCqrs.Handlers.Query.Companies
{
    public class GetCompanyHandler : IRequestHandler<GetCompany, Company>
    {
        private readonly IDbConnection _DbConnection;

        public GetCompanyHandler(IDbConnection dbConnection)
        {
            _DbConnection = dbConnection;
        }

        public Company Handle(GetCompany message)
        {
            // There's gotta be a better way...
            const string query =
                @"SELECT c.Id, c.Name 
                FROM Companies.Company c 
                WHERE c.Id = @CompanyId;

                SELECT c.Addr1, c.Addr2, c.City, c.State, c.ZipCode
                FROM Companies.Company c
                WHERE c.Id = @CompanyId;";

            Company company;

            using (var multi = _DbConnection.QueryMultiple(query, message))
            {
                company = multi.Read<Company>().SingleOrDefault();
                if (company != null)
                {
                    company.Address = multi.Read<Address>().Single();
                }
            }

            return company;
        }
    }
}
