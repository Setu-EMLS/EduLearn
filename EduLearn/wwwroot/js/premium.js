// EduLearn public website behaviour
(function () {
    'use strict';

    // Header shadow once the page scrolls; back-to-top button
    var header = document.querySelector('.ps-header');
    var toTop = document.querySelector('.ps-top');
    function onScroll() {
        var y = window.scrollY || document.documentElement.scrollTop;
        if (header) header.classList.toggle('scrolled', y > 10);
        if (toTop) toTop.classList.toggle('show', y > 600);
    }
    window.addEventListener('scroll', onScroll, { passive: true });
    onScroll();
    if (toTop) toTop.addEventListener('click', function () { window.scrollTo({ top: 0, behavior: 'smooth' }); });

    // Animated counters: <span data-count="1232">0</span>
    var counters = document.querySelectorAll('[data-count]');
    function animate(el) {
        var target = parseInt(el.getAttribute('data-count'), 10) || 0;
        if (window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches) {
            el.textContent = target.toLocaleString();
            return;
        }
        var duration = 1600, start = null;
        function step(ts) {
            if (!start) start = ts;
            var p = Math.min((ts - start) / duration, 1);
            var eased = 1 - Math.pow(1 - p, 3);
            el.textContent = Math.round(target * eased).toLocaleString();
            if (p < 1) requestAnimationFrame(step);
        }
        requestAnimationFrame(step);
    }
    if ('IntersectionObserver' in window) {
        var io = new IntersectionObserver(function (entries) {
            entries.forEach(function (e) {
                if (e.isIntersecting) { animate(e.target); io.unobserve(e.target); }
            });
        }, { threshold: 0.4 });
        counters.forEach(function (c) { io.observe(c); });
    } else {
        counters.forEach(function (c) { c.textContent = parseInt(c.getAttribute('data-count'), 10).toLocaleString(); });
    }

    // Password show / hide: <button class="toggle-pw" data-target="#Password">
    document.addEventListener('click', function (e) {
        var btn = e.target.closest('.toggle-pw');
        if (!btn) return;
        var input = document.querySelector(btn.getAttribute('data-target'));
        if (!input) return;
        var show = input.type === 'password';
        input.type = show ? 'text' : 'password';
        var icon = btn.querySelector('i');
        if (icon) icon.className = show ? 'bi bi-eye-slash' : 'bi bi-eye';
    });

    // Copy account numbers: <button class="copy-btn" data-copy="0191...">
    document.addEventListener('click', function (e) {
        var btn = e.target.closest('[data-copy]');
        if (!btn || !navigator.clipboard) return;
        navigator.clipboard.writeText(btn.getAttribute('data-copy')).then(function () {
            var icon = btn.querySelector('i');
            if (!icon) return;
            var old = icon.className;
            icon.className = 'bi bi-check2';
            setTimeout(function () { icon.className = old; }, 1500);
        });
    });

    // Header search popover
    var searchToggle = document.querySelector('.search-toggle');
    var searchPop = document.getElementById('searchPop');
    if (searchToggle && searchPop) {
        searchToggle.addEventListener('click', function (e) {
            e.stopPropagation();
            var open = searchPop.classList.toggle('show');
            searchToggle.setAttribute('aria-expanded', open ? 'true' : 'false');
            if (open) { var input = searchPop.querySelector('input'); if (input) input.focus(); }
        });
        document.addEventListener('click', function (e) {
            if (!searchPop.contains(e.target)) { searchPop.classList.remove('show'); searchToggle.setAttribute('aria-expanded', 'false'); }
        });
    }

    // Testimonial carousel: arrows scroll the track; hidden when everything fits
    document.querySelectorAll('[data-carousel]').forEach(function (wrap) {
        var track = wrap.querySelector('.testi-track');
        var nav = wrap.querySelector('.testi-nav');
        if (!track || !nav) return;
        function sync() { nav.style.visibility = track.scrollWidth > track.clientWidth + 4 ? 'visible' : 'hidden'; }
        nav.querySelectorAll('[data-dir]').forEach(function (btn) {
            btn.addEventListener('click', function () {
                var card = track.firstElementChild;
                var step = card ? card.getBoundingClientRect().width + 24 : track.clientWidth;
                track.scrollBy({ left: step * parseInt(btn.getAttribute('data-dir'), 10), behavior: 'smooth' });
            });
        });
        window.addEventListener('resize', sync);
        sync();
    });

    // Close the mobile menu after picking a link
    document.querySelectorAll('.ps-header .navbar-collapse .nav-link').forEach(function (link) {
        link.addEventListener('click', function () {
            var collapse = document.querySelector('.ps-header .navbar-collapse.show');
            if (collapse && window.bootstrap) bootstrap.Collapse.getOrCreateInstance(collapse).hide();
        });
    });

    // Toasts fade out on their own
    setTimeout(function () {
        document.querySelectorAll('.ps-toast').forEach(function (t) {
            t.style.transition = 'opacity .4s ease';
            t.style.opacity = '0';
            setTimeout(function () { t.remove(); }, 450);
        });
    }, 6000);

    // Scroll animations. Without the library (failed to load) or with reduced motion requested,
    // drop the data-aos attributes so nothing stays hidden.
    var reduceMotion = window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    if (window.AOS && !reduceMotion) {
        AOS.init({ duration: 700, easing: 'ease-out-cubic', once: true, offset: 60 });
    } else {
        document.querySelectorAll('[data-aos]').forEach(function (el) { el.removeAttribute('data-aos'); });
    }
})();
