//let userRole = ""; // Global User Role

// Function to check admin status FIRST
function checkRoleStatus() {
    /*return new Promise((resolve, reject) => {
        $.ajax({
            url: "/Auth/GetUserRoleStatus",
            type: "GET",
            success: function (data) {
                if (data) {
                    userRole = data;
                    resolve();
                } else {
                    reject("Could not determine admin role.");
                }
            },
            error: function (xhr, status, error) {
                reject("Error while checking admin status: " + error);
            }
        });
    });*/
}
function updateCartCountBadge() {
    $.ajax({
        url: "/BookCart/GetCartItemCount",
        method: "GET",
        success: function (count) {
            const badge = document.getElementById("cart-count-badge");

            if (badge) {
                if (count > 0) {
                    badge.style.display = "inline-block";
                    badge.textContent = count;
                } else {
                    badge.style.display = "none";
                }
            }
        },
        error: function (err) {
            console.error("Failed to fetch cart count:", err);
        }
    });
}

// ✅ Global Toast Notification Function
function showToastMessage(message, type = "success", delay = 2000) {
    const toastEl = document.getElementById("globalToast");
    const toastBody = document.getElementById("globalToastBody");

    if (!toastEl || !toastBody) {
        console.warn("Toast elements not found in DOM.");
        return;
    }

    toastBody.textContent = message;

    toastEl.className = "toast align-items-center text-white border-0";

    const bgMap = {
        success: "bg-success",
        error: "bg-danger",
        info: "bg-info",
        warning: "bg-warning"
    };

    toastEl.classList.add(bgMap[type] || "bg-success");

    const toast = new bootstrap.Toast(toastEl, { delay: delay });
    toast.show();
}