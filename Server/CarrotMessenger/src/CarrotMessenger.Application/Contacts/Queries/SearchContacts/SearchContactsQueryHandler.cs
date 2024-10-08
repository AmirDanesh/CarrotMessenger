using CarrotMessenger.Identity;
using MediatR;

namespace CarrotMessenger.Application.Contacts.Queries.SearchContacts;

public class SearchContactsQueryHandler(IUserRepository userRepository)
    : IRequestHandler<SearchContactsQuery, IEnumerable<ContactDto>>
{
    public async Task<IEnumerable<ContactDto>> Handle(SearchContactsQuery request, CancellationToken cancellationToken)
    {
        var contacts = await userRepository.GetByQueryAsync(request.Query);
        return contacts.Select(x => new ContactDto(x.Name, ContactStatus.Online)).ToList();
    }
}