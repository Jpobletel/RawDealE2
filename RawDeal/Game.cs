using RawDealView;
using Newtonsoft.Json;
using RawDealView.Formatters;
using RawDealView.Options;
using static RawDeal.DeckChecker;

namespace RawDeal;

public class Game
{
    private readonly View _view;
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
                bool usedAbility = false;
                _view.SayThatATurnBegins(player.GetPlayerSuperstarName());
                if (player.GetPlayerSuperstarName() == "KANE")
                {
                    _view.SayThatPlayerIsGoingToUseHisAbility(player.GetPlayerSuperstarName(), player.GetPlayerSuperstarAbility());
                    _view.SayThatSuperstarWillTakeSomeDamage(GetOtherPlayer(player).GetPlayerSuperstarName(),1);
                    _winner = DamagePlayer(GetOtherPlayer(player), 1);
                }
                else if(player.GetPlayerSuperstarName() == "THE ROCK")
                {
                    if (player.GetRingsideCount() > 0)
                    {
                        if (_view.DoesPlayerWantToUseHisAbility("THE ROCK"))
                        {
                            _view.SayThatPlayerIsGoingToUseHisAbility(player.GetPlayerSuperstarName(), player.GetPlayerSuperstarAbility());
                            var index = _view.AskPlayerToSelectCardsToRecover(player.GetPlayerSuperstarName(), 1, player.GetStringCards(CardSet.RingsidePile));
                            player.MoveCardFromRingsideToBottomPile(index);
                        }
                    }
                }

                if (player.GetPlayerSuperstarName() == "MANKIND" && player.GetPlayerDeckSize() > 1)
                {
                    player.DrawCard();
                }
                player.DrawCard();
                _view.ShowGameInfo(player.GetPlayerInfo(), GetOtherPlayer(player).GetPlayerInfo());


                var optionNextPlay = GetInput(player, usedAbility);
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
                    else if (optionNextPlay == NextPlay.UseAbility)
                    {
                        if (player.GetPlayerSuperstarName() == "THE UNDERTAKER" && player.GetPlayerHandSize() >= 2)
                        {
                            _view.SayThatPlayerIsGoingToUseHisAbility(player.GetPlayerSuperstarName(), player.GetPlayerSuperstarAbility());
                            var handStringList = player.GetStringCards(CardSet.Hand);
                            var superstarName = player.GetPlayerSuperstarName();
                            var discardIndexOne = _view.AskPlayerToSelectACardToDiscard(handStringList,
                                superstarName, superstarName, 2);
                            player.MoveCardFromHandToRingside(discardIndexOne);
                            handStringList = player.GetStringCards(CardSet.Hand);
                            var discardIndexTwo = _view.AskPlayerToSelectACardToDiscard(handStringList,
                                superstarName, superstarName, 1);
                            player.MoveCardFromHandToRingside(discardIndexTwo);
                            var ringsideStringList = player.GetStringCards(CardSet.RingsidePile);
                            var cardToHandIndex =
                                _view.AskPlayerToSelectCardsToPutInHisHand(superstarName, 1, ringsideStringList);
                            player.MoveCardFromRingsideToHand(cardToHandIndex);

                            usedAbility = true;
                        }
                        else if (player.GetPlayerSuperstarName() == "CHRIS JERICHO" && player.GetPlayerHandSize() >= 1)
                        {
                            _view.SayThatPlayerIsGoingToUseHisAbility(player.GetPlayerSuperstarName(), player.GetPlayerSuperstarAbility());
                            var handStringList = player.GetStringCards(CardSet.Hand);
                            var discardIndex = _view.AskPlayerToSelectACardToDiscard(handStringList,
                                player.GetPlayerSuperstarName(), player.GetPlayerSuperstarName(), 1);
                            player.MoveCardFromHandToRingside(discardIndex);
                            handStringList = GetOtherPlayer(player).GetStringCards(CardSet.Hand);
                            discardIndex = _view.AskPlayerToSelectACardToDiscard(handStringList,
                                GetOtherPlayer(player).GetPlayerSuperstarName(), GetOtherPlayer(player).GetPlayerSuperstarName(), 1);
                            GetOtherPlayer(player).MoveCardFromHandToRingside(discardIndex);
                            usedAbility = true;
                        }
                        else if (player.GetPlayerSuperstarName() == "STONE COLD STEVE AUSTIN" && player.GetPlayerHandSize() >= 1)
                        {
                            _view.SayThatPlayerIsGoingToUseHisAbility(player.GetPlayerSuperstarName(), player.GetPlayerSuperstarAbility());
                            _view.SayThatPlayerDrawCards(player.GetPlayerSuperstarName(), 1);
                            player.DrawCard();
                            var indexDiscardedCard = _view.AskPlayerToReturnOneCardFromHisHandToHisArsenal(player.GetPlayerSuperstarName(), player.GetStringCards(CardSet.Hand));
                            player.MoveCardFromHandToBottomPile(indexDiscardedCard);
                            usedAbility = true;
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
                            optionNextPlay = GetInput(player, usedAbility);
                            continue;
                        }
                        var playedCard = stringPlayList[optionNextPlaySet];
                        player.PlayCard(playList[optionNextPlaySet], optionNextPlaySet);
                        _view.SayThatPlayerIsTryingToPlayThisCard(player.GetPlayerSuperstarName() ,playedCard);
                        _view.SayThatPlayerSuccessfullyPlayedACard();
                        var damage = Int32.Parse(playList[optionNextPlaySet].Damage);
                        if (GetOtherPlayer(player).GetPlayerSuperstarName() == "MANKIND") damage--;
                        _view.SayThatSuperstarWillTakeSomeDamage(GetOtherPlayer(player).GetPlayerSuperstarName(),damage);
                        _winner = DamagePlayer(GetOtherPlayer(player), damage);
                    }
                    

                    if (!_winner) break;
                    _view.ShowGameInfo(player.GetPlayerInfo(), GetOtherPlayer(player).GetPlayerInfo());
                    optionNextPlay = GetInput(player, usedAbility);

                }

                if (optionNextPlay == NextPlay.GiveUp)
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
                if (player.GetPlayerDeckSize() is 0)
                {
                    _winner = false;
                    _view.CongratulateWinner(GetOtherPlayer(player).GetPlayerSuperstarName());
                    break;
                }
            }
        }
        
    }

    private List<Player> CreateTurnList()
    {
        if (_playerOne.GetPlayerSuperstarValue() >= _playerTwo.GetPlayerSuperstarValue())
            return new List<Player> { _playerOne, _playerTwo };
        return new List<Player> { _playerTwo, _playerOne };
    }

    private Player AssignDeckToPlayer()
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

    private void ImportDecks()
    {
        using StreamReader reader = new(Path.Combine("data", "cards.json"));
        var json = reader.ReadToEnd();
        _cardList = JsonConvert.DeserializeObject<List<Card>>(json);
    }

    private void ImportSuperstars()
    {
        using StreamReader reader = new(Path.Combine("data", "superstar.json"));
        var json = reader.ReadToEnd();
        _superstarList = JsonConvert.DeserializeObject<List<Superstar>>(json);
    }

    private Player GetOtherPlayer(Player player)
    {
        if (player == _playerOne) return _playerTwo;
        return _playerOne;
    }

    private List<string> ConvertPlaysToString(List<Card> cardList)
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

    private bool DamagePlayer(Player player, int damage)
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

    private NextPlay GetInput(Player player, bool usedAbility)
    {
        if (!usedAbility)
        {
            if (player.GetPlayerSuperstarName() == "THE UNDERTAKER" && player.GetPlayerHandSize() >= 2)
            {
                return _view.AskUserWhatToDoWhenUsingHisAbilityIsPossible();
            }
            if (player.GetPlayerSuperstarName() == "CHRIS JERICHO"&& player.GetPlayerHandSize() >= 1)
            {
                return _view.AskUserWhatToDoWhenUsingHisAbilityIsPossible();
            }

            if (player.GetPlayerSuperstarName() == "STONE COLD STEVE AUSTIN" && player.GetPlayerHandSize() > 0)
            {
                return _view.AskUserWhatToDoWhenUsingHisAbilityIsPossible();
            }
        }
        return _view.AskUserWhatToDoWhenHeCannotUseHisAbility();
    }
}