/**
 * FAME LMS - Secure Dynamic Watermark Module
 * Version: 2.0.0
 * Copyright 2026 First Aid Made Easy
 * 
 * This module provides tamper-resistant dynamic watermarking with:
 * - Canvas-based rendering (cannot be removed via CSS)
 * - MutationObserver for DOM tampering detection
 * - DevTools detection
 * - Screen recording behavior detection
 * - Self-healing watermark that regenerates if removed
 * - Randomized positioning including center
 * - Smooth fade animations for non-intrusive UX
 */

(function(window, document) {
    'use strict';

    // Obfuscated configuration
    var _0x = {
        interval: 12000,      // 12 seconds between watermarks
        duration: 4000,       // 4 seconds visible
        fadeTime: 1000,       // 1 second fade
        blurThreshold: 10,    // Max blur events per minute
        checkInterval: 500,   // Security check interval
        devToolsThreshold: 160 // Height difference threshold for devtools
    };

    // Detect mobile device
    var isMobile = /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent) ||
                   (window.innerWidth <= 768) ||
                   ('ontouchstart' in window) ||
                   (navigator.maxTouchPoints > 0);

    // State management
    var state = {
        canvas: null,
        ctx: null,
        container: null,
        watermarkData: null,
        isVisible: false,
        blurCount: 0,
        lastBlurReset: Date.now(),
        violationCount: 0,
        sessionId: null,
        videoId: null,
        videoName: null,
        isInitialized: false,
        animationFrame: null,
        checkTimer: null,
        watermarkTimer: null,
        isPaused: false,
        isPseudoFullscreen: false
    };

    // Generate unique session ID
    function generateSessionId() {
        return 'sess_' + Date.now().toString(36) + Math.random().toString(36).substr(2, 9);
    }

    // Initialize the secure watermark system
    function init(options) {
        if (state.isInitialized) return;
        
        options = options || {};
        state.sessionId = generateSessionId();
        state.videoId = options.videoId || null;
        state.videoName = options.videoName || '';

        // Inject fullscreen CSS
        injectFullscreenCSS();

        // Create canvas overlay
        createCanvasOverlay();

        // Add custom fullscreen button
        addFullscreenButton();
        
        // Load watermark data
        loadWatermarkData();
        
        // Start security monitors
        startSecurityMonitors();
        
        // Start watermark display cycle
        startWatermarkCycle();
        
        // Start integrity checks
        startIntegrityChecks();
        
        state.isInitialized = true;
        
        console.log('%c[FAME Security] Watermark system initialized', 'color: green; font-weight: bold;');
    }

    // Create canvas overlay for watermark
    function createCanvasOverlay() {
        // Find video container
        var containers = ['#vid-cont', '.player__embed', '.video-container', '.embed-responsive', '.fame-card.position-relative', '.ratio.ratio-16x9'];
        var container = null;
        
        for (var i = 0; i < containers.length; i++) {
            container = document.querySelector(containers[i]);
            if (container) break;
        }
        
        if (!container) {
            container = document.body;
        }
        
        state.container = container;
        
        // Create canvas
        state.canvas = document.createElement('canvas');
        state.canvas.id = 'fame-wm-canvas';
        state.canvas.style.cssText = 
            'position: absolute !important;' +
            'top: 0 !important;' +
            'left: 0 !important;' +
            'width: 100% !important;' +
            'height: 100% !important;' +
            'pointer-events: none !important;' +
            'z-index: 2147483647 !important;' +
            'opacity: 1 !important;' +
            'display: block !important;' +
            'visibility: visible !important;';
        
        // Ensure container has relative positioning
        var containerStyle = window.getComputedStyle(container);
        if (containerStyle.position === 'static') {
            container.style.position = 'relative';
        }
        
        container.appendChild(state.canvas);
        state.ctx = state.canvas.getContext('2d');
        
        // Set canvas size
        resizeCanvas();
        window.addEventListener('resize', resizeCanvas);
        
        // Protect canvas from removal
        protectCanvas();
    }

    // Resize canvas to match container (handles fullscreen + pseudo-fullscreen)
    function resizeCanvas() {
        if (!state.canvas || !state.container) return;
        
        var fsElement = document.fullscreenElement || document.webkitFullscreenElement || document.mozFullScreenElement || document.msFullscreenElement;
        
        if (state.isPseudoFullscreen || (fsElement && (fsElement === state.container || state.container.contains(state.canvas)))) {
            // In fullscreen or pseudo-fullscreen - use viewport dimensions
            state.canvas.width = window.innerWidth || screen.width;
            state.canvas.height = window.innerHeight || screen.height;
        } else {
            var rect = state.container.getBoundingClientRect();
            state.canvas.width = rect.width;
            state.canvas.height = rect.height;
        }
    }

    // Protect canvas from tampering
    function protectCanvas() {
        // MutationObserver to detect removal/modification
        var observer = new MutationObserver(function(mutations) {
            mutations.forEach(function(mutation) {
                // Check if canvas was removed
                if (mutation.type === 'childList') {
                    mutation.removedNodes.forEach(function(node) {
                        if (node === state.canvas || (node.id && node.id === 'fame-wm-canvas')) {
                            logViolation('WATERMARK_TAMPER', 'Canvas element removed');
                            recreateCanvas();
                        }
                    });
                }
                
                // Check if canvas style was modified
                if (mutation.type === 'attributes' && mutation.target === state.canvas) {
                    if (mutation.attributeName === 'style') {
                        logViolation('WATERMARK_TAMPER', 'Canvas style modified');
                        resetCanvasStyle();
                    }
                }
            });
        });
        
        observer.observe(state.container, {
            childList: true,
            subtree: true,
            attributes: true,
            attributeFilter: ['style', 'class', 'hidden']
        });
        
        // Also observe body for container removal
        var bodyObserver = new MutationObserver(function(mutations) {
            if (!document.contains(state.canvas)) {
                logViolation('WATERMARK_TAMPER', 'Canvas removed from DOM');
                setTimeout(recreateCanvas, 100);
            }
        });
        
        bodyObserver.observe(document.body, { childList: true, subtree: true });
    }

    // Recreate canvas if removed
    function recreateCanvas() {
        if (document.getElementById('fame-wm-canvas')) {
            document.getElementById('fame-wm-canvas').remove();
        }
        createCanvasOverlay();
        if (state.watermarkData) {
            showWatermark();
        }
    }

    // Reset canvas style if tampered
    function resetCanvasStyle() {
        if (state.canvas) {
            state.canvas.style.cssText = 
                'position: absolute !important;' +
                'top: 0 !important;' +
                'left: 0 !important;' +
                'width: 100% !important;' +
                'height: 100% !important;' +
                'pointer-events: none !important;' +
                'z-index: 2147483647 !important;' +
                'opacity: 1 !important;' +
                'display: block !important;' +
                'visibility: visible !important;';
        }
    }

    // Load watermark data from server
    function loadWatermarkData() {
        var xhr = new XMLHttpRequest();
        xhr.open('GET', '/Student/LoadWaterMarkSecure', true);
        xhr.onreadystatechange = function() {
            if (xhr.readyState === 4 && xhr.status === 200) {
                try {
                    state.watermarkData = JSON.parse(xhr.responseText);
                } catch (e) {
                    console.error('[FAME Security] Failed to parse watermark data');
                }
            }
        };
        xhr.send();
    }

    // Start watermark display cycle
    function startWatermarkCycle() {
        // Initial display after 3 seconds
        setTimeout(function() {
            showWatermark();
        }, 3000);
        
        // Recurring display
        state.watermarkTimer = setInterval(function() {
            if (!state.isPaused) {
                loadWatermarkData(); // Refresh data
                showWatermark();
            }
        }, _0x.interval);
    }

    // Show watermark with animation
    function showWatermark() {
        if (!state.watermarkData || !state.ctx || !state.canvas) return;
        
        resizeCanvas();
        
        var positions = getRandomPosition();
        var opacity = 0;
        var startTime = Date.now();
        
        function animate() {
            var elapsed = Date.now() - startTime;
            
            // Fade in phase
            if (elapsed < _0x.fadeTime) {
                opacity = elapsed / _0x.fadeTime * 0.5; // Max 50% opacity
            }
            // Visible phase
            else if (elapsed < _0x.duration - _0x.fadeTime) {
                opacity = 0.5;
            }
            // Fade out phase
            else if (elapsed < _0x.duration) {
                opacity = 0.5 * (1 - (elapsed - (_0x.duration - _0x.fadeTime)) / _0x.fadeTime);
            }
            // End
            else {
                clearCanvas();
                state.isVisible = false;
                return;
            }
            
            renderWatermark(positions.x, positions.y, opacity);
            state.animationFrame = requestAnimationFrame(animate);
        }
        
        state.isVisible = true;
        animate();
    }

    // Get random position (including center)
    function getRandomPosition() {
        var canvas = state.canvas;
        var padding = 50;
        
        // Define zones including center
        var zones = [
            { x: padding, y: padding }, // Top-left
            { x: canvas.width / 2, y: padding }, // Top-center
            { x: canvas.width - padding - 200, y: padding }, // Top-right
            { x: padding, y: canvas.height / 2 - 30 }, // Middle-left
            { x: canvas.width / 2 - 100, y: canvas.height / 2 - 30 }, // CENTER
            { x: canvas.width - padding - 200, y: canvas.height / 2 - 30 }, // Middle-right
            { x: padding, y: canvas.height - padding - 60 }, // Bottom-left
            { x: canvas.width / 2, y: canvas.height - padding - 60 }, // Bottom-center
            { x: canvas.width - padding - 200, y: canvas.height - padding - 60 } // Bottom-right
        ];
        
        // Add some randomness to position
        var zone = zones[Math.floor(Math.random() * zones.length)];
        var jitterX = (Math.random() - 0.5) * 40;
        var jitterY = (Math.random() - 0.5) * 20;
        
        return {
            x: Math.max(10, Math.min(canvas.width - 210, zone.x + jitterX)),
            y: Math.max(20, Math.min(canvas.height - 70, zone.y + jitterY))
        };
    }

    // Render watermark on canvas
    function renderWatermark(x, y, opacity) {
        var ctx = state.ctx;
        var data = state.watermarkData;
        
        if (!ctx || !data) return;
        
        clearCanvas();
        
        ctx.save();
        ctx.globalAlpha = opacity;
        
        // No shadow - clean crisp text
        ctx.shadowColor = 'transparent';
        ctx.shadowBlur = 0;
        ctx.shadowOffsetX = 0;
        ctx.shadowOffsetY = 0;
        
        // Red text, thin font
        ctx.fillStyle = 'rgba(255, 0, 0, 0.95)';
        ctx.font = '12px Arial, sans-serif';
        ctx.textBaseline = 'top';
        
        // Render user info - Name, Phone, CNIC only (vertical alignment, minimal spacing)
        var lineHeight = 15;
        var lines = [];
        
        if (data.Name) lines.push(data.Name);
        if (data.PhoneNo) lines.push(data.PhoneNo);
        if (data.CNIC) lines.push(data.CNIC);
        
        // Draw each line (top to bottom, minimal spacing)
        lines.forEach(function(line, index) {
            ctx.fillText(line, x, y + (index * lineHeight));
        });
        
        ctx.restore();
    }

    // Clear canvas
    function clearCanvas() {
        if (state.ctx && state.canvas) {
            state.ctx.clearRect(0, 0, state.canvas.width, state.canvas.height);
        }
    }

    // Start security monitors
    function startSecurityMonitors() {
        // DevTools detection
        detectDevTools();
        
        // Tab visibility
        document.addEventListener('visibilitychange', handleVisibilityChange);
        
        // Window blur/focus
        window.addEventListener('blur', handleWindowBlur);
        window.addEventListener('focus', handleWindowFocus);
        
        // Right-click prevention
        document.addEventListener('contextmenu', handleContextMenu);
        
        // Keyboard shortcuts
        document.addEventListener('keydown', handleKeyDown);
        
        // Picture-in-Picture
        if (document.pictureInPictureEnabled) {
            document.addEventListener('enterpictureinpicture', handlePiP);
        }
        
        // Fullscreen interception - ensure watermark stays visible
        document.addEventListener('fullscreenchange', handleFullscreenChange);
        document.addEventListener('webkitfullscreenchange', handleFullscreenChange);
        document.addEventListener('mozfullscreenchange', handleFullscreenChange);
        document.addEventListener('MSFullscreenChange', handleFullscreenChange);
        
        // Console detection
        detectConsoleAccess();
        
        // Screen capture API detection
        detectScreenCapture();
    }

    // Handle fullscreen changes to ensure watermark stays visible in fullscreen
    function handleFullscreenChange() {
        var fsElement = document.fullscreenElement || document.webkitFullscreenElement || document.mozFullScreenElement || document.msFullscreenElement;
        
        if (fsElement) {
            // Something went fullscreen
            var videoIframe = document.querySelector('#videoPlayer, #Iframe, iframe');
            var vidCont = document.querySelector('#vid-cont');
            
            // If the iframe itself went fullscreen (not our container)
            if (videoIframe && fsElement === videoIframe && vidCont) {
                // On DESKTOP ONLY: redirect fullscreen to container so watermark shows
                // On MOBILE: don't intercept — requestFullscreen from a non-gesture context
                // will fail and cause a flash-loop. Let the iframe fullscreen naturally.
                if (!isMobile) {
                    var exitFn = document.exitFullscreen || document.webkitExitFullscreen || document.mozCancelFullScreen || document.msExitFullscreen;
                    if (exitFn) {
                        var exitPromise = exitFn.call(document);
                        if (exitPromise && exitPromise.then) {
                            exitPromise.then(function() {
                                requestContainerFullscreen(vidCont);
                            }).catch(function(e) {
                                console.warn('[FAME Security] Could not redirect fullscreen:', e);
                            });
                        } else {
                            setTimeout(function() {
                                requestContainerFullscreen(vidCont);
                            }, 100);
                        }
                    }
                }
                return;
            }
            
            // Container went fullscreen via our button - resize canvas and show watermark
            setTimeout(function() {
                resizeCanvas();
                if (state.watermarkData) {
                    showWatermark();
                }
                updateFullscreenButton(true);
            }, 200);
        } else {
            // Exited fullscreen
            // If pseudo-fullscreen is still active, this was ESC pressed during pseudo —
            // exit pseudo too
            if (state.isPseudoFullscreen) {
                exitPseudoFullscreen();
            }
            
            setTimeout(function() {
                var vidCont = document.querySelector('#vid-cont');
                if (vidCont && state.canvas && state.canvas.parentElement !== vidCont) {
                    vidCont.appendChild(state.canvas);
                    state.container = vidCont;
                }
                resizeCanvas();
                updateFullscreenButton(false);
            }, 200);
        }
    }

    // Check if native Fullscreen API is supported for non-video elements
    function isFullscreenApiSupported() {
        var el = document.createElement('div');
        return !!(el.requestFullscreen || el.webkitRequestFullscreen || el.mozRequestFullScreen || el.msRequestFullscreen);
    }

    // Request fullscreen on the container element
    function requestContainerFullscreen(el) {
        var reqFn = el.requestFullscreen || el.webkitRequestFullscreen || el.mozRequestFullScreen || el.msRequestFullscreen;
        if (reqFn) {
            try {
                var result = reqFn.call(el);
                // If it returns a promise, catch potential rejection (e.g. iOS)
                if (result && result.catch) {
                    result.catch(function(e) {
                        console.warn('[FAME Security] Native fullscreen rejected, using pseudo-fullscreen:', e);
                        enterPseudoFullscreen();
                    });
                }
            } catch (e) {
                console.warn('[FAME Security] requestFullscreen threw, using pseudo-fullscreen:', e);
                enterPseudoFullscreen();
            }
        } else {
            // No native fullscreen support (e.g. iOS Safari for divs) — use pseudo
            enterPseudoFullscreen();
        }
    }

    // Enter CSS-based pseudo-fullscreen (fallback for iOS and unsupported browsers)
    function enterPseudoFullscreen() {
        var vidCont = document.querySelector('#vid-cont');
        if (!vidCont) return;
        
        state.isPseudoFullscreen = true;
        vidCont.classList.add('fame-pseudo-fullscreen');
        document.body.classList.add('fame-body-fullscreen');
        
        // Lock scroll
        document.body.style.overflow = 'hidden';
        document.documentElement.style.overflow = 'hidden';
        
        // Try to lock to landscape on mobile for better viewing
        try {
            if (screen.orientation && screen.orientation.lock && isMobile) {
                screen.orientation.lock('landscape').catch(function() { /* not supported */ });
            }
        } catch (e) { /* ignore */ }
        
        // Add ESC key listener for pseudo-fullscreen exit
        document.addEventListener('keydown', handlePseudoEsc);
        
        setTimeout(function() {
            resizeCanvas();
            if (state.watermarkData) {
                showWatermark();
            }
            updateFullscreenButton(true);
        }, 100);
    }

    // Exit CSS-based pseudo-fullscreen
    function exitPseudoFullscreen() {
        var vidCont = document.querySelector('#vid-cont');
        if (!vidCont) return;
        
        state.isPseudoFullscreen = false;
        vidCont.classList.remove('fame-pseudo-fullscreen');
        document.body.classList.remove('fame-body-fullscreen');
        
        // Restore scroll
        document.body.style.overflow = '';
        document.documentElement.style.overflow = '';
        
        // Unlock orientation
        try {
            if (screen.orientation && screen.orientation.unlock) {
                screen.orientation.unlock();
            }
        } catch (e) { /* ignore */ }
        
        document.removeEventListener('keydown', handlePseudoEsc);
        
        setTimeout(function() {
            resizeCanvas();
            updateFullscreenButton(false);
        }, 100);
    }

    // ESC handler for pseudo-fullscreen
    function handlePseudoEsc(e) {
        if (e.key === 'Escape' && state.isPseudoFullscreen) {
            e.preventDefault();
            exitPseudoFullscreen();
        }
    }

    // Toggle fullscreen on the video container
    function toggleContainerFullscreen() {
        var vidCont = document.querySelector('#vid-cont');
        if (!vidCont) return;
        
        var fsElement = document.fullscreenElement || document.webkitFullscreenElement || document.mozFullScreenElement || document.msFullscreenElement;
        
        // Currently in pseudo-fullscreen — exit it
        if (state.isPseudoFullscreen) {
            exitPseudoFullscreen();
            return;
        }
        
        // Currently in native fullscreen — exit it
        if (fsElement) {
            var exitFn = document.exitFullscreen || document.webkitExitFullscreen || document.mozCancelFullScreen || document.msExitFullscreen;
            if (exitFn) exitFn.call(document);
            return;
        }
        
        // Not fullscreen — enter fullscreen
        // Try native API first; requestContainerFullscreen will fall back to pseudo if it fails
        requestContainerFullscreen(vidCont);
    }

    // Update fullscreen button icon
    function updateFullscreenButton(isFullscreen) {
        var btn = document.getElementById('fame-fullscreen-btn');
        if (btn) {
            btn.innerHTML = isFullscreen 
                ? '<i class="bi bi-fullscreen-exit"></i>' 
                : '<i class="bi bi-fullscreen"></i>';
            btn.title = isFullscreen ? 'Exit Fullscreen' : 'Fullscreen';
        }
    }

    // Inject CSS for fullscreen watermark display
    function injectFullscreenCSS() {
        if (document.getElementById('fame-wm-fullscreen-css')) return;
        
        var style = document.createElement('style');
        style.id = 'fame-wm-fullscreen-css';
        style.textContent = 
            /* ===== NATIVE FULLSCREEN (desktop + Android Chrome) ===== */
            '#vid-cont:fullscreen, #vid-cont:-webkit-full-screen {' +
            '  background: #000 !important;' +
            '  width: 100vw !important;' +
            '  height: 100vh !important;' +
            '  border-radius: 0 !important;' +
            '  overflow: visible !important;' +
            '  display: flex !important;' +
            '  align-items: center !important;' +
            '  justify-content: center !important;' +
            '}' +
            '#vid-cont:fullscreen .ratio, #vid-cont:-webkit-full-screen .ratio {' +
            '  width: 100vw !important;' +
            '  height: 100vh !important;' +
            '  max-width: 100vw !important;' +
            '  max-height: 100vh !important;' +
            '  padding-top: 0 !important;' +
            '}' +
            '#vid-cont:fullscreen .ratio::before, #vid-cont:-webkit-full-screen .ratio::before {' +
            '  display: none !important;' +
            '}' +
            '#vid-cont:fullscreen .ratio iframe, #vid-cont:-webkit-full-screen .ratio iframe,' +
            '#vid-cont:fullscreen .ratio video, #vid-cont:-webkit-full-screen .ratio video {' +
            '  position: absolute !important;' +
            '  top: 0 !important;' +
            '  left: 0 !important;' +
            '  width: 100% !important;' +
            '  height: 100% !important;' +
            '}' +
            '#vid-cont:fullscreen #fame-wm-canvas, #vid-cont:-webkit-full-screen #fame-wm-canvas {' +
            '  position: fixed !important;' +
            '  top: 0 !important;' +
            '  left: 0 !important;' +
            '  width: 100vw !important;' +
            '  height: 100vh !important;' +
            '  z-index: 2147483647 !important;' +
            '  pointer-events: none !important;' +
            '}' +
            '#vid-cont:fullscreen #fame-fullscreen-btn, #vid-cont:-webkit-full-screen #fame-fullscreen-btn {' +
            '  position: fixed !important;' +
            '  top: 16px !important;' +
            '  right: 16px !important;' +
            '  bottom: auto !important;' +
            '  z-index: 2147483646 !important;' +
            '  opacity: 0.7 !important;' +
            '}' +
            '#vid-cont:fullscreen #videoLoader, #vid-cont:-webkit-full-screen #videoLoader {' +
            '  z-index: 2147483640 !important;' +
            '}' +
            
            /* ===== PSEUDO-FULLSCREEN (iOS Safari fallback + any failed native) ===== */
            '#vid-cont.fame-pseudo-fullscreen {' +
            '  position: fixed !important;' +
            '  top: 0 !important;' +
            '  left: 0 !important;' +
            '  width: 100vw !important;' +
            '  height: 100vh !important;' +
            '  z-index: 999999 !important;' +
            '  background: #000 !important;' +
            '  border-radius: 0 !important;' +
            '  margin: 0 !important;' +
            '  padding: 0 !important;' +
            '  max-width: none !important;' +
            '  max-height: none !important;' +
            '  overflow: visible !important;' +
            '  display: flex !important;' +
            '  align-items: center !important;' +
            '  justify-content: center !important;' +
            '}' +
            '#vid-cont.fame-pseudo-fullscreen .ratio {' +
            '  width: 100vw !important;' +
            '  height: 100vh !important;' +
            '  max-width: 100vw !important;' +
            '  max-height: 100vh !important;' +
            '  padding-top: 0 !important;' +
            '}' +
            '#vid-cont.fame-pseudo-fullscreen .ratio::before {' +
            '  display: none !important;' +
            '}' +
            '#vid-cont.fame-pseudo-fullscreen .ratio iframe,' +
            '#vid-cont.fame-pseudo-fullscreen .ratio video {' +
            '  position: absolute !important;' +
            '  top: 0 !important;' +
            '  left: 0 !important;' +
            '  width: 100% !important;' +
            '  height: 100% !important;' +
            '}' +
            '#vid-cont.fame-pseudo-fullscreen #fame-wm-canvas {' +
            '  position: fixed !important;' +
            '  top: 0 !important;' +
            '  left: 0 !important;' +
            '  width: 100vw !important;' +
            '  height: 100vh !important;' +
            '  z-index: 2147483647 !important;' +
            '  pointer-events: none !important;' +
            '}' +
            '#vid-cont.fame-pseudo-fullscreen #fame-fullscreen-btn {' +
            '  position: fixed !important;' +
            '  top: 16px !important;' +
            '  right: 16px !important;' +
            '  bottom: auto !important;' +
            '  z-index: 2147483646 !important;' +
            '  opacity: 0.7 !important;' +
            '}' +
            '#vid-cont.fame-pseudo-fullscreen #videoLoader {' +
            '  z-index: 2147483640 !important;' +
            '}' +
            'body.fame-body-fullscreen {' +
            '  overflow: hidden !important;' +
            '}' +
            
            /* ===== FULLSCREEN BUTTON BASE STYLES ===== */
            '#fame-fullscreen-btn {' +
            '  position: absolute;' +
            '  top: 10px;' +
            '  right: 10px;' +
            '  z-index: 20;' +
            '  background: rgba(0,0,0,0.6);' +
            '  color: #fff;' +
            '  border: 1px solid rgba(255,255,255,0.25);' +
            '  border-radius: 6px;' +
            '  padding: 6px 14px;' +
            '  cursor: pointer;' +
            '  font-size: 14px;' +
            '  line-height: 1;' +
            '  pointer-events: auto;' +
            '  -webkit-tap-highlight-color: transparent;' +
            '  touch-action: manipulation;' +
            '  display: flex;' +
            '  align-items: center;' +
            '  gap: 6px;' +
            '}' +
            /* Desktop: show on hover */
            '@media (hover: hover) and (pointer: fine) {' +
            '  #fame-fullscreen-btn {' +
            '    opacity: 0;' +
            '    transition: opacity 0.3s ease;' +
            '  }' +
            '  #vid-cont:hover #fame-fullscreen-btn { opacity: 0.85; }' +
            '  #fame-fullscreen-btn:hover { opacity: 1 !important; background: rgba(0,0,0,0.85); }' +
            '}' +
            /* Mobile/Touch: always visible */
            '@media (hover: none), (pointer: coarse) {' +
            '  #fame-fullscreen-btn {' +
            '    opacity: 0.7 !important;' +
            '    padding: 10px 14px;' +
            '    font-size: 20px;' +
            '  }' +
            '  #fame-fullscreen-btn:active { opacity: 1 !important; background: rgba(0,0,0,0.9); }' +
            '}';
        
        document.head.appendChild(style);
    }

    // Add custom fullscreen button to the video container
    function addFullscreenButton() {
        var vidCont = document.querySelector('#vid-cont');
        if (!vidCont || document.getElementById('fame-fullscreen-btn')) return;
        
        var btn = document.createElement('button');
        btn.id = 'fame-fullscreen-btn';
        btn.innerHTML = '<i class="bi bi-fullscreen" style="font-size:16px;"></i><span style="font-size:13px;font-weight:600;white-space:nowrap;">Full Screen</span>';
        btn.title = 'Fullscreen';
        btn.setAttribute('aria-label', 'Toggle Fullscreen');
        
        // Use both click and touchend for reliable mobile response
        var handleToggle = function(e) {
            e.preventDefault();
            e.stopPropagation();
            toggleContainerFullscreen();
        };
        btn.addEventListener('click', handleToggle);
        
        vidCont.appendChild(btn);
    }

    // DevTools detection - DESKTOP ONLY (mobile has too many false positives)
    function detectDevTools() {
        // Skip DevTools detection on mobile devices - too many false positives
        // due to browser UI elements (address bar, toolbars, etc.)
        if (isMobile) {
            return;
        }
        
        var threshold = _0x.devToolsThreshold;
        
        setInterval(function() {
            // Skip if mobile (double-check in case viewport changed)
            if (isMobile || window.innerWidth <= 768) {
                return;
            }
            
            var widthDiff = window.outerWidth - window.innerWidth;
            var heightDiff = window.outerHeight - window.innerHeight;
            
            // Only trigger if BOTH width and height show significant difference
            // This reduces false positives from browser toolbars
            if (widthDiff > threshold && heightDiff > threshold) {
                logViolation('DEVTOOLS_OPEN', 'DevTools detected via window size');
                showWarning('Developer tools detected. This activity has been logged.');
            }
        }, 2000); // Reduced frequency to minimize impact
        
        // Debugger trap - DESKTOP ONLY
        var devtools = { open: false };
        var element = new Image();
        
        Object.defineProperty(element, 'id', {
            get: function() {
                devtools.open = true;
                logViolation('DEVTOOLS_OPEN', 'DevTools detected via debugger');
            }
        });
        
        setInterval(function() {
            if (isMobile) return; // Skip on mobile
            devtools.open = false;
            console.log(element);
            console.clear();
        }, 2000);
    }

    // Tab visibility handling
    function handleVisibilityChange() {
        if (document.hidden) {
            state.tabHiddenTime = Date.now();
        } else {
            if (state.tabHiddenTime) {
                var hiddenDuration = Date.now() - state.tabHiddenTime;
                // On mobile, users often switch apps - use longer threshold (30 seconds)
                // On desktop, 5 seconds is suspicious
                var threshold = isMobile ? 30000 : 5000;
                
                if (hiddenDuration > threshold) {
                    // Only log as info on mobile, violation on desktop
                    if (!isMobile) {
                        logViolation('TAB_HIDDEN', 'Tab hidden for ' + Math.round(hiddenDuration/1000) + ' seconds');
                    }
                }
                state.tabHiddenTime = null;
            }
        }
    }

    // Window blur handling
    function handleWindowBlur() {
        // On mobile, blur events happen frequently due to notifications, 
        // app switching, etc. - don't log these as violations
        if (isMobile) {
            return;
        }
        
        state.blurCount++;
        
        // Reset count every minute
        if (Date.now() - state.lastBlurReset > 60000) {
            state.blurCount = 1;
            state.lastBlurReset = Date.now();
        }
        
        // Only log if threshold exceeded AND not on mobile
        if (state.blurCount > _0x.blurThreshold) {
            logViolation('WINDOW_BLUR_FREQUENT', 'Excessive window switching detected');
        }
    }

    // Window focus handling
    function handleWindowFocus() {
        // Resume watermark if paused
        state.isPaused = false;
    }

    // Context menu handling
    function handleContextMenu(e) {
        // On mobile, context menu is triggered by long-press which is common
        // Don't log as violation on mobile
        e.preventDefault();
        if (!isMobile) {
            logViolation('RIGHT_CLICK_ATTEMPT', 'Right-click attempted');
        }
        return false;
    }

    // Keyboard shortcut handling
    function handleKeyDown(e) {
        // Block common shortcuts
        var blocked = [
            { key: 'F12', ctrl: false, shift: false },
            { key: 'I', ctrl: true, shift: true },
            { key: 'J', ctrl: true, shift: true },
            { key: 'C', ctrl: true, shift: true },
            { key: 'U', ctrl: true, shift: false },
            { key: 'S', ctrl: true, shift: false },
            { key: 'PrintScreen', ctrl: false, shift: false }
        ];
        
        for (var i = 0; i < blocked.length; i++) {
            var shortcut = blocked[i];
            if (e.key === shortcut.key && 
                e.ctrlKey === shortcut.ctrl && 
                e.shiftKey === shortcut.shift) {
                e.preventDefault();
                logViolation('KEYBOARD_SHORTCUT', 'Blocked shortcut: ' + e.key);
                showWarning('This keyboard shortcut is disabled for security reasons.');
                return false;
            }
        }
    }

    // Picture-in-Picture handling
    function handlePiP() {
        logViolation('PIP_DETECTED', 'Picture-in-Picture mode activated');
        showWarning('Picture-in-Picture mode is not allowed.');
        
        // Try to exit PiP
        if (document.pictureInPictureElement) {
            document.exitPictureInPicture();
        }
    }

    // Console access detection
    function detectConsoleAccess() {
        var consoleOpened = false;
        
        var originalLog = console.log;
        console.log = function() {
            if (!consoleOpened && arguments[0] && typeof arguments[0] === 'object') {
                // Skip our own detection
            }
            return originalLog.apply(console, arguments);
        };
    }

    // Screen capture detection (limited browser support)
    function detectScreenCapture() {
        // Monitor for getDisplayMedia calls
        if (navigator.mediaDevices && navigator.mediaDevices.getDisplayMedia) {
            var originalGetDisplayMedia = navigator.mediaDevices.getDisplayMedia.bind(navigator.mediaDevices);
            
            navigator.mediaDevices.getDisplayMedia = function(constraints) {
                logViolation('SCREEN_CAPTURE_DETECTED', 'Screen capture API called');
                showWarning('Screen recording has been detected and logged.');
                return originalGetDisplayMedia(constraints);
            };
        }
        
        // Detect screen capture through MediaRecorder
        if (window.MediaRecorder) {
            var originalMediaRecorder = window.MediaRecorder;
            
            window.MediaRecorder = function(stream, options) {
                // Check if stream is from display
                if (stream.getVideoTracks) {
                    var tracks = stream.getVideoTracks();
                    for (var i = 0; i < tracks.length; i++) {
                        var settings = tracks[i].getSettings();
                        if (settings.displaySurface) {
                            logViolation('SCREEN_CAPTURE_DETECTED', 'MediaRecorder with display surface');
                        }
                    }
                }
                return new originalMediaRecorder(stream, options);
            };
            
            window.MediaRecorder.prototype = originalMediaRecorder.prototype;
        }
    }

    // Start integrity checks
    function startIntegrityChecks() {
        state.checkTimer = setInterval(function() {
            // Check if canvas exists
            if (!document.getElementById('fame-wm-canvas')) {
                logViolation('WATERMARK_TAMPER', 'Canvas missing during integrity check');
                recreateCanvas();
            }
            
            // Check canvas visibility
            var canvas = document.getElementById('fame-wm-canvas');
            if (canvas) {
                var style = window.getComputedStyle(canvas);
                if (style.display === 'none' || style.visibility === 'hidden' || parseFloat(style.opacity) < 0.1) {
                    logViolation('WATERMARK_TAMPER', 'Canvas hidden via CSS');
                    resetCanvasStyle();
                }
            }
        }, _0x.checkInterval);
    }

    // Log violation to server
    function logViolation(type, details) {
        state.violationCount++;
        
        var data = {
            ViolationType: type,
            VideoId: state.videoId,
            VideoName: state.videoName,
            Details: details,
            Fingerprint: state.sessionId
        };
        
        // Send to server
        var xhr = new XMLHttpRequest();
        xhr.open('POST', '/Student/LogSecurityViolation', true);
        xhr.setRequestHeader('Content-Type', 'application/json');
        xhr.send(JSON.stringify(data));
        
        // Check if should pause video
        if (state.violationCount >= 3) {
            pauseVideo();
            showWarning('Multiple security violations detected. Please contact support if you believe this is an error.');
        }
    }

    // Show warning to user
    function showWarning(message) {
        // Use SweetAlert if available
        if (window.Swal) {
            Swal.fire({
                title: 'Security Alert',
                text: message,
                icon: 'warning',
                confirmButtonText: 'I Understand'
            });
        } else {
            alert('Security Alert: ' + message);
        }
    }

    // Pause video
    function pauseVideo() {
        state.isPaused = true;
        
        // Try to pause iframe video
        var iframe = document.querySelector('#MyIframe, iframe');
        if (iframe && iframe.contentWindow) {
            try {
                iframe.contentWindow.postMessage('{"event":"command","func":"pauseVideo","args":""}', '*');
            } catch (e) {}
        }
        
        // Try to pause HTML5 video
        var video = document.querySelector('video');
        if (video) {
            video.pause();
        }
    }

    // Update video context
    function setVideoContext(videoId, videoName) {
        state.videoId = videoId;
        state.videoName = videoName;
    }

    // Cleanup
    function destroy() {
        if (state.watermarkTimer) clearInterval(state.watermarkTimer);
        if (state.checkTimer) clearInterval(state.checkTimer);
        if (state.animationFrame) cancelAnimationFrame(state.animationFrame);
        if (state.canvas) state.canvas.remove();
        state.isInitialized = false;
    }

    // Expose public API
    window.FAMEWatermark = {
        init: init,
        setVideoContext: setVideoContext,
        refresh: function() {
            loadWatermarkData();
            resizeCanvas();
        },
        destroy: destroy,
        toggleFullscreen: toggleContainerFullscreen
    };

    // Auto-initialize on DOM ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function() {
            // Wait for video container to be ready
            setTimeout(init, 1000);
        });
    } else {
        setTimeout(init, 1000);
    }

})(window, document);
