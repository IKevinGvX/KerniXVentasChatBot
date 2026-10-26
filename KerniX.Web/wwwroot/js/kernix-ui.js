document.addEventListener("DOMContentLoaded", () => {

    /* =========================================================
       KERNIX UI V2
       Navbar + Reveal + Hero 3D + IA Tilt + Magnetic Buttons
    ========================================================= */

    const reduceMotion = window.matchMedia(
        "(prefers-reduced-motion: reduce)"
    ).matches;

    const desktopPointer = window.matchMedia(
        "(min-width: 1200px) and (hover: hover) and (pointer: fine)"
    ).matches;


    /* =========================================================
       1. NAVBAR DROPDOWNS
    ========================================================= */

    const dropdowns =
        document.querySelectorAll(".kx-nav-dropdown");

    const closeAllDropdowns = (except = null) => {

        dropdowns.forEach(dropdown => {

            if (dropdown !== except) {
                dropdown.classList.remove("kx-open");
            }

        });

    };


    dropdowns.forEach(dropdown => {

        const trigger =
            dropdown.querySelector(".kx-nav-trigger");

        if (!trigger) return;


        trigger.setAttribute("aria-expanded", "false");


        trigger.addEventListener("click", event => {

            event.preventDefault();
            event.stopPropagation();

            const isOpen =
                dropdown.classList.contains("kx-open");

            closeAllDropdowns(dropdown);

            dropdown.classList.toggle(
                "kx-open",
                !isOpen
            );

            trigger.setAttribute(
                "aria-expanded",
                String(!isOpen)
            );

        });


        dropdown.addEventListener("click", event => {
            event.stopPropagation();
        });

    });


    document.addEventListener("click", () => {
        closeAllDropdowns();
    });


    document.addEventListener("keydown", event => {

        if (event.key === "Escape") {

            closeAllDropdowns();

            dropdowns.forEach(dropdown => {

                const trigger =
                    dropdown.querySelector(".kx-nav-trigger");

                trigger?.setAttribute(
                    "aria-expanded",
                    "false"
                );

            });

        }

    });


    /* =========================================================
       2. SCROLL REVEAL
    ========================================================= */

    const revealElements = [
        ...document.querySelectorAll(".kx-section"),
        ...document.querySelectorAll(".kx-media-frame"),
        ...document.querySelectorAll(".kx-content-title"),
        ...document.querySelectorAll(".kx-content-text"),
        ...document.querySelectorAll(".kx-feature-list"),
        ...document.querySelectorAll(".kx-sales-grid"),
        ...document.querySelectorAll(".kx-promo")
    ];


    revealElements.forEach((element, index) => {

        element.classList.add("kx-reveal");

        /*
         * Alternamos algunas entradas para dar
         * más sensación de profundidad.
         */

        if (
            element.classList.contains("kx-media-frame")
        ) {

            element.classList.add(
                index % 2 === 0
                    ? "kx-reveal-left"
                    : "kx-reveal-right"
            );

        }

    });


    if (reduceMotion) {

        revealElements.forEach(element => {
            element.classList.add("kx-visible");
        });

    } else {

        const revealObserver =
            new IntersectionObserver(
                entries => {

                    entries.forEach(entry => {

                        if (!entry.isIntersecting) {
                            return;
                        }

                        entry.target.classList.add(
                            "kx-visible"
                        );

                        revealObserver.unobserve(
                            entry.target
                        );

                    });

                },
                {
                    threshold: 0.10,
                    rootMargin:
                        "0px 0px -45px 0px"
                }
            );


        revealElements.forEach(element => {
            revealObserver.observe(element);
        });

    }


    /* =========================================================
       3. HERO
    ========================================================= */

    const hero =
        document.querySelector(".kx-hero");

    if (
        hero &&
        desktopPointer &&
        !reduceMotion
    ) {

        const character =
            hero.querySelector(".kx-hero-character");

        const empress =
            hero.querySelector(".kx-empress");

        const robot =
            hero.querySelector(".kx-robot");

        const salesWidget =
            hero.querySelector(".kx-sales-widget");


        let targetX = 0;
        let targetY = 0;

        let currentX = 0;
        let currentY = 0;

        let frameRequested = false;


        const renderHero = () => {

            /*
             * Interpolación suave.
             * Evita movimiento brusco.
             */

            currentX +=
                (targetX - currentX) * 0.08;

            currentY +=
                (targetY - currentY) * 0.08;


            /* EMPERATRIZ */

            if (empress) {

                empress.style.transform =
                    `translate3d(
                        ${currentX * -7}px,
                        ${currentY * -3}px,
                        22px
                    )`;

            }


            /* ROBOT */

            if (robot) {

                /*
                 * No usamos transform aquí porque
                 * el CSS tiene animación de flotación.
                 *
                 * Translate permite combinar ambos.
                 */

                robot.style.translate =
                    `${currentX * 12}px ${currentY * 6}px`;

            }


            /* WIDGET DE VENTAS */

            if (salesWidget) {

                salesWidget.style.transform =
                    `translate3d(
                        ${currentX * 8}px,
                        ${currentY * 4}px,
                        35px
                    )
                    rotateY(${currentX * 2.5}deg)
                    rotateX(${currentY * -2}deg)`;

            }


            /* PERSONAJE COMPLETO */

            if (character) {

                character.style.transform =
                    `rotateY(${currentX * 1.2}deg)
                     rotateX(${currentY * -0.7}deg)`;

            }


            frameRequested = false;


            /*
             * Si todavía no llegamos al objetivo,
             * seguimos interpolando.
             */

            if (
                Math.abs(targetX - currentX) > 0.001 ||
                Math.abs(targetY - currentY) > 0.001
            ) {

                requestHeroFrame();

            }

        };


        const requestHeroFrame = () => {

            if (frameRequested) return;

            frameRequested = true;

            requestAnimationFrame(renderHero);

        };


        hero.addEventListener(
            "pointermove",
            event => {

                const rect =
                    hero.getBoundingClientRect();

                const x =
                    (event.clientX - rect.left)
                    / rect.width;

                const y =
                    (event.clientY - rect.top)
                    / rect.height;


                /*
                 * Valores normalizados:
                 * -1 hasta 1
                 */

                targetX =
                    (x - 0.5) * 2;

                targetY =
                    (y - 0.5) * 2;


                /* GLOW */

                hero.style.setProperty(
                    "--kx-mouse-x",
                    `${x * 100}%`
                );

                hero.style.setProperty(
                    "--kx-mouse-y",
                    `${y * 100}%`
                );


                requestHeroFrame();

            },
            { passive: true }
        );


        hero.addEventListener(
            "pointerleave",
            () => {

                targetX = 0;
                targetY = 0;

                hero.style.setProperty(
                    "--kx-mouse-x",
                    "50%"
                );

                hero.style.setProperty(
                    "--kx-mouse-y",
                    "50%"
                );

                requestHeroFrame();

            }
        );

    }


    /* =========================================================
       4. PANEL KERNIX IA - TILT 3D
    ========================================================= */

    const aiCard =
        document.querySelector(".kx-ai-card");


    if (
        aiCard &&
        desktopPointer &&
        !reduceMotion
    ) {

        let aiFrame = null;


        aiCard.addEventListener(
            "pointermove",
            event => {

                const rect =
                    aiCard.getBoundingClientRect();

                const x =
                    event.clientX - rect.left;

                const y =
                    event.clientY - rect.top;


                const px =
                    x / rect.width;

                const py =
                    y / rect.height;


                /*
                 * Rotación limitada.
                 * No queremos una tarjeta exagerada.
                 */

                const rotateY =
                    (px - 0.5) * 5;

                const rotateX =
                    (0.5 - py) * 4;


                aiCard.style.setProperty(
                    "--kx-ai-light-x",
                    `${px * 100}%`
                );

                aiCard.style.setProperty(
                    "--kx-ai-light-y",
                    `${py * 100}%`
                );


                if (aiFrame) {
                    cancelAnimationFrame(aiFrame);
                }


                aiFrame =
                    requestAnimationFrame(() => {

                        aiCard.style.setProperty(
                            "--kx-card-rx",
                            `${rotateX}deg`
                        );

                        aiCard.style.setProperty(
                            "--kx-card-ry",
                            `${rotateY}deg`
                        );

                    });

            },
            { passive: true }
        );


        aiCard.addEventListener(
            "pointerleave",
            () => {

                aiCard.style.setProperty(
                    "--kx-card-rx",
                    "0deg"
                );

                aiCard.style.setProperty(
                    "--kx-card-ry",
                    "0deg"
                );

                aiCard.style.setProperty(
                    "--kx-ai-light-x",
                    "50%"
                );

                aiCard.style.setProperty(
                    "--kx-ai-light-y",
                    "20%"
                );

            }
        );

    }


    /* =========================================================
       5. MAGNETIC BUTTONS
    ========================================================= */

    const magneticButtons =
        document.querySelectorAll(
            ".kx-button-ai, " +
            ".kx-button-whatsapp, " +
            ".kx-seller-premium"
        );


    if (
        desktopPointer &&
        !reduceMotion
    ) {

        magneticButtons.forEach(button => {

            button.addEventListener(
                "pointermove",
                event => {

                    const rect =
                        button.getBoundingClientRect();

                    const x =
                        event.clientX -
                        rect.left -
                        rect.width / 2;

                    const y =
                        event.clientY -
                        rect.top -
                        rect.height / 2;


                    const moveX =
                        x * 0.08;

                    const moveY =
                        y * 0.10;


                    button.style.translate =
                        `${moveX}px ${moveY}px`;

                },
                { passive: true }
            );


            button.addEventListener(
                "pointerleave",
                () => {

                    button.style.translate =
                        "0 0";

                }
            );

        });

    }


    /* =========================================================
       6. CARDS 3D DE SECCIONES
    ========================================================= */

    const tiltCards =
        document.querySelectorAll(
            ".kx-mini-card, " +
            ".kx-feature-list > div"
        );


    if (
        desktopPointer &&
        !reduceMotion
    ) {

        tiltCards.forEach(card => {

            card.addEventListener(
                "pointermove",
                event => {

                    const rect =
                        card.getBoundingClientRect();

                    const x =
                        (
                            event.clientX -
                            rect.left
                        ) / rect.width;

                    const y =
                        (
                            event.clientY -
                            rect.top
                        ) / rect.height;


                    const rotateY =
                        (x - 0.5) * 5;

                    const rotateX =
                        (0.5 - y) * 4;


                    card.style.transform =
                        `perspective(700px)
                         translateY(-5px)
                         rotateX(${rotateX}deg)
                         rotateY(${rotateY}deg)`;

                },
                { passive: true }
            );


            card.addEventListener(
                "pointerleave",
                () => {

                    card.style.transform = "";

                }
            );

        });

    }


    /* =========================================================
       7. NAVBAR SCROLL STATE
    ========================================================= */

    const header =
        document.querySelector(".kx-premium-header");


    if (header) {

        let ticking = false;


        const updateNavbar = () => {

            header.classList.toggle(
                "kx-header-scrolled",
                window.scrollY > 30
            );

            ticking = false;

        };


        window.addEventListener(
            "scroll",
            () => {

                if (ticking) return;

                ticking = true;

                requestAnimationFrame(
                    updateNavbar
                );

            },
            { passive: true }
        );


        updateNavbar();

    }

});