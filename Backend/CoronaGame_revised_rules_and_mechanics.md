**Corona Game: rules and implemented mechanics.**
 
**Introduction**  
In the Corona Game players play a social situation, in which the Corona virus is spreading. They must maintain a regular social life, while working against the pandemic situation.

**Players**   
Players have seven attributes:
* Identification (in the empirical game a name and RFID, with which they can be identified)
* Money units (MU) with which they can "purchase" certain goods and services
* Health points (HP) that they lose during the game and have to refill
* Infection status (IS) with Covid
* Stress Level (SL) that increases the amount of HP they lose and is increased by intensive working
* Education (ED) which affects how much money they earn and can be increased in the school
* Learning Speed (LS) which affects how quickly their education increases
* (Stress resilience)

**Stations:**  
There are eight stations that players can or must visit:
* The mall (supermarket, bank, pharmacy), where players can purchase goods and services that they require or are helpful for the game. 
* The office, where players can earn money.
* The lounge, where players can reduce their stress level.
* The school, where players can learn and raise their education.
* Home, where players can be in quarantine and every round starts and ends.
* The town hall, where they can decide on changes that affect the gameplay
* The hospital, where people are staying that run out of HP
* Marketsquare (infoboard)

**Scales**  
Furthermore, there are a couple of scales that influence the gameplay:
* The stock fund into which players can invest to get a dividend and trigger economic growth by increasing the factor in the office.
* The overall fund of taxes
* The income tax rate
* The property tax rate
* The office factor controls how much MU per task players get in the office.
* The basic infection probability
* The item prices in the mall
* The cost of school 
* The cost of the lounge
* The cost of the care package in the hospital

**Gameplay**  
The game starts with everyone being at home. Every player is initialized with a certain amount of MU, HP, a Learning Speed and an infection status. Initially their ED is 1. They can now visit the other stations with the restriction that they have to choose between the office and the school every round and can visit one of them only once per round. There is no restriction on visiting the mall except for people in quarantine. After each round the players lose 10 HP and if they are infected with Corona another 10 HP. Additionally, if they encountered an infected person in one of the stations, there is a chance that they become infected. 
Every round consists of two phases: The daylight phase, during which they can go to the office, school, town hall, and mall, and the evening phase, in which they have to chose between visiting the lounge, home, and the town hall if there is a town hall meeting.

**The mall**  
In the mall players can purchase goods and services
* Health points: Players can purchase HP for 10 MU. The first purchase gives them 10 HP, the second 5HP, the third 2.5 HP, etc.
* A disinfectant: The disinfectant reduces the likelihood of being infected for its buyer. It does not protect other players from infection. For a player with Corona the disinfectant is therefore useless. The initial price 10 MU.
* A health check that gives players information about their SL, HP and IS. This costs 20 MU.
* The vaccine fund: With increasing investment, the likelihood of a Corona vaccine being developed increases. The likelihood is initially set to (gross vaccine fund/10,000)^2, so that it is rather unlikely the vaccine is developed in early rounds. 
* The stock fund: Players can purchase stocks and get every round 15% dividend for it. Additionally, every round the office factor increases by (gross stock investment/400), which means that for every 400 MU of stocks the factor increases by 1 every round. 
* Send money to other players

**The office**   
By registering in the office, players get an amount of money that equals (2 x ED x Office factor) MU. However, it increases their SL by 1. 

**The school**  
By registering in the school, players can increase their ED in the height of their LS. However, it increases their SL by 4. 

**The lounge**  
The lounge is one of two meeting places. Every stay in the lounge reduces their SL by 6.

**The town hall**  
Every third round there is a town hall meeting, such that players can join the town hall in these rounds and develop petitions that are then laid out in the town hall for a vote. During the daytime phase players can vote here until the next townhall meeting.

**The hospital**  
Players, who run out of HP come to the hospital for two rounds. They can purchase the care package there for 10 MU, such that their hospital stay is reduced to one round. In the case the health insurance exists this is automatically purchased. After the two rounds he leaves the hospital with 10 HP and without a Covid infection. 

**Home**  
Once every player has returned to the home station, the round ends and the next round starts. If a player is in quarantine, he also stays at home and loses no HP. The home station contains an interface that displays the information available at the marketsquare plus the ability to check the personal status.

**Marketsquare**
The marketsquare contains an infoboard.

**Infection**  
Initially every player is set with an infection status. This infection status can change by five mechanisms:
* An infected player can get into quarantine, which means staying in the home station for the entire round. In this case his infection status is set to not infected. 
* A player that is not infected can become infected by being in the same station with someone that is infected without using disinfectant. The mechanism is implemented such that if an infected player is at a station the likelihood of each player that visited the station after him is reduced by a constant factor that is currently set to 5. This means the player immediately after an infected player has a 20% chance of being infected, the next player a 4% chance, and so on. However, this likelihood is cumulative. If a player visited one station as the third person behind an infected person and as the first person behind another infected player, and another station as the second person behind an infected player, his total infection probability is 0.8% + 20% + 4% = 24.8%. 
* After a hospital stay players are automatically not infected
* The vaccine cures all players. 
* There is a 10% chance for every player to spontaneously get infected after every round, if otherwise no one is infected anymore. This is only prevented if the vaccine is available. 
This means that the only way to end the pandemic is for the vaccine to be available. All players staying at home is not a possible strategy to end the effects of Corona. 
 
**Votes**  
During a townhall meeting the present players can design a petition. These petition follow one of eight basic patters:
* Free school
* Lockdown (lounge is closed)
* Change the income tax rate
* Change the property tax rate
* Change minimum wage
* Disappropriate the wealthy
* Introduce health insurance
* Introduce social safety

Every of these eight basic patterns affects a scale of the game, such that it is either switched on or set to a certain value. This follows the following patterns:
* Social safety, School tuition, and health insurance are paid from the tax fund. The tax fund is filled by the income and property tax. If after a round there are not enough taxes collected the deficit is divided among all players.
* If social safety is introduced, players that have a negative amount of MU at the end of a round are automatically set to 10 MU. This is paid from the tax fund.
* If health insurance is introduced, the care package is automatically purchased and paid by the tax fund.
* The minimum wage is set to (2*OfficeFactor + x) MU. This however sets the maximum wage to (2 * OfficeFactor * HighestED - x) MU. 
* Disappropriation means that every player is disappropriated of every MU more than x he has at the end of every round. 

