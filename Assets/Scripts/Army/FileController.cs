using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FileController : MonoBehaviour
{
    public static FileController instance;

    public Sprite skill_attackWeakestUnit;
    public Sprite skill_reduceEnemyDamage;
    public Sprite skill_buffAlliedRow;
    public Sprite skill_healAllies;
    public Sprite skill_increasedDamageToStructures;

    public Sprite speed_fast;
    public Sprite speed_normal;
    public Sprite speed_slow;

    public Sprite leaderHeartFull;
    public Sprite leaderHeartEmpty;

    #region UnitList
    //Leaders
    public UnitSO kingJohn;

    //Structures
    public UnitSO castleWalls;

    //First Row
    public UnitSO simpleFighter;
    public UnitSO shieldsquire;

    //Second Row
    public UnitSO horseman;
    public UnitSO battleDrummer;
    public UnitSO conquistador;

    //Ranged
    public UnitSO simpleArcher;
    public UnitSO sleepDartNinja;

    //Siege
    public UnitSO catapult;
    public UnitSO sharpshooter;

    //Multiple
    public UnitSO fieldMedic;
    #endregion

    public readonly string armyTag = "Army";
    public readonly string unitTag = "Unit";
    public readonly string unitDetailTag = "UnitDetails";
    public readonly string questionmarkTag = "Questionmark";



    public void SetInstance()
    {
        instance = this;
        SetDescriptions();
    }

    //Change in FileController
    private void SetDescriptions()
    {
        kingJohn.description = "King John I.";

        castleWalls.description = "This is a wall. Medics recommend not running it down head first, use a sword instead.";

        simpleFighter.description = "Big, muscular and as bright as a new moon. Likes to skip leg day for additional chest workout.";
        shieldsquire.description = "Who needs a sword when you can wield a two-handed shield. The only drawback is that you're really slow dragging that thing around.";

        horseman.description = "He bought his horse from an old chessmaster, so it can jump over pretty much everything, including gates, enemies and whole castles.";
        battleDrummer.description = "With his raging drumming, allied front units gain additional damage. Pretty weak otherwise.";
        conquistador.description = "With his pike this guy can stab enemies from beind the first row. If you're in need of a friend just compliment his moustache.";

        simpleArcher.description = "With a body like a leaf in the wind he isn't made for the front line. Instead, he prefers to shoot arrows at his enemies from a safe distance.";
        sleepDartNinja.description = "Always carries a sleep dart with him. Says they're perfect for discussing with unhappy customers.";

        catapult.description = "A siege unit, takes a while to get ready but packs quite a punch. Make sure to read the manual first.";
        sharpshooter.description = "He sure takes his sweet time to aim, but when he hits his target, all that is left is a funeral. Shame he didn't bring more ammunition.";

        fieldMedic.description = "Likely to lecture people about the dangers of not washing your hands while they are lying impaled in front of him.";
    }
}
