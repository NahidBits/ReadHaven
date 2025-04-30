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

// ✅ Global Toast Notification
function showToastMessage(message, type = "success") {
    const config = {
        success: { bg: "bg-success", icon: "bi-check-circle-fill", text: "text-white" },
        error: { bg: "bg-danger", icon: "bi-exclamation-triangle-fill", text: "text-white" },
        warning: { bg: "bg-warning", icon: "bi-exclamation-circle-fill", text: "text-dark" },
        info: { bg: "bg-info", icon: "bi-info-circle-fill", text: "text-dark" }
    };

    const toastType = config[type] || config.success;

    const toast = document.createElement("div");
    toast.className = `toast align-items-center ${toastType.bg} ${toastType.text} border-0 position-fixed bottom-0 end-0 m-3 shadow`;
    toast.role = "alert";
    toast.setAttribute("aria-live", "assertive");
    toast.setAttribute("aria-atomic", "true");

    toast.innerHTML = `
        <div class="d-flex">
            <div class="toast-body d-flex align-items-center gap-2">
                <i class="bi ${toastType.icon} fs-5"></i>
                <span>${message}</span>
            </div>
            <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
        </div>
    `;

    document.body.appendChild(toast);

    const bsToast = new bootstrap.Toast(toast, { delay: 3000 });
    bsToast.show();

    // Clean up after showing
    setTimeout(() => toast.remove(), 3500);
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
