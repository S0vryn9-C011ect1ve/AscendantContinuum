function setupNavActiveSection() {
    const navLinks = Array.from(document.querySelectorAll('.top-nav-link[href^="#"]'));
    if (navLinks.length === 0) return;

    const sectionMap = navLinks
        .map(link => {
            const targetId = link.getAttribute('href');
            const section = targetId ? document.querySelector(targetId) : null;
            return section ? { link, section } : null;
        })
        .filter(Boolean);

    if (sectionMap.length === 0) return;

    const setActiveLink = (activeLink) => {
        navLinks.forEach(link => link.classList.remove('active'));
        if (activeLink) {
            activeLink.classList.add('active');
        }
    };

    navLinks.forEach(link => {
        link.addEventListener('click', () => setActiveLink(link));
    });

    const sectionObserver = new IntersectionObserver((entries) => {
        const visible = entries
            .filter(entry => entry.isIntersecting)
            .sort((a, b) => b.intersectionRatio - a.intersectionRatio);

        if (visible.length === 0) return;

        const activeSection = visible[0].target;
        const activeMatch = sectionMap.find(item => item.section === activeSection);
        if (activeMatch) {
            setActiveLink(activeMatch.link);
        }
    }, {
        root: null,
        rootMargin: '-35% 0px -50% 0px',
        threshold: [0.2, 0.4, 0.6]
    });

    sectionMap.forEach(item => sectionObserver.observe(item.section));

    setActiveLink(sectionMap[0].link);
}

window.addEventListener('load', setupNavActiveSection);
