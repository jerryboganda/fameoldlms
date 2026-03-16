/**
 * FAME LMS Student Dashboard - JavaScript
 * Version: 2026.1
 * Core functionality for student panel
 */

(function ($) {
    'use strict';

    // ============================================
    // Theme Management
    // ============================================
    const ThemeManager = {
        init: function () {
            const savedTheme = localStorage.getItem('fame-theme') || 'light';
            this.setTheme(savedTheme);

            // Bind toggle button
            $('#themeToggle').on('click', () => {
                const currentTheme = document.documentElement.getAttribute('data-theme');
                const newTheme = currentTheme === 'dark' ? 'light' : 'dark';
                this.setTheme(newTheme);
            });
        },

        setTheme: function (theme) {
            document.documentElement.setAttribute('data-theme', theme);
            localStorage.setItem('fame-theme', theme);
        }
    };

    // ============================================
    // Sidebar Management
    // ============================================
    const SidebarManager = {
        init: function () {
            const $sidebar = $('#fameSidebar');
            const $overlay = $('#sidebarOverlay');
            const $toggleBtn = $('#sidebarToggle');

            // Check saved state (only for desktop)
            if (window.innerWidth >= 1024) {
                const isCollapsed = localStorage.getItem('fame-sidebar-collapsed') === 'true';
                if (isCollapsed) {
                    $sidebar.addClass('collapsed');
                }
            }

            // Toggle button click
            $toggleBtn.on('click', () => {
                if (window.innerWidth >= 1024) {
                    // Desktop: collapse/expand
                    $sidebar.toggleClass('collapsed');
                    localStorage.setItem('fame-sidebar-collapsed', $sidebar.hasClass('collapsed'));
                } else {
                    // Mobile: show/hide
                    $sidebar.toggleClass('open');
                    $overlay.toggleClass('active');
                }
            });

            // Overlay click closes sidebar on mobile
            $overlay.on('click', () => {
                $sidebar.removeClass('open');
                $overlay.removeClass('active');
            });

            // Handle resize
            $(window).on('resize', () => {
                if (window.innerWidth >= 1024) {
                    $sidebar.removeClass('open');
                    $overlay.removeClass('active');
                }
            });
        }
    };

    // ============================================
    // Dropdown Management
    // ============================================
    const DropdownManager = {
        init: function () {
            // User dropdown
            $('#userDropdownBtn').on('click', (e) => {
                e.stopPropagation();
                const $dropdown = $('#userDropdown');
                $dropdown.toggle();
                $('#notifDropdown').hide();
            });

            // Notification dropdown
            $('#notifBtn').on('click', (e) => {
                e.stopPropagation();
                const $dropdown = $('#notifDropdown');
                $dropdown.toggle();
                $('#userDropdown').hide();

                if ($dropdown.is(':visible')) {
                    NotificationManager.loadNotifications();
                }
            });

            // Close dropdowns on outside click
            $(document).on('click', (e) => {
                if (!$(e.target).closest('.fame-user-dropdown, .fame-notif-dropdown').length) {
                    $('#userDropdown, #notifDropdown').hide();
                }
            });

            // Hover effects for dropdown items
            $('.fame-dropdown-menu a, .fame-dropdown-menu button').hover(
                function () { $(this).css('background', 'var(--fame-bg-tertiary)'); },
                function () { $(this).css('background', 'transparent'); }
            );
        }
    };

    // ============================================
    // Notification Management
    // ============================================
    const NotificationManager = {
        init: function () {
            this.checkForNotifications();
            // Poll every 30 seconds
            setInterval(() => this.checkForNotifications(), 30000);
        },

        checkForNotifications: function () {
            $.get('/Student/CheckForNotification', (response) => {
                const count = response.count || 0;
                const $badge = $('#notifBadge');

                if (count > 0) {
                    $badge.text(count > 99 ? '99+' : count).show();
                } else {
                    $badge.hide();
                }
            }).fail(() => {
                // Silently fail
            });
        },

        loadNotifications: function () {
            const $list = $('#notifList');
            $list.html('<div style="padding: 32px; text-align: center;"><div class="fame-skeleton" style="width: 100%; height: 60px; margin-bottom: 12px;"></div><div class="fame-skeleton" style="width: 100%; height: 60px;"></div></div>');

            $.get('/Student/Notifications?side=dropdown', (html) => {
                $list.html(html || '<div style="padding: 32px; text-align: center; color: var(--fame-text-tertiary);"><p>No notifications</p></div>');
                feather.replace();
            }).fail(() => {
                $list.html('<div style="padding: 32px; text-align: center; color: var(--fame-text-tertiary);"><p>Failed to load notifications</p></div>');
            });
        },

        markAsSeen: function (id) {
            $.get('/Student/HaveSeen?id=' + id, () => {
                this.checkForNotifications();
            });
        }
    };

    // ============================================
    // Search Functionality
    // ============================================
    const SearchManager = {
        init: function () {
            const $input = $('#globalSearch');
            let debounceTimer;

            $input.on('keyup', function (e) {
                clearTimeout(debounceTimer);
                const query = $(this).val().trim();

                if (e.key === 'Enter' && query.length > 0) {
                    // Navigate to search results
                    window.location.href = '/Student/MyCourses?search=' + encodeURIComponent(query);
                } else if (query.length >= 3) {
                    debounceTimer = setTimeout(() => {
                        // Could implement live search suggestions here
                        console.log('Searching for:', query);
                    }, 300);
                }
            });
        }
    };

    // ============================================
    // Progress Ring Animation
    // ============================================
    const ProgressRingManager = {
        init: function () {
            $('.fame-course-progress-ring').each(function () {
                const $ring = $(this);
                const progress = $ring.data('progress') || 0;
                $ring.css('--progress', progress);
            });
        },

        animate: function (selector, targetProgress, duration = 1000) {
            const $ring = $(selector);
            let currentProgress = 0;
            const increment = targetProgress / (duration / 16);

            const animate = () => {
                currentProgress = Math.min(currentProgress + increment, targetProgress);
                $ring.css('--progress', currentProgress);

                if (currentProgress < targetProgress) {
                    requestAnimationFrame(animate);
                }
            };

            requestAnimationFrame(animate);
        }
    };

    // ============================================
    // Toast Notifications
    // ============================================
    const ToastManager = {
        container: null,

        init: function () {
            // Create toast container if not exists
            if (!$('#fameToastContainer').length) {
                $('body').append('<div id="fameToastContainer" style="position: fixed; top: 90px; right: 24px; z-index: 9999; display: flex; flex-direction: column; gap: 12px;"></div>');
            }
            this.container = $('#fameToastContainer');
        },

        show: function (message, type = 'info', duration = 5000) {
            const icons = {
                success: 'check-circle',
                error: 'x-circle',
                warning: 'alert-triangle',
                info: 'info'
            };

            const colors = {
                success: 'var(--fame-success)',
                error: 'var(--fame-danger)',
                warning: 'var(--fame-warning)',
                info: 'var(--fame-info)'
            };

            const $toast = $(`
                <div class="fame-toast fame-animate-slide-up" style="display: flex; align-items: center; gap: 12px; padding: 16px 20px; background: var(--fame-bg-secondary); border: 1px solid var(--fame-border); border-left: 4px solid ${colors[type]}; border-radius: var(--fame-radius-lg); box-shadow: var(--fame-shadow-lg); min-width: 300px; max-width: 400px;">
                    <i data-feather="${icons[type]}" style="width: 20px; height: 20px; color: ${colors[type]}; flex-shrink: 0;"></i>
                    <span style="flex: 1; color: var(--fame-text-primary); font-size: 14px;">${message}</span>
                    <button class="fame-toast-close" style="background: none; border: none; color: var(--fame-text-tertiary); cursor: pointer; padding: 4px;">
                        <i data-feather="x" style="width: 16px; height: 16px;"></i>
                    </button>
                </div>
            `);

            this.container.append($toast);
            feather.replace();

            // Close button
            $toast.find('.fame-toast-close').on('click', () => {
                $toast.fadeOut(200, () => $toast.remove());
            });

            // Auto dismiss
            if (duration > 0) {
                setTimeout(() => {
                    $toast.fadeOut(200, () => $toast.remove());
                }, duration);
            }

            return $toast;
        },

        success: function (message) { return this.show(message, 'success'); },
        error: function (message) { return this.show(message, 'error'); },
        warning: function (message) { return this.show(message, 'warning'); },
        info: function (message) { return this.show(message, 'info'); }
    };

    // ============================================
    // Performance Chart
    // ============================================
    const ChartManager = {
        init: function () {
            // Initialize charts if Chart.js is available
            if (typeof Chart !== 'undefined') {
                this.initPerformanceChart();
            }
        },

        initPerformanceChart: function () {
            const ctx = document.getElementById('performanceChart');
            if (!ctx) return;

            const isDark = document.documentElement.getAttribute('data-theme') === 'dark';
            const gridColor = isDark ? 'rgba(255,255,255,0.1)' : 'rgba(0,0,0,0.1)';
            const textColor = isDark ? '#CBD5E1' : '#475569';

            new Chart(ctx, {
                type: 'line',
                data: {
                    labels: ctx.dataset.labels ? JSON.parse(ctx.dataset.labels) : ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'],
                    datasets: [{
                        label: 'Score %',
                        data: ctx.dataset.values ? JSON.parse(ctx.dataset.values) : [65, 72, 68, 75, 82, 78, 85],
                        borderColor: '#6366F1',
                        backgroundColor: 'rgba(99, 102, 241, 0.1)',
                        fill: true,
                        tension: 0.4,
                        pointRadius: 4,
                        pointBackgroundColor: '#6366F1'
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        legend: { display: false }
                    },
                    scales: {
                        x: {
                            grid: { color: gridColor },
                            ticks: { color: textColor }
                        },
                        y: {
                            grid: { color: gridColor },
                            ticks: { color: textColor },
                            min: 0,
                            max: 100
                        }
                    }
                }
            });
        }
    };

    // ============================================
    // Utility Functions
    // ============================================
    const Utils = {
        // Format time ago
        timeAgo: function (date) {
            const seconds = Math.floor((new Date() - new Date(date)) / 1000);
            const intervals = {
                year: 31536000,
                month: 2592000,
                week: 604800,
                day: 86400,
                hour: 3600,
                minute: 60
            };

            for (const [unit, secondsInUnit] of Object.entries(intervals)) {
                const interval = Math.floor(seconds / secondsInUnit);
                if (interval >= 1) {
                    return interval + ' ' + unit + (interval > 1 ? 's' : '') + ' ago';
                }
            }
            return 'Just now';
        },

        // Format duration
        formatDuration: function (minutes) {
            const hours = Math.floor(minutes / 60);
            const mins = minutes % 60;
            if (hours > 0) {
                return hours + 'h ' + mins + 'm';
            }
            return mins + ' min';
        },

        // Debounce function
        debounce: function (func, wait) {
            let timeout;
            return function (...args) {
                clearTimeout(timeout);
                timeout = setTimeout(() => func.apply(this, args), wait);
            };
        }
    };

    // ============================================
    // Initialize Everything
    // ============================================
    $(document).ready(function () {
        ThemeManager.init();
        SidebarManager.init();
        DropdownManager.init();
        NotificationManager.init();
        SearchManager.init();
        ProgressRingManager.init();
        ToastManager.init();
        ChartManager.init();

        // Re-render feather icons
        feather.replace();

        // Log initialization
        console.log('FAME LMS Student Dashboard initialized');
    });

    // ============================================
    // Expose to global scope
    // ============================================
    window.FAME = {
        Toast: ToastManager,
        Notifications: NotificationManager,
        ProgressRing: ProgressRingManager,
        Charts: ChartManager,
        Utils: Utils
    };

})(jQuery);
