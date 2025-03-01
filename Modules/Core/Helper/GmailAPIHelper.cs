using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using NLog;

namespace NDBotUI.Modules.Core.Helper;

public class GmailAPIHelper
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static readonly string[] Scopes = [GmailService.Scope.GmailModify,];
    private static readonly string ApplicationName = "Gmail API C# Helper";
    private static readonly string CredentialPath = "credentials.json";
    private static readonly string TokenPath = "token.json";
    private GmailService? _service;

    public GmailAPIHelper()
    {
        Authenticate()
            .Wait(); // Gọi xác thực khi khởi tạo
    }

    /// <summary>
    ///     Xác thực OAuth 2.0
    /// </summary>
    private async Task Authenticate()
    {
        UserCredential credential;
        using (var stream = new FileStream(CredentialPath, FileMode.Open, FileAccess.Read))
        {
            credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.FromStream(stream)
                    .Secrets,
                Scopes,
                "user",
                CancellationToken.None,
                new FileDataStore(TokenPath, true)
            );
        }

        // Khởi tạo Gmail API Service
        _service = new GmailService(
            new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            }
        );
    }

    /// <summary>
    ///     Lấy danh sách email (mặc định lấy 10 email)
    /// </summary>
    public async Task<List<Message>> GetEmailListAsync(int maxResults = 10, string query = "")
    {
        if (_service == null)
        {
            return [];
        }

        var request = _service.Users.Messages.List("me");
        request.MaxResults = maxResults;
        request.Q = query; // Nếu muốn tìm kiếm email theo tiêu chí

        var response = await request.ExecuteAsync();
        return response.Messages?.ToList() ?? new List<Message>();
    }

    /// <summary>
    ///     Lấy nội dung email theo ID
    /// </summary>
    public async Task<EmailContent> GetEmailByIdAsync(string messageId)
    {
        var message = await _service!
            .Users
            .Messages
            .Get("me", messageId)
            .ExecuteAsync();
        return ParseEmailContent(message);
    }

    /// <summary>
    ///     Chuyển đổi Message thành EmailContent
    /// </summary>
    private EmailContent ParseEmailContent(Message message)
    {
        var subject = GetHeaderValue(message, "Subject");
        var from = GetHeaderValue(message, "From");
        var to = GetHeaderValue(message, "To");
        var body = ExtractEmailBody(message);

        return new EmailContent
        {
            Id = message.Id,
            From = from,
            To = to,
            Subject = subject,
            Body = body,
        };
    }

    private string ExtractEmailBody(Message message)
    {
        if (message.Payload?.Body?.Data != null)
        {
            // Trường hợp nội dung email nằm trực tiếp trong Payload.Body.Data
            return Base64UrlDecode(message.Payload.Body.Data);
        }

        if (message.Payload?.Parts != null)
        {
            // Duyệt qua tất cả các phần để tìm nội dung thực sự
            foreach (var part in message.Payload.Parts)
            {
                var body = GetBodyFromPart(part);
                if (!string.IsNullOrEmpty(body))
                {
                    return body;
                }
            }
        }

        return "No Content";
    }

    /// <summary>
    ///     Đệ quy tìm nội dung email trong tất cả các phần
    /// </summary>
    private string? GetBodyFromPart(MessagePart part)
    {
        if (part.Body?.Data != null)
        {
            return Base64UrlDecode(part.Body.Data);
        }

        if (part.Parts != null)
        {
            foreach (var subPart in part.Parts)
            {
                var body = GetBodyFromPart(subPart);
                if (!string.IsNullOrEmpty(body))
                {
                    return body;
                }
            }
        }

        return null;
    }

    /// <summary>
    ///     Giải mã Base64URL thành văn bản đọc được
    /// </summary>
    private string Base64UrlDecode(string input)
    {
        input = input
            .Replace("-", "+")
            .Replace("_", "/");
        switch (input.Length % 4)
        {
            case 2:
                input += "==";
                break;
            case 3:
                input += "=";
                break;
        }

        var bytes = Convert.FromBase64String(input);
        return Encoding.UTF8.GetString(bytes);
    }


    /// <summary>
    ///     Lấy giá trị header từ email
    /// </summary>
    private string GetHeaderValue(Message message, string headerName)
    {
        return message.Payload.Headers
                   .FirstOrDefault(h => h.Name.Equals(headerName, StringComparison.OrdinalIgnoreCase))
                   ?.Value
               ?? "Unknown";
    }


    public async Task MoveToTrash(string id)
    {
        try
        {
            if (_service == null)
            {
                throw new InvalidOperationException("Gmail service is not initialized.");
            }

            await _service
                .Users
                .Messages
                .Trash("me", id)
                .ExecuteAsync();
        }catch (Exception e)
        {
            Logger.Error(e);
        }
    }

    /// <summary>
    ///     Chuyển toàn bộ email trong inbox vào trash
    /// </summary>
    public async Task MoveAllEmailsToTrashAsync()
    {
        try
        {
            Logger.Info(">>MoveAllEmailsToTrashAsync");
            if (_service == null)
            {
                throw new InvalidOperationException("Gmail service is not initialized.");
            }

            var emailList = await GetEmailListAsync(10, "in:inbox");

            if (emailList.Count == 0)
            {
                Logger.Info("No emails to move to trash.");
                return;
            }

            foreach (var email in emailList)
            {
                await _service
                    .Users
                    .Messages
                    .Trash("me", email.Id)
                    .ExecuteAsync();
            }

            Logger.Info($"Moved {emailList.Count} emails to trash.");
        }
        catch (Exception e)
        {
            Logger.Error(e);
            throw;
        }
    }

    public async Task DeleteEmailAsync(string emailId)
    {
        try
        {
            Logger.Info(">>DeleteEmailAsync");
            if (_service == null)
            {
                throw new InvalidOperationException("Gmail service is not initialized.");
            }

            await _service
                .Users
                .Messages
                .Delete("me", emailId)
                .ExecuteAsync();
            Console.WriteLine($"Deleted email with ID: {emailId}");
        }
        catch (Exception e)
        {
            Logger.Error(e, "Error when deleting email");
        }
    }

    /// <summary>
    ///     Xóa vĩnh viễn tất cả email trong trash
    /// </summary>
    public async Task DeleteAllEmailsInTrashAsync()
    {
        Logger.Info(">>DeleteAllEmailsInTrashAsync");
        if (_service == null)
        {
            throw new InvalidOperationException("Gmail service is not initialized.");
        }

        var trashEmails = await GetEmailListAsync(100, "in:trash");

        if (trashEmails.Count == 0)
        {
            Console.WriteLine("No emails to delete permanently.");
            return;
        }

        foreach (var email in trashEmails)
        {
            await _service
                .Users
                .Messages
                .Delete("me", email.Id)
                .ExecuteAsync();
        }

        Console.WriteLine($"Permanently deleted {trashEmails.Count} emails from trash.");
    }
}

/// <summary>
///     Model chứa thông tin email
/// </summary>
public class EmailContent
{
    public required string Id { get; set; }
    public required string From { get; set; }
    public required string To { get; set; }
    public required string Subject { get; set; }
    public required string Body { get; set; }
}