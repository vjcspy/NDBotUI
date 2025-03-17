using LanguageExt;
using NDBotUI.Modules.Game.AutoCore.Store;
using NDBotUI.Modules.Game.AutoCore.Typing;
using NDBotUI.Modules.Game.Ete.Typing;
using NDBotUI.Modules.Game.MementoMori.Helper;
using NDBotUI.Modules.Shared.Emulator.Models;
using NDBotUI.Modules.Shared.Emulator.Store;
using NDBotUI.Modules.Shared.EventManager;
using NLog;

namespace NDBotUI.Modules.Game.Ete.Store;

public class EteReducer
{
    protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public static EteState Reduce(EteState state, EventAction action)
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
                                    EteGameInstance.Factory(emulatorConnection.Id)
                                ),
                            };
                        }
                    }
                }

                return state;
            }

            case EteAction.Type.ToggleStartStopReRoll:
            {
                if (action.Payload is not BaseActionPayload baseActionPayload)
                {
                    return state;
                }

                var emulatorId = baseActionPayload.EmulatorId;

                var isRunning = state.IsReRollJobRunning(emulatorId);
                var newJobReRollState = EteJobReRollState.Factory();
                state = state with
                {
                    GameInstances = state.GameInstances.Map(
                        gameInstance =>
                            gameInstance.EmulatorId == emulatorId
                                ? gameInstance with
                                {
                                    State = isRunning ? AutoState.Off : AutoState.On,
                                    JobType = EteJobType.ReRoll,
                                    JobReRollState = newJobReRollState with
                                    {
                                        ReRollStatus = isRunning ? EteReRollStatus.Open : EteReRollStatus.Start,
                                    },
                                }
                                : gameInstance
                    ),
                };
                TemplateImageDataHelper.ResetTemplateImagesPriority(emulatorId);
                return state;
            }

            case EteAction.Type.DetectScreen:
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

                if (baseActionPayload.Data is DetectTemplatePoint detectTemplatePoint)
                {
                    // Home step 1
                    if (Equals(detectTemplatePoint.TemplateKey, EteTemplateKey.GuideSupplyQuick)
                        || Equals(detectTemplatePoint.TemplateKey, EteTemplateKey.GuideSupplyBack)
                        || Equals(detectTemplatePoint.TemplateKey, EteTemplateKey.GuideSupplyFree)
                       )
                    {
                        state = state with
                        {
                            GameInstances = state.GameInstances.Map(
                                instance =>
                                    instance.EmulatorId == emulatorId
                                        ? instance with
                                        {
                                            JobReRollState = instance.JobReRollState with
                                            {
                                                ReRollStatus = EteReRollStatus.Step1HomeScreen,
                                            },
                                        }
                                        : instance
                            ),
                        };
                    }
                    // home step 2
                    else if (
                        gameInstance.JobReRollState.ReRollStatus < EteReRollStatus.Step2HomeScreen
                        && (Equals(detectTemplatePoint.TemplateKey, EteTemplateKey.GuideContinueBattle)
                            || Equals(detectTemplatePoint.TemplateKey, EteTemplateKey.Mission1Header)
                            || Equals(detectTemplatePoint.TemplateKey, EteTemplateKey.Mission1Task))
                    )
                    {
                        state = state with
                        {
                            GameInstances = state.GameInstances.Map(
                                instance =>
                                    instance.EmulatorId == emulatorId
                                        ? instance with
                                        {
                                            JobReRollState = instance.JobReRollState with
                                            {
                                                ReRollStatus = EteReRollStatus.Step2HomeScreen,
                                            },
                                        }
                                        : instance
                            ),
                        };
                    }
                    else if (
                        gameInstance.JobReRollState.ReRollStatus < EteReRollStatus.Step3HomeScreen
                        && Equals(detectTemplatePoint.TemplateKey, EteTemplateKey.MissionStartBtn)
                    )
                    {
                        state = state with
                        {
                            GameInstances = state.GameInstances.Map(
                                instance =>
                                    instance.EmulatorId == emulatorId
                                        ? instance with
                                        {
                                            JobReRollState = instance.JobReRollState with
                                            {
                                                ReRollStatus = EteReRollStatus.Step3HomeScreen,
                                            },
                                        }
                                        : instance
                            ),
                        };
                    }
                }

                return state;
            }

            case EteAction.Type.GetReward:
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

                state = state with
                {
                    GameInstances = state.GameInstances.Map(
                        instance =>
                            instance.EmulatorId == emulatorId
                                ? instance with
                                {
                                    JobReRollState = instance.JobReRollState with
                                    {
                                        ReRollStatus = EteReRollStatus.GetReward,
                                    },
                                }
                                : instance
                    ),
                };

                return state;
            }

            case EteAction.Type.GotEmailReward:
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

                state = state with
                {
                    GameInstances = state.GameInstances.Map(
                        instance =>
                            instance.EmulatorId == emulatorId
                                ? instance with
                                {
                                    JobReRollState = instance.JobReRollState with
                                    {
                                        ReRollStatus = EteReRollStatus.GotEmailReward,
                                    },
                                }
                                : instance
                    ),
                };

                return state;
            }

            case EteAction.Type.DoReRoll:
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

                state = state with
                {
                    GameInstances = state.GameInstances.Map(
                        instance =>
                            instance.EmulatorId == emulatorId
                                ? instance with
                                {
                                    State = AutoState.On,
                                    JobReRollState = instance.JobReRollState with
                                    {
                                        ReRollStatus = EteReRollStatus.DoReRoll,
                                    },
                                }
                                : instance
                    ),
                };

                return state;
            }

            default:
                return state;
        }
    }
}