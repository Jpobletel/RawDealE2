using RawDealView.Formatters;

namespace RawDeal;

public class CardPlayInfo : IViewablePlayInfo
{

    public IViewableCardInfo CardInfo { get; }
    public string PlayedAs { get; }
    
    public CardPlayInfo(Card cardInfo, string playedAs)
    {
        CardInfo = cardInfo;
        PlayedAs = playedAs;
    }
}
