using RawDealView;
using RawDealView.Formatters;
using RawDealView.Options;

namespace RawDeal;

public class Player
{
    private readonly Superstar _superstar;
    private int _fortitude;
    private readonly List<Card> _hand = new();
    private readonly List<Card> _ringside = new();
    private readonly List<Card> _ringArea = new();
    private readonly Deck _deck;
    public Player(Deck deck)
    {
        _deck = deck;
        _superstar = deck.GetSuperstar();
    }

    public string GetPlayerSuperstarName() => _superstar.Name;
    public string GetPlayerSuperstarAbility() => _superstar.SuperstarAbility;
    public int GetPlayerSuperstarValue() => _superstar.SuperstarValue;
    public int GetPlayerDeckSize() => _deck.GetDeckSize();
    public int GetPlayerHandSize() => _hand.Count;
    public List<Card> GetHand() => _hand;

    public void FirstTurn()
    {
        for (int i = 0; i < _superstar.HandSize; i++) _hand.Add(_deck.DrawCard());
    }

    public void DrawCard() => _hand.Add(_deck.DrawCard());
    public PlayerInfo GetPlayerInfo() => new(_superstar.Name, _fortitude, _hand.Count, _deck.GetDeckSize());

    public List<string> GetStringCards(CardSet cardSet)
    {
        List<string> cardStringList = new();
        if (cardSet == CardSet.Hand)
        {
            foreach (var card in _hand) cardStringList.Add(Formatter.CardToString(card));
        }
        else if (cardSet is CardSet.RingArea or CardSet.OpponentsRingArea)
        {
            foreach (var card in _ringArea) cardStringList.Add(Formatter.CardToString(card));
        }
        else if (cardSet is CardSet.RingsidePile or CardSet.OpponentsRingsidePile)
        {
            foreach (var card in _ringside) cardStringList.Add(Formatter.CardToString(card));
        }

        return cardStringList;
    }

    public List<Card> GetPlays()
    {
        List<Card> plays = new();
        foreach (var card in _hand)
        {
            if (card.Types.Contains("Maneuver") || card.Types.Contains("Action"))
            {
                if (Int32.Parse(card.Fortitude) <= _fortitude)
                {
                    plays.Add(card);
                }
            }
        }
        return plays;
    }

    public Card GetLastCardOfDeck()
    {
        var removedCard = _deck.DrawCard();
        return removedCard;
    }

    public void PlayCard(Card card, int index)
    {
        var handIndex = GetDeckIndexOfPlayableCard(index);
        _hand.RemoveAt(handIndex);
        _ringArea.Add(card);
        CalculateFortitude();
    }

    private void CalculateFortitude()
    {
        _fortitude = 0;
        foreach (var card in _ringArea)
        {
            _fortitude += Int32.Parse(card.Damage);
        }
    }

    public void RecieveDamage(Card discardedCard) => _ringside.Add(discardedCard);

    private int GetDeckIndexOfPlayableCard(int playableIndex)
    {
        int i = 0;
        for (var index = 0; index < _hand.Count; index++)
        {
            var card = _hand[index];
            if (card.Types.Contains("Maneuver") || card.Types.Contains("Action"))
            {
                if (Int32.Parse(card.Fortitude) <= _fortitude)
                {
                    if (i == playableIndex) return index;
                    i++;
                }
            }

        }
        return playableIndex;
    }
    public int GetRingsideCount() => _ringside.Count;

    public void MoveCardFromRingsideToBottomPile(int index)
    {
        var card = _ringside[index];
        _ringside.RemoveAt(index);
        _deck.InsertCardToTheBottomOfTheDeck(card);
    }
    
    public void MoveCardFromHandToRingside(int index)
    {
        var card = _hand[index];
        _hand.RemoveAt(index);
        _ringside.Add(card);
    }
    public void MoveCardFromRingsideToHand(int index)
    {
        var card = _ringside[index];
        _ringside.RemoveAt(index);
        _hand.Add(card);
    }
    public void MoveCardFromHandToBottomPile(int index)
    {
        var card = _hand[index];
        _hand.RemoveAt(index);
        _deck.InsertCardToTheBottomOfTheDeck(card);
    }
}

