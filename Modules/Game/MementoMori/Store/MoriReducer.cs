using System.Linq;
using NDBotUI.Modules.Game.AutoCore.Store;
using NDBotUI.Modules.Game.AutoCore.Typing;
using NDBotUI.Modules.Game.MementoMori.Store.State;
using NDBotUI.Modules.Game.MementoMori.Typing;
using NDBotUI.Modules.Shared.EventManager;

namespace NDBotUI.Modules.Game.MementoMori.Store;

public class MoriReducer
{
    public static MoriState Reduce(MoriState state, EventAction action)
    {
        switch (action.Type)
        {
            case MoriAction.Type.InitMoriSuccess:
            {
                if (action.Payload is BaseActionPayload baseActionPayload)
                {
                    var gameInstance = state.GetGameInstance(baseActionPayload.EmulatorId);
                    if (gameInstance == null)
                        state = state with
                        {
                            GameInstances = state.GameInstances.Add(
                                new GameInstance(
                                    baseActionPayload.EmulatorId,
                                    AutoState.Off,
                                    "",
                                    MoriJobType.None,
                                    new JobReRollState()
                                )
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

                state = state with
                {
                    GameInstances = state.GameInstances.Map(gameInstance =>
                        gameInstance.EmulatorId == emulatorId
                            ? gameInstance with
                            {
                                State = isRunning ? AutoState.Off : AutoState.On,
                                JobType = MoriJobType.ReRoll,
                                JobReRollState = gameInstance.JobReRollState with
                                {
                                    ReRollStatus = isRunning ? ReRollStatus.Open : ReRollStatus.Start
                                }
                            }
                            : gameInstance
                    )
                };

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

                MoriTemplateKey[] checkLv =
                [
                    MoriTemplateKey.BeforeChallengeEnemyPower17
                ];
                if (checkLv.Contains(detectedTemplatePoint.MoriTemplateKey))
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

            default:
                return state;
        }
    }
}