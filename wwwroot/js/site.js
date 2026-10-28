(() => {
    const root = document.documentElement;
    const toggle = document.getElementById("theme-toggle");
    const icon = document.getElementById("theme-icon");
    const label = document.getElementById("theme-label");

    if (!toggle || !icon || !label) {
        return;
    }

    const updateThemeControl = () => {
        const isDark = root.dataset.theme !== "light";
        toggle.setAttribute("aria-pressed", String(!isDark));
        label.textContent = isDark ? "Switch to light theme" : "Switch to dark theme";
        icon.textContent = isDark ? "☼" : "☾";
    };

    toggle.addEventListener("click", () => {
        const nextTheme = root.dataset.theme === "light" ? "dark" : "light";
        root.dataset.theme = nextTheme;
        localStorage.setItem("algoflow-theme", nextTheme);
        updateThemeControl();
    });

    updateThemeControl();
})();

// Write your JavaScript code.
