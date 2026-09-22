(() => {
    document.querySelectorAll('.day-name-editor').forEach(editor => {
        const trigger = editor.querySelector('.day-name-trigger');
        const heading = editor.querySelector('h2');
        const form = editor.querySelector('form');
        const input = form.querySelector('input[name="name"]');
        function open() {
            heading.hidden = true;
            form.hidden = false;
            input.focus();
            input.select();
        }
        function cancel() {
            form.reset();
            form.hidden = true;
            heading.hidden = false;
            trigger.focus();
        }
        trigger.addEventListener('click', open);
        trigger.addEventListener('contextmenu', event => {
            event.preventDefault();
            open();
        });
        editor.querySelector('.day-name-cancel').addEventListener('click', cancel);
        form.addEventListener('keydown', event => {
            if (event.key === 'Escape') {
                event.preventDefault();
                cancel();
            }
        });
    });
})();
