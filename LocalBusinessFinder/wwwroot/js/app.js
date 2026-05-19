window.pindi = {
    togglePasswordVisibility: function (inputId, iconId) {
        const input = document.getElementById(inputId);
        const icon = document.getElementById(iconId);
        if (!input || !icon) return;

        if (input.type === 'password') {
            input.type = 'text';
            icon.classList.replace('bi-eye', 'bi-eye-slash');
            setTimeout(() => {
                input.type = 'password';
                icon.classList.replace('bi-eye-slash', 'bi-eye');
            }, 2000);
        } else {
            input.type = 'password';
            icon.classList.replace('bi-eye-slash', 'bi-eye');
        }
    },

    fadeOutSplash: function () {
        // Play splash audio if it exists
        try {
            const clickAudio = new Audio('/audio/PindiSong.m4a');
            clickAudio.play().catch(e => console.log('Audio playback skipped/failed:', e));
        } catch (e) { }

        const splash = document.getElementById('splash-screen');
        if (splash) {
            splash.style.opacity = '0';
            splash.style.transform = 'scale(1.1)';
            setTimeout(() => {
                splash.style.display = 'none';
            }, 600); // Wait for CSS transition
        }
    }
};
