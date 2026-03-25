// NativeTTS.mm — iOS AVSpeechSynthesizer bridge for Ascendant Continuum
// Unity calls these C symbols via [DllImport("__Internal")] from LanternRitual.cs.
// No extra Xcode entitlements are required; AVFoundation is a standard iOS framework.

#import <Foundation/Foundation.h>
#import <AVFoundation/AVFoundation.h>

// Keep a single synthesizer instance so rapid calls don't overlap.
static AVSpeechSynthesizer* _tts_synth = nil;

static AVSpeechSynthesizer* GetSynth()
{
    if (_tts_synth == nil)
        _tts_synth = [[AVSpeechSynthesizer alloc] init];
    return _tts_synth;
}

extern "C"
{
    // Speaks text out loud via AVSpeechSynthesizer.
    // rate:   0.0–1.0 (AVSpeechUtteranceDefaultSpeechRate ≈ 0.5). Pass -1 for default.
    // pitch:  0.5–2.0 (1.0 = normal). Pass -1 for default.
    // volume: 0.0–1.0. Pass -1 for default.
    void _NativeTTS_Speak(const char* text, float rate, float pitch, float volume)
    {
        if (text == nullptr) return;

        NSString* nsText = [NSString stringWithUTF8String:text];
        if (nsText.length == 0) return;

        AVSpeechSynthesizer* synth = GetSynth();

        // Cancel any in-progress speech so the new wish announcement is immediately heard.
        if (synth.isSpeaking)
            [synth stopSpeakingAtBoundary:AVSpeechBoundaryImmediate];

        AVSpeechUtterance* utterance = [AVSpeechUtterance speechUtteranceWithString:nsText];
        utterance.rate          = (rate   >= 0.f) ? rate   : AVSpeechUtteranceDefaultSpeechRate;
        utterance.pitchMultiplier = (pitch >= 0.f) ? pitch  : 1.0f;
        utterance.volume        = (volume >= 0.f) ? volume : 0.8f;

        // Use the device's default language; falls back gracefully on unsupported languages.
        utterance.voice = [AVSpeechSynthesisVoice voiceWithLanguage:
                           [[NSLocale currentLocale] languageCode]];

        [synth speakUtterance:utterance];
    }

    // Interrupts any in-progress speech immediately.
    void _NativeTTS_Stop()
    {
        AVSpeechSynthesizer* synth = GetSynth();
        if (synth.isSpeaking)
            [synth stopSpeakingAtBoundary:AVSpeechBoundaryImmediate];
    }

    // Returns 1 if TTS is currently speaking, 0 otherwise.
    int _NativeTTS_IsSpeaking()
    {
        return GetSynth().isSpeaking ? 1 : 0;
    }
}
