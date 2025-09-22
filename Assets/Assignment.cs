
/*
This RPG data streaming assignment was created by Fernando Restituto with 
pixel RPG characters created by Sean Browning.
*/

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Threading;
using System.Xml.Linq;
using UnityEditor.Tilemaps;
using UnityEngine;


#region Assignment Instructions

/*  Hello!  Welcome to your first lab :)

Wax on, wax off.

    The development of saving and loading systems shares much in common with that of networked gameplay development.  
    Both involve developing around data which is packaged and passed into (or gotten from) a stream.  
    Thus, prior to attacking the problems of development for networked games, you will strengthen your abilities to develop solutions using the easier to work with HD saving/loading frameworks.

    Try to understand not just the framework tools, but also, 
    seek to familiarize yourself with how we are able to break data down, pass it into a stream and then rebuild it from another stream.


Lab Part 1

    Begin by exploring the UI elements that you are presented with upon hitting play.
    You can roll a new party, view party stats and hit a save and load button, both of which do nothing.
    You are challenged to create the functions that will save and load the party data which is being displayed on screen for you.

    Below, a SavePartyButtonPressed and a LoadPartyButtonPressed function are provided for you.
    Both are being called by the internal systems when the respective button is hit.
    You must code the save/load functionality.
    Access to Party Character data is provided via demo usage in the save and load functions.

    The PartyCharacter class members are defined as follows.  */

public partial class PartyCharacter
{
    public int classID;

    public int health;
    public int mana;

    public int strength;
    public int agility;
    public int wisdom;

    public LinkedList<int> equipment;

}


/*
    Access to the on screen party data can be achieved via …..

    Once you have loaded party data from the HD, you can have it loaded on screen via …...

    These are the stream reader/writer that I want you to use.
    https://docs.microsoft.com/en-us/dotnet/api/system.io.streamwriter
    https://docs.microsoft.com/en-us/dotnet/api/system.io.streamreader

    Alright, that’s all you need to get started on the first part of this assignment, here are your functions, good luck and journey well!
*/


#endregion


#region Assignment Part 1

static public class AssignmentPart1
{

    private static string SavePath = "party.save";

    static public void SavePartyButtonPressed()
    {
        Debug.Log("[Save] Entered SavePartyButtonPressed");

        using (StreamWriter writer = new StreamWriter(SavePath, false))
        {
            writer.WriteLine(GameContent.partyCharacters.Count);

            foreach (PartyCharacter pc in GameContent.partyCharacters)
            {
                string line = pc.classID + ":" +
                              pc.health + ":" +
                              pc.mana + ":" +
                              pc.strength + ":" +
                              pc.agility + ":" +
                              pc.wisdom;

                if (pc.equipment != null && pc.equipment.Count > 0)
                {
                    line += "/";
                    bool first = true;
                    foreach (int eq in pc.equipment)
                    {
                        if (!first)
                        {
                            line += ":";
                        }
                        line += eq;
                        first = false;
                    }
                }

                writer.WriteLine(line);
            }
        }

        Debug.Log("[Save] Party successfully saved.");
    }



    static public void LoadPartyButtonPressed()
    {
        using (StreamReader reader = new StreamReader(SavePath))
        {
            int count = int.Parse(reader.ReadLine());
            GameContent.partyCharacters.Clear();

            for (int i = 0; i < count; i++)
            {
                string line = reader.ReadLine();

                string statsPart = line;
                string equipPart = null;
                int slashIndex = line.IndexOf('/');
                if (slashIndex >= 0)
                {
                    statsPart = line.Substring(0, slashIndex);
                    if (slashIndex + 1 < line.Length)
                        equipPart = line.Substring(slashIndex + 1);
                }

                string[] s = statsPart.Split(':');
                PartyCharacter pc = new PartyCharacter(
                    int.Parse(s[0]),
                    int.Parse(s[1]),
                    int.Parse(s[2]),
                    int.Parse(s[3]),
                    int.Parse(s[4]),
                    int.Parse(s[5])
                );
                pc.equipment = new LinkedList<int>();

                if (!string.IsNullOrEmpty(equipPart))
                {
                    string[] eqs = equipPart.Split(':');
                    foreach (string eq in eqs)
                    {
                        if (eq.Length > 0)
                            pc.equipment.AddLast(int.Parse(eq));
                    }
                }
                GameContent.partyCharacters.AddLast(pc);
            }
        }

        GameContent.RefreshUI();
        Debug.Log("[Load] Party loaded.");
    }


}


#endregion


#region Assignment Part 2

//  Before Proceeding!
//  To inform the internal systems that you are proceeding onto the second part of this assignment,
//  change the below value of AssignmentConfiguration.PartOfAssignmentInDevelopment from 1 to 2.
//  This will enable the needed UI/function calls for your to proceed with your assignment.
static public class AssignmentConfiguration
{
    public const int PartOfAssignmentThatIsInDevelopment = 2;
}

/*

In this part of the assignment you are challenged to expand on the functionality that you have already created.  
    You are being challenged to save, load and manage multiple parties.
    You are being challenged to identify each party via a string name (a member of the Party class).

To aid you in this challenge, the UI has been altered.  

    The load button has been replaced with a drop down list.  
    When this load party drop down list is changed, LoadPartyDropDownChanged(string selectedName) will be called.  
    When this drop down is created, it will be populated with the return value of GetListOfPartyNames().

    GameStart() is called when the program starts.

    For quality of life, a new SavePartyButtonPressed() has been provided to you below.

    An new/delete button has been added, you will also find below NewPartyButtonPressed() and DeletePartyButtonPressed()

Again, you are being challenged to develop the ability to save and load multiple parties.
    This challenge is different from the previous.
    In the above challenge, what you had to develop was much more directly named.
    With this challenge however, there is a much more predicate process required.
    Let me ask you,
        What do you need to program to produce the saving, loading and management of multiple parties?
        What are the variables that you will need to declare?
        What are the things that you will need to do?  
    So much of development is just breaking problems down into smaller parts.
    Take the time to name each part of what you will create and then, do it.

Good luck, journey well.

*/

static public class AssignmentPart2
{

    static List<string> listOfPartyNames = new List<string>();

    static public void GameStart()
    {
        ReloadNameList();
        GameContent.RefreshUI();
    }
    static void ReloadNameList()
    {
        listOfPartyNames.Clear();
        foreach (var f in System.IO.Directory.GetFiles(".", "*.save"))
            listOfPartyNames.Add(System.IO.Path.GetFileNameWithoutExtension(f));
        listOfPartyNames.Sort(System.StringComparer.OrdinalIgnoreCase);
    }

    static string PruneFileName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "Default Party";

        char[] invalid = System.IO.Path.GetInvalidFileNameChars();
        foreach (char c in invalid)
            name = name.Replace(c.ToString(), "");

        name = name.Trim();
        return string.IsNullOrEmpty(name) ? "Party" : name;
    }

    static public List<string> GetListOfPartyNames()
    {
        return listOfPartyNames;
    }

    static public void LoadPartyDropDownChanged(string selectedName)
    {
        if (string.IsNullOrWhiteSpace(selectedName)) return;

        string path = PruneFileName(selectedName) + ".save";
        if (!System.IO.File.Exists(path)) return;

        using (var r = new System.IO.StreamReader(path))
        {
            string first = r.ReadLine();
            if (string.IsNullOrWhiteSpace(first)) return;

            int count = int.Parse(first.Trim());
            GameContent.partyCharacters.Clear();

            for (int i = 0; i < count; i++)
            {
                string line = r.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) continue;

                string stats = line, equip = null;
                int slash = line.IndexOf('/');
                if (slash >= 0)
                {
                    stats = line.Substring(0, slash);
                    if (slash + 1 < line.Length) equip = line.Substring(slash + 1);
                }

                var s = stats.Split(':');
                if (s.Length < 6) continue;

                var pc = new PartyCharacter();
                pc.classID = int.Parse(s[0]);
                pc.health = int.Parse(s[1]);
                pc.mana = int.Parse(s[2]);
                pc.strength = int.Parse(s[3]);
                pc.agility = int.Parse(s[4]);
                pc.wisdom = int.Parse(s[5]);

                pc.equipment = new LinkedList<int>();
                if (!string.IsNullOrEmpty(equip))
                {
                    foreach (var tok in equip.Split(':'))
                        if (tok.Length > 0) pc.equipment.AddLast(int.Parse(tok));
                }

                GameContent.partyCharacters.AddLast(pc);
            }
        }

        GameContent.RefreshUI();
    }


    static public void SavePartyButtonPressed(string partyName)
    {
        string path = PruneFileName(partyName) + ".save";

        using (var w = new System.IO.StreamWriter(path, false))
        {
            w.WriteLine(GameContent.partyCharacters.Count);
            foreach (PartyCharacter pc in GameContent.partyCharacters)
            {
                string line = pc.classID + ":" + pc.health + ":" + pc.mana + ":" +
                              pc.strength + ":" + pc.agility + ":" + pc.wisdom;

                if (pc.equipment != null && pc.equipment.Count > 0)
                {
                    line += "/";
                    bool first = true;
                    foreach (int eq in pc.equipment)
                    {
                        if (!first) line += ":";
                        line += eq; first = false;
                    }
                }
                w.WriteLine(line);
            }
        }

        ReloadNameList();
        GameContent.RefreshUI();
    }

    static public void DeletePartyButtonPressed(string partyName)
    {
        if (string.IsNullOrWhiteSpace(partyName)) return;

        string path = PruneFileName(partyName) + ".save";
        if (System.IO.File.Exists(path)) System.IO.File.Delete(path);

        GameContent.partyCharacters.Clear();
        ReloadNameList();
        GameContent.RefreshUI();
    }

}

#endregion


