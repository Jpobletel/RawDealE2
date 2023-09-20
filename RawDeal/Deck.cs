namespace RawDeal;

public class Deck
{
    private List<Card> _cardList = new();
    private Superstar _superstar;
    
    public Deck( IEnumerable<string> enumerableList, List<Card> cardList, List<Superstar> superstarList )
    {
        CreateDeck(enumerableList, cardList, superstarList);
    }
    
    public List<Card> GetCardList() => _cardList;
    public Superstar GetSuperstar() => _superstar;
    
    public int getSuperstarValue() => _superstar.SuperstarValue;

    public void CreateDeck(IEnumerable<string> enumerableList, List<Card> cardList, List<Superstar> superstarList)
    {
        List<string> rawDeckList = enumerableList.ToList();
        string superstarName = rawDeckList.First();
        superstarName = superstarName.Replace(" (Superstar Card)", "");
        _superstar = superstarList.Find(x => x.Name == superstarName);
        
        foreach (var cardTitle in rawDeckList)
        {
            Card card = cardList.Find(x => x.Title == cardTitle);
            if (card != null) _cardList.Add(card);
        }
    }
    
    public Card DrawCard()
    {
        Card card = _cardList.Last();
        _cardList.Remove(card);
        return card;
    }
    
    public int GetDeckSize() => _cardList.Count;
    
    
}