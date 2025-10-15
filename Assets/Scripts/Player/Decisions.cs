using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class Makedecision
{
    public class Decisions
    {
        private static System.Random random = new System.Random();
        private static Queue<DelayedEffect> delayedEffects = new Queue<DelayedEffect>();

        [System.Serializable]
        public class DelayedEffect
        {
            public int turnsUntilEffect;
            public System.Action effectAction;
            public string description;
        }

        public enum DecisionType
        {
            Execute,
            Exile,
            Forgive,
            Confiscate,
            Imprison,
            Torture,
            TrialByOrdeal,
            RedemptionQuest,
            PublicHumiliation,
            BanishWilderness
        }

        // All possible decisions
        private static List<DecisionType> allDecisions = new List<DecisionType>
        {
            DecisionType.Execute,
            DecisionType.Exile,
            DecisionType.Forgive,
            DecisionType.Confiscate,
            DecisionType.Imprison,
            DecisionType.Torture,
            DecisionType.TrialByOrdeal,
            DecisionType.RedemptionQuest,
            DecisionType.PublicHumiliation,
            DecisionType.BanishWilderness
        };

        // Current active decisions for the buttons
        private static List<DecisionType> currentActiveDecisions = new List<DecisionType>();

        public static void InitializeDecisions()
        {
            SelectRandomDecisions();
            delayedEffects.Clear();
        }

        public static void SelectRandomDecisions()
        {
            currentActiveDecisions.Clear();

            // Always include the basic three decisions
            List<DecisionType> availablePool = new List<DecisionType>(allDecisions);

            // Remove the basic three from the pool so we don't duplicate
            availablePool.Remove(DecisionType.Execute);
            availablePool.Remove(DecisionType.Exile);
            availablePool.Remove(DecisionType.Forgive);

            // Shuffle the remaining pool
            availablePool = availablePool.OrderBy(x => random.Next()).ToList();

            // Add the basic three plus random ones to make 10 total options
            currentActiveDecisions.Add(DecisionType.Execute);
            currentActiveDecisions.Add(DecisionType.Exile);
            currentActiveDecisions.Add(DecisionType.Forgive);

            // Add 0-2 random additional decisions (30% chance for each)
            if (random.Next(0, 100) < 30 && availablePool.Count > 0)
            {
                currentActiveDecisions.Add(availablePool[0]);
                availablePool.RemoveAt(0);
            }

            if (random.Next(0, 100) < 30 && availablePool.Count > 0)
            {
                currentActiveDecisions.Add(availablePool[0]);
                availablePool.RemoveAt(0);
            }

            // Shuffle the final list so buttons are in random order
            currentActiveDecisions = currentActiveDecisions.OrderBy(x => random.Next()).ToList();

            Debug.Log($"Selected decisions: {string.Join(", ", currentActiveDecisions)}");
        }

        public static List<DecisionType> GetCurrentActiveDecisions()
        {
            return new List<DecisionType>(currentActiveDecisions);
        }

        public static string GetDecisionDisplayName(DecisionType decisionType)
        {
            switch (decisionType)
            {
                case DecisionType.Execute: return "EXECUTE";
                case DecisionType.Exile: return "EXILE";
                case DecisionType.Forgive: return "FORGIVE";
                case DecisionType.Confiscate: return "CONFISCATE";
                case DecisionType.Imprison: return "IMPRISON";
                case DecisionType.Torture: return "TORTURE";
                case DecisionType.TrialByOrdeal: return "TRIAL BY ORDEAL";
                case DecisionType.RedemptionQuest: return "REDEMPTION QUEST";
                case DecisionType.PublicHumiliation: return "PUBLIC HUMILIATION";
                case DecisionType.BanishWilderness: return "BANISH TO WILDERNESS";
                default: return decisionType.ToString();
            }
        }

        public static string GetDecisionDescription(DecisionType decisionType)
        {
            switch (decisionType)
            {
                case DecisionType.Execute: return "Death penalty for serious crimes";
                case DecisionType.Exile: return "Banish from the realm";
                case DecisionType.Forgive: return "Show mercy and release";
                case DecisionType.Confiscate: return "Seize property and wealth";
                case DecisionType.Imprison: return "Lock away in dungeon";
                case DecisionType.Torture: return "Extract information painfully";
                case DecisionType.TrialByOrdeal: return "Let God decide guilt";
                case DecisionType.RedemptionQuest: return "Send on holy mission";
                case DecisionType.PublicHumiliation: return "Shame publicly";
                case DecisionType.BanishWilderness: return "Exile to cursed lands";
                default: return "Make a judgment";
            }
        }

        private static void ProcessDelayedEffects()
        {
            int effectsCount = delayedEffects.Count;
            for (int i = 0; i < effectsCount; i++)
            {
                DelayedEffect effect = delayedEffects.Dequeue();
                effect.turnsUntilEffect--;

                if (effect.turnsUntilEffect <= 0)
                {
                    effect.effectAction.Invoke();
                    Debug.Log($"Delayed effect triggered: {effect.description}");
                }
                else
                {
                    delayedEffects.Enqueue(effect);
                }
            }
        }

        public static void ExecuteDecision(Characters.Character currentCharacter, DecisionType decisionType)
        {
            GameState stats = GameState.Instance;
            bool isGuilty = currentCharacter.isGuilty;

            switch (decisionType)
            {
                case DecisionType.Execute:
                    ExecuteEffects(currentCharacter, isGuilty);
                    break;
                case DecisionType.Exile:
                    ExileEffects(currentCharacter, isGuilty);
                    break;
                case DecisionType.Forgive:
                    ForgiveEffects(currentCharacter, isGuilty);
                    break;
                case DecisionType.Confiscate:
                    ConfiscateEffects(currentCharacter, isGuilty);
                    break;
                case DecisionType.Imprison:
                    ImprisonEffects(currentCharacter, isGuilty);
                    break;
                case DecisionType.Torture:
                    TortureEffects(currentCharacter, isGuilty);
                    break;
                case DecisionType.TrialByOrdeal:
                    TrialByOrdealEffects(currentCharacter, isGuilty);
                    break;
                case DecisionType.RedemptionQuest:
                    RedemptionQuestEffects(currentCharacter, isGuilty);
                    break;
                case DecisionType.PublicHumiliation:
                    PublicHumiliationEffects(currentCharacter, isGuilty);
                    break;
                case DecisionType.BanishWilderness:
                    BanishWildernessEffects(currentCharacter, isGuilty);
                    break;
            }

            stats.currentStats.ClampValues();
            ProcessDelayedEffects();

            // Select new random decisions for next time
            SelectRandomDecisions();
        }

        // EXECUTE Decision
        private static void ExecuteEffects(Characters.Character character, bool isGuilty)
        {
            GameState stats = GameState.Instance;

            int popChange = -random.Next(10, 21);
            int fearChange = random.Next(15, 26);
            int divineChange = isGuilty ? random.Next(5, 16) : -random.Next(20, 31);
            int karmaChange = -random.Next(10, 21);

            stats.currentStats.population += popChange;
            stats.currentStats.fear += fearChange;
            stats.currentStats.divineFavor += divineChange;
            stats.currentStats.karma += karmaChange;

            // Delayed effects
            if (stats.recentExecutions >= 2)
            {
                delayedEffects.Enqueue(new DelayedEffect
                {
                    turnsUntilEffect = random.Next(2, 4),
                    effectAction = () => {
                        if (stats.recentExecutions >= 3)
                        {
                            stats.currentStats.population -= 10;
                            Debug.Log("Families flee after multiple executions!");
                        }
                    },
                    description = "Multiple executions consequence"
                });
            }

            if (stats.currentStats.fear > 70)
            {
                delayedEffects.Enqueue(new DelayedEffect
                {
                    turnsUntilEffect = random.Next(2, 4),
                    effectAction = () => {
                        stats.currentStats.divineFavor -= 10;
                        Debug.Log("God disapproves of your cruelty!");
                    },
                    description = "High fear divine disapproval"
                });
            }

            stats.recentExecutions++;
            Debug.Log($"Executed {character.characterName}");
        }

        // EXILE Decision
        private static void ExileEffects(Characters.Character character, bool isGuilty)
        {
            GameState stats = GameState.Instance;

            int popChange = -random.Next(5, 11);
            int fearChange = random.Next(5, 11);
            int divineChange = random.Next(2, 9);
            int karmaChange = -random.Next(2, 6);

            stats.currentStats.population += popChange;
            stats.currentStats.fear += fearChange;
            stats.currentStats.divineFavor += divineChange;
            stats.currentStats.karma += karmaChange;

            // Delayed effects
            if (random.Next(0, 100) < 30)
            {
                delayedEffects.Enqueue(new DelayedEffect
                {
                    turnsUntilEffect = random.Next(3, 6),
                    effectAction = () => {
                        stats.currentStats.fear += 15;
                        stats.currentStats.population -= 5;
                        Debug.Log($"{character.characterName} returned as a worse criminal!");
                    },
                    description = "Exiled criminal returns"
                });
            }

            if (random.Next(0, 100) < 20)
            {
                delayedEffects.Enqueue(new DelayedEffect
                {
                    turnsUntilEffect = random.Next(3, 6),
                    effectAction = () => {
                        stats.currentStats.fear -= 5;
                        Debug.Log("Bad rumors spread about your judgment!");
                    },
                    description = "Bad rumors spread"
                });
            }

            stats.recentExiles++;
            Debug.Log($"Exiled {character.characterName}");
        }

        // FORGIVE Decision
        private static void ForgiveEffects(Characters.Character character, bool isGuilty)
        {
            GameState stats = GameState.Instance;

            int popChange = random.Next(5, 11);
            int fearChange = -random.Next(10, 16);
            int divineChange = random.Next(-5, 11);
            int karmaChange = random.Next(10, 21);

            stats.currentStats.population += popChange;
            stats.currentStats.fear += fearChange;
            stats.currentStats.divineFavor += divineChange;
            stats.currentStats.karma += karmaChange;

            // Delayed effects
            if (random.Next(0, 100) < 40)
            {
                delayedEffects.Enqueue(new DelayedEffect
                {
                    turnsUntilEffect = random.Next(2, 5),
                    effectAction = () => {
                        stats.currentStats.fear += 10;
                        stats.currentStats.divineFavor -= 5;
                        Debug.Log($"{character.characterName} committed another crime!");
                    },
                    description = "Forgiven criminal reoffends"
                });
            }

            if (stats.currentStats.karma > 70)
            {
                delayedEffects.Enqueue(new DelayedEffect
                {
                    turnsUntilEffect = random.Next(2, 5),
                    effectAction = () => {
                        stats.currentStats.population += 10;
                        stats.currentStats.divineFavor += 10;
                        stats.currentStats.karma += 10;
                        Debug.Log("Divine blessing for your mercy!");
                    },
                    description = "Divine blessing"
                });
            }

            stats.recentForgives++;
            Debug.Log($"Forgave {character.characterName}");
        }

        // CONFISCATE Decision
        private static void ConfiscateEffects(Characters.Character character, bool isGuilty)
        {
            GameState stats = GameState.Instance;

            int popChange = -random.Next(2, 6);
            int fearChange = random.Next(8, 13);
            int divineChange = random.Next(3, 9);
            int karmaChange = -random.Next(5, 9);

            stats.currentStats.population += popChange;
            stats.currentStats.fear += fearChange;
            stats.currentStats.divineFavor += divineChange;
            stats.currentStats.karma += karmaChange;

            if (!isGuilty && stats.confiscatedFromInnocents >= 2)
            {
                delayedEffects.Enqueue(new DelayedEffect
                {
                    turnsUntilEffect = random.Next(2, 4),
                    effectAction = () => {
                        stats.currentStats.divineFavor -= 15;
                        Debug.Log("God punishes your greed against innocents!");
                    },
                    description = "Divine punishment for confiscating from innocents"
                });
            }

            if (!isGuilty) stats.confiscatedFromInnocents++;
            Debug.Log($"Confiscated from {character.characterName}");
        }

        // IMPRISON Decision
        private static void ImprisonEffects(Characters.Character character, bool isGuilty)
        {
            GameState stats = GameState.Instance;

            int popChange = 0; // Still alive but locked up
            int fearChange = random.Next(12, 19);
            int divineChange = random.Next(5, 11);
            int karmaChange = -random.Next(5, 11);

            stats.currentStats.population += popChange;
            stats.currentStats.fear += fearChange;
            stats.currentStats.divineFavor += divineChange;
            stats.currentStats.karma += karmaChange;

            if (random.Next(0, 100) < 20)
            {
                delayedEffects.Enqueue(new DelayedEffect
                {
                    turnsUntilEffect = random.Next(3, 6),
                    effectAction = () => {
                        stats.currentStats.fear += 10;
                        Debug.Log($"{character.characterName} escaped from prison!");
                    },
                    description = "Prison escape"
                });
            }

            stats.currentPrisoners++;
            Debug.Log($"Imprisoned {character.characterName}");
        }

        // TORTURE Decision
        private static void TortureEffects(Characters.Character character, bool isGuilty)
        {
            GameState stats = GameState.Instance;

            int popChange = -random.Next(5, 9); // May die from torture
            int fearChange = random.Next(20, 31);
            int divineChange = -random.Next(10, 21);
            int karmaChange = -random.Next(15, 26);

            stats.currentStats.population += popChange;
            stats.currentStats.fear += fearChange;
            stats.currentStats.divineFavor += divineChange;
            stats.currentStats.karma += karmaChange;

            // Special: May reveal information
            if (random.Next(0, 100) < 60)
            {
                Debug.Log("Torture revealed valuable information!");
                // This could affect future characters
            }
            else
            {
                Debug.Log("Torture yielded false information!");
                stats.currentStats.fear -= 5; // People see through the deception
            }

            stats.tortureCount++;
            Debug.Log($"Tortured {character.characterName}");
        }

        // TRIAL BY ORDEAL Decision
        private static void TrialByOrdealEffects(Characters.Character character, bool isGuilty)
        {
            GameState stats = GameState.Instance;

            int fearChange = random.Next(10, 16);
            int divineChange = random.Next(15, 26);
            int karmaChange = 0; // Neutral

            stats.currentStats.fear += fearChange;
            stats.currentStats.divineFavor += divineChange;
            stats.currentStats.karma += karmaChange;

            // Random outcome (50/50)
            bool survivesOrdeal = random.Next(0, 2) == 0;

            if (survivesOrdeal)
            {
                stats.currentStats.population += 5;
                Debug.Log($"{character.characterName} survived the ordeal - proven innocent!");
            }
            else
            {
                stats.currentStats.population -= 10;
                stats.currentStats.fear += 5;
                Debug.Log($"{character.characterName} died during ordeal - proven guilty!");
            }

            stats.trialByOrdealCount++;
            Debug.Log($"Subjected {character.characterName} to trial by ordeal");
        }

        // REDEMPTION QUEST Decision
        private static void RedemptionQuestEffects(Characters.Character character, bool isGuilty)
        {
            GameState stats = GameState.Instance;

            int popChange = 0; // Stays in community
            int fearChange = -random.Next(5, 9);
            int divineChange = random.Next(10, 21);
            int karmaChange = random.Next(15, 21);

            stats.currentStats.population += popChange;
            stats.currentStats.fear += fearChange;
            stats.currentStats.divineFavor += divineChange;
            stats.currentStats.karma += karmaChange;

            // Delayed outcome
            int successRate = stats.currentStats.karma > 60 ? 80 : 70;

            if (random.Next(0, 100) < successRate)
            {
                delayedEffects.Enqueue(new DelayedEffect
                {
                    turnsUntilEffect = random.Next(3, 6),
                    effectAction = () => {
                        stats.currentStats.population += 10;
                        stats.currentStats.divineFavor += 10;
                        Debug.Log($"{character.characterName} returned successful from redemption quest!");
                    },
                    description = "Successful redemption quest"
                });
            }
            else
            {
                delayedEffects.Enqueue(new DelayedEffect
                {
                    turnsUntilEffect = random.Next(3, 6),
                    effectAction = () => {
                        stats.currentStats.divineFavor -= 10;
                        stats.currentStats.population -= 5;
                        Debug.Log($"{character.characterName} failed the redemption quest!");
                    },
                    description = "Failed redemption quest"
                });
            }

            Debug.Log($"Sent {character.characterName} on redemption quest");
        }

        // PUBLIC HUMILIATION Decision
        private static void PublicHumiliationEffects(Characters.Character character, bool isGuilty)
        {
            GameState stats = GameState.Instance;

            int popChange = 0; // No one dies
            int fearChange = random.Next(8, 13);
            int divineChange = random.Next(3, 8);
            int karmaChange = -random.Next(3, 9);

            stats.currentStats.population += popChange;
            stats.currentStats.fear += fearChange;
            stats.currentStats.divineFavor += divineChange;
            stats.currentStats.karma += karmaChange;

            if (random.Next(0, 100) < 25)
            {
                delayedEffects.Enqueue(new DelayedEffect
                {
                    turnsUntilEffect = random.Next(3, 6),
                    effectAction = () => {
                        stats.currentStats.fear += 10;
                        Debug.Log($"{character.characterName} seeks revenge through worse crimes!");
                    },
                    description = "Humiliation leads to revenge"
                });
            }

            stats.publicHumiliationCount++;
            Debug.Log($"Publicly humiliated {character.characterName}");
        }

        // BANISH TO WILDERNESS Decision
        private static void BanishWildernessEffects(Characters.Character character, bool isGuilty)
        {
            GameState stats = GameState.Instance;

            int popChange = -random.Next(8, 13); // Likely dies in wilderness
            int fearChange = random.Next(10, 16);
            int divineChange = random.Next(12, 19);
            int karmaChange = -random.Next(5, 11);

            stats.currentStats.population += popChange;
            stats.currentStats.fear += fearChange;
            stats.currentStats.divineFavor += divineChange;
            stats.currentStats.karma += karmaChange;

            // Small chance of special return
            if (random.Next(0, 100) < 10)
            {
                delayedEffects.Enqueue(new DelayedEffect
                {
                    turnsUntilEffect = random.Next(4, 8),
                    effectAction = () => {
                        if (stats.currentStats.divineFavor > 70)
                        {
                            stats.currentStats.divineFavor += 20;
                            Debug.Log($"{character.characterName} returned as a blessed prophet!");
                        }
                        else
                        {
                            stats.currentStats.fear += 25;
                            Debug.Log($"{character.characterName} returned as a cursed demon!");
                        }
                    },
                    description = "Banished person returns transformed"
                });
            }

            Debug.Log($"Banished {character.characterName} to wilderness");
        }
    }
}
