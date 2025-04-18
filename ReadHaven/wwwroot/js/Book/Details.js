let bookId = null;
let reviews = [];
let currentReviewIndex = 0;
let currentUserReview = null;
const ratingLabels = {
    1: "Bad",
    2: "Satisfactory",
    3: "Good",
    4: "Very Good",
    5: "Excellent"
};


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

        // If logged in, check if user already has a review
        if (userId) {
            currentUserReview = reviews.find(r => r.userId === userId);
            populateReviewForm();
        }
    });
}

function showReview(index) {
    const review = reviews[index];
    const stars = "★".repeat(review.rating) + "☆".repeat(5 - review.rating);
    const label = ratingLabels[review.rating] || "Unrated";


    $("#reviewDisplay").html(`
    <div class="p-3 border rounded shadow-sm text-center">
        <h6 class="mb-1">${review.userName}</h6>
        <div class="text-warning fs-4">${stars}</div>
        <div class="mb-2 text-muted">${label}</div>
        <p class="fst-italic mb-2">"${review.reviewText}"</p>
        <small class="text-muted">${new Date(review.date + "T00:00:00").toLocaleDateString()}</small>
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

// ----------------------
// Review Form
// ----------------------

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
        data: { id: id }, // Send the `id` as part of the request body
        success: function () {
            showToastMessage("Review deleted.", "info");
            loadReviews(bookId); // Reload reviews after deletion
        },
        error: function () {
            showToastMessage("Failed to delete review.", "error");
        }
    });
}


// ----------------------
// Init
// ----------------------

document.addEventListener("DOMContentLoaded", () => {
    bookId = window.location.pathname.split("/").pop();
    updateCartCountBadge();

    showBook(bookId);
    loadReviews(bookId);
});
