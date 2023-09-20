namespace RawDeal;


public class DeckChecker
{
    public static bool CheckEntireDeck(Deck deckToCheck, List<Superstar> superstarsList)
    {
        var deckCardList = deckToCheck.GetCardList();
        var size = CheckSize(deckCardList);
        var uniques = CheckUniques(deckCardList);
        var heelFace = CheckHeelFace(deckCardList);
        var dups = CheckDups(deckCardList);
        var logo = CheckSuperStarMove(deckToCheck, superstarsList);

        return (size && uniques && heelFace && dups && logo);
    }
    private static bool CheckSize(List<Card> cardList) => cardList.Count() == 60;

    private static bool CheckUniques(List<Card> cardList)
    {
        List<Card> onlyUniqueList = new();
        foreach (var card in cardList)
        {
            if (card.Subtypes.Contains("Unique")) onlyUniqueList.Add(card);
        }
        return onlyUniqueList.Count == onlyUniqueList.Distinct().Count();
    }

    private static bool CheckHeelFace(List<Card> cardList)
    {
        bool heel = false;
        bool face = false;
        foreach (var card in cardList)
        {
            if (card.Subtypes.Contains("Heel")) heel = true;
            if (card.Subtypes.Contains("Face")) face = true;
        }
        return (!heel || !face);
    }

    private static bool CheckDups(List<Card> cardList)
    {
        List<Card> normalCards = new();
        foreach (var card in cardList)
        {
            if (!card.Subtypes.Contains("Unique") && !card.Subtypes.Contains("SetUp")) normalCards.Add(card);
        }
        
        var counts = normalCards.GroupBy(x => x.Title)
            .Select(g => new { Title = g.Key, Count = g.Count() });
        
        foreach (var countCard in counts.ToList())
            if (countCard.Count > 3)
                return false;

        return true;
    }

    private static bool CheckSuperStarMove(Deck deckToCheck, List<Superstar> superstars)
    {
        var currentSuperstar = deckToCheck.GetSuperstar();
        var cardList = deckToCheck.GetCardList();
        var cloneSuperstarList = new List<Superstar>(superstars);
        cloneSuperstarList.Remove(currentSuperstar);
        
        foreach (var card in cardList)
        {
            foreach (var super in cloneSuperstarList)
            {
                if (card.Subtypes.Contains(super.Logo)) return false;
            }
        }
        return true;
    }
}