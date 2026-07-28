document.addEventListener('DOMContentLoaded', () => {
    const toggle = document.getElementById('sidebarToggle');
    const sidebar = document.getElementById('appSidebar');

    if (toggle && sidebar) {
        toggle.addEventListener('click', () => {
            sidebar.classList.toggle('mobile-open');
        });

        document.addEventListener('click', (e) => {
            if (!sidebar.contains(e.target) && !toggle.contains(e.target)) {
                sidebar.classList.remove('mobile-open');
            }
        });
    }

    initSidebarGroups(sidebar);
});

function initSidebarGroups(sidebar) {
    if (!sidebar) return;

    const STORAGE_KEY = 'agriactifs-sidebar-collapsed';
    const getCollapsed = () => {
        try { return JSON.parse(localStorage.getItem(STORAGE_KEY) || '[]'); } catch { return []; }
    };
    const setCollapsed = (arr) => {
        try { localStorage.setItem(STORAGE_KEY, JSON.stringify(arr)); } catch { /* ignore */ }
    };

    const collapsed = getCollapsed();
    const groups = sidebar.querySelectorAll('.app-sidebar-group[data-group]');

    groups.forEach((group) => {
        const id = group.dataset.group;
        const btn = group.querySelector('.app-sidebar-group-toggle');
        const body = group.querySelector('.app-sidebar-group-body');
        if (!btn || !body) return;

        const hasActive = group.querySelector('.app-sidebar-link.active') !== null;
        const isCollapsed = !hasActive && collapsed.includes(id);

        const setOpen = (open) => {
            group.classList.toggle('open', open);
            btn.setAttribute('aria-expanded', open ? 'true' : 'false');
            body.style.maxHeight = open ? `${body.scrollHeight}px` : '0';
        };

        setOpen(!isCollapsed);

        btn.addEventListener('click', () => {
            const open = !group.classList.contains('open');
            setOpen(open);

            const arr = getCollapsed();
            if (open) {
                const idx = arr.indexOf(id);
                if (idx > -1) arr.splice(idx, 1);
            } else if (!arr.includes(id)) {
                arr.push(id);
            }
            setCollapsed(arr);
        });
    });
}
