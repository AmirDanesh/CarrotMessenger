using MediatR;

namespace CarrotMessenger.Application.Contacts.Queries.SearchContacts;

public record SearchContactsQuery(string Query) : IRequest<IEnumerable<ContactDto>>;