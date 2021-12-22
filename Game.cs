using Algiers;
using Algiers.StartKit;
using System;

public class Game
{
    public static World SetWorld()
    {
        State introState = new State();
        State saloonState = new State();
        State notPlayingState = BlackJack.blackjackState.Inverse();

        World world = new World();
        world.start = "You come to in the back seat of the cab as it comes to a stop. You're here.\n(Type 'help' for a list of commands)";

        SPlayer player = new SPlayer();
        world.player = player;

        string PlayBlackJack()
        {
            BlackJack game = new BlackJack(world);
            return game.Start();
        }
        world.AddIntransitiveCommand("play blackjack", PlayBlackJack, saloonState);
        BlackJack.AddCommands(world);
        
        world.AddIntransitiveCommand("look", CMD.Look(player), notPlayingState, new string[]{"look around"});
        
        string Inv()
        {
            string inv = CMD.Inv(player)();
            int money = player.GetCounter("money");

            if (money <= 0)
            {
                return inv;
            }
            else if (inv == "Your inventory is empty.")
            {
                return money + "Ð";
            }
            else
            {
                return inv + ", " + money + "Ð";
            }
        }
        world.AddIntransitiveCommand("inv", Inv, State.All);

        world.AddTransitiveCommand("examine", CMD.What(player), notPlayingState, "Examine what?");
        world.AddTransitiveCommand("what", CMD.What(player), notPlayingState, "Examine what?");
        world.AddTransitiveCommand("take", CMD.Take(player), notPlayingState, "Take what?");
        world.AddTransitiveCommand("talk", CMD.Talk(player), notPlayingState, "Talk to whom?", preps: new string[]{"to"});
        world.AddTransitiveCommand("go", CMD.Go(player, world), notPlayingState, "go where?", new string[]{"go to", "enter"});

        Func<string, string> Drink()
        {
            return (target) =>
            {
                if (!player.InInventory(target))
                {
                    return "You don't have a " + target + " to drink";
                }
                else
                {
                    GameObject drinkObj = player.GetObject(target);
                    Func<string> drink = drinkObj.GetTransitiveResponse("drink");
                    if (drink == null)
                    {
                        return "You can't drink the " + target + ".";
                    }
                    else
                    {
                        return drink();
                    }
                }
            };
        }
        world.AddTransitiveCommand("drink", Drink(), State.All, "Drink what?");
        world.AddTransitiveCommand("buy", Bar.Buy(player), saloonState, "Buy what?");
        
        string Open(string target)
        {
            if (target == "door")
            {
                GameObject door = player.GetFromRoom("door");
                return door.GetTransitiveResponse("open")();
            }
            else
            {
                return "You can't open the " + target + ".";
            }
        }
        world.AddTransitiveCommand("open", Open, saloonState, "Open what?");

        string Use(string tool, string target)
        {
            if (tool == "jukebox")
            {
                GameObject juke = player.GetFromRoom("jukebox");
                return juke.GetTransitiveResponse("use")();
            }
            else
            {
                return CMD.Use(player)(tool, target);
            }
        }
        world.AddDitransitiveCommand("use", Use, notPlayingState, "Use what?", new string[]{"on", "with"});
        
        string Give(string gift, string target)
        {
            if (gift == "signet")
            {
                return "After everything you went through to get a hold of that thing, you shouldn't just give it away.";
            }
            else
            {
                return CMD.Give(player)(gift, target);
            }
        }
        world.AddDitransitiveCommand("give", Give, notPlayingState, "Give what?", new string[]{"to"});

        string Show(string item, string target)
        {
            if (!player.InInventory(item))
            {
                string indef = (Parser.StartsWithVowel(item))? "an " : "a ";
                return "You don't have " + indef + item + " in your inventory.";
            }
            else if (target == "")
            {
                return "Show " + item + " to whom?";
            }
            else if (!player.CanAccessObject(target))
            {
                return "There is nobody named " + target + " here to show the " + item + " to.";
            }
            else
            {
                GameObject targetObj = player.GetFromRoom(target);
                Func<string, string> give = targetObj.GetDitransitiveResponse("show");
                string nullHandler = "You can't show the " + item + " to " + target + ".";
                if (give == null)
                {
                    return nullHandler;
                }
                else
                {
                    string response = give(item);
                    return (response == null)? nullHandler : response;
                }
            }
        }
        world.AddDitransitiveCommand("show", Show, notPlayingState, "Show what?", new string[]{"to"});

        Room intro = new Intro(player);
        world.AddRoom(intro);
        Room saloon = new Saloon(player);
        saloon.OnEnter = () => {player.State = saloonState;};
        world.AddRoom(saloon);
        
        player.current_room = intro;
        player.State = introState;
        player.AddCounter("money");
        player.IncrementCounter("money", 20);
        
        // player.current_room = saloon;
        // //player.AddWaypoint("firstgame");
        // //player.AddToInventory(new GameObject("quarter"));
        // saloon.AddObject(new Rys(player));
        // player.AddToInventory(new GameObject("peanut tequila"));

        return world;
    }
}