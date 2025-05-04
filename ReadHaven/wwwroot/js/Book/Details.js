let bookId = null;
let reviews = [];
let currentReviewIndex = 0;
let currentUserReview = null;
let isPurchased = false;
const reviewForm = document.getElementById("reviewForm");
const ratingLabels = {
    1: "Bad",
    2: "Satisfactory",
    3: "Good",
    4: "Very Good",
    5: "Excellent"
};
function isPurchasedBook() {
    $.ajax({
        url: `/BookCart/IsPurchasedBook`,
        type: "GET",
        data: { bookId: bookId },
        success: function (data) {
            isPurchased = data;
        },
        error: function () {
            showToastMessage("Failed to load user role.", "error");
        }
    });
}
// Book Section
function populateBookForm(book) {
    $("#Title").val(book.title);
    $("#Genre").val(book.genre);
    $("#Price").val(book.price);
    $("#bookIdField").val(book.id);
}

function populateBookDetails(book) {
    $("#bookImage").attr("src", book.imagePath || "/uploads/book/Default_image.webp");
    $("#ReadTitle").text(book.title || "N/A");
    $("#ReadGenre").text(book.genre || "N/A");
    $("#ReadPrice").text(book.price ? `$${book.price.toFixed(2)}` : "N/A");

    const container = document.querySelector("#bookActionButtons");
    if (!container) return;

    const isLoved = book.isLoved === true;
    const wishlistIconClass = isLoved ? "bi-heart-fill" : "bi-heart";
    const wishlistBtnClass = isLoved ? "isLoved" : "";
    const wishlistTitle = isLoved ? "Remove from Wishlist" : "Add to Wishlist";

    const deleteBtn = (userRole === "Admin") ? `
        <button class="btn btn-outline-danger btn-sm" onclick="deleteBook('${book.id}')" title="Delete Book">
            <i class="bi bi-trash"></i>
        </button>` : "";

    const cartBtn = `
        <button class="btn btn-outline-primary btn-sm" onclick="addToCart('${book.id}')" title="Add to Cart">
            <i class="bi bi-cart-plus"></i>
        </button>`;

    const wishlistBtn = isAuthenticated ? `
        <button class="btn btn-outline-danger btn-sm wishlist-btn ${wishlistBtnClass}" 
                onclick="toggleWishlist('${book.id}', this)" 
                title="${wishlistTitle}" id="wishlistBtn">
            <i class="bi ${wishlistIconClass}" id="wishlistIcon"></i>
        </button>` : "";

    container.innerHTML = `
        <div class="d-flex justify-content-start gap-2 book-actions">
            ${deleteBtn}
            ${cartBtn}
            ${wishlistBtn}
        </div>`;
}

// Delete a book
function deleteBook(bookId) {
    if (confirm('Are you sure you want to delete this book?')) {
        $.ajax({
            url: '/Delete/' + bookId,
            type: 'POST',
            success: function () {
                showToastMessage("Book deleted successfully!");
                window.location.href = "/Book/Index";
            },
            error: function (xhr, status, error) {
                showToastMessage("Error deleting book: " + error, "error");
            }
        });
    }
}

// Add book to cart
function addToCart(bookId) {
    $.ajax({
        url: "/BookCart/AddToCart",
        type: "POST",
        data: { bookId },
        success: function () {
            showToastMessage("Book added to cart!");
            updateCartCountBadge();
        },
        error: function (xhr, status, error) {
            console.error("Failed to add to cart:", error);
            showToastMessage("Failed to add book to cart.", "error");
        }
    });
}

function toggleWishlist(bookId, btn) {
    $.ajax({
        url: `/Wishlist`,
        type: "POST",
        data: { bookId: bookId },
        success: function () {
            const icon = btn.querySelector("i");
            btn.classList.toggle("isLoved");

            const isWishlisted = btn.classList.contains("isLoved");
            icon.className = `bi ${isWishlisted ? "bi-heart-fill" : "bi-heart"}`;
            btn.setAttribute("title", isWishlisted ? "Remove from Wishlist" : "Add to Wishlist");
        },
        error: function (xhr) {
            console.error("Error updating wishlist:", xhr.responseText);
            showToastMessage("Could not update wishlist", "error");
        }
    });
}
function showBook(bookId) {
    $.get("/GetBookById", { id: bookId }, function (data) {
        if (data) {
            if (userRole === "Admin") populateBookForm(data);
            populateBookDetails(data);
            $("#reviewBookId").val(bookId);
        } else {
            showToastMessage("Error loading book details.", "error");
        }
    });
}
function showRating(bookId) {
    $.get("/BookReview/GetRatingByBook", { bookId: bookId }, function (data) {
        if (data && data.length > 0) {
            const ratingBarsContainer = $("#ratingBars");
            ratingBarsContainer.empty();

            let totalRating = 0;
            let totalReviews = 0;

            data.forEach(item => {
                const stars = "★".repeat(item.rating); 
                const emptyStars = "☆".repeat(5 - item.rating); 

                const ratingLine = `<div class="rating-bar d-flex justify-content-between">
                    <span class="text-warning">${stars}${emptyStars}</span>
                    <span class="ms-2 text-muted small">${item.count}</span>
                </div>`;
                ratingBarsContainer.append(ratingLine);
                totalRating += item.rating * item.count;
                totalReviews += item.count;
            });

            const averageRating = totalReviews > 0 ? (totalRating / totalReviews).toFixed(2) : 0;

            const averageRatingDisplay = `
                <div class="d-flex justify-content-between mt-3">
                    <span class="text-warning">${"★".repeat(Math.round(averageRating))}${"☆".repeat(5 - Math.round(averageRating))}</span>
                    <span class="ms-2 text-muted small">${averageRating} / 5</span>
                </div>
            `;
            ratingBarsContainer.append(averageRatingDisplay);

        } else {
            $("#ratingBars").html("<div class='text-muted small'>No ratings yet.</div>");
        }
    }).fail(function () {
        showToastMessage("Error loading book rating.", "error");
    });
}

function saveBookUpdate() {
    const formData = new FormData(document.getElementById("bookUpdateForm"));

    $.ajax({
        url: "/BookUpdate",
        type: "POST",
        data: formData,
        contentType: false,
        processData: false,
        success: function () {
            showToastMessage("Book updated successfully!", "success");
            showBook(formData.get("Id"));
        },
        error: function () {
            showToastMessage("Failed to update book.", "error");
        }
    });
}

// Reviews Section
function loadReviews(bookId) {
    $.get("/BookReview/GetReviewsByBook", { id: bookId }, function (data) {
        reviews = data;
        currentReviewIndex = 0;

        if (reviews.length > 0) {
            showReview(currentReviewIndex);
        } else {
            $("#reviewDisplay").html("<p class='text-muted text-center'>No reviews yet.</p>");
        }

        if (userId) {
            currentUserReview = reviews.find(r => r.userId === userId);
          //  populateReviewForm();
        }
    });
}

function showReview(index) {
    const review = reviews[index];
    const stars = "★".repeat(review.rating) + "☆".repeat(5 - review.rating);
    const label = ratingLabels[review.rating] || "Unrated";

    $("#reviewDisplay").html(`
        <div class="review-item">
            <div class="review-header">
                <span class="review-name">${review.userName}</span>
                <span class="rating">${stars}</span>
            </div>
            <div class="review-text">
                <p>"${review.reviewText}"</p>
            </div>
            <div class="review-date text-muted">
                <small>Posted on: ${new Date(review.date + "T00:00:00").toLocaleDateString()}</small>
            </div>
        </div>
    `);
}

function previousReview() {
    if (!reviews.length) return;
    currentReviewIndex = (currentReviewIndex - 1 + reviews.length) % reviews.length;
    showReview(currentReviewIndex);
}

function nextReview() {
    if (!reviews.length) return;
    currentReviewIndex = (currentReviewIndex + 1) % reviews.length;
    showReview(currentReviewIndex);
}

// Review Form
function populateReviewForm() {
    const review = currentUserReview;

    if (review) {
        $("#ratingSelect").val(review.rating);
        $("#reviewTextArea").val(review.reviewText);
        $("#reviewSubmitBtn").text("Update Review");
        $("#deleteReviewBtn").removeClass("d-none").attr("data-review-id", review.id);
    } else {
        $("#ratingSelect").val("");
        $("#reviewTextArea").val("");
        $("#reviewSubmitBtn").text("Submit Review");
        $("#deleteReviewBtn").addClass("d-none").removeAttr("data-review-id");
    }

    $("#reviewBookId").val(bookId);
}

function submitReview(e) {
    e.preventDefault();

    const formData = {
        Rating: $("#ratingSelect").val(),
        ReviewText: $("#reviewTextArea").val(),
        BookId: bookId,
        UserId: userId
    };

    $.post("/BookReview/Save", formData, function () {
        showToastMessage("Review saved successfully!", "success");
        loadReviews(bookId);
        showRating(bookId);
        populateReviewForm();
    }).fail(() => {
        showToastMessage("Failed to save review.", "error");
    });
}

function deleteReview() {
    const id = $("#deleteReviewBtn").attr("data-review-id");
    if (!id || !confirm("Are you sure you want to delete your review?")) return;

    $.ajax({
        url: `/BookReview/Delete`,
        type: "DELETE",
        data: { id: id },
        success: function () {
            showToastMessage("Review deleted.", "info");
            loadReviews(bookId);
            showRating(bookId);
            populateReviewForm();
        },
        error: function () {
            showToastMessage("Failed to delete review.", "error");
        }
    });
}

function writeReview() {
    const userReview = document.getElementById("writeReviewButton");

    if (!userReview) return;

    if (userRole === "Guest") {
        showToastMessage("Login to review", "info");
    } else if (isPurchased) {
        userReview.innerHTML = '';
        reviewForm.style.display = 'block';
        userReview.style.display = 'none';
        populateReviewForm(); 
    } else {
        showToastMessage("Purchase book to review", "warning");
    }
}

// Init
document.addEventListener("DOMContentLoaded", () => {
    bookId = window.location.pathname.split("/").pop();
    reviewForm.style.display = 'none'; 

    updateCartCountBadge();
    showBook(bookId);
    showRating(bookId);
    loadReviews(bookId);
    isPurchasedBook();
    localStorage.setItem("returnUrl", window.location.href);
});