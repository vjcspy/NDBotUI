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
                {
                    foreach (var emulatorConnection in emulatorConnections)
                    {
                        var gameInstance = state.GetGameInstance(emulatorConnection.Id);
                        if (gameInstance == null)
                        {
                            state = state with
                            {
                                GameInstances = state.GameInstances.Add(
                                    GameInstance.Factory(emulatorConnection.Id)
                                ),
                            };
                        }
                    }
                }

                return state;
            }

            /* _____________________________ ReRoll _____________________________*/
            case MoriAction.Type.ToggleStartStopMoriReRoll:
            {
                if (action.Payload is not BaseActionPayload baseActionPayload)
                {
                    return state;
                }

                var emulatorId = baseActionPayload.EmulatorId;

                var isRunning = state.IsReRollJobRunning(emulatorId);
                var newJobReRollState = JobReRollState.Factory();
                state = state with
                {
                    GameInstances = state.GameInstances.Map(
                        gameInstance =>
                            gameInstance.EmulatorId == emulatorId
                                ? gameInstance with
                                {
                                    State = isRunning ? AutoState.Off : AutoState.On,
                                    JobType = MoriJobType.ReRoll,
                                    JobReRollState = newJobReRollState with
                                    {
                                        ReRollStatus = isRunning ? ReRollStatus.Open : ReRollStatus.Start,
                                    },
                                }
                                : gameInstance
                    ),
                };
                TemplateImageDataHelper.ResetTemplateImagesPriority(emulatorId);
                return state;
            }
            case MoriAction.Type.EligibilityChapterCheck:
            {
                if (action.Payload is not BaseActionPayload baseActionPayload)
                {
                    return state;
                }

                var emulatorId = baseActionPayload.EmulatorId;

                state = state with
                {
                    GameInstances = state.GameInstances.Map(
                        gameInstance =>
                            gameInstance.EmulatorId == emulatorId
                                ? gameInstance with
                                {
                                    JobReRollState = gameInstance.JobReRollState with
                                    {
                                        ReRollStatus = ReRollStatus.EligibilityChapterCheck,
                                    },
                                }
                                : gameInstance
                    ),
                };

                return state;
            }

            case MoriAction.Type.DetectedMoriScreen:
            {
                if (action.Payload is not BaseActionPayload baseActionPayload
                    || baseActionPayload.Data is not DetectedTemplatePoint detectedTemplatePoint)
                {
                    return state;
                }

                var emulatorId = baseActionPayload.EmulatorId;


                // Save current Screen Template
                state = state with
                {
                    GameInstances = state.GameInstances.Map(
                        gameInstance =>
                            gameInstance.EmulatorId == emulatorId
                                ? gameInstance with
                                {
                                    JobReRollState = gameInstance.JobReRollState with
                                    {
                                        DetectScreenTry = 0,
                                        CurrentScreenTemplate = detectedTemplatePoint.MoriTemplateKey,
                                        LastScreenTemplate = gameInstance.JobReRollState.CurrentScreenTemplate,
                                    },
                                }
                                : gameInstance
                    ),
                };

                // Chỉ ưu tiên check current chapter duy nhất 1 lần
                MoriTemplateKey[] currentChapter =
                [
                    MoriTemplateKey.BeforeChallengeEnemyPower15,
                    MoriTemplateKey.BeforeChallengeEnemyPower16,
                    MoriTemplateKey.BeforeChallengeEnemyPower17,
                    MoriTemplateKey.BeforeChallengeEnemyPower18,
                    MoriTemplateKey.BeforeChallengeEnemyPower19,
                    MoriTemplateKey.BeforeChallengeEnemyPower111,
                    MoriTemplateKey.BeforeChallengeEnemyPower112,
                ];
                if (currentChapter.Contains(detectedTemplatePoint.MoriTemplateKey))
                {
                    state = state with
                    {
                        GameInstances = state.GameInstances.Map(
                            gameInstance =>
                                gameInstance.EmulatorId == emulatorId
                                    ? gameInstance with
                                    {
                                        JobReRollState = gameInstance.JobReRollState with
                                        {
                                            CurrentLevel = (int)detectedTemplatePoint.MoriTemplateKey,
                                            ReRollStatus = ReRollStatus.EligibilityChapterPassed,
                                        },
                                    }
                                    : gameInstance
                        ),
                    };
                    // Sau đó không ưu tiên nữa
                    Logger.Info($"Reduce Priority for template {detectedTemplatePoint.MoriTemplateKey.ToString()}");
                    TemplateImageDataHelper
                        .TemplateImageData[detectedTemplatePoint.MoriTemplateKey]
                        .SetPriority(emulatorId, 101);
                }

                // Nếu là Màn 2-2 thì chuyển ngay đến save result
                if (detectedTemplatePoint.MoriTemplateKey == MoriTemplateKey.BeforeChallengeEnemyPower22)
                {
                    state = state with
                    {
                        GameInstances = state.GameInstances.Map(
                            gameInstance =>
                                gameInstance.EmulatorId == emulatorId
                                    ? gameInstance with
                                    {
                                        State = AutoState.On,
                                        Status = "",
                                        JobReRollState = gameInstance.JobReRollState with
                                        {
                                            ReRollStatus = ReRollStatus.SaveResult,
                                        },
                                    }
                                    : gameInstance
                        ),
                    };

                    return state;
                }

                // Lần đầu tiên vào các chapter này sẽ check level up
                // Thử nghiệm cơ chế mới sẽ bỏ hard code vào màn để check.
                // MoriTemplateKey[] levelUpCheckForChapters =
                // [
                //     MoriTemplateKey.BeforeChallengeEnemyPower17,
                //     MoriTemplateKey.BeforeChallengeEnemyPower19,
                // ];
                // if (levelUpCheckForChapters.Contains(detectedTemplatePoint.MoriTemplateKey))
                // {
                //     state = state with
                //     {
                //         GameInstances = state.GameInstances.Map(
                //             gameInstance =>
                //                 gameInstance.EmulatorId == emulatorId
                //                 && gameInstance.JobReRollState.ReRollStatus < ReRollStatus.EligibilityLevelCheck
                //                     ? gameInstance with
                //                     {
                //                         JobReRollState = gameInstance.JobReRollState with
                //                         {
                //                             ReRollStatus = ReRollStatus.EligibilityLevelCheck,
                //                         },
                //                     }
                //                     : gameInstance
                //         ),
                //     };
                // }

                return state;
            }

            case MoriAction.Type.EligibilityLevelCheck:
            {
                if (action.Payload is not BaseActionPayload baseActionPayload)
                {
                    return state;
                }

                var emulatorId = baseActionPayload.EmulatorId;

                state = state with
                {
                    GameInstances = state.GameInstances.Map(
                        gameInstance =>
                            gameInstance.EmulatorId == emulatorId
                                ? gameInstance with
                                {
                                    JobReRollState = gameInstance.JobReRollState with
                                    {
                                        ReRollStatus = ReRollStatus.EligibilityLevelCheck,
                                    },
                                }
                                : gameInstance
                    ),
                };

                return state;
            }

            case MoriAction.Type.EligibilityLevelPassed:
            {
                if (action.Payload is not BaseActionPayload baseActionPayload)
                {
                    return state;
                }

                var emulatorId = baseActionPayload.EmulatorId;

                state = state with
                {
                    GameInstances = state.GameInstances.Map(
                        gameInstance =>
                            gameInstance.EmulatorId == emulatorId
                                ? gameInstance with
                                {
                                    JobReRollState = gameInstance.JobReRollState with
                                    {
                                        ReRollStatus = ReRollStatus.EligibilityLevelPassed,
                                    },
                                }
                                : gameInstance
                    ),
                };

                return state;
            }

            case MoriAction.Type.ResetUserData:
            {
                if (action.Payload is not BaseActionPayload baseActionPayload)
                {
                    return state;
                }

                var emulatorId = baseActionPayload.EmulatorId;

                state = state with
                {
                    GameInstances = state.GameInstances.Map(
                        gameInstance =>
                            gameInstance.EmulatorId == emulatorId
                                ? gameInstance with
                                {
                                    JobReRollState = gameInstance.JobReRollState with
                                    {
                                        ReRollStatus = ReRollStatus.ResetUserData,
                                    },
                                }
                                : gameInstance
                    ),
                };

                return state;
            }

            case MoriAction.Type.EligibilityLevelCheckOnChar:
            {
                if (action.Payload is not BaseActionPayload baseActionPayload)
                {
                    return state;
                }

                var emulatorId = baseActionPayload.EmulatorId;

                state = state with
                {
                    GameInstances = state.GameInstances.Map(
                        gameInstance =>
                            gameInstance.EmulatorId == emulatorId
                                ? gameInstance with
                                {
                                    JobReRollState = gameInstance.JobReRollState with
                                    {
                                        ReRollStatus = ReRollStatus.EligibilityLevelCheckOnChar,
                                    },
                                }
                                : gameInstance
                    ),
                };

                return state;
            }

            case MoriAction.Type.EligibilityLevelCheckOnCharOk:
            {
                if (action.Payload is not BaseActionPayload baseActionPayload)
                {
                    return state;
                }

                var emulatorId = baseActionPayload.EmulatorId;

                var currentGameInstance = state.GetGameInstance(emulatorId);
                if (currentGameInstance is not null && currentGameInstance.JobReRollState.LevelUpCharPosition == 3)
                {
                    state = state with
                    {
                        GameInstances = state.GameInstances.Map(
                            gameInstance =>
                                gameInstance.EmulatorId == emulatorId
                                    ? gameInstance with
                                    {
                                        JobReRollState = gameInstance.JobReRollState with
                                        {
                                            ReRollStatus = ReRollStatus.EligibilityLevelPassed,
                                        },
                                    }
                                    : gameInstance
                        ),
                    };
                }
                else
                {
                    state = state with
                    {
                        GameInstances = state.GameInstances.Map(
                            gameInstance =>
                                gameInstance.EmulatorId == emulatorId
                                    ? gameInstance with
                                    {
                                        JobReRollState = gameInstance.JobReRollState with
                                        {
                                            ReRollStatus = ReRollStatus.EligibilityLevelCheckOnChar,
                                            LevelUpCharPosition = gameInstance.JobReRollState.LevelUpCharPosition + 1,
                                        },
                                    }
                                    : gameInstance
                        ),
                    };
                }

                return state;
            }

            default:
                return state;
        }
    }
}