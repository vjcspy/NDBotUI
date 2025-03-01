using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NDBotUI.Modules.Core.Helper;
using NDBotUI.Modules.Core.Store;
using NDBotUI.Modules.Game.AutoCore.Helper;
using NDBotUI.Modules.Game.AutoCore.Store;
using NDBotUI.Modules.Game.R1999.Helper;
using NDBotUI.Modules.Shared.Emulator.Models;
using NDBotUI.Modules.Shared.Emulator.Services;
using NDBotUI.Modules.Shared.Emulator.Typing;
using NDBotUI.Modules.Shared.EventManager;

namespace NDBotUI.Modules.Game.R1999.Store.Effects.ReRollEffects;

public class WhenDetectedScreenNewAccountEffect : DetectScreenEffectBase
{
    private readonly Enum[] _clickOnTemplateKeys =
    [
        R1999TemplateKey.SettingButton,
        R1999TemplateKey.LogOutExitBtn,
        R1999TemplateKey.ConfirmBtn,
        R1999TemplateKey.LoginLogoutBtn,
        R1999TemplateKey.LoginAnotherAccBtn,
        R1999TemplateKey.LoginWithEmailBtn,
        R1999TemplateKey.RegisterBtn,
    ];

    private readonly GmailAPIHelper GmailAPIHelper = new();

    protected override bool IsParallel()
    {
        return false;
    }

    protected override IEventActionFactory[] GetAllowEventActions()
    {
        return [R1999Action.DetectScreen,];
    }

    protected override async Task<EventAction> Process(EventAction action)
    {
        Logger.Info(">>Process WhenDetectedScreenNewAccountEffect");
        if (action.Payload is not BaseActionPayload baseActionPayload
            || baseActionPayload.Data is not DetectTemplatePoint detectTemplatePoint)
        {
            return CoreAction.Empty;
        }

        // Click
        var emulatorConnection = EmulatorManager.Instance.GetConnection(baseActionPayload.EmulatorId);

        if (emulatorConnection is null)
        {
            return CoreAction.Empty;
        }

        var isClicked = false;


        switch (detectTemplatePoint.TemplateKey)
        {
            case R1999TemplateKey.CharacterLevelText1:
            {
                await emulatorConnection.ClickPPointAsync(new PPoint(11.1f, 6.3f));
                isClicked = true;
                break;
            }

            case R1999TemplateKey.SummonX1Text:
            {
                await emulatorConnection.ClickPPointAsync(new PPoint(11.3f, 7.2f));
                isClicked = true;
                break;
            }

            case R1999TemplateKey.HomeMail:
            {
                await emulatorConnection.ClickPPointAsync(new PPoint(6.5f, 74.1f));
                isClicked = true;
                break;
            }

            case R1999TemplateKey.UntickButton:
            {
                await emulatorConnection.ClickPPointAsync(new PPoint(35f, 62.2f));
                isClicked = true;
                break;
            }

            case R1999TemplateKey.RegisterAccHeader:
            case R1999TemplateKey.RegisterAccHeader1:
            {
                var gameInstance = AppStore.Instance.R1999Store.State.GetGameInstance(emulatorConnection.Id);

                if (gameInstance == null)
                {
                    return CoreAction.Empty;
                }

                if (gameInstance.JobReRollState.ReRollStatus == R1999ReRollStatus.SaveResultOk)
                {
                    var isClickSendCode = await SendCodeNewAccount(emulatorConnection);
                    if (isClickSendCode)
                    {
                        return R1999Action.SentCode.Create(baseActionPayload);
                    }

                    return CoreAction.Empty;
                }

                if (gameInstance.JobReRollState.ReRollStatus == R1999ReRollStatus.SentCode)
                {
                    var verficationCode = await GetVerificationCode(gameInstance.JobReRollState.Ordinal);
                    if (verficationCode != null)
                    {
                        await emulatorConnection.ClickPPointAsync(new PPoint(39.5f, 52.3f));
                        await Task.Delay(350);
                        await emulatorConnection.SendTextAsync(verficationCode);
                        await Task.Delay(150);
                        await emulatorConnection.ClickPPointAsync(new PPoint(49.9f, 60.6f));
                        await Task.Delay(150);
                        // confirm code;
                        await emulatorConnection.ClickPPointAsync(new PPoint(50.2f, 70.9f));
                        return CoreAction.Empty;
                    }

                    Logger.Info("!!!! Verification code not found");
                    await Task.Delay(5000);
                    return CoreAction.Empty;
                }

                isClicked = true;
                break;
            }

            case R1999TemplateKey.RegisterAccPassBtn:
            {
                var isSetPassword = await SetPassword(emulatorConnection);
                if (isSetPassword)
                {
                    await Task.Delay(5000);
                    var isStillRegister = await ScanTemplateAsync(
                        [R1999TemplateKey.RegisterAccPassBtn,],
                        emulatorConnection
                    );
                    if (isStillRegister.Length == 0)
                    {
                        return R1999Action.RegisteredAccount.Create(baseActionPayload);
                    }

                    return CoreAction.Empty;
                }

                return CoreAction.Empty;
            }

            case R1999TemplateKey.ProfileTextMotto:
            {
                await emulatorConnection.ClickPPointAsync(new PPoint(11.6f, 5.8f));

                isClicked = true;
                break;
            }

            case R1999TemplateKey.CharacterLevelText:
            {
                await emulatorConnection.ClickPPointAsync(new PPoint(10.9f, 5.7f));
                isClicked = true;
                break;
            }


            default:
            {
                if (_clickOnTemplateKeys.Contains(detectTemplatePoint.TemplateKey))
                {
                    Logger.Info(
                        $">>>> Click template {detectTemplatePoint.TemplateKey} on {detectTemplatePoint.Point}"
                    );
                    await emulatorConnection.ClickOnPointAsync(detectTemplatePoint.Point);
                    isClicked = true;
                }

                break;
            }
        }

        return isClicked ? R1999Action.ClickedAfterDetectedScreen.Create(baseActionPayload) : CoreAction.Empty;
    }

    protected override ScreenDetectorDataBase GetScreenDetectorDataHelper()
    {
        return R1999ScreenDetectorDataHelper.GetInstance();
    }

    protected override bool Filter(EventAction action)
    {
        if (action.Payload is BaseActionPayload baseActionPayload)
        {
            var gameInstance =
                AppStore.Instance.R1999Store.State.GetGameInstance(baseActionPayload.EmulatorId);
            if (gameInstance is { } gameInstanceData)
            {
                var currentStatus = gameInstanceData.JobReRollState.ReRollStatus;

                return currentStatus >= R1999ReRollStatus.SaveResultOk;
            }
        }

        return false;
    }

    private async Task<bool> SendCodeNewAccount(EmulatorConnection emulatorConnection)
    {
        //  click into email
        await emulatorConnection.ClickPPointAsync(new PPoint(47.3f, 40.2f));
        await Task.Delay(250);

        var gameInstance = AppStore.Instance.R1999Store.State.GetGameInstance(emulatorConnection.Id);

        if (gameInstance == null)
        {
            return false;
        }

        // nhap email
        await emulatorConnection.SendTextAsync(
            $"{R1999DataHelper.GetAccountEmail(gameInstance.JobReRollState.Ordinal)}"
        );
        await Task.Delay(550);
        // click send code;
        await emulatorConnection.ClickPPointAsync(new PPoint(60.4f, 51.7f));
        await Task.Delay(150);
        await emulatorConnection.ClickPPointAsync(new PPoint(60.4f, 51.7f));


        return true;
    }

    private async Task<bool> SetPassword(EmulatorConnection emulatorConnection)
    {
        Logger.Info(">>>>>>> SetPassword");
        await Task.Delay(1250);
        await emulatorConnection.ClickPPointAsync(new PPoint(39.5f, 38.8f));
        await Task.Delay(550);
        emulatorConnection.ClearInput();
        await Task.Delay(550);
        await emulatorConnection.SendTextAsync("123456aA");
        await Task.Delay(550);
        await emulatorConnection.ClickPPointAsync(new PPoint(39.5f, 54.9f));
        await Task.Delay(1250);
        emulatorConnection.ClearInput();
        await Task.Delay(550);
        await emulatorConnection.SendTextAsync("123456aA");
        await Task.Delay(1250);
        await emulatorConnection.ClickPPointAsync(new PPoint(39.5f, 62.0f));
        await Task.Delay(1250);
        //click register
        await emulatorConnection.ClickPPointAsync(new PPoint(50.4f, 70.4f));
        return true;
    }

    private async Task<string?> GetVerificationCode(string Ordinal)
    {
        var listemail = await GmailAPIHelper.GetEmailListAsync();
        if (listemail.Count > 0)
        {
            var getEmailTasks = listemail.Select(email => GmailAPIHelper.GetEmailByIdAsync(email.Id));
            var results = await Task.WhenAll(getEmailTasks);
            var verificationCode = results
                .Where(content =>
                    {
                        return content.To.Contains(R1999DataHelper.GetAccountEmail(Ordinal));
                    }
                )
                .Select(
                    result =>
                    {
                        var body = result.Body;
                        Logger.Info($"body: {body}");
                        // Regex để tìm mã xác minh
                        var pattern = @"verification code:\s*(\d+)";

                        var match = Regex.Match(body, pattern);

                        if (match.Success)
                        {
                            // delete this email
                            GmailAPIHelper.MoveToTrash(result.Id);
                            // GmailAPIHelper.DeleteEmailAsync(result.Id);
                            return match.Groups[1].Value;
                        }

                        return "";
                    }
                )
                .ToList()
                .FirstOrDefault();


            return verificationCode;
        }

        return null;
    }
}