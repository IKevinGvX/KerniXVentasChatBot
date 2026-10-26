/* ============================================================
   KERNIX - SOLUCIONES SCROLL ANIMATION
============================================================ */

(() => {

    const section = document.querySelector(".kx-solutions-3d");

    if (!section) return;


    const showSection = () => {
        section.classList.add("kx-solutions-visible");
    };


    if (!("IntersectionObserver" in window)) {
        showSection();
        return;
    }


    const observer = new IntersectionObserver(
        (entries, observerInstance) => {

            entries.forEach(entry => {

                if (!entry.isIntersecting) return;

                showSection();

                observerInstance.unobserve(entry.target);

            });

        },
        {
            threshold: 0.18
        }
    );


    observer.observe(section);

})();