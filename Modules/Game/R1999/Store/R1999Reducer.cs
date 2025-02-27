using System;
using LanguageExt;
using NDBotUI.Modules.Core.Db;
using NDBotUI.Modules.Game.AutoCore.Store;
using NDBotUI.Modules.Game.AutoCore.Typing;
using NDBotUI.Modules.Game.MementoMori.Helper;
using NDBotUI.Modules.Game.R1999.Helper;
using NDBotUI.Modules.Game.R1999.Service;
using NDBotUI.Modules.Game.R1999.Typing;
using NDBotUI.Modules.Shared.Emulator.Models;
using NDBotUI.Modules.Shared.Emulator.Store;
using NDBotUI.Modules.Shared.EventManager;
using NLog;

namespace NDBotUI.Modules.Game.R1999.Store;

public class R1999Reducer
{
    protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public static R1999State Reduce(R1999State state, EventAction action)
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
                                    R1999GameInstance.Factory(emulatorConnection.Id)
                                ),
                            };
                        }
                    }
                }

                return state;
            }

            case R1999Action.Type.ToggleStartStopReRoll:
            {
                if (action.Payload is not BaseActionPayload baseActionPayload)
                {
                    return state;
                }

                var emulatorId = baseActionPayload.EmulatorId;

                var isRunning = state.IsReRollJobRunning(emulatorId);
                var newJobReRollState = R1999JobReRollState.Factory();
                state = state with
                {
                    GameInstances = state.GameInstances.Map(
                        gameInstance =>
                            gameInstance.EmulatorId == emulatorId
                                ? gameInstance with
                                {
                                    State = isRunning ? AutoState.Off : AutoState.On,
                                    JobType = R1999JobType.ReRoll,
                                    JobReRollState = newJobReRollState with
                                    {
                                        ReRollStatus = isRunning ? R1999ReRollStatus.Open : R1999ReRollStatus.Start,
                                    },
                                }
                                : gameInstance
                    ),
                };
                TemplateImageDataHelper.ResetTemplateImagesPriority(emulatorId);
                return state;
            }

            case R1999Action.Type.CouldNotDetectScreen:
            {
                if (action.Payload is not BaseActionPayload baseActionPayload)
                {
                    return state;
                }

                var emulatorId = baseActionPayload.EmulatorId;

                var gameInstance = state.GetGameInstance(emulatorId);
                if (gameInstance == null)
                {
                    return state;
                }

                var newJobReRollState = gameInstance.JobReRollState with
                {
                    DetectScreenTry = gameInstance.JobReRollState.DetectScreenTry + 1,
                    CurrentScreen =  new CurrentScreen(R1999TemplateKey.Unknown.ToString()),
                };

                state = state with
                {
                    GameInstances = state.GameInstances.Map(
                        instance =>
                            instance.EmulatorId == emulatorId
                                ? instance with
                                {
                                    JobReRollState = newJobReRollState,
                                }
                                : instance
                    ),
                };

                return state;
            }

            case R1999Action.Type.DetectScreen:
            {
                if (action.Payload is not BaseActionPayload baseActionPayload)
                {
                    return state;
                }

                var emulatorId = baseActionPayload.EmulatorId;
                var detectedTemplatePoint = baseActionPayload.Data as DetectTemplatePoint;

                var gameInstance = state.GetGameInstance(emulatorId);
                if (gameInstance == null || detectedTemplatePoint == null)
                {
                    return state;
                }

                var newJobReRollState = gameInstance.JobReRollState with
                {
                    CurrentScreen = new CurrentScreen(detectedTemplatePoint.TemplateKey.ToString()),
                    DetectScreenTry = 0,
                };


                if (Equals(detectedTemplatePoint.TemplateKey, R1999TemplateKey.Chapter5Text))
                {
                    newJobReRollState = newJobReRollState with
                    {
                        ReRollStatus = R1999ReRollStatus.FinishQuest,
                        Ordinal = gameInstance.JobReRollState.Ordinal == ""
                            ? Guid
                                .NewGuid()
                                .ToString()
                            : gameInstance.JobReRollState.Ordinal,
                    };
                }

                state = state with
                {
                    GameInstances = state.GameInstances.Map(
                        instance =>
                            instance.EmulatorId == emulatorId
                                ? instance with
                                {
                                    JobReRollState = newJobReRollState,
                                }
                                : instance
                    ),
                };

                return state;
            }

            case R1999Action.Type.GotDailyReward:
            {
                if (action.Payload is not BaseActionPayload baseActionPayload)
                {
                    return state;
                }

                var emulatorId = baseActionPayload.EmulatorId;

                var gameInstance = state.GetGameInstance(emulatorId);
                if (gameInstance == null)
                {
                    return state;
                }

                var newJobReRollState = gameInstance.JobReRollState with
                {
                    ReRollStatus = R1999ReRollStatus.Got1UniCurrentDay,
                };

                state = state with
                {
                    GameInstances = state.GameInstances.Map(
                        instance =>
                            instance.EmulatorId == emulatorId
                                ? instance with
                                {
                                    JobReRollState = newJobReRollState,
                                }
                                : instance
                    ),
                };

                return state;
            }

            case R1999Action.Type.GotChapterReward:
            {
                if (action.Payload is not BaseActionPayload baseActionPayload)
                {
                    return state;
                }

                var emulatorId = baseActionPayload.EmulatorId;

                var gameInstance = state.GetGameInstance(emulatorId);
                if (gameInstance == null)
                {
                    return state;
                }

                var newJobReRollState = gameInstance.JobReRollState with
                {
                    ReRollStatus = R1999ReRollStatus.GotChapterReward,
                };

                state = state with
                {
                    GameInstances = state.GameInstances.Map(
                        instance =>
                            instance.EmulatorId == emulatorId
                                ? instance with
                                {
                                    JobReRollState = newJobReRollState,
                                }
                                : instance
                    ),
                };

                return state;
            }
            case R1999Action.Type.GotEmailReward:
            {
                if (action.Payload is not BaseActionPayload baseActionPayload)
                {
                    return state;
                }

                var emulatorId = baseActionPayload.EmulatorId;

                var gameInstance = state.GetGameInstance(emulatorId);
                if (gameInstance == null)
                {
                    return state;
                }

                var newJobReRollState = gameInstance.JobReRollState with
                {
                    ReRollStatus = R1999ReRollStatus.GotEmailReward,
                };

                state = state with
                {
                    GameInstances = state.GameInstances.Map(
                        instance =>
                            instance.EmulatorId == emulatorId
                                ? instance with
                                {
                                    JobReRollState = newJobReRollState,
                                }
                                : instance
                    ),
                };

                return state;
            }

            case R1999Action.Type.RollFinished:
            {
                if (action.Payload is not BaseActionPayload baseActionPayload)
                {
                    return state;
                }

                var emulatorId = baseActionPayload.EmulatorId;

                var gameInstance = state.GetGameInstance(emulatorId);
                if (gameInstance == null)
                {
                    return state;
                }

                var newJobReRollState = gameInstance.JobReRollState with
                {
                    ReRollStatus = R1999ReRollStatus.RollFinished,
                };

                state = state with
                {
                    GameInstances = state.GameInstances.Map(
                        instance =>
                            instance.EmulatorId == emulatorId
                                ? instance with
                                {
                                    JobReRollState = newJobReRollState,
                                }
                                : instance
                    ),
                };

                return state;
            }

            case R1999Action.Type.SaveResultOk:
            {
                if (action.Payload is not BaseActionPayload baseActionPayload)
                {
                    return state;
                }

                var emulatorId = baseActionPayload.EmulatorId;

                var gameInstance = state.GetGameInstance(emulatorId);
                if (gameInstance == null)
                {
                    return state;
                }

                using var context = new ApplicationDbContext();
                var accountService = new R1999AccountService(context);
                var nextAccount = accountService.CreateNewAccount(R1999DataHelper.GetAccountEmail());
                Logger.Info(">> Created new account in DB");

                var newJobReRollState = gameInstance.JobReRollState with
                {
                    ReRollStatus = R1999ReRollStatus.SaveResultOk,
                    Ordinal = nextAccount.Ordinal.ToString(),
                };

                state = state with
                {
                    GameInstances = state.GameInstances.Map(
                        instance =>
                            instance.EmulatorId == emulatorId
                                ? instance with
                                {
                                    JobReRollState = newJobReRollState,
                                    State = AutoState.On
                                }
                                : instance
                    ),
                };

                return state;
            }

            case R1999Action.Type.ClickedSendCode:
            {
                if (action.Payload is not BaseActionPayload baseActionPayload)
                {
                    return state;
                }

                var emulatorId = baseActionPayload.EmulatorId;

                var gameInstance = state.GetGameInstance(emulatorId);
                if (gameInstance == null)
                {
                    return state;
                }

                var newJobReRollState = gameInstance.JobReRollState with
                {
                    ReRollStatus = R1999ReRollStatus.ClickedSendCode,
                };

                state = state with
                {
                    GameInstances = state.GameInstances.Map(
                        instance =>
                            instance.EmulatorId == emulatorId
                                ? instance with
                                {
                                    JobReRollState = newJobReRollState,
                                }
                                : instance
                    ),
                };

                return state;
            }

            case R1999Action.Type.SentCode:
            {
                if (action.Payload is not BaseActionPayload baseActionPayload)
                {
                    return state;
                }

                var emulatorId = baseActionPayload.EmulatorId;

                var gameInstance = state.GetGameInstance(emulatorId);
                if (gameInstance == null)
                {
                    return state;
                }

                var newJobReRollState = gameInstance.JobReRollState with
                {
                    ReRollStatus = R1999ReRollStatus.SentCode,
                };

                state = state with
                {
                    GameInstances = state.GameInstances.Map(
                        instance =>
                            instance.EmulatorId == emulatorId
                                ? instance with
                                {
                                    JobReRollState = newJobReRollState,
                                }
                                : instance
                    ),
                };

                return state;
            }

            case R1999Action.Type.RegisteredAccount:
            {
                if (action.Payload is not BaseActionPayload baseActionPayload)
                {
                    return state;
                }

                var emulatorId = baseActionPayload.EmulatorId;

                var gameInstance = state.GetGameInstance(emulatorId);
                if (gameInstance == null)
                {
                    return state;
                }

                var newJobReRollState = gameInstance.JobReRollState with
                {
                    ReRollStatus = R1999ReRollStatus.Start,
                };

                state = state with
                {
                    GameInstances = state.GameInstances.Map(
                        instance =>
                            instance.EmulatorId == emulatorId
                                ? instance with
                                {
                                    JobReRollState = newJobReRollState,
                                }
                                : instance
                    ),
                };

                return state;
            }
        }



        return state;
    }
}