using System;
using System.Collections.Generic;
using System.Linq;

public record Contact(string Id, string Name, string Phone, string? Email);

public class AddressBook
{
    private readonly Dictionary<string, Contact> _contacts = new();

    public void Add(Contact contact)
    {
        if (contact == null)
        {
            throw new ArgumentNullException(nameof(contact));
        }

        // Note: This will overwrite if the ID already exists. 
        // Alternatively, throw an exception if _contacts.ContainsKey(contact.Id) is true.
        _contacts[contact.Id] = contact;
    }

    public Contact? FindById(string id)
    {
        if (id == null)
        {
            return null;
        }

        _contacts.TryGetValue(id, out Contact? contact);
        return contact;
    }

    public List<Contact> Search(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return new List<Contact>();
        }

        return _contacts.Values
            .Where(c => c.Name.Contains(query, StringComparison.OrdinalIgnoreCase) || 
                        c.Phone.Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public bool Delete(string id)
    {
        if (id == null)
        {
            return false;
        }

        return _contacts.Remove(id);
    }
}