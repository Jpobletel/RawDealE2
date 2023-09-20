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
        _playerOne = AssignDeckToPlayer();
        if (_playerOne == null) return;
        _playerTwo = AssignDeckToPlayer();
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
                _view.ShowGameInfo(_playerOne.GetPlayerInfo(), _playerTwo.GetPlayerInfo());
                var optionNextPlay = _view.AskUserWhatToDoWhenHeCannotUseHisAbility();
                if (optionNextPlay == NextPlay.ShowCards)
                {
                    var optionCardSet = _view.AskUserWhatSetOfCardsHeWantsToSee();
                    if (optionCardSet == CardSet.Hand)
                    {
                        _view.ShowCards(player.GetPlayerHand());
                    }
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
    public Player AssignDeckToPlayer()
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
}