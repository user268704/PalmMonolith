function handleCredentialResponse(response) {console.log("Encoded JWT ID token: " + response.credential);
}

window.onload = function () {
    google.accounts.id.initialize({
        client_id: "684061569948-qkge5t2rfm82qddncadkc6scl7qfp4q8.apps.googleusercontent.com",
        callback: handleCredentialResponse
    });
    google.accounts.id.renderButton(
        document.getElementById("buttonDiv"),
        { theme: "outline", size: "large" }  // customization attributes
    );
    google.accounts.id.prompt(); // also display the One Tap dialog
}
