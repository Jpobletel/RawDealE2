using RawDealView;

namespace RawDeal;

public class Player
{
    private readonly Superstar _superstar;
    private readonly List<Card> _hand = new();
    private readonly List<Card> _ringside = new();
    private readonly List<Card> _ring = new();
    private View _view;
    private Deck _deck;

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
        for (int i = 0; i < _superstar.HandSize; i++)
        {
            _hand.Add(_deck.DrawCard());
        }
    }

    public void DrawCard() => _hand.Add(_deck.DrawCard());

    public void Turn()
    {
        _view.SayThatATurnBegins(_superstar.Name);
        DrawCard();
    }

    public PlayerInfo GetPlayerInfo()
    {
        return new(_superstar.Name, 0, _hand.Count, _deck.GetDeckSize());
    }

}