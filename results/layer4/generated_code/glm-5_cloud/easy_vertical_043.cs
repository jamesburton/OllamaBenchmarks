using System;
using System.Collections.Generic;
using System.Linq;

public record GiftCard(string Code, decimal Balance, bool IsActive);

public class GiftCardWallet
{
    private readonly List<GiftCard> _cards = new List<GiftCard>();

    public void Add(GiftCard card)
    {
        _cards.Add(card);
    }

    public decimal? GetBalance(string code)
    {
        var card = _cards.FirstOrDefault(c => c.Code == code);

        if (card == null || !card.IsActive)
        {
            return null;
        }

        return card.Balance;
    }

    public bool Redeem(string code, decimal amount)
    {
        var card = _cards.FirstOrDefault(c => c.Code == code);

        if (card == null || !card.IsActive || card.Balance < amount)
        {
            return false;
        }

        // Create a new record with the updated balance since records are immutable
        var updatedCard = card with { Balance = card.Balance - amount };

        // Replace the old card with the updated one
        _cards.Remove(card);
        _cards.Add(updatedCard);

        return true;
    }
}