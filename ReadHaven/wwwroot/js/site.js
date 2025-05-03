// ✅ Update Cart Count Badge
function updateCartCountBadge() {
    $.ajax({
        url: "/BookCart/GetCartItemCount",
        method: "GET",
        success: function (count) {
            const badge = document.getElementById("cart-count-badge");
            if (badge) {
                badge.textContent = count;
                badge.style.display = count > 0 ? "inline-block" : "none";
            }
        },
        error: function (err) {
            console.error("Failed to fetch cart count:", err);
        }
    });
}

// ✅ Format Date to Friendly Format
function formatDate(dateString) {
    const date = new Date(dateString);
    const day = date.getDate();
    const dayName = date.toLocaleString("default", { weekday: "long" });
    const month = date.toLocaleString("default", { month: "long" });
    const year = date.getFullYear();

    const getOrdinalSuffix = (n) => {
        if (n > 3 && n < 21) return "th";
        switch (n % 10) {
            case 1: return "st";
            case 2: return "nd";
            case 3: return "rd";
            default: return "th";
        }
    };

    return `${dayName}, ${day}${getOrdinalSuffix(day)} ${month} ${year}`;
}

// ✅ Global Toast Container (only once)
(function createToastContainer() {
    if (!document.getElementById("toast-container")) {
        const container = document.createElement("div");
        container.id = "toast-container";
        container.className = "toast-container position-fixed bottom-0 end-0 p-3";
        container.style.zIndex = "1080";
        document.body.appendChild(container);
    }
})();


// ✅ Global Toast Notification
function showToastMessage(message, type = "success") {
    const config = {
        success: { bg: "bg-success bg-opacity-75", icon: "bi-check-circle", text: "text-white" },
        error: { bg: "bg-danger bg-opacity-75", icon: "bi-exclamation-triangle", text: "text-white" },
        warning: { bg: "bg-warning bg-opacity-75", icon: "bi-exclamation-circle", text: "text-dark" },
        info: { bg: "bg-info bg-opacity-75", icon: "bi-info-circle", text: "text-dark" }
    };

    const toastType = config[type] || config.success;

    const toast = document.createElement("div");
    toast.className = `toast ${toastType.bg} ${toastType.text} border-0 rounded px-3 py-2 mb-2 shadow-sm small`;
    toast.role = "alert";
    toast.setAttribute("aria-live", "assertive");
    toast.setAttribute("aria-atomic", "true");

    toast.innerHTML = `
        <div class="d-flex align-items-center gap-2">
            <i class="bi ${toastType.icon}"></i>
            <div class="flex-grow-1">${message}</div>
        </div>
    `;

    document.getElementById("toast-container").appendChild(toast);

    const bsToast = new bootstrap.Toast(toast, { delay: 1500 });
    bsToast.show();

    setTimeout(() => toast.remove(), 1800);
}


// ✅ Save scroll position before navigating away
window.addEventListener('beforeunload', function () {
    sessionStorage.setItem('scrollPos_' + location.pathname, window.scrollY);
});

// ✅ Restore scroll position safely using load event listener
window.addEventListener('load', function () {
    setTimeout(() => {
        const scrollKey = 'scrollPos_' + location.pathname;
        const scrollPos = sessionStorage.getItem(scrollKey);
        if (scrollPos !== null) {
            window.scrollTo(0, parseInt(scrollPos));
        }
    }, 100); // Give some delay for other content to load
});
