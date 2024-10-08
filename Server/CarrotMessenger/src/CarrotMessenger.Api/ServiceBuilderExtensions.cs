using CarrotMessenger.Application.Contacts.Queries.SearchContacts;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CarrotMessenger.Api;

public static class ServiceBuilderExtensions
{
    public static void MapRoutes(this WebApplication app)
    {
        app.MapGet("/contacts/search", ([FromQuery] string query, ISender sender) => Task.FromResult(sender.Send(new SearchContactsQuery(query))))
            .WithName("search");
    }
}