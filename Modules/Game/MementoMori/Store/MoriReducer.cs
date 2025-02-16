﻿using System.Linq;
using LanguageExt;
using NDBotUI.Modules.Game.AutoCore.Store;
using NDBotUI.Modules.Game.AutoCore.Typing;
using NDBotUI.Modules.Game.MementoMori.Helper;
using NDBotUI.Modules.Game.MementoMori.Store.State;
using NDBotUI.Modules.Game.MementoMori.Typing;
using NDBotUI.Modules.Shared.Emulator.Models;
using NDBotUI.Modules.Shared.Emulator.Store;
using NDBotUI.Modules.Shared.EventManager;
using NLog;

namespace NDBotUI.Modules.Game.MementoMori.Store;

public class MoriReducer
{
    protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public static MoriState Reduce(MoriState state, EventAction action)
    {
        switch (action.Type)
        {
            case EmulatorAction.Type.EmulatorConnectSuccess:
            {
                if (action.Payload is Lst<EmulatorConnection> emulatorConnections)
                    foreach (var emulatorConnection in emulatorConnections)
                    {
                        var gameInstance = state.GetGameInstance(emulatorConnection.Id);
                        if (gameInstance == null)
                            state = state with
                            {
                                GameInstances = state.GameInstances.Add(
                                    GameInstance.Factory(emulatorConnection.Id)
                                )
                            };
                    }

                return state;
            }

            /* _____________________________ ReRoll _____________________________*/
            case MoriAction.Type.ToggleStartStopMoriReRoll:
            {
                if (action.Payload is not BaseActionPayload baseActionPayload) return state;
                var emulatorId = baseActionPayload.EmulatorId;

                var isRunning = state.IsReRollJobRunning(emulatorId);
                var newJobReRollState = new JobReRollState();
                state = state with
                {
                    GameInstances = state.GameInstances.Map(gameInstance =>
                        gameInstance.EmulatorId == emulatorId
                            ? gameInstance with
                            {
                                State = isRunning ? AutoState.Off : AutoState.On,
                                JobType = MoriJobType.ReRoll,
                                JobReRollState = newJobReRollState with
                                {
                                    ReRollStatus = isRunning ? ReRollStatus.Open : ReRollStatus.Start
                                }
                            }
                            : gameInstance
                    )
                };
                TemplateImageDataHelper.ResetTemplateImagesPriority(emulatorId);
                return state;
            }
            case MoriAction.Type.EligibilityChapterCheck:
            {
                if (action.Payload is not BaseActionPayload baseActionPayload) return state;
                var emulatorId = baseActionPayload.EmulatorId;

                state = state with
                {
                    GameInstances = state.GameInstances.Map(gameInstance =>
                        gameInstance.EmulatorId == emulatorId
                            ? gameInstance with
                            {
                                JobReRollState = gameInstance.JobReRollState with
                                {
                                    ReRollStatus = ReRollStatus.EligibilityChapterCheck
                                }
                            }
                            : gameInstance
                    )
                };

                return state;
            }

            case MoriAction.Type.DetectedMoriScreen:
            {
                if (action.Payload is not BaseActionPayload baseActionPayload ||
                    baseActionPayload.Data is not DetectedTemplatePoint detectedTemplatePoint) return state;
                var emulatorId = baseActionPayload.EmulatorId;

                state = state with
                {
                    GameInstances = state.GameInstances.Map(gameInstance =>
                        gameInstance.EmulatorId == emulatorId
                            ? gameInstance with
                            {
                                JobReRollState = gameInstance.JobReRollState with
                                {
                                    DetectScreenTry = 0,
                                    MoriCurrentScreen = detectedTemplatePoint.MoriTemplateKey,
                                    MoriLastScreen = gameInstance.JobReRollState.MoriCurrentScreen
                                }
                            }
                            : gameInstance
                    )
                };

                if (detectedTemplatePoint.MoriTemplateKey == MoriTemplateKey.BeforeChallengeEnemyPower22)
                    state = state with
                    {
                        GameInstances = state.GameInstances.Map(gameInstance =>
                            gameInstance.EmulatorId == emulatorId
                                ? gameInstance with
                                {
                                    State = AutoState.On,
                                    Status = "",
                                    JobReRollState = gameInstance.JobReRollState with
                                    {
                                        ReRollStatus = ReRollStatus.SaveResult
                                    }
                                }
                                : gameInstance
                        )
                    };

                MoriTemplateKey[] currentChapter =
                [
                    MoriTemplateKey.BeforeChallengeEnemyPower15,
                    MoriTemplateKey.BeforeChallengeEnemyPower16,
                    MoriTemplateKey.BeforeChallengeEnemyPower17,
                    MoriTemplateKey.BeforeChallengeEnemyPower19,
                    MoriTemplateKey.BeforeChallengeEnemyPower111,
                    MoriTemplateKey.BeforeChallengeEnemyPower112
                ];
                if (currentChapter.Contains(detectedTemplatePoint.MoriTemplateKey))
                {
                    state = state with
                    {
                        GameInstances = state.GameInstances.Map(gameInstance =>
                            gameInstance.EmulatorId == emulatorId
                                ? gameInstance with
                                {
                                    JobReRollState = gameInstance.JobReRollState with
                                    {
                                        CurrentLevel = (int)detectedTemplatePoint.MoriTemplateKey
                                    }
                                }
                                : gameInstance
                        )
                    };
                    // Sau đó không ưu tiên nữa
                    Logger.Info($"Reduce Priority for template {detectedTemplatePoint.MoriTemplateKey.ToString()}");
                    TemplateImageDataHelper.TemplateImageData[detectedTemplatePoint.MoriTemplateKey]
                        .SetPriority(emulatorId, 101);
                }

                MoriTemplateKey[] chapterValidEligibilityCheck =
                [
                    MoriTemplateKey.BeforeChallengeEnemyPower15,
                    MoriTemplateKey.BeforeChallengeEnemyPower16
                ];
                if (chapterValidEligibilityCheck.Contains(detectedTemplatePoint.MoriTemplateKey))
                    state = state with
                    {
                        GameInstances = state.GameInstances.Map(gameInstance =>
                            gameInstance.EmulatorId == emulatorId
                                ? gameInstance with
                                {
                                    JobReRollState = gameInstance.JobReRollState with
                                    {
                                        ReRollStatus = ReRollStatus.EligibilityChapterPassed
                                    }
                                }
                                : gameInstance
                        )
                    };

                MoriTemplateKey[] levelUpCheckForChapters =
                [
                    MoriTemplateKey.BeforeChallengeEnemyPower17,
                    MoriTemplateKey.BeforeChallengeEnemyPower19
                ];
                if (levelUpCheckForChapters.Contains(detectedTemplatePoint.MoriTemplateKey))
                    state = state with
                    {
                        GameInstances = state.GameInstances.Map(gameInstance =>
                            gameInstance.EmulatorId == emulatorId && gameInstance.JobReRollState.ReRollStatus <
                            ReRollStatus.EligibilityLevelCheck
                                ? gameInstance with
                                {
                                    JobReRollState = gameInstance.JobReRollState with
                                    {
                                        ReRollStatus = ReRollStatus.EligibilityLevelCheck
                                    }
                                }
                                : gameInstance
                        )
                    };

                return state;
            }

            case MoriAction.Type.EligibilityLevelCheck:
            {
                if (action.Payload is not BaseActionPayload baseActionPayload ||
                    baseActionPayload.Data is not DetectedTemplatePoint detectedTemplatePoint) return state;
                var emulatorId = baseActionPayload.EmulatorId;

                state = state with
                {
                    GameInstances = state.GameInstances.Map(gameInstance =>
                        gameInstance.EmulatorId == emulatorId
                            ? gameInstance with
                            {
                                JobReRollState = gameInstance.JobReRollState with
                                {
                                    ReRollStatus = ReRollStatus.EligibilityLevelCheck
                                }
                            }
                            : gameInstance
                    )
                };

                return state;
            }

            case MoriAction.Type.EligibilityLevelPass:
            {
                if (action.Payload is not BaseActionPayload baseActionPayload) return state;
                var emulatorId = baseActionPayload.EmulatorId;

                state = state with
                {
                    GameInstances = state.GameInstances.Map(gameInstance =>
                        gameInstance.EmulatorId == emulatorId
                            ? gameInstance with
                            {
                                JobReRollState = gameInstance.JobReRollState with
                                {
                                    ReRollStatus = ReRollStatus.EligibilityLevelPass
                                }
                            }
                            : gameInstance
                    )
                };

                return state;
            }

            case MoriAction.Type.ResetUserData:
            {
                if (action.Payload is not BaseActionPayload baseActionPayload) return state;
                var emulatorId = baseActionPayload.EmulatorId;

                state = state with
                {
                    GameInstances = state.GameInstances.Map(gameInstance =>
                        gameInstance.EmulatorId == emulatorId
                            ? gameInstance with
                            {
                                JobReRollState = gameInstance.JobReRollState with
                                {
                                    ReRollStatus = ReRollStatus.ResetUserData
                                }
                            }
                            : gameInstance
                    )
                };

                return state;
            }

            default:
                return state;
        }
    }
}