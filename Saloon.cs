using Algiers;

public class Saloon : Room
{
    State saloonState = new State();

    SPlayer player;

    public Saloon(SPlayer player) : base("saloon")
    {
        this.player = player;

        description = "A smoky, seedy looking place that doesn't seem to know what year it is. There's even a JUKEBOX." +
            " The BARTENDER stands in front of an empty COUNTER. The DEALER is running a game of blackjack for a few" +
            " people around a TABLE in the corner. All the way in the back, almost hidden in the dark, there's a DOOR with a sign: 'Staff Only'.";
        
        OnEnter = () => {player.State = saloonState;};
        OnExit = () => {Delete();};

        World.GetWorld.AddTransitiveCommand("buy", Bar.Buy(player), saloonState, "Buy what?");
        World.GetWorld.AddTransitiveCommand("open", Open, saloonState, "Open what?");
        World.GetWorld.AddIntransitiveCommand("play blackjack", PlayBlackJack, saloonState);

        AddObject(new Dealer(player));
        AddObject(new Bartender(player));
        
        //Counter
        GameObject counter = new GameObject("counter");
        counter.SetTransitiveResponse("what", () => {
            return "The main thing you notice about the counter is that it's sticky. You also see a DRINK LIST and a BOWL OF PEANUTS.";
        });
        AddObject(counter);

        //Peanuts
        Hoard peanuts = new Hoard("bowl of peanuts", "peanut");
        peanuts.SetTransitiveResponse("what", () => {
            return "A small bowl, but it fits a surprisng number of peanuts.";
        });
        peanuts.SetMemberTransitiveResponse("what", () => {
            return "Just a peanut.";
        });
        peanuts.SetMemberTransitiveResponse("take", () => {
            player.AddToInventory(peanuts.TakeMember());
            return "You take a peanut from the bowl. You don't really care for peanuts but hey, maybe you'll run into an elephant.";
        });
        AddObject(peanuts);

        //DrinkList
        GameObject menu = new GameObject("drink list");
        menu.SetTransitiveResponse("what", () => {
            return "Whiskey - 3Ð\nTequila - 5Ð\nGin - 5Ð\nMoonshine - 8Ð\nBottle of Whiskey - 14Ð";
        });
        AddObject(menu);

        //JukeBox
        AddObject(new Jukebox(player));

        //Door
        AddObject(new Door(player));

        //Keypad
        GameObject keypad = new GameObject("keypad");
            keypad.SetTransitiveResponse("what", () => {
                return "A keypad with physical number keys, 0-9.";
            });
        AddObject(keypad);

        //Players
        GameObject table = new GameObject("table");
            table.SetTransitiveResponse("what", () => {
                return "A small card table. Around it sit the DEALER, a MAN, a WOMAN, and a PERSON IN A BIG GREEN HAT.";
            });
        AddObject(table);

        GameObject man = new GameObject("man");
            man.SetTransitiveResponse("what", () => {
                return "A man with short hair and a long nose. He doesn't seem to be winning much.";
            });
            man.SetTransitiveResponse("talk", () => {
                return "'Hey shoo, I'm losing money here.'";
            });
        AddObject(man);

        GameObject woman = new GameObject("woman");
            woman.SetTransitiveResponse("what", () => {
                return "She's assembled quite a collection of empty glasses in front of her. She's holding a leather briefcase"
                    + " and keeps glancing at the door.";
            });
            woman.SetTransitiveResponse("talk", () => {
                return "'Mind your own business, would you.'";
            });
        AddObject(woman);

        GameObject person = new GameObject("person in a big green hat");
            person.SetTransitiveResponse("what", () => {
                return "They're wearing a very large, very bright green hat, sunglasses, and a trenchcoat. Couldn't say much else about them.";
            });
            person.SetTransitiveResponse("talk", () => {
                return "You try to ask them something but they just smile back at you.";
            });
        AddObject(person);
    }

    string PlayBlackJack()
    {
        if (player.GetCounter("money") == 0)
        {
            return "You gotta have money to play BlackJack.";
        }
        else
        {
            BlackJack game = new BlackJack(player);
            return game.Start();
        }
    }

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
}