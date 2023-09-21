using RawDealView;
using Newtonsoft.Json;
using RawDealView.Options;
using static RawDeal.DeckChecker;

namespace RawDeal;

public class Game
{
    private View _view;
    private readonly string _deckFolder;
    private List<Card> _cardList;
    private List<Superstar> _superstarList;
    private Player _playerOne;
    private Player _playerTwo;
    private bool _winner = true;
    public Game(View view, string deckFolder)
    {
        _view = view;
        _deckFolder = deckFolder;
        ImportDecks();
        ImportSuperstars();
    }

    public void Play()
    {
        _playerOne = AssignDeckToPlayer(1);
        if (_playerOne == null) return;
        _playerTwo = AssignDeckToPlayer(2);
        if (_playerTwo == null) return;
        _playerOne.FirstTurn();
        _playerTwo.FirstTurn();
        List<Player> turnList = CreateTurnList();
        while (_winner)
        {
            foreach (var player in turnList)
            {
                _view.SayThatATurnBegins(player.GetPlayerSuperstarName());
                player.DrawCard();
                _view.ShowGameInfo(player.GetPlayerInfo(), GetOtherPlayer(player).GetPlayerInfo());
                var optionNextPlay = _view.AskUserWhatToDoWhenHeCannotUseHisAbility();
                
                while (optionNextPlay is not (NextPlay.EndTurn or NextPlay.GiveUp))
                {
                    if (optionNextPlay == NextPlay.ShowCards)
                    {
                        var optionCardSet = _view.AskUserWhatSetOfCardsHeWantsToSee();
                        if (optionCardSet == CardSet.OpponentsRingArea || optionCardSet == CardSet.OpponentsRingsidePile)
                        {
                            _view.ShowCards(GetOtherPlayer(player).GetStringCards(optionCardSet));
                        }
                    }

                    if (optionNextPlay == NextPlay.PlayCard )
                    {
                        var optionNextPlaySet = _view.AskUserToSelectAPlay(player.GetPlays());
                    }
                    _view.ShowGameInfo(_playerOne.GetPlayerInfo(), _playerTwo.GetPlayerInfo());
                    optionNextPlay = _view.AskUserWhatToDoWhenHeCannotUseHisAbility();
                }

                if (optionNextPlay == NextPlay.GiveUp)
                {
                    _winner = false;
                    if (player == _playerOne) _view.CongratulateWinner(_playerTwo.GetPlayerSuperstarName());
                    else _view.CongratulateWinner(_playerTwo.GetPlayerSuperstarName());
                    break;
                        
                }
            }
        }
        
    }
    
    public List<Player> CreateTurnList()
    {
        if (_playerOne.GetPlayerSuperstarValue() >= _playerTwo.GetPlayerSuperstarValue())
            return new List<Player> { _playerOne, _playerTwo };
        return new List<Player> { _playerTwo, _playerOne };
    }
    public Player AssignDeckToPlayer(int playerNumber )
    {
        string deckRoot = _view.AskUserToSelectDeck(_deckFolder);
        IEnumerable<string> enumerableList = File.ReadLines(deckRoot, System.Text.Encoding.UTF8);
        Deck deckToCheck = new(enumerableList, _cardList, _superstarList);
        if (!CheckEntireDeck(deckToCheck, _superstarList))
        {
            _view.SayThatDeckIsInvalid();
            return null!;
        }
        return new(deckToCheck);
    }
    
    public void ImportDecks()
    {
        using StreamReader reader = new(Path.Combine("data", "cards.json"));
        var json = reader.ReadToEnd();
        _cardList = JsonConvert.DeserializeObject<List<Card>>(json);
    }

    public void ImportSuperstars()
    {
        using StreamReader reader = new(Path.Combine("data", "superstar.json"));
        var json = reader.ReadToEnd();
        _superstarList = JsonConvert.DeserializeObject<List<Superstar>>(json);
    }

    public Player GetOtherPlayer(Player player)
    {
        if (player == _playerOne) return _playerTwo;
        return _playerOne;
    }
}