let reviews = [];
let currentReviewIndex = 0;
function populateBookForm(book) {
    
    document.getElementById("Title").value = book.title;
    document.getElementById("Genre").value = book.genre;
    document.getElementById("Price").value = book.price;
    document.getElementById("bookIdField").value = book.id;
}

function populateBookDetails(book) {

    document.getElementById("bookImage").src = book.imagePath || "uploads/book/Default_image.webp";  
    document.getElementById("ReadTitle").innerText = book.title || "N/A";
    document.getElementById("ReadGenre").innerText = book.genre || "N/A";
    document.getElementById("ReadPrice").innerText = book.price ? `$${book.price.toFixed(2)}` : "N/A";
}


function showBook(bookId) {
    $.ajax({
        url: "/GetBookById",  
        type: "GET",
        data: { id: bookId },  
        success: function (data) {
            if (data) {

                if(userRole === "Admin") {
                    populateBookForm(data);  
                }
                populateBookDetails(data);    
            } else {
                showToastMessage("Error loading book details.", "error");
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            console.error("Error loading book details:", textStatus, errorThrown);
            showToastMessage("Something went wrong while fetching book details.", "error");
        }
    });
}

function saveBookUpdate() {
    const form = document.getElementById("bookUpdateForm");
    const formData = new FormData(form);

    $.ajax({
        url: "/BookUpdate",
        type: "POST",
        data: formData,
        contentType: false,   // Important: let jQuery set this
        processData: false,   // Important: prevent automatic transformation
        success: function (response) {
            showToastMessage("Book updated successfully!", "success");
            showBook(formData.get("Id")); 
        },
        error: function (xhr, status, error) {
            console.error("Error updating book:", status, error);
            showToastMessage("Failed to update book.", "error");
        }
    });
}
function loadReviews(bookId) {
    $.ajax({
        url: `/BookReview/GetReviewsByBook`,
        method: "GET",
        data: { id: bookId },   
        success: function (data) {
            reviews = data;
            if (reviews.length > 0) {
                currentReviewIndex = 0;
                showReview(currentReviewIndex);
            } else {
                document.getElementById("reviewDisplay").innerHTML = "<p class='text-muted text-center'>No reviews yet.</p>";
            }
        },
        error: function () {
            document.getElementById("reviewDisplay").innerHTML = "<p class='text-danger text-center'>Failed to load reviews.</p>";
        }
    });
}

function showReview(index) {
    const review = reviews[index];
    const display = document.getElementById("reviewDisplay");
    const stars = "★".repeat(review.rating) + "☆".repeat(5 - review.rating);

    display.innerHTML = `
        <div class="p-3 border rounded shadow-sm text-center">
            <h6 class="mb-1">${review.userName}</h6>
            <div class="text-warning mb-2">${stars}</div>
            <p class="fst-italic mb-2">"${review.reviewText}"</p>
            <small class="text-muted">${new Date(review.date + "T00:00:00").toLocaleDateString()}</small>
        </div>
    `;
}

function previousReview() {
    if (reviews.length === 0) return;
    currentReviewIndex = (currentReviewIndex - 1 + reviews.length) % reviews.length;
    showReview(currentReviewIndex);
}

function nextReview() {
    if (reviews.length === 0) return;
    currentReviewIndex = (currentReviewIndex + 1) % reviews.length;
    showReview(currentReviewIndex);
}


document.addEventListener("DOMContentLoaded", async () => {
    bookId = window.location.pathname.split("/").pop();
    updateCartCountBadge();

    checkRoleStatus()
        .then(() => {
            showBook(bookId);  
            loadReviews(bookId);
        })
        .catch((error) => {
            console.error(error);
        });
    
});
