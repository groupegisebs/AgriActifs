document.addEventListener('DOMContentLoaded', () => {
    const form = document.getElementById('rbacMatrixForm');
    if (!form) return;

    form.querySelectorAll('.rbac-cell-toggle').forEach(toggle => {
        toggle.addEventListener('keydown', (e) => {
            if (e.key === ' ' || e.key === 'Enter') {
                e.preventDefault();
                const input = toggle.querySelector('.rbac-cell-input');
                if (input) {
                    input.checked = !input.checked;
                }
            }
        });

        toggle.setAttribute('tabindex', '0');
        toggle.setAttribute('role', 'button');
    });
});
