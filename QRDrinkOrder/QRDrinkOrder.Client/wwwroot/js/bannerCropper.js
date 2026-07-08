// Banner Cropper JS Interop for 16:9 Image Cropping in Admin
window.bannerCropper = (function () {
    let canvas = null;
    let ctx = null;
    let img = null;
    let scale = 1;
    let minScale = 1;
    let maxScale = 4;
    let offsetX = 0;
    let offsetY = 0;
    let isDragging = false;
    let startX = 0;
    let startY = 0;

    function draw() {
        if (!canvas || !ctx || !img) return;

        const cw = canvas.width;
        const ch = canvas.height;

        // Clear background
        ctx.fillStyle = '#1A1D20';
        ctx.fillRect(0, 0, cw, ch);

        // Draw image with current scale and offset
        ctx.save();
        const iw = img.width * scale;
        const ih = img.height * scale;
        ctx.drawImage(img, offsetX, offsetY, iw, ih);
        ctx.restore();

        // Draw subtle rule-of-thirds guide grid
        ctx.save();
        ctx.strokeStyle = 'rgba(255, 255, 255, 0.25)';
        ctx.lineWidth = 1;
        
        // Vertical lines
        ctx.beginPath();
        ctx.moveTo(cw / 3, 0);
        ctx.lineTo(cw / 3, ch);
        ctx.moveTo((cw * 2) / 3, 0);
        ctx.lineTo((cw * 2) / 3, ch);
        ctx.stroke();

        // Horizontal lines
        ctx.beginPath();
        ctx.moveTo(0, ch / 3);
        ctx.lineTo(cw, ch / 3);
        ctx.moveTo(0, (ch * 2) / 3);
        ctx.lineTo(cw, (ch * 2) / 3);
        ctx.stroke();

        // Outer border
        ctx.strokeStyle = 'rgba(234, 128, 37, 0.8)';
        ctx.lineWidth = 2;
        ctx.strokeRect(1, 1, cw - 2, ch - 2);

        ctx.restore();
    }

    function clampOffsets() {
        if (!canvas || !img) return;
        const cw = canvas.width;
        const ch = canvas.height;
        const iw = img.width * scale;
        const ih = img.height * scale;

        // Keep image covering the canvas if larger than canvas
        if (iw >= cw) {
            if (offsetX > 0) offsetX = 0;
            if (offsetX + iw < cw) offsetX = cw - iw;
        } else {
            // Center horizontally if smaller
            offsetX = (cw - iw) / 2;
        }

        if (ih >= ch) {
            if (offsetY > 0) offsetY = 0;
            if (offsetY + ih < ch) offsetY = ch - ih;
        } else {
            // Center vertically if smaller
            offsetY = (ch - ih) / 2;
        }
    }

    function attachEvents() {
        if (!canvas) return;

        // Mouse Events
        canvas.onmousedown = function (e) {
            isDragging = true;
            startX = e.clientX - offsetX;
            startY = e.clientY - offsetY;
            canvas.style.cursor = 'grabbing';
        };

        window.onmousemove = function (e) {
            if (!isDragging) return;
            offsetX = e.clientX - startX;
            offsetY = e.clientY - startY;
            clampOffsets();
            draw();
        };

        window.onmouseup = function () {
            if (isDragging) {
                isDragging = false;
                if (canvas) canvas.style.cursor = 'grab';
            }
        };

        // Wheel Zoom
        canvas.onwheel = function (e) {
            e.preventDefault();
            const zoomFactor = e.deltaY < 0 ? 1.08 : 0.92;
            const newScale = scale * zoomFactor;
            if (newScale >= minScale && newScale <= maxScale) {
                // Zoom towards mouse point
                const rect = canvas.getBoundingClientRect();
                const mx = e.clientX - rect.left;
                const my = e.clientY - rect.top;

                offsetX = mx - (mx - offsetX) * (newScale / scale);
                offsetY = my - (my - offsetY) * (newScale / scale);
                scale = newScale;
                clampOffsets();
                draw();
            }
        };

        // Touch Events for Mobile/Tablet
        canvas.ontouchstart = function (e) {
            if (e.touches.length === 1) {
                isDragging = true;
                const touch = e.touches[0];
                startX = touch.clientX - offsetX;
                startY = touch.clientY - offsetY;
            }
        };

        canvas.ontouchmove = function (e) {
            if (!isDragging || e.touches.length !== 1) return;
            e.preventDefault();
            const touch = e.touches[0];
            offsetX = touch.clientX - startX;
            offsetY = touch.clientY - startY;
            clampOffsets();
            draw();
        };

        canvas.ontouchend = function () {
            isDragging = false;
        };
    }

    return {
        init: function (canvasId, imageSrc) {
            return new Promise((resolve, reject) => {
                canvas = document.getElementById(canvasId);
                if (!canvas) {
                    reject("Canvas not found: " + canvasId);
                    return;
                }
                ctx = canvas.getContext('2d');
                canvas.style.cursor = 'grab';

                img = new Image();
                img.crossOrigin = "anonymous";
                img.onload = function () {
                    const cw = canvas.width;
                    const ch = canvas.height;

                    // Calculate cover scale (16:9 frame)
                    const scaleX = cw / img.width;
                    const scaleY = ch / img.height;
                    minScale = Math.max(scaleX, scaleY);
                    scale = minScale;
                    maxScale = minScale * 4;

                    // Center image initially
                    offsetX = (cw - img.width * scale) / 2;
                    offsetY = (ch - img.height * scale) / 2;

                    attachEvents();
                    draw();
                    resolve(true);
                };
                img.onerror = function (err) {
                    reject("Failed to load image into cropper.");
                };
                img.src = imageSrc;
            });
        },

        setZoom: function (zoomPercent) {
            if (!img || !canvas) return;
            const targetScale = minScale * (zoomPercent / 100);
            if (targetScale >= minScale && targetScale <= maxScale) {
                const cw = canvas.width;
                const ch = canvas.height;
                const mx = cw / 2;
                const my = ch / 2;

                offsetX = mx - (mx - offsetX) * (targetScale / scale);
                offsetY = my - (my - offsetY) * (targetScale / scale);
                scale = targetScale;
                clampOffsets();
                draw();
            }
        },

        resetCenter: function () {
            if (!img || !canvas) return;
            scale = minScale;
            offsetX = (canvas.width - img.width * scale) / 2;
            offsetY = (canvas.height - img.height * scale) / 2;
            draw();
        },

        exportCroppedImage: function (targetWidth, targetHeight) {
            if (!canvas || !img) return null;

            targetWidth = targetWidth || 1280;
            targetHeight = targetHeight || 720;

            const offCanvas = document.createElement('canvas');
            offCanvas.width = targetWidth;
            offCanvas.height = targetHeight;
            const offCtx = offCanvas.getContext('2d');

            // Ratio between export resolution and display canvas resolution
            const ratioX = targetWidth / canvas.width;
            const ratioY = targetHeight / canvas.height;

            offCtx.fillStyle = '#FFFFFF';
            offCtx.fillRect(0, 0, targetWidth, targetHeight);

            const iw = img.width * scale * ratioX;
            const ih = img.height * scale * ratioY;
            const ox = offsetX * ratioX;
            const oy = offsetY * ratioY;

            offCtx.drawImage(img, ox, oy, iw, ih);

            return offCanvas.toDataURL('image/jpeg', 0.92);
        }
    };
})();
