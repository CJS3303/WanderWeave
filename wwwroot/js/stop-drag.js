(() => {
    const container = document.getElementById('trip-days');
    const status = document.getElementById('move-status');
    let dragged, originalList, originalNext, saving = false;

    function restore() {
        if (dragged) originalList.insertBefore(dragged, originalNext);
    }
    function clear() {
        dragged?.classList.remove('opacity-50');
        container.querySelectorAll('.stop-list').forEach(list => list.classList.remove('bg-cyan-50'));
        dragged = null;
        container.dispatchEvent(new CustomEvent('stop-order-settled'));
    }
    container.addEventListener('dragstart', event => {
        const row = event.target.closest('[data-stop-id]');
        if (!row || saving || event.target.closest('button, input, textarea, summary, details')) {
            event.preventDefault();
            return;
        }
        dragged = row;
        originalList = row.parentElement;
        originalNext = row.nextSibling;
        event.dataTransfer.effectAllowed = 'move';
        event.dataTransfer.setData('text/plain', row.dataset.stopId);
        row.classList.add('opacity-50');
        status.textContent = '';
    });
    container.addEventListener('dragover', event => {
        const list = event.target.closest('.stop-list');
        if (!dragged || saving || !list) return;
        event.preventDefault();
        event.dataTransfer.dropEffect = 'move';
        container.querySelectorAll('.stop-list').forEach(item => item.classList.toggle('bg-cyan-50', item === list));
        const next = [...list.querySelectorAll('[data-stop-id]')].find(row => {
            if (row === dragged) return false;
            const rect = row.getBoundingClientRect();
            return event.clientY < rect.top + rect.height / 2;
        });
        list.insertBefore(dragged, next || null);
    });
    container.addEventListener('drop', async event => {
        const list = event.target.closest('.stop-list');
        if (!dragged || saving || !list) return;
        event.preventDefault();
        if (dragged.parentElement === originalList && dragged.nextSibling === originalNext) {
            clear();
            return;
        }
        saving = true;
        status.textContent = 'Saving stop order…';
        // Prevent other mutations while the move is pending.
        const buttons = [...document.querySelectorAll('section button')].filter(button => !button.disabled);
        buttons.forEach(button => button.disabled = true);
        const controller = new AbortController();
        const timeout = setTimeout(() => controller.abort(), 15000);
        try {
            const response = await fetch('/Trip/MoveStop', {
                method: 'POST',
                signal: controller.signal,
                body: new URLSearchParams({
                    id: dragged.dataset.stopId,
                    dayId: list.dataset.dayId,
                    position: [...list.querySelectorAll('[data-stop-id]')].indexOf(dragged),
                    __RequestVerificationToken: document.querySelector('input[name="__RequestVerificationToken"]').value
                })
            });
            if (!response.ok || response.redirected) throw new Error('Move failed');
            status.textContent = 'Stop order saved.';
        } catch {
            restore();
            status.textContent = 'Could not confirm the move. Refresh to check the saved order before trying again.';
        } finally {
            clearTimeout(timeout);
            buttons.forEach(button => button.disabled = false);
            clear();
            saving = false;
        }
    });
    container.addEventListener('dragend', () => {
        if (saving) return;
        restore();
        clear();
    });
})();
