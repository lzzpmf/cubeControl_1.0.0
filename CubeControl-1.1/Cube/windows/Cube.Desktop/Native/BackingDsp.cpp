#include "signalsmith-stretch.h"
#include <cmath>
#include <vector>

struct BackingDsp {
    signalsmith::stretch::SignalsmithStretch<float> stretch;
    int channels;
    std::vector<std::vector<float>> input, output;
    BackingDsp(int channels, int rate) : channels(channels), input(channels), output(channels) {
        stretch.presetDefault(channels, rate);
    }
    void unpack(const float *data, int frames) {
        for (int c = 0; c < channels; ++c) {
            input[c].resize(frames);
            for (int f = 0; f < frames; ++f) input[c][f] = data[f*channels + c];
        }
    }
};

#define API extern "C" __declspec(dllexport)
API BackingDsp * backing_create(int channels, int rate) noexcept {
    try { return channels > 0 && channels <= 32 && rate >= 8000 && rate <= 384000 ? new BackingDsp(channels, rate) : nullptr; }
    catch (...) { return nullptr; }
}
API BackingDsp * backing_create_offline(int channels, int rate) noexcept {
    auto *p = backing_create(channels, rate);
    if (!p) return nullptr;
    try {
        // Exports can use a longer analysis window without adding playback latency.
        p->stretch.configure(channels, static_cast<int>(rate * 0.24), static_cast<int>(rate * 0.03));
        return p;
    } catch (...) { delete p; return nullptr; }
}
API void backing_destroy(BackingDsp *p) noexcept { delete p; }
API int backing_prepare(BackingDsp *p, double speed, double semitones) noexcept {
    try {
        if (!p || !std::isfinite(speed) || speed < 0.32 || speed > 3.07 || !std::isfinite(semitones) || std::abs(semitones) > 12) return -1;
        p->stretch.reset(); p->stretch.setTransposeSemitones(semitones);
        return p->stretch.outputSeekLength(speed);
    } catch (...) { return -1; }
}
API int backing_seek(BackingDsp *p, const float *input, int frames) noexcept {
    try { if (!p || !input || frames < 0) return -1; p->unpack(input, frames); p->stretch.outputSeek(p->input, frames); return 0; }
    catch (...) { return -1; }
}
API int backing_process(BackingDsp *p, const float *input, int inFrames, float *output, int outFrames) noexcept {
    try {
        if (!p || !input || !output || inFrames < 0 || outFrames < 0) return -1;
        p->unpack(input, inFrames);
        for (auto &channel : p->output) channel.resize(outFrames);
        p->stretch.process(p->input, inFrames, p->output, outFrames);
        for (int f = 0; f < outFrames; ++f) for (int c = 0; c < p->channels; ++c) output[f*p->channels+c] = p->output[c][f];
        return 0;
    } catch (...) { return -1; }
}
API int backing_flush(BackingDsp *p, float *output, int frames) noexcept {
    try {
        if (!p || !output || frames < 0) return -1;
        for (auto &channel : p->output) channel.resize(frames);
        p->stretch.flush(p->output, frames);
        for (int f = 0; f < frames; ++f) for (int c = 0; c < p->channels; ++c) output[f*p->channels+c] = p->output[c][f];
        return 0;
    } catch (...) { return -1; }
}
