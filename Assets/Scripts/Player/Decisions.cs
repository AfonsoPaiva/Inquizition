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
            BanishWilderness,
            AcceptBribe,
            // New context-dependent decisions
            SpareWithWarning,
            CollectivePunishment,
            SacrificeToGod,
            Corruption,
            AskGodForGuidance
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
            DecisionType.BanishWilderness,
            DecisionType.SpareWithWarning,
            DecisionType.AcceptBribe,
            DecisionType.CollectivePunishment,
            DecisionType.SacrificeToGod,
            DecisionType.Corruption,
            DecisionType.AskGodForGuidance
        };

        // Current active decisions for the buttons
        private static List<DecisionType> currentActiveDecisions = new List<DecisionType>();

        public static void InitializeDecisions()
        {
            SelectRandomDecisions();
            delayedEffects.Clear();
        }

        public static void SelectRandomDecisions(Characters.Character currentCharacter = null, GameState gameState = null)
        {
            currentActiveDecisions.Clear();

            // Create a pool of ALL available decisions
            List<DecisionType> availablePool = new List<DecisionType>(allDecisions);

            // Add context-dependent decisions if conditions are met FIRST
            // This ensures they have priority in the random selection
            List<DecisionType> contextDecisions = new List<DecisionType>();

            if (currentCharacter != null && gameState != null)
            {
                // SPARE WITH WARNING - Only if first-time offender, minor crime, shows remorse
                if (currentCharacter.isFirstTimeOffender &&
                    currentCharacter.crimeSeverity == "minor" &&
                    currentCharacter.showsRemorse &&
                    !contextDecisions.Contains(DecisionType.SpareWithWarning))
                {
                    contextDecisions.Add(DecisionType.SpareWithWarning);
                    availablePool.Remove(DecisionType.SpareWithWarning);
                }

                // COLLECTIVE PUNISHMENT - Only if part of conspiracy
                if (currentCharacter.isPartOfConspiracy &&
                    !contextDecisions.Contains(DecisionType.CollectivePunishment))
                {
                    contextDecisions.Add(DecisionType.CollectivePunishment);
                    availablePool.Remove(DecisionType.CollectivePunishment);
                }

                // SACRIFICE TO GOD - Only if Divine Favor < 20
                if (gameState.currentStats.divineFavor < 20 &&
                    !contextDecisions.Contains(DecisionType.SacrificeToGod))
                {
                    contextDecisions.Add(DecisionType.SacrificeToGod);
                    availablePool.Remove(DecisionType.SacrificeToGod);
                }

                // CORRUPTION - Only if character offers bribe and player has low divine favor
                if (currentCharacter.offersBribe && gameState.currentStats.divineFavor < 40 &&
                    !contextDecisions.Contains(DecisionType.Corruption))
                {
                    contextDecisions.Add(DecisionType.Corruption);
                    availablePool.Remove(DecisionType.Corruption);
                }

                // ASK GOD FOR GUIDANCE - Only if Divine Favor > 60 and used less than 2 times
                if (gameState.currentStats.divineFavor > 60 && gameState.askGodCount < 2 &&
                    !contextDecisions.Contains(DecisionType.AskGodForGuidance))
                {
                    contextDecisions.Add(DecisionType.AskGodForGuidance);
                    availablePool.Remove(DecisionType.AskGodForGuidance);
                }
            }

            // Add context decisions first (they're guaranteed to appear if conditions are met)
            currentActiveDecisions.AddRange(contextDecisions);

            // Shuffle the remaining pool
            availablePool = availablePool.OrderBy(x => random.Next()).ToList();

            // Calculate how many more decisions we need to reach 3 total
            int decisionsNeeded = 3 - currentActiveDecisions.Count;

            // Add random decisions from the available pool
            for (int i = 0; i < decisionsNeeded && availablePool.Count > 0; i++)
            {
                currentActiveDecisions.Add(availablePool[0]);
                availablePool.RemoveAt(0);
            }

            // If we still don't have 3 decisions (edge case), add any remaining
            while (currentActiveDecisions.Count < 3 && availablePool.Count > 0)
            {
                currentActiveDecisions.Add(availablePool[0]);
                availablePool.RemoveAt(0);
            }

            // Final shuffle to randomize button positions
            currentActiveDecisions = currentActiveDecisions.OrderBy(x => random.Next()).ToList();

        }

        public static List<DecisionType> GetCurrentActiveDecisions()
        {
            return new List<DecisionType>(currentActiveDecisions);
        }

        public static string GetDecisionDisplayName(DecisionType decisionType)
        {
            switch (decisionType)
            {
                case DecisionType.Execute: return "Execute";
                case DecisionType.Exile: return "Exile";
                case DecisionType.Forgive: return "Forgive";
                case DecisionType.Confiscate: return "Confiscate";
                case DecisionType.Imprison: return "Imprison";
                case DecisionType.Torture: return "Torture";
                case DecisionType.TrialByOrdeal: return "Trial By Order";
                case DecisionType.RedemptionQuest: return "Redemption Quest";
                case DecisionType.PublicHumiliation: return "Public humiliation";
                case DecisionType.BanishWilderness: return "Banish To Wilderness";
                // New decisions
                case DecisionType.SpareWithWarning: return "Spare (With Warning)";
                case DecisionType.CollectivePunishment: return "Collective Punisment";
                case DecisionType.SacrificeToGod: return "Sacrifice To God";
                case DecisionType.Corruption: return "Accept Bribe";
                case DecisionType.AskGodForGuidance: return "Ask God for Guidence";
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
                // New decisions
                case DecisionType.SpareWithWarning: return "Show mercy to first-time offenders";
                case DecisionType.CollectivePunishment: return "Punish entire group for conspiracy";
                case DecisionType.SacrificeToGod: return "Desperate measure to regain divine favor";
                case DecisionType.Corruption: return "Accept bribe for favorable judgment";
                case DecisionType.AskGodForGuidance: return "Divine revelation of truth";
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

            // Store decision type for post-decision dialogue
            stats.lastDecisionType = decisionType.ToString();

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
                // New decision effects
                case DecisionType.SpareWithWarning:
                    SpareWithWarningEffects(currentCharacter, isGuilty);
                    break;
                case DecisionType.CollectivePunishment:
                    CollectivePunishmentEffects(currentCharacter, isGuilty);
                    break;
                case DecisionType.SacrificeToGod:
                    SacrificeToGodEffects(currentCharacter, isGuilty);
                    break;
                case DecisionType.Corruption:
                    CorruptionEffects(currentCharacter, isGuilty);
                    break;
                case DecisionType.AskGodForGuidance:
                    AskGodForGuidanceEffects(currentCharacter, isGuilty);
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
                    },
                    description = "High fear divine disapproval"
                });
            }

            stats.recentExecutions++;
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
                    },
                    description = "Bad rumors spread"
                });
            }

            stats.recentExiles++;
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
                    },
                    description = "Divine blessing"
                });
            }

            stats.recentForgives++;
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

            // Add gold for confiscation
            stats.currentStats.gold += random.Next(20, 41);

            if (!isGuilty && stats.confiscatedFromInnocents >= 2)
            {
                delayedEffects.Enqueue(new DelayedEffect
                {
                    turnsUntilEffect = random.Next(2, 4),
                    effectAction = () => {
                        stats.currentStats.divineFavor -= 15;
                    },
                    description = "Divine punishment for confiscating from innocents"
                });
            }

            if (!isGuilty) stats.confiscatedFromInnocents++;
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
                    },
                    description = "Prison escape"
                });
            }

            stats.currentPrisoners++;
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

            if (random.Next(0, 100) > 60)
            {
                stats.currentStats.fear -= 5; // People see through the deception
            }

            stats.tortureCount++;
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
            }
            else
            {
                stats.currentStats.population -= 10;
                stats.currentStats.fear += 5;
            }

            stats.trialByOrdealCount++;
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
                    },
                    description = "Failed redemption quest"
                });
            }

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
                    },
                    description = "Humiliation leads to revenge"
                });
            }

            stats.publicHumiliationCount++;
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
                        }
                        else
                        {
                            stats.currentStats.fear += 25;
                        }
                    },
                    description = "Banished person returns transformed"
                });
            }

        }

        // SPARE WITH WARNING Decision
        private static void SpareWithWarningEffects(Characters.Character character, bool isGuilty)
        {
            GameState stats = GameState.Instance;

            int popChange = 3;
            int fearChange = -5;
            int divineChange = 5;
            int karmaChange = 8;

            stats.currentStats.population += popChange;
            stats.currentStats.fear += fearChange;
            stats.currentStats.divineFavor += divineChange;
            stats.currentStats.karma += karmaChange;

            // Special: Mark character in database; if they return, punishment must be severe
            if (!stats.sparedCharacters.Contains(character.characterName))
            {
                stats.sparedCharacters.Add(character.characterName);
            }

        }

        // COLLECTIVE PUNISHMENT Decision
        private static void CollectivePunishmentEffects(Characters.Character character, bool isGuilty)
        {
            GameState stats = GameState.Instance;

            int popChange = -random.Next(20, 41);
            int fearChange = random.Next(30, 41);
            int divineChange = isGuilty ? random.Next(-15, 16) : -random.Next(15, 26);
            int karmaChange = -random.Next(25, 36);

            stats.currentStats.population += popChange;
            stats.currentStats.fear += fearChange;
            stats.currentStats.divineFavor += divineChange;
            stats.currentStats.karma += karmaChange;

            // Special: Prevents rebellion for 5-8 cards
            delayedEffects.Enqueue(new DelayedEffect
            {
                turnsUntilEffect = random.Next(5, 9),
                effectAction = () => {
                    stats.currentStats.fear += 10; // Rebellion fear returns
                },
                description = "Collective punishment rebellion prevention"
            });

        }

        // SACRIFICE TO GOD Decision
        private static void SacrificeToGodEffects(Characters.Character character, bool isGuilty)
        {
            GameState stats = GameState.Instance;

            int popChange = -random.Next(15, 21);
            int fearChange = random.Next(25, 36);
            int divineChange = random.Next(40, 51);
            int karmaChange = -random.Next(30, 41);

            stats.currentStats.population += popChange;
            stats.currentStats.fear += fearChange;
            stats.currentStats.divineFavor += divineChange;
            stats.currentStats.karma += karmaChange;

            // Special: Resets Divine Favor crisis
            stats.divineFavorCrisis = false;

        }

        // CORRUPTION Decision
        private static void CorruptionEffects(Characters.Character character, bool isGuilty)
        {
            GameState stats = GameState.Instance;

            int popChange = 0;
            int fearChange = -10;
            int divineChange = -random.Next(20, 31);
            int karmaChange = -random.Next(20, 31);

            stats.currentStats.population += popChange;
            stats.currentStats.fear += fearChange;
            stats.currentStats.divineFavor += divineChange;
            stats.currentStats.karma += karmaChange;

            // Special: Unlocks "corruption path" with more bribe opportunities
            stats.corruptionLevel++;
            stats.currentStats.gold += 50;

        }

        // ASK GOD FOR GUIDANCE Decision
        private static void AskGodForGuidanceEffects(Characters.Character character, bool isGuilty)
        {
            GameState stats = GameState.Instance;

            int divineChange = -10;
            stats.currentStats.divineFavor += divineChange;
            stats.askGodCount++;

            // Special: Reveals true guilt/innocence of character
            stats.godRevealedTruth = true;
            stats.revealedGuiltStatus = character.isGuilty;

        }
    }
}