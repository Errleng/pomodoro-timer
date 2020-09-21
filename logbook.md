# Logbook
## 2020-07-29
* I want to add text-to-speech (TTS) to say the name of the last played song
* Learned about `System.Speech.Synthesis`, and realized it was for the .NET Framework
  * .NET Core does not have this API
* .NET Core seems to recommend using the Azure Cognitive Services speech synthesis, `Microsoft.CognitiveServices.Speech`
  * I need a subscription to access this service, so I'll put this task off for later