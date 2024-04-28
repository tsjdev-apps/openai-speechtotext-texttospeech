using Azure;
using Azure.AI.OpenAI;
using SpeechToTextAndTextToSpeechApp.Utils;

// Create header
ConsoleHelper.CreateHeader();

// Get Host
string host =
    ConsoleHelper.SelectFromOptions(
        [Statics.OpenAIKey, Statics.AzureOpenAIKey]);

// OpenAI Client
OpenAIClient? client = null;
string ttsDeploymentName = "tts-1";
string sttDeploymentName = "whisper-1";

switch (host)
{
    case Statics.OpenAIKey:

        // Get OpenAI Key
        string openAIKey =
            ConsoleHelper.GetString(
                $"Please insert your [yellow]{Statics.OpenAIKey}[/] API key:");

        // Create OpenAI client
        client = new(openAIKey);

        break;

    case Statics.AzureOpenAIKey:

        // Get Endpoint
        string endpoint =
            ConsoleHelper.GetUrl(
                "Please insert your [yellow]Azure OpenAI endpoint[/]:");

        // Get Azure OpenAI Key
        string azureOpenAIKey =
            ConsoleHelper.GetString(
                $"Please insert your [yellow]{Statics.AzureOpenAIKey}[/] API key:");

        // Get TTS Deployment name
        ttsDeploymentName =
            ConsoleHelper.GetString(
                $"Please insert the name of your [yellow]TTS model[/]:");

        // Get STT Deployment name
        sttDeploymentName =
            ConsoleHelper.GetString(
                $"Please insert the name of your [yellow]STT model[/]:");

        // Create OpenAI client
        client =
            new(new Uri(endpoint), new AzureKeyCredential(azureOpenAIKey));

        break;
}

if (client == null)
{
    return;
}

while (true)
{

    // Get Mode
    string mode =
        ConsoleHelper.SelectFromOptions(
            [Statics.STTKey, Statics.TTSKey]);

    switch (mode)
    {
        case Statics.STTKey:
            string? audioInputFilePath =
                ConsoleHelper.GetString(
                    "Please insert the [yellow]full path of your audio file[/]:");

            byte[] audioInputBytes = await File.ReadAllBytesAsync(audioInputFilePath);
            BinaryData audioInputBinaryData = BinaryData.FromBytes(audioInputBytes);

            Response<AudioTranscription>? sttResult =
                await client.GetAudioTranscriptionAsync(
                    new AudioTranscriptionOptions(sttDeploymentName, audioInputBinaryData));

            // Write Output
            ConsoleHelper.WriteString(
                $"Your file was processed. Here is the content: {sttResult.Value.Text}" +
                $"{Environment.NewLine}Press any key to start again.");

            Console.ReadKey();

            break;

        case Statics.TTSKey:
            // Get Input
            string input =
                ConsoleHelper.GetString(
                    "Please insert your [yellow]text[/]:");

            // Get Voice
            string voice =
                ConsoleHelper.SelectFromOptions(
                    [Statics.AlloyVoice, Statics.EchoVoice, Statics.FableVoice, 
                    Statics.OnyxVoice, Statics.NovaVoice, Statics.ShimmerVoice]);

            // Get Output Folder
            string outputFolder =
                ConsoleHelper.GetString(
                    "Please insert the [yellow]output folder[/]:");

            // Create Folder if needed
            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }

            // Show Header
            ConsoleHelper.CreateHeader();

            // Make Request
            Response<BinaryData> ttsResult =
                await client.GenerateSpeechFromTextAsync(
                    new SpeechGenerationOptions(ttsDeploymentName, input, new SpeechVoice(voice)));

            // Write Data
            string filePath = $"{outputFolder}/{Guid.NewGuid()}.mp3";
            File.WriteAllBytes(filePath, ttsResult.Value.ToArray());

            // Write Output
            ConsoleHelper.WriteString($"Your file was created. You will find it here: {filePath}{Environment.NewLine}Press any key to start again.");

            Console.ReadKey();

            break;
    }
}