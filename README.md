# Pioneers

A side project I worked on a few years ago, figured I'd upload it here before it goes to waste.

It's a round based combat game with cards, where the player to first lose two battles loses the game. It's a bit inspired by Gwent from The Witcher.

To play (or change) it yourself you need to open it in Unity.

# Rules

Each player starts with two hearts. Losing a round or ending in a draw results in losing one heart. If you reach zero hearts, you lose.

Before starting the battle, you have to select a leader. Currently this is pretty useless since there is only one leader card in the game.

A battle begins with the players alternatedly putting down one card for five turns each.
- You draw 8 cards into your hand. Every time you put a card on the table, you may draw another card.
- The computer starts with a "castle walls" card already placed down for some more challenge (I'm not good at programming ai).
- You can place a card from your hand on the table by pulling it in with the mouse.
- If you are happy with your choice, hit the "next turn" button in the bottom right.
- If you do not wish to play a card in a turn (to save more cards for the next battle maybe - at the end of a battle all cards on the table are discarded) you can just press the "next turn" button without playing.

After that comes the "battle phase".
- The first turn goes to the fast units. Each fast unit gets to play it's turn.
- Once all fast units have completed their turns, all units with zero HP left will die.
- Next turn the normal units attack. Again, units with zero HP left will be removed from the table only when every normal unit has completed their attack.
- Lastly the slow units play their turn.
- If a players king has died at any point in the battle phase, this player will lose one heart and the next battle will start. All cards left on the table will be discarded!

Each unit will attack enemies in this order, within each row from top to bottom (except the unit has a special skill that changes this):
- First Row
- Second Row
- Ranged
- Siege

If both kings are still alive after the battle phase, there will be a shortened drawing phase wich lasts for two turns each. Players can put down cards or skip, just like at the beginning of the battle.

# Cards

Simple Fighter
- default front row unit
- low damage, decent hp

Shieldsquire
- high health front row unit
- decent damage, but slow

Conquistador
- decent health, decent damage
- second row unit

Horseman
- low health and low damage, but fast
- second row unit
- attacks weakest enemy unit first

Simple Archer
- default ranged unit
- low health, good damage

Sleep Dart Ninja
- low health, decent damage
- ranged unit
- reduces the damage of the highest damage enemy unit (one time use)

Catapult
- classic siege unit
- decent health, very good damage, but slow

Sharpshooter
- low health siege unit
- very high damage for two turns, after that it's decent
- attacks the highest damage enemy unit first

Battle Drummer
- low health, low damage
- can be placed in every row
- increases damage of each unit in the same row every turn he gets to play

Field Medic
- decent health, low damage
- can be placed in every row
- each turn he heals the weakest unit in the same row
- can heal above max hp, any health above max hp will be removed at the end of a round
