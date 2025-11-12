// Sound effects
const sounds = {
    'piece-drop': new Audio('/sounds/piece-drop.mp3'),
    'win': new Audio('/sounds/win.mp3'),
    'draw': new Audio('/sounds/draw.mp3'),
    'error': new Audio('/sounds/error.mp3')
};

// Initialize all sounds
Object.values(sounds).forEach(sound => {
    sound.volume = 0.5; // Set default volume

    // Add error handler for each sound
    sound.onerror = () => {
        console.warn(`Sound file not found: ${sound.src}`);
        // Remove the errored sound from the sounds object
        Object.keys(sounds).forEach(key => {
            if (sounds[key] === sound) {
                delete sounds[key];
            }
        });
    };
});

// Function to play a sound
window.playSound = (soundName) => {
    const sound = sounds[soundName];
    if (sound) {
        sound.currentTime = 0; // Reset to start
        sound.play().catch(error => {
            console.warn('Failed to play sound:', error);
            // Remove the sound if it fails to play
            delete sounds[soundName];
        });
    }
}; 