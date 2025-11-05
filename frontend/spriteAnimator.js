class SpriteAnimator {
  constructor(spritesheetPath, frameWidth, frameHeight, cols, scale=1) {
    this.element = document.createElement('div');
    this.frameWidth = frameWidth;
    this.frameHeight = frameHeight;
    this.cols = cols;
    this.currentAnimation = null;
    this.scale = scale;
    
    // Apply scaling
    const scaledWidth = frameWidth * scale;
    const scaledHeight = frameHeight * scale;
    
    this.element.style.width = scaledWidth + 'px';
    this.element.style.height = scaledHeight + 'px';
    this.element.style.backgroundImage = `url("${spritesheetPath}")`;
    this.element.style.backgroundRepeat = 'no-repeat';
    this.element.style.backgroundSize = `${frameWidth * cols * scale}px auto`; // Scale the background

    this.element.style.position = 'absolute';
    this.element.style.top = '50%';
    this.element.style.left = '50%';
    this.element.style.transform = 'translate(-50%, -50%)';
    this.element.style.zIndex = '10';
    this.element.style.pointerEvents = 'none'; // Allow clicks to pass through to tile
  }
  
  playAnimation(row, duration = 1, loop = true) {
    const yPosition = -row * this.frameHeight * this.scale;
    const totalWidth = this.cols * this.frameWidth * this.scale;
    const keyframeName = `sprite-row-${row}-${Date.now()}`;
    
    const keyframes = `
      @keyframes ${keyframeName} {
        from { background-position: 0px ${yPosition}px; }
        to { background-position: -${totalWidth}px ${yPosition}px; }
      }
    `;
    
    // Remove old style if exists
    if (this.currentAnimation) {
      this.currentAnimation.remove();
    }
    
    // Add new keyframes
    const style = document.createElement('style');
    style.textContent = keyframes;
    document.head.appendChild(style);
    this.currentAnimation = style;
    
    // Apply animation
    const loopType = loop ? 'infinite' : '1';
    this.element.style.animation = `${keyframeName} ${duration}s steps(${this.cols}) ${loopType}`;
  }
  
  stopAnimation() {
    this.element.style.animation = 'none';
  }
}