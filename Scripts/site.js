(function () {
  'use strict';
  var $ = function (s, r) { return (r || document).querySelector(s); };
  var $$ = function (s, r) { return Array.prototype.slice.call((r || document).querySelectorAll(s)); };

  // Toast tự ẩn
  $$('.toast-msg').forEach(function (t) { setTimeout(function () { t.style.transition = 'opacity .4s'; t.style.opacity = 0; setTimeout(function () { t.remove(); }, 450); }, 6000); });

  // Xác nhận trước thao tác quan trọng: <form data-confirm="..."> hoặc <button data-confirm>
  document.addEventListener('submit', function (e) {
    var f = e.target, m = f.getAttribute('data-confirm') || (e.submitter && e.submitter.getAttribute('data-confirm'));
    if (m && !window.confirm(m)) { e.preventDefault(); return; }
    // loading state, chặn bấm đúp
    if (f.checkValidity && !f.checkValidity()) return;
    var b = e.submitter || f.querySelector('button[type=submit],input[type=submit]');
    if (b && !b.classList.contains('no-loading')) { setTimeout(function () { b.classList.add('loading'); b.setAttribute('aria-busy', 'true'); }, 0); }
  });

  // Ngày nhận/trả phòng: trả phòng luôn sau nhận phòng
  $$('form').forEach(function (f) {
    var ci = f.querySelector('input[name=checkIn],input[name=CheckIn]'), co = f.querySelector('input[name=checkOut],input[name=CheckOut]');
    if (!ci || !co) return;
    var today = new Date(); today.setHours(0, 0, 0, 0);
    var iso = function (d) { var z = new Date(d.getTime() - d.getTimezoneOffset() * 60000); return z.toISOString().slice(0, 10); };
    ci.min = iso(today);
    var sync = function () {
      if (!ci.value) { co.min = iso(new Date(today.getTime() + 86400000)); return; }
      var next = new Date(new Date(ci.value).getTime() + 86400000); co.min = iso(next);
      if (!co.value || co.value < co.min) co.value = co.min;
    };
    ci.addEventListener('change', sync); sync();
  });

  // Yêu thích (AJAX + antiforgery); chưa đăng nhập → chuyển tới trang đăng nhập
  document.addEventListener('click', function (e) {
    var b = e.target.closest('.fav-btn'); if (!b) return;
    e.preventDefault(); e.stopPropagation();
    var tok = document.querySelector('#af-token input[name=__RequestVerificationToken]');
    var body = new URLSearchParams(); body.set('id', b.getAttribute('data-id')); if (tok) body.set('__RequestVerificationToken', tok.value);
    fetch('/Favorites/Toggle', { method: 'POST', credentials: 'same-origin', headers: { 'Content-Type': 'application/x-www-form-urlencoded', 'X-Requested-With': 'XMLHttpRequest' }, body: body.toString() })
      .then(function (r) { if (r.status === 401 || r.redirected) { location.href = '/Account/Login?returnUrl=' + encodeURIComponent(location.pathname + location.search); return null; } return r.json(); })
      .then(function (j) { if (!j) return; b.classList.toggle('on', j.on); b.setAttribute('aria-pressed', j.on); toast(j.on ? 'Đã thêm vào yêu thích' : 'Đã xóa khỏi yêu thích'); })
      .catch(function () { toast('Không thể cập nhật yêu thích, vui lòng thử lại', true); });
  });

  function toast(msg, err) {
    var s = $('.toast-stack'); if (!s) return;
    var d = document.createElement('div'); d.className = 'toast-msg ' + (err ? 'err' : 'ok'); d.textContent = msg; s.appendChild(d);
    setTimeout(function () { d.remove(); }, 3500);
  }
  window.thToast = toast;

  // Lightbox gallery (nút, phím mũi tên, vuốt trên mobile)
  var gal = $$('[data-lb]');
  if (gal.length) {
    var items = gal.map(function (g) { return { src: g.getAttribute('data-lb'), cap: g.getAttribute('data-cap') || '' }; });
    var lb = document.createElement('div'); lb.className = 'lightbox'; lb.setAttribute('role', 'dialog'); lb.setAttribute('aria-modal', 'true'); lb.setAttribute('aria-label', 'Thư viện ảnh');
    lb.innerHTML = '<button class="lb-btn lb-close" aria-label="Đóng">✕</button><button class="lb-btn lb-prev" aria-label="Ảnh trước">‹</button><img alt=""><div class="lb-cap"></div><button class="lb-btn lb-next" aria-label="Ảnh sau">›</button><div class="lb-thumbs"></div>';
    document.body.appendChild(lb);
    var img = $('img', lb), cap = $('.lb-cap', lb), th = $('.lb-thumbs', lb), cur = 0;
    items.forEach(function (it, i) { var t = document.createElement('img'); t.src = it.src; t.alt = it.cap; t.loading = 'lazy'; t.addEventListener('click', function () { show(i); }); th.appendChild(t); });
    function show(i) { cur = (i + items.length) % items.length; img.src = items[cur].src; img.alt = items[cur].cap; cap.textContent = items[cur].cap + ' (' + (cur + 1) + '/' + items.length + ')'; $$('img', th).forEach(function (t, k) { t.classList.toggle('on', k === cur); }); }
    function open(i) { show(i); lb.classList.add('open'); document.body.style.overflow = 'hidden'; $('.lb-close', lb).focus(); }
    function close() { lb.classList.remove('open'); document.body.style.overflow = ''; }
    gal.forEach(function (g, i) { g.addEventListener('click', function () { open(i); }); });
    $('.lb-close', lb).onclick = close; $('.lb-prev', lb).onclick = function () { show(cur - 1); }; $('.lb-next', lb).onclick = function () { show(cur + 1); };
    lb.addEventListener('click', function (e) { if (e.target === lb) close(); });
    document.addEventListener('keydown', function (e) { if (!lb.classList.contains('open')) return; if (e.key === 'Escape') close(); if (e.key === 'ArrowLeft') show(cur - 1); if (e.key === 'ArrowRight') show(cur + 1); });
    var sx = 0; lb.addEventListener('touchstart', function (e) { sx = e.touches[0].clientX; }, { passive: true });
    lb.addEventListener('touchend', function (e) { var dx = e.changedTouches[0].clientX - sx; if (Math.abs(dx) > 50) show(cur + (dx < 0 ? 1 : -1)); });
  }

  // Đếm ngược giữ phòng
  $$('[data-countdown]').forEach(function (el) {
    var end = new Date(el.getAttribute('data-countdown')).getTime();
    (function tick() {
      var s = Math.max(0, Math.floor((end - Date.now()) / 1000));
      el.textContent = ('0' + Math.floor(s / 60)).slice(-2) + ':' + ('0' + (s % 60)).slice(-2);
      if (s === 0) { el.textContent = 'Đã hết thời gian giữ phòng'; setTimeout(function () { location.reload(); }, 2500); return; }
      setTimeout(tick, 1000);
    })();
  });


  // ---- Header trong suốt → đặc khi cuộn ----
  var hdr = $('.th-header');
  if (hdr) { var onS = function () { hdr.classList.toggle('scrolled', window.scrollY > 40); }; onS(); window.addEventListener('scroll', onS, { passive: true }); }

  // ---- Reveal khi xuất hiện (tôn trọng reduced-motion qua CSS) ----
  var rv = $$('.reveal');
  if (rv.length) {
    if (!('IntersectionObserver' in window) || window.matchMedia('(prefers-reduced-motion: reduce)').matches) rv.forEach(function (e) { e.classList.add('in'); });
    else { var io = new IntersectionObserver(function (es) { es.forEach(function (e) { if (e.isIntersecting) { e.target.classList.add('in'); io.unobserve(e.target); } }); }, { threshold: .12 }); rv.forEach(function (e) { io.observe(e); }); }
  }

  // ---- Search panel: gợi ý điểm đến, chọn ngày, khách & phòng ----
  $$('.search-panel').forEach(function (panel) {
    var dest = $('[name=q]', panel), destId = $('[name=destinationId]', panel), fDest = $('.sp-field.dest', panel), sug = $('.sp-sug', panel);
    var fGuest = $('.sp-field.guests', panel), timer, items = [], idx = -1;
    function closeAll(except) { $$('.sp-field.open', panel).forEach(function (f) { if (f !== except) f.classList.remove('open'); }); }
    document.addEventListener('click', function (e) { if (!panel.contains(e.target)) closeAll(); });
    document.addEventListener('keydown', function (e) { if (e.key === 'Escape') closeAll(); });

    // Điểm đến
    function render(list) {
      items = list; idx = -1; sug.innerHTML = '';
      if (!list.length) { var li = document.createElement('li'); li.className = 'empty-s'; li.textContent = 'Không có gợi ý — nhấn Tìm để tìm theo từ khóa'; sug.appendChild(li); return; }
      list.forEach(function (it, i) {
        var li = document.createElement('li'); li.setAttribute('role', 'option');
        li.innerHTML = '<span class="ico">' + (it.type === 'dest' ? '<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M20 10c0 6-8 12-8 12s-8-6-8-12a8 8 0 0 1 16 0Z"/><circle cx="12" cy="10" r="3"/></svg>' : '<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect width="16" height="20" x="4" y="2" rx="2"/><path d="M9 22v-4h6v4"/></svg>') + '</span><span><div class="t"></div><div class="s"></div></span>';
        $('.t', li).textContent = it.name; $('.s', li).textContent = it.sub;
        li.addEventListener('mousedown', function (e) { e.preventDefault(); pick(i); });
        sug.appendChild(li);
      });
    }
    function pick(i) { var it = items[i]; if (!it) return; dest.value = it.name; destId.value = it.type === 'dest' ? it.id : ''; fDest.classList.remove('open'); var ci = $('[name=checkIn]', panel); if (ci && !ci.value) ci.focus(); }
    function fetchSug() {
      var q = dest.value.trim();
      fetch('/Hotels/Suggest?q=' + encodeURIComponent(q), { credentials: 'same-origin' }).then(function (r) { return r.json(); }).then(render).catch(function () { render([]); });
    }
    dest.addEventListener('focus', function () { closeAll(fDest); fDest.classList.add('open'); fetchSug(); });
    dest.addEventListener('input', function () { destId.value = ''; clearTimeout(timer); timer = setTimeout(fetchSug, 180); fDest.classList.add('open'); });
    dest.addEventListener('keydown', function (e) {
      var lis = $$('li[role=option]', sug); if (!lis.length) return;
      if (e.key === 'ArrowDown' || e.key === 'ArrowUp') { e.preventDefault(); idx = (idx + (e.key === 'ArrowDown' ? 1 : -1) + lis.length) % lis.length; lis.forEach(function (l, k) { l.setAttribute('aria-selected', k === idx); }); }
      else if (e.key === 'Enter' && idx >= 0) { e.preventDefault(); pick(idx); }
    });
    fDest.addEventListener('click', function (e) { if (e.target.closest('.sp-pop')) return; dest.focus(); });

    // Ngày: bấm vào cả ô mở date picker
    $$('.sp-field.date', panel).forEach(function (f) { f.addEventListener('click', function () { var i = $('input', f); closeAll(f); try { i.showPicker && i.showPicker(); } catch (e) { i.focus(); } }); });

    // Khách & phòng
    if (fGuest) {
      var out = $('.sp-guest-text', fGuest), cfg = { adults: [1, 20], children: [0, 10], rooms: [1, 10] };
      function sync() {
        var a = +$('[name=adults]', panel).value, c = +$('[name=children]', panel).value, r = +$('[name=rooms]', panel).value;
        out.textContent = a + ' người lớn' + (c ? ', ' + c + ' trẻ em' : '') + ' · ' + r + ' phòng';
        Object.keys(cfg).forEach(function (k) { var v = +$('[name=' + k + ']', panel).value; $('[data-step="' + k + '"][data-dir="-1"]', panel).disabled = v <= cfg[k][0]; $('[data-step="' + k + '"][data-dir="1"]', panel).disabled = v >= cfg[k][1]; $('output[data-out="' + k + '"]', panel).textContent = v; });
      }
      $('.sp-guest-btn', fGuest).addEventListener('click', function (e) { e.stopPropagation(); closeAll(fGuest); fGuest.classList.toggle('open'); });
      $$('[data-step]', panel).forEach(function (b) { b.addEventListener('click', function (e) { e.stopPropagation(); var k = b.getAttribute('data-step'), inp = $('[name=' + k + ']', panel), v = +inp.value + (+b.getAttribute('data-dir')); inp.value = Math.min(cfg[k][1], Math.max(cfg[k][0], v)); if (k === 'adults' || k === 'children') { var need = Math.ceil((+$('[name=adults]', panel).value + +$('[name=children]', panel).value) / 4); } sync(); }); });
      $('.sp-pop.guest', fGuest).addEventListener('click', function (e) { e.stopPropagation(); });
      sync();
    }

    panel.addEventListener('submit', function (e) {
      var ci = $('[name=checkIn]', panel), co = $('[name=checkOut]', panel);
      if ((ci.value && !co.value) || (!ci.value && co.value)) { e.preventDefault(); window.thToast('Vui lòng chọn cả ngày nhận và trả phòng', true); (ci.value ? co : ci).focus(); return; }
      if (ci.value && co.value && co.value <= ci.value) { e.preventDefault(); window.thToast('Ngày trả phòng phải sau ngày nhận phòng', true); co.focus(); return; }
      if (!ci.value && !co.value) { ci.disabled = true; co.disabled = true; }       // không gửi tham số ngày rỗng
      if (!destId.value) destId.disabled = true;
      if (!dest.value.trim()) dest.disabled = true;
    });
  });

  // ---- Cinematic hero: parallax theo cuộn + chuột, đếm số, chuyển cảnh cất cánh khi tìm ----
  var hero = $('#hero'), reduce = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
  if (hero && !reduce) {
    var ticking = false, mx = 0, my = 0, tx = 0, ty = 0;
    function prog() { var h = hero.offsetHeight || 1; hero.style.setProperty('--p', Math.max(0, Math.min(1, window.scrollY / (h * .9))).toFixed(3)); ticking = false; }
    window.addEventListener('scroll', function () { if (!ticking) { ticking = true; requestAnimationFrame(prog); } }, { passive: true }); prog();
    if (window.matchMedia('(pointer: fine)').matches) {
      hero.addEventListener('mousemove', function (e) { var r = hero.getBoundingClientRect(); tx = (e.clientX - r.left) / r.width - .5; ty = (e.clientY - r.top) / r.height - .5; });
      (function loop() { mx += (tx - mx) * .06; my += (ty - my) * .06; hero.style.setProperty('--mx', mx.toFixed(3)); hero.style.setProperty('--my', my.toFixed(3)); requestAnimationFrame(loop); })();
    }
    var sp = hero.querySelector('.search-panel');
    if (sp) sp.addEventListener('submit', function (e) {
      if (e.defaultPrevented) return;                 // đã bị chặn do dữ liệu không hợp lệ
      e.preventDefault(); document.body.classList.add('taking-off');
      var f = sp; setTimeout(function () { HTMLFormElement.prototype.submit.call(f); }, 780);
    });
    window.addEventListener('pageshow', function () { document.body.classList.remove('taking-off'); });
  }
  $$('[data-count]').forEach(function (el) {
    var target = +el.getAttribute('data-count'); if (reduce || !('IntersectionObserver' in window)) { el.textContent = target; return; }
    var io2 = new IntersectionObserver(function (es) { es.forEach(function (e) { if (!e.isIntersecting) return; io2.disconnect(); var t0 = performance.now(); (function step(t) { var k = Math.min(1, (t - t0) / 1400), v = Math.round(target * (1 - Math.pow(1 - k, 3))); el.textContent = v; if (k < 1) requestAnimationFrame(step); })(t0); }); }, { threshold: .4 });
    io2.observe(el);
  });
  window.addEventListener('pageshow', function () { $$('.search-panel [disabled]').forEach(function (i) { i.disabled = false; }); });

  // ---- Mega menu: mở khi rê chuột/focus (desktop), chạm vào mũi tên (mobile), Esc/click ngoài để đóng, điều hướng bàn phím ----
  var megas = $$('.has-mega'), desk = function () { return window.matchMedia('(min-width: 992px)').matches; }, closeT;
  function setOpen(li, on) { li.classList.toggle('open', on); var b = $('.mega-caret', li); if (b) b.setAttribute('aria-expanded', on ? 'true' : 'false'); }
  function closeAll(except) { megas.forEach(function (m) { if (m !== except) setOpen(m, false); }); }
  megas.forEach(function (li) {
    li.addEventListener('mouseenter', function () { if (!desk()) return; clearTimeout(closeT); closeAll(li); setOpen(li, true); });
    li.addEventListener('mouseleave', function () { if (!desk()) return; closeT = setTimeout(function () { setOpen(li, false); }, 140); });
    li.addEventListener('focusin', function () { if (desk()) { closeAll(li); setOpen(li, true); } });
    var caret = $('.mega-caret', li); if (caret) caret.addEventListener('click', function (e) { e.preventDefault(); var on = !li.classList.contains('open'); closeAll(li); setOpen(li, on); });
  });
  document.addEventListener('click', function (e) { if (!e.target.closest('.has-mega')) closeAll(); });
  document.addEventListener('keydown', function (e) { if (e.key === 'Escape') closeAll(); });
  document.addEventListener('focusin', function (e) { if (desk() && !e.target.closest('.has-mega')) closeAll(); });
  // Ảnh lỗi → placeholder
  $$('img').forEach(function (i) { i.addEventListener('error', function () { if (!i.dataset.fb) { i.dataset.fb = 1; i.src = '/Content/images/placeholder.svg'; } }); });
})();
