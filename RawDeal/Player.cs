using RawDealView;
using RawDealView.Formatters;
using RawDealView.Options;

namespace RawDeal;

public class Player
{
    private readonly Superstar _superstar;
    private int _fortitude = 0;
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
    public int GetPlayerSuperstarValue() => _superstar.SuperstarValue;
    public List<Card> GetPlayerHand() => _hand;
    public void FirstTurn()
    {
        for (int i = 0; i < _superstar.HandSize; i++) _hand.Add(_deck.DrawCard());
    }
    public void DrawCard() => _hand.Add(_deck.DrawCard());
    public PlayerInfo GetPlayerInfo() => new(_superstar.Name, 0, _hand.Count, _deck.GetDeckSize());

    public List<string> GetStringCards(CardSet cardSet)
    {
        List<string> cardStringList = new();
        if (cardSet == CardSet.Hand)
        {
            foreach (var card in _hand) cardStringList.Add(Formatter.CardToString(card));
        }
        else if (cardSet == CardSet.RingArea)
        {
            foreach (var card in _ringArea) cardStringList.Add(Formatter.CardToString(card));
        }
        else if (cardSet == CardSet.RingsidePile)
        {
            foreach (var card in _ringside) cardStringList.Add(Formatter.CardToString(card));
        }
        return cardStringList;
    }

    public List<string> GetPlays()
    {
        List<string> plays = new();
        foreach (var card in _hand)
        {
            if (card.Types.Contains("Maneuver") || card.Types.Contains("Action"))
            {
                if (Int32.Parse(card.Fortitude) <= _fortitude)
                {
                    string playString = Formatter.PlayToString(new CardPlayInfo(card, _superstar.Name));
                    plays.Add(playString);
                }
            }
        }
        return plays;
    }
}