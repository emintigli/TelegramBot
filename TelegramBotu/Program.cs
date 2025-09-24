using System.Text;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Exceptions;

class Program
{
    static async Task Main()
    {
        string token = "8452394167:AAHEaAATuzyoN1fu525N4U4v8h6S5uF9--g"; // BotFather’dan aldığın token
        var bot = new TelegramBotClient(token);

        using var cts = new CancellationTokenSource();

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = new Telegram.Bot.Types.Enums.UpdateType[]
    {
        Telegram.Bot.Types.Enums.UpdateType.Message
    }
        };

        bot.StartReceiving(
            updateHandler: async (client, update, token) =>
            {
                if (update.Message?.Text != null)
                {
                    await HandleMessage(client, update.Message, token);
                }
            },
            errorHandler: async (client, exception, token) =>
            {
                Console.WriteLine($"Hata: {exception.Message}");
                await Task.CompletedTask;
            },
            receiverOptions: receiverOptions,
            cancellationToken: cts.Token
        );

        Console.WriteLine("Bot çalışıyor...");
        Console.ReadLine();
        cts.Cancel();
    }

    static async Task HandleMessage(ITelegramBotClient botClient, Telegram.Bot.Types.Message message, CancellationToken cancellationToken)
    {
        string mesaj = message.Text;

        var satirlar = mesaj.Split('t', StringSplitOptions.RemoveEmptyEntries);
        var sb = new StringBuilder();
        sb.AppendLine("Boy,En,Adet,Uzun1,Uzun2,Kısa1,Kısa2");

        foreach (var satir in satirlar)
        {
            int boy = Yakala(satir, @"x\s*(\d+)");
            int en = Yakala(satir, @"y\s*(\d+)");
            int adet = Yakala(satir, @"z\s*(\d+)");
            int uzun = Yakala(satir, @"w\s*(\d+)");
            int kisa = Yakala(satir, @"v\s*(\d+)");

            int uzun1 = 0, uzun2 = 0;
            if (uzun == 2)
            {
                uzun1 = 1;
                uzun2 = 1;
            }
            else if (uzun == 1)
            {
                uzun1 = 1;
                uzun2 = 0;
            }

            int kisa1 = 0, kisa2 = 0;
            if (kisa == 2)
            {
                kisa1 = 1;
                kisa2 = 1;
            }
            else if (kisa == 1)
            {
                kisa1 = 1;
                kisa2 = 0;
            }

            sb.AppendLine($"{boy},{en},{adet},{uzun1},{uzun2},{kisa1},{kisa2}");
        }

        // Dosya yolu burada tanımlanmalı
        string filePath = Path.Combine(Path.GetTempPath(), "sonuc.csv");
        await File.WriteAllTextAsync(filePath, sb.ToString(), Encoding.UTF8);

        // Dosyayı aç ve InputFile oluştur
        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var inputFile = new InputFileStream(stream, "sonuc.csv");

        
        await botClient.SendDocument(
            chatId: message.Chat.Id,
            document: inputFile,
            caption: "CSV dosyan hazır 📄",
            cancellationToken: cancellationToken
        );
    }


    static int Yakala(string text, string pattern)
    {
        var m = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
        if (m.Success && int.TryParse(m.Groups[1].Value, out int value))
            return value;
        return 0;
    }
}
