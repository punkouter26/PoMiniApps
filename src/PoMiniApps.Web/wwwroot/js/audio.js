window.audioInterop = (() => {
    let audioElement = null;
    let currentObjectUrl = null;
    let audioContext = null;
    let currentSource = null;
    let isUnlocked = false;

    function getAudioElement() {
        if (!audioElement) {
            audioElement = new Audio();
            audioElement.preload = 'auto';
            audioElement.volume = 1.0;
        }

        return audioElement;
    }

    function releaseCurrentUrl() {
        if (currentObjectUrl) {
            URL.revokeObjectURL(currentObjectUrl);
            currentObjectUrl = null;
        }
    }

    function getAudioContext() {
        if (!audioContext) {
            const AudioContextCtor = window.AudioContext || window.webkitAudioContext;
            if (!AudioContextCtor) {
                return null;
            }

            audioContext = new AudioContextCtor();
        }

        return audioContext;
    }

    function stopCurrentSource() {
        if (currentSource) {
            currentSource.onended = null;
            try {
                currentSource.stop();
            } catch {
                // Ignore stop errors for already completed sources.
            }
            try {
                currentSource.disconnect();
            } catch {
                // Ignore disconnect errors.
            }
            currentSource = null;
        }
    }

    async function unlockAudio() {
        const context = getAudioContext();

        if (isUnlocked) {
            return true;
        }

        try {
            if (context) {
                if (context.state !== 'running') {
                    await context.resume();
                }

                const buffer = context.createBuffer(1, 1, 22050);
                const source = context.createBufferSource();
                source.buffer = buffer;
                source.connect(context.destination);
                source.start(0);
                source.disconnect();

                isUnlocked = context.state === 'running';
                if (isUnlocked) {
                    return true;
                }
            }

            const audio = getAudioElement();
            audio.muted = true;
            audio.src = 'data:audio/wav;base64,UklGRiQAAABXQVZFZm10IBAAAAABAAEAQB8AAEAfAAABAAgAZGF0YQAAAAA=';
            await audio.play();
            audio.pause();
            audio.currentTime = 0;
            audio.removeAttribute('src');
            audio.load();
            audio.muted = false;
            isUnlocked = true;
            return true;
        } catch (err) {
            console.warn('Audio unlock failed:', err?.message ?? err);
            const audio = getAudioElement();
            audio.muted = false;
            return false;
        }
    }

    async function playAudioFromBase64(base64Data, format) {
        try {
            if (!isUnlocked) {
                const unlocked = await unlockAudio();
                if (!unlocked) {
                    return false;
                }
            }

            const audioData = atob(base64Data);
            const bytes = new Uint8Array(audioData.length);

            for (let i = 0; i < audioData.length; i++) {
                bytes[i] = audioData.charCodeAt(i);
            }

            const context = getAudioContext();
            if (context) {
                const audioBuffer = await context.decodeAudioData(bytes.buffer.slice(0));

                stopCurrentSource();

                return await new Promise((resolve) => {
                    const source = context.createBufferSource();
                    source.buffer = audioBuffer;
                    source.connect(context.destination);
                    source.onended = () => {
                        if (currentSource === source) {
                            currentSource = null;
                        }
                        try {
                            source.disconnect();
                        } catch {
                            // Ignore disconnect errors.
                        }
                        resolve(true);
                    };

                    currentSource = source;
                    source.start(0);
                });
            }

            const mimeType = format === 'mp3' ? 'audio/mpeg' : 'audio/wav';
            const blob = new Blob([bytes], { type: mimeType });
            const audio = getAudioElement();

            releaseCurrentUrl();
            currentObjectUrl = URL.createObjectURL(blob);

            audio.pause();
            audio.currentTime = 0;
            audio.src = currentObjectUrl;

            return await new Promise((resolve) => {
                audio.onended = () => {
                    releaseCurrentUrl();
                    resolve(true);
                };

                audio.onerror = (event) => {
                    console.warn('Audio playback error:', event);
                    releaseCurrentUrl();
                    resolve(false);
                };

                const playPromise = audio.play();
                if (playPromise !== undefined) {
                    playPromise.catch((err) => {
                        console.warn('Audio playback blocked:', err?.message ?? err);
                        releaseCurrentUrl();
                        resolve(false);
                    });
                }
            });
        } catch (err) {
            console.error('Error decoding audio:', err);
            return false;
        }
    }

    function stopAudio() {
        stopCurrentSource();
        const audio = getAudioElement();
        audio.pause();
        audio.currentTime = 0;
        releaseCurrentUrl();
    }

    function audioUnlocked() {
        return isUnlocked;
    }

    return {
        unlockAudio,
        playAudioFromBase64,
        stopAudio,
        audioUnlocked
    };
})();
