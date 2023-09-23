using RawDealView;
using Newtonsoft.Json;
using RawDealView.Formatters;
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
                _view.ShowGameInfo(player.GetPlayerInfo(), GetOtherPlayer(player).GetPlayerInfo());
                var optionNextPlay = _view.AskUserWhatToDoWhenHeCannotUseHisAbility();
                
                while (optionNextPlay is not (NextPlay.EndTurn or NextPlay.GiveUp))
                {
                    if (optionNextPlay == NextPlay.ShowCards)
                    {
                        var optionCardSet = _view.AskUserWhatSetOfCardsHeWantsToSee();
                        if (optionCardSet is CardSet.OpponentsRingArea or CardSet.OpponentsRingsidePile)
                        {
                            _view.ShowCards(GetOtherPlayer(player).GetStringCards(optionCardSet));
                        }
                        else
                        {
                            _view.ShowCards(player.GetStringCards(optionCardSet));
                        }
                    }

                    else if (optionNextPlay == NextPlay.PlayCard )
                    {
                        var playList = player.GetPlays();
                        var stringPlayList = ConvertPlaysToString(playList);
                        int optionNextPlaySet = _view.AskUserToSelectAPlay(stringPlayList);
                        if (optionNextPlaySet == -1)
                        {
                            _view.ShowGameInfo(player.GetPlayerInfo(), GetOtherPlayer(player).GetPlayerInfo());
                            optionNextPlay = _view.AskUserWhatToDoWhenHeCannotUseHisAbility();
                            continue;
                        }

                        var playedCard = stringPlayList[optionNextPlaySet];
                        player.PlayCard(playList[optionNextPlaySet], optionNextPlaySet);
                        _view.SayThatPlayerIsTryingToPlayThisCard(player.GetPlayerSuperstarName() ,playedCard);
                        _view.SayThatPlayerSuccessfullyPlayedACard();
                        var damage = Int32.Parse(playList[optionNextPlaySet].Damage);
                        _view.SayThatSuperstarWillTakeSomeDamage(GetOtherPlayer(player).GetPlayerSuperstarName(),damage);
                        _winner = DamagePlayer(GetOtherPlayer(player), damage);

                    }

                    if (!_winner) break;
                    {
                        
                    }
                    _view.ShowGameInfo(player.GetPlayerInfo(), GetOtherPlayer(player).GetPlayerInfo());
                    optionNextPlay = _view.AskUserWhatToDoWhenHeCannotUseHisAbility();

                }

                if (optionNextPlay == NextPlay.GiveUp)
                {
                    _winner = false;
                    if (player == _playerOne) _view.CongratulateWinner(_playerTwo.GetPlayerSuperstarName());
                    else _view.CongratulateWinner(_playerTwo.GetPlayerSuperstarName());
                    break;
                        
                }

                if (player.GetPlayerDeckSize() is 0)
                {
                    _winner = false;
                    _view.CongratulateWinner(GetOtherPlayer(player).GetPlayerSuperstarName());
                    break;
                }
                if (GetOtherPlayer(player).GetPlayerDeckSize() == 0)
                {
                    _winner = false;
                    _view.CongratulateWinner(player.GetPlayerSuperstarName());
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

    public Player GetOtherPlayer(Player player)
    {
        if (player == _playerOne) return _playerTwo;
        return _playerOne;
    }

    public List<string> ConvertPlaysToString(List<Card> cardList)
    {
        List<string> stringPlays = new();
        foreach (var card in cardList)
        {
            
            if (card.Types.Contains("Maneuver"))
            {
                string playString = Formatter.PlayToString(new CardPlayInfo(card, "MANEUVER"));
                stringPlays.Add(playString);
            }
            else if (card.Types.Contains("Action"))
            {
                string playString = Formatter.PlayToString(new CardPlayInfo(card, "ACTION"));
                stringPlays.Add(playString);
            }
        }

        return stringPlays;
    }
    public bool DamagePlayer(Player player, int damage)
    {
        for (int currentDamage = 1; currentDamage <= damage; currentDamage++)
        {
            if (player.GetPlayerDeckSize()==0)
            {
                return false;
            }
            var discardedCard = player.GetLastCardOfDeck();
            player.RecieveDamage(discardedCard);
            _view.ShowCardOverturnByTakingDamage(Formatter.CardToString(discardedCard), currentDamage, damage );
            
        }
        return true;
    }
}