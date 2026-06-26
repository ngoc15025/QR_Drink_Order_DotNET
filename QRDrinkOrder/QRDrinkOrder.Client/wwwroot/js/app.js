window.gsapHelpers = {
    staggerLoad: function(selector) {
        if (typeof gsap === 'undefined') return;
        gsap.fromTo(selector, 
            { y: 30, opacity: 0 }, 
            { y: 0, opacity: 1, duration: 0.5, stagger: 0.05, ease: "power2.out", overwrite: "auto" }
        );
    }
};

window.menuScrollSpy = {
    isScrolling: false,
    lastActiveId: null,
    _scrollListener: null,
    _dotNetHelper: null,
    scrollToCategory: function (id) {
        this.isScrolling = true;
        this.lastActiveId = id.toString();
        const element = document.getElementById('cat-' + id);
        const container = document.getElementById('menu-content-scroll');
        if (element && container) {
            const containerRect = container.getBoundingClientRect();
            const elementRect = element.getBoundingClientRect();
            // Trừ đi 60px để header vừa khớp với phần top sticky (ở Home/Menu đều dùng top 60px)
            const scrollTop = container.scrollTop + (elementRect.top - containerRect.top) - 60;
            container.scrollTo({ top: scrollTop, behavior: "smooth" });
        }
        setTimeout(() => { this.isScrolling = false; }, 800);
    },
    scrollToTop: function () {
        this.isScrolling = true;
        this.lastActiveId = null;
        const container = document.getElementById('menu-content-scroll');
        if (container) {
            container.scrollTo({ top: 0, behavior: "smooth" });
        }
        setTimeout(() => { this.isScrolling = false; }, 800);
    },
    initialize: function (dotNetHelper) {
        const container = document.getElementById('menu-content-scroll');
        console.log("menuScrollSpy initialized - v3.3", {
            container: !!container,
            headersCount: document.querySelectorAll('.category-header').length
        });
        this._dotNetHelper = dotNetHelper;

        // Xóa listener cũ
        if (this._scrollListener) {
            window.removeEventListener('scroll', this._scrollListener);
            const oldContainer = document.getElementById('menu-content-scroll');
            if (oldContainer) oldContainer.removeEventListener('scroll', this._scrollListener);
        }

        this._scrollListener = function () {
            if (window.menuScrollSpy.isScrolling) return;

            const headers = document.querySelectorAll('.category-header');
            let currentActiveId = null;
            const scrollContainer = document.getElementById('menu-content-scroll');
            if (scrollContainer) {
                const containerRect = scrollContainer.getBoundingClientRect();
                for (let i = 0; i < headers.length; i++) {
                    const header = headers[i];
                    const rect = header.getBoundingClientRect();
                    const offset = rect.top - containerRect.top;

                    // Tăng threshold lên 150 để khi category vừa ló đầu lên một chút là sidebar chuyển active luôn
                    if (offset <= 150) {
                        currentActiveId = header.id.replace('cat-', '');
                    } else {
                        break;
                    }
                }
            }

            if (currentActiveId !== window.menuScrollSpy.lastActiveId && window.menuScrollSpy._dotNetHelper) {
                try {
                    window.menuScrollSpy.lastActiveId = currentActiveId;
                    const arg = currentActiveId ? parseInt(currentActiveId) : null;
                    window.menuScrollSpy._dotNetHelper.invokeMethodAsync('UpdateActiveCategory', arg);

                    // Auto scroll sidebar để active item luôn nằm trong tầm nhìn
                    setTimeout(() => {
                        const sidebarId = currentActiveId ? 'sidebar-cat-' + currentActiveId : 'sidebar-cat-all';
                        const sidebarItem = document.getElementById(sidebarId);
                        if (sidebarItem) {
                            sidebarItem.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
                        }
                    }, 50); // Đợi Blazor render class active
                } catch (e) {
                    console.error("error invoking UpdateActiveCategory:", e);
                }
            }
        };

        window.addEventListener('scroll', this._scrollListener);
        if (container) {
            container.addEventListener('scroll', this._scrollListener);
        }
    },
    dispose: function () {
        console.log("menuScrollSpy disposed");
        if (this._scrollListener) {
            window.removeEventListener('scroll', this._scrollListener);
            const container = document.getElementById('menu-content-scroll');
            if (container) container.removeEventListener('scroll', this._scrollListener);
            this._scrollListener = null;
        }
        this._dotNetHelper = null;
    }
};

// --- Dashboard Charts ---
window._dashChartInstances = {};
function destroyChart(id) {
    if (window._dashChartInstances[id]) {
        window._dashChartInstances[id].destroy();
        delete window._dashChartInstances[id];
    }
}

window.dashCharts = {
    // Chart 1: Daily Revenue — Line with area fill
    renderDailyRevenue: function(canvasId, labels, revenues, orders) {
        const ctx = document.getElementById(canvasId);
        if (!ctx) return;
        destroyChart(canvasId);
        window._dashChartInstances[canvasId] = new Chart(ctx, {
            type: 'line',
            data: {
                labels: labels,
                datasets: [{
                    label: 'Doanh thu (đ)',
                    data: revenues,
                    borderColor: '#EA8025',
                    backgroundColor: function(context) {
                        const chart = context.chart;
                        const {ctx: c, chartArea} = chart;
                        if (!chartArea) return 'rgba(234,128,37,0.1)';
                        const gradient = c.createLinearGradient(0, chartArea.top, 0, chartArea.bottom);
                        gradient.addColorStop(0, 'rgba(234,128,37,0.35)');
                        gradient.addColorStop(1, 'rgba(234,128,37,0.02)');
                        return gradient;
                    },
                    fill: true,
                    tension: 0.4,
                    pointBackgroundColor: '#EA8025',
                    pointRadius: 4,
                    pointHoverRadius: 6,
                    borderWidth: 2.5
                }]
            },
            options: {
                responsive: true, maintainAspectRatio: false,
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        callbacks: {
                            label: (ctx) => ` ${ctx.parsed.y.toLocaleString('vi-VN')}đ`,
                            afterLabel: (ctx) => orders && orders[ctx.dataIndex] ? `  ${orders[ctx.dataIndex]} đơn hàng` : ''
                        }
                    }
                },
                scales: {
                    x: { grid: { display: false }, ticks: { font: { size: 11 } } },
                    y: {
                        beginAtZero: true, grid: { color: 'rgba(0,0,0,0.05)' },
                        ticks: { callback: v => v >= 1000000 ? (v/1000000).toFixed(1)+'M' : v >= 1000 ? (v/1000)+'K' : v, font: { size: 11 } }
                    }
                }
            }
        });
    },

    // Chart 2: Payment Donut
    renderPaymentDonut: function(canvasId, cash, sepay) {
        const ctx = document.getElementById(canvasId);
        if (!ctx) return;
        destroyChart(canvasId);
        const total = cash + sepay;
        window._dashChartInstances[canvasId] = new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: ['Tiền mặt', 'SePay'],
                datasets: [{
                    data: total > 0 ? [cash, sepay] : [1, 1],
                    backgroundColor: ['#F5A623', '#3B82F6'],
                    borderWidth: 2,
                    borderColor: '#fff',
                    hoverOffset: 6
                }]
            },
            options: {
                responsive: true, maintainAspectRatio: false,
                cutout: '68%',
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        callbacks: {
                            label: (ctx) => ` ${ctx.parsed.toLocaleString('vi-VN')}đ`
                        }
                    }
                }
            }
        });
    },

    // Chart 3: Peak Hours — Mixed Bar + Line
    renderPeakHours: function(canvasId, labels, revenues, orderCounts) {
        const ctx = document.getElementById(canvasId);
        if (!ctx) return;
        destroyChart(canvasId);
        window._dashChartInstances[canvasId] = new Chart(ctx, {
            data: {
                labels: labels,
                datasets: [
                    {
                        type: 'bar',
                        label: 'Doanh thu (đ)',
                        data: revenues,
                        backgroundColor: 'rgba(234,128,37,0.75)',
                        borderRadius: 4,
                        yAxisID: 'yRevenue',
                        order: 2
                    },
                    {
                        type: 'line',
                        label: 'Số đơn',
                        data: orderCounts,
                        borderColor: '#1e3a5f',
                        backgroundColor: 'transparent',
                        pointBackgroundColor: '#1e3a5f',
                        pointRadius: 3,
                        tension: 0.4,
                        borderWidth: 2,
                        yAxisID: 'yOrders',
                        order: 1
                    }
                ]
            },
            options: {
                responsive: true, maintainAspectRatio: false,
                plugins: {
                    legend: { position: 'top', labels: { boxWidth: 12, font: { size: 11 } } }
                },
                scales: {
                    x: { grid: { display: false }, ticks: { font: { size: 10 } } },
                    yRevenue: {
                        position: 'left', beginAtZero: true, grid: { color: 'rgba(0,0,0,0.05)' },
                        ticks: { callback: v => v >= 1000 ? (v/1000)+'K' : v, font: { size: 10 } }
                    },
                    yOrders: {
                        position: 'right', beginAtZero: true, grid: { display: false },
                        ticks: { font: { size: 10 }, stepSize: 1 }
                    }
                }
            }
        });
    },

    // Chart 4: Coupon Usage Bar
    renderCoupon: function(canvasId, labels, timesUsed, discounts) {
        const ctx = document.getElementById(canvasId);
        if (!ctx) return;
        destroyChart(canvasId);
        window._dashChartInstances[canvasId] = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [
                    {
                        label: 'Số lần dùng',
                        data: timesUsed,
                        backgroundColor: 'rgba(139,92,246,0.8)',
                        borderRadius: 4,
                        yAxisID: 'yUsed'
                    },
                    {
                        label: 'Tổng giảm (đ)',
                        data: discounts,
                        backgroundColor: 'rgba(236,72,153,0.7)',
                        borderRadius: 4,
                        yAxisID: 'yDiscount'
                    }
                ]
            },
            options: {
                responsive: true, maintainAspectRatio: false,
                plugins: {
                    legend: { position: 'top', labels: { boxWidth: 12, font: { size: 11 } } }
                },
                scales: {
                    x: { grid: { display: false } },
                    yUsed: {
                        position: 'left', beginAtZero: true, grid: { color: 'rgba(0,0,0,0.05)' },
                        ticks: { stepSize: 1, font: { size: 10 } }
                    },
                    yDiscount: {
                        position: 'right', beginAtZero: true, grid: { display: false },
                        ticks: { callback: v => v >= 1000 ? (v/1000)+'K' : v, font: { size: 10 } }
                    }
                }
            }
        });
    }
};

// --- Flying Animation for Cart ---
window.animateAddToCart = function(imageSrc, clientX, clientY) {
    const cartIcon = document.querySelector('.bi-bag');
    if (!cartIcon) return;

    const flyingImg = document.createElement('img');
    flyingImg.src = imageSrc || '/images/placeholder.png';
    flyingImg.style.position = 'fixed';
    flyingImg.style.width = '60px';
    flyingImg.style.height = '60px';
    flyingImg.style.borderRadius = '50%';
    flyingImg.style.zIndex = '9999';
    flyingImg.style.objectFit = 'cover';
    flyingImg.style.left = clientX + 'px';
    flyingImg.style.top = clientY + 'px';
    flyingImg.style.boxShadow = '0 4px 12px rgba(0,0,0,0.15)';
    flyingImg.style.transition = 'all 0.6s cubic-bezier(0.25, 0.8, 0.25, 1)';
    document.body.appendChild(flyingImg);

    // Trigger reflow
    void flyingImg.offsetWidth;

    setTimeout(() => {
        const cartRect = cartIcon.getBoundingClientRect();
        flyingImg.style.left = (cartRect.left - 10) + 'px';
        flyingImg.style.top = (cartRect.top - 10) + 'px';
        flyingImg.style.transform = 'scale(0.1)';
        flyingImg.style.opacity = '0.5';
    }, 50);

    setTimeout(() => {
        if (document.body.contains(flyingImg)) {
            document.body.removeChild(flyingImg);
        }
        const cartContainer = cartIcon.closest('.tch-header-icon');
        if (cartContainer) {
            cartContainer.classList.add('cart-bounce');
            setTimeout(() => cartContainer.classList.remove('cart-bounce'), 300);
        }
    }, 650);
};

window.printInvoice = function(invoiceHtml) {
    var printWindow = window.open('', '_blank', 'width=450,height=600');
    printWindow.document.write('<html><head><title>Hóa đơn thanh toán</title>');
    printWindow.document.write('<style>');
    printWindow.document.write('body { font-family: "Be Vietnam Pro", "Segoe UI", Arial, sans-serif; padding: 20px; color: #333; margin: 0; }');
    printWindow.document.write('.invoice-box { max-width: 100%; margin: auto; padding: 10px; font-size: 14px; line-height: 24px; }');
    printWindow.document.write('.text-center { text-align: center; }');
    printWindow.document.write('.text-right { text-align: right; }');
    printWindow.document.write('.bold { font-weight: bold; }');
    printWindow.document.write('.divider { border-top: 1px dashed #333; margin: 10px 0; }');
    printWindow.document.write('.header-title { font-size: 18px; font-weight: bold; margin-bottom: 5px; letter-spacing: 0.5px; }');
    printWindow.document.write('.item-table { width: 100%; border-collapse: collapse; margin: 10px 0; }');
    printWindow.document.write('.item-table th { border-bottom: 1px solid #333; text-align: left; font-weight: bold; }');
    printWindow.document.write('.item-table td { padding: 6px 0; vertical-align: top; }');
    printWindow.document.write('small { color: #666; font-size: 11px; display: block; line-height: 14px; }');
    printWindow.document.write('</style></head><body>');
    printWindow.document.write(invoiceHtml);
    printWindow.document.write('</body></html>');
    printWindow.document.close();
    
    setTimeout(function() {
        printWindow.print();
        printWindow.close();
    }, 300);
};

// --- Promo Carousel ---
window.promoCarousel = {
    goTo: function(slidePercent) {
        var el = document.getElementById('promoSlides');
        if (el) {
            el.style.transform = 'translateX(-' + slidePercent + '%)';
        }
    }
};

// --- Push Notification Service Worker ---
window.blazorPushNotifications = {
    requestSubscription: async () => {
        const permission = await Notification.requestPermission();
        if (permission !== 'granted') return null;

        const worker = await navigator.serviceWorker.ready;
        const existingSubscription = await worker.pushManager.getSubscription();
        if (existingSubscription) {
            return {
                url: existingSubscription.endpoint,
                p256dh: arrayBufferToBase64(existingSubscription.getKey('p256dh')),
                auth: arrayBufferToBase64(existingSubscription.getKey('auth'))
            };
        }
        
        const subscribeOptions = {
            userVisibleOnly: true,
            applicationServerKey: urlB64ToUint8Array('BIBaoDjpghKOdpqm-kGYrvZH4QzM217PQnl7AN8mEvy10nixIwUHgkJ-lvMWo9ZAWVRO3iDG-nUghTFapiHLav8')
        };
        
        const subscription = await worker.pushManager.subscribe(subscribeOptions);
        if (subscription) {
            return {
                url: subscription.endpoint,
                p256dh: arrayBufferToBase64(subscription.getKey('p256dh')),
                auth: arrayBufferToBase64(subscription.getKey('auth'))
            };
        }
        return null;
    }
};

function arrayBufferToBase64(buffer) {
    var binary = '';
    var bytes = new Uint8Array(buffer);
    var len = bytes.byteLength;
    for (var i = 0; i < len; i++) {
        binary += String.fromCharCode(bytes[i]);
    }
    return window.btoa(binary);
}

function urlB64ToUint8Array(base64String) {
    const padding = '='.repeat((4 - base64String.length % 4) % 4);
    const base64 = (base64String + padding).replace(/\-/g, '+').replace(/_/g, '/');
    const rawData = window.atob(base64);
    const outputArray = new Uint8Array(rawData.length);
    for (let i = 0; i < rawData.length; ++i) {
        outputArray[i] = rawData.charCodeAt(i);
    }
    return outputArray;
}

if ('serviceWorker' in navigator) {
    navigator.serviceWorker.register('service-worker.js');
}
